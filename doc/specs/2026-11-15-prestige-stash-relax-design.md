# 설계 — 프레스티지 이전 창고의 크기와 품목 제한을 F12 에서 풀기 (PrestigeStashRelax)

- 날짜: 2026-11-15
- 상태: R0~R3 끝(8×12 는 안 잘린다). 분류만으로는 가방이 거부돼 CategoryExpander 를 더했고, 서버 재시작 뒤 다시 확인해야 한다. 내용물이 든 가방은 클라이언트가 박아 둔 규칙으로 못 넘긴다.
- 대상: `E:\Spt` (SPT 4.1.6, EFT 0.16.9.5.40743)
- 전제: `Content Backport - Prestiges` 1.1.0(모드 2540) 설치됨 — P1~P6 이 들어와 있다.

## 1. 왜 만드는가

프레스티지를 하면 창고가 비워지고, 다음 판으로 넘길 물건은 **이전 창고**(전송 창고)에 담은 것만
살아남는다. 그 창고가 **좁고, 담을 수 있는 품목도 정해져 있다.** 가방·박스를 못 넣으니 한 칸에
하나씩만 들어가고, 24~64칸으로는 쓸 만한 것을 거의 못 가져간다.

제한은 **서버 데이터에 적혀 있고 클라이언트가 그걸 읽어 화면을 그린다.** 그래서 데이터를 바꾸면
제한이 바뀐다 — 게임 코드를 뜯을 필요가 없다. 아래가 실측이다.

## 2. 실측 — 제한이 어디에 어떻게 적혀 있나

### 2-1. 데이터 위치

| 무엇 | 어디 |
| --- | --- |
| SPT 기본 4단계 | `SPT_Runtime\SPT_Data\database\templates\prestige.json` (`elements[]`) |
| 2540 이 덮어쓴 6단계 | `SPT_Runtime\user\mods\archangelwtf-contentbackportprestiges\db\PrestigeBackport\prestiges.json` |

구조는 `elements[i].transferConfigs.stashConfig` 하나다.

```
stashConfig: { xCellCount, yCellCount, filters: { includedItems[], excludedItems[] } }
```

서버 모델도 그대로다(`SPTarkov.Server.Core.dll` 디컴파일) — `StashPrestigeConfig.XCellCount`
· `YCellCount` · `Filters`, `StashPrestigeFilters.IncludedItems` · `ExcludedItems`. 전부
`record` 의 **쓰기 가능한 속성**이라 런타임에 바꿀 수 있다.

클라이언트(`Assembly-CSharp.dll`)도 같은 이름을 쓴다 — `xCellCount` · `yCellCount` ·
`includedItems` · `excludedItems` · `IncludedItems` 문자열이 그대로 들어 있다.

### 2-2. 단계별 현재 값 — 가로는 8 로 고정, 세로만 1씩 늘어난다

| 단계 | 격자 | 칸 수 | 허용 항목 수 | 스킬·숙련 이전율 |
| --- | --- | --- | --- | --- |
| P1 | 8 × 3 | 24 | 203 | 5% |
| P2 | 8 × 4 | 32 | 186 | 10% |
| P3 | 8 × 5 | 40 | 186 | 15% |
| P4 | 8 × 6 | 48 | 186 | 20% |
| P5 | 8 × 7 | 56 | 186 | 25% |
| P6 | 8 × 8 | 64 | 186 | 30% |

이전율은 JSON 의 `transferMultiplier`(0.95~0.70)를 서버가 `1 - 값` 으로 쓴다 — 적힌 값은
**깎는 비율**이다. P5·P6 은 2540 이 더한 것이고 같은 규칙을 이었다.

### 2-3. 품목 제한은 화이트리스트다 — `excludedItems` 는 여섯 단계 모두 **비어 있다**

`includedItems` 의 186~203개는 **구체적 아이템 id 대부분 + 분류(Node) id 몇 개**가 섞여 있다
(P1 기준 아이템 197 / 분류 6). 분류 id 가 통한다는 것은 **이미 쓰이고 있다는 사실로 증명된다** —
즉 분류 하나를 더하면 그 아래 품목이 한꺼번에 열린다.

