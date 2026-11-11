# 설치된 모드의 성능 관련 사용자 리뷰 (2026-11-11)

> 우리 쪽 실측은 [MEMORY-AUDIT-2026-11-11.md](MEMORY-AUDIT-2026-11-11.md), 넣을 후보는 [CANDIDATES-PERF.md](CANDIDATES-PERF.md).
> 이 문서는 **남들이 뭐라고 적었나**다. 도구는 [tools/fetch-mod-comments.mjs](../tools/fetch-mod-comments.mjs),
> 추린 자료는 [stage2-survey/comments-perf-2026-11-11.json](stage2-survey/comments-perf-2026-11-11.json).

## 어떻게 읽어 왔나 — API 에 리뷰 길이 없다

Forge API v0 에는 댓글·리뷰가 없다. `/mod/<id>/comments`·`/reviews`·`/ratings` 전부 **404** 이고, `?include=comments` 는 `INVALID_QUERY_PARAMETER` 로 유효한 값 넷(`versions`·`license`·`category`·`source_code_links`)만 알려 준다. 댓글 탭은 **Livewire 지연 컴포넌트**라 모드 페이지 HTML 에도 본문이 없다.

그래서 브라우저가 하는 일을 그대로 했다 — 페이지에서 csrf 토큰·세션 쿠키·댓글 탭의 `wire:snapshot`·`__lazyLoad` 인자를 꺼내고, **페이지가 알려 주는 갱신 주소**(`"uri":"…/livewire-<해시>/update"`, 배포마다 접두어가 바뀐다)로 `__lazyLoad` 를 POST 한다. 돌아온 HTML 의 `<div class="user-markdown …">` 가 본문이고, 작성자는 그 앞의 마지막 `/user/<id>/<이름>` 링크다.

| 센 것 | 값 |
| --- | --- |
| 훑은 모드 | **312개**(`INSTALLED-MODS.md` 의 Forge 링크 전체 — 설치본 305개 + 제거분 등 문서에 적힌 링크) |
| 읽어 온 댓글 | **5,077건** / 사이트 표기 합 **12,321건** |
| 성능·메모리 낱말이 걸린 댓글 | **185건**, **91개 모드** |
| 못 읽은 모드 | **7개**(아래) |

**한계를 먼저 적는다.** 모드마다 **첫 1~2쪽(최신 10~15건)**만 읽었다. 12,321건 가운데 5,077건이니 **41%**다. 오래된 댓글에 묻힌 보고는 이 문서에 없다. 낱말 필터(`fps`·`stutter`·`memory`·`leak`·`crash` 등)로 걸렀으니 **다른 말로 적힌 성능 보고도 빠진다**.

### 못 읽은 7개 — 왜인지 적는다

| 모드 | 사유 |
| --- | --- |
| SAIN(791) · Lotus(1232) · Olympus(505) · Waypoints(827) · HandsAreNotBusy(1298) | **댓글 탭이 아예 없다.** 791 로 확인했다 — 탭이 `description`·`versions`·`addons`·`dependents` 넷이고 `comments-tab` 이 없다(2173 에는 있다). 제작자가 댓글을 끈 것이고, 우리 파서 문제가 아니다 |
| Tactical Gear Component(1125) | 페이지 **403**, API 도 **404** |
| Painter(1025) | 페이지 **404**, API 도 **404** |

**덤으로 알게 된 것: 1025·1125 는 Forge 에서 사라졌다**(API `NOT_FOUND`). 둘 다 설치본에 있다(`Painter` 27파일, `Tactical Gear Component` 137파일). `node tools/check-mod-updates.mjs` 가 "forge 에서 사라짐" 으로 잡을 항목이다.

## 1. 우리 로그로 교차검증된 것 — 여기가 핵심

### 1-A. Icebreaker 백포트가 **다른 맵에서도** 일한다 (2866, 설치 1.1.3)

`ffloridamann` 이 **1.1.3 기준**으로, 즉 **우리가 쓰는 바로 그 버전**에 적은 보고다(요지만 옮긴다).

