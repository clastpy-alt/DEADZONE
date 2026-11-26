using System.Collections.Generic;
using Deadzone.Buildings;
using Deadzone.Data;
using Deadzone.SaveSystem;
using UnityEditor;
using UnityEngine;

namespace Deadzone.EditorTools
{
    /// <summary>
    /// Creates the BuildingDefinition assets and the BuildingDatabase from a seed table (the ONLY place balance seeds live;
    /// at runtime everything is read from the assets). Re-running never duplicates or overwrites existing assets;
    /// level rows are only rebuilt when they are missing or when overwrite is requested explicitly.
    /// </summary>
    internal static class BuildingDatabaseGenerator
    {
        private sealed class Seed
        {
            public string Id, FileName, NameKey, DescKey, Label, Color, PrefabFile;
            public BuildingType Type;
            public BuildingCategory Category;
            public bool Functional;
            public int UnlockHq = 1;
            public long Power = 40;
            public ResourceType? Produces;
            public long Production, Storage;
            public bool GlobalStorage;
            public (ResourceType type, long amount)[] Cost = { (ResourceType.Food, 300), (ResourceType.Wood, 300), (ResourceType.Steel, 200) };
        }

        private static readonly (ResourceType, long)[] CostMilitary = { (ResourceType.Food, 250), (ResourceType.Wood, 250), (ResourceType.Steel, 150) };

