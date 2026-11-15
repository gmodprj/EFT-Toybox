# 계획 — 프레스티지 모드 둘 설치 (3332 Prestige Perks · 2687 Sicc Case Fix)

- 날짜: 2026-11-15
- 상태: **계획만. 아직 아무것도 받지 않았고 설치하지 않았다.**
- 대상: `E:\Spt` (SPT 4.1.6, EFT 0.16.9.5.40743)
- 순서: **자체 모드 `PrestigeStashRelax`(`doc/specs/2026-11-15-prestige-stash-relax-design.md`)를
  먼저 만든다.** 이 설치는 그 뒤다 — 사용자가 그렇게 정했다.

## 1. 무엇을 넣나

| 묶음 | 모드 | 버전 | 크기 | 결속 | 의존 | Fika |
| --- | --- | --- | --- | --- | --- | --- |
| **P-A** | [2687 Sicc Case Fix](https://sp-mod.com/mod/2687) | 2.0.0 (`~4.1.3`) | 4.4KB | **결속** | - | 호환 |
| **P-B** | [3332 Prestige Perks](https://sp-mod.com/mod/3332) | 0.1.9 (`4.1.6`) | 4.8MB | - | 2540 (설치됨) | **비호환** |

Fika 비호환은 우리에게 문제가 아니다 — Fika 는 설치하지 않았다(기동 로그의 DirectReload+Fika
오류 1줄이 그 증거다).

- 2687: P3·P4 군번줄이 SICC 파우치에 안 들어가는 것만 고친다. 결속 표시가 붙어 있지만 4.4KB 이고
  하는 일이 아이템 필터 한 줄이다. 그래도 절차대로 **하나씩, 프로필 백업 뒤**에 넣는다.
- 3332: 프레스티지 요구치를 E** 1.0 수준으로 낮추고, 단계마다 퍽 슬롯 1칸(최대 6)을 준다.
  프레스티지 코인으로 퍽을 Tier I~III 로 올린다. 캐릭터 화면의 PRESTIGE 탭에 PERKS 메뉴가 붙는다.
  **공개 베타**이고, 제작자가 "정식판에서 퍽 데이터가 초기화될 수 있다"고 모드 페이지에 적어 두었다.

## 2. 왜 이 순서인가

2687 을 먼저 넣는다. 작고, 다른 모드와 겹칠 데가 없고, 결속이라 **혼자 넣고 혼자 확인해야** 한다.
3332 는 프레스티지 데이터와 화면을 함께 건드리므로 뒤에 두고, 앞의 것이 멀쩡한 것을 본 뒤에 넣는다.

## 3. 자체 모드와 부딪히는 곳 — 설계에 미리 반영한다

3332 는 **프레스티지 요구 조건**을 바꾼다. 우리 `PrestigeStashRelax` 는 같은
`prestige.Elements` 의 **`transferConfigs` 만** 건드리므로 바꾸는 필드가 겹치지 않는다. 그래도 둘
다 같은 테이블을 고치므로 **순서가 중요하다.**

- `PrestigeStashRelax` 는 원본 스냅샷을 **3332 가 자기 값을 넣은 뒤**에 떠야 한다. 설계 §5-3·§6-3
  의 측정에 **3332 설치 후 `Elements.Count` 재측정**을 더한다.
- 3332 가 `Elements` 를 **새 리스트로 갈아끼우면** 우리가 들고 있던 원본 참조가 떠 버린다. 그때는
  스냅샷을 id 기준(`PrestigeElement.Id`)으로 들고, 적용할 때마다 현재 리스트에서 id 로 찾는다.
  설계 §5-3 에 그 방식으로 적는다.
- 3332 가 **퍽으로 칸 수를 주는 종류의 퍽**을 갖고 있으면 우리 조정과 이중으로 더해질 수 있다.
  설치 후 퍽 목록을 읽어 확인한다.

## 4. 절차 — `doc/PROCEDURE.md` 그대로

### 4-1. 받기

`doc/download/manifest-prestige-pack.json` 을 쓴다.

```json
{ "mods": [
  { "group": "P-A", "id": 2687, "name": "Sicc Case Fix", "version": "2.0.0",
    "link": "https://sp-mod.com/mod/download/2687/sicc-case-fix/2.0.0" },
  { "group": "P-B", "id": 3332, "name": "Prestige Perks", "version": "0.1.9",
    "link": "https://sp-mod.com/mod/download/3332/prestige-perks/0.1.9" }
] }
```

`node tools/download-stage.mjs prestige-pack` → `downloads/prestige-pack/`.

전송이 기어가면 `tools/download-ranged.mjs` 로 끊어 받는다
(`one-stream-throttled-use-ranges`). 504 가 나면 `gh` 로 받는다
(`forge-download-may-504-use-gh`). 둘 다 5MB 이하라 느릴 일은 없어 보이지만, 호스트 쪽 사정은
재 봐야 안다.

해제는 **절대경로로** — `/c/Windows/System32/tar.exe -xf <파일> -C extracted/prestige-pack/<이름>`.

### 4-2. 겹침 검사

`node tools/check-installed-vs-extracted.mjs` 와 구조 확인. **덮어쓸 파일이 하나라도 있으면
멈춘다.** 3332 가 2540 의 폴더에 파일을 얹으려 들 수 있으니 특히 본다.

압축 뿌리에 섞여 온 README·CHANGELOG 는 치운다(그대로 넣으면 `E:\Spt\` 에 떨어진다).

### 4-3. 넣기

```powershell
E:\Spt_Mods\tools\install-and-test.ps1 -Stage prestige-pack -Pattern '2687__*' -Tag 'prestige-2687-SiccCase'
E:\Spt_Mods\tools\install-and-test.ps1 -Stage prestige-pack -Pattern '3332__*' -Tag 'prestige-3332-PrestigePerks'
```

- 그 전에 프로필 백업: `E:\Spt\SPT_Runtime\user\profiles\*` → `backup/profiles-before-prestige-pack/`
- **하나 넣고 멈춘다.** 두 번째는 첫 번째를 게임에서 확인한 뒤.
- 설치·테스트 전에 게임과 서버가 꺼져 있는지 본다. **내가 끄지 않는다** — 사용자에게 묻는다
  (`never-stop-server-or-game-unasked`).
- DLL 이 잠겨 있으면 "나중에 반영"이 아니라 **안 깔린 것**이다. 이름을 바꿔 치우고 넣는다
  (`locked-dll-means-not-installed`).

### 4-4. 기록·검사

1. `node tools/gen-installed-mods.mjs` → `doc/INSTALLED-MODS.md`
2. `node tools/check-install-records.mjs` → **`OK — 0/0`**
   - **이 둘과 상관없는 다른 모드가 걸릴 수 있다.** 모드가 들어오면 다른 모드의 설정 파일이 늘기
     때문이다(`install-records-can-misattribute`). 없는 파일이 나와도 유실로 단정하지 않는다 —
     해제본·`removed/`·중복 기록 셋을 먼저 본다.
3. `node tools/check-conflicts.mjs` 로 분류를 다시 뜬다.
4. 한글화: `node tools/check-plugin-locales.mjs` · `check-lang-coverage.mjs` 로 새 항목을 센다.
   **사전은 모드가 들어오면 가만히 있어도 구멍이 난다**(`lang-dicts-leak-as-mods-arrive`).
   3332 가 자기 한국어를 들고 있는지 먼저 센다(`mod-may-ship-own-korean`).

### 4-5. 확인

| 무엇 | 어떻게 |
| --- | --- |
| 서버 모드 수 | 기동 로그 `ModLoader: 총 N 개`. 3332 가 서버 모드를 싣는지로 175 → 176 을 기대 |
| 게임 플러그인 수 | `Loading [` 줄 수. **넣은 DLL 수와 같지 않다** — 라이브러리는 0 으로 센다(`plugin-count-is-not-dll-count`) |
| 오류 | `Could not add` 고유 id 수를 **날짜와 함께** 적는다. 줄 수로 보지 않는다 |
| 2687 | SICC 파우치에 P3·P4 군번줄을 넣어 본다 |
| 3332 | 캐릭터 화면 → PRESTIGE 탭 → PERKS 가 뜨는지. 요구치가 낮아졌는지 |

3332 의 퍽 화면은 **화면으로만 확인된다** — 로그에 안 남는 종류다. 스크린샷을 남긴다.

### 4-6. 되돌리기

- 2687 은 **결속**이다. 플레이 후 빼면 프로필이 깨질 수 있다. `backup/profiles-before-prestige-pack/`
  가 그 전 지점이다.
- 3332 는 결속이 아니지만 **프로필에 퍽 데이터를 쓴다.** 빼면 그 데이터가 뜬다. 베타라 정식판에서
  초기화될 수 있다는 점과 함께, 빼기 전에 프로필을 다시 뜬다.
- 제거 기록은 `doc/install-records/removed/` 로 옮긴다.

## 5. 넣지 않는 것

| 모드 | 왜 |
| --- | --- |
| [2716 The balance of gaining prestige](https://sp-mod.com/mod/2716) | 4.1.6 호환판이 **없다**(최신 1.0.2 `~4.0.0`). 제작자도 2540 이 같은 원칙으로 요구치를 갱신했다고 적어 사실상 흡수됐다 |
| [3265 GigaPMC](https://sp-mod.com/mod/3265) | 레벨·프레스티지로 PMC 최대 체력을 올리는 밸런스 모드다. 프레스티지 구성과 상관이 없다 |

## 6. 함께 보는 문서

- 자체 모드 설계: `doc/specs/2026-11-15-prestige-stash-relax-design.md`
- 절차: `doc/PROCEDURE.md`
- 업데이트 절차의 U1~U8: `doc/UPDATE-CHECK-2026-11-09.md` §3-1
