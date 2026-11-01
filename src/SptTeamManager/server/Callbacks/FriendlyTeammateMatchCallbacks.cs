using pitTeam.Server.Models;
using pitTeam.Server.Services;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Services.Commerce;
using SPTarkov.Server.Core.Utils;

namespace pitTeam.Server.Callbacks;

[Injectable]
public class FriendlyTeammateMatchCallbacks(
    FriendlyTeammateService teammateService,
    FriendlyPostRaidService postRaidService,
    FollowerInsuranceRaidDiagnostics insuranceDiagnostics,
    HttpResponseUtil httpResponseUtil,
    NotificationSendHelper notificationSendHelper,
    MailSendService mailSendService,
    StmProfileMemberStore profileMemberStore,
    ISptLogger<FriendlyTeammateMatchCallbacks> logger
)
{
    public ValueTask<string> SendGroupInvite(
        string url,
        MatchGroupInviteSendRequest request,
        MongoId sessionId,
        string? previousOutput)
    {
        long modeGeneration = FriendlyModeRequestGate.Generation;
        if (!teammateService.TryGetRaidGroupCharacter(sessionId, request.To, out var teammate, out var rejectionReason))
        {
            if (teammate != null && !string.IsNullOrWhiteSpace(rejectionReason))
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    await FriendlyModeRequestGate.RunIfCurrent(modeGeneration, async () =>
                    {
                        await notificationSendHelper.SendMessageAsync(
                            sessionId,
                            new FriendlyGroupMatchInviteDecline
                            {
                                EventType = NotificationEventType.groupMatchInviteDecline,
                                EventIdentifier = new MongoId(),
                                Aid = teammate.Aid?.ToString(),
                                Nickname = teammate.Info?.Nickname ?? teammate.Aid?.ToString(),
                            }
                        );

                        mailSendService.SendSystemMessageToPlayer(sessionId, rejectionReason, null);
                    });
                });

                return new ValueTask<string>(previousOutput ?? httpResponseUtil.GetBody("pitfireteam-teammate-invite"));
            }

            // NOTE: the block below CANNOT currently fire. request.To arrives as an Aid string, but the
            // registration document holds only the profile id (a MongoId), so the comparison is never true.
            // Resolving this is a pending decision for the repository owner (see doc/WORKLOG.md,
            // "열린 결정 — 프로필 팀원 초대 응답"). Do not rely on it, and do not delete it before that decision.
            // STM: PIT's lookup found nothing. If the id is a registered launcher-profile member, say so
            // instead of staying silent (profile members cannot be taken into a raid yet). Contained: any
            // failure here is logged and falls through to exactly the return below, as before STM.
            if (teammate == null)
            {
                try
                {
                    string target = request.To?.Trim() ?? string.Empty;
                    if (target.Length > 0 && profileMemberStore.List(sessionId).Any(m => string.Equals(m.ProfileId, target, StringComparison.Ordinal)))
                    {
                        mailSendService.SendSystemMessageToPlayer(
                            sessionId,
                            "Profile teammates cannot join a raid yet. This is planned.",
                            null);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"[STM] Group invite: could not check whether '{request.To}' is a profile member (session '{sessionId}'); no message sent. {ex}");
                }
            }

            return new ValueTask<string>(previousOutput ?? httpResponseUtil.NullResponse());
        }

        _ = Task.Run(async () =>
        {
            await Task.Delay(1000);
            await FriendlyModeRequestGate.RunIfCurrent(modeGeneration, async () =>
            {
                var acceptedTeammate = teammate!;

                await notificationSendHelper.SendMessageAsync(
                    sessionId,
                    new FriendlyGroupMatchInviteAccept
                    {
                        EventType = NotificationEventType.groupMatchInviteAccept,
                        EventIdentifier = new MongoId(),
                        Id = acceptedTeammate.Id,
                        Aid = acceptedTeammate.Aid?.ToString(),
                        Info = acceptedTeammate.Info,
                        VisualRepresentation = acceptedTeammate.VisualRepresentation,
                        IsReady = true,
                    }
                );
            });
        });

        return new ValueTask<string>(previousOutput ?? httpResponseUtil.GetBody("pitfireteam-teammate-invite"));
    }

    public ValueTask<string> GenerateFollowerProfile(
        string url,
        FriendlyTeammateFollowerGenerateRequest request,
        MongoId sessionId)
    {
        if (!teammateService.TryGetSpawnProfile(sessionId, request.MemberId, request.Custom?.Health, out var teammate))
        {
            return new ValueTask<string>(httpResponseUtil.GetBody(Array.Empty<object>()));
        }

        string profileId = teammate?.Id.ToString() ?? request.MemberId ?? "unknown";
        insuranceDiagnostics.CaptureGeneratedFollower(sessionId, teammate);
        HashSet<string> protectedSpawnIds = teammateService.GetProtectedSpawnItemIdsForExtraction(teammate);
        postRaidService.RegisterProtectedRaidItemIds(
            sessionId,
            protectedSpawnIds,
            $"server-generated teammate spawn equipment {profileId}");

        return new ValueTask<string>(httpResponseUtil.GetBody(new[] { teammate }));
    }

    public ValueTask<string> GetFollowerDetails(string url, EmptyRequestData request, MongoId sessionId)
    {
        var details = teammateService.ListFollowerDetails(sessionId);
        return new ValueTask<string>(httpResponseUtil.GetBody(details));
    }

    public ValueTask<string> PersistFollowerProgress(
        string url,
        FriendlyTeammateFollowerProgressBatchRequest request,
        MongoId sessionId)
    {
        teammateService.PersistFollowerProgress(sessionId, request.Entries);
        return new ValueTask<string>(httpResponseUtil.EmptyResponse());
    }
}
