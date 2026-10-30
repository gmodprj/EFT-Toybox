# DynamicMapsFactionMarkers 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Dynamic Maps 의 지도·미니맵에서 모드로 추가된 세력(ISB 16역할)을 역할 묶음 8가지의 색·모양으로 갈라 보이게 한다.

**Architecture:** Dynamic Maps 파일은 고치지 않는다. 새 BepInEx 플러그인이 Harmony 로 `OtherPlayersMarkerProvider` 의 private 메서드 세 지점에 끼어들어, 우리 역할이면 우리 색·모양으로 마커를 만들고 **그들의 마커 사전에 넣는다**(제거·맵 전환·사망 처리를 그들 코드가 계속 맡는다). 판정 로직(설정 읽기·역할 분류·색 파싱)은 게임 어셈블리를 모르는 순수 파일로 떼어 내 xunit 으로 시험한다.

**Tech Stack:** C# / net472 / BepInEx 5 / HarmonyX / Newtonsoft.Json(게임 번들) · 시험은 net8.0 + xunit

**Spec:** [doc/specs/2026-10-30-dynamicmaps-faction-markers-design.md](../specs/2026-10-30-dynamicmaps-faction-markers-design.md) (실측 근거는 [같은 폴더의 survey 문서](../specs/2026-10-30-faction-identification-survey.md))

## Global Constraints

- `TargetFramework` 은 `net472`, `DebugType` 은 `none`, `AppendTargetFrameworkToOutputPath` 는 false. 설치 경로는 `<SptDir Condition="'$(SptDir)' == ''">E:\Spt</SptDir>` 로 두고 `-p:SptDir=` 로 덮어쓸 수 있게 한다 (`src/GridWindowScroll/GridWindowScroll.csproj` 와 같은 틀).
- 플러그인 GUID `spt.dynamicmapsfactionmarkers`, 표시 이름 `모드 세력 지도 표시`, 버전 `0.1.0`.
- Dynamic Maps 는 **하드 의존**: `[BepInDependency("com.mpstark.dynamicmaps", BepInDependency.DependencyFlags.HardDependency)]`. 없으면 BepInEx 가 우리 플러그인을 조용히 건너뛴다.
- MoreBotsAPI 는 **소프트 의존**: 타입 이름(`MoreBotsAPI.CustomWildSpawnTypeManager`)으로 리플렉션해 찾고, 없으면 설정의 `extraRoles` 만 쓴다.
- 주석·로그·설정 설명은 **한국어**. 사용자에게 보이는 범주 이름은 서버 locale 값(영문 `ISB BOSS` 등)을 그대로 쓴다.
- 대상 버전은 Dynamic Maps **1.2.1**. 핵심 패치 지점을 못 찾으면 전체 비활성(spec §6-5).
- **git**: 같은 작업 트리를 쓰는 옆 세션(AirdropCaller)이 git 을 쥐고 있다. 각 Task 의 커밋 단계는 **그 세션이 끝났다고 알린 뒤** `feat/dynamicmaps-faction-markers` 브랜치에서 몰아 한다. 그때까지 `commit`·`checkout`·`merge` 금지 — 작업은 미커밋으로 쌓는다.
- 레이드 확인이 필요한 Task(4·5)는 사용자가 게임을 켜 줘야 한다. 서버는 끄지 않는다.

## Review Focus

이 다섯 가지는 spec 이 명시하지 않았지만 틀리면 사용자를 문다. 각 줄의 시험은 그 코드를 가진 Task 에 넣었다.

1. `config.jsonc` 가 깨졌을 때 — 기본값으로 떨어지고 지도는 그대로 돌아야 한다(Task 1).
2. `colorHex` 가 `"red"`·`"#GG0000"`·`""` 처럼 색이 아닐 때 — 그 묶음만 기본색으로, 예외 없이(Task 1).
3. 같은 `scavRole` 이 두 줄 이상일 때 — 먼저 적힌 줄이 이기고 예외 없이(Task 2).
4. 커스텀 역할이 팔레트 색보다 많을 때 — 색을 돌려 쓰고 예외 없이(Task 2).
5. 서버 locale 을 못 읽었을 때 — `ScavRole` 키 문자열이 범주 이름이 되고 마커는 뜬다(Task 2).

