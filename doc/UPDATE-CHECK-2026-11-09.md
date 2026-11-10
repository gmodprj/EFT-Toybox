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
**업데이트 20개를 모두 올렸다 — U-A부터 U-H 까지 끝났다(U-A~U-E·U-G·U-H 는 2026-11-09, U-F 는 2026-11-10). 결과는 §4.**

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

### U-D — 끝났다 (2026-11-09, U-E 보다 뒤에 했다)

ECOTI 하나. 결속이라 프로필을 먼저 받아 뒀다(`backup/profiles-before-update-2930/`).
받은 양 **31.5MB**.

| id | 모드 | 올린 버전 | 설치 파일 | 기동 |
| --- | --- | --- | --- | --- |
| 2930 | ECOTI | 3.4.2 → **3.5.0** | 71 → **56** | `SERVER STARTED in 186s` |

- **기록이 71 → 56 으로 줄었지만 잃은 것이 없다.** 새 압축에 없는 15개는 전부 모드가 만드는
  것이다 — `nvghostcompat\*.json` 5개(WTT Artem 야시경 호환)와 `wwwroot\meshes-auto\*.glb`
  10개. 기동에서 **31개로 다시 만들었다**(전보다 많다. 3.5.0 이 더 많은 호환을 뽑는다).
- **아이콘 캐시 323개는 건드리지 않았다.** `icons\*.png` 는 기록에도 새 압축에도 없어 지워지지
  않고 남는데, 새 압축의 56개 경로와 **겹치는 파일이 0개**임을 먼저 확인하고 설치했다. 아이템
  id 로 이름이 붙는 캐시라 판이 바뀌어도 맞는다(QuestCodex 의 loot 캐시와 달리 치우지 않았다).
- 설치 뒤 그 폴더의 파일은 **387개**다(설치 56 + 아이콘 323 + 다시 만든 31 가운데 기록 밖의 것).
- **U8 실측** — 기록 검사 `OK — 0/0`(모드 304 그대로). `불러옴` **174줄 / 거부 0**,
  `Could not add` 48(고유 id 48) · bundle 1 · quest 1 로 기준선과 같다. 로그가
  `모드 ECOTI 버전 3.5.0 ... 불러옴` · `Mod ECOTI has a wwwroot, mapping to /coti-assets/` ·
  `[COTI] Thermal riser created ...` 를 찍었다.
- 프로필 11개는 **마이그레이션 시각 한 곳만** 다르다.
- **게임에서 볼 것**: 기준선의 `[COTI] Per-frame state update failed:
  ArgumentOutOfRangeException` 가 3.5.0 에서도 나는지. 이 오류는 **레이드를 돌아야** 나온다.

### U-E — 끝났다 (2026-11-09)

> **묶음 이름을 바로잡았다.** 이 작업을 처음에 "U-D" 로 적었는데, §3-2 의 U-D 는
> **2930 ECOTI** 다. MSGA·Epic's AIO 는 **U-E** 다. **U-D(ECOTI, 32MB)는 아직 안 했다.**

계획대로 **하나씩** 넣었다. MSGA 를 넣고 **레이드까지 돌아 본 뒤**(게임 쪽 확인 절) Epic's AIO 를
넣었다. 받은 양 **1.9GB**.

| id | 모드 | 올린 버전 | 설치 파일 | 기동 |
| --- | --- | --- | --- | --- |
| 1453 | Make Shotguns Great Again! | 1.19.1 → **1.20.0** | 195 → **223** | `SERVER STARTED in 177s` |
| 1263 | Epic's All in One | 5.0.2 → **5.0.3** | 632 (경로 변화 0) | `SERVER STARTED in 186s` |

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
  지우고 나니 **빈 폴더 62개가 남았다** — 설치 스크립트가 압축의 빈 폴더까지 만들고
  `*.dirs.txt` 에 따로 적기 때문이다. 파일이 아니라 덮어쓰기 검사에 걸리지 않는다.
