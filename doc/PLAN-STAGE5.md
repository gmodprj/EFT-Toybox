# 설치 계획 — 5차 (SPT 4.1.6)

> 작성: 2026-10-24. 후보는 `doc/CANDIDATES-416.md`, 절차는 `doc/PROCEDURE.md`.

대상: `E:\Spt` (SPT 4.1.6, EFT 0.16.9.40743) / 현재 설치 41개.

## 고른 기준

1. **SVM으로 되는 건 모드로 넣지 않는다** — 기존 방침. FIR·구매 제한·레이드 시간·무게 등이 해당.
2. **이미 설치한 모드와 역할이 겹치면 넣지 않는다** — 같은 화면·같은 표기를 두 모드가 건드리면 어느 쪽이 이겼는지 알기 어렵다.
3. **결속(프로필 결속)은 뒤로 미루고 하나씩** — 넣고 플레이하면 빼기 어렵다. 비결속부터 쌓아 올린다.
4. **의존성이 새로 필요하거나 4.1.6 표기가 없는 건 보류** — 단독 설치로 확인한 뒤에.
5. **용량이 큰 콘텐츠 확장(총기 팩·커스텀 상인 대형)은 이번 범위에서 제외** — 넣을지부터 따로 정한다.

## 단계 요약

| 단계 | 내용 | 개수 | 다운로드 | 결속 |
| --- | --- | --- | --- | --- |
| `stage5-A` | 게임 전용 UI·편의 | 21 | 1.5MB | - |
| `stage5-B` | 총기·조준 편의 | 9 | 725KB | - |
| `stage5-C` | 탄약·장전 | 6 | 2.9MB | 2 |
| `stage5-D` | 상인·플레아 | 10 | 97.3MB | 6 |
| `stage5-E` | 치트·도구 | 6 | 498KB | - |
| **합계** | | **52** | **103.0MB** | **8** |

각 단계 공통 절차 (`doc/PROCEDURE.md` 그대로):

1. `doc/download/manifest-<stage>.json` 작성 → `node tools/download-stage.mjs <stage>`
2. `tar.exe -xf`로 `extracted/<stage>/`에 해제 → 기존 설치본·모드끼리 경로 충돌 검사
3. 프로필 백업: `E:\Spt\SPT_Runtime\user\profiles\*` → `backup/profiles-before-<stage>/`
4. 설치: `tools\install-and-test.ps1 -Stage <stage> -Pattern '<id>__*' -Tag '<tag>'` (덮어쓰기 발생 시 자동 중단)
5. 서버 로그로 모드 로드 확인 → 게임 실행해 `E:\Spt\BepInEx\LogOutput.log` 수정 시각 확인
6. `node tools/gen-installed-mods.mjs`로 `doc/INSTALLED-MODS.md` 재생성

## stage5-A — 게임 전용 UI·편의 (결속 없음)

예상 위치: 게임 (압축 해제 경로로 확정) / 다운로드 1.5MB

전부 `-SkipTest`로 복사하고 마지막에 한 번만 서버 테스트. 대부분 게임 쪽 플러그인으로 보이지만 실제 위치는 압축 해제 후 경로로 확정한다.

