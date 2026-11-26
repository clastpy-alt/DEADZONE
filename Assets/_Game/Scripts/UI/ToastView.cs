using TMPro;
using UnityEngine;

namespace Deadzone.UI
{
    /// <summary>Short non-blocking message ("locked", "saved"). Show() is a no-op if no toast exists.</summary>
    public sealed class ToastView : MonoBehaviour
    {
        private static ToastView instance;

        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text label;
        [SerializeField] private float visibleSeconds = 1.8f;

        private float timer;

        public void Bind(CanvasGroup canvasGroup, TMP_Text text)
        {
            group = canvasGroup;
            label = text;
        }

        public static void Show(string message)
        {
            if (instance != null) instance.Display(message);
        }

        private void Awake()
        {
            instance = this;
            if (group != null) group.alpha = 0f;
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void Display(string message)
        {
            if (label != null) label.text = message;
            timer = visibleSeconds;
            if (group != null) group.alpha = 1f;
        }

        private void Update()
        {
            if (timer <= 0f) return;

            timer -= Time.unscaledDeltaTime;
            if (group != null) group.alpha = Mathf.Clamp01(timer / 0.4f);
        }
    }
}
