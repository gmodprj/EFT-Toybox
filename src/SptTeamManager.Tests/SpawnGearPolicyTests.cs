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
}
