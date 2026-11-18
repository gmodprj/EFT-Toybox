# 서버 모드 페이지 일괄 점검 — 추천값/중상급 (2026-11-18)

대상: `https://127.0.0.1:6969/` 의 모드 페이지 전부(Web Config Bridge 제외). ISB 맵·편성·외부 세력은 같은 날 따로 했다(`isb-2h-spawn-plan-2026-11-18.md`).
원칙: **사용자가 바꾼 값과 문서로 남긴 결정은 지킨다.** 기록 없이 바닐라보다 크게 쉬워진 값만 모드 권장값 또는 중상급(바닐라~약간 어려움)으로 돌린다.
백업: `backup/mod-pages-2026-11-18/` (커밋 안 됨, 서버 모드 설정 363개 + BepInEx cfg + CCI `Data/`).
적용 시점: 전부 **서버·게임 재기동 뒤**(UltimateFlea 응답도 `restartRecommended:true`). 아직 재기동 안 함.

## 바꾼 것

| 모드 | 파일 | 바꾼 값 | 이유 |
|---|---|---|---|
| SAIN 4.5.1 | `Solarint-SAIN-ServerMod/Presets/My Preset.json` (강제 프리셋) | `GlobalSettings.Difficulty` Scattering 0.99→**0.65**, PRECISION_SPEED 0.8→**1.1**, ACCURACY_SPEED 1.11→**0.7**, VisibleDist 1→**1.1**, GainSight 1→**1.1**; `General.BotsUseGrenades` false→**true** | 기존 값은 Default(hard)보다 쉬웠다. Default 와 I Like Pain 의 중간으로 맞췄다. 6줄만 글자 치환(CRLF·나머지 보존). `SAIN_EXTRACT_TOGGLE=false`(ORBIT 조건) 유지 |
| Lots of Loot Redux 4.2.2 | `archangelwtf-lotsoflootredux/Presets/default.json` | Loose 7.5 → 바닐라의 약 1.25배(세관 3.0, 팩토리 4.5, 우즈 2.5, 인터체인지·연구소·리저브·등대·GZ·라비린스 3.5, 해안 4.5, 시내 4.0); Static 2→**1.5**; MarkedRoom 4→**2**; RefRoom 2→**1**; `AllowLootOverlay` true→**false**(모드 기본) | 바닐라 2~4배였다 |
| Cultist Circle Improvements 1.1.0 | `config.json`, `Data/ContentBackportCrafts.json` | 보상 배율 0.85–3.15 → **0.7–1.4**, 보너스 0.45→**0.25**, 은신처·퀘스트 보상 시간 60→**21600**초, 고가 기준 1,000,000→**70,000**; 백포트 제작 25건 3초 → **배포본(3960/666초)** | 모드 기본값 = SPT 바닐라. 3.15배·1분은 제단으로 돈을 찍는 수준 |
| Ultimate Flea 1.3.2 | `UltimateFlea/config/economy.json` (모드 API `POST /ultimateflea/api/config/economy`) | demandPerBuy 0.03→**0.04**, priceElasticity 0.3→**0.4**, minPriceFactor 0.7→**0.8**, maxPriceFactor 2.0→**2.5** | 기본값과 `hardcore_scarce` 의 중간. 프리셋 통째 적용은 pool 을 갈아엎어 쓰지 않았다 |
| The Blacklist 3.1.1 | `platinum-theblacklist/Config/config.jsonc` 9행 | `enableScarceOffers` false→**true** | 제작자 주석 "Highly recommend to enable". 글자 편집이라 주석·되살림 블록 그대로(웹 저장은 주석을 지운다) |

## 지킨 것 (현상 유지)

