# 설계 — 지도에서 모드 세력 구분 (DynamicMapsFactionMarkers)

- 날짜: 2026-10-30
- 상태: 설계 승인 대기. 아직 코드 없음.
- 실측 근거: [2026-10-30-faction-identification-survey.md](2026-10-30-faction-identification-survey.md)
- 대상: `E:\Spt` (SPT 4.1.6), Dynamic Maps 1.2.1, MoreBotsAPI 2.1.1, ISB Special Forces 2.0.1

## 1. 왜 만드는가

스캐브 무리 안에 ISB가 섞여 있을 때 **지도만 보고는 판단이 서지 않는다**. 지금 Dynamic Maps 는 ISB 16역할 전부를 일반 스캐브와 똑같은 주황 화살표로 그린다 — 보스 판정이 enum 숫자 21개가 박힌 HashSet 이고 거기에 ISB(13701~13716)가 없기 때문이다. 같은 봇이 킬 패널·시체 표시에서는 "ISB BOSS"·"WHITE TUSK" 로 제 이름이 나오므로, **지도만 뒤처져 있다.**

## 2. 목표

- ISB 16역할을 **역할 묶음 8가지**로 색·모양을 나눠 지도와 미니맵에 그린다.
- 켜고 끄기는 **ISB 전체 하나**(사용자 결정).
- 색·모양은 설정 파일에서 바꿀 수 있다.
- 바닐라 4범주(Friendly Player / Enemy Player / Boss / Scav)의 지금 동작을 그대로 둔다.
- 나중에 다른 세력 모드가 들어와도 같은 틀로 늘어난다(설정에 줄만 더하면 된다).

## 3. 비목표

- 세력 간 적대 관계·AI 행동은 건드리지 않는다.
- HUD·킬 패널·나침반·Amands' Sense 는 건드리지 않는다(이미 제 이름으로 나온다).
- 서버 모드·DB(`templateTable`·`locations`)·서버 라우트는 손대지 않는다. 이 작업은 **게임 쪽 플러그인 하나**로 끝난다.
- `ScavRole/ISB*` 의 한국어 번역을 넣는 일은 이 작업 밖이다(별건으로 다룬다. 지금 ko 와 en 이 같은 영문이다).

## 4. 안티골 — 이렇게 되면 실패다

- **Dynamic Maps 의 파일을 고치거나 DLL 을 바꿔치기하는 것.** 그 모드가 업데이트되면 날아가고, 남에게 줄 수도 없다. 우리 쪽은 Harmony 패치만 쓴다.
- **마커가 두 개 겹쳐 뜨는 것**(우리 것 + 그들의 스캐브 것).
- **바닐라 범주의 토글이 뒤섞이는 것** — "스캐브 숨기기" 를 눌렀는데 ISB 가 사라지거나, 그 반대.
- **지도가 깨지는 것.** 안 고쳐지는 것보다 나쁘다. 조건이 안 맞으면 조용히 비활성이어야 한다.

## 5. 제약 — 실측으로 확인한 것

Dynamic Maps 1.2.1 역컴파일 기준.

