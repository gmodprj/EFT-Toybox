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

/// <summary>칸 하나를 어떻게 처리하기로 했는지. 채우는 쪽과 보여 주는 쪽이 같이 쓴다.</summary>
/// <param name="Path">`Front_plate` 또는 `mod_mount.mod_nvg`. 겹친 칸은 점으로 잇는다.</param>
/// <param name="Depth">0 이 품목 바로 밑이다. 창이 들여쓰기에 쓴다.</param>
/// <param name="Kind">`soft`(삽입물) · `plate`(방어판) · `other`(장비).</param>
/// <param name="Auto">자동이 고를 것. 후보가 없거나 다 막혔으면 null.</param>
/// <param name="Chosen">실제로 붙일 것. null 이면 이 칸은 빈다.</param>
/// <param name="Picked">사용자가 직접 고른 칸인가. 자동이 고른 것과 구별한다.</param>
public record SlotDecision(
    string Path,
    int Depth,
    string Name,
    bool Required,
    string Kind,
    IReadOnlyList<MongoId> Pool,
    MongoId? Auto,
    MongoId? Chosen,
    bool Picked);

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
/// <item>IOTV 의 <c>Front_plate</c> 후보 18개 → 방어 7등급 판(모드가 넣은 것)</item>
/// <item>FAST MT 의 <c>mod_nvg</c> 후보 3개 → GPNVG-18</item>
/// <item>FAST MT 의 <c>mod_equipment_000</c> 후보 3개 → 멀티 히트 안면 보호 바이저</item>
/// </list>
///
/// 방어 등급을 먼저 보는 규칙도 해 봤는데 <c>mod_nvg</c> 에서 엉뚱한 답이 났다 —
/// 후보 중 Tac-Kek 마스크만 등급(2)을 들고 있어서 야시경 대신 마스크가 뽑혔다.
/// 값만 보는 쪽이 세 자리 모두 맞았다.
///
/// <para>
/// 사용자가 칸마다 직접 고른 것(<c>ItemSpec.Slots</c>)이 있으면 그쪽이 이긴다.
/// **보여 주는 쪽과 채우는 쪽이 같은 코드를 쓴다**(<see cref="Walk"/>) — 창이
/// "자동: A" 라고 적어 놓고 상자에 B 가 들어 있는 것이 제일 나쁘다.
/// </para>
/// </summary>
[Injectable(InjectionType.Singleton)]
public class SlotFiller(ItemHelper itemHelper, TemplateTable templateTable, PresetHelper presetHelper)
{
    /// <summary>마운트에 마운트에 마운트를 끝없이 달지 않는다.</summary>
    private const int MaxDepth = 3;

    /// <summary>탄창 칸 이름. 게임이 쓰는 그대로다.</summary>
    private const string MagazineSlot = "mod_magazine";

    private readonly object _lock = new();
    private Dictionary<MongoId, double>? _price;