- **게임·서버를 끄고 넣었다**(사용자 허락). 게임은 `CloseMainWindow` → 확인 → 서버는
  `tools/stop-spt-server.ps1`. 그 스크립트는 **게임이 돌고 있으면 아무것도 하지 않고 멈춘다**
  (레이드 중 서버를 끊으면 그 판이 날아간다), 그래서 게임부터 껐다.
- **레이드를 돈 뒤라 프로필을 새로 받아 뒀다** — `backup/profiles-before-update-1263/`.
  앞의 U-E 백업과 견주니 **3개가 달라져 있었다**(한 프로필은 92,672B → 543,064B). 레이드
  결과가 들어온 것이라, Epic's AIO 를 넣기 전 스냅샷은 **이 새 것**이어야 한다.
- **U8 실측** — 기록 검사 `OK — 0/0`(모드 304 그대로). 두 기동 모두 `불러옴` **174줄 / 거부 0**,
  `Could not add` 48(고유 id 48) · `Unable to add bundle` **1**(알려진 `barrel_vpo215` 건뿐,
  MSGA 번들이 28개 늘었는데도 안 늘었다) · quest 1. 로그가
  `모드 Make Shotguns Great Again 버전 1.20.0 ... 불러옴` 과
  `모드 Epic's All In One 버전 5.0.3 ... 불러옴`(함께 오는 `Vanguard 2.0.0` 도)을 찍었다.
- 프로필을 각 단계의 백업과 맞대어 **마이그레이션 시각 한 곳만** 다른 것을 확인했다 —
  MSGA 는 `profiles-before-update-2026-11-09-ud/`, Epic's AIO 는 `profiles-before-update-1263/`.
- **남은 것**: Epic's AIO 는 게임에서 아직 못 봤다(다음 기동에서 플러그인 246 유지·`[Fatal` 0).

### 게임 쪽 확인 — U-A·U-B·U-C 와 MSGA 한꺼번에 (2026-11-09 17:17 기동)

`E:\Spt\BepInEx\LogOutput.log`(17:50 기준, 417,995B)을 쟀다. **레이드를 돌고 나온 기동**이다 —
`Game started` · `LocationScene` · `Raid end` 가 찍혀 있다.

| 재는 것 | 값 | 판정 |
| --- | --- | --- |
| `N plugins to load` | **246** | **예상대로** (245 + `QuestCodex.Overlay`) |
| `Loading [` 줄 | **246** | 수가 맞는다 |
| `Chainloader startup complete` | 1 | 끝까지 돌았다 |
| `[Fatal` | **0** | — |
| `[Error` | **2** | 둘 다 **알려진 것** |

`[Error` 두 줄은 ① `[DirectReload] Patch FAILED to enable: FikaHostReloadMirror`(Fika 미설치라
걸 대상이 없다 — 1.4.1 에서도 같다), ② `[COTI] Per-frame state update failed:
ArgumentOutOfRangeException`(기준선에 있던 ECOTI 건)이다. **새 오류는 없다.**

올라온 것을 이름으로 확인했다 — `Loading [` 에 `QuestCodex`, `Briefing`,
`CompatibilityHighlighter`, `StashFilter`, `Configuration Manager`, `BiC`, `WebOverlay`,
`Direct Reload`, `RepairMaxDurability`, `UltimateFlea` 가 있고, 이름이 다른 둘도 찾았다 —
**`Loading [Realistic Insurance (client) 0.2.3]`**, **`Loading [makeshotgunsgreatagain 1.20.0]`**.
새 모드 관련 `[Warning` 0줄, 번들·리소스를 못 찾은 줄 0줄이다.

> **오류가 2줄인 것을 좋아진 것으로 읽지 않는다.** 기준선의 깊은 레이드 기동은 9줄이었고
> 그중 SAIN 6 · STM 2 는 특정 상황에서만 나온다. 이번에 안 나온 것은 **아직 안 나온 것**이다.

이것으로 **U-A·U-B·U-C 는 서버·게임 양쪽에서 닫혔고**, U-D 의 MSGA 도 레이드까지 지났다.

### U-F — 끝났다 (2026-11-10)

