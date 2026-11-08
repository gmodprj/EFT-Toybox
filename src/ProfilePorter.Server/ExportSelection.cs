using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfilePorter.Server;

/// <summary>고를 수 있는지의 판정 결과. <see cref="Accepted"/> 가 거짓이면 **아무것도 내보내지 않는다.**</summary>
public sealed record ExportPlan(
    bool Accepted,
    string Reason,
    IReadOnlyList<string> ItemIds,
    IReadOnlyList<string> Skipped);

/// <summary>
/// 무엇을 내보낼지 고르는 판정. **순수** — 프로필 파일도 SPT 타입도 건드리지 않는다.
///
/// 설계 §4.5(내보내지 않는 것)와 §9.1(재 본 것)이 여기 들어 있다. 특히 **상한은 고른 개수가
/// 아니라 자손까지 센 수**로 둔다 — 실측에서 56개를 고르면 1,358개가 따라오는 프로필이 있었고,
/// 자손이 하나도 없는 프로필도 있었다. 고른 개수로 재면 뜻이 없다.
///
/// 내보내기는 **원본을 고치지 않는다**(설계 §2.2). 그래서 여기서 나오는 것은 "원본에 실제로
/// 있는 id" 뿐이고, 이 판정이 무엇을 만들어 내는 일은 없다.
/// </summary>
public static class ExportSelection
{
    /// <summary>스태시 바닥(주 격자)의 칸 이름. STM 의 `StashTransferPolicy.StashSlot` 과 같은 값이다.</summary>
    public const string StashSlot = "hideout";

    /// <summary>보안 컨테이너가 달리는 칸. 설계 §4.5 가 통째로 빼기로 한 자리다.</summary>
    public const string SecuredContainerSlot = "SecuredContainer";

    /// <summary>
    /// 한 번에 내보낼 수 있는 최대 개수(**자손까지 센 수**).
    ///
    /// 실측(§9.1)에서 가장 큰 것이 Nomad 의 1,358개였다. 그 두 배가 안 되는 자리에 둔다 —
    /// 사람이 손으로 고르는 화면의 상한이고, 넘으면 "나눠서 하라" 고 말해 준다.
    /// </summary>
    public const int MaxItems = 2048;

    /// <param name="items">원본 프로필의 아이템 전부.</param>
    /// <param name="stashRootId">스태시 루트 아이템의 id.</param>
    /// <param name="selectedIds">사람이 고른 것(스태시 바닥에 놓인 것이어야 한다).</param>
    /// <param name="blockedTemplateIds">내보낼 수 없는 아이템 종류(퀘스트 물품 등). 비어도 된다.</param>
    public static ExportPlan PlanStash(
        IReadOnlyCollection<PorterItem> items,
        string? stashRootId,
        IReadOnlyCollection<string> selectedIds,
        IReadOnlyCollection<string>? blockedTemplateIds = null)
    {
        if (items is null || items.Count == 0)
        {
            return Refuse("원본 프로필에서 아이템을 읽지 못했습니다.");
        }

        if (string.IsNullOrEmpty(stashRootId))
        {
            return Refuse("원본 프로필의 스태시를 찾지 못했습니다.");
        }

        if (selectedIds is null || selectedIds.Count == 0)
        {
            return Refuse("내보낼 것을 하나도 고르지 않았습니다.");
        }

        var byId = new Dictionary<string, PorterItem>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            // 같은 id 가 두 번 오면 어느 쪽이 진짜인지 알 수 없다. 조용히 덮지 않는다.
            if (byId.ContainsKey(item.Id))
            {
                return Refuse($"원본에 같은 식별자가 두 번 있습니다({item.Id}).");
            }

            byId[item.Id] = item;
        }

        var byParent = BuildChildIndex(items);
        var blocked = blockedTemplateIds is null
            ? new HashSet<string>(StringComparer.Ordinal)
            : new HashSet<string>(blockedTemplateIds, StringComparer.Ordinal);

        var skipped = new List<string>();
        var roots = new List<string>();

        foreach (var id in selectedIds.Distinct(StringComparer.Ordinal))
        {
            if (!byId.TryGetValue(id, out var item))
            {
                return Refuse($"고른 아이템이 원본에 없습니다({id}).");
            }

            // 스태시 바닥에 놓인 것만 고를 수 있다. 입고 있는 것·은신처의 것은 이 길이 아니다.
            if (!string.Equals(item.ParentId, stashRootId, StringComparison.Ordinal)
                || !string.Equals(item.SlotId, StashSlot, StringComparison.Ordinal))
            {
                skipped.Add($"{id} (스태시 바닥에 놓인 것이 아닙니다)");
                continue;
            }

            if (blocked.Contains(item.TemplateId))
            {
                skipped.Add($"{id} (내보낼 수 없는 종류입니다)");
                continue;
            }

            roots.Add(id);
        }

        if (roots.Count == 0)
        {
            return new ExportPlan(false, "고른 것 가운데 내보낼 수 있는 것이 없습니다.", [], skipped);
        }

        var collected = Collect(roots, byParent, out var cycle);
        if (cycle is not null)
        {
            return Refuse($"아이템이 자기 자신을 품고 있습니다({cycle}).");
        }

