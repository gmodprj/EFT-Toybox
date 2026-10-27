# 7차 후보 — 상인 / 탄환 / 총기 파츠 / 방어구 (2026-10-25)

**밸런스는 따지지 않는다.** 지금까지는 "치트성"·"바닐라 설계를 벗어남"을 이유로 뺀 것이 많았는데, 이번에는 그 기준을 적용하지 않고 네 분야를 전부 다시 훑었다. 그래서 앞선 [CANDIDATES-416.md](CANDIDATES-416.md)·[PLAN-STAGE5.md](PLAN-STAGE5.md)에서 뺐던 것도 다시 올렸다.

## 조회 방식을 바꿨다

지난번까지는 이름·소개의 키워드로만 추렸고, 그 탓에 ECOTI(클립온 열상, 다운로드 15,943)를 두 번이나 놓쳤다. 이번에는 **Forge 카테고리**를 함께 썼다.

- 4.1.6 호환 **633개**를 카테고리까지 받아 왔다: Other 176 / Tools 65 / Items 59 / Overhauls 56 / Scripting 37 / **Weapons 35** / **Equipment 31** / **Traders 30** / Bots 28 / Quests 22 / Hideout 19 / Locations 17 / Audio 16 / Clothing 15 / 그 외.
- 네 분야 = 해당 카테고리(Traders·Weapons·Equipment) + 분야별 정규식을 **합집합**으로 걸렀다. 설치본 109개를 뺀 결과 **165개**가 걸렸고, 그중 **84개는 기존 문서에 한 번도 안 나온 것**이다.
- 추려낸 것 중 네 분야에 실제로 해당하는 **65개**를 `tools/survey-candidates.mjs`로 버전·용량·결속·의존성까지 확인했다(원자료 `stage2-survey/survey-stage7-416.json`).

**방어구는 이번이 처음**이다. 1~6차에서 다룬 적이 없어 거의 전부가 새 후보다.

## 방어구 (20개) — 새 분야

