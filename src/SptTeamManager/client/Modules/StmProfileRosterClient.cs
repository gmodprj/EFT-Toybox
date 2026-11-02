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

        // ---- Display text, in one place --------------------------------------------------------
        internal const string TileCaption = "프로필 팀원";
        internal const string LoadButtonText = "프로필 불러오기";
        internal const string ReasonEmpty = "게임에 한 번 들어가야 합니다";
        internal const string ReasonUnreadable = "프로필을 읽을 수 없습니다";
        internal const string ReasonMissing = "프로필이 더 이상 없습니다";
        // 임시 장비를 받는가, 맞춰 둔 장비를 그대로 입는가. 계획 2·3 이 끝나 서버가 명단 응답에 handEdited 를
        // 싣는다(2026-11-03) - 그 전까지는 둘 중 어느 쪽도 주장하지 않으려고 "아직 판정하지 않음" 만 보였다.
        // 한 방향이다: 한 번 손으로 맞추면(장비 편집·꾸러미 구매) 그 팀원은 임시 장비 대상에서 빠진다.
        internal const string EquipmentHandEdited = "장비 구분: 맞춰 둔 장비";
        internal const string EquipmentTemporary = "장비 구분: 레이드마다 임시 장비";
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
