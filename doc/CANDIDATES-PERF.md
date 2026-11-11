# 성능·메모리 정리 후보 (2026-11-11)

> 조사: 2026-11-11. 출처 sp-mod.com API v0. 재조회는
> `node tools/sweep-candidates.mjs "<정규식>"`(목록 전체 훑기) + `node tools/survey-candidates.mjs`(버전·용량·결속·의존성).
> 원자료는 [stage2-survey/sweep-perf-416.json](stage2-survey/sweep-perf-416.json)·[stage2-survey/survey-perf-416.json](stage2-survey/survey-perf-416.json).

- 대상 설치본: `E:\Spt` — SPT **4.1.6**, 모드 **304개** / 기록 15,947줄(`node tools/check-install-records.mjs` → `OK — 0/0`, 2026-11-11 실측)
- 이 PC: RAM **32GB** · **RTX 2080 Ti**. **VRAM 값은 WMI 로 재지 않는다** — `Win32_VideoController.AdapterRAM` 은 32비트로 잘려 2080 Ti 를 4GB 로 보고한다
- 아직 **아무것도 받지 않았고 설치하지 않았다.** 이 문서는 고르기 전 단계다
- 이미 깔려 있는 304개 쪽의 메모리 실측은 [MEMORY-AUDIT-2026-11-11.md](MEMORY-AUDIT-2026-11-11.md) 에 있다 — 3시간 17분 기동에서 **2,367 → 6,696 MB**, 떠난 맵 번들이 가장 유력한 정황이다(그래서 3209 의 순위가 올라간다)
- 아래 버전은 전부 **4.1.6 호환 최신**이다. 호환 버전이 없는 것은 "차단" 절에 사유와 함께 적었다

## 조회 방식 — 키워드만으로는 못 센다

이름·소개의 키워드로만 훑는 [`tools/search-mods.mjs`](../tools/search-mods.mjs) 로 먼저 열 몇 낱말(`memory`·`RAM`·`performance`·`FPS`·`culling`·`cleaner`·`garbage`·`leak`·`unload`·`asset`·`grass` 등)을 돌렸다. 그 다음 **4.1.6 호환 모드 목록 전체 751개를 카테고리까지 받아 와** 같은 정규식을 걸었다(새로 만든 [`tools/sweep-candidates.mjs`](../tools/sweep-candidates.mjs)).

**뒤쪽에서 둘이 더 나왔다** — 다운로드 68,200의 **Ai Limit**(카테고리 Bots)과 17,042의 **Smoothie**. 앞쪽 키워드 조회에서는 둘 다 안 걸렸다. 적중 49개 가운데 **21개는 이미 설치본에 있었고**, 남은 미설치 28개에서 성능·메모리와 무관한 것(모드 매니저·FOV·실내 밝기·일시정지·시네마틱 등)을 걸러 아래 10개를 남겼다.

## 이미 깔려 있는 성능 계열 — 중복을 피하려고 먼저 센다

