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
    //   AmmoCapacity : 탄을 **색인**으로 담는 자리(데이터베이스의 Cartridges·Chambers)의 최대 발수다.
    //                  일반 슬롯은 null 이다. 값이 있으면 (1) 더미 수량이 용량을 넘으면 거부하고
    //                  (2) 쓸 때 격자 좌표가 아니라 색인 0 을 쓴다(StmGearEditService). 둘을 한 값으로
    //                  가르는 이유는 "탄 자리인가" 를 슬롯 이름으로 다시 추측하지 않으려는 것이다.
    public sealed record GearSlotDefinition(string Name, IReadOnlyCollection<string> AllowedTemplateIds, int? AmmoCapacity = null);

    // 용기의 **격자**(가방·리그·주머니·보안 컨테이너의 칸). 슬롯과 다른 점 둘: 여러 개를 담으므로 점유를
    // 묻지 않고, 데이터베이스가 **받지 않는 종류**(ExcludedFilter)를 따로 적어 둔다 - 가방 안에 가방을
    // 넣지 못하는 것이 그것이다. 허용 쪽은 보통 뿌리 노드 하나라서 조상 사슬을 걷어야 맞는다.
    public sealed record GearGridDefinition(
        string Name,
        IReadOnlyCollection<string> AllowedTemplateIds,
        IReadOnlyCollection<string>? ExcludedTemplateIds = null);

    // 아이템 데이터베이스에서 옮겨 온 사실. 판정은 SPT 타입을 모르므로 서버가 이 모양으로 바꿔 넘긴다.
    //   TemplateIdByItemId : 이 프로필 아이템 id → 그 아이템의 템플릿 id
    //   SlotsByTemplateId  : 템플릿 id → 그 템플릿이 **정의한** 슬롯들(없는 템플릿은 슬롯이 없다는 뜻이 아니라,
    //                        읽지 못했다는 뜻으로 읽는다 - 거부)
    //   ParentTemplateIdByTemplateId : 템플릿 id → 그 템플릿의 `_parent`. 슬롯 필터가 **범주 노드**를
    //                        담기 때문에 반드시 필요하다 - 아래 FilterAccepts 의 주석에 실측 근거가 있다.
    //   StackCountByItemId : 이 프로필 아이템 id → 그 더미의 발수(Upd.StackObjectsCount, 없으면 1).
    //                  용량이 있는 자리의 판정에만 쓰인다. 비거나 그 id 가 없으면 "모른다" 이고,
    //                  모르면 넣지 않는다 - "한 발이겠지" 로 치면 용량 검사가 그대로 뚫린다.
    public sealed record GearSlotFacts(
        IReadOnlyDictionary<string, string> TemplateIdByItemId,
        IReadOnlyDictionary<string, IReadOnlyList<GearSlotDefinition>> SlotsByTemplateId,
        IReadOnlyDictionary<string, string> ParentTemplateIdByTemplateId,
        IReadOnlyDictionary<string, int>? StackCountByItemId = null,
        IReadOnlyDictionary<string, IReadOnlyList<GearGridDefinition>>? GridsByTemplateId = null);

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

        // 스태시 루트의 격자 이름. 벗긴 장비가 돌아가는 유일한 목적지다 - SPT 의
        // InventoryHelper.PlaceItemInContainer 의 기본 desiredSlotId 와 같은 값이고(refs/4.1.0),
        // 스태시 템플릿의 격자 이름이다. 역시 허용 목록이다: 모르는 칸 이름으로는 넣지 않는다.
        private static readonly HashSet<string> StashSlots = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "hideout",
        };

        // 테스트가 "벗기기의 목적지 칸" 어휘를 기계로 확인할 수 있게 열어 둔다. 읽기 전용.
        internal static IReadOnlySet<string> StashSlotNames => StashSlots;

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

            // 방향을 먼저 정한다. 목적지가 어디냐가 "출처가 어디여야 하는가" 를 정하기 때문이다.
            //   장비 트리로  = 입히기(장착). 출처는 **이 팀원의 것**이면 된다 - 스태시든 장비든.
            //   스태시 루트로 = 벗기기(해제). 출처는 이 팀원 장비 트리뿐이다.
            // 둘 다 한 프로필 안의 이동이고, 공짜 생성이 없다는 점도 같다 - 벗기는 아이템은 이미 그
            // 프로필의 것이다. §6-4 의 "출처는 본인 스태시뿐" 은 **새 아이템이 어디서 오는가** 의 규칙이고,
            // 그 팀원 자신의 장비가 그 팀원 자신의 스태시로 돌아가는 것을 금지하는 규칙이 아니다.
            // 둘 다 아닌 목적지는 거부다: 스태시 안의 가방 칸도(§6-5 가 미뤘다), 다른 프로필의 무엇도
            // 이 경로로는 닿을 수 없다 - 애초에 이 목록에는 이 프로필의 아이템만 있다.
            var parentId = string.IsNullOrWhiteSpace(request.TargetParentId) ? equipmentRoot : request.TargetParentId!.Trim();
            var toStash = string.Equals(parentId, stashRoot, StringComparison.Ordinal);
            var parentInStash = stashIds.Contains(parentId);
            var parentInEquipment = equipmentIds.Contains(parentId);
            if (!parentInStash && !parentInEquipment)
            {
                return Reject($"넣을 자리 '{parentId}' 이(가) 이 팀원의 것이 아니어서 편집하지 않습니다.");
            }

            if (parentInStash && parentInEquipment)
            {
                return Reject($"넣을 자리 '{parentId}' 이(가) 스태시와 장비 양쪽에 보여 어느 쪽이 맞는지 알 수 없습니다.");
            }

            var toEquipment = parentInEquipment;

            // 출처 판정. 방향과 무관하게 **이 팀원의 것**이면 된다(계획 5: 플레이어 화면과 같게).
            {
                // 출처는 **이 팀원의 것**이면 된다. 플레이어 스태시의 id 는 이 목록에 아예 없으므로
                // 여기서 떨어진다. 2026-11-03 전에는 스태시 하위만 받았는데, 그러면 리그 안 탄창이나
                // 가방 안 여분 탄창처럼 **그 팀원이 이미 가진** 것을 쓸 수 없었다(인게임에서 막혔다).
                // 넓혀도 공짜 생성은 없다: 두 경우 모두 한 프로필 안의 이동이고 쓰기는 한 번이다.
                var fromStash = stashIds.Contains(itemId);
                var fromEquipment = equipmentIds.Contains(itemId);
                if (!fromStash && !fromEquipment)
                {
                    return Reject($"아이템 '{itemId}' 은(는) 이 팀원의 것이 아니어서 편집하지 않습니다.");
                }

                // 같은 id 가 두 트리에 다 보이는 프로필은 깨져 있다. 어느 쪽이 진짜인지 모르므로 쓰지 않는다.
                if (fromStash && fromEquipment)
                {
                    return Reject($"아이템 '{itemId}' 이(가) 스태시와 장비 양쪽에 보여 어느 쪽이 맞는지 알 수 없습니다.");
                }

                // 루트 자체는 아이템이 아니다(옮기면 트리 하나가 다른 트리 밑으로 들어간다).
                if (string.Equals(itemId, stashRoot, StringComparison.Ordinal))
                {
                    return Reject("스태시 루트 자체는 장비에 넣을 수 없습니다.");
                }

                if (string.Equals(itemId, equipmentRoot, StringComparison.Ordinal))
                {
                    return Reject("장비 루트 자체는 다른 자리에 넣을 수 없습니다.");
                }

                // 떼어내선 안 되는 자리. 판정의 권위는 **WornGearLossPolicy** 다 - 사망이 가져가지 않는
                // 슬롯(주머니·칼집·완장·보안 컨테이너)과 영구 조각(Dogtag·SpecialSlot)을 거기서 그대로
                // 가져온다. 두 번째 목록을 만들지 않았다: 둘이 갈라지면 한쪽에서 지키는 것을 다른 쪽에서
                // 떼어낸다(교차 계약 테스트가 못박는다). 주머니를 떼면 장비 손실이 아니라 **구조 손상**
                // 이고 EFT 가 그 프로필의 적재를 거부할 수 있다. 주머니 **안의 짐**은 걸리지 않는다
                // (그 아이템들의 슬롯은 pocket1 등이다).
                var source = FindItem(items, itemId);
                if (WornGearLossPolicy.IsNeverRemovedSlot(source?.SlotId))
                {
                    return Reject($"'{source!.SlotId!.Trim()}' 은(는) 프로필에서 떼어낼 수 없는 자리라서 옮기지 않습니다.");
                }

                // 고리 방지. 자기 자신이나 자기 안으로 옮기면 트리가 끊겨 그 가지 전체가 프로필에서
                // 사라진 것처럼 보인다.
                var ownSubtree = WornGearLossPolicy.CollectSubtreeIds(items, new[] { itemId });
                if (ownSubtree.Contains(parentId))
                {
                    return Reject($"아이템 '{itemId}' 은(는) 자기 자신이나 자기 안의 아이템에 넣을 수 없습니다.");
                }
            }

            var slot = request.TargetSlotId.Trim();

            // 목적지가 **용기의 격자**인가(가방·리그·주머니·보안 컨테이너의 칸). 이름의 권위는 그 용기
            // 템플릿의 Grids 이고, 스태시 루트는 StashSlots 가 못박은 자기 경로를 쓴다(아래).
            // 격자는 슬롯과 둘이 다르다: 여러 개를 담으므로 **점유를 묻지 않고**, 자리가 있는지는 서버가
            // SPT 의 PlaceItemInContainer 로 센다 - 거기서 못 찾으면 그 편집은 거부된다.
            if (!toStash)
            {
                var grid = FindGrid(slotFacts, parentId, slot);
                if (grid != null)
                {
                    if (slotFacts == null
                        || !slotFacts.TemplateIdByItemId.TryGetValue(itemId, out var gridItemTemplateId)
                        || string.IsNullOrWhiteSpace(gridItemTemplateId))
                    {
                        return Reject($"아이템 '{itemId}' 의 종류를 읽을 수 없어 편집하지 않습니다.");
                    }

                    // 비면 "제한 없음" 이 아니라 "모름" 이다(슬롯과 같은 규칙).
                    if (grid.AllowedTemplateIds == null || grid.AllowedTemplateIds.Count == 0)
                    {
                        return Reject($"'{parentId}' 의 '{slot}' 칸이 무엇을 받는지 읽을 수 없어 넣지 않습니다.");
                    }

                    if (!FilterAccepts(slotFacts, grid.AllowedTemplateIds, gridItemTemplateId!))
                    {
                        return Reject($"아이템 '{itemId}' 은(는) '{parentId}' 의 '{slot}' 칸에 들어가지 않는 종류라서 넣지 않습니다.");
                    }

                    // 받지 않는 종류(ExcludedFilter). 가방 안에 가방을 넣지 못하는 것이 이것이다.
                    if (grid.ExcludedTemplateIds != null && grid.ExcludedTemplateIds.Count > 0
                        && FilterAccepts(slotFacts, grid.ExcludedTemplateIds, gridItemTemplateId!))
                    {
                        return Reject($"아이템 '{itemId}' 은(는) '{parentId}' 의 '{slot}' 칸이 받지 않는 종류라서 넣지 않습니다.");
                    }

                    return GearEditVerdict.Allow();
                }
            }

            var atEquipmentRoot = toEquipment && string.Equals(parentId, equipmentRoot, StringComparison.Ordinal);
            var known = toStash
                ? StashSlots.Contains(slot)
                : atEquipmentRoot ? EquipmentRootSlots.Contains(slot) : IsKnownNestedSlot(slot);
            if (!known)
            {
                return Reject(toStash
                    ? $"슬롯 '{slot}' 은(는) 아는 스태시 칸 이름이 아니어서 편집하지 않습니다."
                    : atEquipmentRoot
                        ? $"슬롯 '{slot}' 은(는) 아는 장비 슬롯 이름이 아니어서 편집하지 않습니다."
                        : $"슬롯 '{slot}' 은(는) 아는 장착 슬롯 이름이 아니어서 편집하지 않습니다.");
            }

            // 스태시로 벗기는 쪽에는 아래 (a)(b)(c) 를 적용하지 **않는다** - 빠뜨린 것이 아니라 질문이
            // 성립하지 않아서다. 셋 다 "슬롯" 의 성질을 묻는데 스태시 목적지는 슬롯이 아니라 **격자**다:
            //   (a) 점유 - 격자는 아이템 하나가 아니라 여러 개를 담는다. 자리가 있는지는 좌표 계산의
            //       문제이고, 그 판정은 SPT 자신의 PlaceItemInContainer 가 한다(StmGearEditService).
            //       거기서 자리를 못 찾으면 편집은 거부된다 - 즉 "자리 있나" 는 여전히 검사된다.
            //   (b) 슬롯 존재 - 스태시 격자의 이름은 StashSlots 허용 목록이 이미 못박았고, 실제로 그
            //       격자가 있는지는 같은 SPT 호출이 템플릿에서 읽어 판정한다.
            //   (c) 필터 - 스태시는 그 프로필의 일반 보관소이고, 벗기는 아이템은 **이미 그 프로필의 것**
            //       이다. 받을 수 없는 아이템을 새로 들여오는 경우가 아니다.
            // 대신 벗기기 쪽에서 **살아 있는** 검사는 위에 다 있다: 출처가 이 팀원의 장비 트리인지,
            // 장비 루트 자체가 아닌지, 두 트리에 겹쳐 보이지 않는지, 목적지가 스태시 루트인지, 칸 이름이
            // 허용 목록인지, 그리고 **프로필에서 떼어낼 수 없는 자리가 아닌지**
            // (WornGearLossPolicy.IsNeverRemovedSlot - 주머니·보안 컨테이너·완장·칼집·영구 조각).
            // 자리가 있는지는 서비스에서 SPT 의 PlaceItemInContainer 가 답한다.
            if (toStash)
            {
                return GearEditVerdict.Allow();
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

            if (!FilterAccepts(slotFacts, definition.AllowedTemplateIds, itemTemplateId!))
            {
                return Reject($"아이템 '{itemId}' 은(는) '{parentId}' 의 '{slot}' 슬롯에 들어가지 않는 종류라서 넣지 않습니다.");
            }

            // (d) 용량. 탄을 담는 자리에는 발수 상한이 있고 이 편집은 **수량을 바꾸지 않는다**(설계) -
            // 1080발 더미를 30발 탄창에 "일부만" 넣을 수 없다. 넘치는 더미를 그대로 쓰면 EFT 가 적재를
            // 거부할 수 있는 프로필이 된다(관문 17 의 '규칙을 어긴 스태시 항목' 과 같은 부류다).
            // 용량을 아는데 수량을 모르면 거부한다 - 모르는 쪽을 1 로 치면 검사가 뚫린다.
            if (definition.AmmoCapacity is int capacity)
            {
                if (capacity <= 0)
                {
                    return Reject($"'{parentId}' 의 '{slot}' 자리가 몇 발을 받는지 읽을 수 없어 넣지 않습니다.");
                }

                if (slotFacts.StackCountByItemId == null
                    || !slotFacts.StackCountByItemId.TryGetValue(itemId, out var stackCount)
                    || stackCount <= 0)
                {
                    return Reject($"아이템 '{itemId}' 의 수량을 읽을 수 없어 '{slot}' 자리에 넣지 않습니다.");
                }

                if (stackCount > capacity)
                {
                    return Reject($"아이템 '{itemId}' 은(는) {stackCount}발이고 '{parentId}' 의 '{slot}' 자리는 {capacity}발까지라서 넣지 않습니다. 수량을 쪼개지 않습니다.");
                }
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

            // 한 아이템은 한 묶음에서 한 번만 옮긴다. 출처가 스태시뿐이던 때는 두 번째 건이 "스태시에
            // 없다" 로 저절로 걸렸는데, 출처를 넓히면서 그 그물이 없어졌다. 두 번 적는 것은 최종 자리가
            // 모호하다는 뜻이고(무엇을 쓸지 요청이 말하지 않는다), 디스크 재독의 기대도 둘이 된다.
            var movedItemIds = new HashSet<string>(StringComparer.Ordinal);

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
                if (!movedItemIds.Add(itemId))
                {
                    return Reject($"아이템 '{itemId}' 을(를) 한 묶음에서 두 번 옮길 수 없습니다. 마지막 자리 하나만 보냅니다.");
                }

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

        // 프로필에 **쓸** 정식 슬롯 이름. 요청의 철자는 쓰지 않는다: 어휘 검사는 대소문자를 무시하고
        // 받아 주지만 SPT·EFT 는 슬롯 이름을 대소문자까지 그대로 본다 - "headwear" 로 저장된 헬멧은
        // 착용으로 인식되지 않는다. 검사를 통과한 요청이 깨진 프로필을 쓰는 길이 여기였다.
        //
        // 정식 이름의 출처는 **부모 아이템 템플릿이 정의한 슬롯 이름**(GearSlotDefinition.Name)이다 -
        // 데이터베이스 자신의 철자이므로 추측이 아니다. 장비 루트 슬롯은 허용 목록의 항목이 그 백업이고
        // (EquipmentSlots 이름 그대로), 스태시 칸은 StashSlots 의 항목이다(서비스의 격자 배치는 템플릿의
        // 격자 이름을 직접 쓰므로 그쪽은 이미 정식이다).
        // 어느 출처에도 없으면 null 이다. 호출자는 그때 **거부한다** - 철자를 지어내지 않는다.
        public static string? CanonicalSlotName(
            string? slot,
            string? parentId,
            string? equipmentRootId,
            string? stashRootId,
            GearSlotFacts? slotFacts)
        {
            if (string.IsNullOrWhiteSpace(slot)) return null;

            var wanted = slot!.Trim();
            var parent = parentId?.Trim();
            if (!string.IsNullOrWhiteSpace(stashRootId) && string.Equals(parent, stashRootId!.Trim(), StringComparison.Ordinal))
            {
                return StashSlots.TryGetValue(wanted, out var stashSlot) ? stashSlot : null;
            }

            if (!string.IsNullOrWhiteSpace(parent) && slotFacts?.TemplateIdByItemId != null
                && slotFacts.TemplateIdByItemId.TryGetValue(parent!, out var parentTemplateId)
                && !string.IsNullOrWhiteSpace(parentTemplateId)
                && slotFacts.SlotsByTemplateId != null
                && slotFacts.SlotsByTemplateId.TryGetValue(parentTemplateId!, out var slots)
                && slots != null)
            {
                foreach (var definition in slots)
                {
                    if (definition?.Name == null) continue;
                    if (string.Equals(definition.Name, wanted, StringComparison.OrdinalIgnoreCase)) return definition.Name;
                }
            }

            if (!string.IsNullOrWhiteSpace(equipmentRootId) && string.Equals(parent, equipmentRootId!.Trim(), StringComparison.Ordinal))
            {
                return EquipmentRootSlots.TryGetValue(wanted, out var rootSlot) ? rootSlot : null;
            }

            return null;
        }

        // 이 자리가 용기의 **격자**인가. 서비스는 이 값으로 "SPT 에게 빈 자리를 찾게 할 것인가" 를
        // 가른다. 슬롯이면 false 이고, 그때는 좌표 없이 슬롯 이름만 쓴다.
        public static bool IsGridDestination(string? slot, string? parentId, GearSlotFacts? slotFacts)
            => FindGrid(slotFacts, parentId, slot) != null;

        // 이 자리가 탄을 색인으로 담는가, 담으면 몇 발까지인가. 서비스는 이 값으로 쓰는 Location 을
        // 가른다(값이 있으면 색인 0, 없으면 null). 슬롯 이름으로 "cartridges 면 탄 자리" 라고 다시
        // 판정하지 않는 것이 요점이다 - 어휘의 집은 데이터베이스 하나다(불변식 8).
        public static int? SlotAmmoCapacity(string? slot, string? parentId, GearSlotFacts? slotFacts)
        {
            if (string.IsNullOrWhiteSpace(slot) || string.IsNullOrWhiteSpace(parentId)) return null;
            if (slotFacts?.TemplateIdByItemId == null || slotFacts.SlotsByTemplateId == null) return null;
            if (!slotFacts.TemplateIdByItemId.TryGetValue(parentId!.Trim(), out var parentTemplateId)
                || string.IsNullOrWhiteSpace(parentTemplateId))
            {
                return null;
            }

            if (!slotFacts.SlotsByTemplateId.TryGetValue(parentTemplateId!, out var slots) || slots == null) return null;

            var wanted = slot!.Trim();
            foreach (var definition in slots)
            {
                if (definition?.Name == null) continue;
                if (string.Equals(definition.Name, wanted, StringComparison.OrdinalIgnoreCase)) return definition.AmmoCapacity;
            }

            return null;
        }

        // 이 부모의 이 이름이 격자인가. 아니면 null 이고, 호출자는 슬롯 쪽으로 간다.
        private static GearGridDefinition? FindGrid(GearSlotFacts? facts, string? parentId, string? name)
        {
            if (string.IsNullOrWhiteSpace(parentId) || string.IsNullOrWhiteSpace(name)) return null;
            if (facts?.TemplateIdByItemId == null || facts.GridsByTemplateId == null) return null;
            if (!facts.TemplateIdByItemId.TryGetValue(parentId!.Trim(), out var parentTemplateId)
                || string.IsNullOrWhiteSpace(parentTemplateId))
            {
                return null;
            }

            if (!facts.GridsByTemplateId.TryGetValue(parentTemplateId!, out var grids) || grids == null) return null;

            var wanted = name!.Trim();
            foreach (var grid in grids)
            {
                if (grid?.Name == null) continue;
                if (string.Equals(grid.Name, wanted, StringComparison.OrdinalIgnoreCase)) return grid;
            }

            return null;
        }

        private static WornGearItem? FindItem(IReadOnlyCollection<WornGearItem> items, string itemId)
        {
            foreach (var item in items)
            {
                if (item?.Id == null) continue;
                if (string.Equals(item.Id, itemId, StringComparison.Ordinal)) return item;
            }

            return null;
        }

        // 슬롯 필터가 이 아이템을 받는가. 아이템 자신의 템플릿 id 와 **조상 템플릿 id 들**을 차례로 본다.
        //
        // 조상까지 봐야 하는 이유는 실측이다(2026-11-02, E:\Spt\SPT_Runtime\SPT_Data\database\templates\items.json):
        // 'Default Inventory' 템플릿 `55d7217a4bdc2d86028b456d` 의 Headwear 슬롯 필터는 정확히
        // `["5a341c4086f77401f2541505"]` 하나이고, 그 id 는 `_type: "Node"`·`_name: "Headwear"` 인
        // **범주 노드**다(아이템 템플릿이 아니다). TacticalVest·Backpack·ArmorVest·Pockets·
        // SecuredContainer·FaceCover·Eyewear·ArmBand·Scabbard 도 같다. 그래서 아이템 자신의 id 만
        // 비교하면 **모든 몸 장비가 "안 들어가는 종류" 로 거부**되고 무기 개조 슬롯만 동작한다
        // (그쪽 필터는 실제 아이템 tpl 을 늘어놓는다).
        // 예: 헬멧 `5645bc214bdc2d363b8b4571` 의 `_parent` 가 바로 그 노드다 - 한 칸 올라가면 맞는다.
        //
        // 닫힌 쪽은 그대로다: 필터를 읽지 못했으면(비었으면) 애초에 여기 오지 않고 거부되고, 조상을
        // 끝까지 올라가도 못 만나면 거부다. 조상 사슬을 모르면(맵에 없으면) 그 자리에서 멈춘다 -
        // 없는 조상을 지어내지 않는다. 같은 id 를 두 번 밟지 않으므로 손상된 사슬(고리)도 끝난다.
        private static bool FilterAccepts(GearSlotFacts facts, IReadOnlyCollection<string> allowedTemplateIds, string itemTemplateId)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var templateId = itemTemplateId;
            while (!string.IsNullOrWhiteSpace(templateId) && seen.Add(templateId!))
            {
                if (allowedTemplateIds.Contains(templateId!, StringComparer.OrdinalIgnoreCase)) return true;
                if (facts.ParentTemplateIdByTemplateId == null) return false;
                facts.ParentTemplateIdByTemplateId.TryGetValue(templateId!, out var parentTemplateId);
                templateId = parentTemplateId;
            }

            return false;
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
