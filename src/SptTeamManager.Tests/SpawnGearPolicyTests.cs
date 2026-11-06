using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

public class SpawnGearPolicyTests
{
    [Fact]
    public void 착용_장비가_없으면_Temporary_다()
    {
        // 무기 슬롯 판정이 어떻든 장비가 없으면 임시다.
        Assert.Equal(SpawnGearKind.Temporary, SpawnGearPolicy.Decide(false, false).Kind);
        Assert.Equal(SpawnGearKind.Temporary, SpawnGearPolicy.Decide(false, true).Kind);
    }

    [Fact]
    public void 무기_슬롯이_비면_장비가_있어도_Temporary_다()
    {
        Assert.Equal(SpawnGearKind.Temporary, SpawnGearPolicy.Decide(true, false).Kind);
    }

    [Fact]
    public void 장비와_무기가_있으면_Real_이다()
    {
        Assert.Equal(SpawnGearKind.Real, SpawnGearPolicy.Decide(true, true).Kind);
    }

    [Fact]
    public void 손으로_고친_팀원은_비어_있어도_임시가_아니다()
    {
        // 설계 §6-4 "편집하면 실물이 된다": 손으로 한 번 고치면 그 팀원은 그 뒤로 임시 장비 대상이
        // 아니다. 편집이 슬롯을 비워 놓고 끝났어도 그렇다 - 비웠다는 것도 플레이어의 결정이고,
        // 임시 장비를 덮어씌우면 그 결정이 조용히 사라진다. 되돌리려면 등록을 빼고 다시 넣는다.
        foreach (var hasWeapon in new[] { false, true })
        {
            var decision = SpawnGearPolicy.Decide(hasAnyEquipment: false, hasWeaponInRequiredSlot: hasWeapon, handEdited: true);
            Assert.Equal(SpawnGearKind.Real, decision.Kind);
            Assert.False(string.IsNullOrWhiteSpace(decision.Reason));
            // 사유가 "장비가 하나도 없어 임시" 와 같아서는 안 된다 - 로그에서 원인을 가려야 한다.
            Assert.NotEqual(SpawnGearPolicy.Decide(false, hasWeapon).Reason, decision.Reason);
        }

        // 손으로 고치고 장비도 갖춘 팀원은 평소와 같은 Real 이다.
        Assert.Equal(SpawnGearKind.Real, SpawnGearPolicy.Decide(true, true, handEdited: true).Kind);

        // 손으로 고치지 않은 팀원의 판정은 그대로다(기존 두 인자 오버로드와 같다).
        foreach (var (equip, weapon) in new[] { (false, false), (false, true), (true, false), (true, true) })
        {
            Assert.Equal(
                SpawnGearPolicy.Decide(equip, weapon).Kind,
                SpawnGearPolicy.Decide(equip, weapon, handEdited: false).Kind);
            Assert.Equal(
                SpawnGearPolicy.Decide(equip, weapon).Reason,
                SpawnGearPolicy.Decide(equip, weapon, handEdited: false).Reason);
        }
    }

    [Fact]
    public void 기본_슬롯만_찬_프로필은_장비_없음_사유로_Temporary_다()
    {
        // 슬롯 이름에서 결정까지 한 번에 — 이 테스트만이 두 단계를 이어서 고정한다.
        // 주머니·보안 컨테이너·완장·칼집은 플레이어가 아무것도 착용하지 않아도 들어 있다.
        var facts = SpawnGearPolicy.FromOccupiedSlots(new[] { "Pockets", "SecuredContainer", "ArmBand", "Scabbard" });
        var decision = SpawnGearPolicy.Decide(facts.HasAnyEquipment, facts.HasWeaponInRequiredSlot);

        Assert.Equal(SpawnGearKind.Temporary, decision.Kind);
        // 무기 없음이 아니라 "장비 없음" 쪽 사유로 떨어져야 한다.
        Assert.Equal(SpawnGearPolicy.Decide(false, true).Reason, decision.Reason);
        Assert.NotEqual(SpawnGearPolicy.Decide(true, false).Reason, decision.Reason);
    }

