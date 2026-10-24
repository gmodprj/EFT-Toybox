# Spt_Mods

`E:\Spt`에 설치한 SPT 4.1.6(Single Player Tarkov) 모드의 관리 폴더.

- 설치된 모드: [doc/INSTALLED-MODS.md](doc/INSTALLED-MODS.md)
- 작업 이력: [doc/WORKLOG.md](doc/WORKLOG.md)
- 모드 추가·검증·되돌리기 절차: [doc/PROCEDURE.md](doc/PROCEDURE.md)
- 문서 색인: [doc/README.md](doc/README.md)

## 구성

| 경로 | 내용 | git |
| --- | --- | --- |
| `doc/` | 기록·문서 | 추적 |
| `tools/` | Forge API 조회, 다운로드, 설치·테스트, 목록 생성 스크립트 | 추적 |
| `downloads/` | 원본 압축 파일 (약 6.2 GB) | 제외 |
| `extracted/` | 압축 해제본, 설치 원본 (약 6.4 GB) | 제외 |
| `backup/` | 원본 설정·프로필·SVM 프리셋 백업 (프로필 개인 데이터 포함) | 제외 |

## 도구

| 스크립트 | 용도 |
| --- | --- |
| `tools/search-mods.mjs` | Forge에서 이름·소개로 모드 검색 |
| `tools/mod-detail.mjs` | 모드별 4.1.6 호환 버전·의존성·결속·설명 |
| `tools/survey-candidates.mjs` | 후보 여러 개 일괄 조회 |
| `tools/resolve-versions.mjs` | 후보의 호환 버전과 의존성 트리 |
| `tools/download-stage.mjs` | `doc/download/manifest-<stage>.json` 기준 다운로드·크기 검증 |
| `tools/install-and-test.ps1` | 덮어쓰기 금지 설치 + 기록 + 테스트 서버 확인 |
| `tools/gen-installed-mods.mjs` | 설치 기록으로 `doc/INSTALLED-MODS.md` 생성 |

Node 24, PowerShell 7 기준. Forge API는 인증 없이 읽기 전용.
