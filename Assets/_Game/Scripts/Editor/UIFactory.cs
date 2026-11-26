using Deadzone.Core;
using Deadzone.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.EditorTools
{
    internal static class UIFactory
    {
        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void Place(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        /// <summary>Divides the parent into 'count' equal horizontal slots and fills slot 'index'.</summary>
        public static void Slot(RectTransform rect, int index, int count, float padX, float padY)
        {
            rect.anchorMin = new Vector2(index / (float)count, 0f);
            rect.anchorMax = new Vector2((index + 1) / (float)count, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(padX, padY);
            rect.offsetMax = new Vector2(-padX, -padY);
        }

        public static Image Panel(string name, Transform parent, Color color, bool raycastTarget = true)
        {
            RectTransform rect = Rect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        public static TextMeshProUGUI Text(string name, Transform parent, string text, float size, Color color,
            TextAlignmentOptions alignment, FontStyles style = FontStyles.Normal)
        {
            RectTransform rect = Rect(name, parent);
            var tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.fontStyle = style;
            tmp.raycastTarget = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return tmp;
        }

        public static void AutoSize(TextMeshProUGUI text, float min, float max)
        {
            text.enableAutoSizing = true;
            text.fontSizeMin = min;
            text.fontSizeMax = max;
        }

        public static void AddOutline(Graphic graphic, Color color, float distance = 3f)
        {
            var outline = graphic.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, -distance);
        }

        public static Button CreateButton(string name, Transform parent, Color fill, Color border,
            string labelKey, float maxFontSize, out TextMeshProUGUI label)
        {
            Image image = Panel(name, parent, fill);
            AddOutline(image, border, 3f);

            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            button.colors = colors;
            image.gameObject.AddComponent<ButtonPressAnimator>();

            label = Text("Label", image.transform, Loc.T(labelKey), maxFontSize, UIColors.TextPrimary,
                TextAlignmentOptions.Center, FontStyles.Bold);
            Stretch(label.rectTransform);
            label.margin = new Vector4(8f, 4f, 8f, 4f);
            AutoSize(label, 14f, maxFontSize);
            label.gameObject.AddComponent<LocalizedText>().SetKey(labelKey);
            return button;
        }

        /// <summary>Thin light stripe along the top edge: cheap "metal bevel" until real art exists.</summary>
        public static void Bevel(Transform parent)
        {
            Image stripe = Panel("Bevel", parent, UIColors.WithAlpha(UIColors.Silver, 0.22f), false);
            Place(stripe.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -3), new Vector2(-8, 3));
        }

        public static Image Window(string name, Transform parent, Vector2 size)
        {
            Image window = Panel(name, parent, UIColors.Panel);
            Place(window.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
            AddOutline(window, UIColors.Gold, 3f);
            Bevel(window.transform);
            return window;
        }
    }
}
