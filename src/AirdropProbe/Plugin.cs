using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Airdrop;
using UnityEngine;

namespace AirdropProbe
{
    /// <summary>
    /// 보급 호출기(AirdropCaller) 1단계용 시험 도구. 두 가지를 한다.
    ///
    /// 1. **측정** — 보급 지점(AirdropPoint)이 맵에 몇 개 있고 얼마나 먼지 잰다. 읽기만 한다.
    /// 2. **드롭 시험** — 키를 누르면 `ClientGameWorld.InitAirdrop` 을 직접 불러 상자를
    ///    떨어뜨린다. **이쪽은 게임을 실제로 바꾼다.** 키를 누를 때만 일어난다.
    ///
    /// 둘 다 끝나면 지운다. 근거: doc/specs/2026-10-29-airdrop-stage1-findings.md
    /// </summary>
    [BepInPlugin(PluginGuid, "보급 지점 측정기 + 드롭 시험", "0.3.1")]
    public class AirdropProbePlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "spt.airdropprobe";

        private static ManualLogSource _log;

        private ConfigEntry<KeyboardShortcut> _measureKey;
        private ConfigEntry<bool> _measureOnRaidStart;
        private ConfigEntry<int> _nearestCount;
        private ConfigEntry<bool> _writeFile;

        private ConfigEntry<KeyboardShortcut> _dropKey;
        private ConfigEntry<KeyCode> _dropKeySimple;
        private ConfigEntry<KeyCode> _measureKeySimple;
        private ConfigEntry<string> _lootTemplateId;
        private ConfigEntry<bool> _logKeys;

        /// <summary>키 로그를 1초에 한 번으로 줄인다.</summary>
        private float _nextKeyLogAt;

        /// <summary>레이드마다 자동 측정을 한 번만 하기 위한 표시.</summary>
        private bool _autoMeasuredThisRaid;

        /// <summary>드롭 키도 꾹 누르면 비행기가 여러 대 뜬다. 3초는 막는다.</summary>
        private float _nextDropAllowedAt;

        /// <summary>이 레이드에서 몇 번 떨어뜨렸나. 지점 소모를 눈으로 보려고 센다.</summary>
        private int _dropsThisRaid;

        /// <summary>
        /// 키를 꾹 누르면 프레임마다 재던 것을 막는다. 한 번 재는 데 씬 전체를 훑고
        /// (Customs 기준 지점 608개) 4KB 를 쓰므로, 5초에 14번은 낭비였다.
        /// </summary>
        private float _nextMeasureAllowedAt;

        private string _logFilePath;

