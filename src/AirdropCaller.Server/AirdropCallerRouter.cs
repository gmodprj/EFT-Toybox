using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Location;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace AirdropCaller.Server;

/// <summary>
/// 두 가지를 한다.
///
/// **하나 — 바닐라의 보급 전리품 라우트(`POST /client/airdrop/loot`)에 뒤에서 끼어든다.**
/// `HttpRouter.HandleRouteAsync` 는 같은 URL 을 잡는 라우터에서 멈추지 않는다. 전부 차례로
/// 돌리면서 앞선 결과를 다음 라우터에 `output` 으로 넘기고, 마지막 결과가 응답이 된다.
/// 그래서 바닐라보다 **뒤에** 등록되면 그 답을 받아 보고 바꿀 수 있다. 순서는
/// `Injectable.TypePriority` 오름차순인데(`DependencyInjectionHandler`), 바닐라
/// `LocationStaticRouter` 가 400,000 이고 `Injectable` 기본값이 `int.MaxValue` 라
/// 인수 없이 붙이는 것만으로 우리가 뒤에 선다. 우리 id 가 아니면 `output` 을 **그대로**
/// 돌려주므로 바닐라 보급기와 플레어 보급은 건드리지 않는다.
///
/// **둘 — `/airdrop-caller/*` 로 우리 라우트를 연다.** 외부 창이 드롭을 예약하고,
/// 플러그인이 그것을 꺼내 가고 레이드 상태를 보고한다.
/// </summary>
/// <remarks>
/// Singleton 이다. 기본값(Transient)이면 전리품 요청마다 새로 만들어진다.
/// 등록 순서는 수명과 무관하게 `TypePriority` 로 정해지므로 바닐라 뒤에 서는 것은 그대로다.
/// </remarks>
[Injectable(InjectionType.Singleton)]
public class AirdropCallerRouter : StaticRouter
{
    public AirdropCallerRouter(
        JsonUtil jsonUtil,
        ISptLogger<AirdropCallerRouter> logger,
        ConfigStore store,
        HttpResponseUtil httpResponseUtil,
        CrateBuilder crateBuilder,
        DropQueue queue,
        RaidState raidState,
        ItemSearch itemSearch,
        PresetStore presets)
        : base(jsonUtil, BuildRoutes(
            jsonUtil, logger, store, httpResponseUtil, crateBuilder, queue, raidState, itemSearch, presets))
    {
    }

    private static List<RouteAction> BuildRoutes(
        JsonUtil jsonUtil,
        ISptLogger<AirdropCallerRouter> logger,
        ConfigStore store,
        HttpResponseUtil httpResponseUtil,
        CrateBuilder crateBuilder,
        DropQueue queue,
        RaidState raidState,
        ItemSearch itemSearch,
        PresetStore presets)
    {
        string Json<T>(T value) => jsonUtil.Serialize(value) ?? "{}";

        return
        [
            new RouteAction<GetAirdropLootRequest>(
                "/client/airdrop/loot",
                (url, info, sessionId, output, ct) => ValueTask.FromResult(
                    HandleLoot(logger, httpResponseUtil, crateBuilder, store.Config, queue, info, output))),

            new RouteAction(
                "/airdrop-caller/state",
                (url, info, sessionId, output, ct) => ValueTask.FromResult<object>(
                    Json(BuildState(store.Config, queue, raidState)))),

            new RouteAction<QueueRequest>(
                "/airdrop-caller/queue",
                (url, info, sessionId, output, ct) => ValueTask.FromResult(
                    Json(HandleQueue(logger, store.Config, crateBuilder, queue, raidState, presets, sessionId.ToString(), info)))),

            new RouteAction<SearchRequest>(
                "/airdrop-caller/items",
                (url, info, sessionId, output, ct) => ValueTask.FromResult(
                    Json(itemSearch.Search(info.Query, info.Category, info.Limit, info.Tpls)))),

            new RouteAction(
                "/airdrop-caller/categories",
                (url, info, sessionId, output, ct) => ValueTask.FromResult<object>(
                    Json(itemSearch.ListCategories()))),

            new RouteAction(
                "/airdrop-caller/presets",
                (url, info, sessionId, output, ct) => ValueTask.FromResult<object>(
                    Json(new PresetListResponse { Presets = presets.List(sessionId.ToString()) }))),

            new RouteAction<PresetSaveRequest>(
                "/airdrop-caller/presets/save",
                (url, info, sessionId, output, ct) => ValueTask.FromResult(
                    Json(info.Preset is not null && presets.Save(sessionId.ToString(), info.Preset)
                        ? new SimpleResponse { Ok = true }
                        : new SimpleResponse { Ok = false, Reason = "프리셋을 저장하지 못했다. 서버 로그를 본다." }))),

            new RouteAction<PresetIdRequest>(
                "/airdrop-caller/presets/delete",
                (url, info, sessionId, output, ct) => ValueTask.FromResult(
                    Json(info.Id is not null && presets.Delete(sessionId.ToString(), info.Id)
                        ? new SimpleResponse { Ok = true }
                        : new SimpleResponse { Ok = false, Reason = "그런 프리셋이 없다." }))),

            new RouteAction(
                "/airdrop-caller/pending",
                (url, info, sessionId, output, ct) => ValueTask.FromResult<object>(
                    Json(new PendingResponse { DropId = queue.Pop() }))),

            new RouteAction(
                "/airdrop-caller/cancel",
                (url, info, sessionId, output, ct) => ValueTask.FromResult<object>(
                    Json(queue.Cancel()
                        ? new SimpleResponse { Ok = true }
                        : new SimpleResponse { Ok = false, Reason = "취소할 예약이 없다." }))),

            new RouteAction<HeartbeatRequest>(
                "/airdrop-caller/heartbeat",
                (url, info, sessionId, output, ct) =>
                {
                    raidState.Report(info);

                    // 레이드를 나갔으면 예약을 비운다. 다음 레이드에 떨어지면 놀랄 일이다.
                    if (!info.InRaid)
                    {
                        queue.Clear();
                    }

                    return ValueTask.FromResult(Json(new SimpleResponse { Ok = true }));
                }),
        ];
    }

