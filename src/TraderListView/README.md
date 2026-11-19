# TraderListView — 상인 화면 목록 보기

상인 화면(구매 모드)에서 물건을 **격자 ↔ 목록**으로 바꿔 보고, **가격·이름·재고·등급**으로 줄 세운다.
EDIT ASSORTS 버튼이 검색창을 가리던 것도 고친다.

- 설계: [doc/specs/2026-11-18-trader-list-view-design.md](../../doc/specs/2026-11-18-trader-list-view-design.md)
- 계획: [doc/plans/2026-11-18-trader-list-view-plan.md](../../doc/plans/2026-11-18-trader-list-view-plan.md)

## 화면

```
[↻][✎]  [☰|⋮⋮⋮]  [검색창 ................ ✕ 🔍]  [▦][I][II][III][♛]   ← 헤더(버튼은 등급 탭과 같은 정사각형)
 정렬  기본 · 가격▲ · 이름 · 재고 · 등급            ← 격자 보기에서만
┌ 기본 │ 이름                  │ 가격        │ 재고        │ 등급 ┐  ← 목록 보기: 열 머리 클릭 = 정렬
│ [■]  │ FN SCAR-L Black Market│       1 ₽   │     999999  │ LL1  │
│ [■]  │ KA-BAR USMC 나이프    │ ⇄ 물물교환 2종│  50 (2/3)  │ LL2 (잠김)│
```

- **헤더**: 새로고침·편집(EDIT ASSORTS)·보기 방식을 등급 탭과 같은 정사각형 아이콘 버튼으로 맞추고 공통 틀(테두리 #454B4E, 바탕 #202528,
  마우스 오버 #343B3F, 활성 #477D9D)을 씌운다. 편집 버튼은 편집 중에만 활성 색, 보기 방식은 [목록|격자] 두 칸 묶음에서 지금 쪽이 활성 색.
  격자 그림은 3×3 — 등급 탭의 2×2(▦)와 헷갈리지 않게. 검색창은 남은 폭을 쓰고, 포커스면 테두리가 활성 색, 검색어가 있으면 ✕ 지우기.
  아이콘 버튼에는 0.5초 뒤 툴팁. 왕관(♛)은 게임의 최고 신뢰 등급 탭이라 그대로 둔다.
- **목록 줄**: 한 번 누르면 구매 창, 두 번 누르면 아이템 상세 창, 우클릭은 문맥 메뉴(상세정보·조사 등) — 격자와 같다.
- **조사하지 않은 물건**: 격자처럼 이름은 "알 수 없는 아이템", 아이콘은 검은 실루엣. 이름순 정렬·툴팁도 이름을 감춘다.
  우클릭 → 조사하는 동안 줄 아래에 진행 막대가 차오르고(격자 칸의 막대처럼, 조사 시간만큼), 끝나면 그 줄이 바로 원래 이름·아이콘으로 바뀐다.

## 하는 것 / 안 하는 것

| 하는 것 | 안 하는 것 |
| --- | --- |
| 목록 줄을 누르면 `Assortment.SelectItem` — 오른쪽 **원래 구매 창**에 그 물건이 뜬다 | 목록에서 직접 사지 않는다(구매 버튼 없음) |
| 목록 집합 = 상인 격자에 지금 보이는 것(등급 탭·카테고리·Trader Search 검색어 그대로) | 목록만의 필터를 따로 두지 않는다 |
| 정렬을 상인 격자의 `ApplyFilter` 정렬 자리에서 바꿔 **격자에도** 먹인다 | 판매 쪽 내 창고 격자는 건드리지 않는다 |
| 가격 비교는 루블 환산(통화·물물교환 재료의 **핸드북 가격**). 모르는 것은 늘 맨 뒤 | 상인 데이터(가격·재고)를 바꾸지 않는다 |
| EDIT ASSORTS 를 연필 아이콘만 남겨 검색창 오른쪽에 둔다 | Assort Editor·Trader Search 설정을 바꾸지 않는다 |
| 목록 줄에 플리마켓 매물 줄의 바탕·테두리·글꼴을 빌린다(못 찾으면 기본 모양) | 플리마켓 화면을 바꾸지 않는다 |

- 격자 정렬은 "대체로 그 순서"다 — 격자는 왼쪽 위부터 빈칸을 채우므로 크기가 다른 물건이 앞의 빈칸에 낀다. 정확한 순서는 목록에서 본다.
- Assort Editor 편집 중과 판매 모드에서는 목록·정렬 막대를 숨기고 격자로 돌아간다.
- 글꼴에 없는 기호(🔒 ⇄ ▲ ▼ ▦ ☰)는 글자로 바꿔 그린다(`GlyphFallback`).

## 설정 (F12, `com.spt-mods.traderlistview.cfg`)

| 키 | 값 | 기본 |
| --- | --- | --- |
| `General.ViewMode` | `Grid` · `List` | `Grid` |
| `General.Sort` | `<키>:<방향>` — 키 `Default`·`Price`·`Name`·`Stock`·`Loyalty`, 방향 `Asc`·`Desc` | `Default:Asc` |

상인마다가 아니라 전체에 하나다.

## 같이 사는 모드

Trader Search(`com.maschine.TraderSearch`)·Ultimate Trader Assort Editor(`com.goatboy.ultimate-trader-assort-editor`)는
소프트 의존이다 — 없어도 돈다. OpenBarters·WishlistExtended·IOF·CaliberUnderName 이 격자 아이템에 덧그리는 표시는
격자 보기에서만 보인다.

## 빌드·시험·설치

```
dotnet test src/TraderListView.Tests          # 순수 로직(정렬·가격 환산·표시 글자·기호 대체)
dotnet build src/TraderListView -c Release
pwsh -File tools/install-traderlistview.ps1   # BepInEx\plugins\TraderListView\ — 게임을 다시 켜야 적용
pwsh -File tools/install-traderlistview.ps1 -Remove
```