        private static readonly Seed[] Seeds =
        {
            new Seed { Id = "headquarters", FileName = "Headquarters", NameKey = "building.hq", DescKey = "building.hq.desc", Label = "HQ", Color = "#465744", PrefabFile = "Headquarters",
                Type = BuildingType.Headquarters, Category = BuildingCategory.Military, Functional = true, Power = 120,
                Cost = new[] { (ResourceType.Steel, 400L), (ResourceType.Wood, 300L), (ResourceType.Gold, 80L) } },
            new Seed { Id = "barracks", FileName = "Barracks", NameKey = "building.barracks", DescKey = "building.barracks.desc", Label = "BR", Color = "#3F4F3D", PrefabFile = "Barracks",
                Type = BuildingType.Barracks, Category = BuildingCategory.Military, Functional = true, Power = 60, Cost = CostMilitary },
            new Seed { Id = "tank_factory", FileName = "TankFactory", NameKey = "building.tank_factory", DescKey = "building.tank_factory.desc", Label = "TF", Color = "#4A5148",
                Type = BuildingType.TankFactory, Category = BuildingCategory.Military, UnlockHq = 3, Power = 80 },
            new Seed { Id = "air_base", FileName = "AirBase", NameKey = "building.air_base", DescKey = "building.air_base.desc", Label = "AB", Color = "#56606B",
                Type = BuildingType.AirBase, Category = BuildingCategory.Military, UnlockHq = 5, Power = 90 },
            new Seed { Id = "hospital", FileName = "Hospital", NameKey = "building.hospital", DescKey = "building.hospital.desc", Label = "HS", Color = "#8A9199", PrefabFile = "Hospital",
                Type = BuildingType.MilitaryHospital, Category = BuildingCategory.Military, Functional = true, Power = 50,
                Cost = new[] { (ResourceType.Food, 200L), (ResourceType.Wood, 150L), (ResourceType.Steel, 150L) } },
            new Seed { Id = "training_ground", FileName = "TrainingGround", NameKey = "building.training_ground", DescKey = "building.training_ground.desc", Label = "TG", Color = "#5B6B4B",
                Type = BuildingType.TrainingGround, Category = BuildingCategory.Military, UnlockHq = 2, Power = 45 },

            new Seed { Id = "food_farm", FileName = "FoodFarm", NameKey = "building.farm", DescKey = "building.farm.desc", Label = "FD", Color = "#6B5B3E", PrefabFile = "FoodFarm",
                Type = BuildingType.FoodFarm, Category = BuildingCategory.Economy, Functional = true, Power = 20,
                Produces = ResourceType.Food, Production = 1200, Storage = 4000,
                Cost = new[] { (ResourceType.Wood, 250L), (ResourceType.Steel, 100L) } },
            new Seed { Id = "lumber_mill", FileName = "LumberMill", NameKey = "building.lumber_mill", DescKey = "building.lumber_mill.desc", Label = "WD", Color = "#7A5A36",
                Type = BuildingType.LumberMill, Category = BuildingCategory.Economy, UnlockHq = 2, Power = 20, Produces = ResourceType.Wood, Production = 1200, Storage = 4000 },
            new Seed { Id = "steel_factory", FileName = "SteelFactory", NameKey = "building.steel", DescKey = "building.steel.desc", Label = "ST", Color = "#5A5F66", PrefabFile = "SteelFactory",
                Type = BuildingType.SteelFactory, Category = BuildingCategory.Economy, Functional = true, Power = 25,
                Produces = ResourceType.Steel, Production = 900, Storage = 3000,
                Cost = new[] { (ResourceType.Food, 200L), (ResourceType.Wood, 250L), (ResourceType.Gold, 40L) } },
            new Seed { Id = "oil_refinery", FileName = "OilRefinery", NameKey = "building.oil_refinery", DescKey = "building.oil_refinery.desc", Label = "FU", Color = "#7C6A2E",
                Type = BuildingType.OilRefinery, Category = BuildingCategory.Economy, UnlockHq = 3, Power = 25, Produces = ResourceType.Fuel, Production = 500, Storage = 2000 },
            new Seed { Id = "gold_mine", FileName = "GoldMine", NameKey = "building.gold_mine", DescKey = "building.gold_mine.desc", Label = "GD", Color = "#8A7A3A",
                Type = BuildingType.GoldMine, Category = BuildingCategory.Economy, UnlockHq = 3, Power = 25, Produces = ResourceType.Gold, Production = 300, Storage = 1500 },
            new Seed { Id = "power_plant", FileName = "PowerPlant", NameKey = "building.power_plant", DescKey = "building.power_plant.desc", Label = "PW", Color = "#3E6A8A",
                Type = BuildingType.PowerPlant, Category = BuildingCategory.Economy, UnlockHq = 2, Power = 30, Produces = ResourceType.Energy, Production = 10, Storage = 100 },
            new Seed { Id = "warehouse", FileName = "Warehouse", NameKey = "building.warehouse", DescKey = "building.warehouse.desc", Label = "WH", Color = "#59626B",
                Type = BuildingType.Warehouse, Category = BuildingCategory.Economy, UnlockHq = 2, Power = 15, Storage = 5000, GlobalStorage = true },

            new Seed { Id = "research_lab", FileName = "ResearchLab", NameKey = "building.lab", DescKey = "building.lab.desc", Label = "LB", Color = "#4E5E6E", PrefabFile = "ResearchLab",
                Type = BuildingType.ResearchLaboratory, Category = BuildingCategory.Technology, Functional = true, Power = 60,
                Cost = new[] { (ResourceType.Steel, 250L), (ResourceType.Wood, 150L), (ResourceType.Gold, 100L) } },
            new Seed { Id = "radar_station", FileName = "RadarStation", NameKey = "building.radar_station", DescKey = "building.radar_station.desc", Label = "RD", Color = "#4F6B6B",
                Type = BuildingType.RadarStation, Category = BuildingCategory.Technology, UnlockHq = 4, Power = 35 },
            new Seed { Id = "intelligence_center", FileName = "IntelligenceCenter", NameKey = "building.intelligence_center", DescKey = "building.intelligence_center.desc", Label = "IC", Color = "#5A4F6B",
                Type = BuildingType.IntelligenceCenter, Category = BuildingCategory.Technology, UnlockHq = 5, Power = 35 },

            new Seed { Id = "alliance_center", FileName = "AllianceCenter", NameKey = "building.alliance_center", DescKey = "building.alliance_center.desc", Label = "AL", Color = "#3E5E7A",
                Type = BuildingType.AllianceCenter, Category = BuildingCategory.Social, UnlockHq = 4, Power = 30 },
            new Seed { Id = "recruitment_center", FileName = "RecruitmentCenter", NameKey = "building.recruitment_center", DescKey = "building.recruitment_center.desc", Label = "RC", Color = "#6B4F4F",
                Type = BuildingType.RecruitmentCenter, Category = BuildingCategory.Social, UnlockHq = 3, Power = 30 },
            new Seed { Id = "trading_center", FileName = "TradingCenter", NameKey = "building.trading_center", DescKey = "building.trading_center.desc", Label = "TC", Color = "#7A6A3E",
                Type = BuildingType.TradingCenter, Category = BuildingCategory.Social, UnlockHq = 6, Power = 30 },

            new Seed { Id = "watchtower", FileName = "Watchtower", NameKey = "building.watchtower", DescKey = "building.watchtower.desc", Label = "WT", Color = "#505A4A",
                Type = BuildingType.Watchtower, Category = BuildingCategory.Defense, UnlockHq = 2, Power = 30 },
            new Seed { Id = "defense_turret", FileName = "DefenseTurret", NameKey = "building.defense_turret", DescKey = "building.defense_turret.desc", Label = "DT", Color = "#5E4A4A",
                Type = BuildingType.DefenseTurret, Category = BuildingCategory.Defense, UnlockHq = 3, Power = 40 },
            new Seed { Id = "defensive_wall", FileName = "DefensiveWall", NameKey = "building.defensive_wall", DescKey = "building.defensive_wall.desc", Label = "DW", Color = "#6B6B66",
                Type = BuildingType.DefensiveWall, Category = BuildingCategory.Defense, UnlockHq = 2, Power = 25 }
        };

