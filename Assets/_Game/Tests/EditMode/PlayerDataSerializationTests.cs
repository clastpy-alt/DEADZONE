using Deadzone.Data;
using Deadzone.SaveSystem;
using NUnit.Framework;
using UnityEngine;

namespace Deadzone.Tests
{
    public sealed class PlayerDataSerializationTests
    {
        private GameConfig config;

        [SetUp]
        public void SetUp() => config = ScriptableObject.CreateInstance<GameConfig>();

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(config);

        [Test]
        public void RoundTrip_KeepsAllFields()
        {
            PlayerData original = PlayerData.CreateNew(config);
            original.playerName = "Test Commander";
            original.commanderLevel = 7;
            original.commanderExperience = 12345;
            original.vipLevel = 3;
            original.combatPower = 987654321012L;
            original.language = Language.English;

            PlayerData copy = JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(original));

            Assert.AreEqual(original.playerId, copy.playerId);
            Assert.AreEqual("Test Commander", copy.playerName);
            Assert.AreEqual(7, copy.commanderLevel);
            Assert.AreEqual(12345L, copy.commanderExperience);
            Assert.AreEqual(3, copy.vipLevel);
            Assert.AreEqual(987654321012L, copy.combatPower, "long values above int range must survive");
            Assert.AreEqual(Language.English, copy.language);
            Assert.AreEqual(original.createdUtc, copy.createdUtc);
        }

        [Test]
        public void RoundTrip_KeepsResources()
        {
            PlayerData original = PlayerData.CreateNew(config);
            original.resources.Clear();
            original.resources.Add(new ResourceAmount { type = ResourceType.Steel, amount = 4200 });
            original.resources.Add(new ResourceAmount { type = ResourceType.Diamonds, amount = 55 });

            PlayerData copy = JsonUtility.FromJson<PlayerData>(JsonUtility.ToJson(original));

            Assert.AreEqual(2, copy.resources.Count);
            Assert.AreEqual(ResourceType.Steel, copy.resources[0].type);
            Assert.AreEqual(4200L, copy.resources[0].amount);
            Assert.AreEqual(ResourceType.Diamonds, copy.resources[1].type);
            Assert.AreEqual(55L, copy.resources[1].amount);
        }

        [Test]
        public void CreateNew_ProducesUniqueIdsAndNonNegativeStartingResources()
        {
            PlayerData a = PlayerData.CreateNew(config);
            PlayerData b = PlayerData.CreateNew(config);

            Assert.IsFalse(string.IsNullOrEmpty(a.playerId));
            Assert.AreNotEqual(a.playerId, b.playerId);
            foreach (ResourceAmount resource in a.resources) Assert.GreaterOrEqual(resource.amount, 0L);
        }

        [Test]
        public void ResourceType_NumericValuesAreStable()
        {
            // These numbers are written to save files. Changing them breaks existing saves.
            Assert.AreEqual(0, (int)ResourceType.Food);
            Assert.AreEqual(1, (int)ResourceType.Wood);
            Assert.AreEqual(2, (int)ResourceType.Steel);
            Assert.AreEqual(3, (int)ResourceType.Fuel);
            Assert.AreEqual(4, (int)ResourceType.Gold);
            Assert.AreEqual(5, (int)ResourceType.Diamonds);
            Assert.AreEqual(6, (int)ResourceType.Energy);
            Assert.AreEqual(7, (int)ResourceType.AllianceCredits);
            Assert.AreEqual(8, (int)ResourceType.VipPoints);
        }
    }
}
