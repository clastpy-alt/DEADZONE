using System;
using System.Text;
using Deadzone.Visuals;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Deadzone.EditorTools
{
    /// <summary>
    /// PHASE 02.58: creates the LOW / MEDIUM / HIGH graphics presets as assets: three URP pipeline assets (copies of the project's
    /// Mobile asset with different render scale, MSAA, HDR, shadow resolution/distance/softness), a High renderer with SSAO,
    /// post-processing volume profiles (Medium: grading + vignette, High: + bloom), the Resources/GraphicsProfiles.asset that the
    /// game reads, and three quality levels (DZ_Low/DZ_Medium/DZ_High) so every URP asset is part of the build.
    /// Existing assets are never overwritten (hand edits survive).
    /// </summary>
    internal static class GraphicsProfileGenerator
    {
        public const string Folder = EditorPaths.Root + "/Settings/Graphics";
        public const string SetPath = EditorPaths.Resources + "/GraphicsProfiles.asset";
        private const string MobileRp = "Assets/Settings/Mobile_RPAsset.asset";
        private const string PcRenderer = "Assets/Settings/PC_Renderer.asset";

        public static string Generate()
        {
            var report = new StringBuilder();
            EditorPaths.EnsureFolder(Folder);
            EditorPaths.EnsureFolder(EditorPaths.Resources);

            if (AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(MobileRp) == null)
            {
                return "Missing " + MobileRp + " (URP template asset). Cannot generate graphics presets.";
            }

            // High renderer = PC renderer (has SSAO) switched to Forward so it also suits phones.
            string highRendererPath = Folder + "/Renderer_High.asset";
            if (AssetDatabase.LoadMainAssetAtPath(highRendererPath) == null)
            {
                if (AssetDatabase.CopyAsset(PcRenderer, highRendererPath))
                {
                    TuneHighRenderer(highRendererPath);
                    report.AppendLine("Renderer_High created (Forward + SSAO)");
                }
                else report.AppendLine("Could not copy " + PcRenderer);
            }
            var highRenderer = AssetDatabase.LoadMainAssetAtPath(highRendererPath);

            RenderPipelineAsset low = CreateRp("URP_Low", null, so =>
            {
                F(so, "m_RenderScale", 0.75f); I(so, "m_MSAA", 1); B(so, "m_SupportsHDR", false);
                I(so, "m_MainLightShadowmapResolution", 512); F(so, "m_ShadowDistance", 25f); I(so, "m_ShadowCascadeCount", 1);
                B(so, "m_SoftShadowsSupported", false); I(so, "m_AdditionalLightsRenderingMode", 0);
            }, report);
            RenderPipelineAsset medium = CreateRp("URP_Medium", null, so =>
            {
                F(so, "m_RenderScale", 0.9f); I(so, "m_MSAA", 2); B(so, "m_SupportsHDR", false);
                I(so, "m_MainLightShadowmapResolution", 1024); F(so, "m_ShadowDistance", 38f); I(so, "m_ShadowCascadeCount", 1);
                B(so, "m_SoftShadowsSupported", false); I(so, "m_AdditionalLightsRenderingMode", 1);
            }, report);
            RenderPipelineAsset high = CreateRp("URP_High", highRenderer, so =>
            {
                F(so, "m_RenderScale", 1f); I(so, "m_MSAA", 4); B(so, "m_SupportsHDR", true);
                I(so, "m_MainLightShadowmapResolution", 2048); F(so, "m_ShadowDistance", 55f); I(so, "m_ShadowCascadeCount", 2);
                B(so, "m_SoftShadowsSupported", true); I(so, "m_SoftShadowQuality", 2); I(so, "m_AdditionalLightsRenderingMode", 1);
            }, report);

            VolumeProfile mediumVolume = CreateVolume("Volume_Medium", false, report);
            VolumeProfile highVolume = CreateVolume("Volume_High", true, report);

            // The preset asset the game loads.
            var set = AssetDatabase.LoadAssetAtPath<GraphicsProfileSet>(SetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<GraphicsProfileSet>();
                set.profiles.Add(new GraphicsProfile { quality = GraphicsQuality.Low, pipelineAsset = low, volumeProfile = null, postProcessing = false, softShadows = false, targetFrameRate = 30, lodBias = 0.7f, fog = false, fogDensity = 0.002f });
                set.profiles.Add(new GraphicsProfile { quality = GraphicsQuality.Medium, pipelineAsset = medium, volumeProfile = mediumVolume, postProcessing = true, softShadows = false, targetFrameRate = 45, lodBias = 1f, fog = true, fogDensity = 0.0018f });
                set.profiles.Add(new GraphicsProfile { quality = GraphicsQuality.High, pipelineAsset = high, volumeProfile = highVolume, postProcessing = true, softShadows = true, targetFrameRate = 60, lodBias = 1.4f, fog = true, fogDensity = 0.0022f });
                AssetDatabase.CreateAsset(set, SetPath);
                report.AppendLine("GraphicsProfiles.asset created");
            }

            report.AppendLine(EnsureQualityLevels(low, medium, high));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return report.ToString().Trim();
        }

        // ------------------------------------------------------------------ URP assets

        private static RenderPipelineAsset CreateRp(string name, UnityEngine.Object renderer, Action<SerializedObject> tune, StringBuilder report)
        {
            string path = Folder + "/" + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path);
            if (existing != null) return existing;

            if (!AssetDatabase.CopyAsset(MobileRp, path))
            {
                report.AppendLine("Could not create " + name);
                return null;
            }

            var asset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(path);
            var so = new SerializedObject(asset);
            tune(so);
            if (renderer != null)
            {
                SerializedProperty list = so.FindProperty("m_RendererDataList");
                if (list != null && list.isArray && list.arraySize > 0) list.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
                else report.AppendLine(name + ": m_RendererDataList not found (renderer unchanged)");
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            report.AppendLine(name + " created");
            return asset;
        }

        private static void TuneHighRenderer(string path)
        {
            foreach (UnityEngine.Object obj in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (obj == null) continue;
                var so = new SerializedObject(obj);
                SerializedProperty mode = so.FindProperty("m_RenderingMode");
                if (mode != null)
                {
                    mode.intValue = 0; // Forward
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                SerializedProperty downsample = so.FindProperty("m_Settings.Downsample");
                if (downsample != null)
                {
                    downsample.boolValue = true; // half-resolution AO is plenty for an orthographic base view
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                EditorUtility.SetDirty(obj);
            }
        }

        private static void F(SerializedObject so, string name, float value)
        {
            SerializedProperty p = so.FindProperty(name);
            if (p == null) Debug.LogWarning("[DEADZONE] URP property not found: " + name);
            else p.floatValue = value;
        }

        private static void I(SerializedObject so, string name, int value)
        {
            SerializedProperty p = so.FindProperty(name);
            if (p == null) Debug.LogWarning("[DEADZONE] URP property not found: " + name);
            else p.intValue = value;
        }

        private static void B(SerializedObject so, string name, bool value)
        {
            SerializedProperty p = so.FindProperty(name);
            if (p == null) Debug.LogWarning("[DEADZONE] URP property not found: " + name);
            else p.boolValue = value;
        }

        // ------------------------------------------------------------------ volume profiles

        private static VolumeProfile CreateVolume(string name, bool high, StringBuilder report)
        {
            string path = Folder + "/" + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (existing != null) return existing;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, path);

            T Add<T>() where T : VolumeComponent
            {
                var component = profile.Add<T>(true);
                component.name = typeof(T).Name;
                AssetDatabase.AddObjectToAsset(component, profile);
                return component;
            }

            var tone = Add<Tonemapping>();
            tone.mode.Override(TonemappingMode.Neutral);

            // "Dark but readable": slight lift in exposure/contrast, a touch of warmth, restrained saturation.
            var grade = Add<ColorAdjustments>();
            grade.postExposure.Override(0.25f);
            grade.contrast.Override(12f);
            grade.saturation.Override(4f);
            grade.colorFilter.Override(new Color(1f, 0.975f, 0.93f));

            var vignette = Add<Vignette>();
            vignette.intensity.Override(0.2f);
            vignette.smoothness.Override(0.45f);

            if (high)
            {
                var bloom = Add<Bloom>();
                bloom.threshold.Override(1.05f);
                bloom.intensity.Override(0.22f);
                bloom.scatter.Override(0.55f);
                bloom.highQualityFiltering.Override(false);
            }

            EditorUtility.SetDirty(profile);
            report.AppendLine(name + " created");
            return profile;
        }

        // ------------------------------------------------------------------ quality levels

        /// <summary>Adds DZ_Low / DZ_Medium / DZ_High quality levels (each with its URP asset) when missing. Existing levels are untouched.</summary>
        private static string EnsureQualityLevels(RenderPipelineAsset low, RenderPipelineAsset medium, RenderPipelineAsset high)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset");
            if (assets == null || assets.Length == 0) return "QualitySettings.asset not readable: quality levels not created (runtime falls back to assigning the URP asset).";

            var so = new SerializedObject(assets[0]);
            SerializedProperty levels = so.FindProperty("m_QualitySettings");
            if (levels == null || !levels.isArray || levels.arraySize == 0) return "m_QualitySettings not found: quality levels not created.";

            var result = new StringBuilder();
            var wanted = new (string name, RenderPipelineAsset asset)[] { ("DZ_Low", low), ("DZ_Medium", medium), ("DZ_High", high) };
            foreach (var entry in wanted)
            {
                if (entry.asset == null || FindLevel(levels, entry.name) >= 0) continue;

                int source = FindLevel(levels, "Mobile");
                if (source < 0) source = 0;
                levels.InsertArrayElementAtIndex(source);
                SerializedProperty added = levels.GetArrayElementAtIndex(levels.arraySize - 1);
                // InsertArrayElementAtIndex duplicates 'source' right after it; move the copy to the end for stable indices.
                int insertedAt = source + 1;
                if (insertedAt != levels.arraySize - 1) levels.MoveArrayElement(insertedAt, levels.arraySize - 1);
                added = levels.GetArrayElementAtIndex(levels.arraySize - 1);

                added.FindPropertyRelative("name").stringValue = entry.name;
                added.FindPropertyRelative("customRenderPipeline").objectReferenceValue = entry.asset;
                SerializedProperty excluded = added.FindPropertyRelative("excludedTargetPlatforms");
                if (excluded != null) excluded.ClearArray();
                result.Append(entry.name).Append(' ');
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return result.Length == 0 ? "Quality levels already present." : "Quality levels added: " + result.ToString().Trim();
        }

        private static int FindLevel(SerializedProperty levels, string name)
        {
            for (int i = 0; i < levels.arraySize; i++)
            {
                SerializedProperty n = levels.GetArrayElementAtIndex(i).FindPropertyRelative("name");
                if (n != null && n.stringValue == name) return i;
            }
            return -1;
        }
    }
}
