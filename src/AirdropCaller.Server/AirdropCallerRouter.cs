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
        PresetStore presets,
        WindowWatch windowWatch,
        SlotCatalog slotCatalog,
        EquipmentExtractor extractor)
        : base(jsonUtil, BuildRoutes(
            jsonUtil, logger, store, httpResponseUtil, crateBuilder, queue, raidState, itemSearch, presets,
            windowWatch, slotCatalog, extractor))
    {
    }

    /// <summary>마지막으로 적은 하늘 상태. 같은 줄을 2초마다 되풀이하지 않는다.</summary>
    private static string? _lastSky;

    private static List<RouteAction> BuildRoutes(
        JsonUtil jsonUtil,
        ISptLogger<AirdropCallerRouter> logger,
        ConfigStore store,
        HttpResponseUtil httpResponseUtil,
        CrateBuilder crateBuilder,
        DropQueue queue,
        RaidState raidState,
        ItemSearch itemSearch,
        PresetStore presets,
        WindowWatch windowWatch,
        SlotCatalog slotCatalog,
        EquipmentExtractor extractor)
    {
        string Json<T>(T value) => jsonUtil.Serialize(value) ?? "{}";

        return
        [
            // **`RouteAction<T>` 를 쓰지 않는다.** 그쪽은 `(T)info` 로 **딱딱하게** 캐스팅한다.
            // 본문이 빈 요청이 오면 `StaticRouter` 가 `EmptyRequestData` 를 넣어 주는데,
            // 그 캐스팅이 거기서 터져 요청 전체가 500 이 된다 — 전리품이 통째로 날아간다.
            //
            //   Unable to cast object of type 'EmptyRequestData'
            //   to type 'GetAirdropLootRequest'.
            //
            // 바닐라 `LocationStaticRouter` 는 같은 자리에서 `info as GetAirdropLootRequest`
            // 로 **무르게** 받는다. 우리도 그래야 한다. `bodyType` 은 그대로 넘겨 주어야
            // 본문이 있을 때 `containerId` 를 읽을 수 있다(없으면 영영 못 읽는다).
            //
            // 플레어를 쏘자 이것으로 세 번 연달아 터졌다(2026-10-30 03:42~03:44).
            new RouteAction(
                "/client/airdrop/loot",
                (url, info, sessionId, output, ct) => ValueTask.FromResult<object>(
                    HandleLoot(logger, httpResponseUtil, crateBuilder, store.Config, queue,
                               info as GetAirdropLootRequest, output)),
                typeof(GetAirdropLootRequest)),

            new RouteAction(
                "/airdrop-caller/state",
                (url, info, sessionId, output, ct) =>
                {
                    // 창이 아직 보고 있다는 표시. 플러그인의 보고 주기가 여기에 달렸다.
                    windowWatch.Note();
                    return ValueTask.FromResult<object>(Json(BuildState(store.Config, queue, raidState)));
                }),

            new RouteAction<QueueRequest>(
                "/airdrop-caller/queue",
                (url, info, sessionId, output, ct) => ValueTask.FromResult(
                    Json(HandleQueue(logger, store.Config, crateBuilder, queue, raidState, presets, sessionId.ToString(), info)))),

            new RouteAction<SearchRequest>(
                "/airdrop-caller/items",
                (url, info, sessionId, output, ct) => ValueTask.FromResult(
                    Json(itemSearch.Search(info.Query, info.Category, info.Limit, info.Tpls)))),

            new RouteAction<SlotQueryRequest>(
                "/airdrop-caller/slots",
                (url, info, sessionId, output, ct) => ValueTask.FromResult(
                    Json(slotCatalog.Describe(info)))),

            new RouteAction(
                "/airdrop-caller/categories",
                (url, info, sessionId, output, ct) => ValueTask.FromResult<object>(
                    Json(itemSearch.ListCategories()))),

            // 지금 입고 있는 것을 프리셋으로 뽑는다(설계 §18). **저장하지 않는다** —
            // 창이 받아서 보여 주고 사람이 이름을 붙여 저장한다.
            new RouteAction<ExtractRequest>(
                "/airdrop-caller/extract",
                (url, info, sessionId, output, ct) => ValueTask.FromResult(
                    Json(extractor.Extract(sessionId.ToString(), info.Name)))),
            new RouteAction(
                "/airdrop-caller/presets",
                (url, info, sessionId, output, ct) => ValueTask.FromResult<object>(
                    Json(new PresetListResponse { Presets = presets.List() }))),

            new RouteAction<PresetSaveRequest>(
                "/airdrop-caller/presets/save",
                (url, info, sessionId, output, ct) => ValueTask.FromResult(
                    Json(info.Preset is not null && presets.Save(info.Preset)
                        ? new SimpleResponse { Ok = true }
                        : new SimpleResponse { Ok = false, Reason = "프리셋을 저장하지 못했다. 서버 로그를 본다." }))),

            new RouteAction<PresetIdRequest>(
                "/airdrop-caller/presets/delete",
                (url, info, sessionId, output, ct) => ValueTask.FromResult(
                    Json(info.Id is not null && presets.Delete(info.Id)
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

                    // 계측: 보급이 겹쳤거나 맵이 자기 것을 띄웠으면 적는다. 낙하산이
                    // 안 사라지는 증상의 첫 용의자가 겹침이다(2026-10-30).
                    // **바뀔 때만 적는다.** 멈춘 상자는 영영 안 사라져서, 매번 적으면
                    // 2초마다 같은 줄이 끝없이 쌓인다(실제로 그랬다).
                    if (info.InRaid && (info.Planes > 0 || info.Crates > 0))
                    {
                        var sky = $"비행기 {info.Planes}, 상자 {info.Crates}, "
                                  + $"맵이 센 보급 {info.MapCount}, 단계 [{info.Stages}]";
                        if (sky != _lastSky)
                        {
                            _lastSky = sky;
                            logger.Info($"[AirdropCaller] 하늘: {sky}");
                        }
                    }
                    else
                    {
                        _lastSky = null;
                    }

                    // 레이드를 나갔으면 예약을 비운다. 다음 레이드에 떨어지면 놀랄 일이다.
                    if (!info.InRaid)
                    {
                        queue.Clear();
                    }

                    // 예약은 **레이드 안에서만** 꺼내 간다. 밖에서 꺼내면 떨어뜨릴 곳이 없다.
                    var dropId = info.InRaid ? queue.Pop() : null;

                    return ValueTask.FromResult(Json(new HeartbeatResponse
                    {
                        DropId = dropId,
                        NextSeconds = NextHeartbeatSeconds(store.Config, queue, windowWatch, dropId),
                    }));
                }),
        ];
    }

    /// <summary>로그 한 줄에 덧붙일 말. 못 담은 것과 고친 것을 따로 적는다.</summary>
    private static string Note(CrateBuilder.Result built)
    {
        var parts = new List<string>();
        if (built.Skipped.Count > 0)
        {
            parts.Add($"못 담은 것: {string.Join(", ", built.Skipped)}");
        }

        if (built.Adjusted.Count > 0)
        {
            parts.Add($"고친 것: {string.Join(", ", built.Adjusted)}");
        }

        return parts.Count == 0 ? "." : $" ({string.Join(" / ", parts)})";
    }

    /// <summary>
    /// 플러그인이 다음 보고까지 쉴 시간.
    ///
    /// 빠르게 받아야 하는 때는 **둘뿐**이다 — 창이 열려 상태를 보고 있을 때(곧
    /// 떨어뜨리기를 누를 수 있다)와, 이미 예약이 걸려 있을 때. 그 밖에는 아무도
    /// 안 읽는 보고라 천천히 받는다.
    ///
    /// 창은 상태를 2초마다 물어보므로, 창을 켜면 늦어도 한 번의 느린 주기 안에
    /// 다시 빨라진다.
    /// </summary>
    private static double NextHeartbeatSeconds(
        Config config, DropQueue queue, WindowWatch windowWatch, string? justPopped)
    {
        if (justPopped is not null || windowWatch.Watching)
        {
            return config.FastHeartbeatSeconds;
        }

        var (hasPending, _, _) = queue.Status();
        return hasPending ? config.FastHeartbeatSeconds : config.IdleHeartbeatSeconds;
    }

    // ── 바닐라 전리품 라우트 ────────────────────────────────────────────────

    private static string HandleLoot(
        ISptLogger<AirdropCallerRouter> logger,
        HttpResponseUtil httpResponseUtil,
        CrateBuilder crateBuilder,
        Config config,
        DropQueue queue,
        GetAirdropLootRequest? info,
        string? vanillaOutput)
    {
        // 우리 것이 아니면 바닐라가 만든 답을 그대로 흘려보낸다.
        var passthrough = vanillaOutput ?? string.Empty;

        // 본문이 비어 있으면 `StaticRouter` 가 `EmptyRequestData` 를 준다 — 여기서는
        // null 로 온다. 어느 상자인지 알 길이 없으니 바닐라 답을 그대로 보낸다.
        // **터뜨리지 않는다**: 여기서 터지면 요청이 500 이 되어 전리품이 통째로 날아간다.
        if (info is null)
        {
            if (config.Debug)
            {
                logger.Info("[AirdropCaller] 전리품 요청에 상자 id 가 없다 — 바닐라 답을 그대로 보낸다.");
            }

            return passthrough;
        }

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
                    + Note(built));

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

        state.BlockedBy = QueueGate.WhyBlocked(GateState(config, state));
        state.CanQueue = state.BlockedBy is null;
        return state;
    }

    /// <summary>
    /// 빗장 판정에 넘기는 값만 추린다 — 판정 자체는 <see cref="QueueGate"/> 에 있고
    /// 거기만 시험이 본다.
    /// </summary>
    private static QueueGateState GateState(Config config, StateResponse s)
    {
        return new QueueGateState
        {
            ModEnabled = config.ModEnabled,
            GameConnected = s.GameConnected,
            InRaid = s.InRaid,
            CanSummon = s.CanSummon,
            AvailablePoints = s.AvailablePoints,
            CrateInFlight = s.CrateInFlight,
            HasPending = s.HasPending,
            CooldownSeconds = s.CooldownSeconds,
        };
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

        // 마른 조립은 예약을 지키려고 있는 빗장을 건너뛴다 — 예약을 하지 않으니 지킬 것이 없다.
        var blocked = QueueGate.WhyBlocked(GateState(config, state), request.DryRun);
        if (blocked is not null)
        {
            return new QueueResponse { Ok = false, Reason = blocked, DryRun = request.DryRun };
        }

        var icon = request.Icon;
        var items = request.Items;
        var bigCrate = request.BigCrate;

        // 프리셋으로 불렀으면 거기서 읽어 온다. 외부 창이 품목을 다시 보낼 필요가 없다.
        if (!string.IsNullOrWhiteSpace(request.PresetId))
        {
            var preset = presets.Get(request.PresetId!);
            if (preset is null)
            {
                return new QueueResponse { Ok = false, Reason = $"프리셋 '{request.PresetId}' 을 찾지 못했다." };
            }

            icon = preset.Icon;
            items = preset.Items;
            bigCrate = preset.BigCrate;
        }

        if (items is null || items.Count == 0)
        {
            return new QueueResponse { Ok = false, Reason = "담을 품목이 없다.", DryRun = request.DryRun };
        }

        var spec = new DropSpec
        {
            Icon = string.IsNullOrWhiteSpace(icon) ? "Common" : icon,
            Items = items,
            BigCrate = bigCrate,
        };

        // 조립을 **지금** 끝낸다. 전리품 요청은 상자가 생기자마자 날아온다(착지 때가 아니다).
        var built = crateBuilder.Build(request.DryRun ? "(마른 조립)" : "(예약)", spec);
        if (built is null)
        {
            return new QueueResponse
            {
                Ok = false,
                Reason = $"상자 종류 '{spec.Icon}' 을 모른다.",
                DryRun = request.DryRun,
            };
        }

        if (built.Placed == 0)
        {
            return new QueueResponse
            {
                Ok = false,
                Reason = "하나도 담기지 않았다 — 품목이 상자 격자에 안 들어간다.",
                Skipped = built.Skipped,
                DryRun = request.DryRun,
            };
        }

        // 여기서 멈춘다 — 조립 결과만 돌려주고 큐에는 넣지 않는다. 쿨다운도 건드리지 않는다.
        if (request.DryRun)
        {
            return DryRunDone(logger, spec, built);
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

    /// <summary>
    /// 마른 조립의 답. <c>DropId</c> 를 **주지 않는다** — 플러그인이 꺼내 갈 것이 없다는 뜻이고,
    /// 창은 <c>dryRun</c> 을 보고 "떨어지지 않는다" 고 말한다.
    /// </summary>
    private static QueueResponse DryRunDone(
        ISptLogger<AirdropCallerRouter> logger, DropSpec spec, CrateBuilder.Result built)
    {
        var attached = built.Attached > 0 ? $" (부속 {built.Attached}개 붙임)" : string.Empty;
        var size = spec.BigCrate ? " 큰" : string.Empty;
        logger.Info($"[AirdropCaller] 마른 조립 → {spec.Icon}{size} 상자에 {built.Placed}개{attached}"
                    + " — 예약하지 않았다" + Note(built));

        return new QueueResponse
        {
            Ok = true,
            DryRun = true,
            DropId = null,
            Placed = built.Placed,
            Attached = built.Attached,
            Skipped = built.Skipped,
            Adjusted = built.Adjusted,
        };
    }

    private static QueueResponse Accepted(
        ISptLogger<AirdropCallerRouter> logger, string dropId, DropSpec spec, CrateBuilder.Result built)
    {
        var attached = built.Attached > 0 ? $" (부속 {built.Attached}개 붙임)" : string.Empty;
        var size = spec.BigCrate ? " 큰" : string.Empty;
        logger.Info($"[AirdropCaller] 드롭 예약 {dropId} → {spec.Icon}{size} 상자에 {built.Placed}개{attached}"
                    + Note(built));

        return new QueueResponse
        {
            Ok = true,
            DropId = dropId,
            Placed = built.Placed,
            Attached = built.Attached,
            Skipped = built.Skipped,
            Adjusted = built.Adjusted,
        };
    }
}
