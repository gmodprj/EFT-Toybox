# 설계 — 야시경·페이스실드를 N 하나로 갈라 여닫기 (GogglesWheel)

- 날짜: 2026-10-30
- 상태: 설계 승인 대기. 아직 코드 없음.
- 대상: `E:\Spt` (SPT 4.1.6, EFT 0.16.9.5.40743)
- 1차 설계의 전제 다섯 개가 리뷰에서 뒤집혀 §5·§6 을 고쳐 적었다.

## 1. 왜 만드는가

헤드기어 장착 제한을 푸는 모드를 넣어 **야시경과 페이스실드를 헬멧에 동시에** 달 수 있게 됐다. 그런데 둘을 여닫는 키가 N 하나뿐이라 **한쪽만 열리고 다른 쪽은 영원히 못 연다.**

원인은 게임 코드에 그대로 있다.

```csharp
public void ToggleGoggles() {
    if (Equipment.GetSlot(EquipmentSlot.Headwear).ContainedItem is CompoundItem thisItem) {
        TogglableComponent c = thisItem.GetItemComponentsInChildren<TogglableComponent>().FirstOrDefault();
        if (c != null) InventoryController.TryRunNetworkTransaction(c.Set(!c.On, simulate: true));
    }
}
```

`FirstOrDefault()` — 여닫이 중 **첫 번째 하나만** 건드린다. 바닐라에서는 둘이 동시에 달릴 수 없어 드러나지 않던 구멍이고, 제한을 푸는 순간 나타난다.

## 2. 목표

- **짧게 N** — 야시경을 여닫는다. 가장 자주 쓰는 것이라 손이 빠른 쪽을 준다.
- **길게 N** — 원형 메뉴가 뜨고, **마우스를 기울여 고르고 N을 떼면** 그 항목을 여닫는다.
- 휠 항목은 헬멧에 실제로 달려 있는 여닫이에서 **자동으로** 만든다.
- 생김새는 가능하면 게임 제스처 휠과 같게 간다.
- 휠이 떠 있는 동안 **시점이 돌지 않는다. 다만 이동은 멈추지 않는다.**
- **이미 같은 메서드를 가로채는 모드들과 부딪히지 않는다**(UI Fixes 나침반, COTI 수정키).

## 3. 비목표

- 전술 라이트(DBAL-PL 등)는 건드리지 않는다. `LightComponent` 라 N이 아니라 헤드라이트 키가 따로 담당한다.
- 얼굴 칸(FaceCover)의 Ghost Mask 류는 대상이 아니다. 바닐라 `ToggleGoggles` 가 **헬멧 칸만** 본다.
- 장비 제한을 푸는 일은 하지 않는다(이미 다른 모드가 한다).
- 키 바인딩 설정 화면은 건드리지 않는다.
- 하이드아웃은 대상이 아니다 — 거기는 `HideoutPlayer.ToggleNightVision()` 으로 경로가 다르다.

## 4. 안티골 — 이렇게 되면 실패다

- **UI Fixes 의 나침반 치우기를 깨뜨리는 것**, **COTI 의 수정키를 빼앗는 것.**
- 휠이 뜬 동안 **시점이 홱 도는 것**, 또는 **제자리에 얼어붙는 것.**
- 짧게 눌렀는데 **지연이 느껴지는 것.**
- 헬멧에 여닫이가 하나뿐인데 휠이 떠서 손이 늦어지는 것.
- 휠이 뜬 채 레이드가 끝나거나 죽어서 **입력이 잠긴 채 남는 것.**
- 제스처 휠이 망가지는 것.

## 5. 제약 — 실측으로 확인한 것

### 5-1. 1차 설계가 틀렸던 것 (리뷰에서 역컴파일로 확인)

