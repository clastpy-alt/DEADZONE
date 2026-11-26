using UnityEngine;

namespace Deadzone.EditorTools
{
    /// <summary>
    /// PHASE 02.5: the six non-HQ building models (distinct silhouettes, see ART_DIRECTION.md) plus the shared tier additions.
    /// Footprints follow BuildingPrefabGenerator.Specs; front = -Z; Y0 (0.2) is the top of the foundation slab.
    /// </summary>
    internal static partial class MilitaryModelGenerator
    {
        // Window on a facade facing Z (front/back) or X (sides).
        private static void WinZ(Transform t, float x, float y, float z, float w, float h, string mat = "Mil_Glass")
            => Box(t, "WindowZ", x, y, z, w, h, 0.06f, mat);

        private static void WinX(Transform t, float x, float y, float z, float w, float h, string mat = "Mil_Glass")
            => Box(t, "WindowX", x, y, z, 0.06f, h, w, mat);

        // ------------------------------------------------------------------ shared tier additions (outside the footprint, so they work for every silhouette)

        /// <summary>w/d = body footprint, h = wall height. Tier 1 barriers+lights, 2 sandbags, 3 wall+gate, 4 solar+generator, 5 shed+comms, 6 armour+turret, 7 gold trim.</summary>
        private static void CommonTiers(Ctx c, float w, float d, float h)
        {
            float hw = w / 2f, hd = d / 2f;

            // Tier 1 (L1-5)
            foreach (float s in new[] { -1f, 1f })
            {
                Box(c.Tier[0], "Jersey", s * w * 0.3f, Y0 + 0.35f, -(hd + 0.75f), 1.4f, 0.7f, 0.45f, "Mil_ConcreteLight");
                Box(c.Tier[0], "JerseyStripe", s * w * 0.3f, Y0 + 0.55f, -(hd + 0.98f), 1.4f, 0.1f, 0.02f, "Mil_Hazard");
                Cyl(c.Tier[0], "LightPole", s * (hw + 0.25f), Y0 + 1.4f, -(hd + 0.25f), 0.05f, 2.8f, "Mil_SteelDark");
                Box(c.Tier[0], "LightHead", s * (hw + 0.25f), Y0 + 2.85f, -(hd + 0.25f), 0.5f, 0.12f, 0.3f, "Mil_WindowGlow");
            }

            // Tier 2 (L6-10): sandbag lines at the back
            foreach (float s in new[] { -1f, 1f })
            {
                for (int i = 0; i < 4; i++)
                {
                    Box(c.Tier[1], "Sandbag", s * (1.0f + i * 0.55f), Y0 + 0.15f, hd + 0.7f, 0.55f, 0.3f, 0.4f, "Mil_Sandbag");
                    if (i < 3) Box(c.Tier[1], "SandbagTop", s * (1.275f + i * 0.55f), Y0 + 0.45f, hd + 0.7f, 0.55f, 0.3f, 0.4f, "Mil_Sandbag");
                }
            }

            // Tier 3 (L11-15): perimeter wall and gate posts
            Box(c.Tier[2], "WallBack", 0f, Y0 + 0.3f, hd + 1.1f, w + 1.9f, 0.6f, 0.25f, "Mil_ConcreteLight");
            foreach (float s in new[] { -1f, 1f })
            {
                Box(c.Tier[2], "WallSide", s * (hw + 0.8f), Y0 + 0.3f, 0.2f, 0.25f, 0.6f, d + 2.0f, "Mil_ConcreteLight");
                Box(c.Tier[2], "GatePost", s * (hw + 0.8f), Y0 + 0.7f, -(hd + 0.8f), 0.3f, 1.4f, 0.3f, "Mil_ConcreteLight");
                Sph(c.Tier[2], "GateLamp", s * (hw + 0.8f), Y0 + 1.5f, -(hd + 0.8f), 0.2f, 0.2f, 0.2f, "Mil_WarningRed");
            }

            // Tier 4 (L16-20): ground solar racks behind the wall, generator beside the building
            for (int i = 0; i < 3; i++)
            {
                Box(c.Tier[3], "Solar", -1.4f + i * 1.4f, Y0 + 0.55f, hd + 1.9f, 1.2f, 0.05f, 0.7f, "Mil_Solar", -25f, 0f, 0f);
                Cyl(c.Tier[3], "SolarLeg", -1.4f + i * 1.4f, Y0 + 0.25f, hd + 1.9f, 0.04f, 0.5f, "Mil_SteelDark");
            }
            Box(c.Tier[3], "Generator", -(hw + 1.6f), Y0 + 0.45f, hd - 0.4f, 1.2f, 0.9f, 0.8f, "Mil_PaintOliveDark");
            Cyl(c.Tier[3], "GeneratorExhaust", -(hw + 1.6f), Y0 + 1.2f, hd - 0.2f, 0.1f, 0.6f, "Mil_SteelDark");

            // Tier 5 (L21-25): storage shed with comms array
            Box(c.Tier[4], "Shed", -(hw + 1.6f), Y0 + 0.7f, -(hd - 0.6f), 1.2f, 1.4f, 1.4f, "Mil_PaintOlive");
            Box(c.Tier[4], "ShedRoof", -(hw + 1.6f), Y0 + 1.45f, -(hd - 0.6f), 1.4f, 0.1f, 1.6f, "Mil_SteelDark");
            Cyl(c.Tier[4], "ArrayPost", -(hw + 1.6f), Y0 + 2.3f, -(hd - 0.6f), 0.05f, 1.6f, "Mil_MetalLight");
            Box(c.Tier[4], "ArrayPanel", -(hw + 1.6f), Y0 + 2.85f, -(hd - 0.6f), 1.0f, 0.5f, 0.06f, "Mil_SteelGray");

            // Tier 6 (L26-30): armoured corner plates and a gun emplacement
            foreach (float s in new[] { -1f, 1f })
            {
                Box(c.Tier[5], "Armour", s * (hw + 0.05f), Y0 + (h - 0.3f) / 2f, -hd, 0.3f, h - 0.3f, 0.3f, "Mil_SteelDark");
            }
            Cyl(c.Tier[5], "TurretBase", hw + 1.4f, Y0 + 0.25f, -(hd + 0.9f), 0.3f, 0.5f, "Mil_SteelDark");
            Box(c.Tier[5], "TurretHead", hw + 1.4f, Y0 + 0.65f, -(hd + 0.9f), 0.5f, 0.3f, 0.5f, "Mil_SteelGray");
            Cyl(c.Tier[5], "TurretBarrel", hw + 1.4f, Y0 + 0.65f, -(hd + 1.35f), 0.05f, 0.8f, "Mil_MetalLight", 90f, 0f, 0f);

            // Tier 7 (L31-35): gold border on the foundation, gold plaque on a pedestal
            Box(c.Tier[6], "GoldFront", 0f, Y0 + 0.03f, -(hd + 0.47f), w + 0.94f, 0.06f, 0.07f, "Mil_Gold");
            Box(c.Tier[6], "GoldBack", 0f, Y0 + 0.03f, hd + 0.47f, w + 0.94f, 0.06f, 0.07f, "Mil_Gold");
            Box(c.Tier[6], "GoldLeft", -(hw + 0.47f), Y0 + 0.03f, 0f, 0.07f, 0.06f, d + 0.94f, "Mil_Gold");
            Box(c.Tier[6], "GoldRight", hw + 0.47f, Y0 + 0.03f, 0f, 0.07f, 0.06f, d + 0.94f, "Mil_Gold");
            Box(c.Tier[6], "PlaquePedestal", -(hw - 0.1f), Y0 + 0.5f, -(hd + 0.25f), 0.35f, 1.0f, 0.35f, "Mil_ConcreteLight");
            Box(c.Tier[6], "PlaqueGold", -(hw - 0.1f), Y0 + 0.9f, -(hd + 0.44f), 0.3f, 0.3f, 0.03f, "Mil_Gold");
            Sph(c.Tier[6], "PlaqueCap", -(hw - 0.1f), Y0 + 1.15f, -(hd + 0.25f), 0.3f, 0.3f, 0.3f, "Mil_Gold");
            Box(c.Tier[6], "GlowStrip", 0f, Y0 + 0.55f, -(hd + 0.03f), w * 0.8f, 0.05f, 0.03f, "Mil_WindowGlow");
        }

