using System.Text.Json.Serialization;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace ProfilePorter.Server;

// **열쇠 이름을 명시한다.** 없으면 `ProfileId` 처럼 PascalCase 로만 받고, 창이 보내는
// camelCase(`profileId`)는 **조용히 null 이 되어** "그 프로필을 찾지 못했습니다" 로 돌아온다.
// 2026-11-08 기동 확인에서 실제로 그렇게 거절당했다. AirdropCaller 의 `QueueRequest` 가
// 같은 이유로 모든 속성에 이것을 달고 있다.
public record PorterExportRequest : IRequestData
{
    [JsonPropertyName("profileId")] public string? ProfileId { get; set; }
    [JsonPropertyName("kind")] public string? Kind { get; set; }
    [JsonPropertyName("itemIds")] public List<string>? ItemIds { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("note")] public string? Note { get; set; }

    /// <summary>같은 이름이 있어도 덮어쓴다. **창이 사람에게 묻고 나서** 켠다.</summary>
    [JsonPropertyName("overwrite")] public bool Overwrite { get; set; }

    /// <summary>같은 이름이 있으면 `-2`·`-3` 을 붙여 새 이름으로 적는다.</summary>
    [JsonPropertyName("rename")] public bool Rename { get; set; }
}

public record PorterImportRequest : IRequestData
{
    [JsonPropertyName("profileId")] public string? ProfileId { get; set; }
    [JsonPropertyName("fileName")] public string? FileName { get; set; }
}

/// <summary>
/// `/profile-porter/*` 를 연다. 창(P4)이 부른다.
///
/// **바닐라 라우트에 끼어들지 않는다** — AirdropCaller 와 가장 다른 점이다. 이 모드는 게임이
/// 부르는 길에 아무것도 더하지 않고, 사람이 창에서 누를 때만 움직인다.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ProfilePorterRouter : StaticRouter
{
    public ProfilePorterRouter(
        JsonUtil jsonUtil,
        ISptLogger<ProfilePorterRouter> logger,
        ConfigStore store,
        ProfileReader profiles,
        PorterFiles files,
        ProfilePorterService service)
        : base(jsonUtil, BuildRoutes(jsonUtil, logger, store, profiles, files, service))
    {
    }

    private static List<RouteAction> BuildRoutes(
        JsonUtil jsonUtil,
        ISptLogger<ProfilePorterRouter> logger,
        ConfigStore store,
        ProfileReader profiles,
        PorterFiles files,
        ProfilePorterService service)
    {
        return
        [
            // 프로필 목록 — 창의 왼쪽·오른쪽 칸을 채운다.
            new RouteAction(
                "/profile-porter/profiles",
                (url, info, sessionId, output, ct) => ValueTask.FromResult<object>(
                    Json(jsonUtil, new { ok = store.Config.ModEnabled, profiles = profiles.List() }))),

            // 한 프로필의 스태시 바닥 — 무엇을 고를 수 있는지 보여 준다.
            new RouteAction<PorterImportRequest>(
                "/profile-porter/stash",
                (url, info, sessionId, output, ct) =>
                {
                    var pmc = profiles.Pmc(info?.ProfileId);
                    if (pmc?.Inventory is null)
                    {
                        return ValueTask.FromResult(Json(jsonUtil, new { ok = false, reason = "그 프로필을 찾지 못했습니다." }));
                    }

                    return ValueTask.FromResult(Json(jsonUtil, new
                    {
                        ok = true,
                        grid = profiles.StashSize(pmc),
                        items = profiles.StashFloor(pmc),
                    }));
                }),

            // 내보낸 파일 목록.
            new RouteAction(
                "/profile-porter/files",
                (url, info, sessionId, output, ct) => ValueTask.FromResult<object>(
                    Json(jsonUtil, new { ok = true, folder = files.Folder, files = files.List() }))),

            new RouteAction<PorterExportRequest>(
                "/profile-porter/export",
                (url, info, sessionId, output, ct) =>
                {
                    if (!Enabled(store, logger, out var off))
                    {
                        return ValueTask.FromResult(Json(jsonUtil, off!));
                    }

                    var result = string.Equals(info?.Kind, PorterBundle.KindLoadout, StringComparison.Ordinal)
                        ? service.ExportLoadout(info?.ProfileId, info?.Name, info?.Note, info?.Overwrite ?? false, info?.Rename ?? false)
                        : service.ExportStash(info?.ProfileId, info?.ItemIds, info?.Name, info?.Note, info?.Overwrite ?? false, info?.Rename ?? false);

                    return ValueTask.FromResult(Json(jsonUtil, result));
                }),

            new RouteAction<PorterImportRequest>(
                "/profile-porter/import",
                (url, info, sessionId, output, ct) =>
                {
                    if (!Enabled(store, logger, out var off))
                    {
                        return ValueTask.FromResult(Json(jsonUtil, off!));
                    }

                    return ValueTask.FromResult(
                        Json(jsonUtil, service.Import(info?.ProfileId, info?.FileName)));
                }),
        ];
    }

    private static bool Enabled(ConfigStore store, ISptLogger<ProfilePorterRouter> logger, out object? refusal)
    {
        if (store.Config.ModEnabled)
        {
            refusal = null;
            return true;
        }

        logger.Warning("[ProfilePorter] 꺼져 있어 요청을 거절했다.");
        refusal = new PorterResult(false, "모드가 꺼져 있습니다(config.jsonc 의 ModEnabled).", null, 0, []);
        return false;
    }

    private static string Json(JsonUtil jsonUtil, object value) => jsonUtil.Serialize(value) ?? "{}";
}
