# TraderListView 구현 계획

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 상인 화면 구매 모드에 격자↔목록 전환·정렬(가격·이름·재고·등급)을 넣고, EDIT ASSORTS 와 검색창 겹침을 없애고, 목록에 플리마켓 겉모습을 입힌다.

**Architecture:** BepInEx 클라이언트 플러그인 하나. 정렬·가격 환산·표시 문자열은 EFT 를 모르는 순수 파일로 두고 net8 xunit 으로 시험한다. 게임 쪽은 Harmony 패치 셋(상인 격자 정렬 자리, 상인 화면 Show, 선택 변경 구독)과 목록 UI(가상 스크롤)로 붙는다. 구매는 오직 `Assortment.SelectItem` → 원래 구매 창.

**Tech Stack:** C# net472, BepInEx 5, HarmonyX, Unity UI(uGUI)·TextMeshPro, 시험은 net8.0 + xunit 2.9.2.

**Spec:** [doc/specs/2026-11-18-trader-list-view-design.md](../specs/2026-11-18-trader-list-view-design.md) — 실행자는 설계 문서를 함께 읽는다.

## Global Constraints

- 구매는 **오직** `Assortment.SelectItem(item)` → 원래 구매 창. 목록에 "구매" 버튼 없음.
- 목록 집합 = 상인 격자의 `FilterPanel.GetFilteredItems(_items)` (격자와 같은 집합). 목록만의 필터 없음.
- 정렬은 상인 쪽 `TradingGridView` 의 `ApplyFilter` 정렬 자리에서 바꾼다. 판매 쪽 `_stashGridView` 는 건드리지 않는다.
- 가격 비교 = 루블 환산. 통화는 그 통화 아이템의 **핸드북 가격** × 금액(루블 핸드북 = 1). 물물교환은 재료 핸드북 가격 × 개수 합. 핸드북에 없는 재료가 있으면 **정렬 방향과 상관없이 맨 뒤**. 같으면 이름, 그다음 게임 기본 순서.
- 헤더: `[상점 새로고침][검색창(남은 폭)][✎][▦|☰]`. EDIT ASSORTS(`UTAE Edit Assorts Button`)는 글자를 지우고 검색창 높이의 정사각형 연필 버튼. 못 찾으면 아무것도 안 함.
- Assort Editor 편집 중(`TraderEditorController.IsEditing == true`)에는 목록을 숨기고 격자로.
- 판매 모드에서는 목록·정렬 막대를 숨긴다.
- 보기 방식·정렬은 BepInEx 설정 하나로 전체 공유(상인별 아님). 설정 키 이름은 영어, 화면 글자는 한글.
- 수천 줄(AIOTrader)에서 멈추지 않는다 — 목록은 보이는 줄만 만든다.
- 플러그인 GUID `com.spt-mods.traderlistview`, 이름 "상인 목록 보기", 버전 0.1.0. 소프트 의존: `com.maschine.TraderSearch`, `com.goatboy.ultimate-trader-assort-editor`(없어도 돈다).

## Review Focus

1. **목록이 빈 상인(신뢰 등급 탭에 물건 0개)** — 빈 목록에 "물건이 없습니다" 한 줄, 예외 없음. → Task 1 `Sort_EmptyInput_ReturnsEmpty`, Task 5 확인 단계.
2. **같은 물건이 여러 번(같은 템플릿, 다른 offer)** — 각각 따로 줄, 선택은 그 `Item` 인스턴스. → Task 1 `Sort_DuplicateNames_StableByDefaultIndex`.
3. **물물교환 재료 중 하나만 핸드북에 없음** — 전체가 "가격 모름"으로 맨 뒤(오름·내림 모두). → Task 2 `Barter_WithUnknownPart_IsNull`, Task 1 `Sort_UnknownPrice_LastInBothDirections`.
4. **정렬 중 상점 새로고침·재고 변동(`AssortmentUpdated`)** — 목록·선택이 새 집합으로 다시 그려지고, 사라진 물건이 선택돼 있으면 선택 표시를 지운다. → Task 5 확인 단계.
5. **Trader Search·Assort Editor 가 없는 설치** — 헤더 배치는 있는 것만, 목록·정렬은 그대로 동작. → Task 4 확인 단계.

