using System;
using BepInEx.Logging;
using EFT.Airdrop;
using HarmonyLib;

namespace AirdropCaller.Plugin
{
    /// <summary>
    /// `ClientAirDrop.CheckSurface` 에서 **남의 패치가 터뜨리는 예외를 삼킨다.**
    ///
    /// 이 설치본의 Game Panel HUD 나침반(4.1.6 포팅본)은 그 메서드에 Postfix 를 걸고,
    /// 그 Postfix 가 **매 착지마다** 터진다. 자기 트램폴린 타입이 자기 `private static`
    /// 메서드를 못 부르기 때문이다.
    ///
    ///     MethodAccessException: Method `GamePanelHUDCompass…RaycastGround(…)' is
    ///       inaccessible from method `DTFType_GamePanelHUDCompassPlugin_RaycastGround…'
    ///       at (wrapper dynamic-method) ClientAirDrop.DMD&lt;ClientAirDrop::CheckSurface&gt;
    ///       at ClientAirDrop.ManualUpdate (System.Single deltaTime)
    ///
    /// Postfix 의 예외는 패치된 메서드 밖으로 나가 `ManualUpdate` 를 중단시킨다. 착지
    /// 분기가 이 순서라서 뒤의 두 줄이 날아간다.
    ///
    ///     _airdropDust.SetActive(true);   // 여기까지는 된다
    ///     _airdropDust.…Play();
    ///     CheckSurface();                 // 여기서 터진다
    ///     PlayLandingSound();             // 못 돈다 — 낙하산 소리가 안 멈춘다
    ///     CloseParachute();               // 못 돈다 — 낙하산이 남는다
    ///
    /// 그래서 Finalizer 를 하나 걸어 예외를 **없던 것으로 만든다**(null 을 돌려주면
    /// Harmony 가 삼킨다). 그러면 바닐라가 끝까지 돌아 착지음도 나고 낙하산도 스스로
    /// 접힌다. 우리 보급만이 아니라 **바닐라·플레어 보급도** 같이 고쳐진다.
    ///
    /// 나침반 표시는 돌아오지 않는다 — 그쪽 코드는 애초에 불리지 못한다. 그건 포팅본이
    /// 고쳐져야 하는 문제다.
    ///
    /// 실측·근거는 doc/specs/2026-10-28-airdrop-caller-design.md §16.9.
    /// </summary>
    internal static class SurfaceGuard
    {
        /// <summary>F12 에서 끌 수 있다. 끄면 패치는 걸린 채로 아무것도 안 한다.</summary>
        internal static bool Enabled = true;

        private static ManualLogSource _log;

        /// <summary>같은 말을 끝없이 적지 않는다. 몇 번만 적고 센다.</summary>
        private static int _said;
        private static int _swallowed;

        internal static void Install(ManualLogSource log, Harmony harmony)
        {
            _log = log;

            // 못 걸어도 플러그인은 계속 돈다. 우리 쪽 보수(낙하산 접기·소리 끊기)가
            // 증상은 이미 가리므로, 여기서 터져 전체가 멈추는 것이 더 나쁘다.
            try
            {
                var target = AccessTools.Method(typeof(ClientAirDrop), nameof(ClientAirDrop.CheckSurface));
                if (target == null)
                {
                    log.LogWarning("[AirdropCaller] ClientAirDrop.CheckSurface 를 못 찾았다. "
                                   + "예외 삼키기를 걸지 않는다.");
                    return;
                }

                harmony.Patch(target, finalizer: new HarmonyMethod(
                    typeof(SurfaceGuard), nameof(Swallow)));
                log.LogInfo("[AirdropCaller] CheckSurface 의 남의 패치 예외를 삼키도록 걸었다.");
            }
            catch (Exception e)
            {
                log.LogWarning($"[AirdropCaller] 예외 삼키기를 못 걸었다: {e.Message}");
            }
        }

        /// <summary>
        /// Harmony Finalizer. `null` 을 돌려주면 예외가 없던 것이 된다.
        ///
        /// 삼키는 것은 **그 프레임의 지면 판정 하나**뿐이다. `_currentSet` 이 안 정해진
        /// 채로 `PlayLandingSound` 가 돌면 소리만 엉뚱해지는데, 그쪽 패치는 Postfix 라
        /// 바닐라 본문은 이미 다 돈 뒤다 — 즉 `_currentSet` 은 정해져 있다.
        /// </summary>
        private static Exception Swallow(Exception __exception)
        {
            if (__exception == null || !Enabled)
            {
                return __exception;
            }

            _swallowed++;

            if (_said < 3)
            {
                _said++;
                _log?.LogWarning($"[AirdropCaller] CheckSurface 에서 남의 패치가 터졌다. 삼킨다 "
                                 + $"({_swallowed}번째) — {__exception.GetType().Name}: "
                                 + $"{__exception.Message}");
                if (_said == 3)
                {
                    _log?.LogWarning("[AirdropCaller] 같은 예외는 이제 조용히 삼킨다.");
                }
            }

            return null;
        }
    }
}
