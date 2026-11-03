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

    // ───────────────────────────────────────────────────────────────────────────────────────────
    // 자리의 적법성 (인계 문서 열린 항목 10). 보존만 보던 이 경로가 "이 아이템이 이 자리에 들어가는
    // 종류인가" 도 본다. **이동 목록 경로와 극성이 반대다**: 거기서는 모르면 거부였지만(요청 한 건을
    // 거절하는 비용), 여기서 같은 극성을 쓰면 모델하지 못하는 자리에 앉은 **기존** 아이템 하나가 그
    // 프로필의 모든 저장을 영구히 막는다. 그래서 (ㄱ) 이번 제출이 **실제로 옮긴 자리만** 보고
    // (ㄴ) **틀렸다고 증명된 것만** 거절한다. 아래 테스트가 그 두 좁힘을 못박는다.
    // ───────────────────────────────────────────────────────────────────────────────────────────

    // 자리 판정을 쓰는 팀원. 리그 칸에 탄창, 스태시에 헬멧 둘·가방·탄 860발, 그리고 **원래부터 틀린
    // 자리**에 앉은 가방 하나(odd) - 기존 상태를 건드리지 않는다는 것을 그것으로 증명한다.
    private static List<InventoryItemState> PlaceBefore() => new()
    {
        Item(Equip, "tpl-equip", null, null),
        Item("pockets", "tpl-pockets", Equip, "Pockets"),
        Item("secure", "tpl-secure", Equip, "SecuredContainer"),
        Item("rig", "tpl-rig", Equip, "TacticalVest"),
        Item("mag", "tpl-mag", "rig", "1"),
        Item("odd", "tpl-bag", "mag", "cartridges"),
        Item(Stash, "tpl-stash", null, null),
        Item("helmet", "tpl-helmet", Stash, "hideout"),
        Item("helmet2", "tpl-helmet", Stash, "hideout"),
        Item("bag", "tpl-bag", Stash, "hideout"),
        Item("ammo", "tpl-ammo", Stash, "hideout", 860),
    };

    // 실제 데이터베이스처럼 **범주 노드**를 담는다(ProfileGearEditPolicy.FilterAccepts 의 실측 주석).
    // 모든 템플릿이 Slots·Grids 양쪽에 (비어 있더라도) 들어 있다 - 그것이 "이 템플릿을 읽었다" 는
    // 신호이고, 읽지 못한 템플릿과 구별된다.
    private static GearSlotFacts PlaceFacts(
        IReadOnlyList<GearSlotDefinition>? magSlots = null,
        bool dropMagTemplate = false)
    {
        var templates = new Dictionary<string, string>
        {
            [Equip] = "tpl-equip",
            ["pockets"] = "tpl-pockets",
            ["secure"] = "tpl-secure",
            ["rig"] = "tpl-rig",
            ["mag"] = "tpl-mag",
            ["odd"] = "tpl-bag",
            [Stash] = "tpl-stash",
            ["helmet"] = "tpl-helmet",
            ["helmet2"] = "tpl-helmet",
            ["bag"] = "tpl-bag",
            ["ammo"] = "tpl-ammo",
        };

        var slots = new Dictionary<string, IReadOnlyList<GearSlotDefinition>>
        {
            ["tpl-equip"] = new[]
            {
                new GearSlotDefinition("Headwear", new[] { "node-headwear" }),
                new GearSlotDefinition("TacticalVest", new[] { "node-vest" }),
                new GearSlotDefinition("Pockets", new[] { "node-pockets" }),
                new GearSlotDefinition("SecuredContainer", new[] { "node-secure" }),
            },
            ["tpl-mag"] = magSlots ?? new[] { new GearSlotDefinition("cartridges", new[] { "tpl-ammo" }, 30) },
            ["tpl-rig"] = Array.Empty<GearSlotDefinition>(),
            ["tpl-stash"] = Array.Empty<GearSlotDefinition>(),
            ["tpl-pockets"] = Array.Empty<GearSlotDefinition>(),
            ["tpl-secure"] = Array.Empty<GearSlotDefinition>(),
            ["tpl-helmet"] = Array.Empty<GearSlotDefinition>(),
            ["tpl-bag"] = Array.Empty<GearSlotDefinition>(),
        };

        var grids = new Dictionary<string, IReadOnlyList<GearGridDefinition>>
        {
            ["tpl-stash"] = new[] { new GearGridDefinition("hideout", new[] { "node-item" }) },
            // 가방 안에 가방을 넣지 못하는 것이 ExcludedFilter 다.
            ["tpl-rig"] = new[] { new GearGridDefinition("1", new[] { "node-item" }, new[] { "node-bag" }) },
            ["tpl-pockets"] = new[] { new GearGridDefinition("1", new[] { "node-item" }) },
            ["tpl-secure"] = new[] { new GearGridDefinition("1", new[] { "node-item" }) },
            ["tpl-bag"] = new[] { new GearGridDefinition("1", new[] { "node-item" }) },
            ["tpl-equip"] = Array.Empty<GearGridDefinition>(),
            ["tpl-mag"] = Array.Empty<GearGridDefinition>(),
            ["tpl-helmet"] = Array.Empty<GearGridDefinition>(),
        };

        var parents = new Dictionary<string, string>
        {
            ["tpl-helmet"] = "node-headwear",
            ["tpl-rig"] = "node-vest",
            ["tpl-pockets"] = "node-pockets",
            ["tpl-secure"] = "node-secure",
            ["tpl-bag"] = "node-bag",
            ["tpl-mag"] = "node-item",
            ["tpl-ammo"] = "node-item",
            ["node-headwear"] = "node-item",
            ["node-vest"] = "node-item",
            ["node-pockets"] = "node-item",
            ["node-secure"] = "node-item",
            ["node-bag"] = "node-item",
        };

        if (dropMagTemplate)
        {
            slots.Remove("tpl-mag");
            grids.Remove("tpl-mag");
        }

        return new GearSlotFacts(templates, slots, parents, null, grids);
    }

    private static InventoryApplyVerdict JudgePlace(List<InventoryItemState> after, GearSlotFacts? facts = null)
        => ProfileInventoryApplyPolicy.Validate(
            PlaceBefore(), after, Stash, Equip, PlaceBefore().Select(i => i.Id!).ToList(), facts ?? PlaceFacts());

    // 그 자리로 옮긴 after 를 만든다.
    private static List<InventoryItemState> MovedTo(string id, string parent, string slot, int count = 1)
    {
        var after = PlaceBefore();
        var tpl = after.First(i => i.Id == id).TemplateId!;
        after.RemoveAll(i => i.Id == id);
        after.Add(Item(id, tpl, parent, slot, count));
        return after;
    }

    [Fact]
    public void 자리가_맞으면_허용한다()
    {
        // 헬멧을 머리에(범주 노드를 거쳐 맞는다), 탄창을 스태시 칸으로.
        Assert.True(JudgePlace(MovedTo("helmet", Equip, "Headwear")).Allowed);

        var toStash = MovedTo("mag", Stash, "hideout");
        var verdict = JudgePlace(toStash);
        Assert.True(verdict.Allowed, verdict.Reason);
    }

    [Fact]
    public void 종류가_맞지_않는_슬롯이면_거절한다()
    {
        // 가방을 탄창의 탄 자리에. 필터는 tpl-ammo 만 받고 가방의 조상 사슬에 그것이 없다.
        var verdict = JudgePlace(MovedTo("bag", "mag", "cartridges"));

        Assert.False(verdict.Allowed);
        Assert.Contains("bag", verdict.Reason);
    }

    [Fact]
    public void 격자가_받지_않는_종류면_거절한다()
    {
        // 리그 칸은 node-item 을 받지만 node-bag 을 받지 않는다(ExcludedFilter).
        var verdict = JudgePlace(MovedTo("bag", "rig", "1"));

        Assert.False(verdict.Allowed);
        Assert.Contains("bag", verdict.Reason);
    }

    [Fact]
    public void 그런_자리가_없다는_것은_거절_근거가_아니다()
    {
        // 주머니 템플릿은 읽혔고(슬롯 없음·칸 '1' 하나) 'cartridges' 라는 자리가 없다. 그래도 거절하지
        // 않는다 - 2026-11-04 에 실제 프로필 여섯 개로 재 보고 이 거절을 뺐다. 자리를 정의하는 곳을
        // 전부 안다는 전제가 두 번 깨졌다: 탄박스는 StackSlots 에 자리를 두고(87쌍이 없는 자리로
        // 읽혔다), 그 프로필들의 부모 템플릿 51종(1577쌍)은 정적 데이터베이스에 아예 없다(모드가
        // 기동 때 주입한다). 틀린 거절의 값은 "그 팀원은 다시 저장할 수 없다" 다.
        var verdict = JudgePlace(MovedTo("ammo", "pockets", "cartridges", 860));

        Assert.True(verdict.Allowed, verdict.Reason);
    }

    [Fact]
    public void 일반_슬롯에_둘이_앉으면_거절한다()
    {
        var after = PlaceBefore();
        after.RemoveAll(i => i.Id == "helmet" || i.Id == "helmet2");
        after.Add(Item("helmet", "tpl-helmet", Equip, "Headwear"));
        after.Add(Item("helmet2", "tpl-helmet", Equip, "Headwear"));

        var verdict = JudgePlace(after);

        Assert.False(verdict.Allowed);
        Assert.Contains("Headwear", verdict.Reason);
    }

    [Fact]
    public void 탄_자리에는_여러_더미가_앉을_수_있다()
    {
        // 탄 자리는 색인으로 담으므로 더미 여럿이 정상이다(다른 구경을 섞어 넣는 것이 플레이어 화면의
        // 동작이다). 860발에서 10발씩 둘을 떼어 넣는다 - 합 20 에 원래 있던 odd 1 까지 30 안이다.
        var after = PlaceBefore();
        after.RemoveAll(i => i.Id == "ammo");
        after.Add(Item("ammo", "tpl-ammo", Stash, "hideout", 840));
        after.Add(Item("ammo-a", "tpl-ammo", "mag", "cartridges", 10));
        after.Add(Item("ammo-b", "tpl-ammo", "mag", "cartridges", 10));

        var verdict = JudgePlace(after);

        Assert.True(verdict.Allowed, verdict.Reason);
    }

    [Fact]
    public void 탄_자리의_합이_용량을_넘으면_거절한다()
    {
        // 더미 하나하나는 30 안이지만 합이 41 이다. 한 발씩 보면 뚫리는 자리다.
        var after = PlaceBefore();
        after.RemoveAll(i => i.Id == "ammo");
        after.Add(Item("ammo", "tpl-ammo", Stash, "hideout", 820));
        after.Add(Item("ammo-a", "tpl-ammo", "mag", "cartridges", 20));
        after.Add(Item("ammo-b", "tpl-ammo", "mag", "cartridges", 20));

        var verdict = JudgePlace(after);

        Assert.False(verdict.Allowed);
        Assert.Contains("30", verdict.Reason);
    }

    [Fact]
    public void 자리가_그대로면_보지_않는다()
    {
        // odd 는 처음부터 탄 자리에 앉은 가방이다 - 우리가 만든 상태가 아니고, EFT 는 이미 그 프로필을
        // 적재하고 있다. 이것을 거절하면 그 팀원은 **아무것도** 저장할 수 없다.
        var verdict = JudgePlace(PlaceBefore());

        Assert.True(verdict.Allowed, verdict.Reason);
    }

    [Fact]
    public void 아이템_정보가_없으면_자리를_보지_않는다()
    {
        // 이 검사는 보존 검사를 **거드는** 두 번째 그물이지 혼자 서는 관문이 아니다. 데이터베이스를
        // 읽지 못하면(서버가 빈 사실을 준다) 자리는 보지 못하고 보존만 남는다.
        var after = MovedTo("bag", "mag", "cartridges");

        Assert.True(ProfileInventoryApplyPolicy.Validate(
            PlaceBefore(), after, Stash, Equip, PlaceBefore().Select(i => i.Id!).ToList(), null).Allowed);
    }

    [Fact]
    public void 부모_종류를_읽지_못하면_통과시킨다()
    {
        // tpl-mag 를 Slots·Grids 양쪽에서 빼면 그 템플릿의 자리를 하나도 모른다. 모르는 것으로
        // 거절하지 않는다 - 실제 프로필의 부모 템플릿 51종이 이 상태다(모드가 주입한 템플릿).
        var verdict = JudgePlace(MovedTo("bag", "mag", "cartridges"), PlaceFacts(dropMagTemplate: true));

        Assert.True(verdict.Allowed, verdict.Reason);
    }

    [Fact]
    public void 아이템의_분류를_모르면_종류를_판정하지_않는다()
    {
        // 슬롯 필터는 아이템 tpl 대신 **범주 노드**를 담으므로, 조상 사슬을 걷지 못하면 "받지 않는
        // 종류" 와 "모르는 종류" 가 같은 모양이 된다. 그 둘을 섞으면 **모드가 더한 아이템이 전부
        // 거부**된다 - 2026-11-04 실측: 이 PC 의 프로필에서 부모 템플릿 51종이 정적 데이터베이스에
        // 없고, 필터 거절 153쌍 중 122쌍이 이 "모르는 종류" 였다.
        var facts = PlaceFacts();
        var noAncestry = new GearSlotFacts(
            facts.TemplateIdByItemId,
            facts.SlotsByTemplateId,
            new Dictionary<string, string>(),   // 조상 사슬을 모른다
            null,
            facts.GridsByTemplateId);

        var verdict = JudgePlace(MovedTo("bag", "mag", "cartridges"), noAncestry);

        Assert.True(verdict.Allowed, verdict.Reason);
    }

    [Fact]
    public void 필터를_읽지_못한_자리는_거절하지_않는다()
    {
        // 필터가 비면 이동 목록 경로는 "모름" 으로 거부했다. 여기서는 통과시킨다 - 거절의 근거가 없다.
        var emptyFilter = new[] { new GearSlotDefinition("cartridges", Array.Empty<string>(), 30) };
        var verdict = JudgePlace(MovedTo("bag", "mag", "cartridges"), PlaceFacts(emptyFilter));

        Assert.True(verdict.Allowed, verdict.Reason);
    }

    [Fact]
    public void 용량을_읽지_못하면_수량을_보지_않는다()
    {
        // 0 은 "몇 발인지 모른다" 다(BuildSlotFacts 가 읽지 못하면 0 으로 싣는다).
        var unknownCapacity = new[] { new GearSlotDefinition("cartridges", new[] { "tpl-ammo" }, 0) };
        var after = PlaceBefore();
        after.RemoveAll(i => i.Id == "ammo");
        after.Add(Item("ammo", "tpl-ammo", "mag", "cartridges", 860));

        var verdict = JudgePlace(after, PlaceFacts(unknownCapacity));

        Assert.True(verdict.Allowed, verdict.Reason);
    }
}
