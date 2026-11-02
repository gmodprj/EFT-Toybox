using System;
using System.Collections.Generic;
using System.Linq;

namespace pitTeam.Shared
{
    // 편집 한 번의 요청. 순수 레코드 - SPT 타입을 모른다. TargetParentId 가 비면 "장비 루트 아래" 를 뜻한다.
    // Location(격자 좌표)이 없다: 이 경로는 슬롯에 넣는 조작만 표현한다(아래 주석).
    public sealed record GearEditRequest(string ProfileId, string ItemId, string TargetSlotId, string? TargetParentId);

    // 허용 아니면 사유. 사유는 사람이 읽는 한 줄이고 로그와 화면에 같이 간다.
    public sealed record GearEditVerdict(bool Allowed, string Reason)
    {
        private static readonly GearEditVerdict Ok = new GearEditVerdict(true, string.Empty);

        public static GearEditVerdict Allow() => Ok;

        public static GearEditVerdict Reject(string reason) => new GearEditVerdict(false, reason);
    }

    // 보존 판정의 결과. MissingItemIds 는 "원래 있었는데 어느 쪽에도 없는" 아이템 id - 비어 있으면 보존이다.
    public sealed record StashConservationVerdict(bool Conserved, string Reason, IReadOnlyList<string> MissingItemIds);

    // 아이템 템플릿이 정의한 슬롯 하나. AllowedTemplateIds 는 그 슬롯의 필터가 허락하는 템플릿 id 들이고,
    // **비어 있으면 "제한 없음" 이 아니라 "모름" 이다** - 그때는 넣지 않는다(§6-9 닫힌 쪽).
    public sealed record GearSlotDefinition(string Name, IReadOnlyCollection<string> AllowedTemplateIds);

    // 아이템 데이터베이스에서 옮겨 온 사실. 판정은 SPT 타입을 모르므로 서버가 이 모양으로 바꿔 넘긴다.
    //   TemplateIdByItemId : 이 프로필 아이템 id → 그 아이템의 템플릿 id
    //   SlotsByTemplateId  : 템플릿 id → 그 템플릿이 **정의한** 슬롯들(없는 템플릿은 슬롯이 없다는 뜻이 아니라,
    //                        읽지 못했다는 뜻으로 읽는다 - 거부)
    public sealed record GearSlotFacts(
        IReadOnlyDictionary<string, string> TemplateIdByItemId,
        IReadOnlyDictionary<string, IReadOnlyList<GearSlotDefinition>> SlotsByTemplateId);

    // 설계 §6-4 의 두 문장을 코드로 옮긴 곳이다.
    //   "편집하면 실물이 된다"  - 쓰기는 프로필에 박힌다(서비스 쪽), 그래서 이 판정은 쓰기 전에 거절할 유일한 관문이다.
    //   "출처는 본인 스태시뿐" - 집어넣을 수 있는 것은 그 팀원 스태시에 실제로 있는 것뿐이다. 공짜 생성은 없다.
    //
    // 순수 문자열 작업이라 서버 프로젝트 밖에 산다: 쓰기 기구(SaveServer·SaveProfile)는 거기 남고
    // 판정은 여기서 단위 테스트된다. 캐치올이 없다 - 아래 가드들이 입력이 닿을 수 있는 모든 던짐을
    // 덮고, 캐치올을 두면 가드 하나를 지워도 테스트가 통과해 버린다.
    //
    // 아이템 소속 판정은 WornGearLossPolicy.CollectSubtreeIds 를 재사용한다. 계획 2 가 "어느 id 가 이
    // 프로필의 트리에 속하는가" 의 권위로 세운 함수이고, 트리 걷기를 두 번 쓰면 두 판정이 갈린다.
    public static class ProfileGearEditPolicy
    {
        // 장비 루트의 직접 자식으로 허용하는 슬롯. SPT EquipmentSlots 이름이고
        // SpawnGearPolicy.KnownSlots 와 같아야 한다(둘 중 하나만 고치지 말 것). 허용 목록이지
        // 제외 목록이 아니다 - 모르는 이름은 넣지 않는다(§6-9 닫힌 쪽).
        private static readonly HashSet<string> EquipmentRootSlots = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Headwear", "Earpiece", "FaceCover", "ArmorVest", "Eyewear", "ArmBand", "TacticalVest",
            "Pockets", "Backpack", "SecuredContainer", "FirstPrimaryWeapon", "SecondPrimaryWeapon",
            "Holster", "Scabbard",
        };

