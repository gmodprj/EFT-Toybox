using CompatToggle;
using Xunit;

namespace CompatToggle.Tests
{
    public class CompatGateTests
    {
        static ToggleSet AllOn => new(true, true, true, true, true, true, true);
        static ToggleSet AllOff => new(false, false, false, false, false, false, false);

        [Theory]
        [InlineData(SlotKind.WeaponBody)]
        [InlineData(SlotKind.Attachment)]
        [InlineData(SlotKind.Magazine)]
        [InlineData(SlotKind.Chamber)]
        [InlineData(SlotKind.CartridgeStack)]
        public void AllOn_forces_allow_for_every_handled_kind(SlotKind kind)
            => Assert.True(CompatGate.ShouldForceAllow(AllOn, kind));

        [Theory]
        [InlineData(SlotKind.WeaponBody)]
        [InlineData(SlotKind.Attachment)]
        [InlineData(SlotKind.Magazine)]
        [InlineData(SlotKind.Chamber)]
        [InlineData(SlotKind.CartridgeStack)]
        public void AllOff_forces_nothing(SlotKind kind)
            => Assert.False(CompatGate.ShouldForceAllow(AllOff, kind));

        [Fact]
        public void Other_kind_is_never_forced()
            => Assert.False(CompatGate.ShouldForceAllow(AllOn, SlotKind.Other));

        [Fact]
        public void Master_off_overrides_every_toggle()
        {
            var t = new ToggleSet(false, true, true, true, true, true, true);
            Assert.False(CompatGate.ShouldForceAllow(t, SlotKind.WeaponBody));
            Assert.False(CompatGate.ShouldIgnoreConflict(t, subjectIsWeaponOrMod: true, hostIsWeapon: true));
            Assert.False(CompatGate.ShouldForceAllowCartridgeStack(t, hostIsMagazine: true));
        }

        [Fact]
        public void Each_toggle_governs_only_its_own_kind()
        {
            var onlyWeapon = new ToggleSet(true, true, false, false, false, false, false);
            Assert.True(CompatGate.ShouldForceAllow(onlyWeapon, SlotKind.WeaponBody));
            Assert.False(CompatGate.ShouldForceAllow(onlyWeapon, SlotKind.Attachment));
            Assert.False(CompatGate.ShouldForceAllow(onlyWeapon, SlotKind.Magazine));
            Assert.False(CompatGate.ShouldForceAllow(onlyWeapon, SlotKind.Chamber));
            Assert.False(CompatGate.ShouldForceAllow(onlyWeapon, SlotKind.CartridgeStack));
        }

        [Fact]
        public void Conflicts_are_ignored_only_for_weapons_and_mods()
        {
            Assert.True(CompatGate.ShouldIgnoreConflict(AllOn, subjectIsWeaponOrMod: true, hostIsWeapon: true));
            Assert.False(CompatGate.ShouldIgnoreConflict(AllOn, subjectIsWeaponOrMod: false, hostIsWeapon: true));
        }

        [Fact]
        public void Conflicts_off_ignores_nothing()
            => Assert.False(CompatGate.ShouldIgnoreConflict(AllOff, subjectIsWeaponOrMod: true, hostIsWeapon: true));

        /// <summary>
        /// 헬멧에 끼우는 야투경·레일 어댑터는 Mod 다(NightVision → SightMod → FunctionalMod → Mod,
        /// Mount → GearMod → Mod). 그래서 대상 아이템만 보면 헬멧 쪽 충돌까지 무시된다 —
        /// 설계 목표 5가 깨지는 자리다. 숙주가 무기일 때만 무시해야 한다.
        /// </summary>
        [Fact]
        public void Conflicts_on_a_helmet_host_are_never_ignored()
            => Assert.False(CompatGate.ShouldIgnoreConflict(AllOn, subjectIsWeaponOrMod: true, hostIsWeapon: false));

        /// <summary>
        /// StackSlot 은 Magazine 과 AmmoBox 가 모두 만든다. 탄창 토글이 탄약 상자까지
        /// 열어버리면 보관 제한이 무너진다.
        /// </summary>
        [Fact]
        public void Cartridge_stacks_open_only_inside_magazines()
        {
            Assert.True(CompatGate.ShouldForceAllowCartridgeStack(AllOn, hostIsMagazine: true));
            Assert.False(CompatGate.ShouldForceAllowCartridgeStack(AllOn, hostIsMagazine: false));
        }

        [Fact]
        public void Cartridge_stacks_stay_closed_when_the_toggle_is_off()
            => Assert.False(CompatGate.ShouldForceAllowCartridgeStack(AllOff, hostIsMagazine: true));
    }
}
