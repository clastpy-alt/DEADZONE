using System;
using System.Collections.Generic;
using Deadzone.Core;
using Deadzone.Data;
using Deadzone.Economy;
using Deadzone.SaveSystem;

namespace Deadzone.Buildings
{
    /// <summary>
    /// Owns building instances: levels, production, storage and collection. Constructions are handled by
    /// BuildingUpgradeService. Time always comes from IGameClock and never moves backwards.
    /// </summary>
    public sealed class BuildingManager
    {
        private readonly PlayerData data;
        private readonly ResourceWallet wallet;
        private readonly IGameClock clock;
        private readonly GameConfig config;

        public BuildingDatabase Database { get; }

        public BuildingManager(PlayerData data, BuildingDatabase database, ResourceWallet wallet, IGameClock clock, GameConfig config)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            this.wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public IReadOnlyList<BuildingInstanceData> Instances => data.buildings;

        /// <summary>Current UTC unix seconds. Never lower than a value seen before (protects against clock rollback).</summary>
        public long Now()
        {
            long time = clock.UtcNowSeconds;
            if (time < data.lastSeenUtc) return data.lastSeenUtc;
            data.lastSeenUtc = time;
            return time;
        }

        public BuildingInstanceData GetInstance(string buildingId)
        {
            foreach (BuildingInstanceData instance in data.buildings)
            {
                if (instance.buildingId == buildingId) return instance;
            }
            return null;
        }

        public int GetLevel(string buildingId) => GetInstance(buildingId)?.level ?? 0;

        public BuildingDefinition GetDefinition(string buildingId) => Database.Get(buildingId);

        public BuildingLevelData GetCurrentLevelData(string buildingId)
        {
            BuildingInstanceData instance = GetInstance(buildingId);
            return instance == null ? null : Database.Get(buildingId)?.GetLevel(instance.level);
        }

        public long GetProductionPerHour(string buildingId)
        {
            BuildingDefinition definition = Database.Get(buildingId);
            if (definition == null || !definition.production.producesResource || definition.production.storageIsGlobal) return 0;
            return GetCurrentLevelData(buildingId)?.productionPerHour ?? 0;
        }

        /// <summary>Capacity of the building's own output buffer (0 for non-producers and warehouses).</summary>
        public long GetBufferCapacity(string buildingId)
        {
            BuildingDefinition definition = Database.Get(buildingId);
            if (definition == null || !definition.production.producesResource || definition.production.storageIsGlobal) return 0;
            return GetCurrentLevelData(buildingId)?.storageCapacity ?? 0;
        }

        public long GetStored(string buildingId) => GetInstance(buildingId)?.storedAmount ?? 0;

        /// <summary>Extra wallet capacity from warehouses. Only the bulk materials are affected.</summary>
        public long GetCapacityBonus(ResourceType type)
        {
            if (type != ResourceType.Food && type != ResourceType.Wood && type != ResourceType.Steel && type != ResourceType.Fuel) return 0;

            long bonus = 0;
            foreach (BuildingInstanceData instance in data.buildings)
            {
                BuildingDefinition definition = Database.Get(instance.buildingId);
                if (definition == null || !definition.production.storageIsGlobal) continue;
                bonus += definition.GetLevel(instance.level)?.storageCapacity ?? 0;
            }
            return bonus;
        }

        /// <summary>Credits production up to 'untilUtc'. Safe to call any number of times; time that was already credited is never credited twice.</summary>
        public void Accrue(BuildingInstanceData instance, long untilUtc)
        {
            BuildingDefinition definition = Database.Get(instance.buildingId);
            BuildingLevelData level = definition?.GetLevel(instance.level);
            if (level == null || !definition.production.producesResource || definition.production.storageIsGlobal || level.productionPerHour <= 0)
            {
                if (untilUtc > instance.productionAnchorUtc) instance.productionAnchorUtc = untilUtc;
                return;
            }

            if (untilUtc <= instance.productionAnchorUtc) return; // clock rollback or already credited

            long maxSeconds = (long)(config.maxOfflineHours * 3600f);
            if (untilUtc - instance.productionAnchorUtc > maxSeconds) instance.productionAnchorUtc = untilUtc - maxSeconds;

            if (instance.storedAmount >= level.storageCapacity)
            {
                instance.productionAnchorUtc = untilUtc; // buffer full: production is wasted, not banked
                return;
            }

            long elapsed = untilUtc - instance.productionAnchorUtc;
            long produced = (long)Math.Floor(level.productionPerHour * (double)elapsed / 3600.0);
            if (produced <= 0) return; // keep the anchor so the fraction accumulates

            long before = instance.storedAmount;
            instance.storedAmount = Math.Min(level.storageCapacity, instance.storedAmount + produced);
            if (instance.storedAmount >= level.storageCapacity)
            {
                instance.productionAnchorUtc = untilUtc;
            }
            else
            {
                // advance only by the time that was paid out (rounded up so the same second is never credited twice)
                instance.productionAnchorUtc += (long)Math.Ceiling(produced * 3600.0 / level.productionPerHour);
                if (instance.productionAnchorUtc > untilUtc) instance.productionAnchorUtc = untilUtc;
            }

            if (instance.storedAmount != before) GameEvents.RaiseBuildingChanged(instance.buildingId);
        }

        public void AccrueAll()
        {
            long now = Now();
            foreach (BuildingInstanceData instance in data.buildings) Accrue(instance, now);
        }

        /// <summary>Moves the stored output into the wallet. Whatever does not fit stays in the building.</summary>
        public long Collect(string buildingId)
        {
            BuildingInstanceData instance = GetInstance(buildingId);
            BuildingDefinition definition = Database.Get(buildingId);
            if (instance == null || definition == null || !definition.production.producesResource || definition.production.storageIsGlobal) return 0;

            long now = Now();
            Accrue(instance, now);
            if (instance.storedAmount <= 0) return 0;

            long added = wallet.Add(definition.production.resource, instance.storedAmount);
            if (added <= 0) return 0;

            instance.storedAmount -= added;
            instance.lastCollectedUtc = now;
            GameEvents.RaiseBuildingChanged(buildingId);
            return added;
        }

        public void RecalculatePower()
        {
            long total = PlayerPower.Calculate(data, Database).Total;
            if (data.combatPower == total) return;
            data.combatPower = total;
            GameEvents.RaisePowerChanged();
            GameEvents.RaisePlayerProfileChanged();
        }
    }
}
