using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConfigurationManager;
using HarmonyLib;

namespace SptKoreanConfig
{
    /// <summary>
    /// Better BepInEx(3207)가 F12 창을 자기 메뉴로 갈아끼우면 CM 의 목록 생성이 돌지 않아
    /// 한글화가 통째로 비껴간다. 그 모드가 설정을 모으는 지점에 같은 번역을 다시 건다.
    ///
    /// 그 모드의 내부 이름에 기대지 않으려고 두 단계로 한다.
    ///  1) 이름이 그럴듯한 메서드(CollectSettings·BuildSettingList…)에 후처리를 건다.
    ///  2) 후처리에서 그 타입의 필드를 훑어 <see cref="SettingEntryBase"/> 목록을 찾아 번역한다.
    /// 못 찾으면 후보 필드를 로그에 적어 다음에 고칠 단서를 남긴다(조용히 실패하지 않는다).
    /// </summary>
    internal static class BetterBepInExPatch
    {
        internal const string Guid = "com.mybutthasarash.betterbepinex";

        /// <summary>
        /// 이 이름을 가진 메서드 뒤에 번역을 건다.
        /// **그리는 메서드에는 걸지 않는다** — 2026-11-05 에 <c>MenuWindow.BuildingList</c> 에도 걸었다가
        /// 메뉴가 열려 있는 동안 매 프레임 돌아(321회) 프레임이 뚝 떨어졌다.
        /// 설정을 모으는 지점에만 건다.
        /// </summary>
        private static readonly string[] HookNames =
        {
            "CollectSettings", "BuildSettingList",
        };

        private static bool _reportedMiss;
        private static int _applied;

        internal static void TryPatch(Harmony harmony)
        {
            var asm = FindAssembly();
            if (asm == null)
            {
                KoreanConfigPlugin.Log.LogInfo("Better BepInEx 가 없다 — 새 메뉴 한글화는 건너뛴다");
                return;
            }

            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }

            // void 메서드에 __result 를 요구하면 Harmony 가 "Cannot get result from void method" 로 거절한다.
            // 반환형을 보고 둘 중 맞는 것을 쓴다.
            var withResult = new HarmonyMethod(AccessTools.Method(typeof(BetterBepInExPatch), nameof(AfterCollect)));
            var voidSafe = new HarmonyMethod(AccessTools.Method(typeof(BetterBepInExPatch), nameof(AfterCollectVoid)));
            var hooked = 0;

            foreach (var type in types)
            {
                MethodInfo[] methods;
                try { methods = type.GetMethods(AccessTools.all); }
                catch { continue; }

                foreach (var m in methods)
                {
                    if (m.DeclaringType != type) continue;
                    if (m.IsAbstract || m.ContainsGenericParameters) continue;
                    if (!HookNames.Contains(m.Name)) continue;

                    try
                    {
                        harmony.Patch(m, postfix: m.ReturnType == typeof(void) ? voidSafe : withResult);
                        hooked++;
                        KoreanConfigPlugin.Log.LogInfo($"  새 메뉴 후처리 걸었다: {type.FullName}.{m.Name}");
                    }
                    catch (Exception e)
                    {
                        KoreanConfigPlugin.Log.LogWarning($"  새 메뉴 후처리 실패: {type.FullName}.{m.Name} — {e.Message}");
                    }
                }
            }

            if (hooked == 0)
            {
                KoreanConfigPlugin.Log.LogWarning(
                    "Better BepInEx 에서 걸 만한 메서드를 찾지 못했다. 그 어셈블리의 메서드 이름을 적어 둔다:");
                foreach (var t in types.Take(40))
                {
                    MethodInfo[] ms;
                    try { ms = t.GetMethods(AccessTools.all).Where(x => x.DeclaringType == t).ToArray(); }
                    catch { continue; }
                    var names = ms.Select(x => x.Name).Where(n => !n.StartsWith("get_") && !n.StartsWith("set_")).Distinct().Take(12);
                    var joined = string.Join(", ", names);
                    if (!string.IsNullOrEmpty(joined))
                        KoreanConfigPlugin.Log.LogWarning($"    {t.FullName}: {joined}");
                }
                return;
            }

