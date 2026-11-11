namespace IcebreakerOffMapGuard
{
    /// <summary>
    /// "지금 아이스브레이커가 떠 있나" 의 답을 한 번만 받아 두고, 씬이 바뀔 때만 버린다.
    ///
    /// 우리가 그 판정을 **다시 구현하지 않는다** — 원본을 한 번 돌려 그 결과를 적어 둘 뿐이다.
    /// 판정 기준(씬 이름을 어떻게 보는지)은 모드 쪽 사정이고, 우리가 흉내 내면 버전이 바뀔 때
    /// 조용히 틀린다. 씬이 로드·언로드되지 않는 동안 답이 바뀔 수 없다는 것만 쓴다.
    ///
    /// 유니티 메인 스레드에서만 불리지만, 씬 이벤트와 패치가 같은 프레임에 겹칠 수 있어
    /// 읽기·쓰기 순서만 <c>volatile</c> 로 못 박는다.
    /// </summary>
    public static class SceneMemo
    {
        private static volatile bool _valid;
        private static volatile bool _value;

        /// <summary>캐시가 산 값을 들고 있으면 그것을 주고 true.</summary>
        public static bool TryGet(out bool value)
        {
            value = _value;
            return _valid;
        }

        /// <summary>원본이 낸 답을 적어 둔다.</summary>
        public static void Set(bool value)
        {
            _value = value;
            _valid = true;
            Stats.Stored++;
        }

        /// <summary>씬이 바뀌었다 — 다음 물음은 원본에게 보낸다.</summary>
        public static void Invalidate()
        {
            _valid = false;
            Stats.Invalidated++;
        }

        /// <summary>얼마나 아꼈는지 보려고 센다. 문자열은 만들지 않는다(뜨거운 경로다).</summary>
        public static class Stats
        {
            public static long Served;
            public static long Stored;
            public static long Invalidated;

            public static void Reset()
            {
                Served = 0;
                Stored = 0;
                Invalidated = 0;
            }
        }
    }
}