---

## 파일 구조

| 파일 | 책임 | EFT 의존 |
|---|---|---|
| `src/TraderListView/TraderListView.csproj` | net472, 참조는 `src/GogglesWheel/GogglesWheel.csproj` 꼴 + `UnityEngine.UI.dll`, `Unity.TextMeshPro.dll` | — |
| `src/TraderListView/OfferRow.cs` | 정렬·표시에 쓰는 한 줄의 값(순수) | 없음 |
| `src/TraderListView/SortSpec.cs` | `SortKey` 열거, `SortSpec`(키·방향), 설정 문자열 왕복 | 없음 |
| `src/TraderListView/TraderSorter.cs` | 정렬 규칙 | 없음 |
| `src/TraderListView/PriceNormalizer.cs` | 루블 환산 | 없음 |
| `src/TraderListView/PriceText.cs` | 가격·재고·등급 표시 문자열 | 없음 |
| `src/TraderListView/Plugin.cs` | 진입점·설정·패치 적용 | 있음 |
| `src/TraderListView/OfferRowFactory.cs` | `Item`+`Assortment` → `OfferRow` | 있음 |
| `src/TraderListView/Patches/TraderGridSortPatch.cs` | `ApplyFilter` 정렬 자리 + 목록 집합 공급 | 있음 |
| `src/TraderListView/Patches/TraderScreenShowPatch.cs` | 헤더 배치, 목록 붙이기, 모드 전환 | 있음 |
| `src/TraderListView/Ui/HeaderLayout.cs` | 헤더 재배치·전환 버튼·정렬 막대 | 있음 |
| `src/TraderListView/Ui/OfferListView.cs` | 가상 스크롤 목록·열 머리·선택 | 있음 |
| `src/TraderListView/Ui/FleaSkin.cs` | 플리 겉모습 빌리기(C) | 있음 |
| `src/TraderListView.Tests/*` | 순수 파일 링크 컴파일 시험 | 없음 |
| `tools/install-traderlistview.ps1` | 설치·제거(`install-goggleswheel.ps1` 꼴, config 없음) | — |

---

### Task 1: 정렬 규칙 (순수)

**Files:**
- Create: `src/TraderListView/TraderListView.csproj`, `OfferRow.cs`, `SortSpec.cs`, `TraderSorter.cs`
- Create: `src/TraderListView.Tests/TraderListView.Tests.csproj`(GogglesWheel.Tests 꼴, 순수 파일만 링크), `TraderSorterTests.cs`, `SortSpecTests.cs`

**Interfaces:**
- Produces:
  - `public sealed class OfferRow { string Name; double? RubPrice; long Stock; int LoyaltyLevel; bool Locked; int DefaultIndex; object Tag; }` — `Tag` 에 EFT `Item` 을 담는다(순수 쪽은 모름). 생성자는 전부 받는 하나.
  - `public enum SortKey { Default, Price, Name, Stock, Loyalty }`
  - `public readonly struct SortSpec { SortKey Key; bool Descending; string ToConfig(); static SortSpec Parse(string s); }` — 형식 `"Price:Desc"`/`"Name:Asc"`, 못 읽으면 `Default:Asc`.
  - `public static class TraderSorter { static List<OfferRow> Sort(IEnumerable<OfferRow> rows, SortSpec spec); }`