| 사실 | 뜻 |
| --- | --- |
| `OtherPlayersMarkerProvider.TryAddMarker(IPlayer)` 가 private, 분류 순서는 `같은 그룹 → IsTrackedBoss → IsPMC → IsScav` | 여기 앞에 끼어들어야 스캐브로 찍히기 전에 가로챌 수 있다 |
| `ShouldShowCategory(string)` 가 private 이고 **모르는 범주면 false** | 우리 범주를 여기서 true 로 만들어 주지 않으면 마커가 아예 안 붙는다 |
| `RemoveDisabledMarkers()` 가 `ShouldShowCategory` 로 기존 마커를 솎아낸다 | 토글이 바뀔 때 우리 마커가 조용히 지워질 수 있다 — 같은 이유로 위 패치가 필수다 |
| `MapView.AddPlayerMarker(IPlayer, category, color, imagePath)` 가 **public** | 마커 추가는 정상 API 로 된다 |
| 큰 지도와 미니맵이 같은 `MapView`(`IsMiniMapActive` 로만 구분) | 한 번 추가하면 둘 다 나온다. 따로 할 일 없다 |
| 아이콘은 `Path.Combine(Plugin.Path, path)` 로 읽고 `DynamicMaps.Plugin.Path` 는 public static | 경로가 **DynamicMaps 폴더 기준**이다. `../DynamicMapsFactionMarkers/diamond.png` 처럼 우리 폴더 파일을 쓸 수 있다 — 남의 폴더에 파일을 넣지 않아도 된다 |
| 쓸 수 있는 기본 아이콘: `arrow`·`star`·`dot`·`skull`·`exit`·`quest` … (**마름모는 없다**) | ◆ 는 우리가 png 를 만들어 우리 폴더에 둔다 |
| 생명주기 이벤트(`GameWorldUnregisterPlayerPatch.OnUnregisterPlayer`, `PlayerOnDeadPatch.OnDead`)가 **internal** | 우리가 따로 구독하기 어렵다 → 그들의 생명주기에 **얹는다**(아래 6-2) |
| `ModdedMapScreen.ReadConfig()` 가 바닐라 네 설정(Friendly·Enemy·Boss·Scav)의 **OR** 로만 `OtherPlayersMarkerProvider` 를 등록한다 | 네 개가 다 꺼지면 `TryAddMarker` 가 아예 불리지 않아 우리 패치가 死코드가 된다 → 패치 ③ 이 필요하다 |
| 네 분기 모두 intel level 게이트(`Settings.Show*IntelLevel <= GetIntelLevel()`)를 통과해야 그려진다 | 우리도 어느 게이트에 묶일지 정해야 한다 |
| `RemoveDisabledMarkers()`·`TryAddMarkers()` 는 `HandleSetBoolOption` 에서만, 즉 **그들의 설정값이 바뀔 때만** 돈다 | 우리 토글을 뒤집어도 아무 코드가 안 돌아 마커가 그대로 남는다 → 우리가 `RefreshMarkers()` 를 불러 줘야 즉시 반영된다 |
| `ModdedMapScreen.Instance` 가 public static, `RefreshMarkers()` 가 public, `_dynamicMarkerProviders` 는 private | 즉시 반영 경로가 있다(사전만 리플렉션으로 꺼내면 된다) |
| 이 설치본의 현재 설정: Friendly·Enemy·Scav·Boss 마커 **네 개 모두 켜짐**, intel 요구치 **전부 0** | 지금 상태에서는 패치 ③·intel 게이트가 눈에 보이는 차이를 만들지 않는다. 둘 다 "나중에 설정을 바꿨을 때 깨지지 않게" 하는 장치다 |
| 역할 식별: MoreBotsAPI `CustomWildSpawnTypeManager.IsCustomWildSpawnType(int)` / `GetCustomWildSpawnTypeDict()[id].ScavRole` (public static, 프리패처 어셈블리) | ISB 전용 코드를 쓰지 않고 **커스텀 역할 일반**으로 다룰 수 있다 |
| BSG `WildSpawnTypeExtension.GetScavRoleKey(role)` 가 `"ScavRole/ISBWhiteTusk"` 를 준다 | MoreBotsAPI 가 없을 때의 대체 경로 |
| 표시 이름은 서버 locale 의 `ScavRole/*` (ISB 10개) | 범주 이름을 손으로 적지 않아도 된다 |

## 6. 어떻게 — 정한 것

### 6-1. 형태

