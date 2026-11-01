using System;

namespace pitTeam.Shared
{
    // Judges the idempotence key of a raid receipt: raid id + teammate profile id, nothing else.
    // Pure string work over primitives. Deliberately has NO catch-all: nothing here can throw, and a
    // catch would make the guards unobservable (deleting a guard would leave its test passing).
    public static class RaidReceiptKeyPolicy
    {
        // A key half that is null, empty or whitespace identifies nothing, so it can never be proven "not applied".
        public static bool IsUsableKey(string? raidId, string? profileId) =>
            !string.IsNullOrWhiteSpace(raidId) && !string.IsNullOrWhiteSpace(profileId);

        // Surrounding whitespace is ignored and case is ignored, on BOTH halves. The direction matters:
        // a near miss that fails to match reads as "not applied" and would delete gear a second time,
        // so doubtful equality must resolve to "same key". An unusable key on either side never matches.
        public static bool KeysMatch(string? leftRaidId, string? leftProfileId, string? rightRaidId, string? rightProfileId)
        {
            if (!IsUsableKey(leftRaidId, leftProfileId) || !IsUsableKey(rightRaidId, rightProfileId))
            {
                return false;
            }

            return string.Equals(leftRaidId!.Trim(), rightRaidId!.Trim(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(leftProfileId!.Trim(), rightProfileId!.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
