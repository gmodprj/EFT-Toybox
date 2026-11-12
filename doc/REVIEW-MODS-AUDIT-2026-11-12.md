# 네 갈래 점검 — 한글화 누락 · 총기 부품 로드 · 플리마켓 덮어쓰기 · 레벨 제한 (2026-11-12)

대상: `E:\Spt` (SPT 4.1.6 / EFT 0.16.9.5.40743). 모드 305개 + 자작 19개.

**어떻게 쟀나.** 게임은 꺼져 있었고(마지막 기동 17:11~19:15), **서버만 새로 띄워**
`/client/items`·`/client/locale/kr` 응답을 받아 셌다. 바닐라 쪽 값은 설치본의
`SPT_Data\database\templates\items.json`(템플릿 4,673개)이다 —
서버가 내려준 것은 **10,486개**라 모드가 더한 것이 **5,813개**다.
서버 로그는 `SPT_Runtime\user\logs\spt\spt20261112.log`(19,312줄).

---

## 1. 한글화 누락

| 갈래 | 덮개 | 남은 것 |
| --- | --- | --- |
| F12 설정 창 (`SptKoreanConfig`) | **5,136 / 5,150** | 14항목 — DynamicMaps Extended 2 · ECOTI 10(+섹션 1) · BiC 1 · Direct Reload 1. 사전이 아예 없는 모드 3개(QuestCodex 1 · UltimateFlea 1 · **우리 IcebreakerOffMapGuard 1**) |
| 모드 폴더 locale 4개 (`PluginLocales`) | 338 / 338 | **0** — 원본·설치본과 같다 |
| 서버 웹 설정창 (`ServerWebKorean`) | 230 / 230 | 원문 그대로 1(`example-heading`, 보기 글이라 정상) |
| 게임 DB 퀘스트·역할·대사 (`SptKoreanLocale`) | **8,468 / 8,468 적용**(켜진 서버 실측) | 값 다름 0 · 열쇠 없음 0 · 묶음 누락 0 |
| 게임 DB **아이템 이름** | — | **8,296항목**(모드 5,604 / 바닐라 2,692), 영문 51.6만 자 |
| 게임 DB **짧은 이름** | — | **9,216항목**(모드 5,512 / 바닐라 3,704), 31.0만 자 |
| 게임 DB **아이템 설명** | — | **1,295항목**(모드 771 / 바닐라 524), 8.6만 자 |

- 퀘스트 줄기는 **구멍이 없다.** 남은 한글화는 사실상 **아이템 이름·짧은 이름**이고,
  이름과 짧은 이름을 합치면 17,512항목 / 82.6만 자다. `item-names-isb-1.json` 으로
  ISB 계열 378개를 덮은 것이 이 묶음의 첫 조각이다.
- 바닐라 열쇠가 2,692·3,704개나 남은 것은 **바닐라도 영문으로 두는 이름**(프리셋·시즌·장비 변형)이
  섞여 있어서다 — 번역 대상을 고르는 기준이 [REVIEW-KOREAN-LOCALIZATION.md](REVIEW-KOREAN-LOCALIZATION.md)에 있다.
- 우리 모드 몫이 **한 줄** 비어 있다(`spt.icebreakeroffmapguard`). 다음 기동에 `.cfg` 가
  셋으로 늘어나므로 그때 사전을 만든다.

## 2. 총기 부품이 정상적으로 로드되는지

서버가 내려준 총기 **1,038종**(바닐라 177 + 모드 861) / 칸 **6,846개**를 전수로 봤다.

