using UnityEngine;

namespace Deadzone.EditorTools
{
    /// <summary>
    /// PLACEHOLDER environment built from primitives (walls, roads, towers, trees, vehicles...).
    /// Real assets replace these in PHASE 09. Everything is deterministic (seeded) so re-generation is stable.
    /// </summary>
    internal static class PlaceholderModels
    {
        public static void BuildEnvironment(Transform root)
        {
            Material ground = MaterialLibrary.Get("Asphalt", "#1B1F24");
            Material road = MaterialLibrary.Get("Road", "#2B3036");
            Material concrete = MaterialLibrary.Get("Concrete", "#5E6670");
            Material concreteDark = MaterialLibrary.Get("ConcreteDark", "#444B54");
            Material roof = MaterialLibrary.Get("Roof", "#2A2F36");

            ModelBuilder.Prim(PrimitiveType.Cube, "Ground", root, new Vector3(0, -0.5f, 0), new Vector3(200, 1, 200), ground);

            // Concrete base pad inside the walls
            ModelBuilder.Prim(PrimitiveType.Cube, "ConcretePad", root, new Vector3(0, 0.02f, 0), new Vector3(47, 0.04f, 47), MaterialLibrary.Get("PadConcrete", "#4A5159"));

            // Roads
            ModelBuilder.Prim(PrimitiveType.Cube, "Road_Main", root, new Vector3(0, 0.03f, -11), new Vector3(5, 0.06f, 46), road);
            ModelBuilder.Prim(PrimitiveType.Cube, "Road_Cross", root, new Vector3(0, 0.03f, -2), new Vector3(44, 0.06f, 4), road);
            ModelBuilder.Prim(PrimitiveType.Cube, "Road_Outside", root, new Vector3(0, 0.03f, -45), new Vector3(5, 0.06f, 30), road);

            // Perimeter walls (south side has a gate gap)
            Wall(root, "Wall_N", new Vector3(0, 1.1f, 24), new Vector3(48, 2.2f, 1), concrete);
            Wall(root, "Wall_E", new Vector3(24, 1.1f, 0), new Vector3(1, 2.2f, 48), concrete);
            Wall(root, "Wall_W", new Vector3(-24, 1.1f, 0), new Vector3(1, 2.2f, 48), concrete);
            Wall(root, "Wall_S_Left", new Vector3(-14, 1.1f, -24), new Vector3(20, 2.2f, 1), concrete);
            Wall(root, "Wall_S_Right", new Vector3(14, 1.1f, -24), new Vector3(20, 2.2f, 1), concrete);

            // Checkpoint
            for (int side = -1; side <= 1; side += 2)
            {
                Transform post = ModelBuilder.Group("GuardPost_PH", root, new Vector3(side * 5.5f, 0, -24));
                ModelBuilder.Prim(PrimitiveType.Cube, "Body", post, new Vector3(0, 1.3f, 0), new Vector3(2f, 2.6f, 2f), concreteDark);
                ModelBuilder.Prim(PrimitiveType.Cube, "Roof", post, new Vector3(0, 2.75f, 0), new Vector3(2.5f, 0.2f, 2.5f), roof);
            }
            ModelBuilder.Prim(PrimitiveType.Cube, "Barrier", root, new Vector3(-1.4f, 1.0f, -26), new Vector3(5.6f, 0.15f, 0.15f), MaterialLibrary.Get("BarrierRed", "#B33A3A"));

            // Watch towers at the corners
            Tower(root, new Vector3(-24, 0, 24));
            Tower(root, new Vector3(24, 0, 24));
            Tower(root, new Vector3(-24, 0, -24));
            Tower(root, new Vector3(24, 0, -24));

            // Street lamps along the main road
            for (int i = 0; i < 6; i++)
            {
                float z = -34f + i * 9f;
                Lamp(root, new Vector3(3.6f, 0, z));
                Lamp(root, new Vector3(-3.6f, 0, z));
            }

            // Sandbags at the gate
            Sandbags(root, new Vector3(-3.6f, 0, -22.5f));
            Sandbags(root, new Vector3(3.6f, 0, -22.5f));

            // Containers and crates
            Container(root, new Vector3(16, 0, -19), 0, "ContainerOrange", "#A85A2A");
            Container(root, new Vector3(16, 2.6f, -19), 0, "ContainerOrange", "#A85A2A");
            Container(root, new Vector3(16, 0, -22), 0, "ContainerGreen", "#3F5A3F");
            Material crate = MaterialLibrary.Get("Crate", "#5B6B4A");
            for (int i = 0; i < 4; i++)
            {
                ModelBuilder.Prim(PrimitiveType.Cube, "MilitaryCrate_PH", root, new Vector3(-6f - (i % 2) * 1.2f, 0.45f + (i / 2) * 0.9f, 10f), new Vector3(1.1f, 0.9f, 1.1f), crate);
            }

            // Vehicles
            Truck(root, new Vector3(-5.5f, 0, -19f), 90f);
            Tank(root, new Vector3(5.5f, 0, -20f), -90f);

            // Forest ring (seeded, outside the walls, keeping the entrance road clear)
            var rng = new System.Random(1337);
            Transform forest = ModelBuilder.Group("Forest_PH", root);
            int placed = 0;
            for (int attempt = 0; attempt < 400 && placed < 90; attempt++)
            {
                double angle = rng.NextDouble() * System.Math.PI * 2.0;
                double radius = 32.0 + rng.NextDouble() * 32.0;
                float x = (float)(System.Math.Cos(angle) * radius);
                float z = (float)(System.Math.Sin(angle) * radius);
                if (Mathf.Abs(x) < 6f && z < 0f) continue;
                Tree(forest, new Vector3(x, 0, z), 0.8f + (float)rng.NextDouble() * 0.9f);
                placed++;
            }

            // Abandoned houses and rubble outside the walls
            House(root, new Vector3(36, 0, 4), 10f);
            House(root, new Vector3(40, 0, 17), -15f);
            House(root, new Vector3(-37, 0, -6), 80f);
            Material rubble = MaterialLibrary.Get("Rubble", "#3A3C40");
            Transform debris = ModelBuilder.Group("Debris_PH", root);
            for (int i = 0; i < 28; i++)
            {
                float a = (float)(rng.NextDouble() * System.Math.PI * 2.0);
                float r = 27f + (float)rng.NextDouble() * 12f;
                float s = 0.3f + (float)rng.NextDouble() * 0.9f;
                ModelBuilder.Prim(PrimitiveType.Cube, "Rubble", debris,
                    new Vector3(Mathf.Cos(a) * r, s * 0.4f, Mathf.Sin(a) * r),
                    new Vector3(s, s * 0.8f, s * 1.2f), rubble, new Vector3(0, (float)rng.NextDouble() * 180f, 0));
            }

            // Static batching hint for the whole environment
            foreach (Transform child in root.GetComponentsInChildren<Transform>())
            {
                child.gameObject.isStatic = true;
            }
        }

