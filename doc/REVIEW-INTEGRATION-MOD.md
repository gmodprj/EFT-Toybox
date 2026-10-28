# 중복 기능을 통합 모드로 모을 수 있나 — 검토

2026-10-28. 브레인스토밍 4번. "설치 모드들의 기능을 확인하고 중복되는 기능(총탄 등의 스택변경
등)은 마지막에 로드되는 별도의 통합 관리 모드로 관리하자"를 검토했다.

**결론부터 — 된다.** 다만 되는 것과 안 되는 것이 갈린다. 아래 "무엇까지 되나"를 먼저 본다.

## 1. 마지막에 로드되는 모드를 만들 수 있나 — 된다 (서버 코드로 확정)

전제가 하나 틀려 있었다. [REVIEW-LOAD-ORDER.md](REVIEW-LOAD-ORDER.md) 에 "로드 순서는 밖에서
바꿀 수 없다"고 적었는데, 그것은 **남의 모드**에만 맞는 말이다. 자작 모드의
`Injectable.TypePriority` 는 내가 정하고, 그게 순서의 1차 기준이다.

SPT 서버 DLL 을 읽어 확정했다.

| 읽은 곳 | 무엇이 나왔나 |
| --- | --- |
| `SPTarkov.DI.DependencyInjectionHandler::InjectAll` | `OrderBy(TypePriority)` — **오름차순** |
| `SPT.Server.ProgramExtensions::RunPreSptLoadCallbacks` | `0 <= TypePriority < 200000` 인 `IOnLoad` 만 |
| `SPTStartupHostedService::StartAsync` | `TypePriority >= 200000` 인 `IOnLoad` 만 |
| `SPTarkov.Server.Core.DI.OnLoadOrder` (public) | 단계 상수가 이미 있다 (`Watermark` 0 … `PostLoad` 1000000) |

**설치된 서버 모드 114개의 `[Injectable]` 선언 603개가 전부 `TypePriority = 0`** 이다(앞서
실측). 그러니

```csharp
[Injectable(InjectionType.Singleton, OnLoadOrder.PostLoad)]
public class IntegrationService : IOnLoad { ... }
```

하나로 **모든 설치 모드 뒤, SPT 자신의 콜백 뒤**에 설 수 있다. 설정 파일로 순서를 적는 길은
없지만, 내 모드를 맨 뒤에 놓는 손잡이는 분명히 있다.

## 2. 무엇까지 되나 — 값은 되고, 행동은 안 된다

통합 모드가 할 수 있는 일은 **마지막에 값을 덮어쓰는 것**뿐이다. 다른 모드가 자기 일을 하는
것을 막지는 못한다. 그래서 겹침의 종류에 따라 갈린다.

| 겹침의 종류 | 통합으로 되나 | 왜 |
| --- | --- | --- |
| **덮어쓰기형** — 탄약 스택, 내구도, 화폐 스택, 아이템 크기 | **된다** | 마지막 값이 곧 결과다. 통합 모드가 유일한 출처가 된다 |
| **집합 계산형** — 칸 필터 합집합, 플레아 금지 목록 | **되지만 전부 다시 계산해야** | 남이 더하고 뺀 뒤의 상태에 또 더하는 꼴이라, 통합 모드가 "최종 집합"을 통째로 써야 뜻이 분명해진다 |
| **행동형** — 봇 AI, 상인 재입고 주기, 레이드 중 로직 | **안 된다** | DB 값이 아니라 코드 경로다. 통합 모드가 끌 수 없다. 그 모드를 빼는 것 말고는 방법이 없다 |

즉 통합 모드는 **"중복을 없애는 것"이 아니라 "최종값을 한 곳에서 정하는 것"** 이다. 중복 작업
자체는 그대로 돌고(서버 시작이 그만큼 느려지는 것도 그대로), 결과만 내가 정한 값이 된다.

## 3. 어디가 겹치나

### 실측된 것 3건 (이것만 확실하다)

[REVIEW-LOAD-ORDER.md](REVIEW-LOAD-ORDER.md) 의 "실측한 겹침 3건" 을 본다. 떠 있는 서버에서
최종 DB 를 받아 각 모드 설정과 대조한 결과다.

| 겹침 | 지금 상태 | 종류 |
| --- | --- | --- |
| 탄약 스택 — CobrasAmmoStack vs SVM | Cobra 쪽이 **0/31 완전히 죽어 있다** | 덮어쓰기형 |
| 화폐 스택 — SVM vs BarterItemsStacks | SVM 의 4건이 덮인다 (BarterItemsStacks 452/460 살아 있음) | 덮어쓰기형 |
| 플레아 판매 금지 — ISB-Aishi 243 + Lotus 1 vs TheBlacklist | TheBlacklist 의 `disableBsgBlacklist: true` 가 뒤집는다 | 집합 계산형 |

