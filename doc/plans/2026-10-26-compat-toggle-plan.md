# CompatToggle Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 무기·부품·탄약의 호환성 제한을 F12 토글로 세션 중에 켜고 끌 수 있는 BepInEx 플러그인을 만들고, 서버 모드 `AllinWeaponUnslotted`를 제거한다.

**Architecture:** 아이템 DB를 고치지 않는다. `EFT.InventoryLogic`의 호환성 판정 메서드를 Harmony로 후킹해, 토글이 켜져 있으면 결과를 "허용"으로 바꾸고 꺼져 있으면 원본 결과를 그대로 흘려보낸다. 판정 로직은 EFT 타입을 모르는 순수 클래스(`CompatGate`)로 분리해 단위 테스트하고, 패치는 EFT 객체를 그 순수 입력으로 번역하는 얇은 껍데기만 담당한다.

**Tech Stack:** C# / net472 / BepInEx 5 / HarmonyX(`0Harmony`) / `SPT.Reflection.Patching` / xunit(net8.0, 테스트 전용)

**Spec:** `doc/specs/2026-10-26-compat-toggle-design.md`

## Global Constraints

- 대상 설치본 `E:\Spt` — SPT 4.1.6, EFT 0.16.9.5.40743.
- 플러그인 GUID `spt.compattoggle`, 표시명 `Compat Toggle`, 버전 `0.1.0`.
- 플러그인 프로젝트는 `net472`. 참조는 `$(SptDir)`(기본 `E:\Spt`) 기준 `HintPath` + `<Private>false</Private>`. NuGet 의존성은 `Microsoft.NETFramework.ReferenceAssemblies` 하나만.
- 토글 기본값: `Master switch`·`Weapon slots`·`Attachment slots`·`Chamber ammo`·`Magazine calibers`·`Magazine ammo`·`Ignore conflicts` = **켜짐**, `Log decisions` = **꺼짐**.
- F12 문구는 영어. 한국어는 `src/SptKoreanConfig/lang/spt.compattoggle.json`에 따로 넣는다.
- 토글이 꺼져 있으면 패치는 **원본 결과를 바꾸지 않는다.** 패치를 런타임에 부착·분리하지 않는다.
- `Ignore conflicts`는 무기·부품에만 적용한다. 헬멧·안면 장비의 `ConflictingItems`는 건드리지 않는다.
- 원본의 `FckALL`·`Experimental`은 구현하지 않는다(설계 §3 비목표). 토글도 만들지 않는다.
- 개조 화면의 **부품 목록** 필터 표시는 이번 범위에서 제외한다(설계 §7이 구현자 판단으로 남긴 항목 — 내 결정으로 0.1.0에서는 하지 않는다). 판정만 바꾸므로 "목록에는 안 보이지만 끌어다 놓으면 들어가는" 상태가 될 수 있다. Task 3 Step 7에서 그 상태를 확인해 `doc/WORKLOG.md`에 기록만 남긴다.
- `AllinWeaponUnslotted`가 설치돼 있는 동안에는 DB가 이미 풀려 있어 "허용" 쪽을 검증할 수 없다. 전환 순서는 설계 §5.5를 따른다.
- 커밋은 작업 브랜치에. `main`·`develop` 직접 커밋 금지.

## Review Focus