        // ------------------------------------------------------------------ BARRACKS: long, low, stepped gable roof, chimney, flag

        private static void BuildBarracks(Ctx c)
        {
            Transform t = c.Core;
            Box(t, "Body", 0f, Y0 + 1.1f, 0f, 6.8f, 2.2f, 3.3f, "Mil_PaintOlive");
            Box(t, "Plinth", 0f, Y0 + 0.2f, 0f, 6.9f, 0.4f, 3.4f, "Mil_ConcreteDark");
            Box(t, "Eave", 0f, Y0 + 2.28f, 0f, 7.2f, 0.14f, 3.8f, "Mil_SteelDark");
            Box(t, "RoofStep1", 0f, Y0 + 2.55f, 0f, 6.6f, 0.4f, 2.6f, "Mil_PaintOliveDark");
            Box(t, "RoofStep2", 0f, Y0 + 2.92f, 0f, 6.2f, 0.35f, 1.7f, "Mil_PaintOliveDark");
            Box(t, "RoofRidge", 0f, Y0 + 3.25f, 0f, 5.8f, 0.3f, 0.8f, "Mil_SteelGray");

            foreach (float x in new[] { -2.6f, -1.6f, 1.6f, 2.6f })
            {
                WinZ(t, x, Y0 + 1.3f, -1.67f, 0.6f, 0.6f, x < 0 ? "Mil_Glass" : "Mil_WindowGlow");
                WinZ(t, x, Y0 + 1.3f, 1.67f, 0.6f, 0.6f);
            }
            foreach (float z in new[] { -0.7f, 0.7f })
            {
                WinX(t, -3.43f, Y0 + 1.3f, z, 0.6f, 0.6f);
                WinX(t, 3.43f, Y0 + 1.3f, z, 0.6f, 0.6f);
            }

            // Entrance
            Box(t, "Door", 0f, Y0 + 0.85f, -1.68f, 1.1f, 1.7f, 0.1f, "Mil_SteelDark");
            Box(t, "Canopy", 0f, Y0 + 1.95f, -2.05f, 1.9f, 0.12f, 0.9f, "Mil_SteelDark");
            Box(t, "CanopyStripe", 0f, Y0 + 1.85f, -2.5f, 1.9f, 0.1f, 0.04f, "Mil_Hazard");
            Cyl(t, "PostL", -0.85f, Y0 + 0.95f, -2.4f, 0.06f, 1.9f, "Mil_MetalLight");
            Cyl(t, "PostR", 0.85f, Y0 + 0.95f, -2.4f, 0.06f, 1.9f, "Mil_MetalLight");
            Box(t, "Step", 0f, Y0 + 0.08f, -2.1f, 1.6f, 0.16f, 0.7f, "Mil_ConcreteLight");

            // Roof details
            Box(t, "Chimney", -2.4f, Y0 + 3.3f, 0.6f, 0.5f, 1.2f, 0.5f, "Mil_ConcreteDark");
            Cyl(t, "VentA", 1.8f, Y0 + 3.05f, 0.8f, 0.18f, 0.5f, "Mil_SteelGray");
            Cyl(t, "VentB", 2.5f, Y0 + 3.05f, 0.8f, 0.18f, 0.5f, "Mil_SteelGray");
            Cyl(t, "Antenna", 2.9f, Y0 + 3.6f, -0.6f, 0.03f, 2.0f, "Mil_MetalLight");

            // Flag pole
            Box(t, "FlagBase", 3.0f, Y0 + 0.1f, -2.0f, 0.4f, 0.2f, 0.4f, "Mil_ConcreteDark");
            Cyl(t, "FlagPole", 3.0f, Y0 + 1.9f, -2.0f, 0.04f, 3.6f, "Mil_MetalLight");
            Box(t, "Flag", 3.35f, Y0 + 3.3f, -2.0f, 0.7f, 0.4f, 0.03f, "Mil_PaintOlive");
            Box(t, "FlagStripe", 3.35f, Y0 + 3.3f, -2.02f, 0.7f, 0.08f, 0.01f, "Mil_Hazard");

            CommonTiers(c, 6.8f, 3.3f, 2.2f);
        }

