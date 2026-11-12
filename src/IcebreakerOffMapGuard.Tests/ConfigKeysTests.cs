using IcebreakerOffMapGuard;
using Xunit;

namespace IcebreakerOffMapGuard.Tests;

/// <summary>
/// 설정 이름이 BepInEx 의 금지 문자를 쓰지 않는지만 본다.
///
/// 2026-11-11 설치본은 열쇠에 홑따옴표를 써서 두 번째 <c>Config.Bind</c> 에서
/// <c>ArgumentException</c> 을 맞고 <c>Awake</c> 가 멈췄다 — 패치를 하나도 걸지 못했고,
/// 설치본 .cfg 에는 설정이 셋 중 하나만 적혔다. 그 반복을 여기서 막는다.
/// BepInEx 를 참조하지 않으므로 금지 목록은 <see cref="ConfigKeys.Invalid"/> 에 베껴 두고,
/// 그 목록이 실제 BepInEx 와 같은지는 설치본 어셈블리로 따로 쟀다(경위 문서 참고).
/// </summary>
public class ConfigKeysTests
{
    [Fact]
    public void 쓰는_이름에는_금지_문자가_없다()
    {
        foreach (var name in ConfigKeys.All())
        {
            Assert.True(ConfigKeys.IsValid(name), $"BepInEx 가 거부하는 이름이다: [{name}]");
        }
    }

    [Theory]
    [InlineData("Cache 'is Icebreaker loaded'")] // 2026-11-11 에 실제로 터진 이름
    [InlineData("a=b")]
    [InlineData("say \"hi\"")]
    [InlineData("[General]")]
    [InlineData("back\\slash")]
    [InlineData("tab\there")]
    [InlineData("new\nline")]
    [InlineData("")]
    [InlineData("   ")]
    public void 금지_문자가_있으면_걸러낸다(string name)
    {
        Assert.False(ConfigKeys.IsValid(name));
    }

    [Fact]
    public void 이름은_서로_달라야_한다()
    {
        var names = new[] { ConfigKeys.SkipCensus, ConfigKeys.CacheLoaded, ConfigKeys.Report };

        Assert.Equal(names.Length, new HashSet<string>(names).Count);
    }
}
