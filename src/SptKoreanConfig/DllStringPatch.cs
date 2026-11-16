using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using Newtonsoft.Json;

namespace SptKoreanConfig
{
    /// <summary>lang/_dll-*.json 하나의 모양.</summary>
    internal sealed class DllStringFile
    {
        /// <summary>바꿀 어셈블리의 짧은 이름(확장자 없이). 예: "BetterBepInEx"</summary>
        [JsonProperty("assembly")] public string Assembly;

        /// <summary>
        /// 바꿀 메서드. "네임스페이스.타입::메서드". 중첩 타입은 '/' 나 '+' 로 잇고, '*' 는 아무 글자.
        /// 이 목록에 든 메서드 안의 리터럴만 바꾼다 — 같은 글자를 열쇠로 쓰는 다른 메서드는 그대로 둔다.
        /// </summary>
        [JsonProperty("methods")] public List<string> Methods;

        /// <summary>원문 → 한글. 원문은 DLL 안의 글자와 앞뒤 공백까지 같아야 한다.</summary>
        [JsonProperty("strings")] public Dictionary<string, string> Strings;

        /// <summary>
        /// 메서드 패턴 → (원문 → 한글). 그 메서드에서만 <see cref="Strings"/> 보다 먼저 쓴다.
        /// "Reset " + 수 + " settings" 처럼 이어 붙이는 글은 한국어 어순이 달라 메서드마다 다르게 옮겨야 할 때 쓴다.
        /// 여기 적은 패턴도 바꿀 메서드로 친다.
        /// </summary>
        [JsonProperty("methodStrings")] public Dictionary<string, Dictionary<string, string>> MethodStrings;

        /// <summary>패턴에 들어도 건드리지 않을 메서드(같은 꼴). 다시 컴파일하지 못하는 메서드를 뺄 때 쓴다.</summary>
        [JsonProperty("excludeMethods")] public List<string> ExcludeMethods;
    }

    /// <summary>
    /// 다른 모드의 창 안에 박힌 글자(버튼·제목·안내문)를 바꾼다.
    ///
    /// <see cref="UiStringPatch"/>·<see cref="SainUiStringPatch"/> 와 같은 방식(문자열 상수 ldstr 를 바꿔치기)이지만,
    /// 그 둘은 어셈블리 전체에서 같은 글자를 모두 바꾸므로 "Name"·"Scale" 처럼 짧은 낱말은 넣을 수 없었다 —
    /// 그런 낱말은 화면 밖에서 JSON 열쇠나 파일 이름으로도 쓰인다. 여기서는 사전마다 **바꿀 메서드를 적고 그 안의 리터럴만** 바꾼다.
    ///
    /// 트랜스파일러라 패치할 때 IL 을 한 번 고칠 뿐 매 프레임 비용은 없다.
    /// DLL 파일은 건드리지 않으므로 사전을 빼면 다음 실행에서 원문으로 돌아간다.
    /// </summary>
    internal static class DllStringPatch
    {
        private sealed class Loaded
        {
            public string File;
            public DllStringFile Data;
            public List<Regex> Patterns;
            public List<KeyValuePair<Regex, string>> OverridePatterns; // 정규식, 원래 패턴 글자
            public List<Regex> Excludes;
            /// <summary>쓰인 항목. 공통 항목은 원문 그대로, 메서드별 항목은 "패턴 ‖ 원문".</summary>
            public readonly HashSet<string> Used = new HashSet<string>(StringComparer.Ordinal);
            public int Methods;

            public IEnumerable<string> AllEntryIds()
            {
                foreach (var k in Data.Strings.Keys) yield return k;
                if (Data.MethodStrings == null) yield break;
                foreach (var kv in Data.MethodStrings)
                    foreach (var k in kv.Value.Keys)
                        yield return kv.Key + " ‖ " + k;
            }
        }

        /// <summary>한 메서드 안에서 바꿀 글자 하나 — 한글과, 어느 사전의 어느 항목인지.</summary>
        private sealed class Replacement
        {
            public string Korean;
            public Loaded File;
            public string EntryId;
        }

        /// <summary>메서드 → (원문 → 바꿀 것). 여러 사전이 같은 메서드를 고르면 합친다(뒤 사전이 이긴다).</summary>
        private static readonly Dictionary<string, Dictionary<string, Replacement>> MapByMethod =
            new Dictionary<string, Dictionary<string, Replacement>>(StringComparer.Ordinal);

