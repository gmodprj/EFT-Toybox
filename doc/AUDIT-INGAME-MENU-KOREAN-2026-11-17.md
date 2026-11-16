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
| ImpactCue | Armor Hit Icon Color | **방어구 관통** 아이콘 색상 | 방어구 명중 아이콘 색상 | 관통/비관통은 따로 있다(Blocked 쪽이 비관통) |
| Dynamic Maps | BTR marker color | BTR | BTR 표식 색 | 같은 섹션의 다른 항목은 이름이 대상만 적혀도 섹션이 "표식 색"이라 읽히지만, 이 항목만 원문과 같아 미번역처럼 보인다 → 섹션 관례 확인 후 정리 |
| Trauma Core | Scav Hitboxes / Scav Systems | 봇 피격 부위 / 봇 적용 | 확인 필요 | 설명을 보고 정한다 |

### 3-2. 남은 영문 — 공식 한국어가 있다

| 모드 | 지금 | 고칠 말 |
| --- | --- | --- |
| ImpactCue | Boss 피해자 색상 · Boss 아이콘 파일 | 보스 … |
| ImpactCue | Scav 아이콘 파일 · SCAV 피해자 이름 색상 | 스캐브 … |
| ImpactCue | Health HUD 표시 · Health Bar 표시 | 체력 HUD 표시 · 체력 막대 표시 |
| ImpactCue | Compact HUD 활성화 · Compact HUD 위치 초기화 | 간이 HUD … (F10 ko 와 맞춘다) |

`Transit`(공식도 영문)·탄약/수류탄 이름·`Arc`·`Focus Rail`(나침반 표시 방식 이름)은 고유명으로 둔다.

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

(고칠 때마다 아래에 덧붙인다)
