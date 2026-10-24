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
