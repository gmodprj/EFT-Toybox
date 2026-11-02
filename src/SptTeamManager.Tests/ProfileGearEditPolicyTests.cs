using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 이 판정 뒤에는 플레이어가 실제로 플레이하는 프로필에 쓰기가 일어난다. 그래서 "허용한다" 보다
// "거부한다" 쪽 테스트가 많고, 거부는 전부 닫힌 쪽이다(읽을 수 없으면 거부, 모르는 어휘면 거부,
// 증명되지 않은 자리면 거부).
public class ProfileGearEditPolicyTests
{
    private const string Equip = "equipmentroot0000000001";
    private const string Stash = "stashroot00000000000001";

    // 전형적인 런처 프로필 하나. 장비 루트 아래 착용 장비, 스태시 루트 아래 재고.
    // 라이플의 탄창 슬롯은 **차 있고**(worn-rifle-mag), 권총의 탄창 슬롯은 비어 있다 - 점유 검사의
    // 양쪽을 한 프로필 안에서 다 보려는 것이다.
    private static List<WornGearItem> Profile() => new List<WornGearItem>
    {
        new WornGearItem(Equip, null, null),
        new WornGearItem("worn-rig", Equip, "TacticalVest"),
        new WornGearItem("worn-rifle", Equip, "FirstPrimaryWeapon"),
        new WornGearItem("worn-rifle-mag", "worn-rifle", "mod_magazine"),
        new WornGearItem("worn-pistol", Equip, "Holster"),
        new WornGearItem(Stash, null, null),
        new WornGearItem("stash-helmet", Stash, "hideout"),
        new WornGearItem("stash-helmet2", Stash, "hideout"),
        new WornGearItem("stash-mag", Stash, "hideout"),
        new WornGearItem("stash-ammo", Stash, "hideout"),
    };

    // 아이템 데이터베이스에서 올 사실을 손으로 적은 것. 서버는 ItemHelper.GetItem 으로 같은 모양을 만든다.
    private static GearSlotFacts Facts()
    {
        var templates = new Dictionary<string, string>
        {
            [Equip] = "tpl-equip-root",
            ["worn-rig"] = "tpl-rig",
            ["worn-rifle"] = "tpl-rifle",
            ["worn-rifle-mag"] = "tpl-mag",
            ["worn-pistol"] = "tpl-pistol",
            [Stash] = "tpl-stash",
            ["stash-helmet"] = "tpl-helmet",
            ["stash-helmet2"] = "tpl-helmet",
            ["stash-mag"] = "tpl-mag",
            ["stash-ammo"] = "tpl-ammo",
        };

        var slots = new Dictionary<string, IReadOnlyList<GearSlotDefinition>>
        {
            ["tpl-equip-root"] = new[]
            {
                new GearSlotDefinition("Headwear", new[] { "tpl-helmet" }),
                new GearSlotDefinition("TacticalVest", new[] { "tpl-rig" }),
                new GearSlotDefinition("FirstPrimaryWeapon", new[] { "tpl-rifle" }),
                new GearSlotDefinition("Holster", new[] { "tpl-pistol" }),
            },
            ["tpl-rifle"] = new[]
            {
                new GearSlotDefinition("mod_magazine", new[] { "tpl-mag" }),
                new GearSlotDefinition("patron_in_weapon", new[] { "tpl-ammo" }),
                // 필터를 읽지 못한 슬롯. "제한 없음" 이 아니라 "모름" 이다.
                new GearSlotDefinition("mod_mount", Array.Empty<string>()),
            },
            ["tpl-pistol"] = new[] { new GearSlotDefinition("mod_magazine", new[] { "tpl-mag" }) },
            ["tpl-mag"] = new[] { new GearSlotDefinition("cartridges", new[] { "tpl-ammo" }) },
            // 리그는 격자만 있고 슬롯이 없다.
            ["tpl-rig"] = Array.Empty<GearSlotDefinition>(),
            ["tpl-stash"] = Array.Empty<GearSlotDefinition>(),
            ["tpl-helmet"] = Array.Empty<GearSlotDefinition>(),
            ["tpl-ammo"] = Array.Empty<GearSlotDefinition>(),
        };

        return new GearSlotFacts(templates, slots);
    }

