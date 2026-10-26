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
        /// ConflictingItems 충돌을 무시해야 하는가.
        ///
        /// 설치하려는 아이템이 무기·부품인 것만으로는 부족하다. 헬멧에 끼우는 야투경과
        /// 레일 어댑터도 <c>Mod</c>이기 때문이다(NightVision → SightMod → FunctionalMod → Mod,
        /// Mount → GearMod → Mod). 그래서 숙주가 무기인지도 함께 본다 — 그러지 않으면
        /// 헬멧 모드들이 의도를 갖고 써넣은 충돌까지 가려버린다.
        /// </summary>
        public static bool ShouldIgnoreConflict(in ToggleSet toggles, bool subjectIsWeaponOrMod, bool hostIsWeapon)
        {
            return toggles.Master && toggles.IgnoreConflicts && subjectIsWeaponOrMod && hostIsWeapon;
        }

        /// <summary>
        /// 탄약 묶음 자리를 열어야 하는가. <c>StackSlot</c>은 탄창과 탄약 상자가 모두 만들므로,
        /// 숙주가 탄창일 때만 연다 — 그러지 않으면 탄약 상자의 보관 제한까지 무너진다.
        /// </summary>
        public static bool ShouldForceAllowCartridgeStack(in ToggleSet toggles, bool hostIsMagazine)
        {
            return hostIsMagazine && ShouldForceAllow(toggles, SlotKind.CartridgeStack);
        }
    }
}
