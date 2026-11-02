using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 이 판정 뒤에는 플레이어가 실제로 플레이하는 프로필에 쓰기가 일어난다. 그래서 "허용한다" 보다
// "거부한다" 쪽 테스트가 많고, 거부는 전부 닫힌 쪽이다(읽을 수 없으면 거부, 모르는 어휘면 거부).
public class ProfileGearEditPolicyTests
{
    private const string Equip = "equipmentroot0000000001";
    private const string Stash = "stashroot00000000000001";

    // 전형적인 런처 프로필 하나. 장비 루트 아래 착용 장비, 스태시 루트 아래 재고.
    private static List<WornGearItem> Profile() => new List<WornGearItem>
    {
        new WornGearItem(Equip, null, null),
        new WornGearItem("worn-rig", Equip, "TacticalVest"),
        new WornGearItem("worn-rifle", Equip, "FirstPrimaryWeapon"),
        new WornGearItem("worn-rifle-mag", "worn-rifle", "mod_magazine"),
        new WornGearItem(Stash, null, null),
        new WornGearItem("stash-helmet", Stash, "hideout"),
        new WornGearItem("stash-mag", Stash, "hideout"),
        new WornGearItem("stash-ammo", Stash, "hideout"),
    };

    private static GearEditRequest Edit(string itemId, string slot, string? parent) =>
        new GearEditRequest("profile-1", itemId, slot, parent);

    [Fact]
    public void 그_프로필_스태시에_있는_아이템만_출처로_허용한다()
    {
        var verdict = ProfileGearEditPolicy.Validate(Edit("stash-helmet", "Headwear", Equip), Profile(), Stash, Equip);

        Assert.True(verdict.Allowed);
        // 부모를 적지 않으면 장비 루트로 읽는다 - 같은 결과여야 한다.
        Assert.True(ProfileGearEditPolicy.Validate(Edit("stash-helmet", "Headwear", null), Profile(), Stash, Equip).Allowed);
    }

    [Fact]
    public void 스태시_밖의_id_는_거부한다()
    {
        // 플레이어 스태시의 아이템은 이 프로필의 아이템 목록에 아예 없다.
        var foreign = ProfileGearEditPolicy.Validate(Edit("player-stash-helmet", "Headwear", Equip), Profile(), Stash, Equip);
        Assert.False(foreign.Allowed);
        Assert.False(string.IsNullOrWhiteSpace(foreign.Reason));

        // 존재하지 않는 id.
        Assert.False(ProfileGearEditPolicy.Validate(Edit("없는아이템", "Headwear", Equip), Profile(), Stash, Equip).Allowed);

        // 스태시 루트 자신은 옮길 수 없다.
        Assert.False(ProfileGearEditPolicy.Validate(Edit(Stash, "Headwear", Equip), Profile(), Stash, Equip).Allowed);

        // 이미 장비 트리에 있는 아이템은 이 경로의 출처가 아니다(스태시가 출처다).
        Assert.False(ProfileGearEditPolicy.Validate(Edit("worn-rifle-mag", "Headwear", Equip), Profile(), Stash, Equip).Allowed);
    }

    [Fact]
    public void 스태시_루트를_읽을_수_없으면_거부한다()
    {
        // 닫힌 쪽: 루트를 추측하면 엉뚱한 트리를 출처로 읽는다.
        foreach (var root in new string?[] { null, "", "   " })
        {
            var verdict = ProfileGearEditPolicy.Validate(Edit("stash-helmet", "Headwear", Equip), Profile(), root, Equip);
            Assert.False(verdict.Allowed);
            Assert.False(string.IsNullOrWhiteSpace(verdict.Reason));
        }
    }

    [Fact]
    public void 장비_루트를_읽을_수_없으면_거부한다()
    {
        foreach (var root in new string?[] { null, "", "   " })
        {
            var verdict = ProfileGearEditPolicy.Validate(Edit("stash-helmet", "Headwear", Equip), Profile(), Stash, root);
            Assert.False(verdict.Allowed);
            Assert.False(string.IsNullOrWhiteSpace(verdict.Reason));
        }

        // 두 루트가 같으면 어느 트리가 출처이고 어느 트리가 목적지인지 가릴 수 없다.
        Assert.False(ProfileGearEditPolicy.Validate(Edit("stash-helmet", "Headwear", Stash), Profile(), Stash, Stash).Allowed);

        // 아이템 목록을 못 읽으면(널) 역시 거부다.
        Assert.False(ProfileGearEditPolicy.Validate(Edit("stash-helmet", "Headwear", Equip), null, Stash, Equip).Allowed);
    }

