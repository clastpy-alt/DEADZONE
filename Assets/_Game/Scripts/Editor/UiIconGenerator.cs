using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Deadzone.EditorTools
{
    /// <summary>
    /// Procedural HUD icon set (PHASE 02.5). Every icon is drawn from signed-distance shapes into a 128x128 PNG under
    /// Resources/Icons, so no external art or tool is needed. A file that already exists is never overwritten (hand-made
    /// replacements survive); pass overwrite to regenerate. Runtime lookup: UiIcons.Get(name).
    /// </summary>
    internal static class UiIconGenerator
    {
        public const string Folder = EditorPaths.Resources + "/Icons";
        private const int Size = 128;
        private const float Unit = 100f;
        private const float PxUnit = Unit / Size;

        private delegate float Sdf(float x, float y);

        // ------------------------------------------------------------------ shapes (unit space 0..100, y up)

        private static class S
        {
            public static Sdf Circle(float cx, float cy, float r) =>
                (x, y) => Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) - r;

            public static Sdf Ring(float cx, float cy, float r, float thickness) =>
                (x, y) => Mathf.Abs(Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) - r) - thickness * 0.5f;

            public static Sdf Box(float cx, float cy, float hw, float hh, float round = 0f, float rotDeg = 0f)
            {
                float a = -rotDeg * Mathf.Deg2Rad;
                float c = Mathf.Cos(a);
                float s = Mathf.Sin(a);
                return (x, y) =>
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float px = dx * c - dy * s;
                    float py = dx * s + dy * c;
                    float qx = Mathf.Abs(px) - (hw - round);
                    float qy = Mathf.Abs(py) - (hh - round);
                    float ox = Mathf.Max(qx, 0f);
                    float oy = Mathf.Max(qy, 0f);
                    return Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(qx, qy), 0f) - round;
                };
            }

            public static Sdf Seg(float ax, float ay, float bx, float by, float r) => (x, y) =>
            {
                float pax = x - ax, pay = y - ay, bax = bx - ax, bay = by - ay;
                float h = Mathf.Clamp01((pax * bax + pay * bay) / (bax * bax + bay * bay));
                float dx = pax - bax * h, dy = pay - bay * h;
                return Mathf.Sqrt(dx * dx + dy * dy) - r;
            };

            public static Sdf Poly(params float[] p)
            {
                int n = p.Length / 2;
                return (x, y) =>
                {
                    float d = (x - p[0]) * (x - p[0]) + (y - p[1]) * (y - p[1]);
                    float sign = 1f;
                    for (int i = 0, j = n - 1; i < n; j = i, i++)
                    {
                        float ex = p[j * 2] - p[i * 2], ey = p[j * 2 + 1] - p[i * 2 + 1];
                        float wx = x - p[i * 2], wy = y - p[i * 2 + 1];
                        float t = Mathf.Clamp01((wx * ex + wy * ey) / (ex * ex + ey * ey));
                        float bx = wx - ex * t, by = wy - ey * t;
                        d = Mathf.Min(d, bx * bx + by * by);
                        bool c1 = y >= p[i * 2 + 1];
                        bool c2 = y < p[j * 2 + 1];
                        bool c3 = ex * wy > ey * wx;
                        if ((c1 && c2 && c3) || (!c1 && !c2 && !c3)) sign = -sign;
                    }
                    return sign * Mathf.Sqrt(d);
                };
            }

            public static float[] StarPoints(float cx, float cy, float outer, float inner, int points)
            {
                var result = new float[points * 4];
                for (int i = 0; i < points * 2; i++)
                {
                    float angle = (90f + i * 180f / points) * Mathf.Deg2Rad;
                    float radius = i % 2 == 0 ? outer : inner;
                    result[i * 2] = cx + Mathf.Cos(angle) * radius;
                    result[i * 2 + 1] = cy + Mathf.Sin(angle) * radius;
                }
                return result;
            }

            public static Sdf Star(float cx, float cy, float outer, float inner, int points) =>
                Poly(StarPoints(cx, cy, outer, inner, points));

            /// <summary>Heater shield, pointed at the bottom.</summary>
            public static Sdf Shield(float cx, float cy, float scale) => Poly(
                cx - 36f * scale, cy + 44f * scale,
                cx + 36f * scale, cy + 44f * scale,
                cx + 36f * scale, cy + 4f * scale,
                cx, cy - 46f * scale,
                cx - 36f * scale, cy + 4f * scale);

            public static Sdf Union(Sdf a, Sdf b) => (x, y) => Mathf.Min(a(x, y), b(x, y));
            public static Sdf Intersect(Sdf a, Sdf b) => (x, y) => Mathf.Max(a(x, y), b(x, y));
            public static Sdf Subtract(Sdf a, Sdf b) => (x, y) => Mathf.Max(a(x, y), -b(x, y));
            public static Sdf Above(float limit) => (x, y) => limit - y;
            public static Sdf Below(float limit) => (x, y) => y - limit;
        }

        // ------------------------------------------------------------------ raster

        private sealed class Art
        {
            public readonly Color[] Px = new Color[Size * Size];

            public void Fill(Sdf sdf, Color top, Color bottom)
            {
                for (int j = 0; j < Size; j++)
                {
                    float y = (j + 0.5f) * PxUnit;
                    for (int i = 0; i < Size; i++)
                    {
                        float coverage = Mathf.Clamp01(0.5f - sdf((i + 0.5f) * PxUnit, y) / PxUnit);
                        if (coverage <= 0f) continue;

                        Color src = Color.Lerp(bottom, top, y / Unit);
                        float a = coverage * src.a;
                        Color dst = Px[j * Size + i];
                        float outA = a + dst.a * (1f - a);
                        if (outA <= 0f) continue;
                        float keep = dst.a * (1f - a);
                        Px[j * Size + i] = new Color(
                            (src.r * a + dst.r * keep) / outA,
                            (src.g * a + dst.g * keep) / outA,
                            (src.b * a + dst.b * keep) / outA,
                            outA);
                    }
                }
            }

            public void Fill(Sdf sdf, Color color) => Fill(sdf, color, color);

            /// <summary>Dark soft edge behind the silhouette so the icon reads on any background.</summary>
            public void Rim(Color rim, int radius)
            {
                var source = (Color[])Px.Clone();
                for (int j = 0; j < Size; j++)
                {
                    for (int i = 0; i < Size; i++)
                    {
                        float best = 0f;
                        for (int dy = -radius; dy <= radius; dy++)
                        {
                            int nj = j + dy;
                            if (nj < 0 || nj >= Size) continue;
                            for (int dx = -radius; dx <= radius; dx++)
                            {
                                int ni = i + dx;
                                if (ni < 0 || ni >= Size) continue;
                                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                                if (distance > radius) continue;
                                best = Mathf.Max(best, source[nj * Size + ni].a * (1f - distance / (radius + 0.5f)));
                            }
                        }

                        Color s = source[j * Size + i];
                        float ra = rim.a * Mathf.Clamp01(best * 1.6f);
                        float outA = s.a + ra * (1f - s.a);
                        if (outA <= 0f) continue;
                        float under = ra * (1f - s.a);
                        Px[j * Size + i] = new Color(
                            (s.r * s.a + rim.r * under) / outA,
                            (s.g * s.a + rim.g * under) / outA,
                            (s.b * s.a + rim.b * under) / outA,
                            outA);
                    }
                }
            }
        }

        private static Color C(string hex, float alpha = 1f)
        {
            ColorUtility.TryParseHtmlString(hex, out Color color);
            color.a = alpha;
            return color;
        }

        // ------------------------------------------------------------------ icon definitions

        private static Dictionary<string, Action<Art>> Definitions()
        {
            return new Dictionary<string, Action<Art>>
            {
                ["res_food"] = a =>
                {
                    a.Fill(S.Box(50, 46, 26, 30, 6), C("#A9D36A"), C("#5E8F2E"));
                    a.Fill(S.Box(50, 46, 26, 11), C("#F4E9BC"), C("#D9C98E"));
                    a.Fill(S.Star(50, 46, 8, 3.5f, 5), C("#5E8F2E"));
                    a.Fill(S.Box(50, 77, 27, 6, 3), C("#E3F0BF"), C("#B7D184"));
                    a.Fill(S.Ring(50, 77, 6, 2.4f), C("#6F8F3C"));
                },
                ["res_wood"] = a =>
                {
                    a.Fill(S.Box(44, 34, 34, 13, 12), C("#9B6A38"), C("#6B4220"));
                    a.Fill(S.Box(44, 62, 34, 13, 12), C("#A97745"), C("#744824"));
                    a.Fill(S.Circle(78, 34, 13), C("#E3B777"), C("#C99452"));
                    a.Fill(S.Ring(78, 34, 7, 2.2f), C("#8A5A2B"));
                    a.Fill(S.Circle(78, 62, 13), C("#EBC184"), C("#CFA05E"));
                    a.Fill(S.Ring(78, 62, 7, 2.2f), C("#8A5A2B"));
                    a.Fill(S.Seg(18, 30, 52, 30, 1.4f), C("#4A2D14", 0.5f));
                },
                ["res_steel"] = a =>
                {
                    a.Fill(S.Poly(12, 26, 88, 26, 76, 54, 24, 54), C("#B9C4D0"), C("#6E7C8C"));
                    a.Fill(S.Poly(24, 54, 76, 54, 66, 70, 34, 70), C("#EAF0F6"), C("#C4CFDA"));
                    a.Fill(S.Seg(20, 31, 80, 31, 1.5f), C("#FFFFFF", 0.35f));
                    a.Fill(S.Poly(30, 76, 70, 76, 62, 88, 38, 88), C("#D8E0E8"), C("#9AA7B5"));
                },
                ["res_fuel"] = a =>
                {
                    Sdf drop = S.Union(S.Circle(50, 34, 26), S.Poly(50, 94, 26, 44, 74, 44));
                    a.Fill(drop, C("#FFB347"), C("#D4631C"));
                    Sdf shine = S.Union(S.Circle(50, 32, 13), S.Poly(50, 72, 38, 40, 62, 40));
                    a.Fill(shine, C("#FFE6A8"), C("#FFC25A"));
                },
                ["res_gold"] = a =>
                {
                    a.Fill(S.Circle(50, 50, 40), C("#FFE49A"), C("#C99A2E"));
                    a.Fill(S.Ring(50, 50, 30, 4), C("#8A6414", 0.85f));
                    a.Fill(S.Poly(50, 72, 62, 50, 50, 28, 38, 50), C("#8A6414", 0.85f));
                },
                ["res_diamond"] = a =>
                {
                    a.Fill(S.Poly(12, 58, 88, 58, 50, 14), C("#4FA0F0"), C("#1F5FA8"));
                    a.Fill(S.Poly(28, 76, 72, 76, 88, 58, 12, 58), C("#B6E2FF"), C("#6EBBFF"));
                    a.Fill(S.Seg(38, 58, 50, 14, 0.9f), C("#FFFFFF", 0.4f));
                    a.Fill(S.Seg(62, 58, 50, 14, 0.9f), C("#FFFFFF", 0.4f));
                    a.Fill(S.Seg(28, 76, 38, 58, 0.9f), C("#FFFFFF", 0.5f));
                    a.Fill(S.Seg(72, 76, 62, 58, 0.9f), C("#FFFFFF", 0.5f));
                },
                ["hud_power"] = a =>
                {
                    a.Fill(S.Poly(60, 94, 22, 46, 44, 46, 38, 6, 80, 58, 56, 58, 72, 94), C("#FFE98A"), C("#F29A1E"));
                },
                ["hud_vip"] = a =>
                {
                    a.Fill(S.Poly(14, 26, 86, 26, 92, 70, 70, 50, 50, 80, 30, 50, 8, 70), C("#FFE49A"), C("#C99A2E"));
                    a.Fill(S.Box(50, 31, 36, 5, 2), C("#B07F1A"));
                    a.Fill(S.Circle(8, 72, 6), C("#FFF6D0"));
                    a.Fill(S.Circle(92, 72, 6), C("#FFF6D0"));
                    a.Fill(S.Circle(50, 82, 6.5f), C("#FFF6D0"));
                },
                ["hud_gear"] = a =>
                {
                    Sdf body = S.Circle(50, 50, 30);
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = i * 45f;
                        float rad = angle * Mathf.Deg2Rad;
                        body = S.Union(body, S.Box(50 + Mathf.Cos(rad) * 33, 50 + Mathf.Sin(rad) * 33, 10, 7.5f, 2, angle));
                    }
                    a.Fill(S.Subtract(body, S.Circle(50, 50, 12)), C("#E1E8F0"), C("#8794A4"));
                    a.Fill(S.Ring(50, 50, 12, 2.5f), C("#5A6470", 0.8f));
                },
                ["hud_lock"] = a =>
                {
                    a.Fill(S.Intersect(S.Ring(50, 56, 15, 8), S.Above(54)), C("#E1E8F0"), C("#8794A4"));
                    a.Fill(S.Seg(35, 56, 35, 40, 4), C("#B9C4D0"));
                    a.Fill(S.Seg(65, 56, 65, 40, 4), C("#B9C4D0"));
                    a.Fill(S.Box(50, 34, 29, 23, 6), C("#F2CE6B"), C("#B98A26"));
                    a.Fill(S.Circle(50, 38, 5.5f), C("#5A4210"));
                    a.Fill(S.Box(50, 29, 2.8f, 7), C("#5A4210"));
                },
                ["hud_clock"] = a =>
                {
                    a.Fill(S.Circle(50, 50, 42), C("#F4F7FA"), C("#B9C4D0"));
                    a.Fill(S.Ring(50, 50, 40, 5), C("#5A6470"));
                    a.Fill(S.Seg(50, 50, 50, 78, 3), C("#2B3138"));
                    a.Fill(S.Seg(50, 50, 70, 40, 3), C("#2B3138"));
                    a.Fill(S.Circle(50, 50, 4.5f), C("#D94B4B"));
                },
                ["hud_speed"] = a =>
                {
                    a.Fill(S.Poly(10, 84, 38, 50, 10, 16, 28, 16, 56, 50, 28, 84), C("#FFE98A"), C("#F29A1E"));
                    a.Fill(S.Poly(44, 84, 72, 50, 44, 16, 62, 16, 90, 50, 62, 84), C("#FFE98A"), C("#F29A1E"));
                },
                ["hud_dot"] = a =>
                {
                    a.Fill(S.Circle(50, 50, 44), C("#FFFFFF"), C("#D5DAE0"));
                },
                ["hud_check"] = a =>
                {
                    a.Fill(S.Seg(18, 50, 40, 26, 7), C("#8CF0AB"), C("#3FBF6B"));
                    a.Fill(S.Seg(40, 26, 84, 78, 7), C("#8CF0AB"), C("#3FBF6B"));
                },
                ["hud_upgrade"] = a =>
                {
                    a.Fill(S.Poly(50, 94, 88, 52, 64, 52, 64, 10, 36, 10, 36, 52, 12, 52), C("#8CF0AB"), C("#3FBF6B"));
                },
                ["avatar_cmd"] = a =>
                {
                    Sdf disc = S.Circle(50, 50, 50);
                    a.Fill(disc, C("#35443A"), C("#1B221D"));
                    a.Fill(S.Intersect(S.Circle(50, 6, 36), disc), C("#6B7A5E"), C("#3C4538"));
                    a.Fill(S.Box(50, 40, 7, 8, 2), C("#B88F6B"));
                    a.Fill(S.Circle(50, 54, 15), C("#E0BC96"), C("#B88F6B"));
                    a.Fill(S.Intersect(S.Circle(50, 58, 19.5f), S.Above(58)), C("#6B7E5C"), C("#3B4A33"));
                    a.Fill(S.Box(50, 58, 22, 2.8f, 1.4f), C("#2F3B28"));
                    a.Fill(S.Star(50, 68, 6, 2.6f, 5), C("#F0CE62"));
                },
                ["frame_avatar"] = a =>
                {
                    a.Fill(S.Ring(50, 50, 44, 9), C("#FFE49A"), C("#B98A26"));
                    a.Fill(S.Ring(50, 50, 38.5f, 2), C("#5A3F0C", 0.7f));
                    a.Fill(S.Ring(50, 50, 49, 1.6f), C("#5A3F0C", 0.7f));
                },
                ["badge_level"] = a =>
                {
                    a.Fill(S.Shield(50, 50, 1f), C("#FFFFFF"), C("#C9CED4"));
                    a.Fill(S.Shield(50, 51, 0.8f), C("#2C343B"), C("#171C21"));
                },
                ["nav_base"] = a =>
                {
                    a.Fill(S.Box(50, 38, 35, 24, 2), C("#B7C5BE"), C("#6F8179"));
                    a.Fill(S.Box(24, 68, 8, 8, 1), C("#B7C5BE"), C("#8CA095"));
                    a.Fill(S.Box(50, 68, 8, 8, 1), C("#B7C5BE"), C("#8CA095"));
                    a.Fill(S.Box(76, 68, 8, 8, 1), C("#B7C5BE"), C("#8CA095"));
                    a.Fill(S.Union(S.Box(50, 26, 10, 12, 2), S.Circle(50, 38, 10)), C("#2B3138"));
                    a.Fill(S.Seg(50, 76, 50, 96, 1.6f), C("#D8DDE2"));
                    a.Fill(S.Poly(50, 96, 74, 90, 50, 84), C("#E25A5A"), C("#B83A3A"));
                },
                ["nav_heroes"] = a =>
                {
                    a.Fill(S.Intersect(S.Circle(50, 50, 36), S.Above(46)), C("#7E9470"), C("#465744"));
                    a.Fill(S.Box(50, 45, 42, 5, 2.5f), C("#364230"));
                    a.Fill(S.Circle(50, 28, 17), C("#E0BC96"), C("#B88F6B"));
                    a.Fill(S.Box(50, 40, 17, 3), C("#364230"));
                    a.Fill(S.Star(50, 62, 9, 4, 5), C("#F0CE62"));
                },
                ["nav_troops"] = a =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float dy = -i * 24f;
                        a.Fill(S.Poly(12, 74 + dy, 50, 96 + dy, 88, 74 + dy, 88, 58 + dy, 50, 80 + dy, 12, 58 + dy),
                            C("#FFE49A"), C("#C99A2E"));
                    }
                },
                ["nav_inventory"] = a =>
                {
                    a.Fill(S.Box(50, 46, 36, 32, 4), C("#A9895C"), C("#6B5232"));
                    a.Fill(S.Box(50, 66, 38, 8, 3), C("#C2A06E"), C("#8A6C42"));
                    a.Fill(S.Box(28, 44, 4.5f, 30, 1), C("#3C3226"));
                    a.Fill(S.Box(72, 44, 4.5f, 30, 1), C("#3C3226"));
                    a.Fill(S.Box(50, 42, 9, 4, 2), C("#2B2218"));
                },
                ["nav_world"] = a =>
                {
                    Sdf globe = S.Circle(50, 50, 40);
                    a.Fill(globe, C("#6CBBFF"), C("#1F5FA8"));
                    a.Fill(S.Intersect(S.Union(S.Union(S.Circle(36, 62, 14), S.Circle(62, 68, 10)), S.Union(S.Circle(58, 34, 15), S.Circle(30, 32, 7))), globe),
                        C("#8BD17F"), C("#4E9A4A"));
                    a.Fill(S.Ring(50, 50, 40, 3), C("#D6EEFF", 0.75f));
                },
                ["nav_alliance"] = a =>
                {
                    a.Fill(S.Shield(50, 50, 1f), C("#F2D27A"), C("#B98A26"));
                    a.Fill(S.Shield(50, 51, 0.82f), C("#6F9CC4"), C("#2F5878"));
                    a.Fill(S.Star(50, 54, 17, 7.5f, 5), C("#FFE49A"), C("#E0B84F"));
                },
                ["nav_shop"] = a =>
                {
                    a.Fill(S.Intersect(S.Ring(50, 72, 17, 6), S.Above(70)), C("#8A6414"));
                    a.Fill(S.Poly(20, 72, 80, 72, 87, 12, 13, 12), C("#F2CE6B"), C("#B98A26"));
                    a.Fill(S.Circle(50, 42, 14), C("#FFF0B8"), C("#E8C456"));
                    a.Fill(S.Ring(50, 42, 14, 3), C("#8A6414"));
                },
                ["side_construction"] = a =>
                {
                    a.Fill(S.Seg(26, 14, 66, 74, 6), C("#B98550"), C("#7E5328"));
                    a.Fill(S.Box(68, 78, 25, 11, 4, -34), C("#D4DCE5"), C("#7E8B9A"));
                    a.Fill(S.Box(54, 62, 4, 11, 1.5f, -34), C("#5A6470"));
                },
                ["side_research"] = a =>
                {
                    Sdf flask = S.Union(S.Box(50, 74, 9, 18, 2), S.Poly(41, 66, 59, 66, 86, 14, 14, 14));
                    a.Fill(flask, C("#E6F4FC", 0.95f), C("#B8D7EA", 0.95f));
                    a.Fill(S.Intersect(S.Poly(41, 66, 59, 66, 86, 14, 14, 14), S.Below(40)), C("#7BF0C0"), C("#2FBF8A"));
                    a.Fill(S.Circle(44, 26, 4), C("#E8FFF6"));
                    a.Fill(S.Circle(58, 20, 3), C("#E8FFF6"));
                    a.Fill(S.Box(50, 92, 14, 3.5f, 1.5f), C("#9FB4C4"));
                },
                ["side_missions"] = a =>
                {
                    a.Fill(S.Box(50, 46, 34, 42, 5), C("#D1AE72"), C("#9A7A45"));
                    a.Fill(S.Box(50, 44, 28, 35, 2), C("#F7F4EA"), C("#E3DFCF"));
                    a.Fill(S.Box(50, 86, 13, 7, 3), C("#7E8B9A"), C("#4A5460"));
                    for (int i = 0; i < 3; i++)
                    {
                        float y = 64 - i * 18;
                        a.Fill(S.Seg(33, y, 37, y - 5, 2.2f), C("#3FBF6B"));
                        a.Fill(S.Seg(37, y - 5, 44, y + 5, 2.2f), C("#3FBF6B"));
                        a.Fill(S.Seg(51, y, 68, y, 2f), C("#9AA7B5"));
                    }
                },
                ["side_events"] = a =>
                {
                    a.Fill(S.Star(50, 50, 46, 26, 8), C("#FFB070"), C("#E04B2B"));
                    a.Fill(S.Circle(50, 50, 16), C("#FFF0B8"), C("#FFC25A"));
                    a.Fill(S.Star(50, 50, 10, 4.2f, 5), C("#E04B2B"));
                },
                ["side_calendar"] = a =>
                {
                    a.Fill(S.Box(50, 46, 38, 38, 5), C("#FAF8F0"), C("#D8D4C4"));
                    a.Fill(S.Intersect(S.Box(50, 46, 38, 38, 5), S.Above(66)), C("#E96A6A"), C("#B83A3A"));
                    a.Fill(S.Box(32, 86, 3.5f, 8, 1.7f), C("#7E8B9A"));
                    a.Fill(S.Box(68, 86, 3.5f, 8, 1.7f), C("#7E8B9A"));
                    for (int row = 0; row < 2; row++)
                    {
                        for (int col = 0; col < 3; col++)
                        {
                            bool today = row == 1 && col == 1;
                            a.Fill(S.Box(30 + col * 20, 46 - row * 18 - 0, 6, 5.5f, 1.5f), today ? C("#E96A6A") : C("#AAB6C4"));
                        }
                    }
                },
                ["side_daily"] = a =>
                {
                    a.Fill(S.Box(50, 34, 33, 23, 3), C("#F07A7A"), C("#B83A3A"));
                    a.Fill(S.Box(50, 62, 37, 9, 3), C("#FF9A9A"), C("#D95A5A"));
                    a.Fill(S.Box(50, 48, 6.5f, 36), C("#FFE49A"), C("#E0B84F"));
                    a.Fill(S.Ring(40, 78, 9, 5), C("#FFE49A"));
                    a.Fill(S.Ring(60, 78, 9, 5), C("#FFE49A"));
                },
                ["side_battlepass"] = a =>
                {
                    a.Fill(S.Poly(26, 96, 46, 96, 60, 52, 40, 52), C("#6FB6F2"), C("#2F7FC8"));
                    a.Fill(S.Poly(74, 96, 54, 96, 40, 52, 60, 52), C("#F07A7A"), C("#B83A3A"));
                    a.Fill(S.Circle(50, 34, 28), C("#FFE49A"), C("#C99A2E"));
                    a.Fill(S.Ring(50, 34, 20, 3), C("#8A6414", 0.85f));
                    a.Fill(S.Star(50, 34, 13, 5.5f, 5), C("#8A6414", 0.9f));
                },
                ["side_offers"] = a =>
                {
                    a.Fill(S.Subtract(S.Poly(8, 50, 36, 88, 92, 88, 92, 12, 36, 12), S.Circle(32, 50, 6)), C("#FF8A7A"), C("#D13E3E"));
                    a.Fill(S.Seg(56, 28, 80, 72, 3), C("#FFFFFF"));
                    a.Fill(S.Ring(58, 68, 6.5f, 3.4f), C("#FFFFFF"));
                    a.Fill(S.Ring(78, 32, 6.5f, 3.4f), C("#FFFFFF"));
                },
            };
        }

        // ------------------------------------------------------------------ generation

        public static IReadOnlyCollection<string> Names => Definitions().Keys;

        /// <summary>Creates every missing icon (or all of them with overwrite). Returns how many files were written.</summary>
        public static int GenerateAll(bool overwrite)
        {
            EditorPaths.EnsureFolder(Folder);
            int written = 0;
            var created = new List<string>();

            foreach (KeyValuePair<string, Action<Art>> pair in Definitions())
            {
                string assetPath = $"{Folder}/{pair.Key}.png";
                if (!overwrite && File.Exists(assetPath)) continue;

                var art = new Art();
                pair.Value(art);
                if (!pair.Key.StartsWith("avatar_") && !pair.Key.StartsWith("frame_"))
                {
                    art.Rim(new Color(0.04f, 0.06f, 0.08f, 0.9f), 3);
                }

                var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
                texture.SetPixels(art.Px);
                texture.Apply();
                File.WriteAllBytes(assetPath, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
                created.Add(assetPath);
                written++;
            }

            AssetDatabase.Refresh();
            foreach (string assetPath in created) ConfigureImporter(assetPath);
            AssetDatabase.SaveAssets();
            return written;
        }

        private static void ConfigureImporter(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 128;
            importer.isReadable = false;
            importer.spritePixelsPerUnit = 100f;
            importer.SaveAndReimport();
        }

        /// <summary>Validation helper: names whose PNG is missing.</summary>
        public static List<string> FindMissing()
        {
            var missing = new List<string>();
            foreach (string name in Names)
            {
                if (!File.Exists($"{Folder}/{name}.png")) missing.Add(name);
            }
            return missing;
        }
    }
}
