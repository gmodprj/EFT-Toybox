using ProfilePorter.Server;
using Xunit;

namespace ProfilePorter.Tests;

/// <summary>
/// 내보낼 것을 고르는 판정. 설계 §4.5(빼는 것)·§9.1(자손까지 세는 상한)이 여기 걸린다.
/// </summary>
public class ExportSelectionTests
{
    private const string Stash = "stash-root";
    private const string Equip = "equip-root";

    private static PorterItem Floor(string id, string tpl = "tpl-bag") =>
        new(id, Stash, ExportSelection.StashSlot, tpl);

    private static PorterItem Inside(string id, string parent, string tpl = "tpl-thing") =>
        new(id, parent, "main", tpl);

    [Fact]
    public void 가방을_고르면_안에_든_것이_따라간다()
    {
        List<PorterItem> items =
        [
            Floor("bag"),
            Inside("a", "bag"),
            Inside("b", "bag"),
            Inside("deep", "a"),
            Floor("other"),          // 안 고른 것
        ];

        var plan = ExportSelection.PlanStash(items, Stash, ["bag"]);

        Assert.True(plan.Accepted, plan.Reason);
        Assert.Equal(4, plan.ItemIds.Count);
        Assert.Contains("deep", plan.ItemIds);
        Assert.DoesNotContain("other", plan.ItemIds);
    }

    [Fact]
    public void 스태시_바닥에_없는_것은_고를_수_없다()
    {
        List<PorterItem> items =
        [
            Floor("bag"),
            new PorterItem("worn", Equip, "ArmorVest", "tpl-armor"),
        ];

        var plan = ExportSelection.PlanStash(items, Stash, ["worn"]);

        Assert.False(plan.Accepted);
        Assert.Contains(plan.Skipped, s => s.Contains("worn"));
    }

    [Fact]
    public void 막힌_종류가_안에_들어_있으면_뿌리째_거절한다()
    {
        // 안에서만 빼면 가방은 가는데 내용물이 사라져, 사람이 "잃어버렸다" 고 읽는다.
        List<PorterItem> items =
        [
            Floor("bag"),
            Inside("quest", "bag", "tpl-quest"),
        ];

        var plan = ExportSelection.PlanStash(items, Stash, ["bag"], ["tpl-quest"]);

        Assert.False(plan.Accepted);
        Assert.Contains("quest", plan.Reason);
        Assert.Empty(plan.ItemIds);
    }

    [Fact]
    public void 상한은_고른_개수가_아니라_자손까지_센_수다()
    {
        // 하나만 골랐지만 안에 MaxItems 를 넘게 들어 있다. 고른 개수로 재면 통과해 버린다.
        List<PorterItem> items = [Floor("bag")];
        for (var i = 0; i < ExportSelection.MaxItems + 1; i++)
        {
            items.Add(Inside($"i{i}", "bag"));
        }

        var plan = ExportSelection.PlanStash(items, Stash, ["bag"]);

        Assert.False(plan.Accepted);
        Assert.Contains("나눠서", plan.Reason);
    }

    [Fact]
    public void 한_벌은_보안_컨테이너를_통째로_뺀다()
    {
        List<PorterItem> items =
        [
            new PorterItem("armor", Equip, "ArmorVest", "tpl-armor"),
            new PorterItem("case", Equip, ExportSelection.SecuredContainerSlot, "tpl-case"),
            Inside("hidden1", "case"),
            Inside("hidden2", "case"),
        ];

        var plan = ExportSelection.PlanLoadout(items, Equip);

        Assert.True(plan.Accepted, plan.Reason);
        Assert.Equal(["armor"], plan.ItemIds);
        Assert.Contains(plan.Skipped, s => s.Contains("보안 컨테이너") && s.Contains("2개"));
    }

    [Fact]
    public void 원본에_같은_식별자가_두_번_있으면_거절한다()
    {
        List<PorterItem> items = [Floor("bag"), Floor("bag")];

        var plan = ExportSelection.PlanStash(items, Stash, ["bag"]);

        Assert.False(plan.Accepted);
        Assert.Contains("두 번", plan.Reason);
    }

    [Fact]
    public void 고를_것이_없거나_스태시를_못_찾으면_거절한다()
    {
        List<PorterItem> items = [Floor("bag")];

        Assert.False(ExportSelection.PlanStash(items, Stash, []).Accepted);
        Assert.False(ExportSelection.PlanStash(items, null, ["bag"]).Accepted);
        Assert.False(ExportSelection.PlanStash([], Stash, ["bag"]).Accepted);
    }
}