    /// <summary>
    /// <paramref name="root"/> 에 붙일 자식들을 만든다. 격자 칸을 쓰지 않으므로
    /// 자리 계산은 필요 없지만, **크기는 바뀔 수 있다** — 부른 쪽이
    /// <c>GetItemSize</c> 에 이 목록을 함께 넘겨야 한다.
    /// </summary>
    /// <param name="picks">칸 경로 → tpl. 빈 문자열은 "비워 둬라" 다.</param>
    /// <param name="notes">못 쓴 지정을 적는다. 조용히 버리지 않는다.</param>
    public List<Item> Fill(
        Item root, SlotFill mode, IReadOnlyDictionary<string, string>? picks, List<string> notes)
    {
        var added = new List<Item>();

        // 경로 → 그 자리에 붙은 아이템. 빈 경로가 뿌리다.
        var at = new Dictionary<string, Item> { [string.Empty] = root };

        // ① 완성품이 있는 물건(무기)은 그것으로 세운다. 빈 몸으로 보내면 총열도
        //    노리쇠도 손잡이도 없는 껍데기가 간다 — 삽입물 없는 방어구와 같은 일이다.
        var taken = new HashSet<string>(StringComparer.Ordinal);
        if (mode != SlotFill.None)
        {
            AddDefaultPreset(root, added, at, taken);
        }

        // ② 남은 칸을 모드대로 채운다. 완성품이 이미 쓴 칸은 건드리지 않는다.
        Walk(root.Template, mode, picks, notes, decision =>
        {
            // 사용자가 그 칸을 직접 골랐으면 완성품보다 그쪽이 세다.
            if (taken.Contains(decision.Path) && !decision.Picked)
            {
                return;
            }

            if (decision.Chosen is not { } tpl)
            {
                return;
            }

            var parentPath = Parent(decision.Path);
            if (!at.TryGetValue(parentPath, out var parent))
            {
                // 윗칸이 안 붙었으면 아랫칸도 갈 곳이 없다. Walk 가 그런 길로는
                // 내려가지 않지만, 믿고 넘어가지는 않는다.
                return;
            }

            var child = new Item
            {
                Id = new MongoId(),
                Template = tpl,
                ParentId = parent.Id,
                SlotId = decision.Name,
                Upd = new Upd { SpawnedInSession = true, StackObjectsCount = 1.0 },
            };

            added.Add(child);
            at[decision.Path] = child;
        });

        return added;
    }

    /// <summary>
    /// 칸을 **채우지 않고** 훑기만 한다. 창이 "어떤 칸이 있고, 거기에 뭘 달 수 있고,
    /// 자동은 뭘 고르는지" 를 보여 줄 때 쓴다.
    /// </summary>
    public List<SlotDecision> Describe(
        MongoId rootTpl, SlotFill mode, IReadOnlyDictionary<string, string>? picks)
    {
        var found = new List<SlotDecision>();
        Walk(rootTpl, mode, picks, notes: null, found.Add);
        return found;
    }

    /// <summary>
    /// 게임이 정해 둔 **완성품**(기본 프리셋)으로 세운다. 바닐라 보급이 무기를 넣을 때
    /// 쓰는 그것이다.
    ///
    /// 이것이 없으면 무기는 `mod_reciever`·`mod_pistol_grip`·`mod_charge` 가 전부 빈
    /// 껍데기로 간다. 셋 다 `_required` 다 — 총이 아니다. 실측한 것(2026-10-30):
    /// MCX-SPEAR 가 **0.286 kg** 으로 떨어졌다(제대로면 4 kg 대).
    ///
    /// 창에 무기가 `1x1` 로 보이던 것도 같은 뿌리다. 무기의 템플릿 크기는 원래 1×1 이고
    /// 실제 크기는 **붙은 부품의 `ExtraSize*`** 에서 나온다. 빈 몸이면 영영 1×1 이다.
    /// </summary>
    private void AddDefaultPreset(
        Item root, List<Item> added, Dictionary<string, Item> at, HashSet<string> taken)
    {
        var preset = presetHelper.GetDefaultPreset(root.Template);
        if (preset?.Items is not { Count: > 1 } items)
        {
            return;
        }

        // 프리셋의 id 를 그대로 쓰면 한 상자에 같은 총 두 자루를 넣었을 때 id 가 겹친다.
        // SPT 가 주는 도구로 뿌리에 맞춰 새 id 를 발급받는다.
        var built = itemHelper.ReparentItemAndChildren(root, [.. items]);

        // 경로를 만들려면 부모를 거슬러 올라가야 한다. id → 아이템 으로 먼저 깔아 둔다.
        var byId = built.ToDictionary(i => i.Id.ToString(), i => i);
        var rootId = built[0].Id.ToString();

        foreach (var part in built.Skip(1))
        {
            part.Upd ??= new Upd();
            part.Upd.SpawnedInSession = true;

            var path = PathOf(part, byId, rootId);
            if (path is null)
            {
                continue;
            }

            added.Add(part);
            at[path] = part;
            taken.Add(path);
        }
    }

