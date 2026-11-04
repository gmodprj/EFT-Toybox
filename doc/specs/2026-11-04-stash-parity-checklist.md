# 스태시 동작 대조표 — 본인 스태시 vs 팀원 편집기

계획 5 의 요구("플레이어 화면에서 되는 것은 하나도 빠짐없이 팀원 화면에서도")를 **증상이 아니라
목록으로** 다루기 위한 표다. 배경과 조사 근거는
[stash-operations-survey](2026-11-04-stash-operations-survey.md).

## 채우는 법

두 화면을 비교한다. **같은 조작을 양쪽에서 해 보는 것**이 전부다.

| 칸 | 무슨 화면인가 | 여는 법 |
|---|---|---|
| **내 스태시** | 지금 로그인한 캐릭터(Nomad) 자신의 인벤토리 화면 | 평소처럼 `I` |
| **팀원 편집기** | 프로필 팀원의 장비를 고치는 우리 모드 화면 | 분대 화면 → 팀원 타일 → 상세 → `팀원 장비 편집` |

**찾는 것은 `내 스태시=O, 팀원 편집기=X` 인 칸이다.** 그게 계획 5 가 말하는 "빠짐없이 동일하게" 의
구멍이고, 한 칸이 곧 작업 하나다.

적는 법:

- `O` 된다 · `X` 안 된다 · `-` 해당 없음(그 아이템이 없어서 못 해 봤다 등)

**전부 채울 필요 없다.** 1부부터 채우고, 막히면 거기서 멈춰도 된다 - 차이 나는 칸 하나가 곧 작업
하나다.

> **내 예상(비고)은 근거가 있을 때만 적었다.** 빈 비고는 "모른다" 는 뜻이지 "될 것이다" 가 아니다.

---

## 1부 — 직접 조작 (메뉴가 아니라 마우스로)

여기가 제일 중요하다. `EItemInfoButton` 열거에 **없는** 것들이라 그동안 빠뜨려 왔다.

| | 조작 | 내 스태시 | 팀원 편집기 | 비고 |
|---|---|---|---|---|
| 1-1 | 드래그로 **옮기기**(칸 → 칸) |O  | O | 관문에서 확인됨(편집기 O) |
| 1-2 | 드래그로 **입히기/벗기기** |O |O | 관문에서 확인됨(편집기 O) |
| 1-3 | **회전**(드래그 중 R 등) |O |O | UIFixes `RotateKeyBind` 가 관여 |
| 1-4 | **분할**(Ctrl+드래그 등) | O| O| 편집기 O (SPT02 실측, 보존 확인) |
| 1-5 | **합치기**(같은 더미에 겹치기) |O |O | 편집기 O (SPT02 실측, 보존 확인) |
| 1-6 | **드래그 복수 선택**(빈 칸에서 끌기) |O |x | **현재 문제.** UIFixes 기능. `SelectionBoxKey` 설정 있음 — **키를 눌러야 하는지 확인 필요** |
| 1-7 | **클릭 복수 선택**(Ctrl+클릭 등) | O| O| UIFixes `EnableMultiClick` |
| 1-8 | **같은 종류 전부 선택** | O| O| UIFixes `SelectAllOfTypeKeyBind` |
| 1-9 | 복수 선택 후 **한꺼번에 옮기기** |O | O| 1-6/1-7 이 돼야 의미 있다 |
| 1-10 | **빠른 이동**(Ctrl+클릭 등으로 반대편으로) |O | O| UIFixes `HighlightQuickMove` |
| 1-11 | **빠른 착용**(더블클릭 등) | O| O| UIFixes `HighlightQuickEquip` |
| 1-12 | **스왑**(자리 맞바꾸기) | O| O| UIFixes `SwapItems`·`SwapMags`. 편집기는 서버 쪽 통과 확인됨 |

## 2부 — 우클릭 메뉴 (자주 쓰는 것)

