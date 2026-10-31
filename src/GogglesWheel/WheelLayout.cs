namespace GogglesWheel
{
    /// <summary>
    /// 휠의 치수. Unity 도 EFT 도 모르는 순수 코드다.
    ///
    /// 구멍의 반지름과 **고르기가 시작되는 거리**가 한 수에서 나와야 한다. 전에는 구멍을
    /// 112, 데드존 원을 42 로 따로 그려서, 그 사이(50~110px)는 아무 것도 안 그려진 빈 자리인데
    /// 이미 조각이 켜져 있었다 — 그림이 거짓말을 하는 구간이었다(리뷰 지적).
    /// 이제 구멍의 테두리가 곧 그 경계다.
    /// </summary>
    public static class WheelLayout
    {
        /// <summary>고리의 바깥 반지름. 가상 커서의 길이 1 이 여기에 닿는다.</summary>
        public const float OuterRadius = 210f;

        /// <summary>
        /// 고리의 안쪽 반지름 = 구멍. 데드존이 이 안이라 여기서 떼면 취소다.
        /// 설정 범위를 <see cref="WheelConfig.MinDeadZone"/>~<see cref="WheelConfig.MaxDeadZone"/>
        /// 로 좁혀 둬서, 끝까지 밀어도 구멍에 글자가 들어가고 고리가 가리킬 두께로 남는다.
        /// </summary>
        public static float InnerRadius(float deadZone)
        {
            return deadZone * OuterRadius;
        }
    }
}
