# 문서 색인

대상: `E:\Spt` (SPT 4.1.6, EFT 0.16.9.5.40743). 2026-10-24 기준 모드 93개 설치.

## 먼저 볼 문서

| 문서 | 내용 |
| --- | --- |
| [MODS-GUIDE.md](MODS-GUIDE.md) | 설치된 93개가 무엇을 하는 모드인지 — 기능별 10묶음, 결속 목록, 지우면 안 되는 파일, 손볼 설정 |
| [INSTALLED-MODS.md](INSTALLED-MODS.md) | 지금 설치된 모드 전체 — 버전, 게임/서버 위치, 프로필 결속 여부, 설치 기록 (자동 생성) |
| [WORKLOG.md](WORKLOG.md) | 단계별 작업 이력, 발생한 문제와 조치, 바로잡은 안내 |
| [PROCEDURE.md](PROCEDURE.md) | 모드 추가 절차(조회→다운로드→설치→검증→되돌리기), SVM 메모, 무시해도 되는 로그 메시지 |
| [CANDIDATES-416.md](CANDIDATES-416.md) | 4.1.6 호환 미설치 후보 136개 — 분야별 버전·용량·결속·의존성 |
| [PLAN-STAGE5.md](PLAN-STAGE5.md) | 5차 설치 계획과 실행 결과 — 설치 52 / 보류 32 / 제외 52, 단계별 결과 |
| [../src/SptKoreanConfig/README.md](../src/SptKoreanConfig/README.md) | F12 설정 창 한글화 플러그인 — 구조, 사전 형식, 빌드·설치 |

## 기록 폴더

| 폴더 | 내용 |
| --- | --- |
| `install-records/` | 설치 단위별 설치 파일 목록 `installed-<Tag>.txt` (`모드폴더<TAB>E:\Spt 기준 경로`). 되돌릴 때 이 파일 목록만 삭제 |
| `download/` | 단계별 다운로드 목록 `manifest-stage*.json`, 결과·크기 검증 `download-results*.json` (1차 결과는 `download-results.json`), 5차 경로 충돌 검사 `conflict-check-stage5-*.txt` |
| `server-tests/` | 설치 직후 테스트 서버 출력. `server-test1~7`은 1~2차, `server-test-<Tag>`는 2차 B 이후 |
| `stage2-survey/` | 2·3차 후보 76개 Forge 조회 결과, 5차 후보 조회(`survey-candidates-416.json`, `candidate-groups-416.json`, `survey-deps-416.json`) |

설치 기록에 `installed-<Tag>.dirs.txt`가 있으면 그 단계에서 **빈 폴더**를 만들었다는 뜻이다(모드가 폴더 존재를 요구하는 경우). 되돌릴 때 함께 지운다.

## 설치 단위 이름

| 단계 | Tag | 모드 |
| --- | --- | --- |
| 1차 | `batch1`~`batch4` | UI·탄약 정보·개조 화면 편의 23개 |
| 2차 | `stage2-A`, `stage2-C`, `stage2-B1~B3` | 편의 5, SVM, WTT CommonLib·Armory·Artem |
| 3차 | `stage3-T1~T3`, `stage3-L1~L4` | Skipper·Open Sesame·give-ui, Lotus·Scorpion·Ref·HarryHideout |
| 4차 | `stage4-U1`, `stage4-U2` | Kaeno-TraderScrolling, TraderSpace |
| 5차 | `stage5A-<id>` ~ `stage5E-<id>` | A UI 21, B 총기 9, C 탄약 6, D 상인 10, E 치트·도구 6 (모드 id별로 하나씩) |

## 백업 (git 제외, `../backup/`)

| 폴더 | 시점 |
| --- | --- |
| `clean-2026-10-24/` | 모드 설치 전 BepInEx·서버 설정·doorstop 원본 |
| `profiles-before-batch2` ~ `batch4` | 1차 2~4묶음 직전 프로필 |
| `profiles-before-stage2A`, `stage2C`, `stage2B` | 2차 각 묶음 직전 |
| `profiles-before-stage3` | 3차 직전 (2차 B 콘텐츠로 플레이한 뒤) |
| `profiles-before-stage5A` ~ `stage5E` | 5차 각 단계 직전 |
| `profiles-before-stage5C-2840`, `-2990`, `stage5D-3124`·`3128`·`3024`·`2966`·`3085`·`2419` | 결속 모드 하나하나 설치 직전 |
| `svm-presets/` | SVM 프리셋 수정 전 사본 |
