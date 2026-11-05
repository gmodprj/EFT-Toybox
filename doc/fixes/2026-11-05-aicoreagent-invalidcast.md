# AICoreAgent.Update 의 InvalidCastException 2,262건 (2026-11-05)

## 무엇이 보였나

10-M 설치 뒤 첫 게임 기동(20:06 KST)에서 게임 로그의 `[Error` 가 **4,525줄**이었다. 기준선은 레이드를 깊게 돈 기동에서 9~15줄이었다.

| 출처 | 줄 수 |
| --- | --- |
| SPT Team Manager | **4,524** |
| Direct Reload (알려진 Fika 건) | 1 |

4,524줄의 내역: `AICoreAgent.Update exception` 2,262 + `System.InvalidCastException: Specified cast is not valid.` 2,254 + `NullReferenceException` 8. 즉 **예외 2,262건이 한 건당 두 줄**로 찍혔다. 매 프레임이다.

## 던지는 쪽은 STM 이 아니다

`src/SptTeamManager/client/Patches/AICoreAgentUpdatePatch.cs` 는 `AICoreAgent<BotLogicDecision>.Update` 에 걸린 **finalizer** 로, 예외를 적고 그대로 되던지기만 한다. **전달자다.**

스택의 맨 위는 `DrakiaXYZ.BigBrain.Patches.BotAgentUpdatePatch.PatchPrefix` 다. BigBrain 1.5.0 의 원본을 확인했다 — 그 prefix 는 `Update` 를 **통째로 대체**하고(`return false`) 안에서 이것을 부른다.

```
__instance._strategy.Update(__instance._lastResult)
```

그 파일에는 **명시적 캐스트가 하나도 없다.** 유일한 타입 검사가 `nodeInstance is CustomLogicWrapper` 인데 `is` 는 실패해도 예외를 내지 않는다. 그러므로 `InvalidCastException` 은 BigBrain 코드가 아니라 **그 아래 게임 자신의 brain 코드**에서 난다.

## 레이드 안이었다 — 처음엔 메뉴로 잘못 읽었다

서버 로그에 `/client/match/local/start` 가 없고 SAIN·ORBIT 런타임 줄이 안 보여 "메뉴에서 난다"로 읽었다. **틀렸다.** 게임 로그 3968줄의 `Deploy Screen Overhaul ... for 'laboratory'` 와 전투 기록 파일 이름(`20261105-202810-laboratory.jsonl`)이 레이드를 가리키고, 첫 오류는 4927줄로 그 뒤다.

**적어 둘 것:** 레이드 여부는 서버의 매치 줄로 가리지 말고 **Deploy Screen 줄**이나 **Combat Log·전투 기록 파일**로 가린다.

## 한 일 — 고치지 않고 계측부터 (`75eab70`)

finalizer 를 진단용으로 바꿨다. 예외는 그대로 되던진다.

- 같은 종류(예외 타입 + 스택 첫 줄)는 **처음 한 번만** 자세히 적는다.
- 그때 주변 상태를 함께 남긴다 — `UsingLayer`, `_strategy` 타입, `_lastActiveLayer`, `aICoreLayer`, `_layers`·`_activeLayers` 의 레이어 이름·타입, brain 쪽에서 찾은 `BotOwner` 의 이름·역할.
- 그다음부터는 세기만 하고 **30초마다 집계 한 줄**. 매 프레임 `LogError` 두 번의 비용이 사라진다.
- 읽기는 전부 `try` 로 감쌌다. 진단이 실패해도 원래 예외는 그대로 간다.

빌드를 위해 `addon/refs/4.5.1` 에 설치본의 `SAIN.dll`·`SAIN.Preset.Shared.dll` 을 넣었다 — SHA-256 이 `addon/docs/References.md` 의 값과 정확히 같다. 그 파일은 `.gitignore` 다.

## 그런데 재현되지 않았다

계측본을 설치하고 **같은 맵(연구소)** 을 더 깊게 돌았는데 예외가 **0건**이었다. Combat Log 의 실측 비교다.

