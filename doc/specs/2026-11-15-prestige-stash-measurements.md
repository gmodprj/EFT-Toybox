# 측정 — 프레스티지 이전 창고 (R1~R3)

- 날짜: 2026-11-15
- 설계: `doc/specs/2026-11-15-prestige-stash-relax-design.md` §6 의 세 가지를 잰 결과
- 대상: `E:\Spt` (SPT 4.1.6, EFT 0.16.9.5.40743)
- 도구: `ilspycmd` 11.2.0 (dotnet 전역 도구)

**셋 가운데 둘이 코드로 끝났다 — 게임을 재볼 필요가 없었다.** 남은 하나(격자 상한)만 화면으로
확인한다.

| 측정 | 결과 | 어떻게 쟀나 |
| --- | --- | --- |
| **R1** 적용 순서 | **우리가 2540 보다 뒤다. 늘 그렇다** | SPT·2540 어셈블리 디컴파일 |
| **R2** 필터 판정 | **분류 id 가 사슬 전체를 연다. 뿌리 `Item` 하나로 전부 열린다** | 클라이언트 어셈블리 디컴파일 |
| **R3** 격자 상한 | **코드로는 답이 없다** — 격자는 칸 수대로 커지고 늘이지 않는다(칸 63px). 화면이 어디서 잘리는지는 봐야 안다 | 같음 |

## R1 — 적용 순서: 우리가 뒤다

SPT 4.1.6 은 `IOnLoad` 를 **두 단계로** 돌리고, 양쪽 다 `Injectable.TypePriority` **오름차순**이다
(`SPTarkov.DI` 의 `orderby tRef.InjectableAttribute.TypePriority`).

| 단계 | 어디 | 조건 |
| --- | --- | --- |
| 앞 | `SPT.Server.dll` 의 `RunPreSptLoadCallbacks` (웹 서버 기동 **전**) | `0 <= TypePriority < 200000` |
| 뒤 | `SPTStartupHostedService.StartAsync` (번들 적재 뒤) | `TypePriority >= 200000` |

그리고

- **2540** 의 `ContentBackportPrestigesServer.OnLoad.PreSPTLoad` 는
  `[Injectable(InjectionType.Singleton, int.MaxValue) { TypePriority = 100003 }]` 이다. IL 의
  attribute 블롭이 `01 00 02 00 00 00 ff ff ff 7f 01 00 54 08 0c TypePriority a3 86 01 00` —
  위치 인자로 `int.MaxValue` 를 주고 **이름 있는 `TypePriority` 로 0x186a3 = 100003 을 덮어쓴다**
  (이름 있는 쪽이 생성자 뒤에 대입되므로 이쪽이 이긴다). 100003 < 200000 이므로 **앞 단계**다.
- **우리**(`PrestigeStashRelax`)는 `[Injectable(InjectionType.Singleton, int.MaxValue)]` 이고
  `TypePriority` 를 따로 안 주므로 `int.MaxValue` → **뒤 단계**다.

→ **단계가 갈려 있어 순서가 뒤집힐 일이 없다.** 설계 §6-3 이 걱정한 "우리가 먼저 돌아 4단계
원본을 뜨는" 일은 일어나지 않는다.

덤으로, 2540 이 **리스트를 갈아끼우지 않는다**는 것도 확인했다.

```csharp
PrestigeElement val = databasePrestiges.Elements.FirstOrDefault(e => e.Id == prestige.Id);
if (val != null) { int index = databasePrestiges.Elements.IndexOf(val); databasePrestiges.Elements[index] = prestige; }
else databasePrestiges.Elements.Add(prestige);
```

제자리에서 바꾸고 모자란 것만 더한다(P5·P6). 그래도 스냅샷은 **id 를 열쇠로** 둔 그대로 간다 —
3332 가 어떻게 할지는 아직 모른다.

## R2 — 필터 판정: 분류가 사슬 전체를 연다

클라이언트의 전송 화면은 `EFT.UI.PrestigeTransferItemsState` 이고, 품목 판정은 한 줄이다.

```csharp
if (!_stashConfig.Filters.Contains(context.Item.Template))   // 못 넣는다
```

