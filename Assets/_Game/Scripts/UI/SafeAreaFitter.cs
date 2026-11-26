using UnityEngine;

namespace Deadzone.UI
{
    /// <summary>Fits this RectTransform to Screen.safeArea (notches, rounded corners).</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void OnEnable() => Apply(true);
        private void Update() => Apply(false);

        private void Apply(bool force)
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;

            Rect safe = Screen.safeArea;
            var size = new Vector2Int(Screen.width, Screen.height);
            if (!force && safe == lastSafeArea && size == lastScreenSize) return;

            lastSafeArea = safe;
            lastScreenSize = size;

            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

            Vector2 min = safe.position;
            Vector2 max = safe.position + safe.size;
            min.x /= size.x; min.y /= size.y;
            max.x /= size.x; max.y /= size.y;

            rectTransform.anchorMin = min;
            rectTransform.anchorMax = max;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
