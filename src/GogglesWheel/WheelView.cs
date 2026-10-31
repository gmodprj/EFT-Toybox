using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GogglesWheel
{
    /// <summary>
    /// 길게 누르면 뜨는 원형 메뉴. 고리를 조각으로 나눠 직접 그린다(<see cref="WheelGraphic"/>).
    /// 색은 게임에서 빌려 오고(<see cref="WheelPalette"/>), 가운데에는 지금 가리키는 것의
    /// 이름이 뜬다. 가리키는 것이 없으면 "취소".
    ///
    /// 제스처 휠 자체는 건드리지 않는다(설계 §4 안티골). 전에는 제스처 항목을 복제해
    /// 썼는데, 제스처용 아이콘과 좁은 글자칸이 딸려 와 보기 흉했다(실측). 그 길은 지웠다.
    /// </summary>
    public static class WheelView
    {
        private const float OuterRadius = 210f;
        private const float InnerRadius = 112f;
        private const float LabelWidth = 150f;
        private const float LabelHeight = 44f;
        private const float LabelFontSize = 23f;
        private const float CenterFontSize = 21f;

        /// <summary>1920×1080 를 기준으로 재어 화면 크기가 달라도 같은 비율로 뜬다.</summary>
        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

        private static GameObject _root;
        private static WheelGraphic _graphic;
        private static TextMeshProUGUI _centerLabel;
        private static readonly List<TextMeshProUGUI> _labels = new List<TextMeshProUGUI>();
        private static IReadOnlyList<ToggleTarget> _targets;
        private static WheelPalette _palette;
        private static float _pointerX;
        private static float _pointerY;

        public static bool IsOpen { get; private set; }

        /// <summary>지금 가리키고 있는 항목. 가운데면 -1.</summary>
        public static int SelectedIndex { get; private set; } = -1;

        /// <summary>가상 커서가 원 안에서 어디에 있는지(-1 ~ 1).</summary>
        public static Vector2 Pointer => new Vector2(_pointerX, _pointerY);

        public static void Open(IReadOnlyList<ToggleTarget> targets)
        {
            Close(false);

            _targets = targets;
            SelectedIndex = -1;
            _pointerX = 0f;
            _pointerY = 0f;

            try
            {
                Build(targets);
                IsOpen = true;
                Paint();
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("휠을 띄우지 못했다: " + e.Message);
                Teardown();
                IsOpen = false;
                throw;
            }
        }

        /// <summary>
        /// 매 프레임 부른다. 마우스가 움직인 양을 가상 커서에 쌓고, 커서가 가리키는 항목을 정한다.
        /// 값은 <c>Input.GetAxisRaw("Mouse X" / "Mouse Y")</c> 에서 온다 — 게임 시점 축이 아니다.
        /// </summary>
        public static void UpdateSelection(float mouseX, float mouseY)
        {
            if (!IsOpen || _targets == null)
            {
                return;
            }

            var next = WheelPointer.Advance(_pointerX, _pointerY, mouseX, mouseY, Plugin.Settings.MouseSensitivity);
            _pointerX = next.X;
            _pointerY = next.Y;

            SelectedIndex = TargetPicker.PickByDirection(_pointerX, _pointerY, _targets.Count, Plugin.Settings.WheelDeadZone);
            Paint();
        }

        public static void Close(bool applySelection)
        {
            if (!applySelection)
            {
                SelectedIndex = -1;
            }

            Teardown();
            IsOpen = false;
        }

        /// <summary>고른 것에 따라 달라지는 것만 고쳐 칠한다.</summary>
        private static void Paint()
        {
            if (_graphic != null)
            {
                _graphic.Style(
                    _targets.Count,
                    SelectedIndex,
                    Pointer,
                    InnerRadius,
                    OuterRadius,
                    Plugin.Settings.WheelDeadZone * OuterRadius,
                    _palette);
            }

            for (var i = 0; i < _labels.Count; i++)
            {
                _labels[i].color = i == SelectedIndex ? _palette.Text : WithAlpha(_palette.Text, 0.55f);
            }

            if (_centerLabel != null)
            {
                var picked = SelectedIndex >= 0 && SelectedIndex < _targets.Count;
                _centerLabel.text = picked ? _targets[SelectedIndex].Name : "취소";
                _centerLabel.color = picked ? _palette.Text : WithAlpha(_palette.Text, 0.6f);
            }
        }

        private static void Build(IReadOnlyList<ToggleTarget> targets)
        {
            _palette = WheelPalette.Read();

            _root = new GameObject("GogglesWheel", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
            UnityEngine.Object.DontDestroyOnLoad(_root);

            var canvas = _root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            var scaler = _root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // 우리 휠은 보여 주기만 한다. 클릭을 삼켜 게임의 다른 UI 를 막아서는 안 된다.
            var group = _root.GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;

            var ring = new GameObject("Ring", typeof(RectTransform), typeof(CanvasRenderer), typeof(WheelGraphic));
            ring.transform.SetParent(_root.transform, false);
            Center(ring.GetComponent<RectTransform>(), Vector2.zero, new Vector2(OuterRadius * 2f, OuterRadius * 2f));

            _graphic = ring.GetComponent<WheelGraphic>();
            _graphic.raycastTarget = false;

            var labelRadius = (InnerRadius + OuterRadius) / 2f;
            for (var i = 0; i < targets.Count; i++)
            {
                var angle = TargetPicker.AngleForIndex(i, targets.Count) * Mathf.Deg2Rad;
                var position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * labelRadius;

                _labels.Add(MakeLabel("Item" + i, targets[i].Name, position, new Vector2(LabelWidth, LabelHeight), LabelFontSize));
            }

            // 데드존 원과 글자가 겹치지 않게 원 아래에 둔다. 구멍(InnerRadius) 안에는 들어온다.
            var centerLabelY = -(Plugin.Settings.WheelDeadZone * OuterRadius + LabelHeight * 0.75f);
            _centerLabel = MakeLabel("Center", "취소", new Vector2(0f, centerLabelY), new Vector2(InnerRadius * 1.7f, LabelHeight), CenterFontSize);
        }

        private static TextMeshProUGUI MakeLabel(string name, string text, Vector2 position, Vector2 size, float fontSize)
        {
            var holder = new GameObject(name, typeof(RectTransform));
            holder.transform.SetParent(_root.transform, false);
            Center(holder.GetComponent<RectTransform>(), position, size);

            var label = holder.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = fontSize;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            label.color = _palette.Text;

            return label;
        }

        private static void Center(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        private static void Teardown()
        {
            _labels.Clear();
            _targets = null;
            _graphic = null;
            _centerLabel = null;

            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root);
                _root = null;
            }
        }
    }
}
