using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace pitTeam.Shared
{
    // What the squad screen reads out of /stm/members/roster. Moved verbatim out of the client's
    // StmProfileRosterClient.Load so it can be compiled and tested outside the game.
    public sealed class StmRosterSnapshot
    {
        public bool Ok;
        public string Error = string.Empty;
        // Registered profiles, already filtered of the current session's own profile.
        public List<RegisteredProfileRow> Members = new List<RegisteredProfileRow>();
        // Ready, unregistered, not the current session. Empty when Ok is false.
        public List<RegisteredProfileRow> Candidates = new List<RegisteredProfileRow>();
        // False when the client could not read which profile is logged in.
        public bool OwnAccountKnown;
    }

    public static class StmRosterWire
    {
        // Throws on malformed JSON (the caller's catch turns that into a failed snapshot).
        public static StmRosterSnapshot Interpret(string? response, string? ownAccountId)
        {
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

            StmRosterSnapshot snapshot = new StmRosterSnapshot { Ok = true };

            foreach (RegisteredProfileRow row in ParseRows(data["members"]))
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
                foreach (RegisteredProfileRow row in ParseRows(data["candidates"]))
                {
                    if (row.Status != RosterRowStatus.Ready || row.Aid == 0 || IsCurrentSession(row, ownAccountId))
                    {
                        continue;
                    }

                    snapshot.Candidates.Add(row);
                }
            }

            snapshot.OwnAccountKnown = ownAccountId != null;
            return snapshot;
        }

        public static StmRosterSnapshot Failed(string message)
        {
            return new StmRosterSnapshot { Ok = false, Error = message ?? string.Empty };
        }

        public static bool IsCurrentSession(RegisteredProfileRow row, string? ownAccountId)
        {
            if (row.Status == RosterRowStatus.CurrentSession)
            {
                return true;
            }

            return ownAccountId != null
                && row.Aid != 0
                && string.Equals(row.Aid.ToString(), ownAccountId, StringComparison.Ordinal);
        }

        public static IEnumerable<RegisteredProfileRow> ParseRows(JToken? token)
        {
            if (!(token is JArray array))
            {
                return Enumerable.Empty<RegisteredProfileRow>();
            }

            List<RegisteredProfileRow> rows = new List<RegisteredProfileRow>();
            foreach (JToken? item in array)
            {
                string? profileId = item?["profileId"]?.ToString();
                if (string.IsNullOrWhiteSpace(profileId))
                {
                    continue;
                }

                rows.Add(new RegisteredProfileRow(
                    profileId.Trim(),
                    item!["aid"]?.Value<int?>() ?? 0,
                    item["nickname"]?.ToString() ?? string.Empty,
                    item["level"]?.Value<int?>() ?? 0,
                    item["side"]?.ToString() ?? string.Empty,
                    ParseStatus(item["status"]?.ToString())));
            }

            return rows;
        }

        // Anything the client does not recognise is Unreadable: unavailable, never Ready.
        public static RosterRowStatus ParseStatus(string? value)
        {
            if (!string.IsNullOrWhiteSpace(value)
                && Enum.TryParse(value.Trim(), true, out RosterRowStatus parsed)
                && Enum.IsDefined(typeof(RosterRowStatus), parsed))
            {
                return parsed;
            }

            return RosterRowStatus.Unreadable;
        }
    }
}