### 참조 신호 (약한 근거 — 확정이 아니다)

서버 모드 DLL 114개에서 아이템 템플릿 **필드 이름이 문자열로 남는 성질**을 써서, 같은 필드를
건드리는 모드를 세어 봤다. **이름만 읽고 쓰지는 않는 모드도 걸리므로 후보 목록일 뿐이다.**

| 필드 | 모드 수 | 통합 후보로서 |
| --- | --- | --- |
| `Filter` | 45 | 집합 계산형. 자작 `HelmetSlotUnion`·`SlotCategoryRelax` 도 여기 있다 |
| `Slots` | 42 | 집합 계산형 |
| `StackObjectsCount` | 43 | 대부분 상인 판매 목록을 만들며 쓰는 것이라 겹침이 아닐 수 있다 |
| `Durability` 23 / `MaxDurability` 15 | 23 | 덮어쓰기형. **통합 1순위** |
| `Grids`·`CellsH`·`CellsV` | 20 / 14 / 14 | 덮어쓰기형 |
| `Width`·`Height` | 19 / 17 | 덮어쓰기형 |
| `Damage` 17 / `PenetrationPower` 7 | 17 | 덮어쓰기형. AmmoStats·Amonya·AmmoGen·ISB-Aishi 가 겹친다 |
| `Cartridges` | 17 | 집합 계산형 |
| `StackMaxSize` | 13 | 덮어쓰기형. **이미 실측된 겹침이 여기 있다** |
| `CanSellOnRagfair` | 7 | 집합 계산형. AllWeaponsFlea·TheBlacklist·UltimateFlea 가 겹친다 |

이 표는 **어디를 재 볼지 정하는 데에만** 쓴다. 통합 대상으로 올리기 전에 3건처럼 최종 DB 와
대조해 실제로 겹치는지 확인해야 한다.

## 4. 그래서 만들까 — 권하는 범위

전부 통합하는 것은 권하지 않는다. 필드 45개짜리 모드를 만들면 그것 자체가 다음 사람이 풀어야
할 수수께끼가 된다. **실측으로 겹침이 확인된 덮어쓰기형부터, 좁게** 만드는 것이 맞다.

| 단계 | 범위 | 근거 |
| --- | --- | --- |
| 1 | 탄약·화폐 **스택 크기** (`StackMaxSize`) | 겹침 2건이 이미 실측됐다. 모드 13개가 같은 필드를 건드린다 |
| 2 | ~~**내구도**~~ **필요 없다 (2026-10-28 실측)** | 템플릿 겹침이 없다. 아래 "2단계는 필요 없었다" 참고 |
| 3 | **탄 성능** (`Damage`·`PenetrationPower`) | AmmoStats·Amonya·AmmoGen·ISB-Aishi 가 겹친다 |
| 안 함 | 칸 필터·플레아 금지 | 집합 계산형이라 자작 모드 둘(`HelmetSlotUnion`·`SlotCategoryRelax`)과 또 겹친다. 그 둘을 합치는 쪽이 먼저다 |
| 안 함 | 행동형 전부 | 통합 모드로 끌 수 없다 |

설정은 `config.jsonc` 에 **"내가 정하는 값만" 적고, 안 적은 것은 손대지 않는** 꼴이어야 한다.
전부 적게 만들면 다른 모드의 의도를 통째로 지우게 된다.

### 지금 바로 할 수 있는 더 싼 일

통합 모드를 만들기 전에, 이미 알고 있는 것만 치워도 상태가 나아진다.

| 할 일 | 왜 |
| --- | --- |
| ~~**CobrasAmmoStack 제거**~~ **했다 (2026-10-28)** | 0/31 완전히 죽어 있었다. 제거 뒤 다시 재 보니 구경 31개의 값이 하나도 변하지 않았다 |
| ~~SVM 의 죽은 화폐 스택 4건~~ **껐다 (2026-10-28)** | 값을 맞추는 대신 `EnableCurrency` 를 꺼서 BarterItemsStacks 를 유일한 주인으로 만들었다 |
| TheBlacklist `disableBsgBlacklist` **켜 둔 채 858건을 되살렸다 (2026-10-28)** | 죽은 설정이 아니라 모드의 본래 기능이다. 그래서 끄지 않고, 제작자가 막아 둔 것만 `customItemConfigs` 로 다시 막았다 — 858건 중 744건 반영. 세어 보니 244건이 아니라 858건이었다 |

## 4.5 2단계(내구도)는 필요 없었다 — 겹침이 없고, 대신 죽은 모드가 또 나왔다

1단계를 끝내고 2단계를 재 보니 **통합할 겹침이 없었다.**

