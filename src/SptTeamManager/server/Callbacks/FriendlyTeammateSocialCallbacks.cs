using SPTarkov.Server.Core.Models.Enums;
using pitTeam.Server.Models;
using pitTeam.Server.Services;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Utils;
using System;

namespace pitTeam.Server.Callbacks;

[Injectable]
public class FriendlyTeammateSocialCallbacks(
    FriendlyTeammateService teammateService,
    FriendlyRecruitService recruitService,
    FriendlySquadOnboardingService onboarding,
    StmProfileMemberStore profileMemberStore,
    StmProfileCatalog profileCatalog,
    StmProfileViewFactory profileViewFactory,
    ISptLogger<FriendlyTeammateSocialCallbacks> logger,
    HttpResponseUtil httpResponseUtil,
    JsonUtil jsonUtil
)
{
    public ValueTask<string> MergeFriendList(string url, EmptyRequestData _, MongoId sessionId, string? previousOutput)
    {
        var body = DeserializeBody<GetFriendListDataResponse>(previousOutput);
        var response = body?.Data ?? new GetFriendListDataResponse
        {
            Friends = [],
            Ignore = [],
            InIgnoreList = [],
        };

        response.Friends ??= [];

        // Real response objects, kept by (kind, Id): FriendEntry only carries what the duplicate
        // decision needs, so the final list is assembled from these originals, never rebuilt.
        var originals = new Dictionary<string, Queue<UserDialogInfo>>(StringComparer.Ordinal);
        var existingEntries = new List<FriendEntry>();
        foreach (var friend in response.Friends)
        {
            var entry = new FriendEntry(friend.Id.ToString(), friend.Aid ?? 0, FriendKind.Native);
            existingEntries.Add(entry);
            Remember(originals, entry, friend);
        }

        var incomingEntries = new List<FriendEntry>();
        foreach (var teammate in teammateService.ListTeammateDialogs(sessionId))
        {
            var entry = new FriendEntry(teammate.Id.ToString(), teammate.Aid ?? 0, FriendKind.Mercenary);
            incomingEntries.Add(entry);
            Remember(originals, entry, teammate);
        }

        // Registered launcher profiles, joined against a fresh catalog read: only Ready candidates
        // become entries. The logged-in profile, empty/unreadable ones and ones deleted since
        // registration (absent from the catalog) never reach the list.
        // Contained so that a failure here costs only the profile members: the native and mercenary
        // lists above are already complete and are returned exactly as before this feature.
        IReadOnlyList<ProfileCandidate> readyMembers = Array.Empty<ProfileCandidate>();
        try
        {
            var registeredIds = profileMemberStore.List(sessionId).Select(m => m.ProfileId).ToList();
            if (registeredIds.Count > 0)
            {
                var candidates = profileCatalog.GetCandidates(sessionId.ToString());
                readyMembers = ProfileRosterPolicy.SelectRegisteredReady(candidates, registeredIds);
            }
        }
        catch (Exception ex)
        {
            LogFaultOnce($"read|{sessionId}|{ex.GetType().FullName}|{ex.Message}",
                $"[STM] Friend list: profile members could not be read for session '{sessionId}'; none are offered (logged once per run for this fault; it recurs on every poll until fixed). {ex}");
            readyMembers = Array.Empty<ProfileCandidate>();
        }

        foreach (var candidate in readyMembers)
        {
            // Per entry: one malformed ProfileId (e.g. not a valid MongoId) only drops that member.
            try
            {
                var entry = new FriendEntry(candidate.ProfileId, candidate.Aid, FriendKind.ProfileMember);
                var dialog = ToProfileMemberDialog(candidate);
                incomingEntries.Add(entry);
                Remember(originals, entry, dialog);
            }
            catch (Exception ex)
            {
                LogFaultOnce($"convert|{sessionId}|{candidate.ProfileId}|{ex.GetType().FullName}",
                    $"[STM] Friend list: profile member '{candidate.ProfileId}' could not be converted and is left out (session '{sessionId}'; logged once per run for this fault). {ex}");
            }
        }

        var outcome = FriendListMergePolicy.Plan(existingEntries, incomingEntries);
        foreach (var (dropped, reason) in outcome.Skipped)
        {
            // The game polls this route, so each distinct (kind, Id, reason) is warned once per server run.
            var key = $"{dropped.Kind}|{dropped.Id}|{reason}";
            bool first;
            lock (LoggedSkips) first = LoggedSkips.Add(key);
            if (first)
            {
                logger.Warning($"[STM] Friend list: {dropped.Kind} '{dropped.Id}' (Aid {dropped.Aid}) was left out - {reason}");
            }
        }

        var merged = new List<UserDialogInfo>(outcome.Final.Count);
        foreach (var entry in outcome.Final)
        {
            if (originals.TryGetValue(OriginalKey(entry), out var queue) && queue.Count > 0)
            {
                merged.Add(queue.Dequeue());
            }
            else
            {
                // Cannot happen: every entry came from an original. Fail closed rather than invent one.
                logger.Error($"[STM] Friend list: no original response object for {entry.Kind} '{entry.Id}'; entry dropped.");
            }
        }

        response.Friends = merged;
        return new ValueTask<string>(httpResponseUtil.GetBody(response, body?.Err ?? 0, body?.ErrMsg));
    }

    private static readonly HashSet<string> LoggedFaults = new(StringComparer.Ordinal);

    // The game polls /client/friend/list, and a corrupt registration document throws on every poll by
    // design. The full exception is logged once per distinct fault per server run, then not again.
    private void LogFaultOnce(string key, string message)
    {
        bool first;
        lock (LoggedFaults) first = LoggedFaults.Add(key);
        if (first)
        {
            logger.Error(message);
        }
    }

    private static readonly HashSet<string> LoggedSkips = new(StringComparer.Ordinal);

    private static string OriginalKey(FriendEntry entry) => $"{(int)entry.Kind}|{entry.Id}";

    private static void Remember(Dictionary<string, Queue<UserDialogInfo>> originals, FriendEntry entry, UserDialogInfo original)
    {
        var key = OriginalKey(entry);
        if (!originals.TryGetValue(key, out var queue))
        {
            originals[key] = queue = new Queue<UserDialogInfo>();
        }

        queue.Enqueue(original);
    }

    // Membership category: mercenaries are MemberCategory.Unheard (ToFriendSummary), so profile
    // members take Default to be told apart in the invite window.
    // CONFIRMED in game (2026-11-01, plan 1 gate item 5): the invite window draws profile members
    // with a person icon and PIT mercenaries with a lightning icon, so the two are told apart by
    // eye. That closed the item design section 8 left to the builder.
    // THE one place to change the profile members' icon/category if that ever stops looking
    // different from mercenaries (Unheard) in game. Both fields below read it.
    internal const MemberCategory ProfileMemberCategory = MemberCategory.Default;

    private static UserDialogInfo ToProfileMemberDialog(ProfileCandidate candidate) => new()
    {
        Id = new MongoId(candidate.ProfileId),
        Aid = candidate.Aid,
        Info = new UserDialogDetails
        {
            Nickname = candidate.Nickname,
            Side = candidate.Side,
            Level = candidate.Level,
            MemberCategory = ProfileMemberCategory,
            SelectedMemberCategory = ProfileMemberCategory,
        },
    };

    public ValueTask<string> MergeFriendRequestInbox(string url, EmptyRequestData _, MongoId sessionId, string? previousOutput)
    {
        // Recover a due welcome delivery after restart or a lost client response.
        onboarding.DeliverWelcomeInvitation(sessionId);
        var body = DeserializeBody<List<FriendlySocialFriendRequestEntry>>(previousOutput);
        var response = body?.Data ?? [];

        foreach (var request in recruitService.ListRecruitFriendRequests(sessionId))
        {
            if (response.Any(existing => string.Equals(existing.From, request.From, StringComparison.Ordinal)))
            {
                continue;
            }

            response.Add(request);
        }

        return new ValueTask<string>(httpResponseUtil.GetBody(response, body?.Err ?? 0, body?.ErrMsg));
    }

    public ValueTask<string> MergeProfileView(string url, GetOtherProfileRequest request, MongoId sessionId, string? previousOutput)
    {
        var body = DeserializeBody<GetOtherProfileResponse>(previousOutput);

        // STM 먼저(불변식 3). 이 라우트는 SPT 가 먼저 답한 뒤 불리는데, SPT 는 aid 로 "다른 런처 프로필" 을 직접 찾아 돌려준다
        // (ProfileHelper.GetFullProfileByAccountId -> SaveServer 의 프로필을 ProfileInfo.Aid 로 조회, 4.1.0·4.1.6 역컴파일로 확인).
        // 그 응답은 Equipment.Items 에 프로필의 인벤토리 전체(스태시 포함)를 싣고, 못 찾으면 로그인한 본인 프로필로 대체한다.
        // 그래서 프로필 팀원의 Aid 는 SPT 의 응답을 흘려보내지 않고 착용 장비로 줄인 STM 의 응답으로 바꾼다.
        // 등록됐지만 쓸 수 없는 팀원은 오류로 돌린다 - SPT 의 "본인 프로필로 대체" 가 팀원인 척 보이면 안 된다.
        // 등록을 믿을 수 없는 상태(시작 검사가 실패했다)에서는 이 Aid 가 등록된 프로필인지 알 수 없다. 해석기는 이때 던졌다 잡고 사유 없이
        // "팀원 아님" 을 돌려주는데, 이 라우트의 꼬리는 PIT 의 길이 아니라 SPT 의 응답(다른 프로필의 인벤토리 전체, 또는 본인
        // 프로필로의 대체)이라 거기로 흘리면 위 수정이 무너진다. 모르면 닫힌 쪽: 조회를 거절한다.
        // 단, PIT 자기 용병은 통과시킨다(ProfileViewPolicy): 용병 데이터는 PIT 의 것이라 다른 프로필의 스태시가 샐 수 없고, STM 의
        // 고장으로 PIT 의 조회까지 막으면 PIT 의 기능이 깨진다. 이 완화는 STM 이 아무것도 해석하지 못하는 상태에서만 돈다 -
        // 평소에는 Decide 가 PIT 를 묻지 않고 아래의 STM 먼저 순서가 그대로다.
        // PIT 자기 용병의 응답을 찾는다. 평소에는 아래 STM 먼저 순서가 끝난 뒤에야 쓰이고, 읽기가 실패한 경우에만 STM 보다 먼저 쓰인다.
        string? pitMercenaryView = null;
        bool PitKnowsAid()
        {
            if (teammateService.TryGetPendingTeammateProfile(sessionId, request.AccountId, out var pending))
            {
                pitMercenaryView = httpResponseUtil.GetBody(pending);
                return true;
            }

            if (teammateService.TryGetTeammateProfile(sessionId, request.AccountId, out var mercenary) && mercenary != null)
            {
                pitMercenaryView = httpResponseUtil.GetBody(mercenary);
                return true;
            }

            if (recruitService.TryGetRecruitProfile(sessionId, request.AccountId, out var recruit) && recruit != null)
            {
                pitMercenaryView = httpResponseUtil.GetBody(recruit);
                return true;
            }

            return false;
        }

        ValueTask<string> RefuseUnreadableView()
        {
            LogFaultOnce("view-registrations-unusable",
                "[STM] 프로필 조회: 등록된 프로필 팀원을 읽을 수 없는 상태라 이 Aid 가 프로필 팀원인지 알 수 없어, SPT 의 응답(다른 프로필의 인벤토리 전체일 수 있다)을 내보내지 않고 조회를 거절한다. PIT 용병의 조회는 계속 낸다. 원인은 시작 검사나 읽기 실패의 Error 로그를 보라(이 로그는 실행당 한 번).");
            return new ValueTask<string>(httpResponseUtil.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: "Unreadable"));
        }

        // 시작 때 등록을 믿을 수 없다고 표시된 경우: STM 은 아무것도 해석하지 못하므로 PIT 용병 먼저(ProfileViewPolicy), 아니면 거절.
        var viewRoute = ProfileViewPolicy.Decide(profileMemberStore.RegistrationsUsable, PitKnowsAid);
        if (viewRoute == ProfileViewRoute.ServePit && pitMercenaryView != null)
        {
            return new ValueTask<string>(pitMercenaryView);
        }

        if (viewRoute != ProfileViewRoute.StmFirst)
        {
            return RefuseUnreadableView();
        }

        if (profileViewFactory.TryBuildView(sessionId, request.AccountId, out var profileMemberView, out var profileMemberUnavailableReason))
        {
            return new ValueTask<string>(httpResponseUtil.GetBody(profileMemberView));
        }

        // 실행 중 읽기 실패(해석기가 한 번 읽고 한 번 분류한 결과): 시작 때 실패와 같은 길 - PIT 용병이면 낸다, 아니면 거절한다.
        // 확인된 비팀원(사유 없음)은 이 분기에 오지 않고 아래 PIT 길로 그대로 흐른다.
        if (string.Equals(profileMemberUnavailableReason, ProfileViewPolicy.ReadFailedReason, StringComparison.Ordinal))
        {
            var failedRoute = ProfileViewPolicy.RouteAfterLookup(ProfileLookupOutcome.ReadFailed, PitKnowsAid);
            if (failedRoute == ProfileViewRoute.ServePit && pitMercenaryView != null)
            {
                return new ValueTask<string>(pitMercenaryView);
            }

            return RefuseUnreadableView();
        }

        if (!string.IsNullOrWhiteSpace(profileMemberUnavailableReason))
        {
            return new ValueTask<string>(httpResponseUtil.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError, errmsg: profileMemberUnavailableReason));
        }

        if (teammateService.TryGetPendingTeammateProfile(sessionId, request.AccountId, out var pendingProfile))
            return new ValueTask<string>(httpResponseUtil.GetBody(pendingProfile));

        if (teammateService.TryGetTeammateProfile(sessionId, request.AccountId, out var teammateProfile)
            && teammateProfile != null)
        {
            return new ValueTask<string>(httpResponseUtil.GetBody(teammateProfile));
        }

        if (recruitService.TryGetRecruitProfile(sessionId, request.AccountId, out var recruitProfile)
            && recruitProfile != null)
        {
            return new ValueTask<string>(httpResponseUtil.GetBody(recruitProfile));
        }

        if (int.TryParse(request.AccountId, out var requestedAid) && body?.Data?.Aid == requestedAid)
        {
            return new ValueTask<string>(previousOutput ?? httpResponseUtil.NullResponse());
        }

        return new ValueTask<string>(previousOutput ?? httpResponseUtil.NullResponse());
    }

    public ValueTask<string> DeleteFriend(string url, DeleteFriendRequest request, MongoId sessionId, string? previousOutput)
    {
        try
        {
            teammateService.DeleteTeammateByProfileId(sessionId, request.FriendId);
            return new ValueTask<string>(previousOutput ?? httpResponseUtil.NullResponse());
        }
        catch (Exception ex)
        {
            return new(httpResponseUtil.GetBody<object?>(null, err: BackendErrorCodes.UnknownTradingError,
                errmsg: ex is FriendlyTeammateException ? ex.Message : "TeammateDeleteFailed"));
        }
    }

    public ValueTask<string> AcceptFriendRequest(string url, AcceptFriendRequestData request, MongoId sessionId, string? previousOutput)
    {
        if (recruitService.AcceptRecruitRequest(sessionId, request.ProfileId))
        {
            return new ValueTask<string>(httpResponseUtil.NullResponse());
        }

        return new ValueTask<string>(previousOutput ?? httpResponseUtil.NullResponse());
    }

    public ValueTask<string> AcceptAllFriendRequests(string url, EmptyRequestData request, MongoId sessionId, string? previousOutput)
    {
        recruitService.AcceptAllRecruitRequests(sessionId);
        return new ValueTask<string>(previousOutput ?? httpResponseUtil.NullResponse());
    }

    public ValueTask<string> DeclineFriendRequest(string url, DeclineFriendRequestData request, MongoId sessionId, string? previousOutput)
    {
        if (recruitService.DeclineRecruitRequest(sessionId, request.ProfileId))
        {
            return new ValueTask<string>(httpResponseUtil.NullResponse());
        }

        return new ValueTask<string>(previousOutput ?? httpResponseUtil.NullResponse());
    }

    private FriendlyTeammateBodyResponse<T>? DeserializeBody<T>(string? previousOutput)
    {
        if (string.IsNullOrWhiteSpace(previousOutput))
        {
            return null;
        }

        return jsonUtil.Deserialize<FriendlyTeammateBodyResponse<T>>(previousOutput);
    }
}
