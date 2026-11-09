using System;
using System.Collections.Generic;

namespace pitTeam.Shared
{
    // 편집기가 끝난 뒤의 **그 팀원 인벤토리 전체**를 한 아이템씩 줄인 것. 클라이언트가 EFT 트리를 평평하게
    // 펴서 만들고, 서버는 디스크의 프로필에서 같은 모양을 만든다.
    public sealed record InventoryItemState(string? Id, string? TemplateId, string? ParentId, string? SlotId, int StackCount);

    // Allowed == false 면 Reason 이 플레이어가 읽는 한 줄이다. LostItemIds 는 **사라지는** 아이템들이고,
    // 거절 사유가 아니라 호출자가 로그에 남길 사실이다(버리기·탄박스 소비는 정당한 조작이다).
    // PreservedItemIds 는 **편집기가 보여 주지 않아 제출본에 없는** 아이템들이다 - 호출자는 그것을
    // 원래 객체 그대로(원래 부모·자리·수량) 최종 목록에 다시 붙여야 한다. 지우면 안 된다.
    public sealed record InventoryApplyVerdict(bool Allowed, string Reason, IReadOnlyList<string> LostItemIds, IReadOnlyList<string> PreservedItemIds);

    // 계획 5 Task B 의 판정. **조작의 종류를 세지 않는다** - 이동·나누기·합치기·열기·버리기를 하나씩
    // 열어 주는 방식은 셀 때마다 빠지고, 다음 패치에서 EFT 가 조작을 더하면 또 빠진다. 대신 최종 상태를
    // 받아 "이 변화가 정당한가" 만 본다.
    //
    // 막는 것은 셋이다:
    //   1. **늘어나는 것** - tpl 별 총 수량이 before 보다 많으면 복제다.
    //   2. **바깥에서 들어오는 것** - before 에 없던 종류가 생기면 플레이어 스태시에서 온 것이다.
    //   3. **구조 손상** - 주머니·보안 컨테이너·완장·칼집·인식표가 없어지거나 다른 자리로 가면 EFT 가
    //      그 프로필의 적재를 거부할 수 있다. 플레이어 화면에서도 그것들은 움직이지 않는다.
    // 그리고 트리가 멀쩡한지(두 루트에 다 닿는지, 고리가 없는지)를 본다.
    //
    // **줄어드는 것은 막지 않는다 - 단, 편집기가 들고 있던 것만.** 플레이어는 자기 화면에서 버리고
    // 탄박스를 소비한다. 그러나 **편집기가 보여 주지도 않은 아이템**은 플레이어가 버린 것일 수 없다:
    // 2026-11-03 실측 사고에서 편집기가 EFT 역직렬화에 걸린 아이템을 건너뛰고 열렸고, 최종 상태를
    // 제출하자 그 68개가 프로필에서 지워졌다. 그래서 호출자는 **편집기가 열릴 때 실제로 들고 있던
    // id 목록**(loadedItemIds)을 함께 준다. 허용된 손실의 id 는 전부 돌려주어 호출자가 로그에 남긴다.
    //
    // 그 목록 **밖**의 손실은 **거절이 아니라 보존**이다(PreservedItemIds). 한때 거절이었는데,
    // 그러면 편집기가 못 읽는 아이템이 하나라도 있는 팀원은 **영영 저장할 수 없었다** - 2026-11-09
    // 실측: Sarge 의 Mk18 탄창 하나가 탄 더미를 둘(6발 + 4발) 들고 있었고 편집기가 둘째 더미를
    // 싣지 못해, 스태시 안에서 자리만 옮기는 저장까지 전부 막혔다. 지키려던 것은 "못 본 것을 지우지
    // 않는다" 이지 "저장을 막는다" 가 아니다. 보존이 그것을 그대로 지키면서 편집을 막지 않는다.
    // 목록을 모르면(비면) 손실 전부가 보존이다(닫힌 쪽 - 아무것도 지우지 않는다).
    //
    // 보존은 **붙일 자리가 남아 있을 때만** 할 수 있다. 못 본 아이템의 부모 사슬이 끊겼으면(부모를
    // 플레이어가 버렸으면) 고아가 되므로 그때는 거절한다.
    public static class ProfileInventoryApplyPolicy
    {
        // 부모를 따라 올라갈 때의 걸음 제한. 손상된 트리(고리)에서 돌지 않게 한다.
        private const int MaxDepth = 64;

