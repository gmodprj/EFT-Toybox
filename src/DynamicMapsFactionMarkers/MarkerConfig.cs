using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DynamicMapsFactionMarkers
{
    /// <summary>묶음 한 줄 — 어떤 ScavRole 을 어떤 이름·색·모양으로 그릴지.</summary>
    public sealed class MarkerGroupConfig
    {
        [JsonProperty("scavRole")]
        public string ScavRole { get; set; }

        /// <summary>null 이면 서버 locale 의 표시 이름을 쓴다.</summary>
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("colorHex")]
        public string ColorHex { get; set; }

        /// <summary>star · arrow · diamond · dot</summary>
        [JsonProperty("shape")]
        public string Shape { get; set; }

        public MarkerGroupConfig()
        {
        }

        public MarkerGroupConfig(string scavRole, string colorHex, string shape)
        {
            ScavRole = scavRole;
            ColorHex = colorHex;
            Shape = shape;
        }
    }

    /// <summary>플러그인 폴더의 config.jsonc 를 그대로 옮긴 것.</summary>
    public sealed class MarkerConfig
    {
        /// <summary>false 면 역할 수집도 패치도 하지 않는다.</summary>
        [JsonProperty("enabled")]
        public bool Enabled { get; set; } = true;

        /// <summary>intel level 게이트를 무엇에 묶을지 — boss · scav · none.</summary>
        [JsonProperty("intelLevelSource")]
        public string IntelLevelSource { get; set; } = "boss";

        /// <summary>설정에 없는 커스텀 역할도 색을 자동 배정해 바로 그린다.</summary>
        [JsonProperty("autoAssignUnknown")]
        public bool AutoAssignUnknown { get; set; } = true;

        [JsonProperty("groups")]
        public List<MarkerGroupConfig> Groups { get; set; }

        /// <summary>MoreBotsAPI 를 거치지 않고 들어온 역할 id 를 손으로 적는 칸.</summary>
        [JsonProperty("extraRoles")]
        public List<int> ExtraRoles { get; set; }

        /// <summary>설계 §6-3 의 8묶음.</summary>
        public static MarkerConfig Default()
        {
            return new MarkerConfig
            {
                Groups = new List<MarkerGroupConfig>
                {
                    new MarkerGroupConfig("ISBBoss", "#FF2D55", "star"),
                    new MarkerGroupConfig("ISBWhiteTusk", "#FFFFFF", "star"),
                    new MarkerGroupConfig("ISBCommander", "#FFFFFF", "star"),
                    new MarkerGroupConfig("ISBBossFollower", "#FFFFFF", "star"),
                    new MarkerGroupConfig("ISBSniper", "#2EC8FF", "diamond"),
                    new MarkerGroupConfig("ISBAssassin", "#B6FF00", "diamond"),
                    new MarkerGroupConfig("ISBScout", "#00E5B0", "diamond"),
                    new MarkerGroupConfig("ISBHeavy", "#8A6BFF", "arrow"),
                    new MarkerGroupConfig("ISB", "#C400FF", "arrow"),
                    new MarkerGroupConfig("ISBGuard", "#FF9ED8", "arrow"),
                },
                ExtraRoles = new List<int>(),
            };
        }

        /// <summary>
        /// 깨진 파일은 기본값으로 떨어뜨리고 이유를 <paramref name="warning"/> 에 담는다.
        /// 지도가 멈추는 쪽보다 안 고쳐지는 쪽이 낫다.
        /// </summary>
        public static MarkerConfig LoadFromText(string jsonc, out string warning)
        {
            warning = null;

            if (string.IsNullOrEmpty(jsonc))
            {
                warning = "설정 파일이 비어 있다. 기본값으로 돈다.";
                return Default();
            }

            MarkerConfig parsed;
            try
            {
                parsed = JsonConvert.DeserializeObject<MarkerConfig>(jsonc);
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

            if (parsed.Groups == null || parsed.Groups.Count == 0)
            {
                parsed.Groups = Default().Groups;
            }

            if (parsed.ExtraRoles == null)
            {
                parsed.ExtraRoles = new List<int>();
            }

            if (string.IsNullOrEmpty(parsed.IntelLevelSource))
            {
                parsed.IntelLevelSource = "boss";
            }

            return parsed;
        }
    }
}
