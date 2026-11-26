using System;
using System.Collections.Generic;
using Deadzone.Core;
using Deadzone.Data;
using Deadzone.Economy;
using Deadzone.SaveSystem;

namespace Deadzone.Buildings
{
    public enum UpgradeBlockReason
    {
        None = 0, UnknownBuilding, NotAvailable, NotBuilt, MaxLevel, AlreadyUpgrading, QueueFull, RequirementNotMet, InsufficientResources,
        /// <summary>A cost line is larger than the wallet can ever hold: it could never be paid. Fix: upgrade the Warehouse.</summary>
        StorageTooSmall
    }

    /// <summary>Result of CheckUpgrade: why it is blocked, and exactly what is missing.</summary>
    public sealed class UpgradeCheck
    {
        public UpgradeBlockReason reason;
        public BuildingLevelData target;
        public readonly List<BuildingRequirement> unmetRequirements = new List<BuildingRequirement>();
        public readonly List<ResourceAmount> missingResources = new List<ResourceAmount>();
        /// <summary>Cost lines that exceed the current wallet capacity of their resource.</summary>
        public readonly List<ResourceAmount> storageShort = new List<ResourceAmount>();
        public bool CanUpgrade => reason == UpgradeBlockReason.None;
    }

    /// <summary>
    /// Construction queue and upgrades. Starting a job is one all-or-nothing wallet payment plus a saved job record;
    /// completing a job is idempotent (the level is SET to the target, the job is removed first), so a repeated
    /// call, a reload or an offline catch-up can never grant a level or resources twice.
    /// </summary>
    public sealed class BuildingUpgradeService
    {
        private readonly PlayerData data;
        private readonly BuildingManager buildings;
        private readonly ResourceWallet wallet;
        private readonly GameConfig config;
        private readonly Action saveRequested;

        /// <summary>VIP hook: extra queue slots. Returns 0 until the VIP system exists.</summary>
        public Func<int> ExtraQueueSlots { get; set; } = () => 0;

        public BuildingUpgradeService(PlayerData data, BuildingManager buildings, ResourceWallet wallet, GameConfig config, Action saveRequested = null)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            this.buildings = buildings ?? throw new ArgumentNullException(nameof(buildings));
            this.wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.saveRequested = saveRequested;
        }

        public IReadOnlyList<ConstructionJobData> Queue => data.constructionQueue;
        public int QueueSlots => Math.Max(1, config.constructionQueueSlots + Math.Max(0, ExtraQueueSlots()));

        public ConstructionJobData GetJob(string buildingId)
        {
            foreach (ConstructionJobData job in data.constructionQueue)
            {
                if (job.buildingId == buildingId) return job;
            }
            return null;
        }

        public long RemainingSeconds(ConstructionJobData job) => job == null ? 0 : Math.Max(0, job.endUtc - buildings.Now());

        public float Progress01(ConstructionJobData job)
        {
            if (job == null) return 0f;
            long total = job.endUtc - job.startUtc;
            if (total <= 0) return 1f;
            double done = buildings.Now() - job.startUtc;
            return (float)Math.Max(0.0, Math.Min(1.0, done / total));
        }

        // ------------------------------------------------------------------ rules

        public UpgradeCheck CheckUpgrade(string buildingId)
        {
            var check = new UpgradeCheck();
            BuildingDefinition definition = buildings.GetDefinition(buildingId);
            BuildingInstanceData instance = buildings.GetInstance(buildingId);

            if (definition == null) { check.reason = UpgradeBlockReason.UnknownBuilding; return check; }
            if (!definition.IsFunctional) { check.reason = UpgradeBlockReason.NotAvailable; return check; }
            if (instance == null || instance.level < 1) { check.reason = UpgradeBlockReason.NotBuilt; return check; }
            if (instance.level >= definition.maxLevel) { check.reason = UpgradeBlockReason.MaxLevel; return check; }

            check.target = definition.GetLevel(instance.level + 1);
            if (check.target == null) { check.reason = UpgradeBlockReason.MaxLevel; return check; }

            // Reported in priority order, but every unmet requirement / missing resource is still listed for the UI.
            foreach (BuildingRequirement requirement in check.target.requirements)
            {
                if (!requirement.IsMet(buildings.GetLevel)) check.unmetRequirements.Add(requirement);
            }
            foreach (ResourceAmount cost in check.target.upgradeCost)
            {
                long missing = cost.amount - wallet.Get(cost.type);
                if (missing > 0) check.missingResources.Add(new ResourceAmount { type = cost.type, amount = missing });
                if (cost.amount > wallet.GetCapacity(cost.type)) check.storageShort.Add(new ResourceAmount { type = cost.type, amount = cost.amount });
            }

            if (GetJob(buildingId) != null) check.reason = UpgradeBlockReason.AlreadyUpgrading;
            else if (data.constructionQueue.Count >= QueueSlots) check.reason = UpgradeBlockReason.QueueFull;
            else if (check.unmetRequirements.Count > 0) check.reason = UpgradeBlockReason.RequirementNotMet;
            else if (check.storageShort.Count > 0) check.reason = UpgradeBlockReason.StorageTooSmall;
            else if (check.missingResources.Count > 0) check.reason = UpgradeBlockReason.InsufficientResources;
            return check;
        }

