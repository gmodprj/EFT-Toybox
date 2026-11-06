using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;

namespace AirdropCaller.Server;

public record ExtractRequest : IRequestData
{
    /// <summary>만들어 줄 프리셋 이름. 비우면 `내 장비`.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public record ExtractResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    /// <summary>뽑은 것. **저장하지 않는다** — 창이 받아서 보여 주고 사람이 이름을 붙여 저장한다.</summary>
    [JsonPropertyName("preset")]
    public Preset? Preset { get; set; }

    /// <summary>뺀 것과 그 이유. 조용히 버리지 않는다(설계 §7.5).</summary>
    [JsonPropertyName("skipped")]
    public List<string> Skipped { get; set; } = [];

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

/// <summary>
/// 지금 로그인한 프로필이 **입고 있는 것**을 프리셋으로 뽑는다 (설계 §18).
///
/// **읽기만 한다.** §7.6 의 "프로필에 쓰지 않는다" 는 쓰기 금지이므로 어긋나지 않는다.
///
/// 계약을 새로 만들지 않았다 — <see cref="ItemSpec"/> 이 이미 셋을 들고 있다:
/// 컨테이너 **안**은 <see cref="ItemSpec.Items"/> 재귀, 칸에 붙은 것은
/// <see cref="ItemSpec.Slots"/>(점으로 잇는 경로), 무기는 그 둘로 완성품을 이긴다
/// (`SlotFiller` 가 "직접 고른 칸이 완성품보다 세다").
///
/// 정해진 것(설계 §18.3):
/// <list type="bullet">
/// <item>**보안 컨테이너는 뺀다** — 죽어도 남는 칸이라 성격이 다르다.</item>
/// <item>**주머니는 넣는다.**</item>
/// <item>**탄은 따로 품목으로** 낸다 — `Slots` 는 경로→tpl 하나뿐이라 색인·수량을 못 담고,
///   `Fill` 은 §15.6 대로 탄창 칸까지만 채운다.</item>
/// </list>
/// </summary>
[Injectable(InjectionType.Singleton)]
public class EquipmentExtractor(
    ISptLogger<EquipmentExtractor> logger,
    SaveServer saveServer,
    ItemHelper itemHelper)
{
    /// <summary>죽어도 남는 칸. 보급 상자로 떨어뜨리는 것과 성격이 다르다(§18.3).</summary>
    private const string SecuredContainer = "SecuredContainer";

    /// <summary>탄이 앉는 칸. 여기 있는 것은 따로 품목으로 낸다(§18.3).</summary>
    private static readonly string[] AmmoSlots = ["cartridges", "patron_in_weapon", "camora_000"];

    public ExtractResponse Extract(string sessionId, string? name)
    {
        var response = new ExtractResponse();
        try
        {
            var pmc = saveServer.GetProfile(new MongoId(sessionId))?.CharacterData?.PmcData;
            var inventory = pmc?.Inventory;
            var items = inventory?.Items;
            if (items is null || inventory?.Equipment is null)
            {
                response.Reason = "프로필의 장비를 읽지 못했다.";
                return response;
            }

            var byParent = new Dictionary<string, List<Item>>(StringComparer.Ordinal);
            foreach (var item in items)
            {
                if (item.ParentId is null)
                {
                    continue;
                }

                if (!byParent.TryGetValue(item.ParentId, out var list))
                {
                    byParent[item.ParentId] = list = [];
                }

                list.Add(item);
            }

            var equipmentRoot = inventory.Equipment.ToString()!;
            var worn = byParent.TryGetValue(equipmentRoot, out var top) ? top : [];

            // 탄은 트리 밖으로 빼서 tpl 별로 합친다 — 상자에 묶음으로 들어간다.
            var ammo = new Dictionary<string, int>(StringComparer.Ordinal);
            var specs = new List<ItemSpec>();

            foreach (var slot in worn.OrderBy(i => i.SlotId, StringComparer.Ordinal))
            {
                if (string.Equals(slot.SlotId, SecuredContainer, StringComparison.Ordinal))
                {
                    var inside = CountSubtree(byParent, slot.Id.ToString()!);
                    response.Skipped.Add($"보안 컨테이너는 뺐다(안에 {inside}개) — 설계 §18.3");
                    continue;
                }

                var spec = Build(slot, byParent, ammo, response.Skipped);
                if (spec is not null)
                {
                    specs.Add(spec);
                }
            }

            foreach (var (tpl, count) in ammo.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                // 탄은 한 줄에 묶는다. 상자 칸을 덜 쓴다.
                specs.Add(new ItemSpec { Tpl = tpl, Count = 1, StackSize = count });
            }

            response.Preset = new Preset
            {
                Id = $"extract-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                Name = string.IsNullOrWhiteSpace(name) ? "내 장비" : name!.Trim(),
                Icon = "Common",
                BigCrate = true,
                Items = Merge(specs),
            };
            response.Ok = true;

            logger.Info(
                $"[AirdropCaller] 장비를 뽑았다: 품목 {response.Preset.Items.Count}줄, "
                + $"탄 {ammo.Count}종, 뺀 것 {response.Skipped.Count}건.");
            return response;
        }
        catch (Exception e)
        {
            logger.Error($"[AirdropCaller] 장비를 뽑지 못했다: {e.Message}");
            response.Reason = "장비를 뽑는 중 오류가 났다. 서버 로그를 본다.";
            return response;
        }
    }

    /// <summary>
    /// 아이템 하나를 <see cref="ItemSpec"/> 으로. 자식은 **칸이냐 격자냐**로 갈린다 —
    /// 그 판정의 권위는 **부모 템플릿의 `Slots`·`Grids`** 다. 이름 모양으로 짐작하지 않는다.
    /// </summary>
    private ItemSpec? Build(
        Item item,
        Dictionary<string, List<Item>> byParent,
        Dictionary<string, int> ammo,
        List<string> skipped)
    {
        var tpl = item.Template.ToString();
        if (string.IsNullOrEmpty(tpl))
        {
            // 조용히 버리지 않는다(설계 §7.5). 템플릿이 비는 것은 있어서는 안 되는 값이다.
            skipped.Add($"템플릿이 비어 있어 뺐다 (칸 {item.SlotId ?? "?"})");
            return null;
        }

        var spec = new ItemSpec { Tpl = tpl, Count = 1 };
        var stack = item.Upd?.StackObjectsCount;
        if (stack is > 1)
        {
            spec.StackSize = (int)stack.Value;
        }

        var children = byParent.TryGetValue(item.Id.ToString()!, out var list) ? list : null;
        if (children is null || children.Count == 0)
        {
            return spec;
        }

        var (ok, template) = itemHelper.GetItem(item.Template);
        var gridNames = ok && template?.Properties?.Grids is { } grids
            ? grids.Where(g => g?.Name is not null).Select(g => g.Name!).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : [];

        var slots = new Dictionary<string, string>(StringComparer.Ordinal);
        var inside = new List<ItemSpec>();

        foreach (var child in children.OrderBy(c => c.SlotId, StringComparer.Ordinal))
        {
            var slotId = child.SlotId ?? string.Empty;

            if (AmmoSlots.Contains(slotId, StringComparer.OrdinalIgnoreCase))
            {
                // 탄은 트리에 담지 못한다(§18.3). 밖으로 빼서 합친다.
                var childTpl = child.Template.ToString();
                if (!string.IsNullOrEmpty(childTpl))
                {
                    ammo[childTpl!] = ammo.GetValueOrDefault(childTpl!) + (int)(child.Upd?.StackObjectsCount ?? 1);
                }

                continue;
            }

            if (gridNames.Contains(slotId))
            {
                // 격자 안 — 컨테이너 내용물이다.
                var nested = Build(child, byParent, ammo, skipped);
                if (nested is not null)
                {
                    inside.Add(nested);
                }

                continue;
            }

            // 칸에 붙은 것. 겹친 칸은 점으로 잇는다.
            CollectSlots(child, slotId, byParent, slots, ammo, skipped);
        }

        if (slots.Count > 0)
        {
            spec.Slots = slots;
        }

        if (inside.Count > 0)
        {
            spec.Items = Merge(inside);
        }

        return spec;
    }

    /// <summary>칸에 붙은 것과 그 아래 칸을 점으로 이어 평평하게 담는다.</summary>
    private void CollectSlots(
        Item item,
        string path,
        Dictionary<string, List<Item>> byParent,
        Dictionary<string, string> slots,
        Dictionary<string, int> ammo,
        List<string> skipped)
    {
        var tpl = item.Template.ToString();
        if (string.IsNullOrEmpty(tpl))
        {
            skipped.Add($"템플릿이 비어 있어 뺐다 (칸 {path})");
            return;
        }

        // 같은 칸 경로가 두 번 오면 **덮지 않고 말한다.** 실측(2026-11-06)에서는 0건이었지만,
        // 사전은 조용히 덮는 자료구조라 그 성질을 여기서 막아 둔다.
        if (slots.TryGetValue(path, out var already))
        {
            skipped.Add($"칸 {path} 이(가) 두 번 나와 뒤엣것을 뺐다 ({already} 를 남겼다)");
            return;
        }

        slots[path] = tpl!;

        if (!byParent.TryGetValue(item.Id.ToString()!, out var children))
        {
            return;
        }

        foreach (var child in children.OrderBy(c => c.SlotId, StringComparer.Ordinal))
        {
            var slotId = child.SlotId ?? string.Empty;
            if (AmmoSlots.Contains(slotId, StringComparer.OrdinalIgnoreCase))
            {
                var childTpl = child.Template.ToString();
                if (!string.IsNullOrEmpty(childTpl))
                {
                    ammo[childTpl!] = ammo.GetValueOrDefault(childTpl!) + (int)(child.Upd?.StackObjectsCount ?? 1);
                }

                continue;
            }

            CollectSlots(child, $"{path}.{slotId}", byParent, slots, ammo, skipped);
        }
    }

    /// <summary>
    /// 같은 것을 한 줄로 묶는다 — tpl·칸·내용물이 모두 같아야 같은 줄이다.
    /// 상자 칸을 덜 쓴다(설계 §18.5).
    /// </summary>
    private static List<ItemSpec> Merge(List<ItemSpec> specs)
    {
        var merged = new List<ItemSpec>();
        var index = new Dictionary<string, ItemSpec>(StringComparer.Ordinal);

        foreach (var spec in specs)
        {
            var key = Key(spec);
            if (index.TryGetValue(key, out var found))
            {
                found.Count += spec.Count;
                continue;
            }

            index[key] = spec;
            merged.Add(spec);
        }

        return merged;
    }

    private static string Key(ItemSpec spec) =>
        spec.Tpl
        + "|" + (spec.StackSize?.ToString() ?? "-")
        + "|" + (spec.Slots is null ? "-" : JsonSerializer.Serialize(spec.Slots.OrderBy(p => p.Key, StringComparer.Ordinal)))
        + "|" + (spec.Items is null ? "-" : JsonSerializer.Serialize(spec.Items.Select(Key)));

    private static int CountSubtree(Dictionary<string, List<Item>> byParent, string id)
    {
        var n = 0;
        var stack = new Stack<string>();
        stack.Push(id);
        while (stack.Count > 0)
        {
            if (!byParent.TryGetValue(stack.Pop(), out var children))
            {
                continue;
            }

            foreach (var child in children)
            {
                n++;
                stack.Push(child.Id.ToString()!);
            }
        }

        return n;
    }
}
