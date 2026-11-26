using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.UI
{
    /// <summary>HUD button bound to a feature. Shows a lock overlay while the feature is unavailable.</summary>
    [RequireComponent(typeof(Button))]
    public sealed class NavButton : MonoBehaviour
    {
        [SerializeField] private HudFeature feature;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private CanvasGroup canvasGroup;

        public void Configure(HudFeature newFeature, GameObject overlay, CanvasGroup group)
        {
            feature = newFeature;
            lockOverlay = overlay;
            canvasGroup = group;
            Refresh();
        }

        public void SetFeature(HudFeature newFeature)
        {
            feature = newFeature;
            Refresh();
        }

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClicked);
            Refresh();
        }

        private void OnClicked() => FeatureRouter.Open(feature);

        private void Refresh()
        {
            bool available = FeatureRouter.IsAvailable(feature);
            if (lockOverlay != null) lockOverlay.SetActive(!available);
            if (canvasGroup != null) canvasGroup.alpha = available ? 1f : 0.7f;
        }
    }
}
