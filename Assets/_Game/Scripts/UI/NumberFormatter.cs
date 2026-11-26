using System;
using System.Globalization;

namespace Deadzone.UI
{
    public static class NumberFormatter
    {
        /// <summary>TimeSpan -> "2d 03h" (>= 1 day) or "03:25:10". Negative values become zero.</summary>
        public static string Duration(TimeSpan span)
        {
            if (span < TimeSpan.Zero) span = TimeSpan.Zero;
            if (span.TotalDays >= 1d)
                return string.Format(CultureInfo.InvariantCulture, "{0}d {1:00}h", (int)span.TotalDays, span.Hours);
            return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}", (int)span.TotalHours, span.Minutes, span.Seconds);
        }

        /// <summary>1234 -> "1.2K", 1500000 -> "1.5M". Truncates, never rounds up.</summary>
        public static string Compact(long value)
        {
            if (value < 0) return "-" + Compact(-value);
            if (value < 1000) return value.ToString(CultureInfo.InvariantCulture);

            double scaled;
            string suffix;
            if (value >= 1000000000L) { scaled = value / 1e9; suffix = "B"; }
            else if (value >= 1000000L) { scaled = value / 1e6; suffix = "M"; }
            else { scaled = value / 1e3; suffix = "K"; }

            scaled = Math.Floor(scaled * 10.0) / 10.0;
            return scaled.ToString("0.#", CultureInfo.InvariantCulture) + suffix;
        }
    }
}