        // 장비 트리 "안쪽" 의 슬롯. 이름이 열린 집합(모드 슬롯은 아이템마다 다르다)이라 접두 허용
        // 목록을 쓴다 - 그래도 허용 목록이다. 탄약 슬롯(cartridges·patron_in_weapon*·camora_*)이
        // 여기 있는 것이 인계 문서 열린 항목 4 를 닫는다: 탄약은 특별 취급 대상이 아니고, 넣을 수
        // 없으면 그 팀원은 빈 탄창으로 레이드에 나간다(계획 2 에서 실제로 일어났던 결함).
        private static readonly string[] NestedSlotPrefixes = { "mod_", "patron_in_weapon", "camora_" };

        private const string CartridgeSlot = "cartridges";

        // 테스트가 두 어휘의 약속(장비 루트 슬롯은 SpawnGearPolicy 가 아는 이름과 같다)을 사람 말이 아니라
        // 기계로 지킬 수 있게 열어 둔다. 읽기 전용 - 바깥에서 쓰지 않는다.
        internal static IReadOnlySet<string> EquipmentRootSlotNames => EquipmentRootSlots;

        // 격자(가방·리그 칸)에 넣는 조작은 이 경로가 받지 않는다: 요청에 Location 이 없어 어느 칸인지
        // 표현할 수 없고, 좌표 없이 넣으면 서버가 자리를 지어내야 한다(추측 금지). 슬롯만 받는다.
        public static GearEditVerdict Validate(
            GearEditRequest request,
            IEnumerable<WornGearItem>? profileItems,
            string? stashRootId,
            string? equipmentRootId,
            GearSlotFacts? slotFacts)
        {
            if (request == null) return Reject("편집 요청이 비어 있습니다.");
            if (string.IsNullOrWhiteSpace(request.ProfileId)) return Reject("어느 프로필의 장비인지 알 수 없어 편집하지 않습니다.");
            if (string.IsNullOrWhiteSpace(request.ItemId)) return Reject("옮길 아이템을 알 수 없어 편집하지 않습니다.");
            if (string.IsNullOrWhiteSpace(request.TargetSlotId)) return Reject("넣을 슬롯을 알 수 없어 편집하지 않습니다.");

            // 루트를 추측하면 엉뚱한 트리를 출처나 목적지로 읽는다. 모르면 하지 않는다.
            if (string.IsNullOrWhiteSpace(stashRootId)) return Reject("이 프로필의 스태시 루트를 읽을 수 없어 편집하지 않습니다.");
            if (string.IsNullOrWhiteSpace(equipmentRootId)) return Reject("이 프로필의 장비 루트를 읽을 수 없어 편집하지 않습니다.");
            if (profileItems == null) return Reject("이 프로필의 아이템 목록을 읽을 수 없어 편집하지 않습니다.");

            var stashRoot = stashRootId!.Trim();
            var equipmentRoot = equipmentRootId!.Trim();
            if (string.Equals(stashRoot, equipmentRoot, StringComparison.Ordinal))
            {
                return Reject("이 프로필의 스태시 루트와 장비 루트가 같아 어느 쪽이 출처인지 가릴 수 없습니다.");
            }

            // 한 번만 재료로 만든다. 같은 목록을 두 번 걷되 판정은 CollectSubtreeIds 하나다.
            var items = profileItems as IReadOnlyCollection<WornGearItem> ?? profileItems.ToList();
            var stashIds = WornGearLossPolicy.CollectSubtreeIds(items, new[] { stashRoot });
            var equipmentIds = WornGearLossPolicy.CollectSubtreeIds(items, new[] { equipmentRoot });

            var itemId = request.ItemId.Trim();
            // 출처는 본인 스태시뿐. 플레이어 스태시의 id 는 이 목록에 아예 없으므로 여기서 떨어진다.
            if (!stashIds.Contains(itemId))
            {
                return Reject($"아이템 '{itemId}' 은(는) 이 팀원의 스태시에 없어 장비에 넣을 수 없습니다.");
            }

            // 루트 자체는 아이템이 아니다(옮기면 스태시가 장비 아래로 들어간다).
            if (string.Equals(itemId, stashRoot, StringComparison.Ordinal))
            {
                return Reject("스태시 루트 자체는 장비에 넣을 수 없습니다.");
            }

            // 같은 id 가 두 트리에 다 보이는 프로필은 깨져 있다. 어느 쪽이 진짜인지 모르므로 쓰지 않는다.
            if (equipmentIds.Contains(itemId))
            {
                return Reject($"아이템 '{itemId}' 이(가) 스태시와 장비 양쪽에 보여 어느 쪽이 맞는지 알 수 없습니다.");
            }

            // 부모를 적지 않으면 장비 루트 아래로 읽는다. 적었다면 그 부모는 이 프로필의 장비 트리
            // 안이어야 한다 - 스태시 안의 가방에 넣는 길은 이 경로가 아니다(§6-5 는 다음으로 미뤘다).
            var parentId = string.IsNullOrWhiteSpace(request.TargetParentId) ? equipmentRoot : request.TargetParentId!.Trim();
            if (!equipmentIds.Contains(parentId))
            {
                return Reject($"넣을 자리 '{parentId}' 이(가) 이 팀원의 장비 트리에 없어 편집하지 않습니다.");
            }

            var slot = request.TargetSlotId.Trim();
            var atEquipmentRoot = string.Equals(parentId, equipmentRoot, StringComparison.Ordinal);
            var known = atEquipmentRoot ? EquipmentRootSlots.Contains(slot) : IsKnownNestedSlot(slot);
            if (!known)
            {
                return Reject(atEquipmentRoot
                    ? $"슬롯 '{slot}' 은(는) 아는 장비 슬롯 이름이 아니어서 편집하지 않습니다."
                    : $"슬롯 '{slot}' 은(는) 아는 장착 슬롯 이름이 아니어서 편집하지 않습니다.");
            }

            // 여기까지는 **어휘**만 봤다. 이름이 맞는 슬롯이라도 (a) 이미 차 있을 수 있고, (b) 그 부모
            // 아이템에 아예 없는 슬롯일 수 있고, (c) 그 슬롯이 받지 않는 아이템일 수 있다. 셋 다 집합은
            // 보존되고 트리도 멀쩡하므로 아래의 어떤 가드도 걸러 주지 않는다 - 결과는 EFT 가 적재를
            // 거부할 수 있는, 플레이어가 **플레이하는** 프로필이다. 그래서 여기서 막는다.

            // (a) 한 슬롯에 하나. 두 아이템이 같은 Headwear·같은 mod_magazine 에 앉으면 안 된다.
            // cartridges 도 예외로 두지 않는다: 탄창에 여러 탄 더미를 넣는 것은 Location 색인이 필요한
            // 조작이고 이 요청에는 좌표가 없다(자리를 지어내지 않는다). 빈 탄창에 넣는 길은 열려 있다.
            foreach (var item in items)
            {
                if (item?.Id == null || string.IsNullOrWhiteSpace(item.SlotId)) continue;
                if (string.Equals(item.Id, itemId, StringComparison.Ordinal)) continue;
                if (!string.Equals(item.ParentId, parentId, StringComparison.Ordinal)) continue;
                if (!string.Equals(item.SlotId!.Trim(), slot, StringComparison.OrdinalIgnoreCase)) continue;

                return Reject($"'{parentId}' 의 '{slot}' 슬롯에는 이미 아이템 '{item.Id}' 이(가) 있어 넣지 않습니다.");
            }

            // (b)(c) 아이템 데이터베이스의 사실이 있어야 판정할 수 있다. 없으면 넣지 않는다.
            if (slotFacts == null) return Reject("아이템 정보를 읽을 수 없어 넣을 자리가 맞는지 확인하지 못했습니다. 편집하지 않습니다.");
            if (!slotFacts.TemplateIdByItemId.TryGetValue(parentId, out var parentTemplateId) || string.IsNullOrWhiteSpace(parentTemplateId))
            {
                return Reject($"넣을 자리 '{parentId}' 의 아이템 종류를 읽을 수 없어 편집하지 않습니다.");
            }

            if (!slotFacts.SlotsByTemplateId.TryGetValue(parentTemplateId!, out var slots) || slots == null)
            {
                return Reject($"넣을 자리 '{parentId}' 의 슬롯 정보를 읽을 수 없어 편집하지 않습니다.");
            }

            var definition = slots.FirstOrDefault(s => s != null && string.Equals(s.Name, slot, StringComparison.OrdinalIgnoreCase));
            if (definition == null)
            {
                return Reject($"'{parentId}' 에는 '{slot}' 슬롯이 없어 넣지 않습니다.");
            }

            if (!slotFacts.TemplateIdByItemId.TryGetValue(itemId, out var itemTemplateId) || string.IsNullOrWhiteSpace(itemTemplateId))
            {
                return Reject($"아이템 '{itemId}' 의 종류를 읽을 수 없어 편집하지 않습니다.");
            }

            // 필터가 비면 "제한 없음" 이 아니라 "모름" 으로 읽는다. 제외 목록을 만들지 않는다 -
            // 증명되지 않은 것은 넣지 않는다.
            if (definition.AllowedTemplateIds == null || definition.AllowedTemplateIds.Count == 0)
            {
                return Reject($"'{parentId}' 의 '{slot}' 슬롯이 무엇을 받는지 읽을 수 없어 넣지 않습니다.");
            }

            if (!definition.AllowedTemplateIds.Contains(itemTemplateId!, StringComparer.OrdinalIgnoreCase))
            {
                return Reject($"아이템 '{itemId}' 은(는) '{parentId}' 의 '{slot}' 슬롯에 들어가지 않는 종류라서 넣지 않습니다.");
            }

            return GearEditVerdict.Allow();
        }

