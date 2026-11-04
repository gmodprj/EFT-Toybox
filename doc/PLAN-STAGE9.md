# 9차 설치·시험 설계 — 공개 목록 125개와 부착물 호환 수단 (2026-11-04)

후보 문서 둘을 하나의 순차 설치·시험 계획으로 묶은 것이다.

- [CANDIDATES-ATTACH-COMPAT.md](CANDIDATES-ATTACH-COMPAT.md) — 총기별 부착물 호환을 정하는 수단 (1순위 Easy Slot Filters)
- [CANDIDATES-LIST-127534.md](CANDIDATES-LIST-127534.md) — 공개 목록에서 추린 미설치 후보 125개

절차 자체는 [PROCEDURE.md](PROCEDURE.md) 를 그대로 따른다. 이 문서는 **무엇을 어느 순서로, 어디서 멈추는지**만 정한다.

설계일 기준으로 **아무것도 다운로드·설치하지 않았다.** 아래 수치는 Forge API 값이고, 실제 파일 겹침은 매 단계 `tools/check-conflicts.mjs` 를 거쳐야 안다.

## 줄 세운 기준

[PLAN-STAGE8.md](PLAN-STAGE8.md) 의 기준을 그대로 쓰고, 9차에서 둘을 더한다.

1. **선행이 먼저다.** 다른 모드가 요구하는 프레임워크를 앞으로 뺀다(9-B).
2. **비결속이 먼저다.** 파일만 지우면 끝나는 것부터.
3. **용량이 작은 것이 먼저다.** 문제 범위를 좁히기 쉽다.
4. **설치본과 역할이 겹치는 것은 뒤로.** 택일을 9-0 에서 먼저 정한다.
5. (9차 추가) **서버 테스트로 검증되는 것과 게임을 띄워야 보이는 것을 섞지 않는다.** 서버 쪽 모드는 묶어서, 게임 전용(그래픽·조작)은 하나씩 간다. 섞으면 "로그는 깨끗한데 게임이 이상하다"에서 원인을 못 가린다.
6. (9차 추가) **Forge 밖에서 받는 것은 이 계획에 넣지 않는다.** SPT Essentials 가 그렇다(§보류).

## 전체 분량

| 단계 | 내용 | 개수 | 용량 | 설치 방식 |
| --- | --- | --- | --- | --- |
| 9-0 | 결정·기준선 (설치 없음) | - | - | - |
| 9-A | 부착물 호환 — Easy Slot Filters | 1 | 6 KB | 단독 |
| 9-B | 선행·프레임워크 | 7 | 7.1 MB | 한 번에 복사(`-SkipTest`) → 마지막에 서버 테스트 한 번 |
| 9-C | 편의·인벤토리 — 소형 비결속 | 11 | 6.6 MB | 한 번에 복사(`-SkipTest`) → 마지막에 서버 테스트 한 번 |
| 9-D | UI·표시 — 소형 비결속 | 12 | 44.4 MB | 한 번에 복사(`-SkipTest`) → 마지막에 서버 테스트 한 번 |
| 9-E | 퀘스트·하이드아웃·진행 — 비결속 | 12 | 20.3 MB | 한 번에 복사(`-SkipTest`) → 마지막에 서버 테스트 한 번 |
| 9-E2 | 효과 프레임워크를 쓰는 것 — 단독 | 1 | 9 KB | 하나씩 설치·테스트 |
| 9-F | 전투·조작·이동 — 비결속 | 12 | 2.7 MB | 한 번에 복사(`-SkipTest`) → 마지막에 서버 테스트 한 번 |
| 9-F2 | 봇 행동을 바꾸는 것 — 단독 | 1 | 807 KB | 하나씩 설치·테스트 |
| 9-G | 전리품·경제·봇 이름 — 비결속 | 11 | 1.2 MB | 한 번에 복사(`-SkipTest`) → 마지막에 서버 테스트 한 번 |
| 9-H | 맵·레이드 — 소형 비결속 | 7 | 16.4 MB | 하나씩 설치·테스트 |
| 9-I | 그래픽·음향 — 하나씩 | 16 | 489.5 MB | 하나씩 설치·테스트 |
| 9-J | 도구·성능 — 하나씩 | 7 | 7.7 MB | 하나씩 설치·테스트 |
| 9-K | 결속 — 소형, 하나씩 | 6 | 900 KB | 프로필 백업 → 하나씩 설치·테스트 → 프로필 비교 |
| 9-L | 결속 — 콘텐츠·대형, 하나씩 | 11 | 3.57 GB | 프로필 백업 → 하나씩 설치·테스트 → 프로필 비교 |
| 9-M | 겹침·초대형 — 9-0 의 택일이 끝난 뒤 | 7 | 5.36 GB | 하나씩 설치·테스트 |
| 9-N | 보류 — 이 계획에서는 넣지 않는다 | 4 | 5.25 GB | 넣지 않는다 |

9-N(보류)을 빼면 실제로 받는 양은 약 **9.5 GB** 다. 9-M 의 결정에서 겹침 모드를 빼기로 하면 더 줄어든다.

## 9-0 — 설치 전에 끝낼 결정 (설치 없음)

설치본(`E:\Spt`)은 건드리지 않는다. 여기서 정하지 않으면 뒤 단계가 서로를 덮는다.

진행 상황 — **9-0-1(기준선)·9-0-2(PIT 기록)는 2026-11-04 에 끝냈다.** 남은 것은 9-0-3(택일 4건)·9-0-4(확인만)·9-0-5(차단 재확인)이고, 9-0-3 은 사용자가 정해야 한다.

### 9-0-1 기준선 — **재측정 완료 (2026-11-04)**

`tools/gen-installed-mods.mjs` 머리글의 로드 수(145 / 116, 2026-10-29)는 손으로 적은 값이라 실측과 다르다 — **그 줄은 기준선으로 쓰지 않는다.** 아래가 기준선이다.

