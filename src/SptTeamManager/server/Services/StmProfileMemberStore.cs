using pitTeam.Server.Models;
using pitTeam.Shared;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Utils;

namespace pitTeam.Server.Services;

// Persists which launcher profiles the user registered as teammates, in PIT's encrypted
// per-session database. Only the registration fact is stored; profile data is read from the
// profile itself (via StmProfileCatalog) every time it is needed.
[Injectable(InjectionType.Singleton)]
public class StmProfileMemberStore(FriendlyTeammateStorage storage, StmProfileCatalog catalog)
{
    // Not a "{aid}.json" name, so TeammateDatabase.IsProfileDocument() never lists it as a teammate.
    private const string Document = "stm-profile-members.json";
    // Same defaults PIT gives a new teammate (FriendlyTeammateService.cs CombatTactic / AutoJoinEnabled).
    private const string DefaultTactic = "Rifleman";
    private const bool DefaultAutoJoin = false;

    private readonly object sync = new();
    // Set when the startup binding check fails. Every read then refuses instead of handing back records
    // that may be wrong - see VerifyOldDocumentBinding.
    private volatile bool bindingUnreliable;

    // 서버 시작 때 한 번 부른다. 옛 모양 문서({profileId, tactic, autoJoin} 만, handEdited 없음)를 저장 계층과
    // 같은 직렬화기로 읽어 HandEdited == false 이고 나머지 셋이 그대로 나오는지 본다. 테스트 프로젝트는 SPT
    // 어셈블리를 못 참조해서 이 검증을 할 수 없다.
    // 이 가정이 깨지면 사용자가 등록해 둔 프로필 팀원이 로스터에서 조용히 사라지거나 값이 어긋난다.
    // 그래서 닫힌 쪽으로 넘어뜨린다: 이후 모든 읽기·쓰기가 "읽을 수 없음" 으로 실패한다. 시작을 던져
    // 막지는 않는다(모드 전체를 못 올리는 비용이 더 크다). 반환값 false 면 호출자가 Error 로 남긴다.
    public bool VerifyOldDocumentBinding(JsonUtil jsonUtil, out string detail)
    {
        try
        {
            const string oldShape = "{\"members\":[{\"profileId\":\"canary-profile\",\"tactic\":\"canary-tactic\",\"autoJoin\":true}]}";
            var doc = jsonUtil.Deserialize<StmProfileMembersDocument>(oldShape);
            var m = doc?.Members is { Count: 1 } ? doc.Members[0] : null;
            if (m != null && m.ProfileId == "canary-profile" && m.Tactic == "canary-tactic" && m.AutoJoin && !m.HandEdited)
            {
                detail = string.Empty;
                return true;
            }
            detail = $"old-shape document bound to {m?.ToString() ?? "nothing"}";
        }
        catch (Exception ex)
        {
            detail = ex.Message;
        }
        bindingUnreliable = true;
        return false;
    }

    // 등록을 지금 믿고 읽을 수 있는지 던지지 않고 답한다. Load 의 던지는 동작은 그대로 두고(List/Add/Remove/Update 는
    // 닫힌 쪽으로 실패해야 한다), 매 호출 던졌다 잡는 경로를 피하려는 호출자(auto-join 목록)만 쓴다.
    public bool RegistrationsUsable => !bindingUnreliable;

    public IReadOnlyList<StmProfileMember> List(MongoId sessionId)
    {
        lock (sync) return Load(sessionId);
    }

    public void Add(MongoId sessionId, string profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
            throw new InvalidOperationException("StmProfileIdMissing");
        profileId = profileId.Trim();
        // Judged against a fresh read of the launcher profiles. Anything that is not Ready
        // (empty, unreadable, the logged-in profile, or simply unknown) is refused.
        var candidate = catalog.GetCandidates(sessionId.ToString())
            .FirstOrDefault(c => string.Equals(c.ProfileId, profileId, StringComparison.Ordinal));
        if (candidate == null || candidate.Availability != ProfileAvailability.Ready)
            throw new InvalidOperationException("StmProfileNotAvailable");

        lock (sync)
        {
            var members = Load(sessionId).ToList();
            if (members.Any(m => m.ProfileId == candidate.ProfileId)) return;
            members.Add(new StmProfileMember(candidate.ProfileId, DefaultTactic, DefaultAutoJoin, HandEdited: false));
            Save(sessionId, members);
        }
    }

    public void Remove(MongoId sessionId, string profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
            throw new InvalidOperationException("StmProfileIdMissing");
        profileId = profileId.Trim();
        lock (sync)
        {
            var members = Load(sessionId).ToList();
            // Removal needs no availability check: a profile that went bad must still be removable.
            if (members.RemoveAll(m => m.ProfileId == profileId) == 0) return;
            Save(sessionId, members);
        }
    }

    // Changes fields of an existing registration. Returns false - and writes nothing - when the
    // profile is not registered, so a stale click can never create or resurrect a registration.
    public bool Update(MongoId sessionId, string profileId, StmProfileMemberChange change)
    {
        if (string.IsNullOrWhiteSpace(profileId)) return false;
        profileId = profileId.Trim();
        lock (sync)
        {
            var members = Load(sessionId).ToList();
            var index = members.FindIndex(m => m.ProfileId == profileId);
            if (index < 0) return false;
            var current = members[index];
            var updated = ProfileMemberUpdatePolicy.Apply(
                new StmProfileMemberFields(current.ProfileId, current.Tactic, current.AutoJoin, current.HandEdited), change);
            // Fail closed: a policy that cannot judge changes nothing.
            if (updated == null) return false;
            members[index] = new StmProfileMember(updated.ProfileId, updated.Tactic, updated.AutoJoin, updated.HandEdited);
            Save(sessionId, members);
            return true;
        }
    }

    // Per logged-in profile (keyed by sessionId) and independent of the Guns for Hire / Allegiance mode: the
    // team does not change with the mode. (Upstream calls this "account-wide"; it is not shared across profiles.)
    private List<StmProfileMember> Load(MongoId sessionId) =>
        bindingUnreliable
            ? throw new InvalidOperationException("StmRegistrationsUnreadable")
            : storage.ReadSharedDocument<StmProfileMembersDocument>(sessionId, Document)?.Members ?? [];

    private void Save(MongoId sessionId, List<StmProfileMember> members) =>
        storage.WriteSharedDocument(sessionId, Document, new StmProfileMembersDocument { Members = members });
}
