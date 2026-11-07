using System;
using EFT.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace pitTeam.Modules
{
    /// <summary>
    /// 복제한 <c>DefaultUIButton</c> 의 글씨가 **한 글자만 보이던 것**을 고친다.
    ///
    /// 2026-11-07 인게임 실측 — 금색 버튼 둘이 같은 증상이었다:
    ///   "장비 없이"       → "장"   (용병 추가 화면)
    ///   "프로필 불러오기"  → "프"   (명단 화면)
    ///
    /// 둘 다 템플릿을 <c>Instantiate</c> 하고 <c>SetRawText</c> 로 글을 넣은 뒤 버튼 자신의
    /// <c>sizeDelta</c> 만 키운 것이다. **안쪽 글자 상자는 템플릿의 것 그대로**라 폭이 한 글자치였고,
    /// 줄바꿈이 켜져 있어 글자마다 줄이 바뀌면서 첫 줄 한 글자만 보였다(한국어는 아무 데서나 줄이
    /// 바뀐다 — 영어였다면 단어 하나가 통째로 넘어가 증상이 달랐을 것이다).
    ///
    /// 그래서 글을 넣은 **뒤에** 이 함수를 부른다: 안쪽 글자 상자를 버튼에 맞춰 늘이고, 줄바꿈을 끄고,
    /// 넘치면 잘리지 않게 두고, 배치를 즉시 다시 계산한다. 버튼 자체의 크기·색·동작은 건드리지 않는다.
    /// </summary>
    internal static class StmButtonLabelFit
    {
        /// <param name="padding">글자 상자를 버튼 안쪽에서 얼마나 띄울지(좌우).</param>
        internal static void Fit(DefaultUIButton button, float padding = 10f)
        {
            if (button == null) return;

            try
            {
                RectTransform buttonRect = button.transform as RectTransform;
                if (buttonRect == null) return;

                TMP_Text[] labels = button.GetComponentsInChildren<TMP_Text>(true);
                foreach (TMP_Text label in labels)
                {
                    if (label == null) continue;

                    if (label.transform is RectTransform labelRect && labelRect != buttonRect)
                    {
                        labelRect.anchorMin = Vector2.zero;
                        labelRect.anchorMax = Vector2.one;
                        labelRect.pivot = new Vector2(0.5f, 0.5f);
                        labelRect.offsetMin = new Vector2(padding, 0f);
                        labelRect.offsetMax = new Vector2(-padding, 0f);
                    }

                    // 줄바꿈을 끄는 것이 이 고침의 핵심이다. 폭이 모자라도 한 줄로 그린다.
                    label.enableWordWrapping = false;
                    label.overflowMode = TextOverflowModes.Overflow;
                    label.alignment = TextAlignmentOptions.Center;
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate(buttonRect);
            }
            catch (Exception ex)
            {
                // 글씨 손질이 실패해도 버튼은 눌린다. 화면을 죽이지 않는다.
                pitFireTeam.Log.LogWarning("[STM] 버튼 글씨를 맞추다 실패했다: " + ex.Message);
            }
        }
    }
}