| | 버튼 | 내 스태시 | 팀원 편집기 | 비고 |
|---|---|---|---|---|
| 2-1 | 살펴보기 `Inspect` / `Examine` | O|O | 편집기 O (관문) |
| 2-2 | **열기** `Open`(탄박스·케이스) |O |O | 편집기 O (탄박스 관문 통과) |
| 2-3 | **탄 빼기** `UnloadAmmo` |O | O| 편집기 O (관문) |
| 2-4 | **삽탄** `LoadAmmo` | O|O | 편집기 O — **2026-11-04 고쳐 확인**. 문맥 게이트였다 |
| 2-5 | **탄창 프리셋** `ApplyMagPreset` |O |O | 둘 다 O — 처음 `x` 는 무기에 꽂힌 탄창으로 시험한 탓이었다(아래 해독 참조) | `LoadAmmo` 와 **같은 게이트**라 함께 열렸을 것 — 확인 필요 |
| 2-6 | `Unload`(탄창 분리) |O |O | 편집기 O (관문) |
| 2-7 | `CheckMagazine`(탄창 확인) |O |O | `Magazine` 검사 있음 |
| 2-8 | `Discard`(버리기) | O|O | |
| 2-9 | `Fold`/`Unfold`(접기) |O | O| `ozen.Foldables` 모드 |
| 2-10 | `TurnOn`/`TurnOff` |O | O| |
| 2-11 | `Tag`/`ResetTag`(태그) |O | O| UIFixes `TagVests`·`TagBackpacks` |
| 2-12 | `AddToFavorites` |O |O | |
| 2-13 | `TopUp`(채우기) | O| x→**우리 경로**| 실행이 `_itemController`·그 팀원의 스태시/장비로만 간다(판정 §2-13~2-17) |
| 2-14 | `Unpack`(개봉) |O |x **설계상 닫는다**| `Session.Unpack(id, **플레이어** 프로필 id)` - 서버로, 그것도 남의 프로필로 나간다 |
| 2-15 | `Install`/`Uninstall` |O |x→**우리 경로**| `QuickFindAppropriatePlace`·`RunWithSound` 둘 다 우리 컨트롤러다 |
| 2-16 | `Modding`(개조 화면) |O |x→**문맥이었다**| 짧은 목록(`InventoryDuringMatching`)에 없던 것이다. 지금 문맥(`Inventory`)에는 있다. 별도 화면의 수명만 확인 |
| 2-17 | `Use`/`UseAll`/`HealAll` |O |x **설계상 닫는다**| `_healthController.ApplyItem` 으로 간다 - **플레이어의 체력**이다. 팀원 물건을 써서 내가 낫는다 |
| 2-18 | `Disassemble` | O| **우리 경로**| `IsActive` 는 `Owner == _itemController`(우리 것이다), 실행은 `DisassembleOperationsList` + 우리 컨트롤러의 `Execute` 로 끝난다 |
| 2-19 | `FilterSearch`/`LinkedSearch` |O | **열었다(2026-11-04)**| `InitSearch` 의 `bool_2` 관문을 건너뛰고 창을 직접 연다. 메뉴의 두 항목은 `RagFairAvailable` 을 따로 보므로 별개다 |
| 2-20 | `SetPin`/`SetLock` |O | x| **복수 선택과 무관**(핀=정렬 고정, 잠금=분실 방지). 편집기에선 버튼이 꺼져 있었다 |

## 3부 — 팀원 편집기에 **없어야** 맞는 것

판단이 필요한 칸이다. "되면 좋다" 가 아니라 **되면 안 되는 것**일 수 있다.

| | 버튼 | 왜 |
|---|---|---|
| 3-1 | `Insure`(보험) | 팀원 프로필의 돈·보험을 건드린다 |
| 3-2 | `AddOffer`(플리 등록) | 플레이어 계정의 거래다 |
| 3-3 | `Repair`(수리) | 설계가 프로필 팀원에게 **거부**하기로 한 조작 |
| 3-4 | `WatchProfile`·`Metrics`·`AddToWishlist` | 플레이어 계정 기능 |
| 3-5 | `ViewMap` | 해롭진 않다 |

## 4부 — 화면 자체

