using System.Reflection;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;

namespace CompatToggle.Patches
{
    /// <summary>
    /// 탄창 내부에 들어가는 탄약 묶음. 약실과 무기의 탄창 슬롯은 <see cref="Slot"/>이므로
    /// <see cref="SlotCompatibilityPatch"/>가 이미 처리한다.
    ///
    /// <see cref="StackSlot"/>은 <see cref="Magazine"/>과 <c>AmmoBox</c>가 모두 만들므로
    /// 숙주가 탄창인지 확인한다. 그러지 않으면 탄약 상자에 탄약이 아닌 물건이 들어간다.
    ///
    /// 탄을 채우는 경로는 <c>StackSlot.GetMaxAddCount</c>를 지나 이 판정에 닿는다. 그래서
    /// <c>Magazine.CheckCompatibility(Ammo)</c>는 패치하지 않는다 — 그쪽은 봇의 재장전
    /// 판단(<c>BotReloadOnlyBarrel</c>, <c>BotReloadRevolver</c>)과 탄약 선택 UI 목록까지
    /// 함께 지나가므로, 플레이어 조작에 필요하지 않은데 봇 동작을 바꾼다.
    /// </summary>
    public class StackSlotCompatibilityPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(StackSlot).GetMethod(nameof(StackSlot.CheckCompatibility), new[] { typeof(Item) });
        }

        [PatchPostfix]
        private static void Postfix(StackSlot __instance, Item item, ref bool __result)
        {
            if (__result)
            {
                return;
            }

            var allow = CompatGate.ShouldForceAllowCartridgeStack(
                CompatTogglePlugin.Toggles,
                SlotClassifier.HostIsMagazine(__instance));

            if (!allow)
            {
                return;
            }

            __result = true;
            CompatTogglePlugin.LogForced("CartridgeStack", __instance?.ID, item?.TemplateId.ToString());
        }
    }
}
