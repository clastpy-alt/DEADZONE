using Deadzone.Core;
using Deadzone.Data;
using Deadzone.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.EditorTools
{
    /// <summary>Reusable UI component prefabs (PHASE 01.5). Every popup prefab is inactive by default and carries a UIPanel.</summary>
    internal static partial class UIPrefabGenerator
    {
        public const string PopupPanelPath = EditorPaths.PrefabsUI + "/PopupPanel.prefab";
        public const string ConfirmationDialogPath = EditorPaths.PrefabsUI + "/ConfirmationDialog.prefab";
        public const string RewardPopupPath = EditorPaths.PrefabsUI + "/RewardPopup.prefab";
        public const string TooltipPath = EditorPaths.PrefabsUI + "/Tooltip.prefab";
        public const string LoadingScreenPath = EditorPaths.PrefabsUI + "/LoadingScreen.prefab";
        public const string ProgressBarPath = EditorPaths.PrefabsUI + "/ProgressBar.prefab";
        public const string NotificationBadgePath = EditorPaths.PrefabsUI + "/NotificationBadge.prefab";
        public const string TabNavigationPath = EditorPaths.PrefabsUI + "/TabNavigation.prefab";
        public const string ScrollablePanelPath = EditorPaths.PrefabsUI + "/ScrollablePanel.prefab";
        public const string SafeAreaControllerPath = EditorPaths.PrefabsUI + "/SafeAreaController.prefab";
        public const string HeroCardPath = EditorPaths.PrefabsUI + "/HeroCard.prefab";
        public const string ItemCardPath = EditorPaths.PrefabsUI + "/ItemCard.prefab";
        public const string EventCardPath = EditorPaths.PrefabsUI + "/EventCard.prefab";
        public const string ShopCardPath = EditorPaths.PrefabsUI + "/ShopCard.prefab";
        public const string UIOverlayPath = EditorPaths.PrefabsUI + "/UIOverlay.prefab";

        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        /// <summary>Order matters: UIOverlay nests the popup prefabs, so they are generated first.</summary>
        private static void GenerateComponents(bool overwrite)
        {
            SavePrefab(PopupPanelPath, overwrite, BuildPopupPanel);
            SavePrefab(ConfirmationDialogPath, overwrite, BuildConfirmationDialog);
            SavePrefab(RewardPopupPath, overwrite, BuildRewardPopup);
            SavePrefab(TooltipPath, overwrite, BuildTooltip);
            SavePrefab(ProgressBarPath, overwrite, BuildProgressBar);
            SavePrefab(LoadingScreenPath, overwrite, BuildLoadingScreen);
            SavePrefab(NotificationBadgePath, overwrite, BuildNotificationBadge);
            SavePrefab(TabNavigationPath, overwrite, BuildTabNavigation);
            SavePrefab(ScrollablePanelPath, overwrite, BuildScrollablePanel);
            SavePrefab(SafeAreaControllerPath, overwrite, BuildSafeAreaController);
            SavePrefab(HeroCardPath, overwrite, BuildHeroCard);
            SavePrefab(ItemCardPath, overwrite, BuildItemCard);
            SavePrefab(EventCardPath, overwrite, BuildEventCard);
            SavePrefab(ShopCardPath, overwrite, BuildShopCard);
            SavePrefab(UIOverlayPath, overwrite, BuildUIOverlay);
        }

        // ------------------------------------------------------------------ popups

        /// <summary>Full-screen dim layer + animated window. Returns the root (inactive, UIPanel attached).</summary>
        private static GameObject BuildPopupRoot(string name, Vector2 windowSize, out UIPanel panel, out RectTransform window, out Button dim)
        {
            Image root = UIFactory.Panel(name, null, new Color(0f, 0f, 0f, 0.68f));
            UIFactory.Stretch(root.rectTransform);

            dim = root.gameObject.AddComponent<Button>();
            dim.targetGraphic = root;
            dim.transition = Selectable.Transition.None;

            var group = root.gameObject.AddComponent<CanvasGroup>();
            panel = root.gameObject.AddComponent<UIPanel>();

            Image windowImage = UIFactory.Window("Window", root.transform, windowSize);
            window = windowImage.rectTransform;
            panel.Bind(group, window);

            root.gameObject.SetActive(false);
            return root.gameObject;
        }

        private static TextMeshProUGUI AddTitle(Transform window, string text, float height = 90f)
        {
            TextMeshProUGUI title = UIFactory.Text("Title", window, text, 44f, UIColors.Gold, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -24f), new Vector2(-48f, height));
            UIFactory.AutoSize(title, 24f, 44f);
            return title;
        }

        private static Button AddBottomButton(Transform window, string name, string key, Color fill, Color border,
            float xAnchor, float widthFraction)
        {
            Button button = UIFactory.CreateButton(name, window, fill, border, key, 34f, out _);
            var rect = (RectTransform)button.transform;
            rect.anchorMin = new Vector2(xAnchor - widthFraction * 0.5f, 0f);
            rect.anchorMax = new Vector2(xAnchor + widthFraction * 0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 32f);
            rect.sizeDelta = new Vector2(-24f, 96f);
            return button;
        }

        private static GameObject BuildPopupPanel()
        {
            GameObject root = BuildPopupRoot("PopupPanel", new Vector2(900f, 980f), out UIPanel panel, out RectTransform window, out Button dim);

            TextMeshProUGUI title = AddTitle(window, "Title");

            TextMeshProUGUI body = UIFactory.Text("Body", window, "Body", 36f, UIColors.TextPrimary, TextAlignmentOptions.TopLeft);
            UIFactory.Stretch(body.rectTransform);
            body.rectTransform.offsetMin = new Vector2(48f, 160f);
            body.rectTransform.offsetMax = new Vector2(-48f, -130f);
            body.overflowMode = TextOverflowModes.Overflow;
            body.lineSpacing = 12f;

            Button close = AddBottomButton(window, "CloseButton", "popup.close", UIColors.Panel, UIColors.Silver, 0.5f, 0.6f);

            var view = root.AddComponent<PopupPanelView>();
            view.Bind(panel, title, body, close, dim);
            return root;
        }

        private static GameObject BuildConfirmationDialog()
        {
            GameObject root = BuildPopupRoot("ConfirmationDialog", new Vector2(860f, 560f), out UIPanel panel, out RectTransform window, out Button dim);

            TextMeshProUGUI title = AddTitle(window, "Title");

            TextMeshProUGUI message = UIFactory.Text("Message", window, "Message", 34f, UIColors.TextPrimary, TextAlignmentOptions.Center);
            UIFactory.Stretch(message.rectTransform);
            message.rectTransform.offsetMin = new Vector2(48f, 170f);
            message.rectTransform.offsetMax = new Vector2(-48f, -130f);
            message.overflowMode = TextOverflowModes.Overflow;

            Button confirm = AddBottomButton(window, "ConfirmButton", "confirm.yes", UIColors.MilitaryGreen, UIColors.Gold, 0.27f, 0.5f);
            Button cancel = AddBottomButton(window, "CancelButton", "confirm.no", UIColors.Panel, UIColors.Silver, 0.73f, 0.5f);

            var view = root.AddComponent<ConfirmationDialogView>();
            view.Bind(panel, title, message, confirm, cancel);
            return root;
        }

        private static GameObject BuildRewardPopup()
        {
            GameObject root = BuildPopupRoot("RewardPopup", new Vector2(820f, 720f), out UIPanel panel, out RectTransform window, out _);

            TextMeshProUGUI title = AddTitle(window, "Title");

            TextMeshProUGUI lines = UIFactory.Text("Lines", window, "+0", 42f, UIColors.TextPrimary, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Stretch(lines.rectTransform);
            lines.rectTransform.offsetMin = new Vector2(48f, 170f);
            lines.rectTransform.offsetMax = new Vector2(-48f, -130f);
            lines.overflowMode = TextOverflowModes.Overflow;
            lines.lineSpacing = 18f;

            Button claim = AddBottomButton(window, "ClaimButton", "reward.claim", UIColors.MilitaryGreen, UIColors.Gold, 0.5f, 0.7f);

            var view = root.AddComponent<RewardPopupView>();
            view.Bind(panel, title, lines, claim);
            return root;
        }

        private static GameObject BuildLoadingScreen()
        {
            Image root = UIFactory.Panel("LoadingScreen", null, UIColors.Background);
            UIFactory.Stretch(root.rectTransform);

            var group = root.gameObject.AddComponent<CanvasGroup>();
            var panel = root.gameObject.AddComponent<UIPanel>();

            RectTransform content = UIFactory.Rect("Content", root.transform);
            UIFactory.Stretch(content);
            panel.Bind(group, content);

            TextMeshProUGUI title = UIFactory.Text("Title", content, Loc.T("app.title"), 64f, UIColors.Gold, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Place(title.rectTransform, Center, Center, Center, new Vector2(0f, 120f), new Vector2(960f, 160f));
            UIFactory.AutoSize(title, 30f, 64f);
            title.gameObject.AddComponent<LocalizedText>().SetKey("app.title");

            GameObject bar = Instantiate(ProgressBarPath, content);
            var barRect = (RectTransform)bar.transform;
            UIFactory.Place(barRect, Center, Center, Center, new Vector2(0f, -40f), new Vector2(760f, 40f));

            TextMeshProUGUI tip = UIFactory.Text("Tip", content, Loc.T("loading.text"), 32f, UIColors.TextSecondary, TextAlignmentOptions.Center);
            UIFactory.Place(tip.rectTransform, Center, Center, Center, new Vector2(0f, -120f), new Vector2(900f, 80f));
            tip.gameObject.AddComponent<LocalizedText>().SetKey("loading.text");

            var view = root.gameObject.AddComponent<LoadingScreenView>();
            view.Bind(panel, bar.GetComponent<ProgressBarView>(), tip);

            root.gameObject.SetActive(false);
            return root.gameObject;
        }

        private static GameObject BuildTooltip()
        {
            Image root = UIFactory.Panel("Tooltip", null, new Color(0.06f, 0.08f, 0.1f, 0.95f), false);
            var rect = root.rectTransform;
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(440f, 80f);
            UIFactory.AddOutline(root, UIColors.Gold, 2f);

            var layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 12, 12);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = root.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            TextMeshProUGUI label = UIFactory.Text("Label", root.transform, "Tooltip", 28f, UIColors.TextPrimary, TextAlignmentOptions.Center);
            label.overflowMode = TextOverflowModes.Overflow;

            var view = root.gameObject.AddComponent<TooltipView>();
            view.Bind(rect, label);

            root.gameObject.SetActive(false);
            return root.gameObject;
        }

        // ------------------------------------------------------------------ small components

        private static GameObject BuildProgressBar()
        {
            Image root = UIFactory.Panel("ProgressBar", null, new Color(0f, 0f, 0f, 0.5f), false);
            root.rectTransform.sizeDelta = new Vector2(600f, 36f);
            UIFactory.AddOutline(root, UIColors.WithAlpha(UIColors.Silver, 0.6f), 2f);

            Image fill = UIFactory.Panel("Fill", root.transform, UIColors.MilitaryGreen, false);
            var fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = new Vector2(3f, 3f);
            fillRect.offsetMax = new Vector2(0f, -3f);

            TextMeshProUGUI label = UIFactory.Text("Label", root.transform, "0%", 22f, UIColors.TextPrimary, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Stretch(label.rectTransform);

            var view = root.gameObject.AddComponent<ProgressBarView>();
            view.Bind(fillRect, label);
            return root.gameObject;
        }

        private static GameObject BuildNotificationBadge()
        {
            Image root = UIFactory.Panel("NotificationBadge", null, UIColors.Danger, false);
            root.rectTransform.sizeDelta = new Vector2(44f, 44f);
            UIFactory.AddOutline(root, UIColors.TextPrimary, 2f);

            TextMeshProUGUI count = UIFactory.Text("Count", root.transform, "0", 24f, UIColors.TextPrimary, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Stretch(count.rectTransform);
            UIFactory.AutoSize(count, 12f, 24f);

            var view = root.gameObject.AddComponent<NotificationBadgeView>();
            view.Bind(count);
            return root.gameObject;
        }

        private static GameObject BuildTabNavigation()
        {
            Image root = UIFactory.Panel("TabNavigation", null, new Color(0f, 0f, 0f, 0.35f), false);
            root.rectTransform.sizeDelta = new Vector2(900f, 100f);

            var layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.spacing = 6f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            const int tabCount = 3;
            var buttons = new Button[tabCount];
            var backgrounds = new Image[tabCount];
            var labels = new TMP_Text[tabCount];
            for (int i = 0; i < tabCount; i++)
            {
                buttons[i] = UIFactory.CreateButton("Tab" + (i + 1), root.transform, UIColors.Panel, UIColors.WithAlpha(UIColors.Silver, 0.6f),
                    "nav.base", 30f, out TextMeshProUGUI label);
                // Tab labels are set by the screen via SetLabels, so the fixed localization key is removed.
                UnityEngine.Object.DestroyImmediate(label.GetComponent<LocalizedText>());
                label.text = "Tab " + (i + 1) + " [PLACEHOLDER]";
                backgrounds[i] = buttons[i].GetComponent<Image>();
                labels[i] = label;
            }

            var view = root.gameObject.AddComponent<TabNavigationView>();
            view.Bind(buttons, backgrounds, labels);
            return root.gameObject;
        }

        private static GameObject BuildScrollablePanel()
        {
            Image root = UIFactory.Panel("ScrollablePanel", null, UIColors.WithAlpha(UIColors.Panel, 0.9f), false);
            root.rectTransform.sizeDelta = new Vector2(900f, 1000f);
            UIFactory.AddOutline(root, UIColors.WithAlpha(UIColors.Silver, 0.5f), 2f);

            Image viewport = UIFactory.Panel("Viewport", root.transform, new Color(0f, 0f, 0f, 0.01f), true);
            UIFactory.Stretch(viewport.rectTransform);
            viewport.rectTransform.offsetMin = new Vector2(8f, 8f);
            viewport.rectTransform.offsetMax = new Vector2(-8f, -8f);
            viewport.gameObject.AddComponent<RectMask2D>();

            RectTransform content = UIFactory.Rect("Content", viewport.transform);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;

            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = root.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport.rectTransform;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 30f;
            return root.gameObject;
        }

        private static GameObject BuildSafeAreaController()
        {
            RectTransform rect = UIFactory.Rect("SafeAreaController", null);
            UIFactory.Stretch(rect);
            rect.gameObject.AddComponent<SafeAreaFitter>();
            return rect.gameObject;
        }

        // ------------------------------------------------------------------ cards

        private static GameObject BuildHeroCard()
        {
            Image frame = UIFactory.Panel("HeroCard", null, UIColors.Silver);
            frame.rectTransform.sizeDelta = new Vector2(260f, 360f);

            Image portrait = UIFactory.Panel("Portrait", frame.transform, UIColors.Panel, false);
            UIFactory.Stretch(portrait.rectTransform);
            portrait.rectTransform.offsetMin = new Vector2(6f, 6f);
            portrait.rectTransform.offsetMax = new Vector2(-6f, -6f);

            TextMeshProUGUI initials = UIFactory.Text("Initials [PLACEHOLDER]", portrait.transform, "??", 96f, UIColors.WithAlpha(UIColors.TextSecondary, 0.7f),
                TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Stretch(initials.rectTransform);
            initials.rectTransform.offsetMin = new Vector2(0f, 90f);
            initials.rectTransform.offsetMax = new Vector2(0f, -40f);

            TextMeshProUGUI rarity = UIFactory.Text("Rarity", portrait.transform, "R", 34f, UIColors.Gold, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            UIFactory.Place(rarity.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12f, -8f), new Vector2(110f, 44f));

            TextMeshProUGUI level = UIFactory.Text("Level", portrait.transform, "Lv.1", 26f, UIColors.TextPrimary, TextAlignmentOptions.TopRight, FontStyles.Bold);
            UIFactory.Place(level.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-12f, -10f), new Vector2(130f, 40f));

            TextMeshProUGUI heroName = UIFactory.Text("Name", portrait.transform, "Hero", 30f, UIColors.TextPrimary, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Place(heroName.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 62f), new Vector2(-16f, 40f));
            UIFactory.AutoSize(heroName, 16f, 30f);

            TextMeshProUGUI heroClass = UIFactory.Text("Class", portrait.transform, "Class", 22f, UIColors.TextSecondary, TextAlignmentOptions.Center);
            UIFactory.Place(heroClass.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 32f), new Vector2(-16f, 30f));

            RectTransform starRow = UIFactory.Rect("Stars", portrait.transform);
            UIFactory.Place(starRow, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(-16f, 24f));
            var row = starRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 4f;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = false;
            row.childControlHeight = false;

            var stars = new Image[HeroCardView.MaxStars];
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i] = UIFactory.Panel("Star" + (i + 1), starRow, UIColors.Gold, false);
                stars[i].rectTransform.sizeDelta = new Vector2(24f, 24f);
            }

            var view = frame.gameObject.AddComponent<HeroCardView>();
            view.Bind(frame, portrait, initials, rarity, heroName, heroClass, level, stars);
            return frame.gameObject;
        }

        private static GameObject BuildItemCard()
        {
            Image frame = UIFactory.Panel("ItemCard", null, UIColors.Silver);
            frame.rectTransform.sizeDelta = new Vector2(170f, 210f);

            Image inner = UIFactory.Panel("Inner", frame.transform, UIColors.Panel, false);
            UIFactory.Stretch(inner.rectTransform);
            inner.rectTransform.offsetMin = new Vector2(5f, 5f);
            inner.rectTransform.offsetMax = new Vector2(-5f, -5f);

            Image icon = UIFactory.Panel("Icon [PLACEHOLDER]", inner.transform, UIColors.MilitaryGreen, false);
            UIFactory.Place(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(110f, 110f));

            TextMeshProUGUI code = UIFactory.Text("Code", icon.transform, "--", 40f, UIColors.TextPrimary, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Stretch(code.rectTransform);

            TextMeshProUGUI quantity = UIFactory.Text("Quantity", inner.transform, "x1", 26f, UIColors.Gold, TextAlignmentOptions.Right, FontStyles.Bold);
            UIFactory.Place(quantity.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(-14f, 32f));

            TextMeshProUGUI itemName = UIFactory.Text("Name", inner.transform, "Item", 24f, UIColors.TextPrimary, TextAlignmentOptions.Center);
            UIFactory.Place(itemName.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(-14f, 32f));
            UIFactory.AutoSize(itemName, 14f, 24f);

            var view = frame.gameObject.AddComponent<ItemCardView>();
            view.Bind(frame, icon, code, itemName, quantity);
            return frame.gameObject;
        }

        private static GameObject BuildEventCard()
        {
            Image root = UIFactory.Panel("EventCard", null, UIColors.Panel);
            root.rectTransform.sizeDelta = new Vector2(900f, 180f);
            UIFactory.AddOutline(root, UIColors.WithAlpha(UIColors.Gold, 0.6f), 2f);

            Image banner = UIFactory.Panel("Banner [PLACEHOLDER]", root.transform, UIColors.MilitaryGreen, false);
            UIFactory.Place(banner.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(240f, -16f));

            TextMeshProUGUI title = UIFactory.Text("Title", root.transform, "Event", 38f, UIColors.TextPrimary, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            UIFactory.Stretch(title.rectTransform);
            title.rectTransform.offsetMin = new Vector2(268f, 70f);
            title.rectTransform.offsetMax = new Vector2(-20f, -16f);
            UIFactory.AutoSize(title, 20f, 38f);

            TextMeshProUGUI status = UIFactory.Text("Status", root.transform, "Active", 28f, UIColors.Success, TextAlignmentOptions.BottomLeft, FontStyles.Bold);
            UIFactory.Stretch(status.rectTransform);
            status.rectTransform.offsetMin = new Vector2(268f, 14f);
            status.rectTransform.offsetMax = new Vector2(-300f, -110f);

            TextMeshProUGUI time = UIFactory.Text("Time", root.transform, "00:00:00", 28f, UIColors.TextSecondary, TextAlignmentOptions.BottomRight);
            UIFactory.Stretch(time.rectTransform);
            time.rectTransform.offsetMin = new Vector2(500f, 14f);
            time.rectTransform.offsetMax = new Vector2(-20f, -110f);

            var view = root.gameObject.AddComponent<EventCardView>();
            view.Bind(banner, title, status, time);
            return root.gameObject;
        }

        private static GameObject BuildShopCard()
        {
            Image root = UIFactory.Panel("ShopCard", null, UIColors.Panel);
            root.rectTransform.sizeDelta = new Vector2(300f, 420f);
            UIFactory.AddOutline(root, UIColors.WithAlpha(UIColors.Gold, 0.6f), 2f);
            var group = root.gameObject.AddComponent<CanvasGroup>();

            Image icon = UIFactory.Panel("Icon [PLACEHOLDER]", root.transform, UIColors.MilitaryGreen, false);
            UIFactory.Place(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(180f, 180f));

            TextMeshProUGUI code = UIFactory.Text("Code", icon.transform, "--", 56f, UIColors.TextPrimary, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Stretch(code.rectTransform);

            TextMeshProUGUI productName = UIFactory.Text("Name", root.transform, "Product", 30f, UIColors.TextPrimary, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Place(productName.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -214f), new Vector2(-24f, 50f));
            UIFactory.AutoSize(productName, 16f, 30f);

            TextMeshProUGUI price = UIFactory.Text("Price", root.transform, "0", 32f, UIColors.Gold, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Place(price.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -268f), new Vector2(-24f, 44f));

            Button buy = UIFactory.CreateButton("BuyButton", root.transform, UIColors.MilitaryGreen, UIColors.Gold, "shop.buy", 32f, out _);
            UIFactory.Place((RectTransform)buy.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(250f, 80f));

            var view = root.gameObject.AddComponent<ShopCardView>();
            view.Bind(icon, code, productName, price, buy, group);
            return root.gameObject;
        }

        // ------------------------------------------------------------------ overlay composite

        /// <summary>Canvas (sorting order 100) holding nested popup prefab instances, all bound to UIOverlay.</summary>
        private static GameObject BuildUIOverlay()
        {
            var go = new GameObject("UIOverlay", typeof(RectTransform));

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();

            GameObject popup = Instantiate(PopupPanelPath, go.transform);
            GameObject confirmation = Instantiate(ConfirmationDialogPath, go.transform);
            GameObject reward = Instantiate(RewardPopupPath, go.transform);
            GameObject loading = Instantiate(LoadingScreenPath, go.transform);
            GameObject tooltip = Instantiate(TooltipPath, go.transform);

            var overlay = go.AddComponent<UIOverlay>();
            overlay.Bind(popup.GetComponent<PopupPanelView>(), confirmation.GetComponent<ConfirmationDialogView>(),
                reward.GetComponent<RewardPopupView>(), tooltip.GetComponent<TooltipView>(), loading.GetComponent<LoadingScreenView>());
            return go;
        }
    }
}
