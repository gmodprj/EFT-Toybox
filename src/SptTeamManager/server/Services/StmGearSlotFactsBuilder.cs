using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace pitTeam.Server.Services;

// 아이템 데이터베이스의 사실을 정책이 쓰는 모양(GearSlotFacts)으로 옮기는 **유일한** 자리다.
//
// 왜 서비스 둘이 쓰는 집 하나인가: 자리의 어휘(슬롯·탄 자리·격자·필터·용량)를 묻는 경로가 둘이 됐다 -
// 꾸러미 구매가 쓰는 격자 배치(StmGearEditService)와 최종 상태 저장의 자리 검사
// (StmInventoryApplyService → ProfileInventoryApplyPolicy). 두 벌로 적으면 다음에 하나를 고치는 순간
// 갈라지고, 한쪽에서 허락하는 자리를 다른 쪽에서 거부한다(불변식 8: 한 판단에 집은 하나).
//
// 던지지 않는다: 데이터베이스 읽기가 터지면 **빈 사실**을 돌려준다. 빈 사실을 두 호출자가 반대로
// 읽는다는 것을 알고 있어야 한다 - 이동 목록 경로는 전부 거부하고(닫힌 쪽), 최종 상태 경로는 자리를
// 보지 않고 보존만 본다. 그 극성 차이의 이유는 ProfileInventoryApplyPolicy.CheckPlacements 의 주석에 있다.
[Injectable(InjectionType.Singleton)]
public class StmGearSlotFactsBuilder(ItemHelper itemHelper, ISptLogger<StmGearSlotFactsBuilder> logger)
{
    // 아이템 데이터베이스는 서버에서 닿는다 - ItemHelper.GetItem(tpl) 이 (찾았나, 템플릿) 을 주고,
    // 템플릿의 Properties.Slots 가 그 아이템이 **정의한** 슬롯과 각 슬롯의 필터
    // (Props.Filters[].Filter = 허용 템플릿 id 집합)를 들고 있다. 하나라도 읽히지 않으면 그 항목을
    // 넣지 않는다 - 빈 칸이 곧 "모름" 이고, 모름을 어떻게 읽을지는 호출자가 정한다.
    public GearSlotFacts Build(IEnumerable<Item> items)
    {
        var templateIdByItemId = new Dictionary<string, string>(StringComparer.Ordinal);
        var slotsByTemplateId = new Dictionary<string, IReadOnlyList<GearSlotDefinition>>(StringComparer.Ordinal);
        var parentByTemplateId = new Dictionary<string, string>(StringComparer.Ordinal);
        // 더미 수량. 탄을 담는 자리의 용량 검사에만 쓰인다(PIT 의 GetItemStackCount 와 같은 식이다).
        var stackCountByItemId = new Dictionary<string, int>(StringComparer.Ordinal);
        // 용기의 칸(가방·리그·주머니·보안 컨테이너). 슬롯과 같은 길로 싣는다 - 둘 다 그 템플릿이 정의한다.
        var gridsByTemplateId = new Dictionary<string, IReadOnlyList<GearGridDefinition>>(StringComparer.Ordinal);
        try
        {
            foreach (var item in items)
            {
                if (item?.Id == null || item.Template.IsEmpty) continue;
                var itemId = item.Id.ToString();
                var templateId = item.Template.ToString();
                templateIdByItemId[itemId] = templateId;
                stackCountByItemId[itemId] = Math.Max(1, (int)Math.Ceiling(item.Upd?.StackObjectsCount ?? 1d));
                // 조상 사슬. 슬롯 필터가 아이템 tpl 대신 **범주 노드**를 담기 때문에 필요하다
                // (ProfileGearEditPolicy.FilterAccepts 의 실측 주석). 노드도 items.json 의 항목이라
                // 같은 GetItem 으로 읽힌다. 32칸으로 끊는다 - 손상된 사슬에서 돌지 않게.
                CollectTemplateAncestry(item.Template, parentByTemplateId);
                if (slotsByTemplateId.ContainsKey(templateId)) continue;

                var found = itemHelper.GetItem(item.Template);
                if (!found.Key || found.Value == null) continue;

                var definitions = new List<GearSlotDefinition>();
                foreach (var slot in found.Value.Properties?.Slots ?? [])
                {
                    if (slot?.Name == null) continue;
                    definitions.Add(new GearSlotDefinition(slot.Name, AllowedTemplatesOf(slot)));
                }

                // 탄을 담는 자리는 **Slots 에 없다**(2026-11-03 실측: 바닐라 탄창
                // mag_stanag_colt_ar15_std_556x45_30 은 Slots 가 비어 있고 Cartridges 에 'cartridges'
                // 하나를 둔다). 약실(patron_in_weapon)도 같은 이유로 Chambers 에 있다. 이 둘을 읽지
                // 않으면 탄은 **어떤 탄창에도** 넣을 수 없다 - 인계 문서 열린 항목 4 가 그 결함이었다.
                // 격자 좌표가 아니라 색인으로 담으므로 용량(_max_count)을 함께 싣는다.
                // 탄창: 용량이 Cartridges 항목의 _max_count 에 있다(STANAG 30발 = 30, GP-5 = 33).
                // 읽지 못하면 0 으로 싣는다 - "몇 발인지 모른다" 다. 자리를 빼 버리면 "그런 슬롯이
                // 없다" 로 읽혀 사유가 사실과 달라진다.
                foreach (var magazineSlot in found.Value.Properties?.Cartridges ?? [])
                {
                    if (magazineSlot?.Name == null) continue;
                    definitions.Add(new GearSlotDefinition(
                        magazineSlot.Name,
                        AllowedTemplatesOf(magazineSlot),
                        (int?)magazineSlot.MaxCount ?? 0));
                }

                // 약실: Chambers 항목에는 **개수 필드가 아예 없다**(2026-11-03 실측: M4A1 의
                // patron_in_weapon 은 filters 하나만 들고 있다). 그래서 1 로 싣는다 - 이름에서 뜻을
                // 지어낸 것이 아니라 (ㄱ) 데이터베이스가 개수를 말하지 않고 (ㄴ) 이 PC 의 프로필 여섯 개에
                // 있는 약실 아이템 15개가 전부 수량 1 이고 (ㄷ) 1 이 닫힌 쪽이기 때문이다.
                foreach (var chamberSlot in found.Value.Properties?.Chambers ?? [])
                {
                    if (chamberSlot?.Name == null) continue;
                    definitions.Add(new GearSlotDefinition(chamberSlot.Name, AllowedTemplatesOf(chamberSlot), 1));
                }

                // 격자. 허용 쪽은 보통 뿌리 노드 하나이고(조상 사슬로 맞춘다), 제외 쪽이 종류를 걷어낸다
                // (가방 안의 가방 등). 둘 다 그대로 옮긴다 - 뜻을 지어내지 않는다.
                var gridDefinitions = new List<GearGridDefinition>();
                foreach (var grid in found.Value.Properties?.Grids ?? [])
                {
                    if (grid?.Name == null) continue;
                    var allowedInGrid = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    var excludedFromGrid = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var filter in grid.Properties?.Filters ?? [])
                    {
                        foreach (var tpl in filter?.Filter ?? [])
                        {
                            if (!tpl.IsEmpty) allowedInGrid.Add(tpl.ToString());
                        }

                        foreach (var tpl in filter?.ExcludedFilter ?? [])
                        {
                            if (!tpl.IsEmpty) excludedFromGrid.Add(tpl.ToString());
                        }
                    }

                    gridDefinitions.Add(new GearGridDefinition(grid.Name, allowedInGrid, excludedFromGrid));
                }

                // 슬롯과 격자를 **함께** 싣는다. 둘 다 있는 것이 "이 템플릿을 읽었다" 의 신호이고,
                // ProfileGearEditPolicy.KnowsPlacesOf 가 그것으로 "그런 자리가 없다" 를 말할 자격을
                // 가린다 - 한쪽만 싣지 말 것.
                gridsByTemplateId[templateId] = gridDefinitions;
                slotsByTemplateId[templateId] = definitions;
            }
        }
        catch (Exception ex)
        {
            // 반쯤 채운 사실로 판정하면 어떤 것은 통과하고 어떤 것은 거부되는 설명할 수 없는 결과가 된다.
            logger.Error($"[STM] 아이템 데이터베이스를 읽지 못해 넣을 자리 검사를 할 수 없다: {ex}");
            return new GearSlotFacts(
                new Dictionary<string, string>(StringComparer.Ordinal),
                new Dictionary<string, IReadOnlyList<GearSlotDefinition>>(StringComparer.Ordinal),
                new Dictionary<string, string>(StringComparer.Ordinal),
                new Dictionary<string, int>(StringComparer.Ordinal),
                new Dictionary<string, IReadOnlyList<GearGridDefinition>>(StringComparer.Ordinal));
        }

