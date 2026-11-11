# Spt_Mods

`E:\Spt`에 설치한 SPT 4.1.6(Single Player Tarkov) 모드의 관리 폴더. 모드를 **덮어쓰지 않고** 넣고,
무엇이 어디로 들어갔는지 파일 단위로 적어 두고, 되돌릴 수 있게 하는 것이 목적이다.

**2026-11-10 기준 모드 304개 / 설치 기록 15,947줄. 서버 모드 174개 · 게임 플러그인 246개(둘 다 실측).**

- 설치된 모드: [doc/INSTALLED-MODS.md](doc/INSTALLED-MODS.md) (목록, 자동 생성), [doc/MODS-GUIDE.md](doc/MODS-GUIDE.md) (무엇을 하는 모드인지)
- 작업 이력: [doc/WORKLOG.md](doc/WORKLOG.md) · 가장 최근 인계: [doc/HANDOVER-2026-11-10.md](doc/HANDOVER-2026-11-10.md)
- 모드 추가·검증·되돌리기 절차: [doc/PROCEDURE.md](doc/PROCEDURE.md)
- 설치본 추출·백업: [doc/EXPORT-2026-11-10.md](doc/EXPORT-2026-11-10.md)
- 직접 만든 모드 18개: [doc/README.md](doc/README.md) 의 "직접 만든 모드" 절
- **문서 색인: [doc/README.md](doc/README.md)** — 먼저 여기를 본다

## 구성

| 경로 | 내용 | git |
| --- | --- | --- |
| `doc/` | 기록·문서 (설치 기록 287파일, 사고·수정 기록, 설계·계획) | 추적 |
| `tools/` | Forge API 조회, 다운로드, 설치·테스트, 검사, 목록 생성, 추출·백업 | 추적 |
| `src/` | 직접 만든 모드·플러그인 소스 (빌드 결과 `bin`·`obj`는 제외) | 추적 |
| `downloads/` | 원본 압축 파일 (약 48 GB) | 제외 |
| `extracted/` | 압축 해제본, 설치 원본 (약 40 GB) | 제외 |
| `backup/` | 원본 설정·프로필·SVM 프리셋·locale 스냅샷 백업 (프로필 개인 데이터 포함, 약 14 GB) | 제외 |

