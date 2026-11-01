using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 이 판정이 고른 것은 플레이어가 실제로 플레이하는 프로필에서 영구히 지워진다. 되돌릴 방법이 없다.
// 그래서 여기 테스트는 "지워진다"보다 "지워지지 않는다" 쪽이 많고, 경계마다 하나씩 못박는다.
public class WornGearLossPolicyTests
{
    private const string Root = "equipmentroot0000000001";
    private const string Stash = "stashroot00000000000001";

    private static WornGearItem Worn(string id, string slot) => new WornGearItem(id, Root, slot);

    // 전형적인 프로필 하나: 장비 루트 아래 착용 장비, 유지 슬롯, 주머니와 그 짐, 보안 컨테이너와 그 내용물,
    // 그리고 전혀 다른 부모를 가진 스태시 아이템.
    private static List<WornGearItem> Profile() => new List<WornGearItem>
    {
        new WornGearItem(Root, null, null),
        Worn("helmet", "Headwear"),
        Worn("rig", "TacticalVest"),
        new WornGearItem("rig-ammo", "rig", "main"),
        Worn("rifle", "FirstPrimaryWeapon"),
        new WornGearItem("rifle-mag", "rifle", "mod_magazine"),
        Worn("knife", "Scabbard"),
        Worn("band", "ArmBand"),
        Worn("tag", "Dogtag"),
        Worn("pockets", "Pockets"),
        new WornGearItem("pocket-cash", "pockets", "pocket1"),
        new WornGearItem("pocket-meds", "pockets", "pocket2"),
        Worn("secure", "SecuredContainer"),
        new WornGearItem("secure-keys", "secure", "main"),
        new WornGearItem(Stash, null, null),
        new WornGearItem("stash-rifle", Stash, "hideout"),
        new WornGearItem("stash-rifle-mag", "stash-rifle", "mod_magazine"),
    };

    [Fact]
    public void 착용한_장비는_고른다()
    {
        var selected = WornGearLossPolicy.SelectLostRootIds(Profile(), Root);

        Assert.Contains("helmet", selected);
        Assert.Contains("rig", selected);
        Assert.Contains("rifle", selected);
    }

    [Fact]
    public void 보안_컨테이너와_그_내용물은_지키다()
    {
        var selected = WornGearLossPolicy.SelectLostRootIds(Profile(), Root);
        var gone = WornGearLossPolicy.CollectSubtreeIds(Profile(), selected);

        Assert.DoesNotContain("secure", selected);
        Assert.DoesNotContain("secure", gone);
        Assert.DoesNotContain("secure-keys", gone);
    }

    [Fact]
    public void 주머니_컨테이너는_남고_그_안의_짐은_지운다()
    {
        var selected = WornGearLossPolicy.SelectLostRootIds(Profile(), Root);

        Assert.DoesNotContain("pockets", selected);
        Assert.Contains("pocket-cash", selected);
        Assert.Contains("pocket-meds", selected);
    }

    [Fact]
    public void 칼_완장_인식표_특수슬롯은_지키다()
    {
        var items = Profile();
        items.Add(Worn("radio", "SpecialSlot1"));

        var selected = WornGearLossPolicy.SelectLostRootIds(items, Root);

        Assert.DoesNotContain("knife", selected);
        Assert.DoesNotContain("band", selected);
        Assert.DoesNotContain("tag", selected);
        Assert.DoesNotContain("radio", selected);
    }

    [Fact]
    public void 주머니에_달린_인식표나_특수슬롯은_짐이_아니라_지키다()
    {
        var items = Profile();
        // PIT 은 스폰 시 특수 슬롯을 주머니 아래로 재부모화한다. 그래도 지워지면 안 된다.
        items.Add(new WornGearItem("pocket-special", "pockets", "SpecialSlot2"));
        items.Add(new WornGearItem("pocket-tag", "pockets", "Dogtag"));

        var selected = WornGearLossPolicy.SelectLostRootIds(items, Root);

        Assert.DoesNotContain("pocket-special", selected);
        Assert.DoesNotContain("pocket-tag", selected);
    }

    [Fact]
    public void 스태시_아이템은_결코_고르지_않는다()
    {
        var selected = WornGearLossPolicy.SelectLostRootIds(Profile(), Root);
        var gone = WornGearLossPolicy.CollectSubtreeIds(Profile(), selected);

        Assert.DoesNotContain(Stash, selected);
        Assert.DoesNotContain("stash-rifle", selected);
        Assert.DoesNotContain(Stash, gone);
        Assert.DoesNotContain("stash-rifle", gone);
        Assert.DoesNotContain("stash-rifle-mag", gone);
    }

    [Fact]
    public void 유지_슬롯에_달린_아이템은_고르지_않는다()
    {
        var items = Profile();
        // 보안 컨테이너와 칼에 달린 것은 주머니 짐이 아니다. 주머니만 짐을 잃는다.
        items.Add(new WornGearItem("secure-extra", "secure", "main"));
        items.Add(new WornGearItem("knife-mod", "knife", "mod_handle"));

        var selected = WornGearLossPolicy.SelectLostRootIds(items, Root);

        Assert.DoesNotContain("secure-extra", selected);
        Assert.DoesNotContain("knife-mod", selected);
    }

