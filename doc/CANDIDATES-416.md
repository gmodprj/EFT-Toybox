# 미설치 후보 모드 (SPT 4.1.6)

> 조사: 2026-10-24. 출처 sp-mod.com API v0 (`filter[spt_version]=4.1.6`). 재조회는 `node tools/survey-candidates.mjs`.

- 대상 설치본: `E:\Spt` — SPT **4.1.6**, EFT **0.16.9.40743** (`SPT_Data/configs/core.json`, `user/logs/spt/spt20261024.log` 확인)
- 사이트 전체 **2021개** 중 4.1.6 호환 **626개**. 이미 설치된 **41개**를 뺀 **585개**에서 요청한 5개 분야(UI·총기·탄약·상인·치트)로 추린 **136개**.
- 아래 버전은 API로 확인한 **4.1.6 호환 최신 버전**이다. 전부 실제 호환 버전이 존재함.
- **결속** = 프로필 결속 표시(설치 후 플레이하면 제거 시 프로필 손상 가능) / **치트** = Forge 치트 표시.
- 의존성: ~~취소선~~은 이미 설치된 것, **굵게**는 새로 받아야 하는 것.
- 설치는 `doc/PROCEDURE.md` 절차대로. SVM으로 되는 기능은 모드를 추가하지 않는 기존 방침에 따라 메모에 중복 표시해 둠.

## UI·편의 (44개)

