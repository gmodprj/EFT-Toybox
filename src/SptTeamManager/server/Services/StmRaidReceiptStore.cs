using pitTeam.Server.Models;
using pitTeam.Shared;
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
// TryReserve is the ONLY way in, deliberately: reserve first, then delete. The separate
// WasApplied/Record pair this class used to offer was deleted rather than left unused - checking under
// one lock and recording under another is the race TryReserve exists to close, and a method that still
// exists is a method someone reaches for.
// Known quiet way this guard disappears: TeammateDatabase.Wipe() deletes every document except the import
// marker, including stm-raid-receipts.json, so a profile wipe re-opens every past raid. Harmless in practice,
// because the same wipe removes the teammates whose gear a replay would delete - but nothing in Wipe says so.
[Injectable(InjectionType.Singleton)]
public class StmRaidReceiptStore(FriendlyTeammateStorage storage, ISptLogger<StmRaidReceiptStore> logger)
{
    // Not a "{aid}.json" name, so TeammateDatabase.IsProfileDocument() never lists it as a teammate.
    private const string Document = "stm-raid-receipts.json";

    private readonly object sync = new();

    // Reserve-before-acting: the caller reserves, and only when this returns true does it delete.
    // Under ONE lock: if a receipt for the pair exists, nothing is written and the answer is false;
    // otherwise the receipt is written and the answer is true. True means "I wrote a new receipt" and
    // nothing else. Every other outcome - already present, blank key, unreadable document, write
    // failure - is false, so the caller does nothing. Uniformly fail closed.
    // If the deletion then fails, the receipt still says "applied" and a retry skips it: one
    // teammate's gear surviving is the price; deleting a second set has no undo.
    public bool TryReserve(MongoId sessionId, string? raidId, string? profileId, string? action)
    {
        if (!RaidReceiptKeyPolicy.IsUsableKey(raidId, profileId))
        {
            logger.Error("[STM] TryReserve called with a blank raidId or profileId; refused so nothing is deleted.");
            return false;
        }
        raidId = raidId!.Trim();
        profileId = profileId!.Trim();
        try
        {
            lock (sync)
            {
                var receipts = Load(sessionId).ToList();
                if (receipts.Any(r => Matches(r, raidId, profileId))) return false;
                receipts.Add(new StmRaidReceipt(raidId, profileId, action ?? string.Empty, DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                Save(sessionId, receipts);
                return true;
            }
        }
        catch (Exception ex)
        {
            // False on purpose: unreadable document or failed write means we cannot prove this raid is
            // unprocessed, and a mistaken "go ahead" deletes real equipment with no undo.
            logger.Error($"[STM] Raid receipt could not be reserved for raid '{raidId}' / profile '{profileId}'; refused (fail closed, nothing will be deleted): {ex}");
            return false;
        }
    }

    // The match judgment lives in RaidReceiptKeyPolicy (pure, tested): trim, ignore case, both halves.
    // Deliberately OrdinalIgnoreCase, unlike StmProfileMemberStore's Ordinal/==: there a near miss on a
    // profile id is harmless, here a near miss reads as "not applied" and permits a second deletion.
    // Do not "unify" the two stores.
    private static bool Matches(StmRaidReceipt r, string raidId, string profileId) =>
        RaidReceiptKeyPolicy.KeysMatch(r.RaidId, r.ProfileId, raidId, profileId);

    // Per logged-in profile (keyed by sessionId), independent of the Guns for Hire / Allegiance mode.
    // Two different states must not be confused: no document at all (nothing applied yet -> empty list)
    // and a document whose "receipts" is null or missing (key drift, hand edit, truncation). The second
    // is UNREADABLE, not empty: treating it as empty would permit a deletion and then overwrite the file,
    // discarding every receipt it held. It throws, which TryReserve's fail-closed catch turns into a
    // refusal (and logs at Error), so nothing is deleted and nothing is overwritten.
    // Chosen over [JsonRequired] because that depends on how SPT's serializer is configured; this check
    // does not. The property has no default value so a missing key also arrives here as null.
    private List<StmRaidReceipt> Load(MongoId sessionId)
    {
        var doc = storage.ReadSharedDocument<StmRaidReceiptsDocument>(sessionId, Document);
        if (doc is null) return [];
        return doc.Receipts ?? throw new InvalidDataException($"Raid receipts document has no readable 'receipts' list: {Document}");
    }

    private void Save(MongoId sessionId, List<StmRaidReceipt> receipts) =>
        storage.WriteSharedDocument(sessionId, Document, new StmRaidReceiptsDocument { Receipts = receipts });
}
