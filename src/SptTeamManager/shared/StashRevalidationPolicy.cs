using System;
using System.Collections.Generic;

namespace pitTeam.Shared
{
    // 클라이언트가 EFT 에 "이 스태시 항목이 받아들여지는가" 를 물은 뒤의 항목 하나. 질문(EFT 의 아이템 필터)은
    // 게임에게만 할 수 있으니 클라이언트에 남고, 이 레코드는 그 답과 트리 위치(ParentId)만 나른다.
    // ParentId 는 어긴 항목의 자손을 함께 빼는 데 쓴다. 모르면 null 이고, 그러면 자손 판단에서 빠질 뿐이다.
    public sealed record StashCandidate(string? Id, bool Accepted, string? RejectionReason, string? ParentId = null);

    // 빠진 항목과 그 이유. Reason 은 절대 비지 않는다: 화면에서 아이템이 사라졌을 때 사용자가 로그에서 찾는 것이다.
    public sealed record StashRejection(string? Id, string Reason);

    public sealed record StashRevalidationResult(List<string> KeptIds, List<StashRejection> Skipped);

    // 스태시 재검증 결과를 "어떻게 처리하는가" 를 정한다. 2026-10-27 사고(doc/fixes/2026-10-27-pit-stash-ammo.md)에서
    // 9x19 탄창 속 5.56 탄 하나가 스태시 전체 재검증을 실패시켜 동료 생성이 중간에 끊겼다. 수리는 어긴 항목을
    // 그것만, 소리 내어 건너뛰는 것이다. 조용히 버리면 같은 사고가 또 오진된다.
    //
    // 순수하고 예외를 던지지 않는다. 신원에 대해 닫힌 쪽이다: 이름을 댈 수 없는 항목은 남기지 않는다.
    // 아래 모든 id 비교는 대소문자를 가리지 않는다 - 16진 id 의 표기 차이 때문에 어긴 항목을 놓치면 안 된다.
    public static class StashRevalidationPolicy
    {
        // EFT 오류문은 어긴 항목을 "(id: <24자리 16진>)" 으로 가리킨다. 실제 문구(위 문서의 로그):
        //   No filter for item Pro+ (id: 6adea532010015d86e5ffb7e) in StackSlot cartridges in item mag_glock_...
        // 닫는 괄호를 패턴에 넣은 것은 의도다: 길이가 틀린 id 를 잘라서 받아들이지 않고 거르는 것이 그 괄호다.
        private static readonly System.Text.RegularExpressions.Regex ItemIdPattern =
            new System.Text.RegularExpressions.Regex(@"\(id:\s*([0-9a-fA-F]{24})\)");

        // 오류문에서 어긴 항목의 id 를 뽑는다. 믿을 만한 것이 없으면 false 이고, 호출자는 어떤 항목을 뺄지 추측하면 안 된다.
        //
        // 왜 "첫 id 가 어긴 항목" 인가: 실제 문구가 어긴 항목(탄약)만 id 로 적고 부모 탄창은 이름으로만 적기 때문에
        // 문구에 id 가 하나뿐이고 그것이 어긴 항목의 것이다. 이 가정을 한 줄의 로그가 받친다. 그래서 서로 다른 id 가
        // 둘 이상 나오면(부모까지 id 로 적는 문구가 나오면 가정이 깨진 것) 어느 쪽인지 모르니 뽑지 않는다 -
        // 잘못된 항목을 건너뛰느니 예전처럼 실패하는 쪽이 낫다. 같은 id 의 반복은 하나로 본다.
        // 정책에 둔 이유: 이 가정이 주석이 아니라 테스트로 고정되게 하려는 것이다.
        public static bool TryExtractItemId(string? errorText, out string? itemId)
        {
            itemId = null;
            if (string.IsNullOrWhiteSpace(errorText)) return false;

            string? found = null;
            foreach (System.Text.RegularExpressions.Match match in ItemIdPattern.Matches(errorText!))
            {
                var candidate = match.Groups[1].Value;
                if (found == null)
                {
                    found = candidate;
                    continue;
                }

                if (!string.Equals(found, candidate, StringComparison.OrdinalIgnoreCase)) return false;
            }

            if (found == null) return false;

            itemId = found;
            return true;
        }

