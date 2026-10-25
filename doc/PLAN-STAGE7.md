# 7차 설치 계획 — 작고 가벼운 것부터 (2026-10-25)

[CANDIDATES-STAGE7.md](CANDIDATES-STAGE7.md)의 65개를 **용량과 되돌리기 쉬움** 순으로 줄 세운 계획이다. 밸런스는 판단 기준에서 뺐다. 절차는 [PROCEDURE.md](PROCEDURE.md)를 그대로 따른다.

## 줄 세운 기준

1. **비결속이 먼저다.** 결속(프로필 결속)은 플레이 후 제거하면 프로필이 깨질 수 있으니, 파일만 지우면 끝나는 것부터 넣는다.
2. **용량이 작은 것이 먼저다.** 문제가 났을 때 범위를 좁히기 쉽고, 되돌리는 비용도 작다.
3. **설치본과 역할이 겹치는 것은 뒤로.** 어느 쪽을 쓸지 게임에서 보고 정해야 한다.
4. **새 의존성을 끌고 오는 것과 대형 콘텐츠는 맨 뒤.** 넣을지부터 따로 정한다.

## 7-A — 비결속·소형 14개 (합계 314KB)

전부 파일만 지우면 되돌아간다. 한 번에 복사하고 마지막에 서버 테스트 한 번.

