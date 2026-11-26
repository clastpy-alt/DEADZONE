using Deadzone.Buildings;
using Deadzone.Data;
using NUnit.Framework;

namespace Deadzone.Tests
{
    /// <summary>Upgrade rules: payment, requirements, queue, duplicates, completion (also offline), cancel and speed-up.</summary>
    public sealed class BuildingUpgradeTests
    {
        private TestWorld world;

        [SetUp]
        public void SetUp() => world = TestWorld.Create();

        [TearDown]
        public void TearDown() => world.Cleanup();

        // ---------------------------------------------------------------- Building Upgrade

        [Test]
        public void StartUpgrade_PaysCost_AndCreatesTimedJob()
        {
            long before = world.Get(ResourceType.Food);
            long cost = world.Database.Get(TestWorld.Farm).GetLevel(2).upgradeCost[0].amount;

            Assert.AreEqual(UpgradeBlockReason.None, world.Upgrades.TryStartUpgrade(TestWorld.Farm));

            Assert.AreEqual(before - cost, world.Get(ResourceType.Food));
            ConstructionJobData job = world.Upgrades.GetJob(TestWorld.Farm);
            Assert.IsNotNull(job);
            Assert.AreEqual(1, job.fromLevel);
            Assert.AreEqual(2, job.targetLevel);
            Assert.AreEqual(TestWorld.StartTime, job.startUtc);
            Assert.AreEqual(TestWorld.StartTime + 30, job.endUtc);
            Assert.AreEqual(1, world.Buildings.GetLevel(TestWorld.Farm), "level changes only on completion");
            Assert.AreEqual(1, world.SaveRequests, "a save is requested after starting");
        }

        [Test]
        public void UnknownAndPlannedBuildings_CannotBeUpgraded()
        {
            Assert.AreEqual(UpgradeBlockReason.UnknownBuilding, world.Upgrades.TryStartUpgrade("nope"));

            world.Database.Get(TestWorld.Barracks).availability = BuildingAvailability.Planned;
            Assert.AreEqual(UpgradeBlockReason.NotAvailable, world.Upgrades.TryStartUpgrade(TestWorld.Barracks));
        }

        [Test]
        public void MaxLevel_CannotBeUpgraded()
        {
            world.SetLevel(TestWorld.Hq, 10);
            Assert.AreEqual(UpgradeBlockReason.MaxLevel, world.Upgrades.TryStartUpgrade(TestWorld.Hq));
        }

        // ---------------------------------------------------------------- Insufficient Resources

        [Test]
        public void InsufficientResources_BlocksUpgrade_AndChargesNothing()
        {
            world.Set(ResourceType.Food, 50); // L2 costs 100

            UpgradeCheck check = world.Upgrades.CheckUpgrade(TestWorld.Farm);
            Assert.AreEqual(UpgradeBlockReason.InsufficientResources, check.reason);
            Assert.AreEqual(1, check.missingResources.Count);
            Assert.AreEqual(50, check.missingResources[0].amount);

            Assert.AreEqual(UpgradeBlockReason.InsufficientResources, world.Upgrades.TryStartUpgrade(TestWorld.Farm));
            Assert.AreEqual(50, world.Get(ResourceType.Food), "balance untouched");
            Assert.AreEqual(0, world.Upgrades.Queue.Count);
        }

        [Test]
        public void Wallet_NeverGoesNegative()
        {
            world.Set(ResourceType.Gold, 10);
            Assert.IsFalse(world.Wallet.TrySpend(ResourceType.Gold, 11));
            Assert.IsFalse(world.Wallet.TrySpend(ResourceType.Gold, -5));
            Assert.AreEqual(0, world.Wallet.Add(ResourceType.Gold, -5));
            Assert.AreEqual(10, world.Get(ResourceType.Gold));
        }

        // ---------------------------------------------------------------- HQ Level Requirement

