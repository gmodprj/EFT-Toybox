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
3. 해제: Windows 기본 bsdtar 를 **절대경로로** 부른다 — `/c/Windows/System32/tar.exe -xf <파일> -C extracted/<stage>/<파일명>` (zip·7z 모두 가능, 7-Zip 불필요). Git Bash 에서 `tar.exe` 로만 쓰면 `/usr/bin/tar.exe` 의 GNU tar 가 먼저 잡혀 zip 을 `This does not look like a tar archive` 로 거부한다
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
5. 기록 검사: `node tools/check-install-records.mjs` → **`OK — 0/0`** 이어야 한다(문제가 있으면 종료 코드 1).
   - `[1]` 기록에 있는데 설치본에 없는 경로, `[2]` 서로 다른 모드가 같은 경로를 기록한 것을 본다.
   - **이 단계에서 넣은 모드와 상관없는 다른 모드가 걸릴 수 있다.** 모드가 들어오면 다른 모드의 설정 항목·파일이 늘어나기 때문이다(보기: 조명 모드 하나가 들어와 No Barrel Shadow 기록에 46줄이 생겼다).
   - 없는 파일이 나와도 **유실로 단정하지 않는다.** 2번의 마지막 항목대로 테스트 중 새로 생긴 파일도 기록에 들어가는데, 그것을 누가 만들었는지는 기록에 남지 않는다 — 서버나 다른 모드가 만든 파일이 그때 설치 중이던 모드 몫으로 적힌다. 판정 전에 셋을 본다: ① 해제본 `extracted/<단계>/<모드>/`에 그 경로가 있나, ② `install-records/removed/`에 그 경로를 가진 제거 기록이 있나, ③ 같은 경로를 여러 모드가 적었나.
   - 지울 때는 "없는 것 전부"가 아니라 아는 종류만 지우고 지운 내역을 파일로 남긴다(보기: `doc/install-records/README-removed-stale-lines-2026-11-06.md`).


### STM(SPT Team Manager)과 PIT Fireteam은 함께 설치하지 않는다

- STM 은 PIT Fireteam 의 포크다. 설계 §6-1 도 "설치하면 PIT는 뺀다"로 못박았다.
- 이유: 둘이 같은 서버 라우트 `/singleplayer/pitfireteam/*` 를 쓴다. 업스트림 머지를 살리려고 라우트 문자열을 바꾸지 않았으므로, 같이 두면 한쪽 요청을 다른 쪽이 받는다.
- GUID·어셈블리·서버 모드 폴더(`SptTeamManager-ServerMod`)·팀원 DB 폴더(`stm-teammates*`)는 이미 갈라 놓았으므로 파일은 섞이지 않는다. 문제는 라우트뿐이다.
- `tools/install-sptteammanager.ps1` 이 이를 강제한다. `BepInEx\plugins\pitFireTeam` 또는 `SPT_Runtime\user\mods\pitFireTeam-ServerMod` 가 있으면 아무것도 복사하지 않고 중단하며, 지울 폴더를 알려 준다. PIT 는 사람이 직접 뺀다.
- `-Remove` 는 설치한 파일만 지우고 `Resources\settings.json` 과 `Resources\stm-teammates*` 는 남긴다.

## 4. 검증

| 항목 | 위치 | 볼 것 |
| --- | --- | --- |
| 서버 모드 로드 | 테스트 출력 / `E:\Spt\SPT_Runtime\user\logs\spt\spt<날짜>.log` | `ModLoader: 총 N 개`, 각 `... 불러옴`, `[Error]`/`[Critical]` |
| 서버 시작 | 같은 곳 | `시작된 웹서버 https://127.0.0.1:6969`, `Loaded self-signed certificate` (완료 문구 자체는 랜덤) |
| 게임 플러그인 | `E:\Spt\BepInEx\LogOutput.log` | `N plugins to load`, `Loading [이름 버전]`, `[Error`/`[Fatal` |
| 게임 실행 여부 | `LogOutput.log` 수정 시각, `E:\Spt\Logs\log_*`, 런처 로그 | 설치 시각 이후인지 (이전이면 새 모드는 아직 로드된 적 없음) |
| 프로필 영향 | 프로필 해시 vs 백업 | 서버 시작 시 재저장으로 시각은 바뀌어도 내용은 같아야 함 |
| 설치 기록 정합성 | `node tools/check-install-records.mjs` | `OK — 0/0`. 없는 경로·중복 기록 (§3-5 의 주의를 함께 읽는다) |

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
| 게임 | `[Error][SPTBattleAmbience] Couldnt find map config for location: labyrinth`·`: icebreaker` | 포팅본 맵 둘에 `Assets\configs\maps\<맵>.json` 이 없다. 그 맵에서만 전투 소음이 빠지고 나머지는 그대로다(2026-11-14 실측) |
| 게임 | `[Error][Manimal-Icebreaker] [Firewall] SkillsExtended.Client.Skills's hook ... threw on the icebreaker — swallowed` | Icebreaker 백포트가 제 방화벽으로 **일부러 삼킨 것**이다(그 모드의 훅만 건너뛰고 뒤의 훅은 돈다). 다만 그 맵에서는 Skills Extended 의 기동 훅이 안 돈다 |
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
| 서버 `SPT에 존재하지않는 상인 <id>가 프로필에서 발견되었습니다`가 로그인마다 | 상인 모드를 지웠으나 프로필 `characters.pmc.TradersInfo`에 평판 항목이 남음 (`6952ced4bcc1dd1e3c80dfcb` = The Quartermaster) | 경고 1줄뿐이므로 두어도 된다. 지우려면 프로필 백업 후 그 키 하나만 뺀다. **서버가 안내하는 `core.json`의 `removeModItemsFromProfile: true`는 켜지 않는다** — 결속 모드의 멀쩡한 아이템까지 지울 수 있다 |
| 게임 `[UTAE] Could not load the editor baseline` + `HandbookFilterPanel.RegisterItem` 인덱스 범위 | UTAE가 퀘스트로 잠긴 매물까지 한 번에 그리드에 띄워 바닐라 가정을 깸 | 그 상인의 편집 화면만 안 뜨고 게임은 정상. 조정용 도구라 평소 플레이에는 영향 없음 |