    // 모르는 슬롯 이름은 fail closed, 즉 **지우지 않는다** 를 골랐다. 허용 목록 방식이라 오타든
    // 상위 SPT 가 나중에 더한 슬롯이든 전부 이쪽으로 떨어진다. 반대로 골랐다면 이름이 바뀐 한 번의
    // 업스트림 변경이 플레이어 장비를 지우는 길이 된다.
    [Fact]
    public void 모르는_슬롯_이름은_지우지_않는다()
    {
        var items = Profile();
        items.Add(Worn("mystery", "ThirdPrimaryWeapon"));
        items.Add(Worn("typo", "Headwearr"));

        var selected = WornGearLossPolicy.SelectLostRootIds(items, Root);

        Assert.DoesNotContain("mystery", selected);
        Assert.DoesNotContain("typo", selected);
    }

    [Fact]
    public void 장비_루트가_비면_아무것도_고르지_않는다()
    {
        Assert.Empty(WornGearLossPolicy.SelectLostRootIds(Profile(), null));
        Assert.Empty(WornGearLossPolicy.SelectLostRootIds(Profile(), string.Empty));
        Assert.Empty(WornGearLossPolicy.SelectLostRootIds(Profile(), "   "));
    }

    [Fact]
    public void 장비_루트가_다르면_아무것도_고르지_않는다()
    {
        // 루트를 잘못 짚으면 엉뚱한 트리가 사라진다. 그래서 부모 id 는 정확히 일치해야 한다.
        Assert.Empty(WornGearLossPolicy.SelectLostRootIds(Profile(), Root + "x"));
        Assert.Empty(WornGearLossPolicy.SelectLostRootIds(Profile(), Root.ToUpperInvariant()));
    }

    [Fact]
    public void 목록이_없으면_아무것도_고르지_않는다()
    {
        Assert.Empty(WornGearLossPolicy.SelectLostRootIds(null, Root));
    }

    [Fact]
    public void 아이템이_null_이거나_id_슬롯이_비면_건너뛴다()
    {
        var items = new List<WornGearItem>
        {
            null!,
            new WornGearItem(null, Root, "Headwear"),
            new WornGearItem("  ", Root, "ArmorVest"),
            new WornGearItem("noslot", Root, null),
            new WornGearItem("blankslot", Root, "   "),
            Worn("helmet", "Headwear"),
        };

        var selected = WornGearLossPolicy.SelectLostRootIds(items, Root);

        Assert.Equal(new[] { "helmet" }, selected);
    }

    [Fact]
    public void 슬롯_이름은_앞뒤_공백과_대소문자를_무시한다()
    {
        var items = new List<WornGearItem>
        {
            Worn("helmet", " headwear "),
            Worn("pockets", " POCKETS "),
            new WornGearItem("pocket-cash", "pockets", "pocket1"),
        };

        var selected = WornGearLossPolicy.SelectLostRootIds(items, Root);

        Assert.Contains("helmet", selected);
        Assert.DoesNotContain("pockets", selected);
        Assert.Contains("pocket-cash", selected);
    }

    [Fact]
    public void 지울_목록에는_하위_전부가_담긴다()
    {
        var selected = WornGearLossPolicy.SelectLostRootIds(Profile(), Root);
        var gone = WornGearLossPolicy.CollectSubtreeIds(Profile(), selected);

        Assert.Contains("rig", gone);
        Assert.Contains("rig-ammo", gone);
        Assert.Contains("rifle", gone);
        Assert.Contains("rifle-mag", gone);
    }

    [Fact]
    public void 루트가_없으면_지울_목록도_비어_있다()
    {
        Assert.Empty(WornGearLossPolicy.CollectSubtreeIds(Profile(), null));
        Assert.Empty(WornGearLossPolicy.CollectSubtreeIds(Profile(), new[] { "  ", string.Empty }));
        Assert.Equal(new[] { "helmet" }, WornGearLossPolicy.CollectSubtreeIds(null, new[] { "helmet" }));
    }

    [Fact]
    public void 부모_고리가_있어도_지울_목록_계산이_끝난다()
    {
        // 손상된 프로필: a 의 부모가 b, b 의 부모가 a. 같은 id 를 두 번 담지 않으므로 멈춘다.
        var items = new List<WornGearItem>
        {
            Worn("helmet", "Headwear"),
            new WornGearItem("a", "helmet", "main"),
            new WornGearItem("b", "a", "main"),
            new WornGearItem("a", "b", "main"),
        };

        var gone = WornGearLossPolicy.CollectSubtreeIds(items, new[] { "helmet" });

        Assert.Equal(new[] { "helmet", "a", "b" }.Order(), gone.Order());
    }
}
