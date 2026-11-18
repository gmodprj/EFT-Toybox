using System;
using EFT.UI;
using HarmonyLib;
using TraderListView.Ui;

namespace TraderListView.Patches
{
    /// <summary>
    /// 상인 화면이 열릴 때 헤더를 다시 놓고(겹침 해소) 우리 부품을 붙인다.
    /// Trader Search·Assort Editor 의 Show 패치보다 늦게 돈다.
    /// </summary>
    [HarmonyPatch(typeof(TraderDealScreen), nameof(TraderDealScreen.Show))]
    public static class TraderScreenShowPatch
    {
        /// <summary>화면에 우리 부품이 붙은 뒤(헤더가 준비된 뒤). Task 5 의 목록이 여기에 붙는다.</summary>
        public static event Action<TraderDealScreen, HeaderLayout> Attached;

        [HarmonyPostfix]
        [HarmonyAfter("com.maschine.TraderSearch", "com.goatboy.ultimate-trader-assort-editor")]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(TraderDealScreen __instance)
        {
            try
            {
                var header = HeaderLayout.Attach(__instance);
                header.OnShown();
                Attached?.Invoke(__instance, header);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("상인 화면 부품 붙이기 실패: " + e);
            }
        }
    }
}
