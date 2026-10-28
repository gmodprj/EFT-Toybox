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

## 7. 그래서 1단계에 남은 것

조사로는 **합격 조건 두 개 중 하나를 문서상 확인**했다 (식별 가능 — §2). 나머지는 실제로
떨어뜨려 봐야 한다.

| 1단계 합격 조건 | 상태 |
| --- | --- |
| 야외에서 상자가 떨어지고 열린다 | **미확인.** 코드를 써서 봐야 한다 |
| 연출 시간이 바닐라만큼 | 미확인 |
| 우리 것으로 식별 가능 | **확인.** 우리가 id 를 발급해 넘긴다 (§2) |
| 실내·지하 거부 | **조건 자체가 틀렸다.** 거부가 아니라 먼 지점에 떨어진다 (§4) |
| 비행기 자리를 먹는가 | **확인.** 안 먹는다 (§3) |

다음 할 일:

1. **보급 지점 밀도를 센다** — 맵별 `AirdropPoint` 개수와 간격. "가장 가까운 지점" 이
   실제로 얼마나 가까운지가 이 모드의 값어치를 가른다.
2. 서버 모드: `/client/airdrop/loot` 를 우리 id 에 대해 먼저 답하게 한다.
   `LocationStaticRouter` 의 `TypePriority = 400000` 보다 앞서는 라우터를 등록하는 방법
   (DI 가 `orderby TypePriority` 오름차순으로 돈다 — 어느 쪽이 이기는지 확인 필요).
3. 플러그인: 폴링 + `ClientGameWorld.InitAirdrop(ourId, true, playerPos)` 호출.
4. 띄워 보고 로그를 읽는다.
