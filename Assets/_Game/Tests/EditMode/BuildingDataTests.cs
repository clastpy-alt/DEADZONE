using System.Collections.Generic;
using Deadzone.Buildings;
using Deadzone.Data;
using Deadzone.SaveSystem;
using NUnit.Framework;
using UnityEngine;

namespace Deadzone.Tests
{
    /// <summary>Building Data Validation: the shipped database (Resources/BuildingDatabase) and the rules the validator enforces.</summary>
    public sealed class BuildingDataTests
    {
        private static readonly BuildingType[] FunctionalTypes =
        {
            BuildingType.Headquarters, BuildingType.Barracks, BuildingType.MilitaryHospital,
            BuildingType.ResearchLaboratory, BuildingType.FoodFarm, BuildingType.SteelFactory
        };

        private BuildingDatabase database;

        [SetUp]
        public void SetUp()
        {
            database = BuildingDatabase.Load();
        }

        [Test]
        public void ShippedDatabase_HasNoValidationProblems()
        {
            List<string> problems = database.Validate();
            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        [Test]
        public void ShippedDatabase_ContainsAll22BuildingTypes()
        {
            Assert.AreEqual(22, database.definitions.Count);
            foreach (BuildingType type in System.Enum.GetValues(typeof(BuildingType)))
            {
                Assert.IsNotNull(database.definitions.Find(d => d.type == type), "missing building type " + type);
            }
        }

        [Test]
        public void ShippedDatabase_OnlySixBuildingsAreFunctional()
        {
            foreach (BuildingDefinition definition in database.definitions)
            {
                bool expected = System.Array.IndexOf(FunctionalTypes, definition.type) >= 0;
                Assert.AreEqual(expected, definition.IsFunctional, definition.buildingId + " availability");
            }
        }

        [Test]
        public void ShippedDatabase_EveryBuildingSupports35Levels()
        {
            foreach (BuildingDefinition definition in database.definitions)
            {
                Assert.AreEqual(35, definition.maxLevel, definition.buildingId);
                Assert.AreEqual(35, definition.levels.Count, definition.buildingId);
            }
        }

        [Test]
        public void ShippedDatabase_HeadquartersLimitsOtherBuildings()
        {
            foreach (BuildingDefinition definition in database.definitions)
            {
                if (definition.IsHeadquarters) continue;
                BuildingLevelData level5 = definition.GetLevel(5);
                Assert.IsTrue(level5.requirements.Exists(r => r.buildingId == database.GetHeadquarters().buildingId && r.level >= 1),
                    definition.buildingId + " level 5 has no HQ requirement");
            }
        }

        [Test]
        public void ShippedDatabase_EarlyUpgradeTimesFollowTestTable()
        {
            BuildingDefinition hq = database.GetHeadquarters();
            Assert.AreEqual(30, hq.GetLevel(2).upgradeDurationSeconds, "1->2");
            Assert.AreEqual(120, hq.GetLevel(3).upgradeDurationSeconds, "2->3");
            Assert.AreEqual(300, hq.GetLevel(4).upgradeDurationSeconds, "3->4");
            Assert.AreEqual(900, hq.GetLevel(5).upgradeDurationSeconds, "4->5");
        }

        [Test]
        public void GetLevel_OutOfRange_ReturnsNull()
        {
            BuildingDefinition hq = database.GetHeadquarters();
            Assert.IsNull(hq.GetLevel(0));
            Assert.IsNull(hq.GetLevel(36));
        }

        [Test]
        public void Validator_ReportsDuplicateIdsWrongLevelCountAndMissingHq()
        {
            var broken = ScriptableObject.CreateInstance<BuildingDatabase>();
            var a = ScriptableObject.CreateInstance<BuildingDefinition>();
            var b = ScriptableObject.CreateInstance<BuildingDefinition>();
            try
            {
                a.buildingId = "dup"; a.type = BuildingType.Barracks; a.maxLevel = 5; // zero level rows
                b.buildingId = "dup"; b.type = BuildingType.Barracks; b.maxLevel = 5;
                broken.definitions.Add(a);
                broken.definitions.Add(b);

                string all = string.Join("\n", broken.Validate());
                StringAssert.Contains("duplicate buildingId", all);
                StringAssert.Contains("duplicate building type", all);
                StringAssert.Contains("level rows", all);
                StringAssert.Contains("No Headquarters", all);
            }
            finally
            {
                Object.DestroyImmediate(broken);
                Object.DestroyImmediate(a);
                Object.DestroyImmediate(b);
            }
        }

        [Test]
        public void BalanceCurve_BuildsMonotonicRowsWithHqRequirement()
        {
            var curve = new BuildingBalanceCurve
            {
                baseCost = new List<ResourceAmount> { new ResourceAmount { type = ResourceType.Steel, amount = 200 } },
                productionBase = 100
            };
            List<BuildingLevelData> rows = curve.BuildLevels(35, "hq", false);

            Assert.AreEqual(35, rows.Count);
            Assert.IsEmpty(rows[0].upgradeCost, "level 1 is free (starting level)");
            for (int i = 1; i < rows.Count; i++)
            {
                Assert.AreEqual(i + 1, rows[i].level);
                Assert.Greater(rows[i].upgradeCost[0].amount, 0);
                Assert.GreaterOrEqual(rows[i].upgradeCost[0].amount, rows[i - 1].upgradeCost.Count == 0 ? 0 : rows[i - 1].upgradeCost[0].amount);
                Assert.GreaterOrEqual(rows[i].upgradeDurationSeconds, i > 1 ? rows[i - 1].upgradeDurationSeconds : 1);
                Assert.GreaterOrEqual(rows[i].powerContribution, rows[i - 1].powerContribution);
                Assert.GreaterOrEqual(rows[i].productionPerHour, rows[i - 1].productionPerHour);
                Assert.AreEqual("hq", rows[i].requirements[0].buildingId);
                Assert.AreEqual(System.Math.Max(1, rows[i].level - curve.hqLevelOffset), rows[i].requirements[0].level);
            }
        }

        [Test]
        public void BalanceCurve_Headquarters_HasNoSelfRequirement()
        {
            var curve = new BuildingBalanceCurve { baseCost = new List<ResourceAmount> { new ResourceAmount { type = ResourceType.Food, amount = 10 } } };
            foreach (BuildingLevelData row in curve.BuildLevels(10, "hq", true)) Assert.IsEmpty(row.requirements);
        }
    }
}
