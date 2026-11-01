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
        private static bool _diagnosed;

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
                    Diagnose("입력 프리셋이 아직 없다");
                    return null;
                }

                var key = SearchCombinations(preset, "_inputKeyCombinations")
                    ?? SearchCombinations(preset, "_workingKeyCombinations");

                if (key == null)
                {
                    Diagnose(DescribePreset(preset));
                }

                return key;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("입력 조합을 뒤지다 멈췄다: " + e.Message);
            }

            return null;
        }

        /// <summary>한 번만 적는다. 왜 못 찾았는지 알아야 고칠 수 있다.</summary>
        private static void Diagnose(string detail)
        {
            if (_diagnosed)
            {
                return;
            }

            _diagnosed = true;
            Plugin.Log.LogWarning("N 키를 못 찾은 까닭: " + detail);
        }

        private static string DescribePreset(object preset)
        {
            try
            {
                var combinationsField = AccessTools.Field(preset.GetType(), "_inputKeyCombinations");
                if (!(combinationsField?.GetValue(preset) is IEnumerable combinations))
                {
                    return "_inputKeyCombinations 를 읽지 못했다";
                }

                var total = 0;
                var withKeys = 0;
                var gogglesKeyCount = -1;

                foreach (var combination in combinations)
                {
                    if (combination == null)
                    {
                        continue;
                    }

                    total++;
                    var keys = ReadKeys(combination);
                    if (keys != null && keys.Length > 0)
                    {
                        withKeys++;
                    }

                    if (ReadGameKey(combination) == EGameKey.ToggleGoggles)
                    {
                        gogglesKeyCount = keys?.Length ?? -1;
                    }
                }

                return $"조합 {total}개 중 키가 묶인 것 {withKeys}개, ToggleGoggles 조합의 키 수 {gogglesKeyCount}" +
                       " (-1 이면 조합 자체를 못 찾은 것이다)";
            }
            catch (Exception e)
            {
                return "살펴보다 멈췄다: " + e.Message;
            }
        }

        private static IGameKey SearchCombinations(object preset, string fieldName)
        {
            var combinationsField = AccessTools.Field(preset.GetType(), fieldName);
            if (!(combinationsField?.GetValue(preset) is IEnumerable combinations))
            {
                return null;
            }

            foreach (var combination in combinations)
            {
                if (combination == null || ReadGameKey(combination) != EGameKey.ToggleGoggles)
                {
                    continue;
                }

                var keys = ReadKeys(combination);
                if (keys != null && keys.Length > 0)
                {
                    return keys[0];
                }
            }

            return null;
        }

        private static EGameKey ReadGameKey(object combination)
        {
            var type = combination.GetType();
            var value = AccessTools.Field(type, "GameKey")?.GetValue(combination)
                ?? AccessTools.Property(type, "GameKey")?.GetValue(combination, null);

            return value is EGameKey gameKey ? gameKey : EGameKey.None;
        }

        private static IGameKey[] ReadKeys(object combination)
        {
            var type = combination.GetType();
            var value = AccessTools.Field(type, "InputKeys")?.GetValue(combination)
                ?? AccessTools.Property(type, "InputKeys")?.GetValue(combination, null);

            return value as IGameKey[];
        }
    }
}
