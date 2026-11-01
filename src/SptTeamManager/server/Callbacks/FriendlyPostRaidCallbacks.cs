using pitTeam.Server.Models;
using pitTeam.Server.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Models.Logging;
using System.Text.Json;

namespace pitTeam.Server.Callbacks;

[Injectable]
public class FriendlyPostRaidCallbacks(
    HttpResponseUtil httpResponse,
    FriendlyPostRaidService postRaidService,
    FriendlyRecruitService recruitService,
    FriendlyEncounterPenaltyService encounterPenalties,
    FollowerInsuranceRaidDiagnostics insuranceDiagnostics,
    FollowerInsuranceCourierTransferService courierTransfers,
    FriendlyTeammateService teammateService,
    FriendlyScavCargoService scavCargo,
    StmRaidOutcomeService stmRaidOutcomes,
    ISptLogger<FriendlyPostRaidCallbacks> logger
)
{
    public ValueTask<string> GiveGearInsurance(GiveGearInsuranceRequest request, MongoId sessionId)
    {
        insuranceDiagnostics.CancelGiftInsurance(sessionId, request);
        return new ValueTask<string>(httpResponse.NullResponse());
    }

    public ValueTask<string> ScavCargo(FriendlyScavCargoRequest request, MongoId sessionId)
    {
        scavCargo.Queue(sessionId, request);
        return new ValueTask<string>(httpResponse.NullResponse());
    }
    public ValueTask<string> ReturnItems(string url, FriendlyPostRaidReturnItemsRequest request, MongoId sessionId)
    {
        postRaidService.HandleReturnItems(sessionId, request);
        return new ValueTask<string>(httpResponse.NullResponse());
    }

    public ValueTask<string> TeamEscaped(string url, FriendlyPostRaidTeamEscapedRequest request, MongoId sessionId)
    {
        postRaidService.HandleTeamEscaped(sessionId, request);
        return new ValueTask<string>(httpResponse.NullResponse());
    }

    public ValueTask<string> RecruitPickup(string url, FriendlyRecruitPickupRequest request, MongoId sessionId)
    {
        recruitService.QueueRecruitPickups(sessionId, request.Candidates);
        return new ValueTask<string>(httpResponse.NullResponse());
    }

    public ValueTask<string> RecordKillMessage(string url, FriendlyPostRaidKillMessageRequest request, MongoId sessionId)
    {
        postRaidService.RecordKillMessage(sessionId, request);
        if (request.MessageKind == "traitor" && request.KilledAtUnixMs > 0)
            return new ValueTask<string>(httpResponse.GetBody(encounterPenalties.Record(sessionId, request)));
        return new ValueTask<string>(httpResponse.NullResponse());
    }

    public ValueTask<string> RegisterProtectedItems(string url, FriendlyPostRaidProtectedItemsRequest request, MongoId sessionId)
    {
        postRaidService.RegisterProtectedRaidItems(sessionId, request);
        return new ValueTask<string>(httpResponse.NullResponse());
    }

    public ValueTask<string> StartLocalRaid(string url, StartLocalRaidRequestData request, MongoId sessionId, string? output)
    {
        insuranceDiagnostics.BeginRaid(sessionId, request, output);
        scavCargo.BeginRaid(sessionId, output, request);
        return new ValueTask<string>(output ?? httpResponse.NullResponse());
    }

    public ValueTask<string> EndLocalRaid(string url, EndLocalRaidRequestData request, MongoId sessionId, string? output)
    {
        postRaidService.RemoveProtectedTeammateItemsFromExtractedProfile(sessionId, request);
        postRaidService.HandleEndLocalRaidKillMessages(sessionId, request);
        scavCargo.FinishRaid(sessionId, request);
        insuranceDiagnostics.EndRaid(sessionId, request);
        return new ValueTask<string>(output ?? httpResponse.NullResponse());
    }

    public ValueTask<string> RaidOutcomes(string url, FriendlyTeammateDeathEscapeRequest request, MongoId sessionId)
    {
        request ??= new FriendlyTeammateDeathEscapeRequest();

        if (request.ResolveOnly)
        {
            return new ValueTask<string>(httpResponse.GetBody(teammateService.ResolveRaidOutcomes(request.Entries)));
        }

        List<FriendlyTeammateDeathEscapeEntry> entries = request.Entries ?? [];
        // STM: whether the escape rolls were settled in THIS request decides whether an entry's
        // Escaped flag may be trusted for a profile teammate (see ApplyProfileTeammateOutcomes).
        bool rollsResolvedHere = false;
        if (entries.Any(entry => entry?.RollEscape == true))
        {
            entries = teammateService.ResolveRaidOutcomes(entries).Entries;
            rollsResolvedHere = true;
        }

        // PIT's own persistence runs first and unchanged. It lists every entry's name in the summary's
        // escaped/lost lists - profile teammates included, which is factually right - and then skips the
        // profile teammates themselves because TryFindByAccountId finds no mercenary for their Aid
        // (FriendlyTeammateService.cs:1519-1522). STM applies their outcome below, which is what makes
        // the names it already printed true.
        FriendlyTeammateDeathEscapeSummary summary = teammateService.PersistDeathEscapeOutcomes(sessionId, entries);
        ApplyProfileTeammateOutcomes(sessionId, request.InsuranceServerId, entries, rollsResolvedHere);
        insuranceDiagnostics.ObserveOutcomes(sessionId, entries, request.InsuranceServerId, request.InsuranceReportId);
        if (request.Notify)
        {
            postRaidService.HandleDeathEscapeSummary(sessionId, summary);
        }

        return new ValueTask<string>(httpResponse.NullResponse());
    }

    public ValueTask<string> DeathEscape(string url, FriendlyTeammateDeathEscapeRequest request, MongoId sessionId)
    {
        return RaidOutcomes(url, request, sessionId);
    }

    // STM: hands each reported outcome to StmRaidOutcomeService, which ignores anything that is not a
    // registered launcher-profile teammate. Wrapped per entry AND as a whole: a failure here must not
    // stop PIT's own mercenary outcome handling, nor the remaining entries.
    //
    // 'escaped' is read from FriendlyTeammateDeathEscapeEntry.Escaped, which is bool? precisely because
    // not-escaped is what causes a DELETION: an entry whose escape state is not settled must never be
    // reported as not-escaped. Two unsettled states are refused here rather than guessed in either
    // direction - the report did not carry the field at all (null), and the entry still asks for a roll
    // (RollEscape) that was not resolved in this request, where Escaped=false is only a placeholder
    // (client/Modules/FollowerDeathEscapeResolver.cs:209).
    private void ApplyProfileTeammateOutcomes(
        MongoId sessionId,
        string? raidId,
        List<FriendlyTeammateDeathEscapeEntry> entries,
        bool rollsResolvedHere)
    {
        try
        {
            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.Aid))
                {
                    continue;
                }

                if (entry.Escaped is null)
                {
                    logger.Warning(
                        $"[STM] 레이드 결과: '{entry.Nickname}'(aid {entry.Aid}) 보고에 탈출 여부(Escaped)가 없다. 생존했는지 알 수 없으므로 아무것도 하지 않는다.");
                    continue;
                }

                if (entry.RollEscape && !rollsResolvedHere)
                {
                    logger.Warning(
                        $"[STM] 레이드 결과: '{entry.Nickname}'(aid {entry.Aid})의 탈출 여부가 아직 정해지지 않았다(RollEscape). 생존했는지 알 수 없으므로 아무것도 하지 않는다.");
                    continue;
                }

                try
                {
                    stmRaidOutcomes.Apply(sessionId, raidId, entry.Aid, entry.Escaped.Value);
                }
                catch (Exception ex)
                {
                    logger.Error($"[STM] 레이드 결과: aid '{entry.Aid}' 처리에 실패했다(나머지 항목은 계속 처리한다): {ex}");
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 레이드 결과: 프로필 팀원 결과 적용 경로가 실패했다(PIT 용병 처리는 이미 끝났다): {ex}");
        }
    }

    public ValueTask<string> InsuranceReportsComplete(string url, FollowerInsuranceRaidCompletionRequest request, MongoId sessionId)
    {
        insuranceDiagnostics.CompleteReports(sessionId, request);
        return new ValueTask<string>(httpResponse.NullResponse());
    }

    public ValueTask<string> InsuranceMailClaim(ItemEventRouterRequest request, MongoId sessionId, string? output)
    {
        if (request.Data?.Any(body => body.ValueKind == JsonValueKind.Object
            && body.TryGetProperty("fromOwner", out var owner)
            && owner.ValueKind == JsonValueKind.Object
            && owner.TryGetProperty("type", out var type)
            && string.Equals(type.GetString(), "Mail", StringComparison.OrdinalIgnoreCase)) == true)
        {
            try { courierTransfers.TryApplyClaimed(sessionId); }
            catch (Exception ex) { logger.Warning($"[FollowerInsurance:CourierTransfer] Claim observation failed: {ex}"); }
        }
        return new ValueTask<string>(output ?? httpResponse.NullResponse());
    }

    public ValueTask<string> InsuranceRaidDisplay(string url, FollowerInsuranceRaidDisplayRequest request, MongoId sessionId)
    {
        return new ValueTask<string>(httpResponse.GetBody(
            insuranceDiagnostics.GetDisplayPolicies(sessionId, request?.InsuranceServerId)));
    }

    public ValueTask<string> InsuranceMailClaimPolicies(string url, FollowerInsuranceMailClaimRequest request, MongoId sessionId)
    {
        return new ValueTask<string>(httpResponse.GetBody(courierTransfers.GetClaimedPolicies(sessionId, request?.ItemIds ?? [])));
    }
}
