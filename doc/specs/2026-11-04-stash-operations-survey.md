# 스태시에서 되는 동작은 무엇이고 어디서 오는가 — 실측 조사 (2026-11-04)

계획 5 의 요구는 **"플레이어 자신의 스태시·장비창에서 가능한 동작은 하나도 빠짐없이 동료창에서도"**
다. 그런데 그 "가능한 동작" 의 목록을 **한 번도 만들지 않고** 개별 증상만 쫓아 고쳐 왔다. 그 결과
엉뚱한 기능(핀/잠금)을 복수 선택으로 오인해 쓸모없는 버튼 넷을 설치하는 사고를 냈다(§5).
이 문서는 그 목록을 먼저 만든다.

## 1. 아이템 조작의 전체 어휘 — 48개

`EItemInfoButton`(Assembly-CSharp, Cecil 로 추출). 우클릭 메뉴와 단축키가 모두 이 값들로 돈다.

```
Inspect  Repair  Reload  Load  Unload  Equip  Unequip  Modding
EditBuild  ApplyMagPreset  Use  UseAll  HealAll  Install  Uninstall  Open
ViewMap  UnloadAmmo  LoadAmmo  Examine  CheckMagazine  Insure  Uncover  Overlook
TurnOn  TurnOff  Fold  Unfold  Disassemble  TopUp  Unpack  AddToFavorites
RemoveFromFavorites  AddToWishlist  RemoveFromWishlist  AddOffer  FilterSearch  LinkedSearch
NeededSearch  Metrics  Discard  Tag  ResetTag  WatchProfile
SetPin  SetLock  SetUnPin  SetUnLock
```

**이 48개가 "조작" 의 전부는 아니다.** 드래그로 옮기기·분할·합치기·회전, 그리고 **복수 선택**은 이
열거에 없다 - 메뉴 항목이 아니라 그리드/아이템 뷰의 입력 처리다. 그래서 어휘를 둘로 나눠 봐야 한다:
**메뉴 조작(48개)** 과 **직접 조작(드래그 계열)**.

## 2. 그 동작들이 전부 바닐라인 것은 아니다 — 설치본 실측

`E:\Spt\BepInEx\plugins` 전수 조사: 스태시·아이템 UI 타입(`SimpleStashPanel`·`GridView`·`ItemView`·
`ItemUiContext`·`BaseItemContextInteractions`·`EItemInfoButton`·`ItemContextInteractionsSwitcher`·
`InventoryScreen`)을 **둘 이상 참조하는 플러그인이 32개**다. 상위는 이렇다:

| 플러그인 | 참조 | 비고 |
|---|---|---|
| **Tyfon.UIFixes** | **8개 전부** | 설정 항목 **139개**. 스태시 경험의 상당수가 이 모드다 |
| ozen.ContinuousLoadAmmo | 6 | 탄 연속 장전, `MultiSelectInterop` 으로 UIFixes 에 붙는다 |
| ozen.Foldables | 6 | 접기, 역시 `MultiSelectInterop` |
| **SptTeamManager** | 6 | 우리 모드 |
| QuickSell / QuickSellFlea | 5 / 4 | 빠른 판매, `MultiSelect` 연동 |
| AdvancedStashSorting | 4 | 정렬 |
| Tyfon.Discarder / OpenBarters | 2 / 3 | 버리기·물물교환, `MultiSelect` 연동 |

> **교훈.** "플레이어 스태시에서 되는 것" 을 바닐라 어셈블리만 보고 판정하면 틀린다. 2026-11-04
> 오전에 아이템 **데이터**에 대해 같은 교훈을 적어 놓고(`정적 items.json 은 런타임 DB 가 아니다`)
> **UI 동작에는 같은 질문을 하지 않았다.**

### 복수 선택은 UI Fixes 가 준다

바닐라 어셈블리에는 `MultiSelect` 라는 이름이 **타입·필드·메서드 어디에도 없다**(Cecil 전수 검색).
드래그 복수 선택은 `Tyfon.UIFixes` 의 기능이다:

```
UIFixes.MultiSelect, MultiSelectController, MultiSelectPatches, MultiSelectStrategy,
MultiSelectItemContext, MultiSelectExtensions, DrawMultiSelect, UIFixesInterop.MultiSelect
```

관련 설정: `EnableMultiSelect`, `EnableMultiSelectInRaid`, `SelectionBoxKey`, `EnableMultiClick`,
`SelectAllOfTypeKeyBind`, `MultiSelectStrat`, `MultiQuickMoveMerge`, `ShowMultiSelectDebug`.

## 3. 무엇이 우리 편집기에 적용되고 무엇이 안 되는가 — 가르는 규칙

우리 편집기는 **EFT 의 패널을 복제**하고 **자기 `ItemUiContext` 를 Configure** 한다. 그래서 외부
모드의 적용 여부는 그 모드가 **무엇을 보고 켜지는가**로 갈린다.

