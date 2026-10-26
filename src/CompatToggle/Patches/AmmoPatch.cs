using System.Reflection;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;

namespace CompatToggle.Patches
{
    /// <summary>
    /// 탄창 내부에 들어가는 탄약 묶음. 약실과 무기의 탄창 슬롯은 <see cref="Slot"/>이므로
    /// <see cref="SlotCompatibilityPatch"/>가 이미 처리한다.
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

            if (!CompatGate.ShouldForceAllow(CompatTogglePlugin.Toggles, SlotKind.CartridgeStack))
            {
                return;
            }

            __result = true;
            CompatTogglePlugin.LogForced("CartridgeStack", __instance?.ID, item?.TemplateId.ToString());
        }
    }

    /// <summary>
    /// 탄창이 그 탄약을 받는지. 인벤토리에서 끌어다 놓는 경로뿐 아니라 탄을 채우는
    /// 장전 경로도 이 판정을 지나므로, 토글은 레이드 중에도 동작한다(설계 §5.3).
    /// </summary>
    public class MagazineCompatibilityPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Magazine).GetMethod(nameof(Magazine.CheckCompatibility), new[] { typeof(Ammo) });
        }

        [PatchPostfix]
        private static void Postfix(Magazine __instance, Ammo ammo, ref bool __result)
        {
            if (__result)
            {
                return;
            }

            if (!CompatGate.ShouldForceAllow(CompatTogglePlugin.Toggles, SlotKind.CartridgeStack))
            {
                return;
            }

            __result = true;
            CompatTogglePlugin.LogForced("Magazine", __instance?.TemplateId.ToString(), ammo?.TemplateId.ToString());
        }
    }
}
