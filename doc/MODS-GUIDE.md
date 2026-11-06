# 설치된 모드 설명

> 2026-10-25 기준 Forge 모드 **145개** + Forge 밖 **3개**(GitHub 2·Nexus 1) + 직접 만든 것 **2개**. 무엇을 하는 모드인지 기능별로 묶어 적은 문서.
> 버전·설치 파일 수·설치 기록은 [INSTALLED-MODS.md](INSTALLED-MODS.md)(자동 생성), 작업 경과는 [WORKLOG.md](WORKLOG.md).

- 대상: `E:\Spt` (SPT 4.1.6, EFT 0.16.9.40743)
- **위치** — `게임`은 `BepInEx\plugins`, `서버`는 `SPT_Runtime\user\mods`, `루트`는 `E:\Spt` 바로 아래.
- **결속** = Forge "프로필 결속" 표시. 설치하고 플레이한 뒤 제거하면 프로필이 깨질 수 있다.
- 설명은 모드 소개와 이번 설치에서 실제로 확인한 동작을 합쳐 적었다.

## 기반 라이브러리 (3개)

다른 모드가 요구하는 것. 단독으로는 눈에 보이는 변화가 없다.

| 모드 | 버전 | 단계 | 위치 | 설명 |
| --- | --- | --- | --- | --- |
| [Color Converter API](https://sp-mod.com/mod/1090) | 1.2.0 | 1차 | 게임 | 아이템 배경색에 16진수 색상 코드를 쓸 수 있게 하는 클라이언트 API. 색을 쓰는 모드들의 선행 조건. |
| [WTT - CommonLib](https://sp-mod.com/mod/2310) | 3.0.6 | 2차 | 게임+서버 | WTT 계열 모드가 커스텀 아이템·퀘스트·의류를 추가할 때 쓰는 서버/클라이언트 공용 라이브러리. |
| [UnityToolkit](https://sp-mod.com/mod/1426) | 2.0.2 | 6차 | 게임 | 클라이언트 모드용 공용 라이브러리(UniTask·ZLinq 등). `BepInEx\patchers`에 프리패처도 깔려 게임 시작 중 어셈블리를 교체한다. Forge 표기는 4.1.5지만 4.1.6에서 정상 동작하는 것을 게임 로그로 확인했다. Field Attachments·Scrollable Attachments의 선행 조건. |

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

## 스태시·인벤토리 (18개)

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
| [Slimmer Magazines](https://sp-mod.com/mod/2373) | 2.0.0 | 7차 | 서버 | **결속** 2x2 탄창을 1x2로 줄인다. 설치 때 탄창 44개가 바뀌었다. 같은 일을 하는 Shorter Magazines(3186)는 **같이 넣지 않는다**. |
| [BiC - Bind in Container](https://sp-mod.com/mod/3190) | 1.0.1 | 7차 | 게임 | 보안 컨테이너 안의 물건에도 단축키를 걸 수 있게 한다. |

## 아이템 정보·가격 표시 (12개)

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
| [Armor Expert](https://sp-mod.com/mod/3106) | 3.1.0 | 7차 | 게임 | 방어 장비·헤드셋의 숨은 수치를 아이템 설명에 표시한다. ODT Item Info와 같은 자리를 쓰므로 설명이 길어질 수 있다. |
| [Munitions Expert - Reloaded](https://sp-mod.com/mod/3006) | 3.1.0 | 7차 | 게임 | 탄약·폭발물의 숨은 수치를 표시한다. Armor Expert와 한 쌍. |
| [ArmorInfoPlus](https://sp-mod.com/mod/3049) | 1.0.0 | 7차 | 게임 | 모든 아이템의 24자 템플릿 ID를 띄운다. 조사·설정용이고 평소에는 꺼 두어도 된다. |

## 총기 개조·빌드 화면 (11개)

| 모드 | 버전 | 단계 | 위치 | 설명 |
| --- | --- | --- | --- | --- |
| [Trader Modding And Improved Weapon Building](https://sp-mod.com/mod/1283) | 2.1.2 | 1차 | 게임+서버 | **결속** 개조 화면 개편 — 현재 상인 레벨/보유 부품만 보기, 상인·플레아 가격 표시 등. |
| [Modding Stats Helper - by wara](https://sp-mod.com/mod/1300) | 1.2.0 | 1차 | 게임 | **결속** 개조 화면에서 부품에 마우스를 올리면 변화 수치를 요약해 보여 준다. |
| [Weapon Builder Search](https://sp-mod.com/mod/2743) | 2.0.0 | 1차 | 게임 | 무기 빌더·개조 화면의 부착물 목록에 실시간 검색창을 넣는다. |
| [Weapon Customizer](https://sp-mod.com/mod/1950) | 4.0.0 | 2차 | 게임+서버 | 레일과 부착물 위치를 미세 조정해 외형을 바꾼다. |
| [Flexible Builds](https://sp-mod.com/mod/2646) | 1.2.2 | 5차 | 게임+서버 | 무기 프리셋과 빌드 저장·적용을 개선한다. |
| [Advanced Modding Lights](https://sp-mod.com/mod/3081) | 2.0.0 | 5차 | 게임 | 개조·빌드 화면 조명을 밝게 해 부품이 잘 보이게 한다. |
| [Transparent Sights](https://sp-mod.com/mod/2774) | 0.2.3 | 5차 | 게임 | 조준경을 반투명하게 만들어 양쪽 눈을 뜬 채 조준하는 느낌을 준다. 설정은 F12 메뉴, 조준경별 전환은 인벤토리 아이콘 더블클릭(`TRANSP: ON` / `OFF` / `ON + MOUNT`). 첫 실행 때 `config.json` 없다는 Error가 뜨는 것은 정상 — 조준경별 설정을 처음 바꿀 때 모드가 직접 만든다. |
| [Tylevo's Field Attachments](https://sp-mod.com/mod/3125) | 1.2.0 | 6차 | 게임 | **결속** 인벤토리를 열지 않고 레이드 중에 부품을 바꾼다. Left Alt를 누르면 무기 주변에 카드가 뜨고, 들고 있는 부품 중에서 광학·소음기·손잡이를 고른다. 개머리판·핸드가드·권총 손잡이까지 바꾸려면 Leatherman 멀티툴을 지녀야 한다. UnityToolkit 2.0.2 이상 필요. |
| [Scrollable Attachments](https://sp-mod.com/mod/2677) | 2.0.0 | 6차 | 게임 | 개조 화면의 부착물 목록을 휠로 넘긴다. UnityToolkit 필요. |
| [BETTER ATTACHMENTS](https://sp-mod.com/mod/1939) | 4.1.5 | 7차 | 게임+서버 | **결속** 모든 부착물의 인간공학·반동을 좋게 만든다. 설치 때 **부착물 1,092개**가 바뀌었다. DLL 표기는 2.0.0이다. |
| [All In Weapon: Unslotted](https://sp-mod.com/mod/2576) | 1.2.0 | 7차 | 서버 | 슬롯·호환 제한을 없애 아무 부품·탄창·탄약이나 끼울 수 있다. 봇 무기 생성은 바닐라 그대로. 설치된 Compatibility Highlighter의 강조가 "전부 호환"처럼 보일 수 있다. **제거 영향 실측**은 [specs/2026-11-04-allinweapon-removal-survey.md](specs/2026-11-04-allinweapon-removal-survey.md) — 프로필 파일은 안 바뀌고, 이 모드만의 영향은 "부품 위의 부품" 23건이다(헬멧·방어구는 다른 모드 소관). |

## 조준·사격·전투 (19개)

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
| [ECOTI - Enhanced Clip-On Thermal Imager](https://sp-mod.com/mod/2930) | 3.4.2 | 6차 | 게임+서버 | **결속** 야투경 앞에 물리는 클립온 열상(AN/PAS-29B). 열상이 화면을 덮지 않고 야투경 튜브 원 **안에만** 겹친다. Peacekeeper가 판다(본체 LL4 $2,000, 열상 라이저·스코프링 LL3 $1,750, 각 한도 3). 서버가 처음 뜰 때 설치된 야투경(WTT-Artem 등)에 맞춘 호환 파일 15개를 스스로 만든다. 3.4.2가 **PiP Disabler와의 열상 표시 문제를 고친 버전**이라 그 아래로 내리지 않는다. Better Thermal과 같은 화면을 건드리므로 이상하면 그쪽을 먼저 끈다. |
| [BrightLasers](https://sp-mod.com/mod/1358) | 3.0.0 | 6차 | 게임+서버 | 레이저 밝기·굵기·색을 F12에서 조절한다. 3.0.0부터 번들 모드가 아니어서 WTT Content Backport와 충돌하지 않는다. |
| [No Barrel Shadow](https://sp-mod.com/mod/2780) | 2.0.1 | 6차 | 게임 | **결속** 라이트·전술 장비가 총구에 드리우던 그림자를 없애고, 조명별 밝기·세기를 F12에서 조절한다. 결속 표시가 있지만 클라이언트 전용이다. |
| [Canted Aiming](https://sp-mod.com/mod/2415) | 2.0.0 | 7차 | 게임+서버 | **결속** 백업 사이트 없이도 총을 기울여 조준한다. |
| [Magic Optic Mount](https://sp-mod.com/mod/3101) | 0.1.0 | 7차 | 게임+서버 | **결속** 광학 조준경과 열상·야투를 한 총에 같이 단다. 설치된 ECOTI와 성격이 겹친다. |
| [Realistic Suppressors](https://sp-mod.com/mod/3171) | 1.0.0 | 7차 | 서버 | **결속** 소음기 성능을 크게 올린다. 설치 때 **소음기 103개**에 적용됐다. |
| [Full Automatic Guns](https://sp-mod.com/mod/1972) | 4.1.5 | 7차 | 서버 | **결속** 일부 SMG·소총·카빈을 자동 사격으로 바꾼다(무기 12종). DLL 표기는 1.0.0이다. |
| [ShrapnelFix](https://sp-mod.com/mod/3118) | 1.0.0 | 7차 | 게임 | 수류탄 파편이 바닥이 아니라 몸 높이로 날아가게 고친다. |
| [Airburst](https://sp-mod.com/mod/2948) | 1.1.2 | 7차 | 게임+서버 | **결속** 엄폐물 위에서 터지는 40mm 공중폭발탄을 추가한다. |
| [Quick Aim](https://sp-mod.com/mod/2985) | 1.0.0 | 7차 | 게임 | 앉기·엎드리기·거치 상태에서 조준을 더 오래 유지한다. |

## 장전·탄약 취급 (8개)

| 모드 | 버전 | 단계 | 위치 | 설명 |
| --- | --- | --- | --- | --- |
| [Mag Retention Reload](https://sp-mod.com/mod/2589) | 1.3.0 | 2차 | 게임 | 재장전할 때 빈 탄창을 바닥에 버리지 않고 유지한다. **꺼 둬야 한다 — 켜지 말 것.** Direct Reload와 충돌해 탄창이 사라진다(2026-10-25 확인, 10-26 재발). 아래 "알려진 충돌" 참고. 같은 기능은 UI Fixes가 맡는다. |
| [Unload All Magazines](https://sp-mod.com/mod/2739) | 2.2.1 | 2차 | 게임 | 여러 탄창을 한 번에 비운다. |
| [Continuous Load Ammo](https://sp-mod.com/mod/2112) | 1.2.0 | 1차 | 게임 | 인벤토리 밖에서도 탄을 연속으로 장전·해제한다. |
| [Magazine Check Interrupt](https://sp-mod.com/mod/2643) | 1.1.1 | 5차 | 게임 | 탄창 확인 동작 중에 바로 재장전으로 이어진다. |
| [Direct Reload & Painless Meds](https://sp-mod.com/mod/3074) | 1.3.2 | 5차 | 게임+서버 | 느슨한 탄약에서 바로 재장전한다. 의료품 사용시간 x0.5·약물 지속 x2 같은 수치 변경도 기본 포함되므로 SVM 설정과 겹치는지 볼 것(`config.json`). |
| [Low Ammo Cue](https://sp-mod.com/mod/3046) | 1.0.0 | 5차 | 게임 | 잔탄이 적어지면 금속음이 점점 커지며 알려 준다. |
| [Manimal's Quick Cancel](https://sp-mod.com/mod/2969) | 2.0.0 | 7차 | 게임 | 소비 동작(치료·음식)을 재장전 키로 즉시 끊는다. |
| [AmmoGen](https://sp-mod.com/mod/2777) | 2.4.0 | 7차 | 서버 | **결속** 커스텀 탄약·탄약 상자를 JSON으로 만드는 프레임워크. 팩을 넣기 전에는 `No ammo packs found`만 찍고 아무것도 하지 않는다. |

## 방어구·장비 (12개)

7차에서 처음 다룬 분야. 장비를 어디에 어떻게 걸칠 수 있는지와 방어판·내구도를 건드린다.

| 모드 | 버전 | 단계 | 위치 | 설명 |
| --- | --- | --- | --- | --- |
| [Less Restricting Headwear](https://sp-mod.com/mod/922) | 4.0.0 | 7차 | 서버 | **결속** 헬멧·헤드셋·마스크·페이스실드를 함께 쓰지 못하게 막던 제한을 푼다. 설치 때 **품목 182개**에서 차단 117건·세부 충돌 4,005건이 사라졌다. |
| [Unrestricted Armor Plate](https://sp-mod.com/mod/1878) | 2.1.0 | 7차 | 서버 | 아무 방어판이나 아무 방탄 조끼·리그에 꽂을 수 있게 한다. |
| [LMSP - Let Me Switch Plates](https://sp-mod.com/mod/2814) | 1.0.1 | 7차 | 게임 | 레이드 중에 방어구를 벗지 않고 방어판만 바꾼다. |
| [Restore Armor Plates](https://sp-mod.com/mod/2973) | 1.0.2 | 7차 | 게임+서버 | **결속** 전용 키트로 방어판의 최대 내구도를 되돌린다. DLL 표기는 1.0.1. |
| [Repair Skill Extension](https://sp-mod.com/mod/2556) | 1.4.1 | 7차 | 서버 | **결속** 수리 버프가 붙을 확률을 올리고 수리 스킬을 모든 방어구에 적용한다. |
| [Black Exfil Revision](https://sp-mod.com/mod/2380) | 1.1.0 | 7차 | 서버 | **결속** Black Exfil 마스크에 페이스실드를 다시 달 수 있게 한다. |
| [Change Helmet Visor](https://sp-mod.com/mod/2554) | 3.0.0 | 7차 | 게임+서버 | 페이스실드·바이저가 시야를 가리는 마스크를 바꾼다. **지금은 모두 `NoMask`** — 타길라 용접 마스크 3종·Altyn·Rys-T·Maska 2종의 가림이 사라진다. |
| [VisorEffectManager](https://sp-mod.com/mod/2429) | 1.1.0 | 7차 | 게임 | 바이저의 시각 효과(김서림·빗물 등)를 켜고 끈다. |
| [Equipment Is Eternal](https://sp-mod.com/mod/2897) | 1.2.0 | 7차 | 서버 | **결속** 죽어도 장비를 잃지 않는다. 슬롯별로 끌 수 있고 지금은 보험·퀘스트 아이템·암밴드·보안 컨테이너가 제외돼 있다. |
| [Foldables](https://sp-mod.com/mod/2422) | 1.1.1 | 7차 | 게임+서버 | 배낭·조끼·헤드폰을 접어 보관 자리를 줄인다(배낭 46·조끼 36·헤드폰 21). |
| [Jero Backpack](https://sp-mod.com/mod/2369) | 4.1.1 | 7차 | 서버 | **결속** 배낭 격자를 키운다(배낭 36개 변경, 격자가 나뉜 10개는 제외). |
| [Holster Everything](https://sp-mod.com/mod/2650) | 2.0.0 | 7차 | 게임+서버 | **결속** 홀스터 슬롯에 넣을 수 있는 품목을 넓힌다. |

**SOMF(SitOnMyFace)는 7-A에 넣었다가 뺐다.** Less Restricting Headwear가 같은 일을 먼저 해 버려 규칙을 0건 바꾸게 됐기 때문이다. 아래 "헷갈리기 쉬운 정상 동작" 참고.

## 상인·플레아 화면 (20개)

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
| [Quick Trader Load Times](https://github.com/JoelHauser/QuickTraderLoadTimes) | 1.0.0 | 6차 | 게임+서버 | **Forge 밖**(GitHub·MIT). 상인 화면과 플레아가 뜨는 속도를 끌어올린다 — 한 프레임에 칸 하나씩 그리던 것을 프레임 예산 안에서 여러 개씩, 상인을 바꿀 때 스태시를 한 프레임에 다시 그리던 것을 여러 프레임에 나눠서. 상인이 늘어난 지금 체감이 가장 큰 쪽이다. |
| [Moe-TradeMarker](https://sp-mod.com/mod/2736) | 1.2.6 | 6차 | 게임+서버 | **결속** 상인에게서 산 물건에 배지를 붙여 전리품과 구분한다. 원하면 그 물건의 플레아 재판매를 막는다(기본 켜짐). QuickSell 연동. |
| [OpenBarters](https://sp-mod.com/mod/3194) | 1.0.1 | 6차 | 게임 | 물물교환 대금을 요구 품목 대신 **상인이 사주는 아무 아이템**으로 치른다(합산 가치가 요구치를 넘으면 성립). |
| [All Weapons Flea](https://sp-mod.com/mod/3200) | 1.0.1 | 6차 | 서버 | 상인 전용이던 총기를 플레아에서 사고팔 수 있게 한다(무기 238종, BSG 제한 33개 해제). 총기 파츠를 모으는 데 직접 도움이 된다. |
| [Ref Rework](https://sp-mod.com/mod/3014) | 1.0.0 | 6차 | 게임+서버 | 도그태그와 레가 메달을 Ref에 팔아 GP 코인으로 바꾼다. 레벨이 높은 태그일수록 비싸다. **Ledger와 도그태그 용처가 겹치므로** 게임에서 보고 한쪽을 끄는 편이 낫다. |
| [Realistic Insurance](https://sp-mod.com/mod/2949) | 0.2.1 | 6차 | 게임+서버 | 보험 회수를 고정 확률이 아니라 "누가 죽였고 그가 살아 나갔는지"로 정한다(PMC 55 / 플레이어 스캐브 45 / 스캐브 75 / 보스 40%). SVM 보험 설정과 겹치면 한쪽만 쓴다. |
| [VanillaTradersRework-Reloaded](https://www.nexusmods.com/escapefromtarkov/mods/10) | 1.0.0 | 6차 | 서버 | **Forge 밖**(Nexus·받으려면 로그인 필요). 상인 초상화를 사실적인 그림으로 교체한다 — 바닐라 9명과 Lotus·Scorpion·Artem·Couturier·Croupier 등. 초상화가 없는 상인 11명(등대지기·BTR·Amonya·Harry·Gunwright·Ledger·PresetTrader·Elias Ward 등)은 원본 그대로다. 안 바뀌어 보이면 런처 Settings → Clean Temp Files. 제작자가 AI 생성 이미지를 썼다고 밝혔다. |
| [Ultimate Flea](https://sp-mod.com/mod/2844) | 1.2.0 | 7차 | 게임+서버 | **결속** 플레아를 전면 통제한다(품목 풀·가격·수급·레벨 락). 설치 때 **레벨 락 1,691개 품목 + 카테고리 17종**이 걸렸다. SVM 플레아 설정과 겹치는지 볼 것. |
| [500 Euro for Skier](https://sp-mod.com/mod/2434) | 2.0.0 | 7차 | 서버 | **결속** 스키어 거래를 루블이 아닌 유로로 바꾼다. |
| [Boxes At Ref (BARF)](https://sp-mod.com/mod/2483) | 3.0.0 | 7차 | 서버 | **결속** Arena의 전리품 상자류를 Ref 판매 목록에 넣는다. |

## 커스텀 상인·콘텐츠 (19개)

PortableTraders만 빼고 전부 프로필 결속. 넣고 플레이한 뒤 제거하면 프로필이 깨질 수 있다.

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
| [NATO Quartermaster](https://sp-mod.com/mod/3030) | 1.1.3 | 6차 | 서버 | **결속** 서방제 총기·방어구·탄약·완성 빌드를 파는 상인 Elias Ward. 매물 49개와 6단계 퀘스트 체인(뒤로 갈수록 좋은 장비가 열린다). 싸게 주는 치트 상인이 아니라 "비싸지만 바로 살 수 있는" 쪽이다. |
| [PortableTraders](https://github.com/wangbofan/SPT-PortableTraders) | 1.1.0 | 6차 | 서버 | **Forge 밖**(GitHub·MIT, 결속 아님). 레이드 안에서만 상대할 수 있던 **등대지기와 BTR 운전병**을 주 화면 상인 목록에 올려, 과제 30개를 받고 제출하게 한다(물품 보상은 메일로). 두 상인의 **판매 목록은 원래 비어 있다** — 아래 "헷갈리기 쉬운 정상 동작" 참고. |
| [Vanguard](https://sp-mod.com/mod/2917) | 2.0.0 | 6차 | 서버 | **결속** Insight WMLX 라이트와 Dead Air Sandman X 소음기를 흑·FDE 두 색으로 추가한다. WTT CommonLib 필요. |
| [Definitive Weapon Variants](https://sp-mod.com/mod/2010) | 4.3.2 | 7차 | 서버 | **결속** 기존 총기를 바탕으로 변형을 대량 추가한다 — 설치 때 **커스텀 아이템 776개**를 만들었고, 마크드룸 전리품에도 409종이 들어간다. |
| [AllGoodsTrader](https://sp-mod.com/mod/2648) | 2.0.0 | 7차 | 서버 | **결속** 이중 언어 상인 4명(Armorer 포함)을 추가한다. 대부분 아이템을 사 주고 수리·보험도 한다. 그 보험 상인은 Realistic Insurance가 모르는 상대라 **회수율 90% 기본값**으로 돈다. |
| [bluehead's AIO Trader](https://sp-mod.com/mod/374) | 5.3.1 | 7차 | 서버 | **결속** 거의 모든 물품을 한 상인에서 판다 — **품목 6,261개 / 매물 7,178개**. 치트성이 강하다. DLL 표기는 5.3.0. |
| [ALL AMMO TRADER](https://sp-mod.com/mod/1554) | 1.5.0 | 7차 | 서버 | **결속** 모든 탄약을 파는 상인 VAFELZ. |

## 서버 조정·도구·치트 (12개)

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
| [Item Limits Begone!](https://sp-mod.com/mod/2524) | 1.1.0 | 7차 | 서버 | 레이드·인벤토리·플레아 등록 수량 제한을 없앤다. |
| [Limitless](https://sp-mod.com/mod/2175) | 1.0.0 | 7차 | 서버 | **결속** 상인 구매 제한을 풀거나 배수로 늘린다. 설치 때 **26,179개 품목**에 적용됐다. |
| [Repair Max Durability](https://sp-mod.com/mod/1305) | 2.2.0 | 7차 | 게임+서버 | **결속** 무기의 최대 내구도를 되돌리는 아이템을 추가한다. |
| [Cobra's Durability Tweaks](https://sp-mod.com/mod/3019) | 1.0.0 | 7차 | 서버 | **결속** 내구도 소모를 줄이거나 없앤다 — 지금 설정은 **소모 12%**, 고장은 바닐라 그대로(무기 932개). 압축이 `SPT_RunTime`(대문자 T)으로 돼 있어 설치 때 경로를 고쳐야 했다. |

## 직접 만든 것

Forge에서 받은 모드가 아니라 이 저장소에서 만든 것. 소스는 [../src](../src) 아래에 있다.
지금 설치된 전체 목록은 [INSTALLED-MODS.md](INSTALLED-MODS.md) 의 "다른 경로로 설치된 것" 표가 기준이다.

| 모드 | 버전 | 위치 | 결속 | 설명 |
| --- | --- | --- | --- | --- |
| SptKoreanConfig | 0.1.0 | 게임 | - | F12(설정 창)에 보이는 글자를 한글로 바꾼다. 모드 83개의 설정 1457항목(이름·묶음 제목·설명)과 창 자체의 글자 33항목. 다른 모드 파일과 `ConfigurationManager.dll`은 건드리지 않고, 화면에 그릴 값만 바꾸므로 플러그인을 빼면 영문으로 돌아간다. 설정 값과 `.cfg` 파일에도 영향이 없다. |
| SptKoreanLocale | 0.1.0 | 서버 | - | 게임 안의 글자(아이템 이름·퀘스트·봇 역할 이름 …)를 한글로 바꾼다. F12 창을 맡는 SptKoreanConfig 와 짝이고 이쪽은 **서버가 내려주는 locale** 쪽이다. 지금까지 **봇 역할 이름 22 + 퀘스트 이름 1,243 + 퀘스트 목표 1,858항목**을 덮었다. 역할 이름은 킬 패널·시체 표시·지도 범주에 보이던 `ISB BOSS`·`WHITE TUSK`·`Infected`·`GHOST BUST.` 를 바꿨다. 세력 약어(`ISB`·`RUAF`·`UNTAR`·`REMNANT`)는 바닐라가 `USEC`·`BEAR` 를 영문으로 두는 것과 같이 그대로 둔다. 다른 모드 파일은 건드리지 않아 모드 업데이트에 날아가지 않는다. 퀘스트 이름은 목록·상인 화면·추적기·지도 표시에 쓰인다. 남은 범위는 [REVIEW-KOREAN-LOCALIZATION.md](REVIEW-KOREAN-LOCALIZATION.md) |
| PluginLocales | — | 게임 | - | 모드 폴더 안에 넣는 한국어 locale 파일 **4개 / 338항목**. ISB 무전·방송 자막 222줄, 보관함 정렬 창(Advanced Stash Sorting) 104, 위시리스트 확장 10, 주변 아이템 패널(Loot In Vicinity) 2. 네 모드 모두 `locales\` 를 훑어 파일 이름을 locale 코드로 쓰고 빠진 키는 영어로 떨어진다(역컴파일 실측). 게임이 서버에 `/client/locale/kr` 을 요청하므로 코드는 `kr` 이다. **그 모드를 업데이트하면 날아가므로** `tools/install-plugin-locales.ps1` 을 다시 돌린다. 확인은 `node tools/check-plugin-locales.mjs`. 사전은 [../src/PluginLocales](../src/PluginLocales) |
| GridWindowScroll | 0.1.0 | 게임 | - | 가방·상자·트렁크를 열었을 때 뜨는 내부 인벤 창이 화면을 넘어가지 않게 한다. 한 번에 보일 칸 수를 가로·세로로 못박고(기본 12x10), 그보다 큰 격자는 스크롤바와 마우스 휠로 넘겨 본다. 격자·칸 크기와 아이템 위치는 건드리지 않는다. 칸 수는 창 오른쪽 아래 손잡이를 끌어 그 자리에서 정하거나 F12 에서 정한다. 끌어 정한 값은 상자 종류마다 기억한다(`sizes.json`). F12 에서 숫자를 고치면 창을 닫고 다시 열어야 한다. |
| StackUnify | 0.1.0 | 서버 | - | 최대 스택 크기를 한 곳에서 정한다. 그 값을 쓰는 서버 모드가 13개나 되어 어느 값이 남는지 예측할 수 없었다. **클라이언트 목록이 굳기 전의 가장 늦은 자리**(`OnLoadOrder.PostLoad`)에서 설정에 적힌 값으로 못박는다. 기본 설정은 비어 있어 아무 일도 하지 않는다 — `config.jsonc` 의 `StackByCaliber`/`StackByItemId` 를 채워야 움직인다. `ReportOnly: true` 로 띄우면 바꾸지 않고 겹침만 알려 준다. |
| HelmetSlotUnion | 0.1.0 | 서버 | - | 헬멧의 부착 칸 필터를 **칸 이름별 합집합**으로 넓힌다. 같은 이름의 칸(`helmet_ears`·`mod_nvg`·`mod_equipment_000` …)을 헬멧 전체에서 모아 그 합집합을 각 칸에 더한다. 헬멧 64종·칸 묶음 11개, 필터 항목 495 → 6719. 기존 항목은 지우지 않고 더하기만 한다. |

번역을 고치려면 `BepInEx\plugins\SptKoreanConfig\lang\<모드 GUID>.json`을 고치고 F12를 다시 열면 된다(재시작 불필요). 창 자체의 글자(`lang\_ui.json`)만은 게임을 다시 켜야 한다.

HelmetSlotUnion 설정은 `SPT_Runtime\user\mods\HelmetSlotUnion\config.jsonc`이고 서버를 다시 켜야 적용된다. `helmet_top`·`helmet_back`은 내장 방탄판 자리(36종)라 기본값대로 합치면 아무 헬멧에 아무 방탄판이 들어간다 — 방어력을 그대로 두려면 `"ExcludedSlotNames": ["helmet_top", "helmet_back"]`. 자세한 내용은 [../src/HelmetSlotUnion/README.md](../src/HelmetSlotUnion/README.md).

## 프로필 결속 모드 (50개)

제거하려면 프로필 백업이 먼저다. 되돌릴 때는 `doc/install-records/installed-<Tag>.txt`의 경로만 지운다.

- [Amonya - Ammo Loving Trader & Quester](https://sp-mod.com/mod/2419) (2.2.0)
- [Caliber Split Storage Solutions](https://sp-mod.com/mod/2990) (1.0.1)
- [CaseTrader — Buy any case](https://sp-mod.com/mod/3024) (1.0.1)
- [Cobra's Ammo Stack](https://sp-mod.com/mod/2840) (1.1.0)
- [ECOTI - Enhanced Clip-On Thermal Imager](https://sp-mod.com/mod/2930) (3.4.2)
- [Gunsmith Task Trader](https://sp-mod.com/mod/3124) (1.0.2)
- [Gunwright](https://sp-mod.com/mod/3128) (1.0.2)
- [HarryHideout](https://sp-mod.com/mod/1303) (3.1.2)
- [Ledger](https://sp-mod.com/mod/3085) (1.1.0)
- [Lotus](https://sp-mod.com/mod/1232) (1.8.1)
- [Modding Stats Helper - by wara](https://sp-mod.com/mod/1300) (1.2.0)
- [Moe-TradeMarker](https://sp-mod.com/mod/2736) (1.2.6)
- [NATO Quartermaster](https://sp-mod.com/mod/3030) (1.1.3)
- [No Barrel Shadow](https://sp-mod.com/mod/2780) (2.0.1)
- [ODT's Item Info - SPT 4.0](https://sp-mod.com/mod/2430) (3.0.0)
- [PresetTrader](https://sp-mod.com/mod/2966) (1.2.0)
- [Ref - SPT Friendly Quests](https://sp-mod.com/mod/1538) (2.1.0)
- [Scorpion](https://sp-mod.com/mod/1348) (1.1.1)
- [Server Value Modifier [SVM]](https://sp-mod.com/mod/236) (2.2.3)
- [Trader Modding And Improved Weapon Building](https://sp-mod.com/mod/1283) (2.1.2)
- [Tylevo's Field Attachments](https://sp-mod.com/mod/3125) (1.2.0)
- [Vanguard](https://sp-mod.com/mod/2917) (2.0.0)
- [WTT- Armory](https://sp-mod.com/mod/2246) (3.0.0)
- [WTT- Artem](https://sp-mod.com/mod/1023) (3.0.2)
- [500 Euro for Skier](https://sp-mod.com/mod/2434) (2.0.0)
- [Airburst](https://sp-mod.com/mod/2948) (1.1.2)
- [AllGoodsTrader](https://sp-mod.com/mod/2648) (2.0.0)
- [ALL AMMO TRADER](https://sp-mod.com/mod/1554) (1.5.0)
- [AmmoGen](https://sp-mod.com/mod/2777) (2.4.0)
- [BETTER ATTACHMENTS](https://sp-mod.com/mod/1939) (4.1.5)
- [Black Exfil Revision](https://sp-mod.com/mod/2380) (1.1.0)
- [bluehead's AIO Trader](https://sp-mod.com/mod/374) (5.3.1)
- [Boxes At Ref (BARF)](https://sp-mod.com/mod/2483) (3.0.0)
- [Canted Aiming](https://sp-mod.com/mod/2415) (2.0.0)
- [Cobra's Durability Tweaks](https://sp-mod.com/mod/3019) (1.0.0)
- [Definitive Weapon Variants](https://sp-mod.com/mod/2010) (4.3.2)
- [Equipment Is Eternal](https://sp-mod.com/mod/2897) (1.2.0)
- [Full Automatic Guns](https://sp-mod.com/mod/1972) (4.1.5)
- [Holster Everything](https://sp-mod.com/mod/2650) (2.0.0)
- [Jero Backpack](https://sp-mod.com/mod/2369) (4.1.1)
- [Less Restricting Headwear](https://sp-mod.com/mod/922) (4.0.0)
- [Limitless](https://sp-mod.com/mod/2175) (1.0.0)
- [Magic Optic Mount](https://sp-mod.com/mod/3101) (0.1.0)
- [Realistic Suppressors](https://sp-mod.com/mod/3171) (1.0.0)
- [Repair Max Durability](https://sp-mod.com/mod/1305) (2.2.0)
- [Repair Skill Extension](https://sp-mod.com/mod/2556) (1.4.1)
- [Restore Armor Plates](https://sp-mod.com/mod/2973) (1.0.2)
- [ShrapnelFix](https://sp-mod.com/mod/3118) (1.0.0)
- [Slimmer Magazines](https://sp-mod.com/mod/2373) (2.0.0)
- [Ultimate Flea](https://sp-mod.com/mod/2844) (1.2.0)

## 알려진 충돌

### R(장전)이 먹지 않던 것 — UI Fixes `Always Reload Magazines In-Place` (2026-10-28, 해결)

**증상:** 게임에서 **R(장전)이 먹지 않는다.**

**원인:** UI Fixes(`com.tyfon.uifixes`)의 **`Always Reload Magazines In-Place`**
(= "탄창 항상 제자리 교체")를 **켜 둔 것**. 기본값은 **꺼짐**이다.

설정 설명은 이렇다 — *Force the reload to always be in-place, even if there's space to reload
normally. Note that in-place reloads are slower.* 즉 **빈 자리가 있어도 반드시 새 탄창과 헌
탄창의 자리를 맞바꾸는 식으로만** 장전하게 만든다. 맞바꿀 수 있는 자리가 없으면 **장전이
성립하지 않아 R 이 아무 일도 하지 않는다.**

**조치:** F12 → UI Fixes → `[K. Inventory]` 쪽의 **`Always Reload Magazines In-Place` 를 끈다**
(기본값으로 돌린다). 바로 아래 `Reload Magazines In-Place`(기본 켜짐)는 "필요할 때만" 맞바꾸는
것이라 켜 둬도 된다.

> 같은 증상이 또 나오면 **먼저 이 항목을 본다.** DirectReload 도 "주머니·탄띠에 빈 칸이 없으면
> 장전을 거부"하므로, 둘이 겹치면 더 잘 막힌다.

### Mag Retention Reload × Direct Reload — 탄창 소실 (2026-10-25 확인 / 2026-10-26 재발)

> **⚠ Mag Retention Reload의 `1. 일반` → `모드 사용`은 켜지 말 것.**
> 켜면 레이드 중 재장전에서 탄창이 사라진다. Direct Reload를 같이 쓰는 동안은 고쳐 쓸 길이 없다.
> 탄창 회수 기능은 UI Fixes가 맡으므로 **꺼도 잃는 기능이 없다**(`com.tyfon.uifixes.cfg`의
> `Reload Magazines In-Place = true` 확인됨). 둘 중 하나만 지우고 싶다면 Mag Retention Reload 쪽이다.
>
> **2026-10-26 재발했다.** 10-25에 끈 것이 F12에서 다시 켜져 같은 예외가 레이드 로그에 2건 찍혔다
> (`Direct load flow error | Trying to get parent of an item that doesn't have a parent`).
> F12 목록에서 이 모드를 보더라도 손대지 말라 — 켜는 순간 되돌아온다.

**증상** — 레이드 중 R로 직접 장전하면 화면에 `Reload failed: Direct load flow error`가 뜨고, 때때로 **탄창이 사라진다.** 한 레이드에서 14번 발생했고 탄창 종류와는 무관했다(D60 드럼 9, MPX TTI 4, UMP45 표준 1).

**원인** — 두 모드가 "재장전할 때 빼낸 탄창을 어디에 둘지"를 각자 고쳐 쓴다.

- Direct Reload는 **총에 이미 꽂혀 있는 탄창**을 "새로 넣을 탄창"으로 넘겨 게임의 `ReloadMag`를 부른다(빼낼 탄창과 넣을 탄창이 같은 객체다).
- Mag Retention Reload는 `ReloadExternalMagResult.Run`에 끼어들어 빼낸 탄창의 보관을 가로챈다.
- 그래서 떼어낸 직후 같은 객체를 다시 넣으려는 순간 부모가 없어 `Item.get_Parent`가 예외를 던진다.
- 예외가 트랜잭션 **중간**에 터지고 Direct Reload의 예외 처리에는 되돌리기가 없어, 떼어낸 탄창이 어느 칸에도 속하지 않은 채 남는다 — 이것이 "사라짐"이다.

**조치** — Mag Retention Reload를 끈다(F12 → `탄창 회수 재장전` → `1. 일반` → `모드 사용`). 탄창 회수 기능은 UI Fixes가 맡으므로 잃는 기능은 없다. 그 모드 자신의 설명도 "끄면 UI Fixes에 넘긴다"이다.

끄는 자리가 둘인데 **게임이 켜져 있으면 F12 쪽만 쓴다.** 설정 파일(`BepInEx/config/de.tosox.magretentionreload.cfg`)을 게임 실행 중에 고치면 종료할 때 메모리에 있던 값으로 덮어써져 되돌아간다. 파일에서 고치는 것은 게임이 꺼져 있을 때만 유효하다.

**확인 방법** — 세 조합을 하나씩 비교해 분리했다.

| UI Fixes `탄창 제자리 교체` | Mag Retention `모드 사용` | 결과 |
| --- | --- | --- |
| 켬 | 켬 | 오류 13건 |
| **끔** | 켬 | 오류 계속 (14번째) |
| 켬 | **끔** | **오류 0건**, `calling ReloadMag` 4회 도달·전부 통과 |
| 켬 | 켬 (10-26 재발) | 오류 2건 — 끈 것이 F12에서 다시 켜졌을 때 |

UI Fixes의 `탄창 제자리 교체`는 켠 채로 문제없다.

### 헷갈리기 쉬운 정상 동작

Direct Reload의 아래 두 알림은 고장이 아니다.

| 알림 | 뜻 |
| --- | --- |
| `Cannot start a reload right now` | 달리는 중 등 게임이 재장전을 거절하는 상태. 멈춰 서면 된다 |
| `No free slot to stow the current magazine` | 빼낸 탄창을 둘 빈 칸이 없음. 모드가 억지로 진행하지 않고 멈추는 설계다 |

**탄창 이름 아래 구경이 30줄로 늘어나는 것**(2026-10-25 해결)은 두 모드가 겹친 결과다. [All In Weapon: Unslotted](https://sp-mod.com/mod/2576)(7-A)가 탄창의 구경 필터를 지워 **모든 구경을 받는 탄창**으로 만들고, [Caliber Under Ammo Name](https://sp-mod.com/mod/2586)이 받는 구경을 전부 적기 때문이다(설치 로그의 `Fcked: weapons, mods, chambers, calibers, magazines`가 그 증거). 고치는 길은 둘이다 — ⒜ F12 → **이름 아래 구경 표시** → `탄창에 구경 표시`를 끄면 즉시 사라진다(**현재 이렇게 해 둠**). ⒝ `AllinWeaponUnslotted/config/config.jsonc`의 `FckMagazines`를 false로 두면 탄창이 제 구경만 받아 한 줄로 돌아온다(서버 재시작 필요, 나머지 자유도는 유지).

**SOMF(SitOnMyFace)가 "relaxed: 0"만 찍는 것도 정상이다**(2026-10-25 확인). 7-A 때는 머리/얼굴 차단 규칙 37건·세부 호환 2,089건을 풀었는데, 7-C에서 **Less Restricting Headwear(922)** 를 넣은 뒤로는 전부 0이 됐다. 922가 먼저 `117 block flags cleared / 4005 conflict entries removed`로 같은 일을 해 버려 SOMF가 풀 것이 남지 않는다. 둘은 겹치므로 **하나만 두면 된다** — 922 쪽이 다루는 품목이 더 많다(182개).

**등대지기·BTR 운전병의 판매 목록이 비어 있는 것도 정상이다**(2026-10-25 확인). PortableTraders(6차)가 이 둘을 주 화면 상인 목록에 올리지만, SPT 4.1.6 자체 DB에서 두 상인의 `assort.json`이 `items: []`다(메카닉은 385KB). 원래 **서비스·퀘스트 전용 상인**이라 그렇다 — 등대지기는 Zryachiy 지원·ExUsec 충성·교단 지원 3종, BTR은 물품 배달·택시·엄호·환승 배달 4종을 가진다. 재입고 타이머가 `00:00:00`인 것도 채울 물건이 없어서다. 이 모드의 값어치는 **임무 탭에서 두 상인의 과제 30개를 받고 제출**할 수 있게 되는 것이다(전에는 레이드 안에서만 가능). 물건을 팔게 하려면 Ultimate Trader Assort Editor로 직접 품목을 넣어야 하는데, 바닐라 설계를 벗어나므로 이번에는 하지 않기로 했다.

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
| SptKoreanConfig | `...\SptKoreanConfig\lang\*.json` | 번역 문구. 모드별 사전은 고치고 F12를 다시 열면 반영, `_ui.json`은 재시작 필요. F12 안에서 한글 표시 자체를 끌 수도 있다 |
| Transparent Sights | F12 메뉴 + 조준경 아이콘 더블클릭 | 파일로 받을 설정이 없다. `plugins\7Bpencil.TransparentSights\config.json`은 조준경별 전환을 저장할 때 모드가 직접 만든다(없으면 기본값). 형식은 `"<아이템 템플릿 ID>": "Enabled" / "EnabledWithMount" / "Disabled"` |