    [Fact]
    public void 목적_슬롯_어휘를_모르면_거부한다()
    {
        // 허용 목록이지 제외 목록이 아니다. 장비 루트 아래는 EquipmentSlots 이름만 받는다.
        foreach (var slot in new[] { "Dogtag", "SpecialSlot1", "뭔가이상한슬롯", "main", "hideout", "1" })
        {
            var verdict = ProfileGearEditPolicy.Validate(Edit("stash-helmet", slot, Equip), Profile(), Stash, Equip);
            Assert.False(verdict.Allowed);
            Assert.False(string.IsNullOrWhiteSpace(verdict.Reason));
        }

        // 빈 슬롯 이름도 거부다.
        foreach (var slot in new[] { "", "   " })
        {
            Assert.False(ProfileGearEditPolicy.Validate(Edit("stash-helmet", slot, Equip), Profile(), Stash, Equip).Allowed);
        }

        // 목적지 부모가 이 프로필의 장비 트리에 없으면 거부다(스태시 안의 가방에 넣는 길은 이 경로가 아니다).
        Assert.False(ProfileGearEditPolicy.Validate(Edit("stash-helmet", "mod_magazine", "stash-mag"), Profile(), Stash, Equip).Allowed);
        Assert.False(ProfileGearEditPolicy.Validate(Edit("stash-helmet", "mod_magazine", "없는부모"), Profile(), Stash, Equip).Allowed);
    }

    [Fact]
    public void 탄약도_다른_아이템과_같은_규칙으로_허용한다()
    {
        // 인계 문서 열린 항목 4: 탄창 리필은 팀원이 몸에 지닌 탄만 쓴다. 편집으로 탄을 넣을 수 없으면
        // 그 팀원은 빈 탄창으로 레이드에 나간다. 그래서 탄약은 특별 취급하지 않는다 - 그 프로필
        // 스태시에 있으면 장비 트리의 탄약 슬롯에 넣을 수 있다.
        Assert.True(ProfileGearEditPolicy.Validate(Edit("stash-ammo", "cartridges", "worn-rifle-mag"), Profile(), Stash, Equip).Allowed);
        Assert.True(ProfileGearEditPolicy.Validate(Edit("stash-ammo", "patron_in_weapon", "worn-rifle"), Profile(), Stash, Equip).Allowed);

        // 탄창 자체도 같은 규칙이다.
        Assert.True(ProfileGearEditPolicy.Validate(Edit("stash-mag", "mod_magazine", "worn-rifle"), Profile(), Stash, Equip).Allowed);

        // 같은 규칙이라는 말의 반대쪽: 스태시에 없는 탄은 탄이라서 봐주지 않는다.
        Assert.False(ProfileGearEditPolicy.Validate(Edit("player-ammo", "cartridges", "worn-rifle-mag"), Profile(), Stash, Equip).Allowed);
    }

    [Fact]
    public void 요청이_비면_거부한다()
    {
        Assert.False(ProfileGearEditPolicy.Validate(null!, Profile(), Stash, Equip).Allowed);
        Assert.False(ProfileGearEditPolicy.Validate(new GearEditRequest("", "stash-helmet", "Headwear", Equip), Profile(), Stash, Equip).Allowed);
        Assert.False(ProfileGearEditPolicy.Validate(new GearEditRequest("profile-1", "  ", "Headwear", Equip), Profile(), Stash, Equip).Allowed);
    }

    // ---- 보존 검사: 아무것도 사라지지 않는다 ----
    // PIT 의 실물 커밋은 플레이어 스태시 트리를 제출된 것으로 통째로 갈아 끼우고(:1277-1280),
    // 검증은 "제출된" 아이템만 훑는다(:2625) - 빠진 아이템은 아무도 찾지 않는다. 그 구멍을 메운다.

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
        Assert.False(ProfileGearEditPolicy.CheckStashConservation(null, new[] { "root" }, new[] { "e" }, null).Conserved);
        Assert.False(ProfileGearEditPolicy.CheckStashConservation(new[] { "root" }, null, new[] { "e" }, null).Conserved);
        Assert.False(ProfileGearEditPolicy.CheckStashConservation(new[] { "root" }, new[] { "root" }, null, null).Conserved);

