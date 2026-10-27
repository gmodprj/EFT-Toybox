# PIT Fireteam — `LootPatrolLayer.GetDecision` NullReferenceException

2026-10-27 레이드 로그(`E:\Spt\BepInEx\LogOutput.log`, 16:51)에서 3회 발생.

## 로그

```
[Error :ModulePatch] LootPatrol GetDecision exception suppressed to keep AI update alive
[Error :ModulePatch] System.NullReferenceException: Object reference not set to an instance of an object
  at (wrapper dynamic-method) LootPatrolLayer.DMD<LootPatrolLayer::GetDecision>(LootPatrolLayer)
```

치명적이지 않다. **PIT 가 스스로 잡아 AI 를 계속 돌린다**("suppressed to keep AI update
alive"). 게임은 멈추지 않았고 레이드도 정상 종료됐다.

## 알아낸 것

### 1. PIT 가 이 메서드를 패치한다

로드 때 두 패치가 걸린다.

```
[Info :ModulePatch] Enabled patch LootPatrolActiveLayerListPatch
[Info :ModulePatch] Enabled patch LootPatrolDecisionBypassPatch
```

DLL 에 `IsVanillaLootPatrol` 이라는 판별자가 있다. 바닐라 LootPatrol 과 자기 쪽을 구분한다는
뜻이다. 같은 성격의 "가드" 패치가 다른 바닐라 레이어에도 붙어 있다 —
`AdvAssaultTargetFollowerGuardPatch`, `PatrolDataFollowerUpdateGuardPatch`,
`AvoidDangerFollowerGuardPatch`. 즉 **바닐라 브레인 레이어가 PIT 상황에서 깨지는 것을 막는
패턴이 모드 전반에 있고, LootPatrol 은 그중 예외를 잡아 삼키는 쪽**이다.

예외 문구를 내는 주체가 PIT 이므로, **PIT 는 신고자이지 반드시 원인은 아니다.** 다만 이
메서드를 패치하는 유일한 모드이기도 하다.

### 2. 이번 레이드에는 동료가 없었다 — 이게 핵심이다

| | 12:21 레이드 | 16:51 레이드 |
| --- | --- | --- |
| `/client/game/bot/followerdetails` | 호출됨 | **0회** |
| `/client/game/bot/followerprogress` | 호출됨 | **0회** |
| LootPatrol NRE | **0회** | **3회** |

**동료를 데려갔을 때는 안 났고, 동료 없이 들어갔을 때 났다.** 직관과 반대다. PIT 의 LootPatrol
패치가 동료가 아닌 일반 봇에도 걸리고 있고, 동료용 상태가 없는 쪽에서 널이 난다는 설명과
맞아떨어진다.

### 3. 발생 시점이 봇 스폰 직후다

세 번 모두 `Updated spt bot <이름>: pmcUSEC/pmcBEAR to use: pmcXXX brain` 경고 바로 뒤다.

```
[Warning:ModulePatch] Updated spt bot CZPZ: pmcBEAR to use: pmcBEAR brain
[Error  :ModulePatch] LootPatrol GetDecision exception suppressed ...
```

```
[Warning:ModulePatch] Updated spt bot bunray: pmcUSEC to use: pmcUSEC brain
[Warning:ModulePatch] Updated spt bot gley: pmcUSEC to use: pmcUSEC brain
[Error  :ModulePatch] LootPatrol GetDecision exception suppressed ...
```

새로 스폰된 PMC 봇이 브레인을 배정받은 직후에 터진다. 초기화가 끝나기 전에 `GetDecision` 이
한 번 도는 경합으로 보이지만, **이건 추정이다.**

## 확인하지 않은 것

- 어느 봇에서 났는지 로그에 남지 않는다. 스폰 직후라는 상관관계만 있다
- PIT 를 빼고 같은 조건으로 돌려 보지 않았다. 바닐라 SPT 4.1.6 단독에서도 나는지는 모른다
- 8-F Make Shotguns Great Again(이번에 추가된 대량 아이템)이 전리품 판단에 영향을 주는지도
  확인하지 않았다. LootPatrol 이 "주울 물건" 을 고르는 레이어라 배제할 수 없다
- 재현 조건을 못 박지 못했다. 한 레이드에서 3회 났을 뿐이다

## 영향

동료의 전리품 순찰 판단이 간헐적으로 실패한다. 예외가 삼켜지므로 그 틱의 결정이 비는 것이고,
AI 가 멈추거나 게임이 죽지는 않는다. 이번 레이드에는 동료가 없었으므로 **실제로 체감된 피해는
없었다.**

---

# 제작자 보고용 (영문)

모드 페이지 "Reporting an issue" 가 요구하는 항목을 채운 것이다.

```
Title: LootPatrolLayer.GetDecision NullReferenceException (suppressed) with no followers in raid

Versions
- SPT 4.1.6 (EFT 0.16.9.5.40743)
- pitFireTeam 1.0.0 (Forge, 2026-10-24)
- No pitFireTeam SAIN Addon. SAIN is NOT installed.

Mode / settings
- GameplayMode: GunsForHire
- LoadoutManagement: Immersive
- Pickup: true, TieredPickup: true, MaximumPickup: 2, RecruitPickup: true
- LootMinimumPrice 20000 / LootMaximumPrice 5000000
- LootFilterFood/Meds/Valuables/Weapons: true, LootFilterGear: false
- ScanDistance 165, PatrolRadius 50
- FactionHostilities: false, PmcArmbands: true, BotPrefetch: true, SpawnPoint: true

Follower involved
- NONE. This raid had no followers at all. The client made no
  /client/game/bot/followerdetails or /client/game/bot/followerprogress calls.
  An earlier raid on the same install WITH followers produced zero occurrences
  of this error.

What happened
Three times during the raid the client logged:

  [Error :ModulePatch] LootPatrol GetDecision exception suppressed to keep AI update alive
  [Error :ModulePatch] System.NullReferenceException: Object reference not set to an instance of an object
    at (wrapper dynamic-method) LootPatrolLayer.DMD<LootPatrolLayer::GetDecision>(LootPatrolLayer)

Each occurrence directly follows SPT assigning a brain to a freshly spawned PMC
bot, e.g.

  [Warning:ModulePatch] Updated spt bot bunray: pmcUSEC to use: pmcUSEC brain
  [Warning:ModulePatch] Updated spt bot gley: pmcUSEC to use: pmcUSEC brain
  [Error  :ModulePatch] LootPatrol GetDecision exception suppressed ...

Nothing was ordered or transferred; no follower existed to command, so
"Attention" was not applicable. Gameplay was otherwise unaffected - the
suppression kept AI alive and the raid finished normally.

Relevant other mods (bot/AI related only)
- DrakiaXYZ BigBrain 1.5.0, DrakiaXYZ Waypoints 1.9.0 (both are the only
  4.1.6-listed builds; both were published 2026-08-03, before SPT 4.1.6)
- Tushonka-800 Combat Augmentation System 1.0.1 (draws a scan overlay; patches
  shield colliders and wheel input only)
- No SAIN, no Looting Bots, no APBS, no custom spawn mods.

Note
LootPatrolDecisionBypassPatch / LootPatrolActiveLayerListPatch are the only
patches on this method in the install, so I could not tell whether the NRE
originates in vanilla LootPatrolLayer or in state set up by the bypass prefix.
It fires for bots that are not followers, which is why I am reporting it even
though the mod suppresses it.
```

## 어디에 올리나

- Forge 모드 페이지 댓글: https://sp-mod.com/mod/2676
- 모드 문서의 "Reporting an issue" 절이 위 항목들을 요구한다