`Filters` 는 `PrestigeTemplate.ItemsTransferConfig.TransferStashConfig.ItemFilter` 이고,

```csharp
public bool Contains(ItemTemplate item) {
    if (_includedItems.Contains(item)) return !_excludedItems.Contains(item);
    ...
}
```

`_includedItems` · `_excludedItems` 는 `UnifiedHandbookEntities : GlobalConfiguration.TraderSettings.HandbookEntities` 다.
**여기가 요점이다** — 그 생성자가 받은 id 목록을 `Items` 와 `Categories` **양쪽에 똑같이** 넣는다.

```csharp
public HandbookEntities(IReadOnlyCollection<MongoID> entities) { Items = entities; Categories = entities; }

public virtual bool Contains(ItemTemplate itemTemplate) {
    foreach (var item in Items)      if (item == itemTemplate._id) return true;
    foreach (var category in Categories) if (itemTemplate.IsChildOf(category)) return true;
    return false;
}
```

그리고 `ItemTemplate.IsChildOf` 는 **부모 사슬을 뿌리까지 거슬러 간다**(자신은 빼고 `Parent`
에서 시작한다).

```csharp
public bool IsChildOf(string parentTemplateId) {
    ItemTemplate parent = Parent;
    while (true) { if (parent != null) { if (parent._id == parentTemplateId) break; parent = parent.Parent; continue; } return false; }
    return true;
}
```

따라서 `includedItems` 에 적는 id 는 **모두 "그 아이템" 이면서 동시에 "그 분류의 모든 자손"** 이다.
설계 §6-1 의 물음 둘이 한꺼번에 풀린다.

- `Backpack`(`5448e53e4bdc2d60728b4567`) 하나로 **배낭 전부**가 열린다. 구체 id 를 펼쳐 넣는
  대비책(설계 §6-1 의 "안 통하면")은 **필요 없다.**
- 뿌리 `Item`(`54009119af1c881c07000029`) 하나로 **그 아래 전부**가 열린다. 게임의 거의 모든
  템플릿이 그 사슬에 있다.
- `excludedItems` 는 **빼기로 작동한다** — 여섯 단계 모두 비어 있으니, "전부 허용 + 몇 가지만
  빼기" 가 쓸 수 있는 조합이다(설계 §8 의 그 자리).

## R3 — 격자 상한: 코드로는 답이 없다

격자는 설정값대로 그대로 만들어진다.

```csharp
_transferGrid = new ReferenceGrid("transferGrid", _stashConfig.Size.x, _stashConfig.Size.y,
    canStretchVertically: false, canStretchHorizontally: false, Array.Empty<ItemFilter>(), stash, canAcceptItems: true);
```

- `Size` 는 `xCellCount` · `yCellCount` 를 그대로 받은 `Vector2Int` 다 — **가로 8 은 데이터가
  그런 것일 뿐이고 코드가 막는 값이 아니다.**
- `canStretchVertically: false` · `canStretchHorizontally: false` — 격자는 **늘어나지도 줄지도
  않는다.** 칸 수가 곧 크기다.
- 격자 자체에는 필터가 없다(`Array.Empty<ItemFilter>()`) — 품목 판정은 위의 `_stashConfig.Filters`
  한 곳뿐이다.
- 뷰(`ArenaEftItemTransferGridView : GridView`)는 `GridView.Show` 에서
  `_rectTransform.sizeDelta = cellPixelSize` 로 **칸 수만큼 커진다**. **칸 하나는 63px** 이다
  (`(value - x) * 63`). `GridView` 자체에는 스크롤이 없다 — 그래서 우리가 `GridWindowScroll` 을
  만들었던 것이고, 그 모드는 `GridWindow.ShowContents` 를 잡으므로 **이 화면은 받쳐 주지 않는다.**

남은 것은 **그 격자를 담은 화면이 어디서 잘리는지**이고, 그것은 Unity 프리팹의 레이아웃이라
코드에 없다. `ObtainPrestigeScreen` 이 가진 수는 `RECT_WIDTH = 1920` · `OFFSET = 250` 뿐이다.
→ **화면으로만 확인된다.** `isb-subtitles-leave-no-log` 와 같은 종류다.

