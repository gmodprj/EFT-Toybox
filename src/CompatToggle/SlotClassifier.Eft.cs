using EFT.InventoryLogic;

namespace CompatToggle
{
    /// <summary>
    /// EFT 객체를 순수 분류부의 입력으로 번역한다. 이 파일만 게임 어셈블리에 의존한다.
    /// </summary>
    public static partial class SlotClassifier
    {
        /// <summary>
        /// 슬롯의 판정 자리 종류. 슬롯이나 부모가 없으면 <see cref="SlotKind.Other"/> —
        /// UI 미리보기나 고아 슬롯에서 예외를 던지면 인벤토리 조작 전체가 멈춘다.
        /// </summary>
        public static SlotKind Classify(Slot slot)
        {
            if (slot == null)
            {
                return SlotKind.Other;
            }

            var parent = slot.ParentItem;
            if (parent == null)
            {
                return SlotKind.Other;
            }

            return ClassifyById(slot.ID, parent is Weapon, parent is Mod);
        }

        /// <summary>대상 아이템이 무기 또는 무기 부품인가. 헬멧·안면 장비를 걸러내는 기준이다.</summary>
        public static bool IsWeaponOrMod(Item item)
        {
            return item is Weapon || item is Mod;
        }
    }
}
