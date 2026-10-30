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
/// `ParentId`·`SlotId` 로 붙인다. **좌표는 넣지 않는다**: 바닐라도 자리 유무만 보고
/// 보내며 실제 배치는 클라이언트가 한다.
///
/// 바닐라와 다른 점은 **중첩**이다. 품목 안에 품목을 넣을 수 있다 —
/// 보급 상자가 10×12=120칸으로 고정이라 더 담으려면 안에 컨테이너를 넣는 수밖에 없고,
/// 무기를 케이스에 담을지 말지도 같은 구조로 고른다.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class CrateBuilder(ISptLogger<CrateBuilder> logger, ItemHelper itemHelper, SlotFiller slotFiller)
{
    /// <summary>중첩이 끝없이 깊어지는 것을 막는다. 가방 안의 가방 정도면 충분하다.</summary>
    private const int MaxDepth = 4;

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
    /// 조립 결과. 조용히 버리거나 조용히 고치지 않는다.
    ///
    /// 둘을 **따로** 둔다. <see cref="Skipped"/> 는 못 담은 것이고,
    /// <see cref="Adjusted"/> 는 담기는 했는데 적어 낸 대로는 아닌 것이다.
    /// 한 자루에 넣었더니 창이 "못 담은 것 1개" 라고 말하는데 정작 상자에는
    /// 들어 있는 꼴이 됐다.
    /// </summary>
    /// <param name="Placed">격자에 자리를 잡은 품목 수. 딸려 붙은 부속은 세지 않는다.</param>
    /// <param name="Attached">방어판·삽입물·헬멧 부속처럼 품목에 **붙은** 것의 수.</param>
    public record Result(
        GetAirdropLootResponse Response, int Placed, int Attached,
        List<string> Skipped, List<string> Adjusted);

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

        if (!TryOpenContainer(crateTpl, out var crateMap, out var crateSlot))
        {
            logger.Error($"[AirdropCaller] {dropId}: 상자 {crateTpl} 에 격자가 없다.");
            return null;
        }

        var items = new List<Item> { crate };
        var skipped = new List<string>();
        var adjusted = new List<string>();
        var tally = new Tally();

        Place(spec.Items, crate, crateMap!, crateSlot, items, skipped, adjusted, tally, depth: 0);

        return new Result(
            new GetAirdropLootResponse { Icon = icon, Container = items },
            tally.Placed,
            tally.Attached,
            Collapse(skipped),
            Collapse(adjusted));
    }

    /// <summary>
    /// 담은 것과 붙은 것을 따로 센다. 예전처럼 `items.Count - 1` 로 세면 방어판까지
    /// 품목 수에 섞여, 30개를 담았는데 창이 "78개" 라고 말하게 된다.
    /// </summary>
    private sealed class Tally
    {
        public int Placed;
        public int Attached;
    }

    /// <summary>
    /// 같은 말을 여러 줄로 늘어놓지 않는다. 방탄복 20개를 120칸 상자에 넣으면
    /// "자리 없음" 이 열네 줄로 돌아왔다 — 창에서 읽을 수 있는 문구가 아니다.
    /// </summary>
    private static List<string> Collapse(List<string> lines) =>
        lines.GroupBy(line => line)
             .Select(g => g.Count() > 1 ? $"{g.Key} ×{g.Count()}" : g.Key)
             .ToList();

    /// <summary>
    /// <paramref name="parent"/> 안에 <paramref name="entries"/> 를 넣는다.
    /// 자리가 없으면 넣지 않고 <paramref name="skipped"/> 에 적는다.
    /// </summary>
    private void Place(
        List<ItemSpec>? entries,
        Item parent,
        int[,] parentMap,
        string parentSlot,
        List<Item> items,
        List<string> skipped,
        List<string> adjusted,
        Tally tally,
        int depth)
    {
        if (entries is null || entries.Count == 0)
        {
            return;
        }

        if (depth >= MaxDepth)
        {
            skipped.Add($"{entries.Count}종 (너무 깊이 겹쳤다 — {MaxDepth}겹까지만 담는다)");
            return;
        }

        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Tpl))
            {
                continue;
            }

            var stack = StackFor(entry, adjusted);

            for (var i = 0; i < Math.Max(1, entry.Count); i++)
            {
                var item = new Item
                {
                    Id = new MongoId(),
                    Template = new MongoId(entry.Tpl),
                    ParentId = parent.Id,
                    SlotId = parentSlot,
                    Upd = new Upd
                    {
                        SpawnedInSession = true,
                        StackObjectsCount = stack,
                    },
                };

                // 딸린 칸을 **자리를 보기 전에** 채운다. 붙은 것이 크기를 바꾼다 —
                // 개머리판을 단 총은 빈 총보다 넓다. 빈 몸으로 재 놓고 채워 넣으면
                // 격자 지도가 실제와 어긋난다.
                var attached = slotFiller.Fill(item, entry.Fill ?? SlotFill.Auto);

                var size = itemHelper.GetItemSize([item, .. attached], item.Id);
                if (size is null)
                {
                    skipped.Add($"{entry.Tpl} (크기를 모른다 — 없는 아이템일 수 있다)");
                    continue;
                }

                var slot = parentMap.FindSlotForItem(size.Width, size.Height);
                if (slot.Success != true)
                {
                    skipped.Add($"{entry.Tpl} (자리 없음)");
                    continue;
                }

                parentMap.TryFillContainerMapWithItem(
                    slot.X!.Value, slot.Y!.Value, size.Width, size.Height, slot.Rotation ?? false, out _);
                items.Add(item);
                items.AddRange(attached);
                tally.Placed++;
                tally.Attached += attached.Count;

                if (entry.Items is null || entry.Items.Count == 0)
                {
                    continue;
                }

                // 안에 넣을 것이 있다. 이 품목이 격자를 가진 컨테이너여야 한다.
                if (!TryOpenContainer(item.Template, out var childMap, out var childSlot))
                {
                    skipped.Add($"{entry.Tpl} 안의 {entry.Items.Count}종 (이 품목은 담을 수 없다 — 격자가 없다)");
                    continue;
                }

                Place(entry.Items, item, childMap!, childSlot, items, skipped, adjusted, tally, depth + 1);
            }
        }
    }

    /// <summary>
    /// 적어 낸 묶음을 **그 아이템이 실제로 묶이는 만큼까지만** 받는다.
    ///
    /// 적은 값을 그대로 내보내면 게임이 최대치를 넘는 묶음을 들고 있게 되고, 레이드가
    /// 끝날 때 서버가 그 아이템을 어떻게 받아 줄지 알 수 없다 — 설계 §5 안티목표 1
    /// (프로필을 망가뜨리지 않는다)에 바로 걸린다. 넘치면 깎고 **말해 준다.**
    ///
    /// 최대치는 아이템 DB 에서 읽는다. 스택을 바꾸는 서버 모드가 여럿 깔려 있어
    /// (BarterItemsStacks 등) 어느 설정 파일도 진실이 아니다 — 다 지나간 뒤의 DB 만 맞다.
    /// </summary>
    private double StackFor(ItemSpec entry, List<string> adjusted)
    {
        var wanted = entry.StackSize is > 0 ? entry.StackSize.Value : 1;

        var (ok, template) = itemHelper.GetItem(new MongoId(entry.Tpl));
        if (!ok || template?.Properties?.StackMaxSize is not { } raw)
        {
            // 크기를 모르는 아이템은 아래에서 어차피 걸러진다. 여기서는 그대로 둔다.
            return wanted;
        }

        var max = Math.Max(1, Convert.ToDouble(raw));
        if (wanted <= max)
        {
            return wanted;
        }

        adjusted.Add($"{entry.Tpl} (묶음 {wanted} → {max} 로 깎았다 — 이 아이템은 그만큼만 묶인다)");
        return max;
    }

    /// <summary>
    /// 격자를 가진 컨테이너면 빈 격자 지도와 칸 이름을 돌려준다.
    ///
    /// `ItemHelper.GetContainerMapping` 은 **첫 격자만** 보고 격자가 없으면 던진다.
    /// 밖에서 오는 tpl 로 부르는 자리라 던지게 둘 수 없어 감싼다. 격자가 여럿인
    /// 물건(조끼 등)은 **첫 칸만** 쓴다 — 적게 담을지언정 틀리게 담지는 않는다.
    /// </summary>
    private bool TryOpenContainer(MongoId tpl, out int[,]? map, out string slotId)
    {
        map = null;
        slotId = "main";

        var (ok, template) = itemHelper.GetItem(tpl);
        var grids = template?.Properties?.Grids;
        if (!ok || grids is null)
        {
            return false;
        }

        var first = grids.FirstOrDefault();
        if (first is null)
        {
            return false;
        }
        if (first.Properties?.CellsH is not > 0 || first.Properties?.CellsV is not > 0)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(first.Name))
        {
            slotId = first.Name!;
        }

        try
        {
            map = itemHelper.GetContainerMapping(tpl);
            return true;
        }
        catch (Exception e)
        {
            logger.Error($"[AirdropCaller] {tpl} 의 격자를 읽지 못했다: {e.Message}");
            return false;
        }
    }
}