사용자 결정대로 느린 전송을 맨 뒤로 보냈고, **다음 날 다시 돌리니 호스트가 30배 빨랐다.**

| id | 모드 | 올린 버전 | 설치 파일 | 기동 |
| --- | --- | --- | --- | --- |
| 2426 | MoreBotsAPI | 2.1.1 → **2.1.2** | 4 | `SERVER STARTED in 186s`(11-09) |
| 2478 | ISB Aishi | 2.0.4 → **2.0.7** | 3659 → **3880** | 불러옴 174 / 거부 0 |
| 2485 | ISB Special Forces | 2.0.1 → **2.0.4** | 1584 → **2508**(+빈 폴더 기록 665) | 불러옴 174 / 거부 0 |

- **이어받기가 값을 했다.** 어제 7.1%(조각 37/522, 296MB)에서 멈춰 둔 것을
  `download-ranged.mjs` 로 다시 돌리니 **받아 둔 37조각을 건너뛰고** 이어받았다. 속도는
  조각 38 → 116 사이 실측 **약 8.3MB/s**(어제 260KB/s). 받은 크기는 매니페스트와 바이트까지 같다.
  화면의 누적 속도(`89,069KB/s`)는 먼저 받아 둔 조각까지 함께 나눈 값이라 처음 몇 줄은 부풀어 있다.
- **bsdtar 가 `Archive entry has empty or unreadable filename … skipping` 을 내고 오류로 끝났지만
  잃은 것은 없다.** 목록을 세어 확인했다 — 압축 안의 파일 **3,880개 = 해제된 파일 3,880개**
  (나머지 422개는 디렉터리 항목이고, 건너뛴 것은 이름이 빈 항목 하나다). **오류 문구만으로
  판정하지 않는다.**
- **2485 에서 덮어쓰기 검사가 두 개를 잡았다**(그게 안전장치다).
  ① `BepInEx\config\com.samc137.isbsofextras.cfg` — **설치본 설정에 기본값이 아닌 값이 6개**
  있었고 그중 하나가 **`Subtitle Language = kr`**, 우리 한글 자막을 띄우는 설정이다. 새 판의
  기본값은 `Auto` 라 그대로 덮었으면 자막이 영문으로 돌아갔다. 받아 두고, 새 파일에 여섯 값을
  다시 입혔다(`Containment Breach Event Chance` 55 · `Dead Man Switch Alarm Volume` 51 ·
  `ISB Radio Volume` 100 · `Laboratory Blackout Event Chance` 15 · `Restrict Laboratory Exits`
  false · `Subtitle Language` kr). 새로 생긴 설정은 새 기본값으로 뒀다.
  ② `SPT_Runtime\user\patchers\…\ISBEnums.json` — 모드가 만드는 열거값 등록부라 새 판 것으로 바꿨다.
  둘 다 `backup/before-update-uf/2485-extras/` 에 있다.
- **자막 번역을 다시 올렸다** — `tools/install-plugin-locales.ps1` 로 네 사전을 재설치하고
  `node tools/check-plugin-locales.mjs` 로 쟀다: ISB 자막 **222/222**, 빠진 열쇠 0, 설치본과 같음.
  **2.0.4 에서도 자막 열쇠가 222개 그대로**라 번역이 비지 않았다.
- **프로필 11개 중 하나가 마이그레이션 시각 밖으로도 달라져 재어 봤다 — 손실이 아니다.**
  `6adc2669…` 의 우편 세 통(532·536·537)에서 빈 첨부의 `items.stash` 열쇠가 사라졌다.
  **첨부 아이템 수는 전·후 모두 0**, 우편 수는 653 그대로, 글과 시각도 같다. 이미 받아 간
  빈 첨부에서 서버가 쓸모없는 id 를 떼어 낸 것이다.
- 설치 중 `-First` 로 파이프를 끊어 **2478 은 설치만 되고 테스트가 잘렸다**(`Select-Object -First`
  는 위쪽 명령을 끝낸다). 서버를 따로 띄워 쟀다 — 불러옴 174 / 거부 0,
  `Could not add` 48(고유 48) · bundle 1 · quest 1. 2485 는 `-Last` 로 바꿔 테스트까지 돌렸다.

