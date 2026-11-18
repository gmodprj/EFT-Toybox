using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.HandBook;
using EFT.InventoryLogic;
using EFT.Trading;

namespace TraderListView
{
    /// <summary>게임 물건(<see cref="Item"/>) → 정렬·표시용 <see cref="OfferRow"/>.</summary>
    public static class OfferRowFactory
    {
        public static OfferRow Create(Item item, Assortment assortment, int defaultIndex, int currentLoyaltyLevel)
        {
            var parts = Parts(item, assortment);
            double? rub = PriceNormalizer.ToRubles(parts, HandbookPrice);

            int loyalty = 1;
            if (assortment?.LoyalLevelItems != null && assortment.LoyalLevelItems.TryGetValue(item.Id, out var level))
            {
                loyalty = level;
            }

            return new OfferRow(DisplayName(item), rub, AvailableStock(item), loyalty, loyalty > currentLoyaltyLevel, defaultIndex, item);
        }

        /// <summary>
        /// 값을 치르는 것들. 물물교환 스킴의 첫 변형(게임 구매 창이 쓰는 것)을 (템플릿, 개수)로 편다.
        /// <c>Assortment.GetBarterPrice</c> 는 재료를 구분하지 않고 개수만 더하므로 쓰지 않는다.
        /// </summary>
        public static List<PricePart> Parts(Item item, Assortment assortment)
        {
            var parts = new List<PricePart>();
            var scheme = assortment?.GetSchemeForItem(item);
            if (scheme == null || scheme.Count == 0 || scheme[0] == null)
            {
                return parts;
            }

            foreach (var template in scheme[0])
            {
                parts.Add(new PricePart(template._tpl, template.count));
            }

            return parts;
        }

        /// <summary>핸드북 가격. 핸드북에 없으면 null(정렬에서 맨 뒤). 루블은 1.</summary>
        public static double? HandbookPrice(string templateId)
        {
            if (templateId == PriceText.RubTpl)
            {
                return 1;
            }

            var handbook = Singleton<Handbook>.Instance;
            if (handbook == null || string.IsNullOrEmpty(templateId))
            {
                return null;
            }

            try
            {
                var node = handbook.StructuredItems[templateId];
                return node?.Data != null ? node.Data.Price : (double?)null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string DisplayName(Item item) => item.LocalizedName();

        /// <summary>재료의 짧은 이름(게임 locale 키 "<tpl> ShortName").</summary>
        public static string ShortName(string templateId) => (templateId + " ShortName").Localized();

        /// <summary>살 수 있는 수: 재고, 구매 제한이 있으면 남은 수와 둘 중 작은 것.</summary>
        public static long AvailableStock(Item item)
        {
            long stack = item.StackObjectsCount;
            int max = item.BuyRestrictionMax;
            if (max > 0)
            {
                long remaining = Math.Max(0, max - item.BuyRestrictionCurrent);
                return Math.Min(stack, remaining);
            }

            return stack;
        }
    }
}
