using pitTeam.Server.Models;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Servers;

namespace pitTeam.Server.Services;

// 프로필 두 개를 고치는 모든 조작이 지나는 3단 전송 저널(설계 §6-8). 이 클래스는 아이템을 옮기지 않는다 -
// "옮기기 전에 적고, 옮긴 뒤에 표시하고, 확인한 뒤에 닫는다" 만 맡는다.
//
// 쓰는 쪽이 지켜야 할 순서 (이 순서가 안전 방향이다 - 끊기면 복제 대신 소실로 기운다). 각 단계는 저널이 강제한다:
// 순서를 어기는 호출은 거절(false)되고 항목은 그대로 열려 있다.
//   1. Prepare                  - 저널에 먼저 적는다. 실패하면 던진다. 던졌으면 아이템을 건드리지 않는다.
//   (취소) Abort                - 출처 쓰기를 시작하기 전에만. BeginSourceWrite 뒤에는 거절된다.
//   2. BeginSourceWrite         - 출처에서 빼는 쓰기 "직전" 에 부른다(사실을 쓰기보다 먼저 남긴다). 그 뒤 출처에서 빼고 SaveProfile.
//   3. RecordDestinationItemIds - 목적지에 실제로 쓸 id(재매김 뒤의 id, 재매김이 없으면 출처와 같은 id)를 기록한다.
//                                 목적지 쓰기 시작 전에만, 한 번 기록되면 바뀌지 않는다.
//   4. BeginDestinationWrite    - 목적지에 넣는 쓰기 "직전" 에 부른다. 출처 쓰기가 시작됐고 id 가 기록돼 있어야 한다.
//                                 그 뒤 목적지에 넣고 SaveProfile.
//   5. MarkCommitted            - 두 사실과 id 기록이 다 있을 때만 된다. 두 저장이 "둘 다" 끝난 뒤에 부른다.
//   6. Verify                   - SaveProfile 뒤 디스크에서 다시 읽은 결과를 대조해 맞으면 항목을 닫는다. 커밋된 항목만.
// "쓰기 직전" 계약: Begin* 이 false 를 주면 그 쓰기를 하지 않는다. Begin* 이 true 를 준 뒤에는 쓰기가 일어났을 수
// 있다고 저널이 본다(그래서 Abort 가 막힌다). 단계(Prepared)는 MarkCommitted 에서야 바뀌므로, 단계만으로는 "안 움직임 /
// 출처 쓰기 일어남 / 목적지 쓰기 일어남" 을 구별할 수 없다 - 그 구별을 두 사실이 맡는다.
// 1 에서 5 사이 어디서 끊겨도 항목은 열린 채 남고, 다음 기동의 RecoverOpenEntries 가 TransferJournalPolicy 로
// 판정한다. 판정이 애매하면 항목을 닫지 않고 그 팀원의 편집을 막은 채 사람에게 보고한다.
//
// StmRaidReceiptStore 의 관례를 따른다: 캐시 없음(매 호출 문서를 새로 읽는다), 락 하나, 파일명은
// "{정수}.json" 이 아닌 이름(TeammateDatabase.IsProfileDocument 가 문서 종류를 파일명 모양으로 가린다),
// 읽을 수 없는 문서와 문서 없음의 구별, "먼저 예약하고 그 다음 쓴다".
// 다른 점: 영수증은 "있으면 거절" 하나만 묻지만 저널은 단계가 바뀌며 항목이 열렸다 닫힌다. 그리고 영수증 저장소의
// 실패는 전부 false 였지만, 여기서 "열린 항목이 있나" 의 실패는 true 다 - 닫힌 쪽이 서로 반대다.
[Injectable(InjectionType.Singleton)]
public class StmTransferJournal(FriendlyTeammateStorage storage, SaveServer saveServer, ISptLogger<StmTransferJournal> logger)
{
    // "{aid}.json" 도, "{aid}-settings.json" / "{aid}-equipment.json" / "recruit-requests.json" 도 아니다.
    private const string Document = "stm-transfer-journal.json";