| 틀린 전제 | 실제 |
| --- | --- |
| "Harmony prefix 하나로 통째로 가져올 수 있다" | **이미 두 모드가 같은 메서드를 prefix 한다.** `UIFixes.CompassGogglesPatch` 는 왼손 나침반을 치우고 `return false` 한 뒤 **애니메이션이 끝난 다음 `ToggleGoggles()` 를 다시 부른다**. `Coti.Client.Patches.GoggleToggleSuppressPatch` 는 자기 수정키(`CotiPowerToggle.ModifierHeld`)가 눌려 있으면 `return false` 로 억제한다 — COTI 는 이미 "수정키+N" 을 쓴다 |
| "제스처 휠은 마우스 방향으로 고른다" | **커서 호버로 고른다.** `ShouldLockCursor() => ShowCursor`, `CloseWithSelected()` 는 `_allItems.FirstOrDefault(x => x.IsUnderPointer)`. 방향 방식을 쓰려면 하이라이트를 우리가 직접 몰아줘야 한다 |
| "바닐라와 같은 수단으로 시점만 막는다" | `GesturesMenu.TranslateAxes` 가 **축 배열을 통째로 null** 로 만든다. `EAxis` 는 MoveX·MoveY·TurnX·TurnY·LookX·LookY·LeanX 전부라 **이동·린까지 멈춘다**(제스처 휠이 실제로 그렇다) |
| "셋 다 공통 인터페이스로 묶인다" | `NightVisionComponent`·`ThermalVisionComponent` 만 `ITogglableComponentContainer<TogglableComponent>` 를 구현한다. `FaceShieldComponent.Togglable` 은 `[CanBeNull]` 필드다. **아이템 클래스(`NightVision`·`ThermalVision`·`ArmoredEquipment`)로 가르는 쪽이 깔끔하다** |
| "헬멧의 **자식**에서 모은다" | `GetItemComponentsInChildren<T>` 는 `GetAllItems()` 기반이라 **헬멧 자신도 포함**한다. `ArmoredEquipment` 는 `template.HasHinge` 면 페이스실드 없이도 `TogglableComponent` 를 가진다 — 힌지 헬멧에서는 지금 N이 **헬멧 바이저 자신**을 여닫고 있었을 수 있다 |

### 5-2. 맞은 것으로 확인된 것

| 사실 | 뜻 |
| --- | --- |
| `Player.ToggleGoggles()` public 비가상. 호출처는 어셈블리 전체에서 `MoveInputTranslator.TranslateCommand` **한 곳뿐** | 패치 지점이 하나다 |
| `EGameKey.ToggleGoggles`(36) → `ECommand.ToggleGoggles`, 조합은 `EPressType.Press` → **키 Down 프레임에** 명령이 나간다 | prefix 가 곧 key-down 이벤트라는 전제가 맞다 |
| `IGameKey.Press` 가 public, `InputKey.Update()` 가 `Input.GetKey` 로 매 프레임 갱신 | N에 실제로 묶인 키 상태를 읽는다. 물리 키코드를 몰라도 되고 키를 바꿔도 따라간다 |
| 조합 목록은 `InputPreset._inputKeyCombinations`(public), 다만 `InputManager._currentPreset` 이 **private static** | 리플렉션 한 번 필요 |
| `GesturesMenu : UIInputNode`, `_gestureItemTemplate`(public)을 `Instantiate` → `_gestureContainer` | 항목 템플릿 복제가 된다. 단 레이드 전투 UI가 올라온 뒤에만 있다 |
| `GesturesMenuItem.Show()` 는 제스처 전용(`Gesture.ToString().Localized()`, static `OnBindUpdated` 구독) | `Show()` 를 부르지 말고 라벨만 직접 넣는다 |
| 지금 헬멧의 자식: `mod_nvg` GPNVG-18, `mod_equipment_000` LShZ-2DTM, `mod_mount` DBAL-PL(라이트) | N의 후보는 둘 + 힌지면 헬멧 자신 |

## 6. 어떻게 — 정한 것

### 6-1. 형태

