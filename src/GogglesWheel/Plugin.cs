using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace GogglesWheel
{
    /// <summary>
    /// N 하나로 야시경과 페이스실드를 갈라 여닫는다 — 짧게 누르면 야시경,
    /// 길게 누르면 원형 메뉴에서 골라서.
    /// 설계는 doc/specs/2026-10-30-goggles-wheel-design.md.
    /// </summary>
    [BepInPlugin(PluginGuid, "야시경·실드 휠", "0.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "spt.goggleswheel";

        internal static ManualLogSource Log;
        internal static Plugin Instance;

        /// <summary>F12 토글 하나. 나머지는 config.jsonc 에서 고친다.</summary>
        internal static ConfigEntry<bool> EnableWheel;

        internal static WheelConfig Settings;

        /// <summary>패치가 걸려 실제로 동작하는 중인지.</summary>
        internal static bool Active;

        private void Awake()
        {
            Log = Logger;
            Instance = this;

            EnableWheel = Config.Bind("일반", "휠 쓰기", true,
                "끄면 N 이 게임이 하던 대로 동작한다(헬멧의 여닫이 중 첫 번째 하나만).");

            Settings = LoadSettings(PluginFolder());

            if (!Settings.Enabled)
            {
                Log.LogInfo("config.jsonc 의 enabled 가 false 라 아무 것도 하지 않는다.");
                return;
            }

            var harmony = new Harmony(PluginGuid);
            if (!GogglesPatch.TryApply(harmony))
            {
                return;
            }

            // 곁가지 — 못 걸어도 전체를 끄지 않는다. 휠이 떠 있는 동안 시점이 도는 것으로 끝난다.
            LookBlocker.TryPatch(harmony);

            Active = true;
            Log.LogInfo($"준비됐다. 길게 누름 기준 {Settings.LongPressSeconds:0.00}초, 기본 대상 {Settings.DefaultTarget}.");
        }

        private static WheelConfig LoadSettings(string folder)
        {
            var path = Path.Combine(folder, "config.jsonc");

            if (!File.Exists(path))
            {
                Log.LogWarning("config.jsonc 가 없다. 기본값으로 돈다: " + path);
                return WheelConfig.Default();
            }

            string text;
            try
            {
                text = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                Log.LogWarning("config.jsonc 를 읽지 못했다. 기본값으로 돈다: " + e.Message);
                return WheelConfig.Default();
            }

            var settings = WheelConfig.LoadFromText(text, out var warning);
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
