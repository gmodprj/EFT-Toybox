using System;
using System.Collections.Generic;

namespace pitTeam.Shared
{
    // 편집기가 끝난 뒤의 **그 팀원 인벤토리 전체**를 한 아이템씩 줄인 것. 클라이언트가 EFT 트리를 평평하게
    // 펴서 만들고, 서버는 디스크의 프로필에서 같은 모양을 만든다.
    public sealed record InventoryItemState(string? Id, string? TemplateId, string? ParentId, string? SlotId, int StackCount);

    // Allowed == false 면 Reason 이 플레이어가 읽는 한 줄이다. LostItemIds 는 **사라지는** 아이템들이고,
    // 거절 사유가 아니라 호출자가 로그에 남길 사실이다(버리기·탄박스 소비는 정당한 조작이다).
    public sealed record InventoryApplyVerdict(bool Allowed, string Reason, IReadOnlyList<string> LostItemIds);

    // 계획 5 Task B 의 판정. **조작의 종류를 세지 않는다** - 이동·나누기·합치기·열기·버리기를 하나씩
    // 열어 주는 방식은 셀 때마다 빠지고, 다음 패치에서 EFT 가 조작을 더하면 또 빠진다. 대신 최종 상태를
    // 받아 "이 변화가 정당한가" 만 본다.
    //
    // 막는 것은 셋이다:
    //   1. **늘어나는 것** - tpl 별 총 수량이 before 보다 많으면 복제다.
    //   2. **바깥에서 들어오는 것** - before 에 없던 종류가 생기면 플레이어 스태시에서 온 것이다.
    //   3. **구조 손상** - 주머니·보안 컨테이너·완장·칼집·인식표가 없어지거나 다른 자리로 가면 EFT 가
    //      그 프로필의 적재를 거부할 수 있다. 플레이어 화면에서도 그것들은 움직이지 않는다.
    // 그리고 트리가 멀쩡한지(두 루트에 다 닿는지, 고리가 없는지)를 본다.
    //
    // **줄어드는 것은 막지 않는다.** 플레이어는 자기 화면에서 버리고 탄박스를 소비한다 - 팀원 화면도
    // 같아야 한다는 것이 이 계획의 전제다. 대신 사라지는 id 를 전부 돌려주어 호출자가 로그에 남긴다.
    public static class ProfileInventoryApplyPolicy
    {
        // 부모를 따라 올라갈 때의 걸음 제한. 손상된 트리(고리)에서 돌지 않게 한다.
        private const int MaxDepth = 64;

        private static InventoryApplyVerdict Reject(string reason)
            => new InventoryApplyVerdict(false, reason, Array.Empty<string>());

