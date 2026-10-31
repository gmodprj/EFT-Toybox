using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using HarmonyLib;
using UnityEngine;

namespace GogglesWheel
{
    /// <summary>
    /// `Player.ToggleGoggles` 를 대신한다. 바닐라는 헬멧의 여닫이 중 첫 번째 하나만 건드린다.
    ///
    /// 먼저 **물러나는 조건**을 본다(설계 §6-2) — 같은 메서드를 이미 가로채는 모드가 둘 있다.
    /// UI Fixes 는 나침반을 치운 뒤 되부르고, COTI 는 자기 수정키로 억제한다.
    /// </summary>
    public static class GogglesPatch
    {
        /// <summary>휠이 이보다 오래 떠 있으면 접는다. 키 상태가 얼어붙었을 때의 탈출구.</summary>
        private const float MaxWheelSeconds = 15f;

        private static MethodInfo _cotiModifierGetter;
        private static bool _cotiChecked;
        private static bool _watching;

        public static bool TryApply(Harmony harmony)
        {
            var target = AccessTools.Method(typeof(Player), "ToggleGoggles");
            if (target == null)
            {
                Plugin.Log.LogWarning("Player.ToggleGoggles 를 찾지 못했다. 아무 것도 걸지 않는다 — N 은 게임이 하던 대로 돈다.");
                return false;
            }

            // 우선순위를 낮게 둬서 남의 가드(UI Fixes·COTI)가 먼저 돌게 한다.
            var prefix = new HarmonyMethod(AccessTools.Method(typeof(GogglesPatch), nameof(ToggleGogglesPrefix)))
            {
                priority = Priority.Low,
            };

            harmony.Patch(target, prefix: prefix);
            return true;
        }

