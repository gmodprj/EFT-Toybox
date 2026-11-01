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

        public static MemberLookup ByAid(int aid, IReadOnlyList<string> registeredProfileIds, IReadOnlyList<ProfileCandidate> candidates)
        {
            try
            {
                if (aid <= 0 || registeredProfileIds == null || candidates == null) return None;

                var registered = new HashSet<string>(
                    registeredProfileIds.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()),
                    StringComparer.Ordinal);
                if (registered.Count == 0) return None;

                // Only registered candidates carrying this exact Aid are in play.
                var matches = candidates
                    .Where(c => c != null && c.Aid == aid && registered.Contains((c.ProfileId ?? string.Empty).Trim()))
                    .ToList();
                if (matches.Count == 0) return None;

                var ready = matches.Where(c => c.Availability == ProfileAvailability.Ready).ToList();
                if (ready.Count == 1)
                    return new MemberLookup(MemberLookupResult.Ready, ready[0].ProfileId.Trim(), ProfileAvailability.Ready);

                // Two Ready profiles claiming one Aid: we cannot tell which one is meant, so take neither.
                if (ready.Count > 1)
                    return new MemberLookup(MemberLookupResult.NotAvailable, null, ProfileAvailability.Unreadable);

                // Not Ready anywhere: report the first match's own availability so the caller can say why.
                // An availability value this code does not know is not passed through as usable.
                var first = matches[0];
                var why = Enum.IsDefined(typeof(ProfileAvailability), first.Availability) ? first.Availability : ProfileAvailability.Unreadable;
                return new MemberLookup(MemberLookupResult.NotAvailable, first.ProfileId.Trim(), why);
            }
            catch (Exception)
            {
                return None;
            }
        }
    }
}