단계별로 열려 있는 분류:

| 단계 | 분류 |
| --- | --- |
| P1 | BuildingMaterial · Fuel · FoodDrink · Map · Drugs · Medical |
| P2 | Drugs · Medical · Map · FoodDrink · HouseholdGoods · Tool · Fuel |
| **P3~P6** | 위 7개 + **ArmoredEquipment · Headphones · Weapon · Mod** |

**P3 부터는 무기·부품·방어 장비·헤드셋이 이미 열려 있다.** 막혀 있는 것은 P1·P2 에서는 교역품
말고는 전부이고, P3 이상에서도 아래는 **여섯 단계 어디에도 없다.**

| 막힌 것 | 분류 id |
| --- | --- |
| 배낭 Backpack | `5448e53e4bdc2d60728b4567` |
| 가방·케이스 SimpleContainer | `5795f317245977243854e041` |
| 보안 컨테이너 MobContainer | `5448bf274bdc2dfc2f8b456a` |
| 조끼 Vest | `5448e5284bdc2dcb718b4567` |
| 돈 Money | `543be5dd4bdc2deb348b4569` |
| 탄 Ammo / 탄박스 AmmoBox | `5485a8684bdc2da71d8b4567` / `543be5cb4bdc2deb348b4568` |
| 열쇠 Key / 카드 Keycard | `543be5e94bdc2df1348b4568` / `5c164d2286f774194c5e69fa` |
| 탄창 Magazine | `5448bc234bdc2d3c308b4569` |
| 투척물 ThrowWeap | `543be6564bdc2df4348b4568` |
| 정보 Info | `5448ecbe4bdc2d60728b4568` |
| 특수 SpecItem | `5447e0e74bdc2d3c308b4567` |

분류 사슬의 뿌리는 `Item` = `54009119af1c881c07000029` 이다. 클라이언트가 **부모 사슬을 거슬러
맞춘다면** 이 하나만 더해도 전부 열린다 — 다만 그 판정 방식은 아직 안 재 봤다(§6-1).

### 2-4. 서버는 **아무것도 검사하지 않는다** — 제한은 화면에만 있다

`PrestigeHelper.ProcessPendingPrestige` 를 디컴파일해 확인했다. 클라이언트가 보낸
`prestige.Items` 를 그대로 돌면서

- `GetItemWithChildren(transferRequest.Id)` 로 **자식까지 함께** 집어
- 핀·잠금 상태만 풀고
- `SendSystemMessageToPlayer(..., 8760시간)` 으로 **우편으로 보낸다.**

칸 수 검사도, 필터 검사도, 개수 검사도 **없다.** 즉 이전 창고의 크기와 품목 제한은 **클라이언트
UI 가 서버 데이터를 보고 스스로 거는 제한**이고, 데이터를 바꾸면 그대로 풀린다. 서버 쪽에 새로
검사를 넣을 필요도, 뚫을 필요도 없다.

자식을 함께 집는다는 점이 중요하다 — **가방을 허용하면 그 안의 내용물이 다 같이 넘어간다.**
BSG 가 가방을 막아 둔 이유이고, 열 때 그 뜻을 알고 열어야 한다.

### 2-5. 클라이언트는 이 데이터를 **다시 물어본다** — 서버 재시작이 필요 없다

`PrestigeController.GetPrestige` 는 `templateTable.Prestige` 를 **그대로** 돌려준다
(`/client/prestige/list`). 그리고 2026-11-15 서버 로그를 보면 한 세션에서 그 라우트가
**6번** 불렸다 — 16:09 · 16:33 · 17:28 · 18:32 · 19:01 · 19:45. 프로필을 다시 읽을 때마다
물어보는 것이다.

따라서 **살아 있는 `templateTable.Prestige` 객체를 고치면, 다음 조회부터 새 값이 간다.** F12 에서
값을 바꾸고 메뉴를 한 번 되돌아오면 반영된다 — 게임도 서버도 끄지 않는다.

## 3. 목표

