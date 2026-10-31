using System;
using System.Collections.Generic;
using System.Linq;

namespace pitTeam.Shared
{
    // Native = friends the game/SPT already put in the response, Mercenary = PIT's fabricated
    // teammates, ProfileMember = real launcher profiles registered as teammates (STM).
    public enum FriendKind { Native, Mercenary, ProfileMember }

    // Only what the duplicate decision needs. The caller keeps the real response objects and
    // finds them again by Id; never rebuild a response object from this record.
    public sealed record FriendEntry(string Id, int Aid, FriendKind Kind);

    public sealed record MergeOutcome(IReadOnlyList<FriendEntry> Final, IReadOnlyList<(FriendEntry Dropped, string Reason)> Skipped);

    // Decides which incoming friends may join the list. Priority: Native > ProfileMember > Mercenary.
    // PIT gives mercenaries fabricated Aids that can collide with a real profile's Aid, so on any
    // Id/Aid clash the lower-priority side is the one dropped - a real profile never loses to a mercenary.
    public static class FriendListMergePolicy
    {
        public static MergeOutcome Plan(IReadOnlyList<FriendEntry> existing, IReadOnlyList<FriendEntry> incoming)
        {
            existing ??= Array.Empty<FriendEntry>();
            incoming ??= Array.Empty<FriendEntry>();

            // Existing entries are never removed, whatever they collide with among themselves.
            var final = new List<FriendEntry>(existing);
            var skipped = new List<(FriendEntry Dropped, string Reason)>();
            var seenIds = new Dictionary<string, FriendEntry>(StringComparer.Ordinal);
            var seenAids = new Dictionary<int, FriendEntry>();
            foreach (var e in existing)
            {
                if (e.Id != null && !seenIds.ContainsKey(e.Id)) seenIds[e.Id] = e;
                if (e.Aid != 0 && !seenAids.ContainsKey(e.Aid)) seenAids[e.Aid] = e;
            }

            // Decide in priority order (stable within a rank), then emit accepted entries in incoming order.
            var accepted = new HashSet<int>();
            foreach (var index in Enumerable.Range(0, incoming.Count).OrderBy(i => Rank(incoming[i].Kind)))
            {
                var entry = incoming[index];
                if (entry == null) continue;
                if (entry.Id != null && seenIds.TryGetValue(entry.Id, out var sameId))
                {
                    skipped.Add((entry, $"same Id as {sameId.Kind} entry '{sameId.Id}'"));
                    continue;
                }
                if (entry.Aid != 0 && seenAids.TryGetValue(entry.Aid, out var sameAid))
                {
                    skipped.Add((entry, $"same Aid {entry.Aid} as {sameAid.Kind} entry '{sameAid.Id}'"));
                    continue;
                }
                accepted.Add(index);
                if (entry.Id != null) seenIds[entry.Id] = entry;
                if (entry.Aid != 0) seenAids[entry.Aid] = entry;
            }

            for (int i = 0; i < incoming.Count; i++)
                if (accepted.Contains(i)) final.Add(incoming[i]);

            return new MergeOutcome(final, skipped);
        }

        private static int Rank(FriendKind kind) => kind switch
        {
            FriendKind.Native => 0,
            FriendKind.ProfileMember => 1,
            _ => 2,
        };
    }
}
