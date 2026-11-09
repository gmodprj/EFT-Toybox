using System.Collections.Generic;
using System.Linq;

namespace ProfilePorter.Server;

/// <summary>상자 안의 격자 하나. 이름과 칸 수는 **서버 DB** 가 준다(설계 §4.2).</summary>
public sealed record GridSpec(string Name, int CellsH, int CellsV);

/// <summary>슬롯에 꽂힌 것. 좌표가 아니라 **슬롯 이름**으로 붙어 있다.</summary>
public sealed record DetailSlot(string SlotId, string Id, string Tpl, int Stack, int Contained);

/// <summary>격자 하나와 그 안에 놓인 것들. 비어 있어도 격자는 그린다.</summary>
public sealed record DetailGrid(string Name, int CellsH, int CellsV, IReadOnlyList<PorterItem> Rows);

/// <summary>자리를 못 잡은 것. **버리지 않는다** — 되짚어 보는 창에서 사라지는 것이 가장 나쁘다.</summary>
public sealed record DetailLoose(string Id, string Tpl, int Stack, int Contained, string Why);

public sealed record DetailPlace(
    IReadOnlyList<DetailGrid> Grids,
    IReadOnlyList<DetailSlot> Slots,
    IReadOnlyList<DetailLoose> Loose);

/// <summary>
/// 내보낸 파일 하나를 **세부 보기 화면의 자리**로 나눈다.
///
/// SPT 타입을 들이지 않는다 — <see cref="ExportSelection"/>·<see cref="ImportPlanner"/> 와 같이
/// 게임 없이 시험할 수 있는 쪽이다. 이름·크기·격자 칸 수처럼 **DB 가 아는 것**은 서버가 붙인다.
/// </summary>
public static class DetailPlanner
{
    internal const string NoSpotReason = "자리 정보가 없습니다";
    internal const string UnknownSlotReason = "이 상자에 없는 자리입니다";

    /// <summary>
    /// 자식들을 격자·슬롯·<see cref="DetailLoose"/> 로 나눈다. 설계 §4.2 의 순서를 그대로 따른다.
    ///
    /// **②(슬롯)가 ③(자리 없음)보다 먼저다.** 슬롯에 꽂힌 것은 좌표가 **원래** 없어서,
    /// 순서를 뒤집으면 플레이트·부착물이 통째로 <see cref="DetailLoose"/> 로 샌다.
    /// </summary>
    public static DetailPlace Place(
        IReadOnlyList<PorterItem> children,
        IReadOnlyList<GridSpec> grids,
        IReadOnlySet<string> slotNames)
    {
        // 같은 이름의 격자가 둘일 수 있다(모드 아이템이 `_name` 을 겹쳐 쓴다). **던지지 않는다** —
        // 던지면 그 상자는 영영 못 연다. 첫 격자에 담고 나머지는 빈 격자로 그린다.
        var rows = new Dictionary<string, List<PorterItem>>(StringComparer.Ordinal);
        var lists = new List<List<PorterItem>>(grids.Count);

        foreach (var grid in grids)
        {
            var list = new List<PorterItem>();
            lists.Add(list);

            if (!rows.ContainsKey(grid.Name))
            {
                rows[grid.Name] = list;
            }
        }
        var slots = new List<DetailSlot>();
        var loose = new List<DetailLoose>();

        foreach (var child in children ?? [])
        {
            var slotId = child.SlotId ?? string.Empty;

            // ① 격자 이름과 같은가 — 같으면 좌표가 있어야 그릴 수 있다.
            if (rows.TryGetValue(slotId, out var into))
            {
                if (child.Location is null)
                {
                    loose.Add(Loose(child, NoSpotReason));
                }
                else
                {
                    into.Add(child);
                }

                continue;
            }

            // ② 슬롯인가 — 좌표를 묻지 않는다.
            if (slotNames.Contains(slotId))
            {
                slots.Add(new DetailSlot(slotId, child.Id, child.TemplateId, StackOf(child), 0));
                continue;
            }

            // ③ 어느 쪽도 아니다. 버리지 않고 모아 둔다.
            loose.Add(Loose(child, UnknownSlotReason));
        }

        return new DetailPlace(
            grids.Select((g, i) => new DetailGrid(g.Name, g.CellsH, g.CellsV, lists[i])).ToList(),
            slots,
            loose);
    }