    private static GearEditRequest Edit(string itemId, string slot, string? parent) =>
        new GearEditRequest("profile-1", itemId, slot, parent);

    private static GearEditVerdict Judge(GearEditRequest request) =>
        ProfileGearEditPolicy.Validate(request, Profile(), Stash, Equip, Facts());

    [Fact]
    public void 그_프로필_스태시에_있는_아이템만_출처로_허용한다()
    {
        Assert.True(Judge(Edit("stash-helmet", "Headwear", Equip)).Allowed);
        // 부모를 적지 않으면 장비 루트로 읽는다 - 같은 결과여야 한다.
        Assert.True(Judge(Edit("stash-helmet", "Headwear", null)).Allowed);
    }

    [Fact]
    public void 스태시_밖의_id_는_거부한다()
    {
        // 플레이어 스태시의 아이템은 이 프로필의 아이템 목록에 아예 없다.
        var foreign = Judge(Edit("player-stash-helmet", "Headwear", Equip));
        Assert.False(foreign.Allowed);
        Assert.False(string.IsNullOrWhiteSpace(foreign.Reason));

        // 존재하지 않는 id.
        Assert.False(Judge(Edit("없는아이템", "Headwear", Equip)).Allowed);

        // 스태시 루트 자신은 옮길 수 없다.
        Assert.False(Judge(Edit(Stash, "Headwear", Equip)).Allowed);

        // 이미 장비 트리에 있는 아이템은 이 경로의 출처가 아니다(스태시가 출처다).
        Assert.False(Judge(Edit("worn-rifle-mag", "Headwear", Equip)).Allowed);
    }

    [Fact]
    public void 스태시_루트를_읽을_수_없으면_거부한다()
    {
        // 닫힌 쪽: 루트를 추측하면 엉뚱한 트리를 출처로 읽는다.
        foreach (var root in new string?[] { null, "", "   " })
        {
            var verdict = ProfileGearEditPolicy.Validate(Edit("stash-helmet", "Headwear", Equip), Profile(), root, Equip, Facts());
            Assert.False(verdict.Allowed);
            Assert.False(string.IsNullOrWhiteSpace(verdict.Reason));
        }
    }

    [Fact]
    public void 장비_루트를_읽을_수_없으면_거부한다()
    {
        foreach (var root in new string?[] { null, "", "   " })
        {
            var verdict = ProfileGearEditPolicy.Validate(Edit("stash-helmet", "Headwear", Equip), Profile(), Stash, root, Facts());
            Assert.False(verdict.Allowed);
            Assert.False(string.IsNullOrWhiteSpace(verdict.Reason));
        }

        // 두 루트가 같으면 어느 트리가 출처이고 어느 트리가 목적지인지 가릴 수 없다.
        Assert.False(ProfileGearEditPolicy.Validate(Edit("stash-helmet", "Headwear", Stash), Profile(), Stash, Stash, Facts()).Allowed);

        // 아이템 목록을 못 읽으면(널) 역시 거부다.
        Assert.False(ProfileGearEditPolicy.Validate(Edit("stash-helmet", "Headwear", Equip), null, Stash, Equip, Facts()).Allowed);
    }

    [Fact]
    public void 목적_슬롯_어휘를_모르면_거부한다()
    {
        // 허용 목록이지 제외 목록이 아니다. 장비 루트 아래는 EquipmentSlots 이름만 받는다.
        foreach (var slot in new[] { "Dogtag", "SpecialSlot1", "뭔가이상한슬롯", "main", "hideout", "1" })
        {
            var verdict = Judge(Edit("stash-helmet", slot, Equip));
            Assert.False(verdict.Allowed);
            Assert.False(string.IsNullOrWhiteSpace(verdict.Reason));
        }

        // 빈 슬롯 이름도 거부다.
        foreach (var slot in new[] { "", "   " })
        {
            Assert.False(Judge(Edit("stash-helmet", slot, Equip)).Allowed);
        }

        // 목적지 부모가 이 프로필의 장비 트리에 없으면 거부다(스태시 안 가방에 넣는 길은 이 경로가 아니다).
        Assert.False(Judge(Edit("stash-helmet", "mod_magazine", "stash-mag")).Allowed);
        Assert.False(Judge(Edit("stash-helmet", "mod_magazine", "없는부모")).Allowed);
    }

