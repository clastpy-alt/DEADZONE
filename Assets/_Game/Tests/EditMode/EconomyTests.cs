using Deadzone.Buildings;
using Deadzone.Data;
using NUnit.Framework;

namespace Deadzone.Tests
{
    /// <summary>Production, storage capacity, collection, offline production and the "no duplication" guarantees.</summary>
    public sealed class EconomyTests
    {
        private TestWorld world;

        [SetUp]
        public void SetUp() => world = TestWorld.Create();

        [TearDown]
        public void TearDown() => world.Cleanup();

        // ---------------------------------------------------------------- Resource Production

        [Test]
        public void Production_AccruesPerSecondFromTheClock()
        {
            Assert.AreEqual(3600, world.Buildings.GetProductionPerHour(TestWorld.Farm));

            world.Clock.Advance(100);
            world.Buildings.AccrueAll();

            Assert.AreEqual(100, world.Buildings.GetStored(TestWorld.Farm));
        }

        [Test]
        public void Production_FromNonProducers_IsZero()
        {
            world.Clock.Advance(1000);
            world.Buildings.AccrueAll();
            Assert.AreEqual(0, world.Buildings.GetProductionPerHour(TestWorld.Barracks));
            Assert.AreEqual(0, world.Buildings.GetStored(TestWorld.Barracks));
            Assert.AreEqual(0, world.Buildings.GetStored(TestWorld.Hq));
        }

        [Test]
        public void Collect_MovesStoredOutputIntoTheWalletOnce()
        {
            long before = world.Get(ResourceType.Food);
            world.Clock.Advance(100);

            Assert.AreEqual(100, world.Buildings.Collect(TestWorld.Farm));
            Assert.AreEqual(before + 100, world.Get(ResourceType.Food));
            Assert.AreEqual(0, world.Buildings.GetStored(TestWorld.Farm));

            Assert.AreEqual(0, world.Buildings.Collect(TestWorld.Farm), "second collect finds nothing");
            Assert.AreEqual(before + 100, world.Get(ResourceType.Food));
        }

        [Test]
        public void Accrue_SameMomentTwice_CreditsOnce()
        {
            world.Clock.Advance(100);
            world.Buildings.AccrueAll();
            world.Buildings.AccrueAll();
            world.Buildings.AccrueAll();
            Assert.AreEqual(100, world.Buildings.GetStored(TestWorld.Farm));
        }

        [Test]
        public void Accrue_ClockRollback_CreditsNothing()
        {
            world.Clock.Advance(100);
            world.Buildings.AccrueAll();

            world.Clock.Now = TestWorld.StartTime - 5000;
            world.Buildings.AccrueAll();
            Assert.AreEqual(100, world.Buildings.GetStored(TestWorld.Farm));
        }

        [Test]
        public void Production_UsesOldLevelUntilUpgradeFinishes_ThenNewLevel()
        {
            world.Upgrades.TryStartUpgrade(TestWorld.Farm); // 30s, level 1 -> 2
            world.Clock.Advance(3630);
            world.Upgrades.CompleteFinished();
            world.Buildings.AccrueAll();

            long level2PerHour = world.Database.Get(TestWorld.Farm).GetLevel(2).productionPerHour;
            Assert.AreEqual(4248, level2PerHour);
            // 30 s at level 1 (30) + 3600 s at level 2 (4248)
            Assert.AreEqual(30 + level2PerHour, world.Buildings.GetStored(TestWorld.Farm));
        }

        // ---------------------------------------------------------------- Storage Capacity

        [Test]
        public void BufferCapacity_StopsProduction_AndNothingIsBanked()
        {
            world.Database.Get(TestWorld.Farm).levels[0].storageCapacity = 500;

            world.Clock.Advance(1000);
            world.Buildings.AccrueAll();
            Assert.AreEqual(500, world.Buildings.GetStored(TestWorld.Farm));
            Assert.AreEqual(500, world.Buildings.GetBufferCapacity(TestWorld.Farm));

            world.Clock.Advance(5000); // still full: wasted, not stored for later
            world.Buildings.AccrueAll();
            Assert.AreEqual(500, world.Buildings.GetStored(TestWorld.Farm));

            world.Buildings.Collect(TestWorld.Farm);
            world.Clock.Advance(10);
            world.Buildings.AccrueAll();
            Assert.AreEqual(10, world.Buildings.GetStored(TestWorld.Farm), "production restarts from the collection moment");
        }

