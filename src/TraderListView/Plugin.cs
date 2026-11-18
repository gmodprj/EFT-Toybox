using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace TraderListView
{
    /// <summary>
    /// 상인 화면 목록 보기. 설계: doc/specs/2026-11-18-trader-list-view-design.md
    /// </summary>
    [BepInPlugin(PluginGuid, "상인 목록 보기", "0.1.0")]
    [BepInDependency("com.maschine.TraderSearch", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.goatboy.ultimate-trader-assort-editor", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.spt-mods.traderlistview";

        /// <summary>"Grid" 또는 "List". 상인마다가 아니라 전체 하나.</summary>
        public static ConfigEntry<string> ViewMode;

        /// <summary><see cref="SortSpec.ToConfig"/> 꼴. 기본 "Default:Asc"(게임 순서).</summary>
        public static ConfigEntry<string> Sort;

        public static ManualLogSource Log;

        public static bool IsListMode => ViewMode != null && ViewMode.Value == "List";

        public static SortSpec CurrentSort => SortSpec.Parse(Sort?.Value);

        private void Awake()
        {
            Log = Logger;
            ViewMode = Config.Bind("General", "ViewMode", "Grid",
                new ConfigDescription("상인 물건을 격자(Grid)로 볼지 목록(List)으로 볼지",
                    new AcceptableValueList<string>("Grid", "List")));
            Sort = Config.Bind("General", "Sort", SortSpec.DefaultAsc.ToConfig(),
                "정렬. 키는 Default·Price·Name·Stock·Loyalty, 방향은 Asc·Desc (예: Price:Asc)");

            new Harmony(PluginGuid).PatchAll(typeof(Plugin).Assembly);
            Log.LogInfo("상인 목록 보기 0.1.0 로드");
        }
    }
}