    private readonly object sync = new();

    // 기동 때 RecoverOpenEntries 가 "코드가 정할 수 없다" 고 멈추고 보고한 항목의 id. 메모리에만 둔다 - 다음 기동이 같은
    // 항목을 다시 판정해 다시 채우므로 저장할 이유가 없고, 저장하면 "보고된 적 있다" 가 낡은 사실로 남는다.
    // 플레이어가 항목을 닫을 수 있는지의 첫 조건이다(TransferEntryClosePolicy): 이번 실행에서 막 열린 항목은 쓰기가
    // 진행 중일 수 있어 여기 없고, 따라서 닫을 수 없다. sync 로 보호한다.
    private readonly HashSet<string> reportedStopped = new(StringComparer.Ordinal);

    // 저널에 먼저 적는다. 성공하면 journalId 를 돌려주고, 실패하면 던진다(반환값을 무시해도 아이템을 옮길 수
    // 없게). 같은 프로필이 이미 미결 항목에 걸려 있으면 거절한다 - 끊긴 전송 위에 새 전송을 쌓지 않는다.
    public string Prepare(MongoId sessionId, string kind, string sourceProfileId, string destinationProfileId, IReadOnlyList<string> itemIds)
    {
        if (string.IsNullOrWhiteSpace(kind) || string.IsNullOrWhiteSpace(sourceProfileId) || string.IsNullOrWhiteSpace(destinationProfileId)
            || itemIds == null || itemIds.Count == 0 || itemIds.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("StmTransferJournalPrepareInvalid");
        if (string.Equals(sourceProfileId.Trim(), destinationProfileId.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("StmTransferJournalSameProfile");

        lock (sync)
        {
            var entries = Load(sessionId).ToList();
            if (entries.Any(e => TransferJournalPolicy.BlocksEditing(e.Stage, e.SourceProfileId, e.DestinationProfileId, sourceProfileId)
                              || TransferJournalPolicy.BlocksEditing(e.Stage, e.SourceProfileId, e.DestinationProfileId, destinationProfileId)))
                throw new InvalidOperationException("StmTransferJournalOpenEntry");

            var journalId = Guid.NewGuid().ToString("N");
            entries.Add(new StmTransferJournalEntry(
                journalId, kind.Trim(), sourceProfileId.Trim(), destinationProfileId.Trim(),
                itemIds.Select(i => i.Trim()).ToList(),
                nameof(TransferStage.Prepared), DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            Save(sessionId, entries);
            return journalId;
        }
    }

    // 목적지에 실제로 쓸 id 가 확정되면(재매김이 있었다면 재매김 뒤의 id) BeginDestinationWrite 전에 부른다. 복구가 이
    // 기록으로 목적지를 본다. 출처 ItemIds 와 같은 순서·개수여야 한다. 목적지 쓰기 시작 뒤에는 거절, 한 번 기록된
    // 뒤에는 다른 id 로 거절(같은 id 는 허용) - TransferJournalPolicy.CanRecordDestinationIds.
    public bool RecordDestinationItemIds(MongoId sessionId, string journalId, IReadOnlyList<string> destinationItemIds) =>
        Transition(sessionId, journalId, "목적지 id 기록",
            e => TransferJournalPolicy.CanRecordDestinationIds(e.Stage, e.DestinationWriteStarted, e.ItemIds, e.DestinationItemIds, destinationItemIds),
            e => e with { DestinationItemIds = destinationItemIds.Select(i => i.Trim()).ToList() });

    // 출처에서 빼는 쓰기 "직전" 에 부른다. true 를 받아야만 쓴다. 이 사실이 적힌 뒤에는 Abort 가 거절된다.
    public bool BeginSourceWrite(MongoId sessionId, string journalId) =>
        Transition(sessionId, journalId, "출처 쓰기 시작",
            e => TransferJournalPolicy.CanBeginSourceWrite(e.Stage, e.SourceWriteStarted),
            e => e with { SourceWriteStarted = true });

    // 목적지에 넣는 쓰기 "직전" 에 부른다. true 를 받아야만 쓴다. 출처 쓰기가 시작됐고 목적지 id 가 기록돼 있어야 한다.
    public bool BeginDestinationWrite(MongoId sessionId, string journalId) =>
        Transition(sessionId, journalId, "목적지 쓰기 시작",
            e => TransferJournalPolicy.CanBeginDestinationWrite(e.Stage, e.SourceWriteStarted, e.DestinationItemIds != null, e.DestinationWriteStarted),
            e => e with { DestinationWriteStarted = true });

    // 두 저장이 둘 다 끝난 "뒤" 에 부른다. 두 쓰기 사실과 id 기록이 다 없으면 거절한다. 표시를 못 남기면 false -
    // 항목은 Prepared 로 남아 복구가 사실(실물)을 보고 정하므로, 표시 실패가 아이템을 해치지는 않는다.
    public bool MarkCommitted(MongoId sessionId, string journalId) =>
        Transition(sessionId, journalId, "커밋 표시",
            e => TransferJournalPolicy.CanCommit(e.Stage, e.SourceWriteStarted, e.DestinationWriteStarted, e.DestinationItemIds != null),
            e => e with { Stage = nameof(TransferStage.Committed) });

    // 커밋 없이 닫는다 - Prepare 뒤 호출자가 깨끗이 취소할 때. BeginSourceWrite 전에만 된다
    // (TransferJournalPolicy.CanAbort): 그 사실이 쓰기보다 먼저 적히므로 "출처에서 뺀 뒤의 취소" 는 표현할 수 없다.
    public bool Abort(MongoId sessionId, string journalId) =>
        CloseIf(sessionId, journalId, e => TransferJournalPolicy.CanAbort(e.Stage, e.SourceWriteStarted), "취소");

    // 락 안에서 항목을 다시 읽고, 판정을 통과하면 바꿔 저장한다. 판정과 변경 사이에 다른 호출이 끼어들 수 없다.
    private bool Transition(MongoId sessionId, string journalId, string what,
        Func<StmTransferJournalEntry, bool> allowed, Func<StmTransferJournalEntry, StmTransferJournalEntry> change)
    {
        try
        {
            lock (sync)
            {
                var entries = Load(sessionId).ToList();
                var index = entries.FindIndex(e => e.JournalId == journalId);
                if (index < 0) return false;
                var entry = entries[index];
                if (!allowed(entry))
                {
                    logger.Error($"[STM] 전송 '{journalId}' {what} 거절: 단계 {entry.Stage} / 출처 쓰기 시작 {entry.SourceWriteStarted} / 목적지 쓰기 시작 {entry.DestinationWriteStarted} / 목적지 id 기록 {(entry.DestinationItemIds != null ? "있음" : "없음")}. 항목을 그대로 둔다.");
                    return false;
                }
                entries[index] = change(entry);
                Save(sessionId, entries);
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 전송 '{journalId}' {what} 을(를) 저장하지 못했다: {ex}");
            return false;
        }
    }


    // readBackMatches: 호출자가 SaveProfile 뒤 디스크에서 다시 읽어 기대와 대조한 결과. 저널은 프로필을 읽는
    // 방법을 모른다 - 대신 대조가 통과했다는 증명 없이는 항목을 닫지 않는다.
    // 커밋된 항목만 확인할 수 있다(TransferJournalPolicy.CanVerify): 준비 직후 Verify 는 아무것도 안 옮긴 항목을
    // 닫는 길이라 대조를 부르기 전에 거절한다. false 이거나 대조가 던지면 항목은 열려 있고 편집은 계속 막힌다.
    public bool Verify(MongoId sessionId, string journalId, Func<bool> readBackMatches)
    {
        try
        {
            lock (sync)
            {
                var entry = Load(sessionId).FirstOrDefault(e => e.JournalId == journalId);
                if (entry == null || !TransferJournalPolicy.CanVerify(entry.Stage))
                {
                    logger.Error($"[STM] 전송 '{journalId}' 은(는) 커밋된 항목이 아니라 확인할 수 없다(단계 {entry?.Stage ?? "항목 없음"}).");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 전송 '{journalId}' 의 단계를 읽을 수 없어 확인하지 않는다: {ex.Message}");
            return false;
        }

        bool matched;
        try
        {
            matched = readBackMatches();
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 전송 '{journalId}' 의 디스크 재확인이 실패했다. 항목을 닫지 않는다: {ex}");
            return false;
        }
        if (!matched)
        {
            logger.Error($"[STM] 전송 '{journalId}' 의 디스크 재확인이 기대와 다르다. 항목을 닫지 않고 편집을 막은 채 둔다.");
            return false;
        }
        return CloseIf(sessionId, journalId, e => TransferJournalPolicy.CanVerify(e.Stage), "확인");
    }

    // 문서를 못 읽는 동안 HasOpenEntry 가 매 호출 Error 를 남기면 로그가 묻힌다. 프로세스당 한 번만 남기고, 다시
    // 읽히면 플래그를 풀어 다음 장애는 다시 한 번 남긴다. 답(true)은 호출마다 그대로다.
    private volatile bool unreadableLogged;

    // 던지지 않는다. 읽을 수 없으면 true("열린 항목이 있다") - 편집을 거절하는 비용은 편집 한 번이고, 깨진 저널
    // 위에서 허락하는 비용은 아이템이다. 매 편집 시도마다 불리므로 문서 하나를 읽는 것 말고는 하지 않는다.
    public bool HasOpenEntry(MongoId sessionId, string profileId)
    {
        try
        {
            lock (sync)
            {
                var open = Load(sessionId).Any(e => TransferJournalPolicy.BlocksEditing(e.Stage, e.SourceProfileId, e.DestinationProfileId, profileId));
                unreadableLogged = false;
                return open;
            }
        }
        catch (Exception ex)
        {
            if (!unreadableLogged)
            {
                unreadableLogged = true;
                logger.Error($"[STM] 전송 저널을 읽을 수 없어 열린 항목이 있는 것으로 치고 팀원 편집을 막는다(이후 같은 로그는 생략, 처음 프로필 '{profileId}'): {ex.Message}");
            }
            return true;
        }
    }

    // 기동 때 세션마다 한 번. 던지지 않는다(기동을 막지 않는다 - PitFireTeamServerPostLoad 의 다른 기동 점검과 같다).
    // RollForward / RollBack 은 "사실이 이미 일관된 상태" 라서 항목을 닫는 것이 복구의 전부다 - 이 클래스는
    // 아이템을 움직이지 않는다. StopAndReport 는 항목을 열어 둔 채(= 그 팀원 편집 차단 유지) Error 를 남긴다.
    // 반환값은 멈추고 보고한 항목 수.
    public int RecoverOpenEntries(MongoId sessionId)
    {
        var stopped = 0;
        List<StmTransferJournalEntry> open;
        try
        {
            lock (sync)
                open = Load(sessionId).Where(e => TransferJournalPolicy.ParseStage(e.Stage) != TransferStage.Verified).ToList();
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 전송 저널을 읽을 수 없다. 이 세션의 팀원 편집은 열린 항목이 있는 것으로 막힌다: {ex.Message}");
            return 1;
        }

        foreach (var entry in open)
        {
            try
            {
                var decision = Decide(entry);
                if (decision == RecoveryDecision.StopAndReport)
                {
                    stopped++;
                    lock (sync) reportedStopped.Add(entry.JournalId);
                    // 한 줄로 사람이 행동할 수 있게: 어느 파일의 어느 문서인가, 어느 항목인가, 언제 시작됐나, 무엇이 걸려 있나.
                    // 문서는 암호화된 데이터베이스 안의 레코드라 메모장으로 고칠 수 없다 - 그래서 경로와 문서 이름을 둘 다 적는다.
                    var started = FormatStart(entry.CreatedAtUnixSeconds);
                    logger.Error(
                        $"[STM] 끝나지 않은 전송을 자동으로 정할 수 없어 두 프로필의 팀원 편집을 막는다. " +
                        $"항목 '{entry.JournalId}' / 종류 '{entry.Kind}' / 단계 {entry.Stage} / 시작 {started}(UTC) / " +
                        $"출처 프로필 '{entry.SourceProfileId}' / 목적지 프로필 '{entry.DestinationProfileId}' / " +
                        $"아이템 [{string.Join(", ", entry.ItemIds ?? [])}] / 목적지 기록 id [{string.Join(", ", entry.DestinationItemIds ?? [])}]. " +
                        $"저널 문서 '{Document}' 는 데이터베이스 파일 '{storage.SharedDocumentDatabasePath(sessionId)}' 안에 있다. " +
                        $"아이템이 복제됐거나 사라졌을 수 있다: 두 프로필의 장비·스태시를 직접 확인해 바로잡은 뒤 이 항목을 닫아야 편집이 풀린다.");
                    continue;
                }

                if (!Close(sessionId, entry.JournalId))
                {
                    stopped++;
                    logger.Error($"[STM] 전송 '{entry.JournalId}' 은 {decision} 로 정해졌지만 항목을 닫지 못했다. 편집을 계속 막는다.");
                    continue;
                }
                logger.Info($"[STM] 끝나지 않은 전송 '{entry.JournalId}' 을(를) {decision} 로 닫았다(실물은 이미 일관된 상태).");
            }
            catch (Exception ex)
            {
                stopped++;
                logger.Error($"[STM] 전송 '{entry.JournalId}' 복구 중 오류. 항목을 열어 두고 편집을 막는다: {ex}");
            }
        }
        return stopped;
    }

    // ---- 플레이어가 막힌 항목을 닫는 길 ----
    // 저널 문서는 세션별 암호화 데이터베이스 안에 있어 손으로 고칠 수 없고, 다시 시작해도 멈춤 판정은 같은 이유로 다시
    // 멈춘다. 코드가 추측으로 풀 수 없는 상태에서 사람에게도 출구가 없으면 그 팀원의 편집은 영영 막힌다. 그래서 이 한
    // 가지 출구를 둔다. 닫을 수 있는지의 판정은 TransferEntryClosePolicy 가 하고 이 클래스는 사실만 모은다.

    // 이 팀원의 편집을 막고 있는 항목과 각 항목을 지금 닫을 수 있는지의 판정. 던지지 않는다.
    // 문서를 못 읽으면 JournalReadable == false: 어느 항목이 문제인지 모르므로 닫을 대상이 없다(HasOpenEntry 는 이때 true).
    public StmBlockingEntries GetBlockingEntries(MongoId sessionId, string profileId)
    {
        try
        {
            lock (sync)
            {
                var blocking = Load(sessionId)
                    .Where(e => TransferJournalPolicy.BlocksEditing(e.Stage, e.SourceProfileId, e.DestinationProfileId, profileId))
                    .Select(e => new StmBlockingEntry(e, JudgeClose(e)))
                    .ToList();
                return new StmBlockingEntries(true, blocking);
            }
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 팀원 프로필 '{profileId}' 을(를) 막는 전송 항목을 읽지 못했다: {ex.Message}");
            return new StmBlockingEntries(false, []);
        }
    }

    // 플레이어가 경고를 읽고 확인한 뒤에만 부른다(acknowledged). 판정과 닫기를 같은 락 안에서 하므로 판정과 닫기 사이에
    // 다른 호출이 끼어들 수 없다. 항목이 profileId 의 편집을 막는 항목이 아니면 닫지 않는다 - 다른 팀원의 항목을 이
    // 화면에서 닫는 길은 없다. 거절하면 false 와 플레이어에게 보여 줄 말(message)을 준다. 던지지 않는다.
    public bool TryCloseByPlayer(MongoId sessionId, string journalId, string profileId, bool acknowledged, out string message)
    {
        message = string.Empty;
        // 빈 profileId 는 BlocksEditing 이 "모르면 막는다" 로 true 를 주므로, 비어 있으면 어떤 항목에도 맞아 버린다.
        // 어느 팀원의 항목인지 모르는 닫기 요청은 여기서 끊는다.
        if (string.IsNullOrWhiteSpace(journalId) || string.IsNullOrWhiteSpace(profileId))
        {
            message = "닫을 기록이나 팀원을 알 수 없어 닫지 않았습니다.";
            return false;
        }

        try
        {
            lock (sync)
            {
                var entry = Load(sessionId).FirstOrDefault(e => e.JournalId == journalId);
                if (entry == null)
                {
                    message = TransferEntryClosePolicy.RefusalMessage(TransferCloseVerdict.AlreadyClosed);
                    return false;
                }

                if (!TransferJournalPolicy.BlocksEditing(entry.Stage, entry.SourceProfileId, entry.DestinationProfileId, profileId))
                {
                    message = "이 기록은 이 팀원의 편집을 막고 있지 않아 여기서 닫을 수 없습니다.";
                    return false;
                }

                var verdict = JudgeClose(entry);
                if (!TransferEntryClosePolicy.MayCloseNow(verdict, acknowledged))
                {
                    message = acknowledged
                        ? TransferEntryClosePolicy.RefusalMessage(verdict)
                        : "경고를 확인하지 않아 닫지 않았습니다.";
                    logger.Warning($"[STM] 플레이어의 전송 항목 닫기 요청을 거절했다: 항목 '{journalId}' / 판정 {verdict} / 확인 {acknowledged}.");
                    return false;
                }

                // 기록이 남는 닫기다: 나중에 아이템이 이상하다는 말이 나왔을 때 무엇이 닫혔는지 로그에서 찾을 수 있어야 한다.
                logger.Warning(
                    $"[STM] 플레이어가 멈춘 전송 항목을 직접 닫기를 시도한다(경고 확인함). 항목 '{entry.JournalId}' / 종류 '{entry.Kind}' / 단계 {entry.Stage} / " +
                    $"시작 {FormatStart(entry.CreatedAtUnixSeconds)}(UTC) / 출처 프로필 '{entry.SourceProfileId}' / 목적지 프로필 '{entry.DestinationProfileId}' / " +
                    $"아이템 [{string.Join(", ", entry.ItemIds ?? [])}] / 목적지 기록 id [{string.Join(", ", entry.DestinationItemIds ?? [])}]. " +
                    "아이템은 복구되지 않는다: 기록만 지운다.");
                // 락 안에서 한 번 더 판정하고 닫는다(CloseIf 의 판정 인자). 같은 스레드의 재진입이라 다른 호출이 끼지 않는다.
                var closed = CloseIf(sessionId, journalId, e => TransferEntryClosePolicy.MayCloseNow(JudgeClose(e), acknowledged), "플레이어 닫기");
                if (!closed) message = "기록을 닫지 못했습니다. 서버 로그를 확인하세요.";
                return closed;
            }
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 플레이어의 전송 항목 닫기 중 오류. 항목 '{journalId}' 을(를) 그대로 둔다: {ex}");
            message = "기록을 닫지 못했습니다. 서버 로그를 확인하세요.";
            return false;
        }
    }

    // 락 안에서 부른다.
    private TransferCloseVerdict JudgeClose(StmTransferJournalEntry entry) =>
        TransferEntryClosePolicy.Judge(entry.Stage, reportedStopped.Contains(entry.JournalId), Decide(entry));

    // 사실 모으기. 판정 자체는 TransferJournalPolicy.DecideFromInventories(순수)가 한다 - 여기는 id 목록을 꺼내 넘길 뿐이다.
    // 목적지는 기록된 DestinationItemIds 로 본다. "전부 있다" 와 "일부만 있다" 를 따로 본다: 일부만 있는 쪽은 일부가 옮겨졌거나 사라진
    // 반쯤 된 상태이므로 어느 쪽 사실도 아니다. 프로필을 못 읽어도 같다 -> 둘 다 멈추고 보고한다.
    private RecoveryDecision Decide(StmTransferJournalEntry entry) =>
        TransferJournalPolicy.DecideFromInventories(
            TransferJournalPolicy.ParseStage(entry.Stage), entry.ItemIds, entry.DestinationItemIds,
            InventoryIds(entry.SourceProfileId), InventoryIds(entry.DestinationProfileId));

    // 인메모리 SaveServer 프로필(= 기동 직후엔 디스크에서 막 읽은 것)의 PMC 인벤토리 아이템 id 들. 읽을 수 없으면 null.
    private List<string>? InventoryIds(string profileId)
    {
        try
        {
            var profiles = saveServer.GetProfiles();
            if (!profiles.TryGetValue(new MongoId(profileId), out var profile)) return null;
            var items = profile?.CharacterData?.PmcData?.Inventory?.Items;
            return items?.Select(i => i.Id.ToString()).ToList();
        }
        catch (Exception ex)
        {
            logger.Warning($"[STM] 프로필 '{profileId}' 의 인벤토리를 읽지 못했다: {ex.Message}");
            return null;
        }
    }

    // 터무니없는 시각이 FromUnixTimeSeconds 를 던지게 해서 사람에게 가는 유일한 보고줄이 사라지면 안 된다.
    private static string FormatStart(long unixSeconds)
    {
        try { return DateTimeOffset.FromUnixTimeSeconds(unixSeconds).ToString("u"); }
        catch (ArgumentOutOfRangeException) { return $"알 수 없음(unix {unixSeconds})"; }
    }

    private bool Close(MongoId sessionId, string journalId) => CloseIf(sessionId, journalId, _ => true, "복구");

    // 단계 판정을 락 안에서 다시 확인하고 닫는다. 판정이 거절하면 항목을 그대로 둔다.
    private bool CloseIf(MongoId sessionId, string journalId, Func<StmTransferJournalEntry, bool> allowed, string what)
    {
        try
        {
            lock (sync)
            {
                var entries = Load(sessionId).ToList();
                var entry = entries.FirstOrDefault(e => e.JournalId == journalId);
                if (entry == null) return false;
                if (!allowed(entry))
                {
                    logger.Error($"[STM] 전송 '{journalId}' {what} 거절: 현재 상태(단계 {entry.Stage} / 출처 쓰기 시작 {entry.SourceWriteStarted})에서는 허용되지 않는다. 항목을 그대로 둔다.");
                    return false;
                }
                entries.Remove(entry);
                Save(sessionId, entries);
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 전송 '{journalId}' 항목을 닫지 못했다: {ex}");
            return false;
        }
    }

    // 문서 없음(미결 없음 -> 빈 목록)과 "entries" 가 없거나 null 인 문서(읽을 수 없음 -> 던진다)를 구별한다.
    // 빈 목록으로 읽고 덮어쓰면 열려 있던 항목이 통째로 사라진다 - 영수증 저장소와 같은 이유.
    private List<StmTransferJournalEntry> Load(MongoId sessionId)
    {
        var doc = storage.ReadSharedDocument<StmTransferJournalDocument>(sessionId, Document);
        if (doc is null) return [];
        return doc.Entries ?? throw new InvalidDataException($"Transfer journal document has no readable 'entries' list: {Document}");
    }

    private void Save(MongoId sessionId, List<StmTransferJournalEntry> entries) =>
        storage.WriteSharedDocument(sessionId, Document, new StmTransferJournalDocument { Entries = entries });
}
