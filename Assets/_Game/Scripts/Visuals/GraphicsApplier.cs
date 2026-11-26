using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Deadzone.Visuals
{
    /// <summary>
    /// Per-scene part of the graphics preset: toggles the camera's post-processing, swaps the global volume's profile and sets the
    /// sun's shadow softness. Lives on the "Lighting Rig" object of the Base scene; follows live changes from the settings panel.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GraphicsApplier : MonoBehaviour
    {
        [SerializeField] private Volume volume;
        [SerializeField] private Light sun;
        [SerializeField] private Camera targetCamera;

        public void Bind(Volume globalVolume, Light sunLight, Camera camera)
        {
            volume = globalVolume;
            sun = sunLight;
            targetCamera = camera;
        }

        private void OnEnable()
        {
            GraphicsSettingsService.Changed += OnChanged;
            GraphicsSettingsService.ApplyCurrent();
            Apply();
        }

        private void OnDisable() => GraphicsSettingsService.Changed -= OnChanged;

        private void OnChanged(GraphicsQuality quality) => Apply();

        public void Apply()
        {
            GraphicsProfile profile = GraphicsSettingsService.ActiveProfile;

            Camera cam = targetCamera != null ? targetCamera : Camera.main;
            if (cam != null)
            {
                UniversalAdditionalCameraData data = cam.GetUniversalAdditionalCameraData();
                data.renderPostProcessing = profile.postProcessing && profile.volumeProfile != null;
                data.renderShadows = true;
            }

            if (volume != null)
            {
                volume.sharedProfile = profile.volumeProfile;
                volume.enabled = profile.postProcessing && profile.volumeProfile != null;
            }

            if (sun != null) sun.shadows = profile.softShadows ? LightShadows.Soft : LightShadows.Hard;
        }
    }
}