| | 항목 | 내 스태시 | 팀원 편집기 | 비고 |
|---|---|---|---|---|
| 4-1 | 스크롤 위치 기억 | O | **일부러 안 쓴다** | UIFixes 가 `SimpleStashPanel.Show` 를 패치하므로 우리 복제본에도 걸리지만, 기억하는 값이 **플레이어 스태시와 공유하는 static 하나**다. 남의 스태시 위치를 물려받는 것은 틀렸으므로 우리가 맨 위로 되돌린다 |
| 4-2 | 정렬 버튼 | O | **열었다(2026-11-04)** | 진짜 컨트롤러 관문이었다 - `IsAllowedToSort` 가 기본 컨트롤러에서 무조건 false 이고 `ItemManipulator.Sort` 가 **안에서** 그것을 본다. 그 판정 하나만 재정의해 열었다(실행 경로는 그대로 지역이다) |
| 4-3 | 검색 | O | **열었다(2026-11-04)** | 값은 `Unavailable` 그대로 두고 `_searchTab` 을 직접 켠다. 2-19 와 같은 자리 |
| 4-4 | 컨테이너 창 따로 열기 | O | **열린다. 기억은 안 된다** | 여는 것은 `ItemUiContext.OpenItem` 으로 된다(보안 컨테이너만 우리가 막는다). UIFixes 의 저장·복원은 `InventoryScreen.ShowWithTab` 에만 걸려 있어 편집기에는 오지 않는다 - 대신 플레이어가 기억해 둔 창을 **지우지도 않는다** |
| 4-5 | 안내 문구가 머리글과 **겹친다** | - | **고쳤다(2026-11-04)** | 앵커를 직접 주고 칸 여유 36px. 그 뒤 격자 오른쪽 띠·왼쪽 창 밖 튀어나옴까지 `FitStmStashSection` 하나로 합쳤다 |

---

## 채운 뒤에 할 일

1. `내 스태시=O, 팀원 편집기=X` 인 칸만 모은다.
2. **그 X 를 둘로 가른다.** 전부 고칠 거리가 아니다
   ([survey §6](2026-11-04-stash-operations-survey.md), 인계 문서 불변식 11):

   | X 의 종류 | 무엇인가 | 어떻게 하나 |
   |---|---|---|
   | **구멍** | 자리만 바꾸는 조작인데 안 된다 | 고친다. 문맥·배선 문제일 가능성이 높다(삽탄이 그랬다) |
   | **설계** | 컨트롤러의 **실행**이 필요한 조작이다 | **고치지 않는다.** 실행을 막은 것이 안전장치다 |

   가르는 질문: **"이 조작이 서버에 가야 끝나는 일인가?"** 자리만 옮기면 끝나는 것(이동·분할·합치기·
   열기)은 구멍 쪽이고, 컨트롤러가 명령을 실행해야 하는 것은 설계 쪽이다.
3. **고치기 전에 그 기능이 무엇인지 확인한다.** 2026-11-04 의 핀/잠금 오인이 이 단계를 건너뛴 결과였다.
4. 설계 쪽으로 판정된 칸도 **지우지 말고 그렇게 적어 둔다.** 다음 사람이 같은 자리를 다시 파지 않게.

> **"설계" 라고 해서 영영 못 하는 것은 아니다.** 분할·합치기도 한때 그쪽이었고, **최종 상태 제출**
> 이라는 우리 경로로 열었다. 꼭 필요한 조작이면 같은 방법을 따져 본다 - 화면에서는 받기만 하고,
> 결과만 제출하는 것이다.

---

## 채운 결과 (2026-11-04, 사용자 실측)

`내 스태시=O, 팀원 편집기=X` 인 칸 **아홉 개**다(1-1 은 처음 `x` 로 적혔으나 사용자가 **O** 로
정정했다 - 모순이 아니라 오타였다).

**1부에서 안 되는 것은 1-6 하나뿐이다.** 드래그 이동·입히기·벗기기·회전·분할·합치기·복수 선택·
복수 이동·빠른 이동·빠른 착용·스왑이 전부 O 다.

| 칸 | 조작 | 1차 판정 | 왜 |
|---|---|---|---|
| **1-6** | **선택 상자**(빈 칸 드래그) | **구멍** | 1-7·1-8·1-9 가 O 이므로 선택 기구도 복수 이동도 산다. 입력 방식 하나만 안 걸린다 |
| ~~2-5~~ | `ApplyMagPreset` | **구멍 아님 — 닫힘** | 둘 다 O. 조건은 아이템의 자리였다. 삽탄 고침이 같은 문맥 게이트로 이것도 열었다 |
| 2-13 | `TopUp` | 설계 쪽 유력 | 실행이 필요한 조작 |
| 2-14 | `Unpack` | 설계 쪽 유력 | 〃 |
| 2-15 | `Install`/`Uninstall` | 설계 쪽 유력 | 〃 |
| 2-16 | `Modding` | 설계 쪽 유력 | 개조 화면은 별도 화면을 연다 |
| 2-17 | `Use`/`UseAll`/`HealAll` | **설계 판단 먼저** | 팀원에게 소모품을 쓰는 게 맞는 일인지부터 정해야 한다 |
| 2-19 | `FilterSearch`/`LinkedSearch` | **시도했다 실패 — 되돌림** | `StashOnly` 로 바꿔 설치했으나 **검색칸은 안 뜨고 오른쪽에 빈 폭만 생겨 격자가 밀렸다.** 값은 IL 로 들어간 것을 확인했으니 **그 패널이 우리 배치에서 세워지지 않는 문제**다. 다시 열려면 `InitSearch`·`_searchTab` 부터 본다 |
| 2-20 | `SetPin`/`SetLock` | 보류 | 핀/잠금 패널을 되돌려 껐다. 필요하면 다시 판단 |

