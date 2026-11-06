using System;
using System.Collections.Generic;

namespace pitTeam.Shared
{
    // 두 프로필을 고치는 조작의 3단계. 순서가 곧 안전 방향이다: 실제 이동(빼는 쪽 먼저, 넣는 쪽 나중)은
    // Prepared 와 Committed 사이에서 일어나고, 그 순서라서 끊기면 아이템이 복제되는 대신 사라지는 쪽으로 기운다.
    //   Prepared  - 무엇을 어디서 어디로 옮기는지 적었다. 실물은 아직 안 움직였다.
    //   Committed - 출처에서 뺀 저장과 목적지에 넣은 저장이 둘 다 끝났다(이 표시는 두 번째 저장 "뒤"에 남긴다).
    //   Verified  - 저장 뒤 디스크에서 다시 읽어 대조까지 끝났다. 이 단계의 항목은 닫힌다.
    public enum TransferStage
    {
        Prepared = 0,
        Committed = 1,
        Verified = 2,
    }

    public enum RecoveryDecision
    {
        // 목적지가 이미 가졌다 - 이동이 끝난 상태이므로 저널만 닫으면 된다.
        RollForward,
        // 출처가 아직 가졌고 목적지는 없다 - 이동이 일어나지 않은 상태이므로 저널만 닫으면 된다.
        RollBack,
        // 코드가 정할 수 없다. 사람이 본다. 이 값 말고 "알아서 고친다" 는 결과는 없다.
        StopAndReport,
    }

    // 끊긴 전송을 어느 쪽으로 마무리할지 "사실" 만 보고 정한다. 프로필을 읽지도, 로그를 남기지도, SPT 를 건드리지도
    // 않는 순수 판정이다. 사실을 모으는 일(프로필에서 아이템이 있는지 보기)은 호출자 몫이다.
    // 의도적으로 catch 가 없다: 입력으로 닿을 수 있는 모든 경로가 아래 분기로 끝나고, catch 가 있으면 분기를
    // 지워도 테스트가 통과해 버린다.
    // 열린 결과는 둘뿐이다 - 출처만 가졌다(안 움직임) / 목적지만 가졌다(다 움직임). 그 밖의 모든 모양 -
    // 양쪽 다 가짐(복제 가능, 어느 쪽이 진짜인지 모름), 양쪽 다 없음(사라짐, 코드가 지어낼 수 없음),
    // 단계와 실물이 어긋남, 모르는 단계 - 은 StopAndReport 다.
    public static class TransferJournalPolicy
    {
        // 저장된 문서에서 읽은 단계 이름이 알려진 값이 아닐 때 쓰는 값. Enum 정의 밖이라 DecideRecovery 가 닫는다.
        public const TransferStage UnknownStage = (TransferStage)(-1);

        // Enum.TryParse 를 쓰지 않는다: "7" 같은 숫자 문자열도 통과시켜 정의 밖 값이 알려진 값처럼 보인다.
        public static TransferStage ParseStage(string? name)
        {
            switch (name?.Trim().ToLowerInvariant())
            {
                case "prepared": return TransferStage.Prepared;
                case "committed": return TransferStage.Committed;
                case "verified": return TransferStage.Verified;
                default: return UnknownStage;
            }
        }

        public static RecoveryDecision DecideRecovery(TransferStage stage, bool sourceStillHasItems, bool destinationHasItems)
        {
            if (stage != TransferStage.Prepared && stage != TransferStage.Committed && stage != TransferStage.Verified)
            {
                return RecoveryDecision.StopAndReport;
            }

            // 양쪽 다 가졌다: 복제일 수 있고 어느 쪽이 진짜인지 모른다. 단계와 무관하게 멈춘다.
            if (sourceStillHasItems && destinationHasItems)
            {
                return RecoveryDecision.StopAndReport;
            }

            // 양쪽 다 없다: 빼는 쪽을 먼저 저장하고 넣는 쪽 전에 끊긴 모양. 사라졌다고 사람에게 알린다.
            if (!sourceStillHasItems && !destinationHasItems)
            {
                return RecoveryDecision.StopAndReport;
            }

            // 여기부터는 정확히 한쪽만 가졌다.
            if (destinationHasItems)
            {
                // 다 움직였다. 어느 단계 표시든 사실과 어긋나지 않는다(커밋 표시만 못 남기고 끊겼을 수 있다).
                return RecoveryDecision.RollForward;
            }

            // 출처만 가졌다 = 안 움직였다. 이건 준비 단계에서만 말이 된다. 커밋·확인이라 적혀 있는데
            // 안 움직였다면 저장이 되돌려졌다는 뜻일 수 있어 조용히 닫지 않는다.
            return stage == TransferStage.Prepared ? RecoveryDecision.RollBack : RecoveryDecision.StopAndReport;
        }

