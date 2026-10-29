# 1단계 조사 결과 — 에어드롭을 띄울 수 있는가

- 날짜: 2026-10-29
- 대상: `E:\Spt` (SPT 4.1.6, EFT 0.16.9.5.40743)
- 설계서: [2026-10-28-airdrop-caller-design.md](2026-10-28-airdrop-caller-design.md)
- 방법: `ilspycmd` 로 `Assembly-CSharp.dll`·SPT 서버 어셈블리·`spt-custom.dll` 디컴파일
  (8.2.0 은 net10 서버 어셈블리를 못 읽어 11.2.0 으로 올렸다)

## 결론 먼저

**된다. 그리고 생각보다 훨씬 깨끗하다.**

공개 메서드 **하나**를 호출하면 끝이다. Harmony 패치도, 리플렉션도, 플레어 흉내도 필요 없다.

```csharp
// EFT.ClientGameWorld
public override void InitAirdrop(
    string lootTemplateId = null,      // ← 전리품 id. 우리가 정한 값을 넣는다
    bool takeNearbyPoint = false,      // ← true 면 position 에서 가장 가까운 지점
    Vector3 position = default)
```

설계서 §7.4 의 **경로 1 이 통째로 필요 없어졌다.** 플레어 이벤트를 흉내 낼 일이 아니라,
플레어 경로와 **같은 함수**를 우리가 직접 부르면 된다.

## 1. 드롭은 이렇게 일어난다

`ServerAirdropManager.ManualUpdate` 가 매 프레임 돌면서 두 갈래로 드롭을 시작한다.

```csharp
// 갈래 A — 플레어 (ServerAirdropManager.ManualUpdate:162)
if (_projectileSuccessPositions.Count > 0) {
    _currentAirdropsCount += 1f;
    UpdateAirdropTimers(PlaneAirdropCooldown);
    Singleton<GameWorld>.Instance.InitAirdrop(
        _overrideLootTemplateId, takeNearbyPoint: true, GetEquidistantPoint());
    ...
}
// 갈래 B — 시간 기반 (같은 메서드:170)
else if (HasLeftPlayersCount(MinPlayersCountToSpawnAirdrop) && ...) {
    if (Random.Range(0f,1f) < PlaneAirdropChance) {
        _currentAirdropsCount += 1f;
        UpdateAirdropTimers(PlaneAirdropCooldown);
        Singleton<GameWorld>.Instance.InitAirdrop();   // 인자 없음 = 무작위 지점·무작위 전리품
    } else UpdateAirdropTimers(UnsuccessfulTryPenalty);
}
```

둘 다 **같은 `InitAirdrop` 로 모인다.** 그리고 그 구현은:

```csharp
// EFT.ClientGameWorld.InitAirdrop
GameObject gameObject = TakeAirdropPoint(takeNearbyPoint, position);
if (gameObject == null) { Debug.LogError("There is no airdrop points here!"); return; }
SynchronizableObject so = ClientSynchronizableObjectLogicProcessor.TakeFromPool(
    SynchronizableObjectType.AirPlane);
if (so.Logic is ClientAirPlane { offlineMode: not false } plane)
    plane.OfflineServerLogic.ContainerTemplateId = lootTemplateId;   // ← 여기
ClientSynchronizableObjectLogicProcessor.InitSyncObject(so, gameObject.transform.position, Vector3.forward);
```

비행기를 풀에서 꺼내 띄우고, 거기에 `ContainerTemplateId` 를 심는다. 연출·낙하·상자 생성은
그 뒤 전부 바닐라가 한다.

## 2. 내용물도 같은 호출로 정해진다

`lootTemplateId` 가 상자가 생길 때 서버에 그대로 올라간다.

```csharp
// ServerAirdropManager.InitContainerLoot
AirdropContainerData value = (await (...ClientBackendSession)
    .LoadLootContainerData(lootTemplateId)).Value;
Item item = Singleton<ItemFactory>.Instance.FlatItemsToTree(value.data).Items[value.data[0]._id];
Enum.TryParse<EAirdropType>(value.Icon, true, out var result);
LootItem.CreateLootContainer(componentInChildren, item, item.ShortName.Localized(), ...);
```

