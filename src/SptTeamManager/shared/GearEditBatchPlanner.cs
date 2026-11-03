using System;
using System.Collections.Generic;

namespace pitTeam.Shared
{
    // 편집기 화면이 들고 있는 아이템 하나를 이 판정에 필요한 네 칸으로 줄인 것. 클라이언트가 EFT 아이템 트리를
    // 평평하게 펴서 만든다 - 여기서는 그 타입이 있다는 것조차 모른다. LayoutKey 는 이 경로가 저장하지
    // 못하는 속성인 격자 칸 위치를 문자열로 바꾼 것이다. 같으면 같은 문자열이면 되고 뜻은 보지 않는다.
    public sealed record EditorItemState(string? Id, string? ParentId, string? SlotId, string? LayoutKey);

    // 서버 라우트(/stm/members/edit-gear)에 보낼 한 건: 이 아이템을 이 부모의 이 슬롯으로.
    public sealed record PlannedGearEdit(string ItemId, string TargetSlotId, string TargetParentId);

    // Ok == false 면 Reason 이 플레이어가 읽는 한 줄이고 Edits 는 비어 있다. LayoutOnlyCount 는 부모·슬롯은
    // 그대로이고 LayoutKey 만 바뀐 아이템의 수 - 이 경로가 좌표를 받지 않아 저장되지 않는 변화다.
    public sealed record GearEditPlan(bool Ok, string Reason, IReadOnlyList<PlannedGearEdit> Edits, int LayoutOnlyCount);

    // 편집기를 연 때의 상태와 저장 버튼을 누른 때의 상태를 비교해, 서버에 보낼 "한 건씩의 이동" 목록과 그 순서를 정한다.
    //
    // 왜 편집기가 최종 인벤토리를 내지 않는가: PIT 의 편집기는 제출한 쪽이 곧 새 상태가 되는 모양이라 빠진 아이템이
    // 조용히 사라진다(그 구멍을 메우려고 서버에 보존 검사를 따로 넣었다). STM 은 반대다 - 클라이언트는 "무엇을 어디로"
    // 만 말하고, 최종 집합은 서버가 자기 프로필에서 만든다. 그래서 여기서 하는 일은 두 가지뿐이다:
    //   1) 바뀐 것을 건으로 바꾼다. 서브트리는 부모와 함께 가므로 부모가 바뀐 아이템만 건이 된다.
    //   2) 서버가 건을 받은 순서대로 누적해 판정하므로(ProfileGearEditPolicy.ValidateBatch) 맞는 순서로 세운다.
    //
    // 순서의 두 제약(둘 다 "먼저 해야 할 건" 을 가리킨다):
    //   - 새 부모가 이번에 같이 옮겨지는 아이템이면 그 부모의 건이 먼저다: 입히는 아이템의 부모는 그 순간 장비
    //     트리에 있어야 한다("탄창을 총에 꽂고 그 탄창에 탄을 넣는" 연쇄).
    //   - 옛 부모가 이번에 같이 옮겨지는 아이템이면 이 아이템을 꺼내는 건이 먼저다: 부모가 먼저 다른 트리로 가면
    //     자식은 이미 출처 트리 밖에 있다(벗길 때 "총과 그 탄창을 함께 스태시로").
    //   - 어떤 자리를 **비우는** 건이 그 자리를 **채우는** 건보다 먼저다: 서버는 한 슬롯에 하나만 허용하므로
    //     순서가 뒤집히면 "그 슬롯에는 이미 아이템이 있어 넣지 않습니다" 로 맞는 편집이 거절된다
    //     (2026-11-03 인게임: 무기의 탄창을 스태시로 빼고 리그의 탄창을 그 자리에 꽂는 교환이 이것 때문에 막혔다).
    // 두 제약이 서로를 가리키면(순환) 순서를 지어내지 않고 거부한다.
    //
    // 서버가 표현하지 못하는 변화는 조용히 버리지 않고 거부하거나(새로 생기거나 사라진 아이템 - 묶음 쌓기를 나누고
    // 합치는 조작) 개수로 알린다(칸 위치만 바뀜). 의도적으로 catch 가 없다: 입력이 닿는 모든 경로는 아래 반환으로 끝난다.
    public static class GearEditBatchPlanner
    {
        // 자리 하나를 (부모, 슬롯) 한 문자열로 적을 때 쓰는 구분자. 아이템 id 는 16진이고 슬롯 이름에도
        // 이 모양은 없다 - 둘을 그냥 이으면 서로 다른 자리가 같은 열쇠가 될 수 있다.
        private const string SlotKeySeparator = "::";