    // ── 바닐라 전리품 라우트 ────────────────────────────────────────────────

    private static string HandleLoot(
        ISptLogger<AirdropCallerRouter> logger,
        HttpResponseUtil httpResponseUtil,
        CrateBuilder crateBuilder,
        Config config,
        DropQueue queue,
        GetAirdropLootRequest info,
        string? vanillaOutput)
    {
        // 우리 것이 아니면 바닐라가 만든 답을 그대로 흘려보낸다.
        var passthrough = vanillaOutput ?? string.Empty;

        if (!config.ModEnabled)
        {
            return passthrough;
        }

        var containerId = info.ContainerId.ToString();
        if (string.IsNullOrEmpty(containerId))
        {
            return passthrough;
        }

        // 1) 외부 창이 예약한 드롭. 조립은 예약받을 때 이미 끝냈다.
        var queued = queue.TakeBuilt(containerId);
        if (queued is not null)
        {
            logger.Info($"[AirdropCaller] {containerId} → 예약된 상자를 보낸다.");
            return httpResponseUtil.GetBody(queued);
        }

        // 2) config.jsonc 에 손으로 적어 둔 상자.
        if (!config.Drops.TryGetValue(containerId, out var spec))
        {
            if (config.Debug)
            {
                logger.Info($"[AirdropCaller] {containerId} 는 우리 것이 아니다 — 바닐라 답을 그대로 보낸다.");
            }

            return passthrough;
        }

        var built = crateBuilder.Build(containerId, spec);
        if (built is null)
        {
            // 조립이 안 되면 상자를 비우느니 바닐라 무작위가 낫다.
            logger.Error($"[AirdropCaller] {containerId} 조립에 실패해 바닐라 답을 그대로 보낸다.");
            return passthrough;
        }

        logger.Info($"[AirdropCaller] {containerId} → {spec.Icon} 상자에 {built.Placed}개를 담았다"
                    + (built.Skipped.Count > 0
                        ? $" (못 담은 것 {built.Skipped.Count}개: {string.Join(", ", built.Skipped)})"
                        : "."));

        return httpResponseUtil.GetBody(built.Response);
    }

    // ── 우리 라우트 ────────────────────────────────────────────────────────

    private static StateResponse BuildState(Config config, DropQueue queue, RaidState raidState)
    {
        var (raid, stale) = raidState.Snapshot();
        var (hasPending, cooldown, _) = queue.Status();

        // 게임 보고가 끊겼으면 레이드도 끝난 것으로 본다. 남은 예약을 여기서 비운다.
        if (stale)
        {
            queue.Clear();
        }

        var state = new StateResponse
        {
            GameConnected = !stale,
            InRaid = !stale && raid.InRaid,
            Map = stale ? null : raid.Map,
            CanSummon = !stale && raid.CanSummon,
            CrateInFlight = !stale && raid.CrateInFlight,
            AvailablePoints = stale ? 0 : raid.AvailablePoints,
            HasPending = hasPending,
            CooldownSeconds = cooldown,
        };

        state.BlockedBy = WhyBlocked(config, state);
        state.CanQueue = state.BlockedBy is null;
        return state;
    }

