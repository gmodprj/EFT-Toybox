# 설치된 모드 목록

> 자동 생성: `node tools/gen-installed-mods.mjs` (설치 기록·매니페스트 기반). 직접 고치지 말고 기록을 고친 뒤 다시 생성.

- 대상: `E:\Spt` (SPT 4.1.6, EFT 0.16.9.5.40743)
- 설치된 모드: **186개**, 설치 파일 **10630개** (자동 생성 설정 파일 포함)
- 게임 쪽 플러그인 로드: 145개 (2026-10-29, AirdropCaller.Plugin 추가 후. 마지막 기동은 메뉴까지만 돌아 오류 2건: ①Fika 미설치 관련 DirectReload 패치 ②Show Me The Money 툴팁 NullReference. 레이드에 들어가면 COTI per-frame 과 PIT `LootPatrolLayer.GetDecision` NullReference 가, UTAE 편집 화면을 열면 기준선 로드 실패가 더 날 수 있다) / 서버 모드 로드: 116개 (2026-10-30 ScavRaidWaves 추가 후 테스트, 정상 시작 136초 — `server-tests/server-test-scavraidwaves.out.txt`. **오류 0건**, `Could not add` 48건 그대로 — 원인은 `MountOlympus.dll` 로, 모드가 만들지 않는 아이템 id 48개를 들고 있다(`REVIEW-LOAD-ORDER.md`))
- **결속** = Forge "프로필 결속" 표시. 플레이 후 제거하면 프로필이 깨질 수 있음

## 1차 — UI·탄약 정보·개조 화면 편의