        foreach (var verdict in new[]
                 {
                     ProfileGearEditPolicy.CheckStashConservation(null, new[] { "root" }, new[] { "e" }, null),
                     ProfileGearEditPolicy.CheckStashConservation(new[] { "root" }, null, new[] { "e" }, null),
                     ProfileGearEditPolicy.CheckStashConservation(new[] { "root" }, new[] { "root" }, null, null),
                 })
        {
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

    // ---- 여러 건을 한 번에 (쓰기 한 번) ----

    [Fact]
    public void 여러_건은_순서대로_누적해_판정한다()
    {
        // 탄창을 총에 꽂고 → 그 탄창에 탄을 넣는 연쇄. 두 번째 건의 부모(탄창)는 첫 번째 건의
        // 결과로 장비 트리에 들어와 있어야 통과한다. 인계 문서 열린 항목 4(빈 탄창)가 이 연쇄다.
        var batch = new[]
        {
            Edit("stash-mag", "mod_magazine", "worn-rifle"),
            Edit("stash-ammo", "cartridges", "stash-mag"),
        };

        Assert.True(ProfileGearEditPolicy.ValidateBatch(batch, Profile(), Stash, Equip).Allowed);
    }

    [Fact]
    public void 순서가_뒤집히면_거절한다()
    {
        // 탄을 먼저 넣으려 하면 그때 탄창은 아직 스태시에 있어 장비 트리의 부모가 아니다.
        var batch = new[]
        {
            Edit("stash-ammo", "cartridges", "stash-mag"),
            Edit("stash-mag", "mod_magazine", "worn-rifle"),
        };

        var verdict = ProfileGearEditPolicy.ValidateBatch(batch, Profile(), Stash, Equip);
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

        var verdict = ProfileGearEditPolicy.ValidateBatch(batch, Profile(), Stash, Equip);
        Assert.False(verdict.Allowed);
        // 거절 사유는 **실패한 그 건**의 사유다 - 어느 건이 왜 막혔는지 플레이어가 알아야 한다.
        Assert.Equal(
            ProfileGearEditPolicy.Validate(Edit("stash-mag", "뭔가이상한슬롯", "worn-rifle"), Profile(), Stash, Equip).Reason,
            verdict.Reason);
    }

    [Fact]
    public void 같은_아이템을_두_번_옮기려_하면_거절한다()
    {
        // 두 번째 건의 시점에 그 아이템은 이미 스태시 밖이다(출처가 아니다).
        var batch = new[]
        {
            Edit("stash-helmet", "Headwear", Equip),
            Edit("stash-helmet", "Eyewear", Equip),
        };

        Assert.False(ProfileGearEditPolicy.ValidateBatch(batch, Profile(), Stash, Equip).Allowed);
    }

    [Fact]
    public void 빈_묶음과_읽을_수_없는_입력은_거절한다()
    {
        foreach (var verdict in new[]
                 {
                     ProfileGearEditPolicy.ValidateBatch(null, Profile(), Stash, Equip),
                     ProfileGearEditPolicy.ValidateBatch(Array.Empty<GearEditRequest>(), Profile(), Stash, Equip),
                     ProfileGearEditPolicy.ValidateBatch(new[] { Edit("stash-helmet", "Headwear", Equip) }, null, Stash, Equip),
                     ProfileGearEditPolicy.ValidateBatch(new[] { Edit("stash-helmet", "Headwear", Equip) }, Profile(), Stash, null),
                     ProfileGearEditPolicy.ValidateBatch(new[] { Edit("stash-helmet", "Headwear", Equip) }, Profile(), null, Equip),
                     ProfileGearEditPolicy.ValidateBatch(new GearEditRequest[] { null! }, Profile(), Stash, Equip),
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
            new GearEditRequest("profile-2", "stash-mag", "mod_magazine", "worn-rifle"),
        };

        var verdict = ProfileGearEditPolicy.ValidateBatch(batch, Profile(), Stash, Equip);
        Assert.False(verdict.Allowed);
        Assert.Contains("여러 프로필", verdict.Reason);
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
