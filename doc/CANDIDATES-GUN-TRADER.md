# 총기·파츠 / 상인 추가 모드 재조사 (2026-10-25)

이미 있는 [CANDIDATES-416.md](CANDIDATES-416.md)는 이름·소개의 키워드로 5개 분야를 추렸고, [PLAN-STAGE5.md](PLAN-STAGE5.md)에서 그중 52개를 설치했다. 여기서는 **총기·파츠**와 **상인·플레아** 두 분야만 다시, 키워드를 넓혀 훑었다. 앞선 추림에서 분야 분류를 비켜간 것과 그 뒤 올라온 것을 찾는 것이 목적이다.

## 어떻게 훑었나

- Forge API `GET /mods?filter[spt_version]=4.1.6` 전수 — **633개** (10-24 조사 시점 626개, 7개 증가).
- 이름·소개를 부착물·조준경·소음기·탄창·빌드·반동 / 상인·플레아·물물교환·보험·수리 계열 정규식으로 걸러 **181개**.
- 그중 설치본·기존 후보 문서·5차 계획서에 한 번도 나오지 않은 것이 **81개**. 원자료는 `stage2-survey/sweep-416-gun-trader.json`.
- 81개 중 두 분야에 실제로 해당하는 것만 `tools/mod-detail.mjs`로 버전·용량·결속·의존성을 확인했다(아래 표). 나머지는 효과·연출·퀘스트·개발 도구 쪽이라 뺐다.

## 총기·파츠 (새로 확인한 것)

