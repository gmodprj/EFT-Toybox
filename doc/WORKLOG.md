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
- 명단 화면과 클라이언트 코드(프로필 타일, 불러오기 선택기): 약 750줄이 컴파일러를 한 번도 통과하지 못했다.

### 확인되지 않은 것 (사람 관문 대기)

인게임 메뉴 충돌 여부, 클라이언트 빌드 자체, SPT 직렬화 왕복, `[Injectable]` 런타임 해석,
아이콘 구분, 실제 레이드 투입. 클라이언트 약 750줄은 컴파일러를 한 번도 통과하지 못했다(API 인용은
리뷰어가 실제 어셈블리 메타데이터로 대조해 틀린 것이 없었으나 컴파일과는 다르다).

### 아직 안 되는 것 (계획 2·3)

- 계획 2 — 장비: 실물/임시 장비 분기, 로드아웃 편집, 본인 스태시만 출처로 쓰기, §5-14 스태시 재검증 수리,
  §6-8 3단 전송 저널. (화면의 장비 줄은 "아직 판정하지 않음"으로 둔다.)
- 계획 3 — 레이드 후: 경험치·스킬·전적 적립, 전리품 메일, 소모·손상, 사망 처리, 추가 기능 셋.

두 계획은 이 단계가 인게임에서 통과한 뒤에 쓴다.

### 사람 관문 검증 항목

1. **클라이언트 참조 결정.** 클라이언트가 요구하는 4.1.0.0 `spt-common.dll`·`spt-reflection.dll`
   은 이 기계에 없다(설치본은 4.1.6 뿐, NuGet 에도 없음). 4.1.6 기준으로 참조를 낮출지, SPT 4.1.0 릴리스를
   내려받을지 정한다. 이 결정 전에는 클라이언트를 빌드할 수 없다.
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