    /// <summary>
    /// **뿌리에서 닿지 않는 가지의 머리들.** 뿌리 목록에도 없고 부모가 파일 안에도 없는 항목이다.
    ///
    /// 창은 그런 id 를 알 길이 없어 지목할 수도 없으므로, 서버가 먼저 찾아내 보여 주지 않으면
    /// 사람은 **그 물건이 애초에 없었다고** 읽는다. 설계 §4.2 의 "자손은 어디로도 버려지지
    /// 않는다" 는 뿌리에도 적용된다.
    /// </summary>
    public static IReadOnlyList<PorterItem> Unreachable(PorterBundle bundle)
    {
        var items = bundle?.Items ?? [];
        var ids = items.Select(i => i.Id).ToHashSet(StringComparer.Ordinal);
        var roots = (bundle?.RootIds ?? []).ToHashSet(StringComparer.Ordinal);

        // 부모가 파일 안에 있으면 그 상자를 열어 볼 수 있다 — 끊긴 것은 **가지의 머리**뿐이다.
        return items
            .Where(i => !roots.Contains(i.Id))
            .Where(i => i.ParentId is null || !ids.Contains(i.ParentId))
            .ToList();
    }

    /// <summary>
    /// 그 자리의 **직계 자식만**. 뿌리 목록에 매달리지 않은 상자라도 id 로 고르면 안이 보인다.
    /// </summary>
    public static IReadOnlyList<PorterItem> ChildrenOf(PorterBundle bundle, string? parentId) =>
        (bundle?.Items ?? [])
        .Where(i => string.Equals(i.ParentId, parentId, StringComparison.Ordinal))
        .ToList();

    /// <summary>
    /// 스태시 격자의 칸 수 — **현재 크기와 좌표 테두리 중 큰 쪽**(설계 §7-5).
    ///
    /// 칸 수는 돌고 있는 DB 가 정하고 그 사이 바뀐다(은신처 등급·스태시 확장 모드). 현재 크기만
    /// 믿으면 그보다 큰 좌표를 가진 물건이 격자 **밖에** 그려져 사람이 못 본다.
    /// 보낸 프로필이 없으면 0 을 넘긴다 — 그러면 테두리만 쓴다.
    /// </summary>
    public static (int cellsH, int cellsV) StashCells(
        int profileCellsH, int profileCellsV, IEnumerable<(int x, int y, int w, int h)> rows)
    {
        var h = profileCellsH;
        var v = profileCellsV;

        foreach (var (x, y, w, hh) in rows ?? [])
        {
            h = Math.Max(h, x + w);
            v = Math.Max(v, y + hh);
        }

        return (h, v);
    }

    /// <summary>
    /// 자리 하나를 그릴 때 **고를 수 있는 크기 둘** — 조립 크기(부착물까지)와 기본 크기.
    /// </summary>
    public sealed record SizeChoice(
        string Id, int X, int Y, int AssembledW, int AssembledH, int BaseW, int BaseH);

    /// <summary>고른 크기. <see cref="Shrunk"/> 면 겹침을 피하려고 물러난 것이다.</summary>
    public sealed record FittedSize(int W, int H, bool Shrunk);

