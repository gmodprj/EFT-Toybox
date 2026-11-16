# 인게임 메뉴 한글화 점검 — F6·F7·F8·F10·F12 (2026-11-17)

게임 안에서 단축키로 여는 메뉴와 각 모드의 메뉴 구성이 한국어로 나오는지, 같은 말을 다르게 옮기지 않았는지,
잘못 옮긴 것이 없는지 점검하고 고친 기록이다. 앞선 점검은 F12 설정 항목(5,184개)과 서버 웹 화면이었다
([REVIEW-KOREAN-LOCALIZATION.md](REVIEW-KOREAN-LOCALIZATION.md), [AUDIT-WEB-UI-KOREAN-2026-11-16.md](AUDIT-WEB-UI-KOREAN-2026-11-16.md)).

> 이 문서는 **점검(§1~§3) → 수정 계획(§4) → 진행 기록(§5)** 순이다. 진행 기록은 고칠 때마다 덧붙인다.

## 1. 어느 키가 무엇을 여나 (설정 파일 실측)

`E:\Spt\BepInEx\config\*.cfg` 에서 F6~F12 에 묶인 키를 모두 뽑았다.

| 키 | 모드 | 설정 | 메뉴인가 |
| --- | --- | --- | --- |
| **F6** | SAIN | `Open Editor Shortcut` | **메뉴** — SAIN 편집기(IMGUI) |
| F6 | Field Attachments | `ArmSelection` | 동작 키(진단) |
| **F7** | Breakout Outlines | `Toggle Outline Key` | 동작 키 — 외곽선 켜고 끄기. **메뉴 없음** |
| F7 | Field Attachments | `AttemptEmptySlotInstall` | 동작 키(실험 기능) |
| **F8** | Scope Rangefinder | `ToggleEditor` | **메뉴** — 조준경 표시 편집기(IMGUI) |
| **F8** | Field Attachments | `OpenOverlay` | **메뉴** — 구형 부착물 화면(uGUI). 새 Alt 메뉴와 같은 오버레이를 쓴다 |
| F8+Ctrl | ORBIT | `Capture door shortcut` | 동작 키(문 좌표 캡처) |
| **F10** | ImpactCue | `Standalone Settings Hotkey` | **메뉴** — ImpactCue 설정 창(자체 locale) |
| F10 | Field Attachments | `ExportReport` | 동작 키(진단 보고서) |
| F10 | Prestige Perks | `Mark key` | 동작 키(개발자 모드 전용 스크린숏) |
| **F12** | ConfigurationManager → **Better BepInEx** | `Show config manager` | **메뉴** — 모드 설정 창 |

**F12 창은 지금 ConfigurationManager 가 아니라 Better BepInEx(1.0.4) 의 새 메뉴가 그린다**
(`Use the new menu = true`). 기동 로그에 Better BepInEx 가 원래 창의 트랜스파일러를 떼어 냈다는 줄이 남는다:

```
[Info :BetterBepInEx] Took SptKoreanConfig.UiStringPatch.Transpiler (spt.koreanconfig) off the stock window's OnGUI; this menu replaces that window.
```

즉 `lang/_ui.json`(원래 창의 버튼·안내문 33개)은 **화면에 쓰이지 않는다.** 설정 항목 번역은 Better BepInEx 의
`StockManager.CollectSettings` 에 건 후처리로 들어가지만, **새 메뉴 자체의 글자(제목·버튼·검색·안내문)는 아무도 바꾸지 않고 있었다.**

## 2. 메뉴별 상태

| 메뉴 | 글자가 어디 있나 | 지금 한글화 | 남은 것 |
| --- | --- | --- | --- |
| F12 설정 항목 | 각 모드의 `ConfigEntry` | 사전 187개, 5,184/5,184 (`check-lang-coverage`) | **Prestige Perks 사전 없음(11항목)** · 런타임 "사전에 없음 48" · 용어 불일치(§3) |
| F12 창 틀 | `BetterBepInEx.dll` 리터럴 | **없음** | 제목·버튼·검색·안내 약 120개, 선택지 이름(글꼴·테마·정렬) 18개 |
| F6 SAIN 설정 | `[Name]`·`[Description]`·`[Category]` 속성 | 757/757 (사전 791) | 없음 |
| F6 SAIN 창 틀 | `SAIN.dll` 리터럴 | 36개 | **프리셋 화면·검색·저장·삭제 등 약 45개 영문** |
| F8 Scope Rangefinder | `maschine-ScopeRangefinder.dll` 리터럴 | **없음** | 편집기 제목·버튼·항목·안내 약 85개 |
| F8 Field Attachments | `Tylevo.FieldAttachments.dll` 리터럴 | **없음** | 오버레이 제목·조작 안내·비교표·상태 문구 약 40개 |
| F10 ImpactCue | 모드의 `locales/ko.json` | 1,302열쇠 중 1,259 | **43열쇠 영문**(조준점·방어구 상태·스태미나 HUD 분류 등) · 용어가 F12 사전과 다름 |
| F7 | — | — | 메뉴 없음 |

