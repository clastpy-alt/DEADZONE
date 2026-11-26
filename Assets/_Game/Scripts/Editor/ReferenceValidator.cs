using System.Collections.Generic;
using Deadzone.UI;
using UnityEditor;
using UnityEngine;

namespace Deadzone.EditorTools
{
    /// <summary>
    /// Finds unassigned serialized object references on Deadzone components inside the generated UI and building prefabs.
    /// Public so the EditMode tests can call it.
    /// </summary>
    public static class ReferenceValidator
    {
        public static readonly string[] PrefabFolders =
        {
            EditorPaths.PrefabsUI,
            EditorPaths.PrefabsBuildings
        };

        /// <summary>Returns one message per problem. An empty list means everything is wired.</summary>
        public static List<string> Validate()
        {
            var problems = new List<string>();

            foreach (string folder in PrefabFolders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    problems.Add("Folder missing: " + folder + " (run Tools > DEADZONE > Setup Phase 01)");
                    continue;
                }

                foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { folder }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab == null)
                    {
                        problems.Add("Prefab cannot be loaded: " + path);
                        continue;
                    }
                    ValidatePrefab(prefab, path, problems);
                }
            }
            return problems;
        }

        private static void ValidatePrefab(GameObject prefab, string path, List<string> problems)
        {
            foreach (MonoBehaviour behaviour in prefab.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null)
                {
                    problems.Add($"{path}: missing script on a child object");
                    continue;
                }

                string ns = behaviour.GetType().Namespace;
                if (ns == null || !ns.StartsWith("Deadzone")) continue;

                var serialized = new SerializedObject(behaviour);
                SerializedProperty property = serialized.GetIterator();
                bool enterChildren = true;
                while (property.NextVisible(enterChildren))
                {
                    enterChildren = true;
                    if (property.name == "m_Script") continue;
                    if (property.propertyType != SerializedPropertyType.ObjectReference) continue;
                    if (property.objectReferenceValue != null) continue;

                    problems.Add($"{path}: {behaviour.GetType().Name}.{property.propertyPath} on '{behaviour.gameObject.name}' is not assigned");
                }
            }
        }
    }
}
