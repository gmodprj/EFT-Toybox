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
    /// <summary>lang/_sain.json · lang/_sain-ui.json 의 모양. 왼쪽이 원문, 오른쪽이 한글.</summary>
    internal sealed class SainStringFile
    {
        [JsonProperty("strings")] public Dictionary<string, string> Strings;
    }

    /// <summary>두 SAIN 사전을 읽어 들고 있는 곳. 파일이 없으면 그 부분은 그냥 꺼진다.</summary>
    internal static class SainDictionary
    {
        internal static Dictionary<string, string> Attributes =
            new Dictionary<string, string>(StringComparer.Ordinal);

        internal static Dictionary<string, string> UiStrings =
            new Dictionary<string, string>(StringComparer.Ordinal);

        internal static void Load(string langDir)
        {
            Attributes = Read(Path.Combine(langDir, "_sain.json"), "SAIN 설정 사전");
            UiStrings = Read(Path.Combine(langDir, "_sain-ui.json"), "SAIN 창 UI 사전");
        }

        private static Dictionary<string, string> Read(string path, string what)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            if (!File.Exists(path))
            {
                KoreanConfigPlugin.Log.LogInfo($"{what}이 없어 원문 그대로 둔다: {path}");
                return map;
            }

            try
            {
                var text = File.ReadAllText(path, Encoding.UTF8);
                var file = JsonConvert.DeserializeObject<SainStringFile>(text, new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                });

                if (file?.Strings == null) return map;

                foreach (var kv in file.Strings)
                {
                    if (string.IsNullOrEmpty(kv.Key) || string.IsNullOrEmpty(kv.Value)) continue;
                    map[kv.Key] = kv.Value;
                }
            }
            catch (Exception e)
            {
                KoreanConfigPlugin.Log.LogError($"{what}을 읽지 못했다: {path} — {e.Message}");
            }

            return map;
        }
    }

    /// <summary>
    /// SAIN 편집기의 설정 이름·설명·분류를 한글로 바꾼다.
    ///
    /// SAIN 은 각 설정의 표시 글자를 `[Name("..")]`·`[Description("..")]`·`[Category("..")]` 속성에 두고,
    /// 편집기는 그것을 모아 둔 `SAIN.Attributes.ConfigInfoClass` 의 Name·Description·Category 를 그대로
    /// 그린다(`AttributesGUI`: `SAINLayout.Label(info.Name + ": ")`, `new GUIContent(info.Name, info.Description)`).
    /// 속성의 값은 `readonly` 필드라 거기서는 손댈 수 없으므로, **그 객체가 만들어진 직후** 세 값만 바꾼다.
    /// SAIN 은 이 객체를 멤버마다 한 번 만들어 정적 사전에 쌓으므로(`AttributesGUI.AddAttributesToDictionary`)
    /// 교체도 한 번씩만 일어난다.
    ///
    /// 바꾸는 것은 화면에 그릴 글자뿐이다. 설정 값, 프리셋 파일, 멤버 이름은 건드리지 않는다 — SAIN 은
    /// 값을 필드 이름으로 저장하고(프리셋 JSON), Name 을 열쇠로 쓰는 곳은 "저장하지 않은 변경" 목록
    /// (`ConfigEditingTracker`, 메모리 안의 표시용)과 편집기 검색창뿐이다. 그래서 한글로 바꿔도
    /// 저장·불러오기에 영향이 없고, 검색은 한글로 하게 된다.
    ///
    /// SAIN 이 깔려 있지 않으면 아무 일도 하지 않는다(타입을 못 찾으면 그대로 돌아간다).
    /// </summary>
    internal static class SainAttributePatch
    {
        private const string ConfigInfoTypeName = "SAIN.Attributes.ConfigInfoClass";

        private static FieldInfo _nameField;
        private static FieldInfo _descriptionField;
        private static FieldInfo _categoryField;

        private static readonly HashSet<string> Missing = new HashSet<string>(StringComparer.Ordinal);
        // 번역을 이어 붙일 때 이 목록이 유일한 단서다. 60 으로 두었더니 2026-11-02 run 에서 딱 60줄이 차
        // 더 있는지 알 수 없었다. 로그가 길어지는 것보다 빠진 것을 다 보는 쪽이 낫다.
        private const int MissingLogLimit = 300;

        internal static int Translated { get; private set; }
        internal static int Seen { get; private set; }

        /// <summary>패치했으면 true. SAIN 이 없거나 모양이 달라지면 false 를 돌려주고 아무것도 바꾸지 않는다.</summary>
        internal static bool TryPatch(Harmony harmony)
        {
            if (SainDictionary.Attributes.Count == 0) return false;

            var type = AccessTools.TypeByName(ConfigInfoTypeName);
            if (type == null)
            {
                KoreanConfigPlugin.Log.LogInfo($"SAIN 이 없어 편집기 한글화를 건너뛴다 ({ConfigInfoTypeName} 없음)");
                return false;
            }

            _nameField = AccessTools.Field(type, "<Name>k__BackingField");
            _descriptionField = AccessTools.Field(type, "<Description>k__BackingField");
            _categoryField = AccessTools.Field(type, "<Category>k__BackingField");

            if (_nameField == null || _descriptionField == null || _categoryField == null)
            {
                KoreanConfigPlugin.Log.LogWarning(
                    "SAIN 의 ConfigInfoClass 모양이 달라져 한글화를 건너뛴다 " +
                    $"(Name {(_nameField != null ? "찾음" : "없음")}, Description {(_descriptionField != null ? "찾음" : "없음")}, Category {(_categoryField != null ? "찾음" : "없음")})");
                return false;
            }

            // 멤버 하나를 받는 생성자만 고른다. ConfigInfoClass(string) 은 이름을 그대로 쓰는 다른 길이다.
            var ctor = type.GetConstructors(AccessTools.all)
                .FirstOrDefault(c =>
                {
                    var ps = c.GetParameters();
                    return ps.Length == 1 && typeof(MemberInfo).IsAssignableFrom(ps[0].ParameterType);
                });

            if (ctor == null)
            {
                KoreanConfigPlugin.Log.LogWarning("SAIN 의 ConfigInfoClass(MemberInfo) 생성자를 찾지 못해 한글화를 건너뛴다");
                return false;
            }

            harmony.Patch(ctor, postfix: new HarmonyMethod(AccessTools.Method(typeof(SainAttributePatch), nameof(AfterCtor))));
            KoreanConfigPlugin.Log.LogInfo($"SAIN 편집기 한글화 패치 완료 — 사전 {SainDictionary.Attributes.Count}개");
            return true;
        }

        private static void AfterCtor(object __instance)
        {
            if (__instance == null) return;
            if (KoreanConfigPlugin.UseKorean != null && !KoreanConfigPlugin.UseKorean.Value) return;

            try
            {
                Seen++;
                Apply(_nameField, __instance);
                Apply(_descriptionField, __instance);
                Apply(_categoryField, __instance);
            }
            catch (Exception e)
            {
                // 한 항목이 터져도 편집기 전체를 막지 않는다.
                KoreanConfigPlugin.Log.LogError("SAIN 설정 한글화 중 오류 — 그 항목은 원문으로 둔다: " + e.Message);
            }
        }

        private static void Apply(FieldInfo field, object instance)
        {
            if (!(field.GetValue(instance) is string value) || value.Length == 0) return;

            if (SainDictionary.Attributes.TryGetValue(value, out var korean))
            {
                field.SetValue(instance, korean);
                Translated++;
                return;
            }

            // 사전에 없는 글자는 그대로 둔다. 번역을 이어 붙일 때 무엇이 남았는지 알 수 있게 한 번만 찍는다.
            if (Missing.Add(value) && Missing.Count <= MissingLogLimit)
            {
                KoreanConfigPlugin.Log.LogInfo($"  SAIN 사전에 없음: \"{value.Replace("\n", "\\n")}\"");
            }
        }

        internal static void LogSummary()
        {
            KoreanConfigPlugin.Log.LogInfo(
                $"SAIN 편집기 한글화 — 글자 {Translated}개 교체, 설정 {Seen}개 거침, 사전에 없는 글자 {Missing.Count}종");
        }
    }

    /// <summary>
    /// SAIN 어셈블리 안에 코드로 박혀 있는 글자(창 제목·버튼·안내 문구)를 한글로 바꾼다.
    /// 속성으로 선언된 설정 이름과 달리 이 글자들은 메서드 본문의 문자열 상수(ldstr)라 표시용 필드가 없다.
    /// `UiStringPatch` 가 ConfigurationManager 에 하는 것과 같은 방법으로, 그 상수를 불러올 때 한글 문자열로
    /// 바꿔치기한다. DLL 파일은 건드리지 않으므로 이 플러그인만 빼면 원문으로 돌아간다.
    /// </summary>
    [HarmonyPatch]
    internal static class SainUiStringPatch
    {
        private static readonly HashSet<string> Used = new HashSet<string>(StringComparer.Ordinal);

        internal static int PatchedMethods { get; private set; }
        internal static int ReplacedLiterals { get; private set; }

        /// <summary>SAIN 의 어셈블리. 둘 다 사전의 글자를 들고 있을 수 있다.</summary>
        private static readonly string[] AssemblyNames = { "SAIN", "SAIN.Preset.Shared" };

        internal static IEnumerable<MethodBase> TargetMethods()
        {
            var targets = new List<MethodBase>();
            if (SainDictionary.UiStrings.Count == 0) return targets;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name;
                try { name = assembly.GetName().Name; }
                catch { continue; }

                if (Array.IndexOf(AssemblyNames, name) < 0) continue;

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
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
            }

            PatchedMethods = targets.Count;
            return targets;
        }

        private static bool UsesTranslatedString(MethodBase method)
        {
            try
            {
                foreach (var pair in PatchProcessor.ReadMethodBody(method))
                    if (pair.Key == OpCodes.Ldstr && pair.Value is string s && SainDictionary.UiStrings.ContainsKey(s))
                        return true;
            }
            catch
            {
                // 본문을 읽을 수 없는 메서드는 건너뛴다.
            }
            return false;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldstr
                    && instruction.operand is string s
                    && SainDictionary.UiStrings.TryGetValue(s, out var korean))
                {
                    instruction.operand = korean;
                    ReplacedLiterals++;
                    Used.Add(s);
                }
                yield return instruction;
            }
        }

        /// <summary>한 번도 바뀌지 않은 항목을 알린다. 원문이 다르거나 더 긴 문자열의 일부일 때 이렇게 된다.</summary>
        internal static void LogUnused()
        {
            var unused = SainDictionary.UiStrings.Keys.Where(k => !Used.Contains(k)).ToList();
            if (unused.Count == 0)
            {
                KoreanConfigPlugin.Log.LogInfo("  SAIN 창 UI 사전 항목 전부 사용됨");
                return;
            }

            KoreanConfigPlugin.Log.LogWarning($"  SAIN 창 UI 사전 {unused.Count}개 항목이 쓰이지 않았다 (원문이 다르거나 더 긴 문자열의 일부)");
            foreach (var k in unused)
                KoreanConfigPlugin.Log.LogWarning($"    안 쓰임: \"{k.Replace("\n", "\\n")}\"");
        }
    }
}
