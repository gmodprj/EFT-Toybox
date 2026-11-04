# 총기별 부착물 호환을 다루는 모드 후보 (2026-11-04)

`All In Weapon: Unslotted`(2576)를 빼고, **총기마다 어느 부착물을 달 수 있는지**를 정하는 쪽으로 돌릴 때 쓸 후보를 훑었다. 밸런스는 고려하지 않았다 — 원하는 조합이 되게 만드는 것만 본다.

`All In Weapon` 은 칸 필터를 통째로 없애는 쪽이라 "총기별로 정한다"의 반대편에 있다. 여기서 찾는 것은 **필터를 지우지 않고 원하는 항목만 넣거나 빼는** 수단이다.

## 어떻게 훑었나

- Forge 검색을 `attachment` · `compatibility` · `slot` · `unslotted` · `filters` · `customizer` 로 나눠 돌렸다. 4.1.6 표기만 남기고, 표시·수치만 바꾸는 것은 뺐다.
- 남은 후보는 **소스를 직접 읽어** 어디를 고치는지(전역 DB vs 클라이언트 목록), 넣기만 되는지 빼기도 되는지, 로드 순위를 확인했다. 모드 페이지 설명만으로는 이 셋이 전부 안 적혀 있다.
- 조사는 설치본을 건드리지 않았다. 무엇도 다운로드·설치하지 않았다.

## 1순위 — Easy Slot Filters

