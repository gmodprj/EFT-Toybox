using System;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Airdrop;
using EFT.SynchronizableObjects;
using Newtonsoft.Json;
using SPT.Common.Http;
using UnityEngine;

namespace AirdropCaller.Plugin
{
    /// <summary>
    /// 보급 호출기의 게임 쪽 조각.
    ///
    /// 두 가지를 한다.
    /// 1. **보고** — 레이드 상태를 서버에 2초마다 올린다. 외부 창이 버튼을 켤지 끌지를
    ///    이것으로 정한다.
    /// 2. **실행** — 서버에 예약된 드롭이 있으면 꺼내 와 `ClientGameWorld.InitAirdrop` 을
    ///    부른다. 공개 메서드 하나다 — Harmony 패치도 리플렉션도 쓰지 않는다.
    ///
    /// 근거: doc/specs/2026-10-29-airdrop-stage1-findings.md
    /// </summary>
    [BepInPlugin(PluginGuid, "보급 호출기", "0.1.0")]
    public class AirdropCallerPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "spt.airdropcaller";

        private const string PendingUrl = "/airdrop-caller/pending";
        private const string HeartbeatUrl = "/airdrop-caller/heartbeat";

        private static ManualLogSource _log;

        private ConfigEntry<bool> _enabled;
        private ConfigEntry<float> _pollSeconds;
        private ConfigEntry<float> _heartbeatSeconds;
        private ConfigEntry<bool> _verbose;

        private float _nextPollAt;
        private float _nextHeartbeatAt;

        /// <summary>
        /// 서버 호출은 **기다리지 않는다.** `RequestHandler` 의 동기 메서드를 쓰면 응답이
        /// 올 때까지 프레임이 멈춘다. 보낸 일감을 들고 있다가 끝났을 때만 읽는다.
        /// </summary>
        private Task<string> _pendingCall;
        private Task<string> _heartbeatCall;

        private void Awake()
        {
            _log = Logger;

            _enabled = Config.Bind("일반", "켜기", true,
                "끄면 보고도 폴링도 하지 않는다. 바닐라 보급은 그대로다.");

            _pollSeconds = Config.Bind("일반", "예약 확인 주기(초)", 1f,
                new ConfigDescription(
                    "레이드 중에만 이 주기로 서버에 예약된 드롭이 있는지 묻는다.",
                    new AcceptableValueRange<float>(0.5f, 10f)));

            _heartbeatSeconds = Config.Bind("일반", "상태 보고 주기(초)", 2f,
                new ConfigDescription(
                    "레이드 밖에서도 보낸다 — 외부 창이 '게임 안 켜짐' 과 '레이드 아님' 을 구분해야 한다.",
                    new AcceptableValueRange<float>(1f, 10f)));

            _verbose = Config.Bind("일반", "자세히 적기", false,
                "보고와 폴링 결과를 매번 로그에 적는다. 평소엔 끈다.");

            _log.LogInfo($"[AirdropCaller] 준비됨. 서버 {RequestHandler.Host}");
        }

        private void Update()
        {
            if (!_enabled.Value)
            {
                return;
            }

            var world = GetClientGameWorld(out var manager);
            var now = Time.unscaledTime;

            if (now >= _nextHeartbeatAt && IsDone(_heartbeatCall))
            {
                _nextHeartbeatAt = now + _heartbeatSeconds.Value;
                SendHeartbeat(world, manager);
            }

            // 예약은 레이드 안에서만 꺼내 간다. 밖에서 꺼내면 떨어뜨릴 곳이 없다.
            if (world != null && manager != null && now >= _nextPollAt)
            {
                if (IsDone(_pendingCall))
                {
                    ReadPending(world);
                    _nextPollAt = now + _pollSeconds.Value;
                    _pendingCall = RequestHandler.GetJsonAsync(PendingUrl);
                }
            }
        }

        private static bool IsDone(Task<string> task) => task == null || task.IsCompleted;

        // ── 보고 ───────────────────────────────────────────────────────────

        private void SendHeartbeat(ClientGameWorld world, ServerAirdropManager manager)
        {
            var inRaid = world != null && manager != null;
            var body = new HeartbeatBody
            {
                inRaid = inRaid,
                map = inRaid ? world.LocationId : null,
                canSummon = inRaid && manager.CanSummonAirdrop,
                crateInFlight = inRaid && HasPlaneInAir(world),
                availablePoints = inRaid ? manager._airdropPoints.Count : 0,
            };

            try
            {
                _heartbeatCall = RequestHandler.PostJsonAsync(HeartbeatUrl, JsonConvert.SerializeObject(body));
                if (_verbose.Value)
                {
                    _log.LogInfo($"[AirdropCaller] 보고: 레이드={body.inRaid} 맵={body.map} "
                                 + $"지점={body.availablePoints} 비행중={body.crateInFlight}");
                }
            }
            catch (Exception e)
            {
                _log.LogError("[AirdropCaller] 상태 보고에 실패했다: " + e.Message);
            }
        }