            KoreanConfigPlugin.Log.LogInfo($"새 메뉴(Better BepInEx) 한글화 — 메서드 {hooked}개에 후처리를 걸었다");
        }

        private static Assembly FindAssembly()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a =>
                {
                    try { return a.GetName().Name.IndexOf("BetterBepInEx", StringComparison.OrdinalIgnoreCase) >= 0; }
                    catch { return false; }
                });
        }

        /// <summary>void 메서드용. 반환값이 없으니 인스턴스 쪽만 훑는다.</summary>
        private static void AfterCollectVoid(object __instance, MethodBase __originalMethod)
            => Run(__instance, __originalMethod, null);

        /// <summary>값을 돌려주는 메서드용. 반환값부터 본다.</summary>
        private static void AfterCollect(object __instance, MethodBase __originalMethod, object __result)
            => Run(__instance, __originalMethod, __result);

        /// <summary>어느 메서드든 끝난 뒤, 그 타입이 들고 있는 설정 목록을 찾아 번역한다.</summary>
        private static void Run(object __instance, MethodBase __originalMethod, object __result)
        {
            try
            {
                if (!KoreanConfigPlugin.UseKorean.Value) return;

                var entries = new HashSet<SettingEntryBase>();
                Harvest(__result, entries);
                if (entries.Count == 0) HarvestFields(__instance, __originalMethod?.DeclaringType, entries);

                if (entries.Count == 0)
                {
                    ReportMiss(__originalMethod);
                    return;
                }

                BuildSettingListPatch.ApplyToEntries(entries, "새 메뉴");
                _applied++;
            }
            catch (Exception e)
            {
                KoreanConfigPlugin.Log.LogError("새 메뉴 한글화 중 오류 — 원문 그대로 둔다: " + e.Message);
            }
        }

        /// <summary>
        /// 값 하나에서 SettingEntryBase 묶음을 꺼내 본다.
        /// (아래 주석의 수치는 2026-11-05 측정분이다)
        /// 같은 항목이 여러 필드에 걸쳐 있어 **중복으로 모이므로** 참조로 한 번씩만 담는다
        /// (2026-11-05: 3,350개가 7,594개로 불어나 있었다).
        /// </summary>
        private static void Harvest(object value, HashSet<SettingEntryBase> into, int depth = 0)
        {
            if (value == null || depth > 2) return;
            if (value is SettingEntryBase one) { into.Add(one); return; }
            if (!(value is IEnumerable seq) || value is string) return;

            foreach (var item in seq)
            {
                if (item is SettingEntryBase e) { into.Add(e); continue; }
                // 모드별로 묶여 있는 경우(모드 → 설정 목록)도 한 겹 더 들어가 본다.
                if (item != null) HarvestFields(item, item.GetType(), into, depth + 1);
            }
        }

        private static void HarvestFields(object instance, Type type, HashSet<SettingEntryBase> into, int depth = 0)
        {
            if (type == null || depth > 2) return;

            foreach (var f in type.GetFields(AccessTools.all))
            {
                if (!f.IsStatic && instance == null) continue;
                object value;
                try { value = f.GetValue(f.IsStatic ? null : instance); }
                catch { continue; }
                Harvest(value, into, depth);
            }
        }

        private static void ReportMiss(MethodBase origin)
        {
            if (_reportedMiss) return;
            _reportedMiss = true;

            var type = origin?.DeclaringType;
            KoreanConfigPlugin.Log.LogWarning(
                $"새 메뉴에서 설정 목록을 찾지 못했다 ({type?.FullName}.{origin?.Name}). 그 타입의 필드를 적어 둔다:");
            if (type == null) return;
            foreach (var f in type.GetFields(AccessTools.all).Take(30))
                KoreanConfigPlugin.Log.LogWarning($"    {f.FieldType.Name} {f.Name}");
        }
    }
}
