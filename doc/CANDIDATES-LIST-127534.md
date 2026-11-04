# Encode's Mod List Vol. 1 에서 뽑은 미설치 후보 (2026-11-04)

공개 모드 목록 [Encode's Mod List Vol. 1](https://sp-mod.com/list/127534/encodes-mod-list-vol-1)(SPT 4.1.6, 모드 215 + addon 2)을 긁어 이 설치본에 **없는 것만** 남긴 목록이다.

## 어떻게 추렸나

- 목록 페이지에서 Forge 모드 id **214개**를 뽑았다. addon 2개(RaidReviewOverlay, PIT Fireteam — SAIN Tactics)는 자체 링크·버전이 없어 세지 않았다.
- `doc/INSTALLED-MODS.md` 의 `sp-mod.com/mod/<id>` 링크에서 설치된 id **190개**를 뽑아 id 로 대조했다. 이름으로도 교차 검증해 id 불일치 의심 6건을 추렸으나 전부 부분 일치 오탐이었다.
- **이미 설치됨 84개 / 미설치 130개.** 미설치 130개는 Forge API(`/api/v0/mod/<id>`, `/versions?filter[spt_version]=4.1.6`)로 버전·용량·결속·의존성·Fika 를 받아 아래 표로 만들었다. 요청 250건, 오류 0건.
- 원자료는 `stage2-survey/list-127534-not-installed.tsv`(id·이름·버전·호환·다운로드·결속·바이트·Fika·의존성).
- 설치도 다운로드도 하지 않았다. 용량·결속·의존성은 API 값 그대로이고, 실제 충돌은 `tools/check-conflicts.mjs` 를 거쳐야 안다.

## 먼저 걸리는 것

- **PIT Fireteam(2676)은 다시 넣으면 안 된다.** 설치 문서에는 아직 설치된 것으로 적혀 있어 "이미 설치됨"으로 분류됐지만, 실제로는 SPT Team Manager 를 넣을 때 빼냈다(`backup/2026-11-01-before-stm/`). 같은 `/singleplayer/pitfireteam/*` 라우트를 써서 함께 둘 수 없다. 설치 기록에 제거가 적히지 않아 자동 생성 문서가 뒤처진 것이다 — 기록을 고쳐야 한다.
- **역할이 겹치는 것** — Amands's Graphics(592) ↔ 설치된 Amands' Sense, DynamicMaps Extended(3097) ↔ 설치된 Dynamic Maps, APBS/ABPS(1594·2097) ↔ 설치된 SAIN·MoreBotsAPI. 택일이 필요하다.
- **Six 시리즈**(CloudSix·FogSix·ReflectSix·AOSix·POMSix·TraceSix)는 같은 제작자의 그래픽 묶음이라 서로 전제가 있을 수 있다. 한꺼번에 넣지 말고 하나씩 본다.
- **UnityToolkit(1426)이 막는 3개** — Manimal 백포트 셋(2866 ICEBREAKER, 3025 Interchange, 3022 Lighthouse)이 UnityToolkit 을 의존하는데, 그 모드 자체가 **4.1.5 까지만 있어 설치 불가 목록에 있다**. 선행 모드가 4.1.6 으로 올라오기 전에는 이 셋을 넣을 수 없다.
- **용량** — 125개를 모두 받으면 **약 14.8GB**다. 1GB 넘는 것이 7개: Arena Map Port 3.6GB, Lighthouse Rework 2.3GB, DynamicMaps Extended 1.7GB, ICEBREAKER 1.6GB, Epic's All in One 1.5GB, Interchange Expansion 1.4GB, ECOT 1.1GB. Epic's All in One(1263)과 Russian Special Forces Pack(3035)은 `CANDIDATES-GUN-TRADER.md` 에서 이미 한 번 검토했다.
- **결속 18개** — 표의 `결속` 표시가 그것이다. 플레이 후 제거하면 프로필이 깨질 수 있어, 프로필 백업 후 하나씩 넣는다.

## 설치할 수 없는 5개

| 모드 | id | 버전 | 상태 |
| --- | --- | --- | --- |
| [Eco's Attachment Emporium](https://sp-mod.com/mod/2882) | 2882 | 3.0.0 | SPT 4.1.1 까지 — 4.1.6 버전 없음 |
| [Tylevo's Tactical Service Control](https://sp-mod.com/mod/2791) | 2791 | 1.3.13 | SPT 4.1.5 까지 — 4.1.6 버전 없음 |
| [Better Flashlights](https://sp-mod.com/mod/3084) | 3084 | 1.0.1 | **더 이상 제공되지 않음** |
| [Natalya Trader](https://sp-mod.com/mod/2445) | 2445 | 1.0.1 | SPT 4.1.3 까지 — 4.1.6 버전 없음 |
| [BossNotifier ](https://sp-mod.com/mod/2543) | 2543 | 1.2.1 | SPT 4.1.3 까지 — 4.1.6 버전 없음 |

## 설치 가능 125개

분야는 이름·소개를 보고 나눈 것이고, 표의 수치는 4.1.6 호환 최신 버전 기준이다.

### 맵·레이드 콘텐츠 (14)

| 모드 | id | 버전 | 크기 | 다운로드 | 표시 | Fika | 의존성 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [Manimal's 1.0 Labs Office Backport](https://sp-mod.com/mod/2937) | 2937 | 2.0.0 | 90MB | 32.4K | - | 불명 | WTT - CommonLib, WTT - Content Backport |
| [Enable Labyrinth](https://sp-mod.com/mod/2416) | 2416 | 1.1.0 | 157KB | 64.9K | - | 호환 | - |
| [MANIMAL'S ICEBREAKER BACKPORT](https://sp-mod.com/mod/2866) | 2866 | 1.1.3 | 1595MB | 80.4K | - | 호환 | BigBrain, WTT - CommonLib, MoreBotsAPI, WTT - Black Division [REDACTED] Home, WTT - Content Backport, Climbable Ladders, Manimal's CS Gas, Manimal's 1.0 Interchange Expansion Backport , UnityToolkit |
| [Manimal's 1.0 Interchange Expansion Backport ](https://sp-mod.com/mod/3025) | 3025 | 1.0.8 | 1440MB | 41.8K | - | 불명 | UnityToolkit, WTT - CommonLib, WTT - Content Backport |
| [Manimal's 1.0 Lighthouse Rework Backport ](https://sp-mod.com/mod/3022) | 3022 | 1.0.7 | 2342MB | 24.3K | - | 불명 | UnityToolkit, WTT - CommonLib, WTT - Content Backport |
| [Map Variants](https://sp-mod.com/mod/3040) | 3040 | 1.1.0 | 4MB | 14.6K | - | 호환 | - |
| [Arena Map Port](https://sp-mod.com/mod/3166) | 3166 | 0.4.0 | 3659MB | 1.3K | - | 불명 | - |
| [Arena Mode](https://sp-mod.com/mod/3175) | 3175 | 0.4.1 | 4MB | 1.8K | - | 비호환 | SAIN - Solarint's AI Modifications - Full AI Combat System Replacement, APBS - Acid's Progressive Bot System, ABPS - Acid's Bot Placement System |
| [Content Backport - Prestiges](https://sp-mod.com/mod/2540) | 2540 | 1.1.0 | 21MB | 66.2K | **결속** | 호환 | WTT - CommonLib, WTT - Content Backport |
| [WTT - Black Division [REDACTED] Home](https://sp-mod.com/mod/2511) | 2511 | 1.3.1 | 1MB | 130.9K | - | 호환 | SAIN - Solarint's AI Modifications - Full AI Combat System Replacement, BigBrain, WTT - CommonLib, MoreBotsAPI, WTT - Content Backport |
| [UNTAR Go Home!](https://sp-mod.com/mod/2342) | 2342 | 3.2.1 | 174KB | 45.2K | - | 호환 | SAIN - Solarint's AI Modifications - Full AI Combat System Replacement, BigBrain, WTT - CommonLib, MoreBotsAPI |
| [RUAF Come Home!](https://sp-mod.com/mod/2427) | 2427 | 1.2.1 | 10MB | 42.2K | - | 호환 | BigBrain, WTT- Armory, WTT - CommonLib, MoreBotsAPI, WTT - Content Backport, SAIN - Solarint's AI Modifications - Full AI Combat System Replacement |
| [Door Randomizer](https://sp-mod.com/mod/820) | 820 | 1.8.0 | 5KB | 50.7K | - | 호환 | - |
| [LET ME OUT](https://sp-mod.com/mod/2240) | 2240 | 1.1.0 | 6KB | 11.7K | - | 호환 | - |

### 총기·장비·아이템 콘텐츠 (7)

| 모드 | id | 버전 | 크기 | 다운로드 | 표시 | Fika | 의존성 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [Epic's All in One](https://sp-mod.com/mod/1263) | 1263 | 5.0.2 | 1509MB | 287.0K | **결속** | 호환 | WTT - CommonLib |
| [Russian Special Forces Pack](https://sp-mod.com/mod/3035) | 3035 | 2.0.1 | 560MB | 10.5K | **결속** | 호환 | WTT - CommonLib |
| [ECOT - Eukyre's Consortium of Things](https://sp-mod.com/mod/2195) | 2195 | 2.0.1 | 1161MB | 34.1K | **결속** | 불명 | Epic's All in One, WTT - CommonLib |
| [Rook ](https://sp-mod.com/mod/3055) | 3055 | 1.2.2 | 747KB | 6.6K | **결속** | 호환 | Epic's All in One, ECOT - Eukyre's Consortium of Things, WTT- Armory, WTT - CommonLib, WTT - Content Backport, Eco's Attachment Emporium |
| [Manimal's CS Gas](https://sp-mod.com/mod/2864) | 2864 | 2.0.0 | 12MB | 44.3K | **결속** | 호환 | WTT - CommonLib, WTT - Content Backport |
| [WTT Halloween Masks](https://sp-mod.com/mod/2401) | 2401 | 2.0.0 | 87MB | 1.1K | **결속** | 호환 | WTT - CommonLib |
| [Rippers Casino](https://sp-mod.com/mod/3212) | 3212 | 1.0.1 | 5MB | 657 | - | 불명 | - |

### 봇·AI (6)

| 모드 | id | 버전 | 크기 | 다운로드 | 표시 | Fika | 의존성 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [APBS - Acid's Progressive Bot System](https://sp-mod.com/mod/1594) | 1594 | 2.4.1 | 1012KB | 265.9K | - | 호환 | - |
| [ABPS - Acid's Bot Placement System](https://sp-mod.com/mod/2097) | 2097 | 2.1.0 | 1MB | 199.7K | - | 호환 | - |
| [Bot Callsigns - Reloaded](https://sp-mod.com/mod/1873) | 1873 | 2.0.6 | 20KB | 72.7K | - | 호환 | - |
| [Realistic PMC Names](https://sp-mod.com/mod/3064) | 3064 | 1.1.0 | 60KB | 3.0K | - | 불명 | - |
| [Smooth Talker](https://sp-mod.com/mod/2635) | 2635 | 1.3.0 | 11KB | 16.2K | - | 호환 | - |
| [Auto IFF](https://sp-mod.com/mod/2784) | 2784 | 2.1.1 | 15KB | 6.1K | - | 호환 | - |

### 그래픽·연출 (16)

| 모드 | id | 버전 | 크기 | 다운로드 | 표시 | Fika | 의존성 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [Amands's Graphics](https://sp-mod.com/mod/592) | 592 | 1.8.0 | 32KB | 880.3K | - | 호환 | - |
| [HollywoodFX](https://sp-mod.com/mod/2003) | 2003 | 3.0.0 | 258MB | 228.8K | - | 호환 | - |
| [CloudSix - Volumetric Clouds](https://sp-mod.com/mod/2567) | 2567 | 4.0.2 | 7MB | 22.8K | - | 호환 | - |
| [FogSix](https://sp-mod.com/mod/3110) | 3110 | 1.0.1 | 108KB | 2.3K | - | 호환 | - |
| [ReflectSix](https://sp-mod.com/mod/3099) | 3099 | 1.0.1 | 295KB | 2.7K | - | 호환 | - |
| [AOSix](https://sp-mod.com/mod/2569) | 2569 | 2.0.1 | 28KB | 10.9K | - | 호환 | - |
| [POMSix](https://sp-mod.com/mod/3100) | 3100 | 1.0.2 | 16MB | 3.1K | - | 호환 | - |
| [TraceSix - Full Raytracing and Voxel GI](https://sp-mod.com/mod/3221) | 3221 | 1.0.1 | 690KB | 1.9K | - | 호환 | - |
| [Better Night Skies](https://sp-mod.com/mod/2734) | 2734 | 2.0.0 | 68MB | 10.8K | - | 호환 | - |
| [Blackout](https://sp-mod.com/mod/2843) | 2843 | 4.1.1 | 6MB | 30.2K | - | 호환 | WTT - Content Backport |
| [Borkel's Realistic Night Vision Goggles (NVGs and T-7)](https://sp-mod.com/mod/954) | 954 | 3.0.4 | 32MB | 272.4K | **결속** | 불명 | - |
| [Shadow Flicker Fix](https://sp-mod.com/mod/1621) | 1621 | 1.5.3 | 7KB | 79.9K | - | 호환 | - |
| [Headshot Darkness](https://sp-mod.com/mod/1582) | 1582 | 1.2.5 | 10KB | 54.7K | - | 호환 | - |
| [Ragdoll Kinetics](https://sp-mod.com/mod/2905) | 2905 | 1.2.0 | 44KB | 8.1K | - | 불명 | - |
| [Manimal's Watch Animations](https://sp-mod.com/mod/2745) | 2745 | 2.0.0 | 86KB | 8.1K | - | 불명 | - |
| [BushWhacker - Standalone](https://sp-mod.com/mod/2329) | 2329 | 1.7.0 | 6KB | 22.3K | - | 불명 | - |

### 음향 (2)

| 모드 | id | 버전 | 크기 | 다운로드 | 표시 | Fika | 의존성 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [Battle Ambience](https://sp-mod.com/mod/2264) | 2264 | 2.4.0 | 11MB | 31.6K | - | 호환 | - |
| [Music Extender](https://sp-mod.com/mod/2962) | 2962 | 1.2.0 | 122MB | 3.2K | - | 호환 | - |

### UI·표시 (15)

| 모드 | id | 버전 | 크기 | 다운로드 | 표시 | Fika | 의존성 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [WTT - Menu Overhaul](https://sp-mod.com/mod/1775) | 1775 | 1.3.0 | 761KB | 131.7K | - | 호환 | - |
| [Deploy Screen Overhaul](https://sp-mod.com/mod/3047) | 3047 | 1.1.0 | 62MB | 3.0K | **결속** | 호환 | - |
| [ImpactCue-UI-MOD](https://sp-mod.com/mod/3029) | 3029 | 2.0.3 | 40MB | 13.2K | - | 호환 | - |
| [Hj's Improved Customization UI](https://sp-mod.com/mod/3069) | 3069 | 1.1.2 | 184KB | 6.0K | - | 호환 | - |
| [Hj's Task Item Indicator](https://sp-mod.com/mod/3082) | 3082 | 1.0.1 | 9KB | 4.3K | - | 호환 | - |
| [Hj's Task Turn in Indicator](https://sp-mod.com/mod/3071) | 3071 | 1.0.0 | 21KB | 2.5K | - | 호환 | - |
| [DynamicMaps Extended](https://sp-mod.com/mod/3097) | 3097 | 2.5.2 | 1739MB | 16.0K | - | 호환 | Dynamic Maps, Map Variants |
| [Name Plate](https://sp-mod.com/mod/3131) | 3131 | 1.1.0 | 2MB | 871 | - | 호환 | - |
| [Briefing](https://sp-mod.com/mod/3158) | 3158 | 1.0.2 | 385KB | 2.6K | - | 불명 | Quest Codex |
| [Breakout Outlines](https://sp-mod.com/mod/3007) | 3007 | 1.2.0 | 39KB | 6.4K | - | 호환 | - |
| [Wishlist Extended](https://sp-mod.com/mod/2500) | 2500 | 1.2.0 | 53KB | 8.5K | - | 호환 | - |
| [Inventory Walker](https://sp-mod.com/mod/3058) | 3058 | 0.3.1 | 13KB | 1.3K | - | 불명 | - |
| [Easy Ammo Names](https://sp-mod.com/mod/1262) | 1262 | 2.1.0 | 16KB | 31.5K | - | 불명 | - |
| [More Tag Colours](https://sp-mod.com/mod/1533) | 1533 | 1.5.0 | 5KB | 26.0K | - | 호환 | - |
| [Item Preview QoL + screenshots](https://sp-mod.com/mod/2206) | 2206 | 1.2.0 | 12KB | 18.9K | **결속** | 불명 | - |

### 퀘스트 (4)

| 모드 | id | 버전 | 크기 | 다운로드 | 표시 | Fika | 의존성 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [Quest Backport](https://sp-mod.com/mod/3026) | 3026 | 1.0.9 | 695KB | 11.1K | **결속** | 불명 | WTT - CommonLib, WTT - Content Backport |
| [Quest Codex](https://sp-mod.com/mod/3054) | 3054 | 1.11.0 | 3MB | 5.7K | - | 불명 | - |
| [All Quests Checkmarks](https://sp-mod.com/mod/2025) | 2025 | 1.4.0 | 45KB | 67.1K | - | 호환 | - |
| [Expanded Task Text (ETT)](https://sp-mod.com/mod/2153) | 2153 | 2.1.0 | 50KB | 20.2K | - | 호환 | - |

### 편의·인벤토리 (12)

| 모드 | id | 버전 | 크기 | 다운로드 | 표시 | Fika | 의존성 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [MergeConsumables](https://sp-mod.com/mod/1657) | 1657 | 1.6.3 | 20KB | 91.9K | - | 호환 | - |
| [Continuous Healing](https://sp-mod.com/mod/1884) | 1884 | 1.6.3 | 6KB | 148.2K | - | 호환 | - |
| [Healing Autocancel](https://sp-mod.com/mod/1274) | 1274 | 5.0.1 | 4KB | 93.0K | - | 호환 | - |
| [HandsAreNotBusy](https://sp-mod.com/mod/1298) | 1298 | 1.7.1 | 5KB | 171.3K | - | 호환 | - |
| [Gilded Key Storage](https://sp-mod.com/mod/865) | 865 | 2.1.1 | 6MB | 178.2K | - | 호환 | - |
| [Use Loose Loot](https://sp-mod.com/mod/933) | 933 | 1.6.0 | 3KB | 114.0K | - | 불명 | - |
| [Loot In Vicinity](https://sp-mod.com/mod/2698) | 2698 | 3.0.6 | 45KB | 17.1K | - | 비호환 | - |
| [Cabinet Looter](https://sp-mod.com/mod/3103) | 3103 | 1.0.3 | 19KB | 14.6K | - | 호환 | - |
| [IOF: Inventory Organizing Feature](https://sp-mod.com/mod/2960) | 2960 | 1.10.1 | 24KB | 2.8K | **결속** | 호환 | - |
| [Raid Settings Skipper](https://sp-mod.com/mod/2857) | 2857 | 1.3.1 | 5KB | 4.0K | - | 호환 | - |
| [AutoKeycard for Labs](https://sp-mod.com/mod/2881) | 2881 | 1.0.1 | 6KB | 9.4K | - | 호환 | - |
| [Dynamic Item Weights](https://sp-mod.com/mod/2344) | 2344 | 1.2.1 | 7KB | 16.6K | - | 호환 | - |

### 하이드아웃 (7)

| 모드 | id | 버전 | 크기 | 다운로드 | 표시 | Fika | 의존성 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [Hideout In Progress](https://sp-mod.com/mod/2076) | 2076 | 3.0.0 | 22KB | 59.8K | - | 호환 | - |
| [Hideout Overhaul](https://sp-mod.com/mod/2982) | 2982 | 0.2.0 | 67KB | 4.1K | - | 불명 | - |
| [Vertical Hideout](https://sp-mod.com/mod/2530) | 2530 | 1.1.1 | 8KB | 39.7K | - | 불명 | - |
| [Target Dummies](https://sp-mod.com/mod/2932) | 2932 | 0.3.1 | 15KB | 2.1K | - | 호환 | - |
| [Tarkin's Hideout Cat](https://sp-mod.com/mod/2908) | 2908 | 1.1.1 | 20MB | 7.6K | - | 호환 | - |
| [Cultist Circle Improvements](https://sp-mod.com/mod/2657) | 2657 | 1.1.0 | 119KB | 14.1K | - | 호환 | - |
| [Simple Workout QTE](https://sp-mod.com/mod/1437) | 1437 | 2.2.2 | 13KB | 72.0K | - | 호환 | - |

### 전리품·경제 (10)

| 모드 | id | 버전 | 크기 | 다운로드 | 표시 | Fika | 의존성 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [Lots of Loot Redux](https://sp-mod.com/mod/1619) | 1619 | 4.2.2 | 110KB | 260.3K | - | 호환 | - |
| [Rai's Hidden Caches](https://sp-mod.com/mod/1265) | 1265 | 1.4.0 | 8KB | 110.8K | - | 호환 | - |
| [Strange Curios](https://sp-mod.com/mod/3059) | 3059 | 1.1.0 | 26MB | 2.0K | **결속** | 호환 | WTT - CommonLib |
| [Tushonka Rare Collectibles](https://sp-mod.com/mod/3062) | 3062 | 1.2.2 | 73MB | 4.0K | **결속** | 호환 | - |
| [PityLoot](https://sp-mod.com/mod/3077) | 3077 | 1.0.0 | 45KB | 1.6K | **결속** | 불명 | - |
| [Bosses Have Lega Medals](https://sp-mod.com/mod/1539) | 1539 | 2.1.0 | 8KB | 222.3K | - | 호환 | - |
| [Bosses Have Gp Coins](https://sp-mod.com/mod/2523) | 2523 | 1.1.0 | 8KB | 48.9K | - | 호환 | - |
| [LootNET](https://sp-mod.com/mod/2679) | 2679 | 1.2.0 | 946KB | 33.3K | - | 호환 | - |
| [Crates In Airdrop](https://sp-mod.com/mod/2481) | 2481 | 2.0.0 | 8KB | 19.7K | - | 호환 | - |
| [Fast Sell In Flea](https://sp-mod.com/mod/2207) | 2207 | 1.3.1 | 12KB | 81.8K | - | 호환 | - |

### 진행·스킬·체력 (6)

| 모드 | id | 버전 | 크기 | 다운로드 | 표시 | Fika | 의존성 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [Skills Extended](https://sp-mod.com/mod/2383) | 2383 | 3.1.4 | 110MB | 62.7K | **결속** | 호환 | - |
| [Skill Distribution](https://sp-mod.com/mod/2144) | 2144 | 1.3.0 | 27KB | 42.8K | - | 호환 | - |
| [Health Per Level](https://sp-mod.com/mod/1706) | 1706 | 2.2.0 | 37KB | 48.7K | - | 불명 | - |
| [Fair Progression](https://sp-mod.com/mod/2652) | 2652 | 1.1.0 | 28KB | 2.9K | - | 호환 | - |
| [TraumaCore](https://sp-mod.com/mod/2893) | 2893 | 1.7.0 | 106KB | 6.3K | **결속** | 불명 | - |
| [Broken Limbs Don't Heal](https://sp-mod.com/mod/3121) | 3121 | 1.1.0 | 9KB | 415 | - | 불명 | Effects Manager Framework |

### 전투·조작·이동 (14)

| 모드 | id | 버전 | 크기 | 다운로드 | 표시 | Fika | 의존성 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [Bring Back Concussion](https://sp-mod.com/mod/2550) | 2550 | 1.2.1 | 12KB | 13.4K | - | 호환 | - |
| [CookingGrenades 4.1](https://sp-mod.com/mod/2951) | 2951 | 1.4.4 | 2MB | 3.5K | - | 호환 | - |
| [Inspectionless Malfunctions Reborn](https://sp-mod.com/mod/2992) | 2992 | 1.0.0 | 5KB | 3.4K | - | 불명 | - |
| [Deminvincibility](https://sp-mod.com/mod/1117) | 1117 | 2.2.0 | 8KB | 62.7K | - | 호환 | - |
| [Smajlec's Stamina](https://sp-mod.com/mod/2608) | 2608 | 1.4.0 | 5KB | 10.1K | - | 호환 | - |
| [Classic Movement](https://sp-mod.com/mod/1860) | 1860 | 1.1.3 | 62KB | 145.1K | - | 호환 | - |
| [Climbable Ladders](https://sp-mod.com/mod/2649) | 2649 | 1.0.4 | 920KB | 91.5K | - | 호환 | - |
| [Increase Climb Height](https://sp-mod.com/mod/1575) | 1575 | 2.1.1 | 8KB | 128.2K | - | 호환 | - |
| [Increase Look Direction](https://sp-mod.com/mod/2121) | 2121 | 1.3.0 | 5KB | 27.6K | - | 호환 | - |
| [Auto Shoulder Swap](https://sp-mod.com/mod/2639) | 2639 | 1.1.1 | 5KB | 12.0K | - | 불명 | - |
| [No Aim Swing](https://sp-mod.com/mod/2719) | 2719 | 1.2.0 | 4KB | 4.6K | - | 호환 | - |
| [Deadzone](https://sp-mod.com/mod/3067) | 3067 | 1.2.0 | 7KB | 1.1K | - | 불명 | - |
| [Lacy's PvE Tweaks](https://sp-mod.com/mod/2395) | 2395 | 1.3.2 | 18KB | 43.6K | **결속** | 호환 | - |
| [ORBIT 3.0](https://sp-mod.com/mod/2706) | 2706 | 3.1.0 | 807KB | 68.8K | - | 호환 | SAIN - Solarint's AI Modifications - Full AI Combat System Replacement, Waypoints - Expanded Navmesh, BigBrain |

### 도구·프레임워크·성능 (12)

| 모드 | id | 버전 | 크기 | 다운로드 | 표시 | Fika | 의존성 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [Raid Review](https://sp-mod.com/mod/1479) | 1479 | 1.10.0 | 5MB | 59.7K | - | 호환 | - |
| [WebOverlay](https://sp-mod.com/mod/2879) | 2879 | 1.11.0 | 170KB | 12.1K | - | 호환 | - |
| [Combat Log](https://sp-mod.com/mod/3072) | 3072 | 1.0.0 | 123KB | 710 | - | 비호환 | WebOverlay |
| [Integrated Configuration Manager](https://sp-mod.com/mod/3095) | 3095 | 1.0.0 | 19KB | 923 | - | 호환 | - |
| [Hj's Mod Source](https://sp-mod.com/mod/3083) | 3083 | 1.1.0 | 39KB | 2.7K | - | 호환 | - |
| [Effects Manager Framework](https://sp-mod.com/mod/3123) | 3123 | 1.1.0 | 30KB | 1.5K | - | 호환 | - |
| [ASBP - Acid's Scalable BepinEx Panel](https://sp-mod.com/mod/2931) | 2931 | 1.0.1 | 8KB | 2.1K | - | 호환 | - |
| [Better BepInEx](https://sp-mod.com/mod/3207) | 3207 | 1.0.3 | 61KB | 4.4K | - | 호환 | - |
| [Item Property Backport](https://sp-mod.com/mod/2689) | 2689 | 1.5.2 | 128KB | 13.2K | - | 호환 | - |
| [Performance Scope](https://sp-mod.com/mod/3076) | 3076 | 0.2.0 | 11KB | 1.9K | - | 불명 | - |
| [Dynamic External Resolution Patch (DERP)](https://sp-mod.com/mod/2200) | 2200 | 1.1.2 | 7KB | 28.1K | - | 불명 | - |
| [Knight Mask Fix](https://sp-mod.com/mod/3109) | 3109 | 1.0.0 | 2MB | 3.5K | - | 호환 | - |
