using System;
using System.Collections.Generic;
using System.Globalization;

namespace TraderListView
{
    /// <summary>목록 칸에 쓰는 글자(가격·재고·등급). 게임처럼 천 단위를 공백으로 띄운다.</summary>
    public static class PriceText
    {
        public const string RubTpl = "5449016a4bdc2d6f028b456f";
        public const string UsdTpl = "5696686a4bdc2da3298b456a";
        public const string EurTpl = "569668774bdc2da2298b4568";

        private static readonly NumberFormatInfo Grouped = new NumberFormatInfo
        {
            NumberGroupSeparator = " ",
            NumberDecimalDigits = 0,
        };

        public static string Price(IReadOnlyList<PricePart> parts, Func<string, string> shortName)
        {
            if (parts == null || parts.Count == 0)
            {
                return "-";
            }

            if (parts.Count == 1)
            {
                var part = parts[0];
                var symbol = CurrencySymbol(part.TemplateId);
                if (symbol != null)
                {
                    return Group(part.Count) + " " + symbol;
                }

                var name = shortName?.Invoke(part.TemplateId) ?? part.TemplateId;
                return "⇄ " + name + " ×" + Group(part.Count);
            }

            return "⇄ 물물교환 " + parts.Count + "종";
        }

        /// <summary>구매 제한이 있으면 <c>"재고 (남은/최대)"</c>.</summary>
        public static string Stock(long stack, int? restrictionMax, int? restrictionCurrent)
        {
            var text = stack.ToString(CultureInfo.InvariantCulture);
            if (restrictionMax.HasValue && restrictionMax.Value > 0)
            {
                int remaining = Math.Max(0, restrictionMax.Value - (restrictionCurrent ?? 0));
                text += " (" + remaining + "/" + restrictionMax.Value + ")";
            }

            return text;
        }

        public static string Loyalty(int level, bool locked) => "LL" + level + (locked ? " 🔒" : string.Empty);

        private static string CurrencySymbol(string templateId)
        {
            switch (templateId)
            {
                case RubTpl:
                    return "₽";
                case UsdTpl:
                    return "$";
                case EurTpl:
                    return "€";
                default:
                    return null;
            }
        }

        private static string Group(double value) => Math.Round(value).ToString("N0", Grouped);
    }
}
