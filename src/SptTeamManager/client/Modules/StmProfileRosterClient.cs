using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pitTeam.Patches;
using pitTeam.Shared;
using SPT.Common.Http;

namespace pitTeam.Modules
{
    // Client side of the STM profile-member roster (design 6-3, left list).
    //
    // Every call here is contained: nothing throws to the caller. A failure comes back as
    // Ok == false with a message, and the squad screen draws PIT's mercenary roster regardless.
    //
    // Nothing is cached. Load() asks the server for the registration list already joined against
    // a fresh catalog read; no field of this class keeps a roster, a row, or a candidate between
    // calls (design 4: a stored join is a way to "ghost assets").
    internal static class StmProfileRosterClient
    {
        internal const string RosterRoute = "/stm/members/roster";
        internal const string AddRoute = "/stm/members/add";
        internal const string RemoveRoute = "/stm/members/remove";
        // PIT 의 라우트다. 서버가 그 안에서 STM 을 먼저 묻는다.
        internal const string AutoJoinRoute = "/singleplayer/pitfireteam/teammate/autojoin";

        // ---- Display text, in one place --------------------------------------------------------
        internal const string TileCaption = "프로필 팀원";
        internal const string LoadButtonText = "프로필 불러오기";
        internal const string ReasonEmpty = "게임에 한 번 들어가야 합니다";
        internal const string ReasonUnreadable = "프로필을 읽을 수 없습니다";
        internal const string ReasonMissing = "프로필이 더 이상 없습니다";
        // 레이드에 무엇을 입고 나가는가. 서버가 SpawnGearPolicy 로 판정해 "Real"/"Temporary" 를 싣는다.
        // **손으로 고쳤는지만 보면 틀린다**(2026-11-03 인게임): 손대지 않았어도 입은 장비와 무기가 다 있으면
        // 실물로 나간다. 임시 장비는 입은 것이 없거나 필수 무기 슬롯이 비었을 때의 이야기다.
        // 타일은 좁다. "장비 구분:" 머리말은 프로필 팀원 타일에서 말하지 않아도 뜻이 통하고,
        // 그 자리를 비워야 자동 합류 단추가 들어간다(2026-11-03).
        internal const string EquipmentReal = "실물 장비로 출격";
        internal const string EquipmentTemporary = "레이드마다 임시 장비";
        internal const string EquipmentUndecided = "장비 구분 미정";

        // 자동 합류. 켜면 출격할 때 이 팀원이 따라붙는다.
        internal const string AutoJoinOn = "자동 합류 : 켬";
        internal const string AutoJoinOff = "자동 합류 : 끔";
        internal const string AutoJoinFailedToast = "자동 합류 설정을 바꾸지 못했습니다";
        internal const string GearKindReal = "Real";
        internal const string GearKindTemporary = "Temporary";
        internal const string LoadFailedTitle = "프로필 팀원을 읽지 못했습니다";
        internal const string LoadFailedLine = "서버 응답이 없거나 올바르지 않습니다";
        internal const string SessionUnverifiedTitle = "로그인 프로필 확인 불가";
        internal const string SessionUnverifiedLine = "지금 로그인한 프로필을 확인하지 못해 현재 프로필 걸러내기를 마치지 못했습니다";
        internal const string PickerMoreLineFormat = "외 {0}개는 표시하지 않았습니다. 위 프로필을 등록한 뒤 다시 여세요.";
        internal const string PickerTitle = "프로필 불러오기";
        internal const string PickerNoneLine = "불러올 수 있는 프로필이 없습니다.\n게임에 한 번 들어간 다른 프로필만 나옵니다.";
        internal const string PickerOwnUnknownLine = "지금 로그인한 프로필을 확인할 수 없어 목록을 열지 않습니다.";
        internal const string PickerFailedLine = "프로필 목록을 읽지 못했습니다.";
        internal const string AddNotAvailableToast = "이 프로필은 지금 팀원이 될 수 없습니다";
        internal const string AddFailedToast = "프로필을 불러오지 못했습니다";
        internal const string RemoveFailedToast = "프로필 팀원을 목록에서 빼지 못했습니다";