        [Test]
        public void HeadquartersLevel_LimitsOtherBuildings()
        {
            world.Upgrades.TryStartUpgrade(TestWorld.Farm);
            world.Clock.Advance(30);
            world.Upgrades.CompleteFinished(); // farm level 2, HQ still level 1

            UpgradeCheck check = world.Upgrades.CheckUpgrade(TestWorld.Farm); // L3 needs HQ level 2
            Assert.AreEqual(UpgradeBlockReason.RequirementNotMet, check.reason);
            Assert.AreEqual(1, check.unmetRequirements.Count);
            Assert.AreEqual(TestWorld.Hq, check.unmetRequirements[0].buildingId);
            Assert.AreEqual(2, check.unmetRequirements[0].level);

            long food = world.Get(ResourceType.Food);
            Assert.AreEqual(UpgradeBlockReason.RequirementNotMet, world.Upgrades.TryStartUpgrade(TestWorld.Farm));
            Assert.AreEqual(food, world.Get(ResourceType.Food), "nothing charged while blocked");
        }

        [Test]
        public void HeadquartersUpgrade_UnlocksNextLevelOfOtherBuildings()
        {
            world.Upgrades.TryStartUpgrade(TestWorld.Farm);
            world.Clock.Advance(30);
            world.Upgrades.CompleteFinished();

            world.Upgrades.TryStartUpgrade(TestWorld.Hq);
            world.Clock.Advance(30);
            world.Upgrades.CompleteFinished();

            Assert.AreEqual(UpgradeBlockReason.None, world.Upgrades.CheckUpgrade(TestWorld.Farm).reason);
        }

        // ---------------------------------------------------------------- Construction Queue Limit

        [Test]
        public void QueueLimit_AllowsOnlyOneConstructionByDefault()
        {
            Assert.AreEqual(1, world.Upgrades.QueueSlots);
            Assert.AreEqual(UpgradeBlockReason.None, world.Upgrades.TryStartUpgrade(TestWorld.Hq));

            long food = world.Get(ResourceType.Food);
            Assert.AreEqual(UpgradeBlockReason.QueueFull, world.Upgrades.TryStartUpgrade(TestWorld.Barracks));
            Assert.AreEqual(food, world.Get(ResourceType.Food));
            Assert.AreEqual(1, world.Upgrades.Queue.Count);
        }

        [Test]
        public void QueueLimit_ExtraSlotsFromVipHookAreHonoured()
        {
            world.Upgrades.ExtraQueueSlots = () => 1;
            Assert.AreEqual(2, world.Upgrades.QueueSlots);
            Assert.AreEqual(UpgradeBlockReason.None, world.Upgrades.TryStartUpgrade(TestWorld.Hq));
            Assert.AreEqual(UpgradeBlockReason.None, world.Upgrades.TryStartUpgrade(TestWorld.Barracks));
            Assert.AreEqual(UpgradeBlockReason.QueueFull, world.Upgrades.TryStartUpgrade(TestWorld.Farm));
        }

        [Test]
        public void QueueSlot_IsFreeAgainAfterCompletion()
        {
            world.Upgrades.TryStartUpgrade(TestWorld.Hq);
            world.Clock.Advance(30);
            world.Upgrades.CompleteFinished();
            Assert.AreEqual(UpgradeBlockReason.None, world.Upgrades.TryStartUpgrade(TestWorld.Barracks));
        }

        // ---------------------------------------------------------------- Duplicate Upgrade Prevention

        [Test]
        public void DuplicateUpgrade_OfSameBuilding_IsRejectedAndPaidOnce()
        {
            long before = world.Get(ResourceType.Food);
            long cost = world.Database.Get(TestWorld.Farm).GetLevel(2).upgradeCost[0].amount;

            Assert.AreEqual(UpgradeBlockReason.None, world.Upgrades.TryStartUpgrade(TestWorld.Farm));
            Assert.AreEqual(UpgradeBlockReason.AlreadyUpgrading, world.Upgrades.TryStartUpgrade(TestWorld.Farm));
            Assert.AreEqual(UpgradeBlockReason.AlreadyUpgrading, world.Upgrades.TryStartUpgrade(TestWorld.Farm));

            Assert.AreEqual(1, world.Upgrades.Queue.Count);
            Assert.AreEqual(before - cost, world.Get(ResourceType.Food), "charged exactly once");
        }

        [Test]
        public void DuplicateCompletion_GrantsOneLevelOnly()
        {
            world.Upgrades.TryStartUpgrade(TestWorld.Farm);
            world.Clock.Advance(1000);

            Assert.AreEqual(1, world.Upgrades.CompleteFinished());
            Assert.AreEqual(0, world.Upgrades.CompleteFinished());
            Assert.AreEqual(0, world.Upgrades.CompleteFinished());
            Assert.AreEqual(2, world.Buildings.GetLevel(TestWorld.Farm));
        }

