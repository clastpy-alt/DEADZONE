using System;
using System.IO;
using System.Text.RegularExpressions;
using Deadzone.Data;
using Deadzone.SaveSystem;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Deadzone.Tests
{
    public sealed class SaveServiceTests
    {
        private GameConfig config;
        private string directory;
        private SaveService service;

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            directory = Path.Combine(Path.GetTempPath(), "deadzone-tests-" + Guid.NewGuid().ToString("N"));
            service = new SaveService(directory);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(config);
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        [Test]
        public void Load_WithoutFile_CreatesNewPlayer()
        {
            SaveService.LoadResult result = service.Load(config);
            Assert.AreEqual(SaveService.LoadOutcome.CreatedNew, result.Outcome);
            Assert.IsNotNull(result.Data);
            Assert.IsNotEmpty(result.Data.playerId);
        }

        [Test]
        public void SaveThenLoad_RoundTripsData()
        {
            PlayerData data = PlayerData.CreateNew(config);
            data.playerName = "Alpha";
            data.commanderLevel = 7;
            data.resources.Find(r => r.type == ResourceType.Food).amount = 1234;
            service.Save(data);

            SaveService.LoadResult result = new SaveService(directory).Load(config);
            Assert.AreEqual(SaveService.LoadOutcome.Loaded, result.Outcome);
            Assert.AreEqual("Alpha", result.Data.playerName);
            Assert.AreEqual(7, result.Data.commanderLevel);
            Assert.AreEqual(1234, result.Data.resources.Find(r => r.type == ResourceType.Food).amount);
            Assert.AreEqual(data.playerId, result.Data.playerId);
        }

        [Test]
        public void CorruptedMain_RecoversFromBackup_AndKeepsCorruptFile()
        {
            PlayerData data = PlayerData.CreateNew(config);
            data.playerName = "First";
            service.Save(data);
            data.playerName = "Second";
            service.Save(data); // main = Second, backup = First

            File.WriteAllText(service.MainPath, "{ this is not valid json");
            LogAssert.Expect(LogType.Warning, new Regex(@"\[Save\] Could not read"));

            SaveService.LoadResult result = service.Load(config);
            Assert.AreEqual(SaveService.LoadOutcome.RecoveredFromBackup, result.Outcome);
            Assert.AreEqual("First", result.Data.playerName);
            Assert.AreEqual(1, Directory.GetFiles(directory, "save.corrupt-*").Length);
        }

        [Test]
        public void TamperedPayload_FailsChecksum()
        {
            service.Save(PlayerData.CreateNew(config));
            string json = File.ReadAllText(service.MainPath);
            Assert.IsTrue(json.Contains("Commander"), "test precondition: default name present in save");
            File.WriteAllText(service.MainPath, json.Replace("Commander", "Hacked!!"));
            LogAssert.Expect(LogType.Warning, new Regex(@"checksum mismatch"));

            SaveService.LoadResult result = service.Load(config);
            Assert.AreEqual(SaveService.LoadOutcome.ResetAfterCorruption, result.Outcome);
        }

        [Test]
        public void FutureVersion_IsNotLoaded_ButPreserved()
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(service.MainPath, "{\"version\":999,\"checksum\":\"x\",\"payload\":\"{}\"}");
            LogAssert.Expect(LogType.Warning, new Regex(@"unsupported save version"));

            SaveService.LoadResult result = service.Load(config);
            Assert.AreEqual(SaveService.LoadOutcome.ResetAfterCorruption, result.Outcome);
            Assert.AreEqual(1, Directory.GetFiles(directory, "save.corrupt-*").Length);
        }

        [Test]
        public void NegativeValues_AreSanitizedOnLoad()
        {
            PlayerData data = PlayerData.CreateNew(config);
            data.commanderLevel = 0;
            data.resources.Find(r => r.type == ResourceType.Gold).amount = -50;
            service.Save(data);

            PlayerData loaded = service.Load(config).Data;
            Assert.AreEqual(1, loaded.commanderLevel);
            Assert.AreEqual(0, loaded.resources.Find(r => r.type == ResourceType.Gold).amount);
        }
    }
}
