using pitTeam.Server.Models;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;

namespace pitTeam.Server.Services;

// Remembers which (raid, teammate profile) results were already applied, in PIT's encrypted
// per-session database, so the same raid is never processed twice. Applying a result can DELETE
// real equipment from a launcher profile the player actually plays, and that cannot be undone;
// this store is what prevents a second deletion.
// No caching: every call reads the document fresh. A stale cached "not applied" is exactly the
// dangerous direction, because it would permit a second deletion.
[Injectable(InjectionType.Singleton)]
public class StmRaidReceiptStore(FriendlyTeammateStorage storage, ISptLogger<StmRaidReceiptStore> logger)
{
    // Not a "{aid}.json" name, so TeammateDatabase.IsProfileDocument() never lists it as a teammate.
    private const string Document = "stm-raid-receipts.json";

    private readonly object sync = new();

    // True when this teammate's result for this raid was already applied - or when that cannot be
    // determined. FAIL CLOSED: every "cannot tell" answers true.
    public bool WasApplied(MongoId sessionId, string raidId, string profileId)
    {
        // Blank keys cannot identify a receipt, so they can never be proven "not applied".
        if (string.IsNullOrWhiteSpace(raidId) || string.IsNullOrWhiteSpace(profileId))
        {
            logger.Error("[STM] WasApplied called with a blank raidId or profileId; treated as already applied so nothing is deleted.");
            return true;
        }
        raidId = raidId.Trim();
        profileId = profileId.Trim();
        try
        {
            lock (sync) return Load(sessionId).Any(r => Matches(r, raidId, profileId));
        }
        catch (Exception ex)
        {
            // Answer "already applied" on purpose. Skipping a legitimate deletion costs the player one
            // teammate's gear surviving a raid it should not have; answering false here could delete
            // real equipment a second time, and there is no undo. Logged as an error so this is not
            // mistaken for a real receipt.
            logger.Error($"[STM] Raid receipts could not be read; treating raid '{raidId}' / profile '{profileId}' as ALREADY APPLIED (fail closed, nothing will be deleted): {ex}");
            return true;
        }
    }

    // Idempotent per key. If the existing document cannot be read this throws instead of writing:
    // overwriting an unreadable document would erase the receipts it holds and re-open those raids.
    public void Record(MongoId sessionId, StmRaidReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        if (string.IsNullOrWhiteSpace(receipt.RaidId))
            throw new InvalidOperationException("StmRaidIdMissing");
        if (string.IsNullOrWhiteSpace(receipt.ProfileId))
            throw new InvalidOperationException("StmProfileIdMissing");
        var raidId = receipt.RaidId.Trim();
        var profileId = receipt.ProfileId.Trim();

        lock (sync)
        {
            var receipts = Load(sessionId).ToList();
            if (receipts.Any(r => Matches(r, raidId, profileId))) return;
            receipts.Add(receipt with { RaidId = raidId, ProfileId = profileId });
            Save(sessionId, receipts);
        }
    }

    // The whole key, exact match on both halves: a receipt of another teammate or another raid never matches.
    private static bool Matches(StmRaidReceipt r, string raidId, string profileId) =>
        string.Equals(r.RaidId, raidId, StringComparison.Ordinal)
        && string.Equals(r.ProfileId, profileId, StringComparison.Ordinal);

    // Per logged-in profile (keyed by sessionId), independent of the Guns for Hire / Allegiance mode.
    private List<StmRaidReceipt> Load(MongoId sessionId) =>
        storage.ReadSharedDocument<StmRaidReceiptsDocument>(sessionId, Document)?.Receipts ?? [];

    private void Save(MongoId sessionId, List<StmRaidReceipt> receipts) =>
        storage.WriteSharedDocument(sessionId, Document, new StmRaidReceiptsDocument { Receipts = receipts });
}
