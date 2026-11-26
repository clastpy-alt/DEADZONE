using System.IO;
using System.Text;
using Deadzone.Core;
using Deadzone.UI;
using Deadzone.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Deadzone.EditorTools
{
    /// <summary>
    /// PHASE 02.58: lighting and atmosphere of the Base scene. A warm "late afternoon" sun with soft shadows, a tri-colour ambient,
    /// light fog, a custom baked reflection probe, a light-probe grid, a global post-processing volume and the GraphicsApplier that
    /// follows the LOW/MEDIUM/HIGH preference. Everything generated sits under one "Lighting Rig [GENERATED]" root; scene-wide
    /// values (sun, ambient, fog) are only written on the first run so later manual tuning survives. The rig is made static-friendly
    /// (probes + reflection probe) but NO lightmap bake is run: the base uses real-time lighting for now.
    /// </summary>
    internal static class LightingSetup
    {
        public const string RigName = "Lighting Rig [GENERATED]";
        public const string ReflectionPath = EditorPaths.Root + "/Lighting/BaseReflection.exr";

        public static string ApplyToBaseScene(bool rebuild)
        {
            string path = EditorPaths.ScenePath(SceneNames.Base);
            if (!File.Exists(path)) return "Base scene not found (run Setup Phase 01 first).";

            Scene previousActive = SceneManager.GetActiveScene();
            Scene existing = SceneManager.GetSceneByPath(path);
            bool wasLoaded = existing.IsValid() && existing.isLoaded;
            Scene scene = wasLoaded ? existing : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            var report = new StringBuilder();

            try
            {
                SceneManager.SetActiveScene(scene);

                GameObject oldRig = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (root.name == RigName) oldRig = root;
                }
                if (oldRig != null && !rebuild) return "Lighting rig already present (kept).";
                bool firstRun = oldRig == null;
                if (oldRig != null) Object.DestroyImmediate(oldRig);

                Light sun = FindSun(scene);
                Camera cam = FindCamera(scene);

                if (firstRun || rebuild)
                {
                    ConfigureSun(sun);
                    ConfigureAmbientAndFog();
                    if (cam != null) cam.backgroundColor = UIColors.Hex("#12171D");
                    report.AppendLine("Sun, ambient and fog configured.");
                }

                var rig = new GameObject(RigName);
                SceneManager.MoveGameObjectToScene(rig, scene);

                // Global post-processing volume (profile switched at runtime by the LOW/MEDIUM/HIGH preference).
                var volumeObject = new GameObject("Global Volume");
                volumeObject.transform.SetParent(rig.transform, false);
                var volume = volumeObject.AddComponent<Volume>();
                volume.isGlobal = true;
                volume.weight = 1f;
                volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(GraphicsProfileGenerator.Folder + "/Volume_High.asset");

                // Reflection probe covering the whole base.
                var probeObject = new GameObject("Base Reflection Probe");
                probeObject.transform.SetParent(rig.transform, false);
                probeObject.transform.position = new Vector3(0f, 9f, 0f);
                var probe = probeObject.AddComponent<ReflectionProbe>();
                probe.size = new Vector3(70f, 26f, 70f);
                probe.resolution = 128;
                probe.hdr = true;
                probe.boxProjection = true;
                probe.intensity = 0.9f;
                report.AppendLine(BakeReflection(probe));

                // Light probe grid around buildings and roads (ready for a future bake).
                var probesObject = new GameObject("Light Probes");
                probesObject.transform.SetParent(rig.transform, false);
                var probes = probesObject.AddComponent<LightProbeGroup>();
                var positions = new System.Collections.Generic.List<Vector3>();
                for (int x = -20; x <= 20; x += 10)
                {
                    for (int z = -20; z <= 20; z += 10)
                    {
                        positions.Add(new Vector3(x, 1.5f, z));
                        positions.Add(new Vector3(x, 5f, z));
                    }
                }
                probes.probePositions = positions.ToArray();

                var applier = rig.AddComponent<GraphicsApplier>();
                applier.Bind(volume, sun, cam);

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
                report.AppendLine("Lighting rig built (volume, reflection probe, " + positions.Count + " light probes, graphics applier).");
                return report.ToString().Trim();
            }
            finally
            {
                if (previousActive.IsValid() && previousActive.isLoaded) SceneManager.SetActiveScene(previousActive);
                if (!wasLoaded) EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static Light FindSun(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Light light in root.GetComponentsInChildren<Light>(true))
                {
                    if (light.type == LightType.Directional) return light;
                }
            }
            return null;
        }

        private static Camera FindCamera(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Camera cam = root.GetComponentInChildren<Camera>(true);
                if (cam != null) return cam;
            }
            return null;
        }

        private static void ConfigureSun(Light sun)
        {
            if (sun == null) return;
            sun.color = UIColors.Hex("#FFDDB0");
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.85f;
            sun.lightmapBakeType = LightmapBakeType.Mixed;
            sun.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            EditorUtility.SetDirty(sun);
        }

        private static void ConfigureAmbientAndFog()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = UIColors.Hex("#6B7C93");
            RenderSettings.ambientEquatorColor = UIColors.Hex("#48545F");
            RenderSettings.ambientGroundColor = UIColors.Hex("#2A2F35");
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = UIColors.Hex("#1B222B");
            RenderSettings.fogDensity = 0.0022f;
        }

        private static string BakeReflection(ReflectionProbe probe)
        {
            try
            {
                EditorPaths.EnsureFolder(EditorPaths.Root + "/Lighting");
                if (!Lightmapping.BakeReflectionProbe(probe, ReflectionPath)) return "Reflection probe bake failed (probe left empty).";

                AssetDatabase.ImportAsset(ReflectionPath);
                var texture = AssetDatabase.LoadAssetAtPath<Texture>(ReflectionPath);
                if (texture == null) return "Reflection bake produced no texture.";

                probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Custom;
                probe.customBakedTexture = texture;
                RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
                RenderSettings.customReflectionTexture = texture;
                return "Reflection probe baked to " + ReflectionPath;
            }
            catch (System.Exception exception)
            {
                return "Reflection probe bake threw: " + exception.Message;
            }
        }
    }
}