| 켜지는 조건 | 우리 편집기에 | 증거 |
|---|---|---|
| **컴포넌트 타입**을 패치(ItemView·GridView…) | **적용된다** - 우리도 같은 컴포넌트를 쓴다 | 아래 UIFixes 패치 목록 |
| **아이템 문맥의 종류**를 본다 | **안 된다** - 우리가 넘기는 문맥이 다르면 걸러진다 | `LoadAmmo` 가 `InventorySelectableItemContext` 를 요구(2026-11-03·11-04) |
| **화면/스크린**을 특정 | 안 된다 | - |

**`삽탄` 은 둘째 줄이었다.** 그래서 문맥을 바꿔 고쳐졌다(2026-11-04, 인게임 확인).

**복수 선택은 첫째 줄이다 — 그래서 이번 추론이 틀렸다.** `MultiSelect.Enabled` 의 IL 은 이렇다:

```csharp
Enabled => Settings.EnableMultiSelect.Value
           && (!Plugin.InRaid() || Settings.EnableMultiSelectInRaid.Value)
```

**화면·문맥 검사가 없다.** 패치 대상도 컴포넌트 단위다 - `ItemView.OnPointerDown`,
`GridItemView.OnClick`, `ItemView.OnBeginDrag/OnEndDrag`, `ItemView.Init/Kill`,
`GridView.CanAccept/AcceptItem/GetTargetItem`, `BaseItemContextInteractions.ExecuteInteractionInternal`
등 40개. 초기화는 `CommonUI.Awake`·`MenuUI.Awake` 에서 한 번 한다.

> **따라서 "우리 화면이라 안 된다" 는 설명은 성립하지 않는다.** 원인은 더 좁은 데 있다. 후보:
> ① 우리 아이템 뷰가 `GridItemView` 가 아니거나 ② `MultiSelectStrategy`/`MultiSelectExtensions` 가
> 아이템 **문맥**을 보거나 ③ 선택 자체는 되는데 **그 다음 동작**(이동)이 우리 컨트롤러에서 막히거나
> ④ `SelectionBoxKey` 를 눌러야 하는데 그 조작을 안 했거나. **넷 다 확인하지 않았다.**

## 4. 그래서 다음 계획은 "증상" 이 아니라 "목록" 에서 출발한다

> **표는 따로 있다**: [stash-parity-checklist](2026-11-04-stash-parity-checklist.md) -
> 직접 조작 12칸·메뉴 20칸·판단이 필요한 5칸·화면 5칸. 아는 칸은 미리 채워 두었다.

1. **목록을 만든다.** §1 의 48개 + 직접 조작(드래그 이동·분할·합치기·회전·복수 선택)에 대해
   **플레이어 자기 스태시에서 되는가 / 편집기에서 되는가** 를 한 칸씩 채운다. 사용자가 둘을 나란히
   열어 보는 것이 가장 싸고 정확하다.
2. **차이 나는 칸만** 원인을 판정한다. 판정은 §3 의 표로 시작한다 - 컴포넌트 패치면 우리 쪽 구조를,
   문맥 게이트면 문맥을 본다.
3. **고치기 전에 그 기능이 무엇인지 먼저 확인한다.** 이번 사고의 직접 원인이다(§5).

## 5. 이번에 틀린 것 — 기록으로 남긴다

**핀/잠금을 복수 선택으로 오인했다.** `SimpleStashPanel` 이 `SelectableItemContext.ISelectionContext`
를 구현하고 그 `IsActive` 가 `_pinLockSelectionMode` 로 켜지는 것을 보고 **"스태시의 복수 선택 = 이것"**
이라고 단정했다. 그 단정 위에 진단을 두 번 쌓고, `BottomLeftPanel` 을 켜는 코드를 **설치까지** 했다.
결과는 **동작하지 않는 버튼 넷**(Priority·Pin·Lock·SortButton)이 화면에 보이는 것이고, 고치기 전보다
나쁜 상태다.

실제 핀/잠금의 뜻(사용자 제공 자료로 확인):
- **핀(P)**: 자동 정렬 때 위치가 안 바뀐다. 수동 이동은 된다.
- **잠금(L)**: 버리기·판매·제작 재료 사용 등을 막는다.

**둘 다 복수 선택과 무관하다.**

**무엇을 했어야 했나.** 설치된 플러그인을 한 번 뒤지는 것(이 문서 §2, 명령 한 줄)이 **첫 단계**여야
했다. 그러면 `Tyfon.UIFixes` 가 바로 나왔고 핀/잠금 쪽으로 가지 않았다. 바닐라 어셈블리에
`MultiSelect` 가 없다는 사실도 그때 확인됐을 것이다.

**되돌릴 것.** 버튼을 켜는 코드와 그 진단은 소스에서 걷어냈다(빌드 `9F955305`). **설치본
`A127DB8A` 에는 아직 들어 있으므로 다음 설치에서 사라진다.**