        return new GearSlotFacts(templateIdByItemId, slotsByTemplateId, parentByTemplateId, stackCountByItemId, gridsByTemplateId);
    }

    // 자리 하나가 받는 템플릿 집합. Slots·Cartridges·Chambers 가 같은 모양(Props.Filters[].Filter)이다.
    // 비어 있으면 "제한 없음" 이 아니라 "모름" 이다.
    private static HashSet<string> AllowedTemplatesOf(Slot slot)
    {
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var filter in slot.Properties?.Filters ?? [])
        {
            foreach (var tpl in filter?.Filter ?? [])
            {
                if (!tpl.IsEmpty) allowed.Add(tpl.ToString());
            }
        }

        return allowed;
    }

    // tpl → `_parent` 사슬을 위로 모은다. 이미 들어 있는 고리를 만나면 멈춘다(두 번 걷지 않는다).
    private void CollectTemplateAncestry(MongoId templateId, Dictionary<string, string> parentByTemplateId)
    {
        var current = templateId;
        for (var step = 0; step < 32; step++)
        {
            var key = current.ToString();
            if (parentByTemplateId.ContainsKey(key)) return;

            var found = itemHelper.GetItem(current);
            if (!found.Key || found.Value == null || found.Value.Parent.IsEmpty) return;

            parentByTemplateId[key] = found.Value.Parent.ToString();
            current = found.Value.Parent;
        }
    }
}
