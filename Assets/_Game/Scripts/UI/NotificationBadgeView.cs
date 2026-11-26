using TMPro;
using UnityEngine;

namespace Deadzone.UI
{
    /// <summary>Red counter bubble. Hidden at 0, "99+" above 99.</summary>
    public sealed class NotificationBadgeView : MonoBehaviour
    {
        [SerializeField] private TMP_Text countText;

        public void Bind(TMP_Text text) => countText = text;

        public void SetCount(int count)
        {
            gameObject.SetActive(count > 0);
            if (countText != null) countText.text = count > 99 ? "99+" : Mathf.Max(0, count).ToString();
        }
    }
}
