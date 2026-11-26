using Deadzone.Buildings;
using UnityEditor;
using UnityEngine;

namespace Deadzone.EditorTools
{
    /// <summary>Generates PLACEHOLDER building prefabs from primitives. Re-running skips existing prefabs.</summary>
    internal static class BuildingPrefabGenerator
    {
        internal sealed class Spec
        {
            public string Id;
            public string NameKey;
            public string FileName;
            public float Width;
            public float Height;
            public float Depth;
            public string ColorHex;
        }

        public static readonly Spec[] Specs =
        {
            new Spec { Id = "headquarters", NameKey = "building.hq", FileName = "Headquarters", Width = 6f, Height = 4f, Depth = 6f, ColorHex = "#465744" },
            new Spec { Id = "barracks", NameKey = "building.barracks", FileName = "Barracks", Width = 7f, Height = 2.6f, Depth = 3.5f, ColorHex = "#3F4F3D" },
            new Spec { Id = "hospital", NameKey = "building.hospital", FileName = "Hospital", Width = 5f, Height = 3f, Depth = 5f, ColorHex = "#8A9199" },
            new Spec { Id = "research_lab", NameKey = "building.lab", FileName = "ResearchLab", Width = 5f, Height = 3.4f, Depth = 5f, ColorHex = "#4E5E6E" },
            new Spec { Id = "food_farm", NameKey = "building.farm", FileName = "FoodFarm", Width = 6f, Height = 2f, Depth = 4.5f, ColorHex = "#6B5B3E" },
            new Spec { Id = "steel_factory", NameKey = "building.steel", FileName = "SteelFactory", Width = 6f, Height = 3f, Depth = 5f, ColorHex = "#5A5F66" },
            new Spec { Id = "warehouse", NameKey = "building.warehouse", FileName = "Warehouse", Width = 6f, Height = 2.4f, Depth = 4.5f, ColorHex = "#59626B" }
        };

        public static string PrefabPath(Spec spec) => EditorPaths.PrefabsBuildings + "/" + spec.FileName + ".prefab";

