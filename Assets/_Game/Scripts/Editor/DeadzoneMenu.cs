using Deadzone.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Deadzone.EditorTools
{
    /// <summary>Tools > DEADZONE menu. Every tool is safe to re-run: existing assets and scenes are kept.</summary>
    internal static class DeadzoneMenu
    {
        [MenuItem("Tools/DEADZONE/Setup Phase 01 (Run This First)", priority = 0)]
        private static void SetupPhase01()
        {
            if (!ProjectSetup.CheckTextMeshPro()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            ProjectSetup.ApplyProjectSettings();
            ProjectSetup.EnsureGameConfig();
            UIPrefabGenerator.GenerateAll(false);
            BuildingPrefabGenerator.GenerateAll(false);
            SceneGenerator.CreateAll(false);
            SceneGenerator.RegisterBuildScenes();
            AssetDatabase.SaveAssets();

            EditorSceneManager.OpenScene(EditorPaths.ScenePath(SceneNames.Base));
            ProjectValidator.Run(true);
        }

        [MenuItem("Tools/DEADZONE/Validate Project", priority = 20)]
        private static void ValidateProject() => ProjectValidator.Run(true);

        [MenuItem("Tools/DEADZONE/Validate References", priority = 21)]
        private static void ValidateReferences()
        {
            var problems = ReferenceValidator.Validate();
            if (problems.Count == 0)
            {
                Debug.Log("[DEADZONE] Reference validation passed: all serialized references in UI and building prefabs are assigned.");
                EditorUtility.DisplayDialog("Validate References", "No problems found.", "OK");
                return;
            }

            Debug.LogError("[DEADZONE] Reference validation found " + problems.Count + " problem(s):\n" + string.Join("\n", problems));
            EditorUtility.DisplayDialog("Validate References", problems.Count + " problem(s). Details are in the Console.", "OK");
        }

        [MenuItem("Tools/DEADZONE/Generate Scenes", priority = 40)]
        private static void GenerateScenes()
        {
            if (!ProjectSetup.CheckTextMeshPro()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            // Scenes need the prefabs; generating them first is harmless because existing ones are kept.
            UIPrefabGenerator.GenerateAll(false);
            BuildingPrefabGenerator.GenerateAll(false);
            SceneGenerator.CreateAll(false);
            SceneGenerator.RegisterBuildScenes();
            Debug.Log("[DEADZONE] Scenes ready (existing scenes were kept).");
        }

        [MenuItem("Tools/DEADZONE/Generate UI Prefabs", priority = 41)]
        private static void GenerateUiPrefabs()
        {
            if (!ProjectSetup.CheckTextMeshPro()) return;
            UIPrefabGenerator.GenerateAll(false);
            Debug.Log("[DEADZONE] UI prefabs ready (existing ones were kept).");
        }

        [MenuItem("Tools/DEADZONE/Generate Building Prefabs", priority = 42)]
        private static void GenerateBuildingPrefabs()
        {
            BuildingPrefabGenerator.GenerateAll(false);
            Debug.Log("[DEADZONE] Building prefabs ready (existing ones were kept).");
        }

        [MenuItem("Tools/DEADZONE/Generate Game Data", priority = 43)]
        private static void GenerateGameData()
        {
            ProjectSetup.EnsureGameConfig();
            Debug.Log("[DEADZONE] GameConfig asset ready at " + EditorPaths.GameConfigAsset + " (existing values were kept).");
        }

        [MenuItem("Tools/DEADZONE/Recreate Base Scene (Overwrite)", priority = 60)]
        private static void RecreateBaseScene()
        {
            if (!ProjectSetup.CheckTextMeshPro()) return;

            if (System.IO.File.Exists(EditorPaths.ScenePath(SceneNames.Base)) &&
                !EditorUtility.DisplayDialog("Base scene exists",
                    "Overwrite the Base scene? Manual changes in it will be lost.", "Overwrite", "Keep"))
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            UIPrefabGenerator.GenerateAll(false);
            BuildingPrefabGenerator.GenerateAll(false);
            SceneGenerator.CreateBaseScene(true);
            SceneGenerator.RegisterBuildScenes();
            EditorSceneManager.OpenScene(EditorPaths.ScenePath(SceneNames.Base));
        }

        [MenuItem("Tools/DEADZONE/Open UI Gallery", priority = 61)]
        private static void OpenUiGallery()
        {
            string path = EditorPaths.ScenePath(SceneNames.UIGallery);
            if (!System.IO.File.Exists(path))
            {
                EditorUtility.DisplayDialog("UI Gallery", "The gallery scene does not exist yet. Run Setup Phase 01 first.", "OK");
                return;
            }
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(path);
        }

        // ---- Debug helpers (Play Mode only)

        [MenuItem("Tools/DEADZONE/Debug/Add Test Resources", priority = 100)]
        private static void AddTestResources()
        {
            if (!RequirePlayMode()) return;
            foreach (Deadzone.Data.ResourceType type in System.Enum.GetValues(typeof(Deadzone.Data.ResourceType)))
            {
                Game.Wallet.Add(type, 1000);
            }
            Debug.Log("[DEADZONE] +1000 of every resource added (debug).");
        }

        [MenuItem("Tools/DEADZONE/Debug/Spend Test Resources", priority = 101)]
        private static void SpendTestResources()
        {
            if (!RequirePlayMode()) return;
            foreach (Deadzone.Data.ResourceType type in System.Enum.GetValues(typeof(Deadzone.Data.ResourceType)))
            {
                Game.Wallet.TrySpend(type, 500);
            }
            Debug.Log("[DEADZONE] Tried to spend 500 of every resource (debug); resources below 500 were left untouched.");
        }

        private static bool RequirePlayMode()
        {
            if (Application.isPlaying && Game.IsInitialized) return true;
            EditorUtility.DisplayDialog("Play Mode required", "Enter Play Mode first (the game must be running).", "OK");
            return false;
        }

        [MenuItem("Tools/DEADZONE/Build Development APK", priority = 120)]
        private static void BuildApk() => BuildTools.BuildDevelopmentApk();
    }
}