### 그래서 어떻게 재나

설치본 설정을 **세로 +4** 로 두고 한 번 본다. 그러면 P1 8×7(56칸) … P6 8×12(96칸)이고,
세로 12칸이면 `12 × 63 = 756px` 다. 잘리지 않으면 올리고, 잘리면 내린다. 한 번 보는 데
서버 재시작이 한 번 드니 **사다리를 성급히 올리지 않는다.**

## 지금 설치본에 들어 있는 것

`E:\Spt\SPT_Runtime\user\mods\PrestigeStashRelax\` (DLL 26,112 bytes, 2026-11-15 20:55 빌드)

| 설정 | 값 | 왜 |
| --- | --- | --- |
| `Apply` | **true** | R1·R2 가 끝났으므로 재기만 할 이유가 없다 |
| `AllowBags` | **true** | 사용자가 첫 순위로 꼽은 것 — 가방·배낭·케이스 |
| `AllowVest` | **true** | 조끼도 `ArmoredEquipment` 가 아니라 막혀 있었다 |
| `YCellAdd` | **4** | R3 측정용 첫 칸. P1 8×7 … P6 8×12 |
| `Debug` | true | 단계마다 한 줄씩 로그에 |
| 나머지 | false | 돈·탄·열쇠·보안 컨테이너·전부 허용은 아직 안 켠다 |

저장소의 기본 `config.jsonc` 는 `Apply: false` 그대로 둔다 — 새로 설치하는 사람이 모르는 채로
값이 바뀌지 않게.

## 기동 실측 — 2026-11-15 21:21 (서버 로그 12:21 UTC)

서버를 다시 켜서 **R1 을 런타임으로도 확정했다.** 로그가 `[PrestigeStashRelax] 프레스티지 단계
6개를 보았다` 를 찍었다 — 4 가 아니라 6 이므로 우리가 2540 보다 뒤에 돈 것이 맞다.

```
[PrestigeStashRelax] 프레스티지 단계 6개를 보았다.
[PrestigeStashRelax] 672df12f97f0469cea52f55e: 8x3(203) → 8x7(206)
[PrestigeStashRelax] 672df4281ab8d9c8849a0c88: 8x4(186) → 8x8(189)
[PrestigeStashRelax] 683da91d6f472cfa738c52f2: 8x5(186) → 8x9(189)
[PrestigeStashRelax] 6842f121000d98ce33b9a60f: 8x6(186) → 8x10(189)
[PrestigeStashRelax] 68d3ddb4fc101237e601d774: 8x7(186) → 8x11(189)
[PrestigeStashRelax] 68d3e6f46a7ba36646713fa6: 8x8(186) → 8x12(189)
[PrestigeStashRelax] 6단계에 적용했다.
```

| 단계 | 격자 | 칸 수 | 허용 항목 |
| --- | --- | --- | --- |
| P1 | 8×3 → **8×7** | 24 → **56** | 203 → **206** |
| P2 | 8×4 → **8×8** | 32 → **64** | 186 → **189** |
| P3 | 8×5 → **8×9** | 40 → **72** | 186 → **189** |
| P4 | 8×6 → **8×10** | 48 → **80** | 186 → **189** |
| P5 | 8×7 → **8×11** | 56 → **88** | 186 → **189** |
| P6 | 8×8 → **8×12** | 64 → **96** | 186 → **189** |

항목이 **+3** 인 것이 맞다 — `Backpack` · `SimpleContainer` · `Vest` 셋이고, 그 셋이 분류이므로
사슬 아래 전부가 열린다(R2). `prestige-stash-report.json` 의 `before`/`after` 도 같은 값이다.

### 기동 상태 — 기준선과 같다

| 무엇 | 이번 기동 | 전 |
| --- | --- | --- |
| 서버 모드 | **177개** | 175(11-14) + ArmorTweaks(12차-A, 11-15 11:36) + 우리 = 177 ✔ |
| `Could not add` | 48줄 / **고유 id 48개** | 11-09 48 · 11-10 58 · 11-11 54 — 기동마다 다른 값이고 이번은 48 |
| `Unable to add bundle` | 1 | 1 (알려진 번들 중복) |
| `cannot import quest` | 1 | 1 |
| 기동 완료 | `시작된 웹서버 https://127.0.0.1:6969` · `Loaded self-signed certificate` | — |