- **F12 에서** 단계별 이전 창고의 **가로·세로 칸 수**를 조정한다.
- **F12 에서** 담을 수 있는 품목 분류를 켜고 끈다. 특히 **가방·배낭·케이스·조끼 제한을 푼다**.
- 바꾼 값이 **게임·서버를 끄지 않고** 반영된다.
- 끄면 **바닐라(2540 포함) 값 그대로**로 돌아간다 — 원본을 지니고 있다가 되돌린다.
- 2540 이 업데이트돼 값이 바뀌어도 **그 새 값 위에** 우리 조정을 얹는다(파일을 고치지 않는다).

## 4. 비목표

- 프레스티지 **요구 조건**은 건드리지 않는다(2540·3332 의 몫).
- 스킬·숙련 **이전율**은 1차에서 건드리지 않는다. 같은 `transferConfigs` 안에 있어 나중에 더하기
  쉬운 자리만 비워 둔다(§8).
- 넘긴 물건이 **우편으로 온다**는 방식은 그대로 둔다.
- 프레스티지 **초기화 자체를 막지 않는다.** 창고가 비워지는 것은 게임의 규칙이고, 우리는 **넘길
  수 있는 양과 종류**만 넓힌다.
- 퍽·보상 같은 콘텐츠는 넣지 않는다(3332 의 몫).

## 5. 설계

### 5-1. 왜 서버 모드 + F12 플러그인 두 짝인가

| 방식 | 되나 | 왜 안 고르나 |
| --- | --- | --- |
| 설치본 JSON 직접 수정 | 된다 | 2540 업데이트에 날아간다. F12 도 못 쓴다. CLAUDE.md 가 금지한 방식이다 |
| 클라이언트 플러그인 단독 | 아마 된다 | 난독화된 전송 화면 타입을 찾아 패치해야 한다. 서버가 쥔 데이터를 화면에서 되돌리는 꼴이라 깨지기 쉽다 |
| **서버 모드 + F12 플러그인** | **된다** | 제한이 사는 곳(서버 데이터)을 고치고, 손잡이만 F12 에 둔다 |

`WebConfigBridge`·`AirdropCaller` 가 이미 같은 짝으로 돌고 있다 — 플러그인이
`RequestHandler.PostJsonAsync` 로 우리 라우트를 치고, 서버 모드가 `StaticRouter` 로 받는다.

### 5-2. 구성

```
src/PrestigeStashRelax/            서버 모드
  PrestigeStashRelax.cs            PostDBLoad 에서 원본을 떠 두고 설정을 얹는다
  PrestigeSnapshot.cs              단계별 원본 격자·필터 보관(되돌리기용)
  PrestigeApply.cs                 설정 → templateTable.Prestige 적용(멱등)
  CategoryCatalog.cs               분류 id ↔ 보여 줄 이름 표
  PrestigeStashRelaxRouter.cs      /prestige-stash/state · /prestige-stash/apply
  Config.cs · ConfigStore.cs       config.jsonc 읽고 쓰기
  ModMetadata.cs
src/PrestigeStashRelax.Plugin/     F12 플러그인
  Plugin.cs                        ConfigEntry 묶음 + 바뀌면 서버로 보낸다
src/PrestigeStashRelax.Tests/      적용 멱등성·되돌리기 시험
```

### 5-3. 적용은 **멱등**이어야 한다

`regenerating-must-be-idempotent` 의 교훈을 그대로 받는다. **적용된 상태를 기준으로 삼으면 두 번
적용할 때 값이 어긋난다.** 그래서

1. `PostDBLoad` 에서 **가장 먼저** 단계별 원본(`x`, `y`, `includedItems` 사본)을 떠 둔다.
2. 적용은 늘 **원본에서 시작해** 설정을 얹는다 — 지금 값에 더하지 않는다.
3. F12 가 값을 또 보내도 1번의 원본은 그대로이므로 몇 번이든 같은 결과가 된다.

스냅샷은 **리스트 색인이 아니라 `PrestigeElement.Id` 를 열쇠로** 들고, 적용할 때마다 현재
`Elements` 에서 id 로 찾아 쓴다. 다른 모드가 `Elements` 를 **새 리스트로 갈아끼우면** 색인으로
들고 있던 참조가 떠 버리기 때문이다 — 3332 Prestige Perks 가 그럴 수 있다
(`doc/plans/2026-11-15-prestige-mods-install-plan.md` §3).

