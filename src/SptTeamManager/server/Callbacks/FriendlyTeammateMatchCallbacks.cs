using pitTeam.Server.Models;
using pitTeam.Server.Services;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Services.Commerce;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

using CommonCustomization = SPTarkov.Server.Core.Models.Eft.Common.Tables.Customization;

namespace pitTeam.Server.Callbacks;

[Injectable]
public class FriendlyTeammateMatchCallbacks(
    FriendlyTeammateService teammateService,
    FriendlyPostRaidService postRaidService,
    FollowerInsuranceRaidDiagnostics insuranceDiagnostics,
    HttpResponseUtil httpResponseUtil,
    NotificationSendHelper notificationSendHelper,
    MailSendService mailSendService,
    StmProfileMemberResolver profileMemberResolver,
    ICloner cloner,
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

            // STM: PIT's lookup found nothing. Ask whether the id is a launcher-profile teammate; if it is
            // not (no reason either), fall through to the return below exactly as before STM.
            if (teammate == null)
            {
                if (profileMemberResolver.TryResolve(sessionId, request.To, out var profileMember, out var unavailableReason))
                {
                    // Identity only, like PIT's invite (ToGroupCharacter): no spawn profile is built here, so
                    // StmSpawnProfileFactory.LastGearKind is not written for an invite that is not a spawn.
                    // Read from a clone: profileMember.Pmc is the live object SaveServer holds.
                    FriendlyGroupMatchInviteAccept accept;
                    try
                    {
                        accept = BuildProfileMemberAccept(profileMember!);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"[STM] Group invite: could not build the invite for profile teammate '{request.To}' (session '{sessionId}'); no invite sent. {ex}");
                        return new ValueTask<string>(previousOutput ?? httpResponseUtil.NullResponse());
                    }

                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1000);
                        await FriendlyModeRequestGate.RunIfCurrent(modeGeneration, async () =>
                        {
                            await notificationSendHelper.SendMessageAsync(sessionId, accept);
                        });
                    });

                    return new ValueTask<string>(previousOutput ?? httpResponseUtil.GetBody("pitfireteam-teammate-invite"));
                }

                if (!string.IsNullOrWhiteSpace(unavailableReason))
                {
                    mailSendService.SendSystemMessageToPlayer(
                        sessionId,
                        $"Cannot add profile teammate {request.To} to the raid group: {unavailableReason}.",
                        null);
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

    // Same fields PIT's ToGroupCharacter fills for the invite-accept notification. Built from a clone
    // because member.Pmc is the live profile object: nothing here may alias or write into it.
    private FriendlyGroupMatchInviteAccept BuildProfileMemberAccept(StmResolvedMember member)
    {
        var pmc = cloner.Clone(member.Pmc) ?? throw new InvalidOperationException("Profile clone returned null");
        var info = pmc.Info ?? throw new InvalidOperationException("Profile teammate is missing Info");
        var customization = pmc.Customization ?? new CommonCustomization();
        var inventory = pmc.Inventory ?? new BotBaseInventory { Items = [] };

        return new FriendlyGroupMatchInviteAccept
        {
            EventType = NotificationEventType.groupMatchInviteAccept,
            EventIdentifier = new MongoId(),
            Id = pmc.Id?.ToString(),
            Aid = pmc.Aid?.ToString(),
            Info = new CharacterInfo
            {
                Nickname = info.Nickname,
                SavageNickname = info.Nickname,
                Side = info.Side,
                Level = info.Level,
                MemberCategory = MemberCategory.Unheard,
                GameVersion = info.GameVersion,
                HasCoopExtension = info.HasCoopExtension,
            },
            VisualRepresentation = new PlayerVisualRepresentation
            {
                Info = new VisualInfo
                {
                    Nickname = info.Nickname,
                    Side = info.Side,
                    Level = info.Level,
                    MemberCategory = MemberCategory.Unheard,
                    GameVersion = info.GameVersion,
                },
                Customization = new SPTarkov.Server.Core.Models.Eft.Match.Customization
                {
                    Head = customization.Head?.ToString(),
                    Body = customization.Body?.ToString(),
                    Feet = customization.Feet?.ToString(),
                    Hands = customization.Hands?.ToString(),
                },
                Equipment = new SPTarkov.Server.Core.Models.Eft.Match.Equipment
                {
                    Id = inventory.Equipment?.ToString(),
                    Items = inventory.Items ?? [],
                },
            },
            IsReady = true,
        };
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
