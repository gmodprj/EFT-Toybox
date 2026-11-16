using System.Reflection;
using HarmonyLib;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Eft.Prestige;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace PrestigeTransferKeep;

/// <summary>
/// 프레스티지에서 **넘기기로 고른 물건을 그대로 받게** 되돌린다.
///
/// 왜 — 2026-11-16 실측. 3332 Prestige Perks 를 넣은 뒤 전송 창고에 담은 것이 우편으로 오지
/// 않았다. 원인은 3332 가 설계상 그렇게 동작하기 때문이다. 그 모드는
/// <c>PrestigeHelper.ProcessPendingPrestige</c> 에 Harmony Prefix 를 걸고 그 안에서
///
/// <code>
/// prestige.Items = new ObtainPrestigeRequestList();   // 전송 목록을 비운다
/// logger.Info("... vanilla transfer mail suppressed");
/// </code>
///
/// 로 목록을 비운 뒤, 담았던 것을 **프레스티지 코인으로 환산**해 준다. Nomad 의 경우 가방 하나에
/// 든 2,001개(122억 루블)가 코인 1,039개가 되고 121,075개어치가 상한에 걸려 버려졌다.
///
/// 설정으로 끌 수 있는 항목은 없다. 그래서 밖에서 되돌린다.
///
/// 하는 일은 둘이고 **둘 다 끄고 켤 수 있다.**
/// 1. 같은 메서드에 Prefix 를 **두 개** 건다. 하나는 <see cref="HarmonyLib.Priority.First"/> 로
///    3332 보다 **먼저** 돌며 목록을 떠 두고, 하나는 <see cref="HarmonyLib.Priority.Last"/> 로
///    3332 **뒤에** 돌며 도로 끼운다. 그러면 SPT 본체는 원래 목록을 보고 우편을 보낸다.
/// 2. 전송분 코인 우편(<c>PrestigeCoinService.Deliver</c>)을 막는다 — 물건을 받으면서 코인까지
///    받으면 두 번 받는 셈이다.
///
/// **3332 의 어셈블리를 참조하지 않는다.** 타입을 이름으로 찾고, 막는 쪽은 인자가 없는 Prefix 라
/// 3332 의 타입을 쓸 일이 없다. 3332 가 없거나 판이 바뀌면 그 패치만 조용히 건너뛰고 로그에 적는다.
/// </summary>
[Injectable(InjectionType.Singleton, int.MaxValue)]
public class PrestigeTransferKeep(
    ISptLogger<PrestigeTransferKeep> logger,
    ModHelper modHelper
) : IOnLoad
{
    /// <summary>3332 의 코인 전달 메서드. 이름으로만 찾는다.</summary>
    public const string CoinServiceTypeName = "PrestigePerks.Items.PrestigeCoinService";

    /// <summary>같은 메서드를 가리키는 이름. Harmony 소유자 id 로도 쓴다.</summary>
    public const string HarmonyId = "spt.prestigetransferkeep";

    internal static readonly PendingItemsStore Store = new();

    internal static ISptLogger<PrestigeTransferKeep>? Log;
    internal static bool Verbose;

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        var modFolder = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var config = modHelper.GetJsonDataFromFile<Config>(modFolder, "config.jsonc");

        Log = logger;
        Verbose = config.Debug;

        if (!config.ModEnabled)
        {
            logger.Info("[PrestigeTransferKeep] 꺼져 있어 아무것도 걸지 않았다.");
            return Task.CompletedTask;
        }

        var harmony = new Harmony(HarmonyId);

        if (config.KeepTransferItems)
        {
            PatchKeepItems(harmony);
        }

        if (config.SuppressCoinMail)
        {
            PatchSuppressCoins(harmony);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 3332 를 **사이에 두고** 앞뒤로 한 번씩 끼어든다. 우선순위가 큰 쪽이 먼저 돈다
    /// (<c>First</c> 800 · 기본 <c>Normal</c> 400 · <c>Last</c> 0).
    /// </summary>
    private void PatchKeepItems(Harmony harmony)
    {
        var target = AccessTools.Method(typeof(PrestigeHelper), nameof(PrestigeHelper.ProcessPendingPrestige));
        if (target is null)
        {
            logger.Error("[PrestigeTransferKeep] ProcessPendingPrestige 를 못 찾았다. 넘길 물건을 지키지 못한다.");
            return;
        }

        harmony.Patch(target, prefix: new HarmonyMethod(typeof(TransferGuard), nameof(TransferGuard.SaveBefore))
        {
            priority = Priority.First,
        });

        harmony.Patch(target, prefix: new HarmonyMethod(typeof(TransferGuard), nameof(TransferGuard.RestoreAfter))
        {
            priority = Priority.Last,
        });

        logger.Info("[PrestigeTransferKeep] 넘길 물건을 지킨다 — ProcessPendingPrestige 앞뒤로 걸었다.");
    }

    /// <summary>
    /// 전송분 코인 우편을 막는다. 3332 가 없으면 **조용히 지나가지 않고** 적어 둔다.
    /// 인자가 없는 Prefix 라 3332 의 타입을 쓰지 않는다.
    /// </summary>
    private void PatchSuppressCoins(Harmony harmony)
    {
        var type = AccessTools.TypeByName(CoinServiceTypeName);
        if (type is null)
        {
            logger.Info("[PrestigeTransferKeep] 3332 Prestige Perks 가 없다 — 코인 우편 막기는 건너뛴다.");
            return;
        }

        var deliver = AccessTools.Method(type, "Deliver");
        if (deliver is null)
        {
            logger.Error("[PrestigeTransferKeep] PrestigeCoinService.Deliver 를 못 찾았다. 코인 우편은 그대로 온다.");
            return;
        }

        harmony.Patch(deliver, prefix: new HarmonyMethod(typeof(TransferGuard), nameof(TransferGuard.SkipCoinMail)));
        logger.Info("[PrestigeTransferKeep] 전송분 코인 우편을 막는다 — 보스·금고 코인은 그대로다.");
    }
}

/// <summary>Harmony 가 부르는 자리들. 상태는 <see cref="PrestigeTransferKeep.Store"/> 하나뿐이다.</summary>
internal static class TransferGuard
{
    /// <summary>3332 보다 먼저 돌아 넘길 목록을 떠 둔다.</summary>
    internal static void SaveBefore(PendingPrestige prestige)
    {
        if (prestige?.Items is null)
        {
            return;
        }

        PrestigeTransferKeep.Store.Save(prestige, prestige.Items);

        if (PrestigeTransferKeep.Verbose)
        {
            PrestigeTransferKeep.Log?.Info(
                $"[PrestigeTransferKeep] 넘길 목록 {prestige.Items.Count}건을 떠 뒀다.");
        }
    }

    /// <summary>3332 뒤에 돌아 목록이 비워졌으면 도로 끼운다.</summary>
    internal static void RestoreAfter(PendingPrestige prestige)
    {
        if (prestige is null)
        {
            return;
        }

        var saved = PrestigeTransferKeep.Store.TakeBack(prestige);
        if (saved is not ObtainPrestigeRequestList list)
        {
            return;
        }

        var now = prestige.Items?.Count ?? 0;
        if (now >= list.Count)
        {
            // 아무도 건드리지 않았다. 그대로 둔다.
            return;
        }

        prestige.Items = list;

        if (PrestigeTransferKeep.Verbose)
        {
            PrestigeTransferKeep.Log?.Info(
                $"[PrestigeTransferKeep] 비워진 목록({now}건)을 원래 {list.Count}건으로 되돌렸다.");
        }
    }

    /// <summary>코인 전달을 건너뛴다. 인자를 받지 않아 3332 의 타입을 쓰지 않는다.</summary>
    internal static bool SkipCoinMail()
    {
        if (PrestigeTransferKeep.Verbose)
        {
            PrestigeTransferKeep.Log?.Info("[PrestigeTransferKeep] 전송분 코인 우편을 건너뛰었다.");
        }

        return false;
    }
}
