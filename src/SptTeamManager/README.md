# SPT Team Manager (STM)

STM is a fork of **PIT Fireteam 1.0.0** by PitAlex. It adds a second kind of teammate (another
profile on the same SPT server) next to PIT's mercenaries. The original author and upstream
repository stay credited; this fork changes names and adds files. Upstream history is preserved via git
subtree and modifications to upstream files are kept minimal (identifiers such as the GUID, assembly names and display name were renamed inside upstream files).

## Provenance and licence

- Upstream repository: https://github.com/pitAlex/SPT-PitFireTeam
- Mod page: https://sp-mod.com/mod/2676/pit-fireteam
- Fork base: PIT Fireteam 1.0.0, commit `f32a41b` (full SHA `f32a41b2def356a25b3ed40d1115af9116c7e51b`),
  imported as a git subtree with history.
- Authorship: PitAlex. The `Author` and `Url` fields of the server mod still point at PitAlex and the
  upstream repository.

**Licence status is not settled, and this README does not claim a licence.** What is known:

- The mod page above has a labelled "License" field in its Details section whose value is "MIT License" (linking to https://choosealicense.com/licenses/mit/). The author therefore **declared** MIT on the distribution page (verified 2026-11-01). The page does not reproduce the licence text.
- Upstream's own server metadata also declares MIT: `server/PitFireTeamServerPlugin.cs:28` reads `License = "MIT"` (same record as `Author = "PitAlex"` and the upstream `Url`; identical at `f32a41b`). That is a declaration string, not licence text and not a copyright notice.
- There is **no LICENSE file** anywhere in the upstream source tree (`git ls-files src/SptTeamManager` finds none), so no licence text accompanies the source.
- **Open decision for the repository owner (not decided here):** STM ships that metadata record, so the installed fork mod currently asserts `License = "MIT"` while this README makes no licence claim of its own. Whether the fork keeps that field as-is or changes it is the owner's call. Whether and how this fork may be redistributed (including the missing licence file and the SAIN notice below) should also be settled, ideally by asking the author, before any public release.

**Unresolved attribution defect inherited from upstream (must be resolved before distributing).**
The optional SAIN addon in `addon/` contains code copied from SAIN, which needs its own upstream notice
file `addon/SAIN-LICENSE.txt`. That file is already deleted in the upstream working tree. It is also
absent from the live installation: a search of `E:/Spt` for `SAIN-LICENSE*` finds nothing, and neither installed
PIT directory (`BepInEx/plugins/pitFireTeam`, `SPT_Runtime/user/mods/pitFireTeam-ServerMod`) contains any
licence file (measured 2026-11-01). That makes upstream's claim in `addon/docs/Validation.md` that the notice "is
already installed and was retained" untrue at least for this installation. Upstream's own
documents record the problem: `addon/docs/References.md` (line 24) says the notice must be resolved
before packaging, and `addon/docs/Validation.md` (line 566) says the build output lacks it and the issue
"remains unresolved". This fork imported the addon as-is, so it has the same gap. Plan 1 neither builds
nor installs the addon, so nothing is being distributed now. This repository also lacks the notice: no SAIN-LICENSE file exists in the repo.

## What is in this fork today (only the pure policy code is covered by tests)

Plans 1 (foundation) and 2 (into the raid) are committed, installed, and **verified by playing the
game** on 2026-11-01. Unit tests: 141 passing; server builds with 0 warnings and 0 errors.

**What works**

- Register another launcher profile as a teammate; it appears in the roster and the friend list, and
  the profile currently logged in is never offered.
- Invite it: it joins the party. Up to four teammates - the client refuses the fifth with "the party is
  full", so there is no server-side seat gate and none is needed.
- Take it into a raid **wearing that profile's own gear**. A profile with no usable kit goes in with a
  **generated temporary loadout** instead, keeping its own level, health and skills - the two paths
  differ in the inventory and nothing else.
- If it went in with **real** gear and did not come back, the gear it wore is **deleted from the stored
  profile**. The stash and the secured container's contents are kept, as are the pockets container, the
  armband and the knife. A receipt makes this happen at most once per raid.
- Gear the player loots off a dead teammate's corpse and carries out **stays with the player** - the
  raid-entry protection on that teammate's items is released once they are gone from the profile.
- PIT's own mercenaries are untouched and can be mixed into the same squad.

**What it deliberately does not do**

- Nothing is ever *added* to a teammate's profile: no experience, no skills, no raid statistics, no
  loot. Plan 2 only subtracts, because adding is where duplication bugs live. That is Plan 4.
- Gear looted from a teammate who died in a **temporary** loadout disappears at raid end. Releasing it
  would turn an unequipped profile into an endless gear supply; a log line says so when it happens.

**SPT requirement of this fork: 4.1.6, not 4.1.x.** Upstream compiles against the oldest supported
patch (4.1.0) so one build runs across the whole 4.1.x range; those 4.1.0.0 client assemblies are not
obtainable on this machine, and a personal fork on a single 4.1.6 install gains nothing from the wider
range. So the client's reference check was moved to 4.1.6.0 - the check itself is still enforced. Note
that the server metadata still declares `SptVersion = "~4.1.0"`, which is therefore no longer true for
the client; that is recorded, deliberately unchanged, and must be settled before any distribution. See
`doc/WORKLOG.md`, "클라이언트 기준 변경과 첫 빌드".

Known limitation: the pre-raid matchmaker's auto-join path cannot rebuild a profile teammate, because
`/singleplayer/pitfireteam/teammate/profile` still resolves only PIT's own mercenary documents - the one
route of four that Plan 2 did not reach. The party screen itself draws them correctly and the spawn is
unaffected; what is unverified is whether "restore last squad" works. See `doc/WORKLOG.md`.

Known limitation: the native friend-list "delete" action does nothing for a profile member
(it only removes PIT mercenaries, and the entry returns on the next poll); remove a profile member from
STM's own roster screen instead.

Shared sources in `shared/` are linked by hand, one line per file and no wildcards, to keep the upstream
merge surface small. Forgetting a link fails silently in the project that lacks it, so check every
project when adding a shared file: `ProfileRosterPolicy.cs` is linked into the server, the client and
the tests; `FriendListMergePolicy.cs`, `SpawnGearPolicy.cs`, `RaidOutcomePolicy.cs`,
`RaidReceiptKeyPolicy.cs`, `WornGearLossPolicy.cs`, `RaidProtectionReleasePolicy.cs` and
`FollowerAiProfilePolicy.cs` into the server and
the tests only (server-side policy); `StmRosterWire.cs` into the client and the tests only, **not the
server**, because it depends on `Newtonsoft.Json.Linq`, which the server does not need.

`PartySlotPolicy.cs` was deleted, together with its tests. The in-game check measured the cap as the
player plus FOUR teammates - five party slots - and found the client enforcing it itself, so the policy
had no caller, no future caller, and a constant (three teammate seats) the measurement had falsified.
Tests pinning a number that has been measured false are worse than no policy at all.

Not done (later plans): editing a profile teammate's equipment and the teammate detail screen (Plan 3),
and applying raid results - experience, skills, stats, loot (Plan 4). See `doc/WORKLOG.md` (repo root)
for the in-game results, the three defects the game found that unit tests and review did not, and the
constraints for later plans.

Install with `tools/install-sptteammanager.ps1` from the repository root; it refuses if PIT Fireteam is
installed, because STM keeps PIT's route prefix and the two cannot coexist.

---

# Upstream README (PIT Fireteam 1.0.0, unchanged below this line)

# Pit Fire Team

Pit Fire Team is a Single Player Tushonka mod for managing and commanding AI teammates.

**SPT target: 4.1.x (minimum 4.1.0).**

## Download

Download the latest release from the official SPT Forge page:

https://sp-mod.com/mod/2676/pit-fireteam

The project is split into three parts:

- `client/` - the BepInEx game plugin, follower control logic, UI, patches, and runtime systems.
- `server/` - the SPT server mod for teammate storage, profile/social integration, and raid support routes.
- `addon/` - the optional addons for specific features.  

This repository is the active development home for the mod formerly developed under the friendlyPMC working name.

The mod is still under active development, so implementation details and packaging can change between builds.
## Documentation

- [Core documentation](docs/README.md)
- [SAIN addon documentation](addon/docs/README.md)
- [Engineering guidance](AGENTS.md)
