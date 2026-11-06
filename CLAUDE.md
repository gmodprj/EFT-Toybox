# Spt_Mods 작업 규칙

## 브랜치 전략

- `main`은 보호 브랜치. 직접 커밋·푸시·force-push 금지.
- `develop`이 통합 브랜치. 작업은 `develop`에서 `feat/*`·`fix/*`·`chore/*`·`docs/*`로 분기해 `develop`으로 PR → 머지.
- `main`에는 릴리스 시점에 `develop` → `main` PR로만 반영.
- 머지는 기본 merge commit. CI가 있으면 녹색 확인 후 머지.

## 모드 작업

- 절차는 `doc/PROCEDURE.md`를 따른다. 설치는 `tools/install-and-test.ps1`로 하고, 덮어쓰기가 생기면 중단한다.
- 설치·제거 후 `node tools/gen-installed-mods.mjs`로 `doc/INSTALLED-MODS.md`를 다시 만든다. 그 파일은 직접 고치지 않는다.
- 단계를 끝낼 때마다 `node tools/check-install-records.mjs`를 돌려 **`OK — 0/0`**인지 본다. 모드가 들어오면 **그 모드와 상관없는 다른 모드의 기록에도** 구멍이 난다.
  - 없는 파일이 나와도 **바로 유실로 단정하지 않는다.** 기록에는 설치 후 새로 생긴 파일(`NEWFILE`)도 같은 형식으로 들어가는데 누가 만들었는지는 알 수 없어, **서버나 다른 모드가 만든 파일이 그때 설치 중이던 모드 몫으로 적힌다.** 해제본(`extracted/<단계>/<모드>/`)에 그 경로가 있는지, `install-records/removed/`에 그 경로를 가진 제거 기록이 있는지, 같은 경로를 여러 모드가 적었는지를 먼저 본다.
  - 기록 줄을 지울 때는 "없는 것 전부"가 아니라 **아는 종류만** 지우고 지운 내역을 파일로 남긴다(보기: `doc/install-records/README-removed-stale-lines-2026-11-06.md`).
- 결속(프로필 결속) 모드는 프로필 백업 후 하나씩 설치한다.
- 게임 확인은 `E:\Spt\BepInEx\LogOutput.log`의 수정 시각이 설치 이후인지 먼저 본다.
- `downloads/`, `extracted/`, `backup/`은 커밋하지 않는다(`.gitignore`).