        private static readonly Dictionary<string, MethodBase> MethodByKey =
            new Dictionary<string, MethodBase>(StringComparer.Ordinal);

        private static readonly List<Loaded> Files = new List<Loaded>();

        /// <summary>지금 패치 중인 메서드에서 바꾼 항목. 패치가 성공해야 Used 로 옮긴다.</summary>
        private static readonly List<Replacement> Pending = new List<Replacement>();

        internal static int ReplacedLiterals { get; private set; }

        internal static void LoadAndPatch(string langDir, Harmony harmony)
        {
            if (!Directory.Exists(langDir)) return;

            foreach (var path in Directory.GetFiles(langDir, "_dll-*.json").OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
            {
                var loaded = Read(path);
                if (loaded == null) continue;
                Files.Add(loaded);
                Collect(loaded);
            }

            if (MapByMethod.Count == 0) return;

            var transpiler = new HarmonyMethod(AccessTools.Method(typeof(DllStringPatch), nameof(Transpiler)));
            foreach (var kv in MethodByKey)
            {
                Pending.Clear();
                try
                {
                    harmony.Patch(kv.Value, transpiler: transpiler);
                    // 트랜스파일러가 돌았어도 다시 컴파일에 실패하면 화면은 원문 그대로다.
                    // 패치가 끝까지 성공한 뒤에만 "쓰임"으로 확정한다(2026-11-17: DrawRow 가 실패했는데 쓰였다고 셌다).
                    foreach (var r in Pending) r.File.Used.Add(r.EntryId);
                }
                catch (Exception e)
                {
                    KoreanConfigPlugin.Log.LogWarning($"  {Describe(kv.Value)} 패치 실패 — {e.Message} (이 메서드의 글자는 원문으로 남는다. 사전의 excludeMethods 에 넣는다)");
                }
                Pending.Clear();
            }

            // 정적 생성자(.cctor)는 우리보다 먼저 올라온 모드에서 이미 돌았을 수 있다 — 그러면 IL 을 고쳐도
            // 화면에 쓰는 정적 필드(GUIContent·문자열·탭 목록)는 영문 그대로다(2026-11-17 화면 실측: Better BepInEx 의
            // "Mod Settings"·"Show advanced", SAIN 의 탭 이름). 그래서 패치 뒤에 그 필드들을 한 번 훑어 바꾼다.
            foreach (var kv in MethodByKey)
            {
                if (!(kv.Value is ConstructorInfo ctor) || !ctor.IsStatic || ctor.DeclaringType == null) continue;
                try
                {
                    FixStaticFields(ctor.DeclaringType, MapByMethod[kv.Key]);
                }
                catch (Exception e)
                {
                    KoreanConfigPlugin.Log.LogWarning($"  {Describe(ctor)} 정적 필드 바꾸기 실패 — {e.Message}");
                }
            }

            foreach (var file in Files)
                ReportFile(file);
        }

        private static readonly BindingFlags StaticFields = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        private static readonly BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// 이미 초기화된 정적 필드를 사전대로 바꾼다. 값이 이미 한글(패치된 .cctor 가 막 돌았다)이면 쓰인 것으로만 센다.
        /// 문자열·문자열 배열·GUIContent 를 바꾸고, 컬렉션이면 그 안 객체(같은 어셈블리의 클래스)의 문자열 필드를 한 단계만 바꾼다.
        /// </summary>
        private static void FixStaticFields(Type type, Dictionary<string, Replacement> map)
        {
            var byKorean = new Dictionary<string, Replacement>(StringComparer.Ordinal);
            foreach (var r in map.Values) byKorean[r.Korean] = r;
            var changed = 0;

            string Swap(string s)
            {
                if (s == null) return null;
                if (map.TryGetValue(s, out var r)) { r.File.Used.Add(r.EntryId); changed++; return r.Korean; }
                if (byKorean.TryGetValue(s, out var k)) k.File.Used.Add(k.EntryId);
                return null;
            }

            void FixGui(object gui)
            {
                foreach (var prop in new[] { "text", "tooltip" })
                {
                    var p = gui.GetType().GetProperty(prop);
                    if (p == null) continue;
                    var ko = Swap(p.GetValue(gui, null) as string);
                    if (ko != null) p.SetValue(gui, ko, null);
                }
            }

            bool IsGui(object o) => o != null && o.GetType().FullName == "UnityEngine.GUIContent";

            void FixObject(object o)
            {
                if (o == null) return;
                if (IsGui(o)) { FixGui(o); return; }
                var t = o.GetType();
                if (t.Assembly != type.Assembly || !t.IsClass) return;
                foreach (var f in t.GetFields(InstanceFields))
                {
                    if (f.FieldType != typeof(string)) continue;
                    var ko = Swap(f.GetValue(o) as string);
                    if (ko != null) f.SetValue(o, ko);
                }
            }

            foreach (var f in type.GetFields(StaticFields))
            {
                if (f.IsLiteral) continue;
                object v;
                try { v = f.GetValue(null); } catch { continue; }
                if (v == null) continue;

                if (v is string s)
                {
                    var ko = Swap(s);
                    if (ko != null) { try { f.SetValue(null, ko); } catch { /* 런타임이 readonly 정적 필드 쓰기를 막으면 원문으로 둔다 */ } }
                }
                else if (v is string[] arr)
                {
                    for (var i = 0; i < arr.Length; i++) { var ko = Swap(arr[i]); if (ko != null) arr[i] = ko; }
                }
                else if (IsGui(v)) FixGui(v);
                else if (v is System.Collections.IDictionary d) { foreach (var item in d.Values) FixObject(item); }
                else if (v is System.Collections.IEnumerable e) { foreach (var item in e) FixObject(item); }
            }

            if (changed > 0)
                KoreanConfigPlugin.Log.LogInfo($"  {type.FullName}: 이미 초기화된 정적 필드의 글자 {changed}개를 바꿨다");
        }

        private static Loaded Read(string path)
        {
            var name = Path.GetFileName(path);
            try
            {
                var data = JsonConvert.DeserializeObject<DllStringFile>(File.ReadAllText(path, Encoding.UTF8),
                    new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });

                if (data == null || string.IsNullOrEmpty(data.Assembly) || data.Methods == null || data.Methods.Count == 0)
                {
                    KoreanConfigPlugin.Log.LogWarning($"{name}: assembly·methods 가 비어 있어 건너뛴다");
                    return null;
                }

                // 빈 값은 원문 유지로 친다.
                data.Strings = Clean(data.Strings);
                if (data.MethodStrings != null)
                    data.MethodStrings = data.MethodStrings.ToDictionary(kv => kv.Key, kv => Clean(kv.Value), StringComparer.Ordinal);

                return new Loaded
                {
                    File = name,
                    Data = data,
                    Patterns = data.Methods.Select(ToRegex).ToList(),
                    OverridePatterns = (data.MethodStrings ?? new Dictionary<string, Dictionary<string, string>>())
                        .Keys.Select(p => new KeyValuePair<Regex, string>(ToRegex(p), p)).ToList(),
                    Excludes = (data.ExcludeMethods ?? new List<string>()).Select(ToRegex).ToList(),
                };
            }
            catch (Exception e)
            {
                KoreanConfigPlugin.Log.LogError($"{name} 을 읽지 못했다 — {e.Message}");
                return null;
            }
        }

