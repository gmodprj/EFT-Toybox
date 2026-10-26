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