압축 추출본은 저장소 밖 `E:\Spt_Mods_Export\` 에 쌓는다(git과 무관).

## 브랜치 전략

`main`은 **보호 브랜치**다. 직접 커밋·푸시·force-push를 하지 않는다.

1. `develop`에서 `feat/*`·`fix/*`·`chore/*`·`docs/*`로 분기한다.
2. 작업 브랜치 → `develop` PR을 올려 머지한다.
3. 릴리스 시점에만 `develop` → `main` PR을 올려 머지한다. `main`에는 이 경로로만 들어간다.

원격(`origin`)은 [gmodprj/EFT-Toybox](https://github.com/gmodprj/EFT-Toybox)이고 **2026-11-10에
처음 올렸다.** 그 전까지는 원격이 없어 위 흐름을 로컬 머지 커밋으로만 돌렸다 — `main`의
`Release ...` 커밋들이 그것이다. 자세한 규칙은 [CLAUDE.md](CLAUDE.md).

## 도구

조회·설치는 `tools/`의 스크립트로 하고, 단계를 끝낼 때마다 **검사**를 돌린다.

| 스크립트 | 용도 |
| --- | --- |
| `tools/search-mods.mjs` | Forge에서 이름·소개로 모드 검색 |
| `tools/sweep-candidates.mjs` | 4.1.6 호환 목록 **전체**를 카테고리까지 받아 정규식으로 추리고 설치된 것을 뺀다 (키워드 검색이 놓치는 것을 센다) |
| `tools/mod-detail.mjs` | 모드별 4.1.6 호환 버전·의존성·결속·설명 |
| `tools/survey-candidates.mjs` | 후보 여러 개 일괄 조회 |
| `tools/resolve-versions.mjs` | 후보의 호환 버전과 의존성 트리 |
| `tools/download-stage.mjs` | `doc/download/manifest-<stage>.json` 기준 다운로드·크기 검증 |
| `tools/download-ranged.mjs` | 한 줄기를 조이는 호스트에서 Range로 끊어 받기(`.state.json`으로 이어받기) |
| `tools/install-and-test.ps1` | 덮어쓰기 금지 설치 + 기록 + 테스트 서버 확인 |
| `tools/gen-installed-mods.mjs` | 설치 기록으로 `doc/INSTALLED-MODS.md` 생성 |
| `tools/install-koreanconfig.ps1` | 한글화 플러그인 빌드 결과와 사전 설치 |

### 검사 (끝낼 때마다 돌린다)

| 스크립트 | 보는 것 | 통과 기준 |
| --- | --- | --- |
| `tools/check-install-records.mjs` | 설치 기록 ↔ 설치본 대조, 같은 경로를 여러 모드가 적었는지 | `OK — 0/0` |
| `tools/check-conflicts.mjs` | 설치 **전** 경로 충돌(덮어쓰기) | 충돌 0 |
| `tools/check-mod-updates.mjs` | 설치된 Forge 모드의 최신 호환 버전 (조회 실패를 "없음"으로 삼키지 않는다) | 종료 코드 0 |
| `tools/check-installed-vs-extracted.mjs` | 설치본과 해제본을 md5로 대조해 "설치 후 달라진 파일" 가리기 | — |
| `tools/check-locale-dict.mjs` | 한글 사전이 서버 locale 열쇠를 덮는지 | `OK — n/n` |
| `tools/check-lang-coverage.mjs` | 한글 사전이 F12 설정 항목을 빠짐없이 덮는지 | 빠진 항목 0 |
| `tools/check-plugin-locales.mjs` | 모드 폴더에 넣은 한국어 locale 4개가 원본과 같은지 | `OK` |
| `tools/check-ui-strings.ps1` | 창 UI 사전의 원문 오타 (한계는 스크립트 머리말) | — |
| `tools/snapshot-locale.mjs` | locale 기준선을 떠 두고 `--diff`로 늘고 줄은 열쇠 가리기 | — |

### 추출·백업 (`tools/export/`)

| 스크립트 | 용도 |
| --- | --- |
| `export-copy.ps1` | `E:\Spt`의 모드·설정을 스테이징 폴더로 (폴더 전체 + 설치 기록 + 루트 모드 파일) |
| `export-docs.ps1` | 스테이징에 `_EXPORT-DOC` 문서와 실측 `README.md` 생성 |
| `export-zip.ps1` | 모드 zip과 프로필 zip (프로필은 JSON 파싱으로 확인한 뒤에만) |
| `export-upload.ps1` | rclone으로 `gdrive:SPT-Mods/<날짜>/`에 올리고 크기 대조 |

네 파일은 맨 위 `$STAMP` 상수를 그날 날짜로 바꿔 쓴다. 경위와 한계는
[doc/EXPORT-2026-11-10.md](doc/EXPORT-2026-11-10.md).

Node 24, PowerShell 7 기준. Forge API는 인증 없이 읽기 전용.

## 라이선스

[MIT](LICENSE). **단, 직접 만든 것에만 적용된다.** 범위 밖은 [NOTICE](NOTICE)에 적었다.

- `src/SptTeamManager/`·`src/SptTeamManager.Tests/`는 [PIT Fireteam](https://github.com/pitAlex/SPT-PitFireTeam)의
  포크다. **업스트림은 라이선스 파일을 두지 않았다**(2026-11-10 확인) — 거기서 유래한
  코드·이미지·소리의 권리는 원저작자에게 있고, 재배포 조건은 확인된 바 없다.
- `doc/`의 모드 이름·버전·설명은 Forge 메타데이터를 옮겨 적은 것이다. 각 모드의 저작권은 그
  제작자에게 있다. **이 저장소는 모드 파일 자체를 담지 않는다.**
- Battlestate Games 및 SPT 프로젝트와 무관하다.
