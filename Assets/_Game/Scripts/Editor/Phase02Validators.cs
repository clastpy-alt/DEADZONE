using System.Collections.Generic;
using System.Text;
using Deadzone.Buildings;
using Deadzone.Data;
using Deadzone.SaveSystem;
using UnityEditor;
using UnityEngine;

namespace Deadzone.EditorTools
{
    /// <summary>Validate Buildings / Economy / Save Migration. Each returns one message per problem; empty = OK.</summary>
    internal static class Phase02Validators
    {
        public static List<string> ValidateBuildings()
        {
            var database = AssetDatabase.LoadAssetAtPath<BuildingDatabase>(EditorPaths.BuildingDatabaseAsset);
            if (database == null) return new List<string> { "BuildingDatabase asset missing (Tools > DEADZONE > Generate Building Database)" };

            List<string> problems = database.Validate();
            if (database.definitions.Count != 22) problems.Add("Expected 22 building definitions, found " + database.definitions.Count);

            int functional = 0;
            foreach (BuildingDefinition definition in database.definitions)
            {
                if (definition == null) continue;
                if (definition.IsFunctional) functional++;
                if (definition.IsFunctional && definition.prefab == null) problems.Add(definition.buildingId + ": functional building has no prefab");
            }
            if (functional != 7) problems.Add("Expected 7 functional buildings (6 PHASE 02 + Warehouse), found " + functional);
            return problems;
        }

        public static List<string> ValidateEconomy()
        {
            var problems = new List<string>();
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(EditorPaths.GameConfigAsset);
            if (config == null) { problems.Add("GameConfig asset missing"); return problems; }

            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                if (config.GetCapacity(type) <= 0) problems.Add("Capacity for " + type + " must be > 0");
            }
            foreach (GameConfig.StartingResource start in config.startingResources)
            {
                if (start.amount < 0) problems.Add("Starting amount of " + start.type + " is negative");
                if (start.amount > config.GetCapacity(start.type)) problems.Add("Starting amount of " + start.type + " exceeds its capacity");
            }
            if (config.maxOfflineHours <= 0f) problems.Add("maxOfflineHours must be > 0");
            if (config.constructionQueueSlots < 1) problems.Add("constructionQueueSlots must be >= 1");
            if (config.startingBuildings.Count == 0) problems.Add("startingBuildings is empty");

            var database = AssetDatabase.LoadAssetAtPath<BuildingDatabase>(EditorPaths.BuildingDatabaseAsset);
            if (database == null) { problems.Add("BuildingDatabase missing"); return problems; }

            foreach (GameConfig.StartingBuilding start in config.startingBuildings)
            {
                if (database.Get(start.buildingId) == null) problems.Add("Starting building '" + start.buildingId + "' is not in the database");
            }

            // Every functional upgrade must stay payable: its cost may not exceed the capacity reachable with the Warehouse one level behind.
            BuildingDefinition warehouseDef = database.definitions.Find(d => d != null && d.type == BuildingType.Warehouse);
            if (warehouseDef != null && warehouseDef.IsFunctional)
            {
                for (int level = 2; level <= warehouseDef.maxLevel; level++)
                {
                    BuildingLevelData row = warehouseDef.GetLevel(level);
                    long capacity = config.defaultResourceCapacity + (warehouseDef.GetLevel(level - 1)?.storageCapacity ?? 0);
                    foreach (var cost in row.upgradeCost)
                    {
                        if (cost.amount > capacity) problems.Add("Warehouse L" + level + ": " + cost.type + " cost " + cost.amount + " exceeds the capacity " + capacity + " it can have at that point");
                    }
                }
            }

            foreach (BuildingDefinition definition in database.definitions)
            {
                if (definition == null || !definition.production.producesResource || definition.production.storageIsGlobal) continue;
                BuildingLevelData first = definition.GetLevel(1);
                if (first == null || first.productionPerHour <= 0) problems.Add(definition.buildingId + ": production per hour at level 1 must be > 0");
            }
            return problems;
        }

        /// <summary>Builds a synthetic version 1 save and checks that migration keeps everything and adds the new data once.</summary>
        public static List<string> ValidateSaveMigration()
        {
            var problems = new List<string>();
            var v1 = new PlayerData { playerId = "abc", playerName = "Test", commanderLevel = 7, combatPower = 1234 };
            v1.resources.Add(new ResourceAmount { type = ResourceType.Gold, amount = 777 });
            v1.buildings = null;
            v1.constructionQueue = null;

            SaveMigration.Migrate(v1, 1, 1000);
            SaveMigration.Sanitize(v1, 1000);

            if (v1.playerName != "Test" || v1.commanderLevel != 7 || v1.combatPower != 1234) problems.Add("Profile data changed by migration");
            if (v1.resources.Count != 1 || v1.resources[0].amount != 777) problems.Add("Resources changed by migration");
            if (v1.buildings.Count != GameConfig.DefaultStartingBuildings().Count) problems.Add("Migration did not add the starting buildings");
            if (v1.constructionQueue.Count != 0) problems.Add("Migration invented a construction job");

            int before = v1.buildings.Count;
            SaveMigration.Migrate(v1, 1, 2000);
            if (v1.buildings.Count != before) problems.Add("Running migration twice duplicated buildings");
            return problems;
        }

        public static void Report(string title, List<string> problems, bool showDialog)
        {
            if (problems.Count == 0)
            {
                Debug.Log("[DEADZONE] " + title + ": OK");
                if (showDialog) EditorUtility.DisplayDialog(title, "No problems found.", "OK");
                return;
            }

            var text = new StringBuilder();
            foreach (string problem in problems) text.AppendLine(" - " + problem);
            Debug.LogError("[DEADZONE] " + title + ": " + problems.Count + " problem(s)\n" + text);
            if (showDialog) EditorUtility.DisplayDialog(title, problems.Count + " problem(s). Details are in the Console.", "OK");
        }
    }
}
