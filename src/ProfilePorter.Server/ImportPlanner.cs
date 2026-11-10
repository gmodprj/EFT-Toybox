using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfilePorter.Server;

/// <summary>
/// 들여오기 계획. <see cref="Accepted"/> 가 거짓이면 **아무것도 쓰지 않는다**.
///
/// <see cref="Items"/> 는 **새 id 로 갈아입은** 아이템들이고, 뿌리의 <c>ParentId</c>·<c>SlotId</c>
/// 는 비어 있다 — 어디에 놓을지는 자리 찾기(P3)가 정한다.
/// <see cref="IdMap"/> 은 옛 id → 새 id 다. 되돌려 볼 사람이 쓴다.
/// </summary>
public sealed record ImportPlan(
    bool Accepted,
    string Reason,
    IReadOnlyList<PorterItem> Items,
    IReadOnlyList<string> RootIds,
    IReadOnlyDictionary<string, string> IdMap);

/// <summary>
/// 들여올 꾸러미를 검사하고 **id 를 전부 새로 매긴다**. **순수** — 프로필도 DB 도 건드리지 않는다.
///
/// 왜 새로 매기나(설계 §2.2·§4.2): 이 도구는 **복제 도구**다. 원본을 남긴 채 베끼므로 같은 id 가
/// 두 프로필에 생기면, STM 의 전송이 그것을 "복제" 로 읽고 거절한다
/// (`StmStashTransferService.VerifyFromDisk` — 실측으로 확인). 새로 매기는 값이 문자열 사전
/// 하나뿐이라 안 할 이유가 없다.
///
/// **부모가 꾸러미 밖을 가리키면 거절한다.** 조용히 떼어 내면 부모 없는 아이템이 프로필에 남고,
/// 그 프로필로 로그인한 게임이 망가진다(안티목표 4).
/// </summary>
public static class ImportPlanner
{
    /// <param name="bundle">내보낸 파일의 알맹이.</param>
    /// <param name="newId">새 id 생성기. 서버는 `MongoId` 를, 시험은 세는 수를 준다.</param>
    public static ImportPlan Plan(PorterBundle? bundle, Func<string> newId)
    {
        ArgumentNullException.ThrowIfNull(newId);

        if (bundle is null || bundle.Items is null || bundle.Items.Count == 0)
        {
            return Refuse("들여올 것이 없습니다 — 파일이 비었거나 읽지 못했습니다.");
        }

        if (!string.Equals(bundle.Kind, PorterBundle.KindStash, StringComparison.Ordinal)
            && !string.Equals(bundle.Kind, PorterBundle.KindLoadout, StringComparison.Ordinal))
        {
            return Refuse($"모르는 꾸러미 종류입니다('{bundle.Kind}').");
        }

        if (bundle.RootIds is null || bundle.RootIds.Count == 0)
        {
            return Refuse("꾸러미에 뿌리가 적혀 있지 않습니다.");
        }

        if (bundle.Items.Count > ExportSelection.MaxItems)
        {
            return Refuse($"한 번에 들여올 수 있는 것은 {ExportSelection.MaxItems}개까지입니다"
                          + $" — 꾸러미에 {bundle.Items.Count}개가 들어 있습니다.");
        }

        var byId = new Dictionary<string, PorterItem>(StringComparer.Ordinal);
        foreach (var item in bundle.Items)
        {
            if (string.IsNullOrEmpty(item.Id))
            {
                return Refuse("식별자가 없는 아이템이 꾸러미에 있습니다.");
            }

            if (string.IsNullOrEmpty(item.TemplateId))
            {
                return Refuse($"아이템 종류가 적혀 있지 않습니다({item.Id}).");
            }

            if (!byId.TryAdd(item.Id, item))
            {
                return Refuse($"꾸러미에 같은 식별자가 두 번 있습니다({item.Id}).");
            }
        }

        var roots = new HashSet<string>(bundle.RootIds, StringComparer.Ordinal);
        foreach (var rootId in roots)
        {
            if (!byId.ContainsKey(rootId))
            {
                return Refuse($"뿌리로 적힌 것이 꾸러미에 없습니다({rootId}).");
            }
        }

        // 뿌리가 아닌 것의 부모는 **반드시 꾸러미 안**에 있어야 한다.
        // 뿌리의 부모는 원본 프로필의 스태시·장비를 가리키므로 꾸러미 밖인 것이 정상이다.
        foreach (var item in bundle.Items)
        {
            if (roots.Contains(item.Id))
            {
                continue;
            }

            if (string.IsNullOrEmpty(item.ParentId) || !byId.ContainsKey(item.ParentId!))
            {
                return Refuse($"부모가 꾸러미 밖을 가리킵니다({item.Id}). 꾸러미가 온전하지 않습니다.");
            }
        }

        // 뿌리에서 닿지 않는 것이 있으면 꾸러미가 깨진 것이다. 조용히 버리지 않는다.
        var reachable = Reachable(bundle.Items, roots);
        if (reachable.Count != bundle.Items.Count)
        {
            var orphan = bundle.Items.First(i => !reachable.Contains(i.Id));
            return Refuse($"뿌리에서 닿지 않는 아이템이 있습니다({orphan.Id}).");
        }

        // 여기까지 왔으면 쓸 수 있다. id 를 갈아입힌다.
        var idMap = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in bundle.Items)
        {
            var fresh = newId();
            if (string.IsNullOrEmpty(fresh))
            {
                return Refuse("새 식별자를 만들지 못했습니다.");
            }

            if (idMap.Values.Contains(fresh, StringComparer.Ordinal))
            {
                return Refuse($"새 식별자가 겹쳤습니다({fresh}).");
            }

            idMap[item.Id] = fresh;
        }

        var planned = new List<PorterItem>(bundle.Items.Count);
        foreach (var item in bundle.Items)
        {
            var isRoot = roots.Contains(item.Id);
            planned.Add(item with
            {
                Id = idMap[item.Id],
                // 뿌리는 어디에 놓일지 아직 모른다 — 자리 찾기가 채운다.
                ParentId = isRoot ? null : idMap[item.ParentId!],
                SlotId = isRoot ? null : item.SlotId,

                // 자리도 같은 이유로 **뿌리만** 비운다. 보내는 쪽 스태시의 좌표를 그대로 들고
                // 가면 받는 쪽의 남의 자리를 가리키게 된다. 컨테이너 **안**의 좌표는 상자를
                // 기준으로 한 것이라 그대로 들고 간다 — 그것을 잃은 것이 설계 §20 의 결함이다.
                Location = isRoot ? null : item.Location,
            });
        }

        var newRoots = bundle.RootIds.Select(id => idMap[id]).ToList();
        return new ImportPlan(true, string.Empty, planned, newRoots, idMap);
    }

    private static ImportPlan Refuse(string reason) =>
        new(false, reason, [], [], new Dictionary<string, string>(StringComparer.Ordinal));

    private static HashSet<string> Reachable(IReadOnlyList<PorterItem> items, HashSet<string> roots)
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

        var seen = new HashSet<string>(roots, StringComparer.Ordinal);
        var queue = new Queue<string>(roots);
        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            if (!byParent.TryGetValue(id, out var children))
            {
                continue;
            }

            foreach (var child in children)
            {
                if (seen.Add(child))
                {
                    queue.Enqueue(child);
                }
            }
        }

        return seen;
    }
}