        /// <summary>
        /// 비행기가 아직 떠 있는가. 서버가 "지금 또 부르면 안 된다" 를 판단하는 근거다.
        ///
        /// **보급 상자(AirDrop)는 세지 않는다.** 처음에는 "Air" 가 들어간 객체를 모두
        /// 셌는데, 착지한 상자가 `_activeSynchronizableObjects` 에 계속 남아 있어
        /// (상자는 레이드가 끝날 때까지 주울 수 있는 물건이다) 한 번 부르고 나면 영영
        /// "비행 중" 이 됐다. 실측에서 `비행중=True` 가 13분 넘게 이어졌고, 그래서
        /// 두 번째 드롭이 막혔다.
        ///
        /// 막아야 하는 것은 **비행기가 두 대 뜨는 것**이지 상자가 땅에 있는 것이 아니다.
        /// </summary>
        private static bool HasPlaneInAir(ClientGameWorld world)
        {
            var active = world.ClientSynchronizableObjectLogicProcessor?._activeSynchronizableObjects;
            if (active == null)
            {
                return false;
            }

            foreach (var so in active)
            {
                if (so != null && so.Type == SynchronizableObjectType.AirPlane)
                {
                    return true;
                }
            }

            return false;
        }

        // ── 실행 ───────────────────────────────────────────────────────────

        private void ReadPending(ClientGameWorld world)
        {
            if (_pendingCall == null)
            {
                return;
            }

            var call = _pendingCall;
            _pendingCall = null;

            if (call.IsFaulted)
            {
                _log.LogError("[AirdropCaller] 예약 확인에 실패했다: " + call.Exception?.GetBaseException().Message);
                return;
            }

            string dropId;
            try
            {
                dropId = JsonConvert.DeserializeObject<PendingBody>(call.Result)?.dropId;
            }
            catch (Exception e)
            {
                _log.LogError("[AirdropCaller] 예약 응답을 못 읽었다: " + e.Message);
                return;
            }

            if (string.IsNullOrEmpty(dropId))
            {
                return;
            }

            Drop(world, dropId);
        }

        private void Drop(ClientGameWorld world, string dropId)
        {
            var player = world.MainPlayer;
            if (player == null)
            {
                _log.LogError($"[AirdropCaller] 예약 {dropId} 을 꺼냈는데 MainPlayer 가 없다 — 떨어뜨리지 못했다.");
                return;
            }

            var at = player.Transform.position;
            try
            {
                // 조사에서 찾은 호출. 가장 가까운 사전 정의 보급 지점으로 떨어진다.
                world.InitAirdrop(dropId, takeNearbyPoint: true, position: at);
                _log.LogInfo($"[AirdropCaller] 예약 {dropId} 실행 — 내 자리에서 가장 가까운 지점으로 부른다.");
            }
            catch (Exception e)
            {
                // 꺼내 간 예약은 이미 서버에서 지워졌다. 여기서 터지면 그 드롭은 사라진다.
                _log.LogError($"[AirdropCaller] 예약 {dropId} 실행이 터졌다: {e}");
            }
        }

        // ── 거들기 ─────────────────────────────────────────────────────────

        private static ClientGameWorld GetClientGameWorld(out ServerAirdropManager manager)
        {
            manager = null;

            if (!Singleton<GameWorld>.Instantiated)
            {
                return null;
            }

            var world = Singleton<GameWorld>.Instance as ClientGameWorld;
            var processor = world?.ClientSynchronizableObjectLogicProcessor;
            if (processor == null)
            {
                return null;
            }

            manager = processor.ServerAirdropManager;
            return manager == null ? null : world;
        }

        private class HeartbeatBody
        {
            public bool inRaid;
            public string map;
            public bool canSummon;
            public bool crateInFlight;
            public int availablePoints;
        }

        private class PendingBody
        {
            // 속성으로 둔다. 필드로 두면 "아무도 안 넣는다" 는 경고가 뜬다 —
            // 실제로 채우는 것은 역직렬화기다.
            public string dropId { get; set; }
        }
    }
}
