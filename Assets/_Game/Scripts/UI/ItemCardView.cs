using Deadzone.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.UI
{
    public sealed class ItemCardView : MonoBehaviour
    {
        [SerializeField] private Image frame;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text iconCode;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text quantityText;

        public void Bind(Image frameImage, Image iconImage, TMP_Text code, TMP_Text name, TMP_Text quantity)
        {
            frame = frameImage;
            icon = iconImage;
            iconCode = code;
            nameText = name;
            quantityText = quantity;
        }

        public void Set(string itemName, string code, long quantity, Rarity rarity, Color iconColor)
        {
            nameText.text = itemName;
            iconCode.text = code;
            icon.color = iconColor;
            quantityText.text = "x" + NumberFormatter.Compact(quantity);
            Outline outline = frame.GetComponent<Outline>();
            if (outline != null) outline.effectColor = RarityStyle.ColorOf(rarity);
        }
    }
}
