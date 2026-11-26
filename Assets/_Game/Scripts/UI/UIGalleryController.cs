using System.Collections;
using Deadzone.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.UI
{
    /// <summary>Developer-only test scene controller: opens every popup type so the components can be checked visually.</summary>
    public sealed class UIGalleryController : MonoBehaviour
    {
        [SerializeField] private Button popupButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button rewardButton;
        [SerializeField] private Button loadingButton;
        [SerializeField] private Button toastButton;

        public void Bind(Button popup, Button confirm, Button reward, Button loading, Button toast)
        {
            popupButton = popup;
            confirmButton = confirm;
            rewardButton = reward;
            loadingButton = loading;
            toastButton = toast;
        }

        private void OnEnable()
        {
            Game.EnsureInitialized();
            popupButton.onClick.AddListener(OnPopup);
            confirmButton.onClick.AddListener(OnConfirm);
            rewardButton.onClick.AddListener(OnReward);
            loadingButton.onClick.AddListener(OnLoading);
            toastButton.onClick.AddListener(OnToast);
        }

        private void OnDisable()
        {
            popupButton.onClick.RemoveListener(OnPopup);
            confirmButton.onClick.RemoveListener(OnConfirm);
            rewardButton.onClick.RemoveListener(OnReward);
            loadingButton.onClick.RemoveListener(OnLoading);
            toastButton.onClick.RemoveListener(OnToast);
        }

        private void OnPopup() => UIOverlay.ShowPopup("PopupPanel", "Sample popup body.\nSecond line of text.");
        private void OnConfirm() => UIOverlay.Confirm(Loc.T("menu.newgame"), Loc.T("newgame.confirm.message"),
            () => ToastView.Show("Confirmed"), () => ToastView.Show("Cancelled"));
        private void OnReward() => UIOverlay.ShowReward("Reward", new[] { "Food x 500", "Steel x 200", "Diamonds x 10" });
        private void OnToast() => ToastView.Show("Toast sample");
        private void OnLoading() => StartCoroutine(LoadingDemo());

        private IEnumerator LoadingDemo()
        {
            UIOverlay.SetLoading(true, 0f);
            for (float t = 0f; t < 2f; t += Time.unscaledDeltaTime)
            {
                UIOverlay.SetLoading(true, t / 2f);
                yield return null;
            }
            UIOverlay.SetLoading(false);
        }
    }
}
