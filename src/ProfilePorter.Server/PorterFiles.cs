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
    /// <summary>
    /// 파일 이름(확장자 없이). **파일에는 적히지 않고** 목록을 만들 때 채운다 — 이름은 폴더가
    /// 가진 사실이지 파일 속 내용이 아니고, 사람이 파일을 옮기거나 바꿔 둘 수 있다.
    /// 창은 이 값으로 들여올 파일을 지목한다.
    /// </summary>
    [JsonPropertyName("fileName")] public string FileName { get; set; } = string.Empty;

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
    /// <summary>
    /// 파일을 읽고 쓰는 설정. <see cref="ProfilePorterService"/> 가 `upd` 를 담을 때도 같은 것을
    /// 쓴다 — 두 곳이 다르면 담을 때와 적을 때의 모양이 갈린다.
    /// </summary>
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        // 사람이 열어 보는 파일이다. 한국어 메모가 \uXXXX 로 깨지지 않게 한다.
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        // **비어 있는 필드는 적지 않는다.** SPT 의 `Upd` 는 쓰지 않는 필드까지 들고 있는 타입이라,
        // 그대로 적으면 원본이 2필드인 것이 파일에서는 28필드(26개가 null)가 된다. 값은 같지만
        // 파일이 부풀고 사람이 프로필과 견주기 어려워진다(2026-11-08 기동 확인에서 실측).
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
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
            var name = Path.GetFileNameWithoutExtension(path);
            var file = Read(name);
            if (file is null)
            {
                continue;
            }

            // 이름은 **폴더가 가진 사실**이다. 파일 속 값이 무엇이든 여기서 덮는다 —
            // 그래야 창이 지목한 이름으로 서버가 같은 파일을 연다.
            file.FileName = name;
            files.Add(file);
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

    /// <summary>같은 이름이 이미 있다는 뜻. 호출자가 사람에게 물을 수 있게 따로 알린다.</summary>
    public const string NameTakenReason = "같은 이름의 파일이 이미 있습니다.";

    /// <summary>
    /// 파일로 적는다. 이름을 돌려준다.
    ///
    /// **덮어쓰지 않는다.** 같은 이름이 있으면 <see cref="NameTakenReason"/> 으로 거절한다 —
    /// 내보낸 파일은 사람이 만든 것이고, 조용히 덮으면 앞의 것이 사라진 줄도 모른다.
    /// 덮어쓰려면 <paramref name="overwrite"/> 를 켜야 하고, 그것은 창이 묻고 나서 켠다.
    /// </summary>
    /// <param name="rename">켜면 거절하는 대신 `-2`·`-3` 을 붙여 **빈 이름을 찾아** 적는다.</param>
    public string? Write(PorterFile file, string? name, bool overwrite, bool rename, out string? refusal)
    {
        refusal = null;
        var safe = Sanitize(name) ?? DefaultName(file);

        try
        {
            Directory.CreateDirectory(Folder);

            if (File.Exists(Path.Combine(Folder, safe + ".json")))
            {
                if (rename)
                {
                    safe = FreeName(safe);
                }
                else if (!overwrite)
                {
                    refusal = NameTakenReason;
                    return null;
                }
            }

            File.WriteAllText(Path.Combine(Folder, safe + ".json"), JsonSerializer.Serialize(file, Options));
            return safe;
        }
        catch (Exception e)
        {
            logger.Error($"[ProfilePorter] 파일을 쓰지 못했다 {safe}.json — {e.Message}");
            refusal = $"파일을 쓰지 못했습니다: {e.Message}";
            return null;
        }
    }

    /// <summary>
    /// 이름을 안 적었을 때 쓰는 이름 — **보낸 프로필 + 날짜_시각**이다.
    /// 시각은 사람이 보는 **현지 시각**으로 둔다. 파일 속 `createdUtc` 는 UTC 라 둘이 다르지만,
    /// 이름은 사람이 목록에서 고르는 것이고 서버 로그와 맞출 값이 아니다.
    /// </summary>
    private static string DefaultName(PorterFile file)
    {
        var who = Sanitize(file.FromNickname) ?? "프로필";
        var kind = string.Equals(file.Kind, PorterBundle.KindLoadout, StringComparison.Ordinal) ? "착용장비" : "스태시";
        return $"{who}-{kind}-{DateTime.Now:yyyyMMdd_HHmmss}";
    }

    /// <summary>`이름-2`, `이름-3` … 으로 비어 있는 자리를 찾는다. AirdropCaller 의 프리셋 이전과 같은 꼴이다.</summary>
    private string FreeName(string baseName)
    {
        for (var n = 2; n < 1000; n++)
        {
            var candidate = $"{baseName}-{n}";
            if (!File.Exists(Path.Combine(Folder, candidate + ".json")))
            {
                return candidate;
            }
        }

        // 999개까지 찼으면 시각을 붙인다. 여기서 포기하지는 않는다.
        return $"{baseName}-{DateTime.Now:HHmmss}";
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
