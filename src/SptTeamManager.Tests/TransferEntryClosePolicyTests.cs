using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 멈추고 보고한 전송 항목을 플레이어가 닫는 길. 이 판정이 느슨하면 진행 중인 전송의 기록을 지워 소실을 가리고,
// 너무 빡빡하면 편집이 영영 안 풀린다(저널은 암호화된 데이터베이스 안에 있어 손으로 못 고친다). 그래서 닫을 수 있는
// 경우를 하나로 좁히고, 닫기는 플레이어가 경고를 읽고 확인했을 때만 일어난다.
public class TransferEntryClosePolicyTests
{
    private const string Hero = "SPT02";
    private const string Mate = "Knox";

    [Theory]
    [InlineData("Prepared")]
    [InlineData("Committed")]
    [InlineData("뜻을-알-수-없는-단계")]
    public void 기동_때_멈추고_보고했고_지금도_멈춤이면_닫을_수_있다(string stage)
    {
        Assert.Equal(TransferCloseVerdict.MayClose,
            TransferEntryClosePolicy.Judge(stage, reportedAtStartup: true, RecoveryDecision.StopAndReport));
    }

    [Fact]
    public void 기동_때_보고하지_않은_항목은_닫지_못한다()
    {
        // 이번 실행에서 막 열린 항목은 지금 쓰기가 진행 중일 수 있다. 사람이 닫으면 진행 중인 전송의 기록이 사라진다.
        Assert.Equal(TransferCloseVerdict.NotYetReported,
            TransferEntryClosePolicy.Judge("Prepared", reportedAtStartup: false, RecoveryDecision.StopAndReport));
    }

    [Fact]
    public void 이미_확인까지_끝난_항목은_닫을_것이_없다()
    {
        Assert.Equal(TransferCloseVerdict.AlreadyClosed,
            TransferEntryClosePolicy.Judge("Verified", reportedAtStartup: true, RecoveryDecision.StopAndReport));
    }

    [Theory]
    [InlineData(RecoveryDecision.RollForward)]
    [InlineData(RecoveryDecision.RollBack)]
    public void 지금_사실이_일관되면_다음_기동이_스스로_정리하므로_사람이_닫지_않는다(RecoveryDecision decision)
    {
        Assert.Equal(TransferCloseVerdict.ResolvesItself,
            TransferEntryClosePolicy.Judge("Committed", reportedAtStartup: true, decision));
    }

    [Fact]
    public void 정의_밖_판정은_닫지_못하는_쪽으로_닫는다()
    {
        Assert.NotEqual(TransferCloseVerdict.MayClose,
            TransferEntryClosePolicy.Judge("Committed", reportedAtStartup: true, (RecoveryDecision)99));
    }

    [Fact]
    public void 확인_없이는_어떤_판정에서도_닫지_않는다()
    {
        foreach (var verdict in Enum.GetValues<TransferCloseVerdict>())
        {
            Assert.False(TransferEntryClosePolicy.MayCloseNow(verdict, playerAcknowledged: false));
        }
    }

    [Fact]
    public void 확인해도_닫을_수_있는_판정에서만_닫는다()
    {
        foreach (var verdict in Enum.GetValues<TransferCloseVerdict>())
        {
            Assert.Equal(verdict == TransferCloseVerdict.MayClose,
                TransferEntryClosePolicy.MayCloseNow(verdict, playerAcknowledged: true));
        }
    }

    [Fact]
    public void 정의_밖_판정은_확인해도_닫지_않는다()
    {
        Assert.False(TransferEntryClosePolicy.MayCloseNow((TransferCloseVerdict)99, playerAcknowledged: true));
    }

    [Fact]
    public void 경고는_양쪽에_다_있거나_양쪽에_다_없을_수_있다고_말한다()
    {
        var warning = TransferEntryClosePolicy.CloseWarning(Hero, Mate, 3);
        Assert.Contains("양쪽에 모두 있을 수도", warning);
        Assert.Contains("어디에도 없을 수도", warning);
    }

    [Fact]
    public void 경고는_코드가_판단하지_못했다고_말한다()
    {
        Assert.Contains("판단하지 못했습니다", TransferEntryClosePolicy.CloseWarning(Hero, Mate, 3));
    }

    [Fact]
    public void 경고는_닫는_것이_플레이어의_결정이라고_말한다()
    {
        Assert.Contains("직접 결정", TransferEntryClosePolicy.CloseWarning(Hero, Mate, 3));
    }

    [Fact]
    public void 경고는_닫아도_아이템이_복구되지_않는다고_말한다()
    {
        Assert.Contains("기록만 지워집니다", TransferEntryClosePolicy.CloseWarning(Hero, Mate, 3));
    }

