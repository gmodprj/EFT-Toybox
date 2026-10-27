using WebConfigBridge;
using Xunit;

namespace CompatToggle.Tests;

/// <summary>
/// 모드 페이지가 묵은 값으로 저장을 덮어쓰던 결함을 막는 지문 검사.
/// <see cref="ConfigStamp"/> 는 SPT 를 참조하지 않는 순수 코드라 여기서 시험할 수 있다.
/// </summary>
public class ConfigStampTests
{
    [Fact]
    public void 같은_내용은_같은_지문()
    {
        var a = "{ \"ModEnabled\": true }";
        var b = "{ \"ModEnabled\": true }";

        Assert.Equal(ConfigStamp.Of(a), ConfigStamp.Of(b));
    }

    [Fact]
    public void 한_글자만_달라도_지문이_달라진다()
    {
        var before = "{ \"FckWeapons\": true }";
        var after = "{ \"FckWeapons\": false }";

        Assert.NotEqual(ConfigStamp.Of(before), ConfigStamp.Of(after));
    }

    [Fact]
    public void 읽은_그대로면_맞다고_한다()
    {
        var text = "{ \"FckWeapons\": true }";
        var stamp = new ConfigStamp { Value = ConfigStamp.Of(text) };

        Assert.True(stamp.Matches(text));
    }

    [Fact]
    public void 밖에서_바뀌었으면_틀렸다고_한다()
    {
        var read = "{ \"FckWeapons\": true }";
        var stamp = new ConfigStamp { Value = ConfigStamp.Of(read) };

        // 사람이 페이지를 열어둔 사이 파일에서 직접 껐다.
        Assert.False(stamp.Matches("{ \"FckWeapons\": false }"));
    }

    [Fact]
    public void 파일을_읽을_수_없게_되었으면_틀렸다고_한다()
    {
        var stamp = new ConfigStamp { Value = ConfigStamp.Of("{}") };

        Assert.False(stamp.Matches(null));
    }

    [Fact]
    public void 아직_아무것도_읽지_않았으면_견주지_않는다()
    {
        // 지문을 쓰지 않는 호출자의 동작을 바꾸지 않는다.
        var stamp = new ConfigStamp();

        Assert.True(stamp.Matches("{ \"무엇이든\": 1 }"));
        Assert.True(stamp.Matches(null));
    }

    [Fact]
    public void 공백만_달라도_다른_파일로_본다()
    {
        // JSON 으로는 같은 뜻이지만 우리가 쓸 때 덮는 바이트가 다르다.
        // 되돌림을 막는 쪽이 목적이라 보수적으로 다르다고 본다.
        Assert.NotEqual(ConfigStamp.Of("{\"a\":1}"), ConfigStamp.Of("{ \"a\": 1 }"));
    }
}
