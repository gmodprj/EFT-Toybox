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
// 쓰는 쪽이 지켜야 할 순서 (이 순서가 안전 방향이다 - 끊기면 복제 대신 소실로 기운다):
//   1. Prepare            - 저널에 먼저 적는다. 실패하면 던진다. 던졌으면 아이템을 건드리지 않는다.
//   2. 출처에서 뺀다       - SaveServer 인메모리 객체를 고치고 SaveProfile. (빼는 쪽 먼저)
//   3. 목적지에 넣는다     - 같은 방식. (넣는 쪽 나중)
//   (2 전에) RecordDestinationItemIds - 목적지에 실제로 쓸 id 를 기록한다. 안 하면 MarkCommitted 가 거절한다.
//   (취소)  Abort         - 2 전에만. 출처를 건드린 뒤에는 거절된다.
//   4. MarkCommitted      - 두 저장이 "둘 다" 끝난 뒤에만 부른다. 그 전에 부르면 복구가 거짓 사실 위에서 판정한다.
//   5. Verify             - SaveProfile 뒤 디스크에서 다시 읽은 결과를 대조해 맞으면 항목을 닫는다.
// 1 에서 4 사이 어디서 끊겨도 항목은 열린 채 남고, 다음 기동의 RecoverOpenEntries 가 TransferJournalPolicy 로
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

    // 목적지에 실제로 쓸 id 가 확정되면(재매김이 있었다면 재매김 뒤의 id) 목적지에 쓰기 "전에" 부른다. 복구가 이 기록으로
    // 목적지를 본다. 출처 ItemIds 와 같은 순서·개수여야 하고, 아니면 false(기록하지 않음) - 호출자는 전송을 중단한다.
    // 이미 열린 항목의 Prepared 단계에서만 받는다.
    public bool RecordDestinationItemIds(MongoId sessionId, string journalId, IReadOnlyList<string> destinationItemIds)
    {
        try
        {
            lock (sync)
            {
                var entries = Load(sessionId).ToList();
                var index = entries.FindIndex(e => e.JournalId == journalId);
                if (index < 0) return false;
                if (TransferJournalPolicy.ParseStage(entries[index].Stage) != TransferStage.Prepared) return false;
                if (!TransferJournalPolicy.IsUsableDestinationIds(entries[index].ItemIds, destinationItemIds)) return false;
                entries[index] = entries[index] with { DestinationItemIds = destinationItemIds.Select(i => i.Trim()).ToList() };
                Save(sessionId, entries);
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 전송 '{journalId}' 의 목적지 id 를 기록하지 못했다: {ex}");
            return false;
        }
    }

    // 두 저장이 둘 다 끝난 "뒤" 에 부른다. 표시를 못 남기면 false - 항목은 Prepared 로 남아 복구가 사실(실물)을
    // 보고 정하므로, 표시 실패가 아이템을 해치지는 않는다.
    // 목적지 id 는 여기서 받지 않는다(받으면 목적지 쓰기 "뒤" 에 기록하는 길이 열려, 쓰기 전에 기록한다는 방어가
    // 무력화된다). RecordDestinationItemIds 로 미리 기록하지 않았으면 거절한다 - 재매김이 없는 전송도 같은 id 를
    // 기록한다. 준비 단계가 아니어도 거절한다(TransferJournalPolicy.CanCommit).
    public bool MarkCommitted(MongoId sessionId, string journalId)
    {
        try
        {
            lock (sync)
            {
                var entries = Load(sessionId).ToList();
                var index = entries.FindIndex(e => e.JournalId == journalId);
                if (index < 0) return false;
                var entry = entries[index];
                if (!TransferJournalPolicy.CanCommit(entry.Stage, entry.DestinationItemIds != null))
                {
                    logger.Error($"[STM] 전송 '{journalId}' 을(를) 커밋 표시할 수 없다(단계 {entry.Stage}, 목적지 id 기록 {(entry.DestinationItemIds != null ? "있음" : "없음")}). 항목을 그대로 둔다.");
                    return false;
                }
                entries[index] = entry with { Stage = nameof(TransferStage.Committed) };
                Save(sessionId, entries);
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 전송 '{journalId}' 을(를) 커밋으로 표시하지 못했다: {ex}");
            return false;
        }
    }

    // 커밋 없이 닫는다 - Prepare 뒤 RecordDestinationItemIds 가 실패했거나 호출자가 깨끗이 취소할 때. 준비 단계에서만
    // 된다(TransferJournalPolicy.CanAbort): 그 단계는 실물이 안 움직였다고 약속된 유일한 단계이고, 출처에서 뺀 뒤의
    // 취소는 항목만 지워 소실을 가리는 길이다. 따라서 출처에 쓰기 "전에" 만 부를 수 있다. 커밋 이후는 거절(false).
    public bool Abort(MongoId sessionId, string journalId) =>
        CloseIf(sessionId, journalId, TransferJournalPolicy.CanAbort, "취소");

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
        return CloseIf(sessionId, journalId, TransferJournalPolicy.CanVerify, "확인");
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
                    // 한 줄로 사람이 행동할 수 있게: 어느 파일의 어느 문서인가, 어느 항목인가, 언제 시작됐나, 무엇이 걸려 있나.
                    // 문서는 암호화된 데이터베이스 안의 레코드라 메모장으로 고칠 수 없다 - 그래서 경로와 문서 이름을 둘 다 적는다.
                    var started = DateTimeOffset.FromUnixTimeSeconds(entry.CreatedAtUnixSeconds).ToString("u");
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

    private bool Close(MongoId sessionId, string journalId) => CloseIf(sessionId, journalId, _ => true, "복구");

    // 단계 판정을 락 안에서 다시 확인하고 닫는다. 판정이 거절하면 항목을 그대로 둔다.
    private bool CloseIf(MongoId sessionId, string journalId, Func<string?, bool> stageAllows, string what)
    {
        try
        {
            lock (sync)
            {
                var entries = Load(sessionId).ToList();
                var entry = entries.FirstOrDefault(e => e.JournalId == journalId);
                if (entry == null) return false;
                if (!stageAllows(entry.Stage))
                {
                    logger.Error($"[STM] 전송 '{journalId}' {what} 거절: 현재 단계 {entry.Stage} 에서는 허용되지 않는다. 항목을 그대로 둔다.");
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
