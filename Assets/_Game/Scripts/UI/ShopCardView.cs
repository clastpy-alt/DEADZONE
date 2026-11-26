using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.UI
{
    /// <summary>Display-only product card. No real purchase logic exists yet (PHASE 07, mock provider first).</summary>
    public sealed class ShopCardView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text iconCode;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private Button buyButton;
        [SerializeField] private CanvasGroup group;

        public void Bind(Image iconImage, TMP_Text code, TMP_Text name, TMP_Text price, Button buy, CanvasGroup canvasGroup)
        {
            icon = iconImage;
            iconCode = code;
            nameText = name;
            priceText = price;
            buyButton = buy;
            group = canvasGroup;
        }

        public void Set(string productName, string code, string price, bool locked, Action onBuy)
        {
            nameText.text = productName;
            iconCode.text = code;
            priceText.text = price;
            group.alpha = locked ? 0.55f : 1f;
            buyButton.interactable = !locked;
            buyButton.onClick.RemoveAllListeners();
            if (onBuy != null) buyButton.onClick.AddListener(() => onBuy());
        }
    }
}
