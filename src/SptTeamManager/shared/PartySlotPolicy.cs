using System;

namespace pitTeam.Shared
{
    // Allowed == true means RejectionReason is null; Allowed == false always carries text the player can read.
    public sealed record SlotDecision(bool Allowed, string? RejectionReason);

    // Not called from the invite path yet, on purpose. The native party window caps the party at 4
    // (player + 3), but the SPT server has no party-membership signal: invite/send carries only
    // {To, InLobby} and there is no accept/leave/kick route, so the cap cannot be evaluated server-side
    // today. This is the gate for when a client->server composition report exists. Never invent a
    // count from invite traffic - a stale count would silently refuse legitimate mercenary invites.
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
