using System.Globalization;
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
    StmSpawnProfileFactory spawnProfileFactory,
    StmProfileMemberStore profileMemberStore,
    StmProfileCatalog profileCatalog,
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
                var resolved = profileMemberResolver.TryResolve(sessionId, request.To, out var profileMember, out var unavailableReason);
                FriendlyGroupMatchInviteAccept? accept = null;
                if (resolved)
                {
                    // Identity only, like PIT's invite (ToGroupCharacter): no spawn profile is built here, so
                    // StmSpawnProfileFactory.LastGearKind is not written for an invite that is not a spawn.
                    // Read from a clone: profileMember.Pmc is the live object SaveServer holds.
                    try
                    {
                        accept = BuildProfileMemberAccept(profileMember!);
                    }
                    catch (Exception ex)
                    {
                        // The resolver already identified a registered teammate, so a failure now is
                        // "a teammate we could not read", never "not a teammate": decline below.
                        unavailableReason = "Unreadable";
                        logger.Error($"[STM] Group invite: could not build the invite for profile teammate '{request.To}' (session '{sessionId}'); declining. {ex}");
                    }
                }

                if (accept != null)
                {
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
                    // The raw enum name stays in the log; the player gets a sentence.
                    logger.Warning($"[STM] Group invite: profile teammate '{request.To}' (session '{sessionId}') unavailable: {unavailableReason}");
                    var reasonText = unavailableReason switch
                    {
                        "CurrentSession" => "that profile is the one you are currently playing",
                        "Empty" => "that profile has no character yet",
                        "Unreadable" => "that profile could not be read",
                        _ => "that profile is not available right now",
                    };

                    // Same shape as PIT's rejection above: decline notification then system message,
                    // after the 1s delay and inside the mode gate. Aid/Nickname come from the request
                    // because no member was resolved.
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
                                    Aid = request.To,
                                    Nickname = request.To,
                                }
                            );

                            mailSendService.SendSystemMessageToPlayer(
                                sessionId,
                                $"Cannot add profile teammate {request.To} to the raid group: {reasonText}.",
                                null);
                        });
                    });

                    return new ValueTask<string>(previousOutput ?? httpResponseUtil.GetBody("pitfireteam-teammate-invite"));
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

    // Same fields PIT's ToGroupCharacter (FriendlyTeammateService) fills for the invite-accept notification. Built from a clone
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
            // Must equal StmSpawnProfileFactory.Build's spawn.Id: the client is told this id here and handed a bot with it at raid entry. member.ProfileId is the source of truth (the key the profile was read by).
            Id = new MongoId(member.ProfileId).ToString(),
            Aid = member.Aid.ToString(),
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
            // STM: PIT has no mercenary with this MemberId. Ask whether it is a launcher-profile
            // teammate; if it is not (false with no reason), fall through to PIT's empty array exactly
            // as before STM. This is the ONLY call site of StmSpawnProfileFactory.Build: Build records
            // the spawn's gear kind, and a second call would both overwrite that record and roll a
            // different temporary loadout.
            // Of PIT's two post-spawn calls below, only RegisterProtectedRaidItemIds is made for a
            // profile teammate (inside the helper). insuranceDiagnostics.CaptureGeneratedFollower is
            // NOT, and must not be restored for symmetry with PIT: it is not diagnostics. It reads the
            // mercenary's {aid}-settings.json policies and writes the raid diagnostic document, then
            // runs gift cancellation, policy transfer, courier authorisation and settlement
            // (FollowerInsuranceRaidDiagnostics.cs:132-153 via Update/SaveAndReport :214-241). A
            // launcher profile has no policy file, so that pass is a no-op at best and a way into the
            // player's own insurance at worst. Teammate insurance for profile members is out of scope.
            var generated = TryBuildProfileMemberSpawn(sessionId, request.MemberId, request.Custom?.Health);
            if (generated != null)
            {
                return new ValueTask<string>(httpResponseUtil.GetBody(new[] { generated }));
            }

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

    // Builds the raid spawn for a launcher-profile teammate, or null when this id is not one (then
    // PIT's own behaviour must stand). Never throws: a failure here must not break raid entry.
    private BotBase? TryBuildProfileMemberSpawn(MongoId sessionId, string? memberId, double? healthMultiplier)
    {
        if (!profileMemberResolver.TryResolve(sessionId, memberId, out var member, out var unavailableReason))
        {
            if (!string.IsNullOrWhiteSpace(unavailableReason))
            {
                // A registered teammate that must not be taken. Nothing is spawned for it; the invite
                // path already told the player why, so this is a log line only.
                logger.Warning($"[STM] Follower generate: profile teammate '{memberId}' (session '{sessionId}') unavailable: {unavailableReason}");
            }

            return null;
        }

        try
        {
            // healthMultiplier is the client's own setting off this request (Custom.Health), the same
            // value PIT hands TryGetSpawnProfile above.
            var spawn = spawnProfileFactory.Build(sessionId, member!, out var gearKind, healthMultiplier);

            // The same registration PIT does after its own successful spawn (see the mercenary path
            // below), with its own context string. This is not bookkeeping: ProtectedRaidItemIds is
            // what StripProtectedTeammateItemsFromReturnItems (FriendlyPostRaidService.cs:125-154)
            // uses to keep the teammate's gear OUT of the follower-loot delivery the player gets at
            // raid end. Without it, Real gear would be copied into the player's stash while it still
            // exists in its own launcher profile (duplication), and Temporary gear - generated out of
            // nothing - would be free loot.
            // Deliberately the UNGATED collection, not PIT's GetProtectedSpawnItemIdsForExtraction: that
            // one protects nothing in Immersive/Extreme mode (the default), which is a loot choice for a
            // mercenary whose gear exists nowhere else. A profile teammate's gear is also sitting in a
            // real launcher profile, so the same delivery would COPY an item instead of moving it -
            // protected in every mode. Not delivering it destroys nothing: it stays in that profile.
            HashSet<string> protectedSpawnIds = teammateService.CollectSpawnEquipmentTreeIds(spawn);
            postRaidService.RegisterProtectedRaidItemIds(
                sessionId,
                protectedSpawnIds,
                $"profile teammate spawn equipment {member!.ProfileId} ({gearKind} gear)");

            logger.Info($"[STM] Follower generate: spawning profile teammate '{member.Nickname}' (aid {member.Aid}, profile '{member.ProfileId}') with {gearKind} gear.");
            return spawn;
        }
        catch (Exception ex)
        {
            // Covers the protected-id registration too, deliberately: a spawn whose gear was not
            // registered would hand that gear to the player at raid end, so no spawn is the safe
            // outcome. PIT's path is unaffected either way.
            logger.Error($"[STM] Follower generate: could not build the spawn for profile teammate '{memberId}' (session '{sessionId}'); nothing is spawned for it. {ex}");
            return null;
        }
    }

    public ValueTask<string> GetFollowerDetails(string url, EmptyRequestData request, MongoId sessionId)
    {
        var details = teammateService.ListFollowerDetails(sessionId);
        // STM: the client looks this list up per spawn id, and what it does with a spawn that is
        // missing from it is not established - so profile teammates get a row too. A failure here
        // leaves PIT's own rows untouched.
        try
        {
            details.AddRange(ListProfileMemberFollowerDetails(sessionId));
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] Follower details: profile teammate rows could not be built for session '{sessionId}'; only PIT's own rows are returned. {ex}");
        }

        return new ValueTask<string>(httpResponseUtil.GetBody(details));
    }

    // One row per registered launcher profile that resolves right now. Tactic is the registration
    // document's value; Voice/Head come from the profile's own Customization; everything else is the
    // same value PIT's ListFollowerDetails produces for a freshly created teammate
    // (FriendlyTeammateService.cs:816-834 over CreateDefaultTeammateSettings, FriendlyTeammateService.cs:4585-4594).
    private List<FriendlyTeammateFollowerDetailsResponse> ListProfileMemberFollowerDetails(MongoId sessionId)
    {
        var rows = new List<FriendlyTeammateFollowerDetailsResponse>();
        var registered = profileMemberStore.List(sessionId);
        if (registered.Count == 0)
        {
            return rows;
        }

        // The client keys these rows by Aid, and the registration document holds only ProfileIds, so
        // the Aid comes from a fresh catalog read and the row itself from the resolver (which is what
        // decides whether a registered profile may be used at all).
        var aids = profileCatalog.GetCandidates(sessionId.ToString())
            .Where(candidate => candidate != null && candidate.Aid > 0)
            .Select(candidate => candidate.Aid)
            .Distinct()
            .ToList();

        foreach (var aid in aids)
        {
            if (!profileMemberResolver.TryResolve(sessionId, aid.ToString(CultureInfo.InvariantCulture), out var member, out _) || member == null)
            {
                continue;
            }

            var tactic = registered
                .FirstOrDefault(entry => string.Equals(entry.ProfileId?.Trim(), member.ProfileId, StringComparison.Ordinal))
                ?.Tactic;
            // Read-only: member.Pmc is the live profile object, and ToString() hands out no reference into it.
            var customization = member.Pmc?.Customization;
            // PIT's own defaults for a teammate nobody configured, through PIT's own normalizers, so a
            // change there cannot silently diverge from this row. (FriendlyTeammateService.cs:4585,
            // :4724, :4734, :4763 - promoted from private to internal for this call.)
            var defaults = FriendlyTeammateService.CreateDefaultTeammateSettings();

            rows.Add(new FriendlyTeammateFollowerDetailsResponse
            {
                Aid = member.Aid.ToString(CultureInfo.InvariantCulture),
                // NormalizeCombatTactic maps a blank value to PIT's own default, so the registration
                // document's tactic needs no separate fallback here.
                Tactic = FriendlyTeammateService.NormalizeCombatTactic(tactic),
                Aggression = FriendlyTeammateService.NormalizeAggression(defaults.Aggression),
                SelfDetermination = pitTeam.Shared.SelfDeterminationPolicy.Normalize(defaults.SelfDetermination),
                Proficiency = FriendlyTeammateService.NormalizeProficiency(defaults.Proficiency),
                Equipment = FriendlyTeammateService.DefaultLoadoutName,
                Voice = customization?.Voice?.ToString() ?? string.Empty,
                Head = customization?.Head?.ToString() ?? string.Empty,
            });
        }

        return rows;
    }

    public ValueTask<string> PersistFollowerProgress(
        string url,
        FriendlyTeammateFollowerProgressBatchRequest request,
        MongoId sessionId)
    {
        // STM: PIT's ApplyFollowerProgress adds experience and recalculates the level
        // (FriendlyTeammateService.cs:3563-3564, 3778-3797). Applied to a real launcher profile that
        // silently corrupts a character the player plays, so profile teammates are dropped before
        // PIT sees the batch. Experience for them is plan 4's job.
        teammateService.PersistFollowerProgress(sessionId, WithoutProfileMemberProgress(sessionId, request.Entries));
        return new ValueTask<string>(httpResponseUtil.EmptyResponse());
    }

    // Fails closed: an entry is kept only when the resolver says "not a profile teammate at all"
    // (false with a null reason), which is PIT's own path unchanged. Resolved teammates and
    // registered-but-unreadable ones are dropped, and so is an entry whose check itself failed -
    // not writing costs an unrecorded raid, writing costs the player's profile.
    private List<FriendlyTeammateFollowerProgressRequest>? WithoutProfileMemberProgress(
        MongoId sessionId,
        List<FriendlyTeammateFollowerProgressRequest>? entries)
    {
        if (entries == null || entries.Count == 0)
        {
            return entries;
        }

        var kept = new List<FriendlyTeammateFollowerProgressRequest>(entries.Count);
        var skipped = 0;
        foreach (var entry in entries)
        {
            if (ShouldSkipFollowerProgress(sessionId, entry))
            {
                skipped++;
                continue;
            }

            kept.Add(entry);
        }

        // One line per progress batch, not per teammate: raid teardown posts this once.
        if (skipped > 0)
        {
            logger.Info($"[STM] Follower progress: skipped {skipped} profile teammate entry/entries of {entries.Count} (session '{sessionId}'); a launcher profile's level must not be recalculated here.");
        }

        return kept;
    }

    private bool ShouldSkipFollowerProgress(MongoId sessionId, FriendlyTeammateFollowerProgressRequest? entry)
    {
        if (entry == null)
        {
            return false;
        }

        try
        {
            // true = a profile teammate; false with a reason = a registered teammate we could not
            // read, still a registered teammate. Both are skipped.
            return profileMemberResolver.TryResolve(sessionId, entry.Aid, out _, out var unavailableReason)
                || !string.IsNullOrWhiteSpace(unavailableReason);
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] Follower progress: could not tell whether aid '{entry.Aid}' (session '{sessionId}') is a profile teammate; skipping the entry rather than risk writing to a launcher profile. {ex}");
            return true;
        }
    }
}
