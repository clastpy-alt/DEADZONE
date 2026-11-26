using System;
using Deadzone.Core;
using Deadzone.Data;
using Deadzone.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.EditorTools
{
    /// <summary>Generates reusable UI prefabs (idempotent: existing prefabs are kept unless overwrite).</summary>
    internal static partial class UIPrefabGenerator
    {
        public const string PrimaryButtonPath = EditorPaths.PrefabsUI + "/PrimaryButton.prefab";
        public const string SecondaryButtonPath = EditorPaths.PrefabsUI + "/SecondaryButton.prefab";
        public const string IconButtonPath = EditorPaths.PrefabsUI + "/IconButton.prefab";
        public const string ResourceCounterPath = EditorPaths.PrefabsUI + "/ResourceCounter.prefab";

        public static void GenerateAll(bool overwrite)
        {
            EditorPaths.EnsureFolder(EditorPaths.PrefabsUI);

            SavePrefab(PrimaryButtonPath, overwrite, BuildPrimaryButton);
            SavePrefab(SecondaryButtonPath, overwrite, BuildSecondaryButton);
            SavePrefab(IconButtonPath, overwrite, BuildIconButton);
            SavePrefab(ResourceCounterPath, overwrite, BuildResourceCounter);
            GenerateComponents(overwrite);
            GenerateBuildingUi(overwrite);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static GameObject Instantiate(string prefabPath, Transform parent)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) throw new InvalidOperationException("Missing UI prefab: " + prefabPath + " (run Generate UI)");
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        }

        private static void SavePrefab(string path, bool overwrite, Func<GameObject> build)
        {
            if (!overwrite && AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            GameObject go = build();
            PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);
        }

        private static GameObject BuildPrimaryButton()
        {
            Button button = UIFactory.CreateButton("PrimaryButton", null, UIColors.MilitaryGreen, UIColors.Gold,
                "menu.play", 44f, out _);
            ((RectTransform)button.transform).sizeDelta = new Vector2(500f, 120f);
            return button.gameObject;
        }

        private static GameObject BuildSecondaryButton()
        {
            Button button = UIFactory.CreateButton("SecondaryButton", null, UIColors.Panel, UIColors.Silver,
                "menu.newgame", 36f, out _);
            ((RectTransform)button.transform).sizeDelta = new Vector2(500f, 100f);
            return button.gameObject;
        }

        private static GameObject BuildIconButton()
        {
            Button button = UIFactory.CreateButton("IconButton", null, UIColors.Panel,
                UIColors.WithAlpha(UIColors.Gold, 0.7f), "nav.base", 30f, out _);
            ((RectTransform)button.transform).sizeDelta = new Vector2(120f, 120f);

            var group = button.gameObject.AddComponent<CanvasGroup>();

            Image overlay = UIFactory.Panel("LockOverlay", button.transform, new Color(0f, 0f, 0f, 0.55f), false);
            UIFactory.Stretch(overlay.rectTransform);

            TextMeshProUGUI lockText = UIFactory.Text("LockText", overlay.transform, Loc.T("common.locked"), 20f,
                UIColors.TextSecondary, TextAlignmentOptions.Bottom);
            UIFactory.Stretch(lockText.rectTransform);
            lockText.margin = new Vector4(4f, 4f, 4f, 6f);
            lockText.gameObject.AddComponent<LocalizedText>().SetKey("common.locked");

            var nav = button.gameObject.AddComponent<NavButton>();
            nav.Configure(HudFeature.Base, overlay.gameObject, group);
            return button.gameObject;
        }

        private static GameObject BuildResourceCounter()
        {
            Image root = UIFactory.Panel("ResourceCounter", null, new Color(0f, 0f, 0f, 0.35f), true);
            ((RectTransform)root.transform).sizeDelta = new Vector2(170f, 64f);
            UIFactory.AddOutline(root, UIColors.WithAlpha(UIColors.Silver, 0.5f), 2f);

            Image icon = UIFactory.Panel("Icon", root.transform, UIColors.Silver, false);
            UIFactory.Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(8f, 0f), new Vector2(44f, 44f));

            TextMeshProUGUI code = UIFactory.Text("Code", icon.transform, "--", 18f, UIColors.Background,
                TextAlignmentOptions.Center, FontStyles.Bold);
            UIFactory.Stretch(code.rectTransform);

            TextMeshProUGUI value = UIFactory.Text("Value", root.transform, "0", 30f, UIColors.TextPrimary,
                TextAlignmentOptions.Right, FontStyles.Bold);
            UIFactory.Stretch(value.rectTransform);
            value.rectTransform.offsetMin = new Vector2(60f, 0f);
            value.rectTransform.offsetMax = new Vector2(-8f, 0f);
            UIFactory.AutoSize(value, 16f, 30f);

            root.gameObject.AddComponent<TooltipTrigger>();
            var view = root.gameObject.AddComponent<ResourceCounterView>();
            view.Configure(ResourceType.Food, value);
            return root.gameObject;
        }
    }
}
