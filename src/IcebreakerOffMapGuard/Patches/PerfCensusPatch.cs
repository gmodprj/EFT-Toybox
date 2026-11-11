namespace IcebreakerOffMapGuard.Patches
{
    /// <summary>
    /// <c>Manimal.Icebreaker.RenderEnvProbe.TickVanillaPerfCensus()</c> 를 건너뛴다.
    ///
    /// 그 메서드(private instance, IL 680바이트)는 아이스브레이커가 **아닌** 맵에서 스폰
    /// 60초쯤 뒤에 <c>FindObjectsOfType&lt;CullingGridContent&gt;()</c>·
    /// <c>FindObjectsOfType&lt;LODGroup&gt;()</c> 로 장면을 전수조사하고 <c>[PerfCensus]</c>
    /// 경고 한 줄을 남긴다. 재는 것뿐이고 게임을 바꾸지 않는다 — 우리 2026-11-11 기동에서는
    /// 라이트하우스 <c>lodGroups=58529</c> · laboratory 17124 · hideout 1873 으로 세 번 돌았다.
    ///
    /// <c>DevMode</c>·<c>DiagHotkeys</c> 를 둘 다 꺼 둬도 돈다(그 기동의 설정이 그랬다).
    /// 그래서 설정으로는 끌 수 없고, 이 패치가 유일한 손잡이다.
    ///
    /// 되돌리기: F12 에서 <c>Skip vanilla perf census</c> 를 끄면 다음 호출부터 원본이 돈다.
    /// </summary>
    internal static class PerfCensusPatch
    {
        /// <returns>false 면 원본을 건너뛴다.</returns>
        internal static bool Prefix()
        {
            if (!IcebreakerOffMapGuardPlugin.SkipPerfCensus)
            {
                return true;
            }

            Skipped++;
            return false;
        }

        /// <summary>건너뛴 횟수. 60초에 한 번쯤 늘어난다.</summary>
        internal static long Skipped;
    }
}
