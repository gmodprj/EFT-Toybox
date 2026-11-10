using ProfilePorter.Server;
using Xunit;

namespace ProfilePorter.Tests;

/// <summary>
/// 내보내고 들여오는 한 바퀴. 두 판정이 **같은 것을 뜻하는지**를 본다 — 따로 보면
/// 각자 맞는데 이어 붙이면 어긋나는 자리가 여기서 드러난다.
/// </summary>
public class RoundTripTests
{
    private const string Stash = "stash-root";

    private static Func<string> Counter()
    {
        var n = 0;
        return () => $"new{++n}";
    }

    /// <summary>내보내기 계획을 꾸러미로 바꾼다 — 서버(P3)가 할 일을 시험에서 흉내 낸다.</summary>
    private static PorterBundle ToBundle(
        IReadOnlyList<PorterItem> source, ExportPlan plan, IReadOnlyList<string> rootIds)
    {
        var picked = plan.ItemIds.ToHashSet(StringComparer.Ordinal);
        return new PorterBundle(
            PorterBundle.KindStash,
            rootIds,
            source.Where(i => picked.Contains(i.Id)).ToList());
    }

    [Fact]
    public void 내보낸_것을_들여오면_개수와_모양이_그대로다()
    {
        List<PorterItem> source =
        [
            new("bag", Stash, ExportSelection.StashSlot, "tpl-bag"),
            new("gun", "bag", "main", "tpl-gun", new { Repairable = new { Durability = 77 } }),
            new("mag", "gun", "mod_magazine", "tpl-mag"),
            new("ammo", "mag", "cartridges", "tpl-ammo", new { StackObjectsCount = 30 }),
            new("elsewhere", Stash, ExportSelection.StashSlot, "tpl-other"),
        ];

        var export = ExportSelection.PlanStash(source, Stash, ["bag"]);
        Assert.True(export.Accepted, export.Reason);
        Assert.Equal(4, export.ItemIds.Count);

        var plan = ImportPlanner.Plan(ToBundle(source, export, ["bag"]), Counter());
        Assert.True(plan.Accepted, plan.Reason);

        // 개수가 같다.
        Assert.Equal(export.ItemIds.Count, plan.Items.Count);

        // 트리 모양이 같다 — 옛 이름으로 되돌려 비교한다.
        var back = plan.IdMap.ToDictionary(kv => kv.Value, kv => kv.Key, StringComparer.Ordinal);
        foreach (var item in plan.Items)
        {
            var oldId = back[item.Id];
            var original = source.Single(i => i.Id == oldId);

            Assert.Equal(original.TemplateId, item.TemplateId);
            Assert.Same(original.Upd, item.Upd);

            if (oldId == "bag")
            {
                Assert.Null(item.ParentId); // 뿌리는 자리를 비운다
            }
            else
            {
                Assert.Equal(original.ParentId, back[item.ParentId!]);
                Assert.Equal(original.SlotId, item.SlotId);
            }
        }
    }

    /// <summary>
    /// 설계 §20 의 결함을 가두는 시험. 2026-11-09 까지 <c>PorterItem</c> 에 <c>Location</c> 이
    /// 아예 없어서, 내보낸 파일 넷에서 좌표가 **100% 떨어졌다**(1,130개 → 0개). 개수·<c>upd</c>·
    /// <c>slotId</c> 만 세던 관문들은 전부 녹색이었다.
    ///
    /// **뿌리는 비우고 자손은 지킨다** — 보내는 쪽 스태시 좌표는 받는 쪽에서 남의 자리이고,
    /// 상자 안의 좌표는 상자를 기준으로 한 것이라 뜻이 그대로 통한다.
    /// </summary>
    [Fact]
    public void 컨테이너_안의_자리와_회전은_한_바퀴를_돌아도_그대로다()
    {
        var bagSpot = new { x = 3, y = 4, r = "Horizontal" };
        var gunSpot = new { x = 1, y = 0, r = "Vertical" };
        var ammoSlot = 2;

        List<PorterItem> source =
        [
            new("bag", Stash, ExportSelection.StashSlot, "tpl-bag", null, bagSpot),
            new("gun", "bag", "main", "tpl-gun", null, gunSpot),
            new("mag", "gun", "mod_magazine", "tpl-mag", null, ammoSlot),
        ];

        var export = ExportSelection.PlanStash(source, Stash, ["bag"]);
        Assert.True(export.Accepted, export.Reason);

        var plan = ImportPlanner.Plan(ToBundle(source, export, ["bag"]), Counter());
        Assert.True(plan.Accepted, plan.Reason);

        var back = plan.IdMap.ToDictionary(kv => kv.Value, kv => kv.Key, StringComparer.Ordinal);
        var byOldId = plan.Items.ToDictionary(i => back[i.Id], i => i, StringComparer.Ordinal);

        // 뿌리는 자리를 비운다 — 자리 찾기가 채운다.
        Assert.Null(byOldId["bag"].Location);

        // 자손은 **같은 객체 그대로**다. 해석하거나 다시 만들지 않는다.
        Assert.Same(gunSpot, byOldId["gun"].Location);

        // 슬롯에 꽂힌 것은 정수 색인이다(값 타입이라 같은 객체일 수는 없고, 값이 같아야 한다).
        Assert.Equal(ammoSlot, byOldId["mag"].Location);
    }

    [Fact]
    public void 두_번_들여오면_서로_다른_id_를_받는다()
    {
        // 같은 파일을 두 번 들여오는 것은 막지 않는다 — 이것은 복제 도구다(설계 §2).
        // 다만 두 벌이 **같은 id 를 갖지는 않아야** 한다.
        List<PorterItem> source =
        [
            new("bag", Stash, ExportSelection.StashSlot, "tpl-bag"),
            new("thing", "bag", "main", "tpl-thing"),
        ];
        var export = ExportSelection.PlanStash(source, Stash, ["bag"]);
        var bundle = ToBundle(source, export, ["bag"]);

        var first = ImportPlanner.Plan(bundle, Counter());
        var second = ImportPlanner.Plan(bundle, PrefixCounter("other"));

        Assert.True(first.Accepted);
        Assert.True(second.Accepted);
        Assert.Empty(first.Items.Select(i => i.Id).Intersect(second.Items.Select(i => i.Id), StringComparer.Ordinal));
    }

    [Fact]
    public void 내보낸_꾸러미는_원본_목록을_고치지_않는다()
    {
        // 안티목표 1 — 내보내는 쪽은 읽기 전용이다.
        List<PorterItem> source =
        [
            new("bag", Stash, ExportSelection.StashSlot, "tpl-bag"),
            new("thing", "bag", "main", "tpl-thing"),
        ];
        var before = source.Select(i => (i.Id, i.ParentId, i.SlotId, i.TemplateId)).ToList();

        var export = ExportSelection.PlanStash(source, Stash, ["bag"]);
        ImportPlanner.Plan(ToBundle(source, export, ["bag"]), Counter());

        var after = source.Select(i => (i.Id, i.ParentId, i.SlotId, i.TemplateId)).ToList();
        Assert.Equal(before, after);
    }

    private static Func<string> PrefixCounter(string prefix)
    {
        var n = 0;
        return () => $"{prefix}{++n}";
    }
}
