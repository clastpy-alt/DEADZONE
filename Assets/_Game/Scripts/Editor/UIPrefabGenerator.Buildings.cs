using Deadzone.Core;
using Deadzone.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.EditorTools
{
    /// <summary>PHASE 02 building UI: the construction queue window (prefab) and the building info sheet (built into the HUD).</summary>
    internal static partial class UIPrefabGenerator
    {
        public const string ConstructionQueuePath = EditorPaths.PrefabsUI + "/ConstructionQueuePanel.prefab";
        public const int QueueRows = 3;

        public static void GenerateBuildingUi(bool overwrite)
        {
            EditorPaths.EnsureFolder(EditorPaths.PrefabsUI);
            SavePrefab(ConstructionQueuePath, overwrite, BuildConstructionQueue);
        }

        // ------------------------------------------------------------------ layout helpers

        /// <summary>Full-width strip: left/right margins, distance from the top and height.</summary>
        private static void Strip(RectTransform rect, float left, float right, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(left, -(top + height));
            rect.offsetMax = new Vector2(-right, -top);
        }

        /// <summary>Strip limited to a horizontal range of the parent (0..1).</summary>
        private static void Column(RectTransform rect, float xMin, float xMax, float left, float right, float top, float height)
        {
            rect.anchorMin = new Vector2(xMin, 1f);
            rect.anchorMax = new Vector2(xMax, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(left, -(top + height));
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static TextMeshProUGUI Body(Transform parent, string name, float size, float minSize, TextAlignmentOptions alignment,
            Color color, FontStyles style = FontStyles.Normal)
        {
            TextMeshProUGUI text = UIFactory.Text(name, parent, string.Empty, size, color, alignment, style);
            UIFactory.AutoSize(text, minSize, size);
            text.richText = true;
            text.textWrappingMode = TextWrappingModes.Normal;
            return text;
        }

        private static void StaticLabel(Button button)
        {
            // Dynamic labels are written by the panel; a LocalizedText would overwrite them.
            var localized = button.GetComponentInChildren<LocalizedText>();
            if (localized != null) Object.DestroyImmediate(localized);
        }

        // ------------------------------------------------------------------ building info sheet

        /// <summary>Builds the bottom sheet under 'safe' and returns its root. The caller decides sibling order.</summary>
        public static RectTransform BuildInfoPanelUi(Transform safe)
        {
            RectTransform root = UIFactory.Rect("BuildingInfoPanel", safe);
            UIFactory.Place(root, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 216f), new Vector2(-48f, 900f));

            Image content = UIFactory.Panel("Content", root, UIColors.WithAlpha(UIColors.Panel, 0.97f));
            UIFactory.Stretch(content.rectTransform);
            UIFactory.AddOutline(content, UIColors.Gold, 3f);
            UIFactory.Bevel(content.transform);

            var w = new BuildingInfoPanel.Widgets { content = content.gameObject };

            // Header: image placeholder, name, level, close
            Image icon = UIFactory.Panel("BuildingImage_PH", content.transform, UIColors.MilitaryGreen, false);
            UIFactory.Place(icon.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(220f, 220f));
            UIFactory.AddOutline(icon, UIColors.Silver, 3f);
            TextMeshProUGUI iconLabel = UIFactory.Text("IconLabel", icon.transform, "BLD", 40f, UIColors.TextPrimary, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Stretch(iconLabel.rectTransform);
            w.icon = icon;
            w.iconLabel = iconLabel;

            // Live 3D preview (BuildingPreview renders the selected building into this RawImage).
            var previewObject = new GameObject("Preview", typeof(RectTransform), typeof(RawImage), typeof(BuildingPreview));
            previewObject.transform.SetParent(icon.transform, false);
            UIFactory.Stretch((RectTransform)previewObject.transform);
            ((RectTransform)previewObject.transform).offsetMin = new Vector2(6f, 6f);
            ((RectTransform)previewObject.transform).offsetMax = new Vector2(-6f, -6f);
            var raw = previewObject.GetComponent<RawImage>();
            raw.raycastTarget = false;
            raw.enabled = false;
            previewObject.GetComponent<BuildingPreview>().Bind(raw);
            w.preview = raw;

            w.nameText = Body(content.transform, "BuildingName", 44f, 26f, TextAlignmentOptions.Left, UIColors.TextPrimary, FontStyles.Bold);
            Strip(w.nameText.rectTransform, 264f, 210f, 20f, 60f);
            w.levelText = Body(content.transform, "BuildingLevel", 32f, 20f, TextAlignmentOptions.Left, UIColors.TextSecondary);
            Strip(w.levelText.rectTransform, 264f, 210f, 84f, 46f);

            Button close = UIFactory.CreateButton("CloseButton", content.transform, UIColors.Panel, UIColors.Silver, "popup.close", 30f, out _);
            UIFactory.Place((RectTransform)close.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(180f, 84f));
            w.closeButton = close;

            // Description + benefits
            w.descriptionText = Body(content.transform, "Description", 28f, 20f, TextAlignmentOptions.TopLeft, UIColors.TextSecondary);
            Strip(w.descriptionText.rectTransform, 264f, 28f, 140f, 104f);

            w.currentText = Body(content.transform, "CurrentBenefits", 28f, 20f, TextAlignmentOptions.TopLeft, UIColors.TextPrimary);
            Column(w.currentText.rectTransform, 0f, 0.5f, 28f, 12f, 258f, 160f);
            w.nextText = Body(content.transform, "NextBenefits", 28f, 20f, TextAlignmentOptions.TopLeft, UIColors.TextPrimary);
            Column(w.nextText.rectTransform, 0.5f, 1f, 12f, 28f, 258f, 160f);

            // Cost, duration, requirements
            w.costText = Body(content.transform, "Cost", 30f, 20f, TextAlignmentOptions.TopLeft, UIColors.TextPrimary);
            Strip(w.costText.rectTransform, 28f, 28f, 426f, 112f);
            w.requirementsText = Body(content.transform, "Requirements", 28f, 20f, TextAlignmentOptions.TopLeft, UIColors.TextPrimary);
            Strip(w.requirementsText.rectTransform, 28f, 28f, 544f, 128f);

            // Reason (disabled upgrade) / running construction
            w.reasonText = Body(content.transform, "Reason", 30f, 20f, TextAlignmentOptions.Center, UIColors.Danger, FontStyles.Bold);
            Strip(w.reasonText.rectTransform, 28f, 28f, 684f, 56f);

            RectTransform progress = UIFactory.Rect("Progress", content.transform);
            Strip(progress, 28f, 28f, 684f, 56f);
            GameObject bar = Instantiate(ProgressBarPath, progress);
            var barRect = (RectTransform)bar.transform;
            barRect.anchorMin = new Vector2(0f, 0.5f);
            barRect.anchorMax = new Vector2(0.6f, 0.5f);
            barRect.pivot = new Vector2(0f, 0.5f);
            barRect.offsetMin = new Vector2(0f, -20f);
            barRect.offsetMax = new Vector2(-8f, 20f);
            w.progressBar = bar.GetComponent<ProgressBarView>();
            w.remainingText = Body(progress, "Remaining", 30f, 20f, TextAlignmentOptions.Right, UIColors.TextPrimary, FontStyles.Bold);
            w.remainingText.rectTransform.anchorMin = new Vector2(0.6f, 0f);
            w.remainingText.rectTransform.anchorMax = Vector2.one;
            w.remainingText.rectTransform.offsetMin = Vector2.zero;
            w.remainingText.rectTransform.offsetMax = Vector2.zero;
            w.progressGroup = progress.gameObject;

            // Buttons
            RectTransform buttons = UIFactory.Rect("Buttons", content.transform);
            buttons.anchorMin = new Vector2(0f, 0f);
            buttons.anchorMax = new Vector2(1f, 0f);
            buttons.pivot = new Vector2(0.5f, 0f);
            buttons.offsetMin = new Vector2(22f, 24f);
            buttons.offsetMax = new Vector2(-22f, 134f);

            w.collectButton = BuildSheetButton(buttons, "CollectButton", "bi.collect", UIColors.Panel, UIColors.Gold, 0f, 0.34f, out w.collectLabel);
            w.infoButton = BuildSheetButton(buttons, "InfoButton", "bi.info", UIColors.Panel, UIColors.Silver, 0.34f, 0.56f, out _);
            w.upgradeButton = BuildSheetButton(buttons, "UpgradeButton", "building.upgrade", UIColors.MilitaryGreen, UIColors.Gold, 0.56f, 1f, out w.upgradeLabel);
            StaticLabel(w.collectButton);
            StaticLabel(w.upgradeButton);

            var panel = root.gameObject.AddComponent<BuildingInfoPanel>();
            panel.Bind(w);
            content.gameObject.SetActive(false);
            return root;
        }

        private static Button BuildSheetButton(Transform parent, string name, string key, Color fill, Color border, float x0, float x1, out TMP_Text label)
        {
            Button button = UIFactory.CreateButton(name, parent, fill, border, key, 34f, out TextMeshProUGUI text);
            var rect = (RectTransform)button.transform;
            rect.anchorMin = new Vector2(x0, 0f);
            rect.anchorMax = new Vector2(x1, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(6f, 0f);
            rect.offsetMax = new Vector2(-6f, 0f);
            label = text;
            return button;
        }

        // ------------------------------------------------------------------ construction queue window

        private static GameObject BuildConstructionQueue()
        {
            GameObject root = BuildPopupRoot("ConstructionQueuePanel", new Vector2(960f, 1060f), out UIPanel panel, out RectTransform window, out Button dim);

            TextMeshProUGUI title = AddTitle(window, Loc.T("side.construction"));
            title.gameObject.AddComponent<LocalizedText>().SetKey("side.construction");

            TextMeshProUGUI slots = Body(window, "Slots", 30f, 20f, TextAlignmentOptions.Center, UIColors.TextSecondary);
            Strip(slots.rectTransform, 24f, 24f, 112f, 48f);

            TextMeshProUGUI empty = Body(window, "Empty", 36f, 22f, TextAlignmentOptions.Center, UIColors.TextSecondary);
            empty.text = Loc.T("cq.empty");
            empty.gameObject.AddComponent<LocalizedText>().SetKey("cq.empty");
            Strip(empty.rectTransform, 24f, 24f, 300f, 120f);

            var rows = new ConstructionQueuePanel.Row[QueueRows];
            for (int i = 0; i < QueueRows; i++)
            {
                rows[i] = BuildQueueRow(window, i);
            }

            Button close = AddBottomButton(window, "CloseButton", "popup.close", UIColors.Panel, UIColors.Silver, 0.5f, 0.6f);

            var view = root.AddComponent<ConstructionQueuePanel>();
            view.Bind(panel, slots, empty, close, dim, rows);
            return root;
        }

        private static ConstructionQueuePanel.Row BuildQueueRow(Transform window, int index)
        {
            var row = new ConstructionQueuePanel.Row();

            Image bg = UIFactory.Panel("Row" + index, window, UIColors.WithAlpha(UIColors.Background, 0.9f), false);
            UIFactory.AddOutline(bg, UIColors.WithAlpha(UIColors.Silver, 0.5f), 2f);
            Strip(bg.rectTransform, 24f, 24f, 176f + index * 236f, 220f);
            row.root = bg.gameObject;

            Image icon = UIFactory.Panel("Icon_PH", bg.transform, UIColors.MilitaryGreen, false);
            UIFactory.Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(120f, 120f));
            UIFactory.AddOutline(icon, UIColors.Silver, 2f);
            TextMeshProUGUI iconLabel = UIFactory.Text("IconLabel", icon.transform, "BLD", 32f, UIColors.TextPrimary, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Stretch(iconLabel.rectTransform);
            row.icon = icon;
            row.iconLabel = iconLabel;

            row.nameText = Body(bg.transform, "Name", 36f, 22f, TextAlignmentOptions.Left, UIColors.TextPrimary, FontStyles.Bold);
            Strip(row.nameText.rectTransform, 152f, 270f, 12f, 46f);
            row.levelText = Body(bg.transform, "Level", 30f, 20f, TextAlignmentOptions.Left, UIColors.Gold);
            Strip(row.levelText.rectTransform, 152f, 270f, 58f, 38f);

            GameObject bar = Instantiate(ProgressBarPath, bg.transform);
            var barRect = (RectTransform)bar.transform;
            barRect.anchorMin = new Vector2(0f, 1f);
            barRect.anchorMax = new Vector2(1f, 1f);
            barRect.pivot = new Vector2(0.5f, 1f);
            barRect.offsetMin = new Vector2(152f, -146f);
            barRect.offsetMax = new Vector2(-270f, -106f);
            row.bar = bar.GetComponent<ProgressBarView>();

            row.remainingText = Body(bg.transform, "Remaining", 30f, 20f, TextAlignmentOptions.Left, UIColors.TextSecondary);
            Strip(row.remainingText.rectTransform, 152f, 270f, 156f, 44f);

            row.speedupButton = QueueButton(bg.transform, "SpeedupButton", "cq.speedup", UIColors.MilitaryGreen, UIColors.Gold, -26f);
            row.cancelButton = QueueButton(bg.transform, "CancelButton", "cq.cancel", UIColors.Panel, UIColors.Danger, -122f);
            return row;
        }

        private static Button QueueButton(Transform parent, string name, string key, Color fill, Color border, float y)
        {
            Button button = UIFactory.CreateButton(name, parent, fill, border, key, 30f, out _);
            UIFactory.Place((RectTransform)button.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-16f, y), new Vector2(240f, 80f));
            return button;
        }
    }
}
