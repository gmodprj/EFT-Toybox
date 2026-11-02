using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 편집기가 끝난 뒤의 상태와 시작 때의 상태를 비교해, 서버 라우트에 보낼 "한 건씩의 이동" 목록을 만든다.
// 서버는 건을 받은 순서대로 누적해 판정하므로 순서가 틀리면 맞는 편집이 거절된다(부모가 아직 장비 트리에 없다 /
// 꺼낼 아이템이 이미 다른 트리로 갔다). 그 순서와 "보낼 수 없는 변화를 조용히 버리지 않는다" 가 이 판정의 일이다.
public class GearEditBatchPlannerTests
{
    private const string Stash = "stash";
    private const string Equip = "equip";

    private static EditorItemState Item(string id, string? parent, string? slot, string? location = null)
        => new EditorItemState(id, parent, slot, location);

    // 시작 상태: 장비 = 소총(worn-rifle). 스태시 = 헬멧, 탄창(+탄 한 알), 조끼. 마지막 인자는 LayoutKey(칸 위치).
    private static List<EditorItemState> Before() => new()
    {
        Item(Equip, null, null),
        Item("worn-rifle", Equip, "FirstPrimaryWeapon"),
        Item(Stash, null, null),
        Item("stash-helmet", Stash, "hideout", "0,0"),
        Item("stash-mag", Stash, "hideout", "2,0"),
        Item("stash-bullet", "stash-mag", "cartridges"),
        Item("stash-vest", Stash, "hideout", "4,0"),
    };

    private static List<EditorItemState> Moved(List<EditorItemState> items, string id, string? parent, string? slot, string? location = null)
        => items.Select(i => i.Id == id ? i with { ParentId = parent, SlotId = slot, LayoutKey = location } : i).ToList();

    [Fact]
    public void 바뀐_것이_없으면_빈_목록이고_성공이다()
    {
        var plan = GearEditBatchPlanner.Plan(Before(), Before());

        Assert.True(plan.Ok);
        Assert.Empty(plan.Edits);
        Assert.Equal(0, plan.LayoutOnlyCount);
    }

    [Fact]
    public void 스태시에서_장비로_옮긴_아이템은_도착_부모와_슬롯으로_한_건이_된다()
    {
        var after = Moved(Before(), "stash-helmet", Equip, "Headwear");

        var plan = GearEditBatchPlanner.Plan(Before(), after);

        Assert.True(plan.Ok);
        var edit = Assert.Single(plan.Edits);
        Assert.Equal(new PlannedGearEdit("stash-helmet", "Headwear", Equip), edit);
    }

    [Fact]
    public void 장비에서_스태시_루트로_옮긴_아이템은_스태시_루트와_hideout_로_한_건이_된다()
    {
        var after = Moved(Before(), "worn-rifle", Stash, "hideout", "6,0");

        var plan = GearEditBatchPlanner.Plan(Before(), after);

        Assert.True(plan.Ok);
        Assert.Equal(new PlannedGearEdit("worn-rifle", "hideout", Stash), Assert.Single(plan.Edits));
    }

    [Fact]
    public void 아이템과_함께_움직이는_자식은_건이_되지_않는다()
    {
        // 탄창이 총에 꽂혀도 그 안의 탄은 부모(탄창)가 그대로이므로 바뀐 것이 아니다. 서브트리는 부모와 함께 간다.
        var after = Moved(Before(), "stash-mag", "worn-rifle", "mod_magazine");

        var plan = GearEditBatchPlanner.Plan(Before(), after);

        Assert.True(plan.Ok);
        Assert.Equal("stash-mag", Assert.Single(plan.Edits).ItemId);
    }

    [Fact]
    public void 새_부모가_같이_옮겨지면_부모_건이_먼저다()
    {
        // 탄창을 총에 꽂고(1) 그 탄창에 스태시의 다른 탄을 넣는다(2). 서버는 순서대로 누적해 보므로
        // 2 의 부모(탄창)가 장비 트리에 있으려면 1 이 먼저여야 한다. 입력 순서와 무관하게 부모가 앞이다.
        var before = Before();
        before.Add(Item("stash-bullet2", Stash, "hideout", "8,0"));
        var after = Moved(before, "stash-bullet2", "stash-mag", "cartridges");
        after = Moved(after, "stash-mag", "worn-rifle", "mod_magazine");

        var plan = GearEditBatchPlanner.Plan(before, after);

        Assert.True(plan.Ok);
        Assert.Equal(new[] { "stash-mag", "stash-bullet2" }, plan.Edits.Select(e => e.ItemId));
    }

