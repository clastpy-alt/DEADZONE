using Deadzone.Core;
using Deadzone.Data;
using Deadzone.Visuals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.UI
{
    /// <summary>
    /// Settings window opened from the HUD gear: graphics quality (LOW / MEDIUM / HIGH, saved per device), language and a manual
    /// save. The game saves automatically (timer, pause, quit, every construction), so the save button is only a convenience.
    /// Frame-rate figures shown here are targets, not measurements.
    /// </summary>
    public sealed class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private UIPanel panel;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button dimButton;
        [SerializeField] private Button languageButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button[] qualityButtons = new Button[3];
        [SerializeField] private TMP_Text infoText;

        public void Bind(UIPanel uiPanel, Button close, Button dim, Button language, Button save, Button[] quality, TMP_Text info)
        {
            panel = uiPanel;
            closeButton = close;
            dimButton = dim;
            languageButton = language;
            saveButton = save;
            qualityButtons = quality;
            infoText = info;
        }

        /// <summary>Opens the (possibly inactive) panel of the current scene.</summary>
        public static void Open()
        {
            var instance = FindAnyObjectByType<SettingsPanel>(FindObjectsInactive.Include);
            if (instance == null)
            {
                ToastView.Show(Loc.T("hud.settings") + ": " + Loc.T("feature.locked"));
                return;
            }
            instance.panel.Show();
            instance.Refresh();
        }

        public void Hide() => panel.Hide();

        private void OnEnable()
        {
            GameEvents.LanguageChanged += Refresh;
            GraphicsSettingsService.Changed += OnQualityChanged;

            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (dimButton != null) dimButton.onClick.AddListener(Hide);
            if (languageButton != null) languageButton.onClick.AddListener(OnLanguage);
            if (saveButton != null) saveButton.onClick.AddListener(OnSave);
            for (int i = 0; i < qualityButtons.Length; i++)
            {
                if (qualityButtons[i] == null) continue;
                int index = i;
                qualityButtons[i].onClick.AddListener(() => OnQuality(index));
            }
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.LanguageChanged -= Refresh;
            GraphicsSettingsService.Changed -= OnQualityChanged;

            if (closeButton != null) closeButton.onClick.RemoveListener(Hide);
            if (dimButton != null) dimButton.onClick.RemoveListener(Hide);
            if (languageButton != null) languageButton.onClick.RemoveListener(OnLanguage);
            if (saveButton != null) saveButton.onClick.RemoveListener(OnSave);
            for (int i = 0; i < qualityButtons.Length; i++)
            {
                if (qualityButtons[i] != null) qualityButtons[i].onClick.RemoveAllListeners();
            }
        }

        private static void OnQuality(int index) => GraphicsSettingsService.Set((GraphicsQuality)index, true);

        private void OnQualityChanged(GraphicsQuality quality) => Refresh();

        private static void OnLanguage() => Game.SetLanguage(Loc.Current == Language.Turkish ? Language.English : Language.Turkish);

        private static void OnSave() => Game.Save(true);

        private void Refresh()
        {
            GraphicsQuality current = GraphicsSettingsService.Current;
            for (int i = 0; i < qualityButtons.Length; i++)
            {
                if (qualityButtons[i] == null || qualityButtons[i].targetGraphic == null) continue;
                qualityButtons[i].targetGraphic.color = i == (int)current ? UIColors.MilitaryGreen : UIColors.Panel;
            }

            if (infoText != null)
            {
                string description = Loc.T("gfx." + current.ToString().ToLowerInvariant() + ".desc");
                string target = string.Format(Loc.T("gfx.target"), GraphicsSettingsService.ActiveProfile.targetFrameRate);
                infoText.text = description + "\n<color=#9AA7B5>" + target + "</color>";
            }
        }
    }
}