| 항목 | 값 | 판정 |
| --- | --- | --- |
| 약실(Chambers) | 975개, 필터가 빈 것 **0** | **정상** — `SlotCategoryRelax` 가 하는 일이 닿아 있다 |
| 바닐라 총기의 칸 중 **부품이 늘어난 칸** | 576 | **정상** — 호환 확장 모드가 실제로 넓혔다 |
| 모드가 새로 만든 칸 | 7 | 정상 |
| 필터가 빈 칸 | 18 (전부 `mod_launcher`) | **정상** — AK-102/104/105 는 **바닐라도 0개**다(확인함). 그 파생 변형 15종이 그대로 물려받은 것 |
| 부품이 **줄어든 칸** | 17 (전부 `mod_scope`, 각 -1) | 확인 필요 — 어느 모드가 조준경 하나를 빼고 있다 |
| **끊긴 참조**(없는 부품 id 를 가리키는 필터) | **33회 / 21칸 / id 13종** | **결함** — 바닐라에는 **0회**다 |

끊긴 참조가 몰린 자리:

| 총기 | 칸 | 가리키는 없는 id |
| --- | --- | --- |
| HK 416A5 (기본 + 변형 8종) | `mod_reciever` | `c6aa3fe8…` · `cc857617…` |
| IWI Tavor X95 (4종) | `mod_foregrip`·`mod_sight_rear`·`mod_sight_front` | `66a5465a…` · `67a01e79…`(+`…10f0`) |
| HK UMP 9x19 / .40 | `mod_stock`·`mod_mount_000`·`mod_scope` | `6701ae47…` 외 5종 |
| AN-94 "Kirov's Custom" | `mod_pistol_grip` | `679a76fc…` |
| Patriot M4A1 | `mod_sight_rear` | `67a01e79…` |
| AKS-74UN | `mod_stock` | `df1d3bfb…` |

**서버도 같은 것을 적고 있었다** — 오늘 로그에
`Could not add <id> to cache, it does not exist in the item database!` 가 **554줄**,
서로 다른 id **53종**이다(우리가 총기 칸에서 찾은 13종이 그 안에 든다. 나머지 40종은
방어구 칸·상인 물표·전리품 표 등 다른 자리에서 참조된다).

- 53종 id 를 모드 폴더에서 찾아 보면 **`EukyreECOT` 의 파일 4개**에만 나온다.
  나머지는 어느 모드의 파일에도 없다 — **런타임에 만들어지는 참조**다.
  끊긴 자리가 `HK 416A5 … Meta`·`Pay to Win`·`Black Market` 처럼 **변형 생성기가 만든 총기**에
  몰려 있어, 변형을 만들 때 부품은 만들지 못한(또는 다른 모드가 지운) 것으로 보인다.
  **어느 모드인지는 아직 확정하지 않았다**(`ItemGen` 은 팩이 없어 아무것도 만들지 않는다고 로그에 적는다).
- **증상의 크기**: 게임이 그 칸을 열 때 없는 id 는 조용히 빠지므로 개조 화면이 터지지는 않는다.
  실제로 11-12 기동의 게임 로그(9,624줄)에는 템플릿 관련 오류가 **0건**이었다.
  대신 서버가 기동마다 554줄을 적고, 캐시를 다시 세울 때마다 같은 일을 반복한다.

## 3. 플리마켓 항목을 덮어쓰는 모드끼리 충돌이 없는지

같은 자리를 건드리는 모드를 DLL 식별자로 찾았다.

| 건드리는 자리 | 모드 |
| --- | --- |
| 아이템마다의 `CanSellOnRagfair` | **12개** — AllWeaponsFlea · UltimateFlea · platinum-theblacklist · ItemGen · ItemPropertyBackport · TraderGen · ISB-Aishi · ODT-ItemInfo · COTI · GildedKeyStorage · Epic's All In One · lotsoflootredux |
| `RagfairConfig`(전역 설정) | **30개 남짓** — SVM · UltimateFlea · TheBlacklist · TraderGen · Lotus · StrangeCurios · harryhideout · scorpion · ShowMeTheMoney 등 |
| 상품 생성·값 (`RagfairOfferGenerator`·`PriceService`·`OfferService`) | UltimateFlea · StrangeCurios · harryhideout · scorpion · TheBlacklist · QuickSell · ShowMeTheMoney · LootNetServer · RealisticInsurance · ChooChoo-TraderModding · ISB-Aishi |

