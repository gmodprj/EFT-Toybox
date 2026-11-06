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

**되돌렸다.** 버튼을 켜는 코드와 그 진단을 소스에서 걷어내고 **설치까지 마쳤다**
(클라이언트 `A127DB8A` → `9F955305`, 2026-11-04). 설치본 바이트로 양쪽을 확인했다 - 선택 버튼·진단
문구는 **없고**, 삽탄 고침과 안내 문구 고침은 **남아 있다**. 죽은 버튼 넷은 화면에서 사라졌다.

**덧붙임 — 애초에 증상 판단도 과했다.** 2026-11-04 대조표 실측에서 복수 선택은 **이미 되고 있었다**
(Shift+클릭·Ctrl+A·선택 후 한꺼번에 옮기기 전부 O). 안 되는 것은 **선택 상자(빈 칸 드래그) 하나**다.
"복수 선택이 안 된다" 를 기능 전체로 받아들이고 그 기구를 통째로 쫓은 것이 첫 단추부터 틀렸다.

---

## 6. "플레이어 스태시를 통째로 옮기면 되지 않나" — 실험과 결론 (2026-11-04)

사용자 제안: 조작을 하나씩 따라 열지 말고 **진짜 인벤토리 화면을 그대로 쓰라.** 방향은 옳다 -
지금 구조가 패널을 **복제**한 것이라 모드가 붙은 배선이 떨어져 나가고, 삽탄·복수 선택은 그 한
원인의 증상이다. 그래서 코드를 고치기 전에 **가능한지부터** 쟀다.

### 가능해 보였던 근거

```csharp
InventoryScreen.Show(IHealthController, InventoryController controller, …)
ScavengerInventoryScreenController(OfflineInventoryController scav,
                                   OfflineInventoryController main, …)
```

진짜 화면이 **컨트롤러를 인자로 받고**, EFT 자신도 스캐브 이전에서 **다른 프로필**을 그 화면으로 연다.
전례가 있는 셈이다.

### 막는 것 — 컨트롤러의 종류가 곧 안전장치다

| | 우리 편집기 | EFT 의 진짜 화면들 |
|---|---|---|
| 컨트롤러 | `InventoryController`(기본) | `OfflineInventoryController : BackEndInventoryController` |
| 조작이 가는 곳 | **로컬 트리뿐** | `Execute` → `ToBaseInventoryCommand(_mainProfileId)` → **백엔드 세션** |
| 저장 | 우리가 **최종 상태 한 번** 제출(보존 검사) | 조작마다 서버 |

**`BackEndInventoryController.Execute` 가 조작을 서버로 보낸다**(IL 확인). 우리 편집기를 그 컨트롤러로
바꾸면 드래그마다 **플레이어 인벤토리 조작이 서버로 나가** 엉뚱한 프로필에 쓰인다. 지금 기본
`InventoryController` 를 쓰는 것은 게으름이 아니라 **그 안전장치**다.

### 그래서 빠진 조작의 정체가 설명된다

`ItemContextInteractionsSwitcher.IsActive` 는 곳곳에서 컨트롤러의 **종류**를 본다
(`isinst OfflineInventoryController`, `isinst Player/PlayerInventoryController`). 한 자리는 아예
`controller is OfflineInventoryController` 를 그대로 돌려준다(IL_0497). 우리 컨트롤러는 **둘 다 아니다.**

즉 빠진 것은 무작위 구멍 37개가 아니라 **한 줄로 갈린다**:

- **자리만 바꾸는 조작** — 드래그 이동·입히기·벗기기·분할·합치기·탄박스 열기 → **된다**
  (실행이 필요 없고, 우리가 최종 상태로 제출한다)
- **컨트롤러의 실행이 필요한 조작** — 그리고 그 실행 가능 여부를 타입으로 검사하는 것들 → **안 된다**
  (설계가 그 실행을 일부러 막았으므로)

복수 선택으로 **여러 개를 한꺼번에 옮기는 것**도 이 둘째 줄에 있을 가능성이 높다 - UI Fixes 의 선택
자체는 컴포넌트 패치라 걸리지만, 옮기는 동작은 컨트롤러를 통한다.

### 건널 수 있는 길과 값

1. **그 줄을 받아들인다.** 실행이 필요한 조작은 **설계상 범위 밖**임을 문서에 못박고, 필요한 것만
   우리 저장 경로로 따로 구현한다(분할·합치기를 그렇게 열었다). **값이 가장 싸다.**
2. **세션 스텁을 만든다.** `OfflineInventoryController(IClientSession, Profile, string)` 의 세션을
   **우리가 넘긴다** - 요청을 삼키는 가짜 세션을 주면 타입 검사는 통과하고 서버로는 안 나간다.
   비용: `IClientSession` 은 **메서드 69개·속성 16개**이고, 어느 하나라도 실제로 필요하면 그 자리에서
   깨진다. **위험도 가장 높다**(프로필에 쓰는 경로를 흉내 내는 일이다).
3. **진짜 화면을 쓰되 컨트롤러는 우리 것으로.** `InventoryScreen.Show` 는 `InventoryController` 를
   받으므로 타입상 가능하다. 다만 그러면 **지금과 같은 제약**이 그대로 남는다(실행이 필요한 조작은
   여전히 안 된다). 얻는 것은 화면 쪽 모드 지원뿐이고, 퀘스트·업적 컨트롤러를 무엇으로 넘길지라는
   새 위험이 생긴다(팀원 물건으로 플레이어 퀘스트를 넘기는 복제 경로).

**권고: 1번.** 2번은 안전장치를 흉내 내는 일이라 이 모드의 설계 원칙(의심스러우면 쓰지 않는다)과
어긋나고, 3번은 값에 비해 얻는 것이 적다.
