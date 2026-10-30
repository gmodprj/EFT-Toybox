using System;
using System.Collections.Generic;
using System.Reflection;
using DynamicMaps.DynamicMarkers;
using DynamicMaps.UI;
using HarmonyLib;

namespace DynamicMapsFactionMarkers
{
    /// <summary>
    /// 곁가지 — 없어도 전체를 끄지 않는다(설계 §6-5).
    ///
    /// Dynamic Maps 의 RemoveDisabledMarkers·TryAddMarkers 는 **자기 설정이 바뀔 때만** 돈다.
    /// 우리 F12 토글을 뒤집어도 아무 코드가 안 돌아 마커가 다음 맵 열기까지 남으므로,
    /// 여기서 그들의 public RefreshMarkers() 를 직접 불러 준다.
    /// </summary>
    public static class MapScreenBridge
    {
        private static FieldInfo _providersField;
        private static bool _resolved;

        /// <summary>즉시 반영 경로를 찾았는지.</summary>
        public static bool Available { get; private set; }

        public static void Resolve()
        {
            if (_resolved)
            {
                return;
            }

            _resolved = true;
            _providersField = AccessTools.Field(typeof(ModdedMapScreen), "_dynamicMarkerProviders");
            Available = _providersField != null;

            if (!Available)
            {
                Plugin.Log.LogWarning("지도 화면의 마커 담당 목록을 찾지 못했다. 토글은 다음 맵 열기부터 적용된다.");
            }
        }

        /// <summary>토글이 바뀐 자리에서 마커를 다시 그린다. 못 찾으면 조용히 넘어간다.</summary>
        public static void RefreshNow()
        {
            if (!Plugin.Active || !Available)
            {
                return;
            }

            try
            {
                var provider = GetPlayerMarkerProvider(FindMapScreen());
                provider?.RefreshMarkers();
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("마커를 다시 그리지 못했다. 다음 맵 열기부터 적용된다: " + e.Message);
            }
        }

        public static bool HasPlayerMarkerProvider(ModdedMapScreen screen)
        {
            return GetPlayerMarkerProvider(screen) != null;
        }

        public static OtherPlayersMarkerProvider GetPlayerMarkerProvider(ModdedMapScreen screen)
        {
            if (screen == null || _providersField == null)
            {
                return null;
            }

            if (!(_providersField.GetValue(screen) is Dictionary<Type, IDynamicMarkerProvider> providers))
            {
                return null;
            }

            return providers.TryGetValue(typeof(OtherPlayersMarkerProvider), out var provider)
                ? provider as OtherPlayersMarkerProvider
                : null;
        }

        /// <summary>
        /// 지도 화면은 닫히면 GameObject 가 꺼져서 FindObjectOfType 으로는 안 잡힌다.
        /// Dynamic Maps 의 플러그인 인스턴스가 들고 있는 것을 쓴다(둘 다 public).
        /// </summary>
        private static ModdedMapScreen FindMapScreen()
        {
            return DynamicMaps.Plugin.Instance?.Map;
        }
    }
}