새 플러그인 하나. 저장소 소스는 `src/DynamicMapsFactionMarkers`, 설치 위치는 `BepInEx\plugins\DynamicMapsFactionMarkers\`(DLL 1 + `config.jsonc` + 아이콘 png 1개). 설치·제거는 `tools/install-dynamicmapsfactionmarkers.ps1`(`-Remove`) — 기존 자작 플러그인들과 같은 틀. 설치 후 `node tools/gen-installed-mods.mjs` 로 `doc/INSTALLED-MODS.md` 재생성(저장소 규칙).

의존:
- Dynamic Maps — 없으면 조용히 비활성.
- MoreBotsAPI — 없으면 설정의 수동 역할 목록만 쓴다(기능은 돈다).

### 6-2. Harmony 패치 두 개

둘 다 Dynamic Maps 1.2.1 의 `OtherPlayersMarkerProvider` private 메서드를 겨눈다.

**① `TryAddMarker(IPlayer)` prefix**
대상이 우리 표에 걸리는 역할이면:
1. `__instance` 에서 `_lastMapView` 와 `_playerMarkers` 를 `Traverse` 로 꺼낸다.
2. 이미 `_playerMarkers` 에 있으면 아무 것도 안 하고 원본을 건너뛴다.
3. `mapView.AddPlayerMarker(player, 우리범주, 색, 아이콘경로)` 로 마커를 만들고 `_playerMarkers[player]` 에 넣는다.
4. 원본을 건너뛴다(`return false`).

**그들의 사전에 넣는 것이 핵심이다.** 제거·맵 전환·레벨 전환·사망·`RefreshMarkers`·`RemoveNonActivePlayers` 가 전부 그들 코드로 그대로 돌아간다. 우리는 생명주기를 따로 들고 있지 않는다(그래서 internal 이벤트가 필요 없다).

우리 표에 안 걸리는 대상(바닐라 봇, PMC, 같은 그룹 동료, PIT Fireteam 동료)은 손대지 않고 원본으로 흘려보낸다.

**② `ShouldShowCategory(string)` prefix**
범주가 우리 것이면 **마스터 토글 값**을 돌려주고 원본을 건너뛴다. 아니면 원본대로.

**③ `ModdedMapScreen.ReadConfig()` postfix**
그들은 바닐라 네 설정의 OR 로만 `OtherPlayersMarkerProvider` 를 등록한다. 네 개가 다 꺼져 있으면 ①·② 가 불릴 자리가 없어진다. 담당이 **없을 때만** `AddRemoveMarkerProvider<OtherPlayersMarkerProvider>(true)` 를 불러 등록을 보장한다. 이미 있으면 그들이 네 bool 을 자기 설정대로 넣어 둔 것이므로 손대지 않는다.

주의(리뷰에서 잡힌 것): 새로 만들어진 담당은 `_showFriendlyPlayers` 가 **true 로 태어난다**(그들의 필드 초기값). 그리고 `ReadConfig` 는 OR 가 false 일 때 네 bool 을 **아예 넣지 않는다** — 서버의 허용 스위치(`ServerConfig.AllowShow*`)도 거기서만 적용된다. 그대로 두면 바닐라 아군 마커가 되살아나고 서버 스위치를 건너뛴다(§4 안티골 위반, PIT Fireteam 동료가 있으면 실제로 보인다). 그래서 **우리가 만든 경우에만** 네 bool 을 직접 false 로 내린다.

**토글 즉시 반영**
우리 F12 토글이 바뀌면 `ModdedMapScreen.Instance`(public static) → `_dynamicMarkerProviders`(private, 리플렉션)에서 `OtherPlayersMarkerProvider` 를 꺼내 public `RefreshMarkers()` 를 부른다. 그들의 `RemoveDisabledMarkers`·`TryAddMarkers` 는 자기 설정이 바뀔 때만 돌기 때문에, 이 호출이 없으면 토글을 꺼도 마커가 다음 맵 열기까지 남는다.

### 6-3. 범주·색·모양

범주 이름은 서버 locale 의 표시 이름을 그대로 쓴다(`"ISB BOSS"`, `"WHITE TUSK"` …). locale 을 못 읽으면 `ScavRole` 키를 쓴다. 바닐라 4범주와 글자가 겹칠 일은 없다.

서버가 주는 ISB 이름은 10개인데 그중 `ISBWhiteTusk`·`ISBCommander`·`ISBBossFollower` 가 모두 "WHITE TUSK" 라, **눈에 보이는 묶음은 8가지**다. 기본값:

| 묶음(표시 이름) | 역할 수 | 모양 | 색 |
| --- | --- | --- | --- |
| ISB BOSS (`ISBBoss`) | 1 (ISBTeamLeader) | ★ star | `#FF2D55` |
| WHITE TUSK (`ISBWhiteTusk`+`ISBCommander`+`ISBBossFollower`) | 9 | ★ star | `#FFFFFF` |
| ISB SNIPER (`ISBSniper`) | 1 | ◆ diamond | `#2EC8FF` |
| ISB ASSASSIN (`ISBAssassin`) | 1 | ◆ diamond | `#B6FF00` |
| ISB SCOUT (`ISBScout`) | 1 | ◆ diamond | `#00E5B0` |
| ISB HEAVY (`ISBHeavy`) | 1 | ▲ arrow | `#8A6BFF` |
| ISB (`ISB`) | 1 (ISBSpecialForces) | ▲ arrow | `#C400FF` |
| ISB GUARD (`ISBGuard`) | 1 (ISBSecondLeader) | ▲ arrow | `#FF9ED8` |