    [Fact]
    public void 탄약도_다른_아이템과_같은_규칙으로_허용한다()
    {
        // 인계 문서 열린 항목 4: 탄창 리필은 팀원이 몸에 지닌 탄만 쓴다. 편집으로 탄을 넣을 수 없으면
        // 그 팀원은 빈 탄창으로 레이드에 나간다. 그래서 탄약은 특별 취급하지 않는다 - 그 프로필
        // 스태시에 있으면 장비 트리의 탄약 슬롯에 넣을 수 있다.
        Assert.True(Judge(Edit("stash-ammo", "cartridges", "worn-rifle-mag")).Allowed);
        Assert.True(Judge(Edit("stash-ammo", "patron_in_weapon", "worn-rifle")).Allowed);

        // 탄창 자체도 같은 규칙이다(권총의 탄창 슬롯은 비어 있다).
        Assert.True(Judge(Edit("stash-mag", "mod_magazine", "worn-pistol")).Allowed);

        // 같은 규칙이라는 말의 반대쪽: 스태시에 없는 탄은 탄이라서 봐주지 않는다.
        Assert.False(Judge(Edit("player-ammo", "cartridges", "worn-rifle-mag")).Allowed);
    }

    [Fact]
    public void 요청이_비면_거부한다()
    {
        Assert.False(ProfileGearEditPolicy.Validate(null!, Profile(), Stash, Equip, Facts()).Allowed);
        Assert.False(Judge(new GearEditRequest("", "stash-helmet", "Headwear", Equip)).Allowed);
        Assert.False(Judge(new GearEditRequest("profile-1", "  ", "Headwear", Equip)).Allowed);
    }

    // ---- 벗기기: 그 팀원 장비 → 그 팀원 **자신의** 스태시 ----
    // §6-4 의 "출처는 본인 스태시뿐" 은 새 아이템이 어디서 오는가의 규칙이고, 그 팀원 자신의 장비가
    // 자기 스태시로 돌아가는 것을 막는 규칙이 아니다. 한 프로필 안의 이동이고 공짜 생성도 없다.

    [Fact]
    public void 착용한_것을_그_팀원_자신의_스태시로_벗긴다()
    {
        // 장비 루트의 직접 자식(착용 장비)
        Assert.True(Judge(Edit("worn-rifle", "hideout", Stash)).Allowed);
        // 장비 트리 안쪽의 장착물(총에 꽂힌 탄창)도 같다.
        Assert.True(Judge(Edit("worn-rifle-mag", "hideout", Stash)).Allowed);
    }

    [Fact]
    public void 어느_트리에도_없는_아이템은_벗길_수_없다()
    {
        var verdict = Judge(Edit("없는아이템", "hideout", Stash));
        Assert.False(verdict.Allowed);
        Assert.False(string.IsNullOrWhiteSpace(verdict.Reason));

        // 플레이어 스태시의 것도 이 목록에 없다 - 벗기기로도 들어올 길이 없다.
        Assert.False(Judge(Edit("player-stash-helmet", "hideout", Stash)).Allowed);

        // 이미 스태시에 있는 것은 벗길 것이 없다(착용·장착한 것이 아니다).
        Assert.False(Judge(Edit("stash-helmet", "hideout", Stash)).Allowed);
    }

    [Fact]
    public void 팀원_자신의_스태시가_아닌_곳으로는_벗길_수_없다()
    {
        // 스태시 **안의** 가방 칸은 목적지가 아니다(§6-5 가 미뤘다). 스태시 루트만 받는다.
        Assert.False(Judge(Edit("worn-rifle-mag", "hideout", "stash-mag")).Allowed);

        // 장비 트리 안의 용기로 옮기는 것도 이 경로가 아니다 - 'hideout' 은 장비 슬롯 어휘가 아니다.
        Assert.False(Judge(Edit("worn-rifle-mag", "hideout", "worn-rig")).Allowed);

        // 장비 루트 자체를 스태시로 옮기면 그 프로필의 장비 트리가 사라진다.
        var root = Judge(Edit(Equip, "hideout", Stash));
        Assert.False(root.Allowed);
        Assert.Contains("장비 루트", root.Reason);

        // 존재하지 않는 부모도 거부다(추측하지 않는다).
        Assert.False(Judge(Edit("worn-rifle-mag", "hideout", "없는부모")).Allowed);
    }