        private static GearEditPlan Refuse(string reason)
            => new GearEditPlan(false, reason, Array.Empty<PlannedGearEdit>(), 0);

        public static GearEditPlan Plan(IReadOnlyList<EditorItemState>? before, IReadOnlyList<EditorItemState>? after)
        {
            if (before == null || before.Count == 0 || after == null || after.Count == 0)
            {
                return Refuse("편집기의 상태를 읽을 수 없어 저장하지 않습니다.");
            }

            if (!TryIndex(before, out var beforeById, out var duplicate)
                || !TryIndex(after, out var afterById, out duplicate))
            {
                return Refuse($"편집기에 같은 아이템 '{duplicate}' 이(가) 두 번 나와 저장하지 않습니다.");
            }

            foreach (var item in after)
            {
                if (!beforeById.ContainsKey(item.Id!))
                {
                    return Refuse($"편집 중 새로 생긴 아이템 '{item.Id}' 이(가) 있어 저장하지 않습니다. 묶음을 나누는 조작은 저장되지 않습니다.");
                }
            }

            foreach (var item in before)
            {
                if (!afterById.ContainsKey(item.Id!))
                {
                    return Refuse($"편집 중 사라진 아이템 '{item.Id}' 이(가) 있어 저장하지 않습니다. 버리거나 합치는 조작은 저장되지 않습니다.");
                }
            }

            // 바뀐 것을 끝 상태 목록의 순서로 모은다(제약이 없는 건들의 순서가 입력에 대해 결정적이도록).
            var edits = new List<PlannedGearEdit>();
            var locationOnly = 0;
            foreach (var item in after)
            {
                var original = beforeById[item.Id!];
                var sameParent = string.Equals(original.ParentId, item.ParentId, StringComparison.Ordinal);
                var sameSlot = string.Equals(original.SlotId, item.SlotId, StringComparison.Ordinal);
                if (sameParent && sameSlot)
                {
                    if (!string.Equals(original.LayoutKey, item.LayoutKey, StringComparison.Ordinal))
                    {
                        locationOnly++;
                    }

                    continue;
                }

                // 어디로 가는지 모르면 서버에 말할 수 없다. 지어내지 않는다.
                if (string.IsNullOrWhiteSpace(item.ParentId) || string.IsNullOrWhiteSpace(item.SlotId))
                {
                    return Refuse($"아이템 '{item.Id}' 이(가) 놓인 자리를 읽지 못해 저장하지 않습니다.");
                }

                edits.Add(new PlannedGearEdit(item.Id!, item.SlotId!, item.ParentId!));
            }

            if (!TryOrder(edits, beforeById, out var ordered))
            {
                return Refuse("옮긴 아이템들의 순서를 정할 수 없어 저장하지 않습니다. 한 번에 옮기는 수를 줄여 다시 시도하세요.");
            }

            return new GearEditPlan(true, string.Empty, ordered, locationOnly);
        }