    /// <summary>
    /// 겹치지 않게 크기를 고른다.
    ///
    /// **왜 필요한가** — 조립 크기는 지금 DB 가 말하는 값이고, 좌표는 **그때 놓았던 자리**다.
    /// 모드가 아이템이나 부착물을 바꾸면 둘이 어긋나, 큰 쪽으로 그리면 옆 것을 덮어 버린다
    /// (2026-11-09 사용자 제보: "아이템이 겹쳐보임"). 덮는 대신 **물러난다** —
    /// 조립 크기 → 기본 크기 → 빈 자리만큼. 그래서 **아무것도 숨지 않는다.**
    ///
    /// 위에서 아래로, 왼쪽에서 오른쪽으로 놓는다(사람이 읽는 차례). 먼저 놓인 것이 자리를 지킨다.
    /// </summary>
    public static Dictionary<string, FittedSize> FitSizes(IReadOnlyList<SizeChoice> choices)
    {
        var taken = new HashSet<(int, int)>();
        var result = new Dictionary<string, FittedSize>(StringComparer.Ordinal);

        bool Free(int x, int y, int w, int h)
        {
            for (var dx = 0; dx < w; dx++)
            {
                for (var dy = 0; dy < h; dy++)
                {
                    if (taken.Contains((x + dx, y + dy))) return false;
                }
            }

            return true;
        }

        var list = (choices ?? []).OrderBy(c => c.Y).ThenBy(c => c.X).ToList();

        // **남의 자리 머리를 덮지 않는다.** 다른 물건이 시작하는 칸까지 뻗으면, 큰 쪽이 작은
        // 쪽을 가려 버린다. 그 칸 앞에서 멈춘다 — 먼저 놓인 것이든 나중 것이든 똑같이.
        var origins = list.Select(c => (c.X, c.Y)).ToHashSet();

        foreach (var c in list)
        {
            var w = Math.Max(1, c.AssembledW);
            var h = Math.Max(1, c.AssembledH);

            for (var dx = 1; dx < w; dx++)
            {
                if (origins.Contains((c.X + dx, c.Y)) || !Free(c.X + dx, c.Y, 1, 1))
                {
                    w = dx;
                    break;
                }
            }

            for (var dy = 1; dy < h; dy++)
            {
                var blocked = false;
                for (var dx = 0; dx < w; dx++)
                {
                    if (origins.Contains((c.X + dx, c.Y + dy)) || !Free(c.X + dx, c.Y + dy, 1, 1))
                    {
                        blocked = true;
                        break;
                    }
                }

                if (blocked)
                {
                    h = dy;
                    break;
                }
            }

            for (var dx = 0; dx < w; dx++)
            {
                for (var dy = 0; dy < h; dy++)
                {
                    taken.Add((c.X + dx, c.Y + dy));
                }
            }

            result[c.Id] = new FittedSize(w, h, w != c.AssembledW || h != c.AssembledH);
        }

        return result;
    }

    /// <summary>
    /// 두 번 이상 차지된 칸의 수.
    ///
    /// **격자가 겹치는 것은 우리가 잘못 그려서가 아니다.** 모드가 아이템 크기를 바꾸면,
    /// 그때 그 자리에 놓았던 것과 **지금 DB 가 말하는 크기**가 어긋난다(2026-11-09 실측:
    /// 1,361개 중 두 칸. 원본 프로필의 좌표만으로는 겹침이 0건이었다). 숨기지 않고 세어서
    /// 사람에게 "왜 겹쳐 보이는지" 를 말해 준다.
    /// </summary>
    public static int CountOverlaps(IEnumerable<(int x, int y, int w, int h)> rows)
    {
        var taken = new HashSet<(int, int)>();
        var n = 0;

        foreach (var (x, y, w, h) in rows ?? [])
        {
            for (var dx = 0; dx < w; dx++)
            {
                for (var dy = 0; dy < h; dy++)
                {
                    if (!taken.Add((x + dx, y + dy)))
                    {
                        n++;
                    }
                }
            }
        }

        return n;
    }

    private static DetailLoose Loose(PorterItem item, string why) =>
        new(item.Id, item.TemplateId, StackOf(item), 0, why);

    /// <summary>묶음 수는 `upd` 안에 있다. 여기서는 **들여다보지 않는다** — 서버가 붙인다.</summary>
    private static int StackOf(PorterItem item) => 1;
}