확인 방법:

- SAIN 속성: 디컴파일 소스의 `[Name("…")]` 등 757개를 `_sain.json` 열쇠와 대조(빠진 것 0).
- 창 리터럴: `ilspycmd -il` 로 뽑은 `ldstr` 를 메서드와 함께 모아 그리는 메서드(OnGUI·Draw*·Render·Build*Ui 등)만 골라 사전과 대조.
- ImpactCue: `ko.json` 과 `en.json` 의 열쇠·값 비교(값이 영문과 똑같은 것 43).
- Prestige Perks 의 자체 페이지(`Locales/kr.json`)는 164/164 다 있다 — 문제는 F12 쪽뿐.

## 3. 같은 말 다른 번역 · 오역 (F12 사전 187개 + ImpactCue)

F12 사전의 이름·섹션 7,000여 줄을 영문 낱말별로 묶어 한국어 표기 분포를 셌다(`variants.mjs`, 점검용 스크립트).
**게임 공식 kr 표기**(SPT `locales/global/kr.json`)를 기준으로 삼는다 — 창고(Stash)·플리마켓·스캐브·보스·
스태미나·체력·인체 공학·반동·내구도·Transit(공식도 영문).

### 3-1. 오역 — 뜻이 다르다

| 모드 | 원문 | 지금 | 고칠 말 | 까닭 |
| --- | --- | --- | --- | --- |
| ImpactCue | Armor Hit Icon Opacity | 아머 히트 아이콘 **투명도** | 방어구 명중 아이콘 불투명도 | Opacity 는 불투명도. 값이 클수록 진하다 |
| ImpactCue | Blocked Armor Icon Enabled | **방어된** 방어구 아이콘 표시 | 방어구 비관통 아이콘 표시 | 같은 Blocked 를 색상 항목은 "비관통"으로 옮겼다 |
| ImpactCue · GamePanelHUD · ISB · SAIN | Energy / Stamina | **기력** (두 뜻) | Energy → 에너지, Stamina → 스태미나 | 한 낱말이 모드마다 다른 뜻이었다. 공식 kr 은 에너지·스태미나 |

**처음에 오역으로 적었다가 거둔 것** (실측으로 정정):

- ImpactCue `Armor Hit Icon Color` → "방어구 관통 아이콘 색상": 열쇠만 보고 오역이라 했는데, 모드가 화면에 쓰는 영문 이름이
  **"Penetrated Armor Icon Color"** 다. 관통이 맞다.
- Dynamic Maps `BTR marker color` → "BTR": 같은 섹션("6. 표시 색상")의 다른 항목도 모두 대상 이름만 적는 관례다(스캐브·보스…). 둔다.
- Trauma Core `Scav Hitboxes/Systems` → "봇 …": 설명이 AI 전체에 걸리는 설정이라 "봇"이 뜻에 맞다. 둔다.

### 3-2. 남은 영문 — 공식 한국어가 있다

| 모드 | 지금 | 고칠 말 |
| --- | --- | --- |
| ImpactCue | Boss 피해자 색상 · Boss 아이콘 파일 | 보스 … |
| ImpactCue | Scav 아이콘 파일 · SCAV 피해자 이름 색상 | 스캐브 … |
| ImpactCue | Health HUD 표시 · Health Bar 표시 | 체력 HUD 표시 · 체력 막대 표시 |
| ImpactCue | Follower·Guard·Rogue·Raider·Cultist 피해자 색상 | 경호원·로그·레이더·교단원 (퀘스트 목표 사전과 같게) |

`Transit`(공식도 영문)·탄약/수류탄 이름·`Arc`·`Focus Rail`(나침반 표시 방식 이름)과 ImpactCue 의 기능 이름
(`Compact HUD`·`Combat Strip`·`Tactical Glass` — 모드 번역자가 F10 에서 일부러 영문으로 둔 것)은 고유명으로 둔다.

### 3-3. 같은 말 다른 번역

