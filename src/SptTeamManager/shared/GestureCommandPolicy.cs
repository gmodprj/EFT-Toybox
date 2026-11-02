using System;

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

        // Menu labels as the game shows them (WORKLOG table).
        public const string FireCommand = "FIRE";
        public const string HoldFireCommand = "HOLD FIRE";
        public const string TakeCoverCommand = "TAKE COVER";

        // The phrases the COMMAND group of the squad-command menu shows, by EPhraseTrigger name, in menu
        // order. This is the ONE list of what the menu offers: the menu filter reads it instead of keeping
        // its own, so the menu and the translation cannot drift apart. The first ten are handled by the
        // fork's PhraseSaid router in their own branches; the last three are the ones Translate covers.
        // Not listed (and so hidden): Silence, KnifesOnly, Spreadout, GetBack.
        public static readonly string[] CommandGroupPhrases =
        {
            "Suppress", "Regroup", "HoldPosition", "Look", "Gogogo", "GoForward", "FollowMe", "CoverMe",
            "Stop", "OnYourOwn",
            "Fire", "HoldFire", "GetInCover",
        };

        // Allow list: a phrase not named above is not supported, whatever it is called.
        public static bool IsSupportedPhrase(string? phraseName)
        {
            return phraseName != null && Array.IndexOf(CommandGroupPhrases, phraseName) >= 0;
        }

        // The menu label of the phrases Translate covers, from the game's phrase name; null for any other.
        public static string? MenuCommandForPhrase(string? phraseName)
        {
            switch (phraseName)
            {
                case "Fire": return FireCommand;
                case "HoldFire": return HoldFireCommand;
                case "GetInCover": return TakeCoverCommand;
                default: return null;
            }
        }

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

        // Case and surrounding whitespace do not matter, and a run of whitespace between words counts as one.
        // Words are never split or joined: "F I R E" and "HOLDFIRE" are not menu spellings, and accepting them
        // would widen the allow list past what the menu can actually send.
        private static string Normalize(string? command)
        {
            if (command == null)
            {
                return string.Empty;
            }

            return string.Join(" ", command.Split(new char[0], StringSplitOptions.RemoveEmptyEntries))
                .ToUpperInvariant();
        }
    }
}
