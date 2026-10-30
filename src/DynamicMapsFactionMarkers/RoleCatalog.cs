using System;
using System.Collections.Generic;
using System.Globalization;

namespace DynamicMapsFactionMarkers
{
    /// <summary>역할 하나를 어떤 범주·모습으로 그릴지 정해진 결과.</summary>
    public sealed class RoleEntry
    {
        public int RoleId { get; }
        public string ScavRole { get; }
        public string Category { get; }
        public MarkerStyle Style { get; }

        public RoleEntry(int roleId, string scavRole, string category, MarkerStyle style)
        {
            RoleId = roleId;
            ScavRole = scavRole;
            Category = category;
            Style = style;
        }
    }

    /// <summary>
    /// 역할 id → 범주·색·모양. Unity 도 EFT 도 모르는 순수 코드라 EFT 없이 시험한다.
    /// 설정에 적힌 묶음이 먼저고, 없는 역할은 설정이 허락할 때만 팔레트에서 색을 받아 들어온다.
    /// </summary>
    public sealed class RoleCatalog
    {
        /// <summary>색을 못 읽었을 때 쓰는 색 — ISB 자홍.</summary>
        public const string DefaultColorHex = "#C400FF";

        /// <summary>설정에 없는 커스텀 역할에 돌려 쓰는 색.</summary>
        public static readonly string[] FallbackPalette =
        {
            "#C400FF", "#2EC8FF", "#B6FF00", "#00E5B0",
            "#FF9ED8", "#8A6BFF", "#FFFFFF", "#FF2D55",
        };

        /// <summary>Dynamic Maps 가 쓰는 범주 이름. 우리가 쓰면 바닐라 마커를 가로채게 된다.</summary>
        private static readonly string[] VanillaCategories =
        {
            "Friendly Player", "Enemy Player", "Boss", "Scav",
        };

        private readonly MarkerConfig _config;
        private readonly Dictionary<string, MarkerGroupConfig> _byScavRole =
            new Dictionary<string, MarkerGroupConfig>(StringComparer.Ordinal);
        private readonly HashSet<int> _extraRoles = new HashSet<int>();
        private readonly Dictionary<int, RoleEntry> _entries = new Dictionary<int, RoleEntry>();
        private readonly HashSet<string> _categories = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<RoleEntry> _discovered = new List<RoleEntry>();
        private bool _labelsApplied;

        public RoleCatalog(MarkerConfig config)
        {
            _config = config ?? MarkerConfig.Default();

            if (_config.Groups != null)
            {
                foreach (var group in _config.Groups)
                {
                    // 같은 ScavRole 이 여러 줄이면 먼저 적힌 줄이 이긴다.
                    if (group != null && !string.IsNullOrEmpty(group.ScavRole) && !_byScavRole.ContainsKey(group.ScavRole))
                    {
                        _byScavRole.Add(group.ScavRole, group);
                    }
                }
            }

            if (_config.ExtraRoles != null)
            {
                foreach (var roleId in _config.ExtraRoles)
                {
                    _extraRoles.Add(roleId);
                }
            }
        }

        /// <summary>자동 배정으로 들어온 것만.</summary>
        public IReadOnlyList<RoleEntry> Discovered => _discovered;

        /// <summary>
        /// 역할 하나를 표에 올린다. <paramref name="localizedLabel"/> 가 비어 있으면
        /// ScavRole 키를 범주 이름으로 쓴다. 같은 역할을 두 번 불러도 한 번만 올라간다.
        /// </summary>
        public void Register(int roleId, string scavRole, string localizedLabel)
        {
            if (_entries.ContainsKey(roleId))
            {
                return;
            }

            var configured = !string.IsNullOrEmpty(scavRole) && _byScavRole.TryGetValue(scavRole, out var group)
                ? group
                : null;

            if (configured == null && !_config.AutoAssignUnknown && !_extraRoles.Contains(roleId))
            {
                return;
            }

            var category = !string.IsNullOrEmpty(configured?.Label)
                ? configured.Label
                : (!string.IsNullOrEmpty(localizedLabel) ? localizedLabel : scavRole);

            if (string.IsNullOrEmpty(category) || IsVanillaCategory(category))
            {
                // 바닐라 범주와 이름이 겹치면 올리지 않는다 — 올리면 그들의 마커를 가로챈다.
                return;
            }

            var colorHex = configured != null ? configured.ColorHex : PaletteColorForNext();
            if (!MarkerStyle.TryParseColor(colorHex, out var r, out var g, out var b))
            {
                MarkerStyle.TryParseColor(DefaultColorHex, out r, out g, out b);
            }

            var shape = configured != null ? configured.Shape : "arrow";
            var entry = new RoleEntry(roleId, scavRole, category, new MarkerStyle(r, g, b, MarkerStyle.IconPathForShape(shape)));

            _entries.Add(roleId, entry);
            _categories.Add(category);

            if (configured == null)
            {
                _discovered.Add(entry);
            }
        }

        /// <summary>
        /// MoreBotsAPI 를 거치지 않고 들어온 역할(설정의 extraRoles). 이름을 알 길이 없으므로
        /// 역할 id 를 그대로 ScavRole 키로 쓴다 — config 의 groups 에 그 숫자를 적어 두면
        /// 거기 적은 이름·색·모양을 받는다.
        /// </summary>
        public void RegisterExtraRoles()
        {
            foreach (var roleId in _extraRoles)
            {
                Register(roleId, roleId.ToString(CultureInfo.InvariantCulture), null);
            }
        }

        /// <summary>
        /// 범주 이름을 locale 로 다시 짓는다. 게임 locale 은 BepInEx Awake 보다 늦게 올라오므로
        /// 레이드에서 처음 쓸 때 한 번 부른다. 두 번째부터는 아무 것도 하지 않고 false 를 돌려준다.
        /// </summary>
        public bool ApplyLabels(Func<string, string> lookup)
        {
            if (_labelsApplied || lookup == null)
            {
                return false;
            }

            _labelsApplied = true;

            var renamed = new List<RoleEntry>();
            foreach (var entry in _entries.Values)
            {
                var configured = !string.IsNullOrEmpty(entry.ScavRole) && _byScavRole.TryGetValue(entry.ScavRole, out var group)
                    ? group
                    : null;

                if (!string.IsNullOrEmpty(configured?.Label))
                {
                    renamed.Add(entry);
                    continue;
                }

                string label = null;
                try
                {
                    label = lookup(entry.ScavRole);
                }
                catch (Exception)
                {
                    // locale 을 못 읽으면 키를 그대로 쓴다.
                }

                var category = !string.IsNullOrEmpty(label) && !IsVanillaCategory(label) ? label : entry.ScavRole;
                renamed.Add(category == entry.Category
                    ? entry
                    : new RoleEntry(entry.RoleId, entry.ScavRole, category, entry.Style));
            }

            _entries.Clear();
            _categories.Clear();
            foreach (var entry in renamed)
            {
                _entries[entry.RoleId] = entry;
                _categories.Add(entry.Category);
            }

            return true;
        }

        public bool TryGet(int roleId, out RoleEntry entry)
        {
            return _entries.TryGetValue(roleId, out entry);
        }

        public bool IsOurCategory(string category)
        {
            return !string.IsNullOrEmpty(category) && !IsVanillaCategory(category) && _categories.Contains(category);
        }

        private string PaletteColorForNext()
        {
            return FallbackPalette[_discovered.Count % FallbackPalette.Length];
        }

        private static bool IsVanillaCategory(string category)
        {
            foreach (var vanilla in VanillaCategories)
            {
                if (string.Equals(vanilla, category, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
