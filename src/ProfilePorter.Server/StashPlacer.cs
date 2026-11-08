using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;

namespace ProfilePorter.Server;

/// <summary>
/// 들여온 것을 받는 쪽 스태시에 놓는다.
///
/// **좌표를 지어내지 않는다.** 빈 자리를 찾아 `ParentId`·`SlotId`·`Location` 을 쓰는 것은 SPT
/// 자신의 <c>InventoryHelper.PlaceItemInContainer</c> 다 — STM 의 장비 편집·꾸러미 구매가 쓰는
/// 것과 같은 자리이고, 판정이 두 곳에 생기면 한쪽만 고쳐져 아이템이 겹쳐 놓인다.
///
/// **크기와 칸 수는 서버 DB 에서 읽는다**(설계 §9.3). `items.json` 으로 재면 틀린다 — 그 파일은
/// 스태시를 키우는 모드가 지나가기 전의 값이라, 같은 스태시가 파일에서는 680칸이고 서버에서는
/// 800칸이었다.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class StashPlacer(
    InventoryHelper inventoryHelper,
    ItemHelper itemHelper,
    ISptLogger<StashPlacer> logger)
{
    /// <summary>
    /// 뿌리들을 스태시 바닥에 하나씩 놓는다. **전부 들어가거나 하나도 안 들어간다**(설계 §4.3) —
    /// 호출자는 복제본 위에서 이것을 먼저 돌려 보고, 참일 때만 실물에 쓴다.
    /// </summary>
    /// <param name="items">받는 쪽 프로필의 아이템 목록. 들여올 것이 **이미 더해져 있어야** 한다.</param>
    /// <param name="stashRootId">받는 쪽 스태시 루트 id.</param>
    /// <param name="rootIds">자리를 잡아 줄 뿌리들의 id.</param>
    public bool TryPlaceAll(
        List<Item> items,
        string stashRootId,
        IReadOnlyList<string> rootIds,
        out string? failure)
    {
        failure = null;

        var stash = items.FirstOrDefault(i => string.Equals(i.Id.ToString(), stashRootId, StringComparison.Ordinal));
        if (stash is null)
        {
            failure = "받는 쪽 스태시를 찾지 못했습니다.";
            return false;
        }

        var found = itemHelper.GetItem(stash.Template);
        var grid = (found.Key ? found.Value?.Properties?.Grids : null)
            ?.FirstOrDefault(g => g?.Properties?.CellsH is > 0 && g.Properties.CellsV is > 0);
        if (grid?.Properties is null)
        {
            failure = "받는 쪽 스태시에 격자가 없습니다.";
            return false;
        }

        var gridName = string.IsNullOrWhiteSpace(grid.Name) ? ExportSelection.StashSlot : grid.Name!;

        foreach (var rootId in rootIds)
        {
            var root = items.FirstOrDefault(i => string.Equals(i.Id.ToString(), rootId, StringComparison.Ordinal));
            if (root is null)
            {
                failure = $"놓을 것을 목록에서 찾지 못했습니다({rootId}).";
                return false;
            }

            // 지도는 **놓을 때마다 다시** 만든다. 한 번 만들어 돌려 쓰면 앞서 놓은 것이 지도에
            // 반영되지 않아 둘이 같은 칸에 겹친다.
            var gridItems = items.Where(i =>
                string.Equals(i.ParentId, stashRootId, StringComparison.Ordinal)
                && string.Equals(i.SlotId, gridName, StringComparison.OrdinalIgnoreCase));

            int[,] map;
            try
            {
                map = inventoryHelper.GetContainerMap(
                    grid.Properties.CellsH!.Value, grid.Properties.CellsV!.Value, gridItems, stash.Id);
            }
            catch (Exception e)
            {
                failure = $"받는 쪽 스태시의 격자를 읽지 못했습니다: {e.Message}";
                return false;
            }

            // `PlaceItemInContainer` 는 목록의 **첫 번째**를 크기 계산의 주체로 쓴다 — 뿌리가 먼저여야 한다.
            var tree = Subtree(items, rootId);
            tree.Remove(root);
            tree.Insert(0, root);

            var placement = inventoryHelper.PlaceItemInContainer(map, tree, stash.Id.ToString(), gridName);
            if (placement?.Success != true)
            {
                failure = "받는 쪽 스태시에 빈 자리가 없습니다 — 하나도 들여오지 않았습니다.";
                return false;
            }

            logger.Debug($"[ProfilePorter] {rootId} 를 스태시에 놓았다(안에 든 것 {tree.Count - 1}개).");
        }

        return true;
    }

    /// <summary>그 아이템과 자손 전부. 고리를 만나면 멈춘다 — 무한 루프로 가지 않는다.</summary>
    private static List<Item> Subtree(List<Item> items, string rootId)
    {
        var byParent = new Dictionary<string, List<Item>>(StringComparer.Ordinal);
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

            list.Add(item);
        }

        var seen = new HashSet<string>(StringComparer.Ordinal) { rootId };
        var tree = new List<Item>();
        var queue = new Queue<string>();
        queue.Enqueue(rootId);

        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            var self = items.FirstOrDefault(i => string.Equals(i.Id.ToString(), id, StringComparison.Ordinal));
            if (self is not null)
            {
                tree.Add(self);
            }

            if (!byParent.TryGetValue(id, out var children))
            {
                continue;
            }

            foreach (var child in children)
            {
                var childId = child.Id.ToString();
                if (seen.Add(childId))
                {
                    queue.Enqueue(childId);
                }
            }
        }

        return tree;
    }
}