### U-F — 받는 중이던 기록 (2026-11-09)

ISB 셋. 의존 방향은 **MoreBotsAPI → ISB Aishi → ISB Special Forces** 다.

| id | 모드 | 상태 | 기동 |
| --- | --- | --- | --- |
| 2426 | MoreBotsAPI | 2.1.1 → **2.1.2** 넣었다 (파일 4개) | `SERVER STARTED in 186s` |
| 2478 | ISB Aishi | 2.0.4 → 2.0.7 **받는 중**(4.07GB) | — |
| 2485 | ISB Special Forces | 2.0.1 → 2.0.4 **받아 뒀다**(782MB) | — |

- **이 호스트는 한 줄기를 조여 놓는다.** ISB Aishi 는 Forge 에서
  `isb-downloads.samc137.workers.dev`(Cloudflare Worker, LAX 엣지)로 307 된다. `download-stage.mjs`
  가 한 줄기로 받으니 **33분에 8MB**(1.6KB/s)였다. 그런데 **`Accept-Ranges: bytes` 를 주고
  Range 요청에 206 으로 답한다** — 한 줄기로 끊어 받으면 56KB/s, 여덟 줄기면 273KB/s 였다.
- 그래서 [tools/download-ranged.mjs](../tools/download-ranged.mjs) 를 만들었다. 조각(기본 8MB)을
  줄기(기본 12) 나눠 `.part` 의 제자리에 쓰고 `.state.json` 에 적는다 — **다시 돌리면 안 받은
  조각만** 받는다. Range 를 안 받는 호스트면 조용히 한 줄기로 떨어지지 않고 **멈춘다**.
  16줄기로 돌려 **260KB/s, 남은 시간 약 4시간**이다. 오래 걸리므로 **터미널 탭**에서 돌린다.
- ISB SF 2.0.4 는 변화가 크다 — **새 경로 931개**(Chaos Insurgency 외형, Black Division 프리셋
  다수, 읽는 음성 번들), 새 판에 없는 기록 7개(그중 `data\quest_truce.json`·`revenge.json`·
  `spawn-progression.json` 은 모드가 쓰는 상태 파일이다).
- 셋 다 **해제본 원본이 없어 수정 여부를 못 쟀던 것들**이라, 넣기 전에 기록된 파일을
  `backup/before-update-uf/<id>/` 로 받아 둔다. 프로필도 받아 뒀다
  (`backup/profiles-before-update-uf/`, ISB Aishi 가 결속이다).
- **2485 를 넣은 뒤 자막 번역을 다시 올려야 한다** — `tools/install-plugin-locales.ps1`,
  확인은 `node tools/check-plugin-locales.mjs`.

### U-G — 끝났다 (2026-11-09)

| id | 모드 | 올린 버전 | 설치 파일 | 기동 |
| --- | --- | --- | --- | --- |
| 3156 | GF2 Skin Pack | 1.0.0 → **1.2.4** | 58 → **76** | `SERVER STARTED in 171s` |

- **의존은 막힌 것이 아니었다.** 계획이 "GF2 가 1.0.8 을 요구하면 멈추고 판단한다" 로 적어 둔
  자리인데, Forge 의존 해석(`/mods/dependencies?mods=3156:1.2.4`)에 **버전 제약이 없고**
  `conflict: false` 다. 설치본의 WTT Head&Voice **1.1.0 은 서버 기동에서 정상 적재**된다
  (`모드 WTT Head Voice Selector 버전 1.1.0 ... 불러옴`). 표기가 아니라 실측으로 판정했다.
- **압축에 폴더가 한 겹 더 있었다.** 다른 모드는 압축 뿌리가 바로 `SPT_Runtime\...` 인데 이
  압축은 `GF2_Skin_Pack_Full_SPT_4.1.6\SPT_Runtime\...` 이다. 그대로 풀면 설치 경로가 한 겹
  어긋나 **76개 전부 "설치 경로가 아닌 파일"** 로 잡힌다. `tar.exe ... --strip-components=1` 로
  다시 풀었다.
