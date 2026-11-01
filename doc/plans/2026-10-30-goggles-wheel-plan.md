# GogglesWheel 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** N 하나로 야시경과 페이스실드를 갈라 여닫는다 — 짧게 누르면 야시경, 길게 누르면 원형 메뉴에서 골라서.

**Architecture:** `Player.ToggleGoggles` 에 Harmony prefix 를 걸어 바닐라의 `FirstOrDefault()` 동작을 대신한다. 이미 같은 메서드를 가로채는 모드(UI Fixes·COTI)가 있어 **물러나는 조건**이 먼저다. 짧게/길게는 `IGameKey.Press` 를 보고 가르고, 길게면 제스처 휠 항목 템플릿을 복제한 원형 메뉴를 띄운다. 판정 로직(설정·각도·기본 대상)은 게임 어셈블리를 모르는 순수 파일로 떼어 내 xunit 으로 시험한다.

**Tech Stack:** C# / net472 / BepInEx 5 / HarmonyX / Unity UGUI · 시험은 net8.0 + xunit

**Spec:** [doc/specs/2026-10-30-goggles-wheel-design.md](../specs/2026-10-30-goggles-wheel-design.md)

## Global Constraints

- `TargetFramework` 은 `net472`, `DebugType` 은 `none`, `AppendTargetFrameworkToOutputPath` 는 false, `<SptDir Condition="'$(SptDir)' == ''">E:\Spt</SptDir>` (`src/DynamicMapsFactionMarkers/DynamicMapsFactionMarkers.csproj` 와 같은 틀).
- 플러그인 GUID `spt.goggleswheel`, 표시 이름 `야시경·실드 휠`, 버전 `0.1.0`.
- **하드 의존 없음.** UI Fixes·COTI 는 있을 수도 없을 수도 있다 — 리플렉션으로 찾고 없으면 그 검사만 건너뛴다.
- Harmony 우선순위는 **낮게**(`HarmonyPriority(Priority.Low)`) — 남의 가드가 먼저 돌아야 한다.
- 주석·로그·설정 설명은 **한국어**.
- 레이드 밖(하이드아웃·메뉴)에서는 아무 것도 하지 않는다.
- **git**: 같은 작업 트리를 옆 세션(AirdropCaller)과 공유한다. 커밋 전에 `git status --short` 를 보고 내가 건드린 적 없는 파일이 보이면 멈추고 그 세션에 묻는다. 브랜치는 `develop` 에서 `feat/goggles-wheel` 로 떼고, 모든 git 명령에 `-C E:\Spt_Mods` 를 붙인다.

## Review Focus

설계가 명시하지 않았지만 틀리면 사용자를 문다. 각 줄의 시험은 그 코드를 가진 Task 에 넣었다.

1. 헬멧이 없거나 여닫이가 0개일 때 — 아무 일도 없고 예외도 없다(Task 2).
2. `longPressSeconds` 가 0·음수, `wheelDeadZone` 이 1 초과 같은 값일 때 — 안전한 범위로 눌러 쓴다(Task 1).
3. 항목이 3개 이상일 때 경계각 — 두 항목 사이 정확한 경계에서 한쪽으로 확실히 정해진다(Task 1).
4. 각도가 ±180도를 넘어 감길 때 — 올바른 항목이 나온다(Task 1).
5. 같은 종류가 둘일 때(야시경 2개 등) — 목록과 기본 대상이 흔들리지 않는다(Task 1·2).

---

### Task 1: 설정과 고르기 판정 (순수)

**Files:**
- Create: `src/GogglesWheel/WheelConfig.cs`
- Create: `src/GogglesWheel/ToggleTarget.cs`
- Create: `src/GogglesWheel/TargetPicker.cs`
- Create: `src/GogglesWheel/config.jsonc`
- Create: `src/GogglesWheel.Tests/GogglesWheel.Tests.csproj`
- Test: `src/GogglesWheel.Tests/WheelConfigTests.cs`, `src/GogglesWheel.Tests/TargetPickerTests.cs`

