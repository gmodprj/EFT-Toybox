# 외부 설치 모드 업데이트 점검 — 2026-11-09

이 저장소에서 만든 모드(`src/` 아래)는 대상이 아니다. **Forge·GitHub·Nexus 에서 받아 설치한
모드**만 본다. 설치 목록은 [INSTALLED-MODS.md](INSTALLED-MODS.md), 설치 절차는
[PROCEDURE.md](PROCEDURE.md).

## 한 줄

설치된 외부 모드 **304개**(Forge 300 + Forge 밖 4) 중 **업데이트가 있는 것은 20개**다. 그중
**설치 후 내용이 달라진 것은 4개**뿐이고(ISB 자막 번역 1 · 설정 파일 1 · 모드가 쓴 파일 2),
**나머지 16개는 해제본 원본과 바이트까지 같아 그냥 덮어써도 된다.** 내려받을 양이
**약 12.9GB** 이고 그 91%가 큰 콘텐츠 모드 넷(Arena Map Port 5.1GB · ISB Aishi 4.2GB ·
Epic's AIO 1.5GB · GF2 Skin Pack 1.1GB)이다.

## 어떻게 쟀나

| 재는 것 | 도구 | 근거 |
| --- | --- | --- |
| 최신 호환 버전 | `node tools/check-mod-updates.mjs` | Forge API `GET /mod/<id>/versions?filter[spt_version]=4.1.6` 의 최대 버전 |
| 설치 후 수정 여부 | `node tools/check-installed-vs-extracted.mjs` | 설치 기록의 경로마다 `E:\Spt\<경로>` 와 `extracted/<단계>/<모드>/<경로>` 의 **md5 대조** |

둘 다 읽기만 한다. 판정 기준을 못 박아 둔다.

- **설치 기록이 0줄인 모드는 설치된 것이 아니다.** 매니페스트에는 315개가 있는데 11개는
  `install-records/removed/` 에 제거 기록이 있다(All In Weapon, PIT Fireteam, The Quartermaster 등).
  이것들을 업데이트 대상에 넣으면 **지운 모드를 되살리는 일**이 된다.
- **조회 실패를 "업데이트 없음" 으로 삼키지 않는다.** 첫 조회에서 7개가 전송 실패로 떨어졌는데,
  다시 재 보니 그중 **ECOTI 하나는 실제로 업데이트가 있었다**(3.4.2 → 3.5.0). 도구는 실패를
  실패로 적고 종료 코드 1 을 낸다.
- **9차 모드 9개는 해제본이 없어 수정 여부를 못 쟀다.** 절차 밖에서 들어와 설치 기록을 뒤늦게
  복원한 것들이라 비교할 원본이 없다. "같다" 가 아니라 **"모른다"** 로 둔다.

## 1. 업데이트가 있는 모드 20개

`결속` = 프로필 결속(Forge `shows_profile_binding_notice`). 플레이한 뒤 되돌리면 프로필이 깨질 수 있다.

| id | 모드 | 설치 → 최신 | 공개 | 크기 | 결속 | 의존 | 설치 후 손댄 흔적 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 3166 | Arena Map Port | 0.4.0 → 0.5.0 | 11-07 | 5121.9MB | - | - | 없음 |
| 2478 | ISB Aishi | 2.0.4 → 2.0.7 | 11-08 | 4168.7MB | **결속** | WTT CommonLib, WTT Content Backport | **못 쟀다**(해제본 없음) |
| 1263 | Epic's All in One | 5.0.2 → 5.0.3 | 11-08 | 1509.4MB | **결속** | WTT CommonLib | 없음 |
| 3156 | GF2 Skin Pack | 1.0.0 → 1.2.4 | 11-08 | 1144.8MB | - | WTT Head&Voice, WTT CommonLib | **못 쟀다**(해제본 없음) |
| 2485 | ISB Special Forces | 2.0.1 → 2.0.4 | 11-08 | 781.7MB | - | BigBrain, MoreBotsAPI, ISB Aishi | **우리 자막 번역** + 못 쟀다 |
| 1453 | Make Shotguns Great Again! | 1.19.1 → 1.20.0 | 10-31 | 416.3MB | **결속** | WTT CommonLib | 없음 |
| 2930 | ECOTI | 3.4.2 → 3.5.0 | 11-01 | 31.5MB | **결속** | - | 모드가 만든 파일 15개 |
| 3054 | Quest Codex | 1.11.1 → 1.12.0 | 11-07 | 2.9MB | - | - | 없음 |
| 3158 | Briefing | 1.1.0 → 1.2.0 | 11-07 | 0.4MB | - | **Quest Codex** | 없음 |
| 2844 | Ultimate Flea | 1.2.0 → 1.3.1 | 11-03 | 0.2MB | **결속** | - | 모드가 쓴 `economy_state.json` |
| 2879 | WebOverlay | 1.11.0 → 1.12.0 | 11-08 | 0.2MB | - | - | 없음 |
| 3074 | Direct Reload & Painless Meds | 1.3.2 → 1.4.1 | 10-29 | 0.1MB | - | - | 없음 |
| 1305 | Repair Max Durability | 2.2.0 → 2.2.1 | 11-05 | 0.0MB | **결속** | - | 없음 |
| 2426 | MoreBotsAPI | 2.1.1 → 2.1.2 | 11-06 | 0.0MB | - | BigBrain | **못 쟀다**(해제본 없음) |
| 3194 | OpenBarters | 1.0.1 → 1.1.3 | 11-07 | 0.0MB | - | - | 없음 |
| 3095 | Integrated Configuration Manager | 1.0.0 → 1.1.0 | 11-06 | 0.0MB | - | - | 없음 |
| 2907 | Stash Filter | 1.0.0 → 1.1.0 | 11-02 | 0.0MB | - | - | 없음 |
| 2949 | Realistic Insurance | 0.2.1 → 0.2.3 | 11-02 | 0.0MB | - | - | **`config/config.json` 이 원본과 다르다** |
| 3190 | BiC - Bind in Container | 1.0.1 → 1.0.2 | 11-02 | 0.0MB | - | - | 없음 |
| 2859 | Compatibility Highlighter | 1.3.0 → 1.3.2 | 10-30 | 0.0MB | - | - | 없음 |

`0.0MB` 는 반올림이다(수십~수백 KB).

### 업데이트가 아닌데 눈에 걸리는 것 3개

| id | 모드 | 무슨 일 | 어떻게 할까 |
| --- | --- | --- | --- |
| 1025 | **Painter** (3.0.0, 결속) | Forge API 가 `NOT_FOUND` 를 돌려준다 — **모드 페이지가 사라졌다** | 업데이트 불가. 설치본은 그대로 돌고 있으므로 **그냥 둔다.** 지우면 다시 받을 곳이 없고 결속이라 프로필도 위험하다 |
| 1426 | UnityToolkit (2.0.2) | 4.1.6 호환으로 올라온 버전이 **0개**(최신 2.0.1 이 `>=4.0.0 <4.1.0`) | 의존 라이브러리고 지금 돌고 있다. **그냥 둔다** |
| 1277 | WTT - Head and Voice Selector | 설치본 **1.1.0** 이 Forge 최신 **1.0.8** 보다 높다 | 9차에 절차 밖으로 들어온 것이다. GF2 Skin Pack 이 이것을 의존하므로 GF2 를 올릴 때 **같이 확인**한다 |

Forge 밖 4개(`VanillaTradersRework-Reloaded`(Nexus), `PortableTraders`(GitHub),
`Quick Trader Load Times`(GitHub), `Game Panel HUD` 4.1.6 포팅본)는 API 로 못 본다. 앞의 셋은
배포 페이지를 손으로 보고, 포팅본은 우리가 만든 것이라 대상이 아니다.

**PIT Fireteam(2676) 1.1.1 이 11-05 에 나왔다.** PIT 자체는 제거돼 설치 대상이 아니지만
`src/SptTeamManager` 가 그 포크다(기준 커밋 `f32a41b`, 업스트림 1.0.0). 업스트림 머지는 이
점검과 별개의 일이다.

## 2. 설치 후 수정된 모드 가리기

설치 기록 **14001줄 전부**를 해제본과 md5 로 맞댔다. **설정 파일이 아닌 파일(DLL·번들·에셋)이
원본과 다른 모드는 0개**다 — 지금까지 남의 DLL 을 고친 적이 없다는 뜻이고, 업데이트의
대부분은 그냥 덮어쓰면 된다.

### 2-1. 우리가 일부러 고친 것 — 업데이트하면 **다시 입혀야 한다**

| 모드 | 파일 | 무엇을 했나 | 다시 입히는 길 |
| --- | --- | --- | --- |
| 505 Olympus | `MountOlympus\Modifiers\firearmMags.json` | 설치본에 없는 총기 12개를 빼 `AddMagazinesToFirearms` 가 중간에 멈추던 것을 고쳤다(1311줄 → 988줄) | [mod-patches/505-olympus-firearmMags.md](mod-patches/505-olympus-firearmMags.md), 원본·중간본이 같은 폴더에 있다 |
| 755 The Blacklist | `platinum-theblacklist\Config\config.jsonc` | 제작자가 막아 둔 아이템 858건을 `customItemConfigs` 에 되살렸다(252줄 → 5402줄) | [mod-patches/theblacklist-restore-author-bans.md](mod-patches/theblacklist-restore-author-bans.md) + `theblacklist-restored-ids.json` |
| 2667 PiP Disabler | `PiP-Disabler\custom_mesh_surgery_settings.json` | 모드가 인게임 저장 키로 쓰는 파일인데, 거기서 **배율 단계 이름으로 잘못 저장된 3개를 지웠다**(87 → 84) | [fixes/2026-10-30-pipdisabler-scope-misalign.md](fixes/2026-10-30-pipdisabler-scope-misalign.md), 보관본은 `backup/pip-disabler/` |
| 2949 Realistic Insurance | `config\config.json` | **0.2.3 으로 올릴 때 돌려놓은 보험 설정 6개** — `baseReturnChancePercent` 의 pmc 65 · playerScav 55 · scav 80 · boss 55, `looterExtractedChancePercent` 25, `looterDiedBonusPercent` 50. 새 판이 더한 `timeRemaining` 은 기본값으로 뒀다 | 아래 §5, 업데이트 전 설정은 `backup/before-update-2026-11-09/2949/` |
| 2485 ISB Special Forces | `ISBSpecialForces_Audios\Subtitles\locales\kr.json` | 무전·방송 자막 한국어 번역 | `tools/install-plugin-locales.ps1` 을 다시 돌린다 |
| 2924 Advanced Stash Sorting | `locales\kr.json` | 정렬 창 번역 | 같음 |
| 2500 Wishlist Extended | `locales\kr.json` | 위시리스트 번역 | 같음 |
| 2698 Loot In Vicinity | `locales\kr.json` | 주변 아이템 패널 번역 | 같음 |
| 236 SVM | `Loader\loader.json` + `Presets\Noname.json` | 프리셋 `Noname` 을 쓰도록 바꿨다. `Hideout.RemoveConstructionsRequirements` 는 **false 유지** | `backup/svm-presets/`, [PROCEDURE.md](PROCEDURE.md) SVM 메모 |

번역 넷은 **설치 기록에 없다**(Forge 모드가 아니라 우리 파일이다). 그래서 md5 대조에 걸리지
않는다 — 이 표에 손으로 적어 둔다. 맞는지는 `node tools/check-plugin-locales.mjs` 가 본다.

### 2-2. 설정 파일이 원본과 다른 것 — 모드·서버가 돌면서 다시 쓴 것으로 보인다

문서에 손댄 기록이 없고, 모양이 **모드가 다시 쓴 것**(주석이 사라졌거나, 빈 틀이 실제 값으로
채워졌거나, 상점 품목이 생성됨)이다. 그래도 **원본과 다르므로 업데이트 때 덮어쓰면 지금 값이
사라진다.**

| 모드 | 파일 | 다른 점 |
| --- | --- | --- |
| 374 bluehead's AIO Trader | `data\assort_aio_trader.json` | 773B → **3,015KB**(상점 품목 생성) |
| 1025 Painter | `db\assort.json` | 146줄 → 69줄 |
| 2556 Repair Skill Extension | `buffs.jsonc` | 주석이 사라지고 값이 바뀌었다(`CommonBuffMinChanceValue` 0.25 → 0.75) |
| 3025 Manimal Interchange | `BepInEx\config\com.manimal.interchange.cfg` | 3줄 → 28줄(첫 기동에 F12 항목이 펼쳐진 것) |
| 2430 ODT's Item Info · 2554 Change Helmet Visor · 2650 Holster Everything · 3024 CaseTrader | 각 `config.json` | 1~7줄 |

> **2556 은 값이 바뀌었으니 사람이 고친 것일 수 있다.** 모드가 다시 쓴 것인지 누가 바꾼 것인지
> 이 점검으로는 가려지지 않았다. 업데이트 대상도 아니므로 **지금은 그대로 두고**, 그 모드를
> 올릴 때 설정을 먼저 받아 둔다.

### 2-3. 설치 후 새로 생긴 파일 25개 모드 — 전부 모드·서버가 만든 것

`descriptionCache.json`·`economy_state.json`·`raid_review_mod.db`·`ids_<날짜>.json` 처럼 런타임
상태·캐시다. 그 가운데 몇 줄은 **엉뚱한 모드 몫으로 적혀 있다**(755 기록에 MeleeMultiplier 의
설정이, 2948·2516 기록에 Amonya 의 id 파일이 들어 있다) — 알려진
[기록 오기](install-records/README-removed-stale-lines-2026-11-06.md) 성질이고 유실이 아니다.

### 2-4. 못 쟀다 — 9차 모드 9개

`gh-gamepanelhud-port` · 1688 More Energy Drinks · 2426 MoreBotsAPI · 2512 WTT Content Backport ·
791 SAIN · 2478 ISB Aishi · 2485 ISB Special Forces · 3156 GF2 Skin Pack · 1277 WTT Head&Voice.

이 중 **업데이트 대상이 4개**(2478 · 2485 · 3156 · 2426)다. 수정 여부를 모르는 채 덮어쓰게
되므로, 그 넷은 **업데이트 전에 설치본 폴더를 통째로 `backup/` 에 복사**한다. 그러면 뒤늦게
무엇이 달라졌는지 비교할 수 있다.

## 3. 업데이트 진행 계획

### 3-0. 왜 묶음을 나누나

업데이트는 설치와 같은 위험을 지는데 한 가지가 더 있다 — **덮어쓸 파일이 이미 있다.**
`install-and-test.ps1` 은 덮어쓰기가 생기면 아무것도 복사하지 않고 멈춘다(그게 안전장치다).
그래서 업데이트는 "넣기" 가 아니라 **"지우고 넣기"** 이고, 지우는 근거는 그 모드의 설치 기록이다.

### 3-1. 모드 하나를 올리는 절차 (U1~U8)

```
U1  받는다       doc/download/manifest-update-<날짜>.json 에 적고 node tools/download-stage.mjs update-<날짜>
U2  푼다         /c/Windows/System32/tar.exe -xf <파일> -C extracted/update-<날짜>/<id>__<이름>
U3  견준다       새 해제본의 파일 목록 vs 지금 설치 기록 — 사라진 파일·새 경로를 먼저 본다
U4  받아 둔다    설치본의 그 모드 폴더 전체 → backup/before-update-<날짜>/<id>/
                 (결속 모드면 프로필까지: user\profiles\* → backup/profiles-before-update-<날짜>/)
U5  지운다       그 모드의 설치 기록에 적힌 경로만 삭제, 기록은 install-records/removed/ 로 옮긴다
U6  넣는다       tools/install-and-test.ps1 -Stage update-<날짜> -Pattern '<id>__*' -Tag 'update-<id>-<버전>'
U7  다시 입힌다  2-1 표에 그 모드가 있으면 손질·번역을 다시 올린다
U8  확인한다     매니페스트 버전 고치기 → gen-installed-mods.mjs → check-install-records.mjs (OK — 0/0)
                 → check-installed-vs-extracted.mjs → 서버 기동(모드 수·오류 48/1/1) → 게임 확인
```

**게임과 서버는 꺼진 상태에서 한다.** 서버 모드를 넣고 재시작하지 않으면 적용되지 않는다.

### 3-2. 묶음과 순서

의존 관계와 위험으로 가른다. **묶음을 끝낼 때마다 기동해 보고 다음 묶음으로 넘어간다.**
**U-A·U-B·U-C 는 2026-11-09 에 끝났고 U-D 는 절반이다 — 결과는 §4.**

| 묶음 | 모드 | 왜 이 순서인가 | 내려받을 양 |
| --- | --- | --- | --- |
| **U-A** 작고 독립 8개 | 2879 WebOverlay · 3074 Direct Reload · 3194 OpenBarters · 3095 ICM · 2907 Stash Filter · 3190 BiC · 2859 Compatibility Highlighter · 2949 Realistic Insurance | 의존이 없고 전부 1MB 이하다. 되돌리기도 쉽다. **2949 는 설정을 먼저 받아 둔다** | < 1MB |
| **U-B** Quest Codex → Briefing | 3054 → 3158 | Briefing 이 Quest Codex 를 **의존**한다. 반대로 하면 깨진다 | 3.3MB |
| **U-C** 결속 소형 2개 | 1305 Repair Max Durability · 2844 Ultimate Flea | 둘 다 결속이다. **하나씩**, 프로필 백업 뒤에. Ultimate Flea 는 `economy_state.json` 을 받아 둔다 | 0.2MB |
| **U-D** 결속 중형 1개 | 2930 ECOTI | 결속. `nvghostcompat/` 15개는 스스로 다시 만든다 | 32MB |
| **U-E** 결속 대형 2개 | 1453 MSGA · 1263 Epic's AIO | 결속 + WTT CommonLib 의존. **하나씩**, 레이드까지 돌아 본 뒤 다음 것 | 1.9GB |
| **U-F** ISB 셋 | 2426 MoreBotsAPI → 2478 ISB Aishi → 2485 ISB SF | ISB SF 가 MoreBotsAPI·ISB Aishi 를 의존한다. Aishi 는 **결속 4.2GB**. 2485 를 넣은 뒤 **자막 번역을 다시 올린다**(U7) | 4.9GB |
| **U-G** GF2 Skin Pack | 3156 (+1277 확인) | WTT Head&Voice 를 의존하는데 **설치본이 Forge 보다 높다**. GF2 가 1.0.8 을 요구하면 멈추고 판단한다 | 1.1GB |
| **U-H** Arena Map Port | 3166 | 5.1GB 하나. 맵 포팅본이라 **레이드를 들어가 봐야** 확인된다 | 5.1GB |

### 3-3. 멈추는 조건

- `check-install-records.mjs` 가 `OK — 0/0` 이 아니면 다음 묶음으로 가지 않는다.
- 서버 기동에서 `ModLoader: 총 N 개` 가 줄거나 `거부` 가 0 이 아니면 되돌린다.
- `Could not add` 가 **고유 id 48개**보다 늘면 되돌린다(줄 수가 아니라 고유 id 수로 센다 —
  오래 떠 있는 기동은 같은 48줄을 되풀이한다).
- 게임 `[Fatal` 이 0 이 아니거나 `N plugins to load` 가 줄면 되돌린다.
- 결속 모드를 넣고 **레이드를 한 번이라도 돈 뒤**에는 프로필 백업 없이 되돌릴 수 없다.

### 3-4. 이번에 하지 않는 것

- **Painter(1025)** — 페이지가 사라져 받을 수 없다. 그대로 둔다.
- **UnityToolkit(1426)** — 4.1.6 호환 버전이 없다. 지금 돌고 있으니 둔다.
- **Forge 밖 3개** — 배포 페이지를 손으로 봐야 한다. 이 계획에 넣지 않는다.
- **PIT 업스트림 1.1.1 머지** — STM 포크 쪽 일이고 모드 업데이트가 아니다.
- **2-2 의 설정 차이** — 업데이트 대상이 아닌 모드는 건드리지 않는다.

## 4. 진행 기록

### U-A — 끝났다 (2026-11-09)

작은 8개를 한 묶음으로 올렸다. 받은 양 **346KB**, 설치 파일 **18개**.

| id | 모드 | 올린 버전 | 설치 파일 |
| --- | --- | --- | --- |
| 2859 | Compatibility Highlighter | 1.3.0 → **1.3.2** | 1 |
| 2879 | WebOverlay | 1.11.0 → **1.12.0** | 7 |
| 2907 | Stash Filter | 1.0.0 → **1.1.0** | 1 |
| 2949 | Realistic Insurance | 0.2.1 → **0.2.3** | 3 (LICENSE·README 가 새 판에서 빠졌다) |
| 3074 | Direct Reload & Painless Meds | 1.3.2 → **1.4.1** | 3 |
| 3095 | Integrated Configuration Manager | 1.0.0 → **1.1.0** | 1 |
| 3190 | BiC - Bind in Container | 1.0.1 → **1.0.2** | 1 |
| 3194 | OpenBarters | 1.0.1 → **1.1.3** | 1 |

- **U3 에서 새 경로는 0개**였다. 사라진 기록은 2949 의 `LICENSE`·`README.md` 둘뿐이라, 설치 기록
  줄이 14001 → **13999** 로 줄었다. 모드 수는 304 로 그대로다.
- **U4** 업데이트 전 20개 파일을 `backup/before-update-2026-11-09/<id>/` 로 받아 뒀다.
  U-A 에 결속 모드가 없어 프로필 백업은 하지 않았다.
- **U5** 기록 파일 7개는 `install-records/removed/*-before-update-2026-11-09.txt` 로 옮겼고,
  `installed-batch3.txt` 는 **2859 한 줄만** 빼서 따로 남겼다(그 파일에는 다른 모드 6개의
  기록이 함께 들어 있다 — 통째로 옮기면 멀쩡한 기록을 잃는다).
- **U7 — 2949 의 보험 설정 6개를 돌려놨다.** 새 판이 `config/config.json` 을 함께 싣는데 그
  값이 **0.2.1 의 기본값과 같다**(pmc 55 등). 지금 쓰던 값은 기본값이 아니었으므로(pmc 65 ·
  playerScav 55 · scav 80 · boss 55 · 약탈자 탈출 25 · 못 나갔을 때 +50) 덮어쓰면 **설정이
  기본으로 돌아갔을 것**이다. 돌려놓고, 새로 생긴 `timeRemaining` 은 기본값으로 뒀다.
- **U8 실측** — 기록 검사 `OK — 0/0`. 서버 기동에서 `불러옴` **174줄 / 거부 0**(업데이트 전과
  같다), `[Error]` **49줄 = `Could not add` 48(고유 id 48) + bundle 1** 로 기준선과 같고,
  `cannot import quest` 1 도 같다. 두 모드가 새 버전으로 올라온 것을 로그가 직접 찍었다 —
  `Realistic Insurance 버전 0.2.3 ... 불러옴`, `Direct Reload & Painless Meds 버전 1.4.1 ... 불러옴`.
  **돌려놓은 설정도 로그가 확인해 준다** — `[RealisticInsurance] loaded - pmc 65% / playerScav 55%
  / scav 80% / boss 55% / other 90%, looter extracts 25% (+50% when they don't)`.
- `check-installed-vs-extracted.mjs` 를 다시 돌리면 **설정 아닌 차이는 여전히 0개**이고, 원본과
  다른 모드는 13개로 같다(2949 는 §2-2 에서 **§2-1 로 옮겨 적었다** — 이제 우리가 일부러 넣은
  값이다).
- **아직 안 본 것: 게임 쪽.** 여덟 중 여섯이 클라이언트 플러그인인데 게임을 띄우지 않았다.
  다음 기동에서 `N plugins to load` 가 **245 로 유지**되고 `[Fatal` 이 0 인지 봐야 한다.
  `Armorer ... offers insurance but is not listed` 경고는 **업데이트 전부터 있던 것**이다(같은
  날 앞 기동들에도 찍혀 있다).

### U-B — 끝났다 (2026-11-09)

Quest Codex 를 먼저, Briefing 을 뒤에 올렸다(의존 방향). 받은 양 **3.3MB**, 설치 파일 **108개**.

| id | 모드 | 올린 버전 | 설치 파일 |
| --- | --- | --- | --- |
| 3054 | Quest Codex | 1.11.1 → **1.12.0** | 95 → **96** |
| 3158 | Briefing | 1.1.0 → **1.2.0** | 12 (그대로) |

- **Quest Codex 1.12.0 이 게임 쪽 플러그인을 새로 들고 왔다** —
  `BepInEx\plugins\QuestCodex.Overlay\QuestCodex.Overlay.dll`. 그래서 이 모드의 위치가
  `서버` 에서 **`게임+서버`** 로 바뀌었고, **다음 게임 기동에서 플러그인 수가 245 → 246 으로
  늘어야 맞다.** 줄면 되돌리는 것이 기준이지만 **이번에는 느는 것이 정상**이다.
- 웹 자산 이름이 해시째 바뀌었다(`main-BptEaV9o.css`·`main-DkP2dusw.js` →
  `main-CxYgGp9M.css`·`main-BQtV9iGb.js`). 묶음 빌드라 당연한 변화다.
- **기록에 없는 캐시를 치웠다** — `QuestCodex\cache\loot-data.json`·`warm-langs.json` 은 모드가
  만든 것이라 기록에 없고 새 압축에도 없어 지워지지 않는다. 판이 바뀌면 묵은 캐시가 맞지
  않을 수 있어 `backup/before-update-2026-11-09-ub/3054-cache/` 로 옮겼다. 새 판이 **7.9초에
  다시 만들었다.**
- **1.12.0 은 기동할 때 카탈로그를 더 만들지 않는다.** 1.11.1 은 시작 직후
  `catalog warm-up started` 를 찍었는데 이번 기동에는 그 줄이 없다. 없어진 것이 아니라
  **첫 요청에 만든다** — `/questcodex/api/catalog` 를 불러 **200 / 10,097,351B / 15.9초**,
  `/questcodex/api/profiles` 는 **200 / 1,610B / 0.02초**로 답했고 그 뒤 캐시가 생겼다.
  카탈로그 경고도 `vanillaSnapshotMismatch=1 · questZoneNotFound=76 · missingLocale=2 ·
  danglingPrereq=1 · exitPositionMissing=4` 로 **업데이트 전과 같은 숫자**다.
- **U8 실측** — 기록 검사 `OK — 0/0`(줄 13999 → 14000, 모드 304 그대로). 서버 기동
  `불러옴` **174줄 / 거부 0**, `[Error]` **49줄 = `Could not add` 48(고유 id 48) + bundle 1**,
  `cannot import quest` 1 로 기준선과 같다. 로그가 `모드 QuestCodex 버전 1.12.0 ... 불러옴` 과
  `Mod QuestCodex has a wwwroot, mapping to /questcodex/` 를 찍었다.
- Briefing 은 게임 전용이라 서버 로그에 줄이 없다 — **게임 기동에서 봐야 한다**(`Briefing.Core.dll`
  은 플러그인 속성이 없는 라이브러리라 0 으로 세는 것이 정상이다).

### U-C — 끝났다 (2026-11-09)

**결속 둘이라 프로필을 먼저 받아 두고**(`backup/profiles-before-update-2026-11-09/`, 11개 9.2MB)
**하나씩** 넣고 그때마다 테스트 서버를 돌렸다. 받은 양 **224KB**.

| id | 모드 | 올린 버전 | 설치 파일 | 기동 |
| --- | --- | --- | --- | --- |
| 1305 | Repair Max Durability | 2.2.0 → **2.2.1** | 3 (그대로) | `SERVER STARTED in 174s` |
| 2844 | Ultimate Flea | 1.2.0 → **1.3.1** | 11 → **14** | `SERVER STARTED in 171s` |

- **압축 뿌리에 섞여 온 `README.txt` 를 치웠다.** Ultimate Flea 1.3.1 의 압축에는 설치 경로가
  아닌 `README.txt` 가 뿌리에 있어, 그대로 넣으면 **`E:\Spt\README.txt`** 가 생긴다. 설치 전에
  `backup/before-update-2026-11-09-uc/2844-stray/` 로 옮겼다(설치본에 그 이름은 없었다).
- **Ultimate Flea 가 웹 화면을 들고 왔다** — `wwwroot/` 4개가 새로 들어오고 로그가
  `Mod UltimateFlea has a wwwroot, mapping to /ultimateflea/` 를 찍었다. 서버 포트(6969) 안이다.
- **`data\economy_state.json` 은 되돌리지 않았다.** 모드가 쓰는 상태 파일이고 새 압축에는 없어
  지워졌는데, 1.3.1 이 기동에서 **스스로 다시 만들었다**(1,221,770B → 890,571B). 판이 바뀌며
  모양이 달라진 것으로 보여 옛 것을 덮지 않았다 —
  `backup/before-update-2026-11-09-uc/2844/.../economy_state.json` 에 남겨 뒀다.
- 1305 도 `config/config.json` 을 함께 싣는데, 업데이트 전 설정이 **원본과 같았으므로**(md5 대조)
  돌려놓을 것이 없었다.
- **프로필을 백업과 맞댔다.** 11개 전부 **바이트 수가 같고, 다른 곳은
  `spt.migrations.TraderGenPocketMigration` 시각 한 곳뿐**이다(보기: 1794207819 → 1794208505).
  서버가 시작할 때마다 다시 쓰는 값이라 **결속 모드 둘이 프로필을 건드리지 않았다**는 뜻이다.
- **U8 실측** — 기록 검사 `OK — 0/0`(줄 14000 → 14003, 모드 304 그대로). 두 기동 모두
  `불러옴` **174줄 / 거부 0**, `Could not add` 48(고유 id 48) · bundle 1 · quest 1 로 기준선과
  같다. 로그가 `RepairMaxDurability 버전 2.2.1 ... 불러옴` · `[RepairMaxDurability] Loaded
  successfully` · `UltimateFlea 버전 1.3.1 ... 불러옴` · `[UltimateFlea] Trade hooks enabled.` 를
  찍었다.
- 설치 스크립트가 이번에는 **다른 모드가 만든 파일을 기록하지 않았다** — 출력에
  `OTHERMOD ... (다른 모드가 만든 파일 — 기록하지 않음)` 가 19줄 찍혔다(ORBIT·ETT·SIM·Raid
  Review 것). [기록 오기](install-records/README-removed-stale-lines-2026-11-06.md) 가 더 생기지
  않는다.

### U-D — 절반 (2026-11-09)

둘 다 받아 뒀고 **MSGA 만 넣었다.** 계획이 "하나씩, 레이드까지 돌아 본 뒤 다음 것" 으로
못 박아 둔 자리라, **레이드 확인은 사람 몫**이므로 Epic's AIO 는 넣지 않고 멈췄다.

| id | 모드 | 상태 | 설치 파일 | 기동 |
| --- | --- | --- | --- | --- |
| 1453 | Make Shotguns Great Again! | 1.19.1 → **1.20.0** 넣었다 | 195 → **223** | `SERVER STARTED in 177s` |
| 1263 | Epic's All in One | 5.0.2 → 5.0.3 **받아만 뒀다**(1.5GB) | 632 (경로 변화 0) | — |

- **받는 길이 막혀 `gh` 로 돌아갔다.** `sp-mod.com/mod/download/1453/.../1.20.0` 은
  `github.com/viniHNS/Make-Shotguns-Great-Again/releases/download/v1.20.0/...` 로 307 을
  돌려주는데, 그 **github.com 주소가 504 Gateway Time-out** 이다 — `download-stage.mjs` 가
  한 번, `redownload-mod.mjs` 가 네 번, 손으로 한 번, 모두 504 였다. 같은 묶음의 Epic's AIO 는
  `release-assets.githubusercontent.com` 로 가서 **40초에 1.5GB** 를 받았다. 그래서
  `gh release download v1.20.0 --repo viniHNS/Make-Shotguns-Great-Again` 으로 받았고
  **바이트 수가 매니페스트와 정확히 같다**(436,552,891B). 받은 뒤 이름을 `1453__` 꼴로 맞췄다.
- MSGA 1.20.0 은 파일이 **28개 늘었다** — ATI Omni 410 과 Iver Johnson Champion 쪽 번들·텍스처다.
  콘텐츠가 늘어난 것이고 경로가 사라진 것은 없다.
- Epic's AIO 5.0.3 은 **경로 집합이 5.0.2 와 똑같다**(632개, 새 경로 0 · 사라진 경로 0).
- **U8 실측** — 기록 검사 `OK — 0/0`(모드 304 그대로). `불러옴` **174줄 / 거부 0**,
  `Could not add` 48(고유 id 48) · `Unable to add bundle` **1**(알려진 `barrel_vpo215` 건뿐,
  번들 28개가 늘었는데도 안 늘었다) · quest 1. 로그가
  `모드 Make Shotguns Great Again 버전 1.20.0 ... 불러옴` 을 찍었다.
- 프로필 11개를 `backup/profiles-before-update-2026-11-09-ud/` 와 맞대어 **마이그레이션 시각
  한 곳만** 다른 것을 확인했다.
- **남은 것**: 레이드로 MSGA 를 확인하고 나서 Epic's AIO 를 넣는다. 압축은
  `downloads/update-2026-11-09-ud/`, 해제본은 `extracted/update-2026-11-09-ud/` 에 그대로 있다.

## 5. 다시 재는 법

```
node tools/check-mod-updates.mjs             # 업데이트 있는 모드 (실패하면 종료 코드 1)
node tools/check-installed-vs-extracted.mjs  # 설치 후 달라진 파일
```

앞의 것은 Forge API 를 모드마다 한 번 부르므로 약 5분 걸린다. **종료 코드가 1 이면 조회 실패가
있다는 뜻이고, 그때의 "업데이트 없음" 은 믿을 수 없다.**
