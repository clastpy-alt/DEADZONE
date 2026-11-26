using System;
using Deadzone.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deadzone.UI
{
    /// <summary>Shows a tooltip on mouse hover or touch hold. Text comes from a localization key, raw text or a provider.</summary>
    public sealed class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private string textKey;
        [SerializeField] private float delaySeconds = 0.35f;

        private Func<string> provider;
        private bool pointerInside;
        private bool visible;
        private float timer;
        private Vector2 lastPosition;

        public void SetProvider(Func<string> textProvider) => provider = textProvider;

        private string ResolveText()
        {
            if (provider != null) return provider();
            return string.IsNullOrEmpty(textKey) ? string.Empty : Loc.T(textKey);
        }

        public void OnPointerEnter(PointerEventData eventData) { pointerInside = true; timer = 0f; lastPosition = eventData.position; }
        public void OnPointerDown(PointerEventData eventData) { pointerInside = true; timer = 0f; lastPosition = eventData.position; }
        public void OnPointerExit(PointerEventData eventData) => Hide();
        public void OnPointerUp(PointerEventData eventData) => Hide();
        private void OnDisable() => Hide();

        private void Update()
        {
            if (!pointerInside || visible) return;

            timer += Time.unscaledDeltaTime;
            if (timer < delaySeconds) return;

            string text = ResolveText();
            if (string.IsNullOrEmpty(text)) return;

            visible = true;
            UIOverlay.ShowTooltip(text, lastPosition);
        }

        private void Hide()
        {
            pointerInside = false;
            if (visible) UIOverlay.HideTooltip();
            visible = false;
        }
    }
}