| 모드 | 버전 | 크기 | 다운로드 | 갱신 | 표시 | 의존성 | 메모 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [Use Items Anywhere](https://sp-mod.com/mod/2386) | 2.1.6 | 1.2MB | 124,240 | 2026-10-07 | - | - | 인벤토리 어디서나 아이템 사용·단축키 지정 |
| [AutoDeposit](https://sp-mod.com/mod/1469) | 6.0.0 | 10KB | 106,111 | 2026-08-12 | - | - | 컨테이너에 같은 아이템 자동 이동(테라리아식 퀵스택) |
| [Task List Fixes](https://sp-mod.com/mod/824) | 1.8.0 | 7KB | 90,041 | 2026-08-03 | - | - | 작업 목록 정렬·성능 수정. 설치된 Quest Tracker와 역할 다름 |
| [Audio Accessibility Indicators](https://sp-mod.com/mod/1760) | 2.1.0 | 52KB | 59,566 | 2026-08-07 | - | - | 청각 접근성 인디케이터(발사·질주 방향 표시) |
| [Raid Review](https://sp-mod.com/mod/1479) | 1.8.0 | 5.4MB | 57,804 | 2026-10-01 | - | - | 레이드 후 웹 리플레이·히트맵 |
| [StatTrack](https://sp-mod.com/mod/1853) | 2.1.1 | 23KB | 47,196 | 2026-08-07 | - | - | 무기별 킬·헤드샷 누적 추적. Preview Sizer(2339) 권장 |
| [Exfil Flare Notification](https://sp-mod.com/mod/1249) | 1.0.6 | 4KB | 42,457 | 2026-08-09 | - | - | 탈출 조명탄 사용 알림 |
| [Equip From Weapon Rack](https://sp-mod.com/mod/1136) | 1.6.0 | 4KB | 38,208 | 2026-08-03 | - | - | 무기 거치대에서 바로 장착 |
| [Loot Instant Search](https://sp-mod.com/mod/2069) | 1.0.5 | 4KB | 37,899 | 2026-09-04 | - | - | 컨테이너·시체 즉시 검색 |
| [Stash Management Helper](https://sp-mod.com/mod/1861) | 0.3.0 | 22KB | 27,853 | 2026-08-22 | - | - | 스태시 정렬 보강. 2924와 역할 중복 → 하나만 |
| [LetMeRightClick](https://sp-mod.com/mod/2405) | 1.1.1 | 3KB | 26,209 | 2026-08-21 | - | - | 검색 중 우클릭 허용 |
| [Item Preview QoL + screenshots](https://sp-mod.com/mod/2206) | 1.2.0 | 12KB | 18,553 | 2026-09-20 | **결속** | - | 아이템 미리보기 확대·회전·스크린샷. UI Fixes 호환 |
| [Discarder](https://sp-mod.com/mod/1745) | 5.0.0 | 6KB | 17,910 | 2026-08-09 | - | ~~UI Fixes~~(설치됨) | UI Fixes 기반 다중 선택 버리기 |
| [Auto Corpse Search](https://sp-mod.com/mod/2730) | 2.2.0 | 9KB | 14,781 | 2026-09-19 | - | - | 시체 전리품 창 열면 장비 슬롯 자동 검색 |
| [Scrollable Attachments](https://sp-mod.com/mod/2677) | 2.0.0 | 7KB | 13,092 | 2026-10-04 | - | **UnityToolkit**(1426) | 개조 화면 부착물 목록 스크롤 |
| [DynamicMaps Extended](https://sp-mod.com/mod/3097) | 2.5.1 | 1.8GB | 11,967 | 2026-10-21 | - | ~~Dynamic Maps~~(설치됨), **Map Variants**(3040) | 설치된 Dynamic Maps 확장(백포트 맵 추가). 용량 큼 |
| [ImpactCue-UI-MOD](https://sp-mod.com/mod/3029) | 2.0.3 | 40.4MB | 11,858 | 2026-10-12 | - | - | 피격·킬 피드백, 전술 HUD, 무기 아이콘 |
| [Quick Sell](https://sp-mod.com/mod/2955) | 4.3.0 | 50KB | 9,936 | 2026-10-17 | - | - | 최적 상인/플레아 즉시 판매 + 툴팁 가격. 설치된 QuickSellFlea와 역할 중복 |
| [Raid Menu Cleanup Fix](https://sp-mod.com/mod/2733) | 2.0.0 | 3KB | 9,790 | 2026-08-08 | - | - | 레이드 중 ESC 메뉴 열 때 프리즈 제거 |
| [Wishlist Extended](https://sp-mod.com/mod/2500) | 1.2.0 | 53KB | 8,225 | 2026-08-31 | - | - | 자동 위시리스트 확장 설정 |
| [READABLE KEYS by DeadW0Lf](https://sp-mod.com/mod/1981) | 4.1.5 | 32KB | 8,179 | 2026-10-14 | **결속** | - | 열쇠·키카드 설명·짧은 이름 개선. 설치된 Better Keys NG와 중복 → 하나만 |
| [Hj's Task Search](https://sp-mod.com/mod/2909) | 2.0.2 | 32KB | 6,515 | 2026-09-22 | - | - | 1.0 스타일 퀘스트 검색 |
| [Hj's Improved Customization UI](https://sp-mod.com/mod/3069) | 1.1.1 | 187KB | 4,242 | 2026-10-04 | - | - | 캐릭터 커스터마이즈 탭 개선 + 포토 모드 |
| [Refined Flea Offer List](https://sp-mod.com/mod/2623) | 1.2.0 | 10KB | 3,384 | 2026-10-10 | - | - | 플레아 카테고리에서 최저가만 표시 |
| [Advanced Stash Sorting](https://sp-mod.com/mod/2924) | 1.0.6 | 117KB | 3,278 | 2026-09-16 | - | - | 스태시 정렬 규칙 설정형. 1861과 중복 → 하나만 |
| [Linked Search In Stash](https://sp-mod.com/mod/2731) | 2.0.1 | 6KB | 3,276 | 2026-08-20 | - | - | 스태시에서 연결 검색(컨텍스트 메뉴) |
| [[Melx] Loot Value](https://sp-mod.com/mod/3086) | 1.1.0 | 20KB | 2,764 | 2026-10-01 | - | - | 툴팁에 플레아·최적 상인 가격·슬롯당 가격. Show Me The Money와 중복 검토 |
| [Career Log](https://sp-mod.com/mod/2713) | 2.0.0 | 1.6MB | 2,688 | 2026-09-21 | - | - | 레이드 기록·평생 통계·스태시 이력 웹 뷰 |
| [Interface 1.1 Backport](https://sp-mod.com/mod/3179) | 1.0.4 | 8.0MB | 2,608 | 2026-10-21 | - | - | 1.0~1.2 인터페이스 전체 백포트. 설치된 UI 모드와 충돌 가능 → 단독 검증 필요 |
| [Keymaster](https://sp-mod.com/mod/3134) | 1.1.1 | 6.6MB | 2,054 | 2026-10-15 | - | - | 맵별 열쇠·현금 자동 패킹 케이스 |
| [Briefing](https://sp-mod.com/mod/3158) | 1.0.2 | 385KB | 1,818 | 2026-10-22 | - | **Quest Codex**(3054) | 퀘스트·장비 상태 통합 탭 |
| [Stash Filter](https://sp-mod.com/mod/2907) | 1.0.0 | 24KB | 1,386 | 2026-08-13 | - | - | 스태시 검색에 무기·방탄·탄창·탄약 필터 탭 추가 |
| [Advanced Modding Lights](https://sp-mod.com/mod/3081) | 2.0.0 | 7KB | 1,282 | 2026-09-29 | - | - | 개조·빌드 화면 조명 개선 |
| [HUD Breakout](https://sp-mod.com/mod/3148) | 1.2.0 | 9.3MB | 1,160 | 2026-10-18 | - | - | 커스터마이즈형 전술 HUD. 3029와 중복 → 하나만 |
| [Ultrawide Stash](https://sp-mod.com/mod/3053) | 1.1.4 | 86KB | 1,099 | 2026-10-23 | - | - | 울트라와이드 스태시 확장 |
| [Notification Filter](https://sp-mod.com/mod/2729) | 2.0.0 | 5KB | 1,007 | 2026-08-08 | - | - | 레이드 중 알림 팝업 선별 |
| [Tag Over Name](https://sp-mod.com/mod/3079) | 1.0.2 | 18KB | 854 | 2026-09-29 | - | - | 태그 있으면 아이템 이름 숨김 |
| [Price Color Sync](https://sp-mod.com/mod/3151) | 1.1.4 | 22KB | 698 | 2026-10-16 | **결속** | ~~Show Me The Money (Item Pricing) (4.1.x)~~(설치됨) | 인벤토리 배경 등급과 Show Me The Money 가격 동기화 |
| [Combat Log](https://sp-mod.com/mod/3072) | 1.0.0 | 123KB | 598 | 2026-09-27 | - | **WebOverlay**(2879) | 교전 후 피격 분석·무기 통계 |
| [Pinned Quest HUD](https://sp-mod.com/mod/3155) | 1.0.1 | 20KB | 535 | 2026-10-16 | - | - | 핀 고정 퀘스트 인레이드 HUD 추적기 |
| [Pre-Raid Checklist](https://sp-mod.com/mod/3160) | 1.0.0 | 21KB | 319 | 2026-10-16 | - | - | 출격 전 필수품 누락 경고 |
| [Loadout Manager](https://sp-mod.com/mod/3181) | 1.0.1 | 164KB | 281 | 2026-10-19 | - | - | 착용 장비 저장·원클릭 재착용 |
| [Hide Locked Flea](https://sp-mod.com/mod/3152) | 1.0.0 | 15KB | 223 | 2026-10-15 | - | - | 플레아에서 잠긴 매물 숨기기 필터 |
| [Trader Only Part Markers](https://sp-mod.com/mod/3162) | 1.0.0 | 320KB | 164 | 2026-10-16 | - | ~~Trader Modding And Improved Weapon Building~~(설치됨) | 상인 구매 가능 부품 표시 |

## 총기·조준·부착물 (34개)

| 모드 | 버전 | 크기 | 다운로드 | 갱신 | 표시 | 의존성 | 메모 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [Painter](https://sp-mod.com/mod/1025) | 3.0.0 | 21.2MB | 318,623 | 2026-09-19 | **결속** | **Tactical Gear Component**(1125), ~~WTT - CommonLib~~(설치됨) | 도장 부품·장비 상인 Painter |
| [Epic's All in One](https://sp-mod.com/mod/1263) | 5.0.2 | 1.5GB | 283,129 | 2026-10-16 | **결속** | ~~WTT - CommonLib~~(설치됨) | 무기·부품 대규모 확장 + Badger 상인. 용량 매우 큼 |
| [Better Rear Sights](https://sp-mod.com/mod/1591) | 1.8.0 | 51.6MB | 56,980 | 2026-09-17 | **결속** | - | 기계식 조준기 가시성 개선 |
| [Make Shotguns Great Again!](https://sp-mod.com/mod/1453) | 1.19.1 | 380.7MB | 53,019 | 2026-10-24 | **결속** | ~~WTT - CommonLib~~(설치됨) | 샷건 리밸런스·신규 탄종 |
| [Camo And Stickers](https://sp-mod.com/mod/2658) | 1.19.0 | 212KB | 52,357 | 2026-09-15 | - | - | 위장·스티커 도장 |
| [C11 - True North](https://sp-mod.com/mod/2423) | 4.0.29 | 1.7GB | 47,618 | 2026-10-14 | **결속** | ~~WTT - CommonLib~~(설치됨), **.22lr Ammunition**(2516) | 해외 총기·부품 추가. 용량 매우 큼 |
| [Definitive Weapon Variants](https://sp-mod.com/mod/2010) | 4.3.2 | 143KB | 43,992 | 2026-08-09 | **결속** | - | 기존 총기 기반 변형 600종+ |
| [Repair Max Durability](https://sp-mod.com/mod/1305) | 2.2.0 | 30KB | 38,770 | 2026-10-03 | **결속** | - | 무기 최대 내구도 복구 아이템 추가 |
| [Recoil Rework (Legacy)](https://sp-mod.com/mod/2190) | 2.0.0 | 35KB | 35,571 | 2026-09-09 | - | - | 반동 재조정(레거시) |
| [BETTER ATTACHMENTS](https://sp-mod.com/mod/1939) | 4.1.5 | 54KB | 33,709 | 2026-10-14 | **결속** | - | 모든 부착물 인간공학·반동 개선. 치트성 |
| [Picture in Picture Disabler](https://sp-mod.com/mod/2667) | 2.1.0 | 84KB | 29,808 | 2026-10-19 | - | - | PiP 조준경 비활성화로 FPS 확보 |
| [Better Thermal & Night Vision](https://sp-mod.com/mod/2531) | 2.1.1 | 22KB | 27,991 | 2026-10-10 | - | - | 열상·야투 화질/성능 개선 |
| [Dynamic External Resolution Patch (DERP)](https://sp-mod.com/mod/2200) | 1.1.2 | 7KB | 27,654 | 2026-08-19 | - | - | 조준경 외부 렌더 해상도 동적 조절(성능) |
| [Canted Aiming](https://sp-mod.com/mod/2415) | 2.0.0 | 11KB | 22,539 | 2026-08-12 | **결속** | ~~WTT - CommonLib~~(설치됨) | 백업 사이트 없이 경사 조준 |
| [Default Fire Mode Fixer](https://sp-mod.com/mod/1416) | 2.1.0 | 6KB | 17,098 | 2026-08-09 | - | - | 무기 기본 발사모드 연발로 수정 |
| [Scope Rangefinder](https://sp-mod.com/mod/2735) | 3.5.0 | 536KB | 15,729 | 2026-09-29 | - | - | 조준경 내 거리 측정·자동 제로 |
| [NVG Sight Dimmer](https://sp-mod.com/mod/2789) | 2.1.0 | 5KB | 14,911 | 2026-08-27 | - | - | 야투경 켜면 조준경 밝기 자동 감광 |
| [Easy Mounting](https://sp-mod.com/mod/2790) | 2.0.1 | 37KB | 11,066 | 2026-08-23 | - | - | 무기 거치(마운트) 조작 개선 |
| [Auto Ranging](https://sp-mod.com/mod/2786) | 2.0.0 | 5KB | 11,027 | 2026-08-01 | - | - | 조준한 지점으로 조준경 자동 제로 |
| [Slimmer Magazines](https://sp-mod.com/mod/2373) | 2.0.0 | 6KB | 10,512 | 2026-08-12 | **결속** | - | 2x2 탄창을 1x2로 |
| [Holster Everything](https://sp-mod.com/mod/2650) | 2.0.0 | 37KB | 7,235 | 2026-08-21 | **결속** | - | 홀스터 슬롯에 장착 가능 품목 확대 |
| [Transparent Sights](https://sp-mod.com/mod/2774) | 0.2.3 | 16KB | 6,479 | 2026-10-02 | - | - | 양안 조준(조준경 반투명) |
| [Flexible Builds](https://sp-mod.com/mod/2646) | 1.2.2 | 14KB | 5,025 | 2026-08-03 | - | - | 무기 프리셋·빌드 개선 |
| [Fold That Stock](https://sp-mod.com/mod/2691) | 2.1.0 | 56.3MB | 4,538 | 2026-09-01 | **결속** | - | 비바닐라 접이식 개머리판 작동 |
| [Default Auto Fire Mode](https://sp-mod.com/mod/2740) | 2.0.0 | 4KB | 3,258 | 2026-08-08 | - | - | 무기 꺼낼 때 자동 연발 전환. 1416과 중복 → 하나만 |
| [Weapon Build Master](https://sp-mod.com/mod/2669) | 1.2.0 | 24KB | 2,597 | 2026-08-07 | - | - | 무기 빌드 코드로 공유 |
| [Performance Scope](https://sp-mod.com/mod/3076) | 0.2.0 | 11KB | 1,581 | 2026-09-29 | - | - | 조준경 해상도 조절. 2200·2667과 역할 중복 |
| [Magic Optic Mount](https://sp-mod.com/mod/3101) | 0.1.0 | 19KB | 1,296 | 2026-10-04 | **결속** | ~~Weapon Customizer~~(설치됨), ~~WTT - CommonLib~~(설치됨) | 광학 + 열상/야투 동시 장착 |
| [Tylevo's Field Attachments](https://sp-mod.com/mod/3125) | 1.2.0 | 21.0MB | 1,033 | 2026-10-13 | **결속** | **UnityToolkit**(1426) | 인벤토리 안 열고 광학·소음기·손잡이 교체 |
| [Cobra's Durability Tweaks](https://sp-mod.com/mod/3019) | 1.0.0 | 12KB | 1,003 | 2026-09-12 | **결속** | - | 내구도 소모·고장 축소/비활성. 치트성 |
| [Realistic Suppressors](https://sp-mod.com/mod/3171) | 1.0.0 | 8KB | 970 | 2026-10-18 | **결속** | - | 소음기 성능 비현실적 강화. 치트성 |
| [Deadzone](https://sp-mod.com/mod/3067) | 1.2.0 | 7KB | 905 | 2026-09-26 | - | - | 데드존(프리 에임) 추가 |
| [Shorter Magazines](https://sp-mod.com/mod/3186) | 1.1.0 | 7KB | 832 | 2026-10-22 | **결속** | - | 3x1 탄창을 2x1로. 2373과 중복 → 하나만 |
| [Flash Suppressor](https://sp-mod.com/mod/3137) | 1.0.0 | 4KB | 364 | 2026-10-13 | **결속** | - | 총구 화염·연기 조절 |

## 탄약·탄창 (14개)

| 모드 | 버전 | 크기 | 다운로드 | 갱신 | 표시 | 의존성 | 메모 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [The Blacklist - flea market enhancements](https://sp-mod.com/mod/755) | 3.1.1 | 80KB | 171,089 | 2026-08-25 | - | - | 플레아 블랙리스트 해제·가격 재조정. SVM 플레아 설정과 중복 확인 |
| [BarterItemsStacks](https://sp-mod.com/mod/2480) | 1.4.3 | 2.8MB | 57,307 | 2026-09-03 | - | - | 교환용 아이템 스택 크기 변경 |
| [Easy Ammo Names](https://sp-mod.com/mod/1262) | 2.1.0 | 16KB | 30,901 | 2026-08-03 | - | - | 탄약 이름을 알기 쉽게 변경. 설치된 Caliber Under Ammo Name·Ammo Stats와 겹침 확인 |
| [.22lr Ammunition](https://sp-mod.com/mod/2516) | 2.0.0 | 28.9MB | 28,233 | 2026-09-25 | **결속** | ~~WTT - CommonLib~~(설치됨) | .22lr 탄종 추가. C11(2423)의 선행 조건 |
| [Manimal's Ammo Loading Animations](https://sp-mod.com/mod/2681) | 2.0.0 | 18.6MB | 22,787 | 2026-09-14 | **결속** | ~~Continuous Load Ammo~~(설치됨), ~~WTT - CommonLib~~(설치됨) | 탄 한 발씩 장전 1인칭 애니메이션 |
| [Magazine Check Interrupt](https://sp-mod.com/mod/2643) | 1.1.1 | 46KB | 18,847 | 2026-09-10 | - | - | 탄창 확인 중 바로 재장전 전환 |
| [Bela.s.Speedloader](https://sp-mod.com/mod/2458) | 1.2.4 | 23KB | 10,146 | 2026-08-14 | - | - | 장전 속도·탄 스택 증가. 치트성 수치 포함 |
| [AmmoGen](https://sp-mod.com/mod/2777) | 2.4.0 | 64KB | 7,308 | 2026-08-04 | **결속** | - | 커스텀 탄약 제작 프레임워크(비개발자용) |
| [Cobra's Ammo Stack](https://sp-mod.com/mod/2840) | 1.1.0 | 11KB | 6,596 | 2026-08-06 | **결속** | - | 구경별 탄약 스택 크기 설정. 치트성 |
| [Munitions Expert - Reloaded](https://sp-mod.com/mod/3006) | 3.1.0 | 14KB | 4,162 | 2026-10-21 | - | - | 탄약·폭발물 숨은 수치 표시. 설치된 ODT Item Info와 역할 겹침 |
| [Low Ammo Cue](https://sp-mod.com/mod/3046) | 1.0.0 | 29KB | 2,055 | 2026-09-20 | - | - | 잔탄 부족 시 금속음 신호 |
| [Direct Reload & Painless Meds](https://sp-mod.com/mod/3074) | 1.3.2 | 49KB | 1,313 | 2026-10-18 | - | - | 느슨한 탄약에서 직접 재장전 |
| [Ammo Stats In Names](https://sp-mod.com/mod/2989) | 1.0.0 | 16KB | 1,190 | 2026-09-03 | - | - | 탄약 이름에 관통·데미지 표기. 1262와 중복 → 하나만 |
| [Caliber Split Storage Solutions](https://sp-mod.com/mod/2990) | 1.0.1 | 47KB | 1,100 | 2026-09-11 | **결속** | - | 구경별 분리 보관 케이스 |

## 상인·플레아 (26개)

| 모드 | 버전 | 크기 | 다운로드 | 갱신 | 표시 | 의존성 | 메모 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [bluehead's AIO Trader](https://sp-mod.com/mod/374) | 5.3.1 | 41KB | 154,807 | 2026-08-15 | **결속** | - | 모든 물품 한 상인에서 구매 + 열쇠 일괄. 치트성 강함 |
| [Fast Sell In Flea](https://sp-mod.com/mod/2207) | 1.3.1 | 12KB | 80,138 | 2026-08-23 | - | - | 플레아 등록 시 현재가 자동 입력. 설치된 QuickSellFlea와 역할 겹침 |
| [ALL AMMO TRADER - UPDATED BY TILTUSHKIN](https://sp-mod.com/mod/1554) | 1.5.0 | 2.8MB | 55,781 | 2026-09-14 | **결속** | - | 모든 탄약 판매 상인. 치트성 |
| [ISB Aishi](https://sp-mod.com/mod/2478) | 2.0.4 | 4.3GB | 45,162 | 2026-10-20 | **결속** | ~~WTT - CommonLib~~(설치됨), **WTT - Content Backport**(2512) | 퀘스트 110개 커스텀 상인. 용량 매우 큼 |
| [Croupier - loadout generator + flea quicksell](https://sp-mod.com/mod/1971) | 2.0.7 | 20.6MB | 43,078 | 2026-08-19 | **결속** | - | 20초 로드아웃 생성 + 플레아 즉시 판매 |
| [Instant Insurance](https://sp-mod.com/mod/2394) | 1.1.2 | 38KB | 37,792 | 2026-09-05 | - | - | 사망 후에도 보험 아이템 유지. 치트성 |
| [Amonya - Ammo Loving Trader & Quester](https://sp-mod.com/mod/2419) | 2.2.0 | 93.9MB | 18,234 | 2026-08-18 | **결속** | - | 탄약 전문 커스텀 상인 + 퀘스트 |
| [The Quartermaster](https://sp-mod.com/mod/2817) | 1.4.3 | 5.7MB | 13,654 | 2026-08-03 | **결속** | ~~WTT - CommonLib~~(설치됨), **ItemGen**(2803) | 플레이어 중고 장터(글로벌) |
| [FIR Fence Purchases](https://sp-mod.com/mod/2371) | 1.1.0 | 6KB | 11,982 | 2026-08-10 | - | - | Fence 구매품 전부 FIR 처리. SVM FIR 설정과 중복 |
| [Advanced Flea Simulator](https://sp-mod.com/mod/2526) | 2.0.1 | 33KB | 9,283 | 2026-08-20 | - | - | 플레아 체감을 실섭에 가깝게 시뮬레이트 |
| [Trader Namer](https://sp-mod.com/mod/1527) | 4.1.0 | 11KB | 8,632 | 2026-08-03 | - | - | 상인 이름·아바타 변경 |
| [TraderGen](https://sp-mod.com/mod/2749) | 2.4.1 | 1.3MB | 6,583 | 2026-08-17 | **결속** | ~~WTT - CommonLib~~(설치됨) | 커스텀 상인 제작 프레임워크 |
| [Rook](https://sp-mod.com/mod/3055) | 1.2.2 | 747KB | 5,921 | 2026-10-19 | **결속** | **Epic's All in One**(1263), **ECOT - Eukyre's Consortium of Things**(2195), ~~WTT- Armory~~(설치됨), ~~WTT - CommonLib~~(설치됨), **WTT - Content Backport**(2512), **Eco's Attachment Emporium**(2882) | 큐레이션 무기·장비 상인. 미설치 의존성 많아 부담 큼 |
| [Terminal Commander](https://sp-mod.com/mod/2999) | 1.3.0 | 153.6MB | 5,630 | 2026-09-23 | **결속** | ~~WTT - CommonLib~~(설치됨), **VisitAPI Framework**(2788) | 음성 연출 스토리 상인 |
| [Progressive Flea](https://sp-mod.com/mod/2539) | 2.0.0 | 9KB | 4,426 | 2026-08-12 | - | - | 플레아 아이템에 레벨 제한 추가 |
| [AllGoodsTrader](https://sp-mod.com/mod/2648) | 2.0.0 | 589KB | 3,849 | 2026-09-17 | **결속** | - | 이중 언어 상인 4명. 대부분 아이템 매입 + 수리·보험 |
| [Astral Divide](https://sp-mod.com/mod/3091) | 0.2.0 | 636.8MB | 3,205 | 2026-10-09 | **결속** | ~~WTT - CommonLib~~(설치됨), **WTT - Black Division [REDACTED] Home**(2511), **VisitAPI Framework**(2788) | 스토리형 커스텀 상인. 용량 큼 |
| [PresetTrader](https://sp-mod.com/mod/2966) | 1.2.0 | 1.3MB | 3,183 | 2026-09-01 | **결속** | - | 저장한 빌드를 비싸게 파는 상인 |
| [CaseTrader — Buy any case](https://sp-mod.com/mod/3024) | 1.0.1 | 322KB | 2,009 | 2026-09-17 | **결속** | - | 각종 케이스 판매 상인 |
| [Ledger](https://sp-mod.com/mod/3085) | 1.1.0 | 494KB | 1,937 | 2026-10-01 | **결속** | - | 도그태그 상인 + 퀘스트 40종 |
| [Ultimate Trader Assort Editor](https://sp-mod.com/mod/2834) | 2.1.1 | 159KB | 1,892 | 2026-09-06 | - | - | 상인 판매 목록 편집 도구 |
| [Gunwright](https://sp-mod.com/mod/3128) | 1.0.2 | 1.0MB | 1,463 | 2026-10-19 | **결속** | - | 건스미스 프리셋·커스텀 빌드 판매 상인 |
| [Gunsmith Task Trader](https://sp-mod.com/mod/3124) | 1.0.2 | 185KB | 1,383 | 2026-10-11 | **결속** | - | 건스미스 과제별 총기 판매 상인 |
| [SPT Trader Restock Refresh](https://sp-mod.com/mod/3098) | 1.0.0 | 10KB | 674 | 2026-10-02 | - | - | 보고 있는 상인 화면 재입고 시 자동 갱신 |
| [Better Flea](https://sp-mod.com/mod/3177) | 2.1.0 | 354KB | 622 | 2026-10-24 | - | - | 실섭 플레아 재현(라이브 가격·밴). SVM 플레아 설정과 충돌 확인 |
| [Trader Level Requirements Editor](https://sp-mod.com/mod/3005) | 1.1.0 | 10KB | 513 | 2026-10-19 | - | - | 상인 레벨 요구치 편집 |

## 치트·디버그 (18개)

| 모드 | 버전 | 크기 | 다운로드 | 갱신 | 표시 | 의존성 | 메모 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [Freecam](https://sp-mod.com/mod/164) | 1.4.8 | 6KB | 53,564 | 2026-08-01 | - | - | 자유 시점 카메라(스크린샷·관전용) |
| [Actually Found In Raid (UPDATED)](https://sp-mod.com/mod/2011) | 1.1.1 | 6KB | 26,887 | 2026-10-02 | - | - | 봇 사망 전리품을 FIR로. SVM FIR 설정과 중복 |
| [Item Limits Begone!](https://sp-mod.com/mod/2524) | 1.1.0 | 5KB | 19,321 | 2026-08-05 | - | - | 인레이드·인벤토리·플레아 등록 제한 해제. SVM과 중복 |
| [Low Budget Bullet Time NG](https://sp-mod.com/mod/1988) | 2.3.0 | 69KB | 9,619 | 2026-08-17 | - | - | 불릿 타임(시간 감속) |
| [Repair Skill Extension](https://sp-mod.com/mod/2556) | 1.4.1 | 15KB | 9,440 | 2026-10-22 | **결속** | - | 수리 버프 확률 상향·전 방어구 적용. 치트성 |
| [Raids Are Infinite](https://sp-mod.com/mod/2903) | 1.1.0 | 14KB | 8,232 | 2026-08-23 | - | - | 레이드 시간 무제한. SVM 레이드 시간과 중복 |
| [Fast Grenade Throw](https://sp-mod.com/mod/2783) | 2.0.0 | 7KB | 7,918 | 2026-08-01 | - | - | 수류탄 즉시 투척 복원 |
| [Set Speed - Set Player Speed with Hotkeys](https://sp-mod.com/mod/994) | 1.5.0 | 3KB | 5,569 | 2026-08-03 | - | - | 단축키로 걷기 속도 프리셋 전환 |
| [Tripwire Improvement](https://sp-mod.com/mod/2618) | 1.1.0 | 5KB | 5,515 | 2026-10-10 | - | - | 와이어 트랩 무한·즉시 폭발 |
| [🛠️ Limitless – Unlimited or Multiply Buy Limits](https://sp-mod.com/mod/2175) | 1.0.0 | 27KB | 5,391 | 2026-08-03 | **결속** | - | 상인 구매 제한 해제/배수. SVM으로 가능 — 중복 |
| [Super Sprint and Jump](https://sp-mod.com/mod/2612) | 2.0.0 | 6KB | 4,572 | 2026-10-10 | - | - | 달리기·점프 최대 20배 |
| [Map Editor Lite](https://sp-mod.com/mod/2805) | 2.1.0 | 2.4MB | 4,297 | 2026-08-04 | 치트 | ~~WTT - CommonLib~~(설치됨) | 커스텀 맵 콘텐츠 제작 프레임워크 |
| [Infinite Everything](https://sp-mod.com/mod/3108) | 2.5.3 | 56KB | 2,561 | 2026-10-16 | - | - | 탄약·탄창·열쇠·돈·체력 등 18종 무한 옵션 전환 |
| [DebugTools](https://sp-mod.com/mod/3050) | 1.1.0 | 375KB | 899 | 2026-09-22 | - | - | 인레이드 DB 뷰어·아이템 스폰·디버그. 설치된 give-ui와 역할 겹침 |
| [MagicBullet](https://sp-mod.com/mod/3065) | 1.0.0 | 4KB | 612 | 2026-09-25 | 치트 | - | 플레이어 데미지 배수(봇은 바닐라) |
| [Tushonka-800: Combat Augmentation System](https://sp-mod.com/mod/3169) | 1.0.1 | 43KB | 442 | 2026-10-19 | 치트 | - | 적 스캔·방어막 등 전투 강화 |
| [Cobra's SPT Explorer](https://sp-mod.com/mod/3009) | 1.0.0 | 407KB | 240 | 2026-09-08 | 치트 | - | 읽기 전용 DB 탐색기. 치트 표시지만 조회 전용 |
| [Found to FIR](https://sp-mod.com/mod/3198) | 1.0.1 | 6KB | 209 | 2026-10-23 | **결속** | - | 탈출 성공 시 비FIR을 FIR로. SVM과 중복 |

## 분포

| 분야 | 키워드 분류 결과(미설치) | 이 문서에 추린 수 |
| --- | --- | --- |
| UI·편의 | 104 | 44 |
| 총기·조준·부착물 | 79 | 34 |
| 탄약·탄창 | 32 | 14 |
| 상인·플레아 | 53 | 26 |
| 치트·디버그 | 17 | 18 |

- 1차 분류는 Forge 카테고리 + 이름·소개 키워드 기준이고, 한 모드가 여러 분야에 겹쳐 들어간다(예: Painter는 총기·상인). 가운데 수치는 그 중복을 포함한 값.
- 치트는 추린 수가 분류 결과보다 많다. 키워드에 안 걸리지만 성격상 치트인 것(Freecam, 속도·점프·시간 조절, 수리 버프 등)을 손으로 더했다.

## 제외한 것

- 4.1.6 호환 버전이 없는 모드(사이트 전체 2021개 중 1395개).
- 요청 5개 분야 밖: 맵·로케이션, 봇·AI, 퀘스트, 의류·외형, 오디오, 하이드아웃, 번역, 모드 개발용 프레임워크 등.
- 같은 분야여도 다운로드가 매우 적고 갱신이 멈춘 소규모 모드.
