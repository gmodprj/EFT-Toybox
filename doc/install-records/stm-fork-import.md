# STM 포크 들여오기 기록

## 업스트림
- 저장소: https://github.com/pitAlex/SPT-PitFireTeam (remote `pit-upstream`)
- 기준 커밋: `f32a41b` (전체 `f32a41b2def356a25b3ed40d1115af9116c7e51b`, 1.0.0 머지)
- 방법: `git subtree add --prefix=src/SptTeamManager <전체 SHA>` (히스토리 유지). 짧은 SHA 는 `subtree add` 가 원격 ref 로 해석해 실패하므로 전체 SHA 를 썼다. `git fetch pit-upstream` 으로 객체를 먼저 받았다.
- 재현 명령(원격 추가는 원래 기록에 빠져 있어 2026-11-01 에 더했다):
  ```
  git remote add pit-upstream https://github.com/pitAlex/SPT-PitFireTeam
  git fetch pit-upstream
  git subtree add --prefix=src/SptTeamManager f32a41b2def356a25b3ed40d1115af9116c7e51b
  ```
- 이 단계에서는 아무것도 리네임하지 않았다. 업스트림 소스는 그대로다.
- **라이선스 사실(정정 2026-11-01, 실측).** 업스트림 저장소에는 LICENSE 파일이 **없다**(`git ls-files src/SptTeamManager | grep -i licen` 이 빈 출력). MIT 는 모드 페이지(https://sp-mod.com/mod/2676/pit-fireteam)의 "License" 필드("MIT License")와 업스트림 서버 메타데이터(`server/PitFireTeamServerPlugin.cs:28` 의 `License = "MIT"`)가 선언할 뿐이고, 라이선스 본문을 담은 파일은 없다. STM 이 그 메타데이터를 그대로 싣고 있어 유지할지 바꿀지는 소유자가 정할 열린 결정이다. 앞 줄에 있던 "라이선스(MIT)는 그대로다"는 고지 파일이 있는 듯 읽혀 **틀렸다.** 또 복사된 SAIN 코드의 고지 파일 `addon/SAIN-LICENSE.txt` 는 업스트림 작업 트리에서 이미 삭제돼 있다(`addon/docs/References.md:24`, `addon/docs/Validation.md:566`). 포크가 그 결함을 물려받았다. 자세한 것은 `src/SptTeamManager/README.md`.

## 서버 참조 DLL (`src/SptTeamManager/server/refs/4.1.0/`, 커밋 안 함)
출처는 **설치본이 아니라 NuGet 4.1.0** 이다. 이 기계의 설치본은 전부 4.1.6 이고, 서버 csproj 의 `ValidateSptReferences` 는 컴파일 참조가 4.1.0.0 이어야 한다고 강제한다(끄지 않음).

내려받는 방법(2026-11-01 에 더함; 원래 기록에 없었다). NuGet 표준 flat container 주소 형식은 `https://api.nuget.org/v3-flatcontainer/<id 소문자>/<버전>/<id 소문자>.<버전>.nupkg` 이다(예: `.../sptarkov.server.core/4.1.0/sptarkov.server.core.4.1.0.nupkg`). `.nupkg` 는 zip 이므로 풀어서 `lib/net10.0/<이름>.dll` 을 `server/refs/4.1.0/` 에 복사한다. 단 당시 실제로 쓴 명령줄은 기록으로 남아 있지 않다. 위 형식은 NuGet 표준에서 가져온 것이고 이번에 다시 내려받아 확인하지는 않았다. 받은 DLL 의 어셈블리 버전(4.1.0.0)만 아래 표대로 실측됐다.

| DLL | 출처 | 어셈블리 버전(실측) |
|---|---|---|
| SPTarkov.Server.Core.dll | NuGet `SPTarkov.Server.Core` 4.1.0, `lib/net10.0/` | 4.1.0.0 |
| SPTarkov.Common.dll | NuGet `SPTarkov.Common` 4.1.0, `lib/net10.0/` | 4.1.0.0 |
| SPTarkov.DI.dll | NuGet `SPTarkov.DI` 4.1.0, `lib/net10.0/` | 4.1.0.0 |
| SemanticVersioning.dll | `E:\Spt\SPT_Runtime\SemanticVersioning.dll` | 3.0.0.0 (버전 검사 제외 대상) |

`/server/refs/` 와 `*.dll` 은 업스트림 `src/SptTeamManager/.gitignore:63-64` (63행 `/server/refs/`, 64행 `*.dll`) 가 이미 제외하므로 `.gitignore` 는 고치지 않았다.

## 서버 빌드
```
dotnet build src/SptTeamManager/server/pitFireTeam.Server.csproj -c Debug
```
결과: 성공, 경고 0, 오류 0. 산출물 `server/bin/Debug/net10.0/pitFireTeam.Server.dll`.

## 클라이언트 빌드: 보류
클라이언트가 요구하는 `spt-common.dll`·`spt-reflection.dll` 4.1.0.0 을 이 기계에서 구할 수 없다(SPT 4.1.0 릴리스 전체를 내려받아야 함). 사용자 결정 대기 중이라 시도하지 않았다.
