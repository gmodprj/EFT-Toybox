using ProfilePorter.Server;
using Xunit;

namespace ProfilePorter.Tests;

/// <summary>
/// 세부 보기의 **자리 배치 판정**. 설계 §4.2 의 세 갈래(격자·슬롯·loose)를 가둔다.
///
/// 이 판정이 틀리면 **물건이 화면에서 사라진다** — 되짚어 보려고 여는 창에서 그것이 가장 나쁘다.
/// </summary>
public class DetailPlannerTests
{
    private const string Rig = "rig";

    /// <summary>격자에 놓인 것의 자리. 모양은 프로필과 같다(`{x,y,r}`).</summary>
    private static object Spot(int x, int y, string r = "Horizontal") => new { x, y, r };

    private static PorterItem Child(string id, string slotId, object? location = null) =>
        new(id, Rig, slotId, "tpl-" + id, null, location);

    [Fact]
    public void 격자와_슬롯을_함께_가진_리그는_둘_다_나온다()
    {
        // 방탄 리그 — 주머니(격자)와 플레이트(슬롯)를 같이 가진다. DB 335종 중 33종이 그렇다.
        List<PorterItem> children =
        [
            Child("mag", "1", Spot(0, 0)),
            Child("plate", "Front_plate"),
        ];

        var place = DetailPlanner.Place(children, [new GridSpec("1", 2, 2)], new HashSet<string> { "Front_plate" });

        Assert.Single(place.Grids);
        Assert.Equal("mag", Assert.Single(place.Grids[0].Rows).Id);
        Assert.Equal("plate", Assert.Single(place.Slots).Id);
        Assert.Empty(place.Loose);
    }

    [Fact]
    public void 슬롯에_꽂힌_것은_자리가_없어도_loose_가_아니다()
    {
        // 플레이트·부착물은 좌표가 **원래** 없다. "좌표 없으면 loose" 를 먼저 적용하면 여기서 샌다.
        List<PorterItem> children = [Child("scope", "mod_scope")];

        var place = DetailPlanner.Place(children, [], new HashSet<string> { "mod_scope" });

        Assert.Equal("scope", Assert.Single(place.Slots).Id);
        Assert.Empty(place.Loose);
    }

    [Fact]
    public void 격자에_가야_하는데_좌표가_없으면_loose_로_간다()
    {
        // 2026-11-08 이전에 내보낸 묵은 파일이 이렇다(설계 §20).
        List<PorterItem> children = [Child("lost", "1")];

        var place = DetailPlanner.Place(children, [new GridSpec("1", 2, 2)], new HashSet<string>());

        Assert.Empty(place.Grids[0].Rows);
        var loose = Assert.Single(place.Loose);
        Assert.Equal("lost", loose.Id);
        Assert.False(string.IsNullOrWhiteSpace(loose.Why));
    }

    [Fact]
    public void 모르는_slotId_는_버리지_않고_loose_로_간다()
    {
        List<PorterItem> children = [Child("wibble", "mod_wibble", Spot(0, 0))];

        var place = DetailPlanner.Place(children, [new GridSpec("1", 2, 2)], new HashSet<string>());

        Assert.Equal("wibble", Assert.Single(place.Loose).Id);
    }

    [Fact]
    public void 같은_칸에_둘이_겹쳐도_둘_다_나온다()
    {
        List<PorterItem> children =
        [
            Child("a", "1", Spot(0, 0)),
            Child("b", "1", Spot(0, 0)),
        ];

        var place = DetailPlanner.Place(children, [new GridSpec("1", 2, 2)], new HashSet<string>());

        Assert.Equal(2, place.Grids[0].Rows.Count);
    }

    [Fact]
    public void 뿌리에서_닿지_않는_자손도_ChildrenOf_로_찾을_수_있다()
    {
        // 뿌리 목록에 없는 상자에 매달린 것도 그 상자를 고르면 보여야 한다.
        var bundle = new PorterBundle(
            PorterBundle.KindStash,
            ["bag"],
            [
                new PorterItem("bag", "old-stash", "hideout", "tpl-bag"),
                new PorterItem("orphan-box", "nowhere", "main", "tpl-box"),
                new PorterItem("inside", "orphan-box", "main", "tpl-thing"),
            ]);

        var children = DetailPlanner.ChildrenOf(bundle, "orphan-box");

        Assert.Equal("inside", Assert.Single(children).Id);
    }

    [Fact]
    public void ChildrenOf_는_직계_자식만_준다()
    {
        var bundle = new PorterBundle(
            PorterBundle.KindStash,
            ["bag"],
            [
                new PorterItem("bag", "old-stash", "hideout", "tpl-bag"),
                new PorterItem("a", "bag", "main", "tpl-thing"),
                new PorterItem("deep", "a", "main", "tpl-thing"),
            ]);

        var children = DetailPlanner.ChildrenOf(bundle, "bag");

        Assert.Equal("a", Assert.Single(children).Id);
    }

    [Fact]
    public void 뿌리에서_닿지_않는_항목을_찾아낸다()
    {
        // 뿌리에도 없고 부모가 파일 안에 없으면, 창은 그 id 를 **알 길이 없어** 지목할 수도 없다.
        // 서버가 먼저 찾아내 보여 줘야 "없던 것" 으로 읽히지 않는다.
        var bundle = new PorterBundle(
            PorterBundle.KindStash,
            ["bag"],
            [
                new PorterItem("bag", "old-stash", "hideout", "tpl-bag"),
                new PorterItem("inside", "bag", "main", "tpl-thing"),
                new PorterItem("orphan-box", "nowhere", "main", "tpl-box"),
                new PorterItem("deep", "orphan-box", "main", "tpl-thing"),
            ]);

        var lost = DetailPlanner.Unreachable(bundle);

        // 끊긴 **가지의 머리**만 돌려준다 — 그 아래는 그 상자를 열면 보인다.
        Assert.Equal("orphan-box", Assert.Single(lost).Id);
    }

    [Fact]
    public void 같은_이름_격자가_둘이어도_던지지_않는다()
    {
        // 모드 아이템이 `_name` 을 겹쳐 쓸 수 있다. 던지면 그 상자는 영영 못 연다.
        List<PorterItem> children = [Child("a", "1", Spot(0, 0))];

        var place = DetailPlanner.Place(
            children, [new GridSpec("1", 2, 2), new GridSpec("1", 3, 3)], new HashSet<string>());

        Assert.Equal("a", Assert.Single(place.Grids[0].Rows).Id);
        Assert.Empty(place.Loose);
    }

    [Fact]
    public void 스태시_칸수는_현재크기와_테두리_중_큰_쪽이다()
    {
        // 칸 수는 돌고 있는 DB 가 정하고 그 사이 바뀐다. 작게 잡으면 큰 좌표의 물건이 격자 밖이다.
        var wide = DetailPlanner.StashCells(10, 20, [(11, 0, 2, 1)]);
        Assert.Equal((13, 20), wide);

        // 보낸 프로필이 없으면 테두리만 쓴다.
        var onlyBounds = DetailPlanner.StashCells(0, 0, [(0, 0, 2, 3)]);
        Assert.Equal((2, 3), onlyBounds);
    }
}