**가장 큰 소득: 경계가 내가 그은 자리에 없다.** 1-9(복수 선택 후 한꺼번에 옮기기)가 **O** 다.
"컨트롤러의 실행이 필요한 조작은 안 된다" 가 사실이라면 복수 이동도 막혔어야 한다. 막히지 않았다.
불변식 11 의 **원리는 맞지만**(컨트롤러를 바꾸면 서버로 나간다) **어떤 조작이 그 선 너머인지는 아직
정확히 모른다** - 2-13~2-16 을 "설계 유력" 으로만 적어 둔 이유다. 확정 전에는 고치지도, 포기하지도
않는다.

**미확인:** 2-18 `Disassemble`, 4부 전체(스크롤·정렬·검색·컨테이너 창).

---

## 1-6 선택 상자 — 원인과 판단 (2026-11-04)

**왜 이것만 안 되나.** UI Fixes 의 선택 상자는 `DrawMultiSelect`(MonoBehaviour)가 그린다. 그런데
마우스 대상을 EventSystem 전체가 아니라 **자기가 들고 있는 레이캐스터 둘**에서만 찾는다:

```csharp
_localRaycaster     = GetComponentInParent<GraphicRaycaster>();   // 자기가 붙은 캔버스
_preloaderRaycaster = Singleton<PreloaderUI>.Instance … GetComponent<GraphicRaycaster>();
```

그 컴포넌트는 `CommonUI.Awake`·`MenuUI.Awake` 에서 붙는다. 우리 편집기 오버레이는
`UI/InventoryOtherPlayerProfile/pitFireTeam_LoadoutEditorOverlay` 에 있고, 저 두 레이캐스터가
덮지 않으면 **빈 칸 드래그는 애초에 잡히지 않는다.**

**다른 칸이 되는 것과 앞뒤가 맞는다.** Shift+클릭(1-7)·Ctrl+A(1-8)·복수 이동(1-9)은
`ItemView.OnPointerDown`·`GridItemView.OnClick` **컴포넌트 패치**라 캔버스와 무관하다. 그래서
**선택 기구는 살아 있고 입력 경로 하나만 빠졌다.**

**고치려면 드는 값.**
- ⒜ 오버레이를 그 캔버스 아래로 옮긴다 - 지금 잘 도는 화면의 부모를 바꾸는 일이라 회귀 위험이 크다.
- ⒝ `DrawMultiSelect` 를 우리 캔버스에도 붙인다 - **UI Fixes 내부에 의존**하게 된다. 그 모드가
  바뀌면 같이 깨지고, 없는 사람에게는 의미도 없다.

**판단(뒤집혔다 - 이 절 아래 "길 ⒞" 를 보라): 하지 않는다(보류).** 복수 선택 자체는 **Shift+클릭과 Ctrl+A 로 이미 된다.** 얻는 것은
입력 방식 하나이고, 두 길 모두 값이 그보다 크다. 다시 집는다면 ⒜ 쪽이고, 그때는 오버레이의 부모를
바꾸는 것이 다른 것을 깨지 않는지부터 본다.

## 2-5 `ApplyMagPreset` — 조건을 해독했다. 우리 결함이 아닐 수 있다 (2026-11-04)

`ItemContextInteractionsSwitcher.IsActive` 의 그 자리는 이렇다(IL 실측):

```csharp
if (Gameplay) return false;
if (!(_itemContext is InventorySelectableItemContext)) return false;   // 삽탄과 같은 게이트
return MagBuildsStorage.TryFindPresetSource(_item).Succeeded;          // ← 추가 조건
```

**문맥 게이트는 삽탄과 같다** - 2026-11-04 고침으로 이미 통과한다. 내 예상이 빗나간 것은 **조건이
하나 더** 있었기 때문이다.

