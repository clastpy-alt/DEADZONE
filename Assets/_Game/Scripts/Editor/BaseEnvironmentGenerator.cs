using System.IO;
using System.Text;
using Deadzone.Buildings;
using Deadzone.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Deadzone.EditorTools
{
    /// <summary>
    /// PHASE 02.56: professional military-base layout (asphalt roads with markings, sidewalks, detailed perimeter walls with
    /// gate and guard booths, watch towers, sandbags, jersey barriers, container yard, water tower, fuel tank, power line,
    /// parking bays, grass/dirt ground and damage details). Everything is built from Unity primitives with the shared
    /// Mil_* URP materials, then merged into one mesh per material per group (draw-call friendly, see PERFORMANCE_BUDGET.md).
    /// The generated content lives under one scene root ("Base Environment v2 [PLACEHOLDER]"). The old PHASE 01/02 environment
    /// objects that it replaces are DEACTIVATED, never deleted. Re-running does nothing when the root already exists;
    /// <c>rebuild</c> regenerates only that root.
    /// </summary>
    internal static class BaseEnvironmentGenerator
    {
        public const string RootName = "Base Environment v2 [PLACEHOLDER]";
        private const string MeshPrefix = "Env";

        public static readonly Vector3 OldWarehousePosition = new Vector3(0f, 0f, -12f);
        public static readonly Vector3 WarehousePosition = new Vector3(-19f, 0f, -4.5f);

        private static readonly string[] LegacyEnvironmentNames =
        {
            "Ground", "ConcretePad", "Road_Main", "Road_Cross", "Road_Outside", "Wall_N", "Wall_E", "Wall_W", "Wall_S_Left",
            "Wall_S_Right", "GuardPost_PH", "Barrier", "WatchTower_PH", "StreetLamp_PH", "Sandbags_PH", "Container_PH",
            "MilitaryCrate_PH",
        };

        // ------------------------------------------------------------------ entry point

        public static string Generate(bool rebuild)
        {
            string path = EditorPaths.ScenePath(SceneNames.Base);
            if (!File.Exists(path)) return "Base scene not found (run Setup Phase 01 first).";

            MilitaryMaterials.GenerateAll();

            Scene previousActive = SceneManager.GetActiveScene();
            Scene existing = SceneManager.GetSceneByPath(path);
            bool wasLoaded = existing.IsValid() && existing.isLoaded;
            Scene scene = wasLoaded ? existing : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            var report = new StringBuilder();

            try
            {
                SceneManager.SetActiveScene(scene);

                GameObject oldRoot = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (root.name == RootName) oldRoot = root;
                }

                if (oldRoot != null && !rebuild) return "Base environment v2 already present (kept).";
                if (oldRoot != null) Object.DestroyImmediate(oldRoot);

                report.AppendLine(DeactivateLegacy(scene) + " legacy object(s) deactivated");
                report.AppendLine(RelocateWarehouse(scene));

                Transform root2 = ModelBuilder.Group(RootName, null);
                int meshes = 0;
                meshes += Finish(Build(root2, "Ground", BuildGround), true);
                meshes += Finish(Build(root2, "Roads", BuildRoads), true);
                meshes += Finish(Build(root2, "Walls", BuildWalls), false);
                meshes += Finish(Build(root2, "Gate", BuildGate), false);
                meshes += Finish(Build(root2, "Towers", BuildTowers), false);
                meshes += Finish(Build(root2, "Barricades", BuildBarricades), false);
                meshes += Finish(Build(root2, "Yard", BuildYard), false);
                meshes += Finish(Build(root2, "Utilities", BuildUtilities), false);
                meshes += Finish(Build(root2, "Damage", BuildDamage), true);

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
                report.AppendLine("Base environment v2 built: " + meshes + " merged mesh(es).");
                return report.ToString().Trim();
            }
            finally
            {
                if (previousActive.IsValid() && previousActive.isLoaded) SceneManager.SetActiveScene(previousActive);
                if (!wasLoaded) EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static Transform Build(Transform root, string name, System.Action<Transform> builder)
        {
            Transform group = ModelBuilder.Group(name, root);
            builder(group);
            return group;
        }

        /// <summary>Merges one group into one mesh per material and applies static/shadow flags. Flat decals cast no shadows.</summary>
        private static int Finish(Transform group, bool flatDecals)
        {
            int count = MeshMerger.MergeGroup(group, MeshPrefix);
            const StaticEditorFlags flags = StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI | StaticEditorFlags.ReflectionProbeStatic;
            foreach (Transform child in group)
            {
                GameObjectUtility.SetStaticEditorFlags(child.gameObject, flags);
                var renderer = child.GetComponent<MeshRenderer>();
                if (renderer != null && flatDecals) renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
            GameObjectUtility.SetStaticEditorFlags(group.gameObject, flags);
            return count;
        }

        private static int DeactivateLegacy(Scene scene)
        {
            int count = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == "Environment [PLACEHOLDER]")
                {
                    foreach (Transform child in root.transform)
                    {
                        if (System.Array.IndexOf(LegacyEnvironmentNames, child.name) >= 0 && child.gameObject.activeSelf)
                        {
                            child.gameObject.SetActive(false);
                            count++;
                        }
                    }
                }
                else if (root.name == MilitaryBaseGenerator.PropsGroupName)
                {
                    foreach (Transform child in root.transform)
                    {
                        if (child.name == "Bushes_PH" && child.gameObject.activeSelf)
                        {
                            child.gameObject.SetActive(false);
                            count++;
                        }
                    }
                }
            }
            return count;
        }

        /// <summary>The Warehouse used to stand on the main road; moves it only while it still has the old default position.</summary>
        private static string RelocateWarehouse(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (BuildingSelectable selectable in root.GetComponentsInChildren<BuildingSelectable>(true))
                {
                    if (selectable.BuildingId != "warehouse") continue;
                    if (Vector3.Distance(selectable.transform.position, OldWarehousePosition) < 0.05f)
                    {
                        selectable.transform.position = WarehousePosition;
                        return "Warehouse moved off the main road to " + WarehousePosition;
                    }
                    return "Warehouse position kept (was edited by hand).";
                }
            }
            return "Warehouse not in scene.";
        }

        // ------------------------------------------------------------------ primitives

        private static Material M(string name) => MilitaryMaterials.Get(name);

        private static GameObject Box(Transform p, float x, float y, float z, float sx, float sy, float sz, string mat, float yaw = 0f, float pitch = 0f, float roll = 0f)
        {
            return ModelBuilder.Prim(PrimitiveType.Cube, "b", p, new Vector3(x, y, z), new Vector3(sx, sy, sz), M(mat), new Vector3(pitch, yaw, roll));
        }

        /// <summary>Cylinder given by diameter and height (Unity's cylinder is 1 wide and 2 tall at scale 1).</summary>
        private static GameObject Cyl(Transform p, float x, float y, float z, float dia, float height, string mat, float pitch = 0f, float yaw = 0f, float roll = 0f)
        {
            return ModelBuilder.Prim(PrimitiveType.Cylinder, "c", p, new Vector3(x, y, z), new Vector3(dia, height / 2f, dia), M(mat), new Vector3(pitch, yaw, roll));
        }

        private static GameObject Sph(Transform p, float x, float y, float z, float sx, float sy, float sz, string mat)
        {
            return ModelBuilder.Prim(PrimitiveType.Sphere, "s", p, new Vector3(x, y, z), new Vector3(sx, sy, sz), M(mat));
        }

        private static void Beam(Transform p, Vector3 a, Vector3 b, float thick, string mat)
        {
            Vector3 d = b - a;
            if (d.sqrMagnitude < 0.0001f) return;
            GameObject go = ModelBuilder.Prim(PrimitiveType.Cube, "beam", p, (a + b) * 0.5f, new Vector3(thick, thick, d.magnitude), M(mat));
            go.transform.localRotation = Quaternion.LookRotation(d.normalized);
        }

        // ------------------------------------------------------------------ ground

        private static void BuildGround(Transform g)
        {
            Box(g, 0, -0.5f, 0, 220, 1, 220, "Mil_Dirt");

            // Grass patches outside the compound; every patch gets its own height so overlapping patches never z-fight.
            var rng = new System.Random(5601);
            int placed = 0;
            for (int attempt = 0; attempt < 400 && placed < 64; attempt++)
            {
                float angle = (float)(rng.NextDouble() * Mathf.PI * 2.0);
                float radius = 29f + (float)rng.NextDouble() * 48f;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                if (Mathf.Abs(x) < 7f && z < 0f) continue; // keep the approach road clear
                float sx = 5f + (float)rng.NextDouble() * 10f;
                float sz = 4f + (float)rng.NextDouble() * 9f;
                float top = 0.02f + placed * 0.0004f;
                Box(g, x, top / 2f, z, sx, top, sz, placed % 3 == 0 ? "Mil_GrassDry" : "Mil_Grass", (float)rng.NextDouble() * 90f);
                placed++;
            }

            // Concrete base pad with expansion joints.
            Box(g, 0, 0.025f, 0, 47.4f, 0.05f, 47.4f, "Mil_Concrete");
            for (int i = -3; i <= 3; i++)
            {
                Box(g, i * 6f, 0.052f, 0, 0.07f, 0.006f, 47f, "Mil_ConcreteDark");
                Box(g, 0, 0.0525f, i * 6f, 47f, 0.006f, 0.07f, "Mil_ConcreteDark");
            }

            // Worn dirt along the inside of the walls (slightly above the pad).
            for (int i = 0; i < 12; i++)
            {
                int side = i % 4;
                float along = (float)(rng.NextDouble() * 38.0 - 19.0);
                float len = 3f + (float)rng.NextDouble() * 4f;
                float top = 0.06f + i * 0.0004f;
                switch (side)
                {
                    case 0: Box(g, along, top / 2f, 22.6f, len, top, 1.6f, "Mil_Dirt"); break;
                    case 1: Box(g, 22.6f, top / 2f, along, 1.6f, top, len, "Mil_Dirt"); break;
                    case 2: Box(g, -22.6f, top / 2f, along, 1.6f, top, len, "Mil_Dirt"); break;
                    default: if (Mathf.Abs(along) > 8f) Box(g, along, top / 2f, -22.6f, len, top, 1.6f, "Mil_Dirt"); break;
                }
            }
        }

        // ------------------------------------------------------------------ roads, markings, sidewalks, parking

        private static void BuildRoads(Transform g)
        {
            // Main road from far outside the gate to the HQ plaza; cross road slightly higher so the junction never z-fights.
            Box(g, 0, 0.04f, -27f, 6f, 0.08f, 74f, "Mil_Asphalt");
            Box(g, 0, 0.0405f, -2f, 30f, 0.081f, 4.5f, "Mil_Asphalt");

            // HQ plaza with a painted emblem.
            Box(g, 0, 0.045f, 9.6f, 15f, 0.09f, 6.4f, "Mil_ConcreteLight");
            Cyl(g, 0, 0.092f, 9.8f, 5.2f, 0.01f, "Mil_Gold");
            Cyl(g, 0, 0.097f, 9.8f, 4.6f, 0.01f, "Mil_PaintOliveDark");
            for (int i = 0; i < 4; i++)
            {
                Box(g, 0, 0.103f, 9.8f, 0.3f, 0.012f, 4.0f, "Mil_Gold", i * 45f);
            }

            // Edge lines and center dashes (yellow on the main road, white on the cross road).
            Box(g, -2.7f, 0.086f, -27f, 0.12f, 0.012f, 74f, "Mil_RoadWhite");
            Box(g, 2.7f, 0.086f, -27f, 0.12f, 0.012f, 74f, "Mil_RoadWhite");
            for (float z = -62f; z < 6f; z += 4.4f)
            {
                if (z > -5.2f && z < 1.2f) continue; // junction
                Box(g, 0, 0.088f, z + 1.1f, 0.16f, 0.012f, 2.2f, "Mil_Hazard");
            }
            for (float x = -14f; x < 14f; x += 4.4f)
            {
                if (Mathf.Abs(x + 1.1f) < 3.6f) continue;
                Box(g, x + 1.1f, 0.088f, -2f, 2.2f, 0.012f, 0.14f, "Mil_RoadWhite");
            }
            // Stop line at the gate and a pedestrian crossing at the HQ plaza.
            Box(g, 1.4f, 0.088f, -20.5f, 2.6f, 0.012f, 0.35f, "Mil_RoadWhite");
            for (int i = 0; i < 6; i++)
            {
                Box(g, -2.0f + i * 0.8f, 0.093f, 6.2f, 0.45f, 0.012f, 1.6f, "Mil_RoadWhite");
            }

            // Sidewalks with a darker curb face along the main road inside the compound.
            for (int side = -1; side <= 1; side += 2)
            {
                Box(g, side * 3.85f, 0.1f, -9f, 1.3f, 0.2f, 33f, "Mil_ConcreteLight");
                Box(g, side * 3.22f, 0.1f, -9f, 0.14f, 0.2f, 33f, "Mil_ConcreteDark");
            }

            // Vehicle parking bays beside the gate (west: trucks, east: armour) with painted slots.
            Box(g, -8.5f, 0.045f, -19.5f, 10f, 0.09f, 6.5f, "Mil_ConcreteDark");
            Box(g, 8.5f, 0.045f, -19.5f, 10f, 0.09f, 6.5f, "Mil_ConcreteDark");
            for (int i = 0; i <= 3; i++)
            {
                Box(g, -12.5f + i * 2.6f + 0.2f, 0.096f, -19.5f, 0.1f, 0.012f, 5.4f, "Mil_RoadWhite");
                Box(g, 4.5f + i * 2.6f + 0.2f, 0.096f, -19.5f, 0.1f, 0.012f, 5.4f, "Mil_RoadWhite");
            }

            // Warehouse apron, container yard slab and helipad safety square.
            Box(g, WarehousePosition.x, 0.045f, WarehousePosition.z - 2.4f, 8f, 0.09f, 3.2f, "Mil_ConcreteDark");
            Box(g, 18f, 0.045f, -19.5f, 11f, 0.09f, 10f, "Mil_ConcreteDark");
            Box(g, 16f, 0.045f, 12f, 12.5f, 0.09f, 12.5f, "Mil_ConcreteDark");

            // Outside: dirt shoulder fades the asphalt into the ground.
            Box(g, -4.4f, 0.022f, -50f, 1.6f, 0.044f, 30f, "Mil_Dirt");
            Box(g, 4.4f, 0.022f, -50f, 1.6f, 0.044f, 30f, "Mil_Dirt");
        }

        // ------------------------------------------------------------------ perimeter walls

        private static void BuildWalls(Transform g)
        {
            WallRun(g, true, 24f, -24.5f, 24.5f);                 // north
            WallRun(g, false, -24f, -24.5f, 24.5f);               // west
            WallRun(g, true, -24f, -24.5f, -4.2f);                // south, left of the gate
            WallRun(g, true, -24f, 4.2f, 24.5f);                  // south, right of the gate
            WallRun(g, false, 24f, -24.5f, -9.5f);                // east, up to the damaged section
            WallRun(g, false, 24f, -4.5f, 24.5f);                 // east, after the damaged section
            BrokenWall(g, 24f, -9.5f, -4.5f);                           // east, collapsed top
        }

        /// <summary>Wall along X (alongX) at constant z, or along Z at constant x.</summary>
        private static void WallRun(Transform g, bool alongX, float fixedCoord, float a0, float a1)
        {
            float len = a1 - a0;
            float mid = (a0 + a1) / 2f;
            Vector3 P(float along, float y) => alongX ? new Vector3(along, y, fixedCoord) : new Vector3(fixedCoord, y, along);
            Vector3 S(float l, float h, float t) => alongX ? new Vector3(l, h, t) : new Vector3(t, h, l);

            Place(g, P(mid, 1.2f), S(len, 2.4f, 1.0f), "Mil_Concrete");
            Place(g, P(mid, 0.25f), S(len, 0.5f, 1.3f), "Mil_ConcreteDark");
            Place(g, P(mid, 2.47f), S(len, 0.14f, 1.25f), "Mil_ConcreteLight");

            // Expansion joints every 4 units, pilasters every 8.
            for (float a = Mathf.Ceil(a0 / 4f) * 4f; a < a1; a += 4f)
            {
                if (a - a0 < 0.3f || a1 - a < 0.3f) continue;
                bool pilaster = Mathf.Abs(Mathf.Repeat(a, 8f)) < 0.01f;
                if (pilaster) Place(g, P(a, 1.45f), S(1.4f, 2.9f, 1.55f), "Mil_ConcreteLight");
                else Place(g, P(a, 1.2f), S(0.07f, 2.3f, 1.03f), "Mil_ConcreteDark");
            }

            // Razor-wire frame on top: posts every 3 units and two rails.
            for (float a = a0 + 1.5f; a < a1; a += 3f)
            {
                Place(g, P(a, 2.9f), S(0.07f, 0.7f, 0.07f), "Mil_SteelDark");
            }
            Place(g, P(mid, 3.15f), S(len, 0.04f, 0.04f), "Mil_SteelDark");
            Place(g, P(mid, 2.85f), S(len, 0.04f, 0.04f), "Mil_SteelDark");
        }

        private static void Place(Transform g, Vector3 pos, Vector3 scale, string mat)
        {
            Box(g, pos.x, pos.y, pos.z, scale.x, scale.y, scale.z, mat);
        }

        /// <summary>Collapsed wall segment: low base, jagged blocks, rebar and rubble on both sides.</summary>
        private static void BrokenWall(Transform g, float x, float z0, float z1)
        {
            float mid = (z0 + z1) / 2f;
            Box(g, x, 0.25f, mid, 1.3f, 0.5f, z1 - z0, "Mil_ConcreteDark");
            Box(g, x, 0.75f, mid, 1.0f, 0.9f, z1 - z0, "Mil_ConcreteDamaged");
            Box(g, x, 1.45f, z0 + 0.9f, 1.0f, 0.9f, 1.6f, "Mil_ConcreteDamaged", 0, 0, 4f);
            Box(g, x, 1.2f, z1 - 1.2f, 1.0f, 0.55f, 1.4f, "Mil_ConcreteDamaged", 0, 0, -6f);
            Box(g, x, 1.1f, mid, 0.9f, 0.4f, 1.0f, "Mil_ConcreteDamaged");
            for (int i = 0; i < 5; i++)
            {
                float z = z0 + 0.7f + i * ((z1 - z0 - 1.4f) / 4f);
                Cyl(g, x + (i % 2 == 0 ? 0.2f : -0.2f), 1.9f + (i % 3) * 0.12f, z, 0.05f, 1.0f, "Mil_Rust", (i - 2) * 9f, 0f, (i % 2 == 0 ? 8f : -8f));
            }
            var rng = new System.Random(77);
            for (int i = 0; i < 9; i++)
            {
                float s = 0.35f + (float)rng.NextDouble() * 0.6f;
                float side = i % 2 == 0 ? -1.6f : 1.6f;
                Box(g, x + side + (float)(rng.NextDouble() - 0.5) * 0.8f, s * 0.4f, mid + (float)(rng.NextDouble() - 0.5) * 4.6f,
                    s, s * 0.8f, s * 1.1f, i % 3 == 0 ? "Mil_ConcreteLight" : "Mil_ConcreteDamaged", (float)rng.NextDouble() * 180f, 0, (float)(rng.NextDouble() - 0.5) * 18f);
            }
        }

        // ------------------------------------------------------------------ gate, booths, barrier arm

        private static void BuildGate(Transform g)
        {
            // Gate pillars with lamps.
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * 4.9f;
                Box(g, x, 1.7f, -24f, 1.8f, 3.4f, 1.8f, "Mil_ConcreteLight");
                Box(g, x, 3.5f, -24f, 2.1f, 0.2f, 2.1f, "Mil_ConcreteDark");
                Sph(g, x, 3.85f, -24f, 0.55f, 0.55f, 0.55f, "Mil_WindowGlow");
                Box(g, x, 0.25f, -24f, 2.1f, 0.5f, 2.1f, "Mil_ConcreteDark");
            }
            // Gate sign beam.
            Box(g, 0, 3.55f, -24f, 8.0f, 0.5f, 0.5f, "Mil_PaintOliveDark");
            Box(g, 0, 3.55f, -24.27f, 7.0f, 0.3f, 0.06f, "Mil_Hazard");

            // Open sliding gate leaf behind the right wall, with frame and brace.
            Box(g, 8.6f, 1.15f, -22.9f, 5.2f, 2.0f, 0.12f, "Mil_SteelGray");
            Box(g, 8.6f, 2.18f, -22.9f, 5.3f, 0.12f, 0.16f, "Mil_SteelDark");
            Box(g, 8.6f, 0.2f, -22.9f, 5.3f, 0.12f, 0.16f, "Mil_SteelDark");
            Beam(g, new Vector3(6.1f, 0.3f, -22.85f), new Vector3(11.1f, 2.1f, -22.85f), 0.1f, "Mil_SteelDark");

            // Barrier arm raised (open), red/white segments.
            Box(g, -3.0f, 0.55f, -22.3f, 0.5f, 1.1f, 0.5f, "Mil_SteelDark");
            Vector3 hinge = new Vector3(-3.0f, 1.0f, -22.3f);
            Vector3 tip = hinge + new Vector3(2.9f * Mathf.Cos(55f * Mathf.Deg2Rad), 2.9f * Mathf.Sin(55f * Mathf.Deg2Rad), 0f);
            for (int i = 0; i < 6; i++)
            {
                Vector3 a = Vector3.Lerp(hinge, tip, i / 6f);
                Vector3 b = Vector3.Lerp(hinge, tip, (i + 1) / 6f);
                Beam(g, a, b, 0.14f, i % 2 == 0 ? "Mil_WarningRed" : "Mil_RoadWhite");
            }

            // Guard booths on both sides of the road.
            Booth(g, -7.6f, -21.4f);
            Booth(g, 7.6f, -21.4f);
        }

        private static void Booth(Transform g, float x, float z)
        {
            Box(g, x, 0.12f, z, 3.0f, 0.24f, 3.0f, "Mil_ConcreteDark");
            Box(g, x, 1.5f, z, 2.4f, 2.5f, 2.4f, "Mil_PaintOlive");
            Box(g, x, 1.9f, z - 1.23f, 1.8f, 0.8f, 0.05f, "Mil_Glass");
            Box(g, x - 1.23f, 1.9f, z, 0.05f, 0.8f, 1.8f, "Mil_Glass");
            Box(g, x + 1.23f, 1.9f, z, 0.05f, 0.8f, 1.8f, "Mil_Glass");
            Box(g, x, 2.85f, z, 3.0f, 0.2f, 3.0f, "Mil_SteelDark");
            Box(g, x, 3.0f, z - 0.5f, 0.6f, 0.12f, 0.3f, "Mil_WarningRed");
            Box(g, x + (x > 0 ? 0.6f : -0.6f), 0.9f, z - 1.25f, 0.8f, 1.5f, 0.05f, "Mil_PaintOliveDark");
            Box(g, x, 2.2f, z + 1.3f, 0.8f, 0.5f, 0.45f, "Mil_SteelGray"); // AC unit
            SandbagLine(g, new Vector3(x - 1.5f, 0, z - 2.0f), new Vector3(x + 1.5f, 0, z - 2.0f), 2);
        }

        // ------------------------------------------------------------------ watch towers

        private static void BuildTowers(Transform g)
        {
            Tower(g, -24f, 24f);
            Tower(g, 24f, 24f);
            Tower(g, -24f, -24f);
            Tower(g, 24f, -24f);
        }

        private static void Tower(Transform g, float x, float z)
        {
            const float h = 6.4f;
            for (int i = 0; i < 4; i++)
            {
                float lx = x + (i % 2 == 0 ? -1.1f : 1.1f);
                float lz = z + (i < 2 ? -1.1f : 1.1f);
                Box(g, lx, h / 2f, lz, 0.3f, h, 0.3f, "Mil_SteelGray");
            }
            for (int level = 0; level < 2; level++)
            {
                float y0 = 0.5f + level * 2.9f, y1 = y0 + 2.9f;
                Beam(g, new Vector3(x - 1.1f, y0, z - 1.1f), new Vector3(x + 1.1f, y1, z - 1.1f), 0.12f, "Mil_SteelDark");
                Beam(g, new Vector3(x + 1.1f, y0, z - 1.1f), new Vector3(x - 1.1f, y1, z - 1.1f), 0.12f, "Mil_SteelDark");
                Beam(g, new Vector3(x - 1.1f, y0, z + 1.1f), new Vector3(x - 1.1f, y1, z - 1.1f), 0.12f, "Mil_SteelDark");
                Beam(g, new Vector3(x + 1.1f, y0, z + 1.1f), new Vector3(x + 1.1f, y1, z - 1.1f), 0.12f, "Mil_SteelDark");
            }
            Box(g, x, 3.4f, z, 2.6f, 0.12f, 2.6f, "Mil_SteelDark");
            Box(g, x, h + 0.12f, z, 3.4f, 0.25f, 3.4f, "Mil_SteelDark");
            // railing + cabin
            Box(g, x, h + 0.7f, z, 2.6f, 1.6f, 2.6f, "Mil_PaintOlive");
            Box(g, x, h + 0.95f, z - 1.33f, 2.0f, 0.65f, 0.05f, "Mil_Glass");
            Box(g, x - 1.33f, h + 0.95f, z, 0.05f, 0.65f, 2.0f, "Mil_Glass");
            Box(g, x + 1.33f, h + 0.95f, z, 0.05f, 0.65f, 2.0f, "Mil_Glass");
            Box(g, x, h + 1.65f, z, 3.4f, 0.2f, 3.4f, "Mil_PaintOliveDark");
            // searchlight
            Cyl(g, x - 1.2f, h + 1.95f, z - 1.2f, 0.45f, 0.5f, "Mil_SteelGray", 55f);
            Cyl(g, x - 1.2f, h + 1.95f, z - 1.2f, 0.38f, 0.52f, "Mil_WindowGlow", 55f);
            // ladder on the inner side
            float ix = x > 0 ? x - 1.25f : x + 1.25f;
            float iz = z > 0 ? z - 1.25f : z + 1.25f;
            Box(g, ix - 0.25f, 3.2f, iz, 0.06f, 6.4f, 0.06f, "Mil_SteelDark");
            Box(g, ix + 0.25f, 3.2f, iz, 0.06f, 6.4f, 0.06f, "Mil_SteelDark");
            for (int i = 0; i < 12; i++) Box(g, ix, 0.4f + i * 0.5f, iz, 0.5f, 0.05f, 0.05f, "Mil_SteelDark");
            // sandbags at the tower base
            SandbagLine(g, new Vector3(x - 2.2f, 0, z - 2.2f), new Vector3(x + 2.2f, 0, z - 2.2f), 2);
        }

        // ------------------------------------------------------------------ sandbags, jersey barriers

        private static void SandbagLine(Transform g, Vector3 a, Vector3 b, int layers)
        {
            Vector3 d = b - a;
            float length = d.magnitude;
            int count = Mathf.Max(1, Mathf.RoundToInt(length / 1.0f));
            float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg + 90f;
            for (int layer = 0; layer < layers; layer++)
            {
                for (int i = 0; i < count; i++)
                {
                    float t = (i + 0.5f + (layer % 2) * 0.5f) / count;
                    if (t > 1f) continue;
                    Vector3 p = a + d * t;
                    Box(g, p.x, 0.17f + layer * 0.3f, p.z, 1.0f, 0.3f, 0.5f, "Mil_Sandbag", yaw + (i % 2 == 0 ? 2f : -3f));
                }
            }
        }

        private static void SandbagArc(Transform g, Vector3 center, float radius, float fromDeg, float toDeg, int layers)
        {
            float arc = Mathf.Abs(toDeg - fromDeg) * Mathf.Deg2Rad * radius;
            int count = Mathf.Max(3, Mathf.RoundToInt(arc / 1.0f));
            for (int layer = 0; layer < layers; layer++)
            {
                for (int i = 0; i < count; i++)
                {
                    float t = (i + 0.5f + (layer % 2) * 0.5f) / count;
                    if (t > 1f) continue;
                    float deg = Mathf.Lerp(fromDeg, toDeg, t);
                    float rad = deg * Mathf.Deg2Rad;
                    Box(g, center.x + Mathf.Cos(rad) * radius, 0.17f + layer * 0.3f, center.z + Mathf.Sin(rad) * radius,
                        1.0f, 0.3f, 0.5f, "Mil_Sandbag", -deg + 90f);
                }
            }
        }

        private static void JerseyBarrier(Transform g, float x, float z, float yaw)
        {
            // Two boxes make the classic profile; hazard bands on the top.
            Transform t = ModelBuilder.Group("jb", g, new Vector3(x, 0, z), new Vector3(0, yaw, 0));
            Box(t, 0, 0.2f, 0, 3.0f, 0.4f, 0.8f, "Mil_ConcreteLight");
            Box(t, 0, 0.7f, 0, 3.0f, 0.6f, 0.42f, "Mil_ConcreteLight");
            Box(t, -1.2f, 0.95f, 0, 0.4f, 0.12f, 0.44f, "Mil_Hazard");
            Box(t, 1.2f, 0.95f, 0, 0.4f, 0.12f, 0.44f, "Mil_Hazard");
        }

        private static void BuildBarricades(Transform g)
        {
            // Chicane on the approach road outside the gate.
            JerseyBarrier(g, -1.5f, -30f, 0f);
            JerseyBarrier(g, 1.5f, -35f, 0f);
            JerseyBarrier(g, -1.5f, -40f, 0f);
            for (int i = 0; i < 3; i++)
            {
                JerseyBarrier(g, -4.4f, -28f - i * 3.1f, 90f);
                JerseyBarrier(g, 4.4f, -28f - i * 3.1f, 90f);
            }

            // Machine-gun nests: sandbag arcs inside the corners and beside the gate road.
            SandbagArc(g, new Vector3(-19f, 0, 19f), 2.0f, 180f, 270f, 3);
            SandbagArc(g, new Vector3(19f, 0, 19f), 2.0f, 270f, 360f, 3);
            SandbagLine(g, new Vector3(-5.2f, 0, -26.8f), new Vector3(-1.0f, 0, -26.8f), 3);
            SandbagLine(g, new Vector3(1.0f, 0, -26.8f), new Vector3(5.2f, 0, -26.8f), 3);
            SandbagLine(g, new Vector3(20.8f, 0, 2f), new Vector3(20.8f, 0, 6f), 2);
            SandbagLine(g, new Vector3(-20.8f, 0, 6f), new Vector3(-20.8f, 0, 2f), 2);

            // Concrete barriers shielding the HQ plaza.
            JerseyBarrier(g, -6.5f, 6.4f, 0f);
            JerseyBarrier(g, 6.5f, 6.4f, 0f);
        }

        // ------------------------------------------------------------------ container yard, crates, fence

        private static void BuildYard(Transform g)
        {
            Container(g, 18f, 0f, -16.6f, 0f, "Mil_PaintOlive");
            Container(g, 18f, 0f, -19.4f, 0f, "Mil_Rust");
            Container(g, 18.4f, 2.6f, -19.4f, 3f, "Mil_PaintSteelBlue");
            Container(g, 19.8f, 0f, -22.1f, -90f, "Mil_SteelGray", 3.0f);

            // Yard fence with an entrance gap.
            Fence(g, new Vector3(13f, 0, -14.8f), new Vector3(23f, 0, -14.8f));
            Fence(g, new Vector3(13f, 0, -14.8f), new Vector3(13f, 0, -18f));
            Fence(g, new Vector3(13f, 0, -21f), new Vector3(13f, 0, -23f));

            // Crate stacks on pallets: warehouse side and beside the steel factory.
            CrateStack(g, WarehousePosition.x + 4.2f, WarehousePosition.z - 1.2f, 3);
            CrateStack(g, WarehousePosition.x + 5.5f, WarehousePosition.z - 3.4f, 2);
            CrateStack(g, 15f, -5.9f, 3);

            // Fuel tank with cradle and pump beside the generator corner.
            Cyl(g, -14.5f, 1.15f, 22f, 1.8f, 4.2f, "Mil_PaintOlive", 0f, 0f, 90f);
            Box(g, -15.8f, 0.3f, 22f, 0.35f, 0.6f, 1.6f, "Mil_ConcreteDark");
            Box(g, -13.2f, 0.3f, 22f, 0.35f, 0.6f, 1.6f, "Mil_ConcreteDark");
            Box(g, -14.5f, 2.15f, 22f, 0.9f, 0.18f, 0.9f, "Mil_SteelGray");
            Box(g, -14.5f, 1.15f, 21.07f, 3.2f, 0.18f, 0.03f, "Mil_Hazard");
            Box(g, -11.8f, 0.5f, 21.2f, 0.7f, 1.0f, 0.5f, "Mil_SteelDark");

            // Flagpole and windsock.
            Cyl(g, -6.5f, 3.5f, 10.5f, 0.14f, 7.0f, "Mil_MetalLight");
            Box(g, -5.55f, 6.35f, 10.5f, 1.9f, 1.1f, 0.04f, "Mil_PaintOliveDark");
            Box(g, -5.55f, 6.55f, 10.5f, 1.9f, 0.2f, 0.05f, "Mil_Gold");
            Box(g, -6.5f, 0.15f, 10.5f, 0.6f, 0.3f, 0.6f, "Mil_ConcreteDark");
            Cyl(g, 21.6f, 1.6f, 8.6f, 0.08f, 3.2f, "Mil_SteelGray");
            Cyl(g, 21.1f, 3.1f, 8.6f, 0.5f, 0.45f, "Mil_Hazard", 0f, 0f, 90f);
            Cyl(g, 20.65f, 3.1f, 8.6f, 0.4f, 0.45f, "Mil_RoadWhite", 0f, 0f, 90f);
            Cyl(g, 20.25f, 3.1f, 8.6f, 0.3f, 0.35f, "Mil_Hazard", 0f, 0f, 90f);
        }

        private static void Container(Transform g, float x, float y, float z, float yaw, string mat, float length = 6f)
        {
            Transform t = ModelBuilder.Group("ct", g, new Vector3(x, y, z), new Vector3(0, yaw, 0));
            Box(t, 0, 1.3f, 0, length, 2.5f, 2.4f, mat);
            // corrugation ribs on both long sides, corner posts and door end
            for (int i = 0; i < Mathf.FloorToInt(length / 0.5f) - 1; i++)
            {
                float rx = -length / 2f + 0.5f + i * 0.5f;
                Box(t, rx, 1.3f, -1.22f, 0.12f, 2.3f, 0.04f, mat);
                Box(t, rx, 1.3f, 1.22f, 0.12f, 2.3f, 0.04f, mat);
            }
            for (int sx = -1; sx <= 1; sx += 2)
            {
                for (int sz = -1; sz <= 1; sz += 2) Box(t, sx * (length / 2f - 0.05f), 1.3f, sz * 1.15f, 0.14f, 2.6f, 0.14f, "Mil_SteelDark");
            }
            Box(t, length / 2f + 0.01f, 1.3f, 0, 0.04f, 2.2f, 2.0f, "Mil_SteelDark");
            Box(t, length / 2f + 0.03f, 1.3f, 0, 0.04f, 2.2f, 0.05f, "Mil_MetalLight");
        }

        private static void CrateStack(Transform g, float x, float z, int count)
        {
            Box(g, x, 0.08f, z, 1.5f, 0.16f, 1.5f, "Mil_Wood");
            for (int i = 0; i < count; i++)
            {
                float s = 1.1f - i * 0.1f;
                Box(g, x + i * 0.05f, 0.16f + s / 2f + i * 0.9f, z, s, 0.85f, s, i % 2 == 0 ? "Mil_PaintOliveDark" : "Mil_Fabric", i * 12f);
                Box(g, x + i * 0.05f, 0.16f + s / 2f + i * 0.9f, z, s + 0.04f, 0.1f, s + 0.04f, "Mil_Hazard", i * 12f);
            }
        }

        private static void Fence(Transform g, Vector3 from, Vector3 to)
        {
            Vector3 d = to - from;
            float len = d.magnitude;
            int segments = Mathf.Max(1, Mathf.RoundToInt(len / 2.5f));
            for (int i = 0; i <= segments; i++)
            {
                Vector3 p = from + d * (i / (float)segments);
                Cyl(g, p.x, 0.95f, p.z, 0.12f, 1.9f, "Mil_SteelGray");
            }
            for (int i = 0; i < 3; i++)
            {
                Beam(g, from + new Vector3(0, 0.4f + i * 0.6f, 0), to + new Vector3(0, 0.4f + i * 0.6f, 0), 0.04f, "Mil_SteelDark");
            }
            Beam(g, from + new Vector3(0, 0.3f, 0), to + new Vector3(0, 1.7f, 0), 0.025f, "Mil_SteelDark");
        }

        // ------------------------------------------------------------------ utilities: lamps, water tower, power line

        private static void BuildUtilities(Transform g)
        {
            // Street lamps along the main road (inside) and two outside the gate.
            for (int i = 0; i < 4; i++)
            {
                float z = -14f + i * 7f;
                if (Mathf.Abs(z + 2f) < 3f) z += 3.5f;
                Lamp(g, -4.9f, z, 1f);
                Lamp(g, 4.9f, z, -1f);
            }
            Lamp(g, -4.4f, -44f, 1f);
            Lamp(g, 4.4f, -52f, -1f);

            WaterTower(g, 20.5f, -6.5f);

            // Power line along the west wall outside: wooden poles, cross arms, insulators, sagging wires.
            float[] zs = { 32f, 20f, 8f, -4f, -16f, -28f, -40f };
            for (int i = 0; i < zs.Length; i++)
            {
                float z = zs[i];
                float x = -28.5f;
                Cyl(g, x, 4.5f, z, 0.34f, 9f, "Mil_Wood");
                Box(g, x, 8.4f, z, 3.0f, 0.18f, 0.18f, "Mil_Wood");
                Box(g, x, 7.7f, z, 2.2f, 0.14f, 0.14f, "Mil_Wood");
                for (int k = -1; k <= 1; k++)
                {
                    Cyl(g, x + k * 1.2f, 8.65f, z, 0.12f, 0.25f, "Mil_MetalLight");
                }
                if (i == 3) Cyl(g, x + 0.65f, 6.6f, z, 0.7f, 1.1f, "Mil_SteelGray"); // transformer
                if (i < zs.Length - 1)
                {
                    float nz = zs[i + 1];
                    for (int k = -1; k <= 1; k++)
                    {
                        Vector3 a = new Vector3(x + k * 1.2f, 8.72f, z);
                        Vector3 b = new Vector3(x + k * 1.2f, 8.72f, nz);
                        Vector3 mid = (a + b) * 0.5f + Vector3.down * 0.45f;
                        Beam(g, a, mid, 0.035f, "Mil_SteelDark");
                        Beam(g, mid, b, 0.035f, "Mil_SteelDark");
                    }
                }
            }
        }

        private static void Lamp(Transform g, float x, float z, float towardRoad)
        {
            Box(g, x, 0.15f, z, 0.45f, 0.3f, 0.45f, "Mil_ConcreteDark");
            Cyl(g, x, 2.4f, z, 0.14f, 4.6f, "Mil_SteelDark");
            Beam(g, new Vector3(x, 4.5f, z), new Vector3(x + towardRoad * 0.9f, 4.7f, z), 0.1f, "Mil_SteelDark");
            Box(g, x + towardRoad * 1.0f, 4.68f, z, 0.7f, 0.1f, 0.3f, "Mil_SteelGray");
            Box(g, x + towardRoad * 1.0f, 4.6f, z, 0.6f, 0.03f, 0.22f, "Mil_WindowGlow");
        }

        private static void WaterTower(Transform g, float x, float z)
        {
            for (int i = 0; i < 4; i++)
            {
                float lx = x + (i % 2 == 0 ? -1.3f : 1.3f);
                float lz = z + (i < 2 ? -1.3f : 1.3f);
                Box(g, lx, 2.5f, lz, 0.25f, 5f, 0.25f, "Mil_SteelGray");
            }
            for (int level = 0; level < 2; level++)
            {
                float y0 = 0.4f + level * 2.2f, y1 = y0 + 2.2f;
                Beam(g, new Vector3(x - 1.3f, y0, z - 1.3f), new Vector3(x + 1.3f, y1, z - 1.3f), 0.1f, "Mil_SteelDark");
                Beam(g, new Vector3(x + 1.3f, y0, z - 1.3f), new Vector3(x - 1.3f, y1, z - 1.3f), 0.1f, "Mil_SteelDark");
                Beam(g, new Vector3(x - 1.3f, y0, z - 1.3f), new Vector3(x - 1.3f, y1, z + 1.3f), 0.1f, "Mil_SteelDark");
            }
            Box(g, x, 5.05f, z, 3.2f, 0.2f, 3.2f, "Mil_SteelDark");
            Cyl(g, x, 6.3f, z, 3.0f, 2.3f, "Mil_PaintSteelBlue");
            Cyl(g, x, 6.3f, z, 3.06f, 0.22f, "Mil_Hazard");
            Sph(g, x, 7.55f, z, 3.0f, 0.9f, 3.0f, "Mil_PaintSteelBlue");
            Box(g, x - 1.45f, 2.6f, z - 1.55f, 0.06f, 5.2f, 0.06f, "Mil_SteelDark");
            Box(g, x - 1.15f, 2.6f, z - 1.55f, 0.06f, 5.2f, 0.06f, "Mil_SteelDark");
            for (int i = 0; i < 10; i++) Box(g, x - 1.3f, 0.5f + i * 0.5f, z - 1.55f, 0.35f, 0.05f, 0.05f, "Mil_SteelDark");
        }

        // ------------------------------------------------------------------ damage details (flat decals + small debris)

        private static void BuildDamage(Transform g)
        {
            var rng = new System.Random(9001);

            // Cracks in the concrete pad: short zig-zag chains of thin dark strips.
            Vector2[] crackStarts = { new Vector2(-6f, 20f), new Vector2(14f, 0.6f), new Vector2(-18f, -9.5f), new Vector2(5f, -8f), new Vector2(18f, 18f), new Vector2(-3f, 3.5f) };
            foreach (Vector2 s in crackStarts)
            {
                float x = s.x, z = s.y, yaw = (float)rng.NextDouble() * 180f;
                for (int i = 0; i < 4; i++)
                {
                    float len = 0.9f + (float)rng.NextDouble() * 0.9f;
                    float rad = yaw * Mathf.Deg2Rad;
                    Box(g, x + Mathf.Sin(rad) * len / 2f, 0.056f, z + Mathf.Cos(rad) * len / 2f, 0.06f, 0.008f, len, "Mil_Stain", yaw);
                    x += Mathf.Sin(rad) * len;
                    z += Mathf.Cos(rad) * len;
                    yaw += (i % 2 == 0 ? 1f : -1f) * (20f + (float)rng.NextDouble() * 35f);
                }
            }

            // Oil stains below vehicles and the generator, scorch marks near the broken wall.
            Cyl(g, -5.4f, 0.1f, -19.5f, 2.4f, 0.01f, "Mil_Stain");
            Cyl(g, 5.4f, 0.101f, -20.3f, 2.0f, 0.01f, "Mil_Stain");
            Cyl(g, -8f, 0.056f, 22f, 2.2f, 0.01f, "Mil_Stain");
            Cyl(g, 20.2f, 0.057f, -7f, 3.4f, 0.01f, "Mil_Stain");
            Cyl(g, 21.3f, 0.058f, -9.8f, 1.6f, 0.012f, "Mil_Stain");

            // Debris and a toppled lamp pole near the broken wall (inside), plus loose rubble along the east side.
            Cyl(g, 20.5f, 0.2f, -2.6f, 0.14f, 4.0f, "Mil_SteelDark", 90f, 18f, 0f);
            Box(g, 19.2f, 0.12f, -1.1f, 0.7f, 0.1f, 0.3f, "Mil_SteelGray", 25f);
            for (int i = 0; i < 10; i++)
            {
                float s = 0.25f + (float)rng.NextDouble() * 0.5f;
                Box(g, 20f + (float)rng.NextDouble() * 3f, s * 0.4f, -10f + (float)rng.NextDouble() * 7f, s, s * 0.7f, s * 1.1f,
                    i % 2 == 0 ? "Mil_ConcreteDamaged" : "Mil_ConcreteLight", (float)rng.NextDouble() * 180f);
            }
            // Hazard barricade across the breach.
            JerseyBarrier(g, 21.2f, -10.5f, 90f);
            JerseyBarrier(g, 21.2f, -3.5f, 90f);
        }
    }
}
