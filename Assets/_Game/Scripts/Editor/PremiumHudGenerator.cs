using System;
using System.IO;
using System.Text;
using Deadzone.Core;
using Deadzone.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Deadzone.EditorTools
{
    /// <summary>
    /// PHASE 02.5 premium HUD. Patches the existing Base scene HUD in place (never rebuilds it): icon set, avatar frame with level
    /// badge, power / VIP icons, resource icons, side and bottom buttons with icons, lock icon for unfinished features, live
    /// construction status and the settings gear that replaces the save button (the game saves automatically).
    /// Every step is idempotent: running it twice changes nothing the second time.
    /// </summary>
    internal static class PremiumHudGenerator
    {
        private static readonly Color Danger = new Color32(0xD9, 0x4B, 0x4B, 255);

        public static string Generate()
        {
            var report = new StringBuilder();
            int icons = UiIconGenerator.GenerateAll(false);
            report.AppendLine($"Icons: {icons} new file(s), {UiIconGenerator.Names.Count} in the set");

            UIPrefabGenerator.GenerateSettingsUi(false);

            string path = EditorPaths.ScenePath(SceneNames.Base);
            if (!File.Exists(path))
            {
                report.AppendLine("Base scene not found - run Setup Phase 02 first.");
                return report.ToString().Trim();
            }

            Scene previousActive = SceneManager.GetActiveScene();
            Scene existing = SceneManager.GetSceneByPath(path);
            bool wasLoaded = existing.IsValid() && existing.isLoaded;
            Scene scene = wasLoaded ? existing : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(scene);
                report.Append(Patch(scene));
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            finally
            {
                if (previousActive.IsValid() && previousActive.isLoaded) SceneManager.SetActiveScene(previousActive);
                if (!wasLoaded) EditorSceneManager.CloseScene(scene, true);
            }
            return report.ToString().Trim();
        }

        // ------------------------------------------------------------------ scene patch

        private static string Patch(Scene scene)
        {
            var report = new StringBuilder();
            Transform canvas = Find(scene, "HUD Canvas");
            if (canvas == null) return "HUD Canvas not found.\n";

            Transform topBar = FindUnder(canvas, "TopBar");
            if (topBar != null)
            {
                PatchAvatar(topBar, report);
                PatchPower(topBar);
                PatchVipAndSettings(topBar, report);
                RecAll(topBar);
            }

            int counters = 0;
            int buttons = 0;
            foreach (Transform t in canvas.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.StartsWith("Counter_") && PatchCounter(t)) { counters++; RecAll(t); }
                else if ((t.name.StartsWith("Nav_") || t.name.StartsWith("Side_")) && PatchButton(t)) { buttons++; RecAll(t); }
            }
            report.AppendLine($"Resource counters: {counters}, nav/side buttons: {buttons}");

            if (FindUnder(canvas, "SettingsPanel") == null)
            {
                UIPrefabGenerator.Instantiate(UIPrefabGenerator.SettingsPanelPath, canvas);
                report.AppendLine("Settings panel added to the HUD canvas.");
            }
            return report.ToString();
        }

        private static void PatchAvatar(Transform topBar, StringBuilder report)
        {
            Transform avatar = FindUnder(topBar, "Avatar");
            if (avatar == null) return;

            var image = avatar.GetComponent<Image>();
            image.sprite = Icon("avatar_cmd");
            image.color = Color.white;
            image.preserveAspect = true;
            var outline = avatar.GetComponent<Outline>();
            if (outline != null) outline.enabled = false;
            Transform code = avatar.Find("AvatarText");
            if (code != null) code.gameObject.SetActive(false);

            Transform frame = avatar.Find("AvatarFrame");
            if (frame == null)
            {
                Image f = UIFactory.Panel("AvatarFrame", avatar, Color.white, false);
                UIFactory.Stretch(f.rectTransform);
                f.rectTransform.offsetMin = new Vector2(-8f, -8f);
                f.rectTransform.offsetMax = new Vector2(8f, 8f);
                f.sprite = Icon("frame_avatar");
                frame = f.transform;
            }

            Transform badge = avatar.Find("LevelBadge");
            TMP_Text number;
            if (badge == null)
            {
                Image b = UIFactory.Panel("LevelBadge", avatar, UIColors.Gold, false);
                UIFactory.Place(b.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-4f, 10f), new Vector2(58f, 64f));
                b.sprite = Icon("badge_level");
                b.preserveAspect = true;
                TextMeshProUGUI t = UIFactory.Text("Number", b.transform, "1", 30f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
                UIFactory.Stretch(t.rectTransform);
                UIFactory.AutoSize(t, 16f, 30f);
                number = t;
            }
            else
            {
                number = badge.Find("Number").GetComponent<TMP_Text>();
            }

            var hud = UnityEngine.Object.FindAnyObjectByType<HudController>(FindObjectsInactive.Include);
            if (hud != null)
            {
                hud.BindLevelBadge(number);
                EditorUtility.SetDirty(hud);
            }
            report.AppendLine("Avatar frame + level badge ready.");
        }

        private static void PatchPower(Transform topBar)
        {
            Transform power = FindUnder(topBar, "Power");
            if (power == null) return;

            var rect = (RectTransform)power;
            rect.anchoredPosition = new Vector2(226f, rect.anchoredPosition.y);
            rect.sizeDelta = new Vector2(470f, rect.sizeDelta.y);

            if (topBar.Find("PowerIcon") == null)
            {
                Image icon = UIFactory.Panel("PowerIcon", topBar, Color.white, false);
                UIFactory.Place(icon.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(176f, -114f), new Vector2(40f, 40f));
                icon.sprite = Icon("hud_power");
                icon.preserveAspect = true;
            }
        }

        private static void PatchVipAndSettings(Transform topBar, StringBuilder report)
        {
            Transform vip = FindUnder(topBar, "VipBadge");
            if (vip != null)
            {
                var rect = (RectTransform)vip;
                rect.anchoredPosition = new Vector2(-104f, -24f);
                rect.sizeDelta = new Vector2(190f, 64f);

                Transform text = vip.Find("VipText");
                if (text != null)
                {
                    var textRect = (RectTransform)text;
                    textRect.offsetMin = new Vector2(52f, 0f);
                    textRect.offsetMax = new Vector2(-8f, 0f);
                }
                if (vip.Find("VipIcon") == null)
                {
                    Image icon = UIFactory.Panel("VipIcon", vip, Color.white, false);
                    UIFactory.Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(46f, 46f));
                    icon.sprite = Icon("hud_vip");
                    icon.preserveAspect = true;
                }
            }

            Transform settings = FindUnder(topBar, "SettingsButton");
            if (settings == null) settings = FindUnder(topBar, "SaveButton");
            if (settings == null) return;

            settings.name = "SettingsButton";
            var settingsRect = (RectTransform)settings;
            settingsRect.anchoredPosition = new Vector2(-24f, -24f);
            settingsRect.sizeDelta = new Vector2(64f, 64f);

            Transform label = settings.Find("Label");
            if (label != null) Hide(label.gameObject);
            if (settings.Find("GearIcon") == null)
            {
                Image icon = UIFactory.Panel("GearIcon", settings, Color.white, false);
                UIFactory.Stretch(icon.rectTransform);
                icon.rectTransform.offsetMin = new Vector2(8f, 8f);
                icon.rectTransform.offsetMax = new Vector2(-8f, -8f);
                icon.sprite = Icon("hud_gear");
                icon.preserveAspect = true;
            }
            report.AppendLine("Save button replaced by the settings gear (auto-save stays on).");
        }

        private static bool PatchCounter(Transform counter)
        {
            string name = counter.name.Substring("Counter_".Length);
            if (!Enum.TryParse(name, out Deadzone.Data.ResourceType type)) return false;

            Transform iconTransform = counter.Find("Icon");
            if (iconTransform == null) return false;

            var image = iconTransform.GetComponent<Image>();
            SetImage(image, Icon(UiIcons.ForResource(type)), Color.white);
            SetSize((RectTransform)iconTransform, new Vector2(50f, 50f));
            Transform code = iconTransform.Find("Code");
            if (code != null) Hide(code.gameObject);
            return true;
        }

        private static bool PatchButton(Transform button)
        {
            bool nav = button.name.StartsWith("Nav_");
            string featureName = button.name.Substring(nav ? 4 : 5);
            if (!Enum.TryParse(featureName, out HudFeature feature)) return false;

            float iconSize = nav ? 84f : 66f;
            float iconTop = nav ? 14f : 8f;
            float labelHeight = nav ? 46f : 34f;

            Transform icon = button.Find("Icon");
            if (icon == null)
            {
                Image image = UIFactory.Panel("Icon", button, Color.white, false);
                icon = image.transform;
            }
            icon.SetSiblingIndex(0); // behind the label and the lock overlay
            var iconImage = icon.GetComponent<Image>();
            iconImage.sprite = Icon(UiIcons.ForFeature(feature));
            iconImage.preserveAspect = true;
            UIFactory.Place((RectTransform)icon, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -iconTop), new Vector2(iconSize, iconSize));

            Transform label = button.Find("Label");
            if (label != null)
            {
                var rect = (RectTransform)label;
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.offsetMin = new Vector2(2f, 4f);
                rect.offsetMax = new Vector2(-2f, 4f + labelHeight);
                var text = label.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    var serializedText = new SerializedObject(text);
                    serializedText.FindProperty("m_fontSizeMax").floatValue = nav ? 26f : 20f;
                    serializedText.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            // Lock overlay: a small padlock in the corner instead of the word "Locked".
            Transform overlay = button.Find("LockOverlay");
            if (overlay != null)
            {
                Transform lockText = overlay.Find("LockText");
                if (lockText != null) Hide(lockText.gameObject);
                if (overlay.Find("LockIcon") == null)
                {
                    Image lockIcon = UIFactory.Panel("LockIcon", overlay, Color.white, false);
                    UIFactory.Place(lockIcon.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-4f, -4f), new Vector2(nav ? 44f : 38f, nav ? 44f : 38f));
                    lockIcon.sprite = Icon("hud_lock");
                    lockIcon.preserveAspect = true;
                }
            }

            if (feature == HudFeature.Base && button.Find("ActiveBar") == null)
            {
                Image bar = UIFactory.Panel("ActiveBar", button, UIColors.Gold, false);
                UIFactory.Place(bar.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 2f), new Vector2(72f, 6f));
            }

            if (feature == HudFeature.ConstructionQueue && button.GetComponent<HudStatusView>() == null)
            {
                AddConstructionStatus(button);
            }
            return true;
        }

        private static void AddConstructionStatus(Transform button)
        {
            Image pill = UIFactory.Panel("TimerPill", button, new Color(0.04f, 0.06f, 0.08f, 0.85f), false);
            UIFactory.Place(pill.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(104f, 28f));
            TextMeshProUGUI timer = UIFactory.Text("Timer", pill.transform, "00:00", 20f, new Color32(0xFF, 0xD2, 0x5A, 255), TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Stretch(timer.rectTransform);
            UIFactory.AutoSize(timer, 12f, 20f);
            pill.gameObject.SetActive(false);

            Image bubble = UIFactory.Panel("CountBadge", button, Danger, false);
            UIFactory.Place(bubble.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-6f, -6f), new Vector2(40f, 40f));
            bubble.sprite = Icon("hud_dot");
            TextMeshProUGUI count = UIFactory.Text("Count", bubble.transform, "0", 24f, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Stretch(count.rectTransform);
            UIFactory.AutoSize(count, 12f, 24f);
            var badge = bubble.gameObject.AddComponent<NotificationBadgeView>();
            badge.Bind(count);
            bubble.gameObject.SetActive(false);

            var status = button.gameObject.AddComponent<HudStatusView>();
            status.Bind(badge, timer, pill.gameObject);
        }

        // ------------------------------------------------------------------ helpers

        /// <summary>Property changes on children of prefab instances (HUD counters, nav buttons) only persist when recorded.</summary>
        private static void Rec(UnityEngine.Object target)
        {
            if (target == null) return;
            EditorUtility.SetDirty(target);
            if (PrefabUtility.IsPartOfPrefabInstance(target)) PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        }

        private static void RecAll(Transform root)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                Rec(t.gameObject);
                foreach (Component component in t.GetComponents<Component>()) Rec(component);
            }
        }

        /// <summary>Deactivates an object through SerializedObject, which always records the override on prefab instances.</summary>
        private static void Hide(GameObject target)
        {
            if (target == null) return;
            var serialized = new SerializedObject(target);
            serialized.FindProperty("m_IsActive").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetImage(Image image, Sprite sprite, Color color)
        {
            var serialized = new SerializedObject(image);
            serialized.FindProperty("m_Sprite").objectReferenceValue = sprite;
            serialized.FindProperty("m_Color").colorValue = color;
            serialized.FindProperty("m_PreserveAspect").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSize(RectTransform rect, Vector2 size)
        {
            var serialized = new SerializedObject(rect);
            serialized.FindProperty("m_SizeDelta").vector2Value = size;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Sprite Icon(string name)
        {
            return string.IsNullOrEmpty(name) ? null : AssetDatabase.LoadAssetAtPath<Sprite>($"{UiIconGenerator.Folder}/{name}.png");
        }

        private static Transform Find(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == name) return root.transform;
            }
            return null;
        }

        private static Transform FindUnder(Transform parent, string name)
        {
            foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name) return t;
            }
            return null;
        }

        /// <summary>Validation: icon names that the HUD needs but that have no PNG yet.</summary>
        public static string Validate()
        {
            var missing = UiIconGenerator.FindMissing();
            return missing.Count == 0 ? "All HUD icons present." : "Missing icons: " + string.Join(", ", missing);
        }
    }
}
