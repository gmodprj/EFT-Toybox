using System;

namespace pitTeam.Shared
{
    // Allowed == true means RejectionReason is null; Allowed == false always carries text the player can read.
    public sealed record SlotDecision(bool Allowed, string? RejectionReason);

    // The native party window holds 4 total: the player plus 3 teammates. Profile members and
    // mercenaries share those 3 seats. The cap is a native limit, not a tunable - do not make it a setting.
    public static class PartySlotPolicy
    {
        public const int MaxTeammateSlots = 3;

        public static SlotDecision Admit(int mercenariesInParty, int profileMembersInParty, FriendKind joining)
        {
            // A count that cannot be right means we cannot judge; refuse rather than admit blindly.
            if (mercenariesInParty < 0 || profileMembersInParty < 0)
            {
                return new SlotDecision(false, "Cannot add this teammate: the current party size could not be determined.");
            }

            // Native friends are real players and never use the AI teammate seats.
            if (joining == FriendKind.Native)
            {
                return new SlotDecision(true, null);
            }

            long occupied = (long)mercenariesInParty + profileMembersInParty;
            if (occupied >= MaxTeammateSlots)
            {
                return new SlotDecision(false,
                    $"Cannot add another teammate: the party already has {MaxTeammateSlots} teammates, which is the most a raid party can hold.");
            }

            return new SlotDecision(true, null);
        }
    }
}