- `RenderEnvProbe` 는 첫 `CameraManager.SetCamera` 에 만들어져 **세션 내내 살아 있다** — 하이드아웃까지.
- 아이스브레이커가 아닌 맵에서 `RenderEnvProbe.Update` 가 **매 프레임** 돌고, 스폰 60초쯤 뒤 `FindObjectsOfType<CullingGridContent>()`·`FindObjectsOfType<LODGroup>()` 로 전수조사를 하며 `[PerfCensus]` 경고를 남긴다. **Factory 에서 LOD 24,518개를 찾아 53 ms 프레임**이 됐다.
- 같은 `Update` 가 매 프레임 `IcebreakerAcoustics.IcebreakerLoaded()` 를 불러 **로드된 모든 씬의 이름을 읽는다** → **Factory 에서 프레임당 약 1.5 KB 쓰레기**.
- 제안: census 는 디버그 옵션일 때만, 리셋은 맵을 떠날 때 한 번, 씬 검사는 `sceneLoaded`/`sceneUnloaded` 에서.

**우리 설치본에서 그대로 찍혔다.** 2026-11-11 기동(아이스브레이커에 들어가지 않은 기동)의 `LogOutput.log` 에 `[PerfCensus]` 가 **세 번**이다.

```
[PerfCensus] map=Lighthouse  ... lodGroups=58529 oneLod=24281
[PerfCensus] map=laboratory  ... lodGroups=17124 oneLod=10488
[PerfCensus] map=hideout     ... lodGroups=1873  oneLod=748
```

라이트하우스에서 센 **58,529개**는 리뷰어가 Factory 에서 53 ms 를 본 24,518개의 **2.4배**다. 프레임 시간은 로그에 없지만, 같은 기동의 라이트하우스 프레임 통계에 **최악 3,309 ms** 가 한 번 있었다([MEMORY-AUDIT](MEMORY-AUDIT-2026-11-11.md) §2).

그리고 "매 프레임 1.5 KB 쓰레기" 는 같은 문서에서 재 둔 **관리 힙 바닥값 +670 MiB / 47분**과 기전이 맞는다. **상관이지 증명은 아니다** — 다만 메모리 조사에서 "모드 하나를 지목할 증거가 없다" 고 적었던 자리에 **처음으로 이름이 붙은 후보**가 생겼다.

### 1-B. Interchange Expansion 이 Streets 에서 프레임당 수천 번 (3025, 설치 1.0.8)

`bay-harbour-enjoyer` 가 **1.0.8** — 우리 버전 — 에 Mod Profiler 로 재서 적었다: Streets 에서 `RoomGeometryPatch.Prefix`(`AudioRoomStorage.CheckPositionInsideRoom` 패치)가 **프레임당 평균 약 1,400회, 번잡한 곳 약 4,800회** 불린다. 본인도 "프로파일러 자체 overhead 가 per-call 비용을 부풀릴 수 있어 실제 비용은 단정 못 한다" 고 적었다. 다른 사용자(`alexgardien5`)가 "나도 이 모드 켜면 FPS 떨어진다" 고 동의했다.

### 1-C. 재현되지 않은 것 — Hideout Cat (2908, 설치 1.1.1)

리뷰 둘이 하이드아웃에서 `Cat.FixedUpdate` → `IsPlayerInTheWay` 가 **FixedUpdate 틱마다 NullReferenceException** 을 뱉고, 모드를 빼니 로그 스팸이 멎고 "체감이 덜 끈적였다" 고 적었다.

**우리 로그에는 그 예외가 없다.** `hideoutcat` 로 걸리는 줄은 47줄인데 전부 Info 이고(상태 변경 `New state: Lying` 19 · `Sleeping` 18, 나머지는 애니메이션·다음 자리 로그), 예외는 0이다. 같은 1.1.1 인데 우리 쪽에서는 나지 않는다 — **빼야 할 이유가 아직 없다.**

### 1-D. 우리에게는 해당 없는 것 — ISB Aishi 의 5초 프리즈 (2478, 설치 2.0.7)

`xxxxtok` 이 2.0.6 에서 **5초마다 약 150 ms 프리즈**의 원인을 특정했다. 처음엔 `/ISB/spawn/progress`·`/Aishi/miniLabs/state` 주기 요청을 의심해 DLL 을 고쳐 5초 → 60초로 늘렸지만 **달라지지 않았고**, 범인은 아이템 `a2c16b836b441dc953652bd6` 였다 — 그게 인벤토리에 있으면 `AishiKeys.AishiAnomalyS2.UnlockNearDoor()` 가 `Resources.FindObjectsOfTypeAll()` 로 5초마다 유니티 객체를 전수조사한다. 아이템을 빼니 프리즈가 사라졌다.

**우리 프로필 11개에 그 아이템은 없다**(`grep` 0건). 지금은 해당 없고, **그 열쇠를 줍는 날 다시 볼 거리**다. 덧붙여 이 보고는 우리가 [MEMORY-AUDIT](MEMORY-AUDIT-2026-11-11.md) §3 에서 "모드 폴링은 범인이 아니다" 로 판정한 것과 **같은 결론**이다 — 간격을 12배 늘려도 안 변했다.

