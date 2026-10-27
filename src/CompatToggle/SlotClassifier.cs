using System;

namespace CompatToggle
{
    /// <summary>
    /// 슬롯을 판정 자리 종류로 나눈다. 이 파일은 EFT를 참조하지 않으므로 단위 테스트할 수 있다.
    /// EFT 객체를 입력으로 번역하는 쪽은 SlotClassifier.Eft.cs 에 있다.
    /// </summary>
    public static partial class SlotClassifier
    {
        private const string ChamberPrefix = "patron_in_weapon";
        private const string CamoraPrefix = "camora";
        private const string MagazineSlotId = "mod_magazine";
        private const string ModPrefix = "mod_";

        /// <summary>
        /// 슬롯 id와 부모 아이템의 종류로 판정 자리를 가린다.
        /// 무기·부품이 아닌 부모(장비, 보관 용기, 하이드아웃)는 전부 <see cref="SlotKind.Other"/>다.
        /// </summary>
        public static SlotKind ClassifyById(string slotId, bool parentIsWeapon, bool parentIsMod)
        {
            if (string.IsNullOrEmpty(slotId))
            {
                return SlotKind.Other;
            }

            if (slotId.StartsWith(ChamberPrefix, StringComparison.OrdinalIgnoreCase)
                || slotId.StartsWith(CamoraPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return SlotKind.Chamber;
            }

            if (slotId.Equals(MagazineSlotId, StringComparison.OrdinalIgnoreCase))
            {
                return SlotKind.Magazine;
            }

            if (slotId.StartsWith(ModPrefix, StringComparison.OrdinalIgnoreCase))
            {
                if (parentIsWeapon)
                {
                    return SlotKind.WeaponBody;
                }

                if (parentIsMod)
                {
                    return SlotKind.Attachment;
                }
            }

            return SlotKind.Other;
        }
    }
}