**Interfaces:**
- Consumes: 없음(첫 Task).
- Produces:
  - `enum ToggleKind { NightVision, Thermal, FaceShield, HelmetVisor, Other }`
  - `sealed class ToggleTarget { string Id; ToggleKind Kind; string Name; }` — `Id` 는 아이템 id(게임 쪽에서 실제 부품을 다시 찾는 열쇠). 생성자 `ToggleTarget(string id, ToggleKind kind, string name)`.
  - `sealed class WheelConfig { bool Enabled; float LongPressSeconds; string DefaultTarget; float WheelDeadZone; bool UseGameWheelSkin; bool IncludeHelmetVisor; bool StandDownForCoti; }`
  - `static WheelConfig WheelConfig.Default()` / `static WheelConfig WheelConfig.LoadFromText(string jsonc, out string warning)` — 깨졌으면 기본값, 값은 범위로 눌러 쓴다(`LongPressSeconds` 0.05~2.0, `WheelDeadZone` 0.0~0.9).
  - `static class TargetPicker`
    - `static float AngleForIndex(int index, int count)` — 도 단위. `count == 2` 면 index 0 = 180(왼쪽), 1 = 0(오른쪽). 그 밖이면 `90 - index * 360 / count`(위에서 시계 방향).
    - `static int PickByDirection(float x, float y, int count, float deadZone)` — 길이가 `deadZone` 미만이거나 `count <= 0` 이면 `-1`.
    - `static int DefaultTargetIndex(IReadOnlyList<ToggleTarget> targets, string defaultTarget)` — `"nightvision"`·`"faceshield"`·`"first"`. 해당 종류가 없으면 0, 목록이 비었으면 -1. 같은 종류가 둘이면 **먼저 나온 것**.

시험 프로젝트는 `src/DynamicMapsFactionMarkers.Tests/DynamicMapsFactionMarkers.Tests.csproj` 와 같은 틀(net8.0, xunit 2.9.2, Newtonsoft.Json 13.0.3, `EnableDefaultCompileItems` false, 게임 어셈블리 참조 없음). 위 세 순수 파일만 `<Compile Include="..\GogglesWheel\*.cs" />` 로 링크한다.

- [ ] **Step 1: 실패하는 시험을 쓴다** — `WheelConfigTests.cs`

```csharp
[Fact] public void Default_values_match_the_spec()
    // Enabled, LongPressSeconds 0.25, DefaultTarget "nightvision", WheelDeadZone 0.2,
    // UseGameWheelSkin, IncludeHelmetVisor, StandDownForCoti 전부 true
[Fact] public void LoadFromText_broken_json_falls_back_to_default_with_warning()
[Fact] public void LoadFromText_accepts_comments_and_trailing_commas()        // "// 주석\n{ \"enabled\": false, }"
[Theory] [InlineData(0f)] [InlineData(-1f)] [InlineData(99f)]
    public void LongPressSeconds_is_clamped(float v)                          // 0.05 ~ 2.0 안으로
[Theory] [InlineData(-0.5f)] [InlineData(1.5f)]
    public void WheelDeadZone_is_clamped(float v)                             // 0.0 ~ 0.9 안으로
[Fact] public void Unknown_default_target_falls_back_to_nightvision()
```

- [ ] **Step 2: 실패하는 시험을 쓴다** — `TargetPickerTests.cs`

```csharp
[Fact] public void Two_items_sit_left_and_right()          // AngleForIndex(0,2)==180, (1,2)==0
[Fact] public void Four_items_start_at_top_clockwise()     // (0,4)==90, (1,4)==0, (2,4)==-90, (3,4)==-180
[Fact] public void Direction_inside_the_dead_zone_picks_nothing()   // PickByDirection(0.1f,0f,2,0.2f) == -1
[Fact] public void Direction_right_picks_the_right_item()           // (1,0,2,0.2f) == 1
[Fact] public void Direction_left_picks_the_left_item()             // (-1,0,2,0.2f) == 0
[Fact] public void Direction_wraps_around_180_degrees()
    // count 4 에서 (-1, -0.01f) 과 (-1, 0.01f) 이 각각 어느 쪽으로 가는지 — 감겨도 이웃 항목으로만 간다
[Fact] public void Exact_boundary_between_two_items_is_stable()
    // count 3 에서 두 항목 정확한 중간각 → 같은 입력에 늘 같은 답, 범위 안의 index
[Fact] public void No_items_picks_nothing()                         // count 0 → -1
[Fact] public void Default_target_prefers_night_vision()
[Fact] public void Default_target_falls_back_to_first_when_kind_absent()
[Fact] public void Default_target_with_two_night_visions_takes_the_first()
[Fact] public void Default_target_on_empty_list_is_minus_one()
```

