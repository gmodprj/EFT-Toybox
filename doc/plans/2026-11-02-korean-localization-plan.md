# 한글화 — 구현 계획: STM 과 SAIN

> **에이전트 작업자에게:** 필수 하위 스킬 — `superpowers:subagent-driven-development`(권장) 또는 `superpowers:executing-plans`로 Task 단위로 구현한다. 단계는 체크박스(`- [ ]`)로 추적한다.

**Goal:** STM 의 서버 문자열을 한국어로 내보내고, SAIN 의 F6 편집기 창을 한국어로 읽을 수 있게 한다 — 두 모드의 원본 파일을 고치지 않고.

**Architecture:** 두 대상이 **서로 다른 문제**이고, 쉬운 쪽과 모르는 쪽이 갈린다. STM 은 우리 포크라 **언어 파일을 하나 더 놓으면 끝난다**(`ko.json`) — 기존 서비스가 locale 별로 파일을 골라 읽고 빠진 키는 영어로 메우므로, 부분 번역도 안전하고 점진적으로 채울 수 있다. SAIN 은 남의 모드고 번역 대상 글자가 **DLL 안에** 있다 — 그래서 이 계획은 SAIN 에 대해 **먼저 조사 Task 를 두고**, 그 실측으로 접근 방법이 정해진 뒤에 번역한다. 조사 없이 "Harmony 로 UI 문자열을 갈아치운다" 를 전제하지 않는다.

**Tech Stack:** JSON 언어 파일(STM 서버, `net10.0` 이 읽는다). SAIN 쪽은 조사 결과가 정한다 — 사전 JSON 만으로 되면 기존 `src/SptKoreanConfig`(net472, BepInEx + ConfigurationManager)에 얹고, UI 패치가 필요하면 같은 플러그인에 패치를 더한다.

**Spec:** 없다. 이 계획의 근거는 아래 **실측**이고, 각 Task 가 그것을 다시 확인한다. 기존 선례는 `src/SptKoreanConfig/README.md`(F12 창 한글화)와 `doc/SERVER-WEB-KOREAN.md`(서버 웹 한글화)다.

### 실측 (2026-11-02, 이 계획의 전제)

- **STM 언어 파일은 `en`·`chs`·`ru` 셋이고 `ko` 가 없다.** `src/SptTeamManager/server/Resources/lang/`.
- `FriendlyLanguageService` 가 세션 locale 로 `lang/<locale>.json` 을 고르고, `NormalizeLocale` 은 `ko` 를 그대로 통과시킨다(`zh`·`ch` → `chs`, `de` → `ge` 만 바꾼다). 즉 **파일명은 `ko.json`** 이다.
- 같은 서비스가 `MergeMissingValues` 로 **빠진 키를 영어로 메운다.** `chs.json` 은 최상위 키가 76개뿐인데 `en.json` 은 97개다 — 부분 번역이 이미 정상 동작이다.
- `en.json` 의 번역 대상은 최상위 키 **97개**, 그 안의 중첩 블록 76개를 펴면 문자열 **약 344개**(대부분 `{ "Name": ..., "Description": ... }` 짝).
- **SAIN 의 F12 설정은 2개뿐이고 이미 한국어다.** `src/SptKoreanConfig/lang/me.sol.sain.json` 에 `Open Editor`·`Open Editor Shortcut` 이 번역돼 있고, `BepInEx/config/me.sol.sain.cfg` 에도 그 둘뿐이다. **F12 쪽은 할 일이 없다.**
- SAIN 의 실제 설정은 **F6 "SAIN Editor"** 창에 있다. 설치 폴더에는 `SAIN.dll`·`SAIN.Preset.Shared.dll`·`EditorDefaults.json` 뿐이고 **언어 자산이 없다** — 창의 글자는 DLL 안에 있다.

## Global Constraints