        private void Awake()
        {
            _log = Logger;

            _measureKey = Config.Bind("측정", "측정 키", new KeyboardShortcut(KeyCode.A, KeyCode.LeftControl, KeyCode.LeftAlt),
                "이 키를 누르면 지금 서 있는 자리에서 보급 지점까지의 거리를 잰다. 레이드 안에서만 듣는다.");

            _measureOnRaidStart = Config.Bind("측정", "레이드 시작 때 한 번", true,
                "레이드에 들어가 보급 관리자가 준비되면 자동으로 한 번 잰다.");

            _nearestCount = Config.Bind("측정", "자세히 볼 가까운 지점 수", 5,
                new ConfigDescription(
                    "가까운 순서로 이만큼은 좌표·방향까지 적는다. 나머지는 거리만 적는다.",
                    new AcceptableValueRange<int>(1, 30)));

            _writeFile = Config.Bind("측정", "따로 파일에도 적기", true,
                "BepInEx 로그 말고 plugins\\AirdropProbe\\probe.log 에도 같은 내용을 쌓는다. 끄면 로그에만 적는다.");

            _dropKey = Config.Bind("드롭 시험", "드롭 키", new KeyboardShortcut(KeyCode.D, KeyCode.LeftControl, KeyCode.LeftAlt),
                "누르면 지금 자리에서 가장 가까운 보급 지점으로 상자를 떨어뜨린다. "
                + "**게임을 실제로 바꾼다.** 레이드 안에서만 듣는다.");

            // 기본값을 일부러 가짜 id 로 둔다. 서버는 모르는 id 를 받으면 경고를 찍고
            // 무작위 전리품으로 넘어가므로(AirdropService.GenerateCustomAirdropLoot),
            // 상자는 그대로 채워지면서 "우리가 넘긴 id 가 서버까지 갔다" 가 증명된다.
            // 순수 바닐라 경로만 보고 싶으면 이 값을 비운다 → null 을 넘긴다.
            _lootTemplateId = Config.Bind("드롭 시험", "전리품 id", "aabbccdd0000000000000001",
                "InitAirdrop 에 넘길 lootTemplateId. 비워 두면 null 을 넘겨 순수 바닐라 경로로 간다. "
                + "기본값은 아무 데도 없는 가짜 id 다 — 서버 로그에 이 값이 그대로 찍히면 "
                + "id 왕복이 증명된 것이고, 상자는 무작위 전리품으로 채워진다.");

            // 조합 키가 안 먹는 줄 알고 넣은 대안인데, 조합은 멀쩡했다. **기본은 꺼 둔다** —
            // 기본값을 주면 위에서 사용자가 조합 키에 지정한 키와 겹칠 수 있다(F7 이 실제로 그랬다).
            // 쓰고 싶으면 F12 에서 키를 넣는다.
            _dropKeySimple = Config.Bind("드롭 시험", "드롭 키(단일)", KeyCode.None,
                "조합 키 말고 단일 키로도 받고 싶을 때 쓴다. None 이면 끈 것이다. "
                + "위의 조합 키와 겹치지 않게 정한다.");

            _measureKeySimple = Config.Bind("측정", "측정 키(단일)", KeyCode.None,
                "조합 키 말고 단일 키로도 받고 싶을 때 쓴다. None 이면 끈 것이다. "
                + "위의 조합 키와 겹치지 않게 정한다.");

            // 키가 안 먹는 줄 알았을 때 쓰던 진단이다. 멀쩡한 것이 확인됐으니 기본은 꺼 둔다.
            _logKeys = Config.Bind("진단", "눌린 키를 로그에 찍기", false,
                "레이드 안에서 키를 누를 때마다 무엇이 눌렸는지 BepInEx 로그에 적는다(1초에 한 번). "
                + "키가 안 먹는 것 같을 때만 켠다 — 게임이 삼키는지 플러그인이 못 읽는지 가려진다.");

            _logFilePath = Path.Combine(Path.GetDirectoryName(Info.Location) ?? ".", "probe.log");

            _log.LogInfo("[Probe] 준비됨. 측정 " + _measureKey.Value + " 또는 " + _measureKeySimple.Value
                         + " / 드롭 " + _dropKey.Value + " 또는 " + _dropKeySimple.Value + " (레이드 안에서)");
        }

        private void Update()
        {
            var inRaid = TryGetAirdropManager(out var world, out var manager);

            // 레이드 밖에서도 찍는다. "키가 안 보인다" 와 "레이드로 안 쳐준다" 를 갈라야 한다.
            LogPressedKeys(inRaid);

            if (!inRaid)
            {
                // 레이드를 나갔으면 다음 레이드에서 다시 자동 측정한다.
                _autoMeasuredThisRaid = false;
                _dropsThisRaid = 0;
                return;
            }

            if (_measureOnRaidStart.Value && !_autoMeasuredThisRaid && manager.IsInited)
            {
                _autoMeasuredThisRaid = true;
                Measure(world, manager, "레이드 시작");
            }

            if (Pressed(_measureKey, _measureKeySimple) && Time.unscaledTime >= _nextMeasureAllowedAt)
            {
                _nextMeasureAllowedAt = Time.unscaledTime + 1f;
                Measure(world, manager, "키 누름");
            }

            if (Pressed(_dropKey, _dropKeySimple) && Time.unscaledTime >= _nextDropAllowedAt)
            {
                _nextDropAllowedAt = Time.unscaledTime + 3f;
                Drop(world, manager);
            }
        }

        /// <summary>조합 키와 단일 키 중 어느 쪽이 눌려도 참.</summary>
        private static bool Pressed(ConfigEntry<KeyboardShortcut> chord, ConfigEntry<KeyCode> simple)
        {
            if (simple.Value != KeyCode.None && Input.GetKeyDown(simple.Value))
            {
                return true;
            }

            return chord.Value.IsDown();
        }