### `MaxDurability`·`Durability` 를 쓰는 곳은 템플릿이 아니다

`set_` 호출을 찾아 보니 MaxDurability 12개 모드, Durability 9개 모드가 걸렸다. 그러나
**들여다보면 거의 전부 아이템 개체나 다른 객체다.**

| 무엇을 쓰나 | 어디 | 보기 |
| --- | --- | --- |
| 아이템 **개체** (`UpdRepairable`) | 수리·복원 | `RepairMaxService::RepairMaxItemByKit`, `PlateRestorationService::Restore`, `RepairCallbacks::HandleRepairWithKit` |
| 상인 물건 개체 | 판매 목록 정리 | `GunwrightRefresher::NormalizeDurability`, `PresetTraderRefresher::NormalizeDurability`, `TraderWeaponDurabilityService` |
| 퀘스트 조건 객체 | 조건 생성 | `LedgerMod::BuildFinishConditions`, `NATOQuartermasterMod::CreateHandoverCondition`, `RepeatableQuestGenerator::BuildFindItemCondition` |
| **자기가 만든 새 아이템** | 아이템 생성 | `MountOlympus/AddItemsRigsHelper`, `WolfiksHeavyTrooperMasks/ItemCreator` |

즉 **같은 템플릿 칸을 두 모드가 다투는 구도가 아니다.** 스택 때와 다르다. 템플릿을 쓰는 것은
`egbog-RepairMaxDurabilityServer` 의 `MaxRepairResource` 하나뿐이고, 그것은 수리 키트 자원이라
다른 모드와 겹치지 않는다.

### 전역 내구도를 바꾸려던 모드는 죽어 있다

`CobrasDurabilityTweaks` 는 설정이 `DurabilityBurnMultiplier: 0.12` 이고 로그도
"Modified 1002 weapons" 라고 말한다. **그런데 최종 DB 에 적용된 것이 하나도 없다.**

| | |
| --- | --- |
| SPT 기본 DB 와 최종 DB 에 다 있는 아이템 | 176 |
| 값이 그대로 | **176** |
| x0.12 이 적용됨 | **0** |

표본: `weapon_colt_m4a1_556x45` 는 기본 1.15 → 최종 1.15. 0.12 를 곱했다면 0.138 이어야 한다.

왜 안 먹는지는 코드에 단서가 있다. 이 모드는 속성을 **리플렉션**으로 고친다 —
`TryMultiplyNumericProperty(props, "DurabilityBurnRatio", 0.12)`. 이름은 맞다(그 속성이
실제로 있다). 다만 그 속성의 형이 `double?`(`Nullable`)이라, 숫자형을 가정한 변환이 조용히
실패하는 것으로 보인다 — `Try…` 라는 이름대로 실패를 삼킨다. **확정은 하지 않았다.**

**CobrasAmmoStack 에 이어 두 번째 죽은 Cobra 모드다.** 치우는 것이 맞다.

### 방법 메모 — 리플렉션으로 쓰는 모드는 `set_` 스캔에 안 걸린다

`CobrasDurabilityTweaks` 는 `set_` 호출 목록에 **한 줄도 나오지 않았다**(쓰는 속성이
`JsonSerializerOptions.PropertyNameCaseInsensitive` 하나로 잡혔다). 리플렉션을 쓰기 때문이다.
"누가 이 필드를 쓰나" 를 `set_` 으로만 세면 이런 모드를 통째로 놓친다.

## 5. 아직 재지 않은 것

- 2단계·3단계 후보가 **실제로** 겹치는지(최종 DB 대조). 참조 신호만 있다.
- ~~`OnLoadOrder.PostLoad` 단계에서 아이템 DB 를 고치면 클라이언트에 그대로 가는지~~
  **쟀다 (2026-10-28): `PostLoad` 는 온다. 그보다 늦으면 오지 않는다.**
  `Caliber9x19PARA` 를 137 로 못박고 `POST /client/items` 로 받아 보니 `PostLoad` 에서는
  38개 중 **37개가 137**, `int.MaxValue` 에서는 **0개**였다. 클라이언트에 보낼 목록이 굳는
  지점이 그 사이에 있다. 1단계를 `StackUnify` 로 만들었다 —
  [../src/StackUnify/README.md](../src/StackUnify/README.md).
  이 과정에서 **"설치 모드의 우선순위가 전부 0" 이라는 전제가 틀렸다는 것도 드러났다**
  (`Injectable` 기본값이 `int.MaxValue` 다. 실측 603개 중 304개가 거기 있다). 자세한 것은
  [REVIEW-LOAD-ORDER.md](REVIEW-LOAD-ORDER.md) 의 2차 정정.
- 플레아 가격·상인 판매 목록처럼 DB 가 아니라 생성 시점에 만들어지는 것들.
