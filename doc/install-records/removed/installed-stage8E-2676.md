# PIT Fireteam (2676) — 제거됨 (2026-11-01)

8-E 에서 설치했다가 **SPT Team Manager 를 넣을 때 빼냈다.** 아래 목록은 설치 당시의 기록이고,
제거할 때 지운 파일 목록이 그대로 이 목록이다.

- 백업: `backup/2026-11-01-before-stm/`
- 왜 빼냈나: STM 은 PIT 의 포크이고 **같은 서버 라우트 `/singleplayer/pitfireteam/*` 를 쓴다.**
  같이 두면 한쪽 요청을 다른 쪽이 받는다. `tools/install-sptteammanager.ps1` 이 이를 강제한다
  (PIT 폴더가 있으면 아무것도 복사하지 않고 중단) — 자세한 것은 `doc/PROCEDURE.md` §3.
- **다시 설치하지 않는다.** 후보 목록에 PIT 가 올라오면 이 문서를 근거로 뺀다.
- 2026-11-04 실측: `E:\Spt\BepInEx\plugins\pitFireTeam` 과
  `E:\Spt\SPT_Runtime\user\mods\pitFireTeam-ServerMod` 모두 **없다**(제거가 끝난 상태).

이 파일은 `installed-stage8E-2676.txt` 였다. `tools/gen-installed-mods.mjs` 가
`install-records/*.txt` 만 읽기 때문에, 확장자를 `.md` 로 바꿔 **자동 생성 목록에서 빠지게** 했다.
기록 자체는 아래에 그대로 남긴다.

## 설치 당시 파일 목록 (26줄, `모드폴더<TAB>상대경로`)

```
2676__pitFireTeam-1.0.0	BepInEx\plugins\pitFireTeam\allegiance-card.png
2676__pitFireTeam-1.0.0	BepInEx\plugins\pitFireTeam\auto-join.png
2676__pitFireTeam-1.0.0	BepInEx\plugins\pitFireTeam\brain.png
2676__pitFireTeam-1.0.0	BepInEx\plugins\pitFireTeam\diagonal_lines.png
2676__pitFireTeam-1.0.0	BepInEx\plugins\pitFireTeam\enemy-down.png
2676__pitFireTeam-1.0.0	BepInEx\plugins\pitFireTeam\enemy-seen.png
2676__pitFireTeam-1.0.0	BepInEx\plugins\pitFireTeam\enemy-visible.png
2676__pitFireTeam-1.0.0	BepInEx\plugins\pitFireTeam\gear.png
2676__pitFireTeam-1.0.0	BepInEx\plugins\pitFireTeam\gesture_over_there.png
2676__pitFireTeam-1.0.0	BepInEx\plugins\pitFireTeam\guns-for-hire-card.png
2676__pitFireTeam-1.0.0	BepInEx\plugins\pitFireTeam\icon_group.png
2676__pitFireTeam-1.0.0	BepInEx\plugins\pitFireTeam\icon_info_money_roubles_big.png
2676__pitFireTeam-1.0.0	BepInEx\plugins\pitFireTeam\locationping.ogg
2676__pitFireTeam-1.0.0	BepInEx\plugins\pitFireTeam\pitFireTeam.dll
2676__pitFireTeam-1.0.0	BepInEx\plugins\pitFireTeam\radiobeep.ogg
2676__pitFireTeam-1.0.0	BepInEx\plugins\pitFireTeam\radiochat.ogg
2676__pitFireTeam-1.0.0	BepInEx\plugins\pitFireTeam\squad-inverse.png
2676__pitFireTeam-1.0.0	BepInEx\plugins\pitFireTeam\squad.png
2676__pitFireTeam-1.0.0	SPT_Runtime\user\mods\pitFireTeam-ServerMod\LiteDB.dll
2676__pitFireTeam-1.0.0	SPT_Runtime\user\mods\pitFireTeam-ServerMod\pitFireTeam.Server.deps.json
2676__pitFireTeam-1.0.0	SPT_Runtime\user\mods\pitFireTeam-ServerMod\pitFireTeam.Server.dll
2676__pitFireTeam-1.0.0	SPT_Runtime\user\mods\pitFireTeam-ServerMod\Resources\avatars\courier.png
2676__pitFireTeam-1.0.0	SPT_Runtime\user\mods\pitFireTeam-ServerMod\Resources\lang\chs.json
2676__pitFireTeam-1.0.0	SPT_Runtime\user\mods\pitFireTeam-ServerMod\Resources\lang\en.json
2676__pitFireTeam-1.0.0	SPT_Runtime\user\mods\pitFireTeam-ServerMod\Resources\lang\ru.json
2676__pitFireTeam-1.0.0	SPT_Runtime\user\mods\pitFireTeam-ServerMod\Resources\teammates\6adc26693f5b5290609ea366.db
```
