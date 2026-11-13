using System;
using System.Collections.Generic;
using System.Linq;
using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// B1 — 플레이어 ↔ 팀원 스태시 전송의 판정(순수). 계획 6 §4.
//
// 가장 중요한 시험은 **총량 보존**이다: 옮기는 목록이 빼는 쪽과 넣는 쪽의 같은 단위여야 하고,
// 고른 것 안에 든 것이 빠지면 그 내용물이 원본에 남아 **복제**가 된다(설계 §4).
public class StashTransferPolicyTests
{
    private const string Root = "stash-root";
    private const string Src = "profile-a";
    private const string Dst = "profile-b";

    // 스태시 바닥에 가방 하나(안에 물건 둘), 그리고 낱개 하나. 입고 있는 것 하나는 바닥이 아니다.
    private static Dictionary<string, StashItem> Inventory() =>
        new()
        {
            [Root] = new StashItem(Root, null, null, "tpl-stash"),
            ["bag"] = new StashItem("bag", Root, "hideout", "tpl-bag"),
            ["in-bag-1"] = new StashItem("in-bag-1", "bag", "main", "tpl-ammo"),
            ["in-bag-2"] = new StashItem("in-bag-2", "bag", "main", "tpl-med"),
            ["loose"] = new StashItem("loose", Root, "hideout", "tpl-med"),
            ["worn"] = new StashItem("worn", "equipment-root", "ArmorVest", "tpl-vest"),
        };

    private static TransferPlan Decide(params string[] ids) =>
        StashTransferPolicy.Decide(
            new StashTransferRequest(Src, Dst, ids),
            Inventory(),
            Root,
            blockedTemplateIds: null);

    [Fact]
    public void 가방을_고르면_안에_든_것이_따라간다()
    {
        var plan = Decide("bag");

        Assert.True(plan.Accepted);
        Assert.Equal(new[] { "bag", "in-bag-1", "in-bag-2" }.OrderBy(x => x), plan.MovingItemIds.OrderBy(x => x));
    }

    // 내용물이 빠지면 그것이 원본에 남아 복제가 된다. 이 시험이 그 자리를 지킨다.
    [Fact]
    public void 옮기는_목록에_중복도_누락도_없다()
    {
        var plan = Decide("bag", "loose");

        Assert.True(plan.Accepted);
        Assert.Equal(4, plan.MovingItemIds.Count);
        Assert.Equal(plan.MovingItemIds.Count, plan.MovingItemIds.Distinct().Count());
        Assert.Contains("in-bag-1", plan.MovingItemIds);
        Assert.Contains("in-bag-2", plan.MovingItemIds);
    }

    // 2026-11-13 에 열었다 — 전에는 "바닥에 놓인 것" 만 건넬 수 있었다(B1 정책).
    // 창은 스태시 트리 전체를 그려 가방을 열 수 있었는데 판정만 바닥을 고집해, 가방 안의 것을
    // 골라 끌면 **아무 일도 일어나지 않았다**(fixes/2026-11-13-stash-transfer-silent-drops.md).
    [Fact]
    public void 가방_안의_것도_따로_건넬_수_있다()
    {
        var plan = Decide("in-bag-1");

        Assert.True(plan.Accepted);
        // 그 하나만 간다 — 가방도, 옆에 든 것도 따라가지 않는다.
        Assert.Equal(new[] { "in-bag-1" }, plan.MovingItemIds);
    }