    [Fact]
    public void 경고는_두_프로필_이름과_개수를_담는다()
    {
        var warning = TransferEntryClosePolicy.CloseWarning(Hero, Mate, 3);
        Assert.Contains(Hero, warning);
        Assert.Contains(Mate, warning);
        Assert.Contains("3개", warning);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void 이름을_모르면_모른다고_적고_빈칸으로_두지_않는다(string? name)
    {
        var warning = TransferEntryClosePolicy.CloseWarning(name, name, 1);
        Assert.Contains("알 수 없는 프로필", warning);
        Assert.DoesNotContain("''", warning);
    }

    [Theory]
    [InlineData(TransferCloseVerdict.MayClose)]
    [InlineData(TransferCloseVerdict.NotYetReported)]
    [InlineData(TransferCloseVerdict.ResolvesItself)]
    [InlineData(TransferCloseVerdict.AlreadyClosed)]
    public void 막힘_안내는_플레이어의_말이고_내부_이름이_아니다(TransferCloseVerdict verdict)
    {
        var notice = TransferEntryClosePolicy.BlockedNotice(Hero, Mate, 2, verdict);
        Assert.False(string.IsNullOrWhiteSpace(notice));
        Assert.DoesNotContain("StopAndReport", notice);
        Assert.DoesNotContain("Exception", notice);
        Assert.DoesNotContain("StmTransfer", notice);
        Assert.DoesNotContain("Stage", notice);
    }

    [Fact]
    public void 닫을_수_있을_때만_닫는_길을_안내한다()
    {
        Assert.Contains("버튼", TransferEntryClosePolicy.BlockedNotice(Hero, Mate, 2, TransferCloseVerdict.MayClose));
        Assert.DoesNotContain("버튼", TransferEntryClosePolicy.BlockedNotice(Hero, Mate, 2, TransferCloseVerdict.NotYetReported));
        Assert.DoesNotContain("버튼", TransferEntryClosePolicy.BlockedNotice(Hero, Mate, 2, TransferCloseVerdict.ResolvesItself));
    }

    [Fact]
    public void 아직_보고되지_않은_항목은_서버_재시작을_안내한다()
    {
        Assert.Contains("다시 시작", TransferEntryClosePolicy.BlockedNotice(Hero, Mate, 2, TransferCloseVerdict.NotYetReported));
    }

    // 지금 닫을 수 있는 항목과 재시작이 먼저 필요한 항목을 안내가 구별한다: 닫을 수 없는 이유와 언제 가능해지는지를 말해야
    // 출구 없는 막다른 길이 되지 않는다.
    [Fact]
    public void 재시작이_필요한_안내는_지금은_닫을_수_없다고_말한다()
    {
        Assert.Contains("지금은 닫을 수 없습니다", TransferEntryClosePolicy.BlockedNotice(Hero, Mate, 2, TransferCloseVerdict.NotYetReported));
    }

    [Fact]
    public void 재시작이_필요한_안내는_왜_그런지_말한다()
    {
        var notice = TransferEntryClosePolicy.BlockedNotice(Hero, Mate, 2, TransferCloseVerdict.NotYetReported);
        Assert.Contains("진행 중", notice);
        Assert.Contains("시작할 때", notice);
    }

    [Fact]
    public void 재시작이_필요한_안내는_언제_닫을_수_있게_되는지_말한다()
    {
        var notice = TransferEntryClosePolicy.BlockedNotice(Hero, Mate, 2, TransferCloseVerdict.NotYetReported);
        Assert.Contains("한 번 다시 시작", notice);
        Assert.Contains("이 화면에서 닫을 수 있습니다", notice);
    }

    [Fact]
    public void 지금_닫을_수_있는_안내는_재시작을_요구하지_않는다()
    {
        Assert.DoesNotContain("다시 시작", TransferEntryClosePolicy.BlockedNotice(Hero, Mate, 2, TransferCloseVerdict.MayClose));
    }

    [Fact]
    public void 재시작이_필요한_거절_메시지도_언제_가능한지_말한다()
    {
        var message = TransferEntryClosePolicy.RefusalMessage(TransferCloseVerdict.NotYetReported);
        Assert.Contains("한 번 다시 시작", message);
    }

    [Fact]
    public void 저널을_읽을_수_없을_때의_안내는_닫는_길이_없다고_솔직히_말한다()
    {
        var notice = TransferEntryClosePolicy.JournalUnreadableNotice;
        Assert.Contains("읽을 수 없", notice);
        Assert.DoesNotContain("버튼", notice);
    }

    [Theory]
    [InlineData(TransferCloseVerdict.MayClose)]
    [InlineData(TransferCloseVerdict.NotYetReported)]
    [InlineData(TransferCloseVerdict.ResolvesItself)]
    [InlineData(TransferCloseVerdict.AlreadyClosed)]
    public void 거절_메시지는_각_판정마다_비어_있지_않다(TransferCloseVerdict verdict)
    {
        Assert.False(string.IsNullOrWhiteSpace(TransferEntryClosePolicy.RefusalMessage(verdict)));
    }
}