### 1-E. 같은 컴포넌트에서 나오는 것 — WTT Menu Overhaul (1775, 설치 1.3.0)

리뷰 셋이 성능을 말한다. `neksys`: "기본 테마에서 **스태시 → 메뉴 전환 때 5~8초 프리즈**, 계속(SPT 4.1.2)". `estek`: "Factory 테마 메인 메뉴에서 **GPU 72℃** — 평소 50℃, 레이드가 61℃". `bernardvi`: "나만 성능 문제가 있나?" 제작자(`moxopixel`)는 테마·버전·증상을 되묻는 중이다.

우리 로그에서 **경고가 가장 많은 것이 이 모드다 — 162줄**(`[Layout] EnvironmentUISceneFactory GameObject not found in Environment UI`). 같은 컴포넌트에서 나오지만 **같은 건이라고 단정하지 않는다**(우리는 프리즈를 재지 않았다).

### 1-F. 퀘스트가 많을수록 심하다는 보고 — UI Fixes (1342, 설치 6.0.4)

`thisfkngletters` 가 **6.0.4** 에서 Mod Profiler 로 짚었다: 상인 화면·과제 화면에서 **1~7초 프리즈**, `QuestListItemPatch`(과제 목록 로드, 1~4초) · `HandoverPatch`(상인에게 아이템 넘길 때, 6~7초) · `ShowIconsPatch`. 본인이 "**모드 퀘스트까지 해서 활성 과제가 많아서** 더 심한 것 같다" 고 덧붙였다. `nikita-tyan`·`nomad-35` 도 같은 증상을 적었고(`nomad-35` 는 6.0.2 로 내려도 그대로), 제작자 `tyfon` 은 **재현되지 않는다**며 프로파일러 자료를 요청하는 상태다.

우리는 Quest Backport(293파일) · Lotus · Scorpion · Ref Friendly Quests 등으로 **활성 과제가 많은 쪽**이다. 그래서 이 보고는 우리 환경과 조건이 겹친다.

## 2. 제작자가 비용을 인정한 것

| 모드 | 적힌 말 |
| --- | --- |
| Breakout Outlines(3007, 설치 1.2.0) | 제작자 본인: "lazy caching 때문에 **성능을 대가로 치른다**. 미안하다, 나중에 우회책을 찾겠다". 우리 로그에도 `Searched item did not match a tracked container` 40줄이 있다 |
| CloudSix(2567) | 제작자(`matsix`): "F12 에 성능 설정이 많다. **primary steps 를 낮추라** — 많이 낮춰도 구름은 괜찮다". 사용자: 저사양에서 **15~17 fps** 먹는다 |
| TraceSix(3221) | 제작자: "일반 섀도는 **특히 Streets 에서 CPU 를 아주 많이 먹는다. RT 섀도는 그 CPU 비용을 거의 없앤다**" — 그래서 RT 를 켜고 프레임이 **오른** 사례도 있다(`raverman`) |

## 3. 숫자가 붙은 비용 보고 (설치된 그래픽·UI 모드)