| 모드 | id | 버전 | 크기 | 고른 이유 |
| --- | --- | --- | --- | --- |
| [Use Items Anywhere](https://sp-mod.com/mod/2386) | 2386 | 2.1.6 | 1.2MB | 인벤토리 어디서나 아이템 사용. 단독 동작 |
| [AutoDeposit](https://sp-mod.com/mod/1469) | 1469 | 6.0.0 | 10KB | 컨테이너 퀵스택. 스태시 정리 효과 큼 |
| [Task List Fixes](https://sp-mod.com/mod/824) | 824 | 1.8.0 | 7KB | 작업 목록 정렬·성능. 설치된 Quest Tracker와 역할 다름 |
| [Exfil Flare Notification](https://sp-mod.com/mod/1249) | 1249 | 1.0.6 | 4KB | 탈출 조명탄 알림 |
| [Equip From Weapon Rack](https://sp-mod.com/mod/1136) | 1136 | 1.6.0 | 4KB | 무기 거치대에서 바로 장착 |
| [Loot Instant Search](https://sp-mod.com/mod/2069) | 2069 | 1.0.5 | 4KB | 컨테이너·시체 즉시 검색 |
| [LetMeRightClick](https://sp-mod.com/mod/2405) | 2405 | 1.1.1 | 3KB | 검색 중 우클릭 |
| [Auto Corpse Search](https://sp-mod.com/mod/2730) | 2730 | 2.2.0 | 9KB | 시체 장비 슬롯 자동 검색. 2069와 보완 관계 |
| [Discarder](https://sp-mod.com/mod/1745) | 1745 | 5.0.0 | 6KB | UI Fixes 기반 다중 버리기. 의존성 이미 설치됨 |
| [Advanced Stash Sorting](https://sp-mod.com/mod/2924) | 2924 | 1.0.6 | 117KB | 스태시 정렬 규칙. 1861(Stash Management Helper)은 역할이 겹쳐 제외 |
| [Stash Filter](https://sp-mod.com/mod/2907) | 2907 | 1.0.0 | 24KB | 스태시 검색 필터 탭 |
| [Linked Search In Stash](https://sp-mod.com/mod/2731) | 2731 | 2.0.1 | 6KB | 스태시 연결 검색 |
| [Raid Menu Cleanup Fix](https://sp-mod.com/mod/2733) | 2733 | 2.0.0 | 3KB | ESC 메뉴 프리즈 제거 |
| [Advanced Modding Lights](https://sp-mod.com/mod/3081) | 3081 | 2.0.0 | 7KB | 개조·빌드 화면 조명 |
| [Notification Filter](https://sp-mod.com/mod/2729) | 2729 | 2.0.0 | 5KB | 레이드 알림 선별 |
| [Tag Over Name](https://sp-mod.com/mod/3079) | 3079 | 1.0.2 | 18KB | 태그 있으면 이름 숨김 |
| [Hide Locked Flea](https://sp-mod.com/mod/3152) | 3152 | 1.0.0 | 15KB | 플레아 잠긴 매물 숨기기 |
| [Refined Flea Offer List](https://sp-mod.com/mod/2623) | 2623 | 1.2.0 | 10KB | 플레아 최저가만 표시 |
| [Hj's Task Search](https://sp-mod.com/mod/2909) | 2909 | 2.0.2 | 32KB | 퀘스트 검색 |
| [Pinned Quest HUD](https://sp-mod.com/mod/3155) | 3155 | 1.0.1 | 20KB | 인레이드 퀘스트 HUD |
| [Pre-Raid Checklist](https://sp-mod.com/mod/3160) | 3160 | 1.0.0 | 21KB | 출격 전 누락 경고 |

## stage5-B — 총기·조준 편의 (결속 없음)

예상 위치: 게임 (압축 해제 경로로 확정) / 다운로드 725KB

`-SkipTest` 일괄 복사 후 한 번 테스트. 조준경 관련은 게임에서 실제 조준해 보고 FPS·화질을 함께 확인한다.

| 모드 | id | 버전 | 크기 | 고른 이유 |
| --- | --- | --- | --- | --- |
| [Picture in Picture Disabler](https://sp-mod.com/mod/2667) | 2667 | 2.1.0 | 84KB | PiP 조준경 비활성화로 FPS 확보. 2200(DERP)·3076(Performance Scope)은 같은 영역을 다르게 건드려 동시 설치하지 않고 이것부터 |
| [Scope Rangefinder](https://sp-mod.com/mod/2735) | 2735 | 3.5.0 | 536KB | 조준경 거리 측정·자동 제로 |
| [Auto Ranging](https://sp-mod.com/mod/2786) | 2786 | 2.0.0 | 5KB | 조준점 자동 제로. 2735와 역할 보완 |
| [NVG Sight Dimmer](https://sp-mod.com/mod/2789) | 2789 | 2.1.0 | 5KB | 야투경 켜면 조준경 감광 |
| [Easy Mounting](https://sp-mod.com/mod/2790) | 2790 | 2.0.1 | 37KB | 무기 거치 조작 개선 |
| [Transparent Sights](https://sp-mod.com/mod/2774) | 2774 | 0.2.3 | 16KB | 양안 조준 |
| [Default Fire Mode Fixer](https://sp-mod.com/mod/1416) | 1416 | 2.1.0 | 6KB | 기본 발사모드 연발. 2740(Default Auto Fire Mode)은 중복이라 제외 |
| [Flexible Builds](https://sp-mod.com/mod/2646) | 2646 | 1.2.2 | 14KB | 무기 프리셋·빌드 개선 |
| [Better Thermal & Night Vision](https://sp-mod.com/mod/2531) | 2531 | 2.1.1 | 22KB | 열상·야투 화질/성능 |

## stage5-C — 탄약·장전 (결속 없음 → 결속 개별)

예상 위치: 게임+서버 (압축 해제 경로로 확정) / 다운로드 2.9MB

비결속 4개를 먼저 일괄 설치·테스트. 그 다음 결속 2개는 프로필 백업 후 하나씩.

| 모드 | id | 버전 | 크기 | 고른 이유 |
| --- | --- | --- | --- | --- |
| [Magazine Check Interrupt](https://sp-mod.com/mod/2643) | 2643 | 1.1.1 | 46KB | 탄창 확인 중 재장전 전환 |
| [Direct Reload & Painless Meds](https://sp-mod.com/mod/3074) | 3074 | 1.3.2 | 49KB | 느슨한 탄약에서 직접 재장전 |
| [Low Ammo Cue](https://sp-mod.com/mod/3046) | 3046 | 1.0.0 | 29KB | 잔탄 부족 신호음 |
| [BarterItemsStacks](https://sp-mod.com/mod/2480) | 2480 | 1.4.3 | 2.8MB | 교환 아이템 스택. 스태시 공간 효과 |
| [Cobra's Ammo Stack](https://sp-mod.com/mod/2840) | 2840 | 1.1.0 | 11KB | **결속** 구경별 탄약 스택 크기. 수치는 보수적으로 |
| [Caliber Split Storage Solutions](https://sp-mod.com/mod/2990) | 2990 | 1.0.1 | 47KB | **결속** 구경별 분리 보관 케이스 |

## stage5-D — 상인·플레아 (비결속 먼저, 결속 상인은 하나씩)

예상 위치: 서버 (압축 해제 경로로 확정) / 다운로드 97.3MB

비결속 4개 → 테스트. 결속 상인은 프로필 백업 후 하나씩 설치·테스트하고, 상인 목록이 넘치는지 게임에서 확인(설치된 TraderScrolling·TraderSpace가 받쳐 준다).

| 모드 | id | 버전 | 크기 | 고른 이유 |
| --- | --- | --- | --- | --- |
| [SPT Trader Restock Refresh](https://sp-mod.com/mod/3098) | 3098 | 1.0.0 | 10KB | 보고 있는 상인 화면 재입고 갱신 |
| [Trader Level Requirements Editor](https://sp-mod.com/mod/3005) | 3005 | 1.1.0 | 10KB | 상인 레벨 요구치 편집 |
| [Trader Namer](https://sp-mod.com/mod/1527) | 1527 | 4.1.0 | 11KB | 상인 이름·아바타 |
| [Ultimate Trader Assort Editor](https://sp-mod.com/mod/2834) | 2834 | 2.1.1 | 159KB | 상인 판매 목록 편집 도구. 조정용으로 먼저 |
| [Gunsmith Task Trader](https://sp-mod.com/mod/3124) | 3124 | 1.0.2 | 185KB | **결속** 건스미스 과제별 총기 상인. 과제 진행에 직접 도움 |
| [Gunwright](https://sp-mod.com/mod/3128) | 3128 | 1.0.2 | 1.0MB | **결속** 건스미스 프리셋·빌드 판매 |
| [CaseTrader — Buy any case](https://sp-mod.com/mod/3024) | 3024 | 1.0.1 | 322KB | **결속** 케이스 판매 상인. 스태시 정리와 맞물림 |
| [PresetTrader](https://sp-mod.com/mod/2966) | 2966 | 1.2.0 | 1.3MB | **결속** 저장 빌드 판매 |
| [Ledger](https://sp-mod.com/mod/3085) | 3085 | 1.1.0 | 494KB | **결속** 도그태그 상인 + 퀘스트 40종 |
| [Amonya - Ammo Loving Trader & Quester](https://sp-mod.com/mod/2419) | 2419 | 2.2.0 | 93.9MB | **결속** 탄약 전문 상인 + 퀘스트. 96MB |

## stage5-E — 치트·도구 (SVM으로 되는 것 제외)

예상 위치: 게임+서버 (압축 해제 경로로 확정) / 다운로드 498KB

전부 선택 사항. 넣기로 한 것만 일괄 복사 후 한 번 테스트. 성격상 게임 난이도를 바꾸므로 하나씩 켜 보며 결정해도 된다.

| 모드 | id | 버전 | 크기 | 고른 이유 |
| --- | --- | --- | --- | --- |
| [Freecam](https://sp-mod.com/mod/164) | 164 | 1.4.8 | 6KB | 자유 시점 카메라. 스크린샷·확인용으로 실용적 |
| [Cobra's SPT Explorer](https://sp-mod.com/mod/3009) | 3009 | 1.0.0 | 407KB | 읽기 전용 DB 탐색기. 치트 표시지만 조회만 함 |
| [Set Speed - Set Player Speed with Hotkeys](https://sp-mod.com/mod/994) | 994 | 1.5.0 | 3KB | 걷기 속도 프리셋 단축키 |
| [Fast Grenade Throw](https://sp-mod.com/mod/2783) | 2783 | 2.0.0 | 7KB | 수류탄 즉시 투척 복원 |
| [Low Budget Bullet Time NG](https://sp-mod.com/mod/1988) | 1988 | 2.3.0 | 69KB | 불릿 타임. 취향 강함 |
| [Tripwire Improvement](https://sp-mod.com/mod/2618) | 2618 | 1.1.0 | 5KB | 와이어 트랩 무한·즉시 폭발 |

## 보류 — 결정이 필요한 것 (32개)

넣고 싶지만 먼저 정할 게 있는 것들. 괄호 안이 걸리는 점.

| 모드 | id | 분야 | 버전 | 크기 | 걸리는 점 |
| --- | --- | --- | --- | --- | --- |
| [Item Preview QoL + screenshots](https://sp-mod.com/mod/2206) | 2206 | UI | 1.2.0 | 12KB | 아이템 미리보기 QoL. **결속**이라 단독 단계로 돌려야 함. 넣을지 먼저 결정 |
| [Price Color Sync](https://sp-mod.com/mod/3151) | 3151 | UI | 1.1.4 | 22KB | Show Me The Money 가격 동기화. **결속**. 가격 표시를 더 손볼 때 |
| [Interface 1.1 Backport](https://sp-mod.com/mod/3179) | 3179 | UI | 1.0.4 | 8.0MB | 1.0~1.2 인터페이스 전체 백포트. 설치된 UI Fixes·ImpactCue류와 충돌 가능 → 반드시 단독 설치·검증 |
| [ImpactCue-UI-MOD](https://sp-mod.com/mod/3029) | 3029 | UI | 2.0.3 | 40.4MB | 전술 HUD(40MB). 3148(HUD Breakout)과 택일. HUD를 바꿀지 정한 뒤 |
| [DynamicMaps Extended](https://sp-mod.com/mod/3097) | 3097 | UI | 2.5.1 | 1.8GB | DynamicMaps 확장. 1.8GB + Map Variants(3040, 3.9MB) 동반. 용량 결정 필요 |
| [Briefing](https://sp-mod.com/mod/3158) | 3158 | UI | 1.0.2 | 385KB | 퀘스트·장비 통합 탭. Quest Codex(3054, 2.9MB) 동반 설치 필요 |
| [Combat Log](https://sp-mod.com/mod/3072) | 3072 | UI | 1.0.0 | 123KB | 교전 분석. WebOverlay(2879, 170KB) 동반 |
| [Raid Review](https://sp-mod.com/mod/1479) | 1479 | UI | 1.8.0 | 5.4MB | 레이드 웹 리플레이(5.4MB). 서버 쪽 부하·포트 확인 후 |
| [StatTrack](https://sp-mod.com/mod/1853) | 1853 | UI | 2.1.1 | 23KB | StatTrack. Preview Sizer(2339) 권장 조합이라 같이 볼 것 |
| [Scrollable Attachments](https://sp-mod.com/mod/2677) | 2677 | 총기 | 2.0.0 | 7KB | 부착물 스크롤. 의존성 UnityToolkit(1426) 최신이 **4.1.5 표기** — 4.1.6 미표기. 넣으려면 단독 설치 후 서버 테스트로 확인 |
| [Tylevo's Field Attachments](https://sp-mod.com/mod/3125) | 3125 | 총기 | 1.2.0 | 21.0MB | 인벤토리 안 열고 부착물 교체(21MB). 같은 UnityToolkit 문제 + **결속** |
| [Canted Aiming](https://sp-mod.com/mod/2415) | 2415 | 총기 | 2.0.0 | 11KB | 경사 조준. **결속**, WTT CommonLib는 이미 설치됨 |
| [Better Rear Sights](https://sp-mod.com/mod/1591) | 1591 | 총기 | 1.8.0 | 51.6MB | 기계식 조준기 개선(53MB). **결속** |
| [Deadzone](https://sp-mod.com/mod/3067) | 3067 | 총기 | 1.2.0 | 7KB | 데드존(프리 에임). 조작 감각이 크게 바뀜 → 따로 체험 |
| [Easy Ammo Names](https://sp-mod.com/mod/1262) | 1262 | 탄약 | 2.1.0 | 16KB | 탄약 이름 변경. 설치된 Caliber Under Ammo Name·Ammo Stats와 표기 충돌 가능 → 게임에서 먼저 확인 |
| [Ammo Stats In Names](https://sp-mod.com/mod/2989) | 2989 | 탄약 | 1.0.0 | 16KB | 탄약 이름에 수치. 1262와 택일 |
| [Munitions Expert - Reloaded](https://sp-mod.com/mod/3006) | 3006 | 탄약 | 3.1.0 | 14KB | 탄약 숨은 수치. 설치된 ODT Item Info와 역할 겹침 |
| [Manimal's Ammo Loading Animations](https://sp-mod.com/mod/2681) | 2681 | 탄약 | 2.0.0 | 18.6MB | 한 발씩 장전 애니메이션(19MB). **결속**, Continuous Load Ammo는 설치됨 |
| [Quick Sell](https://sp-mod.com/mod/2955) | 2955 | 상인 | 4.3.0 | 50KB | 빠른 판매. 설치된 QuickSellFlea와 중복 → 교체할지 결정 |
| [[Melx] Loot Value](https://sp-mod.com/mod/3086) | 3086 | 상인 | 1.1.0 | 20KB | 툴팁 가격. 설치된 Show Me The Money와 중복 |
| [Fast Sell In Flea](https://sp-mod.com/mod/2207) | 2207 | 상인 | 1.3.1 | 12KB | 플레아 현재가 자동 입력. QuickSellFlea와 겹침 |
| [Better Flea](https://sp-mod.com/mod/3177) | 3177 | 상인 | 2.1.0 | 354KB | 실섭 플레아 재현. SVM 플레아 설정과 충돌 확인 필요 |
| [Advanced Flea Simulator](https://sp-mod.com/mod/2526) | 2526 | 상인 | 2.0.1 | 33KB | 플레아 시뮬레이터. 3177과 택일 |
| [Progressive Flea](https://sp-mod.com/mod/2539) | 2539 | 상인 | 2.0.0 | 9KB | 플레아 레벨 제한. 난이도 방향 결정 후 |
| [Croupier - loadout generator + flea quicksell](https://sp-mod.com/mod/1971) | 1971 | 상인 | 2.0.7 | 20.6MB | 로드아웃 생성기(21MB). **결속** |
| [TraderGen](https://sp-mod.com/mod/2749) | 2749 | 상인 | 2.4.1 | 1.3MB | 커스텀 상인 프레임워크. 이걸 요구하는 상인을 넣을 때 함께 |
| [Infinite Everything](https://sp-mod.com/mod/3108) | 3108 | 치트 | 2.5.3 | 56KB | 18종 무한 옵션. SVM과 크게 중복 → SVM으로 먼저 시도 |
| [Super Sprint and Jump](https://sp-mod.com/mod/2612) | 2612 | 치트 | 2.0.0 | 6KB | 달리기·점프 20배. 설치된 Open Sesame와 성격 겹침 |
| [MagicBullet](https://sp-mod.com/mod/3065) | 3065 | 치트 | 1.0.0 | 4KB | 데미지 배수. 치트 표시 |
| [Tushonka-800: Combat Augmentation System](https://sp-mod.com/mod/3169) | 3169 | 치트 | 1.0.1 | 43KB | 전투 강화(스캔·방어막). 치트 표시, 게임성 크게 변함 |
| [DebugTools](https://sp-mod.com/mod/3050) | 3050 | 치트 | 1.1.0 | 375KB | 인레이드 DB 뷰어·스폰. 설치된 give-ui와 역할 겹침 |
| [Map Editor Lite](https://sp-mod.com/mod/2805) | 2805 | 치트 | 2.1.0 | 2.4MB | 맵 콘텐츠 제작 프레임워크. 맵을 만들 때만 |

## 이번에 빼는 것 (52개)

| 모드 | id | 뺀 이유 |
| --- | --- | --- |
| [🛠️ Limitless – Unlimited or Multiply Buy Limits](https://sp-mod.com/mod/2175) | 2175 | SVM의 구매 제한 설정으로 됨 |
| [Item Limits Begone!](https://sp-mod.com/mod/2524) | 2524 | SVM의 제한 해제 설정과 중복 |
| [Raids Are Infinite](https://sp-mod.com/mod/2903) | 2903 | SVM의 레이드 시간 설정과 중복 |
| [Actually Found In Raid (UPDATED)](https://sp-mod.com/mod/2011) | 2011 | SVM의 FIR 설정과 중복 |
| [Found to FIR](https://sp-mod.com/mod/3198) | 3198 | SVM의 FIR 설정과 중복 + 결속 |
| [FIR Fence Purchases](https://sp-mod.com/mod/2371) | 2371 | SVM의 FIR 설정과 중복 |
| [Stash Management Helper](https://sp-mod.com/mod/1861) | 1861 | 2924(Advanced Stash Sorting)와 역할 중복 |
| [Default Auto Fire Mode](https://sp-mod.com/mod/2740) | 2740 | 1416(Default Fire Mode Fixer)와 중복 |
| [Shorter Magazines](https://sp-mod.com/mod/3186) | 3186 | 2373(Slimmer Magazines)과 중복, 둘 다 결속 |
| [Slimmer Magazines](https://sp-mod.com/mod/2373) | 2373 | 탄창 크기 변경은 결속이고 되돌리기 어려움 |
| [HUD Breakout](https://sp-mod.com/mod/3148) | 3148 | 3029와 택일. HUD를 둘 다 넣으면 충돌 |
| [Performance Scope](https://sp-mod.com/mod/3076) | 3076 | 2667과 같은 영역(조준경 렌더) |
| [Dynamic External Resolution Patch (DERP)](https://sp-mod.com/mod/2200) | 2200 | 2667과 같은 영역(조준경 렌더) |
| [READABLE KEYS by DeadW0Lf](https://sp-mod.com/mod/1981) | 1981 | 설치된 Better Keys NG와 중복 |
| [Ultrawide Stash](https://sp-mod.com/mod/3053) | 3053 | 울트라와이드 모니터가 아니면 효과 없음 |
| [Audio Accessibility Indicators](https://sp-mod.com/mod/1760) | 1760 | 청각 보조용. 필요 시에만 |
| [Wishlist Extended](https://sp-mod.com/mod/2500) | 2500 | 위시리스트 확장. 효용 낮음 |
| [Career Log](https://sp-mod.com/mod/2713) | 2713 | 웹 통계. 1479와 역할 겹침 |
| [Keymaster](https://sp-mod.com/mod/3134) | 3134 | 열쇠 자동 패킹(6.7MB). 설치된 Better Keys NG + 보관으로 충분 |
| [Loadout Manager](https://sp-mod.com/mod/3181) | 3181 | 로드아웃 저장. 1971과 역할 겹침 |
| [Trader Only Part Markers](https://sp-mod.com/mod/3162) | 3162 | 상인 부품 표시. 플레아를 쓰면 효용 낮음 |
| [Holster Everything](https://sp-mod.com/mod/2650) | 2650 | 홀스터 확장. 결속 |
| [Fold That Stock](https://sp-mod.com/mod/2691) | 2691 | 접이식 개머리판(58MB). 결속 |
| [Repair Max Durability](https://sp-mod.com/mod/1305) | 1305 | 내구도 복구 아이템. 결속 |
| [Cobra's Durability Tweaks](https://sp-mod.com/mod/3019) | 3019 | 내구도·고장 축소. 치트성 + 결속 |
| [Repair Skill Extension](https://sp-mod.com/mod/2556) | 2556 | 수리 버프 상향. 치트성 + 결속 |
| [BETTER ATTACHMENTS](https://sp-mod.com/mod/1939) | 1939 | 부착물 전체 상향. 치트성 + 결속 |
| [Realistic Suppressors](https://sp-mod.com/mod/3171) | 3171 | 소음기 비현실 강화. 치트성 + 결속 |
| [Flash Suppressor](https://sp-mod.com/mod/3137) | 3137 | 총구 화염 조절. 결속, 효용 낮음 |
| [Recoil Rework (Legacy)](https://sp-mod.com/mod/2190) | 2190 | 반동 재조정(레거시). 바닐라 반동 방향을 바꿀지 먼저 결정 |
| [Camo And Stickers](https://sp-mod.com/mod/2658) | 2658 | 위장·스티커. 외형 전용 |
| [Weapon Build Master](https://sp-mod.com/mod/2669) | 2669 | 빌드 코드 공유. 혼자 하면 효용 낮음 |
| [Bela.s.Speedloader](https://sp-mod.com/mod/2458) | 2458 | 장전 속도·스택 상향. 치트성 |
| [AmmoGen](https://sp-mod.com/mod/2777) | 2777 | 탄약 제작 프레임워크. 커스텀 탄약을 만들 때만 |
| [The Blacklist - flea market enhancements](https://sp-mod.com/mod/755) | 755 | 플레아 블랙리스트 해제. SVM 플레아 설정과 중복 |
| [.22lr Ammunition](https://sp-mod.com/mod/2516) | 2516 | .22lr 탄종(30MB). 결속, C11을 넣을 때만 |
| [bluehead's AIO Trader](https://sp-mod.com/mod/374) | 374 | 전 품목 단일 상인. 치트성 강함 |
| [ALL AMMO TRADER - UPDATED BY TILTUSHKIN](https://sp-mod.com/mod/1554) | 1554 | 전 탄약 상인. 치트성 |
| [AllGoodsTrader](https://sp-mod.com/mod/2648) | 2648 | 상인 4명 + 전 품목 매입. 치트성 |
| [The Quartermaster](https://sp-mod.com/mod/2817) | 2817 | 플레이어 장터. ItemGen(2803, 결속) 동반 필요 |
| [ISB Aishi](https://sp-mod.com/mod/2478) | 2478 | 상인 1종에 4.5GB + WTT Content Backport(2512) **6.0GB** 동반 |
| [Astral Divide](https://sp-mod.com/mod/3091) | 3091 | 652MB + WTT Black Division(2511) → SAIN·BigBrain·MoreBotsAPI까지 연쇄 |
| [Terminal Commander](https://sp-mod.com/mod/2999) | 2999 | 157MB + VisitAPI(2788, 결속) 동반 |
| [Rook](https://sp-mod.com/mod/3055) | 3055 | 의존성 Eco’s Attachment Emporium(2882) 최신이 **4.1.1 표기** — 4.1.6 미호환 |
| [Epic's All in One](https://sp-mod.com/mod/1263) | 1263 | 1.5GB 무기 대확장. 결속. 콘텐츠 방향을 정한 뒤 별도 단계 |
| [C11 - True North](https://sp-mod.com/mod/2423) | 2423 | 1.8GB + .22lr 동반. 결속 |
| [Make Shotguns Great Again!](https://sp-mod.com/mod/1453) | 1453 | 390MB 샷건 개편. 결속 |
| [Definitive Weapon Variants](https://sp-mod.com/mod/2010) | 2010 | 총기 변형 600종. 결속 |
| [Painter](https://sp-mod.com/mod/1025) | 1025 | Painter ↔ Tactical Gear Component(1125, 641MB) 상호 의존. 결속 |
| [Magic Optic Mount](https://sp-mod.com/mod/3101) | 3101 | 광학+열상 동시 장착. 결속, 치트성 |
| [Instant Insurance](https://sp-mod.com/mod/2394) | 2394 | 사망 후 보험 유지. 치트성 |
| [Hj's Improved Customization UI](https://sp-mod.com/mod/3069) | 3069 | 커스터마이즈 탭. 외형 전용 |

## 순서와 확인 지점

```
stage5-A (UI 21개, 게임 전용)
   └ 게임 실행: 인벤토리·스태시·검색·플레아 화면이 전부 열리는지
stage5-B (총기 9개, 게임 전용)
   └ 게임 실행: 조준경 들어가 FPS·화질, 거리 측정·자동 제로 동작
stage5-C (탄약 4개 + 결속 2개)
   └ 비결속 먼저 → 결속은 프로필 백업 후 하나씩
stage5-D (상인 4개 + 결속 상인 6개)
   └ 결속 상인은 하나 넣을 때마다 서버 테스트 + 상인 목록 스크롤 확인
stage5-E (치트·도구, 선택)
   └ 넣기로 한 것만
```

막히는 지점은 대개 둘이다. 하나는 **게임 쪽 플러그인이 조용히 안 뜨는 경우** — 서버 로그는 깨끗한데 기능이 없다. `LogOutput.log`에서 해당 플러그인 이름을 찾는다. 다른 하나는 **같은 화면을 건드리는 모드끼리의 충돌** — stage5-A에 UI가 21개 몰려 있어 문제가 생기면 범위를 좁히기 어렵다. 그래서 A를 스태시·검색(1469·2069·2405·2730·2924·2907·2731)과 그 나머지로 두 번에 나눠 복사해도 된다.

## 이 계획에서 하지 않는 것

- 다운로드·설치·설정 변경을 하지 않는다. 이 문서는 무엇을 어떤 순서로 넣을지만 정한다.
- 대용량 콘텐츠 확장(총기 팩, 대형 커스텀 상인)은 범위 밖. 넣을지부터 따로 정한 뒤 6차로 계획한다.
- SVM 프리셋은 건드리지 않는다. 치트성 기능은 SVM 쪽을 먼저 쓴다.

## 실행 결과 (2026-10-24)

| 단계 | 설치 | 서버 모드 | 서버 테스트 | 비고 |
| --- | --- | --- | --- | --- |
| stage5-A | 21 | 19 → 20 | STARTED 29s | 게임 쪽 21개 로드 확인(18:21) |
| stage5-B | 9 | 20 → 23 | STARTED 26s | 3개가 서버 쪽(계획의 "게임 전용" 정정) |
| stage5-C | 6 | 23 → 27 | 4건 모두 STARTED | 결속 2개는 하나씩. CobrasAmmoStack 압축 경로 교정 |
| stage5-D | 10 | 27 → 37 | 1건 FAILED → 수정 후 STARTED | TraderNamer 빈 폴더 누락으로 서버 중단 |
| stage5-E | 6 | 37 → 38 | STARTED 35s | Explorer UI는 README대로 SPT 루트에 설치 |

합계 **52개 설치**, 설치본 41 → 93개, 파일 1809 → 2280개.

### 절차에서 고친 것

- `tools/check-conflicts.mjs` 신규 — 해제본 경로를 `E:\Spt`와 비교(이미 있는 경로·모드 간 중복·설치 경로 아닌 파일).
- `tools/install-and-test.ps1` — 압축에 들어 있던 **빈 폴더도 생성**한다. TraderNamer가 `avatars/`를 열면서 서버가 죽은 뒤 수정. 생성한 폴더는 `installed-<Tag>.dirs.txt`에 기록.
- `tools/gen-installed-mods.mjs` — `manifest-stage<N>-<X>.json` 형식도 읽고, 로드 현황을 `loadStatus` 상수로 분리.

### 압축 구조가 달라 손본 것

| 모드 | 문제 | 처리 |
| --- | --- | --- |
| Tag Over Name(3079) | 읽어보기·소스·중첩 복사본이 섞여 있음 | `extracted/_extras-stage5-A/`로 분리 |
| Cobra's Ammo Stack(2840) | `<루트>/SPT/user/mods/...` (이 설치본은 `SPT_Runtime/`) | 경로 이동 |
| Trader Level Req Editor(3005) | 폴더가 한 겹 더 싸여 있음 | 평탄화 |
| Cobra's SPT Explorer(3009) | 외부 UI 앱·문서가 SPT 루트로 들어감 | README 지시대로 설치 |

### 남은 확인

- 게임 쪽 16개(stage5-B 조준경 6개, 5-C 3개, 5-D 2개, 5-E 5개) — 게임 실행 후 `LogOutput.log`.
- 상인 6명이 목록에 모두 보이는지(TraderScrolling·TraderSpace 동작).
- Cobra's Ammo Stack 기본 스택 500 유지 결정(사용자 판단). stage5-E 테스트 시점에는 탄약 **427종**에 적용됨(Amonya 추가분 포함).
- Direct Reload의 의료 사용시간 x0.5·약물 지속 x2 기본값이 SVM 설정과 겹치는지.