원본을 뜨는 시점이 **2540 이 자기 6단계를 넣은 뒤**여야 한다. `db/PrestigeBackport/prestiges.json`
은 2540 의 서버 모드가 싣는다. `last-transformer-wins` 대로 **우리가 나중에** 돌아야 하므로
`Injectable` 우선순위를 `int.MaxValue` 로 두고, 그래도 순서가 어긋나면 `PostSptLoad` 로 내린다
(§6-3 에서 실측해 정한다).

### 5-4. F12 손잡이

| 묶음 | 항목 | 기본값 | 비고 |
| --- | --- | --- | --- |
| 일반 | 켜기 | 끔 | 끄면 원본으로 되돌린다 |
| 일반 | 자세히 적기 | 끔 | |
| 크기 | 가로 칸 수 | 0 (= 손대지 않음) | 1~? — 상한은 §6-2 에서 재고 정한다 |
| 크기 | 세로 칸 수 더하기 | 0 | 단계별 원본 세로에 **더한다**. P1 3+n … P6 8+n 식으로 단계차를 지킨다 |
| 크기 | 세로 칸 수 고정 | 0 (= 안 씀) | 모든 단계를 같은 세로로 못 박고 싶을 때. 더하기보다 뒤에 적용 |
| 품목 | 가방·배낭 허용 | **켬** | Backpack + SimpleContainer |
| 품목 | 조끼 허용 | 켬 | Vest |
| 품목 | 보안 컨테이너 허용 | 끔 | MobContainer — 넣으면 안의 것이 다 넘어간다 |
| 품목 | 돈 허용 | 끔 | Money |
| 품목 | 탄·탄창 허용 | 끔 | Ammo · AmmoBox · Magazine |
| 품목 | 열쇠·카드 허용 | 끔 | Key · Keycard |
| 품목 | 무기·부품 허용(P1·P2 에도) | 끔 | P3 부터는 이미 열려 있다 |
| 품목 | 방어 장비 허용(P1·P2 에도) | 끔 | ArmoredEquipment |
| 품목 | **전부 허용** | 끔 | 뿌리 분류 `Item` 하나를 넣는다. 통하는지는 §6-1 |
| 품목 | 직접 적은 분류 id | 빈 칸 | 쉼표로 나눠 적는다 |

**적용 범위**는 단계마다 따로 두지 않는다 — F12 는 칸이 길어지면 쓰기 어렵다. 대신 "P1·P2 에도"
같은 식으로 **이미 열린 단계와 그렇지 않은 단계**만 가린다. 단계별 세밀 조정이 필요하면
`config.jsonc` 에서 한다(서버 쪽 설정 파일이 단계별 표를 그대로 받는다).

### 5-5. 값이 오가는 길

```
F12 에서 값 바꿈
  → ConfigEntry.SettingChanged
  → Task.Run(() => RequestHandler.PostJsonAsync("/prestige-stash/apply", 설정))
  → 서버: 설정 저장 + 원본에서 다시 적용
  → 플레이어가 메뉴로 돌아옴 → 클라이언트가 /client/prestige/list 재조회
  → 새 격자·필터로 화면이 그려진다
```

`AirdropCaller.Plugin` 과 같은 이유로 **서버 호출은 기다리지 않는다**(`Task.Run` 으로 감싼다 —
`PostJsonAsync` 의 첫 await 까지가 동기라 메인 스레드가 멈춘다).

서버는 설정을 `config.jsonc` 에도 적어 **다음 기동에도 남게** 한다.

## 6. 재야 했던 셋 — **둘은 끝났다**

> 2026-11-15 측정: `doc/specs/2026-11-15-prestige-stash-measurements.md`.
> **R1·R2 는 어셈블리 디컴파일로 끝났다 — 게임을 재볼 필요가 없었다. R3 만 화면으로 남았다.**
> 아래는 무엇을 왜 재려 했는지의 기록이고, 항목마다 결과를 붙였다.

