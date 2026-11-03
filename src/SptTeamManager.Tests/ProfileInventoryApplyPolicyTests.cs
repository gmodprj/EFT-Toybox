using System.Linq;
using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 계획 5 Task B. 편집기가 끝난 뒤의 **그 팀원 인벤토리 전체**를 받아 "이 변화가 정당한가" 만 본다.
// 조작의 종류(이동·나누기·합치기·열기·버리기)를 세지 않는 것이 요점이다 - 세면 늘 빠진다.
//
// 막아야 하는 것은 **늘어나는 것**(복제)과 **바깥에서 들어오는 것**(플레이어 아이템)과 **구조 손상**이다.
// 줄어드는 것은 막지 않는다: 플레이어는 자기 화면에서 버리고 탄박스를 소비할 수 있고, 팀원 화면도 같아야
// 한다. 대신 사라지는 id 를 전부 돌려주어 호출자가 로그에 남긴다.
public class ProfileInventoryApplyPolicyTests
{
    private const string Stash = "stash-root";
    private const string Equip = "equip-root";

    private static InventoryItemState Item(string id, string tpl, string? parent, string? slot, int count = 1)
        => new InventoryItemState(id, tpl, parent, slot, count);

    // 팀원 하나: 장비 루트에 주머니·보안 컨테이너·리그, 스태시에 탄 더미 하나와 탄박스 하나.
    private static List<InventoryItemState> Before() => new()
    {
        Item(Equip, "tpl-equip", null, null),
        Item("pockets", "tpl-pockets", Equip, "Pockets"),
        Item("secure", "tpl-secure", Equip, "SecuredContainer"),
        Item("rig", "tpl-rig", Equip, "TacticalVest"),
        Item("mag", "tpl-mag", "rig", "1"),
        Item(Stash, "tpl-stash", null, null),
        Item("ammo", "tpl-ammo", Stash, "hideout", 860),
        Item("box", "tpl-box", Stash, "hideout"),
        Item("box-ammo", "tpl-ammo", "box", "cartridges", 120),
    };

    // 편집기가 before 의 아이템을 **전부** 들고 있었던 경우. 사라진 것은 플레이어가 버린 것이다.
    private static InventoryApplyVerdict Judge(List<InventoryItemState> after)
        => ProfileInventoryApplyPolicy.Validate(Before(), after, Stash, Equip, Before().Select(i => i.Id!).ToList());

    [Fact]
    public void 바뀐_것이_없으면_허용하고_잃은_것도_없다()
    {
        var verdict = Judge(Before());

        Assert.True(verdict.Allowed);
        Assert.Empty(verdict.LostItemIds);
    }

    [Fact]
    public void 묶음을_나누면_허용한다()
    {
        // 860발에서 50발을 떼어 탄창에 넣었다. 새 id 가 생기지만 총량은 그대로다.
        var after = Before();
        after.RemoveAll(i => i.Id == "ammo");
        after.Add(Item("ammo", "tpl-ammo", Stash, "hideout", 810));
        after.Add(Item("ammo-split", "tpl-ammo", "mag", "cartridges", 50));

        var verdict = Judge(after);

        Assert.True(verdict.Allowed, verdict.Reason);
        Assert.Empty(verdict.LostItemIds);
    }

    [Fact]
    public void 총량이_늘면_거절한다()
    {
        // 나눈 것처럼 보이지만 합이 10발 많다 - 복제다.
        var after = Before();
        after.RemoveAll(i => i.Id == "ammo");
        after.Add(Item("ammo", "tpl-ammo", Stash, "hideout", 860));
        after.Add(Item("ammo-split", "tpl-ammo", "mag", "cartridges", 10));

        var verdict = Judge(after);

        Assert.False(verdict.Allowed);
        Assert.Contains("tpl-ammo", verdict.Reason);
    }

    [Fact]
    public void 원래_없던_종류는_거절한다()
    {
        // 플레이어 스태시에서 들어온 아이템이 이렇게 보인다 - 그 종류가 before 에 아예 없다.
        var after = Before();
        after.Add(Item("foreign", "tpl-gold", Stash, "hideout"));

        var verdict = Judge(after);

        Assert.False(verdict.Allowed);
        Assert.Contains("tpl-gold", verdict.Reason);
    }

    [Fact]
    public void 탄박스를_소비하는_것은_허용하고_사라진_id_를_보고한다()
    {
        // 탄박스를 열면 상자가 없어지고 안의 탄만 남는다. 줄어드는 것은 플레이어의 권한이다.
        var after = Before();
        after.RemoveAll(i => i.Id == "box" || i.Id == "box-ammo");
        after.Add(Item("box-ammo", "tpl-ammo", Stash, "hideout", 120));

        var verdict = Judge(after);

        Assert.True(verdict.Allowed, verdict.Reason);
        Assert.Contains("box", verdict.LostItemIds);
    }

