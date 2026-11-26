using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deadzone.UI
{
    /// <summary>
    /// Scene-level hub for popups. Static helpers are null-safe: without an overlay in the scene
    /// they log a warning instead of throwing.
    /// </summary>
    public sealed class UIOverlay : MonoBehaviour
    {
        public static UIOverlay Instance { get; private set; }

        [SerializeField] private PopupPanelView popup;
        [SerializeField] private ConfirmationDialogView confirmation;
        [SerializeField] private RewardPopupView reward;
        [SerializeField] private TooltipView tooltip;
        [SerializeField] private LoadingScreenView loading;

        public void Bind(PopupPanelView popupView, ConfirmationDialogView confirmationView, RewardPopupView rewardView,
            TooltipView tooltipView, LoadingScreenView loadingView)
        {
            popup = popupView;
            confirmation = confirmationView;
            reward = rewardView;
            tooltip = tooltipView;
            loading = loadingView;
        }

        private void Awake() => Instance = this;

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static bool Exists => Instance != null;

        public static void ShowPopup(string title, string body)
        {
            if (Check()) Instance.popup.Show(title, body);
        }

        public static void Confirm(string title, string message, Action onConfirm, Action onCancel = null)
        {
            if (Check()) Instance.confirmation.Show(title, message, onConfirm, onCancel);
        }

        public static void ShowReward(string title, IEnumerable<string> lines, Action onClaim = null)
        {
            if (Check()) Instance.reward.Show(title, lines, onClaim);
        }

        public static void ShowTooltip(string text, Vector2 screenPosition)
        {
            if (Instance != null) Instance.tooltip.Show(text, screenPosition);
        }

        public static void HideTooltip()
        {
            if (Instance != null) Instance.tooltip.Hide();
        }

        public static void SetLoading(bool visible, float progress01 = 0f)
        {
            if (!Check()) return;
            if (visible) Instance.loading.Show(progress01);
            else Instance.loading.Hide();
        }

        private static bool Check()
        {
            if (Instance != null) return true;
            Debug.LogWarning("[UIOverlay] No UIOverlay in the scene. Add the UIOverlay prefab (Tools > DEADZONE > Generate UI Prefabs).");
            return false;
        }
    }
}
