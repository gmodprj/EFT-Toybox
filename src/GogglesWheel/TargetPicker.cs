using System;
using System.Collections.Generic;

namespace GogglesWheel
{
    /// <summary>
    /// 휠에서 무엇을 고르는지, 짧게 눌렀을 때 무엇을 여닫는지. Unity 도 EFT 도 모르는 순수 코드다.
    /// 각도는 도 단위이고 화면 기준이다 — 0 이 오른쪽, 90 이 위.
    /// </summary>
    public static class TargetPicker
    {
        /// <summary>두 항목까지의 거리가 이 안이면 비긴 것으로 본다(도 단위).</summary>
        private const double TieEpsilon = 1e-4;

        /// <summary>
        /// 항목이 놓이는 각도. 둘이면 왼쪽·오른쪽(설계 §6-4), 그 밖이면 위에서 시계 방향으로 고르게 나눈다.
        /// </summary>
        public static float AngleForIndex(int index, int count)
        {
            if (count <= 0)
            {
                return 0f;
            }

            if (count == 2)
            {
                return index == 0 ? 180f : 0f;
            }

            return 90f - index * (360f / count);
        }

        /// <summary>
        /// 기울인 방향으로 항목 하나를 고른다. 데드존 안이거나 항목이 없으면 -1.
        /// </summary>
        public static int PickByDirection(float x, float y, int count, float deadZone)
        {
            if (count <= 0)
            {
                return -1;
            }

            var length = Math.Sqrt((double)x * x + (double)y * y);
            if (length < deadZone)
            {
                return -1;
            }

            var angle = Math.Atan2(y, x) * 180.0 / Math.PI;

            var best = -1;
            var bestDistance = double.MaxValue;
            for (var i = 0; i < count; i++)
            {
                var distance = Math.Abs(NormalizeDegrees(angle - AngleForIndex(i, count)));

                // 정확히 경계일 때 부동소수 잡음으로 갈리지 않게, 비기면 낮은 번호가 이긴다.
                if (distance < bestDistance - TieEpsilon)
                {
                    bestDistance = distance;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>
        /// 쌓인 시점 입력을 0~1 기울기로 눌러 준다. 데드존이 0~1 단위라 여기에 맞춰야 한다.
        /// <paramref name="fullTilt"/> 만큼 움직이면 끝까지 기울인 것으로 친다.
        /// </summary>
        public static (float X, float Y) NormalizeTilt(float x, float y, float fullTilt)
        {
            var length = Math.Sqrt((double)x * x + (double)y * y);
            if (length <= 0.0)
            {
                return (0f, 0f);
            }

            if (fullTilt <= 0f)
            {
                fullTilt = 1e-4f;
            }

            var scale = Math.Min(1.0, length / fullTilt) / length;
            return ((float)(x * scale), (float)(y * scale));
        }

        /// <summary>짧게 눌렀을 때 여닫을 항목. 목록이 비었으면 -1.</summary>
        public static int DefaultTargetIndex(IReadOnlyList<ToggleTarget> targets, string defaultTarget)
        {
            if (targets == null || targets.Count == 0)
            {
                return -1;
            }

            if (string.Equals(defaultTarget, "first", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            var wanted = string.Equals(defaultTarget, "faceshield", StringComparison.OrdinalIgnoreCase)
                ? ToggleKind.FaceShield
                : ToggleKind.NightVision;

            for (var i = 0; i < targets.Count; i++)
            {
                if (targets[i] != null && targets[i].Kind == wanted)
                {
                    return i;
                }
            }

            return 0;
        }

        /// <summary>-180 < d <= 180 으로 감아 준다. 180 도 근처에서 이웃을 잘못 집지 않게.</summary>
        private static double NormalizeDegrees(double degrees)
        {
            degrees %= 360.0;
            if (degrees > 180.0)
            {
                degrees -= 360.0;
            }
            else if (degrees <= -180.0)
            {
                degrees += 360.0;
            }

            return degrees;
        }
    }
}
