using System.Globalization;

namespace DynamicMapsFactionMarkers
{
    /// <summary>
    /// 마커 하나를 그리는 데 필요한 것 — 색(0~1)과 아이콘 경로.
    /// Unity 를 모르는 순수 타입이라 EFT 없이 시험할 수 있다.
    /// 아이콘 경로는 Dynamic Maps 가 자기 폴더를 기준으로 읽으므로,
    /// 우리 파일은 `../DynamicMapsFactionMarkers/` 로 거슬러 올라가 가리킨다.
    /// </summary>
    public readonly struct MarkerStyle
    {
        public readonly float R;
        public readonly float G;
        public readonly float B;
        public readonly string IconPath;

        public MarkerStyle(float r, float g, float b, string iconPath)
        {
            R = r;
            G = g;
            B = b;
            IconPath = iconPath;
        }

        /// <summary>`#RRGGBB` 또는 `RRGGBB` 만 받는다. 그 밖은 false.</summary>
        public static bool TryParseColor(string hex, out float r, out float g, out float b)
        {
            r = g = b = 0f;

            if (string.IsNullOrEmpty(hex))
            {
                return false;
            }

            var body = hex[0] == '#' ? hex.Substring(1) : hex;
            if (body.Length != 6)
            {
                return false;
            }

            if (!int.TryParse(body, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var packed))
            {
                return false;
            }

            r = ((packed >> 16) & 0xFF) / 255f;
            g = ((packed >> 8) & 0xFF) / 255f;
            b = (packed & 0xFF) / 255f;
            return true;
        }

        /// <summary>모양 이름을 아이콘 경로로. 모르는 이름은 화살표로 둔다.</summary>
        public static string IconPathForShape(string shape)
        {
            switch (shape)
            {
                case "star":
                    return "Markers/star.png";
                case "dot":
                    return "Markers/dot.png";
                case "diamond":
                    return "../DynamicMapsFactionMarkers/diamond.png";
                default:
                    return "Markers/arrow.png";
            }
        }
    }
}
