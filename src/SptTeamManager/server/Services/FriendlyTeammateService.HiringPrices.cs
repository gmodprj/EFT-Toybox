using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;

namespace pitTeam.Server.Services;

public partial class FriendlyTeammateService
{
    private int CalculateRecruitmentGearPrice(BotBase teammate) => TeammateEquipmentPrice.Calculate(
        teammate.Inventory!.Items!, GetEquipmentRootId(teammate),
        item => itemHelper.GetItem(item.Template).Value?.Properties?.Slots?
            .Where(slot => !string.IsNullOrEmpty(slot.Name)).Select(slot => slot.Name!) ?? [],
        item => itemHelper.IsOfBaseclass(item.Template, BaseClasses.AMMO)
            || itemHelper.IsOfBaseclass(item.Template, BaseClasses.BUILT_IN_INSERTS),
        item => itemHelper.IsOfBaseclass(item.Template, BaseClasses.ARMOR_PLATE),
        CreateHiringUnitPriceLookup());

    // STM 의 장비 꾸러미 견적이 같은 값을 쓴다(계획 4 Task 2). 값을 매기는 집은 하나여야 한다 -
    // 용병 고용가와 프로필 팀원 꾸러미 값이 다른 표를 보면 같은 물건에 다른 값이 붙는다.
    // PIT 파일에는 private -> internal 승격만 한다(인계 §3-4).
    internal Func<Item, double> CreateHiringUnitPriceLookup()
    {
        var config = ragfairConfig.Dynamic.GenerateBaseFleaPrices;
        var craftItems = (hideoutTable.Production.Recipes ?? [])
            .SelectMany(recipe => recipe.Requirements ?? [])
            .Where(requirement => requirement.Type == "Item")
            .Select(requirement => requirement.TemplateId).ToHashSet();
        var prices = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        return item =>
        {
            string key = item.Template.ToString();
            if (prices.TryGetValue(key, out double cached)) return cached;
            double handbook = itemHelper.GetStaticItemPrice(item.Template);
            double price;
            if (config.UseHandbookPrice && handbook > 0)
            {
                // SPT 4.1's ReplaceFleaBasePrices adds its generated price to the existing
                // table. Recompute the intended value once, without mutating the global economy.
                double multiplier = config.PriceMultiplier;
                if (config.ItemTplMultiplierOverride.TryGetValue(item.Template, out double specific)) multiplier = specific;
                else
                {
                    foreach (var entry in config.ItemTypeMultiplierOverride)
                    {
                        if (!itemHelper.IsOfBaseclass(item.Template, entry.Key)) continue;
                        multiplier = entry.Value;
                        break;
                    }
                }
                if (config.UseHideoutCraftMultiplier && craftItems.Contains(item.Template))
                    multiplier += config.HideoutCraftMultiplier;
                price = handbook * multiplier;
                if (config.PreventPriceBeingBelowTraderBuyPrice)
                    price = Math.Max(price, traderHelper.GetHighestSellToTraderPrice(item.Template));
            }
            else
            {
                // With handbook generation disabled (or unavailable for a modded template),
                // the configured flea table remains the source, with handbook fallback.
                price = itemHelper.GetDynamicItemPrice(item.Template) is > 0 and var market ? market : handbook;
            }
            // Calculate validates non-finite, missing and non-positive prices before charging.
            prices[key] = price;
            return price;
        };
    }
}