모양 규칙은 ★ 대장급 / ◆ 특수(저격·암살·정찰) / ▲ 일반. 색은 바닐라가 쓰는 주황(스캐브·보스)·빨강(BEAR)·노랑(USEC)·연파랑(아군)을 피했다. ISB BOSS 만 빨강 계열인데 모양이 ★ 로 달라 BEAR 화살표와 섞이지 않는다.

아이콘 경로: ★ `Markers/star.png`, ▲ `Markers/arrow.png`(둘 다 Dynamic Maps 것을 그대로 쓴다), ◆ `../DynamicMapsFactionMarkers/diamond.png`(우리 것).

### 6-4. 설정

`config.jsonc`(우리 폴더):
- `enabled` — 전체 끄기.
- `groups` — `[{ "scavRole": "ISBWhiteTusk", "label": null, "colorHex": "#FFFFFF", "shape": "star" }]` 꼴. `label` 이 null 이면 locale 이름을 쓴다. `shape` 는 `star`·`arrow`·`diamond`·`dot`.
- `intelLevelSource` — intel level 게이트를 무엇에 묶을지. 기본값 `"boss"`(ISB 16역할 중 14개가 BSG 테이블상 보스이므로 보스 쪽 요구치를 따른다). `"scav"`·`"none"` 도 받는다. 이 설치본은 요구치가 전부 0 이라 기본값에서 차이가 없다.
- `extraRoles` — 숫자 역할 id 를 직접 적는 칸. MoreBotsAPI 를 거치지 않고 SPT 커스텀 enum 으로 들어온 역할(예: Black Division `848400`대 — 지금 설치본에는 봇 타입이 없어 뜨지 않지만 틀은 열어 둔다)을 위한 것.
- 처음 보는 커스텀 역할은 기본 팔레트에서 색을 자동 배정해 **그 레이드에서 바로 그리고**, 레이드 시작 때 `discovered-roles.json` 으로 적어 내놓는다(SlotCategoryRelax 의 전례와 같은 방식). 그걸 보고 색을 바꾸고 싶으면 `config.jsonc` 에 옮겨 적는다. 바로 그리는 쪽을 고른 이유는 §2 의 "다른 세력 모드가 들어와도 늘어난다" 를 설정 편집 없이 지키기 위해서다. 대신 그 봇들은 스캐브 범주에서 빠져나오므로 `Show Scav Markers` 를 꺼도 사라지지 않고, 우리 토글로 꺼야 한다 — 의도된 동작이다.
- `autoAssignUnknown` — 위 자동 배정을 끄면 `config.jsonc`·`extraRoles` 에 적힌 역할만 그리고, 나머지는 원본(스캐브) 흐름으로 보낸다.

F12(BepInEx 설정 창): **토글 하나** — "모드 세력 표시"(기본 켜짐). 색·모양은 F12 에 올리지 않는다(항목이 24개로 불어난다).

### 6-5. 안전장치 — 닫힌 쪽으로 넘어뜨린다

