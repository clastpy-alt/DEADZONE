using Deadzone.Core;
using Deadzone.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.EditorTools
{
    /// <summary>PHASE 02.5 settings window: graphics quality, language, manual save.</summary>
    internal static partial class UIPrefabGenerator
    {
        public const string SettingsPanelPath = EditorPaths.PrefabsUI + "/SettingsPanel.prefab";

        public static void GenerateSettingsUi(bool overwrite)
        {
            EditorPaths.EnsureFolder(EditorPaths.PrefabsUI);
            SavePrefab(SettingsPanelPath, overwrite, BuildSettingsPanel);
        }

        private static GameObject BuildSettingsPanel()
        {
            GameObject root = BuildPopupRoot("SettingsPanel", new Vector2(960f, 1120f), out UIPanel panel, out RectTransform window, out Button dim);

            TextMeshProUGUI title = AddTitle(window, Loc.T("settings.title"));
            title.gameObject.AddComponent<LocalizedText>().SetKey("settings.title");

            TextMeshProUGUI header = Body(window, "GraphicsHeader", 34f, 22f, TextAlignmentOptions.Left, UIColors.Gold, FontStyles.Bold);
            header.text = Loc.T("gfx.title");
            header.gameObject.AddComponent<LocalizedText>().SetKey("gfx.title");
            Strip(header.rectTransform, 32f, 32f, 128f, 50f);

            string[] keys = { "gfx.low", "gfx.medium", "gfx.high" };
            var quality = new Button[3];
            for (int i = 0; i < 3; i++)
            {
                Button button = UIFactory.CreateButton("Quality_" + i, window, UIColors.Panel, UIColors.Gold, keys[i], 34f, out _);
                Column((RectTransform)button.transform, i / 3f, (i + 1) / 3f, i == 0 ? 24f : 8f, i == 2 ? 24f : 8f, 188f, 112f);
                quality[i] = button;
            }

            TextMeshProUGUI info = Body(window, "QualityInfo", 28f, 18f, TextAlignmentOptions.Center, UIColors.TextSecondary);
            Strip(info.rectTransform, 32f, 32f, 316f, 130f);

            Button language = UIFactory.CreateButton("LanguageButton", window, UIColors.Panel, UIColors.Silver, "menu.language", 34f, out _);
            Strip((RectTransform)language.transform, 24f, 24f, 480f, 100f);

            Button save = UIFactory.CreateButton("SaveNowButton", window, UIColors.MilitaryGreen, UIColors.Gold, "settings.save", 34f, out _);
            Strip((RectTransform)save.transform, 24f, 24f, 610f, 100f);

            TextMeshProUGUI autosave = Body(window, "AutosaveNote", 26f, 18f, TextAlignmentOptions.Center, UIColors.TextSecondary);
            autosave.text = Loc.T("settings.autosave");
            autosave.gameObject.AddComponent<LocalizedText>().SetKey("settings.autosave");
            Strip(autosave.rectTransform, 32f, 32f, 726f, 60f);

            Button close = AddBottomButton(window, "CloseButton", "popup.close", UIColors.Panel, UIColors.Silver, 0.5f, 0.6f);

            var view = root.AddComponent<SettingsPanel>();
            view.Bind(panel, close, dim, language, save, quality, info);
            return root;
        }
    }
}
