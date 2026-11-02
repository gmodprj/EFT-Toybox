using System;
using System.Collections.Generic;
using System.Linq;
using pitTeam.Server.Models;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;

namespace pitTeam.Server.Services;

// 계획 4 Task 1 — 장비 꾸러미에 보일 **저장된 장비 빌드** 목록을 읽는다. 읽기 전용이다.
//
// 빌드는 프로필의 뿌리에 있다: SptProfile.UserBuildData.EquipmentBuilds (`userbuilds.equipmentBuilds`).
// PMC 문서가 아니라 프로필 문서라서, 팀원 쪽도 SaveServer 에서 그 프로필을 직접 꺼내 읽는다.
//
// 내보내는 것은 **id 와 이름뿐**이다. 빌드의 Items 는 SaveServer 가 들고 있는 살아 있는 목록이므로
// 참조를 밖으로 넘기지 않는다(불변식 1). 값 매기기와 적용은 Task 2·3 이고, 그때도 복제본 위에서 한다.
//
// 라우트 안에서 불리므로 던지지 않는다. false 일 때 refusalReason 은 비지 않는다.
[Injectable(InjectionType.Singleton)]
public class StmGearBuildCatalog(
    StmProfileMemberResolver resolver,
    SaveServer saveServer,
    ISptLogger<StmGearBuildCatalog> logger)
{
    private static string DescribeLookupFailure(ProfileLookupOutcome outcome, string? unavailableReason) => outcome switch
    {
        ProfileLookupOutcome.ReadFailed => "이 팀원이 프로필 팀원인지 확인할 수 없어 빌드 목록을 보내지 않습니다. 잠시 뒤 다시 시도하세요.",
        ProfileLookupOutcome.NonMember => "이 팀원은 프로필 팀원이 아니어서 이 경로로 빌드를 볼 수 없습니다.",
        _ => $"이 프로필을 지금 쓸 수 없어 빌드 목록을 보내지 않습니다({unavailableReason ?? "Unavailable"}).",
    };

    public bool TryList(MongoId sessionId, string? accountId, string? requestedProfileId, out StmGearBuildsResponse? builds, out string? refusalReason)
    {
        builds = null;
        refusalReason = null;
        try
        {
            // 해석기를 PIT 보다, 그리고 다른 무엇보다 먼저 묻는다(불변식 3).
            var outcome = resolver.Lookup(sessionId, accountId, out var member, out var unavailableReason);
            if (outcome != ProfileLookupOutcome.Resolved || member == null)
            {
                refusalReason = DescribeLookupFailure(outcome, unavailableReason);
                return false;
            }

            // 요청이 말한 프로필과 해석된 프로필이 다르면 보내지 않는다(스태시 읽기와 같은 약속).
            if (!string.Equals(member.ProfileId, requestedProfileId?.Trim(), StringComparison.Ordinal))
            {
                logger.Error($"[STM] 빌드 목록 거부: 요청의 프로필 '{requestedProfileId}' 과(와) Aid '{accountId}' 로 해석된 프로필 '{member.ProfileId}' 이(가) 다르다.");
                refusalReason = "요청한 프로필과 이 팀원의 프로필이 달라 빌드 목록을 보내지 않습니다.";
                return false;
            }

            var teammate = ReadBuilds(member.ProfileId, "팀원");
            var player = ReadBuilds(sessionId.ToString(), "플레이어");

            // 두 묶음을 가르는 것도, 고를 수 없는 빌드를 빼는 것도 순수 정책 한 곳이다.
            var list = GearBuildListPolicy.Build(teammate, player);

            builds = new StmGearBuildsResponse
            {
                TeammateBuilds = list.TeammateBuilds.Select(ToRow).ToList(),
                PlayerBuilds = list.PlayerBuilds.Select(ToRow).ToList(),
                TeammateNickname = member.Nickname,
                ProfileId = member.ProfileId,
            };

            logger.Info($"[STM] 빌드 목록: 팀원 '{member.ProfileId}' {builds.TeammateBuilds.Count}개, 플레이어 {builds.PlayerBuilds.Count}개.");
            return true;
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 빌드 목록 읽기 실패 (Aid '{accountId}', 세션 '{sessionId}'): {ex}");
            refusalReason = "빌드 목록을 읽지 못했습니다. 서버 로그를 확인하세요.";
            return false;
        }
    }

    private static StmGearBuildRow ToRow(GearBuildRow row) => new() { Id = row.Id, Name = row.Name };

    // 한 프로필의 장비 빌드를 값만 베껴 온다. 살아 있는 Items 목록은 개수만 세고 참조는 버린다.
    // 프로필이 없거나 빌드 칸이 비어 있으면 빈 목록이다 - 그것은 실패가 아니라 "저장해 둔 빌드가 없다" 다.
    private List<GearBuildCandidate> ReadBuilds(string profileId, string whose)
    {
        var rows = new List<GearBuildCandidate>();
        if (string.IsNullOrWhiteSpace(profileId)) return rows;

        SptProfile? profile;
        try
        {
            profile = saveServer.GetProfile(new MongoId(profileId));
        }
        catch (Exception ex)
        {
            // 한쪽을 못 읽어도 다른 쪽 목록은 보여 준다. 빈 묶음과 "읽지 못했다" 를 화면이 가릴 수 없으므로
            // 사실은 로그에만 남긴다 - 목록이 비어 보이는 것이 틀린 빌드를 입히는 것보다 안전하다.
            logger.Warning($"[STM] 빌드 목록: {whose} 프로필 '{profileId}' 을(를) 읽지 못해 빈 목록으로 둔다: {ex.Message}");
            return rows;
        }

        var builds = profile?.UserBuildData?.EquipmentBuilds;
        if (builds == null) return rows;

        foreach (var build in builds)
        {
            if (build == null) continue;
            rows.Add(new GearBuildCandidate(build.Id.ToString(), build.Name, build.Items?.Count ?? 0));
        }

        return rows;
    }
}