        // 자손 가운데 내보낼 수 없는 종류가 있으면 **그 뿌리째** 거절한다. 안에서만 빼면
        // 가방은 가는데 안의 것이 사라져, 사람이 "잃어버렸다" 고 읽는다.
        foreach (var id in collected)
        {
            if (byId.TryGetValue(id, out var item) && blocked.Contains(item.TemplateId))
            {
                return Refuse($"고른 것 안에 내보낼 수 없는 종류가 들어 있습니다({id}).");
            }
        }

        if (collected.Count > MaxItems)
        {
            return Refuse($"한 번에 내보낼 수 있는 것은 {MaxItems}개까지입니다"
                          + $" — 고른 것 안에 든 것까지 {collected.Count}개입니다. 나눠서 하세요.");
        }

        return new ExportPlan(true, string.Empty, collected, skipped);
    }

    /// <summary>
    /// 입고 있는 한 벌을 고른다. **보안 컨테이너와 그 안은 통째로 뺀다**(설계 §4.5).
    /// </summary>
    /// <param name="equipmentRootId">장비 루트 아이템의 id.</param>
    public static ExportPlan PlanLoadout(
        IReadOnlyCollection<PorterItem> items,
        string? equipmentRootId,
        IReadOnlyCollection<string>? blockedTemplateIds = null)
    {
        if (items is null || items.Count == 0)
        {
            return Refuse("원본 프로필에서 아이템을 읽지 못했습니다.");
        }

        if (string.IsNullOrEmpty(equipmentRootId))
        {
            return Refuse("원본 프로필의 장비를 찾지 못했습니다.");
        }

        var byParent = BuildChildIndex(items);
        var blocked = blockedTemplateIds is null
            ? new HashSet<string>(StringComparer.Ordinal)
            : new HashSet<string>(blockedTemplateIds, StringComparer.Ordinal);

        var skipped = new List<string>();
        var roots = new List<string>();
        var securedRoots = new List<string>();

        foreach (var item in items)
        {
            if (!string.Equals(item.ParentId, equipmentRootId, StringComparison.Ordinal))
            {
                continue;
            }

            if (string.Equals(item.SlotId, SecuredContainerSlot, StringComparison.Ordinal))
            {
                securedRoots.Add(item.Id);
                continue;
            }

            if (blocked.Contains(item.TemplateId))
            {
                skipped.Add($"{item.SlotId} (내보낼 수 없는 종류입니다)");
                continue;
            }

            roots.Add(item.Id);
        }

        if (securedRoots.Count > 0)
        {
            var inside = Collect(securedRoots, byParent, out _);
            skipped.Add($"보안 컨테이너는 뺐습니다(안에 {inside.Count - securedRoots.Count}개)");
        }

        if (roots.Count == 0)
        {
            return new ExportPlan(false, "입고 있는 것 가운데 내보낼 수 있는 것이 없습니다.", [], skipped);
        }

        var collected = Collect(roots, byParent, out var cycle);
        if (cycle is not null)
        {
            return Refuse($"아이템이 자기 자신을 품고 있습니다({cycle}).");
        }

        if (collected.Count > MaxItems)
        {
            return Refuse($"한 번에 내보낼 수 있는 것은 {MaxItems}개까지입니다 — 한 벌이 {collected.Count}개입니다.");
        }

        return new ExportPlan(true, string.Empty, collected, skipped);
    }

    private static ExportPlan Refuse(string reason) => new(false, reason, [], []);

    private static Dictionary<string, List<string>> BuildChildIndex(IReadOnlyCollection<PorterItem> items)
    {
        var byParent = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item.ParentId))
            {
                continue;
            }

            if (!byParent.TryGetValue(item.ParentId!, out var list))
            {
                list = [];
                byParent[item.ParentId!] = list;
            }

            list.Add(item.Id);
        }

        return byParent;
    }

    /// <summary>
    /// 뿌리들과 그 자손을 전부 모은다. 같은 것을 두 번 담지 않고, 고리를 만나면
    /// <paramref name="cycle"/> 에 그 id 를 담아 알린다 — 조용히 멈추지 않는다.
    /// </summary>
    private static List<string> Collect(
        IReadOnlyCollection<string> rootIds,
        Dictionary<string, List<string>> byParent,
        out string? cycle)
    {
        cycle = null;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var ordered = new List<string>();
        var queue = new Queue<string>();

        foreach (var id in rootIds)
        {
            if (seen.Add(id))
            {
                queue.Enqueue(id);
            }
        }

        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            ordered.Add(id);

            if (!byParent.TryGetValue(id, out var children))
            {
                continue;
            }

            foreach (var child in children)
            {
                if (string.Equals(child, id, StringComparison.Ordinal))
                {
                    cycle ??= child;
                    continue;
                }

                if (seen.Add(child))
                {
                    queue.Enqueue(child);
                }
                else if (rootIds.Contains(child))
                {
                    // 뿌리가 다른 뿌리의 자손이면 겹쳐 담긴 것이다. 담지는 않지만 고리는 아니다.
                    continue;
                }
            }
        }

        return ordered;
    }
}
