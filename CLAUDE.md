# Spt_Mods 작업 규칙

## 브랜치 전략

- `main`은 보호 브랜치. 직접 커밋·푸시·force-push 금지.
- `develop`이 통합 브랜치. 작업은 `develop`에서 `feat/*`·`fix/*`·`chore/*`·`docs/*`로 분기해 `develop`으로 PR → 머지.
- `main`에는 릴리스 시점에 `develop` → `main` PR로만 반영.
- 머지는 기본 merge commit. CI가 있으면 녹색 확인 후 머지.

## 모드 작업

- 절차는 `doc/PROCEDURE.md`를 따른다. 설치는 `tools/install-and-test.ps1`로 하고, 덮어쓰기가 생기면 중단한다.
- 설치·제거 후 `node tools/gen-installed-mods.mjs`로 `doc/INSTALLED-MODS.md`를 다시 만든다. 그 파일은 직접 고치지 않는다.
- 결속(프로필 결속) 모드는 프로필 백업 후 하나씩 설치한다.
- 게임 확인은 `E:\Spt\BepInEx\LogOutput.log`의 수정 시각이 설치 이후인지 먼저 본다.
- `downloads/`, `extracted/`, `backup/`은 커밋하지 않는다(`.gitignore`).
