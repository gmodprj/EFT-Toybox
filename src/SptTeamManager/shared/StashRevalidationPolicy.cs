using System.Collections.Generic;

namespace pitTeam.Shared
{
    // One stash item after the client asked EFT whether it is acceptable. The question itself (EFT's item
    // filters) stays in the client, which is the only place that can ask the game; this record carries
    // the answer and nothing more.
    public sealed record StashCandidate(string? Id, bool Accepted, string? RejectionReason);

    // An item that was left out, and why. Reason is never blank: it is what the player finds in the log
    // when an item vanishes from a screen.
    public sealed record StashRejection(string? Id, string Reason);

    public sealed record StashRevalidationResult(List<string> KeptIds, List<StashRejection> Skipped);

    // Decides what to DO with per-item revalidation results. Born of the 2026-10-27 accident
    // (doc/fixes/2026-10-27-pit-stash-ammo.md): one 5.56 round inside a 9x19 magazine failed the whole
    // stash revalidation, which aborted teammate creation partway and left half-made teammates behind.
    // The repair is that one offending item is skipped BY ITSELF, loudly, instead of killing the batch.
    //
    // Pure and exception-free: it only reads the candidates it is handed. Fails closed on identity - an
    // item that cannot be named is never kept, because nothing downstream could refer to it.
    public static class StashRevalidationPolicy
    {
        // EFT's error text names the offending item as "(id: <24 hex>)", e.g.
        //   No filter for item Pro+ (id: 6adea532010015d86e5ffb7e) in StackSlot cartridges in item mag_...
        // (the real line from doc/fixes/2026-10-27-pit-stash-ammo.md). The closing paren is part of the
        // pattern on purpose: it is what rejects an id of the wrong length instead of truncating it.
        private static readonly System.Text.RegularExpressions.Regex ItemIdPattern =
            new System.Text.RegularExpressions.Regex(@"\(id:\s*([0-9a-fA-F]{24})\)");

        // Pulls the FIRST item id out of an EFT error line (the offending item; a parent is named by name
        // only). False when nothing trustworthy is there - the caller must then not guess which item to
        // skip. Lives here, not in the client, so the one assumption this repair rests on is test-pinned.
        public static bool TryExtractItemId(string? errorText, out string? itemId)
        {
            itemId = null;
            if (string.IsNullOrWhiteSpace(errorText)) return false;

            var match = ItemIdPattern.Match(errorText!);
            if (!match.Success) return false;

            itemId = match.Groups[1].Value;
            return true;
        }

        public static StashRevalidationResult Partition(IEnumerable<StashCandidate>? items)
        {
            var kept = new List<string>();
            var skipped = new List<StashRejection>();
            if (items == null)
            {
                return new StashRevalidationResult(kept, skipped);
            }

            foreach (var item in items)
            {
                // Reported, not ignored: a silent drop is how the original accident stayed misdiagnosed.
                if (item == null)
                {
                    skipped.Add(new StashRejection(null, "빈 항목이라 건너뛴다"));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.Id))
                {
                    skipped.Add(new StashRejection(item.Id, "id 가 없어 건너뛴다"));
                    continue;
                }

                if (!item.Accepted)
                {
                    var reason = string.IsNullOrWhiteSpace(item.RejectionReason)
                        ? "사유가 전해지지 않았다"
                        : item.RejectionReason!;
                    skipped.Add(new StashRejection(item.Id, reason));
                    continue;
                }

                kept.Add(item.Id!);
            }

            return new StashRevalidationResult(kept, skipped);
        }
    }
}