    [Fact]
    public void 벗기기도_스태시_칸_이름을_허용_목록으로_받는다()
    {
        foreach (var slot in new[] { "main", "1", "Headwear", "mod_magazine", "뭔가이상한칸" })
        {
            var verdict = Judge(Edit("worn-rifle-mag", slot, Stash));
            Assert.False(verdict.Allowed);
            Assert.False(string.IsNullOrWhiteSpace(verdict.Reason));
        }

        // 허용 목록은 비어 있지 않다 - 어휘가 비면 벗기기 자체가 불가능해진다.
        Assert.NotEmpty(ProfileGearEditPolicy.StashSlotNames);
    }

    [Fact]
    public void 벗기고_다른_것을_그_자리에_넣는_묶음이_통과한다()
    {
        // 라이플의 탄창 슬롯은 차 있다. 벗긴 뒤에는 그 자리에 다른 탄창을 넣을 수 있다 -
        // 점유 판정이 누적되기 때문이다. 부분 장전 탄창의 막다른 길이 이렇게 풀린다.
        var batch = new[]
        {
            Edit("worn-rifle-mag", "hideout", Stash),
            Edit("stash-mag", "mod_magazine", "worn-rifle"),
        };

        Assert.True(ProfileGearEditPolicy.ValidateBatch(batch, Profile(), Stash, Equip, Facts()).Allowed);

        // 순서를 뒤집으면 그때 슬롯은 아직 차 있다.
        var reversed = new[]
        {
            Edit("stash-mag", "mod_magazine", "worn-rifle"),
            Edit("worn-rifle-mag", "hideout", Stash),
        };
        Assert.False(ProfileGearEditPolicy.ValidateBatch(reversed, Profile(), Stash, Equip, Facts()).Allowed);
    }

    [Fact]
    public void 벗기기는_아이템_집합을_바꾸지_않으므로_대조를_통과한다()
    {
        // 벗기기는 부모와 슬롯만 바꾼다. 집합이 그대로이므로 디스크 재독 대조가 통과한다.
        var before = new[] { Stash, Equip, "worn-rifle", "worn-rifle-mag" };
        var after = new[] { Stash, Equip, "worn-rifle", "worn-rifle-mag" };
        Assert.True(ProfileGearEditPolicy.CheckItemSetUnchanged(before, after).Conserved);

        // PIT 경로의 보존 검사도 같다: 팀원 쪽으로 간 것이 아니라 스태시에 남아도 보존이다.
        var conservation = ProfileGearEditPolicy.CheckStashConservation(
            new[] { "root", "helmet" },
            new[] { "root", "helmet" },
            new[] { "teammate-equip" },
            null);
        Assert.True(conservation.Conserved);
    }

    // ---- 넣을 자리가 실제로 맞는가 (어휘만으로는 부족하다) ----
    // 집합은 보존되고 트리도 멀쩡한데 EFT 가 적재를 거부할 수 있는 프로필이 만들어지는 경우들.

    [Fact]
    public void 이미_찬_슬롯에는_넣지_않는다()
    {
        // 라이플의 탄창 슬롯에는 worn-rifle-mag 가 이미 있다.
        var verdict = Judge(Edit("stash-mag", "mod_magazine", "worn-rifle"));
        Assert.False(verdict.Allowed);
        Assert.Contains("worn-rifle-mag", verdict.Reason);

        // 묶음 안에서도 같다: 같은 Headwear 에 두 아이템을 앉히지 못한다.
        var batch = new[]
        {
            Edit("stash-helmet", "Headwear", Equip),
            Edit("stash-helmet2", "Headwear", Equip),
        };
        Assert.False(ProfileGearEditPolicy.ValidateBatch(batch, Profile(), Stash, Equip, Facts()).Allowed);
    }

    [Fact]
    public void 부모_템플릿에_없는_슬롯이면_거부한다()
    {
        // 어휘로는 아는 이름(mod_ 접두)이지만 리그에는 그런 슬롯이 없다.
        var verdict = Judge(Edit("stash-mag", "mod_magazine", "worn-rig"));
        Assert.False(verdict.Allowed);
        Assert.Contains("mod_magazine", verdict.Reason);
    }

