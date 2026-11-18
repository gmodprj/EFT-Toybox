using System;
using System.Collections.Generic;
using System.Text;

namespace TraderListView
{
    /// <summary>
    /// 게임 글꼴에 없는 기호를 글자로 바꾼다(없는 글리프는 빈 네모로 그려진다).
    /// 바꿀 표를 아는 기호만 바꾸고, 모르는 것은 그대로 둔다.
    /// </summary>
    public static class GlyphFallback
    {
        private static readonly Dictionary<int, string> Replacements = new Dictionary<int, string>
        {
            [0x1F512] = "(잠김)", // 🔒
            [0x21C4] = "교환",    // ⇄
            [0x25B2] = "^",       // ▲
            [0x25BC] = "v",       // ▼
            [0x25A6] = "격자",    // ▦
            [0x2630] = "목록",    // ☰
        };

        public static string Replace(string text, Func<int, bool> hasCodepoint)
        {
            if (string.IsNullOrEmpty(text) || hasCodepoint == null)
            {
                return text;
            }

            StringBuilder sb = null;
            for (int i = 0; i < text.Length; i++)
            {
                // 짝 없는 서로게이트(깨진 locale 글자)는 바꾸지 않고 그대로 옮긴다.
                bool pair = char.IsSurrogatePair(text, i);
                int width = pair ? 2 : 1;
                int cp = pair || !char.IsSurrogate(text[i]) ? char.ConvertToUtf32(text, i) : -1;
                if (Replacements.TryGetValue(cp, out var replacement) && !hasCodepoint(cp))
                {
                    if (sb == null)
                    {
                        sb = new StringBuilder(text.Length + 8);
                        sb.Append(text, 0, i);
                    }

                    sb.Append(replacement);
                }
                else if (sb != null)
                {
                    sb.Append(text, i, width);
                }

                i += width - 1;
            }

            return sb?.ToString() ?? text;
        }
    }
}
