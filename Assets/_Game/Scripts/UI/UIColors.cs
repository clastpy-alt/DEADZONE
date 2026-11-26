using UnityEngine;

namespace Deadzone.UI
{
    /// <summary>Palette from the art direction (UI_DESIGN_SYSTEM.md).</summary>
    public static class UIColors
    {
        public static readonly Color Background = Hex("#101419");
        public static readonly Color Panel = Hex("#1C232B");
        public static readonly Color MilitaryGreen = Hex("#465744");
        public static readonly Color Gold = Hex("#D4AF63");
        public static readonly Color Silver = Hex("#ADB8C4");
        public static readonly Color Danger = Hex("#D94B4B");
        public static readonly Color Success = Hex("#4CAF72");
        public static readonly Color Info = Hex("#4C9FE8");
        public static readonly Color TextPrimary = Hex("#F2F4F6");
        public static readonly Color TextSecondary = Hex("#8C97A3");

        public static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out Color color) ? color : Color.magenta;
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
