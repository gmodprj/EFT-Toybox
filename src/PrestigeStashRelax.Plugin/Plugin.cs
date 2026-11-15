using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace PrestigeStashRelaxClient
{
    /// <summary>
    /// 프레스티지 **이전 창고**에 담을 수 있는 것을 화면 쪽에서 넓힌다.
    ///
    /// 서버 쪽(<c>PrestigeStashRelax</c>)이 격자 크기와 허용 목록을 넓혀도 클라이언트가 그 앞에
    /// 두 개의 관문을 따로 세운다. `EFT.UI.PrestigeTransferItemsState` 의
    /// `ISelectionContext.IsActive` 가 그 둘이다(2026-11-15 디컴파일).
    ///
    /// <code>
    /// if (!_stashConfig.Filters.Contains(context.Item.Template)) { tooltip = "UI/Trader/TransferLocked"; return false; }
    /// if (context.Item is CompoundItem { IsEmpty: false })       { tooltip = "UI/Prestige/NonEmptyContainerError"; return false; }
    /// tooltip = null; return true;
    /// </code>
    ///
    /// 둘째는 **데이터로 풀 수 없다** — 가방에 물건이 들어 있으면 허용 목록과 무관하게 거부한다.
    /// 그래서 여기서 뗀다. 서버는 이미 받은 id 를 <c>GetItemWithChildren</c> 으로 **자식째** 집어
    /// 우편으로 보내므로(검사도 하지 않는다), 화면만 열어 주면 내용물이 그대로 따라간다.
    ///
    /// 패치는 <b>Postfix</b> 다. 원본을 그대로 돌린 뒤 **거부 사유(tooltip)를 보고** 뒤집는다 —
    /// 어느 규칙이 걸렸는지 그 문자열이 알려 주므로, 규칙을 하나씩 가려 끌 수 있다. 원본을 건너뛰면
    /// 그 구분이 사라진다.
    /// </summary>
    [BepInPlugin(PluginGuid, "프레스티지 이전 창고 풀기", "0.1.0")]
    public class PrestigeStashRelaxPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "spt.prestigestashrelax.client";

        /// <summary>필터에 걸렸을 때 원본이 넣는 말풍선 키.</summary>
        internal const string FilterTooltip = "UI/Trader/TransferLocked";

        /// <summary>내용물이 든 가방에 걸렸을 때 원본이 넣는 말풍선 키.</summary>
        internal const string NonEmptyTooltip = "UI/Prestige/NonEmptyContainerError";

        internal static ManualLogSource Log;

        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<bool> AllowNonEmptyContainers;
        internal static ConfigEntry<bool> IgnoreItemFilter;
        internal static ConfigEntry<bool> Verbose;

        private void Awake()
        {
            Log = Logger;

            Enabled = Config.Bind("일반", "켜기", true,
                "끄면 바닐라 규칙 그대로 돌아간다. 이미 열려 있는 전송 화면은 닫고 다시 열어야 한다.");

            AllowNonEmptyContainers = Config.Bind("제한 풀기", "내용물이 든 가방도 넘기기", true,
                "바닐라는 가방·케이스·조끼에 물건이 하나라도 들어 있으면 거부한다"
                + "(\"You can't transfer a container containing items.\"). 이것을 뗀다. "
                + "서버는 보낸 물건을 자식째 집어 우편으로 보내므로 안에 든 것이 그대로 따라온다.");

            IgnoreItemFilter = Config.Bind("제한 풀기", "품목 제한 무시", false,
                "담을 수 있는 품목 목록 자체를 무시한다. 켜면 서버 쪽 허용 목록(PrestigeStashRelax 의 "
                + "AllowBags·AllowMoney 등)이 무의미해지고 **무엇이든** 담긴다. "
                + "서버 설정으로 종류를 고르고 싶으면 끈 채로 둔다.");

            Verbose = Config.Bind("일반", "자세히 적기", false,
                "뒤집을 때마다 로그에 한 줄씩. 무엇이 왜 열렸는지 볼 때만 켠다.");

            var target = AccessTools.Method(
                typeof(EFT.UI.PrestigeTransferItemsState),
                "EFT.InventoryLogic.SelectableItemContext.ISelectionContext.IsActive");

            if (target == null)
            {
                // 이름이 바뀌었으면 조용히 죽지 않고 **말하고** 그만둔다 — 안전장치는 닫힌 쪽으로.
                Logger.LogError(
                    "[PrestigeStashRelax] IsActive 를 못 찾았다. 게임 판이 바뀌었을 수 있다. 아무것도 패치하지 않는다.");
                return;
            }

            var harmony = new Harmony(PluginGuid);

            harmony.Patch(
                target,
                postfix: new HarmonyMethod(typeof(SelectionGate), nameof(SelectionGate.Postfix)));

            Logger.LogInfo("[PrestigeStashRelax] 클릭 경로(IsActive)에 패치를 걸었다.");

            // 끌기는 다른 경로다 — 격자 뷰가 **같은 검사를 자기 복사본으로** 한 번 더 한다.
            var dragTarget = AccessTools.Method(
                typeof(EFT.UI.ArenaEftItemTransferGridView),
                nameof(EFT.UI.ArenaEftItemTransferGridView.CanAccept));

            if (dragTarget == null)
            {
                Logger.LogError(
                    "[PrestigeStashRelax] CanAccept 를 못 찾았다. 클릭은 되지만 끌기는 막힌 채로 둔다.");
                return;
            }

            harmony.Patch(
                dragTarget,
                transpiler: new HarmonyMethod(typeof(DragGate), nameof(DragGate.Transpiler)));

            Logger.LogInfo("[PrestigeStashRelax] 끌기 경로(CanAccept)에 패치를 걸었다.");
        }
    }

    /// <summary>
    /// 원본이 거부한 것을 **사유별로** 되돌린다.
    /// </summary>
    internal static class SelectionGate
    {
        /// <param name="__result">원본의 판정. 우리가 뒤집는다.</param>
        /// <param name="tooltip">원본이 넣은 거부 사유. 어느 규칙이 걸렸는지 이것으로 안다.</param>
        internal static void Postfix(ref bool __result, ref string tooltip)
        {
            if (__result || !PrestigeStashRelaxPlugin.Enabled.Value)
            {
                return;
            }

            var reason = tooltip;

            var allow =
                (reason == PrestigeStashRelaxPlugin.NonEmptyTooltip
                    && PrestigeStashRelaxPlugin.AllowNonEmptyContainers.Value)
                || (reason == PrestigeStashRelaxPlugin.FilterTooltip
                    && PrestigeStashRelaxPlugin.IgnoreItemFilter.Value);

            if (!allow)
            {
                return;
            }

            __result = true;
            tooltip = null;

            if (PrestigeStashRelaxPlugin.Verbose.Value)
            {
                PrestigeStashRelaxPlugin.Log.LogInfo("[PrestigeStashRelax] 거부를 되돌렸다: " + reason);
            }
        }
    }

    /// <summary>
    /// **끌기** 경로의 관문.
    ///
    /// 클릭은 <c>IsActive</c> → <c>SelectItem</c> 으로 가지만, 끌어서 놓는 것은
    /// <c>ArenaEftItemTransferGridView.CanAccept</c> 를 거친다. 그리고 그 메서드가 **같은 검사를
    /// 자기 복사본으로** 한 번 더 한다(2026-11-16 실측 — 클릭은 넘어가는데 끌기는 안 넘어갔다).
    ///
    /// <code>
    /// if (itemContext.Item is CompoundItem { IsEmpty: false }) { operation = new NonEmptyCompoundItemError(); return false; }
    /// </code>
    ///
    /// Postfix 로는 못 고친다 — 거부되면 <c>operation</c> 이 오류로 채워져 나오는데, 되돌리려면
    /// 원본이 하던 <c>ItemManipulator.Move(..., simulate: true)</c> 를 우리가 다시 해야 하고
    /// 그러자면 private 필드까지 흉내 내야 한다. Prefix 로 건너뛰어도 마찬가지다.
    ///
    /// 그래서 **Transpiler** 로 `IsEmpty` 호출 **하나의 반환값만** 뒤집는다. IL 은
    /// `callvirt get_IsEmpty` 다음이 `brtrue`(비어 있으면 오류를 건너뛴다)이므로, 그 자리에서
    /// true 를 돌려주면 검사만 빠지고 나머지 판정(겹침·자리·소유권)은 **원본 그대로** 돈다.
    /// 주변 코드가 바뀌어도 `get_IsEmpty` 호출만 있으면 붙는다.
    /// </summary>
    internal static class DragGate
    {
        /// <summary>원래 값이 "비어 있다"가 아니어도, 설정이 켜져 있으면 비어 있다고 답한다.</summary>
        internal static bool RelaxEmpty(bool original)
        {
            return original
                || (PrestigeStashRelaxPlugin.Enabled.Value
                    && PrestigeStashRelaxPlugin.AllowNonEmptyContainers.Value);
        }

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var isEmpty = AccessTools.PropertyGetter(
                typeof(EFT.InventoryLogic.CompoundItem),
                nameof(EFT.InventoryLogic.CompoundItem.IsEmpty));
            var relax = AccessTools.Method(typeof(DragGate), nameof(RelaxEmpty));

            var code = new List<CodeInstruction>(instructions);

            if (isEmpty == null || relax == null)
            {
                PrestigeStashRelaxPlugin.Log.LogError(
                    "[PrestigeStashRelax] IsEmpty 를 못 찾았다. 끌기 경로는 바닐라 그대로 둔다.");
                return code;
            }

            var patched = 0;
            for (var i = code.Count - 1; i >= 0; i--)
            {
                if (code[i].Calls(isEmpty))
                {
                    code.Insert(i + 1, new CodeInstruction(OpCodes.Call, relax));
                    patched++;
                }
            }

            if (patched == 0)
            {
                // 조용히 지나가면 "걸렸는데 안 듣는다"가 되어 가장 나쁘다.
                PrestigeStashRelaxPlugin.Log.LogError(
                    "[PrestigeStashRelax] CanAccept 안에서 IsEmpty 호출을 못 찾았다. 끌기는 막힌 채로 둔다.");
            }
            else
            {
                PrestigeStashRelaxPlugin.Log.LogInfo(
                    "[PrestigeStashRelax] 끌기 경로에서 IsEmpty 검사 " + patched + "곳을 비껴가게 했다.");
            }

            return code;
        }
    }
}