        // 편집 여러 건을 한 번에 판정한다. 쓰기 한 번(SaveProfile 한 번 + 디스크 재독 한 번)으로 끝내려면
        // 최종 상태를 **서버가** 만들어야 한다 - 클라이언트가 최종 집합을 제출하는 모양은 계약 2 의
        // 보존 검사를 만들게 한 바로 그 구멍이다. 그래서 요청은 "아이템 하나를 어디로" 가 여러 건이고,
        // 각 건은 **자기 혼자** Validate 를 통과해야 하며, 통과한 건만 작업 사본에 반영한 뒤 다음 건을 본다.
        //
        // 순서대로 누적해서 보는 것이 요구사항이다(한 건씩 원래 상태에 대고 보면 안 된다): 탄창을 총에
        // 꽂고 그 탄창에 탄을 넣는 연쇄는 두 번째 건의 부모(탄창)가 **첫 번째 건의 결과로** 장비 트리
        // 안에 들어와 있어야 통과한다. 인계 문서 열린 항목 4(빈 탄창)가 바로 그 연쇄다.
        //
        // 하나라도 거절되면 **전체를 거절하고 아무것도 쓰지 않는다.** 반쯤 입은 상태를 만들지 않는다.
        public static GearEditVerdict ValidateBatch(
            IReadOnlyList<GearEditRequest>? requests,
            IEnumerable<WornGearItem>? profileItems,
            string? stashRootId,
            string? equipmentRootId,
            GearSlotFacts? slotFacts)
        {
            if (requests == null || requests.Count == 0) return Reject("고칠 아이템이 없어 편집하지 않습니다.");
            if (profileItems == null) return Reject("이 프로필의 아이템 목록을 읽을 수 없어 편집하지 않습니다.");
            if (string.IsNullOrWhiteSpace(equipmentRootId)) return Reject("이 프로필의 장비 루트를 읽을 수 없어 편집하지 않습니다.");

            var working = new List<WornGearItem>();
            foreach (var item in profileItems)
            {
                if (item != null) working.Add(item);
            }

            var equipmentRoot = equipmentRootId!.Trim();
            string? batchProfileId = null;
            foreach (var request in requests)
            {
                if (request == null) return Reject("편집 요청 하나가 비어 있어 전체를 취소합니다.");
                // 한 번의 요청은 한 프로필만 고친다. 섞여 오면 어느 프로필을 저장할지 알 수 없다.
                if (batchProfileId == null) batchProfileId = request.ProfileId;
                else if (!string.Equals(batchProfileId, request.ProfileId, StringComparison.Ordinal))
                {
                    return Reject("한 번의 편집 요청이 여러 프로필을 가리켜 전체를 취소합니다.");
                }

                // 누적된 작업 사본 위에서 본다 - 앞 건이 채운 슬롯은 뒤 건에게 "이미 찬 슬롯" 이고,
                // 앞 건이 장비 트리로 옮긴 아이템은 뒤 건의 부모가 될 수 있다.
                var verdict = Validate(request, working, stashRootId, equipmentRoot, slotFacts);
                if (!verdict.Allowed) return verdict;

                var itemId = request.ItemId.Trim();
                var parentId = string.IsNullOrWhiteSpace(request.TargetParentId) ? equipmentRoot : request.TargetParentId!.Trim();
                var index = working.FindIndex(i => string.Equals(i.Id, itemId, StringComparison.Ordinal));
                if (index < 0)
                {
                    // Validate 가 통과시켰으면 있어야 한다. 그래도 믿고 쓰지 않는다.
                    return Reject($"아이템 '{itemId}' 을(를) 프로필에서 찾지 못해 전체를 취소합니다.");
                }

                working[index] = working[index] with { ParentId = parentId, SlotId = request.TargetSlotId.Trim() };
            }

            return GearEditVerdict.Allow();
        }