    [Fact]
    public void 가방_안의_가방을_고르면_그_안의_것이_따라간다()
    {
        var items = Inventory();
        items["inner-bag"] = new StashItem("inner-bag", "bag", "main", "tpl-bag");
        items["in-inner"] = new StashItem("in-inner", "inner-bag", "main", "tpl-ammo");

        var plan = StashTransferPolicy.Decide(
            new StashTransferRequest(Src, Dst, ["inner-bag"]), items, Root, blockedTemplateIds: null);

        Assert.True(plan.Accepted);
        Assert.Equal(["in-inner", "inner-bag"], plan.MovingItemIds.OrderBy(id => id, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void 스태시_밖의_것은_거절한다()
    {
        // 입고 있는 것은 스태시 트리에 매달려 있지 않다 — 열린 뒤에도 그대로 거절이다.
        Assert.False(Decide("worn").Accepted);
    }

    [Fact]
    public void 가방과_그_안의_것을_함께_고르면_거절한다()
    {
        // 둘 다 받으면 안의 것이 가방에서 **빠져나와** 바닥에 놓인다 — 고른 사람이 뜻한 바인지
        // 알 수 없다. 조용히 한쪽을 버리지 않고 거절한다.
        var plan = Decide("bag", "in-bag-1");

        Assert.False(plan.Accepted);
        Assert.Contains("in-bag-1", plan.Reason);
    }

    [Fact]
    public void 원본에_없는_식별자는_거절한다()
    {
        var plan = Decide("ghost");

        Assert.False(plan.Accepted);
        Assert.Empty(plan.MovingItemIds);
        Assert.Contains("없는", plan.Reason);
    }

    // 조용히 지우면 넣는 쪽이 두 개로 읽을 수 있다. 거절로 돌린다.
    [Fact]
    public void 같은_것을_두_번_고르면_거절한다()
    {
        var plan = Decide("loose", "loose");

        Assert.False(plan.Accepted);
        Assert.Contains("두 번", plan.Reason);
    }

    [Fact]
    public void 고른_것이_없으면_거절한다()
    {
        Assert.False(Decide().Accepted);
        Assert.False(StashTransferPolicy.Decide(new StashTransferRequest(Src, Dst, null!), Inventory(), Root, null).Accepted);
    }

    // 저널이 프로필 둘을 전제한다 — 같은 프로필이면 Prepare 가 던지고 그 프로필이 영구히 막힌다.
    [Fact]
    public void 같은_프로필끼리는_거절한다()
    {
        var plan = StashTransferPolicy.Decide(
            new StashTransferRequest(Src, Src, new[] { "loose" }), Inventory(), Root, null);

        Assert.False(plan.Accepted);
        Assert.Contains("같은 프로필", plan.Reason);
    }

    [Fact]
    public void 프로필_식별자가_비면_거절한다()
    {
        Assert.False(StashTransferPolicy.Decide(new StashTransferRequest("  ", Dst, new[] { "loose" }), Inventory(), Root, null).Accepted);
        Assert.False(StashTransferPolicy.Decide(new StashTransferRequest(Src, null, new[] { "loose" }), Inventory(), Root, null).Accepted);
        Assert.False(StashTransferPolicy.Decide(null, Inventory(), Root, null).Accepted);
    }

    [Fact]
    public void 원본이나_루트를_못_읽으면_거절한다()
    {
        var req = new StashTransferRequest(Src, Dst, new[] { "loose" });

        Assert.False(StashTransferPolicy.Decide(req, null, Root, null).Accepted);
        Assert.False(StashTransferPolicy.Decide(req, new Dictionary<string, StashItem>(), Root, null).Accepted);
        Assert.False(StashTransferPolicy.Decide(req, Inventory(), null, null).Accepted);
    }

    [Fact]
    public void 막힌_종류는_거절한다()
    {
        var plan = StashTransferPolicy.Decide(
            new StashTransferRequest(Src, Dst, new[] { "loose" }),
            Inventory(),
            Root,
            new[] { "tpl-med" });

        Assert.False(plan.Accepted);
        Assert.Contains("건넬 수 없는", plan.Reason);
    }

    // 고리가 있는 손상된 프로필에서도 멈춰야 한다 — 안 그러면 서버가 그 자리에서 굳는다.
    [Fact]
    public void 부모_고리가_있어도_멈춘다()
    {
        var items = new Dictionary<string, StashItem>
        {
            [Root] = new StashItem(Root, null, null, "tpl-stash"),
            ["a"] = new StashItem("a", Root, "hideout", "tpl-bag"),
            ["b"] = new StashItem("b", "a", "main", "tpl-bag"),
            ["c"] = new StashItem("c", "b", "main", "tpl-bag"),
        };
        // c 의 자식으로 a 를 매단다(고리).
        items["a2"] = new StashItem("a2", "c", "main", "tpl-bag");
        items["c2"] = new StashItem("c2", "a2", "main", "tpl-bag");

        var plan = StashTransferPolicy.Decide(
            new StashTransferRequest(Src, Dst, new[] { "a" }), items, Root, null);

        Assert.True(plan.Accepted);
        Assert.Equal(plan.MovingItemIds.Count, plan.MovingItemIds.Distinct().Count());
    }

    [Fact]
    public void 한_번에_옮길_수_있는_수를_넘으면_거절한다()
    {
        var items = new Dictionary<string, StashItem>
        {
            [Root] = new StashItem(Root, null, null, "tpl-stash"),
            ["bag"] = new StashItem("bag", Root, "hideout", "tpl-bag"),
        };
        for (var i = 0; i < StashTransferPolicy.MaxItems + 1; i++)
        {
            items[$"i{i}"] = new StashItem($"i{i}", "bag", "main", "tpl-ammo");
        }

        var plan = StashTransferPolicy.Decide(
            new StashTransferRequest(Src, Dst, new[] { "bag" }), items, Root, null);

        Assert.False(plan.Accepted);
        Assert.Contains("한 번에", plan.Reason);
    }

    // 거절은 **아무것도 옮기지 않는다**. 목록이 비어 있어야 호출자가 실수로 반쯤 옮기지 않는다.
    [Fact]
    public void 거절하면_옮길_목록이_비어_있다()
    {
        foreach (var plan in new[] { Decide("worn"), Decide("ghost"), Decide() })
        {
            Assert.False(plan.Accepted);
            Assert.Empty(plan.MovingItemIds);
        }
    }
}