        // 이 저널 항목이 profileId 의 편집을 막는가. 의심스러우면 막는다(설계 §6-9): 거절하면 편집 한 번을 잃고,
        // 깨진 저널 위에서 허락하면 아이템을 잃는다. 막지 않는 경우는 딱 하나 - 확인까지 끝난 항목이거나,
        // 끝나지 않았어도 출처·목적지가 둘 다 읽히고 둘 다 이 프로필이 아닌 경우.
        // 프로필 식별자는 앞뒤 공백·대소문자를 무시하고 비교한다(RaidReceiptKeyPolicy 와 같은 방향: 애매하면 같다).
        public static bool BlocksEditing(string? stageName, string? sourceProfileId, string? destinationProfileId, string? profileId)
        {
            if (ParseStage(stageName) == TransferStage.Verified)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(profileId)
                || string.IsNullOrWhiteSpace(sourceProfileId)
                || string.IsNullOrWhiteSpace(destinationProfileId))
            {
                return true;
            }

            var id = profileId!.Trim();
            return string.Equals(sourceProfileId!.Trim(), id, StringComparison.OrdinalIgnoreCase)
                || string.Equals(destinationProfileId!.Trim(), id, StringComparison.OrdinalIgnoreCase);
        }

        // 기록된 목적지 id 를 쓸 수 있는가: 출처와 같은 개수, 비지 않음, 서로 다름. 쓸 수 없는 기록으로 판정하면
        // 엉뚱한 아이템을 "목적지에 있다" 로 읽을 수 있어서, 이 검사는 기록할 때와 읽을 때 둘 다 쓴다.
        public static bool IsUsableDestinationIds(IReadOnlyList<string>? sourceIds, IReadOnlyList<string>? destinationIds)
        {
            if (sourceIds == null || destinationIds == null || destinationIds.Count == 0 || destinationIds.Count != sourceIds.Count)
            {
                return false;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var id in destinationIds)
            {
                if (string.IsNullOrWhiteSpace(id) || !seen.Add(id.Trim()))
                {
                    return false;
                }
            }

            return true;
        }

        // 인벤토리(아이템 id 목록)만 보고 복구를 정하는 순수 판정. 서비스는 SaveServer 에서 id 목록을 꺼내 넘기기만 한다.
        // 출처는 Prepare 때 적은 id 로, 목적지는 "커밋 전에 기록된 실제 저장 id" 로 본다. 기록이 없으면(null) 재매김이
        // 없었다는 전제로 출처 id 와 같게 본다 - 기록 없이 재매김이 일어났다면 목적지가 안 보여 없음/없음이 되어
        // 멈춘다. 새 id 를 추측해 찾지 않는다. 기록이 있는데 쓸 수 없으면 멈춘다. 재매김은 "목적지를 보는 id" 만 바꾼다:
        // 양쪽 다 가짐은 여전히 멈춤이라 재매김이 복제를 설명해 주지 못한다.
        // 인벤토리가 null(읽지 못함)이거나 일부만 있으면 어느 쪽 사실도 아니므로 멈춘다.
        public static RecoveryDecision DecideFromInventories(
            TransferStage stage,
            IReadOnlyList<string>? sourceItemIds,
            IReadOnlyList<string>? recordedDestinationItemIds,
            IEnumerable<string>? sourceInventory,
            IEnumerable<string>? destinationInventory)
        {
            if (sourceItemIds == null || sourceItemIds.Count == 0 || sourceInventory == null || destinationInventory == null)
            {
                return RecoveryDecision.StopAndReport;
            }

            IReadOnlyList<string> destinationIds = sourceItemIds;
            if (recordedDestinationItemIds != null)
            {
                if (!IsUsableDestinationIds(sourceItemIds, recordedDestinationItemIds))
                {
                    return RecoveryDecision.StopAndReport;
                }

                destinationIds = recordedDestinationItemIds;
            }

            var src = Classify(sourceItemIds, sourceInventory);
            var dst = Classify(destinationIds, destinationInventory);
            if (src == Presence.Partial || dst == Presence.Partial)
            {
                return RecoveryDecision.StopAndReport;
            }

            return DecideRecovery(stage, src == Presence.All, dst == Presence.All);
        }

