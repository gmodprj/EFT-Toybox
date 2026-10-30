using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace AirdropCaller.Server;

/// <summary>품목의 빈 칸(슬롯)을 어디까지 채울지.</summary>
public enum SlotFill
{
    /// <summary>
    /// 방어구면 삽입물과 방어판을 채우고, 그 밖의 물건은 건드리지 않는다. 기본값.
    ///
    /// 삽입물이 빠진 방어구는 게임에서 **보호력이 0** 인 껍데기다. 적어 낸 사람이
    /// 바란 물건이 아니므로 이쪽을 기본으로 둔다.
    /// </summary>
    Auto,

    /// <summary>모든 칸을 채운다. 헬멧이면 야시경·마운트·바이저까지 붙는다.</summary>
    All,

    /// <summary>아무것도 안 붙인다. 적어 낸 그대로 빈 몸으로 나간다.</summary>
    None,
}

/// <summary>
/// 품목에 딸린 칸(`Slots`)을 채운다 — 방어판·삽입물·헬멧 부속 따위.
///
/// SPT 에도 <c>ItemHelper.AddChildSlotItems</c> 가 있지만 쓰지 않는다. 그쪽은
/// 후보 중에서 **무작위로** 하나를 고른다. 보급 상자에 방탄복 열 벌을 넣으면 방어
/// 등급이 3에서 6까지 제각각으로 나온다는 뜻이고, 같은 프리셋을 두 번 불러도 다른
/// 것이 나온다. 부르는 쪽이 고른 물건을 서버가 주사위로 바꿔 버리는 셈이다.
///
/// 그래서 고르는 규칙을 우리가 쥔다 — **핸드북 값이 가장 비싼 것**. 실측해 보니
/// 이 한 줄이 바라는 답을 모두 냈다(2026-10-30, 이 설치본의 DB):
///
/// <list type="bullet">
/// <item>IOTV 의 <c>Front_plate</c> 후보 18개 → GAC 4sss2(85,002원, 방어 6등급)</item>
/// <item>FAST MT 의 <c>mod_nvg</c> 후보 3개 → GPNVG-18(145,004원)</item>
/// <item>FAST MT 의 <c>mod_equipment_000</c> 후보 3개 → 멀티 히트 안면 보호 바이저</item>
/// </list>
///
/// 방어 등급을 먼저 보는 규칙도 해 봤는데 <c>mod_nvg</c> 에서 엉뚱한 답이 났다 —
/// 후보 중 Tac-Kek 마스크만 등급(2)을 들고 있어서 야시경 대신 마스크가 뽑혔다.
/// 값만 보는 쪽이 세 자리 모두 맞았다.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class SlotFiller(ItemHelper itemHelper, TemplateTable templateTable)
{
    /// <summary>마운트에 마운트에 마운트를 끝없이 달지 않는다.</summary>
    private const int MaxDepth = 3;

    private readonly object _lock = new();
    private Dictionary<MongoId, double>? _price;

    /// <summary>
    /// <paramref name="root"/> 에 붙일 자식들을 만든다. 격자 칸을 쓰지 않으므로
    /// 자리 계산은 필요 없지만, **크기는 바뀔 수 있다** — 부른 쪽이
    /// <c>GetItemSize</c> 에 이 목록을 함께 넘겨야 한다.
    /// </summary>
    public List<Item> Fill(Item root, SlotFill mode)
    {
        var added = new List<Item>();
        if (mode == SlotFill.None)
        {
            return added;
        }

        FillInto(root, mode, blocked: [], used: [], added, depth: 0);
        return added;
    }

    private void FillInto(
        Item parent,
        SlotFill mode,
        HashSet<MongoId> blocked,
        HashSet<MongoId> used,
        List<Item> added,
        int depth)
    {
        if (depth >= MaxDepth)
        {
            return;
        }

        var (ok, template) = itemHelper.GetItem(parent.Template);
        if (!ok || template?.Properties?.Slots is not { } slots)
        {
            return;
        }

        // 자동은 방어구에만 손댄다. 무기에 부품을 멋대로 달아 주는 것은 다른 이야기다.
        var armorOnly = mode == SlotFill.Auto;
        if (armorOnly && !itemHelper.ArmorItemHasRemovableOrSoftInsertSlots(parent.Template))
        {
            return;
        }

        foreach (var slot in slots)
        {
            var name = slot.Name ?? string.Empty;
            var required = slot.Required == true;

            // 자동이 채우는 칸: 반드시 있어야 하는 것(삽입물)과 탈착식 방어판.
            if (armorOnly && !required && !itemHelper.IsRemovablePlateSlot(name))
            {
                continue;
            }

            var pool = slot.Properties?.Filters?.FirstOrDefault()?.Filter;
            if (pool is null || pool.Count == 0)
            {
                continue;
            }

            // 같은 것을 두 번 달지 않는다 — 헬멧에 야시경, 그 헬멧의 마운트에 또 야시경이
            // 붙는 꼴을 막는다. 다만 **반드시 있어야 하는 칸은 예외다**: 아라미드 삽입물
            // 하나가 깃·어깨·사타구니에 똑같이 들어가야 해서, 막으면 첫 칸만 채워진다.
            var pick = Best(pool, blocked, required ? null : used);
            if (pick is not { } tpl)
            {
                continue;
            }

            var child = new Item
            {
                Id = new MongoId(),
                Template = tpl,
                ParentId = parent.Id,
                SlotId = name,
                Upd = new Upd { SpawnedInSession = true, StackObjectsCount = 1.0 },
            };

            added.Add(child);
            if (!required)
            {
                used.Add(tpl);
            }

            // 같이 달 수 없는 것을 뒤이어 고르지 않는다. SPT 가 하는 것과 같다.
            var (hit, chosen) = itemHelper.GetItem(tpl);
            if (hit && chosen?.Properties?.ConflictingItems is { } conflicts)
            {
                blocked.UnionWith(conflicts);
            }

            FillInto(child, mode, blocked, used, added, depth + 1);
        }
    }

    /// <summary>
    /// 후보 중 핸드북 값이 가장 비싼 것. 값이 같으면 tpl 순서로 끊는다 —
    /// **같은 프리셋을 두 번 부르면 같은 것이 나와야 한다.**
    /// </summary>
    private MongoId? Best(IEnumerable<MongoId> pool, HashSet<MongoId> blocked, HashSet<MongoId>? used)
    {
        MongoId? best = null;
        var bestPrice = double.NegativeInfinity;

        foreach (var tpl in pool)
        {
            if (blocked.Contains(tpl) || used?.Contains(tpl) == true)
            {
                continue;
            }

            // 같이 달 수 없는 것을 **이쪽에서도** 본다. 충돌 목록은 보통 양쪽에
            // 다 적혀 있지만, 한쪽에만 적힌 것이 있어도 걸러진다.
            var (ok, template) = itemHelper.GetItem(tpl);
            if (!ok || template is null)
            {
                continue;
            }

            var price = PriceOf(tpl);
            if (price < bestPrice)
            {
                continue;
            }

            if (price > bestPrice || best is null || tpl.CompareTo(best.Value) < 0)
            {
                best = tpl;
                bestPrice = price;
            }
        }

        return best;
    }

    private double PriceOf(MongoId tpl)
    {
        EnsurePrices();
        return _price!.GetValueOrDefault(tpl, 0);
    }

    /// <summary>
    /// 핸드북 값 표를 한 번만 만든다. 핸드북은 2만 줄쯤 되어 칸마다 훑을 것이 아니다.
    /// </summary>
    private void EnsurePrices()
    {
        lock (_lock)
        {
            if (_price is not null)
            {
                return;
            }

            var table = new Dictionary<MongoId, double>();
            foreach (var entry in templateTable.Handbook?.Items ?? [])
            {
                table[entry.Id] = Convert.ToDouble(entry.Price ?? 0);
            }

            _price = table;
        }
    }
}
