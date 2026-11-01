using System;
using System.Collections.Generic;

namespace pitTeam.Shared
{
    // Decides WHICH raid-entry protections may be released once a profile teammate's raid result has
    // been applied, so that gear the player looted off a teammate's corpse survives the raid end.
    //
    // Why this is a judgment and not a formality. At raid entry every item of a profile teammate's
    // spawn inventory is registered as protected, which makes PIT's
    // RemoveProtectedTeammateItemsFromExtractedProfile strip those ids from what the player carried out.
    // While the items still exist in the teammate's profile that is exactly right - delivering them
    // would copy an item rather than move it. Once they have been DELETED from that profile the same
    // protection turns destructive, and only then may it be lifted. Both directions were measured in
    // game on 2026-11-01 (doc/WORKLOG.md):
    //   - release too much: a surviving teammate keeps its gear in its profile, so releasing it leaves
    //     one rifle existing in two places;
    //   - release a dead Temporary teammate's loadout: that kit belonged to no profile at all, so
    //     releasing it turns an unequipped profile into an endless gear tap - invite it bare, let it
    //     die, loot the corpse, repeat;
    //   - release nothing when something WAS deleted: 35 items left a teammate's profile and 0 of them
    //     reached the player's. All destroyed.
    // So the scope is exactly the set that was just deleted, and nothing else. This file exists because
    // that sentence is the kind of thing a later reader "simplifies".
    //
    // Pure set work over primitives, no SPT types - which is why it lives here, where a test can hold
    // it, while the release mechanism stays in StmRaidOutcomeService. Deliberately has NO catch-all:
    // the guards below cover every input the signature allows, and a catch would make them unobservable.
    public static class RaidProtectionReleasePolicy
    {
        // The ids whose protection may be lifted. Empty means "release nothing", which is the closed
        // side on every path but one.
        //
        // escaped must mean "survived or extracted"; gearKind is null when there is no record of what
        // the teammate went in with (a server restart between spawn and raid end). deletedIds is what
        // was ACTUALLY deleted from the stored profile - not what a policy planned to delete - because
        // entitlement follows the deletion, not the intention.
        //
        // A SET, with the same Ordinal comparison WornGearLossPolicy's id handling uses: ids are not
        // trimmed and case is not ignored, because a looser match here would release an id that was
        // never deleted.
        public static HashSet<string> SelectReleasableIds(SpawnGearKind? gearKind, bool escaped, IEnumerable<string>? deletedIds)
        {
            var releasable = new HashSet<string>(StringComparer.Ordinal);

            // No record of the loadout: nothing was deleted on a guess, so nothing is released on one.
            if (gearKind == null)
            {
                return releasable;
            }

            // Still the teammate's property, wherever the teammate ended up.
            if (escaped)
            {
                return releasable;
            }

            // A generated loadout is nobody's property, so there is nothing to hand over - and handing
            // it over anyway is the endless gear tap above. Matched explicitly rather than by
            // elimination, so a SpawnGearKind member added later releases nothing either.
            if (gearKind != SpawnGearKind.Real)
            {
                return releasable;
            }

            if (deletedIds == null)
            {
                return releasable;
            }

            foreach (var id in deletedIds)
            {
                // A blank id names nothing; releasing it would be a no-op at best and is not a
                // protection anyone registered.
                if (string.IsNullOrWhiteSpace(id)) continue;
                releasable.Add(id!);
            }

            return releasable;
        }
    }
}