    [Fact]
    public void 슬롯이_허용하지_않는_아이템은_거부한다()
    {
        // 약실에 헬멧을 넣을 수는 없다. 슬롯 이름도 맞고 자리도 비었지만 필터가 받지 않는다.
        var verdict = Judge(Edit("stash-helmet", "patron_in_weapon", "worn-rifle"));
        Assert.False(verdict.Allowed);
        Assert.False(string.IsNullOrWhiteSpace(verdict.Reason));

        // 같은 슬롯에 탄은 들어간다 - 필터를 "전부 거부" 로 읽은 것이 아니라는 반대쪽.
        Assert.True(Judge(Edit("stash-ammo", "patron_in_weapon", "worn-rifle")).Allowed);
    }

    [Fact]
    public void 아이템_정보를_읽을_수_없으면_거부한다()
    {
        var request = Edit("stash-helmet", "Headwear", Equip);

        // 사실 자체가 없다.
        Assert.False(ProfileGearEditPolicy.Validate(request, Profile(), Stash, Equip, null).Allowed);

        // 부모의 종류를 모른다.
        var noParentTemplate = new GearSlotFacts(
            new Dictionary<string, string> { ["stash-helmet"] = "tpl-helmet" },
            new Dictionary<string, IReadOnlyList<GearSlotDefinition>>());
        Assert.False(ProfileGearEditPolicy.Validate(request, Profile(), Stash, Equip, noParentTemplate).Allowed);

        // 부모의 슬롯 목록을 모른다.
        var noSlots = new GearSlotFacts(
            new Dictionary<string, string> { [Equip] = "tpl-equip-root", ["stash-helmet"] = "tpl-helmet" },
            new Dictionary<string, IReadOnlyList<GearSlotDefinition>>());
        Assert.False(ProfileGearEditPolicy.Validate(request, Profile(), Stash, Equip, noSlots).Allowed);

        // 옮길 아이템의 종류를 모른다.
        var noItemTemplate = new GearSlotFacts(
            new Dictionary<string, string> { [Equip] = "tpl-equip-root" },
            Facts().SlotsByTemplateId);
        Assert.False(ProfileGearEditPolicy.Validate(request, Profile(), Stash, Equip, noItemTemplate).Allowed);

        // 슬롯의 필터가 비어 있다 = "제한 없음" 이 아니라 "모름". 이것도 거부다.
        Assert.False(Judge(Edit("stash-mag", "mod_mount", "worn-rifle")).Allowed);
    }

    [Fact]
    public void 장비_루트_슬롯_어휘는_SpawnGearPolicy_와_같다()
    {
        // 두 어휘가 조용히 갈라지면, 한쪽이 "착용 장비" 로 세지 않는 슬롯에 편집이 아이템을 넣을 수
        // 있다 - 그러면 장비를 갖춘 팀원이 임시 장비로 나간다(또는 그 반대). WornGearLossPolicyTests
        // 가 같은 방식으로 지키는 약속이다.
        foreach (var slot in ProfileGearEditPolicy.EquipmentRootSlotNames)
        {
            Assert.True(
                SpawnGearPolicy.KnownSlotNames.Contains(slot, StringComparer.OrdinalIgnoreCase),
                $"'{slot}' 은 SpawnGearPolicy 가 아는 슬롯이 아니다. 모르는 슬롯에 넣으면 안 된다.");
        }

        foreach (var slot in SpawnGearPolicy.KnownSlotNames)
        {
            Assert.True(
                ProfileGearEditPolicy.EquipmentRootSlotNames.Contains(slot, StringComparer.OrdinalIgnoreCase),
                $"'{slot}' 은 SpawnGearPolicy 가 장비로 세는데 편집은 모른다. 플레이어가 채울 수 없는 슬롯이 된다.");
        }
    }

    // ---- 여러 건을 한 번에 (쓰기 한 번) ----

