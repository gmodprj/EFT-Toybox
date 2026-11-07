using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace pitTeam.Patches
{
    /// <summary>
    /// 교환 화면에서 **격자 사이로 끌어다 놓으면 그 자체가 전송**이 되게 한다(계획 6 B3-c).
    ///
    /// 왜 가로채야 하나 — 두 칸은 서로 다른 `InventoryController` 를 쓴다(프로필이 둘이다).
    /// EFT 의 드롭은 자기 컨트롤러 안에서만 옮기므로, 그대로 두면 남의 스태시로 끌어다 놓는 순간
    /// **실패하거나(아무 일도 안 일어남) 엉뚱한 곳으로 간다.** 그래서 교차 드롭만 낚아채
    /// 원래 처리를 막고 서버의 전송 라우트로 돌린다.
    ///
    /// **가로채는 범위를 좁게 잡는다.** 교환 화면이 열려 있고(`StmTransferGridsReady`), 끌어온
    /// 아이템이 한쪽 스태시의 것이며, 놓는 격자가 **반대쪽** 패널 밑일 때만이다. 그 밖의 모든
    /// 드롭(같은 칸 안의 정리, 편집기, 플레이어 자신의 스태시 화면)은 **원래 코드가 그대로** 돈다.
    ///
    /// 복수도 같은 길로 간다 — 끌어온 것이 지금 고른 것들 중 하나면 그 선택 전체를 한 번에 보낸다
    /// (UI Fixes 의 복수 선택). 그래서 서버 호출도 저널 항목도 **한 판에 하나**다.
    /// </summary>
    internal class StmStashTransferDropPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // GridView.AcceptItem(DragItemContext, ItemContext) — 격자가 드롭을 받는 자리.
            return AccessTools.Method(typeof(GridView), nameof(GridView.AcceptItem));
        }

        [PatchPrefix]
        private static bool PatchPrefix(GridView __instance, DragItemContext itemContext, ref Task __result)
        {
            try
            {
                if (!OtherPlayerProfileScreenPatch.StmTransferHandlesDrop(__instance, itemContext?.Item, out bool toTeammate))
                {
                    return true; // 우리 일이 아니다. 원래대로 돈다.
                }

                // **놓은 자리를 그대로 읽는다.** 서버가 빈 자리를 찾아 넣으면 사용자가 놓은 곳과
                // 다른 데로 가고, 그건 "자동 정렬" 처럼 보인다(2026-11-07 지적). EFT 가 드롭 위치를
                // 계산하는 바로 그 함수를 쓴다 — 우리가 좌표를 다시 셈하지 않는다.
                int x = -1, y = -1, rotation = 0;
                string gridName = null;
                try
                {
                    LocationInGrid where = __instance.CalculateItemLocation(itemContext);
                    if (where != null)
                    {
                        x = where.x;
                        y = where.y;
                        rotation = (int)where.r;
                    }

                    gridName = __instance.Grid?.ID;
                }
                catch (Exception locationEx)
                {
                    // 자리를 못 읽어도 전송은 한다 — 서버가 빈 자리를 찾는다(예전 동작).
                    pitFireTeam.Log.LogWarning("[STM] 스태시 교환: 놓은 자리를 읽지 못했다: " + locationEx.Message);
                }

                // 원래 처리를 막는다. 막지 않으면 EFT 가 자기 컨트롤러 안에서 옮기려다 실패하고,
                // 그 실패가 우리 전송과 겹쳐 화면과 실제가 어긋난다.
                __result = Task.CompletedTask;
                OtherPlayerProfileScreenPatch.StmTransferDropped(itemContext.Item, toTeammate, gridName, x, y, rotation);
                return false;
            }
            catch (Exception ex)
            {
                // 가로채다 실패하면 **원래 코드로 돌려보낸다**(막아 둔 채 끝내지 않는다).
                pitFireTeam.Log.LogError("[STM] 스태시 교환: 드롭을 가로채다 실패했다.");
                pitFireTeam.Log.LogError(ex);
                return true;
            }
        }
    }
}