- [ ] **Step 3: 실패를 확인한다**

Run: `dotnet test src/GogglesWheel.Tests -v minimal`
Expected: 컴파일 실패(`WheelConfig`·`TargetPicker` 없음).

- [ ] **Step 4: `ToggleTarget`·`WheelConfig`·`TargetPicker` 를 구현한다**

각도는 `Mathf` 없이 `System.Math` 로 계산한다(순수 파일이라 Unity 를 참조하지 않는다). `PickByDirection` 은 `Atan2(y, x)` 를 도로 바꿔 각 항목 각도와의 차이를 ±180 으로 정규화해 가장 작은 것을 고른다.

- [ ] **Step 5: 시험이 통과하는지 확인한다**

Run: `dotnet test src/GogglesWheel.Tests -v minimal`
Expected: PASS (전부)

- [ ] **Step 6: `config.jsonc` 를 기본값대로 적는다** — 설계 §6-5 의 7개 항목, 각 줄에 한국어 주석 한 줄.

- [ ] **Step 7: 커밋**

```bash
git -C E:\Spt_Mods add src/GogglesWheel src/GogglesWheel.Tests
git -C E:\Spt_Mods commit -m "feat: 휠 설정과 고르기 판정"
```

---

### Task 2: 패치·헬멧 훑기·짧게/길게 (휠 없이)

**Files:**
- Create: `src/GogglesWheel/GogglesWheel.csproj`
- Create: `src/GogglesWheel/Plugin.cs`
- Create: `src/GogglesWheel/HeadwearScanner.cs`
- Create: `src/GogglesWheel/GogglesPatch.cs`
- Create: `src/GogglesWheel/PressWatcher.cs`
- Create: `tools/install-goggleswheel.ps1`

**Interfaces:**
- Consumes: Task 1 의 `WheelConfig`·`ToggleTarget`·`TargetPicker`.
- Produces:
  - `static class Plugin` 정적 멤버: `ManualLogSource Log`, `ConfigEntry<bool> EnableWheel`, `WheelConfig Settings`, `bool Active`.
  - `static IReadOnlyList<ToggleTarget> HeadwearScanner.Scan(Player player)` — 설계 §6-3 의 순서로. 비었으면 빈 목록.
  - `static bool HeadwearScanner.Toggle(Player player, string targetId)` — 그 부품의 `TogglableComponent` 를 뒤집는다. 못 찾으면 false.
  - `static bool PressWatcher.TryGetGogglesKey(out IGameKey key)` — `InputManager._currentPreset`(private static) → `InputPreset._inputKeyCombinations` → `EGameKey.ToggleGoggles` 조합의 첫 `IGameKey`.
  - `static bool PressWatcher.IsHeldNow()` — 위 키의 `Press` 가 Down/Hold 인지. 키를 못 찾으면 false.
  - `static bool GogglesPatch.TryApply(Harmony harmony)`
  - `static void GogglesPatch.OnLongPress(IReadOnlyList<ToggleTarget> targets)` — **이 Task 에서는 임시로 목록의 두 번째를 여닫는다.** Task 3 이 휠로 바꾼다.

prefix 가 물러나는 조건(설계 §6-2): ① COTI 수정키가 눌려 있으면 `return true` ② `PressWatcher.IsHeldNow()` 가 false 면(코드가 부른 호출 — UI Fixes 되부름) 기본 대상을 즉시 여닫고 `return false`. 그 밖에는 코루틴으로 키를 보며 기다리다 짧게/길게를 가른다.

