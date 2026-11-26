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

        [MenuItem("Tools/DEADZONE/Fix TMP Overflow (Ellipsis to Truncate)", priority = 44)]
        private static void FixTmpOverflow()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string openScene = EditorSceneManager.GetActiveScene().path;
            int changed = UiTextNormalizer.Run();
            if (!string.IsNullOrEmpty(openScene)) EditorSceneManager.OpenScene(openScene);
            EditorUtility.DisplayDialog("Fix TMP Overflow", changed + " text component(s) changed.", "OK");
        }

        // ---- PHASE 02

        [MenuItem("Tools/DEADZONE/Generate Building Database", priority = 45)]
        private static void GenerateBuildingDatabase()
        {
            BuildingPrefabGenerator.GenerateAll(false);
            Phase02Setup.GenerateBuildingData();
        }

        [MenuItem("Tools/DEADZONE/Generate Building Level Data", priority = 46)]
        private static void GenerateBuildingLevelData()
        {
            BuildingDatabaseGenerator.GenerateDefinitions();
            if (EditorUtility.DisplayDialog("Generate Building Level Data",
                    "Rebuild ALL level rows from each building's balance curve?\nManual edits to level rows will be lost. 'Fill missing' keeps them.",
                    "Rebuild all", "Fill missing only"))
            {
                Debug.Log("[DEADZONE] Level rows rebuilt for " + BuildingDatabaseGenerator.GenerateLevels(true) + " building(s).");
            }
            else
            {
                Debug.Log("[DEADZONE] Level rows filled for " + BuildingDatabaseGenerator.GenerateLevels(false) + " building(s) (existing rows kept).");
            }
        }

        [MenuItem("Tools/DEADZONE/Setup Phase 02 (Run After Phase 01)", priority = 1)]
        private static void SetupPhase02()
        {
            if (!ProjectSetup.CheckTextMeshPro()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Phase02Setup.UpdateGameConfig();
            BuildingPrefabGenerator.GenerateAll(false);
            Phase02Setup.GenerateBuildingData();
            UIPrefabGenerator.GenerateAll(false);
            Debug.Log("[DEADZONE] Warehouse data: " + Phase025Setup.PatchWarehouseData());
            Debug.Log("[DEADZONE] Base scene: " + BuildingUiTools.PatchBaseScene());
            Debug.Log("[DEADZONE] Warehouse scene: " + Phase025Setup.PatchWarehouseScene());
            Debug.Log("[DEADZONE] Military base: " + MilitaryBaseGenerator.Generate());
            AssetDatabase.SaveAssets();

            Phase02Validators.Report("Validate Buildings", Phase02Validators.ValidateBuildings(), false);
            Phase02Validators.Report("Validate Economy", Phase02Validators.ValidateEconomy(), false);
            Phase02Validators.Report("Validate Save Migration", Phase02Validators.ValidateSaveMigration(), false);
            ProjectValidator.Run(false);
            Debug.Log("[DEADZONE] Setup Phase 02 finished.");
        }

        [MenuItem("Tools/DEADZONE/Generate Building UI", priority = 47)]
        private static void GenerateBuildingUi()
        {
            if (!ProjectSetup.CheckTextMeshPro()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Debug.Log("[DEADZONE] Building UI: " + BuildingUiTools.PatchBaseScene());
        }

        [MenuItem("Tools/DEADZONE/Generate Military Base", priority = 48)]
        private static void GenerateMilitaryBase()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildingPrefabGenerator.GenerateAll(false);
            Debug.Log("[DEADZONE] Military base: " + MilitaryBaseGenerator.Generate());
        }

        [MenuItem("Tools/DEADZONE/Generate Military Materials", priority = 49)]
        private static void GenerateMilitaryMaterials()
        {
            int created = MilitaryMaterials.GenerateAll();
            Debug.Log("[DEADZONE] Military materials: " + created + " new material(s) (existing ones were kept).");
        }

        [MenuItem("Tools/DEADZONE/Generate Military Models", priority = 50)]
        private static void GenerateMilitaryModels()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildingPrefabGenerator.GenerateAll(false);
            Debug.Log("[DEADZONE] Military models:\n" + MilitaryModelGenerator.Generate(false));
        }

        [MenuItem("Tools/DEADZONE/Rebuild Military Models (Generated Parts Only)", priority = 51)]
        private static void RebuildMilitaryModels()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!EditorUtility.DisplayDialog("Rebuild Military Models",
                    "This deletes and regenerates ONLY the generated 'Mil_Visual' child of each building prefab. Manual edits inside Mil_Visual are lost. Continue?",
                    "Rebuild", "Cancel")) return;
            Debug.Log("[DEADZONE] Military models rebuilt:\n" + MilitaryModelGenerator.Generate(true));
        }

        [MenuItem("Tools/DEADZONE/Upgrade Base Environment", priority = 52)]
        private static void UpgradeBaseEnvironment()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Debug.Log("[DEADZONE] Base environment:\n" + BaseEnvironmentGenerator.Generate(false));
        }

        [MenuItem("Tools/DEADZONE/Rebuild Base Environment (Generated Only)", priority = 53)]
        private static void RebuildBaseEnvironment()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!EditorUtility.DisplayDialog("Rebuild Base Environment",
                    "This deletes and regenerates ONLY the 'Base Environment v2 [PLACEHOLDER]' object in the Base scene. Manual edits inside it are lost. Continue?",
                    "Rebuild", "Cancel")) return;
            Debug.Log("[DEADZONE] Base environment rebuilt:\n" + BaseEnvironmentGenerator.Generate(true));
        }

        [MenuItem("Tools/DEADZONE/Setup Lighting Profiles", priority = 54)]
        private static void SetupLightingProfiles()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Debug.Log("[DEADZONE] Graphics profiles:\n" + GraphicsProfileGenerator.Generate());
            Debug.Log("[DEADZONE] Lighting:\n" + LightingSetup.ApplyToBaseScene(false));
        }

        [MenuItem("Tools/DEADZONE/Rebuild Lighting Rig (Generated Only)", priority = 55)]
        private static void RebuildLightingRig()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!EditorUtility.DisplayDialog("Rebuild Lighting Rig",
                    "This deletes and regenerates ONLY the 'Lighting Rig [GENERATED]' object and re-applies sun, ambient and fog of the Base scene. Continue?",
                    "Rebuild", "Cancel")) return;
            Debug.Log("[DEADZONE] Lighting rebuilt:\n" + LightingSetup.ApplyToBaseScene(true));
        }

        [MenuItem("Tools/DEADZONE/Setup Phase 02.5 (Run After Phase 02)", priority = 2)]
        private static void SetupPhase025()
        {
            if (!ProjectSetup.CheckTextMeshPro()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Debug.Log("[DEADZONE] Military materials: " + MilitaryMaterials.GenerateAll() + " new material(s).");
            BuildingPrefabGenerator.GenerateAll(false);
            Debug.Log("[DEADZONE] Military models:\n" + MilitaryModelGenerator.Generate(false));
            Debug.Log("[DEADZONE] Base environment:\n" + BaseEnvironmentGenerator.Generate(false));
            Debug.Log("[DEADZONE] Graphics profiles:\n" + GraphicsProfileGenerator.Generate());
            Debug.Log("[DEADZONE] Lighting:\n" + LightingSetup.ApplyToBaseScene(false));
            Debug.Log("[DEADZONE] Premium HUD:\n" + PremiumHudGenerator.Generate());
            Debug.Log("[DEADZONE] LOD models:\n" + GraphicsTools.GenerateLodModels());
            AssetDatabase.SaveAssets();
            ReportGraphics();
            Debug.Log("[DEADZONE] Setup Phase 02.5 finished.");
        }

        [MenuItem("Tools/DEADZONE/Generate Premium HUD", priority = 56)]
        private static void GeneratePremiumHud()
        {
            if (!ProjectSetup.CheckTextMeshPro()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Debug.Log("[DEADZONE] Premium HUD:\n" + PremiumHudGenerator.Generate());
        }

        [MenuItem("Tools/DEADZONE/Generate LOD Models", priority = 57)]
        private static void GenerateLodModels()
        {
            Debug.Log("[DEADZONE] LOD models:\n" + GraphicsTools.GenerateLodModels());
        }

        [MenuItem("Tools/DEADZONE/Validate Graphics", priority = 25)]
        private static void ValidateGraphics() => ReportGraphics();

        [MenuItem("Tools/DEADZONE/Generate Performance Report", priority = 58)]
        private static void GeneratePerformanceReport()
        {
            Debug.Log("[DEADZONE] Performance report (static analysis, not a device measurement):\n" + GraphicsTools.GeneratePerformanceReport());
        }

        private static void ReportGraphics()
        {
            var problems = GraphicsTools.ValidateGraphics(out string summary);
            if (problems.Count == 0) Debug.Log("[DEADZONE] Validate Graphics: OK\n" + summary);
            else Debug.LogWarning("[DEADZONE] Validate Graphics: " + problems.Count + " problem(s)\n- " + string.Join("\n- ", problems) + "\n" + summary);
        }

        [MenuItem("Tools/DEADZONE/Validate Buildings", priority = 22)]
        private static void ValidateBuildings() => Phase02Validators.Report("Validate Buildings", Phase02Validators.ValidateBuildings(), true);

        [MenuItem("Tools/DEADZONE/Validate Economy", priority = 23)]
        private static void ValidateEconomy() => Phase02Validators.Report("Validate Economy", Phase02Validators.ValidateEconomy(), true);

        [MenuItem("Tools/DEADZONE/Validate Save Migration", priority = 24)]
        private static void ValidateSaveMigration() => Phase02Validators.Report("Validate Save Migration", Phase02Validators.ValidateSaveMigration(), true);

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
