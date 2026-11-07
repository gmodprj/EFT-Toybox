using System;
using System.Collections.Generic;

namespace pitTeam.Shared
{
    // 스태시 아이템 하나의 자리. SPT 타입을 들이지 않으려고 필요한 네 가지만 옮겨 담는다.
    // (`Id`, 담긴 곳 `ParentId`, 담긴 칸 이름 `SlotId`, 아이템 종류 `TemplateId`)
    public sealed record StashItem(string Id, string? ParentId, string? SlotId, string TemplateId);

    // 건네 달라는 요청. 고른 것은 **스태시 바닥에 놓인 아이템**들이고, 그 안에 든 것은 따라간다.
    public sealed record StashTransferRequest(
        string? SourceProfileId,
        string? DestinationProfileId,
        IReadOnlyList<string> ItemIds);

    // 전송 계획. Accepted 가 거짓이면 **아무것도 옮기지 않는다** — Reason 이 왜인지 말한다.
    //
    // MovingItemIds 는 고른 것 **과 그 안에 든 것 전부**다(가방 하나를 고르면 내용물까지). 호출자는
    // 이 목록을 그대로 빼고 그대로 넣는다 — 그래서 이 목록 하나가 **총량 보존**의 단위다.
    public sealed record TransferPlan(
        bool Accepted,
        string Reason,
        IReadOnlyList<string> MovingItemIds);

    // 플레이어 스태시 ↔ 프로필 팀원 스태시 전송의 판정. 순수 — SPT 타입도 프로필 접근도 없고,
    // 실제 쓰기와 저널은 B2(서버 라우트)가 한다.
    //
    // 설계 §6-5 가 정한 규칙을 그대로 지킨다: **건넨 순간 소유가 넘어간다**(중간 상태 없음),
    // 3단 저널을 쓴다, 그리고 §6-4 의 "공짜 생성은 없다" — 이 정책은 **옮길 것을 고를 뿐 만들지
    // 않는다.** 그래서 출력은 "원본에 실제로 있는 id" 로만 이뤄진다.
    //
    // 불변식 하나: **총량 보존.** 빼는 쪽에서 사라지는 것과 넣는 쪽에서 생기는 것이 같은 목록이다.
    // 늘지도 줄지도 않는다. 그 목록을 여기서 한 번만 만들어 양쪽이 같은 것을 쓰게 한다.
    public static class StashTransferPolicy
    {
        // 스태시 바닥(그리드)의 칸 이름. 스태시 루트의 자식 중 이 칸에 놓인 것이 "바닥에 놓인 것" 이다.
        public const string StashSlot = "hideout";

        // 안전장치: 한 번에 옮길 수 있는 최대 개수(안에 든 것까지 센다). 사람이 손으로 고르는 화면의
        // 상한이고, 끊김 창을 짧게 유지한다 — 저널이 열려 있는 동안 서버가 죽으면 그만큼이 걸린다.
        public const int MaxItems = 512;