        [Test]
        public void WalletCapacity_LimitsCollect_AndRemainderStaysInTheBuilding()
        {
            world.Cleanup();
            world = TestWorld.Create(capacity: 1000);
            world.Database.Get(TestWorld.Farm).levels[0].storageCapacity = 500;
            world.Set(ResourceType.Food, 900);
            world.Clock.Advance(1000);

            Assert.AreEqual(100, world.Buildings.Collect(TestWorld.Farm));
            Assert.AreEqual(1000, world.Get(ResourceType.Food));
            Assert.AreEqual(400, world.Buildings.GetStored(TestWorld.Farm));
            Assert.AreEqual(0, world.Buildings.Collect(TestWorld.Farm), "wallet is full: nothing moves, nothing is lost");
            Assert.AreEqual(400, world.Buildings.GetStored(TestWorld.Farm));
        }

        [Test]
        public void Warehouse_RaisesCapacityOfBulkMaterialsOnly()
        {
            world.Cleanup();
            world = TestWorld.Create(withWarehouse: true, capacity: 1000);

            Assert.AreEqual(2000, world.Buildings.GetCapacityBonus(ResourceType.Food));
            Assert.AreEqual(3000, world.Wallet.GetCapacity(ResourceType.Food));
            Assert.AreEqual(3000, world.Wallet.GetCapacity(ResourceType.Steel));
            Assert.AreEqual(0, world.Buildings.GetCapacityBonus(ResourceType.Gold));
            Assert.AreEqual(0, world.Buildings.GetCapacityBonus(ResourceType.Diamonds));
            Assert.AreEqual(120, world.Wallet.GetCapacity(ResourceType.Energy));

            world.Set(ResourceType.Food, 2900);
            Assert.AreEqual(100, world.Wallet.Add(ResourceType.Food, 500));
        }

        [Test]
        public void Warehouse_DoesNotProduceIntoItsOwnBuffer()
        {
            world.Cleanup();
            world = TestWorld.Create(withWarehouse: true);
            world.Clock.Advance(5000);
            world.Buildings.AccrueAll();
            Assert.AreEqual(0, world.Buildings.GetProductionPerHour(TestWorld.Warehouse));
            Assert.AreEqual(0, world.Buildings.GetStored(TestWorld.Warehouse));
            Assert.AreEqual(0, world.Buildings.Collect(TestWorld.Warehouse));
        }

        // ---------------------------------------------------------------- Offline Production

        [Test]
        public void OfflineProduction_IsCreditedAfterReload()
        {
            TestWorld reopened = world.Reload(advanceSeconds: 3600);
            try
            {
                reopened.Buildings.AccrueAll();
                Assert.AreEqual(3600, reopened.Buildings.GetStored(TestWorld.Farm));
            }
            finally { reopened.Cleanup(); }
        }

        [Test]
        public void OfflineProduction_IsCappedAtMaxOfflineHours()
        {
            TestWorld reopened = world.Reload(advanceSeconds: 20 * 3600); // 20 h away, cap is 8 h
            try
            {
                reopened.Buildings.AccrueAll();
                Assert.AreEqual(8 * 3600, reopened.Buildings.GetStored(TestWorld.Farm));
            }
            finally { reopened.Cleanup(); }
        }

        [Test]
        public void OfflineProduction_AfterCapIsReached_DoesNotRestartTheClock()
        {
            TestWorld reopened = world.Reload(advanceSeconds: 20 * 3600);
            try
            {
                reopened.Buildings.AccrueAll();
                reopened.Clock.Advance(60);
                reopened.Buildings.AccrueAll();
                Assert.AreEqual(8 * 3600 + 60, reopened.Buildings.GetStored(TestWorld.Farm));
            }
            finally { reopened.Cleanup(); }
        }