        private static void Wall(Transform parent, string name, Vector3 pos, Vector3 scale, Material material)
        {
            ModelBuilder.Prim(PrimitiveType.Cube, name, parent, pos, scale, material);
        }

        private static void Tower(Transform parent, Vector3 pos)
        {
            Material concrete = MaterialLibrary.Get("ConcreteDark", "#444B54");
            Material roof = MaterialLibrary.Get("Roof", "#2A2F36");
            Transform t = ModelBuilder.Group("WatchTower_PH", parent, pos);
            ModelBuilder.Prim(PrimitiveType.Cube, "Column", t, new Vector3(0, 3f, 0), new Vector3(1.2f, 6f, 1.2f), concrete);
            ModelBuilder.Prim(PrimitiveType.Cube, "Cabin", t, new Vector3(0, 6.8f, 0), new Vector3(3f, 1.6f, 3f), concrete);
            ModelBuilder.Prim(PrimitiveType.Cube, "Roof", t, new Vector3(0, 7.8f, 0), new Vector3(3.5f, 0.25f, 3.5f), roof);
        }

        private static void Lamp(Transform parent, Vector3 pos)
        {
            Material metal = MaterialLibrary.Get("Metal", "#6F7A86");
            Material glow = MaterialLibrary.Get("LampGlow", "#FFD9A0", "#FFB45A");
            Transform t = ModelBuilder.Group("StreetLamp_PH", parent, pos);
            ModelBuilder.Prim(PrimitiveType.Cylinder, "Pole", t, new Vector3(0, 2.2f, 0), new Vector3(0.12f, 2.2f, 0.12f), metal);
            ModelBuilder.Prim(PrimitiveType.Sphere, "Head", t, new Vector3(0, 4.5f, 0), new Vector3(0.5f, 0.35f, 0.5f), glow);
        }

        private static void Sandbags(Transform parent, Vector3 pos)
        {
            Material sand = MaterialLibrary.Get("Sandbag", "#7A6A4A");
            Transform t = ModelBuilder.Group("Sandbags_PH", parent, pos);
            for (int layer = 0; layer < 2; layer++)
            {
                for (int i = 0; i < 4; i++)
                {
                    float x = (i - 1.5f) * 1.15f + (layer % 2) * 0.5f;
                    ModelBuilder.Prim(PrimitiveType.Cube, "Bag", t, new Vector3(x, 0.2f + layer * 0.38f, 0), new Vector3(1.1f, 0.36f, 0.55f), sand);
                }
            }
        }