```csharp
static Option<Item> TryFindPresetSource(Item item)   // static
{
    if (item is Ammo a) return a;
    Magazine mag = (item is Weapon w) ? w.GetCurrentMagazine() : item as Magazine;
    if (mag != null && !(mag is CylinderMagazine)
        && mag.GetRootMergedItem().Parent.Container is Grid)    // ← 핵심
        return mag;
    return Error;
}
```

**조건은 "그 탄창의 뿌리 아이템이 `Grid` 에 들어 있는가" 다.** 격자(스태시 칸·리그 칸·가방 칸)면
되고 **슬롯이면 안 된다.** 실린더 탄창은 아예 제외다.

따라서:

| 탄창이 어디 있나 | 뿌리의 부모 | 뜨는가 |
|---|---|---|
| 스태시 칸에 그냥 놓여 있다 | Grid | **뜬다** |
| 리그·가방 칸에 있다 | Grid | **뜬다** |
| **무기에 꽂혀 있고 그 무기가 장비 슬롯에 있다** | Slot | **안 뜬다** |
| 실린더 탄창 | - | 안 뜬다 |

**이 조건은 프로필·문맥과 무관하고 오직 아이템의 자리에 달렸다.** 즉 본인 스태시에서도 같은 자리면
같은 결과다. 2-5 가 `X` 로 적힌 것은 **우리 편집기의 구멍이 아니라 시험한 탄창의 자리 차이**일 수
있다.

**다시 재는 법:** 팀원 스태시 칸에 **그냥 놓인 탄창**(무기에 꽂히지 않은 것, 실린더 아닌 것)에
우클릭한다. 거기서도 안 뜨면 그때는 진짜 구멍이고, 뜨면 이 칸은 `O` 다.

---

## 2-13~2-17 확정 (2026-11-04, IL 실측 — 게임 없이 끝났다)

"설계 유력, 미확정" 으로 남겨 둔 넷과 "설계 판단 먼저" 인 2-17 을 **실행 경로까지 따라가** 확정했다.
결론부터: **넷 중 셋은 불변식 11 의 선 너머가 아니다.** 진짜로 프로필 경계를 넘는 것은 `Unpack` 하나다.

### 왜 X 였나 — 관문은 컨트롤러가 아니라 **뷰 타입**이다

조작 목록은 `ItemUiContext.CreateInteractions` 가 **`EItemViewType` 으로** 고른다(컨트롤러 종류가
아니다):

| 뷰 타입 | 조작 목록 | TopUp·Install·Unpack | Modding·Use·검색 |
|---|---|---|---|
| `Inventory` 등 | `InventoryItemContextInteractions` | **있다** | **있다** |
| `InventoryDuringMatching` | `MatchingInventoryItemContextInteractions` | **있다** | **없다** |

그리고 `ItemContextInteractionsSwitcher.IsActive` 의 관문은 전부 **구조적**이다 - `Gameplay`
(= `InGameStatus.InRaid`), `ModInstalled`(= 부모가 컨테이너가 아님), `StackMaxSize > 1`. **컨트롤러
종류를 보는 자리가 없다**(그것을 보는 것은 `WatchProfile` 하나뿐: `_item.Owner is OfflineInventoryController`).

**2-16 Modding 의 X 는 뷰 타입이었다.** 장비 트리가 `InventoryDuringMatching` 이던 때 잰 값이고,
그 목록에 `Modding` 이 **아예 없다**. 2026-11-04 삽탄 고침이 문맥을 `Inventory` 로 바꾸면서
2-5(`ApplyMagPreset`)가 열린 것과 **같은 자리**다 - 그때 2-16 을 다시 재지 않았다.

### `PlayerCollections` 는 이름이 거짓말이다

`InstallMod`·`ModWeapon` 이 부품 출처로 받는 `ItemUiContext.PlayerCollections` 는
`_equipment.GetCollections(...).Concat(PlayerStash)` 이고 `PlayerStash => _rightPanelItem` 이다.
둘 다 `Configure` 가 받은 것을 그대로 돌려준다. 우리 편집기는 거기에 **그 팀원의 장비와 그 팀원의
스태시**를 넣었다(`StmGearEditor` 의 `Configure` 호출: 7번째 `rightPanelItems`, 11번째 `equipment`).
그래서 이 경로의 "플레이어" 는 **그 팀원**이다 - 플레이어 아이템이 섞일 자리가 아니다.

### 항목별 확정

