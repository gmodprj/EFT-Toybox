# 리워크 라이트하우스 진입 로딩에서 멈췄다 (2026-11-06)

**상태: 원인 미확정.** 두 번 시도해 한 번 멈추고 한 번은 47분을 돌았다. 멈춘 자리와 두 번의 메모리 상황을 실측했고, **메모리 여유로는 설명되지 않는다**(2차가 60배 적은 여유에서 통과했다). 게임이 다시 켜지면서 `BepInEx/LogOutput.log` 가 **덮어써져 원본 로그는 남지 않았다** — 아래 인용은 멈춘 동안 읽어 둔 것이다.

## 무슨 일이 있었나

10-O 를 다 설치한 뒤 첫 기동(2026-11-06 07:14~)에서 사용자가 Map Variants 창에서 리워크 라이트하우스를 골랐고(`[MV] player chose Lighthouse - Manimal`), **진입 로딩 중에 멈췄다.**

## 멈춘 자리

로그의 마지막 다섯 줄이다.

```
[Manimal-LighthouseRework] Lighthouse rain: live renderer bound to Manimal/Lighthouse/RainDrops; native motion, density and roof mask preserved.
[CloudSix] Shadow RT created: True
[RequestHandler] [REQUEST]: /airdrop-caller/heartbeat        ← 마지막 심장박동
[Manimal-LighthouseRework] Lighthouse: removed 112 unavailable component references from Lighthouse_Scripts_ML lookup caches.
[ReflectSix] [ReflectSix] Baked cube reflections removed from 2135 materials (3639 this raid).
```

**ReflectSix 가 마지막으로 적혔다는 것이 ReflectSix 가 원인이라는 뜻은 아니다** — 그 뒤에 아무 말 없이 일하는 다른 모드가 있을 수 있다.

### 심장박동이 멈췄다 — 메인 스레드가 한 작업에 붙잡혔다

`/airdrop-caller/heartbeat` 는 로딩 중에도 몇 초마다 꾸준히 찍힌다. 그것이 위 자리에서 **끊겼다.** 즉 비동기로 뭔가 돌다 느려진 것이 아니라 **메인 스레드가 한 긴 동작에 들어가 돌아오지 않았다.**

| 항목 | 실측 |
| --- | --- |
| 로그가 멈춘 시각 | 07:55:32 |
| 확인한 시각 | 08:01:20 — **약 6분 동안 로그 0줄** |
| 게임 프로세스 | 살아 있고 `Responding=True` (윈도가 보기에 응답 중) |
| 게임 작업셋 | **9,949 MB** |
| 시스템 메모리 | 전체 32,649 MB / **남은 4,466 MB** |

로딩 자체도 원래 무겁다 — 이 모드가 적은 대로 **파일 검증에 50.6초, 준비 완료까지 55.9초**가 걸린 뒤에야 장면이 시작된다.

```
Lighthouse preparation: file verification finished at 50.6s; opening bundles asynchronously.
Lighthouse preparation completed in 55.9s; starting scenes.
Lighthouse test: native camera/weather loaded; removed 2145 old geographic components and 2 old border/water roots.
```

### 메모리 쪽 정황이 하나 더 있다

멈춘 동안 **이 세션의 셸 훅(Go 바이너리)도 두 번 연달아 `fatal error: out of memory allocating heap arena map` 으로 죽었다.** 모드와 상관없는 별개 프로그램이 같은 시각에 메모리를 못 잡은 것이므로, 그때 기계 전체가 메모리에 쪼들렸다는 독립된 증거다.

게임이 다시 켜진 뒤(08:01:27, 작업셋 3,087 MB) 남은 메모리는 **15,038 MB** 였다. 멈췄을 때의 4,466 MB 와 3배 차이다.

## 그래서 무엇이 원인인가 — 아직 모른다

세울 수 있는 가설은 둘이고, 가린 적이 없다.

1. **메모리 압박.** 2.3 GiB 짜리 번들을 여는 중에 남은 메모리가 4 GB 대로 떨어졌고, 교체(swap)와 GC 로 진행이 사실상 멈췄다.
2. **모드 하나의 레이드 시작 패스가 새 맵에서 오래 걸렸다.** matsix 묶음은 레이드가 시작될 때 맵 전체를 한 번 훑는다 — TraceSix 는 복셀 세계를 짓고, POMSix 는 "이 맵·계절의 첫 레이드에만" 높이맵을 만들고, ReflectSix 는 재질을 돌며 구워 둔 반사를 떼어 낸다(이번 레이드에 3,639개).

둘은 배타적이지 않다. 둘 다일 수도 있다.

## 변수 하나만 바꾸는 시험

TraceSix 는 맵 이름으로 자기를 끄는 설정을 이미 갖고 있고, 지금 값은 이렇다.

```
E:\Spt\BepInEx\config\com.matsix.tracesix.cfg
Maps Without Indirect Light = laboratory,labyrinth
```

여기에 라이트하우스를 더하면 **TraceSix 의 복셀·조명 패스만** 빠지고 나머지는 그대로 간다. 한 줄이고 되돌리기도 쉽다. F12 에서 "되튄 빛 조명 (TraceSix) → 1. 일반 → 되튄 빛을 끄는 맵" 으로 찾을 수 있다(2026-11-06 한글화 2차에서 넣었다).

