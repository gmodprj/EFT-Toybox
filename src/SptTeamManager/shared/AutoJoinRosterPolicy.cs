using System;
using System.Collections.Generic;

namespace pitTeam.Shared
{
    // 출격 화면(MatchMakerAcceptScreen.Show)에서 분대에 자동으로 넣을 프로필 팀원의 계정 id(Aid) 를 가린다.
    // 순수하고, 예외를 밖으로 던지지 않는다.
    public static class AutoJoinRosterPolicy
    {
        // 허용 목록이다: 토글이 켜져 있고 카탈로그에서 Ready 인 것만 담는다. Ready 가 아닌 모든 값은
        // (모르는 enum 값 포함) 담지 않는다 - 못 쓰는 팀원을 분대에 넣으면 출격 때 클라이언트가 경고만 남기고
        // 조용히 떨군다. 등록은 됐는데 카탈로그에 없는 프로필도 마찬가지다.
        // 같은 Aid 는 한 번만 담는다: 분대에 같은 계정이 두 번 들어가면 안 된다. Aid 가 양수가 아니면
        // 해석기(StmProfileMemberResolver)가 어차피 거절하므로 여기서도 담지 않는다.
        public static List<string> SelectAccountIds(IEnumerable<StmProfileMemberFields>? members, IEnumerable<ProfileCandidate>? candidates)
        {
            var result = new List<string>();
            if (members == null || candidates == null) return result;

            var byProfileId = new Dictionary<string, ProfileCandidate>(StringComparer.Ordinal);
            foreach (var candidate in candidates)
            {
                if (candidate?.ProfileId == null) continue;
                if (!byProfileId.ContainsKey(candidate.ProfileId)) byProfileId[candidate.ProfileId] = candidate;
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var member in members)
            {
                if (member == null || !member.AutoJoin || member.ProfileId == null) continue;
                if (!byProfileId.TryGetValue(member.ProfileId, out var candidate)) continue;
                if (candidate.Availability != ProfileAvailability.Ready) continue;
                if (candidate.Aid <= 0) continue;
                var accountId = candidate.Aid.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (seen.Add(accountId)) result.Add(accountId);
            }

            return result;
        }
    }
}
