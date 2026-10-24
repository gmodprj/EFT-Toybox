using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ConfigurationManager;
using HarmonyLib;

namespace SptKoreanConfig
{
    /// <summary>
    /// F12(ConfigurationManager) 창에 보이는 글자만 한글로 바꾼다.
    /// 다른 모드의 파일은 건드리지 않고, 설정 값과 .cfg 파일도 그대로 둔다.
    /// </summary>
    [BepInPlugin(PluginGuid, "F12 설정 한글화", "0.1.0")]
    [BepInDependency(CmGuid, BepInDependency.DependencyFlags.HardDependency)]
    public class KoreanConfigPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "spt.koreanconfig";
        public const string CmGuid = "com.bepis.bepinex.configurationmanager";

        internal static ManualLogSource Log;
        internal static TranslationStore Store;
        internal static ConfigEntry<bool> UseKorean;
        internal static ConfigEntry<bool> MarkUntranslated;

        private void Awake()
        {
            Log = Logger;

            UseKorean = Config.Bind("일반", "한글로 표시", true,
                "F12 창의 모드 이름·설정 이름·설명을 한글로 바꾼다. 끄면 원래 영문으로 돌아간다.");
            MarkUntranslated = Config.Bind("일반", "번역 안 된 항목 표시", false,
                "사전에 아직 없는 항목 앞에 · 를 붙인다. 번역 작업할 때만 켠다.");

            var langDir = Path.Combine(Path.GetDirectoryName(Info.Location) ?? ".", "lang");
            Store = new TranslationStore(langDir);
            Store.Reload();
            Log.LogInfo($"번역 사전 {Store.PluginCount}개 모드 / 섹션 {Store.SectionCount} / 항목 {Store.SettingCount} 읽음 ({langDir})");

            BuildSettingListPatch.LogReflectionTargets();

            new Harmony(PluginGuid).PatchAll(typeof(BuildSettingListPatch));
            Log.LogInfo("ConfigurationManager.BuildSettingList 패치 완료");
        }
    }

    /// <summary>
    /// CM 이 설정 목록을 만든 직후, 화면에 쓸 표시용 필드만 한글로 덮어쓴다.
    /// 목록은 F12 를 열 때마다 새로 만들어지므로 끄면 그대로 원문으로 돌아간다.
    /// </summary>
    [HarmonyPatch]
    internal static class BuildSettingListPatch
    {
        private static readonly FieldInfo AllSettingsField =
            AccessTools.Field(typeof(ConfigurationManager.ConfigurationManager), "_allSettings");

        private static readonly MethodInfo BuildFiltered =
            AccessTools.Method(typeof(ConfigurationManager.ConfigurationManager), "BuildFilteredSettingList");

        private static readonly FieldInfo DispNameField =
            AccessTools.Field(typeof(SettingEntryBase), "<DispName>k__BackingField");

        private static readonly FieldInfo CategoryField =
            AccessTools.Field(typeof(SettingEntryBase), "<Category>k__BackingField");

        private static readonly FieldInfo DescriptionField =
            AccessTools.Field(typeof(SettingEntryBase), "<Description>k__BackingField");

        private static readonly FieldInfo PluginNameField =
            AccessTools.Field(typeof(BepInPlugin), "<Name>k__BackingField");

        /// <summary>모드 이름은 BepInPlugin 객체를 직접 고치므로 끌 때 되돌리려면 원문을 들고 있어야 한다.</summary>
        private static readonly Dictionary<BepInPlugin, string> OriginalPluginNames =
            new Dictionary<BepInPlugin, string>();

        private static bool _lastAppliedKorean;

        /// <summary>리플렉션으로 집는 대상이 이 CM 버전에 실제로 있는지 시작할 때 한 번 찍어 둔다.</summary>
        internal static void LogReflectionTargets()
        {
            void Report(string what, object target) =>
                KoreanConfigPlugin.Log.LogInfo($"  {(target != null ? "찾음" : "없음 ←확인필요")} : {what}");

            KoreanConfigPlugin.Log.LogInfo("리플렉션 대상 점검");
            Report("ConfigurationManager._allSettings", AllSettingsField);
            Report("ConfigurationManager.BuildFilteredSettingList()", BuildFiltered);
            Report("SettingEntryBase.DispName", DispNameField);
            Report("SettingEntryBase.Category", CategoryField);
            Report("SettingEntryBase.Description", DescriptionField);
            Report("BepInPlugin.Name", PluginNameField);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ConfigurationManager.ConfigurationManager), "BuildSettingList")]
        private static void AfterBuildSettingList(ConfigurationManager.ConfigurationManager __instance)
        {
            try
            {
                Apply(__instance);
            }
            catch (Exception e)
            {
                KoreanConfigPlugin.Log.LogError("한글화 적용 중 오류 — 원문 그대로 둔다: " + e);
            }
        }

        private static void Apply(ConfigurationManager.ConfigurationManager cm)
        {
            var on = KoreanConfigPlugin.UseKorean.Value;

            if (!on)
            {
                if (_lastAppliedKorean) RestorePluginNames();
                _lastAppliedKorean = false;
                return;
            }

            if (KoreanConfigPlugin.Store.ReloadIfChanged())
            {
                KoreanConfigPlugin.Log.LogInfo(
                    $"번역 사전 다시 읽음 — 모드 {KoreanConfigPlugin.Store.PluginCount} / 섹션 {KoreanConfigPlugin.Store.SectionCount} / 항목 {KoreanConfigPlugin.Store.SettingCount}");
            }

            if (!(AllSettingsField.GetValue(cm) is List<SettingEntryBase> all)) return;

            var mark = KoreanConfigPlugin.MarkUntranslated.Value;
            int named = 0, sectioned = 0, missing = 0;

            foreach (var setting in all)
            {
                var guid = setting.PluginInfo?.GUID;
                var table = KoreanConfigPlugin.Store.Get(guid);

                // 모드 이름 (헤더)
                if (table != null && !string.IsNullOrEmpty(table.Name) && setting.PluginInfo != null)
                {
                    if (!OriginalPluginNames.ContainsKey(setting.PluginInfo))
                        OriginalPluginNames[setting.PluginInfo] = setting.PluginInfo.Name;
                    if (setting.PluginInfo.Name != table.Name)
                        PluginNameField.SetValue(setting.PluginInfo, table.Name);
                }

                // 원문 섹션·키는 CM 이 만든 표시용 값이 아니라 ConfigEntry 쪽에서 가져온다.
                GetSourceNames(setting, out var srcSection, out var srcKey);

                var koSection = table?.SectionOf(srcSection);
                if (koSection != null)
                {
                    CategoryField.SetValue(setting, koSection);
                    sectioned++;
                }

                var ko = table?.SettingOf(srcSection, srcKey);
                if (ko != null && !string.IsNullOrEmpty(ko.Name))
                {
                    DispNameField.SetValue(setting, ko.Name);
                    named++;
                }
                else
                {
                    missing++;
                    if (mark && !string.IsNullOrEmpty(setting.DispName) && !setting.DispName.StartsWith("·"))
                        DispNameField.SetValue(setting, "· " + setting.DispName);
                }

                if (ko != null && !string.IsNullOrEmpty(ko.Desc))
                    DescriptionField.SetValue(setting, ko.Desc);
            }

            _lastAppliedKorean = true;
            KoreanConfigPlugin.Log.LogInfo($"한글 적용 — 항목 {named}/{all.Count}, 섹션 {sectioned}, 사전에 없음 {missing}");

            // 섹션 묶음은 Category 로 다시 만들어야 한글 제목으로 묶인다.
            BuildFiltered?.Invoke(cm, null);
        }

        /// <summary>표시용 이름이 아니라 .cfg 에 적히는 원문 섹션/키를 꺼낸다.</summary>
        private static void GetSourceNames(SettingEntryBase setting, out string section, out string key)
        {
            section = null;
            key = null;

            var entryProp = AccessTools.Property(setting.GetType(), "Entry");
            if (entryProp?.GetValue(setting, null) is ConfigEntryBase entry)
            {
                section = entry.Definition.Section;
                key = entry.Definition.Key;
                return;
            }

            // ConfigEntry 가 아닌 항목(속성 기반 설정)은 표시 이름을 그대로 열쇠로 쓴다.
            section = setting.Category;
            key = setting.DispName;
        }

        private static void RestorePluginNames()
        {
            foreach (var kv in OriginalPluginNames)
                PluginNameField.SetValue(kv.Key, kv.Value);
            OriginalPluginNames.Clear();
            KoreanConfigPlugin.Log.LogInfo("한글 표시를 끄고 모드 이름을 원문으로 되돌렸다");
        }
    }
}
