# 아이스브레이커 백포트가 그 맵이 아닌 곳에서 하는 일 둘을 멈춘다 (준비분)

2026-11-11. 사용자가 **측정(A/B)을 건너뛰고 수정부터** 하기로 했다. 이 문서는 **무엇을 왜 어떻게
가리는지**와, 아직 하지 않은 것을 적는다. 아직 **설치하지 않았다** — 빌드와 시험까지다.

- 발단: [REVIEWS-PERF-2026-11-11.md](../REVIEWS-PERF-2026-11-11.md) §1-A — 모드 페이지의 1.1.3 보고.
- 우리 쪽 증거: [MEMORY-AUDIT-2026-11-11.md](../MEMORY-AUDIT-2026-11-11.md) — 한 기동에 2,367 → 6,696 MB, 관리 힙 바닥값 +670 MiB/47분.
- 대상 모드: [MANIMAL'S ICEBREAKER BACKPORT](https://sp-mod.com/mod/2866) **1.1.3**(설치본 그대로), GUID `com.manimal.icebreaker`.

## 1. 무엇이 문제인가 — 설치본 DLL 로 확인한 것

`BepInEx\plugins\ManimalIcebreaker\ManimalIcebreakerClient.dll`(11,142,656 B, 2026-09-14)의
메타데이터를 직접 읽어 **이름과 서명을 확인했다**(타입 439개).

| 대상 | 확인된 것 |
| --- | --- |
| `Manimal.Icebreaker.RenderEnvProbe.Update()` | **private instance, IL 6,664 B.** 매 프레임 도는 메서드 하나에 `Tick*` 열 몇 개가 달려 있다 |
| `Manimal.Icebreaker.RenderEnvProbe.TickVanillaPerfCensus()` | **private instance, IL 680 B** |
| `Manimal.Icebreaker.IcebreakerAcoustics.IcebreakerLoaded()` | **static public Boolean, IL 64 B** |
| `Manimal.Icebreaker.IcebreakerAcoustics.OnSceneLoaded(Scene)` | static public — 모드도 씬 이벤트를 쓴다 |

**설정으로는 끌 수 없다.** `BepInEx\config\com.manimal.icebreaker.cfg` 의 `DevMode = false`,
`DiagHotkeys = false` 로 둘 다 꺼져 있는데도 2026-11-11 기동에서 census 가 **세 번** 돌았다.

```
[Warning:Manimal-Icebreaker] [PerfCensus] map=Lighthouse  ... lodGroups=58529 oneLod=24281
[Warning:Manimal-Icebreaker] [PerfCensus] map=laboratory  ... lodGroups=17124 oneLod=10488
[Warning:Manimal-Icebreaker] [PerfCensus] map=hideout     ... lodGroups=1873  oneLod=748
```

아이스브레이커에 **들어가지도 않은** 기동이다. 모드 페이지의 보고자는 Factory 에서 LOD 24,518개를
세며 **53 ms 프레임**을 봤는데, 우리 라이트하우스는 **58,529개**다.

보고된 나머지 하나는 매 프레임 쪽이다 — `RenderEnvProbe.Update` 의 "아이스브레이커가 아닐 때"
가지가 `IcebreakerAcoustics.IcebreakerLoaded()` 를 **프레임마다** 부르고, 그 메서드는 **로드된 씬
전부의 이름을 읽는다**(보고자 측정: Factory 에서 프레임당 약 1.5 KB 쓰레기).

## 2. 어떻게 가리는가 — 밖에서 두 메서드만

새 플러그인 [`src/IcebreakerOffMapGuard`](../../src/IcebreakerOffMapGuard) (net472, 10,752 B).
**남의 모드 파일을 고치지 않는다** — Harmony 로 두 군데만 가린다.

| 패치 | 하는 일 | 되돌리기 |
| --- | --- | --- |
| `TickVanillaPerfCensus` **Prefix** | 건너뛴다. 재는 것뿐이고 게임을 바꾸지 않는 메서드다 | F12 `Skip vanilla perf census` 끄기 |
| `IcebreakerLoaded` **Prefix+Postfix** | 답을 **기억**한다. 캐시가 비면 **원본을 그대로 돌리고** 그 결과만 적는다. 씬 로드·언로드·활성 씬 변경에서 버린다 | F12 `Cache 'is Icebreaker loaded'` 끄기 |

**판정을 흉내 내지 않는 것이 이 설계의 핵심이다.** "씬 이름을 어떻게 보는지" 는 모드 쪽 사정이고,
우리가 다시 구현하면 모드가 바뀔 때 조용히 틀린다. 그래서 캐시는 **원본의 답을 적어 두는 것**
뿐이고, 쓰는 가정은 하나다 — **씬이 로드·언로드되지 않는 동안 답은 바뀌지 않는다.**

안전 쪽 성질 셋:

- **대상 모드가 없으면 아무것도 걸지 않는다.** 이름으로 찾아(`AccessTools.Method`) null 이면 그만두고
  로그 한 줄만 남긴다. 아이스브레이커 어셈블리를 **참조하지 않는다**(11 MB 를 끌어오지 않고, 그 모드
  없는 설치본에서도 안 터진다). 의존은 `SoftDependency` 로만 적어 로드 순서를 뒤에 둔다.
- **이름이 바뀌면 조용히 지나가지 않는다.** 한쪽만 못 찾으면 `못 찾았다: …` 를 **경고**로 남긴다.
  모드가 업데이트되면 이 줄로 알게 된다.
- **매 프레임 문자열을 만들지 않는다.** 횟수만 센다. 60초마다 한 줄 내는 보고는 기본 **꺼져 있다**.

`Update`(IL 6,664 B)에는 **손대지 않았다.** 매 프레임 도는 큰 메서드를 트랜스파일하는 쪽이 더 많은
것을 고칠 수 있지만, 깨질 자리도 그만큼 많다.

## 3. 지금까지 한 것 / 아직 안 한 것

| 한 것 | 결과 |
| --- | --- |
| 설치본 DLL 메타데이터로 타입·메서드·정적 여부 확인 | 위 표. **추측이 아니다** |
| 설정으로 끌 수 있는지 확인 | 불가(`DevMode`·`DiagHotkeys` 가 꺼져 있는데도 돌았다) |
| 플러그인 작성·빌드 | `dotnet build -c Release` → **경고 0 / 오류 0**, 10,752 B |
| 캐시 규칙 시험 | `src/IcebreakerOffMapGuard.Tests` → **5개 전부 통과**(총 5개) |
| 설치 스크립트 | `tools/install-icebreakeroffmapguard.ps1`(`-Remove` 지원). 게임이 떠 있으면 **멈춘다** — 잠긴 DLL 은 "나중에 반영" 이 아니라 안 깔린 것이다 |

| 아직 안 한 것 | 메모 |
| --- | --- |
| **설치** | 스크립트를 돌리지 않았다. 설치본은 11-A(VRAM Cleaner) 이후 그대로다 |
| 게임에서의 확인 | 기동 로그에 `걸었다: Manimal.Icebreaker.RenderEnvProbe:TickVanillaPerfCensus` 와 `…IcebreakerAcoustics:IcebreakerLoaded` 두 줄, 그리고 **`[PerfCensus]` 가 사라지는지**를 본다 |
| 효과 측정 | 사용자 결정으로 **A/B 를 건너뛰었다.** 그래도 사후 비교는 공짜다 — `totalMemoryBeforeCleanUp` 의 처음·끝과 라이트하우스 `managedMiB` 바닥값을 11-11 값(2,367 → 6,696 MB / 바닥 4,868 → 5,539 MiB)과 견주면 된다 |
| 제작자에게 보고 | 모드 페이지에 이미 같은 내용의 보고가 있다(1.1.3 기준). 우리 `lodGroups=58529` 를 덧붙일 수 있다 |

## 4. 설치할 때

```powershell
pwsh -File tools/install-icebreakeroffmapguard.ps1
```

이 플러그인은 **Forge 모드가 아니라 이 저장소에서 만든 것**이므로 설치 기록
(`doc/install-records/`)에 들어가지 않는다 — ProfilePorter·SptKoreanLocale 과 같은 취급이다
([INSTALLED-MODS.md](../INSTALLED-MODS.md) 의 "절차 밖" 표에 적는다). 되돌리기는
`-Remove` 하나이고, 지워도 아이스브레이커 모드 자체는 손대지 않은 그대로다.

**게임 플러그인 수는 1 늘어 247 → 248 이 된다**(11-A 의 VRAM Cleaner 가 247로 올린 뒤).