- [ ] **Step 1: 시험 쓰기** — `TraderSorterTests`:
  - `Sort_Default_KeepsDefaultIndexOrder`(DefaultIndex 2,0,1 → 0,1,2)
  - `Sort_PriceAsc_ThenNameThenDefaultIndex`(가격 같으면 이름 오름, 이름도 같으면 DefaultIndex)
  - `Sort_PriceDesc_ReversesPriceOnly`(가격만 뒤집히고 동률 처리는 그대로 오름)
  - `Sort_UnknownPrice_LastInBothDirections`(RubPrice null 인 줄이 Asc·Desc 모두 맨 뒤, null 끼리는 이름순)
  - `Sort_Name_IsCaseInsensitiveOrdinal`("ak" == "AK" 동률 → DefaultIndex)
  - `Sort_Stock_Desc`, `Sort_Loyalty_Asc`
  - `Sort_EmptyInput_ReturnsEmpty`, `Sort_DuplicateNames_StableByDefaultIndex`
  - `SortSpecTests`: `"Price:Desc"` 왕복, `"garbage"` → `Default:Asc`, `null` → `Default:Asc`.
- [ ] **Step 2: 실행해 실패 확인** — `dotnet test src/TraderListView.Tests` → 컴파일 실패(타입 없음).
- [ ] **Step 3: 구현** — 위 시그니처. 비교는 키 비교(방향 적용) → 이름(`StringComparer.OrdinalIgnoreCase`, 방향 미적용) → DefaultIndex. 가격 키에서 null 은 방향 적용 전에 뒤로.
- [ ] **Step 4: 실행해 통과 확인** — `dotnet test src/TraderListView.Tests` → 시험 개수 전부 Passed(요약줄이 아니라 총 개수를 본다).
- [ ] **Step 5: 커밋** — `feat(traderlistview): 정렬 규칙과 시험`

### Task 2: 가격 환산·표시 문자열 (순수)

**Files:**
- Create: `src/TraderListView/PriceNormalizer.cs`, `PriceText.cs`
- Test: `src/TraderListView.Tests/PriceNormalizerTests.cs`, `PriceTextTests.cs`

**Interfaces:**
- Consumes: 없음.
- Produces:
  - `public readonly struct PricePart { string TemplateId; double Count; }`
  - `public static class PriceNormalizer { static double? ToRubles(IReadOnlyList<PricePart> parts, Func<string, double?> handbookPrice); }` — 통화 한 줄이든 물물교환 여러 줄이든 같은 길: Σ(handbook × count). 하나라도 핸드북에 없으면 null. `parts` 가 비면 null.
  - `public static class PriceText { const string RubTpl = "5449016a4bdc2d6f028b456f", UsdTpl = "5696686a4bdc2da3298b456a", EurTpl = "569668774bdc2da2298b4568"; static string Price(IReadOnlyList<PricePart> parts, Func<string,string> shortName); static string Stock(long stack, int? restrictionMax, int? restrictionCurrent); static string Loyalty(int level, bool locked); }`

- [ ] **Step 1: 시험 쓰기**
  - `Rub_IsAmount`(RUB 핸드북 1, 101개 → 101)
  - `Usd_UsesHandbook`(USD 핸드북 140, 10 → 1400)
  - `Barter_SumsParts`(A 1000×2 + B 500×1 → 2500)
  - `Barter_WithUnknownPart_IsNull`, `Empty_IsNull`
  - `PriceText`: RUB 101 → `"101 ₽"`, USD 10 → `"10 $"`, EUR 10 → `"10 €"`, 천 단위 공백 `"6 000 ₽"`(게임 `FormatPrice` 와 같은 꼴), 물물교환 2종 → `"⇄ 물물교환 2종"`, 1종 → `"⇄ {재료 짧은 이름} ×{개수}"`
  - `Stock`: 제한 없음 999999 → `"999999"`, 제한 3 중 1 사용 → `"50 (2/3)"`
  - `Loyalty`: `(2,false)` → `"LL2"`, `(3,true)` → `"LL3 🔒"`
