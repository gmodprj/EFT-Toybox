using System.Reflection;
using System.Text.Json;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Server;

namespace WebConfigBridge;

/// <summary>
/// 같은 <c>user/mods</c> 안에 있는 다른 모드의 설정 파일을 읽고 쓴다.
/// 남의 파일을 다루므로 실패해도 던지지 않고 null·false 로 알린다.
///
/// 모드를 더하려면 <see cref="Targets"/> 에 한 줄을 더하고 POCO 를 하나 만들면 된다.
/// </summary>
[Injectable]
public class ConfigStore(ModHelper modHelper, ISptLogger<ConfigStore> logger)
{
    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
    };

    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    /// <summary>다룰 대상. 값은 화면과 코드가 같이 쓰는 식별자다.</summary>
    public static class Targets
    {
        public const string AllinWeaponUnslotted = "AllinWeaponUnslotted";
        public const string HelmetSlotUnion = "HelmetSlotUnion";
        public const string DynamicMaps = "DynamicMaps";
        public const string TradeMarker = "TradeMarker";
        public const string EquipmentIsEternal = "EquipmentIsEternal";
        public const string UltimateFleaMod = "UltimateFleaMod";
        public const string RealisticInsurance = "RealisticInsurance";
        public const string RepairSkillExtension = "RepairSkillExtension";
        public const string InfiniteEverything = "InfiniteEverything";
        public const string SlotCategoryRelax = "SlotCategoryRelax";
        public const string SlotCategoryRelaxDiscovered = "SlotCategoryRelaxDiscovered";
    }

    /// <summary>
    /// 대상 → (모드 폴더명, 모드 폴더 기준 상대경로).
    /// 모드 하나가 설정 파일을 여럿 가질 수 있으므로 키는 폴더명이 아니라 대상 식별자다.
    /// </summary>
    private static readonly Dictionary<string, (string Folder, string Relative)> RelativePaths = new()
    {
        [Targets.AllinWeaponUnslotted] = ("AllinWeaponUnslotted", Path.Combine("config", "config.jsonc")),
        [Targets.HelmetSlotUnion] = ("HelmetSlotUnion", "config.jsonc"),
        [Targets.DynamicMaps] = ("mpstark-dynamicmaps", "config.json"),
        [Targets.TradeMarker] = ("Moe-TradeMarker", "config.json"),
        [Targets.EquipmentIsEternal] = ("EquipmentIsEternal", "config.jsonc"),
        [Targets.UltimateFleaMod] = ("UltimateFlea", Path.Combine("config", "mod.json")),
        [Targets.RealisticInsurance] = ("RealisticInsurance", Path.Combine("config", "config.json")),
        [Targets.RepairSkillExtension] = ("RepairSkillExtension", "buffs.jsonc"),
        [Targets.InfiniteEverything] = ("InfiniteEverything", "state.json"),
        [Targets.SlotCategoryRelax] = ("SlotCategoryRelax", "config.jsonc"),
        [Targets.SlotCategoryRelaxDiscovered] = ("SlotCategoryRelax", "discovered-categories.json"),
    };

    /// <summary>대상 설정 파일의 절대경로. 모드나 파일이 없으면 null.</summary>
    public string? PathOf(string target)
    {
        if (!RelativePaths.TryGetValue(target, out var entry))
        {
            return null;
        }

        var (modFolderName, relative) = entry;

        var ownFolder = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        var modsRoot = Path.GetDirectoryName(ownFolder);
        if (string.IsNullOrEmpty(modsRoot))
        {
            return null;
        }

        var full = Path.Combine(modsRoot, modFolderName, relative);
        return File.Exists(full) ? full : null;
    }

    /// <summary>주석이 있는 JSONC 를 읽는다. 읽지 못하면 null.</summary>
    public T? Load<T>(string target)
        where T : class
    {
        var path = PathOf(target);
        if (path is null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(path), ReadOptions);
        }
        catch (Exception ex)
        {
            logger.Warning($"[WebConfigBridge] 설정을 읽지 못했다: {path} — {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 설정을 파일에 쓴다. 처음 쓸 때 원본을 <c>.orig</c> 로 한 번 남긴다 —
    /// 이 쓰기는 원본의 주석을 지우기 때문이다.
    /// </summary>
    public bool Save<T>(string target, T config)
    {
        var path = PathOf(target);
        if (path is null)
        {
            return false;
        }

        try
        {
            var backup = path + ".orig";
            if (!File.Exists(backup))
            {
                File.Copy(path, backup);
                logger.Info($"[WebConfigBridge] 원본을 남겼다: {backup}");
            }

            File.WriteAllText(path, JsonSerializer.Serialize(config, WriteOptions));
            logger.Info($"[WebConfigBridge] 설정을 저장했다: {path} (적용은 다음 서버 시작부터)");
            return true;
        }
        catch (Exception ex)
        {
            logger.Error($"[WebConfigBridge] 설정을 저장하지 못했다: {path} — {ex.Message}");
            return false;
        }
    }
}
