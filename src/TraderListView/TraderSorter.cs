using System;
using System.Collections.Generic;
using System.Linq;

namespace TraderListView
{
    /// <summary>
    /// 상인 물건 줄 세우기. 규칙(설계 §5):
    /// 키 비교(방향 적용) → 이름(대소문자 무시, 방향 미적용) → 게임 기본 순서.
    /// 가격을 모르는 줄은 방향과 상관없이 맨 뒤.
    /// </summary>
    public static class TraderSorter
    {
        private static readonly StringComparer NameComparer = StringComparer.OrdinalIgnoreCase;

        public static List<OfferRow> Sort(IEnumerable<OfferRow> rows, SortSpec spec)
        {
            var list = rows?.ToList() ?? new List<OfferRow>();
            list.Sort((a, b) => Compare(a, b, spec));
            return list;
        }

        private static int Compare(OfferRow a, OfferRow b, SortSpec spec)
        {
            if (spec.Key == SortKey.Default)
            {
                return a.DefaultIndex.CompareTo(b.DefaultIndex);
            }

            if (spec.Key == SortKey.Price)
            {
                bool aUnknown = !a.RubPrice.HasValue;
                bool bUnknown = !b.RubPrice.HasValue;
                if (aUnknown != bUnknown)
                {
                    return aUnknown ? 1 : -1;
                }
            }

            int byKey = CompareKey(a, b, spec.Key);
            if (byKey != 0)
            {
                return spec.Descending ? -byKey : byKey;
            }

            int byName = NameComparer.Compare(a.Name, b.Name);
            if (byName != 0)
            {
                return byName;
            }

            return a.DefaultIndex.CompareTo(b.DefaultIndex);
        }

        private static int CompareKey(OfferRow a, OfferRow b, SortKey key)
        {
            switch (key)
            {
                case SortKey.Price:
                    // 둘 다 모르면 0 — 이름으로 넘어간다.
                    return Nullable.Compare(a.RubPrice, b.RubPrice);
                case SortKey.Name:
                    return NameComparer.Compare(a.Name, b.Name);
                case SortKey.Stock:
                    return a.Stock.CompareTo(b.Stock);
                case SortKey.Loyalty:
                    return a.LoyaltyLevel.CompareTo(b.LoyaltyLevel);
                default:
                    return 0;
            }
        }
    }
}
