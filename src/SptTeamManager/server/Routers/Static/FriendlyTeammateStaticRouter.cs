using pitTeam.Server.Callbacks;
using pitTeam.Server.Services;
using pitTeam.Server.Models;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Insurance;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Utils;

namespace pitTeam.Server.Routers.Static;

[Injectable(TypePriority = OnLoadOrder.Routers + 1)]
public class FriendlyTeammateStaticRouter(JsonUtil jsonUtil, FriendlyTeammateCallbacks callbacks, FriendlyTeammateStorage storage, StmProfileMemberCallbacks stmMembers)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/client/game/start",
                (url, info, sessionId, output, cancellationToken) => FriendlyModeRequestGate.Run(() =>
                {
                    FriendlyModeRequestGate.EndRaid(sessionId.ToString());
                    storage.InitializeProfile(sessionId);
                    return new ValueTask<string>(output!);
                })
            ),
            new RouteAction<GetInsuranceCostRequestData>(
                "/client/insurance/items/list/cost",
                async (url, info, sessionId, output, cancellationToken) =>
                    await FriendlyModeRequestGate.Run(() => callbacks.AugmentInsuranceCosts(url, info, sessionId, output))
            ),
            new RouteAction<FriendlyTeammateCreateRequest>(
                "/singleplayer/pitfireteam/teammate/prepare",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.PrepareCreation(url, info, sessionId))
            ),
            new RouteAction<FriendlyTeammateCreateRequest>(
                "/singleplayer/pitfireteam/teammate/cancel",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.CancelCreation(url, info, sessionId))
            ),
            new RouteAction<FriendlyTeammateCreateRequest>(
                "/singleplayer/pitfireteam/teammate/create",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.Create(url, info, sessionId))
            ),
            new RouteAction<EmptyRequestData>(
                "/singleplayer/pitfireteam/teammates",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.List(url, info, sessionId))
            ),
            new RouteAction<EmptyRequestData>(
                "/singleplayer/autoteam",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.ListAutoJoin(url, info, sessionId))
            ),
            new RouteAction<EmptyRequestData>(
                "/singleplayer/pitfireteam/gameplay-mode",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.GetGameplayMode(url, info, sessionId))
            ),
            new RouteAction<FriendlyServerSettingsRequest>(
                "/singleplayer/pitfireteam/settings",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.SetServerSettings(url, info, sessionId))
            ),
            new RouteAction<EmptyRequestData>(
                "/singleplayer/pitfireteam/lostondeath",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.GetLostOnDeathSettings(url, info, sessionId))
            ),
            new RouteAction<EmptyRequestData>(
                "/singleplayer/pitfireteam/recovery-notice",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.GetStartupRecoveryNotice(url, info, sessionId))
            ),
            new RouteAction<EmptyRequestData>(
                "/singleplayer/pitfireteam/recovery-notice/ack",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.AcknowledgeStartupRecoveryNotice(url, info, sessionId))
            ),
            new RouteAction<GetOtherProfileRequest>(
                "/singleplayer/pitfireteam/teammate/profile",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.GetProfile(url, info, sessionId))
            ),
            new RouteAction<FriendlyTeammateProfileOptionsRequest>(
                "/singleplayer/pitfireteam/teammate/profile/options",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.GetProfileOptions(url, info, sessionId))
            ),
            new RouteAction<FriendlyTeammateSuitRequest>(
                "/singleplayer/pitfireteam/teammate/profile/suit",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.SetSuit(url, info, sessionId))
            ),
            new RouteAction<FriendlyTeammateRenameRequest>(
                "/singleplayer/pitfireteam/teammate/profile/rename",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.Rename(url, info, sessionId))
            ),
            new RouteAction<FriendlyTeammateLoadoutRequest>(
                "/singleplayer/pitfireteam/teammate/profile/loadout",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.SetLoadout(url, info, sessionId))
            ),
            new RouteAction<FriendlyTeammateDefaultEquipmentRequest>(
                "/singleplayer/pitfireteam/teammate/profile/default-equipment",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.SaveDefaultEquipment(url, info, sessionId))
            ),
            new RouteAction<FriendlyTeammateBuyKitRequest>(
                "/singleplayer/pitfireteam/teammate/profile/buy-kit",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.BuyKit(url, info, sessionId))
            ),
            new RouteAction<FriendlyTeammateRepairEquipmentRequest>(
                "/singleplayer/pitfireteam/teammate/profile/repair-equipment",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.RepairDefaultEquipment(url, info, sessionId))
            ),
            new RouteAction<FriendlyTeammateInsuranceRequest>(
                "/singleplayer/pitfireteam/teammate/profile/insurance",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.InsureEquipment(url, info, sessionId))
            ),
            new RouteAction<FriendlyTeammateAggressionRequest>(
                "/singleplayer/pitfireteam/teammate/profile/aggression",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.SetAggression(url, info, sessionId))
            ),
            new RouteAction<FriendlyTeammateProficiencyRequest>(
                "/singleplayer/pitfireteam/teammate/profile/proficiency",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.SetProficiency(url, info, sessionId))
            ),
            new RouteAction<FriendlyTeammateTacticRequest>(
                "/singleplayer/pitfireteam/teammate/profile/tactic",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.SetTactic(url, info, sessionId))
            ),
            new RouteAction<FriendlyTeammateAutoJoinRequest>(
                "/singleplayer/pitfireteam/teammate/autojoin",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.SetAutoJoin(url, info, sessionId))
            ),
            new RouteAction<FriendlyTeammateDeleteRequest>(
                "/singleplayer/pitfireteam/teammate/delete",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => callbacks.Delete(url, info, sessionId))
            ),
            // STM-only routes (own /stm/ prefix; no native EFT route is intercepted).
            new RouteAction<EmptyRequestData>(
                "/stm/members/list",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => stmMembers.List(url, info, sessionId))
            ),
            new RouteAction<EmptyRequestData>(
                "/stm/members/roster",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => stmMembers.Roster(url, info, sessionId))
            ),
            new RouteAction<StmProfileMemberRequest>(
                "/stm/members/add",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => stmMembers.Add(url, info, sessionId))
            ),
            new RouteAction<StmProfileMemberRequest>(
                "/stm/members/remove",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => stmMembers.Remove(url, info, sessionId))
            ),
            new RouteAction<StmCloseTransferRequest>(
                "/stm/members/close-transfer",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => stmMembers.CloseTransfer(url, info, sessionId))
            ),
            new RouteAction<StmGearEditRequest>(
                "/stm/members/edit-gear",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => stmMembers.EditGear(url, info, sessionId))
            ),
            new RouteAction<StmGearStashRequest>(
                "/stm/members/gear-stash",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => stmMembers.GearStash(url, info, sessionId))
            ),
            new RouteAction<StmGearBuildsRequest>(
                "/stm/members/builds",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => stmMembers.GearBuilds(url, info, sessionId))
            ),
            new RouteAction<StmGearKitQuoteRequest>(
                "/stm/members/kit-quote",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => stmMembers.KitQuote(url, info, sessionId))
            ),
            new RouteAction<StmDiscardJournalRequest>(
                "/stm/members/discard-journal",
                async (url, info, sessionId, output, cancellationToken) => await FriendlyModeRequestGate.Run(() => stmMembers.DiscardUnreadableJournal(url, info, sessionId))
            ),
        ]
    )
{ }
