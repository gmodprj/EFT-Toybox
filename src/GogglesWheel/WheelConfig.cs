using System;
using Newtonsoft.Json;

namespace GogglesWheel
{
    /// <summary>플러그인 폴더의 config.jsonc 를 그대로 옮긴 것.</summary>
    public sealed class WheelConfig
    {
        public const float MinLongPress = 0.05f;
        public const float MaxLongPress = 2.0f;
        public const float MinDeadZone = 0.0f;
        public const float MaxDeadZone = 0.9f;

        /// <summary>false 면 패치를 걸지 않는다(바닐라 그대로).</summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;

        /// <summary>이 시간을 넘겨 누르면 "길게" 로 친다.</summary>
        [JsonProperty("longPressSeconds")]
        public float LongPressSeconds { get; set; } = 0.25f;

        /// <summary>짧게 눌렀을 때 — nightvision · faceshield · first.</summary>
        [JsonProperty("defaultTarget")]
        public string DefaultTarget { get; set; } = "nightvision";

        /// <summary>이 길이 미만으로 기울이면 아무 것도 고르지 않는다.</summary>
        [JsonProperty("wheelDeadZone")]
        public float WheelDeadZone { get; set; } = 0.2f;

        /// <summary>제스처 휠 항목 템플릿을 복제해 쓸지.</summary>
        [JsonProperty("useGameWheelSkin")]
        public bool UseGameWheelSkin { get; set; } = true;

        /// <summary>힌지로 여닫이가 생긴 헬멧 자신을 항목에 넣을지.</summary>
        [JsonProperty("includeHelmetVisor")]
        public bool IncludeHelmetVisor { get; set; } = true;

        /// <summary>COTI 수정키가 눌린 N 을 COTI 에 넘길지.</summary>
        [JsonProperty("standDownForCoti")]
        public bool StandDownForCoti { get; set; } = true;

        public static WheelConfig Default()
        {
            return new WheelConfig();
        }

        /// <summary>
        /// 깨진 파일은 기본값으로 떨어뜨리고 이유를 <paramref name="warning"/> 에 담는다.
        /// 값이 범위를 벗어나면 조용히 눌러 쓴다 — 0 초 길게누름 같은 것으로 멈추지 않는다.
        /// </summary>
        public static WheelConfig LoadFromText(string jsonc, out string warning)
        {
            warning = null;

            if (string.IsNullOrEmpty(jsonc))
            {
                warning = "설정 파일이 비어 있다. 기본값으로 돈다.";
                return Default();
            }

            WheelConfig parsed;
            try
            {
                parsed = JsonConvert.DeserializeObject<WheelConfig>(jsonc);
            }
            catch (Exception e)
            {
                warning = "설정 파일을 읽지 못했다. 기본값으로 돈다: " + e.Message;
                return Default();
            }

            if (parsed == null)
            {
                warning = "설정 파일에 내용이 없다. 기본값으로 돈다.";
                return Default();
            }

            parsed.LongPressSeconds = Clamp(parsed.LongPressSeconds, MinLongPress, MaxLongPress);
            parsed.WheelDeadZone = Clamp(parsed.WheelDeadZone, MinDeadZone, MaxDeadZone);

            if (!IsKnownTarget(parsed.DefaultTarget))
            {
                parsed.DefaultTarget = "nightvision";
            }

            return parsed;
        }

        private static bool IsKnownTarget(string value)
        {
            return string.Equals(value, "nightvision", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "faceshield", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "first", StringComparison.OrdinalIgnoreCase);
        }

        private static float Clamp(float value, float min, float max)
        {
            if (float.IsNaN(value))
            {
                return min;
            }

            return value < min ? min : (value > max ? max : value);
        }
    }
}
