using System.IO;
using Deadzone.Buildings;
using Deadzone.Core;
using Deadzone.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Deadzone.EditorTools
{
    /// <summary>
    /// Data/scene upgrades of PHASE 02.5 that must work on a project that already ran Setup Phase 02.
    /// Every step is idempotent and only touches values that still have their previous generated defaults.
    /// </summary>
    internal static class Phase025Setup
    {
        public static readonly Vector3 WarehousePosition = new Vector3(0f, 0f, -12f);

        /// <summary>Makes the Warehouse a functional starting building with a curve that keeps up with the upgrade costs.</summary>
        public static string PatchWarehouseData()
        {
            var report = new System.Text.StringBuilder();

            BuildingPrefabGenerator.GenerateAll(false);
            BuildingDatabaseGenerator.GenerateDefinitions();
            BuildingDatabaseGenerator.LinkPrefabs();

            var warehouse = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(BuildingDatabaseGenerator.AssetPath("Warehouse"));
            if (warehouse != null && warehouse.availability == BuildingAvailability.Planned && warehouse.balance.storageBase == 5000)
            {
                warehouse.availability = BuildingAvailability.Functional;
                warehouse.balance.storageBase = 20000;
                warehouse.balance.storageGrowth = 1.42f;
                warehouse.levels = warehouse.balance.BuildLevels(warehouse.maxLevel, "headquarters", false);
                EditorUtility.SetDirty(warehouse);
                report.AppendLine("Warehouse is now functional (storage 20000 x 1.42^(L-1))");
            }

            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(EditorPaths.GameConfigAsset);
            if (config != null && !config.startingBuildings.Exists(b => b.buildingId == "warehouse"))
            {
                config.startingBuildings.Add(new GameConfig.StartingBuilding("warehouse", 1));
                EditorUtility.SetDirty(config);
                report.AppendLine("GameConfig: warehouse added to the starting base");
            }

            AssetDatabase.SaveAssets();
            return report.Length == 0 ? "Warehouse data already up to date." : report.ToString().Trim();
        }

        /// <summary>Places the Warehouse in the Base scene when it is missing (never moves or deletes anything).</summary>
        public static string PatchWarehouseScene()
        {
            string path = EditorPaths.ScenePath(SceneNames.Base);
            if (!File.Exists(path)) return "Base scene not found.";

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EditorPaths.PrefabsBuildings + "/Warehouse.prefab");
            if (prefab == null) return "Warehouse prefab missing.";

            Scene previousActive = SceneManager.GetActiveScene();
            Scene existing = SceneManager.GetSceneByPath(path);
            bool wasLoaded = existing.IsValid() && existing.isLoaded;
            Scene scene = wasLoaded ? existing : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    foreach (BuildingSelectable selectable in root.GetComponentsInChildren<BuildingSelectable>(true))
                    {
                        if (selectable.BuildingId == "warehouse") return "Warehouse already in the Base scene.";
                    }
                }

                Transform parent = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (root.name == "Buildings [PLACEHOLDER]") parent = root.transform;
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                if (parent != null) instance.transform.SetParent(parent, true);
                instance.transform.position = WarehousePosition;
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                return "Warehouse placed in the Base scene.";
            }
            finally
            {
                if (previousActive.IsValid() && previousActive.isLoaded) SceneManager.SetActiveScene(previousActive);
                if (!wasLoaded) EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