```csharp
// ClientBackendSession.InternalLoadLootContainerData
Url = _backendUrls.Main + "/client/airdrop/loot",
Params = new ClientAirdropLootRequestParams<string>(containerId),
Retries = 3
```

| | 값 |
| --- | --- |
| **라우트** | `POST /client/airdrop/loot` |
| 요청 | `ClientAirdropLootRequestParams<string>(containerId)` — containerId 가 우리가 넘긴 `lootTemplateId` |
| 응답 | `JsonType.AirdropContainerData` = `{ "icon": "<EAirdropType>", "container": FlatItem[] }` |

서버 쪽 처리:

```csharp
// SPTarkov.Server.Core.Routers.Static.LocationStaticRouter
[Injectable(InjectionType.Transient, int.MaxValue, TypePriority = 400000)]
new RouteAction("/client/airdrop/loot", async (url, info, sessionID, output, ct) =>
    await locationCallbacks.GetAirdropLoot(url, info as GetAirdropLootRequest, sessionID))

// SPTarkov.Server.Core.Services.InRaid.AirdropService
public GetAirdropLootResponse GenerateCustomAirdropLoot(GetAirdropLootRequest request) {
    if (airdropConfig.CustomAirdropMapping.TryGetValue(request.ContainerId, out var value))
        return GenerateAirdropLoot(value);
    logger.Warning(... "airdrop-unable_to_find_container_id_generating_random" ...);
    return GenerateAirdropLoot();
}
```

**서버가 이미 `ContainerId` 로 전리품을 가른다.** 설계서 §7.5 가 "식별해서 우리 것만 답한다"
라고 쓴 것이 **바닐라의 설계 그대로**다. 우리는 그 표에 우리 id 를 하나 더 아는 자리를
만들기만 하면 된다.

**식별 문제는 풀렸다** (§7.9 1단계 합격 조건 중 하나). 우리가 `InitAirdrop` 에 넘긴 id 가
그대로 돌아오므로, 그 id 를 우리가 발급하면 우리 상자다. 알아볼 방법을 찾을 필요가 없다 —
**우리가 이름을 붙여 보낸다.**

### 플레어는 탄의 템플릿이 id 를 들고 있다

```csharp
// ClientGameWorld.FlareSuccessEventHandler
if (ammoTemplate.ContainsType(FlareEventType.Airdrop))
    ClientSynchronizableObjectLogicProcessor.ServerAirdropManager?.FlareSuccessEventHandler(
        profileId, flarePosition, ammoTemplate.AirDropTemplateId);
```

플레어 탄의 `AmmoTemplate.AirDropTemplateId` 가 전리품 id 다. 그래서 `airdrop.json` 의
`customAirdropMapping` 에 적힌 두 id(`66da1b49099cf6adcc07a36b`, `66da1b546916142b3b022777`
→ `radar`)는 **플레어 탄 두 종류**의 `AirDropTemplateId` 다.

**우리 id 는 그 표에 없는 값이면 된다.** 없으면 서버가 경고를 찍고 무작위를 돌리는데,
우리 라우트가 그보다 먼저 답하므로 그 경고까지 안 가게 된다.

## 3. 비행기 자리를 안 먹는다 — 완전 공존이 공짜다

설계서 §7.10·§7.14 가 걱정한 "`PlaneAirdropMax: 1` 자리를 나눠 쓴다" 는 **해당되지 않는다.**

`_currentAirdropsCount += 1f` 와 `UpdateAirdropTimers(...)` 는 **`ManualUpdate` 안에서**
일어난다. `InitAirdrop` 자신은 카운터도 타이머도 건드리지 않는다. 즉 우리가
`InitAirdrop` 을 직접 부르면:

- `_currentAirdropsCount` 가 안 올라간다 → 바닐라 예산을 안 쓴다
- `_nextAirdropTimeByFlare` 가 안 밀린다 → 플레어 쿨다운을 안 건드린다

**플레어와 우리 드롭이 한 레이드에서 둘 다 된다.** §7.14 의 "걸림돌 둘" 이 사라졌고,
§7.4 경로 3(직접 스폰, 위험)으로 갈 이유도 없어졌다.

### 대신 소모되는 것: 보급 지점