        public static string AssetPath(string fileName) => EditorPaths.BuildingData + "/Building_" + fileName + ".asset";

        /// <summary>Creates missing definition assets and registers them in the database. Returns the number of NEW assets.</summary>
        public static int GenerateDefinitions()
        {
            EditorPaths.EnsureFolder(EditorPaths.BuildingData);
            EditorPaths.EnsureFolder(EditorPaths.Resources);

            var database = AssetDatabase.LoadAssetAtPath<BuildingDatabase>(EditorPaths.BuildingDatabaseAsset);
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<BuildingDatabase>();
                AssetDatabase.CreateAsset(database, EditorPaths.BuildingDatabaseAsset);
            }

            int created = 0;
            foreach (Seed seed in Seeds)
            {
                string path = AssetPath(seed.FileName);
                var definition = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(path);
                if (definition == null)
                {
                    definition = ScriptableObject.CreateInstance<BuildingDefinition>();
                    Fill(definition, seed);
                    definition.levels = definition.balance.BuildLevels(definition.maxLevel, "headquarters", definition.IsHeadquarters);
                    AssetDatabase.CreateAsset(definition, path);
                    created++;
                }
                if (!database.definitions.Contains(definition)) database.definitions.Add(definition);
            }

            database.Rebuild();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            return created;
        }

        /// <summary>Rebuilds level rows from each definition's balance curve (only missing/incomplete rows unless overwrite).</summary>
        public static int GenerateLevels(bool overwrite)
        {
            var database = AssetDatabase.LoadAssetAtPath<BuildingDatabase>(EditorPaths.BuildingDatabaseAsset);
            if (database == null) return 0;

            int rebuilt = 0;
            foreach (BuildingDefinition definition in database.definitions)
            {
                if (definition == null) continue;
                if (!overwrite && definition.levels.Count == definition.maxLevel) continue;

                definition.levels = definition.balance.BuildLevels(definition.maxLevel, "headquarters", definition.IsHeadquarters);
                EditorUtility.SetDirty(definition);
                rebuilt++;
            }
            AssetDatabase.SaveAssets();
            return rebuilt;
        }

        /// <summary>Links prefab references that are still empty (prefabs are generated separately).</summary>
        public static void LinkPrefabs()
        {
            foreach (Seed seed in Seeds)
            {
                if (string.IsNullOrEmpty(seed.PrefabFile)) continue;
                var definition = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(AssetPath(seed.FileName));
                if (definition == null || definition.prefab != null) continue;

                definition.prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EditorPaths.PrefabsBuildings + "/" + seed.PrefabFile + ".prefab");
                EditorUtility.SetDirty(definition);
            }
            AssetDatabase.SaveAssets();
        }

        private static void Fill(BuildingDefinition definition, Seed seed)
        {
            definition.buildingId = seed.Id;
            definition.type = seed.Type;
            definition.category = seed.Category;
            definition.availability = seed.Functional ? BuildingAvailability.Functional : BuildingAvailability.Planned;
            definition.nameKey = seed.NameKey;
            definition.descriptionKey = seed.DescKey;
            definition.iconLabel = seed.Label;
            if (ColorUtility.TryParseHtmlString(seed.Color, out Color color)) definition.accentColor = color;
            definition.maxLevel = BuildingDefinition.AbsoluteMaxLevel;
            definition.unlockHeadquartersLevel = seed.UnlockHq;

            definition.production = new BuildingProductionData
            {
                producesResource = seed.Produces.HasValue,
                resource = seed.Produces ?? ResourceType.Food,
                storageIsGlobal = seed.GlobalStorage
            };

            var curve = new BuildingBalanceCurve { powerBase = seed.Power, productionBase = seed.Production, storageBase = seed.Storage };
            var costs = new List<ResourceAmount>();
            foreach ((ResourceType type, long amount) cost in seed.Cost) costs.Add(new ResourceAmount { type = cost.type, amount = cost.amount });
            curve.baseCost = costs;
            definition.balance = curve;
        }
    }
}