- **서버는 버전을 1.0.0 으로 찍는다 — 틀린 설치가 아니다.** `GF2.SkinPack.dll` 이 **옛 것과
  md5 가 같다**(9253914f…, 8,704B). 제작자가 어셈블리 버전을 올리지 않았고 이 판은 **내용만
  바뀐 릴리스**다. 바뀐 것은 `bundles.json` 이 **52 → 72 항목**이고 의상·머리 번들이 20개
  늘었다(igla_default·klukai_cerulean·mityl·Nikketa 등 12개를 폴더에서 확인했다).
  판정은 로그의 버전 표기가 아니라 **파일로** 한다.
- **9차 기록에 섞여 있던 임시 파일 2개가 사라졌다** — `.soppo_upper.bundle.h6E1gv`(32.8MB)와
  `.makitto_head.bundle.alMG0a`(4.5MB). 압축을 풀다 끊긴 조각이 그대로 기록에 들어가 있던
  것이고, 새 기록에는 없다(지금 그 폴더에 숨은 임시 파일 0개).
- **U8 실측** — 기록 검사 `OK — 0/0`(모드 304 그대로). `불러옴` **174줄 / 거부 0**,
  `Could not add` 48(고유 id 48) · bundle 1 · quest 1 로 기준선과 같다. 설치 파일 76개 2.7GB.

### U-H — 끝났다 (2026-11-09)

| id | 모드 | 올린 버전 | 설치 파일 | 기동 |
| --- | --- | --- | --- | --- |
| 3166 | Arena Map Port | 0.4.0 → **0.5.0** | 260 → **363** (5.0GB) | `SERVER STARTED in 174s` |

- **받는 곳이 빨랐다** — `pixeldrain.com`, 한 줄기로 **23.3MB/s**(12초에 279MB). 끊어 받을
  필요가 없어 `download-stage.mjs` 로 **146초에 5.0GB** 를 받았다. Range 로 여덟 줄기를 열면
  오히려 16MB/s 로 느렸다. **호스트마다 재 보고 길을 고른다.**
- 새 경로 **104개**는 아레나 맵 하나가 더 들어온 것이다(`arena_iceberg` — 번들·음향·AI·
  DynamicMaps 레이어). 새 판에 없는 기록은 관중 음향 `.wav` 하나뿐이다.
- **이 모드는 지금 아무 맵도 등록하지 못한다 — 이번 업데이트 탓이 아니다.** 기동 로그가
  `[ArenaMapPort] Escape from Tarkov: Arena not found ... The 5 Arena maps stay unregistered.`
  를 찍는데, **같은 줄이 0.4.0 을 넣던 10-M 테스트 출력에도 있다**(지금까지 테스트 출력 12개에
  찍혀 있다). EFT: Arena 설치본이 없어서다. 쓰려면 BSG 런처로 Arena 를 깔거나
  `SPT_Runtime\user\mods\ArenaMapPort\config\arena-install.json` 에 `installPath` 를 적어야 한다.
  **5.0GB 를 차지하면서 지금은 하는 일이 없다** — 지울지는 사용자 몫이다.
- **U8 실측** — 기록 검사 `OK — 0/0`(모드 304 그대로). `불러옴` **174줄 / 거부 0**,
  `Could not add` 48(고유 id 48) · bundle 1 · quest 1 로 기준선과 같다.

#### Arena 관문을 재 봤다 — `installPath` 는 먹지만 폴더가 진짜여야 한다

SPT 설치본 안에 빈 폴더 `E:\Spt\ArenaStub` 를 만들고
`SPT_Runtime\user\mods\ArenaMapPort\config\arena-install.json` 에
`{"installPath": "E:/Spt/ArenaStub"}` 를 적어 기동했다(이 설정 파일은 **원래 없어서 새로
만들었다**).

