using Deadzone.UI;
using UnityEditor;
using UnityEngine;

namespace Deadzone.EditorTools
{
    /// <summary>Creates URP Lit materials once and reuses them (idempotent).</summary>
    internal static class MaterialLibrary
    {
        public static Material Get(string name, string hex, string emissiveHex = null)
        {
            EditorPaths.EnsureFolder(EditorPaths.Materials);
            string path = EditorPaths.Materials + "/" + name + ".mat";

            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            var material = new Material(shader) { name = name };
            Color color = UIColors.Hex(hex);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.15f);

            if (!string.IsNullOrEmpty(emissiveHex))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", UIColors.Hex(emissiveHex));
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
