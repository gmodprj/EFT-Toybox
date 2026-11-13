using System;
using System.Collections.Generic;
using System.Linq;
using pitTeam.Server.Models;
using pitTeam.Shared;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils.Cloners;

namespace pitTeam.Server.Services;

// 2026-11-13 사용자 요청 — 편집기의 "꾸러미로 저장". 그 팀원이 **지금 입고 있는 것**을
// 그 팀원 프로필의 장비 빌드(`userbuilds.equipmentBuilds`)로 담는다.
//
// **아이템을 옮기지 않는다.** 빌드는 본뜬 것이라 스태시 전송과 달리 총량 보존을 걱정할 자리가 없다 —
// 담아도 입고 있는 것은 그대로 있고, 빌드를 지워도 아무것도 사라지지 않는다. 그래서 저널도 쓰지 않는다.
//
// 읽는 것은 **디스크에 저장된 그 프로필**이다(화면이 보낸 트리가 아니다). 편집 중인 변경은 창이
// 먼저 저장(`apply-inventory`)을 끝낸 뒤 이 라우트를 부르므로 그때 함께 담긴다.
//
// 라우트 안에서 불리므로 던지지 않는다. false 일 때 refusalReason 은 비지 않는다.
[Injectable(InjectionType.Singleton)]
public class StmGearBuildSaveService(
    StmProfileMemberResolver resolver,
    SaveServer saveServer,
    ICloner cloner,
    ISptLogger<StmGearBuildSaveService> logger)
{
    public bool TrySave(
        MongoId sessionId,
        StmGearBuildSaveRequest request,
        out StmGearBuildSaveResponse? result,
        out string? refusalReason)
    {
        result = null;
        refusalReason = null;

        try
        {
            // 해석기를 무엇보다 먼저 묻는다(불변식 3).
            var outcome = resolver.Lookup(sessionId, request.Aid, out var member, out var unavailableReason);
            if (outcome != ProfileLookupOutcome.Resolved || member == null)
            {
                refusalReason = outcome switch
                {
                    ProfileLookupOutcome.ReadFailed => "이 팀원이 프로필 팀원인지 확인할 수 없어 꾸러미로 담지 않습니다. 잠시 뒤 다시 시도하세요.",
                    ProfileLookupOutcome.NonMember => "이 팀원은 프로필 팀원이 아니어서 이 경로로 담을 수 없습니다.",
                    _ => $"이 프로필을 지금 쓸 수 없어 꾸러미로 담지 않습니다({unavailableReason ?? "Unavailable"}).",
                };
                return false;
            }

            if (!string.Equals(member.ProfileId, request.ProfileId?.Trim(), StringComparison.Ordinal))
            {
                logger.Error($"[STM] 꾸러미 담기 거부: 요청의 프로필 '{request.ProfileId}' 과(와) Aid '{request.Aid}' 로 해석된 프로필 '{member.ProfileId}' 이(가) 다르다.");
                refusalReason = "요청한 프로필과 이 팀원의 프로필이 달라 담지 않습니다.";
                return false;
            }

            var profileId = new MongoId(member.ProfileId);
            SptProfile? profile = saveServer.GetProfile(profileId);
            var pmc = profile?.CharacterData?.PmcData;
            var equipmentRoot = pmc?.Inventory?.Equipment;
            if (profile == null || pmc?.Inventory?.Items == null || equipmentRoot == null)
            {
                refusalReason = "이 팀원의 프로필을 읽지 못해 담지 않습니다.";
                return false;
            }

            // 입고 있는 것 = 장비 뿌리에 매달린 것 전부(뿌리 자신은 빼고).
            var worn = Descendants(pmc.Inventory.Items, equipmentRoot.Value.ToString());

            var existingNames = profile.UserBuildData?.EquipmentBuilds?
                .Where(build => build != null)
                .Select(build => build!.Name ?? string.Empty)
                .ToList() ?? [];

            // 받아들일지와 이름은 순수 판정 한 곳에서 정한다(GearBuildSavePolicyTests 가 지킨다).
            var plan = GearBuildSavePolicy.Decide(member.Nickname, worn.Count, existingNames, DateTime.Now);
            if (!plan.Accepted)
            {
                refusalReason = plan.Reason;
                return false;
            }

            // **값으로 베껴** 담는다. SaveServer 가 들고 있는 살아 있는 목록을 빌드에 꽂으면
            // 그 뒤의 편집이 빌드까지 바꾼다(불변식 1).
            var copied = cloner.Clone(worn) ?? [];

            var build = new EquipmentBuild
            {
                Id = new MongoId(),
                Name = plan.Name,
                Root = equipmentRoot.Value,
                Items = copied,
            };

            profile.UserBuildData ??= new UserBuilds();
            profile.UserBuildData.EquipmentBuilds ??= [];
            profile.UserBuildData.EquipmentBuilds.Add(build);

            saveServer.SaveProfileAsync(profileId).GetAwaiter().GetResult();

            result = new StmGearBuildSaveResponse
            {
                Id = build.Id.ToString(),
                Name = plan.Name,
                ItemCount = copied.Count,
            };
            logger.Info($"[STM] 꾸러미로 담았다: 팀원 '{member.ProfileId}' — '{plan.Name}', 아이템 {copied.Count}개.");
            return true;
        }
        catch (Exception ex)
        {
            logger.Error($"[STM] 꾸러미 담기 실패 (Aid '{request.Aid}', 세션 '{sessionId}'): {ex}");
            refusalReason = "꾸러미로 담지 못했습니다. 서버 로그를 확인하세요.";
            return false;
        }
    }

    /// <summary>그 뿌리에 매달린 것 전부(뿌리는 빼고). 고리가 있어도 멈춘다(손상된 프로필).</summary>
    private static List<Item> Descendants(List<Item> items, string rootId)
    {
        var children = new Dictionary<string, List<Item>>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            var parent = item?.ParentId;
            if (string.IsNullOrEmpty(parent)) continue;
            if (!children.TryGetValue(parent!, out var list))
            {
                list = [];
                children[parent!] = list;
            }

            list.Add(item!);
        }

        var result = new List<Item>();
        var taken = new HashSet<string>(StringComparer.Ordinal) { rootId };
        var queue = new Queue<string>();
        queue.Enqueue(rootId);
        while (queue.Count > 0)
        {
            if (!children.TryGetValue(queue.Dequeue(), out var kids)) continue;
            foreach (var kid in kids)
            {
                var id = kid.Id.ToString();
                if (!taken.Add(id)) continue;
                result.Add(kid);
                queue.Enqueue(id);
            }
        }

        return result;
    }
}
