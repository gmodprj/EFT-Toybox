using System;

namespace pitTeam.Shared
{
    // What a profile teammate must look like to the raid's AI, as opposed to what the launcher profile
    // happens to store.
    public sealed record FollowerAiProfile(string Role, string Difficulty);

    // Decides the AI role and difficulty a launcher profile spawns with.
    //
    // This exists because a launcher profile stores Settings.Role = "assault" and BotDifficulty = "easy".
    // Those are meaningless while the person plays that profile themselves, and SAIN reads them literally
    // once the same character spawns as somebody's follower: measured 2026-11-01, profile teammates ran
    // profile=0.15 visibleDistance=90 against a mercenary's 1.5 / 225, and did not fight. The factory used
    // to fill Role only when blank - which it never is - so the correction never ran. Two in-game raids
    // were spent finding that, which is why the decision now lives here, where a test can hold it.
    //
    // Pure string work, no SPT types. Deliberately has NO catch-all: the guard below covers every input
    // the signature allows, and a catch would make it unobservable.
    public static class FollowerAiProfilePolicy
    {
        // PIT gives every mercenary it generates this difficulty; a profile teammate fights beside them
        // and gets the same. See StmSpawnProfileFactory for the evidence behind the value.
        public const string Difficulty = "hard";

        // SPT's Sides values. Kept as literals for the same reason SpawnGearPolicy keeps its slot names:
        // referencing the SPT enum would drag this decision back into the server project, where it could
        // not be unit-tested. The two must agree, so change both or neither.
        public const string Usec = "Usec";
        public const string Bear = "Bear";
        public const string PmcUsec = "pmcUSEC";
        public const string PmcBear = "pmcBEAR";

        // Throws on a side this policy does not know, rather than guessing a role: a follower spawned
        // under the wrong faction's role is the failure this whole policy exists to prevent.
        public static FollowerAiProfile ForSide(string? side)
        {
            var trimmed = side?.Trim();
            if (string.Equals(trimmed, Usec, StringComparison.OrdinalIgnoreCase))
            {
                return new FollowerAiProfile(PmcUsec, Difficulty);
            }

            if (string.Equals(trimmed, Bear, StringComparison.OrdinalIgnoreCase))
            {
                return new FollowerAiProfile(PmcBear, Difficulty);
            }

            throw new ArgumentException($"Unsupported profile teammate side '{side}'.", nameof(side));
        }
    }
}