- **남의 모드 파일을 고치지 않는다.** 모드 업데이트에 날아가고, 설치 기록과도 어긋난다(`doc/PROCEDURE.md`). 번역은 전부 **우리 쪽 파일**에 둔다.
- **설정 값과 `.cfg` 파일을 건드리지 않는다.** 보이는 글자만 바꾼다(`SptKoreanConfig` 의 기존 계약: "다른 모드의 파일은 건드리지 않고, 설정 값과 .cfg 파일도 그대로 둔다").
- **끌 수 있어야 한다.** 한글 표시는 토글(`SptKoreanConfig` 의 `한글로 표시`)로 끄면 원문으로 돌아간다. 새로 더하는 번역도 그 토글을 따른다.
- **STM 언어 파일은 `{정수}.json` 이 아니다**(저장 계층이 파일명 모양으로 종류를 가른다). `ko.json` 은 이 제약과 무관한 `Resources/lang/` 아래다.
- 기존 `en.json` 의 **키·구조·배열 길이를 바꾸지 않는다.** `ko.json` 은 같은 키에 한국어 값만 넣는다. 구조가 어긋나면 `IsCompatibleLanguageNode` 가 영어로 되돌린다.
- 번역은 **이 레포 문서의 말투**를 따른다 — 기계 번역체를 쓰지 않고, 이미 번역된 `me.sol.sain.json`·`SptKoreanConfig/lang/*` 의 어휘와 종결어미를 맞춘다.
- `downloads/`, `extracted/`, `backup/` 은 커밋하지 않는다.

## Review Focus

- **구조가 어긋난 `ko.json`.** 배열 길이가 다르거나 `{Name, Description}` 중 하나가 빠지면, 그 키만 영어로 돌아가는 대신 **화면이 비거나 예외가 날** 수 있다. → Task 1 이 구조 대조를 **기계로** 한다(눈으로 보지 않는다).
- **번역했는데 안 보인다.** locale 이 `ko` 로 오지 않으면(`ko-KR`·`kr` 등) 파일이 선택되지 않는다. `NormalizeLocale` 은 `-`·`_` 앞만 자르므로 `ko-KR` → `ko` 는 되지만 `kr` 은 `kr` 로 남는다. → Task 1 Step 5 가 **게임에서 실제로 오는 locale 값**을 로그로 확인한다.
- **SAIN 창을 못 고친다.** 글자가 DLL 안이고 UI 생성 방식이 미지다. 조사 결과가 "지금 방법으로는 안 된다" 일 수 있다. → Task 3 은 **판정으로 끝낼 권리**를 갖는다(안 되면 그 사실과 근거를 적고 멈춘다). 추측으로 패치를 쓰지 않는다.
- **SAIN 번역이 프리셋을 깬다.** SAIN 은 프리셋을 이름으로 저장·불러온다(`EditorDefaults.json`, `SAIN.Preset.Shared.dll`). 표시용 글자를 바꾸는 것이 **저장되는 이름**을 바꾸면 프리셋이 깨진다. → Task 4 는 표시 계층만 건드리고, Task 5 관문이 프리셋 저장·불러오기를 실제로 확인한다.
- **번역이 설정을 잘못 설명한다.** 틀린 설명은 영어보다 나쁘다 — 사용자가 그 말을 믿고 값을 바꾼다. → Task 2·4 는 **원문 뜻이 불확실한 항목을 번역하지 않고 남긴다**(영어로 떨어진다). 지어내지 않는다.

---

### Task 1: STM `ko.json` 뼈대와 기계 대조

번역문을 쓰기 전에 **틀을 먼저 세운다.** 구조 오류는 눈으로 찾는 종류가 아니다.

**Files:**
- Create: `src/SptTeamManager/server/Resources/lang/ko.json`
- Create: `tools/check-stm-locale.mjs`

**Interfaces:**
- Produces: `node tools/check-stm-locale.mjs` — `en.json` 을 기준으로 `ko.json`(그리고 `chs`·`ru`)의 구조를 대조한다. 종료 코드 0 = 호환, 1 = 어긋남. 보고: 어긋난 키, 번역된 문자열 수 / 전체.

- [ ] **Step 1: 대조 스크립트를 쓴다.** 검사 항목: ① `ko` 의 모든 키가 `en` 에 있는가(없는 키는 죽은 번역) ② 값의 종류가 같은가(문자열/객체/배열) ③ 배열이면 길이가 같은가 ④ 객체면 `en` 의 하위 키를 다 갖는가. **`en` 에 있고 `ko` 에 없는 키는 오류가 아니다**(영어로 메워진다) — 진행률로 보고한다.
- [ ] **Step 2: 스크립트가 지금 상태를 바르게 보고하는지 확인한다** — Run: `node tools/check-stm-locale.mjs` / Expected: `ko.json` 이 없다고 말하고, `chs` 의 진행률(76/97 수준)을 보고한다. 종료 코드 0.
- [ ] **Step 3: 일부러 어긋난 `ko.json` 으로 실패를 본다.** 배열 길이를 하나 줄인 `tacticOptions` 를 넣고 Run / Expected: 종료 코드 1, 그 키를 지목한다. **확인한 뒤 되돌린다.**
- [ ] **Step 4: 빈 `ko.json`(`{}`)을 놓고 서버를 켠다** — Expected: 모든 문자열이 영어로 뜨고 오류 0건. 이것이 "부분 번역이 안전하다" 의 실증이다.
- [ ] **Step 5: 게임에서 오는 locale 값을 확인한다.** `FriendlyLanguageService` 에 들어오는 `requestedLocale` 을 한 줄 로그로 남겨 **실제 값**을 본다(`ko`·`ko-KR`·그 밖). `ko` 로 정규화되지 않으면 **`NormalizeLocale` 에 그 매핑을 더한다**(`zh`→`chs` 선례와 같은 자리).
- [ ] **Step 6: 커밋**

