# 리워크 라이트하우스 진입 로딩에서 멈췄다 (2026-11-06)

**상태: 원인 미확정.** 한 번 일어났고, 멈춘 자리와 그때의 메모리 상황만 실측했다. 게임이 다시 켜지면서 `BepInEx/LogOutput.log` 가 **덮어써져 원본 로그는 남지 않았다** — 아래 인용은 멈춘 동안 읽어 둔 것이다.

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

## 참고 — 다른 맵은 멀쩡했다

리워크 인터체인지는 같은 설치본에서 **15분 42초를 끝까지 돌았다**(2026-11-06 04:18~04:34, 기록된 위치 54,951개). 교체 장면 18개를 띄우고 셰이더 1,650개쯤을 다시 묶었다. 그래서 "리워크 맵이면 다 멈춘다" 는 아니다.