    [Fact]
    public void 여러_건은_순서대로_누적해_판정한다()
    {
        // 탄창을 권총에 꽂고 → 그 탄창에 탄을 넣는 연쇄. 두 번째 건의 부모(탄창)는 첫 번째 건의
        // 결과로 장비 트리에 들어와 있어야 통과한다. 인계 문서 열린 항목 4(빈 탄창)가 이 연쇄다.
        var batch = new[]
        {
            Edit("stash-mag", "mod_magazine", "worn-pistol"),
            Edit("stash-ammo", "cartridges", "stash-mag"),
        };

        Assert.True(ProfileGearEditPolicy.ValidateBatch(batch, Profile(), Stash, Equip, Facts()).Allowed);
    }

    [Fact]
    public void 순서가_뒤집히면_거절한다()
    {
        // 탄을 먼저 넣으려 하면 그때 탄창은 아직 스태시에 있어 장비 트리의 부모가 아니다.
        var batch = new[]
        {
            Edit("stash-ammo", "cartridges", "stash-mag"),
            Edit("stash-mag", "mod_magazine", "worn-pistol"),
        };

        var verdict = ProfileGearEditPolicy.ValidateBatch(batch, Profile(), Stash, Equip, Facts());
        Assert.False(verdict.Allowed);
        Assert.False(string.IsNullOrWhiteSpace(verdict.Reason));
    }

    [Fact]
    public void 한_건이라도_거절되면_전체를_거절한다()
    {
        // 첫 건은 혼자서는 통과하지만, 두 번째가 거절되면 전체가 거절이다 - 반쯤 입은 상태를 만들지 않는다.
        var batch = new[]
        {
            Edit("stash-helmet", "Headwear", Equip),
            Edit("stash-mag", "뭔가이상한슬롯", "worn-rifle"),
        };

        var verdict = ProfileGearEditPolicy.ValidateBatch(batch, Profile(), Stash, Equip, Facts());
        Assert.False(verdict.Allowed);
        // 거절 사유는 **실패한 그 건**의 사유다 - 어느 건이 왜 막혔는지 플레이어가 알아야 한다.
        Assert.Equal(Judge(Edit("stash-mag", "뭔가이상한슬롯", "worn-rifle")).Reason, verdict.Reason);
    }

    [Fact]
    public void 같은_아이템을_두_번_옮기려_하면_거절한다()
    {
        // 두 번째 건의 시점에 그 아이템은 이미 스태시 밖이다(출처가 아니다).
        var batch = new[]
        {
            Edit("stash-helmet", "Headwear", Equip),
            Edit("stash-helmet", "Headwear", Equip),
        };

        Assert.False(ProfileGearEditPolicy.ValidateBatch(batch, Profile(), Stash, Equip, Facts()).Allowed);
    }

    [Fact]
    public void 빈_묶음과_읽을_수_없는_입력은_거절한다()
    {
        var one = new[] { Edit("stash-helmet", "Headwear", Equip) };
        foreach (var verdict in new[]
                 {
                     ProfileGearEditPolicy.ValidateBatch(null, Profile(), Stash, Equip, Facts()),
                     ProfileGearEditPolicy.ValidateBatch(Array.Empty<GearEditRequest>(), Profile(), Stash, Equip, Facts()),
                     ProfileGearEditPolicy.ValidateBatch(one, null, Stash, Equip, Facts()),
                     ProfileGearEditPolicy.ValidateBatch(one, Profile(), Stash, null, Facts()),
                     ProfileGearEditPolicy.ValidateBatch(one, Profile(), null, Equip, Facts()),
                     ProfileGearEditPolicy.ValidateBatch(one, Profile(), Stash, Equip, null),
                     ProfileGearEditPolicy.ValidateBatch(new GearEditRequest[] { null! }, Profile(), Stash, Equip, Facts()),
                 })
        {
            Assert.False(verdict.Allowed);
            Assert.False(string.IsNullOrWhiteSpace(verdict.Reason));
        }
    }

    [Fact]
    public void 한_번의_요청은_한_프로필만_고친다()
    {
        var batch = new[]
        {
            new GearEditRequest("profile-1", "stash-helmet", "Headwear", Equip),
            new GearEditRequest("profile-2", "stash-mag", "mod_magazine", "worn-pistol"),
        };

        var verdict = ProfileGearEditPolicy.ValidateBatch(batch, Profile(), Stash, Equip, Facts());
        Assert.False(verdict.Allowed);
        Assert.Contains("여러 프로필", verdict.Reason);
    }

