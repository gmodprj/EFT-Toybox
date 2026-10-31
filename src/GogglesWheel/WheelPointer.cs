using System;

namespace GogglesWheel
{
    /// <summary>
    /// 휠 위의 **가상 커서**. 마우스가 움직인 양을 쌓아 원 안의 한 점으로 들고 있는다.
    /// Unity 도 EFT 도 모르는 순수 코드다.
    ///
    /// 왜 이렇게 하나 — 전에는 게임의 시점 축 배열(axes[4], axes[5])에서 방향을 받았다.
    /// 그 배열은 입력 사슬을 타고 내려오는 것이라, 우리 자리보다 앞에 있는 다른 입력 노드가
    /// 축을 먼저 0 으로 만들면 우리에게는 영원히 0 만 온다. 같은 일을 하는 설치된 모드
    /// Tylevo.FieldAttachments 를 뜯어 보니 축 배열은 **막는 데만** 쓰고, 커서 방향은
    /// `Input.GetAxisRaw("Mouse X" / "Mouse Y")` 에서 따로 받아 자기 좌표에 쌓고 있었다(실측).
    /// 그쪽이 사슬 위치와 무관하므로 우리도 그렇게 바꾼다.
    /// </summary>
    public static class WheelPointer
    {
        /// <summary>커서가 벗어날 수 없는 반지름. 데드존이 0~1 단위라 1 로 맞춘다.</summary>
        public const float MaxRadius = 1.0f;

        /// <summary>
        /// 움직인 양을 더해 새 자리를 돌려준다. 원을 벗어나면 테두리에 붙인다.
        /// 축 값이 NaN·무한이면 그 프레임은 버린다 — 커서가 한 번 깨지면 되돌릴 수가 없다.
        /// </summary>
        public static (float X, float Y) Advance(float x, float y, float dx, float dy, float sensitivity)
        {
            if (!IsFinite(dx) || !IsFinite(dy) || !IsFinite(sensitivity))
            {
                return (x, y);
            }

            var nextX = x + dx * sensitivity;
            var nextY = y + dy * sensitivity;

            if (!IsFinite(nextX) || !IsFinite(nextY))
            {
                return (x, y);
            }

            var length = Math.Sqrt((double)nextX * nextX + (double)nextY * nextY);
            if (length > MaxRadius)
            {
                var scale = MaxRadius / length;
                nextX = (float)(nextX * scale);
                nextY = (float)(nextY * scale);
            }

            return (nextX, nextY);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
