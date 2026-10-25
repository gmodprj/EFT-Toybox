# 설치·검증·되돌리기 절차

이 저장소에서 모드를 추가할 때 쓰는 절차. 모든 경로는 `E:\Spt`(설치본)와 `E:\Spt_Mods`(관리 폴더) 기준.

## 폴더 역할

| 경로 | 역할 | git |
| --- | --- | --- |
| `doc/` | 기록·문서 | 추적 |
| `tools/` | 조회·다운로드·설치 스크립트 | 추적 |
| `downloads/<stage>/` | Forge에서 받은 원본 압축 파일 (`<modId>__<파일명>`) | 제외 |
| `extracted/<stage>/` | 압축 해제본. 설치 원본 (`<modId>__<이름>/` 아래가 `E:\Spt` 기준 경로) | 제외 |
| `backup/` | 원본 설정, 단계별 프로필 백업, SVM 프리셋 백업 | 제외 (프로필 포함) |

## 1. 후보 조회

1. 모드 id 찾기: `node tools/search-mods.mjs "검색어" ...` (이름·소개 퍼지 검색)
2. 상세·호환 확인: `node tools/mod-detail.mjs <id> ...` → 4.1.6 호환 최신 버전, 의존성, 결속 여부, 설명
3. 여러 개 일괄: `node tools/survey-candidates.mjs '{"그룹":[id,...]}' out.json`
4. 의존성 트리: Forge `GET /api/v0/mods/dependencies?mods=<id>:<ver>,...&spt_version=4.1.6`
   - 메타데이터의 의존 버전이 오래된 경우가 있음(예: Color Converter API). 4.1.6 호환 버전이 있으면 그것을 씀.

판단 기준

- **결속**(Forge `shows_profile_binding_notice`)이면 플레이 후 제거 시 프로필이 깨질 수 있음 → 프로필 백업 후 하나씩.
- SVM으로 되는 기능(FIR, 구매 제한, 무게, 탈출구, 경험치, 열쇠 무제한 등)은 별도 치트 모드를 넣지 않음.
- 같은 화면을 바꾸는 모드(예: 개조 화면)는 함께 넣고 게임에서 동작 확인.

## 2. 다운로드

1. `doc/download/manifest-<stage>.json` 작성: `{ mods: [{ group, id, name, version, size, link, ... }] }`
   - `link`는 Forge `.../mod/download/<id>/<slug>/<version>` (GitHub 릴리스 등으로 리다이렉트)
2. `node tools/download-stage.mjs <stage>` → `downloads/<stage>/`, 결과 `doc/download/download-results-<stage>.json`
   - 스트리밍 저장(수 GB 대응), `size`와 바이트 단위 비교.
3. 해제: Windows 기본 `tar.exe -xf <파일> -C extracted/<stage>/<파일명>` (zip·7z 모두 가능, 7-Zip 불필요)
4. 구조·충돌 검사: 각 파일의 상대경로가 `E:\Spt`에 이미 있는지, 모드끼리 겹치는지 확인 → 둘 다 없어야 진행.

## 3. 설치

전제: 게임·서버가 꺼져 있음(게임 전용 모드는 서버가 켜져 있어도 됨).

1. 프로필 백업: `E:\Spt\SPT_Runtime\user\profiles\*` → `backup/profiles-before-<단계>/`
2. 설치+테스트: 
   ```powershell
   E:\Spt_Mods\tools\install-and-test.ps1 -Stage stage3 -Pattern '1232__*' -Tag 'stage3-L1-Lotus'
   ```
   - 덮어쓸 파일이 하나라도 있으면 아무것도 복사하지 않고 중단.
   - 설치한 파일을 `doc/install-records/installed-<Tag>.txt`에 기록(`모드폴더<TAB>상대경로`).
   - 테스트 서버를 숨김 실행 → `Loaded self-signed certificate`(시작 완료) 또는 실패까지 대기 → 종료. 출력은 `doc/server-tests/`.
   - 테스트 중 `user\mods`에 새로 생긴 파일(설정 등)도 기록에 추가.
   - `-SkipTest`: 복사만. 여러 개를 넣고 마지막에 한 번 테스트할 때.
3. 결속 모드는 하나씩 설치·테스트.
4. 목록 갱신: `node tools/gen-installed-mods.mjs` → `doc/INSTALLED-MODS.md`

## 4. 검증