        // ---------------------------------------------------------------- Construction Completion

        [Test]
        public void Construction_DoesNotCompleteBeforeItsEndTime()
        {
            world.Upgrades.TryStartUpgrade(TestWorld.Farm);
            world.Clock.Advance(29);

            Assert.AreEqual(0, world.Upgrades.CompleteFinished());
            Assert.AreEqual(1, world.Buildings.GetLevel(TestWorld.Farm));
            Assert.AreEqual(1, world.Upgrades.RemainingSeconds(world.Upgrades.GetJob(TestWorld.Farm)));
            Assert.Greater(world.Upgrades.Progress01(world.Upgrades.GetJob(TestWorld.Farm)), 0.9f);
        }

        [Test]
        public void Construction_CompletesAtEndTime_RaisesLevelAndPower()
        {
            long powerBefore = PlayerPower.Calculate(world.Data, world.Database).Total;
            int completedEvents = 0;
            void OnCompleted(string id, int level) { if (id == TestWorld.Farm && level == 2) completedEvents++; }
            Deadzone.Core.GameEvents.ConstructionCompleted += OnCompleted;
            try
            {
                world.Upgrades.TryStartUpgrade(TestWorld.Farm);
                world.Clock.Advance(30);
                Assert.AreEqual(1, world.Upgrades.CompleteFinished());
            }
            finally { Deadzone.Core.GameEvents.ConstructionCompleted -= OnCompleted; }

            Assert.AreEqual(2, world.Buildings.GetLevel(TestWorld.Farm));
            Assert.IsNull(world.Upgrades.GetJob(TestWorld.Farm));
            Assert.AreEqual(0, world.Upgrades.Queue.Count);
            Assert.Greater(world.Data.combatPower, powerBefore);
            Assert.AreEqual(PlayerPower.Calculate(world.Data, world.Database).Total, world.Data.combatPower);
            Assert.AreEqual(1, completedEvents);
        }

        // ---------------------------------------------------------------- Offline Construction Completion

        [Test]
        public void OfflineConstruction_CompletesAfterReload()
        {
            world.Upgrades.TryStartUpgrade(TestWorld.Farm);

            TestWorld reopened = world.Reload(advanceSeconds: 3600); // game closed for an hour
            try
            {
                Assert.AreEqual(1, reopened.Buildings.GetLevel(TestWorld.Farm), "still level 1 until the load step runs");
                Assert.AreEqual(1, reopened.Upgrades.CompleteFinished());
                Assert.AreEqual(2, reopened.Buildings.GetLevel(TestWorld.Farm));
                Assert.AreEqual(0, reopened.Upgrades.Queue.Count);
            }
            finally { reopened.Cleanup(); }
        }

        [Test]
        public void OfflineConstruction_NotFinishedJobKeepsRunningAfterReload()
        {
            world.Upgrades.TryStartUpgrade(TestWorld.Hq); // 30s
            TestWorld reopened = world.Reload(advanceSeconds: 10);
            try
            {
                Assert.AreEqual(0, reopened.Upgrades.CompleteFinished());
                ConstructionJobData job = reopened.Upgrades.GetJob(TestWorld.Hq);
                Assert.IsNotNull(job);
                Assert.AreEqual(20, reopened.Upgrades.RemainingSeconds(job));
            }
            finally { reopened.Cleanup(); }
        }

        [Test]
        public void ClockRollback_DoesNotCompleteOrShortenConstruction()
        {
            world.Upgrades.TryStartUpgrade(TestWorld.Farm);
            world.Clock.Advance(10);
            Assert.AreEqual(20, world.Upgrades.RemainingSeconds(world.Upgrades.GetJob(TestWorld.Farm)));

            world.Clock.Now = TestWorld.StartTime - 100000; // player sets the device clock far back
            Assert.GreaterOrEqual(world.Buildings.Now(), TestWorld.StartTime + 10, "time never moves backwards");
            Assert.AreEqual(20, world.Upgrades.RemainingSeconds(world.Upgrades.GetJob(TestWorld.Farm)));
            Assert.AreEqual(0, world.Upgrades.CompleteFinished());
        }

        // ---------------------------------------------------------------- Upgrade Cancellation