| 원문 | 갈린 표기 | 정할 말 | 범위 |
| --- | --- | --- | --- |
| Stash | 창고 4 · 보관함 2 · **스태시** 1 | **창고**(공식) | `flir.iof` 스태시 → 창고. "Hidden stash"(은닉처)·"task stash"(퀘스트 보관함)는 다른 물건이라 둔다 |
| Stamina | 스태미나 10 · **스태미너** 2 · 기력 2 · **기운** 2 | **스태미나**(공식) | Infinite Everything·Quick Aim·Combat Augmentation·Smajlec Stamina 의 이름 |
| Volume | 볼륨 9 · 소리 크기 9 · 음량 5 · 그 밖("…크기" 등) 28 | **음량** | 이름에 "볼륨"이 든 것(ImpactCue)만 음량으로. "소리 크기"는 뜻이 같아 그대로 둔다 |
| Kill | 킬 32 · 처치 29 · 사살 2 | **처치** | ImpactCue 한 모드 안에서 섹션은 "처치 기록", 항목은 "킬 피드"로 갈려 있다 → 처치로 통일 |
| Armor (Hit) | 방어구 · **아머** 6 | **방어구** | ImpactCue "아머 히트 아이콘 …" 6항목 |
| Hit (sound/icon) | 명중 · **히트** | **명중** | ImpactCue |
| Sound | 소리 · **사운드** | **소리** | ImpactCue 이름(활성화·미리듣기) |
| Ergonomics | 조작성 1 | **인체 공학**(공식) | Broken Limbs 는 이미 다르게 적었다 → 둘 다 공식 표기로 |

**ImpactCue 는 F10 과 F12 가 같은 말을 써야 한다.** F10 은 모드의 `ko.json`(모드 제작자 번역: 킬·사운드·볼륨·아머 히트)을,
F12 는 우리 사전(처치 기록·소리)을 읽어 한 모드가 두 화면에서 다른 말을 쓴다. 모드 파일은 덮지 않고
**`locales/ko-KR.json` 을 새로 넣는다** — ImpactCue 는 `Language = Auto` 일 때 `CultureInfo.CurrentUICulture.Name`(ko-KR)
파일을 `ko` 보다 먼저 찾는다(디컴파일 `LocalizationManager.ResolveLanguageCode` 실측). 원본 `ko.json` 은 그대로 남는다.

### 3-4. 둘 것 (점검했고 고치지 않는다)

- 문장 끝맺음: 설명은 "~다", 사용자에게 고르게 하는 안내는 "~세요"로 이미 갈라 써 왔다(SAIN 창 포함). 섞인 것이 아니라 규칙이다.
- `Debug` → 디버그 19 · 진단 44: 진단은 "Diagnostics" 섹션과 디버그 표시 항목을 아우르는 말로 써 왔고 뜻이 어긋나지 않는다.
- 장비 이름이 그대로 든 항목(No Barrel Shadow 의 손전등 300여 줄, NVG Masks, 수류탄·탄약 이름): 공식 kr 도 영문이다.

## 4. 수정 계획 (순서대로)

| # | 메뉴 | 할 일 | 고치는 자리 |
| --- | --- | --- | --- |
| 1 | 공통 | **메서드로 범위를 좁히는 DLL 문자열 패치**를 만든다 | `src/SptKoreanConfig/DllStringPatch.cs`, `lang/_dll-*.json` |
| 2 | F12 | Better BepInEx 창 틀·선택지 이름 | `lang/_dll-betterbepinex.json` |
| 3 | F12 | Prestige Perks 사전 11항목 | `lang/com.viper.prestigeperks.json` |
| 4 | F12 | 런타임 "사전에 없음" 항목을 로그에 열거(1회) | `Plugin.cs` |
| 5 | F12 | §3 오역·잔여 영문·용어 통일 | 해당 사전들 |
| 6 | F6 | SAIN 남은 창 글자 | `lang/_dll-sain.json` |
| 7 | F8 | Scope Rangefinder 편집기 | `lang/_dll-scoperangefinder.json` |
| 8 | F8 | Field Attachments 오버레이 | `lang/_dll-fieldattachments.json` |
| 9 | F10 | ImpactCue 43열쇠 + 용어 통일 | `src/PluginLocales/ImpactCue/ko-KR.json` |
| 10 | 공통 | 빌드·설치·점검 도구·게임 로그로 확인 | `check-dll-strings.mjs`(새) |

### 왜 새 패치가 필요한가 (#1)

기존 두 패치(`UiStringPatch`·`SainUiStringPatch`)는 **어셈블리 전체**에서 사전의 문자열과 같은 `ldstr` 를 모두 바꾼다.
긴 문장은 괜찮지만 `Name`·`Clear`·`Scale`·`Anchor` 같은 짧은 말은 화면 밖에서 열쇠로도 쓰인다 — 예를 들어 Scope Rangefinder 는
`"OffsetX"`·`"Scale"`·`"Anchor"` 를 편집기(`DrawLayoutEditorWindow`)에서는 라벨로, 프리셋 변환(`ConvertShippedPresetsToInferredAnchors`)에서는
JSON 열쇠로 쓴다. 그래서 새 패치는 **사전마다 바꿀 메서드를 적고, 그 메서드 안의 리터럴만** 바꾼다.

