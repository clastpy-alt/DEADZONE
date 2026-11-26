using TMPro;
using UnityEngine;

namespace Deadzone.UI
{
    public sealed class LoadingScreenView : MonoBehaviour
    {
        [SerializeField] private UIPanel panel;
        [SerializeField] private ProgressBarView progressBar;
        [SerializeField] private TMP_Text tipText;

        public void Bind(UIPanel uiPanel, ProgressBarView bar, TMP_Text tip)
        {
            panel = uiPanel;
            progressBar = bar;
            tipText = tip;
        }

        public void Show(float progress01 = 0f)
        {
            progressBar.SetProgress(progress01);
            panel.Show();
        }

        public void SetProgress(float progress01) => progressBar.SetProgress(progress01);
        public void Hide() => panel.Hide();
    }
}