        // PIT 의 실물 커밋 경로에 없는 불변식: **원래 플레이어 스태시의 모든 id 는 제출된 플레이어
        // 스태시나 팀원 장비 어느 한쪽에는 반드시 나타난다.** 아무것도 그냥 사라지지 않는다.
        //
        // 왜 필요한가: PIT 는 현재 스태시 트리의 id 를 전부 지우고 제출된 항목으로 갈아 끼운 뒤
        // 저장한다(FriendlyTeammateService.cs:1277-1280, :1295). 검증(ValidateRealCommitItemSet, :2625)은
        // **제출된** 아이템만 훑으므로 "빠진" 아이템은 아무도 찾지 않는다 - 아이템을 잃는 커밋이
        // 어디에서도 걸리지 않는다. 설계 §4 의 안티골 정면이고, 클라이언트 관문(계획 3 Task 3)을
        // 우회해 라우트로 직접 쏘는 호출자에 대한 유일한 방어이기도 하다.
        //
        // 정상적인 "옮김" 을 사고로 읽지 않는다: 플레이어 스태시에서 사라지는 것은 정상이고, 그때는
        // 팀원 장비 쪽에 있어야 한다. PIT 가 팀원 아이템 id 를 다시 매기는 경우
        // (RemapReplacementEquipmentPlayerEquippedIdCollisions, :2750)도 재매김 뒤 id 로 찾아 준다.
        //
        // 닫힌 쪽: 세 목록 중 하나라도 읽을 수 없으면(널) "사라지지 않았다" 고 말할 근거가 없어 거절한다.
        public static StashConservationVerdict CheckStashConservation(
            IEnumerable<string>? originalPlayerStashItemIds,
            IEnumerable<string>? submittedPlayerStashItemIds,
            IEnumerable<string>? destinationItemIds,
            IReadOnlyDictionary<string, string>? remappedItemIds)
        {
            if (originalPlayerStashItemIds == null || submittedPlayerStashItemIds == null || destinationItemIds == null)
            {
                return new StashConservationVerdict(
                    false,
                    "아이템 보존을 확인할 목록을 읽을 수 없어 저장하지 않습니다.",
                    Array.Empty<string>());
            }

            // PIT 가 아이템 id 를 전부 대소문자 무시로 비교한다. 여기서 더 엄격하면 있는 아이템을
            // "사라졌다" 고 말해 정상적인 편집을 막는다.
            var present = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            AddAll(present, submittedPlayerStashItemIds);
            AddAll(present, destinationItemIds);

            var missing = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var rawId in originalPlayerStashItemIds)
            {
                // 널·공백 id 는 아이템을 가리키지 못하므로 "사라졌다" 의 대상이 아니다.
                if (string.IsNullOrWhiteSpace(rawId)) continue;

                var id = rawId.Trim();
                if (!seen.Add(id)) continue;
                if (present.Contains(id)) continue;
                // 재매김된 id 로 목적지에 있으면 사라진 것이 아니다.
                if (remappedItemIds != null
                    && remappedItemIds.TryGetValue(id, out var newId)
                    && !string.IsNullOrWhiteSpace(newId)
                    && present.Contains(newId!.Trim()))
                {
                    continue;
                }

                missing.Add(id);
            }

