using System;
using System.Collections.Generic;
using System.IO;
using Deadzone.Buildings;
using Deadzone.Data;
using Deadzone.SaveSystem;
using NUnit.Framework;
using UnityEngine;

namespace Deadzone.Tests
{
    /// <summary>Save version 2: lossless migration from version 1, reload of buildings/queue/timestamps, repair of broken data.</summary>
    public sealed class SaveV2Tests
    {
        private GameConfig config;
        private string directory;
        private SaveService service;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            directory = Path.Combine(Path.GetTempPath(), "deadzone-v2-tests-" + Guid.NewGuid().ToString("N"));
            service = new SaveService(directory);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(config);
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        /// <summary>Writes a save exactly as version 1 stored it: no buildings, no queue, no timestamps, envelope version 1.</summary>
        private PlayerData WriteVersion1Save()
        {
            PlayerData data = PlayerData.CreateNew(config);
            data.playerName = "Veteran";
            data.commanderLevel = 12;
            data.combatPower = 4321;
            data.resources.Find(r => r.type == ResourceType.Food).amount = 1234;
            data.resources.Find(r => r.type == ResourceType.Gold).amount = 77;
            data.buildings.Clear();
            data.constructionQueue.Clear();
            data.lastLoginUtc = 0;
            data.lastSeenUtc = 0;
            service.Save(data);

            string json = File.ReadAllText(service.MainPath);
            Assert.IsTrue(json.Contains("\"version\":2"), "test precondition: envelope format");
            File.WriteAllText(service.MainPath, json.Replace("\"version\":2", "\"version\":1"));
            return data;
        }

        // ---------------------------------------------------------------- Save Version Migration

        [Test]
        public void CurrentSaveVersion_IsTwo() => Assert.AreEqual(2, SaveService.CurrentVersion);

        [Test]
        public void Migration_V1ToV2_AddsStartingBuildings_AndKeepsEverythingElse()
        {
            PlayerData original = WriteVersion1Save();

            SaveService.LoadResult result = service.Load(config);

            Assert.AreEqual(SaveService.LoadOutcome.Loaded, result.Outcome);
            PlayerData loaded = result.Data;
            Assert.AreEqual(original.playerId, loaded.playerId);
            Assert.AreEqual("Veteran", loaded.playerName);
            Assert.AreEqual(12, loaded.commanderLevel);
            Assert.AreEqual(4321, loaded.combatPower);
            Assert.AreEqual(1234, loaded.resources.Find(r => r.type == ResourceType.Food).amount, "no resource is gained or lost");
            Assert.AreEqual(77, loaded.resources.Find(r => r.type == ResourceType.Gold).amount);

            Assert.AreEqual(6, loaded.buildings.Count);
            foreach (GameConfig.StartingBuilding start in GameConfig.DefaultStartingBuildings())
            {
                BuildingInstanceData instance = loaded.buildings.Find(b => b.buildingId == start.buildingId);
                Assert.IsNotNull(instance, start.buildingId);
                Assert.AreEqual(1, instance.level);
                Assert.AreEqual(0, instance.storedAmount);
                Assert.Greater(instance.productionAnchorUtc, 0);
            }
            Assert.IsEmpty(loaded.constructionQueue);
            Assert.Greater(loaded.lastLoginUtc, 0);
            Assert.Greater(loaded.lastSeenUtc, 0);
        }

        [Test]
        public void Migration_NeverDeletesTheSaveOrItsBackup()
        {
            WriteVersion1Save();
            service.Load(config);

            Assert.IsTrue(File.Exists(service.MainPath), "main save still there");
            Assert.AreEqual(0, Directory.GetFiles(directory, "save.corrupt-*").Length, "a v1 save is migrated, never quarantined");
            Assert.IsTrue(service.HasSave());
        }

        [Test]
        public void Migration_ThenSave_WritesVersion2()
        {
            WriteVersion1Save();
            PlayerData loaded = service.Load(config).Data;
            service.Save(loaded);

            StringAssert.Contains("\"version\":2", File.ReadAllText(service.MainPath));
            PlayerData again = new SaveService(directory).Load(config).Data;
            Assert.AreEqual(6, again.buildings.Count, "no duplicate starting buildings after another round trip");
        }

        [Test]
        public void Migration_IsIdempotent_AndKeepsExistingBuildingLevels()
        {
            var data = new PlayerData();
            data.buildings.Add(new BuildingInstanceData { instanceId = "food_farm", buildingId = "food_farm", level = 7, storedAmount = 55, productionAnchorUtc = 5, lastCollectedUtc = 5 });

            SaveMigration.Migrate(data, 1, 1_000);
            SaveMigration.Migrate(data, 1, 2_000);

            Assert.AreEqual(6, data.buildings.Count);
            BuildingInstanceData farm = data.buildings.Find(b => b.buildingId == "food_farm");
            Assert.AreEqual(7, farm.level);
            Assert.AreEqual(55, farm.storedAmount);
            Assert.AreEqual(5, farm.productionAnchorUtc);
            Assert.AreEqual(1_000, data.lastLoginUtc, "timestamps are only set once");
        }

