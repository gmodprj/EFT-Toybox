using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 이 판정은 "레이드 입장 때 걸어 둔 보호를 누구 것에서 푸는가" 다. 양쪽으로 다 틀릴 수 있고
// 둘 다 인게임에서 실측됐다(doc/WORKLOG.md): 너무 많이 풀면 같은 소총이 두 곳에 생기고,
// 너무 적게 풀면 플레이어가 시체에서 털어 온 물건이 레이드 종료 때 소멸한다.
public class RaidProtectionReleasePolicyTests
{
    private static readonly string[] Deleted = { "helmet", "rig", "rifle" };

    [Fact]
    public void 실물_장비로_죽으면_지운_것_정확히_그만큼_푼다()
    {
        var released = RaidProtectionReleasePolicy.SelectReleasableIds(SpawnGearKind.Real, escaped: false, Deleted);

        Assert.Equal(new[] { "helmet", "rifle", "rig" }, released.Order());
    }

    [Fact]
    public void 실물_장비로_생환하면_아무것도_풀지_않는다()
    {
        // 장비가 프로필에 그대로 있다. 풀면 플레이어가 가져온 쪽과 합쳐 한 자루가 두 곳에 생긴다.
        Assert.Empty(RaidProtectionReleasePolicy.SelectReleasableIds(SpawnGearKind.Real, escaped: true, Deleted));
    }

    [Fact]
    public void 임시_장비로_죽어도_아무것도_풀지_않는다()
    {
        // 어느 프로필의 재산도 아닌 생성물이다. 풀어 주면 밑천 없는 무한 장비 공급원이 된다.
        Assert.Empty(RaidProtectionReleasePolicy.SelectReleasableIds(SpawnGearKind.Temporary, escaped: false, Deleted));
    }

    [Fact]
    public void 임시_장비로_생환해도_아무것도_풀지_않는다()
    {
        Assert.Empty(RaidProtectionReleasePolicy.SelectReleasableIds(SpawnGearKind.Temporary, escaped: true, Deleted));
    }

    [Fact]
    public void 장비_기록이_없으면_아무것도_풀지_않는다()
    {
        // 스폰과 레이드 종료 사이에 서버가 재기동된 경우. 추측으로 지우지 않으므로 추측으로 풀지도 않는다.
        Assert.Empty(RaidProtectionReleasePolicy.SelectReleasableIds(null, escaped: false, Deleted));
        Assert.Empty(RaidProtectionReleasePolicy.SelectReleasableIds(null, escaped: true, Deleted));
    }

    [Fact]
    public void 지운_것이_없으면_아무것도_풀지_않는다()
    {
        // 권리는 의도가 아니라 실제 삭제를 따른다.
        Assert.Empty(RaidProtectionReleasePolicy.SelectReleasableIds(SpawnGearKind.Real, escaped: false, new string[0]));
        Assert.Empty(RaidProtectionReleasePolicy.SelectReleasableIds(SpawnGearKind.Real, escaped: false, null));
    }

    // 모르는 SpawnGearKind 값은 실물로 취급하지 않는다 - 나중에 종류가 늘어도 그 경로는 풀지 않는다.
    [Fact]
    public void 모르는_장비_종류는_풀지_않는다()
    {
        Assert.Empty(RaidProtectionReleasePolicy.SelectReleasableIds((SpawnGearKind)99, escaped: false, Deleted));
    }

    [Fact]
    public void 빈_id_는_담지_않고_중복은_한_번만_담는다()
    {
        var released = RaidProtectionReleasePolicy.SelectReleasableIds(
            SpawnGearKind.Real,
            escaped: false,
            new[] { "helmet", "helmet", "", "   ", null!, "rig" });

        Assert.Equal(new[] { "helmet", "rig" }, released.Order());
    }

    [Fact]
    public void id_비교는_대소문자와_공백을_구별한다()
    {
        // 느슨하게 맞추면 지워지지 않은 id 의 보호를 푸는 쪽으로 틀린다. 그래서 Ordinal 그대로 담는다.
        var released = RaidProtectionReleasePolicy.SelectReleasableIds(
            SpawnGearKind.Real,
            escaped: false,
            new[] { "helmet", "HELMET", " helmet" });

        Assert.Equal(new[] { " helmet", "HELMET", "helmet" }.Order(), released.Order());
    }
}