**최종 상태**(바닐라와 견준 실측):

- 바닐라에 있던 아이템 중 **판매 허용으로 바뀐 것 645개**, **판매 금지로 바뀐 것 7개**
- 모드가 더한 아이템 5,807개 중 **1,669개가 판매 금지**
- 지금 판매 금지 총계 **1,750개**(그중 부품 244)

**순서가 결과를 정한다** — 오늘 기동의 순서는 로그에 그대로 남아 있다:

```
[UltimateFlea] Pool applied (mode=blacklist): +0 enabled, -0 removed
[UltimateFlea] Economy initialized for 8737 items
[UltimateFlea] Level locks applied: 2148 items, 15 handbook categories locked (buy=True, sell=True)
[The Blacklist] Success! Found 916 blacklisted & 1 non-blacklisted items to update.
[RagfairServer] 플리마켓 상품 생성 중
```

- **TheBlacklist 가 UltimateFlea 보다 뒤에 돈다.** 둘이 같은 아이템의 판매 플래그를 다투면
  **TheBlacklist 의 916개가 이긴다.** UltimateFlea 의 풀은 `+0/-0` 이라 지금은 플래그를
  바꾸지 않으므로 **실제로 부딪히는 자리는 없다** — 조건부 무충돌이다.
  UltimateFlea 쪽에서 풀을 켜는 순간(모드 설정) 이 결론은 깨진다.
- 레벨 잠금은 플래그가 아니라 **UltimateFlea 자기 코드가 사러 올 때 막는** 방식이라
  TheBlacklist 와 **겹쳐 쌓인다**(덮어쓰기가 아니다).
- 값(가격) 쪽은 넷이 각자 표를 만든다 — QuickSell 9,765개(라이브 9.3만 호가에서),
  ShowMeTheMoney, UltimateFlea 경제 8,737개, TheBlacklist. **서로의 표를 지우지는 않지만
  화면마다 다른 값이 보일 수 있다**(미확인).

## 4. 플리마켓에 레벨별 구매·판매 제한

**이미 깔려 있다. 새로 만들 것이 없다.**

- `UltimateFlea` 가 **레벨 잠금**을 그 자리에 두고 있다 — 오늘 기동에서
  **아이템 2,148개 + 핸드북 분류 15개**를 잠갔고, **구매·판매 둘 다**(`buy=True, sell=True`)다.
  설정은 서버 웹 UI(`/ultimateflea/app.html`)와 `/ultimateflea/levellocks` 로 열린다.
- 템플릿 자체에는 레벨 자리가 **없다**(`_props` 의 level 열쇠는 전부 소리 압축 관련이다).
  그러니 "특정 아이템은 특정 레벨부터" 는 **UltimateFlea 의 레벨 잠금으로** 정하는 것이 맞고,
  우리가 서버 모드를 더할 이유는 지금 없다.
- `ISB-Aishi` 만 설정에 `minPlayerLevel` 류 열쇠를 들고 있다(그 모드 자기 상인·아이템용).
- 곁들여 깔린 것: `HideLockedFlea`(못 사는 물건 숨기기), `Moe-TradeMarker`(상인 제한 표시).

---

## 하기로 정하지 않은, 집을 수 있는 거리

1. **끊긴 참조 33회를 만드는 모드 찾기** — 변형 생성 모드를 하나씩 끄고 재면 한 번에 좁혀진다.
   찾으면 그 모드 설정으로 끄거나, 우리 쪽에서 **없는 id 를 필터에서 걸러 내는 서버 모드**(PostLoad)로 막는다.
2. **`mod_scope` 17칸이 왜 줄었나** — 어느 모드가 조준경 하나를 빼는지. 사용에 지장은 아직 없다.
3. **아이템 이름 한글화** — 17,512항목 중 어디부터 할지. 상인·플리에서 자주 보이는 것부터 고르는 셈이 필요하다.
4. **가격표 넷의 값이 화면마다 다른지** — QuickSell·ShowMeTheMoney·UltimateFlea·TheBlacklist.
5. **UltimateFlea 풀을 켤 때** TheBlacklist 와 부딪히는 아이템 수를 미리 세어 두기.

