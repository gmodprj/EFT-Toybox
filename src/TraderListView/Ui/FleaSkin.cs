using System;
using System.Linq;
using EFT.UI.Ragfair;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TraderListView.Ui
{
    /// <summary>
    /// 플리마켓 겉모습(설계 §5, 방식 C). 플리마켓 매물 줄(<see cref="OfferView"/>) 부품에서
    /// 바탕·선택 색·글꼴만 빌려 목록 줄에 입힌다. 못 찾으면 아무것도 안 한다(B 모양 그대로).
    /// 부품 찾기(<c>Resources.FindObjectsOfTypeAll</c>)는 비싸므로 목록을 만들 때 한 번만 한다.
    /// </summary>
    public static class FleaSkin
    {
        private static bool _tried;
        private static bool _loaded;
        private static Sprite _backSprite;
        private static Image.Type _backType = Image.Type.Sliced;
        private static Color _backColor, _selectedColor;
        private static TMP_FontAsset _font;

        public static bool Loaded => _loaded;

        /// <summary>한 번만 찾는다. 성공하면 true.</summary>
        public static bool TryLoad()
        {
            if (_tried)
            {
                return _loaded;
            }

            _tried = true;
            try
            {
                // 플리 줄의 바탕은 루트 Image 가 아니라 _backgroundImages 이고, 실행 중 DefaultPanelColor 로 칠해진다.
                var offer = Resources.FindObjectsOfTypeAll<OfferView>()
                    .FirstOrDefault(o => o != null && o._backgroundImages != null && o._backgroundImages.Any(i => i != null));
                if (offer == null)
                {
                    Plugin.Log.LogInfo("플리 겉모습을 못 찾아 기본 모양으로 그린다");
                    return false;
                }

                var back = offer._backgroundImages.First(i => i != null);
                _backSprite = back.sprite;
                _backType = back.type;
                _backColor = OfferView.DefaultPanelColor;
                // _borderImage 는 "내 매물·살 수 없는 매물" 에만 켜지는 강조 테두리라 빌리지 않는다.

                _selectedColor = offer._selectedBackground != null ? offer._selectedBackground.color : new Color(1f, 0.85f, 0.45f, 0.18f);
                var label = offer.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t => t.font != null);
                _font = label != null ? label.font : null;
                _loaded = _backSprite != null || _font != null;
                Plugin.Log.LogInfo(_loaded ? "플리 겉모습을 빌렸다" : "플리 겉모습을 못 찾아 기본 모양으로 그린다");
                return _loaded;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("플리 겉모습 찾기 실패 — 기본 모양으로 그린다: " + e.Message);
                return false;
            }
        }

        public static void ApplyHeader(GameObject header)
        {
            if (!TryLoad())
            {
                return;
            }

            var image = header.GetComponent<Image>();
            if (image != null && _backSprite != null)
            {
                image.sprite = _backSprite;
                image.type = _backType;
                image.color = new Color(_backColor.r * 0.7f, _backColor.g * 0.7f, _backColor.b * 0.7f, Mathf.Max(_backColor.a, 0.9f));
            }

            ApplyFont(header);
        }

        public static void ApplyRow(GameObject row, bool alternate)
        {
            if (!_loaded)
            {
                return;
            }

            var image = row.GetComponent<Image>();
            if (image != null && _backSprite != null)
            {
                image.sprite = _backSprite;
                image.type = _backType;
                float k = alternate ? 0.85f : 1f;
                image.color = new Color(_backColor.r * k, _backColor.g * k, _backColor.b * k, _backColor.a);
            }

            var selected = row.transform.Find("Selected");
            var selectedImage = selected != null ? selected.GetComponent<Image>() : null;
            if (selectedImage != null)
            {
                selectedImage.color = _selectedColor;
            }

            ApplyFont(row);
        }

        private static void ApplyFont(GameObject root)
        {
            if (_font == null)
            {
                return;
            }

            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.font != _font)
                {
                    text.font = _font;
                }
            }
        }
    }
}
