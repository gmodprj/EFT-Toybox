using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;

namespace ProfilePorter.Server;

/// <summary>프로필 하나의 간추린 모습. 창이 목록을 그리는 데 쓴다.</summary>
public sealed record ProfileSummary(string ProfileId, string Nickname, int Level, int StashFloorCount);

/// <summary>
/// 프로필을 **읽기만** 한다. 설계의 안티목표 1("내보내는 쪽 프로필이 바뀐다")이 여기 걸린다 —
/// 이 클래스에는 쓰는 메서드가 없다.
///
/// SPT 의 <see cref="Item"/> 을 판정이 쓰는 <see cref="PorterItem"/> 으로 줄인다. 판정(§P2)은
/// SPT 타입을 모르고, 그래야 게임 없이 시험할 수 있다.
/// </summary>
[Injectable(InjectionType.Singleton)]
public class ProfileReader(SaveServer saveServer, ISptLogger<ProfileReader> logger)
{
    /// <summary>이 서버가 들고 있는 프로필 전부. 못 읽는 것은 건너뛰고 적는다.</summary>
    public List<ProfileSummary> List()
    {
        var summaries = new List<ProfileSummary>();

        foreach (var (id, profile) in saveServer.GetProfiles())
        {
            var pmc = profile?.CharacterData?.PmcData;
            if (pmc?.Inventory is null)
            {
                logger.Warning($"[ProfilePorter] 프로필 {id} 를 읽지 못해 목록에서 뺐다.");
                continue;
            }

            var floor = (pmc.Inventory.Items ?? [])
                .Count(i => Same(i.ParentId, pmc.Inventory.Stash?.ToString())
                            && Same(i.SlotId, ExportSelection.StashSlot));

            summaries.Add(new ProfileSummary(
                id.ToString(),
                pmc.Info?.Nickname ?? "(이름 없음)",
                pmc.Info?.Level ?? 0,
                floor));
        }

        return summaries;
    }

    /// <summary>그 프로필의 PMC. 없으면 null — 던지지 않는다(밖에서 오는 id 로 불린다).</summary>
    public PmcData? Pmc(string? profileId)
    {
        if (!LooksLikeProfileId(profileId))
        {
            return null;
        }

        // `MongoId` 는 `TryParse` 가 없고 생김새가 틀리면 던진다. 밖에서 오는 글자라 먼저 본다.
        return saveServer.GetProfile(new MongoId(profileId))?.CharacterData?.PmcData;
    }

    /// <summary>24자리 소문자 hex 인지. 프로필 파일 이름이 쓰는 모양과 같다.</summary>
    public static bool LooksLikeProfileId(string? value) =>
        value is { Length: 24 } && value.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');

    /// <summary>판정이 쓰는 모양으로 줄인다. <c>Upd</c> 는 **들여다보지 않고 그대로** 들고 간다.</summary>
    public static List<PorterItem> ToPorterItems(IEnumerable<Item>? items) =>
        (items ?? [])
        .Where(i => i is not null)
        .Select(i => new PorterItem(
            i.Id.ToString(),
            i.ParentId,
            i.SlotId,
            i.Template.ToString(),
            i.Upd))
        .ToList();

    private static bool Same(string? a, string? b) =>
        a is not null && b is not null && string.Equals(a, b, StringComparison.Ordinal);
}