    [Fact]
    public void 벗길_때는_자식_건이_부모_건보다_먼저다()
    {
        // 장비의 총에서 탄창을 빼고(자식) 총도 스태시로 벗긴다(부모). 총을 먼저 벗기면 탄창이 이미 스태시 트리에 있어
        // 서버가 "출처가 장비 트리가 아니다" 로 거절한다. 꺼낼 것이 먼저, 그 부모는 나중이다.
        var before = new List<EditorItemState>
        {
            Item(Equip, null, null),
            Item("worn-rifle", Equip, "FirstPrimaryWeapon"),
            Item("worn-mag", "worn-rifle", "mod_magazine"),
            Item(Stash, null, null),
        };
        var after = Moved(before, "worn-rifle", Stash, "hideout", "0,0");
        after = Moved(after, "worn-mag", Stash, "hideout", "4,0");

        var plan = GearEditBatchPlanner.Plan(before, after);

        Assert.True(plan.Ok);
        Assert.Equal(new[] { "worn-mag", "worn-rifle" }, plan.Edits.Select(e => e.ItemId));
    }

    [Fact]
    public void 꺼낼_아이템이_들어_있던_스태시_가방도_옮겨지면_꺼내는_건이_먼저다()
    {
        // 스태시의 상자(stash-box) 안에 있던 것(stash-pack)을 장비 조끼로 옮기고, 상자도 장비 배낭 자리로 옮긴다.
        // 상자를 먼저 옮기면 그 안의 것은 더 이상 스태시 트리에 없어 서버가 "출처가 스태시가 아니다" 로 거절한다.
        var before = new List<EditorItemState>
        {
            Item(Equip, null, null),
            Item("worn-vest", Equip, "TacticalVest"),
            Item(Stash, null, null),
            Item("stash-box", Stash, "hideout", "0,0"),
            Item("stash-pack", "stash-box", "main", "0,0"),
        };
        var after = Moved(before, "stash-pack", "worn-vest", "pocket1");
        after = Moved(after, "stash-box", Equip, "Backpack");

        var plan = GearEditBatchPlanner.Plan(before, after);

        Assert.True(plan.Ok);
        Assert.Equal(new[] { "stash-pack", "stash-box" }, plan.Edits.Select(e => e.ItemId));
    }

    [Fact]
    public void 얽힌_건도_한_방향으로_풀리면_그_순서로_낸다()
    {
        // a 는 b 안에 있다가 장비로 나오고, b 는 스태시에서 방금 나온 a 안으로 들어간다.
        // a 의 옛 부모 b 에 건이 있으니 a 가 먼저이고, b 의 새 부모 a 에 건이 있으니 이것도 a 가 먼저다 - 같은 방향이라 풀린다.
        var before = new List<EditorItemState>
        {
            Item(Equip, null, null),
            Item(Stash, null, null),
            Item("a", "b", "main", "0,0"),
            Item("b", Stash, "hideout", "0,0"),
        };
        var after = Moved(before, "b", "a", "main", "0,0");
        after = Moved(after, "a", Equip, "Backpack");

        var plan = GearEditBatchPlanner.Plan(before, after);

        Assert.True(plan.Ok);
        Assert.Equal(new[] { "a", "b" }, plan.Edits.Select(e => e.ItemId));
    }

    [Fact]
    public void 서로가_서로의_앞이어야_하면_순환으로_거부한다()
    {
        // x 의 새 부모가 y 이면 y 의 건이 먼저여야 하고, y 의 새 부모가 x 이면 x 의 건이 먼저여야 한다 - 서로 앞.
        // 순서를 지어내서 서버에 던지지 않는다.
        var before = new List<EditorItemState>
        {
            Item(Equip, null, null),
            Item(Stash, null, null),
            Item("x", Stash, "hideout", "0,0"),
            Item("y", Stash, "hideout", "4,0"),
        };
        var after = Moved(before, "x", "y", "main", "0,0");
        after = Moved(after, "y", "x", "main", "0,0");

        var plan = GearEditBatchPlanner.Plan(before, after);

        Assert.False(plan.Ok);
        Assert.Empty(plan.Edits);
        Assert.False(string.IsNullOrWhiteSpace(plan.Reason));
    }

    [Fact]
    public void 칸_위치만_바꾼_것은_건이_아니고_개수로_알린다()
    {
        // 스태시 안에서 격자 위치만 옮기면 부모·슬롯이 그대로다. 이 경로는 좌표를 받지 않으므로 저장되지 않는다 -
        // 조용히 버리지 않고 몇 건인지 센다(화면이 플레이어에게 말한다).
        var after = Moved(Before(), "stash-vest", Stash, "hideout", "10,3");

        var plan = GearEditBatchPlanner.Plan(Before(), after);

        Assert.True(plan.Ok);
        Assert.Empty(plan.Edits);
        Assert.Equal(1, plan.LayoutOnlyCount);
    }