### 6-1. 필터 판정이 부모 사슬을 거슬러 가나

- 분류 id 가 통하는 것은 확실하다(이미 7~11개가 쓰이고 있다).
- 그런데 `Backpack` 을 넣었을 때 **그 아래 구체 배낭들**이 열리는지, 아니면 분류가 직접 부모인
  것만 열리는지는 다르다. 또 뿌리 `Item` 하나로 전부 열리는지도 같은 물음이다.
- **재는 법**: `Backpack` 만 넣고 전송 화면에서 배낭을 끌어 본다. 이어 `Item` 만 넣고 돈·탄을 끌어
  본다.
- 안 통하면 분류 대신 **구체 id 를 펼쳐 넣는다** — `items.json` 에서 그 분류의 자손을 모아
  `includedItems` 에 쏟는다(수백 개가 되지만 데이터라 문제 없다). 그 펼치기는 서버 모드가 한다.
- **결과 — 코드로는 거슬러 가는데, 화면이 뒤집었다.** `ItemFilter.Contains` → `HandbookEntities.Contains` 가 id 목록을
  `Items` 와 `Categories` **양쪽으로** 보고, 분류 쪽은 `ItemTemplate.IsChildOf` 로 **부모 사슬을
  뿌리까지** 거슬러 맞춘다. 그래서 `Backpack` 하나로 배낭 전부, 뿌리 `Item` 하나로 전부가 열린다.
  그런데 게임에서 배낭을 끌자 **필터 쪽 거부**(`UI/Trader/TransferLocked`)가 떴다 — 분류가 그 배낭에
  닿지 않았다. **펼쳐 넣는 대비책이 필요했다**(`CategoryExpander`, `ExpandCategories: true`).
  그리고 둘째 관문을 놓쳤다 — `CompoundItem { IsEmpty: false }` 라서 **내용물이 든 가방은 필터와
  무관하게 못 넘긴다.** `excludedItems` 는 빼기로 작동한다.

### 6-2. 격자를 얼마나 키울 수 있나

- 가로가 8 로 고정인 것은 **데이터가 그런 것**이고 화면이 8 만 그릴 수 있다는 뜻은 아니다.
- 전송 화면의 창이 고정 크기면 세로를 늘렸을 때 **넘치거나 잘릴** 수 있다. 자체 스크롤이 있는지
  모른다. `GridWindowScroll`(우리 모드)은 `GridWindow.ShowContents` 를 잡는데 전송 화면은
  `TransferItemsScreen` · `ArenaEftItemTransferStashGridView` 쪽이라 **그 모드가 받쳐 주지
  않는다.**
- **재는 법**: 8×12 → 8×20 → 12×8 순으로 올려 가며 화면을 본다. 잘리는 지점이 상한이다. 그
  값을 F12 설정 범위의 위끝으로 못 박는다.
- 스크롤이 없고 상한이 낮으면 **가로를 늘리는 쪽**이 나을 수 있다. 그때는 F12 의 "가로 칸 수"가
  주 손잡이가 된다.
- 화면으로만 확인되는 종류다 — `isb-subtitles-leave-no-log` 처럼 로그에 안 남는다. 스크린샷으로
  남긴다.
- **결과(절반) — 코드는 막지 않는다.** `CreateTransferStash` 가 `_stashConfig.Size` 를 그대로
  `ReferenceGrid` 에 넘기고 `canStretchVertically: false` · `canStretchHorizontally: false` 다 —
  칸 수가 곧 크기이고 **가로 8 도 코드의 제한이 아니다.** 뷰는 `GridView.Show` 에서
  `_rectTransform.sizeDelta = cellPixelSize` 로 커지며 **칸 하나가 63px** 이다. 남은 것은 그
  격자를 담은 프리팹이 어디서 잘리는지뿐이고, 그래서 **화면이 유일한 답**이다. 첫 칸은 세로 +4
  (P1 8×7 … P6 8×12, 756px)로 잡았다.

### 6-3. 우리가 2540 보다 나중에 도나

