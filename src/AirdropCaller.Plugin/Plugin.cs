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
        private ConfigEntry<float> _hushSeconds;
        private ConfigEntry<bool> _fixParachute;
        private ConfigEntry<float> _stuckMeters;

        /// <summary>상자 하나를 지켜본 기록. 열쇠는 동기 물체의 인스턴스 번호다.</summary>
        private sealed class Watched
        {
            /// <summary>`Landed` 를 처음 본 시각. 0 이면 아직 못 봤다.</summary>
            public float LandedAt;

            /// <summary>`ParachuteEndOpen` 을 처음 본 시각과 그때의 높이.</summary>
            public float EndOpenAt;
            public float EndOpenY;

            /// <summary>한 상자에 한 번씩만 한다.</summary>
            public bool Rescued;
            public bool Reported;
        }

        private readonly Dictionary<int, Watched> _watched = new Dictionary<int, Watched>();

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

            _hushSeconds = Config.Bind("소리", "착지 뒤 반복 소리 끄기(초)", 2f,
                new ConfigDescription(
                    "상자가 땅에 앉고 이만큼 지나면 반복 재생되는 소리를 끊는다. "
                    + "바닐라는 이 소리를 **영영 끄지 않는다** — 착지음이 끝나면 연막 소리가 "
                    + "반복으로 켜지고, 연막이 다 타도(PlaneAirdropSmoke=600초) 소리만 계속 돈다. "
                    + "음수로 두면 바닐라대로 둔다.",
                    new AcceptableValueRange<float>(-1f, 60f)));

            _fixParachute = Config.Bind("구조", "남은 낙하산 접기", true,
                "상자가 `Landed` 인데 낙하산이 1초 넘게 남아 있으면 우리가 접는다. "
                + "바닐라의 착지 분기는 한 번 겨눔이라, 그 `if` 가 첫 프레임에 거짓이면 "
                + "`CloseParachute` 가 영영 안 불린다.");

            _stuckMeters = Config.Bind("구조", "막힌 것으로 보는 낙하 거리(m)", 200f,
                new ConfigDescription(
                    "`ParachuteEndOpen` 에 들어선 뒤 이보다 더 내려갔으면 땅을 지나친 것으로 "
                    + "본다. 그 단계는 지면이 `ParachuteEndOpenHeight`(=180) 안에 들어와야 "
                    + "시작하므로 정상이면 180m 안에 멈춘다. 지면을 다시 찾아 그 자리에 앉힌다. "
                    + "0 이면 구조하지 않는다.",
                    new AcceptableValueRange<float>(0f, 1000f)));

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

            TendCrates(world, now);

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
                                   + $"@{p.x:F0},{p.y:F0},{p.z:F0}{GroundUnder(p)}");
                    }
                    else
                    {
                        stages.Add($"?@{p.x:F0},{p.y:F0},{p.z:F0}");
                    }
                }
            }

            return (planes, crates, string.Join(",", stages.ToArray()));
        }

        /// <summary>
        /// 떨어지는·앉은 상자를 매 프레임 지켜보고 **바닐라가 놓친 것을 메운다.**
        ///
        /// 세 가지다. 근거는 doc/specs/2026-10-28-airdrop-caller-design.md §16.
        ///
        /// 1. **소리** — 착지음이 끝나는 순간 `ManualUpdate` 의 꼬리가 연막 소리를
        ///    `looped: true` 로 켜고, 그 뒤로는 끌 길이 없다(반복 재생 중에는
        ///    `PlayBackState` 가 늘 `Playing` 이라 `ReleaseSoundSource` 쪽으로 못 간다).
        ///    한 번 풀어 주면 `_loopSoundSource` 가 null 이 되어 다시 켜지지 않는다.
        /// 2. **낙하산** — 착지 분기가 `if` 하나에 걸려 있고 한 번 참이면 먼지가 켜져
        ///    다시는 참이 안 된다. 그 `if` 가 첫 `Landed` 프레임에 거짓이면
        ///    `CloseParachute` 가 영영 안 불린다. 그럴 때 우리가 접는다.
        /// 3. **지나친 상자** — `CollisionEnter` 는 Loot·Player·Triggers·**Default**
        ///    층을 무시한다. 그 층짜리만 밑에 있으면 멈출 것이 없어 계속 내려간다.
        ///    단계 전환은 `TerrainLowPoly` 레이캐스트로 하고 멈추는 것은 물리 충돌로
        ///    하니, 둘이 어긋나는 자리가 있다. 너무 내려간 상자는 지면을 다시 찾아 앉힌다.
        /// </summary>
        private void TendCrates(ClientGameWorld world, float now)
        {
            var active = world?.ClientSynchronizableObjectLogicProcessor?._activeSynchronizableObjects;
            if (active == null)
            {
                // 레이드 밖이다. 다음 판에 번호가 겹치는 일이 없게 비워 둔다.
                if (_watched.Count > 0)
                {
                    _watched.Clear();
                }

                return;
            }

            foreach (var so in active)
            {
                if (so == null || so.Type != SynchronizableObjectType.AirDrop)
                {
                    continue;
                }

                if (!(so.Logic is ClientAirDrop drop))
                {
                    continue;
                }

                var id = so.GetInstanceID();
                if (!_watched.TryGetValue(id, out var seen))
                {
                    seen = new Watched();
                    _watched[id] = seen;
                }

                if (drop._fallingStage == EAirdropFallingStage.ParachuteEndOpen)
                {
                    WatchDescent(so, drop, seen, now);
                }
                else if (drop._fallingStage == EAirdropFallingStage.Landed)
                {
                    TendLanded(drop, seen, now);
                }
            }
        }

        /// <summary>내려오는 중이다. 지면을 지나쳤는지만 본다.</summary>
        private void WatchDescent(SynchronizableObject so, ClientAirDrop drop, Watched seen, float now)
        {
            var y = so.transform.position.y;

            if (seen.EndOpenAt <= 0f)
            {
                seen.EndOpenAt = now;
                seen.EndOpenY = y;
                return;
            }

            if (seen.Rescued || _stuckMeters.Value <= 0f || seen.EndOpenY - y <= _stuckMeters.Value)
            {
                return;
            }

            seen.Rescued = true;
            _log.LogWarning($"[AirdropCaller] 상자가 {seen.EndOpenY - y:F0}m 내려갔는데 아직 안 멈췄다. "
                            + $"자리 {so.transform.position}. 지면을 찾아 앉힌다.");
            Settle(so, drop);
        }

        /// <summary>
        /// 멈추지 못한 상자를 지면에 앉힌다.
        ///
        /// `CollisionEnter` 에 **무시하지 않는 층의 collider** 를 넘겨 주는 것이 요령이다.
        /// 바닐라가 단계 전환에 쓰는 것과 같은 `TerrainLowPoly` 를 쓴다. 그 뒤의
        /// `CalcExtents` 가 네 귀퉁이에서 2m 만 아래로 재므로 **자리를 먼저 옮기고**
        /// 부른다. 아래에서 못 찾으면 위도 본다 — 이미 지나쳤을 수 있다.
        /// </summary>
        private void Settle(SynchronizableObject so, ClientAirDrop drop)
        {
            var from = so.transform.position;
            var mask = LayersMaskController.TerrainLowPoly;

            if (Physics.Raycast(from + Vector3.up * 0.5f, Vector3.down, out var below, 600f, mask))
            {
                Land(so, drop, below, "아래");
                return;
            }

            if (Physics.Raycast(from, Vector3.up, out var above, 600f, mask))
            {
                Land(so, drop, above, "위");
                return;
            }

            _log.LogWarning("[AirdropCaller] 위아래 600m 안에 지면이 없다. 손을 못 쓴다.");
        }

        private void Land(SynchronizableObject so, ClientAirDrop drop, RaycastHit hit, string where)
        {
            var seat = hit.point + Vector3.up * 0.5f;
            so.transform.position = seat;

            // 클라이언트는 매 프레임 `_destinationPosition` 쪽으로 보간한다. 그 값은
            // 서버 쪽 자리에서 오지만, 우리가 옮긴 프레임에는 아직 옛 자리다 — 같이
            // 고쳐 주지 않으면 한 프레임 동안 원래 자리로 끌려 내려간다.
            drop._destinationPosition = seat;

            drop.CollisionEnter(hit.collider);
            _log.LogInfo($"[AirdropCaller] {where}쪽 지면 {hit.point} 에 앉혔다 "
                         + $"(collider {hit.collider?.name}, 층 {hit.collider?.gameObject.layer}). "
                         + $"단계 {drop._fallingStage}.");
        }

        /// <summary>앉았다. 낙하산과 소리를 뒤처리한다.</summary>
        private void TendLanded(ClientAirDrop drop, Watched seen, float now)
        {
            if (seen.LandedAt <= 0f)
            {
                seen.LandedAt = now;
            }

            var dustOn = drop._airdropDust != null && drop._airdropDust.activeSelf;
            var parachuteOn = drop._parachute != null && drop._parachute.activeSelf;

            // 첫 `Landed` 프레임의 두 조건을 한 번 적어 둔다. ② 가 다시 나면
            // 어느 쪽이 거짓이었는지가 여기 남는다.
            if (!seen.Reported)
            {
                seen.Reported = true;
                _log.LogInfo($"[AirdropCaller] 앉았다 — 시작{drop._startFallingStage}, "
                             + $"먼지{(dustOn ? "켜짐" : "꺼짐")}, 낙하산{(parachuteOn ? "남음" : "닫힘")}.");
            }

            // `_startClosingParachute` 를 같이 본다. 정상 판에서도 닫는 데 1초가
            // 걸리고(`ParachuteFadeCoroutine`) 그동안 `activeSelf` 는 참이다 — 그것만
            // 보면 멀쩡한 상자마다 헛경고가 난다. 바닐라가 **접기 시작조차 안 했을 때**만
            // 우리가 끼어든다.
            if (_fixParachute.Value && parachuteOn && !drop._startClosingParachute
                && now - seen.LandedAt > 1f)
            {
                drop.CloseParachute();
                _log.LogWarning("[AirdropCaller] 앉았는데 낙하산이 남아 있었다. 우리가 접었다.");
            }

            if (_hushSeconds.Value >= 0f
                && drop._loopSoundSource != null
                && now - seen.LandedAt >= _hushSeconds.Value)
            {
                drop.ReleaseSoundSource();
                _log.LogInfo("[AirdropCaller] 착지한 상자의 반복 소리를 끊었다.");
            }
        }

        /// <summary>
        /// 상자 아래에 지면이 있는지 적는다. 바닐라가 단계 전환에 쓰는 것과 같은
        /// `TerrainLowPoly` 다. ① 이 다시 나면 "밑에 땅이 있었는가" 가 여기 남는다.
        /// </summary>
        private static string GroundUnder(Vector3 at)
        {
            if (Physics.Raycast(at + Vector3.up * 0.5f, Vector3.down, out var hit, 600f,
                                LayersMaskController.TerrainLowPoly))
            {
                return $",지면{hit.point.y:F0}";
            }

            return ",지면없음";
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
