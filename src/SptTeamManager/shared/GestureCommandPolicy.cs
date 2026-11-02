using System;
using System.Linq;

namespace pitTeam.Shared
{
    // What a squad-command menu entry asks of a follower's engagement, as opposed to the menu's own wording.
    public sealed record GestureCommandEffect(string Engagement);

    // Decides which engagement behavior a squad-command menu entry means.
    //
    // This exists because the fork handled only 11 of the menu's 18 entries (WORKLOG, "제스처 명령 메뉴 대응
    // 범위"), and FIRE / HOLD FIRE / TAKE COVER are the three that control engagement - the first things
    // someone leading a squad wants to say once teammates are in the raid. The judgment of which entry means
    // which behavior lives here, where a test can hold it; the client only carries the result to the AI.
    //
    // Pure string work, no SPT types. This is an ALLOW LIST: an entry not named below translates to null and
    // changes nothing. A command this policy does not know must never fall through to a nearby guess - a
    // follower that obeys the wrong order is worse than one that ignores an order.
    public static class GestureCommandPolicy
    {
        // The behaviors. Kept as literals, like FollowerAiProfilePolicy's side names, so this decision does
        // not drag SPT's enums into the shared project. The client maps each to its AI lever.
        public const string WeaponsFree = "WeaponsFree";
        public const string HoldFire = "HoldFire";
        public const string SeekCover = "SeekCover";

        // Menu labels as the game shows them (WORKLOG table). Compared with whitespace removed, so the
        // enum-style spelling "HOLDFIRE" and the label "HOLD FIRE" are the same entry.
        public const string FireCommand = "FIRE";
        public const string HoldFireCommand = "HOLD FIRE";
        public const string TakeCoverCommand = "TAKE COVER";

        public static GestureCommandEffect? Translate(string? menuCommand)
        {
            var key = Normalize(menuCommand);
            if (key.Length == 0)
            {
                return null;
            }

            if (key == Normalize(FireCommand))
            {
                return new GestureCommandEffect(WeaponsFree);
            }

            if (key == Normalize(HoldFireCommand))
            {
                return new GestureCommandEffect(HoldFire);
            }

            if (key == Normalize(TakeCoverCommand))
            {
                return new GestureCommandEffect(SeekCover);
            }

            return null;
        }

        private static string Normalize(string? command)
        {
            if (command == null)
            {
                return string.Empty;
            }

            return new string(command.Where(c => !char.IsWhiteSpace(c)).ToArray())
                .ToUpperInvariant();
        }
    }
}
