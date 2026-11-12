using SlotFilterSweep;
using Xunit;

namespace SlotFilterSweep.Tests;

/// <summary>
/// "무엇을 죽은 참조로 칠 것인가" 만 본다. 표를 훑는 일은 서버 어셈블리가 필요해 여기서 보지 않는다.
///
/// 지켜야 하는 규칙 셋:
///   ① 표에 있는 id 는 절대 건드리지 않는다(이 모드는 빼기만 한다).
///   ② 표에 없으면 죽은 것이다.
///   ③ <c>KeepIds</c> 에 적힌 것은 표에 없어도 지킨다 — 우리보다 늦게 만드는 모드가 있을 수 있다.
/// </summary>
public class DeadRefScanTests
{
    private static ISet<string> Known(params string[] ids) => new HashSet<string>(ids, StringComparer.Ordinal);

    [Fact]
    public void 표에_있으면_살아_있다()
    {
        Assert.False(DeadRefScan.IsDead("aaa", Known("aaa", "bbb")));
    }

    [Fact]
    public void 표에_없으면_죽은_것이다()
    {
        Assert.True(DeadRefScan.IsDead("ccc", Known("aaa", "bbb")));
    }

    [Fact]
    public void 지키는_목록은_표보다_세다()
    {
        var keep = new HashSet<string>(["ccc"], StringComparer.Ordinal);

        Assert.False(DeadRefScan.IsDead("ccc", Known("aaa"), keep));
        Assert.True(DeadRefScan.IsDead("ddd", Known("aaa"), keep));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void 빈_값은_우리가_판단하지_않는다(string? id)
    {
        Assert.False(DeadRefScan.IsDead(id, Known("aaa")));
    }

    [Fact]
    public void 대소문자는_가린다()
    {
        // MongoId 는 소문자 16진수다. 섣불리 같은 것으로 보면 멀쩡한 id 를 지울 수 있다.
        Assert.True(DeadRefScan.IsDead("AAA", Known("aaa")));
    }

    [Fact]
    public void 필터에서_죽은_것만_골라낸다()
    {
        var dead = DeadRefScan.DeadIn(["aaa", "ccc", "bbb", "ddd"], Known("aaa", "bbb"));

        Assert.Equal(["ccc", "ddd"], dead);
    }

    [Fact]
    public void 같은_id_가_두_번_있어도_한_번만_센다()
    {
        var dead = DeadRefScan.DeadIn(["ccc", "ccc", "ccc"], Known("aaa"));

        Assert.Single(dead);
    }

    [Fact]
    public void 다_살아_있으면_빈_목록이다()
    {
        Assert.Empty(DeadRefScan.DeadIn(["aaa", "bbb"], Known("aaa", "bbb")));
    }

    [Fact]
    public void 필터가_없으면_빈_목록이다()
    {
        Assert.Empty(DeadRefScan.DeadIn(null, Known("aaa")));
    }

    [Fact]
    public void 세기만_했는지_지웠는지를_말에_넣는다()
    {
        Assert.Contains("세기만 했다", DeadRefScan.Describe(100, 33, 13, applied: false));
        Assert.Contains("지웠다", DeadRefScan.Describe(100, 33, 13, applied: true));
    }

    [Fact]
    public void 죽은_것이_없으면_그렇게_말한다()
    {
        var text = DeadRefScan.Describe(6846, 0, 0, applied: false);

        Assert.Contains("죽은 참조 없음", text);
        Assert.DoesNotContain("세기만", text);
    }
}
