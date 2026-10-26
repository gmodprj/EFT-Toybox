namespace CompatToggle
{
    /// <summary>
    /// 호환성 판정이 걸리는 자리의 종류. 아이템 DB가 아니라 "지금 무엇을 판정하는가"로 나눈다.
    /// </summary>
    public enum SlotKind
    {
        /// <summary>토글이 관여하지 않는 자리(장비 슬롯, 보관 용기, 하이드아웃 등).</summary>
        Other = 0,

        /// <summary>무기 본체의 부착 슬롯.</summary>
        WeaponBody,

        /// <summary>부품의 하위 슬롯.</summary>
        Attachment,

        /// <summary>무기의 탄창 슬롯.</summary>
        Magazine,

        /// <summary>약실.</summary>
        Chamber,

        /// <summary>탄창 내부의 탄약 묶음.</summary>
        CartridgeStack,
    }

    /// <summary>
    /// 토글과 판정 자리만 보고 "원본이 거부한 것을 허용으로 바꿀지"를 답한다.
    /// EFT·BepInEx·Unity를 참조하지 않는다.
    /// </summary>
    public static class CompatGate
    {
        /// <summary>
        /// 이 자리의 제한을 무시하고 허용해야 하는가. 원본이 이미 허용한 경우에는 부르지 않아도 된다.
        /// </summary>
        public static bool ShouldForceAllow(in ToggleSet toggles, SlotKind kind)
        {
            if (!toggles.Master)
            {
                return false;
            }

            switch (kind)
            {
                case SlotKind.WeaponBody:
                    return toggles.WeaponSlots;
                case SlotKind.Attachment:
                    return toggles.AttachmentSlots;
                case SlotKind.Magazine:
                    return toggles.MagazineCalibers;
                case SlotKind.Chamber:
                    return toggles.ChamberAmmo;
                case SlotKind.CartridgeStack:
                    return toggles.MagazineAmmo;
                default:
                    return false;
            }
        }

        /// <summary>
        /// ConflictingItems 충돌을 무시해야 하는가. 무기·부품에만 적용한다 —
        /// 헬멧·안면 장비의 충돌은 그것을 써넣은 모드들의 의도이므로 건드리지 않는다.
        /// </summary>
        public static bool ShouldIgnoreConflict(in ToggleSet toggles, bool subjectIsWeaponOrMod)
        {
            return toggles.Master && toggles.IgnoreConflicts && subjectIsWeaponOrMod;
        }
    }
}