---

### Task 2: STM 문자열 번역

**Files:**
- Modify: `src/SptTeamManager/server/Resources/lang/ko.json`

- [ ] **Step 1: 사용자가 가장 많이 보는 블록부터 번역한다.** 순서: ① 설정 화면의 섹션 이름 9개(`baseSettings`·`followSettings`·`combatSettings`·`inputSettings`·`miscSettings`·`testSettings`·`raidSettings`·`lootingSettings`·`loadoutManagementSettings`) ② 전술·장비 선택지(`tacticOptions`·`equipOptions` — **배열 길이를 유지한다**) ③ 명단·파티 화면 문자열 ④ 나머지 `{Name, Description}` 짝.
- [ ] **Step 2: 블록마다 대조를 돌린다** — Run: `node tools/check-stm-locale.mjs` / Expected: 종료 코드 0, 진행률이 늘어난다. **블록 단위로 커밋한다**(한 번에 344개를 커밋하지 않는다 — 되돌릴 단위가 커진다).
- [ ] **Step 3: 뜻이 불확실한 항목은 번역하지 않고 남긴다.** `testSettings` 아래 항목처럼 코드를 읽어야 뜻이 잡히는 것은 **건너뛰고 목록으로 보고한다.** 영어로 떨어지는 것이 틀린 한국어보다 낫다.
- [ ] **Step 4: 서버를 켜서 화면으로 확인한다** — Expected: 번역한 블록이 한국어로 뜨고, 건너뛴 항목은 영어, 오류 0건.
- [ ] **Step 5: 커밋**

---

### Task 3: SAIN 편집기 조사 — 고칠 수 있는지 판정한다

> **이 Task 는 판정으로 끝낼 수 있다.** "지금 방법으로는 안 된다" 도 결과다. 그 경우 근거를 적고 Task 4·5 를 범위에서 뺀다.

**Files:**
- Create: `doc/specs/<날짜>-sain-editor-localization-survey.md`

- [ ] **Step 1: F6 창의 글자가 어디서 오는지 확인한다.** `SAIN.dll` 을 역컴파일해 ① 라벨이 하드코딩 문자열인지, 속성(attribute)인지, 프리셋 JSON 에서 오는지 ② 창을 그리는 자리가 Unity `IMGUI`(`GUILayout`·`GUI.Label`)인지 `UnityEngine.UI` 인지 ③ 번역을 끼울 **단일 통로**가 있는지(예: 라벨을 한 번 지나는 헬퍼). 통로가 하나면 Harmony 로 그 하나만 잡는다.
- [ ] **Step 2: 저장되는 이름과 보이는 이름이 같은지 확인한다.** 같으면 표시만 바꾸는 패치가 **프리셋을 깨뜨릴 수 있다**(Review Focus 4번). 어디서 갈라지는지 코드로 집어 적는다.
- [ ] **Step 3: 번역 대상 수를 센다.** 라벨·설명이 몇 개인지. 수백 개면 Task 4 의 범위를 **자주 쓰는 섹션으로 제한**하고 그 판단을 적는다.
- [ ] **Step 4: 판정을 적는다.** `가능 / 제한적으로 가능 / 지금은 불가`와 그 근거, 그리고 가능하면 **어느 메서드를 어떤 모양으로 패치하는지** 한 단락. 이 문서가 Task 4 의 명세가 된다.
- [ ] **Step 5: 커밋**

---

### Task 4: SAIN 편집기 번역 (Task 3 이 "가능" 일 때만)

**Files:**
- Modify: `src/SptKoreanConfig/` (Task 3 이 정한 패치와 사전)
- Create: `src/SptKoreanConfig/lang/me.sol.sain.editor.json`