COTI 는 `Coti.Client.CotiPowerToggle.ModifierHeld` 를 리플렉션으로 찾는다. 못 찾으면 그 검사는 없는 것으로 친다.

- [ ] **Step 1: csproj·Plugin·HeadwearScanner·PressWatcher·GogglesPatch 를 쓴다**

`Awake()` 에서 설정을 읽고, F12 토글 하나(`"일반"`/`"휠 쓰기"`, 기본 true)를 바인딩하고, `Settings.Enabled` 가 false 면 패치를 걸지 않고 로그 한 줄만 남긴다. `TryApply` 가 `AccessTools.Method(typeof(Player), "ToggleGoggles")` 를 못 찾으면 false 를 돌려주고 `Active` 는 false 로 남는다.

- [ ] **Step 2: 빌드하고 순수 시험이 그대로인지 본다**

Run: `dotnet build src/GogglesWheel -c Release && dotnet test src/GogglesWheel.Tests -v minimal`
Expected: 빌드 경고 0·오류 0, 시험 전부 PASS.

- [ ] **Step 3: 빈 목록 시험을 더한다** — Review Focus 1·5

`src/GogglesWheel.Tests/TargetPickerTests.cs` 에 `Default_target_on_empty_list_is_minus_one` 이 이미 있다. 여기서는 **호출 쪽**을 고정한다: `GogglesPatch` 의 분기 판단만 순수 함수로 떼어 `static GogglesDecision Decide(IReadOnlyList<ToggleTarget> targets, bool keyHeld, bool cotiModifierHeld)` 를 만들고(`enum GogglesDecision { StandDown, DoNothing, ToggleDefaultNow, WatchForLongPress }`) 시험한다.

```csharp
[Fact] public void Coti_modifier_stands_down()                 // cotiModifierHeld → StandDown
[Fact] public void Empty_target_list_does_nothing()            // 빈 목록 → DoNothing
[Fact] public void Single_target_toggles_immediately()         // 1개 → ToggleDefaultNow
[Fact] public void Programmatic_call_toggles_immediately()     // 2개 + keyHeld false → ToggleDefaultNow
[Fact] public void Held_key_with_two_targets_watches()         // 2개 + keyHeld true → WatchForLongPress
```

- [ ] **Step 4: 실패 → 구현 → 통과**

Run: `dotnet test src/GogglesWheel.Tests -v minimal`
Expected: 먼저 FAIL(`Decide` 없음), 구현 뒤 PASS 전부. `Decide` 는 순수 파일(`GogglesDecision.cs`)에 두고 시험 프로젝트에 링크한다.

- [ ] **Step 5: 설치 스크립트를 쓰고 설치한다**

