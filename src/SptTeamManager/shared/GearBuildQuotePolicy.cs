using System;
using System.Collections.Generic;
using System.Linq;

namespace pitTeam.Shared
{
    // 살 것 한 줄의 값.
    public sealed record GearPriceLine(string Tpl, int Price);

    // 값이 다 매겨졌을 때의 견적.
    public sealed record GearBuildQuote(int Total, IReadOnlyList<GearPriceLine> Lines);

    // 고른 빌드를 입히는 데 드는 돈. 순수하고, 던지지 않는다.
    //
    // 규칙 하나가 이 정책의 전부다: **하나라도 값을 못 매기면 전체를 거절한다.** 부분 구매로 어중간하게
    // 입히면 플레이어는 돈을 냈는데 빌드와 다른 것을 입은 팀원을 보게 되고, 무엇이 빠졌는지 알 길이 없다.
    // PIT 의 용병 견적(TeammateEquipmentPrice.Calculate)도 값이 없으면 던져서 같은 결론을 낸다 -
    // 이쪽은 던지는 대신 못 매긴 tpl 을 돌려줘 화면이 이유를 말할 수 있게 한다.
    public static class GearBuildQuotePolicy
    {
        // 값이 없는 것으로 치는 경우: 0 이하, 유한하지 않은 수. 0 원짜리 장비는 없다 - 0 은 "표에 없다" 의
        // 다른 모습이고, 그것을 공짜로 넘기면 상인에게 없는 물건을 공짜로 주게 된다.
        public static bool TryQuote(
            IEnumerable<GearItemRow>? itemsToBuy,
            Func<string, double>? unitPrice,
            out GearBuildQuote? quote,
            out IReadOnlyList<string> unpriceableTpls)
        {
            quote = null;
            unpriceableTpls = Array.Empty<string>();

            var items = itemsToBuy?.Where(i => i != null && !string.IsNullOrWhiteSpace(i.Tpl)).ToList()
                        ?? new List<GearItemRow>();

            // 살 것이 없으면 0 원이다. 이미 그 빌드를 입고 있는 경우가 이렇다 - 실패가 아니다.
            if (items.Count == 0) return Empty(out quote);

            // 값을 물을 길이 없으면 전부 못 매긴 것이다(조용히 0 원으로 넘기지 않는다).
            if (unitPrice == null)
            {
                unpriceableTpls = items.Select(i => i.Tpl).Distinct(StringComparer.Ordinal).ToList();
                return false;
            }

            var lines = new List<GearPriceLine>();
            var missing = new List<string>();
            double total = 0;

            foreach (var item in items)
            {
                double price;
                try
                {
                    price = unitPrice(item.Tpl);
                }
                catch
                {
                    // 값을 묻다 던지는 것도 "못 매겼다" 이지 전체를 무너뜨릴 일이 아니다.
                    price = double.NaN;
                }

                if (!IsFinite(price) || price <= 0)
                {
                    if (!missing.Contains(item.Tpl, StringComparer.Ordinal)) missing.Add(item.Tpl);
                    continue;
                }

                // **묶음 수량을 곱한다.** 탄 60발은 한 점이 아니라 60점 값이다 - 수량을 세지 않으면
                // 탄창 가득한 꾸러미를 탄 한 발 값에 판다(2026-11-04, 격자·탄을 사기 시작하면서 생긴 자리).
                // 수량이 틀어져 있어도(0·음수) 한 점으로 친다 - 값을 깎는 쪽으로 넘어뜨리지 않는다.
                var count = item.StackCount > 0 ? item.StackCount : 1;
                var rounded = (int)Math.Ceiling(price) * count;
                lines.Add(new GearPriceLine(item.Tpl, rounded));
                total += rounded;
            }

            if (missing.Count > 0)
            {
                unpriceableTpls = missing;
                return false;
            }

            // 합이 int 를 넘으면 청구할 수 없는 값이다. 넘겨 보내 화면이 음수를 그리게 두지 않는다.
            if (!IsFinite(total) || total > int.MaxValue)
            {
                unpriceableTpls = lines.Select(l => l.Tpl).Distinct(StringComparer.Ordinal).ToList();
                return false;
            }

            quote = new GearBuildQuote((int)total, lines);
            return true;
        }

        // double.IsFinite 는 .NET Framework 4.7.2(클라이언트)에 없다. shared 는 세 프로젝트에서 다 컴파일되므로
        // 둘 다 가진 것으로만 쓴다.
        private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

        private static bool Empty(out GearBuildQuote? quote)
        {
            quote = new GearBuildQuote(0, Array.Empty<GearPriceLine>());
            return true;
        }
    }
}
