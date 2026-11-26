using Deadzone.Core;
using TMPro;
using UnityEngine;

namespace Deadzone.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string key;
        private TMP_Text label;

        public void SetKey(string newKey)
        {
            key = newKey;
            Refresh();
        }

        private void OnEnable()
        {
            GameEvents.LanguageChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.LanguageChanged -= Refresh;
        }

        public void Refresh()
        {
            if (label == null) label = GetComponent<TMP_Text>();
            if (label != null && !string.IsNullOrEmpty(key)) label.text = Loc.T(key);
        }
    }
}