| 레이드 | 시간 | 킬 | 발사 | 받은 피해 | AICoreAgent 예외 | 전체 `[Error` |
| --- | --- | --- | --- | --- | --- | --- |
| 20:27 연구소 | 660초 | 10 | 251 | 1,063 | **2,262건** | **4,525줄** |
| 21:10 연구소 | **1,512초** | **18** | **540** | **1,751** | **0건** | **2줄** |

뒤쪽이 두 배 넘게 길고 격렬했는데 0건이다. 그러니 **"연구소 + 봇"** 조건만으로는 나지 않는다. 봇이 있었던 것은 서버 로그의 `/ISB/ghostbots/generate` 반복으로 확인했다.

남은 2줄은 둘 다 기존 건이다 — 알려진 DirectReload+Fika 1, ECOTI per-frame `ArgumentOutOfRangeException` 1(기준선에 있던 것).

## 무엇이 달라졌나 — 두 가지를 한꺼번에 바꿨다 (내 잘못)

한 번에 하나만 바꿔야 했는데 둘을 같이 바꿨다. 그래서 **아직 확정하지 못했다.**

1. **STM 이 Debug → Release 가 됐다.** 이전 설치본이 Debug 였음을 확인했다 — `client/bin/Debug/net472/SptTeamManager.dll` 의 시각이 **2026-11-04 19:42:34** 로 설치본의 19:42:54 와 맞고, 크기도 3,342,336(Debug) ↔ 2,849,280(Release)로 다르다. `tools/install-sptteammanager.ps1` 의 **기본값이 `Debug`** 라서 그렇게 깔려 있었다.
2. **옛 `BepInEx/plugins/pitFireTeam` 폴더를 치웠다.** 안에 전투 기록 `.jsonl` 7개뿐이어서 `backup/pitFireTeam-leftover-20261105/` 로 옮겼다(지우지 않았다).

**1번이 유력하다.** STM 에는 `#if DEBUG` 가 18개 파일에 있고 그중 다수가 `client/BigBrain/` — 레이어와 액션 쪽이다(`FollowerPatrolLayer`, `FollowerCombatPush`, `FollowerCombatRiflemanEngagement`, `GestureCommandAction` 등). Debug 전용 경로가 brain 상태를 건드려 게임의 `_strategy.Update` 가 깨지는 상태를 만들었다고 보면 앞뒤가 맞는다. 2번은 DLL 이 없는 폴더라 BepInEx 가 플러그인으로 싣지 않으므로 영향이 없을 것으로 본다 — 다만 **실측하지 않았다.**

## 확정하지 않기로 했다 (2026-11-05, 사용자 결정)

Debug 로 다시 빌드·설치해 연구소를 한 번 돌면 갈린다. 그러나 **일부러 깨진 상태로 되돌리는 일**이고 그 레이드 한 번은 봇 AI 가 또 망가진다. 사용자가 **Release 로 두기**를 택했다.

그래서 이 건의 원인은 **"Debug 빌드가 유력하나 미확정"** 으로 닫는다. 다시 나면 그때 가린다 — 진단 계측이 남아 있어 **어느 봇의 어떤 레이어**인지 한 줄로 알려 주고, 로그가 4,524줄로 불지도 않는다. 되돌려 확정할 필요가 생기면 Debug 로 깔고 연구소를 한 번 돌면 된다.

## 실용적 결론 — 설치는 Release 로 한다

`tools/install-sptteammanager.ps1` 의 기본값이 `Debug` 인 것이 함정이었다. 앞으로 **`-Configuration Release`** 로 설치한다. 기본값 자체를 Release 로 바꾸는 것이 맞을 수 있다(별 건).

진단 계측은 그대로 둔다 — 다시 나면 **어느 봇의 어떤 레이어**인지 한 줄로 알려 주고, 2,262건이 와도 로그가 4,524줄로 불지 않는다.