        private static InventoryApplyVerdict Reject(string reason)
            => new InventoryApplyVerdict(false, reason, Array.Empty<string>(), Array.Empty<string>());

        public static InventoryApplyVerdict Validate(
            IReadOnlyList<InventoryItemState>? before,
            IReadOnlyList<InventoryItemState>? after,
            string? stashRootId,
            string? equipmentRootId,
            IReadOnlyCollection<string>? loadedItemIds,
            GearSlotFacts? slotFacts = null)
        {
            if (before == null || before.Count == 0) return Reject("편집 전 상태를 읽을 수 없어 저장하지 않습니다.");
            if (after == null || after.Count == 0) return Reject("편집기의 상태를 읽을 수 없어 저장하지 않습니다.");
            if (string.IsNullOrWhiteSpace(stashRootId)) return Reject("이 프로필의 스태시 루트를 읽을 수 없어 저장하지 않습니다.");
            if (string.IsNullOrWhiteSpace(equipmentRootId)) return Reject("이 프로필의 장비 루트를 읽을 수 없어 저장하지 않습니다.");

            var stashRoot = stashRootId!.Trim();
            var equipmentRoot = equipmentRootId!.Trim();

            var beforeById = new Dictionary<string, InventoryItemState>(StringComparer.Ordinal);
            foreach (var item in before)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id)) return Reject("편집 전 상태에 id 가 없는 아이템이 있어 저장하지 않습니다.");
                if (beforeById.ContainsKey(item.Id!)) return Reject($"편집 전 상태에 같은 id '{item.Id}' 가 두 번 있어 저장하지 않습니다.");
                beforeById.Add(item.Id!, item);
            }

            var afterById = new Dictionary<string, InventoryItemState>(StringComparer.Ordinal);
            foreach (var item in after)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.Id)) return Reject("저장할 상태에 id 가 없는 아이템이 있어 저장하지 않습니다.");
                if (afterById.ContainsKey(item.Id!)) return Reject($"저장할 상태에 같은 id '{item.Id}' 가 두 번 있어 저장하지 않습니다.");
                if (string.IsNullOrWhiteSpace(item.TemplateId)) return Reject($"아이템 '{item.Id}' 의 종류를 읽을 수 없어 저장하지 않습니다.");
                afterById.Add(item.Id!, item);
            }

            // 두 루트는 그대로여야 한다. 루트가 빠지면 그 트리가 통째로 사라지고, 루트가 다른 것 밑으로
            // 들어가면 트리 하나가 다른 트리의 일부가 된다.
            foreach (var rootId in new[] { stashRoot, equipmentRoot })
            {
                if (!afterById.TryGetValue(rootId, out var root))
                {
                    return Reject($"저장할 상태에 루트 '{rootId}' 가 없어 저장하지 않습니다.");
                }

                if (!string.IsNullOrWhiteSpace(root.ParentId))
                {
                    return Reject($"루트 '{rootId}' 가 다른 아이템 밑으로 들어가 있어 저장하지 않습니다.");
                }
            }

            // 모든 아이템이 두 루트 중 하나에 닿는가. 닿지 않으면 고아이거나 고리다 - 둘 다 그 가지가
            // 프로필에서 사라진 것처럼 보인다.
            foreach (var item in after)
            {
                var id = item.Id!;
                if (string.Equals(id, stashRoot, StringComparison.Ordinal) || string.Equals(id, equipmentRoot, StringComparison.Ordinal)) continue;

                var current = item;
                var reachedRoot = false;
                for (var steps = 0; steps < MaxDepth; steps++)
                {
                    var parentId = current.ParentId;
                    if (string.IsNullOrWhiteSpace(parentId)) break;

                    if (string.Equals(parentId, stashRoot, StringComparison.Ordinal)
                        || string.Equals(parentId, equipmentRoot, StringComparison.Ordinal))
                    {
                        reachedRoot = true;
                        break;
                    }

                    if (!afterById.TryGetValue(parentId!, out var parent)) break;

                    current = parent;
                }

                if (!reachedRoot)
                {
                    return Reject($"아이템 '{id}' 이(가) 스태시와 장비 어느 쪽에도 매달려 있지 않아 저장하지 않습니다.");
                }
            }

            // 같은 id 의 종류가 바뀌면 그것은 더 이상 같은 아이템이 아니다.
            foreach (var pair in afterById)
            {
                if (!beforeById.TryGetValue(pair.Key, out var original)) continue;
                if (!string.Equals(original.TemplateId, pair.Value.TemplateId, StringComparison.Ordinal))
                {
                    return Reject($"아이템 '{pair.Key}' 의 종류가 '{original.TemplateId}' 에서 '{pair.Value.TemplateId}' 로 바뀌어 저장하지 않습니다.");
                }
            }

            // 제출본에 없는 아이템을 둘로 가른다: 편집기가 **들고 있던** 것의 손실은 플레이어가 버린
            // 것(lost)이고, **보여 주지도 않은** 것의 손실은 손실이 아니라 보존(preserved)이다.
            // 아래 검사들이 "없어졌다" 를 묻기 전에 먼저 가른다 - 보존되는 것은 없어지는 것이 아니다.
            var loaded = new HashSet<string>(StringComparer.Ordinal);
            if (loadedItemIds != null)
            {
                foreach (var id in loadedItemIds)
                {
                    if (!string.IsNullOrWhiteSpace(id)) loaded.Add(id!);
                }
            }

            var preserved = new HashSet<string>(StringComparer.Ordinal);
            var lost = new List<string>();
            foreach (var item in before)
            {
                if (afterById.ContainsKey(item.Id!)) continue;
                if (loaded.Contains(item.Id!)) lost.Add(item.Id!);
                else preserved.Add(item.Id!);
            }

            // 보존은 붙일 자리가 남아 있을 때만 할 수 있다. 부모 사슬을 올라가 **제출본에 남은 부모**나
            // 루트에 닿아야 한다(중간이 전부 보존되는 것은 함께 붙으므로 괜찮다).
            foreach (var id in preserved)
            {
                var current = beforeById[id];
                var anchored = false;
                for (var steps = 0; steps < MaxDepth; steps++)
                {
                    var parentId = current.ParentId;
                    if (string.IsNullOrWhiteSpace(parentId)) break;
                    if (string.Equals(parentId, stashRoot, StringComparison.Ordinal)
                        || string.Equals(parentId, equipmentRoot, StringComparison.Ordinal)
                        || afterById.ContainsKey(parentId!))
                    {
                        anchored = true;
                        break;
                    }

                    if (!preserved.Contains(parentId!)) break;
                    if (!beforeById.TryGetValue(parentId!, out var parent)) break;
                    current = parent;
                }

                if (!anchored)
                {
                    return Reject($"아이템 '{id}' 은(는) 편집기가 보여 주지 않은 것인데 그것이 들어 있던 자리가 없어져 저장하지 않습니다.");
                }
            }

            // 구조를 이루는 자리는 같은 id 가 같은 부모의 같은 자리에 그대로 있어야 한다.
            foreach (var item in before)
            {
                if (!ProfileGearEditPolicy.IsStructuralSlot(item.SlotId)) continue;
                if (preserved.Contains(item.Id!)) continue;
                if (!afterById.TryGetValue(item.Id!, out var moved))
                {
                    return Reject($"'{item.SlotId}' 의 아이템 '{item.Id}' 이(가) 없어졌습니다. 프로필에서 떼어낼 수 없는 자리라 저장하지 않습니다.");
                }

                if (!string.Equals(moved.ParentId, item.ParentId, StringComparison.Ordinal)
                    || !string.Equals(moved.SlotId, item.SlotId, StringComparison.Ordinal))
                {
                    return Reject($"아이템 '{item.Id}' 은(는) '{item.SlotId}' 에서 움직일 수 없는 자리라 저장하지 않습니다.");
                }
            }

            // 종류별 총 수량. 늘면 복제이고, before 에 없던 종류면 바깥에서 들어온 것이다.
            var beforeTotals = TotalsByTemplate(before);
            var afterTotals = TotalsByTemplate(after);
            foreach (var pair in afterTotals)
            {
                if (!beforeTotals.TryGetValue(pair.Key, out var allowed))
                {
                    return Reject($"'{pair.Key}' 는 이 팀원이 갖고 있던 종류가 아니어서 저장하지 않습니다.");
                }

                if (pair.Value > allowed)
                {
                    return Reject($"'{pair.Key}' 의 수가 {allowed} 에서 {pair.Value} 로 늘어 저장하지 않습니다. 편집은 아이템을 만들지 않습니다.");
                }
            }

            // 자리의 적법성. 보존이 지키지 못하는 것이다 - id 집합과 종류별 총량이 그대로여도 헬멧이
            // 탄창 안에 앉은 프로필은 EFT 가 적재를 거부할 수 있다(관문 17 의 '규칙을 어긴 스태시 항목'
            // 과 같은 부류다).
            var misplaced = CheckPlacements(before, after, afterById, slotFacts);
            if (misplaced != null) return Reject(misplaced);

            // 사라지는 것(lost)과 그대로 두는 것(preserved)은 위에서 이미 갈랐다. 둘 다 돌려준다:
            // 호출자는 lost 를 로그에 남기고, preserved 를 원래 객체 그대로 최종 목록에 다시 붙인다.
            var preservedIds = new List<string>();
            foreach (var item in before)
            {
                if (preserved.Contains(item.Id!)) preservedIds.Add(item.Id!);
            }

            return new InventoryApplyVerdict(true, string.Empty, lost, preservedIds);
        }

        // 제출된 자리가 적법한가. 어휘(슬롯·격자·필터·용량)의 집은 ProfileGearEditPolicy 하나이고
        // 여기서는 그것을 읽는다(불변식 8).
        //
        // **이동 목록 경로와 극성이 반대다.** 거기서는 모르면 거부였다 - 요청 한 건을 거절하는 비용이었다.
        // 여기서 같은 극성을 쓰면 제출본은 **두 트리 전체**라서, 우리가 모델하지 못하는 자리에 앉은 기존
        // 아이템 하나가 그 프로필의 **모든 저장을 영구히** 막는다(열린 항목 10 이 경고한 자리다).
        // 그래서 두 가지로 좁힌다:
        //   1. **이번 제출이 옮긴 자리만** 본다. 자리가 그대로인 아이템은 EFT 가 이미 그 프로필을
        //      적재하고 있다는 뜻이고, 우리가 만든 상태가 아니다.
        //   2. **틀렸다고 증명된 것만** 거절한다. 부모의 종류를 읽지 못했거나, 그 자리를 찾지 못했거나,
        //      필터가 비었거나, 용량이 0 이면 아무것도 말하지 않고 통과시킨다. 특히 **"그런 자리가 없다"
        //      는 거절 근거가 아니다** - 아래 definition == null 의 주석에 그 실측 근거가 있다.
        //
        // 그 대가로 이것은 **혼자 서는 관문이 아니다** - 데이터베이스를 읽지 못하면 자리는 보지 못하고
        // 보존 검사만 남는다. 보존을 거드는 두 번째 그물로 읽어라.
        private static string? CheckPlacements(
            IReadOnlyList<InventoryItemState> before,
            IReadOnlyList<InventoryItemState> after,
            IReadOnlyDictionary<string, InventoryItemState> afterById,
            GearSlotFacts? slotFacts)
        {
            if (slotFacts == null) return null;

            var placeBefore = new Dictionary<string, (string? Parent, string? Slot)>(StringComparer.Ordinal);
            foreach (var item in before)
            {
                if (item?.Id != null) placeBefore[item.Id] = (item.ParentId, item.SlotId);
            }

            // (부모, 자리) 별로 누가 앉아 있는가. 점유와 용량 합을 이것으로 센다.
            var occupants = new Dictionary<(string Parent, string Slot), List<InventoryItemState>>();
            foreach (var item in after)
            {
                if (string.IsNullOrWhiteSpace(item.ParentId) || string.IsNullOrWhiteSpace(item.SlotId)) continue;
                var seat = (item.ParentId!.Trim(), item.SlotId!.Trim().ToLowerInvariant());
                if (!occupants.TryGetValue(seat, out var sitting))
                {
                    occupants[seat] = sitting = new List<InventoryItemState>();
                }

                sitting.Add(item);
            }

            foreach (var item in after)
            {
                var id = item.Id!;
                if (string.IsNullOrWhiteSpace(item.ParentId) || string.IsNullOrWhiteSpace(item.SlotId)) continue;

                // 좁힘 1. 자리가 그대로면 보지 않는다.
                if (placeBefore.TryGetValue(id, out var was)
                    && string.Equals(was.Parent, item.ParentId, StringComparison.Ordinal)
                    && string.Equals(was.Slot, item.SlotId, StringComparison.Ordinal))
                {
                    continue;
                }

                var parentId = item.ParentId!.Trim();
                var slot = item.SlotId!.Trim();

                // 부모가 제출본에 없으면 위의 트리 검사가 이미 거절했다.
                if (!afterById.TryGetValue(parentId, out var parent)) continue;
                var parentTemplateId = parent.TemplateId;
                var itemTemplateId = item.TemplateId;
                if (string.IsNullOrWhiteSpace(parentTemplateId) || string.IsNullOrWhiteSpace(itemTemplateId)) continue;

                // 종류 필터는 **그 아이템의 분류를 알 때만** 판정할 수 있다. 조상 사슬을 걷지 못하면
                // "받지 않는 종류" 와 "모르는 종류" 가 같은 모양이 되고, 그러면 모드가 더한 아이템이
                // 전부 거부된다(ProfileGearEditPolicy.KnowsCategoryOf 의 실측 근거).
                var knowsKind = ProfileGearEditPolicy.KnowsCategoryOf(slotFacts, itemTemplateId);

                // 격자(가방·리그·주머니·스태시의 칸)가 먼저다. 여러 개를 담으므로 점유를 묻지 않고,
                // 자리가 실제로 남아 있는지는 좌표의 문제라 여기서 셈하지 않는다(EFT 편집기가 센다).
                var grid = ProfileGearEditPolicy.FindGridDefinition(slotFacts, parentTemplateId, slot);
                if (grid != null)
                {
                    if (knowsKind && grid.AllowedTemplateIds != null && grid.AllowedTemplateIds.Count > 0
                        && !ProfileGearEditPolicy.FilterAccepts(slotFacts, grid.AllowedTemplateIds, itemTemplateId!))
                    {
                        return $"아이템 '{id}' 은(는) '{parentId}' 의 '{slot}' 칸에 들어가지 않는 종류라서 저장하지 않습니다.";
                    }

                    // 받지 않는 종류(ExcludedFilter). 가방 안에 가방을 넣지 못하는 것이 이것이다.
                    if (grid.ExcludedTemplateIds != null && grid.ExcludedTemplateIds.Count > 0
                        && ProfileGearEditPolicy.FilterAccepts(slotFacts, grid.ExcludedTemplateIds, itemTemplateId!))
                    {
                        return $"아이템 '{id}' 은(는) '{parentId}' 의 '{slot}' 칸이 받지 않는 종류라서 저장하지 않습니다.";
                    }

                    continue;
                }

                // 자리를 찾지 못하면 아무것도 말하지 않는다. **"그런 자리가 없다" 를 거절 근거로 쓰지
                // 않는다** - 2026-11-04 에 실제 프로필 여섯 개로 재 보고 뺐다. 그 거절은 "자리를 정의하는
                // 곳을 우리가 전부 안다" 에 기대는데 그 전제가 두 번 깨졌다:
                //   (ㄱ) 탄박스는 `StackSlots` 에 자리를 두고, 그것을 읽지 않아 **87쌍**이 없는 자리로
                //        읽혔다(읽도록 고쳤다).
                //   (ㄴ) 그 프로필들의 부모 템플릿 **51종(1577쌍)** 은 정적 데이터베이스에 아예 없다 -
                //        설치된 모드가 기동 때 주입하는 템플릿이다. 런타임 데이터베이스가 무엇을
                //        정의하는지는 파일만 보고 말할 수 없다.
                // 틀린 거절의 값은 "그 팀원은 다시 저장할 수 없다" 이고, 얻는 것은 아래 네 검사가 대개
                // 함께 잡는다. 그래서 하지 않는다(열린 항목 10 의 경고를 그대로 따른다).
                var definition = ProfileGearEditPolicy.FindSlotDefinition(slotFacts, parentTemplateId, slot);
                if (definition == null) continue;

                if (knowsKind && definition.AllowedTemplateIds != null && definition.AllowedTemplateIds.Count > 0
                    && !ProfileGearEditPolicy.FilterAccepts(slotFacts, definition.AllowedTemplateIds, itemTemplateId!))
                {
                    return $"아이템 '{id}' 은(는) '{parentId}' 의 '{slot}' 슬롯에 들어가지 않는 종류라서 저장하지 않습니다.";
                }

                var seatKey = (parentId, slot.ToLowerInvariant());
                var seated = occupants.TryGetValue(seatKey, out var list) ? list : null;

                // 탄을 색인으로 담는 자리. 더미 여럿이 정상이므로(다른 구경을 섞는 것이 플레이어 화면의
                // 동작이다) 점유를 묻지 않고 **합**을 용량과 견준다 - 하나씩 보면 20+20 이 30발 탄창을
                // 지나간다.
                if (definition.AmmoCapacity is int capacity)
                {
                    if (capacity > 0 && seated != null)
                    {
                        var total = 0;
                        foreach (var occupant in seated) total += occupant.StackCount > 0 ? occupant.StackCount : 1;
                        if (total > capacity)
                        {
                            return $"'{parentId}' 의 '{slot}' 자리는 {capacity}발까지인데 {total}발이 들어가 저장하지 않습니다.";
                        }
                    }

                    continue;
                }

                // 일반 슬롯에는 하나만 앉는다.
                if (seated != null && seated.Count > 1)
                {
                    return $"'{parentId}' 의 '{slot}' 슬롯에 아이템 {seated.Count}개가 함께 앉아 저장하지 않습니다.";
                }
            }

            return null;
        }

        private static Dictionary<string, int> TotalsByTemplate(IReadOnlyList<InventoryItemState> items)
        {
            var totals = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var item in items)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.TemplateId)) continue;
                var count = item.StackCount > 0 ? item.StackCount : 1;
                totals.TryGetValue(item.TemplateId!, out var sum);
                totals[item.TemplateId!] = sum + count;
            }

            return totals;
        }
    }
}