- [ ] **Step 2: 실패 확인** — `dotnet test src/TraderListView.Tests`
- [ ] **Step 3: 구현**
- [ ] **Step 4: 통과 확인** — 총 개수 확인.
- [ ] **Step 5: 커밋** — `feat(traderlistview): 가격 환산과 표시 문자열`

### Task 3: 플러그인 뼈대 + 격자 정렬 (게임)

**Files:**
- Create: `src/TraderListView/Plugin.cs`, `OfferRowFactory.cs`, `Patches/TraderGridSortPatch.cs`

**Interfaces:**
- Consumes: Task 1·2 전부.
- Produces:
  - `Plugin.ViewMode : ConfigEntry<string>`(`"Grid"|"List"`, 기본 `"Grid"`), `Plugin.Sort : ConfigEntry<string>`(SortSpec.ToConfig, 기본 `"Default:Asc"`), `Plugin.Log`.
  - `static class OfferRowFactory { static OfferRow Create(Item item, Assortment assortment, int defaultIndex, int currentLoyaltyLevel); }` — 가격 parts 는 `assortment.GetBarterPrice(item)`(통화 하나) 또는 그 물건의 바터 스킴(재료 여럿)에서, 핸드북 값은 `Singleton<HandbookClass>`(게임 핸드북) 에서. 이름은 게임 locale 이름. 재고는 `StackObjectsCount`, 구매 제한은 아이템의 `BuyRestrictionMax`·`BuyRestrictionCurrent`. 등급은 `assortment.LoyalLevelItems`, 잠금은 등급 > 지금 등급.
  - `static class TraderGridSortPatch { static event Action<TradingGridView, IReadOnlyList<Item>> ListApplied; static void Refresh(TraderDealScreen screen); }` — `ListApplied` 는 상인 격자가 정렬을 끝낼 때마다 (격자뷰, 정렬된 집합)을 낸다. Task 5 가 이것을 받아 목록을 다시 그린다.
- 방식(정해 둠): `TradingGridView.ApplyFilter` Prefix 에서 그 인스턴스가 `TraderDealScreen._traderGridView` 인지(`GetComponentInParent<TraderDealScreen>` 후 필드 비교) 보고 [ThreadStatic] 표지를 세운다 → `ItemSorter.Sort` Postfix 에서 표지가 있으면 `OfferRowFactory` + `TraderSorter.Sort` 로 결과를 바꾼다(`SortKey.Default` 면 그대로) → `ApplyFilter` Finalizer 에서 표지를 지우고 `ListApplied` 를 낸다. `ItemSorter.Sort` 의 정확한 오버로드는 디컴파일로 확인해 그것만 패치한다.
  - `Refresh` 는 정렬 설정이 바뀌면 `_traderGridView.ApplyFilterAndRefresh()` 를 부른다.

- [ ] **Step 1: 빌드 뼈대** — `dotnet build src/TraderListView -c Release` 가 경고 없이 성공(플러그인이 로그 한 줄 `상인 목록 보기 0.1.0 로드` 만 남김).
- [ ] **Step 2: 패치 구현** — 위 방식.
- [ ] **Step 3: 설치·확인** — Task 7 의 설치 스크립트를 먼저 만들어 설치 → 게임에서 F12 로 `Sort` 를 `Price:Asc` 로 바꾸고 AIOTrader 를 열어 격자가 싼 것부터 채워지는지, `Default:Asc` 로 돌리면 원래 순서인지, 판매 모드 내 창고 격자는 그대로인지. `LogOutput.log` 에 예외 0.
- [ ] **Step 4: 커밋** — `feat(traderlistview): 상인 격자 정렬`

### Task 4: 헤더 배치 — 겹침 해소·전환 버튼·정렬 막대 (게임)

**Files:**
- Create: `src/TraderListView/Patches/TraderScreenShowPatch.cs`, `Ui/HeaderLayout.cs`

