using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Deadzone.EditorTools
{
    /// <summary>
    /// Combines the primitive parts of a model group into one mesh per material (fewer draw calls and renderers; see
    /// PERFORMANCE_BUDGET.md). The combined meshes are saved as assets so the prefab keeps working after the editor closes.
    /// </summary>
    internal static class MeshMerger
    {
        public const string MeshFolder = EditorPaths.Root + "/Art/Models/Generated";

        /// <summary>Merges every MeshFilter below 'group' (active or not) and returns the number of merged meshes created.</summary>
        public static int MergeGroup(Transform group, string assetPrefix)
        {
            EditorPaths.EnsureFolder(MeshFolder);

            var byMaterial = new Dictionary<Material, List<MeshFilter>>();
            var order = new List<Material>();
            foreach (MeshFilter filter in group.GetComponentsInChildren<MeshFilter>(true))
            {
                var renderer = filter.GetComponent<MeshRenderer>();
                if (renderer == null || filter.sharedMesh == null || renderer.sharedMaterial == null) continue;

                Material material = renderer.sharedMaterial;
                if (!byMaterial.TryGetValue(material, out List<MeshFilter> list))
                {
                    list = new List<MeshFilter>();
                    byMaterial[material] = list;
                    order.Add(material);
                }
                list.Add(filter);
            }

            Matrix4x4 worldToGroup = group.worldToLocalMatrix;
            int created = 0;
            foreach (Material material in order)
            {
                List<MeshFilter> filters = byMaterial[material];
                var combine = new CombineInstance[filters.Count];
                for (int i = 0; i < filters.Count; i++)
                {
                    combine[i].mesh = filters[i].sharedMesh;
                    combine[i].transform = worldToGroup * filters[i].transform.localToWorldMatrix;
                }

                var mesh = new Mesh { name = assetPrefix + "_" + group.name + "_" + material.name, indexFormat = IndexFormat.UInt32 };
                mesh.CombineMeshes(combine, true, true);
                mesh.RecalculateBounds();
                MeshUtility.Optimize(mesh);

                string path = MeshFolder + "/" + mesh.name + ".asset";
                var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (existing != null)
                {
                    EditorUtility.CopySerialized(mesh, existing);
                    Object.DestroyImmediate(mesh);
                    mesh = existing;
                }
                else
                {
                    AssetDatabase.CreateAsset(mesh, path);
                }

                var child = new GameObject("Merged_" + material.name);
                child.transform.SetParent(group, false);
                child.AddComponent<MeshFilter>().sharedMesh = mesh;
                var meshRenderer = child.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = material;
                created++;
            }

            // Remove the original primitives (everything that is not one of the new merged children).
            var toDestroy = new List<GameObject>();
            foreach (Transform child in group)
            {
                if (!child.name.StartsWith("Merged_")) toDestroy.Add(child.gameObject);
            }
            foreach (GameObject go in toDestroy) Object.DestroyImmediate(go);

            return created;
        }
    }
}
