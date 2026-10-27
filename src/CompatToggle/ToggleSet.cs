namespace CompatToggle
{
    /// <summary>
    /// 한 순간의 F12 토글 값 묶음. EFT·BepInEx·Unity를 참조하지 않으므로
    /// 게임 어셈블리 없이 단위 테스트할 수 있다.
    /// </summary>
    public readonly struct ToggleSet
    {
        public ToggleSet(
            bool master,
            bool weaponSlots,
            bool attachmentSlots,
            bool chamberAmmo,
            bool magazineCalibers,
            bool magazineAmmo,
            bool ignoreConflicts)
        {
            Master = master;
            WeaponSlots = weaponSlots;
            AttachmentSlots = attachmentSlots;
            ChamberAmmo = chamberAmmo;
            MagazineCalibers = magazineCalibers;
            MagazineAmmo = magazineAmmo;
            IgnoreConflicts = ignoreConflicts;
        }

        /// <summary>꺼지면 나머지 토글 전부가 무효가 된다.</summary>
        public bool Master { get; }

        /// <summary>무기 본체의 부착 슬롯에 어떤 부품이든 허용.</summary>
        public bool WeaponSlots { get; }

        /// <summary>부품의 하위 슬롯에 어떤 부품이든 허용.</summary>
        public bool AttachmentSlots { get; }

        /// <summary>약실에 어떤 탄약이든 허용.</summary>
        public bool ChamberAmmo { get; }

        /// <summary>무기에 어떤 탄창이든 허용.</summary>
        public bool MagazineCalibers { get; }

        /// <summary>탄창에 어떤 탄약이든 허용.</summary>
        public bool MagazineAmmo { get; }

        /// <summary>무기·부품의 ConflictingItems 충돌을 무시.</summary>
        public bool IgnoreConflicts { get; }
    }
}