    /// <summary>부모를 거슬러 올라가 `mod_mount.mod_nvg` 꼴의 경로를 만든다.</summary>
    private static string? PathOf(Item part, IReadOnlyDictionary<string, Item> byId, string rootId)
    {
        var bits = new List<string>();
        var at = part;

        // 뿌리까지 올라간다. 고리가 있으면(있을 리 없지만) 깊이로 끊는다.
        for (var depth = 0; depth < 16; depth++)
        {
            if (string.IsNullOrEmpty(at.SlotId))
            {
                return null;
            }

            bits.Add(at.SlotId!);

            var parentId = at.ParentId?.ToString();
            if (parentId is null)
            {
                return null;
            }

            if (parentId == rootId)
            {
                bits.Reverse();
                return string.Join('.', bits);
            }

            if (!byId.TryGetValue(parentId, out var next))
            {
                return null;
            }

            at = next;
        }

        return null;
    }

    private static string Parent(string path)
    {
        var cut = path.LastIndexOf('.');
        return cut < 0 ? string.Empty : path[..cut];
    }

    private void Walk(
        MongoId rootTpl,
        SlotFill mode,
        IReadOnlyDictionary<string, string>? picks,
        List<string>? notes,
        Action<SlotDecision> emit)
    {
        // `blocked` 와 `used` 는 나무 전체가 함께 쓴다 — 헬멧에 야시경을 달아 놓고
        // 그 헬멧의 마운트에 또 야시경을 다는 꼴을 막는다.
        WalkInto(rootTpl, string.Empty, 0, mode, picks, notes, blocked: [], used: [], emit);
    }

    private void WalkInto(
        MongoId ownerTpl,
        string prefix,
        int depth,
        SlotFill mode,
        IReadOnlyDictionary<string, string>? picks,
        List<string>? notes,
        HashSet<MongoId> blocked,
        HashSet<MongoId> used,
        Action<SlotDecision> emit)
    {
        if (depth >= MaxDepth)
        {
            return;
        }

        var (ok, template) = itemHelper.GetItem(ownerTpl);
        if (!ok || template?.Properties?.Slots is null)
        {
            return;
        }

        var slots = template.Properties.Slots.ToList();
        if (slots.Count == 0)
        {
            return;
        }

        var isArmor = itemHelper.ArmorItemHasRemovableOrSoftInsertSlots(ownerTpl);

        // **직접 고른 칸을 먼저 본다.** 자동이 먼저 집어 가면, 사용자가 고른 것이
        // "같이 못 단다" 에 걸려 떨어지는 일이 생긴다. 고른 쪽이 이겨야 한다.
        var ordered = slots
            .Select(slot => (Slot: slot, Path: Join(prefix, slot.Name ?? string.Empty)))
            .OrderByDescending(x => picks?.ContainsKey(x.Path) == true)
            .ToList();

        foreach (var (slot, path) in ordered)
        {
            var name = slot.Name ?? string.Empty;
            if (name.Length == 0)
            {
                continue;
            }

            var required = slot.Required == true;
            var isPlate = itemHelper.IsRemovablePlateSlot(name);

            // 탄창은 `_required` 가 아니다. 그런데 **완성품으로 세운 총은 프리셋에
            // 탄창이 들어 있어** 받는다. 필수 칸만 채우는 쪽만 빈손이 되면 같은
            // `자동` 인데 결과가 갈린다 — 쏘려면 탄창을 따로 챙겨야 한다.
            // "필수만" 이라는 규칙에 내는 구멍은 이 하나뿐이다.
            var isMagazine = name.Equals(MagazineSlot, StringComparison.OrdinalIgnoreCase);
            var kind = isPlate ? "plate" : IsSoftInsert(name) ? "soft" : "other";
            var pool = (IReadOnlyList<MongoId>)(slot.Properties?.Filters?.FirstOrDefault()?.Filter?.ToList()
                                                ?? []);

            // 방어판·삽입물은 같은 것이 여러 칸에 들어가도 된다. 막았더니 앞판은
            // 7등급인데 뒷판은 6등급이 됐다(실측 2026-10-30).
            var sharable = required || isPlate;
            var auto = pool.Count == 0 ? null : Best(pool, blocked, sharable ? null : used);

            var deeper = picks is not null && picks.Keys.Any(k => k.StartsWith(path + ".", StringComparison.Ordinal));
            var picked = picks is not null && picks.TryGetValue(path, out var raw);
            MongoId? chosen;

            if (picked)
            {
                picks!.TryGetValue(path, out var raw2);
                chosen = Resolve(raw2, pool, path, notes);
            }
            else
            {
                var wanted = mode switch
                {
                    // 더 깊은 자리를 직접 골랐으면 거기까지 가는 길은 내준다.
                    SlotFill.None => deeper,

                    // **반드시 있어야 하는 칸은 종류를 가리지 않고 채운다.**
                    //
                    // 완성품(기본 프리셋)이 있는 물건은 이미 그 칸이 차 있어 여기까지
                    // 안 온다(`taken`). 여기 오는 것은 **완성품이 없는 물건**이다 —
                    // 디스크 DB 기준으로 칸 있는 무기 152종 중 5종이 그렇다
                    // (FN SCAR-H X-17, PKTM, AK-50, NSV, AGS-30). 그대로 두면 손잡이도
                    // 총몸도 없는 껍데기가 간다. 프리셋이 있다는 전제만 보고 없는 경우를
                    // 안 봤다(2026-10-30).
                    //
                    // 방어구의 삽입물도 `_required` 라 이 줄 하나에 같이 걸린다.
                    SlotFill.Auto => deeper || required || isMagazine || (isArmor && isPlate),
                    _ => true,
                };

                chosen = wanted ? auto : null;
            }

            emit(new SlotDecision(path, depth, name, required, kind, pool, auto, chosen, picked));

            if (chosen is not { } tpl)
            {
                continue;
            }

            if (!sharable)
            {
                used.Add(tpl);
            }

            var (hit, chosenTemplate) = itemHelper.GetItem(tpl);
            if (hit && chosenTemplate?.Properties?.ConflictingItems is { } conflicts)
            {
                blocked.UnionWith(conflicts);
            }

            WalkInto(tpl, path, depth + 1, mode, picks, notes, blocked, used, emit);
        }
    }