    [Fact]
    public void 이동과_위치만_바뀐_것이_섞이면_이동만_건이_된다()
    {
        var after = Moved(Before(), "stash-helmet", Equip, "Headwear");
        after = Moved(after, "stash-vest", Stash, "hideout", "10,3");

        var plan = GearEditBatchPlanner.Plan(Before(), after);

        Assert.True(plan.Ok);
        Assert.Equal("stash-helmet", Assert.Single(plan.Edits).ItemId);
        Assert.Equal(1, plan.LayoutOnlyCount);
    }

    [Fact]
    public void 이동한_아이템의_위치는_위치만_바뀐_것으로_세지_않는다()
    {
        // 슬롯으로 들어가면 좌표가 사라진다(null). 이동 건으로 이미 잡혔으므로 위치 개수에 더하지 않는다.
        var after = Moved(Before(), "stash-helmet", Equip, "Headwear", null);

        Assert.Equal(0, GearEditBatchPlanner.Plan(Before(), after).LayoutOnlyCount);
    }

    [Fact]
    public void 편집_중_새로_생긴_아이템이_있으면_거부한다()
    {
        // 묶음 쌓기를 나누면 새 id 가 생긴다. 서버에는 없는 아이템이라 이 경로로는 표현할 수 없다.
        var after = Before();
        after.Add(Item("split-stack", Stash, "hideout", "12,0"));

        var plan = GearEditBatchPlanner.Plan(Before(), after);

        Assert.False(plan.Ok);
        Assert.Empty(plan.Edits);
        Assert.Contains("split-stack", plan.Reason);
    }

    [Fact]
    public void 편집_중_사라진_아이템이_있으면_거부한다()
    {
        // 버리거나 합쳐서 없어진 아이템. 서버는 아이템을 지우는 편집을 받지 않는다.
        var after = Before().Where(i => i.Id != "stash-vest").ToList();

        var plan = GearEditBatchPlanner.Plan(Before(), after);

        Assert.False(plan.Ok);
        Assert.Empty(plan.Edits);
        Assert.Contains("stash-vest", plan.Reason);
    }

    [Fact]
    public void 이동한_아이템의_도착_슬롯을_읽지_못하면_거부한다()
    {
        // 슬롯 이름이 비면 서버에 "어디로" 를 말할 수 없다. 지어내지 않고 멈춘다.
        var after = Moved(Before(), "stash-helmet", Equip, null);

        var plan = GearEditBatchPlanner.Plan(Before(), after);

        Assert.False(plan.Ok);
        Assert.Contains("stash-helmet", plan.Reason);
    }

    [Fact]
    public void 이동한_아이템의_도착_부모를_읽지_못하면_거부한다()
    {
        var after = Moved(Before(), "stash-helmet", null, "Headwear");

        var plan = GearEditBatchPlanner.Plan(Before(), after);

        Assert.False(plan.Ok);
        Assert.Contains("stash-helmet", plan.Reason);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void 시작_상태나_끝_상태가_없으면_거부한다(bool beforeIsNull)
    {
        var plan = beforeIsNull
            ? GearEditBatchPlanner.Plan(null, Before())
            : GearEditBatchPlanner.Plan(Before(), null);

        Assert.False(plan.Ok);
        Assert.Empty(plan.Edits);
        Assert.False(string.IsNullOrWhiteSpace(plan.Reason));
    }

    [Fact]
    public void 같은_id_가_두_번_나오면_거부한다()
    {
        var after = Before();
        after.Add(Item("stash-helmet", Equip, "Headwear"));

        var plan = GearEditBatchPlanner.Plan(Before(), after);

        Assert.False(plan.Ok);
        Assert.Contains("stash-helmet", plan.Reason);
    }

    [Fact]
    public void 순서는_같은_입력에서_늘_같다()
    {
        var after = Moved(Before(), "stash-helmet", Equip, "Headwear");
        after = Moved(after, "stash-vest", Equip, "TacticalVest");

        var first = GearEditBatchPlanner.Plan(Before(), after).Edits.Select(e => e.ItemId).ToList();
        var second = GearEditBatchPlanner.Plan(Before(), after).Edits.Select(e => e.ItemId).ToList();

        Assert.Equal(first, second);
        // 제약이 없는 건들은 끝 상태 목록의 순서를 따른다(결정적이다).
        Assert.Equal(new[] { "stash-helmet", "stash-vest" }, first);
    }
}
