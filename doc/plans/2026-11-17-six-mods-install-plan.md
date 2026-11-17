# 계획 — 새 모드 6개 (14차): Satellite · Hideout Interiors · Hideout Last Stand · T Override · SPZombies · Skill Cap 100

- 날짜: 2026-11-17
- 상태: **14-A(§8)·14-B(§9·§10)·14-C(§11)·14-D(§12) 설치·확인 끝.** 14-E 는 아직이다. 받기·해제는 §7
- 대상: `E:\Spt` (SPT 4.1.6, EFT 0.16.9.5.40743)
- 조사: Forge API(`tools/survey-candidates.mjs`·버전·의존성 트리), 각 모드 페이지 본문 전문, 애드온 탭(내장 브라우저),
  댓글(`tools/fetch-mod-comments.mjs`, 9개 모드 189건, 실패 0), 설치본 실측(설치 목록·DLL 안 GUID·서버 모드 폴더·`.cfg` 단축키·패처).
- 받기 매니페스트: [`doc/download/manifest-stage14.json`](../download/manifest-stage14.json) (Skill Cap 100 은 넣지 않았다 — §2-6)

## 1. 한눈에

| 묶음 | 모드 | 버전 (SPT) | 크기 | 결속 | 선행 | 판단 |
| --- | --- | --- | ---: | --- | --- | --- |
| 14-A | [3326 Hideout Interiors](https://sp-mod.com/mod/3326) | 1.0.0 (`~4.1.0`) | 0.4MB | - | - | **넣는다** |
| 14-B | [3353 Hideout Last Stand](https://sp-mod.com/mod/3353) | 0.3.1 (`~4.1.0`) | 6.7MB | **결속** | 3326 | **넣는다** — 14-A 확인 뒤, 혼자, 프로필 백업 뒤 |
| 14-C | [3373 Satellite](https://sp-mod.com/mod/3373) + 맵 애드온 4 | 1.0.1 (`~4.1`) | 1.0GB | - | - | **넣는다** — Dynamic Maps 는 남긴다(§2-3) |
| 14-D | [3317 T∗∗∗∗∗ Override](https://sp-mod.com/mod/3317) | 1.4.3 (`4.1.6`) | 0.1MB | - (치트) | - | **넣는다** — 첫 기동 전에 단축키를 바꾼다(§2-4) |
| 14-E | [3352 Dr K's SPZombies](https://sp-mod.com/mod/3352) + 선행 6 + 맵 애드온 | 0.1.2 (`4.1.6`) | **2.0GB** | - | 6개 (설치 0) | **넣는다면 맨 끝** — 알파, 받을 양이 가장 크다(§2-5) |
| - | [3153 Skill Cap 100](https://sp-mod.com/mod/3153) | 1.0.5 (`~4.1.6`) | 25KB | **결속** | - | **넣지 않는다** — Skills Extended 제작자가 비호환을 못박았다(§2-6) |

- 6개와 선행 6개, 모두 12개가 **설치돼 있지 않다**: 설치 목록, 설치본 DLL 안 GUID, 서버 모드 폴더를 셋 다 확인했다.
- 6개 모두 4.1.6 호환판이 있다. Fika 표시는 3317 만 비호환인데, 우리는 Fika 를 설치하지 않았다.
- 받을 양은 모두 **약 3.0GiB**다(14-E 2.04GiB, 14-C 1.0GiB, 나머지 7MB). E: 여유는 799GB.

## 2. 모드별 — 페이지에서 읽은 것과 우리 설치본에 걸리는 것

### 2-1. 14-A Hideout Interiors (`jrs.hideoutinteriors`, 게임 전용)

은신처 전경 화면에 "꾸미기" 버튼을 붙여, 상자·선반·조명을 집어 옮기고 돌리고 숨기고 다른 구역에 붙인다. 1.0.0 은 포스터·TV 화면(동영상·YouTube·Twitch)·웹 브라우저 컴퓨터를 더했다.

- **외부 프로그램을 띄운다**: 컴퓨터 화면은 `browser/HideoutBrowser.exe`(Edge WebView2)가 게임 옆에서 돌다 함께 닫힌다. 겹침 검사에서 이 exe 를 확인한다.
- 바꾼 배치는 `BepInEx/config/jrs.hideoutinteriors.*.json` 에 프로필마다 남는다. 지워도 그 파일은 남는다.
- **단축키**: 꾸미기 모드 안의 `G`(이동/회전 전환)·`R`·`P`·`M`·`Delete`·`Backspace`·`Ctrl+Z`, 걸어 다닐 때 화면 앞에서 `F`.
  설치본의 `F` 는 **Amands' Sense**(`SenseKey = F`)와 UI Fixes(`Filter by Item Shortcut = F`, 인벤토리)가 쓴다.
  Sense 는 레이드용이라 은신처에서 겹칠지 화면으로 본다. `G` 는 Cooking Grenades 의 수류탄 휠인데, 꾸미기 모드에서만 쓰는 키라 겹칠 일은 적다.
- 이미 있는 은신처 모드 — Hideout Overhaul(서버)·Hideout In Progress·Vertical Hideout·Hideout Cat·Hideout UI Revamp — 와의
  호환은 페이지에 언급이 없다. 페이지가 "함께 돈다"고 적은 것은 Hideout Extensions(미설치)·Fika 뿐이다.
- 한국어 없음 → F12 사전을 새로 쓴다.

### 2-2. 14-B Hideout Last Stand (`jrs.hideoutsiege`, 게임+서버, **결속**)

메뉴에 습격 알림이 뜨고, 자기 은신처를 무대로 파도처럼 몰려오는 적을 막는 레이드에 들어간다. 무기 거치대·마네킹의 실제 장비를 들고 싸운다.

- **지면 진짜 창고를 턴다**: 창고·무기 거치대·마네킹에서 비싼 것을 가져가고, 시설 일부를 부수고, 심하게 다친 채로 깬다.
  결속 표시 그대로 **프로필 백업 뒤 혼자** 넣고, 첫 습격은 일부러 지는 시험 없이 이기는 쪽으로 확인한다.
- 선행 3326(0.2.0 이상) — 14-A 를 먼저 넣는 이유다. 방어물(모래주머니·바리케이드·거치 기관총)도 3326 의 꾸미기 화면에서 놓는다.
- 마지막 웨이브의 Black Division 은 MoreBots(패처)·WTT CommonLib·WTT ContentBackport·SAIN 이 필요하다. 설치본에
  `BepInEx/patchers/MoreBotsPrepatch`·`BlackDiv`·`WTT-ContentBackportPatcher`, WTT CommonLib 3.0.7, SAIN, 2511 Black Division Home 이 **있다**.
  Hideout Extensions 는 없으므로 두 번째 침입로는 비상 벽 문이 된다(페이지 설명대로).
- 함께 시험했다는 목록에 HollywoodFX·ORBIT 이 있고, 둘 다 우리 설치본에 있다.
- **댓글**: 0.1.0 때 "Manimal 의 Icebreaker·ISB·Skills Extended 와 문제가 있다"는 보고가 있었고 제작자가 고치겠다고 답했다. 셋 다 우리 설치본에 있으므로
  설치 뒤 기동 로그에서 이 셋의 오류를 따로 센다.
- **곧 1.0.0** — 제작자가 "다음 주 수요일 전에 1.0.0, 그때 번역용 JSON 폴더를 넣는다"고 적었다. **받는 날 다시 조회해** 1.0.0 이 나와 있으면 그것을 받는다.
  지금 0.3.1 에는 번역 파일이 없다.
- 설정: F12 `Tushonka: Hideout Last Stand`, 빈도·창고 상자 크기는 `SPT_Runtime\user\mods\HideoutSiege.Server\config.json`.
- 단축키: F7(허락)·F8(거절)은 **Fika 협동에서 남의 장비를 집을 때만** 뜬다. 우리는 혼자라 F7(Breakout Outlines·Field Attachments)·F8(Scope Rangefinder·Field Attachments)과 부딪힐 자리가 없지만, 설치 뒤 `.cfg` 에서 다시 본다.
- 우리 STM(PIT Fireteam 포크) 팀원은 습격에 들어오지 않는다(댓글에서 제작자가 PIT 는 아직 안 해 봤다고 답했다).

### 2-3. 14-C Satellite (`com.fums.satellite`, 게임+서버)

레이드를 브라우저에 실시간 지도로 띄운다(`https://127.0.0.1:6969/satellite/`, 게임 안 `Left Alt+M`). 봇·탈출구·전리품·잠긴 문과 열쇠·퀘스트 표시를 보여 주고, 읽기만 한다.

- **Dynamic Maps 와 같이 쓰기를 권하지 않는다**고 페이지에 적혀 있다. 우리는 Dynamic Maps 1.2.1·DynamicMaps Extended 2.5.2 에 더해
  **이 저장소가 만든 `DynamicMapsFactionMarkers`** 가 Dynamic Maps 위에서 돈다. 그래서 **Dynamic Maps 는 빼지 않는다.**
  Satellite 는 다른 화면용 웹 지도이고 게임 안 지도는 Dynamic Maps 로 둔다. 둘 다 켠 채 레이드 프레임과 기동 로그를 본다. 문제가 나오면 그때 고른다.
- **별도 포트를 열지 않는다** — 6969 안의 `/satellite/` 다. 휴대폰·태블릿에서 보려면 Fika 가 서버를 망에 열어야 한다고 적혀 있어, 우리는 **같은 PC 의 두 번째 모니터**로만 쓴다.
- **맵 애드온**: 기본 zip 은 바닐라 맵만 담는다. 우리 설치본의 포트 맵에 맞는 애드온 4개를 함께 받는다.

  | 애드온 | 크기 | 우리 맵 |
  | --- | ---: | --- |
  | [201 Interchange Backport](https://sp-mod.com/addon/201) 1.0.0 | 165MB | 3025 Manimal Interchange Expansion Backport |
  | [203 Lighthouse Backport](https://sp-mod.com/addon/203) 1.0.0 | 43MB | 3022 Manimal Lighthouse Rework Backport |
  | [202 The Lab (Office) Backport](https://sp-mod.com/addon/202) 1.0.0 | 8.7MB | 2937 Manimal Labs Office Backport |
  | [200 Icebreaker](https://sp-mod.com/addon/200) 1.0.0 | 5.6MB | 2866 Manimal Icebreaker Backport |

  Terminal·Factory Classic·Halloween 2026(Labs·Factory) 애드온은 해당 맵이 설치본에 없어 받지 않는다. Map Variants(3040)로
  같은 맵의 바닐라·포트를 골라 들어가도 Satellite 가 맞는 쪽을 스스로 고른다고 적혀 있다.
- 전리품 값 색은 Better Flea 가격을 쓴다고 하는데 Better Flea 는 설치돼 있지 않다 — 그때는 기본 가격으로 칠한다.
- SAIN·ORBIT 을 쓰면 봇이 무엇을 하는지 보여 준다. 둘 다 설치돼 있다.
- 10개 언어 번역이 있다. 한국어가 있는지는 해제본에서 센다(`mod-may-ship-own-korean`).
- 단축키 `Left Alt+M` 은 설치본 `.cfg` 에서 겹치는 것이 없다.

### 2-4. 14-D T∗∗∗∗∗ Override (`local.spt.practicemenu`, 게임 전용, 치트)

ESP·조준 보조·아이템 생성·무적·노클립·문 열기·탈출 조건 무시·공중 보급 요청을 한 창에 모은 치트 메뉴다. SVM 으로 되는 기능이 아니라
절차의 "SVM 으로 되는 치트는 넣지 않는다"에는 걸리지 않는다. 다만 이미 있는 치트성 모드(Tushonka-800 Combat Augmentation, Infinite Everything,
이 저장소의 AirdropCaller)와 하는 일이 일부 겹친다.

- **단축키가 이미 쓰이는 키에 걸린다** — 이게 이 모드의 첫 관문이다.
  - 주 메뉴 `Insert`: 설치본에서 **CSF**(`Change Currency Key = Insert`)·**TraceSix**(`Toggle Key = Insert`)가 쓴다.
  - 댓글에 기본 단축키가 **F6~F10** 이라는 언급이 있다. 우리 F6(SAIN 편집기)·F7·F8(편집기 둘)·F9·F10(ImpactCue·Prestige Perks)이 모두 쓰인다.
  - 단축키는 F12 가 아니라 `BepInEx\config\SPTFieldToolkit\` 의 JSON 이다(제작자 댓글). 해제본에서 기본값을 읽고, **첫 기동 전에 JSON 을 넣어** 빈 키로 옮긴다.
- **설치본의 Rippers Casino(3212)** 가, 이 모드가 F12 메뉴에서 사라지게 만든 원인 둘 중 하나로 댓글에 지목됐다(다른 하나 tylevos-tactical-service-control 은 미설치).
  설치 뒤 F12 에 이 모드가 보이는지 화면으로 본다.
- DLSS 를 켜면 ESP 상자가 어긋난다는 보고가 있다(제작자 확인). 6배율 이상 조준경에서 ESP 표시가 흔들린다는 보고도 있다.
- 제작자가 "한동안 마지막 업데이트일 수 있다, 5.0 은 안 올린다"고 적었다.

### 2-5. 14-E Dr K's SPZombies (`com.kennerz.spzombies`) — 선행 6 + 맵

라운드제 좀비 모드다. 자체 메뉴·맵이 있고 본 캐릭터·프로필에는 영향이 없다고 적혀 있다. **알파 / 개념 증명**이라고 제작자가 대문자로 밝혔다.

| 순서 | 모드 | 버전 | 크기 | 하는 일 | 우리 설치본에 걸리는 것 |
| --- | --- | --- | ---: | --- | --- |
| 1 | [3167 Hideout Uncensored](https://sp-mod.com/mod/3167) | 1.3.0 | 33KB | 은신처에서 무기를 꺼내 쏘고, 엎드리고, 넘고, 수류탄을 던진다(사격장 사본으로, 실제 장비는 안 쓴다). 서버 쪽은 "버리기를 메시지로 돌려받기" | 꺼내기 키 `J` — 설치본의 **Pinned Quest HUD**(`Toggle Key = J`)·Airburst(`J`)·Prestige Perks(`Use key = J`)와 같다. Infinite Everything 과 함께 돈다고 적혀 있다 |
| 2 | [3193 Tushonka Mapping SDK](https://sp-mod.com/mod/3193) | 1.9.3 | 121KB | 사용자 맵 로더 | "위치 데이터를 바꾸는 모드(LotsOfLoot)와는 안 된다". LotsOfLoot 은 설치본에 **없다**. 다만 SVM·PityLoot 처럼 위치 값을 만지는 모드가 있어, 좀비 맵 로드에서 오류를 본다 |
| 3 | [3223 Pack A Punch Hideout Module](https://sp-mod.com/mod/3223) | 2.2.0 | 43MB | 은신처 옆방에 PaP 기계. **메카닉 퀘스트**로 연다 | 결속 표시는 없지만 **본 프로필에 퀘스트·아이템을 넣는다**. 2.2.0 에 "좀비 모드에서만" 옵션이 생겼다. WTT DoomArcade 와 자리가 겹칠 수 있다고 하나 DoomArcade 는 미설치. 2.1.1 의 레이드 중 끊김은 다음 판에서 고쳤다고 답했다 |
| 4 | [3226 ZM_Thundergun](https://sp-mod.com/mod/3226) | 1.2.0 | 22MB | 은신처 벽에서 935,000₽ 에 사는 무기 | 같은 "좀비 모드에서만" 옵션 |
| 5 | [3229 ZM_Raygun](https://sp-mod.com/mod/3229) | 1.2.0 | 26MB | 같은 방식의 레이건 | 같은 옵션 |
| 6 | [3351 Dr K's Perk Beverages](https://sp-mod.com/mod/3351) | 0.1.0 | 108MB | 퍽 음료 기계 — 지금은 좀비 모드용 | 혼자는 아무것도 안 한다 |
| 7 | **3352 SPZombies** | 0.1.2 | 343MB | 본체 | 0.1.0 에서 LotsOfLoot 충돌(우리는 미설치). 댓글에 "일반 레이드보다 FPS 30~50% 낮고 3~5초마다 끊긴다", 제작자 "성능은 아직 잡는 중" |
| 8 | [애드온 193 ZM_Stronghold](https://sp-mod.com/addon/193) | 0.1.0 | **1.51GB** | **첫 맵** — 본체에는 맵이 없다 | 애드온 탭이 0.1.0 호환으로만 적지만 0.1.2 변경 기록이 "Stronghold 는 그대로 돈다, 옮길 것 없다"고 밝혔다 |

- **판단**: 좀비 모드 하나를 위해 선행 6개가 본 게임 은신처(무기 꺼내기·PaP 기계·벽 무기 판매·메카닉 퀘스트)를 바꾼다.
  셋(3223·3226·3229)의 "좀비 모드에서만" 옵션을 **켜고** 들이면 본 프로필에 퀘스트·아이템이 들어오지 않는다. 옵션 이름과 기본값은 해제본에서 확인하고,
  **첫 기동 전에 `.cfg` 를 넣어** 켠다(기본값이 "본 게임에도"라면 첫 기동에서 이미 퀘스트가 생긴다).
- Hideout Uncensored 와 14-A 는 둘 다 은신처 안을 바꾼다. 서로 시험한 기록이 없으므로 14-A·14-B 를 확인한 뒤에 넣는다.
- 받을 양 2.0GiB 의 80% 가 Stronghold 맵 하나다. 호스트는 GitHub 이다. 느리면 `download-ranged.mjs` 로 끊어 받는다(`one-stream-throttled-use-ranges`).

### 2-6. Skill Cap 100 — 넣지 않는다

스킬 상한을 51 → 100 으로 올리는 결속 모드(서버+게임). Skill Distribution 은 자동으로 맞추고 SVM 과도 된다고 적혀 있다.

- **설치본의 Skills Extended 3.1.4(2383) 제작자 cj 가 이 모드 댓글에 직접 "다음 업데이트에서 Skills Extended 와 비호환으로 표시한다,
  높은 스킬 레벨에서 많은 것이 깨진다"고 적었다.** 다른 사용자는 "설치 후 스킬이 10 으로 굳었다"고 보고했다.
- 우리는 Skills Extended·Skill Distribution·Repair Skill Extension 셋이 스킬을 만진다. 결속이라 빼도 프로필에 51 넘는 값이 남는다.
- 그래서 매니페스트에 넣지 않았다. **넣으려면 Skills Extended 를 빼는 결정이 먼저**다.

## 3. 순서와 이유

1. **14-A Hideout Interiors** — 작고 게임 전용이며, 14-B 의 선행이다.
2. **14-B Hideout Last Stand** — 결속이라 혼자 넣는다. 14-A 가 은신처에서 멀쩡한 것을 본 뒤에 넣는다.
3. **14-C Satellite** — 은신처와 무관하다. 1GB 라 받기는 14-A·B 와 같이 시작하되 설치는 따로 한다.
4. **14-D T Override** — 단축키 JSON 을 먼저 만들어 넣는다. 첫 기동에서 F12·Insert 겹침을 화면으로 본다.
5. **14-E SPZombies** — 맨 끝에 둔다. 선행 1→6 을 넣고, 본체와 맵은 마지막이다. 가장 무겁고 알파라서, 앞 단계가 다 멀쩡한 기준선 위에서 넣는다.

`download-stage.mjs` 는 작은 파일부터 받는다. 그래서 하나의 매니페스트로도 14-A~D 가 먼저 받아지고, 2GB 짜리 맵이 맨 뒤에 남는다(`slow-downloads-go-last`).

## 4. 절차 — `doc/PROCEDURE.md` 그대로

### 4-1. 받기

1. **받는 날 다시 조회한다**: `node tools/survey-candidates.mjs '{"14":[3326,3353,3373,3317,3352,3167,3193,3223,3226,3229,3351]}' <out>`.
   특히 3353 은 1.0.0 이 곧 나온다. 버전이 바뀌었으면 매니페스트의 `version`·`size`·`link` 를 고친다.
   조회 실패를 "변화 없음"으로 읽지 않는다(`lookup-failure-is-not-a-negative`).
2. `node tools/download-stage.mjs stage14` → `downloads/stage14/`, 결과 `doc/download/download-results-stage14.json`. 바이트 수를 매니페스트와 대조한다.
   504 면 `gh` 로 받는다(`forge-download-may-504-use-gh`).
3. 해제: `/c/Windows/System32/tar.exe -xf <파일> -C extracted/stage14/<id>__<이름>` (**절대경로**).

### 4-2. 해제본에서 먼저 볼 것 (설치 전)

| 모드 | 볼 것 |
| --- | --- |
| 전부 | 압축 뿌리 구조가 `BepInEx/…`·`SPT_Runtime/…` 인지. 뿌리에 섞인 README 는 치운다. `node tools/check-installed-vs-extracted.mjs` 로 **덮어쓸 파일 0** |
| 14-A | `HideoutBrowser.exe` 위치·크기, 한국어 유무 |
| 14-B | 서버 `config.json` 의 습격 빈도·창고 상자 크기 기본값, 번역 파일(1.0.0 이면) |
| 14-C | 애드온 4개가 같은 `BepInEx\plugins\Satellite` 아래 서로 다른 파일만 넣는지(애드온끼리·본체와 겹침), `config.jsonc`, 한국어 유무 |
| 14-D | `SPTFieldToolkit` 단축키 JSON 의 기본값 — Insert·F6~F10 이면 빈 키로 바꿀 JSON 을 만든다 |
| 14-E | 3223·3226·3229 의 "좀비 모드에서만" 설정 이름·기본값, Hideout Uncensored 의 `J`, Stronghold 가 `BepInEx\plugins\SPZombies_Maps\ZM_Stronghold` 로 가는지 |

### 4-3. 넣기

- 전제: 게임·서버가 꺼져 있다. **내가 끄지 않는다** — 켜져 있으면 사용자에게 묻는다(`never-stop-server-or-game-unasked`).
- 프로필 백업: `E:\Spt\SPT_Runtime\user\profiles\*` → `backup/profiles-before-stage14/` (14-B 앞에서 한 번 더: `profiles-before-stage14-B/`).

```powershell
E:\Spt_Mods\tools\install-and-test.ps1 -Stage stage14 -Pattern '3326__*'      -Tag 'stage14-A-3326-HideoutInteriors'
E:\Spt_Mods\tools\install-and-test.ps1 -Stage stage14 -Pattern '3353__*'      -Tag 'stage14-B-3353-HideoutLastStand'
E:\Spt_Mods\tools\install-and-test.ps1 -Stage stage14 -Pattern '3373__*'      -Tag 'stage14-C-3373-Satellite' -SkipTest
E:\Spt_Mods\tools\install-and-test.ps1 -Stage stage14 -Pattern 'addon201__*'  -Tag 'stage14-C-addon201-SatInterchange' -SkipTest
E:\Spt_Mods\tools\install-and-test.ps1 -Stage stage14 -Pattern 'addon203__*'  -Tag 'stage14-C-addon203-SatLighthouse' -SkipTest
E:\Spt_Mods\tools\install-and-test.ps1 -Stage stage14 -Pattern 'addon202__*'  -Tag 'stage14-C-addon202-SatLabOffice' -SkipTest
E:\Spt_Mods\tools\install-and-test.ps1 -Stage stage14 -Pattern 'addon200__*'  -Tag 'stage14-C-addon200-SatIcebreaker'
E:\Spt_Mods\tools\install-and-test.ps1 -Stage stage14 -Pattern '3317__*'      -Tag 'stage14-D-3317-TOverride'
# 14-E: 3167 → 3193 → 3223 → 3226 → 3229 → 3351 → 3352 → addon193, 하나씩
```

- `install-and-test.ps1` 은 `-Pattern`(`Get-ChildItem -Filter`)에 맞는 폴더를 **첫 하나만** 넣는다. `[0-3]` 같은 범위도 받지 않는다 — 애드온은 한 줄에 하나씩 쓴다.
  14-C 는 마지막 줄에서만 서버 시험을 돌린다.

- **한 묶음 넣고 멈춘다.** 다음 묶음은 앞 묶음을 게임에서 확인한 뒤에 넣는다.
- DLL 이 잠겨 있으면 안 깔린 것이다(`locked-dll-means-not-installed`).
- 14-D·14-E 는 **첫 기동 전에** 바꾼 설정 파일(단축키 JSON, "좀비 모드에서만" `.cfg`)을 넣는다. 그 파일도 설치 기록에 적는다.

### 4-4. 기록·검사 (묶음마다)

1. `node tools/gen-installed-mods.mjs` → `doc/INSTALLED-MODS.md`
2. `node tools/check-install-records.mjs` → **`OK — 0/0`**. 다른 모드 기록에 구멍이 나도 유실로 단정하지 않는다(`install-records-can-misattribute`).
3. `node tools/check-conflicts.mjs`
4. 한글화: `node tools/check-lang-coverage.mjs`·`check-plugin-locales.mjs`. 새 F12 항목에 사전을 쓰고, **다른 모드 사전에 난 구멍**도 센다(`lang-dicts-leak-as-mods-arrive`).
   T Override·Hideout Interiors 처럼 자체 창을 그리는 모드는 `_dll-*.json` 대상인지 본다(`src/SptKoreanConfig/README.md`). 대상으로 넣으면 `Plugin.cs` 에 `SoftDependency` 를 단다.

### 4-5. 확인

| 무엇 | 어떻게 |
| --- | --- |
| 서버 모드 수 | `ModLoader: 총 N 개`. 지금 181. 서버 쪽을 싣는 모드만큼 는다(해제본의 `SPT_Runtime\user\mods` 폴더 수로 미리 센다) |
| 게임 플러그인 수 | `N plugins to load`. 지금 252. **넣은 DLL 수와 같지 않다**(`plugin-count-is-not-dll-count`) |
| 오류 | 게임 `[Error`/`[Fatal`, 서버 `Could not add` **고유 id 수를 날짜와 함께**. 14-B 는 Icebreaker·ISB·Skills Extended 관련 줄을 따로 센다 |
| 14-A | 은신처 전경에 꾸미기 버튼, 물건 하나 옮기고 다시 들어와 그대로인지 |
| 14-B | 메뉴에 습격 알림 → 방어 시작 → 첫 웨이브까지. 이겨서 창고 상자 물건이 실제 창고에 들어오는지 |
| 14-C | 레이드 중 `https://127.0.0.1:6969/satellite/` 에 내 위치·봇. 포트 맵 4개에서 맞는 그림. Dynamic Maps 와 함께 켠 레이드 프레임 |
| 14-D | F12 에 모드가 보이는지(Rippers Casino), 바꾼 단축키로 메뉴가 열리고 CSF·TraceSix·SAIN·F8 편집기가 그대로인지 |
| 14-E | 좀비 메뉴 → Stronghold 로드 → 1라운드. 본 프로필 메카닉 퀘스트 목록이 그대로인지("좀비 모드에서만") |

화면으로만 드러나는 것(꾸미기 화면, 습격 알림, 웹 지도, 메뉴)은 스크린숏을 남긴다. 로그만으로 판정하지 않는다(`coverage-is-not-display`).

### 4-6. 되돌리기

- 묶음 하나: `doc/install-records/installed-<Tag>.txt` 의 파일만 지우고 기록을 `removed/` 로 옮긴다.
- 14-B 는 결속이다. 습격에서 진 뒤에는 창고가 이미 털렸으므로 `backup/profiles-before-stage14-B/` 로 돌린다.
- 14-A 의 배치 파일(`jrs.hideoutinteriors.*.json`), 14-E 의 메카닉 퀘스트·PaP 아이템은 모드를 지워도 남는다. 지울 때 함께 본다.

## 5. 사용자가 정할 것

| 무엇 | 권장 | 다른 길 |
| --- | --- | --- |
| Satellite 와 Dynamic Maps | **둘 다 둔다** — 우리 `DynamicMapsFactionMarkers` 가 Dynamic Maps 위에 있다 | Dynamic Maps(+Extended·우리 마커)를 뺀다 |
| SPZombies 묶음 | **넣는다면 맨 끝**, PaP·무기 셋은 "좀비 모드에서만" | 넣지 않는다(2GB·알파·은신처를 바꾸는 선행 6개) |
| Skill Cap 100 | **넣지 않는다** | Skills Extended 를 빼고 넣는다 |

## 6. 함께 보는 문서

- 절차: `doc/PROCEDURE.md`
- 앞선 설치 계획 형식: `doc/plans/2026-11-15-prestige-mods-install-plan.md`
- 한글화 장치: `src/SptKoreanConfig/README.md`

## 7. 받기·해제 결과 (2026-11-17 11:36~11:39)

**설치는 아직 하지 않았다.** 게임(PID 42100)과 서버(PID 45256)가 켜져 있어 사용자에게 물은 뒤에 넣는다.

### 7-1. 받기

- 받기 직전 11개를 다시 조회했다(조회 실패 0) — 버전 변화 없음. 3353 도 아직 0.3.1 이다.
- `node tools/download-stage.mjs stage14` — **16/16, 바이트 일치 16**, 합계 3,265,759,205 바이트, 약 3분 30초. 전부 GitHub(`release-assets.githubusercontent.com`, 3317 만 `raw.githubusercontent.com`).
  결과는 `doc/download/download-results-stage14.json`.
- 해제: `/c/Windows/System32/tar.exe` 절대경로로 16개 모두 성공.

### 7-2. 겹침

해제본 16개 폴더(파일 1,644개)를 설치본과 서로에 대조했다.

| 검사 | 결과 |
| --- | --- |
| 설치본에 이미 있는 경로(덮어쓰기) | **0** |
| 모드끼리 같은 경로 | **0** — Stronghold 맵은 3193 의 `CustomMapLoader` 폴더 아래에 넣지만 같은 파일은 없다. Satellite 애드온 넷은 `maps/<맵>~backport` 처럼 본체와 다른 폴더다 |
| `BepInEx/`·`SPT_Runtime/` 밖의 파일 | **0** — 3317 의 뿌리 `docs/`(안내서 14개)는 `extracted/stage14/_docs-3317/` 로 치웠다 |

### 7-3. 해제본에서 확인한 것과 첫 기동 전에 넣은 파일

| 모드 | 확인한 것 | 넣은 파일 (설치 기록에 함께 남는다) |
| --- | --- | --- |
| 14-A 3326 | `browser/HideoutBrowser.exe` + WebView2 DLL 3개. 한국어 없음 | - |
| 14-B 3353 | 서버 쪽은 DLL 하나 — `config.json` 은 첫 기동에 생긴다. 번역 파일 없음(1.0.0 에서 넣는다고 했다) | - |
| 14-C 3373 | 게임 `Satellite.Client.dll`, 서버 `Satellite.Server.dll`·`web/`·바닐라 맵 13장. **한국어 `web/lang/ko.js` 가 있다** — 707항목 중 원문 그대로 16(Customs·Factory·Woods·Shoreline·Reserve·The Lab 맵 이름 6 + 고유명사). 맵 이름이 "인터체인지·등대"만 한국어라 섞여 있다 | - |
| 14-D 3317 | DLL 역컴파일로 기본 단축키 확인: `OpenSettings=Insert`·`ToggleEnemyESP=F6`·`ToggleLootESP=F7`·`ToggleAimbot=F8`·`ReloadConfig=F9`·`Panic=F10`·`AimHold=Mouse4`. `config.json` 이 있으면 덮어쓰지 않는다 | `BepInEx/config/SPTFieldToolkit/config.json` — **메뉴 PageUp, 적 ESP Keypad7, 전리품 ESP Keypad9, 조준 Keypad6, 패닉 PageDown, 다시 읽기 None**. 설치본 `.cfg` 전체에서 안 쓰이는 키를 골랐다(Home=STM, End=두 모드, Keypad1~5·8=SetSpeed·DynamicMaps 등) |
| 14-E 3223·3226·3229 | 세 모드 모두 F12 `1. Normal gameplay / Show in normal gameplay`(기본 **true**). 끄면 서버 경로(`/pap/normalgame` 등)로 넘기고 "게임과 서버를 다시 켠 뒤" 반영. 첫 기동에 "YOUR NORMAL GAMEPLAY" 선택 창도 뜬다 | `BepInEx/config/com.kennerz.paphideoutmodule.cfg`·`com.kennerz.thundergun.cfg`·`com.kennerz.raygun.cfg` — `Show in normal gameplay = false`. 선택 창이 뜨면 거기서도 끈다 |
| 14-E 3167 | `Draw / holster key = J`(`com.trappuss.hideoutuncensored`). 은신처 전용이고 슬롯 키로도 꺼낸다 — **바꾸지 않았다**. Pinned Quest HUD(`J`)와 함께 눌리는지 화면으로 본다 | - |
| 14-E 3352 | 본체 1,398파일(음성 1,097). 맵은 없다 | - |

- 3223 서버 DLL 은 역컴파일이 실패했다(참조 해석). 클라이언트 쪽 코드만 읽었다.
- 내려받은 JS(`ko.js`)를 `require` 로 읽으려던 것은 막혔다 — 받은 코드는 실행하지 않고 글자로만 셌다.

## 8. 14-A Hideout Interiors 설치 결과 (2026-11-17 12:29~12:50)

- 사용자 허락으로 서버를 끄고 설치했다. 프로필 11개를 `backup/profiles-before-stage14/` 에 떴다(해시 불일치 0).
- `install-and-test.ps1 -Pattern '3326__*' -Tag 'stage14-A-3326-HideoutInteriors'` — **10파일**, 덮어쓰기 0.
  서버 시험: `ModLoader: 총 181 개`(게임 전용이라 그대로), `Loaded self-signed certificate`, `Could not add` 고유 48(기준선과 같다).
- 기록: `gen-installed-mods` 308 → **309개**, `check-install-records` **`OK — 0/0`**.
  생성기가 14차를 몰라서(`stageTitle`·단계 목록에 13까지만) 3326 이 목록에 안 올라왔다 — 14 를 더했다.
  애드온은 `mod/addon201` 처럼 잘못된 링크가 나와 매니페스트에 `homepage`(`/addon/<번호>`)를 적었다.
- 게임 기동(12:35·12:46): `253 plugins to load`(252 → 253), `Loading [JRS-HideoutInteriors 1.0.0]`, `Hideout Interiors 1.0.0 loaded.`
  `[Error` 1줄은 알려진 DirectReload·Fika 건, `Excluding 'HideoutInteriors.Fika.dll'` 은 Fika 미설치 경고다.
- **화면**: 은신처 전경 아래 버튼 줄의 다섯째에 꾸미기 버튼이 붙었고, 누르면 DECORATE 패널(분류·물건 그림·검색, MOVE/TURN/SIZE/HIDE/DONE)이 뜬다.
  아무것도 옮기지 않고 DONE 으로 나왔다 — 배치 파일 `jrs.hideoutinteriors.decor.<프로필>.json` 은 2바이트로 비어 있다.
- 한글화: F12 사전 `lang/jrs.hideoutinteriors.json` 15항목. `check-lang-coverage` **5211/5211**(사전 189개), 기동 로그 "사전에 없음" **36** 그대로.
  **꾸미기 패널 글자(DECORATE·분류 이름·버튼)는 영문** — 모드 DLL 안 리터럴이라 `_dll-*.json` 대상이다. 다음 묶음과 따로 한다.
- 첫 기동(12:32) 뒤 화면이 로딩 표시에서 오래 멈춰 있었고 "Fatal error" 제목의 창이 잠깐 앞에 있었다. 사용자가 게임을 껐고, 로그는
  12:44:55 에 패치를 차례로 끄는 **정상 종료**였다. 두 번째 기동(12:46)은 메인 메뉴·은신처까지 정상이었다. 원인은 §12 에서 찾았다 — computer-use 가 게임을 하나 더 띄운 것이었다.

## 9. 14-B Hideout Last Stand 설치 결과 (2026-11-17 13:30 무렵)

- 설치 직전 재조회: 여전히 0.3.1(1.0.0 아직 없음).
- 게임은 창 닫기로 정상 종료(TraumaCore 등 패치를 차례로 끄는 줄 확인), 서버 종료. 프로필 11개를 `backup/profiles-before-stage14-B/` 에 떴다(불일치 0).
- `install-and-test.ps1 -Pattern '3353__*' -Tag 'stage14-B-3353-HideoutLastStand'` — 파일 29 + 서버가 만든 `HideoutSiege.Server\config.json`(NEWFILE) = **기록 30줄**.
  서버 시험: `ModLoader: 총 182 개`(181 → 182), `Hideout Last Stand: the hideout is ready to be attacked.`, `Loaded self-signed certificate`, `Could not add` 고유 48(기준선).
- 서버 설정 기본값: 습격 간격 3~8시간, 진 뒤 2시간, `InvadedOnFirstRun: true`, 창고 상자 10×6.
- 기록: 설치 목록 **310개**(결속 표시 붙음), `check-install-records` **`OK — 0/0`**.
- 게임 기동: `254 plugins to load`(253 → 254), `Loading [Tushonka: Hideout Last Stand 0.3.1]`, `BigBrain found`, 방어물 셋(모래주머니·강철 바리케이드·철조망)이
  **Hideout Interiors 꾸미기 패널에 들어갔다**는 줄, `Hideout Last Stand loaded.` `[Error` 는 알려진 DirectReload·Fika 1줄뿐.
- **화면**: 메인 메뉴 위에 "HIDEOUT IS BEING INVADED — START THE DEFENSE / LATER" 알림이 떴다. **방어는 시작하지 않았다** — 지면 실제 창고가 털린다.
- 한글화: F12 사전 `lang/jrs.hideoutsiege.json` 21항목(보스 이름은 공식 kr — 르샬라·킬라·타길라·세니타·글루하, 우리 사전 표기 — 구니즈·블랙 디비전, 공식 표기가 없는 카반·콜론타이는 음역).
  모드가 들어오며 **No Barrel Shadow 에 `○ NGAL` 두 항목이 새로 생겨** 함께 채웠다(`lang-dicts-leak-as-mods-arrive`). `check-lang-coverage` **5234/5234**(사전 190개).
  메뉴 알림·습격 화면 글자는 영문 — 제작자가 1.0.0 에 번역 JSON 폴더를 넣겠다고 했다.
- 단축키: `Leave key`(이긴 뒤 바로 나가기) 기본 **F10** 이 ImpactCue 설정 창 키와 같다. 이긴 뒤에만 쓰는 키라 그대로 두고 사전 설명에 적었다. F7·F8 은 협동 전용.
- 같은 기동에서 UI Fixes `Modify Weapons In Raid = Always` 가 반영됐다. 로그에 DWV 의 "UI-Fixes blocks" 줄은 없다(로딩 화면은 찍지 못했다).

## 10. 14-B 첫 습격 — 사용자가 직접 플레이 (2026-11-17 13:19~13:24)

| 무엇 | 결과 |
| --- | --- |
| 진행 (게임 로그) | 맵 `Suburbs`(은신처 복사본, 시설 28/28, 방어 0%) → 1단계 구성 "Scav 2, Scav 2 + Raider 1, 타길라" → 웨이브 1·2·3 → **`Won, wave 3/3`** → `Leaving the raid: Survived` |
| 서버 | `defended at tier 1, wave 3, 6 kills`. 무기 거치대에 장비가 없어 `0 things brought in`, `everything from your racks is still there` |
| 오류 | 습격 구간 게임 로그 `Exception` **0**, Unity 로그 예외 0. `[Error` 는 `SPTBattleAmbience` 가 `suburbs` 맵 설정을 못 찾은 1줄 — Icebreaker·Labyrinth 와 같은 종류(그 맵에서만 전투 소음이 빠진다). 경고는 ISB·UNTARGH·RUAF·ORBIT·사다리가 처음 보는 맵 `Suburbs` 설정이 없다고 적은 것 |
| 서버 오류 | 이번 기동 구간 `Could not add` 고유 **48**, 다른 오류 0 — 기준선과 같다 |
| 프로필 (백업 대조) | 아이템 2186 → 2233. 사라진 3개는 쏜 탄약 묶음 둘과 갱신된 BEAR 군번줄(같은 종류가 새 id 로 들어옴) — **털린 것 없음**. 새 50개는 배낭의 전리품(타길라 에디션 플레이트 캐리어, 연구소 출입 카드 2, 수류탄·탄창·의약품 등)과 장전. 화폐·은신처 시설 레벨·체력 그대로, 경험치 412,314 → 427,193 |

- 습격 중 `Kept you out of past the cultist gate` 45회 — 플레이어가 무대 밖(교단원 방 쪽)으로 나가지 못하게 되돌린 기록이다.
- 기다리던 타길라가 정문 안쪽에 있다가 대기 자리로 되돌려졌다는 경고 1줄(모드 자체 보정).

## 11. 14-C Satellite + 맵 애드온 4 설치 결과 (2026-11-17 13:30~13:45)

- 게임 정상 종료·서버 종료 뒤 설치. 본체와 애드온 셋은 `-SkipTest` 로 복사만, 마지막 Icebreaker 애드온에서 서버 시험.
  | 기록 | 줄 |
  | --- | ---: |
  | `stage14-C-3373-Satellite` | 65 (파일 64 + 서버가 만든 `config.jsonc`) |
  | `stage14-C-addon201-SatInterchange` | 3 |
  | `stage14-C-addon203-SatLighthouse` | 3 |
  | `stage14-C-addon202-SatLabOffice` | 3 |
  | `stage14-C-addon200-SatIcebreaker` | 3 |
- **기록을 하나 옮겼다**: 서버 시험에서 생긴 `Satellite\config.jsonc` 가 시험을 돌린 Icebreaker 애드온 몫(NEWFILE)으로 적혔다. 본체가 만든 파일이라
  본체 기록으로 옮겼다(`install-records-can-misattribute`). 옮긴 뒤 `check-install-records` **`OK — 0/0`**.
- 서버 시험: `ModLoader: 총 183 개`(182 → 183), `[Satellite] Open it at https://127.0.0.1:6969/satellite/`, `Loaded self-signed certificate`, `Could not add` 고유 48.
  "Only this PC can open the map" — Fika 없이 휴대폰에서는 못 연다(계획대로 같은 PC 의 두 번째 모니터용).
- **모드 카드**: 서버 기동 로그 `[ModCardKorean] 사전에 없어 원문으로 남은 카드 1 개: Satellite (/satellite)` → `src/ModCardKorean/mod-cards.ko.json` 에 설명을 더해 설치본에 넣었다.
  `check-mod-card-locale` **16/16**.
- 설치 목록 **315개**(본체 + 애드온 4).
- 게임: `255 plugins to load`(254 → 255), `Loading [Fums-Satellite 1.0.1]`, `Bot details: SAIN found, ORBIT found, COMET not found`. `[Error` 는 알려진 DirectReload 1줄.
  Dynamic Maps·DynamicMaps Extended 도 함께 올라왔다.
- F12 사전 `lang/com.fums.satellite.json` 3항목 → `check-lang-coverage` **5237/5237**(사전 191개).
- 웹 페이지 `https://127.0.0.1:6969/satellite/` 는 200(34KB)으로 응답한다. 내장 브라우저는 SPT 의 자체 서명 인증서를 신뢰하지 않아 화면을 열지 못했다.
  **레이드 중 실제 지도는 아직 보지 않았다** — 게임 안 `Left Alt+M` 으로 연다(처음엔 인증서 경고에서 "고급 → 계속").

## 12. 14-D T Override 설치 결과 (2026-11-17 13:50~14:20)

- 게임 정상 종료·서버 종료 뒤 설치. `install-and-test -Pattern '3317__*' -Tag 'stage14-D-3317-TOverride'` — **2파일**(DLL + 미리 만든 단축키
  `BepInEx\config\SPTFieldToolkit\config.json`), 서버 183 그대로(게임 전용), `Could not add` 고유 48. 설치 목록 316개, 기록 `OK — 0/0`.
- **첫 기동에서 스스로 꺼졌다**: `[Error :Tarkov Override] Solo SPT only: Fika detected, toolkit disabled.`
  - 역컴파일: 로드된 플러그인 중 **GUID 에 `fika` 가 든 것이 하나라도 있으면** 끈다(`Chainloader.PluginInfos` 를 훑는다).
  - Fika 본체는 없다. 걸린 것은 **LootNet(2679) 의 `LootNetFika.dll`** — GUID `com.20fpsguy.LootNet.fika`, 이름 `LootNet.Fika 1.0.9`.
    Fika 가 없으면 `Fika not installed` 한 줄만 찍고 아무것도 하지 않는다(역컴파일 `Awake` 확인). 로드된 플러그인 중 이름에 Fika 가 든 것은 이것 하나였다.
  - 조치: `LootNetFika.dll` → **`LootNetFika.dll.disabled`** 로 이름을 바꿔 BepInEx 가 읽지 않게 했다. LootNet 기록(`installed-stage10-G-2679-LootNET.txt`)의
    그 줄도 같은 이름으로 고쳤다. **LootNet 을 업데이트하면 이 DLL 이 다시 들어와 T Override 가 또 꺼진다** — 그때 다시 이름을 바꾼다.
- 다시 켠 기동: `Tarkov Override 1.4.3 ready`, `Hotkeys loaded: ...SPTFieldToolkit\config.json`(미리 넣은 파일 해시 그대로). 플러그인 255
  (T Override +1, LootNet.Fika −1). `PageUp` 으로 메뉴가 열리는 것을 화면으로 봤다(창 아래 "PageUp / Esc: Close"). 메뉴 안 글자는 영문.
- **Rippers Casino 와 함께여도 F12 에서 사라지지 않았다** — 목록에 있고 236개 항목이 열린다.
- F12 사전 `lang/local.spt.practicemenu.json` **236항목**(섹션 34). 조준 보조·레짓·레이지 세 묶음이 같은 키를 되풀이해 키별 번역에 섹션 예외를 얹어 만들었다.
  `check-lang-coverage` **5473/5473**(사전 192개), 기동 로그 "사전에 없음" 272 → **36**, F12 화면에 "치트 메뉴 (Tarkov Override)"·한국어 항목.
  - Better BepInEx 창은 설정 목록을 한 번만 만들어서, **사전을 고친 뒤 F12 를 다시 열어도 반영되지 않았다**(재적용 로그 없음). 게임을 다시 켜야 한다.
- §8 의 "첫 기동 멈춤·Fatal error 창"은 **원인을 찾았다**: 화면을 앞으로 가져오려고 쓴 computer-use `open_application` 이 게임을 하나 더 띄웠고,
  두 번째 게임이 "Another instance is already running" 오류 창을 냈다(대화상자를 찍어 확인). 게임·모드 문제가 아니다.
