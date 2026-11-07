using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Helpers.Profile;
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
public class CrateBuilder(
    ISptLogger<CrateBuilder> logger, ItemHelper itemHelper, InventoryHelper inventoryHelper,
    SlotFiller slotFiller, BigCrate bigCrate)
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

        // 큰 상자를 못 만들었으면 조용히 작은 상자로 보내지 않는다 — 900칸을 믿고
        // 적어 둔 품목이 "자리 없음" 으로 쏟아지는 쪽이 더 나쁘다.
        if (spec.BigCrate && !bigCrate.Ready)
        {
            logger.Error($"[AirdropCaller] {dropId}: 큰 상자를 못 만들어 둔 채 큰 상자를 달라고 했다. "
                       + "서버가 켜질 때의 기록을 본다.");
            return null;
        }

        var crateTpl = spec.BigCrate ? BigCrate.Tpl : CrateTemplateFor(icon);
        var crate = new Item
        {
            Id = new MongoId(),
            Template = crateTpl,
            Upd = new Upd { SpawnedInSession = true, StackObjectsCount = 1.0 },
        };

        var crateSpots = OpenContainer(crate, crateTpl);
        if (crateSpots.Count == 0)
        {
            logger.Error($"[AirdropCaller] {dropId}: 상자 {crateTpl} 에 격자가 없다.");
            return null;
        }

        var items = new List<Item> { crate };
        var skipped = new List<string>();
        var adjusted = new List<string>();
        var tally = new Tally();

        Place(spec.Items, crateSpots, items, skipped, adjusted, tally, depth: 0);

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
    /// <summary>담을 수 있는 자리 하나 — 그릇과 그 격자 지도.</summary>
    private sealed record Spot(Item Item, int[,] Map, string Slot);

    /// <summary>
    /// <paramref name="entries"/> 를 <paramref name="targets"/> 에 담는다.
    ///
    /// **그릇이 여럿이면 나눠 담는다.** 앞엣것이 차면 다음 그릇으로 넘어간다.
    /// 전에는 그릇 하나하나마다 안에 든 목록을 통째로 넣었다 — 가방 3개에 음료
    /// 10개를 적으면 30개를 시도해 12개만 들어가고 18개가 "자리 없음" 으로
    /// 빠졌다(실측 2026-10-30). `Count` 는 "이 품목 몇 개" 이지 "그릇마다 몇 개"
    /// 가 아니다.
    /// </summary>
    private void Place(
        List<ItemSpec>? entries,
        IReadOnlyList<Spot> targets,
        List<Item> items,
        List<string> skipped,
        List<string> adjusted,
        Tally tally,
        int depth)
    {
        if (entries is null || entries.Count == 0 || targets.Count == 0)
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
            var wantsChildren = entry.Items is { Count: > 0 };

            // 이 품목이 그릇이면 만든 것들을 모아 둔다. 안에 넣을 것은 그 뒤에
            // **이 그릇들에 나눠** 담는다.
            var copies = new List<Spot>();

            for (var i = 0; i < Math.Max(1, entry.Count); i++)
            {
                var item = new Item
                {
                    Id = new MongoId(),
                    Template = new MongoId(entry.Tpl),
                    Upd = new Upd
                    {
                        SpawnedInSession = true,
                        StackObjectsCount = stack,
                    },
                };

                // 딸린 칸을 **자리를 보기 전에** 채운다. 붙은 것이 크기를 바꾼다 —
                // 개머리판을 단 총은 빈 총보다 넓다. 빈 몸으로 재 놓고 채워 넣으면
                // 격자 지도가 실제와 어긋난다.
                var attached = slotFiller.Fill(item, entry.Fill ?? SlotFill.Auto, entry.Slots, adjusted);

                var size = itemHelper.GetItemSize([item, .. attached], item.Id);
                if (size is null)
                {
                    skipped.Add($"{entry.Tpl} (크기를 모른다 — 없는 아이템일 수 있다)");
                    continue;
                }

                // 그릇을 차례로 두드린다. 첫 그릇이 차 있으면 다음 그릇이 받는다.
                var home = Settle(targets, size.Width, size.Height);
                if (home is null)
                {
                    skipped.Add($"{entry.Tpl} (자리 없음)");
                    continue;
                }

                item.ParentId = home.Item.Id;
                item.SlotId = home.Slot;

                items.Add(item);
                items.AddRange(attached);
                tally.Placed++;
                tally.Attached += attached.Count;

                if (!wantsChildren)
                {
                    continue;
                }

                // 안에 넣을 것이 있다. 이 품목이 격자를 가진 그릇이어야 한다.
                var childSpots = OpenContainer(item, item.Template);
                if (childSpots.Count > 0)
                {
                    copies.AddRange(childSpots);
                }
                else
                {
                    skipped.Add($"{entry.Tpl} 안의 {entry.Items!.Count}종 (이 품목은 담을 수 없다 — 격자가 없다)");
                }
            }

            if (wantsChildren && copies.Count > 0)
            {
                Place(entry.Items, copies, items, skipped, adjusted, tally, depth + 1);
            }
        }
    }

    /// <summary>
    /// 그릇들을 차례로 두드려 자리를 잡는다. 잡으면 그 격자를 채우고 그 그릇을
    /// 돌려준다. 어디에도 안 들어가면 null.
    /// </summary>
    private static Spot? Settle(IReadOnlyList<Spot> targets, int width, int height)
    {
        foreach (var target in targets)
        {
            var slot = target.Map.FindSlotForItem(width, height);
            if (slot.Success != true)
            {
                continue;
            }

            target.Map.TryFillContainerMapWithItem(
                slot.X!.Value, slot.Y!.Value, width, height, slot.Rotation ?? false, out _);
            return target;
        }

        return null;
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
    /// <summary>
    /// 용기의 **격자를 전부** 연다 — 격자 하나가 그릇(<see cref="Spot"/>) 하나다.
    ///
    /// 전에는 `grids.FirstOrDefault()` 로 **첫 격자만** 열었다. 가방처럼 격자가 하나인
    /// 것은 멀쩡했지만 리그·워벨트·주머니는 격자가 여러 개라 나머지 칸을 못 썼다 —
    /// 실측(2026-11-07): 워벨트 격자 **11개 중 1개**(25칸 중 6칸), 주머니·리그 **4개 중
    /// 1개**(8칸 중 2칸). 그래서 장비를 통째로 뽑아 넣으면 "자리 없음" 이 쏟아졌다.
    ///
    /// 격자마다 **자기 지도**를 만든다. `GetContainerMapping` 은 여러 격자를 **하나로
    /// 합쳐** 버려서 쓸 수 없다 — 칸 수는 맞아 보여도 `SlotId` 가 한 격자를 가리킨다.
    ///
    /// <see cref="Settle"/> 가 이미 "그릇을 차례로 두드린다" 라서 호출부는 그대로다.
    /// </summary>
    private List<Spot> OpenContainer(Item owner, MongoId tpl)
    {
        var spots = new List<Spot>();

        var (ok, template) = itemHelper.GetItem(tpl);
        var grids = template?.Properties?.Grids;
        if (!ok || grids is null)
        {
            return spots;
        }

        foreach (var grid in grids)
        {
            var h = grid?.Properties?.CellsH;
            var v = grid?.Properties?.CellsV;
            if (h is not > 0 || v is not > 0)
            {
                continue;
            }

            var name = string.IsNullOrWhiteSpace(grid!.Name) ? "main" : grid.Name!;
            try
            {
                // 새 상자라 안에 든 것이 없다. 빈 목록으로 격자 하나짜리 지도를 받는다.
                var map = inventoryHelper.GetContainerMap(h.Value, v.Value, [], owner.Id);
                spots.Add(new Spot(owner, map, name));
            }
            catch (Exception e)
            {
                // 격자 하나를 못 열어도 나머지는 쓴다. 조용히 넘기지 않는다.
                logger.Error($"[AirdropCaller] {tpl} 의 격자 {name} 을 열지 못했다: {e.Message}");
            }
        }

        return spots;
    }
}