| 항목 | 실측값 | 근거 |
| --- | --- | --- |
| 게임 플러그인 | **147** (`147 plugins to load`) | `BepInEx/LogOutput.log`, 수정 시각 2026-11-04 13:44 KST |
| 게임 `[Error` / `[Fatal` | **15줄 / 0줄** | 같은 파일. 내역은 아래 |
| 서버 모드 | **115** (`ModLoader: 총 115 개`, `불러옴` 115줄 = 전부 로드) | `SPT_Runtime/user/logs/spt/spt20261104.log`, 마지막 기동 04:45:24 UTC(13:45 KST) |
| 서버 `[Error]`/`[Critical]` | **49줄** (알려진 48 + 신규 1) | 같은 파일, 마지막 기동 구간만 |
| 서버 시작 완료 | 웹서버 04:47:22, `Loaded self-signed certificate` — 약 **118초** | 같은 파일 |
| 디스크 | `E:\` 여유 **1008.2 GB** (사용 854.8 GB) | 9-M·9-N 을 다 넣어도(14.8 GB) 여유는 충분하다 |

보조 수치 — 디스크의 플러그인 DLL 171개, 패처 7개, 서버 모드 폴더 **115개**(로그와 일치).

문서의 옛 값과 다른 이유

- 서버 **116 → 115**: PIT Fireteam 의 서버 모드를 빼낸 만큼이다(9-0-2). 폴더 수와 로그가 모두 115 로 맞는다.
- 게임 **145 → 147**: 그 뒤 설치분이 들어왔다. 옛 값은 2026-10-29 것이다.

게임 쪽 `[Error` 15줄 내역 — **전부 기존 것이고, 새로 생긴 것은 없다.** 단계마다 이 내역과 비교한다.

| 건수 | 출처 | 비고 |
| --- | --- | --- |
| 6 | SAIN `[sectactPriestEvent] does not exist` | 프리셋에 없는 봇 종류. 기존 |
| 6 | SPT Team Manager `[AimingFailure]` | 동료 조준 실패 로그. 기존 |
| 1 | SPT Team Manager `[MedicalActivity]` | 동료 치료 로그. 기존 |
| 1 | ECOTI `[COTI] Per-frame state update failed` | PROCEDURE 에 적힌 기존 건 |
| 1 | `[DirectReload] Patch FAILED ... Fika ObservedFirearmController not found` | PROCEDURE 의 "알려진 정상 메시지" |

이 기동은 **레이드까지 들어갔다**(`RaidStartPatch`, 프레임 55850 기록) — 메뉴까지만 돌았던 옛 기준선보다 확인 범위가 넓다.

서버 `[Error]` 49줄 가운데 48줄은 `Could not add <id> to cache` 로 `MountOlympus.dll` 이 원인인 기존 건이다([REVIEW-LOAD-ORDER.md](REVIEW-LOAD-ORDER.md)). **남은 1줄은 기존 기록에 없던 것이다.**

```
[Error][SPTarkov.Server.Core.Loaders.BundleLoader] Unable to add bundle:
assets/content/items/mods/barrels/barrel_vpo215_600mm_366tkm.bundle
```

번들 하나가 중복 등록으로 거부된 것으로 보인다(같은 번들 경로를 두 모드가 들고 있을 때 난다). VPO-215 600mm 총열 하나에 국한되고 시작은 정상 완료했으므로 9차를 막지는 않지만, **기준선에 포함시켜 둔다** — 9차 중에 이 줄이 늘어나면 새로 넣은 모드가 번들을 겹치게 가져온 것이다. 원인 모드를 가리는 일은 9차 범위가 아니다.

아직 못 맞춘 것 — `147 plugins to load` 인데 `Loading [` 줄은 **108개**뿐이다. 차이 39개를 설명하지 못했다. 실패·건너뜀 로그는 없다. 단계별 판정에는 **`plugins to load` 숫자만** 쓰고(이 숫자는 설치 전후 비교에 쓸 수 있다), `Loading [` 줄 수는 보조로만 본다.

### 9-0-2 PIT Fireteam(2676) 기록 — **고침 완료 (2026-11-04)**

설치 기록에 제거가 적히지 않아 자동 생성 문서가 PIT 를 설치된 것으로 적고 있었다(실제로는 STM 을 넣을 때 빼냈다, `backup/2026-11-01-before-stm/`).

한 것

1. 설치본 확인 — `BepInEx\plugins\pitFireTeam` 과 `SPT_Runtime\user\mods\pitFireTeam-ServerMod` **둘 다 없다**. 제거는 끝난 상태였고 문서만 뒤처져 있었다.
2. `doc/install-records/installed-stage8E-2676.txt` →
   [`removed-2026-11-01-stage8E-2676-pitfireteam.md`](install-records/removed-2026-11-01-stage8E-2676-pitfireteam.md) 로 바꾸고, 왜 빼냈는지와 실측 결과를 머리말로 붙였다. 파일 목록 26줄은 그대로 남겼다 — 설치 당시 목록이 곧 지운 목록이다.
   `gen-installed-mods.mjs` 가 `install-records/*.txt` 만 읽으므로 확장자를 `.md` 로 바꾸면 자동 생성 목록에서 빠진다. 기록을 지우지 않고 빼내는 방법이다.
3. `node tools/gen-installed-mods.mjs` 재생성 → 모드 **185 → 184개**, 파일 **10627 → 10601개**(26개 = PIT 분), PIT 행은 `게임+서버 / 26` 에서 **미설치 / 0** 으로 바뀌었다. 매니페스트에는 남아 있어 표에서 사라지지 않고 "미설치"로 보인다 — 다시 후보로 올라오는 것을 막는 쪽으로는 이게 낫다.

**PIT 는 어떤 단계에서도 다시 넣지 않는다** — STM 과 같은 `/singleplayer/pitfireteam/*` 라우트를 쓴다.

### 9-0-3 택일 4건

| 넣으려는 것 | 설치본의 상대 | 정해야 할 것 |
| --- | --- | --- |
| Amands's Graphics (592) | Amands' Sense | 같은 제작자의 다른 모드다. 함께 둘 수 있는지, 아니면 하나만 쓸지 |
| DynamicMaps Extended (3097, 1.7 GB) | Dynamic Maps | 확장인지 교체인지. 선행에 Dynamic Maps 가 적혀 있어 덧붙이는 쪽으로 보이나 파일을 받아야 확정된다 |
| APBS (1594) · ABPS (2097) | SAIN · MoreBotsAPI | 봇 생성·배치 주체가 둘이 되면 스폰이 겹친다. **빼기로 하면 Arena Mode(3175)도 함께 빠진다** |
| Hideout Overhaul (2982) · Vertical Hideout (2530) | 서로 | 둘 다 하이드아웃 배치를 건드린다. 9-E 에서 함께 넣고 겹치면 하나만 남긴다 |

### 9-0-4 Fika 비호환 표시 3건은 해당 없다

Loot In Vicinity(2698)·Combat Log(3072)·Arena Mode(3175)에 Fika 비호환 표시가 있으나, 이 설치본은 Fika 를 쓰지 않는다 — 게임 로그의 `FixPluginTypesSerialization ... references Fika.Core which doesn't exist` 가 그 증거다(PROCEDURE 의 정상 메시지 표). 이 표시만으로 빼지 않는다.

### 9-0-5 차단 4건 확인

`node tools/mod-detail.mjs 1426 2882` 로 선행 모드가 4.1.6 으로 올라왔는지 다시 본다. 아직이면 9-N 은 그대로 보류다.

## 단계마다 되풀이하는 틀

어느 단계든 같은 순서다. **하나라도 아니면 그 단계에서 멈춘다.**

1. **매니페스트** — `doc/download/manifest-stage9-<단계>.json`
2. **받기** — `node tools/download-stage.mjs stage9-<단계>` → 바이트 수가 `size` 와 맞는지
3. **해제** — `/c/Windows/System32/tar.exe -xf <파일> -C extracted/stage9-<단계>/<이름>` (절대경로로 부른다)
4. **겹침 검사** — `node tools/check-conflicts.mjs` → 설치본과도, 모드끼리도 겹치지 않아야 진행
5. **설치** — `tools/install-and-test.ps1 -Stage stage9-<단계> -Pattern '<id>__*' -Tag 'stage9-<단계>-<이름>'`
   - 덮어쓸 파일이 하나라도 있으면 아무것도 복사하지 않고 선다. **그때는 되돌릴 것이 없다 — 그 모드를 빼고 간다.**
   - 묶음 단계는 각 모드를 `-SkipTest` 로 복사한 뒤 마지막에 테스트 한 번.
6. **서버 검증** — `doc/server-tests/` 출력에서 `ModLoader: 총 N 개`, 각 `... 불러옴`, `[Error]`/`[Critical]`, `Loaded self-signed certificate`
7. **게임 검증** — `LogOutput.log` 의 **수정 시각이 설치 이후인지 먼저 보고**, 그다음 `N plugins to load` 와 `[Error`/`[Fatal`
8. **목록 갱신** — `node tools/gen-installed-mods.mjs`

### 통과 판정

- 서버 모드 수 = 기준선 + 이 단계의 서버 모드 수. **"시작됐다"가 아니라 숫자를 센다.**
- 게임 플러그인 수 = 기준선 + 이 단계의 플러그인 수.
- 새 `[Error]`/`[Fatal` 이 PROCEDURE 의 "알려진 정상 메시지" 표에 없는 것이면 실패로 본다.
- `LogOutput.log` 를 읽을 수 없거나 시각이 설치 이전이면 **"아직 확인되지 않음"** 이다. 통과로 치지 않는다.

### 멈춤 판정

- 겹침 검사에서 덮어쓰기가 나오면 멈춘다.
- 서버 시작이 실패하면 그 단계에서 설치한 것을 `doc/install-records/installed-<Tag>.txt` 에 적힌 파일만 지워 되돌린다.
- 결속 모드를 이미 플레이한 뒤라면 파일 삭제로는 안 된다 — 그 단계 이전 프로필 백업을 복원해야 한다.

## 9-A — 부착물 호환: Easy Slot Filters (1개, 6 KB)

[CANDIDATES-ATTACH-COMPAT.md](CANDIDATES-ATTACH-COMPAT.md) 쪽이다. **공개 목록 단계들과 섞지 않고 맨 앞에서 단독으로** 끝낸다 — 아이템 DB 를 고치므로 뒤 단계의 아이템 추가 모드와 뒤섞이면 원인을 못 가린다.

| 항목 | 값 |
| --- | --- |
| 모드 | [Easy Slot Filters](https://sp-mod.com/mod/2940) (2940), 1.0.0 / SPT 4.1.6 |
| 크기 / 다운로드 | 5.94 KB / 131 — **거의 검증되지 않은 신규 모드** |
| 고치는 곳 | 서버 `templateTable.Items` = **전역 DB**. 봇·상인·플리마켓에도 적용된다 |
| 설정 | 모드 폴더의 `config.jsonc`, 첫 기동 때 자기가 만든다 |

### 전제 — All In Weapon 과 함께 쓰지 않는다

`All In Weapon: Unslotted`(2576)는 칸 필터를 통째로 없앤다. 그것이 깔린 채로 Easy Slot Filters 를 넣으면 **넓힐 대상이 이미 다 열려 있어 효과가 보이지 않는다.** 그래서 이 단계의 전제는 All In Weapon 제거다. 제거 자체는 이 계획의 범위가 아니고, **사용자가 따로 지시할 때만** 한다.

제거하면 `ConflictingItems`(부품끼리 서로 막는 목록)도 되돌아온다 — All In Weapon 은 그것까지 지운다. Easy Slot Filters 는 `ConflictingItems` 를 건드리지 않으므로, **서로 막는 조합은 Easy Slot Filters 로 풀 수 없다.** 이것이 9-A 의 가장 큰 미지수다.

### 시험 순서

1. All In Weapon 이 빠진 상태에서 서버를 한 번 띄워 **바닐라 필터로 돌아온 것**을 먼저 확인한다(기준선).
2. Easy Slot Filters 만 설치하고 서버 테스트 → `config.jsonc` 가 생겼는지 본다.
3. 기본 예시 한 줄(MP-43-1C ← 톱질 총열)이 게임에서 실제로 달리는지 본다. **이 한 줄이 되면 구조가 맞는 것이고, 안 되면 그 뒤 설정은 전부 무의미하다.**
4. 원하는 조합을 한 번에 다 적지 않는다. **한 무기 → 한 칸 → 한 부착물씩** 넣고 게임에서 확인한다.

### 먼저 알고 들어갈 한계 (소스에서 확인한 것)

- **넓히기만 된다.** 필터에 `Add` 만 하고 제거가 없다 — 달리는 것을 **못 달게** 만들 수는 없다.
- **한 무기에서 같은 부착물을 두 칸에 넣을 수 없다.** 설정이 `{부착물: 칸}` 이라 부착물 id 가 키다. 칸이 여러 개인 상부·레일에서 걸린다.
- **무기 id 가 DB 에 없으면 조용히 넘어간다**(로그도 없다). 오타를 알아채기 어렵다 — 한 줄씩 넣는 이유가 이것이다.
- 로드 순위가 `OnLoadOrder.TraderRegistration - 1` 로 **이르다.** 뒤에 오는 DB 수정 모드가 같은 칸을 덮으면 밀린다. 9-L 에서 아이템을 추가하는 모드를 넣은 **뒤에 9-A 의 조합이 아직 되는지 다시 본다.**
- `config.jsonc` 가 깨지면 서버가 예외를 던지며 선다. 그때는 파일을 지우고 재생성한다.

### 안 되면 — 직접 만드는 쪽

위 한계 중 하나라도 막으면 `src/SlotCategoryRelax` 를 무기 단위로 좁히는 쪽이 대안이다. `src/WebConfigBridge` 에 이미 All In Weapon 설정 화면이 있어 UI 도 재사용된다. 자세한 셈은 후보 문서의 "세 번째 길"에 있다.

## 공개 목록 단계 (9-B ~ 9-N)

### 9-B — 선행·프레임워크 (7개, 7.1 MB)

**설치 방식** — 한 번에 복사(`-SkipTest`) → 마지막에 서버 테스트 한 번.

뒤 단계의 모드가 요구하는 것, 그리고 설정·로그 기반을 깔아 주는 것. 이것부터 넣지 않으면 뒤에서 의존성 때문에 멈춘다.

| 모드 | id | 버전 | 크기 | 결속 | 의존성 |
| --- | --- | --- | --- | --- | --- |
| [Effects Manager Framework](https://sp-mod.com/mod/3123) | 3123 | 1.1.0 | 30 KB | - | - |
| [WebOverlay](https://sp-mod.com/mod/2879) | 2879 | 1.11.0 | 170 KB | - | - |
| [Map Variants](https://sp-mod.com/mod/3040) | 3040 | 1.1.0 | 4 MB | - | - |
| [Quest Codex](https://sp-mod.com/mod/3054) | 3054 | 1.11.0 | 3 MB | - | - |
| [Item Property Backport](https://sp-mod.com/mod/2689) | 2689 | 1.5.2 | 128 KB | - | - |
| [Hj's Mod Source](https://sp-mod.com/mod/3083) | 3083 | 1.1.0 | 39 KB | - | - |
| [Integrated Configuration Manager](https://sp-mod.com/mod/3095) | 3095 | 1.0.0 | 19 KB | - | - |

볼 것

- 서버 테스트 출력에서 `ModLoader: 총 N 개`가 9-0 기준선 + 서버 모드 수만큼 늘었는지.
- WebOverlay(2879)는 포트를 하나 더 연다 — 서버 로그에 그 포트가 올라오는지, 6969 와 충돌하지 않는지.
- Map Variants(3040)·Quest Codex(3054)는 각자 설정 파일을 만든다. `install-and-test.ps1` 이 테스트 중 새로 생긴 파일까지 기록에 넣는다 — 기록 파일을 열어 실제로 들어갔는지 본다.

3123 → 3121(9-E), 2879 → 3072(9-J), 3040 → 3097(9-M), 3054 → 3158(9-D) 의 선행이다. 이 넷 중 하나라도 실패하면 그 뒤를 함께 뺀다.

### 9-C — 편의·인벤토리 — 소형 비결속 (11개, 6.6 MB)

**설치 방식** — 한 번에 복사(`-SkipTest`) → 마지막에 서버 테스트 한 번.

전부 파일만 지우면 되돌아가고, 합쳐도 7 MB 를 넘지 않는다. 문제가 나도 범위가 좁다.

| 모드 | id | 버전 | 크기 | 결속 | 의존성 |
| --- | --- | --- | --- | --- | --- |
| [MergeConsumables](https://sp-mod.com/mod/1657) | 1657 | 1.6.3 | 20 KB | - | - |
| [Continuous Healing](https://sp-mod.com/mod/1884) | 1884 | 1.6.3 | 6 KB | - | - |
| [Healing Autocancel](https://sp-mod.com/mod/1274) | 1274 | 5.0.1 | 4 KB | - | - |
| [HandsAreNotBusy](https://sp-mod.com/mod/1298) | 1298 | 1.7.1 | 5 KB | - | - |
| [Gilded Key Storage](https://sp-mod.com/mod/865) | 865 | 2.1.1 | 6 MB | - | - |
| [Use Loose Loot](https://sp-mod.com/mod/933) | 933 | 1.6.0 | 3 KB | - | - |
| [Loot In Vicinity](https://sp-mod.com/mod/2698) | 2698 | 3.0.6 | 45 KB | - | - |
| [Cabinet Looter](https://sp-mod.com/mod/3103) | 3103 | 1.0.3 | 19 KB | - | - |
| [Raid Settings Skipper](https://sp-mod.com/mod/2857) | 2857 | 1.3.1 | 5 KB | - | - |
| [AutoKeycard for Labs](https://sp-mod.com/mod/2881) | 2881 | 1.0.1 | 6 KB | - | - |
| [Dynamic Item Weights](https://sp-mod.com/mod/2344) | 2344 | 1.2.1 | 7 KB | - | - |

볼 것

- 게임에서 인벤토리를 한 번 열어 본다 — 이 묶음은 전부 인벤토리·치료 화면을 건드린다.
- Loot In Vicinity(2698)는 Forge 가 Fika 비호환으로 표시한다. 이 설치본은 Fika 를 쓰지 않으므로 해당 없다(9-0 참조).
- Dynamic Item Weights(2344)는 아이템 무게를 서버에서 덮어쓴다. SVM 의 무게 옵션과 겹치므로 설치 후 아이템 무게를 하나 집어 확인한다.

### 9-D — UI·표시 — 소형 비결속 (12개, 44.4 MB)

**설치 방식** — 한 번에 복사(`-SkipTest`) → 마지막에 서버 테스트 한 번.

서버 쪽이 거의 없고 화면만 바꾼다. 서버 테스트로는 거의 안 드러나므로 **게임을 한 번 띄워 확인하는 것이 이 단계의 본 시험**이다.

| 모드 | id | 버전 | 크기 | 결속 | 의존성 |
| --- | --- | --- | --- | --- | --- |
| [WTT - Menu Overhaul](https://sp-mod.com/mod/1775) | 1775 | 1.3.0 | 761 KB | - | - |
| [ImpactCue-UI-MOD](https://sp-mod.com/mod/3029) | 3029 | 2.0.3 | 40 MB | - | - |
| [Hj's Improved Customization UI](https://sp-mod.com/mod/3069) | 3069 | 1.1.2 | 184 KB | - | - |
| [Hj's Task Item Indicator](https://sp-mod.com/mod/3082) | 3082 | 1.0.1 | 9 KB | - | - |
| [Hj's Task Turn in Indicator](https://sp-mod.com/mod/3071) | 3071 | 1.0.0 | 21 KB | - | - |
| [Name Plate](https://sp-mod.com/mod/3131) | 3131 | 1.1.0 | 2 MB | - | - |
| [Briefing](https://sp-mod.com/mod/3158) | 3158 | 1.0.2 | 385 KB | - | Quest Codex |
| [Breakout Outlines](https://sp-mod.com/mod/3007) | 3007 | 1.2.0 | 39 KB | - | - |
| [Wishlist Extended](https://sp-mod.com/mod/2500) | 2500 | 1.2.0 | 53 KB | - | - |
| [Inventory Walker](https://sp-mod.com/mod/3058) | 3058 | 0.3.1 | 13 KB | - | - |
| [Easy Ammo Names](https://sp-mod.com/mod/1262) | 1262 | 2.1.0 | 16 KB | - | - |
| [More Tag Colours](https://sp-mod.com/mod/1533) | 1533 | 1.5.0 | 5 KB | - | - |

볼 것

- `E:\Spt\BepInEx\LogOutput.log` 의 `N plugins to load` 가 9-0 기준선 + 이 묶음의 플러그인 수와 맞는지.
- 같은 화면을 건드리는 것끼리 겹침을 본다 — Hj 의 개조 화면(3069) ↔ 설치된 개조 화면 모드, Menu Overhaul(1775) ↔ 설치된 메뉴 계열.
- Briefing(3158)은 Quest Codex(3054, 9-B)가 먼저 들어가 있어야 한다.
- 한국어 표기가 깨지는 곳이 있는지 본다 — 이 묶음은 문자열을 많이 바꾼다.

### 9-E — 퀘스트·하이드아웃·진행 — 비결속 (12개, 20.3 MB)

**설치 방식** — 한 번에 복사(`-SkipTest`) → 마지막에 서버 테스트 한 번.

프로필을 읽기는 하지만 결속 표시가 없는 것들. 하이드아웃·스킬 화면이 함께 바뀌므로 한 묶음으로 보고 한 번에 확인한다.

| 모드 | id | 버전 | 크기 | 결속 | 의존성 |
| --- | --- | --- | --- | --- | --- |
| [All Quests Checkmarks](https://sp-mod.com/mod/2025) | 2025 | 1.4.0 | 45 KB | - | - |
| [Expanded Task Text (ETT)](https://sp-mod.com/mod/2153) | 2153 | 2.1.0 | 50 KB | - | - |
| [Hideout In Progress](https://sp-mod.com/mod/2076) | 2076 | 3.0.0 | 22 KB | - | - |
| [Hideout Overhaul](https://sp-mod.com/mod/2982) | 2982 | 0.2.0 | 67 KB | - | - |
| [Vertical Hideout](https://sp-mod.com/mod/2530) | 2530 | 1.1.1 | 8 KB | - | - |
| [Target Dummies](https://sp-mod.com/mod/2932) | 2932 | 0.3.1 | 15 KB | - | - |
| [Tarkin's Hideout Cat](https://sp-mod.com/mod/2908) | 2908 | 1.1.1 | 20 MB | - | - |
| [Cultist Circle Improvements](https://sp-mod.com/mod/2657) | 2657 | 1.1.0 | 119 KB | - | - |
| [Simple Workout QTE](https://sp-mod.com/mod/1437) | 1437 | 2.2.2 | 13 KB | - | - |
| [Skill Distribution](https://sp-mod.com/mod/2144) | 2144 | 1.3.0 | 27 KB | - | - |
| [Health Per Level](https://sp-mod.com/mod/1706) | 1706 | 2.2.0 | 37 KB | - | - |
| [Fair Progression](https://sp-mod.com/mod/2652) | 2652 | 1.1.0 | 28 KB | - | - |

볼 것

- 하이드아웃 화면을 열어 본다. Hideout Overhaul(2982)·Vertical Hideout(2530)은 **같은 배치를 건드릴 수 있다** — 둘이 겹치면 하나만 남긴다.
- SVM `Hideout.RemoveConstructionsRequirements` 는 **false 유지**다(PROCEDURE 의 SVM 메모). 하이드아웃 모드를 넣은 뒤 이 값이 바뀌지 않았는지 본다.
- 스킬 화면과 레벨당 체력(1706·2144)은 프로필 수치를 다시 계산한다. 설치 전 프로필을 백업해 두고, 설치 후 체력·스킬이 사라지지 않았는지 본다.

Broken Limbs Don't Heal(3121)은 Effects Manager Framework(3123, 9-B) 뒤에 단독으로 넣는다 — 같은 묶음에 섞으면 선행 실패와 구분이 안 된다.

### 9-E2 — 효과 프레임워크를 쓰는 것 — 단독 (1개, 9 KB)

**설치 방식** — 하나씩 설치·테스트.

선행(3123)이 제대로 붙었는지를 이 하나로 검증한다.

| 모드 | id | 버전 | 크기 | 결속 | 의존성 |
| --- | --- | --- | --- | --- | --- |
| [Broken Limbs Don't Heal](https://sp-mod.com/mod/3121) | 3121 | 1.1.0 | 9 KB | - | Effects Manager Framework |

볼 것

- 서버·게임 로그에 Effects Manager 쪽 경고가 없는지.
- 골절 상태가 레이드 종료 후 유지되는지는 레이드를 한 번 돌아야 안다 — 로그만으로는 확정되지 않는다.

### 9-F — 전투·조작·이동 — 비결속 (12개, 2.7 MB)

**설치 방식** — 한 번에 복사(`-SkipTest`) → 마지막에 서버 테스트 한 번.

전부 게임 쪽 플러그인이고 합쳐도 3.5 MB 다. 다만 **조작감이 바뀌는 것이라 로그로는 확인되지 않는다** — 레이드를 한 번 돌아야 한다.

| 모드 | id | 버전 | 크기 | 결속 | 의존성 |
| --- | --- | --- | --- | --- | --- |
| [Bring Back Concussion](https://sp-mod.com/mod/2550) | 2550 | 1.2.1 | 12 KB | - | - |
| [CookingGrenades 4.1](https://sp-mod.com/mod/2951) | 2951 | 1.4.4 | 2 MB | - | - |
| [Inspectionless Malfunctions Reborn](https://sp-mod.com/mod/2992) | 2992 | 1.0.0 | 5 KB | - | - |
| [Deminvincibility](https://sp-mod.com/mod/1117) | 1117 | 2.2.0 | 8 KB | - | - |
| [Smajlec's Stamina](https://sp-mod.com/mod/2608) | 2608 | 1.4.0 | 5 KB | - | - |
| [Classic Movement](https://sp-mod.com/mod/1860) | 1860 | 1.1.3 | 62 KB | - | - |
| [Climbable Ladders](https://sp-mod.com/mod/2649) | 2649 | 1.0.4 | 920 KB | - | - |
| [Increase Climb Height](https://sp-mod.com/mod/1575) | 1575 | 2.1.1 | 8 KB | - | - |
| [Increase Look Direction](https://sp-mod.com/mod/2121) | 2121 | 1.3.0 | 5 KB | - | - |
| [Auto Shoulder Swap](https://sp-mod.com/mod/2639) | 2639 | 1.1.1 | 5 KB | - | - |
| [No Aim Swing](https://sp-mod.com/mod/2719) | 2719 | 1.2.0 | 4 KB | - | - |
| [Deadzone](https://sp-mod.com/mod/3067) | 3067 | 1.2.0 | 7 KB | - | - |

볼 것

- `LogOutput.log` 의 플러그인 수와 `[Error`/`[Fatal` 유무.
- Classic Movement(1860) ↔ Smajlec's Stamina(2608) ↔ Increase Climb Height(1575) ↔ Climbable Ladders(2649)는 모두 이동을 건드린다. 한 묶음에 넣되, 레이드에서 이상하면 이 넷을 먼저 하나씩 빼 본다.
- Deadzone(3067)·No Aim Swing(2719)은 조준 처리를 겹쳐 바꿀 수 있다. 둘 중 하나만 쓰는 쪽이 안전하다.

ORBIT 3.0(2706)은 SAIN·Waypoints·BigBrain 을 모두 요구하고 봇 행동을 바꾼다 — 9-F2 로 떼어 단독 시험한다.

### 9-F2 — 봇 행동을 바꾸는 것 — 단독 (1개, 807 KB)

**설치 방식** — 하나씩 설치·테스트.

설치된 SAIN 과 직접 맞물린다. 묶음에 섞으면 봇이 이상할 때 원인을 못 가린다.

| 모드 | id | 버전 | 크기 | 결속 | 의존성 |
| --- | --- | --- | --- | --- | --- |
| [ORBIT 3.0](https://sp-mod.com/mod/2706) | 2706 | 3.1.0 | 807 KB | - | SAIN - Solarint's AI Modifications - Full AI Combat System Replacement, Waypoints - Expanded Navmesh, BigBrain |

볼 것

- 서버·게임 로그에 SAIN 쪽 새 경고가 없는지.
- 레이드를 한 번 돌아 봇이 멈추거나 떼로 몰리지 않는지.

### 9-G — 전리품·경제·봇 이름 — 비결속 (11개, 1.2 MB)

**설치 방식** — 한 번에 복사(`-SkipTest`) → 마지막에 서버 테스트 한 번.

서버 쪽 전리품·가격 생성이라 서버 테스트로 거의 다 드러난다. 합쳐 1.2 MB.

| 모드 | id | 버전 | 크기 | 결속 | 의존성 |
| --- | --- | --- | --- | --- | --- |
| [Lots of Loot Redux](https://sp-mod.com/mod/1619) | 1619 | 4.2.2 | 110 KB | - | - |
| [Rai's Hidden Caches](https://sp-mod.com/mod/1265) | 1265 | 1.4.0 | 8 KB | - | - |
| [Bosses Have Lega Medals](https://sp-mod.com/mod/1539) | 1539 | 2.1.0 | 8 KB | - | - |
| [Bosses Have Gp Coins](https://sp-mod.com/mod/2523) | 2523 | 1.1.0 | 8 KB | - | - |
| [LootNET](https://sp-mod.com/mod/2679) | 2679 | 1.2.0 | 946 KB | - | - |
| [Crates In Airdrop](https://sp-mod.com/mod/2481) | 2481 | 2.0.0 | 8 KB | - | - |
| [Fast Sell In Flea](https://sp-mod.com/mod/2207) | 2207 | 1.3.1 | 12 KB | - | - |
| [Bot Callsigns - Reloaded](https://sp-mod.com/mod/1873) | 1873 | 2.0.6 | 20 KB | - | - |
| [Realistic PMC Names](https://sp-mod.com/mod/3064) | 3064 | 1.1.0 | 60 KB | - | - |
| [Smooth Talker](https://sp-mod.com/mod/2635) | 2635 | 1.3.0 | 11 KB | - | - |
| [Auto IFF](https://sp-mod.com/mod/2784) | 2784 | 2.1.1 | 15 KB | - | - |

볼 것

- 서버 테스트 출력에 플리마켓 생성 오류가 없는지 — 전리품·가격 모드는 여기서 터진다.
- Lots of Loot Redux(1619) ↔ 설치된 전리품 모드와 배율이 **누적**될 수 있다. 설치 후 맵 하나를 돌아 전리품 밀도를 눈으로 본다.
- Fast Sell In Flea(2207)는 플리 판매 흐름을 바꾼다. SVM 플리 설정과 함께 보아야 한다.

### 9-H — 맵·레이드 — 소형 비결속 (7개, 16.4 MB)

**설치 방식** — 하나씩 설치·테스트.

맵 자체를 건드리는 것은 터질 때 레이드 진입이 막힌다. 작아도 **하나씩** 넣고 그 맵에 한 번 들어가 본다.

| 모드 | id | 버전 | 크기 | 결속 | 의존성 |
| --- | --- | --- | --- | --- | --- |
| [Enable Labyrinth](https://sp-mod.com/mod/2416) | 2416 | 1.1.0 | 157 KB | - | - |
| [UNTAR Go Home!](https://sp-mod.com/mod/2342) | 2342 | 3.2.1 | 174 KB | - | SAIN - Solarint's AI Modifications - Full AI Combat System Replacement, BigBrain, WTT - CommonLib, MoreBotsAPI |
| [RUAF Come Home!](https://sp-mod.com/mod/2427) | 2427 | 1.2.1 | 10 MB | - | BigBrain, WTT- Armory, WTT - CommonLib, MoreBotsAPI, WTT - Content Backport, SAIN - Solarint's AI Modifications - Full AI Combat System Replacement |
| [Door Randomizer](https://sp-mod.com/mod/820) | 820 | 1.8.0 | 5 KB | - | - |
| [LET ME OUT](https://sp-mod.com/mod/2240) | 2240 | 1.1.0 | 6 KB | - | - |
| [WTT - Black Division [REDACTED] Home](https://sp-mod.com/mod/2511) | 2511 | 1.3.1 | 1 MB | - | SAIN - Solarint's AI Modifications - Full AI Combat System Replacement, BigBrain, WTT - CommonLib, MoreBotsAPI, WTT - Content Backport |
| [Rippers Casino](https://sp-mod.com/mod/3212) | 3212 | 1.0.1 | 5 MB | - | - |

볼 것

- 해당 맵으로 레이드 진입이 되는지. 로딩에서 멈추면 그 모드가 원인이다.
- UNTAR(2342)·RUAF(2427)·Black Division 집(2511)은 전부 SAIN·BigBrain·MoreBotsAPI·WTT CommonLib 를 쓴다 — 넷 다 설치본에 있다. 봇 스폰이 늘어나므로 프레임도 함께 본다.
- Enable Labyrinth(2416)·LET ME OUT(2240)·Door Randomizer(820)는 탈출구·문을 바꾼다. 탈출구 목록이 비지 않는지 본다.

### 9-I — 그래픽·음향 — 하나씩 (16개, 489.5 MB)

**설치 방식** — 하나씩 설치·테스트.

전부 게임 전용이라 **서버 테스트로는 아무것도 안 나온다**. 넣을 때마다 게임을 띄워 보는 것 외에는 검증 수단이 없다. Six 계열은 같은 제작자의 묶음이라 서로 전제가 있을 수 있어 특히 하나씩.

| 모드 | id | 버전 | 크기 | 결속 | 의존성 |
| --- | --- | --- | --- | --- | --- |
| [HollywoodFX](https://sp-mod.com/mod/2003) | 2003 | 3.0.0 | 258 MB | - | - |
| [CloudSix - Volumetric Clouds](https://sp-mod.com/mod/2567) | 2567 | 4.0.2 | 7 MB | - | - |
| [FogSix](https://sp-mod.com/mod/3110) | 3110 | 1.0.1 | 108 KB | - | - |
| [ReflectSix](https://sp-mod.com/mod/3099) | 3099 | 1.0.1 | 295 KB | - | - |
| [AOSix](https://sp-mod.com/mod/2569) | 2569 | 2.0.1 | 28 KB | - | - |
| [POMSix](https://sp-mod.com/mod/3100) | 3100 | 1.0.2 | 16 MB | - | - |
| [TraceSix - Full Raytracing and Voxel GI](https://sp-mod.com/mod/3221) | 3221 | 1.0.1 | 690 KB | - | - |
| [Better Night Skies](https://sp-mod.com/mod/2734) | 2734 | 2.0.0 | 68 MB | - | - |
| [Blackout](https://sp-mod.com/mod/2843) | 2843 | 4.1.1 | 6 MB | - | WTT - Content Backport |
| [Shadow Flicker Fix](https://sp-mod.com/mod/1621) | 1621 | 1.5.3 | 7 KB | - | - |
| [Headshot Darkness](https://sp-mod.com/mod/1582) | 1582 | 1.2.5 | 10 KB | - | - |
| [Ragdoll Kinetics](https://sp-mod.com/mod/2905) | 2905 | 1.2.0 | 44 KB | - | - |
| [Manimal's Watch Animations](https://sp-mod.com/mod/2745) | 2745 | 2.0.0 | 86 KB | - | - |
| [BushWhacker - Standalone](https://sp-mod.com/mod/2329) | 2329 | 1.7.0 | 6 KB | - | - |
| [Battle Ambience](https://sp-mod.com/mod/2264) | 2264 | 2.4.0 | 11 MB | - | - |
| [Music Extender](https://sp-mod.com/mod/2962) | 2962 | 1.2.0 | 122 MB | - | - |

볼 것

- `LogOutput.log` 수정 시각이 **설치 이후**인지 먼저 본다. 이전이면 그 모드는 아직 한 번도 로드되지 않았다.
- `N plugins to load` 가 하나씩 늘어나는지, `[Error`/`[Fatal` 이 새로 생기는지.
- TraceSix(3221)·POMSix(3100)·CloudSix(2567)는 무거운 효과다. 프레임을 함께 본다 — 돌아가는 것과 쓸 수 있는 것은 다르다.
- 설치된 Amands' Sense 와 화면이 겹치는지. Amands's Graphics(592)는 9-M 에서 택일한 뒤에 넣는다.

### 9-J — 도구·성능 — 하나씩 (7개, 7.7 MB)

**설치 방식** — 하나씩 설치·테스트.

로더·설정 화면·오버레이를 건드려 **다른 모드 전체에 영향이 간다**. 하나씩 넣고 그때마다 전체 로드 수를 다시 센다.

| 모드 | id | 버전 | 크기 | 결속 | 의존성 |
| --- | --- | --- | --- | --- | --- |
| [Raid Review](https://sp-mod.com/mod/1479) | 1479 | 1.10.0 | 5 MB | - | - |
| [Combat Log](https://sp-mod.com/mod/3072) | 3072 | 1.0.0 | 123 KB | - | WebOverlay |
| [ASBP - Acid's Scalable BepinEx Panel](https://sp-mod.com/mod/2931) | 2931 | 1.0.1 | 8 KB | - | - |
| [Better BepInEx](https://sp-mod.com/mod/3207) | 3207 | 1.0.3 | 61 KB | - | - |
| [Performance Scope](https://sp-mod.com/mod/3076) | 3076 | 0.2.0 | 11 KB | - | - |
| [Dynamic External Resolution Patch (DERP)](https://sp-mod.com/mod/2200) | 2200 | 1.1.2 | 7 KB | - | - |
| [Knight Mask Fix](https://sp-mod.com/mod/3109) | 3109 | 1.0.0 | 2 MB | - | - |

볼 것

- Better BepInEx(3207)는 BepInEx 자체를 건드린다 — 넣은 뒤 **플러그인 로드 수가 줄지 않았는지** 반드시 다시 센다. 줄면 되돌린다.
- Combat Log(3072)는 WebOverlay(2879, 9-B) 뒤에만 들어간다. Fika 비호환 표시는 이 설치본에 해당 없다.
- Raid Review(1479)는 자체 웹 화면과 DB 를 만든다. 설치된 WebConfigBridge 의 포트와 겹치지 않는지 본다.
- ASBP(2931)·Integrated Configuration Manager(3095, 9-B)는 F12 설정 화면을 바꾼다. 설정 화면이 열리는지 확인한다.

### 9-K — 결속 — 소형, 하나씩 (6개, 900 KB, 결속 6)

**설치 방식** — 프로필 백업 → 하나씩 설치·테스트 → 프로필 비교.

프로필에 자기 자료를 넣는 것들. 플레이한 뒤 빼면 프로필이 깨질 수 있어 **백업 → 하나 설치 → 테스트 → 프로필 비교**를 모드마다 되풀이한다.

| 모드 | id | 버전 | 크기 | 결속 | 의존성 |
| --- | --- | --- | --- | --- | --- |
| [IOF: Inventory Organizing Feature](https://sp-mod.com/mod/2960) | 2960 | 1.10.1 | 24 KB | **결속** | - |
| [Item Preview QoL + screenshots](https://sp-mod.com/mod/2206) | 2206 | 1.2.0 | 12 KB | **결속** | - |
| [PityLoot](https://sp-mod.com/mod/3077) | 3077 | 1.0.0 | 45 KB | **결속** | - |
| [TraumaCore](https://sp-mod.com/mod/2893) | 2893 | 1.7.0 | 106 KB | **결속** | - |
| [Lacy's PvE Tweaks](https://sp-mod.com/mod/2395) | 2395 | 1.3.2 | 18 KB | **결속** | - |
| [Quest Backport](https://sp-mod.com/mod/3026) | 3026 | 1.0.9 | 695 KB | **결속** | WTT - CommonLib, WTT - Content Backport |

볼 것

- `backup/profiles-before-stage9-K-<id>/` 에 프로필을 먼저 복사한다.
- 설치·서버 시작 후 프로필을 `tools/stm-profile-diff.mjs` 로 백업과 비교한다. **새로 생긴 키가 그 모드의 것인지** 확인한다.
- Quest Backport(3026)는 퀘스트를 프로필에 추가한다 — 되돌릴 때 백업 복원이 필수다. 가장 뒤에 넣는다.
- Lacy's PvE Tweaks(2395)는 범위가 넓은 묶음 설정이다. 설치된 SVM 과 겹치는 항목을 끈 상태로 시작한다.

### 9-L — 결속 — 콘텐츠·대형, 하나씩 (11개, 3.57 GB, 결속 11)

**설치 방식** — 프로필 백업 → 하나씩 설치·테스트 → 프로필 비교.

아이템·상인·스킬을 프로필에 심는다. 용량 순으로 작은 것부터 넣어, 문제가 났을 때 되돌릴 양을 줄인다. 합쳐 3.57 GB.

| 모드 | id | 버전 | 크기 | 결속 | 의존성 |
| --- | --- | --- | --- | --- | --- |
| [Manimal's CS Gas](https://sp-mod.com/mod/2864) | 2864 | 2.0.0 | 12 MB | **결속** | WTT - CommonLib, WTT - Content Backport |
| [Content Backport - Prestiges](https://sp-mod.com/mod/2540) | 2540 | 1.1.0 | 21 MB | **결속** | WTT - CommonLib, WTT - Content Backport |
| [Strange Curios](https://sp-mod.com/mod/3059) | 3059 | 1.1.0 | 26 MB | **결속** | WTT - CommonLib |
| [Borkel's Realistic Night Vision Goggles (NVGs and T-7)](https://sp-mod.com/mod/954) | 954 | 3.0.4 | 32 MB | **결속** | - |
| [Deploy Screen Overhaul](https://sp-mod.com/mod/3047) | 3047 | 1.1.0 | 62 MB | **결속** | - |
| [Tushonka Rare Collectibles](https://sp-mod.com/mod/3062) | 3062 | 1.2.2 | 73 MB | **결속** | - |
| [WTT Halloween Masks](https://sp-mod.com/mod/2401) | 2401 | 2.0.0 | 87 MB | **결속** | WTT - CommonLib |
| [Skills Extended](https://sp-mod.com/mod/2383) | 2383 | 3.1.4 | 110 MB | **결속** | - |
| [Russian Special Forces Pack](https://sp-mod.com/mod/3035) | 3035 | 2.0.1 | 560 MB | **결속** | WTT - CommonLib |
| [Epic's All in One](https://sp-mod.com/mod/1263) | 1263 | 5.0.2 | 1509 MB | **결속** | WTT - CommonLib |
| [ECOT - Eukyre's Consortium of Things](https://sp-mod.com/mod/2195) | 2195 | 2.0.1 | 1161 MB | **결속** | Epic's All in One, WTT - CommonLib |

볼 것

- 모드마다 프로필 백업 → 설치 → 서버 테스트 → 프로필 비교. 하나라도 서버 시작이 실패하면 거기서 멈춘다.
- ECOT(2195)는 Epic's All in One(1263)이 먼저 들어가 있어야 한다 — 이 둘의 순서를 바꾸지 않는다.
- 아이템을 추가하는 것들은 플리마켓 생성 단계에서 터진다. 사용자 서버를 끈 상태로 테스트해야 플리마켓 이후까지 확인된다(PROCEDURE 의 포트 주의).
- Skills Extended(2383)는 스킬 트리를 바꾼다. 기존 스킬 수치가 남는지 프로필 비교로 본다.
- 상인 줄이 넘치면 설치된 Kaeno-TraderScrolling 으로 해결한다(PROCEDURE 의 문제 표).

### 9-M — 겹침·초대형 — 9-0 의 택일이 끝난 뒤 (7개, 5.36 GB)

**설치 방식** — 하나씩 설치·테스트.

설치본의 모드와 역할이 겹치거나 혼자 GB 를 먹는 것들. **결정 없이 넣지 않는다.** 합쳐 5.36 GB.

| 모드 | id | 버전 | 크기 | 결속 | 의존성 |
| --- | --- | --- | --- | --- | --- |
| [Amands's Graphics](https://sp-mod.com/mod/592) | 592 | 1.8.0 | 32 KB | - | - |
| [APBS - Acid's Progressive Bot System](https://sp-mod.com/mod/1594) | 1594 | 2.4.1 | 1012 KB | - | - |
| [ABPS - Acid's Bot Placement System](https://sp-mod.com/mod/2097) | 2097 | 2.1.0 | 1 MB | - | - |
| [Arena Mode](https://sp-mod.com/mod/3175) | 3175 | 0.4.1 | 4 MB | - | SAIN - Solarint's AI Modifications - Full AI Combat System Replacement, APBS - Acid's Progressive Bot System, ABPS - Acid's Bot Placement System |
| [Manimal's 1.0 Labs Office Backport](https://sp-mod.com/mod/2937) | 2937 | 2.0.0 | 90 MB | - | WTT - CommonLib, WTT - Content Backport |
| [DynamicMaps Extended](https://sp-mod.com/mod/3097) | 3097 | 2.5.2 | 1739 MB | - | Dynamic Maps, Map Variants |
| [Arena Map Port](https://sp-mod.com/mod/3166) | 3166 | 0.4.0 | 3659 MB | - | - |

볼 것

- Amands's Graphics(592) ↔ 설치된 Amands' Sense — 같은 제작자의 다른 모드다. 함께 둘 수 있는지부터 확인한다.
- APBS(1594)·ABPS(2097) ↔ 설치된 SAIN·MoreBotsAPI — 봇 생성 주체가 둘이 되면 스폰이 겹친다. Arena Mode(3175)는 이 둘을 요구하므로 **1594·2097 을 넣지 않기로 하면 3175 도 빠진다**.
- DynamicMaps Extended(3097, 1.7 GB) ↔ 설치된 Dynamic Maps — 선행이 Dynamic Maps 와 Map Variants(3040, 9-B)다. 확장이므로 교체가 아니라 덧붙이는 쪽일 수 있다. 받아서 파일 구조를 봐야 확정된다.
- Arena Map Port(3166, 3.6 GB)는 이 목록에서 가장 크고 버전이 0.4.0 이다. 디스크 여유를 먼저 재고, 단독으로 넣는다.
- Labs Office Backport(2937)는 설치된 WTT Content Backport 와 같은 계열이다 — 파일 겹침 검사를 특히 꼼꼼히 본다.

### 9-N — 보류 — 이 계획에서는 넣지 않는다 (4개, 5.25 GB, 결속 1)

**설치 방식** — 넣지 않는다.

선행 모드가 4.1.6 에 없어서 지금은 설치 자체가 안 된다. 선행이 올라오면 9-M 뒤에 붙인다.

| 모드 | id | 버전 | 크기 | 결속 | 의존성 |
| --- | --- | --- | --- | --- | --- |
| [MANIMAL'S ICEBREAKER BACKPORT](https://sp-mod.com/mod/2866) | 2866 | 1.1.3 | 1595 MB | - | BigBrain, WTT - CommonLib, MoreBotsAPI, WTT - Black Division [REDACTED] Home, WTT - Content Backport, Climbable Ladders, Manimal's CS Gas, Manimal's 1.0 Interchange Expansion Backport , UnityToolkit |
| [Manimal's 1.0 Interchange Expansion Backport](https://sp-mod.com/mod/3025) | 3025 | 1.0.8 | 1440 MB | - | UnityToolkit, WTT - CommonLib, WTT - Content Backport |
| [Manimal's 1.0 Lighthouse Rework Backport](https://sp-mod.com/mod/3022) | 3022 | 1.0.7 | 2342 MB | - | UnityToolkit, WTT - CommonLib, WTT - Content Backport |
| [Rook](https://sp-mod.com/mod/3055) | 3055 | 1.2.2 | 747 KB | **결속** | Epic's All in One, ECOT - Eukyre's Consortium of Things, WTT- Armory, WTT - CommonLib, WTT - Content Backport, Eco's Attachment Emporium |

볼 것

- Manimal 백포트 셋(2866·3025·3022)은 **UnityToolkit(1426)** 을 요구하고, 그 모드는 4.1.5 까지만 있다.
- Rook(3055)은 **Eco's Attachment Emporium(2882)** 을 요구하고, 그것도 4.1.6 버전이 없다.
- 선행이 올라왔는지는 `node tools/mod-detail.mjs 1426 2882` 로 다시 확인한다.

## 보류 — SPT Essentials

[CANDIDATES-ATTACH-COMPAT.md](CANDIDATES-ATTACH-COMPAT.md) 의 2순위다. **이 계획에 넣지 않는다.**

- Forge 에서 내려가 제작자 직접 배포로 바뀌었다. 9차의 기준 6(Forge 밖은 넣지 않는다)에 걸린다.
- 모듈 12개 묶음이고, 그중 Expanded Special Slots·Compact HUD 는 설치본과 역할이 겹친다.
- 서버 시작 때 제작자 GitHub 를 확인하는 Update Check 모듈이 들어 있다.
- 다만 `REPLACE` 플래그로 **필터를 좁히는 것**은 이것뿐이다. 9-A 에서 "못 달게 만들기"가 꼭 필요해지면 그때 따로 한 단계로 떼어 본다.

## 되돌리기

- 묶음 단계: `doc/install-records/installed-stage9-<단계>-*.txt` 의 파일만 삭제. 묶음 안의 한 모드만 빼려면 그 모드의 기록만 쓴다 — 묶음도 모드별로 `-Tag` 를 따로 주는 이유다.
- 결속 단계: 플레이 전이면 파일 삭제로 끝난다. 플레이 후면 `backup/profiles-before-stage9-*/` 복원.
- 9-A: `config.jsonc` 를 지우고 모드 폴더를 지우면 전역 DB 수정도 함께 사라진다(서버 시작마다 다시 적용하는 방식이므로 영구 변경이 아니다).

## 아직 확정되지 않은 것

- 각 단계의 **플러그인/서버 모드 수**는 받아서 열어 봐야 안다. 지금 표에는 모드 개수만 있고, 한 모드가 플러그인 여러 개일 수 있다.
- 파일 겹침은 하나도 검사하지 않았다. 9-M 의 Labs Office Backport(2937)와 설치된 WTT Content Backport 계열이 특히 의심스럽다.
- 9-M 의 DynamicMaps Extended 가 교체인지 확장인지.
- Easy Slot Filters 의 로드 순위가 설치본의 DB 수정 모드들보다 앞인지 뒤인지 실측하지 않았다.
