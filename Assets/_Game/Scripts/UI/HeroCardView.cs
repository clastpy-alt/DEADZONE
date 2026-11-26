using Deadzone.Core;
using Deadzone.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deadzone.UI
{
    public sealed class HeroCardView : MonoBehaviour
    {
        public const int MaxStars = 6;

        [SerializeField] private Image frame;
        [SerializeField] private Image portrait;
        [SerializeField] private TMP_Text initialsText;
        [SerializeField] private TMP_Text rarityText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text classText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private Image[] starImages;

        public void Bind(Image frameImage, Image portraitImage, TMP_Text initials, TMP_Text rarity, TMP_Text name,
            TMP_Text heroClass, TMP_Text level, Image[] stars)
        {
            frame = frameImage;
            portrait = portraitImage;
            initialsText = initials;
            rarityText = rarity;
            nameText = name;
            classText = heroClass;
            levelText = level;
            starImages = stars;
        }

        public void Set(string heroName, string heroClass, int level, int stars, Rarity rarity)
        {
            Color color = RarityStyle.ColorOf(rarity);
            nameText.text = heroName;
            classText.text = heroClass;
            levelText.text = $"{Loc.T("hud.level")} {level}";
            rarityText.text = RarityStyle.HeroCode(rarity);
            rarityText.color = color;
            portrait.color = UIColors.WithAlpha(color, 0.35f);
            initialsText.text = MakeInitials(heroName);

            Outline outline = frame.GetComponent<Outline>();
            if (outline != null) outline.effectColor = color;

            stars = Mathf.Clamp(stars, 0, MaxStars);
            for (int i = 0; i < starImages.Length; i++)
            {
                starImages[i].color = i < stars ? UIColors.Gold : UIColors.WithAlpha(UIColors.TextSecondary, 0.35f);
            }
        }

        private static string MakeInitials(string heroName)
        {
            if (string.IsNullOrWhiteSpace(heroName)) return "?";
            string[] parts = heroName.Split(' ');
            return parts.Length > 1 ? $"{parts[0][0]}{parts[parts.Length - 1][0]}" : parts[0].Substring(0, 1);
        }
    }
}
