using System.IO;
using Deadzone.Core;
using Deadzone.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Deadzone.EditorTools
{
    /// <summary>
    /// Brings an EXISTING Base scene up to the PHASE 02 UI without recreating it: only the old building info panel is
    /// replaced (when it still has the PHASE 01 layout) and the construction queue window is added when missing.
    /// Everything else in the scene (buildings, environment, camera, your own changes) is left alone. Safe to re-run.
    /// </summary>
    internal static class BuildingUiTools
    {
        public static string PatchBaseScene()
        {
            string path = EditorPaths.ScenePath(SceneNames.Base);
            if (!File.Exists(path)) return "Base scene not found (run Setup Phase 01 first).";

            UIPrefabGenerator.GenerateAll(false);

            Scene previousActive = SceneManager.GetActiveScene();
            Scene existing = SceneManager.GetSceneByPath(path);
            bool wasLoaded = existing.IsValid() && existing.isLoaded;
            Scene scene = wasLoaded ? existing : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            var report = new System.Text.StringBuilder();
            try
            {
                SceneManager.SetActiveScene(scene);
                Transform canvas = FindInScene(scene, "HUD Canvas");
                Transform safe = canvas != null ? canvas.Find("SafeArea") : null;
                if (canvas == null || safe == null)
                {
                    return "HUD Canvas / SafeArea not found in the Base scene; nothing was changed.";
                }

                // 1) Building info sheet
                BuildingInfoPanel info = FindComponent<BuildingInfoPanel>(scene);
                Transform toast = safe.Find("Toast");
                if (info == null || !info.HasDetailLayout)
                {
                    if (info != null) Object.DestroyImmediate(info.gameObject);
                    RectTransform sheet = UIPrefabGenerator.BuildInfoPanelUi(safe);
                    if (toast != null) sheet.SetSiblingIndex(toast.GetSiblingIndex());
                    report.AppendLine("info panel rebuilt");
                }

                // The toast must stay visible above the taller sheet (only moved while it still has the PHASE 01 position).
                if (toast != null)
                {
                    var toastRect = (RectTransform)toast;
                    if (Mathf.Abs(toastRect.anchoredPosition.y - 470f) < 1f)
                    {
                        toastRect.anchoredPosition = new Vector2(toastRect.anchoredPosition.x, SceneGenerator.ToastBottomOffset);
                        report.AppendLine("toast moved above the sheet");
                    }
                    toast.SetAsLastSibling();
                }

                // The PHASE 01 start zoom (14) is too close in portrait; raise it only while it still has the old default.
                IsometricCameraController cameraController = FindComponent<IsometricCameraController>(scene);
                if (cameraController != null)
                {
                    var serialized = new SerializedObject(cameraController);
                    SerializedProperty zoom = serialized.FindProperty("startZoom");
                    if (zoom != null && Mathf.Abs(zoom.floatValue - 14f) < 0.01f)
                    {
                        zoom.floatValue = 22f;
                        serialized.ApplyModifiedPropertiesWithoutUndo();
                        report.AppendLine("camera start zoom 14 -> 22");
                    }

                    // PHASE 02.5: portrait view that shows the whole compound (22 -> 26, max 34 -> 40), only while still at the old defaults.
                    zoom = serialized.FindProperty("startZoom");
                    SerializedProperty maxZoom = serialized.FindProperty("maxZoom");
                    bool changed = false;
                    if (zoom != null && Mathf.Abs(zoom.floatValue - 22f) < 0.01f) { zoom.floatValue = 26f; changed = true; }
                    if (maxZoom != null && Mathf.Abs(maxZoom.floatValue - 34f) < 0.01f) { maxZoom.floatValue = 40f; changed = true; }
                    if (changed)
                    {
                        serialized.ApplyModifiedPropertiesWithoutUndo();
                        report.AppendLine("camera zoom: start 26, max 40");
                    }
                }

                // 2) Construction queue window
                if (FindComponent<ConstructionQueuePanel>(scene) == null)
                {
                    GameObject instance = UIPrefabGenerator.Instantiate(UIPrefabGenerator.ConstructionQueuePath, canvas);
                    instance.SetActive(false);
                    report.AppendLine("construction queue panel added");
                }

                if (report.Length > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
            finally
            {
                if (previousActive.IsValid() && previousActive.isLoaded) SceneManager.SetActiveScene(previousActive);
                if (!wasLoaded) EditorSceneManager.CloseScene(scene, true);
            }

            return report.Length == 0 ? "Base scene already up to date." : report.ToString().Trim();
        }

        private static Transform FindInScene(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == name) return root.transform;
            }
            return null;
        }

        private static T FindComponent<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T found = root.GetComponentInChildren<T>(true);
                if (found != null) return found;
            }
            return null;
        }
    }
}
