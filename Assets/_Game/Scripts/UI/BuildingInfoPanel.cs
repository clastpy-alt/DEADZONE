using Deadzone.Buildings;
using Deadzone.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.UI
{
    /// <summary>Shows the selected building. Upgrading is a Phase 02 feature, so the button reports "locked".</summary>
    public sealed class BuildingInfoPanel : MonoBehaviour
    {
        [SerializeField] private GameObject content;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private Button upgradeButton;

        private BuildingSelectable current;

        public void Bind(GameObject contentRoot, TMP_Text name, TMP_Text level, Button upgrade)
        {
            content = contentRoot;
            nameText = name;
            levelText = level;
            upgradeButton = upgrade;
        }

        private void OnEnable()
        {
            GameEvents.BuildingSelected += OnBuildingSelected;
            GameEvents.LanguageChanged += Refresh;
            if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeClicked);
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.BuildingSelected -= OnBuildingSelected;
            GameEvents.LanguageChanged -= Refresh;
            if (upgradeButton != null) upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
        }

        private void OnBuildingSelected(BuildingSelectable building)
        {
            current = building;
            Refresh();
        }

        private void OnUpgradeClicked()
        {
            ToastView.Show(Loc.T("building.upgrade") + ": " + Loc.T("feature.locked"));
        }

        private void Refresh()
        {
            bool visible = current != null;
            if (content != null) content.SetActive(visible);
            if (!visible) return;

            if (nameText != null) nameText.text = current.DisplayName;
            if (levelText != null) levelText.text = $"{Loc.T("building.level")} {current.Level}/{current.MaxLevel}";
        }
    }
}
