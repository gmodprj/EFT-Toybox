# DynamicMapsFactionMarkers — 모드 세력 지도 표시

Dynamic Maps 의 지도·미니맵에서 **모드로 추가된 세력**(ISB Special Forces 등)을 역할 묶음마다 다른 색·모양으로 가른다.

## 왜 필요한가

Dynamic Maps 는 봇을 `같은 그룹 → 보스 → PMC → 스캐브` 네 갈래로만 나눈다. 그런데 보스 판정이 **enum 숫자 21개가 소스에 박힌 목록**이라, 모드가 더한 역할(ISB 는 13701~13716)은 거기 들어갈 자리가 없다. 그래서 ISB 는 게임 테이블상 보스여도 지도에서는 일반 스캐브와 똑같은 주황 화살표로 나온다 — 스캐브 무리에 섞이면 분간이 안 된다. 킬 패널·시체 표시는 이미 "ISB BOSS"·"WHITE TUSK" 로 제 이름을 보여 주는데 지도만 뒤처져 있었다.

실측 근거는 [doc/specs/2026-10-30-faction-identification-survey.md](../../doc/specs/2026-10-30-faction-identification-survey.md), 설계는 [doc/specs/2026-10-30-dynamicmaps-faction-markers-design.md](../../doc/specs/2026-10-30-dynamicmaps-faction-markers-design.md).

## 무엇을 하는가

Dynamic Maps 파일은 한 글자도 고치지 않는다. Harmony 로 세 지점에 끼어든다.

| 지점 | 하는 일 |
| --- | --- |
| `OtherPlayersMarkerProvider.TryAddMarker` | 우리 역할이면 우리 색·모양으로 마커를 만들어 **그들의 마커 사전에 넣는다.** 제거·맵 전환·레벨 전환·사망 처리가 그들 코드로 그대로 돈다 |
| `OtherPlayersMarkerProvider.ShouldShowCategory` | 우리 범주면 F12 토글 값을 돌려준다. 없으면 그들의 정리 코드가 "모르는 범주" 로 보고 우리 마커를 솎아낸다 |
| `ModdedMapScreen.ReadConfig` | 바닐라 마커 네 개가 다 꺼져 있어도 마커 담당 등록을 보장한다. 담당이 **없을 때만** 만들고, 그렇게 만든 담당은 네 bool 을 직접 false 로 내린다 — 새로 만들어진 담당은 "아군 표시" 가 켜진 채 태어나기 때문이다 |

F12 토글을 뒤집으면 그 자리에서 다시 그린다(그들의 정리 코드는 자기 설정이 바뀔 때만 돌기 때문에 직접 불러 준다).

## 기본 색·모양

모양은 세 갈래 — ★ 대장급, ◆ 특수, ▲ 일반.

| 묶음(`scavRole`) | 모양 | 색 |
| --- | --- | --- |
| `ISBBoss` | ★ | `#FF2D55` |
| `ISBWhiteTusk`·`ISBCommander`·`ISBBossFollower` | ★ | `#FFFFFF` |
| `ISBSniper` | ◆ | `#2EC8FF` |
| `ISBAssassin` | ◆ | `#B6FF00` |
| `ISBScout` | ◆ | `#00E5B0` |
| `ISBHeavy` | ▲ | `#8A6BFF` |
| `ISB` | ▲ | `#C400FF` |
| `ISBGuard` | ▲ | `#FF9ED8` |

범주 이름은 서버 locale 의 `ScavRole/*` 값을 그대로 쓴다("ISB BOSS", "WHITE TUSK" …). locale 을 못 읽으면 `scavRole` 키가 그대로 이름이 된다.

## config.jsonc

플러그인 폴더(`BepInEx\plugins\DynamicMapsFactionMarkers\`)에 있다. 설치 스크립트는 **이미 있는 설정을 덮어쓰지 않는다.**

| 항목 | 뜻 |
| --- | --- |
| `enabled` | false 면 역할 수집도 패치도 하지 않는다 |
| `intelLevelSource` | intel 요구치를 무엇에 묶을지 — `boss`(기본) · `scav` · `none` |
| `autoAssignUnknown` | 설정에 없는 커스텀 역할도 팔레트에서 색을 받아 바로 그린다 |
| `groups` | `scavRole` → `label`(null 이면 locale) · `colorHex` · `shape`(`star`·`arrow`·`diamond`·`dot`). 같은 `scavRole` 이 두 줄이면 먼저 적힌 줄이 이긴다 |
| `extraRoles` | MoreBotsAPI 를 거치지 않고 들어온 역할 id 를 손으로 적는 칸 |

`label` 을 바닐라 범주 이름(`Scav`·`Boss`·`Enemy Player`·`Friendly Player`)으로 적으면 그 역할은 **올라가지 않는다** — 그대로 두면 바닐라 마커를 가로채기 때문이다.

처음 보는 커스텀 역할은 자동 배정된 색으로 바로 그려지고 `discovered-roles.json` 에 적힌다. 색을 고치려면 그 줄을 `groups` 로 옮겨 적는다.

## 안 되는 경우

- **Dynamic Maps 가 없으면** BepInEx 가 이 플러그인을 건너뛴다(하드 의존).
- **Dynamic Maps 내부가 바뀌어** 핵심 패치 지점(`TryAddMarker`·`ShouldShowCategory`·`_lastMapView`·`_playerMarkers`)을 못 찾으면 **아무 것도 걸지 않고** 로그 한 줄만 남기고 비활성 — 지도는 바닐라 그대로 돈다. 지도가 깨지는 쪽보다 안 고쳐지는 쪽이 낫다.
- 곁가지(등록 보장·토글 즉시 반영·intel 요구치 읽기)는 못 찾아도 그것만 포기하고 로그를 남긴다.
- **MoreBotsAPI 가 없으면** `extraRoles` 에 적은 역할만 쓴다.

## 빌드·설치

```
dotnet build src/DynamicMapsFactionMarkers -c Release
pwsh -File tools/install-dynamicmapsfactionmarkers.ps1
```

제거는 `-Remove`. 게임만 다시 켜면 되고 서버는 끌 필요 없다.

판정 로직(설정 읽기·역할 분류·색 파싱)은 게임 어셈블리를 모르는 순수 파일로 떼어 두어 EFT 없이 시험한다: `dotnet test src/DynamicMapsFactionMarkers.Tests`.
