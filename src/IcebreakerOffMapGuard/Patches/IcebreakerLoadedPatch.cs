namespace IcebreakerOffMapGuard.Patches
{
    /// <summary>
    /// <c>Manimal.Icebreaker.IcebreakerAcoustics.IcebreakerLoaded()</c> 를 기억해 둔다.
    ///
    /// 그 메서드는 **로드된 씬 전부의 이름을 읽는다**(static public bool, IL 64바이트).
    /// <c>RenderEnvProbe.Update</c> 의 "아이스브레이커가 아닐 때" 가지가 이것을 **매 프레임**
    /// 부르기 때문에, 다른 맵에서도 프레임마다 쓰레기가 생긴다(제작자 페이지의 1.1.3 보고:
    /// Factory 에서 프레임당 약 1.5 KB). 답은 씬이 바뀔 때만 달라지므로 한 번 받아 두면 된다.
    ///
    /// 여기서 판정을 흉내 내지 않는다 — 캐시가 비어 있으면 **원본을 그대로 돌리고**
    /// 그 결과만 적는다(<see cref="SceneMemo"/>).
    /// </summary>
    internal static class IcebreakerLoadedPatch
    {
        /// <returns>false 면 원본을 건너뛴다.</returns>
        internal static bool Prefix(ref bool __result)
        {
            if (!IcebreakerOffMapGuardPlugin.CacheLoadedFlag)
            {
                return true;
            }

            if (!SceneMemo.TryGet(out var cached))
            {
                return true;    // 캐시가 비었다 — 원본에게 묻고 Postfix 에서 적는다
            }

            __result = cached;
            SceneMemo.Stats.Served++;
            return false;
        }

        internal static void Postfix(bool __result)
        {
            if (IcebreakerOffMapGuardPlugin.CacheLoadedFlag)
            {
                SceneMemo.Set(__result);
            }
        }
    }
}