            if (missing.Count == 0)
            {
                return new StashConservationVerdict(true, string.Empty, Array.Empty<string>());
            }

            // 무엇이 사라졌는지 말한다. 사람이 그 아이템을 찾아볼 수 있어야 보고가 쓸모 있다.
            return new StashConservationVerdict(
                false,
                $"저장하면 아이템 {missing.Count}개가 플레이어 스태시와 팀원 장비 어디에도 없게 되어 저장하지 않습니다: {string.Join(", ", missing)}",
                missing);
        }

        // 디스크 재독 대조(§6-8 3단계)의 집합 부분: 편집 전과 후의 아이템 id 집합이 **정확히 같은가.**
        // 한 프로필 안의 이동이라 아이템이 사라져서도, 끼어들어서도 안 된다 - 편집은 부모와 슬롯만 바꾼다.
        // 양방향으로 보존 검사를 쓴다: 사라진 쪽만 보면 "없던 아이템이 생겼다"(= 공짜 생성)를 놓친다.
        // 널이면 어느 쪽도 주장할 수 없어 거절이다(닫힌 쪽). MissingItemIds 는 사라진 id 뒤에 끼어든 id 가 온다.
        public static StashConservationVerdict CheckItemSetUnchanged(IEnumerable<string>? beforeIds, IEnumerable<string>? afterIds)
        {
            if (beforeIds == null || afterIds == null)
            {
                return new StashConservationVerdict(
                    false,
                    "편집 전후의 아이템 목록을 읽을 수 없어 대조하지 못했습니다.",
                    Array.Empty<string>());
            }

            var before = beforeIds as IReadOnlyCollection<string> ?? beforeIds.ToList();
            var after = afterIds as IReadOnlyCollection<string> ?? afterIds.ToList();
            var none = Array.Empty<string>();
            var lost = CheckStashConservation(before, after, none, null);
            var gained = CheckStashConservation(after, before, none, null);
            if (lost.Conserved && gained.Conserved)
            {
                return new StashConservationVerdict(true, string.Empty, none);
            }

            var changed = new List<string>(lost.MissingItemIds);
            changed.AddRange(gained.MissingItemIds);
            var parts = new List<string>();
            if (lost.MissingItemIds.Count > 0) parts.Add($"사라진 아이템 {lost.MissingItemIds.Count}개: {string.Join(", ", lost.MissingItemIds)}");
            if (gained.MissingItemIds.Count > 0) parts.Add($"끼어든 아이템 {gained.MissingItemIds.Count}개: {string.Join(", ", gained.MissingItemIds)}");
            return new StashConservationVerdict(false, $"편집 전후의 아이템 목록이 다릅니다 - {string.Join(" / ", parts)}", changed);
        }

        private static void AddAll(HashSet<string> target, IEnumerable<string> ids)
        {
            foreach (var id in ids)
            {
                if (string.IsNullOrWhiteSpace(id)) continue;
                target.Add(id.Trim());
            }
        }

        private static bool IsKnownNestedSlot(string slot)
        {
            if (string.Equals(slot, CartridgeSlot, StringComparison.OrdinalIgnoreCase)) return true;

            foreach (var prefix in NestedSlotPrefixes)
            {
                if (slot.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }

        private static GearEditVerdict Reject(string reason) => GearEditVerdict.Reject(reason);
    }
}
