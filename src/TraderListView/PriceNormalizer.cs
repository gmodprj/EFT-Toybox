using System;
using System.Collections.Generic;

namespace TraderListView
{
    /// <summary>값을 치르는 것 한 가지: 통화 금액이든 물물교환 재료든 (템플릿, 개수).</summary>
    public readonly struct PricePart
    {
        public PricePart(string templateId, double count)
        {
            TemplateId = templateId;
            Count = count;
        }

        public string TemplateId { get; }

        public double Count { get; }
    }

    /// <summary>
    /// 루블 환산(설계 §5). 통화든 물물교환이든 같은 길: Σ(핸드북 가격 × 개수).
    /// 루블의 핸드북 가격은 1 이다. 하나라도 핸드북에 없으면 null.
    /// </summary>
    public static class PriceNormalizer
    {
        public static double? ToRubles(IReadOnlyList<PricePart> parts, Func<string, double?> handbookPrice)
        {
            if (parts == null || parts.Count == 0 || handbookPrice == null)
            {
                return null;
            }

            double sum = 0;
            foreach (var part in parts)
            {
                var unit = handbookPrice(part.TemplateId);
                if (!unit.HasValue)
                {
                    return null;
                }

                sum += unit.Value * part.Count;
            }

            return sum;
        }
    }
}
