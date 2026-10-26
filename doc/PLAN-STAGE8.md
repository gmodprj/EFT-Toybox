# 8차 설치 계획 — OP·AI 동료·HUD 포팅본 (2026-10-27)

[CANDIDATES-OP-416.md](CANDIDATES-OP-416.md)의 미설치 후보와, 따로 조사한 AI 동료 모드 2개, 그리고 Forge 밖에서 도는 Game Panel HUD 4.1 포팅본까지를 하나의 계획으로 묶은 것. 절차는 [PROCEDURE.md](PROCEDURE.md)를 그대로 따른다.

## 줄 세운 기준

[PLAN-STAGE7.md](PLAN-STAGE7.md)와 같다.

1. **비결속이 먼저다.** 파일만 지우면 끝나는 것부터.
2. **용량이 작은 것이 먼저다.** 문제 범위를 좁히기 쉽다.
3. **설치본과 역할이 겹치는 것은 뒤로.** 택일을 먼저 정한다.
4. **새 의존성·대형·치트 표시는 맨 뒤.** 넣을지부터 따로 정한다.

여기에 8차에서만 더하는 기준이 하나 있다. **Forge 밖에서 받는 것은 맨 마지막이고, 다른 모든 단계와 섞지 않는다**(8-G). 출처가 다르면 문제가 났을 때 원인을 가리기 어렵다.

## 전체 분량

| 단계 | 내용 | 개수 | 합계 |
| --- | --- | --- | --- |
| 8-A | 비결속·소형, 겹침 없음 | 5 | 0.9 MB |
| 8-B | 설치본과 겹침 — 택일 결정 후 | 3 | 0.1 MB |
| 8-C | 결속·단독 | 3 | 1.5 MB |
| 8-D | 치트 표시 | 2 | 46 KB |
| 8-E | AI 동료 (+ 선행 Waypoints) | 3 | 51.5 MB |
| 8-F | 대형 | 1 | 380.7 MB |
| 8-G | HUD 포팅본 (Forge 외부) | 1 | 미확인 |

Forge 경로 합계 약 **435 MB**. 매니페스트는 `doc/download/manifest-stage8-A.json` ~ `-F.json`에 미리 넣어 뒀다.

## 단계별

### 8-A — 비결속·소형, 겹침 없음 (957 KB)

한 번에 복사하고 마지막에 서버 테스트 한 번. 전부 파일만 지우면 되돌아간다.