    [Fact]
    public void 어느_쪽이든_사람이_읽을_사유가_담긴다()
    {
        foreach (var decision in new[]
                 {
                     SpawnGearPolicy.Decide(false, false),
                     SpawnGearPolicy.Decide(false, true),
                     SpawnGearPolicy.Decide(true, false),
                     SpawnGearPolicy.Decide(true, true),
                 })
        {
            Assert.False(string.IsNullOrWhiteSpace(decision.Reason));
        }

        // 임시로 떨어진 두 사유는 서로 달라야 로그에서 원인을 가릴 수 있다.
        Assert.NotEqual(SpawnGearPolicy.Decide(false, true).Reason, SpawnGearPolicy.Decide(true, false).Reason);
    }

    // --- FromOccupiedSlots: 어느 슬롯이 "착용 장비"로 세어지는지를 고정한다 ---

    [Fact]
    public void 보안_컨테이너만_있으면_장비가_없다()
    {
        var facts = SpawnGearPolicy.FromOccupiedSlots(new[] { "SecuredContainer" });
        Assert.False(facts.HasAnyEquipment);
        Assert.False(facts.HasWeaponInRequiredSlot);
    }

    [Fact]
    public void 완장과_칼집만_있으면_장비가_없다()
    {
        // 모든 PMC 가 들고 있는 기본 슬롯들. 주머니까지 합쳐도 마찬가지다.
        var facts = SpawnGearPolicy.FromOccupiedSlots(new[] { "ArmBand", "Scabbard", "Pockets" });
        Assert.False(facts.HasAnyEquipment);
        // 칼집의 칼은 필수 무기 슬롯이 아니다.
        Assert.False(facts.HasWeaponInRequiredSlot);
    }

    [Fact]
    public void 방탄복만_있으면_장비는_있고_무기는_없다()
    {
        var facts = SpawnGearPolicy.FromOccupiedSlots(new[] { "ArmorVest" });
        Assert.True(facts.HasAnyEquipment);
        Assert.False(facts.HasWeaponInRequiredSlot);
    }

    [Fact]
    public void 권총집만_있어도_필수_무기_슬롯이_찬다()
    {
        var facts = SpawnGearPolicy.FromOccupiedSlots(new[] { "Holster" });
        Assert.True(facts.HasWeaponInRequiredSlot);
        // 권총집은 기본 슬롯이 아니므로 착용 장비로도 세어진다 - 권총만 든 프로필은 Real 이다.
        Assert.True(facts.HasAnyEquipment);
        Assert.Equal(SpawnGearKind.Real, SpawnGearPolicy.Decide(facts.HasAnyEquipment, facts.HasWeaponInRequiredSlot).Kind);
    }

    [Fact]
    public void 모르는_슬롯_이름은_장비로_세지_않는다()
    {
        // 닫힌 쪽으로 넘어뜨린다. Dogtag 는 EquipmentSlots 값이 아니라서 여기에 해당한다.
        var facts = SpawnGearPolicy.FromOccupiedSlots(new[] { "Dogtag", "SpecialSlot1", "뭔가이상한슬롯" });
        Assert.False(facts.HasAnyEquipment);
        Assert.False(facts.HasWeaponInRequiredSlot);
    }

    [Fact]
    public void 널이거나_빈_목록이면_장비가_없다()
    {
        foreach (var facts in new[]
                 {
                     SpawnGearPolicy.FromOccupiedSlots(null),
                     SpawnGearPolicy.FromOccupiedSlots([]),
                     SpawnGearPolicy.FromOccupiedSlots([null!, "", "   "]),
                 })
        {
            Assert.False(facts.HasAnyEquipment);
            Assert.False(facts.HasWeaponInRequiredSlot);
        }
    }

    [Fact]
    public void 슬롯_이름의_공백과_대소문자를_견딘다()
    {
        // ProfileMemberLookup 처럼 다듬어 받는다.
        var facts = SpawnGearPolicy.FromOccupiedSlots(new[] { "  armorvest  ", " HOLSTER " });
        Assert.True(facts.HasAnyEquipment);
        Assert.True(facts.HasWeaponInRequiredSlot);
    }
}
