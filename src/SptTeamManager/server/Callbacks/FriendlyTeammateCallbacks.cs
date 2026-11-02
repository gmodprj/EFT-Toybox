using pitTeam.Server.Models;
using pitTeam.Server.Services;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Insurance;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils;

namespace pitTeam.Server.Callbacks;

[Injectable]
public class FriendlyTeammateCallbacks(
    HttpResponseUtil httpResponse,
    FriendlyTeammateService teammateService,
    FriendlyServerSettingsService settingsService,
    FriendlyTeammateStorage storage,
    FriendlyTeammateInsuranceService teammateInsuranceService,
    FollowerInsuranceCourierTransferService courierTransfers,
    JsonUtil jsonUtil,
    StmProfileViewFactory profileViewFactory,
    StmAutoJoinService autoJoinService,
    StmProfileOperationGuard operationGuard,
    ISptLogger<FriendlyTeammateCallbacks> logger
)
{
    // 프로필 팀원에게 뜻이 없는 조작을 PIT 에 닿기 전에 거부한다(STM 먼저, 불변식 3). 프로필 팀원이 아니면 false 이고 PIT 경로가
    // 그대로 흐른다. 사유는 플레이어가 읽는 말이다(ProfileMemberOperationPolicy).
    private bool RefuseProfileMember(MongoId sessionId, string? accountId, ProfileMemberOperation operation, out ValueTask<string> refusal)
    {
        if (operationGuard.TryRefuse(sessionId, accountId, operation, out var message))
        {
            refusal = new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: message));
            return true;
        }

        refusal = default;
        return false;
    }

    private static bool IsReadFailure(string reason) =>
        string.Equals(reason, ProfileViewPolicy.ReadFailedReason, StringComparison.Ordinal);

    public ValueTask<string> InsureEquipment(string url, FriendlyTeammateInsuranceRequest request, MongoId sessionId)
    {
        if (RefuseProfileMember(sessionId, request.Aid, ProfileMemberOperation.Insurance, out var profileMemberRefusal)) return profileMemberRefusal;

        try
        {
            return new ValueTask<string>(httpResponse.GetBody(teammateService.InsureTeammateEquipment(sessionId, request)));
        }
        catch (Exception ex)
        {
            logger.Warning($"[FollowerInsurance:Purchase] teammateAid='{request.Aid}' failed: {ex}");
            string key = ex is FriendlyTeammateException ? ex.Message : "FollowerInsurancePurchaseFailed";
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: key));
        }
    }

    public ValueTask<string> AugmentInsuranceCosts(
        string url,
        GetInsuranceCostRequestData request,
        MongoId sessionId,
        string? previousOutput)
    {
        if (string.IsNullOrWhiteSpace(previousOutput))
        {
            return new ValueTask<string>(httpResponse.NullResponse());
        }

        var body = jsonUtil.Deserialize<FriendlyTeammateBodyResponse<GetInsuranceCostResponseData>>(previousOutput);
        if (body?.Data == null || body.Err is not (null or 0))
        {
            return new ValueTask<string>(previousOutput);
        }

        teammateInsuranceService.AugmentInsuranceCosts(sessionId, request, body.Data);
        return new ValueTask<string>(httpResponse.GetBody(body.Data, body.Err ?? 0, body.ErrMsg));
    }

    public ValueTask<string> Create(string url, FriendlyTeammateCreateRequest request, MongoId sessionId)
    {
        try
        {
            return new ValueTask<string>(httpResponse.GetBody(teammateService.CreateTeammate(sessionId, request)));
        }
        catch (FriendlyTeammateException ex)
        {
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: ex.Message));
        }
        catch (Exception ex)
        {
            logger.Error($"Teammate purchase failed: {ex}");
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: "TeammateHirePurchaseFailed"));
        }
    }

    public ValueTask<string> PrepareCreation(string url, FriendlyTeammateCreateRequest request, MongoId sessionId)
    {
        try { return new(httpResponse.GetBody(teammateService.PrepareTeammateCreation(sessionId, request))); }
        catch (Exception ex)
        {
            logger.Warning($"Teammate quote failed: {ex}");
            return new(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError,
                errmsg: ex is FriendlyTeammateException ? ex.Message : "TeammateHirePriceFailed"));
        }
    }

    public ValueTask<string> CancelCreation(string url, FriendlyTeammateCreateRequest request, MongoId sessionId)
    {
        teammateService.CancelTeammateCreation(sessionId, request);
        return new(httpResponse.NullResponse());
    }

    public ValueTask<string> List(string url, EmptyRequestData _, MongoId sessionId)
    {
        teammateService.RecoverTeammateCreation(sessionId);
        return new ValueTask<string>(httpResponse.GetBody(teammateService.ListTeammates(sessionId)));
    }

    public ValueTask<string> ListAutoJoin(string url, EmptyRequestData _, MongoId sessionId)
    {
        // PIT 결과가 먼저 나오고 STM 은 뒤에 붙기만 한다. AppendAccountIds 는 던지지 않는다(실패하면 PIT 목록 그대로).
        return new ValueTask<string>(httpResponse.GetBody(
            autoJoinService.AppendAccountIds(sessionId, teammateService.GetAutoJoinTeammateAccountIds(sessionId))));
    }

    public ValueTask<string> GetGameplayMode(string url, EmptyRequestData request, MongoId sessionId)
        => new(httpResponse.GetBody(new { gameplayMode = settingsService.LoadSettings().GameplayMode }));

    public ValueTask<string> SetServerSettings(string url, FriendlyServerSettingsRequest request, MongoId sessionId)
    {
        try
        {
            if (request.GameplayMode is not ("GunsForHire" or "Allegiance"))
                throw new FriendlyTeammateException("GameplayModeSwitchFailed");
            var previous = settingsService.LoadSettings();
            bool changingMode = previous.GameplayMode != request.GameplayMode;
            if (changingMode && FriendlyModeRequestGate.HasActiveRaid)
                throw new FriendlyTeammateException("SettingsUnavailableDuringRaid");
            if (changingMode)
            {
                teammateService.RecoverTeammateCreation(sessionId);
                teammateService.RecoverTeammateDeletion(sessionId);
                storage.PrepareMode(sessionId, request.IsAllegiance);
            }
              settingsService.SaveAndApply(request);
              if (changingMode) FriendlyModeRequestGate.ModeChanged();
            // A gameplay switch selects a different roster, not a loadout conversion of either roster.
            if (!changingMode && previous.LoadoutManagementMode != request.LoadoutManagementMode)
            {
                if (previous.LoadoutManagementMode is "Immersive" or "Extreme"
                    && request.LoadoutManagementMode is not ("Immersive" or "Extreme"))
                    courierTransfers.CancelUnclaimed(sessionId);
                teammateService.LogLoadoutManagementModeChange(sessionId, previous.LoadoutManagementMode, request.LoadoutManagementMode);
                teammateService.ApplyLoadoutManagementModeChange(sessionId, previous.LoadoutManagementMode, request.LoadoutManagementMode);
            }
            return new(httpResponse.NullResponse());
        }
        catch (Exception ex)
        {
            logger.Error($"Gameplay/settings update failed: {ex}");
            return new(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError,
                errmsg: ex is FriendlyTeammateException ? ex.Message : "GameplayModeSwitchFailed"));
        }
    }
    public ValueTask<string> GetLostOnDeathSettings(string url, EmptyRequestData _, MongoId sessionId)
    {
        return new ValueTask<string>(httpResponse.GetBody(settingsService.GetLostOnDeathSettings()));
    }

    public ValueTask<string> GetStartupRecoveryNotice(string url, EmptyRequestData _, MongoId sessionId)
    {
        return new ValueTask<string>(httpResponse.GetBody(teammateService.GetStartupRecoveryNotice(sessionId)));
    }

    public ValueTask<string> AcknowledgeStartupRecoveryNotice(string url, EmptyRequestData _, MongoId sessionId)
    {
        teammateService.AcknowledgeStartupRecoveryNotice(sessionId);
        return new ValueTask<string>(httpResponse.NullResponse());
    }

    public ValueTask<string> GetProfile(string url, GetOtherProfileRequest request, MongoId sessionId)
    {
        // STM asks before PIT (invariant: the roster showed the player a PROFILE, so an Aid both own
        // must resolve to the profile). A registered teammate PIT has no mercenary for is answered
        // here, or reported - never handed on to throw a not-found.
        if (profileViewFactory.TryBuildView(sessionId, request.AccountId, out var profileMemberView, out var unavailableReason))
        {
            return new ValueTask<string>(httpResponse.GetBody(profileMemberView));
        }

        // 읽기 실패는 거절이 아니라 PIT 길이다: 이 라우트의 PIT 길에는 SPT 응답 꼬리가 없어(용병이 없으면 오류) 새는 것이 없고,
        // 거절하면 PIT 용병의 조회가 STM 의 읽기 실패로 깨진다.
        if (!string.IsNullOrWhiteSpace(unavailableReason) && !IsReadFailure(unavailableReason))
        {
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: unavailableReason));
        }

        try
        {
            return new ValueTask<string>(httpResponse.GetBody(teammateService.GetTeammateProfile(sessionId, request)));
        }
        catch (FriendlyTeammateException ex)
        {
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: ex.Message));
        }
    }

    public ValueTask<string> GetProfileOptions(string url, FriendlyTeammateProfileOptionsRequest request, MongoId sessionId)
    {
        // STM 먼저. 프로필 팀원이면 여기서 답하고 PIT 의 옵션 조회(용병 문서를 찾다 던진다)는 부르지 않는다.
        // 쓸 수 없는 등록 팀원은 사유(상태 이름)를 오류로 돌려 화면이 설명으로 바꾼다.
        if (profileViewFactory.TryBuildOptions(sessionId, request.Aid, out var profileMemberOptions, out var optionsUnavailableReason))
        {
            return new ValueTask<string>(httpResponse.GetBody(profileMemberOptions));
        }

        // 읽기 실패는 PIT 길로 보낸다(위 GetProfile 과 같은 이유): PIT 용병의 옵션 화면이 STM 의 읽기 실패로 깨지면 안 된다.
        if (!string.IsNullOrWhiteSpace(optionsUnavailableReason) && !IsReadFailure(optionsUnavailableReason))
        {
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: optionsUnavailableReason));
        }

        try
        {
            return new ValueTask<string>(httpResponse.GetBody(teammateService.GetProfileOptions(sessionId, request)));
        }
        catch (FriendlyTeammateException ex)
        {
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: ex.Message));
        }
    }

    public ValueTask<string> SetSuit(string url, FriendlyTeammateSuitRequest request, MongoId sessionId)
    {
        if (RefuseProfileMember(sessionId, request.Aid, ProfileMemberOperation.Suit, out var profileMemberRefusal)) return profileMemberRefusal;

        try
        {
            teammateService.SetTeammateSuit(sessionId, request);
            return new ValueTask<string>(httpResponse.NullResponse());
        }
        catch (FriendlyTeammateException ex)
        {
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: ex.Message));
        }
    }

    public ValueTask<string> Rename(string url, FriendlyTeammateRenameRequest request, MongoId sessionId)
    {
        if (RefuseProfileMember(sessionId, request.Aid, ProfileMemberOperation.Rename, out var profileMemberRefusal)) return profileMemberRefusal;

        try
        {
            teammateService.RenameTeammate(sessionId, request);
            return new ValueTask<string>(httpResponse.NullResponse());
        }
        catch (FriendlyTeammateException ex)
        {
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: ex.Message));
        }
    }

    public ValueTask<string> SetLoadout(string url, FriendlyTeammateLoadoutRequest request, MongoId sessionId)
    {
        if (RefuseProfileMember(sessionId, request.Aid, ProfileMemberOperation.Loadout, out var profileMemberRefusal)) return profileMemberRefusal;

        try
        {
            teammateService.SetTeammateLoadout(sessionId, request);
            return new ValueTask<string>(httpResponse.NullResponse());
        }
        catch (FriendlyTeammateException ex)
        {
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: ex.Message));
        }
    }

    public ValueTask<string> SaveDefaultEquipment(string url, FriendlyTeammateDefaultEquipmentRequest request, MongoId sessionId)
    {
        try
        {
            var response = teammateService.SaveTeammateDefaultEquipment(sessionId, request);
            return new ValueTask<string>(httpResponse.GetBody(response));
        }
        catch (FriendlyTeammateException ex)
        {
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: ex.Message));
        }
    }

    public ValueTask<string> BuyKit(string url, FriendlyTeammateBuyKitRequest request, MongoId sessionId)
    {
        try
        {
            var response = teammateService.BuyTeammateKit(sessionId, request);
            return new ValueTask<string>(httpResponse.GetBody(response));
        }
        catch (FriendlyTeammateException ex)
        {
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: ex.Message));
        }
    }

    public ValueTask<string> RepairDefaultEquipment(string url, FriendlyTeammateRepairEquipmentRequest request, MongoId sessionId)
    {
        try
        {
            var response = teammateService.RepairTeammateDefaultEquipment(sessionId, request);
            return new ValueTask<string>(httpResponse.GetBody(response));
        }
        catch (FriendlyTeammateException ex)
        {
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: ex.Message));
        }
    }

    public ValueTask<string> SetAggression(string url, FriendlyTeammateAggressionRequest request, MongoId sessionId)
    {
        if (RefuseProfileMember(sessionId, request.Aid, ProfileMemberOperation.Aggression, out var profileMemberRefusal)) return profileMemberRefusal;

        try
        {
            teammateService.SetTeammateAggression(sessionId, request);
            return new ValueTask<string>(httpResponse.NullResponse());
        }
        catch (FriendlyTeammateException ex)
        {
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: ex.Message));
        }
    }

    public ValueTask<string> SetProficiency(string url, FriendlyTeammateProficiencyRequest request, MongoId sessionId)
    {
        if (RefuseProfileMember(sessionId, request.Aid, ProfileMemberOperation.Proficiency, out var profileMemberRefusal)) return profileMemberRefusal;

        try
        {
            teammateService.SetTeammateProficiency(sessionId, request);
            return new ValueTask<string>(httpResponse.NullResponse());
        }
        catch (FriendlyTeammateException ex)
        {
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: ex.Message));
        }
    }

    public ValueTask<string> SetTactic(string url, FriendlyTeammateTacticRequest request, MongoId sessionId)
    {
        if (RefuseProfileMember(sessionId, request.Aid, ProfileMemberOperation.Tactic, out var profileMemberRefusal)) return profileMemberRefusal;

        try
        {
            teammateService.SetTeammateTactic(sessionId, request);
            return new ValueTask<string>(httpResponse.NullResponse());
        }
        catch (FriendlyTeammateException ex)
        {
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: ex.Message));
        }
    }

    public ValueTask<string> SetAutoJoin(string url, FriendlyTeammateAutoJoinRequest request, MongoId sessionId)
    {
        // STM 먼저(불변식 3). 처리했으면 PIT 를 부르지 않는다. 프로필 팀원이 아니면(false + 사유 없음) PIT 경로가 그대로다.
        if (autoJoinService.TrySetAutoJoin(sessionId, request.Aid, request.Enabled, out var unavailableReason))
        {
            return new ValueTask<string>(httpResponse.NullResponse());
        }

        if (!string.IsNullOrWhiteSpace(unavailableReason))
        {
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: unavailableReason));
        }

        try
        {
            teammateService.SetTeammateAutoJoin(sessionId, request);
            return new ValueTask<string>(httpResponse.NullResponse());
        }
        catch (FriendlyTeammateException ex)
        {
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: ex.Message));
        }
    }

    public ValueTask<string> Delete(string url, FriendlyTeammateDeleteRequest request, MongoId sessionId)
    {
        try
        {
            return new ValueTask<string>(
                httpResponse.GetBody(teammateService.DeleteTeammateWithPayment(sessionId, request))
            );
        }
        catch (Exception ex)
        {
            logger.Warning($"Teammate deletion failed: {ex}");
            return new ValueTask<string>(httpResponse.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError,
                errmsg: ex is FriendlyTeammateException ? ex.Message : "TeammateDeleteFailed"));
        }
    }
}
