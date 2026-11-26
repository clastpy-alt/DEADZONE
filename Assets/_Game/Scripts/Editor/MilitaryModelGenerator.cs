using System.Collections.Generic;
using System.Text;
using Deadzone.Buildings;
using UnityEditor;
using UnityEngine;

namespace Deadzone.EditorTools
{
    /// <summary>
    /// PHASE 02.5 (02.54/02.55): detailed military building models composed in Unity (Blender was not available, see
    /// BLENDER_PIPELINE.md). Each prefab gets a generated "Mil_Visual" child with a core group (always visible) and seven tier
    /// groups (Tier1..Tier7, switched by BuildingVisualTiers). Parts are merged into one mesh per material per group.
    /// Id, level, collider, selection ring and saves are untouched; the old primitive visuals are only DEACTIVATED, never deleted.
    /// Re-running without 'rebuild' skips buildings that already have Mil_Visual.
    /// </summary>
    internal static partial class MilitaryModelGenerator
    {
        public const string VisualName = "Mil_Visual";
        private const float Y0 = 0.2f; // top of the foundation slab

        // One builder per building (HQ lives in this file, the other six in MilitaryModelBuildings.cs).
        private static readonly Dictionary<string, System.Action<Ctx>> Builders = new Dictionary<string, System.Action<Ctx>>
        {
            { "headquarters", BuildHeadquarters },
            { "barracks", BuildBarracks },
            { "hospital", BuildHospital },
            { "research_lab", BuildResearchLab },
            { "food_farm", BuildFoodFarm },
            { "steel_factory", BuildSteelFactory },
            { "warehouse", BuildWarehouse },
        };

        private sealed class Ctx
        {
            public Transform Core;
            public Transform[] Tier = new Transform[BuildingVisualTiers.TierCount];
        }