`tools/install-goggleswheel.ps1` 은 `tools/install-dynamicmapsfactionmarkers.ps1` 과 같은 틀(`-Remove`, **이미 있는 `config.jsonc` 는 덮어쓰지 않는다**).
Run: `pwsh -File tools/install-goggleswheel.ps1`
Expected: `BepInEx\plugins\GogglesWheel\` 에 DLL + `config.jsonc`.

- [ ] **Step 6: 레이드에서 확인한다** (사용자에게 요청 — 게임만 다시 켠다)

확인: ① 짧게 N → 야시경만 여닫힌다 ② 길게 N → (임시로) 페이스실드가 여닫힌다 ③ 왼손에 나침반을 들고 N → 나침반을 치운 뒤 야시경이 여닫히고 휠 대기가 걸리지 않는다 ④ 야시경만 단 헬멧에서 N 이 즉시 반응한다 ⑤ 로그에 예외 0건.

- [ ] **Step 7: 커밋**

```bash
git -C E:\Spt_Mods add src/GogglesWheel src/GogglesWheel.Tests tools/install-goggleswheel.ps1
git -C E:\Spt_Mods commit -m "feat: N 을 짧게·길게로 가른다"
```

---

### Task 3: 휠과 시점 차단

**Files:**
- Create: `src/GogglesWheel/WheelView.cs`
- Create: `src/GogglesWheel/LookBlocker.cs`
- Modify: `src/GogglesWheel/GogglesPatch.cs` (`OnLongPress` 를 휠로)

**Interfaces:**
- Consumes: Task 1 의 `TargetPicker.AngleForIndex`·`PickByDirection`, Task 2 의 `ToggleTarget` 목록과 `HeadwearScanner.Toggle`.
- Produces:
  - `static void WheelView.Open(IReadOnlyList<ToggleTarget> targets)` / `static void WheelView.Close(bool applySelection)` / `static bool WheelView.IsOpen`.
  - `static void LookBlocker.Block()` / `static void LookBlocker.Release()` — Look·Turn 축만 0 으로 누른다. `Release` 는 여러 번 불러도 안전하다.

휠은 `GesturesMenu._gestureItemTemplate` 을 찾아 복제해 우리 캔버스에 담는다(설계 §6-4). `Show()` 는 부르지 않고 라벨만 직접 넣는다. 템플릿을 못 찾거나 `UseGameWheelSkin` 이 false 면 우리가 그린 단순한 원형으로 떨어진다. 선택은 마우스 이동 누적 → `TargetPicker.PickByDirection`, 하이라이트는 우리가 민다.

- [ ] **Step 1: `LookBlocker` 를 쓴다** — `Block`/`Release` 가 짝이 맞게. `Release` 는 멱등.

- [ ] **Step 2: `WheelView` 를 쓰고 `OnLongPress` 를 휠로 바꾼다**

N을 떼면 `Close(applySelection: true)` → 고른 것이 있으면 `HeadwearScanner.Toggle`. 레이드 종료·사망·화면 전환에서는 `Close(false)`. **어느 경로로 끝나든 `try/finally` 로 `LookBlocker.Release()`.** 휠을 띄우다 예외가 나면 기본 대상을 여닫고 접는다(설계 §6-6).

- [ ] **Step 3: 빌드·시험·설치**

Run: `dotnet build src/GogglesWheel -c Release && dotnet test src/GogglesWheel.Tests -v minimal && pwsh -File tools/install-goggleswheel.ps1`
Expected: 빌드 오류 0, 시험 전부 PASS.

- [ ] **Step 4: 레이드에서 확인한다** (사용자에게 요청)

설계 §7 의 11개를 그대로 본다. 특히 ④ **시점은 안 돌고 이동은 된다** ⑤ 중앙에 둔 채 떼면 아무 일도 없다 ⑦ 휠을 띄운 채 죽어도 입력이 잠기지 않는다 ⑨ COTI 수정키+N 은 COTI 만 ⑩ 제스처 휠이 멀쩡하다.

- [ ] **Step 5: 커밋**

```bash
git -C E:\Spt_Mods add src/GogglesWheel
git -C E:\Spt_Mods commit -m "feat: 길게 누르면 원형 메뉴에서 고른다"
```

---

### Task 4: 문서

**Files:**
- Create: `src/GogglesWheel/README.md`
- Modify: `doc/INSTALLED-MODS.md`(생성 스크립트로), `tools/gen-installed-mods.mjs`, `doc/WORKLOG.md`

- [ ] **Step 1: `README.md`** — 무엇을 하는지, 짧게/길게, `config.jsonc` 항목, 물러나는 두 조건(UI Fixes·COTI), 게임이 바뀌면 비활성으로 떨어진다는 것.

- [ ] **Step 2: 설치 목록을 다시 만든다**

Run: `node tools/gen-installed-mods.mjs`
Expected: `doc/INSTALLED-MODS.md` 의 "다른 경로로 설치된 것" 표에 우리 줄 하나가 는다(생성 스크립트에 줄을 더한 뒤).

- [ ] **Step 3: WORKLOG 에 한 단락** — 왜 필요했고(제한 해제 모드 때문에 드러난 `FirstOrDefault` 구멍), 리뷰에서 전제 다섯 개가 뒤집힌 것.

- [ ] **Step 4: 커밋**

```bash
git -C E:\Spt_Mods add src/GogglesWheel/README.md doc/INSTALLED-MODS.md doc/WORKLOG.md tools/gen-installed-mods.mjs
git -C E:\Spt_Mods commit -m "docs: 야시경·실드 휠 문서"
```