        // ---------------------------------------------------------------- Resource Duplication Prevention

        [Test]
        public void NoDuplication_ReloadingMidConstruction_ChargesAndCompletesOnce()
        {
            long start = world.Get(ResourceType.Food);
            long cost = world.Database.Get(TestWorld.Farm).GetLevel(2).upgradeCost[0].amount;
            world.Upgrades.TryStartUpgrade(TestWorld.Farm);

            TestWorld first = world.Reload(5);
            TestWorld second = first.Reload(5);
            TestWorld third = second.Reload(3600);
            try
            {
                Assert.AreEqual(start - cost, third.Get(ResourceType.Food), "cost is stored with the save, not charged again");
                Assert.AreEqual(1, third.Upgrades.CompleteFinished());
                Assert.AreEqual(0, third.Upgrades.CompleteFinished());
                Assert.AreEqual(2, third.Buildings.GetLevel(TestWorld.Farm));
                Assert.AreEqual(start - cost, third.Get(ResourceType.Food));
            }
            finally { first.Cleanup(); second.Cleanup(); third.Cleanup(); }
        }

        [Test]
        public void NoDuplication_StoredOutputIsCollectedOnlyOnceAcrossReloads()
        {
            world.Clock.Advance(100);
            world.Buildings.AccrueAll();

            TestWorld reopened = world.Reload(0);
            TestWorld again = null;
            try
            {
                Assert.AreEqual(100, reopened.Buildings.GetStored(TestWorld.Farm), "stored output survives the reload");
                long before = reopened.Get(ResourceType.Food);
                Assert.AreEqual(100, reopened.Buildings.Collect(TestWorld.Farm));

                again = reopened.Reload(0);
                Assert.AreEqual(before + 100, again.Get(ResourceType.Food));
                Assert.AreEqual(0, again.Buildings.Collect(TestWorld.Farm));
                Assert.AreEqual(before + 100, again.Get(ResourceType.Food));
            }
            finally { reopened.Cleanup(); again?.Cleanup(); }
        }

        [Test]
        public void NoDuplication_CancelAfterInstantFinish_RefundsNothing()
        {
            world.Upgrades.TryStartUpgrade(TestWorld.Hq);
            Assert.IsTrue(world.Upgrades.TryInstantFinish(TestWorld.Hq));
            long food = world.Get(ResourceType.Food);
            long diamonds = world.Get(ResourceType.Diamonds);

            Assert.IsFalse(world.Upgrades.Cancel(TestWorld.Hq));
            Assert.IsFalse(world.Upgrades.TryInstantFinish(TestWorld.Hq));
            Assert.AreEqual(food, world.Get(ResourceType.Food));
            Assert.AreEqual(diamonds, world.Get(ResourceType.Diamonds));
        }

        [Test]
        public void StartingEconomy_OfNewSaves_MatchesTheTestEconomy()
        {
            var config = UnityEngine.ScriptableObject.CreateInstance<GameConfig>();
            try
            {
                var data = Deadzone.SaveSystem.PlayerData.CreateNew(config);
                long Amount(ResourceType type) => data.resources.Find(r => r.type == type).amount;
                Assert.AreEqual(5000, Amount(ResourceType.Food));
                Assert.AreEqual(5000, Amount(ResourceType.Wood));
                Assert.AreEqual(5000, Amount(ResourceType.Steel));
                Assert.AreEqual(2500, Amount(ResourceType.Fuel));
                Assert.AreEqual(1000, Amount(ResourceType.Gold));
                Assert.AreEqual(200, Amount(ResourceType.Diamonds));
                Assert.AreEqual(100, Amount(ResourceType.Energy));
                Assert.AreEqual(GameConfig.DefaultStartingBuildings().Count, data.buildings.Count);
            }
            finally { UnityEngine.Object.DestroyImmediate(config); }
        }
    }
}
