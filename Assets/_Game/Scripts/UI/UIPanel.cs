using System.Collections;
using UnityEngine;

namespace Deadzone.UI
{
    /// <summary>Animated show/hide (fade + slight scale) for popups. Hidden panels are deactivated.</summary>
    public sealed class UIPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform content;
        [SerializeField] private float duration = 0.18f;

        private const float ClosedScale = 0.92f;
        private Coroutine running;

        public bool IsOpen { get; private set; }

        public void Bind(CanvasGroup canvasGroup, RectTransform animatedContent)
        {
            group = canvasGroup;
            content = animatedContent;
        }

        public void Show()
        {
            IsOpen = true;
            gameObject.SetActive(true);
            Restart(true);
        }

        public void Hide()
        {
            IsOpen = false;
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
                return;
            }
            Restart(false);
        }

        private void Restart(bool opening)
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Animate(opening));
        }

        private IEnumerator Animate(bool opening)
        {
            float fromAlpha = group != null ? group.alpha : 1f;
            float toAlpha = opening ? 1f : 0f;
            if (opening && fromAlpha < 0.01f && content != null) content.localScale = Vector3.one * ClosedScale;
            float fromScale = content != null ? content.localScale.x : 1f;
            float toScale = opening ? 1f : ClosedScale;

            if (group != null) group.blocksRaycasts = opening;

            for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, time / duration);
                if (group != null) group.alpha = Mathf.Lerp(fromAlpha, toAlpha, k);
                if (content != null) content.localScale = Vector3.one * Mathf.Lerp(fromScale, toScale, k);
                yield return null;
            }

            if (group != null) group.alpha = toAlpha;
            if (content != null) content.localScale = Vector3.one * toScale;
            running = null;
            if (!opening) gameObject.SetActive(false);
        }
    }
}
