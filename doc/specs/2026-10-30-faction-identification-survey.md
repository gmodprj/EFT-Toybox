# 세력 구분 실측 — 모드로 늘어난 세력과 표시 모드의 판정 기준

- 날짜: 2026-10-30
- 대상: `E:\Spt` (SPT 4.1.6, EFT 0.16.9.5.40743)
- 방법: 켜져 있는 서버(`SPT.Server.exe`)에 직접 질의 + `BepInEx\LogOutput.log` + DLL 역컴파일(ilspycmd 11.2)
- 요약: **세력을 새로 더하는 모드는 ISB Special Forces 하나**다(틀은 MoreBotsAPI). 구분은 `EPlayerSide`·`WildSpawnType`·MoreBotsAPI 세력표 세 축으로 갈리고, **표시 모드마다 쓰는 축이 달라 같은 봇이 지도에선 스캐브, 킬 로그에선 보스로 나온다**.

## 1. 지금 깔린 것

| 구분 | 수 | 근거 |
| --- | --- | --- |
| 설치 모드 | 186 | `doc/INSTALLED-MODS.md` |
| 게임 플러그인 폴더·DLL | 133 | `BepInEx\plugins` 목록 |
| 서버 모드 | 116 | `SPT_Runtime\user\mods` 목록 |
| 커스텀 `WildSpawnType` 을 등록하는 모드 | 2 (ISB + 틀인 MoreBotsAPI) | `BepInEx` 전체에서 `CustomWildSpawnType` 문자열을 가진 DLL = `ISB-SpecialForces.dll`, `MoreBotsPlugin.dll`, 그 프리패처 2개 |

AI 를 건드리지만 **세력을 더하지는 않는** 것들: SAIN(행동만), Waypoints(내비메시), BigBrain(레이어 틀), MoreBotsAPI(틀·봇 상한 +5), PIT Fireteam(아군 동료 — 바닐라 role 재사용), Mokou·Olympus·Aishi(상인·아이템).

## 2. 구분의 세 축

1. **`Profile.Info.Side` (`EPlayerSide`)** — 실측 값: `1 = Usec`, `2 = Bear`, `4 = Savage`(`DynamicMaps.Utils.GameUtils` 역컴파일에서 숫자 비교로 확인).
2. **`Profile.Info.Settings.Role` (`WildSpawnType`)** — 바닐라는 `0~67`. 프리패처가 enum 에 값을 더해 넓힌다.
3. **MoreBotsAPI 세력표** — 서버가 들고 있고 클라이언트가 레이드 시작 때 받아 간다(`/morebotsapi/getfactions`). 적대·복수 판정용.

### 바닐라 보조 판정(BSG 코드)

`WildSpawnTypeExtension._spawnTypeSettings` 사전 하나로 `IsBoss`·`IsFollower`·`IsHostileToEverybody`·`ScavRoleKey`·`PhraseTag` 가 나온다. `IsBoss` 는 키가 없으면 false 로 빠지지만 **`IsFollower`·`GetScavRoleKey` 는 인덱서라 키가 없으면 예외**다 — 커스텀 role 을 이 사전에 넣지 않으면 HUD 쪽이 터진다.

## 3. 실측: 서버가 들고 있는 세력 22개

`/morebotsapi/getfactions` 응답(zlib, 7970바이트) — 22개 중 21개는 바닐라 묶음이고 마지막 하나가 모드 세력이다.

- 바닐라: `raiders`, `rogues`, `smugglers`, `bloodhounds`, `scavs`, `cultists`, `infected`, `usec`, `bear`, `killaTagilla`, `kabanKolontay`, `reshala`, `shturman`, `gluhar`, `sanitar`, `partisan`, `misc`
- 묶음 세력(하위 세력만 가짐): `pmcs`(usec+bear), `scavbosses`, `criminals`, `savage`
- **모드 세력: `ISB` — role 16개**

### ISB 16 role (게임 로그 `MoreBotsAPI` 등록 줄 + 프리패처 역컴파일)

| enum | role | `ScavRole` 키 | IsBoss | IsFollower |
| --- | --- | --- | --- | --- |
| 13701 | ISBSpecialForces | ISB | - | ● |
| 13702 | ISBBossFirefly | ISBWhiteTusk | ● | - |
| 13703 | ISBFollowerVipper | ISBWhiteTusk | ● | - |
| 13704 | ISBFollowerLoghan | ISBWhiteTusk | ● | - |
| 13705 | ISBFollowerSpearthrower | ISBWhiteTusk | ● | - |
| 13706 | ISBFollowerZofia | ISBWhiteTusk | ● | - |
| 13707 | ISBBossCommander | ISBCommander | ● | - |
| 13708 | ISBFollowerCommander | ISBBossFollower | - | ● |
| 13709 | ISBTeamLeader | ISBBoss | ● | - |
| 13710 | ISBSecondLeader | ISBGuard | - | ● |
| 13711 | ISBScout | ISBScout | ● | - |
| 13712 | ISBHeavy | ISBHeavy | ● | - |
| 13713 | ISBAssassin | ISBAssassin | ● | - |
| 13714 | ISBMarksman | ISBSniper | ● | - |
| 13715 | ISBFollowerEvernight | ISBWhiteTusk | ● | - |
| 13716 | ISBFollowerNemesis | ISBWhiteTusk | ● | - |

