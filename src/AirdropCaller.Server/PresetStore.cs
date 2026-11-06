using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;

namespace AirdropCaller.Server;

public record Preset
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string Icon { get; set; } = "Common";

    [JsonPropertyName("items")]
    public List<ItemSpec> Items { get; set; } = [];

    /// <summary>큰 상자로 보낼지. 겉모습은 <see cref="Icon"/> 이 정하고 격자만 커진다.</summary>
    [JsonPropertyName("bigCrate")]
    public bool BigCrate { get; set; }
}

public record PresetSaveRequest : IRequestData
{
    [JsonPropertyName("preset")]
    public Preset? Preset { get; set; }
}

public record PresetIdRequest : IRequestData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public record PresetListResponse
{
    [JsonPropertyName("presets")]
    public List<Preset> Presets { get; set; } = [];
}

/// <summary>
/// 프리셋은 **프로필과 무관하게 한 곳**에 둔다 — `presets/&lt;id&gt;.json`.
///
/// 설계 §17. 처음에는 `presets/&lt;profileId&gt;/` 였고(§7.11), 그 근거는 "프로필 하나를
/// 가정하지 않는다" 였다. 그 근거는 전역 프리셋을 막지 않는다 — 여러 프로필이 있다는
/// 전제였지 프리셋을 가둬야 한다는 뜻이 아니었다. 사용자가 2026-11-06 에 뒤집었다.
///
/// **대기열은 그대로 프로필 키를 쓴다**(`DropQueue`). 그쪽은 레이드 런타임 상태라
/// 전역으로 두면 남의 레이드에 떨어진다.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class PresetStore(ISptLogger<PresetStore> logger, ModPaths paths)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public List<Preset> List()
    {
        var dir = Dir;
        if (!Directory.Exists(dir))
        {
            return [];
        }

        var presets = new List<Preset>();
        foreach (var file in Directory.EnumerateFiles(dir, "*.json"))
        {
            try
            {
                var preset = JsonSerializer.Deserialize<Preset>(File.ReadAllText(file), Options);
                if (preset is not null)
                {
                    presets.Add(preset);
                }
            }
            catch (Exception e)
            {
                // 깨진 파일 하나가 목록 전체를 막으면 안 된다. 그것만 건너뛴다.
                logger.Error($"[AirdropCaller] 프리셋을 못 읽었다 ({Path.GetFileName(file)}): {e.Message}");
            }
        }

        presets.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCulture));
        return presets;
    }

    public bool Save(Preset preset)
    {
        if (string.IsNullOrWhiteSpace(preset.Id) || !IsSafeId(preset.Id))
        {
            logger.Error($"[AirdropCaller] 프리셋 id 가 비었거나 쓸 수 없는 글자를 쓴다: '{preset.Id}'");
            return false;
        }

        try
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(Path.Combine(Dir, preset.Id + ".json"), JsonSerializer.Serialize(preset, Options));
            return true;
        }
        catch (Exception e)
        {
            logger.Error($"[AirdropCaller] 프리셋을 못 적었다 ({preset.Id}): {e.Message}");
            return false;
        }
    }

    public bool Delete(string id)
    {
        if (!IsSafeId(id))
        {
            return false;
        }

        var file = Path.Combine(Dir, id + ".json");
        if (!File.Exists(file))
        {
            return false;
        }

        try
        {
            File.Delete(file);
            return true;
        }
        catch (Exception e)
        {
            logger.Error($"[AirdropCaller] 프리셋을 못 지웠다 ({id}): {e.Message}");
            return false;
        }
    }

    public Preset? Get(string id) =>
        IsSafeId(id) ? List().FirstOrDefault(p => p.Id == id) : null;

    /// <summary>
    /// 옛 `presets/&lt;profileId&gt;/` 를 `presets/` 로 올린다. 기동 때 한 번 부른다(설계 §17.3).
    ///
    /// **조용히 덮지 않는다.** id 가 겹치면 뒤에 오는 것을 `&lt;id&gt;-2`(그 다음은 `-3`…)로
    /// 바꾸고 이름 뒤에 ` (2)` 를 붙인 뒤, 무엇이 어디로 갔는지 로그에 한 줄씩 남긴다.
    /// 되돌릴 사람이 그 줄만 보고 알 수 있어야 한다.
    ///
    /// 던지지 않는다 — 이전이 실패해도 서버 기동을 막지 않는다. 그때는 옛 폴더가 그대로
    /// 남고 그 프리셋이 목록에 안 보일 뿐이다.
    /// </summary>
    public void MigrateToGlobal()
    {
        try
        {
            var root = Path.Combine(paths.ModFolder, "presets");
            if (!Directory.Exists(root))
            {
                return;
            }

            // 24자리 hex 폴더만 본다 — 프로필 id 모양이다. 그 밖의 폴더는 건드리지 않는다.
            var profileDirs = Directory.EnumerateDirectories(root)
                .Where(d => IsProfileIdFolder(Path.GetFileName(d)))
                .OrderBy(d => d, StringComparer.Ordinal)
                .ToList();
            if (profileDirs.Count == 0)
            {
                return;
            }

            Directory.CreateDirectory(root);
            var moved = 0;
            var renamed = 0;

            foreach (var dir in profileDirs)
            {
                foreach (var file in Directory.EnumerateFiles(dir, "*.json").OrderBy(f => f, StringComparer.Ordinal))
                {
                    var id = Path.GetFileNameWithoutExtension(file);
                    var target = Path.Combine(root, id + ".json");
                    var finalId = id;
                    var suffix = 1;

                    while (File.Exists(target))
                    {
                        suffix++;
                        finalId = $"{id}-{suffix}";
                        target = Path.Combine(root, finalId + ".json");
                    }

                    if (!string.Equals(finalId, id, StringComparison.Ordinal))
                    {
                        // 파일 이름만 바꾸면 안에 적힌 id 와 어긋난다. 둘 다 고친다.
                        RewriteId(file, target, finalId, suffix);
                        File.Delete(file);
                        renamed++;
                        logger.Warning(
                            $"[AirdropCaller] 프리셋 id 가 겹쳐 이름을 바꿨다: "
                            + $"{Path.GetFileName(dir)}/{id}.json → presets/{finalId}.json");
                    }
                    else
                    {
                        File.Move(file, target);
                    }

                    moved++;
                }

                if (!Directory.EnumerateFileSystemEntries(dir).Any())
                {
                    Directory.Delete(dir);
                }
            }

            logger.Info(
                $"[AirdropCaller] 프리셋 {moved}개를 프로필 폴더 {profileDirs.Count}개에서 "
                + $"presets/ 로 올렸다(이름 바꾼 것 {renamed}개). 이제 모든 프로필이 같은 목록을 본다.");
        }
        catch (Exception e)
        {
            logger.Error($"[AirdropCaller] 프리셋 이전이 실패했다: {e.Message}");
        }
    }

    private void RewriteId(string source, string target, string finalId, int suffix)
    {
        try
        {
            var preset = JsonSerializer.Deserialize<Preset>(File.ReadAllText(source), Options);
            if (preset is null)
            {
                File.Copy(source, target);
                return;
            }

            preset.Id = finalId;
            preset.Name = $"{preset.Name} ({suffix})";
            File.WriteAllText(target, JsonSerializer.Serialize(preset, Options));
        }
        catch (Exception e)
        {
            // 못 읽는 파일이라도 **잃지는 않는다.** 그대로 옮겨 두고 사람이 보게 한다.
            logger.Error($"[AirdropCaller] 겹친 프리셋을 고쳐 쓰지 못해 그대로 옮긴다 ({finalId}): {e.Message}");
            File.Copy(source, target, overwrite: true);
        }
    }

    private string Dir => Path.Combine(paths.ModFolder, "presets");

    /// <summary>프로필 id 모양(24자리 hex)인가. 이전 대상을 이것으로만 고른다.</summary>
    private static bool IsProfileIdFolder(string? name) =>
        name is { Length: 24 } && name.All(Uri.IsHexDigit);

    /// <summary>
    /// id 가 경로로 쓰일 자리를 만든다. 밖에서 오는 값이므로 `..` 나 구분자를 막는다.
    /// </summary>
    private static bool IsSafeId(string id) =>
        !string.IsNullOrWhiteSpace(id)
        && id.Length <= 64
        && id.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
}
