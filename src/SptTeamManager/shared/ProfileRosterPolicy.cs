using System;
using System.Collections.Generic;
using System.Linq;

namespace pitTeam.Shared
{
    public enum ProfileAvailability { Ready, Empty, Unreadable, CurrentSession }

    // Plain-data boundary: the server fills it from SaveServer so this file never references SPT types.
    public sealed record ProfileSnapshot(string ProfileId, int Aid, string? Nickname, int? Level, string? Side, bool ReadFailed);

    public sealed record ProfileCandidate(string ProfileId, int Aid, string Nickname, int Level, string Side, ProfileAvailability Availability);

    // Roster-screen view of one REGISTERED profile: Missing = registered but no longer in the catalog.
    public enum RosterRowStatus { Ready, Empty, Unreadable, CurrentSession, Missing }

    public sealed record RegisteredProfileRow(string ProfileId, int Aid, string Nickname, int Level, string Side, RosterRowStatus Status);

    // Decides which launcher profiles may be offered as teammates. Only Ready may join a team;
    // every other availability stays listed so the user can see why it is blocked.
    public static class ProfileRosterPolicy
    {
        public static IReadOnlyList<ProfileCandidate> Select(IEnumerable<ProfileSnapshot> all, string currentSessionId)
        {
            var result = new List<ProfileCandidate>();
            if (all == null) return result;
            // A blank session id means we cannot tell who is logged in, so availability cannot be
            // determined and nothing may be selectable: every candidate is blocked as Unreadable.
            bool sessionKnown = !string.IsNullOrWhiteSpace(currentSessionId);
            string session = sessionKnown ? currentSessionId.Trim() : string.Empty;
            foreach (var snapshot in all)
            {
                if (snapshot == null) continue;
                result.Add(Classify(snapshot, session, sessionKnown));
            }
            return result
                .OrderBy(c => c.Nickname, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.Nickname, StringComparer.Ordinal)
                .ThenBy(c => c.ProfileId, StringComparer.Ordinal)
                .ToList();
        }

        // Registered profiles that may actually be offered: joined against the catalog's candidates
        // by ProfileId, Ready only. A registered id missing from the catalog (deleted profile) or
        // CurrentSession/Empty/Unreadable yields nothing - never a guess.
        public static IReadOnlyList<ProfileCandidate> SelectRegisteredReady(IEnumerable<ProfileCandidate> candidates, IEnumerable<string> registeredProfileIds)
        {
            var result = new List<ProfileCandidate>();
            if (candidates == null || registeredProfileIds == null) return result;
            var registered = new HashSet<string>(
                registeredProfileIds.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()),
                StringComparer.Ordinal);
            foreach (var c in candidates)
            {
                if (c == null || c.Availability != ProfileAvailability.Ready) continue;
                if (registered.Contains((c.ProfileId ?? string.Empty).Trim())) result.Add(c);
            }
            return result;
        }

        // Display-time join for the roster screen: every REGISTERED id, in registration order, with what the
        // catalog says about it right now. Nothing here is stored. A registered id that the catalog no longer
        // lists (profile deleted) is reported as Missing - shown greyed with a reason, never hidden and
        // never guessed to be fine. Duplicate/blank registered ids collapse/skip. Anything not Ready is not offered.
        public static IReadOnlyList<RegisteredProfileRow> JoinRegistered(IEnumerable<ProfileCandidate> candidates, IEnumerable<string> registeredProfileIds)
        {
            var rows = new List<RegisteredProfileRow>();
            if (registeredProfileIds == null) return rows;
            var byId = new Dictionary<string, ProfileCandidate>(StringComparer.Ordinal);
            if (candidates != null)
            {
                foreach (var c in candidates)
                {
                    if (c == null) continue;
                    var key = (c.ProfileId ?? string.Empty).Trim();
                    if (key.Length == 0 || byId.ContainsKey(key)) continue;
                    byId[key] = c;
                }
            }
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var raw in registeredProfileIds)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                var id = raw.Trim();
                if (!seen.Add(id)) continue;
                if (byId.TryGetValue(id, out var c))
                    rows.Add(new RegisteredProfileRow(id, c.Aid, c.Nickname, c.Level, c.Side, ToStatus(c.Availability)));
                else
                    rows.Add(new RegisteredProfileRow(id, 0, DisplayName(null, id), 0, string.Empty, RosterRowStatus.Missing));
            }
            return rows;
        }

        // Profiles that may be newly registered: Ready and not registered yet.
        public static IReadOnlyList<ProfileCandidate> SelectUnregisteredReady(IEnumerable<ProfileCandidate> candidates, IEnumerable<string> registeredProfileIds)
        {
            var result = new List<ProfileCandidate>();
            if (candidates == null) return result;
            var registered = new HashSet<string>(
                (registeredProfileIds ?? Array.Empty<string>()).Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()),
                StringComparer.Ordinal);
            foreach (var c in candidates)
            {
                if (c == null || c.Availability != ProfileAvailability.Ready) continue;
                if (!registered.Contains((c.ProfileId ?? string.Empty).Trim())) result.Add(c);
            }
            return result;
        }

        private static RosterRowStatus ToStatus(ProfileAvailability a) => a switch
        {
            ProfileAvailability.Ready => RosterRowStatus.Ready,
            ProfileAvailability.Empty => RosterRowStatus.Empty,
            ProfileAvailability.CurrentSession => RosterRowStatus.CurrentSession,
            // Unreadable and any value this code does not know fail closed.
            _ => RosterRowStatus.Unreadable,
        };

        private static ProfileCandidate Classify(ProfileSnapshot s, string session, bool sessionKnown)
        {
            string id = s.ProfileId ?? string.Empty;
            string display = DisplayName(s.Nickname, id);

            // Order: current session -> read failure -> no Info -> Ready. Anything unclear fails closed.
            if (!sessionKnown)
                return Blocked(s, id, display, ProfileAvailability.Unreadable);
            if (string.Equals(id.Trim(), session, StringComparison.OrdinalIgnoreCase))
                return Blocked(s, id, display, ProfileAvailability.CurrentSession);
            if (s.ReadFailed)
                return Blocked(s, id, display, ProfileAvailability.Unreadable);
            if (s.Level == null || string.IsNullOrWhiteSpace(s.Side))
                return Blocked(s, id, display, ProfileAvailability.Empty);

            return new ProfileCandidate(id, s.Aid, display, s.Level.Value, s.Side!, ProfileAvailability.Ready);
        }

        // Unread values are not backfilled with plausible ones.
        private static ProfileCandidate Blocked(ProfileSnapshot s, string id, string display, ProfileAvailability availability) =>
            new ProfileCandidate(id, s.Aid, display, 0, string.Empty, availability);

        private static string DisplayName(string? nickname, string id)
        {
            if (!string.IsNullOrWhiteSpace(nickname)) return nickname!.Trim();
            return id.Length <= 8 ? id : id.Substring(0, 8);
        }
    }
}