---

### Task 1: 설정과 스타일 (순수)

**Files:**
- Create: `src/DynamicMapsFactionMarkers/MarkerConfig.cs`
- Create: `src/DynamicMapsFactionMarkers/MarkerStyle.cs`
- Create: `src/DynamicMapsFactionMarkers/config.jsonc`
- Create: `src/DynamicMapsFactionMarkers.Tests/DynamicMapsFactionMarkers.Tests.csproj`
- Test: `src/DynamicMapsFactionMarkers.Tests/MarkerConfigTests.cs`

**Interfaces:**
- Consumes: 없음(첫 Task).
- Produces:
  - `sealed class MarkerConfig { bool Enabled; string IntelLevelSource; bool AutoAssignUnknown; List<MarkerGroupConfig> Groups; List<int> ExtraRoles; }`
  - `sealed class MarkerGroupConfig { string ScavRole; string Label; string ColorHex; string Shape; }`
  - `static MarkerConfig MarkerConfig.LoadFromText(string jsonc, out string warning)` — 깨졌으면 `Default()` 를 돌려주고 `warning` 에 이유를 담는다.
  - `static MarkerConfig MarkerConfig.Default()` — spec §6-3 의 8묶음을 기본값으로 담는다.
  - `readonly struct MarkerStyle { float R; float G; float B; string IconPath; }`
  - `static bool MarkerStyle.TryParseColor(string hex, out float r, out float g, out float b)`
  - `static string MarkerStyle.IconPathForShape(string shape)` — `star`→`Markers/star.png`, `arrow`→`Markers/arrow.png`, `dot`→`Markers/dot.png`, `diamond`→`../DynamicMapsFactionMarkers/diamond.png`, 그 밖이면 arrow.

시험 프로젝트는 `src/CompatToggle.Tests/CompatToggle.Tests.csproj` 와 같은 틀이다: `net8.0`, xunit 2.9.2, `EnableDefaultCompileItems` false, **게임 어셈블리를 참조하지 않고** 위 두 순수 파일만 `<Compile Include="..\DynamicMapsFactionMarkers\MarkerConfig.cs" />` 식으로 링크한다. Newtonsoft.Json 은 시험 프로젝트에서만 PackageReference(`13.0.3`)로 받는다.

- [ ] **Step 1: 실패하는 시험을 쓴다** — `src/DynamicMapsFactionMarkers.Tests/MarkerConfigTests.cs`

```csharp
[Fact] public void Default_has_eight_groups_with_white_tusk_merged()
    // Default().Groups 에 scavRole "ISBWhiteTusk","ISBCommander","ISBBossFollower" 세 줄이 있고
    // 셋의 ColorHex 가 모두 "#FFFFFF", Shape 가 모두 "star"
[Fact] public void Default_isb_boss_is_red_star()        // "ISBBoss" → "#FF2D55", "star"
[Fact] public void Default_sniper_is_cyan_diamond()      // "ISBSniper" → "#2EC8FF", "diamond"
[Fact] public void Default_intel_source_is_boss()        // IntelLevelSource == "boss"
[Fact] public void Default_auto_assign_unknown_is_true() // AutoAssignUnknown
[Fact] public void LoadFromText_broken_json_falls_back_to_default_with_warning()
    // "{ not json" → Groups 수가 Default() 와 같고 warning 이 비어 있지 않다
[Fact] public void LoadFromText_comments_and_trailing_commas_are_accepted()
    // "// 주석\n{ \"enabled\": false, }" → Enabled == false, warning 비어 있음
[Theory] [InlineData("#FF2D55")] [InlineData("ff2d55")]
    public void TryParseColor_accepts_six_digit_hex_with_or_without_hash()  // true, r≈1.0, g≈0.176, b≈0.333
[Theory] [InlineData("red")] [InlineData("#GG0000")] [InlineData("")] [InlineData(null)]
    public void TryParseColor_rejects_non_hex(string hex)                   // false
[Fact] public void IconPathForShape_diamond_points_into_our_own_folder()
    // "../DynamicMapsFactionMarkers/diamond.png"
[Fact] public void IconPathForShape_unknown_shape_falls_back_to_arrow()
```