`IsHostileToEverybody` 는 16개 모두 false. 등록 경로는 `MoreBotsAPI.Plugin.Awake` 가 `Traverse` 로 `_spawnTypeSettings` 에 `new WildSpawnTypeSettings(IsBoss, IsFollower, IsHostileToEverybody, "ScavRole/" + ScavRole, PhraseTag 0)` 을 더하는 것 — 그래서 바닐라 판정 함수들이 ISB 에도 답을 준다.

### 봇 하나를 실제로 뽑아 본 값

`POST /client/game/bot/generate`(body 는 zlib 압축, 응답도 zlib — 안 하면 서버가 `unsupported compression method` 로 끊는다. HTTP 가 아니라 **HTTPS** 여야 한다)로 16 role 전부 생성:

- 16개 모두 `Info.Side = Savage`, `Info.MemberCategory = 0`, `Settings.Role` 은 자기 이름 그대로.
- 비교: `pmcUSEC`·`pmcBEAR` 는 이 라우트에서도 `MemberCategory = 2`(나머지는 0) — **PMC 를 가려내는 또 하나의 축**.
- 닉네임이 영문 고유명으로 박혀 있다(Firefly, Vipper, Loghan, Zofia, Evernight, Nemesis, "The Chosen One", "The Dark Lord", "The Jester", "Carmilla" …). 바닐라 스캐브는 러시아어 이름.

주의: 이 라우트의 `Side` 는 레이드 배치 전 값이라 PMC 도 `Savage` 로 나온다. **레이드 실측**은 프로필 `Victims` 기록이고, 거기선 `pmcBEAR→Bear`, `pmcUSEC→Usec`, 나머지(`assault`·`bossTagilla`·`sectactPriestEvent`·`ravangeZryachiyEvent`)는 전부 `Savage` 였다. ISB 킬 기록은 아직 없어 ISB 의 레이드 중 Side 는 미확인 — 다만 ISB 는 PMC role 이 아니고 ScavRole 키를 받으므로 `Savage` 로 남을 선이다.

### 표시 이름(locale)

서버 `/client/locale/{en,ko}` 의 `ScavRole/*` 는 29개. 그중 **ISB 가 10개**(`ScavRole/ISB` = "ISB", `ISBBoss` = "ISB BOSS", `ISBGuard` = "ISB GUARD", `ISBScout`/`ISBSniper`/`ISBHeavy`/`ISBAssassin`, `ISBWhiteTusk`/`ISBCommander`/`ISBBossFollower` = "WHITE TUSK"). **ko 와 en 이 글자까지 같다 — 한국어 번역이 없다.**

## 4. 표시 모드별 판정 기준 (역컴파일 실측)

### Dynamic Maps 1.2.1 — 지도·미니맵

`DynamicMaps.DynamicMarkers.OtherPlayersMarkerProvider` 가 봇마다 위에서 아래로 한 번만 분류한다:

1. `IsGroupedWithMainPlayer()` → `Friendly Player` (`arrow.png`, 파랑-흰 중간)
2. `IsTrackedBoss()` → `Boss` (`star.png`, `Boss marker color` FF7302)
3. `IsPMC()`(Side 1·2) → `Enemy Player` (Bear FF0000 / Usec FFFF00)
4. `IsScav()`(Side 4) → `Scav` (FF7302)

`IsTrackedBoss` 는 `Side == Savage` **그리고** `_trackedBosses` 에 role 이 있을 때만 true 다. 그런데 `_trackedBosses` 는 **enum 숫자 21개가 소스에 박힌 HashSet**(3,6,7,11,17,21,22,25,26,27,28,29,32,35,43,47,65,66,67,199,801)이다. **13701~13716 은 없다.**

→ **ISB 는 BSG 테이블상 보스여도 지도에선 전부 일반 스캐브(주황 화살표)로 뜬다.** `Show Boss Markers` 를 꺼도 ISB 는 안 사라지고, `Show Scav Markers` 를 꺼야 사라진다. 지도에서 ISB 를 따로 보고 싶으면 그 HashSet 에 13701~13716 을 더하는 패치가 필요하다(모드가 설정으로 열어 둔 구멍은 없다).

### Game Panel HUD (4.1.6 포팅본) — 킬 패널·나침반

`GamePanelHUDKill.Views.KillHUDView` 는 BSG 함수를 그대로 쓴다.