        // ------------------------------------------------------------------ HOSPITAL: white centre block + two wings, red crosses, helipad, ambulance

        private static void BuildHospital(Ctx c)
        {
            Transform t = c.Core;
            Box(t, "Centre", 0f, Y0 + 1.9f, 0.2f, 3.0f, 3.8f, 4.2f, "Mil_MedWhite");
            Box(t, "WingL", -2.1f, Y0 + 1.2f, 0.2f, 1.8f, 2.4f, 3.6f, "Mil_MedWhite");
            Box(t, "WingR", 2.1f, Y0 + 1.2f, 0.2f, 1.8f, 2.4f, 3.6f, "Mil_MedWhite");
            Box(t, "Plinth", 0f, Y0 + 0.2f, 0.2f, 5.1f, 0.4f, 3.7f, "Mil_ConcreteLight");
            Box(t, "RoofCentre", 0f, Y0 + 3.88f, 0.2f, 3.2f, 0.16f, 4.4f, "Mil_ConcreteLight");
            Box(t, "RoofWingL", -2.1f, Y0 + 2.47f, 0.2f, 2.0f, 0.14f, 3.8f, "Mil_ConcreteLight");
            Box(t, "RoofWingR", 2.1f, Y0 + 2.47f, 0.2f, 2.0f, 0.14f, 3.8f, "Mil_ConcreteLight");

            // Red cross on the facade and on the roof
            Box(t, "CrossFH", 0f, Y0 + 3.0f, -1.92f, 1.1f, 0.3f, 0.04f, "Mil_MedRed");
            Box(t, "CrossFV", 0f, Y0 + 3.0f, -1.92f, 0.3f, 1.1f, 0.04f, "Mil_MedRed");
            Box(t, "CrossRH", 0f, Y0 + 3.98f, 0.2f, 1.8f, 0.04f, 0.5f, "Mil_MedRed");
            Box(t, "CrossRV", 0f, Y0 + 3.98f, 0.2f, 0.5f, 0.04f, 1.8f, "Mil_MedRed");

            // Windows (centre block and wings)
            foreach (float x in new[] { -1.0f, 1.0f })
            {
                WinZ(t, x, Y0 + 1.0f, -1.92f, 0.5f, 0.5f);
                WinZ(t, x, Y0 + 1.8f, -1.92f, 0.5f, 0.5f);
            }
            foreach (float x in new[] { -2.5f, -1.7f, 1.7f, 2.5f })
            {
                WinZ(t, x, Y0 + 0.9f, -1.62f, 0.5f, 0.5f);
                WinZ(t, x, Y0 + 1.7f, -1.62f, 0.5f, 0.5f, "Mil_WindowGlow");
            }
            Box(t, "WingStripeL", -2.1f, Y0 + 1.3f, -1.62f, 1.8f, 0.1f, 0.04f, "Mil_MedRed");
            Box(t, "WingStripeR", 2.1f, Y0 + 1.3f, -1.62f, 1.8f, 0.1f, 0.04f, "Mil_MedRed");
            foreach (float z in new[] { -1.0f, 0.2f, 1.4f })
            {
                WinX(t, -3.03f, Y0 + 1.2f, z, 0.6f, 0.6f);
                WinX(t, 3.03f, Y0 + 1.2f, z, 0.6f, 0.6f);
            }
            foreach (float z in new[] { -0.8f, 0.6f, 1.6f })
            {
                WinX(t, -1.53f, Y0 + 3.1f, z, 0.6f, 0.6f);
                WinX(t, 1.53f, Y0 + 3.1f, z, 0.6f, 0.6f);
            }

            // Entrance with red canopy
            Box(t, "Door", 0f, Y0 + 0.8f, -1.93f, 1.3f, 1.5f, 0.1f, "Mil_Glass");
            Box(t, "DoorFrame", 0f, Y0 + 0.8f, -1.96f, 1.5f, 1.6f, 0.04f, "Mil_SteelDark");
            Box(t, "Canopy", 0f, Y0 + 1.95f, -2.4f, 2.4f, 0.14f, 1.0f, "Mil_MedRed");
            Cyl(t, "PostL", -1.05f, Y0 + 0.95f, -2.8f, 0.06f, 1.9f, "Mil_MetalLight");
            Cyl(t, "PostR", 1.05f, Y0 + 0.95f, -2.8f, 0.06f, 1.9f, "Mil_MetalLight");

            // Helipad on the right wing
            Cyl(t, "HelipadDisc", 2.1f, Y0 + 2.58f, 0.2f, 0.85f, 0.06f, "Mil_ConcreteDark");
            Box(t, "H_L", 1.85f, Y0 + 2.63f, 0.2f, 0.12f, 0.04f, 0.7f, "Mil_MedWhite");
            Box(t, "H_R", 2.35f, Y0 + 2.63f, 0.2f, 0.12f, 0.04f, 0.7f, "Mil_MedWhite");
            Box(t, "H_Bar", 2.1f, Y0 + 2.63f, 0.2f, 0.5f, 0.04f, 0.12f, "Mil_MedWhite");

            // Rooftop equipment
            Box(t, "Hvac", -0.8f, Y0 + 4.2f, 1.2f, 0.9f, 0.5f, 0.8f, "Mil_SteelGray");
            Cyl(t, "Antenna", 0.9f, Y0 + 4.8f, 1.4f, 0.04f, 1.8f, "Mil_MetalLight");
            Sph(t, "AntennaTip", 0.9f, Y0 + 5.75f, 1.4f, 0.12f, 0.12f, 0.12f, "Mil_WarningRed");

            // Ambulance
            Box(t, "AmbBody", 2.0f, Y0 + 0.65f, -2.35f, 1.5f, 0.85f, 0.8f, "Mil_MedWhite");
            Box(t, "AmbCab", 1.05f, Y0 + 0.55f, -2.35f, 0.55f, 0.65f, 0.8f, "Mil_MedWhite");
            Box(t, "AmbGlass", 0.8f, Y0 + 0.7f, -2.35f, 0.04f, 0.3f, 0.7f, "Mil_Glass");
            Box(t, "AmbStripe", 2.0f, Y0 + 0.65f, -2.76f, 1.5f, 0.15f, 0.02f, "Mil_MedRed");
            Sph(t, "AmbLight", 2.0f, Y0 + 1.15f, -2.35f, 0.18f, 0.18f, 0.18f, "Mil_WarningRed");
            foreach (float x in new[] { 1.45f, 2.55f })
            {
                foreach (float z in new[] { -2.75f, -1.95f })
                {
                    Cyl(t, "Wheel", x, Y0 + 0.2f, z, 0.2f, 0.12f, "Mil_Rubber", 90f, 0f, 0f);
                }
            }

            CommonTiers(c, 5.0f, 4.2f, 3.0f);
        }

