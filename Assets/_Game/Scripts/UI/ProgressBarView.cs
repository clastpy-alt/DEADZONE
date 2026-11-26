using TMPro;
using UnityEngine;

namespace Deadzone.UI
{
    public sealed class ProgressBarView : MonoBehaviour
    {
        [SerializeField] private RectTransform fill;
        [SerializeField] private TMP_Text label;

        public void Bind(RectTransform fillRect, TMP_Text text)
        {
            fill = fillRect;
            label = text;
        }

        public void SetProgress(float value01)
        {
            value01 = Mathf.Clamp01(value01);
            if (fill != null) fill.anchorMax = new Vector2(value01, 1f);
            if (label != null) label.text = Mathf.RoundToInt(value01 * 100f) + "%";
        }

        public void SetProgress(long current, long max)
        {
            float value = max <= 0 ? 0f : Mathf.Clamp01((float)((double)current / max));
            if (fill != null) fill.anchorMax = new Vector2(value, 1f);
            if (label != null) label.text = $"{NumberFormatter.Compact(current)} / {NumberFormatter.Compact(max)}";
        }
    }
}
