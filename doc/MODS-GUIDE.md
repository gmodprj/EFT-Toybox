# 설치된 모드 설명

> 2026-10-24 기준 **93개**. 무엇을 하는 모드인지 기능별로 묶어 적은 문서.
> 버전·설치 파일 수·설치 기록은 [INSTALLED-MODS.md](INSTALLED-MODS.md)(자동 생성), 작업 경과는 [WORKLOG.md](WORKLOG.md).

- 대상: `E:\Spt` (SPT 4.1.6, EFT 0.16.9.40743)
- **위치** — `게임`은 `BepInEx\plugins`, `서버`는 `SPT_Runtime\user\mods`, `루트`는 `E:\Spt` 바로 아래.
- **결속** = Forge "프로필 결속" 표시. 설치하고 플레이한 뒤 제거하면 프로필이 깨질 수 있다.
- 설명은 모드 소개와 이번 설치에서 실제로 확인한 동작을 합쳐 적었다.

## 기반 라이브러리 (2개)

다른 모드가 요구하는 것. 단독으로는 눈에 보이는 변화가 없다.

| 모드 | 버전 | 단계 | 위치 | 설명 |
| --- | --- | --- | --- | --- |
| [Color Converter API](https://sp-mod.com/mod/1090) | 1.2.0 | 1차 | 게임 | 아이템 배경색에 16진수 색상 코드를 쓸 수 있게 하는 클라이언트 API. 색을 쓰는 모드들의 선행 조건. |
| [WTT - CommonLib](https://sp-mod.com/mod/2310) | 3.0.6 | 2차 | 게임+서버 | WTT 계열 모드가 커스텀 아이템·퀘스트·의류를 추가할 때 쓰는 서버/클라이언트 공용 라이브러리. |

## 인터페이스·화면 편의 (14개)

메뉴와 레이드 중 화면을 손보는 것들. 서로 같은 화면을 건드리므로 문제가 생기면 이 묶음부터 의심한다.

| 모드 | 버전 | 단계 | 위치 | 설명 |
| --- | --- | --- | --- | --- |
| [UI Fixes](https://sp-mod.com/mod/1342) | 6.0.4 | 1차 | 게임+서버 | 메뉴 전반의 품질 개선 묶음(검색·정렬·툴팁·단축 동작 다수). 다른 UI 모드의 토대가 되는 경우가 많다. |
| [Discarder](https://sp-mod.com/mod/1745) | 5.0.0 | 5차 | 게임 | UI Fixes 기반으로 아이템 여러 개를 골라 한 번에 버린다. |
| [Task List Fixes](https://sp-mod.com/mod/824) | 1.8.0 | 5차 | 게임 | 작업(퀘스트) 목록의 정렬 오류를 고치고 목록 성능을 개선한다. |
| [Hj's Task Search](https://sp-mod.com/mod/2909) | 2.0.2 | 5차 | 게임 | 타르코프 1.0 스타일의 퀘스트 검색창을 작업 탭에 넣는다. |
| [Quest Tracker](https://sp-mod.com/mod/1140) | 1.7.0 | 1차 | 게임 | 레이드 중 퀘스트 진행을 토글 가능한 오버레이로 띄운다. |
| [Pinned Quest HUD](https://sp-mod.com/mod/3155) | 1.0.1 | 5차 | 게임 | 작업 탭에서 별표로 고정한 퀘스트만 레이드 중 HUD에 간략히 표시한다. Quest Tracker보다 가볍다. |
| [Pre-Raid Checklist](https://sp-mod.com/mod/3160) | 1.0.0 | 5차 | 게임 | 출격 직전 치료약·탄약·방탄·헬멧 누락을 경고한다. |
| [Raid Menu Cleanup Fix](https://sp-mod.com/mod/2733) | 2.0.0 | 5차 | 게임 | 레이드 중 ESC 메뉴를 열 때 생기는 짧은 멈춤을 없앤다. |
| [Notification Filter](https://sp-mod.com/mod/2729) | 2.0.0 | 5차 | 게임 | 레이드 중 뜨는 알림 팝업을 종류별로 끄거나 남긴다. |
| [Exfil Flare Notification](https://sp-mod.com/mod/1249) | 1.0.6 | 5차 | 게임 | 탈출용 조명탄이 제대로 사용됐는지 알림으로 알려 준다. |
| [Armor Class Icon](https://sp-mod.com/mod/2934) | 1.1.1 | 1차 | 게임 | 방탄복·헬멧·방탄조끼·바이저·마스크에 방탄 등급 아이콘을 띄운다. |
| [Tag Over Name](https://sp-mod.com/mod/3079) | 1.0.2 | 5차 | 게임 | 아이템에 태그를 달면 원래 이름을 숨기고 태그만 보여 준다. |
| [Amands' Sense - Updated](https://sp-mod.com/mod/2521) | 3.1.0 | 1차 | 게임 | 아이템·컨테이너·시체·탈출구에 상호작용 아이콘을 띄워 레이드 중 눈에 잘 띄게 한다. |
| [Dynamic Maps](https://sp-mod.com/mod/1431) | 1.2.1 | 1차 | 게임+게임 데이터+서버 | 게임 기본 지도 화면을 실제로 쓸 수 있는 동적 지도(내 위치·탈출구 표시)로 바꾼다. |

## 스태시·인벤토리 (16개)

보관과 정리. 스태시 압박을 줄이는 쪽이라 난이도에도 영향이 있다.

| 모드 | 버전 | 단계 | 위치 | 설명 |
| --- | --- | --- | --- | --- |
| [AutoDeposit](https://sp-mod.com/mod/1469) | 6.0.0 | 5차 | 게임 | 컨테이너에 같은 종류 아이템을 한 번에 밀어 넣는다(테라리아식 퀵스택). |
| [Advanced Stash Sorting](https://sp-mod.com/mod/2924) | 1.0.6 | 5차 | 게임 | 기본 정렬에 규칙과 옵션을 추가한다. 정렬 기준을 직접 정할 수 있다. |
| [Stash Filter](https://sp-mod.com/mod/2907) | 1.0.0 | 5차 | 게임 | 스태시 검색창에 무기·방탄·탄창·탄약·의료·부착물 필터 탭을 붙인다. |
| [Linked Search In Stash](https://sp-mod.com/mod/2731) | 2.0.1 | 5차 | 게임 | 컨텍스트 메뉴에서 내 스태시 안을 연결 검색한다. |
| [Search Open Containers](https://sp-mod.com/mod/934) | 1.5.0 | 1차 | 게임 | 열어 둔 컨테이너를 닫지 않고 바로 검색한다. |
| [Quick Move To Containers](https://sp-mod.com/mod/1341) | 1.5.0 | 1차 | 게임 | 인벤토리·스태시와 열린 컨테이너 사이로 아이템을 빠르게 옮긴다. |
| [Loot Instant Search](https://sp-mod.com/mod/2069) | 1.0.5 | 5차 | 게임 | 컨테이너와 시체의 전리품을 즉시 검색 상태로 만든다. |
| [Auto Corpse Search](https://sp-mod.com/mod/2730) | 2.2.0 | 5차 | 게임 | 시체 전리품 창을 열면 장비 슬롯 검색이 자동으로 시작된다. |
| [LetMeRightClick](https://sp-mod.com/mod/2405) | 1.1.1 | 5차 | 게임 | 검색 중에도 우클릭 메뉴를 쓸 수 있게 한다. |
| [Use Items Anywhere](https://sp-mod.com/mod/2386) | 2.1.6 | 5차 | 게임 | 인벤토리 어디에 있든 아이템을 사용하고 단축키로 지정한다. |
| [Equip From Weapon Rack](https://sp-mod.com/mod/1136) | 1.6.0 | 5차 | 게임 | 은신처 무기 거치대에서 무기를 바로 장착한다. |
| [Player Encumbrance Bar](https://sp-mod.com/mod/1374) | 1.3.0 | 1차 | 게임 | 인벤토리에 플레이어 총 무게를 막대로 표시한다. |
| [AmmoBox1x1](https://sp-mod.com/mod/2402) | 2.0.0 | 2차 | 서버 | 2x1 탄약 상자를 전부 1x1로 줄인다. |
| [BarterItemsStacks](https://sp-mod.com/mod/2480) | 1.4.3 | 5차 | 게임+서버 | 교환용(바터) 아이템의 스택 크기를 키운다. |
| [Caliber Split Storage Solutions](https://sp-mod.com/mod/2990) | 1.0.1 | 5차 | 서버 | **결속** 구경별로 나뉜 탄약 보관 케이스를 추가한다(아이템 91개 생성). `db/99_Ids/ids_*.json`을 지우면 프로필이 깨진다. |
| [Cobra's Ammo Stack](https://sp-mod.com/mod/2840) | 1.1.0 | 5차 | 서버 | **결속** 구경별 탄약 스택 크기를 설정한다. 기본값 500발은 바닐라보다 훨씬 커서 조정 여지가 있다(`config.json`). |

## 아이템 정보·가격 표시 (9개)

판단에 필요한 수치를 아이템 위에 올려 주는 것들. 표기가 겹치면 서로 가릴 수 있다.

| 모드 | 버전 | 단계 | 위치 | 설명 |
| --- | --- | --- | --- | --- |
| [ODT's Item Info - SPT 4.0](https://sp-mod.com/mod/2430) | 3.0.0 | 1차 | 서버 | **결속** 아이템 설명에 용도·퀘스트·은신처 수요 등 정보를 대량으로 덧붙인다. |
| [Show Me The Money (Item Pricing) (4.1.x)](https://sp-mod.com/mod/3011) | 3.0.2 | 1차 | 게임+서버 | 아이템 툴팁에 가격 정보를 표시한다. |
| [Better Keys NG](https://sp-mod.com/mod/1888) | 1.8.0 | 1차 | 서버 | 열쇠마다 퀘스트 필요 여부·문 뒤의 내용·사용 횟수를 알려 준다. |
| [Ammo Stats](https://sp-mod.com/mod/167) | 4.1.0 | 1차 | 서버 | 탄약 이름과 설명에 관통력·데미지 등 수치를 넣는다(로케일 교체 방식). |
| [Caliber Under Ammo Name](https://sp-mod.com/mod/2586) | 1.2.0 | 1차 | 게임 | 탄약과 탄약 상자의 짧은 이름 아래에 구경을 표기한다. |
| [Compatibility Highlighter](https://sp-mod.com/mod/2859) | 1.3.0 | 1차 | 게임 | 호환되는 탄창·탄약·부착물을 강조 표시한다. |
| [Chamber Ammo Info](https://sp-mod.com/mod/2553) | 1.1.0 | 1차 | 게임 | 약실 점검 시 장전된 탄종을 보여 준다. |
| [BALL - Better Ammo Loading List](https://sp-mod.com/mod/2221) | 1.1.1 | 1차 | 게임 | 탄약 선택 목록을 읽기 쉽게 정리한다. |
| [Colored Tracers](https://sp-mod.com/mod/2037) | 3.0.1 | 1차 | 서버 | 탄의 관통력에 따라 예광탄 색을 다르게 한다. |

## 총기 개조·빌드 화면 (7개)

| 모드 | 버전 | 단계 | 위치 | 설명 |
| --- | --- | --- | --- | --- |
| [Trader Modding And Improved Weapon Building](https://sp-mod.com/mod/1283) | 2.1.2 | 1차 | 게임+서버 | **결속** 개조 화면 개편 — 현재 상인 레벨/보유 부품만 보기, 상인·플레아 가격 표시 등. |
| [Modding Stats Helper - by wara](https://sp-mod.com/mod/1300) | 1.2.0 | 1차 | 게임 | **결속** 개조 화면에서 부품에 마우스를 올리면 변화 수치를 요약해 보여 준다. |
| [Weapon Builder Search](https://sp-mod.com/mod/2743) | 2.0.0 | 1차 | 게임 | 무기 빌더·개조 화면의 부착물 목록에 실시간 검색창을 넣는다. |
| [Weapon Customizer](https://sp-mod.com/mod/1950) | 4.0.0 | 2차 | 게임+서버 | 레일과 부착물 위치를 미세 조정해 외형을 바꾼다. |
| [Flexible Builds](https://sp-mod.com/mod/2646) | 1.2.2 | 5차 | 게임+서버 | 무기 프리셋과 빌드 저장·적용을 개선한다. |
| [Advanced Modding Lights](https://sp-mod.com/mod/3081) | 2.0.0 | 5차 | 게임 | 개조·빌드 화면 조명을 밝게 해 부품이 잘 보이게 한다. |
| [Transparent Sights](https://sp-mod.com/mod/2774) | 0.2.3 | 5차 | 게임 | 조준경을 반투명하게 만들어 양쪽 눈을 뜬 채 조준하는 느낌을 준다. 설정은 F12 메뉴, 조준경별 전환은 인벤토리 아이콘 더블클릭(`TRANSP: ON` / `OFF` / `ON + MOUNT`). 첫 실행 때 `config.json` 없다는 Error가 뜨는 것은 정상 — 조준경별 설정을 처음 바꿀 때 모드가 직접 만든다. |

## 조준·사격·전투 (9개)

stage5-B의 조준경 관련이 여기 모여 있다. PiP Disabler는 성능, 나머지는 조작 편의.

| 모드 | 버전 | 단계 | 위치 | 설명 |
| --- | --- | --- | --- | --- |
| [Picture in Picture Disabler](https://sp-mod.com/mod/2667) | 2.1.0 | 5차 | 게임 | 조준경의 PiP(화면 속 화면) 렌더링을 끈다. FPS 확보 목적. |
| [Scope Rangefinder](https://sp-mod.com/mod/2735) | 3.5.0 | 5차 | 게임 | 조준경 안에 거리 측정기와 미터 단위 자동 제로, 탄도·탄착 예측을 띄운다. |
| [Auto Ranging](https://sp-mod.com/mod/2786) | 2.0.0 | 5차 | 게임 | 조준한 지점의 거리에 맞춰 가늠자를 자동으로 제로한다. |
| [NVG Sight Dimmer](https://sp-mod.com/mod/2789) | 2.1.0 | 5차 | 게임 | 야투경을 켜면 조준경·가늠자 밝기를 자동으로 낮춘다. |
| [Better Thermal & Night Vision](https://sp-mod.com/mod/2531) | 2.1.1 | 5차 | 게임 | 열상·야투경의 화질과 성능(노이즈·블러·스캔라인)을 조정한다. |
| [Easy Mounting](https://sp-mod.com/mod/2790) | 2.0.1 | 5차 | 게임+서버 | 무기를 엄폐물에 거치(마운트)하는 조작을 쉽게 만든다. |
| [Default Fire Mode Fixer](https://sp-mod.com/mod/1416) | 2.1.0 | 5차 | 서버 | 무기 기본 발사모드를 단발이 아닌 연발로 맞춘다(샷건은 2연발). |
| [Fast Grenade Throw](https://sp-mod.com/mod/2783) | 2.0.0 | 5차 | 게임 | 예전 방식의 즉시 수류탄 투척을 되살린다(오버핸드·언더핸드 별도 키). |
| [Tripwire Improvement](https://sp-mod.com/mod/2618) | 1.1.0 | 5차 | 게임 | 와이어 트랩 키트를 무한으로 쓰고 폭발 카운트다운·설치 범위를 조정한다. |

## 장전·탄약 취급 (6개)

| 모드 | 버전 | 단계 | 위치 | 설명 |
| --- | --- | --- | --- | --- |
| [Mag Retention Reload](https://sp-mod.com/mod/2589) | 1.3.0 | 2차 | 게임 | 재장전할 때 빈 탄창을 바닥에 버리지 않고 유지한다. |
| [Unload All Magazines](https://sp-mod.com/mod/2739) | 2.2.1 | 2차 | 게임 | 여러 탄창을 한 번에 비운다. |
| [Continuous Load Ammo](https://sp-mod.com/mod/2112) | 1.2.0 | 1차 | 게임 | 인벤토리 밖에서도 탄을 연속으로 장전·해제한다. |
| [Magazine Check Interrupt](https://sp-mod.com/mod/2643) | 1.1.1 | 5차 | 게임 | 탄창 확인 동작 중에 바로 재장전으로 이어진다. |
| [Direct Reload & Painless Meds](https://sp-mod.com/mod/3074) | 1.3.2 | 5차 | 게임+서버 | 느슨한 탄약에서 바로 재장전한다. 의료품 사용시간 x0.5·약물 지속 x2 같은 수치 변경도 기본 포함되므로 SVM 설정과 겹치는지 볼 것(`config.json`). |
| [Low Ammo Cue](https://sp-mod.com/mod/3046) | 1.0.0 | 5차 | 게임 | 잔탄이 적어지면 금속음이 점점 커지며 알려 준다. |

## 상인·플레아 화면 (10개)

상인 목록이 길어진 상태를 받쳐 주는 쪽. 커스텀 상인 본체는 아래 묶음.

| 모드 | 버전 | 단계 | 위치 | 설명 |
| --- | --- | --- | --- | --- |
| [Trader Search](https://sp-mod.com/mod/2824) | 2.1.0 | 1차 | 게임 | 상인 창과 플레아 등록 창에 검색창을 넣는다. |
| [QuickSellFlea](https://sp-mod.com/mod/2407) | 1.4.5 | 2차 | 게임 | 플레아에 매물을 올리지 않고 바로 판매한다. |
| [Kaeno-TraderScrolling](https://sp-mod.com/mod/1089) | 4.1.0 | 4차 | 게임 | 상단 상인 줄을 좌우로 스크롤한다. 상인이 많아지면 필수. |
| [TraderSpace](https://sp-mod.com/mod/2694) | 2.0.0 | 4차 | 게임 | 상인 줄을 가리던 플레이어 패널을 치워 자리를 넓힌다. |
| [Trader Namer](https://sp-mod.com/mod/1527) | 4.1.0 | 5차 | 서버 | 상인의 이름·아바타·소개를 바꾼다. `avatars` 폴더가 없으면 서버가 죽는다(설치 시 폴더 생성 필요). |
| [Trader Level Requirements Editor](https://sp-mod.com/mod/3005) | 1.1.0 | 5차 | 서버 | SPT 기본 상인들의 레벨 요구치를 편집한다. |
| [SPT Trader Restock Refresh](https://sp-mod.com/mod/3098) | 1.0.0 | 5차 | 게임+서버 | 보고 있는 상인 화면을 재입고 시점에 자동으로 갱신한다. |
| [Ultimate Trader Assort Editor](https://sp-mod.com/mod/2834) | 2.1.1 | 5차 | 게임+서버 | 상인 판매 목록(어소트)을 세부까지 편집하는 도구. |
| [Hide Locked Flea](https://sp-mod.com/mod/3152) | 1.0.0 | 5차 | 게임+서버 | 플레아에서 잠긴(구매 불가) 매물을 숨기는 필터를 넣는다. |
| [Refined Flea Offer List](https://sp-mod.com/mod/2623) | 1.2.0 | 5차 | 게임 | 플레아 카테고리 목록에서 아이템별 최저가만 보여 준다. |

## 커스텀 상인·콘텐츠 (12개)

전부 프로필 결속. 넣고 플레이한 뒤 제거하면 프로필이 깨질 수 있다.

| 모드 | 버전 | 단계 | 위치 | 설명 |
| --- | --- | --- | --- | --- |
| [WTT- Armory](https://sp-mod.com/mod/2246) | 3.0.0 | 2차 | 게임+서버 | **결속** WTT 공식 무기 팩. 총기·부품을 대량 추가한다(설치 파일 954개). |
| [WTT- Artem](https://sp-mod.com/mod/1023) | 3.0.2 | 2차 | 서버 | **결속** 커스텀 상인 Artem. 의류·장비·거래를 함께 추가한다. |
| [Lotus](https://sp-mod.com/mod/1232) | 1.8.1 | 3차 | 서버 | **결속** 커스텀 상인 Lotus — 퀘스트 340개 이상, 판매·교환 품목 900개 이상, 특수 키카드. |
| [Scorpion](https://sp-mod.com/mod/1348) | 1.1.1 | 3차 | 서버 | **결속** 커스텀 상인 Scorpion — 시작부터 가능한 퀘스트 122개. |
| [Ref - SPT Friendly Quests](https://sp-mod.com/mod/1538) | 2.1.0 | 3차 | 서버 | **결속** 아레나 전용이던 Ref 퀘스트를 SPT용으로 변환해 추가한다. |
| [HarryHideout](https://sp-mod.com/mod/1303) | 3.1.2 | 3차 | 서버 | **결속** 은신처 건설·업그레이드 재료를 파는 상인 Harry. SVM에서 건설 요구사항을 없애면 팔 품목이 사라져 오류가 난다(이번 작업에서 겪음). |
| [Gunsmith Task Trader](https://sp-mod.com/mod/3124) | 1.0.2 | 5차 | 서버 | **결속** 건스미스 과제마다 필요한 총기를 파는 상인. 과제 진행 보조용. |
| [Gunwright](https://sp-mod.com/mod/3128) | 1.0.2 | 5차 | 서버 | **결속** 건스미스 프리셋과 직접 만든 빌드를 판매·구매하는 상인. 가격 설정 가능. |
| [CaseTrader — Buy any case](https://sp-mod.com/mod/3024) | 1.0.1 | 5차 | 서버 | **결속** 각종 케이스(보관함·열쇠고리·지갑·의료·탄약 상자)를 파는 상인 Case. |
| [PresetTrader](https://sp-mod.com/mod/2966) | 1.2.0 | 5차 | 서버 | **결속** 저장해 둔 빌드와 장비 세트를 비싼 값에 파는 상인. |
| [Ledger](https://sp-mod.com/mod/3085) | 1.1.0 | 5차 | 서버 | **결속** 도그태그를 모으는 상인 Ledger. 퀘스트 40개 이상의 이야기가 붙는다. |
| [Amonya - Ammo Loving Trader & Quester](https://sp-mod.com/mod/2419) | 2.2.0 | 5차 | 서버 | **결속** 탄약 전문 상인 Amonya. 퀘스트 보상으로 상급 탄을 준다. `db/99_Ids/ids_*.json` 보존 필요. |

## 서버 조정·도구·치트 (8개)

게임 규칙을 직접 바꾸는 쪽. 치트성 기능은 SVM으로 되는지 먼저 확인하는 것이 이 저장소의 방침.

| 모드 | 버전 | 단계 | 위치 | 설명 |
| --- | --- | --- | --- | --- |
| [Server Value Modifier \[SVM\]](https://sp-mod.com/mod/236) | 2.2.3 | 2차 | 서버+루트 | **결속** 서버 설정 종합 도구(SVM). FIR·구매 제한·무게·레이드 시간·경험치 등 수백 개 항목을 `Greed.exe` UI로 조정한다. 프리셋 `Noname` 사용 중. |
| [give-ui](https://sp-mod.com/mod/1617) | 5.0.1 | 3차 | 서버 | give·trader·profile 명령을 쓰기 쉬운 웹 UI로 제공한다(아이템 지급·상인 레벨·프로필 편집). |
| [Skipper](https://sp-mod.com/mod/1343) | 1.1.5 | 3차 | 게임 | 퀘스트를 건너뛰는 작은 클라이언트 도구. |
| [Open Sesame](https://sp-mod.com/mod/1184) | 3.0.1 | 3차 | 게임 | (치트 표시) 잠긴 문·금고를 열쇠 없이 연다. 원래 개발용. |
| [Freecam](https://sp-mod.com/mod/164) | 1.4.8 | 5차 | 게임 | 자유 시점 카메라. 스크린샷·확인용으로 쓴다. |
| [Set Speed - Set Player Speed with Hotkeys](https://sp-mod.com/mod/994) | 1.5.0 | 5차 | 게임 | 단축키로 걷기 속도 프리셋을 바꾼다. |
| [Low Budget Bullet Time NG](https://sp-mod.com/mod/1988) | 2.3.0 | 5차 | 게임 | 시간을 느리게 만드는 불릿 타임(제한 있음). |
| [Cobra's SPT Explorer](https://sp-mod.com/mod/3009) | 1.0.0 | 5차 | 게임+서버+루트 | (치트 표시) 읽기 전용 SPT 데이터베이스 탐색기 + 레이드 중 퀘스트·탈출구 도움. 서버를 켠 뒤 `CobraSPTExplorerUI\CobrasSPTExplorer.UI.exe`를 따로 실행한다. |

## 프로필 결속 모드 (18개)

제거하려면 프로필 백업이 먼저다. 되돌릴 때는 `doc/install-records/installed-<Tag>.txt`의 경로만 지운다.

- [Amonya - Ammo Loving Trader & Quester](https://sp-mod.com/mod/2419) (2.2.0)
- [Caliber Split Storage Solutions](https://sp-mod.com/mod/2990) (1.0.1)
- [CaseTrader — Buy any case](https://sp-mod.com/mod/3024) (1.0.1)
- [Cobra's Ammo Stack](https://sp-mod.com/mod/2840) (1.1.0)
- [Gunsmith Task Trader](https://sp-mod.com/mod/3124) (1.0.2)
- [Gunwright](https://sp-mod.com/mod/3128) (1.0.2)
- [HarryHideout](https://sp-mod.com/mod/1303) (3.1.2)
- [Ledger](https://sp-mod.com/mod/3085) (1.1.0)
- [Lotus](https://sp-mod.com/mod/1232) (1.8.1)
- [Modding Stats Helper - by wara](https://sp-mod.com/mod/1300) (1.2.0)
- [ODT's Item Info - SPT 4.0](https://sp-mod.com/mod/2430) (3.0.0)
- [PresetTrader](https://sp-mod.com/mod/2966) (1.2.0)
- [Ref - SPT Friendly Quests](https://sp-mod.com/mod/1538) (2.1.0)
- [Scorpion](https://sp-mod.com/mod/1348) (1.1.1)
- [Server Value Modifier [SVM]](https://sp-mod.com/mod/236) (2.2.3)
- [Trader Modding And Improved Weapon Building](https://sp-mod.com/mod/1283) (2.1.2)
- [WTT- Armory](https://sp-mod.com/mod/2246) (3.0.0)
- [WTT- Artem](https://sp-mod.com/mod/1023) (3.0.2)

## 지우면 안 되는 파일

| 파일 | 모드 | 이유 |
| --- | --- | --- |
| `SPT_Runtime\user\mods\CaliberSplitStorageSolutions\db\99_Ids\ids_*.json` | Caliber Split Storage Solutions | 생성한 아이템 91개의 ID 목록. 지우면 프로필이 깨진다 |
| `SPT_Runtime\user\mods\Amonya\db\99_Ids\ids_*.json` | Amonya | 같은 이유 |
| `SPT_Runtime\user\mods\TraderNamer\avatars\` (빈 폴더) | Trader Namer | 폴더가 없으면 서버가 시작하지 못한다 |
| `SPT_Runtime\user\mods\[SVM] Server Value Modifier\Presets\Noname.json` | SVM | 사용 중인 프리셋. 사본은 `backup/svm-presets/` |

## 설정을 손볼 만한 곳

| 모드 | 파일 | 메모 |
| --- | --- | --- |
| Cobra's Ammo Stack | `...\CobrasAmmoStack\config.json` | 기본 스택 500발(구경별). stage5-E 시점 적용 대상 탄약 427종 |
| Direct Reload & Painless Meds | `...\RZDirectReload\config.json` | 의료 사용시간 x0.5, 약물·스팀 지속 x2가 기본값 |
| EasyMounting | `...\EasyMounting\Config\config.json` | 프리셋 `AnySurface` 적용 상태 |
| Trader Namer | `...\TraderNamer\config.json` | 이름·소개 변경. 아바타는 `avatars\<상인이름>.jpg` |
| SVM | `Greed.exe` | 치트성 기능은 여기서 먼저 찾는다 |
| Transparent Sights | F12 메뉴 + 조준경 아이콘 더블클릭 | 파일로 받을 설정이 없다. `plugins\7Bpencil.TransparentSights\config.json`은 조준경별 전환을 저장할 때 모드가 직접 만든다(없으면 기본값) |
