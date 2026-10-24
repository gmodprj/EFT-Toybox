using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SptKoreanConfig
{
    /// <summary>한 설정 항목의 번역. 비어 있는 필드는 원문을 그대로 둔다.</summary>
    internal sealed class SettingTranslation
    {
        [JsonProperty("name")] public string Name;
        [JsonProperty("desc")] public string Desc;
    }

    /// <summary>모드 한 개의 번역 파일 내용.</summary>
    internal sealed class PluginTranslation
    {
        [JsonProperty("guid")] public string Guid;
        [JsonProperty("name")] public string Name;

        /// <summary>원문 섹션 이름 → 한글 섹션 이름.</summary>
        [JsonProperty("sections")] public Dictionary<string, string> Sections;

        /// <summary>"원문섹션/원문키" → 번역.</summary>
        [JsonProperty("settings")] public Dictionary<string, SettingTranslation> Settings;

        internal string SourceFile;

        public string SectionOf(string section)
        {
            if (Sections == null || section == null) return null;
            return Sections.TryGetValue(section, out var ko) && !string.IsNullOrEmpty(ko) ? ko : null;
        }

        public SettingTranslation SettingOf(string section, string key)
        {
            if (Settings == null || key == null) return null;
            return Settings.TryGetValue((section ?? string.Empty) + "/" + key, out var t) ? t : null;
        }
    }

    /// <summary>
    /// lang 폴더의 *.json 을 읽어 GUID 로 찾을 수 있게 들고 있는다.
    /// 파일이 바뀌면 게임을 끄지 않고 다시 읽는다(번역하면서 F12 만 다시 열면 반영된다).
    /// </summary>
    internal sealed class TranslationStore
    {
        private readonly string _dir;
        private Dictionary<string, PluginTranslation> _byGuid =
            new Dictionary<string, PluginTranslation>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, DateTime> _stamps = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        public int PluginCount { get; private set; }
        public int SettingCount { get; private set; }
        public int SectionCount { get; private set; }

        public TranslationStore(string dir)
        {
            _dir = dir;
        }

        public PluginTranslation Get(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            return _byGuid.TryGetValue(guid, out var t) ? t : null;
        }

        /// <summary>파일 구성이나 수정 시각이 달라졌을 때만 다시 읽는다. 다시 읽었으면 true.</summary>
        public bool ReloadIfChanged()
        {
            var now = Collect();

            if (now.Count == _stamps.Count)
            {
                var same = true;
                foreach (var kv in now)
                {
                    if (_stamps.TryGetValue(kv.Key, out var t) && t == kv.Value) continue;
                    same = false;
                    break;
                }
                if (same) return false;
            }

            Load(now);
            return true;
        }

        public void Reload()
        {
            Load(Collect());
        }

        /// <summary>모드 사전 파일과 수정 시각. 밑줄로 시작하는 파일(_ui.json 등)은 모드 사전이 아니다.</summary>
        private Dictionary<string, DateTime> Collect()
        {
            var now = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            if (!Directory.Exists(_dir)) return now;

            foreach (var f in Directory.GetFiles(_dir, "*.json", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(f).StartsWith("_", StringComparison.Ordinal)) continue;
                now[f] = File.GetLastWriteTimeUtc(f);
            }
            return now;
        }

        private void Load(Dictionary<string, DateTime> stamps)
        {
            var byGuid = new Dictionary<string, PluginTranslation>(StringComparer.OrdinalIgnoreCase);
            var settings = 0;
            var sections = 0;

            // 게임이 JsonConvert.DefaultSettings 에 컨버터를 꽂아 둘 수 있어 설정을 직접 준다.
            var jsonSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
            };

            foreach (var path in stamps.Keys)
            {
                try
                {
                    var text = File.ReadAllText(path, Encoding.UTF8);
                    var t = JsonConvert.DeserializeObject<PluginTranslation>(text, jsonSettings);
                    if (t == null) continue;

                    t.SourceFile = path;
                    if (string.IsNullOrEmpty(t.Guid))
                        t.Guid = Path.GetFileNameWithoutExtension(path);

                    byGuid[t.Guid] = t;
                    if (t.Settings != null) settings += t.Settings.Count;
                    if (t.Sections != null) sections += t.Sections.Count;
                }
                catch (Exception e)
                {
                    KoreanConfigPlugin.Log?.LogError($"번역 파일을 읽지 못했다: {path} — {e.Message}");
                }
            }

            _byGuid = byGuid;
            _stamps = stamps;
            PluginCount = byGuid.Count;
            SettingCount = settings;
            SectionCount = sections;
        }
    }
}