| 모드 | id | 버전 | 크기 | 무엇이 달라지나 |
| --- | --- | --- | --- | --- |
| [Dev Balaclava Fix](https://sp-mod.com/mod/1937) | 1937 | 1.2.0 | 4 KB | 개발자 발라클라바 착용 시 완전 무적(폭발·저격·BTR 포함) |
| [Headshot Damage Redirection](https://sp-mod.com/mod/2953) | 2953 | 1.8.0 | 8 KB | 머리·흉부 피해를 다른 부위로 분산. **1.9.0은 4.1.5 전용이라 쓰지 않는다** |
| [MeleeMultiplier](https://sp-mod.com/mod/2763) | 2763 | 0.0.4 | 20 KB | 근접 데미지 기본 x10. 설정 파일에서 배율 조정 |
| [The Blacklist](https://sp-mod.com/mod/755) | 755 | 3.1.1 | 80 KB | 플리 블랙리스트 해제 + 고급 탄약 가격 재조정 |
| [Fujiwara Mokou](https://sp-mod.com/mod/1796) | 1796 | 1.2.0 | 845 KB | 플레이어 캐릭터 추가. 선행 WTT CommonLib는 설치본에 있음 |

미리 본 겹침

- **755 ↔ 설치된 SVM** — SVM에도 플리 설정이 있다. 755는 탄약 가격 재조정까지 하므로 범위가 더 넓다. 둘 다 플리를 건드리니 설치 후 플리 가격을 한 번 보는 게 좋다.
- **1796** — 캐릭터 선택 화면을 건드린다. 새 프로필을 만들 때만 고를 수 있어서 기존 프로필에는 옷(래그맨 판매) 쪽만 들어온다.

### 8-B — 설치본과 겹친다, 택일 결정 후 (85 KB)

전부 비결속이라 바꿔 끼우기는 쉽다. **결정 없이 넣지 않는다.**

| 모드 | id | 크기 | 무엇과 겹치나 | 정해야 할 것 |
| --- | --- | --- | --- | --- |
| [Recoil Rework (Legacy)](https://sp-mod.com/mod/2190) | 2190 | 35 KB | 설치된 **BETTER ATTACHMENTS** | 둘 다 반동을 건드린다. 1939는 부착물별 수치, 2190은 전반 커브. 효과가 누적되므로 넣고 나서 반동을 다시 맞춰야 한다 |
| [Instant Insurance](https://sp-mod.com/mod/2394) | 2394 | 38 KB | 설치된 **Equipment Is Eternal** | 2897이 사망 시 장비를 통째로 복원하므로 기능이 거의 겹친다. 추가로 ① SPT `lostOnDeath.wipeOnRaidStart`와 충돌(`false` 권장), ② Forge가 "FIR Means FIR 모드와 비호환"으로 표시 |
| [Shorter Magazines](https://sp-mod.com/mod/3186) | 3186 | 7 KB | 설치된 **Slimmer Magazines** | 3186은 3x1→2x1, 2373은 2x2·1x3→1x2. 둘 다 서버에서 탄창 크기를 덮어쓴다. 3186 저자도 2373을 "대안"으로 적어 뒀으니 **하나만 쓰는 쪽을 권한다**. 3186은 결속 표시도 있다 |

### 8-C — 결속·단독 (1.5 MB)

프로필 백업 후 **하나씩** 설치·테스트. 플레이 후 제거하면 프로필이 깨질 수 있다.

| 모드 | id | 버전 | 크기 | 무엇이 달라지나 |
| --- | --- | --- | --- | --- |
| [Death Shadow Tweaks](https://sp-mod.com/mod/2943) | 2943 | 1.0.0 | 4 KB | 데스 섀도 마스크를 헬멧과 함께 착용. 설치된 **SOMF**와 대상이 겹칠 수 있다 — 먼저 SOMF만으로 되는지 확인 |
| [Olympus](https://sp-mod.com/mod/505) | 505 | 4.1.3 | 53 KB | OP 리그·방어구·헬멧·백팩·자극제·탄창을 테라피스트·래그맨·예거가 판매. Fika 비호환 |
| [SIM - Simple Item Modifier](https://sp-mod.com/mod/2906) | 2906 | 0.1.0 | 1.4 MB | 아이템 DB 일괄 편집 UI. **설치만으로는 아무것도 바뀌지 않는다**(Delta 규칙을 만들어 켤 때만 적용). 설치된 SVM과 역할이 겹치지만 대상이 다르다 — SVM은 서버 설정, SIM은 아이템 수치 |

### 8-D — 치트 표시 (46 KB)

Forge가 공식 치트 경고를 붙인 둘. 넣을지부터 따로 결정한다. 둘 다 비결속이라 되돌리기는 쉽다.

| 모드 | id | 버전 | 크기 | 무엇이 달라지나 |
| --- | --- | --- | --- | --- |
| [MagicBullet](https://sp-mod.com/mod/3065) | 3065 | 1.0.0 | 4 KB | 내 총알·근접 데미지 x1~x100, 원샷킬 체크박스. 받는 피해와 봇끼리 피해는 바닐라 유지 |
| [Tushonka-800](https://sp-mod.com/mod/3169) | 3169 | 1.0.1 | 43 KB | 방향성 실드, 적 스캔, 전투 오버드라이브 |

**3065와 2763(MeleeMultiplier)은 둘 다 근접 데미지를 건드린다.** 같이 넣으면 배율이 곱으로 걸릴 수 있어 한 쪽만 쓰는 게 낫다.

### 8-E — AI 동료 (51.5 MB)

선행 모드가 하나 필요하고, 본체 둘은 **같은 일을 한다.**

| 순서 | 모드 | id | 버전 | 크기 | 비고 |
| --- | --- | --- | --- | --- | --- |
| 8E-0 | [Waypoints - Expanded Navmesh](https://sp-mod.com/mod/827) | 827 | 1.9.0 | 48 MB | **선행.** 봇이 다닐 수 있는 영역을 맵 전체로 넓힌다. 비결속 |
| 8E-1 | [Miyako Carry Service](https://sp-mod.com/mod/2709) | 2709 | 1.1.4 | 509 KB | **이걸 먼저 권한다.** SAIN·Fika 호환 명시, 다른 모드가 추가한 AI 타입도 팀원으로 쓸 수 있다. 비결속 |
| 8E-2 | [PIT Fireteam](https://sp-mod.com/mod/2676) | 2676 | 1.0.0 | 3.0 MB | 분대를 영구 보유하고 장비를 꾸민다. 현장 영입·배신까지 있는 큰 시스템. **Fika 비호환**(설치본에 Fika 없으니 무관). 비결속 |

- **의존성**: 둘 다 `827 Waypoints 1.9.0` + `902 BigBrain 1.5.0`을 요구한다. **BigBrain은 이미 1.5.0으로 설치돼 있어 요구 버전과 정확히 일치**하므로 받을 필요가 없다. 827만 새로 받는다.
- **둘을 같이 넣지 않는다.** 둘 다 "내 편 AI를 레이드에 데려오는" 모드다. 하나 넣고 레이드 한 판 보고, 마음에 드는 쪽을 남긴다. 둘 다 비결속이라 교체 비용은 파일 삭제뿐이다.
- 827은 48 MB지만 맵 데이터라 되돌리기는 파일 삭제로 끝난다. 다만 **봇 이동 범위가 넓어지므로 기존 봇 모드(SAIN 미설치, BigBrain 설치)의 체감이 달라질 수 있다.** 827만 먼저 넣고 레이드 한 판 돌려 성능을 보는 편이 안전하다.

### 8-F — 대형 (380.7 MB)

| 모드 | id | 버전 | 크기 | 비고 |
| --- | --- | --- | --- | --- |
| [Make Shotguns Great Again!](https://sp-mod.com/mod/1453) | 1453 | 1.19.1 | 380.7 MB | **결속.** 특수 산탄, 보스 강제 고장 제거, KS-23 조준경 정렬 수정 등. 선행 WTT CommonLib는 설치본에 있음. 제약이 `~4.1.6`으로 가장 좁게 맞춰져 있다 |

단독으로 받고 단독으로 설치·테스트한다. 설치된 **Cobra's Durability Tweaks**가 이미 고장을 전부 끄고 있어서 "보스 강제 고장 제거" 항목은 체감이 없을 수 있다.

### 8-G — Game Panel HUD 4.1 포팅본 (Forge 외부, 수동)

출처: 아카라이브 SPT 채널 [Game Panel HUD 4.1 포팅](https://arca.live/b/tarkovspt/185302366) (KIDOlogy, 2026-10-08 작성 / 10-09 수정). 다운로드는 글에 적힌 `kio.ac` 링크. 후속 글 [버그 수정했으니 다시 다운하세요](https://arca.live/b/tarkovspt/185363870)에서 "스캐브 킬로그 안띄워주던 버그"를 고쳤고, 본 글 수정일이 그 뒤라 **본 글 링크가 수정판**이다.

이 단계는 다른 단계와 성격이 다르므로 `download-stage.mjs`를 쓰지 않는다(매니페스트의 `link`는 Forge 전용이다). 수동으로 받고 다음 순서를 지킨다.

1. 받은 파일을 `downloads/stage8-G/`에 **그 파일만** 넣는다. 다른 단계 파일과 섞지 않는다.
2. 바이러스 검사.
3. `tar -xf`로 `extracted/stage8-G/`에 풀고 **압축 내부 구조를 눈으로 확인**한다. `BepInEx/plugins/` 아래에 GamePanelHUD와 KmyTarkovApi가 함께 들어 있는지, `E:\Spt` 기준 밖의 경로가 섞여 있지 않은지.
4. `node tools/check-conflicts.mjs stage8-G`로 기존 파일과의 충돌 확인.
5. 설치는 `install-and-test.ps1`로 하되 **단독**으로. 설치 기록 태그는 `stage8-G-GamePanelHUD`.
6. 설치 후 `doc/install-records/`에 출처를 따로 적는다 — Forge가 아니라 아카라이브 글이라는 것, 글 주소, 받은 날짜, 파일 해시.

**넣기 전에 알아야 할 것**

- 원본 라이선스가 **CC BY-NC-ND 4.0**(개작물 배포 금지)이고, 원작자가 Forge 페이지에 "공식 배포처는 Oddba와 SPT 두 곳"이라고 적어 뒀다. 원작 레포 [이슈 #9](https://github.com/kmyuhkyuk/GamePanelHUD/issues/9)에서 4.1.6 빌드 배포 허락을 요청했지만 원작자 답이 없다. **승인이 확인되지 않은 재배포본**이다. 혼자 쓰는 것 자체는 라이선스가 막지 않지만, 이 저장소 기록에 올릴 때 그 사실을 같이 남긴다.
- 이 모드는 **무한 로딩·프리징 전력**이 있다(채널에 4.0.3~4.0.4 무한로딩 이슈 글, 프리징 파일 수정본 글이 있음). 원인은 대개 KmyTarkovApi 버전 짝이 안 맞는 것이라, 포팅본에 들어온 API를 그대로 쓰고 따로 받은 API와 섞지 않는다.
- 설치된 **Pinned Quest HUD(3155)** 와 퀘스트 표시가 겹친다. GamePanelHUD에도 퀘스트 패널이 있으니, 켜 보고 한쪽을 끈다(GamePanelHUD는 패널별로 끌 수 있다).
- 포팅본 대신 Forge 정식 경로를 쓰려면 **[ImpactCue-UI-MOD](https://sp-mod.com/mod/3029) 2.0.3**(4.1.6, 40 MB)이 방위·거리 컴퍼스와 탈출구·환승·퀘스트 마커를 내장한다. 4.1.6 호환 모드 640개 설명을 전수 검색한 결과, 컴퍼스를 가진 것은 ImpactCue와 [Quest Marker](https://sp-mod.com/mod/2954)(1.4.0, 키 누름 방식, 선행 WebOverlay) 둘뿐이었다. **8-G와 ImpactCue를 같이 넣지 않는다** — 같은 화면을 두 모드가 그린다.

## 단계마다 반복하는 절차

[PROCEDURE.md](PROCEDURE.md)의 2~4절. 요약하면:

```powershell
# 1) 프로필 백업 (결속 단계는 필수, 나머지도 권장)
#    E:\Spt\SPT_Runtime\user\profiles\* -> backup\profiles-before-stage8-<단계>\

# 2) 받기
node tools/download-stage.mjs stage8-A

# 3) 풀기 (각 파일을 자기 폴더로)
#    tar.exe -xf <파일> -C extracted\stage8-A\<파일명>

# 4) 충돌 검사 — 겹치는 경로가 하나도 없어야 진행
node tools/check-conflicts.mjs stage8-A

# 5) 설치 + 서버 테스트
E:\Spt_Mods\tools\install-and-test.ps1 -Stage stage8-A -Pattern '*' -Tag 'stage8-A'

# 6) 목록 갱신
node tools/gen-installed-mods.mjs
```

결속 단계(8-C, 8-F)와 8-E, 8-G는 `-Pattern`으로 **하나씩** 지정해 설치하고 매번 테스트한다.

게임 확인은 `E:\Spt\BepInEx\LogOutput.log`의 수정 시각이 설치 이후인지 먼저 본다.

## 권하는 진행 순서

1. **8-A** — 가장 가볍고 되돌리기 쉽다. 여기서 절차가 깨지면 뒤는 의미가 없다.
2. **8-E** — 827만 먼저 넣고 레이드 한 판으로 성능을 본 뒤, Miyako 하나만 추가.
3. **8-C** — 결속이라 프로필 백업 후 하나씩.
4. **8-B** — 택일 결정이 끝난 것만.
5. **8-F** — 대형. 단독.
6. **8-G** — 맨 마지막. 다른 단계가 모두 안정된 뒤에.
7. **8-D** — 치트 표시. 넣을지부터 결정.

## 이 계획에서 뺀 것

[CANDIDATES-OP-416.md](CANDIDATES-OP-416.md)에서 제외 판단한 것들을 그대로 유지한다.

| 모드 | 왜 뺐나 |
| --- | --- |
| FIR Fence Purchases (2371) | SVM이 이미 제공 |
| Arse Scratching Service (1946) | Cobra's Durability Tweaks가 내구도 소모를 0으로 만들어 무의미 |
| Epic's All in One (1263) | 1.5 GB. WTT-Armory·Definitive Weapon Variants와 삼중 겹침 |
| Make Magnifier Great Again (3216) | 의존성으로 1263 + JSOC(392 MB) 약 1.9 GB가 따라온다 |
| Tarkova-cola (3088) | 선행 VisitAPI Framework(13 MB, 결속)를 먼저 넣어야 한다. 넣을지 따로 결정 |
| Strange Curios (3059) | 제약이 `~4.1.5`라 4.1.6 동작을 확인하지 못했다 |

## 아직 확인하지 않은 것

- **파일 단위 충돌 검사는 받기 전까지 할 수 없다.** 위 "겹침"은 Forge 설명·변경 로그와 [INSTALLED-MODS.md](INSTALLED-MODS.md) 비교에 따른 것이고, 게임에서 돌려 본 결과가 아니다.
- 8-G 압축 내부 구조와 포함된 KmyTarkovApi 버전은 받아 보지 않아 모른다.
- 8-E의 두 모드가 설치본의 BigBrain 1.5.0과 실제로 맞물리는지는 레이드에서 봐야 한다.
