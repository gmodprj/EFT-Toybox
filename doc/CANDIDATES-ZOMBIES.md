# 대체 좀비 모드 후보 — 기존 맵에 나오는 것 (2026-11-19 조사)

- 조건(사용자): **기존 맵에 좀비가 나온다.** 14-E SPZombies 처럼 메인 메뉴의 별도 모드·전용 맵(Stronghold)으로 들어가는 방식은 뺀다.
- 앞의 실패: SPZombies 는 자체 메뉴·맵·라운드제였고, 선행 6개가 본 게임 은신처를 바꿨고, "보통 게임에서는 끄기" 가 첫 기동에 되돌아갔고, FogSix 가 좀비 맵 안개를 최대로 그렸다 — 경위는 [HANDOVER-2026-11-17-2.md](HANDOVER-2026-11-17-2.md) §3, [plans/2026-11-17-six-mods-install-plan.md](plans/2026-11-17-six-mods-install-plan.md) §14·§15.
- 조사: `tools/search-mods.mjs`(zombie · zombies · infected · undead · halloween · horde · infection · seasonal · event · crazy), `tools/mod-detail.mjs`, Forge 설명 전문, `tools/fetch-mod-comments.mjs --all`, 설치본 실측(SPT 설정·SVM·SPT 서버 코드 역컴파일). **아무것도 받거나 설치하지 않았다.**

## 1. 먼저 알게 된 것 — SPT 에 이미 들어 있다

설치본 `SPT_Runtime\SPT_Data\configs\seasonalevents.json` 에 **2024 할로윈 감염(좀비) 이벤트가 통째로 있다.**

- `eventBossSpawns.halloweenzombies` — 기존 맵 12곳(bigmap · factory4_day · factory4_night · interchange · laboratory · lighthouse · rezervbase · sandbox · sandbox_high · shoreline · tarkovstreets · woods)에 `infectedAssault` 등 좀비 스폰이 정의돼 있다.
- `events[halloween].settings.zombieSettings` — `enabled: false`, 맵별 감염률 `mapInfectionAmount`(Labs 100 · Factory 50 · Woods 60 · Customs 0 · Interchange 0 …).
- 할로윈 기간은 10/28 ~ 11/9 이고, **그 기간에도 `enabled: false` 라 좀비는 안 나온다.**

SPT 서버 코드(`SPTarkov.Server.Core.dll` 의 `SeasonalEventService`, 역컴파일)로 확인한 동작:

- 좀비는 `ZombieSettings.Enabled == true` 일 때만 `ConfigureZombies` 가 켠다 — 봇 테이블 `ActiveHalloweenZombiesEvent`, 전역 `InfectionHalloween`, 맵별 `InfectionPercentage`, `halloweenzombies` 보스를 감염률 0 초과 맵에 더한다.
- **SVM 의 `Halloween` 스위치로는 좀비가 안 나온다.** SVM 은 `ForceSeasonalEvent(HALLOWEEN)` 만 부르고(역컴파일 `SVM.cs`), 위 `Enabled` 를 건드리지 않는다. SVM 의 `DisableZombies` 는 설정 모델에만 있고 쓰는 코드가 없다. 지금 프리셋 `Noname` 은 `Halloween: false` · `Christmas: false` · `DisableEvents: false`.
- 할로윈 이벤트를 강제로 켜면 좀비 외의 것도 함께 켜진다(소환 의식, 봇 적대 관계 교체, Labs 입장 조건 제거). **좀비만 원하면 이 묶음째 켜는 방식은 맞지 않다.**

## 2. 후보