- **토글을 모두 끈 상태가 바닐라와 다르다.** 어느 패치든 꺼진 토글에서 원본 결과를 바꾸면 "전부 끄면 바닐라"(설계 목표 3)가 깨진다. → Task 2·3·4·5 각각에 꺼진 상태 테스트.
- **`Slot.ParentItem`이 `null`인 슬롯.** UI 미리보기·고아 슬롯에서 분류가 예외를 던지면 인벤토리 조작 전체가 멈춘다. → Task 3. `ClassifyById`의 `null`·빈 id는 단위 테스트로 덮지만, `Classify(Slot)`의 `null` 가드는 EFT 객체가 필요해 단위 테스트로 덮을 수 없다. Step 3의 가드 구현과 Step 7의 인게임 확인이 그 자리를 맡는다.
- **무기 토글이 장비·보관 용기로 새어나간다.** `CheckCompatibility(Item)`는 `Grid`·`VirtualContainer`·`HideoutAreaGrid`에도 있고, 보안 컨테이너·장비 슬롯도 `Slot`이다. 무기 토글이 여기에 걸리면 보관 제한이 무너진다. → Task 3.
- **`Ignore conflicts`가 헬멧·안면 장비로 새어나간다.** 그러면 `HelmetSlotUnion`·`LessRestrictingHeadwear`·`WolfiksHeavyTrooperMasks`가 의도한 충돌을 이 플러그인이 가려버려 설계 목표 5가 깨진다. → Task 4.
- **같은 메서드를 패치하는 기존 플러그인과 공존하지 못한다.** 이 설치본에 `Slot`/`ItemFilter` 계열을 패치하는 플러그인이 여럿 있다(`Tylevo.FieldAttachments`, `Tyfon.UIFixes`, `maschine-LinkedSearchInStash`, `CompatibilityHighlighter`, `WTT-PackNStrapClient`). 원본 실행을 건너뛰는 Prefix는 그들의 패치를 무력화한다. → Task 3에서 Postfix 전용으로 고정.

---

## 패치 대상 (Mono.Cecil로 `Assembly-CSharp.dll`에서 확인, 2026-10-26)

난독화되지 않은 실명이다. 구현자가 다시 찾을 필요는 없다.

| 메서드 | 쓰임 |
| --- | --- |
| `EFT.InventoryLogic.Slot.CheckCompatibility(Item) -> bool` | 슬롯이 아이템을 받을 수 있는지 (무기 본체·부품·탄창·약실 슬롯 전부 `Slot`) |
| `EFT.InventoryLogic.Slot.CheckConflictingItems(Item) -> Option<...>` | 충돌 검사 |
| `EFT.InventoryLogic.Slot.GetConflictingSlot(Item) -> IEnumerable<...>` | 충돌 슬롯 열거 |
| `EFT.InventoryLogic.StackSlot.CheckCompatibility(Item) -> bool` | 탄창 내부 탄약 묶음 |
| `EFT.InventoryLogic.Magazine.CheckCompatibility(Ammo) -> bool` | 탄창이 그 탄약을 받는지 |
| `EFT.InventoryLogic.ItemFilter.CheckItemFilter(Item) -> bool` | 필터 화이트리스트 |
| `EFT.InventoryLogic.ItemFilter.CheckItemExcludedFilter(Item) -> bool` | 필터 블랙리스트 |
| `EFT.InventoryLogic.Item.ConflictingItems -> string[]` | 충돌 품목 목록(속성) |
| `EFT.InventoryLogic.Slot.Filters -> ItemFilter[]` | 슬롯 필터 |
| `EFT.InventoryLogic.Slot.ParentItem -> Item`, `Slot.ID -> string` | 분류 입력 |

같은 이름의 `CheckCompatibility(Item)`가 `Grid`·`VirtualContainer`·`HideoutAreaGrid`·`IContainer`에도 있다. **이들은 패치하지 않는다.**

`EFT.InventoryLogic.Slot`에 `CanAccept(Item)`는 없다. 존재하는 것은 `CanAcceptRaid(out InventoryError)`이고 이 계획의 대상이 아니다.

## File Structure

| 파일 | 책임 |
| --- | --- |
| `src/CompatToggle/CompatToggle.csproj` | net472, 참조, 빌드 출력 |
| `src/CompatToggle/CompatTogglePlugin.cs` | BepInEx 진입점, 설정 바인딩, 패치 등록 |
| `src/CompatToggle/ToggleSet.cs` | 토글 값 묶음(순수 struct) |
| `src/CompatToggle/CompatGate.cs` | 판정 로직. EFT 타입을 전혀 모른다 |
| `src/CompatToggle/SlotClassifier.cs` | EFT 객체 → `SlotKind`·무기/부품 여부 번역 |
| `src/CompatToggle/Patches/SlotCompatibilityPatch.cs` | `Slot.CheckCompatibility` Postfix |
| `src/CompatToggle/Patches/ConflictPatch.cs` | `Slot.CheckConflictingItems` Postfix |
| `src/CompatToggle/Patches/AmmoPatch.cs` | `StackSlot.CheckCompatibility`, `Magazine.CheckCompatibility` Postfix |
| `src/CompatToggle.Tests/CompatToggle.Tests.csproj` | net8.0 xunit. `CompatGate.cs`·`ToggleSet.cs`를 링크 컴파일 |
| `src/CompatToggle.Tests/CompatGateTests.cs` | 판정 로직 테스트 |
| `tools/install-compattoggle.ps1` | 빌드 산출물 설치 |
| `src/SptKoreanConfig/lang/spt.compattoggle.json` | F12 한국어 문구 |

