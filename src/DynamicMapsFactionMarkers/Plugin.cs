using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace DynamicMapsFactionMarkers
{
    /// <summary>
    /// Dynamic Maps 의 지도·미니맵에서 모드로 추가된 세력(ISB 등)을 역할 묶음마다
    /// 다른 색·모양으로 갈라 보이게 한다. Dynamic Maps 파일은 고치지 않고 Harmony 로만 끼어든다.
    /// 설계는 doc/specs/2026-10-30-dynamicmaps-faction-markers-design.md.
    /// </summary>
    [BepInPlugin(PluginGuid, "모드 세력 지도 표시", "0.1.0")]
    [BepInDependency("com.mpstark.dynamicmaps", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "spt.dynamicmapsfactionmarkers";

        internal static ManualLogSource Log;

        /// <summary>F12 토글 하나. 색·모양은 config.jsonc 에서 고친다.</summary>
        internal static ConfigEntry<bool> ShowFactionMarkers;

        internal static MarkerConfig Settings;
        internal static RoleCatalog Catalog;

        /// <summary>패치가 걸려 실제로 동작하는 중인지.</summary>
        internal static bool Active;

        private void Awake()
        {
            Log = Logger;

            ShowFactionMarkers = Config.Bind("일반", "모드 세력 표시", true,
                "모드로 추가된 세력(ISB 등)을 지도·미니맵에 따로 그린다. 끄면 바닐라 분류로 돌아간다.");

            var folder = PluginFolder();
            Settings = LoadSettings(folder);

            if (!Settings.Enabled)
            {
                Log.LogInfo("config.jsonc 의 enabled 가 false 라 아무 것도 하지 않는다.");
                return;
            }

            Catalog = new RoleCatalog(Settings);
            var registered = RoleSource.FillCatalog(Catalog);
            Log.LogInfo($"역할 {registered}건 등록(자동 배정 {Catalog.Discovered.Count}건).");

            if (Catalog.Discovered.Count > 0)
            {
                RoleSource.WriteDiscovered(folder, Catalog);
            }

            var harmony = new Harmony(PluginGuid);
            if (!MarkerPatches.TryApply(harmony))
            {
                return;
            }

            Active = true;

            // 토글을 뒤집으면 그 자리에서 다시 그린다(그들의 정리 코드는 자기 설정이 바뀔 때만 돈다).
            ShowFactionMarkers.SettingChanged += (_, __) => MapScreenBridge.RefreshNow();
        }

        private static MarkerConfig LoadSettings(string folder)
        {
            var path = Path.Combine(folder, "config.jsonc");

            if (!File.Exists(path))
            {
                Log.LogWarning("config.jsonc 가 없다. 기본값으로 돈다: " + path);
                return MarkerConfig.Default();
            }

            string text;
            try
            {
                text = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                Log.LogWarning("config.jsonc 를 읽지 못했다. 기본값으로 돈다: " + e.Message);
                return MarkerConfig.Default();
            }

            var settings = MarkerConfig.LoadFromText(text, out var warning);
            if (!string.IsNullOrEmpty(warning))
            {
                Log.LogWarning(warning);
            }

            return settings;
        }

        /// <summary>DLL 이 놓인 폴더. 폴더 이름을 바꿔도 따라간다.</summary>
        internal static string PluginFolder()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}