        // Asks the server for the joined roster. Never throws. Interpretation of the response
        // (envelope, rows, current-session filter) is in shared/StmRosterWire.cs.
        internal static StmRosterSnapshot Load()
        {
            try
            {
                string response = RequestHandler.GetJson(RosterRoute);
                return StmRosterWire.Interpret(response, TryGetOwnAccountId());
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogWarning($"[STM] Profile roster could not be loaded; the profile section is empty. {ex.Message}");
                return StmRosterWire.Failed(ex.Message);
            }
        }

        // Registers a profile. Returns null on success, otherwise the toast text. Never throws.
        internal static async Task<string> AddAsync(string profileId)
        {
            return await PostAsync(AddRoute, profileId, AddFailedToast, mapNotAvailable: true);
        }

        // 자동 합류를 켜거나 끈다. 성공이면 null, 아니면 토스트 문구. 던지지 않는다.
        // **PIT 의 라우트를 그대로 쓴다**(`/singleplayer/pitfireteam/teammate/autojoin`) - 서버는 이미
        // STM 을 먼저 묻고(FriendlyTeammateCallbacks.SetAutoJoin → StmAutoJoinService) 프로필 팀원을
        // 자기 등록 문서에 쓴다. 없던 것은 **누를 자리**뿐이었다.
        internal static async Task<string> SetAutoJoinAsync(string accountId, bool enabled)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                return AutoJoinFailedToast;
            }

            try
            {
                string body = JsonConvert.SerializeObject(new { aid = accountId, enabled });
                string json = await Task.Run(() => RequestHandler.PostJson(AutoJoinRoute, body));
                if (string.IsNullOrWhiteSpace(json))
                {
                    return AutoJoinFailedToast;
                }

                JToken root = JToken.Parse(json);
                int err = root["err"]?.Value<int>() ?? -1;
                if (err == 0)
                {
                    return null;
                }

                string errmsg = root["errmsg"]?.ToString();
                pitFireTeam.Log.LogWarning($"[STM] 자동 합류 변경이 거절됐다 (aid '{accountId}'): {errmsg}");
                return string.IsNullOrWhiteSpace(errmsg) ? AutoJoinFailedToast : errmsg;
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogWarning($"[STM] 자동 합류 변경에 실패했다 (aid '{accountId}'): {ex.Message}");
                return AutoJoinFailedToast;
            }
        }

        // Unregisters a profile. Returns null on success, otherwise the toast text. Never throws.
        internal static async Task<string> RemoveAsync(string profileId)
        {
            return await PostAsync(RemoveRoute, profileId, RemoveFailedToast, mapNotAvailable: false);
        }

        // Re-reads the native friend list so the game's party-invite window follows the change.
        internal static void RefreshNativeFriendList()
        {
            try
            {
                SocialNetworkClassPatch.RefreshFriendsList(true);
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogWarning($"[STM] Friend list refresh failed: {ex.Message}");
            }
        }

        // The account id (Aid, as a string) of the profile logged in right now, or null when it
        // cannot be read.
        internal static string TryGetOwnAccountId()
        {
            try
            {
                if (pitFireTeam.application == null)
                {
                    pitFireTeam.application = SPT.Reflection.Utils.ClientAppUtils.GetMainApp();
                }

                string accountId = pitFireTeam.application?.Session?.Profile?.AccountId;
                return string.IsNullOrWhiteSpace(accountId) ? null : accountId.Trim();
            }
            catch
            {
                return null;
            }
        }

        private static async Task<string> PostAsync(string route, string profileId, string failureToast, bool mapNotAvailable)
        {
            if (string.IsNullOrWhiteSpace(profileId))
            {
                return failureToast;
            }

            try
            {
                string body = JsonConvert.SerializeObject(new { profileId });
                string json = await Task.Run(() => RequestHandler.PostJson(route, body));
                if (string.IsNullOrWhiteSpace(json))
                {
                    return failureToast;
                }

                JToken root = JToken.Parse(json);
                int err = root["err"]?.Value<int>() ?? -1;
                if (err == 0)
                {
                    return null;
                }

                string errmsg = root["errmsg"]?.ToString();
                pitFireTeam.Log.LogWarning($"[STM] {route} rejected for '{profileId}': {errmsg}");
                if (mapNotAvailable && string.Equals(errmsg, "StmProfileNotAvailable", StringComparison.Ordinal))
                {
                    return AddNotAvailableToast;
                }

                return failureToast;
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogWarning($"[STM] {route} failed for '{profileId}': {ex.Message}");
                return failureToast;
            }
        }
    }
}
