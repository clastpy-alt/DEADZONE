using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.UI
{
    public sealed class ConfirmationDialogView : MonoBehaviour
    {
        [SerializeField] private UIPanel panel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private Action onConfirm;
        private Action onCancel;

        public void Bind(UIPanel uiPanel, TMP_Text title, TMP_Text message, Button confirm, Button cancel)
        {
            panel = uiPanel;
            titleText = title;
            messageText = message;
            confirmButton = confirm;
            cancelButton = cancel;
        }

        private void OnEnable()
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
            cancelButton.onClick.AddListener(OnCancelClicked);
        }

        private void OnDisable()
        {
            confirmButton.onClick.RemoveListener(OnConfirmClicked);
            cancelButton.onClick.RemoveListener(OnCancelClicked);
        }

        public void Show(string title, string message, Action confirmed, Action cancelled = null)
        {
            titleText.text = title;
            messageText.text = message;
            onConfirm = confirmed;
            onCancel = cancelled;
            panel.Show();
        }

        private void OnConfirmClicked() => Close(true);
        private void OnCancelClicked() => Close(false);

        private void Close(bool confirmed)
        {
            Action callback = confirmed ? onConfirm : onCancel;
            onConfirm = null;
            onCancel = null;
            panel.Hide();
            callback?.Invoke();
        }
    }
}