| 칸 | 실행 경로(실측) | 확정 |
|---|---|---|
| **2-13 `TopUp`** | `ItemUiContext.TopUpItem` → `_itemController` 로 합치기, 출처는 `_inventory.Stash`·`_equipment`(그 팀원의 것) | **우리 경로다. 열려 있을 것이다** - 코드 작업 없음. `StackMaxSize>1`·`!InRaid`·`HasSourcesForTopUp` 이 서면 메뉴에 선다 |
| **2-14 `Unpack`** | `Session?.Unpack(targetItem.Id, **ClientSession.Profile.Id**)` | **설계상 닫는다.** 서버로 나가고 그것도 **플레이어 프로필 id** 를 싣는다. 열려면 STM 서버 라우트를 따로 만들어 그 팀원 프로필 안에서 풀어야 한다(불변식 11 의 길 ⒜) - 작은 일이 아니다 |
| **2-15 `Install`/`Uninstall`** | `InstallMod` → 적합한 슬롯 찾기 + `QuickFindAppropriatePlace(_itemController)`; `Uninstall` → 같은 함수 + `RunWithSound(_itemController, …)` | **우리 경로다. 열려 있을 것이다** - 코드 작업 없음 |
| **2-16 `Modding`** | `ModWeapon` → `!InRaid` 일 때 `WeaponModdingScreen...(item, **_inventoryController**, PlayerCollections)` | **문맥 문제였고 지금 문맥에는 있다.** 남은 위험은 **별도 화면의 수명**이다: 우리 오버레이 위에 `EScreenState.Queued` 로 떠서, 그 화면이 고친 것이 우리가 제출하는 **같은 지역 트리**인지, 편집기를 닫을 때 그 화면이 남지 않는지 확인해야 한다 |
| **2-17 `Use`/`UseAll`/`HealAll`** | `UseItem`·`UseAll`·`HealAll` → **전부 `_healthController.ApplyItem(...)`** | **설계상 닫는다 — 판단 끝.** 그 체력은 **플레이어의 것**이다. 팀원 물건을 소모해 내가 낫는 길이고, 애초에 편집기는 `healthController: null` 로 문맥을 세우므로 눌리면 그대로 터진다. 설계 §4(프로필에 더하지 않는다)와도 어긋난다 |

**인게임에서 볼 것은 셋뿐이고, 전부 "눌러 보는 것" 이다**(코드 작업 아님): 2-13 은 겹치는 더미가 있는
탄약에 `채우기`, 2-15 는 무기에 꽂힌 부착물에 `분리`/스태시의 부착물에 `장착`, 2-16 은 무기에 `개조`.
2-16 만 눌러 본 뒤 **저장까지** 가 봐야 한다(별도 화면의 수명).

---

## 2-18·4부 확정 (2026-11-04, IL 실측 — 게임 없이 끝났다)

비어 있던 2-18 과 4부 다섯 칸을 채웠다. **4부에서 진짜 컨트롤러 관문은 정렬(4-2) 하나였고, 그것도
열었다.** 나머지는 "이미 된다" 이거나 "일부러 안 쓴다" 이거나 "플레이어 화면에만 있는 기능" 이다.

