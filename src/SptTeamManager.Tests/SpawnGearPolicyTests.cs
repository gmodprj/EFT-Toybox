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
    public void 기본_슬롯만_찬_프로필은_장비_없음_사유로_Temporary_다()
    {
        // 주머니·보안 컨테이너·완장·칼집은 플레이어가 아무것도 착용하지 않아도 들어 있다.
        // 공장이 그것들을 장비로 세지 않으므로 hasAnyEquipment 는 false 로 들어온다.
        // 칼집의 칼은 필수 무기 슬롯이 아니므로 무기 bool 도 false 다.
        var decision = SpawnGearPolicy.Decide(false, false);
        Assert.Equal(SpawnGearKind.Temporary, decision.Kind);
        // 무기가 아니라 "장비 없음" 쪽 사유로 떨어져야 한다.
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
