using ProfilePorter.Server;
using Xunit;

namespace ProfilePorter.Tests;

/// <summary>
/// 들여오기 판정. 설계 §4.2(id 를 전부 새로 매긴다)와 안티목표 3·4 가 여기 걸린다.
/// </summary>
public class ImportPlannerTests
{
    /// <summary>시험에서는 세는 수를 쓴다 — 어떤 id 가 나올지 미리 알 수 있어야 한다.</summary>
    private static Func<string> Counter()
    {
        var n = 0;
        return () => $"new{++n}";
    }

    private static PorterBundle Bundle(params PorterItem[] items) =>
        new(PorterBundle.KindStash, [items[0].Id], items);

    [Fact]
    public void 모든_id_가_새것으로_바뀌고_부모도_함께_옮겨진다()
    {
        var bundle = Bundle(
            new PorterItem("bag", "old-stash", "hideout", "tpl-bag"),
            new PorterItem("a", "bag", "main", "tpl-thing"),
            new PorterItem("deep", "a", "main", "tpl-thing"));

        var plan = ImportPlanner.Plan(bundle, Counter());

        Assert.True(plan.Accepted, plan.Reason);
        Assert.Equal(3, plan.Items.Count);

        // 옛 id 는 하나도 남지 않는다 — 이것이 복제로 읽히지 않게 하는 전부다.
        Assert.DoesNotContain(plan.Items, i => i.Id is "bag" or "a" or "deep");
        Assert.DoesNotContain(plan.Items, i => i.ParentId is "bag" or "a");

        // 부모-자식 관계는 그대로다.
        var map = plan.IdMap;
        var deep = plan.Items.Single(i => i.Id == map["deep"]);
        Assert.Equal(map["a"], deep.ParentId);
        Assert.Equal("main", deep.SlotId);
    }

    [Fact]
    public void 뿌리의_자리는_비워_둔다()
    {
        // 어디에 놓을지는 자리 찾기(P3)가 정한다. 원본의 스태시를 가리킨 채 두면 안 된다.
        var bundle = Bundle(new PorterItem("bag", "old-stash", "hideout", "tpl-bag"));

        var plan = ImportPlanner.Plan(bundle, Counter());

        var root = Assert.Single(plan.Items);
        Assert.Null(root.ParentId);
        Assert.Null(root.SlotId);
        Assert.Equal([root.Id], plan.RootIds);
    }

    [Fact]
    public void upd_는_손대지_않고_그대로_간다()
    {
        // 설계 §4.1 — "가진 것 그대로" 의 전부가 여기 있다.
        var upd = new { Repairable = new { Durability = 42 } };
        var bundle = Bundle(new PorterItem("gun", "old-stash", "hideout", "tpl-gun", upd));

        var plan = ImportPlanner.Plan(bundle, Counter());

        Assert.Same(upd, Assert.Single(plan.Items).Upd);
    }

    [Fact]
    public void 부모가_꾸러미_밖을_가리키면_거절한다()
    {
        // 조용히 떼어 내면 부모 없는 아이템이 프로필에 남는다(안티목표 4).
        var bundle = new PorterBundle(
            PorterBundle.KindStash,
            ["bag"],
            [
                new PorterItem("bag", "old-stash", "hideout", "tpl-bag"),
                new PorterItem("lost", "somewhere-else", "main", "tpl-thing"),
            ]);

        var plan = ImportPlanner.Plan(bundle, Counter());

        Assert.False(plan.Accepted);
        Assert.Empty(plan.Items);

        // **어느 검사가 잡았는지까지 본다.** 뒤의 "뿌리에서 닿지 않는다" 검사도 같은 꾸러미를
        // 거절하므로, 사유를 보지 않으면 앞의 검사를 통째로 들어내도 시험이 빨개지지 않는다
        // (2026-11-08 에 일부러 망가뜨려 보고 알았다).
        Assert.Contains("부모가 꾸러미 밖", plan.Reason);
        Assert.Contains("lost", plan.Reason);
    }

    [Fact]
    public void 뿌리에서_닿지_않는_것이_있으면_거절한다()
    {
        var bundle = new PorterBundle(
            PorterBundle.KindStash,
            ["bag"],
            [
                new PorterItem("bag", "old-stash", "hideout", "tpl-bag"),
                new PorterItem("island", "lonely", "main", "tpl-thing"),
                new PorterItem("lonely", "island", "main", "tpl-thing"), // 자기들끼리만 이어진 섬
            ]);

        var plan = ImportPlanner.Plan(bundle, Counter());

        Assert.False(plan.Accepted);
        Assert.Empty(plan.Items);

        // 이쪽은 부모가 **꾸러미 안**에 있으므로 앞의 검사에 걸리지 않는다. 닿지 않는 섬을
        // 잡는 것은 이 검사뿐이다 — 그래서 사유로 둘을 가른다.
        Assert.Contains("닿지 않는", plan.Reason);
    }

    [Fact]
    public void 꾸러미에_같은_식별자가_두_번이면_거절한다()
    {
        var bundle = new PorterBundle(
            PorterBundle.KindStash,
            ["bag"],
            [
                new PorterItem("bag", "old-stash", "hideout", "tpl-bag"),
                new PorterItem("bag", "old-stash", "hideout", "tpl-bag"),
            ]);

        Assert.False(ImportPlanner.Plan(bundle, Counter()).Accepted);
    }

    [Fact]
    public void 뿌리로_적힌_것이_꾸러미에_없으면_거절한다()
    {
        var bundle = new PorterBundle(
            PorterBundle.KindStash,
            ["missing"],
            [new PorterItem("bag", "old-stash", "hideout", "tpl-bag")]);

        var plan = ImportPlanner.Plan(bundle, Counter());

        Assert.False(plan.Accepted);
        Assert.Contains("missing", plan.Reason);
    }

    [Fact]
    public void 새_식별자가_겹치면_거절한다()
    {
        // 생성기가 망가진 경우. 겹친 채로 쓰면 프로필 안에서 둘이 같은 것이 된다.
        var bundle = Bundle(
            new PorterItem("bag", "old-stash", "hideout", "tpl-bag"),
            new PorterItem("a", "bag", "main", "tpl-thing"));

        var plan = ImportPlanner.Plan(bundle, () => "always-the-same");

        Assert.False(plan.Accepted);
        Assert.Contains("겹쳤", plan.Reason);
    }

    [Fact]
    public void 모르는_종류나_빈_꾸러미는_거절한다()
    {
        Assert.False(ImportPlanner.Plan(null, Counter()).Accepted);
        Assert.False(ImportPlanner.Plan(new PorterBundle(PorterBundle.KindStash, [], []), Counter()).Accepted);
        Assert.False(ImportPlanner.Plan(
            new PorterBundle("무언가", ["bag"], [new PorterItem("bag", null, null, "tpl")]),
            Counter()).Accepted);
    }

    [Fact]
    public void 종류가_없는_아이템은_거절한다()
    {
        var bundle = Bundle(new PorterItem("bag", "old-stash", "hideout", string.Empty));

        var plan = ImportPlanner.Plan(bundle, Counter());

        Assert.False(plan.Accepted);
        Assert.Contains("종류", plan.Reason);
    }
}