        // ------------------------------------------------------------------ RESEARCH LAB: stepped block, glowing dome, reactor cylinder, radar

        private static void BuildResearchLab(Ctx c)
        {
            Transform t = c.Core;
            Box(t, "Base", 0f, Y0 + 1.1f, 0f, 4.8f, 2.2f, 4.8f, "Mil_ConcreteDark");
            Box(t, "BaseCap", 0f, Y0 + 2.25f, 0f, 5.0f, 0.12f, 5.0f, "Mil_SteelDark");
            Box(t, "Upper", 0.3f, Y0 + 2.8f, -0.2f, 3.8f, 1.0f, 3.6f, "Mil_SteelGray");
            Box(t, "UpperCap", 0.3f, Y0 + 3.35f, -0.2f, 4.0f, 0.1f, 3.8f, "Mil_SteelDark");

            // Glowing bands and windows
            Box(t, "GlowBand", 0f, Y0 + 1.5f, -2.42f, 3.6f, 0.35f, 0.04f, "Mil_LabGlow");
            Box(t, "GlowBandUpper", 0.3f, Y0 + 2.8f, -2.02f, 3.0f, 0.35f, 0.04f, "Mil_LabGlow");
            Box(t, "GlowBandUpperSide", 2.22f, Y0 + 2.8f, -0.2f, 0.04f, 0.35f, 2.8f, "Mil_LabGlow");
            foreach (float z in new[] { -1.2f, 0f, 1.2f })
            {
                WinX(t, -2.43f, Y0 + 1.3f, z, 0.7f, 0.7f);
                WinX(t, 2.43f, Y0 + 1.3f, z, 0.7f, 0.7f, "Mil_LabGlow");
            }
            foreach (float x in new[] { -1.6f, 0f, 1.6f }) WinZ(t, x, Y0 + 1.3f, 2.43f, 0.7f, 0.7f);

            // Dome and ring
            Cyl(t, "DomeRing", 0.3f, Y0 + 3.5f, -0.2f, 1.5f, 0.12f, "Mil_SteelDark");
            Sph(t, "Dome", 0.3f, Y0 + 3.45f, -0.2f, 2.6f, 1.7f, 2.6f, "Mil_LabGlow");

            // Reactor cylinder with glow rings
            Cyl(t, "Reactor", -1.7f, Y0 + 3.4f, 1.4f, 0.55f, 2.4f, "Mil_SteelGray");
            Cyl(t, "ReactorCap", -1.7f, Y0 + 4.65f, 1.4f, 0.65f, 0.15f, "Mil_SteelDark");
            Cyl(t, "ReactorRingA", -1.7f, Y0 + 2.9f, 1.4f, 0.6f, 0.12f, "Mil_LabGlow");
            Cyl(t, "ReactorRingB", -1.7f, Y0 + 3.7f, 1.4f, 0.6f, 0.12f, "Mil_LabGlow");

            // Radar dish and mast
            Cyl(t, "DishPost", 1.7f, Y0 + 2.6f, 1.6f, 0.06f, 0.7f, "Mil_SteelDark");
            Sph(t, "Dish", 1.7f, Y0 + 3.05f, 1.6f, 1.2f, 0.3f, 1.2f, "Mil_MetalLight", -50f, 0f, 0f);
            Cyl(t, "Mast", 1.9f, Y0 + 4.4f, 0.2f, 0.04f, 2.0f, "Mil_MetalLight");
            Sph(t, "MastTip", 1.9f, Y0 + 5.45f, 0.2f, 0.12f, 0.12f, 0.12f, "Mil_WarningRed");

            // Entrance
            Box(t, "Door", 0f, Y0 + 0.8f, -2.42f, 1.2f, 1.6f, 0.1f, "Mil_SteelDark");
            Box(t, "DoorGlass", 0f, Y0 + 0.95f, -2.48f, 0.9f, 1.1f, 0.04f, "Mil_Glass");
            Box(t, "Canopy", 0f, Y0 + 1.95f, -2.8f, 2.0f, 0.1f, 0.9f, "Mil_Glass", -8f, 0f, 0f);
            Box(t, "CanopyFrame", 0f, Y0 + 2.0f, -3.2f, 2.0f, 0.08f, 0.06f, "Mil_SteelDark");

            CommonTiers(c, 4.8f, 4.8f, 2.2f);
        }

