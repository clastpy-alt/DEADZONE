using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.UI
{
    public sealed class PopupPanelView : MonoBehaviour
    {
        [SerializeField] private UIPanel panel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button dimButton;

        public void Bind(UIPanel uiPanel, TMP_Text title, TMP_Text body, Button close, Button dim)
        {
            panel = uiPanel;
            titleText = title;
            bodyText = body;
            closeButton = close;
            dimButton = dim;
        }

        private void OnEnable()
        {
            closeButton.onClick.AddListener(Hide);
            dimButton.onClick.AddListener(Hide);
        }

        private void OnDisable()
        {
            closeButton.onClick.RemoveListener(Hide);
            dimButton.onClick.RemoveListener(Hide);
        }

        public void Show(string title, string body)
        {
            titleText.text = title;
            bodyText.text = body;
            panel.Show();
        }

        public void Hide() => panel.Hide();
    }
}