        /// <summary>
        /// 눌린 키를 그대로 적는다. 아무것도 안 찍히면 게임이 입력을 삼키는 것이고,
        /// 찍히는데 드롭이 안 되면 조합 판정 쪽 문제다. 둘을 가르려고 둔다.
        /// </summary>
        private void LogPressedKeys(bool inRaid)
        {
            if (!_logKeys.Value || !Input.anyKeyDown || Time.unscaledTime < _nextKeyLogAt)
            {
                return;
            }

            var down = new List<string>();
            foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
            {
                // 마우스 버튼·조이스틱은 뺀다. 시끄럽기만 하다.
                if (code >= KeyCode.Mouse0)
                {
                    continue;
                }

                if (Input.GetKeyDown(code))
                {
                    down.Add(code.ToString());
                }
            }

            if (down.Count == 0)
            {
                return;
            }

            _nextKeyLogAt = Time.unscaledTime + 1f;

            var held = new List<string>();
            foreach (var mod in new[]
                     {
                         KeyCode.LeftControl, KeyCode.RightControl, KeyCode.LeftAlt, KeyCode.RightAlt,
                         KeyCode.LeftShift, KeyCode.RightShift,
                     })
            {
                if (Input.GetKey(mod))
                {
                    held.Add(mod.ToString());
                }
            }

            _log.LogInfo("[Probe][키] 눌림: " + string.Join(", ", down.ToArray())
                         + " / 누르고 있음: " + (held.Count == 0 ? "(없음)" : string.Join(", ", held.ToArray()))
                         + " / 레이드: " + (inRaid ? "예" : "아니오"));
        }