    // ---- 보존 검사: 아무것도 사라지지 않는다 ----
    // PIT 의 실물 커밋은 플레이어 스태시 트리를 제출된 것으로 통째로 갈아 끼우고(:1277-1280),
    // 검증은 "제출된" 아이템만 훑는다(:2625) - 빠진 항목은 아무도 찾지 않는다. 그 구멍을 메운다.

    [Fact]
    public void 원래_스태시의_모든_id_가_어느_한쪽에_있으면_보존이다()
    {
        // helmet 은 팀원 장비로 옮겨졌고 나머지는 스태시에 남았다 - 정상적인 편집이다.
        var verdict = ProfileGearEditPolicy.CheckStashConservation(
            new[] { "root", "helmet", "mag", "ammo" },
            new[] { "root", "mag", "ammo" },
            new[] { "teammate-equip", "helmet" },
            null);

        Assert.True(verdict.Conserved);
        Assert.Empty(verdict.MissingItemIds);
    }

    [Fact]
    public void 사라진_id_가_있으면_거부하고_무엇이_사라졌는지_말한다()
    {
        var verdict = ProfileGearEditPolicy.CheckStashConservation(
            new[] { "root", "helmet", "mag", "ammo" },
            new[] { "root", "mag" },
            new[] { "teammate-equip" },
            null);

        Assert.False(verdict.Conserved);
        Assert.Equal(new[] { "helmet", "ammo" }, verdict.MissingItemIds);
        Assert.Contains("helmet", verdict.Reason);
        Assert.Contains("ammo", verdict.Reason);
    }

    [Fact]
    public void 보안_컨테이너로_제출한_아이템이_병합에서_빠지면_거부한다()
    {
        // PIT 의 MergeEquipmentWithPreservedSpecialItems 는 비(非)extreme 모드에서 제출된 보안
        // 컨테이너 트리를 버린다(RemoveSecureContainerTree, FriendlyTeammateService.cs:4823-4825).
        // 그래서 보존 검사는 **제출된** 장비가 아니라 **병합 뒤 저장될** 집합을 목적지로 봐야 한다.
        var original = new[] { "root", "secure-loot" };
        var submittedStash = new[] { "root" };                        // 아이템이 스태시를 떠났다
        var submittedEquipment = new[] { "t-equip", "secure-loot" };  // 제출에는 들어 있다
        var mergedEquipment = new[] { "t-equip" };                    // 병합이 버렸다 = 저장될 집합

        // 저장될 집합으로 보면 사라지는 것이 보인다 → 거부.
        var stored = ProfileGearEditPolicy.CheckStashConservation(original, submittedStash, mergedEquipment, null);
        Assert.False(stored.Conserved);
        Assert.Contains("secure-loot", stored.MissingItemIds);

        // 제출된 집합으로 보면 통과해 버린다 - 호출부가 병합 뒤 집합을 넘겨야 하는 이유를 못박는다.
        var submitted = ProfileGearEditPolicy.CheckStashConservation(original, submittedStash, submittedEquipment, null);
        Assert.True(submitted.Conserved);
    }

    [Fact]
    public void 다시_매긴_id_로_팀원_장비에_있으면_보존이다()
    {
        // PIT 는 팀원 아이템 id 가 플레이어 착용 장비와 겹치면 새 id 를 매긴다
        // (RemapReplacementEquipmentPlayerEquippedIdCollisions, :2750). 재매김된 id 로 보여도 사라진 것이 아니다.
        var verdict = ProfileGearEditPolicy.CheckStashConservation(
            new[] { "root", "helmet" },
            new[] { "root" },
            new[] { "teammate-equip", "helmet-remapped" },
            new Dictionary<string, string> { ["helmet"] = "helmet-remapped" });

        Assert.True(verdict.Conserved);

        // 재매김 기록이 있는데 새 id 도 어디에도 없으면 사라진 것이다.
        var lost = ProfileGearEditPolicy.CheckStashConservation(
            new[] { "root", "helmet" },
            new[] { "root" },
            new[] { "teammate-equip" },
            new Dictionary<string, string> { ["helmet"] = "helmet-remapped" });
        Assert.False(lost.Conserved);
    }

