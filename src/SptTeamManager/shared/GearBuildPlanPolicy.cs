using System;
using System.Collections.Generic;
using System.Linq;

namespace pitTeam.Shared
{
    // 아이템 한 점을 SPT 타입 없이 옮긴 것. 판정이 서버 프로젝트 밖(테스트)에서 검증되어야 한다.
    public sealed record GearItemRow(string Id, string Tpl, string? ParentId, string? SlotId);

    public enum GearSlotOutcome
    {
        // 빌드와 팀원이 같은 것을 들고 있다. 사지도 벗기지도 않는다.
        Unchanged,
        // 팀원이 쓰던 것을 벗기고 빌드의 것을 산다.
        Replaced,
        // 팀원에게 없던 자리다. 사기만 한다.
        Added,
        // 빌드에 없는 자리다. 벗기기만 한다.
        Removed,
    }

    // 장비 칸 하나에 대한 계획. ItemsToBuy 는 빌드 쪽 하위 묶음 전체, ItemsToStash 는 팀원이 쓰던 하위 묶음 전체다.
    public sealed record GearSlotPlan(
        string SlotId,
        GearSlotOutcome Outcome,
        IReadOnlyList<GearItemRow> ItemsToBuy,
        IReadOnlyList<GearItemRow> ItemsToStash);

    public sealed record GearBuildPlan(IReadOnlyList<GearSlotPlan> Slots)
    {
        public IReadOnlyList<GearItemRow> AllItemsToBuy => Slots.SelectMany(s => s.ItemsToBuy).ToList();

        public IReadOnlyList<GearItemRow> AllItemsToStash => Slots.SelectMany(s => s.ItemsToStash).ToList();

        public bool NothingToDo => Slots.All(s => s.Outcome == GearSlotOutcome.Unchanged);
    }

    // 고른 장비 빌드와 팀원이 지금 입고 있는 것을 견주어, **칸 단위로** 무엇을 사고 무엇을 벗길지 정한다.
    //
    // 왜 칸 단위인가: 사용자가 정한 것이 "전부 새로 구매" 다(2026-11-02). 부착물 하나하나를 견주어
    // 모자란 것만 사면 팀원이 쓰던 총을 뜯어 부품을 갈아 끼워야 하는데, 그것은 프로필 두 개를 오가는
    // 조작보다도 되돌리기 어렵다. 칸 하나는 통째로 같거나 통째로 바뀐다.
    //
    // "같다" 의 뜻: 그 칸의 아이템과 **그 아래 붙어 있는 것 전부**가 tpl 과 자리까지 같다. 하나라도 다르면
    // 다른 장비다(같은 총이라도 조준경이 다르면 사용자가 고른 것과 다르다).
    //
    // 순수하다. 부착물 칸 목록만 밖에서 받는다 - 그것은 아이템 템플릿을 읽어야 알 수 있고, 그 읽기는
    // 서버의 일이다(PIT 의 TeammateEquipmentPrice 가 같은 방식으로 받는다).
    public static class GearBuildPlanPolicy
    {
        // 장비의 뿌리 칸. **PIT 의 TeammateEquipmentPrice.Roots 와 같은 열 자리여야 한다** - 한쪽은 값을
        // 매기고 한쪽은 무엇을 살지 정하므로, 어긋나면 값을 매기지 않은 것을 사거나 산 것에 값을 매기지
        // 않는다. 중복 자체는 의도다(정책은 SPT 어셈블리 없이 테스트돼야 한다). 일치는 주석이 아니라
        // 기동 검사가 지킨다(PitFireTeamServerPostLoad).
        private static readonly string[] RootSlots =
        {
            "FirstPrimaryWeapon", "SecondPrimaryWeapon", "Holster", "Headwear", "FaceCover",
            "Eyewear", "Earpiece", "TacticalVest", "ArmorVest", "Backpack",
        };

        public static IReadOnlyList<string> RootSlotNames => RootSlots;