        // ------------------------------------------------------------------ FOOD FARM: barn, two silos, windmill, crop fields

        private static void BuildFoodFarm(Ctx c)
        {
            Transform t = c.Core;

            // Barn with a stepped gable roof
            Box(t, "BarnBody", -1.6f, Y0 + 0.9f, 0.6f, 3.4f, 1.8f, 3.0f, "Mil_Rust");
            Box(t, "BarnPlinth", -1.6f, Y0 + 0.15f, 0.6f, 3.5f, 0.3f, 3.1f, "Mil_ConcreteDark");
            Box(t, "BarnEave", -1.6f, Y0 + 1.88f, 0.6f, 3.7f, 0.14f, 3.3f, "Mil_SteelDark");
            Box(t, "BarnRoof1", -1.6f, Y0 + 2.12f, 0.6f, 3.3f, 0.35f, 2.4f, "Mil_SteelGray");
            Box(t, "BarnRoof2", -1.6f, Y0 + 2.45f, 0.6f, 2.9f, 0.3f, 1.4f, "Mil_SteelGray");
            Box(t, "BarnRidge", -1.6f, Y0 + 2.72f, 0.6f, 2.5f, 0.25f, 0.5f, "Mil_SteelDark");
            Box(t, "BarnDoor", -1.6f, Y0 + 0.7f, -0.93f, 1.3f, 1.4f, 0.1f, "Mil_SteelDark");
            Box(t, "BarnDoorBraceA", -1.6f, Y0 + 0.7f, -1.0f, 0.08f, 1.6f, 0.03f, "Mil_Sandbag", 0f, 0f, 35f);
            Box(t, "BarnDoorBraceB", -1.6f, Y0 + 0.7f, -1.0f, 0.08f, 1.6f, 0.03f, "Mil_Sandbag", 0f, 0f, -35f);
            WinZ(t, -2.9f, Y0 + 1.2f, -0.93f, 0.4f, 0.4f);
            WinZ(t, -0.3f, Y0 + 1.2f, -0.93f, 0.4f, 0.4f);

            // Silos
            Cyl(t, "SiloBig", 2.4f, Y0 + 2.2f, 1.3f, 0.85f, 4.4f, "Mil_SteelGray");
            foreach (float y in new[] { 1.0f, 2.0f, 3.0f }) Cyl(t, "SiloBand", 2.4f, Y0 + y, 1.3f, 0.88f, 0.1f, "Mil_SteelDark");
            Sph(t, "SiloDome", 2.4f, Y0 + 4.4f, 1.3f, 1.7f, 1.0f, 1.7f, "Mil_MetalLight");
            Cyl(t, "SiloCap", 2.4f, Y0 + 4.95f, 1.3f, 0.1f, 0.25f, "Mil_SteelDark");
            Cyl(t, "SiloSmall", 2.5f, Y0 + 1.6f, -0.3f, 0.6f, 3.2f, "Mil_SteelGray");
            Sph(t, "SiloSmallDome", 2.5f, Y0 + 3.2f, -0.3f, 1.2f, 0.7f, 1.2f, "Mil_MetalLight");
            Cyl(t, "SiloPipe", 1.5f, Y0 + 2.2f, 0.6f, 0.07f, 4.4f, "Mil_SteelDark", 0f, 0f, 90f);

            // Crop fields in front of the barn and silos
            Box(t, "SoilLeft", -1.7f, Y0 + 0.04f, -2.0f, 3.2f, 0.08f, 1.4f, "Mil_Soil");
            for (int i = 0; i < 4; i++) Box(t, "CropLeft", -1.7f, Y0 + 0.16f, -2.5f + i * 0.33f, 3.0f, 0.2f, 0.2f, "Mil_Crop");
            Box(t, "SoilRight", 2.0f, Y0 + 0.04f, -1.9f, 2.4f, 0.08f, 1.2f, "Mil_Soil");
            for (int i = 0; i < 3; i++) Box(t, "CropRight", 2.0f, Y0 + 0.16f, -2.3f + i * 0.33f, 2.2f, 0.2f, 0.2f, "Mil_Crop");

            // Hay bales
            Box(t, "Hay1", -3.1f, Y0 + 0.2f, -0.2f, 0.5f, 0.4f, 0.5f, "Mil_Hazard");
            Box(t, "Hay2", -3.1f, Y0 + 0.6f, -0.2f, 0.5f, 0.4f, 0.5f, "Mil_Hazard");
            Box(t, "Hay3", -3.1f, Y0 + 0.2f, 0.4f, 0.5f, 0.4f, 0.5f, "Mil_Hazard");

            // Windmill
            Cyl(t, "WindmillTower", 0.8f, Y0 + 1.7f, -0.4f, 0.09f, 3.4f, "Mil_MetalLight");
            Cyl(t, "WindmillHub", 0.8f, Y0 + 3.45f, -0.55f, 0.1f, 0.2f, "Mil_SteelDark", 90f, 0f, 0f);
            Box(t, "BladeH", 0.8f, Y0 + 3.45f, -0.7f, 1.6f, 0.12f, 0.03f, "Mil_MetalLight");
            Box(t, "BladeV", 0.8f, Y0 + 3.45f, -0.7f, 0.12f, 1.6f, 0.03f, "Mil_MetalLight");
            Box(t, "BladeD1", 0.8f, Y0 + 3.45f, -0.7f, 1.4f, 0.1f, 0.03f, "Mil_MetalLight", 0f, 0f, 45f);
            Box(t, "BladeD2", 0.8f, Y0 + 3.45f, -0.7f, 1.4f, 0.1f, 0.03f, "Mil_MetalLight", 0f, 0f, -45f);

            CommonTiers(c, 6.0f, 4.5f, 2.0f);
        }

