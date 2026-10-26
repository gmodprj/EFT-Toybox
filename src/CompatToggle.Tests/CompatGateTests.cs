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
            Assert.False(CompatGate.ShouldIgnoreConflict(t, subjectIsWeaponOrMod: true));
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
            Assert.True(CompatGate.ShouldIgnoreConflict(AllOn, subjectIsWeaponOrMod: true));
            Assert.False(CompatGate.ShouldIgnoreConflict(AllOn, subjectIsWeaponOrMod: false));
        }

        [Fact]
        public void Conflicts_off_ignores_nothing()
            => Assert.False(CompatGate.ShouldIgnoreConflict(AllOff, subjectIsWeaponOrMod: true));
    }
}