    [Fact]
    public void 구조를_이루는_자리가_없어지거나_옮겨지면_거절한다()
    {
        var removed = Before();
        removed.RemoveAll(i => i.Id == "secure");
        Assert.False(Judge(removed).Allowed);

        var moved = Before();
        moved.RemoveAll(i => i.Id == "pockets");
        moved.Add(Item("pockets", "tpl-pockets", Stash, "hideout"));
        var verdict = Judge(moved);
        Assert.False(verdict.Allowed);
        Assert.Contains("pockets", verdict.Reason);
    }

    [Fact]
    public void 두_루트_중_어디에도_닿지_않는_아이템은_거절한다()
    {
        var after = Before();
        after.Add(Item("orphan", "tpl-ammo", "없는부모", "hideout", 1));

        var verdict = Judge(after);

        Assert.False(verdict.Allowed);
        Assert.Contains("orphan", verdict.Reason);
    }

    [Fact]
    public void 고리가_있으면_거절한다()
    {
        var after = Before();
        after.RemoveAll(i => i.Id == "rig" || i.Id == "mag");
        after.Add(Item("rig", "tpl-rig", "mag", "1"));
        after.Add(Item("mag", "tpl-mag", "rig", "1"));

        Assert.False(Judge(after).Allowed);
    }

    [Fact]
    public void 루트가_빠지거나_옮겨지면_거절한다()
    {
        var missing = Before();
        missing.RemoveAll(i => i.Id == Stash);
        Assert.False(Judge(missing).Allowed);

        var reparented = Before();
        reparented.RemoveAll(i => i.Id == Equip);
        reparented.Add(Item(Equip, "tpl-equip", Stash, "hideout"));
        Assert.False(Judge(reparented).Allowed);
    }

    [Fact]
    public void 빈_제출은_거절한다()
    {
        Assert.False(ProfileInventoryApplyPolicy.Validate(Before(), new List<InventoryItemState>(), Stash, Equip, null).Allowed);
        Assert.False(ProfileInventoryApplyPolicy.Validate(Before(), null, Stash, Equip, null).Allowed);
        Assert.False(ProfileInventoryApplyPolicy.Validate(null, Before(), Stash, Equip, null).Allowed);
    }

    [Fact]
    public void 편집기가_들고_있지_않던_아이템은_사라질_수_없다()
    {
        // 2026-11-03 실측 사고: 편집기가 EFT 역직렬화에 걸린 아이템을 건너뛰고 열렸고, 최종 상태를
        // 제출하자 **그 68개가 프로필에서 지워졌다**. 편집기가 보여 주지 않은 것은 플레이어가 버린
        // 것일 수 없다 - 그래서 "들고 있던 목록" 밖의 손실은 거절한다.
        var after = Before();
        after.RemoveAll(i => i.Id == "box");

        var loadedWithoutBox = Before().Select(i => i.Id!).Where(id => id != "box").ToList();
        var verdict = ProfileInventoryApplyPolicy.Validate(Before(), after, Stash, Equip, loadedWithoutBox);

        Assert.False(verdict.Allowed);
        Assert.Contains("box", verdict.Reason);
    }

    [Fact]
    public void 들고_있던_목록을_모르면_아무것도_잃지_않는다()
    {
        // 닫힌 쪽. 목록이 없으면 "편집기가 뭘 들고 있었는지 모른다" 이고, 모르면 지우지 않는다.
        var after = Before();
        after.RemoveAll(i => i.Id == "box");

        Assert.False(ProfileInventoryApplyPolicy.Validate(Before(), after, Stash, Equip, null).Allowed);
        Assert.False(ProfileInventoryApplyPolicy.Validate(Before(), after, Stash, Equip, new List<string>()).Allowed);
    }

    [Fact]
    public void 들고_있던_아이템을_버리는_것은_그대로_허용한다()
    {
        // 같은 조작이라도 편집기가 그 아이템을 들고 있었으면 플레이어의 선택이다.
        var after = Before();
        after.RemoveAll(i => i.Id == "box" || i.Id == "box-ammo");
        after.Add(Item("box-ammo", "tpl-ammo", Stash, "hideout", 120));

        var verdict = Judge(after);

        Assert.True(verdict.Allowed, verdict.Reason);
        Assert.Contains("box", verdict.LostItemIds);
    }
}
