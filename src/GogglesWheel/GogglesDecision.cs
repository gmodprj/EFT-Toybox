using System.Collections.Generic;

namespace GogglesWheel
{
    /// <summary>N 이 눌렸을 때 우리가 할 일.</summary>
    public enum GogglesDecision
    {
        /// <summary>물러난다 — 남의 패치와 바닐라가 하던 대로 돌게 둔다.</summary>
        StandDown,

        /// <summary>여닫을 것이 없다. 아무 일도 하지 않고 바닐라도 막는다.</summary>
        DoNothing,

        /// <summary>기다리지 않고 기본 대상을 바로 여닫는다.</summary>
        ToggleDefaultNow,

        /// <summary>키를 보며 짧게/길게를 가른다.</summary>
        WatchForLongPress,
    }

    /// <summary>
    /// 분기 판단만 떼어 낸 것. 여기가 사고 나는 자리라 EFT 없이 시험할 수 있게 순수 코드로 둔다.
    /// 설계 §6-2 의 "물러나는 두 조건" 이 여기 들어 있다.
    /// </summary>
    public static class GogglesDecisionMaker
    {
        /// <param name="runOriginal">
        /// HarmonyX 가 넘겨주는 값. 앞의 prefix 중 하나가 이미 false 를 돌려줬으면 false 다.
        /// HarmonyX 는 앞에서 false 가 나와도 **나머지 prefix 를 전부 실행하므로**, 이 값을 보지 않으면
        /// 남이 가져간 입력에 우리가 또 손을 댄다(UI Fixes 의 나침반 치우기가 그 자리다 — 그 호출에서
        /// 우리가 한 번, 애니메이션 뒤 되부름에서 또 한 번 여닫아 결국 아무 일도 없는 것처럼 보인다).
        /// </param>
        public static GogglesDecision Decide(IReadOnlyList<ToggleTarget> targets, bool keyHeld, bool cotiModifierHeld, bool runOriginal)
        {
            // 물러나는 것이 먼저다. 우리가 목록을 못 만들었다고 남의 입력을 삼키면 안 된다.
            if (!runOriginal || cotiModifierHeld)
            {
                return GogglesDecision.StandDown;
            }

            if (targets == null || targets.Count == 0)
            {
                return GogglesDecision.DoNothing;
            }

            if (targets.Count == 1)
            {
                return GogglesDecision.ToggleDefaultNow;
            }

            // 키가 눌려 있지 않다 = 사람이 아니라 코드가 부른 것(UI Fixes 의 나침반 되부름).
            // 휠을 띄우면 떼어질 일 없는 키를 기다리게 된다.
            if (!keyHeld)
            {
                return GogglesDecision.ToggleDefaultNow;
            }

            return GogglesDecision.WatchForLongPress;
        }

        /// <summary>
        /// 휠을 접을 때 가리키던 것을 여닫을지. **뗐을 때만** 여닫는다.
        ///
        /// 죽은 것도, 안전장치 시간이 다 된 것도 "뗐다" 가 아니다. 특히 시간이 다 된 쪽이
        /// 위험하다 — 키 상태가 눌림에 얼어붙은 사이 사람은 손을 떼고 평소대로 시점을 돌리는데,
        /// 그 움직임이 가상 커서를 끌고 가 엉뚱한 조각에 세워 둔다. 그대로 여닫으면
        /// 15초 전에 손을 뗀 사람의 페이스실드가 영문 없이 내려간다(리뷰 지적).
        /// </summary>
        public static bool ShouldApplySelection(bool alive, bool timedOut)
        {
            return alive && !timedOut;
        }
    }
}