| 모드 | id | 버전 | 크기 | 다운로드 | 결속 | 의존성 | 메모 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [WTT - Pack 'n' Strap](https://sp-mod.com/mod/1278) | 1278 | 2.1.1 | 302MB | 239,072 | **결속** | ~~WTT - CommonLib~~(설치됨) | 소형 케이스 + **배틀벨트 모듈 장비**. 이 분야에서 가장 많이 쓰인다. 용량이 크다 |
| [Less Restricting Headwear](https://sp-mod.com/mod/922) | 922 | 4.0.0 | 17KB | 137,095 | **결속** | - | 헬멧·헤드셋·마스크·페이스실드를 **같이 쓰지 못하게 막던 제한 해제** |
| [Foldables](https://sp-mod.com/mod/2422) | 2422 | 1.1.1 | 70KB | 49,849 | - | - | 배낭과 조끼를 접어 자리를 줄인다. 비결속 |
| [Wolfik's Heavy Trooper Masks](https://sp-mod.com/mod/1569) | 1569 | 1.2.0 | 18MB | 46,954 | **결속** | - | 실제 방어력이 있는 중장갑 마스크 5종 |
| [Unrestricted Armor Plate](https://sp-mod.com/mod/1878) | 1878 | 2.1.0 | 15KB | 23,369 | - | - | **아무 방어판이나 아무 조끼·리그에** 꽂을 수 있게 한다. 비결속 |
| [Jero Backpack](https://sp-mod.com/mod/2369) | 2369 | 4.1.1 | 32KB | 21,847 | **결속** | - | 배낭 격자 크기를 키운다. 크기 지정·제외 목록 지원 |
| [Black Exfil Revision](https://sp-mod.com/mod/2380) | 2380 | 1.1.0 | 8KB | 16,869 | **결속** | - | Black Exfil 마스크에 페이스실드를 다시 달 수 있게 한다 |
| [LMSP - Let Me Switch Plates](https://sp-mod.com/mod/2814) | 2814 | 1.0.1 | 3KB | 9,655 | - | - | 레이드 중 **방어구를 벗지 않고 방어판만 교체**. 3KB, 비결속 |
| [Change Helmet Visor](https://sp-mod.com/mod/2554) | 2554 | 3.0.0 | 46KB | 7,438 | - | - | 페이스실드·바이저가 화면에 그려지는 방식을 전환한다 |
| [VisorEffectManager](https://sp-mod.com/mod/2429) | 2429 | 1.1.0 | 4KB | 5,733 | - | - | 바이저의 시각 효과(김서림·빗물 등)를 켜고 끈다 |
| [Better OKP-7](https://sp-mod.com/mod/2711) | 2711 | 1.1.0 | 4.0MB | 4,115 | **결속** | ~~WTT - CommonLib~~(설치됨) | OKP-7 레티클 교체(도브테일·피카티니). 분류는 Equipment지만 실제로는 조준경 쪽 |
| [Restore Armor Plates](https://sp-mod.com/mod/2973) | 2973 | 1.0.2 | 26KB | 3,371 | **결속** | - | 전용 키트로 방어판 최대 내구도를 원래대로 되돌린다 |
| [Backpack Resizer](https://sp-mod.com/mod/2996) | 2996 | 1.1.0 | 52KB | 3,176 | **결속** | - | 분할 격자가 없는 배낭의 가로·세로를 바꾼다. 2369와 역할 중복 |
| [SOMF - SitOnMyFace](https://sp-mod.com/mod/3105) | 3105 | 0.1.2 | 109KB | 3,006 | - | - | 얼굴 슬롯에 장비를 여러 개 겹쳐 착용 |
| [Armor Expert](https://sp-mod.com/mod/3106) | 3106 | 3.1.0 | 10KB | 1,951 | - | - | 방어 장비·헤드셋의 **숨은 수치 표시**. Munitions Expert(3006)의 방어구판 |
| [Equipment Is Eternal](https://sp-mod.com/mod/2897) | 2897 | 1.2.0 | 14KB | 1,668 | **결속** | - | 죽어도 장비를 잃지 않는다(슬롯별 설정) |
| [Fair Equipment Restoration](https://sp-mod.com/mod/2575) | 2575 | 1.1.0 | 23KB | 1,492 | **결속** | - | 2897보다 온건한 쪽 — 마모·소모는 유지한 채 장비를 돌려준다 |
| [Repair Skill Extension](https://sp-mod.com/mod/2556) | 2556 | 1.4.1 | 15KB | 9,496 | **결속** | - | 수리 버프 확률 상향 + **모든 방어구에 수리 스킬 적용** |
| [BiC - Bind in Container](https://sp-mod.com/mod/3190) | 3190 | 1.0.1 | 3KB | 421 | - | - | 보안 컨테이너 안 물건에도 단축키를 건다 |
| [ArmorInfoPlus](https://sp-mod.com/mod/3049) | 3049 | 1.0.0 | 9KB | 365 | - | - | 모든 아이템의 24자 템플릿 ID 표시(개발·조사용) |

## 상인 (14개)

이번 기준으로 다시 올린 것이 대부분이다. 괄호 안이 예전에 뺐던 이유다.

| 모드 | id | 버전 | 크기 | 다운로드 | 결속 | 의존성 | 메모 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [bluehead's AIO Trader](https://sp-mod.com/mod/374) | 374 | 5.3.1 | 41KB | 154,878 | **결속** | - | 모든 물품을 한 상인에서 + 열쇠 일괄 지급 (예전: 치트성 강함) |
| [ALL AMMO TRADER](https://sp-mod.com/mod/1554) | 1554 | 1.5.0 | 2.8MB | 55,842 | **결속** | - | 모든 탄약을 파는 상인 (예전: 치트성). **탄환 분야와 함께 보면 가장 간단한 해답** |
| [AllGoodsTrader](https://sp-mod.com/mod/2648) | 2648 | 2.0.0 | 589KB | 3,857 | **결속** | - | 이중 언어 상인 4명. 대부분 아이템 매입 + 수리·보험 (예전: 치트성) |
| [The Quartermaster](https://sp-mod.com/mod/2817) | 2817 | 1.4.3 | 5.7MB | 13,684 | **결속** | ~~WTT - CommonLib~~(설치됨), **ItemGen**(2803) | 플레이어 중고 장터 (예전: ItemGen 동반 필요 — 지금도 그 하나가 는다) |
| [Instant Insurance](https://sp-mod.com/mod/2394) | 2394 | 1.1.2 | 38KB | 37,853 | - | - | 죽어도 보험 아이템 유지 (예전: 치트성). 설치된 Realistic Insurance와 **정반대 방향** → 택일 |
| [Item Limits Begone!](https://sp-mod.com/mod/2524) | 2524 | 1.1.0 | 5KB | 19,383 | - | - | 레이드·인벤토리·플레아 등록 제한 해제 (예전: SVM과 중복) |
| [Limitless – Unlimited Buy Limits](https://sp-mod.com/mod/2175) | 2175 | 1.0.0 | 27KB | 5,413 | **결속** | - | 상인 구매 제한 해제·배수 (예전: SVM과 중복) |
| [TraderGen](https://sp-mod.com/mod/2749) | 2749 | 2.4.1 | 1.3MB | 6,596 | **결속** | ~~WTT - CommonLib~~(설치됨) | 커스텀 상인 제작 프레임워크. 직접 상인을 만들 때 |
| [500 Euro for Skier](https://sp-mod.com/mod/2434) | 2434 | 2.0.0 | 8KB | 4,490 | **결속** | - | **새로 찾은 것.** 스키어 거래를 루블이 아닌 유로로 |
| [Quick Sell](https://sp-mod.com/mod/2955) | 2955 | 4.3.0 | 50KB | 10,056 | - | - | 최적 상인/플레아 즉시 판매 + 툴팁 가격. 설치된 QuickSellFlea와 중복 → 교체 검토 |
| [Live Flea Prices](https://sp-mod.com/mod/1131) | 1131 | 2.1.0 | 10KB | 298,027 | - | - | Tarkov.dev 실시간 가격을 매시간 반영. 표기 `~4.1.1` |
| [Ultimate Flea](https://sp-mod.com/mod/2844) | 2844 | 1.2.0 | 47KB | 3,758 | **결속** | - | 플레아 전면 통제(품목·가격·수급·레벨 락) |
| [Better Flea](https://sp-mod.com/mod/3177) | 3177 | 2.1.1 | 359KB | 865 | - | - | 실섭 플레아 재현. 2844와 택일 |
| [Boxes At Ref (BARF)](https://sp-mod.com/mod/2483) | 2483 | 3.0.0 | 9KB | 24,173 | **결속** | - | Arena 전리품 상자를 Ref 판매 목록에 추가 |

## 탄환 (11개)

| 모드 | id | 버전 | 크기 | 다운로드 | 결속 | 의존성 | 메모 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [Easy Ammo Names](https://sp-mod.com/mod/1262) | 1262 | 2.1.0 | 16KB | 30,954 | - | - | 탄약 이름을 알아보기 쉽게. 설치된 Ammo Stats·Caliber Under Name과 표기가 겹칠 수 있다 |
| [Ammo Stats In Names](https://sp-mod.com/mod/2989) | 2989 | 1.0.0 | 16KB | 1,204 | - | - | 이름에 관통·데미지. 1262와 택일 |
| [Munitions Expert - Reloaded](https://sp-mod.com/mod/3006) | 3006 | 3.1.0 | 14KB | 4,228 | - | - | 탄약·폭발물 숨은 수치. **Armor Expert(3106)와 같은 제작자 계열** — 둘을 함께 쓰면 정보가 맞물린다 |
| [.22lr Ammunition](https://sp-mod.com/mod/2516) | 2516 | 2.0.0 | 29MB | 28,322 | **결속** | ~~WTT - CommonLib~~(설치됨) | .22lr 탄종 추가. C11(2423)을 넣을 때의 선행 조건이기도 하다 |
| [Manimal's Ammo Loading Animations](https://sp-mod.com/mod/2681) | 2681 | 2.0.0 | 19MB | 23,078 | **결속** | ~~Continuous Load Ammo~~(설치됨), ~~WTT - CommonLib~~(설치됨) | 한 발씩 장전하는 1인칭 애니메이션. 의존성 둘 다 이미 있다 |
| [AmmoGen](https://sp-mod.com/mod/2777) | 2777 | 2.4.0 | 64KB | 7,325 | **결속** | - | 커스텀 탄약·탄약 상자 제작 프레임워크 |
| [Bela.s.Speedloader](https://sp-mod.com/mod/2458) | 2458 | 1.2.4 | 23KB | 10,170 | - | - | 장전 속도·탄 스택 상향 (예전: 치트성). 비결속이라 되돌리기 쉽다 |
| [Infinite Everything](https://sp-mod.com/mod/3108) | 3108 | 2.5.3 | 56KB | 2,645 | - | - | 탄약·탄창·수류탄·의료품 등 18종 무한 옵션 (예전: SVM과 중복) |
| [Slimmer Magazines](https://sp-mod.com/mod/2373) | 2373 | 2.0.0 | 6KB | 10,535 | **결속** | - | 2x2 탄창을 1x2로 (예전: 결속이라 되돌리기 어려움) |
| [Shorter Magazines](https://sp-mod.com/mod/3186) | 3186 | 1.1.0 | 7KB | 928 | **결속** | - | 3x1 탄창을 2x1로. 2373과 **같이 넣지 말 것** |
| [Manimal's Quick Cancel](https://sp-mod.com/mod/2969) | 2969 | 2.0.0 | 4KB | 3,106 | - | - | **새로 찾은 것.** 소비 동작을 재장전 키로 즉시 취소 |

## 총기 파츠 (20개)

| 모드 | id | 버전 | 크기 | 다운로드 | 결속 | 의존성 | 메모 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [Tactical Gear Component](https://sp-mod.com/mod/1125) | 1125 | 3.0.0 | 626MB | 308,610 | **결속** | **Painter**(1025), ~~WTT - CommonLib~~(설치됨) | **새로 찾은 것.** 장비·부착물 대량 추가. Painter와 **서로를 요구한다** |
| [Painter](https://sp-mod.com/mod/1025) | 1025 | 3.0.0 | 21MB | 318,902 | **결속** | **Tactical Gear Component**(1125), ~~WTT - CommonLib~~(설치됨) | 도장 부품·장비를 파는 상인 Painter. 위와 한 쌍 — 합치면 **647MB** |
| [BETTER ATTACHMENTS](https://sp-mod.com/mod/1939) | 1939 | 4.1.5 | 54KB | 33,785 | **결속** | - | 모든 부착물의 인간공학·반동 개선 (예전: 치트성). 54KB로 효과가 가장 크다 |
| [Better Rear Sights](https://sp-mod.com/mod/1591) | 1591 | 1.8.0 | 52MB | 57,042 | **결속** | - | 기계식 가늠자 가시성 개선 |
| [Canted Aiming](https://sp-mod.com/mod/2415) | 2415 | 2.0.0 | 11KB | 22,595 | **결속** | ~~WTT - CommonLib~~(설치됨) | 백업 사이트 없이 경사 조준 |
| [Holster Everything](https://sp-mod.com/mod/2650) | 2650 | 2.0.0 | 37KB | 7,257 | **결속** | - | 홀스터 슬롯에 넣을 수 있는 품목 확대 |
| [Fold That Stock](https://sp-mod.com/mod/2691) | 2691 | 2.1.0 | 56MB | 4,566 | **결속** | - | 비바닐라 접이식 개머리판을 실제로 접히게 |
| [Kalashnikov Enhanced Modding](https://sp-mod.com/mod/3010) | 3010 | 4.1.2 | 260MB | 2,209 | **결속** | - | AK 핸드가드 상·하부·가스튜브·사이드레일·**배럴 분리**, 조합 94종. 파츠 자체를 늘리는 데는 이게 1순위 |
| [Magic Optic Mount](https://sp-mod.com/mod/3101) | 3101 | 0.1.0 | 19KB | 1,316 | **결속** | ~~Weapon Customizer~~(설치됨), ~~WTT - CommonLib~~(설치됨) | 광학 + 열상/야투 **동시 장착**. 설치된 ECOTI와 성격이 겹친다 |
| [All In Weapon: Unslotted](https://sp-mod.com/mod/2576) | 2576 | 1.2.0 | 15KB | 7,959 | - | - | 슬롯·호환 제한 전면 해제(아무 부품·탄창·탄약). 봇 무기 생성은 바닐라 유지 |
| [Cobra's Durability Tweaks](https://sp-mod.com/mod/3019) | 3019 | 1.0.0 | 12KB | 1,014 | **결속** | - | 내구도 소모·고장 축소 또는 제거 |
| [Repair Max Durability](https://sp-mod.com/mod/1305) | 1305 | 2.2.0 | 30KB | 38,829 | **결속** | - | 무기 최대 내구도를 복구하는 아이템 추가 |
| [Realistic Suppressors](https://sp-mod.com/mod/3171) | 3171 | 1.0.0 | 8KB | 1,059 | **결속** | - | 소음기 성능 대폭 강화. 소개부터 "밸런스 따위" |
| [Definitive Weapon Variants](https://sp-mod.com/mod/2010) | 2010 | 4.3.2 | 143KB | 44,041 | **결속** | - | 기존 총기 기반 변형 600종+. 용량 대비 추가량이 가장 좋다 |
| [Full Automatic Guns](https://sp-mod.com/mod/1972) | 1972 | 4.1.5 | 24KB | 3,680 | **결속** | - | **새로 찾은 것.** 일부 SMG·소총·카빈을 자동 사격으로 |
| [Peacemaker Continued](https://sp-mod.com/mod/2414) | 2414 | 2.1.0 | 45MB | 9,665 | **결속** | - | **새로 찾은 것.** 총기 추가 팩 |
| [Quick Aim](https://sp-mod.com/mod/2985) | 2985 | 1.0.0 | 6KB | 1,770 | - | - | **새로 찾은 것.** 앉기·엎드리기·거치 상태에서 조준 유지 시간 증가. 비결속 |
| [Airburst](https://sp-mod.com/mod/2948) | 2948 | 1.1.2 | 29KB | 1,523 | **결속** | ~~WTT - CommonLib~~(설치됨) | **새로 찾은 것.** 엄폐물 위에서 터지는 40mm 공중폭발탄 |
| [Sage's Incendiary Grenade](https://sp-mod.com/mod/3113) | 3113 | 1.1.0 | 24MB | 2,617 | **결속** | **BigBrain**(902) | **새로 찾은 것.** 소이 수류탄. 봇 AI 라이브러리가 하나 는다 |
| [ShrapnelFix](https://sp-mod.com/mod/3118) | 3118 | 1.0.0 | 14KB | 1,847 | **결속** | - | **새로 찾은 것.** 수류탄 파편이 몸 높이로 날아가게 고친다 |

## 넣는다면 이 순서

밸런스를 안 따지므로 **효과 대비 부담**만 본다.

| 순위 | 모드 | 분야 | 왜 |
| --- | --- | --- | --- |
| 1 | [LMSP](https://sp-mod.com/mod/2814) (2814) | 방어구 | 3KB·비결속. 레이드 중 방어판 교체 |
| 2 | [Unrestricted Armor Plate](https://sp-mod.com/mod/1878) (1878) | 방어구 | 15KB·비결속. 방어판 호환 제한 해제 |
| 3 | [Armor Expert](https://sp-mod.com/mod/3106) (3106) + [Munitions Expert](https://sp-mod.com/mod/3006) (3006) | 방어구·탄환 | 둘 다 10여 KB·비결속. 숨은 수치 표시가 맞물린다 |
| 4 | [Foldables](https://sp-mod.com/mod/2422) (2422) | 방어구 | 70KB·비결속. 배낭·조끼 접기 |
| 5 | [Less Restricting Headwear](https://sp-mod.com/mod/922) (922) | 방어구 | 17KB. 결속이지만 파일이 작고 효과가 즉시 보인다 |
| 6 | [BETTER ATTACHMENTS](https://sp-mod.com/mod/1939) (1939) | 파츠 | 54KB로 모든 부착물 성능 개선 |
| 7 | [Definitive Weapon Variants](https://sp-mod.com/mod/2010) (2010) | 파츠 | 143KB로 변형 600종 |
| 8 | [ALL AMMO TRADER](https://sp-mod.com/mod/1554) (1554) | 상인·탄환 | 2.8MB. 탄약 수급을 한 번에 해결 |

## 같이 정해야 하는 것

- **Painter ↔ Tactical Gear Component**(1025+1125)는 서로를 요구한다. 넣으려면 **647MB를 한 묶음으로**. 둘 중 하나만은 불가능하다.
- **대형 콘텐츠** — Pack 'n' Strap 302MB, Kalashnikov 260MB, Fold That Stock 56MB, Better Rear Sights 52MB, Peacemaker 45MB. 7차를 대형까지 갈지 먼저 정한다.
- **택일** — 2373 ↔ 3186(탄창 크기), 2369 ↔ 2996(배낭 크기), 1262 ↔ 2989(탄약 이름), 2844 ↔ 3177(플레아), 2897 ↔ 2575(장비 보존), 2955 ↔ 설치된 QuickSellFlea.
- **정반대 방향** — 2394 Instant Insurance는 6차에 넣은 Realistic Insurance(2949)와 정면으로 어긋난다. 하나를 끄는 전제로만 넣는다.
- **설치본과 겹침** — 3101 Magic Optic Mount는 ECOTI(2930)와, 3106 Armor Expert는 ODT Item Info와 표기가 겹칠 수 있다.
- **새 의존성** — ItemGen(2803, The Quartermaster용), BigBrain(902, Sage's Incendiary Grenade용). 둘 다 지금은 없다.

## 이 문서에서 하지 않는 것

- 다운로드·설치를 하지 않는다. 무엇이 있는지와 부담만 정리했다.
- 밸런스 평가를 하지 않는다. "치트성"이라는 이유로 거른 것이 없다. 게임을 어떻게 만들지는 사용자가 고른다.
