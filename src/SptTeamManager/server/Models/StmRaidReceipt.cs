using System.Text.Json.Serialization;

namespace pitTeam.Server.Models;

// Proof that one teammate's result for one raid was already applied to their launcher profile.
// The idempotence key is RaidId + ProfileId and nothing else: two teammates in one raid are two
// receipts, and one teammate in two raids is two receipts. Action says what was done (for a human
// reading the stored document); it is never part of the key.
// AppliedAtUnixSeconds is UTC seconds (DateTimeOffset.UtcNow.ToUnixTimeSeconds()).
// Property names are pinned: this is persisted data, and renaming a C# property later must not
// orphan receipts already on disk - an orphaned receipt would let a raid be applied a second time.
// Like StmProfileMember this is a positional record, so reading it back depends on constructor-
// parameter binding to the pinned names below.
public record StmRaidReceipt(
    [property: JsonPropertyName("raidId")] string RaidId,
    [property: JsonPropertyName("profileId")] string ProfileId,
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("appliedAtUnixSeconds")] long AppliedAtUnixSeconds);

// Shape of the stm-raid-receipts.json document.
public record StmRaidReceiptsDocument
{
    [JsonPropertyName("receipts")]
    public List<StmRaidReceipt> Receipts { get; set; } = [];
}