    /// <summary>
    /// 사용자가 적어 낸 값을 받아들일지 본다. 빈 값은 "비워 둬라" 다 — 자동이
    /// 채우는 것을 막는 유일한 방법이라 반드시 있어야 한다.
    ///
    /// 칸에 못 다는 것을 적었으면 **거절하고 적는다.** 조용히 버리지 않는다.
    /// </summary>
    private static MongoId? Resolve(
        string? raw, IReadOnlyList<MongoId> pool, string path, List<string>? notes)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        MongoId tpl;
        try
        {
            tpl = new MongoId(raw.Trim());
        }
        catch
        {
            notes?.Add($"{path} 에 적은 '{raw}' 는 아이템 id 가 아니다 — 그 칸은 비웠다");
            return null;
        }

        if (!pool.Contains(tpl))
        {
            notes?.Add($"{path} 에 {tpl} 는 못 단다 — 그 칸은 비웠다");
            return null;
        }

        return tpl;
    }

    private static string Join(string prefix, string name) =>
        prefix.Length == 0 ? name : prefix + "." + name;

    /// <summary>
    /// 삽입물 칸인가. SPT 의 <c>IsSoftInsertId</c> 는 받은 글자를 소문자로 내리지
    /// 않는데(<c>IsRemovablePlateSlot</c> 은 내린다) 실제 칸 이름은 `Soft_armor_front`
    /// 처럼 대문자로 시작한다. 그래서 양쪽을 다 대 본다.
    /// </summary>
    private static bool IsSoftInsert(string name)
    {
        var ids = ItemHelper.GetSoftInsertSlotIds();
        return ids.Contains(name) || ids.Contains(name.ToLowerInvariant());
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

    public double PriceOf(MongoId tpl)
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
