using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Utils.Json;
using System.IO;
using pitTeam.Server.Services;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace pitTeam.Server;

public record PitFireTeamServerMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "xyz.stm.teammanager";
    public string Name { get; init; } = "SPT Team Manager";
    public string Author { get; init; } = "PitAlex";
    public List<string>? Contributors { get; init; }
    public Version Version { get; init; } = new("1.0.0");
    public Range SptVersion { get; init; } = new("~4.1.0");
    public bool HasPrepatcher { get; init; } = false;
    public List<string>? Incompatibilities { get; init; }
    public Dictionary<string, Range>? ModDependencies { get; init; }
    public string? Url { get; init; } = "https://github.com/pitAlex/SPT-PitFireTeam";
    public string License { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.Preload + 1)]
public class PitFireTeamServerPlugin(
    ISptLogger<PitFireTeamServerPlugin> logger,
    TradersTable traders,
    LocaleTable locales,
    FriendlyServerSettingsService settingsService,
    TraderConfig traderConfig,
    ImageRouter imageRouter
) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureCourierTraderRegistered();
        EnsureCourierTraderRefreshTime();
        EnsureCourierTraderLocales();
        EnsureCourierAvatarIsServed();
        settingsService.ApplyPersistedSettings();
        return Task.CompletedTask;
    }

    private void EnsureCourierTraderRegistered()
    {
        try
        {
            if (traders.ContainsKey(FriendlyCourierTraderProfile.CourierTraderId))
            {
                return;
            }

            traders[FriendlyCourierTraderProfile.CourierTraderId] = FriendlyCourierTraderProfile.CreateTrader();
            logger.Info($"Registered courier trader '{FriendlyCourierTraderProfile.CourierTraderIdValue}'");
        }
        catch (Exception ex)
        {
            logger.Warning($"Failed to register courier trader: {ex.Message}");
        }
    }

    // 트레이더를 표에 넣는 것만으로는 모자라다. SPT 의 TraderHelper 는 **trader 설정의 UpdateTime 목록**에서
    // 보충 주기를 찾고, 없으면 기동마다 경고를 내며 기본값 항목을 스스로 끼워 넣는다
    // (`트레이더 … 찾을 수 없음, 기본 새로 고침 시간으로 임시 항목을 생성합니다`).
    // 2026-11-03 서버 로그를 훑다가 그 경고가 **우리 택배원 트레이더** 것임을 알았다 - 10-27 부터 매 기동 났다.
    // 같은 기본값을 우리가 먼저 넣어 두면 경고 없이 같은 결과가 된다.
    private void EnsureCourierTraderRefreshTime()
    {
        try
        {
            var updateTimes = traderConfig.UpdateTime;
            if (updateTimes == null)
            {
                logger.Warning("Courier trader refresh time was left to SPT: trader config had no UpdateTime list.");
                return;
            }

            if (updateTimes.Any(entry => entry?.TraderId == FriendlyCourierTraderProfile.CourierTraderId))
            {
                return;
            }

            int seconds = traderConfig.UpdateTimeDefault;
            updateTimes.Add(new UpdateTime
            {
                Name = FriendlyCourierTraderProfile.CourierNickname,
                TraderId = FriendlyCourierTraderProfile.CourierTraderId,
                Seconds = new MinMax<int>(seconds, seconds),
            });
            logger.Info($"Registered courier trader refresh time ({seconds}s)");
        }
        catch (Exception ex)
        {
            // 보충 주기가 없어도 SPT 가 기본값으로 채운다 - 경고 한 줄이 더 날 뿐이므로 여기서 멈추지 않는다.
            logger.Warning($"Failed to register courier trader refresh time: {ex.Message}");
        }
    }

    private void EnsureCourierTraderLocales()
    {
        try
        {
            string traderId = FriendlyCourierTraderProfile.CourierTraderIdValue;
            foreach (var (locale, lazyGlobal) in locales.Global)
            {
                lazyGlobal.AddTransformer(localized =>
                {
                    if (localized == null)
                    {
                        return localized;
                    }

                    FriendlyCourierTraderProfile.GetLocalizedIdentity(
                        locale,
                        out string nickname,
                        out string location,
                        out string description);

                    localized[$"{traderId} Nickname"] = nickname;
                    localized[$"{traderId} FirstName"] = nickname;
                    localized[$"{traderId} FullName"] = nickname;
                    localized[$"{traderId} Location"] = location;
                    localized[$"{traderId} Description"] = description;
                    return localized;
                });
            }
        }
        catch (Exception ex)
        {
            logger.Warning($"Failed to inject courier trader locale keys: {ex.Message}");
        }
    }

    private void EnsureCourierAvatarIsServed()
    {
        try
        {
            string serverRoot = AppContext.BaseDirectory;
            string sourcePath = Path.Combine(
                serverRoot,
                "user",
                "mods",
                "SptTeamManager-ServerMod",
                "Resources",
                "avatars",
                "courier.png");
            if (!File.Exists(sourcePath))
            {
                logger.Warning($"Courier avatar source missing: {sourcePath}");
                return;
            }

            // SPT matches image URLs without their extension; remote clients cannot use the local cache copy.
            imageRouter.AddRoute(Path.ChangeExtension(FriendlyCourierTraderProfile.CourierAvatarPath, null), sourcePath);

            string targetDirectory = Path.Combine(serverRoot, "user", "sptappdata", "files", "trader", "avatar");
            Directory.CreateDirectory(targetDirectory);

            string targetPath = Path.Combine(targetDirectory, FriendlyCourierTraderProfile.CourierAvatarFileName);
            File.Copy(sourcePath, targetPath, overwrite: true);
        }
        catch (Exception ex)
        {
            logger.Warning($"Failed to publish courier avatar: {ex.Message}");
        }
    }

}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public class PitFireTeamServerPostLoad(
    ISptLogger<PitFireTeamServerPostLoad> logger,
    FriendlyTeammateService teammateService,
    pitTeam.Server.Services.StmProfileMemberStore profileMemberStore,
    pitTeam.Server.Services.StmTransferJournal transferJournal,
    SPTarkov.Server.Core.Servers.SaveServer saveServer,
    SPTarkov.Server.Core.Utils.JsonUtil jsonUtil
) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        teammateService.InitializeStorageForAllProfiles();
        teammateService.RecoverDuplicateTeammateItemsForAllProfiles();
        AssertRequiredWeaponSlotVocabulariesAgree();
        AssertGearBuildRootSlotVocabulariesAgree();
        AssertProfileMemberBindingHolds();
        RecoverOpenTransferJournals();
        logger.Info("PitFireTeam loaded");
        return Task.CompletedTask;
    }

    // 등록 기록(StmProfileMember)은 위치 기반 레코드라 읽기가 생성자 파라미터 바인딩에 기댄다. handEdited 를
    // 뒤에 덧붙였으니, 그것이 없는 옛 문서가 false 로 읽힌다는 가정이 깨지면 사용자의 등록된 프로필 팀원이
    // 로스터에서 조용히 사라진다. 그래서 시작 때 기계가 확인하고, 깨졌으면 Error 를 남기고 등록 목록을
    // "읽을 수 없음" 으로 닫는다(Store 가 이후 읽기를 거부한다). 던지지 않는다.
    private void AssertProfileMemberBindingHolds()
    {
        if (profileMemberStore.VerifyOldDocumentBinding(jsonUtil, out var detail))
        {
            return;
        }

        logger.Error(
            "[STM] 등록된 프로필 팀원을 안정적으로 읽을 수 없다: 옛 모양 등록 문서가 기대대로 바인딩되지 않았다 " +
            $"({detail}). 등록 목록은 읽을 수 없음으로 처리된다.");
    }

    // 끝나지 않은 전송 저널을 세션마다 한 번 처리한다(설계 §6-8). 판정이 애매한 항목은 열어 둔 채 Error 를 남기고
    // 그 팀원 편집을 막는다. 던지지 않는다 - 저널 하나 때문에 모드 전체를 못 올리는 비용이 더 크다.
    private void RecoverOpenTransferJournals()
    {
        try
        {
            foreach (var sessionId in saveServer.GetProfiles().Keys)
            {
                transferJournal.RecoverOpenEntries(sessionId);
            }
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 전송 저널 기동 처리에 실패했다: {ex}");
        }
    }

    // PIT's FriendlyTeammateService.RequiredRaidWeaponSlots and STM's SpawnGearPolicy's own set hold the
    // same three slot names and must keep holding them: PIT decides from its list whether a mercenary is
    // raid-ready, STM decides from its own whether a launcher profile goes in with Real or Temporary
    // gear, and a drift would send one of the two in wearing the wrong thing. The duplication itself is
    // deliberate (the policy must stay unit-testable without the SPT assemblies, which is also why the
    // test project cannot make this comparison), so the agreement is checked here instead of asserted in
    // a comment.
    //
    // Logs rather than throws: a drift is a wrong gear judgment, not a reason to take the whole mod
    // down, and refusing to load would cost the player far more than the mismatch does.
    // PIT 의 TeammateEquipmentPrice.Roots(값을 매기는 자리)와 STM 의 GearBuildPlanPolicy.RootSlotNames
    // (무엇을 살지 정하는 자리)는 같은 열 자리여야 한다. 어긋나면 값을 매기지 않은 것을 사거나, 산 것에
    // 값을 매기지 않는다 - 둘 다 플레이어의 돈이 걸린 어긋남이다. 중복은 의도다(정책은 SPT 어셈블리 없이
    // 테스트돼야 한다), 그래서 일치를 여기서 기계가 지킨다.
    private void AssertGearBuildRootSlotVocabulariesAgree()
    {
        try
        {
            var pit = new HashSet<string>(
                pitTeam.Server.Services.TeammateEquipmentPrice.RootSlotNames,
                StringComparer.OrdinalIgnoreCase);
            var stm = new HashSet<string>(
                pitTeam.Shared.GearBuildPlanPolicy.RootSlotNames,
                StringComparer.OrdinalIgnoreCase);
            if (pit.SetEquals(stm))
            {
                return;
            }

            logger.Error(
                "[STM] 장비 뿌리 칸 어휘가 어긋났다. PIT(TeammateEquipmentPrice.Roots): " +
                $"[{string.Join(", ", pit.Order())}] / STM(GearBuildPlanPolicy): [{string.Join(", ", stm.Order())}]. " +
                "두 목록은 같아야 한다 - 값을 매기는 자리와 사는 자리가 달라지면 플레이어가 틀린 돈을 낸다.");
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 장비 뿌리 칸 어휘를 대조하지 못했다: {ex}");
        }
    }

    private void AssertRequiredWeaponSlotVocabulariesAgree()
    {
        try
        {
            var pit = new HashSet<string>(
                pitTeam.Server.Services.FriendlyTeammateService.RequiredRaidWeaponSlots,
                StringComparer.OrdinalIgnoreCase);
            var stm = new HashSet<string>(
                pitTeam.Shared.SpawnGearPolicy.RequiredWeaponSlotNames,
                StringComparer.OrdinalIgnoreCase);
            if (pit.SetEquals(stm))
            {
                return;
            }

            logger.Error(
                "[STM] 필수 무기 슬롯 어휘가 어긋났다. PIT(FriendlyTeammateService.RequiredRaidWeaponSlots): " +
                $"[{string.Join(", ", pit.Order())}] / STM(SpawnGearPolicy): [{string.Join(", ", stm.Order())}]. " +
                "두 목록은 같아야 한다 - 한쪽만 고치면 용병과 프로필 팀원의 '레이드 준비 완료' 판정이 달라진다.");
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 필수 무기 슬롯 어휘를 대조하지 못했다: {ex}");
        }
    }
}