트랜스파일러는 패치할 때 한 번 IL 을 고칠 뿐 매 프레임 비용이 없다 — "그리는 메서드에 패치를 걸지 않는다"는 규칙은
매 프레임 도는 후처리(postfix) 이야기라 여기에 해당하지 않는다.

## 5. 진행 기록

### 5-1. 고친 것 (계획 #1~#9)

| # | 메뉴 | 결과 | 확인 |
| --- | --- | --- | --- |
| 1 | 공통 | `DllStringPatch` — `lang/_dll-*.json` 이 고른 메서드 안의 리터럴만 바꾼다. `methodStrings`(메서드마다 어순), `excludeMethods`, 이미 초기화된 정적 필드 다시 쓰기(5-3) | `node tools/check-dll-strings.mjs` (게임 없이 IL 대조, `--list`·`--where`) |
| 2 | F12 | `_dll-betterbepinex.json` **117항목** — 제목·검색·사이드바·상태줄·초기화 확인·스크린숏 문구 | 117/117 |
| 3 | F12 | `com.viper.prestigeperks.json` 11항목 | `check-lang-coverage` **5195/5195** |
| 4 | F12 | 런타임 "사전에 없음" 항목을 `GUID \| 섹션 \| 키` 로 로그에 한 번씩 적는다 | 다음 기동 로그에서 본다 |
| 5 | F12 | 기력 두 뜻 분리, 스태미너·기운→스태미나, 스태시→창고, 조작성(ergo)→인체 공학, 호위→경호원. 받침 바뀐 곳 조사 고침 | 용어 분포 다시 셈: Stamina 16/16 스태미나, Stash 창고 5(나머지 2는 은닉처·퀘스트 보관함), Boss 25/25 |
| 6 | F6 | `_dll-sain.json` 68항목(프리셋 화면·검색·저장·삭제 확인·탭) + `_sain-ui.json` 성향 설명 4·경고 1 | 68/68 |
| 7 | F8 | `_dll-scoperangefinder.json` 90항목 — F12 사전 용어와 같게 | 90/90 |
| 8 | F8 | `_dll-fieldattachments.json` 150항목 — 공식 kr 부착물 칸 이름 | 150/150 |
| 9 | F10 | `locales/ko-KR.json` 1,309열쇠 — 미번역 43 채움, 용어 227곳, 설명 속 영문 설정 이름 12곳. F12 ImpactCue 사전 이름 80·설명 120·섹션 19 를 같은 값으로 | `check-plugin-locales` OK, `gen-impactcue-ko --check` 두 번째 실행 0건(멱등) |

### 5-2. 위험해서 범위를 좁힌 것

`--where` 로 공통 항목이 여러 메서드에 걸친 곳을 보니, 화면 글자와 같은 리터럴을 **열쇠로 쓰는 메서드**가 섞여 있었다.
어셈블리 전체를 바꾸는 예전 방식이었다면 그 기능이 깨졌다.

| 모드 | 글자 | 열쇠로 쓰는 곳 | 조치 |
| --- | --- | --- | --- |
| Field Attachments | `Ergonomics`·`Weight` | `WeaponComparisonReader` 가 리플렉션 속성 이름으로 | 비교 패널(`BuildComparisonUi`)에서만 |
| Field Attachments | `None` | 열거형 이름 | 쓸 수 없는 부품 팝업에서만 |
| Field Attachments | `POSE FAULT` | `Plugin` 이 `StartsWith` 로 비교 | 넣지 않음 |
| Scope Rangefinder | `OffsetX`·`Scale`·`Anchor` | 프리셋 변환의 JSON 열쇠 | 편집기 메서드에서만 |
| SAIN | `Search`·`Name` | 전투 레이어 이름·BigBrain 레이어 찾기 | 편집기 메서드에서만 |
| SAIN | `Enter Name Here` 등 | 그대로 새 프리셋 이름이 된다 | 넣지 않음 |
| Better BepInEx | `HIDDEN BY THE MOD'S AUTHOR` | 다른 메서드가 같은 글자로 섹션을 비교 | 넣지 않음 |

### 5-3. 게임을 켜서 본 것 (08:13 기동, 0.3.0 첫 판)

기동 로그(`LogOutput.log` 08:13):