        public static GearBuildPlan Plan(
            IEnumerable<GearItemRow>? buildItems,
            string? buildRootId,
            IEnumerable<GearItemRow>? currentItems,
            string? currentEquipmentId,
            Func<string, IEnumerable<string>>? attachmentSlotsOfTpl)
        {
            var build = Index(buildItems);
            var current = Index(currentItems);
            var slotsOf = attachmentSlotsOfTpl ?? (_ => Array.Empty<string>());

            var plans = new List<GearSlotPlan>();
            foreach (var slot in RootSlots)
            {
                var buildTop = TopOf(build, buildRootId, slot);
                var currentTop = TopOf(current, currentEquipmentId, slot);

                if (buildTop == null && currentTop == null) continue;

                var buildTree = Subtree(build, buildTop, slotsOf);
                var currentTree = Subtree(current, currentTop, slotsOf);

                if (buildTop == null)
                {
                    plans.Add(new GearSlotPlan(slot, GearSlotOutcome.Removed, Array.Empty<GearItemRow>(), currentTree));
                    continue;
                }

                if (currentTop == null)
                {
                    plans.Add(new GearSlotPlan(slot, GearSlotOutcome.Added, buildTree, Array.Empty<GearItemRow>()));
                    continue;
                }

                if (SameShape(buildTree, currentTree))
                {
                    plans.Add(new GearSlotPlan(slot, GearSlotOutcome.Unchanged, Array.Empty<GearItemRow>(), Array.Empty<GearItemRow>()));
                    continue;
                }

                plans.Add(new GearSlotPlan(slot, GearSlotOutcome.Replaced, buildTree, currentTree));
            }

            return new GearBuildPlan(plans);
        }

        private static List<GearItemRow> Index(IEnumerable<GearItemRow>? items) =>
            items?.Where(i => i != null && !string.IsNullOrWhiteSpace(i.Id)).ToList() ?? new List<GearItemRow>();

        // 그 칸에 바로 들어 있는 아이템. 같은 칸에 둘이 있으면(있어서는 안 되는 자료다) 처음 것만 본다.
        private static GearItemRow? TopOf(List<GearItemRow> items, string? parentId, string slot)
        {
            if (string.IsNullOrWhiteSpace(parentId)) return null;
            return items.FirstOrDefault(i =>
                string.Equals(i.ParentId, parentId, StringComparison.Ordinal) &&
                string.Equals(i.SlotId, slot, StringComparison.OrdinalIgnoreCase));
        }

        // 그 아이템과, 부착물 칸에 달린 것 전부. 가방·조끼 안의 짐(격자 내용물)은 칸 이름이 부착물 칸이
        // 아니므로 자연히 빠진다 - 짐은 장비가 아니고, 사 주는 대상도 아니다.
        private static List<GearItemRow> Subtree(List<GearItemRow> items, GearItemRow? top, Func<string, IEnumerable<string>> slotsOf)
        {
            var result = new List<GearItemRow>();
            if (top == null) return result;

            var seen = new HashSet<string>(StringComparer.Ordinal);
            var queue = new Queue<GearItemRow>();
            queue.Enqueue(top);

            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                if (!seen.Add(item.Id)) continue;
                result.Add(item);

                var slots = new HashSet<string>(slotsOf(item.Tpl) ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
                if (slots.Count == 0) continue;

                foreach (var child in items.Where(i =>
                    string.Equals(i.ParentId, item.Id, StringComparison.Ordinal) &&
                    i.SlotId != null && slots.Contains(i.SlotId)))
                {
                    queue.Enqueue(child);
                }
            }

            return result;
        }

        // id 는 빌드와 프로필에서 서로 다르므로 견주지 않는다. "무엇이 어느 자리에 있는가" 만 본다.
        private static bool SameShape(List<GearItemRow> left, List<GearItemRow> right)
        {
            if (left.Count != right.Count) return false;

            static List<string> Shape(List<GearItemRow> items)
            {
                var byId = items.ToDictionary(i => i.Id, StringComparer.Ordinal);

                string Path(GearItemRow item)
                {
                    var parts = new List<string>();
                    var cursor = item;
                    var guard = 0;
                    while (cursor != null && guard++ < 64)
                    {
                        parts.Add(cursor.SlotId ?? string.Empty);
                        if (cursor.ParentId == null || !byId.TryGetValue(cursor.ParentId, out var parent)) break;
                        cursor = parent;
                    }

                    parts.Reverse();
                    return string.Join("/", parts) + "=" + item.Tpl;
                }

                return items.Select(Path).OrderBy(p => p, StringComparer.Ordinal).ToList();
            }

            return Shape(left).SequenceEqual(Shape(right), StringComparer.Ordinal);
        }
    }
}
