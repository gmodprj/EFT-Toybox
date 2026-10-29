using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Location;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;

namespace AirdropCaller.Server;

/// <summary>
/// 설정에 적힌 품목을 바닐라와 같은 모양의 상자로 조립한다.
///
/// 바닐라(`AirdropService.GenerateAirdropLoot`)가 하는 것을 그대로 따른다 —
/// 상자 아이템 하나를 만들고, 담을 것마다 격자에 자리가 있는지 보고, 들어가는 것만
/// `ParentId` = 상자 id, `SlotId` = "main" 으로 붙인다. **좌표는 넣지 않는다**:
/// 바닐라도 자리 유무만 보고 보내며 실제 배치는 클라이언트가 한다.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class CrateBuilder(ISptLogger<CrateBuilder> logger, ItemHelper itemHelper)
{
    /// <summary>
    /// 상자 종류 → 상자 템플릿. 바닐라 `GetAirdropCrateItem` 의 짝을 그대로 쓴다.
    /// </summary>
    private static MongoId CrateTemplateFor(AirdropTypeEnum icon) => icon switch
    {
        AirdropTypeEnum.Medical => ItemTpl.LOOTCONTAINER_AIRDROP_MEDICAL_CRATE,
        AirdropTypeEnum.Supply => ItemTpl.LOOTCONTAINER_AIRDROP_SUPPLY_CRATE,
        AirdropTypeEnum.Weapon => ItemTpl.LOOTCONTAINER_AIRDROP_WEAPON_CRATE,
        _ => ItemTpl.LOOTCONTAINER_AIRDROP_COMMON_SUPPLY_CRATE,
    };

    /// <summary>
    /// 조립 결과. 안 들어간 품목이 있으면 <see cref="Skipped"/> 에 남는다 —
    /// 조용히 버리지 않는다.
    /// </summary>
    public record Result(GetAirdropLootResponse Response, int Placed, List<string> Skipped);

    public Result? Build(string dropId, DropSpec spec)
    {
        if (!Enum.TryParse<AirdropTypeEnum>(spec.Icon, ignoreCase: true, out var icon))
        {
            logger.Error($"[AirdropCaller] {dropId}: Icon '{spec.Icon}' 을 모른다. "
                       + "Common / Supply / Medical / Weapon 중 하나여야 한다.");
            return null;
        }

        var crateTpl = CrateTemplateFor(icon);
        var crate = new Item
        {
            Id = new MongoId(),
            Template = crateTpl,
            Upd = new Upd { SpawnedInSession = true, StackObjectsCount = 1.0 },
        };

        // 이 격자가 "무엇이 들어가는가" 를 정한다. 상자 종류마다 다르다.
        var map = itemHelper.GetContainerMapping(crateTpl);

        var items = new List<Item> { crate };
        var skipped = new List<string>();

        foreach (var entry in spec.Items)
        {
            if (string.IsNullOrWhiteSpace(entry.Tpl))
            {
                continue;
            }

            for (var i = 0; i < Math.Max(1, entry.Count); i++)
            {
                var item = new Item
                {
                    Id = new MongoId(),
                    Template = new MongoId(entry.Tpl),
                    ParentId = crate.Id,
                    SlotId = "main",
                    Upd = new Upd
                    {
                        SpawnedInSession = true,
                        StackObjectsCount = entry.StackSize is > 0 ? entry.StackSize.Value : 1.0,
                    },
                };

                var size = itemHelper.GetItemSize([item], item.Id);
                if (size is null)
                {
                    skipped.Add($"{entry.Tpl} (크기를 모른다 — 없는 아이템일 수 있다)");
                    continue;
                }

                var slot = map.FindSlotForItem(size.Width, size.Height);
                if (slot.Success != true)
                {
                    skipped.Add($"{entry.Tpl} (자리 없음)");
                    continue;
                }

                map.TryFillContainerMapWithItem(
                    slot.X!.Value, slot.Y!.Value, size.Width, size.Height, slot.Rotation ?? false, out _);
                items.Add(item);
            }
        }

        return new Result(
            new GetAirdropLootResponse { Icon = icon, Container = items },
            items.Count - 1,
            skipped);
    }
}