새 플러그인 하나. 소스 `src/GogglesWheel`, 설치 `BepInEx\plugins\GogglesWheel\`(DLL + `config.jsonc`), 설치·제거 `tools/install-goggleswheel.ps1`(`-Remove`). 저장소의 기존 자작 플러그인과 같은 틀(net472, `SptDir` 덮어쓰기, 한국어 주석·로그).

### 6-2. 키 판정과 남의 패치와의 공존

`Player.ToggleGoggles` prefix. **다음 두 경우에는 우리가 물러난다**(`return true` — 남의 패치와 바닐라가 하던 대로 돌게 둔다).

1. **COTI 수정키가 눌려 있을 때.** 남의 입력을 빼앗지 않는다. COTI 가 없으면 이 검사는 건너뛴다(소프트 의존, 리플렉션으로 찾는다).
2. **우리 prefix 가 도는 순간 N 키가 눌려 있지 않을 때.** 이것은 사람이 누른 것이 아니라 **코드가 부른 것**이다 — UI Fixes 가 나침반 애니메이션 뒤에 되부르는 경우가 정확히 여기다. 이때 휠을 띄우면 떼어질 일 없는 키를 기다리며 입력이 잠긴다. 그래서 휠을 띄우지 않고, 아래 6-3의 기본 대상을 **즉시** 여닫은 뒤 `return false` 한다.

그 밖에는 우리가 가져간다(`return false`).

1. 여닫이 목록을 모은다(6-3). **비었으면** 아무 것도 하지 않는다.
2. **하나뿐이면** 바로 그것을 여닫는다 — 휠 없음, 지연 없음.
3. 둘 이상이면 `IGameKey.Press` 를 보며 기다린다.
   - 기준 시간(기본 0.25초) 전에 떼면 → **짧게**: 기본 대상을 여닫는다. **토글은 키를 뗄 때 일어난다**(사용자 결정). 그래서 휠에는 야시경도 올라간다.
   - 기준 시간을 넘기면 → **길게**: 휠을 띄운다. N을 떼는 순간 가리키던 항목을 여닫고 닫는다.

Harmony 우선순위는 **낮게** 둔다(남의 가드가 먼저 돌게). 한 prefix 가 `false` 를 돌려줬을 때 나머지 prefix 가 어떻게 되는지는 빌더가 Harmony 동작을 확인해 맞춘다(§9).

### 6-3. 대상 목록과 기본 대상

`EquipmentSlot.Headwear` 의 아이템과 **그 자신을 포함한** 모든 하위에서 `TogglableComponent` 를 가진 것을 모은다. 분류는 아이템 클래스로 한다.

| 아이템 클래스 | 이름 | 순서 |
| --- | --- | --- |
| `NightVision` | 야시경 | 1 (기본 대상) |
| `ThermalVision` | 열화상 | 2 |
| `ArmoredEquipment` 중 `FaceShieldComponent` 를 가진 것 | 페이스실드 | 3 |
| `ArmoredEquipment` 중 힌지(`HasHinge`)로 여닫이가 생긴 헬멧 자신 | 바이저 | 4 |
| 그 밖 | 아이템 이름 그대로 | 5 |

**힌지 헬멧의 바이저도 휠에 넣는다** — 지금 N으로 되던 것을 없애면 기능이 줄어든다. 다만 그 헬멧에 야시경만 더 달면 항목이 둘이 되어 휠이 뜬다(의도한 동작).

기본 대상은 **야시경**, 없으면 목록의 첫 번째. 이름은 게임 locale 의 아이템 이름을 쓰고, 못 읽으면 위 한국어 이름을 쓴다.

### 6-4. 휠

- 레이드의 `GesturesMenu._gestureItemTemplate` 을 **복제**해 우리 캔버스에 담는다. **제스처 휠 자체는 건드리지 않는다.** 복제한 항목은 `Show()` 를 부르지 않고 라벨만 직접 넣는다(그 메서드는 제스처 전용이고 static 이벤트를 구독한다).
- 템플릿을 못 찾으면 우리가 그린 단순한 원형으로 떨어진다. 사라지지 않는다.
- **선택은 마우스 방향이다**(사용자 결정). 휠이 뜬 동안의 마우스 이동을 누적해 방향 벡터를 만들고 가장 가까운 항목을 밝힌다. 커서는 띄우지 않는다 — 바닐라의 호버 방식과 다르므로 **하이라이트는 우리가 직접 몰아준다**(`CloseWithSelected` 는 쓰지 않는다).
- 중앙(기본 0.2 미만)이면 **아무 것도 고르지 않음**. 그대로 떼면 아무 일도 없다.
- 항목은 2개면 좌·우, 3개 이상이면 균등 분할.
- **시점만 막고 이동은 남긴다.** 바닐라처럼 축 배열을 통째로 null 로 두면 이동·린까지 멈춘다 — 교전 중에 쓰는 동작이라 그건 안 된다. Look/Turn 축만 0 으로 눌러 쓴다.
- 닫히는 조건: N을 뗀다 / 레이드가 끝난다 / 플레이어가 죽는다 / 화면이 바뀐다. **어느 경우든 축 차단을 반드시 되돌린다.**

### 6-5. 설정 (`config.jsonc`)

| 항목 | 기본값 | 뜻 |
| --- | --- | --- |
| `enabled` | true | false 면 패치를 걸지 않는다 |
| `longPressSeconds` | 0.25 | 길게로 치는 기준 |
| `defaultTarget` | `"nightvision"` | 짧게 눌렀을 때 — `nightvision`·`faceshield`·`first` |
| `wheelDeadZone` | 0.2 | 이 아래로 기울이면 고르지 않음 |
| `useGameWheelSkin` | true | 제스처 휠 항목 템플릿을 복제해 쓸지 |
| `includeHelmetVisor` | true | 힌지 헬멧 자신을 항목에 넣을지 |
| `standDownForCoti` | true | COTI 수정키가 눌린 N을 COTI 에 넘길지 |

F12 에는 **켜기/끄기 하나만** 둔다.

### 6-6. 안전장치 — 닫힌 쪽으로 넘어뜨린다

- `Player.ToggleGoggles` 를 못 찾으면 패치를 걸지 않고 로그 한 줄만 남긴다(바닐라 그대로).
- 휠을 띄우다 예외가 나면 **그 자리에서 기본 대상을 여닫고** 휠을 접는다. 손이 묶이는 것보다 낫다.
- 축 차단은 `try/finally` 로 되돌린다. 어떤 경로로 끝나도 남지 않게 한다.
- 레이드 밖에서는 아무 것도 하지 않는다.

## 7. 확인 방법

1. 헬멧에 야시경·페이스실드 둘 다 달고 레이드 진입.
2. **짧게 N** — 야시경만 여닫힌다.
3. **길게 N** — 휠이 뜨고, 마우스를 기울이면 해당 항목이 밝아지고, 떼면 그것이 여닫힌다.
4. 휠이 떠 있는 동안 **시점은 안 돌고 이동은 된다.** 떼면 바로 정상.
5. 중앙에 둔 채 떼면 아무 일도 없다.
6. 야시경만 달린(힌지 없는) 헬멧 — N이 즉시 여닫고 휠이 뜨지 않는다.
7. 휠을 띄운 채 죽거나 레이드를 끝내도 입력이 잠기지 않는다.
8. **왼손에 나침반을 들고 N** — UI Fixes 가 나침반을 치우고, 그 뒤 야시경이 여닫힌다(휠이 뜨지 않는다).
9. **COTI 수정키 + N** — COTI 동작만 일어나고 우리 휠은 뜨지 않는다.
10. 제스처 휠이 멀쩡히 돈다.
11. `LogOutput.log` 에 예외 0건.

## 8. 제 판단으로 정한 것 (사용자가 고르지 않았다)

| 정한 것 | 왜 |
| --- | --- |
| COTI 수정키가 눌린 N은 COTI 에 넘긴다 | 남의 모드가 먼저 쓰던 입력이다 |
| 키가 눌려 있지 않은 호출은 즉시 토글(휠 없음) | UI Fixes 의 되부름이 여기다. 휠을 띄우면 입력이 잠긴다 |
| 이동은 막지 않고 시점만 막는다 | 교전 중에 쓰는 동작이다. 바닐라 수단을 그대로 못 써서 손이 더 가지만 그만한 값이 있다 |
| 힌지 헬멧의 바이저도 항목에 넣는다 | 지금 되던 것을 없애지 않는다. 끄려면 설정으로 |
| 기준 시간 0.25초 | 짧게가 둔해지지 않는 선 |
| 중앙은 "고르지 않음" | 마음이 바뀌었을 때 빠져나갈 길 |
| 휠에 "둘 다" 항목은 넣지 않는다 | 필요하다는 말이 없었다. 나중에 설정으로 |

## 9. 빌더에게 남긴 것

- `InputManager._currentPreset`(private static) → `InputPreset._inputKeyCombinations` → `EGameKey.ToggleGoggles` 조합 → `IGameKey` 를 꺼내는 코드.
- 한 prefix 가 `false` 를 돌려준 뒤 나머지 prefix 가 실행되는지 — Harmony 동작을 확인해 우선순위와 가드를 맞춘다. 우리 패치가 남의 가드를 건너뛰게 해서는 안 된다.
- Look/Turn 축만 눌러 막는 구체적 수단과 삽입 위치(`EAxis` 인덱스, 입력 트리 순서).
- 휠 캔버스의 레이어·스케일, 항목 배치 각도, 복제한 항목의 하이라이트를 미는 방법.
- 아이콘은 **넣지 않는다**(글자만). 제스처 스프라이트에는 야시경·페이스실드에 맞는 그림이 없다. 나중에 PNG 를 넣을 자리만 비워 둔다.
- 순수 로직(여닫이 목록 만들기·각도 → 항목 고르기·짧게/길게 판정)은 게임 어셈블리를 모르는 파일로 떼어 내 xunit 으로 시험한다(`CompatToggle.Tests` 와 같은 틀).

## 10. 같은 일을 하는 설치된 모드 — `Tylevo.FieldAttachments` 실측

인게임 2차 확인에서 휠은 떴지만 **고른 것이 움직이지 않았다**. 추측으로 고치지 않고,
설치된 모드 중 레이드 중에 선택 UI 를 띄우고 시점을 막는 것을 찾아 뜯어 봤다.
`grep -rail TranslateAxes --include=*.dll` 로 네 개가 걸렸고, 그 중
`Tylevo.FieldAttachments` 가 십자·육각 선택판을 띄우는 바로 그 구조였다.

역컴파일로 확인한 것(`ilspycmd -p`):

| 그쪽이 하는 것 | 우리가 하던 것 | 우리가 바꾼 것 |
| --- | --- | --- |
| 방향은 `Input.GetAxisRaw("Mouse X" / "Mouse Y")` × 설정 감도에서 받는다 | 시점 축 배열 `axes[4]`·`axes[5]` 를 쌓아 썼다 | 날 마우스 움직임으로 바꿨다 (`WheelPointer`) |
| 축 배열은 **막는 데만** 쓴다. `PointerInputPolicy.FilterAxes` 가 2~6 을 0 으로 만든다 | 막으면서 같은 배열에서 방향도 읽었다 | `LookBlocker` 는 막기만 한다 (2~5, 이동·린은 살린다) |
| 자기 좌표 `_pointerPosition` 을 0~1 로 쌓고 `Mathf.Clamp01` 로 가둔다 | 쌓은 값을 매번 눌러 0~1 로 환산했다 | 단위원 안에 가두는 가상 커서로 바꿨다 |
| `_pointerVisual` 로 **커서를 직접 그린다** | 아무 것도 그리지 않았다 | 가운데 표시 + 노란 커서 점을 그린다 |
| `ShouldLockCursor() => LockCursor` 로 실제 커서는 잠가 둔다 | 같다(레이드 중 기본 잠김) | 그대로 |
| 감도를 자기 설정으로 둔다(`MouseSensitivity`, 1~80) | 없었다 | `mouseSensitivity` 설정을 뒀다(0.01~2.0, 기본 0.15) |

왜 축 배열이 비었을 가능성이 높은가 — `MoveInputTranslator.TranslateAxes` 는 축을
**쓰는** 쪽이고(`_controller.Look(axes[4], axes[5])`), 입력 사슬에서 우리보다 앞에 있는
노드가 축을 먼저 0 으로 만들면 우리에게는 영영 0 만 온다. `Tylevo.FieldAttachments`
자신이 그렇게 2~6 을 0 으로 만드는 노드이고, 그 모드가 지금 깔려 있다.
그래서 "축을 읽어 방향을 얻는다" 는 전제 자체를 버렸다.

사용자가 꼽은 세 가지와의 대응:

- **마우스 안뜸** → 레이드 중 실제 포인터는 잠겨 있다. 그쪽처럼 **커서를 직접 그린다**.
- **안움직임** → 방향을 날 마우스 움직임에서 받는다. 사슬 위치와 무관하다.
- **디자인 개판** → `useGameWheelSkin` 기본을 false 로. 설치된 설정 파일에도 반영했다.
