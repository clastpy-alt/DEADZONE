using Deadzone.Buildings;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.UI
{
    /// <summary>
    /// Large live preview of the selected building for the info sheet (PHASE 02.5): a clone of the building's generated visual
    /// is placed far away from the base and turned slowly in front of a small off-screen camera that renders into a RenderTexture.
    /// It only renders while a building is selected, at 15 frames per second, and uses the scene lighting, so it matches the base.
    /// A building without a generated visual simply shows no preview.
    /// </summary>
    public sealed class BuildingPreview : MonoBehaviour
    {
        private const int TextureSize = 384;
        private const float RenderInterval = 1f / 15f;
        private const float TurnSpeed = 22f;
        private static readonly Vector3 Origin = new Vector3(4000f, 0f, 4000f);

        [SerializeField] private RawImage target;

        private RenderTexture texture;
        private Camera previewCamera;
        private Transform pivot;
        private GameObject clone;
        private Vector3 center;
        private float distance;
        private float yaw = 35f;
        private float nextRender;

        public void Bind(RawImage image) => target = image;

        public void Show(BuildingSelectable building)
        {
            Clear();
            if (building == null || target == null) return;

            // The scene instance is static-batched at runtime (a clone of it renders nothing), so the clone is made from the prefab
            // asset and the tier groups are then switched to the same state as the scene instance.
            Transform live = building.transform.Find("Mil_Visual");
            if (live == null) return;
            GameObject prefab = building.Definition != null ? building.Definition.prefab : null;
            Transform source = prefab != null ? prefab.transform.Find("Mil_Visual") : null;

            EnsureRig();
            clone = Instantiate(source != null ? source.gameObject : live.gameObject, pivot);
            clone.transform.localPosition = live.localPosition;
            clone.transform.localRotation = live.localRotation;
            clone.SetActive(true);
            CopyActiveStates(live, clone.transform);

            Renderer[] renderers = clone.GetComponentsInChildren<Renderer>(false);
            if (renderers.Length == 0)
            {
                Debug.LogWarning($"[DEADZONE] BuildingPreview: '{building.BuildingId}' has no active renderers in its Mil_Visual clone (prefab visual: {(source != null)}).");
                Clear();
                return;
            }
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);

            center = bounds.center;
            float radius = Mathf.Max(0.5f, bounds.extents.magnitude);
            distance = radius / Mathf.Sin(previewCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 1.02f;
            previewCamera.nearClipPlane = Mathf.Max(0.1f, distance - radius * 1.5f);
            previewCamera.farClipPlane = distance + radius * 2f;

            target.texture = texture;
            target.enabled = true;
            previewCamera.enabled = true;
            nextRender = 0f;
        }

        public void Clear()
        {
            if (clone != null) Destroy(clone);
            clone = null;
            if (previewCamera != null) previewCamera.enabled = false;
            if (target != null) target.enabled = false;
        }

        private static void CopyActiveStates(Transform from, Transform to)
        {
            int count = Mathf.Min(from.childCount, to.childCount);
            for (int i = 0; i < count; i++)
            {
                Transform source = from.GetChild(i);
                Transform target = to.GetChild(i);
                if (source.name != target.name) continue;
                target.gameObject.SetActive(source.gameObject.activeSelf);
                CopyActiveStates(source, target);
            }
        }

        private void EnsureRig()
        {
            if (previewCamera != null) return;

            texture = new RenderTexture(TextureSize, TextureSize, 24, RenderTextureFormat.ARGB32) { name = "BuildingPreview" };

            var rig = new GameObject("BuildingPreviewRig");
            rig.transform.position = Origin;
            pivot = new GameObject("Pivot").transform;
            pivot.SetParent(rig.transform, false);

            // The camera must NOT be a parent/child of the pivot: it is moved every frame to orbit the building.
            var cameraObject = new GameObject("BuildingPreviewCamera");
            cameraObject.transform.SetParent(rig.transform, false);
            previewCamera = cameraObject.AddComponent<Camera>();
            previewCamera.enabled = false; // switched on only while a building is shown (URP renders enabled cameras itself)
            previewCamera.targetTexture = texture;
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color32(0x16, 0x1C, 0x23, 255);
            previewCamera.fieldOfView = 24f;
            previewCamera.allowHDR = false;
            previewCamera.allowMSAA = false;
            previewCamera.depth = -20f;
        }

        private void LateUpdate()
        {
            if (clone == null || previewCamera == null || Time.unscaledTime < nextRender) return;
            nextRender = Time.unscaledTime + RenderInterval;

            yaw += TurnSpeed * RenderInterval;
            Quaternion rotation = Quaternion.Euler(30f, yaw, 0f);
            previewCamera.transform.position = center - rotation * Vector3.forward * distance;
            previewCamera.transform.rotation = rotation;
        }

        private void OnDestroy()
        {
            if (previewCamera != null && previewCamera.transform.parent != null) Destroy(previewCamera.transform.parent.gameObject);
            if (texture != null) texture.Release();
        }
    }
}
