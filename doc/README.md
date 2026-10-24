# 문서 색인

대상: `E:\Spt` (SPT 4.1.6, EFT 0.16.9.5.40743). 2026-10-24 기준 모드 41개 설치.

## 먼저 볼 문서

| 문서 | 내용 |
| --- | --- |
| [INSTALLED-MODS.md](INSTALLED-MODS.md) | 지금 설치된 모드 전체 — 버전, 게임/서버 위치, 프로필 결속 여부, 설치 기록 (자동 생성) |
| [WORKLOG.md](WORKLOG.md) | 단계별 작업 이력, 발생한 문제와 조치, 바로잡은 안내 |
| [PROCEDURE.md](PROCEDURE.md) | 모드 추가 절차(조회→다운로드→설치→검증→되돌리기), SVM 메모, 무시해도 되는 로그 메시지 |

## 기록 폴더

| 폴더 | 내용 |
| --- | --- |
| `install-records/` | 설치 단위별 설치 파일 목록 `installed-<Tag>.txt` (`모드폴더<TAB>E:\Spt 기준 경로`). 되돌릴 때 이 파일 목록만 삭제 |
| `download/` | 단계별 다운로드 목록 `manifest-stage*.json`, 결과·크기 검증 `download-results*.json` (1차 결과는 `download-results.json`) |
| `server-tests/` | 설치 직후 테스트 서버 출력. `server-test1~7`은 1~2차, `server-test-<Tag>`는 2차 B 이후 |
| `stage2-survey/` | 2·3차 후보 76개 Forge 조회 결과 |

## 설치 단위 이름

| 단계 | Tag | 모드 |
| --- | --- | --- |
| 1차 | `batch1`~`batch4` | UI·탄약 정보·개조 화면 편의 23개 |
| 2차 | `stage2-A`, `stage2-C`, `stage2-B1~B3` | 편의 5, SVM, WTT CommonLib·Armory·Artem |
| 3차 | `stage3-T1~T3`, `stage3-L1~L4` | Skipper·Open Sesame·give-ui, Lotus·Scorpion·Ref·HarryHideout |
| 4차 | `stage4-U1`, `stage4-U2` | Kaeno-TraderScrolling, TraderSpace |

## 백업 (git 제외, `../backup/`)

| 폴더 | 시점 |
| --- | --- |
| `clean-2026-10-24/` | 모드 설치 전 BepInEx·서버 설정·doorstop 원본 |
| `profiles-before-batch2` ~ `batch4` | 1차 2~4묶음 직전 프로필 |
| `profiles-before-stage2A`, `stage2C`, `stage2B` | 2차 각 묶음 직전 |
| `profiles-before-stage3` | 3차 직전 (2차 B 콘텐츠로 플레이한 뒤) |
| `svm-presets/` | SVM 프리셋 수정 전 사본 |
