# 2단계 결과 — 내가 정한 것만 든 상자가 떨어진다

- 날짜: 2026-10-29
- 설계: [2026-10-28-airdrop-caller-design.md](2026-10-28-airdrop-caller-design.md)
- 1단계: [2026-10-29-airdrop-stage1-findings.md](2026-10-29-airdrop-stage1-findings.md)
- 소스: [../../src/AirdropCaller.Server](../../src/AirdropCaller.Server)

## 결론

**된다.** 우리가 정한 상자 id 로 부르면 우리가 정한 내용물이 담긴다. 바닐라 보급은 그대로다.

```
서버 로그 (UTC. 한국 시간 −9)
[01:03:48][AirdropCallerStartup] 상자 aabbccdd0000000000000001 → Medical, 품목 8종 24개
[01:30:35][AirdropCallerRouter]  aabbccdd0000000000000001 → Medical 상자에 24개를 담았다.
[01:36:59][AirdropCallerRouter]  aabbccdd0000000000000001 → Medical 상자에 24개를 담았다.
```

```
probe.log (현지 시각)
===== 드롭 시험 #1 10:30:35 =====   가장 가까운 지점 13.6m → 부른 뒤 45.8m
===== 드롭 시험 #2 10:36:59 =====   가장 가까운 지점  9.1m → 부른 뒤 59.7m
```

**시각이 초 단위로 맞는다.** 플러그인이 `InitAirdrop(우리 id, ...)` 를 부른 그 순간에
서버의 **우리 라우터**가 답했다.

**"못 담은 것" 이 하나도 없다.** 24개가 Medical 상자 격자에 전부 들어갔다.

### 이것으로 같이 증명된 것 — id 왕복

1단계에서 못 가렸던 것이 여기서 공짜로 풀렸다. 우리가 `InitAirdrop` 에 넘긴 id 가
`POST /client/airdrop/loot` 의 `containerId` 로 돌아왔고, **우리 라우터가 그 id 를 알아보고
답했다.** 로그 두 줄이 그 사슬 전체의 증거다.

## 어떻게 끼어들었나

**패치도 리플렉션도 쓰지 않았다.** `HttpRouter.HandleRouteAsync` 의 성질을 그대로 썼다.

```csharp
foreach (Router router in routers)
    if (router.CanHandle(url, dynamic)) {
        string output = wrapper.Output ?? string.Empty;  // 앞선 라우터의 답을 넘겨받는다
        ...
        wrapper.Output = obj2 as string;                 // 덮어쓴다
    }
// break 가 없다 — 마지막으로 답한 라우터가 이긴다
```

같은 URL 을 잡는 라우터에서 **멈추지 않는다.** 전부 차례로 돌리고, 마지막 결과가 응답이
된다. 그래서 바닐라보다 **뒤에** 서면 그 답을 받아 보고 바꿀 수 있다.

순서는 `Injectable.TypePriority` 오름차순이다. 바닐라 `LocationStaticRouter` 가 **400,000**,
`Injectable` 의 기본값이 **`int.MaxValue`** 라 — **인수 없이 `[Injectable]` 만 붙이면
우리가 뒤에 선다.** (`src/StackUnify` 가 같은 순서 규칙을 반대 방향으로 쓴다)

우리 id 가 아니면 받은 `output` 을 **그대로** 돌려준다. 그래서 바닐라 보급기도 플레어
보급도 건드리지 않는다.

## 조립

바닐라 `AirdropService.GenerateAirdropLoot` 를 그대로 따른다.

1. 상자 아이템 하나(`Icon` → 상자 tpl, 바닐라 `GetAirdropCrateItem` 의 짝)
2. 담을 것마다 `ItemHelper.GetContainerMapping` 격자에 자리를 찾는다
3. 들어가는 것만 `ParentId` = 상자 id, `SlotId` = `"main"` 으로 붙인다
4. **좌표는 넣지 않는다** — 바닐라도 자리 유무만 보고 보내며 배치는 클라이언트가 한다

못 담은 것은 버리지 않고 로그에 남긴다(`(못 담은 것 N개: …)`).

## 설정을 시작 때 읽는 이유

처음에는 라우터가 직접 `config.jsonc` 를 읽었다. 그러면 **첫 전리품 요청 때** 처음 읽혀,
설정이 깨져 있어도 레이드 한가운데에서야 안다. `ConfigStore` 로 한 번만 읽고
`AirdropCallerStartup`(`IOnLoad`)이 서버 시작 때 요약을 찍게 했다.

```
[AirdropCaller] 상자 aabbccdd0000000000000001 → Medical, 품목 8종 24개
```

이 줄이 보이면 설정이 파싱됐고 DI 도 풀린 것이다. **설정을 고치면 서버를 다시 켜야 한다** —
이 저장소의 다른 서버 모드와 같은 규칙이다.

## 영향 확인

| 항목 | 결과 |
| --- | --- |
| 서버 모드 수 | 114 → **115** |
| 서버 시작 | 정상 147초, 웹서버·웹소켓 정상 |
| 서버 새 오류 | **0건.** `Could not add` 48건은 그대로(`MountOlympus.dll`, 기존) |
| 게임 플러그인 | 143 → **144** (AirdropProbe) |
| 게임 오류 | DirectReload 1(기존 정상), SAIN 3 — **우리와 무관** |
| 프로필 | 테스트 전 백업과 견줘 SPT 자체 마이그레이션 시각 한 줄 말고 동일 |

`BundleLoader` 의 `barrel_vpo215_600mm_366tkm.bundle` 오류는 **10-27 부터 있던 것**이다
(10-27 8건, 10-28 26건). 우리와 무관하다.

프로필 백업은 `backup/profiles-before-airdropcaller/` 에 있다.

## 설계와 어긋난 곳

없다. 설계서 §7.5 가 그린 경로(서버가 조립해 바닐라의 전리품 요청에 답한다)가 **그대로**
돌았다. §10-10 에 "커스텀 라우터가 바닐라보다 먼저 답하게 하는 방법을 확인하지 않았다"
라고 적어 둔 것이 이 단계에서 풀렸다 — **먼저가 아니라 나중이 답이었다.**

## 다음

| 남은 것 | |
| --- | --- |
| 3단계 — 외부 창 | 지금은 `config.jsonc` 를 손으로 고친다. WPF 창이 이 자리를 채운다 |
| 대기열 라우트 | `POST /queue`·`GET /pending` 은 아직 없다. 지금은 측정기가 id 를 직접 넘긴다 |
| 스택 크기 | `StackSize` 를 아이템 최대치 넘게 적으면 어떻게 되는지 안 봤다 |
| 격자 넘침 | 24개는 다 들어갔다. **넘치는 경우를 아직 안 봤다** — 거절 문구가 실제로 뜨는지 확인해야 한다 |
| `AirdropProbe` | 3단계까지 그대로 둔다. 끝나면 지운다 |
