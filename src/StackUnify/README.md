# StackUnify — 최대 스택 크기를 한 곳에서 정한다

설치본에는 `StackMaxSize` 를 쓰는 서버 모드가 **13개** 있다(실측). 어느 값이 남는지 예측할 수
없어 설정 화면과 실제가 다른 상태가 생겼다 — CobrasAmmoStack 은 31개 구경에 값을 썼지만
**하나도 남지 않았고**, SVM 의 화폐 네 줄도 BarterItemsStacks 에 덮여 있었다.

이 모드는 **클라이언트 목록이 굳기 전의 가장 늦은 자리**에서 설정에 적힌 값으로 못박는다.
중복 작업을 없애는 것이 아니라 **최종값의 주인을 하나로 만드는 것**이다.

## 어느 자리에 설 것인가 — 늦을수록 좋은 것이 아니다

순서는 `Injectable.TypePriority` 로 정해진다. SPT 는 그 값으로 **오름차순** 정렬한다
(`DependencyInjectionHandler.InjectAll`). 그래서 처음에는 가장 큰 값을 쓰면 이긴다고 봤다.
**아니었다.**

같은 값을 두 우선순위에서 써 보고 잡았다(`Caliber9x19PARA` → 137, 실제 값 150).

| 우선순위 | 모드가 본 값 | 모드가 쓴 값 | 클라이언트가 받은 값 |
| --- | --- | --- | --- |
| `OnLoadOrder.PostLoad` (1,000,000) | 150 | 137 | **137 — 38개 중 37개** |
| `int.MaxValue` (2,147,483,647) | **900** | 137 | 150 — 하나도 안 옴 |

둘째 줄의 **900** 이 열쇠다. 클라이언트에 보낼 아이템 목록이 굳는 지점이 중간에 있고,
**목록이 굳은 뒤에도 DB 는 계속 바뀐다.** 그러니 그보다 뒤에 쓰면 DB 만 바뀌고 클라이언트는
못 받는다.

그래서 이 모드는 `OnLoadOrder.PostLoad` 를 쓴다 — 실측으로 확인된, 굳기 전의 가장 늦은 자리다.

### 딸린 발견

`Injectable` 의 **생성자 기본값이 `typePriority = int.MaxValue`** 다.

```
.ctor(InjectionType injectionType = 2, Int32 typePriority = 2147483647)
```

인수 없이 `[Injectable]` 만 붙이면 그 값이 된다. 설치된 서버 모드의 선언 603개 중
**304개가 거기 있다** — 즉 그 모드들은 **DB 를 고쳐도 클라이언트 목록에는 닿지 않는 자리**에서
돈다. 전수는 [../../doc/server-tests/injectable-priority-census.txt](../../doc/server-tests/injectable-priority-census.txt).

## 못 이기는 것 — 화폐

화폐 네 종은 어느 우선순위로도 못 이겼다. 777 로 쓰라고 해도 클라이언트는 50,000,000
(BarterItemsStacks 값)을 받는다.

| 우선순위 | 모드가 본 값 |
| --- | --- |
| `PostLoad` | 10,000,000 |
| `int.MaxValue` | 50,000,000 |

즉 BarterItemsStacks 는 **`PostLoad` 와 목록이 굳는 지점 사이**에 쓴다. 그 사이에 끼어들
방법은 찾지 못했다. **순서로 이길 수 있는 범위가 있고, 그 밖은 순서로 해결되지 않는다.**

탄약은 이긴다. 그것이 이 모드의 쓸모다.

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

### 전수 조사하는 법

구경마다 **아무 아이템도 가지지 않을 값**(예: 999999)을 정해 두고 `ReportOnly: true`·
`Debug: true` 로 띄우면, 모든 항목이 "다르다"가 되어 지금 값이 전부 로그에 찍힌다. 그렇게
재 보니 **구경 44종 중 15종은 한 구경 안에서도 값이 섞여 있었다**(`Caliber9x18PM` 은
150×44·1×17·50×2, `Caliber12g` 는 60×46·20×14 …).

## 설정이 깨지면

**아무것도 바꾸지 않고 오류만 남긴다.** 처음에는 그대로 예외가 올라가 **서버가 죽었다**
(`JsonReaderException`). 설정이 깨졌다고 서버를 죽일 이유가 없어 `try/catch` 로 감쌌다.

`config.jsonc` 는 주석을 쓸 수 있는 JSONC 다. 주석을 **값 뒤에** 붙이면 쉼표가 주석에 먹혀
깨지므로, 주석은 앞 줄에 둔다.

## 빌드와 설치

```
dotnet build -c Release src/StackUnify
pwsh -File tools/install-stackunify.ps1
```

`E:\Spt\SPT_Runtime\user\mods\StackUnify\` 에 들어간다. **서버를 다시 켜야 한다.**
설치 스크립트는 이미 있는 `config.jsonc` 를 덮지 않는다. 서버가 켜져 있으면 DLL 이 잠겨
복사가 실패하므로 먼저 서버를 끈다.

## 아직 안 한 것

- **내구도**(`MaxDurability`·`Durability`)와 **탄 성능**(`Damage`·`PenetrationPower`) 은 다음
  단계다. 같은 틀을 쓰면 된다 — 다만 그 값들도 목록이 굳는 지점 뒤에서 덮이는지 같은
  방식으로 먼저 재야 한다.
- 화폐처럼 **`PostLoad` 뒤에 쓰는 모드**를 이기는 길. 지금은 없다.
- 칸 필터·플레아 금지 같은 **집합 계산형**은 이 틀로 다루지 않는다. 최종 집합을 통째로 다시
  써야 뜻이 분명해지고, 자작 모드 둘(`HelmetSlotUnion`·`SlotCategoryRelax`)과 또 겹친다.
- 봇 AI·재입고 주기 같은 **행동형**은 값이 아니라 코드 경로라 이 방식으로 끌 수 없다.

[../../doc/REVIEW-INTEGRATION-MOD.md](../../doc/REVIEW-INTEGRATION-MOD.md) 에 어디가 겹치는지와
무엇까지 되는지, [../../doc/REVIEW-LOAD-ORDER.md](../../doc/REVIEW-LOAD-ORDER.md) 에 순서 측정이
두 번 틀렸던 경위를 적어 뒀다.