| 모드 | id | 버전 | 크기 | 분야 | 무엇이 달라지나 |
| --- | --- | --- | --- | --- | --- |
| [LMSP - Let Me Switch Plates](https://sp-mod.com/mod/2814) | 2814 | 1.0.1 | 3KB | 방어구 | 레이드 중 방어구를 벗지 않고 방어판만 교체 |
| [BiC - Bind in Container](https://sp-mod.com/mod/3190) | 3190 | 1.0.1 | 3KB | 방어구 | 보안 컨테이너 안 물건에도 단축키 지정 |
| [VisorEffectManager](https://sp-mod.com/mod/2429) | 2429 | 1.1.0 | 4KB | 방어구 | 바이저 시각 효과를 켜고 끈다 |
| [Manimal's Quick Cancel](https://sp-mod.com/mod/2969) | 2969 | 2.0.0 | 4KB | 탄환 | 소비 동작을 재장전 키로 즉시 취소 |
| [Item Limits Begone!](https://sp-mod.com/mod/2524) | 2524 | 1.1.0 | 5KB | 상인 | 레이드·인벤토리·플레아 등록 제한 해제 |
| [Quick Aim](https://sp-mod.com/mod/2985) | 2985 | 1.0.0 | 6KB | 파츠 | 앉기·엎드리기·거치 상태에서 조준 유지 시간 증가 |
| [ArmorInfoPlus](https://sp-mod.com/mod/3049) | 3049 | 1.0.0 | 9KB | 방어구 | 아이템 템플릿 ID 표시(조사·디버그용) |
| [Armor Expert](https://sp-mod.com/mod/3106) | 3106 | 3.1.0 | 10KB | 방어구 | 방어 장비·헤드셋의 숨은 수치 표시 |
| [Munitions Expert - Reloaded](https://sp-mod.com/mod/3006) | 3006 | 3.1.0 | 14KB | 탄환 | 탄약·폭발물의 숨은 수치 표시. 3106과 한 쌍 |
| [Unrestricted Armor Plate](https://sp-mod.com/mod/1878) | 1878 | 2.1.0 | 15KB | 방어구 | 아무 방어판이나 아무 조끼·리그에 장착 |
| [All In Weapon: Unslotted](https://sp-mod.com/mod/2576) | 2576 | 1.2.0 | 15KB | 파츠 | 슬롯·호환 제한 해제. 봇 무기 생성은 바닐라 유지 |
| [Change Helmet Visor](https://sp-mod.com/mod/2554) | 2554 | 3.0.0 | 46KB | 방어구 | 페이스실드·바이저가 그려지는 방식 전환 |
| [Foldables](https://sp-mod.com/mod/2422) | 2422 | 1.1.1 | 70KB | 방어구 | 배낭·조끼를 접어 자리를 줄인다 |
| [SOMF - SitOnMyFace](https://sp-mod.com/mod/3105) | 3105 | 0.1.2 | 109KB | 방어구 | 얼굴 슬롯에 장비 여러 개 겹쳐 착용. **0.1.x라 가장 어리다** |

겹침을 미리 본 것

- 3106 Armor Expert ↔ 설치된 ODT's Item Info — 둘 다 아이템 설명에 글을 덧붙인다. 같이 보면 길어질 수 있다.
- 2554 Change Helmet Visor ↔ 2429 VisorEffectManager — 서로 다른 쪽을 건드린다(그리는 방식 / 효과 on·off). 같이 넣고 확인한다.
- 2576 All In Weapon ↔ 설치된 Compatibility Highlighter — 호환 강조 표시가 "전부 호환"으로 보일 수 있다.

## 7-B — 비결속인데 설치본과 겹치는 것 (결정 후)

넣기 전에 어느 쪽을 쓸지 정해야 한다. 전부 비결속이라 바꿔 끼우기는 쉽다.

| 모드 | id | 크기 | 무엇과 겹치나 |
| --- | --- | --- | --- |
| [Easy Ammo Names](https://sp-mod.com/mod/1262) | 1262 | 16KB | 설치된 Ammo Stats·Caliber Under Ammo Name과 탄약 이름 표기. [Ammo Stats In Names](https://sp-mod.com/mod/2989)(2989)와도 택일 |
| [Quick Sell](https://sp-mod.com/mod/2955) | 2955 | 50KB | 설치된 QuickSellFlea를 대체하는 쪽 |
| [Live Flea Prices](https://sp-mod.com/mod/1131) | 1131 | 10KB | 플레아 가격 소스. 표기가 `~4.1.1`이라 단독 테스트 필요 |
| [Better Flea](https://sp-mod.com/mod/3177) | 3177 | 359KB | SVM 플레아 설정과. [Ultimate Flea](https://sp-mod.com/mod/2844)(2844, 결속)와 택일 |
| [Bela.s.Speedloader](https://sp-mod.com/mod/2458) | 2458 | 23KB | 설치된 Direct Reload·Continuous Load Ammo와 장전 속도 |
| [Infinite Everything](https://sp-mod.com/mod/3108) | 3108 | 56KB | SVM의 무한 옵션과 광범위하게 |
| [Instant Insurance](https://sp-mod.com/mod/2394) | 2394 | 38KB | **6차에 넣은 Realistic Insurance(2949)와 정면으로 어긋난다.** 하나를 끄는 전제로만 |

## 7-C — 결속·소형 (프로필 백업 후 하나씩)

파일은 작지만 결속이다. 한 단계에 하나씩, 매번 프로필 백업 + 서버 테스트. 위에서부터 효과가 큰 순서다.

| 순서 | 모드 | id | 크기 | 메모 |
| --- | --- | --- | --- | --- |
| 1 | [Less Restricting Headwear](https://sp-mod.com/mod/922) | 922 | 17KB | 헬멧·헤드셋·마스크·페이스실드 동시 착용 제한 해제 |
| 2 | [BETTER ATTACHMENTS](https://sp-mod.com/mod/1939) | 1939 | 54KB | 모든 부착물의 인간공학·반동 개선 |
| 3 | [Definitive Weapon Variants](https://sp-mod.com/mod/2010) | 2010 | 143KB | 기존 총기 기반 변형 600종 |
| 4 | [Canted Aiming](https://sp-mod.com/mod/2415) | 2415 | 11KB | 백업 사이트 없이 경사 조준 |
| 5 | [Holster Everything](https://sp-mod.com/mod/2650) | 2650 | 37KB | 홀스터에 넣을 수 있는 품목 확대 |
| 6 | [Black Exfil Revision](https://sp-mod.com/mod/2380) | 2380 | 8KB | Black Exfil에 페이스실드 장착 |
| 7 | [Jero Backpack](https://sp-mod.com/mod/2369) | 2369 | 32KB | 배낭 격자 확대. [Backpack Resizer](https://sp-mod.com/mod/2996)(2996)와 택일 |
| 8 | [Restore Armor Plates](https://sp-mod.com/mod/2973) | 2973 | 26KB | 방어판 최대 내구도 복구 키트 |
| 9 | [Repair Skill Extension](https://sp-mod.com/mod/2556) | 2556 | 15KB | 수리 버프 상향 + 모든 방어구에 적용 |
| 10 | [Repair Max Durability](https://sp-mod.com/mod/1305) | 1305 | 30KB | 무기 최대 내구도 복구 아이템 |
| 11 | [Cobra's Durability Tweaks](https://sp-mod.com/mod/3019) | 3019 | 12KB | 내구도 소모·고장 축소/제거. 10번과 방향이 겹친다 |
| 12 | [Realistic Suppressors](https://sp-mod.com/mod/3171) | 3171 | 8KB | 소음기 성능 대폭 강화 |
| 13 | [Full Automatic Guns](https://sp-mod.com/mod/1972) | 1972 | 24KB | 일부 SMG·소총을 자동 사격으로 |
| 14 | [ShrapnelFix](https://sp-mod.com/mod/3118) | 3118 | 14KB | 수류탄 파편이 몸 높이로 |
| 15 | [Airburst](https://sp-mod.com/mod/2948) | 2948 | 29KB | 40mm 공중폭발탄. 의존성은 설치된 WTT CommonLib |
| 16 | [Magic Optic Mount](https://sp-mod.com/mod/3101) | 3101 | 19KB | 광학 + 열상 동시 장착. **설치된 ECOTI와 성격이 겹친다** |
| 17 | [500 Euro for Skier](https://sp-mod.com/mod/2434) | 2434 | 8KB | 스키어 거래를 유로로 |
| 18 | [Boxes At Ref (BARF)](https://sp-mod.com/mod/2483) | 2483 | 9KB | Arena 상자를 Ref 목록에 |
| 19 | [Limitless](https://sp-mod.com/mod/2175) | 2175 | 27KB | 상인 구매 제한 해제·배수 |
| 20 | [Equipment Is Eternal](https://sp-mod.com/mod/2897) | 2897 | 14KB | 죽어도 장비 유지. [Fair Equipment Restoration](https://sp-mod.com/mod/2575)(2575, 23KB)와 택일 |
| 21 | [AmmoGen](https://sp-mod.com/mod/2777) | 2777 | 64KB | 커스텀 탄약 제작 프레임워크. 직접 만들 때만 |
| 22 | [Slimmer Magazines](https://sp-mod.com/mod/2373) | 2373 | 6KB | 2x2 탄창 → 1x2. [Shorter Magazines](https://sp-mod.com/mod/3186)(3186)와 **같이 넣지 말 것** |
| 23 | [Ultimate Flea](https://sp-mod.com/mod/2844) | 2844 | 47KB | 플레아 전면 통제. 7-B의 3177과 택일 |
| 24 | [AllGoodsTrader](https://sp-mod.com/mod/2648) | 2648 | 589KB | 상인 4명 추가. 상인 줄이 11명이 된다 |
| 25 | [bluehead's AIO Trader](https://sp-mod.com/mod/374) | 374 | 41KB | 전 품목 단일 상인 |
| 26 | [ALL AMMO TRADER](https://sp-mod.com/mod/1554) | 1554 | 2.8MB | 전 탄약 상인 |

## 7-D — 중형 (10MB 이상, 넣을지 따로 결정)

| 모드 | id | 크기 | 걸리는 점 |
| --- | --- | --- | --- |
| [Wolfik's Heavy Trooper Masks](https://sp-mod.com/mod/1569) | 1569 | 18MB | 방어력 있는 마스크 5종 |
| [Manimal's Ammo Loading Animations](https://sp-mod.com/mod/2681) | 2681 | 19MB | 의존성 둘(Continuous Load Ammo·WTT CommonLib)이 **이미 설치돼 있다** |
| [Sage's Incendiary Grenade](https://sp-mod.com/mod/3113) | 3113 | 24MB | **BigBrain**(902)이 새로 필요하다 |
| [.22lr Ammunition](https://sp-mod.com/mod/2516) | 2516 | 29MB | 탄종 추가. C11을 넣을 때의 선행 조건이기도 |
| [Peacemaker Continued](https://sp-mod.com/mod/2414) | 2414 | 45MB | 총기 추가 팩 |
| [Better Rear Sights](https://sp-mod.com/mod/1591) | 1591 | 52MB | 기계식 가늠자 개선 |
| [Fold That Stock](https://sp-mod.com/mod/2691) | 2691 | 56MB | 접이식 개머리판 작동 |
| [The Quartermaster](https://sp-mod.com/mod/2817) | 2817 | 5.7MB | **ItemGen**(2803)이 새로 필요하다 |
| [Better OKP-7](https://sp-mod.com/mod/2711) | 2711 | 4MB | OKP-7 레티클. 표기 `~4.1.3` |
| [TraderGen](https://sp-mod.com/mod/2749) | 2749 | 1.3MB | 상인 제작 프레임워크. 직접 만들 때만 |

## 7-E — 대형 (100MB 이상, 7차 범위 밖)

| 모드 | id | 크기 | 메모 |
| --- | --- | --- | --- |
| [Painter](https://sp-mod.com/mod/1025) + [Tactical Gear Component](https://sp-mod.com/mod/1125) | 1025·1125 | **647MB** | 서로를 요구한다. 한 묶음으로만 가능 |
| [WTT - Pack 'n' Strap](https://sp-mod.com/mod/1278) | 1278 | 302MB | 배틀벨트 모듈 장비 |
| [Kalashnikov Enhanced Modding](https://sp-mod.com/mod/3010) | 3010 | 260MB | AK 배럴·핸드가드 분리 |

## 실행 순서 (7-A)

다운로드 목록은 [download/manifest-stage7.json](download/manifest-stage7.json)에 만들어 뒀다(14개, 314KB).

```bash
node tools/download-stage.mjs stage7
```

해제는 **Windows의 `tar.exe`** 로 한다(Git Bash의 `tar`로는 zip이 안 풀린다).

```powershell
Get-ChildItem E:\Spt_Mods\downloads\stage7 -File | ForEach-Object {
  $d = "E:\Spt_Mods\extracted\stage7\" + [IO.Path]::GetFileNameWithoutExtension($_.Name)
  New-Item -ItemType Directory -Force $d | Out-Null
  & C:\Windows\System32\tar.exe -xf $_.FullName -C $d
}
```

```bash
node tools/check-conflicts.mjs stage7
```

설치는 게임·서버를 **모두 끈 뒤**(설치 스크립트가 실행 중이면 거부한다. 서버를 내리기 전에 `EscapeFromTarkov` 프로세스부터 본다) 프로필을 백업하고:

```powershell
# 13개는 복사만, 마지막 하나에서 서버 테스트
tools\install-and-test.ps1 -Stage stage7 -Pattern '2814__*' -Tag 'stage7A-2814' -SkipTest
# ... 3190 2429 2969 2524 2985 3049 3106 3006 1878 2576 2554 2422 동일 ...
tools\install-and-test.ps1 -Stage stage7 -Pattern '3105__*' -Tag 'stage7A-3105'
```

끝나면 `node tools/gen-installed-mods.mjs`로 목록을 다시 만든다(7단계를 추가하려면 `gen-installed-mods.mjs`의 `stageTitle`과 단계 루프에 `7`을 넣어야 한다 — 6차 때와 같은 손질).

## 확인 지점

| 어디서 | 볼 것 |
| --- | --- |
| 서버 테스트 | 서버 모드 49 → 52 전후(3106·3006·2524 등 서버 쪽), 오류 0 |
| 게임 로그 | 플러그인 88 → 99 전후. 표시명이 파일명과 다를 수 있으니 `Loading [` 줄 수로 센다 |
| 인벤토리 | 방어구 툴팁에 수치(3106), 탄약 툴팁에 수치(3006) — ODT Item Info와 겹쳐 길어지지 않는지 |
| 장비 화면 | 방어판을 다른 조끼에 꽂아 보기(1878), 배낭·조끼 접기(2422), 얼굴 슬롯 겹침(3105) |
| 레이드 | 방어판 교체(2814), 컨테이너 단축키(3190), 조준 유지(2985) |
| 개조 화면 | 아무 부품이나 꽂히는지(2576), 호환 강조가 망가지지 않는지 |

## 되돌리기

- 7-A: `doc/install-records/installed-stage7A-*.txt`의 파일만 지운다. 전부 비결속이라 프로필 복원이 필요 없다.
- 7-C부터: 플레이 전이면 파일 삭제로 충분하고, 플레이한 뒤면 `backup/profiles-before-stage7C-<id>`로 복원한다.

## 이 계획에서 하지 않는 것

- 7-B의 겹치는 것들은 어느 쪽을 쓸지 정하기 전에는 넣지 않는다.
- 7-D·7-E는 넣을지부터 따로 정한다. 특히 7-E는 합쳐 1.2GB다.
- 밸런스를 이유로 거르지 않는다. 치트성 여부는 선택의 문제로 둔다.
