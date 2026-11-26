using Deadzone.Core;
using Deadzone.UI;
using TMPro;
using UnityEngine;

namespace Deadzone.Buildings
{
    /// <summary>
    /// Billboarded label above a building: level, remaining construction time or collectable output.
    /// Created at runtime by BuildingSelectable, so building prefabs need no extra wiring.
    /// </summary>
    public sealed class BuildingWorldLabel : MonoBehaviour
    {
        private const float RefreshInterval = 0.25f;

        private BuildingSelectable owner;
        private TextMeshPro text;
        private Camera cam;
        private float nextRefresh;

        public static void Attach(BuildingSelectable owner)
        {
            var go = new GameObject("WorldLabel [PLACEHOLDER]");
            go.transform.SetParent(owner.transform, false);
            var label = go.AddComponent<BuildingWorldLabel>();
            label.owner = owner;
            label.Build();
        }

        private void Build()
        {
            Bounds bounds = new Bounds(owner.transform.position, Vector3.zero);
            bool any = false;
            foreach (Renderer renderer in owner.GetComponentsInChildren<Renderer>())
            {
                if (renderer is MeshRenderer && renderer.gameObject.name.StartsWith("SelectionRing")) continue;
                if (!any) { bounds = renderer.bounds; any = true; }
                else bounds.Encapsulate(renderer.bounds);
            }
            transform.position = new Vector3(bounds.center.x, bounds.max.y + 1.6f, bounds.center.z);

            text = gameObject.AddComponent<TextMeshPro>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 11f;
            text.fontStyle = FontStyles.Bold;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.rectTransform.sizeDelta = new Vector2(14f, 4f);
            text.outlineWidth = 0.25f;
            text.outlineColor = new Color32(0, 0, 0, 255);
            text.raycastTarget = false;
            Refresh();
        }

        private void LateUpdate()
        {
            if (cam == null) cam = Camera.main;
            if (cam != null) transform.rotation = cam.transform.rotation;

            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + RefreshInterval;
            Refresh();
        }

        private void Refresh()
        {
            if (text == null || owner == null || !Game.IsInitialized) return;

            string id = owner.BuildingId;
            string line = $"{Loc.T("building.level")} {owner.Level}";
            ConstructionJobData job = Game.Upgrades.GetJob(id);

            if (job != null)
            {
                string time = NumberFormatter.Duration(System.TimeSpan.FromSeconds(Game.Upgrades.RemainingSeconds(job)));
                text.text = $"{line} > {job.targetLevel}\n<color=#FFD25A>{time}</color>";
                return;
            }

            long stored = Game.Buildings.GetStored(id);
            text.text = stored > 0 ? $"{line}\n<color=#6BE08F>+{NumberFormatter.Compact(stored)}</color>" : line;
        }
    }
}
