using System;
using System.Collections.Generic;
using System.Reflection;

using DynamicMaps.DynamicMarkers;
using DynamicMaps.UI;
using DynamicMaps.UI.Components;
using DynamicMaps.Utils;
using EFT;
using HarmonyLib;
using UnityEngine;

namespace DynamicMapsFactionMarkers
{
    /// <summary>
    /// Dynamic Maps 1.2.1 의 <c>OtherPlayersMarkerProvider</c> private 지점에 끼어든다.
    ///
    /// ① TryAddMarker  — 우리 역할이면 우리 색·모양으로 마커를 만들고 **그들의 사전에 넣는다**.
    ///    제거·맵 전환·레벨 전환·사망·RefreshMarkers 가 그들 코드로 그대로 돌게 하려는 것이다.
    /// ② ShouldShowCategory — 우리 범주면 F12 토글 값을 돌려준다. 이게 없으면 그들의
    ///    RemoveDisabledMarkers 가 "모르는 범주" 로 보고 우리 마커를 솎아낸다.
    /// </summary>
    public static class MarkerPatches
    {
        private static FieldInfo _lastMapViewField;
        private static FieldInfo _playerMarkersField;
        private static readonly Dictionary<string, FieldInfo> _intelFields = new Dictionary<string, FieldInfo>();

        /// <summary>핵심 지점을 다 찾아 걸었으면 true. 하나라도 없으면 아무 것도 걸지 않는다.</summary>
        public static bool TryApply(Harmony harmony)
        {
            var providerType = typeof(OtherPlayersMarkerProvider);

            var tryAddMarker = AccessTools.Method(providerType, "TryAddMarker", new[] { typeof(IPlayer) });
            var shouldShowCategory = AccessTools.Method(providerType, "ShouldShowCategory", new[] { typeof(string) });
            _lastMapViewField = AccessTools.Field(providerType, "_lastMapView");
            _playerMarkersField = AccessTools.Field(providerType, "_playerMarkers");

            if (tryAddMarker == null || shouldShowCategory == null || _lastMapViewField == null || _playerMarkersField == null)
            {
                Plugin.Log.LogWarning(
                    "Dynamic Maps 의 마커 담당 내부가 예상과 다르다(TryAddMarker/ShouldShowCategory/_lastMapView/_playerMarkers). " +
                    "지도 세력 표시를 끈다 — 지도는 바닐라 그대로 돈다.");
                return false;
            }

            harmony.Patch(tryAddMarker,
                prefix: new HarmonyMethod(AccessTools.Method(typeof(MarkerPatches), nameof(TryAddMarkerPrefix))));
            harmony.Patch(shouldShowCategory,
                prefix: new HarmonyMethod(AccessTools.Method(typeof(MarkerPatches), nameof(ShouldShowCategoryPrefix))));

            ApplyProviderRegistration(harmony);
            MapScreenBridge.Resolve();

            return true;
        }

        /// <summary>
        /// ③ 곁가지 — 그들은 바닐라 네 설정의 OR 로만 마커 담당을 등록한다. 네 개가 다 꺼져 있으면
        /// 위 두 패치가 불릴 자리가 없다. 우리 토글이 켜져 있으면 **등록만** 보장한다.
        /// 그들의 네 범주는 자기 bool 이 false 라 ShouldShowCategory 에서 막히므로 바닐라 마커는 늘지 않는다.
        /// 없어도 전체를 끄지 않는다(설계 §6-5).
        /// </summary>
        private static void ApplyProviderRegistration(Harmony harmony)
        {
            var readConfig = AccessTools.Method(typeof(ModdedMapScreen), "ReadConfig");
            if (readConfig == null)
            {
                Plugin.Log.LogWarning(
                    "지도 화면의 ReadConfig 를 찾지 못했다. Dynamic Maps 의 마커 네 개(아군·적 PMC·보스·스캐브) 중 " +
                    "하나는 켜 두어야 모드 세력이 보인다.");
                return;
            }

            harmony.Patch(readConfig,
                postfix: new HarmonyMethod(AccessTools.Method(typeof(MarkerPatches), nameof(ReadConfigPostfix))));
        }

        private static void ReadConfigPostfix(ModdedMapScreen __instance)
        {
            try
            {
                if (!Plugin.Active || !Plugin.ShowFactionMarkers.Value)
                {
                    return;
                }

                // 담당이 이미 있으면 그들이 네 bool 을 자기 설정대로 넣어 둔 것이다 — 손대지 않는다.
                if (MapScreenBridge.HasPlayerMarkerProvider(__instance))
                {
                    return;
                }

                // 없다는 것은 바닐라 네 설정이 모두 꺼져 ReadConfig 가 담당을 **떼어 냈다**는 뜻이다.
                // 새로 만들면 `_showFriendlyPlayers` 가 true 로 태어나(그들의 필드 초기값) 아군 마커가
                // 되살아나고 서버의 허용 스위치도 건너뛴다. 그래서 만든 뒤 넷을 직접 false 로 내린다.
                __instance.AddRemoveMarkerProvider<OtherPlayersMarkerProvider>(true);

                var provider = MapScreenBridge.GetPlayerMarkerProvider(__instance);
                if (provider == null)
                {
                    return;
                }

                provider.ShowFriendlyPlayers = false;
                provider.ShowEnemyPlayers = false;
                provider.ShowScavs = false;
                provider.ShowBosses = false;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("마커 담당 등록을 보장하지 못했다: " + e.Message);
            }
        }

