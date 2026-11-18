using System;
using System.Collections.Generic;
using System.Linq;
using EFT.InventoryLogic;
using EFT.Trading;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;

namespace TraderListView.Patches
{
    /// <summary>
    /// 상인 격자 정렬 자리(설계 §4). <c>TradingGridView.ApplyFilter</c> 는 매번 새 격자를 만들고
    /// <c>ItemSorter.Sort(FilterPanel.GetFilteredItems(_items))</c> 순서대로 채운다.
    /// 그 <c>ApplyFilter</c> 가 상인 쪽 격자일 때만 <c>ItemSorter.Sort</c> 결과를 우리 정렬로 바꾼다.
    /// 판매 쪽 내 창고 격자는 건드리지 않는다.
    /// </summary>
    [HarmonyPatch]
    public static class TraderGridSortPatch
    {
        [ThreadStatic] private static TradingGridView _sortingGrid;
        [ThreadStatic] private static IReadOnlyList<Item> _lastSorted;

        private static readonly AccessTools.FieldRef<TradingGridView, Assortment> AssortmentRef =
            AccessTools.FieldRefAccess<TradingGridView, Assortment>("_traderAssortment");

        /// <summary>상인 격자가 정렬을 끝낼 때마다 (격자뷰, 정렬된 집합 = 지금 격자에 보이는 것).</summary>
        public static event Action<TradingGridView, IReadOnlyList<Item>> ListApplied;

        /// <summary>정렬 설정이 바뀌면 부른다 — 상인 격자를 다시 채운다(목록도 따라온다).</summary>
        public static void Refresh(TraderDealScreen screen)
        {
            var grid = screen != null ? screen._traderGridView : null;
            if (grid != null && grid.gameObject.activeInHierarchy)
            {
                grid.ApplyFilterAndRefresh();
            }
        }

        public static bool IsTraderGrid(TradingGridView grid)
        {
            if (grid == null)
            {
                return false;
            }

            var screen = grid.GetComponentInParent<TraderDealScreen>(true);
            return screen != null && screen._traderGridView == grid;
        }

        [HarmonyPatch(typeof(TradingGridView), nameof(TradingGridView.ApplyFilter))]
        [HarmonyPrefix]
        private static void ApplyFilterPrefix(TradingGridView __instance)
        {
            _sortingGrid = IsTraderGrid(__instance) ? __instance : null;
            _lastSorted = null;
        }

        [HarmonyPatch(typeof(ItemSorter), nameof(ItemSorter.Sort))]
        [HarmonyPostfix]
        private static void SortPostfix(ref List<Item> __result)
        {
            var grid = _sortingGrid;
            if (grid == null || __result == null)
            {
                return;
            }

            // 한 번의 ApplyFilter 안에서 한 번만 바꾼다.
            _sortingGrid = null;
            try
            {
                var spec = Plugin.CurrentSort;
                if (spec.Key != SortKey.Default)
                {
                    var assortment = AssortmentRef(grid);
                    int loyalty = TraderLoyalty(assortment);
                    var rows = new List<OfferRow>(__result.Count);
                    for (int i = 0; i < __result.Count; i++)
                    {
                        rows.Add(OfferRowFactory.Create(__result[i], assortment, i, loyalty));
                    }

                    __result = TraderSorter.Sort(rows, spec).Select(r => (Item)r.Tag).ToList();
                }

                _lastSorted = __result;
                _lastGrid = grid;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("상인 격자 정렬 실패 — 게임 순서로 둔다: " + e);
            }
        }

        [ThreadStatic] private static TradingGridView _lastGrid;

        [HarmonyPatch(typeof(TradingGridView), nameof(TradingGridView.ApplyFilter))]
        [HarmonyFinalizer]
        private static void ApplyFilterFinalizer()
        {
            _sortingGrid = null;
            var grid = _lastGrid;
            var sorted = _lastSorted;
            _lastGrid = null;
            _lastSorted = null;
            if (grid == null || sorted == null)
            {
                return;
            }

            try
            {
                ListApplied?.Invoke(grid, sorted);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("목록 갱신 실패: " + e);
            }
        }

        /// <summary>격자가 보여 주는 상인의 물건 묶음(구매 창이 따르는 것).</summary>
        public static Assortment AssortmentOf(TradingGridView grid) => grid != null ? AssortmentRef(grid) : null;

        public static int TraderLoyalty(Assortment assortment)
        {
            var trader = assortment != null ? assortment._trader : null;
            return trader != null ? trader.LoyaltyLevel : 1;
        }
    }
}