        /// <summary>
        /// false 를 돌려주면 바닐라를 건너뛴다.
        ///
        /// <paramref name="__runOriginal"/> 은 HarmonyX 가 넘겨주는 값이다. HarmonyX 는 앞의 prefix 가
        /// false 를 돌려줘도 **나머지 prefix 를 전부 실행하므로**, 이 값을 보지 않으면 남이 이미 가져간
        /// 입력에 우리가 또 손을 댄다.
        /// </summary>
        private static bool ToggleGogglesPrefix(Player __instance, bool __runOriginal)
        {
            try
            {
                if (!Plugin.Active || !Plugin.EnableWheel.Value || __instance == null || !__instance.IsYourPlayer)
                {
                    return true;
                }

                var targets = HeadwearScanner.Scan(__instance);
                var decision = GogglesDecisionMaker.Decide(targets, PressWatcher.IsHeldNow(), IsCotiModifierHeld(), __runOriginal);

                switch (decision)
                {
                    case GogglesDecision.StandDown:
                        return true;

                    case GogglesDecision.DoNothing:
                        return false;

                    case GogglesDecision.ToggleDefaultNow:
                        ToggleDefault(__instance, targets);
                        return false;

                    case GogglesDecision.WatchForLongPress:
                        if (_watching)
                        {
                            return false;
                        }

                        _watching = true;
                        try
                        {
                            Plugin.Instance.StartCoroutine(WatchPress(__instance, targets));
                        }
                        catch (Exception)
                        {
                            // 코루틴을 못 띄우면 깃발을 내려 둔다. 안 그러면 이번 판 내내 N 이 죽는다.
                            _watching = false;
                            throw;
                        }

                        return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("N 을 가르다 멈췄다. 이번 입력은 게임에 넘긴다: " + e.Message);
                return true;
            }
        }

        /// <summary>기준 시간 전에 떼면 짧게, 넘기면 길게.</summary>
        private static IEnumerator WatchPress(Player player, IReadOnlyList<ToggleTarget> targets)
        {
            var held = 0f;
            var longPress = false;

            try
            {
                while (PressWatcher.IsHeldNow())
                {
                    held += Time.unscaledDeltaTime;
                    if (held >= Plugin.Settings.LongPressSeconds)
                    {
                        longPress = true;
                        break;
                    }

                    yield return null;
                }

                if (!longPress)
                {
                    ToggleDefault(player, targets);
                    yield break;
                }

                foreach (var step in OnLongPress(player, targets))
                {
                    yield return step;
                }
            }
            finally
            {
                _watching = false;
            }
        }

        /// <summary>
        /// 길게 눌렀을 때 — 휠을 띄우고, 떼는 순간 가리키던 항목을 여닫는다.
        /// 어떤 경로로 끝나든 시점 차단은 반드시 되돌린다.
        /// </summary>
        private static IEnumerable OnLongPress(Player player, IReadOnlyList<ToggleTarget> targets)
        {
            var opened = false;

            try
            {
                try
                {
                    // 시점을 못 막으면 휠의 방향 입력이 들어오지 않는다 — 떠 있어도 아무것도 고를 수 없다.
                    // 그런 허수아비를 띄우느니 기본 대상을 여닫고 끝낸다(설계 §6-6).
                    if (!LookBlocker.Block())
                    {
                        ToggleDefault(player, targets);
                        yield break;
                    }

                    WheelView.Open(targets);
                    opened = true;
                }
                catch (Exception)
                {
                    // 휠을 못 띄우면 손을 묶어 두지 않는다 — 기본 대상을 여닫고 끝낸다.
                    ToggleDefault(player, targets);
                    yield break;
                }

                // 안전장치: 키 상태가 어떤 이유로든 "눌림" 에 얼어붙어도 휠이 영원히 떠 있지 않게 한다.
                // 그대로 두면 시점이 잠긴 채 레이드가 끝날 때까지 간다 — 설계가 최악으로 꼽은 사고다.
                var open = 0f;

                while (PressWatcher.IsHeldNow() && IsStillInRaid(player) && open < MaxWheelSeconds)
                {
                    open += Time.unscaledDeltaTime;

                    // 방향은 게임 시점 축이 아니라 날 마우스 움직임에서 받는다 —
                    // 축 배열은 우리보다 앞의 입력 노드가 먼저 0 으로 만들 수 있다(WheelPointer 주석 참고).
                    WheelView.UpdateSelection(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

                    yield return null;
                }

                if (open >= MaxWheelSeconds)
                {
                    Plugin.Log.LogWarning($"휠이 {MaxWheelSeconds:0}초를 넘겨 떠 있어 접는다. 키 상태가 얼어붙었을 수 있다.");

                    // 키를 바꿨다면 들고 있던 것이 더는 갱신되지 않는다. 다음 번에 다시 찾게 한다.
                    PressWatcher.Forget();
                }

                var alive = IsStillInRaid(player);
                var index = alive ? WheelView.SelectedIndex : -1;
                WheelView.Close(alive);

                if (index >= 0 && index < targets.Count)
                {
                    HeadwearScanner.Toggle(player, targets[index].Id);
                }
            }
            finally
            {
                LookBlocker.Release();
                if (opened && WheelView.IsOpen)
                {
                    WheelView.Close(false);
                }
            }
        }

        /// <summary>죽거나 레이드가 끝나면 휠을 접는다.</summary>
        private static bool IsStillInRaid(Player player)
        {
            try
            {
                return player != null && player.HealthController != null && player.HealthController.IsAlive;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void ToggleDefault(Player player, IReadOnlyList<ToggleTarget> targets)
        {
            var index = TargetPicker.DefaultTargetIndex(targets, Plugin.Settings.DefaultTarget);
            if (index >= 0)
            {
                HeadwearScanner.Toggle(player, targets[index].Id);
            }
        }

        /// <summary>COTI 가 깔려 있고 그 수정키가 눌려 있는가. COTI 가 없으면 늘 false.</summary>
        private static bool IsCotiModifierHeld()
        {
            if (!Plugin.Settings.StandDownForCoti)
            {
                return false;
            }

            if (!_cotiChecked)
            {
                _cotiChecked = true;

                var type = AccessTools.TypeByName("Coti.Client.CotiPowerToggle");
                _cotiModifierGetter = type == null ? null : AccessTools.PropertyGetter(type, "ModifierHeld");

                if (_cotiModifierGetter == null && type != null)
                {
                    Plugin.Log.LogInfo("COTI 는 있는데 ModifierHeld 를 찾지 못했다. 수정키 검사는 건너뛴다.");
                }
            }

            if (_cotiModifierGetter == null)
            {
                return false;
            }

            try
            {
                return _cotiModifierGetter.Invoke(null, null) is bool held && held;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