        public static StashRevalidationResult Partition(IEnumerable<StashCandidate>? items)
        {
            var kept = new List<string>();
            var skipped = new List<StashRejection>();
            if (items == null)
            {
                return new StashRevalidationResult(kept, skipped);
            }

            var list = new List<StashCandidate>(items);

            // 이름을 댈 수 있는 항목의 부모 링크와, EFT 가 직접 어긴 것으로 지목한 항목. 부모 사슬을 따라 올라갈 때 쓴다.
            var parentById = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            var rejectedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in list)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id)) continue;
                // 같은 id 가 둘이면 먼저 나온 것의 부모를 쓴다. 판단이 입력 순서에 기대는 것은 이 경우뿐이다.
                if (!parentById.ContainsKey(item.Id!)) parentById[item.Id!] = item.ParentId;
                if (!item.Accepted) rejectedIds.Add(item.Id!);
            }

            foreach (var item in list)
            {
                // 보고하지 않고 넘기지 않는다: 조용한 누락이 원래 사고를 오진으로 남겼다.
                if (item == null)
                {
                    skipped.Add(new StashRejection(null, "빈 항목이라 건너뛴다"));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.Id))
                {
                    skipped.Add(new StashRejection(item.Id, "id 가 없어 건너뛴다"));
                    continue;
                }

                if (!item.Accepted)
                {
                    var reason = string.IsNullOrWhiteSpace(item.RejectionReason)
                        ? "사유가 전해지지 않았다"
                        : item.RejectionReason!;
                    skipped.Add(new StashRejection(item.Id, reason));
                    continue;
                }

                // 어긴 항목의 자손은 붙을 곳이 없어진다. 남기면 트리 밖 고아가 되어 스냅샷이 다시 깨진다.
                var ancestor = FindRejectedAncestor(item.Id!, parentById, rejectedIds);
                if (ancestor != null)
                {
                    skipped.Add(new StashRejection(item.Id, $"상위 항목 {ancestor} 이(가) 규칙을 어겨 건너뛰어져 함께 뺀다"));
                    continue;
                }

                kept.Add(item.Id!);
            }

            return new StashRevalidationResult(kept, skipped);
        }

        // 스냅샷에는 있었는데 지금 스태시에 없는 id 중, 건너뛰기로 한 것이 아닌 것. 비어 있어야 정상이다.
        // 비어 있지 않으면 계단식 제외나 트리 구성이 건너뛰라 하지 않은 아이템을 잃었다는 뜻이니 호출자는 던져야 한다.
        // 현재 목록을 알 수 없으면(null) 전부 설명되지 않은 누락으로 친다 - 못 읽으면 "괜찮다" 가 아니다.
        public static List<string> FindUnexplainedMissingIds(
            IEnumerable<string>? snapshotIds,
            IEnumerable<string>? presentIds,
            IEnumerable<string>? skippedIds)
        {
            var missing = new List<string>();
            if (snapshotIds == null) return missing;

            var present = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (presentIds != null)
            {
                foreach (var id in presentIds)
                {
                    if (!string.IsNullOrWhiteSpace(id)) present.Add(id);
                }
            }

            var skip = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (skippedIds != null)
            {
                foreach (var id in skippedIds)
                {
                    if (!string.IsNullOrWhiteSpace(id)) skip.Add(id);
                }
            }

            foreach (var id in snapshotIds)
            {
                if (string.IsNullOrWhiteSpace(id)) continue;
                if (present.Contains(id) || skip.Contains(id)) continue;
                missing.Add(id);
            }

            return missing;
        }

        // 가장 가까운 조상이 아니라 "EFT 가 직접 어긴 것으로 지목한" 가장 가까운 조상을 돌려준다: 로그에서 사용자가
        // 따라갈 것은 중간 후손이 아니라 원인이다. 순환은 방문 집합으로 끝내고, 어긴 조상이 없으면 null(남긴다).
        private static string? FindRejectedAncestor(
            string id,
            Dictionary<string, string?> parentById,
            HashSet<string> rejectedIds)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { id };
            parentById.TryGetValue(id, out var parent);
            while (!string.IsNullOrWhiteSpace(parent))
            {
                if (rejectedIds.Contains(parent!)) return parent;
                // 이미 지나온 id 로 돌아왔다 = 순환. 더 올라갈 곳이 없다.
                if (!visited.Add(parent!)) return null;
                // 부모가 목록에 없으면 거기서 끝이다. 어긴 조상을 확인하지 못한 항목을 정책이 떨구지는 않는다.
                if (!parentById.TryGetValue(parent!, out parent)) return null;
            }

            return null;
        }
    }
}