        private static void Container(Transform parent, Vector3 pos, float yaw, string materialName, string hex)
        {
            Material m = MaterialLibrary.Get(materialName, hex);
            ModelBuilder.Prim(PrimitiveType.Cube, "Container_PH", parent, pos + new Vector3(0, 1.3f, 0), new Vector3(6f, 2.6f, 2.4f), m, new Vector3(0, yaw, 0));
        }

        private static void Truck(Transform parent, Vector3 pos, float yaw)
        {
            Material green = MaterialLibrary.Get("MilitaryGreen", "#465744");
            Material dark = MaterialLibrary.Get("Trim", "#2F353C");
            Transform t = ModelBuilder.Group("MilitaryTruck_PH", parent, pos, new Vector3(0, yaw, 0));
            ModelBuilder.Prim(PrimitiveType.Cube, "Cargo", t, new Vector3(0, 1.5f, -1.2f), new Vector3(2.4f, 2f, 4.2f), green);
            ModelBuilder.Prim(PrimitiveType.Cube, "Cab", t, new Vector3(0, 1.4f, 1.9f), new Vector3(2.4f, 1.8f, 1.8f), green);
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -1.25f : 1.25f);
                float z = (i < 2 ? -2.0f : 1.8f);
                ModelBuilder.Prim(PrimitiveType.Cylinder, "Wheel", t, new Vector3(x, 0.55f, z), new Vector3(1.1f, 0.2f, 1.1f), dark, new Vector3(0, 0, 90));
            }
        }

        private static void Tank(Transform parent, Vector3 pos, float yaw)
        {
            Material green = MaterialLibrary.Get("MilitaryGreen", "#465744");
            Material dark = MaterialLibrary.Get("Trim", "#2F353C");
            Transform t = ModelBuilder.Group("Tank_PH", parent, pos, new Vector3(0, yaw, 0));
            ModelBuilder.Prim(PrimitiveType.Cube, "Hull", t, new Vector3(0, 0.9f, 0), new Vector3(2.6f, 1.0f, 4.6f), green);
            ModelBuilder.Prim(PrimitiveType.Cube, "TrackL", t, new Vector3(-1.4f, 0.6f, 0), new Vector3(0.6f, 1.0f, 4.8f), dark);
            ModelBuilder.Prim(PrimitiveType.Cube, "TrackR", t, new Vector3(1.4f, 0.6f, 0), new Vector3(0.6f, 1.0f, 4.8f), dark);
            ModelBuilder.Prim(PrimitiveType.Cube, "Turret", t, new Vector3(0, 1.8f, -0.2f), new Vector3(1.8f, 0.8f, 2.0f), green);
            ModelBuilder.Prim(PrimitiveType.Cylinder, "Barrel", t, new Vector3(0, 1.9f, 2.0f), new Vector3(0.22f, 1.6f, 0.22f), dark, new Vector3(90, 0, 0));
        }

        private static void Tree(Transform parent, Vector3 pos, float s)
        {
            Material trunk = MaterialLibrary.Get("Trunk", "#4A3A2A");
            Material leaves = MaterialLibrary.Get("Foliage", "#2F4A2E");
            Transform t = ModelBuilder.Group("Tree_PH", parent, pos);
            ModelBuilder.Prim(PrimitiveType.Cylinder, "Trunk", t, new Vector3(0, 1.2f * s, 0), new Vector3(0.35f * s, 1.2f * s, 0.35f * s), trunk);
            ModelBuilder.Prim(PrimitiveType.Sphere, "Leaves1", t, new Vector3(0, 3.0f * s, 0), new Vector3(2.4f * s, 2.4f * s, 2.4f * s), leaves);
            ModelBuilder.Prim(PrimitiveType.Sphere, "Leaves2", t, new Vector3(0, 4.3f * s, 0), new Vector3(1.7f * s, 1.7f * s, 1.7f * s), leaves);
        }

        private static void House(Transform parent, Vector3 pos, float yaw)
        {
            Material wall = MaterialLibrary.Get("RuinWall", "#4B4E52");
            Material roof = MaterialLibrary.Get("Roof", "#2A2F36");
            Transform t = ModelBuilder.Group("AbandonedHouse_PH", parent, pos, new Vector3(0, yaw, 0));
            ModelBuilder.Prim(PrimitiveType.Cube, "Body", t, new Vector3(0, 1.6f, 0), new Vector3(6f, 3.2f, 5f), wall);
            ModelBuilder.Prim(PrimitiveType.Cube, "Roof", t, new Vector3(0.4f, 3.5f, 0), new Vector3(6.6f, 0.3f, 5.6f), roof, new Vector3(0, 0, 8f));
            ModelBuilder.Prim(PrimitiveType.Cube, "CollapsedWall", t, new Vector3(3.6f, 0.5f, 1f), new Vector3(1.6f, 1f, 2.2f), wall, new Vector3(0, 25f, 0));
        }
    }
}