        public static void GenerateAll(bool overwrite)
        {
            EditorPaths.EnsureFolder(EditorPaths.PrefabsBuildings);

            foreach (Spec spec in Specs)
            {
                string path = PrefabPath(spec);
                if (!overwrite && AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) continue;

                GameObject root = Build(spec);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Object.DestroyImmediate(root);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GameObject Build(Spec spec)
        {
            Material concrete = MaterialLibrary.Get("Concrete", "#5E6670");
            Material roofMat = MaterialLibrary.Get("Roof", "#2A2F36");
            Material trim = MaterialLibrary.Get("Trim", "#2F353C");
            Material metal = MaterialLibrary.Get("Metal", "#6F7A86");
            Material body = MaterialLibrary.Get("Bld_" + spec.FileName, spec.ColorHex);
            Material ringMat = MaterialLibrary.Get("SelectionRing", "#D4AF63", "#D4AF63");

            float w = spec.Width, h = spec.Height, d = spec.Depth;
            var root = new GameObject(spec.FileName + "_PLACEHOLDER");
            Transform t = root.transform;

            ModelBuilder.Prim(PrimitiveType.Cube, "Foundation", t, new Vector3(0, 0.1f, 0), new Vector3(w + 1f, 0.2f, d + 1f), concrete);
            ModelBuilder.Prim(PrimitiveType.Cube, "Body", t, new Vector3(0, 0.2f + h / 2f, 0), new Vector3(w, h, d), body);
            ModelBuilder.Prim(PrimitiveType.Cube, "Trim", t, new Vector3(0, 0.2f + h * 0.55f, 0), new Vector3(w + 0.06f, 0.12f, d + 0.06f), trim);
            ModelBuilder.Prim(PrimitiveType.Cube, "Roof", t, new Vector3(0, 0.2f + h + 0.125f, 0), new Vector3(w + 0.3f, 0.25f, d + 0.3f), roofMat);
            ModelBuilder.Prim(PrimitiveType.Cube, "Door", t, new Vector3(0, 0.2f + 0.8f, -d / 2f - 0.03f), new Vector3(1.2f, 1.6f, 0.1f), trim);

            float roofY = 0.2f + h + 0.25f;
            switch (spec.Id)
            {
                case "headquarters":
                    ModelBuilder.Prim(PrimitiveType.Cube, "UpperBlock", t, new Vector3(0, roofY + 0.6f, 0), new Vector3(w * 0.55f, 1.2f, d * 0.55f), body);
                    ModelBuilder.Prim(PrimitiveType.Cylinder, "Antenna", t, new Vector3(1.4f, roofY + 2.2f, 1.4f), new Vector3(0.12f, 1.6f, 0.12f), metal);
                    ModelBuilder.Prim(PrimitiveType.Cylinder, "FlagPole", t, new Vector3(-2.4f, roofY + 1.5f, -2.4f), new Vector3(0.08f, 1.5f, 0.08f), metal);
                    ModelBuilder.Prim(PrimitiveType.Cube, "Flag", t, new Vector3(-2.0f, roofY + 2.5f, -2.4f), new Vector3(0.8f, 0.5f, 0.05f), MaterialLibrary.Get("FlagGreen", "#4CAF72"));
                    break;
                case "hospital":
                    Material red = MaterialLibrary.Get("MedicalRed", "#C23B3B");
                    ModelBuilder.Prim(PrimitiveType.Cube, "CrossH", t, new Vector3(0, roofY + 0.03f, 0), new Vector3(2.0f, 0.06f, 0.55f), red);
                    ModelBuilder.Prim(PrimitiveType.Cube, "CrossV", t, new Vector3(0, roofY + 0.03f, 0), new Vector3(0.55f, 0.06f, 2.0f), red);
                    break;
                case "research_lab":
                    ModelBuilder.Prim(PrimitiveType.Sphere, "Dome", t, new Vector3(0, roofY + 0.2f, 0), new Vector3(w * 0.55f, w * 0.4f, w * 0.55f), metal);
                    ModelBuilder.Prim(PrimitiveType.Cylinder, "Antenna", t, new Vector3(1.8f, roofY + 1.2f, -1.8f), new Vector3(0.1f, 1.2f, 0.1f), metal);
                    break;
                case "food_farm":
                    ModelBuilder.Prim(PrimitiveType.Cylinder, "Silo", t, new Vector3(w / 2f + 1.4f, 2.2f, 0), new Vector3(2f, 2.2f, 2f), metal);
                    ModelBuilder.Prim(PrimitiveType.Sphere, "SiloCap", t, new Vector3(w / 2f + 1.4f, 4.4f, 0), new Vector3(2f, 1f, 2f), metal);
                    break;
                case "steel_factory":
                    ModelBuilder.Prim(PrimitiveType.Cylinder, "Chimney1", t, new Vector3(-1.8f, roofY + 2.2f, 1.2f), new Vector3(0.9f, 2.2f, 0.9f), trim);
                    ModelBuilder.Prim(PrimitiveType.Cylinder, "Chimney2", t, new Vector3(0.2f, roofY + 2.2f, 1.2f), new Vector3(0.9f, 2.2f, 0.9f), trim);
                    break;
                case "warehouse":
                    ModelBuilder.Prim(PrimitiveType.Cube, "Container1", t, new Vector3(-1.6f, roofY + 0.6f, 0.4f), new Vector3(2.4f, 1.2f, 1.2f), MaterialLibrary.Get("ContainerRust", "#7A4B3A"));
                    ModelBuilder.Prim(PrimitiveType.Cube, "Container2", t, new Vector3(1.0f, roofY + 0.6f, -0.3f), new Vector3(2.4f, 1.2f, 1.2f), MaterialLibrary.Get("ContainerGreen", "#3F5A44"));
                    ModelBuilder.Prim(PrimitiveType.Cylinder, "WaterTank", t, new Vector3(w / 2f + 1.3f, 1.2f, 0), new Vector3(1.8f, 1.2f, 1.8f), metal);
                    break;
                case "barracks":
                    ModelBuilder.Prim(PrimitiveType.Cube, "RoofRidge", t, new Vector3(0, roofY + 0.15f, 0), new Vector3(w * 0.9f, 0.3f, 0.5f), trim);
                    break;
            }

            float ringSize = Mathf.Max(w, d) + 2.4f;
            GameObject ring = ModelBuilder.Prim(PrimitiveType.Cylinder, "SelectionRing", t, new Vector3(0, 0.04f, 0), new Vector3(ringSize, 0.01f, ringSize), ringMat);

            var collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0, (h + 0.4f) / 2f, 0);
            collider.size = new Vector3(w + 1f, h + 0.4f, d + 1f);

            var selectable = root.AddComponent<BuildingSelectable>();
            selectable.Configure(spec.Id, spec.NameKey, 35, ring);
            return root;
        }
    }
}
