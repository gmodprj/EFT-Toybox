using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pitTeam.Patches;
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
        internal const string SectionTitle = "프로필 팀원";
        internal const string LoadButtonText = "프로필 불러오기";
        internal const string ReasonEmpty = "게임에 한 번 들어가야 합니다";
        internal const string ReasonUnreadable = "프로필을 읽을 수 없습니다";
        internal const string ReasonMissing = "프로필이 더 이상 없습니다";
        // Whether a member wears real gear or gets temporary gear is decided from the profile's worn
        // equipment, which is plan 2's branch (design 6-4). Plan 1 does not read it, so the screen
        // must not claim either. Swap this line when plan 2 supplies the answer.
        internal const string EquipmentUndecided = "장비 구분: 아직 판정하지 않음";
        internal const string LoadFailedTitle = "프로필 팀원을 읽지 못했습니다";
        internal const string LoadFailedLine = "서버 응답이 없거나 올바르지 않습니다";
        internal const string PickerTitle = "프로필 불러오기";
        internal const string PickerNoneLine = "불러올 수 있는 프로필이 없습니다.\n게임에 한 번 들어간 다른 프로필만 나옵니다.";
        internal const string PickerOwnUnknownLine = "지금 로그인한 프로필을 확인할 수 없어 목록을 열지 않습니다.";
        internal const string PickerFailedLine = "프로필 목록을 읽지 못했습니다.";
        internal const string AddNotAvailableToast = "이 프로필은 지금 팀원이 될 수 없습니다";
        internal const string AddFailedToast = "프로필을 불러오지 못했습니다";
        internal const string RemoveFailedToast = "프로필 팀원을 목록에서 빼지 못했습니다";

        internal enum RowStatus { Ready, Empty, Unreadable, CurrentSession, Missing }

        internal sealed class Row
        {
            public string ProfileId = string.Empty;
            public int Aid;
            public string Nickname = string.Empty;
            public int Level;
            public string Side = string.Empty;
            public RowStatus Status = RowStatus.Unreadable;
        }

        internal sealed class Snapshot
        {
            public bool Ok;
            public string Error = string.Empty;
            // Registered profiles, already filtered of the current session's own profile.
            public List<Row> Members = new List<Row>();
            // Ready, unregistered, not the current session. Empty when Ok is false.
            public List<Row> Candidates = new List<Row>();
            // False when the client could not read which profile is logged in.
            public bool OwnAccountKnown;
        }

        // Asks the server for the joined roster. Never throws.
        internal static Snapshot Load()
        {
            try
            {
                string response = RequestHandler.GetJson(RosterRoute);
                if (string.IsNullOrWhiteSpace(response))
                {
                    return Failed("empty response");
                }

                JToken root = JToken.Parse(response);
                if (root.Type != JTokenType.Object)
                {
                    return Failed("response is not an object");
                }

                int err = root["err"]?.Value<int>() ?? -1;
                if (err != 0)
                {
                    return Failed(root["errmsg"]?.ToString() ?? $"err={err}");
                }

                if (!(root["data"] is JObject data))
                {
                    return Failed("response has no data");
                }

                string ownAccountId = TryGetOwnAccountId();
                Snapshot snapshot = new Snapshot { Ok = true };

                foreach (Row row in ParseRows(data["members"]))
                {
                    // The server already marks the logged-in profile CurrentSession; the client does
                    // not rely on it. Such a profile is neither drawn nor offered.
                    if (IsCurrentSession(row, ownAccountId))
                    {
                        continue;
                    }

                    snapshot.Members.Add(row);
                }

                // Candidates are offered only when the client can itself tell who is logged in.
                // If it cannot, nothing is offered (unreadable means unavailable, never a guess).
                if (ownAccountId != null)
                {
                    foreach (Row row in ParseRows(data["candidates"]))
                    {
                        if (row.Status != RowStatus.Ready || row.Aid == 0 || IsCurrentSession(row, ownAccountId))
                        {
                            continue;
                        }

                        snapshot.Candidates.Add(row);
                    }
                }

                snapshot.OwnAccountKnown = ownAccountId != null;
                return snapshot;
            }
            catch (Exception ex)
            {
                pitFireTeam.Log.LogWarning($"[STM] Profile roster could not be loaded; the profile section is empty. {ex.Message}");
                return Failed(ex.Message);
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

        private static bool IsCurrentSession(Row row, string ownAccountId)
        {
            if (row.Status == RowStatus.CurrentSession)
            {
                return true;
            }

            return ownAccountId != null
                && row.Aid != 0
                && string.Equals(row.Aid.ToString(), ownAccountId, StringComparison.Ordinal);
        }

        private static IEnumerable<Row> ParseRows(JToken token)
        {
            if (!(token is JArray array))
            {
                return Enumerable.Empty<Row>();
            }

            List<Row> rows = new List<Row>();
            foreach (JToken item in array)
            {
                string profileId = item?["profileId"]?.ToString();
                if (string.IsNullOrWhiteSpace(profileId))
                {
                    continue;
                }

                rows.Add(new Row
                {
                    ProfileId = profileId.Trim(),
                    Aid = item["aid"]?.Value<int?>() ?? 0,
                    Nickname = item["nickname"]?.ToString() ?? string.Empty,
                    Level = item["level"]?.Value<int?>() ?? 0,
                    Side = item["side"]?.ToString() ?? string.Empty,
                    Status = ParseStatus(item["status"]?.ToString())
                });
            }

            return rows;
        }

        // Anything the client does not recognise is Unreadable: unavailable, never Ready.
        private static RowStatus ParseStatus(string value)
        {
            if (!string.IsNullOrWhiteSpace(value)
                && Enum.TryParse(value.Trim(), true, out RowStatus parsed)
                && Enum.IsDefined(typeof(RowStatus), parsed))
            {
                return parsed;
            }

            return RowStatus.Unreadable;
        }

        private static Snapshot Failed(string message)
        {
            return new Snapshot { Ok = false, Error = message ?? string.Empty };
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