- [ ] **Step 2: 실패를 확인한다**

Run: `dotnet test src/DynamicMapsFactionMarkers.Tests -v minimal`
Expected: 컴파일 실패(`MarkerConfig` 없음).

- [ ] **Step 3: `MarkerConfig`·`MarkerStyle` 을 구현한다**

jsonc 는 `JsonConvert.DeserializeObject<MarkerConfig>(text, new JsonSerializerSettings { ... })` 로 읽고, 주석·끝쉼표는 `JsonTextReader`/`JsonLoadSettings` 로 허용한다(저장소의 다른 플러그인이 `config.jsonc` 를 읽는 방식과 같게). `TryParseColor` 는 6자리 16진수만 받고 0~1 로 나눈다.

- [ ] **Step 4: 시험이 통과하는지 확인한다**

Run: `dotnet test src/DynamicMapsFactionMarkers.Tests -v minimal`
Expected: PASS (전부)

- [ ] **Step 5: `config.jsonc` 를 기본값 그대로 적어 둔다** — 8묶음·`intelLevelSource`·`autoAssignUnknown`·`extraRoles` 빈 배열. 각 줄에 한국어 주석 한 줄.

- [ ] **Step 6: 커밋(보류 — Global Constraints 의 git 규칙)**

```bash
git add src/DynamicMapsFactionMarkers src/DynamicMapsFactionMarkers.Tests
git commit -m "feat: 지도 세력 표시 설정·스타일 파싱"
```

---

### Task 2: 역할 분류표 (순수)

**Files:**
- Create: `src/DynamicMapsFactionMarkers/RoleCatalog.cs`
- Test: `src/DynamicMapsFactionMarkers.Tests/RoleCatalogTests.cs`
- Modify: `src/DynamicMapsFactionMarkers.Tests/DynamicMapsFactionMarkers.Tests.csproj` (`RoleCatalog.cs` 링크 추가)

**Interfaces:**
- Consumes: Task 1 의 `MarkerConfig`·`MarkerGroupConfig`·`MarkerStyle`.
- Produces:
  - `sealed class RoleEntry { int RoleId; string ScavRole; string Category; MarkerStyle Style; }`
  - `sealed class RoleCatalog`
    - `RoleCatalog(MarkerConfig config)`
    - `void Register(int roleId, string scavRole, string localizedLabel)` — `localizedLabel` 이 null/빈 문자열이면 `scavRole` 을 범주 이름으로 쓴다.
    - `bool TryGet(int roleId, out RoleEntry entry)` — 분류 대상이 아니면 false.
    - `bool IsOurCategory(string category)`
    - `IReadOnlyList<RoleEntry> Discovered` — 설정에 없어 자동 배정된 것만.
    - `static readonly string[] FallbackPalette` — 자동 배정 색 목록.

- [ ] **Step 1: 실패하는 시험을 쓴다** — `RoleCatalogTests.cs`