        // id 가 비었거나 겹치면 어느 아이템인지 가릴 수 없다. 겹친 id 를 돌려준다(빈 id 는 빈 문자열로 말한다).
        private static bool TryIndex(
            IReadOnlyList<EditorItemState> items,
            out Dictionary<string, EditorItemState> byId,
            out string duplicate)
        {
            byId = new Dictionary<string, EditorItemState>(StringComparer.Ordinal);
            duplicate = string.Empty;
            foreach (var item in items)
            {
                // Dictionary.TryAdd 는 net472 에 없다(이 파일은 클라이언트에도 링크된다).
                if (item == null || string.IsNullOrWhiteSpace(item.Id) || byId.ContainsKey(item.Id!))
                {
                    duplicate = item?.Id ?? string.Empty;
                    return false;
                }

                byId.Add(item.Id!, item);
            }

            return true;
        }

        // Kahn 의 위상 정렬. 같은 차례에 놓을 수 있는 건들은 원래 순서를 지킨다.
        private static bool TryOrder(
            List<PlannedGearEdit> edits,
            Dictionary<string, EditorItemState> beforeById,
            out List<PlannedGearEdit> ordered)
        {
            ordered = new List<PlannedGearEdit>(edits.Count);
            var indexById = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var i = 0; i < edits.Count; i++)
            {
                indexById[edits[i].ItemId] = i;
            }

            // mustFollow[i] = i 번 건보다 먼저 와야 하는 건들의 수. successors[j] = j 번 건 다음에 와야 하는 건들.
            var waitingFor = new int[edits.Count];
            var successors = new List<int>[edits.Count];
            for (var i = 0; i < edits.Count; i++)
            {
                successors[i] = new List<int>();
            }

            void Require(int first, int then)
            {
                if (first == then) return;
                if (successors[first].Contains(then)) return;
                successors[first].Add(then);
                waitingFor[then]++;
            }

            // 비우는 건을 찾기 위한 색인: (옛 부모, 옛 슬롯) → 그 자리를 떠나는 건. 슬롯 이름은
            // 서버 판정과 같은 기준으로 맞춘다(대소문자 무시).
            var vacating = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < edits.Count; i++)
            {
                var origin = beforeById[edits[i].ItemId];
                if (string.IsNullOrWhiteSpace(origin.ParentId) || string.IsNullOrWhiteSpace(origin.SlotId)) continue;
                var key = origin.ParentId + SlotKeySeparator + origin.SlotId;
                // 같은 자리를 둘이 떠나는 프로필은 이미 깨져 있다(서버의 점유 검사가 잡는다). 먼저 것을 쓴다.
                if (!vacating.ContainsKey(key)) vacating.Add(key, i);
            }

            for (var i = 0; i < edits.Count; i++)
            {
                // 새 부모의 건이 먼저.
                if (indexById.TryGetValue(edits[i].TargetParentId, out var newParentEdit))
                {
                    Require(newParentEdit, i);
                }

                // 옛 부모의 건보다 이 건이 먼저.
                var oldParentId = beforeById[edits[i].ItemId].ParentId;
                if (oldParentId != null && indexById.TryGetValue(oldParentId, out var oldParentEdit))
                {
                    Require(i, oldParentEdit);
                }

                // 그 자리를 비우는 건이 먼저. 서로 자리를 맞바꾸면 고리가 되고, 그때는 순서를 지어내지
                // 않고 거절한다(중간 자리가 필요한 조작이다).
                var targetKey = edits[i].TargetParentId + SlotKeySeparator + edits[i].TargetSlotId;
                if (vacating.TryGetValue(targetKey, out var vacatingEdit))
                {
                    Require(vacatingEdit, i);
                }
            }

            var ready = new SortedSet<int>();
            for (var i = 0; i < edits.Count; i++)
            {
                if (waitingFor[i] == 0) ready.Add(i);
            }

            while (ready.Count > 0)
            {
                var next = ready.Min;
                ready.Remove(next);
                ordered.Add(edits[next]);
                foreach (var after in successors[next])
                {
                    if (--waitingFor[after] == 0) ready.Add(after);
                }
            }

            if (ordered.Count != edits.Count)
            {
                ordered = new List<PlannedGearEdit>();
                return false;
            }

            return true;
        }
    }
}
