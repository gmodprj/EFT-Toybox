namespace pitTeam.Shared
{
    // Real = the launcher profile's own gear goes into the raid (and can be lost there).
    // Temporary = a generated loadout goes instead, so an unequipped profile is still playable.
    public enum SpawnGearKind { Real, Temporary }

    // Reason is one human-readable line; it goes into logs and player-facing messages.
    public sealed record GearDecision(SpawnGearKind Kind, string Reason);

    // Pure: takes two booleans the caller reads off the PMC inventory, so this file stays free of
    // SPT types and compiles for every target. Who computes the booleans is the factory's business.
    public static class SpawnGearPolicy
    {
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
