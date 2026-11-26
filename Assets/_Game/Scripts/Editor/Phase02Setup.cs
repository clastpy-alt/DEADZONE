using System.Collections.Generic;
using Deadzone.Buildings;
using Deadzone.Data;
using UnityEditor;
using UnityEngine;

namespace Deadzone.EditorTools
{
    /// <summary>Data side of Setup Phase 02. Everything here is idempotent and never deletes or overwrites user-edited assets.</summary>
    internal static class Phase02Setup
    {
        /// <summary>Applies the PHASE 02 test economy to the GameConfig asset, but only if it still holds the PHASE 01 defaults.</summary>
        public static void UpdateGameConfig()
        {
            ProjectSetup.EnsureGameConfig();
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(EditorPaths.GameConfigAsset);

            if (IsPhase01Economy(config.startingResources))
            {
                config.startingResources = GameConfig.DefaultStartingResources();
                Debug.Log("[DEADZONE] GameConfig: PHASE 02 test economy applied (new saves only; existing saves are never changed).");
            }
            if (config.startingBuildings == null || config.startingBuildings.Count == 0)
            {
                config.startingBuildings = GameConfig.DefaultStartingBuildings();
            }
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        private static bool IsPhase01Economy(List<GameConfig.StartingResource> list)
        {
            var old = new Dictionary<ResourceType, long>
            {
                { ResourceType.Food, 5000 }, { ResourceType.Wood, 5000 }, { ResourceType.Steel, 3000 }, { ResourceType.Fuel, 2000 },
                { ResourceType.Gold, 10000 }, { ResourceType.Diamonds, 100 }, { ResourceType.Energy, 100 }
            };
            if (list == null || list.Count != old.Count) return false;
            foreach (GameConfig.StartingResource entry in list)
            {
                if (!old.TryGetValue(entry.type, out long amount) || amount != entry.amount) return false;
            }
            return true;
        }

        public static void GenerateBuildingData()
        {
            int created = BuildingDatabaseGenerator.GenerateDefinitions();
            BuildingDatabaseGenerator.GenerateLevels(false);
            BuildingDatabaseGenerator.LinkPrefabs();
            Debug.Log("[DEADZONE] Building database ready (" + created + " new definition(s); existing ones were kept).");
        }
    }
}