```csharp
[Fact] public void Configured_role_uses_configured_color_and_label()
    // Register(13709,"ISBBoss","ISB BOSS") → TryGet true, Category=="ISB BOSS", Style 가 #FF2D55 star
[Fact] public void Three_white_tusk_roles_share_one_category()
    // Register(13702,"ISBWhiteTusk","WHITE TUSK"), (13707,"ISBCommander","WHITE TUSK"),
    // (13708,"ISBBossFollower","WHITE TUSK") → 세 Category 가 모두 "WHITE TUSK"
[Fact] public void Missing_locale_label_falls_back_to_scav_role_key()
    // Register(13714,"ISBSniper", null) → Category=="ISBSniper", TryGet true
[Fact] public void Duplicate_scav_role_in_config_first_line_wins()
    // Groups 에 "ISBBoss" 가 두 줄(#FF2D55, #00FF00) → Style 은 #FF2D55, 예외 없음
[Fact] public void Unknown_role_gets_auto_assigned_and_is_listed_in_discovered()
    // Register(848400,"blackDivAssault","GHOST BUST.") → TryGet true, Discovered 에 1건
[Fact] public void Unknown_role_is_skipped_when_auto_assign_is_off()
    // AutoAssignUnknown=false → TryGet false, Discovered 비어 있음
[Fact] public void Extra_roles_are_classified_even_when_auto_assign_is_off()
    // ExtraRoles=[848400], AutoAssignUnknown=false → TryGet true
[Fact] public void Palette_wraps_when_more_unknown_roles_than_colors()
    // FallbackPalette.Length + 3 개를 Register → 전부 TryGet true, 예외 없음,
    // 색이 FallbackPalette 안의 값들로만 채워진다
[Fact] public void IsOurCategory_is_false_for_vanilla_categories()
    // "Scav","Boss","Enemy Player","Friendly Player" 네 개 모두 false
[Fact] public void Register_is_idempotent()
    // 같은 roleId 를 두 번 Register → Discovered 가 1건
```

- [ ] **Step 2: 실패를 확인한다**

Run: `dotnet test src/DynamicMapsFactionMarkers.Tests -v minimal`
Expected: FAIL (`RoleCatalog` 없음)

- [ ] **Step 3: `RoleCatalog` 을 구현한다**

`scavRole` → 설정 줄 사전을 만들 때 **먼저 적힌 줄만** 넣는다. 자동 배정은 `Discovered.Count % FallbackPalette.Length` 로 색을 돌려 쓰고 모양은 `arrow` 로 둔다. 바닐라 범주 네 개는 상수 배열로 들고 `IsOurCategory` 에서 제외한다.

- [ ] **Step 4: 시험이 통과하는지 확인한다**

Run: `dotnet test src/DynamicMapsFactionMarkers.Tests -v minimal`
Expected: PASS (전부)

- [ ] **Step 5: 커밋(보류)**

```bash
git add src/DynamicMapsFactionMarkers/RoleCatalog.cs src/DynamicMapsFactionMarkers.Tests
git commit -m "feat: 지도 세력 역할 분류표"
```

---

### Task 3: 플러그인 뼈대·역할 수집·설치

**Files:**
- Create: `src/DynamicMapsFactionMarkers/DynamicMapsFactionMarkers.csproj`
- Create: `src/DynamicMapsFactionMarkers/Plugin.cs`
- Create: `src/DynamicMapsFactionMarkers/RoleSource.cs`
- Create: `tools/install-dynamicmapsfactionmarkers.ps1`

**Interfaces:**
- Consumes: Task 1·2 의 `MarkerConfig`·`RoleCatalog`.
- Produces:
  - `static class Plugin` 정적 멤버: `ManualLogSource Log`, `ConfigEntry<bool> ShowFactionMarkers`, `MarkerConfig Settings`, `RoleCatalog Catalog`, `bool Active`.
  - `static int RoleSource.FillCatalog(RoleCatalog catalog)` — 등록한 역할 수를 돌려준다.
  - `static void RoleSource.WriteDiscovered(string folder, RoleCatalog catalog)` — `discovered-roles.json` 을 쓴다.