`CompatGate.cs`와 `ToggleSet.cs`는 **EFT·BepInEx·Unity를 참조하지 않는다**(내 결정). 그래야 테스트 프로젝트가 게임 어셈블리 없이 net8.0에서 돌 수 있다. 테스트 프로젝트는 그 두 파일을 `<Compile Include="..\CompatToggle\CompatGate.cs" />`로 링크한다.

---

### Task 1: 프로젝트 골격과 토글

**Files:**
- Create: `src/CompatToggle/CompatToggle.csproj`, `src/CompatToggle/CompatTogglePlugin.cs`, `src/CompatToggle/ToggleSet.cs`
- Reference: `src/SptKoreanConfig/SptKoreanConfig.csproj` (참조·`SptDir` 구성의 틀)

**Interfaces:**
- Consumes: 없음
- Produces: `public readonly struct ToggleSet` — 생성자 `ToggleSet(bool master, bool weaponSlots, bool attachmentSlots, bool chamberAmmo, bool magazineCalibers, bool magazineAmmo, bool ignoreConflicts)`, 같은 이름의 `bool` 속성 7개. `public static ToggleSet CompatTogglePlugin.Toggles { get; }` — 현재 F12 값을 읽어 만든 묶음. `public static bool CompatTogglePlugin.LogDecisions { get; }`.

- [ ] **Step 1: `ToggleSet.cs` 작성**

EFT·BepInEx·Unity 참조 없는 `readonly struct`. 위 Produces의 생성자와 속성 7개만.

- [ ] **Step 2: `CompatToggle.csproj` 작성**