| 항목 | 값 |
| --- | --- |
| 페이지 | [Easy Slot Filters](https://sp-mod.com/mod/2940/easy-slot-filters) (id 2940) |
| 버전 / 호환 | 1.0.0 / **SPT 4.1.6** |
| 제작자 | bushtail |
| 크기 / 다운로드 | 5.94KB / 131 |
| 표시 | 결속 표시 없음. Fika 호환 버전 있음 |
| 분류 / 라이선스 | Tools / MIT |
| GUID | `ca.bushtail.easyslotfilters` |
| 소스 | <https://github.com/bushtail/EasySlotFilters> |

찾던 것에 가장 정확히 맞는다. 무기 id → 부착물 id → 칸 이름을 한 줄씩 적는 서버 모드다. 설정은 모드 폴더의 `config.jsonc` 로, 첫 기동 때 스스로 만든다.

```jsonc
{
  "Filters": {
    "5580223e4bdc2d1c128b457f": {                  // 무기 id (MP-43-1C 12ga)
      "64748d02d1c009260702b526": "mod_barrel"     // "부착물 id": "칸 이름"
    }
  }
}
```

README 에 쓸 수 있는 칸 이름 41개가 모두 적혀 있다 — `mod_barrel` · `mod_handguard` · `mod_scope`/`mod_scope_000`~`001` · `mod_mount`/`mod_mount_000`~`004` · `mod_muzzle`/`mod_muzzle_000`~`001` · `mod_tactical`/`mod_tactical_000`~`003` · `mod_stock`/`mod_stock_000`~`002`/`mod_stock_akms`/`mod_stock_axis` · `mod_pistol_grip`/`mod_pistolgrip`/`mod_pistolgrip_000`~`001`/`mod_pistol_grip_akms` · `mod_sight_front`/`mod_sight_rear` · `mod_gas_block` · `mod_reciever`(철자 그대로) · `mod_magazine` · `mod_bipod` · `mod_foregrip` · `mod_flashlight` · `mod_nvg` · `mod_catch` · `mod_charge` · `mod_hammer` · `mod_trigger` · `mod_equipment`.

### 소스를 읽어 확인한 것

`EasySlotFilters.cs` / `ESFConfig.cs` 기준.

- `IOnLoad` 구현이고 로드 순위가 `OnLoadOrder.TraderRegistration - 1` — **이른 자리**다. 뒤에 오는 DB 수정 모드가 같은 칸을 덮으면 밀릴 수 있다(StackUnify 에서 겪은 문제의 반대 방향 — 그쪽은 너무 늦어서, 이쪽은 너무 일러서).
- `templateTable.Items` 를 직접 고친다 → **전역 DB**다. `All In Weapon` 1.2.0 이 클라이언트 목록만 고쳐 봇 무기 생성을 바닐라로 남겨 둔 것과 다르다. 여기서 넓힌 칸은 **봇·상인·플레아에도 그대로 적용된다**.
- 그 칸의 **모든 필터 묶음에 id 를 `Add` 만** 한다. 중복 검사도 없고 **제거도 없다** — 넓히기만 되고 좁히기는 안 된다.
- 설정 구조가 `{부착물: 칸}` 이라 부착물 id 가 키다. 그래서 **한 무기에서 같은 부착물을 두 칸에 넣을 수 없다**(예: `mod_scope_000` 과 `mod_scope_001` 양쪽에 같은 조준경 → 키 충돌). 칸이 여러 개인 상부/레일에서 걸린다.
- 칸을 못 찾으면 노란 경고만 적고 넘어간다. 무기 id 자체가 DB 에 없으면 **조용히** 넘어간다(로그도 없다) — 오타를 알아채기 어렵다.
- `config.jsonc` 역직렬화가 깨지면 예외를 던지며 선다(파일 지우고 재생성하라는 안내).

### 위험

다운로드 131, 추천 0, 커밋 3개. **거의 검증되지 않은 신규 모드**다. 코드가 100줄 남짓이라 읽고 판단하기는 쉽다.

## 2순위 — SPT Essentials 의 Compatibility Rules

| 항목 | 값 |
| --- | --- |
| 소스 | <https://github.com/notSENZE/SPT-Essentials> |
| 호환 | SPT ~4.1.6 |
| 의존성 | ~~WTT-CommonLib 3.x~~(설치됨) |
| 라이선스 | MIT. Fika 호환 불명 |
| 설정 | `SPT_Runtime/user/mods/SPTEssentials/config/compat/compatibility.jsonc` |

- 한 파일에서 **아이템·컨테이너·Special Slot·무기·부착물 필터**를 모두 넓힌다. 주석 달린 예시가 파일 안에 들어 있다.
- `REPLACE` 플래그가 있어 **기존 필터를 비우는 것도 된다** → Easy Slot Filters 와 달리 *좁히기*가 된다. 이게 유일한 구조적 장점이다.
- **칸 단위 granularity 는 README 에 안 적혀 있다.** 특정 무기의 특정 슬롯을 집어낼 수 있는지는 파일 안 예시를 봐야 확정된다 — 받아 보지 않고는 모른다.
- 모듈 12개가 한 묶음인 **모드팩**이다(Field Repair, Quickload Mag Saver, Raid Pause, Smart Air Filter, Expanded Special Slots, Compatibility Rules, Compact HUD, Custom Reticle Color, 트립와이어 회수, Breacher, Raid-Long Pain Relief, Update Check). F12 로 개별 off 가 되지만 우리 설치본과 겹치는 게 많다(Special Slots, HUD 계열).
- **Forge 에서 내려갔다.** 제작자가 Forge 규칙 때문에 직접 배포로 돌렸고 지원은 Discord 뿐이다. 설치하면 Forge 쪽 지원을 못 받는 쪽으로 분류된다.
- 서버 시작 때 제작자 GitHub 를 확인하는 Update Check 모듈이 들어 있다(F12 로 끌 수 있다).

## 후보에서 뺀 것

| 모드 | 버전 / 호환 | 왜 아닌지 |
| --- | --- | --- |
| [All In Weapon: Unslotted](https://sp-mod.com/mod/2576/all-in-weapon-unslotted) | 1.2.0 / 4.1.6 | 지금 빼려는 것. 1.2.0 부터 클라이언트 목록만 고쳐 봇은 바닐라로 남긴다. 필터를 **통째로 없애는** 쪽이라 총기별 지정의 반대 |
| [Compatibility Highlighter](https://sp-mod.com/mod/2859/compatibility-highlighter) | 1.3.2 / 4.1.6 | 호환 여부를 **보여주기만** 한다. 규칙은 안 바꾼다 |
| [AC's Attachment Compatibility Highlight](https://forge.sp-tarkov.com/mod/2823/acs-attachment-compatibility-highlight) | - / 4.0.13 | 위와 같은 표시 모드. 게임의 기존 필터·`ConflictingItems` 를 그대로 읽는다 |
| [BETTER ATTACHMENTS](https://sp-mod.com/mod/1939/better-attachments) | 4.1.5 / 4.1.6 | 부착물 인간공학·반동 수치만 건드린다. 칸 호환과 무관 |
| [Weapon Customizer](https://sp-mod.com/mod/1950/weapon-customizer) | 4.0.0 / 4.1.6 | 레일 위치·부착물 좌표 조정. **이미 설치돼 있고** 호환 필터와 무관 |
| [Sensible Universal Weapon Mods](https://hub.sp-tarkov.com/files/file/1148-sensible-universal-weapon-mods/) | - / 3.7.6 | 칸 종류별 전면 개방(핸드가드는 전부 호환 식). 4.x 대응 없음 |
| Weapons And Attachments Customizer | - / 3.7.6 | `specificGunsConfig.json` 으로 총기별 설정이 됐지만 3.7.6 시절이고, **새 Forge 에 페이지가 없다**(옛 hub id 1284 는 다른 모드로 바뀌었다) |
| [Tylevo's Field Attachments](https://sp-mod.com/mods?query=attachment) | 1.2.0 / 4.1.6 | 레이드 중 부착물 교체 UI. 호환 규칙과 무관 |
| [Holster Everything](https://sp-mod.com/mod/2650/holster-everything) | 2.0.0 / 4.1.6 | 홀스터 칸에 들어갈 **무기 분류**를 고른다. 총기별 부착물이 아니다 |
| [Weapon Attachment Modifier](https://forge.sp-tarkov.com/mod/2412/weapon-attachment-modifier) | 1.0.1 / 4.0.13 | 부착물 수치 조정. 4.1 대응 없음 |

## 세 번째 길 — 직접 만들기

이 저장소에는 이미 비슷한 것이 둘 있다.

- [src/SlotCategoryRelax](../src/SlotCategoryRelax) — 부품 **분류**를 골라 그 분류만 칸 필터를 넓힌다. 고를 수 있는 분류는 서버 시작 때 DB 에서 찾아 `discovered-categories.json` 으로 내놓는다.
- [src/HelmetSlotUnion](../src/HelmetSlotUnion) — 헬멧 부착 칸의 합집합.

"총기별 부착물 지정"은 SlotCategoryRelax 를 **무기 단위로 좁히는** 쪽이라 새로 만드는 비용이 크지 않고, Easy Slot Filters 의 세 한계(전역 DB 오염, 제거 불가, 한 부착물 ↔ 한 칸)를 모두 피할 수 있다. 더해서 [src/WebConfigBridge](../src/WebConfigBridge) 가 이미 서버 웹 페이지에서 `All In Weapon` 설정을 고치는 화면을 들고 있으니, 그 카드를 새 모드 쪽으로 돌리면 **총기를 골라 칸별 목록을 편집하는 UI** 까지 재사용된다.

## 아직 안 본 것

- Easy Slot Filters 의 이른 로드 순위(`TraderRegistration - 1`)가 우리 설치본의 DB 수정 모드들보다 **앞인지 뒤인지** 실측하지 않았다. 앞이면 넓힌 칸이 뒤에 오는 모드에 덮일 수 있다.
- SPT Essentials `compatibility.jsonc` 의 실제 구조. 받아서 파일을 열어야 칸 단위 지정 여부가 확정된다.
- 두 후보 모두 `ConflictingItems`(부품끼리 서로 막는 목록)는 건드리지 않는 것으로 보이나, 확인하지 않았다. `All In Weapon` 은 그것까지 지운다고 README 에 적혀 있어, 빼면 **서로 막는 조합이 되돌아온다**.