| 재는 것 | 전(레지스트리) | 후(빈 폴더 지정) |
| --- | --- | --- |
| 로그 | `...not registered in the Windows registry (HKEY_LOCAL_MACHINE\...)` | **`E:\Spt\ArenaStub does not contain EscapeFromTarkovArena.exe`** |
| `/arenamapport/status` 의 `gate.source` | — | **`override`**, `overrideFile: "used"` |
| `gate.state` / `cause` | — | `not_verified` / **`not_arena_folder`** |
| 맵 5개 | `not_registered` | `not_registered` (cause `gate`) |

- **설정은 제대로 먹는다.** 메시지가 "레지스트리에 없다" 에서 "그 폴더에 exe 가 없다" 로
  바뀌었고 상태 라우트가 `source: override` · `overrideFile: used` 로 답한다. 즉 **진짜 Arena
  설치 폴더를 가리키면 레지스트리 없이도 통과하도록 만들어진 길이 맞다.**
- **빈 폴더로는 통과하지 않는다.** 서버 DLL 문자열로 관문이 보는 것을 읽었다 —
  ① 폴더가 있어야 하고, ② `EscapeFromTarkovArena.exe` 가 있어야 하고,
  ③ 런처 매니페스트 **`ConsistencyInfo`** 가 있고 유효해야 하며(`Entries` 키,
  `EscapeFromTarkovArena_Data\level` 로 시작하는 항목이 `minLevelEntries` 개 이상),
  ④ `Uninstall.exe` 는 없어도 된다(`tolerated`).
- `/arenamapport/status` 는 **zlib 로 눌려 온다** — `curl` 로 받아 `zlib.inflateSync` 해야 읽힌다
  (`--compressed` 로는 안 풀린다).
- **그 셋을 흉내 내는 짓은 하지 않았다.** exe 와 런처 매니페스트를 지어내는 것은 별개 유료
  제품(EFT: Arena)의 **설치·소유 확인을 속이는 것**이다. Arena 를 가지고 있다면 이 설정에
  **그 설치 폴더를 적으면** 된다 — 길이 열려 있는 것은 위에서 확인했다.
