using System;

namespace pitTeam.Shared
{
    public enum TransferCloseVerdict
    {
        // 기동 때 멈추고 보고한 항목이고 지금도 멈춤이다. 플레이어가 경고를 읽고 확인하면 닫을 수 있다.
        MayClose,
        // 이미 확인까지 끝난 항목이다. 닫을 것이 없다.
        AlreadyClosed,
        // 기동 때 보고되지 않은 항목: 이번 실행에서 열렸으니 쓰기가 진행 중일 수 있다. 서버를 다시 시작해 기동 점검이 보게 한다.
        NotYetReported,
        // 지금 실물 사실이 일관돼 있다(한쪽만 가짐). 다음 기동이 스스로 정리하므로 사람이 닫지 않는다.
        ResolvesItself,
    }

    // 멈추고 보고한 전송 항목을 플레이어가 닫아도 되는가, 그리고 그때 무엇을 말하는가.
    //
    // 왜 이 길이 있는가: 저널은 세션마다 암호화된 데이터베이스 안에 있어 손으로 고칠 수 없고, 서버를 다시 시작해도
    // 멈춤 판정은 같은 이유로 다시 멈춘다. 코드가 추측으로 풀 수 없는 상태에서 사람에게도 출구가 없으면 그 팀원의
    // 편집은 영영 막힌다. 그래서 출구를 하나 두되, 닫는 일이 조용히 일어나지 않도록 세 가지를 요구한다:
    //   1) 기동 때 멈추고 보고한 항목일 것(진행 중인 전송을 지우지 않는다),
    //   2) 지금도 코드가 정할 수 없을 것,
    //   3) 플레이어가 경고를 읽고 명시적으로 확인했을 것.
    //
    // 의도적으로 catch 가 없다: 입력으로 닿는 모든 경로가 아래 분기로 끝난다.
    public static class TransferEntryClosePolicy
    {
        private const string UnknownProfileName = "알 수 없는 프로필";

        public static TransferCloseVerdict Judge(string? stageName, bool reportedAtStartup, RecoveryDecision decisionNow)
        {
            if (TransferJournalPolicy.ParseStage(stageName) == TransferStage.Verified)
            {
                return TransferCloseVerdict.AlreadyClosed;
            }

            if (!reportedAtStartup)
            {
                return TransferCloseVerdict.NotYetReported;
            }

            // StopAndReport 만 사람의 몫이다. 그 밖의 값(정의 밖 포함)은 닫지 않는 쪽 - 사람이 닫을 이유가 없거나 모른다.
            return decisionNow == RecoveryDecision.StopAndReport
                ? TransferCloseVerdict.MayClose
                : TransferCloseVerdict.ResolvesItself;
        }

        // 닫기는 "닫을 수 있는 판정" 과 "플레이어의 명시적 확인" 이 둘 다 있을 때만. 확인은 기본값이 아니다.
        public static bool MayCloseNow(TransferCloseVerdict verdict, bool playerAcknowledged) =>
            playerAcknowledged && verdict == TransferCloseVerdict.MayClose;

        public static string BlockedNotice(string? sourceName, string? destinationName, int itemCount, TransferCloseVerdict verdict)
        {
            if (verdict == TransferCloseVerdict.AlreadyClosed)
            {
                return "이 기록은 이미 끝난 전송입니다.";
            }

            var head =
                $"이 팀원의 장비 편집이 막혀 있습니다. '{Name(sourceName)}'와(과) '{Name(destinationName)}' 사이에서 " +
                $"아이템 {itemCount}개를 옮기던 작업이 끝까지 가지 못하고 멈췄고, 어느 쪽이 맞는지 코드가 판단하지 못해 더 진행하지 않았습니다.";

            switch (verdict)
            {
                case TransferCloseVerdict.MayClose:
                    return head + " 두 프로필의 장비와 스태시를 직접 확인한 뒤, 아래 버튼으로 이 기록을 닫을 수 있습니다.";
                case TransferCloseVerdict.NotYetReported:
                    return head + " 진행 중인 작업일 수 있으니, 서버를 다시 시작한 뒤에 다시 확인하세요.";
                case TransferCloseVerdict.ResolvesItself:
                    return head + " 지금은 양쪽 상태가 서로 맞아 보입니다. 서버를 다시 시작하면 저절로 정리됩니다.";
                default:
                    return head;
            }
        }

        // 닫기 확인창의 본문. 플레이어가 무엇을 받아들이는지 있는 그대로 적는다.
        public static string CloseWarning(string? sourceName, string? destinationName, int itemCount)
        {
            var source = Name(sourceName);
            var destination = Name(destinationName);
            return
                "이 기록을 닫으면 이 팀원의 장비 편집 제한이 풀립니다. 먼저 알아 두세요.\n\n" +
                $"- 옮기던 아이템 {itemCount}개가 '{source}'와(과) '{destination}' 양쪽에 모두 있을 수도, 어디에도 없을 수도 있습니다. " +
                "코드는 어느 쪽인지 판단하지 못했습니다.\n" +
                "- 닫아도 아이템은 옮겨지거나 복구되지 않습니다. 기록만 지워집니다.\n" +
                "- 닫기는 되돌릴 수 없고, 닫을지는 직접 결정하는 일입니다. 두 프로필의 장비와 스태시를 확인한 뒤에 닫으세요.";
        }

        // 저널 문서 자체를 읽을 수 없을 때. 어느 항목이 문제인지 알 수 없으므로 닫을 대상이 없다 - 솔직히 그렇게 말한다.
        public const string JournalUnreadableNotice =
            "이 팀원의 장비 편집이 막혀 있습니다. 전송 기록을 읽을 수 없어 안전한지 확인할 수 없습니다. " +
            "이 화면에서는 풀 수 없으니 서버 로그를 확인하세요.";

        // 서버가 닫기를 거절했을 때 플레이어에게 보여 줄 말.
        public static string RefusalMessage(TransferCloseVerdict verdict)
        {
            switch (verdict)
            {
                case TransferCloseVerdict.AlreadyClosed:
                    return "이 기록은 이미 닫혔습니다.";
                case TransferCloseVerdict.NotYetReported:
                    return "이 기록은 진행 중일 수 있어 지금 닫을 수 없습니다. 서버를 다시 시작한 뒤에 다시 확인하세요.";
                case TransferCloseVerdict.ResolvesItself:
                    return "이 기록은 서버를 다시 시작하면 저절로 정리되므로 직접 닫을 필요가 없습니다.";
                default:
                    return "이 기록을 지금은 닫을 수 없습니다.";
            }
        }

        private static string Name(string? name) => string.IsNullOrWhiteSpace(name) ? UnknownProfileName : name!.Trim();
    }
}
