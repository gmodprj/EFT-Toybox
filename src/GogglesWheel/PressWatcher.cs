using System;
using System.Collections;
using System.Reflection;
using EFT.InputSystem;
using HarmonyLib;

namespace GogglesWheel
{
    /// <summary>
    /// N 에 실제로 묶인 키의 눌림 상태를 게임 입력 시스템에서 그대로 읽는다.
    /// 물리 키코드를 우리가 들고 있지 않으므로 사용자가 키를 바꿔도 따라간다.
    /// </summary>
    public static class PressWatcher
    {
        private static IGameKey _key;
        private static bool _warned;

        /// <summary>
        /// ToggleGoggles 조합의 첫 키. 못 찾으면 false.
        ///
        /// **찾지 못한 결과는 캐시하지 않는다** — 입력 프리셋은 메뉴에서 키를 바꾸면 다시 만들어지므로,
        /// 한 번 실패했다고 영영 포기하면 그 판 내내 길게 누르기가 죽는다.
        /// </summary>
        public static bool TryGetGogglesKey(out IGameKey key)
        {
            if (_key == null)
            {
                _key = FindGogglesKey();

                if (_key == null && !_warned)
                {
                    _warned = true;
                    Plugin.Log.LogWarning("N 키 조합을 찾지 못했다. 길게 누르기를 쓸 수 없어 기본 대상만 여닫는다.");
                }
            }

            key = _key;
            return key != null;
        }

        /// <summary>키를 다시 찾게 한다. 사용자가 키 설정을 바꿨을 때 쓴다.</summary>
        public static void Forget()
        {
            _key = null;
        }

        /// <summary>지금 눌려 있는가. 키를 못 찾으면 false(= 코드가 부른 호출로 친다).</summary>
        public static bool IsHeldNow()
        {
            if (!TryGetGogglesKey(out var key))
            {
                return false;
            }

            try
            {
                var press = key.Press;
                return press == EKeyPress.Down || press == EKeyPress.Hold;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("키 상태를 읽지 못했다: " + e.Message);
                return false;
            }
        }

        private static IGameKey FindGogglesKey()
        {
            try
            {
                var presetField = AccessTools.Field(typeof(InputManager), "_currentPreset");
                var preset = presetField?.GetValue(null);
                if (preset == null)
                {
                    return null;
                }

                var combinationsField = AccessTools.Field(preset.GetType(), "_inputKeyCombinations");
                if (!(combinationsField?.GetValue(preset) is IEnumerable combinations))
                {
                    return null;
                }

                foreach (var combination in combinations)
                {
                    if (combination == null)
                    {
                        continue;
                    }

                    var type = combination.GetType();
                    var gameKey = AccessTools.Field(type, "GameKey")?.GetValue(combination)
                        ?? AccessTools.Property(type, "GameKey")?.GetValue(combination, null);

                    if (!(gameKey is EGameKey egk) || egk != EGameKey.ToggleGoggles)
                    {
                        continue;
                    }

                    var keys = AccessTools.Field(type, "InputKeys")?.GetValue(combination)
                        ?? AccessTools.Property(type, "InputKeys")?.GetValue(combination, null);

                    if (keys is IGameKey[] array && array.Length > 0)
                    {
                        return array[0];
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("입력 조합을 뒤지다 멈췄다: " + e.Message);
            }

            return null;
        }
    }
}