        private enum Presence { None, Partial, All }

        private static Presence Classify(IReadOnlyList<string> wanted, IEnumerable<string> inventory)
        {
            var present = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var id in inventory)
            {
                if (id != null)
                {
                    present.Add(id.Trim());
                }
            }

            var found = 0;
            foreach (var id in wanted)
            {
                if (id != null && present.Contains(id.Trim()))
                {
                    found++;
                }
            }

            return found == 0 ? Presence.None : found == wanted.Count ? Presence.All : Presence.Partial;
        }

        // ---- 전이 판정 ----
        // Prepared 한 단계가 세 가지 다른 상태를 덮는다(안 움직임 / 출처 쓰기가 일어남 / 목적지 쓰기가 일어남). 단계는
        // MarkCommitted 에서야 바뀌므로 단계만으로는 쓰기의 진행을 못 따라간다. 그래서 각 쓰기 "직전" 에 사실(출처 쓰기
        // 시작, 목적지 쓰기 시작)을 저널에 적고, 모든 전이를 그 사실로 판정한다. 사실이 쓰기보다 먼저 남으므로 판정과
        // 쓰기 사이에 창이 없다. 서비스는 락 안에서 이 판정을 통과해야만 사실을 적거나 단계를 바꾸거나 항목을 닫는다.
        // 모르는 단계는 전부 거절이다.

        // 출처 쓰기 시작: 호출자가 출처에서 빼기 "직전" 에 한 번.
        public static bool CanBeginSourceWrite(string? currentStageName, bool sourceWriteStarted) =>
            !sourceWriteStarted && ParseStage(currentStageName) == TransferStage.Prepared;

        // 취소(커밋 없이 닫기): 출처 쓰기가 시작되기 전에만. 시작된 뒤에는 단계가 아직 Prepared 여도 거절 - 아이템이
        // 이미 출처를 떠났을 수 있고, 그때의 취소는 항목만 지워 소실을 가린다.
        public static bool CanAbort(string? currentStageName, bool sourceWriteStarted) =>
            !sourceWriteStarted && ParseStage(currentStageName) == TransferStage.Prepared;

        // 목적지 id 기록: 목적지 쓰기가 시작되기 전에만, 쓸 수 있는 id 만, 그리고 한 번 기록되면 바꿀 수 없다
        // (같은 id 를 다시 기록하는 것은 허용 - 바꾸는 게 아니다). 쓰기 뒤의 기록은 "쓰기 전에 기록" 방어를 무력화한다.
        public static bool CanRecordDestinationIds(
            string? currentStageName, bool destinationWriteStarted, IReadOnlyList<string>? sourceIds,
            IReadOnlyList<string>? recorded, IReadOnlyList<string>? proposed)
        {
            if (destinationWriteStarted || ParseStage(currentStageName) != TransferStage.Prepared)
            {
                return false;
            }

            if (!IsUsableDestinationIds(sourceIds, proposed))
            {
                return false;
            }

            if (recorded == null)
            {
                return true;
            }

            if (recorded.Count != proposed!.Count)
            {
                return false;
            }

            for (var i = 0; i < recorded.Count; i++)
            {
                if (!string.Equals(recorded[i]?.Trim(), proposed[i]?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        // 목적지 쓰기 시작: 호출자가 목적지에 넣기 "직전" 에 한 번. 출처를 먼저 뺐고(빼는 쪽 먼저) 목적지 id 가
        // 기록돼 있어야 한다.
        public static bool CanBeginDestinationWrite(
            string? currentStageName, bool sourceWriteStarted, bool destinationIdsRecorded, bool destinationWriteStarted) =>
            sourceWriteStarted && destinationIdsRecorded && !destinationWriteStarted
            && ParseStage(currentStageName) == TransferStage.Prepared;

        // 커밋: 두 쓰기가 다 시작됐고 목적지 id 가 기록된 Prepared 에서만.
        public static bool CanCommit(string? currentStageName, bool sourceWriteStarted, bool destinationWriteStarted, bool destinationIdsRecorded) =>
            sourceWriteStarted && destinationWriteStarted && destinationIdsRecorded
            && ParseStage(currentStageName) == TransferStage.Prepared;

        // 확인은 커밋된 항목만. 준비 직후 Verify 가 통하면 아무것도 안 옮긴 항목이 "확인됨" 으로 닫힌다.
        public static bool CanVerify(string? currentStageName) => ParseStage(currentStageName) == TransferStage.Committed;
    }
}
