using EFT;
using pitTeam.Modules;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

using EventInfo = GlobalEventDispatcher.PhraseDelegateInfo;

namespace pitTeam.Patches
{
    // Route Stop through pitAIBossPlayer instead of vanilla BotReceiver.method_0 logic.
    internal class BotReceiverPhraseOverridePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotReceiver), nameof(BotReceiver.OnPhraseSay));
        }

        [PatchPrefix]
        private static bool PatchPrefix(BotReceiver __instance, EventInfo info)
        {
            if (info?.PlayerRequester == null)
            {
                return true;
            }

            BotOwner botOwner = __instance._owner;
            if (botOwner == null || !BossPlayers.IsFollower(botOwner))
            {
                return true;
            }

            if (!BossPlayers.IsPlayerBoss(info.PlayerRequester.ProfileId))
            {
                return true;
            }

            switch (info.phrase)
            {
                case EPhraseTrigger.Stop:
                case EPhraseTrigger.HoldPosition:
                case EPhraseTrigger.Gogogo:
                case EPhraseTrigger.Suppress:
                case EPhraseTrigger.NeedSniper:
                case EPhraseTrigger.NeedHelp:
                case EPhraseTrigger.OnYourOwn:
                case EPhraseTrigger.CoverMe:
                    return false;
                default:
                    // Fire / Hold Fire / Take Cover. DELIBERATE BEHAVIOR CHANGE: with default settings
                    // (hideUnsupportedCommands is false) these three were visible in the menu and vanilla
                    // BotReceiver acted on them for the player's squad. Where the fork now handles the command,
                    // its handler REPLACES vanilla's, because two systems acting on one command is worse than
                    // either - and for Hold Fire they pull in opposite directions.
                    // Vanilla is skipped ONLY where the fork handler will actually run for this follower - the
                    // same predicate the router uses. For any other follower (another boss's squad, an
                    // unregistered speaker) vanilla still runs, so the command never silently does nothing.
                    return !pitTeam.Components.pitAIBossPlayer.ForkHandlesEngagementFor(botOwner, info.PlayerRequester, info.phrase);
            }
        }
    }
}