| 모드 | 버전 | 위치 | 결속 | 파일 | 설치 기록 |
| --- | --- | --- | --- | --- | --- |
| [Color Converter API](https://sp-mod.com/mod/1090) | 1.2.0 | 게임 | - | 1 | `batch1` |
| [UI Fixes](https://sp-mod.com/mod/1342) | 6.0.4 | 게임+서버 | - | 3 | `batch1` |
| [Dynamic Maps](https://sp-mod.com/mod/1431) | 1.2.1 | 게임+게임 데이터+서버 | - | 95 | `batch2` |
| [Amands' Sense - Updated](https://sp-mod.com/mod/2521) | 3.1.0 | 게임 | - | 61 | `batch2` |
| [Quest Tracker](https://sp-mod.com/mod/1140) | 1.7.0 | 게임 | - | 2 | `batch1` |
| [ODT's Item Info - SPT 4.0](https://sp-mod.com/mod/2430) | 3.0.0 | 서버 | **결속** | 6 | `batch2` |
| [Show Me The Money (Item Pricing) (4.1.x)](https://sp-mod.com/mod/3011) | 3.0.2 | 게임+서버 | - | 3 | `batch1` |
| [Better Keys NG](https://sp-mod.com/mod/1888) | 1.8.0 | 서버 | - | 18 | `batch2` |
| [Player Encumbrance Bar](https://sp-mod.com/mod/1374) | 1.3.0 | 게임 | - | 3 | `batch2` |
| [Search Open Containers](https://sp-mod.com/mod/934) | 1.5.0 | 게임 | - | 1 | `batch1` |
| [Quick Move To Containers](https://sp-mod.com/mod/1341) | 1.5.0 | 게임 | - | 1 | `batch1` |
| [Armor Class Icon](https://sp-mod.com/mod/2934) | 1.1.1 | 게임 | - | 1 | `batch2` |
| [Weapon Builder Search](https://sp-mod.com/mod/2743) | 2.0.0 | 게임 | - | 1 | `batch4` |
| [Trader Search](https://sp-mod.com/mod/2824) | 2.1.0 | 게임 | - | 1 | `batch4` |
| [Ammo Stats](https://sp-mod.com/mod/167) | 4.1.0 | 서버 | - | 5 | `batch3` |
| [Caliber Under Ammo Name](https://sp-mod.com/mod/2586) | 1.2.0 | 게임 | - | 1 | `batch3` |
| [Chamber Ammo Info](https://sp-mod.com/mod/2553) | 1.1.0 | 게임 | - | 1 | `batch3` |
| [BALL - Better Ammo Loading List](https://sp-mod.com/mod/2221) | 1.1.1 | 게임 | - | 1 | `batch3` |
| [Continuous Load Ammo](https://sp-mod.com/mod/2112) | 1.2.0 | 게임 | - | 2 | `batch3` |
| [Compatibility Highlighter](https://sp-mod.com/mod/2859) | 1.3.0 | 게임 | - | 1 | `batch3` |
| [Colored Tracers](https://sp-mod.com/mod/2037) | 3.0.1 | 서버 | - | 3 | `batch3` |
| [Trader Modding And Improved Weapon Building](https://sp-mod.com/mod/1283) | 2.1.2 | 게임+서버 | **결속** | 2 | `batch4` |
| [Modding Stats Helper - by wara](https://sp-mod.com/mod/1300) | 1.2.0 | 게임 | **결속** | 1 | `batch4` |

## 2차 — 편의·WTT 콘텐츠·SVM

| 모드 | 버전 | 위치 | 결속 | 파일 | 설치 기록 |
| --- | --- | --- | --- | --- | --- |
| [Weapon Customizer](https://sp-mod.com/mod/1950) | 4.0.0 | 게임+서버 | - | 3 | `stage2-A` |
| [Mag Retention Reload](https://sp-mod.com/mod/2589) | 1.3.0 | 게임 | - | 2 | `stage2-A` |
| [Unload All Magazines](https://sp-mod.com/mod/2739) | 2.2.1 | 게임 | - | 1 | `stage2-A` |
| [AmmoBox1x1](https://sp-mod.com/mod/2402) | 2.0.0 | 서버 | - | 1 | `stage2-A` |
| [QuickSellFlea](https://sp-mod.com/mod/2407) | 1.4.5 | 게임 | - | 2 | `stage2-A` |
| [WTT - CommonLib](https://sp-mod.com/mod/2310) | 3.0.6 | 게임+서버 | - | 6 | `stage2-B1-CommonLib` |
| [WTT- Armory](https://sp-mod.com/mod/2246) | 3.0.0 | 게임+서버 | **결속** | 954 | `stage2-B2-Armory` |
| [WTT- Artem](https://sp-mod.com/mod/1023) | 3.0.2 | 서버 | **결속** | 302 | `stage2-B3-Artem` |
| [Server Value Modifier \[SVM\]](https://sp-mod.com/mod/236) | 2.2.3 | 서버+루트 | **결속** | 20 | `stage2-C` |

## 3차 — 도구·커스텀 상인

| 모드 | 버전 | 위치 | 결속 | 파일 | 설치 기록 |
| --- | --- | --- | --- | --- | --- |
| [Skipper](https://sp-mod.com/mod/1343) | 1.1.5 | 게임 | - | 1 | `stage3-T1-Skipper` |
| [give-ui](https://sp-mod.com/mod/1617) | 5.0.1 | 서버 | - | 2 | `stage3-T3-giveui` |
| [Open Sesame](https://sp-mod.com/mod/1184) | 3.0.1 | 게임 | - (치트 표시) | 1 | `stage3-T2-OpenSesame` |
| [Lotus](https://sp-mod.com/mod/1232) | 1.8.1 | 서버 | **결속** | 266 | `stage3-L1-Lotus` |
| [Scorpion](https://sp-mod.com/mod/1348) | 1.1.1 | 서버 | **결속** | 22 | `stage3-L2-Scorpion` |
| [Ref - SPT Friendly Quests](https://sp-mod.com/mod/1538) | 2.1.0 | 서버 | **결속** | 4 | `stage3-L3-Ref` |
| [HarryHideout](https://sp-mod.com/mod/1303) | 3.1.2 | 서버 | **결속** | 6 | `stage3-L4-HarryHideout` |

## 4차 — 상인 목록 넘침 대응

| 모드 | 버전 | 위치 | 결속 | 파일 | 설치 기록 |
| --- | --- | --- | --- | --- | --- |
| [Kaeno-TraderScrolling](https://sp-mod.com/mod/1089) | 4.1.0 | 게임 | - | 1 | `stage4-U1-TraderScrolling` |
| [TraderSpace](https://sp-mod.com/mod/2694) | 2.0.0 | 게임 | - | 1 | `stage4-U2-TraderSpace` |

## 5차 — UI·편의(5-A)·총기(5-B)·탄약(5-C)·상인(5-D)·치트·도구(5-E)

| 모드 | 버전 | 위치 | 결속 | 파일 | 설치 기록 |
| --- | --- | --- | --- | --- | --- |
| [Use Items Anywhere](https://sp-mod.com/mod/2386) | 2.1.6 | 게임 | - | 4 | `stage5A-2386` |
| [AutoDeposit](https://sp-mod.com/mod/1469) | 6.0.0 | 게임 | - | 1 | `stage5A-1469` |
| [Task List Fixes](https://sp-mod.com/mod/824) | 1.8.0 | 게임 | - | 1 | `stage5A-824` |
| [Exfil Flare Notification](https://sp-mod.com/mod/1249) | 1.0.6 | 게임 | - | 2 | `stage5A-1249` |
| [Equip From Weapon Rack](https://sp-mod.com/mod/1136) | 1.6.0 | 게임 | - | 1 | `stage5A-1136` |
| [Loot Instant Search](https://sp-mod.com/mod/2069) | 1.0.5 | 게임 | - | 1 | `stage5A-2069` |
| [LetMeRightClick](https://sp-mod.com/mod/2405) | 1.1.1 | 게임 | - | 2 | `stage5A-2405` |
| [Auto Corpse Search](https://sp-mod.com/mod/2730) | 2.2.0 | 게임 | - | 1 | `stage5A-2730` |
| [Discarder](https://sp-mod.com/mod/1745) | 5.0.0 | 게임 | - | 1 | `stage5A-1745` |
| [Advanced Stash Sorting](https://sp-mod.com/mod/2924) | 1.0.6 | 게임 | - | 4 | `stage5A-2924` |
| [Stash Filter](https://sp-mod.com/mod/2907) | 1.0.0 | 게임 | - | 1 | `stage5A-2907` |
| [Linked Search In Stash](https://sp-mod.com/mod/2731) | 2.0.1 | 게임 | - | 1 | `stage5A-2731` |
| [Raid Menu Cleanup Fix](https://sp-mod.com/mod/2733) | 2.0.0 | 게임 | - | 1 | `stage5A-2733` |
| [Advanced Modding Lights](https://sp-mod.com/mod/3081) | 2.0.0 | 게임 | - | 1 | `stage5A-3081` |
| [Notification Filter](https://sp-mod.com/mod/2729) | 2.0.0 | 게임 | - | 1 | `stage5A-2729` |
| [Tag Over Name](https://sp-mod.com/mod/3079) | 1.0.2 | 게임 | - | 1 | `stage5A-3079` |
| [Hide Locked Flea](https://sp-mod.com/mod/3152) | 1.0.0 | 게임+서버 | - | 2 | `stage5A-3152` |
| [Refined Flea Offer List](https://sp-mod.com/mod/2623) | 1.2.0 | 게임 | - | 1 | `stage5A-2623` |
| [Hj's Task Search](https://sp-mod.com/mod/2909) | 2.0.2 | 게임 | - | 1 | `stage5A-2909` |
| [Pinned Quest HUD](https://sp-mod.com/mod/3155) | 1.0.1 | 게임 | - | 3 | `stage5A-3155` |
| [Pre-Raid Checklist](https://sp-mod.com/mod/3160) | 1.0.0 | 게임 | - | 3 | `stage5A-3160` |
| [Picture in Picture Disabler](https://sp-mod.com/mod/2667) | 2.1.0 | 게임 | - | 4 | `stage5B-2667` |
| [Scope Rangefinder](https://sp-mod.com/mod/2735) | 3.5.0 | 게임 | - | 32 | `stage5B-2735` |
| [Auto Ranging](https://sp-mod.com/mod/2786) | 2.0.0 | 게임 | - | 1 | `stage5B-2786` |
| [NVG Sight Dimmer](https://sp-mod.com/mod/2789) | 2.1.0 | 게임 | - | 1 | `stage5B-2789` |
| [Easy Mounting](https://sp-mod.com/mod/2790) | 2.0.1 | 게임+서버 | - | 4 | `stage5B-2790` |
| [Transparent Sights](https://sp-mod.com/mod/2774) | 0.2.3 | 게임 | - | 1 | `stage5B-2774` |
| [Default Fire Mode Fixer](https://sp-mod.com/mod/1416) | 2.1.0 | 서버 | - | 2 | `stage5B-1416` |
| [Flexible Builds](https://sp-mod.com/mod/2646) | 1.2.2 | 게임+서버 | - | 2 | `stage5B-2646` |
| [Better Thermal & Night Vision](https://sp-mod.com/mod/2531) | 2.1.1 | 게임 | - | 3 | `stage5B-2531` |
| [Magazine Check Interrupt](https://sp-mod.com/mod/2643) | 1.1.1 | 게임 | - | 4 | `stage5C-2643` |
| [Direct Reload & Painless Meds](https://sp-mod.com/mod/3074) | 1.3.2 | 게임+서버 | - | 3 | `stage5C-3074` |
| [Low Ammo Cue](https://sp-mod.com/mod/3046) | 1.0.0 | 게임 | - | 1 | `stage5C-3046` |
| [BarterItemsStacks](https://sp-mod.com/mod/2480) | 1.4.3 | 게임+서버 | - | 5 | `stage5C-2480` |
| [Cobra's Ammo Stack](https://sp-mod.com/mod/2840) | 1.1.0 | - | **결속** | 0 | **미설치** |
| [Caliber Split Storage Solutions](https://sp-mod.com/mod/2990) | 1.0.1 | 서버 | **결속** | 12 | `stage5C-2990` |
| [SPT Trader Restock Refresh](https://sp-mod.com/mod/3098) | 1.0.0 | 게임+서버 | - | 4 | `stage5D-3098` |
| [Trader Level Requirements Editor](https://sp-mod.com/mod/3005) | 1.1.0 | 서버 | - | 2 | `stage5D-3005` |
| [Trader Namer](https://sp-mod.com/mod/1527) | 4.1.0 | 서버 | - | 4 | `stage5D-1527.dirs`, `stage5D-1527` |
| [Ultimate Trader Assort Editor](https://sp-mod.com/mod/2834) | 2.1.1 | 게임+서버 | - | 2 | `stage5D-2834` |
| [Gunsmith Task Trader](https://sp-mod.com/mod/3124) | 1.0.2 | 서버 | **결속** | 7 | `stage5D-3124` |
| [Gunwright](https://sp-mod.com/mod/3128) | 1.0.2 | 서버 | **결속** | 5 | `stage5D-3128` |
| [CaseTrader — Buy any case](https://sp-mod.com/mod/3024) | 1.0.1 | 서버 | **결속** | 7 | `stage5D-3024` |
| [PresetTrader](https://sp-mod.com/mod/2966) | 1.2.0 | 서버 | **결속** | 9 | `stage5D-2966` |
| [Ledger](https://sp-mod.com/mod/3085) | 1.1.0 | 서버 | **결속** | 10 | `stage5D-3085` |
| [Amonya - Ammo Loving Trader & Quester](https://sp-mod.com/mod/2419) | 2.2.0 | 서버 | **결속** | 280 | `stage5D-2419.dirs`, `stage5D-2419` |
| [Freecam](https://sp-mod.com/mod/164) | 1.4.8 | 게임 | - | 1 | `stage5E-164` |
| [Cobra's SPT Explorer](https://sp-mod.com/mod/3009) | 1.0.0 | 게임+서버+루트 | - (치트 표시) | 20 | `stage5E-3009` |
| [Set Speed - Set Player Speed with Hotkeys](https://sp-mod.com/mod/994) | 1.5.0 | 게임 | - | 1 | `stage5E-994` |
| [Fast Grenade Throw](https://sp-mod.com/mod/2783) | 2.0.0 | 게임 | - | 1 | `stage5E-2783` |
| [Low Budget Bullet Time NG](https://sp-mod.com/mod/1988) | 2.3.0 | 게임 | - | 3 | `stage5E-1988` |
| [Tripwire Improvement](https://sp-mod.com/mod/2618) | 1.1.0 | 게임 | - | 1 | `stage5E-2618` |

## 6차 — 상인·파츠 (6-A 10 + 6-B 결속 2 + 6-C 부착물 UI 3 + 6-D 초상화 1)

| 모드 | 버전 | 위치 | 결속 | 파일 | 설치 기록 |
| --- | --- | --- | --- | --- | --- |
| [UnityToolkit](https://sp-mod.com/mod/1426) | 2.0.2 | 게임 | - | 17 | `stage6C-1426` |
| [Tylevo's Field Attachments](https://sp-mod.com/mod/3125) | 1.2.0 | 게임 | **결속** | 240 | `stage6C-3125` |
| [Scrollable Attachments](https://sp-mod.com/mod/2677) | 2.0.0 | 게임 | - | 1 | `stage6C-2677` |
| [VanillaTradersRework-Reloaded (Nexus)](https://www.nexusmods.com/escapefromtarkov/mods/10) | 1.0.0 | 서버 | - | 19 | `stage6D-vanillatradersrework` |
| [BrightLasers](https://sp-mod.com/mod/1358) | 3.0.0 | 게임+서버 | - | 4 | `stage6A-1358` |
| [No Barrel Shadow](https://sp-mod.com/mod/2780) | 2.0.1 | 게임 | **결속** | 2 | `stage6A-2780` |
| [Vanguard](https://sp-mod.com/mod/2917) | 2.0.0 | 서버 | **결속** | 10 | `stage6A-2917` |
| [OpenBarters](https://sp-mod.com/mod/3194) | 1.0.1 | 게임 | - | 1 | `stage6A-3194` |
| [Moe-TradeMarker](https://sp-mod.com/mod/2736) | 1.2.6 | 게임+서버 | **결속** | 9 | `stage6A-2736` |
| [Ref Rework](https://sp-mod.com/mod/3014) | 1.0.0 | 게임+서버 | - | 4 | `stage6A-3014` |
| [Realistic Insurance](https://sp-mod.com/mod/2949) | 0.2.1 | 게임+서버 | - | 5 | `stage6A-2949` |
| [All Weapons Flea](https://sp-mod.com/mod/3200) | 1.0.1 | 서버 | - | 3 | `stage6A-3200` |
| [NATO Quartermaster — Western Gear Trader, Presets & Quest Unlocks](https://sp-mod.com/mod/3030) | 1.1.3 | 서버 | **결속** | 4 | `stage6B-3030` |
| [ECOTI - Enhanced Clip-On Thermal Imager](https://sp-mod.com/mod/2930) | 3.4.2 | 게임+서버 | **결속** | 71 | `stage6B-2930` |
| [PortableTraders (GitHub)](https://github.com/wangbofan/SPT-PortableTraders) | 1.1.0 | 서버 | - | 2 | `stage6A-portabletraders` |
| [Quick Trader Load Times (GitHub)](https://github.com/JoelHauser/QuickTraderLoadTimes) | 1.0.0 | 게임+서버 | - | 2 | `stage6A-quicktraderloadtimes` |

## 7차 — 방어구·탄환·파츠·상인 (7-A 13 + 7-B 3 + 7-C 26 + 7-D 12 + 7-E 대형 3)

| 모드 | 버전 | 위치 | 결속 | 파일 | 설치 기록 |
| --- | --- | --- | --- | --- | --- |
| [Quick Sell](https://sp-mod.com/mod/2955) | 4.3.0 | 게임+서버 | - | 3 | `stage7B-2955` |
| [Bela.s.Speedloader](https://sp-mod.com/mod/2458) | 1.2.4 | 서버 | - | 4 | `stage7B-2458` |
| [Infinite Everything](https://sp-mod.com/mod/3108) | 2.5.3 | 게임+서버 | - | 4 | `stage7B-3108` |
| [Less Restricting Headwear](https://sp-mod.com/mod/922) | 4.0.0 | 서버 | **결속** | 4 | `stage7C-922` |
| [BETTER ATTACHMENTS](https://sp-mod.com/mod/1939) | 4.1.5 | 서버 | **결속** | 4 | `stage7C-1939` |
| [Definitive Weapon Variants](https://sp-mod.com/mod/2010) | 4.3.2 | 서버 | **결속** | 34 | `stage7C-2010` |
| [Canted Aiming](https://sp-mod.com/mod/2415) | 2.0.0 | 서버 | **결속** | 4 | `stage7C-2415` |
| [Holster Everything](https://sp-mod.com/mod/2650) | 2.0.0 | 게임+서버 | **결속** | 7 | `stage7C-2650` |
| [Black Exfil Revision](https://sp-mod.com/mod/2380) | 1.1.0 | 서버 | **결속** | 1 | `stage7C-2380` |
| [Jero Backpack](https://sp-mod.com/mod/2369) | 4.1.1 | 서버 | **결속** | 4 | `stage7C-2369` |
| [Restore Armor Plates](https://sp-mod.com/mod/2973) | 1.0.2 | 게임+서버 | **결속** | 4 | `stage7C-2973` |
| [Repair Skill Extension](https://sp-mod.com/mod/2556) | 1.4.1 | 서버 | **결속** | 2 | `stage7C-2556` |
| [Repair Max Durability](https://sp-mod.com/mod/1305) | 2.2.0 | 게임+서버 | **결속** | 3 | `stage7C-1305` |
| [Cobra's Durability Tweaks](https://sp-mod.com/mod/3019) | 1.0.0 | 서버 | **결속** | 5 | `stage7C-3019` |
| [Realistic Suppressors](https://sp-mod.com/mod/3171) | 1.0.0 | 서버 | **결속** | 3 | `stage7C-3171` |
| [Full Automatic Guns](https://sp-mod.com/mod/1972) | 4.1.5 | 서버 | **결속** | 5 | `stage7C-1972` |
| [ShrapnelFix](https://sp-mod.com/mod/3118) | 1.0.0 | 게임 | **결속** | 2 | `stage7C-3118` |
| [Airburst](https://sp-mod.com/mod/2948) | 1.1.2 | 게임+서버 | **결속** | 7 | `stage7C-2948` |
| [Magic Optic Mount](https://sp-mod.com/mod/3101) | 0.1.0 | 게임+서버 | **결속** | 7 | `stage7C-3101` |
| [500 Euro for Skier](https://sp-mod.com/mod/2434) | 2.0.0 | 서버 | **결속** | 1 | `stage7C-2434` |
| [Boxes At Ref (BARF) - NG](https://sp-mod.com/mod/2483) | 3.0.0 | 서버 | **결속** | 3 | `stage7C-2483` |
| [🛠️ Limitless – Unlimited or Multiply Buy Limits](https://sp-mod.com/mod/2175) | 1.0.0 | 서버 | **결속** | 6 | `stage7C-2175` |
| [Equipment Is Eternal](https://sp-mod.com/mod/2897) | 1.2.0 | 서버 | **결속** | 2 | `stage7C-2897` |
| [AmmoGen](https://sp-mod.com/mod/2777) | 2.4.0 | 게임+서버 | **결속** | 4 | `stage7C-2777` |
| [Slimmer Magazines](https://sp-mod.com/mod/2373) | 2.0.0 | 서버 | **결속** | 1 | `stage7C-2373` |
| [Ultimate Flea](https://sp-mod.com/mod/2844) | 1.2.0 | 게임+서버 | **결속** | 11 | `stage7C-2844` |
| [AllGoodsTrader](https://sp-mod.com/mod/2648) | 2.0.0 | 서버 | **결속** | 12 | `stage7C-2648` |
| [bluehead's AIO Trader](https://sp-mod.com/mod/374) | 5.3.1 | 서버 | **결속** | 10 | `stage7C-374` |
| [ALL AMMO TRADER - UPDATED BY TILTUSHKIN](https://sp-mod.com/mod/1554) | 1.5.0 | 서버 | **결속** | 5 | `stage7C-1554` |
| [BigBrain](https://sp-mod.com/mod/902) | 1.5.0 | 게임 | - | 1 | `stage7D-902` |
| [ItemGen](https://sp-mod.com/mod/2803) | 1.5.1 | 게임+서버 | **결속** | 4 | `stage7D-2803` |
| [Better OKP-7](https://sp-mod.com/mod/2711) | 1.1.0 | 서버 | **결속** | 6 | `stage7D-2711.dirs`, `stage7D-2711` |
| [TraderGen](https://sp-mod.com/mod/2749) | 2.4.1 | 게임+서버 | **결속** | 12 | `stage7D-2749` |
| [The Quartermaster](https://sp-mod.com/mod/2817) | 1.4.3 | - | **결속** | 0 | **미설치** |
| [Wolfik's Heavy Trooper Masks](https://sp-mod.com/mod/1569) | 1.2.0 | 서버 | **결속** | 13 | `stage7D-1569` |
| [Manimal's Ammo Loading Animations](https://sp-mod.com/mod/2681) | 2.0.0 | 게임+서버 | **결속** | 10 | `stage7D-2681` |
| [Sage's Incendiary Grenade](https://sp-mod.com/mod/3113) | 1.1.0 | 게임+서버 | **결속** | 9 | `stage7D-3113` |
| [.22lr Ammunition](https://sp-mod.com/mod/2516) | 2.0.0 | 서버 | **결속** | 24 | `stage7D-2516` |
| [Peacemaker Continued](https://sp-mod.com/mod/2414) | 2.1.0 | 서버 | **결속** | 18 | `stage7D-2414` |
| [Better Rear Sights](https://sp-mod.com/mod/1591) | 1.8.0 | 서버 | **결속** | 43 | `stage7D-1591` |
| [Fold That Stock](https://sp-mod.com/mod/2691) | 2.1.0 | 게임+서버 | **결속** | 25 | `stage7D-2691` |
| [WTT - Pack 'n' Strap](https://sp-mod.com/mod/1278) | 2.1.1 | 게임+서버 | **결속** | 60 | `stage7E-1278` |
| [Kalashnikov Enhanced Modding ](https://sp-mod.com/mod/3010) | 4.1.2 | - | **결속** | 0 | **미설치** |
| [Tactical Gear Component](https://sp-mod.com/mod/1125) | 3.0.0 | 서버 | **결속** | 137 | `stage7E-1125` |
| [Painter](https://sp-mod.com/mod/1025) | 3.0.0 | 서버 | - | 27 | `stage7E-1025` |
| [LMSP - Let Me Switch Plates](https://sp-mod.com/mod/2814) | 1.0.1 | 게임 | - | 1 | `stage7A-2814` |
| [Unrestricted Armor Plate](https://sp-mod.com/mod/1878) | 2.1.0 | 서버 | - | 3 | `stage7A-1878` |
| [Armor Expert](https://sp-mod.com/mod/3106) | 3.1.0 | 게임 | - | 1 | `stage7A-3106` |
| [Munitions Expert - Reloaded](https://sp-mod.com/mod/3006) | 3.1.0 | 게임 | - | 1 | `stage7A-3006` |
| [Foldables](https://sp-mod.com/mod/2422) | 1.1.1 | 게임+서버 | - | 8 | `stage7A-2422` |
| [VisorEffectManager](https://sp-mod.com/mod/2429) | 1.1.0 | 게임 | - | 1 | `stage7A-2429` |
| [Change Helmet Visor](https://sp-mod.com/mod/2554) | 3.0.0 | 게임+서버 | - | 5 | `stage7A-2554` |
| [BiC - Bind in Container](https://sp-mod.com/mod/3190) | 1.0.1 | 게임 | - | 1 | `stage7A-3190` |
| [Manimal's Quick Cancel](https://sp-mod.com/mod/2969) | 2.0.0 | 게임 | - | 1 | `stage7A-2969` |
| [Quick Aim](https://sp-mod.com/mod/2985) | 1.0.0 | 게임 | - | 1 | `stage7A-2985` |
| [All In Weapon: Unslotted](https://sp-mod.com/mod/2576) | 1.2.0 | 서버 | - | 3 | `stage7A-2576` |
| [Item Limits Begone!](https://sp-mod.com/mod/2524) | 1.1.0 | 서버 | - | 2 | `stage7A-2524` |
| [SOMF - SitOnMyFace](https://sp-mod.com/mod/3105) | 0.1.2 | - | - | 0 | **미설치** |
| [ArmorInfoPlus](https://sp-mod.com/mod/3049) | 1.0.0 | 게임 | - | 2 | `stage7A-3049` |

## 8차 — OP·AI 동료·HUD 포팅본 (8-A 5 + 8-B 3 + 8-C 3 + 8-D 치트 2 + 8-E AI 동료 3 + 8-F 대형 1)

| 모드 | 버전 | 위치 | 결속 | 파일 | 설치 기록 |
| --- | --- | --- | --- | --- | --- |
| [Dev Balaclava Fix](https://sp-mod.com/mod/1937) | 1.2.0 | 게임 | - | 2 | `stage8A-1937` |
| [Headshot Damage Redirection](https://sp-mod.com/mod/2953) | 1.8.0 | 게임 | - | 1 | `stage8A-2953` |
| [MeleeMultiplier](https://sp-mod.com/mod/2763) | 0.0.4 | 서버 | - | 2 | `stage8A-2763` |
| [The Blacklist - flea market enhancements](https://sp-mod.com/mod/755) | 3.1.1 | 서버 | - | 5 | `stage8A-755` |
| [Fujiwara Mokou (Touhou)](https://sp-mod.com/mod/1796) | 1.2.0 | 서버 | - | 8 | `stage8A-1796` |
| [Recoil Rework (Legacy)](https://sp-mod.com/mod/2190) | 2.0.0 | 게임+서버 | - | 13 | `stage8B-2190` |
| [Instant Insurance](https://sp-mod.com/mod/2394) | 1.1.2 | - | - | 0 | **미설치** |
| [Shorter Magazines](https://sp-mod.com/mod/3186) | 1.1.0 | - | **결속** | 0 | **미설치** |
| [Death Shadow Tweaks](https://sp-mod.com/mod/2943) | 1.0.0 | - | **결속** | 0 | **미설치** |
| [Olympus](https://sp-mod.com/mod/505) | 4.1.3 | 서버 | **결속** | 4 | `stage8C-505` |
| [SIM - Simple Item Modifier](https://sp-mod.com/mod/2906) | 0.1.0 | 서버+루트 | **결속** | 35 | `stage8C-2906` |
| [MagicBullet](https://sp-mod.com/mod/3065) | 1.0.0 | 게임 | - (치트 표시) | 1 | `stage8D-3065` |
| [Tushonka-800: Combat Augmentation System](https://sp-mod.com/mod/3169) | 1.0.1 | 게임 | - (치트 표시) | 1 | `stage8D-3169` |
| [Waypoints - Expanded Navmesh](https://sp-mod.com/mod/827) | 1.9.0 | 게임 | - | 13 | `stage8E-827` |
| [Miyako Carry Service](https://sp-mod.com/mod/2709) | 1.1.4 | - | - | 0 | **미설치** |
| [PIT Fireteam](https://sp-mod.com/mod/2676) | 1.0.0 | 게임+서버 | - | 26 | `stage8E-2676` |
| [Make Shotguns Great Again!](https://sp-mod.com/mod/1453) | 1.19.1 | 게임+서버 | **결속** | 195 | `stage8F-1453` |
| [Game Panel HUD (SPT 4.1.6 포팅본, KmyTarkovApi 2.0.0 포함)](https://arca.live/b/tarkovspt/185302366) | 4.1.6-port | 게임 | - | 30 | `stage8G-gamepanelhud` |

## 절차 밖에서 들어온 것 (설치 기록을 뒤늦게 복원 — 당시 기록이 아니다)

| 모드 | 버전 | 위치 | 결속 | 파일 | 설치 기록 |
| --- | --- | --- | --- | --- | --- |
| [More Energy Drinks](https://sp-mod.com/mod/1688) | 1.3.2 | 서버 | **결속** | 87 | `stage9-1688` |
| [MoreBotsAPI](https://sp-mod.com/mod/2426) | 2.1.1 | 게임+서버 | - | 4 | `stage9-2426` |
| [WTT - Content Backport](https://sp-mod.com/mod/2512) | 2.0.5 | 게임+서버 | **결속** | 1637 | `stage9-2512` |
| [SAIN - Solarint's AI Modifications](https://sp-mod.com/mod/791) | 4.5.1 | 게임+서버 | - | 11 | `stage9-791` |
| [ISB Aishi](https://sp-mod.com/mod/2478) | 2.0.4 | 게임+서버 | **결속** | 3659 | `stage9-2478` |
| [ISB Special Forces](https://sp-mod.com/mod/2485) | 2.0.1 | 게임+서버 | - | 1584 | `stage9-2485` |
| [GF2 Skin Pack](https://sp-mod.com/mod/3156) | 1.0.0 | 서버 | - | 58 | `stage9-3156` |
| [WTT - Head and Voice Selector](https://sp-mod.com/mod/1277) | 1.1.0 | 게임+서버 | - | 4 | `stage9-1277` |

## 다른 경로로 설치된 것

| 항목 | 위치 | 설치 주체 |
| --- | --- | --- |
| SptKoreanConfig 0.1.0 (F12 설정 창 한글화) | `BepInEx\plugins\SptKoreanConfig\` (DLL 1 + 번역 사전 105) | 이 저장소에서 직접 만든 것. 소스는 [../src/SptKoreanConfig](../src/SptKoreanConfig), 설치는 `tools/install-koreanconfig.ps1`. Forge 모드가 아니라 설치 기록에 없다 |
| HelmetSlotUnion 0.1.0 (헬멧 부착 칸 합집합) | `SPT_Runtime\user\mods\HelmetSlotUnion\` (DLL 1 + `config.jsonc`) | 이 저장소에서 직접 만든 것. 소스는 [../src/HelmetSlotUnion](../src/HelmetSlotUnion), 설치는 `tools/install-helmetslotunion.ps1`. Forge 모드가 아니라 설치 기록에 없다 |
| SlotCategoryRelax 0.1.0 (분류별 칸 열기) | `SPT_Runtime\user\mods\SlotCategoryRelax\` (DLL 1 + `config.jsonc`) | 이 저장소에서 직접 만든 것. 소스는 [../src/SlotCategoryRelax](../src/SlotCategoryRelax), 설치는 `tools/install-slotcategoryrelax.ps1`. 부품 분류를 골라 그 분류만 칸 필터를 넓힌다 — 조준경은 열고 핸드가드는 닫는 식. 고를 수 있는 분류는 서버 시작 때 DB 에서 찾아 `discovered-categories.json` 으로 내놓는다. Forge 모드가 아니라 설치 기록에 없다 |
| StackUnify 0.1.0 (스택 크기 통합) | `SPT_Runtime\user\mods\StackUnify\` (DLL 1 + `config.jsonc`) | 이 저장소에서 직접 만든 것. 소스는 [../src/StackUnify](../src/StackUnify), 설치는 `tools/install-stackunify.ps1`. 최대 스택 크기를 쓰는 서버 모드가 13개나 되어 어느 값이 남는지 예측할 수 없었다. 클라이언트 목록이 굳기 전의 가장 늦은 자리(`OnLoadOrder.PostLoad`)에서 설정에 적힌 값으로 못박는다. 더 늦은 자리는 DB 만 바뀌고 클라이언트에 닿지 않는다(실측). 기본 설정은 비어 있어 아무 일도 하지 않는다. Forge 모드가 아니라 설치 기록에 없다 |
| ScavRaidWaves 0.1.0 (스캐브 레이드 PMC 웨이브) | `SPT_Runtime\user\mods\ScavRaidWaves\` (DLL 1 + `config.jsonc`) | 이 저장소에서 직접 만든 것. 소스는 [../src/ScavRaidWaves](../src/ScavRaidWaves), 설치·제거는 `tools/install-scavraidwaves.ps1` (`-Remove`). 스캐브로 들어가면 PMC 가 안 나오던 것을 고친다. PMC 웨이브는 레이드 시작 30~1800초에 몰려 있는데, 스캐브 레이드는 `scavRaidTimeSettings` 때문에 시간이 40~70% 깎인 채 중간에 끼어드는 것으로 처리돼 그 웨이브를 통째로 놓친다. `LocationConfig` 의 `ReducedChancePercent`·`AdjustWaves` 만 설정대로 덮어쓴다. 기본값은 모든 맵의 `AdjustWaves` 를 false 로 두는 것 — 레이드는 짧은 채로 두고 그 길이 안에 드는 PMC 웨이브만 살린다. PMC 가 모자라면 `ReducedChancePercent` 를 0 으로 바꿔 시간 삭감 자체를 끈다. Forge 모드가 아니라 설치 기록에 없다 |
| AirdropCaller 0.1.0 (보급 호출기 — 외부 창) | `AirdropCaller.exe` + `airdrop-caller.config.json` (설치본 루트, `Give Ui.exe` 옆) | 이 저장소에서 직접 만든 것. 소스는 [../src/AirdropCaller.App](../src/AirdropCaller.App), 설치·제거는 `tools/install-airdropcaller-app.ps1` (`-Remove`). WPF 창. 프리셋을 짜고 레이드 중에 버튼으로 보급을 부른다. 아이템은 **한국어 이름으로 검색**한다. 인증은 게임 클라이언트와 같은 `Cookie: PHPSESSID=<프로필 id>` 를 쓰고, 그 id 는 `user\profiles` 의 파일 이름에서 읽는다. .NET 10 데스크톱 런타임이 필요하다(런타임 의존 게시, 222KB). 서버 모드·게임 플러그인과 셋이 한 벌이다. Forge 모드가 아니라 설치 기록에 없다 |
| AirdropCaller.Plugin 0.1.0 (보급 호출기 — 게임) | `BepInEx\plugins\AirdropCaller\` (DLL 1) | 이 저장소에서 직접 만든 것. 소스는 [../src/AirdropCaller.Plugin](../src/AirdropCaller.Plugin), 설치·제거는 `tools/install-airdropcaller-plugin.ps1` (`-Remove`). 레이드 상태를 서버에 2초마다 보고하고, 서버에 예약된 드롭이 있으면 꺼내 와 `ClientGameWorld.InitAirdrop` 을 부른다 — 공개 메서드 하나라 Harmony 패치가 없다. 서버 쪽 `AirdropCaller.Server` 와 짝이다. Forge 모드가 아니라 설치 기록에 없다 |
| AirdropCaller.Server 0.1.0 (보급 호출기 — 서버) | `SPT_Runtime\user\mods\AirdropCaller\` (DLL 1 + `config.jsonc`) | 이 저장소에서 직접 만든 것. 소스는 [../src/AirdropCaller.Server](../src/AirdropCaller.Server), 설치·제거는 `tools/install-airdropcaller-server.ps1` (`-Remove`). 바닐라의 보급 전리품 라우트(`POST /client/airdrop/loot`)에 **뒤에서** 끼어들어, `config.jsonc` 의 `Drops` 에 적힌 상자 id 에만 우리가 정한 내용물로 답한다. 적혀 있지 않은 id 는 바닐라 답을 그대로 흘려보내므로 **바닐라 보급기와 플레어 보급은 건드리지 않는다**. 설계는 [specs/2026-10-28-airdrop-caller-design.md](specs/2026-10-28-airdrop-caller-design.md). Forge 모드가 아니라 설치 기록에 없다 |
| **AirdropProbe 0.1.0 (보급 지점 측정기) — 임시** | `BepInEx\plugins\AirdropProbe\` (DLL 1 + probe.log) | 이 저장소에서 직접 만든 **측정 전용** 도구. 소스는 [../src/AirdropProbe](../src/AirdropProbe), 설치·제거는 `tools/install-airdropprobe.ps1` (`-Remove`). 아무것도 바꾸지 않고, 레이드에서 Ctrl+Alt+A 를 누르면 보급 지점 개수와 거리를 적는다. 보급 호출기 설계의 [미확인 사항 9](specs/2026-10-29-airdrop-stage1-findings.md)를 재기 위한 것이고 **재고 나면 지운다**. Forge 모드가 아니라 설치 기록에 없다 |
| GridWindowScroll 0.1.0 (인벤토리 격자 스크롤) | `BepInEx\plugins\GridWindowScroll\` (DLL 1 + 상자별 크기 sizes.json) | 이 저장소에서 직접 만든 것. 소스는 [../src/GridWindowScroll](../src/GridWindowScroll), 설치는 `tools/install-gridwindowscroll.ps1`. 가방·상자를 열었을 때 뜨는 내부 인벤 창이 화면을 넘어가지 않게, 보이는 칸 수를 가로·세로로 못박고 그보다 큰 격자는 스크롤바와 마우스 휠로 넘겨 본다. 칸 수는 창 오른쪽 아래 손잡이를 끌어 정하거나 F12 에서 정하고, 끌어 정한 값은 상자 종류마다 기억한다. Forge 모드가 아니라 설치 기록에 없다 |
| GogglesWheel 0.1.0 (야시경·실드 휠) | `BepInEx\plugins\GogglesWheel\` (DLL 1 + `config.jsonc`) | 이 저장소에서 직접 만든 것. 소스는 [../src/GogglesWheel](../src/GogglesWheel), 설치·제거는 `tools/install-goggleswheel.ps1` (`-Remove`). 헤드기어 제한을 푸는 모드를 넣으면 헬멧에 야시경과 페이스실드가 동시에 달리는데, 게임의 `ToggleGoggles` 가 여닫이 중 **첫 번째 하나만** 건드려 한쪽을 영영 못 열었다. 짧게 누르면 야시경, 길게 누르면 원형 메뉴에서 고른다. 같은 메서드를 가로채는 UI Fixes(나침반)·COTI(수정키)와 부딪히지 않게 물러나는 조건을 둔다. 설계는 [specs/2026-10-30-goggles-wheel-design.md](specs/2026-10-30-goggles-wheel-design.md). Forge 모드가 아니라 설치 기록에 없다 |
| DynamicMapsFactionMarkers 0.1.0 (모드 세력 지도 표시) | `BepInEx\plugins\DynamicMapsFactionMarkers\` (DLL 1 + `config.jsonc` + `diamond.png`) | 이 저장소에서 직접 만든 것. 소스는 [../src/DynamicMapsFactionMarkers](../src/DynamicMapsFactionMarkers), 설치·제거는 `tools/install-dynamicmapsfactionmarkers.ps1` (`-Remove`). Dynamic Maps 가 ISB 같은 모드 세력을 전부 일반 스캐브(주황 화살표)로 그리던 것을 역할 묶음마다 다른 색·모양으로 가른다 — 그 모드의 보스 판정이 enum 숫자가 박힌 목록이라 13701~13716 이 들어갈 자리가 없었다(실측은 [specs/2026-10-30-faction-identification-survey.md](specs/2026-10-30-faction-identification-survey.md)). Dynamic Maps 파일은 고치지 않고 Harmony 로만 끼어들며, 그 모드 내부가 바뀌면 조용히 비활성으로 떨어진다. 설계는 [specs/2026-10-30-dynamicmaps-faction-markers-design.md](specs/2026-10-30-dynamicmaps-faction-markers-design.md). Forge 모드가 아니라 설치 기록에 없다 |
| WebConfigBridge 0.1.0 (모드 페이지) | `SPT_Runtime\user\mods\WebConfigBridge\` (DLL 1) | 이 저장소에서 직접 만든 것. 소스는 [../src/WebConfigBridge](../src/WebConfigBridge), 설치는 `tools/install-webconfigbridge.ps1`. 서버 웹 페이지 첫 화면의 **모드 페이지** 구역에 카드를 만들고(`/webconfigbridge`), 거기서 `All In Weapon: Unslotted` 의 무기·부품·탄약 호환 설정을 스위치로 고친다. 저장은 즉시, 적용은 다음 서버 시작부터. Forge 모드가 아니라 설치 기록에 없다 |
| SPT Team Manager 1.0.0 (프로필 팀원) | `BepInEx\plugins\SptTeamManager\` (DLL 1 + 에셋 17) + `SPT_Runtime\user\mods\SptTeamManager-ServerMod\` (DLL 2 + `deps.json` + `Resources\`) | **PIT Fireteam 1.0.0([2676](https://sp-mod.com/mod/2676/pit-fireteam), 기준 커밋 `f32a41b`)의 포크**다. 소스는 [../src/SptTeamManager](../src/SptTeamManager)(히스토리째 git subtree), 설치·제거는 `tools/install-sptteammanager.ps1` (`-Remove`). **PIT 와 함께 둘 수 없어 PIT 를 빼고 설치했다**(같은 `/singleplayer/pitfireteam/*` 라우트 접두를 쓴다 — 스크립트가 감지해 거부한다). 빼낸 PIT 는 `backup/2026-11-01-before-stm/`. PIT 의 용병 기능은 그대로 두고, SPT 런처의 **다른 프로필**(로그인한 프로필은 제외)을 팀원으로 등록하는 것을 더한다. 계획 1(기반)까지 된 것: 프로필 열거·가용성 판정, 등록 저장, 친구 목록 병합, 명단 화면. **아직 안 되는 것**: 프로필 팀원을 파티에 초대해 레이드에 데려가는 것(초대를 눌러도 아무 일도 일어나지 않는다), 장비 편집, 레이드 후 경험치·전리품 반영 — 계획 2·3 의 몫이다. 설계는 [specs/2026-10-31-spt-team-manager-design.md](specs/2026-10-31-spt-team-manager-design.md), 결과와 남은 확인 항목은 [WORKLOG](WORKLOG.md). 컴파일 기준은 이 설치본의 4.1.6(업스트림은 4.1.0) |
| CompatToggle 0.1.0 (호환성 토글) | **설치 안 함** | 이 저장소에서 직접 만든 것. 소스는 [../src/CompatToggle](../src/CompatToggle). `All In Weapon: Unslotted`를 F12 토글로 대체하려 했으나 개조 화면의 부품 목록을 채우지 못해 철회했다. 경위는 [WORKLOG](WORKLOG.md) 참고 |
| HideSpecialIconGrids 1.1.0.1 | `BepInEx\plugins\HideSpecialIconGrids.dll`, `BepInEx\config\com.pandahhcorp.hidespecialicongrids.cfg` | SVM `Greed.exe`가 프리셋 `CSM.EnableCSM: true` 적용 시 설치 (15:01). 설치 기록에 없음 |
| SVM 프리셋 `Noname` | `SPT_Runtime\user\mods\[SVM] Server Value Modifier\Presets\Noname.json` | 사용자가 `Greed.exe`로 생성. 16:00에 `Hideout.RemoveConstructionsRequirements`만 false로 수정 |
