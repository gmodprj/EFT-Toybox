using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace GridWindowScroll
{
    /// <summary>
    /// 가방·상자·트렁크를 열었을 때 뜨는 내부 인벤토리 창이 화면을 넘어가지 않게 한다.
    /// 보이는 칸 수를 가로·세로로 못박고, 그보다 큰 격자는 스크롤로 넘겨 본다.
    /// </summary>
    [BepInPlugin(PluginGuid, "인벤토리 격자 스크롤", "0.1.0")]
    public class GridWindowScrollPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "spt.gridwindowscroll";

        internal static ManualLogSource Log;

        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<int> MaxColumns;
        internal static ConfigEntry<int> MaxRows;
        internal static ConfigEntry<float> WheelCells;
        internal static ConfigEntry<bool> UseStencilMask;
        internal static ConfigEntry<bool> Verbose;

        private void Awake()
        {
            Log = Logger;

            Enabled = Config.Bind("일반", "켜기", true,
                "끄면 창이 다시 격자 크기 그대로 커진다. 이미 열려 있는 창은 닫고 다시 열어야 한다.");

            MaxColumns = Config.Bind("일반", "가로 최대 칸", 12,
                new ConfigDescription(
                    "창에 한 번에 보일 가로 칸 수. 격자가 이보다 넓으면 좌우 스크롤이 생긴다.",
                    new AcceptableValueRange<int>(3, 40)));

            MaxRows = Config.Bind("일반", "세로 최대 칸", 10,
                new ConfigDescription(
                    "창에 한 번에 보일 세로 칸 수. 격자가 이보다 높으면 위아래 스크롤이 생긴다.",
                    new AcceptableValueRange<int>(3, 40)));

            WheelCells = Config.Bind("일반", "휠 한 번에 넘길 칸", 2f,
                new ConfigDescription(
                    "마우스 휠을 한 칸 돌릴 때 움직일 칸 수.",
                    new AcceptableValueRange<float>(0.5f, 10f)));

            UseStencilMask = Config.Bind("고급", "스텐실 마스크 사용", false,
                "칸이 창 밖으로 비어져 나와 보이면 켠다. 기본 방식(RectMask2D)을 Mask 로 바꾼다. 켜고 끈 뒤에는 창을 다시 열어야 한다.");

            Verbose = Config.Bind("고급", "자세한 기록", false,
                "창을 열 때마다 격자 크기와 자른 크기를 BepInEx 로그에 남긴다.");

            new Harmony(PluginGuid).PatchAll(typeof(GridWindowPatch));
            Log.LogInfo($"EFT.UI.GridWindow.ShowContents 패치 완료 — 보이는 칸 최대 {MaxColumns.Value}x{MaxRows.Value}");
        }
    }
}
