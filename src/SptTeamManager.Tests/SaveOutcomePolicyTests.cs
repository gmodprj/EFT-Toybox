using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 저장 요청에 대한 서버의 응답이 무엇을 뜻하는가를 **한 번** 정한다. 읽지 못한 응답을 거절로, 서버가 말한 거절을 모름으로
// 바꾸는 것은 둘 다 플레이어에게 거짓을 말하는 일이다(다시 편집하거나, 되돌려졌는지 모르고 믿는다).
public class SaveOutcomePolicyTests
{
    [Fact]
    public void err_0_이면_서버가_쓰기를_확인한_것이다()
    {
        var outcome = SaveOutcomePolicy.Classify(bodyPresent: true, err: 0, errmsg: null);

        Assert.Equal(SaveOutcomeKind.Confirmed, outcome.Kind);
    }

    [Fact]
    public void err_가_0이_아니면_서버가_말한_사유_그대로_거절이다()
    {
        var outcome = SaveOutcomePolicy.Classify(true, 1, "슬롯 'Headwear' 은(는) 이미 차 있습니다.");

        Assert.Equal(SaveOutcomeKind.Refused, outcome.Kind);
        Assert.Equal("슬롯 'Headwear' 은(는) 이미 차 있습니다.", outcome.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void 거절인데_사유가_비어_있어도_거절이고_중립_문구를_쓴다(string? errmsg)
    {
        var outcome = SaveOutcomePolicy.Classify(true, 7, errmsg);

        Assert.Equal(SaveOutcomeKind.Refused, outcome.Kind);
        Assert.Equal(SaveOutcomePolicy.RefusedWithoutReason, outcome.Message);
    }

    [Fact]
    public void 본문이_없거나_빈_응답은_거절이_아니라_모름이다()
    {
        var outcome = SaveOutcomePolicy.Classify(bodyPresent: false, err: null, errmsg: null);

        Assert.Equal(SaveOutcomeKind.Unknown, outcome.Kind);
        Assert.Equal(SaveOutcomePolicy.UnknownText, outcome.Message);
    }

    [Fact]
    public void 본문은_있지만_err_를_읽지_못했으면_모름이다()
    {
        // 서버가 거절했는지 썼는지 이 응답으로는 알 수 없다. 어느 쪽으로도 단정하지 않는다.
        var outcome = SaveOutcomePolicy.Classify(true, null, "뭔가");

        Assert.Equal(SaveOutcomeKind.Unknown, outcome.Kind);
    }

    [Fact]
    public void 모름_문구는_저장_여부를_단정하지_않는다()
    {
        Assert.Contains("저장되었을 수 있", SaveOutcomePolicy.UnknownText);
    }
}
