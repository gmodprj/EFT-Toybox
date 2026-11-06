using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace pitTeam.Patches
{
    /// <summary>
    /// <c>AICoreAgent&lt;BotLogicDecision&gt;.Update</c> 에서 터진 예외를 적는다.
    /// **고치지 않는다 — 적기만 한다.** 예외는 그대로 되던진다.
    ///
    /// 2026-11-05: 이 자리에서 `InvalidCastException` 이 한 기동에 2,262건 나왔다.
    /// 전에는 한 건마다 두 줄을 찍어 게임 로그가 4,524줄로 불었고, 매 프레임 찍는
    /// 비용 자체가 프레임을 깎았다. 그래서 둘을 고친다.
    ///
    ///  1) **같은 종류는 한 번만 자세히 적는다.** 그다음부터는 세기만 하고,
    ///     일정 시간마다 집계 한 줄을 남긴다.
    ///  2) **처음 한 번은 주변 상태를 함께 적는다** — 던진 쪽이 BigBrain 의
    ///     `BotAgentUpdatePatch.PatchPrefix` 안에서 부르는 `_strategy.Update(...)`,
    ///     즉 게임 자신의 brain 코드이므로, 어느 봇·어느 레이어에서 났는지를
    ///     알아야 원인을 가릴 수 있다.
    ///
    /// 진단이 끝나면 이 클래스는 다시 단순해져도 된다.
    /// </summary>
    internal class AICoreAgentUpdatePatch : ModulePatch
    {
        /// <summary>같은 종류(예외 타입 + 첫 스택 줄)마다 센다.</summary>
        private static readonly Dictionary<string, int> Counts = new Dictionary<string, int>();

        /// <summary>집계 줄을 남긴 마지막 시각(초). 너무 자주 적지 않는다.</summary>
        private static double _lastRollupAt;

        /// <summary>집계 간격(초).</summary>
        private const double RollupEverySeconds = 30.0;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(AICoreAgent<BotLogicDecision>), "Update");
        }

        [PatchFinalizer]
        private static Exception PatchFinalizer(Exception __exception, AICoreAgent<BotLogicDecision> __instance)
        {
            if (__exception == null) return null;

            // 적는 일이 게임을 깨뜨리지 않게 전부 감싼다.
            try
            {
                Report(__exception, __instance);
            }
            catch
            {
                // 진단이 실패해도 원래 예외는 그대로 간다.
            }

            return __exception;
        }

        private static void Report(Exception ex, AICoreAgent<BotLogicDecision> agent)
        {
            string key = Signature(ex);

            int seen;
            Counts.TryGetValue(key, out seen);
            Counts[key] = seen + 1;

            if (seen == 0)
            {
                // 처음 보는 종류 — 자세히 적는다.
                Modules.Logger.LogError($"AICoreAgent.Update 예외 (처음) — {key}");
                Modules.Logger.LogError(ex);
                Modules.Logger.LogError("  그때의 상태: " + Describe(agent));
                return;
            }

            // 이미 본 종류 — 집계만 한다.
            double now = UnityEngine.Time.unscaledTimeAsDouble;
            if (now - _lastRollupAt < RollupEverySeconds) return;
            _lastRollupAt = now;

            var parts = Counts.OrderByDescending(p => p.Value).Select(p => $"{p.Key} ×{p.Value}");
            Modules.Logger.LogError("AICoreAgent.Update 예외 집계 — " + string.Join(" | ", parts));
        }

        /// <summary>예외 타입 + 스택 첫 줄. 같은 자리에서 나는 것을 한 종류로 묶는다.</summary>
        private static string Signature(Exception ex)
        {
            string firstFrame = (ex.StackTrace ?? string.Empty)
                .Split('\n')
                .Select(l => l.Trim())
                .FirstOrDefault(l => l.Length > 0) ?? "(스택 없음)";

            // 긴 제네릭 이름은 잘라 둔다 — 묶는 데에는 앞쪽이면 충분하다.
            if (firstFrame.Length > 160) firstFrame = firstFrame.Substring(0, 160);
            return ex.GetType().Name + " @ " + firstFrame;
        }

        /// <summary>
        /// 터진 자리의 주변을 읽는다. 어느 봇인지, 어떤 레이어가 붙어 있는지가 핵심이다.
        /// 읽기만 하고, 하나라도 실패하면 그 항목만 비운다.
        /// </summary>
        private static string Describe(AICoreAgent<BotLogicDecision> agent)
        {
            if (agent == null) return "agent=null";

            var sb = new StringBuilder();
            sb.Append("agent#").Append(agent.GetHashCode());

            Try(sb, "UsingLayer", () => Quote(agent.UsingLayer));

            object strategy = FieldOf(agent, "_strategy");
            Try(sb, "strategy", () => strategy == null ? "null" : strategy.GetType().FullName);

            object lastActive = FieldOf(agent, "_lastActiveLayer");
            Try(sb, "_lastActiveLayer", () => lastActive == null ? "null" : lastActive.GetType().FullName);

            if (strategy != null)
            {
                Try(sb, "aICoreLayer", () =>
                {
                    object layer = FieldOf(strategy, "aICoreLayer");
                    return layer == null ? "null" : layer.GetType().FullName;
                });

                Try(sb, "_layers", () => LayerNames(FieldOf(strategy, "_layers")));
                Try(sb, "_activeLayers", () => LayerNames(FieldOf(strategy, "_activeLayers")));
                Try(sb, "bot", () => BotNameNear(strategy));
            }

            return sb.ToString();
        }

        private static void Try(StringBuilder sb, string label, Func<string> read)
        {
            string value;
            try { value = read() ?? "null"; }
            catch (Exception e) { value = "(읽기 실패: " + e.GetType().Name + ")"; }
            sb.Append(' ').Append(label).Append('=').Append(value);
        }

        private static object FieldOf(object instance, string name)
        {
            if (instance == null) return null;
            // 선언 타입을 거슬러 올라가며 찾는다 — 제네릭 기반 클래스에 있을 수 있다.
            for (Type t = instance.GetType(); t != null; t = t.BaseType)
            {
                FieldInfo f = AccessTools.Field(t, name);
                if (f != null) return f.GetValue(instance);
            }
            return null;
        }

        /// <summary>레이어 묶음에서 이름과 타입을 뽑는다. 사전이든 목록이든 받는다.</summary>
        private static string LayerNames(object layers)
        {
            if (layers == null) return "null";

            var names = new List<string>();
            IEnumerable seq = layers as IEnumerable;
            if (seq == null) return layers.GetType().Name;

            foreach (object item in seq)
            {
                object layer = item;

                // 사전이면 Value 를 꺼낸다.
                Type it = item?.GetType();
                if (it != null && it.IsGenericType && it.Name.StartsWith("KeyValuePair"))
                {
                    PropertyInfo vp = it.GetProperty("Value");
                    if (vp != null) layer = vp.GetValue(item, null);
                }

                if (layer == null) { names.Add("null"); continue; }

                string name = null;
                try
                {
                    MethodInfo nameMethod = AccessTools.Method(layer.GetType(), "Name");
                    if (nameMethod != null && nameMethod.GetParameters().Length == 0)
                        name = nameMethod.Invoke(layer, null) as string;
                }
                catch { /* 이름을 못 얻으면 타입만 적는다 */ }

                names.Add((string.IsNullOrEmpty(name) ? "?" : name) + "[" + layer.GetType().Name + "]");
                if (names.Count >= 12) { names.Add("..."); break; }
            }

            return names.Count == 0 ? "(빈 묶음)" : string.Join(",", names);
        }

        /// <summary>brain 쪽에서 BotOwner 를 찾아 이름·역할을 적는다.</summary>
        private static string BotNameNear(object strategy)
        {
            for (Type t = strategy.GetType(); t != null; t = t.BaseType)
            {
                foreach (FieldInfo f in t.GetFields(AccessTools.all))
                {
                    if (!typeof(BotOwner).IsAssignableFrom(f.FieldType)) continue;
                    var owner = f.GetValue(f.IsStatic ? null : strategy) as BotOwner;
                    if (owner == null) continue;
                    string nick = owner.Profile?.Nickname ?? "?";
                    string role = owner.Profile?.Info?.Settings?.Role.ToString() ?? "?";
                    return Quote(nick) + "/" + role;
                }
            }
            return "(BotOwner 없음)";
        }

        private static string Quote(string s) => s == null ? "null" : "\"" + s + "\"";
    }
}