그래도 멈추면 TraceSix 는 범인이 아니다. 그다음 후보는 POMSix 의 높이맵 생성(`요철 방식` 을 POM 으로 낮추거나 `켜기` 를 끈다)과 ReflectSix 의 `구워 둔 반사 없애기`(지금 `true`)다.

**아직 아무것도 바꾸지 않았다.** 한 번 멈춘 것으로 설정을 건드리기 전에, 같은 자리에서 또 멈추는지 먼저 봐야 한다 — 되풀이되지 않으면 그때의 메모리 상황이 원인이었을 쪽에 무게가 실린다.

## 2차 시도 — 같은 맵을 47분 돌았다 (2026-11-06 08:07~08:54)

게임을 다시 켜고(08:01:27) 곧장 리워크 라이트하우스에 들어갔다. **통과했고 레이드를 끝까지 돌았다.**

| 항목 | 값 |
| --- | --- |
| 레이드 | `lighthouse`, `[MV] raid on 'lighthouse': BACKPORT` |
| 시작 ~ 끝 | 08:07:27 ~ 08:54:33 (**47분 6초**) |
| 기록된 위치 | **249,134개** (15분 42초였던 인터체인지 레이드는 54,951개) |
| 레이드 구간 서버 오류 | `[Error]` **48줄 — 전부 알려진 `Could not add`**, `[Critical]` **0** |
| 진입 성공 신호 | 08:09:09 에 ORBIT 이 프레임 번호를 찍기 시작 |

### 1차에서 멈췄던 자리 뒤에 실제로 온 것

평범한 레이드 시작 절차였다. **막고 있던 모드가 따로 있었던 것이 아니다.**

```
/ISB/revenge/raid/start                 성공
/ISB/aishiLoyaltyCondition/Lighthouse   성공
[CAS] HUD overlay created
[FogSix] Layered froxels verified — volume + integration render in one draw per 32 slices.
[FogSix] Volumetric glass shader swapped onto 86 materials (86 total)
```

### 메모리 — 가설 ①을 그대로 세울 수 없다

| | 남은 물리 메모리 | 게임 작업셋 | 결과 |
| --- | --- | --- | --- |
| 1차 (07:55 멈춤) | **4,466 MB** | 9,949 MB | 멈췄다 |
| 2차 (08:09 레이드 중) | **72 MB** | 11,894 MB | **47분 돌았다** |
| 2차 (08:56 레이드 뒤 화면) | 401 MB | **12,596 MB** (최대) | 정상 |
| 2차 (08:57:56 정리 끝) | 6,681 MB | 9,319 MB | — |

**2차가 1차보다 60배 적은 여유에서 통과했다.** 그러므로 "남은 메모리가 적어서 멈췄다" 는 설명은 **버린다.** 앞서 메모리 쪽에 무게를 실었던 것을 여기서 거둔다.

커밋은 레이드 중 69,160 MB 까지 올라갔다(물리 32,649 MB). 페이지 파일이 35 GB 가까이를 떠안은 채로도 47분을 돌았으므로, **이 설치본에서 교체(swap)는 그 자체로 멈춤의 원인이 아니다.**

### 남은 가설 하나와, 재지 않은 것

차이는 **프로세스의 나이**다. 1차는 07:14 에 켜서 메뉴·은신처를 40분 넘게 돌아다닌 뒤 들어갔고, 2차는 08:01 에 새로 켜서 곧장 들어갔다. 오래 산 유니티 프로세스의 힙 단편화는 흔한 설명이지만 **재지 않았다** — 재려면 1차와 같은 조건(오래 돌아다닌 뒤 진입)을 일부러 만들어 한 번 더 멈추는지 봐야 한다.

**설정은 아무것도 바꾸지 않았다.** TraceSix 의 `Maps Without Indirect Light` 에 라이트하우스를 더하는 시험은 적어 두었을 뿐 손대지 않았다 — 두 번째 시도가 통과했으므로 그 시험을 할 근거가 지금은 없다.

### 그래서 지금 말할 수 있는 것

1. **리워크 라이트하우스는 멀쩡히 돌아간다.** 47분 레이드, 위치 249,134개, 새 오류 0.
2. **여유는 거의 없다.** 레이드 중 남은 물리 메모리가 72 MB 까지 내려간다. 멈춤이 또 나도 이상한 일이 아니다.
3. **멈춘 원인은 아직 모른다.** 한 번 일어나고 한 번은 일어나지 않았으며, 두 번의 차이로 설명되는 변수는 "프로세스가 얼마나 오래 살아 있었는가" 뿐인데 그것을 확인하지 않았다.

실용적으로는 **새로 켠 게임에서 바로 들어가는 것**이 1차의 조건을 피한다. 또 멈추면 그때 TraceSix 제외부터 한 번에 하나씩 짚는다.

## 참고 — 다른 맵은 멀쩡했다

리워크 인터체인지는 같은 설치본에서 **15분 42초를 끝까지 돌았다**(2026-11-06 04:18~04:34, 기록된 위치 54,951개). 교체 장면 18개를 띄우고 셰이더 1,650개쯤을 다시 묶었다. 그래서 "리워크 맵이면 다 멈춘다" 는 아니다.