    /// <summary>
    /// 못 누르는 이유를 문장으로 돌려준다. 아무 일도 안 일어나는데 이유를 모르는 것이
    /// 가장 나쁘다 — 외부 창은 이 문장을 그대로 띄운다.
    /// </summary>
    private static string? WhyBlocked(Config config, StateResponse s)
    {
        if (!config.ModEnabled) return "모드가 설정에서 꺼져 있다.";
        if (!s.GameConnected) return "게임이 안 켜져 있다.";
        if (!s.InRaid) return "레이드 중이 아니다.";
        if (!s.CanSummon) return "이 맵에서는 보급기가 뜨지 않는다.";
        if (s.AvailablePoints <= 0) return "쓸 수 있는 보급 지점이 없다.";
        if (s.CrateInFlight) return "상자가 아직 하늘에 있다.";
        if (s.HasPending) return "이미 예약된 드롭이 있다.";
        if (s.CooldownSeconds > 0) return $"쿨다운 {s.CooldownSeconds}초 남았다.";
        return null;
    }

    private static QueueResponse HandleQueue(
        ISptLogger<AirdropCallerRouter> logger,
        Config config,
        CrateBuilder crateBuilder,
        DropQueue queue,
        RaidState raidState,
        PresetStore presets,
        string profileId,
        QueueRequest request)
    {
        var state = BuildState(config, queue, raidState);
        if (state.BlockedBy is not null)
        {
            return new QueueResponse { Ok = false, Reason = state.BlockedBy };
        }

        var icon = request.Icon;
        var items = request.Items;

        // 프리셋으로 불렀으면 거기서 읽어 온다. 외부 창이 품목을 다시 보낼 필요가 없다.
        if (!string.IsNullOrWhiteSpace(request.PresetId))
        {
            var preset = presets.Get(profileId, request.PresetId!);
            if (preset is null)
            {
                return new QueueResponse { Ok = false, Reason = $"프리셋 '{request.PresetId}' 을 찾지 못했다." };
            }

            icon = preset.Icon;
            items = preset.Items;
        }

        if (items is null || items.Count == 0)
        {
            return new QueueResponse { Ok = false, Reason = "담을 품목이 없다." };
        }

        var spec = new DropSpec
        {
            Icon = string.IsNullOrWhiteSpace(icon) ? "Common" : icon,
            Items = items,
        };

        // 조립을 **지금** 끝낸다. 전리품 요청은 상자가 생기자마자 날아온다(착지 때가 아니다).
        var built = crateBuilder.Build("(예약)", spec);
        if (built is null)
        {
            return new QueueResponse { Ok = false, Reason = $"상자 종류 '{spec.Icon}' 을 모른다." };
        }

        if (built.Placed == 0)
        {
            return new QueueResponse
            {
                Ok = false,
                Reason = "하나도 담기지 않았다 — 품목이 상자 격자에 안 들어간다.",
                Skipped = built.Skipped,
            };
        }

        var cooldown = TimeSpan.FromSeconds(Math.Max(0, config.CooldownSeconds));
        var (result, dropId, cooldownLeft) = queue.Enqueue(built.Response, cooldown);

        return result switch
        {
            DropQueue.QueueResult.Accepted => Accepted(logger, dropId!, spec, built),
            DropQueue.QueueResult.AlreadyQueued =>
                new QueueResponse { Ok = false, Reason = "이미 예약된 드롭이 있다." },
            _ => new QueueResponse { Ok = false, Reason = $"쿨다운 {cooldownLeft}초 남았다." },
        };
    }

    private static QueueResponse Accepted(
        ISptLogger<AirdropCallerRouter> logger, string dropId, DropSpec spec, CrateBuilder.Result built)
    {
        logger.Info($"[AirdropCaller] 드롭 예약 {dropId} → {spec.Icon} 상자에 {built.Placed}개"
                    + (built.Skipped.Count > 0 ? $" (못 담은 것 {built.Skipped.Count}개)" : "."));

        return new QueueResponse
        {
            Ok = true,
            DropId = dropId,
            Placed = built.Placed,
            Skipped = built.Skipped,
        };
    }
}