- **되돌렸다**(사용자가 Arena 를 가지고 있지 않다). `config\arena-install.json` 과 그 `config\`
  폴더, `E:\Spt\ArenaStub` 를 지우고 다시 기동해 **원래 상태로 돌아온 것을 쟀다** —
  로그가 다시 레지스트리 문구이고, 상태 라우트가 `source: "none"` · `overrideFile: "absent"` ·
  `cause: "not_registered"` 다. 기동은 `불러옴` 174줄 / 거부 0, 오류 48(고유 48)/1/1 로 같다.
  모드 폴더에 남은 것은 설치 기록에 적힌 파일뿐이다(`banners`·`db`·`locales`·DLL·고지문).

### 게임 쪽 확인 — 업데이트 20개를 다 올린 뒤 (2026-11-10 기동)

| 재는 것 | 값 | 판정 |
| --- | --- | --- |
| `N plugins to load` | **246** | 11-09 기동과 같다(ISB 둘은 플러그인 수를 늘리지 않았다) |
| `Loading [` 줄 | **246** | 수가 맞는다 |
| `Chainloader startup complete` | 1 | 끝까지 돌았다 |
| `[Fatal` | **0** | — |
| `[Error` | **1** | 알려진 DirectReload+Fika 건뿐 |

새 판으로 올라온 것을 이름·버전으로 확인했다 — `AishiMasterKeyFunction 2.0.7`,
`ISB SOF Notifier 2.0.4`, `ISB SOF: Immersion Systems Plugin 2.0.4`, `ISB Special Forces 2.0.4`,
`ECOTI 3.5.0`, `Realistic Insurance (client) 0.2.3`, `makeshotgunsgreatagain 1.20.0`,
`Briefing 1.2.0`, `QuestCodex Overlay 1.12.0`, `UltimateFlea Client 1.3.1`,
`RepairMaxDurabilityClient 2.2.1`.

- **자막은 이 기동만으로 판정되지 않는다.** ISB 자막은 실험실 방송·무전이 나올 때 쓰이므로
  메뉴에서는 로그에 아무 줄도 남지 않는다 — **레이드에서 봐야 한다.**
- **아이템 이름·등급 색도 로그에 안 나온다.** 서버가 내려주는 값으로는 이미 확인했다(§4-1).
  화면에서 한 번 보는 것이 남았다.
- locale 항목이 **61,221 → 61,391**(+170)로 또 늘었다. ISB 둘의 새 콘텐츠가 들어온 몫이고,
  §4-1 과 같은 성질(아이템 이름)이다. 우리 사전은 **8,086/8,089** 그대로 입혀진다.

#### 레이드까지 돈 뒤 (아이스브레이커)

사용자가 레이드를 돌고 나왔다(`LocationScene` 1 · `GameStarted` 17 · 맵은 아이스브레이커).

| 재는 것 | 값 |
| --- | --- |
| `[Fatal` | **0** |
| `[Error` | **4** — DirectReload(Fika, 알려진 것) · ECOTI per-frame(기준선) · **SPTBattleAmbience 1** · **Manimal-Icebreaker 1** |

뒤의 둘은 **이번 업데이트와 무관하고 아이스브레이커 맵에서만 나는 것**이다.

- `[SPTBattleAmbience] Couldnt find map config for location: icebreaker` — 그 모드에 포팅된
  아이스브레이커용 설정이 없다. 두 모드 다 이번에 올린 20개에 들어 있지 않다.
- `[Manimal-Icebreaker] [Firewall] SkillsExtended.Client.Skills's hook OnGameStartedPatch.Postfix
  threw on the icebreaker — swallowed; only THAT mod's hook is skipped, later mods' hooks still r…`
  — 아이스브레이커 모드가 **제 방화벽으로 Skills Extended 의 훅 하나만 건너뛰고 삼켰다**.
  레이드는 그대로 진행됐다.
- **처음 보는 줄이지만 "전에도 있었다" 고 적지 않는다.** 11-09 기동은 다른 맵이라 이 줄이 날
  자리가 없었고, `BepInEx\LogOutput.log` 는 기동마다 덮여 옛 기록이 없다. `E:\Spt\Logs\` 의
  EFT 자체 로그에는 모드 줄이 남지 않아 대조할 것이 없다. **아이스브레이커 특정 + 삼켜진 예외**
  라는 것까지가 지금 잴 수 있는 전부다.
- **자막은 로그로 잴 수 없다 — 화면으로 확인했다.** `subtitle`·`C.A.S.S.I.E`·`announcement` 로
  걸리는 줄이 **끝까지 0** 이다. 그 플러그인은 **자막을 로그에 적지 않는다**. 2026-11-10 에
  사용자가 레이드에서 **한글로 떴다**고 확인했다 — 설정 `Subtitle Language = kr`,
  사전 222/222, 설치본 동일(§U-F)까지 맞춰 둔 것이 화면에서 먹었다.
  - 로그에 남는 것은 주변 사실뿐이다 — `ISB SOF: Immersion Systems Plugin v2.0.4 loaded.`,
    `ISB Labs menu state: enabled=True, unlocked=False, …`, ISB 봇 사살
    (`ISBSpecialForces kill: Zarnica/Fallen/Faust`). **실험실은 아직 안 열렸는데도**(unlocked=False)
    자막이 떴으므로, 자막은 미니랩 전용이 아니라 **레이드 무전에서도** 나온다.
  - 다음에 이 항목을 확인할 때 **로그를 뒤지는 데 시간을 쓰지 않는다.** 화면을 보는 것이
    유일한 길이다.

### 받는 순서 규칙 (2026-11-09, 사용자 결정)

**느린 다운로드는 마지막까지 미룬다.** 호스트가 조여 놓은 것은 다른 일을 다 끝낸 뒤에 받는다.

- ISB Aishi(4.07GB)는 **7.1%(조각 37/522, 296MB)에서 멈춰 뒀다.** 조각 상태가
  `downloads/update-2026-11-09-uf/2478__ISB-Aishi-v2.0.7.7z.state.json` 에 남아 있어,
  `node tools/download-ranged.mjs update-2026-11-09-uf 2478 16 8` 을 다시 돌리면 **받은 37개는
  건너뛰고** 이어 받는다.
- 그 호스트는 **일정하지도 않다.** 처음 24조각은 **900~1,100KB/s**(남은 시간 60~80분)였는데,
  25번째 조각 앞에서 **9분쯤 멈췄다가** 그 뒤로 누적 260KB/s 로 내려앉았다. 한 번 재고
  "이 속도다" 라고 적으면 틀린다.

## 4-1. 업데이트가 한글화에 낸 구멍 — 쟀다 (2026-11-09)

업데이트로 새 아이템이 들어오면 **게임 locale 항목이 늘고, 늘어난 것은 영문이다.** 18개를
올린 뒤 재 보니 `kr` locale 이 **61,125 → 61,221 항목**(+96)이 됐다.

| 재는 것 | 값 | 판정 |
| --- | --- | --- |
| 우리 사전 적용 | **8061/8061**, 값이 다름 0 · 열쇠 없음 0 | 사전은 멀쩡하다 |
| 퀘스트 이름 미번역 | **3** | 사실상 0 — 아래 참고 |
| 퀘스트 설명 미번역 | **0** | — |
| 퀘스트 목표·대사·메일 미번역 | **0** | — |
| 아이템 `Name` 미번역 | 8,656 (모드 5,964 / 바닐라 2,692) | **원래 범위 밖** |
| 아이템 `ShortName` 미번역 | 9,224 (모드 5,520 / 바닐라 3,704) | 같음 |
| 아이템 `Description` 미번역 | 1,288 (모드 764 / 바닐라 524) | 같음, 게다가 **화면에 안 보인다** |

- **사전이 덮는 묶음에는 구멍이 생기지 않았다.** 퀘스트 이름으로 잡힌 3개는 퀘스트가 아니다 —
  `PMC`·`U-SUCC`(세력 표시, 바닐라)와 `tgc_black_combatshirt_hands`(에셋 키)다. 목표·설명·
  대사·메일은 **전부 0** 이다.
- **+96 은 아이템 이름·약칭 쪽이다.** 오늘 들어온 것으로 확인했다 — `ATI Omni Hybrid …` 7개,
  `Iver Johnson Champion …` 5개(MSGA 1.20.0), ECOTI 3.5.0 의 `TR-1`·`TSR-1` 8개. 전부
  `<b><color=#FFFFFF>…</color></b>` 로 싸인 영문 이름이다.
- **아이템 이름은 원래 마지막 순서다.** [REVIEW-KOREAN-LOCALIZATION.md](REVIEW-KOREAN-LOCALIZATION.md)
  가 권한 순서가 퀘스트 → 봇 역할 → 클라이언트 locale 파일 → **아이템 이름**이고, 아이템
  `Description` 은 ODT's Item Info 가 화면에서 갈아끼우므로 **번역해도 보이지 않는다**.
- 그래서 이번 업데이트가 **한글화를 되돌린 것은 없다.** 새 총기·부품 이름이 영문으로 뜨는 것은
  업데이트 전에도 모드 아이템 5,964개가 그랬던 것과 같은 성질이다.

> 재는 법: 서버를 띄우고 `node tools/check-locale-dict.mjs kr`(사전이 입혔나),
> `node tools/dump-untranslated.mjs "<정규식>"`(아직 영문인 열쇠). 뒤엣것은 `--tsv` 로
> 번역 거리를 바로 뽑는다.

## 5. 다시 재는 법

```
node tools/check-mod-updates.mjs             # 업데이트 있는 모드 (실패하면 종료 코드 1)
node tools/check-installed-vs-extracted.mjs  # 설치 후 달라진 파일
node tools/download-ranged.mjs <stage> <id>  # 한 줄기를 조이는 호스트에서 끊어 받기
```

앞의 것은 Forge API 를 모드마다 한 번 부르므로 약 5분 걸린다. **종료 코드가 1 이면 조회 실패가
있다는 뜻이고, 그때의 "업데이트 없음" 은 믿을 수 없다.**
