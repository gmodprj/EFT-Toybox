using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 상세 화면의 "장비 편집" 입구를 열어도 되는가. 열 수 없는데 열린 화면은 끝까지 만져 놓고 저장에서 거절당하게 하므로,
// 입구가 먼저 이유를 말한다. 서버는 저장 때 같은 관문을 다시 선다 - 여기는 화면이 헛걸음을 시키지 않기 위한 판정이다.
public class GearEditEntryPolicyTests
{
    [Fact]
    public void 막히지_않았고_레이드도_없으면_연다()
    {
        var entry = GearEditEntryPolicy.Decide(blocked: false, blockedNotice: null, raidInProgress: false);

        Assert.True(entry.CanEdit);
        Assert.Equal(string.Empty, entry.Notice);
    }

    [Fact]
    public void 미결_전송에_막혀_있으면_막힘_안내문을_그대로_싣고_열지_않는다()
    {
        var entry = GearEditEntryPolicy.Decide(blocked: true, blockedNotice: "전송 기록이 남아 있습니다.", raidInProgress: false);

        Assert.False(entry.CanEdit);
        Assert.Equal("전송 기록이 남아 있습니다.", entry.Notice);
    }

    [Fact]
    public void 막혔는데_안내문이_비어_있어도_열지_않고_중립_문구를_쓴다()
    {
        var entry = GearEditEntryPolicy.Decide(blocked: true, blockedNotice: "  ", raidInProgress: false);

        Assert.False(entry.CanEdit);
        Assert.Equal(GearEditEntryPolicy.NeutralUnavailableNotice, entry.Notice);
    }

    [Fact]
    public void 레이드_중이면_열지_않고_서버가_저장_때_쓰는_것과_같은_사유를_말한다()
    {
        var entry = GearEditEntryPolicy.Decide(blocked: false, blockedNotice: null, raidInProgress: true);

        Assert.False(entry.CanEdit);
        Assert.Equal(GearEditEntryPolicy.RaidInProgressReason, entry.Notice);
    }

    [Fact]
    public void 막힘과_레이드가_겹치면_막힘_안내가_먼저다()
    {
        // 막힘 쪽은 플레이어가 닫기 버튼으로 풀 수 있는 이유다. 풀면 그다음에 레이드 사유가 나온다.
        var entry = GearEditEntryPolicy.Decide(blocked: true, blockedNotice: "전송 기록", raidInProgress: true);

        Assert.False(entry.CanEdit);
        Assert.Equal("전송 기록", entry.Notice);
    }

    [Fact]
    public void 레이드_사유와_중립_문구는_비어_있지_않다()
    {
        Assert.Contains("레이드", GearEditEntryPolicy.RaidInProgressReason);
        Assert.False(string.IsNullOrWhiteSpace(GearEditEntryPolicy.NeutralUnavailableNotice));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void 서버가_편집_가능_여부를_싣지_않았으면_클라이언트는_닫힌_쪽으로_안내한다(string? serverNotice)
    {
        // 와이어에 값이 없는 응답(옛 서버)은 "열어도 된다" 가 아니다.
        var text = GearEditEntryPolicy.NoticeWhenNotEditable(serverNotice);

        Assert.False(string.IsNullOrWhiteSpace(text));
        Assert.Equal(GearEditEntryPolicy.NeutralUnavailableNotice, text);
    }

    [Fact]
    public void 서버_안내문이_있으면_그대로_쓴다()
    {
        Assert.Equal("레이드 중", GearEditEntryPolicy.NoticeWhenNotEditable("레이드 중"));
    }
}
