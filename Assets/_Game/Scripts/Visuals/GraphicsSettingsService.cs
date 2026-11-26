using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Deadzone.Visuals
{
    /// <summary>
    /// Player graphics preference (LOW / MEDIUM / HIGH). The choice is a per-device PlayerPrefs value (it is deliberately NOT part
    /// of the save file: a cloud save moved to another phone must not inherit this phone's quality). On first launch a default is
    /// derived from device memory. Applying a preset switches the URP asset, frame-rate target and LOD bias; per-scene parts
    /// (post-processing volume, shadow softness) are handled by GraphicsApplier.
    /// </summary>
    public static class GraphicsSettingsService
    {
        public const string PrefKey = "deadzone.graphics.quality";
        public const string ProfileResource = "GraphicsProfiles";

        public static event Action<GraphicsQuality> Changed;

        private static GraphicsQuality? current;
        private static GraphicsProfileSet profileSet;
        private static bool profileSetLoaded;
        private static bool captured;
        private static int originalQualityLevel;
        private static int originalFrameRate;
        private static float originalLodBias;

        public static GraphicsQuality Current
        {
            get
            {
                if (!current.HasValue) current = Load();
                return current.Value;
            }
        }

        public static bool HasSavedPreference => PlayerPrefs.HasKey(PrefKey);

        public static GraphicsProfileSet ProfileSet
        {
            get
            {
                if (!profileSetLoaded)
                {
                    profileSet = Resources.Load<GraphicsProfileSet>(ProfileResource);
                    profileSetLoaded = true;
                }
                return profileSet;
            }
        }

        public static GraphicsProfile ActiveProfile => ProfileSet != null ? ProfileSet.Get(Current) : GraphicsProfile.Fallback(Current);

        /// <summary>First-launch default. Desktop/editor: High. Phones: by RAM (minimum supported device is 4 GB, which reports about 3.5-4 GB).</summary>
        public static GraphicsQuality DetectDefault(int systemMemoryMb, int processorCount, bool isMobile)
        {
            if (!isMobile) return GraphicsQuality.High;
            if (systemMemoryMb < 5500 || processorCount < 6) return GraphicsQuality.Low;
            if (systemMemoryMb < 7500) return GraphicsQuality.Medium;
            return GraphicsQuality.High;
        }

        public static GraphicsQuality Load()
        {
            if (PlayerPrefs.HasKey(PrefKey))
            {
                int stored = PlayerPrefs.GetInt(PrefKey, -1);
                if (Enum.IsDefined(typeof(GraphicsQuality), stored)) return (GraphicsQuality)stored;
            }
            return DetectDefault(SystemInfo.systemMemorySize, SystemInfo.processorCount, Application.isMobilePlatform);
        }

        public static void Set(GraphicsQuality quality, bool save = true)
        {
            current = quality;
            if (save)
            {
                PlayerPrefs.SetInt(PrefKey, (int)quality);
                PlayerPrefs.Save();
            }
            if (Application.isPlaying) ApplyGlobal(ActiveProfile);
            Changed?.Invoke(quality);
        }

        /// <summary>Applies the current preset to the global (non-scene) settings. Called automatically after a scene loads.</summary>
        public static void ApplyCurrent()
        {
            if (Application.isPlaying) ApplyGlobal(ActiveProfile);
        }

        private static void ApplyGlobal(GraphicsProfile profile)
        {
            Capture();

            // Preferred: the generated quality level "DZ_<Quality>" (its URP asset is then part of the build's shader stripping data).
            // Fallback: assign the profile's URP asset directly.
            int level = FindQualityLevel("DZ_" + profile.quality);
            if (level >= 0) QualitySettings.SetQualityLevel(level, true);
            else if (profile.pipelineAsset != null) QualitySettings.renderPipeline = profile.pipelineAsset;

            QualitySettings.vSyncCount = 0;
            QualitySettings.lodBias = profile.lodBias;
            Application.targetFrameRate = profile.targetFrameRate;
            RenderSettings.fog = profile.fog;
            RenderSettings.fogDensity = profile.fogDensity;
        }

        private static int FindQualityLevel(string name)
        {
            string[] names = QualitySettings.names;
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == name) return i;
            }
            return -1;
        }

        private static void Capture()
        {
            if (captured) return;
            captured = true;
            originalQualityLevel = QualitySettings.GetQualityLevel();
            originalFrameRate = Application.targetFrameRate;
            originalLodBias = QualitySettings.lodBias;
#if UNITY_EDITOR
            // Leaving Play Mode must not leave edited project settings behind.
            Application.quitting += RestoreEditorSettings;
#endif
        }

#if UNITY_EDITOR
        private static void RestoreEditorSettings()
        {
            Application.quitting -= RestoreEditorSettings;
            QualitySettings.SetQualityLevel(originalQualityLevel, true);
            QualitySettings.lodBias = originalLodBias;
            Application.targetFrameRate = originalFrameRate;
            captured = false;
        }
#endif

        /// <summary>Test hook: forgets cached state (does not touch PlayerPrefs).</summary>
        public static void ResetForTests()
        {
            current = null;
            profileSet = null;
            profileSetLoaded = false;
            Changed = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            current = null;
            profileSet = null;
            profileSetLoaded = false;
            captured = false;
            Changed = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ApplyAfterSceneLoad() => ApplyCurrent();
    }
}