- 라벨: `Side == Savage && IsBossOrFollower(role)` 이면 `GetScavRoleKey(role)`, 아니면 `Side.ToString()`.
- 색: `IsBoss(role)` 이면 `Kill Boss`(기본 빨강), 아니면 Usec/Bear/Scav 색.
- 스캐브 이름은 `Kill Scav Name To En` 이 켜져 있어 음역된다.

→ **ISB 는 킬 패널에서 "ISB BOSS"·"WHITE TUSK" 같은 제 이름과 보스색으로 뜬다.** 같은 봇을 두고 지도와 HUD 의 결론이 어긋난다(지도=스캐브, HUD=보스). 나침반 쪽(`CompassFireHUDView`)도 `IsBoss`/`IsFollower` 를 쓴다 — 즉 ISB 를 보스로 본다.

### Amands' Sense 3.1.0 — 시체 표시

`AmandsSenseDeadPlayer` 는 `Side` 로 갈라 `Usec → "USEC"`, `Bear → "BEAR"`, `Savage → GetScavRoleKey(role)` 현지화 + 닉네임 음역. ISB 시체는 "ISB BOSS"/"WHITE TUSK" 로 뜬다(HUD 와 같은 축).

### PIT Fireteam 1.0.0 — 아군 동료

- 동료는 커스텀 role 이 아니라 **바닐라 role 을 재사용**한다(`BossPlayers` 에서 `Settings.Role` 을 26 등으로 세팅). 플레이어 그룹에 들어가므로 지도에선 1번 규칙에 걸려 `Friendly Player` 로 뜬다.
- `PmcArmbands`(기본 켜짐): PMC 봇에 완장 — **BEAR 빨강, USEC 파랑**. 눈으로 가리는 유일한 축.
- 적대 쪽 옵션: `Bad Guy`(세력 무시하고 모든 PMC 와 적대), `Faction Hostilities`(세력 간 적대가 가끔 안 먹는 것을 고치는 패치), `Friendly Scav encounters`.

### 적대 판정(그림 아닌 쪽)

- MoreBotsAPI `FactionManager` 가 `WildSpawnTypeFactionDict`(role → 세력 이름들)를 만들고, `BotsGroupIsPlayerEnemyPatch`·`ShallBossAttackPatch` 가 그걸로 묶음 단위 적대를 정한다. 서버 쪽은 `AddEnemyByFaction`·`AddFriendlyByFaction`·`AddWarnByFaction`·`AddRevengeByFaction`.
- ISB `config.jsonc` → `forcedHostility.enabled = true`, `friendlyBotTypes` 27개(`exUsec`, `gifter`, `bossKnight`, `followerBigPipe`, `followerBirdEye`, `848400~848406`, `13701~13716`). `revenge.enabled = true`, `raidAmount = 6`(ISB 를 죽이면 6레이드 동안 보복).
- 도그태그: `displaySide = "ISB"` 인데 `internalSide = "Usec"` — 보이는 세력과 아이템 내부 세력이 다르다.

## 5. 덧붙여 확인한 것

- **Black Division(`848400~848406`, `blackDiv*`·`GhostBuster` role)**: locale·적대 목록·퀘스트 정의에는 있지만 `db/bots/sharedTypes` 에 봇 타입이 없고, `bot/generate` 로 `blackDivAssault`·`blackDivLead`·`blackDivSupport`·`blackDivBreacher`·`blackDivIb`·`blackDivGhostBusterLead`·`bossWedgeGhostBuster`·`848400` 전부 **0건**이 돌아왔다. 이 설치에서는 뜨지 않는다(Containment Breach 이벤트용으로 보인다). 등록 경로도 MoreBotsAPI 가 아니라 SPT 자체의 커스텀 enum(`SptPrePatcher.GetCustomEnumEntries`)이다.
- `sain_defaults`·`sharedConfig` 에만 있는 `ISBFirefly`·`ISBFollowerShielder` 도 생성 0건 — 지금 살아 있는 ISB role 은 16개가 전부다.
- SAIN 은 MoreBotsAPI `SAINInterop` 이 커스텀 role 마다 `BotType` 과 브레인 레이어를 만들어 주어 role 단위로 구분한다(ISB 가 `sain_defaults/ISB*.jsonc` 18개를 들고 온다).

## 6. 측정 방법 메모 (재현용)

- 서버는 **HTTPS**, 포트 6969, `Cookie: PHPSESSID=<프로필 id>`. 요청 본문과 응답 모두 zlib. 평문 본문을 보내면 서버가 `unsupported compression method` 로 끊고 빈 응답을 준다.
- 쓴 라우트: `/morebotsapi/getfactions`, `/morebotsapi/getrevenges`, `/client/locale/ko|en`, `/client/game/bot/generate`.
- 역컴파일: `ilspycmd`(이미 `C:\Users\deepe\.dotnet\tools` 에 있음). `-t <타입>` 으로 `Assembly-CSharp.dll` 의 `WildSpawnTypeExtension`·`WildSpawnType` 만 떠내면 빠르다.
