using System;
using pitTeam.Shared;
using Xunit;

public class FollowerAiProfilePolicyTests
{
    [Fact]
    public void Usec_는_pmcUSEC_역할을_받는다()
    {
        Assert.Equal("pmcUSEC", FollowerAiProfilePolicy.ForSide("Usec").Role);
    }

    [Fact]
    public void Bear_는_pmcBEAR_역할을_받는다()
    {
        Assert.Equal("pmcBEAR", FollowerAiProfilePolicy.ForSide("Bear").Role);
    }

    [Fact]
    public void 진영이_바뀌면_역할도_바뀐다()
    {
        // 한쪽만 보면 상수를 돌려주는 구현도 통과한다.
        Assert.NotEqual(
            FollowerAiProfilePolicy.ForSide("Usec").Role,
            FollowerAiProfilePolicy.ForSide("Bear").Role);
    }

    [Theory]
    [InlineData("Usec")]
    [InlineData("Bear")]
    public void 어느_진영이든_난이도는_hard_다(string side)
    {
        // 프로필에 저장된 'easy' 가 그대로 흘러나오면 팀원이 적을 보지 못한다.
        Assert.Equal("hard", FollowerAiProfilePolicy.ForSide(side).Difficulty);
    }

    [Theory]
    [InlineData("usec", "pmcUSEC")]
    [InlineData("BEAR", "pmcBEAR")]
    [InlineData("  Usec  ", "pmcUSEC")]
    public void 대소문자와_앞뒤_공백은_같은_진영으로_본다(string side, string expected)
    {
        Assert.Equal(expected, FollowerAiProfilePolicy.ForSide(side).Role);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Savage")]
    [InlineData("pmcUSEC")]
    public void 모르는_진영은_역할을_지어내지_않고_던진다(string? side)
    {
        // 엉뚱한 진영의 역할로 내보내는 것이 이 정책이 막으려는 실패다.
        Assert.Throws<ArgumentException>(() => FollowerAiProfilePolicy.ForSide(side));
    }

    [Fact]
    public void 역할은_저장된_프로필의_assault_가_아니다()
    {
        // 2026-11-01 결함의 회귀 테스트: 프로필에 들어 있던 값이 그대로 나오면 안 된다.
        Assert.DoesNotContain("assault", FollowerAiProfilePolicy.ForSide("Usec").Role, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("assault", FollowerAiProfilePolicy.ForSide("Bear").Role, StringComparison.OrdinalIgnoreCase);
    }
}
