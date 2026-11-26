using Deadzone.Core;
using Deadzone.UI;
using TMPro;
using UnityEngine;

namespace Deadzone.Buildings
{
    /// <summary>
    /// Compact status marker above a building (PHASE 02.5): a small military level badge whose colour follows the visual tier,
    /// plus - only while upgrading - a thin progress bar with the remaining time, or a small "+N" when output can be collected.
    /// Replaces the large "Level N" text. Created at runtime by BuildingSelectable, so building prefabs need no extra wiring.
    /// </summary>
    public sealed class BuildingWorldLabel : MonoBehaviour
    {
        private const float RefreshInterval = 0.25f;
        private const float BadgeSize = 1.35f;
        private const float BarWidth = 2.2f;
        private const float BarHeight = 0.22f;
        private const float ReferenceZoom = 26f;

        // Tier 1..7: concrete, olive, steel blue, violet, gold, orange, red.
        private static readonly Color[] TierColors =
        {
            new Color32(0x8A, 0x90, 0x97, 255), new Color32(0x6F, 0xA0, 0x6A, 255), new Color32(0x4C, 0x9F, 0xE8, 255),
            new Color32(0xB0, 0x7C, 0xE8, 255), new Color32(0xE0, 0xB8, 0x4F, 255), new Color32(0xE8, 0x82, 0x2E, 255),
            new Color32(0xE2, 0x5A, 0x5A, 255),
        };

        private BuildingSelectable owner;
        private Transform visual;
        private SpriteRenderer badge;
        private TextMeshPro levelText;
        private SpriteRenderer barBack;
        private SpriteRenderer barFill;
        private TextMeshPro timeText;
        private TextMeshPro storedText;
        private Camera cam;
        private float nextRefresh;
        private int shownLevel = -1;

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
            transform.position = new Vector3(bounds.center.x, bounds.max.y + 1.2f, bounds.center.z);

            visual = new GameObject("Visual").transform;
            visual.SetParent(transform, false);

            badge = CreateSprite("Badge", UiIcons.Get("badge_level"), 10);
            badge.transform.localScale = Vector3.one * (BadgeSize / 1.28f);

            levelText = CreateText("Level", 5.2f, FontStyles.Bold, Color.white, 11);
            levelText.rectTransform.sizeDelta = new Vector2(2f, 1f);
            levelText.transform.localPosition = new Vector3(0f, 0.04f, 0f);

            barBack = CreateSprite("BarBack", UiIcons.WhiteBar, 10);
            barBack.color = new Color(0.04f, 0.06f, 0.08f, 0.85f);
            barBack.transform.localPosition = new Vector3(-BarWidth * 0.5f, -1.0f, 0f);
            barBack.transform.localScale = new Vector3(BarWidth, BarHeight, 1f);

            barFill = CreateSprite("BarFill", UiIcons.WhiteBar, 11);
            barFill.color = new Color32(0xFF, 0xD2, 0x5A, 255);
            barFill.transform.localPosition = new Vector3(-BarWidth * 0.5f, -1.0f, 0f);

            timeText = CreateText("Time", 3.6f, FontStyles.Bold, new Color32(0xFF, 0xD2, 0x5A, 255), 11);
            timeText.rectTransform.sizeDelta = new Vector2(4f, 0.8f);
            timeText.transform.localPosition = new Vector3(0f, -1.55f, 0f);

            storedText = CreateText("Stored", 3.8f, FontStyles.Bold, new Color32(0x6B, 0xE0, 0x8F, 255), 11);
            storedText.rectTransform.sizeDelta = new Vector2(4f, 0.8f);
            storedText.alignment = TextAlignmentOptions.Left;
            storedText.transform.localPosition = new Vector3(0.85f + 2f, 0f, 0f);

            Refresh();
        }

        private SpriteRenderer CreateSprite(string name, Sprite sprite, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(visual, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return renderer;
        }

        private TextMeshPro CreateText(string name, float size, FontStyles style, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(visual, false);
            var text = go.AddComponent<TextMeshPro>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Overflow;
            text.outlineWidth = 0.25f;
            text.outlineColor = new Color32(0, 0, 0, 255);
            text.raycastTarget = false;
            text.sortingOrder = order;
            return text;
        }

        private void LateUpdate()
        {
            if (cam == null) cam = Camera.main;
            if (cam != null)
            {
                transform.rotation = cam.transform.rotation;
                // Keep a roughly constant on-screen size while the player zooms.
                float scale = Mathf.Clamp(cam.orthographicSize / ReferenceZoom, 0.7f, 1.5f);
                float parentScale = transform.parent != null ? Mathf.Max(0.0001f, transform.parent.lossyScale.x) : 1f;
                transform.localScale = Vector3.one * scale / parentScale;
            }

            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + RefreshInterval;
            Refresh();
        }

        private void Refresh()
        {
            if (badge == null || owner == null || !Game.IsInitialized) return;

            int level = owner.Level;
            if (level != shownLevel)
            {
                shownLevel = level;
                int tier = BuildingVisualTiers.TierForLevel(level);
                badge.color = TierColors[Mathf.Clamp(tier - 1, 0, TierColors.Length - 1)];
            }

            string id = owner.BuildingId;
            ConstructionJobData job = Game.Upgrades.GetJob(id);
            bool building = job != null;

            barBack.gameObject.SetActive(building);
            barFill.gameObject.SetActive(building);
            timeText.gameObject.SetActive(building);

            if (building)
            {
                float progress = Mathf.Clamp01(Game.Upgrades.Progress01(job));
                barFill.transform.localScale = new Vector3(Mathf.Max(0.001f, BarWidth * progress), BarHeight * 0.7f, 1f);
                timeText.text = NumberFormatter.Duration(System.TimeSpan.FromSeconds(Game.Upgrades.RemainingSeconds(job)));
                levelText.text = $"{level}<size=65%>></size>{job.targetLevel}";
                storedText.gameObject.SetActive(false);
                return;
            }

            levelText.text = level.ToString();
            long stored = Game.Buildings.GetStored(id);
            storedText.gameObject.SetActive(stored > 0);
            if (stored > 0) storedText.text = "+" + NumberFormatter.Compact(stored);
        }
    }
}
