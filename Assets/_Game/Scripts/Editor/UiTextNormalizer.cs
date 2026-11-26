using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Deadzone.EditorTools
{
    /// <summary>
    /// The default TMP font has no '…' glyph, so TextOverflowModes.Ellipsis only logs a warning and falls back
    /// to Truncate. This switches existing prefabs and scenes to Truncate without recreating them. Safe to re-run.
    /// </summary>
    internal static class UiTextNormalizer
    {
        public static int Run()
        {
            int changed = 0;

            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { EditorPaths.Root + "/Prefabs" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                int count = Normalize(root);
                if (count > 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    changed += count;
                }
                PrefabUtility.UnloadPrefabContents(root);
            }

            var scenePaths = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { EditorPaths.Scenes }))
            {
                scenePaths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }

            foreach (string scenePath in scenePaths)
            {
                var scene = EditorSceneManager.GetSceneByPath(scenePath);
                bool wasOpen = scene.isLoaded;
                if (!wasOpen) scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                int count = 0;
                foreach (GameObject root in scene.GetRootGameObjects()) count += Normalize(root);
                if (count > 0)
                {
                    EditorSceneManager.SaveScene(scene);
                    changed += count;
                }
                if (!wasOpen) EditorSceneManager.CloseScene(scene, true);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[DEADZONE] TMP overflow normalized: {changed} text component(s) changed Ellipsis -> Truncate.");
            return changed;
        }

        private static int Normalize(GameObject root)
        {
            int count = 0;
            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.overflowMode != TextOverflowModes.Ellipsis) continue;
                text.overflowMode = TextOverflowModes.Truncate;
                EditorUtility.SetDirty(text);
                count++;
            }
            return count;
        }
    }
}
