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
    /// 보급 지점(AirdropPoint)이 맵에 몇 개 있고 플레이어에게서 얼마나 먼지 잰다.
    /// 보급 호출기(AirdropCaller) 설계의 §10-9 를 답하기 위한 측정 전용 도구다 —
    /// 아무것도 바꾸지 않고 읽어서 적기만 한다. 측정이 끝나면 지운다.
    /// 근거: doc/specs/2026-10-29-airdrop-stage1-findings.md
    /// </summary>
    [BepInPlugin(PluginGuid, "보급 지점 측정기", "0.1.0")]
    public class AirdropProbePlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "spt.airdropprobe";

        private static ManualLogSource _log;

        private ConfigEntry<KeyboardShortcut> _measureKey;
        private ConfigEntry<bool> _measureOnRaidStart;
        private ConfigEntry<int> _nearestCount;
        private ConfigEntry<bool> _writeFile;

        /// <summary>레이드마다 자동 측정을 한 번만 하기 위한 표시.</summary>
        private bool _autoMeasuredThisRaid;

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

            _logFilePath = Path.Combine(Path.GetDirectoryName(Info.Location) ?? ".", "probe.log");

            _log.LogInfo("[Probe] 보급 지점 측정기 준비됨. 측정 키: " + _measureKey.Value + " (레이드 안에서)");
        }

        private void Update()
        {
            if (!TryGetAirdropManager(out var world, out var manager))
            {
                // 레이드를 나갔으면 다음 레이드에서 다시 자동 측정한다.
                _autoMeasuredThisRaid = false;
                return;
            }

            if (_measureOnRaidStart.Value && !_autoMeasuredThisRaid && manager.IsInited)
            {
                _autoMeasuredThisRaid = true;
                Measure(world, manager, "레이드 시작");
            }

            if (_measureKey.Value.IsDown())
            {
                Measure(world, manager, "키 누름");
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
                var report = BuildReport(world, manager, why);
                _log.LogInfo(report);

                if (_writeFile.Value)
                {
                    File.AppendAllText(_logFilePath, report + Environment.NewLine, Encoding.UTF8);
                }
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

            if (rows.Count > detail)
            {
                var rest = new StringBuilder();
                for (var i = detail; i < rows.Count; i++)
                {
                    if (rest.Length > 0)
                    {
                        rest.Append(", ");
                    }

                    rest.Append(rows[i].Distance.ToString("0", inv));
                }

                sb.AppendLine("나머지 " + (rows.Count - detail) + "개 거리(m): " + rest);
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
