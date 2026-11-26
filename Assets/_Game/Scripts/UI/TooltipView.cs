using TMPro;
using UnityEngine;

namespace Deadzone.UI
{
    public sealed class TooltipView : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;
        [SerializeField] private TMP_Text label;

        public void Bind(RectTransform root, TMP_Text text)
        {
            rect = root;
            label = text;
        }

        /// <summary>screenPosition is in pixels; works for a Screen Space Overlay canvas.</summary>
        public void Show(string text, Vector2 screenPosition)
        {
            label.text = text;
            gameObject.SetActive(true);

            Canvas canvas = GetComponentInParent<Canvas>();
            float scale = canvas != null ? canvas.scaleFactor : 1f;
            float halfWidth = rect.rect.width * scale * 0.5f;
            float height = rect.rect.height * scale;

            float x = Mathf.Clamp(screenPosition.x, halfWidth, Mathf.Max(halfWidth, Screen.width - halfWidth));
            float y = Mathf.Clamp(screenPosition.y + 70f * scale, 0f, Mathf.Max(0f, Screen.height - height));
            rect.position = new Vector3(x, y, 0f);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