        /// <param name="sourceItems">원본 프로필의 아이템 전부. id → 자리.</param>
        /// <param name="sourceStashRootId">원본 스태시의 루트 아이템 id.</param>
        /// <param name="blockedTemplateIds">건넬 수 없는 아이템 종류(퀘스트 물품 등). 비어도 된다.</param>
        public static TransferPlan Decide(
            StashTransferRequest? request,
            IReadOnlyDictionary<string, StashItem>? sourceItems,
            string? sourceStashRootId,
            IReadOnlyCollection<string>? blockedTemplateIds)
        {
            if (request == null)
            {
                return Refused("요청이 비어 있어 옮기지 않는다.");
            }

            if (string.IsNullOrWhiteSpace(request.SourceProfileId)
                || string.IsNullOrWhiteSpace(request.DestinationProfileId))
            {
                return Refused("프로필 식별자가 비어 있어 옮기지 않는다.");
            }

            // 저널은 프로필 **둘**을 전제한다 — 같은 프로필이면 Prepare 가 던지고, 억지로 쓰면
            // 그 프로필이 다음 기동부터 영구히 막힌다(설계 §6-8 의 2026-11-02 정정).
            if (string.Equals(request.SourceProfileId!.Trim(), request.DestinationProfileId!.Trim(), StringComparison.Ordinal))
            {
                return Refused("같은 프로필 안의 이동은 전송이 아니다. 장비 편집 화면을 쓴다.");
            }

            if (sourceItems == null || sourceItems.Count == 0)
            {
                return Refused("원본 스태시를 읽지 못해 옮기지 않는다.");
            }

            if (string.IsNullOrWhiteSpace(sourceStashRootId))
            {
                return Refused("원본 스태시의 루트를 찾지 못해 옮기지 않는다.");
            }

            if (request.ItemIds == null || request.ItemIds.Count == 0)
            {
                return Refused("고른 것이 없어 옮기지 않는다.");
            }

            var rootId = sourceStashRootId!.Trim();
            var blocked = blockedTemplateIds == null
                ? new HashSet<string>(StringComparer.Ordinal)
                : new HashSet<string>(blockedTemplateIds, StringComparer.Ordinal);

            // 고른 것 자체의 검사. 여기서 걸리면 **한 건이라도 걸리면 전체를 거절한다** —
            // 일부만 옮기면 화면이 보여 준 것과 실제가 어긋나고, 사람이 무엇이 갔는지 모른다.
            var picked = new List<string>(request.ItemIds.Count);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var raw in request.ItemIds)
            {
                if (string.IsNullOrWhiteSpace(raw))
                {
                    return Refused("고른 것 중에 빈 식별자가 있어 옮기지 않는다.");
                }

                var id = raw.Trim();
                if (!seen.Add(id))
                {
                    // 같은 것을 두 번 세면 넣는 쪽이 두 개로 읽을 수 있다. 조용히 지우지 않는다.
                    return Refused($"같은 아이템을 두 번 골랐다('{id}').");
                }

                if (!sourceItems.TryGetValue(id, out var item))
                {
                    return Refused($"원본 스태시에 없는 아이템이다('{id}').");
                }

                // **스태시 바닥에 놓인 것만** 건넨다. 입고 있는 것·총에 달린 것·가방 안의 것은
                // 따로 고를 수 없다 — 가방째 건네면 안에 든 것은 따라간다.
                if (!string.Equals(item.ParentId, rootId, StringComparison.Ordinal)
                    || !string.Equals(item.SlotId, StashSlot, StringComparison.Ordinal))
                {
                    return Refused($"스태시 바닥에 놓인 것만 건넬 수 있다('{id}').");
                }

                if (blocked.Contains(item.TemplateId))
                {
                    return Refused($"건넬 수 없는 종류의 아이템이다('{id}').");
                }

                picked.Add(id);
            }

            // 안에 든 것까지 모은다. 이 목록이 빼는 쪽과 넣는 쪽 양쪽의 단위다.
            var moving = Closure(picked, sourceItems);

            if (moving.Count > MaxItems)
            {
                return Refused($"한 번에 옮길 수 있는 것은 {MaxItems}개까지다(고른 것 안에 든 것까지 {moving.Count}개).");
            }

            return new TransferPlan(
                Accepted: true,
                Reason: $"스태시 전송 — 고른 {picked.Count}개, 안에 든 것까지 {moving.Count}개.",
                MovingItemIds: moving);
        }

        // 고른 것과 그 자손 전부. 부모 → 자식 색인을 한 번 만들고 너비 우선으로 내려간다.
        // 이미 담은 id 는 다시 담지 않으므로 **고리가 있어도 멈춘다**(손상된 프로필에서 나올 수 있다).
        private static List<string> Closure(List<string> picked, IReadOnlyDictionary<string, StashItem> items)
        {
            var children = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (var item in items.Values)
            {
                if (string.IsNullOrEmpty(item.ParentId)) continue;
                if (!children.TryGetValue(item.ParentId!, out var list))
                {
                    list = [];
                    children[item.ParentId!] = list;
                }

                list.Add(item.Id);
            }

            var result = new List<string>();
            var taken = new HashSet<string>(StringComparer.Ordinal);
            var queue = new Queue<string>(picked);
            while (queue.Count > 0)
            {
                var id = queue.Dequeue();
                if (!taken.Add(id)) continue;
                result.Add(id);

                if (!children.TryGetValue(id, out var kids)) continue;
                foreach (var kid in kids) queue.Enqueue(kid);
            }

            return result;
        }

        private static TransferPlan Refused(string reason) =>
            new TransferPlan(Accepted: false, Reason: reason, MovingItemIds: []);
    }
}