**Interfaces:**
- Consumes: `Plugin.ViewMode`, `Plugin.Sort`, `TraderGridSortPatch.Refresh`.
- Produces:
  - `sealed class HeaderLayout : MonoBehaviour { static HeaderLayout Attach(TraderDealScreen screen); event Action<string> ViewModeChanged; void Relayout(); }` — 화면당 하나(`TraderDealScreen` 게임오브젝트에 붙임).
- 정해 둠:
  - `TraderDealScreen.Show` Postfix, `[HarmonyAfter("com.maschine.TraderSearch","com.goatboy.ultimate-trader-assort-editor")]`, `Priority.Last`. 두 모드가 배치를 다음 프레임에 다시 할 수 있으므로 `Relayout` 을 Show 직후와 프레임 끝(코루틴 `WaitForEndOfFrame` 1회) 두 번 부른다.
  - 헤더 부모 = `_updateAssort.transform.parent`. 순서: 새로고침 → 검색창(`TraderSearchField` 이름, 있으면) → 연필 버튼(`UTAE Edit Assorts Button`, 있으면) → 전환 버튼.
  - 연필 버튼: 글자(`EDIT ASSORTS`) 끄고 아이콘 층만 남김, 크기 = 검색창 높이 정사각형. 검색창 폭 = 헤더 폭 − 나머지.
  - 전환 버튼 `▦|☰`: 눌리면 `ViewMode` 를 바꾸고 `ViewModeChanged` 를 낸다. 정렬 막대(격자 모드에서만, 헤더 아래 한 줄): `기본 · 가격 · 이름 · 재고 · 등급`, 누르면 같은 키는 방향 뒤집기, 다른 키는 오름.
  - 판매 모드(`OnTradingModeChanged`)·편집 중이면 전환 버튼·정렬 막대를 숨긴다. 편집 여부는 `TraderEditorController`(같은 게임오브젝트의 컴포넌트, 타입 이름 `UltimateTraderAssortEditor.Client.Editor.TraderEditorController`)의 internal `IsEditing` 을 `AccessTools.Property` 로 읽는다. 타입이 없으면 편집 아님.

- [ ] **Step 1: 구현**
- [ ] **Step 2: 확인(게임, 스크린샷)** — 헤더 겹침 없음, 연필 버튼 눌러 편집 모드 진입·퇴장이 그대로 되는지, 정렬 막대로 격자 순서가 바뀌는지. Trader Search·Assort Editor 둘 다 있는 지금 설치에서 확인하고, 둘의 DLL 을 잠깐 `mods-disabled` 로 뺀 설치에서도 예외 0 인지(뺀 것은 바로 되돌린다).
- [ ] **Step 3: 커밋** — `feat(traderlistview): 헤더 겹침 해소와 정렬 막대`

### Task 5: 목록 보기 B (게임)

**Files:**
- Create: `src/TraderListView/Ui/OfferListView.cs`
- Modify: `Patches/TraderScreenShowPatch.cs`(목록 붙이기·모드 전환 연결)

**Interfaces:**
- Consumes: `TraderGridSortPatch.ListApplied`, `HeaderLayout.ViewModeChanged`, `OfferRowFactory`, `PriceText`, `Plugin.Sort`.
- Produces: `sealed class OfferListView : MonoBehaviour { static OfferListView Attach(TraderDealScreen screen); void SetVisible(bool on); void SetItems(IReadOnlyList<Item> items); }` — Task 6 이 같은 클래스에 겉모습만 입힌다.
- 정해 둠:
  - 자리: `_traderScroll` 과 같은 부모·같은 앵커의 형제 RectTransform. 목록 모드면 `_traderScroll` 를 끄고 목록을 켠다(격자뷰 자체는 끄지 않는다 — FilterPanel 이 산다).
  - 열: 아이콘 | 이름 | 가격 | 재고 | 등급. 열 머리 클릭 = 정렬(Task 4 정렬 막대와 같은 규칙, 같은 `Plugin.Sort`). 지금 정렬 열에 ▲/▼.
  - 줄 클릭 → 그 줄의 `Item` 으로 `Assortment.SelectItem(item)`. 선택 표시는 `Assortment.SelectedItemChanged` 구독(그리드 클릭으로 바뀌어도 맞음). 사라진 물건이 선택이면 표시 지움.
  - 마우스 올림 → 격자와 같은 아이템 툴팁(`ItemUiContext` 경유). 물물교환 가격 칸에 마우스 → 재료 목록 툴팁.
  - 가상 스크롤: 보이는 줄 수 + 2 개만 만들어 재사용. 빈 집합이면 "물건이 없습니다".
  - 갱신 원천은 `ListApplied` 하나(필터·검색·등급 탭·새로고침이 모두 `ApplyFilter` 를 거친다).