```
[Info   :ImpactCue-UI-MOD] ImpactCue locale: ko-KR (…\ImpactCue-UI-MOD\locales)
[Warning:F12 설정 한글화]   BetterBepInEx.MenuWindow::DrawRow 패치 실패 — IL Compile Error (unknown location)
[Info   :F12 설정 한글화] _dll-betterbepinex.json — BetterBepInEx: 사전 120개, 메서드 32개, 쓰인 항목 120개
[Info   :F12 설정 한글화] _dll-fieldattachments.json … 쓰인 항목 150개 / _dll-sain.json … 64개 / _dll-scoperangefinder.json … 90개
```

화면(스크린숏으로 직접 봄):

| 화면 | 한국어로 나온 것 | 영문으로 남은 것 |
| --- | --- | --- |
| 메인 메뉴 | Field Attachments 안내("STANCE OVERHAUL 없음"·"Forge 에서 보기"·"넘기기") — uGUI 기본 글꼴로도 한글이 그려진다 | — |
| F12 | 바뀐 설정·단축키·모드 189·전체·숨긴 모드 보기·모드 초기화·상태줄(모드/설정/바뀜) | **Mod Settings·Show advanced·Undo·Redo·Filter mods·Add to favorites·Screenshot·Search this mod·All** |
| F10 | 처치 기록·전술 HUD·명중 소리·처치 소리·마스터 음량·육체 명중 음량 | 기능 고유명(의도) |
| F6 | 편집기 제목·고급 설정: [끔]·찾기·지우기·저장하고 내보내기·설정 분류 | **Home·Bot Settings·Personalities·Equipment Stealth·Advanced Options·Save All Changes** |

**보고 두 가지가 거짓말을 했다.**

1. **`DrawRow` 는 패치가 실패했는데 그 안의 항목을 "쓰였다"고 셌다.** 트랜스파일러는 돌았고 다시 컴파일에서 떨어졌다.
   → 패치가 끝까지 성공한 뒤에만 "쓰임"으로 확정하게 고쳤다. `DrawRow` 는 Harmony 가 다시 컴파일하지 못하므로
   `excludeMethods` 로 빼고 그 안의 세 글자(초기화 버튼·"기본값으로" 도움말·그리기 실패 문구)는 원문으로 둔다.
2. **영문으로 남은 것은 모두 정적 생성자(`.cctor`)의 글자였다.** Better BepInEx·SAIN 이 우리보다 먼저 올라와 정적 필드가
   이미 초기화된 뒤라, IL 을 고쳐도 화면이 바뀌지 않는다. 그런데 로그는 "사용됨"이라 했다 — **이전 세션부터 `_sain-ui.json` 의
   "Bot Settings" 도 같은 이유로 한 번도 한국어로 나온 적이 없다.**
   → 패치 뒤에 그 타입의 정적 필드(문자열·문자열 배열·GUIContent, 컬렉션 안 객체의 문자열 필드 한 단계)를 사전대로 다시 쓰고,
   `.cctor` 항목은 **실제로 필드 값을 바꿨거나 값이 이미 한글일 때만** "쓰임"으로 센다.

교훈은 이 프로젝트가 이미 적어 둔 것과 같다 — "덮개 100%는 화면이 한글이라는 뜻이 아니다". 이번에는 **"사전 항목이 쓰였다"도 화면이
아니다**: 정적 생성자 글자는 패치 시점보다 먼저 굳는다.

### 5-4. 남은 것

| 무엇 | 왜 |
| --- | --- |
| 5-3 의 고친 판(정적 필드 다시 쓰기·DrawRow 제외)을 **게임에 아직 넣지 못했다** | 게임이 켜져 있어 DLL 이 잠겨 있다. 게임을 끄고 `tools/install-koreanconfig.ps1` → 다시 켜서 F12·F6 를 본다 |
| F8 두 편집기를 화면으로 보지 않았다 | Scope Rangefinder 편집기·부착물 메뉴는 레이드 안에서 열린다 |
| 런타임 "사전에 없음" 목록 | F12 를 연 뒤 로그에 찍힌다. 새 판에서 본다 |
| Better BepInEx 자기 설정의 선택지 이름(글꼴·테마·정렬 18개) | `[Description]` 속성이라 리터럴 바꾸기로 안 된다 |
| Scope Rangefinder 열거형 값(Meters·MOA …) | `ToString()` 이라 리터럴 바꾸기로 안 된다 |
| Better BepInEx 창 안 "Nothing to undo" 첫 표시 | 인스턴스 생성자 글자. 첫 되돌리기 뒤로는 바뀐 글자가 나온다 |