        public static InventoryApplyVerdict Validate(
            IReadOnlyList<InventoryItemState>? before,
            IReadOnlyList<InventoryItemState>? after,
            string? stashRootId,
            string? equipmentRootId)
        {
            if (before == null || before.Count == 0) return Reject("편집 전 상태를 읽을 수 없어 저장하지 않습니다.");
            if (after == null || after.Count == 0) return Reject("편집기의 상태를 읽을 수 없어 저장하지 않습니다.");
            if (string.IsNullOrWhiteSpace(stashRootId)) return Reject("이 프로필의 스태시 루트를 읽을 수 없어 저장하지 않습니다.");
            if (string.IsNullOrWhiteSpace(equipmentRootId)) return Reject("이 프로필의 장비 루트를 읽을 수 없어 저장하지 않습니다.");

            var stashRoot = stashRootId!.Trim();
            var equipmentRoot = equipmentRootId!.Trim();

            var beforeById = new Dictionary<string, InventoryItemState>(StringComparer.Ordinal);
            foreach (var item in before)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id)) return Reject("편집 전 상태에 id 가 없는 아이템이 있어 저장하지 않습니다.");
                if (beforeById.ContainsKey(item.Id!)) return Reject($"편집 전 상태에 같은 id '{item.Id}' 가 두 번 있어 저장하지 않습니다.");
                beforeById.Add(item.Id!, item);
            }

            var afterById = new Dictionary<string, InventoryItemState>(StringComparer.Ordinal);
            foreach (var item in after)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id)) return Reject("저장할 상태에 id 가 없는 아이템이 있어 저장하지 않습니다.");
                if (afterById.ContainsKey(item.Id!)) return Reject($"저장할 상태에 같은 id '{item.Id}' 가 두 번 있어 저장하지 않습니다.");
                if (string.IsNullOrWhiteSpace(item.TemplateId)) return Reject($"아이템 '{item.Id}' 의 종류를 읽을 수 없어 저장하지 않습니다.");
                afterById.Add(item.Id!, item);
            }

            // 두 루트는 그대로여야 한다. 루트가 빠지면 그 트리가 통째로 사라지고, 루트가 다른 것 밑으로
            // 들어가면 트리 하나가 다른 트리의 일부가 된다.
            foreach (var rootId in new[] { stashRoot, equipmentRoot })
            {
                if (!afterById.TryGetValue(rootId, out var root))
                {
                    return Reject($"저장할 상태에 루트 '{rootId}' 가 없어 저장하지 않습니다.");
                }

                if (!string.IsNullOrWhiteSpace(root.ParentId))
                {
                    return Reject($"루트 '{rootId}' 가 다른 아이템 밑으로 들어가 있어 저장하지 않습니다.");
                }
            }

            // 모든 아이템이 두 루트 중 하나에 닿는가. 닿지 않으면 고아이거나 고리다 - 둘 다 그 가지가
            // 프로필에서 사라진 것처럼 보인다.
            foreach (var item in after)
            {
                var id = item.Id!;
                if (string.Equals(id, stashRoot, StringComparison.Ordinal) || string.Equals(id, equipmentRoot, StringComparison.Ordinal)) continue;

                var current = item;
                var reachedRoot = false;
                for (var steps = 0; steps < MaxDepth; steps++)
                {
                    var parentId = current.ParentId;
                    if (string.IsNullOrWhiteSpace(parentId)) break;

                    if (string.Equals(parentId, stashRoot, StringComparison.Ordinal)
                        || string.Equals(parentId, equipmentRoot, StringComparison.Ordinal))
                    {
                        reachedRoot = true;
                        break;
                    }

                    if (!afterById.TryGetValue(parentId!, out var parent)) break;

                    current = parent;
                }

                if (!reachedRoot)
                {
                    return Reject($"아이템 '{id}' 이(가) 스태시와 장비 어느 쪽에도 매달려 있지 않아 저장하지 않습니다.");
                }
            }

            // 같은 id 의 종류가 바뀌면 그것은 더 이상 같은 아이템이 아니다.
            foreach (var pair in afterById)
            {
                if (!beforeById.TryGetValue(pair.Key, out var original)) continue;
                if (!string.Equals(original.TemplateId, pair.Value.TemplateId, StringComparison.Ordinal))
                {
                    return Reject($"아이템 '{pair.Key}' 의 종류가 '{original.TemplateId}' 에서 '{pair.Value.TemplateId}' 로 바뀌어 저장하지 않습니다.");
                }
            }

            // 구조를 이루는 자리는 같은 id 가 같은 부모의 같은 자리에 그대로 있어야 한다.
            foreach (var item in before)
            {
                if (!ProfileGearEditPolicy.IsStructuralSlot(item.SlotId)) continue;
                if (!afterById.TryGetValue(item.Id!, out var moved))
                {
                    return Reject($"'{item.SlotId}' 의 아이템 '{item.Id}' 이(가) 없어졌습니다. 프로필에서 떼어낼 수 없는 자리라 저장하지 않습니다.");
                }

                if (!string.Equals(moved.ParentId, item.ParentId, StringComparison.Ordinal)
                    || !string.Equals(moved.SlotId, item.SlotId, StringComparison.Ordinal))
                {
                    return Reject($"아이템 '{item.Id}' 은(는) '{item.SlotId}' 에서 움직일 수 없는 자리라 저장하지 않습니다.");
                }
            }

            // 종류별 총 수량. 늘면 복제이고, before 에 없던 종류면 바깥에서 들어온 것이다.
            var beforeTotals = TotalsByTemplate(before);
            var afterTotals = TotalsByTemplate(after);
            foreach (var pair in afterTotals)
            {
                if (!beforeTotals.TryGetValue(pair.Key, out var allowed))
                {
                    return Reject($"'{pair.Key}' 는 이 팀원이 갖고 있던 종류가 아니어서 저장하지 않습니다.");
                }

                if (pair.Value > allowed)
                {
                    return Reject($"'{pair.Key}' 의 수가 {allowed} 에서 {pair.Value} 로 늘어 저장하지 않습니다. 편집은 아이템을 만들지 않습니다.");
                }
            }

            // 사라지는 것은 막지 않는다(버리기·탄박스 소비). 무엇이 사라지는지는 돌려준다.
            var lost = new List<string>();
            foreach (var item in before)
            {
                if (!afterById.ContainsKey(item.Id!)) lost.Add(item.Id!);
            }

            return new InventoryApplyVerdict(true, string.Empty, lost);
        }

        private static Dictionary<string, int> TotalsByTemplate(IReadOnlyList<InventoryItemState> items)
        {
            var totals = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var item in items)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.TemplateId)) continue;
                var count = item.StackCount > 0 ? item.StackCount : 1;
                totals.TryGetValue(item.TemplateId!, out var sum);
                totals[item.TemplateId!] = sum + count;
            }

            return totals;
        }
    }
}
