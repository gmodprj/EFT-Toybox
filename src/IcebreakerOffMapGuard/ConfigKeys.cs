namespace IcebreakerOffMapGuard
{
    /// <summary>
    /// F12 설정의 구역·열쇠 이름.
    ///
    /// BepInEx 는 구역·열쇠에 <c>= \n \t \ " ' [ ]</c> 를 금지하고, 하나라도 들어가면
    /// <c>Config.Bind</c> 가 <c>ArgumentException</c> 을 던져 <c>Awake</c> 가 그 자리에서 멈춘다.
    /// 2026-11-11 설치본이 열쇠에 홑따옴표를 써서(<c>Cache 'is Icebreaker loaded'</c>) 두 번째
    /// 바인드에서 터졌고, 패치를 하나도 걸지 못한 채 조용히 쉬었다(2026-11-12 기동 로그에서
    /// <c>[PerfCensus]</c> 가 그대로 3회 찍혔다). 그래서 이름을 여기 모아 두고
    /// <c>ConfigKeysTests</c> 가 금지 문자를 지킨다.
    /// </summary>
    internal static class ConfigKeys
    {
        public const string Section = "General";

        public const string SkipCensus = "Skip vanilla perf census";

        public const string CacheLoaded = "Cache the is-Icebreaker-loaded answer";

        public const string Report = "Log a line every 60 s";

        /// <summary>BepInEx 가 구역·열쇠에서 금지하는 문자(<c>ConfigDefinition</c> 와 같은 목록).</summary>
        public static readonly char[] Invalid = { '=', '\n', '\t', '\\', '"', '\'', '[', ']' };

        /// <summary>이 이름으로 <c>Config.Bind</c> 를 불러도 터지지 않는지.</summary>
        public static bool IsValid(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.IndexOfAny(Invalid) < 0;
        }

        /// <summary>바인드에 쓰는 이름 전부 — 테스트가 이 목록을 훑는다.</summary>
        public static string[] All()
        {
            return new[] { Section, SkipCensus, CacheLoaded, Report };
        }
    }
}
