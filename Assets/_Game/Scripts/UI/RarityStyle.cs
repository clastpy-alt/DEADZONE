using Deadzone.Data;
using UnityEngine;

namespace Deadzone.UI
{
    public static class RarityStyle
    {
        public static Color ColorOf(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Uncommon: return UIColors.Success;
                case Rarity.Rare: return UIColors.Info;
                case Rarity.Epic: return UIColors.Hex("#A66BDB");
                case Rarity.Legendary: return UIColors.Gold;
                case Rarity.Mythic: return UIColors.Danger;
                default: return UIColors.TextSecondary;
            }
        }

        /// <summary>Hero rank code used by the design: R / SR / SSR / UR.</summary>
        public static string HeroCode(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Epic: return "SR";
                case Rarity.Legendary: return "SSR";
                case Rarity.Mythic: return "UR";
                default: return "R";
            }
        }
    }
}