        /// <summary>false 를 돌려주면 원본을 건너뛴다.</summary>
        private static bool TryAddMarkerPrefix(OtherPlayersMarkerProvider __instance, IPlayer iPlayer)
        {
            try
            {
                if (!Plugin.Active || !Plugin.ShowFactionMarkers.Value || Plugin.Catalog == null)
                {
                    return true;
                }

                var player = iPlayer as Player;
                if (player == null || player.IsYourPlayer)
                {
                    return true;
                }

                // 같은 그룹 동료는 그들의 '아군' 분류가 먼저다(PIT Fireteam 동료 등).
                if (iPlayer.IsGroupedWithMainPlayer())
                {
                    return true;
                }

                // 게임 locale 은 BepInEx Awake 보다 늦게 올라온다. 레이드에서 처음 쓸 때 한 번 이름을 짓는다.
                if (Plugin.Catalog.ApplyLabels(RoleSource.LookupLabel))
                {
                    Plugin.Log.LogInfo("범주 이름을 locale 로 다시 지었다.");
                }

                var role = (int)player.Profile.Info.Settings.Role;
                if (!Plugin.Catalog.TryGet(role, out var entry))
                {
                    return true;
                }

                if (iPlayer.IsHeadlessClient() || iPlayer.IsBTRShooter())
                {
                    return true;
                }

                var markers = (Dictionary<Player, PlayerMapMarker>)_playerMarkersField.GetValue(__instance);
                var mapView = (MapView)_lastMapViewField.GetValue(__instance);
                if (markers == null || mapView == null)
                {
                    return true;
                }

                if (markers.ContainsKey(player))
                {
                    // 이미 우리가 넣어 둔 것. 원본이 또 넣지 않게 막는다.
                    return false;
                }

                if (!PassesIntelGate())
                {
                    // 바닐라가 그렸을 것(스캐브)까지 지우지는 않는다 — 원본에 넘긴다.
                    return true;
                }

                var color = new Color(entry.Style.R, entry.Style.G, entry.Style.B, 1f);
                markers[player] = mapView.AddPlayerMarker(iPlayer, entry.Category, color, entry.Style.IconPath);
                return false;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("마커를 그리다 멈췄다. 이 봇은 바닐라 분류로 보낸다: " + e.Message);
                return true;
            }
        }

        private static bool ShouldShowCategoryPrefix(string category, ref bool __result)
        {
            try
            {
                if (!Plugin.Active || Plugin.Catalog == null || !Plugin.Catalog.IsOurCategory(category))
                {
                    return true;
                }

                __result = Plugin.ShowFactionMarkers.Value;
                return false;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("범주를 판정하다 멈췄다: " + e.Message);
                return true;
            }
        }

        /// <summary>
        /// 설계 §7-1 — intel 요구치는 기본으로 보스 쪽에 묶는다.
        /// `none` 이면 게이트를 보지 않는다.
        /// </summary>
        private static bool PassesIntelGate()
        {
            var source = Plugin.Settings?.IntelLevelSource;
            if (string.Equals(source, "none", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var fieldName = string.Equals(source, "scav", StringComparison.OrdinalIgnoreCase)
                ? "ShowScavIntelLevel"
                : "ShowBossIntelLevel";

            if (!TryReadIntelRequirement(fieldName, out var required))
            {
                // 요구치를 못 읽으면 게이트를 보지 않는다 — 곁가지라 전체를 끄지 않는다.
                return true;
            }

            // 바닐라와 똑같이 판정한다(그들도 int? 와 비교해 null 이면 안 그린다).
            var intelLevel = GameUtils.GetIntelLevel();
            return required <= intelLevel;
        }

        /// <summary>
        /// Dynamic Maps 의 Settings 는 internal 이라 리플렉션으로 읽는다.
        /// 못 읽으면 한 번만 경고하고 게이트를 건너뛴다.
        /// </summary>
        private static bool TryReadIntelRequirement(string fieldName, out int required)
        {
            required = 0;

            if (!_intelFields.TryGetValue(fieldName, out var field))
            {
                var settingsType = AccessTools.TypeByName("DynamicMaps.Config.Settings");
                field = settingsType != null ? AccessTools.Field(settingsType, fieldName) : null;
                _intelFields[fieldName] = field;

                if (field == null)
                {
                    Plugin.Log.LogWarning("Dynamic Maps 의 intel 요구치(" + fieldName + ")를 읽지 못했다. 그 게이트는 보지 않는다.");
                }
            }

            var entry = field?.GetValue(null);
            if (entry == null)
            {
                return false;
            }

            var value = AccessTools.Property(entry.GetType(), "Value")?.GetValue(entry, null);
            if (!(value is int parsed))
            {
                return false;
            }

            required = parsed;
            return true;
        }
    }
}
