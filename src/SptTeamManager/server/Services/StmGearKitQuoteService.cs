using System;
using System.Collections.Generic;
using System.Linq;
using pitTeam.Server.Models;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services.Locales;

namespace pitTeam.Server.Services;

// 계획 4 Task 2 — 고른 장비 빌드를 팀원에게 입히는 데 드는 돈을 미리 센다. **읽기 전용이다**:
// 돈도 물건도 건드리지 않는다. 쓰는 것은 Task 3 이고, 그때 전송 저널이 처음 쓰인다.
//
// 판정은 전부 순수 정책 둘에 있다: 무엇을 사고 무엇을 벗길지(GearBuildPlanPolicy), 얼마인지와
// 못 매긴 것이 있는지(GearBuildQuotePolicy). 이 서비스가 하는 일은 SPT 에서 사실을 모아 넘기고
// 결과를 전선 모양으로 옮기는 것뿐이다.
//
// 값은 **PIT 의 고용가 표를 그대로 쓴다**(FriendlyTeammateService.CreateHiringUnitPriceLookup).
// 같은 물건에 용병 고용 때와 꾸러미 때 다른 값이 붙으면 그것이 곧 버그 신고가 된다.
[Injectable(InjectionType.Singleton)]
public class StmGearKitQuoteService(
    StmProfileMemberResolver resolver,
    FriendlyTeammateService teammateService,
    SaveServer saveServer,
    ItemHelper itemHelper,
    LocaleService localeService,
    ISptLogger<StmGearKitQuoteService> logger)
{
    internal const string OwnerTeammate = "teammate";
    internal const string OwnerPlayer = "player";

    private static string DescribeLookupFailure(ProfileLookupOutcome outcome, string? unavailableReason) => outcome switch
    {
        ProfileLookupOutcome.ReadFailed => "이 팀원이 프로필 팀원인지 확인할 수 없어 값을 매기지 않습니다. 잠시 뒤 다시 시도하세요.",
        ProfileLookupOutcome.NonMember => "이 팀원은 프로필 팀원이 아니어서 이 경로로 꾸러미를 살 수 없습니다.",
        _ => $"이 프로필을 지금 쓸 수 없어 값을 매기지 않습니다({unavailableReason ?? "Unavailable"}).",
    };

    public bool TryQuote(MongoId sessionId, StmGearKitQuoteRequest request, out StmGearKitQuoteResponse? quote, out string? refusalReason)
    {
        quote = null;
        refusalReason = null;
        try
        {
            if (request == null)
            {
                refusalReason = "요청이 비어 값을 매기지 않습니다.";
                return false;
            }

            var owner = request.Owner?.Trim().ToLowerInvariant();
            if (owner != OwnerTeammate && owner != OwnerPlayer)
            {
                // 모르는 값을 추측해서 엉뚱한 프로필의 빌드를 입히지 않는다.
                logger.Warning($"[STM] 꾸러미 견적 거부: 모르는 빌드 소유자 '{request.Owner}'.");
                refusalReason = "어느 쪽 빌드인지 알 수 없어 값을 매기지 않습니다.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.BuildId))
            {
                refusalReason = "고른 빌드가 없어 값을 매기지 않습니다.";
                return false;
            }

            // 해석기를 먼저 묻는다(불변식 3).
            var outcome = resolver.Lookup(sessionId, request.Aid, out var member, out var unavailableReason);
            if (outcome != ProfileLookupOutcome.Resolved || member == null)
            {
                refusalReason = DescribeLookupFailure(outcome, unavailableReason);
                return false;
            }

            if (!string.Equals(member.ProfileId, request.ProfileId?.Trim(), StringComparison.Ordinal))
            {
                logger.Error($"[STM] 꾸러미 견적 거부: 요청의 프로필 '{request.ProfileId}' 과(와) Aid '{request.Aid}' 로 해석된 '{member.ProfileId}' 이(가) 다르다.");
                refusalReason = "요청한 프로필과 이 팀원의 프로필이 달라 값을 매기지 않습니다.";
                return false;
            }

            var ownerProfileId = owner == OwnerTeammate ? member.ProfileId : sessionId.ToString();
            var build = FindBuild(ownerProfileId, request.BuildId!.Trim());
            if (build == null)
            {
                refusalReason = "고른 빌드를 찾지 못했습니다. 목록을 다시 열어 주세요.";
                return false;
            }

            // 빌드와 팀원의 지금 장비를 견준다. 둘 다 읽기만 한다 - member.Pmc 는 살아 있는 객체다(불변식 1).
            var plan = GearBuildPlanPolicy.Plan(
                build.Items?.Select(ToRow),
                build.Root.ToString(),
                member.Pmc?.Inventory?.Items?.Select(ToRow),
                member.Pmc?.Inventory?.Equipment.ToString(),
                AttachmentSlotsOf);

            var unitPrice = teammateService.CreateHiringUnitPriceLookup();
            if (!GearBuildQuotePolicy.TryQuote(plan.AllItemsToBuy, tpl => unitPrice(PriceProbe(tpl)),
                    out var priced, out var unpriceable) || priced == null)
            {
                var tpls = string.Join(", ", unpriceable.Take(5));
                var shown = unpriceable.Take(3).Select(DescribeItem).ToList();
                var andMore = unpriceable.Count > shown.Count ? $" 외 {unpriceable.Count - shown.Count}개" : string.Empty;
                logger.Warning($"[STM] 꾸러미 견적 거부: 값을 매길 수 없는 물건 {unpriceable.Count}개 ({string.Join(" / ", shown)}{andMore}, tpl 앞 5: {tpls}).");
                refusalReason = $"{string.Join(", ", shown)}{andMore} 의 값을 매길 수 없어 이 빌드는 살 수 없습니다. 상인이 팔지 않거나 플리마켓에서 막힌 물건입니다.";
                return false;
            }

            var playerRoubles = PlayerRoubles(sessionId);
            quote = new StmGearKitQuoteResponse
            {
                BuildId = build.Id.ToString(),
                BuildName = string.IsNullOrWhiteSpace(build.Name) ? GearBuildListPolicy.UnnamedBuild : build.Name!,
                Owner = owner!,
                Total = priced.Total,
                PlayerRoubles = playerRoubles,
                Affordable = playerRoubles >= priced.Total,
                ItemsToBuy = plan.AllItemsToBuy.Count,
                ItemsToStash = plan.AllItemsToStash.Count,
                Slots = plan.Slots.Select(s => new StmGearKitSlotRow
                {
                    SlotId = s.SlotId,
                    Outcome = s.Outcome.ToString(),
                    BuyCount = s.ItemsToBuy.Count,
                    StashCount = s.ItemsToStash.Count,
                }).ToList(),
            };

            logger.Info($"[STM] 꾸러미 견적: 팀원 '{member.ProfileId}' 빌드 '{quote.BuildId}'({owner}) {quote.Total}₽, 살 것 {quote.ItemsToBuy}개, 스태시로 {quote.ItemsToStash}개, 가진 돈 {playerRoubles}₽.");
            return true;
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 꾸러미 견적 실패 (Aid '{request?.Aid}', 세션 '{sessionId}'): {ex}");
            refusalReason = "값을 매기지 못했습니다. 서버 로그를 확인하세요.";
            return false;
        }
    }

    // PIT 의 값 표는 Item 을 받지만 보는 것은 Template 뿐이다(CreateHiringUnitPriceLookup). 값을 묻기 위한
    // 껍데기 하나를 만들어 넘긴다 - 이 물건은 어디에도 들어가지 않고 값을 묻는 동안만 산다.
    private static Item PriceProbe(string tpl) => new() { Id = new MongoId(), Template = new MongoId(tpl) };

    private static GearItemRow ToRow(Item item) =>
        new(item.Id.ToString(), item.Template.ToString(), item.ParentId?.ToString(), item.SlotId,
            // 묶음 수량. 없으면 한 점이다 - 탄 묶음만 이 값이 1 보다 크다.
            (int?)item.Upd?.StackObjectsCount ?? 1);

    // 부착물 칸 이름. PIT 의 고용가 계산이 쓰는 것과 같은 읽기다 - 격자(가방 안 짐)는 칸 이름이 다르므로
    // 자연히 빠진다.
    private IEnumerable<string> AttachmentSlotsOf(string tpl)
    {
        try
        {
            // 값과 구매는 **같은 목록**을 봐야 한다 - 어휘가 갈리면 값을 매기지 않은 것을 사거나
            // 산 것에 값을 매기지 않는다. 그래서 구매 쪽의 어휘를 그대로 부른다(집은 하나다).
            return StmGearKitPurchaseService.ChildSlotNamesOf(itemHelper.GetItem(new MongoId(tpl)).Value);
        }
        catch
        {
            // 모르는 템플릿이면 부착물이 없는 것으로 본다. 없는 하위를 상상하지 않는다.
            return [];
        }
    }

    private EquipmentBuild? FindBuild(string profileId, string buildId)
    {
        try
        {
            var builds = saveServer.GetProfile(new MongoId(profileId))?.UserBuildData?.EquipmentBuilds;
            return builds?.FirstOrDefault(b => b != null && string.Equals(b.Id.ToString(), buildId, StringComparison.Ordinal));
        }
        catch (Exception ex)
        {
            logger.Warning($"[STM] 꾸러미 견적: 프로필 '{profileId}' 의 빌드를 읽지 못했다: {ex.Message}");
            return null;
        }
    }

    // tpl 을 사람이 읽을 수 있는 이름으로. 거절 문구에만 쓴다 - 판정에는 쓰지 않으므로
    // 읽지 못하면 tpl 을 그대로 보여 주고 넘어간다(이름을 못 읽었다고 거절을 못 내면 더 나쁘다).
    private string DescribeItem(string tpl)
    {
        try
        {
            var locale = localeService.GetLocaleDb();
            if (locale != null
                && locale.TryGetValue($"{tpl} Name", out var localized)
                && !string.IsNullOrWhiteSpace(localized))
            {
                return localized.Trim();
            }

            var (found, template) = itemHelper.GetItem(tpl);
            var fallback = found ? template?.Properties?.ShortName ?? template?.Name : null;
            return string.IsNullOrWhiteSpace(fallback) ? tpl : fallback!.Trim();
        }
        catch (Exception ex)
        {
            logger.Warning($"[STM] 물건 이름을 읽지 못해 tpl 을 그대로 쓴다 ({tpl}): {ex.Message}");
            return tpl;
        }
    }

    // 플레이어가 가진 루블. 기준은 PIT 의 결제가 쓰는 것과 같다(IsPaymentMoney).
    // **안내용이다**: 실제 청구는 Task 3 이 PIT 의 차감 경로로 하고, 그때 다시 센다.
    private int PlayerRoubles(MongoId sessionId)
    {
        try
        {
            var items = saveServer.GetProfile(sessionId)?.CharacterData?.PmcData?.Inventory?.Items;
            if (items == null) return 0;

            double total = items.Where(FriendlyTeammateService.IsPaymentMoney)
                .Sum(item => (double)(item.Upd?.StackObjectsCount ?? 1));
            return total > int.MaxValue ? int.MaxValue : (int)total;
        }
        catch (Exception ex)
        {
            logger.Warning($"[STM] 꾸러미 견적: 플레이어의 돈을 세지 못해 0 으로 둔다: {ex.Message}");
            return 0;
        }
    }
}
