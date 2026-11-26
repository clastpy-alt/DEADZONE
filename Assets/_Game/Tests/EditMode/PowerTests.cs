using Deadzone.Buildings;
using Deadzone.SaveSystem;
using NUnit.Framework;

namespace Deadzone.Tests
{
    /// <summary>Power Calculation: building power from level data, plus future systems through IPowerSource.</summary>
    public sealed class PowerTests
    {
        private sealed class FakePower : IPowerSource
        {
            public PowerCategory Category { get; set; }
            public long Power;
            public long GetPower() => Power;
        }

        private TestWorld world;

        [SetUp]
        public void SetUp() => world = TestWorld.Create();

        [TearDown]
        public void TearDown() => world.Cleanup();

        private long LevelPower(string id, int level) => world.Database.Get(id).GetLevel(level).powerContribution;

        [Test]
        public void Total_IsSumOfEveryBuildingsLevelPower_SplitIntoHqAndOthers()
        {
            PowerBreakdown breakdown = PlayerPower.Calculate(world.Data, world.Database);

            Assert.AreEqual(LevelPower(TestWorld.Hq, 1), breakdown.headquarters);
            Assert.AreEqual(LevelPower(TestWorld.Farm, 1) + LevelPower(TestWorld.Barracks, 1), breakdown.buildings);
            Assert.AreEqual(0, breakdown.research + breakdown.heroes + breakdown.troops);
            Assert.AreEqual(breakdown.headquarters + breakdown.buildings, breakdown.Total);
        }

        [Test]
        public void HigherLevels_GiveMorePower()
        {
            long before = PlayerPower.Calculate(world.Data, world.Database).Total;
            world.SetLevel(TestWorld.Hq, 5);
            world.SetLevel(TestWorld.Farm, 4);
            long after = PlayerPower.Calculate(world.Data, world.Database).Total;

            Assert.Greater(after, before);
            Assert.AreEqual(LevelPower(TestWorld.Hq, 5) + LevelPower(TestWorld.Farm, 4) + LevelPower(TestWorld.Barracks, 1), after);
        }

        [Test]
        public void UnknownBuildingsAndEmptyBase_ContributeNothing()
        {
            world.AddInstance(world.Data, "ghost", 9);
            Assert.AreEqual(PlayerPower.Calculate(world.Data, world.Database).Total,
                LevelPower(TestWorld.Hq, 1) + LevelPower(TestWorld.Farm, 1) + LevelPower(TestWorld.Barracks, 1));

            world.Data.buildings.Clear();
            Assert.AreEqual(0, PlayerPower.Calculate(world.Data, world.Database).Total);
        }

        [Test]
        public void RegisteredSources_AreAddedByCategory_AndNegativeValuesAreIgnored()
        {
            var research = new FakePower { Category = PowerCategory.Research, Power = 500 };
            var troops = new FakePower { Category = PowerCategory.Troop, Power = 250 };
            var broken = new FakePower { Category = PowerCategory.Hero, Power = -900 };
            long baseTotal = PlayerPower.Calculate(world.Data, world.Database).Total;

            PlayerPower.Register(research);
            PlayerPower.Register(research); // registering twice must not double count
            PlayerPower.Register(troops);
            PlayerPower.Register(broken);
            try
            {
                PowerBreakdown breakdown = PlayerPower.Calculate(world.Data, world.Database);
                Assert.AreEqual(500, breakdown.research);
                Assert.AreEqual(250, breakdown.troops);
                Assert.AreEqual(0, breakdown.heroes);
                Assert.AreEqual(baseTotal + 750, breakdown.Total);
            }
            finally
            {
                PlayerPower.Unregister(research);
                PlayerPower.Unregister(troops);
                PlayerPower.Unregister(broken);
            }
            Assert.AreEqual(baseTotal, PlayerPower.Calculate(world.Data, world.Database).Total);
        }

        [Test]
        public void RecalculatePower_StoresTheTotalInThePlayerProfile()
        {
            Assert.AreEqual(0, world.Data.combatPower);
            world.Buildings.RecalculatePower();
            Assert.AreEqual(PlayerPower.Calculate(world.Data, world.Database).Total, world.Data.combatPower);
            Assert.Greater(world.Data.combatPower, 0);
        }
    }
}