        /// <summary>
        /// 조사에서 찾은 호출을 그대로 해 본다 — 이것이 1단계의 전부다.
        ///   ClientGameWorld.InitAirdrop(lootTemplateId, takeNearbyPoint: true, position)
        /// 설계서 §7.4 / 조사 문서 §1.
        /// </summary>
        private void Drop(ClientGameWorld world, ServerAirdropManager manager)
        {
            var sb = new StringBuilder();
            var inv = CultureInfo.InvariantCulture;

            sb.AppendLine();
            sb.AppendLine("===== 드롭 시험 #" + (_dropsThisRaid + 1) + " "
                          + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", inv) + " =====");

            var player = world.MainPlayer;
            if (player == null)
            {
                sb.AppendLine("MainPlayer 가 null — 부르지 않았다.");
                Emit(sb.ToString());
                return;
            }

            var me = player.Transform.position;
            var id = string.IsNullOrWhiteSpace(_lootTemplateId.Value) ? null : _lootTemplateId.Value.Trim();

            // 부르기 전 상태. 부른 뒤와 견줘 보면 지점이 실제로 소모되는지 보인다.
            var availableBefore = manager._airdropPoints.Count;
            var deactivatedBefore = manager._deactivatedAirdropPoints.Count;
            var nearest = NearestAvailable(manager, me);

            sb.AppendLine("내 위치: " + Fmt(me, inv));
            sb.AppendLine("넘길 lootTemplateId: " + (id ?? "(null — 바닐라 무작위)"));
            sb.AppendLine("부르기 전: 쓸 수 있음 " + availableBefore + " / 비활성 " + deactivatedBefore
                          + " / 가장 가까운 지점 " + (nearest < 0f ? "없음" : nearest.ToString("0.0", inv) + "m"));
            sb.AppendLine("CanSummonAirdrop=" + manager.CanSummonAirdrop);

            try
            {
                world.InitAirdrop(id, takeNearbyPoint: true, position: me);
                _dropsThisRaid++;
                sb.AppendLine("InitAirdrop 호출 — 예외 없음.");
            }
            catch (Exception e)
            {
                sb.AppendLine("InitAirdrop 이 터졌다: " + e);
                Emit(sb.ToString());
                return;
            }

            // TakeAirdropPoint 가 지점을 빼 갔으면 숫자가 줄어 있어야 한다.
            var availableAfter = manager._airdropPoints.Count;
            var deactivatedAfter = manager._deactivatedAirdropPoints.Count;
            sb.AppendLine("부른 뒤: 쓸 수 있음 " + availableAfter + " (" + (availableAfter - availableBefore) + ")"
                          + " / 비활성 " + deactivatedAfter + " (+" + (deactivatedAfter - deactivatedBefore) + ")");
            sb.AppendLine("다음 드롭의 가장 가까운 지점: "
                          + Describe(NearestAvailable(manager, me), inv));
            sb.AppendLine("이제 하늘을 본다. 비행기 소리 → 낙하산 → 착지 순서다. "
                          + "상자를 열어 내용물이 무엇인지 적어 두면 좋다.");
            sb.AppendLine("===== 끝 =====");
            Emit(sb.ToString());
        }

        private static float NearestAvailable(ServerAirdropManager manager, Vector3 from)
        {
            var best = -1f;
            foreach (var point in manager._airdropPoints)
            {
                if (point == null || point.gameObject == null)
                {
                    continue;
                }

                var d = Vector3.Distance(from, point.transform.position);
                if (best < 0f || d < best)
                {
                    best = d;
                }
            }

            return best;
        }

        private static string Describe(float distance, IFormatProvider inv)
        {
            return distance < 0f ? "없음" : distance.ToString("0.0", inv) + "m";
        }

        private void Emit(string text)
        {
            _log.LogInfo(text);
            if (_writeFile.Value)
            {
                try
                {
                    File.AppendAllText(_logFilePath, text + Environment.NewLine, Encoding.UTF8);
                }
                catch (Exception e)
                {
                    _log.LogError("[Probe] 파일에 못 적었다: " + e.Message);
                }
            }
        }

        /// <summary>
        /// 레이드 안이고 보급 관리자를 잡을 수 있으면 true. 메뉴·하이드아웃에서는 false.
        /// </summary>
        private static bool TryGetAirdropManager(out ClientGameWorld world, out ServerAirdropManager manager)
        {
            world = null;
            manager = null;

            if (!Singleton<GameWorld>.Instantiated)
            {
                return false;
            }

            world = Singleton<GameWorld>.Instance as ClientGameWorld;
            if (world == null)
            {
                return false;
            }

            var processor = world.ClientSynchronizableObjectLogicProcessor;
            if (processor == null)
            {
                return false;
            }

            manager = processor.ServerAirdropManager;
            return manager != null;
        }

        private void Measure(ClientGameWorld world, ServerAirdropManager manager, string why)
        {
            try
            {
                Emit(BuildReport(world, manager, why));
            }
            catch (Exception e)
            {
                _log.LogError("[Probe] 측정 중 터졌다: " + e);
            }
        }

        private string BuildReport(ClientGameWorld world, ServerAirdropManager manager, string why)
        {
            var player = world.MainPlayer;
            var sb = new StringBuilder();
            var inv = CultureInfo.InvariantCulture;

            sb.AppendLine();
            sb.AppendLine("===== 보급 지점 측정 (" + why + ") " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", inv) + " =====");
            sb.AppendLine("맵: " + (world.LocationId ?? "(모름)"));

            // 드롭이 가능한 상태인가
            sb.AppendLine("관리자: IsInited=" + manager.IsInited
                          + " _airdropsEnabled=" + manager._airdropsEnabled
                          + " CanSummonAirdrop=" + manager.CanSummonAirdrop);

            var p = manager.CachedAirdropParameters;
            if (p == null)
            {
                sb.AppendLine("CachedAirdropParameters: null — 이 맵은 보급기 설정이 없다");
            }
            else
            {
                sb.AppendLine("설정: PlaneAirdropMax=" + p.PlaneAirdropMax
                              + " Chance=" + p.PlaneAirdropChance.ToString("0.###", inv)
                              + " MinPlayers=" + p.MinPlayersCountToSpawnAirdrop
                              + " 지점비활성거리=" + p.AirdropPointDeactivateDistance + "m"
                              + " 쿨다운=" + p.PlaneAirdropCooldownMin + "~" + p.PlaneAirdropCooldownMax + "s"
                              + " 시작=" + p.PlaneAirdropStartMin + "~" + p.PlaneAirdropStartMax + "s"
                              + " 끝=" + p.PlaneAirdropEnd + "s");
            }

            // 지점 개수. 관리자가 쥔 목록은 쓰면 줄어들고, 씬 전체는 줄지 않는다.
            var allInScene = UnityEngine.Object.FindObjectsOfType<AirdropPoint>();
            sb.AppendLine("지점: 씬 전체 " + allInScene.Length
                          + " / 쓸 수 있음 " + manager._airdropPoints.Count
                          + " / 비활성 " + manager._deactivatedAirdropPoints.Count);

            if (player == null)
            {
                sb.AppendLine("MainPlayer 가 null 이라 거리는 못 잰다.");
                sb.AppendLine("===== 끝 =====");
                return sb.ToString();
            }

            var me = player.Transform.position;
            var forward = player.Transform.forward;
            sb.AppendLine("내 위치: " + Fmt(me, inv));

            // 쓸 수 있는 지점만 거리순으로. takeNearbyPoint:true 는 이 목록의 첫 줄을 고른다.
            var rows = new List<Row>();
            foreach (var point in manager._airdropPoints)
            {
                if (point == null || point.gameObject == null)
                {
                    continue;
                }

                var at = point.transform.position;
                rows.Add(new Row
                {
                    Position = at,
                    Distance = Vector3.Distance(me, at),
                    Flat = Vector2.Distance(new Vector2(me.x, me.z), new Vector2(at.x, at.z)),
                    Up = at.y - me.y,
                    Bearing = Bearing(forward, at - me),
                });
            }

            rows.Sort((a, b) => a.Distance.CompareTo(b.Distance));

            if (rows.Count == 0)
            {
                sb.AppendLine("쓸 수 있는 지점이 없다 — 이 상태로 부르면 'There is no airdrop points here!' 가 찍힌다.");
                sb.AppendLine("===== 끝 =====");
                return sb.ToString();
            }

            var detail = Math.Min(_nearestCount.Value, rows.Count);
            sb.AppendLine("가까운 " + detail + "개 (takeNearbyPoint:true 는 1번을 고른다):");
            for (var i = 0; i < detail; i++)
            {
                var r = rows[i];
                sb.AppendLine("  " + (i + 1) + ") 직선 " + r.Distance.ToString("0.0", inv) + "m"
                              + " / 수평 " + r.Flat.ToString("0.0", inv) + "m"
                              + " / 높이차 " + r.Up.ToString("+0.0;-0.0;0", inv) + "m"
                              + " / " + r.Bearing
                              + " / " + Fmt(r.Position, inv));
            }

            // 거리 603개를 한 줄로 쏟으면 파일만 커지고 읽히지 않는다. 구간별로 센다.
            var edges = new[] { 25f, 50f, 100f, 200f, 400f, 800f };
            var buckets = new int[edges.Length + 1];
            foreach (var r in rows)
            {
                var slot = edges.Length;
                for (var e = 0; e < edges.Length; e++)
                {
                    if (r.Distance <= edges[e])
                    {
                        slot = e;
                        break;
                    }
                }

                buckets[slot]++;
            }

            var hist = new StringBuilder("거리 분포: ");
            for (var e = 0; e < edges.Length; e++)
            {
                hist.Append("≤").Append(edges[e].ToString("0", inv)).Append("m ").Append(buckets[e]).Append("  ");
            }

            hist.Append("그 밖 ").Append(buckets[edges.Length]);
            sb.AppendLine(hist.ToString());

            // 한 번 떨어뜨리면 AirdropPointDeactivateDistance 안의 지점이 통째로 비활성된다.
            // 그래서 "연달아 부르면 두 번째는 얼마나 멀어지나" 가 이 숫자로 답해진다.
            var deactivate = p?.AirdropPointDeactivateDistance ?? 0;
            if (deactivate > 0)
            {
                var within = 0;
                var firstOutside = -1f;
                foreach (var r in rows)
                {
                    if (r.Distance <= deactivate)
                    {
                        within++;
                    }
                    else if (firstOutside < 0f)
                    {
                        firstOutside = r.Distance;
                    }
                }

                sb.AppendLine("한 번 쓰면: " + deactivate + "m 안 지점 " + within + "개가 비활성 → "
                              + "다음 드롭은 " + (firstOutside < 0f ? "쓸 지점이 없어 전부 되살아난다" : firstOutside.ToString("0.0", inv) + "m 밖"));
            }

            sb.AppendLine("요약: 가장 가까운 지점 " + rows[0].Distance.ToString("0.0", inv) + "m"
                          + ", 가장 먼 지점 " + rows[rows.Count - 1].Distance.ToString("0.0", inv) + "m"
                          + ", 중간값 " + rows[rows.Count / 2].Distance.ToString("0.0", inv) + "m");
            sb.AppendLine("===== 끝 =====");
            return sb.ToString();
        }

        private static string Fmt(Vector3 v, IFormatProvider inv)
        {
            return "(" + v.x.ToString("0.0", inv) + ", " + v.y.ToString("0.0", inv) + ", " + v.z.ToString("0.0", inv) + ")";
        }

        /// <summary>
        /// 내가 보는 방향을 기준으로 "앞/오른쪽 30°" 처럼 적는다. 나침반 방위는 맵마다
        /// 북쪽이 달라 쓸모가 없고, 실제로 궁금한 것은 "지금 어느 쪽으로 가야 하나" 다.
        /// </summary>
        private static string Bearing(Vector3 forward, Vector3 toPoint)
        {
            var a = new Vector2(forward.x, forward.z);
            var b = new Vector2(toPoint.x, toPoint.z);
            if (a.sqrMagnitude < 0.0001f || b.sqrMagnitude < 0.0001f)
            {
                return "방향 모름";
            }

            var angle = Vector2.SignedAngle(a, b);
            var abs = Mathf.Abs(angle);
            var side = angle < 0f ? "오른쪽" : "왼쪽";

            if (abs < 20f)
            {
                return "앞";
            }

            if (abs > 160f)
            {
                return "뒤";
            }

            return side + " " + abs.ToString("0", CultureInfo.InvariantCulture) + "°";
        }

        private struct Row
        {
            public Vector3 Position;
            public float Distance;
            public float Flat;
            public float Up;
            public string Bearing;
        }
    }
}