| 순위 | 모드 | 4.1.6 | 방식 | 결속 | 판단 |
| --- | --- | --- | --- | --- | --- |
| **1** | [3232 zezinho's Past & Present Event Selector (PPES)](https://sp-mod.com/mod/3232) 1.7.7 | **예**(`~4.1.6`, 2026-11-06, Fika 호환) | SPT 내장 이벤트를 **부분별로** 켜고 끈다. 할로윈을 "2024 좀비·감염" 만 켜고 나머지(의식·컬트의 밤·2026 발병·호박 전리품)는 끌 수 있다 | **결속** | **추천.** 기존 맵 · 별도 모드 없음 · 서버 웹 패널(`/ppes`)에서 켜고 끔 · 게임 파일에 쓰지 않는다고 밝힘 |
| 2 | 직접 만들기 — 좀비만 켜는 작은 서버 모드 | — | 위 §1 의 `ZombieSettings` 를 켜고 할로윈의 다른 부분은 켜지 않는다 | 없음 | 받을 것 없음. 다만 아래 SAIN 문제를 따로 풀어야 한다 |
| 3 | [2644 Roaming Zombies](https://sp-mod.com/mod/2644) 1.2.0 | **아니오**(`~4.0.13` 뿐) | 레이드마다 무리 하나(근접 8~16 + 권총 4~8)를 맵에 넣는다. 맵별 확률·크기·지연 설정 | 없음 | **지금은 보류.** 공식 4.1.6 판이 없고, GitHub 의 비공식 4.1.6 PR 은 "ORBIT 과 충돌" 댓글이 있다(ORBIT 3.1.1 설치돼 있음) |
| 제외 | [3369 Manimal's Halloween 2026 Backport](https://sp-mod.com/mod/3369) 1.0.0 | 예 | 2026 할로윈 퀘스트·업적·좀비 변종·**Infected Labs 맵 변형**(Map Variants 로 고른다) | **결속** | 517MB. 선행 중 **3041 Factory Classic 이 없다**(2512·2310·3040·2864 는 있다). 좀비가 기존 맵 전체가 아니라 새 콘텐츠 중심이고 PPES 와 겹친다 |
| 제외 | [3352 Dr K's SPZombies](https://sp-mod.com/mod/3352) | 예 | 별도 메뉴·전용 맵·라운드제 | - | 이미 넣었다 뺐다 — 조건과 반대 |
| 제외 | [2842 Dynamic Raid Events](https://sp-mod.com/mod/2842) | 아니오(`~4.0.13`) | 레이드마다 무작위 이벤트 | - | 이벤트 목록에 좀비가 없다(보스·레이더·컬트·전리품) |
| 제외 | [3290 Raid Roulette \[Beta\]](https://sp-mod.com/mod/3290) | 예 | F1 자체 창에서 "특이한 적·날씨·전리품" 레이드를 고른다 | - | 116MB 베타·자체 UI. 좀비가 들어 있는지는 **설명으로 확인되지 않았다** |

## 3. 1순위 PPES — 넣는다면 확인할 것

설명 전문과 댓글 34건(14건 본문) 기준.

**맞는 점**

- 할로윈 부분 선택지에 **"zombies and infected (2024)"** 가 따로 있다 — 위 §1 의 내장 이벤트를 쓰므로 기존 맵 12곳에 나온다.
- 요구 모드 **MoreBotsAPI 2.1.1+** — 설치본은 **2.1.2**(9차). 나머지는 선택(Black Division · Blackout · SCOURGE · WTT-ContentBackport)이고 Blackout(10-I1)·WTT-ContentBackport 는 이미 있다.
- **SAIN**: 제작자 설명 — "SAIN 이 좀비가 때리지 못하게 막는다. PPES 의 클라이언트 플러그인이 다시 공격하게 한다". 설치본에 SAIN 4.5.1 이 있으므로 그 플러그인이 필요하다(PPES 가 함께 넣는다 — `BepInEx\plugins\PastPresentEventSelector`, 프리패처 `BepInEx\patchers\PastPresentEventSelector`).
- **ORBIT**: "충돌 없음. 고스트 모드가 먼 봇(이벤트 보스 포함)을 재운다".
- **SVM**: "SVM 이 할로윈·크리스마스를 강제하면 PPES 선택을 덮는다" — 우리 프리셋은 둘 다 `false` 라 덮지 않는다.
- 끄면 진행이 지워지지 않는다고 밝힌다. 보스·공수 이벤트는 다음 레이드부터, 계절 이벤트는 **서버 재시작** 후 적용.

**조심할 점**

- **결속이다** — 이벤트 퀘스트가 프로필에 들어온다. 절차대로 **프로필 백업 후 혼자** 넣는다.
- **기본값을 첫 기동 전에 확인해야 한다.** 설명에 "오늘의 이벤트가 자동으로 골라진다", 계절 이벤트 기본이 **Auto**(기간 중 켜짐), 지난 이벤트 44개·자동 순환·Fence 메시지가 있다. 받은 뒤 해제본의 설정 기본값을 읽어 **좀비 부분만 켜고 나머지는 Off** 로 맞춘다(SPZombies 때 "미리 넣은 `.cfg` 가 첫 기동에 되돌아갔다" — 이번에도 첫 기동 뒤 다시 본다).
- **2026 발병(모든 맵 감염 + 감염된 타길라) 부분은 켜지 않는다** — 1.7.7 댓글에 "2026 좀비 이벤트를 켜면 'civilian key' 를 못 찾아 맵 로딩이 터진다".
- 댓글: 2026 이벤트 좀비가 "아주 느리고 감염 타길라가 공격적이지 않다"(Roaming Zombies 댓글란) — 내장 좀비 그대로의 움직임이다.
- 댓글: GZ(Ground Zero)에 감염률 100 인데 안 나온다는 보고 1건. 내장 설정의 `sandbox` 기본값은 25 다.
- 맵 안개: 이번에는 기존 맵이라 SPZombies 전용 맵 같은 문제는 예상되지 않지만, 첫 레이드에서 FogSix·Battle Ambience 와 화면을 본다.
- 서버 웹 패널 페이지가 하나 는다(`/ppes`, 영어·포르투갈어) — 모드 카드·웹 화면 한글화(ModCardKorean) 대상이 된다.

## 4. 2순위 직접 만들기 — 갈 경우의 뼈대

- 서버 모드 하나: `seasonalevents.json` 의 할로윈 `zombieSettings` 를 `Enabled=true` 로 두고, 할로윈 이벤트를 강제하지 않은 채 `ConfigureZombies` 와 같은 일(봇 테이블·전역 감염·맵별 감염률·`halloweenzombies` 보스)만 한다. 맵별 감염률은 `config.jsonc` 로.
- 위험: **SAIN 이 좀비 공격을 막는 문제**(PPES 설명). 클라이언트 쪽 해결이 필요하면 범위가 커진다 — 먼저 서버 쪽만 만들어 레이드에서 좀비가 때리는지 본다.
- 이 저장소의 다른 서버 모드처럼 SPT DB 는 **변환기·PostLoad** 로 고친다(메모리 `spt-db-writes-need-transformers`, `db-changes-must-outrun-item-mods`).

## 5. 15-A PPES 설치 결과 (2026-11-19 14:16~14:40, 사용자 결정 "PPES로 설치")

- 받기: `manifest-stage15.json` → `downloads/stage15/3232__PastPresentEventSelector-1.7.7.zip` **380,614 B 일치**(GitHub 릴리스). 해제본 7파일, 설치본과 겹침 0.
- 해제본 역컴파일로 본 기본값: 계절 이벤트 전부 `auto`, 지난 이벤트 `[]`, 순환 `off`. 할로윈 부분은 **ritual · cultists · labInfectedOnly · freeLab · loot 가 기본 켜짐**, zombies · zombieWaves · outbreak2026 이 기본 꺼짐. `config.json` 은 **있으면 그대로 읽고 없을 때만 만든다**(`ConfigService.Load`).
- 그래서 **첫 기동 전에 `config.json` 을 넣었다**(해제본에 두고 함께 설치 — 기록에 남는다):
  - `seasonal.halloween = "on"`(오늘이 할로윈 기간 밖이라 `auto` 면 좀비가 안 켜진다), 크리스마스·새해·만우절은 `auto` 그대로.
  - 할로윈 부분: **zombies 만 true**. ritual · cultists · freeLab · loot · outbreak2026 · zombieWaves false. `labInfectedOnly` false — Labs 는 "감염자만" 이 아니라 **평소 봇에 좀비가 섞인다**. `botsHideFromZombies` false(봇이 좀비를 피해 숨지 않는다, 클라이언트 플러그인 필요).
  - `traderMessage` false — 로그인 때 Fence 가 보내는 영어 소문 메시지를 끈다.
  - 바꾸려면 서버 웹 `https://127.0.0.1:6969/ppes` 에서. 계절 이벤트 변경은 **서버 재시작** 후 적용.
- 설치: 서버 정지(사용자 승인, `tools/stop-spt-server.ps1`, 게임 꺼짐) → 프로필 11개 백업 `backup/profiles-before-stage15-A/`(정지 뒤 다시 떴다) → `install-and-test -Stage stage15 -Pattern '3232__*' -Tag 'stage15-A-3232-PPES'` — **8파일**(해제본 7 + 미리 넣은 `config.json`).
- 시험·재기동 모두: `ModLoader: 총 184 개`(183 → 184), `Seasonal: Halloween on (on) … Halloween parts: ritual off, zombies on, zombie waves off, cultists off, Lab infected only off, free Lab off, 2026 outbreak off, loot off`, `No past events picked`, `Registered 5 built-in factions with SAIN`, `Event quests ready (146)`. 오류는 `Could not add` 고유 48 · bundle 1 로 **기준선 그대로**. 설치본 `config.json` 은 넣은 것과 **md5 같다**(첫 기동이 덮어쓰지 않았다).
- 프로필: 11개 모두 달라졌으나 바뀐 곳은 **`spt.migrations.TraderGenPocketMigration` 하나뿐**(다른 모드의 이전 기록 시각). PPES 가 넣는 이벤트 퀘스트는 아직 없다 — 로그인 때 들어올 수 있다.
- 서버는 터미널 탭 `SPT server` 에 다시 띄웠다(14:40 무렵 6969 응답 200).

**아직 확인하지 않은 것**

- 게임 기동: 플러그인 수(예상 256 = AirdropProbe −1, PPES 클라이언트 +1)와 `Loading [` 줄.
- **레이드에서 좀비가 실제로 나오고 때리는지**(SAIN 과 PPES 클라이언트 플러그인). 감염률 기본값으로 **Customs·Interchange 는 0** 이라 그 두 맵에는 안 나온다 — 넣으려면 PPES 의 레이드 미리보기 → 스폰 편집기.
- 할로윈을 켜면 SPT 가 봇의 할로윈 마스크·이벤트 장비도 함께 켠다(PPES 가 나누지 않는다). 할로윈 퀘스트가 퀘스트 목록에 보일 수 있다.
- 서버 웹 첫 화면 모드 카드: ModCardKorean 이 `사전에 없어 원문으로 남은 카드 1 개: Past & Present Event Selector (/ppes)` — 한글 카드 사전에 아직 없다.
