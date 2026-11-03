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

        // 분대에 실제로 나갈 사람을 고른다(2026-11-03, 사용자가 정한 규칙).
        //
        // 상한이 **없을 때까지는** 켠 만큼 전부 나갔다 - 용병 넷에 프로필 다섯이면 아홉이 따라붙었다.
        // 규칙: **프로필 팀원 먼저**, 그 안에서 무작위. 자리가 남으면 용병에서, 역시 무작위.
        //   max <= 0 이면 무제한(지금까지의 동작). 같은 id 는 한 번만 - 프로필 쪽이 이긴다.
        //
        // rng 를 받는 이유는 **시험할 수 있게** 하기 위해서다. 같은 씨앗이면 같은 차례가 나오므로
        // "무작위" 가 실제로 섞는지, 상한이 정확히 몇에서 끊는지를 기계가 확인한다.
        public static List<string> Compose(
            IEnumerable<string>? profileAccountIds,
            IEnumerable<string>? mercenaryAccountIds,
            int max,
            Random? rng = null)
        {
            var random = rng ?? new Random();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var profiles = Collect(profileAccountIds, seen);
            var mercenaries = Collect(mercenaryAccountIds, seen);

            Shuffle(profiles, random);
            Shuffle(mercenaries, random);

            var result = new List<string>(profiles.Count + mercenaries.Count);
            result.AddRange(profiles);
            result.AddRange(mercenaries);

            if (max > 0 && result.Count > max) result.RemoveRange(max, result.Count - max);
            return result;
        }

        private static List<string> Collect(IEnumerable<string>? ids, HashSet<string> seen)
        {
            var list = new List<string>();
            if (ids == null) return list;
            foreach (var id in ids)
            {
                if (string.IsNullOrWhiteSpace(id)) continue;
                var trimmed = id.Trim();
                if (seen.Add(trimmed)) list.Add(trimmed);
            }

            return list;
        }

        // Fisher-Yates. 뒤에서부터 한 번씩만 바꾼다 - 치우침 없는 섞기다.
        private static void Shuffle(List<string> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                if (i == j) continue;
                var swap = list[i];
                list[i] = list[j];
                list[j] = swap;
            }
        }
    }
}
