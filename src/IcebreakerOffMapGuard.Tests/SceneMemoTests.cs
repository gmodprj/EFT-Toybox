using IcebreakerOffMapGuard;
using Xunit;

namespace IcebreakerOffMapGuard.Tests;

/// <summary>
/// 캐시 규칙만 본다 — 판정 자체는 모드 쪽 것이고 우리가 흉내 내지 않는다.
/// 테스트는 한 프로세스에서 이어 돌므로 각 시험이 시작할 때 상태를 비운다.
/// </summary>
public class SceneMemoTests
{
    private static void Reset()
    {
        SceneMemo.Invalidate();
        SceneMemo.Stats.Reset();
    }

    [Fact]
    public void 처음에는_캐시가_비어_있어_원본에게_물어야_한다()
    {
        Reset();

        Assert.False(SceneMemo.TryGet(out _));
    }

    [Fact]
    public void 적어_두면_같은_값을_돌려준다()
    {
        Reset();
        SceneMemo.Set(true);

        Assert.True(SceneMemo.TryGet(out var value));
        Assert.True(value);

        SceneMemo.Set(false);

        Assert.True(SceneMemo.TryGet(out var second));
        Assert.False(second);
    }

    [Fact]
    public void 씬이_바뀌면_버린다()
    {
        Reset();
        SceneMemo.Set(true);
        SceneMemo.Invalidate();

        Assert.False(SceneMemo.TryGet(out _));
    }

    [Fact]
    public void 버린_뒤_다시_적으면_또_답한다()
    {
        Reset();
        SceneMemo.Set(true);
        SceneMemo.Invalidate();
        SceneMemo.Set(false);

        Assert.True(SceneMemo.TryGet(out var value));
        Assert.False(value);
    }

    [Fact]
    public void 센_값이_올라간다()
    {
        Reset();
        SceneMemo.Set(true);
        SceneMemo.Invalidate();
        SceneMemo.Set(true);

        Assert.Equal(2, SceneMemo.Stats.Stored);
        // Reset 직후의 Invalidate 하나 + 위에서 부른 하나
        Assert.Equal(1, SceneMemo.Stats.Invalidated);
    }
}
