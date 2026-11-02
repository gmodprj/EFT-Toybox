using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace pitTeam.Server.Models;

// The fact of registration only: which launcher profile the user put on the team, plus the two
// team-level choices made about it. No nickname, level, gear, stash or stats are stored here -
// the launcher profile is the single source of truth, and a copy would become "ghost assets".
// Tactic is persisted only and nothing interprets it yet. AutoJoin and HandEdited are changed after
// registration through StmProfileMemberStore.Update (ProfileMemberUpdatePolicy); whoever acts on them
// is a later task.
// Property names are pinned: this is persisted data, and renaming a property later orphans what
// the human gate writes through SPT's real serializer.
// NOTE: StmProfileMember is a positional record, so reading it back depends on constructor-parameter
// binding (parameters are matched to the pinned property names below), not on settable properties -
// unlike every other persisted model in this codebase.
public record StmProfileMember(
    [property: JsonPropertyName("profileId")] string ProfileId,
    [property: JsonPropertyName("tactic")] string Tactic,
    [property: JsonPropertyName("autoJoin")] bool AutoJoin,
    // Appended last, and documents written before it existed have no "handEdited": a constructor parameter
    // missing from the JSON binds to false. That is checked by a machine at every server start
    // (StmProfileMemberStore.VerifyOldDocumentBinding), not assumed. One-way: set once the teammate's gear
    // was hand-edited, never cleared by an update (ProfileMemberUpdatePolicy).
    [property: JsonPropertyName("handEdited")] bool HandEdited);

// Shape of the stm-profile-members.json document.
public record StmProfileMembersDocument
{
    [JsonPropertyName("members")]
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
    [JsonPropertyName("autoJoin")] public bool AutoJoin { get; set; }
    [JsonPropertyName("handEdited")] public bool HandEdited { get; set; }
}

public record StmRosterResponse
{
    // Registered profiles, joined against the catalog right now (any status).
    [JsonPropertyName("members")] public List<StmRosterRow> Members { get; set; } = [];
    // Ready profiles not registered yet: what the load picker may offer.
    [JsonPropertyName("candidates")] public List<StmRosterRow> Candidates { get; set; } = [];
}
