using System.Reflection;
using System.Threading.Tasks;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace pitTeam.Patches
{
    // 편집기에서 **탄박스를 여는** 조작만 우리가 대신 처리한다.
    //
    // EFT 의 `ItemUiContext.UnloadAmmo` 는 탄박스에 대해 ① 탄을 적당한 자리로 옮기고 ② 빈 상자를
    // `ThrowItem`(버리기) 한다(역컴파일: `InventoryController.UnloadAmmoInstantly`). 플레이어 자신의
    // 화면에서는 그 조작이 서버로 가서 탄이 스태시에 남지만, 이 편집기의 컨트롤러는 **로컬 전용**이라
    // 2026-11-03 실측에서 **탄박스 10종과 그 안의 탄이 함께 사라졌다**(늘어난 종류가 하나도 없었다).
    //
    // 그래서 편집기가 열려 있는 동안에는 상자만 가로채서: 탄을 **그 팀원 스태시**로 한 더미씩 옮기고,
    // 하나라도 옮기지 못하면 **아무것도 하지 않는다**(상자는 그대로 남는다). 빈 상자는 버리지 않는다 -
    // 버리는 것은 아이템을 없애는 조작이고, 없애는 것은 플레이어가 직접 고르게 둔다(저장 때 확인을 받는다).
    //
    // 탄창·무기의 `탄약 꺼내기` 는 가로채지 않는다 - 그쪽은 상자를 버리는 단계가 없고 인게임에서 정상 동작이 확인됐다.
    internal sealed class StmEditorAmmoBoxUnloadPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(ItemUiContext), nameof(ItemUiContext.UnloadAmmo));

        [PatchPrefix]
        private static bool Prefix(ItemContext itemContext, ref Task __result)
        {
            if (!OtherPlayerProfileScreenPatch.TryStmEditorUnloadAmmoBox(itemContext, out Task handled))
            {
                return true;
            }

            __result = handled;
            return false;
        }
    }
}
