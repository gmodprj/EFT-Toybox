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
/// 프리셋을 프로필마다 따로 둔다 — `presets/&lt;profileId&gt;/&lt;id&gt;.json`.
///
/// 프로필 하나를 가정하지 않는다. 비용이 거의 없고, 나중에 프로필을 늘렸을 때
/// 설계를 다시 보지 않아도 된다.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class PresetStore(ISptLogger<PresetStore> logger, ModPaths paths)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public List<Preset> List(string profileId)
    {
        var dir = DirFor(profileId);
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

    public bool Save(string profileId, Preset preset)
    {
        if (string.IsNullOrWhiteSpace(preset.Id) || !IsSafeId(preset.Id))
        {
            logger.Error($"[AirdropCaller] 프리셋 id 가 비었거나 쓸 수 없는 글자를 쓴다: '{preset.Id}'");
            return false;
        }

        try
        {
            var dir = DirFor(profileId);
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, preset.Id + ".json"), JsonSerializer.Serialize(preset, Options));
            return true;
        }
        catch (Exception e)
        {
            logger.Error($"[AirdropCaller] 프리셋을 못 적었다 ({preset.Id}): {e.Message}");
            return false;
        }
    }

    public bool Delete(string profileId, string id)
    {
        if (!IsSafeId(id))
        {
            return false;
        }

        var file = Path.Combine(DirFor(profileId), id + ".json");
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

    public Preset? Get(string profileId, string id) =>
        IsSafeId(id) ? List(profileId).FirstOrDefault(p => p.Id == id) : null;

    private string DirFor(string profileId) =>
        Path.Combine(paths.ModFolder, "presets", IsSafeId(profileId) ? profileId : "default");

    /// <summary>
    /// id 가 경로로 쓰일 자리를 만든다. 밖에서 오는 값이므로 `..` 나 구분자를 막는다.
    /// </summary>
    private static bool IsSafeId(string id) =>
        !string.IsNullOrWhiteSpace(id)
        && id.Length <= 64
        && id.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
}
