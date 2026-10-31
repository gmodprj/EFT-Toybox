using System;
using System.Collections.Generic;
using System.Linq;
using EFT.UI.Gestures;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GogglesWheel
{
    /// <summary>
    /// 길게 누르면 뜨는 원형 메뉴. 제스처 휠의 항목 템플릿을 **복제**해 쓴다 —
    /// 제스처 휠 자체는 건드리지 않는다(설계 §4 안티골).
    /// 템플릿을 못 찾으면 우리가 만든 단순한 항목으로 떨어진다. 사라지지는 않는다.
    /// </summary>
    public static class WheelView
    {
        private const float Radius = 170f;
        private const float ItemSize = 96f;

        private static GameObject _root;
        private static readonly List<GameObject> _items = new List<GameObject>();
        private static IReadOnlyList<ToggleTarget> _targets;

        public static bool IsOpen { get; private set; }

        /// <summary>지금 가리키고 있는 항목. 가운데면 -1.</summary>
        public static int SelectedIndex { get; private set; } = -1;

        public static void Open(IReadOnlyList<ToggleTarget> targets)
        {
            Close(false);

            _targets = targets;
            SelectedIndex = -1;

            try
            {
                Build(targets);
                IsOpen = true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("휠을 띄우지 못했다: " + e.Message);
                Teardown();
                IsOpen = false;
                throw;
            }
        }

        /// <summary>매 프레임 부른다. 쌓인 시점 입력으로 가리키는 항목을 정한다.</summary>
        public static void UpdateSelection(Vector2 direction)
        {
            if (!IsOpen || _targets == null)
            {
                return;
            }

            // 화면 크기와 무관하게 같은 손맛이 나도록 한 번 나눠 준다.
            var scaled = direction / 6f;
            SelectedIndex = TargetPicker.PickByDirection(scaled.x, scaled.y, _targets.Count, Plugin.Settings.WheelDeadZone);

            for (var i = 0; i < _items.Count; i++)
            {
                var selected = i == SelectedIndex;
                _items[i].transform.localScale = Vector3.one * (selected ? 1.15f : 0.95f);

                foreach (var image in _items[i].GetComponentsInChildren<Image>(true))
                {
                    var color = image.color;
                    color.a = selected ? 1f : 0.55f;
                    image.color = color;
                }
            }
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

        private static void Build(IReadOnlyList<ToggleTarget> targets)
        {
            _root = new GameObject("GogglesWheel");
            UnityEngine.Object.DontDestroyOnLoad(_root);

            var canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            _root.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

            var template = FindTemplate();

            for (var i = 0; i < targets.Count; i++)
            {
                var angle = TargetPicker.AngleForIndex(i, targets.Count) * Mathf.Deg2Rad;
                var position = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Radius;

                var item = template != null ? CloneTemplate(template) : MakePlainItem();
                item.transform.SetParent(_root.transform, false);

                var rect = item.GetComponent<RectTransform>() ?? item.AddComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = position;
                rect.sizeDelta = new Vector2(ItemSize * 1.6f, ItemSize * 0.7f);

                SetLabel(item, targets[i].Name);
                item.SetActive(true);
                _items.Add(item);
            }
        }

        /// <summary>제스처 휠의 항목 템플릿. 메뉴가 닫혀 있어도 찾을 수 있게 비활성까지 뒤진다.</summary>
        private static GameObject FindTemplate()
        {
            if (!Plugin.Settings.UseGameWheelSkin)
            {
                return null;
            }

            try
            {
                var menu = Resources.FindObjectsOfTypeAll<GesturesMenu>().FirstOrDefault();
                var template = menu?._gestureItemTemplate;
                return template == null ? null : template.gameObject;
            }
            catch (Exception e)
            {
                Plugin.Log.LogInfo("제스처 휠 템플릿을 못 찾아 단순한 항목으로 그린다: " + e.Message);
                return null;
            }
        }

        private static GameObject CloneTemplate(GameObject template)
        {
            var clone = UnityEngine.Object.Instantiate(template);

            // 템플릿의 제스처 전용 부품은 떼어 낸다 — Show() 를 부르지 않으므로
            // 그 스크립트들이 돌면서 제 것을 덮어쓰지 못하게 한다.
            foreach (var behaviour in clone.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour != null && !(behaviour is Graphic) && !(behaviour is TMP_Text))
                {
                    UnityEngine.Object.Destroy(behaviour);
                }
            }

            return clone;
        }

        private static GameObject MakePlainItem()
        {
            var item = new GameObject("Item", typeof(RectTransform));
            var background = item.AddComponent<Image>();
            background.color = new Color(0.07f, 0.08f, 0.09f, 0.92f);

            var labelObject = new GameObject("Label", typeof(RectTransform));
            labelObject.transform.SetParent(item.transform, false);

            var label = labelObject.AddComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 22f;
            label.color = Color.white;

            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            return item;
        }

        /// <summary>
        /// 항목 이름을 넣는다. 복제본의 필드 이름에 기대지 않고 **처음 보이는 글자 부품**에 넣는다 —
        /// 게임이 업데이트돼 필드 이름이 바뀌어도 글자는 나온다.
        /// </summary>
        private static void SetLabel(GameObject item, string text)
        {
            var tmp = item.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault();
            if (tmp != null)
            {
                tmp.text = text;
                return;
            }

            var legacy = item.GetComponentsInChildren<Text>(true).FirstOrDefault();
            if (legacy != null)
            {
                legacy.text = text;
            }
        }

        private static void Teardown()
        {
            _items.Clear();
            _targets = null;

            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root);
                _root = null;
            }
        }
    }
}
