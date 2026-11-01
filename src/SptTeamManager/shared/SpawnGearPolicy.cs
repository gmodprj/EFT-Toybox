using System;
using System.Collections.Generic;

namespace pitTeam.Shared
{
    // Real = the launcher profile's own gear goes into the raid (and can be lost there).
    // Temporary = a generated loadout goes instead, so an unequipped profile is still playable.
    public enum SpawnGearKind { Real, Temporary }

    // Reason is one human-readable line; it goes into logs and player-facing messages.
    public sealed record GearDecision(SpawnGearKind Kind, string Reason);

    public sealed record GearFacts(bool HasAnyEquipment, bool HasWeaponInRequiredSlot);

    // Pure string work, which is why this lives outside the server project: both the judgment about
    // which slots count and the decision itself stay unit-testable without SPT types. The caller
    // supplies the names of the PMC's occupied equipment slots; nothing here touches a profile.
    public static class SpawnGearPolicy
    {
        private static readonly GearFacts NoGear = new GearFacts(false, false);

        // Mirrors the names of SPT's EquipmentSlots enum. The factory derives its own reading filter
        // from that enum (SPT types belong there); this set is the policy's own vocabulary, so a slot
        // name the policy does not recognize is never counted - the closed side.
        // Dogtag is deliberately absent: it is not an EquipmentSlots value at all.
        private static readonly HashSet<string> KnownSlots = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Headwear", "Earpiece", "FaceCover", "ArmorVest", "Eyewear", "ArmBand", "TacticalVest",
            "Pockets", "Backpack", "SecuredContainer", "FirstPrimaryWeapon", "SecondPrimaryWeapon",
            "Holster", "Scabbard",
        };

        // Slots every PMC profile carries without the player having equipped anything. Counting these
        // as worn equipment would keep HasAnyEquipment true for every profile in existence, which
        // would collapse Decide onto the weapon check and leave its no-equipment branch unreachable.
        private static readonly HashSet<string> DefaultSlots = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Pockets", "SecuredContainer", "ArmBand", "Scabbard",
        };

        // 필수 무기 슬롯: any one of these holding something satisfies it. A knife in Scabbard does not.
        private static readonly HashSet<string> RequiredWeaponSlots = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "FirstPrimaryWeapon", "SecondPrimaryWeapon", "Holster",
        };

        // Trims its input and tolerates null, like ProfileMemberLookup.ByAid. Deliberately has NO
        // catch-all: the two guards below cover every throw an IEnumerable<string>? can reach here,
        // and a catch would make them unobservable - delete either guard and a test that is supposed
        // to pin it would still pass, because the catch would swallow the throw and return NoGear.
        public static GearFacts FromOccupiedSlots(IEnumerable<string>? slotNames)
        {
            if (slotNames == null) return NoGear;

            var hasAnyEquipment = false;
            var hasWeaponInRequiredSlot = false;

            foreach (var rawSlot in slotNames)
            {
                if (string.IsNullOrWhiteSpace(rawSlot)) continue;

                var slot = rawSlot.Trim();
                if (!KnownSlots.Contains(slot)) continue;

                if (RequiredWeaponSlots.Contains(slot)) hasWeaponInRequiredSlot = true;
                if (!DefaultSlots.Contains(slot)) hasAnyEquipment = true;
            }

            return new GearFacts(hasAnyEquipment, hasWeaponInRequiredSlot);
        }

        public static GearDecision Decide(bool hasAnyEquipment, bool hasWeaponInRequiredSlot)
        {
            // Nothing worn: sending the profile in as-is would be a naked teammate.
            if (!hasAnyEquipment)
            {
                return new GearDecision(SpawnGearKind.Temporary, "착용한 장비가 하나도 없어 임시 장비로 내보낸다.");
            }

            // Gear but no gun (a knife-only profile lands here): still not raid-ready.
            if (!hasWeaponInRequiredSlot)
            {
                return new GearDecision(SpawnGearKind.Temporary, "필수 무기 슬롯(주무기1·주무기2·권총집)에 무기가 없어 임시 장비로 내보낸다.");
            }

            return new GearDecision(SpawnGearKind.Real, "착용 장비와 무기가 모두 있어 프로필의 실물 장비로 내보낸다.");
        }
    }
}
