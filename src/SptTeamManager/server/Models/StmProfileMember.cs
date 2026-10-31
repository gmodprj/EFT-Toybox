using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace pitTeam.Server.Models;

// The fact of registration only: which launcher profile the user put on the team, plus the two
// team-level choices made about it. No nickname, level, gear, stash or stats are stored here -
// the launcher profile is the single source of truth, and a copy would become "ghost assets".
// Tactic and AutoJoin are persisted only; nothing in this task interprets them.
public record StmProfileMember(string ProfileId, string Tactic, bool AutoJoin);

// Shape of the stm-profile-members.json document.
public record StmProfileMembersDocument
{
    public List<StmProfileMember> Members { get; set; } = [];
}

public record StmProfileMemberRequest : IRequestData
{
    [JsonPropertyName("profileId")]
    public string? ProfileId { get; set; }
}

// Wire shape of /stm/members/roster. Property names are pinned (not left to the serializer's
// casing policy) and the status is a string, so the client reads it without guessing. Built fresh
// on every request from the store and the catalog; never persisted.
public record StmRosterRow
{
    [JsonPropertyName("profileId")] public string ProfileId { get; set; } = string.Empty;
    [JsonPropertyName("aid")] public int Aid { get; set; }
    [JsonPropertyName("nickname")] public string Nickname { get; set; } = string.Empty;
    [JsonPropertyName("level")] public int Level { get; set; }
    [JsonPropertyName("side")] public string Side { get; set; } = string.Empty;
    // Ready | Empty | Unreadable | CurrentSession | Missing
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
}

public record StmRosterResponse
{
    // Registered profiles, joined against the catalog right now (any status).
    [JsonPropertyName("members")] public List<StmRosterRow> Members { get; set; } = [];
    // Ready profiles not registered yet: what the load picker may offer.
    [JsonPropertyName("candidates")] public List<StmRosterRow> Candidates { get; set; } = [];
}
