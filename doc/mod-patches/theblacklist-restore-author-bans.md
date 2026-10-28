# TheBlacklist — 제작자가 막아 둔 아이템을 되살린다 (858건 추가, 744건 반영)

2026-10-28. `platinum-theblacklist/Config/config.jsonc` 의 `customItemConfigs` 에
`{"type":"item","itemId":"…","blacklisted":true}` **858개**를 넣었다.

## 왜

`disableBsgBlacklist: true` 는 이 모드의 본래 기능이라 그대로 켜 뒀다
([../REVIEW-LOAD-ORDER.md](../REVIEW-LOAD-ORDER.md) 3번). 그 대신 **콘텐츠 모드 제작자가
자기 데이터 파일에 `CanSellOnRagfair: false` 로 적어 둔 아이템만** 아이템 단위로 다시 막았다.
바닐라 블랙리스트는 계속 풀려 있다 — 모드를 쓰는 이유가 그것이기 때문이다.

## 숫자를 바로잡았다 — 244건이 아니라 858건이다

전에 "ISB-Aishi 243건 + Lotus 키카드 1건 = 244건"으로 적어 뒀다. **다시 재 보니 틀렸다.**

| 모드 | 막아 둔 것 | 그중 풀려 있던 것 |
| --- | --- | --- |
| ISB-Aishi | 458 | **438** |
| WTT-ContentBackport | 374 | **366** |
| WTT-Armory | 39 | **39** |
| WTT-Artem | 12 | **12** |
| Airburst | 2 | **2** |
| LoadAmmoAnimServer | 1 | **1** |
| TGC-NG | 1 | 0 |
| **합** | **887** | **858** |

Lotus 는 한 건도 없었다. 전에는 ISB-Aishi 만 세고 WTT 계열 417건을 빠뜨린 것으로 보인다.

### 왜 처음에 놓쳤나 (방법 메모)

모드마다 아이템 정의 꼴이 다르다. 처음 스캔은 `{"_id": …, "_props": {…}}` 꼴만 찾았는데,
ISB-Aishi·WTT 는 **id 가 객체의 열쇠이고 속성은 `overrideProperties` 안에** 있다.

```json
{ "6a30ed97ce95057496b93476": { "itemTplToClone": "…",
    "overrideProperties": { "CanSellOnRagfair": false, … } } }
```

그래서 0건이 나왔다. 두 꼴을 다 받게 고치자 887건이 나왔다. 또 `kazusa-SimpleItemModifier`
폴더에는 아이템 DB **사본**(스냅샷 8882개)이 있어, 거기서 1956건이 걸렸지만 그것은 제작자
의도가 아니라 바닐라 블랙리스트다 — 건너뛴다.

## 결과 — 858건 중 744건이 막혔다

서버를 다시 띄워 `POST /client/items` 로 최종 DB 를 받아 확인했다.

| | 건수 |
| --- | --- |
| 막혔다 (`CanSellOnRagfair: false`) | **744** |
| 아직 플래그가 `true` 다 | **114** |
| DB 에 없다 | 0 |

대조군도 봤다 — `.338 AP` 와 `SJ6`(원래 바닐라 블랙리스트) 는 **그대로 `true`** 다. 즉
모드의 본래 기능은 살아 있고, 넣은 항목이 실제로 읽히고 있다.

### 남은 114건은 원인이 하나가 아니다

| 분류 | 건수 |
| --- | --- |
| Info (쪽지·퀘스트 품목) | 36 |
| AssaultRifle | 24 |
| CultistAmulet | 7 |
| Smg | 6 |
| SpecItem | 6 |
| ArmoredEquipment | 5 |
| MarksmanRifle · Electronics | 4 · 4 |
| 그 밖 (Keycard 3, Pistol 2, Shotgun 2 …) | 22 |

총기 41정쯤은 **AllWeaponsFlea** 가 설명할 수 있다(그 모드의 일이 바로 총기를 플레아에
올리는 것이고, 설정 파일도 없이 그렇게 한다). 그러나 Info 36·CultistAmulet 7·SpecItem 6 은
그것으로 설명되지 않는다. `CanSellOnRagfair` 를 건드리는 모드가 일곱 개 더 있다
(UltimateFlea·ItemGen·TraderGen·LennoxP90-COTI·ODT-ItemInfo…). **어느 것이 뒤에 쓰는지는 아직
가리지 않았다** — 하나씩 빼고 서버를 띄워 보는 수밖에 없다.

### 한 가지 더 정확히

`blacklisted: true` 가 하는 일은 두 갈래다. 코드를 읽어 보면
`TheBlacklist.OnLoad.Preload::RegisterBlacklistedItems` 가 id 를 SPT 의
`RagfairConfig.Dynamic.Blacklist.Custom` 에 넣고, `PricingService::ApplyPrices` 가 아이템의
`CanSellOnRagfair` 를 손본다. 위에서 센 114건은 **아이템 플래그가 `true` 라는 뜻**이고,
플레아 생성 쪽(`Blacklist.Custom`)에서는 걸러질 수도 있다. 플래그만으로 "플레아에 뜬다"고
단정하지는 않는다.

## 넣은 꼴

`customItemConfigs` 배열이 열리는 자리에 표시를 달아 끼웠다. 주석을 살리려고 JSON 으로 다시
쓰지 않고 글로 넣었다.

```jsonc
"customItemConfigs": [
    // === 제작자가 막아 둔 아이템 되살림 (Spt_Mods 2026-10-28) — 아래 블록째 지우면 원래대로 ===
    {
        // ISB-Aishi: [Aishi]_(5.45x39mm 7N24M)
        "type": "item",
        "itemId": "6a30ed97ce95057496b93476",
        "blacklisted": true
    },
    …
    // === 되살림 끝 ===
```

항목 수는 38 → **896** (그중 `blacklisted` 인 아이템 890). 넣은 뒤 주석을 떼고 파싱해
확인했다. 넣은 id 목록은 [theblacklist-restored-ids.json](theblacklist-restored-ids.json)
이다(모드 이름과 아이템 이름이 함께 들어 있다).

## 되돌리기

패치 직전 파일이 `backup/theblacklist-config.jsonc.before-restore-bans` 에 있다.
`E:\Spt\SPT_Runtime\user\mods\platinum-theblacklist\Config\config.jsonc` 로 덮어쓰고 서버를
다시 켜면 된다. 블록만 지우려면 위의 `=== 되살림 ===` 두 줄 사이를 지운다.

## 주의

- **모드를 갱신하면 사라진다.**
- 이 모드는 **서버 웹 페이지에 설정 화면**(`wwwroot`, "Item overrides")을 가지고 있다. 거기서
  저장하면 `config.jsonc` 를 다시 쓸 수 있고, 그때 **주석과 이 블록 표시가 사라질 수 있다.**
  웹에서 고친 뒤에는 블록이 아직 있는지 확인한다.
- 되살린 858건은 **플레아에서 사고팔 수 없게 된다.** ISB-Aishi 탄약을 플레아에서 사던 습관이
  있으면 그게 막힌다. 그것이 제작자가 적어 둔 의도다.