        [Test]
        public void Cancel_RefundsConfiguredPercent_AndRemovesJob()
        {
            long before = world.Get(ResourceType.Food);
            long cost = world.Database.Get(TestWorld.Farm).GetLevel(2).upgradeCost[0].amount;
            world.Upgrades.TryStartUpgrade(TestWorld.Farm);

            Assert.IsTrue(world.Upgrades.Cancel(TestWorld.Farm));

            Assert.IsNull(world.Upgrades.GetJob(TestWorld.Farm));
            Assert.AreEqual(1, world.Buildings.GetLevel(TestWorld.Farm));
            Assert.AreEqual(before - cost + cost * 80 / 100, world.Get(ResourceType.Food));
        }

        [Test]
        public void Cancel_Twice_RefundsOnlyOnce()
        {
            world.Upgrades.TryStartUpgrade(TestWorld.Farm);
            Assert.IsTrue(world.Upgrades.Cancel(TestWorld.Farm));
            long afterFirst = world.Get(ResourceType.Food);

            Assert.IsFalse(world.Upgrades.Cancel(TestWorld.Farm));
            Assert.AreEqual(afterFirst, world.Get(ResourceType.Food));
        }

        [Test]
        public void Cancel_AfterCompletion_DoesNothing()
        {
            world.Upgrades.TryStartUpgrade(TestWorld.Farm);
            world.Clock.Advance(30);
            world.Upgrades.CompleteFinished();
            long food = world.Get(ResourceType.Food);

            Assert.IsFalse(world.Upgrades.Cancel(TestWorld.Farm));
            Assert.AreEqual(food, world.Get(ResourceType.Food));
            Assert.AreEqual(2, world.Buildings.GetLevel(TestWorld.Farm));
        }

        [Test]
        public void Cancel_FreesTheQueueSlot()
        {
            world.Upgrades.TryStartUpgrade(TestWorld.Hq);
            world.Upgrades.Cancel(TestWorld.Hq);
            Assert.AreEqual(UpgradeBlockReason.None, world.Upgrades.TryStartUpgrade(TestWorld.Barracks));
        }

        // ---------------------------------------------------------------- Speed-up / instant finish

        [Test]
        public void InstantFinish_ChargesDiamondsPerStartedMinute_AndCompletes()
        {
            world.Upgrades.TryStartUpgrade(TestWorld.Hq); // 30s => 1 started minute => 1 diamond
            Assert.AreEqual(1, world.Upgrades.GetInstantFinishCost(TestWorld.Hq));

            Assert.IsTrue(world.Upgrades.TryInstantFinish(TestWorld.Hq));

            Assert.AreEqual(9, world.Get(ResourceType.Diamonds));
            Assert.AreEqual(2, world.Buildings.GetLevel(TestWorld.Hq));
            Assert.AreEqual(0, world.Upgrades.Queue.Count);
        }

        [Test]
        public void InstantFinish_WithoutDiamonds_ChangesNothing()
        {
            world.Upgrades.TryStartUpgrade(TestWorld.Hq);
            world.Set(ResourceType.Diamonds, 0);

            Assert.IsFalse(world.Upgrades.TryInstantFinish(TestWorld.Hq));
            Assert.IsNotNull(world.Upgrades.GetJob(TestWorld.Hq));
            Assert.AreEqual(1, world.Buildings.GetLevel(TestWorld.Hq));
            Assert.AreEqual(0, world.Get(ResourceType.Diamonds));
        }

        [Test]
        public void Speedup_ShortensRemainingTime_AndCompletesWhenTimeRunsOut()
        {
            world.SetLevel(TestWorld.Hq, 3);
            world.Upgrades.TryStartUpgrade(TestWorld.Hq); // 3 -> 4 = 300s
            Assert.IsTrue(world.Upgrades.ApplySpeedup(TestWorld.Hq, 100));
            Assert.AreEqual(200, world.Upgrades.RemainingSeconds(world.Upgrades.GetJob(TestWorld.Hq)));

            Assert.IsTrue(world.Upgrades.ApplySpeedup(TestWorld.Hq, 100000));
            Assert.AreEqual(4, world.Buildings.GetLevel(TestWorld.Hq));
            Assert.IsFalse(world.Upgrades.ApplySpeedup(TestWorld.Hq, 10), "no job left");
        }
    }
}