`[ItemInfo] Completed processing 10673 items in 134672 ms` — 시작 시간의 대부분은 여전히 그
하나가 쓴다.

**오류가 늘지 않았다.** 우리 모드는 아이템 표를 건드리지 않으니 당연한 결과이고, 그것을 숫자로
확인했다.

서버 로그 시각은 **UTC** 다(`server-log-is-utc`) — 12:26 이 KST 21:26 이다.

### 남은 것 — 화면 하나

R3 은 게임을 띄워 **프레스티지 화면 → 전송 화면**을 봐야 한다. 볼 것 둘:

1. **세로 12칸 격자가 잘리지 않는가**(P6 기준 `12 × 63 = 756px`). 잘리면 `YCellAdd` 를 내리고,
   멀쩡하면 올린다.
2. **가방·배낭·케이스·조끼가 끌리는가.** R2 가 코드로는 끝났지만, 실제로 끌어 보는 것이 마지막
   확인이다.

## R3 의 관문 — 전송 화면은 프레스티지가 **열려 있어야** 뜬다

전송 화면(`PrestigeTransferItemsState`)은 `ObtainPrestigeScreen` 의 한 상태이고, 그 화면은
프레스티지 조건이 채워졌을 때만 열린다. 그래서 **프로필 11개 가운데 지금 그 화면을 열 수 있는
것이 있는지**를 먼저 쟀다.

측정을 한 번에 하려고 설정을 바꿨다 — `YCellCount: 12` 로 **모든 단계를 8×12 로 못 박았다.**
그러지 않으면 프레스티지 0 프로필에서는 P1 의 8×7 밖에 못 보고, 쓰려는 최대치(8×12)는 P5·P6 에
가서야 나온다.

### P1 조건 (프레스티지 0 → 1)

| 조건 | 요구 |
| --- | --- |
| Level | ≥ 25 |
| Quest | `6761f28a022f60bb320f3e95` = **New Beginning** (상인 `5ac3b934156ae10c4430e83c`, 2540 이 들고 온 퀘스트다 — SPT 기본 `quests.json` 에는 없다) |
| HideoutArea ×3 | 각각 lv ≥ 1 · 2 · 2 |
| HasItem | 루블 **10,000,000** |

### 프로필 11개 실측 (창고 트리를 내려가며 루블을 셌다 — 가방 안까지)

| 프로필 | Lv | 프레스티지 | 창고 루블 | New Beginning |
| --- | --- | --- | --- | --- |
| Nomad | 58 | **2** | 1,473,785,436 | 상태 0 (잠김) |
| **Talon** | **69** | 0 | 497,013,660 | **상태 3 = 넘기기만 하면 됨** |
| Knox | 69 | 0 | 550,754,881 | 기록 없음 |
| Buck | 69 | 0 | 539,879,009 | 기록 없음 |
| Mace | 69 | 0 | 499,974,122 | 기록 없음 |
| Raptor | 69 | 0 | 534,724,884 | 기록 없음 |
| Wraith · Moose · Sarge · Banshee | 16~19 | 0 | 1억~15억 | 기록 없음 |
| Hawk | 22 | 0 | 3,879,107 (부족) | 기록 없음 |

루블은 어디서도 걸리지 않는다 — 10M 요구에 가장 적은 것도 4.9억이다. **걸리는 것은 퀘스트
하나뿐이고, Talon 이 그 하나를 이미 받아 완료 직전(상태 3 = AvailableForFinish)**이다.

Nomad(프레스티지 2 → P3)는 더 멀다 — P3 가 요구하는 퀘스트 셋 가운데 `6848100b00afffa81f09e365`
는 상태 4(성공)지만 `68cbd33676fe74b1e80bfd91` · `68cbcdc4c964ab83cc0c928e` 는 **프로필에 기록이
아예 없다.**

