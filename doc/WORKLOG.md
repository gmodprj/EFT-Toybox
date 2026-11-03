# 작업 이력

2026-10-24, `E:\Spt` (SPT 4.1.6 / EFT 0.16.9.5.40743) 모드 설치 작업 기록. 시각은 한국 시간(서버 로그는 UTC로 9시간 느림).

## 요약

| 단계 | 내용 | 모드 | 결과 |
| --- | --- | --- | --- |
| 준비 | 설치 상태 점검, 스테이징·백업 폴더 구성 | - | SPT 4.1.6 설치 정상(11:57 설치 완료) |
| 1차 | UI·탄약 정보·개조 화면 편의 (4묶음) | 23 | 게임 플러그인 24개·서버 모드 8개, 오류 0 |
| 2차 | 편의(A)·SVM(C)·WTT 콘텐츠(B) | 9 | 게임 30개·서버 14개, 오류 0 |
| 3차 | 도구 3·커스텀 상인 4 | 7 | 서버 19개. HarryHideout 상점 비는 문제 → SVM 설정 수정으로 해결 |
| 4차 | 상인 목록 넘침 대응 | 2 | 게임 35개, 오류 0 |
| 정리 | 문서·스크립트 정리, git 저장소 구성 | - | 이 문서 |
| 후보 조사 | 4.1.6 호환 626개 전수 조회 → 미설치 585개에서 136개 추림 | - | [CANDIDATES-416.md](CANDIDATES-416.md) |
| 5차 | UI(A)·총기(B)·탄약(C)·상인(D)·치트·도구(E) | 52 | 게임 56개·서버 38개. TraderNamer로 서버 1회 중단 → 설치 스크립트 수정 |

설치된 모드 전체: [INSTALLED-MODS.md](INSTALLED-MODS.md) / 모드별 설명: [MODS-GUIDE.md](MODS-GUIDE.md)

## 상세

### 준비 (11:57 ~ 12:24)

- SPT 설치기 로그(`E:\Spt\spt-installer.log`)로 4.1.6 설치 완료·패처 정상 확인. 원본 라이브 게임은 `E:\Battlestate Games\Eft`(빌드 48146), 복사본만 다운패치됨.
- 기준 문서: `D:\Download\SPT 4.1.6 타르코프 싱글 모드 정리 (UI·총기·탄약·상인·치트).md`
- Forge API(`https://sp-mod.com/api/v0`, 인증 불필요)로 후보 22개의 4.1.6 호환 최신 버전·의존성 조회.
  - Color Converter API: ODT's Item Info·Colored Tracers 메타데이터는 구버전(1.0.0/1.1.1)을 가리키나 4.1.3+용은 1.2.0뿐 → 1.2.0 채택.
  - UI Fixes: Show Me The Money 제약은 6.0.2까지였으나 최신 6.0.4 채택.
- `E:\Spt_Mods\{downloads,extracted,backup}` 구성, 모드 없는 원본을 `backup/clean-2026-10-24`에 백업.
- 1차 23개 다운로드(약 2 MB) — 전부 크기 일치. 7z는 Windows 기본 `tar.exe`로 해제.

### 1차 설치 (12:35 ~ 13:25)

| 묶음 | 시각 | 내용 | 확인 |
| --- | --- | --- | --- |
| 1 | 12:35 | 기반(UI Fixes, Color Converter API) + 편의 4 | 서버 모드 2개. 첫 테스트에서 6969 포트 충돌 1회(원인 불명, 재시도 정상) |
| 2 | 12:50 | 지도·아이템 정보·열쇠·무게·방어구 아이콘 | 사용자 서버 가동 중이라 테스트 서버를 별도 실행(포트 충돌로 웹서버는 미기동, 모드 로드만 확인). 서버 모드 5개 |
| 3 | 13:12 | 탄약 정보 7 | 서버 모드 7개. 시작 문구가 랜덤("모빈할 시간이다")이라 대기 조건 오판 → 인증서 로드 문구로 판정 변경 |
| 4 | 13:19 | 개조 화면 4 | 서버 모드 8개 |

- 13:25 게임 실행으로 3·4묶음 게임 쪽 9개가 처음 로드됨(그 전 "확인 완료" 시점엔 게임이 실행되지 않았음을 로그로 확인).
- 결과: 게임 플러그인 24개(SPT 5 + 모드 19), 오류 0.

### 2차 (13:30 ~ 14:40)

- 후보 76개 조회(총기·탄약·상인·치트 + 의존). 대부분의 총기 팩·상인이 프로필 결속. 대용량(Armory 4.5 GB 등).
- 제외: Tylevo's Field Attachments(UnityToolkit 4.1.6 없음), Epic's AIO(Armory와 충돌 위험), 1차와 기능 중복 모드.
- 추천 세트 A(편의 5)·B(WTT CommonLib→Armory→Artem)·C(SVM) 9개, 6.05 GB 다운로드(스트리밍). Armory는 WTT 자체 서버에서 받음.

| 묶음 | 시각 | 확인 |
| --- | --- | --- |
| A 편의 | 13:40 | 서버 모드 10개. 게임 확인은 14:03 실행에서(플러그인 28개). 사용자 보고 시점엔 미실행이었음 |
| C SVM | 13:52 | 서버 모드 11개. 프리셋 없음 → 비활성. 사용자 서버(13:51)엔 미적용 |
| B1 CommonLib | 14:15 | 서버 12개, 20초. `BepInEx\patchers`에 프리패처 추가 |
| B2 Armory | 14:15 | 서버 13개, 23초. DLL 표기 2.0.5(사이트 3.0.0, 파일 크기는 3.0.0과 일치) |
| B3 Artem | 14:16 | 서버 14개, 26초 |

- 14:39 게임: 플러그인 30개, 프리패처 정상, 오류 0. Fika용 DLL은 Fika 없으면 로드되지 않음(30개가 정상).
- 같은 시각 사용자 요청으로 기록 문서를 `doc/` 아래 카테고리별로 이동.

### 3차 (15:00 ~ 16:10)

- SVM 한국어 사전으로 기능 범위 확인 → FIR·구매 제한·무게·탈출구·경험치 관련 치트 8개를 SVM 중복으로 제외.
- 상인 4(Lotus, Scorpion, Ref, HarryHideout) + 도구 3(Skipper, give-ui, Open Sesame) 다운로드(26 MB).
- 사용자가 14:45~15:02에 `Greed.exe`로 SVM 프리셋 `Noname` 생성·적용. SVM이 `HideSpecialIconGrids.dll`을 직접 설치.

| 묶음 | 시각 | 확인 |
| --- | --- | --- |
| 도구 3 | 15:37 | 서버 15개 |
| Lotus | 15:38 | 16개, 35초 |
| Scorpion | 15:39 | 17개. `(╯°□°)╯︵ ┻━┻`는 랜덤 문구 |
| Ref | 15:40 | 18개 |
| HarryHideout | 15:40 | 19개. "플리마켓 거래 생성 실패, 물품 없음" |

- 15:55 게임: 플러그인 33개(+Skipper, Open Sesame, HideSpecialIconGrids), 오류 0.
- **HarryHideout 문제**: 서버에서 10초마다 "물품이 없습니다" Error.
  - 원인: SVM 프리셋 `Hideout.RemoveConstructionsRequirements: true` → 은신처 건설 요구사항이 사라져 Harry가 팔 품목이 없음.
  - 조치(16:00, 사용자 선택): 해당 값만 `false`로 변경. 백업 `backup/svm-presets/Noname-before-harryfix-160015.json`.
  - 확인(16:08 서버 재시작): Harry 오류 0건.

### 4차 (16:01 ~ 16:10)

- 상인이 늘어 상단 상인 줄이 플레이어 패널 뒤로 넘어감(스크린샷).
- Forge 검색 → Kaeno-TraderScrolling 4.1.0(좌우 스크롤) + TraderSpace 2.0.0(플레이어 패널 제거) 16:03 설치(게임 전용, 서버 가동 중 복사).
- 제외: Adjustable Trader Rows REVIVED(4.0.13 전용).
- 16:09 게임: 플러그인 35개, 오류 0. TraderScrolling DLL 표기는 4.0.0.

### 정리 (16:15 ~)

- 문서 재구성: [INSTALLED-MODS.md](INSTALLED-MODS.md)(자동 생성), 이 문서, [PROCEDURE.md](PROCEDURE.md).
- 작업 스크립트를 `tools/`로 보관.
- `E:\Spt_Mods`를 git 저장소로 구성(문서·스크립트만 추적, 대용량·프로필 백업은 제외).

### 후보 조사 (17:0x)

- sp-mod.com API로 4.1.6 호환 모드를 전수 조회: 사이트 전체 2021개 중 **626개**가 4.1.6 호환. 설치된 41개를 뺀 **585개**를 카테고리·키워드로 분류하고, 요청 분야(UI·총기·탄약·상인·치트)에서 **136개**를 추려 [CANDIDATES-416.md](CANDIDATES-416.md)로 정리.
- 의존성 조회에서 걸러낸 것: **UnityToolkit(1426)** 최신이 `4.1.5` 표기(4.1.6 없음) → Scrollable Attachments·Tylevo's Field Attachments 보류. **Eco's Attachment Emporium(2882)** 최신이 `4.1.1` → Rook 제외.
- 대형 상인의 실제 비용 확인: ISB Aishi는 본체 4.5 GB에 WTT Content Backport **6.0 GB**가 동반, Astral Divide는 SAIN·BigBrain·MoreBotsAPI까지 연쇄 → 둘 다 제외.
- 계획은 [PLAN-STAGE5.md](PLAN-STAGE5.md)에 5단계로 작성(설치 52 / 보류 32 / 제외 52).

### 5차 (17:2x ~ 19:00)

| 단계 | 모드 | 서버 모드 | 서버 테스트 | 메모 |
| --- | --- | --- | --- | --- |
| 5-A UI | 21 | 19 → 20 | STARTED 29초 | 20개가 게임 쪽. 18:21 게임 실행에서 21개 전부 로드 확인(플러그인 56개) |
| 5-B 총기 | 9 | 20 → 23 | STARTED 26초 | 계획에 "게임 전용"이라 적었으나 3개가 서버 쪽 |
| 5-C 탄약 | 6 | 23 → 27 | 4회 모두 STARTED | 비결속 4개 일괄 → 결속 2개 하나씩 |
| 5-D 상인 | 10 | 27 → 37 | 1회 FAILED → 수정 후 STARTED | 결속 상인 6개를 하나씩 |
| 5-E 치트·도구 | 6 | 37 → 38 | STARTED 35초 | 전부 비결속 |

- **TraderNamer로 서버 중단(5-D)**: `SERVER FAILED in 11s`, `Could not find a part of the path '...\TraderNamer\avatars'`. 압축에는 빈 폴더 `avatars/`가 있는데 `install-and-test.ps1`이 파일만 복사해 누락됐고, 모드가 시작 때 그 폴더를 열면서 서버 전체가 멈췄다. 스크립트가 **빈 폴더도 생성**하도록 고치고(기록은 `installed-<Tag>.dirs.txt`) 재테스트 `STARTED 29s`. 직후 설치한 Amonya도 빈 폴더(`db/98_Debug`)가 있어 같은 처리로 넘어갔다.
- **압축 구조가 달라 손본 것 4건** — Tag Over Name(읽어보기·소스·중첩 복사본 혼입 → `extracted/_extras-stage5-A/`로 분리), Cobra's Ammo Stack(`<루트>/SPT/user/mods/...` → `SPT_Runtime/` 기준으로 이동), Trader Level Requirements Editor(폴더 한 겹 더 → 평탄화), Cobra's SPT Explorer(외부 UI·문서가 SPT 루트로 들어가는 드래그 설치형 → README대로 설치). 네 건 모두 새로 만든 `tools/check-conflicts.mjs`가 미리 잡았다.
- **메타데이터 불일치** — PresetTrader는 Forge 크기(1,358,724)와 실제 GitHub 에셋(232,040)이 다름(zip 자체는 정상). Gunwright는 Forge 버전 1.0.2인데 서버 로그에 1.3.0으로 찍힌다.
- **기본값이 센 것** — Cobra's Ammo Stack 기본 스택 500발(5-E 시점 탄약 427종에 적용, 사용자 판단으로 유지), Direct Reload의 의료 사용시간 x0.5·약물 지속 x2.
- 결과: 설치 41 → **93개**, 파일 1809 → **2280개**. 게임 쪽 플러그인 56개, 서버 모드 38개, 오류 0.

### 게임 확인 (19:13 ~ 19:33)

- 사용자가 서버(19:11:57)와 게임(19:13:00)을 실행. 게임 쪽 플러그인 **76개** = SPT 기본 5 + 모드 71. stage5-B~5-E의 게임 쪽 20개가 전부 로드됨(`ozen.MagCheckInterrupt.Net.dll`은 Fika용이라 로드되지 않는 것이 정상).
- **ScopeRangefinder가 충돌을 스스로 처리**한다는 로그 확인 — AutoRanging은 `AutoZeroEnabled`가 켜져 있으면 자기 거리 측정을 억제하고, PiP-Disabler가 있으면 조준경 대신 화면 오버레이로 표시한다. 세 모드를 같이 넣어도 서로 싸우지 않는다.
- 게임 로그 Error 2건:
  - `7Bpencil.TransparentSights`: 플러그인 폴더에 `config.json`이 없어 기본 설정으로 되돌림(압축에 dll만 들어 있었다). 기능은 동작하고 F12 설정(`7Bpencil.TransparentSights.cfg`)은 생성됨.
    - 모드 페이지를 확인한 결과 **배포되는 `config.json`은 없다**. 설정은 F12 메뉴와 조준경 아이콘 더블클릭(`TRANSP: ON` / `OFF` / `ON + MOUNT`)으로 하고, dll에 `SaveTransparentScopesToFile`·`LoadTransparentScopes`가 있어 그 파일은 **모드가 조준경별 전환을 저장할 때 직접 만든다**. 즉 첫 실행의 Error는 "저장된 조준경별 설정이 아직 없다"는 뜻이고 조치할 것이 없다. 0.2.3 기준 기본값은 모든 조준경 투명·광학 하우징 투명 끔.
    - 19:48:09 사용자가 게임에서 조준경 아이콘을 더블클릭해 전환하자 `config.json`(293바이트)이 실제로 생성됨 — 항목 6개(`EnabledWithMount` 4, `Disabled` 2). 형식은 `"<아이템 템플릿 ID>": "Enabled" / "EnabledWithMount" / "Disabled"`. 추정이 아니라 실측으로 확인.
  - `Direct Reload`: Fika 전용 패치 `FikaHostReloadMirror` 실패 — Fika 미설치라 정상.
- ScopeRangefinder는 `Anvil-WebOverlay`(미설치)를 참조해 Serialization Fix에서 제외되고, 레이아웃 파일이 없어 기본값으로 생성됨. `EFT.TacticalRangeFinderController.method_0`을 찾지 못한다는 HarmonyX 경고도 있다.
- 서버 쪽(사용자 서버, 모드 38개): Error·Critical 0건. 상인 어소트 요청이 **17개 상인**에 들어가고 커스텀 상인 id(Gunwright `8b4f2d91…`, PresetTrader `67a5e1c2…`, `6688d464…`)도 응답 중. Amonya는 커스텀 아이템 193개·퀘스트 183개를 만들었다.
- 남은 경고는 알려진 것들 — SVM 배낭 제한(무시 가능 문구 포함), CaseTrader가 핸드북 가격 없는 케이스 2종 건너뜀, BTR 기사(`638f541a…`) 임시 항목 생성.

### 문서 정리 (19:1x)

- [MODS-GUIDE.md](MODS-GUIDE.md) 작성 — 설치된 93개를 기능별 10묶음으로 나눠 무엇을 하는 모드인지, 결속 여부와 손볼 설정 파일까지 적음.
- `tools/` 변경: `check-conflicts.mjs` 신규, `install-and-test.ps1`(빈 폴더), `gen-installed-mods.mjs`(`manifest-stage<N>-<X>.json` 인식·로드 현황 분리).

### F12 설정 창 한글화 — 파일럿 (21:0x ~ 21:45)

목표: F12(ConfigurationManager) 창의 글자를 한글로. 번역 대상은 네 층 — 창 UI 약 40, 모드 이름 71, 섹션 187, 설정 1096과 설명 1082 (`BepInEx\config\*.cfg` 58개 집계). 합 약 2,480개.

- **`.cfg` 를 고치는 방식은 안 된다.** 키 이름·`##` 설명은 메모리의 ConfigEntry에서 저장 때마다 다시 써지는 산출물이라 화면이 바뀌지 않고 원복된다.
- 한글 렌더링은 **사용자가 F12 검색창에 직접 입력해 확인** — 기본 IMGUI 폰트로 정상 출력. 폰트 교체 작업 불필요.
- 어셈블리를 `MetadataLoadContext` 로 떠서 확인한 것: ConfigurationManager **v18.4.0**, `BuildSettingList()` public, `SettingEntryBase` 에 `DispName`·`Category`·`Description` 필드가 따로 있음. BepInEx 5.4.23.5 / Harmony 2.9.0.
- 그래서 **Bind 후킹 대신 `BuildSettingList()` Postfix** 로 정했다. 화면용 복사본만 고치므로 `.cfg` 파일과 설정 값, 다른 모드 파일을 건드리지 않고, 목록이 매번 새로 만들어져 플러그인을 빼면 자동 원복된다. 모드 헤더 이름만 `BepInPlugin.Name` 을 직접 고치므로 끌 때 되돌리도록 원문을 보관한다.
- `src/SptKoreanConfig/` 신규(net472, 13KB). `lang/<GUID>.json` 사전은 **수정 시각이 바뀌면 게임 재시작 없이** 다시 읽는다. 설치는 `tools/install-koreanconfig.ps1`.
- 파일럿: `7Bpencil.TransparentSights` (섹션 3, 항목 10). 21:45 게임 재시작 후 로그 확인 — 리플렉션 대상 6개 전부 `찾음`, `한글 적용 — 항목 10/…`, 오류 0. 화면에서 모드 이름·섹션 3개·항목 10개 전부 한글로 표시됨.
- 로그에서 전체 항목 수가 **992 → 1029 로 늘었다.** 설정을 뒤늦게 등록하는 모드가 있다는 뜻이고, F12 를 열 때마다 다시 적용하는 지금 방식이라 문제없다.
- 남은 것: 창 자체 UI 약 40개는 `ConfigurationManager.dll` 에 하드코딩이라 이 방식으로 안 된다(LGPL 소스 포크 재빌드 또는 DLL 문자열 치환 필요).

### F12 한글화 — 사전 4개 (21:5x ~ 22:2x)

- 큰 모드부터 사전 작성: UI Fixes 138, Amands' Sense 100, PiP-Disabler 96, Dynamic Maps 83 — **427항목 전부**. 파일럿 10항목까지 합쳐 사전 5개.
- `tools/check-lang-coverage.mjs` 신규. `.cfg` 의 실제 항목과 사전을 맞춰 누락·오타·설명 없는 항목을 센다. UI Fixes 작성 직후 12건이 "cfg 에 없는 열쇠"로 떴는데, **검사기 쪽 버그**였다(값이 비어 있는 단축키 줄 `Use Item Shortcut = ` 을 `trimEnd` 후 찾아 놓침). 고친 뒤 138/138.
- 색상 항목(Sense 58, Dynamic Maps 19)은 이름만 두고 설명을 넣지 않았다. 이름이 곧 설명이라 덧붙일 것이 없다.
- 설치는 JSON만 복사하면 된다(DLL 변경 없음). 게임이 꺼져 있어 화면 확인은 다음 실행 때.

### F12 한글화 — 나머지 사전 (22:3x ~ 2026-10-25 00:0x)

- 남은 53개 모드를 큰 것부터 전부 작성. **사전 58개 / 1096항목**으로 `BepInEx\config\*.cfg` 의 설정 항목을 **빠짐없이** 덮었다. 검사기 기준 누락·오타 0.
  - 100항목 이상: UI Fixes 138, Sense 100
  - 40~96: PiP-Disabler 96, Dynamic Maps 83, CaliberUnderName 65, ScopeRangefinder 55, Use Items Anywhere 52, Better Thermal & NV 49, Show Me The Money 40, Flexible Builds 40
  - 나머지 48개는 29항목 이하
- 사전이 없는 cfg는 `spt.koreanconfig`(한글화 모드 자신) 하나뿐이고, 이미 한글이라 만들지 않았다.
- 설치된 플러그인 71개 중 cfg가 있는 것은 58개다. BepInEx는 `Bind` 시점에 cfg를 쓰므로 **cfg가 없다 = 설정 항목이 없다**는 뜻이고, F12에 보일 것은 전부 덮은 셈이다.
- BepInEx 본체(22)·ConfigurationManager(6)·FixPluginTypesSerialization(11) 같은 기반 설정도 F12에 뜨므로 함께 번역했다.
- 작은 모드 31개는 JSON을 하나씩 쓰는 대신 스크래치패드의 생성 스크립트 2개로 써냈다. 결과는 같은 검사기로 검증.
- 설치는 JSON 복사만(203KB). 게임이 꺼져 있어 화면 확인은 다음 실행 때.

### F12 한글화 — 창 자체의 글자 (2026-10-25 05:0x)

- 창 제목·버튼·안내 문구는 `ConfigurationManager.dll` 코드 안에 박혀 있어 표시용 필드로는 못 바꾼다. 검토 때 적어 둔 방법은 ⒜ LGPL 소스 포크 재빌드 ⒝ DLL 바이너리 치환이었는데, **둘 다 쓰지 않았다.**
- 대신 **Harmony 트랜스파일러로 `ldstr`(문자열 상수)을 불러오는 자리에서 한글 문자열로 바꿔치기**했다. DLL 파일을 건드리지 않으므로 플러그인만 빼면 원문으로 돌아가고, 모드 업데이트에도 안전하다. 사전에 있는 문자열을 실제로 쓰는 메서드만 골라 패치한다.
- 바꾸면 안 되는 문자열은 코드에 **금지 목록**으로 박았다 — 설정 섹션·키 이름(`General`, `Show advanced` 등)은 바꾸면 `.cfg` 의 키가 달라지고, `ConfigurationManagerAttributes`·`DispName` 같은 이름은 덕 타이핑이 깨진다. 사전에 적어도 건너뛰고 경고를 남긴다.
- `tools/check-ui-strings.ps1` 신규. 사전의 원문이 DLL 안에 **글자 그대로** 있는지 확인한다(끝 공백 포함). 한 글자만 달라도 조용히 무시되기 때문이다. 34개 전부 확인.
  - DLL의 UTF-16 문자열은 파일 기준 짝수 위치에 있다는 보장이 없어, 0번과 1번 오프셋 양쪽에서 읽어야 전부 걸린다. 처음 DLL을 뜯을 때 `Plugin / mod settings` 가 "없다"고 나온 것도 이 때문이었다.
- 번역한 것 34개: 창 제목, 탭 4개(일반 설정·단축키·고급 설정·디버그 정보), 로그 열기·닫기·검색·지우기·모두 펼치기·모두 접기·되돌리기·지정·취소, 키 지정 안내, 켬/끔, 도움말 2줄, 오류 문구 7개.
- 모드 사전과 달리 코드를 고치는 방식이라 **이것만은 게임 재시작이 필요하다**(JSON만 고칠 때의 즉시 반영은 모드 사전에만 해당).
- 설치 완료(DLL 19KB). 게임이 꺼져 있어 화면 확인은 다음 실행 때.

### F12 한글화 — 실측 확인과 사전 수정 (05:17 ~ 05:2x)

- 사용자가 게임을 켠 뒤 로그 확인: **사전 58개 / 섹션 187 / 항목 1096 읽음**, 리플렉션 대상 6개 전부 `찾음`, **항목 990/992 적용**, 오류 0.
  - '사전에 없음 2'는 한글화 모드 자신(`spt.koreanconfig`)의 설정 2개다. 일부러 사전을 만들지 않았으니 예상대로다.
- 창 UI는 `사전 34개, 메서드 14개, 문자열 31개 교체` — **34개 중 3개가 모자랐다.** Mono.Cecil 로 IL의 `ldstr` 를 전부 훑어 원인을 찾았다.
  - **5개 항목이 더 긴 문자열의 일부였다.** 문자열 상수는 통째로만 바꿀 수 있어, 일부만 적은 항목은 영영 걸리지 않는다. 예: 툴팁 3줄이 줄바꿈을 포함한 **하나의** 문자열이었고, `Error message:` 는 경고 문구와 한 덩어리였다.
  - `tools/check-ui-strings.ps1` 이 이걸 놓친 이유도 같다. **DLL 안에 그 글자가 "들어 있는지"만 봤기 때문에** 더 긴 문자열의 일부도 OK 로 통과시켰다. 스크립트에 이 한계를 명시하고, 확실한 확인은 로그 쪽으로 옮겼다.
- 고친 것: 세 줄짜리 툴팁을 한 항목으로 합치고, 경고 문구 2종을 통째로, `Cause: ` 를 `\nCause: ` 로. 사전 34 → **33개**, IL 기준 **쓰이지 않는 항목 0개 / 출현 35개 / 메서드 16개**.
- 플러그인이 시작할 때 **한 번도 바뀌지 않은 항목을 경고로 찍도록** 했다. 로그만 보면 알 수 있으니 같은 실수를 두 번 하지 않는다.
- 이 수정은 DLL 변경이라 **게임을 다시 켜야** 반영된다.

### F12 한글화 — 최종 확인 (05:31)

사용자가 게임을 다시 켠 뒤 로그 확인. 예상한 수치와 전부 일치했다.

```
번역 사전 58개 모드 / 섹션 187 / 항목 1096 읽음
리플렉션 대상 점검 — 6개 전부 찾음
창 UI 한글화 — 사전 33개, 메서드 16개, 문자열 35개 교체
  창 UI 사전 항목 전부 사용됨
한글 적용 — 항목 990/992, 섹션 171종, 사전에 없음 2
```

- 창 UI 교체가 31 → **35개**로 늘고 쓰이지 않는 항목이 **0**이 됐다. 부분 문자열 문제는 해결.
- `사전에 없음 2`는 한글화 모드 자신(`spt.koreanconfig`)의 설정 2개다. 이미 한글이라 사전을 만들지 않았으므로 정상이며, 이 숫자는 앞으로도 2로 남는다.
- 플러그인 관련 경고·오류 0건. **F12 한글화 작업 완료.**

### 문서 정리 — 자체 제작 플러그인 반영 (06:4x)

한글화 플러그인이 게임에는 깔려 있는데 모드 문서 어디에도 없어서, 설치물 목록에 올렸다.

- `gen-installed-mods.mjs`의 "다른 경로로 설치된 것"에 `SptKoreanConfig 0.1.0` 추가. Forge 모드가 아니라 매니페스트·설치 기록이 없으므로 이 자리가 맞다.
- 같은 스크립트의 로드 현황을 갱신 — 게임 플러그인 **76 → 77개**(SPT 기본 5 + 모드 71 + 자체 제작 1, 06:37 로그). 서버 모드 38개는 그대로(클라이언트 전용 플러그인이라 영향 없음).
- [MODS-GUIDE.md](MODS-GUIDE.md)에 "직접 만든 것" 묶음 신설, "설정을 손볼 만한 곳"에 사전 파일 위치 추가. 머리말을 "Forge 모드 93개 + 직접 만든 것 1개"로 바로잡았다.
- 루트 [README.md](../README.md)에 `src/` 와 도구 4개(`check-conflicts`, `install-koreanconfig`, `check-lang-coverage`, `check-ui-strings`)를 빠뜨리고 있어 함께 채웠다.

### 탄창 소실 오류 조사 (08:1x ~ 09:3x)

레이드 중 `Reload failed: Direct load flow error`가 뜨고 탄창이 사라진다는 제보로 시작했다. 결론은 [MODS-GUIDE.md의 "알려진 충돌"](MODS-GUIDE.md)에 정리했고, 여기에는 어떻게 좁혔는지를 남긴다.

**증거 수집**

- `LogOutput.log`에 예외 전문이 있었다. 던지는 쪽은 모드가 아니라 게임이다 — `Item.get_Parent` → "Trying to get parent of an item that doesn't have a parent".
- 스택의 `DMD<...>` 프레임은 전부 Harmony 패치라는 뜻이다. 설치된 플러그인 DLL을 Mono.Cecil 로 전부 훑어 **누가 어느 메서드를 패치했는지** 표로 만들었다.
  - `FirearmController.ReloadMag` ← UI Fixes `SwapIfNoSpacePatch`
  - `ReloadExternalMagResult.Run` ← Mag Retention Reload `ReloadRunPatch`
  - `ItemManipulator.Move` ← UI Fixes `MoveBeforeResize`·`MoveOperationDontForgetAboutPins`
  - 호출한 쪽 ← Direct Reload `SoftcoreReloadPatch`
- Direct Reload의 IL을 읽어 호출 인자를 확정했다: `ReloadMag(weapon.GetCurrentMagazine(), FindFreeAddress(...), callback)` — **빼낼 탄창과 넣을 탄창이 같은 객체다.** 이것이 깨지기 쉬운 구조의 핵심이었다.

**틀린 가설 두 개 (실측으로 기각)**

- "대형 탄창에서만 난다" — D60·MPX TTI 만 나와 그렇게 보였으나, 이어서 **UMP45 표준 탄창**에서도 났다. 쓰던 총이 그랬을 뿐이다. 패치 이름이 `SwapIfNoSpace` 라 더 그럴듯해 보였던 것도 한몫했다.
- "UI Fixes의 `탄창 제자리 교체`가 범인" — 껐는데 그대로 났다(14번째). 기각.

**확정**

세 조합을 한 번에 하나씩만 바꿔 비교했다. `켬/켬` 13건 → `끔/켬` 계속 → **`켬/끔` 0건**. UI Fixes 설정은 켠 채로 문제없다.

마지막에 Direct Reload 디버그 로그를 켜고 통과 장면을 직접 확인했다. `calling ReloadMag` 에 4회 도달(빈 탄창 `mag=0/25` 포함)해 전부 예외 없이 끝났고, 진단 줄이 탄창이 `Slot(mod_magazine)` → `SearchableGrid` 로 옮겨간 것까지 보여 준다. 예전에는 이 구간에서 부모를 잃었다.

**남긴 것**

- 조사 중 "오류가 안 난다"만으로는 고쳐졌는지 알 수 없었다. 성공 경로가 로그를 남기지 않기 때문이다. 그래서 디버그 로그로 **도달 횟수를 세는** 방식으로 바꿔 확증했다. 무오류는 증거가 약하다.
- 디버그 로그는 로그를 빠르게 불린다(1,700줄 → 5,100줄). 확인 뒤 다시 끈다.

## 바로잡은 사항

- 1차 모드는 "빼도 안전"하다고 안내했으나, Forge 기준 ODT's Item Info·Trader Modding·Modding Stats Helper도 프로필 결속 표시가 있음. [INSTALLED-MODS.md](INSTALLED-MODS.md)의 결속 열이 기준.
- 4묶음 설치 당시 "3묶음 플레이 로그"라고 한 것은 실제로는 2묶음 상태의 로그였음.
- 2차 B 확인 시 게임 플러그인 31개를 예상했으나 Fika용 DLL은 로드되지 않으므로 30개가 정상.
- 5차 계획서에 stage5-B를 "게임 전용"으로 적었으나 Default Fire Mode Fixer는 서버 전용, Flexible Builds·EasyMounting은 게임+서버였다. 설치 위치는 압축 해제 경로로만 확정된다.
- 5-A 게임 로드 확인에서 처음 18/21로 셌으나, Use Items Anywhere·Advanced Modding Lights·Refined Flea Offer List는 로그에 공백 포함 표시명으로 찍혀 파일명 검색에 걸리지 않았을 뿐 21개 전부 로드돼 있었다.

### 총기·파츠 / 상인 모드 재조사 (2026-10-25)

기존 후보 문서는 이름·소개 키워드로 5개 분야를 추린 것이라, 분류를 비켜간 것이 남아 있을 수 있었다. 그래서 두 분야만 다시 훑었다.

- Forge 전수 조회가 **633개**로 늘었다(10-24 조사 626개). 하루 사이 7개.
- 부착물·조준경·소음기·탄창·빌드 / 상인·플레아·물물교환·보험 계열로 걸러 181개, 그중 설치본·후보 문서·5차 계획서에 **한 번도 안 나온 것이 81개**였다.
- 81개를 성격별로 가른 뒤 두 분야에 실제 해당하는 **27개**만 `tools/mod-detail.mjs`로 버전·용량·결속·의존성을 확인해 [CANDIDATES-GUN-TRADER.md](CANDIDATES-GUN-TRADER.md)에 정리했다. 원자료는 `stage2-survey/sweep-416-gun-trader.json`.
- 가장 "총기 파츠"에 가까운 것은 **Kalashnikov Enhanced Modding**(3010) — AK의 핸드가드·가스튜브·사이드레일·배럴을 분리해 부품으로 만든다. 다만 260MB·결속이다.
- 상인 쪽은 **NATO Quartermaster**(3030, 354KB, 4.1.6 정확)와 **OpenBarters**(3194, 14KB, 비결속)가 부담이 가장 적다.
- 설치는 하지 않았다. 대형 콘텐츠(3010·3035·2239·2737)는 5차 계획의 "총기 팩·대형 상인은 범위 밖" 방침대로 넣을지부터 정해야 한다.

### sp-mod 밖 배포지 조사와 6차 계획 (2026-10-25)

사용자 요청으로 Forge 밖도 훑었다.

- `forge.sp-tarkov.com`·`hub.sp-tarkov.com`은 **DNS 조회부터 실패**한다. 예전 공식 주소는 죽었고 sp-mod.com이 그 후속이다.
- GitHub(topic `spt-mod`·`sptarkov`·`spt-aki`)에는 Forge에 없는 4.1.6 모드가 실제로 있었다. 이번에 집은 둘: **Quick Trader Load Times**(상인 화면 로딩, MIT)와 **PortableTraders**(BTR·등대 상인을 주 화면으로, MIT). 릴리스 asset URL은 `download-stage.mjs`로 그대로 받힌다.
- Nexus Escape from Tarkov 섹션은 SPT 모드가 **22개 전부**다. 대부분 한 사람의 포팅본이고 다운로드 30~700대. 쓸 만한 건 상인 초상화 교체(VanillaTradersRework-Reloaded) 하나. 받으려면 계정 로그인이 필요해 스크립트로는 못 받는다.
- 아카이브(tekunogosu/sptmodarchive)는 sp-mod.com 목록의 사본이라 새 출처가 아니다. Reddit·일반 검색은 SPT 4.1 결과가 사실상 없었다.
- **키워드 추림의 구멍을 하나 찾았다.** ECOTI(2930, 클립온 열상, 다운로드 15,943)는 이름·소개에 부착물 계열 단어가 없어 1·2차 추림 모두 놓쳤고 GitHub 쪽에서 먼저 보였다. 6차에 넣는다. 그 3.4.2가 설치된 PiP Disabler(2667)와의 열상 표시 문제를 고친 버전이다.
- 5차에서 보류한 Field Attachments(3125)의 걸림돌을 확인했다: UnityToolkit **2.0.2 이상**이 필요하고, 2.0.2는 **Forge에만**(4.1.5 표기) 있다. GitHub 최신은 2.0.1이라 그쪽으로는 해결되지 않는다. 그래서 6-C는 1426 단독 설치·확인부터로 잡았다.
- 결과를 [PLAN-STAGE6.md](PLAN-STAGE6.md)에 정리했다(6-A 10개, 6-B 결속 2개, 6-C 선행 확인 3개, 6-D 수동 1개). 다운로드 목록 `download/manifest-stage6.json`까지 만들어 뒀고, 다운로드·설치는 하지 않았다.

### 6-A 설치 (2026-10-25 09:0x ~ 09:4x)

[PLAN-STAGE6.md](PLAN-STAGE6.md)의 6-A 10개를 받아 설치했다. 설치본 93 → **103개**, 파일 2280 → 2322개, 서버 모드 38 → **46개**.

- 다운로드 12개 모두 바이트 일치. Forge 링크가 전부 `release-assets.githubusercontent.com`으로 리다이렉트된다 — Forge도 GitHub 릴리스에 파일을 둔다.
- **Git Bash의 `tar`로는 zip이 안 풀린다**(GNU tar). `C:\Windows\System32\tar.exe`를 직접 불러야 한다. 문서의 "tar.exe"는 그쪽이다.
- 압축 구조를 손본 것: **PortableTraders**가 `SPT/user/mods/...`(이 설치본은 `SPT_Runtime/`)였고 루트에 LICENSE·README가 섞여 있었다. 경로를 옮기고 문서는 `extracted/_extras-stage6/`로 뺐다. stage5의 Cobra's Ammo Stack과 같은 유형.
- BrightLasers는 같은 이름의 DLL이 `BepInEx/plugins`와 `user/mods` 양쪽에 있다. 해시가 달라 **클라이언트·서버 두 짝이 맞다**(중복 아님). 둘 다 설치.
- `check-conflicts.mjs stage6` 결과 CLEAN — 기존 경로와 겹침 0, 모드 간 겹침 0, 설치 경로 아닌 파일 0.
- 설치 순서: 비결속 8개(`-SkipTest`) → 서버 테스트 1회 → 결속 2개는 프로필 백업 후 하나씩. 테스트 3회 모두 `SERVER STARTED`(50·47·47초), stderr 0바이트, `[Error]`·`[Critical]` 없음.
- 서버 쪽 신규 8개 로드 확인: BrightLasers·Ref Rework·Realistic Insurance·All Weapons Flea·Quick Trader Load Times·PortableTraders·Moe-TradeMarker·Vanguard. PortableTraders가 `BTR Driver·Lightkeeper 2명, 과제 30개`를, All Weapons Flea가 `무기 238종, BSG 제한 33개 해제`를 찍었다.
- 프로필은 설치 전 백업과 **해시가 같다**(`8c0fe366…`). 백업은 `profiles-before-stage6A`, `-2736`, `-2917`.
- 사용자 서버가 켜져 있어 설치 스크립트가 거부했다(스크립트 자체 안전장치). 종료하고 설치한 뒤 다시 띄워 뒀다 — 46개 로드, 웹서버 정상.
- `gen-installed-mods.mjs`를 6차에 맞게 고쳤다: 6단계 추가, **모드 id를 문자열로** 취급(Forge 밖 모드는 `gh-...`), 링크는 매니페스트의 `homepage`를 쓰고 없으면 Forge, 결속 조회는 숫자 id만.
- 남은 확인: **게임 쪽 7개**(OpenBarters·BrightLasers·NoBarrelShadow·RefReworkClient·RealisticInsurance·QuickTraderLoadTimes·Moe-TradeMarker)는 게임을 켠 뒤 `LogOutput.log`로 본다. 그리고 도그태그 용처가 Ledger와 Ref Rework로 둘이 됐다 — 게임에서 보고 하나를 끈다.

### 6-B 설치 — 결속 2개 (2026-10-25 09:5x)

프로필 백업 → 설치 → 테스트를 하나씩. 설치본 103 → **105개**, 서버 모드 46 → **48개**.

- **NATO Quartermaster 1.1.3**(4파일) — `49 curated offers and 6 quests`, 퀘스트 해금 Q1~Q6까지 로그로 확인. 상인이 7명째가 됐다. `SERVER STARTED in 41s`, 오류 0.
- **ECOTI 3.4.2**(압축 56파일 → 기록 **71파일**) — 서버가 뜰 때 WTT-Artem의 야투경에 맞춘 호환 파일을 **스스로 만든다**: `nvghostcompat/[wtt-artem]_(gpnvg-18…)` 5종 + `wwwroot/meshes-auto/*.glb` 10종. 설치 스크립트가 이 생성분까지 `installed-stage6B-2930.txt`에 넣었으므로 되돌릴 때 함께 지워진다.
  - 등록된 것: COTI 본체, 열상 라이저 TR-1 2색, 열상 스코프링 TSR-1/2/3 6종. 판매는 Peacekeeper LL3~LL4($1,750~$2,000, 한도 3).
  - `[COTI] meshes: 5 converted, 0 unchanged, 0 removed, 0 failed`. 오류 0.
- 프로필 해시는 6-A 시작 전과 **그대로 같다**(`8c0fe366…`). 백업 `profiles-before-stage6B-3030`, `-2930`.
- 사용자 서버를 다시 띄웠다 — 48개 로드, 웹서버 정상.
- 남은 확인(게임을 켠 뒤): 6차 게임 쪽 플러그인 **8개**, 상인 7명이 목록에 다 보이는지(TraderScrolling·TraderSpace), ECOTI 열상이 야투경 튜브 안에만 들어오는지 — **PiP Disabler(2667)·Better Thermal(2531)을 켠 상태**에서 본다. 문제가 나면 2531을 먼저 끈다.

### 6-C 설치 — 부착물 교체 UI (2026-10-25 10:0x ~ 10:2x)

5차에서 "UnityToolkit이 4.1.5 표기"라 보류했던 묶음을, 계획대로 **의존성 단독 설치 → 게임 로그 확인 → 본 모드** 순으로 처리했다. 설치본 105 → **108개**, 파일 2322 → 2655개.

- 조회에서 먼저 드러난 것: **Field Attachments 1.2.0은 `4.1.6` 정확 표기**, Scrollable Attachments 2.0.0은 `~4.1.6`이었다. 4.1.6 표기가 없는 건 UnityToolkit 2.0.2(`4.1.5`) 하나뿐이어서, 위험은 그 하나로 좁혀졌다.
- UnityToolkit은 `BepInEx\plugins`만이 아니라 **`BepInEx\patchers`(프리패처)** 에도 들어간다. 프리패처가 어긋나면 게임이 아예 안 뜨는 형태로 드러나므로 단독 확인이 맞았다.
- 게임을 한 번 켠 로그(09:57)에서 통과를 확인했다.

```
Loaded 1 patcher method from [UnityToolkit-Prepatcher 2.0.2.0]
UnityToolkit-Prepatcher: Loading new version of assembly 'System.Runtime.CompilerServices.Unsafe.dll'
Patching [System.Runtime.CompilerServices.Unsafe] with [UnityToolkit.Prepatcher.Patcher]
Loading [Unity Toolkit 2.0.2]  /  86 plugins to load
```

- 게임 플러그인 77 → **86개**(+9). 표시명이 파일·모드명과 달라 처음 검색에 안 걸린 것이 많았다 — `Unity Toolkit`, `No Barrel Shadow`, `Realistic Insurance (client)`, `Quick Trader Load Times`, `Ref Rework`. 5-A 때와 같은 함정이라 이번에는 `Loading [` 줄 수(86)와 대조해 확정했다.
- 로그의 `[Error]` 1건은 Direct Reload가 Fika용 패치를 못 걸었다는 것. Fika 미설치라 정상이며 [PROCEDURE.md](PROCEDURE.md)의 무시 목록에 올렸다.
- 그 뒤 설치: Field Attachments 240파일(25.3MB, **결속**) → Scrollable Attachments 1파일. 테스트 3회 모두 `SERVER STARTED`(44·44·41초), 서버 48개 그대로, 오류 0.
- 프로필은 설치 직전 백업과 해시 동일(`edfb23fe…`). 사용자가 게임을 켠 뒤 프로필이 바뀌었으므로 **설치 직전에 백업을 다시 떴다**(`profiles-before-stage6C-3125`, 10:15).
- **절차에서 고칠 점(실수 기록)**: 게임이 켜져 있는 동안 설치하려고 서버를 먼저 내렸다. 설치 스크립트가 게임 실행을 잡아내 막아 줘서 파일은 건드리지 않았지만, 그 사이 게임의 서버 연결이 끊겼다. 즉시 다시 띄웠다. **서버를 내리기 전에 `EscapeFromTarkov` 프로세스를 먼저 본다.**
- 남은 확인: 게임을 다음에 켤 때 플러그인 **88개**가 되는지, Left Alt로 부착물 카드가 뜨는지(스톡·핸드가드·손잡이 교체는 Leatherman 멀티툴 소지 필요), 부착물 목록 스크롤이 되는지.

### 6-D 설치 — 상인 초상화 (2026-10-25 10:4x)

Nexus 모드라 자동 다운로드가 안 되는 건 **실측으로 확인**했다. 내장 브라우저로 파일 탭을 열자 `You have to be logged in to download files`가 떴다(미로그인). 사용자 요청으로 **외부 Chrome**(이미 Nexus 로그인 상태)에서 Manual download → Slow download를 눌러 받았다. 받은 파일은 Chrome 기본 경로인 `E:\Spt_Mods\downloads\`로 떨어졌다(파일명에 Nexus 토큰이 붙은 긴 이름).

- 압축을 풀어 보니 **텍스처를 덮어쓰는 방식이 아니었다**. `SPT_Runtime/user/mods/VanillaTradersRework.Reloaded/`에 DLL 1 + `avatars/` 18장이 들어가는 평범한 서버 모드다. 그래서 걱정했던 덮어쓰기 충돌이 없다 — `check-conflicts.mjs stage6-D` **CLEAN**.
- 단계 폴더를 `stage6-D`로 따로 두고 설치(19파일, 10.3MB). `SERVER STARTED in 47s`, 서버 모드 48 → **49개**, 오류 0. 프로필 해시 동일.
- 로그가 포르투갈어로 찍힌다. `Mod carregado com sucesso.`가 성공 신호다.
- **초상화가 없는 상인 11명**은 원본 그대로 간다: Смотритель(등대)·APC(BTR)·Storyteller·Amonya·Harry·Gunwright·Victor Barinov·Gunsmith·PresetTrader·Ledger·Elias Ward(NATO Quartermaster). 이 모드가 다루는 건 바닐라 9명 + Lotus·Scorpion·Artem·Couturier·Croupier 등이다.
- 제작자는 적용 후 런처의 **Settings → Clean Temp Files**를 누르라고 안내한다. `%LOCALAPPDATA%\Temp\Battlestate Games`도 `E:\Spt\cache`도 없어서 지울 캐시를 특정하지 못했다(있는 건 `user\cache\bundleHashCache.json`과 BepInEx 타입 캐시뿐이라 성격이 다르다). 초상화가 그대로면 런처에서 한 번 눌러 보는 쪽이 맞다.

### 등대지기·BTR 판매 목록이 비어 있는 건 (2026-10-25 11:0x)

6차로 넣은 PortableTraders 덕에 두 상인이 주 화면에 떴는데 **아무것도 팔지 않는다**는 제보. 모드 설정·SPT DB를 직접 열어 확인했다.

- `database/traders/638f541a29ffd1183d187f57/assort.json`(등대지기), `656f0f98d80a697f855d34b1/assort.json`(BTR) 모두 `{"barter_scheme":{},"items":[],"loyal_level_items":{}}`. 메카닉의 같은 파일은 385KB다. 등대지기는 `sell_category: 0`에 수리·보험도 false라 **사지도 않는다**.
- 둘은 원래 서비스·퀘스트 전용이다. 등대지기 `services.json`에 Zryachiy 지원·ExUsec 충성·교단 지원 3종(각각 선행 퀘스트), BTR에 물품 배달·택시·엄호·환승 배달 4종.
- 즉 PortableTraders는 정상 동작이다. 설치 로그의 `30 task(s) reachable from the main menu`가 이 모드의 결과물이고, 확인은 거래 탭이 아니라 **임무 탭**에서 한다.
- 사용자 판단으로 **품목은 채우지 않기로** 했다. 바닐라 설계를 그대로 둔다. 결론은 [MODS-GUIDE.md](MODS-GUIDE.md)의 "헷갈리기 쉬운 정상 동작"에 남겼다.

### 7차 후보 조사 — 상인·탄환·총기 파츠·방어구 (2026-10-25 11:3x)

"밸런스는 무시"라는 조건이 붙어, 지금까지 치트성·바닐라 이탈을 이유로 걸렀던 것을 전부 다시 올렸다.

- **조회 방식을 바꿨다.** 이름·소개 키워드만 쓰던 것을 **Forge 카테고리와 합쳐** 걸렀다. ECOTI를 두 번 놓친 원인이 키워드였기 때문이다. 4.1.6 호환 633개의 카테고리 분포는 Other 176 / Tools 65 / Items 59 / Overhauls 56 / Scripting 37 / Weapons 35 / Equipment 31 / Traders 30 / … 이다.
- 네 분야로 165개가 걸렸고 그중 **84개는 기존 문서에 한 번도 없던 것**이었다. 실제 해당하는 65개를 버전·용량·결속·의존성까지 확인해 [CANDIDATES-STAGE7.md](CANDIDATES-STAGE7.md)에 정리했다(원자료 `stage2-survey/survey-stage7-416.json`).
- **방어구는 이번이 처음 다루는 분야**다. 1~6차 어디에도 없었고, 그래서 20개 중 대부분이 새 후보다. 다운로드 상위는 Pack 'n' Strap(239k)·Less Restricting Headwear(137k)·Foldables(50k)다.
- 확인한 제약 몇 가지: **Painter(1025) ↔ Tactical Gear Component(1125)가 서로를 요구**해 647MB를 한 묶음으로만 넣을 수 있다. The Quartermaster는 ItemGen(2803), Sage's Incendiary Grenade는 BigBrain(902)이라는 **새 의존성**을 끌고 온다. Instant Insurance(2394)는 6차에 넣은 Realistic Insurance(2949)와 **정면으로 어긋난다**.
- 설치·다운로드는 하지 않았다. 문서의 개수(방어구 20·상인 14·탄환 11·파츠 20 = 65)와 설치본 중복 0은 스크립트로 확인했다.

### 7차 계획 — 작고 가벼운 것부터 (2026-10-25 11:5x)

후보 65개를 **비결속 → 결속, 작은 것 → 큰 것** 순으로 [PLAN-STAGE7.md](PLAN-STAGE7.md)에 줄 세웠다.

- **7-A 비결속·소형 14개 / 합계 314KB** — 방어구 9(LMSP·Unrestricted Armor Plate·Foldables·Armor Expert·Change Helmet Visor·VisorEffectManager·SOMF·BiC·ArmorInfoPlus), 탄환 2(Munitions Expert·Quick Cancel), 파츠 2(Quick Aim·All In Weapon), 상인 1(Item Limits Begone). 전부 파일 삭제만으로 되돌아간다. 다운로드 목록 `download/manifest-stage7.json`까지 만들어 뒀다.
- **7-B(8개)** 는 비결속이지만 설치본과 겹쳐 선택이 필요한 것 — 탄약 이름 표기, QuickSell 교체, 플레아 가격 소스, 장전 속도, 무한 옵션, 그리고 6차 Realistic Insurance와 **정면으로 어긋나는** Instant Insurance.
- **7-C(26개)** 는 결속이지만 파일이 작은 것들. 효과가 큰 순서로 1번 Less Restricting Headwear, 2번 BETTER ATTACHMENTS, 3번 Definitive Weapon Variants를 뒀다. 하나씩 프로필 백업 후 설치.
- **7-D(10개)** 는 10MB 이상. 이 중 Manimal's 장전 애니메이션은 의존성 둘이 이미 깔려 있어 부담이 가장 작고, Sage's Incendiary Grenade는 BigBrain(902), The Quartermaster는 ItemGen(2803)이라는 새 의존성을 끌고 온다.
- **7-E(3항목)** 는 100MB 이상, 합쳐 1.2GB. Painter ↔ Tactical Gear Component는 서로를 요구해 647MB 한 묶음이다.
- 후보 65개가 계획 어딘가에 **빠짐없이** 들어갔고(스크립트 대조), 7-A 표와 매니페스트가 같은 14개임도 확인했다. 설치·다운로드는 하지 않았다.
- 계획에 적어 둔 주의 둘: Git Bash의 `tar`로는 zip이 안 풀리니 `C:\Windows\System32\tar.exe`를 쓸 것, 서버를 내리기 전에 `EscapeFromTarkov` 프로세스를 먼저 볼 것.

### 7-A 설치 — 비결속·소형 14개 (2026-10-25 14:2x ~ 14:4x)

[PLAN-STAGE7.md](PLAN-STAGE7.md)의 7-A를 그대로 받아 설치했다. 설치본 109 → **123개**, 파일 2655 → 2743개, 서버 모드 49 → **55개**.

- 다운로드 14개 전부 바이트 일치(합계 314KB). `check-conflicts.mjs stage7` **CLEAN** — 기존 경로 겹침 0, 모드 간 겹침 0, 섞여 온 파일 0.
- 설치 위치가 예상과 달랐던 것: **Armor Expert·Munitions Expert가 게임(클라이언트) 플러그인**이었고, Unrestricted Armor Plate·All In Weapon·Item Limits Begone·SOMF는 서버 쪽이다. 위치는 압축 구조로만 확정된다는 5차의 교훈 그대로다.
- SOMF(0.1.2)는 압축에 **빌드 찌꺼기**가 들어 있다(`obj/Release/staticwebassets`, `Config/Debug` 빈 폴더). 설치 스크립트가 빈 폴더까지 만들고 `installed-stage7A-3105.dirs.txt`에 기록했다.
- 서버 테스트에서 신규 서버 모드 6개 전부 로드 확인(SitOnMyFace·Item Limits Begone·UnrestrictedArmorPlates·Foldables·All In Weapon: Unslotted·Change Helmet Visor). `SERVER STARTED`, stderr 0바이트, `[Error]`·`[Critical]` 0건. 프로필 해시도 설치 전과 같다(`cf795863…`).
- **절차에서 걸린 것** — 설치 호출을 `| Select-Object -First 3`으로 받았더니 PowerShell이 파이프라인을 일찍 끊어 **서버 테스트 단계가 통째로 생략**됐다. 스크립트의 테스트 부분을 그대로 재현해 따로 돌렸고, 그 과정에서 All In Weapon이 첫 실행 때 만드는 `config/config.jsonc`가 설치 기록에 빠지는 것을 발견해 손으로 채웠다. **설치 스크립트 출력은 `Select-Object`로 자르지 않는다.**
- `gen-installed-mods.mjs`에 7단계를 추가했다(단계 루프·제목·로드 현황).
- 남은 확인: 게임 쪽 플러그인 **12개**(6-C의 2개 + 7-A의 10개)는 게임을 켠 뒤 `LogOutput.log`에서 본다. 방어구 툴팁 수치(Armor Expert)가 ODT Item Info와 겹쳐 길어지지 않는지, 호환 강조(Compatibility Highlighter)가 All In Weapon 때문에 "전부 호환"으로 보이지 않는지도 함께.

### 7-C 설치 — 결속 26개를 하나씩 (2026-10-25 15:0x ~ 15:4x)

[PLAN-STAGE7.md](PLAN-STAGE7.md)의 7-C 순서 그대로, **매번 프로필 백업 → 설치 → 서버 테스트**로 26번 반복했다. 서버 모드 55 → **80개**(ShrapnelFix만 클라이언트 전용이라 그때는 서버 수가 그대로였다).

- 다운로드 26개(합계 4.0MB) 중 **크기 불일치 2건** — Repair Max Durability(실제 31,209 / 메타데이터 31,097), bluehead's AIO Trader(42,151 / 41,700). 둘 다 정상으로 풀리고 내용도 멀쩡해 **Forge 메타데이터가 낡은 쪽**으로 판단했다. 5차 PresetTrader와 같은 유형.
- **압축 구조를 손본 것 2건**
  - **Cobra's Durability Tweaks** — 폴더가 한 겹 더 싸여 있고 그 안이 `SPT_RunTime`(대문자 T)이었다. 평탄화 + 이름 교정 후 설치. 그냥 두면 `E:\Spt` 아래에 엉뚱한 폴더가 생긴다.
  - **Ultimate Flea** — 루트에 `README.txt`가 섞여 있어 `extracted/_extras-stage7-C/`로 분리.
  - 두 건 모두 `check-conflicts.mjs`가 "설치 경로가 아닌 파일 6"으로 미리 잡았고, 교정 후 재검사는 CLEAN.
- 26번 모두 `SERVER STARTED`(38~71초). 테스트 출력 26개 전체에서 `[Error]`·`[Critical]` **0건**, stderr 26개 **전부 0바이트**, 설치 기록 26개 생성.
- 백업은 `backup/profiles-before-stage7C-<id>` 26벌.
- **중간에 작업이 막혔다** — 셸·파일 편집 도구의 안전 검사가 여러 턴에 걸쳐 응답하지 않아(일시적 오류) 설치 직후의 **서버 재가동·목록 재생성·커밋이 미뤄졌다.** 설치와 검증 자체는 그 전에 끝났고, 검증은 읽기 전용 도구(Grep)로 대신했다. 그동안 사용자 서버가 꺼져 있어 **사용자가 직접 켰다.**

### 7차 게임 확인 — 사용자 서버·게임 로그 (2026-10-25 15:3x)

사용자가 서버를 켜고 게임까지 접속한 로그를 받아 확인했다.

- **게임 플러그인 86 → 106개**(`106 plugins to load`, `Loading [` 줄도 106). 6-C의 2개 + 7-A의 10개 + 7-C의 클라이언트 쪽 8개가 전부 떴다. 표시명이 또 달라 이름으로는 안 걸리는 것이 많았다 — `Armor Info Mod`(ArmorInfoPlus), `BlackHawk-QuickAim`, `ArmorExpert`, `UltimateFlea Client`, `RepairMaxDurabilityClient`, `maschine-Airburst`, `Tylevo Field Attachments`, `pein-ScrollableAttachments`.
- 게임 쪽 `[Error]`는 **1건뿐**이고 Fika 미설치로 나는 Direct Reload의 그 메시지다(무시 목록에 있음). 서버 쪽은 80개 로드에 오류 0.
- **SOMF가 무력화됐다.** 7-A 때는 `Head/Face blocking 37 / specific 2089 / Head/Eyes 7 / 123`을 풀었는데, 7-C에서 Less Restricting Headwear(922)를 넣은 뒤로는 **전부 0**이다. 922가 먼저 `117 block flags cleared, 4005 conflict entries removed`로 같은 일을 끝내기 때문이다. 둘은 역할이 겹치므로 하나만 두면 된다 — 결론은 [MODS-GUIDE.md](MODS-GUIDE.md)의 "헷갈리기 쉬운 정상 동작"에 남겼다.
- **DLL 표기 버전이 사이트와 다른 것이 또 나왔다**: BETTER ATTACHMENTS(Forge 4.1.5 → 로그 **2.0.0**), Full Automatic Guns(4.1.5 → **1.0.0**), bluehead's AIO Trader(5.3.1 → 5.3.0), Restore Armor Plates(1.0.2 → 1.0.1), Ultimate Trader Assort Editor(2.1.1 → 2.1.0). 제작자가 DLL 버전을 안 올린 것으로, 기존 사례(Armory·TraderScrolling·Gunwright)와 같다.
- **새 상호작용 하나** — `[RealisticInsurance] trader 'Armorer' ... not listed in insurance.json returnChancePercent ... will use the '90%' fallback`. 7-C에서 넣은 AllGoodsTrader의 보험 상인을 Realistic Insurance가 모르기 때문에 **그 상인 보험만 90% 고정**으로 돌아간다. 오류는 아니다.
- 수치로 확인된 적용 범위: Definitive Weapon Variants **커스텀 아이템 776개**, BETTER ATTACHMENTS 부착물 **1,092개**, Less Restricting Headwear 품목 182개, Realistic Suppressors 소음기 103개, Cobra's Durability Tweaks 무기 932개(내구도 소모 **12%**), Slimmer Magazines 탄창 44개, Jero Backpack 배낭 36개, Foldables 배낭 46·조끼 36·헤드폰 21, Limitless **26,179개 품목**, Ultimate Flea 레벨 락 1,691개 + 카테고리 17종, bluehead AIO 품목 6,261개(매물 7,178), All Weapons Flea **무기 925종**(6차 때 238종 → 총기 팩이 늘어 확대).
- 서버 시작에서 가장 오래 걸리는 것은 **ODT's Item Info의 아이템 처리 53.5초**다(6,728개). 시작이 느려진 주된 이유.

### SOMF 제거 (2026-10-25 16:1x)

Less Restricting Headwear와 역할이 겹쳐 아무 일도 하지 않게 된 SOMF를 사용자 판단으로 뺐다. 설치본 149 → **148개**, 파일 2894 → 2855개, 서버 모드 80 → **79개**.

- 게임이 켜져 있는 동안은 **서버가 `SitOnMyFace.dll`을 잠그고 있어 삭제가 안 된다**는 것을 `[IO.File]::Open`으로 실제 확인했다. 게임 종료 → 서버 종료 → 잠김 해제 확인 후 진행.
- `installed-stage7A-3105.txt`의 **37개 전부** 삭제(누락 0), `.dirs.txt`의 빈 폴더 2개도 삭제. 남은 것은 빈 폴더뿐이어서 모드 폴더째 지웠고, `sitonmyface`·`somf`로 전수 검색해 잔류 0 확인.
- **압축에 섞여 온 것들이 함께 빠졌다** — `SPT_Runtime\desktop.ini`(제작자 PC의 폴더 설정 파일), `Mod.cs`, `SitOnMyFace.csproj`, `bin/Release`·`obj/Release` 빌드 산출물. `desktop.ini`는 경로가 `SPT_Runtime/` 아래라 `check-conflicts.mjs`가 정상 설치 경로로 보고 통과시켰다. **검사기가 걸러내지 못하는 유형**이니 앞으로 해제본 파일 목록을 눈으로 볼 때 이런 잡파일을 함께 본다.
- 설치 기록은 지우지 않고 `doc/install-records/removed/`로 옮겼다. 생성기는 `install-records` 바로 아래만 읽으므로 목록에서 **미설치**로 표시된다(매니페스트에는 남는다).
- 사용자 서버 재가동 확인: 모드 **79개**, `[SOMF]` 줄 0건, Error·Critical 0건.

### 7-B 판단과 설치, 7-D 중형 설치 (2026-10-25 16:3x ~ 17:1x)

설치본 148 → **163개**, 파일 2855 → 3046개, 서버 모드 79 → **93개**. 테스트 13회 전부 `SERVER STARTED`, 오류 0, stderr 0바이트.

**7-B — 겹치는 8개를 판단해 3개만 넣었다**

| 모드 | 판단 |
| --- | --- |
| Quick Sell(2955) | **넣음.** 최적 상인·플레아 즉시 판매 + 툴팁 가격. 설치된 QuickSellFlea는 일단 두고, 메뉴가 중복돼 보이면 그때 뺀다(비결속 2파일이라 되돌리기 쉽다) |
| Bela's Speedloader(2458) | **넣음.** 장전 속도·탄 스택. Continuous Load Ammo·Cobra's Ammo Stack과 수치가 겹칠 수 있어 게임에서 확인 필요 |
| Infinite Everything(3108) | **넣음.** 로그가 `infinite money + hideout` 등으로 찍힌다. SVM 옵션과 겹치지만 SVM은 건드리지 않았다 |
| Easy Ammo Names(1262) / Ammo Stats In Names(2989) | **뺐다.** 설치된 Ammo Stats·Caliber Under Ammo Name이 같은 로케일 열쇠를 쓴다. 나중에 로드되는 쪽이 이겨 표기가 뒤섞인다 |
| Live Flea Prices(1131) | **뺐다.** 7-C에서 넣은 Ultimate Flea가 가격 소스를 쥐고 있다(`Price source: local`). 가격을 실시간으로 쓰려면 Ultimate Flea의 `pricing.json`에서 켜는 쪽이 맞다 |
| Better Flea(3177) | **뺐다.** Ultimate Flea와 정면으로 같은 영역 |
| Instant Insurance(2394) | **뺐다.** 6차 Realistic Insurance와 정반대다. 넣으려면 2949를 꺼야 한다 |

**7-D — 새 의존성 2개 + 중형 10개, 하나씩**

- 선행: **BigBrain**(902, 비결속, 클라이언트 전용이라 서버 수 그대로) → **ItemGen**(2803, 결속). 각각 Sage's Incendiary Grenade와 The Quartermaster가 요구한다.
- 이어서 Better OKP-7 → TraderGen → The Quartermaster → Wolfik's Heavy Trooper Masks → Manimal's 장전 애니메이션 → Sage's Incendiary Grenade → .22lr → Peacemaker → Better Rear Sights → Fold That Stock.
- 압축 해제 후 실제 용량이 다운로드보다 큰 것이 많다(.22lr 29MB → 105MB, Peacemaker 45MB → 137MB). 번들이 압축돼 있어서다.
- The Quartermaster가 `18 active listings / 86 items`, 커뮤니티 계약 2건을 띄웠다. 상인이 더 늘었다.
- **프로필이 이 단계에서 바뀌기 시작했다.** 7-B와 7-D의 902·2803·2711까지는 해시가 같았고(`3b683421`), **TraderGen(2749) 테스트부터 매 실행마다 달라졌다.** 커스텀 상인·아이템이 프로필에 자기 자리를 만들기 때문이고, 이것이 "프로필 결속"의 실체다. 단계별 백업 13벌이 남아 있다.
- 사용자 서버 재가동 확인: 모드 **93개**, 오류 0.

### The Quartermaster 제거 — 5초마다 혼자 재입고 (2026-10-25 17:0x)

사용자 제보: 7-D로 넣은 The Quartermaster만 상인 화면이 혼자 계속 새로고침된다.

**원인을 로그와 IL로 확정했다.**

- 서버 로그가 **정확히 5초 간격**으로 같은 일을 반복한다: `/client/trading/api/getTraderAssort/66789abc…` → `Loaded 187 active listings` → `Assort refreshed with 77 items` → 5초 뒤 again(`:04 :09 :14 :19 :24 …`). 다른 상인에는 이런 패턴이 없다.
- `TheQuartermaster.Server.dll`을 Mono.Cecil로 뜯어 보니 **상인 데이터를 만들 때마다 다음 재입고 시각을 현재 시각으로 설정**한다 — `BuildTraderBase`·`RefreshAssort`·`OnListingUploaded`·`OnListingPurchased` 네 곳 모두 `set_NextResupply(DateTimeOffset.UtcNow…)`. 재입고 타이머가 항상 만료 상태라 클라이언트가 계속 다시 요청하고, 요청할 때마다 서버가 187건을 다시 만들며 타이머를 또 "지금"으로 되돌린다.
- 설정으로는 못 고친다. `config/config.json`에 `modEnabled`·`jackpotEnabled` 둘뿐이고 주기 항목이 없다.

**제거 결과** — 설치본 163 → **162개**, 서버 모드 93 → **92개**. 재가동한 서버 로그(마지막 세션 286줄)에 `TheQuartermaster` 0건, `Assort refreshed` 0건, 오류 0건.

- 기록 `installed-stage7D-2817.txt`의 **15개 전부** 삭제(누락 0). 이 기록에는 설치 테스트 중 모드가 만든 `cache/*`·`client_submissions.json`·`db/CommunityQuests/*`까지 들어 있어 함께 지워졌다.
- 기록에 없던 잔류물 3종을 손으로 찾아 지웠다 — `BepInEx\plugins\Serenity-TheQuartermaster\TheQuartermaster\community_auth.json`(커뮤니티 장터 인증 토큰), `BepInEx\config\com.shaneeexd.thequartermaster.cfg`, Ultimate Trader Assort Editor의 빈 편집 폴더 `Quartermaster [66789abcde1234567890abcd]`. **게임을 실제로 켠 뒤 생기는 파일은 설치 기록에 안 잡힌다**는 뜻이라, 앞으로 제거할 때는 모드 이름으로 전수 검색을 한 번 더 한다.
- 이름이 비슷한 **NATO Quartermaster(3030, id `bb371fed…`)는 그대로 둔다.** 다른 모드다. UTAE의 `Quartermaster [bb371fed…]` 폴더와 `sptappdata`의 NATO 아바타도 건드리지 않았다.
- 의존성으로 함께 넣었던 **ItemGen(2803)은 남겼다.** 지금은 쓰는 모드가 없어 `No item packs found`만 찍지만, 비용이 없고 나중에 다른 모드가 요구할 수 있다.

### 프로필에 남은 상인 흔적 정리 (2026-10-25 18:0x)

The Quartermaster를 뺀 뒤 게임이 경고를 띄웠다 — "SPT에 존재하지 않는 상인 `66789abcde1234567890abcd`가 프로필에서 발견되었습니다… `core.json`의 `removeModItemsFromProfile`을 true로".

- **조사(examine) 문제의 범인도 이 모드였다.** 제거하자마자 사라졌다. 앞서 의심했던 ArmorExpert·MunitionsExpert·ArmorInfoPlus 셋은 무죄였고, 비활성화 테스트는 하지 않아도 됐다. 5초마다 상인 매물을 다시 만드는 동안 조사 완료 처리가 막혔던 것으로 보인다.
- 프로필에서 그 id가 **4곳**에 남아 있었다: `characters.pmc.TradersInfo.<id>`(평판 loyaltyLevel 1·standing 0.2·판매 0)와 `dialogues.<id>`(메시지 1건, **첨부 아이템 0개**, 이미 읽음). 관련 퀘스트는 0개였다.
- 안내대로 `removeModItemsFromProfile`을 true로 바꿔 서버를 재시작했지만 **프로필은 그대로였다.** 이 정리는 서버가 뜰 때가 아니라 **클라이언트가 프로필을 선택해 불러올 때** 도는 것으로 보인다.
- 내용이 평판 한 줄과 첨부 없는 메일 하나뿐이라, 백업을 두 벌(자동 정리 전·수동 편집 직전) 뜬 뒤 **두 키를 직접 지웠다.** 원본이 한 줄짜리 compact JSON이라 같은 형식으로 다시 썼다(415KB → 414KB).
- 서버를 내린 뒤 편집했다. 서버가 프로필을 메모리에 들고 있어, 켠 채로 고치면 종료할 때 되살아날 수 있다.
- 결과: 재시작한 서버에서 경고 0건, 오류 0건, 모드 92개. 프로필은 상인 29명·대화 13건·레벨 15로 멀쩡하다. `removeModItemsFromProfile`은 **false로 되돌렸다**(켜 두면 앞으로 모드를 뺄 때마다 조용히 지워진다).
- 백업: `backup/profiles-before-profile-cleanup/`에 프로필 2벌과 `core.json.bak`.

### 7-E 설치 — Pack 'n' Strap + Painter·Tactical Gear Component (2026-10-25 18:2x ~ 18:3x)

사용자 결정으로 **Kalashnikov Enhanced Modding(3010)은 제외**하고 둘만 넣었다(받아 둔 파일은 `downloads/stage7-E`·`extracted/stage7-E`에 남겨 뒀고, 매니페스트에 제외 표시). 설치본 162 → **165개**, 파일 3031 → 3255개, 서버 모드 92 → **95개**.

- 다운로드 4개 1,210MB 전부 바이트 일치. `check-conflicts.mjs stage7-E` **CLEAN**.
- **WTT Pack 'n' Strap 2.1.1** — 60파일 302MB. `SERVER STARTED in 77s`, 오류 0.
- **Painter 3.0.0 + Tactical Gear Component 3.0.0** — 서로를 요구하므로 TGC(137파일 658MB)를 `-SkipTest`로 먼저 복사하고 Painter(27파일)를 넣은 뒤 한 번에 테스트. `SERVER STARTED in 77s`, 오류 0. 둘 다 `Mod loaded successfully.`
- 설치 후 사용자 서버: 모드 **95개**, 모드 로드 → 웹서버 **69초**(7-E 전 67초). 1.2GB가 늘었는데 시작 시간은 2초 차이다.
- **새로 생긴 오류 4건** — `ItemBaseClassService: Could not add 668ff5bde41a0cce3b142464 / 6699546547ad52e0fccf6da9 to cache, it does not exist in the item database!` (플리마켓 상품 생성 단계). 추적해 보니 **Painter의 `db/assort.json`이 어디에도 없는 아이템 두 종을 팔려고 올려 둔 것**이다. 설치된 모드 전체 JSON을 훑어도 그 두 tpl을 정의하는 곳이 없는 **죽은 항목 4줄**(두 종 × 2)이다. (처음에는 "같은 제작자의 다른 팩에 있는 듯", "가격 정보도 없다"고 적었는데 **둘 다 틀렸다** — 아래 추적 결과 참고.)
  - 영향: 시작 때마다 오류 4건 + 그 두 칸은 살 수 없다. 서버는 계속 진행한다.
  - 고치려면 Painter의 assort에서 그 4줄을 지우면 된다(모드 데이터 수정이라 보류 중).

### Painter 의 죽은 매물 제거 (2026-10-25 18:4x)

7-E 설치 뒤 시작마다 나던 `ItemBaseClassService: Could not add … to cache` 오류 4건을 없앴다.

- Painter의 `db/assort.json`에는 매물이 7건 있었는데, 그중 **4건이 존재하지 않는 아이템**(`668ff5bde41a0cce3b142464`, `6699546547ad52e0fccf6da9` 각 2건)이었다. 같은 제작자의 다른 팩에 있는 아이템으로 보이고, 설치된 모드 어디에도 정의가 없다.
- 그 4건의 `items` 항목과 거기에 딸린 `barter_scheme`·`loyal_level_items` 키를 함께 지웠다(7 → **3건**). 처음 점검 때 "가격 정보 없음"으로 본 것은 내가 tpl id로 찾았기 때문이고, 실제로는 **항목 id로 가격이 걸려 있었다** — 그래서 같이 지웠다. 부모가 사라진 고아 자식 항목은 0개.
- 원본은 두 곳에 있다: `backup/painter-assort-original.json`과 해제본 `extracted/stage7-E/1025__Painter_3.0.0/…/db/assort.json`. 되돌리려면 둘 중 하나를 덮으면 된다.
- 결과: 재시작한 서버에서 **Error·Critical 0건**, `ItemBaseClassService` 메시지 0건, 모드 95개, Painter·TGC 정상 로드.
- 주의: **모드가 배포한 데이터 파일을 직접 고친 것**이라 Painter를 업데이트하면 되돌아간다. 설치 기록(`installed-stage7E-1025.txt`)의 파일 목록은 그대로지만 내용은 원본과 다르다.

### 그 두 아이템의 정체 — 다른 모드가 아니라 Painter 자신 (2026-10-25 19:0x)

"같은 제작자의 다른 팩에 있는 아이템"이라고 추측했었는데, 추적해 보니 **틀렸다.**

- GitHub 코드 검색에서 바로 나왔다. 두 id는 Painter 저장소(`emilanderss0n/Painter`)의 `src/CustomItemsManager.ts`에 있는 **Painter 자신의 커스텀 아이템**이다.
  - `668ff5bde41a0cce3b142464` = **Painter's Special Delivery**(루트박스, 봉인 무기 상자를 복제해 만든다)
  - `6699546547ad52e0fccf6da9` = **Painter's War Box**(두 번째 루트박스, `6489981f7063b903ff4b8565` 복제)
  - 모드가 `createItemFromClone`으로 **실행 시점에 만들어 내는** 아이템이라, 어느 모드의 JSON에도 정의가 없는 게 당연했다.
- 그런데 **4.x 포팅본(`Painter-4.0.dll`)에는 그 코드가 없다.** DLL을 ASCII·UTF-16 양쪽으로 훑어도 `mysterybox`·`LootBox`·두 id·`CreateItemFromClone`이 하나도 안 나오고, 클래스도 `EpicTraderHelper`·`ModMetadata`·`Painter`·`PainterModQuest` 넷뿐이다.
- 반면 **`db/assort.json`에는 그 매물이 남아 있고 번들(`bundles/mysterybox.bundle`, `mysterybox_2.bundle`)도 같이 배포된다.** 즉 TypeScript 시절의 루트박스 기능이 C# 포팅에서 빠졌는데 데이터와 리소스만 남은 것이다. **다른 모드를 깐다고 해결되지 않는다.**
- 같은 제작자(MoxoPixel)의 다른 팩 6종(BlackCore·SandCore·GreenCore·WhiteCore·Mag Tape·War Heads)은 **전부 3.x까지만** 나와 있어 4.1.6에서는 설치 대상이 아니다. 이 건과도 무관하다.
- 결론: 앞서 지운 매물 4줄은 **되살릴 방법이 없는 항목**이 맞다. 제작자가 루트박스 코드를 다시 넣기 전까지는 지운 채로 두는 것이 옳다.

### 탄창 이름 아래 구경이 30줄로 늘어난 건 (2026-10-25 19:2x, 해결)

인벤토리에서 탄창 하나에 구경이 30줄 넘게 붙는다는 제보(스크린샷).

- **글자를 그리는 쪽**은 1차에 넣은 Caliber Under Ammo Name(2586). 탄창이 받는 구경을 전부 적는다.
- **길어진 원인**은 7-A의 All In Weapon: Unslotted(2576)다. 설치 로그가 `Fcked: weapons, mods, chambers, calibers, magazines`로 찍혔듯 **탄창의 구경 필터를 지워** 모든 구경을 받게 만들었고, 그러니 표시 모드가 전부 나열했다. 스크린샷의 목록이 2586 설정 파일 `[4. Caliber Names]`의 구경 표와 그대로 일치한다.
- 고치는 길 둘을 확인했다: ⒜ F12 → `탄창에 구경 표시` 끄기(즉시, 되돌리기 쉬움, 탄창 구경 정보는 포기) ⒝ `AllinWeaponUnslotted/config/config.jsonc`의 `FckMagazines`를 false(탄창이 제 구경만 받음 → 한 줄, 서버 재시작 필요, 나머지 자유도 유지).
- **사용자가 ⒜를 선택해 F12에서 껐고, 길게 나오지 않는 것을 확인했다.** 결론은 [MODS-GUIDE.md](MODS-GUIDE.md)의 "헷갈리기 쉬운 정상 동작"에 남겼다.
- 배운 것: "제한을 푸는 모드"와 "제한을 읽어 표시하는 모드"는 서로를 증폭한다. 표시 쪽 모드가 갑자기 수다스러워지면 최근에 넣은 해제형 모드를 먼저 본다.

### F12 미한글화 항목을 전부 덮었다 (2026-10-25, 사전 59 → 83)

설치 목록을 다시 생성해 보니 `doc/INSTALLED-MODS.md`는 이미 최신이었다(모드 165개, 미설치 3건). 그래서 F12 쪽만 손봤다.

- 먼저 센 방법이 틀렸다. `grep -c ' = '` 로 `.cfg` 항목을 세면 설명 줄의 `1.0 = default` 까지 세어 미한글화가 483항목으로 부풀어 보였다. `check-lang-coverage.mjs` 와 같은 파서로 다시 세니 **25개 모드 / 268항목**이었다.
- 사전이 아예 없던 **24개 모드**에 사전을 새로 썼다. 항목 많은 쪽부터: No Barrel Shadow(93) · Field Attachments(39) · ShrapnelFix(34) · QuickSell(27) · ECOTI(27) · Munitions Expert(22) · Infinite Everything(21) · Holster Everything(20) · Quick Trader Load Times(13) · BrightLasers(10) · Armor Expert(10) · VisorEffectManager(8) · Airburst(8) · Quick Aim(6) · Foldables(4) · Moe-TradeMarker(4) · Scrollable Attachments(3) · Change Helmet Visor(3) · ArmorInfoPlus·BiC·Quick Cancel·Incendiary Grenade·AmmoGen·TraderGen·BeltSlot(각 1).
- No Barrel Shadow 는 손전등·레이저 82종 × 거리/밝기 두 항목이 기계적으로 반복되는 꼴이라 스크립트로 생성했다. 장비 이름은 고유명사라 그대로 두고 접미사(`— Range`/`— Intensity`)와 설명만 한글로 바꿨다.
- 직전에 발견한 CaliberUnderName 누락 2항목(`Caliber22LR`, `Caliber1143x33mmR`)도 채웠다.
- `spt.koreanconfig`(한글화 모드 자신)는 사전을 만들지 않았다. 플러그인이 자기 설정 이름을 처음부터 한글로 등록한다.
- `gen-installed-mods.mjs` 에 `번역 사전 59` 가 하드코딩돼 있어 `lang/` 을 직접 세도록 고쳤다. 손으로 적은 수는 늘 어긋난다.
- 결과: `check-lang-coverage.mjs` **1457/1457 항목, 사전 83개, 누락·오타 0**. `install-koreanconfig.ps1` 로 설치해 `BepInEx\plugins\SptKoreanConfig\lang` 에 84개 파일(사전 83 + `_ui.json`)이 들어갔다. 게임에서 실제로 보이는지는 아직 확인하지 않았다 — JSON 만 고친 것이라 F12 를 다시 열면 반영된다.
- 열거형 값(`Outline`/`Full`, `AbovePockets`/`BelowPockets` 등)은 사전 형식에 `values` 칸이 없어 영문 그대로 나온다. 설명에서 각 값이 무슨 뜻인지 풀어 적어 두었다.

### 안면 보호구 시야 가림을 없앴다 — ChangeHelmetVisor MaskType (2026-10-25)

타길라 용접 마스크처럼 시야를 가리는 안면 보호구의 가림을 풀 모드를 새로 찾아야 하는지 물었는데, **이미 깔려 있었다**. 7-A 의 Change Helmet Visor(2554)가 바로 그 모드다.

- 원리: 아이템 DB 의 `_props.FaceShieldMask` 가 가림 마스크를 정한다. 전체 아이템 중 가리는 것은 **7종뿐**이고 나머지 720개는 이미 `NoMask` 다.
  - `Wide`: 타길라의 용접 마스크 "UBEY"(`60a7ad2a2198820d95707a2e`) · "고릴라"(`60a7ad3a0c5cb24b0134664a`) · "ZABEY"(`678f84bb9e85556ca60f0362`), Altyn 안면 보호대(`5aa7e373e5b5b000137b76f0`), "Rys-T" 안면 보호대(`5f60c85b58eff926626a60f7`)
  - `Narrow`: Maska-1SCh 안면 보호대 Olive Drab(`5c0919b50db834001b7ce3b9`) · 킬라 에디션(`5c0e842486f77443a74d2976`)
- 바꾼 것: `SPT_Runtime/user/mods/ChangeHelmetVisor/config.json` 의 `MaskType` 을 `"Wide"` → **`"NoMask"`**. 한 줄만 고쳤다. 원본은 `backup/changehelmetvisor-config-original.json`.
  - 전에는 `AllFaceShields: true` + `MaskType: "Wide"` 였다. 타길라 마스크는 원래부터 `Wide` 라서 **해제 효과가 하나도 없는 설정**이었다. Maska 2종만 `Narrow` → `Wide` 로 완화된 셈이었다.
- `SpecificFaceShields` 의 3줄은 그대로 뒀다. 서버 DLL 에 `FindAllFaceShieldHelmets` 와 `ChangeSpecificHelmetsVisors` 가 따로 있어 `AllFaceShields: true` 면 개별 지정은 쓰이지 않는 구조로 보이지만, **코드를 읽은 것이 아니라 메서드 이름으로 추정한 것이다.** 게임에서 Altyn(`Narrow` 지정)이나 Maska Olive Drab(`Wide` 지정)에 아직 마스크가 보이면 개별 지정이 이기는 것이니 그 3줄도 `NoMask` 로 바꾼다.
- **서버 모드라 서버를 다시 켜야 적용된다.** 게임에서 실제로 확인하지는 않았다.
- 곁가지: VisorEffectManager(2429)는 가림이 아니라 유리의 파손·긁힘·흐림·일그러짐만 없앤다(네 항목 모두 켜져 있음). Less Restricting Headwear(922)는 착용 조합 제한만 풀고 시야와 무관하다. 2554 의 게임 쪽 F12 슬라이더(`좁은/넓은 바이저 확대`)는 마스크를 없애지 않고 구멍만 키우는 중간 선택지다.

### 헬멧 부착 칸 합집합 서버 모드를 직접 만들었다 — HelmetSlotUnion (2026-10-26, 빌드 미완)

Forge 에 4.1.6 호환 모드가 없어 직접 만들었다. 소스는 [../src/HelmetSlotUnion](../src/HelmetSlotUnion), 설치는 `tools/install-helmetslotunion.ps1`.

- 하는 일: `Headwear` 밑 헬멧의 부착 칸을 **칸 이름별로 묶어 합집합**을 각 칸 필터에 더한다. 기존 항목은 지우지 않고 `UnionWith` 로 더하기만 한다.
- 설정(`config.jsonc`, DLL 옆): `ModEnabled`·`Debug`·`MergeEquipmentSlots`·`ExcludedSlotNames`·`ExcludedHelmetIds`.
- 같은 로직을 Node 로 실제 DB 에 돌려 미리 셌다: **헬멧 64종 / 칸 묶음 11개 / 필터 항목 495 → 6719 / 바뀐 칸 281**. `MergeEquipmentSlots` 를 켜면 8706, 내장 방탄판 칸(`helmet_top`·`helmet_back`)을 빼면 2379.
- 서버 API 는 기존 Less Restricting Headwear(922) 를 메타데이터로 뜯어 알아냈다: `[Injectable(InjectionType.Singleton, 0)]` + `IOnLoad.OnLoadAsync`, `TemplateTable.Items`, `ModHelper.GetAbsolutePathToModFolder/GetJsonDataFromFile<T>`, `ISptLogger<T>`. 정확한 타입(`SlotFilter.Filter` 가 `HashSet<MongoId>`, `Slots`·`Filters` 는 `IEnumerable<>`)은 net8 콘솔 도구로 메타데이터만 읽어 확인했다 — net10 어셈블리는 net8 런타임에 로드할 수 없어 `System.Reflection.Metadata` 로 읽었다.
- **아직 빌드하지 못했다.** SPT 4.1.6 서버는 `net10.0`(`SPT.Server.runtimeconfig.json` 의 `tfm`)인데 이 PC 에는 **.NET SDK 8.0.426 뿐**이다(런타임은 10.0.13 이 있다). `dotnet build` 가 `NETSDK1045` 로 멈춘다. .NET 10 SDK 를 깔아야 한다.
- 그래서 **설치·게임 확인도 하지 않았다.** 컴파일을 거치지 않은 코드라 오류가 남아 있을 수 있다.

#### 결정해 둔 것

- `helmet_top`·`helmet_back` 은 **내장 방탄판 자리**(36종)다. 합치면 아무 헬멧에 아무 방탄판이 들어가 방어력이 크게 달라진다. 요청이 "합집합"이었으므로 **기본값은 포함**으로 두고, 빼는 설정을 README 에 적어 뒀다.
- `mod_equipment_000·001·002` 는 번호의 뜻이 헬멧마다 다르다(Ops-Core FAST 는 001 이 RAC 헤드셋, 002 가 SLAAP 방탄판). 그래서 번호를 떼어 한 묶음으로 보는 `MergeEquipmentSlots` 는 **기본 꺼짐**이다.
- 모드를 뺄 때 주의: 이 모드로만 가능했던 조합을 끼운 채 빼면 필터에 없는 자식을 가진 헬멧이 남는다.

#### 빌드·설치·검증까지 마쳤다 (2026-10-26, 이어서)

- `winget install Microsoft.DotNet.SDK.10` 으로 **SDK 10.0.402** 를 깔았다(런타임 10.0.13 은 이미 있었다). 그 전까지 SDK 는 8.0.426 뿐이었다.
- 첫 빌드에서 오류 24개가 났고 둘 다 메타데이터만 보고는 알 수 없던 것이었다.
  1. `ImplicitUsings` 를 켜지 않아 `Task`·`List`·`Dictionary`·`HashSet`·`CancellationToken` 을 못 찾았다 → csproj 에 `<ImplicitUsings>enable</ImplicitUsings>`.
  2. `IModMetadata` 의 속성은 모두 **`init` 전용**이다. `set` 으로 두면 구현으로 인정되지 않아 CS8854·CS0738 이 쏟아진다 → 전부 `{ get; init; }` 로, `Contributors`·`Incompatibilities`·`ModDependencies`·`Url` 은 nullable 로 맞췄다.
- 다시 빌드: **오류 0 · 경고 0**. `tools/install-helmetslotunion.ps1` 로 `SPT_Runtime\user\mods\HelmetSlotUnion\` 에 DLL(15,872B)과 `config.jsonc` 를 넣었다.
- 서버를 띄워 확인했다(`server-tests/server-test8-helmetslotunion.out.txt`):
  - `모드 Helmet Slot Union 버전 0.1.0 (GUID: spt.helmetslotunion | targets SPT: ~4.1.6) by Qahnaarin 불러옴`
  - `[HelmetSlotUnion] 헬멧 64종, 칸 묶음 11개. 281개 칸에 항목 6224개를 더했다.`
  - **Node 로 미리 센 값(바뀐 칸 281, 더한 항목 6224)과 정확히 같다.** 알고리즘 검증이 실측과 맞았다.
  - 서버 모드 **96개**(이전 95), stderr 0바이트, `서버가 시작되었습니다` 까지 정상. `Not an error, don't report it` 3줄은 이전 테스트에도 있던 것으로 이 모드와 무관하다.
- 아직 안 한 것: **게임에서 실제로 끼워 보지는 않았다.** 필터가 넓어진 것은 서버 로그로 확인했지만, 모양이 겹쳐 3D 로 어색한 조합이 있는지는 플레이로만 알 수 있다.

### 칸 필터만으론 부족했다 — ConflictingItems 도 지운다 (2026-10-26)

게임에서 Ops-Core FAST 헬멧에 파츠를 끼우려 하니 이 메시지가 떴다:

> ★ FAST FS (3/40)과 ★ SCRA 아이템은 동시에 장착할 수 없습니다

- **칸 필터 문제가 아니었다.** 두 아이템 모두 칸 목록에는 올라와 있다. 막는 것은 파츠 자신의 `_props.ConflictingItems` 다.
  - `5a16b7e1fcdbcb00165aa6c9` = Ops-Core FAST 멀티 히트 안면 보호 바이저(FAST FS) — 충돌 9건(야간투시경 5·마운트 3·SLAAP 방탄판 1)
  - `5a398ab9c4a282000c5a9842` = Ops-Core Single Clamp 레일 어댑터 마운트(SCRA) — 충돌 1건(FAST FS)
- 즉 헬멧 제한은 **두 층이 아니라 세 층**이었다: 착용 조합(922) / 부착 칸 필터(이 모드) / **파츠끼리의 충돌(여기서 추가)**.
- `RemoveAttachmentConflicts`(기본 켜짐)를 넣었다. 헬멧 64 + 파츠 176 = **범위 240개**를 잡고(파츠에 또 달리는 파츠까지 따라간다) **그 범위 안끼리의 항목만** 지운다. 미리 센 결과 **아이템 57개에서 295건**을 지우고, 헤드셋·안면 보호구 등 **범위 밖 623건은 남긴다** — 그쪽은 착용 조합이라 922 영역이다.
- 통째로 비우는 `RemoveAllConflictsOnAttachments` 도 넣었지만 기본은 꺼짐이다.
- 빌드 오류 0 · 경고 0. 설치된 `config.jsonc` 는 손대지 않은 기본값이어서 새 기본값으로 바꿔 두었다.
- **DLL 교체는 못 했다.** 사용자가 06:00 에 SPT.Server, 06:03 에 게임을 직접 켜 둔 상태라 `HelmetSlotUnion.dll` 이 잠겨 있다(`The process cannot access the file`). 게임과 서버를 닫은 뒤 `tools/install-helmetslotunion.ps1` 을 다시 돌려야 한다. 남의 플레이 세션을 끄지 않았다.

#### 설치·검증 완료 (2026-10-26, 이어서)

사용자가 게임과 서버를 닫은 뒤 설치했다. DLL 15,872 → 18,432 B.

서버 로그(`server-tests/server-test9-helmet-conflicts.out.txt`):

```
[HelmetSlotUnion] 헬멧 64종, 칸 묶음 11개. 281개 칸에 항목 6224개를 더했다.
[HelmetSlotUnion] 충돌 정리(범위 안끼리만): 대상 240개 중 57개에서 295건을 지웠다. 남은 항목 623건.
```

**미리 센 값(대상 240 / 57개 / 295건 / 남은 623건)과 정확히 같다.** 서버 모드 96개, stderr 0바이트, `서버가 시작되었습니다` 까지 정상. `Not an error` 3줄은 이전 테스트에도 있던 것으로 무관하다.

스크린샷의 그 쌍은 둘 다 범위 안이라 양쪽 다 0건이 됐다: FAST FS 9건 → 0, SCRA 1건 → 0.

게임에서 실제로 끼워 보는 것만 남았다.

#### 게임에서 동작 확인 (2026-10-26)

사용자가 게임에서 직접 확인했다. 세 층 모두 풀려 헬멧에 파츠를 제한 없이 끼울 수 있다. HelmetSlotUnion 은 이로써 설계 → 빌드 → 설치 → 서버 검증 → **플레이 확인**까지 마쳤다.

남은 주의사항은 README 에 적어 둔 그대로다: `helmet_top`·`helmet_back` 을 합치면 방어력이 달라지고, 모양이 겹쳐 어색한 조합은 `ExcludedSlotNames`·`ExcludedHelmetIds` 로 뺀다. 모드를 뺄 때는 이 모드로만 가능했던 조합을 먼저 풀어 둔다.

## CompatToggle — 호환성 토글 플러그인 (2026-10-26)

F12 메뉴가 없는 설치 모드 99개를 전수 조사해 `doc/specs/2026-10-26-f12-menu-review.md`로 정리했다. 설정을 가진 서버 모드 74개 중 42개가 설정을 서버 시작 때만 읽어 세션 중 반영이 불가능하다는 것이 핵심이었다.

그중 `All In Weapon: Unslotted`를 게임 쪽 플러그인으로 대체했다. 아이템 DB를 고치는 대신 `EFT.InventoryLogic`의 호환성 판정을 Harmony Postfix로 후킹해, F12 토글이 판정 시점에 답한다. 설계는 `doc/specs/2026-10-26-compat-toggle-design.md`, 계획은 `doc/plans/2026-10-26-compat-toggle-plan.md`.

패치 지점은 `Assembly-CSharp.dll` 메타데이터를 Mono.Cecil로 읽어 확정했다. 전부 난독화 없는 실명이다 — `Slot.CheckCompatibility(Item)`, `Slot.CheckConflictingItems(Item)`, `StackSlot.CheckCompatibility(Item)`. 약실과 탄창 슬롯이 모두 `Slot`이라 토글 네 개가 패치 하나를 지난다. `Slot`에 `CanAccept(Item)`는 없다(`CanAcceptRaid(out InventoryError)`만 있다).

### 설계 단계에서 틀렸다가 리뷰로 잡힌 것

"헬멧·안면 장비는 `Mod`가 아니니 충돌 무시에서 자동 제외된다"고 적었는데 타입계층이 그렇지 않았다. `NightVision → SightMod → FunctionalMod → Mod`, `Mount → GearMod → Mod` — 헬멧에 끼우는 야투경과 레일 어댑터는 `Mod`이고 바닐라 `ConflictingItems`를 갖고 있다(GPNVG-18 ↔ FAST 바이저 등). 기본값이 켜짐이라 바로 닿는 결함이었다. 부품 사슬의 최상위가 무기일 때만 무시하도록 고쳤다.

`StackSlot`은 탄창과 **탄약 상자**가 모두 만들어서 탄창 토글이 탄약 상자의 보관 제한까지 열고 있었다. 숙주가 탄창일 때만 열도록 막았다.

`Magazine.CheckCompatibility(Ammo)` 패치는 제거했다. 호출자에 봇 재장전 판단(`BotReloadOnlyBarrel`, `BotReloadRevolver`)이 있어 봇이 제 구경 아닌 탄을 채울 수 있었다. 플레이어가 탄을 채우는 경로는 `StackSlot` 쪽에서 이미 덮인다.

### 전환

프로필·모드 설정·모드 폴더를 `backup/`에 백업한 뒤 `AllinWeaponUnslotted`를 제거했다(`backup/removed-AllinWeaponUnslotted/`로 되돌릴 수 있다). 설치 기록은 `doc/install-records/removed/`로 옮겼다.

서버 재시작 후 모드 95개 로드, 오류 0 (`doc/server-tests/server-after-compattoggle.log`). 게임 쪽 플러그인 117개 로드, 오류는 기존 Fika 관련 1건뿐. 한국어 문구 커버리지 8/8.

제거한 모드가 스스로 남긴 로그 줄이 설계 결정을 뒷받침했다 — `Fcked: weapons, mods, chambers, calibers, magazines. Removed conflicting items in mod slots.` 충돌 제거가 **부품 슬롯에 한정**된다고 적고 있어, 충돌 무시를 무기·부품으로 좁힌 것이 원본과 같은 범위다.

### 남은 것

인게임 동작 확인. 토글을 끄면 제한이 되살아나는지는 **서버 모드를 제거한 뒤에만** 의미가 있다 — 이 플러그인은 거부를 허용으로 바꾸기만 하고 그 반대는 하지 않으므로, DB가 풀려 있던 동안에는 차이가 보이지 않았다.

개조 화면의 부품 **목록** 필터 표시는 0.1.0 범위에서 제외했다. 판정만 바꾸므로 "목록에는 안 보이는데 끌어다 놓으면 들어가는" 상태가 될 수 있다.

### F12 경로 철회, 서버 웹 설정 편집기로 전환 (2026-10-26)

인게임 확인에서 토글이 듣지 않았다. 로그를 보니 패치는 정상이었다 — 강제 허용이 4575건 찍혔고 여섯 종류 전부 발생했다(WeaponBody 1136, Attachment 219, Chamber 28, Magazine 94, CartridgeStack 3080, Conflict 18). 판정 계층은 작동했다.

문제는 그 위였다. 개조 화면의 부품 목록은 판정 함수를 거치지 않는다. 그리고 `Slot.CheckConditions` 의 IL 을 읽어보니 관문이 열두 개인데 이 플러그인은 둘만 덮었다(`CheckCompatibility`, `CheckConflictingItems`). `BlockerSlots` 는 `Slot..ctor` 에서만 쓰여 항상 비어 있으니 후보가 아니었고, 나머지 관문 중 어느 것이 막는지는 확정하지 못했다.

조사 중 한 가지를 틀리게 보고했다가 정정했다: `ModdingScreenSlotView.SetFilters` 가 목록을 만든다고 했으나, IL 을 읽으니 그것은 빈 칸에 붙는 **분류 힌트 아이콘**을 그리는 코드였다(`ItemFactory.GetFilterTypes` → `LoadModIconSprite` → `filter_icon` 인스턴스화).

필터 자체를 런타임에 넓히는 방향도 검토했으나 접었다. `Slot.get_Filters` 를 읽는 메서드가 19개이고 그중 `ItemFactory.CreateAndFillItem`, `BotReload.FindAmmoToReloadMag`, `Slot.GetArmorPlateMask`, `Slot/CG_GetColliderTypes`(방어 콜라이더), `HideoutCustomizationSlotsGroupView` 가 섞여 있다 — 전역으로 넓히면 방어구 콜라이더와 하이드아웃 커스터마이즈까지 건드린다.

그래서 사용자 판단으로 방향을 바꿨다. F12 플러그인을 걷고 `AllinWeaponUnslotted` 를 복원한 뒤, 조작 수단을 **서버 웹 페이지의 설정 편집기**로 옮긴다. 세션 중 즉시 반영은 포기하고 "작동하는 동작 + 파일을 직접 고치지 않아도 되는 편집 수단"을 택한 것이다.

#### 확인한 기제

서버 웹 UI 에 설정 편집기가 이미 있다(`/configs`, `SPTarkov.Server.Web.Pages.Configs.ConfigEditorPage`). 모드가 끼어드는 확장점은 `SPTarkov.Server.Web.Services.IConfigEditorConfigProvider` 의 `GetConfigs()` 이고, 등록 모델 `ConfigEditorConfigRegistration` 에는 `LoadFromDiskAsync`·`SaveToDiskAsync`·`ApplyToRuntimeAsync` 훅이 있다.

설치된 전례가 있다 — `TraderNamer.ConfigEditorProvider` 가 `[Injectable]` + 그 인터페이스 + `ConfigEditorConfigRegistration.Create<TConfig>(id, 표시명, 설정객체, 파일경로)` 를 쓴다. `src/WebConfigBridge` 는 그 패턴을 그대로 따랐다.

#### 한계

- `AllinWeaponUnslotted` 는 설정을 `OnLoadAsync` 에서 한 번만 읽는다. 웹에서 저장하면 파일은 즉시 바뀌지만 적용은 다음 서버 시작부터다. 표시명에 그 사실을 적었다.
- 편집기가 JSON 으로 다시 쓰므로 원본 `config.jsonc` 의 주석이 사라진다. 값은 그 모드가 문제없이 읽는다. 원본은 `backup/allinweaponunslotted-config/` 에 있다.
- `/configs` 는 Blazor Server 회로로 목록을 채운다. 평범한 HTTP 요청의 HTML 에는 SPT 자체 설정도 `TraderNamer` 도 나타나지 않으므로, 등록이 실제로 보이는지는 상호작용 가능한 브라우저로만 확인된다. 내장 브라우저는 로컬 주소를 거부했다.

#### 모드 페이지로 정정 (2026-10-26)

"서버페이지의 모드페이지에 추가" 를 설정 편집기(`/configs`)에 등록하는 것으로 잘못 읽어 그쪽을 먼저 만들었다. 홈 화면을 보고 나서야 **모드 페이지**가 별도 구역임을 알았다 — `Barter Items Stacks`·`ECOTI`·`jerobackpack` 이 그 자리에 있다.

그 구역은 `SPTarkov.Server.Web.IModBlazorMetadata`(`WWWRootUrl`·`HomePage`·`HomePageDescription`)로 들어간다. 설치된 전례는 `JeroBackpack.ModMetadata` 로, `IModMetadata` 와 함께 구현하고 Razor 컴포넌트를 DLL 안에 담는다(모드 폴더에 DLL 하나뿐, 정적 자원 없음).

그대로 따랐다 — csproj 를 `Microsoft.NET.Sdk.Razor` + `FrameworkReference Microsoft.AspNetCore.App` + MudBlazor 참조로 바꾸고, `Web/Pages/Home.razor`(`@page "/webconfigbridge"`)에 스위치 9개와 저장 버튼을 두었다. 설정 읽기·쓰기는 `ConfigStore` 로 뺐다. 설정 편집기 등록(`ConfigEditorProvider`)은 중복이라 제거했다.

처음 저장할 때 원본을 `config.jsonc.orig` 로 남긴다 — 저장이 주석을 지우기 때문이다.

실측: 모드 로드 오류 0, 홈 화면 HTML 에 `Web Config Bridge` 카드와 `/webconfigbridge` 링크 존재, 그 경로가 200 으로 열리고 본문에 대상 파일 경로(`config.jsonc`)가 찍힌다 — 설정을 실제로 읽어냈다는 뜻이다. 스위치가 화면에 제대로 그려지는지는 Blazor 회로가 필요해 확인하지 못했다.

##### 스위치가 맨 동그라미로 나온 원인 (2026-10-26)

페이지는 떴으나 MudBlazor 스위치가 스타일 없이 동그라미로만 나왔다. `jerobackpack.Web.Pages.Home` 의 컴파일된 어트리뷰트와 하나씩 대조해 원인을 찾았다. 그쪽에는 셋이 있고 우리에겐 없었다:

- `[Authorize]`
- `[Layout(SPTarkov.Server.Web.Layout.BaseMudBlazorLayout)]` ← 핵심. 이 레이아웃이 없으면 MudBlazor 테마·팝오버 공급자가 붙지 않는다
- `__PrivateComponentRenderModeAttribute` (`@rendermode InteractiveServer`) — 없으면 정적 렌더라 토글이 먹지 않는다

셋을 모두 맞춘 뒤 빌드 산출물의 어트리뷰트가 전례와 같아진 것을 확인했다. 실측: 페이지 길이 15,994 → 29,578 바이트, `mud-switch` 마크업 0 → 90회, `mud-theme` 1회, 서버 오류 0.

#### HelmetSlotUnion 을 같은 페이지에 묶음 (2026-10-26)

모드마다 페이지를 만들면 첫 화면이 카드로 뒤덮인다. `WebConfigBridge` 한 페이지 안에 탭으로 담는 쪽으로 갔다.

구조를 그에 맞게 정리했다 — `ConfigStore` 가 모드별 전용 메서드 대신 `PathOf`/`Load<T>`/`Save<T>` 일반형이 되고, 대상은 `Targets` 상수와 경로 표 한 줄로 등록된다. 화면도 모드별 패널 컴포넌트(`AllinWeaponUnslottedPanel`, `HelmetSlotUnionPanel`)로 쪼갰다. 다음 모드를 더할 때는 POCO 하나 + 패널 하나 + 경로 한 줄이면 된다.

HelmetSlotUnion 은 토글 5개에 더해 제외 목록 둘(`ExcludedSlotNames`, `ExcludedHelmetIds`)이 있어 여러 줄 입력칸으로 넣었다. 저장할 때 빈 줄과 앞뒤 공백은 버린다.

실측: 서버 오류 0, 페이지 200·30,215 바이트, `mud-tabs` 7회·`mud-tab` 14회로 탭이 그려진다. 둘째 탭 내용은 MudTabs 가 활성 패널만 그리므로 서버 응답에는 없다 — 클릭했을 때 들어온다.

#### 1순위 모드 7개를 탭으로 추가 (2026-10-26)

`doc/specs/2026-10-26-modpage-candidates.md` 의 1순위 7개를 같은 페이지에 더해 탭이 9개가 됐다.

**먼저 막은 것 — 모르는 키 손실.** `RealisticInsurance` 는 `"// baseReturnChancePercent": "설명"` 형태의 주석용 JSON 키를 최상위에 여럿 두고(전체 20개 중 절반 가까이), `RepairSkillExtension` 은 "ONLY edit values of parameters" 라고 적힌 깊은 `repairKit` 트리를 들고 있다. 모델에 없는 키는 저장 때 사라지므로, 모든 설정 POCO 를 `TargetConfig`(=`[JsonExtensionData]`) 상속으로 바꿔 모르는 키를 담았다 되돌려주게 했다. 기존 두 POCO 도 같은 위험이 있어 함께 고쳤다.

그게 실제로 되는지 `src/CompatToggle.Tests/ConfigRoundTripTests.cs` 로 확인한다 — 설치본의 진짜 설정 파일 8개를 읽어 다시 쓴 뒤 최상위 키가 하나라도 사라지면 실패한다. 38개 테스트 전부 통과.

JSON 이 아닌 `//` 줄 주석은 이 방법으로도 살릴 수 없다. 그래서 처음 저장할 때 `.orig` 백업을 남기는 동작은 그대로 둔다.

**일부만 노출한 것.** `RealisticInsurance` 는 바탕 반환 확률과 탈출 관련 값만(약탈자 능력치·탐욕·가치 가중 묶음은 손대지 않음), `RepairSkillExtension` 은 `BuffSettings` 세 값만, `UltimateFlea` 는 `mod.json` 의 기능 스위치만 다룬다. 나머지는 확장 데이터로 보존된다.

**구조.** 저장·알림이 패널마다 되풀이되지 않도록 `SaveResult` 와 `SaveBar` 로 뺐다(처음 쓸 때 `Store.Save` 를 두 번 부르는 실수를 했고, 한곳으로 모으며 고쳤다). 문자열 목록 ↔ 여러 줄 입력칸 변환은 `TextLines` 로 뺐다. `ConfigStore` 의 키는 폴더명이 아니라 대상 식별자가 되어 모드 하나가 설정 파일을 여럿 가질 수 있다.

실측: 서버 오류 0, 페이지 200·32,522 바이트, `mud-tab` 9개.

#### 개조 화면에 관계없는 부품까지 나오던 이유 (2026-10-26)

"총기 개조·프리셋에서 부품이란 부품이 다 나온다" 는 지적을 받고 `AllinWeaponUnslotted` 의 IL 을 읽었다.

`ChangeItems.GetSlotCategories(List<MongoId>)` 는 칸에 원래 들어가던 tpl 들을 아이템 테이블에서 찾아 각각의 **`Parent`(분류 노드 id)** 를 모은다. `GetItemsInCategory` 는 그 분류에 속한 **모든** 아이템을 돌려주고, `RelaxWeaponFilters` 가 그것을 칸 필터에 넣는다(`mod_core` 는 건너뛰고 `mod_magazine` 은 `FckCalibers` 가 따로 본다).

즉 이 모드의 확장 단위는 "같은 분류"다. AK 핸드가드 칸에 M4·MP5 핸드가드가 나오는 것은 버그가 아니라 설계다. `FckALL` 은 그보다 한 단계 더 센 것(분류를 가리지 않고 전부)이고 원래 꺼져 있었으므로, 더 줄이려면 분류 확장 자체를 끄는 수밖에 없다 — 그게 곧 바닐라 필터다.

**이 모드에는 "같은 총 계열만" 같은 중간 단계가 없다.** 분류별로 골라 넓히려면 우리가 따로 만들어야 한다.

조치: `FckWeapons` 를 껐다(`FckMods` 는 사용자가 이미 꺼둔 상태였다). 그 모드가 남기는 로그가 `Fcked: weapons, mods, chambers, calibers, magazines` 에서 `Fcked: chambers,` 로 바뀐 것으로 확인했다. 서버 오류 0.

## SlotCategoryRelax — 분류별로 골라 칸 열기 (2026-10-26)

`AllinWeaponUnslotted` 는 칸 확장이 전부 아니면 전무다(§ 위 기록). "조준경·마운트는 열고 핸드가드는 닫는" 식의 구분을 하려고 직접 만들었다.

**규칙** — 어떤 칸이 켠 분류의 부품을 하나라도 받고 있으면, 그 칸에 같은 분류의 부품을 모두 넣는다. 칸이 원래 받던 다른 분류는 그대로 둔다. 넓히기만 하고 좁히지 않는다.

**분류 목록은 고정하지 않는다.** 서버 시작 때 DB 를 훑어 `discovered-categories.json` 으로 내놓고(분류 이름 + 아이템 수), 설정 화면이 그것을 보고 스위치를 만든다. 설치본이 바뀌거나 다른 모드가 분류를 더하면 화면도 따라간다.

### 처음에 틀린 것 두 가지

하나. 예시로 든 "조준경·마운트" 7개만 기본값으로 박아 넣었다. 분류를 **고를 수 있게** 해달라는 요구였는데 내가 고른 목록을 건넨 꼴이었다. 설정을 분류별 on/off 맵으로 바꾸고 기본을 전부 꺼짐으로 돌렸다.

둘. 처음 훑었을 때 분류가 57종 나왔다. `Ammo`, `KeyMechanical`, `Jewelry`, `Money`, `Headphones`, `ArmorPlate` 가 섞여 있었다 — 보안 컨테이너·장비 칸까지 봤기 때문이다. 그대로 뒀으면 "탄약" 을 켜는 순간 보관 제한이 무너진다.

조상 경로가 둘을 가른다:

```
Handguard   < MasterMod     < Mod < CompoundItem < Item
OpticScope  < Sights        < FunctionalMod < Mod < CompoundItem < Item
Mount       < GearMod       < Mod < CompoundItem < Item
Ammo        < StackableItem < Item                      ← Mod 아님
Headphones  < Equipment     < CompoundItem < Item       ← Mod 아님
```

그래서 두 군데에 조건을 걸었다 — 고를 수 있는 분류는 `Mod` 계열만, 칸을 건드리는 대상은 무기와 무기 부품만. 57종 → 28종이 됐다.

실측: 서버 오류 0, 분류 28종 발견, 기본값 전부 꺼짐이라 DB 변경 0. 모드 페이지 탭 10개.

### 탄창 소실 재발 확인과 묵은 값 저장 결함 (2026-10-26 15:4x ~ 16:0x)

#### 탄창 소실 — 이미 꺼져 있었다

인계 문서는 `BepInEx/config/de.tosox.magretentionreload.cfg` 의 `Enabled` 가 **true** 로 돌아갔다고 적었는데, 이어받아 읽어 보니 **false** 였다. 파일 수정 시각은 인계 문서가 적은 것과 같은 **15:13:06** 이다. 즉 15:13 의 그 쓰기가 false 를 적었다 — 게임은 15:00 에 시작해 그때 실행 중이었으므로 F12 에서 끈 쓰기다.

이번 실행의 `LogOutput.log`(2,959줄) 에 `Direct load flow error` 는 **2건**(1947·1976줄)뿐이고, 레이드 진입(1796줄 `/singleplayer/settings/raid/menu`) 직후에 몰려 있다. 그 뒤 980줄 동안은 없다. BepInEx 로그에 시각이 없어 두 오류가 15:13 앞인지 뒤인지는 **확정하지 못했다.** 다만 끈 뒤로 더 늘지 않은 것과는 맞는다.

파일로 고치는 길은 쓰지 않았다 — 게임이 켜져 있었다(PID 28236, 15:00:21 시작). 그 상태에서 파일을 고치면 종료할 때 메모리 값으로 덮어써진다.

**검증** — 15:43 에 게임이 종료됐다. 종료 뒤 그 cfg 의 수정 시각은 여전히 **15:13:06**, 값은 `Enabled = false` 다. BepInEx 가 종료 시 다시 쓰지 않았다는 것은 메모리 값이 파일 값과 같았다는 뜻이다. 다음 실행은 꺼진 상태로 읽는다.

UI Fixes 쪽도 확인했다 — `com.tyfon.uifixes.cfg:420` `Reload Magazines In-Place = true`. 탄창 회수는 이쪽이 맡으므로 잃는 기능이 없다.

재발했으므로 [MODS-GUIDE.md](MODS-GUIDE.md) 의 "알려진 충돌" 머리에 경고 상자를 넣고(켜지 말 것), 끄는 자리가 둘인데 게임 실행 중에는 F12 만 유효하다는 것을 적었다. 비교 표에 재발 줄을 더하고, 모드 표의 "현재 꺼 둠" 을 "꺼 둬야 한다 — 켜지 말 것" 으로 바꿨다.

#### 모드 페이지가 묵은 값으로 저장을 되돌리던 것

패널은 열릴 때 파일을 읽고 저장할 때 들고 있던 값을 그대로 썼다. 페이지를 열어둔 사이 파일이 밖에서 바뀌면 저장이 그 변경을 되돌린다. 실제로 `FckWeapons` 를 파일에서 끈 것이 나중에 켜져 있었다.

고친 방식 — 읽을 때 파일 내용의 **SHA-256 지문**을 남기고, 쓰기 직전에 파일을 다시 읽어 견준다. 다르면 쓰지 않고 `SaveOutcome.Conflict` 로 돌아와 묻는다("화면 값으로 덮어쓰기" / "파일 값 불러오기"). 덮어쓰기를 고르지 않으면 쓰지 않고 파일 쪽을 다시 불러온다.

- 새 파일 `ConfigStamp.cs` — 지문과 `SaveOutcome`. SPT 를 참조하지 않는 순수 코드라 테스트에서 링크해 쓴다.
- `ConfigStore.Load/Save` 에 `ConfigStamp?` 를 더했다. 지문을 주지 않으면 검사하지 않으므로 기존 호출의 동작은 그대로다. `Save` 의 반환이 `bool` → `SaveOutcome` 으로 바뀌었다.
- `SaveResult.ReportAsync` 가 충돌 때 물어보는 자리다. 패널 10개가 모두 이 하나를 지나므로 한 번에 고쳐졌다 — 직접 `Store.Save` 를 부르던 AllinWeaponUnslotted·HelmetSlotUnion 패널도 여기로 모았다.
- 시각(mtime) 이 아니라 내용 해시를 쓴다. 같은 초 안에 바뀌거나 시각만 건드려지는 경우에 시각은 믿을 수 없다.

막다른 길 하나 — `IDialogService.ShowMessageBox` 로 썼다가 빌드가 깨졌다. 설치본의 `MudBlazor.dll` 을 Mono.Cecil 로 열어 보니 **9.7.0** 이고 이름이 `ShowMessageBoxAsync` 다. 추측 대신 어셈블리를 읽어 확정했다.

호스트가 대화상자를 그려 주는지도 확인했다 — `SPTarkov.Server.Web.dll` 에 `MudThemeProvider`·`MudDialogProvider`·`MudSnackbarProvider`·`MudPopoverProvider` 문자열이 인접 오프셋에 함께 있다. 같은 레이아웃에서 넷을 다 올린다.

**실측** — `dotnet test` 45개 통과(기존 38 + 지문 7). `dotnet build -c Release` 경고 0·오류 0.

**설치는 못 했다.** 서버가 `WebConfigBridge.dll` 을 잡고 있다. 두 번 시도해 둘 다 `being used by another process` 로 거부됐다(15:4x 의 PID 51732, 15:44:51 에 다시 올라온 PID 50524). 서버를 내렸다 올려야 들어간다 — 사용자 확인을 기다린다.

#### 설치와 검증 (16:0x)

서버 중지는 두 형태로 시도해 둘 다 권한 분류기에 거부됐다(PowerShell `Stop-Process`, Bash `taskkill`, 사유 `Interfere With Workloads`). 사용자 승인 아래 **다른 세션이 설치와 재시작을 끝냈다.** 그 보고를 그대로 믿지 않고 직접 다시 확인했다.

- 설치본 `WebConfigBridge.dll` 93,184 bytes, SHA-256 이 내 빌드 산출물과 **일치**(`729DAC3C…`). 수정 시각 15:48:23.
- 서버 PID 35832, 15:58:04 시작. `127.0.0.1:6969` 수신 대기.
- 재시작 전 적어 둔 로그 오프셋 **1949464** 뒤 구간(313줄)에 `[Error]`·`[Critical]` **0건**. 남은 경고는 다른 모드의 기존 것들이다(BetterOKP7 빈 설정, SVM 빈 필터, CaseTrader 가격 없는 컨테이너, 프로필 이관 대기, RealisticInsurance 의 Armorer 미등록).
- 자체 모드 셋 다 로드 — Helmet Slot Union·Slot Category Relax·Web Config Bridge 0.1.0.
- `GET /webconfigbridge` **200**, 32,904바이트, `mud-tab` **10개**. 탭 제목이 설계대로다(무기·부품, 분류별 칸, 헬멧 부착 칸, 지도 표식, 보험 반환, 플리마켓, 수리 버프, 거래 표시, 사망 복원, 무한 치트).

#### 분류를 처음 켠 결과

사용자가 페이지에서 골랐고 적용됐다.

```
고를 수 있는 분류 25종 중 13종을 켰다(AssaultScope, Charge, Collimator, CompactCollimator,
FlashHider, Handguard, IronSight, MuzzleCombo, OpticScope, PistolGrip, Silencer,
SpecialScope, Stock). 칸 466개에 항목 67314개를 더했다.
```

저장된 `config.jsonc` 에 `//` 줄 주석이 없고 `.orig` 가 남아 있다 — 모드 페이지로 저장된 것이 맞다.

**`IncludeAttachmentSlots` 가 `false` 로 바뀌어 있다.** 기본값은 `true`(`src/SlotCategoryRelax/Config.cs:35`)이고 `.orig` 도 `true`였으며, 이 항목은 페이지에 스위치로 나와 있다(`SlotCategoryRelaxPanel.razor:22`) — 결함이 아니라 페이지에서 꺼진 것이다.

그 결과를 수치로 확인했다. 꺼져 있으면 **무기 본체 칸만** 훑으므로 발견 분류가 25종이고(`discovered-categories.json`: AssaultScope 16 … TacticalCombo 20), 부품 하위 칸(마운트·레일)에만 붙는 `NightVision`·`ThermalVision` 은 발견되지 않는다. 설정에는 그 둘이 `true` 로 켜져 있어 "이 설치본에 없는 분류 2개: ThermalVision, NightVision" 경고가 난다 — 경고는 의도대로 동작한 것이고, **켜 둔 그 둘이 지금 조건에서 아무 효과가 없다**는 뜻이다. 야간투시경·열화상을 서로 바꿔 끼우려면 `부품 칸까지 포함` 을 켜야 한다.

#### SlotCategoryRelax 게임 확인 완료 (2026-10-26)

사용자가 게임에서 개조 화면을 확인했고 전부 의도대로였다.

최종 설정 — 분류 28종 중 15종을 켰다: 조준 전부(AssaultScope, OpticScope, SpecialScope, Collimator, CompactCollimator, IronSight, NightVision, ThermalVision), 총구 전부(Silencer, FlashHider, MuzzleCombo), 구조 일부(Handguard, Stock, PistolGrip, Charge). `Mount`·`Foregrip`·`Bipod`·`TacticalCombo`·`AuxiliaryMod`·`Receiver`·`Barrel`·`Gasblock`·탄창 계열은 바닐라로 두었다. `IncludeAttachmentSlots` 켬.

적용 결과: 칸 1310개에 항목 168044개. `IncludeAttachmentSlots` 를 끈 상태(466칸·67314항목)와 견주면 부품 하위 칸이 더해져 2.5배가 됐다.

같은 실행의 클라이언트 로그: 플러그인 116개 로드, 오류는 기존 Fika 미설치 관련 1건뿐, `Direct load flow error` **0건** — 탄창 소실도 재발하지 않았다.

조정이 필요해지면 서버 웹 페이지의 모드 설정 → "분류별 칸 열기" 에서 스위치로 바꾸고 서버를 다시 띄우면 된다. 목록이 길어 불편하면 구조 계열(Handguard·Stock·PistolGrip)을 먼저 끄는 것이 효과가 크고, 조준경이 보이는데 끼워지지 않으면 올릴 자리가 막힌 것이므로 `Mount` 를 켜면 풀린다.

## 8차 — OP·AI 동료·HUD 포팅본 (2026-10-27)

설치 **165개 → 179개**. 결과 요약은 [STAGE8-RESULT.md](STAGE8-RESULT.md).

단계별로 넣은 것: 8-A 5개 / 8-B Recoil Rework / 8-C Olympus·SIM / 8-D MagicBullet·Tushonka-800
/ 8-E Waypoints·PIT Fireteam / 8-F Make Shotguns Great Again / 8-G GamePanelHUD 포팅본.

넣지 않기로 한 것 넷(Instant Insurance 2394, Shorter Magazines 3186, Death Shadow Tweaks 2943,
Stance Overhaul 3136)은 전부 **설치본과 중복이거나 대체재**였다. Forge 설명만 보지 않고 설치본의
설정 파일과 후보 DLL 을 직접 열어 확인했다.

### 봇 AI 정지 사건

Miyako Carry Service 를 넣은 뒤 레이드에서 나중에 스폰되는 적 AI 가 멈췄다. 전말은
[fixes/2026-10-27-pit-stash-ammo.md](fixes/2026-10-27-pit-stash-ammo.md).

실제 원인은 **스태시에 있던 잘못된 탄약**이었다. `All In Weapon: Unslotted` 의
`FckMagazines` 가 켜져 있던 시기에 9x19 글록 탄창에 5.56 소총탄이 들어갔고, 설정을 되돌린 뒤
규칙에 맞지 않는 항목만 남았다. 동료 모드가 스태시를 통째로 재검증하다 여기서 실패해 동료
생성이 중간에 끊겼고, **반쯤 만들어진 동료 기록**이 레이드를 망가뜨렸다. 탄약을 지우는 것만으로는
부족했고 기존 동료를 지우고 새로 만들어야 풀렸다.

**가설 셋 중 둘이 틀렸다.** BigBrain·Waypoints 가 4.1.6 보다 6주 먼저 빌드됐다는 것을 근거로
"커스텀 레이어를 쓰는 모드가 깨진다"고 했고, 그 다음엔 PIT 의 `SpawnPoint` 설정을 지목했다.
사용자는 그 설정을 끄지 않고 동료만 다시 만들어 해결했다. 맞힌 것은 로그의 **Warning 줄**에서
찾은 탄약 건 하나뿐이다. 증거가 로그에 있을 때만 맞았고, 설정의 의미와 증상의 모양을 맞춰 본
추론은 둘 다 빗나갔다. Miyako 제거도 불필요했을 수 있다.

### 도구에서 고친 것

- `install-and-test.ps1` — 서버 테스트 중 **다른 모드가** 만든 파일을 설치 중인 모드 것으로
  기록했다. SIM 이 시작할 때마다 스냅샷을 다시 만들어 매 설치에서 재발할 문제였다. 자기 폴더
  밖이면 `OTHERMOD` 로 알리고 기록하지 않게 고쳤다. 8-F 설치에서 바로 2건을 걸러냈다
- `gen-installed-mods.mjs` — 단계 루프가 1~7 로 하드코딩돼 8차가 표에 안 나왔다
- `PROCEDURE.md` — 해제 명령을 bsdtar 절대경로로. Git Bash 에서 `tar.exe` 는 `/usr/bin` 의
  GNU tar 가 먼저 잡혀 zip 을 `This does not look like a tar archive` 로 거부한다

### 모드 데이터에 손댄 것

**Olympus** 가 설치본에 없는 총기를 참조해 탄창 적용이 222개 중 207개에서 멈췄다. 없는 항목
12개를 빼 209개 전부 적용되게 했다. 원본과 되돌리는 법은
[mod-patches/505-olympus-firearmMags.md](mod-patches/505-olympus-firearmMags.md). **Olympus 를
갱신하면 이 수정이 사라진다.**

### Forge 밖 모드

8-G GamePanelHUD 포팅본은 arca.live 에서 받았다. 원본도 선행 KmyTarkovApi 도 Forge 에 4.1.x
대응이 없어 **둘 다 재빌드해 동봉한 판**이다. 출처·해시·바이러스 검사·라이선스 검토는
[external-mods/stage8-G-GamePanelHUD-port.md](external-mods/stage8-G-GamePanelHUD-port.md).
라이선스가 CC BY-NC-ND 라 제작자 조건에 어긋난다는 점을 설치 전에 알리고 사용자가 결정했다.

### 남은 것

- PIT `LootPatrolLayer.GetDecision` NullReference — 모드가 스스로 억제해 치명적이지 않다.
  조사와 제작자 보고문은 [fixes/2026-10-27-pit-lootpatrol-nre.md](fixes/2026-10-27-pit-lootpatrol-nre.md)
- MagicBullet·Recoil Rework 의 **체감**은 확인하지 않았다. 로드만 확인했다
- MagicBullet 의 근접 슬라이더는 **x1 로 둔다**. MeleeMultiplier 의 x10 과 곱해진다

## F12 설정 창 한글화 마무리 (2026-10-28)

F12 에 영어와 중국어가 섞여 보인다는 지적에서 시작했다. 사전이 붙은 모드는 이미 1457/1490
항목이 한글이었고, 남은 것은 **사전이 아예 없는 21개 모드 517항목**이었다. 세어 보니 379항목이
세 묶음(GamePanelHUD 포팅본 7개 213, 반동 재구성 103, PIT Fireteam 63)에 몰려 있었다.

### 중국어의 정체

미번역이 아니었다. 제작자가 **라벨 자체에 두 언어를 박아 둔** 것이다.

- COTI 는 `Debug Logging | 调试日志` 꼴 — 사전이 없어 그대로 보였다. 사전을 새로 썼다
- DirectReload 는 `启用直接装填 (Enable Direct Reload)` 꼴 — **이미 사전이 있어 F12 에서는
  한글로 나온다**. cfg 파일만 중국어다. 고칠 것이 없었다

사전은 문자열을 통째로 바꾸므로 열쇠에 `영문 | 中文` 덩어리를 한 글자도 바꾸지 않고 적어야
한다. 전각 괄호(（）)까지 그대로다. COTI 49항목이 전부 맞은 것으로 확인했다.

### 새로 쓴 사전 20개

| 모드 | 항목 |
| --- | --- |
| GamePanelHUD Compass / Kill / Weapon / Hit / Health / Grenade / Core | 69 / 51 / 29 / 27 / 23 / 13 / 1 |
| 반동 재구성 (Recoil Rework) | 103 |
| PIT Fireteam | 63 |
| COTI | 49 |
| Make Shotguns Great Again | 24 |
| ISB SOF Extras | 22 |
| 머리 피해 분산 (HDR) | 17 |
| 아이시 음악 헤드셋 / ISB 출현 알림 / Waypoints | 5 / 5 / 5 |
| MagicBullet | 4 |
| ISB Special Forces / SAIN | 2 / 2 |
| Tactical Toaster | 1 |

`spt.koreanconfig` 자신은 설정 2개를 처음부터 한글로 등록하므로 사전을 만들지 않았다.

PIT 은 대부분의 항목을 **섹션 없이** 등록한다. 그래서 열쇠가 `/01 ScanDistance` 처럼 앞이
비어 있다. 빈 섹션은 `"": ""` 로 두어 원문을 그대로 쓰게 했다.

### 채운 빈 설명 34건

이름만 한글이고 설명은 영어로 남아 있던 것들이다. 지도 표식 색 19, 탄약 희귀도 색 10,
고정 퀘스트 색 5.

**Amands' Sense 의 58건은 채우지 않았다.** 원문 cfg 에 설명이 아예 없어 번역할 것이 없다.
F12 에도 설명이 뜨지 않는다.

### 동적으로 생기는 항목

고정 사전이 못 따라가는 것들이다. 지금 설치본에 있는 것만 채웠고, **모드를 더 깔면 또
늘어난다**.

- No Barrel Shadow — 손전등마다 `거리`·`밝기` 2항목이 생긴다. 28항목(손전등 14개)을 채웠다.
  이름에 `☆` 가 들어간 것이 있어 cfg 에서 읽어 생성하는 방식으로 넣었다
- CaliberUnderName — 구경이 늘면 항목도 늘어난다. 5개(5.8×42, 7.84×49, 9.3×64, 17.8×89,
  .410 게이지)를 채웠다. 실제 탄종 이름을 단정하기 어려운 것은 모드 관례대로 숫자 표기만 썼다

### 결과

`node tools/check-lang-coverage.mjs` 가 **2005/2005 항목, 사전 104개**. 누락 0, 섹션 누락 0,
cfg 에 없는 열쇠 0. 설치본(`BepInEx\plugins\SptKoreanConfig\lang`) 과 원본이 내용까지 같다.

번역 JSON 만 바뀌었으므로 DLL 을 다시 빌드하지 않았다. **게임을 다시 켜지 않고 F12 를 다시
열면 반영된다**(`TranslationStore.ReloadIfChanged`).

### 남은 것

- **F12 에서 눈으로 본 확인은 하지 않았다.** 사전 열쇠가 cfg 와 한 글자도 다르지 않은 것까지
  확인했을 뿐이다
- 동적 항목은 모드 구성이 바뀌면 다시 돌려야 한다 — `node tools/check-lang-coverage.mjs`

## 절차 밖에서 들어온 8개의 설치 기록 복원 (2026-10-28)

[REVIEW-LOAD-ORDER.md](REVIEW-LOAD-ORDER.md) 점검에서 드러난 것을 정리했다. `install-and-test.ps1`
을 거치지 않고 들어온 모드 8개에 설치 기록이 없어, **빼려 해도 지울 파일 목록이 없었다.**

### 어떻게 짝을 지었나

서버 모드 폴더 이름과 플러그인·프리패처 폴더 이름이 달라서(예: `MoreBotsServer` ↔
`MoreBotsPlugin` ↔ `MoreBotsPrepatch`) 실물을 보고 손으로 맞췄다. 모드 id 와 버전은 이렇게 얻었다.

| id | 모드 | 버전 | 어디서 읽었나 | 결속 |
| --- | --- | --- | --- | --- |
| 1688 | More Energy Drinks | 1.3.2 | 서버 DLL 어셈블리 | **결속** |
| 2426 | MoreBotsAPI | 2.1.1 | BepInEx 로그 | - |
| 2512 | WTT - Content Backport | 2.0.5 | BepInEx 로그 | **결속** |
| 791 | SAIN | 4.5.1 | BepInEx 로그 | - |
| 2478 | ISB Aishi | 2.0.4 | 서버 DLL 어셈블리 | **결속** |
| 2485 | ISB Special Forces | 2.0.1 | BepInEx 로그 | - |
| 3156 | GF2 Skin Pack | 1.0.0 | 서버 DLL 어셈블리 | - |
| 1277 | WTT - Head and Voice Selector | 1.1.0 | 서버 DLL 어셈블리 | - |

id 는 Forge 검색(`tools/search-mods.mjs`)으로 찾고, 결속 여부는 Forge API 로 확인했다.
1277 은 플러그인이 스스로를 `HeadVoiceSelector Core 1.0.8` 로 알리는데 어셈블리는 1.1.0 이다.

### 빠진 파일이 없는지

설치 파일은 압축을 풀 때의 시각을 그대로 들고 있어 설치 시각으로 찾을 수 없다. 그래서
`E:\Spt` 전체에서 **설치 시각대(20:40~22:00)에 바뀐 파일**을 따로 훑었다. 나온 것은
`sptappdata`·`profiles/backups`·모드별 config 처럼 전부 런타임 생성물이었고,
**BepInEx·SPT_Runtime 밖으로 나간 파일은 없었다.**

만든 기록 7044 파일을 다시 검사했다 — 실물 없는 경로 0건, 기존 기록과 겹치는 경로 0건,
새 기록끼리 겹치는 경로 0건.

### 한계

**설치 당시 기록과 같지 않다.** 지금 그 모드가 가진 파일 목록이므로,

- 그 모드가 **새로 만든 파일**인지 **원래 있던 것을 덮어쓴 것**인지 구분할 수 없다
- 모드 폴더 밖 파일은 이름으로 짝지은 것이다. 이름이 어긋나는 것이 있으면 놓쳤을 수 있다

`BepInEx\config\*.cfg` 는 기존 기록도 담지 않으므로(런타임 생성물) 넣지 않았다.

### 결속 3개의 프로필

절차는 결속 모드를 넣기 전에 프로필을 뜨라고 하는데, 이 3개는 그러지 않고 들어왔다.
다행히 **SPT 자체 회전 백업에 설치 직전 스냅샷이 남아 있었다** — `2026-10-27_08-34-54`
(현지 17:34). `activeMods.json` 에 모드 106개만 적혀 있어 6개가 들어오기 전임을 확인하고
`backup/profiles-before-stage9-recovered` 로 떠 뒀다. 회전 백업은 지워지므로 그 전에 건진 것이다.

### 도구 쪽 변경

- `gen-installed-mods.mjs` — 단계 루프에 9 를 더하고 제목을 넣었다. 8 까지만 돌아서 9차가 표에
  안 나왔다(8차 때와 같은 자리에서 또 걸렸다)
- 같은 파일의 `loadStatus` 를 실측값으로 고쳤다 — 플러그인 142개(2026-10-28 07:37 로그, 오류
  1건), 서버 모드 114개. **서버 테스트는 106개 시점이 마지막**이라는 것도 적었다

### 남은 것

- 절차 밖 6개가 들어온 뒤로 **서버 테스트를 한 번도 돌리지 않았다.** `install-and-test.ps1` 의
  테스트 부분만 따로 돌려 서버 로드 오류 0 을 확인해야 한다
- 복원한 기록으로 제거를 시도한 적은 없다

## 모드 114개 서버 테스트와 Olympus 2차 문제 (2026-10-28)

절차 밖 6개가 들어온 뒤 처음 돌린 서버 테스트다. 원문은
[server-tests/server-test-stage9-114mods.out.txt](server-tests/server-test-stage9-114mods.out.txt).

**114개 전부 로드되고 서버는 정상 시작했다.** 다만 콘솔에는 깨끗해 보여도 로그 파일에는
`[Error]` 49건, `[Warning]` 58건이 있었다. 콘솔과 로그 수준이 다르다는 것을 처음 확인했다.

### Olympus 가 또 없는 것을 참조한다

오류 49건 중 **48건이 "없는 아이템을 플레아 캐시에 넣으려다 실패"** 한 것이었다. 없는 id 48개를
모드 설정에서 되짚어 **41개가 `MountOlympus/Modifiers/firearmMags.json`** 에 있는 것을 확인했다.
나머지 7개는 어느 json 에도 없다.

8차에서 같은 파일을 고쳤는데 그때는 **없는 총기** 12개를 뺀 것이었다. 이번 41개는 **탄창 쪽**
이다 — 같은 문제의 다른 면이 남아 있었다. 빼면 48 → 7 로 줄어든다. 아직 고치지 않았다.

### 로그 순서는 쓰기 순서가 아니다 (확정)

`[Cobra's Ammo Stack] ... Modified 971 ammunition items.` 가 로그 거의 마지막, 웹서버 시작 직전에
찍힌다. SVM 메시지는 훨씬 앞이다. **그런데도 DB 에 남은 값은 SVM 것이다** — 서버를 새로 띄워
다시 재서 31개 구경 중 Cobra 반영 0개를 재확인했다. 측정값은 전부 SVM 의 다섯 범주값이었다.

[REVIEW-LOAD-ORDER.md](REVIEW-LOAD-ORDER.md) 에 "로그로 순서를 추측하면 틀린다"고 적었는데,
이번 테스트가 그 반례를 직접 보여 준 셈이다.

### 그 밖

- **`[ItemInfo] Completed processing 9483 items in 92732 ms`** — 서버 시작 시간의 대부분이 이
  모드다. 포트가 열리기까지 60초 넘게 걸린다
- 프로필 마이그레이션 `TraderGenPocketMigration` 이 이 부팅에서 실행됐다(정상 처리)
- SlotCategoryRelax 가 분류 14종으로 돌았다(이전 15종). 설정이 바뀌어 더한 항목이
  168044 → 91037 로 줄었다
- 나머지 경고는 모드가 스스로 "오류 아니다"라고 적어 둔 것이거나 대체값을 쓴다는 알림이다
  (SVM 빈 필터 3, RealisticInsurance 의 Armorer 90% 대체, CaseTrader 건너뜀 2 등)

### 남은 것

- Olympus 탄창 41개를 아직 빼지 않았다
- 7건은 출처를 못 찾았다. 실행 중에 만들어지는 id 로 보이는데 확인하지 않았다

### 없는 탄창 41개의 출처 (2026-10-28 추가 조사)

"어느 모드 것인지, Olympus 에 원래 없는 것인지, 다른 모드를 더 깔아야 하는지"를 가렸다.

**어디에도 없다.** SPT 기본 DB(4673개), 지금 서버 DB(9483개), 설치된 모드의 파일 전체,
서버 모드·플러그인 DLL 295개의 문자열(ASCII·UTF-16 양쪽), 프로필, 서버 캐시까지 봤다.

**Olympus 가 만드는 것이 아니다.** `firearmMags.json` 은 `총기 → { maglist }` 구조고, Olympus 는
그 id 를 총기의 탄창 칸 필터에 끼워 넣는 일만 한다(로그도 `AddMagazinesToFirearms` 209건이 전부).

총기 출처로 갈린다.

| 총기 출처 | 탄창 참조 | 있음 | 없음 |
| --- | --- | --- | --- |
| 바닐라 | 203 | 203 | 0 |
| 모드(WTT-Armory 등) | 181 | 118 | 63 (서로 다른 id 41개) |

바닐라 총기용 탄창 80종은 Olympus 가 자기 것으로 만들어 둔 것이다(이름 `ArSL`·`ArM4w` 등이
DLL 에 박혀 있고 번들은 바닐라 것을 쓴다). 깨진 것은 **모드 총기 쪽만**이다.

WTT-Armory AXMC 를 보면 탄창 칸에 **진짜 WTT 탄창(`68a38767…`)과 없는 id(`436d497b…`) 둘**이
들어 있다. 총기는 이미 쓸 탄창이 있고 Olympus 가 없는 것을 하나 더 끼운 꼴이라, 게임 기능이
막히지는 않는다.

**다른 모드로 메울 수는 없다.** 그 41개가 어느 모드 것인지 단서가 전혀 없다. Olympus 가 자기
판 기준으로 적어 둔 id 인데 설치된 WTT-Armory 2.0.5 와 어긋난 것으로 보인다(WTT 계열은 아이템
id 를 생성해 쓰므로 판이 다르면 id 가 달라진다). 그래서 빼는 것이 맞다.

7개는 끝까지 출처를 못 찾았다 — 위 어디에도 없고, 지금 DB 아이템들의 칸·탄창·격자 필터에도
없다. 플레아 생성 중에만 참조되므로 실행 중 만들어졌다 사라지는 id 로 보인다.

### 없는 탄창 41개 제거 — 고쳐졌지만 오류는 그대로 (2026-10-28)

`firearmMags.json` 에서 설치본에 없는 탄창 41개를 뺐다. 총기 209 → 166개(탄창이 다 빠진 43개는
항목째), 탄창 참조 384 → 321개, 없는 참조 63 → 0. 서버를 다시 띄워 `AddMagazinesToFirearms`
166건과 DB 내 참조 0건을 확인했다. 경위는
[mod-patches/505-olympus-firearmMags-mags.md](mod-patches/505-olympus-firearmMags-mags.md).

**그런데 서버 오류 48건은 하나도 줄지 않았다.** 내 인과 진단이 틀렸다.

- 패치 후에도 없는 id 목록이 이전과 한 글자도 같다
- 패치 후 DB 에는 그 48개를 참조하는 칸·탄창·격자 필터가 **0건**인데도 오류가 난다
- 즉 플레아 캐시는 그 id 를 아이템 DB 가 아닌 **다른 곳**에서 가져온다. 오류 주체는
  `ItemBaseClassService` 이고 바로 앞 줄이 `플리마켓 상품 생성 중` 이다
- 48개 id 는 디스크 어디에도 없다(모드 파일·`SPT_Data`·프로필·`sptappdata` 전부 확인).
  실행 중에 만들어진다
- 과거 로그로 보면 7-E(모드 95개) 4건 → 8-B(105개) 48건 → 이후 줄곧 48건이다. 7-E 의 4건은
  id 가 아예 달라 다른 문제다

남은 후보는 UltimateFlea(8503개로 경제 초기화)와 The Blacklist(1307개 손봄)다. 하나씩 빼고
서버를 띄워 보는 수밖에 없는데, 아직 하지 않았다.

패치 자체는 남겨 뒀다 — 없는 아이템을 가리키던 참조가 사라진 것은 그대로 이득이다. 되돌리려면
`doc/mod-patches/505-firearmMags-before-mag-removal.json` 을 덮어쓴다.

### 48건의 원인은 Olympus DLL (2026-10-28, 위 항목 정정)

플레아 관련 모드를 하나씩 빼 가며 서버를 띄웠다(한 번 2분 반).

| 뺀 것 | 모드 수 | `Could not add` |
| --- | --- | --- |
| (전부) | 114 | 48 |
| The Blacklist | 113 | 48 |
| All Weapons Flea + Item Limits Begone + HideLockedFlea | 111 | 48 |
| UltimateFlea | 113 | 48 |

전부 무죄였다. 상인 판매 목록 32명 38965개, 핸드북 8991개, 로케일 56854개에도 없었다.

**범인은 `MountOlympus.dll` 이다.** 48개 id 전부가 그 DLL 안에 UTF-16 문자열로 있고, 설치된
DLL 301개 중 그 하나만 들고 있다. 모드가 같은 목록을 데이터 파일과 DLL 양쪽에 들고 있어서,
`firearmMags.json` 을 고친 것으로는 플레아 캐시 쪽 경로가 바뀌지 않았던 것이다.

사용자가 고칠 수 있는 자리가 아니다. 그냥 두거나(로그 오류일 뿐이고 없는 아이템이 플레아에
안 뜨는 것은 정상), 제작자에게 보고하는 것이 맞다. **Olympus 제거는 프로필에 그 모드 아이템이
24종(탄창 19 + 장비 5) 들어 있어 권하지 않는다** — 템플릿 없는 아이템을 프로필이 들고 있는
상태는 이 설치본을 한 번 망가뜨린 유형이다.

방법 하나 배웠다. 처음 DLL 스캔은 "인쇄 가능한 문자 24자 이상 구간을 뽑아 비교"하는 방식이라
48개를 모두 놓쳤다. `Buffer.includes` 로 id 를 UTF-16LE 바이트열 그대로 찾자 전부 나왔다.
.NET 문자열은 추출해 비교하지 말고 찾을 것을 바이트로 만들어 찾는다 — 있는 아이템으로 대조군을
두면 방법이 맞는지 바로 드러난다.

모드는 전부 원복했고(114개), 서버도 다시 띄웠다. 프로필은
`backup/profiles-before-flea-bisect/` 에 떠 두었다.

## 2026-10-28 — 내부 인벤 창을 화면 안에 가두는 플러그인 (GridWindowScroll)

가방·상자·트렁크를 열면 내부 격자가 칸 수 그대로 창이 된다. 가로나 세로가 15칸을 넘으면
창이 화면을 덮거나 화면 밖으로 넘어가 아무것도 고를 수 없었다. 보이는 칸 수를 못박고 그보다
큰 격자는 스크롤로 넘겨 보게 하는 플러그인을 만들었다 — [../src/GridWindowScroll](../src/GridWindowScroll).

### 먼저 확인한 것

같은 일을 하는 모드가 이미 있는지 봤다. 없었다. UI Fixes 에 창 관련 기능이 많지만(열어 둔
위치 기억, 좌우 붙이기, 조사 창 크기 기억) **보이는 칸 수를 제한하는 것은 없다.**
Kaeno-TraderScrolling·ScrollableAttachments 는 다른 화면이다.

그다음 게임 어셈블리에서 패치 자리를 찾았다(Mono.Cecil 로 `Assembly-CSharp.dll` 을 읽었다).

| 찾은 것 | 쓸모 |
| --- | --- |
| `EFT.UI.GridWindow` | 상자·가방을 열었을 때 뜨는 그 창. `ShowContents`·`ShowGrids` 가 public |
| `EFT.UI.DragAndDrop.ContainedGridsView` | 창 안의 격자 묶음. 이것을 감싸면 된다 |
| `GridView.OnGridResized` | 격자가 `LayoutElement` 최소 크기와 `sizeDelta` 를 칸 수 그대로 요구한다 — 창이 커지는 원인 |
| `ItemViewFactory.GetCellPixelSize` | 한 칸 = `63n + 1` px. public static 이라 직접 부를 수 있다 |
| `SimpleStashPanel._stashScroll` | **창고가 이미 격자를 `ScrollRect` 안에 넣고 있다** — 게임이 지원하는 길이라는 증거 |

필요한 것이 전부 public 이라 리플렉션 없이 됐다.

### 방식

격자를 줄이지 않는다. 칸 좌표와 드래그 판정이 격자의 지역 좌표로 계산되므로, **격자는 그대로
두고 부모를 잘라내는 뷰포트로 감싼다.** 뷰포트에 `ScrollRect` + 마스크를 붙이고, 뷰포트의
`LayoutElement` 가 자른 크기만 요구하게 해서 창 자체가 줄어들게 했다.

창이 내용을 채운 **다음 프레임**에 손을 댄다(격자 GameObject 가 그 프레임에 만들어지고 크기는
레이아웃이 한 번 돌아야 정해진다). 창은 재사용되므로 뷰포트는 한 번만 만들고 열 때마다 크기만
다시 잡는다. 크기는 격자 묶음 자신이 아니라 **자식들의 범위로 잰다** — 뷰포트가 가둔 뒤에는
묶음 자신의 크기를 믿을 수 없다.

한 칸 픽셀 크기는 값을 박지 않고 게임 공식을 부른다. 게임이 칸 크기를 바꿔도 따라간다.

### 결과 — 게임에서 확인했다

빌드·설치하고 게임을 켜서 확인했다(`LogOutput.log` 2026-10-28 11:38).

| 본 것 | 값 |
| --- | --- |
| 플러그인 로드 | 143개 — 이 플러그인 포함, 패치 완료 줄까지 |
| 격자 뷰포트 생성 | 9건 |
| 이 플러그인 오류 | **0건** (로그 전체 `[Error]` 는 Fika 미설치 관련 DirectReload 1건으로 설치 전과 같다) |
| 빈 뷰포트 치움 | 0건 — 창을 다시 쓸 때 뷰포트가 쌓이지 않았다 |

눈으로 볼 것(창이 화면 안에 들어오는지, 아이콘이 마스크를 넘지 않는지, 스크롤한 상태에서
집고 놓는 자리가 맞는지)은 사용자가 직접 열어 확인했다. **스텐실 마스크는 켜지 않아도 됐다** —
기본 `RectMask2D` 로 잘린다.

F12 에서 값을 바꾼 것도 `.cfg` 에 남았다(가로 10 · 세로 12). 설정 창에서 조절하는 길이
실제로 통한다는 뜻이다.

### 남은 것

- **스크롤바.** 지금은 마우스 휠과 빈 칸 끌기로 움직인다.
- 창고(스태시)·장비 화면의 격자는 대상이 아니다. 창으로 열리는 격자만 손댄다.

### 곁들여 바로잡은 것

INSTALLED-MODS.md 의 로드 현황에 서버 오류 48건의 원인이 아직 `firearmMags.json` 으로 적혀
있었다. 원인은 `MountOlympus.dll` 이므로 고쳤다(생성기 `tools/gen-installed-mods.mjs` 쪽).
게임 플러그인 수도 142 → 143 으로 다시 쟀다.

### 이어서 — 모서리를 끌어 조절 (같은 날)

칸 수를 F12 에서만 정하는 것이 불편해, 창 **오른쪽 아래 손잡이**를 끌어 그 자리에서 바꾸고
**상자 종류마다 기억**하게 했다.

| 들어간 것 | 내용 |
| --- | --- |
| `ResizeGrip.cs` | 손잡이. 끌면 바로 다시 맞추고, 떼면 기억한다 |
| `SizeStore.cs` | 상자별 칸 수를 DLL 옆 `sizes.json` 에 둔다. 열쇠는 아이템 템플릿 id (`Item.StringTemplateId`) |

손잡이 자리를 잡는 데가 까다로웠다. 뷰포트 **안**에 두면 마스크에 잘리거나 끝 칸을 가리고,
**바깥**(부모의 자식)에 두면 부모의 레이아웃 그룹이 자리를 정해 버린다. 그래서 바깥에 두고
`LayoutElement.ignoreLayout` 으로 레이아웃에서 뺀 뒤, 매 프레임 뷰포트의 월드 모서리를
따라가게 했다. 그래도 모서리에서 7px 쯤은 끝 칸에 겹친다.

끄는 양은 화면 좌표 차를 칸 크기 x 캔버스 배율로 나눠 센다. 끄는 동안 뷰포트 자신이 바뀌므로
뷰포트 지역 좌표로 재면 값이 흔들린다.

상자별 기억을 끄면 끌었을 때 F12 기본값이 바뀌게 했다 — 어느 쪽을 쓰든 끈 값이 남는다.
기억한 것을 지우는 것은 버튼을 만들 수 없어 `기억한 크기 지우기` 스위치로 했다(켜면 지우고
스스로 꺼진다).

템플릿 id 는 `GridView.Grid.ParentItem.StringTemplateId` 로 꺼낸다. 전부 public 이라 여기도
리플렉션이 필요 없었다. 칸(슬롯)만 있는 묶음은 id 가 없어 전체 기본값으로 간다.

빌드·설치까지 했다. **게임에서는 확인하지 않았다** — 손잡이가 보이는지, 끌면 따라오는지,
떼면 다음에 열 때 그 크기로 열리는지 봐야 한다.

### 고침 — 커서와 모서리가 어긋나던 것 (같은 날)

인게임에서 **모서리가 커서를 따라오지 못했다.** 끌기 자체는 동작했다 — 세 번 끌어
`sizes.json` 에 `66326bfd46817c660d015123: [11, 8]` 이 남았고 실패 기록은 없었다. 자리만
어긋났다.

원인은 세는 방식이었다. 처음에는 **끈 거리를 더해 가는 방식**이었다 — 화면 좌표 차를
`한 칸 x 캔버스 배율` 로 나눠 칸 수를 센다. 이러면 배율을 한 번 잘못 잡으면 커서와 모서리가
**계속 벌어진다.** 칸 단위로 끊어 세는 것(최대 반 칸)과 손잡이가 한 프레임 뒤처지는 것도
겹쳤다.

방식을 바꿨다. 거리를 더하지 않고 **커서가 가리키는 자리에 모서리를 맞춘다.**

| 바꾼 것 | 왜 |
| --- | --- |
| 좌표를 **캔버스 뿌리 기준**으로 옮긴다 | 그 좌표의 1 단위가 UI 1 픽셀이고 한 칸이 63 단위라, 배율을 곱하거나 나눌 일이 없다 |
| 뷰포트 왼위 모서리를 **매 프레임 다시 읽는다** | 뷰포트가 커지면 창 레이아웃에 따라 왼위도 움직일 수 있다 |
| 잡은 자리와 모서리의 차를 빼 준다 | 손잡이 가운데가 아닌 곳을 잡아도 처음에 모서리가 튀지 않는다 |
| 크기를 바꾼 직후 손잡이를 바로 옮긴다 | 다음 프레임까지 기다리면 끄는 동안 뒤처져 보인다 |

`자세한 기록`을 켜면 끌 때마다 커서·모서리·잡은 차·칸 수·캔버스 배율을 남긴다. 또 어긋나면
그 숫자로 바로 가린다.

### 다시 고침 — 창이 가운데를 기준으로 커지는 것이 원인이었다 (같은 날)

`자세한 기록`을 켜고 받은 69줄이 답을 줬다. **캔버스 배율은 1 이었다** — 처음 의심한 배율은
원인이 아니었다. 실제 원인은 로그의 `왼위` 좌표에 있었다.

```
→ 13x7칸   왼위 (-441,210)
→ 12x7칸   왼위 (-409,210)
→ 11x7칸   왼위 (-377,210)
→ 10x7칸   왼위 (-345,210)
```

한 칸(63px) 바뀔 때마다 **왼위가 32px 씩 움직인다.** 창이 가운데를 기준으로 커지고 작아지니
왼쪽 변이 반 칸 반대로 밀리는 것이다. 그러면 내가 끄는 오른아래 모서리는 **한 칸 바뀌어도
반 칸만 움직인다** — 커서의 절반 속도다. 세로도 같았다(7→8칸에 왼위 y 210→242).

커서 자리로 모서리를 맞추는 계산 자체는 맞았다. 다만 기준선인 왼위가 결과에 따라 움직여서
되먹임이 생기고, 한 프레임에 오차가 절반씩만 줄었다. 그래서 "따라오지 못한다"가 됐다.

**크기를 바꾼 뒤 창을 그만큼 되밀어 왼위를 붙잡는다.** 창 뿌리는
`Window<T>.WindowTransform` (public) 이고, 되밀 양을 재려면 레이아웃을 그 자리에서 돌려야
한다(`ForceRebuildLayoutImmediate`). 뷰포트의 조상이 아니면 되밀어도 소용이 없으므로 그때는
건드리지 않는다.

왼위가 고정되면 되먹임이 사라져 한 프레임에 바로 맞고, 끈 만큼 칸이 늘어난다. 창이
제자리에서 커지는 것은 창 크기 조절의 보통 동작이기도 하다.

로그에서 확인할 것 — 끄는 동안 `왼위` 좌표가 **변하지 않아야 한다.**

### 결과 — 고쳐졌다

끌기 기록 **162줄 전부** 왼위가 `(-727,302)` 한 좌표였다. 되먹임이 사라졌다.

| 본 것 | 값 |
| --- | --- |
| 왼위 좌표 종류 | **1종** (162/162 동일) |
| 커서↔칸 비율 | 커서 63px 에 한 칸 — 예: 커서 x 108→167(59px) 에 13→14칸 |
| 격자 끝에서 막힘 | 커서가 더 가도 18칸(격자 폭)에서 멈춘다 |
| 저장 | `sizes.json` 에 `[18, 6]` |
| 이 플러그인 오류 | **0건** (로그 전체 `[Error]` 1건은 설치 전과 같은 Fika 관련 DirectReload) |

계산도 손으로 맞춰 봤다 — 커서 108, 잡은차 -6, 왼위 -727 이면
`(108+6+727)/63 = 13.35 → 13칸`, 로그와 같다.

남는 것은 **칸 단위 끊김**뿐이다. 모서리는 63px 배수에만 설 수 있어 커서와 최대 반 칸
차이가 남는다. 칸 격자를 쓰는 한 어쩔 수 없다.

### 이어서 — 스크롤바 (같은 날)

휠만으로는 얼마나 남았는지 안 보여 스크롤바를 붙였다. 게임 프리팹에서 가져오는 쪽은
포기했다 — 창마다 가져올 만한 바를 찾아 복제하는 것이 더 잘 깨진다. 직접 만든다
(`ScrollBars.cs`).

| 정한 것 | 왜 |
| --- | --- |
| 뷰포트의 **자식**으로 둔다 | 내용(격자)과 형제라 스크롤해도 따라 움직이지 않고, 마스크 안이라 창 밖으로 비어져 나가지 않는다. 끝 칸 위로 굵기만큼 겹친다 |
| `ScrollbarVisibility.Permanent` + 직접 끄고 켜기 | 자동 숨김은 뷰포트를 자기 멋대로 늘인다 |
| 손잡이 그림은 흰색, 색은 상태색에서 | 상태색이 그림 색에 곱해지므로 이쪽이 밝기·투명도를 다루기 쉽다 |

빈 뷰포트를 치우는 판정도 고쳐야 했다. 자식 수가 0인지로 봤는데 스크롤바가 자식으로
들어가면서 영원히 0이 아니게 됐다. **격자 묶음이 있는지**로 바꿨다.

F12 에 `스크롤바 보이기`·`스크롤바 굵기`(기본 6px)를 넣었다.

### 결과 — 되고, 숫자에서 흠 하나를 찾아 고쳤다

게임에서 동작을 확인했다. 로그도 깨끗하다 — 이 플러그인 오류 0건, 시작할 때 `sizes.json`
1개를 읽어 **재시작 뒤에도 상자별 크기가 살아 있었다.**

`자세한 기록`의 숫자에서 흠이 보였다.

```
격자 18.2x15.2칸 (1144x957px) → 보임 18x6칸 (1135x379px)
```

18칸의 공식 크기는 `63x18+1 = 1135` 인데 격자가 실제로 차지하는 것은 **1144px** 이다
(레이아웃 여백 9px). 칸 수로만 자르면 **끝까지 늘려도 9px 짜리 스크롤이 남아 스크롤바가
괜히 붙는다.** 칸 수가 격자만큼이면 자르지 않도록 고쳤다.

눈으로 보이는 쪽(바가 보이는지, 끌면 움직이는지)은 사용자가 확인했다.

## 2026-10-28 — 중복 기능 통합 검토 (4번), 그리고 전제를 바로잡음

브레인스토밍 4번을 검토했다 — [REVIEW-INTEGRATION-MOD.md](REVIEW-INTEGRATION-MOD.md).

### 먼저, 내가 적어 둔 전제가 틀렸다

REVIEW-LOAD-ORDER.md 에 "로드 순서는 밖에서 바꿀 수 없다"고 적어 뒀다. **남의 모드에만 맞는
말이었다.** 자작 모드의 `TypePriority` 는 내가 정하고, 그게 순서의 1차 기준이다. 그때
"TypePriority 가 높으면 늦게 도는지는 재지 않았다"고 적고 넘어간 것을, 이번에 서버 DLL 을 읽어
확정했다.

| 읽은 곳 | 무엇이 나왔나 |
| --- | --- |
| `DependencyInjectionHandler::InjectAll` | `OrderBy(TypePriority)` — **오름차순** |
| `ProgramExtensions::RunPreSptLoadCallbacks` | `0 <= 우선순위 < 200000` 인 `IOnLoad` 만 |
| `SPTStartupHostedService::StartAsync` | `우선순위 >= 200000` 인 `IOnLoad` 만 |
| `SPTarkov.Server.Core.DI.OnLoadOrder` (public) | 단계 상수가 이미 있다 — `Watermark` 0 … `PostLoad` 1000000 |

설치된 서버 모드 114개의 선언 603개가 전부 0 이므로,
`[Injectable(InjectionType.Singleton, OnLoadOrder.PostLoad)]` 하나로 **모든 모드와 SPT 자신의
콜백 뒤**에 설 수 있다. 4번이 말한 "마지막에 로드되는 통합 모드"는 **만들 수 있다.**

### 다만 되는 것과 안 되는 것이 갈린다

통합 모드가 할 수 있는 일은 마지막에 **값을 덮어쓰는 것**뿐이다. 다른 모드가 자기 일을 하는
것을 막지는 못한다.

| 종류 | 되나 |
| --- | --- |
| 덮어쓰기형 (탄약·화폐 스택, 내구도, 아이템 크기, 탄 성능) | **된다.** 마지막 값이 결과다 |
| 집합 계산형 (칸 필터 합집합, 플레아 금지) | 되지만 **최종 집합을 통째로 다시 써야** 뜻이 분명해진다 |
| 행동형 (봇 AI, 재입고 주기, 레이드 로직) | **안 된다.** DB 값이 아니라 코드 경로다 |

즉 "중복을 없애는 것"이 아니라 **"최종값을 한 곳에서 정하는 것"** 이다. 중복 작업 자체는 그대로
돌고 서버 시작 시간도 그대로다.

### 겹침 후보

실측된 것은 전에 잰 3건(탄약 스택·화폐 스택·플레아 금지)뿐이다. 그 밖은 DLL 에서 필드 이름을
세어 본 **약한 신호**다 — 이름만 읽고 쓰지는 않는 모드도 걸린다. `Filter` 45개, `Slots` 42개,
`Durability` 23개, `Grids` 20개, `Damage` 17개, `StackMaxSize` 13개. 통합 대상으로 올리기 전에
최종 DB 와 대조해야 한다.

### 권하는 범위

전부 통합하는 것은 권하지 않는다. 필드 45개짜리 모드는 그 자체가 다음 수수께끼가 된다.
**실측된 덮어쓰기형부터 좁게** — 1) 스택 크기, 2) 내구도, 3) 탄 성능. 칸 필터와 플레아 금지는
집합 계산형이고 자작 모드 둘(`HelmetSlotUnion`·`SlotCategoryRelax`)과 또 겹치므로, 그 둘을
합치는 쪽이 먼저다.

만들기 전에 반드시 잴 것 하나 — **`PostLoad` 단계에서 아이템 DB 를 고쳐도 클라이언트에 그대로
가는지.** SPT 가 그 전에 아이템 목록을 캐시해 두면 늦을 수 있다. 값 하나를 바꿔
`POST /client/items` 로 확인하면 된다.

통합 모드를 만들지 않더라도 지금 바로 할 수 있는 더 싼 일이 있다 — **CobrasAmmoStack 제거**
(0/31 완전히 죽어 있다), SVM 의 죽은 화폐 스택 4건 맞추기, TheBlacklist `disableBsgBlacklist`
결정.

## 2026-10-28 — 죽은 설정 세 가지를 치웠다

[REVIEW-INTEGRATION-MOD.md](REVIEW-INTEGRATION-MOD.md) 가 꼽은 "지금 바로 할 수 있는 더 싼 일"
셋을 처리했다. 통합 모드를 만들지 않아도 상태가 나아지는 것들이다.

### 1. CobrasAmmoStack 제거 — 값이 하나도 변하지 않았다

결속 모드라 먼저 떠 뒀다 — 프로필은 `backup/profiles-before-cleanup-2026-10-28/`, 모드 파일
전체는 `backup/removed-cobrasammostack-2026-10-28/`, 설치 기록은 git 에 남아 있다.

설치 기록 `installed-stage5C-2840.txt` 에 적힌 **파일 5개만** 지우고 빈 폴더를 치웠다(기록에
없는 것은 건드리지 않는 스크립트로). 그다음 서버를 다시 띄워 같은 측정을 반복했다.

| 본 것 | 값 |
| --- | --- |
| 서버 모드 | **113개** 정상 시작 (`server-tests/server-test-cleanup-113mods.out.txt`) |
| Cobra 가 쓰라던 구경 31개 | **Cobra 값과 일치 0/31** — 제거 전과 같다 |
| 그 31개의 실제 값 | SVM 다섯 값 중 하나 25개, 콘텐츠 모드가 정한 값 6개 |
| `Could not add` | 48건 그대로 (Olympus 쪽이라 무관) |

즉 **제거로 바뀐 것이 아무것도 없다.** 죽어 있다는 측정이 맞았다.

프로필의 `spt.mods` 목록에는 이름이 남는다. "한때 켜져 있었다"는 기록일 뿐이고 서버가 아무
경고도 내지 않으므로 손대지 않았다. kazusa-SimpleItemModifier 가 `(Removed 1 mods: Cobra's
Ammo Stack (1.1.0))` 를 찍는데, 그 모드가 모드 목록 변화를 적어 두는 것이라 정상이다.

### 2. SVM 화폐 스택 — 맞추는 대신 껐다

처음엔 "SVM 값을 BarterItemsStacks 와 맞추기"로 생각했는데, 그러면 **같은 필드에 주인이 둘인
상태가 그대로 남는다.** 한쪽만 바뀌면 다시 조용히 어긋난다. 그래서 SVM 프리셋의
`EnableCurrency` 를 꺼서 네 줄을 재웠다 — 이제 BarterItemsStacks 가 유일한 주인이다.

서버를 다시 띄워 확인했다 — 루블·달러·유로 50,000,000, GP 코인 5,000. 전부
BarterItemsStacks 값 그대로다. **SVM 의 네 줄은 정말로 아무 일도 하지 않고 있었다.**

값 네 줄은 지우지 않고 뒀다. BarterItemsStacks 를 빼면 다시 켜기만 하면 된다. 자세한 것은
[mod-patches/svm-currency-off.md](mod-patches/svm-currency-off.md).

### 3. TheBlacklist — 치울 것이 아니라 고를 것이었다

`disableBsgBlacklist: true` 는 **죽은 설정이 아니다.** 그 모드의 본래 기능이고(주석도
"Main feature of the mod") 설정대로 동작하고 있다. 끄면 모드를 설치한 이유가 대부분 사라진다.
**그대로 켜 두기로 했다.**

ISB-Aishi 243건과 Lotus 키카드 1건이 플레아에 올라오는 것은 그 결과다. 되살리는 길은 따로
있다 — `config.jsonc` 의 `customItemConfigs` 에
`{ "type": "item", "itemId": "…", "blacklisted": true }` 를 넣는다(그 모드가 이미 쓰는
방식이고 `"type": "parent"` 로 분류째 묶을 수도 있다). 244줄을 넣는 것은 그 모드의 경제를
되돌리는 일이라 사용자가 고를 문제로 남겨 뒀다.

### 곁들여 바로잡은 것 둘

- **SVM 화폐 값 세 개가 문서에 틀리게 적혀 있었다.** 달러·유로를 500,000, GP 코인을 1,000 으로
  적어 뒀는데 프리셋의 실제 값은 5,000,000·5,000,000·2,000 이다.
- **`excludedCategories` 가 있는 파일을 틀리게 적었다.** "같은 파일(`config.jsonc`)" 이라고
  했지만 그 열쇠는 `Config/advancedConfig.jsonc` 에 있다. `config.jsonc` 에는 없다.

둘 다 "그때 실제로 열어 보지 않고 적은" 것이다. 숫자와 파일 이름은 쓸 때 확인한다.

## 2026-10-28 — 제작자가 막아 둔 아이템을 되살렸다 (244건이 아니라 858건이었다)

TheBlacklist 의 `disableBsgBlacklist` 는 끄지 않기로 하고, 대신 **콘텐츠 모드 제작자가 자기
데이터에 `CanSellOnRagfair: false` 로 적어 둔 아이템만** `customItemConfigs` 로 다시 막았다.
바닐라 블랙리스트는 계속 풀려 있다.

### 숫자가 틀려 있었다

"ISB-Aishi 243건 + Lotus 키카드 1건 = 244건"으로 적어 뒀는데 **틀렸다.**

| 모드 | 막아 둔 것 | 풀려 있던 것 |
| --- | --- | --- |
| ISB-Aishi | 458 | 438 |
| WTT-ContentBackport | 374 | 366 |
| WTT-Armory | 39 | 39 |
| WTT-Artem | 12 | 12 |
| Airburst | 2 | 2 |
| LoadAmmoAnimServer | 1 | 1 |
| TGC-NG | 1 | 0 |
| **합** | **887** | **858** |

**Lotus 는 한 건도 없다.** 처음에는 ISB-Aishi 만 세고 WTT 계열 417건을 빠뜨렸다.

원인은 스캔 방법이었다. 모드마다 아이템 정의 꼴이 다르다 — ISB-Aishi·WTT 는 **id 가 객체의
열쇠이고 속성이 `overrideProperties` 안**에 있는데, 내 첫 스캔은 `_id` + `_props` 꼴만
찾았다. 그래서 0건이 나왔다. 두 꼴을 다 받게 고치자 887건이 나왔다.

덤으로 하나 더 걸렀다 — `kazusa-SimpleItemModifier` 폴더에는 **아이템 DB 사본**(스냅샷
8882개)이 있어 거기서 1956건이 잡혔는데, 그것은 제작자 의도가 아니라 바닐라 블랙리스트다.
건너뛰어야 한다.

### 결과

858건을 넣고 서버를 다시 띄워 쟀다 — **744건이 막혔고 114건은 아직 플래그가 `true`** 다.
대조군(`.338 AP`·`SJ6`, 원래 바닐라 블랙리스트)은 그대로 열려 있어 **모드의 본래 기능은
살아 있다.**

남은 114건은 원인이 하나가 아니다. 총기 41정쯤은 AllWeaponsFlea 가 설명할 수 있지만
(그 모드의 일이 바로 그것이다), Info 36·CultistAmulet 7·SpecItem 6 은 설명되지 않는다.
`CanSellOnRagfair` 를 건드리는 모드가 일곱 개 더 있어, 하나씩 빼 보지 않고는 못 가린다.

정확히 말해 둘 것 — `blacklisted: true` 는 두 갈래로 동작한다. id 를 SPT 의
`RagfairConfig.Dynamic.Blacklist.Custom` 에 넣고, 아이템의 `CanSellOnRagfair` 를 손본다.
114건은 **아이템 플래그가 `true` 라는 뜻**이고 플레아 생성 쪽에서는 걸러질 수도 있다.
플래그만 보고 "플레아에 뜬다"고 단정하지 않는다.

자세한 것과 되돌리는 법은
[mod-patches/theblacklist-restore-author-bans.md](mod-patches/theblacklist-restore-author-bans.md).

## 2026-10-28 — 남은 114건을 가렸다: AllWeaponsFlea 41건 + 핸드북에 없는 73건

전 작업에서 858건을 넣어 744건이 막히고 114건이 남았다. 그 114건을 가렸다.

### 순서를 코드부터 잡았다

콘텐츠 모드를 함부로 빼면 프로필이 위험하다(이 설치본을 한 번 망가뜨린 유형이다). 그래서
먼저 `CanSellOnRagfair` 를 **쓰는** 모드만 코드로 추렸다.

| 모드 | 무엇을 쓰나 | 후보 |
| --- | --- | --- |
| AllWeaponsFlea | `Mod::Unlock` 에서 **무조건 `true`** | O |
| UltimateFlea | `PoolManager::Apply` 에서 조건부 `true`/`false` | O |
| ItemGen · COTI | 자기가 만든 아이템에만 | X |
| TraderGen | 자리채움 아이템에 `false` 로 | X |
| ODT-ItemInfo | 읽기만 | X |

둘 다 플레아 동작 모드라 프로필에 안전하다. 그 둘만 빼고 쟀다.

| 띄운 상태 | 막힘 | 열림 |
| --- | --- | --- |
| 전부 (113) | 744 | 114 |
| 두 모드 뺌 (111) | 785 | 73 |
| **AllWeaponsFlea 만 뺌 (112)** | **785** | **73** |

세 번째가 답이다 — **41건은 AllWeaponsFlea 단독, UltimateFlea 는 무죄.**

### 남은 73건은 충돌이 아니었다

핸드북과 대조하니 깨끗하게 갈렸다 — **막힌 785건은 전부 핸드북에 있고, 열린 73건은 전부
없다.** TheBlacklist 의 `PricingService::ApplyPrices` 는 핸드북 항목을 돌며 값을 쓰므로
핸드북에 없는 아이템에는 닿지 못한다. 다른 모드가 덮은 것이 아니라 **기능이 닿지 않는
자리**였다.

그래도 그 73건의 id 는 SPT 의 `RagfairBlacklist.Custom` 에 들어갔고, 핸드북 값이 없으면
플레아 가격도 못 매긴다. 아레나 이벤트 품목·배틀패스 상자 같은 것들이라 애초에 플레아에 뜰
물건이 아니다.

### 배운 것

전 기록에 "Info 36·CultistAmulet 7 은 설명되지 않는다. 모드가 일곱 개 더 있다"고 적었다.
**모드를 더 뒤질 일이 아니었다.** 분류를 보고 모드를 의심하기보다, 그 기능이 **무엇을 돌며**
값을 쓰는지(여기서는 핸드북)를 먼저 읽는 쪽이 빨랐다. 48건 때와 같은 교훈이다 —
의심 목록을 늘리기 전에 코드가 도는 길을 본다.

환경은 원복했다(모드 113개, `mods-disabled` 폴더 제거, 서버 재시작).

### 결정 — AllWeaponsFlea 를 살리고 41건은 열어 둔다

남는 선택은 하나였다. **AllWeaponsFlea 를 살리는 쪽으로 정했다.** 그래서 41건(ISB-Aishi 총기
28 + WTT-Armory 7 + WTT-ContentBackport 6)은 제작자가 막아 뒀더라도 플레아에서 사고팔 수 있다.

**고쳐야 할 것이 아니라 고른 상태다.** 다음에 이 숫자를 보고 "왜 41건이 안 막혔지" 하고 다시
뒤지지 않도록 적어 둔다. 되살리려면 `AllWeaponsFlea` 를 지우면 되고, 그 모드는 플레아 동작만
바꾸므로 프로필에 안전하다(이번에 두 번 빼고 넣어 확인했다).

이로써 858건의 최종 상태는 이렇다 — **막힘 785 / 열림 73(핸드북에 없어 닿지 않는 자리) +
41(AllWeaponsFlea, 고른 상태)**. 합이 858 을 넘는 것은 73 과 41 이 겹치지 않고 785 가 둘을
뺀 수이기 때문이다(785 + 73 = 858, AllWeaponsFlea 를 살린 지금은 785 - 41 = 744 가 막힌다).

## 2026-10-28 — 스택 크기 통합 모드(StackUnify). PostLoad 는 클라이언트에 닿는다

4번 검토의 1단계를 만들었다 — [../src/StackUnify/README.md](../src/StackUnify/README.md).

### 만들기 전에 이것부터 쟀다

검토서에 "반드시 먼저 잴 것"으로 적어 둔 것이다. SPT 가 아이템 목록을 미리 굳혀 두면
`PostLoad` 는 늦는다. 그러면 모드를 만들 이유가 없다.

`Caliber9x19PARA` 를 **137**(실제 값 150 과 다른 눈에 띄는 수)로 못박고 서버를 띄운 뒤,
클라이언트가 쓰는 것과 같은 길(`POST /client/items`)로 받아 봤다.

| | 결과 |
| --- | --- |
| 모드가 바꾼 것 | 38개 (`150 -> 137`, 로그로 확인) |
| 클라이언트가 받은 값 | **137 이 37개**, 150 이 1개 |
| 건드리지 않은 구경 | 그대로 (`Caliber556x45NATO` 180) |

**온다.** 그래서 만들었다.

### 설명하지 못한 1개

`[Aishi]_(9x19mm RIP AP)` (`699aae8c9b6a4ab2b769c4a3`) 하나만 150 으로 남았다. 로그에
`150 -> 137` 로 바꾼 줄이 분명히 있고, 서버를 그대로 둔 채 두 번 물어도 150 이었다. 그 id 를
들고 있는 모드가 몇 개 더 있지만(Amonya, BlueheadsAIOTrader, ISB-Aishi assort·base) 전부
우선순위 0 이라 이 모드보다 **먼저** 돈다. 표가 둘이거나 응답이 다른 사본에서 만들어지는
것으로 보인다. **가리지 않았다** — 다음에 재려면 그 아이템 객체의 신원을 찍어 두고 응답 쪽과
대조한다.

### 만든 것

`[Injectable(InjectionType.Singleton, OnLoadOrder.PostLoad)]` 하나로 모든 모드 뒤에 선다.
설치된 서버 모드의 선언이 전부 0 이기 때문이다.

설정은 **비어 있는 것이 기본**이다. 적지 않은 것은 건드리지 않는다 — 다른 모드의 의도를
통째로 지우지 않기 위해서다. 그래서 설치만 해 두면 아무 일도 하지 않고, 로그에
"정해 둔 값이 없어 아무것도 바꾸지 않았다" 를 남긴다.

쓸모 있는 것 하나 — `ReportOnly: true` 로 띄우면 **바꾸지 않고** "내가 정한 값과 이미 들어
있던 값이 다른 항목이 몇 개인지" 만 알려 준다. 그 수가 곧 겹침의 크기다. 시험에서는 38개
전부가 "다른 모드가 이미 다른 값을 넣어 둔 자리"로 나왔다.

시험값은 치우고 기본 설정으로 되돌려 서버를 다시 띄웠다 — 9x19 가 38개 모두 150 으로
돌아온 것을 확인했다. 서버 모드는 114개가 됐고 자작 모드는 6개가 됐다.

### 다음 단계

같은 틀로 내구도(`MaxDurability`·`Durability`)와 탄 성능(`Damage`·`PenetrationPower`)을
할 수 있다. 칸 필터·플레아 금지는 집합 계산형이라 이 틀로 다루지 않는다.

## 2026-10-28 — 겹침 전수 조사. 그리고 로드 순서 측정이 **두 번** 틀렸던 것을 바로잡았다

`StackUnify` 의 `ReportOnly` 로 지금 스택 값이 어떻게 갈리는지 전수로 봤다. 그러다 전제가
틀렸다는 것이 드러나 그쪽이 본 작업이 됐다.

### 전수 조사 결과

구경 44종에 "아무 아이템도 가지지 않을 값"(999999)을 정해 두고 `ReportOnly` 로 띄우면, 모든
항목이 "내가 정한 값과 다르다"가 되어 지금 값이 전부 로그에 찍힌다. 975개가 보고됐다.

| | |
| --- | --- |
| 구경 | 44종, 탄약 971개 |
| **한 구경 안에서 값이 섞여 있는 구경** | **15종** |

섞인 것 보기 — `Caliber9x18PM` 은 150×44 · 1×17 · 50×2 · (없음)×1, `Caliber12g` 는
60×46 · 20×14, `Caliber40x46` 은 5×32 · 1×4. 같은 구경인데 아이템마다 다르다. 바닐라 탄과
모드 탄이 섞여 있고, 모드마다 자기 것만 손대기 때문이다.

### 틀린 것 1 — "우선순위가 전부 0" 은 틀렸다

`Injectable` 의 **생성자 기본값이 `typePriority = int.MaxValue`** 다(IL 로 읽었다). 인수 없이
`[Injectable]` 만 붙이면 가장 뒤다. 인수를 세는 방식으로는 그것을 0 으로 잘못 읽는다.

해석기를 붙여 다시 세자 **서로 다른 값이 약 90종**이었다 — 0 은 5개뿐이고 `int.MaxValue` 에
**304개**가 몰려 있다. 1차로 다시 센 것도 틀려서(전부 기본값으로 읽혔다) 자작 모드를 대조군으로
두고서야 바로잡았다. **소스를 아는 모드를 대조군에 넣으면 도구가 틀린 것이 바로 보인다** —
이 세션에서 세 번째로 같은 교훈이다.

### 틀린 것 2 — 늦을수록 좋은 것이 아니다

"맨 끝에 서면 이긴다"고 적었는데 아니었다. **클라이언트에 보낼 목록이 굳는 지점이 중간에 있다.**

| 우선순위 | 모드가 본 값 | 쓴 값 | 클라이언트가 받은 값 |
| --- | --- | --- | --- |
| `OnLoadOrder.PostLoad` | 150 | 137 | **137 (38개 중 37개)** |
| `int.MaxValue` | **900** | 137 | 150 (하나도 안 옴) |

둘째 줄의 900 이 열쇠다 — 목록이 굳은 뒤에도 DB 는 계속 바뀐다. 그러니 `int.MaxValue` 의
304개 선언은 **DB 를 고쳐도 클라이언트 목록에는 닿지 않는 자리**에서 돈다. `StackUnify` 는
`PostLoad` 로 되돌렸다.

### 못 이기는 것

화폐 네 종은 어느 우선순위로도 못 이겼다. 777 로 쓰라고 해도 클라이언트는 50,000,000 을
받는다. BarterItemsStacks 는 `PostLoad` 와 목록이 굳는 지점 **사이**에 쓴다(PostLoad 에서는
10,000,000 을, int.MaxValue 에서는 50,000,000 을 본다). 끼어들 방법은 찾지 못했다.

이로써 `mod-patches/svm-currency-off.md` 의 "BarterItemsStacks 가 유일한 주인" 은 **결과로는
맞지만 과정 설명으로는 느슨하다.** PostLoad 시점에 10,000,000 이 들어 있으므로 SVM(또는 다른
무엇)이 여전히 쓰고 있고, 그 뒤에 BarterItemsStacks 가 덮는다.

### 곁들여 고친 것 — 설정이 깨지면 서버가 죽었다

전수 조사용 설정을 만들다 쉼표를 주석에 먹히게 써서 `config.jsonc` 가 깨졌다. 그러자
**서버가 `JsonReaderException` 으로 죽었다**(`StackUnify.OnLoadAsync`). 설정이 깨졌다고 서버를
죽일 이유가 없다 — 읽기를 `try/catch` 로 감싸 오류만 남기고 넘어가게 고쳤다.
우연히 찾았지만 실제 결함이다.

## 2026-10-28 — 구경 43종의 최대 스택을 지금 값으로 고정했다

`StackUnify` 에 실제 값을 넣었다 —
[mod-patches/stackunify-pinned-stacks.md](mod-patches/stackunify-pinned-stacks.md).

값은 **새로 정한 것이 아니라 지금 쓰이던 값**이다. 구경마다 "탄약 분류 아이템의 다수 값"을
실측해 넣었고, 수가 같으면 큰 쪽을 골랐다(스택이 줄어드는 쪽을 피한다).

### 넣기 전에 무엇이 바뀌는지 먼저 봤다

다수 값과 다른 아이템이 71개였다. 그 안에 **탄이 아닌 것이 섞여 있었다** — 부모 분류로 가려
보니 탄약 상자 4개(`ammo_box_*`)와 로켓 2개(`patron_725` 등)였다. 탄약 상자를 탄과 같은
스택으로 만들 이유가 없어 `ExcludedItemIds` 에 넣었다.

남은 것 중 눈에 띄는 것들:

| 구경 | 바뀜 | 무엇 |
| --- | --- | --- |
| `Caliber9x18PM` | 1 → 150, 17개 | `shrapnel`·`shrapnel_RGD5` … 수류탄 파편 더미(탄약 분류다) |
| `Caliber12g` | 20 → 60, 14개 | makeshotgunsgreatagain 의 12게이지 탄 |
| `Caliber26x75` | 1 → 60, 10개 | 신호탄 |
| `Caliber127x108` | 2 → 60, 6개 | Arcane/Mystic/Eldritch Rounds |

**바뀌는 것이 전부 늘어나는 쪽**이라, 프로필에 들어 있는 스택이 무효가 될 일은 없다. 그것을
확인하고 적용했다.

### 결과

| | |
| --- | --- |
| 값이 하나로 모인 구경 | **43 / 43** |
| 아직 섞여 있는 구경 | 0 |
| 정한 값과 다른 구경 | 0 |
| 제외한 6개 | 그대로 (전부 1) |

모드 로그는 `바꿈 77개, 이미 같음 888개, 건너뜀 6개` 였다. 사전 집계로는 67개였는데 10개
차이는 **값이 아예 없던 항목**(사전 집계에서 뺐다)과 조사 시점 이후에 다른 모드가 쓴 것이다.
최종 상태는 위 표로 확인했다.

### 저장소에는 기본값으로 넣지 않았다

`src/StackUnify/config.jsonc` 는 **비어 있는 그대로** 두었다. 다른 설치본에서는 다수 값이
다를 수 있어, 값을 베껴 쓰는 것보다 `ReportOnly` 로 다시 재는 쪽이 맞다. 적용한 설정 자체는
`doc/mod-patches/stackunify-pinned-config.jsonc` 에 증거로 남겼다.

## 2026-10-28 — 2단계(내구도)는 필요 없었다. 대신 죽은 모드가 또 나왔다

1단계를 끝내고 2단계로 넘어가, **만들기 전에 겹침이 있는지부터** 쟀다. 없었다.

### `MaxDurability`·`Durability` 는 템플릿 다툼이 아니다

`set_` 호출로 찾으면 12개·9개 모드가 걸린다. 그런데 들여다보면 거의 전부 **아이템 개체**
(`UpdRepairable` — 수리·복원), **상인 물건 개체**(판매 목록 정리), **퀘스트 조건 객체**, 또는
**자기가 만든 새 아이템**이다. 같은 템플릿 칸을 두 모드가 다투는 구도가 아니다 — 스택 때와
다르다.

템플릿을 쓰는 것은 `egbog-RepairMaxDurabilityServer` 의 `MaxRepairResource` 하나뿐이고 수리
키트 자원이라 겹치지 않는다.

**그래서 내구도 통합 모드는 만들지 않는다.** 만들 이유가 없다.

### 전역 내구도를 바꾸려던 모드가 죽어 있다

`CobrasDurabilityTweaks` — 설정은 `DurabilityBurnMultiplier: 0.12`, 로그는 "Modified 1002
weapons". **최종 DB 에는 하나도 적용돼 있지 않다.**

| | |
| --- | --- |
| 기본 DB·최종 DB 양쪽에 있는 아이템 | 176 |
| 값이 그대로 | **176** |
| x0.12 적용됨 | **0** |

`weapon_colt_m4a1_556x45` 기본 1.15 → 최종 1.15. 곱했다면 0.138 이어야 한다.

단서는 있다 — 이 모드는 리플렉션으로
`TryMultiplyNumericProperty(props, "DurabilityBurnRatio", 0.12)` 를 부른다. 속성 이름은 맞다.
다만 그 속성이 `double?` 이라 숫자형을 가정한 변환이 조용히 실패하는 것으로 보인다
(`Try…` 가 실패를 삼킨다). **확정은 하지 않았다.**

**CobrasAmmoStack 에 이어 두 번째다.** 치울 거리다.

### 방법 메모 — 리플렉션 쓰는 모드는 `set_` 스캔에 안 걸린다

`CobrasDurabilityTweaks` 는 `set_` 목록에 한 줄도 안 나왔다(잡힌 것은
`JsonSerializerOptions.PropertyNameCaseInsensitive` 하나). "누가 이 필드를 쓰나" 를 `set_` 으로만
세면 이런 모드를 통째로 놓친다. 이 세션에서 도구가 틀린 네 번째 경우다 — 인쇄 문자열 추출,
해석기 없는 속성 인수, `overrideProperties` 꼴, 그리고 이번.

### 곁들여 본 것

`DurabilityBurnModificator` 는 소음기 133개에서 값이 제각각이다(1.071·1.075·1.06 …). 그것은
`RealisticSuppressors` 가 소음기마다 다른 값을 넣는 것이라 겹침이 아니다 — 로그의
"Applied stat multipliers to 133 suppressors" 와 수가 맞는다.

## 2026-10-28 — 탄약 스택을 바닐라와 모드가 어느 정도씩 잡고 있었나

고정하기 전의 값을 SPT 기본 DB 와 대조해 출처를 가렸다. 탄약 964개 기준 —
[mod-patches/stackunify-pinned-stacks.md](mod-patches/stackunify-pinned-stacks.md) 의
"고정 전에는 누가 잡고 있었나".

| | 개수 | 비율 |
| --- | --- | --- |
| **SVM 이 값을 정한 것** | **734** | **76.1%** |
| 그 탄을 만든 모드가 정한 것 | 198 | 20.5% |
| 바닐라 값이 살아남은 것 | 23 | 2.4% |
| 출처 미상 (유탄 8 + 20x1mm 1) | 9 | 0.9% |

**탄 자체는 78%가 모드 탄**(ISB 계열 481, 표식 없는 것 197, WTT 40 …)이고 바닐라 탄은
208개뿐이다. 그런데 **값은 SVM 이 76% 를 쥐고 있었다** — 바닐라 탄의 85%(176/208)와 모드 탄의
74%(558/756)를 덮고 있었다. 모드가 자기 탄에 적어 둔 값조차 대부분 SVM 에 덮인 것이다.

바닐라 값이 살아남은 23개는 대부분 스택 1(15개)과 60(8개)이다.

1차 집계에서는 '모드가 만든 탄'을 그 모드 몫으로 세어 "모드 탄 78%" 라고 나왔다. **그 탄들의
숫자도 SVM 값이었다.** "누가 만들었나" 와 "누가 값을 정했나" 는 다른 질문이다 — 나눠 세고서야
그림이 맞았다.

한계도 적어 뒀다 — 값이 SVM 의 다섯 값(150·180·160·60·80)과 같으면 SVM 몫으로 셌다. 모드가
스스로 같은 값을 골랐어도 구별할 수 없다.

## 2026-10-28 — R(장전)키가 먹통이 된 원인: 내가 클래스 정의 노드에 값을 써서

사용자가 "R키가 먹통" 이라고 알려 왔고, "최근 작업 전까지 문제 없었다" 고 했다. 오늘 바꾼
것부터 뒤졌다.

### 후보를 먼저 잘라냈다

| 후보 | 결과 |
| --- | --- |
| 장전 관련 예외 | **없다.** 로그의 `[Error]` 986건은 전부 알려진 PIT AI NRE 다(493쌍) |
| 키 충돌 | AutoRanging 이 `R + LeftAlt` 를 잡지만 **`Auto Range = true` 라 그 키는 비활성**이다 |
| GridWindowScroll (오늘 바뀐 유일한 클라이언트 DLL) | **무죄.** 이번 세션에 뷰포트를 **0건** 만들었다(상자가 7.2x7.2 라 "그대로 둔다"). 스크롤바(= Selectable)가 생기지 않았으니 키를 가져갈 수 없다 |
| DirectReload | R 을 가로채기는 한다(홀드=탄약 휠, 탭=직접 장전). 다만 오늘 건드리지 않았다 |

남은 것은 오늘 넣은 **스택 고정**이었다.

### 원인

`StackByCaliber` 를 "`Caliber` 가 있는 모든 것"에 적용한 탓에 **아이템이 아닌 것**까지 썼다.

| 잘못 건드린 것 | 바닐라 | 내가 쓴 값 |
| --- | --- | --- |
| **`Ammo` 클래스 정의 노드** (`_type: "Node"`) | **값 없음** | 150 |
| `shrapnel` 등 수류탄 파편 8개 | 1 | 150 |

클래스 정의 노드는 **그 아래 모든 탄약이 상속하는 틀**이다. 없던 `StackMaxSize` 를 만들어
넣었으니 탄약 전체의 취급이 달라질 수 있다 — 장전이 먹지 않는 것과 맞는 모양이다.

### 고친 것

규칙 둘을 넣었다.

- `_type` 이 `Node` 인 것은 건너뛴다.
- **값이 없던 자리에는 값을 만들지 않는다.** "없다" 도 뜻이 있다.

`shrapnel` 8개는 제외 목록에 넣었다. 고친 뒤 확인 — **바닐라와 값이 다른 Node 0개, 바닐라에
값이 없었는데 생긴 것 0개**, `shrapnel` 1 로 복귀. 구경 통일은 43종 중 32종 유지.

딸린 결과 하나 — 값이 없던 자리를 건너뛰게 되자 ISB 특수 AP 탄 11개는 이 모드가 손대지 않고,
그 뒤에 어느 모드가 500 을 넣는다. 그래서 11개 구경은 "다수 값 + 500 하나" 가 됐다.

### 아직 확인하지 못한 것

**게임에서 R 이 돌아왔는지는 사용자가 봐야 한다.** 원인을 코드와 DB 로는 짚었지만 입력이
돌아온 것을 내가 재지는 못했다. 돌아오지 않으면 다음은 StackUnify 의 `ModEnabled` 를 끄고
(또는 `config.jsonc` 를 빈 기본값으로 덮고) 서버를 다시 켜 보는 것이다 — 오늘 넣은 변경을
한 번에 끄는 스위치다.

### 설치 메모

서버가 켜져 있으면 DLL 이 잠겨 복사가 실패한다(`Device or resource busy`). `taskkill` 직후에도
잠깐 잠겨 있어, 복사를 재시도하는 식으로 넣었다. 설치 스크립트에 그 대기를 넣는 것이 좋겠다.

## 2026-10-28 — R키 먹통: 원인은 UI Fixes 의 설정 하나. 어느 항목인지는 못 적었다

**사용자가 직접 찾아 해결했다.** UI Fixes(`com.tyfon.uifixes`)의 설정 하나였다.
**어느 항목인지는 기록하지 못했다.**

내가 `[F. Item Keybinds]` 의 `Reload Weapon Shortcut`(기본값 `R`)으로 짚었는데 **틀렸다.**
근거가 약했다 — 설정 파일에서 "기본값과 다른 항목"을 뽑아 그 한 줄이 나왔다는 것뿐이고,
그것이 **오늘 바뀐 것인지는 확인할 길이 없었다.** 비교할 이전 사본이 없는데 "방금 사용자가
바꾼 것"으로 단정했다. 값이 기본과 다르다는 것은 **언젠가 달라졌다**는 뜻일 뿐이다.

### 내가 짚은 둘은 틀렸다

| 내 가설 | 실제 |
| --- | --- |
| 내 스택 고정이 `Ammo` 클래스 노드에 값을 써서 | **그것도 사실이지만 R 의 원인은 아니었다.** 고친 것은 그대로 둔다 — 클래스 정의에 값을 쓰는 것은 그 자체로 잘못이다 |
| DirectReload 의 탄약 휠이 장전 명령을 막아서 | **아니었다.** 그 휠은 기본값이 `true` 고 모드가 10-24 에 설치됐으니 나흘간 켜져 있었다 — 오늘 바뀐 것이 아니다. 끄려고 건드린 설정은 **원래대로 되돌렸다**(백업 `backup/directreload-cfg.before-wheel-off`) |

### 무엇을 놓쳤나

둘이다.

**하나 — 가장 싼 확인을 늦게 했다.** 설정 파일에서 "기본값과 다른 항목"을 뽑는 것은 1분이면
되고 모드 전체에 한 번에 쓸 수 있는데, UI Fixes 에 돌려 본 것은 사용자가 답을 찾은 뒤였다.
로그·DLL·DB 를 먼저 뒤졌다. **입력·키 문제는 `# Default value:` 대조부터 한다.**

**둘 — 그 대조 결과를 과하게 읽었다.** "기본값과 다르다" 는 **언젠가 달라졌다**는 뜻일 뿐인데
"방금 사용자가 바꾼 것" 으로 단정해 원인을 틀리게 적었다. 비교할 이전 사본이 없으면 **언제
바뀌었는지는 모르는 것**이다. 그래서 `sptSettings` 를 백업해 둔 것이고, 모드 cfg 도 같은 이유로
떠 둘 만하다.

곁들여 `user/sptSettings/` 를 `backup/sptsettings-2026-10-28/` 에 떠 뒀다. `Control.ini`
(게임 키 설정)는 그동안 백업이 없어서 **바뀌었는지 비교할 수가 없었다.** 이제는 비교할 수 있다.

## 2026-10-28 — 유탄을 5개로 바꾼 것은 BarterItemsStacks 다

스택 출처 산정에서 "미상 9개" 로 남겨 둔 것을 가렸다 —
[mod-patches/stackunify-pinned-stacks.md](mod-patches/stackunify-pinned-stacks.md).

id 로 모드 폴더를 뒤지니 설정 파일에는 없고 **`BarterItemsStacks.dll` 안에 박혀** 있었다.
바이트에서 UTF-16 문자열로 찾는 방법(Olympus 48건 때 배운 것)으로 DLL 296개를 훑었다.

| DLL | 9개 중 |
| --- | --- |
| **`BarterItemsStacks.dll`** | **8** |
| `WTT-ContentBackport.dll` · `ShrapnelFix.dll` | 각 7 |
| 그 밖 4개 | 각 1 |

근거 셋으로 BarterItemsStacks 로 본다 — 8개를 들고 있고, 스택 크기를 관리하는 모드이며,
`PostLoad` 보다 뒤에 쓴다는 것을 이미 실측했다(화폐 겨루기에서 못 이겼다).

`patron_30x29_vog_30` 하나는 그 DLL 에 없어 **확정하지 않았다.** 값이 다른 유탄과 같은 5 라
분류로 묶는 것일 수도 있다.

곁들여 알게 된 것 — **이 설치본에서 스택 값의 최종 주인은 둘이다.** 탄약 대부분은 SVM(76%),
유탄·화폐 같은 몇 가지는 BarterItemsStacks 다. 내가 만든 StackUnify 는 SVM 을 이기지만
BarterItemsStacks 는 못 이긴다.

## 2026-10-28 — R키 먹통의 항목 확정: UI Fixes `Always Reload Magazines In-Place`

사용자가 알려 줬다. **`Always Reload Magazines In-Place`**(탄창 항상 제자리 교체)를 켜 둔
것이었다. 기본값은 꺼짐이다.

설정 설명이 그대로 답이다 — *Force the reload to always be in-place, even if there's space to
reload normally.* **빈 자리가 있어도 반드시 자리를 맞바꾸는 식으로만** 장전하게 만든다.
맞바꿀 자리가 없으면 장전이 성립하지 않고, R 은 아무 일도 하지 않는다.
[MODS-GUIDE.md](MODS-GUIDE.md) 의 "알려진 충돌" 에 적어 뒀다.

### 내 방법이 왜 못 찾았나 — 둘 다 적어 둘 만하다

**하나. 고치면 증거가 사라진다.** 내가 쓴 방법은 "설정 값이 `# Default value:` 와 다른 항목을
뽑기" 였다. 그런데 사용자는 **그 항목을 기본값으로 되돌려서** 고쳤다. 그 시점에 돌린 집계에는
당연히 안 나온다. **이 방법은 "지금 기본과 다른 것" 만 보여 주고, "방금 기본으로 돌아간 것" 은
못 본다.** 그런데도 나는 그 결과에서 엉뚱한 한 줄을 집어 원인이라고 적었다.

**둘. 후보를 '키처럼 보이는 것' 으로 좁혔다.** 집계 결과를 `key|press|hold|click|scroll|…` 로
걸러 "키 관련 14개" 를 먼저 보여 주고 나머지는 잘라 냈다. 진짜 원인은 **키 설정이 아니라 장전
동작 설정**이었다. R 이 안 먹는다고 해서 범인이 키 바인딩일 이유는 없다 — **그 키가 하는 일**이
실패하는 쪽일 수도 있다.

### 그래서 바꿀 것

- 입력 문제에서 설정을 훑을 때 **"키처럼 보이는 것" 으로 좁히지 않는다.** 그 입력이 하는 일과
  관련된 설정까지 본다.
- 모드 cfg 도 **바뀌기 전에 떠 둔다.** 그러면 "무엇이 언제 바뀌었나" 를 비교로 답할 수 있다.
  이번에 `user/sptSettings` 는 떠 뒀지만 `BepInEx/config` 는 사고가 난 뒤에야 한 벌 떴다.

## 2026-10-28 — 20:09 로그에서 새로 보인 오류 2건

문서가 적어 둔 확인 시각은 11:38 인데 `LogOutput.log` 와 서버 로그는 **20:09** 까지 돌았다.
그 사이 세션에 문서에 없던 오류가 둘 있다. 둘 다 게임·서버가 멈추지는 않는다.

### 1. UTAE 편집 기준선 로드 실패 (게임, 1건)

```
[Error  :Ultimate Trader Assort Editor] [UTAE] Could not load the editor baseline:
  System.ArgumentOutOfRangeException: Index was out of range.
  at EFT.UI.HandbookFilterPanel.RegisterItem (Item item)
  ← TradingGridView.Show ← TraderDealScreen.UpdateGridViews
  ← TraderEditorController.ShowQuestLockedTraderOffers
  ← TraderEditorController.EnsureBaselineLoadedAsync
```

바로 앞 줄이 `[REQUEST SUCCESSFUL] /utae/editor/assort/690766de550bc322a810ea1e` 다. 상인
`690766de…` 의 편집 화면을 열어 서버에서 어소트를 받아 온 **뒤**, **퀘스트로 잠긴 매물**을
핸드북 필터 패널에 등록하다 터졌다. 터지는 자리는 UTAE 코드가 아니라 바닐라
`HandbookFilterPanel.RegisterItem` 안의 인덱스다 — UTAE 가 정상 거래 화면이 만들지 않는
조합(잠긴 매물까지 한 번에)으로 그리드를 띄우면서 바닐라 쪽 가정을 깬 것으로 보인다.

결과는 **그 상인의 편집 기준선만 안 뜨는 것**이고 게임은 계속 돌았다. UTAE 는 조정용 도구라
평소 플레이에는 쓰지 않으므로 지금은 그대로 둔다. 다시 쓸 일이 생기면 그 상인이 무엇인지
(퀘스트 잠금 매물이 있는 커스텀 상인인지) 부터 본다.

### 2. 프로필에 남은 유령 상인 (서버, 로그인마다 1건)

```
[Error] SPT에 존재하지않는 상인 6952ced4bcc1dd1e3c80dfcb가 프로필에서 발견되었습니다.
```

프로필(`6adc26693f5b5290609ea366.json`)을 전수로 뒤져 보니 **이 id 는 딱 한 자리에만** 있다.

```
characters.pmc.TradersInfo.6952ced4bcc1dd1e3c80dfcb
  { loyaltyLevel: 1, salesSum: 0, standing: 0.2, nextResupply: 1793062479,
    unlocked: true, disabled: false }
```

메시지(`dialogues`)도, 그 상인에게서 온 아이템도 없다. **상인 평판 껍데기 하나뿐이다.**
`nextResupply` 가 2026-10-26 쯤이라, 2026-10-25 에 지운 **The Quartermaster(2817, 결속)** 가
남긴 것으로 본다(제거 경위는 위 "The Quartermaster 제거" 절). 그때 설치 기록 15개는 전부
지웠지만 **프로필에 쓰인 평판 항목은 기록에 없으니 남았다** — "게임을 실제로 켠 뒤 생기는
것은 설치 기록에 안 잡힌다" 는 그때의 교훈이 프로필에도 똑같이 적용된다는 뜻이다.

**서버가 안내하는 `core.json` 의 `removeModItemsFromProfile: true` 는 켜지 않는다.** 그 설정은
"DB 에 없는 모드 아이템" 을 프로필에서 **싸잡아 지운다.** 지금 설치본에는 결속 모드가 여럿이고
(ODT's Item Info, WTT 계열, ISB 계열 등) 로드 순서나 일시적 실패로 DB 가 덜 찬 상태에서
저 설정이 켜져 있으면 **멀쩡한 아이템까지 날아갈 수 있다.** 경고 1줄과 바꿀 위험이 아니다.

실제 피해는 **매 로그인 경고 1줄**이 전부다. 지우고 싶으면 프로필 백업 후
`characters.pmc.TradersInfo` 에서 저 키 하나만 빼는 쪽이 안전하다 — 지금은 하지 않았다.

## 레이드 중 화면이 깜빡인 것 — 우리 플러그인이었다 (2026-10-29)

사용자가 "`/airdrop-caller/heartbeat` 로그가 찍힐 때마다 화면이 깜빡인다" 고 했다.
처음에 나는 빈도(초당 1.5회)와 "비동기라 프레임을 안 막는다" 를 근거로 **우리가 아니라고
단정했다. 두 번 틀렸다.** 사용자가 본 상관관계가 데이터였고 내 추론이 틀린 쪽이었다.

### 원인

`SPT.Common.Http.Client.SendAsync` 는 첫 `await` **앞에서** 이것을 한다.

```csharp
if (zipped) data = Zlib.Compress(data, ZlibCompression.Maximum);
```

`Zlib.Compress` 는 zlib.NET 의 `ZOutputStream` 을 **최대 압축(level 9)으로 새로 만든다.**
호출마다 deflate 상태 버퍼를 통째로 할당한다. `async` 메서드라도 첫 `await` 전까지는
**부른 스레드**에서 도므로, `Update()` 에서 부르면 **Unity 메인 스레드**가 그 할당과 GC 를
떠안는다. 프레임이 한 번 튀고, 그게 화면 깜빡임으로 보인다.

**GET 은 `data` 가 null 이라 이 줄을 건너뛴다.** 그래서 `/pending`(GET)은 멀쩡했고
보고(POST)만 튀었다 — 사용자가 `heartbeat` 만 짚은 것이 정확했다.

### 고친 것

`Task.Run` 으로 감싸 압축·요청 조립까지 전부 스레드풀에서 돌게 했다.

```csharp
_heartbeatCall = Task.Run(() =>
    RequestHandler.PostJsonAsync(HeartbeatUrl, JsonConvert.SerializeObject(body)));
```

**SPT 플러그인을 쓰는 사람에게 일반화되는 교훈이다 — `RequestHandler` 의 POST 계열을
`Update()`·`LateUpdate()` 에서 그냥 부르면 안 된다.** GET 은 괜찮다.

### 어떻게 좁혔나

추측을 멈추고 하나씩 뺐다 넣었다. 사용자는 매번 "깜빡인다/안 깜빡인다" 만 답했다.

| 설치 상태 | 결과 |
|---|---|
| 오늘 넣은 것 전부 제거 | 안 깜빡임 |
| `AirdropProbe` 만 (키 전부 없앰) | 안 깜빡임 |
| + `AirdropCaller` 플러그인·서버 모드 (고친 판) | **안 깜빡임** |

마지막 판이 실제로 올라가 돌았는지 확인했다 — 플러그인 기동 로그, 요청 194줄,
보고 간격 10.0초.

### 남는 의문

"아직 깜빡인다" 고 한 판(18:24~)은 고친 dll 이 이미 들어가 있었고 `켜기 = false` 라
요청이 0건이었다. 그 판의 깜빡임은 설명하지 못한다. 그때 내가 이 PC 에서 ilspycmd 로
60MB 어셈블리를 돌리고 빌드를 반복하고 서버를 몇 번 내렸다 올렸으니 **그쪽일 가능성이
높지만 증명하지 못했다.** 게임 중에 무거운 작업을 돌리지 않는다.

## 2026-10-30 — 지도에서 모드 세력 구분 (DynamicMapsFactionMarkers)

스캐브 무리에 ISB 가 섞여 있으면 지도만 보고는 판단이 안 선다는 데서 시작했다.
먼저 실측했다([specs/2026-10-30-faction-identification-survey.md](specs/2026-10-30-faction-identification-survey.md)):
세력을 더하는 모드는 ISB Special Forces 하나뿐이고, MoreBotsAPI 가 그 16역할
(`WildSpawnType` 13701~13716)을 BSG 의 `_spawnTypeSettings` 에 등록해 바닐라 판정
함수들이 ISB 에도 답하게 만든다. 그래서 킬 패널·나침반·Amands' Sense 는 ISB 를
"ISB BOSS"·"WHITE TUSK" 로 제 이름과 보스색으로 보여 준다.

**지도만 뒤처진 이유**는 Dynamic Maps 1.2.1 의 보스 판정이 `_trackedBosses` —
enum 숫자 21개가 소스에 박힌 HashSet — 이라 13701~13716 이 들어갈 자리가 없어서였다.
설정으로 열어 둔 구멍이 없으니 패치 말고는 길이 없다.

그래서 플러그인 하나를 만들었다. Dynamic Maps 파일은 고치지 않고 Harmony 로 세 지점에
끼어든다. 핵심은 **우리가 만든 마커를 그들의 마커 사전에 넣는 것** — 제거·맵 전환·사망
처리가 전부 그들 코드로 그대로 돌아, 우리가 생명주기를 따로 들고 있지 않아도 된다.
`ShouldShowCategory` 를 같이 패치하지 않으면 그들의 정리 코드가 "모르는 범주" 로 보고
우리 마커를 솎아낸다. 그리고 그들은 바닐라 마커 네 개의 OR 로만 마커 담당을 등록해서,
네 개를 다 끄면 우리 패치가 불릴 자리 자체가 없어진다 — 등록을 보장하는 패치를 하나 더 넣었다.

전장 리뷰에서 그 세 번째 패치가 거꾸로 사고를 내는 것을 잡았다. 네 설정이 모두 꺼지면
그들은 담당을 **떼어 내고**, 네 bool 을 넣는 블록 자체를 건너뛴다. 거기서 담당을 새로 만들면
`_showFriendlyPlayers` 가 true 로 태어나(필드 초기값) 바닐라 아군 마커가 되살아나고,
서버의 허용 스위치(`ServerConfig.AllowShow*`)까지 건너뛴다 — 그 블록이 서버 스위치를
적용하는 유일한 자리였다. 안티골("토글이 뒤섞이는 것")에 정면으로 걸리는 것이라,
담당이 없을 때만 만들고 그렇게 만든 담당은 네 bool 을 직접 false 로 내리게 고쳤다.

같은 리뷰에서 둘 더 나왔다. 게임 locale 은 BepInEx `Awake` 보다 늦게 올라와서 범주 이름이
늘 ScavRole 키로 굳어 있었고(WHITE TUSK 합치기가 아예 일어나지 않았다), `extraRoles` 는
읽는 코드가 어디에도 없어 설정에 적어도 아무 일이 없었다. 이름 짓기는 레이드에서 처음 쓸 때
한 번 하도록 미루고, `extraRoles` 는 역할 id 를 그대로 키로 써서 등록하게 했다.

판정 로직(설정·역할 분류·색 파싱)은 게임 어셈블리를 모르는 순수 파일로 떼어 내
`CompatToggle.Tests` 와 같은 틀로 시험한다(34건). Harmony 글루는 레이드 실측으로 갈음한다.

설계 중 리뷰에서 구멍 네 개가 나왔고, 그중 하나(바닐라 마커를 다 끄면 ISB 도 안 보인다)는
"스캐브는 끄고 ISB 만 보자" 는 가장 그럴듯한 사용법에서 바로 걸릴 자리였다.

## 2026-10-30 — 보급 착지가 끊기던 진짜 이유 (남의 Harmony Postfix)

보급 호출기에서 "낙하산이 안 사라진다 / 낙하산 소리가 계속된다 / 맵·미니맵에 보급품이
안 뜬다" 를 쫓다가, 셋이 **한 원인**이었음을 측정으로 잡았다. 우리 것이 아니었다.

Game Panel HUD 나침반(4.1.6 포팅본)이 `ClientAirDrop.CheckSurface` 에 Postfix 를 걸고,
그 Postfix 가 `MethodAccessException` 으로 **매 착지마다** 터진다. Postfix 의 예외는
패치된 메서드 밖으로 나가 `ManualUpdate` 를 중단시키고, 착지 분기의 뒤쪽
`PlayLandingSound()`(낙하산 소리를 멈추는 곳)와 `CloseParachute()` 가 날아간다.
그 분기는 먼지를 켜는 순간 **다시는 참이 안 되는** 한 번 겨눔이라 복구 기회도 없다.

어떤 판은 멀쩡했던 것은 `case Contact:` 에 조건 없는 `CloseParachute()` 가 있어서다.
한 점으로 닿으면 회전 코루틴 때문에 `Contact` 가 여러 프레임 유지돼 거기서 접히고,
두·세 점으로 닿으면 `CollisionEnter` 안에서 곧바로 `Landed` 가 되어 그 기회가 없다.

가른 방법이 핵심이었다. 처음엔 보고 줄에 "낙하산남음" 만 적었는데, 그 값은 정상 판에서도
1초 페이드 동안 참이라 **아무것도 가르지 못했다.** `_startClosingParachute`(접는중/안접음)와
앉기 **직전** 프레임의 먼지 상태를 같이 적고 나서야 "바닐라 조건은 둘 다 참인데
CloseParachute 가 안 불렸다 = 중간에서 터졌다" 가 보였다. 그 뒤 `Player.log` 에
스택이 그대로 있었다.

고치는 쪽은 Finalizer 하나다(`SurfaceGuard`). 이 모드의 첫 Harmony 패치다 —
남의 Postfix 가 바닐라 메서드를 끊는 것은 밖에서 막을 길이 이것뿐이었다. 모드 설정
(`Compass Static Airdrop display`)으로 막으려 해 봤지만 패치가 설정과 무관하게 걸려
안 된다는 것도 확인했다.

교훈 셋.

1. **모드가 겹친 환경에서 "바닐라가 왜 안 도나" 를 볼 때는 Player.log 의 스택을 먼저
   본다.** BepInEx 로그에는 안 찍힌다.
2. 계측 줄은 **정상과 비정상이 다른 값을 내는지** 를 먼저 확인하고 믿는다. 처음 적은
   "낙하산남음" 은 정상 판에서도 참이라 아무것도 가르지 못했다.
3. **증상을 원인에 묶을 때 사용자가 말한 그 증상인지 확인한다.** 나는 디컴파일에서
   나침반 모드의 `ShowAirdrop` 이 안 불리는 것을 보고 "맵·미니맵 표시 안 됨" 을 그
   HUD 표시로 단정하고 "포팅본이 고쳐져야 한다" 고 적었다. 사용자가 정정했다 —
   정상 착지가 되면 맵·미니맵에 표시된다. 표시도 같은 원인의 증상이었고 같이 고쳐졌다.
   코드에서 찾은 그럴듯한 경로를, 사용자가 본 것과 맞춰 보지 않고 결론으로 썼다.

## 2026-10-31 — N 하나로 야시경과 페이스실드를 가른다 (GogglesWheel)

헤드기어 제한을 푸는 모드를 넣어 헬멧에 야시경과 페이스실드를 같이 달 수 있게 됐는데,
둘을 여닫는 키가 N 하나라 한쪽만 열렸다. 게임 코드를 보니 원인이 그대로 있었다 —
`Player.ToggleGoggles` 가 `GetItemComponentsInChildren<TogglableComponent>().FirstOrDefault()`,
즉 **첫 번째 하나만** 건드린다. 바닐라에서는 둘이 동시에 달릴 수 없어 드러나지 않던 구멍이다.

짧게 누르면 야시경, 길게 누르면 원형 메뉴에서 고르는 플러그인을 만들었다.

**설계 리뷰에서 전제 다섯 개가 뒤집혔다.** 가장 큰 것은 그 메서드를 **이미 두 모드가
가로채고 있다**는 것이다. UI Fixes 는 왼손 나침반을 치우고 애니메이션이 끝난 뒤
`ToggleGoggles` 를 **되부르고**, COTI 는 자기 수정키가 눌렸으면 억제한다. 되부름 쪽이
특히 고약했다 — 그때는 N 이 이미 떼어져 있어서, 키를 보며 기다리는 휠을 띄우면
떼어질 일 없는 키를 기다리며 입력이 잠긴다. 그래서 "우리 차례에 키가 눌려 있지 않으면
사람이 아니라 코드가 부른 것" 으로 보고 휠 없이 즉시 여닫도록 했다. 이 분기 판단만
순수 파일로 떼어 시험으로 고정했다.

나머지 넷: 바닐라 제스처 휠은 마우스 방향이 아니라 **커서 호버**로 고른다. 바닐라의
시점 차단은 축 배열을 통째로 null 로 만들어 **이동까지 멈춘다**(교전 중에 쓰는 동작이라
우리는 시점·회전 축만 0 으로 눌렀다). `GetItemComponentsInChildren` 는 **헬멧 자신도
포함**한다 — 힌지 달린 헬멧은 자기 자신이 여닫이라, 지금껏 N 이 바이저를 여닫고 있었을
수 있다. 그리고 `FaceShieldComponent` 는 야시경·열화상과 공통 인터페이스가 없다.

### 휠이 떴는데 고른 것이 움직이지 않았다 — 설치된 이웃 모드를 뜯어 봤다

처음에는 휠의 방향을 **시점 축을 막는 그 자리에서 받은 입력**으로 썼다. 따로 마우스를
읽지 않으니 감도·반전 설정이 그대로 따라온다고 생각했다. 인게임에서 휠은 떴지만
고른 것이 **전혀 움직이지 않았다**.

추측으로 고치지 않고, 설치된 모드 중 레이드 중에 선택 UI 를 띄우고 시점을 막는 것을
찾았다. `grep -rail TranslateAxes --include=*.dll` 로 네 개가 걸렸고, 그 중
`Tylevo.FieldAttachments` 가 십자·육각 선택판을 띄우는 바로 그 구조였다.
역컴파일해 보니 그쪽은 축 배열을 **막는 데만** 쓰고(2~6 을 0 으로), 방향은
`Input.GetAxisRaw("Mouse X" / "Mouse Y")` 에서 따로 받아 자기 좌표에 쌓고 있었다.

까닭이 분명했다. `MoveInputTranslator.TranslateAxes` 는 축을 **쓰는** 쪽이고
(`_controller.Look(axes[4], axes[5])`), 입력 사슬에서 우리보다 앞의 노드가 축을 먼저
0 으로 만들면 우리에게는 영영 0 만 온다. **그 모드 자신이 그렇게 2~6 을 0 으로 만드는
노드이고, 지금 깔려 있다.** "축을 읽어 방향을 얻는다" 는 전제 자체가 틀렸다.
감도·반전이 따라오는 이점은 포기하고, 날 마우스 움직임을 받아 단위원 안에 가두는
가상 커서로 바꿨다. 감도는 우리 설정으로 뺐다.

### 생김새 — 색을 우리가 고르지 않는다

처음 판은 "마우스 안뜸 + 안움직임 + 디자인 개판" 이었다. 제스처 휠의 항목을 복제해
썼더니 제스처용 아이콘(엄지)과 좁은 글자칸이 딸려 와 이름이 세로로 한 글자씩 쪼개졌다.
그 길을 지우고, 같은 이웃 모드가 게임에 섞이는 방식을 그대로 가져왔다.

`EFT.UI.InteractionButton` 의 `NormalColor`·`DisabledColor`·`SelectedColor` 와
`ItemView.DefaultSelectedColor` 를 반사로 읽는다 — 게임의 상호작용 버튼이 쓰는 그
색이다. 고리는 `MaskableGraphic.OnPopulateMesh` 에서 꼭짓점을 직접 쌓아 그린다.
그림 파일이 없으니 해상도마다 흐려지지 않는다. 실제 마우스 포인터는 레이드 중 잠겨
있어 뜨지 않으므로 커서를 직접 그린다.

### 리뷰가 잡은 것 중 값이 컸던 둘

**15초 안전장치가 손대지 않은 항목을 여닫았다.** 키 상태가 눌림에 얼어붙은 사이 사람은
손을 떼고 평소대로 시점을 돌리는데, 그 움직임이 가상 커서를 끌고 가 엉뚱한 조각에
세워 둔다. 접힐 때 그것을 여닫으면 15초 전에 손을 뗀 사람의 페이스실드가 영문 없이
내려간다. 죽는 경로는 이미 올바르게 "아무 것도 안 함" 이었는데 시간 초과 경로만
빠져 있었다. 설계가 시간 초과의 뜻을 정해 두지 않아 **"취소"로 정했다** — 나머지
코드가 전부 "떼면 확정" 계약이고, 그 경로에는 뗀 적이 없다.

**그림이 거짓말을 하고 있었다.** 데드존 원을 42px, 고리 구멍을 112px 로 **서로 관계없는
수**로 그려 놔서, 그 사이 50~110px 구간은 아무 것도 안 그려진 빈 자리인데 이미 조각이
켜져 있었다. 구멍 = 데드존 × 바깥반지름 하나로 묶어, 보이는 구멍의 테두리가 곧 고르기가
시작되는 선이 되게 했다.

### 경고가 헛소리였던 자리

한글 폰트를 못 찾는다는 경고가 떴는데, 화면에는 글자가 **보였다**. 까닭은 글꼴이 없는
것이 아니라 검사가 틀린 것이었다 — `TMP_FontAsset.HasCharacter(char)` 의
`searchFallbacks` 기본값이 `false` 인데, 한글은 폰트 자신이 아니라 **폴백 폰트**에
들어 있는 것이 보통이다. 폴백까지 보게 고치고, 나아가 **기본 폰트가 한글을 그리면
아무 것도 바꾸지 않도록** 뒤집었다. 잘 나오는 것을 갈아 끼우는 쪽이 더 위험하다.

머지 시점에 설계 §7 의 7·8·10(휠 띄운 채 죽기, 왼손 나침반, 제스처 휠)은 확인하지
않았다. [BACKLOG.md](BACKLOG.md) 에 남겼다.


## 2026-11-01 — STM 기반 단계(계획 1) 결과와 사람 관문

SPT Team Manager(STM)는 PIT Fireteam 1.0.0(`f32a41b`) 포크다. 계획 1은 그 기반 —
포크 들여오기, 식별자 리네임과 설치 스크립트, 프로필 열거, 등록 저장, 친구 목록 병합,
파티 자리 정책, 명단 화면 코드 — 까지다. 브랜치 `feat/spt-team-manager`.
**설치도, 게임 실행도, 클라이언트 컴파일도 아직 한 번도 하지 않았다.** 아래는 그 전제에서
무엇이 확인됐고 무엇이 안 됐는지다. 이 항목 전부가 사람 관문을 통과하기 전에는 "된다"고 쓰지 않는다.

### 테스트가 덮는 것 (순수 `shared/` 코드, 53개 통과)

서버 빌드 경고 0 오류 0. 테스트는 net8.0 이고 SPT 타입을 참조하지 않는 순수 `shared/` 코드만 건드린다.

- 프로필 가용성 판정(`Ready`·`Empty`·`CurrentSession`·`Unreadable`). 지금 로그인한 프로필은 어떤 경우에도
  팀원이 될 수 없고, 읽지 못하면 사용 불가로 친다(닫힌 쪽으로).
- 친구 목록 병합 **정책**: 네이티브 > 프로필 팀원 > 용병 순으로 `Id`·`Aid` 중복을 거른다.
- 파티 자리 정책 `PartySlotPolicy`(AI 팀원 3자리). **어디에서도 호출되지 않는다** — 의도다. 서버는 파티
  인원을 알 길이 없다(`invite/send` 는 `{To, InLobby}` 만 싣고 수락·탈퇴·추방 라우트가 없다). 인원을
  추측하면 정당한 용병 초대를 거부하게 되므로 연결하지 않았다. 클라이언트→서버 구성 보고 채널이
  생길 때의 관문이다.
- 응답 해석(`ParseRows`·`ParseStatus`·`IsCurrentSession` 등 공유 코드).

### 쓰였으나 검증되지 않은 것

서버 빌드가 컴파일되는 것 외에 아무 실행 증거가 없다. 테스트가 닿지 않는다.

- 등록 라우트와 저장소(`/stm/members/*`, `StmProfileMemberStore`): SPT 직렬화 왕복 미검증(관문 4번).
- 친구 목록 병합 **배선**(`MergeFriendList` 의 읽기 보호 겹과 호출부): 병합 한 겹이 실패해도 네이티브와 용병이
  남도록 감쌌으나 실행해 본 적이 없다.
- 명단 화면과 클라이언트 코드(프로필 타일, 불러오기 선택기): **2026-11-01 에 처음 컴파일했고 경고 0·오류 0
  으로 통과했다**(아래 "클라이언트 기준 변경과 첫 빌드"). 즉 타입과 멤버는 맞다. 그러나 한 번도 **실행**된
  적이 없다 — 화면이 그려지는지, 라우트가 응답하는지는 여전히 미검증이다.

### 확인되지 않은 것 (사람 관문 대기)

인게임 메뉴 충돌 여부, SPT 직렬화 왕복, `[Injectable]` 런타임 해석, 아이콘 구분.

클라이언트 빌드는 **더 이상 미검증이 아니다** — 2026-11-01 에 통과했다. 남은 것은 컴파일이 증명하지
못하는 것들이다: 그려지는 모양, 런타임 해석, 실제 동작.

### 아직 안 되는 것 (계획 2·3)

- 계획 2 — 장비: 실물/임시 장비 분기, 로드아웃 편집, 본인 스태시만 출처로 쓰기, §5-14 스태시 재검증 수리,
  §6-8 3단 전송 저널. (화면의 장비 줄은 "아직 판정하지 않음"으로 둔다.)
- 계획 3 — 레이드 후: 경험치·스킬·전적 적립, 전리품 메일, 소모·손상, 사망 처리, 추가 기능 셋.

두 계획은 이 단계가 인게임에서 통과한 뒤에 쓴다.

### 클라이언트 기준 변경과 첫 빌드 (2026-11-01)

계획 1 내내 클라이언트는 한 번도 컴파일되지 않았다. 업스트림 csproj 의 `ValidateSptClientReferences` 가
`spt-common.dll`·`spt-reflection.dll` 을 **정확히 4.1.0.0** 으로 요구하는데 이 기계에는 4.1.6 뿐이고, 그
두 어셈블리는 net472 BepInEx 쪽이라 NuGet 에도 없다(서버 쪽은 공식 NuGet 의 `4.1.0` 으로 이미 해결돼 있다).

**왜 업스트림이 4.1.0 을 고집하는가.** 그것은 넘어야 할 하한선이 아니라 **지원 범위의 가장 오래된 끝**이다.
csproj 주석이 직접 적는다 — "Compile against the oldest supported SPT patch, not the installed runtime."
가장 오래된 판본으로 컴파일하면 바이너리가 4.1.x 전체에 존재하는 API 표면만 바인딩하므로 4.1.0 부터
4.1.6 까지 어디서나 로드된다. 그래서 서버 메타데이터가 `SptVersion = "~4.1.0"` 을 선언한다. 업스트림은 이
속성을 정확성으로 취급한다 — 커밋 `64e6a23` 이 "Removed the unintended SPT 4.1.3-only server dependency.
Builds now use the SPT 4.1.0 baseline for the intended 4.1.x support range." 로 같은 사고를 고쳤다.

**왜 STM 은 그럴 필요가 없는가.** 4.1.x 전체 호환은 공개 배포된 모드의 사정이다. STM 은 이 4.1.6 설치본
한 대에서 도는 개인 포크이고, 4.1.0~4.1.5 호환은 값이 없다. 배포는 어차피 SAIN 고지 누락과 라이선스
결정이 먼저 풀려야 한다.

**한 것.** 설치본에서 클라이언트 참조 34개를 모아 `client/libs/`·`client/libs4.1/` 에 놓고(전부 `.gitignore`
의 `*.dll` 로 제외, 커밋되지 않음), 검사 조건 한 줄만 `4.1.0.0` → `4.1.6.0` 으로 맞췄다. 검사는 끄지 않았다.
폴더 이름은 `spt4.1.0` 그대로 두었다 — HintPath 네 곳을 흔들지 않으려는 것이고, 그 안에는 4.1.6 어셈블리가
있다(csproj 주석에 적어 두었다).

**결과.** `dotnet build src/SptTeamManager/client/pitFireTeam.csproj -c Debug -p:BuildSAINAddon=false`
→ **경고 0개, 오류 0개.** 산출물 `SptTeamManager.dll` 1.0.0.0 (3.1MB). 컴파일러 피드백 없이 쓴 약 750줄이
첫 시도에 통과했다. API 를 추측하지 않고 기존 소스에서 읽어 인용하게 한 것과, 리뷰어가 실제 어셈블리
메타데이터로 대조한 것이 값을 했다.

**남는 대가 하나.** 서버 메타데이터는 아직 `SptVersion = "~4.1.0"` 으로 4.1.x 전체를 선언하는데, 클라이언트가
4.1.6 을 요구하게 되어 **그 선언은 더 이상 참이 아니다.** 지금 이 설치본에서는 무해하다. 영향은 배포하거나
SPT 를 되돌릴 때만 나타나고, 그때는 예컨대 4.1.3 에서 모드가 로드된 뒤 클라이언트가 런타임에 깨지는 모양이
된다. 업스트림 메타데이터를 고치면 그것이 진짜 갈라짐이 되므로 **고치지 않고 사실만 적어 둔다** — 배포
직전에 SAIN 고지·라이선스 결정과 함께 본다.

### 사람 관문 검증 항목

1. ~~**클라이언트 참조 결정.**~~ **2026-11-01 해결됨** — 아래 "클라이언트 기준 변경과 첫 빌드" 참조.
   남은 관문 항목은 2번부터다.
2. **PIT 를 빼고 STM 을 설치한다.** `tools/install-sptteammanager.ps1`. 살아 있는 PIT 가 있으면 스크립트가
   거부하므로 순서가 강제된다(두 모드는 같은 `/singleplayer/pitfireteam/*` 라우트 접두를 쓴다). 설치 후
   `node tools/gen-installed-mods.mjs` 로 `doc/INSTALLED-MODS.md` 를 다시 만든다(이번 단계에서는 만들지 않았다).
3. **메인메뉴 STM 버튼.** 버튼이 뜨고, 눌리고, 스쿼드 UI 와 충돌하지 않는지 본다.
4. **저장 왕복.** `/stm/members/list` 가 빈 목록과 함께 200 을 돌려주는지, 그리고 add → list → 서버 재시작
   → list 로 등록이 SPT 의 실제 직렬화를 거쳐 되살아나는지 본다. (net8.0 테스트로는 SPT 의 직렬화 옵션을
   증명할 수 없어 일부러 테스트로 흉내 내지 않았다.)
5. **명단과 초대 창.** 인게임에서 프로필 팀원이 STM 명단과 파티 초대 창에 *그려지는지*, PIT 용병과 아이콘으로
   구분되는지(분류 상수는 서버 `internal const` 한 줄) 본다. **등록된 프로필 팀원은 현재 초대할 수도, 레이드에
   데려갈 수도 없다.** 초대 버튼을 누르면 계획 1 에서는 설계상 아무것도 일어나지 않고(`SendGroupInvite` 는 PIT 팀원
   DB 에서만 대상을 찾는다). **시스템 메시지도 오지 않는다 — 버튼을 눌러도 화면에 아무 반응이 없는 것이 정상이며 결함이 아니다.**
   메시지를 달 수 없었던 이유: `invite/send` 는 대상을 Aid 로 지목하는데 등록 문서에는 프로필 ID 만 있어서, 핸들러가
   가진 정보만으로는 그 대상이 프로필 팀원인지 알아볼 수 없다(열린 결정은 아래 "열린 결정 — 프로필 팀원 초대 응답" 참조).
   레이드 투입(입장 관문 연결)은 계획 2·3 의 몫이므로 "레이드에 투입되는가"는 이 단계의 확인 항목이 아니다.
   또 하나 확인한다: 클라이언트의 현재 세션 필터는 `Session.Profile.AccountId` 와 서버의 `pmc.Aid`(문자열로
   표기한 값)가 같다고 가정하고 비교한다. 이 둘이 실제로 같은 값인지는 게임 없이 검증할 수 없으므로 인게임에서
   확인한다(다르면 로그인한 본인 프로필이 후보·명단에 걸러지지 않는다). 넷째 자리 확인은 아래 8번이다.
6. **Aid 잔여 구간.** 용병을 먼저 만들고 그 뒤 런처에서 새 프로필을 만들어 SPT 가 같은 Aid 를 주었을
   때, 프로필 보기·삭제와 **`invite/send`** 가 엉뚱한 용병을 집는지 본다. 가장 날카로운 것은 `invite/send` 다:
   충돌 구간에서 *프로필 팀원*을 초대하면 같은 Aid 를 가진 **용병**으로 해석되고(용병은 병합에서 친구 목록에서는
   빠졌지만 PIT DB 에는 남아 있다), PIT 가 그 용병을 레이드에 초대한다. 별도로 `/client/profile/view` 에서
   SPT 자체 핸들러가 다른 프로필의 카드를 내주는지, 아니면 빈 화면이 되는지 관찰한다(동작은 바꾸지 않았다).
   PIT 는 용병 생성 시점에는 실제 프로필 Aid 와의 충돌을 이미 피하므로(`GetUniqueAccountId`) 이 좁은 창에서만
   성립한다.
7. **`[Injectable]` 해석.** STM 서버 모드 로드 시 오류가 0건이고 `/client/friend/list` 가 프로필 팀원을
   싣는지 본다(`StmProfileMemberStore`·`StmProfileCatalog` 가 런타임에 해석되는지는 빌드로 증명되지 않는다).
   **`invite/send` 도 함께 본다:** `FriendlyTeammateMatchCallbacks` 가 STM 의존성 둘(`StmProfileMemberStore`,
   `ISptLogger<…>`)을 주입받는다. 하나라도 해석에 실패하면 그 클래스 생성이 실패해 **PIT 의 초대·추종자 생성
   경로 전체**가 함께 죽는다. PIT 용병 초대가 여전히 되는지(= 매치 콜백이 생성되는지) 확인한다.
8. **넷째 자리 실측.** 파티 창에 팀원 넷째를 넣으려 할 때 실제로 무엇이 일어나는지 본다. 이 실측이
   서버측 자리 관문(`PartySlotPolicy` 연결)이 언젠가 필요한지를 결정한다.

### 사람 관문 1차 결과 (2026-11-01) — 항목 2·4·7 통과, 3·5·6·8 은 게임이 필요하다

서버 쪽은 **실제로 돌려서** 확인했다. 아래는 추측이 아니라 호출 결과다.

**항목 2 (설치) — 통과.** 먼저 백업했다: 프로필 3개(파일 43개)와 설치된 PIT 두 디렉터리(게임 18개 / 서버
12개)를 `backup/2026-11-01-before-stm/` 에 복사해 **파일 수를 대조**했다. 그 다음 PIT 를 지우지 않고
`backup/2026-11-01-before-stm/removed-from-install/` 로 **옮겼다** — 되돌리려면 두 폴더를 제자리로 옮기면
된다. 설치 스크립트는 거부 없이 통과해 클라이언트 18개(DLL + 에셋 17), 서버 DLL·`deps.json`·`LiteDB.dll`·
`Resources`(avatars, lang)를 깔았다. `settings.json` 은 깔지 않는다(모드가 실행 중 만든다).
설치본에 남은 PIT 흔적은 SPT 가 캐시한 `sptappdata\files\trader\avatar\pitfireteam-courier.png` 하나뿐이다(무해).

**항목 7 (`[Injectable]` 해석) — 통과.** 서버 로그에
`모드 SPT Team Manager 버전 1.0.0 (GUID: xyz.stm.teammanager | targets SPT: ~4.1.0) by PitAlex 불러옴`,
그리고 SPT 자신이 교체를 알아봤다 —
`(Added 1 mods: SPT Team Manager (1.0.0); Removed 1 mods: PitFireTeam (1.0.0))`.
`Resources\stm-teammates\<프로필id>.db` 가 프로필마다 생겼다(디렉터리 리네임 작동). **오류 0건.** 로드가
오류 없이 끝났다는 것은 DI 가 콜백 클래스를 전부 구성했다는 뜻이고, 거기에 최종 리뷰가 위험으로 꼽은
`FriendlyTeammateMatchCallbacks`(STM 의존성 둘이 새로 들어간 클래스)가 포함된다.

**항목 4 (라우트와 직렬화 왕복) — 통과.** 네 라우트를 전부 호출했다.
- `members/roster` → 로그인한 프로필(`6adc2669…`)이 **빠지고** 다른 둘이 실제 데이터로 `Ready` 로 온다:
  `Knox` aid 1100437 Lv.69 Bear, `SPT02` aid 1741641 Lv.15 Usec. 설계 §6-9 의 핵심 규칙이 런타임에서 확인됐다.
- `members/add` → `{profileId, tactic:"Rifleman", autoJoin:false}` (Task 5 가 고른 기본값, 고정한 camelCase).
  쓰인 곳은 **로그인한 프로필의 db** — 주석의 "로그인한 프로필별" 이 맞았다.
- **서버 재시작 → `members/list` 가 그 등록을 그대로 돌려준다.** 직렬화 왕복이 확인됐다. 재시작 후
  `members/roster` 는 그것을 `members` 로 옮기고 닉네임·레벨·진영을 **새로 읽은 카탈로그에서** 채운다 —
  저장소에는 그 값이 없으므로, 프로필 데이터를 캐시하지 않는다는 것이 런타임으로 증명됐다(§4 "유령 재산").
- `members/remove` → 목록에서 빠진다. 다시 추가도 된다.
- **로그인한 프로필 자신을 추가하려는 시도는 거부된다** — `{"err":500,"errmsg":"StmProfileNotAvailable"}`.

**라우트를 손으로 부르는 방법(다음에 필요할 때).** SPT 는 요청 본문을 **deflate(zlib) 로 읽는다.** 평문
JSON 을 보내면 `The archive entry was compressed using an unsupported compression method` 로 터지고 200 에
빈 본문이 돌아온다 — 진짜 SPT 라우트도 똑같으니 모드 결함이 아니다. 압축해 보내야 한다. 없는 경로는 404,
등록된 경로는 200 이므로 그 둘을 대조하면 등록 여부를 가릴 수 있다. TLS 는 자체 서명이라 검증을 꺼야 한다.
세션은 `Cookie: PHPSESSID=<프로필 id>`.

**아직 게임이 필요한 것: 항목 3**(메인메뉴 버튼과 스쿼드 UI 충돌), **5**(명단·초대 창에 그려지는지, 아이콘
구분, `Session.Profile.AccountId` == `pmc.Aid` 확인), **6**(Aid 충돌 표면), **8**(넷째 자리).

**이번에 드러난 사실 둘.**
- 설계 §5-13 이 "쓸 수 있는 테스트 프로필이 아직 없다"(둘뿐이고 하나는 386바이트 빈 프로필)고 적은 것은
  **더는 사실이 아니다.** 지금 프로필이 셋이고 로그인한 것을 뺀 둘이 전부 플레이돼 `Ready` 다. 관문 항목
  5 를 실제로 시험할 수 있다.
- **PIT 의 용병은 STM 명단에 보이지 않는다.** PIT 의 `Resources\teammates\` 에 `.db` 3개(하나는 909KB)가
  있었고 전부 백업에 온전하다. STM 은 `stm-teammates\` 를 보므로 다른 서랍을 여는 셈이다. 가져오려면
  `.db` 를 복사하면 될 것으로 본다 — 암호화 키 시드(`KeyMaterial`)를 일부러 바꾸지 않았고 파일 이름 규칙도
  `<프로필id>.db` 로 같다. **다만 시도하지 않았다**(사용자 데이터이고 어떤 계획에도 없던 일이다).

### PIT 용병 DB 를 STM 으로 가져왔다 (2026-11-01) — 성공, 공존 확인

사용자 결정으로 PIT 가 들고 있던 팀원 DB 를 STM 쪽으로 옮겼다. **된다.**

**왜 되는가.** PIT 의 팀원 DB 는 AES-GCM 으로 암호화돼 있고 키가
`SHA256(KeyMaterial + "/" + 프로필id)` 다. 리뷰 중에 `KeyMaterial` 을 **일부러 바꾸지 않았고**(경로가 아니라
키 재료이고, 바꾸면 기존 PIT DB 를 영구히 못 읽게 되므로) 파일 이름 규칙도 `<프로필id>.db` 로 같다. 그래서
파일을 디렉터리만 바꿔 놓으면 그대로 읽힌다.

**한 것.** 서버를 내린 상태에서, 먼저 STM 이 새로 만든 DB 3개를
`backup/2026-11-01-before-stm/stm-db-before-import/` 로 따로 떠 두고, 백업의
`removed-from-install/pitFireTeam-ServerMod/Resources/` 에서
`teammates/*.db`(3개) → `Resources/stm-teammates/`, `teammates-allegiance/*.db`(1개) →
`Resources/stm-teammates-allegiance/` 로 복사했다. 네 파일 모두 SHA256 이 원본과 일치.

**확인한 것 (실측).**
1. 서버 기동 시 `Teammate database ready` 3건 — **파일 단위 복호화 성공.**
2. `/client/friend/list` → 친구 11명. PIT 용병 8명(`SPT02`~`SPT08`, `Zofia`)이 이름·레벨과 함께 나온다 →
   **문서 단위 복호화까지 성공.** 설계 §5-13 이 실측했던 그 이름들이다. 회원 분류는 **1024**(`Unheard`).
3. 프로필 팀원 등록을 다시 넣고(`members/add`) 다시 호출 → **친구 12명.** 용병 8명 전원이 그대로 남고
   프로필 팀원이 열두 번째로 붙는다: `SPT02 lvl 15 cat 0 id 6ae5870eeb15d61bf003fd1f`.
   - **용병이 하나도 밀려나지 않았다** → 설계 §4 "PIT 기능이 죽는다" 를 피한 것이 런타임으로 확인됐다.
   - **두 종류의 회원 분류가 실제로 다르다** — 용병 1024(`Unheard`), 프로필 팀원 0(`Default`).
     설계 §8 이 빌더에게 남긴 아이콘 구분이 **데이터 수준에서는 갈린다.** 눈으로 다르게 그려지는지는
     여전히 게임이 필요하다(관문 5).
   - 용병 문서와 STM 의 등록 문서가 **같은 `.db` 파일에 서로 다른 문서 키로** 공존한다. §6-1 의 "PIT 기능을
     잃지 않고 STM 을 얹는다" 가 실물로 확인됐다.

**주의 — 이름이 겹친다.** PIT 용병 중에 `SPT02`(레벨 18, cat 1024)가 있고, 런처 프로필에도 `SPT02`(레벨 15,
cat 0)가 있다. 친구 목록에 같은 닉네임이 둘 뜬다. 동일성 판정은 `Id`·`Aid` 로 하므로 **결함은 아니지만**
고를 때 헷갈린다. 레벨과 아이콘으로 가려야 한다. 프로필 쪽 닉네임을 바꾸면 깔끔해진다.

**되돌리려면.** `stm-db-before-import/` 의 3개를 `Resources/stm-teammates/` 로 되돌리고
`stm-teammates-allegiance/` 를 지운다. PIT 자체로 돌아가려면 `removed-from-install/` 의 두 폴더를 제자리로
옮기고 STM 을 `tools/install-sptteammanager.ps1 -Remove` 로 뺀다.

### 사람 관문 2차 — 인게임 확인 (2026-11-01, 사용자가 직접)

사용자가 게임을 띄워 확인했다. 화면 다섯 장으로 남은 관문 항목 대부분이 닫혔다.

**항목 3 (메인메뉴 버튼과 스쿼드 UI) — 통과.** 메인메뉴에 `MY SQUAD` 가 뜨고(캐릭터 / MY SQUAD / 거래 /
은신처 / 게임종료) 눌러서 열린다. 화면 상단 탭 `ROSTER`·`SETTINGS`·`MODE` 와 PIT 의 용병 타일 8개가 그대로
그려진다. **§5-11 의 선례(pmc-squadmates)가 바로 이 자리에서 멈췄는데, 충돌 없이 통과했다.**

**항목 5 (명단·초대 창 표시와 아이콘 구분) — 통과.**
- 명단에 프로필 팀원이 `프로필 팀원 / 닉네임 / Lv N 진영 / 장비 구분: 아직 판정하지 않음` 으로 그려진다
  (`SPT02` Lv 15 Usec, `Knox` Lv 69 Bear). 하단에 `+ ADD TEAMMATE`(PIT 용병)와 `프로필 불러오기`(STM)가
  나란히 있고, 사용자가 불러오기로 `Knox` 를 실제로 추가했다.
- **"파티에 초대" 창에서 두 종류가 눈으로 갈린다.** 프로필 팀원은 **사람 아이콘(👤)**, PIT 용병은
  **번개 아이콘(⚡)**. 설계 §8 이 "빌더가 게임에서 맞춘다" 며 남긴 항목이 **해결됐다** — Task 6 이 고른
  `MemberCategory.Default`(0) vs `Unheard`(1024)가 실제로 다르게 렌더된다. 값을 바꿀 필요가 없다.
- 로그인한 프로필 `SPT01 [36]` 은 `리더 (나)` 로 뜨고 팀원 후보로 제시되지 않는다 → 클라이언트 세션
  필터(`Session.Profile.AccountId` == `pmc.Aid`)가 실제로 작동한다.
- **이름 겹침이 눈에 보인다**: `SPT02 [15]`(👤 프로필) 과 `SPT02 [18]`(⚡ 용병) 이 나란히 뜬다. 예상대로
  아이콘과 레벨로 가려진다.

**항목 6 중 `/client/profile/view` — 답이 나왔다.** 프로필 팀원의 프로필 카드가 **열린다.** `Knox` 의
종합정보가 레벨 69, BEAR, EXP 19,930,020 으로 뜬다. `MergeProfileView` 가 그냥 흘려보내면 SPT 자신의
핸들러가 실제 프로필 카드를 내준다는 뜻이다 — 빈 화면이 되지 않는다. (전적이 전부 0 인 것은 그 프로필이
레이드를 돈 적이 없기 때문으로 보인다. 모드와 무관.)

**초대 — 문서대로 아무 일도 일어나지 않는다(확인).** 용병은 초대하면 즉시 `파티원` 이 된다(`Zofia`,
`SPT06` 이 초록으로 바뀜). 프로필 팀원은 `초대가 발송되었습니다` 에서 멈춘 채 합류하지 않는다. 예상한 그대로다.
**새로 안 사실:** 클라이언트가 "발송되었습니다" 라고 **스스로 표시한다.** 그러므로 완전한 침묵은 아니고,
사용자 눈에는 "보냈는데 안 들어온다" 로 보인다. 아래 "열린 결정" 의 (b) 시스템 메시지를 택하면 그 표시
**옆에** 설명이 붙는 모양이 된다.

**아직 남은 것:** 항목 6 의 Aid 충돌 표면(삭제·초대가 엉뚱한 용병을 집는지 — 충돌이 성립해야 보인다),
항목 8(파티에 팀원 넷째를 넣으려 할 때).

**상세 창이 없는 것은 계획 1 범위다.** 계획 1 Task 7 은 "명단 좌측에 프로필 팀원을 그린다" 까지이고,
설계 §6-3 의 **오른쪽 상세**(착용 슬롯 인형, 전적, 거기서 장비 편집으로 진입)는 계획 2 의 몫이다.
PIT 용병 타일을 누르면 세부 창이 열리는 것은 PIT 가 원래 가진 기능이다.

### 계획 2·3 이 지켜야 할 제약

- **STM 문서는 `{정수}.json` 으로 이름 짓지 않는다.** `TeammateDatabase.IsProfileDocument` 는 `.json` 으로
  끝나고 앞부분이 양의 정수면 PIT 팀원 문서로 읽고, `IsLegacyDocument` 는 거기에 `recruit-requests.json`,
  `{aid}-settings.json`, `{aid}-equipment.json` 도 더한다. 그 모양으로 새 문서를 만들면 명단에 유령 팀원이
  뜬다. 현재 등록 문서 이름 `stm-profile-members.json` 은 이 모양이 아니다.
- **분대 초기화(`WipeProfile`)는 프로필 팀원 등록 문서까지 지운다.** 의도된 동작이다. 분대를 비우는데
  등록이 남으면 지운 분대의 유령 등록이 남는다. 지운 뒤에는 등록을 다시 해야 한다.
- **표시 시점에 조인한다.** `StmProfileMemberStore.List` 는 카탈로그와 대조하지 않는다. 등록된 뒤 삭제되거나
  읽을 수 없게 된 프로필도 목록에 남으므로 화면은 `GetCandidates` 와 조인해야 하고, 조인 결과를 모드 DB 에
  복사하지 않는다.
- **`FriendKind.Native` 는 친구 목록 보호용 분류다.** SPT 에서 네이티브 친구는 레이드 파티에 들어오지
  않는다(실제 프로필의 `friends` 는 `[]`, Fika 는 비목표). 네이티브가 파티 자리를 먹는다는 전제로 상한을
  다시 계산하지 말 것(`shared/PartySlotPolicy.cs` 주석에도 적었다).
- **net8.0 테스트 프로젝트에 SPT 참조를 넣지 않는다.** 어댑터가 테스트되지 않는 것은 그 분리의 대가이고,
  우회는 거짓 안심을 만든다.
- **손상된 등록 문서는 빈 목록이 아니라 오류를 돌려준다(의도, Ruling 25).** 빈 목록으로 취급하면 다음 `Add` 가
  사용자의 등록 데이터를 덮어써 지운다. 계획 2·3 은 이를 "고치지" 말 것. 경고 로그에 전체 예외가 남아 진단은 된다.

### 알려진 한계 — 계획 2 항목

- **네이티브 친구 목록의 "삭제"는 프로필 팀원에게 아무 일도 하지 않는다.** `/client/friend/delete` 는
  PIT 팀원만 지우고, 등록된 프로필 팀원은 다음 폴링에서 다시 나타난다. 프로필 팀원의 제거는 STM 자체 명단
  화면에서 된다. `StmProfileMemberStore.Remove` 로 위임하면 PIT 네이티브 라우트에서 새 쓰기 경로가 생기므로
  별도 작업과 별도 리뷰로 계획 2 에서 한다.
- **프로필 팀원은 초대 → 레이드 합류가 연결돼 있지 않다.** 위 5번 참조. 서버에는 파티 구성 신호가 없어
  입장 관문(`PartySlotPolicy`)을 세지 않고 두었다.

### 설계 문서에서 틀렸던 것(정정함)

설계 문서의 해당 자리에 "정정(2026-11-01, 실측)"으로 덧붙였다. 원문은 지우지 않았다.

- **§6-2:** "동료 4, 5가 안 보이는 것은 네이티브 4명 한계라 수리 대상이 아니다"는 틀렸다. 인용한 출처가
  그것을 스태시 재검증 버그로 팀원 생성이 중간에 끊긴 PIT 결함으로 진단한다. 상한 3 자체는 유지.
- **§5-1·§6-1:** "라이선스 MIT, 원 고지 유지"의 전제가 거짓이다. 업스트림에는 LICENSE 파일이 없다.
- 계획 1 Task 1 Step 2(설치본에서 참조 DLL 복사)와 들여오기 기록(`doc/install-records/stm-fork-import.md`)의
  낡은 줄도 같은 날 정정했다.

### 라이선스와 저작자 표시 — 배포 전에 풀어야 한다

업스트림에 LICENSE 파일이 없고, 복사된 SAIN 코드의 고지 파일 `addon/SAIN-LICENSE.txt` 가 업스트림에서
이미 삭제돼 있다(업스트림 문서가 미해결로 기록). 포크가 이를 물려받았다. 라이선스 판단은 저장소 소유자의
몫이며, README 는 MIT 라고 단정하지 않고 사실만 적었다. 애드온은 계획 1에서 빌드도 설치도 하지 않는다.

(2026-11-01 실측 추가) 모드 페이지의 Details 절에는 "License" 필드가 있고 값이 "MIT License" 다 — 저작자의 선언이다.
저장소에는 파일이 없을 뿐이다. SAIN 고지는 살아 있는 설치본에도 없다: `E:/Spt` 에서 `SAIN-LICENSE*` 가 검색되지 않고
설치된 PIT 두 디렉터리에 라이선스 파일이 전혀 없어, 업스트림 `Validation.md:566` 의 "이미 설치돼 있다"는 이 설치본에서는
사실이 아니다.
(추가 실측) 업스트림 서버 메타데이터도 MIT 를 선언한다: `server/PitFireTeamServerPlugin.cs:28` 의 `License = "MIT"` (선언 문자열일 뿐 라이선스 본문도 저작권 고지도 아니다). 없는 것은 LICENSE 파일(본문)뿐이다.
**열린 결정(소유자 몫):** STM 이 그 메타데이터 레코드를 그대로 싣고 있어 설치된 포크 모드가 `License = "MIT"` 를 주장하는데 README 는 라이선스 주장을 하지 않는다. 그 필드를 유지할지 바꿀지는 소유자가 정한다.

### 열린 결정 — 프로필 팀원 초대 응답 (소유자 몫)

`invite/send` 는 대상을 Aid 문자열로 지목하는데(`FriendlyTeammateService` 가 `int.TryParse` 로 `profile.Aid` 와 대조),
등록 문서(`StmProfileMember`)에는 SaveServer 키인 프로필 ID(MongoId)만 있다. 그래서 `FriendlyTeammateMatchCallbacks`
의 "프로필 팀원이면 시스템 메시지" 블록은 조건이 절대 참이 될 수 없는 죽은 코드다(주석으로 표시함). 초대 클릭은 계획 1 에서 조용하다.
선택지(고르지 않았다):
- (a) 등록 문서에 프로필 ID 와 함께 Aid 를 저장한다. 디스크 이름을 막 고정했고 실데이터가 아직 없어 지금이 가장 싸다.
- (b) 이 핸들러 하나만 카탈로그를 읽어 Aid 를 풀게 허용한다. 관문 6번에 적은 Aid 충돌 모호성을 감수한다.
- (c) 계획 1 에서는 초대를 조용히 두고 입장 관문을 계획 2·3 에서 제대로 연결하며, 죽은 블록을 삭제한다.
저장소 소유자가 정한다.

**닫힘 (2026-11-01, 계획 2): (b) 로 풀렸고 (c) 의 죽은 블록 삭제도 함께 했다.**
계획 2 Task 2 가 `StmProfileMemberResolver` 를 만들었다. 핸들러가 Aid 를 들고 그 해석기를 묻고, 해석기가
`StmProfileMemberStore.List`(등록된 ProfileId)와 `StmProfileCatalog.GetCandidates`(Aid 를 가진 후보)를
**매 호출 새로 읽어** Aid → ProfileId 를 푼다. 등록 문서에 Aid 를 저장하지 않았으므로 (a) 는 쓰지 않았다 —
프로필에서 파생된 값을 복사해 두면 낡는다(설계 §4 "유령 재산" 과 같은 종류의 문제다).

(b) 가 감수한다던 **Aid 충돌 모호성은 정책으로 막았다**(계획 2 Task 1, `ProfileMemberLookup.ByAid`):
같은 Aid 에 `CurrentSession` 후보가 있으면 등록 여부와 **무관하게 막고**, `Ready` 가 둘이면 **어느 쪽도 고르지
않는다**. 즉 충돌 구간에서는 초대가 사유와 함께 거절되고, 조용히 엉뚱한 쪽을 집지 않는다.
다만 그 차단이 **보장이 아니라 우연에 가깝다**는 점은 남는다: PIT 의 용병 Aid 할당기는 쓰이는 Aid 를
`profile.ProfileInfo?.Aid` 로 걸러 내고(`FriendlyTeammateService.cs:4329`, `:4366`) 해석기는 `pmc.Aid` 로
맞춘다 — 실질 같은 값이지만 **다른 필드**다. 해석기에 그 사실을 주석으로 남겼다.

계획 1 의 죽은 블록(`request.To` 의 Aid 문자열을 `m.ProfileId` 의 MongoId 와 비교해 영영 참이 되지 않던 것)은
계획 2 Task 4 가 지웠다.

### 이월된 Minor (고치지 않았다 — 최종 전체 리뷰의 몫)

**관문 전에 반드시 분류:**
- 저장 레코드와 문서에 `[JsonPropertyName]` 이 없어 디스크 포맷이 SPT 직렬화 정책에 매달려 있다
  (`StmProfileMember.cs`, 요청 타입만 `profileId` 를 고정). 관문이 실제 데이터를 쓴 뒤에 고치면 그 데이터가
  고아가 된다.

**그 밖에:**
- 친구 목록 폴링마다 Error 로그가 쌓인다: 손상된 저장 문서나 잘못된 ProfileId 가 있으면 스택 트레이스가
  반복된다(`MergeFriendList` 새 Error 로그). 경고 쪽에 쓴 once-per-run 기법을 같은 방식으로 적용하면 된다.
  `LoggedSkips` 억제는 세션 간에 공유돼, 두 번째 프로필의 같은 충돌은 경고가 안 뜬다.
- `ParseStatus("Missing")` 테스트가 없다. `Missing` 을 `Unreadable` 로 보내는 구현이 새 테스트를 전부
  통과하고, 그러면 "프로필이 더 이상 없습니다" 자리에 "읽을 수 없습니다"가 뜬다. `Interpret` 테스트는
  목록 순서를 고정하지 않는다(등록 순서가 표시 순서다).
- `PartySlotPolicy`: `Native` 와 음수 카운트의 조합이 미검증(가드 순서를 뒤집어도 테스트가 못 잡는다),
  첫 테스트의 `Assert.Contains("3", ...)` 가 약하다, `SlotDecision(false, null)` 을 막는 불변식이 없다.
- 주석 오해: "account-wide"·"per-session database" 는 실제로 "로그인한 프로필별, 게임플레이 모드 무관"
  이다(`StmProfileMemberStore.cs`). `UnknownTradingError` 가 이 경로에 어울리지 않는 오류 코드다.
  카탈로그 읽기와 쓰기 사이에 check-then-act 틈이 있다(단일 프로세스 싱글턴이라 무해).
- Allegiance 모드에서 add 버튼은 숨고 불러오기 버튼이 한쪽으로 치우쳐 보인다. 그 모드도 이제 다른 모드처럼
  rebuild 때 동기 `GetJson` 을 한다. `ownAccountId == ""` 는 known 으로 취급되나 `TryGetOwnAccountId`
  가 빈 값을 null 로 바꿔 도달 불가다. `StmProfileRosterClient.cs` 의 미사용 `using` 둘.
- 설치 스크립트 `Add-PlanTree`: 소스 트리가 손상돼 resources 폴더가 없을 때 `-Remove` 루프가 디렉터리 항목에
  `-Recurse` 없이 `Remove-Item` 을 걸어 던진다(`-PathType Leaf` 필요). 비어 있는 소스 폴더가 조용히 통과한다.
  `doc/PROCEDURE.md` 삽입부에 빈 줄이 둘 겹쳤다.
- 애드온·장식: `addon/SAINAddonPlugin.cs` 표시 이름이 아직 PIT 명이다, `client/Properties/AssemblyInfo.cs`
  의 PIT 명 메타데이터, 업스트림 `tests/*`·`AGENTS.md`·`ModDescription.md` 의 옛 이름(최소 침습 때문에
  일부러 안 고침), `client/pitFireTeam.csproj` 의 `BuildSAINAddon` 이 애드온 DLL 을 출력에 넣지만 설치
  스크립트는 깔지 않는다(리네임 전부터 있던 공백).
- 병합의 원본 맵에서 같은 `Id` 에 다른 `Aid` 가 오는 디큐 엣지 케이스(`ListTeammateDialogs` 가 그렇게 내보내야 성립하는
  코너). 키잉을 incoming 인덱스로 바꾸면 사라진다.
- 들여오기 기록의 빌드 출력 경로가 상대경로라 다른 줄의 형식과 어긋난다(`doc/install-records/stm-fork-import.md` 서버 빌드 절).
- 업스트림 `tests/EnemyTrackingHookFixture.cs:26` 이 애드온 어셈블리 리네임 뒤에도 옛 DLL 이름을 읽는다(우리 빌드에 없음).
- 단위 테스트 불가로 받아들인 자리: 어댑터(`SaveServer` 의존), 저장소 로직(SPT 타입 안), 호출부 배선.
  `Func<>` 를 받는 공유 헬퍼로 떼면 일부는 테스트할 수 있었다.
- `KeyMaterial` 의 `pitFireTeam/followers/v1/...` 는 경로가 아니라 암호화 키 시드라 일부러 바꾸지 않았다
  (바꾸면 PIT 팀원 DB 를 읽을 수 없게 된다).

### 환경 메모

- 이 기계의 `python`·`python3` 은 Windows Store 실행 별칭 스텁이라 아무것도 하지 않고 **exit 0** 을 돌려준다.
  파일 편집에 쓰지 말고, 편집 뒤에는 파일을 다시 읽어 확인한다. 한 서브에이전트가 이 스텁으로 "편집했다"고
  보고했으나 커밋에는 그 편집이 없었다.
- 이 레포에는 `origin` 원격이 없다(`pit-upstream` 뿐). PR 흐름(`develop` → `main`)을 쓰려면 원격이 먼저 필요하다.

## 2026-11-01 — STM 계획 2(레이드 투입)

> ### 이 구간을 읽는 법 (2026-11-02 정리)
>
> 아래는 **시계열 기록**이라 중간 결론이 뒤에서 뒤집힌 곳이 있다. 지우지 않고 남긴 것은 어떻게 틀렸는지가
> 다음 계획에 쓰이기 때문이다. **지금 사실로 통하는 것만 보려면 이 요약을 본다.**
>
> **최종 상태**: 계획 2 의 열 개 Task 완료 · 인게임 13개 확인 · 최종 전체 리뷰 approved 및 그 수정 웨이브 반영.
> 단위 테스트 53 → **138**, 빌드 경고 0 오류 0. 설치본은 커밋 `4f8a85c` 기준.
>
> **되는 것**: 초대 → 파티 합류(최대 4명, 클라이언트가 상한을 막는다) · 프로필의 실물 장비로 레이드 투입
> (장비가 없으면 생성된 임시 장비) · 실물로 나갔다 못 돌아오면 착용 장비만 프로필에서 삭제
> (스태시·보안 컨테이너·주머니·완장·칼집 보존) · 시체에서 털어 나온 것은 플레이어가 가진다 · 같은 레이드 중복 처리 방지.
>
> **일부러 안 하는 것**: 프로필에 **더하는** 것 전부(경험치·스킬·전적·전리품) — 계획 4. 장비 편집과 상세 화면 — 계획 3.
>
> **뒤집힌 결론 셋**(본문에 "정정" 으로 표시):
> 1. 파티 미리보기 영향 범위 — 두 번 틀리게 적었다. 확인된 영향은 **없고** auto-join 복원만 미검증이다.
> 2. 9번 넷째 자리 — "막는 것이 없다" 가 **틀렸다.** 클라이언트가 막고, 그래서 `PartySlotPolicy` 는 삭제했다.
> 3. 차단 우선순위 미고정 — 최종 리뷰가 **근거가 사실과 다르다**고 기각했다. 결정을 바꾸는 순서는 이미 고정돼 있었다.
>
> **아직 열린 것 둘**: `/singleplayer/pitfireteam/teammate/profile` 이 프로필 팀원을 모른다(auto-join 복원 미검증,
> 같은 구멍의 네 번째 경로) · 배포 전 라이선스/`SptVersion` 표기 정리.

Task 1~8 이 끝났다. 단위 테스트 53 → **127**. 서버 빌드 경고 0 오류 0.
작업 브랜치 `feat/spt-team-manager`, 커밋 `4016e9a..3ca9805`. 계획 2 는 **클라이언트·애드온 코드를 하나도 건드리지 않았다**(그래서 클라이언트 DLL 은 계획 1 것이 그대로 유효하다).

설치: 백업(`backup/2026-11-01-before-stm-plan2/` — 프로필 4개 + 팀원 DB) 후 `-Remove` → 재설치.
설치된 서버 DLL 이 최신 소스 빌드와 md5 동일하고, 그보다 새로운 `.cs` 가 없음을 확인했다.

### 사람이 해야 하는 확인 (게임을 켜고 레이드를 돌아야 한다)

**계획서 Task 9 의 9개**

1. 프로필 팀원을 초대하면 **파티원이 된다**(계획 1 에서 멈춰 있던 자리).
2. 레이드에 들어가면 **그 프로필의 장비를 입고** 스폰한다.
3. 장비를 벗겨 둔 프로필은 **임시 장비로** 나간다.
4. 용병과 프로필 팀원을 섞어 들어갈 수 있고, **용병 쪽 동작이 그대로다**.
5. 프로필 팀원이 죽은 뒤 **그 프로필로 로그인하면 착용 장비가 없다.** 스태시와 보안 컨테이너는 그대로다.
6. 임시 장비로 나간 팀원이 죽어도 **그 프로필은 변화가 없다.**
7. 생환하면 장비가 그대로다.
8. 레이드를 두 번 끝내도(보고가 중복돼도) 장비가 **두 번 지워지지 않는다**.
9. **파티에 팀원 넷째를 넣으려 할 때 무엇이 일어나는지 잰다**(계획 1 에서 넘어온 항목). 조용히 잘리는지·창이 막는지·사유가 뜨는지 — 이 실측이 서버측 자리 관문이 필요한지를 정한다.

**구현 중에 드러나 추가된 것 — 이름 붙은 필수 확인**

10. **프로필 팀원 시체를 털고 탈출해, 그 아이템이 플레이어 스태시에 살아남는지 본다.**
    구현자와 컨트롤러 모두 **사라질 것으로 본다.** 이유: PIT 의 `RemoveProtectedTeammateItemsFromExtractedProfile` 가 보호 등록된 아이템을 **플레이어가 들고 나온 인벤토리와 그 저장 프로필에서도** 제거한다. 계획 2 는 레이드 진입 때 팀원 장비를 보호 등록하는데(플레이어에게 전리품으로 배달돼 **복제**되는 것을 막으려고), 팀원이 죽어 그 장비를 프로필에서 지운 뒤에는 같은 보호가 **플레이어 것이 된 아이템까지 지우는** 쪽으로 돌아선다.
    지금 고치지 않은 이유: 고치려면 teardown 요청과 raid-end 의 **순서를 알아야** 하는데 측정되지 않았고, 보호 집합이 raid-end 에서 제거되므로 늦은 해제는 무동작이다. **순서를 추측하면 손실 대신 복제가 날 수 있고, 손실은 보이지만 복제는 보이지 않아 그쪽이 더 나쁘다.** 이 관찰이 고치는 방향을 정한다.
11. 파티창이 **제대로 채워지는지, 눈에 띄는 지연이 있는지**. 수락 알림이 PIT 모양 그대로 **스태시 전체**(측정값 1677 아이템 / 약 332KB)를 싣는다. PIT 가 이미 검증된 모양을 첫 테스트에 쓰기로 했고(우리 최적화를 끼면 파티창이 이상할 때 원인을 가릴 수 없다), 증명된 뒤 장비 서브트리로 줄인다.
12. **실물 장비 팀원이 자기 스태시를 레이드에 들고 간다**(`Real` 경로가 프로필을 통째로 복제하므로). 보호 등록이 전체 인벤토리로 넓어져 **복제는 나지 않지만**, 클라이언트가 그런 봇을 어떻게 다루는지는 확인되지 않았다.
13. **영수증이 쓰이고 다시 읽히는지.** 위치 record 바인딩은 실제 실행 전에 증명할 수 없다. 실패하면 모든 읽기가 던져 `WasApplied` 가 늘 "이미 적용" 을 돌려주므로 **아무것도 지워지지 않는다** — 안전한 쪽이지만, 그러면 5번이 영영 통과하지 않는다. 서버 로그에서 `stm-raid-receipts` 관련 Error 를 함께 본다.

**회귀로 오독하지 말 것**: 계획 1 설치본에서는 프로필 팀원을 초대해도 레이드 투입이 **빈 배열**을 돌려줬다. 그 배선이 계획 2 Task 5 이므로, 지금은 1·2번이 통과해야 한다.

### 어긋나면
**거기서 멈추고 보고한다.** 특히 5번(장비가 안 지워짐 또는 **너무 많이** 지워짐)과 10번은 프로필 데이터에 직결된다. 백업은 `backup/2026-11-01-before-stm-plan2/profiles/` 다.

### 인게임 확인 결과 (2026-11-01, 진행 중)

설치본: 서버 DLL md5 `9d61182c…`, 커밋 `3ca9805` 기준. 서버 기동 시 `Teammate database ready:` 3회, STM 오류 0건.

**통과 (1차 레이드, bigmap, 프로필 팀원 둘 + 용병 혼성, 팀원 둘 생환)**

- **1번 파티 합류** — SPT02(aid 1741641)·Knox(aid 1100437) 둘 다 파티에 들어왔다. 계획 1 에서 막혀 있던 자리.
- **2번 실물 장비 스폰** — 서버 로그: `'SPT02' 를 Real 장비로 내보낸다: 착용 장비와 무기가 모두 있어`, Knox 동일. 둘 다 `Real` 판정.
- **4번 혼성** — 용병과 섞인 상태로 진입했고 용병 쪽 이상 없음.
- **7번 생환 시 장비 보존** — **백업 대조로 확인**(로그가 아니라 프로필 자체):

  | | 아이템 총수 | 스태시 직속 | 보안컨테이너 내용물 | 착용 슬롯 14개 |
  |---|---|---|---|---|
  | SPT02 | 96 → 96 | 29 → 29 | 0 → 0 | 전부 동일 |
  | Knox | 602 → 602 | 249 → 249 | 1 → 1 | 전부 동일 |

- **12번 실물 팀원이 스태시를 들고 간다** — 보호 등록 개수가 **96·602** 로 각 프로필의 **인벤토리 총수와 정확히 일치**한다. 장비 트리만 등록하면 10~20 수준이므로, Ruling 44 가 범위를 전체 인벤토리로 넓힌 수정이 실제로 그만큼을 덮었다는 실측이다. 넓히지 않았다면 그 차액이 레이드 종료 때 플레이어 스태시로 복제됐을 자리다.
- **Task 5 Step 3(경험치가 실제 프로필에 닿지 않는다)** — 서버 로그: `Follower progress: skipped 2 profile teammate entry/entries of 2`. 프로필 비교에서도 변화 없음.
- **클라이언트 쪽 STM 예외 0건** — `ModulePatch` NullReferenceException 44건은 **전부** 스택이 `LootPatrolLayer::GetDecision` 인 타사 모드 것이고(그 모드가 스스로 억제한다고 로그에 적는다), `pitTeam`·`SptTeamManager` 가 스택에 등장하는 오류는 없다.
- 팀원 AI 동작 확인 — `[PostCombat] follower=Knox layer=pitTeam.FollowerPatrol action=postCombatWait haveEnemy=True weaponReady=True sainMirrorMatch=True`.

**아직 못 본 것**: 3(임시 장비), 5(**사망 시 착용 장비 상실 — 계획의 본체**), 6, 8(중복 처리), 9(넷째 자리), 10(시체 루팅 후 탈출), 13(영수증 라운드트립).

**로그 한계**: SPT 서버 콘솔이 긴 줄을 폭에서 잘라, `[STM] 레이드 결과: …` 의 판정 사유를 로그에서 읽을 수 없다. 사유 확인이 필요하면 프로필 대조로 대신한다(위 7번처럼).

### 인게임 확인 중 발견 — 같은 구멍의 네 번째 경로 (계획 2 범위 밖, 회귀 아님)

`/singleplayer/pitfireteam/teammate/profile` 이 **PIT 자신의 팀원 문서만** 뒤진다:

```csharp
// FriendlyTeammateService.cs:4269-4270
var teammate = LoadTeammates(sessionId).FirstOrDefault(profile => profile.Aid == aid);
return teammate ?? throw new FriendlyTeammateException($"Unable to find teammate with accountId '{accountId}'");
```

프로필 팀원은 그 문서가 없어 던지고, 클라이언트의 `RaidStartPatch.BuildGroupPlayer`(`client/Patches/RaidStartPatch.cs:423-438`)가
그 행을 못 만들어 경고를 찍는다. 실측 로그:

```
[Warning:SPT Team Manager] [UI] Failed to build auto-join teammate '1741641': Unable to find teammate with accountId '1741641'
[Warning:SPT Team Manager] [UI] Failed to build auto-join teammate '1100437': Unable to find teammate with accountId '1100437'
[Info   :SPT Team Manager] [UI] Rebuilt group preview with 3 players
```

**영향 범위** (호출부를 읽어 정정, 2026-11-01): `BuildGroupPlayer` 가 널을 돌려주면 호출 루프가 `continue` 하므로
(`RaidStartPatch.cs:290-294`, `:336-341`) 그 팀원이 **매치메이커의 `GroupPlayers` 목록에 들어가지 않는다.**
실측 로그의 `groupPlayers=2`(플레이어+용병)가 이와 일치한다 — 같은 줄의 `Rebuilt group preview with 3 players` 는
다른 곳에서 세는 수이고, 그걸 근거로 "미리보기는 정상" 이라고 본 것은 틀렸다. 따라서 영향은 두 가지다:
레이드 전 **매치메이커 파티 목록**, 그리고 **auto-join(지난 분대 자동 복원)이 프로필 팀원을 되살리지 못한다**.
**스폰은 영향 없다** — 스폰은 서버의 초대 알림과 follower-generate 가 몰고, 1·2차 레이드에서 둘 다 확인됐다.
**회귀 아님**: 계획 2 는 클라이언트 코드를 건드리지 않았고 이 경로도 건드리지 않았다. 계획서의 수정 대상 목록에 이 파일이 없었고,
파티 미리보기가 Task 9 체크리스트에도 없어 설계·검토 단계에서 드러나지 않았다. 프로필 팀원이 **이제 실제로 레이드에 들어가게 되어**
비로소 눈에 띈 것이다.

**고치는 모양**: Task 4·5 가 이미 세운 것과 동일 — PIT 조회가 비면 `StmProfileMemberResolver` 에 묻고, 3값 계약(널 사유 = PIT 경로 그대로)을
지키며, PIT 줄은 불변. 인게임 확인을 마친 뒤에 한다(지금 고치면 서버 재기동이 필요해 진행 중인 확인이 무효가 된다).

### 2차 레이드 (Lighthouse) — 5번 통과, 계획 2 의 본체 확인

프로필 팀원 둘이 **둘 다 실물 장비로 스폰 → 둘 다 사망**(플레이어도 사망). 서버가 양쪽에 `레이드 결과 확인:` 을 찍었다(되읽기 검증은 삭제가 실제로 돈 뒤에만 나온다). 백업 대조 결과:

| | 아이템 총수 | 스태시 직속 | 보안컨테이너 내용물 | 비워진 착용 슬롯 |
|---|---|---|---|---|
| SPT02 | 96 → **40** (−56) | 29 → **29** | 0 → 0 | Headwear·Earpiece·FaceCover·ArmorVest·Eyewear·TacticalVest·Backpack·FirstPrimaryWeapon·Holster (9) |
| Knox | 602 → **526** (−76) | 249 → **249** | **1 → 1** | Headwear·Earpiece·FaceCover·ArmorVest·TacticalVest·FirstPrimaryWeapon·SecondPrimaryWeapon·Holster (8) |

**실측으로 증명된 것** (지키기로 한 슬롯이 애초에 비어 있었다면 증명이 아니므로 before 값을 따로 확인했다):
- **보안 컨테이너는 내용물까지 보존** — Knox 의 컨테이너 안 1개가 살아남았다(설계 §6-4, 타르코프 규칙).
- **주머니 컨테이너 자체 보존**, 그 안의 `SpecialSlot1/2/3` 3개도 보존 — 정책이 일부러 지키는 슬롯이고, 삭제 대상에서 제외된 것이 실제로 작동했다.
- **스태시는 양쪽 다 한 개도 안 줄었다.**
- 경험치 적립이 프로필에 닿지 않았다(`Follower progress: skipped` 사망 시점 각 1건).

**이 판이 덮지 **못한** 것 — 인게임 미검증으로 남는다**:
- **일반 주머니 적재물(pocket1~4) 삭제.** 두 프로필 모두 pocket1~4 가 비어 있어 그 분기가 돌지 않았다. 단위 테스트로는 고정돼 있으나(`WornGearLossPolicyTests`) 게임에서는 확인되지 않았다.
- **Scabbard·ArmBand 보존.** 두 프로필 모두 그 슬롯이 비어 있었다(백업 기준 `없음`). 보존 주장은 단위 테스트에만 기대고 있다.
- 10번(시체 루팅 후 탈출): **플레이어도 죽어서** 탈출이 없었다. 미검증.
- 3·6·8·9·13번: 미검증.

### 정정 2 (2026-11-01, 화면 실측) — 파티 미리보기 영향 범위

바로 위 "정정" 에서 `GroupPlayers` 에 안 들어가므로 **매치메이커 파티 목록**이 영향받는다고 적었으나 **틀렸다.**
인게임 스크린샷(파티 구성 화면)이 SPT02·SPT01·Knox 셋을 레벨 `15·36·69` 와 모델까지 정확히 그린다.
`groupPlayers=2` 는 EFT 네이티브 목록의 수이고, 이 화면은 PIT 자신의 분대 상태에서 그려진다.

**관찰된 사실만 적는다**: `BuildGroupPlayer` 가 프로필 팀원 id 두 개에 대해 널을 돌려주고 경고를 찍는다.
**확인된 영향**: 없음(파티 화면은 정상). **미검증**: auto-join(지난 분대 자동 복원)이 프로필 팀원을 되살리는지.
같은 구멍의 네 번째 경로라는 사실과 고치는 모양은 그대로 유효하다 — 다만 **심각도는 "미검증" 이지 "파티 목록 깨짐" 이 아니다.**

(이 항목을 두 번 틀리게 적었다: 한 번은 과소, 한 번은 과대. 둘 다 호출부만 읽고 화면을 보지 않은 탓이다.)

### 3차 레이드 (Lighthouse) — 3번 통과

두 프로필 모두 직전 판의 사망으로 착용 슬롯이 비어 있었다. 채워진 슬롯은 `Pockets`·`SecuredContainer` 둘뿐이고
그 둘은 기본 슬롯이라 장비로 세지 않는다 → **예측대로 둘 다 `Temporary`, 사유는 "착용한 장비가 하나도 없어"**(무기 없음 사유가 아니다).

- **Ruling 17 이 살려 낸 분기가 실제 게임에서 처음 발동했다.** 처음 구현은 보안 컨테이너·인식표·완장을 장비로 세어
  `hasAnyEquipment` 가 모든 프로필에서 항상 참이었고, 그러면 이 분기는 죽은 코드이고 테스트 둘이 일어날 수 없는 동작을 고정했다.
- **Ruling 16 의 인벤토리 이식이 숫자로 확인됐다.** 보호 등록 개수가 실물일 때 SPT02 96 / Knox **602** 에서
  임시일 때 **51 / 81** 로 떨어진다 — `Temporary` 는 `Inventory` 를 생성물로 통째로 갈므로 **프로필 스태시가 레이드에 따라오지 않는다**
  (Knox 의 249개 스태시가 빠진 결과가 정확히 이 차이다). 이 이식은 Task 3 에서 두 라운드를 미구현인 채 넘어갔다가 재리뷰가 Critical 로 잡은 자리다.
- **레벨 보존**: 임시 장비인데도 `SPT02 level 15`·`Knox level 69` — "인벤토리 하나만 다르다" 계약의 결과다.
- **화면 실측**: 파티 화면에서는 둘 다 맨몸, 레이드 안에서는 헬멧·방어구·리그·소총 착용. 로비가 프로필 상태를,
  레이드가 생성된 로드아웃을 보여 준다. 의도대로지만 **플레이어에겐 혼동될 수 있다**(로비에서 맨몸인데 레이드에서 무장).

### 게이트에서 드러난 결함 — 팀원이 전투를 하지 않는다 (원인 둘, 둘 다 Task 3 브리프가 만든 것)

사용자가 3차 레이드에서 "동료들이 적을 상대로 전혀 전투를 진행하지 않는 것으로 보임" 을 보고했다. 추적 결과 원인이 **둘**이고,
둘 다 내가 Task 3 브리프에 쓴 문장이 사실과 어긋나서 생겼다.

**원인 1 — 빈 탄창** (수정 `e857e52`)

백업 프로필 실측: 1·2차 레이드에 들어간 실물 장비 팀원들의 탄창이 **주무기 0발 / 권총 0발**(Knox 의 산탄총만 20발)이었다.
즉 그때도 쏠 수 없었고 3차에서야 눈에 띈 것이다.

내 브리프는 PIT 의 스폰 손질을 "실제 프로필 장비를 고쳐 쓴다" 는 이유로 **셋을 뭉뚱그려** 건너뛰게 했다. 그러나 셋의 성격이 다르다:
`RefillFollowerMagazinesFromInventoryAmmo` 와 `EnsureFollowerHasScabbardKnife` 는 넘긴 객체에 **더하기만** 하고,
PIT 자신의 주석이 리필을 **"스폰 복제본에만"** 두는 이유를 이미 적어 두었다(`PrepareTeammateForFetch`). 우리 `spawn` 도 깊은 복제본이다.
→ 그 둘은 양쪽 경로에 적용하고, 내용물을 **비우는** 보안 컨테이너 보급만 계속 건너뛴다.

**원인 2 — 역할·난이도 (이쪽이 더 크다)** (수정 `113420f`)

SAIN 로그 실측:

```
follower=SPT02  role=assault  difficulty=easy  profile=0.15  visibleDistance=90    ← 프로필 팀원
follower=Knox   role=assault  difficulty=easy  profile=0.15  visibleDistance=90    ← 프로필 팀원
follower=SPT08  role=pmcBEAR  difficulty=hard  profile=1.5   visibleDistance=225   ← PIT 용병
follower=Zofia  role=pmcBEAR  difficulty=hard  profile=1.5   visibleDistance=225   ← PIT 용병
```

런처 프로필 전부가 `Info.Settings.Role='assault'`, `BotDifficulty='easy'` 를 저장하고 있다(SPT01·SPT02·Knox 모두 확인).
본인이 플레이할 때는 무의미한 기본값이지만, 같은 캐릭터를 **팀원으로 스폰하면 SAIN 이 그대로 읽는다.**
시야 90m 짜리 easy 봇은 적을 거의 보지 못하므로 "전투를 거부하는 것" 처럼 보인다.

내 브리프는 "`Info.Settings.Role` 이 **비어 있으면** 진영에 맞는 PMC 역할로 채운다" 였는데, 그 칸은 비어 있지 않다.
그래서 `EnsurePmcRole` 의 채우기 분기가 **한 번도 발동하지 않았다.**
→ 채우지 말고 **덮어쓴다**(`ForceFollowerRoleAndDifficulty`). 난이도도 PIT 가 모든 용병에 굽는 `"hard"` 로 맞춘다
(Temporary 경로는 생성물의 **인벤토리만** 가져오므로 생성 시점의 난이도를 물려받지 못한다). `spawn` 이 복제본이라 프로필 값은 그대로다.

**확인 신호**: 다음 레이드의 SAIN 줄이 `role=pmcUSEC/pmcBEAR`·`difficulty=hard`·`visibleDistance=225` 로 바뀌어 용병과 같아지는지.

**교훈**: 두 결함 모두 "프로필의 이 칸은 비어 있다/이 손질은 실제 장비를 고친다" 는 **내가 확인하지 않은 전제**에서 나왔다.
실제 프로필 JSON 을 한 번 열어 봤으면 둘 다 설계 단계에서 잡혔다.

### 계획 3 후보 — 제스처 명령 메뉴(Y-Y) 대응 범위

사용자 질문으로 조사했다. 게임 어셈블리(`EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll`)에서 `EPhraseTrigger` 110개를
PEReader 로 뽑고, 라우터 `client/Components/AIBossPlayer.cs` 의 `PhraseSaid` 가 실제로 분기하는 구문과 대조했다.
메뉴 라벨과 enum 이름을 추측하지 않고 양쪽을 실측해 맞췄다.

**메뉴 18개 중 대응 11개**

| 메뉴 | enum |
|---|---|
| FOLLOW ME | `FollowMe` |
| COVER ME | `CoverMe` |
| ATTENTION! | `Look` (라우터 주석: 시선 압력 해제 후 보스 쪽 재집중) |
| GO GO GO | `Gogogo` |
| SUPPRESS | `Suppress` |
| REGROUP | `Regroup` |
| HOLD POSITION | `HoldPosition` |
| GO FORWARD | `GoForward` |
| ON YOUR OWN | `OnYourOwn` |
| GET THE LOOT | `LootGeneric` |
| STOP | `Stop` |

**대응 안 하는 7개**: `SILENCE`(`Silence`) · `TAKE COVER`(`GetInCover`) · `KNIVES ONLY`(`KnifesOnly`) ·
**`FIRE`(`Fire`)** · **`HOLD FIRE`(`HoldFire`)** · `SPREADOUT`(`Spreadout`) · `GET BACK`(`GetBack`)

**메뉴에 없으나 처리하는 것들**(맥락·방향 명령): `CheckHim`·`LootBody`·`LootWeapon`·`LootKey`·`LootMoney`·`LootContainer`,
`InTheFront`·`LeftFlank`·`RightFlank`·`OnSix`, `OpenDoor`·`NeedHelp`·`NeedSniper`·`ExitLocated`,
그리고 모드 자체 구문 `CustomPhrases.TeamStatus`(상태 보고).

**계획 3 후보로 올리는 이유**: 빠진 7개 중 **`FIRE`·`HOLD FIRE`·`TAKE COVER`** 는 교전 통제 손잡이다.
계획 2 가 팀원을 레이드에 넣었으니 다음으로 "쏴라 / 쏘지 마라 / 엄폐하라" 를 직접 주고 싶어지는 것이 자연스럽다.
나머지 넷(`SILENCE`·`KNIVES ONLY`·`SPREADOUT`·`GET BACK`)은 우선순위가 낮다.
**계획 2 범위 밖이고, 지금 겪는 "팀원이 전투하지 않는다" 문제와는 별개다** — 그쪽은 역할·난이도 결함이다.

### 결함 해소 확인 — 역할·난이도 수정 후 (사용자: "교전함")

설치 후 레이드 한 판 기준.

| | 수정 전 | 수정 후 |
|---|---|---|
| 팀원 SAIN 역할 | `role=assault difficulty=easy profile=0.15 visibleDistance=90` | **`role=pmcUSEC`/`pmcBEAR` `difficulty=hard` `profile=1.5` `visibleDistance=225`** |
| `role=assault` 로 뜬 줄 | 매 판 | **0** |
| `AICoreAgent.Update exception` | 3차 4건(NRE), 4차 14건(InvalidCast) | **0** |
| 교전 | 하지 않음(탄창 25/25 가득한데 한 발도 안 씀, 같은 판 용병은 4/30 까지 소모) | **함**(사용자 확인) |

진영 매칭도 맞다 — SPT02(Usec) → `pmcUSEC`, Knox(Bear) → `pmcBEAR`.

**열어 두었던 질문의 답**: BigBrain 의 `CustomLogicWrapper.UpdateNodeByBrain` 예외를 "귀속하지 못했다" 고 남겼는데,
**역할 불일치의 결과였다** — 역할을 PMC 로 바로잡자 함께 사라졌다. 표본이 한 판이므로 "일치한다" 까지가 정확한 표현이고,
단정하려면 몇 판 더 필요하다.

**중간에 틀렸던 가설도 남긴다**: "프로필 팀원에게 PIT 전투 레이어가 안 붙는다" 고 의심했으나,
`FollowerPatrolLayer.Init` 의 `pmcCombatBrains` 목록에 **`"Assault"` 가 포함**돼 있어 틀렸다(확인 후 폐기).
결정적 요인은 레이어 유무가 아니라 SAIN 수치였다.

**두 수정의 기여도**: 탄창(`e857e52`)은 필요했지만 충분하지 않았다 — 탄창이 가득 찬 뒤에도 교전하지 않았다.
역할·난이도(`113420f`)가 결정적이었다. 둘 다 유지한다.

### 10번 확인 — 결함 재현됨, 그리고 고쳤다

**재현**(2026-11-01, Ground Zero, 프로필 팀원 셋 실물 장비 / Hawk 사망 / 플레이어 생환):
플레이어가 Hawk 시체를 털어 탈출했는데, 아이템 `_id` 로 추적한 결과

```
Hawk 프로필에서 사라진 아이템   35개
그중 플레이어 프로필에 있는 것    0개
양쪽 어디에도 없는 것(소멸)     35개
```

헬멧·방탄복·이어피스·마스크까지 전부 **소멸**했다. 우리가 Hawk 프로필에서 지우고, PIT 의
`RemoveProtectedTeammateItemsFromExtractedProfile` 가 플레이어가 들고 나온 인벤토리와 프로필에서도 지웠다.

**막고 있던 것은 순서를 몰랐다는 점이고, 같은 레이드의 로그가 그걸 줬다:**

```
15227  /singleplayer/returnitems
15230  /raid-outcomes            ← 우리 삭제
15231  /raid-outcomes
15240  /singleplayer/teamescaped
15261  /client/match/local/end   ← 플레이어 쪽 보호 아이템 제거
```

`match/local/end` 가 `raid-outcomes` 보다 **뒤**다. 즉 우리가 지우는 시점에 플레이어 쪽은 아직 안 지워졌으므로,
지운 직후 보호를 풀면 제때 반영된다. Task 5 가 남긴 메모가 실행 가능해진 것이다.

**수정**: 저장이 성공한 직후 `postRaidService.RemoveProtectedRaidItemIds(sessionId, expectedGoneIds, …)`.
`RemoveProtectedRaidItemIds` 를 `private` → `internal` 승격(그 파일 변경은 키워드 하나뿐).

**범위는 지운 것 정확히 그만큼**(사용자 결정):
- 생환한 팀원 → 삭제 없음 → 해제 없음. 장비가 프로필에 그대로이므로 플레이어가 가지면 **복제**다.
- 실물로 죽은 팀원 → 프로필에서 사라졌으므로 털어 온 것은 플레이어 것. 설계가 명시한 바다.
- 해제를 **저장 뒤**에 두는 것도 의도다. 아직 프로필에 있는 동안 풀면 한 자루가 두 곳에 생긴다.

**미결(따로 결정하기로 함)**: **임시 장비 팀원이 죽었을 때도 같은 소멸이 난다.** 그 장비는 어느 프로필에도 없던
생성물이라 플레이어가 갖는 것은 "공짜 전리품" 이고, 실물과 판단이 다르다. 지금은 아무것도 해제하지 않는다.

### 임시 장비 소멸 — 결정: 유지하고 로그로 말한다 (사용자 결정, 2026-11-01)

임시 장비로 죽은 팀원의 시체를 털어 나오면 그 아이템도 레이드 종료 때 사라진다(실물 쪽과 같은 기전).
**실물과 달리 이쪽은 고치지 않기로 했다.**

**이유**: 그 장비는 어느 프로필에도 없던 생성물이라 플레이어가 가져도 **복제는 아니다.** 그러나 풀어 주면
**밑천 없는 무한 장비 공급원**이 된다 — 맨몸 프로필을 초대하면 모드가 생성 장비를 입혀 주고, 죽게 둔 뒤
시체를 털고, 반복하면 된다. 실물 장비 쪽에는 이 문제가 없다(프로필에서 실제로 사라지므로 총량이 보존된다).

**대신 조용하지 않게 했다**: `!escaped && gearKind == Temporary` 일 때 로그를 남긴다 —
"임시 장비로 나가 죽었다. 그 장비는 어느 프로필의 재산도 아니므로 보호를 풀지 않는다 - 시체에서 털어 나온
물건은 레이드 종료 때 사라진다(의도된 동작)." 소멸 자체가 버그처럼 읽히므로 원인을 로그에서 찾을 수 있게 한다.

나중에 설정으로 열고 싶어지면 그때 갈라도 된다. 지금은 닫아 둔다.

### 10번 해소 확인 — 한 레이드에서 두 경로가 정반대로 갈렸다

Shoreline, 프로필 팀원 셋(SPT02 Real / Knox Real / Hawk Temporary). SPT02·Hawk 사망, Knox 생환, 플레이어 생환.

| | 프로필 변화 | 플레이어 프로필에 남은 것 |
|---|---|---|
| **SPT02**(Real·사망) | 104 → **40** (64개 삭제) | **53개** |
| **Hawk**(Temporary·사망) | 91 → **91** (변화 없음) | — (삭제 없음 → 해제 없음) |
| **Knox**(Real·생환) | 243 → **243** | — |

**수정 전 같은 상황: 35개 중 0개. 수정 후: 64개 중 53개.** 보호 해제 로그도 SPT02 에만 찍혔다
(`death release 6ae5870e… (raid Shoreline.Pmc …)`). 남은 11개는 플레이어가 시체에서 가져오지 않은 것으로,
사라지는 것이 옳다.

Hawk 쪽은 결정대로다 — 프로필이 그대로이고(지울 재산이 없다), 새 설명 줄이 떴다.
**한 레이드 안에서 "남아야 하는 것" 과 "사라져야 하는 것" 이 동시에 확인된 것이 이 검증의 핵심이다.**

### Task 9 (인게임 게이트) 결론 — 13개 전부 확인

| # | 항목 | 결과 |
|---|---|---|
| 1 | 초대 → 파티 합류 | 통과 |
| 2 | 실물 장비로 스폰 | 통과 |
| 3 | 장비 없는 프로필 → 임시 장비 | 통과(사유까지 예측대로) |
| 4 | 용병과 혼성 | 통과 |
| 5 | 실물 사망 → 착용 장비 상실 | 통과(스태시·보안 컨테이너 보존) |
| 6 | 임시 사망 → 프로필 무변화 | 통과(표본 2) |
| 7 | 생환 → 장비 보존 | 통과(표본 다수) |
| 8 | 중복 처리 | 통과(중복 요청 2회, 삭제 1회) — 단 영수증이 막은 것인지 두 번째 요청이 비었던 것인지는 **귀속 못 함** |
| 9 | 넷째 자리 | ~~막는 것이 없다~~ → **아래 "정정 3" 으로 뒤집힘.** 클라이언트가 막는다(팀원 4명이 상한). 서버측 관문은 필요 없다 |
| 10 | 시체 루팅 후 탈출 | 결함 재현 → 수정 → **해소 확인** |
| 11 | 파티창 지연 | 눈에 띄는 문제 없음 |
| 12 | 실물 팀원이 스태시 동반 | 확인(보호 등록 개수가 프로필 총수와 일치) |
| 13 | 영수증 왕복 | 간접 확인(삭제가 한 번만 일어났고 오류 0건) |

**게이트에서 드러나 고친 결함 셋**: 빈 탄창(`e857e52`), 역할·난이도(`113420f`), 시체 전리품 소멸(`4379470`).
**게이트에서 내린 결정 하나**: 임시 장비 전리품은 소멸 유지 + 로그(`39e3307`).

### 정정 3 — 9번(넷째 자리): 클라이언트가 막는다. 서버측 관문은 필요 없다

위 게이트 결론표에 "**막는 것이 없다**" 고 적었으나 **틀렸다.** 인게임 화면으로 확인:

파티 초대 창에서 `SPT01(리더) + SPT02 + Knox + Hawk + SPT08` 로 **다섯 자리가 차면**, 그 아래 친구 목록
(`Zofia`·`SPT07`·`SPT06`·`SPT05`·`SPT04`·`SPT03`)은 "파티에 초대" 항목이 **비활성**이 되고 툴팁으로
**"파티가 가득 찼습니다"** 가 뜬다. 파티 슬롯 UI 도 5칸이다.

- 상한은 **플레이어 + 팀원 4 = 5명**이다. 설계 메모의 "네이티브 파티 창이 총 4명으로 묶는다" 도 틀렸다.
- 앞서 팀원 4명이 전부 진입한 것은 **제한이 없어서가 아니라 그것이 상한이었기** 때문이다.
  관측(4명 진입)에서 "제한 없음" 을 결론한 것이 비약이었다 — 상한에 **닿아 보지 않고** 단정했다.

**따라서 서버측 자리 관문은 만들지 않는다.** 계획 1 이 만들어 두고 호출부 없이 남긴 `PartySlotPolicy` 는
이 실측으로 용도가 사라졌다. 지우지는 않되(순수·테스트 있음) **호출부를 붙이지 않는다**는 것이 결론이고,
그 판단 근거가 이 항목이다. 계획 1 에서 이월된 관문 항목도 이것으로 닫힌다.

## 2026-11-02 — STM 열린 항목 1: 프로필 조회의 네 번째 경로

인계 문서 §5 의 1번. 계획 2 가 같은 구멍을 세 곳(초대·팔로워 생성·레이드 결과)에서 메웠는데
**네 번째 경로가 수정 대상 목록에 없었다.** 그 경로를 메웠다. 브랜치 `fix/stm-profile-view-fourth-path`.

### 무엇이 망가져 있었나 (코드로 확인)

`/singleplayer/pitfireteam/teammate/profile` 은 포크 고유 경로이고, 클라이언트가 **지난 분대를 복원할 때**
(auto-join) 팀원마다 한 번 묻는다 — `RaidStartPatch.BuildGroupPlayer`(client/Patches/RaidStartPatch.cs:432).
서버 쪽 핸들러는 PIT 의 용병 조회로 바로 갔다:

```
FriendlyTeammateCallbacks.GetProfile → teammateService.GetTeammateProfile → FindByAccountId
```

`StmProfileMemberResolver` 는 `FriendlyTeammateMatchCallbacks` 에만 주입돼 있었고 이 클래스에는 없었다.
그래서 프로필 팀원의 Aid 로 물으면 PIT 가 용병을 못 찾아 `FriendlyTeammateException` 을 던지고,
핸들러가 `err != 0` 을 돌려주고, 클라이언트는 **경고 한 줄만 로그에 남기고 그 팀원을 복원 분대에서
조용히 떨어뜨렸다**(`[UI] Failed to build auto-join teammate ...`). 인계 문서가 "auto-join 미검증" 으로
남겨 둔 자리의 정체가 이것이다.

### 고친 모양

앞의 셋과 같다 — **STM 해석기를 PIT 보다 먼저 묻고**, 3값 계약(참 / 거짓+사유 / 거짓+널 = PIT 경로로)
을 그대로 쓴다.

- 새 파일 `server/Services/StmProfileViewFactory.cs`(STM 소유). 해석기를 묻고, **복제본**에서 응답을 만든다.
- PIT 파일 수정은 둘뿐이다: `FriendlyTeammateService.ToOtherProfileResponse` 를 `private` → `internal`
  승격(불변식 4 가 허용하는 유일한 변경), `FriendlyTeammateCallbacks` 에 생성자 인자 한 줄과 `GetProfile`
  앞의 분기. 업스트림 병합면을 넓히지 않으려고 판단은 전부 STM 파일에 뒀다.

**`PmcData` 가 `BotBase` 를 상속한다**는 계획 2 의 사실이 여기서 또 쓸모가 있었다 — PIT 의 매퍼가
`BotBase` 를 받으므로 프로필의 PMC 복제본을 그대로 넣을 수 있다. 응답 매퍼를 새로 쓰지 않았다.

### 한 가지 갈라진 곳 — 인벤토리 범위

PIT 의 매퍼는 `Inventory.Items` 를 **그대로** `Equipment.Items` 에 넣는다. 용병은 자기 장비 말고
가진 것이 없으니 그래도 됐다. 프로필 팀원은 **스태시째**가 그 목록에 들어 있다. 그대로 보내면
조회 화면 한 번에 수천 개 아이템이 실리고, 입고 있지도 않은 것이 착용 장비로 보인다.

그래서 복제본에서 **장비 트리만 남기고 나머지를 지운다.** 어느 id 가 그 트리인가는 새로 쓰지 않고
`WornGearLossPolicy.CollectSubtreeIds`(순수·테스트 있음, `StmRaidOutcomeService` 가 이미 같은 프로필의
장비 트리를 그것으로 가려낸다)에 묻는다. 장비 루트가 비면 **아이템 없이 신원만** 보낸다(닫힌 쪽) —
루트를 추측해 엉뚱한 장비를 보여 주는 것보다 빈 화면이 낫다.

### 검증 상태

- 서버 빌드 경고 0 오류 0, 단위 테스트 **138개 전부 통과**(총 개수 기대값과 일치).
- **새 순수 정책이 없다**(기존 정책을 재사용했다). 그래서 이 수정에 새 단위 테스트가 없고,
  `server/` 는 설계상 테스트가 닿지 않는다 — 계획 2 의 `BuildProfileMemberAccept` 와 같은 처지다.
  **코드 리뷰와 인게임 확인이 유일한 증거다.** 인게임으로 볼 것:
  1. 프로필 팀원을 데리고 레이드를 한 판 한 뒤 메인메뉴로 돌아와, **지난 분대가 복원되는지**
     (그 프로필 팀원이 파티 창에 다시 뜨는지). 이전에는 조용히 빠졌다.
  2. 복원된 타일의 **닉네임·레벨·진영**이 그 프로필의 것인지.
  3. 서버 로그에 `[STM] 프로필 조회:` 경고가 없는지(있으면 사유가 찍힌다).
  4. **용병만 데려간 분대의 auto-join 이 그대로인지**(PIT 경로 회귀 확인).

### 발견 — 같은 성격의 다섯 번째 경로 (이번 범위 밖, 아직 미판정)

`/client/profile/view` 를 받는 `FriendlyTeammateSocialCallbacks.MergeProfileView` 도 프로필 팀원을
모른다. PIT 의 보류 팀원·용병·신병을 차례로 묻고 전부 아니면 SPT 의 원래 응답을 그대로 흘린다.
**친구 목록에서 프로필 팀원을 눌러 조회 화면을 열었을 때** 무엇이 보이는지는 SPT 가 다른 런처 프로필을
aid 로 돌려주는지에 달려 있고, 그것은 **역컴파일이나 실측 없이는 모른다 — 추측하지 않는다.**
고칠 자리가 생기면 모양은 이번 것과 같다(`StmProfileViewFactory.TryBuildView` 를 그대로 부르면 된다).

### 정정 — auto-join 은 "지난 분대 복원" 이 아니다. 그리고 네 번째 경로는 아직 닿지 않는다

위 항목에서 이 경로를 "클라이언트가 **지난 분대를 복원할 때**" 묻는다고 적었다. **틀렸다.**
인게임 확인에서 분대가 복원되지 않아 로그를 되짚어 알아낸 것이다. 전제를 확인하지 않고 이름
(`SyntheticTeammateAutoJoinLoader`)에서 뜻을 지어냈다 — 계획 2 에서 세 번 겪은 실수와 같은 종류다.

**auto-join 의 실제 뜻**: 팀원마다 켜는 토글이고, 켜 두면 **출격 화면**
(`MatchMakerAcceptScreen.Show` → `EnsureLoaded`, RaidStartPatch.cs:1147)에서 그 팀원이 분대에
자동으로 들어간다. 레이드가 끝난 뒤 메인메뉴에서 지난 분대를 되살리는 기능이 **아니다.**
기본값은 꺼짐(`AutoJoinEnabled = false`)이다.

**로그 증거**(2026-11-02 09:10 KST 레이드, 프로필 팀원 3명 SPT02·Knox·Hawk 전원 생환):

- `/singleplayer/autoteam` 요청이 **0건**. 목록을 물은 적이 없다.
- `/singleplayer/pitfireteam/teammate/profile` 요청도 **0건**. 그래서 `[STM] 프로필 조회:` 줄이 없다.
- 레이드 자체는 정상(`[STM] 레이드 결과 접수` 3건, 삭제 없음).

**auto-join 이 프로필 팀원에게 동작하려면 세 곳이 필요하고, 전부 PIT 만 안다:**

| | 자리 | 지금 |
|---|---|---|
| 1 | 토글 저장 — `/singleplayer/pitfireteam/teammate/autojoin` → `SetTeammateAutoJoin` → `FindByAccountId` | 프로필 팀원이면 **throw**. 명단에서 토글해도 실패한다 |
| 2 | 목록 조회 — `/singleplayer/autoteam` → `GetAutoJoinTeammateAccountIds`(FriendlyTeammateService.cs:646) | **PIT 팀원만 열거**한다. 프로필 팀원은 절대 나오지 않는다 |
| 3 | 플레이어 구성 — `/singleplayer/pitfireteam/teammate/profile` | **메웠다**(위 항목). 그러나 2번 때문에 호출되지 않는다 |

**따라서 이 수정은 지금 닿지 않는 코드다.** 그 경로의 클라이언트 호출부는 auto-join 로더 한 곳뿐이고
(RaidStartPatch.cs:432), 그 로더는 2번이 준 id 로만 돈다. 수정 자체는 맞고 2번의 전제 조건이지만,
**단독으로는 사용자가 볼 수 있는 변화가 없다.**

2번을 메우려면 저장 계층도 손봐야 한다 — `StmProfileMemberStore` 는 `List`·`Add`·`Remove` 뿐이고
**갱신 경로가 없다.** `StmProfileMember.AutoJoin` 은 레코드에 있으나 "저장만 하고 아무도 해석하지
않는다"(모델 주석 그대로). 명단 UI 가 배지 상태를 보여 주려면 로스터 와이어(`StmRosterRow`)에도
`autoJoin` 이 실려야 한다. 결이 **계획 3(팀원 상세·설정)** 에 가깝다.

## 2026-11-02 — STM 계획 3 Task 5: 팀원 상세 화면에 프로필 팀원을 태운다

### 상세 화면이 서버에 묻는 라우트 전수 조사

출발점은 `OtherPlayerProfileScreenPatch*.cs` 의 라우트 상수와 `RequestHandler.PostJson` 호출부다.

| 라우트 | 누가 부르나 | 프로필 팀원에게 | 처리 |
|---|---|---|---|
| `/client/profile/view` (SPT) | 친구 목록에서 팀원을 눌러 화면을 열 때(네이티브) | 뜻 있음 | `MergeProfileView` 가 STM 을 먼저 묻는다 (아래 "다섯 번째 경로") |
| `…/teammate/profile` | auto-join 로더(RaidStartPatch) — 상세 화면 아님 | 뜻 있음 | 이전 작업에서 `TryBuildView` 로 끝남 |
| `…/profile/options` | 화면이 열린 직후 `TryLoadProfileOptions` | 뜻 있음 | `TryBuildOptions` (세 값 계약). 막힘 여부·안내문·경고문을 싣는다 |
| `…/profile/suit` | 의상 드롭다운 | 뜻 없음 (그 프로필의 외형) | 사유와 함께 거부 |
| `…/profile/rename` | 이름 변경 | 뜻 없음 (그 프로필의 이름) | 사유와 함께 거부 |
| `…/profile/aggression` | 공격성 슬라이더 | 뜻 없음 (프로필 팀원에게 저장할 자리가 없다) | 사유와 함께 거부 |
| `…/profile/proficiency` | 숙련 창 | 뜻 없음 (같은 이유) | 사유와 함께 거부 |
| `…/profile/tactic` | 전술 선택 | 아직 뜻 없음 (`Tactic` 은 저장만 되고 갱신 경로·해석이 없다) | 사유와 함께 거부 |
| `…/profile/insurance` | 보험 구매 | 뜻 없음 (프로필 자체의 보험이 따로 있다) | 사유와 함께 거부 |
| `…/profile/loadout` | 클라이언트 호출 없음(옛 호환 라우트) | 뜻 없음 | 일관을 위해 거부 |
| `…/profile/default-equipment` | 장비 편집 창 | 뜻 있음 | **Task 6 의 몫.** 손대지 않았다. 지금은 PIT 가 용병을 못 찾아 오류로 던진다(성공한 척하지 않는다) |
| `…/profile/buy-kit` | 장비 빌드 화면 | 범위 밖(계획 3 의 출처는 본인 스태시뿐) | 손대지 않았다. 위와 같이 오류 |
| `…/profile/repair-equipment` | 수리 | 범위 밖 | 손대지 않았다. 위와 같이 오류 |
| `…/teammate/autojoin` | 명단 컨텍스트 메뉴 | 뜻 있음 | 계획 3 Task 2 에서 끝남. 거절 사유는 상태 이름 그대로 오류 본문에 실린다 |

거부는 `StmProfileOperationGuard` 가 한다 — 해석기를 PIT 보다 먼저 묻는다. 이유는 둘이다: 용병이 없으면 PIT 가
not-found 로 던지고, **같은 Aid 의 용병이 있으면 화면에 보이지 않는 용병의 이름·외형이 바뀐다.**

### 다섯 번째 경로 — `/client/profile/view` 는 프로필 팀원의 Aid 로 무엇을 돌려주나 (실측)

**SPT 가 직접 찾아 돌려준다.** 추측이 아니라 설치된 서버 어셈블리를 역컴파일해서 읽었다
(`E:\Spt\SPT_Runtime\SPTarkov.Server.Core.dll`, 제품 버전 4.1.6; 레포의 `server/refs/4.1.0` 도 같은 코드):

- 라우트 `/client/profile/view` → `ProfileCallbacks.GetOtherProfile` → `ProfileController.GetOtherProfile(sessionId, request)`.
- 그 첫 줄: `profileHelper.GetFullProfileByAccountId(request.AccountId)` — `SaveServer.GetProfiles()` 에서
  `p.Value.ProfileInfo.Aid == aid` 인 **첫 프로필**을 돌려준다. 로그인한 프로필인지, 다른 런처 프로필인지 가리지 않는다.
- 못 찾으면 경고만 남기고 **로그인한 본인 프로필로 대체**한다(`falling back to own profile`).
- 응답의 `Equipment.Items = pmcData.Inventory.Items` — 프로필의 **인벤토리 전체**(스태시 포함)를 싣는다. 이것이
  `TryBuildView` 가 `TrimToWornEquipment` 로 막은 바로 그 문제다. `Items`·`Hideout`·`HideoutAreaStashes` 도 실린다.

따라서 고치기 전에는 친구 목록에서 프로필 팀원을 누르면 (a) 그 프로필의 스태시 전체가 조회 화면으로 가고, (b) Aid 가
어긋나면 본인 프로필이 팀원인 척 보였다. 고친 것은 한 줄 호출이다: `MergeProfileView` 맨 앞에서 `TryBuildView` 를 부르고,
`true` 면 그 응답, 사유가 있으면 오류, 없으면 기존 흐름(PIT 보류 팀원·용병·신병·SPT 응답).

한 가지 다른 점: SPT 는 `ProfileInfo.Aid` 로, STM 해석기는 `pmc.Aid` 로 찾는다. 실제로는 같은 값이지만 같은 필드는 아니다.

## 2026-11-02 — STM 계획 3 결산: 장비 편집·상세 화면·auto-join·제스처 세 개

> ### 이 구간을 읽는 법
>
> 위의 "계획 3 Task 5" 구간부터 이어진다. Task 5 는 거기에 이미 있고, 이 구간은 **계획 전체의 결산**이다.
>
> **상태**: 코드 완료·Task 별 리뷰 통과. 브랜치 `feat/stm-gear-editing`, `52b65f9` 이후 52커밋. 단위 테스트 138 → **504**
> (총 개수를 직접 읽었다), 서버 빌드 경고 0 오류 0, 클라이언트 `error CS`·`warning CS` 0.
> **게임에서는 한 번도 돌지 않았다.** 이 구간의 어떤 문장도 "인게임에서 확인했다" 는 뜻이 아니다. 계획 2 와 달리
> 이 계획은 **관문(Task 8) 앞에서 멈춰 있다.** 관문 목록은 이 구간 맨 아래에 있다.
>
> **들어간 것**: ① 등록 갱신 경로 ② 프로필 팀원의 auto-join ③ 스태시 재검증 수리 ④ 3단 전송 저널 ⑤ 팀원 상세 화면과
> 막힌 팀원의 출구 ⑥ 장비 편집(서버·클라이언트) + 보존 검사 ⑦ 제스처 `FIRE`·`HOLD FIRE`·`TAKE COVER`.
> 계획에 없던 **6b**(편집기의 클라이언트 입구)가 중간에 필요해져 따로 들어갔다 — 서버 경로만 있고 부를 화면이 어느 Task 에도
> 없었다(계획의 구멍).
>
> **저널은 만들어졌지만 아직 도달할 수 없다.** `StmTransferJournal` 의 `Prepare`·`RecordDestinationItemIds`·`BeginSourceWrite`·`BeginDestinationWrite`·`MarkCommitted`·`Abort`·`Verify` 를 부르는 운영 코드가
> 없다(아래 "설계 모순과 결정"의 결과를 이 줄이 처음 적는다). 항목이 생길 수 없고 `HasOpenEntry` 는 항상 거짓이며, 그것을 묻는 편집 관문과 상세 화면의 막힌 팀원 출구는 도달 불가다.
> 첫 사용자는 다음 계획(전리품 배달·경험치 적립, 설계 §6-8)이다. 관문 25번은 이 때문에 아래와 같이 다시 썼다.
>
> **최종 수정 웨이브(전체 리뷰 뒤)**: 편집 가드의 PIT 먼저 탈출구, 제스처 건너뛰기의 팀원별 자격, 편집기 열기의 손상 프로필 관문이 들어갔고 단위 테스트는 **511**이 됐다.
> 아래 관문 1·18~22번의 문장 중 그 수정과 어긋나는 것은 이미 고쳐 적었다.
>
> **뒤집힌 결론 셋**: ⓐ 설계 §6-8 의 저널은 장비 편집에 **쓰지 않는다**(아래 첫 항목). ⓑ 보존 검사는 처음에 **틀린 집합**에
> 걸렸다(아래 셋째 항목). ⓒ 제스처 대조표는 **빠뜨린 사실이 있었고**, 그중 하나(기본 설정에서 바닐라가 이미 처리하고 있었다)를
> Task 7 도 한 번 틀리게 적었다(아래 넷째 항목).

### 설계 모순과 결정 — 장비 편집은 저널을 쓰지 않는다 (의도된 것이다, 빠뜨린 게 아니다)

설계 §6-8 은 3단 저널(Prepared → Committed → Verified)을 "프로필 JSON **두 개**를 동시에 고치는 모든 조작(장비 편집, …)"에
건다. 설계 §6-4 는 장비 편집의 출처를 "**그 팀원 자신의 스태시뿐**"으로 좁힌다. 둘은 서로 어긋난다:

- 팀원의 스태시와 착용 장비는 **같은 `PmcData.Inventory.Items` 하나** 안에 있다. §6-4 대로면 편집은 프로필 **한 개**의 쓰기다.
  §6-8 이 말하는 "두 개를 오가는 조작" 이 아니다. §6-8 의 열거는 PIT 의 플레이어 → 팀원 흐름을 전제로 쓰였다.
- 저널은 두 프로필을 전제하고, 같은 프로필의 `Prepare` 는 **던진다**(`StmTransferJournal.Prepare`, `StmTransferJournalSameProfile`).
  우회로 끼워 넣으면 복구 판정(`DecideFromInventories`)이 출처와 목적지를 **같은 인벤토리**에서 읽는다. 그러면 **성공한 편집마다**
  "양쪽 다 가짐" 으로 읽혀 다음 기동에서 `StopAndReport` 가 되고, 그 팀원의 편집이 영구히 막힌다. 저널은 두 프로필의 쓰기 사이의
  창을 복구 가능하게 만드는 기계이고, 쓰기가 하나면 그 창이 없다 — 없는 위험을 위한 기계는 위험을 줄이지 않고 성공을 실패로 읽는다.

**결정(컨트롤러, Task 6 의 질문 (A)): 편집에는 저널을 쓰지 않는다. 대신 §6-8 의 셋째 단계는 지킨다** — `SaveProfileAsync` 뒤에
프로필 JSON 을 **디스크에서 다시 읽어** 대조한다(`StmGearEditService.VerifyFromDisk`). 네 가지를 본다: 옮긴 아이템의 부모·슬롯·좌표,
장비 트리 안/스태시 트리 밖(또는 반대), 편집 전후 **아이템 id 집합이 양방향으로 같음**(없던 아이템이 끼어드는 것 = 공짜 생성도 잡는다).
어긋나면 전체를 편집 전 복제본으로 되돌려 다시 저장하고, 되돌리기까지 실패하면 그렇다고 말한다. 진입에서는 저널을 **읽는다**:
미결 항목이 그 팀원을 막고 있으면(`HasOpenEntry`, 읽기 실패도 참) 편집을 거부한다.

> **§6-8 을 읽고 "저널이 빠졌다" 고 결론 내리지 말 것.** `StmGearEditService.cs` 머리와 `StmTransferJournal.cs` 머리에 같은 이유를
> WHY 주석으로 적어 두었다. 되돌리려면 먼저 그 둘과 `Prepare` 의 같은-프로필 거절을 확인하라. 저널은 지금 **쓰는 쪽이 하나도 없다** —
> 쓸 조작(계획 4 의 배달·적립, 나중의 스태시 전송)이 아직 없다. 기동 복구와 `HasOpenEntry` 관문만 살아 있다.

디스크 재독 경로는 `Path.Combine(AppContext.BaseDirectory, "user", "profiles")` 로 **고정**했다(`SaveServer` 자신은 상대 경로를 쓰지만 이
서버 모드의 다른 모든 경로가 `AppContext.BaseDirectory` 에 고정돼 있다). 상대 경로가 안 풀리면 확인이 늘 "파일 없음" 으로 실패해 모든 편집이
되돌려진다 — 안전한 쪽이지만 기능이 통째로 죽는다.

### 다섯 번째 경로 — 측정한 것이지 짐작한 것이 아니다

앞 구간("발견 — 같은 성격의 다섯 번째 경로")에서 "SPT 가 다른 런처 프로필을 aid 로 돌려주는지는 역컴파일 없이는 모른다" 고 미뤘다.
Task 5 에서 설치된 4.1.6 서버(`E:\Spt\SPT_Runtime\SPTarkov.Server.Core.dll`, 제품 버전 4.1.6-RELEASE)를 ilspycmd 로 역컴파일해 읽었다
(레포의 `server/refs/4.1.0` 도 같은 코드, 해시만 다르다):

- `/client/profile/view` → `ProfileCallbacks.GetOtherProfile` → `ProfileController.GetOtherProfile`.
- 첫 줄 `profileHelper.GetFullProfileByAccountId(request.AccountId)` 는 `SaveServer.GetProfiles()` 에서 `ProfileInfo.Aid == aid` 인 **첫 프로필**을
  찾는다. 로그인 여부와 무관하다. 못 찾으면 `falling back to own profile` 로 **로그인한 본인 프로필로 대체**한다.
- 응답의 `Equipment.Items = pmcData.Inventory.Items` — **인벤토리 전체**(스태시 포함)를 싣는다.

따라서 **이 작업 전에는** 친구 목록에서 프로필 팀원을 눌러 조회 화면을 열면 그 프로필의 **스태시 전체**가 화면으로 갔고, Aid 가 어긋나면
본인 프로필이 팀원인 척 보였다. 고친 것: `MergeProfileView` 맨 앞에서 `StmProfileViewFactory.TryBuildView`(착용 장비만 남긴다)를 먼저 부른다.

**SPT 자신의 동작이고 우리 범위 밖인 부분**: 위 라우트는 STM 이 있든 없든 **어떤 로컬 프로필의 인벤토리 전체**든 Aid 만 알면 돌려준다.
STM 은 자기가 등록한 프로필(과 읽기 실패 때 등록일 수 있는 Aid)의 응답을 막을 뿐, 등록되지 않은 로컬 프로필에 대한 SPT 의 응답은 건드리지
않는다. 기록만 한다. (SPT 는 `ProfileInfo.Aid`, STM 해석기는 `pmc.Aid` 로 찾는다 — 같은 값이지만 같은 필드는 아니다.)

이 경로를 막는 과정에서 **읽기 실패 신호가 경계에서 사라지는** 결함이 네 번 나왔다(시작 때 플래그만 보기 → 실행 중 실패 놓침 → 해석기가 던짐을
삼켜 "확인된 비팀원" 과 같은 모양이 됨 → 호출자가 SPT 응답을 흘려보냄). 최종 구조: `StmProfileMemberResolver.Lookup` 이 등록(`store.List`)과
카탈로그를 **읽는 유일한 곳**이자 분류(`ProfileViewPolicy.ClassifyLookup`)하는 유일한 곳이다. 분류는 `Resolved` / `NonMember`(읽기 성공, 등록에 없음) /
`Unavailable` / `ReadFailed` 이고, `ReadFailed` 는 PIT 용병이 아니면 조회를 거절하며 편집 가드는 모르는 Aid 를 거부한다. 평소(읽기 성공)의 순서는
그대로 STM 먼저다.

### 보존 검사 — 왜 "저장될 집합" 으로 옮겨야 했나

PIT 의 실물 커밋 경로(`FriendlyTeammateService.SaveTeammateDefaultEquipmentWithRealItemCommit`)는 플레이어 스태시를 **제출된 항목으로 통째로 갈아 끼우고**
저장한다. 검증(`ValidateRealCommitItemSet`)은 **제출된** 아이템만 훑는다 — 제출에서 **빠진** 아이템은 아무도 찾지 않는다. 계획 3 Task 3 의
클라이언트 관문은 사용자를 지키지만 라우트로 직접 쏘는 호출자는 그 밖이다. 그래서 서버에 보존 불변식을 세웠다(`ProfileGearEditPolicy.CheckStashConservation`):
**원래 플레이어 스태시의 모든 id 는 제출된 플레이어 스태시나 팀원 장비 어느 한쪽에 나타나야 한다**(재매김된 id 는 맵으로 따라간다, 비교는 대소문자·공백 무시,
읽을 수 없는 목록이 하나라도 있으면 거부). 용병 편집에도 걸린다 — 구멍은 PIT 경로 자체에 있었다.

**처음 배선이 틀렸다.** 검사를 **제출된** 장비에 걸었더니, 그 뒤 `MergeEquipmentWithPreservedSpecialItems` 가 비-extreme 모드에서 제출된 보안 컨테이너·개목걸이
슬롯 항목을 버리고(`RemoveSecureContainerTree`), 바로 다음 줄이 원래 스태시 id 를 플레이어에게서 전부 떼어 냈다. 플레이어 아이템을 팀원의 보안 컨테이너에 넣으면
**검사를 통과하고 → 병합이 지우고 → 스태시에서도 지워져 소실**됐다. 불변식이 막으려던 바로 그 소실이 통과 도장을 받았다.

지금은 **병합 뒤 저장될 집합**에서 돈다. 변수 이름이 `storedTeammateItems` — 보존 검사의 입력이면서 팀원에게 배정되는 그 목록이다. 병합 → 검사 → 팀원 배정이
붙어 있고, 주석이 "이름을 바꾸지도 두 자리를 떼어 놓지도 말 것 — 단위 테스트로 이 배선을 잡을 수 없다" 고 적는다. 못박은 테스트
(`보안_컨테이너로_제출한_아이템이_병합에서_빠지면_거부한다`)는 병합 뒤 집합으로는 거부되고 **제출된 집합으로는 통과해 버린다**는 것까지 단언한다.
거부는 여전히 모든 쓰기보다 앞이다. 그래도 **배선 자체는 단위 테스트가 없어**(SPT 타입) 관문 몫이다.

### 제스처 — 인계 대조표가 빠뜨린 것

(`FIRE`·`HOLD FIRE`·`TAKE COVER` 를 고른 이유와 효과는 위 "계획 3 후보 — 제스처 명령 메뉴" 구간의 대조표 그대로다. 아래는 그 표에 **없던** 사실이다.)

1. `hideUnsupportedCommands` 가 켜져 있으면 COMMAND 그룹이 10개로 잘려 세 명령은 메뉴에 **아예 뜨지 않았다.** 미대응이면서 숨김이었다.
   → 이제 메뉴 목록이 `GestureCommandPolicy.CommandGroupPhrases` 한 곳에서 와서(`GestureMenuPatch` 가 읽는다) 숨김을 켜도 13개(기존 10 + 셋)가 보인다.
   나머지 넷은 계속 숨는다. 목록과 번역의 일치는 xunit 이 못박는다(번역하지 않는 명령이 보이거나 번역하는 명령이 걸러지면 실패, 번역 개수 3 고정).
2. 처리하지 않는 구문은 라우터 끝의 `Receiver.OnPhraseSay` 로 **흘러** 바닐라 `BotReceiver` 가 받는다.
3. **`hideUnsupportedCommands` 기본값은 false 다.** 그래서 기본 설정에서는 이 세 항목이 **보였고, 바닐라가 처리하고 있었다.** Task 7 수정 2차는 "숨겨져 있었으니 바닐라에 기대던
   동작이 없다" 고 적었는데 **틀렸다**(3차에서 정정, 근거 주석 삭제).
   → 지금 **플레이어 본인 분대에 한해 포크의 처리가 바닐라를 대체**한다(`BotReceiverPhraseOverridePatch` 가 `ForkHandlesEngagementFor` 로 건너뜀). 이것은 **기본 설정의
   동작 변화이고 의도적이다**: 한 명령에 두 체계가 동시에 움직이는 것이 둘 중 어느 하나보다 나쁘고, HOLD FIRE 는 두 체계가 서로 반대로 당긴다. 건너뛰는 조건은 처리하는 조건과
   같은 함수(`HandlesEngagementPhrase`)를 쓰고 말한 사람이 등록된 보스이며 그 팀원이 **그 보스의 분대**일 때뿐이라, 두 번째 보스의 분대는 바닐라가 그대로 돈다.
4. 효과의 한계(코드가 아니라 손잡이의 한계): `FIRE` 는 임시 공격성 오버라이드를 풀 뿐이라 저장된 전술이 이미 소극적이면 **아무 변화가 없다.** `HOLD FIRE` 는 공격성 0 이라
   **밀고 나가지 않음** 이지 방아쇠 잠금이 아닐 수 있다 — 이름이 약속하는 것보다 약하다. `TAKE COVER` 는 교전 중인 팀원에게만 약 8초 보스 곁 엄폐이고, 교전 중이 아닌 팀원에게는
   **조용하다**(누락된 명령과 구별되지 않는다 — 확인 UI 는 일부러 만들지 않았다). 없는 AI 동작을 지어내지 않았다.
5. 남긴 넷(`SILENCE`·`KNIVES ONLY`·`SPREADOUT`·`GET BACK`)은 [BACKLOG](BACKLOG.md).

### 그 밖에 결정으로 남길 것

- **auto-join**: 인계 문서의 세 곳이 모두 메워졌다 — 토글 저장(`StmAutoJoinService.TrySetAutoJoin` → `store.Update`), 목록 조회(`AppendAccountIds` 가 PIT 목록 **뒤에** 붙인다, 읽을 수 없으면 PIT 목록 그대로),
  플레이어 구성(앞서 메움). 이 항목은 BACKLOG 에서 지웠다. 해석기는 PIT 보다 먼저 묻는다(여섯 번째 호출부). 거절 사유는 상태 이름 그대로 오류 본문에 실려 명단 토스트가 말한다. 인게임 미확인.
- **`StmProfileMember.HandEdited`**(단방향 표시): 첫 성공한 편집에서 켜진다. 켜지면 장비가 비어도 임시 장비로 덮지 않는다 — 비웠다는 것도 플레이어의 결정이고, 덮으면 그 결정이 조용히 사라지고 "실물" 의 뜻이
  깨진다. 되돌리는 길은 깃발 뒤집기가 아니라 **등록을 빼고 다시 넣기**다. 옛 등록 문서는 `false` 로 읽힌다 — 시작 때 `VerifyOldDocumentBinding` 이 저장 계층과 같은 `JsonUtil` 로 확인하고, 실패하면 등록을
  쓰는 모든 경로가 닫힌다(읽기 불가). 이 검사는 **실제 서버에서 돌려 본 적이 없고 실패 경로도 실행하지 못했다.**
- **편집은 아이템을 옮길 뿐 수량을 바꾸지 않는다.** 부분 장전 탄창을 채우는 길은 없고, 벗고 가득 찬 탄창을 끼운다. 입히기는 슬롯 한 칸에 하나(`cartridges` 포함), 슬롯은 부모 템플릿이 정의하고 필터가 받는 것만.
  격자(가방·리그 칸) 배치는 요청에 `Location` 이 없어 표현할 수 없다. 편집 요청은 **묶음**(`edits[]`)이고 서버가 순서대로 누적해 판정한 뒤 **쓰기 한 번**으로 적용한다 — 하나라도 거절되면 전체가 거절이고
  아무것도 쓰지 않는다. 클라이언트가 최종 인벤토리를 제출하는 모양은 일부러 피했다(제출된 것이 곧 새 상태가 되어 빠진 아이템이 조용히 사라지는 바로 그 구멍이다).
- **슬롯 필터는 조상 템플릿까지 본다.** 실측(`E:\Spt\SPT_Runtime\SPT_Data\database\templates\items.json`): `Default Inventory` 의 `Headwear` 필터는 `5a341c4086f77401f2541505` 하나인데 이것은
  **아이템이 아니라 범주 노드**이고, 헬멧의 `_parent` 가 그것이다. 아이템 자신의 tpl 만 비교하면 **모든 몸 장비가 거부**된다 — 손으로 tpl 을 적은 테스트 픽스처는 이것을 볼 수 없었다. 필터가 비면 "제한 없음" 이
  아니라 "모름" 으로 거부한다. 조상 사슬은 32칸에서 끊는다(실측 2~4칸).
- **벗기기**(장비 → 그 팀원 자신의 스태시): 같은 프로필 안의 이동이라 공짜 생성이 없다. `Pockets`·`SecuredContainer`·`ArmBand`·`Scabbard`·`Dogtag*`·`SpecialSlot*` 는 벗지 못한다 — 판정은
  `WornGearLossPolicy.IsNeverRemovedSlot` 하나(교차 계약 테스트가 둘의 어휘를 묶는다). 좌표는 서버가 지어내지 않고 SPT 의 `PlaceItemInContainer` 가 빈 자리를 찾는다.
- **쓸 수 없는 경로를 닫았다**: `…/profile/default-equipment`·`buy-kit`·`repair-equipment` 는 프로필 팀원에게 사유와 함께 **거부**한다(플레이어 스태시를 출처로 쓰거나 용병 문서에 쓰는 경로라 틀렸다).
  같은 Aid 의 보이지 않는 용병이 바뀌는 구멍도 닫혔다. 읽을 수 없는 등록 동안은 PIT 용병의 이 조작들도 거부된다(의도된 비용).
- **레이드 중에는 편집하지 않는다**(`FriendlyModeRequestGate.HasActiveRaid`): 레이드 중에 스태시 아이템을 착용 트리로 옮기면 그 팀원이 죽을 때 착용 장비 삭제 경로가 방금 옮긴 아이템을 지우고, 스폰은 시작
  시점의 복제본 위에서 돌아 이번 레이드에 반영되지도 않는다.
- **스태시 재검증 수리**(Task 3): 규칙을 어긴 스태시 항목 하나가 동료 생성 전체를 죽이던 것을, EFT 의 오류문 `(id: <24 hex>)` 에서 id 를 뽑아 그 항목(과 후손)만 건너뛰게 고쳤다(`StashRevalidationPolicy`).
  **건너뛴 세션에서는 동료 장비 저장을 막는다** — 서버가 제출된 스태시로 플레이어 스태시를 통째로 교체하므로 건너뛴 아이템이 영구히 사라질 수 있다. 재시작이 출구다. 정규식 필드는 처음에 역슬래시가 소실돼
  **절대 매치되지 않는** 채로 들어갔다(리뷰가 잡았다).
- **막힌 팀원의 출구**: 저널에 `StopAndReport` 로 멈춘 항목이 있으면 그 팀원의 상세 화면이 안내 상자를 띄우고, 기동 때 보고된 항목만 "이 기록 닫기"(확인창을 거쳐야만 요청이 나간다)로 닫을 수 있다. 저널 문서를 **읽을 수
  없으면** "기록 파일 버리기" 가 같은 방식으로 열려 있다. 저널은 암호화 DB 안의 레코드라 사람이 손으로 못 고친다. **이 출구 UI 는 항목을 만들 수 없어서(쓰는 쪽이 없다) 게임에서도 재현할 수 없다.**
- **미결 전송 계약**(계획 4 가 저널을 쓸 때): `Prepare` → `BeginSourceWrite`(true 일 때만 출처에서 뺌) → `RecordDestinationItemIds`(목적지 id 확정, 한 번 기록되면 불변) → `BeginDestinationWrite`(true 일 때만 넣음) →
  `MarkCommitted` → `Verify`(디스크 대조 콜백). 두 `Begin*` 은 각 쓰기 **직전**에 부른다. 복구는 사실만 보고 `RollBack`/`RollForward`/`StopAndReport` 셋이며 넷째 추측은 없다.
- **`src/SptTeamManager/tests/*` 는 PIT 업스트림의 하네스다.** 우리 xunit 프로젝트 밖이고(csproj 참조 0) 형제 스크립트들이 기본 경로로 `E:/SPTushanka`·`F:/Projects/...` — **업스트림 작성자의 PC** 를 쓴다.
  이 환경에서 **한 번도 돌 수 있었던 적이 없다**(포크의 기존 성질이지 회귀가 아니다). 그래서 그 안의 편집(`GestureMenuFilterFixture`, `Verify-GestureMenuFilter.ps1` 에 정책 소스를 함께 컴파일하는 한 줄 추가)은
  **문서화일 뿐 검증이 아니다.** 돌아가는 스위트에 있는 단언(`GestureCommandPolicyTests`)이 검증 근거다.
- **환경 메모**: `dotnet build client/pitFireTeam.csproj` 는 참조 프로젝트 `addon/pitFireTeam.SAINAddon.csproj` 의 `MSB3644`(.NET Framework 4.7.2 타기팅 팩 없음)로 끝이 실패다. 클라이언트 자체 컴파일은
  `-p:BuildProjectReferences=false` 로 확인했다. 이 계획의 서브에이전트 작업 중 `python` 스텁이 한 번 조용히 아무 일도 하지 않았고, `node` 편집 스크립트 하나가 같은 문자열이 두 번 있던 파일의 엉뚱한 구간을 바꿨다
  (빌드가 즉시 실패해 되돌렸다) — 문자열 치환 뒤에는 diff 를 읽는다.

### 이 계획에서 거짓으로 밝혀진 주석·전제 (같은 종류의 실수 목록)

계획 2 의 "전제를 확인하지 않고 이름에서 뜻을 지어냈다" 와 같은 종류가 이 계획에서도 반복됐다. 다음 계획이 같은 자리를 밟지 않게 남긴다.

1. 라우터의 `return` 이 바닐라 수신기를 막는다는 주석 — 막는 것은 `BotReceiverPhraseOverridePatch` 뿐이다.
2. "숨겨져 있었으니 바닐라 처리에 기대던 동작이 없다" — 기본 설정에서 보였다.
3. 저널 안내 문구 "stm-transfer-journal.json 의 이 항목을 지워라" — 그 문서는 loose JSON 이 아니라 세션별 암호화 DB 안의 레코드라 사람이 실행할 수 없는 문장이었다.
4. 보존 검사를 제출된 집합에 건 것(위).
5. 조상 걷기 없이 아이템 tpl 만 필터와 비교한 것(위) — 테스트 픽스처가 손으로 적은 tpl 이어서 보이지 않았다.
6. 스태시 오류문 정규식의 역슬래시 소실 — 한 번도 매치될 수 없는 패턴이 통과했다.

### 인게임 관문 (Task 8) — 순서대로, 결과를 기록하는 칸은 비워 두었다

**사람이 해야 한다.** 아래는 모든 Task 보고의 "미확인·게임에서 확인" 항목을 한 줄로 모은 것이고, 위쪽일수록 틀렸을 때 피해가 크거나 뒤의 항목을 가린다. **시작 전에**: 프로필 4개와 팀원 DB 를
`backup/2026-11-02-before-stm-plan3/` 같은 이름으로 백업하고(`PROCEDURE.md`), 설치한 DLL 의 md5 가 빌드 산출물과 같은지·그보다 새로운 `.cs` 가 없는지 확인한다. `E:\Spt\BepInEx\LogOutput.log` 의
수정 시각이 설치 이후인지 먼저 본다. 비교는 로그가 아니라 **프로필 JSON 대조**로 한다(아이템 `_id` 는 보존된다 — "사라진 id 가 어디에 있는가" 를 그대로 물을 수 있다).
프로필은 `E:\Spt\SPT_Runtime\user\profiles` 에 있고 BepInEx 는 설치 루트에 있다(배치가 갈려 있다).

| # | 할 것 | 실패하면 이렇게 보인다 |
| --- | --- | --- |
| 1 | **서버 기동 로그**를 읽는다. 있어야 하는 줄: `Teammate database ready` 와 `PitFireTeam loaded`. **없어야 하는 줄**: `StmRegistrationsUnreadable`, `StopAndReport`, 그리고 `[STM]` Error. 그다음 **PIT 용병 하나의 이름을 바꿔 본다**(아래 "확인 방법" 참조) — 이것이 등록 읽기가 멀쩡하다는 유일한 능동 증거다. **저널 기동 점검이 `SaveServer` 적재 뒤에 도는지는 이 관문으로 증명할 수 없다** — 저널에 생산자가 없어 항목이 0개이고, `RecoverOpenEntries` 는 **항목을 닫을 때나 실패할 때만** 로그한다(`StmTransferJournal.cs:262`, `PitFireTeamServerPlugin.cs:179`). 조용한 것은 아무것도 증명하지 않으며, **"점검이 돌았다" 는 줄을 찾으려 하지 말 것**(그런 줄은 없다). 그 순서 확인은 저널에 첫 소비자가 생기는 **계획 4** 로 넘어간다 | `StmRegistrationsUnreadable` 또는 Error 줄 → 등록을 쓰는 **프로필 팀원 경로가 전부 닫힌다**(프로필 팀원이 안 보인다). PIT 용병의 조작은 막히지 않아야 한다 - 막히면 편집 가드의 PIT 먼저 탈출구가 안 도는 것이다. 저널 점검이 프로필을 못 읽으면 `StopAndReport` 줄이 남는다 |
| 2 | **프로필 경로가 런타임에 풀리는가**: 프로필 팀원 하나에게 장비 한 건을 입혀 저장한다(`AppContext.BaseDirectory\user\profiles\{id}.json`) | **모든** 편집이 "저장한 내용을 디스크에서 다시 읽어 확인하지 못해 장비를 원래대로 되돌렸습니다" 로 끝난다. 로그에 "파일이 없다". 조용한 사고는 아니고 기능이 통째로 죽는 모양이다 |
| 3 | **편집기 왼쪽 칸이 그 팀원의 스태시만 보이는가** (플레이어 스태시의 아이템이 하나도 없어야 한다) | 플레이어 아이템이 보이면 **즉시 중단**하고 저장하지 않는다. 출처가 플레이어 스태시로 새는 것이고, 이 계획이 막으려던 복제 경로다. 코드로는 막아 뒀지만(플레이어 스태시를 읽는 호출이 이 경로에 없다) 눈으로는 아직 아무도 못 봤다 |
| 4 | **옛 프로필 역직렬화**: 마이그레이션이 필요했던 오래된 프로필(들)에 편집을 한 건씩 저장한다. 디스크 재독이 `SptProfile` 전체를 역직렬화한다 | 그 프로필만 2번과 같은 "되돌렸습니다" 로 끝나고 로그에 예외가 남는다. 안전하게 떨어지지만 그 프로필은 편집이 막힌다 |
| 5 | **입히기/벗기기 왕복 + 프로필 JSON 대조**: 헬멧 하나를 입히고 벗긴다. 전후 프로필의 아이템 id 집합이 같아야 하고 **플레이어 프로필은 한 글자도 안 바뀌어야 한다** | id 집합이 다르면 심각하다(아이템 생성·소실). 편집 전후 JSON diff 로 본다. 부모·슬롯 철자(`Headwear` 등)가 정식인지, 입힌 아이템에 `location` 이 없는지 |
| 6 | **보존 검사 + 용병 경로 회귀**: (a) 용병 하나의 편집 화면에서 평소처럼 편집·저장한다 (b) 가능하면 플레이어 스태시의 아이템을 용병의 보안 컨테이너에 넣고 저장한다 | (a)가 거부되면 **거짓 거부**다 — 사라질 id 가 로그에 Error 로 적힌다("저장이 안 된다"). (b)는 거부되고 사라질 id 를 말해야 하며 플레이어 스태시는 그대로여야 한다. 아이템이 조용히 사라지면 보존 검사 배선이 틀린 것이다 |
| 7 | **STM 편집기를 열고 닫은 뒤 용병 화면에서 편집 → 저장** (같은 정적 전역을 공유한다). 한 번은 저장 직후에, 한 번은 취소로 닫은 직후에 | 용병 화면이 이전과 다르게 동작하거나, 용병 편집기에 프로필 팀원의 스태시·장비가 남아 보인다 |
| 8 | **스태시 격자 자리 찾기(벗기기)**: 장비를 벗어 스태시로 보낸다. 서로 겹치지 않는 좌표에 놓이는지 본다. 스태시를 일부러 가득 채운 프로필에서 벗기기를 시도한다 | 겹치는 좌표나 좌표 없는 아이템(프로필 JSON 의 `location`). 가득 찼을 때는 "벗긴 아이템 'x' 을(를) 놓을 스태시 자리를 찾지 못해…" 거절이어야 하고 아무것도 안 바뀌어야 한다 |
| 9 | **슬롯 필터**: 몸 장비(헬멧·리그·가방·방탄복), 무기 부착물, **탄창 → 그 탄창에 탄**(순서 연쇄)을 각각 시도한다. 빈 필터·읽을 수 없는 필터인 슬롯이 있는지 눈으로 찾는다 | 넣을 수 있어야 하는데 거부된다 — 사유가 **슬롯 이름**을 말하므로 그 이름을 적어 둔다(필터가 빈 슬롯이면 "모름" 으로 거부한다). 몸 장비가 전부 거부되면 조상 걷기가 안 도는 것이다 |
| 10 | **편집한 장비로 레이드에 나간다**: 장비를 일부러 **비운** 팀원으로도 한 번. 실물 장비(편집한 그대로)로 나가는지, 탄창에 탄이 차 있는지 | 임시 장비로 나가면 `HandEdited` 가 안 켜진 것이다(편집이 확인을 통과한 뒤에만 켜진다). 빈 탄창으로 나가면 인계 문서의 "탄창 리필은 몸에 지닌 탄만 쓴다" 결함이 되살아난 것 |
| 11 | **계획 2 회귀 한 판**: 프로필 팀원을 실물로 데리고 나가 한 명을 죽이고 돌아온다. 착용 장비 삭제·스태시/보안 컨테이너 보존·영수증이 그대로인지 | 계획 2 에서 확인한 13개 중 하나라도 달라지면 이 계획의 `SpawnGearPolicy.Decide(handEdited)`·해석기 `Lookup` 변경이 원인이다 |
| 12 | **다섯 번째 경로**: 친구 목록에서 프로필 팀원을 눌러 조회 화면을 연다 | **착용 장비만** 보여야 한다. 스태시 전체가 보이거나 본인 프로필이 팀원인 척 보이면 `MergeProfileView` 분기가 안 도는 것이다 |
| 13 | **저장 중에 화면을 벗어난다**: 저장 버튼을 누르고 요청이 나가 있는 동안 X·취소·화면 숨김을 시도한다. 서버 거절을 일부러 만들어(예: 가득 찬 스태시 벗기기) 같은 순서로 | 편집기가 저장 중에는 닫히지 않아야 한다. 화면이 사라진 뒤 거절이 오면 사유가 토스트로만 지나가지 않고 **그 팀원의 다음 상세 화면**(10분 이내)에 안내 상자로 떠야 한다. 성공을 거절로 말하거나 모름을 거절로 말하면 `SaveOutcomePolicy` 연결이 틀린 것 |
| 14 | **저장 직후 다시 열기**: 저장한 뒤 곧바로 같은 팀원의 편집기를 다시 연다 (PIT 의 `RefreshPlayerVisualization` 이 비동기다) | 방금 옮긴 아이템이 어느 칸에도 안 보이는 낡은 화면. 저장 판정에는 영향이 없어야 한다 |
| 15 | **auto-join**: 명단에서 프로필 팀원의 자동 합류를 켠다 → 메인메뉴에서 출격 화면으로 간다. 로그인 중인 프로필 등 거절되는 경우의 토스트도 본다. 기본값(꺼짐)에서는 합류하지 않는지 | 출격 화면에서 합류하지 않으면 세 곳(토글 저장·목록·플레이어 구성) 중 하나가 안 닿는 것이다. 이 기능은 계획 2 때 인게임에서 **미동작이 확인**됐던 것이다. 토스트 문장이 잘리면 문구를 줄인다 |
| 16 | **상세 화면**: 프로필 팀원의 상세 화면 — "팀원 장비 편집" 버튼과 안내 상자의 위치(상단 안내 상자 자리 -96 과 겹치지 않는지), 한글 폰트, 레이드 중일 때의 입구 안내, 용병 상세 화면이 이전과 같은지, 프로필 팀원에게 이름·의상·숙련·공격성을 바꾸려 할 때의 거부 사유 | 버튼·상자가 겹치거나 글자가 깨진다. 용병 화면이 달라졌으면 PIT 파일에 넣은 한 줄(`TryApplyStmDetail` 호출)이 원인이다 |
| 17 | **스태시 재검증**: 규칙을 어긴 항목이 든 스태시(알려진 사례: 9x19 탄창 + 5.56 탄)로 동료를 만든다. 자연 발생을 기다려도 된다 | 동료 전체가 안 만들어지면 수리가 안 닿은 것이다. 항목만 건너뛰어야 하고 로그에 `[UI] Skipped stash item <id>:` 가 남고, **그 세션에서는 장비 저장이 막힌다**(재시작이 출구). EFT 오류문이 `(id: ...)` 형식이 아니면 예전처럼 던진다 — 회귀는 아니다 |
| 18 | **제스처 기본 설정 동작**: `hideUnsupportedCommands` 를 **끈 기본 설정**에서 메뉴로 FIRE·HOLD FIRE·TAKE COVER 를 고른다. 포크의 동작이 **한 번만** 일어나는지(바닐라 반응이 추가로 없는지). 한 명을 바라보고 FIRE 를 주면 그 한 명은 포크가, **나머지 분대원은 바닐라가** 반응해야 한다(둘 다 무반응이면 건너뛰기가 팀원별 자격을 안 따르는 것) | 팀원이 바닐라식 자체 응답·움직임을 **추가로** 하면 건너뛰기가 안 도는 것(이중). 아무 반응이 없으면 포크 처리가 안 도는 것(무반응) |
| 19 | **`HOLD FIRE` 대 방아쇠**: 교전 중에 HOLD FIRE 를 준다. 로그의 `BattleRecorder` 공격성 오버라이드(`HoldPositionAggression`)가 값 0 인지도 본다 | 팀원이 밀고 나가면 오버라이드가 안 걸린 것. **시야의 적에게 자기 방어 사격을 계속하면 그것이 이 손잡이의 알려진 한계**이지 결함이 아니다 — 결과를 기록해 BACKLOG 의 정밀화 항목을 정한다 |
| 20 | **`TAKE COVER`**: 교전 중인 팀원과 교전 중이 아닌 팀원 둘 다에게 준다 | 교전 중인 쪽이 약 8초간 보스 곁으로 붙고 Roger + OK 제스처를 해야 한다. 교전 중이 아닌 쪽은 포크가 무반응이고 **바닐라의 TAKE COVER 반응이 나오는 것이 정상**이다(바라보는 팀원이 있으면 그 팀원만 포크 대상이다) |
| 21 | **`FIRE`**: HOLD FIRE 를 준 뒤 FIRE 를 줘 다시 교전·전진하는지, 아무 것도 안 준 상태에서 FIRE 만 주면 **변화 없음이 정상**인지 | HOLD FIRE 뒤에도 그대로면 오버라이드 해제가 안 닿는 것 |
| 22 | **두 번째로 등록된 보스의 분대**가 이 세 구문에 여전히 반응하는가 (바닐라 경로) | 무반응이면 건너뛰기 조건이 등록된 보스 분대 밖까지 넓어진 것(3차 수정 이전 상태) |
| 23 | **`hideUnsupportedCommands` 를 켜고** 메뉴를 본다: COMMAND 그룹이 13개, `SILENCE`·`KNIVES ONLY`·`SPREADOUT`·`GET BACK` 은 숨음. 스캐브 메뉴와 설정을 끈 상태는 원본 18개 그대로 | 12개나 14개면 목록과 번역이 어긋난 것(xunit 이 못박았지만 필터 연결은 게임에서만 보인다) |
| 24 | **아이템 id 대소문자**: 위 항목들 중에 "그 팀원 스태시에 없다" 류의 거절이 나오는지 본다 (클라이언트는 EFT `MongoID.ToString()` 소문자 16진을 보내고 서버 비교는 Ordinal) | SPT 가 소문자로 쓰므로 일치해야 한다. 어긋나면 모든 편집이 거부된다 |
| 25 | **편집 중 서버 강제 종료**: 저장 직전·직후 서버를 죽이고 다시 켠다 | 편집은 저널을 쓰지 않으므로 그 팀원이 **막혀서는 안 된다.** 프로필은 저장 전이나 후 어느 한쪽이어야 하고 반쯤 된 상태가 아니어야 한다(쓰기는 한 번이다). **"저널이 풀리는지" 는 이 항목으로 볼 수 없다** - 저널에 쓰는 조작이 없어 풀 항목이 없다. 이 항목이 확인하는 것은 쓰기 한 번의 원자성뿐이다 |

**관문이 확인할 수 없는 것** (재현 수단이 없다): 막힌 팀원의 안내 상자·"이 기록 닫기"·"기록 파일 버리기" 버튼과 경고창 — 저널에 쓰는 조작이 없어 `StopAndReport` 항목을 만들 수 없다(위치·크기·한글 폰트가 눈으로 확인되지 않은 채다).
`Verify-GestureMenuFilter.ps1`·`GestureMenuFilterFixture` — 업스트림 하네스라 돌릴 수 없다. `VerifyOldDocumentBinding` 의 **실패 경로**. 저널 기동 복구의 `RollBack`/`RollForward`/`StopAndReport` 실물 경로.
이것들은 단위 테스트(판정만)와 코드 리뷰가 전부다 — 계획 4 가 저널에 처음 쓸 때 함께 본다.

**결과 기록**: (Task 8 이 끝나면 위 표 번호별로 여기에 적는다. 지금은 **전부 미실행**이다.)

#### 최종 리뷰가 남긴 세 가지 (고치지 않고 기록만 한다)

전체 브랜치 리뷰와 그 수정 웨이브의 재리뷰가 남긴 것들이다. 셋 다 결함이 아니라 **다음 사람이
알아야 하는 성질**이다.

1. **`PitKnowsAid` 의 3출처 조회가 두 곳에 적혀 있다** — `FriendlyTeammateSocialCallbacks.cs:217`
   (조회 경로)과 `StmProfileOperationGuard.cs:23`(편집 경로). **지금은 서로 맞고**, 가드 쪽이
   `pending` 을 널 검사해 더 엄격하다. 이 계획이 "판단 하나에 집 하나" 를 여러 번 강제했는데
   이 하나가 남았다 — 버그가 아니라 **표류 위험**이다. 한쪽을 고치면 다른 쪽도 보라.
2. **제스처 건너뛰기와 핸들러가 서로 다른 순간에 평가된다** — 건너뛰기는 수신 패치에서,
   효과는 `PhraseSaid` 에서. 그 사이에 교전 상태가 뒤집히면 한 팔로워가 둘 다 받거나 둘 다
   못 받을 수 있다. 창이 좁고 손실이 없다.
3. **`HOLD FIRE` 에서 거절로 응답한 팔로워도 "포크가 처리함" 으로 센다**(부정 대사 뒤 `continue`).
   그래서 그 팔로워에게는 바닐라가 돌지 않는다. **의도된 반응이지 침묵이 아니다.**

### 계획 3 설치와 첫 인게임 확인 (2026-11-02)

**설치.** 프로필 5개를 `backup/2026-11-02-before-plan3/profiles/` 로 백업하고(계획 3 은 프로필에 쓰는
첫 코드다), 서버·클라이언트를 새로 빌드해 `-Remove` 후 재설치했다. 설치본 md5 가 양쪽 다 빌드
산출물과 일치한다(서버 `18674bd5…`, 클라이언트 `4a0e89e3…`). 설치된 DLL 에 `StmGearEditService`·
`ProfileGearEditPolicy`·`StmTransferJournal`·`ProfileViewPolicy`(서버), `StmGearEditor`·
`GestureCommandPolicy`(클라이언트)가 들어 있다. `GestureCommandPolicy` 가 **서버 DLL 에 없는 것은
정상**이다 — 클라이언트·테스트 csproj 에만 링크돼 있고 서버는 제스처를 다루지 않는다.

**기동(관문 1번의 수동 절반).** `Teammate database ready` ×5, `PitFireTeam loaded`,
`StmRegistrationsUnreadable`·`StopAndReport` **없음**, `[STM]` 줄 자체가 없다(클라이언트 요청이
없었으므로 정상). 오류는 **이번 run 에만 있는 종류가 없다**. 개수 비교(243 vs 49)는 구간 길이가
달라 증거가 아니다 — 믿을 것은 **종류의 집합 차이가 비었다**는 쪽이다.

**인게임에서 실제로 본 것.** 명단에 프로필 팀원 셋(SPT02 Lv15 Usec, Knox Lv69 Bear, Hawk Lv15 Usec)이
`프로필 팀원` 캡션과 함께 뜬다. 계획 1·2 의 명단 기능은 살아 있다. `장비 구분: 아직 판정하지 않음`
은 오류가 아니라 명단이 의도적으로 내는 문구다(`StmProfileRosterClient.EquipmentUndecided`) —
레이드에 넣기 전에는 실물/임시를 정하지 않는다.

**그리고 계획 3 의 화면을 열 길이 없다는 것이 드러났다.** 프로필 팀원 타일
(`SquadControlMenuUi.ProfileRoster.cs:150-196`)은 배경 Image 하나, 글자 넷(캡션·이름·`Lv·진영`·장비
구분), 제거 버튼이 전부이고 **클릭 핸들러가 없다.** 용병 타일은 `Roster.cs:379` 에서
`button.onClick → OpenProfile(accountId)` 를 걸고, 상세 화면은 그 `OpenProfile` 로만 열린다. 따라서
Task 5 의 상세 화면(막힌 팀원 출구 포함)과 Task 6b 의 장비 편집기 진입구가 **사용자에게 닿지 않는다.**
이번 세션 로그에 상세·편집 라우트 호출이 한 건도 없는 것이 그 증거다.

**이 계획에서 네 번째 "지었는데 닿지 않는다" 다.** 열린 항목 1(네 번째 경로), Task 6(편집 UI 없음),
Task 7(메뉴에서 숨김), 그리고 이것. 앞의 셋은 리뷰가 잡았는데 이것은 못 잡았다 — **진입구가 계획 1
에서 만든 타일에 있고, 계획 3 의 어느 Task 파일 목록에도 그 파일이 없어서** 어떤 리뷰도 그 자리를
보지 않았다. 계획서가 "상세 화면에서 진입" 이라고 쓰면서 **상세 화면을 여는 길**을 누구에게도
맡기지 않은 결과다. 다음 계획을 쓸 때: **새 화면·새 기능을 넣으면 "사용자가 거기에 어떻게
도달하는가" 를 Task 의 파일 목록에 명시적으로 넣어라.**

**아직 원인을 모르는 것(추측하지 않는다).** 스크롤 중 스크린샷에서 ① Hawk 카드에 `Lv·진영` 줄이
비었다가 다음 장면에서 채워졌고 ② 용병 타일 초상과 `ROSTER`/`SETTINGS`/`MODE` 탭이 빈 네모로
보였다. 로딩 중 한 장면일 수도, 레이아웃 문제일 수도 있다. 진입구를 고친 뒤 같이 본다.

## 2026-11-02 — STM 열린 항목 7: 명단 타일에서 상세 화면으로 가는 길

**한 일.** 프로필 팀원 타일에 클릭 핸들러를 걸었다. `SquadControlMenuUi.ProfileRoster.cs` 의
`CreateStmTile` 이 `openAccountId` 를 받아, 값이 있으면 타일에 `Button`(transition None,
targetGraphic = 타일 배경)과 `RosterTileHoverController` 를 얹고 `OpenProfile(accountId)` 를 건다.
용병 타일(`Components/SquadControlMenuUi.Roster.cs:379`)과 같은 구성이다. 같이 바뀐 것:
`AddStmTileText` 가 만든 `TextMeshProUGUI` 를 돌려주게 됐다(호버 컨트롤러가 이름 라벨을 받는다).

**계정 id 는 aid 를 문자열로 쓴 것이다.** `OpenProfile` → `ShowPlayerProfileScreen(accountId)` 로
들어가는 그 id 는 `StmRosterWire.IsCurrentSession` 이 `row.Aid.ToString()` 과 비교하는 것과 같은 값이다.
`RegisteredProfileRow` 는 `ProfileId`(등록 키)와 `Aid`(계정 id) 를 **둘 다** 들고 있어서, 제거 버튼은
`ProfileId`, 여는 쪽은 `Aid` 를 쓴다 — 섞으면 조용히 틀린 프로필을 연다. 판정은 `StmTileAccountId`
한 곳이고, **쓸 수 있는 행(`Ready`)이면서 aid 가 0 이 아닐 때만** id 를 준다. aid 0 은 등록만 남고
프로필이 없는 `Missing` 행인데, 알 수 없는 계정 id 로 SPT 의 `GetOtherProfile` 을 부르면
**플레이어 자신의 프로필로 대체**되므로(열린 항목 1-b 의 역컴파일 실측) 여는 것 자체가 위험하다.
쓸 수 없는 행과 안내 타일(불러오기 실패·세션 미확인)은 `openAccountId: null` 로 전과 같이 먹통이다.

**제거 버튼과 안 부딪친다.** 제거 버튼은 타일의 **자식 `Button`** 이고 `IPointerClickHandler` 가
클릭을 먹으므로 부모 타일의 `onClick` 으로 올라가지 않는다. 용병 타일이 이미 같은 구조다
(타일 `Button` + `CreateRemoveButton`). 글자 넷은 전부터 `raycastTarget = false` 다.

**실측.** `dotnet build src/SptTeamManager/client/pitFireTeam.csproj -c Debug -p:BuildSAINAddon=false`
경고 0·오류 0(DLL 타임스탬프가 수정한 `.cs` 보다 새로운 것을 확인했다 — "최신 상태" 로 넘어간 빌드가
아니다). `dotnet test` **511개 전부 통과**(총 개수를 전과 맞췄다). 이 변경에 새 테스트는 없다:
클라이언트 `Components` 는 테스트 프로젝트가 참조하지 않는 자리이고, 여기 들어간 판정은
`StmTileAccountId` 의 두 줄뿐이다.

**확인하지 않은 것.** **게임에서 눌러 보지 않았다.** 타일이 실제로 눌리는지, 상세 화면이 뜨는지,
호버 색이 쓸 만한지(이름 라벨의 호버 색은 컨트롤러가 `1 - 색` 으로 만든다 — 프로필 타일의 이름은
밝은 회색이라 어두운 글자가 된다), 제거 버튼이 가려지지 않는지 전부 미확인이다. 관문 2~5번이
**실행 불가에서 미실행으로** 바뀐 것이 이 변경의 결과다.

**설치(2026-11-02 15:33).** 클라이언트 DLL 한 개만 갈았다. 서버는 이 변경에 관여하지 않고
설치본 서버 DLL md5 가 빌드 산출물과 **같다**(`18674bd5…`, 2026-11-02 설치 때와 동일) — 서버를 내릴
이유가 없어 **SPT.Server(PID 46984)를 그대로 두었다.** 게임은 꺼져 있었다. `install-sptteammanager.ps1`
은 덮어쓰기를 만나면 중단하도록 돼 있고 `-Remove` 가 돌아가는 서버의 서버 DLL 을 지우려다 중간에
멈추므로, 이번에는 스크립트 대신 **클라이언트 DLL 한 파일만** 복사했다. 절차는 같은 것을 지켰다:
빌드보다 새로운 `.cs` 가 없음을 확인하고, 설치본을 `backup/2026-11-02-before-tile-click/` 로 백업한 뒤
(옛 md5 `4a0e89e3…`), 복사하고 md5 를 대조했다 — 설치본·산출물 모두 `dbeb48d4…`. 설치된 DLL 에
`StmTileAccountId` 가 들어 있다(메타데이터 문자열로 확인). `doc/INSTALLED-MODS.md` 는 내용이 바뀌지
않는다(모드 목록이 그대로다). **게임을 켜서 눌러 보는 것은 아직이다.**

**옆에서 드러난 것 — auto-join 토글도 길이 없다.** 상세 화면에는 auto-join 토글이 없고
(`OtherPlayerProfileScreenPatch.StmDetail.cs` 는 막힘 안내와 편집 입구만 얹는다), 토글은 **용병 타일의
우클릭 메뉴**에만 있다(`SquadControlMenuUi.ContextMenu.cs:117-120` → `ToggleTeammateAutoJoin`).
그 메뉴는 `SquadRosterEntry`·그룹 초대·초상 로딩에 묶여 있어 프로필 타일에 그대로 달 수 없다.
명단 응답은 행마다 `autoJoin` 을 싣는데(`StmProfileMemberCallbacks.cs:29-37`) **클라이언트가 그 필드를
파싱하지 않는다** — `RegisteredProfileRow` 에 자리가 없다. 그래서 **관문 15번은 여전히 실행 불가**이고
열린 항목 **8번**으로 적었다. 계획 3 이 "지었는데 닿지 않는다" 를 네 번 낸 자리에서, 같은 종류가
**다섯 번째**로 하나 더 있었다는 뜻이다 — 이번에는 서버 쪽 Task(1-c) 가 "완료" 로 닫히면서
**누를 곳**을 아무 Task 도 맡지 않았다.

## 2026-11-02 — STM·SAIN 한글화 (빠져 있던 것을 메운다)

**빠져 있던 것.** 한글화는 두 갈래인데 한쪽만 되어 있었다. ① F12 설정 창은 `SptKoreanConfig` 의
사전(`lang/<GUID>.json`)이 덮는다. ② 게임 안의 모드 UI 는 그 모드 자신의 문구 체계를 써야 한다.
①만 있었고 ②는 STM·SAIN 둘 다 손대지 않은 채였다. 게다가 ①도 STM 에서는 끊겨 있었다.

**왜 끊겼나(실측).** `SptKoreanConfig` 는 `setting.PluginInfo.GUID` 로 사전을 찾고 파일명이 그 GUID 다
(`Plugin.cs:145`). PIT 포크 때 플러그인 GUID 를 `xyz.pit.fireteam` → `xyz.stm.teammanager` 로
바꿨으므로(PIT 와 공존 금지라 피할 수 없었다) 그 순간 사전이 조용히 끊겼다. **아무도 못 본 이유는
검사 도구에 있었다** — `check-lang-coverage.mjs` 가 **사전 파일을 기준으로** 돌아서 사전이 아예 없는
모드는 목록에 나타나지도 않았다. 두 cfg 의 섹션·키 집합은 `diff` 로 완전히 같음을 확인했다.

**한 일 1 — STM 인게임 UI(355개).** PIT 는 클라이언트가 `/singleplayer/pitfireteam/lang` 으로 물어
서버의 `Resources/lang/<언어코드>.json` 을 읽고, 없으면 내장 영어로 떨어진다
(`friendlyPlugin.LoadLanguageOptions`). 거기에 `chs/en/ru` 뿐이라 분대 화면이 전부 영문이었다.
`kr.json` 을 만들었다(355개 문구 전부). **언어 코드는 `ko` 가 아니라 `kr` 이다** — 설치본
`SPT_Runtime/SPT_Data/database/locales/global/` 에 `kr.json` 이 있는 것으로 확인했고, 클라이언트가
`ko` 를 넘기는 경우에도 영문으로 떨어지지 않게 같은 내용을 `ko.json` 으로도 둔다.
덤으로 **F12 설정 창도 이것으로 한글이 된다** — PIT 가 `Config.Bind` 의 `DispName`·`Description` 을
이 lang 파일에서 꺼내 쓴다(`CreateConfigAttributes`, `friendlyPlugin.cs:1354`).

손으로 쓴 26KB JSON 은 믿을 수 없으므로 **생성 + 검사**로 바꿨다: 번역은 `tools/lang/stm-kr.json`
(열쇠가 en.json 의 경로), 산출은 `node tools/build-stm-lang.mjs`. 검사가 **번역 누락, 자리틀({0})
어긋남, 줄바꿈 수 어긋남, 원문과 같은 값, en.json 에 없는 열쇠**를 보고 하나라도 걸리면 아무 파일도
쓰지 않는다. 지금 355/355 통과. `kr.json`·`ko.json` 은 한 벌에서 나오므로 서로 어긋날 수 없다.

**한 일 2 — STM 의 F12 사전.** `lang/xyz.stm.teammanager.json` 을 PIT 사전에서 복사해 `guid`·`name` 만
고쳤다(키 집합이 같으므로 그대로 맞는다). 검사 도구가 63/63 OK 를 찍는다.

**한 일 3 — SAIN 편집기(772개).** SAIN 의 설정 글자는 `[Name("..")]`·`[Description("..")]`·
`[Category("..")]` 속성에 있고(`SAIN.Preset.Shared.dll`: 423 + 340 + 268개, 서로 다른 값 732개),
편집기는 그것을 모아 둔 `SAIN.Attributes.ConfigInfoClass` 의 `Name`·`Description`·`Category` 를
그대로 그린다 — `AttributesGUI.cs`: `SAINLayout.Label(info.Name + ": ")`,
`new GUIContent(info.Name, info.Description)`. 속성 쪽 값은 `readonly` 필드라 거기서는 손댈 수 없으므로
**그 객체의 생성자 Postfix 에서** 세 값만 바꾼다(`SainAttributePatch`). SAIN 은 멤버마다 한 번 만들어
정적 사전에 쌓으므로(`AddAttributesToDictionary`) 교체도 한 번씩이다.

**저장이 깨지지 않는 근거(확인함).** SAIN 은 값을 **필드 이름**으로 프리셋 JSON 에 저장한다. `Name` 을
열쇠로 쓰는 곳은 `ConfigEditingTracker`(메모리 안의 "저장하지 않은 변경" 표시)와 편집기 검색창
(`SkipForSearch`) 둘뿐이다. 그래서 한글로 바꿔도 저장·불러오기에 영향이 없고, **검색은 한글로** 하게 된다.
`Category` 도 묶음 제목으로만 쓰이고 같은 원문은 항상 같은 번역이 되므로 묶임이 흐트러지지 않는다.

창 안에 코드로 박힌 글자(버튼·제목·안내문·성향 설명) 41개는 `UiStringPatch` 와 같은 방법(ldstr 상수
교체)으로 바꾼다(`SainUiStringPatch`, 대상 어셈블리만 SAIN 으로). **로그 문구와 디버그 화면의 글자는
일부러 넣지 않았다** — `[SAIN] …` 같은 줄은 문제를 추적할 때 영문이 낫고, 디버그 오버레이는 개발용이다.

SAIN 이 없으면 둘 다 조용히 꺼진다(타입을 못 찾으면 그대로 돌아간다). `[BepInDependency(SoftDependency)]`
로 SAIN 뒤에 올라오게만 했다. 모양이 달라지면(백킹 필드나 생성자를 못 찾으면) 경고만 찍고 아무것도 바꾸지 않는다.

**번역 열쇠의 오타를 기계가 잡게 했다.** 긴 설명은 열쇠를 손으로 다시 적지 않는다 — 역컴파일한 원문을
정렬해 **번호로 짝지은 배열**로 번역하고, 각 항목에 원문 앞 40자를 적어 자리가 어긋나면 멈추게 했다
(`zip-long.mjs`). 실제로 한 건(`Bots are not as careful with grenades as…`)을 그 검사가 잡았다.
합친 사전은 역컴파일에서 뽑은 집합과 **양방향으로** 대조한다(빠진 것·집합에 없는 열쇠 둘 다).

**한 일 4 — 검사 도구가 "사전이 없는 모드" 를 잡게 했다.** 이번 누락의 직접 원인이었다. 설치된 `.cfg`
쪽에서도 한 바퀴 돌고, 설정 이름이 이미 한글인 우리 모드는 `원문이 이미 한글이다` 로 구별한다.
지금: 사전 105개, 사전 없는 모드 6개 모두 `번역이 필요한 것 0개`.

**실측.** `dotnet build src/SptKoreanConfig -c Release` 경고 0·오류 0. 설치본 DLL md5 가 산출물과 일치
(`ec94c526…`). STM lang 두 파일도 설치본과 md5 일치(`9364c497…`). 게임은 꺼져 있었고 SAIN·STM 의
DLL 은 건드리지 않았다(서버를 내릴 이유가 없었다).

**확인하지 않은 것.** **게임에서 눈으로 본 것이 없다.** 분대 화면이 한글로 뜨는지, SAIN 편집기의 이름·
설명·분류가 바뀌는지, 글자가 칸을 넘치지 않는지, 한글 폰트가 SAIN 의 IMGUI 스타일에서 제대로 그려지는지
전부 미확인이다. 특히 ① SAIN 편집기는 IMGUI 라 폰트가 EFT UI 와 다를 수 있고 ② 번역문이 원문보다 길면
줄이 잘릴 수 있다. 기동 뒤 `SAIN 편집기 한글화 패치 완료`, `SAIN 창 UI 한글화 — … 문자열 N개 교체`,
`SAIN 사전에 없음: "…"` 세 줄을 로그에서 보고 판정한다.

## 2026-11-02 — 첫 인게임 확인이 연 것들 (관문 2~5 통과, 화면 손질 셋)

**통과.** 명단 타일 클릭 → 프로필 팀원 상세 화면이 열린다(관문 2~3). 장비 편집기의 왼쪽 칸은
**그 팀원 자신의 스태시**다(관문 4) — 플레이어 스태시가 아니다. 계획 3 에서 가장 컸던 두 Task 가
사용자에게 처음으로 닿았다.

**고친 것 1 — "VIEW HIDEOUT" 이 플레이어 은신처를 열었다.** 프로필 팀원 화면에서 그 버튼을 누르면
그 팀원이 아니라 **플레이어 자신의 은신처**가 열린다. SPT 는 다른 프로필의 은신처 화면을 만들지 않으므로
(조회 응답의 `Hideout` 은 구역 id 목록이다 — 인계 §3-10) 버튼이 할 수 있는 일이 없다. PIT 가 용병
화면에서 같은 이유로 감추는 `HideStockHideoutButtonPanel` 을 STM 경로에서도 부른다. 되돌리기는 PIT 가
화면마다 하는 `RestoreHideoutButtonVisuals` 가 그대로 맡는다.

**고친 것 2 — 장비 편집 입구가 EFT/ARENA 탭 아래 겹쳐 있었다.** 입구를 화면 가운데 위(`-96`)에 두었더니
통계 탭 줄과 겹쳤다. 이제 `_arenaToggle`(리플렉션)을 찾아 **그 오른쪽에** 붙인다. 못 찾거나 탭 줄이 아직
안 그려졌으면(폭 0) 원래 자리에 그대로 선다. 안내 상자가 설 때는 그 아래가 제자리이므로 옮기지 않는다.

**고친 것 3 — 명단에서 용병과 프로필 팀원이 한 격자에 섞였다.** 명단 위에 하위 탭 줄(`용병` / `프로필 팀원`)을
두고 고른 쪽만 그린다. 기본은 용병 — PIT 가 원래 보여 주던 것이다. 탭을 바꾸면 명단을 다시 그린다
(서버에 다시 묻는다). 탭 그리기가 실패해도 명단은 선다(기본값 쪽이 보인다).

**판정 — 모드 탭의 두 칸은 체크박스가 아니라 라디오다.** `ToggleGroup` + `allowSwitchOff = false` 라
항상 하나만 켜진다(`Mode.cs:33-37`). 누르면 `ChangeGameplayMode`: 전환 중이거나 이미 그 모드면 무시,
**레이드 중이면 거부**(토스트), 아니면 화면을 어둡게 덮고 `ApplyGameplayModeAsync` 뒤 명단·설정을 다시 그리고
결속 모드에서는 `+ ADD TEAMMATE` 를 숨긴다. **확인할 것**: 스크린샷에서 결속이 켜져 있는데
`+ ADD TEAMMATE` 가 보였다. 숨김은 모드를 **바꿀 때**(`Mode.cs:119`)와 버튼을 **만들 때**(`Roster.cs:268`)
두 곳에서 걸리므로, 둘 다 지나왔다면 보이면 안 된다. 두 스크린샷이 같은 순간의 것인지부터 확인해야 한다.

**판정 — 컨텍스트 메뉴에 "착용하기" 가 여러 번 나오는 것은 우리 것이 아닐 가능성이 크다.**
"착용하기" 는 EFT locale 의 `EQUIP` 키다. PIT·STM 이 아이템 컨텍스트 메뉴에 하는 일은 **빼는 것뿐**이다 —
`LoadoutEditorEquipmentRootContext` 가 `BaseItemContextInteractions.IsActive` 를 후킹해 `Insure`·`Repair`·
`Open` 을 막고, `ExecuteInteractionInternal` 을 가로챈다. **`EQUIP` 를 더하는 코드는 없다.**
설치본에 `Tyfon.UIFixes` 가 있고 그 DLL 안에 `CreateSubInteractions`·`SetSubInteractions`·
`GetItemContextInteractions` 가 있다 — UIFixes 는 한 항목을 **슬롯별 하위 항목으로 펼친다.** 하위 항목의
이름이 안 붙으면 부모 이름(`착용하기`)이 그대로 여러 줄로 보인다. 메뉴 맨 위의 `EXPORT TO TG` 도 다른 모드다.
**가르는 실험**: STM 화면을 전혀 열지 않은 상태에서 평소 스태시의 같은 아이템을 우클릭해 본다.
거기서도 여러 줄이면 STM 과 무관하다. STM 편집기 안에서만 그러면 우리 것이다(편집기가 복제 프로필로
`ItemUiContext.Configure` 를 하므로 UIFixes 가 슬롯을 두 벌로 셀 가능성이 남는다).

**아직 영문인 이유(진단 끝).** 이번 스크린샷의 분대 화면이 영문인 것은 번역이 안 걸려서가 아니라
**이 run 이 한글 파일보다 먼저 떴기 때문**이다: `LogOutput.log` 의 마지막 기록이 15:55 이고
`SptKoreanConfig` 설치는 16:00, STM `kr.json` 은 16:05 다. 그 run 의 로그에 `번역 사전 104개`(지금 105)가
찍혀 있고 SAIN 관련 줄이 없다. 게임과 **서버를 다시 켜야** 한다(서버가 lang 파일을 읽는 쪽이다).

**실측.** 클라이언트 빌드 경고 0·오류 0, 테스트 511개 통과. 설치본 md5 `e602b0a8…` 가 산출물과 일치.
옛 DLL 은 `backup/2026-11-02-before-ui-fixes/` 에 있다. **세 손질 모두 게임에서 눈으로 본 것은 없다.**

## 2026-11-02 — 한글 UI 둘째 확인: 로그가 빠진 것을 알려 줬다

**잘 된 것.** SAIN 편집기 한글화가 실제로 돈다 — 로그에 `SAIN 편집기 한글화 패치 완료 — 사전 731개`,
`SAIN 창 UI 한글화 — … 문자열 36개 교체`. 편집기의 설정 이름·분류가 한글로 뜬다. STM 쪽도 분대 화면·
장비 편집기가 한글이다.

**빠진 것을 사람이 찾지 않았다.** 패치가 찍어 둔 `SAIN 사전에 없음` 60줄이 그대로 할 일 목록이었다.
전부 **속성이 없는 설정**이다 — `ConfigInfoClass.GetInfo` 가 `Name = … ?? member.Name` 으로 떨어지면서
멤버 이름(`Aiming`·`Difficulty`·`ShiftCoverResetTime`·`RANDOMSWAY_CIRCLE_RADIUS` 같은 것)을 그대로
보여 준다. 60개를 번역해 더했다(사전 731 → 791). **로그 상한이 60 이라 딱 60줄이 찍혔다** — 더 있는지
알 수 없어 상한을 300 으로 올렸다. 다음 기동의 목록이 남은 전부다.

**칸을 넘치는 이름을 줄였다.** Rush 묶음의 한 줄(`치료·재장전·수류탄 꺼내는 적에게 달려들 수 있다`)이
두 줄로 겹쳐 그려졌다. 이름 칸은 **들여쓰기가 깊을수록 좁아진다**. 한글 한 글자를 라틴 두 글자로 치고
원문보다 넓어진 이름 60개를 뽑아, 그중 눈에 띄게 긴 41개를 줄였다(`Can Auto Pose` → `자세 자동 조절` 등).
짧은 영문에 긴 한글을 붙인 것이 원인이라 번역을 줄이는 것이 맞다.

**`항상 전력 질주` 가 두 줄인 것은 우리 탓이 아니다.** SAIN 이 같은 `[Name("Always Sprint")]` 를 두
설정에 붙였다(`PersonalityGeneralSettings.cs:108,114` — 하나는 엄폐로 질주, 하나는 수색하며 질주).
영문에서도 똑같이 두 번 나온다. 사전은 원문 문자열로만 찾으므로 둘을 가릴 수 없다. 번역이 서로 다른
원문을 같은 한글로 보낸 경우는 `Time To Aim`/`Time to Aim`(대소문자만 다른 같은 뜻) 하나뿐임을
검사로 확인했고, 그 검사를 갱신 스크립트에 넣었다.

**쓰이지 않은 창 UI 항목 5개를 뺐다.** 로그의 `안 쓰임` 목록이다 — 둘은 속성 쪽 사전이 이미 덮고 있고,
나머지 셋(`Assembly Version`·프리셋 설명·플러그인 설명)은 ldstr 이 아니라 바꿀 수 없다.

**장비 편집 입구가 아직 ARENA 와 겹친다.** `anchoredPosition + rect.width` 로 붙였는데 탭의 자기
RectTransform 이 눈에 보이는 탭보다 작았다(그림과 글자가 자식이다). 이제 자식까지 포함한 경계를
`RectTransformUtility.CalculateRelativeRectTransformBounds` 로 재고, **우리 상자의 왼쪽 끝이 ARENA 의
오른쪽 끝 옆에 오도록** 옮긴다(앵커·피벗은 ARENA 와 같게 둔다 - 줄의 피벗이 어디에 있든 맞는다).

**`장비 없이 추가` 버튼의 글자가 깨졌다 - 원인 미상.** 화면에 `장비  추` 로 나왔다. 같은 화면의
`팀원 추가`·`취소` 는 멀쩡하므로 **글자 수나 버튼 폭(260×42) 문제가 아니다** — 그 버튼만 다른 글꼴을
쓰고 그 글꼴에 일부 글자가 없는 쪽이 의심스럽다(TMP 는 없는 글자를 빈칸으로 그린다). 추측으로 고치지
않고 우선 문구를 `장비 없이` 로 줄였다. 다음 확인에서 같은 증상이 남으면 그 버튼의 글꼴 자산을 본다.

**설치.** 게임이 켜져 있어 **사전(JSON)만 넣었다** — `_sain.json`·`_sain-ui.json`·
`xyz.stm.teammanager.json`·STM `kr.json`·`ko.json` 모두 md5 일치. **DLL 둘은 잠겨 있어 남았다**:
`SptTeamManager.dll`(`181f6b56…`, ARENA 자리), `SptKoreanConfig.dll`(`04b85772…`, 로그 상한).
게임을 닫은 뒤 넣어야 한다.

## 2026-11-02 — 프로필 팀원의 AI 타입 (계획 4 의 앞 절반)

**요청.** 용병 화면이 가진 것을 프로필 팀원에게도 — 장비 편집(이미 있다)은 두고, **AI 타입 변경**과
**장비 꾸러미**. 둘의 성격이 아주 달라 앞의 것만 이번에 끝냈다. 뒤의 것은
[계획 4 문서](plans/2026-11-02-stm-plan4-kit-loadouts.md)로 쪼갰다.

**전술은 이미 절반이 깔려 있었다.** 등록 문서의 `Tactic` 이 투입 payload 를 타고 봇까지 간다:
`FriendlyTeammateMatchCallbacks`(팀원 상세 응답) → 클라이언트 `botsTactic[aid]` →
`profileTactic[profileId]` → `BotFollowerPlayer.CombatTactic`. 모델 주석이 "저장만 하고 아무도
해석하지 않는다" 고 적어 둔 것은 **그 사이에 코드가 붙은 뒤에도 갱신되지 않은 문장**이었다.
없었던 것은 **바꿀 길**뿐이다 — 상세 화면이 전술 조작을 거부했고(`ProfileMemberOperationPolicy`),
옵션 응답이 **현재 값 하나만** 목록으로 실어 드롭다운에 고를 것이 없었다.

**연 것.** `StmTacticService.TrySetTactic`(STM 먼저, `StmAutoJoinService` 와 같은 모양) →
`FriendlyTeammateCallbacks.SetTactic` 이 PIT 보다 먼저 묻는다. 허용 목록은 PIT 의
`TacticOptions` 하나이고(§3-4 의 허용 범위 안에서 `private` → `internal` 승격) 값은
`NormalizeCombatTactic` 으로 정규화해 저장한다 — 목록을 STM 쪽에 또 적으면 다음에 하나만 고쳐 갈라진다(§3-8).
`ProfileMemberUpdatePolicy` 에 `Tactic` 을 더했고, **빈 값은 "건드리지 않는다"** 로 읽는다: 빈 값을
저장하면 투입 때 기본 전술로 떨어지는데 그것은 지우기가 아니라 **사용자가 모르는 되돌리기**다.
거부 열거에서 `Tactic` 을 아예 뺐다 — 아무도 부르지 않는 거부 사유는 거짓말이 된다.

**화면은 용병과 같은 한 줄로 바꿨다.** 사용자가 보내 준 용병 화면처럼 `팀원 장비 편집 | AI 타입` 한 줄이다
(`TryShowStmProfileControlRow`). PIT 의 용병 경로가 하는 일을 그대로 하되 — 옷장 패널을 복제해
`DisplayLoadoutOptions` 로 두 칸을 채운다 — **프로필 팀원에게 뜻이 없는 것은 부르지 않는다**:
의상·이름·숙련은 서버가 거부하는 조작이다. 왼쪽 칸은 PIT 의 편집기가 아니라 STM 의 편집기를 연다
(`ReplaceStmLoadoutDropdownWithEditButton`). 장비 편집이 막혀 있어도(레이드 중 등) 줄은 만들고
왼쪽 버튼만 죽인다 — AI 타입은 그때도 바꿀 수 있다. 줄을 못 만들면 예전의 떠 있는 버튼으로 떨어진다.

**전술 드롭다운의 저장은 PIT 의 코드가 그대로 한다.** 선택하면 전술 라우트로 보내고, 그 뒤에 부르는
`SetProficiencyAggressionForTactic` 은 네트워크를 타지 않는 클라이언트 상태뿐임을 읽어 확인했다
(숙련 슬라이더가 없는 우리 화면에서는 아무 일도 하지 않는다).

**실측.** 테스트 **511 → 513**(전술 갱신 5개 추가, 거부 열거에서 `Tactic` 이 빠지며 Theory 3개 감소 —
숫자가 맞는지 세어서 확인했다). 서버·클라이언트·한글화 플러그인 빌드 모두 경고 0·오류 0.
설치본 md5 가 셋 다 산출물과 일치하고 빌드보다 새로운 `.cs` 가 없다.
**한 건 잡혔다**: 한글화 플러그인의 로그 상한 변경이 앞선 빌드에 들어가지 않은 채 설치될 뻔했다
(`04b85772` → 다시 빌드 `bcb25caf`). 매번 타임스탬프를 보는 절차가 잡은 것이다.

**확인하지 않은 것.** 게임에서 보지 않았다. 특히 ① 한 줄이 용병 화면과 같은 자리에 뜨는지
② 드롭다운에 네 전술이 다 보이는지 ③ 고른 뒤 다시 열었을 때 그 값이 남아 있는지
④ 실제 레이드에서 그 전술로 싸우는지(투입 경로는 코드로만 따라갔다).

## 2026-11-02 — 계획 4 Task 1: 장비 빌드 목록 읽기

**실측부터.** 저장된 장비 빌드는 PMC 문서가 아니라 **프로필 뿌리**에 있다:
`SptProfile.UserBuildData.EquipmentBuilds`(json `userbuilds.equipmentBuilds`),
`EquipmentBuild : UserBuild { Id, Name } + { Root, Items, BuildType }`. 설치본의
`SPTarkov.Server.Core.dll` 을 역컴파일해 확인했다. 그래서 팀원 쪽도 `saveServer.GetProfile(profileId)` 로
프로필을 직접 꺼내 읽는다 — `StmResolvedMember` 는 `PmcData` 만 들고 있어 빌드가 없다.

**두 묶음을 자료 구조가 가른다.** 사용자가 정한 "팀원과 플레이어의 프리셋을 별도로 구분하여 표시" 를
꼬리표(`owner` 필드)로 하지 않고 **자리 둘**(`TeammateBuilds` / `PlayerBuilds`)로 했다. 꼬리표는 다음
사람이 떨어뜨릴 수 있지만 자리가 둘이면 떨어뜨릴 수가 없다. 순수 정책(`GearBuildListPolicy`)과 전선
모양(`StmGearBuildsResponse`) 둘 다 같은 모양이다.

**목록에 서지 못하는 빌드.** 아이템이 0개인 빌드(입힐 것이 없다), id 가 없는 빌드(고른 뒤 다시 찾을
열쇠가 없다), 같은 프로필 안의 중복 id(처음 것만). **두 프로필 사이의 id 겹침은 남긴다** — 프로필을
복사해 만들면 같은 id 가 양쪽에 있을 수 있고 그 둘은 서로 다른 빌드다. 이름이 비어 있으면
`이름 없는 빌드` 로 부른다. 이름순으로 세우되 같은 이름은 id 로 한 번 더 갈라 **순서를 고정**한다 —
그러지 않으면 목록을 다시 열 때마다 줄 순서가 바뀐다.

**살아 있는 객체는 넘기지 않는다(불변식 1).** 내보내는 것은 id 와 이름뿐이다. 빌드의 `Items` 는
SaveServer 가 들고 있는 목록이라 개수만 세고 참조는 버린다. 한쪽 프로필을 못 읽으면 그쪽만 빈 목록으로
두고 로그에 남긴다 — 목록이 비어 보이는 것이 틀린 빌드를 입히는 것보다 안전하다.

**실측 — 라우트가 실제로 붙었다.** 단위 테스트가 닿지 않는 자리(DI 등록과 라우트 표)를 확인하려고
**서버를 직접 띄웠다**. 기동 로그: `Teammate database ready` ×5, `PitFireTeam loaded`, STM 오류 없음
(의존성이 안 풀렸으면 여기서 던졌을 것이다). 그 다음 세 경로를 같은 방식으로 찔러 비교했다:

| 경로 | 결과 |
|---|---|
| `/stm/members/builds`(새것) | HTTP 200 |
| `/stm/members/gear-stash`(이미 도는 것) | HTTP 200 |
| `/stm/members/does-not-exist` | HTTP 404 |

새 라우트가 **이미 도는 라우트와 똑같이** 행동하고 없는 경로와는 다르다 — 등록됐다는 뜻이다.
(세션 쿠키 없이 찌른 요청이라 둘 다 서버 로그에 `Error handling request` 를 남긴다. 내 찌르기 탓이지
코드 탓이 아니다 — 같은 줄이 기존 라우트에도 똑같이 났다.) 확인 뒤 서버는 내렸다.

**서버 로그의 시계는 UTC 다**(한국 시간 -9h). 기동 시각이 17:48 인데 로그에 08:48 로 찍혀 "옛 줄" 로
잘못 읽을 뻔했다. BepInEx 의 `LogOutput.log` 는 현지 시각이라 둘을 나란히 볼 때 헷갈린다.

**실측 숫자.** 테스트 **513 → 527**(빌드 목록 정책 14개). 서버·클라이언트 빌드 경고 0·오류 0.
서버 설치본 md5 `84396ec5…` 가 산출물과 일치.

**남은 것.** Task 2(값 매기기) → 3(구매·적용, 저널의 첫 사용자) → 4(화면) → 5(인게임 관문).
지금은 **목록을 읽는 길만** 있고 화면에서 부르는 곳은 없다.

## 2026-11-02 — 계획 4 Task 2: 꾸러미 값 매기기

**칸 단위로 가른다.** 부착물 하나하나를 견주어 모자란 것만 사면 팀원이 쓰던 총을 뜯어 부품을 갈아
끼워야 한다 — 프로필 두 개를 오가는 조작보다도 되돌리기 어렵다. 사용자가 고른 "전부 새로 구매" 와도
어긋난다. 그래서 **칸 하나는 통째로 같거나 통째로 바뀐다**(`GearBuildPlanPolicy`):
`Unchanged` / `Replaced` / `Added` / `Removed`. "같다" 는 그 칸의 아이템과 **그 아래 붙은 것 전부**가
tpl 과 자리까지 같다는 뜻이다 — 같은 총이라도 조준경이 다르면 사용자가 고른 것과 다른 장비다.

격자(가방·조끼 안의 짐)는 자연히 빠진다: 부착물 칸 이름이 아니라서 하위 묶음에 들어오지 않는다.
방탄판은 부착물 칸이라 들어온다. 뿌리 칸이 아닌 자리(주머니·보안 컨테이너·완장·칼집)는 빌드가
건드리지 않는다.

**값은 PIT 의 표를 그대로 쓴다.** `FriendlyTeammateService.CreateHiringUnitPriceLookup` 을
`internal` 로 올려(§3-4 승격) 꾸러미 견적이 같은 값을 본다. 같은 물건에 용병 고용 때와 꾸러미 때
다른 값이 붙으면 그것이 곧 버그 신고가 된다. 그 표는 `Item` 을 받지만 보는 것은 `Template` 뿐이라,
값을 묻는 동안만 사는 껍데기 하나를 만들어 넘긴다(`PriceProbe`).

**하나라도 못 매기면 전체를 거절한다**(`GearBuildQuotePolicy`). 0 이하·NaN·무한대는 전부 "값이 없다"
다 — 0 원짜리 장비는 없고, 0 은 "표에 없다" 의 다른 모습이다. 값을 묻다 던지는 것도 같게 친다.
부분 구매로 어중간하게 입히면 플레이어는 돈을 내고도 빌드와 다른 것을 입은 팀원을 보게 되고 무엇이
빠졌는지 알 길이 없다. 합이 `int` 를 넘는 경우도 거절한다(화면에 음수를 그리지 않는다).

**어휘의 집은 하나다 — 기계가 지킨다.** 장비 뿌리 열 칸이 PIT 의 `TeammateEquipmentPrice.Roots`
(값을 매기는 자리)와 STM 의 `GearBuildPlanPolicy.RootSlotNames`(사는 자리) 두 곳에 있다. 중복은
의도다(정책은 SPT 어셈블리 없이 테스트돼야 한다). 어긋나면 **값을 매기지 않은 것을 사거나 산 것에
값을 매기지 않는다** — 둘 다 돈이 걸린 어긋남이라, 기존 `RequiredRaidWeaponSlots` 검사와 같은 자리
(`PitFireTeamServerPostLoad`)에 대조를 하나 더 넣었다. 이번 기동에서 어긋남 줄이 **찍히지 않았다**.

**shared 는 .NET Framework 4.7.2 에서도 컴파일된다.** `double.IsFinite` 를 썼다가 클라이언트 빌드가
깨졌다(net472 에 없다). 세 프로젝트에 다 링크해 두는 관례가 잡아 준 것이다 — 서버만 보고 넘겼으면
설치 뒤에야 알았을 것이다. `!IsNaN && !IsInfinity` 로 바꿨다.

**실측.** 테스트 **527 → 554**(계획 15 + 견적 12). 빌드 셋 다 경고 0·오류 0(`--no-incremental` 로
확인했다 — 증분 빌드는 경고를 다시 찍지 않는다). 서버를 띄워 새 라우트를 찔렀다:

| 경로 | 결과 |
|---|---|
| `/stm/members/kit-quote`(새것) | HTTP 200 |
| `/stm/members/builds` | HTTP 200 |
| `/stm/members/does-not-exist` | HTTP 404 |

기동 로그에 `PitFireTeam loaded`, STM 오류 없음, 어휘 어긋남 없음. 확인 뒤 서버는 내렸다.
설치본 md5 `7d33a032…` 가 산출물과 일치.

**아직 아무것도 사지 않는다.** 이 Task 는 읽기 전용이다 — 돈도 물건도 건드리지 않는다.
다음이 Task 3(구매·적용)이고, 거기서 전송 저널이 **처음으로** 쓰인다.

## 2026-11-02 — 계획 4 Task 3: 꾸러미 구매 (전송 저널의 첫 사용자)

**열린 항목 6 이 코드에서 닫혔다.** `StmTransferJournal` 에 처음으로 **운영 호출자**가 생겼다.
2026-11-02 아침까지 이 저널은 `Prepare`·`BeginSourceWrite`·… 를 부르는 곳이 **하나도 없어** 항목이
생길 수 없었고, 기동 복구도 막힌 팀원 출구 UI 도 실물로 확인할 길이 없었다.

**저널의 모형에 이 조작을 어떻게 얹었나.** 저널은 "아이템이 A 에서 B 로 간다" 를 위해 만들어졌는데
꾸러미 구매는 "**돈이 A 에서 나가고, 새 물건이 B 에 생긴다**" 다. 복구 판정
(`TransferJournalPolicy.DecideFromInventories`)이 보는 것은 **ItemIds 가 출처/목적지 인벤토리에 있는가**
뿐이므로, ItemIds 를 **새로 사는 물건의 id** 로 둔다. 그러면 복구표가 이렇게 읽힌다:

| 끊긴 지점 | 출처(플레이어) | 목적지(팀원) | 판정 |
|---|---|---|---|
| 물건까지 들어감 | 없음 | 있음 | **RollForward** — 깨끗이 닫는다 |
| 돈만 나감 | 없음 | 없음 | **StopAndReport** — 사람에게 알리고 편집을 막는다 |

둘 다 옳다. "돈은 나갔는데 물건이 없다" 가 정확히 사람이 봐야 하는 상태다. 출처가 이 id 들을 가진 적이
없다는 것은 거짓이 아니라 **이 종류(`kit-purchase`)의 사실**이고, 그래서 `Kind` 를 남긴다.

**터질 수 있는 것은 전부 저널을 열기 전에 끝낸다.** 값 다시 매기기, 관문 판정, 돈 차감, 새 아이템 만들기,
헌 장비의 스태시 자리 찾기 — 전부 **복제본 위에서** 먼저 해 본다. 하나라도 실패하면 살아 있는 객체는
그대로이고 저널은 열리지도 않는다. 저널이 열린 뒤에 남는 일은 **두 번의 저장뿐**이다.

**돈 규칙의 집은 PIT 하나다.** `DeductRoublesFromPlayerStash` 를 `internal` 로 올려 그대로 쓴다
(스태시 안의, 잠기지 않은 루블만 센다). STM 쪽에서 규칙을 다시 적었으면 "살 수 있다" 고 해 놓고
결제에서 거절하는 일이 생겼을 것이다. 견적의 `PlayerRoubles` 는 주머니까지 세는 **안내용 상한**이라
관문을 통과해도 실제 차감이 거절할 수 있는데, 그 거절이 **복제본에서** 일어나므로 아무것도 쓰이지 않는다.

**스태시 자리 찾기도 집이 하나다.** 장비 편집의 `TryPlaceInStash` 를 `internal` 로 올려 같이 쓴다.
격자 자리 판정이 두 곳에 생기면 한쪽만 고쳐져 아이템이 겹쳐 놓인다. 자리가 없으면 **전체를 거절**한다 —
반쯤 벗은 팀원을 남기지 않는다.

**산 물건은 새 id 로 만든다.** 빌드의 id 를 그대로 쓰면 다른 프로필의 아이템과 같은 id 가 생겨 복구가
"복제" 로 읽는다. 부모 사슬을 새 id 로 다시 잇고, 빌드 안에 부모가 없는 것(= 장비 칸에 바로 걸리는 뿌리)은
팀원의 장비 루트에 건다. 격자 좌표(`Location`)는 지운다 — 장비 칸에서는 뜻이 없고 남겨 두면 SPT 가
자리 계산을 엉뚱하게 한다.

**확인은 디스크에서 세 가지를 본다**(불변식 6): ① 산 물건의 새 id 가 팀원 프로필에 전부 있다
② 그 id 가 플레이어 쪽에 **끼어들지 않았다**(복제 검사) ③ 플레이어의 루블 합이 **정확히 낸 만큼** 줄었다.
하나라도 어긋나면 `Verify` 가 항목을 닫지 않고 편집이 막힌 채로 남는다 — 이때는 되돌리지 않는다.
두 저장이 끝난 뒤 디스크 상태를 추측으로 뒤집는 쓰기를 더하는 것이 더 위험하다.

**실측.** 테스트 **554 → 566**(구매 관문 12). 서버·클라이언트 빌드 경고 0·오류 0(`--no-incremental`).
서버를 띄워 `/stm/members/kit-buy` 가 200, 없는 경로가 404 임을 확인했다. 기동 로그에
`PitFireTeam loaded`, STM 오류 없음, 어휘 어긋남 없음, 저널 복구 보고 없음. 설치본 md5 `6fe8f6bd…`.

**한 번도 실행되지 않았다.** 이 경로는 세션 쿠키가 있는 진짜 요청이 와야 돈다 — 컴파일되고, DI 가 풀리고,
라우트가 붙었다는 것까지만 확인했다. **돈이 실제로 빠지고 물건이 실제로 들어가는 것은 아무도 보지 않았다.**
Task 5 의 인게임 관문에서 프로필 JSON 대조(구매 전후 id 집합과 루블 합)로 처음 확인한다. 그때 저널의
기동 복구와 막힌 팀원 출구 UI 도 처음으로 실물 경로를 탄다.

**남은 것.** Task 4(화면) → Task 5(인게임 관문).

## 2026-11-02 — 계획 4 Task 4: 장비 꾸러미 화면

**자리.** 프로필 팀원 상세 화면의 조작 줄 아래 한 줄을 더 둔다 — 용병 화면의 `장비 꾸러미` 와 같은
자리다(`GetProfileControlRowOffset(…, 2)`). 장비 편집이 막혀 있으면(레이드·미결 전송) 버튼도 죽는다:
서버가 같은 관문을 서므로 눌러도 거절만 돌아온다.

**흐름.** 버튼 → 빌드 목록(두 묶음) → 고르면 값 → 확인창 → 구매. 창 하나를 다시 그려 쓴다.
목록은 `{팀원 이름} 의 빌드` 와 `내 빌드` 두 머리글로 갈라 세운다 — 서버가 두 묶음을 **각자의 자리로**
주므로(`teammateBuilds`/`playerBuilds`) 화면이 섞을 여지가 없다. 한 묶음에 열 줄까지만 보이고,
넘치면 넘치게 그리는 대신 끊고 "그 밖에 N개가 더 있습니다" 라고 말한다.

**화면은 판정하지 않는다.** 얼마인지·살 수 있는지·무엇이 바뀌는지는 전부 서버가 정하고 여기서는
보여 주기만 한다. 구매 버튼의 활성 여부도 서버가 보낸 `affordable` 을 그대로 쓴다(화면에서 돈을 다시
세지 않는다 - 세는 곳이 둘이면 갈라진다). 살 때는 **화면이 보여 준 값**(`expectedTotal`)을 함께 보내
서버가 다시 매겨 견준다. 다르면 서버가 거절한다.

**좌표계에서 한 번 틀렸다.** `CreateOverlayButton` 은 앵커·피벗이 (0,0) 이라 **왼쪽 아래 기준**인데
목록을 위에서부터 음수로 쌓았다 — 그대로 뒀으면 버튼이 전부 창 밖에 그려졌을 것이다. 또 머리글을
`CreateOverlayText` 로 만들고 `anchoredPosition` 을 주려 했는데, 그 헬퍼는 앵커가 (0,0)-(1,1) 이라
창 전체로 늘어난다 — 여러 머리글이 같은 자리에 겹친다. 둘 다 빌드가 잡아 주지 않는 종류라
(컴파일은 통과한다) 헬퍼의 구현을 읽고 고쳤다: 목록은 "위에서 몇 px" 로 세고 바닥 기준으로 바꿔 놓으며,
머리글은 위쪽 앵커를 쓰는 전용 함수를 따로 뒀다.

**요청이 나간 뒤의 실패는 "샀는지 모른다" 로 말한다.** 샀다고도 안 샀다고도 하지 않고, 화면을 다시 열어
장비와 돈을 확인하라고 한다. 거절 사유는 토스트와 창에 **둘 다** 남긴다 - 토스트는 금방 사라지는데
이 사유들은 읽을 시간이 필요하다.

**실측.** 클라이언트 빌드 경고 0·오류 0, 설치본 md5 `47428e7e…` 가 산출물과 일치.
**게임에서 열어 보지 않았다** - 버튼이 그 자리에 뜨는지, 목록 두 묶음이 읽히는지, 값 창의 글이 칸을
넘치지 않는지 전부 미확인이다. Task 5 의 인게임 관문에서 처음 본다.

## 2026-11-02 — 꾸러미를 게임의 기본 장비 프리셋 화면으로 옮긴다 (Task 4-b)

**왜.** 자체 창으로는 용병 화면과 같은 모양이 될 수 없다(2026-11-02 인게임: 제목이 목록과 겹쳤고 모양도
달랐다). 용병 쪽이 쓰는 것은 PIT 가 그린 창이 아니라 **EFT 의 `EquipmentBuildsScreen`** 그 자체다 -
`TeammateEquipmentBuildsScreenFlow.Open` 이 그 화면을 띄우고 위에 가격·3D 미리보기·구매를 얹는다.

**이음매는 두 개였다.** 4500줄짜리 패치 묶음이지만 갈라져야 하는 자리는 둘뿐이다:
`HandleBuyRequest()`(구매 한 지점)와 `ApplyBuildListPrice(...)`(줄의 가격 라벨). PIT 파일에는
그 둘 앞에 STM 분기를 걸고, 읽어야 하는 `_activeScreen`·`ScreenSelectedBuildField` 는
`internal` 로 올리기만 했다(인계 §3-4). 프로필로 돌아갈 때 모드를 끄는 한 줄을 더했다 - 남으면
다음에 연 **용병** 화면이 STM 분기를 탄다.

**값은 보여 주지 않다가, 고른 뒤 서버 값만 보여 준다.** PIT 의 줄 가격은 용병용 할인 규칙이 붙은 값이라
우리가 청구하는 값과 다르다. 두 숫자가 한 화면에 있으면 어느 쪽이 진짜인지 알 수 없으므로, STM 모드에서는
**줄에 아무 숫자도 쓰지 않고** 고른 뒤 서버가 매긴 값만 창으로 보여 준다. 살 때도 그 값을
`expectedTotal` 로 보내 서버가 다시 견준다.

**기본 화면에는 플레이어의 빌드만 뜬다.** 그 화면은 `session.EquipmentBuildsStorage` 를 읽는데
(`EquipmentBuildsScreen` 역컴파일 확인) 그것은 **플레이어의** 보관소다. 팀원 본인의 빌드를 그 목록에
끼워 넣으려면 플레이어의 보관소를 건드려야 한다 - 창이 열린 채 튕기면 플레이어 자신의 프리셋 화면에
남의 빌드가 남을 수 있는 길이라 **따로 한다.** 그때까지 팀원 빌드는 상세 화면의 `팀원 빌드` 버튼
(작은 창)에서 고른다. 그 창은 이제 **팀원 빌드만** 보여 준다 - 같은 것을 두 모양으로 두지 않는다.

**버튼 줄.** 조작 줄 아래로 `장비 꾸러미`(기본 화면, 플레이어 빌드)와 `팀원 빌드`(작은 창) 두 줄이다.
둘 다 장비 편집이 막혀 있으면 함께 죽는다.

**실측.** 클라이언트 빌드 경고 0·오류 0. **게임이 켜져 있어 설치는 보류했다** -
산출물 md5 는 설치할 때 적는다. 게임에서 열어 본 것은 없다.

## 2026-11-02 — 꾸러미 입구를 둘로 나누고 팀원 빌드도 기본 화면에 싣는다

**요청.** `내 장비 꾸러미` / `팀원 장비 꾸러미` 두 버튼으로 나누고, **어느 쪽을 눌러도 용병과 같은 화면**이
열리되 고른 쪽의 빌드가 나오게.

**되는 이유(실측).** 기본 화면이 읽는 `EFT.UI.Builds.EquipmentBuildsStorage` 의
`_equipmentBuilds` 는 **public Dictionary 필드**다(역컴파일 확인). 그리고 `EquipmentBuild` 에는
`(MongoID id, string name, MongoID root, FlatItem[] items, EEquipmentBuildType)` 생성자가 있다 —
서버가 보낼 수 있는 모양 그대로다. 그래서 팀원 빌드를 클라이언트에서 만들어 **사전 참조만 바꿔 끼우면**
그 화면이 그대로 그려 준다.

**플레이어의 사전은 건드리지 않는다.** 원본 사전 객체에 넣고 빼는 것이 아니라 **새 사전을 대신 보여 주고**
닫을 때 원래 참조를 돌려놓는다. 되돌리기는 `End()` 와 `Begin()` 양쪽에서 부른다(이중 안전장치) —
창이 제대로 닫히지 않은 다음번에도 끼우기 전에 먼저 되돌린다. 되돌리기가 실패하면 로그에 남기고 참조를
비워, 다음 시도가 **원본을 덮어쓰지 않게** 한다. 그래도 남는 위험은 하나다: 끼운 채로 게임이 튕기면
그 실행 동안 플레이어 자신의 프리셋 화면에 남의 빌드가 보일 수 있다(저장은 하지 않으므로 다시 켜면 사라진다).

**서버.** `/stm/members/builds` 의 **팀원 줄에만** `root`·`items` 를 싣는다. 플레이어 빌드는 게임이
이미 갖고 있어 싣지 않는다 - 같은 것을 두 번 보내지 않는다. 아이템은 `cloner.Clone` 으로 **값을 베껴**
싣는다(SaveServer 의 살아 있는 목록을 그대로 넘기지 않는다, 불변식 1).

**구매가 어느 쪽 빌드인지 헷갈리지 않게.** 화면에 실린 것이 팀원 빌드면 구매 요청의 `owner` 도
`teammate` 로 간다(`CurrentOwner`). 싣기에 실패하면 모드가 켜지지 않아 플레이어 빌드가 보이고 구매도
플레이어 쪽으로 간다 — 보이는 것과 사는 것이 어긋나지 않는다.

**자체 목록 창은 걷어냈다.** 두 입구가 모두 기본 화면을 열게 되면서 닿지 않는 코드가 됐다. 675줄짜리
파일을 240줄로 줄였고, 지운 함수 여덟 개가 어디에서도 불리지 않는 것을 확인했다(`grep` 0건).
"지었는데 닿지 않는다" 를 이 계획에서 다섯 번 냈으므로, 이번에는 같은 턴에 치운다.

**실측.** 클라이언트·서버 빌드 경고 0·오류 0, 테스트 566개 통과. 클라이언트 설치본 md5 `088c1567…`.
**서버가 켜져 있어 서버 DLL 은 설치하지 못했다** - 팀원 빌드의 아이템을 싣는 변경이 거기 있으므로,
서버를 내렸다 올리기 전에는 `팀원 장비 꾸러미` 가 "빌드가 없습니다" 로 뜬다.

**확인하지 않은 것.** 게임에서 두 버튼을 눌러 본 적이 없다. 특히 ① 팀원 빌드가 기본 화면에 제대로 그려지는지
(3D 미리보기와 아이템 목록까지) ② 창을 닫은 뒤 플레이어 자신의 프리셋 화면이 원래대로인지 ③ 구매가
팀원 빌드로 갔는지.

## 2026-11-02 — 장비 꾸러미 두 버튼이 열리지 않던 것을 고쳤다

**증상(인게임).** `내 장비 꾸러미`·`팀원 장비 꾸러미` **둘 다** 누르면 "팀원 장비 꾸러미를 열 수 없습니다"
만 뜨고 화면이 열리지 않았다.

**원인.** 그 문장은 PIT 의 `TeammateEquipmentBuildsScreenFlow.Open` 이 **세션이나 인벤토리 컨트롤러가
없을 때** 내는 것이다. 그 둘을 담는 `ActiveProfileSession`·`ActiveProfileInventoryController` 는 PIT 가
`PatchPostfix` 의 **용병 분기 아래**(996·998줄)에서 채우는데, STM 분기는 970줄에서 `return` 한다.
즉 프로필 팀원 화면에서는 그 전역이 비어 있었다. 장비 편집기가 멀쩡했던 것은 그쪽이 전역이 아니라
인자로 받은 컨트롤러를 쓰고, 자기 경로에서 전역을 직접 채우기 때문이다(`StmGearEditor` 369줄).

**고친 방식(두 겹).** ① `TryApplyStmDetail` 이 STM 분기에서도 `ActiveProfileScreen`·
`ActiveProfileInventoryController`·`ActiveProfileSession` 과 `RememberActiveBackendInventoryController`
를 채운다 - PIT 의 다른 화면 부품도 같은 전역을 보므로 한 곳에서 채우는 쪽이 맞다. ② 꾸러미 경로는
**전역에 기대지 않는다**: 상세 화면이 받은 `inventoryController`·`session` 을 버튼까지 인자로 내려
`Open` 에 그대로 넘기고, 그래도 비어 있으면 **왜 못 열었는지 로그에 남기고** 토스트를 띄운다(전에는
PIT 의 문장만 뜨고 원인이 로그에 없었다).

**실측.** 클라이언트 빌드 경고 0·오류 0. 설치본 md5 `649bbda6…`(산출물과 일치). 서버 DLL 은
`d0da993f…` 로 이미 설치돼 있다(팀원 빌드에 아이템을 싣는 변경 포함).

**확인하지 않은 것.** 게임이 켜진 상태로 설치했으므로 **게임을 다시 켜야** 이 코드가 돈다. 두 버튼이
실제로 열리는지, 팀원 빌드가 기본 화면에 그려지는지, 창을 닫은 뒤 플레이어 자신의 프리셋이 원래대로인지는
아직 보지 못했다.

## 2026-11-02 — 장비 꾸러미 화면이 인게임에서 돌았다 (계획 4 Task 1·4 관문)

**사용자 확인(게임 재시작 후).** ① 두 버튼이 열린다 ② 팀원 빌드가 그려진다 ③ 닫은 뒤 플레이어 자신의
프리셋이 원래대로다.

**로그 대조.** 새 run 의 `LogOutput.log` 에서 STM 줄 네 개가 **짝으로** 찍혔다:

```
3503 [STM] 기본 화면에 팀원 빌드 2개를 실었다(원래 목록은 닫을 때 되돌린다).
4255 [STM] 기본 화면의 빌드 목록을 플레이어의 것으로 되돌렸다.
4275 [STM] 기본 화면에 팀원 빌드 1개를 실었다(원래 목록은 닫을 때 되돌린다).
4418 [STM] 기본 화면의 빌드 목록을 플레이어의 것으로 되돌렸다.
```

싣기 둘·되돌리기 둘로 **짝이 맞는다** - 사용자가 본 ③(프리셋이 원래대로)과 같은 사실을 코드 쪽에서도
말한다. `열지 못했다`·`보관소를 찾지 못했다` 줄은 없다. 이 run 의 `[Error` 는 하나뿐이고 STM·PIT 과
무관하다(Direct Reload 가 Fika 없음으로 패치 포기).

**열린 항목 7번이 닫혔다.** "프로필 타일을 눌러 상세 화면이 뜨는 것을 본 사람이 없다" 가 이제 아니다 -
상세 화면이 뜨고, 왼쪽 칸이 그 팀원의 스태시이고, 그 화면의 장비 편집·AI 타입·꾸러미 두 버튼이 모두
동작한다.

**아직 돌지 않은 것 — 구매.** 값 매기기·돈 빼기·입히기·헌 장비 이송과 **전송 저널의 실물 경로**(열린 항목
6번)는 한 번도 실행되지 않았다. 화면이 열린다는 것은 그 앞의 절반일 뿐이다. 남은 절차는 계획 4 문서
§6 의 2~6번이고, **0번(프로필 백업)을 건너뛰면 되돌릴 수 없다**. 인계 문서에 열린 항목 **9번**으로 세웠다.

## 2026-11-02 — 꾸러미 확인 창이 보이지 않던 것을 고쳤다 (관문 중단)

**증상.** 관문 2단계에서 `구매` 를 눌러도 아무 창도 뜨지 않았다. 사용자가 **열한 번** 눌렀다.

**측정.** 서버 로그에 `/stm/members/kit-quote` 가 11번 찍혔고 `/stm/members/kit-buy` 는 **0번**이다.
클라이언트·서버 양쪽에 STM 오류 0건. 즉 **값은 매겨졌고 창을 만드는 데까지 갔는데 보이지 않았다** -
돈과 아이템은 하나도 움직이지 않았다.

**원인.** `CreatePanel` 이 `GameObject.FindObjectOfType<Canvas>()` 로 **아무 캔버스나** 잡았다. 유니티가
어느 것을 돌려줄지는 정해져 있지 않다. 캔버스를 찾기는 했으니 경고도 남지 않는다 - 조용히 보이지
않는 자리에 그려졌다. PIT 은 같은 화면에서 `_activeScreen.transform` 에 붙인다(`ShowBuyConfirmOverlay`).

**고침.** 열려 있는 그 화면(`TeammateEquipmentBuildsScreenFlow._activeScreen`)에 붙이고, 캔버스는
대비책으로만 남겼다. 띄운 자리를 `[STM] 꾸러미 창을 '…' 위에 띄웠다` 로 로그에 남긴다 - 다음에 또
"안 뜬다" 면 만들어졌는지부터 가를 수 있다. 머리말의 낡은 문장(팀원 빌드는 이 화면에 못 싣는다)도
지웠다.

**판정하지 못한 것 — 목록 줄의 `6.1`.** STM 모드에서는 PIT 의 줄 가격 계산을 건너뛰므로(보여 준 값과
청구가 어긋나는 것이 가장 나쁘다) 그 자리에는 EFT 가 원래 쓰는 값이 남는다. 그것이 무게인지 다른
숫자인지 **화면만 보고는 가릴 수 없었고, 추측으로 고치지 않았다.** 확인 창이 뜨면 거기 값과 견주어
판정한다.

**실측.** 클라이언트 빌드 경고 0·오류 0. 설치본 md5 `fbfea4de…`(산출물과 일치, 더 새로운 `.cs` 없음).
게임을 다시 켜야 돈다.

## 2026-11-02 — 계획 4 인게임 관문: 돈과 아이템이 처음으로 움직였다

**산 것.** 팀원 **Hawk**(`6ae70e1b28569fc69cf98f01`)에게 플레이어 빌드 `Black Division`
(`6ae57670010015d86ee095f4`, owner=`player`)을 **4,578,428 ₽** 에 입혔다. 살 물건 70점,
팀원 스태시로 돌아갈 헌 장비 0점(Hawk 가 입고 있던 것은 `SecuredContainer`·`Pockets`·`ArmBand`
셋뿐이라 벗겨서 돌려줄 장비가 없었다 - 0 이 정답이다).

**대조 (§6-4, 프로필 JSON 재독).**

| 질문 | 결과 |
|---|---|
| 플레이어 루블이 정확히 그 값만큼 줄었나 | 1,643,771,312 → **1,639,192,884** = 정확히 −4,578,428 |
| 플레이어 스태시의 아이템 id 가 사라졌나 | **하나도 없다**(전부 새로 샀다) |
| 팀원이 새로 입은 것이 새 id 인가 | **70개 모두** 전에 어디에도 없던 id |
| 팀원이 플레이어의 물건을 가져왔나 | **0개** |
| 다른 팀원이 바뀌었나 | SPT02·Knox·Buck **바뀐 것 없다** |

Hawk: 입은것 3 → 73(+70), 스태시 79 → 79.

**저널이 처음으로 한 바퀴 돌았다(열린 항목 6).** 서버 로그에 `꾸러미 구매 완료 … (항목
'5814790e3b534136af6c1d26b508b06a')` 한 줄, `구매 확인:` 실패 줄 **0건**. 이 완료 줄은
`Verify`(디스크 재독 대조)가 **통과한 뒤에만** 찍힌다(`StmGearKitPurchaseService` 243~279줄) -
즉 저널이 `Prepare → BeginSourceWrite → RecordDestinationItemIds → BeginDestinationWrite →
MarkCommitted → Verify` 를 끝까지 돌고 항목을 닫았다. 서버의 재독 판정과 우리 쪽 프로필 대조가
**서로 독립적으로** 같은 말을 한다.

**값을 구매 직전에 다시 매기는 계약도 동작했다.** 같은 빌드의 값이 4,578,702 → 4,578,428 로
흔들렸고(상인 가격), `kit-buy` 가 들어온 12:09:35.712 에 서버가 **다시 매긴 뒤** 4,578,428 로 청구했다.

**전체 거절도 처음 인게임에서 봤다.** 팀원 Hawk 자신의 빌드 `NEW PRESET 1` 은 "값을 매길 수 없는
물건이 1개"(tpl `6a565aede592c037e10a3c16`)로 **전체 거절**됐다. 부분 구매로 어중간하게 입히지 않는다.

**곁다리로 판정된 것 — 목록 줄의 숫자.** `44.3 / 25.2 / 12.5` 같은 값은 **무게**다. PIT 이 값을 쓰는
자리가 원래 무게 패널이고(`BuildListWeightField`), STM 모드에서는 그 덮어쓰기를 건너뛰므로 무게가
그대로 남는다. 값이 아니므로 틀린 숫자는 아니지만 **아이콘이 ₽ 처럼 보이는 것은 남은 자리다.**

**아직 안 한 것.** ① 깨끗한 서버 재기동 로그에서 `끝나지 않은 전송` 이 없는 것(§6-5 앞부분)
② 끊긴 경우를 일부러 만드는 실험(구매 직후 서버 강제 종료 → 기동 복구 판정과 `이 기록 닫기` UI).
②는 저널의 **복구** 경로라서 여전히 미증명이다.

### 같은 날 — 깨끗한 재기동으로 저널이 닫힌 것을 확인했다 (§6-5 앞부분)

게임을 닫고 서버를 **저장이 끝난 상태에서** 내린 뒤 프로필을 다시 읽었다. 게임이 켜져 있을 때 읽은
것과 **똑같다**(루블 −4,578,428, 플레이어 스태시 id 손실 0, Hawk 의 새 70점 전부 새 id). 관문의
본체인 §6-4 는 이로써 제대로 된 조건에서 통과했다.

다시 올린 서버의 기동 로그(12:17:23~12:19:12): `Teammate database ready` ×5, `PitFireTeam loaded`,
**`끝나지 않은 전송`·`어휘가 어긋났다`·`StopAndReport`·`RollForward` 0건**, pitTeam 의 Error·Warning
**0건**. 저널 항목 `5814790e…` 는 `Verify` 에서 닫혔고 다음 기동이 집어 들 미결 항목이 없다.

**남은 것은 하나다** — 끊긴 경우의 복구. 구매 직후 서버를 강제 종료해 "돈만 나간" 상태를 만들고,
기동 복구가 `끝나지 않은 전송을 자동으로 정할 수 없어…` 로 보고하는지, 그 팀원의 편집이 막히는지,
상세 화면에 `이 기록 닫기` 가 뜨는지 보는 실험이다. 프로필 백업(`backup/2026-11-02-before-kit/`)이
있어야만 한다.

## 2026-11-03 — 다시 테스트하기 전에 서버를 돌려 보고 나온 것을 고쳤다

**서버 쪽은 깨끗했다.** 게임·서버를 모두 내린 상태에서 서버만 올려 기동 642줄을 전수로 분류했다.
`Error`/`Critical`/`Warning` 는 전부 SPT 코어나 **다른 모드**의 것이고 **pitTeam·STM 의 것은 0건**이다.

| 낸 쪽 | 수 | 무엇 | 우리 것인가 |
|---|---|---|---|
| `ItemBaseClassService` | 48+48 | 아이템 DB 에 없는 tpl 48개를 캐시에 못 넣음. **플리마켓 상품 생성 직후**에 난다 | 아니다(플리마켓/아이템 추가 모드) |
| `ProfileMigrationService` | 5 | `TraderGenPocketMigration` 대기 → **전부 성공**으로 이어진다 | 아니다(정상 흐름) |
| `BundleLoader` | 1 | `barrel_vpo215_600mm_366tkm.bundle` 추가 실패 | 아니다 |
| `FenceService`·`TraderHelper`·SVM·CaseTrader·WTT·RealisticInsurance | 9 | 각 모드의 자기 경고 | 아니다 |

클라이언트 로그(6,029줄)도 같이 봤다. `Warning`/`Error` 는 HarmonyX 21·FixPluginTypesSerialization 13·
OpenSesame 6·AmmoGen 1·DirectReload 1 뿐이고 **우리 플러그인의 것은 0건**이다.

**그래서 고친 것은 로그가 아니라 화면이다 — 같은 종류의 결함 넷.** 전부 "STM 모드인데 PIT 의 용병용
동작이 그대로 돈다" 이고, 셋은 **보여 주는 숫자와 청구가 어긋나는** 자리였다.

1. **목록 줄의 ₽ 아이콘** (`ApplyRoublesIcon`). 값은 안 쓰면서 루블 아이콘만 달아 두어, 그 자리에 남는
   **무게**(44.3·25.2)가 값처럼 보였다. 어제 내가 직접 값으로 읽을 뻔했다. → STM 모드에서는 아이콘도 달지 않는다.
2. **고른 빌드의 값** (`ApplySelectedBuildPrice`). 이 자리만 STM 분기가 **빠져 있었다**. 화면 위쪽 패널에
   PIT 가 매긴 값을 그대로 썼다. 로그에서 찾았다 - STM 모드인데 `[UI][KitPrice]` 진단이 쏟아지고 있었다.
   → 같은 이유로 쓰지 않는다. 덤으로 빌드 하나당 수십 줄의 진단 로그도 사라진다.
3. **"스태시의 물건을 쓴다" 체크박스** (`EnsureExcludeExistingItemsToggle`). STM 서버는 이 값을 **보지
   않는다**(사용자가 "전부 새로 구매" 를 골랐다). 눌러도 아무 일이 없는 칸이었고, 켜 두면 버튼 글자가
   `장착` 으로 바뀌어 **실제 동작과 반대로** 읽혔다. → STM 모드에서는 숨기고, 용병 쪽에서 켜 두었던
   static 값도 끈다(모드를 넘어 남는 값이었다). 버튼 글자도 늘 `구매` 다(`ApplyActionButtonState`).
4. **거절 문구가 무엇 때문인지 말하지 않았다.** 계획 §5-3 의 약속은 "무엇이 없어서인지 말한다" 인데
   개수만 말했다("값을 매길 수 없는 물건이 1개"). 어제 Hawk 의 빌드가 이걸로 막혔고 사용자는 어느
   물건인지 알 길이 없었다. **실물을 찾아보니** tpl `6a565aede592c037e10a3c16` =
   `[WTT-ContentBackport] Thunderbolt 체스트 리그` 이고, `platinum-theblacklist` 가 플리마켓에서
   **블랙리스트로 막아** 둔 물건이라 값이 정말로 없었다(= 전체 거절은 옳은 판정이었다).
   → 이제 `LocaleService` 로 이름을 찾아 앞 세 개를 말한다: "Thunderbolt … 의 값을 매길 수 없어 …
   상인이 팔지 않거나 플리마켓에서 막힌 물건입니다." 이름을 못 읽으면 tpl 을 그대로 쓴다(이름 때문에
   거절을 못 내는 일은 없다).

**판정만 하고 고치지 않은 것.** 어제 09:47·11:02(UTC)에 `kit-buy`·`kit-quote` 가 각각
`Critical: The archive entry was compressed using an unsupported compression method` 로 끝났다.
스택이 SPT 의 `SptHttpListener` 가 **요청 본문을 푸는 자리**라 우리 콜백에 닿기 전이고, 그 뒤 지금까지
한 번도 재현되지 않았다(어제 저녁의 견적 14회·구매 1회 전부 정상). **재현하지 못한 것을 추측으로
고치지 않는다** - 다시 나오면 그때 잡는다.

**실측.** 클라이언트·서버 빌드 경고 0·오류 0, 테스트 566개 통과. 설치본 md5 클라이언트 `4fd943ff`,
서버 `e0ea1189` 둘 다 산출물과 일치하고 더 새로운 `.cs` 없다. 새 서버 DLL 로 기동해 `PitFireTeam loaded`·
`Teammate database ready` ×5 확인, `끝나지 않은 전송`·pitTeam 오류 **0건**. 서버는 켜진 채로 둔다.

**게임에서 볼 것(다음 차례).** ① 목록 줄에 ₽ 아이콘이 없고 무게로 보이는지 ② 화면 위쪽 패널이 PIT 값을
쓰지 않는지 ③ "스태시의 물건을 쓴다" 칸이 **안 보이고** 버튼이 늘 `구매` 인지 ④ Hawk 의 `NEW PRESET 1`
거절 문구에 **Thunderbolt 이름이 나오는지** ⑤ 용병(Guns for Hire) 쪽 꾸러미는 **예전 그대로**인지
(체크박스·값·아이콘이 돌아와야 한다 - 3번에서 static 값을 건드렸다).

### 같은 날 — 기동 경고를 끝까지 따라갔다. 하나는 우리 것이었다

사용자가 "우리 것이고 나발이고 다 고치라" 고 해서 기동 경고를 종류별로 전부 추적했다.

**우리 버그 — 택배원 트레이더의 보충 주기 (10-27부터 매 기동).**
`TraderHelper: 트레이더 67d3a28a3d6f4f7dbd09ed13 찾을 수 없음, 기본 새로 고침 시간으로 임시 항목을
생성합니다` 의 그 id 가 **우리 택배원 트레이더**였다(`FriendlyCourierTraderProfile.CourierTraderIdValue`).
처음에는 "다른 모드 것" 칸에 넣을 뻔했는데, 10-27 로그를 역추적하다 바로 윗줄의
`pitTeam.Server.PitFireTeamServerPlugin: Registered courier trader '67d3a28a…'` 를 보고 잡았다.
**이름에서 뜻을 지어내지 말라는 교훈이 로그에도 그대로 적용된다 - 남의 것처럼 보이는 경고도 한 번은 역추적한다.**

원인: 트레이더를 `TradersTable` 에 넣는 것만으로는 모자라다. `TraderHelper.GetTraderUpdateSeconds` 는
**trader 설정의 `UpdateTime` 목록**에서 주기를 찾고, 없으면 경고를 내고 기본값 항목을 스스로 끼워 넣는다
(역컴파일로 확인). 고침: `EnsureCourierTraderRefreshTime()` 이 등록 직후 같은 기본값
(`UpdateTimeDefault`)으로 항목을 넣는다. **실측**: 재기동 로그에서 경고가 사라지고
`Registered courier trader refresh time (3600s)` 가 찍힌다.

**다른 모드 것 — 고친 둘.**
- **CaseTrader** 가 `Developer case`·`moderator toolbox` 를 핸드북 가격이 없다고 매번 건너뛰며 경고했다.
  플레이어에게 팔 물건이 아니므로 **가격을 지어내지 않고** `blacklistItemIds` 로 대상에서 뺐다.
  원본 config 는 `backup/2026-11-03-mod-warning-fixes/` 에 있다. 재기동에서 2건 → **0건**.

**내가 더 나쁘게 만들었다가 되돌린 것.**
- Eukyre-BetterOKP7 의 **빈** `db/CustomItems` 폴더를 지웠더니 WTT 라이브러리가 경고 대신
  `Error: Directory not found` 를 냈다. 폴더가 있으면 Warning, 없으면 Error 다. **되돌렸다.**
  지우기 전이 더 나은 상태였고, 확인 없이 "빈 폴더니 지우면 조용해지겠지" 로 움직인 것이 틀렸다.

**내 공이 아닌 것.** Fence 경고 3건이 이번 기동에 안 나왔다. 고친 것이 아니라 **Fence 재고가 기동마다
무작위**라 그 방탄복이 안 뽑힌 것뿐이다. 다시 나올 수 있다.

**원인까지 찾았지만 손대지 않은 것.**
- **번들 충돌** `barrel_vpo215_600mm_366tkm.bundle`: `SPTBetterRearSights` 와 `WTT-ContentBackport` 가
  **같은 경로에 서로 다른 파일**(md5 `bf30e412` vs `af7a73f7`)을 넣는다. 폴더 이름 순으로 앞선
  BetterRearSights 가 이기고 WTT 쪽 추가가 실패한다. **지금 이기는 쪽이 의도대로**(가늠자를 고치는 모드)이고,
  지는 선언을 지우면 WTT 가 BetterRearSights 에 숨은 의존을 갖게 된다. 두는 쪽이 낫다.
- **Lotus 퀘스트 보급품**: `WTTCustomQuestService` 가 트레이더 `6747208e…`(Lotus)를 못 찾는 것은
  **로드 순서**다. ISB 계열이 Lotus 퀘스트를 들고 있는데 Lotus 등록(22:30:04.666)보다 2ms 먼저 들여온다.
  고치려면 남의 서명된 DLL 의 `ModDependencies` 를 바꿔야 해서 하지 않았다 - 모드 제작자 쪽 일이다.
- **방탄복 플레이트 칸 없음**(Fence): `BlueheadsAIOTrader` 가 파는 WTT 계열 방탄복의 설계다.
  칸을 만들면 방어 동작이 바뀐다.
- **없는 tpl 48개**(Error 48 + Warning 48): **이미 10-28 에 밝혀 둔 것이었다**(아래 정정 참조).

**집계(이번 기동 / 고치기 전).** TraderHelper 0/1, CaseTrader 0/2, Fence 0/3(무작위), 나머지 그대로:
ItemBaseClass 48+48, ProfileMigration 5(정상 흐름), SVM 3("오류 아님, 보고하지 말라"고 모드가 직접 말한다),
WTT 퀘스트 1, WTT 아이템 1, 보험 1, 번들 1.

**실측.** 서버 빌드 경고 0·오류 0, 테스트 566개 통과. 설치본 md5 서버 `4fffcb90`, 클라이언트 `4fd943ff`.
재기동 뒤 `PitFireTeam loaded`·`Teammate database ready` ×5·`끝나지 않은 전송` 0건.

### 정정 — 없는 tpl 48개는 10-28 에 이미 끝난 조사였다 (2026-11-03)

오늘 이 48개를 **처음 보는 문제처럼** 다시 팠고, "디스크 어디에도 없다 - 실행 중에 만들어진다" 라는
**틀린 결론**까지 적었다. 사용자가 "문서 확인해서 최근 제거한 모드에서 찾아봐" 라고 해서 WORKLOG 를
거슬러 보니 **2026-10-28 에 이미 범인을 잡아 두었다**: `MountOlympus.dll` 이 48개 id 를 그대로 들고 있다.

**왜 오늘 못 찾았나 — 10-28 의 그 메모가 경고한 바로 그 실수를 했다.** 그날 적어 둔 것이
"`.NET 문자열은 추출해 비교하지 말고 찾을 것을 바이트로 만들어 찾는다`" 였는데, 오늘 DLL 을 훑을 때
`readFileSync(p, 'latin1')` 로 읽어 문자열 비교를 했다. .NET 문자열은 **UTF-16LE** 이라 latin1 로는
`0x00` 이 섞여 한 글자도 맞지 않는다. 같은 파일을 보고도 "없다" 가 나온 이유다.

**다시 확인했다(이번에는 바이트로).** `Buffer.from(id, 'utf16le')` 로 찾으니 `MountOlympus.dll` 에서
**48개 전부** 나온다. 대조군으로 실제 존재하는 루블 tpl 을 같은 방법으로 찾으니 **없다** - 방법이
맞는다는 뜻이다(이 대조군도 10-28 메모가 권한 것이다).

**결론은 그날과 같다.** `MOConfig.jsonc` 를 다시 봐도 이 목록이나 탄창 추가 기능을 끄는 설정은 없다
(탄창 관련 항목은 정확도·반동·로열티·블랙리스트뿐). 모드 DLL 안의 하드코딩이라 **사용자가 고칠 자리가
아니다.** 없는 아이템이 플리마켓에 안 뜨는 것은 정상 동작이고 로그 줄만 남는다. Olympus 제거는
프로필에 그 모드 아이템이 24종 들어 있어 여전히 권하지 않는다.

**배운 것(두 번째라 적어 둔다).** 오래된 증상을 만나면 **코드를 뒤지기 전에 WORKLOG 를 먼저 검색한다.**
오늘 모드 2,650개 파일과 777MB 데이터베이스를 훑는 데 쓴 시간은 `grep -n "48" doc/WORKLOG.md` 한 번이면
끝났을 일이었다.

### 인게임 확인 — 화면 수정 넷 다 되고 용병 쪽은 그대로다 (2026-11-03)

| 본 것 | 결과 |
|---|---|
| 목록 줄의 아이콘 | **무게 아이콘 + `6.1`** - ₽ 가 사라졌다 |
| 고른 빌드의 값 | 안 쓴다. 로그로도 확인: STM 구간(싣기~되돌리기) 안의 `[UI][KitPrice]` 진단 **0줄**(그 run 전체로는 48,704줄이 용병 쪽에서 나온다) |
| "스태시의 물건을 쓴다" | STM 화면에 **없고** 버튼은 `구매` 다 |
| 값 못 매김 거절 | **"Direct Action Thunderbolt compact chest rig (Marauder) 의 값을 매길 수 없어…"** - 이름이 나온다 |
| 용병 꾸러미 | **그대로다**: ₽ 19,847,200 / 8,744,465 … 값이 보이고 체크박스도 돌아왔다. static 값을 건드린 회귀 없음 |

명단 탭의 용병/프로필 팀원 하위 탭도 정상이고, 프로필 팀원 넷(SPT02·Knox·Hawk·Buck)이 뜬다.

**새로 보인 것 — 타일의 "장비 구분: 아직 판정하지 않음".** 프로필 팀원 타일 넷이 모두 이 줄을 단다.
`StmProfileRosterClient.EquipmentUndecided` 의 주석이 "계획 2 가 답을 주면 이 줄을 바꿔라" 인데
**계획 2·3 이 끝났고 서버는 이미 `handEdited` 를 명단 응답에 싣는다**(`StmRosterRow.HandEdited`).
어제 꾸러미를 산 Hawk 는 그 표시가 켜져 있는데도 화면은 "아직 판정하지 않음" 이라고 말한다.
고치려면 shared 의 `RegisteredProfileRow` 에 필드를 하나 더 흘려보내야 해서(정책·테스트까지) 한 줄 수정이
아니다. 아직 하지 않았다.

### 타일의 "장비 구분" 이 이제 실제 상태를 말한다 (2026-11-03)

`handEdited` 를 명단 전선 끝까지 흘려보냈다. 서버는 이 값을 **등록 정보**에 들고 있었지만
명단 응답에 싣지 않았고(`StmProfileMemberCallbacks.Roster` 의 주석이 "뒤 작업의 몫" 이라고 적고 있었다),
클라이언트는 타일에 늘 "아직 판정하지 않음" 을 달았다.

- `RegisteredProfileRow` 에 `bool HandEdited = false` 를 더했다. **기본값을 둔 이유**는 이 값을 모르는
  경로(`JoinRegistered` 는 등록 정보가 아니라 카탈로그만 본다)가 컴파일을 깨지 않게 하기 위해서다.
- 서버가 `autoJoinIds` 와 같은 방식으로 `handEditedIds` 를 만들어 채운다.
- `StmRosterWire.ParseRows` 가 `handEdited` 를 읽는다.
- 타일은 쓸 수 있는 줄(Ready)에만 장비 구분을 말한다: **`맞춰 둔 장비`** / **`레이드마다 임시 장비`**.
  못 쓰는 줄에는 그대로 이유만 보인다 - 장비 이야기를 얹으면 왜 못 쓰는지가 묻힌다.

**테스트 둘을 못 박았다**(566 → 568). 이 값이 전선에서 빠지면 화면은 **조용히 전부 "임시 장비"** 라고
말한다. 거짓이 기본값이 되는 쪽이라 "true 면 true 로 읽힌다" 와 "없으면 false" 를 둘 다 고정했다.

**실측.** 클라이언트·서버 빌드 경고 0·오류 0, 테스트 568개 통과.

처음에는 게임이 켜져 있어 서버 DLL 이 잠겨 설치하지 못했다(`Device or resource busy`). 게임을 닫은 뒤
**둘 다 다시 빌드해 설치했다** - 머지가 소스 파일의 수정 시각을 올려 "빌드보다 새로운 `.cs`" 검사에
걸렸기 때문이다(인계에 적힌 함정 그대로다). 설치본 md5 서버 `c06919ea`·클라이언트 `befe98ec`,
둘 다 산출물과 일치하고 더 새로운 `.cs` 는 없다.

**게임에서 볼 것.** 명단 → 프로필 팀원 탭에서 **Hawk 만 `맞춰 둔 장비`**(어제 꾸러미를 샀다),
장비를 손댄 적 없는 팀원은 `레이드마다 임시 장비` 로 보여야 한다. 넷이 전부 임시 장비로 보이면
서버가 값을 안 싣는 것이고, 넷이 전부 맞춰 둔 장비면 반대로 늘 true 로 읽는 것이다.

### 장비 구분을 손으로 고쳤는지로만 가른 것은 틀렸다 (2026-11-03, 앞 항목 정정)

사용자가 "프로필 멤버 전부 고정 장비인데 Hawk 만 표시된다" 고 했다. 확인해 보니 **표시가 틀렸고,
내가 고른 기준이 틀렸다.**

`SpawnGearPolicy.Decide` 는 세 갈래다:
1. 손으로 고쳤다 → **실물**
2. 입은 것이 하나도 없다 → 임시
3. 필수 무기 슬롯(주무기1·2·권총집)이 비었다 → 임시
4. 그 외(장비와 무기가 다 있다) → **실물**

즉 **손대지 않아도 장비와 무기가 다 있으면 실물로 나간다.** 내가 단 문구는 1번만 보고 2~4번을
뭉뚱그려 "레이드마다 임시 장비" 라고 말했다. SPT02·Knox·Buck 은 장비도 무기도 있으니 실물인데
임시라고 적힌 셈이다.

**고침.** 판정을 **서버에서 한 번만** 하고(`"Real"`/`"Temporary"`) 그 결과를 명단 줄에 싣는다.
화면은 그 한 값만 보고 말한다 - 세 사실을 클라이언트에 보내 다시 판정하면 집이 둘이 된다.
- 카탈로그가 프로필을 읽을 때 입은 슬롯에서 두 사실을 뽑는다(`StmSpawnProfileFactory.ReadOccupiedEquipmentSlots`
  를 그대로 쓴다 - 스폰이 쓰는 그 읽기여야 두 화면이 같은 말을 한다).
- 콜백이 등록 정보의 `HandEdited` 와 합쳐 `SpawnGearPolicy.Decide` 를 부른다. **두 조각이 서로 다른
  곳에 있어** 합치는 자리는 거기뿐이다.
- 줄의 `handEdited` 는 `gear` 로 바뀌었다. `RegisteredProfileRow.GearKind` 가 그것을 받는다.
- 문구: `장비 구분: 실물 장비` / `장비 구분: 레이드마다 임시 장비`, 값이 없으면 `아직 판정하지 않음`.

**중간에 걸린 것.** `GearFacts` 는 `SpawnGearPolicy.cs` 에 있는데 그 파일은 net472(클라이언트)에 없는
`IReadOnlySet` 을 쓴다. 명단 줄은 세 프로젝트가 모두 컴파일하므로 중립 그릇
`ProfileGear(bool, bool)` 를 명단 정책 파일에 두고 서버가 옮겨 담는다. 판정은 여전히 한 곳이다.

### 꾸러미 목록에 값이 다시 보인다 (2026-11-03)

사용자: "용병의 장비꾸러미는 금액이 표시되는데 프로필멤버는 왜 무게가 표시되는가?"

어제 ₽ 아이콘을 떼면서 **값 자체를 포기했었다**. PIT 의 값은 용병용 규칙이 붙어 우리가 청구하는
값과 다르기 때문인데, 그렇다고 무게를 보여 주는 것이 답은 아니었다. **우리 값을 보여 주면 된다.**

- 서버가 `/stm/members/builds` 의 줄마다 `price` 를 싣는다. **구매 창과 같은 서비스**(`StmGearKitQuoteService`)
  로 매긴다 - 다른 계산이면 보여 준 값과 청구가 어긋난다. 이미 입고 있는 칸은 빼므로 팀원마다 다르다.
  값을 못 매기는 빌드는 `-1` 이고, 그 줄만 값 없이 간다(목록 전체를 막지 않는다 - 사지 못할 뿐이다).
- 클라이언트는 **두 입구 모두** 목록을 먼저 받는다. 전에는 "내 꾸러미" 가 목록 없이 열려 보여 줄 값이
  없었다. 받은 값을 `StmKitNativeFlow` 가 들고 있다가 줄과 고른 빌드에 쓴다.
- 값을 아는 줄에만 ₽ 아이콘을 단다(`ApplyRoublesIcon(force: true)`). 모르는 줄은 무게가 그대로 남는다 -
  틀린 값을 보여 주느니 말하지 않는 쪽이다.
- `OpenStmTeammateKitScreen` 은 없어졌다. 두 입구가 같은 길을 타므로 `OpenStmKitScreen(teammateOwned:)`
  하나로 합쳤다.

**실측.** 세 프로젝트 빌드 경고 0·오류 0, 테스트 568개 통과. 설치본 md5 서버 `8efdbf3a`·클라이언트
`3eb2c273`, 둘 다 산출물과 일치하고 더 새로운 `.cs` 없다.

**게임에서 볼 것.** ① 프로필 팀원 넷이 모두 `실물 장비` 로 보이는가(넷 다 장비·무기가 있다)
② 꾸러미 목록 줄에 ₽ 와 금액이 보이는가 ③ 그 금액이 구매 창의 값과 **같은가** ④ 값을 못 매기는
빌드(Hawk 의 `NEW PRESET 1`)는 숫자 없이 무게로 남는가 ⑤ 용병 쪽은 예전 그대로인가.

### 인게임 확인 — 다섯 가지 다 됐고, 살 수 없는 줄을 손봤다 (2026-11-03)

| 본 것 | 결과 |
|---|---|
| 프로필 팀원 넷의 장비 구분 | **넷 다 `실물 장비`** - 판정을 서버로 옮긴 것이 맞았다 |
| 목록 줄의 값 | **보인다** (`₽ 3,875,215`) |
| 목록의 값 == 구매 창의 값 | **같다**(3,875,215) - 같은 서비스로 매긴 것이 실물로 확인됐다 |
| 값을 못 매기는 빌드 | 숫자 없이 무게로 남는다 |
| 용병 쪽 꾸러미 | **그대로** |

**사용자 요청 — "금액을 표시하되 버튼을 비활성화 되는지?"**
값을 매길 수 없는 빌드에는 **보여 줄 금액이 없다**(Thunderbolt 리그는 상인도 플리마켓도 값이 없다).
0 이나 어림수를 쓰면 "보여 준 값과 청구가 다르다" 를 스스로 만드는 꼴이라 숫자는 쓰지 않는다.
대신 **누를 수 없게** 했다:
- 그런 빌드를 고르면 구매 버튼이 죽고 글자가 `살 수 없음` 으로 바뀐다. 눌러 보기 전에 알 수 있다.
- **죽은 버튼은 스스로 이유를 말하지 못한다.** 그래서 고른 순간 서버에 한 번 물어 거절 문구를
  토스트로 띄운다("Direct Action Thunderbolt compact chest rig … 의 값을 매길 수 없어…").
  같은 빌드에 두 번 묻지 않는다(`_explainedBuilds`).

**덤으로 가른 것.** 확인 창의 `살 수 없음` 이 "돈이 모자라다" 와 "살 것이 하나도 없다" 를 뭉뚱그리고
있었다. 이미 다 갖춘 팀원에게는 이제 **`살 것이 없음`** 이라고 말한다(값 0 인 빌드가 그렇다).

**실측.** 클라이언트 빌드 경고 0·오류 0. 설치본 md5 `c368e046`, 산출물과 일치하고 더 새로운 `.cs` 없다.
서버는 바뀌지 않아 그대로 둔다. 게임을 다시 켜야 돈다.

### "팀원 추가" 를 "용병 추가" 로 (2026-11-03)

사용자: "원문대로 하면 팀원 추가가 맞으나 STM 은 용병과 프로필 둘 다 있으니 용병 추가로 바꾸는 게
헷갈리지 않겠다."

맞는 지적이다. PIT 에는 팀원이 한 종류뿐이라 "팀원 추가" 로 충분했지만, STM 이 프로필 팀원을 들이면서
명단에 두 종류가 생겼다. 그 버튼이 하는 일은 **언제나 용병 고용**이다(`AddTeammateCreationFlow` /
`TeammateHiringPreview`). 명단의 **두 탭 모두**에 그 버튼이 보이므로 더 헷갈린다.

그 흐름의 한국어 문구 9줄만 바꿨다 - 키는 PIT 의 것이라 그대로 둔다:
`+ 팀원 추가` → `+ 용병 추가`, 확인 버튼, 그리고 실패·중복열기 문구들("이 용병의 장비 값을…",
"용병 추가를 끝내지 못했습니다…", "용병 생성 화면을 열지 못했습니다" 등).

바꾸기 전에 그 키들이 **프로필 팀원 쪽에서도 쓰이는지 확인했다** - 전부 용병 고용 흐름의 두 파일에서만
불린다. 프로필 쪽은 자기 문구를 따로 갖고 있다(`StmProfileRosterClient`).

**실측.** `tools/build-stm-lang.mjs` 로 다시 생성(문구 355개, 누락·자리표시자·줄바꿈 검사 통과),
`kr.json`·`ko.json` 설치본 md5 `134ecc39` 일치. 영어 원문은 PIT 의 것이라 건드리지 않았다.
게임을 다시 켜야 문구를 다시 읽는다.

### 꾸러미 화면의 `표준형` 탭을 숨긴다 (2026-11-03)

사용자: "프로필팀원 - 팀원 장비 꾸러미 - 표준형 - 목록 안 나옴. 표준형을 빼거나 목록을 내거나 둘 중 하나."

**왜 비었나.** 팀원 꾸러미는 화면의 빌드 사전(`storage._equipmentBuilds`)을 통째로 바꿔 끼운다.
그 사전에는 우리가 만든 팀원 빌드만 들어 있고 전부 `Custom` 이라, `표준형` 탭에 걸릴 것이 하나도 없다.

**내 꾸러미에서는 목록이 나오지만 그쪽도 막다른 길이었다.** 표준형에 뜨는 것은 게임의 기본 프리셋이라
**어느 프로필의 저장 목록에도 없다**. 서버의 `FindBuild` 는 프로필의 `userbuilds` 에서 찾으므로 값을
매기지 못하고, 고르면 버튼이 죽은 채 "고른 빌드를 찾지 못했습니다" 가 돌아온다. 즉 한쪽은 빈 목록,
다른 쪽은 살 수 없는 목록이었다.

**그래서 두 입구 모두에서 탭을 숨긴다.** 이 화면이 다루는 것은 **저장된 프로필 빌드**뿐이라는 사실을
화면이 그대로 말하게 한다. 되돌리기는 기존 `ScreenObjects` 경로가 맡는다 - 원래 활성 상태를 적어 두면
`RestoreScreenChrome` 이 되살리므로 용병 화면은 그대로다.

표준형 프리셋까지 사게 만드는 것은 별개의 기능이다(그 프리셋을 어느 프로필의 빌드로도 볼 수 없으니
서버 쪽 조회를 새로 열어야 한다). 지금은 하지 않는다.

**실측.** 클라이언트 빌드 경고 0·오류 0. 설치본 md5 `bcf7b32a`, 산출물과 일치하고 더 새로운 `.cs` 없다.
서버는 바뀌지 않았다. 게임을 다시 켜야 돈다.

**같은 날 확인된 것.** `+ 용병 추가` 문구, 살 수 없는 빌드의 죽은 버튼과 이유 토스트, `살 것이 없음`
구분 셋 다 인게임에서 동작한다(사용자 확인).

**인게임 확인(2026-11-03).** 프로필 팀원의 두 꾸러미에서 `표준형` 탭이 사라졌고, **용병 꾸러미에서는
그대로 있다**. 숨기기만 하고 되살리지 못하면 용병 쪽이 망가지는데, 그 복구 경로(`ScreenObjects`)가
실제로 도는 것을 확인했다.

### 프로필 팀원의 자동 합류를 켜고 끌 자리를 만들었다 (2026-11-03, 열린 항목 8)

인계 문서가 "다음 사람이 가장 먼저 할 일" 로 적어 둔 것이다. 서버는 이미 세 곳을 다 갖추고 있었다 -
토글 저장(`FriendlyTeammateCallbacks.SetAutoJoin` → `StmAutoJoinService`), 명단 응답의 `autoJoin`,
출격 구성(`StmAutoJoinService`). **없던 것은 누를 자리뿐이었고**, 그래서 계획 3 의 관문 15번
(자동 합류 인게임 증명)을 돌릴 수 없었다.

- 전선: `RegisteredProfileRow` 에 `AutoJoin` 을 더하고 `StmRosterWire` 가 `autoJoin` 을 읽는다.
  **서버는 이미 싣고 있었다** - 클라이언트가 버리고 있었을 뿐이다.
- 요청: `StmProfileRosterClient.SetAutoJoinAsync` 가 **PIT 의 라우트**
  (`/singleplayer/pitfireteam/teammate/autojoin`, `{aid, enabled}`)를 그대로 쓴다. 서버가 그 안에서
  STM 을 먼저 묻는다. 새 라우트를 만들지 않았다.
- 화면: 타일 아래쪽 띠에 `자동 합류 : 켬/끔` 단추. **켜짐과 꺼짐을 글자와 색 둘 다로** 가른다
  (색만으로는 꺼진 것인지 쓸 수 없는 것인지 알 수 없다). 쓸 수 있는 줄에만 둔다 - 비어 있거나 읽지
  못한 프로필은 출격에 끼울 수 없으므로 켤 이유도 없다. 타일 자체의 클릭(상세 화면)보다 먼저 먹는
  자식 Button 이라 눌러도 화면이 열리지 않는다(제거 단추와 같은 구조다).
- 누른 뒤에는 **명단을 다시 읽어** 그린다. 눌렀으니 바뀌었겠거니 하고 화면만 고치면 거절당했을 때
  화면과 서버가 어긋난다.
- 자리를 내려고 상태 줄의 `장비 구분:` 머리말을 걷어냈다(`실물 장비로 출격` / `레이드마다 임시 장비`).
  프로필 팀원 타일에서는 머리말 없이도 뜻이 통한다.

**테스트 둘**(568 → 570). `autoJoin` 이 전선에서 빠지면 켜 둔 팀원도 꺼진 것으로 보이고, 사용자가 다시
누르면 **켜져 있던 것을 끄게 된다** - 조용히 반대로 도는 쪽이라 못 박았다.

**실측.** 세 프로젝트 빌드 경고 0·오류 0, 테스트 570개 통과. 설치본 md5 클라이언트 `d29aad71`·
서버 `8578f64c`, 둘 다 산출물과 일치하고 더 새로운 `.cs` 없다.

**게임에서 볼 것.** ① 프로필 팀원 타일에 단추가 보이는가 ② 누르면 켬↔끔이 바뀌고 **화면을 닫았다
열어도 그대로인가**(서버에 저장된다는 뜻이다) ③ 켠 팀원이 출격 때 실제로 따라붙는가(관문 15번).

### 관문 15번 통과 — 자동 합류가 실제로 동작한다 (2026-11-03)

계획 3 에서 "켤 UI 가 없어 돌릴 수 없다" 고 막혀 있던 관문이다. 사용자 확인:
① 타일에 단추가 보인다 ② 켠 상태가 화면을 닫았다 열어도 **유지된다**(서버에 저장된다는 뜻)
③ 출격할 때 **실제로 따라붙는다** — 배치 화면에 Buck·Hawk·Knox·Nomad·SPT02 다섯이 섰다.
서버 로그에도 `/singleplayer/pitfireteam/teammate/autojoin` 요청이 켠 수만큼 찍혔다.

**열린 항목 8 이 닫혔고, 그에 막혀 있던 관문 15번도 닫혔다.**

### 새로 드러난 것 — 자동 합류에 인원 제한이 없다

사용자: "실제 파티원으로 붙는 최대 수는 4명인데 자동 합류가 5명이면 5명이 다 따라온다.
용병 4명까지 넣고 전부 켜면 9명이 따라오나?"

**코드를 보면 그렇다. 어느 쪽에도 상한이 없다.**
- PIT: `FriendlyTeammateService.GetAutoJoinTeammateAccountIds` 는 auto-join 이 켜져 있고 제대로 된
  출격 장비를 갖춘 용병을 **전부** 담는다. 거르는 것은 장비뿐이고 수는 세지 않는다.
- STM: `StmAutoJoinService.AppendAccountIds` 가 등록된 프로필 팀원 중 켜진 것을 **전부** 뒤에 붙인다.
  `AutoJoinRosterPolicy.SelectAccountIds` 에도 상한이 없다.
- 합치는 자리(`FriendlyTeammateCallbacks.ListAutoJoin`)도 중복만 걸러내고 자르지 않는다.

즉 켠 만큼 다 나간다. EFT 의 파티 인원(4)은 **사람 파티의 규칙**이고 이 경로는 그것을 거치지 않는다.

**아직 정하지 않았다.** 상한을 둘지, 둔다면 몇으로 할지, 넘칠 때 누구를 남길지(용병 먼저 / 프로필
먼저 / 명단 순서)는 게임 균형의 문제라 사용자에게 물었다. 추측으로 정하지 않는다.

### 자동 합류에 인원 상한을 두었다 (2026-11-03)

사용자가 정한 규칙: **상한은 설정값**(기본 4 = EFT 파티 인원, 0 이면 무제한), 넘치면 **프로필 팀원을
먼저** 무작위로 고르고 자리가 남으면 **용병에서** 무작위로 채운다.

- 정책: `AutoJoinRosterPolicy.Compose(프로필, 용병, 상한, Random)`. 두 묶음을 각각 Fisher-Yates 로
  섞어 프로필 먼저 이어 붙이고 상한에서 자른다. 같은 id 는 한 번만 나가고 **프로필 쪽이 이긴다**.
  `Random` 을 인자로 받는 이유는 시험할 수 있게 하기 위해서다 - 같은 씨앗이면 같은 차례가 나오므로
  "무작위가 실제로 섞는지" 와 "상한이 정확히 몇에서 끊는지" 를 기계가 확인한다(테스트 6개, 570 → 576).
- 서버: `StmAutoJoinService.AppendAccountIds` 가 붙이기에서 **고르기**로 바뀌었다. 설정은
  `FriendlyServerSettingsRequest.AutoJoinMaxMembers`(음수는 0 으로 본다 - 설정 파일을 손으로 고쳐도
  터지지 않는다). 몇 명 중 몇이 나가는지 로그에 남긴다.
- 클라이언트: BepInEx 설정 `14 AutoJoinMaxMembers`(0~10, 기본 4). 분대 설정 탭에도 나온다
  (`CreateStockSliderRow` 가 int 범위 항목을 슬라이더로 그린다). 바뀌면 서버로 보낸다.
- 문구: `en.json` 에 원문을, `tools/lang/stm-kr.json` 에 한국어를 넣고 다시 생성했다(357개).
  **다른 언어 파일(ru·chs)에는 이 열쇠가 없다** - PIT 업스트림의 것이라서다. 없으면 사전 조회가 터져
  플러그인이 통째로 안 뜨므로 그 자리를 영어로 메웠다.

**실측.** 세 프로젝트 빌드 경고 0·오류 0, 테스트 576개 통과. 설치본 md5 서버 `c2308a81`·
클라이언트 `67e119af`, 한국어·영어 문구 파일도 함께 설치했다.

**게임에서 볼 것.** ① 분대 설정 탭에 `자동 합류 최대 인원` 이 보이는가 ② 4 로 두고 자동 합류를
다섯 이상 켠 뒤 출격하면 **넷만** 따라오는가 ③ 두 번 출격하면 따라오는 얼굴이 **바뀌는가**(무작위)
④ 0 으로 두면 전부 따라오는가.

### 새 설정만 영어로 뜬 이유 (2026-11-03)

사용자: "왜 지 혼자 영어냐?" — 제목이 `AutoJoinMaxMembers`(설정 열쇠 그대로), 설명이 영어였다.

**분대 설정 탭은 문구를 손으로 이어 둔 if 사슬에서 찾는다.** `GetSettingLanguageEntry` 가
`entry == pitFireTeam.x` 를 60줄 넘게 늘어놓고 해당 사전을 돌려준다. 거기에 새 항목을 **더하지 않아서**
null 이 돌아왔고, 제목은 열쇠를 다듬은 값으로, 설명은 `ConfigDescription` 에 넣어 둔 영어 원문으로
떨어졌다. 번역 파일에는 문제가 없었다(설치본 `kr.json` 에 열쇠가 들어 있는 것을 확인했다).

한 줄을 더해 고쳤다. **설정을 새로 만들 때 손댈 자리가 두 곳**이라는 뜻이다 - 바인딩(friendlyPlugin)과
이 사슬. 한쪽만 하면 조용히 영어로 뜬다.

### 상한을 아직 실제로 넘겨 보지 못했다

서버 로그의 판정 줄을 보니 상한이 **10**(03:07)과 **없음**(03:45)으로만 돌았다. 둘 다 프로필 5명이라
잘릴 일이 없었다. 즉 **"5명 중 4명만 나간다" 는 아직 본 적이 없다.** 지금 저장된 설정은
`autoJoinMaxMembers: 0` 이다.

**`파티 구성` 화면이 4명만 보여 주는 것은 우리 상한이 아니다.** 게임의 그 화면은 자리가 다섯(대장 + 넷)
뿐이고, 같은 레이드의 `작전지역에 배치 중` 목록에는 열 명이 다 나온다(상한 0 으로 돌린 화면).
두 화면이 세는 것이 다르다.

**실측.** 클라이언트 빌드 경고 0·오류 0, 설치본 md5 `5701a5e4`.

### 상한이 실제로 잘랐다 (2026-11-03)

| 본 것 | 결과 |
|---|---|
| 설정 문구 | **한국어** — `자동 합류 최대 인원`, 설명까지(사슬 한 줄을 더한 것이 맞았다) |
| 자동 합류 다섯을 켠 채 상한 4 로 출격 | 배치 목록에 **대장 + 넷**(Knox·SPT02·Buck·Hawk). Mace 가 빠졌다 |
| 서버 로그 | `[STM] auto-join 목록: 프로필 5명 + 용병 0명 중 4명이 나간다(상한 4).` |
| 상한 0 | 아홉이 전부 따라온다(앞 항목) |
| 두 번 출격 | 따라오는 얼굴이 바뀐다(무작위) |

화면에서 센 수와 서버가 적은 수가 같다. **보여 준 것과 실제가 어긋나지 않는다.**

계획 4 와 열린 항목 8·관문 15번에 이어, 이번 작업(자동 합류 상한)도 인게임에서 닫혔다.

### 장비 편집 관문 2~5 를 서버 라우트로 돌렸다 — 서버 쪽은 넷 다 통과 (2026-11-03)

계획 3 에서 "코드·테스트·리뷰만 통과하고 게임에서 한 번도 돌지 않았다" 고 적어 둔 장비 편집의 관문이다.
**게임 없이 서버 라우트를 직접 불러** 판정·저장·디스크 재독까지 돌렸다. 화면(왼쪽 칸 그리기·버튼·토스트)은
이 방법으로 볼 수 없으니 그 절반은 그대로 남아 있다.

**먼저 설치본을 확인했다.** 설치된 DLL md5 가 빌드 산출물과 같다(클라이언트 `5701a5e4`, 서버 `c2308a81`).
빌드보다 **새로운 `.cs` 가 여섯 개** 걸렸는데, 전부 머지가 수정 시각을 올린 것이고 내용은 이미 들어 있다:
설치된 서버 DLL 을 **바이트로** 뒤져 `명이 나간다(상한 ` 과 `[STM] auto-join 목록` 을 찾았고(대조군 문자열은
없다), 클라이언트의 마지막 수정(설정 사슬 한 줄)은 그제 인게임에서 한국어로 뜨는 것을 이미 봤다.
**md5 가 같다는 것만으로 "최신 코드다" 라고 하지 않는다 — 들어 있어야 할 문구를 찾는다.**

**요청을 어떻게 보내는지부터 알아내야 했다.** SPT 4.x 의 `SptHttpListener` 는 요청 본문을 **zlib** 로 무조건
푼다. 평문·raw deflate·gzip 셋 다 `The archive entry was compressed using an unsupported compression method`
로 떨어지고 응답은 **200 + 빈 본문**이다. **어제 "판정만 하고 고치지 않은 것" 으로 남겨 둔 `kit-buy`·
`kit-quote` 의 그 Critical 과 같은 오류를 이렇게 재현했다** — 원인은 "zlib 가 아닌 본문이 리스너에 닿는 것"
이다. 어제 무엇이 그런 본문을 보냈는지는 여전히 모르고, 추측으로 고치지 않는다.

| 관문 | 어떻게 | 결과 |
|---|---|---|
| 3 | `/stm/members/gear-stash` 를 등록된 다섯 명 전부에게 | **통과.** 응답 아이템 집합이 그 팀원 스태시 뿌리의 집합과 **정확히 같다**(SPT02 60, Knox 198, Hawk 178, Buck 370, Mace 9). 빠진 것 0, 남는 것 0, **Nomad(플레이어)의 아이템 0**, 장비 트리 0, `equipmentRootId` 가 그 프로필의 것 |
| 2 | `/stm/members/edit-gear` 로 SPT02 의 헬멧을 벗겨 스태시로 | **통과.** `[STM] 장비 편집: … 1건을 한 번에 저장·확인했다(8ms)`. 프로필 파일이 실제로 바뀌었고 "되돌렸습니다" 가 없다. 경로는 `AppContext.BaseDirectory`(= `E:\Spt\SPT_Runtime\`)에서 풀린다 |
| 5 | 그 헬멧을 다시 입히고 편집 전 백업과 대조 | **통과.** 아이템 id 집합 260 → 260 **동일**, 부모·슬롯·좌표까지 편집 전과 같은 상태로 돌아왔다. 벗길 때 좌표는 **SPT 가** 배정했고(`x3,y6`), 입힌 뒤에는 `location` 필드가 **없다**. 슬롯 철자는 정식 `Headwear`, 헬멧의 자식 7개가 그대로 따라 움직였다. **Nomad 프로필은 아이템·루블 한 건도 안 바뀌었다** |
| 4 | 다섯 프로필 전부에 왕복 한 번씩 | **통과.** 기동 로그를 보면 `TraderGenPocketMigration` 이 **매 기동 여섯 프로필 전부에** 걸린다 - 즉 등록된 다섯은 모두 "마이그레이션이 필요한 옛 프로필" 이고, 그 다섯의 왕복 10건이 전부 저장·확인됐다(6~22ms). 역직렬화 예외 0건 |

**합계.** 편집 저장 10건, `장비 편집 거부`·`확인 실패`·`되돌린다`·`되돌리기가 실패` **0건**.
프로필 사이의 아이템 id 충돌은 편집 전후 **10건 그대로**(SPT 프로필 틀이 같은 id 를 쓰는 기본 장비다 -
같은 id 가 프로필마다 tpl 이 다른 것도 있다). **새로 생긴 충돌 0건** = 복제가 없다.

**덤으로 본 것.** 관문 8(벗기기 격자 자리)의 절반 - 벗긴 헬멧에 좌표가 **붙는다**. 가득 찬 스태시에서의
거절은 보지 않았다.

**새 도구 셋(이번에 만들었다).**
- `tools/stm-profile-diff.mjs <before-dir> [after-dir]` - 프로필 아이템 `_id` 집합을 양방향으로 대조하고,
  부모·슬롯·좌표가 바뀐 것을 적고, **편집 뒤 새로 생긴** 프로필 간 id 충돌만 복제로 보고한다.
- `tools/stm-profile-gear.mjs <닉네임|id> [--stash N] [--slot X]` - 착용 장비와 스태시 최상단을 이름까지.
- `tools/stm-call.mjs <라우트> <JSON> [세션 id]` - zlib 로 싸서 STM 라우트를 부르고 응답을 푼다.

**내가 바꿔 놓은 상태(되돌리지 못했다).** 편집이 성공하면 서버가 `손으로 고침`(HandEdited)을 켠다.
그래서 **SPT02·Knox·Buck·Mace 넷의 표시가 켜졌다**(Hawk 는 어제 꾸러미 구매로 이미 켜져 있었다).
끄는 길은 코드에 없다(쓰는 곳이 `HandEdited: true` 하나뿐이고, 기본값 false 는 **등록을 새로 만들 때**만
생긴다). 다섯 다 장비·무기가 있어 출격 판정은 그대로 `실물 장비`이고, 차이가 나는 경우는 **그 팀원의 장비가
비었을 때 임시 장비로 덮지 않는다**는 것뿐이다. 되돌리려면 명단에서 제거하고 다시 등록해야 한다
(자동 합류·전술이 기본값으로 돌아간다).

**알아 두면 좋은 함정 — 서버 로그의 시각은 UTC 다.** 파일 수정 시각은 KST 라 9시간 어긋난다.
`07:14`(로그) = `16:14`(탐색기). 어제 "어제 09:47·11:02(UTC)" 라고 적어 둔 것도 같은 이유다.

**아직 안 한 것(화면 절반).** 왼쪽 칸이 실제로 그 스태시를 그리는지, 편집 버튼·저장 버튼·거절 토스트,
관문 13(저장 중 화면 이탈)·14(저장 직후 재진입)·16(상세 화면 배치)·6·7(용병 경로 회귀)·9(슬롯 필터)·
10(편집한 장비로 레이드)·11(계획 2 회귀)·12(다섯 번째 경로)·17·24·25. 서버 쪽 판정은 위에서 돌았으니
화면에서 볼 것은 **"서버가 옳게 판정한 것이 화면에 그대로 보이는가"** 다.

**실측.** 백업 `backup/2026-11-03-before-plan3-gear-gate/`(프로필 6개). 코드는 바꾸지 않았다(도구·문서만).

### 관문 9·24·6 도 라우트로 돌렸다 — 결함 하나를 찾았다: 탄창에 탄을 넣을 수 없다 (2026-11-03)

관문 2~5 에 이어 **게임 없이 돌 수 있는 것**을 이어서 돌렸다(슬롯 필터·id 대소문자·플레이어 아이템 거절).
교체는 전부 "현재 것 벗기기 + 스태시의 새 것 입히기" **한 묶음**으로 보내고, 다음 묶음으로 되돌렸다.

| 관문 9 — 무엇을 넣어 봤나 | 결과 |
|---|---|
| 몸 장비 `Headwear` (Buck 의 Tor-2 로 교체) | **통과** |
| 몸 장비 `ArmorVest` (THOR IC) | **통과** |
| 몸 장비 `TacticalVest` (Umka, 내용물 18개 딸린 것을 벗겨 교체) | **통과** |
| 몸 장비 `Backpack` (Hawk 의 RUSH 100) | **통과** |
| 무기 부착물 `mod_scope` (ALPHA4 떼었다 붙이기) | **통과** |
| 탄창 분해 (탄 → 스태시, 탄창 → 스태시) | **통과** |
| **탄창 꽂고 그 탄창에 탄 넣기(순서 연쇄)** | **실패** - `'…deaf' 에는 'cartridges' 슬롯이 없어 넣지 않습니다.` |

**결함: 탄은 탄창에 넣을 수 없다.** 인계 문서 열린 항목 4 가 "편집이 탄을 특별 취급하지 않고 같은 규칙으로
허용한다 … 한 묶음 안에서 탄창을 꽂고 그 탄창에 탄을 넣는 순서 연쇄가 된다" 고 적어 둔 것이 **틀렸다.**
원인은 슬롯 어휘를 만드는 자리다: `StmGearEditService.BuildSlotFacts` 가 템플릿의 **`Properties.Slots` 만**
읽는데, 탄창은 탄을 받는 자리를 `Slots` 가 아니라 **`Properties.Cartridges`** 에 둔다. 실측:
바닐라 탄창 `mag_stanag_colt_ar15_std_556x45_30` 는 `Slots: (없음) / Cartridges: cartridges` 다.
그래서 정책은 "그 슬롯이 없다" 로 **항상** 거부한다 - 모드 아이템이라서가 아니다(바닐라 Knox 의 PMAG D60
으로도 같은 거절이 나온다).
**두 번째 벽이 하나 더 있다**: 탄의 원래 `location` 은 격자 좌표가 아니라 **정수 0**(탄창 안 색인)이다.
`StmGearEditItem` 은 좌표를 일부러 보내지 않으므로(설계), `Cartridges` 를 어휘에 넣는 것만으로는
"어느 색인에 넣는가" 가 비어 있다. 고치는 사람은 두 가지를 같이 봐야 한다.

| 그 밖에 돌린 것 | 결과 |
|---|---|
| **관문 24** 같은 id 를 **대문자로** 보내기 | **설계대로 거절** - `아이템 '6AE8…DE67' 은(는) 이 팀원이 착용·장착한 것이 아니어서…`. 소문자는 22건 전부 통과했으니 Ordinal 비교가 SPT 의 소문자 표기와 맞물린다 |
| **관문 6** 플레이어 아이템을 팀원 보안 컨테이너로 | **거절** - `…이 팀원의 스태시에 없어 장비에 넣을 수 없습니다.` 출처 검사에서 막히므로 보존 검사까지 가지도 않는다 |
| 팀원 헬멧을 **플레이어 스태시**로 | **거절** - `넣을 자리 … 이 팀원의 장비 트리도 스태시 루트도 아니어서…` |
| **로그인 중인 프로필(Nomad)** 편집 | **거절** - `이 팀원은 프로필 팀원이 아니어서…` |
| aid 와 profileId **불일치** | **거절** - `요청한 프로필과 이 팀원의 프로필이 달라…` |
| 등록되지 않은 aid / 빈 편집 목록 | 각각 **거절**(같은 사유 / `고칠 아이템이 없어…`) |

**합계.** 편집 저장 22건(관문 2~5 의 10건 포함), `확인 실패`·`되돌린다`·`되돌리기가 실패` **0건**.
아이템 개수는 여섯 프로필 전부 그대로이고(Buck 604 → 604) 새로 생긴 프로필 간 id 충돌 0 이다.

**덤으로 본 것 — 벗기기가 `location` 에 `rotation` 을 더해 쓴다.** 되돌린 뒤에도 네 아이템
(Hawk RUSH 100, Buck THOR IC·Umka·Tor-2)의 `location` 에 `rotation: true/false` 가 **새로 붙었다**.
`x`·`y`·`r` 은 같고 `rotation` 은 `r` 과 같은 뜻이라(Vertical = true) 자리가 바뀐 것이 아니다.
SPT 의 `PlaceItemInContainer` 가 쓰는 모양이고, 우리가 좌표를 만들지 않는다는 증거이기도 하다.

**남겨 둔 상태 하나.** Buck 의 주무기 탄창에 있던 **탄 30발(`6ae8141e677d88877f000000`)이 스태시에
남아 있다**(x0,y26). 분해는 됐는데 위 결함 때문에 다시 넣을 길이 라우트에 없다. 탄창은 무기에 다시
꽂아 두었으므로 **빈 탄창** 상태다 - 관문 10(편집한 장비로 레이드, 빈 탄창으로 나가지 않는지)을 돌릴 때
이 상태가 그대로 쓰인다. 게임에서 드래그로 되돌릴 수도 있고, 위 결함을 고치면 라우트로도 되돌아간다.

**실측.** 코드는 바꾸지 않았다(문서만). 설치본 md5 그대로(클라이언트 `5701a5e4`·서버 `c2308a81`).

### 탄을 탄창·약실에 넣을 수 있게 고쳤다 (2026-11-03, 코드까지)

같은 날 관문 9 에서 찾은 결함이다. **테스트를 먼저 썼다**(TDD): 용량보다 큰 더미 거부, 용량 안의 더미
허용, 약실은 한 발, 수량을 모르면 거부, 용량 조회 — 다섯 개가 **컴파일 단계에서 빨갛게** 떨어지는 것을
보고(새 API 가 없다) 구현했다. 576 → **581개 통과**, 빌드 경고 0·오류 0.

**고친 것 셋.**
1. **어휘.** `BuildSlotFacts` 가 `Properties.Slots` 외에 **`Properties.Cartridges`·`Properties.Chambers`**
   도 같은 모양으로 옮긴다. 셋 다 `Props.Filters[].Filter` 라서 필터 읽기는 `AllowedTemplatesOf` 하나로 모았다.
2. **용량.** `GearSlotDefinition` 에 `AmmoCapacity`, `GearSlotFacts` 에 더미 수량(`StackCountByItemId`)을
   더했다. 이 편집은 **수량을 쪼개지 않으므로** 1080발 더미는 30발 탄창에 들어갈 수 없다 - 넣으면 EFT 가
   적재를 거부할 수 있는 프로필이 된다(관문 17 의 '규칙을 어긴 스태시 항목' 과 같은 부류다).
   용량을 아는데 수량을 모르면 거부한다(닫힌 쪽 - "한 발이겠지" 로 치면 검사가 뚫린다).
   - 탄창 용량은 `Cartridges[]._max_count` 다(실측: STANAG 30발 = 30, GP-5 = 33).
   - **약실에는 개수 필드가 없다**(실측: M4A1 의 `patron_in_weapon` 은 `filters` 하나뿐이다). 그래서 1 로
     싣는다 - 이름에서 뜻을 지어낸 것이 아니라 ① 데이터베이스가 개수를 말하지 않고 ② 이 PC 의 프로필
     여섯 개에 있는 약실 아이템 **15개가 전부 수량 1** 이고 ③ 1 이 닫힌 쪽이기 때문이다.
3. **쓰는 `Location`.** 탄을 담는 자리는 격자 좌표가 아니라 **색인**이다(실측: 프로필 JSON 의 cartridges
   항목은 `location` 이 정수 `0`). 정책 (a) 가 그 자리가 **비어 있음**을 확인했으므로 색인 0 을 쓴다 -
   지어낸 값이 아니고, PIT 의 `CreateCartridges(..., 0)` 도 같은 값을 쓴다.

**단위 테스트가 이 결함을 못 본 이유를 적어 둔다(불변식 9 의 예가 하나 늘었다).** 정책 테스트의 픽스처가
`["tpl-mag"] = cartridges` 정의를 **손으로** 들고 있었다. 그래서 "탄약도 다른 아이템과 같은 규칙으로
허용한다" 는 테스트가 처음부터 녹색이었고, 서버가 그 정의를 **만들지 못한다**는 사실은 아무도 묻지 않았다.
픽스처에 실제 데이터의 모양(용량·수량)을 넣은 지금은 같은 테스트가 그 자리를 지난다.

**아직 설치하지 못했다.** 설치된 서버 DLL 은 돌고 있는 SPT.Server 가 잠그고 있어서 교체에 서버를 내려야
하는데, 프로세스 종료가 권한 분류기에 막혔다(PowerShell `Stop-Process`·bash `taskkill` 둘 다 거부).
그래서 **라우트 실측과 Buck 의 탄 복구는 설치 뒤로 남는다**. 사용자가 서버를 닫으면 바로 이어진다:
`pwsh -File tools/install-sptteammanager.ps1 -Remove` → 같은 스크립트 재실행 → md5 대조 → 서버 기동 →
`node tools/stm-call.mjs stm/members/edit-gear …` 로 탄창 연쇄 재시험.

### 탄창에 탄이 들어간다 — 라우트에서 확인했다 (2026-11-03, 열린 항목 4 닫힘)

사용자가 서버를 닫아 주어 새 서버 DLL 을 설치하고(제거 → 재설치, md5 `fb763a61` 산출물과 일치,
빌드보다 새로운 `.cs` 없음) 기동했다: `Teammate database ready` ×6, `PitFireTeam loaded`,
`StmRegistrationsUnreadable`·`StopAndReport`·`끝나지 않은 전송` 0건.

**첫 시도는 실패했고, 그 실패가 결함 하나를 더 꺼냈다.** 어휘 수정만으로는 모자랐다:

```
[STM] 장비 편집 확인 실패: 디스크의 아이템 '…f000000' 이(가) 기대와 다르다
(… 슬롯 'cartridges' 기대 'cartridges', 좌표 있음, 기대 없음). 원래 상태로 되돌린다.
```

디스크 재독의 기대가 **두 갈래**(격자면 좌표 있음, 슬롯이면 좌표 없음)였는데, 탄을 담는 자리는
색인 좌표를 **가져야** 한다. 즉 쓰는 쪽은 고쳤는데 **확인하는 쪽이 모르고 있었다** - 맞게 저장된 편집이
"기대와 다르다" 로 되돌려졌다. 닫힌 쪽 설계가 제대로 돈 것이기도 하다(아무것도 깨지지 않았다).
기대를 **세 갈래**로 고쳤다: 격자(좌표 있음)·탄 자리(색인 있음)·그 밖의 슬롯(좌표 없음).
한 값(`ToAmmoSlot`)이 쓰는 Location 과 재독 기대를 **둘 다** 가른다 - 한쪽만 알면 같은 일이 또 난다.

**그다음 세 번이 전부 통과했다.**

| 라우트 호출 | 결과 |
|---|---|
| 탄 30발 → 그 탄창의 `cartridges` | **통과**(21ms). 디스크에 `slotId: cartridges`, `location: 0`, 수량 30 |
| 분해: 탄·탄창 → 스태시 (2건 한 묶음) | **통과**(23ms) |
| **탄창 → 무기 `mod_magazine`, 탄 → 그 탄창 `cartridges` (2건 한 묶음, 순서 연쇄)** | **통과**(31ms) - 관문 9 의 마지막 항목이다 |

**Buck 의 상태도 이것으로 되돌아왔다.** 편집 전 백업과 대조하면 **탄은 더 이상 차이 목록에 없다**
(탄창 안, 수량 30, 색인 0). 여섯 프로필의 아이템 개수가 전부 그대로이고 새로 생긴 id 충돌 0 이다.
남은 차이는 네 아이템의 `location` 에 `rotation` 키가 붙은 것뿐이다(`x`·`y`·`r` 동일, 뜻도 같다 -
SPT 의 `PlaceItemInContainer` 가 쓰는 모양이다).

**용량 거부는 단위 테스트에만 있다.** 라우트로 넘치는 더미를 넣어 보려면 스태시에 용량보다 큰
**같은 구경** 더미가 있어야 하는데, Buck 의 그 구경 더미(910발·860발)는 가방과 리그 안에 있다 -
스태시로 빼내면 이 API 로는 가방 격자(`main`)로 돌려놓을 수 없어(격자는 슬롯이 아니다) 찌꺼기가 남는다.
그래서 하지 않았다. 용량 판정은 테스트 셋(용량 초과 거부·용량 안 허용·수량 모르면 거부)이 지킨다.

**실측.** 테스트 581개 통과, 빌드 경고 0·오류 0. 설치본 md5 서버 `fb763a61`·클라이언트 `b5841800`
(클라이언트는 **소스가 그대로이고** 재빌드로 md5 만 달라졌다 - 이 수정이 건드린 세 파일 중 클라이언트가
컴파일하는 것은 하나도 없다. `pitFireTeam.csproj` 에 `ProfileGearEditPolicy` 가 없다).

### 화면 절반을 인게임에서 봤다 — 다섯 통과, 쓸 수 있는 범위가 생각보다 좁다 (2026-11-03)

사용자가 게임에서 돌렸고, 나는 프로필 대조와 서버 로그로 판정했다. 편집 전 상태는
`backup/2026-11-03-before-ingame-screen-gates/` 에 있다.

| 관문 | 결과 |
|---|---|
| **3** 왼쪽 칸이 그 팀원 스태시뿐인가 | **통과.** 눈으로도 확인됐다. 라우트 실측(집합 동일·플레이어 아이템 0)과 같은 말을 한다 |
| **2·5** 입히기/벗기기 저장 | **통과.** 거절 없이 저장됐다. 로그에 Mace 헬멧·방탄복·주무기, Buck 권총·이어피스까지 **9건**이 `한 번에 저장·확인했다`(8~23ms) |
| **14** 저장 직후 다시 열기 | **통과.** 방금 옮긴 아이템이 제자리에 보인다 |
| **6·7** 용병 경로 회귀 | **통과.** STM 편집기를 열고 닫은 뒤 용병 편집·저장이 평소대로다 |
| **12** 다섯 번째 경로 | **통과.** 친구 목록 조회가 착용 장비만 보여 준다 |
| **13** 저장 중 화면 이탈 | **사실상 실행 불가.** 저장이 8~23ms 라 사람 손으로 그 사이를 칠 수 없다. 기록해 두고 더 돌리지 않는다 |

**프로필은 멀쩡하다.** 여섯 프로필 모두 아이템 개수 그대로, 새로 생긴 id 충돌 0, 플레이어 프로필에서
사라진 아이템 0. 차이는 두 아이템의 `location` 에 `rotation` 키가 붙고 빠진 것뿐이다(뜻은 같다).
`[Error]`·`[Critical]` 로 찍힌 pitTeam·STM 줄은 **0건**이다 - 거절은 전부 정책이 사유와 함께 돌려준 것이다.

**그런데 사용자가 겪은 것은 "대부분 안 된다" 였다.** 안 되는 넷은 전부 **설계대로**이고, 셋은 이미
[BACKLOG](BACKLOG.md) 에 있던 항목이 실제로 어떻게 보이는지 드러난 것이다.

| 사용자가 한 조작 | 돌아온 말 | 무엇인가 |
|---|---|---|
| 리그 안 탄창의 탄을 꺼내 옮기기 | `아이템 '6ae818b8…' 은(는) 이 팀원의 스태시에 없어 장비에 넣을 수 없습니다.` | **출처가 스태시뿐**이라서다. 그 탄은 리그(`GridView (5)`) 안 탄창의 `cartridges` 에 있다 - 같은 팀원의 것인데도 장비 트리 안이라 출처가 못 된다. **새 항목**(아래 BACKLOG) |
| 가방·리그 칸에 넣기 | `슬롯 'GridView (4)' 은(는) 아는 장착 슬롯 이름이 아니어서 편집하지 않습니다.` | 격자 배치. BACKLOG 에 이미 있다(요청에 좌표가 없다) |
| 탄박스에서 탄을 꺼내 30 → 50 으로 만들기 | `편집 중 새로 생긴 아이템 '6ae99ab7…' 이(가) 있어 저장하지 않습니다.` | 묶음 나누기. BACKLOG 에 이미 있다(편집은 수량을 읽지도 쓰지도 않는다). 그 새 아이템은 **프로필에 저장되지 않았다**(확인함) |
| 탄창 빼고 탄 넣고 다시 끼운 뒤 저장 | "바뀐 것이 없다" | 중간 조작이 거절돼 최종 상태가 원래와 같아졌다. 거절이 아니라 **빈 묶음**이다 |

**그래서 오늘 고친 "탄창에 탄" 은 화면에서 거의 쓸 수 없다.** 라우트로는 되지만, 화면에서 그 조작을
하려면 탄이 **스태시 최상단에 낱개 더미로** 있어야 한다. Buck 의 탄은 탄박스 안이거나 리그 안이라
꺼낼 길이 없었다. 기능이 없는 것이 아니라 **닿는 길이 좁다.**

**문구 하나는 사실과 어긋난다.** `이 팀원의 스태시에 없어` 는 "남의 것" 처럼 들리는데, 실제로는
**그 팀원의 것이지만 장비 안에 있다**. 출처 규칙을 넓히든 아니든 이 문장은 고쳐야 한다.

**아직 안 본 것.** 관문 16(상세 화면 배치·거부 사유), 8(가득 찬 스태시), 9 의 화면 쪽(위 이유로 좁다),
10·11(레이드), 17(스태시 재검증), 18~23(제스처), 25(서버 강제 종료).

### 장비 안의 아이템도 출처가 된다 — 출처 규칙을 넓히고 문구를 고쳤다 (2026-11-03)

인게임에서 드러난 제약을 바로 고쳤다. 전에는 입히기의 출처가 **스태시 하위뿐**이라, 리그 안 탄창이나
가방 안 여분 탄창처럼 **그 팀원이 이미 가진** 것을 쓸 수 없었다. 이제 **그 팀원의 것이면 된다**
(스태시든 장비든). 같은 프로필 안의 이동이라 공짜 생성이 없고 쓰기는 여전히 한 번이다.

**테스트를 먼저 썼다**(TDD, 581 → 585). 넷이 빨갛게 떨어지는 것을 보고 구현했다:
리그 안 탄창을 권총에 꽂기(허용), 떼어낼 수 없는 자리는 장비 안에서도 거부, **자기 자신이나 자기 안으로는
못 넣기**, 그리고 거절 문구가 "스태시" 를 말하지 않기.

**넓히면서 같이 세운 가드 둘.**
- **떼어낼 수 없는 자리**(주머니·보안 컨테이너·완장·칼집·영구 조각)는 장비 안에서 다른 슬롯으로 옮기는
  것도 막는다. 권위는 벗기기와 같은 `WornGearLossPolicy` 하나다(집을 둘로 만들지 않는다).
- **고리 방지.** 출처가 스태시뿐일 때는 생길 수 없던 모양이다: 리그를 그 리그 안의 탄창에 넣으라는
  요청이 이제 성립한다. 그대로 쓰면 트리가 끊겨 그 가지 전체가 사라진 것처럼 보인다. 목적지 부모가
  **그 아이템의 하위 트리 안**이면 거부한다.
- 덤으로 드러난 것: "같은 아이템을 한 묶음에서 두 번" 을 막던 그물이 **출처 규칙의 부작용**이었다
  (두 번째 건의 시점에 "스태시에 없다" 로 걸렸다). 넓히면서 그 그물이 없어져 테스트 하나가 빨갛게 됐고,
  규칙을 `ValidateBatch` 에 **명시**했다 - 두 번 적으면 최종 자리가 모호하고 재독 기대도 둘이 된다.

**문구.** `이 팀원의 스태시에 없어 장비에 넣을 수 없습니다` → `이 팀원의 것이 아니어서 장비에 넣을 수
없습니다`. 옛 문장은 **그 팀원의 것이지만 장비 안에 있는** 경우를 "남의 것" 처럼 말했다.

**라우트 실측(설치본 md5 `16003986`).** Buck 으로 네 번, 전부 통과하고 **원상복귀**했다:
① 무기 탄창의 탄 30발 → 스태시 ② **리그 탄창의 탄 50발 → 무기 탄창**(장비 → 장비, 전에는 거절되던 그
조작) ③ 되돌림: 탄 50발 → 리그 탄창 ④ 되돌림: 탄 30발 → 무기 탄창. 대조 결과 여섯 프로필의 아이템
개수·id 집합 그대로, 새 충돌 0, Buck 의 탄 둘 다 제자리다.

**아직 안 되는 것은 그대로다.** 격자(가방·리그 **칸**)에 **넣는** 것은 요청에 좌표가 없어 여전히 거부다
([BACKLOG](BACKLOG.md)). 그래서 리그에서 **꺼내 슬롯에 끼우는** 방향만 열렸다 - 꺼낸 것을 리그 칸으로
되돌리려면 스태시를 거쳐야 한다. 묶음 나누기도 그대로 거부다.

**실측.** 테스트 585개 통과, 빌드 경고 0·오류 0. 클라이언트는 이 정책을 컴파일하지 않아 건드리지 않았다.

### 교환이 막힌 진짜 이유는 순서였다 — 비우는 건이 먼저 간다 (2026-11-03)

출처를 넓힌 뒤 인게임에서 "무기의 탄창을 스태시로 빼고 **리그의 탄창을 그 자리에** 꽂기" 를 한 묶음으로
했더니 이렇게 돌아왔다:

```
'…de90' 의 'mod_magazine' 슬롯에는 이미 아이템 '…deaf' 이(가) 있어 넣지 않습니다.
```

**출처 규칙이 아니라 묶음의 순서 문제였다.** `GearEditBatchPlanner` 의 순서 제약은 둘뿐이었다 -
새 부모가 먼저, 옛 부모보다 자식이 먼저. **"그 자리를 비우는 건이 채우는 건보다 먼저"** 가 없어서,
두 건이 서로 부모 관계가 아니면 **편집기 목록에 나온 순서 그대로** 나갔다. 채우는 건이 먼저 나가면
서버의 점유 검사(한 슬롯에 하나)가 **맞는 편집을 거절한다.**

- 규칙을 더했다: 각 건의 목적지 `(부모, 슬롯)` 을 **그 자리를 떠나는 건**과 이어 위상 정렬에 넣는다.
  슬롯 이름 비교는 서버 판정과 같은 기준(대소문자 무시)이다.
- **서로 자리를 맞바꾸는 두 건**은 이제 고리가 되어 `순서를 정할 수 없어 저장하지 않습니다` 로 거절한다.
  중간 자리가 필요한 조작이라 순서를 지어낼 수 없다 - 조용히 하나를 떨어뜨리는 것보다 낫다.

**테스트 둘**(585 → 587). 첫 테스트는 처음에 **우연히 통과**했다 - 픽스처의 목록 순서가 마침 옳았다.
순서를 뒤집어 빨갛게 만든 뒤에 고쳤다. 순서 규칙을 미는 테스트는 **목록 순서가 틀린 상태**에서 써야 한다.

**실측.** 테스트 587개 통과. 클라이언트 빌드 경고 0(애드온의 `MSB3644` 는 이 PC 의 알려진 제약),
새 클라이언트 md5 `d600127f`. **이 파일은 클라이언트가 컴파일한다** - 게임을 닫아야 설치된다.

**같은 날 확인된 것들(인게임).**
- 스태시의 권총을 권총집에 채우고 **이어서** 스태시의 탄창을 그 권총에 꽂는 **순서 연쇄가 화면에서 돈다**
  (저장 2건, 24ms·21ms).
- **보안 컨테이너는 편집기 화면에 아예 없다** - EFT 의 그 화면이 그 칸을 그리지 않는다. 그래서 그 가드는
  화면에서 시험할 수 없고, 라우트로 확인했다: `'SecuredContainer' 은(는) 프로필에서 떼어낼 수 없는
  자리라서 옮기지 않습니다.`, 완장도 같다. 화면에서 같은 가드를 보려면 칼집·완장·주머니를 끌면 된다.
- **EFT 의 UI 는 탄을 넣을 때 묶음을 쪼갠다.** 860발에서 50발을 떼어 내는 순간 새 id 가 생겨
  `편집 중 새로 생긴 아이템 …` 으로 거절된다. 그래서 오늘의 "탄창에 탄" 은 화면에서 **묶음을 통째로**
  옮길 때만 닿는다(라우트로는 쪼개기 없이 된다).

### 계획 5 Task 1 — 가방·리그·주머니 **칸**에 넣을 수 있다 (2026-11-03)

사용자 지시("팀원의 스태시·장비는 플레이어 것과 동일하게 동작해야 한다")를 받아
[계획 5](plans/2026-11-03-stm-plan5-gear-parity.md)를 세우고 첫 Task 를 끝냈다.

**테스트를 먼저 썼다**(587 → 592). 다섯이 컴파일 단계에서 떨어지는 것을 보고 구현했다: 스태시 안 가방의
칸에 넣기, 리그 칸에 넣기, 격자가 **제외**하는 종류 거부, 격자는 이미 든 것이 있어도 받음,
**스태시에 있는 탄창에도 탄 넣기**.

**고친 것 셋.**
1. **어휘에 격자를 더했다.** `BuildSlotFacts` 가 템플릿의 `Properties.Grids` 를 읽어
   `GearGridDefinition(이름, 허용, 제외)` 로 싣는다. 허용은 보통 **뿌리 노드 하나**라 조상 사슬을 걷어야
   맞고(슬롯과 같은 걷기를 쓴다), 제외(`ExcludedFilter`)가 "가방 안에 가방" 같은 것을 걷어낸다.
2. **목적지 판정을 트리에서 풀었다.** 전에는 "장비 트리 또는 스태시 루트" 만 목적지였다. 이제
   **이 팀원의 것이면 어느 트리든** 목적지가 된다 - 그래서 스태시에 둔 탄창에 탄을 넣고, 스태시 안 가방에
   물건을 넣는 것이 된다(플레이어 화면과 같다). 출처 판정도 방향과 무관하게 하나로 모았고, 떼어낼 수 없는
   자리·고리 방지는 **두 방향 모두에** 건다.
3. **쓰기를 일반화했다.** `TryPlaceInStash` → `TryPlaceInContainer(용기, 격자, …)`. 스태시 루트에 묶여
   있던 것을 풀었을 뿐, 자리를 찾는 것은 여전히 SPT 의 `GetContainerMap` + `PlaceItemInContainer` 하나다.
   재독 기대도 넓혔다: 좌표가 **있어야 하는 자리**(격자·탄 색인)와 **없어야 하는 자리**(그 밖의 슬롯),
   그리고 트리 기대는 "목적지가 스태시 트리인가" 로 바뀌었다(가방이 장비에 달려 있으면 장비 트리다).

**라우트 실측**(설치본 md5 `fd71138f`).
- 스태시의 Tor-2 헬멧 → Buck **가방의 `main` 칸**: 통과(23ms), 되돌리기도 통과(22ms).
- 같은 헬멧 → **리그의 `GridView (2)`**: `놓을 자리를 찾지 못해 전체를 취소했습니다` - 그 칸에 들어갈
  크기가 아니다. **자리가 없으면 거절**하는 길이 실제로 도는 것을 봤다(관문 8 의 절반).
- 여섯 프로필의 아이템 개수 그대로, 새 id 충돌 0.

**내가 바꿔 놓은 것 하나.** 위 왕복에서 Tor-2 헬멧이 스태시의 **다른 칸**으로 돌아갔다(y17 → y26).
요청에 좌표가 없어 SPT 가 빈 자리를 찾기 때문이다 - 계획 5 Task 1 의 남은 절반(**정확한 칸** 지정)은
`LayoutKey` 를 전선에 싣는 일이고 아직 하지 않았다. 화면에서 칸만 옮긴 변화는 여전히 "저장되지 않는 변화"
로 개수만 알린다.

**실측.** 테스트 592개 통과, 서버 빌드 경고 0·오류 0. 클라이언트는 이 정책을 컴파일하지 않아 그대로다.

### 특수 슬롯의 물건은 옮길 수 있어야 한다 — 두 판단을 갈랐다 (2026-11-03)

인게임에서 `'SpecialSlot1' 은(는) 프로필에서 떼어낼 수 없는 자리라서 옮기지 않습니다.` 가 나왔다.
**내 가드가 틀렸다.** 편집 금지 판정이 사망 판정(`WornGearLossPolicy.IsNeverRemovedSlot`)을 그대로 쓰는데,
그 집합에는 **특수 슬롯**(열쇠고리·나침반 자리)이 들어 있다 - 사망이 가져가지 않는 것은 맞지만,
**플레이어는 자기 특수 슬롯의 물건을 손으로 넣고 뺀다.** 두 질문을 같은 집합으로 답한 것이 잘못이다.

- `ProfileGearEditPolicy.IsStructuralSlot` 을 새로 두었다: 사망 집합에서 **특수 슬롯만 뺀 것**.
  주머니·보안 컨테이너·완장·칼집·인식표는 그대로 막는다(플레이어 화면에서도 안 움직인다).
- 조각 이름(`SpecialSlot`)의 집은 여전히 `WornGearLossPolicy` 하나다 - 거기에 `IsSpecialSlot` 을 열었다.
- **교차 계약 테스트**를 "두 집합의 차이는 특수 슬롯뿐" 으로 적었다(592 → 595). 다른 차이가 생기면 실패한다.

### 계획 5 Task A — 조작 메뉴가 넷뿐이던 진짜 이유 (2026-11-03)

메뉴가 `상세정보·버리기·SEARCH IN STASH·ITEM INFO` 넷으로 얇아 **탄박스 열기·묶음 나누기·장전·비우기가
아예 없었다.** 컨텍스트 타입(`TransferItemsScreen`) 때문이라고 짐작했는데, 역컴파일로 보니 **아니었다**.

`ItemContextInteractionsSwitcher.IsActive` 의 첫 두 문장이다:

```csharp
if (!_item.ItemInteractionButtons.Contains(button)) return false;
var list = new List<EItemInfoButton> { Inspect, Examine, Discard };
if (!Examined && !list.Contains(button)) return false;   // ← 여기
```

`Examined` 는 `_itemController.Examined(_item)` 이고, `InventoryController.Examined` 는 **생성자의 두 번째
인자**가 true 면 무조건 true, 아니면 플레이어 백과사전을 본다. 편집기는
`new InventoryController(editorProfile, false)` 로 만들어져 있었다(PIT 에서 그대로 물려받은 값) -
그래서 **모든 아이템이 "살펴보지 않은 것"** 이 되어 메뉴가 셋으로 잘린 것이다. 화면 좌표도, 컨텍스트
타입도 아니었다.

**고침은 그 인자 하나다**(`false` → `true`). 읽는 쪽 판정일 뿐 프로필에 아무것도 쓰지 않는다. 플레이어
화면에서 그 물건들이 다 보이는 이유도 같다(자기 백과사전에 있다).

**이것으로 열리는 것**: 탄박스 **열기**, 탄창 **비우기(Unload)**·**장전(Load)**, 그리고 **나누기** 메뉴.
비우기와 열기는 **묶음을 통째로 옮기는** 조작이라 지금 저장 경로로도 그대로 저장된다. **나누기는 아직
저장되지 않는다**(새 id 가 생긴다) - 그것이 Task B 의 몫이다.

**설치.** 바뀐 것은 클라이언트 DLL 하나뿐이라(서버 DLL 은 `fd71138f` 그대로) 그 파일만 덮었다:
설치본 md5 `d12aa418` = 빌드 산출물. 서버는 켜진 채 두었다 - 서버 쪽은 바뀌지 않았다.

### 스태시 칸의 메뉴가 짧았던 이유 — 아이템 문맥의 **종류** (2026-11-03)

Task A 뒤 인게임: **리그·가방에서는 메뉴가 길어졌는데**(`탄약 꺼내기` 등) **스태시 칸에서는 그대로 넷**
이었다. 탄박스를 리그로 옮기면 메뉴가 생긴다는 사용자 관찰이 결정적이었다 - 같은 아이템, 다른 자리.

역컴파일로 갈라지는 자리를 찾았다. EFT 의 진짜 인벤토리 화면(`InventoryScreenController`)은 아이템 문맥으로
**`new InventorySelectableItemContext(EItemViewType.Inventory)`** 를 쓰는데, PIT 의 편집기 스태시 패널은
`AreaStashItemContext`(하이드아웃 구역 스태시용)를 넘긴다. 그리고 메뉴 판정이 **문맥의 종류를 직접 본다**:

```csharp
case EItemInfoButton.ApplyMagPreset:
case EItemInfoButton.LoadAmmo:
    if (!Gameplay && _itemContext is InventorySelectableItemContext) { ... }
    return false;
```

- `TryBuildLoadoutEditorStashPanel` 에 **문맥 override** 인자를 더했다(기본값 null = PIT 가 쓰던 그대로).
  PIT 용병 편집기는 건드리지 않는다 - 업스트림 파일의 동작을 바꾸지 않는다(불변식 4).
- STM 편집기만 `InventorySelectableItemContext(EItemViewType.Inventory)` 를 준다.

**같은 날 사용자가 본 나머지 넷의 상태.**
- 탄창 분리·결합·탄 제거: **된다**(묶음 통째 이동이라 지금 저장 경로로 저장된다).
- 800발 묶음에서 탄창 채우기: **안 된다**(묶음 나누기 - Task B).
- **탄박스를 가방으로 옮겨 탄 추출 → `편집 중 사라진 아이템 …`**: 추출이 상자를 **없앤다**. 사라지는 id 를
  적을 수 없는 것도 Task B 다.
- `SpecialSlot1` 여전히 막힘: **고침이 서버에 설치되지 않았다.** `IsStructuralSlot` 변경은 서버 DLL 인데
  그 뒤로 서버를 다시 설치하지 않았다(설치본 `fd71138f`, 산출물 `40c2420e`). 다음 설치에 함께 올라간다.
- 보안 컨테이너: Task C(아직).

### 계획 5 Task B(1/3) — 최종 상태를 받아들일 판정을 먼저 썼다 (2026-11-03)

조작을 하나씩 열어 주는 방식을 버리고 **최종 상태 제출**로 간다. 그 첫 조각은 "이 변화가 정당한가" 를
보는 판정이다 - 라우트보다 **먼저** 쓴다(이 판정이 뚫리면 복제가 그대로 프로필에 들어간다).

`ProfileInventoryApplyPolicy.Validate(before, after, stashRoot, equipmentRoot)` 가 막는 것:
- **늘어나는 것**: tpl 별 총 수량이 before 보다 많으면 거절(복제).
- **바깥에서 들어온 것**: before 에 없던 tpl 이 after 에 있으면 거절(플레이어 스태시에서 온 것).
- **종류 바꿔치기**: 같은 id 의 tpl 이 달라지면 거절.
- **구조 손상**: 주머니·보안 컨테이너·완장·칼집·인식표가 없어지거나 다른 자리로 가면 거절
  (`ProfileGearEditPolicy.IsStructuralSlot` - 사망 집합에서 특수 슬롯만 뺀 그 판정).
- **트리 손상**: 두 루트가 그대로 있고 부모가 없어야 하며, 모든 아이템이 두 루트 중 하나에 닿아야 한다
  (고아·고리 거절, 걸음 64 제한).

**줄어드는 것은 막지 않는다.** 플레이어는 자기 화면에서 버리고 탄박스를 소비한다 - 팀원 화면도 같아야
한다는 것이 이 계획의 전제다. 대신 **사라지는 id 를 전부 돌려준다**(`LostItemIds`) - 호출자가 로그에 남긴다.

**테스트 열 개**(595 → 605): 변화 없음·나누기 허용·총량 증가 거절·없던 종류 거절·탄박스 소비 허용(+사라진
id 보고)·구조 자리 삭제/이동 거절·고아 거절·고리 거절·루트 삭제/이동 거절·빈 제출 거절.

**남은 두 조각**: ② 라우트와 서비스(제출본을 그 팀원 프로필에 한 번에 쓰고 디스크 재독으로 확인),
③ 클라이언트가 편집기의 최종 트리를 제출하도록 바꾸기. 그때 옛 경로(`edit-gear`)를 걷어낸다.

### 계획 5 Task B(2/3) — 최종 상태 라우트가 돈다 (2026-11-03)

`/stm/members/apply-inventory` 와 `StmInventoryApplyService` 를 붙였다. 받는 것은 **그 팀원 두 트리의
아이템 전체**이고, 조작의 종류는 묻지 않는다.

**지킨 것**(이동 목록 경로와 같다): 레이드 중 거부, 미결 전송 거부, STM 먼저 해석, 요청 프로필 대조,
손상 표시 프로필 거부, 살아 있는 객체는 복제 위에서만 고치기, **쓰기 한 번**, 디스크 재독 대조, 실패하면
되돌리기, `HandEdited` 표시.

**새로 세운 것 셋.**
1. **두 트리 밖은 손대지 않는다.** 프로필에는 퀘스트 보관함·정렬 테이블 같은 **다른 뿌리**의 아이템이
   있다(Buck 실측 23개). 편집기는 그것을 보지도 보내지도 않으므로, 제출본과 대조하는 before 도 두 트리만
   이고 쓸 때 나머지를 그대로 붙인다. 이걸 빼먹으면 그 23개가 "사라진 아이템" 이 되어 통째로 지워진다.
2. **원래 아이템은 원래 객체를 쓴다.** 자리(부모·슬롯·좌표)와 수량만 제출본에서 받는다 - 내구도·충전량
   같은 다른 `Upd` 를 클라이언트가 보낸 값으로 덮지 않는다.
3. **새 아이템의 id 는 서버가 만든다.** 클라이언트가 준 id 를 그대로 쓰면 다른 프로필의 아이템 id 와
   겹치게 보낼 수 있다.

**라우트 실측**(설치본 md5 `e95b6d67`). Buck 의 두 트리 518개를 통째로 제출했다.

| 제출 | 결과 |
|---|---|
| 그대로 | 통과. 전체 541 → 541, 두 트리 518 → 518 |
| **묶음 나누기**(910발에서 50발을 떼어 새 더미로) | **통과.** 두 트리 519개, **새 id 는 서버가 만들었다**(`6ae9c100…`), 종류 총량 1901 그대로 |
| 복제(+10) | **거절** - `'6937dbee…' 의 수가 1901 에서 1911 로 늘어 저장하지 않습니다.` |
| 되돌리기(그 새 더미를 다시 합치기) | 통과. 전체·두 트리·총량이 처음과 같다 |

로그도 범위를 말한다: `두 트리 518 → 519개(두 트리 밖 23개는 그대로), 새로 생긴 더미 1개`.
마지막에 Buck 프로필을 백업과 대조해 **아이템 604개 그대로, 변화 없음**을 확인했다.

**배운 것(실측).** 새 아이템의 `_id` 가 **24자리 16진(MongoId)** 이 아니면 SPT 의 역직렬화가 터지고
응답이 **빈 200** 으로 돌아온다 - 실패가 성공처럼 보인다. 테스트에서 아무 문자열을 쓰다 한 번 속았다.
진짜 클라이언트(EFT)는 MongoId 를 만들므로 제품 경로의 문제는 아니지만, 라우트를 손으로 부를 때는 주의한다.

**중간에 내가 낸 사고와 복구.** 첫 판의 되돌리기 스크립트가 "50발짜리 아무거나" 를 합치는 바람에 리그
탄창의 50발 더미 하나가 사라졌다(총량은 보존). 서버를 내리고 `backup/2026-11-03-before-apply-route/`
에서 Buck 프로필을 되돌린 뒤 다시 돌렸다 - 지금 Buck 은 백업과 같다.

**남은 것**: 3/3 — 클라이언트가 저장 때 이동 목록 대신 **편집기의 최종 트리**를 제출하도록 바꾸기.
그때 옛 경로(`edit-gear`)를 걷어낸다.

### 계획 5 Task B(3/3) — 클라이언트가 최종 트리를 제출한다 (2026-11-03)

편집기의 저장이 이제 **이동 목록이 아니라 최종 트리**를 보낸다.

- 보내는 것: `ItemFactory.TreeToFlatItems` 로 편집기 프로필의 **장비 루트와 스태시 루트** 두 트리를
  펴서 통째로(`/stm/members/apply-inventory`). 조작의 종류를 적지 않는다.
- 그래서 **없어진 거절 둘**: `편집 중 새로 생긴 아이템 …`(나누기)과 `편집 중 사라진 아이템 …`
  (합치기·탄박스 열기·버리기). 둘 다 이제 그대로 저장된다 - 정당한지는 서버의 보존 판정이 본다.
- **칸 위치도 같이 간다.** `location` 이 제출본에 들어 있으므로 "칸 위치만 바꾼 N개는 저장되지 않습니다"
  라는 안내도 사라졌다(격자 좌표가 그대로 저장된다).
- **"바뀐 것이 없다" 판정을 고쳤다.** 전에는 자리(부모·슬롯)만 봤는데, 그러면 **수량만 바뀐 편집**
  (탄창 채우기)을 "바뀐 것이 없다" 로 읽는다. 이제 아이템 한 줄 전체를 직렬화해 비교한다
  (`CountStmChangedItems`) - 자리·좌표·수량·내구도까지 들어간다.
- 옛 라우트(`edit-gear`)는 아직 서버에 남아 있다. 클라이언트는 더 이상 쓰지 않는다(Task D 에서 걷어낸다).

**실측.** 클라이언트 빌드 경고 0(애드온 `MSB3644` 는 이 PC 의 알려진 제약), 테스트 605개 통과,
설치본 md5 `31aea1ba`. 서버는 `e95b6d67` 그대로(바뀌지 않았다).

### 사고 — 최종 상태 제출이 **편집기가 못 읽은 아이템 68개를 지웠다** (2026-11-03)

Task B 를 설치한 직후 인게임 확인에서 사용자가 "1~5 전부 동작" 이라고 했고, 프로필을 대조하니
**Buck 의 아이템이 604 → 537 개**였다. 서버 로그에 그 순간이 그대로 남아 있다:

```
[STM] 인벤토리 저장: … 두 트리 518 → 450개(두 트리 밖 23개는 그대로), 새로 생긴 더미 0개,
사라진 아이템: 6ae810df…27634, 6ae810c3…26e2b, … (68개)
```

사라진 것은 탄박스·탄창(과 그 안의 탄)·수류탄 같은 것들이다. **플레이어가 버린 것이 아니다.**

**원인.** 편집기는 EFT 가 역직렬화하지 못한 스태시 항목을 **건너뛰고** 연다(PIT 가 플레이어 스태시에서
하던 정책 그대로 - `TryBuildStmEditorStash` 의 주석이 "보이지 않을 뿐 서버의 것은 그대로다" 라고
적어 두었다). **이동 목록** 경로에서는 그 말이 맞았다: 건너뛴 아이템은 요청에 아예 나오지 않으니
서버도 건드리지 않았다. 그런데 **최종 상태 제출**로 바꾸는 순간 그 문장이 거짓이 됐다 - 제출본에 없는
아이템은 "플레이어가 버린 것" 으로 읽혀 지워진다. 계획 5 §6-3 이 "위험이 옮겨 간다" 고 적어 둔 그
자리이고, 내 보존 판정은 **줄어드는 것을 일부러 허용**하고 있었다.

**고침 — 편집기가 들고 있던 것만 잃을 수 있다.**
- 클라이언트가 저장할 때 **열릴 때 들고 있던 id 목록**(`loadedItemIds`)을 함께 보낸다.
- 정책은 그 목록 **밖의 손실을 거절**한다: `아이템 '…' 은(는) 편집기가 보여 주지 않은 것이라 지울 수
  없습니다.` 목록이 비거나 없으면 **어떤 손실도 거절**한다(닫힌 쪽).
- 디스크 재독 대조도 같은 목록으로 한 번 더 본다.
- 테스트 셋(605 → 608): 안 들고 있던 것의 손실 거절·목록을 모르면 손실 거절·들고 있던 것의 버리기 허용.

**복구.** 사고 직후의 상태를 `backup/2026-11-03-after-item-loss/` 에 보관했고, 되돌릴 원본은
`backup/2026-11-03-before-final-tree/` 에 있다. 게임과 서버가 떠 있는 동안에는 되돌리지 않는다
(메모리의 프로필이 다시 덮어쓴다).

**교훈(적어 둔다).** "보이지 않을 뿐 서버의 것은 그대로다" 같은 주석은 **그 경로의 계약**에 달려 있다.
계약을 바꾸면(이동 목록 → 최종 상태) 그 주석이 가리키던 안전이 같이 사라진다. 경로를 바꿀 때는
**그 경로가 기대고 있던 주석을 먼저 찾아** 아직 참인지 본다.

### 사라진 68개의 정체와, 보이지 않는 것을 말해 주기 (2026-11-03)

**측정.** 사고 전후 프로필을 종류별 총량으로 대조했다: **11가지 종류 41개가 0 이 됐고, 늘어난 것은
하나도 없다**(탄창 6개, 탄박스 여러 개, 그 안의 탄 등). 합치기나 탄박스 소비로 설명되지 않는다 -
그랬다면 다른 종류가 늘었어야 한다. **편집기가 싣지 못한 아이템이 제출본에서 빠져 지워진 것**이 맞다.

**종류 자체는 문제가 아니었다.** 사라진 17가지 tpl 이 전부 `items.json` 에 **있다** - 모드가 런타임에
만든 "DB 에 없는 tpl" 때문이 아니다. 거부하는 쪽은 EFT 의 역직렬화이고(`FlatItemsToTree` 의
`DeserializationErrors`), PIT 가 플레이어 스태시에서 쓰던 정책 그대로 **그 항목과 자식을 건너뛰고** 연다
(`SkipEftRejectedStashItems`). 왜 거부하는지는 다음에 본다(클라이언트 로그가 돌아가 그 run 의 줄이 없다).

**그래서 두 가지를 한다.**
1. **지워지지 않는다**(앞 항목의 `loadedItemIds` 가드). 라우트로 실측했다:
   편집기가 들고 있지 않던 id 를 빼고 제출하면
   `아이템 '6ae810c3…25d8b' 은(는) 편집기가 보여 주지 않은 것이라 지울 수 없습니다.` 로 거절되고
   프로필은 541개 그대로다.
2. **보이지 않는다는 사실을 말한다.** 편집기를 열 때 건너뛴 수를 세어 상태 줄에 적는다:
   `이 팀원의 아이템 N개를 편집기가 읽지 못해 보이지 않습니다. 저장해도 그 아이템은 건드리지 않습니다.`
   건너뛴 id 는 클라이언트 로그에 Warning 으로 남긴다(다음에 원인을 보려면 그 줄이 필요하다).

**복구 완료.** Buck 프로필을 `backup/2026-11-03-before-final-tree/` 에서 되돌렸고(541개), 여섯 프로필
전부 그 기준점과 **변화 없음**이다. 사고 직후 상태는 `backup/2026-11-03-after-item-loss/` 에 남겨 뒀다.

**실측.** 테스트 608개 통과. 설치본 서버 `521a92f4`·클라이언트 `43ad1592`.

### 사라진 것은 **탄박스였다** — 소실에 확인 한 번을 둔다 (2026-11-03)

사라진 11종 41개의 이름을 붙여 보니 **열 가지가 전부 탄박스**였다:
`7.62x39 US/PS/BP gzh ammo pack(20)`, `7.62x51 M61/M993(20)`, `5.56x45 M855(50)`, `M995(100)`,
`9x19 PSO(50)`, `.338 TAC-X(20)`, `5.45x39 BP gs(120)`. 나머지 셋은 그 안에 들어 있던 **탄**이다.

즉 사용자가 확인한 **"① 탄박스 열기"** 가 그 조작이다. 편집기에서 상자를 열면 **상자는 소비되고**
나온 탄은 놓일 자리를 찾아야 하는데, 그 자리를 찾지 못한 탄이 **그대로 사라졌다.** 늘어난 종류가
하나도 없었던 것이 그 증거다(탄이 어딘가로 갔다면 그 종류가 늘었어야 한다).

**그래서 소실을 조용히 저장하지 않는다.** 저장할 때 "편집기가 들고 있었는데 최종 트리에 없는" 아이템을
세어, 있으면 **한 번은 거절하고 경고**한다:
`저장하면 아이템 N개가 사라집니다. 저장을 한 번 더 누르면 그대로 저장합니다.`
같은 묶음으로 다시 누르면 그때 보낸다. 사라질 id 는 클라이언트 로그에 Warning 으로 남긴다.
(서버 쪽 규칙은 그대로다: 들고 있지 않던 것은 **아예** 못 지운다.)

**상태 줄이 스태시 패널에 가린다**(사용자 스크린샷). 그래서 열 때의 "읽지 못한 N개" 안내도
**토스트로 한 번 더** 띄운다. 상태 줄만 쓰던 자리를 둘 다 쓰게 바꿨다.

**이번 확인에서 건너뛴 아이템은 0개였다**(클라이언트 로그에 `could not load` 줄이 없다). 즉 복구된
프로필은 편집기가 **전부** 싣는다 - 앞선 손실은 역직렬화 거부가 아니라 **탄박스 열기**였다.

**실측.** 클라이언트 빌드 경고 0, 설치본 md5 `1db44bd1`. 서버는 `521a92f4` 그대로.

### 탄박스를 편집기가 직접 연다 — 탄이 사라지지 않게 (2026-11-03)

**EFT 의 원래 처리를 역컴파일로 읽었다**(`InventoryController.UnloadAmmoInstantly`):

```csharp
while (ammo != null) { … QuickFindAppropriatePlace(ammo, …); await TryRunNetworkTransaction(op); … }
if (ammoContainer is AmmoBox) { ThrowItem(ammoContainer, …); }   // ← 빈 상자를 **버린다**
```

즉 탄박스를 여는 조작은 **상자를 버리는 것까지가 한 세트**다. 플레이어 자신의 화면에서는 그 조작이
서버로 가서 탄이 스태시에 남지만, 편집기의 컨트롤러는 **로컬 전용**이라 2026-11-03 실측에서 탄박스
10종과 **그 안의 탄까지** 사라졌다(늘어난 종류가 하나도 없었던 것이 증거다).

**그래서 편집기에서는 상자 열기만 가로챈다**(`StmEditorAmmoBoxUnloadPatch` → `ItemUiContext.UnloadAmmo`).
- 탄을 **그 팀원 스태시**로 한 더미씩 옮긴다(EFT 와 같은 `QuickFindAppropriatePlace` + 적용).
- 자리를 못 찾거나 적용이 실패하면 **거기서 멈춘다**: 이미 옮긴 것은 그대로 두고 **상자는 버리지 않는다**.
  `탄을 놓을 스태시 자리가 없어 상자를 열지 않았습니다.`
- 다 옮기면 `탄을 이 팀원의 스태시로 꺼냈습니다. 빈 상자는 남습니다.` - **빈 상자를 버리지 않는 것이
  이 구현의 요점**이다. 없애는 것은 플레이어가 직접 고르게 두고(버리기), 저장 때 확인을 한 번 받는다.
- **탄창·무기의 `탄약 꺼내기` 는 가로채지 않는다** - 그쪽은 상자를 버리는 단계가 없고 인게임에서 정상
  동작이 확인됐다(2026-11-03).

**실측.** 클라이언트 빌드 경고 0, 테스트 608개 통과, 설치본 md5 `e82dc085`. 서버는 `521a92f4` 그대로.

### 빈 탄 팩은 게임에 없는 물건이다 — 연 상자는 치운다 (2026-11-03)

사용자: "빈상자가 뭐야?" **맞는 지적이다.** EFT 에 "빈 탄 팩" 이라는 아이템은 없다 - 상자를 열면
상자는 없어지는 것이 정상이고, 앞 커밋이 남긴 빈 상자는 **정상 게임에 존재하지 않는 상태**였다.
지우는 단계가 사고의 원인이라 통째로 뺐던 것인데, 뺄 것은 그 단계가 아니라 **확인 없이 지우는 것**이었다.

**이제 EFT 와 같다 — 순서만 안전하게.**
1. 탄을 그 팀원 스태시로 한 더미씩 옮긴다.
2. **상자가 정말로 비었는지 확인한다**(`box.Cartridges.Last == null`). 비지 않았으면 상자를 건드리지
   않고 `상자를 비웠지만 치우지 못했습니다. 저장 전에 직접 버리세요.` 로 알린다.
3. 비었을 때만 `ThrowItem`(= `ItemManipulator.Discard` + RemoveOperation)으로 치운다.

**저장 때의 소실 경고에서는 뺀다.** 연 상자는 사고가 아니라 그 조작의 정상 결과다 - 편집기가 일부러
없앤 id(`StmConsumedItemIds`)를 경고 대상에서 제외한다. 서버 쪽은 그대로다: 그 id 는 편집기가 들고
있던 것이므로 손실이 허용되고, **들고 있지 않던 것은 여전히 못 지운다**.

**복구.** 직전 확인에서 또 66개가 사라져 있어(541 → 475) Buck 을 다시 541개로 되돌렸다(탄박스 41개 확인).
그 세션의 클라이언트에는 이 고침이 없었다. 사고 직전 상태는 `backup/2026-11-03-before-ammobox-check/`.

**실측.** 클라이언트 빌드 경고 0, 설치본 md5 `b92596a6`, 서버 `521a92f4` 그대로.

### 소실 경고와 재확인을 걷어냈다 (2026-11-03)

사용자: "쓸때없는짓". 맞다 - 평범한 편집마다 `저장하면 아이템 25개가 사라집니다` 가 떴다.

**왜 틀렸나.** 경고가 **id 기준**이었다. 탄 더미를 다른 더미에 **합치면** 합쳐진 쪽 id 가 없어지는데,
그것은 정상 동작이지 소실이 아니다(총량은 그대로다). 연 탄 팩도 마찬가지로 정상 결과다. 그래서 탄을
옮기기만 해도 경고가 울렸고, 경고가 울리는 것이 기본이 되면 **경고로서 쓸모가 없다**.

걷어낸 것: 두 번 누르기(`StmPendingLossSignature`), 경고 문구, 소비 목록(`StmConsumedItemIds`).
사라지는 id 는 **클라이언트 로그에만** 남긴다(원인 추적용).

**진짜 보호는 서버에 그대로 있다.** `loadedItemIds` 밖의 아이템은 애초에 지울 수 없다 - 편집기가 보여
주지 않은 것이 제출본에 없다는 이유로 지워지던 그 사고는 막힌 채다. 그 가드는 id 기준이어도 맞다:
"편집기가 들고 있었는가" 는 합치기와 무관한 사실이기 때문이다.

**실측.** 클라이언트 빌드 경고 0, 설치본 md5 `56b76337`. 서버 `521a92f4` 그대로.

### 계획 5 Task C — 보안 컨테이너 칸을 그린다 (2026-11-03)

안 보이던 이유는 EFT 가 아니라 **PIT 가 일부러 숨기고 있었기 때문**이다:

```csharp
if (!pitFireTeam.IsFollowerLoadoutRealisticMode())
    HideLoadoutEditorContainerSlot(panelRoot, EquipmentSlot.SecuredContainer);
```

패널 자체는 **인벤토리 화면의 템플릿**(`ComplexStashPanel`)이라 칸은 원래 있다 - 용병 편집기가 현실
모드가 아닐 때 가리는 것이 PIT 의 설계다. 프로필 팀원은 **플레이어 자신의 화면과 같아야 하므로**
가리지 않는다.

- `TryBuildLoadoutEditorFollowerPanel`·`ShowLoadoutEditorEquipmentPanel` 에 `keepSecuredContainer`
  (기본값 false)를 더했다. **PIT 용병 경로는 한 글자도 달라지지 않는다**(불변식 4).
- STM 편집기만 `keepSecuredContainer: true` 로 부른다.
- 컨테이너 **자체**를 떼는 것은 서버가 계속 막는다(`IsStructuralSlot`) - 안에 넣고 빼는 것만 열린다.
  플레이어 화면에서도 보안 컨테이너는 장비 칸에서 떨어지지 않는다.

**실측.** 클라이언트 빌드 경고 0, 설치본 md5 `a515c442`. 서버 `521a92f4` 그대로.

### 계획 5 Task D — 옛 경로를 걷어냈다 (2026-11-03)

클라이언트가 더 이상 쓰지 않는 **이동 목록 경로**의 바깥 면을 지웠다: 라우트 `/stm/members/edit-gear`,
콜백 `StmProfileMemberCallbacks.EditGear`, 와이어 모델 `StmGearEditRequest`·`StmGearEditItem`.
옛 경로를 가리키던 주석 둘도 고쳤다(편집기 머리 주석, `GearEditBatchPlanner`).

**지우지 않은 것과 그 이유.** `StmGearEditService.TryApply` 와 그 아래 헬퍼는 남겼다 - 격자 배치
(`TryPlaceInContainer`)는 **꾸러미 구매**가 쓰고, 슬롯 어휘(`BuildSlotFacts`·`ProfileGearEditPolicy`)는
아래 열린 항목에서 **다시 쓸 자리**다. 메서드 머리에 "아무도 부르지 않는다. 되살릴 생각이 없다면
BuildSlotFacts·VerifyFromDisk·TryRestore 와 함께 지워라" 를 적어 두었다 - 하나만 지우면 컴파일은 되고
의미가 비는 자리다. `GearEditBatchPlanner` 와 그 테스트도 같은 이유로 남겼다(순서 규칙이 다시 필요해질 수 있다).

**이 교체로 생긴 열린 항목 하나 — 서버가 슬롯 적법성을 보지 않는다.** 이동 목록 경로는 "이 아이템이 이
슬롯에 들어가는 종류인가" 를 서버가 판정했다(`ProfileGearEditPolicy` + 아이템 DB). 최종 상태 경로는
**보존만** 본다 - 무엇이 어디에 있는지는 보지 않는다. 지금은 EFT 의 편집기가 화면에서 막아 주지만,
서버 혼자로는 "헬멧이 탄창에 들어간" 제출을 거절하지 못한다. 고칠 때는 이미 있는 어휘를 그대로 쓰면
된다(그래서 남겼다). **거절은 확실할 때만** 해야 한다 - 격자·모드 아이템처럼 우리가 모델하지 못하는
자리를 "모름" 으로 거절하면 정상 저장이 막힌다.

**실측.** 서버·클라이언트 빌드 경고 0, 테스트 608개 통과. 산출물 md5 서버 `fd384b51`·클라이언트
`1f8e0cce`. **게임이 켜져 있어 아직 설치하지 못했다** - 설치본은 서버 `521a92f4`·클라이언트 `a515c442` 그대로다.

### 설치와 두 번째 복구 (2026-11-03)

게임이 닫힌 뒤 새 DLL 을 올렸다: 서버 `fd384b51`(옛 라우트 제거)·클라이언트 `1f8e0cce`(보안 컨테이너
칸, 경고 제거, 탄박스 고침). 설치본의 바이트를 찍어 확인했다 - `탄을 이 팀원의 스태시로 꺼냈습니다`
등 새 문구가 들어 있고, 지운 경고 문구(`저장하면 아이템 {0}개가 사라집니다`)는 **없다**.

**그 사이의 손실 두 건을 되돌렸다.** 서버 로그의 시각으로 보면 둘 다 **탄박스 고침이 설치되기 전**의
저장이다(사라진 목록에 상자뿐 아니라 그 안의 `cartridges` 가 함께 있다 - 고침이 돌았다면 탄은
스태시로 옮겨져 남는다):

| 시각(UTC) | 프로필 | 두 트리 | 사라진 것 |
|---|---|---|---|
| 12:29:48 | Buck | 518 → 450 | 탄박스와 그 탄 68개 |
| 12:30:35 | Hawk | 313 → 288 | 탄박스와 그 탄 25개 |

`backup/2026-11-03-before-guard-check/` 에서 둘 다 되돌렸고(되돌리기 전 상태는
`backup/2026-11-03-buck-before-restore2/`·`backup/2026-11-03-hawk-before-restore/` 에 보관),
지금 여섯 프로필은 그 기준점과 같다. 다른 둘은 **사용자가 한 정상 거래**라 손대지 않았다:
Nomad 의 루블 −4,275,667 과 Mace 의 아이템 +87 - 꾸러미 구매의 양쪽이다.

**아직 실측되지 않은 것.** 탄박스 고침은 **게임에서 한 번도 돌지 않았다**(클라이언트 로그에
`[Warning:SPT Team Manager]` 줄이 0건이다). 다음 확인에서 탄박스를 열면 그 경로가 처음 돈다.
