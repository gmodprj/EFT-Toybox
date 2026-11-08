using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Utils;

namespace ProfilePorter.Server;

/// <summary>파일 하나의 머리글. 알맹이는 <see cref="PorterBundle"/> 이다(설계 §4.1).</summary>
public sealed class PorterFile
{
    [JsonPropertyName("format")] public string Format { get; set; } = CurrentFormat;
    [JsonPropertyName("kind")] public string Kind { get; set; } = PorterBundle.KindStash;
    [JsonPropertyName("createdUtc")] public string CreatedUtc { get; set; } = string.Empty;
    [JsonPropertyName("fromProfileId")] public string FromProfileId { get; set; } = string.Empty;
    [JsonPropertyName("fromNickname")] public string FromNickname { get; set; } = string.Empty;
    [JsonPropertyName("note")] public string? Note { get; set; }
    [JsonPropertyName("rootIds")] public List<string> RootIds { get; set; } = [];
    [JsonPropertyName("items")] public List<PorterFileItem> Items { get; set; } = [];

    public const string CurrentFormat = "profile-porter/1";
}

/// <summary>
/// 파일에 적히는 아이템. 프로필의 `Item` 과 **같은 이름**을 쓴다(`_id`·`_tpl`) — 사람이 열어
/// 보고 프로필과 견줄 수 있어야 한다.
/// </summary>
public sealed class PorterFileItem
{
    [JsonPropertyName("_id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("_tpl")] public string TemplateId { get; set; } = string.Empty;
    [JsonPropertyName("parentId")] public string? ParentId { get; set; }
    [JsonPropertyName("slotId")] public string? SlotId { get; set; }

    /// <summary>
    /// 그대로 싣고 그대로 꺼낸다(설계 §4.1·§9.2). `JsonElement` 로 두는 것은 **해석하지 않기**
    /// 위해서다 — 모양을 아는 척하면 모르는 필드가 조용히 떨어진다.
    /// </summary>
    [JsonPropertyName("upd")] public JsonElement? Upd { get; set; }
}

/// <summary>
/// 내보낸 파일을 `<모드폴더>/porter/` 에 읽고 쓴다. 백업도 여기서 뜬다.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class PorterFiles(ModHelper modHelper, ISptLogger<PorterFiles> logger)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        // 사람이 열어 보는 파일이다. 한국어 메모가 \uXXXX 로 깨지지 않게 한다.
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private string ModFolder => modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

    public string Folder => Path.Combine(ModFolder, "porter");

    /// <summary>`porter/` 의 파일 목록. 못 읽는 것은 건너뛰고 적는다.</summary>
    public List<PorterFile> List()
    {
        var files = new List<PorterFile>();
        if (!Directory.Exists(Folder))
        {
            return files;
        }

        foreach (var path in Directory.GetFiles(Folder, "*.json").OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
        {
            var file = Read(Path.GetFileNameWithoutExtension(path));
            if (file is not null)
            {
                files.Add(file);
            }
        }

        return files;
    }

    public PorterFile? Read(string? name)
    {
        var path = PathFor(name);
        if (path is null || !File.Exists(path))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<PorterFile>(File.ReadAllText(path), Options);
        }
        catch (Exception e)
        {
            logger.Error($"[ProfilePorter] 파일을 읽지 못했다 {Path.GetFileName(path)} — {e.Message}");
            return null;
        }
    }

    /// <summary>파일로 적는다. 이름을 돌려준다.</summary>
    public string? Write(PorterFile file, string? name)
    {
        var safe = Sanitize(name) ?? $"{file.Kind}-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        try
        {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(Path.Combine(Folder, safe + ".json"), JsonSerializer.Serialize(file, Options));
            return safe;
        }
        catch (Exception e)
        {
            logger.Error($"[ProfilePorter] 파일을 쓰지 못했다 {safe}.json — {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 들여오기 전에 받는 쪽 프로필을 통째로 베껴 둔다(설계 §4.6).
    ///
    /// **백업이 실패하면 들여오기도 하지 않는다** — 호출자가 null 을 받고 멈춘다. 되돌릴 수
    /// 없는 쓰기 앞에서 안전장치가 없는 채로 나아가지 않는다.
    /// </summary>
    public string? BackupProfile(string profileId)
    {
        try
        {
            var source = Path.Combine(AppContext.BaseDirectory, "user", "profiles", $"{profileId}.json");
            if (!File.Exists(source))
            {
                logger.Error($"[ProfilePorter] 백업할 프로필 파일이 없다: {source}");
                return null;
            }

            var dir = Path.Combine(ModFolder, "backups", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
            Directory.CreateDirectory(dir);
            var target = Path.Combine(dir, $"{profileId}.json");
            File.Copy(source, target, overwrite: false);
            logger.Info($"[ProfilePorter] 들여오기 전 프로필을 백업했다 → {target}");
            return target;
        }
        catch (Exception e)
        {
            logger.Error($"[ProfilePorter] 백업에 실패해 들여오지 않는다 — {e.Message}");
            return null;
        }
    }

    private string? PathFor(string? name)
    {
        var safe = Sanitize(name);
        return safe is null ? null : Path.Combine(Folder, safe + ".json");
    }

    /// <summary>
    /// 파일 이름에서 경로를 떼어 낸다. 밖에서 오는 글자라 `..\..\` 같은 것이 섞일 수 있다 —
    /// 그대로 이으면 모드 폴더 밖을 쓴다.
    /// </summary>
    private static string? Sanitize(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var bare = Path.GetFileName(name.Trim());
        if (string.IsNullOrEmpty(bare) || bare is "." or "..")
        {
            return null;
        }

        foreach (var bad in Path.GetInvalidFileNameChars())
        {
            bare = bare.Replace(bad, '_');
        }

        return bare.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            ? bare[..^5]
            : bare;
    }
}