        // ------------------------------------------------------------------ STEEL FACTORY: sawtooth roof, tall chimneys, furnace glow, pipes, conveyor

        private static void BuildSteelFactory(Ctx c)
        {
            Transform t = c.Core;
            Box(t, "Body", -0.2f, Y0 + 1.4f, 0f, 5.6f, 2.8f, 4.4f, "Mil_ConcreteDark");
            Box(t, "Plinth", -0.2f, Y0 + 0.25f, 0f, 5.7f, 0.5f, 4.5f, "Mil_SteelDark");
            Box(t, "RoofBase", -0.2f, Y0 + 2.85f, 0f, 5.8f, 0.14f, 4.6f, "Mil_SteelDark");

            // Sawtooth roof: three tilted slabs with glazed risers
            for (int i = 0; i < 3; i++)
            {
                float x = -1.9f + i * 1.85f;
                Box(t, "Sawtooth", x, Y0 + 3.25f, 0f, 1.8f, 0.12f, 4.5f, "Mil_SteelGray", 0f, 0f, -15f);
                Box(t, "SawGlass", x + 0.85f, Y0 + 3.2f, 0f, 0.08f, 0.6f, 4.4f, "Mil_Glass");
            }

            // Chimneys with red warning bands
            Cyl(t, "ChimneyA", -2.1f, Y0 + 5.2f, 1.4f, 0.45f, 4.6f, "Mil_ConcreteLight");
            Cyl(t, "ChimneyB", -0.9f, Y0 + 5.2f, 1.4f, 0.45f, 4.6f, "Mil_ConcreteLight");
            foreach (float x in new[] { -2.1f, -0.9f })
            {
                Cyl(t, "ChimneyBand", x, Y0 + 7.1f, 1.4f, 0.47f, 0.35f, "Mil_WarningRed");
                Cyl(t, "ChimneyTop", x, Y0 + 7.5f, 1.4f, 0.5f, 0.1f, "Mil_SteelDark");
            }

            // Furnace opening and rolling door on the front
            Box(t, "FurnaceFrame", 0.9f, Y0 + 0.9f, -2.2f, 2.0f, 1.6f, 0.06f, "Mil_SteelDark");
            Box(t, "FurnaceGlow", 0.9f, Y0 + 0.8f, -2.25f, 1.5f, 1.1f, 0.04f, "Mil_HeatGlow");
            Box(t, "RollDoor", -1.7f, Y0 + 1.0f, -2.2f, 1.8f, 2.0f, 0.08f, "Mil_SteelGray");
            Box(t, "RollDoorStripe", -1.7f, Y0 + 2.05f, -2.25f, 1.8f, 0.12f, 0.03f, "Mil_Hazard");
            WinZ(t, 2.2f, Y0 + 1.8f, -2.2f, 0.6f, 0.5f, "Mil_WindowGlow");

            // Side pipes and a rust tank
            Cyl(t, "PipeH", 2.75f, Y0 + 2.0f, -0.6f, 0.15f, 3.2f, "Mil_SteelGray", 90f, 0f, 0f);
            Cyl(t, "PipeV", 2.75f, Y0 + 1.2f, -2.1f, 0.15f, 2.4f, "Mil_SteelGray");
            Cyl(t, "Tank", 3.0f, Y0 + 1.2f, 1.6f, 0.7f, 2.4f, "Mil_Rust");
            Cyl(t, "TankCap", 3.0f, Y0 + 2.45f, 1.6f, 0.72f, 0.1f, "Mil_SteelDark");

            // Conveyor and ingot stacks
            Box(t, "Conveyor", -3.15f, Y0 + 0.65f, -0.5f, 0.6f, 0.15f, 2.4f, "Mil_SteelDark", 18f, 0f, 0f);
            Box(t, "ConveyorLeg", -3.15f, Y0 + 0.3f, -1.4f, 0.1f, 0.6f, 0.1f, "Mil_SteelGray");
            for (int i = 0; i < 3; i++) Box(t, "Ingot", 2.9f, Y0 + 0.1f + i * 0.2f, -2.4f, 0.9f - i * 0.15f, 0.2f, 0.5f, "Mil_MetalLight");

            CommonTiers(c, 5.6f, 4.4f, 2.8f);
        }