        [Test]
        public void Migration_FromVersion2_ChangesNothing()
        {
            var data = new PlayerData();
            SaveMigration.Migrate(data, 2, 1_000);
            Assert.IsEmpty(data.buildings);
            Assert.AreEqual(0, data.lastLoginUtc);
        }

        // ---------------------------------------------------------------- Save and Reload

        [Test]
        public void SaveAndReload_RestoresBuildingsQueueStoredOutputAndTimestamps()
        {
            TestWorld world = TestWorld.Create();
            try
            {
                world.Upgrades.TryStartUpgrade(TestWorld.Farm);
                world.Clock.Advance(10);
                world.Buildings.AccrueAll();
                world.Data.combatPower = 999;
                service.Save(world.Data);

                PlayerData loaded = new SaveService(directory).Load(world.Config).Data;

                Assert.AreEqual(world.Data.buildings.Count, loaded.buildings.Count);
                BuildingInstanceData farm = loaded.buildings.Find(b => b.buildingId == TestWorld.Farm);
                BuildingInstanceData original = world.Data.buildings.Find(b => b.buildingId == TestWorld.Farm);
                Assert.AreEqual(original.level, farm.level);
                Assert.AreEqual(original.storedAmount, farm.storedAmount);
                Assert.AreEqual(original.productionAnchorUtc, farm.productionAnchorUtc);
                Assert.AreEqual(original.lastCollectedUtc, farm.lastCollectedUtc);

                Assert.AreEqual(1, loaded.constructionQueue.Count);
                ConstructionJobData job = loaded.constructionQueue[0];
                ConstructionJobData expected = world.Upgrades.GetJob(TestWorld.Farm);
                Assert.AreEqual(expected.jobId, job.jobId);
                Assert.AreEqual(expected.startUtc, job.startUtc);
                Assert.AreEqual(expected.endUtc, job.endUtc);
                Assert.AreEqual(expected.paid.Count, job.paid.Count);
                Assert.AreEqual(expected.paid[0].amount, job.paid[0].amount);

                Assert.AreEqual(world.Data.lastSeenUtc, loaded.lastSeenUtc);
                Assert.AreEqual(world.Data.lastLoginUtc, loaded.lastLoginUtc);
                Assert.AreEqual(999, loaded.combatPower);
            }
            finally { world.Cleanup(); }
        }

        [Test]
        public void SaveAndReload_ThenRunningServices_FinishesTheConstruction()
        {
            TestWorld world = TestWorld.Create();
            try
            {
                world.Upgrades.TryStartUpgrade(TestWorld.Farm);
                service.Save(world.Data);

                PlayerData loaded = service.Load(world.Config).Data;
                var reopened = new TestWorld { Config = world.Config, Database = world.Database };
                reopened.Wire(loaded, new FakeClock(TestWorld.StartTime + 31));

                Assert.AreEqual(1, reopened.Upgrades.CompleteFinished());
                Assert.AreEqual(2, reopened.Buildings.GetLevel(TestWorld.Farm));
            }
            finally { world.Cleanup(); }
        }

        // ---------------------------------------------------------------- Sanitize

        [Test]
        public void Sanitize_RepairsBrokenBuildingsAndJobs()
        {
            var data = new PlayerData();
            data.buildings.Add(new BuildingInstanceData { buildingId = "a", level = 99, storedAmount = -5, productionAnchorUtc = 0 });
            data.buildings.Add(new BuildingInstanceData { buildingId = "a", level = 3 });   // duplicate id
            data.buildings.Add(new BuildingInstanceData { buildingId = "", level = 3 });    // no id
            data.buildings.Add(null);
            data.constructionQueue.Add(new ConstructionJobData { buildingId = "a", targetLevel = 2, jobId = "" });
            data.constructionQueue.Add(new ConstructionJobData { buildingId = "a", targetLevel = 3 }); // second job for same building
            data.constructionQueue.Add(new ConstructionJobData { buildingId = "b", targetLevel = 0 }); // impossible target
            data.constructionQueue.Add(null);

            SaveMigration.Sanitize(data, 5_000);

            Assert.AreEqual(1, data.buildings.Count);
            BuildingInstanceData a = data.buildings[0];
            Assert.AreEqual(BuildingDefinition.AbsoluteMaxLevel, a.level);
            Assert.AreEqual(0, a.storedAmount);
            Assert.AreEqual(5_000, a.productionAnchorUtc);
            Assert.AreEqual("a", a.instanceId);

            Assert.AreEqual(1, data.constructionQueue.Count);
            Assert.IsNotEmpty(data.constructionQueue[0].jobId);
            Assert.IsNotNull(data.constructionQueue[0].paid);
        }

        [Test]
        public void Sanitize_OnNullLists_CreatesEmptyOnes()
        {
            var data = new PlayerData { buildings = null, constructionQueue = null };
            SaveMigration.Sanitize(data, 1);
            Assert.IsNotNull(data.buildings);
            Assert.IsNotNull(data.constructionQueue);
        }
    }
}
