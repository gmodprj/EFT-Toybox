using HelmetSlotUnion;
using Xunit;

namespace HelmetSlotUnion.Tests;

/// <summary>
/// 합집합에 무엇을 담을지만 본다.
///
/// 막으려는 것은 하나다 — 어느 헬멧 하나에 적혀 있던 **없는 id** 가 합집합을 타고
/// 모든 헬멧으로 퍼지는 것(2026-11-12 실측: 한 id 가 헬멧 174칸에 들어 있었다).
/// </summary>
public class PoolPolicyTests
{
    private static ISet<string> Known(params string[] ids) => new HashSet<string>(ids, StringComparer.Ordinal);

    [Fact]
    public void 표에_있는_것은_담는다()
    {
        Assert.True(PoolPolicy.ShouldPool("aaa", Known("aaa", "bbb")));
    }

    [Fact]
    public void 표에_없는_것은_담지_않는다()
    {
        Assert.False(PoolPolicy.ShouldPool("ccc", Known("aaa", "bbb")));
    }

    [Fact]
    public void 꺼_두면_예전처럼_전부_담는다()
    {
        Assert.True(PoolPolicy.ShouldPool("ccc", Known("aaa"), skipDead: false));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void 빈_값은_담지_않는다(string? id)
    {
        Assert.False(PoolPolicy.ShouldPool(id, Known("aaa")));
        Assert.False(PoolPolicy.ShouldPool(id, Known("aaa"), skipDead: false));
    }

    [Fact]
    public void 대소문자는_가린다()
    {
        Assert.False(PoolPolicy.ShouldPool("AAA", Known("aaa")));
    }

    [Fact]
    public void 걸러낸_것과_버린_수를_함께_준다()
    {
        var (pooled, skipped) = PoolPolicy.Filter(["aaa", "ccc", "bbb"], Known("aaa", "bbb"));

        Assert.Equal(["aaa", "bbb"], pooled);
        Assert.Equal(1, skipped);
    }

    [Fact]
    public void 빈_값은_버린_수에_세지_않는다()
    {
        var (pooled, skipped) = PoolPolicy.Filter(["aaa", "", null!], Known("aaa"));

        Assert.Single(pooled);
        Assert.Equal(0, skipped);
    }

    [Fact]
    public void 꺼_두면_아무것도_버리지_않는다()
    {
        var (pooled, skipped) = PoolPolicy.Filter(["aaa", "ccc"], Known("aaa"), skipDead: false);

        Assert.Equal(2, pooled.Count);
        Assert.Equal(0, skipped);
    }

    [Fact]
    public void 목록이_없으면_빈_결과다()
    {
        var (pooled, skipped) = PoolPolicy.Filter(null, Known("aaa"));

        Assert.Empty(pooled);
        Assert.Equal(0, skipped);
    }

    [Fact]
    public void 버린_것이_있을_때만_말한다()
    {
        Assert.Equal(string.Empty, PoolPolicy.Describe(0));
        Assert.Contains("3개", PoolPolicy.Describe(3));
    }
}