    [Fact]
    public void 어느_목록이든_읽을_수_없으면_보존을_확인하지_않는다()
    {
        // 닫힌 쪽: 셋 중 하나라도 널이면 "사라지지 않았다" 고 말할 근거가 없다.
        foreach (var verdict in new[]
                 {
                     ProfileGearEditPolicy.CheckStashConservation(null, new[] { "root" }, new[] { "e" }, null),
                     ProfileGearEditPolicy.CheckStashConservation(new[] { "root" }, null, new[] { "e" }, null),
                     ProfileGearEditPolicy.CheckStashConservation(new[] { "root" }, new[] { "root" }, null, null),
                 })
        {
            Assert.False(verdict.Conserved);
            Assert.False(string.IsNullOrWhiteSpace(verdict.Reason));
        }
    }

    [Fact]
    public void 아이템_id_의_공백과_대소문자를_견딘다()
    {
        // PIT 는 아이템 id 를 전부 대소문자 무시로 비교한다. 여기서 더 엄격하면 있는 아이템을
        // "사라졌다" 고 말해 정상적인 편집을 막는다.
        var verdict = ProfileGearEditPolicy.CheckStashConservation(
            new[] { " root ", "HELMET" },
            new[] { "root" },
            new[] { "helmet " },
            null);

        Assert.True(verdict.Conserved);
    }

    // ---- 디스크 재독 대조의 집합 부분 (§6-8 3단계) ----
    // 한 프로필 안의 이동이므로 편집 전후의 아이템 집합은 정확히 같아야 한다. 저널을 쓰지 않는 대신
    // 이 대조가 "성공했다고 말할 근거" 전부다.

    [Fact]
    public void 편집_전후의_아이템_집합이_같으면_대조를_통과한다()
    {
        var verdict = ProfileGearEditPolicy.CheckItemSetUnchanged(
            new[] { "root", "helmet", "mag" },
            new[] { "helmet", "root", "mag" });

        Assert.True(verdict.Conserved);
        Assert.Empty(verdict.MissingItemIds);
    }

    [Fact]
    public void 아이템이_사라지면_대조가_실패하고_무엇이_사라졌는지_말한다()
    {
        var verdict = ProfileGearEditPolicy.CheckItemSetUnchanged(
            new[] { "root", "helmet", "mag" },
            new[] { "root", "mag" });

        Assert.False(verdict.Conserved);
        Assert.Contains("helmet", verdict.Reason);
        Assert.Contains("helmet", verdict.MissingItemIds);
    }

    [Fact]
    public void 없던_아이템이_끼어들면_대조가_실패한다()
    {
        // 사라진 쪽만 보면 공짜 생성을 놓친다 - 양방향으로 본다.
        var verdict = ProfileGearEditPolicy.CheckItemSetUnchanged(
            new[] { "root", "helmet" },
            new[] { "root", "helmet", "어디서온것" });

        Assert.False(verdict.Conserved);
        Assert.Contains("어디서온것", verdict.Reason);
        Assert.Contains("어디서온것", verdict.MissingItemIds);
    }

    [Fact]
    public void 어느_쪽이든_읽을_수_없으면_대조를_통과시키지_않는다()
    {
        foreach (var verdict in new[]
                 {
                     ProfileGearEditPolicy.CheckItemSetUnchanged(null, new[] { "root" }),
                     ProfileGearEditPolicy.CheckItemSetUnchanged(new[] { "root" }, null),
                     ProfileGearEditPolicy.CheckItemSetUnchanged(null, null),
                 })
        {
            Assert.False(verdict.Conserved);
            Assert.False(string.IsNullOrWhiteSpace(verdict.Reason));
        }
    }

    [Fact]
    public void 빈_id_는_보존_판정에서_세지_않는다()
    {
        // 널·공백 id 는 아이템을 가리키지 못하므로 "사라졌다" 의 대상이 아니다.
        var verdict = ProfileGearEditPolicy.CheckStashConservation(
            new[] { "root", null!, "   " },
            new[] { "root" },
            new[] { "e" },
            null);

        Assert.True(verdict.Conserved);
        Assert.Empty(verdict.MissingItemIds);
    }
}
