# STM 포크 들여오기 기록

## 업스트림
- 저장소: https://github.com/pitAlex/SPT-PitFireTeam (remote `pit-upstream`)
- 기준 커밋: `f32a41b` (전체 `f32a41b2def356a25b3ed40d1115af9116c7e51b`, 1.0.0 머지)
- 방법: `git subtree add --prefix=src/SptTeamManager <전체 SHA>` (히스토리 유지). 짧은 SHA 는 `subtree add` 가 원격 ref 로 해석해 실패하므로 전체 SHA 를 썼다. `git fetch pit-upstream` 으로 객체를 먼저 받았다.
- 이 단계에서는 아무것도 리네임하지 않았다. 업스트림 소스·라이선스(MIT)는 그대로다.

## 서버 참조 DLL (`src/SptTeamManager/server/refs/4.1.0/`, 커밋 안 함)
출처는 **설치본이 아니라 NuGet 4.1.0** 이다. 이 기계의 설치본은 전부 4.1.6 이고, 서버 csproj 의 `ValidateSptReferences` 는 컴파일 참조가 4.1.0.0 이어야 한다고 강제한다(끄지 않음).

| DLL | 출처 | 어셈블리 버전(실측) |
|---|---|---|
| SPTarkov.Server.Core.dll | NuGet `SPTarkov.Server.Core` 4.1.0, `lib/net10.0/` | 4.1.0.0 |
| SPTarkov.Common.dll | NuGet `SPTarkov.Common` 4.1.0, `lib/net10.0/` | 4.1.0.0 |
| SPTarkov.DI.dll | NuGet `SPTarkov.DI` 4.1.0, `lib/net10.0/` | 4.1.0.0 |
| SemanticVersioning.dll | `E:\Spt\SPT_Runtime\SemanticVersioning.dll` | 3.0.0.0 (버전 검사 제외 대상) |

`/server/refs/` 와 `*.dll` 은 업스트림 `.gitignore` 가 이미 제외하므로 `.gitignore` 는 고치지 않았다.

## 서버 빌드
```
dotnet build src/SptTeamManager/server/pitFireTeam.Server.csproj -c Debug
```
결과: 성공, 경고 0, 오류 0. 산출물 `server/bin/Debug/net10.0/pitFireTeam.Server.dll`.

## 클라이언트 빌드: 보류
클라이언트가 요구하는 `spt-common.dll`·`spt-reflection.dll` 4.1.0.0 을 이 기계에서 구할 수 없다(SPT 4.1.0 릴리스 전체를 내려받아야 함). 사용자 결정 대기 중이라 시도하지 않았다.