```csharp
// AirdropManager.TakeAirdropPoint
_airdropPoints.Remove(airdropPoint);
RemoveNearbyAirdropPoints(airdropPoint.transform.position);  // 50m 안의 점들을 비활성
// _airdropPoints 가 비면 ActivateAirdropPoints() 로 되살린다
```

지점은 쓰면 빠지고, 다 쓰면 되살아난다. 무한히 부를 수 있지만 **같은 자리에 연달아는
안 온다**(50m = `AirdropPointDeactivateDistance`).

## 4. 좌표는 임의가 아니다 — 설계 수정이 필요한 지점

**이것이 이 조사에서 설계와 가장 크게 어긋나는 부분이다.**

```csharp
// AirdropManager.Init
_airdropPoints = LocationScene.GetAll<AirdropPoint>().ToList();
```

낙하 지점은 **맵에 미리 박힌 `AirdropPoint` 중 하나**다. `takeNearbyPoint: true` + position
은 그 중 **가장 가까운 것**을 고르는 것이고, 임의 좌표에 떨어뜨리는 길은 없다.

| 설계서가 약속한 것 | 실제로 되는 것 |
| --- | --- |
| "**내 머리 위**로 떨어진다" | "**내게 가장 가까운 보급 지점**에 떨어진다" |
| 수평 오프셋 15m 를 우리가 정한다 (§7.4) | 오프셋 개념이 없다. 지점은 맵이 정한다 |
| 실내·지하면 하늘이 없어 거부 (§7.12) | 지점 자체가 야외에만 있으므로 **거부가 아니라 "먼 데 떨어짐"** 이 된다 |

실내에서 불러도 거부되지 않고 **바깥 어딘가**에 떨어진다. 얼마나 머냐는 맵의 지점 밀도에
달렸고, **아직 재지 않았다.** 지점이 맵당 몇 개이고 간격이 얼마인지 세는 것이 다음 할 일이다.

이 차이는 §7.4·§7.7·§7.12 를 고쳐야 한다. "머리 위" 가 아니라 "가장 가까운 지점" 이고,
exe 는 **떨어진 자리까지의 거리·방향**을 알려 주는 쪽이 쓸모 있다.

## 5. `AirdropCrashPatch` 는 지뢰가 아니었다

설계서 §10-2 가 "SPT 가 패치를 댔으니 지뢰가 있다" 고 걱정한 것은 **스폰 경로가 아니다.**

```csharp
// SPT.Custom.Patches.FixAirdropCrashPatch — 이름이 Fix 로 시작한다
GetTargetMethod(): TarkovApplication 의 (profileId, savageProfile, location, result) 메서드
                   = 레이드 종료 처리
[PatchPrefix]: SynchronizableObjectLogicProcessor._activeSynchronizableObjects 를 정리
```

**레이드가 끝날 때 살아 있는 동기화 객체를 치우는 패치**다. 상자가 공중에 있는 채로 레이드를
나가면 터지던 것을 막는다. 우리 드롭도 같은 객체를 쓰므로 **그 보호를 함께 받는다.**

`FixAirdropFlareDisposePatch` 는 남은 플레어 발사체를 파괴하는 것으로 우리와 무관하다.

