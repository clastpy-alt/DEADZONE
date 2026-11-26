using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.UI
{
    public sealed class RewardPopupView : MonoBehaviour
    {
        [SerializeField] private UIPanel panel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text linesText;
        [SerializeField] private Button claimButton;

        private Action onClaim;

        public void Bind(UIPanel uiPanel, TMP_Text title, TMP_Text lines, Button claim)
        {
            panel = uiPanel;
            titleText = title;
            linesText = lines;
            claimButton = claim;
        }

        private void OnEnable() => claimButton.onClick.AddListener(OnClaimClicked);
        private void OnDisable() => claimButton.onClick.RemoveListener(OnClaimClicked);

        public void Show(string title, IEnumerable<string> lines, Action claimed = null)
        {
            titleText.text = title;
            linesText.text = string.Join("\n", lines);
            onClaim = claimed;
            panel.Show();
        }

        private void OnClaimClicked()
        {
            Action callback = onClaim;
            onClaim = null;
            panel.Hide();
            callback?.Invoke();
        }
    }
}
