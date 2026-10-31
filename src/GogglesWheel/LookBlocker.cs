using System;
using EFT;
using HarmonyLib;
using UnityEngine;

namespace GogglesWheel
{
    /// <summary>
    /// 휠이 떠 있는 동안 **시점만** 막는다. 이동·린은 그대로 둔다(설계 §6-4).
    ///
    /// 바닐라 제스처 휠은 축 배열을 통째로 null 로 만들어 이동까지 멈추는데,
    /// 이건 교전 중에 쓰는 동작이라 제자리에 얼어붙으면 안 된다.
    ///
    /// `MoveInputTranslator.TranslateAxes(ref float[] axes)` 의 축 번호(실측):
    /// 0·1 = 이동, 2·3 = 회전(Turn), 4·5 = 시점(Look), 6 = 린.
    /// 같은 일을 하는 설치된 모드 Tylevo.FieldAttachments 도 같은 번호로 2~6 을 0 으로 만든다(실측).
    /// 우리는 이동과 린은 살려 두려고 2~5 만 막는다.
    ///
    /// 여기서 **방향을 읽지는 않는다**. 축 배열은 입력 사슬을 타고 내려오는 것이라
    /// 우리보다 앞의 노드가 먼저 0 으로 만들면 영영 0 만 온다. 휠의 방향은
    /// <see cref="WheelPointer"/> 가 날 마우스 움직임에서 따로 받는다.
    /// </summary>
    public static class LookBlocker
    {
        private const int TurnX = 2;
        private const int TurnY = 3;
        private const int LookX = 4;
        private const int LookY = 5;

        private static bool _patched;

        /// <summary>지금 막고 있는가.</summary>
        public static bool Blocking { get; private set; }

        public static bool TryPatch(Harmony harmony)
        {
            var target = AccessTools.Method(typeof(MoveInputTranslator), "TranslateAxes");
            if (target == null)
            {
                Plugin.Log.LogWarning("시점 축을 막을 자리를 찾지 못했다. 휠이 떠 있는 동안 시점이 돈다.");
                return false;
            }

            harmony.Patch(target,
                prefix: new HarmonyMethod(AccessTools.Method(typeof(LookBlocker), nameof(TranslateAxesPrefix))));
            _patched = true;
            return true;
        }

        /// <summary>막기 시작한다. 패치를 못 걸었으면 false — 그때는 휠을 띄워도 고를 수가 없다.</summary>
        public static bool Block()
        {
            if (!_patched)
            {
                return false;
            }

            Blocking = true;
            return true;
        }

        /// <summary>여러 번 불러도 안전하다. 어떤 경로로 끝나든 이것만 불리면 된다.</summary>
        public static void Release()
        {
            Blocking = false;
        }

        private static void TranslateAxesPrefix(ref float[] axes)
        {
            try
            {
                if (!Blocking || axes == null || axes.Length <= LookY)
                {
                    return;
                }

                axes[LookX] = 0f;
                axes[LookY] = 0f;
                axes[TurnX] = 0f;
                axes[TurnY] = 0f;
            }
            catch (Exception e)
            {
                // 막다 터지면 막기를 포기한다 — 조종을 못 하게 되는 쪽이 더 나쁘다.
                Blocking = false;
                Plugin.Log.LogWarning("시점을 막다 멈췄다: " + e.Message);
            }
        }
    }
}
