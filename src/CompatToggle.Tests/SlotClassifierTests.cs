using CompatToggle;
using Xunit;

namespace CompatToggle.Tests
{
    public class SlotClassifierTests
    {
        [Theory]
        [InlineData("patron_in_weapon", SlotKind.Chamber)]
        [InlineData("patron_in_weapon_000", SlotKind.Chamber)]
        [InlineData("camora_000", SlotKind.Chamber)]
        [InlineData("mod_magazine", SlotKind.Magazine)]
        public void Ammo_and_magazine_slots_are_classified_by_id(string id, SlotKind expected)
            => Assert.Equal(expected, SlotClassifier.ClassifyById(id, parentIsWeapon: true, parentIsMod: false));

        [Fact]
        public void Mod_slot_on_a_weapon_is_weapon_body()
            => Assert.Equal(SlotKind.WeaponBody,
                SlotClassifier.ClassifyById("mod_scope", parentIsWeapon: true, parentIsMod: false));

        [Fact]
        public void Mod_slot_on_a_mod_is_attachment()
            => Assert.Equal(SlotKind.Attachment,
                SlotClassifier.ClassifyById("mod_scope", parentIsWeapon: false, parentIsMod: true));

        [Theory]
        [InlineData("Headwear")]
        [InlineData("SecuredContainer")]
        [InlineData("TacticalVest")]
        [InlineData("Helmet_top")]
        public void Equipment_and_container_slots_are_other(string id)
            => Assert.Equal(SlotKind.Other,
                SlotClassifier.ClassifyById(id, parentIsWeapon: false, parentIsMod: false));

        [Fact]
        public void Unknown_parent_is_other()
            => Assert.Equal(SlotKind.Other,
                SlotClassifier.ClassifyById("mod_scope", parentIsWeapon: false, parentIsMod: false));

        [Fact]
        public void Null_or_empty_id_is_other()
        {
            Assert.Equal(SlotKind.Other, SlotClassifier.ClassifyById(null, true, false));
            Assert.Equal(SlotKind.Other, SlotClassifier.ClassifyById("", true, false));
        }
    }
}