---

## 덧붙임 — 끊긴 부품 참조는 **누가 만들었나** (2026-11-12 21:00)

§2 의 "집을 수 있는 거리 1번" 을 집었다. 모드를 하나씩 끄지 않고, **id 를 모드 파일에서 찾아**
가렸다. 총기 칸에서 나온 **없는 id 13종**의 출처는 이렇다.

| 출처 | id | 어디에 |
| --- | --- | --- |
| **WTT-Armory** (9종) | `6701ae47…` `6700c690…` `6700c695…` `6701ae8e…` `6701aea1…` | HK UMP 9x19 의 `mod_stock`·`mod_mount_000` |
| | `679a76fc…` | AN-94 "Kirov's Custom" 의 `mod_pistol_grip` |
| | `66a5465a…` `67a01e79…` `67a01e79…10f0` | IWI Tavor X95 4종의 `mod_foregrip`·`mod_sight_rear`·`mod_sight_front`, Patriot M4A1 의 `mod_sight_rear` |
| **EukyreECOT** (2종) | `df1d3bfb…` | AKS-74UN 5.56x45 의 `mod_stock` — 그 총기를 만드는 `db/CustomItems/Weapon556Krink.json` 안에 적혀 있다 |
| | `686a95d3…` | HK UMP .40 S&W 의 `mod_scope` |
| **출처를 못 찾음** (2종) | `c6aa3fe8…` `cc857617…` | HK 416A5 **9종**(기본 + 변형 8)의 `mod_reciever` |

- 두 모드 다 **자기 파일에 그 id 를 정의하지 않는다.** EukyreECOT 은 아이템 163개를 만드는데
  그중에 없고, 필터에만 적어 둔다 — 다른 팩(또는 자기 옛 판)의 부품을 가리키는 **묵은 참조**다.
- HK 416A5 의 둘은 어느 모드 파일에도 없다. 그 이름을 지은 쪽은
  **DefinitiveWeaponVariants**(`Meta`·`Pay to Win`·`Black Market` 따위 변형 이름이 이 모드 파일에만 있다)이고,
  변형은 **원본 총기의 필터를 그대로 베끼므로** 원본 HK 416A5 의 묵은 참조가 9배로 불어난 모양이다.
  원본에 그 참조를 넣은 쪽은 아직 못 짚었다.
- 서버가 적는 `does not exist in the item database` **53종** 중 모드 파일에 있는 것은
  **EukyreECOT 의 3종뿐**이다. 나머지 50종은 파일에 없다 — 런타임에 만들어지는 참조다.

### 그래서 어떻게 할 수 있나

1. **그대로 둔다.** 게임은 없는 id 를 조용히 빼고 칸을 그린다 — 11-12 게임 로그에 템플릿 오류 0건.
   값은 서버 기동 로그 554줄과, 그 묵은 참조가 가리키던 부품을 **쓸 수 없다**는 것뿐이다.
2. **우리 쪽에서 쓸어낸다.** `OnLoadOrder.PostLoad` 에서 모든 칸 필터를 훑어 **DB 에 없는 id 를 빼는**
   작은 서버 모드. 남의 모드 파일은 건드리지 않는다(업데이트에 날아가지 않는다).
   SlotCategoryRelax·StackUnify 와 같은 자리·같은 방식이라 새로 배울 것이 없다.
3. **제작자에게 알린다.** WTT-Armory 9종·EukyreECOT 2종은 그쪽 파일의 묵은 참조다.

### 아직 못 본 것

- `mod_scope` **17칸이 왜 줄었는지**(§2 표)는 그대로 열려 있다. 어느 모드가 조준경 하나를 빼는지
  보려면 **서버를 다시 띄워** 바닐라와 한 번 더 견줘야 한다.
