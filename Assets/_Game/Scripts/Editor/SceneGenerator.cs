using System.Collections.Generic;
using System.IO;
using Deadzone.Buildings;
using Deadzone.Core;
using Deadzone.Data;
using Deadzone.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Deadzone.EditorTools
{
    /// <summary>Builds the five Phase 01 scenes. Existing scenes are never overwritten unless asked.</summary>
    internal static class SceneGenerator
    {
        private static readonly string[] AllScenes =
        {
            SceneNames.Bootstrap, SceneNames.MainMenu, SceneNames.Base, SceneNames.WorldMap, SceneNames.Battle
        };

        public static void CreateAll(bool overwrite)
        {
            CreateScene(SceneNames.Bootstrap, overwrite, BuildBootstrap);
            CreateScene(SceneNames.MainMenu, overwrite, BuildMainMenu);
            CreateScene(SceneNames.Base, overwrite, BuildBase);
            CreateScene(SceneNames.WorldMap, overwrite, () => BuildPlaceholder("WORLD MAP - PHASE 05"));
            CreateScene(SceneNames.Battle, overwrite, () => BuildPlaceholder("BATTLE - PHASE 04"));
            CreateScene(SceneNames.UIGallery, overwrite, BuildGallery);
        }

        public static void CreateBaseScene(bool overwrite)
        {
            CreateScene(SceneNames.Base, overwrite, BuildBase);
        }

        public static void RegisterBuildScenes()
        {
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (string name in AllScenes)
            {
                string path = EditorPaths.ScenePath(name);
                if (File.Exists(path)) scenes.Add(new EditorBuildSettingsScene(path, true));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void CreateScene(string sceneName, bool overwrite, System.Action populate)
        {
            EditorPaths.EnsureFolder(EditorPaths.Scenes);
            string path = EditorPaths.ScenePath(sceneName);

            if (File.Exists(path) && !overwrite)
            {
                Debug.Log($"[DEADZONE] Scene '{sceneName}' already exists, kept as is.");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            populate();
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[DEADZONE] Scene '{sceneName}' created.");
        }

        // ------------------------------------------------------------------ Bootstrap / placeholders

        private static void BuildBootstrap()
        {
            CreateCamera("Main Camera");
            new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
        }

        private static void BuildPlaceholder(string title)
        {
            CreateCamera("Main Camera");
            CreateDirectionalLight();
            Canvas canvas = CreateCanvas("Canvas");
            RectTransform safe = CreateSafeArea(canvas.transform);
            TextMeshProUGUI label = UIFactory.Text("Title", safe, title, 56f, UIColors.Gold, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Stretch(label.rectTransform);
            CreateEventSystem();
        }

        // ------------------------------------------------------------------ Main menu

        private static void BuildMainMenu()
        {
            CreateCamera("Main Camera");
            Canvas canvas = CreateCanvas("Canvas");
            RectTransform safe = CreateSafeArea(canvas.transform);

            TextMeshProUGUI title = UIFactory.Text("Title", safe, "DEADZONE", 140f, UIColors.Gold, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -330), new Vector2(1000, 180));
            TextMeshProUGUI subtitle = UIFactory.Text("Subtitle", safe, "LAST EMPIRE", 64f, UIColors.Silver, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Place(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -500), new Vector2(1000, 90));

            GameObject play = UIPrefabGenerator.Instantiate(UIPrefabGenerator.PrimaryButtonPath, safe);
            play.name = "PlayButton";
            UIFactory.Place((RectTransform)play.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -80), new Vector2(620, 140));
            play.GetComponentInChildren<LocalizedText>().SetKey("menu.play");

            GameObject newGame = UIPrefabGenerator.Instantiate(UIPrefabGenerator.SecondaryButtonPath, safe);
            newGame.name = "NewGameButton";
            UIFactory.Place((RectTransform)newGame.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -250), new Vector2(620, 110));
            newGame.GetComponentInChildren<LocalizedText>().SetKey("menu.newgame");

            GameObject language = UIPrefabGenerator.Instantiate(UIPrefabGenerator.SecondaryButtonPath, safe);
            language.name = "LanguageButton";
            UIFactory.Place((RectTransform)language.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -390), new Vector2(620, 100));
            language.GetComponentInChildren<LocalizedText>().SetKey("menu.language");

            var controller = new GameObject("MainMenuController").AddComponent<MainMenuController>();
            controller.Bind(
                play.GetComponent<Button>(),
                newGame.GetComponent<Button>(),
                language.GetComponent<Button>());

            CreateOverlay();
            CreateEventSystem();
        }

        // ------------------------------------------------------------------ Base

        private static void BuildBase()
        {
            GameObject cameraObject = CreateCamera("Main Camera");
            cameraObject.AddComponent<IsometricCameraController>();
            CreateDirectionalLight();
            ConfigureLighting();

            Transform environment = ModelBuilder.Group("Environment [PLACEHOLDER]", null);
            PlaceholderModels.BuildEnvironment(environment);

            Transform buildings = ModelBuilder.Group("Buildings [PLACEHOLDER]", null);
            PlaceBuilding(buildings, "Headquarters", new Vector3(0, 0, 16));
            PlaceBuilding(buildings, "Barracks", new Vector3(-10, 0, -12));
            PlaceBuilding(buildings, "Hospital", new Vector3(-10, 0, 4));
            PlaceBuilding(buildings, "ResearchLab", new Vector3(10, 0, 4));
            PlaceBuilding(buildings, "FoodFarm", new Vector3(-14, 0, 17));
            PlaceBuilding(buildings, "SteelFactory", new Vector3(10, 0, -12));

            BuildHud();
            CreateOverlay();
            CreateEventSystem();
        }

        private static void PlaceBuilding(Transform parent, string fileName, Vector3 position)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EditorPaths.PrefabsBuildings + "/" + fileName + ".prefab");
            if (prefab == null)
            {
                Debug.LogError($"[DEADZONE] Missing building prefab '{fileName}'. Run Tools > DEADZONE > Generate Building Prefabs.");
                return;
            }
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.position = position;
        }

        /// <summary>The toast sits just above the building info sheet (216 + 900 + gap).</summary>
        public const float ToastBottomOffset = 1140f;

        private static void BuildHud()
        {
            Canvas canvas = CreateCanvas("HUD Canvas");
            RectTransform safe = CreateSafeArea(canvas.transform);

            // ---- Top bar
            Image top = UIFactory.Panel("TopBar", safe, UIColors.WithAlpha(UIColors.Panel, 0.94f));
            UIFactory.Place(top.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 290));
            UIFactory.AddOutline(top, UIColors.WithAlpha(UIColors.Gold, 0.5f), 2f);

            Image avatar = UIFactory.Panel("Avatar", top.transform, UIColors.MilitaryGreen, true);
            var avatarButton = avatar.gameObject.AddComponent<Button>();
            avatarButton.targetGraphic = avatar;
            avatar.gameObject.AddComponent<ButtonPressAnimator>();
            UIFactory.Place(avatar.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -24), new Vector2(130, 130));
            UIFactory.AddOutline(avatar, UIColors.Gold, 3f);
            TextMeshProUGUI avatarText = UIFactory.Text("AvatarText", avatar.transform, "CMD", 36f, UIColors.TextPrimary, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Stretch(avatarText.rectTransform);

            TextMeshProUGUI nameText = UIFactory.Text("PlayerName", top.transform, "Commander", 40f, UIColors.TextPrimary, TextAlignmentOptions.Left, FontStyles.Bold);
            UIFactory.Place(nameText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(176, -22), new Vector2(520, 48));
            TextMeshProUGUI levelText = UIFactory.Text("Level", top.transform, "Level 1", 30f, UIColors.TextSecondary, TextAlignmentOptions.Left);
            UIFactory.Place(levelText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(176, -72), new Vector2(520, 38));
            TextMeshProUGUI powerText = UIFactory.Text("Power", top.transform, "Power: 0", 30f, UIColors.Gold, TextAlignmentOptions.Left, FontStyles.Bold);
            UIFactory.Place(powerText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(176, -114), new Vector2(520, 38));

            Image vip = UIFactory.Panel("VipBadge", top.transform, UIColors.Background, false);
            UIFactory.Place(vip.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-24, -24), new Vector2(190, 60));
            UIFactory.AddOutline(vip, UIColors.Gold, 3f);
            TextMeshProUGUI vipText = UIFactory.Text("VipText", vip.transform, "VIP 0", 34f, UIColors.Gold, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Stretch(vipText.rectTransform);

            GameObject saveObject = UIPrefabGenerator.Instantiate(UIPrefabGenerator.SecondaryButtonPath, top.transform);
            saveObject.name = "SaveButton";
            UIFactory.Place((RectTransform)saveObject.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-24, -96), new Vector2(190, 64));
            saveObject.GetComponentInChildren<LocalizedText>().SetKey("hud.save");

            // Resource row
            RectTransform row = UIFactory.Rect("ResourceRow", top.transform);
            UIFactory.Place(row, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 12), new Vector2(-24, 76));

            ResourceType[] shown = { ResourceType.Food, ResourceType.Wood, ResourceType.Steel, ResourceType.Fuel, ResourceType.Gold, ResourceType.Diamonds };
            string[] codes = { "FD", "WD", "ST", "FU", "GD", "DM" };
            string[] colors = { "#8BC34A", "#A1783F", "#9AA7B5", "#E0A030", "#D4AF63", "#4C9FE8" };
            for (int i = 0; i < shown.Length; i++)
            {
                GameObject counter = UIPrefabGenerator.Instantiate(UIPrefabGenerator.ResourceCounterPath, row);
                counter.name = "Counter_" + shown[i];
                UIFactory.Slot((RectTransform)counter.transform, i, shown.Length, 3f, 4f);
                counter.GetComponent<ResourceCounterView>().SetType(shown[i]);
                counter.transform.Find("Icon").GetComponent<Image>().color = UIColors.Hex(colors[i]);
                counter.transform.Find("Icon/Code").GetComponent<TextMeshProUGUI>().text = codes[i];
            }

            // ---- Side buttons
            HudFeature[] left = { HudFeature.ConstructionQueue, HudFeature.ResearchQueue, HudFeature.Missions };
            HudFeature[] right = { HudFeature.Events, HudFeature.EventCalendar, HudFeature.Vip, HudFeature.DailyRewards, HudFeature.BattlePass, HudFeature.SpecialOffers };
            for (int i = 0; i < left.Length; i++)
            {
                CreateIconButton(safe, left[i], new Vector2(0, 1), new Vector2(24, -320 - i * 136));
            }
            for (int i = 0; i < right.Length; i++)
            {
                CreateIconButton(safe, right[i], new Vector2(1, 1), new Vector2(-24, -320 - i * 136));
            }

            // ---- Bottom navigation
            Image nav = UIFactory.Panel("BottomNav", safe, UIColors.WithAlpha(UIColors.Panel, 0.96f));
            UIFactory.Place(nav.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 200));
            UIFactory.AddOutline(nav, UIColors.WithAlpha(UIColors.Gold, 0.5f), 2f);

            HudFeature[] items = { HudFeature.Base, HudFeature.Heroes, HudFeature.Troops, HudFeature.Inventory, HudFeature.World, HudFeature.Alliance, HudFeature.Shop };
            for (int i = 0; i < items.Length; i++)
            {
                GameObject button = UIPrefabGenerator.Instantiate(UIPrefabGenerator.IconButtonPath, nav.transform);
                button.name = "Nav_" + items[i];
                UIFactory.Slot((RectTransform)button.transform, i, items.Length, 6f, 16f);
                ConfigureIconButton(button, items[i]);
            }

            // ---- Building info sheet (PHASE 02) and construction queue window
            UIPrefabGenerator.BuildInfoPanelUi(safe);
            UIPrefabGenerator.Instantiate(UIPrefabGenerator.ConstructionQueuePath, canvas.transform);

            CreateToast(safe, ToastBottomOffset);

            // ---- HUD controller
            var hud = new GameObject("HudController").AddComponent<HudController>();
            hud.Bind(nameText, levelText, powerText, vipText, saveObject.GetComponent<Button>(), avatarButton);
        }

        private static void CreateIconButton(Transform parent, HudFeature feature, Vector2 anchor, Vector2 position)
        {
            GameObject button = UIPrefabGenerator.Instantiate(UIPrefabGenerator.IconButtonPath, parent);
            button.name = "Side_" + feature;
            UIFactory.Place((RectTransform)button.transform, anchor, anchor, anchor, position, new Vector2(120, 120));
            ConfigureIconButton(button, feature);
        }

        private static void ConfigureIconButton(GameObject button, HudFeature feature)
        {
            button.GetComponent<NavButton>().SetFeature(feature);
            button.GetComponentInChildren<LocalizedText>().SetKey(FeatureRouter.NameKey(feature));
        }


        private static void CreateToast(Transform safe, float bottomOffset)
        {
            Image toast = UIFactory.Panel("Toast", safe, UIColors.WithAlpha(UIColors.Background, 0.92f), false);
            UIFactory.Place(toast.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, bottomOffset), new Vector2(860, 84));
            UIFactory.AddOutline(toast, UIColors.Gold, 2f);
            var toastGroup = toast.gameObject.AddComponent<CanvasGroup>();
            toastGroup.blocksRaycasts = false;
            toastGroup.interactable = false;
            TextMeshProUGUI toastText = UIFactory.Text("ToastText", toast.transform, "", 32f, UIColors.TextPrimary, TextAlignmentOptions.Center);
            UIFactory.Stretch(toastText.rectTransform);
            toastText.margin = new Vector4(16, 4, 16, 4);
            UIFactory.AutoSize(toastText, 18f, 32f);
            toast.gameObject.AddComponent<ToastView>().Bind(toastGroup, toastText);
        }

        /// <summary>Adds the shared popup/tooltip/loading overlay (nested prefab instance).</summary>
        private static void CreateOverlay()
        {
            UIPrefabGenerator.Instantiate(UIPrefabGenerator.UIOverlayPath, null);
        }

        // ------------------------------------------------------------------ UI gallery (developer scene)

        private static void BuildGallery()
        {
            CreateCamera("Main Camera");
            Canvas canvas = CreateCanvas("Canvas");
            RectTransform safe = CreateSafeArea(canvas.transform);

            Image background = UIFactory.Panel("Background", safe, UIColors.Background, false);
            UIFactory.Stretch(background.rectTransform);

            TextMeshProUGUI title = UIFactory.Text("Title", safe, "UI GALLERY [DEV]", 48f, UIColors.Gold, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Place(title.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(-40, 70));

            GameObject scrollObject = UIPrefabGenerator.Instantiate(UIPrefabGenerator.ScrollablePanelPath, safe);
            var scrollRect = (RectTransform)scrollObject.transform;
            UIFactory.Stretch(scrollRect);
            scrollRect.offsetMin = new Vector2(30, 30);
            scrollRect.offsetMax = new Vector2(-30, -110);
            Transform content = scrollObject.GetComponent<ScrollRect>().content;

            // Popup test buttons
            string[] keys = { "popup.close", "confirm.yes", "reward.claim", "loading.text", "hud.save" };
            string[] names = { "PopupButton", "ConfirmButton", "RewardButton", "LoadingButton", "ToastButton" };
            var buttons = new Button[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                GameObject button = UIPrefabGenerator.Instantiate(UIPrefabGenerator.SecondaryButtonPath, content);
                button.name = names[i];
                button.GetComponentInChildren<LocalizedText>().SetKey(keys[i]);
                SetHeight(button, 90f);
                buttons[i] = button.GetComponent<Button>();
            }

            // Tab navigation
            GameObject tabs = UIPrefabGenerator.Instantiate(UIPrefabGenerator.TabNavigationPath, content);
            SetHeight(tabs, 100f);
            tabs.GetComponent<TabNavigationView>().SetLabels(Loc.T("nav.base"), Loc.T("nav.heroes"), Loc.T("nav.shop"));

            // Progress bar
            GameObject bar = UIPrefabGenerator.Instantiate(UIPrefabGenerator.ProgressBarPath, content);
            SetHeight(bar, 40f);
            bar.GetComponent<ProgressBarView>().SetProgress(65, 100);

            // Card row
            RectTransform row = UIFactory.Rect("CardRow", content);
            var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 16f;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = false;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 380f;

            GameObject hero = UIPrefabGenerator.Instantiate(UIPrefabGenerator.HeroCardPath, row);
            hero.GetComponent<HeroCardView>().Set("Sample Hero", "Class", 12, 3, Rarity.Epic);
            GameObject item = UIPrefabGenerator.Instantiate(UIPrefabGenerator.ItemCardPath, row);
            item.GetComponent<ItemCardView>().Set("Sample Item", "IT", 25, Rarity.Rare, UIColors.MilitaryGreen);
            GameObject shop = UIPrefabGenerator.Instantiate(UIPrefabGenerator.ShopCardPath, row);
            shop.GetComponent<ShopCardView>().Set("Sample Pack", "PK", "100", true, null);

            // Event card
            GameObject eventCard = UIPrefabGenerator.Instantiate(UIPrefabGenerator.EventCardPath, content);
            SetHeight(eventCard, 180f);
            eventCard.GetComponent<EventCardView>().Set("Sample Event", EventCardView.Status.Active, new System.TimeSpan(1, 3, 25, 10));

            // Notification badge
            GameObject badge = UIPrefabGenerator.Instantiate(UIPrefabGenerator.NotificationBadgePath, content);
            SetHeight(badge, 44f);
            badge.GetComponent<NotificationBadgeView>().SetCount(7);

            CreateToast(safe, 80f);

            var gallery = new GameObject("UIGalleryController").AddComponent<UIGalleryController>();
            gallery.Bind(buttons[0], buttons[1], buttons[2], buttons[3], buttons[4]);

            CreateOverlay();
            CreateEventSystem();
        }

        private static void SetHeight(GameObject go, float height)
        {
            var element = go.GetComponent<LayoutElement>();
            if (element == null) element = go.AddComponent<LayoutElement>();
            element.preferredHeight = height;
            element.minHeight = height;
        }

        // ------------------------------------------------------------------ Shared scene pieces

        private static GameObject CreateCamera(string name)
        {
            var go = new GameObject(name) { tag = "MainCamera" };
            var camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = UIColors.Background;
            camera.orthographic = true;
            camera.orthographicSize = 14f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 400f;
            go.AddComponent<AudioListener>();

            go.transform.rotation = Quaternion.Euler(35.264f, 45f, 0f);
            go.transform.position = new Vector3(0f, 0f, 2f) - go.transform.forward * 120f;
            return go;
        }

        private static void CreateDirectionalLight()
        {
            var go = new GameObject("Directional Light");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = UIColors.Hex("#FFD9A8");
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void ConfigureLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = UIColors.Hex("#3A4654");
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = UIColors.Background;
            RenderSettings.fogDensity = 0.003f;
        }

        private static Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static RectTransform CreateSafeArea(Transform canvas)
        {
            RectTransform rect = UIFactory.Rect("SafeArea", canvas);
            UIFactory.Stretch(rect);
            rect.gameObject.AddComponent<SafeAreaFitter>();
            return rect;
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            var module = go.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }
    }
}
