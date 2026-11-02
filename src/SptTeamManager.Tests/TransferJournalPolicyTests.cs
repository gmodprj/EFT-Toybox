using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 이 판정이 틀리면 아이템이 복제되거나 사라진다. 그래서 "확정해도 되는 경우" 를 좁게 열어 두고,
// 나머지는 전부 멈추고 보고하는 쪽으로 닫는다. 둘 다/둘 다 아님/모르는 단계가 그 닫힌 쪽이다.
public class TransferJournalPolicyTests
{
    [Fact]
    public void 준비에서_끊기면_되돌린다()
    {
        // 준비 단계는 실물이 안 움직인다: 출처에 그대로 있고 목적지에는 없다.
        Assert.Equal(RecoveryDecision.RollBack,
            TransferJournalPolicy.DecideRecovery(TransferStage.Prepared, sourceStillHasItems: true, destinationHasItems: false));
    }

    [Fact]
    public void 커밋에서_양쪽이_다_가지면_멈추고_보고한다()
    {
        // 복제일 수 있다. 어느 쪽이 진짜인지 코드는 모른다 - 추측 복구 금지.
        Assert.Equal(RecoveryDecision.StopAndReport,
            TransferJournalPolicy.DecideRecovery(TransferStage.Committed, sourceStillHasItems: true, destinationHasItems: true));
    }

    [Fact]
    public void 준비에서도_양쪽이_다_가지면_멈추고_보고한다()
    {
        Assert.Equal(RecoveryDecision.StopAndReport,
            TransferJournalPolicy.DecideRecovery(TransferStage.Prepared, sourceStillHasItems: true, destinationHasItems: true));
    }

    [Fact]
    public void 커밋에서_출처만_비었으면_앞으로_확정한다()
    {
        Assert.Equal(RecoveryDecision.RollForward,
            TransferJournalPolicy.DecideRecovery(TransferStage.Committed, sourceStillHasItems: false, destinationHasItems: true));
    }

    [Fact]
    public void 빼는_쪽_저장_뒤_넣는_쪽_전에_끊겨_둘_다_없으면_멈추고_보고한다()
    {
        // 빼는 쪽 먼저 쓰는 순서가 고른 쪽: 사라진 상태. 코드가 아이템을 지어낼 수 없다.
        foreach (var stage in new[] { TransferStage.Prepared, TransferStage.Committed, TransferStage.Verified })
            Assert.Equal(RecoveryDecision.StopAndReport,
                TransferJournalPolicy.DecideRecovery(stage, sourceStillHasItems: false, destinationHasItems: false));
    }

    [Fact]
    public void 준비에서_출처가_비고_목적지에_있으면_앞으로_확정한다()
    {
        // 두 쓰기는 끝났고 커밋 표시만 못 남긴 경우.
        Assert.Equal(RecoveryDecision.RollForward,
            TransferJournalPolicy.DecideRecovery(TransferStage.Prepared, sourceStillHasItems: false, destinationHasItems: true));
    }

    [Fact]
    public void 확인까지_갔으면_할_일이_없다()
    {
        Assert.Equal(RecoveryDecision.RollForward,
            TransferJournalPolicy.DecideRecovery(TransferStage.Verified, sourceStillHasItems: false, destinationHasItems: true));
    }

    [Fact]
    public void 단계와_실물이_어긋나면_멈추고_보고한다()
    {
        // 커밋·확인이라고 적혀 있는데 실물은 안 움직였다: 저장이 되돌려졌을 수 있다. 조용히 닫지 않는다.
        Assert.Equal(RecoveryDecision.StopAndReport,
            TransferJournalPolicy.DecideRecovery(TransferStage.Committed, sourceStillHasItems: true, destinationHasItems: false));
        Assert.Equal(RecoveryDecision.StopAndReport,
            TransferJournalPolicy.DecideRecovery(TransferStage.Verified, sourceStillHasItems: true, destinationHasItems: false));
    }

    [Fact]
    public void 모르는_단계는_멈추고_보고한다()
    {
        foreach (var bogus in new[] { (TransferStage)99, (TransferStage)(-1) })
        {
            Assert.Equal(RecoveryDecision.StopAndReport,
                TransferJournalPolicy.DecideRecovery(bogus, sourceStillHasItems: true, destinationHasItems: false));
            Assert.Equal(RecoveryDecision.StopAndReport,
                TransferJournalPolicy.DecideRecovery(bogus, sourceStillHasItems: false, destinationHasItems: true));
        }
    }

    [Fact]
    public void 단계_이름_문자열을_읽는다_모르면_알_수_없음으로_닫는다()
    {
        Assert.Equal(TransferStage.Prepared, TransferJournalPolicy.ParseStage("Prepared"));
        Assert.Equal(TransferStage.Committed, TransferJournalPolicy.ParseStage("committed"));
        Assert.Equal(TransferStage.Verified, TransferJournalPolicy.ParseStage(" Verified "));
        Assert.Equal(TransferJournalPolicy.UnknownStage, TransferJournalPolicy.ParseStage("Done"));
        Assert.Equal(TransferJournalPolicy.UnknownStage, TransferJournalPolicy.ParseStage("7"));
        Assert.Equal(TransferJournalPolicy.UnknownStage, TransferJournalPolicy.ParseStage(null));
        Assert.Equal(TransferJournalPolicy.UnknownStage, TransferJournalPolicy.ParseStage(""));
    }
}

// 편집 차단 판정(HasOpenEntry 의 핵심). 의심스러우면 "막는다" 쪽이다: 편집을 거절하는 비용은 편집 한 번이고,
// 깨진 저널 위에서 편집을 허락하는 비용은 아이템이다.
public class TransferJournalBlockingTests
{
    [Fact]
    public void 미결_저널이_있으면_출처와_목적지_둘_다_편집을_막는다()
    {
        Assert.True(TransferJournalPolicy.BlocksEditing("Prepared", "src", "dst", "src"));
        Assert.True(TransferJournalPolicy.BlocksEditing("Committed", "src", "dst", "dst"));
    }

    [Fact]
    public void 관계없는_프로필은_막지_않는다()
    {
        Assert.False(TransferJournalPolicy.BlocksEditing("Committed", "src", "dst", "other"));
    }

    [Fact]
    public void 확인까지_끝난_항목은_막지_않는다()
    {
        Assert.False(TransferJournalPolicy.BlocksEditing("Verified", "src", "dst", "src"));
    }

    [Fact]
    public void 모르는_단계는_막는다()
    {
        Assert.True(TransferJournalPolicy.BlocksEditing("Whatever", "src", "dst", "src"));
        Assert.True(TransferJournalPolicy.BlocksEditing(null, "src", "dst", "src"));
    }

    [Fact]
    public void 대소문자와_공백이_달라도_같은_프로필로_본다()
    {
        Assert.True(TransferJournalPolicy.BlocksEditing("Prepared", "AbC", "dst", "  abc "));
    }

    [Fact]
    public void 비어_있거나_읽을_수_없는_식별자는_막는다()
    {
        Assert.True(TransferJournalPolicy.BlocksEditing("Prepared", "src", "dst", null));
        Assert.True(TransferJournalPolicy.BlocksEditing("Prepared", "src", "dst", " "));
        // 출처·목적지가 비어 읽을 수 없는 항목은 누가 관련됐는지 모른다 - 모두를 막는다.
        Assert.True(TransferJournalPolicy.BlocksEditing("Prepared", null, "dst", "other"));
        Assert.True(TransferJournalPolicy.BlocksEditing("Prepared", "src", "", "other"));
    }
}
