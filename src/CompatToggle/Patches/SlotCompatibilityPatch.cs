using System.Reflection;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;

namespace CompatToggle.Patches
{
    /// <summary>
    /// 슬롯이 아이템을 받을 수 있는지 판정하는 자리. 무기 본체·부품·탄창·약실 슬롯이 모두
    /// <see cref="Slot"/>이므로 토글 네 개가 이 패치 하나를 지난다.
    ///
    /// Grid·VirtualContainer·HideoutAreaGrid 에도 같은 이름의 메서드가 있지만 대상이 아니다 —
    /// 그쪽까지 열면 보관 제한이 무너진다.
    /// </summary>
    public class SlotCompatibilityPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Slot).GetMethod(nameof(Slot.CheckCompatibility), new[] { typeof(Item) });
        }

        /// <summary>
        /// Postfix만 쓴다. 원본을 건너뛰지 않으므로 같은 메서드를 패치하는 다른 플러그인이
        /// 영향받지 않는다.
        /// </summary>
        [PatchPostfix]
        private static void Postfix(Slot __instance, ref bool __result)
        {
            if (__result)
            {
                return;
            }

            var kind = SlotClassifier.Classify(__instance);
            if (!CompatGate.ShouldForceAllow(CompatTogglePlugin.Toggles, kind))
            {
                return;
            }

            __result = true;
        }
    }
}