- [ ] **Step 1: 구현**
- [ ] **Step 2: 확인(게임)** — 설계 §9-1~3: AIOTrader 목록 전환, 각 열 정렬·방향, 격자로 돌아가도 정렬 유지, 줄 클릭 → 구매 창에 같은 물건, **실제로 사서**(통화 1회·물물교환 1회) 돈·물건이 맞게 오가는지, 검색어·등급 탭 변경 시 격자와 같은 집합, 상점 새로고침 뒤 다시 그려지는지, 빈 등급 탭에서 "물건이 없습니다". 줄 수 많은 상인에서 스크롤이 끊기지 않는지(사용자 체감). 스크린샷 남김.
- [ ] **Step 3: 커밋** — `feat(traderlistview): 상인 목록 보기`

### Task 6: 플리마켓 겉모습 C (게임)

**Files:**
- Create: `src/TraderListView/Ui/FleaSkin.cs`
- Modify: `Ui/OfferListView.cs`(줄을 만들 때 스킨 적용)

**Interfaces:**
- Consumes: `OfferListView`.
- Produces: `static class FleaSkin { static bool TryLoad(); static void ApplyRow(GameObject row, bool alternate); static void ApplyHeader(GameObject header); }` — `TryLoad` 실패면 Apply 는 아무것도 안 한다(B 모양 유지).
- 정해 둠: 자원은 메뉴 UI 에 이미 올라와 있는 플리마켓 화면 부품(행 배경 이미지·구분선·글꼴·가격 색)에서 찾는다. 플리마켓을 한 번도 안 연 상태에서도 시도하고, 못 찾으면 로그 한 줄(`플리 겉모습을 못 찾아 기본 모양으로 그린다`) 남기고 B.

- [ ] **Step 1: 구현**
- [ ] **Step 2: 확인(게임)** — 설계 §9-6: 플리마켓을 열기 전·연 뒤 각각 상인 목록을 찍고, 플리마켓 목록과 나란히 사용자에게 보인다. 깨짐·예외 0.
- [ ] **Step 3: 커밋** — `feat(traderlistview): 플리마켓 겉모습`

### Task 7: 설치·기록·문서

**Files:**
- Create: `tools/install-traderlistview.ps1`(`install-goggleswheel.ps1` 꼴, 대상 `BepInEx\plugins\TraderListView\TraderListView.dll`, `-Remove` 지원, config 복사 없음), `src/TraderListView/README.md`
- Modify: `doc/HANDOVER-*` 해당 세션 인계, `doc/README.md` 모드 표(자체 모드 줄)

- [ ] **Step 1:** 설치 스크립트 — Task 3 Step 3 전에 먼저 만든다.
- [ ] **Step 2:** 설치 후 `node tools/gen-installed-mods.mjs`, `node tools/check-install-records.mjs` → `OK — 0/0`.
- [ ] **Step 3:** README(무엇을·어떻게·안 하는 것 표, 설정 키, 같이 사는 모드) 와 인계 갱신.
- [ ] **Step 4: 커밋·PR·머지** — `docs(traderlistview): 설치와 문서`