        private static Dictionary<string, string> Clean(Dictionary<string, string> map)
        {
            if (map == null) return new Dictionary<string, string>(StringComparer.Ordinal);
            return map.Where(kv => !string.IsNullOrEmpty(kv.Key) && !string.IsNullOrEmpty(kv.Value))
                .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);
        }

        /// <summary>"Ns.Type/Nested::Draw*" → 정규식. 중첩 구분은 '/'·'+' 둘 다 받는다.</summary>
        private static Regex ToRegex(string pattern)
        {
            var p = pattern.Replace('/', '+');
            var escaped = Regex.Escape(p).Replace("\\*", ".*");
            return new Regex("^" + escaped + "$", RegexOptions.CultureInvariant);
        }

        private static string Key(MethodBase m) => (m.DeclaringType?.FullName ?? "?") + "::" + m.Name + "#" + m.MetadataToken;

        private static string Describe(MethodBase m) => (m.DeclaringType?.FullName ?? "?") + "::" + m.Name;

        /// <summary>어셈블리를 훑어 패턴에 맞고 사전의 글자를 실제로 쓰는 메서드만 모은다.</summary>
        private static void Collect(Loaded file)
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a =>
            {
                try { return string.Equals(a.GetName().Name, file.Data.Assembly, StringComparison.OrdinalIgnoreCase); }
                catch { return false; }
            });
            if (asm == null)
            {
                KoreanConfigPlugin.Log.LogInfo($"{file.File}: {file.Data.Assembly} 가 없어 건너뛴다");
                return;
            }

            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }

            foreach (var type in types)
            {
                if (type.IsInterface) continue;
                IEnumerable<MethodBase> members;
                try
                {
                    members = type.GetMethods(AccessTools.all).Cast<MethodBase>().Concat(type.GetConstructors(AccessTools.all));
                }
                catch { continue; }

                foreach (var m in members)
                {
                    if (m.DeclaringType != type || m.IsAbstract || m.ContainsGenericParameters) continue;
                    var name = Describe(m);
                    if (file.Excludes.Any(r => r.IsMatch(name))) continue;

                    // 이 메서드에서 쓸 사전: 공통 항목 위에 맞는 메서드별 항목을 얹는다.
                    var effective = new Dictionary<string, Replacement>(StringComparer.Ordinal);
                    if (file.Patterns.Any(r => r.IsMatch(name)))
                        foreach (var kv in file.Data.Strings)
                            effective[kv.Key] = new Replacement { Korean = kv.Value, File = file, EntryId = kv.Key };
                    foreach (var op in file.OverridePatterns)
                    {
                        if (!op.Key.IsMatch(name)) continue;
                        foreach (var kv in file.Data.MethodStrings[op.Value])
                            effective[kv.Key] = new Replacement { Korean = kv.Value, File = file, EntryId = op.Value + " ‖ " + kv.Key };
                    }
                    if (effective.Count == 0 || !UsesAny(m, effective)) continue;

                    var key = Key(m);
                    if (!MapByMethod.TryGetValue(key, out var map))
                    {
                        map = new Dictionary<string, Replacement>(StringComparer.Ordinal);
                        MapByMethod[key] = map;
                        MethodByKey[key] = m;
                    }
                    foreach (var kv in effective) map[kv.Key] = kv.Value;
                    file.Methods++;
                }
            }
        }

        private static bool UsesAny(MethodBase method, Dictionary<string, Replacement> strings)
        {
            try
            {
                foreach (var pair in PatchProcessor.ReadMethodBody(method))
                    if (pair.Key == OpCodes.Ldstr && pair.Value is string s && strings.ContainsKey(s))
                        return true;
            }
            catch
            {
                // 본문을 읽을 수 없는 메서드는 건너뛴다.
            }
            return false;
        }

        /// <summary>
        /// 다른 모드가 같은 메서드를 나중에 패치하면 Harmony 가 트랜스파일러를 다시 돌린다.
        /// 그래서 "지금 패치 중인 사전" 같은 정적 상태를 쓰지 않고, 원본 메서드로 사전을 찾는다.
        /// </summary>
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            MapByMethod.TryGetValue(Key(original), out var map);

            foreach (var instruction in instructions)
            {
                if (map != null
                    && instruction.opcode == OpCodes.Ldstr
                    && instruction.operand is string s
                    && map.TryGetValue(s, out var r))
                {
                    instruction.operand = r.Korean;
                    ReplacedLiterals++;
                    // 정적 생성자는 이미 돌았을 수 있어 IL 을 고쳤다고 화면이 바뀌지 않는다 — FixStaticFields 가 센다.
                    if (!(original is ConstructorInfo c && c.IsStatic)) Pending.Add(r);
                }
                yield return instruction;
            }
        }

        private static void ReportFile(Loaded file)
        {
            var ids = file.AllEntryIds().ToList();
            var unused = ids.Where(k => !file.Used.Contains(k)).ToList();
            KoreanConfigPlugin.Log.LogInfo($"{file.File} — {file.Data.Assembly}: 사전 {ids.Count}개, 메서드 {file.Methods}개, 쓰인 항목 {ids.Count - unused.Count}개");
            if (unused.Count == 0)
            {
                KoreanConfigPlugin.Log.LogInfo($"  {file.File} 항목 전부 사용됨");
                return;
            }
            KoreanConfigPlugin.Log.LogWarning($"  {file.File}: {unused.Count}개 항목이 쓰이지 않았다 (원문이 다르거나 고른 메서드 밖에 있다)");
            foreach (var k in unused)
                KoreanConfigPlugin.Log.LogWarning($"    안 쓰임: \"{k.Replace("\n", "\\n")}\"");
        }
    }
}