        // ------------------------------------------------------------------ WAREHOUSE: barrel-roof hangar, rolling door, loading dock, containers

        private static void BuildWarehouse(Ctx c)
        {
            Transform t = c.Core;
            Box(t, "Body", 0f, Y0 + 0.9f, 0f, 5.8f, 1.8f, 4.3f, "Mil_SteelGray");
            Box(t, "Plinth", 0f, Y0 + 0.15f, 0f, 5.9f, 0.3f, 4.4f, "Mil_ConcreteDark");
            Cyl(t, "BarrelRoof", 0f, Y0 + 1.8f, 0f, 2.1f, 5.8f, "Mil_PaintOliveDark", 0f, 0f, 90f);
            foreach (float x in new[] { -2.2f, 0f, 2.2f }) Cyl(t, "Rib", x, Y0 + 1.8f, 0f, 2.14f, 0.14f, "Mil_SteelDark", 0f, 0f, 90f);

            // Rolling door, personnel door, hazard stripe
            Box(t, "RollDoor", 0.6f, Y0 + 0.8f, -2.17f, 3.0f, 1.6f, 0.1f, "Mil_SteelDark");
            for (int i = 0; i < 3; i++) Box(t, "Slat", 0.6f, Y0 + 0.4f + i * 0.4f, -2.24f, 3.0f, 0.04f, 0.02f, "Mil_SteelGray");
            Box(t, "DoorStripe", 0.6f, Y0 + 1.65f, -2.24f, 3.0f, 0.12f, 0.03f, "Mil_Hazard");
            Box(t, "PersonDoor", -2.1f, Y0 + 0.7f, -2.17f, 0.8f, 1.4f, 0.1f, "Mil_SteelDark");
            WinZ(t, -2.1f, Y0 + 1.5f, -2.2f, 0.5f, 0.25f, "Mil_WindowGlow");

            // Loading dock
            Box(t, "Dock", 0.6f, Y0 + 0.2f, -2.55f, 3.4f, 0.4f, 0.8f, "Mil_ConcreteLight");
            Box(t, "DockEdge", 0.6f, Y0 + 0.42f, -2.93f, 3.4f, 0.05f, 0.04f, "Mil_Hazard");

            // Roof vents on the ridge
            Cyl(t, "VentA", -1.5f, Y0 + 4.0f, 0f, 0.2f, 0.3f, "Mil_SteelGray");
            Cyl(t, "VentB", 1.5f, Y0 + 4.0f, 0f, 0.2f, 0.3f, "Mil_SteelGray");

            // Containers beyond the (tier 3) wall, drums and pallets
            Box(t, "ContainerRust", 4.55f, Y0 + 0.55f, -1.0f, 1.0f, 1.1f, 2.4f, "Mil_Rust");
            Box(t, "ContainerGreen", 4.55f, Y0 + 0.55f, 1.6f, 1.0f, 1.1f, 2.4f, "Mil_Fabric");
            Box(t, "ContainerTop", 4.55f, Y0 + 1.65f, 0.3f, 1.0f, 1.1f, 2.4f, "Mil_SteelGray");
            Cyl(t, "DrumA", 3.2f, Y0 + 0.3f, -1.7f, 0.25f, 0.6f, "Mil_WarningRed");
            Cyl(t, "DrumB", 3.2f, Y0 + 0.3f, -1.15f, 0.25f, 0.6f, "Mil_PaintOliveDark");
            Box(t, "Pallet", -3.0f, Y0 + 0.1f, -2.1f, 0.8f, 0.2f, 0.8f, "Mil_Sandbag");
            Box(t, "Crate", -3.0f, Y0 + 0.5f, -2.1f, 0.6f, 0.6f, 0.6f, "Mil_PaintOlive");

            CommonTiers(c, 5.8f, 4.3f, 1.8f);
        }
    }
}