| 모드 | id | 하는 일 |
| --- | --- | --- |
| [Picture in Picture Disabler](https://sp-mod.com/mod/2667) | 2667 | PiP 조준경을 끈다 |
| [Dynamic External Resolution Patch (DERP)](https://sp-mod.com/mod/2200) | 2200 | 조준경 외부 렌더 해상도를 동적으로 낮춘다 |
| [Performance Scope](https://sp-mod.com/mod/3076) | 3076 | 조준경 안 해상도 조절 |
| [ORBIT 3.0](https://sp-mod.com/mod/2706) | 2706 | 봇 밀도 — "Ghost Mode" 가 높은 프레임용 |
| [Shadow Flicker Fix](https://sp-mod.com/mod/1621) · [No Barrel Shadow](https://sp-mod.com/mod/2780) · [Death Shadow Tweaks](https://sp-mod.com/mod/2943) | 1621·2780·2943 | 그림자 쪽 |
| [Task List Fixes](https://sp-mod.com/mod/824) | 824 | 작업 목록 정렬·성능 |
| [Raid Menu Cleanup Fix](https://sp-mod.com/mod/2733) | 2733 | 레이드 중 ESC 메뉴를 열 때의 멈춤 제거 |
| [Better Thermal & Night Vision](https://sp-mod.com/mod/2531) | 2531 | 열상·야시의 프레임 상한 해제 |
| [BushWhacker - Standalone](https://sp-mod.com/mod/2329) | 2329 | 수풀(풀은 아니다) |
| [MANIMAL'S ICEBREAKER BACKPORT](https://sp-mod.com/mod/2866) 이 함께 넣은 `PerfectCullingRuntime.dll` | 2866 | 그 맵의 컬링 런타임. **플러그인 속성이 없어 플러그인 수에는 0 으로 센다** |

**반대로 지금 성능을 먹고 있는 쪽**도 설치본에 있다 — TraceSix(전체 레이트레이싱·복셀 GI, 3221), CloudSix(2567), ReflectSix(3099), POMSix(3100), FogSix(3110), AOSix(2569), HollywoodFX(2003). 무엇을 더 넣기 전에 **이쪽 설정을 내리는 것이 더 큰 몫**일 수 있다. 재지 않고 단정하지 않는다.

## 메모리 정리 (3개)

| 모드 | id | 버전 | 제약 | 크기 | 다운로드 | 결속 | 의존성 | 메모 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| [VRAM Cleaner](https://sp-mod.com/mod/2173) | 2173 | 1.1.0 | `4.1.6` | 3KB | 101,016 | - | - | 레이드 시작 타이머가 끝난 뒤 **쓰이지 않는 자산을 비운다**. Fika 호환 표기. 제작자가 "다른 모드와 문제를 겪은 적 없다" 고 적었으나 그건 제작자 말이고 실측은 우리 몫 |
| [Transit Ram Cleaner](https://sp-mod.com/mod/3209) | 3209 | 1.0.1 | `~4.1.6` | 7KB | 2,382 | - | - | 떠난 맵의 AssetBundle 을 게임 자체 `AssetsManager` 로 내린다. EFT 는 다녀온 맵 번들을 프로세스가 끝날 때까지 들고 있다. **`maps/` 아래 번들만** 건드리고 전리품·UI·음향 번들은 두며, 로드가 안 끝난 번들은 건너뛴다(1.0.1 의 안전 수정) |
| [Don't Collect My Garbage](https://sp-mod.com/mod/3139) | 3139 | 2.0.0 | `>=4.1.6 <4.2.0` | 3KB | 339 | - | - | 레이드 중 인벤토리를 열 때 SPT 가 돌리는 짧은 GC 를 **끈다**. **권하지 않는다** — 제작자가 "메모리 여유가 넉넉하지 않거나 크래시를 감수할 생각이 없으면 쓰지 말라" 고 문서 맨 앞에 적었고, 32GB 에 대형 맵 백포트 넷이 들어온 이 설치본은 넉넉한 쪽이 아니다 |

**3209 는 [BACKLOG 의 "리워크 라이트하우스 로딩 멈춤"](BACKLOG.md) 과 결이 맞는다** — 1차는 남은 4,466MB 에서 멈추고 2차는 72MB 에서 통과해 "메모리 여유로는 설명되지 않는다" 고 적어 둔 그 건이다. 두 번의 차이로 남은 변수가 **프로세스의 나이** 하나였는데, 맵 번들이 프로세스 수명 내내 쌓인다는 이 모드의 설명이 바로 그 변수를 가리킨다. 다만 **그 가설을 이 모드로 검증하려면 먼저 재현이 있어야 한다**([fixes/2026-11-06-lighthouse-rework-load-stall.md](fixes/2026-11-06-lighthouse-rework-load-stall.md)). 넣고 나면 "멈추지 않았다" 가 모드 덕인지 그냥 재현이 안 된 것인지 가릴 수 없다.

## 성능 (7개)

| 모드 | id | 버전 | 제약 | 크기 | 다운로드 | 결속 | 의존성 | 메모 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| [Mod Profiler](https://sp-mod.com/mod/2952) | 2952 | 1.4.0 | `~4.1.0` | 64KB | 3,636 | - | ~~WebOverlay~~(2879, 설치됨) | **모드를 끄지 않고 모드별 CPU 시간을 잰다.** 304개 가운데 무엇이 프레임을 먹는지 추측 대신 재는 길. 1.4.0 에 창을 열지 않고 재는 Headless 캡처가 들어왔다 |
| [Smoothie](https://sp-mod.com/mod/3132) | 3132 | 1.1.0 | `4.1.6` | 129KB | 17,042 | - | - | 깊이 버퍼 기반 **자체 컬링**(보이는 것만 그린다) + `UnityPlayer.dll` 패치 + 봇 스폰 CPU(AI brain 은 건드리지 않는다). 1.1.0 에서 커스텀스 공장 한 자리의 드로콜 3,900 → 1,800. 전부 F12 토글. **아래 "맞물림" 절을 먼저 볼 것** |
| [Ai Limit](https://sp-mod.com/mod/1945) | 1945 | 1.9.2 | `~4.1` | 11KB | 68,200 | - | - | 거리·수 상한으로 **먼 봇의 AI 를 잠시 끈다**(가까운 N마리만 켠다). 맵별 상한 설정. 제작자가 **Questing Bots 와 같이 쓰지 말라**고 적었는데 그건 설치본에 없다 |
| [Adaptive Bot Culling](https://sp-mod.com/mod/2874) | 2874 | 1.2.0 | `~4.1.0` | 17KB | 7,296 | - | - | 먼 봇의 brain·애니메이션을 EFT 자체 컬링·LOD 에 얹어 멈춘다. 유리한 경우 CPU 10~40%. **제작자가 "AI 대규모 모드와 충돌하거나 예상 밖으로 움직일 수 있다"** 고 경고하고, 컬링된 봇은 배회·반응·교전을 멈춘다 |
| [Lawnmower](https://sp-mod.com/mod/3231) | 3231 | 1.3.0 | `~4.1` | 7KB | 2,525 | - | - | 맵의 **풀을 지운다**. CWX 의 GrassCutter 를 허락받아 다시 쓴 것. F12 에서 실시간 토글이라 레이드를 다시 시작할 필요가 없다 — **되돌리기가 가장 쉬운 후보** |
| [NoMenuFPSLimit](https://sp-mod.com/mod/2364) | 2364 | 1.1.0 | `>=4.1.3` | 5KB | 8,053 | - | - | **메뉴**의 60프레임 상한만 푼다. 레이드와 무관하고, 그래픽 설정에서 60 이상을 따로 넣어야 한다 |
| [Escape From Low Frames](https://sp-mod.com/mod/2634) | 2634 | 1.0.4 | `4.1.6` | 562KB | 24,415 | - | - | 실체는 **NVIDIA Profile Inspector 설정 묶음**이다. `install-and-test.ps1` 절차 밖이고 드라이버 쪽을 건드린다 — 이 저장소 흐름에 맞지 않아 **보류** |

### 맞물림 — 넣기 전에 반드시 보는 것

- **Smoothie(3132) × Picture in Picture Disabler(2667, 설치됨)**: Smoothie 쪽 호환 표에 PiP-Disabler 가 **`✓½`** 로 적혀 있다 — "조준경을 보는 동안 사물이 튀어나온다(props pop)". 설치본에 2667 이 있으므로 **이 조합이 바로 걸린다.** 같은 표에 ORBIT(설치됨)·Ai Limit(1945)은 `✓` 다.
- **봇 쪽 셋이 겹친다**: ORBIT(2706, 설치됨) · Ai Limit(1945) · Adaptive Bot Culling(2874). 하는 일이 "먼 봇을 줄이거나 멈춘다" 로 포개지고, 설치본에는 SAIN(791)·BigBrain(902)·Waypoints(827)가 있다. **셋을 한꺼번에 넣지 않는다.**
- **결속 표시는 후보 10개 전부 없다**(API `shows_profile_binding_notice` = false). 프로필 백업을 쪼개 할 이유는 없지만, [PROCEDURE.md](PROCEDURE.md) 의 평소 절차는 그대로 간다.

## 차단 — 4.1.6 호환 버전이 없다 (10개)

실측으로만 판정했다. 아래는 전부 `filter[spt_version]=4.1.6` 조회에서 **0개**다.

| 모드 | id | 마지막 버전 | 아까운 점 |
| --- | --- | --- | --- |
| [CompoundingPerf](https://sp-mod.com/mod/2728) | 2728 | 1.3.0 `[4.0.13]` | **가장 아깝다.** 서버 쪽 — 플리마켓 매물 만료 때 강제로 도는 전체 GC 제거(최악 멈춤 8초 → 1.7초), 정적 엔드포인트 15개 응답 캐시, 빠른 압축. 클라이언트 플러그인 없음 |
| [Ram Cleaner Fix](https://sp-mod.com/mod/1185) | 1185 | 1.0.0 `[~3.7.0]` | 설정 메뉴에 들어갈 때만 돌던 RAM 청소를 주기(기본 300초)로 돌린다. 메모리 쪽 고전 |
| [LoadBundleFaster (Crc32 Patch)](https://sp-mod.com/mod/2563) · [LoadBundleEvenFaster](https://sp-mod.com/mod/2599) | 2563·2599 | 1.0.0 `[>=4.0.0 <=4.0.13]` | 번들 로딩 시간. 대형 콘텐츠가 많은 이 설치본에 맞았을 것 |
| [De-Clutterer](https://sp-mod.com/mod/1118) · [Simple Declutter](https://sp-mod.com/mod/2139) | 1118·2139 | 1.1.5 `[~3.7.0]` · 1.0.0+311 `[~3.11.0]` | 맵 잡동사니·그림자 제거. 2139 제작자 실측은 스트리츠에서 4~5프레임 |
| [Advanced Bot Culling](https://sp-mod.com/mod/1720) | 1720 | 1.0.0 `[~3.9.0]` | 제작자가 **"SPT 3.10 부터는 필요 없어 더 손대지 않는다"** 고 적었다 — 차단이 아깝지 않은 쪽 |
| [Expanded FPS Limit](https://sp-mod.com/mod/2066) · [Fps Unlocker](https://sp-mod.com/mod/1255) | 2066·1255 | 1.2.0 `[^4.0.0 <4.1.0]` · 1.2.0 `[~3.11.0]` | 프레임 상한 해제. 메뉴 쪽은 2364 가 대신한다. 1255 는 제작자가 **"성능이 올라가지는 않는다"** 고 못 박았다 |
| [SPTCorpseCleaner](https://sp-mod.com/mod/2058) | 2058 | 1.0.5 `[~4.0 <4.1.0]` | 레이드 중 시체 정리 |

## 권하는 순서

1. **[Mod Profiler](https://sp-mod.com/mod/2952)(2952)** — 먼저 **재고** 나서 고른다. 의존 WebOverlay 가 이미 있고, 끄지 않고 재므로 304개를 건드리지 않는다. 지금은 "무엇이 느린지" 를 아무도 모르는 상태다.
2. **[VRAM Cleaner](https://sp-mod.com/mod/2173)(2173)** — 3KB, 비결속, 되돌리기 쉽다. 메모리 쪽에서 위험이 가장 낮다.
3. **[Transit Ram Cleaner](https://sp-mod.com/mod/3209)(3209)** — 혼자 넣고, 환승을 이어 돌며 잰다. 나온 지 한 주고 1.0.1 이 번들 해제 안전 수정이라 **다른 것과 묶지 않는다**.
4. **[Lawnmower](https://sp-mod.com/mod/3231)(3231)** — F12 실시간 토글. 켠 레이드와 끈 레이드를 견준다.
5. **[Smoothie](https://sp-mod.com/mod/3132)(3132)** — 얻는 몫이 가장 클 수 있지만 PiP Disabler 와의 `✓½` 를 눈으로 확인해야 한다. 넣을 때 2667 을 어떻게 할지 함께 정한다.
6. **[Ai Limit](https://sp-mod.com/mod/1945)(1945)** — 봇 쪽에서 하나만 고른다면 이쪽(다운로드 68,200, 맵별 상한).
7. **[Adaptive Bot Culling](https://sp-mod.com/mod/2874)(2874)** — 맨 마지막, 혼자, 레이드로 확인. AI 모드와의 충돌 경고가 제작자 본인 것이다.

**빼는 것**: 3139(메모리 여유 전제가 안 맞는다) · 2634(절차 밖, 드라이버 설정).

## 다음

- 넣기로 하면 [PROCEDURE.md](PROCEDURE.md) 절차대로 간다 — `tools/install-and-test.ps1`, 덮어쓰기가 생기면 중단, 단계마다 `node tools/check-install-records.mjs` → `OK — 0/0`, 끝나면 `node tools/gen-installed-mods.mjs`.
- **잴 자리를 먼저 정한다.** 성능 모드는 "넣었더니 좋아진 것 같다" 로는 판정이 안 된다. 같은 맵·같은 시간대·같은 경로를 두 번 돌아 Mod Profiler 의 내보내기나 프레임 수치를 견준다.
- 2728(CompoundingPerf)·2563·2599 는 **4.1.x 로 올라오는지 지켜볼 거리**다. `node tools/check-mod-updates.mjs` 는 설치된 것만 보므로 이 셋은 따로 봐야 한다.
