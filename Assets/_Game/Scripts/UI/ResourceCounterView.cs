using Deadzone.Core;
using Deadzone.Data;
using TMPro;
using UnityEngine;

namespace Deadzone.UI
{
    public sealed class ResourceCounterView : MonoBehaviour
    {
        [SerializeField] private ResourceType type;
        [SerializeField] private TMP_Text valueText;

        public void Configure(ResourceType resourceType, TMP_Text text)
        {
            type = resourceType;
            valueText = text;
        }

        public void SetType(ResourceType resourceType)
        {
            type = resourceType;
        }

        private void OnEnable()
        {
            Game.EnsureInitialized();
            GameEvents.ResourceChanged += OnResourceChanged;

            TooltipTrigger tooltip = GetComponent<TooltipTrigger>();
            if (tooltip != null) tooltip.SetProvider(BuildTooltip);

            Refresh();
        }

        private void OnDisable()
        {
            GameEvents.ResourceChanged -= OnResourceChanged;
        }

        private void OnResourceChanged(ResourceType changed, long amount)
        {
            if (changed == type) Refresh();
        }

        private string BuildTooltip()
        {
            string name = Loc.T("res." + type.ToString().ToLowerInvariant());
            return $"{name}: {Game.Wallet.Get(type):N0} / {Game.Wallet.GetCapacity(type):N0}";
        }

        private void Refresh()
        {
            if (valueText != null && Game.Wallet != null)
            {
                valueText.text = NumberFormatter.Compact(Game.Wallet.Get(type));
            }
        }
    }
}
