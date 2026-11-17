# 설치된 모드 목록

> 자동 생성: `node tools/gen-installed-mods.mjs` (설치 기록·매니페스트 기반). 직접 고치지 말고 기록을 고친 뒤 다시 생성.

- 대상: `E:\Spt` (SPT 4.1.6, EFT 0.16.9.5.40743)
- 설치된 모드: **310개**, 설치 파일 **16196개** (자동 생성 설정 파일 포함)
- **현재 로드 수: 서버 모드 180개 — 2026-11-16 실측이다**(`ModLoader: 총 180 개`). 177 → 178 **2687 Sicc Case Fix** → 179 **3332 Prestige Perks**(둘 다 설치 테스트에서 오류 `Could not add` 고유 48 · bundle 1 · quest 1 로 기준선과 같았다) → 180 이 저장소의 **PrestigeTransferKeep**(3332 가 먹던 전송 물건을 되돌린다). **게임 쪽 플러그인은 251개가 마지막 실측(2026-11-16)이고, 그 뒤 3332 가 `PrestigePerks.Client.dll` 을 넣었으나 아직 기동으로 세지 않았다** — 플러그인 속성이 있으면 252가 된다(`plugin-count-is-not-dll-count`). 아래 251 까지의 내력은 그 직전 실측이다.
- **서버 모드 177개 / 게임 쪽 플러그인 251개 — 둘 다 2026-11-16 실측이었다.** 서버는 `ModLoader: 총 177 개`, 게임은 `251 plugins to load` = `Loading [` 251줄이고 **진짜 거부는 0**이다(`skipping` 이 들어간 3줄은 TraderGen·AmmoGen·ItemGen 이 "팩 폴더가 없다"고 적은 안내일 뿐 로드 실패가 아니다). 175 에서 177 이 된 것은 **ArmorTweaks**(12차-A, 11-15 11:36 설치)와 이 저장소에서 만든 **PrestigeStashRelax**(11-15) 둘이다. 플러그인이 248 에서 251 로 는 셋 가운데 하나는 이 저장소의 **PrestigeStashRelaxClient** 이고(기동 로그에 `Loading [프레스티지 이전 창고 풀기 0.1.0]` 으로 이름이 찍힌다), **나머지 둘은 아직 어느 모드 몫인지 가리지 않았다** — 12차 설치와 11-15 업데이트 18건 사이에서 온 것으로 보이나 추측으로 적지 않는다. 아래는 그 앞의 내력이다. **서버 175개 / 플러그인 248개는 2026-11-14 실측이었다.** 그날 기동은 `ModLoader: 총 175 개` 와 `Loading [` 248줄(거부 0)이다. 174 에서 175 가 된 것은 이 저장소에서 만든 `SlotFilterSweep`(2026-11-12 설치) 하나이고, 246 에서 248 이 된 것은 같은 저장소의 `IcebreakerOffMapGuard`(11-12)·`GridWindowScroll`(11-13) 둘이다 — 셋 다 기동 로그에서 이름으로 확인했다. 아래는 그 앞의 내력이다. 245 에서 246 이 된 것은 Quest Codex 1.12.0 이 들고 온 클라이언트 플러그인 `QuestCodex.Overlay` 하나다(업데이트 20개 가운데 플러그인 수를 바꾼 것은 이것뿐이다 — 경위는 `UPDATE-CHECK-2026-11-09.md`). 173 에서 174 가 된 것은 이 저장소에서 만든 `ProfilePorter.Server`(프로필 사이로 물건 베끼기, 2026-11-08 설치) 하나다 — 11-09 서버 로그의 기동 11회 **전부** `ModLoader: 총 174 개`, `불러옴` 174줄, 거부 0 이다. 그 전에 172 에서 173 이 된 것은 `SptKoreanLocale`(게임 DB locale 한글화) 이고, 173 은 10차 전체 완료(2026-11-06) 시점의 값이다. 같은 기동에서 서버 오류는 `Could not add` 48 · `Unable to add bundle` 1 · `cannot import quest` 1 로 기준선과 같다. **오래 떠 있는 기동에서는 같은 48줄이 되풀이된다** — 2026-11-09 의 마지막 기동(약 2시간)은 `Could not add` 가 144줄이었지만 **고유 id 는 48개로 같고**, 03:31·04:06·05:12(UTC)에 `ItemBaseClassService` 캐시가 다시 세워지며 세 번 찍힌 것이다. 줄 수가 아니라 **고유 id 수**로 본다. 다만 **고유 id 수도 고정값이 아니다** — 2026-11-11 실측 54개, 11-10 58개, 11-09 48개로 기동마다 다르다(그 기동에서 클라이언트가 무엇을 물어봤는지에 달렸다). 기준선으로 쓸 때는 **날짜를 함께** 적는다. 그리고 그 오류는 묶음으로 몰려 찍힌다 — 11-11 은 1,046줄이 12묶음(48·108·324줄)이었고, 묶음마다 `ItemBaseClassService` 가 캐시를 다시 세우려 든다(경위는 `MEMORY-AUDIT-2026-11-11.md`). 게임 플러그인은 서버 모드라 그대로다. 3022 와 2866 이 플러그인 속성을 가진 클라이언트 DLL 을 하나씩 넣어 243 에서 2 늘었다(2866 이 함께 넣은 `PerfectCullingRuntime.dll` 은 플러그인 속성이 없어 0 으로 센다 — 넣은 DLL 2개 가운데 1만 센 것이 맞았다). 마지막 기동에서 `245 plugins to load` = `Loading [` 245줄, `Chainloader startup complete`, `[Error` 1줄 / `[Fatal` 0줄(알려진 DirectReload+Fika 건뿐). **다만 Better BepInEx 가 F12 창을 자기 메뉴로 갈아끼우면서 그 창에 붙어 있던 패치를 떼어 냈다** — 수는 줄지 않았지만 **로드된 것과 일하는 것은 다르다**. 그중 **한글화는 2026-11-05 에 고쳤다**(`SptKoreanConfig` 에 `BetterBepInExPatch` 를 더해 새 메뉴가 설정을 모으는 지점에 같은 번역을 다시 건다 — 커밋 f325eaa·1ff99da·446e22d, 설치본 DLL 11-05 16:35). **남은 것은 ASBP 뿐이고, 사용자가 2026-11-06 에 새 메뉴를 쓰기로 했다** — ASBP 는 하는 일이 없다. 경위는 `STAGE10-RESULT.md` 의 10-J 절
- 레이드를 깊게 돈 기동(11-05 11:06)에서는 `[Error` 9줄이었고 **전부 기존 건**이다 — SAIN 6, 알려진 DirectReload 1, STM 의 LootPatrol 억제 예외가 두 줄로 찍힌 것 2. 메뉴까지만 돈 기동이 1줄인 것은 좋아진 것이 아니라 **아직 안 나온 것**이다
- 10차 서버 모드 변화: 115(기준선) → 116(10-A) → 120(10-B) → 122(10-C, 150초) → 127(10-D, 144초) → 136(10-E, 159초 / 10-E2, 129초) → **135**(All Quests Checkmarks 제거) → **137**(10-F, 135초) → **138**(10-F2 ORBIT, 144초) → **145**(10-G, 전리품·경제 11개 중 서버 7, 135초. 플리마켓 생성 단계에서도 오류 없음) → **150**(10-H, 맵 7개를 하나씩. 141~162초, 7회 전부 정상 시작) → **151**(10-I1 중 Blackout, 144초) → **152**(10-I2 중 FogSix) → **153**(10-J 중 Raid Review, 147초) → **154**(10-J 중 Knight Mask Fix, 141초) → **155**(10-K 중 PityLoot) → **156**(10-K 중 Lacy 의 PvE Tweaks) → **157**(10-K 중 Quest Backport) → **167**(10-L, 결속 콘텐츠·대형 11개를 하나씩. 11회 전부 정상 시작이고 11개 중 10개가 서버 모드를 싣는다 — Deploy Screen Overhaul 만 게임 전용). 10-K 는 결속 소형 6개를 **하나씩** 넣었고 6회 전부 정상 시작했다(게임 전용 3개는 수가 그대로다). 이어 **169**(10-M, 대형 3개를 하나씩. DynamicMaps Extended 는 게임 전용이라 167 유지, Labs Office Backport 168, Arena Map Port 169. 3회 전부 정상 시작) → **170**(10-O 중 Manimal Interchange Expansion Backport, 201초) → **171**(10-O 중 Manimal Lighthouse Rework Backport, 198초. 앞의 것을 인터체인지 레이드로 확인한 뒤에 넣었다 — 계획이 못 박은 대로 모드마다 멈춘다) → **172**(10-O 의 마지막 MANIMAL ICEBREAKER BACKPORT, 165초. **이것으로 10차가 끝났다**). 10-O 의 세 모드 모두 기록 줄 수가 설치 파일 수와 같거나 서버가 만든 파일만큼만 많았고, 오류는 세 번 다 48/1/1 로 늘지 않았다. Raid Review 만 자체 포트(7828 WS / 7829 HTTP)를 열고 나머지 웹 화면은 모두 6969 안이다. 제거는 10-F 테스트 출력이 직접 찍었다 — `(Added 2 mods: ClassicMovement, Increase Climb Height; Removed 1 mods: All Quests Checkmarks)`. 시작 시간의 약 100초는 `[ItemInfo]` 하나가 쓴다. QuestCodex 는 서버 포트 안에 `/questcodex` 를 붙인다(별도 포트가 아니다)
- 10차 게임 플러그인 변화: 147(기준선) → 151(10-B) → 173(10-C·10-D) → 181(10-E·10-E2) → **191**(10-F) → **205**(10-G·10-H, 12:05 기동에서 실측) → **222**(10-I, 14:26 기동에서 실측. 16개가 넣은 플러그인 DLL 19개 중 라이브러리 2개를 뺀 17개이고, HollywoodFX 는 혼자 Janky-HollywoodFX·Janky-HollywoodGraphics 둘을 등록한다) → **228**(10-J, 16:33 기동에서 실측). → **232**(10-K, 17:23 기동에서 실측. 클라이언트 DLL 4개 — IOF·Item Preview QoL·TraumaCore·Quest Backport.Client — 가 전부 플러그인이라 예상치와 정확히 맞았다) → **239**(10-L, 19:03 기동에서 실측. 클라이언트 DLL 9개 가운데 Skills Extended 의 `.Client.API`·`.Common` 이 **플러그인 속성이 없어 0 으로 세어져** 7 증가다 — `Loading [Skills` 가 1줄뿐인 것으로 확인했다) → **242**(10-M) → **243**(10-O 중 3025, 2026-11-06 04:44 기동에서 실측) → **245**(10-O 끝, 2026-11-06 07:14 기동에서 실측. 3022·2866 이 하나씩 더했다). **넣은 DLL 수와 같지 않다.** ① 플러그인 속성이 없는 **라이브러리**는 0 으로 센다 — 지금까지 3건(Effects Manager Framework, `BriefingBriefing.Core.dll`, `tarkin-ladders	arkin.ladders.shared.dll`). ② DLL 하나가 플러그인 여러 개를 등록할 수 있다 — ICM 이 그렇고, 그중 `Configuration Manager` 는 SPT 자체의 `plugins/spt/ConfigurationManager` 와 **GUID 가 겹쳐 하나가 조용히 버려진다**(F12 한글화는 2026-11-05 확인 결과 멀쩡하다). ③ 겹침 검사가 `게임+서버` 로 분류한 모드도 클라이언트 DLL 을 싣는다 — 예상치는 `BepInExplugins*.dll` 기록 줄로 세야 한다. ④ **`-Fika` 가 붙은 DLL 은 이름으로 세지 않는다** — 플러그인 속성이 있으면 센다(`LootNetFika.dll` 은 올라왔고, 10-E 의 `AllQuestsCheckmarks-Fika.dll` 은 속성이 없어 안 올라왔다). `FixPluginTypesSerialization` 의 `Excluding ... references Fika.Core` 경고는 **직렬화 수정에서 뺀다**는 뜻이고 로드 여부와 무관하다
- **All Quests Checkmarks(2025)는 10-E 에서 넣었다가 같은 날 뺐다.** 09:49 기동의 `[Error` 25줄 가운데 24줄이 이 모드 하나였다(체크 표시 패치 실패 12 + 모드가 추가한 퀘스트를 못 찾는 것 12). 한국어 locale 도 없다. 기록은 `install-records/removed/` 에 있다
- 기준선(2026-11-04)의 오류 내역: 게임 `[Error` 15줄 / `[Fatal` 0줄이고 전부 기존 건이다 — SAIN `sectactPriestEvent` 6, STM `AimingFailure` 6·`MedicalActivity` 1, ECOTI per-frame 1, Fika 미설치 관련 DirectReload 패치 1. **이 중 14줄은 레이드를 깊게 돌아야 나온다** — 메뉴까지만 돈 기동에서 오류가 1줄이라고 좋아진 것이 아니다. 서버는 `[Error]` 49줄 = `Could not add` 48건(원인 `MountOlympus.dll` — `REVIEW-LOAD-ORDER.md`) + `BundleLoader Unable to add bundle: ...barrel_vpo215_600mm_366tkm.bundle` 1건(번들 중복으로 보이며 시작은 정상 — `PLAN-STAGE10.md` §10-0-1)
- 테스트 콘솔 출력에는 `[Error]` 등급 표기가 없어 로그 파일의 오류 줄 수와 직접 비교되지 않는다 — 비교는 `Could not add`·`Unable to add bundle` 같은 문구 수로 한다
- 더 옛 값(게임 145 / 서버 116, 2026-10-29~30)과 다른 이유: 서버 116→115 는 PIT Fireteam 서버 모드 제거분, 게임 145→147 은 그 뒤 설치분이다
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
| [Compatibility Highlighter](https://sp-mod.com/mod/2859) | 1.3.2 | 게임 | - | 1 | `update-2859-1.3.2` |
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
| [WTT - CommonLib](https://sp-mod.com/mod/2310) | 3.0.7 | 게임+서버 | - | 6 | `update-2310-3.0.7` |
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
| [Stash Filter](https://sp-mod.com/mod/2907) | 1.1.0 | 게임 | - | 1 | `update-2907-1.1.0` |
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
| [Direct Reload & Painless Meds](https://sp-mod.com/mod/3074) | 1.4.1 | 게임+서버 | - | 3 | `update-3074-1.4.1` |
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
| [Amonya - Ammo Loving Trader & Quester](https://sp-mod.com/mod/2419) | 2.2.1 | 서버 | **결속** | 279 | `stage5D-2419`, `update-2419-2.2.1.dirs`, `update-2419-2.2.1` |
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
| [OpenBarters](https://sp-mod.com/mod/3194) | 1.1.3 | 게임 | - | 1 | `update-3194-1.1.3` |
| [Moe-TradeMarker](https://sp-mod.com/mod/2736) | 1.2.6 | 게임+서버 | **결속** | 9 | `stage6A-2736` |
| [Ref Rework](https://sp-mod.com/mod/3014) | 1.0.0 | 게임+서버 | - | 4 | `stage6A-3014` |
| [Realistic Insurance](https://sp-mod.com/mod/2949) | 0.2.3 | 게임+서버 | - | 3 | `update-2949-0.2.3` |
| [All Weapons Flea](https://sp-mod.com/mod/3200) | 1.0.1 | 서버 | - | 3 | `stage6A-3200` |
| [NATO Quartermaster — Western Gear Trader, Presets & Quest Unlocks](https://sp-mod.com/mod/3030) | 1.1.3 | 서버 | **결속** | 4 | `stage6B-3030` |
| [ECOTI - Enhanced Clip-On Thermal Imager](https://sp-mod.com/mod/2930) | 3.5.1 | 게임+서버 | **결속** | 56 | `update-2930-3.5.1` |
| [PortableTraders (GitHub)](https://github.com/wangbofan/SPT-PortableTraders) | 1.1.0 | 서버 | - | 2 | `stage6A-portabletraders` |
| [Quick Trader Load Times (GitHub)](https://github.com/JoelHauser/QuickTraderLoadTimes) | 1.0.0 | 게임+서버 | - | 2 | `stage6A-quicktraderloadtimes` |

## 7차 — 방어구·탄환·파츠·상인 (7-A 13 + 7-B 3 + 7-C 26 + 7-D 12 + 7-E 대형 3)

| 모드 | 버전 | 위치 | 결속 | 파일 | 설치 기록 |
| --- | --- | --- | --- | --- | --- |
| [Quick Sell](https://sp-mod.com/mod/2955) | 4.3.0 | 게임+서버 | - | 3 | `stage7B-2955` |
| [Bela.s.Speedloader](https://sp-mod.com/mod/2458) | 1.2.4 | 서버 | - | 4 | `stage7B-2458` |
| [Infinite Everything](https://sp-mod.com/mod/3108) | 2.5.3 | 게임+서버 | - | 4 | `stage7B-3108` |
| [Less Restricting Headwear](https://sp-mod.com/mod/922) | 4.0.0 | 서버 | - | 4 | `stage7C-922` |
| [BETTER ATTACHMENTS](https://sp-mod.com/mod/1939) | 4.1.5 | 서버 | **결속** | 4 | `stage7C-1939` |
| [Definitive Weapon Variants](https://sp-mod.com/mod/2010) | 5.0.1 | 게임+서버 | **결속** | 52 | `update-2010-5.0.1` |
| [Canted Aiming](https://sp-mod.com/mod/2415) | 2.0.0 | 서버 | **결속** | 4 | `stage7C-2415` |
| [Holster Everything](https://sp-mod.com/mod/2650) | 2.0.0 | 게임+서버 | **결속** | 7 | `stage7C-2650` |
| [Black Exfil Revision](https://sp-mod.com/mod/2380) | 1.1.0 | 서버 | **결속** | 1 | `stage7C-2380` |
| [Jero Backpack](https://sp-mod.com/mod/2369) | 4.1.1 | 서버 | **결속** | 4 | `stage7C-2369` |
| [Restore Armor Plates](https://sp-mod.com/mod/2973) | 1.0.2 | 게임+서버 | **결속** | 4 | `stage7C-2973` |
| [Repair Skill Extension](https://sp-mod.com/mod/2556) | 1.4.1 | 서버 | **결속** | 2 | `stage7C-2556` |
| [Repair Max Durability](https://sp-mod.com/mod/1305) | 2.2.1 | 게임+서버 | **결속** | 3 | `update-1305-2.2.1` |
| [Cobra's Durability Tweaks](https://sp-mod.com/mod/3019) | 1.0.0 | 서버 | **결속** | 5 | `stage7C-3019` |
| [Realistic Suppressors](https://sp-mod.com/mod/3171) | 1.0.0 | 서버 | - | 3 | `stage7C-3171` |
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
| [Ultimate Flea](https://sp-mod.com/mod/2844) | 1.3.2 | 게임+서버 | **결속** | 18 | `update-2844-1.3.1`, `update-2844-1.3.2` |
| [AllGoodsTrader](https://sp-mod.com/mod/2648) | 2.0.0 | 서버 | **결속** | 12 | `stage7C-2648` |
| [bluehead's AIO Trader](https://sp-mod.com/mod/374) | 5.3.1 | 서버 | **결속** | 10 | `stage7C-374` |
| [ALL AMMO TRADER - UPDATED BY TILTUSHKIN](https://sp-mod.com/mod/1554) | 1.5.0 | 서버 | **결속** | 5 | `stage7C-1554` |
| [BigBrain](https://sp-mod.com/mod/902) | 1.5.0 | 게임 | - | 1 | `stage7D-902` |
| [ItemGen](https://sp-mod.com/mod/2803) | 1.5.1 | 게임+서버 | **결속** | 4 | `stage7D-2803` |
| [Better OKP-7](https://sp-mod.com/mod/2711) | 1.1.0 | 서버 | **결속** | 6 | `stage7D-2711.dirs`, `stage7D-2711` |
| [TraderGen](https://sp-mod.com/mod/2749) | 2.4.1 | 게임+서버 | **결속** | 12 | `stage7D-2749` |
| [The Quartermaster](https://sp-mod.com/mod/2817) | 1.4.3 | - | **결속** | 0 | **미설치** |
| [Wolfik's Heavy Trooper Masks](https://sp-mod.com/mod/1569) | 1.2.0 | 서버 | **결속** | 10 | `stage7D-1569` |
| [Manimal's Ammo Loading Animations](https://sp-mod.com/mod/2681) | 2.0.0 | 게임+서버 | **결속** | 7 | `stage7D-2681` |
| [Sage's Incendiary Grenade](https://sp-mod.com/mod/3113) | 1.1.0 | 게임+서버 | **결속** | 6 | `stage7D-3113` |
| [.22lr Ammunition](https://sp-mod.com/mod/2516) | 2.0.0 | 서버 | **결속** | 21 | `stage7D-2516` |
| [Peacemaker Continued](https://sp-mod.com/mod/2414) | 2.1.0 | 서버 | **결속** | 15 | `stage7D-2414` |
| [Better Rear Sights](https://sp-mod.com/mod/1591) | 1.8.0 | 서버 | **결속** | 40 | `stage7D-1591` |
| [Fold That Stock](https://sp-mod.com/mod/2691) | 2.1.0 | 게임+서버 | **결속** | 22 | `stage7D-2691` |
| [WTT - Pack 'n' Strap](https://sp-mod.com/mod/1278) | 2.1.1 | 게임+서버 | **결속** | 60 | `stage7E-1278` |
| [Kalashnikov Enhanced Modding ](https://sp-mod.com/mod/3010) | 4.1.2 | - | **결속** | 0 | **미설치** |
| [Tactical Gear Component](https://sp-mod.com/mod/1125) | 3.0.0 | 서버 | - | 137 | `stage7E-1125` |
| [Painter](https://sp-mod.com/mod/1025) | 3.0.0 | 서버 | - | 27 | `stage7E-1025` |
| [LMSP - Let Me Switch Plates](https://sp-mod.com/mod/2814) | 1.0.1 | 게임 | - | 1 | `stage7A-2814` |
| [Unrestricted Armor Plate](https://sp-mod.com/mod/1878) | 2.1.0 | 서버 | - | 3 | `stage7A-1878` |
| [Armor Expert](https://sp-mod.com/mod/3106) | 3.1.0 | 게임 | - | 1 | `stage7A-3106` |
| [Munitions Expert - Reloaded](https://sp-mod.com/mod/3006) | 3.1.0 | 게임 | - | 1 | `stage7A-3006` |
| [Foldables](https://sp-mod.com/mod/2422) | 1.1.1 | 게임+서버 | - | 8 | `stage7A-2422` |
| [VisorEffectManager](https://sp-mod.com/mod/2429) | 1.1.0 | 게임 | - | 1 | `stage7A-2429` |
| [Change Helmet Visor](https://sp-mod.com/mod/2554) | 3.0.0 | 게임+서버 | - | 5 | `stage7A-2554` |
| [BiC - Bind in Container](https://sp-mod.com/mod/3190) | 1.0.2 | 게임 | - | 1 | `update-3190-1.0.2` |
| [Manimal's Quick Cancel](https://sp-mod.com/mod/2969) | 2.0.0 | 게임 | - | 1 | `stage7A-2969` |
| [Quick Aim](https://sp-mod.com/mod/2985) | 1.0.0 | 게임 | - | 1 | `stage7A-2985` |
| [All In Weapon: Unslotted](https://sp-mod.com/mod/2576) | 1.2.0 | - | - | 0 | **미설치** |
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
| [PIT Fireteam](https://sp-mod.com/mod/2676) | 1.0.0 | - | - | 0 | **미설치** |
| [Make Shotguns Great Again!](https://sp-mod.com/mod/1453) | 1.20.0 | 게임+서버 | **결속** | 223 | `update-1453-1.20.0` |
| [Game Panel HUD (SPT 4.1.6 포팅본, KmyTarkovApi 2.0.0 포함)](https://arca.live/b/tarkovspt/185302366) | 4.1.6-port | 게임 | - | 30 | `stage8G-gamepanelhud` |

## 절차 밖에서 들어온 것 (설치 기록을 뒤늦게 복원 — 당시 기록이 아니다)

| 모드 | 버전 | 위치 | 결속 | 파일 | 설치 기록 |
| --- | --- | --- | --- | --- | --- |
| [More Energy Drinks](https://sp-mod.com/mod/1688) | 1.3.2 | 서버 | **결속** | 87 | `stage9-1688` |
| [MoreBotsAPI](https://sp-mod.com/mod/2426) | 2.1.2 | 게임+서버 | - | 4 | `update-2426-2.1.2` |
| [WTT - Content Backport](https://sp-mod.com/mod/2512) | 2.0.7 | 게임+서버 | **결속** | 1773 | `update-2512-2.0.7` |
| [SAIN - Solarint's AI Modifications](https://sp-mod.com/mod/791) | 4.5.1 | 게임+서버 | - | 11 | `stage9-791` |
| [ISB Aishi](https://sp-mod.com/mod/2478) | 2.0.7 | 게임+서버 | **결속** | 3880 | `update-2478-2.0.7` |
| [ISB Special Forces](https://sp-mod.com/mod/2485) | 2.0.4 | 게임+서버 | - | 3173 | `update-2485-2.0.4.dirs`, `update-2485-2.0.4` |
| [GF2 Skin Pack](https://sp-mod.com/mod/3156) | 1.2.4 | 서버 | - | 76 | `update-3156-1.2.4` |
| [WTT - Head and Voice Selector](https://sp-mod.com/mod/1277) | 1.1.0 | 게임+서버 | - | 4 | `stage9-1277` |

## 10차 — 공개 목록 125개와 부착물 호환 (계획은 `PLAN-STAGE10.md`)

| 모드 | 버전 | 위치 | 결속 | 파일 | 설치 기록 |
| --- | --- | --- | --- | --- | --- |
| [Easy Slot Filters](https://sp-mod.com/mod/2940) | 1.0.0 | 서버 | - | 2 | `stage10-A-2940-EasySlotFilters` |
| [Effects Manager Framework](https://sp-mod.com/mod/3123) | 1.1.0 | 게임 | - | 1 | `stage10-B-3123-EffectsManagerFramework` |
| [WebOverlay](https://sp-mod.com/mod/2879) | 1.12.0 | 게임 | - | 7 | `update-2879-1.12.0` |
| [Map Variants](https://sp-mod.com/mod/3040) | 1.1.0 | 게임+서버 | - | 8 | `stage10-B-3040-MapVariants` |
| [Quest Codex](https://sp-mod.com/mod/3054) | 1.12.0 | 게임+서버 | - | 96 | `update-3054-1.12.0` |
| [Item Property Backport](https://sp-mod.com/mod/2689) | 1.5.2 | 서버 | - | 6 | `stage10-B-2689-ItemPropertyBackport` |
| [Hj's Mod Source](https://sp-mod.com/mod/3083) | 1.2.0 | 게임+서버 | - | 3 | `update-3083-1.2.0` |
| [Integrated Configuration Manager](https://sp-mod.com/mod/3095) | 1.1.0 | 게임 | - | 1 | `update-3095-1.1.0` |
| [MergeConsumables](https://sp-mod.com/mod/1657) | 1.6.3 | 게임+서버 | - | 4 | `stage10-C-1657-MergeConsumables` |
| [Continuous Healing](https://sp-mod.com/mod/1884) | 1.6.3 | 게임 | - | 2 | `stage10-C-1884-ContinuousHealing` |
| [Healing Autocancel](https://sp-mod.com/mod/1274) | 5.0.1 | 게임 | - | 1 | `stage10-C-1274-HealingAutocancel` |
| [HandsAreNotBusy](https://sp-mod.com/mod/1298) | 1.7.1 | 게임 | - | 1 | `stage10-C-1298-HandsAreNotBusy` |
| [Gilded Key Storage](https://sp-mod.com/mod/865) | 2.1.1 | 게임+서버 | - | 12 | `stage10-C-865-GildedKeyStorage` |
| [Use Loose Loot](https://sp-mod.com/mod/933) | 1.6.0 | 게임 | - | 1 | `stage10-C-933-UseLooseLoot` |
| [Loot In Vicinity](https://sp-mod.com/mod/2698) | 3.0.6 | 게임 | - | 4 | `stage10-C-2698-LootInVicinity` |
| [Cabinet Looter](https://sp-mod.com/mod/3103) | 1.0.4 | 게임 | - | 1 | `stage10-C-3103-CabinetLooter` |
| [Raid Settings Skipper](https://sp-mod.com/mod/2857) | 1.3.1 | 게임 | - | 1 | `stage10-C-2857-RaidSettingsSkipper` |
| [AutoKeycard for Labs](https://sp-mod.com/mod/2881) | 1.0.1 | 게임 | - | 1 | `stage10-C-2881-AutoKeycardForLabs` |
| [Dynamic Item Weights](https://sp-mod.com/mod/2344) | 1.2.1 | 게임 | - | 2 | `stage10-C-2344-DynamicItemWeights` |
| [WTT - Menu Overhaul](https://sp-mod.com/mod/1775) | 1.3.0 | 게임 | - | 9 | `stage10-D-1775-WTTMenuOverhaul` |
| [ImpactCue-UI-MOD](https://sp-mod.com/mod/3029) | 2.0.3 | 게임+서버 | - | 28 | `stage10-D-3029-ImpactCueUI` |
| [Hj's Improved Customization UI](https://sp-mod.com/mod/3069) | 1.1.2 | 게임+서버 | - | 3 | `stage10-D-3069-HjsImprovedCustomizationUI` |
| [Hj's Task Item Indicator](https://sp-mod.com/mod/3082) | 1.0.2 | 게임 | - | 1 | `update-3082-1.0.2` |
| [Hj's Task Turn in Indicator](https://sp-mod.com/mod/3071) | 1.0.0 | 게임 | - | 1 | `stage10-D-3071-HjsTaskTurnInIndicator` |
| [Name Plate](https://sp-mod.com/mod/3131) | 1.1.0 | 게임+서버 | - | 248 | `stage10-D-3131-NamePlate` |
| [Briefing](https://sp-mod.com/mod/3158) | 1.2.0 | 게임 | - | 12 | `update-3158-1.2.0` |
| [Breakout Outlines](https://sp-mod.com/mod/3007) | 1.2.0 | 게임 | - | 2 | `stage10-D-3007-BreakoutOutlines` |
| [Wishlist Extended](https://sp-mod.com/mod/2500) | 1.2.0 | 게임+서버 | - | 4 | `stage10-D-2500-WishlistExtended` |
| [Inventory Walker](https://sp-mod.com/mod/3058) | 0.3.1 | 게임 | - | 1 | `stage10-D-3058-InventoryWalker` |
| [Easy Ammo Names](https://sp-mod.com/mod/1262) | 2.1.0 | 서버 | - | 2 | `stage10-D-1262-EasyAmmoNames` |
| [More Tag Colours](https://sp-mod.com/mod/1533) | 1.5.0 | 게임 | - | 2 | `stage10-D-1533-MoreTagColours` |
| [All Quests Checkmarks](https://sp-mod.com/mod/2025) | 1.4.0 | - | - | 0 | **미설치** |
| [Expanded Task Text (ETT)](https://sp-mod.com/mod/2153) | 2.1.0 | 서버 | - | 5 | `stage10-E-2153-ExpandedTaskText` |
| [Hideout In Progress](https://sp-mod.com/mod/2076) | 3.0.0 | 게임+서버 | - | 2 | `stage10-E-2076-HideoutInProgress` |
| [Hideout Overhaul](https://sp-mod.com/mod/2982) | 0.2.0 | 서버 | - | 6 | `stage10-E-2982-HideoutOverhaul` |
| [Vertical Hideout](https://sp-mod.com/mod/2530) | 1.1.1 | 게임 | - | 1 | `stage10-E-2530-VerticalHideout` |
| [Target Dummies](https://sp-mod.com/mod/2932) | 0.3.1 | 게임 | - | 1 | `stage10-E-2932-TargetDummies` |
| [Tarkin's Hideout Cat](https://sp-mod.com/mod/2908) | 1.1.1 | 게임 | - | 9 | `stage10-E-2908-TarkinsHideoutCat` |
| [Cultist Circle Improvements](https://sp-mod.com/mod/2657) | 1.1.0 | 서버 | - | 8 | `stage10-E-2657-CultistCircleImprovements` |
| [Simple Workout QTE](https://sp-mod.com/mod/1437) | 2.2.2 | 게임+서버 | - | 5 | `stage10-E-1437-SimpleWorkoutQTE` |
| [Skill Distribution](https://sp-mod.com/mod/2144) | 1.3.0 | 게임+서버 | - | 3 | `stage10-E-2144-SkillDistribution` |
| [Health Per Level](https://sp-mod.com/mod/1706) | 2.2.0 | 서버 | - | 4 | `stage10-E-1706-HealthPerLevel` |
| [Fair Progression](https://sp-mod.com/mod/2652) | 1.1.0 | 서버 | - | 3 | `stage10-E-2652-FairProgression` |
| [Broken Limbs Don't Heal](https://sp-mod.com/mod/3121) | 1.1.0 | 게임 | - | 2 | `stage10-E2-3121-BrokenLimbsDontHeal` |
| [Bring Back Concussion](https://sp-mod.com/mod/2550) | 1.2.2 | 게임 | - | 1 | `update-2550-1.2.2` |
| [CookingGrenades 4.1](https://sp-mod.com/mod/2951) | 1.4.4 | 게임 | - | 1 | `stage10-F-2951-CookingGrenades` |
| [Inspectionless Malfunctions Reborn](https://sp-mod.com/mod/2992) | 1.0.0 | 게임 | - | 1 | `stage10-F-2992-InspectionlessMalfunctionsReborn` |
| [Deminvincibility](https://sp-mod.com/mod/1117) | 2.2.0 | 게임 | - | 1 | `stage10-F-1117-Deminvincibility` |
| [Smajlec's Stamina](https://sp-mod.com/mod/2608) | 1.4.0 | 게임 | - | 1 | `stage10-F-2608-SmajlecsStamina` |
| [Classic Movement](https://sp-mod.com/mod/1860) | 1.1.3 | 게임+서버 | - | 7 | `stage10-F-1860-ClassicMovement` |
| [Climbable Ladders](https://sp-mod.com/mod/2649) | 1.0.4 | 게임 | - | 11 | `stage10-F-2649-ClimbableLadders` |
| [Increase Climb Height](https://sp-mod.com/mod/1575) | 2.1.1 | 서버 | - | 4 | `stage10-F-1575-IncreaseClimbHeight` |
| [Increase Look Direction](https://sp-mod.com/mod/2121) | 1.3.0 | 게임 | - | 2 | `stage10-F-2121-IncreaseLookDirection` |
| [Auto Shoulder Swap](https://sp-mod.com/mod/2639) | 1.1.1 | 게임 | - | 1 | `stage10-F-2639-AutoShoulderSwap` |
| [No Aim Swing](https://sp-mod.com/mod/2719) | 1.2.0 | 게임 | - | 1 | `stage10-F-2719-NoAimSwing` |
| [Deadzone](https://sp-mod.com/mod/3067) | 1.2.0 | 게임 | - | 1 | `stage10-F-3067-Deadzone` |
| [ORBIT 3.0](https://sp-mod.com/mod/2706) | 3.1.1 | 게임+서버 | - | 19 | `stage10-F2-2706-ORBIT`, `update-2706-3.1.1` |
| [Lots of Loot Redux](https://sp-mod.com/mod/1619) | 4.2.2 | 서버 | - | 5 | `stage10-G-1619-LotsOfLootRedux` |
| [Rai's Hidden Caches](https://sp-mod.com/mod/1265) | 1.4.0 | 게임 | - | 1 | `stage10-G-1265-RaisHiddenCaches` |
| [Bosses Have Lega Medals](https://sp-mod.com/mod/1539) | 2.1.0 | 서버 | - | 3 | `stage10-G-1539-BossesHaveLegaMedals` |
| [Bosses Have Gp Coins](https://sp-mod.com/mod/2523) | 1.1.0 | 서버 | - | 3 | `stage10-G-2523-BossesHaveGpCoins` |
| [LootNET](https://sp-mod.com/mod/2679) | 1.2.0 | 게임+서버 | - | 4 | `stage10-G-2679-LootNET` |
| [Crates In Airdrop](https://sp-mod.com/mod/2481) | 2.0.0 | 서버 | - | 2 | `stage10-G-2481-CratesInAirdrop` |
| [Fast Sell In Flea](https://sp-mod.com/mod/2207) | 1.3.1 | 게임 | - | 1 | `stage10-G-2207-FastSellInFlea` |
| [Bot Callsigns - Reloaded](https://sp-mod.com/mod/1873) | 2.0.6 | 서버 | - | 6 | `stage10-G-1873-BotCallsignsReloaded` |
| [Realistic PMC Names](https://sp-mod.com/mod/3064) | 1.1.0 | 서버 | - | 6 | `stage10-G-3064-RealisticPMCNames` |
| [Smooth Talker](https://sp-mod.com/mod/2635) | 1.3.0 | 게임 | - | 1 | `stage10-G-2635-SmoothTalker` |
| [Auto IFF](https://sp-mod.com/mod/2784) | 2.1.1 | 게임 | - | 1 | `stage10-G-2784-AutoIFF` |
| [Enable Labyrinth](https://sp-mod.com/mod/2416) | 1.1.0 | 게임+서버 | - | 7 | `stage10-H-2416-EnableLabyrinth` |
| [UNTAR Go Home!](https://sp-mod.com/mod/2342) | 3.2.1 | 게임+서버 | - | 24 | `stage10-H-2342-UNTARGoHome` |
| [RUAF Come Home!](https://sp-mod.com/mod/2427) | 1.2.1 | 게임+서버 | - | 38 | `stage10-H-2427-RUAFComeHome` |
| [Door Randomizer](https://sp-mod.com/mod/820) | 1.8.0 | 게임 | - | 1 | `stage10-H-820-DoorRandomizer` |
| [LET ME OUT](https://sp-mod.com/mod/2240) | 1.1.0 | 게임 | - | 2 | `stage10-H-2240-LetMeOut` |
| [WTT - Black Division \[REDACTED\] Home](https://sp-mod.com/mod/2511) | 1.3.1 | 게임+서버 | - | 21 | `stage10-H-2511-WTTBlackDivisionHome` |
| [Rippers Casino](https://sp-mod.com/mod/3212) | 1.0.1 | 게임+서버 | - | 31 | `stage10-H-3212-RippersCasino` |
| [HollywoodFX](https://sp-mod.com/mod/2003) | 3.0.0 | 게임 | - | 13 | `stage10-I1-2003-HollywoodFX` |
| [Better Night Skies](https://sp-mod.com/mod/2734) | 2.0.0 | 게임 | - | 2 | `stage10-I1-2734-BetterNightSkies` |
| [Blackout](https://sp-mod.com/mod/2843) | 4.1.1 | 게임+서버 | - | 5 | `stage10-I1-2843-Blackout` |
| [Shadow Flicker Fix](https://sp-mod.com/mod/1621) | 1.5.3 | 게임 | - | 1 | `stage10-I1-1621-ShadowFlickerFix` |
| [Headshot Darkness](https://sp-mod.com/mod/1582) | 1.2.5 | 게임 | - | 2 | `stage10-I1-1582-HeadshotDarkness` |
| [Ragdoll Kinetics](https://sp-mod.com/mod/2905) | 1.2.0 | 게임 | - | 2 | `stage10-I1-2905-RagdollKinetics` |
| [Manimal's Watch Animations](https://sp-mod.com/mod/2745) | 2.0.0 | 게임 | - | 2 | `stage10-I1-2745-ManimalsWatchAnimations` |
| [BushWhacker - Standalone](https://sp-mod.com/mod/2329) | 1.7.0 | 게임 | - | 1 | `stage10-I1-2329-BushWhackerStandalone` |
| [Battle Ambience](https://sp-mod.com/mod/2264) | 2.4.0 | 게임 | - | 51 | `stage10-I1-2264-BattleAmbience` |
| [Music Extender](https://sp-mod.com/mod/2962) | 1.2.0 | 게임 | - | 3 | `stage10-I1-2962-MusicExtender.dirs`, `stage10-I1-2962-MusicExtender` |
| [CloudSix - Volumetric Clouds](https://sp-mod.com/mod/2567) | 4.0.3 | 게임 | - | 2 | `update-2567-4.0.3` |
| [FogSix](https://sp-mod.com/mod/3110) | 1.0.1 | 게임+서버 | - | 4 | `stage10-I2-3110-FogSix` |
| [ReflectSix](https://sp-mod.com/mod/3099) | 1.0.2 | 게임 | - | 2 | `update-3099-1.0.2` |
| [AOSix](https://sp-mod.com/mod/2569) | 2.0.1 | 게임 | - | 2 | `stage10-I2-2569-AOSix` |
| [POMSix](https://sp-mod.com/mod/3100) | 1.0.2 | 게임 | - | 2 | `stage10-I2-3100-POMSix` |
| [TraceSix - Full Raytracing and Voxel GI](https://sp-mod.com/mod/3221) | 1.0.1 | 게임 | - | 3 | `stage10-I2-3221-TraceSix` |
| [Raid Review](https://sp-mod.com/mod/1479) | 1.10.0 | 게임+서버 | - | 17 | `stage10-J-1479-RaidReview` |
| [Combat Log](https://sp-mod.com/mod/3072) | 1.0.0 | 게임 | - | 1 | `stage10-J-3072-CombatLog` |
| [ASBP - Acid's Scalable BepinEx Panel](https://sp-mod.com/mod/2931) | 1.0.1 | 게임 | - | 2 | `stage10-J-2931-ASBP` |
| [Better BepInEx](https://sp-mod.com/mod/3207) | 1.0.4 | 게임 | - | 1 | `update-3207-1.0.4` |
| [Performance Scope](https://sp-mod.com/mod/3076) | 0.2.0 | 게임 | - | 1 | `stage10-J-3076-PerformanceScope` |
| [Dynamic External Resolution Patch (DERP)](https://sp-mod.com/mod/2200) | 1.1.2 | 게임 | - | 1 | `stage10-J-2200-DERP` |
| [Knight Mask Fix](https://sp-mod.com/mod/3109) | 1.0.0 | 서버 | - | 3 | `stage10-J-3109-KnightMaskFix` |
| [IOF: Inventory Organizing Feature](https://sp-mod.com/mod/2960) | 1.10.1 | 게임 | **결속** | 2 | `stage10-K-2960-IOF` |
| [Item Preview QoL + screenshots](https://sp-mod.com/mod/2206) | 1.2.0 | 게임 | **결속** | 1 | `stage10-K-2206-ItemPreviewQoL` |
| [PityLoot](https://sp-mod.com/mod/3077) | 1.0.0 | 서버 | **결속** | 6 | `stage10-K-3077-PityLoot` |
| [TraumaCore](https://sp-mod.com/mod/2893) | 1.7.0 | 게임 | **결속** | 2 | `stage10-K-2893-TraumaCore` |
| [Lacy's PvE Tweaks](https://sp-mod.com/mod/2395) | 1.3.2 | 서버 | **결속** | 3 | `stage10-K-2395-LacyPvETweaks` |
| [Quest Backport](https://sp-mod.com/mod/3026) | 1.0.9 | 게임+서버 | **결속** | 293 | `stage10-K-3026-QuestBackport` |
| [Manimal's CS Gas](https://sp-mod.com/mod/2864) | 2.0.1 | 게임+서버 | **결속** | 12 | `update-2864-2.0.1` |
| [Content Backport - Prestiges](https://sp-mod.com/mod/2540) | 1.1.0 | 게임+서버 | **결속** | 63 | `stage10-L-2540-Prestiges` |
| [Strange Curios](https://sp-mod.com/mod/3059) | 1.1.0 | 게임+서버 | **결속** | 18 | `stage10-L-3059-StrangeCurios` |
| [Borkel's Realistic Night Vision Goggles (NVGs and T-7)](https://sp-mod.com/mod/954) | 3.0.4 | 게임+서버 | **결속** | 102 | `stage10-L-954-BorkelRNVG` |
| [Deploy Screen Overhaul](https://sp-mod.com/mod/3047) | 1.1.0 | 게임 | **결속** | 149 | `stage10-L-3047-DeployScreen` |
| [Tushonka Rare Collectibles](https://sp-mod.com/mod/3062) | 1.2.2 | 서버 | **결속** | 28 | `stage10-L-3062-Tushonka` |
| [WTT Halloween Masks](https://sp-mod.com/mod/2401) | 2.0.0 | 게임+서버 | **결속** | 15 | `stage10-L-2401-HalloweenMasks` |
| [Skills Extended](https://sp-mod.com/mod/2383) | 3.1.4 | 게임+서버 | **결속** | 95 | `stage10-L-2383-SkillsExtended` |
| [Russian Special Forces Pack](https://sp-mod.com/mod/3035) | 2.0.1 | 서버 | **결속** | 135 | `stage10-L-3035-RussianSFPack` |
| [Epic's All in One](https://sp-mod.com/mod/1263) | 5.0.3 | 서버 | **결속** | 632 | `update-1263-5.0.3` |
| [ECOT - Eukyre's Consortium of Things](https://sp-mod.com/mod/2195) | 2.0.1 | 서버 | **결속** | 259 | `stage10-L-2195-ECOT` |
| [DynamicMaps Extended](https://sp-mod.com/mod/3097) | 2.5.2 | 게임 | - | 201 | `stage10-M-3097-DynamicMapsExtended` |
| [Manimal's 1.0 Labs Office Backport](https://sp-mod.com/mod/2937) | 2.0.1 | 게임+서버 | - | 7 | `update-2937-2.0.1` |
| [Arena Map Port](https://sp-mod.com/mod/3166) | 0.6.0 | 게임+서버 | - | 400 | `update-3166-0.6.0` |
| [Manimal's 1.0 Interchange Expansion Backport](https://sp-mod.com/mod/3025) | 1.0.10 | 게임+서버 | - | 147 | `stage10-O-3025-InterchangeExpansion`, `update-3025-1.0.10` |
| [Manimal's 1.0 Lighthouse Rework Backport](https://sp-mod.com/mod/3022) | 1.0.7 | 게임+서버 | - | 50 | `stage10-O-3022-Lighthouse` |
| [MANIMAL'S ICEBREAKER BACKPORT](https://sp-mod.com/mod/2866) | 1.2.4 | 게임+서버 | - | 95 | `update-2866-1.2.4` |

## 11차 — 성능·메모리 (후보는 `CANDIDATES-PERF.md`, 실측은 `MEMORY-AUDIT-2026-11-11.md`)

| 모드 | 버전 | 위치 | 결속 | 파일 | 설치 기록 |
| --- | --- | --- | --- | --- | --- |
| [VRAM Cleaner](https://sp-mod.com/mod/2173) | 1.1.0 | 게임 | - | 1 | `stage11-A-2173-VRAMCleaner` |

## 12차 — 방어구 인서트 (고정 소프트 아머를 떼고 바꾼다. 조사는 `CANDIDATES-SOFT-ARMOR-SWAP.md`)

| 모드 | 버전 | 위치 | 결속 | 파일 | 설치 기록 |
| --- | --- | --- | --- | --- | --- |
| [Armor Tweaks and Additions](https://sp-mod.com/mod/3355) | 1.0.0 | 게임+서버 | **결속** | 2 | `stage12-A-3355-ArmorTweaks` |

## 13차 — 프레스티지 (계획은 `plans/2026-11-15-prestige-mods-install-plan.md`, 자체 모드는 `specs/2026-11-15-prestige-stash-relax-design.md`)

| 모드 | 버전 | 위치 | 결속 | 파일 | 설치 기록 |
| --- | --- | --- | --- | --- | --- |
| [Sicc Case Fix](https://sp-mod.com/mod/2687) | 2.0.0 | 서버 | **결속** | 1 | `stage13-A-2687-SiccCase` |
| [Prestige Perks](https://sp-mod.com/mod/3332) | 0.1.10 | 게임+서버 | - (치트 표시) | 11 | `stage13-B-3332-PrestigePerks` |

## 14차 — 은신처 꾸미기·습격, 실시간 웹 지도, 치트 메뉴, 좀비 모드 (계획은 `plans/2026-11-17-six-mods-install-plan.md`)

| 모드 | 버전 | 위치 | 결속 | 파일 | 설치 기록 |
| --- | --- | --- | --- | --- | --- |
| [Hideout Interiors](https://sp-mod.com/mod/3326) | 1.0.0 | 게임 | - | 10 | `stage14-A-3326-HideoutInteriors` |
| [Hideout Last Stand](https://sp-mod.com/mod/3353) | 0.3.1 | 게임+서버 | **결속** | 30 | `stage14-B-3353-HideoutLastStand` |
| [Satellite](https://sp-mod.com/mod/3373) | 1.0.1 | - | - | 0 | **미설치** |
| [Satellite - Interchange Backport Map](https://sp-mod.com/addon/201) | 1.0.0 | - | - | 0 | **미설치** |
| [Satellite - Lighthouse Backport Map](https://sp-mod.com/addon/203) | 1.0.0 | - | - | 0 | **미설치** |
| [Satellite - The Lab (Office) Backport Map](https://sp-mod.com/addon/202) | 1.0.0 | - | - | 0 | **미설치** |
| [Satellite - Icebreaker Map](https://sp-mod.com/addon/200) | 1.0.0 | - | - | 0 | **미설치** |
| [T Override \[SP CHEAT\]](https://sp-mod.com/mod/3317) | 1.4.3 | - | - (치트 표시) | 0 | **미설치** |
| [Hideout Uncensored](https://sp-mod.com/mod/3167) | 1.3.0 | - | - | 0 | **미설치** |
| [Tushonka Mapping SDK](https://sp-mod.com/mod/3193) | 1.9.3 | - | - | 0 | **미설치** |
| [Pack A Punch Hideout Module](https://sp-mod.com/mod/3223) | 2.2.0 | - | - | 0 | **미설치** |
| [ZM_Thundergun](https://sp-mod.com/mod/3226) | 1.2.0 | - | - | 0 | **미설치** |
| [ZM_Raygun](https://sp-mod.com/mod/3229) | 1.2.0 | - | - | 0 | **미설치** |
| [Dr K's Perk Beverages](https://sp-mod.com/mod/3351) | 0.1.0 | - | - | 0 | **미설치** |
| [Dr K's SPZombies](https://sp-mod.com/mod/3352) | 0.1.2 | - | - | 0 | **미설치** |
| [ZM_Stronghold](https://sp-mod.com/addon/193) | 0.1.0 | - | - | 0 | **미설치** |

## 다른 경로로 설치된 것

| 항목 | 위치 | 설치 주체 |
| --- | --- | --- |
| SptKoreanConfig 0.1.0 (F12 설정 창 한글화) | `BepInEx\plugins\SptKoreanConfig\` (DLL 1 + 번역 사전 197) | 이 저장소에서 직접 만든 것. 소스는 [../src/SptKoreanConfig](../src/SptKoreanConfig), 설치는 `tools/install-koreanconfig.ps1`. Forge 모드가 아니라 설치 기록에 없다 |
| SptKoreanLocale 0.1.0 (게임 DB locale 한글화) | `SPT_Runtime\user\mods\SptKoreanLocale\` (DLL 1 + `config.jsonc` + 사전 21) | 이 저장소에서 직접 만든 것. 소스는 [../src/SptKoreanLocale](../src/SptKoreanLocale), 설치·제거는 `tools/install-koreanlocale.ps1` (`-Remove`). 서버가 내려주는 게임 locale(`kr`)의 값을 `lang/*.json` 사전으로 덮어쓴다 — **다른 모드의 파일은 건드리지 않는다**(업데이트에 날아간다). 지금 사전 항목은 8804개이고 지금까지 **봇 역할 이름 23개**(킬 패널·시체 표시·지도 범주), **BTR 대화 44개**, **아이템 이름 742개**(바닐라 28 · ISB 계열 378 · QuestCodex 퀘스트 아이템 336), **퀘스트 이름 1,243개**(목록·상인 화면·추적기·지도 표시), **퀘스트 목표 3,482개**(손으로 적은 목표 문구 2,816개를 펴낸 것 — 이 묶음은 전부 덮었다), **퀘스트·업적·치장 설명 1,526개**(머리말은 규칙으로 옮기고 본문은 바닐라 한국어를 되쓰거나 손으로 적는다 — 이 묶음도 전부 덮었다), **퀘스트 대사·메일 1,744개**(상인이 보내는 글과 플레이어의 짧은 대답)를 덮었다. 목표는 열쇠가 아니라 **문구 단위**로 적고(`src/SptKoreanLocale/lang-src/`) `tools/expand-locale-dict.mjs` 가 열쇠 사전으로 펴낸다. `OnLoadOrder.PostLoad` 여야 한다 — 모드가 만든 아이템·퀘스트의 열쇠는 그 모드가 돈 뒤에 생긴다. 사전에 **값을 직접 쓰면 날아간다** — 늦게 변환기를 등록하는 모드가 있으면 `LazyLoad` 캐시가 버려지고 파일에서 다시 세워진다(실측: 22항목을 쓴 뒤 60,398 → 61,125 항목으로 다시 세워져 전부 영문으로 돌아왔다). 그래서 `LazyLoad.AddTransformer` 로 **변환기를 등록**해 사전이 세워질 때마다 입힌다. 그 locale 이 없으면 서버가 조용히 `en` 을 돌려주므로, `en` 과 같은 인스턴스일 때는 **아무것도 하지 않는다**. 적용 여부는 `node tools/check-locale-dict.mjs` 가 켜져 있는 서버에게 물어 확인한다. 범위 검토는 [REVIEW-KOREAN-LOCALIZATION.md](REVIEW-KOREAN-LOCALIZATION.md). Forge 모드가 아니라 설치 기록에 없다 |
| 모드 폴더 안의 한국어 locale 5개 (항목 1647) | `BepInEx\plugins\…\locales\kr.json` 넷 — ISB 자막·Advanced Stash Sorting·Wishlist Extended·Loot In Vicinity | 이 저장소에서 만든 번역 파일. 사전은 [../src/PluginLocales](../src/PluginLocales), 설치·제거는 `tools/install-plugin-locales.ps1` (`-Remove`). 네 모드 모두 `locales/` 를 훑어 파일 이름을 locale 코드로 쓰고 **키마다 영어로 떨어지므로** 일부만 번역해도 된다(역컴파일 실측 — 근거는 manifest 의 `evidence`). 게임이 서버에 `/client/locale/kr` 을 요청하므로 코드는 `kr` 이다(서버 로그 실측). **모드 폴더 안이라 그 모드를 업데이트하면 날아간다** — 그때 설치 스크립트를 다시 돌린다. 원본과 맞는지와 설치본과 같은지는 `node tools/check-plugin-locales.mjs` 가 본다. Forge 모드가 아니라 설치 기록에 없다 |
| HelmetSlotUnion 0.2.0 (헬멧 부착 칸 합집합 + 부착물 충돌 해제) | `SPT_Runtime\user\mods\HelmetSlotUnion\` (DLL 1 + `config.jsonc`) | 이 저장소에서 직접 만든 것. 소스는 [../src/HelmetSlotUnion](../src/HelmetSlotUnion), 설치는 `tools/install-helmetslotunion.ps1`. 헬멧은 칸 필터를 합집합으로 넓히고 파츠끼리의 `ConflictingItems` 를 지운다. 0.2.0 부터 **총기도** "동시에 장착할 수 없습니다" 를 지운다 — 총기 쪽은 칸 필터를 건드리지 않고(그건 SlotCategoryRelax 몫) 충돌 목록만 본다. 탄창·장전 손잡이는 구경 제한이라 그대로 둔다. Forge 모드가 아니라 설치 기록에 없다 |
| SlotCategoryRelax 0.2.0 (분류별 칸 열기 + 약실 탄종 개방) | `SPT_Runtime\user\mods\SlotCategoryRelax\` (DLL 1 + `config.jsonc`) | 이 저장소에서 직접 만든 것. 소스는 [../src/SlotCategoryRelax](../src/SlotCategoryRelax), 설치는 `tools/install-slotcategoryrelax.ps1`. 부품 분류를 골라 그 분류만 칸 필터를 넓힌다 — 조준경은 열고 핸드가드는 닫는 식. 고를 수 있는 분류는 서버 시작 때 DB 에서 찾아 `discovered-categories.json` 으로 내놓는다. `RelaxChambers`(기본 켜짐)는 **모든 총기의 약실에 모든 탄종**을 넣는다 — All In Weapon 의 `FckChambers` 를 대신하는 자리다(약실은 칸이 아니라 `Chambers` 라 분류 스위치가 닿지 않는다). 탄창은 건드리지 않으므로 다른 구경은 약실에 한 발씩 물리는 것까지다. 유탄발사기 셋(40x46·40mmRU·30x29)과 신호권총(26x75)은 `ExcludedChamberCalibers` 로 빼 둔다. 2026-11-05 에 실행 순서를 `OnLoadOrder.PostLoad` 로 옮겨 **모드가 만든 아이템까지** 본다 — 실측 927종 중 12종을 건너뛰고 915종의 약실 929개를 연다(그 전에는 바닐라 141종뿐이었다). Forge 모드가 아니라 설치 기록에 없다 |
| StackUnify 0.1.0 (스택 크기 통합) | `SPT_Runtime\user\mods\StackUnify\` (DLL 1 + `config.jsonc`) | 이 저장소에서 직접 만든 것. 소스는 [../src/StackUnify](../src/StackUnify), 설치는 `tools/install-stackunify.ps1`. 최대 스택 크기를 쓰는 서버 모드가 13개나 되어 어느 값이 남는지 예측할 수 없었다. 클라이언트 목록이 굳기 전의 가장 늦은 자리(`OnLoadOrder.PostLoad`)에서 설정에 적힌 값으로 못박는다. 더 늦은 자리는 DB 만 바뀌고 클라이언트에 닿지 않는다(실측). 기본 설정은 비어 있어 아무 일도 하지 않는다. Forge 모드가 아니라 설치 기록에 없다 |
| ScavRaidWaves 0.1.0 (스캐브 레이드 PMC 웨이브) | `SPT_Runtime\user\mods\ScavRaidWaves\` (DLL 1 + `config.jsonc`) | 이 저장소에서 직접 만든 것. 소스는 [../src/ScavRaidWaves](../src/ScavRaidWaves), 설치·제거는 `tools/install-scavraidwaves.ps1` (`-Remove`). 스캐브로 들어가면 PMC 가 안 나오던 것을 고친다. PMC 웨이브는 레이드 시작 30~1800초에 몰려 있는데, 스캐브 레이드는 `scavRaidTimeSettings` 때문에 시간이 40~70% 깎인 채 중간에 끼어드는 것으로 처리돼 그 웨이브를 통째로 놓친다. `LocationConfig` 의 `ReducedChancePercent`·`AdjustWaves` 만 설정대로 덮어쓴다. 기본값은 모든 맵의 `AdjustWaves` 를 false 로 두는 것 — 레이드는 짧은 채로 두고 그 길이 안에 드는 PMC 웨이브만 살린다. PMC 가 모자라면 `ReducedChancePercent` 를 0 으로 바꿔 시간 삭감 자체를 끈다. Forge 모드가 아니라 설치 기록에 없다 |
| AirdropCaller 0.1.0 (보급 호출기 — 외부 창) | `AirdropCaller.exe` + `airdrop-caller.config.json` (설치본 루트, `Give Ui.exe` 옆) | 이 저장소에서 직접 만든 것. 소스는 [../src/AirdropCaller.App](../src/AirdropCaller.App), 설치·제거는 `tools/install-airdropcaller-app.ps1` (`-Remove`). WPF 창. 프리셋을 짜고 레이드 중에 버튼으로 보급을 부른다. 아이템은 **한국어 이름으로 검색**한다. 인증은 게임 클라이언트와 같은 `Cookie: PHPSESSID=<프로필 id>` 를 쓰고, 그 id 는 `user\profiles` 의 파일 이름에서 읽는다. .NET 10 데스크톱 런타임이 필요하다(런타임 의존 게시, 222KB). 서버 모드·게임 플러그인과 셋이 한 벌이다. Forge 모드가 아니라 설치 기록에 없다 |
| AirdropCaller.Plugin 0.1.0 (보급 호출기 — 게임) | `BepInEx\plugins\AirdropCaller\` (DLL 1) | 이 저장소에서 직접 만든 것. 소스는 [../src/AirdropCaller.Plugin](../src/AirdropCaller.Plugin), 설치·제거는 `tools/install-airdropcaller-plugin.ps1` (`-Remove`). 레이드 상태를 서버에 2초마다 보고하고, 서버에 예약된 드롭이 있으면 꺼내 와 `ClientGameWorld.InitAirdrop` 을 부른다 — 공개 메서드 하나라 Harmony 패치가 없다. 서버 쪽 `AirdropCaller.Server` 와 짝이다. Forge 모드가 아니라 설치 기록에 없다 |
| AirdropCaller.Server 0.1.0 (보급 호출기 — 서버) | `SPT_Runtime\user\mods\AirdropCaller\` (DLL 1 + `config.jsonc`) | 이 저장소에서 직접 만든 것. 소스는 [../src/AirdropCaller.Server](../src/AirdropCaller.Server), 설치·제거는 `tools/install-airdropcaller-server.ps1` (`-Remove`). 바닐라의 보급 전리품 라우트(`POST /client/airdrop/loot`)에 **뒤에서** 끼어들어, `config.jsonc` 의 `Drops` 에 적힌 상자 id 에만 우리가 정한 내용물로 답한다. 적혀 있지 않은 id 는 바닐라 답을 그대로 흘려보내므로 **바닐라 보급기와 플레어 보급은 건드리지 않는다**. 설계는 [specs/2026-10-28-airdrop-caller-design.md](specs/2026-10-28-airdrop-caller-design.md). Forge 모드가 아니라 설치 기록에 없다 |
| ProfilePorter 0.1.0 (프로필 사이로 물건 베끼기 — 외부 창) | `ProfilePorter.exe` + `profile-porter.config.json` (설치본 루트, `Give Ui.exe`·`AirdropCaller.exe` 옆) | 이 저장소에서 직접 만든 것. 소스는 [../src/ProfilePorter.App](../src/ProfilePorter.App), 설치·제거는 `tools/install-profileporter-app.ps1` (`-Remove`). WPF 창. 왼쪽에서 내보낼 프로필과 물건을 **인게임과 같은 격자**로 고르고, 가운데 파일을 거쳐, 오른쪽 프로필에게 **인게임 우편**으로 보낸다. 칸을 누르거나 끌어 여러 개를 고르고, `착용장비 내보내기` 는 고른 것과 무관하게 입은 장비 전체를 낸다(보안 컨테이너는 뺀다). 인증은 게임 클라이언트와 같은 `Cookie: PHPSESSID=<프로필 id>`. 아이템 아이콘은 `icon-cache` 를 **AirdropCaller 와 함께 쓴다**. .NET 10 데스크톱 런타임이 필요하다(런타임 의존 게시, 237KB). 서버 모드와 둘이 한 벌이다. Forge 모드가 아니라 설치 기록에 없다 |
| ProfilePorter.Server 0.1.0 (프로필 사이로 물건 베끼기 — 서버) | `SPT_Runtime\user\mods\ProfilePorter\` (DLL 1 + `config.jsonc` + `porter\` + `backups\`) | 이 저장소에서 직접 만든 것. 소스는 [../src/ProfilePorter.Server](../src/ProfilePorter.Server), 설치·제거는 `tools/install-profileporter.ps1` (`-Remove`는 내보낸 파일·백업을 남기고, `-Purge`가 싹 지운다). **물건을 늘리는 관리 도구다** — 내보내는 쪽 프로필은 고치지 않고, 받는 쪽에는 스태시에 꽂지 않고 **우편으로 보낸다**(받을지는 사람이 우편함에서 정한다). 들여올 때 아이템 **id 를 전부 새로 매기고**, 쓰기 전에 그 프로필을 백업한다. 바닐라 라우트에 끼어들지 않아 **게임이 부르는 길에는 아무것도 더하지 않는다**. 설계는 [specs/2026-11-08-profile-porter-design.md](specs/2026-11-08-profile-porter-design.md). Forge 모드가 아니라 설치 기록에 없다 |
| **AirdropProbe 0.1.0 (보급 지점 측정기) — 임시** | `BepInEx\plugins\AirdropProbe\` (DLL 1 + probe.log) | 이 저장소에서 직접 만든 **측정 전용** 도구. 소스는 [../src/AirdropProbe](../src/AirdropProbe), 설치·제거는 `tools/install-airdropprobe.ps1` (`-Remove`). 아무것도 바꾸지 않고, 레이드에서 Ctrl+Alt+A 를 누르면 보급 지점 개수와 거리를 적는다. 보급 호출기 설계의 [미확인 사항 9](specs/2026-10-29-airdrop-stage1-findings.md)를 재기 위한 것이고 **재고 나면 지운다**. Forge 모드가 아니라 설치 기록에 없다 |
| GridWindowScroll 0.1.0 (인벤토리 격자 스크롤) | `BepInEx\plugins\GridWindowScroll\` (DLL 1 + 상자별 크기 sizes.json) | 이 저장소에서 직접 만든 것. 소스는 [../src/GridWindowScroll](../src/GridWindowScroll), 설치는 `tools/install-gridwindowscroll.ps1`. 가방·상자를 열었을 때 뜨는 내부 인벤 창이 화면을 넘어가지 않게, 보이는 칸 수를 가로·세로로 못박고 그보다 큰 격자는 스크롤바와 마우스 휠로 넘겨 본다. 칸 수는 창 오른쪽 아래 손잡이를 끌어 정하거나 F12 에서 정하고, 끌어 정한 값은 상자 종류마다 기억한다. Forge 모드가 아니라 설치 기록에 없다 |
| GogglesWheel 0.1.0 (야시경·실드 휠) | `BepInEx\plugins\GogglesWheel\` (DLL 1 + `config.jsonc`) | 이 저장소에서 직접 만든 것. 소스는 [../src/GogglesWheel](../src/GogglesWheel), 설치·제거는 `tools/install-goggleswheel.ps1` (`-Remove`). 헤드기어 제한을 푸는 모드를 넣으면 헬멧에 야시경과 페이스실드가 동시에 달리는데, 게임의 `ToggleGoggles` 가 여닫이 중 **첫 번째 하나만** 건드려 한쪽을 영영 못 열었다. 짧게 누르면 야시경, 길게 누르면 원형 메뉴에서 고른다. 같은 메서드를 가로채는 UI Fixes(나침반)·COTI(수정키)와 부딪히지 않게 물러나는 조건을 둔다. 설계는 [specs/2026-10-30-goggles-wheel-design.md](specs/2026-10-30-goggles-wheel-design.md). Forge 모드가 아니라 설치 기록에 없다 |
| DynamicMapsFactionMarkers 0.1.0 (모드 세력 지도 표시) | `BepInEx\plugins\DynamicMapsFactionMarkers\` (DLL 1 + `config.jsonc` + `diamond.png`) | 이 저장소에서 직접 만든 것. 소스는 [../src/DynamicMapsFactionMarkers](../src/DynamicMapsFactionMarkers), 설치·제거는 `tools/install-dynamicmapsfactionmarkers.ps1` (`-Remove`). Dynamic Maps 가 ISB 같은 모드 세력을 전부 일반 스캐브(주황 화살표)로 그리던 것을 역할 묶음마다 다른 색·모양으로 가른다 — 그 모드의 보스 판정이 enum 숫자가 박힌 목록이라 13701~13716 이 들어갈 자리가 없었다(실측은 [specs/2026-10-30-faction-identification-survey.md](specs/2026-10-30-faction-identification-survey.md)). Dynamic Maps 파일은 고치지 않고 Harmony 로만 끼어들며, 그 모드 내부가 바뀌면 조용히 비활성으로 떨어진다. 설계는 [specs/2026-10-30-dynamicmaps-faction-markers-design.md](specs/2026-10-30-dynamicmaps-faction-markers-design.md). Forge 모드가 아니라 설치 기록에 없다 |
| SlotFilterSweep 0.1.0 (죽은 부품 참조 쓸기) | `SPT_Runtime\user\mods\SlotFilterSweep\` (DLL 1 + `config.jsonc` + `dead-refs.json`) | 이 저장소에서 직접 만든 것. 소스는 [../src/SlotFilterSweep](../src/SlotFilterSweep), 설치·제거는 `tools/install-slotfiltersweep.ps1` (`-Remove`, 서버가 떠 있을 때는 `-AllowServerRunning`). 칸 필터에 적혀 있지만 **데이터베이스에 없는 부품 id** 를 걷어낸다 — 2026-11-12 실측으로 총기 칸 21곳이 없는 부품 13종을 가리키고 있었고(바닐라 0), 서버도 기동마다 `does not exist in the item database` 를 554줄 적었다. 출처는 남의 모드의 묵은 참조다(WTT-Armory 9종·EukyreECOT 2종·출처 불명 2종). **빼기만 한다** — 없는 부품을 만들어 주지 않고, 남의 모드 파일도 고치지 않는다(메모리의 표만 손보므로 업데이트에 날아갈 것이 없다). 자리는 `int.MaxValue` 로 아이템을 만드는 모드보다 뒤고, 더 늦게 만드는 모드가 있으면 `KeepIds` 로 지킨다. **기본값은 `Apply: false` — 세기만 하고 지우지 않는다.** 찾은 것은 로그와 `dead-refs.json` 에 남는다. 경위는 [REVIEW-MODS-AUDIT-2026-11-12.md](REVIEW-MODS-AUDIT-2026-11-12.md). Forge 모드가 아니라 설치 기록에 없다 |
| IcebreakerOffMapGuard 0.1.0 (아이스브레이커 off-map 작업 가리기) | `BepInEx\plugins\IcebreakerOffMapGuard\` (DLL 1) | 이 저장소에서 직접 만든 것. 소스는 [../src/IcebreakerOffMapGuard](../src/IcebreakerOffMapGuard), 설치·제거는 `tools/install-icebreakeroffmapguard.ps1` (`-Remove`, 게임이 떠 있을 때는 `-AllowGameRunning`). 아이스브레이커 백포트(2866)가 **그 맵이 아닌 곳에서도** 하는 일 둘을 밖에서 가린다 — 60초마다 도는 LOD 전수조사(`[PerfCensus]`, 우리 라이트하우스 58,529개)와 매 프레임 씬 이름 읽기. 그 모드 파일은 고치지 않고 대상을 런타임에 이름으로 찾으므로, 모드가 없으면 아무것도 걸지 않고 조용히 쉰다. **2026-11-11 첫 설치본은 F12 열쇠의 홑따옴표 때문에 `Awake` 가 터져 아무것도 걸지 못했다** — 2026-11-12 에 열쇠를 바꿔 다시 넣었고(11,776 B), 걸렸는지는 기동 로그의 "걸었다:" 두 줄과 `[PerfCensus]` 0회로 본다. 경위는 [fixes/2026-11-11-icebreaker-offmap-guard.md](fixes/2026-11-11-icebreaker-offmap-guard.md). Forge 모드가 아니라 설치 기록에 없다 |
| PrestigeStashRelax 0.1.0 (프레스티지 이전 창고 — 서버) | `SPT_Runtime\user\mods\PrestigeStashRelax\` (DLL 1 + `config.jsonc` + `prestige-stash-report.json`) | 이 저장소에서 직접 만든 것. 소스는 [../src/PrestigeStashRelax](../src/PrestigeStashRelax), 설치·제거는 `tools/install-prestigestashrelax.ps1` (`-Remove`, 서버가 떠 있을 때는 `-AllowServerRunning`). 프레스티지 **이전 창고**의 격자와 담을 수 있는 품목을 넓힌다 — 바닐라는 P1 8×3(24칸)에서 P6 8×8(64칸)이고, 품목은 교역품 위주의 화이트리스트다(P3 부터 무기·부품·방어 장비가 열리고 **가방·조끼·돈·탄·열쇠는 어느 단계에도 없다**). 단계별 원본을 **단계 id 를 열쇠로** 떠 두고 늘 그 원본에서 다시 쌓아 **몇 번 돌려도 같다**(색인으로 들면 다른 모드가 리스트를 갈아끼울 때 떠 버린다). `ExpandCategories` 는 더한 분류를 **서버의 살아 있는 아이템 표**에서 자손 id 로 펼친다 — 분류만 넣었을 때 모드가 넣은 배낭이 화면에서 거부됐기 때문이다(실측 528개를 더했고, 바닐라 표로는 136개뿐이다). 자리는 `int.MaxValue` 라 2540(`TypePriority` 100003)보다 **뒤 단계**에서 돈다. **기본값은 `Apply: false` — 재기만 한다.** `ProbeIds` 로 적용 뒤 허용 목록에 특정 id 가 들어갔는지 보고서에 적게 할 수 있다. 설계는 [specs/2026-11-15-prestige-stash-relax-design.md](specs/2026-11-15-prestige-stash-relax-design.md), 측정은 [specs/2026-11-15-prestige-stash-measurements.md](specs/2026-11-15-prestige-stash-measurements.md). Forge 모드가 아니라 설치 기록에 없다 |
| PrestigeTransferKeep 0.1.0 (전송 물건 지키기) | `SPT_Runtime\user\mods\PrestigeTransferKeep\` (DLL 1 + `config.jsonc`) | 이 저장소에서 직접 만든 것. 소스는 [../src/PrestigeTransferKeep](../src/PrestigeTransferKeep), 설치·제거는 `tools/install-prestigetransferkeep.ps1` (`-Remove`, 서버가 떠 있을 때는 `-AllowServerRunning`). **3332 Prestige Perks 가 설계상 전송 목록을 비우고 코인으로 바꾸는 것**을 밖에서 되돌린다 — 2026-11-16 실측으로 가방 하나에 든 2,001개(122억 루블)가 코인 1,039개가 되고 121,075개어치가 상한에 걸려 버려졌다(그 상한의 "96 cells" 는 우리가 넓힌 8×12 다). 3332 의 `config.json` 에는 끄는 항목이 없다. 같은 메서드(`ProcessPendingPrestige`)에 Prefix 를 **둘** 걸어 3332 를 사이에 둔다 — `Priority.First` 로 목록을 떠 두고 `Priority.Last` 로 비워진 것을 도로 끼운다(지금 목록이 떠 둔 것보다 적을 때만). 전송분 코인 우편은 `Deliver` 를 건너뛰어 막는다 — 물건까지 받으면 두 번 받는 셈이다. **보스·금고 코인은 그대로**라 퍽을 살 길은 남는다. **3332 의 어셈블리를 참조하지 않는다**(업데이트에 깨진다) — 물건 지키기는 SPT 기본 타입만 쓰고, 코인 막기는 타입을 이름으로 찾아 **인자 없는 Prefix** 로 건다. 대상을 못 찾으면 조용히 지나가지 않고 로그에 적는다. 경위는 [fixes/2026-11-16-prestige-perks-eats-transfer.md](fixes/2026-11-16-prestige-perks-eats-transfer.md). Forge 모드가 아니라 설치 기록에 없다 |
| PrestigeStashRelaxClient 0.1.0 (프레스티지 이전 창고 — 게임) | `BepInEx\plugins\PrestigeStashRelax\` (DLL 1) | 이 저장소에서 직접 만든 것. 소스는 [../src/PrestigeStashRelax.Plugin](../src/PrestigeStashRelax.Plugin), 설치·제거는 `tools/install-prestigestashrelax-plugin.ps1` (`-Remove`, 게임이 떠 있을 때는 `-AllowGameRunning`). **서버 데이터로는 풀 수 없는 관문**을 뗀다 — 클라이언트는 내용물이 든 가방을 허용 목록과 무관하게 거부한다(`CompoundItem` 의 `IsEmpty` 가 false 면 막는다). 그 검사가 **두 곳에 복사돼 있다**: 클릭은 `PrestigeTransferItemsState` 의 `ISelectionContext.IsActive`, 끌기는 `ArenaEftItemTransferGridView.CanAccept` 다(실측 — 클릭은 넘어가는데 끌기는 안 넘어갔다). 앞은 **Postfix** 로 거부 사유(`tooltip`)를 보고 되돌린다 — 그 문자열이 어느 규칙이 걸렸는지 알려 주므로 규칙을 하나씩 가려 끌 수 있다. 뒤는 거부되면 `operation` 이 오류로 채워져 나와 Postfix 로 못 고치므로 **Transpiler** 로 `get_IsEmpty` 호출 하나의 반환값만 뒤집는다 — 그 검사만 빠지고 겹침·자리·소유권 판정은 원본 그대로 돈다. F12 「프레스티지 이전 창고 풀기」에서 **내용물이 든 가방도 넘기기**(기본 켬)와 **품목 제한 무시**(기본 끔)를 따로 켠다. 패치 대상을 못 찾으면 **조용히 죽지 않고** 로그에 에러를 남기고 아무것도 걸지 않는다. 걸렸는지는 기동 로그의 "끌기 경로에서 IsEmpty 검사 1곳을 비껴가게 했다" 로 본다 — **걸린 것과 먹은 것은 다르다**. 2026-11-16 에 실제로 프레스티지해 **가방 3개에 231개가 우편으로 그대로** 왔다(받은 아이템 234 = 3 + 231). Forge 모드가 아니라 설치 기록에 없다 |
| ServerWebKorean (서버 웹 설정창 한글 locale, 항목 230) | `SPT_Runtime\SPT_Data\database\locales\web\ko.json` | 이 저장소에서 만든 번역 파일. 소스는 [../src/ServerWebKorean](../src/ServerWebKorean), 설치·제거는 `tools/install-server-web-locale.ps1` (`-Remove`). 서버 웹 설정창(SIC)의 영문 `en.json` 과 **열쇠 수가 같아야** 한다 — `node tools/check-web-locale.mjs` 가 본다. 자기 폴더 없이 **서버 데이터베이스 안에 파일 하나로** 들어가므로 SPT 를 다시 깔면 날아간다. 문서는 [SERVER-WEB-KOREAN.md](SERVER-WEB-KOREAN.md). Forge 모드가 아니라 설치 기록에 없다 |
| WebConfigBridge 0.1.0 (모드 페이지) | `SPT_Runtime\user\mods\WebConfigBridge\` (DLL 1) | 이 저장소에서 직접 만든 것. 소스는 [../src/WebConfigBridge](../src/WebConfigBridge), 설치는 `tools/install-webconfigbridge.ps1`. 서버 웹 페이지 첫 화면의 **모드 페이지** 구역에 카드를 만들고(`/webconfigbridge`), 거기서 `All In Weapon: Unslotted` 의 무기·부품·탄약 호환 설정을 스위치로 고친다. 저장은 즉시, 적용은 다음 서버 시작부터. Forge 모드가 아니라 설치 기록에 없다 |
| ModCardKorean 0.1.0 (모드 카드·웹 화면 한글화) | `SPT_Runtime\user\mods\ModCardKorean\` (DLL 1 + `mod-cards.ko.json` + `ko-ui.js` + `ko-ui.json`) | 이 저장소에서 직접 만든 것. 소스는 [../src/ModCardKorean](../src/ModCardKorean)·[../src/ServerWebUiKorean](../src/ServerWebUiKorean), 설치·제거는 `tools/install-modcardkorean.ps1` (`-Remove`). **두 가지를 한다.** (1) 첫 화면 **모드 페이지** 카드의 이름·설명 — 그 글귀는 `locales/web/ko.json` 을 거치지 않고 각 모드 DLL 안에 리터럴로 박혀 있다. `SPTWeb.InitializeSptBlazor` 가 만든 `ModPage` 싱글턴을 `LandingPage` 가 로케일 조회 없이 그대로 찍는데, `ModPage` 가 setter 가 있는 보통 클래스이고 기동 순서가 `Build()` → `RunPreSptLoadCallbacks` 라 **남의 DLL 을 건드리지 않고** `OnLoadAsync` 에서 제자리 수정한다. `Injectable` 의 `TypePriority` 를 반드시 적어야 한다 — 기본값 `int.MaxValue` 는 그 패스의 `[0, 200000)` 범위 밖이라 조용히 안 돈다. (2) **페이지 안쪽** — `IStartupFilter` 미들웨어가 HTML 응답마다 `</head>` 앞에 `ko-ui.js` 를 끼우고, 그 번역기가 MutationObserver 로 Blazor 재렌더를 따라가며 사전·규칙으로 바꾼다. **SPT 파일은 하나도 건드리지 않는다** — `/isb`·`/ultimateflea/app.html` 처럼 모드가 직접 내놓는 독립 HTML 까지 잡힌다. 아이템 이름은 `/modcardkorean/item-names.json` 이 **서버 메모리의 로케일에서 그 자리에서** 뽑는다(3,669줄 — 디스크 파일로는 2,778줄뿐이다). 하위 페이지·탭·메뉴까지 돈 점검에서 미번역 4,483 → 1,670, 번역할 UI 문구는 0(남은 것은 이름·식별자·아이템 이름) — [AUDIT-WEB-UI-KOREAN-2026-11-16.md](AUDIT-WEB-UI-KOREAN-2026-11-16.md). 사람 이름·URL·MongoID 는 일부러 번역하지 않는다. 확인은 `node tools/check-mod-card-locale.mjs`(카드)와 `node tools/dump-web-ui-strings.mjs`(화면, headless Chrome). 경위와 남은 것은 [REVIEW-MOD-PAGE-KOREAN.md](REVIEW-MOD-PAGE-KOREAN.md). Forge 모드가 아니라 설치 기록에 없다 |
| SPT Team Manager 1.0.0 (프로필 팀원) | `BepInEx\plugins\SptTeamManager\` (DLL 1 + 에셋 17) + `SPT_Runtime\user\mods\SptTeamManager-ServerMod\` (DLL 2 + `deps.json` + `Resources\`) | **PIT Fireteam 1.0.0([2676](https://sp-mod.com/mod/2676/pit-fireteam), 기준 커밋 `f32a41b`)의 포크**다. 소스는 [../src/SptTeamManager](../src/SptTeamManager)(히스토리째 git subtree), 설치·제거는 `tools/install-sptteammanager.ps1` (`-Remove`). **PIT 와 함께 둘 수 없어 PIT 를 빼고 설치했다**(같은 `/singleplayer/pitfireteam/*` 라우트 접두를 쓴다 — 스크립트가 감지해 거부한다). 빼낸 PIT 는 `backup/2026-11-01-before-stm/`. PIT 의 용병 기능은 그대로 두고, SPT 런처의 **다른 프로필**(로그인한 프로필은 제외)을 팀원으로 등록하는 것을 더한다. 계획 2(레이드 투입)까지 **되는 것**: 프로필 열거·가용성 판정, 등록 저장, 친구 목록 병합, 명단 화면(계획 1); 초대하면 파티에 합류, 그 프로필의 **실물 장비로 레이드 투입**(장비가 없으면 생성된 **임시 장비**), **실물로 나갔다 돌아오지 못하면 착용 장비를 프로필에서 삭제**(스태시·보안 컨테이너는 보존), 같은 레이드를 두 번 처리하지 않는 영수증(계획 2). 팀원은 최대 4명까지 넣을 수 있다(클라이언트가 막는다). 계획 3(장비 편집·팀원 상세 화면·프로필 팀원 auto-join·제스처 `FIRE`/`HOLD FIRE`/`TAKE COVER`)은 **인게임에서 절반 넘게 확인됐다** — 상세 화면·편집기·auto-join 은 2026-11-02~03 에, 편집기의 화면 절반 11항목은 2026-11-04 에 통과했다. **관문 25항목 중 11개(10·11·16·17·18~23·25)가 남아 있다** — 자세히는 `doc/STM-HANDOVER.md`. 계획 4(장비 꾸러미)와 계획 5(편집 동등성)도 들어갔다: **꾸러미 구매**가 2026-11-06 에 실물로 돌았고(1,141,860₽ / 산 물건 31개 / 스태시로 30개), 편집기는 **가방·리그 칸에 놓기·쪼개기·합치기·탄박스 열기·보안 컨테이너 칸**까지 된다. **아직 안 되는 것**: 레이드 후 경험치·스킬·전적·전리품 반영(계획 2 는 **잃는 것만 쓰고 얻는 것은 쓰지 않는다**), 플레이어 스태시를 출처로 쓰는 전송·수리·보험, 제스처 `SILENCE`/`KNIVES ONLY`/`SPREADOUT`/`GET BACK`(BACKLOG). 임시 장비로 죽은 팀원의 시체에서 털어 온 물건은 레이드 종료 때 사라진다(의도된 동작, 로그로 알린다). 설계는 [specs/2026-10-31-spt-team-manager-design.md](specs/2026-10-31-spt-team-manager-design.md), 결과와 남은 확인 항목은 [WORKLOG](WORKLOG.md). 컴파일 기준은 이 설치본의 4.1.6(업스트림은 4.1.0) |
| CompatToggle 0.1.0 (호환성 토글) | **설치 안 함** | 이 저장소에서 직접 만든 것. 소스는 [../src/CompatToggle](../src/CompatToggle). `All In Weapon: Unslotted`를 F12 토글로 대체하려 했으나 개조 화면의 부품 목록을 채우지 못해 철회했다. 경위는 [WORKLOG](WORKLOG.md) 참고 |
| HideSpecialIconGrids 1.1.0.1 | `BepInEx\plugins\HideSpecialIconGrids.dll`, `BepInEx\config\com.pandahhcorp.hidespecialicongrids.cfg` | SVM `Greed.exe`가 프리셋 `CSM.EnableCSM: true` 적용 시 설치 (15:01). 설치 기록에 없음 |
| SVM 프리셋 `Noname` | `SPT_Runtime\user\mods\[SVM] Server Value Modifier\Presets\Noname.json` | 사용자가 `Greed.exe`로 생성. 16:00에 `Hideout.RemoveConstructionsRequirements`만 false로 수정 |
