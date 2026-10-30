using System.Collections.Generic;
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

        private const string HeartbeatUrl = "/airdrop-caller/heartbeat";

        private static ManualLogSource _log;

        private ConfigEntry<bool> _enabled;
        private ConfigEntry<float> _heartbeatSeconds;
        private ConfigEntry<bool> _verbose;

        private float _nextHeartbeatAt;

        /// <summary>서버가 일러 준 다음 보고까지의 시간. 0 이면 아직 못 들었다.</summary>
        private double _nextSeconds;

        /// <summary>
        /// 서버 호출은 **기다리지 않는다.** `RequestHandler` 의 동기 메서드를 쓰면 응답이
        /// 올 때까지 프레임이 멈춘다. 보낸 일감을 들고 있다가 끝났을 때만 읽는다.
        /// </summary>
        private Task<string> _heartbeatCall;

        private void Awake()
        {
            _log = Logger;

            _enabled = Config.Bind("일반", "켜기", true,
                "끄면 보고도 폴링도 하지 않는다. 바닐라 보급은 그대로다.");

            _heartbeatSeconds = Config.Bind("일반", "상태 보고 주기(초)", 2f,
                new ConfigDescription(
                    "가장 빠를 때의 주기다. 예약도 이 보고의 답으로 받아 온다. "
                    + "창이 꺼져 있으면 서버가 더 느리게 보내라고 일러 주므로 실제로는 이보다 뜸해진다. "
                    + "레이드 밖에서도 보낸다 — 외부 창이 '게임 안 켜짐' 과 '레이드 아님' 을 구분해야 한다.",
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

            // 길이 하나다. 보고를 보내고, 그 답에 예약이 실려 온다.
            //
            // 전에는 `/pending` 을 1초마다 따로 물어봤다. 창이 꺼져 있어도 그랬다 —
            // 아무도 안 읽는 보고를 그 속도로 보내느라 게임 로그가 우리 것으로 가득
            // 찼다(최근 4000줄 중 3892줄). 주고받는 자리가 이미 있는데 길을 하나 더
            // 쓸 이유가 없었다.
            if (now >= _nextHeartbeatAt && IsDone(_heartbeatCall))
            {
                ReadHeartbeat(world);

                // 다음 때는 서버가 정한다. 답을 못 받았으면 설정값으로 돈다.
                _nextHeartbeatAt = now + Mathf.Clamp(
                    _nextSeconds > 0 ? (float)_nextSeconds : _heartbeatSeconds.Value,
                    _heartbeatSeconds.Value,
                    MaxHeartbeatSeconds);

                SendHeartbeat(world, manager);
            }
        }

        /// <summary>서버가 아무리 느리게 보내라 해도 여기까지다. 창을 켰을 때 너무 늦지 않게.</summary>
        private const float MaxHeartbeatSeconds = 30f;

        private static bool IsDone(Task<string> task) => task == null || task.IsCompleted;

        // ── 보고 ───────────────────────────────────────────────────────────

        private void SendHeartbeat(ClientGameWorld world, ServerAirdropManager manager)
        {
            var inRaid = world != null && manager != null;
            var counted = CountInAir(world);
            var body = new HeartbeatBody
            {
                inRaid = inRaid,
                map = inRaid ? world.LocationId : null,
                canSummon = inRaid && manager.CanSummonAirdrop,
                crateInFlight = inRaid && HasPlaneInAir(world),
                availablePoints = inRaid ? manager._airdropPoints.Count : 0,
                planes = counted.Planes,
                crates = counted.Crates,
                // `_currentAirdropsCount` 는 float 다. 세는 값이라 그대로 int 로 받는다.
                mapCount = inRaid ? (int)manager._currentAirdropsCount : 0,
                stages = counted.Stages,
            };

            try
            {
                // **반드시 Task.Run 으로 감싼다.** `PostJsonAsync` 는 async 지만, 첫 await
                // 앞까지는 **부른 스레드**에서 돈다 — 그리고 그 앞에 이것이 있다
                // (SPT.Common.Http.Client.SendAsync):
                //
                //     if (zipped) data = Zlib.Compress(data, ZlibCompression.Maximum);
                //
                // zlib.NET 의 `ZOutputStream` 을 최대 압축으로 새로 만드는 일이다. 호출마다
                // deflate 상태 버퍼를 통째로 할당한다. 그걸 Update() 에서 부르면 메인
                // 스레드가 그 할당과 GC 를 떠안고, 화면이 한 번 튄다 — 사용자가 보고한
                // "heartbeat 로그가 찍힐 때마다 깜빡인다" 가 이것이었다(2026-10-29).
                //
                // GET 은 data 가 null 이라 이 줄을 건너뛴다. 그래서 `/pending` 은 멀쩡했고
                // POST 인 보고만 튀었다.
                _heartbeatCall = Task.Run(() =>
                    RequestHandler.PostJsonAsync(HeartbeatUrl, JsonConvert.SerializeObject(body)));
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
        private static bool HasPlaneInAir(ClientGameWorld world) => CountInAir(world).Planes > 0;

        /// <summary>
        /// 하늘에 뜬 비행기와 상자를 센다. 계측용이다 — 둘 이상이면 보급이 겹친 것이고,
        /// 낙하산이 안 사라지는 증상의 첫 용의자다.
        /// </summary>
        private static (int Planes, int Crates, string Stages) CountInAir(ClientGameWorld world)
        {
            var active = world?.ClientSynchronizableObjectLogicProcessor?._activeSynchronizableObjects;
            if (active == null)
            {
                return (0, 0, string.Empty);
            }

            var planes = 0;
            var crates = 0;
            var stages = new List<string>();

            foreach (var so in active)
            {
                if (so == null)
                {
                    continue;
                }

                if (so.Type == SynchronizableObjectType.AirPlane)
                {
                    planes++;
                }
                else if (so.Type == SynchronizableObjectType.AirDrop)
                {
                    crates++;

                    // **자리까지 같이 적는다.** 어떤 상자는 내려앉고 어떤 상자는
                    // `ParachuteEndOpen` 에 멈춰 땅을 뚫는데, 상자 종류도 겹침도
                    // 원인이 아니었다(2026-10-30). 남은 변수는 **어디에 떨어지느냐** 다.
                    // 멈춘 상자의 마지막 자리를 알면 지도에서 짚어 볼 수 있다.
                    var p = so.transform.position;
                    if (so.Logic is ClientAirDrop drop)
                    {
                        // `_startFallingStage` 와 먼지 효과를 같이 적는다.
                        //
                        // `Landed` 까지 갔는데도 낙하산이 남은 판이 있었다(실측
                        // 2026-10-30 08:11). `ClientAirDrop` 의 `Landed` 분기는
                        // **한 번만** 도는 구조라, 아래 `if` 가 한 번도 참이 안 되면
                        // `CloseParachute` 가 영영 안 불린다:
                        //
                        //   case Landed:
                        //     if (_startFallingStage != _fallingStage && !_airdropDust.activeSelf)
                        //       { ...; CloseParachute(); }
                        //
                        // 그래서 그 두 값을 본다. 어느 쪽이 거짓이었는지가 답이다.
                        var same = drop._startFallingStage == drop._fallingStage ? "같음" : "다름";
                        var dust = drop._airdropDust != null && drop._airdropDust.activeSelf ? "먼지켜짐" : "먼지꺼짐";
                        stages.Add($"{drop._fallingStage}(시작{drop._startFallingStage},{same},{dust})"
                                   + $"@{p.x:F0},{p.y:F0},{p.z:F0}");
                    }
                    else
                    {
                        stages.Add($"?@{p.x:F0},{p.y:F0},{p.z:F0}");
                    }
                }
            }

            return (planes, crates, string.Join(",", stages.ToArray()));
        }

        // ── 실행 ───────────────────────────────────────────────────────────

        /// <summary>
        /// 지난 보고의 답을 읽는다. 예약이 실려 있으면 그 자리에서 떨어뜨린다.
        /// 서버가 일러 준 다음 주기도 여기서 받는다.
        /// </summary>
        private void ReadHeartbeat(ClientGameWorld world)
        {
            if (_heartbeatCall == null)
            {
                return;
            }

            var call = _heartbeatCall;
            _heartbeatCall = null;

            if (call.IsFaulted)
            {
                _log.LogError("[AirdropCaller] 상태 보고에 실패했다: " + call.Exception?.GetBaseException().Message);
                return;
            }

            HeartbeatAnswer answer;
            try
            {
                answer = JsonConvert.DeserializeObject<HeartbeatAnswer>(call.Result);
            }
            catch (Exception e)
            {
                _log.LogError("[AirdropCaller] 보고 응답을 못 읽었다: " + e.Message);
                return;
            }

            if (answer == null)
            {
                return;
            }

            _nextSeconds = answer.nextSeconds;

            if (string.IsNullOrEmpty(answer.dropId))
            {
                return;
            }

            // 예약은 레이드 안에서만 꺼내 온다(서버가 그렇게 보낸다). 그래도 한 번 더 본다.
            if (world == null)
            {
                _log.LogError($"[AirdropCaller] 예약 {answer.dropId} 을 받았는데 레이드가 아니다 — 떨어뜨리지 못했다.");
                return;
            }

            Drop(world, answer.dropId);
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

                // 부른 자리를 적어 둔다. 멈춘 상자가 어디서 멈췄는지와 맞춰 보려면
                // **어디서 불렀는지**도 있어야 한다 — 지점은 이 자리에서 가장 가까운
                // 것으로 뽑히기 때문이다.
                _log.LogInfo($"[AirdropCaller] 예약 {dropId} 실행 — 내 자리"
                             + $"({at.x:F0},{at.y:F0},{at.z:F0})에서 가장 가까운 지점으로 부른다.");
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

            /// <summary>
            /// 계측용. 낙하산이 안 사라지고 맵에 표시가 안 되는 일이 **가끔** 생긴다
            /// (2026-10-30 사용자 보고). 보급이 겹쳐서인지 보려고 센다.
            ///
            /// 바닐라는 `InitAirdrop` 앞뒤로 `_currentAirdropsCount++` 와
            /// `UpdateAirdropTimers` 를 하는데 우리는 둘 다 건너뛴다. 그래서 우리 상자가
            /// 하늘에 있는 동안에도 맵이 자기 보급기를 띄울 수 있다 — 그 가설을 잰다.
            /// </summary>
            public int planes;

            /// <inheritdoc cref="planes"/>
            public int crates;

            /// <summary>맵이 센 보급 횟수. 우리는 안 올리므로, 늘면 맵이 자기 것을 띄운 것이다.</summary>
            public int mapCount;

            /// <summary>
            /// 하늘에 있는 상자들의 **낙하 단계**.
            /// `Fall → ParachuteStartOpen → ParachuteEndOpen → Contact → Landed`
            ///
            /// 낙하산은 `Contact`·`Landed` 에서만 닫힌다(`ClientAirDrop.CloseParachute`).
            /// 그런데 그 두 단계로 넘어가는 길은 `ServerAirDrop.CollisionEnter` 하나뿐이고,
            /// 거기서 **여러 레이어를 그냥 무시한다**(Loot·Player·Triggers·Default).
            /// 안 사라지는 낙하산이 `ParachuteEndOpen` 에 멈춰 있는지 여기서 본다.
            /// </summary>
            public string stages;
        }

        /// <summary>보고에 대한 답. 예약과 다음 주기가 실려 온다.</summary>
        private class HeartbeatAnswer
        {
            // 속성으로 둔다. 필드로 두면 "아무도 안 넣는다" 는 경고가 뜬다 —
            // 실제로 채우는 것은 역직렬화기다.
            public string dropId { get; set; }

            public double nextSeconds { get; set; }
        }
    }
}
