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

        /// <summary>대상 아이템이 무기 또는 무기 부품인가.</summary>
        public static bool IsWeaponOrMod(Item item)
        {
            return item is Weapon || item is Mod;
        }

        /// <summary>부품 사슬을 거슬러 올라갈 최대 깊이. 순환이 생겨도 멈춘다.</summary>
        private const int MaxHostDepth = 16;

        /// <summary>
        /// 이 슬롯이 결국 무기에 달린 자리인가. 부품에 부품이 달린 경우를 따라 올라간다.
        ///
        /// 헬멧에 끼운 레일 어댑터는 <c>Mod</c>라서, 그 하위 슬롯만 보면 무기 쪽과 구별되지
        /// 않는다. 사슬의 최상위가 무기일 때만 참이다.
        /// </summary>
        public static bool HostIsWeapon(Slot slot)
        {
            var current = slot?.ParentItem;

            for (var depth = 0; current != null && depth < MaxHostDepth; depth++)
            {
                if (current is Weapon)
                {
                    return true;
                }

                if (!(current is Mod))
                {
                    return false;
                }

                current = current.CurrentAddress?.Container?.ParentItem;
            }

            return false;
        }

        /// <summary>이 탄약 묶음 자리가 탄창 안인가. 탄약 상자를 걸러내는 기준이다.</summary>
        public static bool HostIsMagazine(StackSlot slot)
        {
            return slot?.ParentItem is Magazine;
        }
    }
}