`src/SptKoreanConfig/SptKoreanConfig.csproj`를 틀로 쓴다. `AssemblyName`/`RootNamespace` = `CompatToggle`, `Version` = `0.1.0`. 참조: `BepInEx.dll`, `0Harmony.dll`, `Assembly-CSharp.dll`, `spt-reflection.dll`(둘 다 `$(SptDir)\EscapeFromTarkov_Data\Managed\`와 `$(SptDir)\BepInEx\plugins\spt\` 중 실제 위치), 필요한 `UnityEngine.*`. `ConfigurationManager.dll`은 참조하지 않는다 — 설정 등록에 필요 없다.

- [ ] **Step 3: `CompatTogglePlugin.cs` 작성**

`[BepInPlugin("spt.compattoggle", "Compat Toggle", "0.1.0")]`, `BaseUnityPlugin` 상속. `Awake()`에서 토글 8개를 `Config.Bind`로 등록한다. 섹션은 `General`(Master switch, Log decisions)과 `Compatibility`(나머지 6개). 기본값은 Global Constraints대로. 설명 문구에는 설계 §5.3의 반쪽 조합 주의사항을 넣는다. `Toggles`는 매번 `ConfigEntry.Value`를 읽어 새 `ToggleSet`을 만든다 — 캐시하지 않는다(토글 변경이 다음 판정부터 반영되게).

- [ ] **Step 4: 빌드**

Run: `dotnet build E:\Spt_Mods\src\CompatToggle\CompatToggle.csproj -c Release`
Expected: 경고 없이 성공, `bin\Release\CompatToggle.dll` 생성

- [ ] **Step 5: 게임에서 F12에 뜨는지 확인**

`CompatToggle.dll`을 `E:\Spt\BepInEx\plugins\`에 복사하고 게임 실행 → F12 → `Compat Toggle` 항목 8개가 보이고 기본값이 Global Constraints와 같은지 확인. `E:\Spt\BepInEx\LogOutput.log`에 이 플러그인 관련 오류가 없어야 한다.

- [ ] **Step 6: 커밋**

```bash
git add src/CompatToggle
git commit -m "feat: CompatToggle 플러그인 골격과 F12 토글"
```

---

### Task 2: 판정 로직

**Files:**
- Create: `src/CompatToggle/CompatGate.cs`, `src/CompatToggle.Tests/CompatToggle.Tests.csproj`, `src/CompatToggle.Tests/CompatGateTests.cs`

**Interfaces:**
- Consumes: `ToggleSet` (Task 1)
- Produces:
  - `public enum SlotKind { Other = 0, WeaponBody, Attachment, Magazine, Chamber, CartridgeStack }`
  - `public static bool CompatGate.ShouldForceAllow(in ToggleSet toggles, SlotKind kind)`
  - `public static bool CompatGate.ShouldIgnoreConflict(in ToggleSet toggles, bool subjectIsWeaponOrMod)`

- [ ] **Step 1: 테스트 프로젝트 만들기**

`src/CompatToggle.Tests/CompatToggle.Tests.csproj` — net8.0, xunit, `<Compile Include="..\CompatToggle\CompatGate.cs" />`와 `..\CompatToggle\ToggleSet.cs` 링크. 게임 어셈블리를 참조하지 않는다.

- [ ] **Step 2: 실패하는 테스트 작성**

```csharp
static ToggleSet AllOn => new(true, true, true, true, true, true, true);
static ToggleSet AllOff => new(false, false, false, false, false, false, false);

[Theory]
[InlineData(SlotKind.WeaponBody)]
[InlineData(SlotKind.Attachment)]
[InlineData(SlotKind.Magazine)]
[InlineData(SlotKind.Chamber)]
[InlineData(SlotKind.CartridgeStack)]
public void AllOn_forces_allow_for_every_handled_kind(SlotKind kind)
    => Assert.True(CompatGate.ShouldForceAllow(AllOn, kind));

[Theory]
[InlineData(SlotKind.WeaponBody)]
[InlineData(SlotKind.Attachment)]
[InlineData(SlotKind.Magazine)]
[InlineData(SlotKind.Chamber)]
[InlineData(SlotKind.CartridgeStack)]
public void AllOff_forces_nothing(SlotKind kind)
    => Assert.False(CompatGate.ShouldForceAllow(AllOff, kind));

[Fact]
public void Other_kind_is_never_forced()
    => Assert.False(CompatGate.ShouldForceAllow(AllOn, SlotKind.Other));

[Fact]
public void Master_off_overrides_every_toggle()
{
    var t = new ToggleSet(false, true, true, true, true, true, true);
    Assert.False(CompatGate.ShouldForceAllow(t, SlotKind.WeaponBody));
    Assert.False(CompatGate.ShouldIgnoreConflict(t, subjectIsWeaponOrMod: true));
}

[Fact]
public void Each_toggle_governs_only_its_own_kind()
{
    var onlyWeapon = new ToggleSet(true, true, false, false, false, false, false);
    Assert.True(CompatGate.ShouldForceAllow(onlyWeapon, SlotKind.WeaponBody));
    Assert.False(CompatGate.ShouldForceAllow(onlyWeapon, SlotKind.Attachment));
    Assert.False(CompatGate.ShouldForceAllow(onlyWeapon, SlotKind.Magazine));
    Assert.False(CompatGate.ShouldForceAllow(onlyWeapon, SlotKind.Chamber));
    Assert.False(CompatGate.ShouldForceAllow(onlyWeapon, SlotKind.CartridgeStack));
}

[Fact]
public void Conflicts_are_ignored_only_for_weapons_and_mods()
{
    Assert.True(CompatGate.ShouldIgnoreConflict(AllOn, subjectIsWeaponOrMod: true));
    Assert.False(CompatGate.ShouldIgnoreConflict(AllOn, subjectIsWeaponOrMod: false));
}

[Fact]
public void Conflicts_off_ignores_nothing()
    => Assert.False(CompatGate.ShouldIgnoreConflict(AllOff, subjectIsWeaponOrMod: true));
```

- [ ] **Step 3: 테스트가 실패하는지 확인**

Run: `dotnet test E:\Spt_Mods\src\CompatToggle.Tests -v minimal`
Expected: 컴파일 실패 — `CompatGate`, `SlotKind` 없음

- [ ] **Step 4: `CompatGate.cs` 구현**

위 Produces의 두 메서드와 `SlotKind`. `ShouldForceAllow`는 `Master`가 꺼져 있으면 무조건 `false`, 그 외에는 `kind`에 대응하는 토글 하나를 본다(`Magazine` ↔ `MagazineCalibers`, `CartridgeStack` ↔ `MagazineAmmo`, `Chamber` ↔ `ChamberAmmo`). `Other`는 항상 `false`. `ShouldIgnoreConflict`는 `Master` && `IgnoreConflicts` && `subjectIsWeaponOrMod`.

- [ ] **Step 5: 테스트 통과 확인**

Run: `dotnet test E:\Spt_Mods\src\CompatToggle.Tests -v minimal`
Expected: 전부 PASS

- [ ] **Step 6: 커밋**

```bash
git add src/CompatToggle/CompatGate.cs src/CompatToggle.Tests
git commit -m "feat: 호환성 판정 로직과 단위 테스트"
```

---

### Task 3: 슬롯 분류와 수용 판정 패치

**Files:**
- Create: `src/CompatToggle/SlotClassifier.cs`, `src/CompatToggle/Patches/SlotCompatibilityPatch.cs`
- Modify: `src/CompatToggle/CompatTogglePlugin.cs` (패치 등록)
- Test: `src/CompatToggle.Tests/SlotClassifierTests.cs`

**Interfaces:**
- Consumes: `SlotKind`, `CompatGate` (Task 2), `CompatTogglePlugin.Toggles` (Task 1)
- Produces:
  - `public static SlotKind SlotClassifier.ClassifyById(string slotId, bool parentIsWeapon, bool parentIsMod)` — 순수 함수. 테스트 대상.
  - `public static SlotKind SlotClassifier.Classify(EFT.InventoryLogic.Slot slot)` — EFT 객체를 위 함수 입력으로 번역. `slot`·`slot.ParentItem`이 `null`이면 `SlotKind.Other`.
  - `public static bool SlotClassifier.IsWeaponOrMod(EFT.InventoryLogic.Item item)`

- [ ] **Step 1: 실패하는 분류 테스트 작성**

`ClassifyById`만 테스트한다(EFT 타입이 없는 순수 함수).

```csharp
[Theory]
[InlineData("patron_in_weapon", SlotKind.Chamber)]
[InlineData("patron_in_weapon_000", SlotKind.Chamber)]
[InlineData("camora_000", SlotKind.Chamber)]
[InlineData("mod_magazine", SlotKind.Magazine)]
public void Ammo_and_magazine_slots_are_classified_by_id(string id, SlotKind expected)
    => Assert.Equal(expected, SlotClassifier.ClassifyById(id, parentIsWeapon: true, parentIsMod: false));

[Fact]
public void Mod_slot_on_a_weapon_is_weapon_body()
    => Assert.Equal(SlotKind.WeaponBody,
        SlotClassifier.ClassifyById("mod_scope", parentIsWeapon: true, parentIsMod: false));

[Fact]
public void Mod_slot_on_a_mod_is_attachment()
    => Assert.Equal(SlotKind.Attachment,
        SlotClassifier.ClassifyById("mod_scope", parentIsWeapon: false, parentIsMod: true));

[Theory]
[InlineData("Headwear")]
[InlineData("SecuredContainer")]
[InlineData("TacticalVest")]
[InlineData("Helmet_top")]
public void Equipment_and_container_slots_are_other(string id)
    => Assert.Equal(SlotKind.Other,
        SlotClassifier.ClassifyById(id, parentIsWeapon: false, parentIsMod: false));

[Fact]
public void Unknown_parent_is_other()
    => Assert.Equal(SlotKind.Other,
        SlotClassifier.ClassifyById("mod_scope", parentIsWeapon: false, parentIsMod: false));

[Fact]
public void Null_or_empty_id_is_other()
{
    Assert.Equal(SlotKind.Other, SlotClassifier.ClassifyById(null, true, false));
    Assert.Equal(SlotKind.Other, SlotClassifier.ClassifyById("", true, false));
}
```

- [ ] **Step 2: 테스트 실패 확인**

Run: `dotnet test E:\Spt_Mods\src\CompatToggle.Tests -v minimal`
Expected: 컴파일 실패 — `SlotClassifier` 없음

- [ ] **Step 3: `SlotClassifier.cs` 구현**

`ClassifyById`는 `CompatGate.cs`처럼 EFT를 참조하지 않는 파일에 두고 테스트 프로젝트에 링크한다. 판정 순서: id가 비면 `Other` → `patron_in_weapon`/`camora` 접두사면 `Chamber` → `mod_magazine`이면 `Magazine` → `mod_` 접두사이면 부모에 따라 `WeaponBody`/`Attachment` → 그 외 `Other`.

`Classify(Slot)`와 `IsWeaponOrMod(Item)`는 EFT를 참조하는 별 파일(`SlotClassifier.Eft.cs`)에 둔다. 무기 여부는 `Item is Weapon`, 부품 여부는 `Item is Mod`로 본다 — 실제 타입명은 `Assembly-CSharp.dll`에서 확인해 맞춘다.

- [ ] **Step 4: 테스트 통과 확인**

Run: `dotnet test E:\Spt_Mods\src\CompatToggle.Tests -v minimal`
Expected: 전부 PASS

- [ ] **Step 5: `SlotCompatibilityPatch.cs` 구현**

`SPT.Reflection.Patching.ModulePatch` 상속. 대상은 `EFT.InventoryLogic.Slot.CheckCompatibility(Item)` **하나만**. `Grid`·`VirtualContainer`·`HideoutAreaGrid`·`IContainer`의 동명 메서드는 대상이 아니다.

**Postfix만 쓴다**(Review Focus 5번). 원본 실행을 건너뛰지 않으므로 같은 메서드를 패치하는 다른 플러그인이 영향받지 않는다. 동작: `__result`가 이미 `true`면 그대로 둔다. `false`일 때만 `CompatGate.ShouldForceAllow(CompatTogglePlugin.Toggles, SlotClassifier.Classify(__instance))`가 `true`면 `__result = true`로 바꾼다.

`CompatTogglePlugin.Awake()`에서 이 패치를 `Enable()`한다.

- [ ] **Step 6: 빌드**

Run: `dotnet build E:\Spt_Mods\src\CompatToggle\CompatToggle.csproj -c Release`
Expected: 성공

- [ ] **Step 7: 게임에서 확인**

`AllinWeaponUnslotted`가 아직 설치된 상태다. 개조 화면에서 `Master switch`를 끄고 바닐라라면 들어가지 않는 부품 조합을 시도 → **거부**되어야 한다. 다시 켜고 화면을 나갔다 들어오면 → **허용**되어야 한다. 게임 재시작 없이 바뀌는지가 핵심이다. 보관 용기·장비 슬롯에 엉뚱한 물건이 들어가지 않는지도 함께 본다(Review Focus 3번).

- [ ] **Step 8: 커밋**

```bash
git add src/CompatToggle src/CompatToggle.Tests
git commit -m "feat: 슬롯 분류와 수용 판정 패치"
```

---

### Task 4: 충돌 토글

**Files:**
- Create: `src/CompatToggle/Patches/ConflictPatch.cs`
- Modify: `src/CompatToggle/CompatTogglePlugin.cs` (패치 등록)

**Interfaces:**
- Consumes: `CompatGate.ShouldIgnoreConflict` (Task 2), `SlotClassifier.IsWeaponOrMod` (Task 3)
- Produces: 없음(최종 동작)

- [ ] **Step 1: `ConflictPatch.cs` 구현**

대상 `EFT.InventoryLogic.Slot.CheckConflictingItems(Item) -> Option<...>`. Postfix. 원본이 "충돌 있음"을 돌려준 경우에만, `CompatGate.ShouldIgnoreConflict(CompatTogglePlugin.Toggles, SlotClassifier.IsWeaponOrMod(item))`가 `true`면 "충돌 없음"으로 바꾼다. `Option<...>`의 빈 값 만드는 방법은 `Assembly-CSharp.dll`에서 확인한다.

**`subjectIsWeaponOrMod`는 설치하려는 아이템(`item` 인자)으로 판정한다.** 헬멧·안면 장비는 무기·부품이 아니므로 자동으로 제외된다 — 이것이 설계가 요구한 범위 한정이다.

- [ ] **Step 2: 빌드**

Run: `dotnet build E:\Spt_Mods\src\CompatToggle\CompatToggle.csproj -c Release`
Expected: 성공

- [ ] **Step 3: 헬멧 쪽으로 새어나가지 않는지 게임에서 확인**

`Ignore conflicts`를 켠 상태에서, `HelmetSlotUnion`·`LessRestrictingHeadwear`·`WolfiksHeavyTrooperMasks`가 의도적으로 남겨둔 헬멧·안면 장비 충돌이 **여전히 걸리는지** 확인한다. 걸리지 않으면 범위 한정이 실패한 것이다(Review Focus 4번).

무기 부품끼리의 충돌은 토글을 켜면 무시되고 끄면 되살아나야 한다.

- [ ] **Step 4: 커밋**

```bash
git add src/CompatToggle
git commit -m "feat: 무기·부품 한정 충돌 무시 토글"
```

---

### Task 5: 탄약 토글

**Files:**
- Create: `src/CompatToggle/Patches/AmmoPatch.cs`
- Modify: `src/CompatToggle/CompatTogglePlugin.cs` (패치 등록)

**Interfaces:**
- Consumes: `CompatGate.ShouldForceAllow`, `SlotKind` (Task 2)
- Produces: 없음(최종 동작)

- [ ] **Step 1: `AmmoPatch.cs` 구현**

패치 둘. 각각 Postfix, `__result`가 `false`일 때만 바꾼다.

- `EFT.InventoryLogic.StackSlot.CheckCompatibility(Item)` → `SlotKind.CartridgeStack`
- `EFT.InventoryLogic.Magazine.CheckCompatibility(Ammo)` → `SlotKind.CartridgeStack`

약실과 탄창 슬롯은 `Slot`이므로 Task 3의 패치가 이미 처리한다. 이 Task는 **탄창 내부에 들어가는 탄약**만 담당한다.

- [ ] **Step 2: 빌드**

Run: `dotnet build E:\Spt_Mods\src\CompatToggle\CompatToggle.csproj -c Release`
Expected: 성공

- [ ] **Step 3: 게임에서 확인**

`Magazine ammo`를 켠 상태에서 구경이 다른 탄약을 탄창에 채워본다 → 허용. 끄면 → 거부. 화면 재진입만으로 바뀌어야 한다. `Chamber ammo`로 약실에 다른 구경을 올려본다.

장전 경로도 확인한다 — `ozen.ContinuousLoadAmmo`·`RZDirectReload`·`maschine-UnloadAllMagazines`가 깔려 있으니 그 조작으로도 같은 결과가 나오는지 본다(설계 §5.3).

- [ ] **Step 4: 커밋**

```bash
git add src/CompatToggle
git commit -m "feat: 탄창 탄약 호환 토글"
```

---

### Task 6: 판정 로깅

**Files:**
- Modify: `src/CompatToggle/Patches/SlotCompatibilityPatch.cs`, `src/CompatToggle/Patches/ConflictPatch.cs`, `src/CompatToggle/Patches/AmmoPatch.cs`

**Interfaces:**
- Consumes: `CompatTogglePlugin.LogDecisions` (Task 1)
- Produces: `public static void CompatTogglePlugin.LogForced(string kind, string slotId, string itemName)`

- [ ] **Step 1: `LogForced` 구현과 호출 추가**

`CompatTogglePlugin`에 `LogForced`를 더한다. `LogDecisions`가 꺼져 있으면 즉시 반환한다 — 문자열을 만들기 **전에** 반환해야 한다. `CheckCompatibility`는 인벤토리 조작마다 여러 번 불리는 뜨거운 경로다.

세 패치에서 결과를 `true`로 **바꾼 경우에만** 호출한다. 바꾸지 않은 판정은 로그하지 않는다.

- [ ] **Step 2: 빌드**

Run: `dotnet build E:\Spt_Mods\src\CompatToggle\CompatToggle.csproj -c Release`
Expected: 성공

- [ ] **Step 3: 게임에서 확인**

`Log decisions`를 켜고 개조 화면에서 바닐라라면 거부될 조합을 설치 → `E:\Spt\BepInEx\LogOutput.log`에 그 건이 남는다. 끈 상태에서는 아무것도 남지 않는다. 인벤토리를 한참 조작해도 로그가 폭증하지 않는지 본다.

- [ ] **Step 4: 커밋**

```bash
git add src/CompatToggle
git commit -m "feat: 허용으로 바꾼 판정 로깅 토글"
```

---

### Task 7: 한국어 문구, 설치 스크립트, 전환

**Files:**
- Create: `tools/install-compattoggle.ps1`, `src/SptKoreanConfig/lang/spt.compattoggle.json`
- Modify: `doc/INSTALLED-MODS.md`(생성물), `doc/WORKLOG.md`

**Interfaces:**
- Consumes: Task 1~6의 산출물
- Produces: 없음

- [ ] **Step 1: 한국어 문구 파일 작성**

`src/SptKoreanConfig/lang/` 의 기존 파일 형식을 따른다. 토글 8개의 이름과 설명을 한국어로 넣는다. 용어는 기존 번역과 맞춘다.

- [ ] **Step 2: 커버리지 검사**

Run: `node E:\Spt_Mods\tools\check-lang-coverage.mjs`
Expected: `spt.compattoggle` 누락 0

- [ ] **Step 3: 설치 스크립트 작성**

`tools/install-koreanconfig.ps1`를 틀로 쓴다. `CompatToggle.dll`을 `E:\Spt\BepInEx\plugins\`에, 한국어 파일을 `SptKoreanConfig`의 `lang` 설치 위치에 복사한다. 덮어쓸 파일이 있으면 중단한다(`doc/PROCEDURE.md` 규칙).

- [ ] **Step 4: 프로필 백업과 시험용 무기 조립**

`E:\Spt\SPT_Runtime\user\profiles\*` → `backup/profiles-before-compattoggle/`. `AllinWeaponUnslotted/config/config.jsonc`도 백업한다.

그 다음, **`AllinWeaponUnslotted`가 아직 설치된 상태에서** 바닐라라면 불가능한 조합의 무기를 하나 조립해 프로필에 남긴다(부품 조합 + 구경이 다른 탄창). 설계 §6-3을 확인할 물건이다. 이 단계가 제거보다 먼저다.

- [ ] **Step 5: 서버 모드 제거**

`doc/install-records/`의 기록을 보고 `AllinWeaponUnslotted`가 설치한 파일만 제거한다. 서버를 재시작하고 서버 로그에 오류가 없는지 확인한다.

- [ ] **Step 6: 설계 §6 확인**

설계 §6의 1·2·4·5·6과 3의 메뉴 부분을 게임에서 확인한다. §6-3의 **레이드 입장·복귀 부분은 사용자에게 번호 붙인 체크리스트로 넘기고 결과를 받는다.**

특히 4번: `Tylevo.FieldAttachments`, `Tyfon.UIFixes`, `maschine-LinkedSearchInStash`, `CompatibilityHighlighter`, `WTT-PackNStrapClient`가 계속 동작하는지 하나씩 본다.

- [ ] **Step 7: 기록 재생성과 커밋**

Run: `node E:\Spt_Mods\tools\gen-installed-mods.mjs`

```bash
git add tools/install-compattoggle.ps1 src/SptKoreanConfig/lang/spt.compattoggle.json doc/INSTALLED-MODS.md doc/WORKLOG.md
git commit -m "feat: CompatToggle 설치 스크립트와 한국어 문구, AllinWeaponUnslotted 제거"
```
