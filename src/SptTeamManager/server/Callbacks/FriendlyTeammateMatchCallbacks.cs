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

        // STM: the resolver is asked BEFORE PIT (plan Review Focus). A registered profile teammate is
        // answered here; "not a profile teammate" (false with a null reason) falls through to PIT's own
        // lookup below, which then runs exactly as it did before STM.
        // Why this order: the friend list the player clicks shows the PROFILE for an Aid a mercenary and
        // a registered profile happen to share, so resolving to the mercenary would invite and spawn a
        // teammate nobody can see. PIT-first made that a real hole; resolver-first closes it, and the
        // outcome and progress paths already decide this way.
        if (TryHandleProfileMemberInvite(sessionId, request.To, modeGeneration))
        {
            return new ValueTask<string>(previousOutput ?? httpResponseUtil.GetBody("pitfireteam-teammate-invite"));
        }

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

    // Answers the invite for a launcher-profile teammate: true means STM handled it (an accept or a
    // decline notification was queued) and PIT's own lookup must NOT run. False means this id is not a
    // registered profile teammate, so PIT's path stands untouched.
    //
    // Never throws, on its own: the resolver call is inside the try too, so the guarantee does not rest
    // on another class keeping its catch-all (same shape as TryBuildProfileMemberSpawn below). A throw
    // is reported as "not a profile teammate" so PIT still gets its turn - nothing here writes to a
    // profile, so handing the id on costs nothing a failure has not already cost.
    private bool TryHandleProfileMemberInvite(MongoId sessionId, string? accountId, long modeGeneration)
    {
        try
        {
            var resolved = profileMemberResolver.TryResolve(sessionId, accountId, out var profileMember, out var unavailableReason);
            if (!resolved && string.IsNullOrWhiteSpace(unavailableReason))
            {
                // Not a profile teammate at all. PIT's path, exactly as before STM.
                return false;
            }

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
                    logger.Error($"[STM] Group invite: could not build the invite for profile teammate '{accountId}' (session '{sessionId}'); declining. {ex}");
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

                return true;
            }

            // A registered teammate that must not be taken. The raw enum name stays in the log; the
            // player gets a sentence.
            logger.Warning($"[STM] Group invite: profile teammate '{accountId}' (session '{sessionId}') unavailable: {unavailableReason}");
            var reasonText = unavailableReason switch
            {
                "CurrentSession" => "that profile is the one you are currently playing",
                "Empty" => "that profile has no character yet",
                "Unreadable" => "that profile could not be read",
                _ => "that profile is not available right now",
            };

            // Same shape as PIT's own rejection: decline notification then system message, after the 1s
            // delay and inside the mode gate. Aid/Nickname come from the request because no member was
            // resolved.
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
                            Aid = accountId,
                            Nickname = accountId,
                        }
                    );

                    mailSendService.SendSystemMessageToPlayer(
                        sessionId,
                        $"Cannot add profile teammate {accountId} to the raid group: {reasonText}.",
                        null);
                });
            });

            return true;
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] Group invite: could not tell whether '{accountId}' (session '{sessionId}') is a profile teammate; leaving the invite to PIT's own path. {ex}");
            return false;
        }
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
        // STM: the resolver is asked BEFORE PIT (plan Review Focus), for the same reason as the invite
        // path above - the Aid the client spawns is the one the friend list offered, so a registered
        // profile must win over a mercenary that happens to carry the same Aid. The helper is the ONLY
        // call site of StmSpawnProfileFactory.Build, and the one post-spawn call made for a profile
        // teammate is the protected-id registration inside it - CaptureGeneratedFollower below is
        // insurance settlement, not diagnostics, and must not be restored for symmetry
        // (task-5-report.md, 판단 B).
        var generated = TryBuildProfileMemberSpawn(sessionId, request.MemberId, request.Custom?.Health, out var notAProfileTeammate);
        if (generated != null)
        {
            return new ValueTask<string>(httpResponseUtil.GetBody(new[] { generated }));
        }

        if (!notAProfileTeammate)
        {
            // A registered profile teammate that could not be taken. PIT is not asked: its lookup knows
            // nothing about profile teammates, and letting it answer would spawn whatever else carries
            // this Aid. Nothing is spawned, which is what the invite path already told the player.
            return new ValueTask<string>(httpResponseUtil.GetBody(Array.Empty<object>()));
        }

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

    // Builds the raid spawn for a launcher-profile teammate, or null when there is none to build.
    // notAProfileTeammate separates the two reasons for a null: true = this id is not a registered
    // profile teammate (false with a null reason), so PIT's own path must stand; false = it IS one and
    // must not be spawned, so PIT is not asked either.
    // It starts false, which is the closed side: an id we could not classify is not handed to PIT.
    // Never throws, on its own: the resolver call is inside the try too, so the guarantee does not rest
    // on another class keeping its catch-all.
    private BotBase? TryBuildProfileMemberSpawn(MongoId sessionId, string? memberId, double? healthMultiplier, out bool notAProfileTeammate)
    {
        notAProfileTeammate = false;
        try
        {
            if (!profileMemberResolver.TryResolve(sessionId, memberId, out var member, out var unavailableReason))
            {
                notAProfileTeammate = string.IsNullOrWhiteSpace(unavailableReason);
                if (!string.IsNullOrWhiteSpace(unavailableReason))
                {
                    // A registered teammate that must not be taken. Nothing is spawned for it; the invite
                    // path already told the player why, so this is a log line only.
                    logger.Warning($"[STM] Follower generate: profile teammate '{memberId}' (session '{sessionId}') unavailable: {unavailableReason}");
                }

                return null;
            }

            // healthMultiplier is the client's own setting off this request (Custom.Health), the same
            // value PIT hands its own TryGetSpawnProfile in the caller.
            var spawn = spawnProfileFactory.Build(sessionId, member!, out var gearKind, healthMultiplier);

            // Nothing in a profile teammate's spawn inventory may ever reach the player's raid-end
            // delivery - Real gear because it still exists in that launcher profile (a copy, not a
            // move), Temporary gear because it was generated out of nothing. Hence the WHOLE inventory,
            // in every loadout mode, rather than PIT's equipment-tree collector, whose ignore-slot
            // policy and stash-free scope were written for mercenaries (task-5-report.md, 판단 A /
            // 수정 3차). ProtectedRaidItemIds is consumed by
            // StripProtectedTeammateItemsFromReturnItems (FriendlyPostRaidService.cs:125-154).
            HashSet<string> protectedSpawnIds = new(
                spawn.Inventory?.Items?.Where(item => item?.Id != null).Select(item => item.Id.ToString()) ?? [],
                StringComparer.OrdinalIgnoreCase);
            postRaidService.RegisterProtectedRaidItemIds(
                sessionId,
                protectedSpawnIds,
                $"profile teammate spawn inventory {member!.ProfileId} ({gearKind} gear)");

            if (protectedSpawnIds.Count == 0)
            {
                // Nothing to leak, so this is the safe direction - but an empty spawn inventory is
                // itself odd enough to say out loud rather than hide inside the Info line below.
                logger.Warning($"[STM] Follower generate: profile teammate '{member.ProfileId}' spawned with no inventory item ids to protect.");
            }

            logger.Info($"[STM] Follower generate: spawning profile teammate '{member.Nickname}' (aid {member.Aid}, profile '{member.ProfileId}') with {gearKind} gear.");
            return spawn;
        }
        catch (Exception ex)
        {
            // Covers the protected-id registration too, deliberately: a spawn whose items were not
            // registered would hand them to the player at raid end, so no spawn is the safe outcome.
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

        // PIT's defaults for a teammate nobody configured, through PIT's own normalizers so a change
        // there cannot silently diverge from these rows. Same for every row, so built once.
        var defaults = FriendlyTeammateService.CreateDefaultTeammateSettings();
        var defaultAggression = FriendlyTeammateService.NormalizeAggression(defaults.Aggression);
        var defaultSelfDetermination = pitTeam.Shared.SelfDeterminationPolicy.Normalize(defaults.SelfDetermination);
        var defaultProficiency = FriendlyTeammateService.NormalizeProficiency(defaults.Proficiency);

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

            rows.Add(new FriendlyTeammateFollowerDetailsResponse
            {
                Aid = member.Aid.ToString(CultureInfo.InvariantCulture),
                // NormalizeCombatTactic maps a blank value to PIT's own default, so the registration
                // document's tactic needs no separate fallback here.
                Tactic = FriendlyTeammateService.NormalizeCombatTactic(tactic),
                Aggression = defaultAggression,
                SelfDetermination = defaultSelfDetermination,
                // `with { }` so no two rows share one mutable settings object.
                Proficiency = defaultProficiency with { },
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
