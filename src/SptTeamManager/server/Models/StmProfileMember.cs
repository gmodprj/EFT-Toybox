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