| 모드 | id | 버전 | 크기 | 다운로드 | 표시 | 의존성 | 메모 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [Kalashnikov Enhanced Modding](https://sp-mod.com/mod/3010) | 3010 | 4.1.2 | 260MB | 2,204 | **결속** | - | AK 상·하부 핸드가드·가스튜브·사이드레일·**배럴을 분리해 부품화**. 조합 94종 추가. 이번 조사에서 "총기 파츠"에 가장 정확히 맞는 것 |
| [Vanguard](https://sp-mod.com/mod/2917) | 2917 | 2.0.0 | 18MB | 2,590 | **결속** | ~~WTT - CommonLib~~(설치됨) | Insight WMLX 라이트 + Dead Air Sandman X 소음기(흑·FDE). 부품만 추가, 용량 작음 |
| [No Barrel Shadow](https://sp-mod.com/mod/2780) | 2780 | 2.0.1 | 10KB | 17,337 | **결속** | - | 라이트·전술 장비가 총구에 드리우는 그림자 제거 + F12로 조명별 밝기 조절 |
| [BrightLasers](https://sp-mod.com/mod/1358) | 1358 | 3.0.0 | 12KB | 130,787 | - | - | 레이저 밝기·굵기 F12 조절. 3.0.0에서 번들을 떼어 WTT Content Backport 충돌이 사라졌다 |
| [Better (Picatinny) OKP](https://sp-mod.com/mod/2616) | 2616 | 2.0.0 | 2.1MB | 6,541 | **결속** | - | 피카티니 OKP 조준점을 큰 빨간 원 → 녹색 셰브론으로. 단순 레티클 교체 |
| [SamSWAT's Anime Rear Sights](https://sp-mod.com/mod/2425) | 2425 | 1.4.1 | 3.9MB | 10,532 | - | - | 애니메이션 그림 백업 사이트 24종(Mechanic·플레아 판매). 취향 문제 |
| [BackpackGunSlot](https://sp-mod.com/mod/3141) | 3141 | 1.0.1 | 49KB | 3,907 | **결속** | - | 모든 배낭에 격자를 먹지 않는 "ON PACK" 총기 슬롯. 가방 크기에 따라 들어가는 총 등급이 다름 |
| [DailyGunsmith](https://sp-mod.com/mod/3188) | 3188 | 0.1.1 | 72KB | 188 | **결속** | - | 매일 무작위 총기+부착물 조립 과제. 조건·보상 설정 폭이 넓다. 버전 0.1.x |
| [Gunsmith Tweaks](https://sp-mod.com/mod/1866) | 1866 | 4.1.0 | 47KB | 14,555 | **결속** | - | 건스미스 과제에서 Mechanic이 **기본 총기를 지급**(개조는 직접). 설치된 Gunsmith Task Trader(3124)와 목적이 겹침 → 택일 |
| [All In Weapon: Unslotted](https://sp-mod.com/mod/2576) | 2576 | 1.2.0 | 15KB | 7,952 | - | - | 슬롯·호환 제한 해제(아무 부품·탄창·탄약). 1.2.0부터 봇 무기 생성은 바닐라 유지. 치트성 |
| [Russian Special Forces Pack](https://sp-mod.com/mod/3035) | 3035 | 2.0.1 | 560MB | 9,341 | **결속** | ~~WTT - CommonLib~~(설치됨) | 러시아제 총기 14종 + Zenitco 핸드가드. 대형 콘텐츠 |
| [JSOC Service Weapon Replacement Program](https://sp-mod.com/mod/2737) | 2737 | 3.0.1 | 390MB | 16,983 | **결속** | **Epic's All in One**(1263, 1.5GB), ~~WTT - CommonLib~~(설치됨) | 서방 소총 9종 + 6mm ARC 탄계열. Epic's AIO가 선행 조건이라 사실상 2GB |
| [MassivesoftWeapons Updates](https://sp-mod.com/mod/3127) | 3127 | 2.0.1 | 26KB | 1,953 | **결속** | **MassivesoftCore Update 4.1.5**(3126) | 원본 4.0.13 모드를 깐 뒤 패치를 덮는 방식. 설치 절차가 복잡해 권하지 않음 |

## 상인·플레아 (새로 확인한 것)

| 모드 | id | 버전 | 크기 | 다운로드 | 표시 | 의존성 | 메모 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [NATO Quartermaster](https://sp-mod.com/mod/3030) | 3030 | 1.1.3 | 354KB | 4,418 | **결속** | - | 서방제 총기·방어구·탄약·완성 빌드를 파는 커스텀 상인 + 퀘스트 6단. 4.1.6 정확 표기, 의존성 없음. "치트 상인은 아니게" 비싸게 파는 설계 |
| [OpenBarters](https://sp-mod.com/mod/3194) | 3194 | 1.0.1 | 14KB | 385 | - | - | 물물교환 대금을 **상인이 사주는 아무 아이템으로** 지불(합산 가치 충족 시). 비결속·무의존 |
| [Moe-TradeMarker](https://sp-mod.com/mod/2736) | 2736 | 1.2.6 | 99KB | 1,003 | **결속** | - | 상인에게서 산 물건에 배지 표시 + 플레아 재판매 차단(선택). QuickSell 연동 명시 → 설치된 QuickSellFlea와 함께 쓸 수 있음 |
| [Ref Rework](https://sp-mod.com/mod/3014) | 3014 | 1.0.0 | 17KB | 2,765 | - | - | 도그태그를 Ref에 팔아 GP 코인으로. 설치된 Ledger(3085, 도그태그 상인)와 역할 겹침 → 택일 |
| [Realistic Insurance](https://sp-mod.com/mod/2949) | 2949 | 0.2.1 | 38KB | 2,037 | - | - | 보험 회수율을 "누가 날 죽였고 그가 살아 나갔는지"로 계산. SVM 보험 설정과 겹치는지 확인 필요 |
| [Item Valuation](https://sp-mod.com/mod/2021) | 2021 | 2.1.0 | 18KB | 30,451 | - | ~~Color Converter API~~(설치됨, 색 지정용) | 가격·방어 등급·관통으로 아이템 색칠. 설치된 Show Me The Money와 표시가 겹침 |
| [All Weapons Flea](https://sp-mod.com/mod/3200) | 3200 | 1.0.1 | 8KB | 306 | - | - | 상인 전용 총기(MDR 7.62, Mk-18 등)를 플레아에서 거래 가능하게. 10-23 최초 공개 |
| [SPT Barterizer](https://sp-mod.com/mod/3195) | 3195 | 0.1.3 | 27KB | 119 | **결속** | - | 상인의 현금 매물을 물물교환으로 변환(기존 교환·충성도·재고 유지). OpenBarters 연동 선택. 0.1.x, 10-22 공개 |
| [Ultimate Flea](https://sp-mod.com/mod/2844) | 2844 | 1.2.0 | 47KB | 3,752 | **결속** | - | 플레아 전면 통제(품목 풀·가격·Tarkov.dev 평균·수급 경제·레벨 락). 보류 중인 Better Flea(3177)와 택일 |
| [Flea Price Progression](https://sp-mod.com/mod/3070) | 3070 | 1.1.0 | 609KB | 1,303 | - | - | 플레아 가격이 내 PMC 레벨에 따라 변함. 1131·2844와 가격 소스가 겹침 |
| [Live Flea Prices](https://sp-mod.com/mod/1131) | 1131 | 2.1.0 | 10KB | 297,987 | - | - | Tarkov.dev 실시간 가격을 매시간 반영. 표기 `~4.1.1`이라 4.1.6 단독 검증 필요 |
| [Boxes At Ref (BARF) - NG](https://sp-mod.com/mod/2483) | 2483 | 3.0.0 | 9KB | 24,158 | **결속** | - | Arena 전리품 상자류를 Ref 판매 목록에 추가 |
| [Couturier](https://sp-mod.com/mod/2239) | 2239 | 2.0.4 | 434MB | 51,099 | **결속** | - | 장비·의류 리텍스처 180종 + 전용 상인 Couturier. 대형 콘텐츠 |
| [Vagabond](https://sp-mod.com/mod/2642) | 2642 | 0.8.3 | 158KB | 7,349 | **결속** | - | 오픈월드 개편(Fence에서 시작, 이동은 전부 트랜짓). 상인보다 게임 전체 개편 — 방향 결정이 먼저 |

## 넣기 전에 정해야 하는 것

1. **대형 콘텐츠를 넣을지** — 3010(260MB)·3035(560MB)·2239(434MB)·2737(2GB급). 5차 계획에서 이미 "총기 팩·대형 상인은 범위 밖"으로 뒀다. 넣는다면 6차로 따로, 하나씩.
2. **겹치는 짝은 택일** — 1866 ↔ 설치된 3124(건스미스), 3014 ↔ 설치된 3085(도그태그), 2021 ↔ 설치된 Show Me The Money, 2844/3070/1131 ↔ 보류 중인 3177(플레아 가격·경제).
3. **플레아·보험·경제는 SVM과 먼저 비교** — 2844·3070·2949·3200은 SVM 설정으로 이미 되는 범위가 있다. SVM 쪽을 먼저 쓰는 기존 방침 유지.
4. **결속 모드는 하나씩** — 표의 **결속** 표시는 프로필 결속. 프로필 백업 후 한 단계에 하나만 넣는다([PROCEDURE.md](PROCEDURE.md)).
5. **표기가 4.1.6이 아닌 것** — 1131(`~4.1.1`)·2616(`~4.1.0`)·2425(`~4.1.0`)·2736(`~4.1.0`)·3010(`~4.1.5`)·3127(`~4.1.5`). 단독 설치 + 서버 테스트로 확인한 뒤에 쓴다.

## 바로 넣기 쉬운 것 (참고)

용량 작고, 결속이 없거나 되돌리기 쉽고, 설치본과 역할이 겹치지 않는 것들이다. 순서는 부담이 적은 쪽부터.

| 순위 | 모드 | 분야 | 왜 |
| --- | --- | --- | --- |
| 1 | [OpenBarters](https://sp-mod.com/mod/3194) (3194) | 상인 | 14KB, 비결속, 무의존, 4.1.x 표기. 물물교환 불편만 덜어 준다 |
| 2 | [BrightLasers](https://sp-mod.com/mod/1358) (1358) | 파츠 | 12KB, 비결속, 번들 제거로 충돌 사라짐 |
| 3 | [No Barrel Shadow](https://sp-mod.com/mod/2780) (2780) | 파츠 | 10KB. 결속 표시가 있으나 클라이언트 전용 |
| 4 | [NATO Quartermaster](https://sp-mod.com/mod/3030) (3030) | 상인 | 354KB, 4.1.6 정확, 의존성 없음. 결속이라 단독 단계 |
| 5 | [Vanguard](https://sp-mod.com/mod/2917) (2917) | 파츠 | 18MB. 라이트·소음기만 추가, 의존성은 이미 설치됨 |

## 이번 조사에서 뺀 것 (분야 밖)

81개 중 아래 성격은 두 분야가 아니라고 보고 표에 넣지 않았다. 원자료 JSON에는 전부 남아 있다.

- 연출·성능: Fontaine's FOV Fix(701), HollywoodFX(2003), POMSix(3100), ReShade 프리셋(2235)
- 퀘스트·진행: Quest Backport(3026), QCAdjustments(1867), Kill&Bill(2810), Quest Codex(3054), PityLoot(3077)
- 아이템 단발 추가: Walther WA 2000(3063), China Lake(3056), 스팀(2779), 방어판 수리 키트(2973)
- 바디·전투 규칙: Headshot Damage Redirection(2953), ASS(1946), Hideout 계열(2982·2705·3167)
- 개발·도구: SPT Scaffold(2633), SPT Dev Kit(3039), SIM(2906), SSPTMM(3150), Mini-Launcher(757)
- 하드코어 규칙: TrueHardcoreRules(3120), Hardcore Progression(3208), NUTS(1936), FIR Flea only(3028)
