namespace pitTeam.Shared
{
    // Allowed == true means RejectionReason is null; Allowed == false always carries text the player can read.
    public sealed record SlotDecision(bool Allowed, string? RejectionReason);

    // Not called from the invite path yet, on purpose - do not delete it as unused. The native party
    // window holds 4 total (the player plus 3 teammates), and profile members and mercenaries share
    // those 3 seats. The SPT server has no party-membership signal: invite/send carries only
    // {To, InLobby} and there is no accept/leave/kick route, so the cap cannot be evaluated server-side
    // today. This is the gate for when a client->server composition report exists. Never invent a
    // count from invite traffic - a stale count would silently refuse legitimate mercenary invites.
    // The cap is a native limit, not a tunable - do not make it a setting.
    // In SPT a native friend never joins a raid party: a real profile's friends array is empty
    // (doc/specs/2026-10-31-spt-team-manager-design.md section 5-3 measured it as []) and Fika
    // compatibility is a non-goal (same document, section 3). So FriendKind.Native exists to keep native
    // friend-list entries from being erased by the merge; it does not mean a native player takes one of
    // the party seats. Do not size the cap on that premise.
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

            // Native joins are not metered: in SPT no native friend takes a party seat (see the class
            // comment). This policy only meters the AI teammate seats, so a native join is never counted
            // against (or refused by) those 3.
            if (joining == FriendKind.Native)
            {
                return new SlotDecision(true, null);
            }

            // long: two large int counts must not wrap negative and slip under the cap.
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
