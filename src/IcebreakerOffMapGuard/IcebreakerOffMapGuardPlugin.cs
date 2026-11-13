using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using IcebreakerOffMapGuard.Patches;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IcebreakerOffMapGuard
{
    /// <summary>
    /// 아이스브레이커 백포트(2866, `com.manimal.icebreaker`)가 **그 맵이 아닌 곳에서** 하는
    /// 일 둘을 멈춘다. 경위는 `doc/fixes/2026-11-11-icebreaker-offmap-guard.md`.
    ///
    /// 그 모드를 고치는 것이 아니라 **밖에서 두 메서드만** 가린다. 모드가 없으면 아무것도 걸지
    /// 않고 조용히 끝난다(대상을 이름으로 찾아 null 이면 그만둔다).
    /// </summary>
    [BepInPlugin(PluginGuid, "Icebreaker Off-Map Guard", "0.1.0")]
    [BepInDependency(IcebreakerGuid, BepInDependency.DependencyFlags.SoftDependency)]
    public class IcebreakerOffMapGuardPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "spt.icebreakeroffmapguard";
        public const string IcebreakerGuid = "com.manimal.icebreaker";

        private const string CensusTarget = "Manimal.Icebreaker.RenderEnvProbe:TickVanillaPerfCensus";
        private const string LoadedTarget = "Manimal.Icebreaker.IcebreakerAcoustics:IcebreakerLoaded";

        private static ConfigEntry<bool> _skipCensus;
        private static ConfigEntry<bool> _cacheLoaded;
        private static ConfigEntry<bool> _report;

        /// <summary>60초마다 도는 장면 전수조사를 건너뛴다.</summary>
        public static bool SkipPerfCensus => _skipCensus != null && _skipCensus.Value;

        /// <summary>"아이스브레이커가 떠 있나" 를 씬이 바뀔 때까지 기억한다.</summary>
        public static bool CacheLoadedFlag => _cacheLoaded != null && _cacheLoaded.Value;

        private static ManualLogSource _log;
        private float _nextReportAt;

        private void Awake()
        {
            _log = Logger;

            // 설정·패치가 터져도 로그 한 줄은 남아야 한다 — 2026-11-11 설치본은 두 번째 바인드에서
            // 터져 아무 줄도 남기지 못했고, 그래서 "깔렸는데 안 돈다" 를 하루 동안 몰랐다.
            try
            {
                Setup();
            }
            catch (Exception e)
            {
                _log.LogError($"설정·패치 중에 터졌다 — 아무것도 가리지 못한다: {e}");
            }
        }

        private void Setup()
        {
            _skipCensus = Config.Bind(ConfigKeys.Section, ConfigKeys.SkipCensus, true,
                "Skip RenderEnvProbe.TickVanillaPerfCensus(). It only measures: about 60 s after you spawn on a map that is not Icebreaker it walks every CullingGridContent and LODGroup and logs one [PerfCensus] warning. Measured here: 58529 LOD groups on Lighthouse. Off = the mod's own behaviour.");

            _cacheLoaded = Config.Bind(ConfigKeys.Section, ConfigKeys.CacheLoaded, true,
                "Remember the answer of IcebreakerAcoustics.IcebreakerLoaded() until a scene is loaded or unloaded. The mod calls it every frame from RenderEnvProbe.Update and it reads the name of every loaded scene. The answer itself is still the mod's own - this only stops asking the same question every frame.");

            _report = Config.Bind(ConfigKeys.Section, ConfigKeys.Report, false,
                "Write how many calls were served from the cache and how many censuses were skipped. For the before/after measurement only - leave it off for normal play.");

            var harmony = new Harmony(PluginGuid);

            var census = AccessTools.Method(CensusTarget);
            var loaded = AccessTools.Method(LoadedTarget);

            // 아이스브레이커가 없는 설치본에서는 둘 다 null 이다 — 그때는 거는 것이 없다.
            if (census == null && loaded == null)
            {
                _log.LogInfo($"{IcebreakerGuid} 를 못 찾았다 — 걸 것이 없다. 아무것도 패치하지 않는다.");
                return;
            }

            if (census != null)
            {
                harmony.Patch(census, prefix: new HarmonyMethod(AccessTools.Method(typeof(PerfCensusPatch), nameof(PerfCensusPatch.Prefix))));
                _log.LogInfo($"걸었다: {CensusTarget}");
            }
            else
            {
                _log.LogWarning($"못 찾았다: {CensusTarget} — 모드 쪽에서 이름이 바뀌었을 수 있다");
            }

            if (loaded != null)
            {
                harmony.Patch(loaded,
                    prefix: new HarmonyMethod(AccessTools.Method(typeof(IcebreakerLoadedPatch), nameof(IcebreakerLoadedPatch.Prefix))),
                    postfix: new HarmonyMethod(AccessTools.Method(typeof(IcebreakerLoadedPatch), nameof(IcebreakerLoadedPatch.Postfix))));
                SceneManager.sceneLoaded += OnSceneLoaded;
                SceneManager.sceneUnloaded += OnSceneUnloaded;
                SceneManager.activeSceneChanged += OnActiveSceneChanged;
                _log.LogInfo($"걸었다: {LoadedTarget} (씬 이벤트에서 버린다)");
            }
            else
            {
                _log.LogWarning($"못 찾았다: {LoadedTarget} — 모드 쪽에서 이름이 바뀌었을 수 있다");
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => SceneMemo.Invalidate();

        private static void OnSceneUnloaded(Scene scene) => SceneMemo.Invalidate();

        private static void OnActiveSceneChanged(Scene from, Scene to) => SceneMemo.Invalidate();

        private void Update()
        {
            if (_report == null || !_report.Value)
            {
                return;
            }

            var now = Time.unscaledTime;
            if (now < _nextReportAt)
            {
                return;
            }

            _nextReportAt = now + 60f;
            _log.LogInfo(
                $"캐시로 답한 호출 {SceneMemo.Stats.Served} · 원본에게 물은 횟수 {SceneMemo.Stats.Stored} · " +
                $"씬 변화로 버린 횟수 {SceneMemo.Stats.Invalidated} · 건너뛴 census {PerfCensusPatch.Skipped}");
        }
    }
}
