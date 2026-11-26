using System.IO;
using System.Text;
using Deadzone.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Deadzone.EditorTools
{
    /// <summary>
    /// PHASE 02.03: dresses the military base with more placeholder props (helipad, barrels, fences, bushes, generator, radar)
    /// and adds windows / roof vents / antennas to the building prefabs. Everything is built from Unity primitives with URP
    /// materials from MaterialLibrary (no external assets, no licences). Safe to re-run: a prop group or detail child that
    /// already exists is never rebuilt or duplicated.
    /// </summary>
    internal static class MilitaryBaseGenerator
    {
        public const string PropsGroupName = "Military Base Props [PLACEHOLDER]";
        private const string DetailsName = "Details_PH";

        public static string Generate()
        {
            var report = new StringBuilder();
            report.AppendLine(AddBuildingDetails() + " building prefab(s) received windows/vents/antennas");
            report.Append(PatchBaseScene());
            AssetDatabase.SaveAssets();
            return report.ToString().Trim();
        }

        // ------------------------------------------------------------------ building prefab details

        private static int AddBuildingDetails()
        {
            int patched = 0;
            foreach (BuildingPrefabGenerator.Spec spec in BuildingPrefabGenerator.Specs)
            {
                string path = BuildingPrefabGenerator.PrefabPath(spec);
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;

                GameObject contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    if (contents.transform.Find(DetailsName) != null) continue;
                    BuildDetails(contents.transform, spec);
                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                    patched++;
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }
            return patched;
        }

        private static void BuildDetails(Transform root, BuildingPrefabGenerator.Spec spec)
        {
            Material glass = MaterialLibrary.Get("WindowGlass", "#27384A", "#10202E");
            Material vent = MaterialLibrary.Get("Vent", "#3A4047");
            Material metal = MaterialLibrary.Get("Metal", "#6F7A86");
            Material trim = MaterialLibrary.Get("Trim", "#2F353C");

            Transform details = ModelBuilder.Group(DetailsName, root);
            float w = spec.Width, h = spec.Height, d = spec.Depth;
            float windowY = 0.2f + h * 0.68f;

            // Windows: front (skipping the door), back and both sides.
            int perFront = Mathf.Max(2, Mathf.FloorToInt(w / 1.8f));
            for (int i = 0; i < perFront; i++)
            {
                float x = Mathf.Lerp(-w / 2f + 0.9f, w / 2f - 0.9f, perFront == 1 ? 0.5f : i / (float)(perFront - 1));
                if (Mathf.Abs(x) < 1.1f) continue;
                ModelBuilder.Prim(PrimitiveType.Cube, "Window_F" + i, details, new Vector3(x, windowY, -d / 2f - 0.03f), new Vector3(0.9f, 0.7f, 0.06f), glass);
                ModelBuilder.Prim(PrimitiveType.Cube, "Window_B" + i, details, new Vector3(x, windowY, d / 2f + 0.03f), new Vector3(0.9f, 0.7f, 0.06f), glass);
            }
            int perSide = Mathf.Max(1, Mathf.FloorToInt(d / 2f));
            for (int i = 0; i < perSide; i++)
            {
                float z = Mathf.Lerp(-d / 2f + 1f, d / 2f - 1f, perSide == 1 ? 0.5f : i / (float)(perSide - 1));
                ModelBuilder.Prim(PrimitiveType.Cube, "Window_L" + i, details, new Vector3(-w / 2f - 0.03f, windowY, z), new Vector3(0.06f, 0.7f, 0.9f), glass);
                ModelBuilder.Prim(PrimitiveType.Cube, "Window_R" + i, details, new Vector3(w / 2f + 0.03f, windowY, z), new Vector3(0.06f, 0.7f, 0.9f), glass);
            }

            // Door frame and a small awning above the door.
            ModelBuilder.Prim(PrimitiveType.Cube, "DoorFrame", details, new Vector3(0, 0.2f + 0.85f, -d / 2f - 0.07f), new Vector3(1.5f, 1.8f, 0.05f), trim);
            ModelBuilder.Prim(PrimitiveType.Cube, "Awning", details, new Vector3(0, 0.2f + 1.95f, -d / 2f - 0.35f), new Vector3(1.9f, 0.1f, 0.8f), trim);

            // Roof vents (AC boxes + a small exhaust pipe).
            float roofY = 0.2f + h + 0.25f;
            ModelBuilder.Prim(PrimitiveType.Cube, "VentBox1", details, new Vector3(-w * 0.28f, roofY + 0.25f, d * 0.2f), new Vector3(1.0f, 0.5f, 0.8f), vent);
            ModelBuilder.Prim(PrimitiveType.Cube, "VentBox2", details, new Vector3(w * 0.28f, roofY + 0.25f, -d * 0.2f), new Vector3(0.8f, 0.5f, 0.8f), vent);
            ModelBuilder.Prim(PrimitiveType.Cylinder, "Exhaust", details, new Vector3(w * 0.1f, roofY + 0.45f, d * 0.3f), new Vector3(0.25f, 0.45f, 0.25f), metal);

            // Antennas on the command / science buildings.
            if (spec.Id == "barracks" || spec.Id == "steel_factory" || spec.Id == "food_farm")
            {
                ModelBuilder.Prim(PrimitiveType.Cylinder, "Antenna", details, new Vector3(-w * 0.4f, roofY + 1.0f, d * 0.35f), new Vector3(0.07f, 1.0f, 0.07f), metal);
            }
            if (spec.Id == "hospital")
            {
                ModelBuilder.Prim(PrimitiveType.Cylinder, "Antenna", details, new Vector3(-w * 0.4f, roofY + 0.9f, -d * 0.3f), new Vector3(0.07f, 0.9f, 0.07f), metal);
            }
            if (spec.Id == "headquarters")
            {
                ModelBuilder.Prim(PrimitiveType.Sphere, "SatelliteDish", details, new Vector3(-1.6f, roofY + 1.55f, 1.4f), new Vector3(1.3f, 0.35f, 1.3f), metal, new Vector3(-35f, 0, 0));
                ModelBuilder.Prim(PrimitiveType.Cylinder, "RadioMast", details, new Vector3(2.2f, roofY + 2.6f, -1.6f), new Vector3(0.1f, 2.6f, 0.1f), metal);
            }
        }

        // ------------------------------------------------------------------ Base scene props

        private static string PatchBaseScene()
        {
            string path = EditorPaths.ScenePath(SceneNames.Base);
            if (!File.Exists(path)) return "Base scene not found (run Setup Phase 01 first).";

            Scene previousActive = SceneManager.GetActiveScene();
            Scene existing = SceneManager.GetSceneByPath(path);
            bool wasLoaded = existing.IsValid() && existing.isLoaded;
            Scene scene = wasLoaded ? existing : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            try
            {
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (root.name == PropsGroupName) return "Base props already present (kept).";
                }

                SceneManager.SetActiveScene(scene);
                Transform props = ModelBuilder.Group(PropsGroupName, null);
                BuildProps(props);
                MarkStatic(props.gameObject);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                return "Base props added: " + CountChildren(props) + " object(s).";
            }
            finally
            {
                if (previousActive.IsValid() && previousActive.isLoaded) SceneManager.SetActiveScene(previousActive);
                if (!wasLoaded) EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static int CountChildren(Transform t) => t.GetComponentsInChildren<Transform>(true).Length - 1;

        private static void MarkStatic(GameObject root)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.BatchingStatic);
            }
        }

        private static void BuildProps(Transform root)
        {
            Helipad(root, new Vector3(16f, 0f, 12f));
            BarrelCluster(root, new Vector3(-20f, 0f, -16f), 7);
            BarrelCluster(root, new Vector3(16.5f, 0f, -9f), 5);
            BarrelCluster(root, new Vector3(-19f, 0f, 9f), 4);
            FenceSquare(root, new Vector3(-20f, 0f, -16f), 7f);
            FenceLine(root, new Vector3(8f, 0f, -30f), new Vector3(8f, 0f, -44f));
            FenceLine(root, new Vector3(-8f, 0f, -30f), new Vector3(-8f, 0f, -44f));
            Generator(root, new Vector3(-8f, 0f, 22f));
            RadarDish(root, new Vector3(8f, 0f, 21f));
            Bushes(root);
        }

        private static void Helipad(Transform parent, Vector3 pos)
        {
            Material padMat = MaterialLibrary.Get("HelipadPad", "#2E343B");
            Material yellow = MaterialLibrary.Get("HelipadYellow", "#D4AF63");
            Material white = MaterialLibrary.Get("HelipadWhite", "#E8E8E8");
            Material lamp = MaterialLibrary.Get("LampGlow", "#FFD9A0", "#FFB45A");

            Transform t = ModelBuilder.Group("Helipad_PH", parent, pos);
            ModelBuilder.Prim(PrimitiveType.Cylinder, "Ring", t, new Vector3(0, 0.05f, 0), new Vector3(9.4f, 0.05f, 9.4f), yellow);
            ModelBuilder.Prim(PrimitiveType.Cylinder, "Pad", t, new Vector3(0, 0.09f, 0), new Vector3(8.6f, 0.05f, 8.6f), padMat);
            ModelBuilder.Prim(PrimitiveType.Cube, "H_Left", t, new Vector3(-1.2f, 0.13f, 0), new Vector3(0.45f, 0.03f, 3.2f), white);
            ModelBuilder.Prim(PrimitiveType.Cube, "H_Right", t, new Vector3(1.2f, 0.13f, 0), new Vector3(0.45f, 0.03f, 3.2f), white);
            ModelBuilder.Prim(PrimitiveType.Cube, "H_Bar", t, new Vector3(0, 0.13f, 0), new Vector3(2.4f, 0.03f, 0.45f), white);
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad;
                ModelBuilder.Prim(PrimitiveType.Sphere, "Light" + i, t, new Vector3(Mathf.Cos(angle) * 4.5f, 0.2f, Mathf.Sin(angle) * 4.5f), new Vector3(0.3f, 0.3f, 0.3f), lamp);
            }
        }

        private static void BarrelCluster(Transform parent, Vector3 pos, int count)
        {
            string[] names = { "BarrelRed", "BarrelOlive", "BarrelBlue" };
            string[] hex = { "#9A3B34", "#55633F", "#3F5F85" };
            var rng = new System.Random(Mathf.RoundToInt(pos.x * 31f + pos.z * 17f));
            Transform t = ModelBuilder.Group("Barrels_PH", parent, pos);
            for (int i = 0; i < count; i++)
            {
                int kind = i % names.Length;
                Material m = MaterialLibrary.Get(names[kind], hex[kind]);
                float x = (i % 3) * 1.0f - 1.0f + (float)(rng.NextDouble() - 0.5) * 0.2f;
                float z = (i / 3) * 1.0f + (float)(rng.NextDouble() - 0.5) * 0.2f;
                ModelBuilder.Prim(PrimitiveType.Cylinder, "Barrel", t, new Vector3(x, 0.55f, z), new Vector3(0.8f, 0.55f, 0.8f), m);
                ModelBuilder.Prim(PrimitiveType.Cylinder, "BarrelBand", t, new Vector3(x, 0.55f, z), new Vector3(0.84f, 0.05f, 0.84f), MaterialLibrary.Get("Trim", "#2F353C"));
            }
        }

        private static void FencePost(Transform parent, Vector3 pos, Material post)
        {
            ModelBuilder.Prim(PrimitiveType.Cylinder, "Post", parent, pos + new Vector3(0, 0.9f, 0), new Vector3(0.12f, 0.9f, 0.12f), post);
        }

        private static void FenceLine(Transform parent, Vector3 from, Vector3 to)
        {
            Material post = MaterialLibrary.Get("FencePost", "#555D66");
            Material mesh = MaterialLibrary.Get("FenceMesh", "#3C434B");
            Transform t = ModelBuilder.Group("Fence_PH", parent);

            Vector3 delta = to - from;
            float length = delta.magnitude;
            int segments = Mathf.Max(1, Mathf.RoundToInt(length / 2.5f));
            float yaw = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;

            for (int i = 0; i <= segments; i++)
            {
                FencePost(t, from + delta * (i / (float)segments), post);
            }
            Vector3 mid = from + delta * 0.5f;
            ModelBuilder.Prim(PrimitiveType.Cube, "RailTop", t, mid + new Vector3(0, 1.75f, 0), new Vector3(0.06f, 0.06f, length), post, new Vector3(0, yaw, 0));
            ModelBuilder.Prim(PrimitiveType.Cube, "Panel", t, mid + new Vector3(0, 0.95f, 0), new Vector3(0.03f, 1.5f, length), mesh, new Vector3(0, yaw, 0));
        }

        /// <summary>Square fence with a one-segment gap on the road side.</summary>
        private static void FenceSquare(Transform parent, Vector3 center, float size)
        {
            float h = size / 2f;
            Vector3 a = center + new Vector3(-h, 0, -h);
            Vector3 b = center + new Vector3(h, 0, -h);
            Vector3 c = center + new Vector3(h, 0, h);
            Vector3 d = center + new Vector3(-h, 0, h);
            FenceLine(parent, a, Vector3.Lerp(a, b, 0.35f)); // gap between 0.35 and 0.65 on the south side
            FenceLine(parent, Vector3.Lerp(a, b, 0.65f), b);
            FenceLine(parent, b, c);
            FenceLine(parent, c, d);
            FenceLine(parent, d, a);
        }

        private static void Generator(Transform parent, Vector3 pos)
        {
            Material body = MaterialLibrary.Get("GeneratorBody", "#6B6F3E");
            Material dark = MaterialLibrary.Get("Trim", "#2F353C");
            Material metal = MaterialLibrary.Get("Metal", "#6F7A86");
            Transform t = ModelBuilder.Group("Generator_PH", parent, pos);
            ModelBuilder.Prim(PrimitiveType.Cube, "Body", t, new Vector3(0, 0.8f, 0), new Vector3(3.2f, 1.6f, 1.6f), body);
            ModelBuilder.Prim(PrimitiveType.Cube, "Grille", t, new Vector3(0, 0.8f, -0.82f), new Vector3(2.4f, 1.0f, 0.05f), dark);
            ModelBuilder.Prim(PrimitiveType.Cylinder, "Exhaust", t, new Vector3(1.2f, 2.0f, 0.4f), new Vector3(0.22f, 0.5f, 0.22f), metal);
        }

        private static void RadarDish(Transform parent, Vector3 pos)
        {
            Material metal = MaterialLibrary.Get("Metal", "#6F7A86");
            Material dark = MaterialLibrary.Get("Trim", "#2F353C");
            Transform t = ModelBuilder.Group("RadarDish_PH", parent, pos);
            ModelBuilder.Prim(PrimitiveType.Cylinder, "Base", t, new Vector3(0, 0.3f, 0), new Vector3(1.4f, 0.3f, 1.4f), dark);
            ModelBuilder.Prim(PrimitiveType.Cylinder, "Pole", t, new Vector3(0, 1.4f, 0), new Vector3(0.25f, 1.1f, 0.25f), metal);
            ModelBuilder.Prim(PrimitiveType.Sphere, "Dish", t, new Vector3(0, 2.8f, 0), new Vector3(2.4f, 0.5f, 2.4f), metal, new Vector3(-50f, 0, 0));
            ModelBuilder.Prim(PrimitiveType.Cylinder, "Feed", t, new Vector3(0, 3.35f, -0.55f), new Vector3(0.06f, 0.55f, 0.06f), dark, new Vector3(-50f, 0, 0));
        }

        private static void Bushes(Transform parent)
        {
            Material leaf = MaterialLibrary.Get("Bush", "#35552F");
            Material leafDark = MaterialLibrary.Get("BushDark", "#27422A");
            var rng = new System.Random(2026);
            Transform t = ModelBuilder.Group("Bushes_PH", parent);

            for (int i = 0; i < 34; i++)
            {
                // Along the inside of the walls; the south side is left open for the gate and checkpoint.
                int side = rng.Next(0, 3);
                float along = (float)(rng.NextDouble() * 40.0 - 20.0);
                Vector3 p;
                if (side == 0) p = new Vector3(along, 0, 21.6f);
                else if (side == 1) p = new Vector3(21.6f, 0, along);
                else p = new Vector3(-21.6f, 0, along);

                float s = 0.8f + (float)rng.NextDouble() * 0.7f;
                Material m = (i % 2 == 0) ? leaf : leafDark;
                ModelBuilder.Prim(PrimitiveType.Sphere, "Bush", t, p + new Vector3(0, 0.4f * s, 0), new Vector3(1.4f * s, 0.9f * s, 1.4f * s), m);
            }
        }
    }
}
