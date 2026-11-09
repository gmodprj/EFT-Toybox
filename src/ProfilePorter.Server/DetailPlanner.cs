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
        var rows = grids.ToDictionary(g => g.Name, _ => new List<PorterItem>(), StringComparer.Ordinal);
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
            grids.Select(g => new DetailGrid(g.Name, g.CellsH, g.CellsV, rows[g.Name])).ToList(),
            slots,
            loose);
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

    private static DetailLoose Loose(PorterItem item, string why) =>
        new(item.Id, item.TemplateId, StackOf(item), 0, why);

    /// <summary>묶음 수는 `upd` 안에 있다. 여기서는 **들여다보지 않는다** — 서버가 붙인다.</summary>
    private static int StackOf(PorterItem item) => 1;
}