**에어드롭을 건드리는 패치가 하나 더 있다 — SPT 가 아니라 모드다.** 게임 로그에
`Enabled patch AirdropBoxOnBoxLandPatch` 가 찍히는데, 찾아보니
`BepInEx\plugins\mpstark-dynamicmaps\DynamicMaps.dll` 이다. 상자가 착지하면 지도에
표시를 찍는 것으로 보인다. **우리 드롭도 같은 상자를 쓰므로 지도에 함께 뜬다** — 막을
일이 아니라 공짜로 얻는 편의다. 설계서 §10-5("에어드롭을 건드리는 모드가 있는지 전수로
보지 않았다")의 답이기도 하다.

**SPT 가 `MinPlayersCountToSpawnAirdrop` 이나 `HasLeftPlayersCount` 를 패치하는 곳은 없다.**

## 6. 뜻밖의 발견 — 이 설치본에 바닐라 보급기는 아마 안 온다

```csharp
public bool HasLeftPlayersCount(int count) {
    // GameWorld.allAlivePlayersByID 에서 IsAI 가 아닌 것만 센다
    return num >= count;
}
// ManualUpdate:170 — 시간 기반 갈래의 첫 조건
else if (HasLeftPlayersCount(MinPlayersCountToSpawnAirdrop) && ...)
```

`MinPlayersCountToSpawnAirdrop` 은 맵 DB 값 그대로 **6** 이다 (실측). 확인한 범위에서:

- 서버가 이 값을 덮어쓰지 않는다 (`AirdropParameter.MinimumPlayersCountToSpawnAirdrop` 은
  그냥 전달 필드다)
- `spt-custom.dll` 에 이 값이나 `HasLeftPlayersCount` 를 건드리는 패치가 없다
- 설치된 서버 모드·플러그인에 이 키를 쓰는 것이 없다. 유일하게 걸린 SVM 은 프리셋에서
  `Loot.EnableAirdrops: false` 로 **꺼져 있다**

싱글플레이에서 사람 플레이어는 1명이므로 `1 >= 6` 이 거짓 → **시간 기반 보급기는 조건을
통과하지 못한다.** 플레어 갈래는 이 검사 **앞**에 있어서 영향을 받지 않는다.

**아직 추론이다.** 재 보지 않았다. 하지만 맞다면:

- 설계서 §8 의 "우리 모드를 안 쓴 레이드에서 평소대로 보급기가 오는가" 는 **"원래도 안 왔다"**
  가 답일 수 있다. 그렇다면 그 확인 항목은 기준이 될 수 없다.
- 공존 걱정은 더 줄어든다. 경쟁할 상대가 애초에 없다.
- **사용자가 이 사실을 알고 싶어할 수 있다.** 지금까지 레이드에서 보급기를 본 적이 있는지가
  이 추론을 바로 검증한다.

## 6-1. 측정 결과 — 지점은 촘촘하다 (2026-10-29 08:13~08:18, Customs)

`AirdropProbe` 를 넣고 Customs 레이드에서 두 자리에서 쟀다
(원본: `E:\Spt\BepInEx\plugins\AirdropProbe\probe.log`).

```
맵: bigmap
관리자: IsInited=True _airdropsEnabled=True CanSummonAirdrop=True
설정: PlaneAirdropMax=1 Chance=0.2 MinPlayers=1 지점비활성거리=50m
      쿨다운=600~700s 시작=300~900s 끝=1200s
지점: 씬 전체 608 / 쓸 수 있음 608 / 비활성 0
```

| 잰 자리 | 가장 가까운 지점 | 중간값 | 가장 먼 지점 | 50m 안 |
| --- | --- | --- | --- | --- |
| 1차 스폰 `(368.4, 1.3, -90.1)` | **13.2m** | 275.1m | 712.5m | 11개 |
| 1차 이동 후 `(193.2, 1.1, -201.0)` | **9.1m** | 328.3m | 561.1m | 13개 |
| 2차 스폰 `(-323.9, 0.9, -225.6)` | **16.6m** | 582.4m | 1038.7m | **7개** |

**밀도는 자리마다 꽤 다르다.** 2차 스폰은 중간값이 582m 로 1차의 두 배였고 50m 안에 7개뿐
이었다(거리 분포 `≤25m 1 / ≤50m 6 / ≤100m 18 / ≤200m 58`). 그래도 **가장 가까운 것은
16.6m** 로, 셋 다 20m 안쪽이다. 맵 가장자리에서도 "바로 옆" 이 유지된다는 뜻이다.

```
가까운 5개 (두 번째 자리):
  1) 직선  9.1m / 수평  9.0m / 높이차 +1.3m / 왼쪽 67°
  2) 직선 11.8m / 수평 11.8m / 높이차 +1.3m / 뒤
  3) 직선 12.7m / 수평 12.7m / 높이차 +0.7m / 오른쪽 134°
  4) 직선 13.3m / 수평 13.2m / 높이차 +1.3m / 앞
  5) 직선 13.3m / 수평 13.3m / 높이차 +1.3m / 왼쪽 128°
```

### 이것이 뜻하는 것

**"머리 위" 라는 약속이 사실상 지켜진다.** 설계서 §7.4 가 "가장 가까운 지점" 으로 물러선
것은 여전히 맞지만, 실제 거리가 **9~13m** 다. 상자가 보이는 거리이고 걸어서 몇 초다.
설계서 §10-9 가 걱정한 "300m 면 다른 물건이다" 는 해당되지 않는다.

**높이차가 +0.7~2.9m** 로 일정하다. 지점이 지면보다 조금 위에 떠 있다 — 땅에 박히지 않게
맵 제작자가 잡아 둔 자리라는 뜻이다.

### 두 번째 드롭은 50m 밖이다

`AirdropPointDeactivateDistance: 50` 이라 한 번 쓰면 **50m 안의 지점이 통째로 비활성**된다.
측정값으로 세어 보면:

| 잰 자리 | 50m 안 지점 | 한 번 쓴 뒤 가장 가까운 지점 |
| --- | --- | --- |
| 첫 번째 | 11개 | **52m** |
| 두 번째 | 13개 | **54m** |

연달아 부르면 **9m → 52m → 그 다음 → …** 으로 멀어진다. 608개를 다 쓰면 전부 되살아난다.
설계서 §7.10 의 쿨다운 30초와 겹쳐 읽으면 "연타해도 같은 자리에 쌓이지 않는다" 가 된다 —
막을 필요 없이 **바닐라가 이미 그렇게 만들어 놨다.**

### 추론이 틀렸다 — `MinPlayers` 는 6 이 아니라 **1** 이다

§6 에서 "바닐라 보급기가 이 설치본에 안 오는 것 같다" 고 적은 것은 **틀렸다.** DB 파일
(`locations/bigmap/base.json`)에는 6 이지만 **런타임 값은 1** 이다. 사람 1명 >= 1 이므로
시간 기반 갈래도 조건을 통과한다. **바닐라 보급기는 온다**(확률 0.2, 300~900초 사이 시작,
1200초에 종료).

다른 설정값(시작 300~900, 끝 1200, 쿨다운 600~700, 비활성 50)은 **DB 와 정확히 일치**하므로
역직렬화가 멀쩡하다. 게임 쪽 기본값도 6 이다. 즉 **누군가 1 로 바꿔서 보낸다.**
클라이언트 플러그인 중에는 없다(전수 검색 0건). SVM 은 프리셋에서 airdrop 이 꺼져 있다.
남은 것은 SPT 서버 자신이고, 싱글플레이 포크로서 자연스러운 일이다.

§6 은 "재 보지 않았다, 추론이다" 라고 적어 두었고 **그 단서가 맞았다.** 재니까 뒤집혔다.

## 6-2. 드롭 시험 — 떨어졌다 (2026-10-29 09:07, Customs)

`Ctrl+Alt+D` 로 두 번 불렀다. **`InitAirdrop` 은 예외 없이 돌았고, 서버가 그 상자의
전리품 요청을 받았고, 사용자가 상자를 눈으로 확인했다.**

> 연출에 시간이 걸린다(비행기가 −50m 에서 출발해 날아온다, `PlaneAirdropDuration: 60`).
> 누르고 바로는 아무것도 안 보이므로 **기다려야 한다.** 처음에 "안 보인다" 로 판단했다가
> 조금 뒤 확인됐다 — 로그만 보고 실패로 단정하지 않는다.

```
===== 드롭 시험 #1  09:07:17 =====
내 위치: (309.9, -1.3, 16.6)
넘길 lootTemplateId: aabbccdd0000000000000001
부르기 전: 쓸 수 있음 608 / 비활성 0 / 가장 가까운 지점 4.8m
CanSummonAirdrop=True
InitAirdrop 호출 — 예외 없음.
부른 뒤: 쓸 수 있음 587 (-21) / 비활성 20 (+20)
다음 드롭의 가장 가까운 지점: 53.2m

===== 드롭 시험 #2  09:07:26 =====
부르기 전: 쓸 수 있음 587 / 비활성 20 / 가장 가까운 지점 53.2m
InitAirdrop 호출 — 예외 없음.
부른 뒤: 쓸 수 있음 572 (-15) / 비활성 34 (+14)
다음 드롭의 가장 가까운 지점: 54.7m
```

### 상자를 열었다 — 바닐라 전리품이 정상으로 들었다

화면 제목이 **`AIRDROP MEDICAL CRATE`**, 내용물은 물·에너지드링크·간식·의약품·현금이다.
`airdrop.json` 의 **`foodMedical`** 타입이다(가중치 100/1101 ≈ 9%).

즉 서버가 우리 가짜 id 를 못 찾아 **무작위로 떨어뜨린 결과**이고, 그 무작위 전리품이
상자에 제대로 담겨 열렸다. **`InitAirdrop` → 비행기 → 상자 → 전리품 요청 → 아이템 트리 →
열기** 가 처음부터 끝까지 돈다.

(무작위 결과이므로 이것만으로는 §6-2 끝의 id 왕복이 가려지지 않는다. 그 시험은 따로다)

### 서버 로그가 상자 생성을 증명한다

```
spt20261029.log  (서버 시각은 UTC. 한국 시간 −9)
[2026-10-29 00:07:17.332] [클라이언트 요청] /client/airdrop/loot
[2026-10-29 00:07:26.291] [클라이언트 요청] /client/airdrop/loot
```

**09:07:17 / 09:07:26 과 초 단위로 일치한다.** 이 요청은 `InitContainerLoot` 이 불려야
나가므로(§2), 비행기만 뜬 것이 아니라 **상자 객체가 실제로 만들어졌다**는 뜻이다.
설계서 §7.5 가 그린 경로가 처음부터 끝까지 돌았다.

덤: 요청이 **호출 직후**에 나간다. 착지할 때가 아니라 **상자가 생길 때** 전리품을 받아 온다.
우리 서버 모드는 드롭을 예약받은 시점에 이미 조립을 끝내 두어야 한다 — 설계서 §7.2 의
`POST /queue` 에서 조립하는 순서가 맞았다.

### 지점 소모는 예측대로다

| | 쓸 수 있음 | 비활성 | 가장 가까운 지점 |
| --- | --- | --- | --- |
| 처음 | 608 | 0 | **4.8m** |
| 1번 뒤 | 587 (−21) | 20 (+20) | 53.2m |
| 2번 뒤 | 572 (−15) | 34 (+14) | 54.7m |

하나는 `TakeAirdropPoint` 가 가져가고 나머지는 50m 안이라 비활성된다(−21 = 쓴 것 1 +
비활성 20). §6-1 에서 계산한 "한 번 쓰면 다음은 50m 밖" 이 **실측으로 맞았다.**
4.8m 는 지금까지 잰 것 중 가장 가깝다.

### 바닐라 보급기는 실제로 온다

어제 로그에도 우리와 무관한 `/client/airdrop/loot` 가 한 건 있다
(`[2026-10-28 12:29:08.369]`). §6-1 에서 고친 "MinPlayers 는 1 이라 바닐라 보급기도 온다"
의 방증이다.

### 아직 못 가린 것 — id 왕복

가짜 id(`aabbccdd0000000000000001`)를 넘겼고 서버가 요청을 받은 것까지는 확인했지만,
**그 id 가 서버까지 갔는지는 로그로 가릴 수 없었다.** 서버가 모르는 id 에 찍는 경고는
`logger.Warning` 인데, **오늘 서버 로그에 Warning 등급이 한 줄도 없다.** 안 찍히는 것인지
안 일어난 것인지 구분이 안 된다.

**코드 없이 가리는 방법이 있다.** `airdrop.json` 의 `customAirdropMapping` 에 실제로 있는
id 를 넘겨 보면 된다.

```json
"customAirdropMapping": { "66da1b49099cf6adcc07a36b": "radar", ... }
"airdropTypeWeightings": { "mixed":500, "weaponArmor":400, "foodMedical":100,
                           "barter":100, "radar":0, "toiletPaper":1 }
"loot.radar": { "useForcedLoot": true,
                "forcedLoot": { "66d9f7256916142b3b02276e": { "min":2, "max":4 } },
                "itemCount": {min:0,max:0}, "weaponPresetCount": {min:0,max:0} }
```

**`radar` 는 가중치가 0 이라 무작위로는 절대 안 나온다.** 그리고 내용물이
**`Radar station spare parts` 2~4개뿐**(다른 품목 0개)이다.

그래서 전리품 id 를 `66da1b49099cf6adcc07a36b` 로 두고 한 번 떨어뜨렸을 때 상자 안에
**레이더 부품만 2~4개** 들어 있으면, 우리가 넘긴 id 가 서버의 전리품 선택을 바꿨다는
뜻이다 — **id 왕복이 증명된다.** 우연으로는 일어날 수 없다.

## 7. 그래서 1단계에 남은 것

조사로는 **합격 조건 두 개 중 하나를 문서상 확인**했다 (식별 가능 — §2). 나머지는 실제로
떨어뜨려 봐야 한다.

## 1단계 합격

| 합격 조건 | 상태 |
| --- | --- |
| `InitAirdrop` 이 예외 없이 돈다 | **확인** (§6-2) |
| 상자 객체가 실제로 만들어진다 | **확인.** 서버가 초 단위로 맞는 전리품 요청을 받았다 (§6-2) |
| 눈으로 상자가 떨어지고 열린다 | **확인.** `AIRDROP MEDICAL CRATE` 를 열었다 (§6-2) |
| 연출 시간이 바닐라만큼 | **확인.** 누르고 바로는 안 보인다. 기다리면 온다 |
| 우리 것으로 식별 가능 | **경로는 확인, 효과는 미확인.** id 를 넘기는 길은 돌았지만(§2) 그 id 가 전리품 선택을 바꾸는지는 못 가렸다 (§6-2 끝) |
| 실내·지하 거부 | **조건 자체가 틀렸다.** 거부가 아니라 먼 지점에 떨어진다 (§4) |
| 비행기 자리를 먹는가 | **확인.** 안 먹는다 (§3) |
| 보급 지점 개수·거리 | **확인.** Customs 608개, 가장 가까운 것 4.8~16.6m (§6-1, §6-2) |

**1단계는 통과했다.** 설계서 §7.9 가 "안 떨어지면 나머지는 의미가 없다" 며 가장 먼저
때리라고 한 것이 떨어졌고, 열렸고, 바닐라 전리품이 제대로 담겼다.

남은 하나는 **id 왕복**(radar id 시험, §6-2 끝)인데, 이것은 2단계의 첫 조각이기도 하다 —
우리 서버 모드가 그 id 에 답하게 만드는 순간 같이 증명된다.

### 2단계로 넘어가며 들고 가는 것

1. ~~보급 지점 밀도를 센다~~ → **쟀다** (§6-1). Customs 608개, 4.8~16.6m.
   다른 맵도 궁금하면 `AirdropProbe` 를 그대로 들고 들어가면 된다.
2. **서버 모드**: `/client/airdrop/loot` 를 우리 id 에 대해 먼저 답하게 한다.
   `LocationStaticRouter` 의 `TypePriority = 400000` 보다 앞서는 라우터를 등록하는 방법
   (DI 가 `orderby TypePriority` 오름차순으로 돈다 — 어느 쪽이 이기는지 확인 필요).
   **여기서 id 왕복이 같이 증명된다** — 우리가 답한 내용물이 상자에 들어오면 끝이다.
3. **조립**: 프리셋 → `Item[]` → `{ icon, container }`. 응답 모양은 §2 에 있다.
4. 플러그인: `InitAirdrop` 호출은 이미 되는 것을 봤다. 폴링만 붙이면 된다.

`AirdropProbe` 는 2단계 동안 **그대로 둔다.** 서버 모드를 붙인 뒤 Ctrl+Alt+D 로
우리 id 를 넘겨 보는 것이 가장 빠른 시험이다. 2단계가 끝나면 지운다.

### 측정기 자체에서 고친 것

처음 로그에 **5초에 14번**이 찍혔다. 키를 꾹 누르면 프레임마다 쟀고, 한 번 재는 데 씬
전체(608개)를 훑고 4KB 를 썼다. **1초 디바운스**를 넣었다. 더불어 거리 603개를 한 줄로
쏟던 것을 **구간별 개수**와 **"한 번 쓰면 다음은 몇 m 밖"** 으로 바꿨다 — 숫자 603개는
파일만 키우고 읽히지 않았다.