        // ------------------------------------------------------------------ start / cancel

        public UpgradeBlockReason TryStartUpgrade(string buildingId)
        {
            UpgradeCheck check = CheckUpgrade(buildingId);
            if (!check.CanUpgrade) return check.reason;

            BuildingInstanceData instance = buildings.GetInstance(buildingId);
            var paid = new List<ResourceAmount>();
            foreach (ResourceAmount cost in check.target.upgradeCost) paid.Add(new ResourceAmount { type = cost.type, amount = cost.amount });

            if (!wallet.TrySpendAll(paid)) return UpgradeBlockReason.InsufficientResources;

            long now = buildings.Now();
            buildings.Accrue(instance, now);
            data.constructionQueue.Add(new ConstructionJobData
            {
                jobId = Guid.NewGuid().ToString("N"),
                buildingId = buildingId,
                fromLevel = instance.level,
                targetLevel = instance.level + 1,
                startUtc = now,
                endUtc = now + Math.Max(1, check.target.upgradeDurationSeconds),
                paid = paid
            });

            GameEvents.RaiseConstructionChanged();
            GameEvents.RaiseBuildingChanged(buildingId);
            saveRequested?.Invoke();
            return UpgradeBlockReason.None;
        }

        /// <summary>Cancels a running job and refunds config.cancelRefundPercent of what was paid (capped by wallet capacity).</summary>
        public bool Cancel(string buildingId)
        {
            ConstructionJobData job = GetJob(buildingId);
            if (job == null) return false;

            data.constructionQueue.Remove(job); // removed first: a second Cancel finds nothing and refunds nothing
            foreach (ResourceAmount paid in job.paid)
            {
                long refund = paid.amount * Math.Max(0, Math.Min(100, config.cancelRefundPercent)) / 100;
                if (refund > 0) wallet.Add(paid.type, refund);
            }

            GameEvents.RaiseConstructionChanged();
            GameEvents.RaiseBuildingChanged(buildingId);
            saveRequested?.Invoke();
            return true;
        }

        // ------------------------------------------------------------------ speed up / finish

        /// <summary>Shortens a running job (speedup items will call this; no item is consumed here yet). Completes it when time runs out.</summary>
        public bool ApplySpeedup(string buildingId, long seconds)
        {
            ConstructionJobData job = GetJob(buildingId);
            if (job == null || seconds <= 0) return false;

            job.endUtc = Math.Max(buildings.Now(), job.endUtc - seconds);
            if (job.endUtc <= buildings.Now()) Complete(job);
            GameEvents.RaiseConstructionChanged();
            saveRequested?.Invoke();
            return true;
        }

        public long GetInstantFinishCost(string buildingId)
        {
            long remaining = RemainingSeconds(GetJob(buildingId));
            if (remaining <= 0) return 0;
            long minutes = (remaining + 59) / 60;
            return minutes * Math.Max(1, config.instantFinishDiamondsPerMinute);
        }

        /// <summary>Pays diamonds and completes the job now. Nothing is charged if the job does not exist or diamonds are missing.</summary>
        public bool TryInstantFinish(string buildingId)
        {
            ConstructionJobData job = GetJob(buildingId);
            if (job == null) return false;

            long cost = GetInstantFinishCost(buildingId);
            if (cost > 0 && !wallet.TrySpend(ResourceType.Diamonds, cost)) return false;

            job.endUtc = Math.Min(job.endUtc, buildings.Now());
            Complete(job);
            saveRequested?.Invoke();
            return true;
        }

        // ------------------------------------------------------------------ completion

        /// <summary>Completes every job whose time is up (also used after loading a save, i.e. offline completion). Returns how many completed.</summary>
        public int CompleteFinished()
        {
            int completed = 0;
            long now = buildings.Now();

            while (true)
            {
                ConstructionJobData next = null;
                foreach (ConstructionJobData job in data.constructionQueue)
                {
                    if (job.endUtc <= now && (next == null || job.endUtc < next.endUtc)) next = job;
                }
                if (next == null) break;

                Complete(next);
                completed++;
            }

            if (completed > 0) saveRequested?.Invoke();
            return completed;
        }

        private void Complete(ConstructionJobData job)
        {
            if (!data.constructionQueue.Remove(job)) return; // already completed: do nothing (idempotent)

            BuildingInstanceData instance = buildings.GetInstance(job.buildingId);
            if (instance != null && instance.level == job.fromLevel)
            {
                // Output up to the finish time uses the OLD level; everything after uses the new one.
                long finish = Math.Min(job.endUtc, buildings.Now());
                buildings.Accrue(instance, finish);
                instance.level = job.targetLevel;
                if (instance.productionAnchorUtc < finish) instance.productionAnchorUtc = finish;
                buildings.RecalculatePower();
                GameEvents.RaiseBuildingChanged(job.buildingId);
                GameEvents.RaiseConstructionCompleted(job.buildingId, job.targetLevel);
            }

            GameEvents.RaiseConstructionChanged();
        }
    }
}