| 항목 | 위치 | 볼 것 |
| --- | --- | --- |
| 서버 모드 로드 | 테스트 출력 / `E:\Spt\SPT_Runtime\user\logs\spt\spt<날짜>.log` | `ModLoader: 총 N 개`, 각 `... 불러옴`, `[Error]`/`[Critical]` |
| 서버 시작 | 같은 곳 | `시작된 웹서버 https://127.0.0.1:6969`, `Loaded self-signed certificate` (완료 문구 자체는 랜덤) |
| 게임 플러그인 | `E:\Spt\BepInEx\LogOutput.log` | `N plugins to load`, `Loading [이름 버전]`, `[Error`/`[Fatal` |
| 게임 실행 여부 | `LogOutput.log` 수정 시각, `E:\Spt\Logs\log_*`, 런처 로그 | 설치 시각 이후인지 (이전이면 새 모드는 아직 로드된 적 없음) |
| 프로필 영향 | 프로필 해시 vs 백업 | 서버 시작 시 재저장으로 시각은 바뀌어도 내용은 같아야 함 |

주의

- 서버 로그 시각은 UTC(한국 시간 −9시간).
- 사용자 서버가 켜진 상태에서 테스트 서버를 띄우면 6969 포트 충돌로 웹서버는 안 뜸. 모드 로드까지는 확인 가능하지만 **플리마켓 생성 이후 단계는 확인 불가**.
- 설치 후 서버를 재시작하지 않으면 서버 쪽 변경은 적용되지 않음(프로세스 시작 시각으로 확인).
- 서버의 웹 인증이 켜져 있어 API로 상점 목록 등을 직접 조회할 수 없음.

## 5. 되돌리기

- 모드 하나: 해당 `doc/install-records/installed-<Tag>.txt`에 적힌 파일만 `E:\Spt`에서 삭제.
- 결속 모드를 이미 플레이한 뒤 제거: 해당 단계 이전 프로필 백업(`backup/profiles-before-*`)으로 복원해야 함.
- 전체 초기화: `backup/clean-2026-10-24`(BepInEx·서버 설정·doorstop 원본) + 설치 기록 전체 삭제.
- SVM 프리셋: `backup/svm-presets/`.

## SVM 메모

- 설정: `E:\Spt\Greed.exe` → 프리셋 저장·적용 → **서버 재시작**.
- 현재 프리셋 `Noname`. `Hideout.RemoveConstructionsRequirements`는 **false 유지**(true면 HarryHideout 상점이 빔). Greed에서 다시 저장할 때 이 항목 확인.
- `CSM.EnableCSM`을 켜면 Greed가 `BepInEx\plugins\HideSpecialIconGrids.dll`을 설치함.

## 알려진 정상 메시지 (무시)

| 출처 | 메시지 | 의미 |
| --- | --- | --- |
| 게임 | `HarmonyX ... You should only patch implemented methods` | 패치 방식 안내 (UI Fixes, WTT 등) |
| 게임 | `FixPluginTypesSerialization ... references Fika.Core which doesn't exist` | Fika 미설치라 Fika 연동 DLL 제외 |
| 게임 | `[Error][DirectReload] Patch FAILED to enable: FikaHostReloadMirror — Fika ObservedFirearmController not found` | 같은 이유. **Error 등급으로 찍히지만 정상**이다. Fika를 쓰지 않으면 이 패치는 걸 대상이 없다 |
| 게임 | `WTT-ClientCommonLib [HideoutIcons]/[MarkTextures] No ... in manifest` | 해당 리소스를 쓰는 모드 없음 |
| 게임 | `DanW-OpenSesame Cannot find translations for locale "xx"` | 번역 없는 언어는 영어 사용(한국어는 해당 없음) |
| 서버 | `[SVM] Initialization cancelled: Preset or/and Loader is not found` (Error 등급) | 프리셋 미생성. 지금은 해당 없음 |
| 서버 | `[SVM] Remove backpack restrictions - empty filters ... Not an error` | SVM 배낭 제한 해제 옵션 안내 |
| 서버 | `트레이더 638f541a29ffd1183d187f57 찾을 수 없음` | 가격 조회 시 임시 값 대체 |
| 서버 | `[SVM] Initialization complete. ...`, `[SCORPION] ...`, 시작 완료 문구 | 실행마다 바뀌는 농담 문구 |
| 게임·서버 | DLL 표기 버전이 사이트와 다름 (Armory 2.0.5, TraderScrolling 4.0.0) | 제작자가 DLL 버전을 안 올림. 파일 크기로 릴리스 일치 확인 |

## 문제와 해결

| 증상 | 원인 | 해결 |
| --- | --- | --- |
| 서버 `Hideout Harry 상인의 플리마켓 거래를 생성하지 못했습니다, 물품이 없습니다`가 10초마다 | SVM `RemoveConstructionsRequirements: true` | false로 변경 후 서버 재시작 |
| 상인 줄이 플레이어 패널 뒤로 넘침 | 커스텀 상인 증가 | Kaeno-TraderScrolling + TraderSpace |
| 테스트 서버 `Failed to start the web server ... 6969 AddressAlreadyInUse` | 다른 서버가 포트 사용 중 | 사용자 서버를 끄고 테스트하거나, 모드 로드만 확인 |
| Forge API 429 | 호출 제한 | 스크립트가 `retry-after`(기본 15초)만큼 기다렸다 재시도 |
