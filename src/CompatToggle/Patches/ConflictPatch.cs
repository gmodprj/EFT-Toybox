using System.Reflection;
using Diz.LanguageExtensions;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;

namespace CompatToggle.Patches
{
    /// <summary>
    /// ConflictingItems 충돌 검사.
    ///
    /// 원본은 충돌이 없으면 <c>Option&lt;bool&gt;</c>에 true를, 충돌이 있으면
    /// <c>ConflictingItemError</c>를 담아 돌려준다(Assembly-CSharp IL에서 확인).
    /// 그래서 Failed 인 경우에만 손을 댄다.
    ///
    /// 설치하려는 아이템이 무기·부품일 때만 무시한다. 헬멧·안면 장비의 충돌은
    /// HelmetSlotUnion·LessRestrictingHeadwear·WolfiksHeavyTrooperMasks 가 의도를 갖고
    /// 써넣은 값이므로 건드리지 않는다.
    /// </summary>
    public class ConflictPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Slot).GetMethod(nameof(Slot.CheckConflictingItems), new[] { typeof(Item) });
        }

        [PatchPostfix]
        private static void Postfix(Item item, ref Option<bool> __result)
        {
            if (!__result.Failed)
            {
                return;
            }

            if (!CompatGate.ShouldIgnoreConflict(CompatTogglePlugin.Toggles, SlotClassifier.IsWeaponOrMod(item)))
            {
                return;
            }

            __result = true;
        }
    }
}
