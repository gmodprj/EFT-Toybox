# 아이스브레이커 백포트가 그 맵이 아닌 곳에서 하는 일 둘을 멈춘다

2026-11-11. 사용자가 **측정(A/B)을 건너뛰고 수정부터** 하기로 했다. 이 문서는 **무엇을 왜 어떻게
가리는지**와, 아직 하지 않은 것을 적는다. **설치는 같은 날 17:53 에 했다**(§5).

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
| 게임에서의 확인 | 기동 로그에 `걸었다: Manimal.Icebreaker.RenderEnvProbe:TickVanillaPerfCensus` 와 `…IcebreakerAcoustics:IcebreakerLoaded` 두 줄, 그리고 **`[PerfCensus]` 가 사라지는지**를 본다 |
| 효과 측정 | 사용자 결정으로 **A/B 를 건너뛰었다.** 그래도 사후 비교는 공짜다 — `totalMemoryBeforeCleanUp` 의 처음·끝과 라이트하우스 `managedMiB` 바닥값을 11-11 값(2,367 → 6,696 MB / 바닥 4,868 → 5,539 MiB)과 견주면 된다 |
| 제작자에게 보고 | 모드 페이지에 이미 같은 내용의 보고가 있다(1.1.3 기준). 우리 `lodGroups=58529` 를 덧붙일 수 있다 |

## 4. 설치 방법

```powershell
pwsh -File tools/install-icebreakeroffmapguard.ps1
```

이 플러그인은 **Forge 모드가 아니라 이 저장소에서 만든 것**이므로 설치 기록
(`doc/install-records/`)에 들어가지 않는다 — ProfilePorter·SptKoreanLocale 과 같은 취급이다
([INSTALLED-MODS.md](../INSTALLED-MODS.md) 의 "절차 밖" 표에 적는다). 되돌리기는
`-Remove` 하나이고, 지워도 아이스브레이커 모드 자체는 손대지 않은 그대로다.

**게임 플러그인 수는 1 늘어 247 → 248 이 된다**(11-A 의 VRAM Cleaner 가 247로 올린 뒤).

## 5. 설치 실측 (2026-11-11 17:53)

| 항목 | 값 |
| --- | --- |
| 넣은 것 | `BepInEx\plugins\IcebreakerOffMapGuard\IcebreakerOffMapGuard.dll` **10,752 B** |
| 설치본 md5 | `c49ae741d40c2c87d8917b277c08f8e1` — 빌드 결과(`bin\Release`)와 **같다** |
| 그때 떠 있던 것 | **서버만**(`SPT.Server`, 사용자가 돌리는 중). 게임·런처는 꺼져 있었다. 이 플러그인은 게임 전용이라 서버와 무관하고, **서버는 건드리지 않았다** |
| 대상 모드 | `ManimalIcebreakerClient.dll` 제자리에 있다 — 스크립트가 없으면 경고하는데 나오지 않았다 |

**아직 로드된 적은 없다.** `BepInEx\LogOutput.log` 의 수정 시각이 **15:01** 로 설치(17:53)보다 앞이다.
다음 기동에서 볼 것 셋:

1. `걸었다: Manimal.Icebreaker.RenderEnvProbe:TickVanillaPerfCensus`
2. `걸었다: Manimal.Icebreaker.IcebreakerAcoustics:IcebreakerLoaded (씬 이벤트에서 버린다)`
3. **`[PerfCensus]` 가 한 줄도 없어야 한다**(11-11 기동에는 셋 있었다). `못 찾았다: …` 경고가 나오면
   모드 쪽 이름이 바뀐 것이므로 이 문서의 §1 표를 다시 확인한다.

Forge 모드가 아니라 설치 기록(`doc/install-records/`)에는 들어가지 않는다 — 그래서
`node tools/check-install-records.mjs` 의 숫자(305개 / 15,948줄)는 **그대로다**(설치 후 확인: `OK — 0/0`).

## 6. 첫 기동에서 **걸리지 않았다** — 열쇠의 홑따옴표 (2026-11-12)

위 §5 의 "다음 기동에서 볼 것 셋" 은 **셋 다 실패**였다. 2026-11-12 기동 로그
(`BepInEx\LogOutput.log`, 17:11 기동, `248 plugins to load`)에서:

| 봐야 했던 것 | 실제 |
| --- | --- |
| `걸었다: …TickVanillaPerfCensus` | **없다** |
| `걸었다: …IcebreakerLoaded` | **없다** |
| `[PerfCensus]` 한 줄도 없어야 | **3회** — bigmap(lodGroups 68,028) · hideout(1,109) · Sandbox_high(40,559) |

`Loading [Icebreaker Off-Map Guard 0.1.0]` 뒤로 이 플러그인이 적은 줄이 **하나도 없다**.
소스는 어느 길로 가도 "걸었다"·"못 찾았다"·"걸 것이 없다" 중 하나를 적으므로, `Awake` 가
**로그에 닿기 전에 터진 것**이다. 설치본 DLL 은 빌드본과 md5 가 같았으니 설치 사고가 아니다.

### 원인

`BepInEx\config\spt.icebreakeroffmapguard.cfg` 에 설정이 **셋 중 하나만**(`Skip vanilla perf census`)
적혀 있었다 — 두 번째 `Config.Bind` 에서 멈춘 모양이다. 그 열쇠가
`Cache 'is Icebreaker loaded'` 였고, **BepInEx 는 구역·열쇠에 홑따옴표를 금지한다.**
설치본 `BepInEx\core\BepInEx.dll` 을 직접 불러 재었다:

```text
[General] / [Skip vanilla perf census]        → OK
[General] / [Cache 'is Icebreaker loaded']    → ArgumentException
    Cannot use any of the following characters in section and key names: = \n \t \ " ' [ ] (Parameter 'key')
```

설치본 `.cfg` 190개를 훑어도 열쇠에 홑따옴표를 쓴 것은 하나도 없다(남의 모드도 안 쓴다).
F12 창에 항목이 하나만 뜨고 아무것도 안 가려지는 모양이 그래서 나왔다.

### 조치 (2026-11-12 19:07)

1. 열쇠 이름을 `Cache the is-Icebreaker-loaded answer` 로 바꿨다. 설명문은 그대로다 —
   금지 문자는 **구역·열쇠에만** 걸린다.
2. 이름 셋을 `src/IcebreakerOffMapGuard/ConfigKeys.cs` 한곳에 모으고,
   `ConfigKeysTests` 가 금지 문자 여덟 개를 지킨다(실제로 터진 그 이름을 반례로 박아 뒀다).
   테스트는 BepInEx 를 참조하지 않아 금지 목록을 베껴 두고, 그 목록이 실제와 같은지는
   위처럼 설치본 어셈블리로 따로 쟀다. 시험 16개 전부 통과.
3. `Awake` 를 `try/catch` 로 감쌌다. 다시 터지더라도 **`[Error]` 한 줄은 남는다** —
   이번에는 조용히 쉬어서 "깔렸는데 안 돈다" 를 하루 동안 몰랐다.
4. 다시 빌드해 넣었다. **11,776 B / md5 `1f2b3a85c99cc3cfde2133a2bc399b24`** — 빌드 결과와 같다.
   그때 게임이 떠 있어(17:11 기동) DLL 이 잠겨 있었으므로, 설치 스크립트에
   `-AllowGameRunning` 을 더해 **돌고 있는 DLL 을 이름만 바꿔 치우고**(`.dll.locked-20261112-190828`)
   새것을 넣었다. 돌고 있는 게임은 옛 코드를 그대로 쓴다. **게임도 서버도 끄지 않았다.**

### 아직 확인하지 못한 것

- 다음 게임 기동에서 §5 의 셋(걸었다 두 줄 + `[PerfCensus]` 0회). **지금 돌고 있는 게임에서는
  확인되지 않는다** — 옛 DLL 이 메모리에 올라가 있다.
- 치워 둔 `IcebreakerOffMapGuard.dll.locked-20261112-190828` 은 게임이 꺼진 뒤에 지운다
  (BepInEx 는 `.dll` 만 읽으므로 둬도 로드되지 않는다).
- 새 `.cfg` 는 다음 기동에 다시 쓰인다. 그때 F12 항목이 **셋** 보여야 하고,
  `node tools/check-lang-coverage.mjs` 의 "사전 파일 없음" 한 줄(항목 1개 → 3개)도 그때 다시 센다.

## 7. 인게임에서 확인됐다 (2026-11-12 21:55~21:59)

§6 의 조치 뒤 첫 게임 기동(21:36). **§5 가 적어 둔 셋이 모두 뒤집혔다.**

| 봐야 했던 것 | 11-12 17:11 기동 | 11-12 21:36 기동 |
| --- | --- | --- |
| `걸었다: …TickVanillaPerfCensus` | 없음 | **나왔다** |
| `걸었다: …IcebreakerLoaded (씬 이벤트에서 버린다)` | 없음 | **나왔다** |
| `[PerfCensus]` | **3회**(bigmap·hideout·Sandbox_high) | **0회** |

곁들인 증거 — `BepInEx\config\spt.icebreakeroffmapguard.cfg` 의 설정이 **1개에서 3개**로 늘었다.
홑따옴표 때문에 두 번째 바인드에서 터지던 것이 풀렸다는 뜻이다.

```text
Skip vanilla perf census = true
Cache the is-Icebreaker-loaded answer = true     ← 지난 기동에는 이 줄이 없었다
Log a line every 60 s = false
```

**어느 맵인지까지 확정했다.** 조사는 맵 진입 약 60초 뒤에 도는 것이라 로비에서 0회인 것은
증거가 되지 않는다. 그래서 돌고 있는 서버에게 게임이 보고한 상태를 물었다:

```text
게임 연결: true | 레이드 중: true | 맵: hideout
```

`hideout` 은 지난 기동에서 `[PerfCensus] map=hideout … lodGroups=1109` 가 찍혔던 바로 그 맵이다.
진입 21:55:27, 확인 21:58~21:59 — **60초 문턱을 세 배 넘겨도 한 줄도 나오지 않았다.**

이번 기동의 BepInEx 오류는 1건인데 `Direct Reload` 가 Fika 를 못 찾는 것으로, 이 설치본에
Fika 가 없어 나는 묵은 줄이다(가드와 무관).

**남은 것**: 라이트하우스·Interchange 같은 큰 맵에서도 0회인지는 그 맵에 들어가 봐야 한다.
조사 자체가 맵마다 한 번씩 도는 것이라 같은 결과가 나올 것으로 보지만, 잰 것은 hideout 뿐이다.
