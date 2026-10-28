# StackUnify — 최대 스택 크기를 한 곳에서 정한다

설치본에는 `StackMaxSize` 를 쓰는 서버 모드가 **13개** 있다(실측). 어느 값이 남는지 예측할 수
없어, 설정 화면과 실제가 다른 상태가 생겼다 — CobrasAmmoStack 은 31개 구경에 값을 썼지만
**하나도 남지 않았고**, SVM 의 화폐 네 줄도 BarterItemsStacks 에 덮여 있었다.

이 모드는 **모든 모드 뒤에 돌아** 설정에 적힌 값으로 못박는다. 중복 작업을 없애는 것이 아니라
**최종값의 주인을 하나로 만드는 것**이다.

## 맨 뒤에 서는 방법

`Injectable.TypePriority` 다. SPT 서버 코드를 읽어 확인했다.

| 읽은 곳 | 무엇이 나왔나 |
| --- | --- |
| `DependencyInjectionHandler::InjectAll` | `OrderBy(TypePriority)` — **오름차순** |
| `ProgramExtensions::RunPreSptLoadCallbacks` | `0 <= 우선순위 < 200000` 인 `IOnLoad` 만 |
| `SPTStartupHostedService::StartAsync` | `우선순위 >= 200000` 인 `IOnLoad` 만 |
| `SPTarkov.Server.Core.DI.OnLoadOrder` (public) | `Watermark` 0 … `PostLoad` 1000000 |

설치된 서버 모드 113개의 `[Injectable]` 선언은 **전부 0** 이므로,
`[Injectable(InjectionType.Singleton, OnLoadOrder.PostLoad)]` 하나로 전부 뒤에 선다.

## 확인한 것 — PostLoad 는 클라이언트에 닿는다

만들기 전에 이것부터 재야 했다. SPT 가 아이템 목록을 미리 굳혀 두면 PostLoad 는 늦기 때문이다.

`Caliber9x19PARA` 를 **137**(실제 값 150 과 다른 눈에 띄는 수)로 못박고 서버를 띄운 뒤,
클라이언트가 쓰는 것과 같은 길(`POST /client/items`)로 최종 목록을 받아 봤다.

| | 결과 |
| --- | --- |
| 모드가 바꾼 것 | 38개 (`150 -> 137`, 로그로 확인) |
| 클라이언트가 받은 값 | **137 이 37개**, 150 이 1개 |
| 건드리지 않은 구경 | 그대로 (`Caliber556x45NATO` 180) |

**PostLoad 단계의 수정은 클라이언트까지 온다.** 늦지 않다.

### 남은 1개는 설명하지 못했다

`[Aishi]_(9x19mm RIP AP)` (`699aae8c9b6a4ab2b769c4a3`) 하나만 150 으로 남았다. 로그에는
`150 -> 137` 로 바꾼 줄이 분명히 있고, 서버를 그대로 둔 채 두 번 물어봐도 150 이었다. 즉
**그 아이템은 다른 자리에서 나온다.** 그 id 를 들고 있는 모드가 몇 개 더 있다(Amonya,
BlueheadsAIOTrader, ISB-Aishi 의 assort·base). 다만 그 모드들은 전부 우선순위 0 이라 이
모드보다 **먼저** 도는데도 결과가 그렇다 — 표가 둘이거나, 응답이 다른 사본에서 만들어지는
것으로 보인다. **아직 가리지 않았다.**

다음에 재려면: 이 모드에서 그 아이템 객체의 신원(해시)을 찍어 두고, 응답을 만드는 자리에서
같은 객체를 보는지 대조한다.

## 설정

기본 설정은 **비어 있어 아무 일도 하지 않는다.** 적지 않은 것은 건드리지 않는다 — 다른 모드의
의도를 통째로 지우지 않기 위해서다.

| 항목 | 기본값 | 내용 |
| --- | --- | --- |
| `ModEnabled` | `true` | 끄면 데이터베이스를 건드리지 않는다 |
| `Debug` | `false` | 바꾼 항목을 한 줄씩 기록한다. 수백 줄이 될 수 있다 |
| `ReportOnly` | `false` | **바꾸지 않고 "바꿀 것"만 기록한다.** 겹침을 먼저 볼 때 쓴다 |
| `ReportOverridden` | `true` | 내가 정한 값과 이미 있던 값이 다른 항목 수를 요약에 적는다 |
| `StackByCaliber` | `{}` | 구경 이름 → 최대 스택 (탄약) |
| `StackByItemId` | `{}` | 아이템 id → 최대 스택 (구경 규칙보다 우선) |
| `ExcludedItemIds` | `[]` | 구경 규칙에 걸려도 건너뛸 아이템 |

### 쓰는 순서

1. 고치고 싶은 구경을 `StackByCaliber` 에 적고 **`ReportOnly: true`** 로 서버를 띄운다.
2. 로그의 `그중 N개는 다른 모드가 이미 다른 값을 넣어 둔 자리였다` 를 본다. 그 수가 곧
   **겹침의 크기**다.
3. 값이 마음에 들면 `ReportOnly` 를 끈다.

구경 이름은 게임 값 그대로다(`Caliber9x19PARA`·`Caliber556x45NATO`·`Caliber12g` …).
`config.jsonc` 주석에 이 설치본에서 쓰이는 이름을 범주별로 적어 뒀다.

## 빌드와 설치

```
dotnet build -c Release src/StackUnify
pwsh -File tools/install-stackunify.ps1
```

`E:\Spt\SPT_Runtime\user\mods\StackUnify\` 에 들어간다. **서버를 다시 켜야 한다.**
설치 스크립트는 이미 있는 `config.jsonc` 를 덮지 않는다.

## 아직 안 한 것

- **내구도**(`MaxDurability`·`Durability`)와 **탄 성능**(`Damage`·`PenetrationPower`) 은 다음
  단계다. 같은 틀을 쓰면 된다.
- 칸 필터·플레아 금지 같은 **집합 계산형**은 이 틀로 다루지 않는다. 최종 집합을 통째로 다시
  써야 뜻이 분명해지고, 자작 모드 둘(`HelmetSlotUnion`·`SlotCategoryRelax`)과 또 겹친다.
- 봇 AI·재입고 주기 같은 **행동형**은 값이 아니라 코드 경로라 이 방식으로 끌 수 없다.

[../../doc/REVIEW-INTEGRATION-MOD.md](../../doc/REVIEW-INTEGRATION-MOD.md) 에 어디가 겹치는지와
무엇까지 되는지 적어 뒀다.
