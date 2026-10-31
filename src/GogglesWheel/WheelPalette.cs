using System;
using HarmonyLib;
using UnityEngine;

namespace GogglesWheel
{
    /// <summary>
    /// 게임이 쓰는 색을 그대로 빌려 온다. 우리 색을 새로 고르면 그 순간부터 남의 화면이 된다.
    ///
    /// 자리는 실측으로 확인했다 — `EFT.UI.InteractionButton` 의 private static 색 셋과
    /// `EFT.UI.DragAndDrop.ItemView.DefaultSelectedColor`. 설치된
    /// `Tylevo.FieldAttachments` 도 같은 자리에서 같은 값을 읽는다(그쪽 `TarkovUiPalette`).
    /// 못 읽으면 이 설치에서 역컴파일로 본 값으로 떨어진다.
    /// </summary>
    public sealed class WheelPalette
    {
        /// <summary>눌리지 않은 바탕. 게임에서는 검정이다.</summary>
        public Color Normal = new Color(0f, 0f, 0f, 1f);

        /// <summary>흐린 테두리.</summary>
        public Color Disabled = new Color(0.25f, 0.25f, 0.25f, 1f);

        /// <summary>고른 것의 바탕.</summary>
        public Color Selected = new Color(0.514f, 0.514f, 0.514f, 1f);

        /// <summary>덧칠하는 하이라이트. 게임에서는 흰색에 알파 77 이다.</summary>
        public Color Highlight = new Color32(255, 255, 255, 77);

        /// <summary>어디서 읽었는지. 로그에 한 번 적어 둔다.</summary>
        public string Source = "역컴파일로 본 상수";

        /// <summary>글자색. 하이라이트의 색만 쓰고 알파는 꽉 채운다.</summary>
        public Color Text => new Color(Highlight.r, Highlight.g, Highlight.b, 1f);

        public static WheelPalette Read()
        {
            var palette = new WheelPalette();

            try
            {
                var button = AccessTools.TypeByName("EFT.UI.InteractionButton");
                var itemView = AccessTools.TypeByName("EFT.UI.DragAndDrop.ItemView");

                if (TryColor(button, "NormalColor", out var normal)
                    && TryColor(button, "DisabledColor", out var disabled)
                    && TryColor(button, "SelectedColor", out var selected)
                    && TryColor(itemView, "DefaultSelectedColor", out var highlight))
                {
                    palette.Normal = normal;
                    palette.Disabled = disabled;
                    palette.Selected = selected;
                    palette.Highlight = highlight;
                    palette.Source = "게임의 InteractionButton · ItemView";
                }
            }
            catch (Exception e)
            {
                // 색을 못 읽는 것으로 휠이 안 뜨게 하지는 않는다. 기본값이 그 값과 같다.
                Plugin.Log.LogInfo("게임 색을 읽지 못해 상수로 그린다: " + e.Message);
            }

            return palette;
        }

        private static bool TryColor(Type type, string field, out Color color)
        {
            color = default;

            var info = type == null ? null : AccessTools.Field(type, field);
            if (info == null || info.FieldType != typeof(Color))
            {
                return false;
            }

            color = (Color)info.GetValue(null);
            return true;
        }
    }
}
