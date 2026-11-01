# SPT Team Manager (STM)

STM is a fork of **PIT Fireteam 1.0.0** by PitAlex. It adds a second kind of teammate (another
profile on the same SPT server) next to PIT's mercenaries. The original author and upstream
repository stay credited; this fork changes names and adds files, and it keeps the upstream tree
intact so upstream changes can still be merged.

## Provenance and licence

- Upstream repository: https://github.com/pitAlex/SPT-PitFireTeam
- Mod page: https://sp-mod.com/mod/2676/pit-fireteam
- Fork base: PIT Fireteam 1.0.0, commit `f32a41b` (full SHA `f32a41b2def356a25b3ed40d1115af9116c7e51b`),
  imported as a git subtree with history.
- Authorship: PitAlex. The `Author` and `Url` fields of the server mod still point at PitAlex and the
  upstream repository.

**Licence status is not settled, and this README does not claim a licence.** What is known:

- The mod page above has a labelled "License" field in its Details section whose value is "MIT License" (linking to https://choosealicense.com/licenses/mit/). The author therefore **declared** MIT on the distribution page (verified 2026-11-01). The page does not reproduce the licence text.
- The upstream repository has **no LICENSE file** (`git ls-files src/SptTeamManager` finds none), so the
  source tree itself carries no licence notice.
- The declaration therefore exists only on the mod page; no licence text accompanies the source. Whether and how this fork may be redistributed (including the missing licence file and the SAIN notice below) is for the repository owner to settle, ideally by asking the author, before any public release.

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
nor installs the addon, so nothing is being distributed now.

## What works in this fork today

Plan 1 (foundation) is committed; **nothing has been installed or run in the game yet.**

- Server: lists launcher profiles and judges which can be teammates, stores the registration of a
  profile as a teammate, merges registered profiles into the friend list (native entries and
  mercenaries are kept), and has a pure party-seat policy (3 AI teammate seats) that is deliberately
  not wired into the invite path yet. Unit tests: 51 passing; server builds with 0 warnings and 0 errors.
- Client: roster screen code for profile teammates. **The client has never been compiled** - the
  4.1.0.0 client references do not exist on this machine.

Not done (later plans): editing a profile teammate's equipment, stash transfer, and applying raid
results (experience, skills, stats, loot, losses). See `doc/WORKLOG.md` (repo root) for the full list,
the human verification checklist, and the constraints for later plans.

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
