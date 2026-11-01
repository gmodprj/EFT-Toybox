using System;
using System.Collections.Generic;
using System.Linq;

namespace pitTeam.Shared
{
    // NotAMember = this Aid is not a registered launcher-profile teammate (a mercenary, a stranger, or nobody).
    // Ready = registered and usable. NotAvailable = registered but must not be taken (see Availability for why).
    public enum MemberLookupResult { NotAMember, Ready, NotAvailable }

    public sealed record MemberLookup(MemberLookupResult Result, string? ProfileId, ProfileAvailability? Availability);

    // The game names an invite/spawn target by numeric Aid; the registration store holds only ProfileIds.
    // This maps one to the other. Pure: no SPT types, never throws, and anything unclear fails closed.
    public static class ProfileMemberLookup
    {
        private static readonly MemberLookup None = new MemberLookup(MemberLookupResult.NotAMember, null, null);

        // Deliberately has NO catch-all. The guard clauses below are what make this never throw, and a
        // catch would make them unobservable: delete one and a test meant to pin it would still pass,
        // because the catch would swallow the throw and return the same None the guard returns.
        public static MemberLookup ByAid(int aid, IReadOnlyList<string> registeredProfileIds, IReadOnlyList<ProfileCandidate> candidates)
        {
            if (aid <= 0 || registeredProfileIds == null || candidates == null) return None;

            // Blank ids are dropped here, which is also why ProfileId can be dereferenced further down:
            // a trimmed non-blank id is never "", so "" is never in this set, so a candidate whose
            // ProfileId is null or blank can never reach `matches`.
            var registered = new HashSet<string>(
                registeredProfileIds.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()),
                StringComparer.Ordinal);
            // Fast path, not a safety guard: with an empty set nothing can match, so the
            // `matches.Count == 0` check below reaches the same None on its own.
            if (registered.Count == 0) return None;

            // An availability value this code does not know is never passed through as usable.
            static ProfileAvailability Known(ProfileAvailability a) =>
                Enum.IsDefined(typeof(ProfileAvailability), a) ? a : ProfileAvailability.Unreadable;

            // Candidates carrying this exact Aid; only registered ones can become a teammate.
            var sameAid = candidates.Where(c => c != null && c.Aid == aid).ToList();
            var matches = sameAid
                .Where(c => registered.Contains((c.ProfileId ?? string.Empty).Trim()))
                .ToList();
            if (matches.Count == 0) return None;

            // The game's Aid refers to the logged-in player when ANY profile with that Aid is the current
            // session (a copied profile file shares its Aid), registered or not. Never a teammate (spec 6-9).
            if (sameAid.Any(c => c.Availability == ProfileAvailability.CurrentSession))
            {
                var cur = matches.FirstOrDefault(c => c.Availability == ProfileAvailability.CurrentSession);
                return new MemberLookup(MemberLookupResult.NotAvailable, cur?.ProfileId.Trim(), ProfileAvailability.CurrentSession);
            }

            // Blocking precedence, most blocking first, so the reason never depends on input order.
            foreach (var blocker in new[] { ProfileAvailability.Unreadable, ProfileAvailability.Empty })
            {
                var hit = matches.FirstOrDefault(c => Known(c.Availability) == blocker);
                if (hit != null)
                    return new MemberLookup(MemberLookupResult.NotAvailable, hit.ProfileId.Trim(), blocker);
            }

            var ready = matches.Where(c => c.Availability == ProfileAvailability.Ready).ToList();
            if (ready.Count == 1)
                return new MemberLookup(MemberLookupResult.Ready, ready[0].ProfileId.Trim(), ProfileAvailability.Ready);

            // Two or more Ready profiles claim one Aid: the cause is an AMBIGUOUS Aid. There is no enum value
            // for that and adding one would change an interface Task 2 builds on, so it is deliberately
            // reported as Unreadable with no ProfileId - neither profile is taken.
            return new MemberLookup(MemberLookupResult.NotAvailable, null, ProfileAvailability.Unreadable);
        }
    }
}
