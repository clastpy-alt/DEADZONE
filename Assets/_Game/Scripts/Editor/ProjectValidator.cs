using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Deadzone.Core;
using Deadzone.Data;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Deadzone.EditorTools
{
    /// <summary>Checks Unity version, packages, URP, input handling, assets and scenes. Writes a report to the Console.</summary>
    internal static class ProjectValidator
    {
        public static bool Run(bool showDialog)
        {
            var report = new StringBuilder();
            int errors = 0;
            int warnings = 0;

            void Ok(string message) => report.AppendLine("[OK]    " + message);
            void Warn(string message) { warnings++; report.AppendLine("[WARN]  " + message); }
            void Fail(string message) { errors++; report.AppendLine("[ERROR] " + message); }

            report.AppendLine("DEADZONE project validation");

            if (Application.unityVersion.StartsWith("6000.")) Ok("Unity version " + Application.unityVersion);
            else Warn("Unity version " + Application.unityVersion + " (project targets Unity 6)");

            string manifestPath = Path.Combine(Directory.GetCurrentDirectory(), "Packages", "manifest.json");
            string manifest = File.Exists(manifestPath) ? File.ReadAllText(manifestPath) : string.Empty;
            string[] requiredPackages =
            {
                "com.unity.render-pipelines.universal",
                "com.unity.inputsystem",
                "com.unity.ugui"
            };
            foreach (string package in requiredPackages)
            {
                if (manifest.Contains("\"" + package + "\"")) Ok("Package " + package);
                else Fail("Package missing: " + package + " (Window > Package Manager)");
            }

            // The Test Framework can arrive as a transitive dependency, so ask the Package Manager, not the manifest.
            bool testFramework = false;
            foreach (UnityEditor.PackageManager.PackageInfo package in UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages())
            {
                if (package.name == "com.unity.test-framework") { testFramework = true; break; }
            }
            if (testFramework) Ok("Package com.unity.test-framework");
            else Warn("Package com.unity.test-framework is not installed: EditMode tests will not compile/run until it is added (Package Manager > Unity Registry > Test Framework).");

            RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
            if (pipeline != null && pipeline.GetType().Name.Contains("Universal")) Ok("URP asset active: " + pipeline.name);
            else Fail("URP is not the active render pipeline (Project Settings > Graphics)");

            string settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "ProjectSettings", "ProjectSettings.asset");
            if (File.Exists(settingsPath))
            {
                Match match = Regex.Match(File.ReadAllText(settingsPath), @"activeInputHandler:\s*(\d)");
                if (match.Success && match.Groups[1].Value == "0")
                    Fail("Active Input Handling is 'Input Manager (Old)'. Set it to 'Input System Package (New)' or 'Both'.");
                else
                    Ok("Active Input Handling supports the new Input System");
            }

            if (TMP_Settings.instance != null) Ok("TextMeshPro essential resources imported");
            else Fail("TextMeshPro essential resources missing (Window > TextMeshPro > Import TMP Essential Resources)");

            if (PlayerSettings.defaultInterfaceOrientation == UIOrientation.Portrait) Ok("Default orientation: Portrait");
            else Warn("Default orientation is not Portrait (run Setup Phase 01)");

            if (AssetDatabase.LoadAssetAtPath<GameConfig>(EditorPaths.GameConfigAsset) != null) Ok("GameConfig asset exists");
            else Warn("GameConfig asset missing (run Setup Phase 01; defaults are used at runtime)");

            string[] scenes = { SceneNames.Bootstrap, SceneNames.MainMenu, SceneNames.Base, SceneNames.WorldMap, SceneNames.Battle };
            foreach (string scene in scenes)
            {
                string path = EditorPaths.ScenePath(scene);
                if (!File.Exists(path)) { Fail("Scene missing: " + path); continue; }

                bool inBuild = false;
                foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes)
                {
                    if (entry.path == path && entry.enabled) inBuild = true;
                }
                if (inBuild) Ok("Scene in Build Settings: " + scene);
                else Warn("Scene not in Build Settings: " + scene);
            }

            foreach (BuildingPrefabGenerator.Spec spec in BuildingPrefabGenerator.Specs)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(BuildingPrefabGenerator.PrefabPath(spec)) == null)
                    Fail("Building prefab missing: " + spec.FileName);
            }
            string[] uiPrefabs =
            {
                UIPrefabGenerator.PrimaryButtonPath, UIPrefabGenerator.SecondaryButtonPath, UIPrefabGenerator.IconButtonPath,
                UIPrefabGenerator.ResourceCounterPath, UIPrefabGenerator.PopupPanelPath, UIPrefabGenerator.ConfirmationDialogPath,
                UIPrefabGenerator.RewardPopupPath, UIPrefabGenerator.TooltipPath, UIPrefabGenerator.LoadingScreenPath,
                UIPrefabGenerator.ProgressBarPath, UIPrefabGenerator.NotificationBadgePath, UIPrefabGenerator.TabNavigationPath,
                UIPrefabGenerator.ScrollablePanelPath, UIPrefabGenerator.SafeAreaControllerPath, UIPrefabGenerator.HeroCardPath,
                UIPrefabGenerator.ItemCardPath, UIPrefabGenerator.EventCardPath, UIPrefabGenerator.ShopCardPath, UIPrefabGenerator.UIOverlayPath, UIPrefabGenerator.ConstructionQueuePath
            };
            int missingUi = 0;
            foreach (string path in uiPrefabs)
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) { missingUi++; Fail("UI prefab missing: " + path); }
            }
            if (missingUi == 0) Ok("All " + uiPrefabs.Length + " UI prefabs exist");

            if (File.Exists(EditorPaths.ScenePath(SceneNames.UIGallery))) Ok("UIGallery scene exists (dev only, not in build)");
            else Warn("UIGallery scene missing (run Setup Phase 01)");

            if (missingUi == 0)
            {
                var referenceProblems = ReferenceValidator.Validate();
                if (referenceProblems.Count == 0) Ok("Prefab references: all serialized references assigned");
                foreach (string problem in referenceProblems) Fail(problem);
            }

            int incomplete = 0;
            foreach (string key in Loc.FindIncompleteKeys())
            {
                incomplete++;
                Warn("Incomplete translation: " + key);
            }
            if (incomplete == 0) Ok("Localization table complete (TR/EN)");

            report.AppendLine($"Result: {errors} error(s), {warnings} warning(s).");
            if (errors > 0) Debug.LogError(report.ToString());
            else if (warnings > 0) Debug.LogWarning(report.ToString());
            else Debug.Log(report.ToString());

            if (showDialog)
            {
                EditorUtility.DisplayDialog("DEADZONE Validate Project",
                    $"{errors} error(s), {warnings} warning(s).\nDetails are in the Console.", "OK");
            }
            return errors == 0;
        }
    }
}