| 모드 | 이유 |
|---|---|
| ORBIT 3.1.1 | 오늘 12:59 웹 저장(스캐브 배회 50%, PMC·플레이어 스캐브 탈출 시점 남은 시간 40–70/50–90%). 사용자 값으로 본다 |
| Skills Extended 3.1.4 | 배포본과 바이트 동일 = 제작자 권장 |
| Enable Labyrinth 1.1.0 | 맵 켜기 모드. `ExfiltrationTime 30`(11-16 웹 저장) 보존 |
| Barter Items Stacks 1.4.3 | 화폐 5천만·GP 5천은 `svm-currency-off.md` 결정. 물물교환품 999 는 프로필 스택(예: Nomad 120개)이 기본 한도를 넘어 내리면 위험 |
| jerobackpack | 배포본 그대로 = 제작자 권장. 줄이면 든 물건이 밀린다 |
| Definitive Weapon Variants 5.0.1 | 스키마 기본값 그대로. 난이도 키 없음 |
| ECOTI 3.5.1 | 배포본 그대로(LL4 / $2000 / 한도 3, 플리 판매 불가) |
| ISB 그 밖의 탭 | 봇 체력·디버그는 기본값, 격리 돌파 30%·정전 15%·연구소 탈출 제한 off·자막 kr 은 사용자 값(`UPDATE-CHECK-2026-11-09.md:412-417`) |
| QuestCodex · Satellite | 설정 없음 / 정보 도구. Satellite 의 상자 내용물·봇 장비·스폰 지점 레이어는 끄면 더 어려워지지만 사용자 취향이라 두었다 |
| UltimateFlea `levels.json` | 사용자가 끈 채 두기로 한 결정(`REVIEW-FIVE-ASKS-2026-11-13.md:189-208`) |

## 확인한 것

- 바꾼 6개 파일은 백업과 다르고 JSON 으로 읽힌다. SAIN 은 백업 대비 정확히 6줄.
- 지킨 13개 파일은 백업과 바이트가 같다.
- CCI 백포트 파일은 제작 시간 말고는 이전과 같다.
- `check-install-records.mjs` → `OK — 0/0`.

## 남은 것

- 서버·게임 재기동 후 확인: SAIN 프리셋 로드, LoL 전리품 체감, CCI 제단 보상, 플리 매물 수(Blacklist 희소 매물 + UltimateFlea).
- **UltimateFlea ↔ TheBlacklist 충돌 — 확인됨, 방향 결정 대기**: 아래 절.

## UltimateFlea 가 TheBlacklist 되살림 674건을 다시 열었다 (같은 날 실측)

떠 있는 서버(11:26 기동, 오늘 바꾼 값은 아직 반영 전)에서 `POST /client/items`(10,674종)와 `POST /client/ragfair/find` 로 쟀다. 세션은 `PHPSESSID=<프로필 id>`, 본문은 zlib(`tools/stm-call.mjs`).

| 묶음 | 막힘 | 열림 |
|---|---|---|
| 되살린 858건 — 10-28 측정 | 785 | 73 (+ AllWeaponsFlea 41 은 그 전에 결정) |
| 되살린 858건 — **지금** | **104** | **754** |
| 그중 UltimateFlea `pool.add` 와 겹치는 674건 | **0** | **674** |
| 겹치지 않는 열림 80건 | — | 핸드북에 없음 73(10-28 과 같은 "플래그만 남은" 것) + 총기 7(AllWeaponsFlea, 결정된 상태) |

- 겹친 674건의 출처: ISB-Aishi 353, WTT-ContentBackport 273, WTT-Armory 34, WTT-Artem 12, Airburst 2.
- **플래그만이 아니라 실제 매물이다.** `ragfair/find` 결과 겹친 품목 3개 모두 상인(`memberType 4`) 4건 + **플리 판매자(`memberType 0`) 7~9건**. 대조군: 아직 막힌 품목(`699b6b88…`)은 상인 4건뿐, 일반 품목(Salewa)은 20건.
- 순서: 로그상 UltimateFlea `Pool applied … +5662 enabled … cleared 1277 from item blacklist`(02:27:09.329) 뒤에 TheBlacklist 가 돈다(02:27:10.743). 그런데도 열려 있으니 TheBlacklist 가 이 674건을 다시 닫지 못한다 — 결과가 UltimateFlea 쪽이다. 어떤 코드 경로로 지는지는 읽지 않았다(결과만 실측).
- 원인: 11-16 18:30 에 적용된 UltimateFlea 프리셋 `modded_items_flea` 가 "모드가 판매 불가로 표시한 아이템"까지 풀에 올린다. 10-28 에 "UltimateFlea 는 무죄"였던 것은 그때 풀이 비어 있었기 때문이다.

선택지:
1. `pool.add` 에서 TheBlacklist 되살림 id 674개만 뺀다(UltimateFlea API `POST /ultimateflea/api/config/pool`, 나머지 4,988개는 유지). 제작자 의도대로 다시 막힌다. 서버 재기동 필요.
2. 그대로 둔다 — 모드 아이템 전부 플리 허용을 고른 것으로 본다. 그러면 TheBlacklist 되살림 블록은 이 674건에 대해 효과가 없다.
- 기록 없이 바뀌어 있던 값: BIS 클라이언트 FiR 섞기 4종 true, ISB 격리 돌파 확률 55→30. 둘 다 그대로 두었다.