## R3 결과 — **8×12 는 잘리지 않는다** (2026-11-15 21:45, 1920×1080)

Talon 으로 `New Beginning` 을 넘기자 P1 이 열렸고, 전송 화면이 떴다. 화면을 PNG 로 떠서 읽었다.

- 왼쪽 패널 머리글이 **`SEND ITEMS TO YOURSELF (8x12)`** 다. **우리 값이 클라이언트까지 그대로
  갔다** — 화면이 스스로 적은 숫자다.
- 격자는 **가로 8 × 세로 12 가 전부 그려졌다.** 잘린 줄도, 스크롤바도 없고 패널 아래 테두리가
  닫혀 있다.
- 칸 경계를 세어 12줄을 확인했다. 칸 하나가 **63px** 이라는 코드 쪽 값과도 맞는다.

### 여유는 얼마나 남았나

| 재 것 | 값 |
| --- | --- |
| 패널 위끝 | y ≈ 145 |
| 패널 아래끝 | y ≈ 989 |
| 화면 높이 | 1080 |
| **아래 여백** | **≈ 91px** (칸 하나가 63px 이니 **약 1.4칸**) |

그래서 **13줄은 거의 확실히 들어가고**, 14줄부터는 패널이 어느 쪽으로 자라는지(가운데 기준인지
위 기준인지)에 달린다 — 그건 다시 봐야 안다. **12 를 그대로 두는 것이 안전하다**: 바닐라 P1 의
**4배**(24 → 96칸), P6 의 **1.5배**다. 다른 해상도에서는 여백이 달라질 수 있다는 점도 12 를
고르는 이유다.

가로는 아직 안 재 봤다 — 코드가 막지 않는다는 것만 안다(`canStretchHorizontally: false` 는
"격자가 제 크기를 지킨다"는 뜻이지 8 로 묶는다는 뜻이 아니다). 세로로 충분하므로 당장 필요가
없다.

## 처음에 세운 관문 (기록용)

### 그래서 R3 은 여기서 멈춘다

**Talon 으로 New Beginning 을 상인에게 넘기면** P1 이 열리고, 전송 화면이 우리 8×12 격자로 뜬다.
퀘스트를 넘기는 것은 게임 안에서 사람이 하는 일이고, 프로필의 퀘스트 상태를 손으로 성공으로
바꾸는 것은 **하지 않았다** — 요청에 없고 되돌리기 어렵다.

## 처음에 적었던 것 — 서버 재시작 전 상태 (기록용)

측정을 시작한 21:0x 시점에 **서버는 14:55 부터, 게임은 16:05 부터 돌고 있었다**(프로필
`6adc2669…`(Nomad, 프레스티지 2)가 21:10 에 쓰였다 — 그때도 플레이 중이었다). 서버 모드는
기동 때 한 번 도므로 **다시 켜야 반영된다.** 끄는 일은 하지 않았다 —
`never-stop-server-or-game-unasked`. 파일만 넣었고(`-AllowServerRunning`), 지금 돌고 있는 서버는
옛 상태 그대로다.

다시 켠 뒤 볼 것:

1. 서버 로그 `[PrestigeStashRelax] 프레스티지 단계 N개를 보았다.` → **6** 이어야 한다(4 면 R1 이
   뒤집힌 것이다).
2. `[PrestigeStashRelax] 6단계에 적용했다.`
3. `user\mods\PrestigeStashRelax\prestige-stash-report.json` 의 `before` / `after`
4. 게임에서 프레스티지 화면 → 전송 화면: 격자가 잘리지 않는지, **가방이 끌리는지**

### 측정 중 바로잡은 것

처음에 Git Bash 에서 `tasklist.exe /FO CSV` 로 기동 여부를 보고 **"둘 다 꺼져 있다"고 잘못
판정했다.** Git Bash 가 `/FO` 를 경로로 바꿔 먹어 출력이 비었던 것이다
(`git-bash-eats-leading-slash-routes`). PowerShell 의 `Get-Process` 로 다시 재서 둘 다 돌고 있는
것을 확인했다. **기동 여부는 PowerShell 로 본다.**
