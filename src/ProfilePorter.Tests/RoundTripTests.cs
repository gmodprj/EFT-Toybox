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
