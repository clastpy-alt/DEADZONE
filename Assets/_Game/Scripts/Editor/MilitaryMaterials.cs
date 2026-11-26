using Deadzone.UI;
using UnityEditor;
using UnityEngine;

namespace Deadzone.EditorTools
{
    /// <summary>
    /// Shared URP/Lit material library for the military models (PHASE 02.5). Colours follow ART_DIRECTION.md.
    /// No textures yet (budget: colour + metallic/smoothness only); GPU instancing is enabled. A material that already exists
    /// is never overwritten, so hand edits in the Inspector survive re-running the tool.
    /// </summary>
    internal static class MilitaryMaterials
    {
        private struct Def
        {
            public string Name;
            public string Hex;
            public float Metallic;
            public float Smoothness;
            public string Emission;

            public Def(string name, string hex, float metallic, float smoothness, string emission = null)
            {
                Name = name;
                Hex = hex;
                Metallic = metallic;
                Smoothness = smoothness;
                Emission = emission;
            }
        }

        // Metallic stays moderate: there is no reflection probe yet, so high values would render nearly black.
        private static readonly Def[] All =
        {
            new Def("Mil_Concrete", "#6B7078", 0f, 0.2f),
            new Def("Mil_ConcreteDark", "#3C4249", 0f, 0.2f),
            new Def("Mil_ConcreteLight", "#8A9097", 0f, 0.25f),
            new Def("Mil_Asphalt", "#23272C", 0f, 0.2f),
            new Def("Mil_PaintOlive", "#465744", 0.15f, 0.35f),
            new Def("Mil_PaintOliveDark", "#343F33", 0.15f, 0.35f),
            new Def("Mil_SteelGray", "#5A6470", 0.45f, 0.45f),
            new Def("Mil_SteelDark", "#2B3138", 0.5f, 0.4f),
            new Def("Mil_MetalLight", "#ADB8C4", 0.55f, 0.5f),
            new Def("Mil_Rust", "#7A4B3A", 0.3f, 0.25f),
            new Def("Mil_Glass", "#27384A", 0.1f, 0.8f, "#0B1722"),
            new Def("Mil_WindowGlow", "#E6C46E", 0f, 0.3f, "#B08A30"),
            new Def("Mil_WarningRed", "#D94B4B", 0f, 0.3f, "#D94B4B"),
            new Def("Mil_Hazard", "#D4AF63", 0f, 0.3f),
            new Def("Mil_Gold", "#D4AF63", 0.7f, 0.6f),
            new Def("Mil_Sandbag", "#8B8068", 0f, 0.1f),
            new Def("Mil_Fabric", "#5B6650", 0f, 0.1f),
            new Def("Mil_Solar", "#1D3557", 0.5f, 0.7f),
            new Def("Mil_MedWhite", "#D8DDE2", 0f, 0.3f),
            new Def("Mil_MedRed", "#C23B3B", 0f, 0.3f),
            new Def("Mil_LabGlow", "#4C9FE8", 0f, 0.5f, "#2B6FB0"),
            new Def("Mil_HeatGlow", "#E8822E", 0f, 0.3f, "#C8601A"),
            new Def("Mil_Crop", "#4F7A3A", 0f, 0.1f),
            new Def("Mil_Soil", "#4A3B2A", 0f, 0.1f),
            new Def("Mil_Rubber", "#1A1C1F", 0f, 0.15f),
        };

        public static int GenerateAll()
        {
            int created = 0;
            foreach (Def def in All)
            {
                if (AssetDatabase.LoadAssetAtPath<Material>(PathOf(def.Name)) == null) created++;
                Get(def.Name);
            }
            AssetDatabase.SaveAssets();
            return created;
        }

        public static Material Get(string name)
        {
            string path = PathOf(name);
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;

            Def def = default;
            bool found = false;
            foreach (Def candidate in All)
            {
                if (candidate.Name == name) { def = candidate; found = true; break; }
            }
            if (!found) def = new Def(name, "#808080", 0f, 0.2f);

            EditorPaths.EnsureFolder(EditorPaths.Materials);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            var material = new Material(shader) { name = name, enableInstancing = true };
            Color color = UIColors.Hex(def.Hex);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", def.Metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", def.Smoothness);
            if (!string.IsNullOrEmpty(def.Emission))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", UIColors.Hex(def.Emission));
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static string PathOf(string name) => EditorPaths.Materials + "/" + name + ".mat";
    }
}