        public static string Generate(bool rebuild)
        {
            MilitaryMaterials.GenerateAll();
            var report = new StringBuilder();

            foreach (BuildingPrefabGenerator.Spec spec in BuildingPrefabGenerator.Specs)
            {
                if (!Builders.TryGetValue(spec.Id, out System.Action<Ctx> builder)) continue;

                string path = BuildingPrefabGenerator.PrefabPath(spec);
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
                {
                    report.AppendLine(spec.FileName + ": prefab missing (run Setup Phase 01)");
                    continue;
                }

                GameObject contents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Transform existing = contents.transform.Find(VisualName);
                    if (existing != null && !rebuild)
                    {
                        report.AppendLine(spec.FileName + ": already modelled (kept)");
                        continue;
                    }

                    bool firstTime = existing == null;
                    if (existing != null) Object.DestroyImmediate(existing.gameObject);

                    if (firstTime) HideLegacyVisuals(contents.transform);

                    Transform visual = ModelBuilder.Group(VisualName, contents.transform);
                    var ctx = new Ctx { Core = ModelBuilder.Group("Core", visual) };
                    for (int i = 0; i < ctx.Tier.Length; i++) ctx.Tier[i] = ModelBuilder.Group("Tier" + (i + 1), visual);

                    builder(ctx);

                    int meshes = MeshMerger.MergeGroup(ctx.Core, spec.FileName);
                    var tierObjects = new GameObject[ctx.Tier.Length];
                    for (int i = 0; i < ctx.Tier.Length; i++)
                    {
                        meshes += MeshMerger.MergeGroup(ctx.Tier[i], spec.FileName);
                        tierObjects[i] = ctx.Tier[i].gameObject;
                        ctx.Tier[i].gameObject.SetActive(i == 0); // edit-mode default; BuildingVisualTiers decides at runtime
                    }

                    var tiers = contents.GetComponent<BuildingVisualTiers>();
                    if (tiers == null) tiers = contents.AddComponent<BuildingVisualTiers>();
                    tiers.Configure(tierObjects);

                    PrefabUtility.SaveAsPrefabAsset(contents, path);
                    report.AppendLine(spec.FileName + ": modelled (" + meshes + " merged meshes)");
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return report.ToString().Trim();
        }

        /// <summary>Deactivates the old PHASE 01/02 primitive parts. Foundation and SelectionRing stay (the ring is toggled by code).</summary>
        private static void HideLegacyVisuals(Transform root)
        {
            foreach (Transform child in root)
            {
                if (child.name == "Foundation" || child.name == "SelectionRing" || child.name == VisualName) continue;
                child.gameObject.SetActive(false);
            }
        }

        // ------------------------------------------------------------------ part helpers (all y values are world-up from the ground)

        private static Material M(string name) => MilitaryMaterials.Get(name);

        private static void Box(Transform parent, string name, float x, float y, float z, float sx, float sy, float sz, string mat, float ex = 0f, float ey = 0f, float ez = 0f)
        {
            ModelBuilder.Prim(PrimitiveType.Cube, name, parent, new Vector3(x, y, z), new Vector3(sx, sy, sz), M(mat), new Vector3(ex, ey, ez));
        }

        /// <summary>Upright cylinder: radius and total height.</summary>
        private static void Cyl(Transform parent, string name, float x, float y, float z, float radius, float height, string mat, float ex = 0f, float ey = 0f, float ez = 0f)
        {
            ModelBuilder.Prim(PrimitiveType.Cylinder, name, parent, new Vector3(x, y, z), new Vector3(radius * 2f, height / 2f, radius * 2f), M(mat), new Vector3(ex, ey, ez));
        }

        private static void Sph(Transform parent, string name, float x, float y, float z, float sx, float sy, float sz, string mat, float ex = 0f, float ey = 0f, float ez = 0f)
        {
            ModelBuilder.Prim(PrimitiveType.Sphere, name, parent, new Vector3(x, y, z), new Vector3(sx, sy, sz), M(mat), new Vector3(ex, ey, ez));
        }

        // ------------------------------------------------------------------ HEADQUARTERS (footprint 6 x 6, front = -Z)

        private static void BuildHeadquarters(Ctx c)
        {
            Transform core = c.Core;

            // Main two-storey block with a dark plinth, front pilasters and a cornice.
            Box(core, "MainBlock", 0f, Y0 + 1.6f, 0.4f, 5.8f, 3.2f, 4.8f, "Mil_Concrete");
            Box(core, "Plinth", 0f, Y0 + 0.3f, 0.4f, 5.9f, 0.6f, 4.9f, "Mil_ConcreteDark");
            Box(core, "Cornice", 0f, Y0 + 3.25f, 0.4f, 6.0f, 0.2f, 5.0f, "Mil_ConcreteLight");
            foreach (float x in new[] { -2.6f, -1.6f, 1.6f, 2.6f })
            {
                Box(core, "Pilaster", x, Y0 + 1.6f, -2.05f, 0.35f, 3.2f, 0.3f, "Mil_ConcreteLight");
            }

            // Window rows, some lit.
            float[] rows = { Y0 + 1.3f, Y0 + 2.35f };
            float[] frontX = { -2.1f, -1.1f, 1.1f, 2.1f };
            for (int r = 0; r < rows.Length; r++)
            {
                for (int i = 0; i < frontX.Length; i++)
                {
                    Box(core, "WindowF", frontX[i], rows[r], -2.02f, 0.7f, 0.7f, 0.06f, (i + r) % 3 == 0 ? "Mil_WindowGlow" : "Mil_Glass");
                }
                foreach (float z in new[] { -0.9f, 0.3f, 1.5f })
                {
                    Box(core, "WindowL", -2.93f, rows[r], z, 0.06f, 0.7f, 0.7f, "Mil_Glass");
                    Box(core, "WindowR", 2.93f, rows[r], z, 0.06f, 0.7f, 0.7f, "Mil_Glass");
                }
                foreach (float x in new[] { -1.8f, 0f, 1.8f })
                {
                    Box(core, "WindowB", x, rows[r], 2.83f, 0.7f, 0.7f, 0.06f, "Mil_Glass");
                }
            }

            // Entrance: steps, canopy with hazard stripe, columns, door.
            Box(core, "Steps", 0f, Y0 + 0.1f, -2.9f, 2.4f, 0.2f, 1.0f, "Mil_ConcreteLight");
            Box(core, "Door", 0f, Y0 + 0.95f, -2.02f, 1.5f, 1.9f, 0.12f, "Mil_SteelDark");
            Box(core, "DoorGlass", 0f, Y0 + 1.2f, -2.09f, 1.2f, 0.8f, 0.04f, "Mil_Glass");
            Box(core, "Canopy", 0f, Y0 + 2.35f, -2.7f, 2.8f, 0.18f, 1.4f, "Mil_SteelDark");
            Box(core, "CanopyStripe", 0f, Y0 + 2.2f, -3.38f, 2.8f, 0.1f, 0.05f, "Mil_Hazard");
            Cyl(core, "ColumnL", -1.25f, Y0 + 1.2f, -3.25f, 0.13f, 2.4f, "Mil_ConcreteLight");
            Cyl(core, "ColumnR", 1.25f, Y0 + 1.2f, -3.25f, 0.13f, 2.4f, "Mil_ConcreteLight");

            // Command level (olive, set back) with glazing bands.
            Box(core, "UpperBlock", -0.2f, Y0 + 4.3f, 0.2f, 4.2f, 1.9f, 3.8f, "Mil_PaintOlive");
            Box(core, "UpperBandF", -0.2f, Y0 + 4.25f, -1.73f, 3.6f, 0.5f, 0.06f, "Mil_Glass");
            Box(core, "UpperBandFLit", -0.2f, Y0 + 4.25f, -1.75f, 1.0f, 0.5f, 0.04f, "Mil_WindowGlow");
            Box(core, "UpperBandL", -2.33f, Y0 + 4.25f, 0.2f, 0.06f, 0.5f, 3.0f, "Mil_Glass");
            Box(core, "UpperBandR", 1.93f, Y0 + 4.25f, 0.2f, 0.06f, 0.5f, 3.0f, "Mil_Glass");
            Box(core, "UpperRoof", -0.2f, Y0 + 5.33f, 0.2f, 4.4f, 0.16f, 4.0f, "Mil_SteelDark");

            // Command tower with a glazed cab, mast and warning beacon.
            Box(core, "Tower", 2.35f, Y0 + 5.15f, 2.0f, 1.0f, 3.6f, 1.0f, "Mil_ConcreteLight");
            Box(core, "TowerCab", 2.35f, Y0 + 7.4f, 2.0f, 1.5f, 0.9f, 1.5f, "Mil_Glass");
            Box(core, "TowerCabLit", 2.35f, Y0 + 7.4f, 1.22f, 1.2f, 0.5f, 0.04f, "Mil_WindowGlow");
            Box(core, "TowerRoof", 2.35f, Y0 + 7.95f, 2.0f, 1.8f, 0.15f, 1.8f, "Mil_SteelDark");
            Cyl(core, "TowerMast", 2.35f, Y0 + 8.7f, 2.0f, 0.04f, 1.4f, "Mil_MetalLight");
            Sph(core, "Beacon", 2.35f, Y0 + 9.45f, 2.0f, 0.22f, 0.22f, 0.22f, "Mil_WarningRed");

            // Rooftop equipment: satellite dish, antenna mast, HVAC.
            Cyl(core, "DishPost", -1.2f, Y0 + 5.55f, 1.0f, 0.06f, 0.4f, "Mil_SteelDark");
            Sph(core, "Dish", -1.2f, Y0 + 5.85f, 1.0f, 1.3f, 0.3f, 1.3f, "Mil_MetalLight", -40f, 20f, 0f);
            Cyl(core, "Mast", -1.7f, Y0 + 6.6f, -0.6f, 0.04f, 2.6f, "Mil_MetalLight");
            Sph(core, "MastTip", -1.7f, Y0 + 7.95f, -0.6f, 0.12f, 0.12f, 0.12f, "Mil_WarningRed");
            Box(core, "HvacA", 2.4f, Y0 + 3.65f, -1.0f, 1.0f, 0.6f, 0.9f, "Mil_SteelGray");
            Cyl(core, "HvacFanA", 2.4f, Y0 + 4.0f, -1.0f, 0.3f, 0.1f, "Mil_SteelDark");
            Box(core, "HvacB", -2.6f, Y0 + 3.65f, -0.5f, 0.5f, 0.6f, 1.0f, "Mil_SteelGray");

            // Flag pole in front of the building.
            Box(core, "FlagBase", -3.0f, Y0 + 0.1f, -3.0f, 0.5f, 0.2f, 0.5f, "Mil_ConcreteDark");
            Cyl(core, "FlagPole", -3.0f, Y0 + 2.3f, -3.0f, 0.05f, 4.2f, "Mil_MetalLight");
            Box(core, "Flag", -2.6f, Y0 + 3.95f, -3.0f, 0.8f, 0.5f, 0.03f, "Mil_PaintOlive");
            Box(core, "FlagStripe", -2.6f, Y0 + 3.95f, -3.02f, 0.8f, 0.1f, 0.01f, "Mil_Gold");

            // ---- Tier 1 (L1-5): jersey barriers and floodlights
            Box(c.Tier[0], "JerseyL", -1.9f, Y0 + 0.35f, -3.2f, 1.4f, 0.7f, 0.45f, "Mil_ConcreteLight");
            Box(c.Tier[0], "JerseyLStripe", -1.9f, Y0 + 0.55f, -3.44f, 1.4f, 0.1f, 0.02f, "Mil_Hazard");
            Box(c.Tier[0], "JerseyR", 1.9f, Y0 + 0.35f, -3.2f, 1.4f, 0.7f, 0.45f, "Mil_ConcreteLight");
            Box(c.Tier[0], "JerseyRStripe", 1.9f, Y0 + 0.55f, -3.44f, 1.4f, 0.1f, 0.02f, "Mil_Hazard");
            foreach (float x in new[] { -3.1f, 3.1f })
            {
                Cyl(c.Tier[0], "LightPole", x, Y0 + 1.4f, -1.6f, 0.05f, 2.8f, "Mil_SteelDark");
                Box(c.Tier[0], "LightHead", x, Y0 + 2.85f, -1.6f, 0.5f, 0.12f, 0.3f, "Mil_WindowGlow");
            }

            // ---- Tier 2 (L6-10): sandbag lines and repair plates
            foreach (float side in new[] { -1f, 1f })
            {
                for (int i = 0; i < 4; i++)
                {
                    Box(c.Tier[1], "Sandbag", side * (1.2f + i * 0.55f), Y0 + 0.15f, 3.2f, 0.55f, 0.3f, 0.4f, "Mil_Sandbag");
                    if (i < 3) Box(c.Tier[1], "SandbagTop", side * (1.475f + i * 0.55f), Y0 + 0.45f, 3.2f, 0.55f, 0.3f, 0.4f, "Mil_Sandbag");
                }
            }
            Box(c.Tier[1], "PatchA", -2.1f, Y0 + 1.8f, -2.05f, 0.9f, 0.5f, 0.04f, "Mil_SteelGray");
            Box(c.Tier[1], "PatchB", 1.4f, Y0 + 0.9f, -2.05f, 0.5f, 0.7f, 0.04f, "Mil_SteelGray");

            // ---- Tier 3 (L11-15): perimeter wall and gate posts
            Box(c.Tier[2], "WallBack", 0f, Y0 + 0.3f, 3.3f, 6.9f, 0.6f, 0.25f, "Mil_ConcreteLight");
            Box(c.Tier[2], "WallL", -3.35f, Y0 + 0.3f, 0.2f, 0.25f, 0.6f, 6.2f, "Mil_ConcreteLight");
            Box(c.Tier[2], "WallR", 3.35f, Y0 + 0.3f, 0.2f, 0.25f, 0.6f, 6.2f, "Mil_ConcreteLight");
            foreach (float x in new[] { -3.3f, 3.3f })
            {
                Box(c.Tier[2], "GatePost", x, Y0 + 0.7f, -3.35f, 0.3f, 1.4f, 0.3f, "Mil_ConcreteLight");
                Sph(c.Tier[2], "GateLamp", x, Y0 + 1.5f, -3.35f, 0.2f, 0.2f, 0.2f, "Mil_WarningRed");
            }

            // ---- Tier 4 (L16-20): solar panels, generator, second mast
            for (int i = 0; i < 3; i++)
            {
                Box(c.Tier[3], "Solar", -1.5f + i * 1.2f, Y0 + 3.6f, 2.45f, 1.0f, 0.05f, 0.5f, "Mil_Solar", -20f, 0f, 0f);
            }
            Box(c.Tier[3], "Generator", -3.0f, Y0 + 0.45f, 2.0f, 1.4f, 0.9f, 0.8f, "Mil_PaintOliveDark");
            Cyl(c.Tier[3], "GeneratorExhaust", -3.0f, Y0 + 1.2f, 2.2f, 0.1f, 0.6f, "Mil_SteelDark");
            Cyl(c.Tier[3], "Mast2", 1.2f, Y0 + 6.6f, 2.0f, 0.04f, 2.6f, "Mil_MetalLight");

            // ---- Tier 5 (L21-25): storage annex and comms array
            Box(c.Tier[4], "AnnexBlock", 3.6f, Y0 + 0.8f, -0.5f, 1.4f, 1.6f, 2.4f, "Mil_PaintOlive");
            Box(c.Tier[4], "AnnexRoof", 3.6f, Y0 + 1.65f, -0.5f, 1.6f, 0.12f, 2.6f, "Mil_SteelDark");
            Box(c.Tier[4], "AnnexDoor", 3.6f, Y0 + 0.6f, -1.72f, 0.9f, 1.2f, 0.06f, "Mil_SteelDark");
            Cyl(c.Tier[4], "ArrayPost", 0.4f, Y0 + 5.9f, -0.2f, 0.05f, 1.2f, "Mil_MetalLight");
            Box(c.Tier[4], "ArrayPanel", 0.4f, Y0 + 6.5f, -0.2f, 1.2f, 0.6f, 0.06f, "Mil_SteelGray");

            // ---- Tier 6 (L26-30): armoured corner plates and roof turrets
            foreach (float x in new[] { -2.95f, 2.95f })
            {
                Box(c.Tier[5], "ArmourFront", x, Y0 + 1.6f, -2.0f, 0.3f, 3.0f, 0.3f, "Mil_SteelDark");
                Box(c.Tier[5], "ArmourBack", x, Y0 + 1.6f, 2.8f, 0.3f, 3.0f, 0.3f, "Mil_SteelDark");
            }
            Cyl(c.Tier[5], "TurretBase", -2.6f, Y0 + 3.6f, -1.8f, 0.3f, 0.5f, "Mil_SteelDark");
            Box(c.Tier[5], "TurretHead", -2.6f, Y0 + 3.95f, -1.8f, 0.5f, 0.3f, 0.5f, "Mil_SteelGray");
            Cyl(c.Tier[5], "TurretBarrel", -2.6f, Y0 + 3.95f, -2.25f, 0.05f, 0.8f, "Mil_MetalLight", 90f, 0f, 0f);

            // ---- Tier 7 (L31-35): gold trim, emblem, glow strips
            Box(c.Tier[6], "GoldFront", 0f, Y0 + 3.38f, -2.13f, 6.1f, 0.06f, 0.1f, "Mil_Gold");
            Box(c.Tier[6], "GoldBack", 0f, Y0 + 3.38f, 2.93f, 6.1f, 0.06f, 0.1f, "Mil_Gold");
            Box(c.Tier[6], "GoldLeft", -3.03f, Y0 + 3.38f, 0.4f, 0.1f, 0.06f, 5.0f, "Mil_Gold");
            Box(c.Tier[6], "GoldRight", 3.03f, Y0 + 3.38f, 0.4f, 0.1f, 0.06f, 5.0f, "Mil_Gold");
            Box(c.Tier[6], "Emblem", 0f, Y0 + 2.8f, -2.05f, 0.9f, 0.5f, 0.04f, "Mil_Gold");
            Box(c.Tier[6], "GlowStripF", 0f, Y0 + 0.62f, -2.07f, 5.0f, 0.06f, 0.03f, "Mil_WindowGlow");
            Sph(c.Tier[6], "FlagFinial", -3.0f, Y0 + 4.45f, -3.0f, 0.2f, 0.2f, 0.2f, "Mil_Gold");
        }
    }
}
