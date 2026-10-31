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
        private static TMP_FontAsset _font;
        private static bool _fontChecked;
        private static bool _paletteLogged;
        private static float _pointerX;
        private static float _pointerY;

        private static float OuterRadius => WheelLayout.OuterRadius;

        private static float InnerRadius => WheelLayout.InnerRadius(Plugin.Settings.WheelDeadZone);

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
                _graphic.Style(_targets.Count, SelectedIndex, Pointer, InnerRadius, OuterRadius, _palette);
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

            if (!_paletteLogged)
            {
                _paletteLogged = true;
                Plugin.Log.LogInfo("휠 색은 " + _palette.Source + " 에서 가져왔다.");
            }

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

            // 구멍 가운데. 구멍이 곧 데드존이라 따로 그린 원과 겹칠 일이 없다.
            _centerLabel = MakeLabel("Center", "취소", Vector2.zero, new Vector2(InnerRadius * 1.7f, LabelHeight), CenterFontSize);
        }

        private static TextMeshProUGUI MakeLabel(string name, string text, Vector2 position, Vector2 size, float fontSize)
        {
            var holder = new GameObject(name, typeof(RectTransform));
            holder.transform.SetParent(_root.transform, false);
            Center(holder.GetComponent<RectTransform>(), position, size);

            var label = holder.AddComponent<TextMeshProUGUI>();

            // 글자가 이 휠의 **유일한** 식별 수단이다(아이콘은 설계에서 일부러 뺐다).
            // 항목 이름은 모두 한글이라, 기본 폰트에 한글 글리프가 없으면 조각이 빈칸으로 뜬다.
            // 게임이 쓰는 폰트를 빌려 쓴다 — 전에는 제스처 항목을 복제하며 딸려 왔던 것이다.
            var font = GameFont();
            if (font != null)
            {
                label.font = font;
            }

            label.text = text;
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = fontSize;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;
            label.color = _palette.Text;

            return label;
        }

        /// <summary>
        /// 게임 화면에 이미 떠 있는 글자가 쓰는 폰트. 한 번 찾아 들고 있는다.
        /// 못 찾으면 null — 그때는 TMP 기본 폰트로 간다. 글자가 빈칸으로 보이면
        /// 여기가 null 로 떨어진 것이고, 로그에 적힌다.
        /// </summary>
        private static TMP_FontAsset GameFont()
        {
            if (_fontChecked)
            {
                return _font;
            }

            _fontChecked = true;

            try
            {
                // 이미 화면에 떠 있는 글자가 쓰는 폰트를 먼저 본다 — 게임이 고른 것이니 생김새가 맞다.
                // 그 다음 불려 와 있는 폰트 전부를 본다. 한글은 폴백 폰트에만 있을 수 있어서,
                // 아무 글자에도 붙어 있지 않은 자산까지 뒤져야 걸린다.
                var inUse = Resources.FindObjectsOfTypeAll<TMP_Text>()
                    .Where(text => text != null && text.font != null)
                    .Select(text => text.font);

                var loaded = Resources.FindObjectsOfTypeAll<TMP_FontAsset>()
                    .Where(font => font != null);

                var candidates = inUse.Concat(loaded).Distinct().ToList();
                _font = candidates.FirstOrDefault(HasHangul);

                if (_font == null)
                {
                    Plugin.Log.LogWarning($"한글이 있는 게임 폰트를 찾지 못했다(후보 {candidates.Count}개). 기본 폰트로 그린다 — 항목 이름이 빈칸으로 보이면 이것이 까닭이다.");
                }
                else
                {
                    Plugin.Log.LogInfo($"휠 글자는 게임 폰트 {_font.name} 를 쓴다(후보 {candidates.Count}개 중).");
                }
            }
            catch (Exception e)
            {
                _font = null;
                Plugin.Log.LogWarning("게임 폰트를 찾다 멈췄다. 기본 폰트로 그린다: " + e.Message);
            }

            return _font;
        }

        /// <summary>
        /// 항목 이름에 쓰는 글자가 이 폰트로 그려지는가. '야' 하나로 본다.
        ///
        /// <c>searchFallbacks: true</c> 가 핵심이다. 기본값은 false 인데, 한글은 폰트 자신이 아니라
        /// **폴백 폰트**에 들어 있는 것이 보통이라 그대로 쓰면 멀쩡한 폰트를 다 떨어뜨린다
        /// (실제로 그렇게 떨어뜨렸다). <c>tryAddCharacter</c> 는 끈 채로 둔다 —
        /// 게임의 글자 아틀라스를 우리가 건드릴 일이 아니다.
        /// </summary>
        private static bool HasHangul(TMP_FontAsset font)
        {
            try
            {
                return font.HasCharacter('야', searchFallbacks: true);
            }
            catch (Exception)
            {
                return false;
            }
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