- 2540 의 서버 모드가 `prestiges.json` 을 어느 단계에서 싣는지 모른다. 우리가 먼저 돌면 4단계
  원본을 뜨고, 그 뒤 2540 이 6단계로 갈아끼워 우리 조정이 사라진다.
- **재는 법**: 서버 모드에 `PostDBLoad` 와 `PostSptLoad` 양쪽에서 `Elements.Count` 를 찍어 둔다.
  6 이 나오는 쪽이 우리 자리다.
- `db-changes-must-outrun-item-mods` 와 같은 종류의 함정이다.
- **3332 Prestige Perks 를 넣은 뒤에는 다시 잰다.** 그 모드도 같은 테이블을 고치므로 우리 자리가
  바뀔 수 있다.
- **결과(끝) — 우리가 뒤다. 단계가 갈려 있다.** SPT 는 `IOnLoad` 를 `TypePriority < 200000`
  (`SPT.Server.dll` 의 `RunPreSptLoadCallbacks`, 웹 서버 기동 전)과 `>= 200000`
  (`SPTStartupHostedService`)의 **두 단계**로 돌린다. 2540 의 `PreSPTLoad` 는 `TypePriority = 100003`
  이라 앞 단계이고, 우리는 `int.MaxValue` 라 뒤 단계다. 2540 은 `Elements` 를 **제자리에서** 고치고
  (`Elements[index] = prestige`) 모자란 P5·P6 만 더한다 — 리스트를 갈아끼우지 않는다.

## 7. 단계 계획

| 단계 | 할 일 | 끝났다고 보는 기준 | 상태 |
| --- | --- | --- | --- |
| **R0** | 프로필 백업 | `backup/profiles-before-prestige-stash/` | **끝** — 11개 9MB (11-15 21:1x) |
| **R1** | §6-3 적용 순서 | 단계 수가 6 이다 | **끝** — 코드로. 단계가 갈려 뒤집히지 않는다 |
| **R2** | §6-1 필터 판정 | 분류가 사슬을 여는가 | **끝** — 코드로. 뿌리 하나로 전부 열린다 |
| **R3** | §6-2 격자 상한 | 잘리는 지점과 스크린샷 | **끝** — 8×12 가 잘리지 않는다. 화면이 `(8x12)` 를 스스로 적었다. 아래 여백 91px |
| **R4** | 서버 모드 본체(원본 스냅샷 · 멱등 적용 · `config.jsonc`) | 시험 녹색, 기동 정상, 값이 조회에 반영됨 | **끝** — 시험 15/15, 기동 정상, 단계 6개에 적용 확인. 라우트는 R5 와 함께 |
| **R5** | F12 플러그인 + 서버 라우트 | F12 에서 바꾸고 메뉴 되돌아오면 격자가 바뀐다 | 아직 |
| **R6** | 실제로 한 번 프레스티지해 **가방이 내용물째 넘어오는지** 본다 | 우편으로 온 가방 안이 그대로 | 아직 |
| **R7** | 설치 기록·목록 갱신, `check-install-records.mjs` `OK — 0/0` | 그 출력 | 아직 |

R6 은 프로필을 되돌릴 수 없는 일이다 — R0 의 백업으로만 되돌아온다. 그 전에 사용자에게 묻는다.

## 8. 나중에 더할 자리

- 스킬·숙련 이전율(`transferMultiplier`) 손잡이. 같은 `transferConfigs` 안이라 적용 경로를 그대로
  쓴다.
- `excludedItems` 는 여섯 단계 모두 비어 있다 — "이것만 빼기" 쪽으로 쓰면 전부 허용과 함께 쓰기
  좋다.
- 단계별 세밀 조정 화면을 `WebConfigBridge` 에 붙이기(F12 는 표를 못 그린다).

## 9. 함께 보는 문서

- 설치 계획: `doc/plans/2026-11-15-prestige-mods-install-plan.md` (3332 Prestige Perks · 2687
  Sicc Case Fix)
- 설치 절차: `doc/PROCEDURE.md`
- 비슷한 짝의 선례: `doc/specs/2026-10-28-airdrop-caller-design.md`
