using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using Newtonsoft.Json;

namespace SptKoreanConfig
{
    /// <summary>lang/_ui.json 의 모양.</summary>
    internal sealed class UiStringFile
    {
        [JsonProperty("strings")] public Dictionary<string, string> Strings;
    }

    /// <summary>
    /// 창 자체의 글자(창 제목·버튼·안내 문구)를 한글로 바꾼다.
    /// 이 글자들은 ConfigurationManager.dll 안에 그대로 박혀 있어 표시용 필드로는 손댈 수 없다.
    /// 그래서 DLL 파일을 고치는 대신, 해당 메서드의 문자열 상수(ldstr)를 불러올 때
    /// 한글 문자열로 바꿔치기한다. 파일은 건드리지 않으므로 플러그인만 빼면 원문으로 돌아간다.
    /// </summary>
    [HarmonyPatch]
    internal static class UiStringPatch
    {
        private static Dictionary<string, string> _map = new Dictionary<string, string>(StringComparer.Ordinal);

        private static readonly HashSet<string> Used = new HashSet<string>(StringComparer.Ordinal);

        internal static int Loaded => _map.Count;
        internal static int PatchedMethods { get; private set; }
        internal static int ReplacedLiterals { get; private set; }

        /// <summary>
        /// 한 번도 바뀌지 않은 항목을 알린다. DLL 안의 문자열과 글자가 다르거나,
        /// 더 긴 문자열의 일부일 때 이렇게 된다(문자열 상수는 통째로만 바꿀 수 있다).
        /// </summary>
        internal static void LogUnused()
        {
            var unused = _map.Keys.Where(k => !Used.Contains(k)).ToList();
            if (unused.Count == 0)
            {
                KoreanConfigPlugin.Log.LogInfo("  창 UI 사전 항목 전부 사용됨");
                return;
            }

            KoreanConfigPlugin.Log.LogWarning($"  창 UI 사전 {unused.Count}개 항목이 쓰이지 않았다 (원문이 다르거나 더 긴 문자열의 일부)");
            foreach (var k in unused)
                KoreanConfigPlugin.Log.LogWarning($"    안 쓰임: \"{k.Replace("\n", "\\n")}\"");
        }

        /// <summary>바꾸면 설정 자체가 어긋나는 문자열. 사전에 들어 있어도 건너뛴다.</summary>
        private static readonly HashSet<string> Forbidden = new HashSet<string>(StringComparer.Ordinal)
        {
            // ConfigurationManagerAttributes 덕 타이핑에 쓰이는 필드 이름
            "ConfigurationManagerAttributes", "DispName", "Category", "Browsable", "ReadOnly",
            "AcceptableValues", "Order", "IsAdvanced", "HideDefaultButton", "HideSettingName",
            // CM 자신의 설정 섹션·키 이름 (바꾸면 .cfg 의 키가 달라진다)
            "General", "Filtering", "Advanced", "Hidden", "Unbrowsable",
            "Show advanced", "Show keybinds", "Show settings", "Show config manager",
            "Hide single sections", "Plugin collapsed default",
            // 유니티 메서드 이름
            "Update", "LateUpdate", "FixedUpdate", "OnGUI",
        };

        internal static void Load(string path)
        {
            _map = new Dictionary<string, string>(StringComparer.Ordinal);
            if (!File.Exists(path))
            {
                KoreanConfigPlugin.Log.LogInfo($"창 UI 사전이 없어 원문 그대로 둔다: {path}");
                return;
            }

            try
            {
                var text = File.ReadAllText(path, Encoding.UTF8);
                var file = JsonConvert.DeserializeObject<UiStringFile>(text, new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                });

                if (file?.Strings == null) return;

                foreach (var kv in file.Strings)
                {
                    if (string.IsNullOrEmpty(kv.Key) || string.IsNullOrEmpty(kv.Value)) continue;
                    if (Forbidden.Contains(kv.Key))
                    {
                        KoreanConfigPlugin.Log.LogWarning($"설정이 어긋날 수 있어 건너뛴다: \"{kv.Key}\"");
                        continue;
                    }
                    _map[kv.Key] = kv.Value;
                }
            }
            catch (Exception e)
            {
                KoreanConfigPlugin.Log.LogError($"창 UI 사전을 읽지 못했다: {path} — {e.Message}");
            }
        }

        /// <summary>사전에 있는 문자열을 실제로 쓰는 메서드만 골라 패치한다.</summary>
        internal static IEnumerable<MethodBase> TargetMethods()
        {
            var targets = new List<MethodBase>();
            if (_map.Count == 0) return targets;

            Type[] types;
            try
            {
                types = typeof(ConfigurationManager.ConfigurationManager).Assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null).ToArray();
            }

            foreach (var type in types)
            {
                if (type.IsInterface) continue;

                IEnumerable<MethodBase> members;
                try
                {
                    members = type.GetMethods(AccessTools.all).Cast<MethodBase>()
                        .Concat(type.GetConstructors(AccessTools.all));
                }
                catch
                {
                    continue;
                }

                foreach (var m in members)
                {
                    if (m.DeclaringType != type) continue;
                    if (m.IsAbstract || m.ContainsGenericParameters) continue;
                    if (UsesTranslatedString(m)) targets.Add(m);
                }
            }

            PatchedMethods = targets.Count;
            return targets;
        }

        private static bool UsesTranslatedString(MethodBase method)
        {
            try
            {
                foreach (var pair in PatchProcessor.ReadMethodBody(method))
                    if (pair.Key == OpCodes.Ldstr && pair.Value is string s && _map.ContainsKey(s))
                        return true;
            }
            catch
            {
                // 본문을 읽을 수 없는 메서드(외부 구현 등)는 그냥 건너뛴다.
            }
            return false;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldstr
                    && instruction.operand is string s
                    && _map.TryGetValue(s, out var korean))
                {
                    instruction.operand = korean;
                    ReplacedLiterals++;
                    Used.Add(s);
                }
                yield return instruction;
            }
        }
    }
}