- [ ] **Step 1: 사전 파일 형식을 정한다.** 기존 `me.sol.sain.json`(`guid`·`name`·`sections`·`settings`)과 **같은 모양**을 쓸 수 있으면 그것을 쓰고, 편집기 구조가 다르면 별 파일로 둔다. `TranslationStore` 가 읽을 수 있어야 한다.
- [ ] **Step 2: 실패하는 테스트를 쓴다.** `SptKoreanConfig` 에 테스트 프로젝트가 없으면, **사전 조회만 순수 클래스로 빼서** 테스트한다: 키가 있으면 번역, 없으면 원문, 토글이 꺼지면 원문.
- [ ] **Step 3: 테스트가 실패하는지 확인한다** — Expected: FAIL
- [ ] **Step 4: 조회를 구현하고 패치를 연결한다.** Harmony prefix 면 **`bool __runOriginal` 을 받아 `false` 면 즉시 반환**한다. 번역이 없는 라벨은 **원문 그대로** 흘린다.
- [ ] **Step 5: 자주 쓰는 섹션을 번역한다.** Task 3 이 정한 범위. 뜻이 불확실한 항목은 남긴다.
- [ ] **Step 6: 테스트가 통과하는지 확인한다** — Expected: PASS
- [ ] **Step 7: 커밋**

---

### Task 5: 설치하고 인게임으로 확인한다

**Files:** 없음(확인만). 결과는 `doc/WORKLOG.md` 에 적는다.

> **사람만 할 수 있다.** STM 은 `tools/install-sptteammanager.ps1`(서버를 먼저 내린다), `SptKoreanConfig` 는 그 모드의 설치 경로에 맞춘다.

- [ ] **Step 1: 다음을 확인한다**

1. STM 설정·명단·파티 화면이 **한국어로** 뜬다. 번역하지 않은 항목은 **영어로** 뜨고 빈칸이 없다.
2. 서버 로그에 언어 파일 관련 오류·경고가 **0건**이다.
3. 게임 언어를 영어로 바꾸면 **영어로 돌아온다**(ko 파일이 다른 locale 을 오염시키지 않는다).
4. (Task 4 를 했으면) F6 편집기가 한국어로 뜨고, **값이 그대로**이며, **프리셋 저장·불러오기가 멀쩡**하다 — 프리셋을 하나 저장하고 다시 불러 값이 같은지 본다.
5. (Task 4 를 했으면) `한글로 표시` 를 끄면 **원문으로 돌아온다.**
6. SAIN 의 AI 동작이 **바뀌지 않았다**(표시만 건드렸다는 전제의 확인).

- [ ] **Step 2: 결과를 적는다.** 어긋나면 **여기서 멈추고 보고한다.**

---

### Task 6: 문서

**Files:**
- Modify: `doc/WORKLOG.md`, `doc/BACKLOG.md`, `src/SptKoreanConfig/README.md`, `src/SptTeamManager/README.md`

- [ ] **Step 1: 진행률과 남은 것을 적는다.** STM 은 번역한 블록과 건너뛴 항목 목록, SAIN 은 Task 3 의 판정과 번역 범위.
- [ ] **Step 2: BACKLOG 를 정리한다.** 번역을 미룬 항목을 올린다. **기존 "ISB 역할 이름 10개의 한국어 번역"** 항목과 같은 성격이므로, 이 계획이 세운 방식(우리 쪽 파일에만 둔다)이 그 항목에도 쓸 수 있는지 한 줄 남긴다.
- [ ] **Step 3: 커밋**

---

## 이 계획에 없는 것

- **ISB 역할 이름 10개의 번역**(BACKLOG 기존 항목). 같은 성격이지만 대상 모드가 다르고 서버 locale 덮어쓰기라는 또 다른 방식이 필요하다. Task 6 Step 2 가 이 계획의 방식을 거기에 쓸 수 있는지만 적는다.
- **STM 클라이언트 쪽 문자열.** 이 계획은 서버가 내보내는 문자열(`Resources/lang/`)만 다룬다. 클라이언트에 하드코딩된 글자가 있으면 **찾아서 BACKLOG 에 올리고** 범위 밖으로 둔다.
- **다른 모드의 F12 설정 번역.** `SptKoreanConfig/lang/` 에 사전이 100개 넘게 있고 각자 진행률이 다르다. 이 계획은 SAIN 만 다룬다.
- **기계 번역 일괄 적용.** 틀린 설명은 영어보다 나쁘다는 것이 이 계획의 전제다.
