using UnityEngine;
using UnityEngine.EventSystems;

namespace Deadzone.UI
{
    /// <summary>Scales a button down while pressed (unscaled time, so it works while paused).</summary>
    public sealed class ButtonPressAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private float pressedScale = 0.94f;
        [SerializeField] private float speed = 18f;

        private Vector3 rest = Vector3.one;
        private bool pressed;

        private void Awake() => rest = transform.localScale;

        private void OnDisable()
        {
            pressed = false;
            transform.localScale = rest;
        }

        private void Update()
        {
            Vector3 target = pressed ? rest * pressedScale : rest;
            if ((transform.localScale - target).sqrMagnitude < 0.000001f) return;

            float t = 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, target, t);
        }

        public void OnPointerDown(PointerEventData eventData) => pressed = true;
        public void OnPointerUp(PointerEventData eventData) => pressed = false;
        public void OnPointerExit(PointerEventData eventData) => pressed = false;
    }
}