- Dynamic Maps 가 없거나, **핵심** 대상(`TryAddMarker`·`ShouldShowCategory`·`_playerMarkers`·`_lastMapView`)을 하나라도 못 찾으면 **패치를 걸지 않고** 로그 한 줄만 남기고 비활성. 버전이 올라가 내부가 바뀌면 지도는 바닐라 동작으로 남는다.
- **곁가지** 대상(`ReadConfig`·`ModdedMapScreen.Instance`·`_dynamicMarkerProviders`·`RefreshMarkers`)은 없어도 전체를 끄지 않는다. 그것만 포기하고 로그를 남긴다: `ReadConfig` 를 못 찾으면 "바닐라 마커 네 개 중 하나는 켜 두어야 ISB 가 보인다", 즉시 반영 경로를 못 찾으면 "토글은 다음 맵 열기부터 적용된다" 로 적는다.
- 우리 분류 코드에서 예외가 나면 그 봇은 원본 흐름으로 보낸다(스캐브로 그려진다). 예외를 삼키고 지도를 멈추지 않는다.
- 설정 파일이 깨졌으면 기본값으로 돌아간다.

## 7. 확인 방법

1. 설치 후 게임 재시작(서버는 끌 필요 없다 — 게임 쪽 플러그인뿐이다).
2. ISB 가 나오는 맵에서 레이드 진입. 지도(M)와 미니맵 둘 다에서 ISB 가 위 표의 색·모양으로 나오는지. 스크린샷으로 남긴다.
3. 바닐라 보스(킬라·타길라)가 **그대로 주황 ★** 인지 — 우리가 그쪽을 건드리지 않았다는 확인.
4. F12 토글을 껐을 때 **지도를 다시 열지 않고 그 자리에서** ISB 마커만 사라지고 스캐브 마커는 남는지, 다시 켜면 돌아오는지(즉시 반영 경로 확인).
5. 바닐라 마커 네 개를 **전부 끈** 상태에서도 ISB 만 뜨는지(패치 ③ 확인).
6. `LogOutput.log` 에 우리 플러그인 예외 0건.
7. ISB 를 한 마리 죽여 시체 처리 뒤 마커가 사라지는지(그들의 `OnDead` 흐름에 얹혔는지 확인).

## 7-1. 제 판단으로 정한 것 (사용자가 고르지 않았다)

리뷰어가 "문서에 없다" 고 짚은 네 가지다. 넷 다 바꾸고 싶으면 말만 하면 된다.

| 정한 것 | 왜 |
| --- | --- |
| 바닐라 마커가 전부 꺼져 있어도 ISB 는 뜬다(패치 ③) | 토글이 "ISB 전체 하나" 라고 했으니, 남의 토글 상태에 매달리면 말이 어긋난다. 등록만 강제하는 것이라 바닐라 마커가 늘지도 않는다 |
| intel level 은 **보스 쪽 요구치**에 묶는다(설정으로 변경 가능) | ISB 16역할 중 14개가 BSG 테이블상 보스다. 이 설치본은 요구치가 전부 0 이라 지금은 차이가 없다 |
| 토글은 **즉시** 반영한다 | 토글을 눌렀는데 다음 맵 열기까지 안 바뀌면 고장으로 보인다. 경로를 못 찾으면 그것만 포기한다 |
| 처음 보는 커스텀 역할은 **바로 그린다**(끌 수 있다) | 다른 세력 모드가 들어왔을 때 설정을 손대지 않아도 갈라져 보이게 하려는 것 |

## 8. 빌더에게 남긴 것

- `AccessTools`/`Traverse` 로 private 멤버를 잡는 구체적 코드와 상수화 방식.
- `diamond.png`(32×32, 흰 단색 — 색은 런타임에 입힌다) 그리기.
- 자동 배정 팔레트의 색 목록과 순서.
- `config.jsonc` 직렬화 방식(저장소의 다른 자작 플러그인과 같은 틀을 따른다).
- 로그 문구.

## 9. git 메모

옆 세션(AirdropCaller)이 같은 작업 트리에서 git 을 쓰고 있다. 사용자 지시로 **그쪽이 먼저**다. 이 문서와 실측 문서는 그쪽이 끝났다고 알릴 때까지 미커밋으로 둔다. 이 세션은 `commit`·`checkout`·`merge` 를 하지 않는다.
