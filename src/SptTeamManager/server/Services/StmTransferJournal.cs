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

    // 두 저장이 둘 다 끝난 "뒤" 에 부른다. 표시를 못 남기면 false - 항목은 Prepared 로 남아 복구가 사실(실물)을
    // 보고 정하므로, 표시 실패가 아이템을 해치지는 않는다.
    public bool MarkCommitted(MongoId sessionId, string journalId) =>
        Advance(sessionId, journalId, TransferStage.Committed);

    // readBackMatches: 호출자가 SaveProfile 뒤 디스크에서 다시 읽어 기대와 대조한 결과. 저널은 프로필을 읽는
    // 방법을 모른다 - 대신 대조가 통과했다는 증명 없이는 항목을 닫지 않는다.
    // false 이거나 대조가 던지면 항목은 열려 있고 편집은 계속 막힌다.
    public bool Verify(MongoId sessionId, string journalId, Func<bool> readBackMatches)
    {
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
        return Close(sessionId, journalId);
    }

    // 던지지 않는다. 읽을 수 없으면 true("열린 항목이 있다") - 편집을 거절하는 비용은 편집 한 번이고, 깨진 저널
    // 위에서 허락하는 비용은 아이템이다. 매 편집 시도마다 불리므로 문서 하나를 읽는 것 말고는 하지 않는다.
    public bool HasOpenEntry(MongoId sessionId, string profileId)
    {
        try
        {
            lock (sync)
                return Load(sessionId).Any(e => TransferJournalPolicy.BlocksEditing(e.Stage, e.SourceProfileId, e.DestinationProfileId, profileId));
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 전송 저널을 읽을 수 없어 프로필 '{profileId}' 에 열린 항목이 있는 것으로 친다(편집 차단): {ex.Message}");
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
                    logger.Error(
                        $"[STM] 끝나지 않은 전송 '{entry.JournalId}'({entry.Kind}, 단계 {entry.Stage}) 을 자동으로 정할 수 없다. " +
                        $"출처 프로필 '{entry.SourceProfileId}' 와 목적지 프로필 '{entry.DestinationProfileId}' 의 편집을 막는다. " +
                        $"아이템 {string.Join(", ", entry.ItemIds ?? [])} 이(가) 복제됐거나 사라졌을 수 있다 - 두 프로필의 스태시/장비를 직접 확인한 뒤 " +
                        $"stm-transfer-journal.json 의 이 항목을 지워야 편집이 풀린다.");
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

    // 사실 모으기. "전부 있다" 와 "일부만 있다" 를 따로 본다: 일부만 있는 쪽은 일부가 옮겨졌거나 사라진
    // 반쯤 된 상태이므로 어느 쪽 사실도 아니다. 프로필을 못 읽어도 같다 -> 둘 다 멈추고 보고한다.
    private RecoveryDecision Decide(StmTransferJournalEntry entry)
    {
        if (entry.ItemIds == null || entry.ItemIds.Count == 0)
            return RecoveryDecision.StopAndReport;
        var src = Presence(entry.SourceProfileId, entry.ItemIds);
        var dst = Presence(entry.DestinationProfileId, entry.ItemIds);
        if (src == null || dst == null || src == ItemPresence.Partial || dst == ItemPresence.Partial)
            return RecoveryDecision.StopAndReport;
        return TransferJournalPolicy.DecideRecovery(
            TransferJournalPolicy.ParseStage(entry.Stage), src == ItemPresence.All, dst == ItemPresence.All);
    }

    private enum ItemPresence { None, Partial, All }

    // 인메모리 SaveServer 프로필(= 기동 직후엔 디스크에서 막 읽은 것)의 PMC 인벤토리에서 본다. 읽을 수 없으면 null.
    private ItemPresence? Presence(string profileId, IReadOnlyList<string> itemIds)
    {
        try
        {
            var profiles = saveServer.GetProfiles();
            if (!profiles.TryGetValue(new MongoId(profileId), out var profile)) return null;
            var items = profile?.CharacterData?.PmcData?.Inventory?.Items;
            if (items == null) return null;
            var present = new HashSet<string>(items.Select(i => i.Id.ToString()), StringComparer.OrdinalIgnoreCase);
            var found = itemIds.Count(id => present.Contains(id));
            return found == 0 ? ItemPresence.None : found == itemIds.Count ? ItemPresence.All : ItemPresence.Partial;
        }
        catch (Exception ex)
        {
            logger.Warning($"[STM] 프로필 '{profileId}' 의 인벤토리를 읽지 못했다: {ex.Message}");
            return null;
        }
    }

    private bool Advance(MongoId sessionId, string journalId, TransferStage stage)
    {
        try
        {
            lock (sync)
            {
                var entries = Load(sessionId).ToList();
                var index = entries.FindIndex(e => e.JournalId == journalId);
                if (index < 0) return false;
                entries[index] = entries[index] with { Stage = stage.ToString() };
                Save(sessionId, entries);
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 전송 '{journalId}' 을(를) {stage} 로 표시하지 못했다: {ex}");
            return false;
        }
    }

    private bool Close(MongoId sessionId, string journalId)
    {
        try
        {
            lock (sync)
            {
                var entries = Load(sessionId).ToList();
                if (entries.RemoveAll(e => e.JournalId == journalId) == 0) return false;
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