csproj 는 Task 1 의 Global Constraints 그대로에 `DynamicMaps` 참조(`$(SptDir)\BepInEx\plugins\mpstark-dynamicmaps\DynamicMaps.dll`, `<Private>false</Private>`)를 더한다. 설치 스크립트는 `tools/install-gridwindowscroll.ps1` 과 같은 틀이고 `-Remove` 를 받는다(`tools/install-scavraidwaves.ps1` 의 `-Remove` 처리를 따른다). DLL·`config.jsonc`·`diamond.png` 를 `BepInEx\plugins\DynamicMapsFactionMarkers\` 로 복사하되, **이미 있는 `config.jsonc` 는 덮어쓰지 않는다**.

역할 수집은 리플렉션이다: `MoreBotsAPI.CustomWildSpawnTypeManager.GetCustomWildSpawnTypeDict()` 를 찾아 각 항목의 `WildSpawnTypeValue`·`ScavRole` 을 꺼내고, 표시 이름은 BSG locale 에서 `"ScavRole/" + ScavRole` 키로 찾는다(못 찾으면 null 을 넘겨 Task 2 의 대체 경로를 탄다). 타입을 못 찾으면 0건을 돌려주고 `ExtraRoles` 만 등록한다.

- [ ] **Step 1: csproj·Plugin·RoleSource 를 쓴다** — `Awake()` 에서 설정을 읽고(`Path.Combine(Paths.PluginPath, "DynamicMapsFactionMarkers", "config.jsonc")`), F12 토글 하나(`"일반"`/`"모드 세력 표시"`, 기본 true)를 바인딩하고, `RoleSource.FillCatalog` 결과와 자동 배정 건수를 로그 한 줄로 남긴다. `Settings.Enabled` 가 false 면 역할 수집도 하지 않고 `Active` 를 false 로 둔 뒤 로그 한 줄만 남기고 끝낸다(뒤 Task 의 패치도 걸리지 않는다). 설정을 읽다 경고가 나오면 그 문구를 로그에 남긴다. **이 Task 에서는 Harmony 패치를 걸지 않는다.**

- [ ] **Step 2: 빌드한다**

Run: `dotnet build src/DynamicMapsFactionMarkers -c Release`
Expected: 경고 0·오류 0, `src/DynamicMapsFactionMarkers/bin/Release/DynamicMapsFactionMarkers.dll` 생성.

- [ ] **Step 3: 순수 시험이 깨지지 않았는지 확인한다**

Run: `dotnet test src/DynamicMapsFactionMarkers.Tests -v minimal`
Expected: PASS (전부)

- [ ] **Step 4: 설치한다**

Run: `pwsh -File tools/install-dynamicmapsfactionmarkers.ps1`
Expected: `BepInEx\plugins\DynamicMapsFactionMarkers\` 에 DLL + `config.jsonc` + `diamond.png` 목록이 찍힌다.

- [ ] **Step 5: 게임을 켜 메뉴까지 가서 로그를 본다** (사용자에게 요청)

확인: `E:\Spt\BepInEx\LogOutput.log` 에 우리 플러그인 로드 줄과 **역할 16건 등록** 로그가 있고 예외가 없다. `BepInEx\plugins\DynamicMapsFactionMarkers\discovered-roles.json` 이 생겼고(자동 배정이 있었다면) 내용이 읽힌다. F12 창에 토글 하나가 보인다.

- [ ] **Step 6: 커밋(보류)**

```bash
git add src/DynamicMapsFactionMarkers tools/install-dynamicmapsfactionmarkers.ps1
git commit -m "feat: 지도 세력 표시 플러그인 뼈대와 역할 수집"
```

---

### Task 4: 마커를 그리는 패치 ①②

**Files:**
- Create: `src/DynamicMapsFactionMarkers/MarkerPatches.cs`
- Create: `src/DynamicMapsFactionMarkers/diamond.png`
- Modify: `src/DynamicMapsFactionMarkers/Plugin.cs` (패치 적용과 fail-closed 판정)

**Interfaces:**
- Consumes: Task 3 의 `Plugin.Catalog`·`Plugin.ShowFactionMarkers`·`Plugin.Active`.
- Produces: `static bool MarkerPatches.TryApply(Harmony harmony)` — 핵심 지점을 다 찾아 걸었으면 true.

패치 대상(모두 `DynamicMaps.DynamicMarkers.OtherPlayersMarkerProvider`, private):
- `TryAddMarker(IPlayer)` prefix — `__instance` 의 `_lastMapView`·`_playerMarkers` 를 `Traverse` 로 꺼내고, `player.Profile.Info.Settings.Role` 의 정수값으로 `Catalog.TryGet` 을 본다. 걸리면 `mapView.AddPlayerMarker(player, entry.Category, color, entry.Style.IconPath)` 결과를 `_playerMarkers[player]` 에 넣고 `return false`. 안 걸리면 `return true`. intel 게이트는 `Settings.IntelLevelSource` 에 따라 Dynamic Maps 의 `ShowBossIntelLevel`/`ShowScavIntelLevel` 과 `GameUtils.GetIntelLevel()` 을 비교하고 `"none"` 이면 건너뛴다. 이미 `_playerMarkers` 에 있으면 아무 것도 하지 않고 `return false`.
- `ShouldShowCategory(string)` prefix — `Catalog.IsOurCategory(category)` 면 `__result = ShowFactionMarkers.Value; return false;`.

`diamond.png` 는 32×32 흰 단색 마름모(색은 런타임 tint). 바닐라가 흰 `star.png` 에 색을 입히는 방식과 같다.

- [ ] **Step 1: 패치를 쓴다** — 예외는 전부 `try/catch` 로 감싸 로그 한 줄만 남기고 `return true`(원본 흐름으로 보낸다). 대상 메서드·필드 중 하나라도 `AccessTools` 가 null 을 주면 `TryApply` 가 false 를 돌려주고 `Plugin.Active` 는 false 가 되며 "지도 세력 표시를 끈다" 로그를 남긴다.

- [ ] **Step 2: 빌드·시험·설치**

Run: `dotnet build src/DynamicMapsFactionMarkers -c Release && dotnet test src/DynamicMapsFactionMarkers.Tests -v minimal && pwsh -File tools/install-dynamicmapsfactionmarkers.ps1`
Expected: 빌드 오류 0, 시험 전부 PASS, 설치 목록에 `diamond.png` 포함.

- [ ] **Step 3: 레이드에서 확인한다** (사용자에게 요청 — 게임만 다시 켠다)

확인:
1. ISB 가 나오는 맵에서 지도(M)와 미니맵 둘 다에 ISB 가 spec §6-3 의 색·모양으로 뜬다. 스크린샷을 남긴다.
2. 한 ISB 에 마커가 **하나만** 붙는다(주황 화살표가 겹쳐 있지 않다).
3. 바닐라 보스(킬라·타길라)는 그대로 주황 ★ 다.
4. ISB 를 죽이면 마커가 사라진다.
5. `LogOutput.log` 에 우리 플러그인 예외 0건.

- [ ] **Step 4: 커밋(보류)**

```bash
git add src/DynamicMapsFactionMarkers
git commit -m "feat: 지도에 모드 세력 마커를 그린다"
```

---

### Task 5: 등록 강제와 토글 즉시 반영

**Files:**
- Modify: `src/DynamicMapsFactionMarkers/MarkerPatches.cs` (패치 ③)
- Create: `src/DynamicMapsFactionMarkers/MapScreenBridge.cs`
- Modify: `src/DynamicMapsFactionMarkers/Plugin.cs` (토글 `SettingChanged` 연결)

**Interfaces:**
- Consumes: Task 4 의 `MarkerPatches`.
- Produces:
  - `static void MapScreenBridge.RefreshNow()` — `ModdedMapScreen.Instance` → private `_dynamicMarkerProviders` 에서 `OtherPlayersMarkerProvider` 를 꺼내 public `RefreshMarkers()` 를 부른다. 못 찾으면 아무 것도 하지 않는다.
  - `static bool MapScreenBridge.Available` — 위 경로를 찾았는지.

패치 ③ 은 `ModdedMapScreen.ReadConfig()` postfix 다. `Plugin.ShowFactionMarkers.Value` 가 true 면 `AddRemoveMarkerProvider<OtherPlayersMarkerProvider>(true)` 를 부른다(제네릭이라 `MakeGenericMethod` 로 호출). spec §6-5 의 **곁가지** 규칙: `ReadConfig` 나 `Instance`·`_dynamicMarkerProviders`·`RefreshMarkers` 를 못 찾아도 전체를 끄지 않고 그것만 포기하며, 로그에 "바닐라 마커 네 개 중 하나는 켜 두어야 ISB 가 보인다" / "토글은 다음 맵 열기부터 적용된다" 를 남긴다.

- [ ] **Step 1: 패치 ③ 과 `MapScreenBridge` 를 쓰고, 토글 `SettingChanged` 에서 `RefreshNow()` 를 부른다**

- [ ] **Step 2: 빌드·시험·설치**

Run: `dotnet build src/DynamicMapsFactionMarkers -c Release && dotnet test src/DynamicMapsFactionMarkers.Tests -v minimal && pwsh -File tools/install-dynamicmapsfactionmarkers.ps1`
Expected: 빌드 오류 0, 시험 전부 PASS.

- [ ] **Step 3: 레이드에서 확인한다** (사용자에게 요청)

확인:
1. 레이드 중 F12 토글을 끄면 **지도를 다시 열지 않고** ISB 마커만 사라지고 스캐브 마커는 남는다. 다시 켜면 돌아온다.
2. Dynamic Maps 의 바닐라 마커 네 개(아군·적 PMC·보스·스캐브)를 **전부 끈** 상태에서도 ISB 만 뜬다.
3. 그 상태에서 바닐라 마커를 다시 켜도 ISB 마커가 중복되지 않는다.
4. `LogOutput.log` 에 예외 0건.

- [ ] **Step 4: 커밋(보류)**

```bash
git add src/DynamicMapsFactionMarkers
git commit -m "feat: 마커 담당 등록을 보장하고 토글을 즉시 반영한다"
```

---

### Task 6: 문서

**Files:**
- Create: `src/DynamicMapsFactionMarkers/README.md`
- Modify: `doc/INSTALLED-MODS.md` (직접 고치지 않고 생성 스크립트로)
- Modify: `doc/WORKLOG.md`

- [ ] **Step 1: `README.md` 를 쓴다** — 무엇을 하는지, `config.jsonc` 항목, 색·모양 기본값 표, Dynamic Maps 버전이 올라가면 비활성으로 떨어진다는 것, `discovered-roles.json` 쓰는 법.

- [ ] **Step 2: 설치 목록을 다시 만든다**

Run: `node tools/gen-installed-mods.mjs`
Expected: `doc/INSTALLED-MODS.md` 의 "다른 경로로 설치된 것" 표에 우리 플러그인 줄이 생긴다(생성 스크립트가 자작 플러그인을 모르면 그 스크립트에 줄을 더한다 — 저장소 규칙상 `INSTALLED-MODS.md` 를 손으로 고치지 않는다).

- [ ] **Step 3: WORKLOG 에 한 단락 더한다** — 무엇을 왜 만들었고, Dynamic Maps 의 보스 목록이 하드코딩이라 패치가 필요했다는 것.

- [ ] **Step 4: 커밋(보류)**

```bash
git add src/DynamicMapsFactionMarkers/README.md doc/INSTALLED-MODS.md doc/WORKLOG.md tools/gen-installed-mods.mjs
git commit -m "docs: 지도 세력 표시 플러그인 문서"
```