| 칸 | 실측 | 확정 |
|---|---|---|
| **2-18 `Disassemble`** | `IsActive`: `Owner == _itemController`(편집기 아이템의 주인은 우리 컨트롤러다). 실행: `DisassembleOperationsList` → `_itemController.TryRunNetworkTransaction` → `RunNetworkTransaction` → **우리 컨트롤러의 `Execute`** | **우리 경로.** 부착물을 스태시로 옮기는 자리 바꾸기라 저장 경로가 받는다. 눌러 보기만 하면 된다 |
| **4-1 스크롤 기억** | UIFixes `SyncScrollPositionPatches` 가 `SimpleStashPanel.Show` 에 postfix 로 붙는다 → **우리 복제본에도 걸린다.** 다만 기억하는 값이 `StashScrollPosition` **static 하나**로 플레이어 스태시와 공유다 | **일부러 안 쓴다.** 우리가 `Show` 뒤에 맨 위로 되돌린다(PIT 의 원래 주석 "template may retain a previous screen's offset" 이 가리키던 것이 이것이다). 남의 스태시의 스크롤 위치를 물려받는 것은 틀렸다 |
| **4-2 정렬** | `InitSorting` 이 `_inventoryController.IsAllowedToSort(_item)` 를 보고, **`ItemManipulator.Sort` 가 안에서 또 본다**(아니면 `CannotSortItemError`). 그 판정은 기본 `InventoryController` 에서 `return false` 이고 `BackEndInventoryController` 만 재정의한다 | **열었다.** `StmGearEditInventoryController` 가 그 판정 **하나만** 재정의한다. 컨트롤러의 **종류는 그대로**이므로(불변식 11) `Execute` 는 지역 트리에서 끝난다. 정렬이 하는 일은 자리 바꾸기뿐이라 id 가 보존돼 저장 경로를 통과한다 |
| **4-3 검색** | 2-19 와 같은 자리 | **열었다(같은 날).** 값은 `Unavailable` 그대로, `_searchTab` 을 직접 켠다 |
| **4-4 컨테이너 창** | 여는 것은 `ItemUiContext.OpenItem`(된다). UIFixes 의 저장·복원은 `WindowPatches.InventoryShowPatch` → **`InventoryScreen.ShowWithTab` 에만** 걸린다 | **열린다. 기억은 편집기에 없다.** 그 복원은 `ItemUiContext.Instance.Inventory` 로 아이템을 찾는데 우리 문맥에서는 그게 팀원 것이다 - 다행히 그 경로가 편집기에서는 **아예 돌지 않으므로** 플레이어가 기억해 둔 창 목록을 지우지도 않는다 |
| **4-5 문구 겹침** | - | **고쳤다(같은 날).** 앵커 + 칸 여유, 그 뒤 가장자리 전체를 `FitStmStashSection` 으로 합쳤다 |

**이로써 대조표에 미측정 칸이 없다.** 남은 X 는 전부 "설계상 닫는다"(2-14 `Unpack`, 2-17 `Use` 계열,
3부 전체) 이거나 "보류"(1-6 선택 상자, 2-20 핀/잠금) 다.

---

## 1-6 선택 상자 — 보류를 뒤집었다. 길 ⒞ 가 있었다 (2026-11-04, 같은 날)

**⒝ 를 "모드 내부에 의존한다" 고 본 것이 틀렸다.** UI Fixes 자신이 그 컴포넌트를 **화면마다 붙인다**
(IL 실측): `MultiSelectPatches` 의 `CommonUI.Awake`·`MenuUI.Awake` postfix 가
`GetOrAddComponent<DrawMultiSelect>` 를 인벤토리의 `Items Panel`, 전송 화면 둘, 스캐브 인벤토리,
상인 거래 화면, 은신처 화면 넷에 건다. **우리 편집기는 그 목록에 없는 또 하나의 화면일 뿐이다.**
내부를 흉내 내는 것이 아니라 **같은 방식으로 하나 더 붙이는 것**이고, 캔버스를 옮길 필요가 없으니
⒜ 의 회귀 위험도 사라진다.

**쓰기 경로가 늘지 않는다 — 이 판단의 핵심이다.** `DrawMultiSelect.Update` 가 하는 일은 사각형을 그려
`GridItemView` 를 고르는 것뿐이다(`MultiSelect.Toggle`·`Prune`·`CombineSecondary`). **고른 것을 옮기는
일은 이미 되는 1-7·1-8·1-9 의 경로**(`ItemView.OnPointerDown` 패치)가 한다 - 우리 컨트롤러의 지역
조작이다. 그래서 이 변경은 입력 방식 하나를 더할 뿐 저장·보존 검사에 새 길을 내지 않는다.

**모드 의존은 반사로 없앴다.** `AccessTools.TypeByName("UIFixes.DrawMultiSelect")` 가 `null` 이면
(모드가 없거나 이름이 바뀌었으면) 그냥 넘어간다 - 컴파일 의존도, 없을 때의 오류도 없다.

**붙이기 전에 설 자리를 확인한다.** 그 컴포넌트의 `Start()` 는 레이캐스터 둘(`GetComponentInParent`,
`PreloaderUI` 의 첫 자식) 중 하나라도 못 찾으면 **던지고**, 던지면 `Update` 가 매 프레임 null 을
만진다. 그래서 붙이기 전에 둘을 확인하고 없으면 붙이지 않는다(불변식 2).

**열었다(클라이언트 `BCD4BC1A`). 인게임 미확인** - 빈 칸에서 드래그해 사각형이 그려지고 걸린 아이템이
선택되는지, 선택한 것을 한꺼번에 옮겨 저장이 통과하는지.