| 모드 | 사용자가 적은 값 |
| --- | --- |
| ReflectSix(3099) | 저사양에서 **5~6 fps** |
| POMSix(3100) | RTX 3060 12GB 에서 **10~15%** — "차이가 작아 결국 빼었다" |
| CloudSix(2567) | **15~17 fps**(저사양) |
| TraceSix(3221) | 오디오 끊김 보고(5070 Ti+9950X)와 "**아이스브레이커를 블랙리스트에 넣어야 랙이 없다**"는 보고. 반대로 Streets 에서 프레임이 올랐다는 보고도 있다 |
| HollywoodFX(2003) | "Streets 에서 **커스텀 LOD 설정을 쓰면 두 번째 레이드 입장 때 크래시**"(3회 재현), "새 버전은 어떻게 고쳐도 프레임이 나쁘다", "매치 시작 때 폭발이 터지며 프레임이 1 미만으로" |
| DERP(2200) | PiP 조준경에서 **30~40 → 60 이상 유지**(구형 GPU, 70% 설정). 다만 **RMB 마다 깜빡임·검은 플리커** 보고가 여럿이고, "이득은 대부분 CPU 쪽이라 레이드 중 체감은 미미" 라는 지적도 있다 |
| DynamicMaps Extended(3097) | "Woods 에서 미니맵을 띄우면 **5 fps**. 엿보기 단축키로 볼 때는 정상"(2.0.0 기준, 우리는 2.5.2) |
| Moe-TradeMarker(2736) | "인벤토리 열 때 **120 → 40 fps**, 0.5초 — 들어갈 때 모든 아이템을 상인 표식용으로 검사하는 것 같다" |
| Direct Reload(3074) | 장전을 거의 즉시로 맞추면 Streets 에서 **65 → 57 fps** 가 되풀이된다 |
| Lots of Loot Redux(1619) | "표식 방 전리품을 2배만 하려면? **랙이 안 걸리게**" |
| UNTAR Go Home!(2342) | 제작자: 2.0.0 설치 후 **첫 기동에 1~2분 멈춘다**(알려진 문제). 사용자: "성능이 형편없었다" |
| Amands' Sense(2521) | "원인 공지: **반경을 15m 대신 150m 로 두고 Always on 을 켜면** 몇 초마다 스터터가 난다". 또 "45~60분 레이드의 **마지막 15분**에 큰 랙 스파이크"(5080·64GB) |
| Lighthouse Rework(3022, 설치 1.0.7) | "**엄청난 FPS 하락** 겪는 사람 없나?"(1.0.7), 그리고 "4.1.6 에서 맵 로딩 6% 에서 크래시" |

## 4. 이미 고쳐진 것 — 우리는 고친 버전을 쓰고 있다

| 건 | 사정 |
| --- | --- |
| Rippers Casino(3212) | 1.0.0 에서 "**레이드와 하이드아웃에서 매초 스터터**"(150 fps 나오는 사람도) 보고 둘. 제작자가 모드를 잠시 내리고 원인을 찾아 **1.0.1 성능 핫픽스**를 냈고 보고자가 "모든 맵에서 해결" 확인. **우리는 1.0.1** |
| BiC(3190) | 1.0.0 + UI Fixes 조합에서 보안 컨테이너 단축키로 게임 **무한 행업**·크래시. **1.0.1 에서 해결**(보고자가 긴 레이드로 확인). **우리는 1.0.1** |
| Holster Everything(2650) | 홀스터 슬롯 아이템 더블클릭 시 행업. 원인이 **Compatibility Highlighter 1.2.0** 의 깨진 패치 둘 + UI Fixes 하이라이트 조합으로 밝혀졌고 **CH 1.2.1 에서 수정**. **우리는 CH 1.3.2** |
| All Quests Checkmarks(2025) | "UI 요소가 레이드 중 깨진다", "모드를 빼니 크래시가 멎었다". **우리는 10-E 에서 넣었다가 같은 날 뺐다**([INSTALLED-MODS](INSTALLED-MODS.md)) — 남들 보고와 같은 결론이었다 |

## 5. 그래서 무엇을 할까

1. **Icebreaker 클라이언트부터 A/B 한다.** [MEMORY-AUDIT §6](MEMORY-AUDIT-2026-11-11.md) 의 절차에 이 변수를 맨 앞에 둔다 — 첫 후보였던 TraceSix 보다 **이쪽이 근거가 세다**(우리 로그에 `[PerfCensus]` 가 있고, 보고가 우리 버전 그대로다). 되돌리기는 `BepInEx\plugins` 의 그 DLL 하나를 치우는 것이고, **맵 자체를 지우는 것이 아니다**(아이스브레이커에 들어가지 않는 동안만).
2. **Mod Profiler(2952) 를 넣는 쪽이 더 옳아졌다.** 위 보고 다섯 가운데 셋(UI Fixes·Interchange·Icebreaker)이 **그 도구로 재서** 나왔다. 우리도 같은 자로 재야 남의 보고를 우리 설치본에서 확인할 수 있다.
3. **설정으로 지금 줄일 수 있는 것**: Amands' Sense 반경(150m 오설정 주의) · CloudSix `primary steps` · POMSix·ReflectSix 토글 · DERP 품질 슬라이더. 어느 것도 아직 건드리지 않았다.
4. **Forge 에서 사라진 두 모드**(Painter 1025 · Tactical Gear Component 1125)는 업데이트도 리뷰도 더는 못 본다. 다음 업데이트 점검에서 "사라짐" 으로 잡히면 이 문서를 가리킨다.
5. 전체 댓글(12,321건 중 41%만 읽었다)을 더 보려면 `--pages` 를 올린다. 한 모드에 1~2초가 드니 전수는 20분쯤이다.
