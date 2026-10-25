# 6차 설치 계획 — 총기·파츠 / 상인 (2026-10-25)

[CANDIDATES-GUN-TRADER.md](CANDIDATES-GUN-TRADER.md)의 Forge 재조사에, **sp-mod.com 밖의 배포지**까지 훑은 결과를 합쳐 세운 계획이다. 절차는 [PROCEDURE.md](PROCEDURE.md)를 그대로 따른다.

## 배포지를 어디까지 봤나

| 배포지 | 상태 | 쓸 만한가 |
| --- | --- | --- |
| **sp-mod.com** (현재의 Forge) | 4.1.6 호환 633개 | 주 배포지. API로 조회·다운로드 자동화됨 |
| `forge.sp-tarkov.com`, `hub.sp-tarkov.com` | **DNS 조회 자체가 안 됨**(둘 다 죽음) | 예전 공식 주소. sp-mod.com이 그 후속이라는 뜻 |
| **GitHub** (topic `spt-mod`·`sptarkov`·`spt-aki`, 설명 검색) | 활발. 4.1.6 대상 저장소 다수 | Forge에 안 올라온 것이 실제로 있음. 릴리스 asset URL이라 `download-stage.mjs`로 그대로 받힌다 |
| **Nexus Mods** (Escape from Tarkov) | SPT 모드 **22개 전부** 확인 | 대부분 한 사람이 올린 포팅본, 다운로드 30~700대. 받으려면 로그인이 필요해 자동화 불가 |
| [SPT Mod Archive](https://tekunogosu.github.io/sptmodarchive/) (tekunogosu) | 목록 2,125개 아카이브 | sp-mod.com 목록의 사본. 새 모드 출처는 아니고, 사이트가 내려갔을 때의 조회용 |
| Reddit·구글 일반 검색 | SPT 4.1 관련 결과 거의 없음 | 쓸 수 없었다 |

**이번 조사로 드러난 것 하나** — Forge 키워드 추림은 구멍이 있다. [ECOTI](https://sp-mod.com/mod/2930)(클립온 열상, 다운로드 15,943)는 이름·소개에 "attachment/optic/scope" 같은 말이 없어 1차·2차 추림에 둘 다 안 걸렸고, GitHub 쪽에서 먼저 발견했다. 그래서 아래 목록에 넣는다.

## 배포지별 신뢰 기준

Forge는 업로드 검수와 결속·치트 표시가 붙지만, GitHub·Nexus는 그런 게 없다. 그래서 Forge 밖에서 받는 것은 다음을 지킨다.

- 소스가 공개돼 있고 라이선스가 있는 것만(아래 둘 다 MIT).
- 받은 압축은 `extracted/`에 풀어 **파일 목록을 눈으로 확인한 뒤** 설치(`check-conflicts.mjs`로 경로 충돌 검사는 그대로).
- 서버 모드는 테스트 서버로 먼저 올려 보고, 게임 플러그인은 `LogOutput.log`에서 로드와 오류를 확인.

## 넣을 것

### 6-A — 저부담 (10개 = 비결속 8 + 결속 2)

용량이 작고 되돌리기 쉬운 것들. 비결속 8개는 한 번에 복사하고 마지막에 테스트 한 번, 결속 2개는 그 뒤에 하나씩.

| 모드 | id | 버전 | 크기 | 위치 | 왜 |
| --- | --- | --- | --- | --- | --- |
| [OpenBarters](https://sp-mod.com/mod/3194) | 3194 | 1.0.1 | 14KB | 게임 | 물물교환 대금을 상인이 사주는 아무 아이템으로. 비결속·무의존 |
| [Quick Trader Load Times](https://github.com/JoelHauser/QuickTraderLoadTimes) | `gh-quicktraderloadtimes` | 1.0.0 | 39KB | 게임 | 상인 화면·플레아가 뜨는 속도. 상인이 **6명 더 늘어난** 지금 체감이 가장 큰 쪽. MIT |
| [PortableTraders](https://github.com/wangbofan/SPT-PortableTraders) | `gh-portabletraders` | 1.1.0 | 15KB | 서버 | BTR 기사·등대 상인을 주 화면 상인 목록에 올려 과제 수령·제출. 4.1.6 실측 통과 표기, 순수 서버, MIT |
| [BrightLasers](https://sp-mod.com/mod/1358) | 1358 | 3.0.0 | 12KB | 게임 | 레이저 밝기·굵기 F12 조절. 3.0.0에서 번들을 떼어 WTT 충돌 없어짐 |
| [No Barrel Shadow](https://sp-mod.com/mod/2780) | 2780 | 2.0.1 | 10KB | 게임 | 라이트가 총구에 만드는 그림자 제거 + 조명별 밝기. 결속 표시는 있으나 클라이언트 전용 |
| [Ref Rework](https://sp-mod.com/mod/3014) | 3014 | 1.0.0 | 17KB | 서버 | 도그태그 → Ref GP 코인. **Ledger(3085)와 도그태그 용처가 겹침** → 게임에서 둘 다 보고 하나를 끌 것 |
| [Realistic Insurance](https://sp-mod.com/mod/2949) | 2949 | 0.2.1 | 38KB | 서버 | 보험 회수를 "누가 죽였나"로 계산. SVM 보험 설정과 겹치면 SVM 쪽을 끈다 |
| [All Weapons Flea](https://sp-mod.com/mod/3200) | 3200 | 1.0.1 | 8KB | 서버 | 상인 전용 총기를 플레아에서 거래. 총기 파츠를 모으는 데 직접 도움 |
| [Moe-TradeMarker](https://sp-mod.com/mod/2736) | 2736 | 1.2.6 | 99KB | 게임+서버 | 상인 구매품 배지 + 재판매 차단(선택). QuickSell 연동. 표기 `~4.1.0` → **단독 테스트** |
| [Vanguard](https://sp-mod.com/mod/2917) | 2917 | 2.0.0 | 18MB | 게임+서버 | WMLX 라이트 + Sandman X 소음기. 의존성(WTT CommonLib)은 이미 설치됨 |

마지막 두 개(2736·2917)는 결속 표시가 있어 **6-A 안에서도 맨 뒤에 하나씩** 넣는다.

### 6-B — 결속, 하나씩 (2개)

프로필 백업 → 설치 → 테스트 → 다음.

| 모드 | id | 버전 | 크기 | 왜 | 주의 |
| --- | --- | --- | --- | --- | --- |
| [NATO Quartermaster](https://sp-mod.com/mod/3030) | 3030 | 1.1.3 | 354KB | 서방 총기·방어구·완성 빌드 상인 + 퀘스트 6단. 4.1.6 정확, 의존성 없음 | 상인이 7명째가 된다 → TraderScrolling·TraderSpace 동작 재확인 |
| [ECOTI](https://sp-mod.com/mod/2930) | 2930 | 3.4.2 | 32MB | 야투경에 물리는 클립온 열상. Peacekeeper 판매, 야투경과 같은 자리에 스폰 | 3.4.2가 **PiP Disabler(2667, 설치됨)와의 열상 표시 문제를 고친 버전**이다. 그 아래 버전은 쓰지 않는다. Better Thermal(2531)과 같은 화면을 건드리므로 게임에서 함께 확인 |

### 6-C — 선행 조건부터 확인해야 하는 것 (2개, 6-B 뒤)

[Tylevo's Field Attachments](https://sp-mod.com/mod/3125)(3125)와 [Scrollable Attachments](https://sp-mod.com/mod/2677)(2677)는 둘 다 **UnityToolkit(1426)** 을 요구한다. 5차에서 "UnityToolkit 최신이 4.1.5 표기"라 보류했는데, 이번에 확인한 사실은 이렇다.

- 3125의 README는 **UnityToolkit 2.0.2 이상**을 요구하며 자신은 4.1.5/4.1.6 대상이라고 적고 있다.
- UnityToolkit의 GitHub 최신 릴리스는 **2.0.1**이고, **2.0.2는 Forge에만** 있으며 표기가 4.1.5다. 즉 "GitHub에서 받으면 해결"이 아니다.
- 그러므로 순서는 **UnityToolkit 2.0.2 단독 설치 → 서버·게임 로드 확인 → 3125 → 2677**. 1426은 비결속이라 되돌리기 쉽다.

여기까지 되면 인벤토리를 열지 않고 광학·소음기·손잡이를 바꾸는 것과 부착물 목록 스크롤이 생긴다. 안 되면 셋 다 되돌리고 6-C는 접는다.

### 6-D — 사람 손이 필요한 것 (1개)

[VanillaTradersRework-Reloaded](https://www.nexusmods.com/escapefromtarkov/mods/10) (Nexus, v1.0.0, SPT 4.1.2 표기) — 상인 초상화 교체. 게임성 변화 없고 프로필에 영향 없으며, 설치된 커스텀 상인(Lotus·Scorpion·Artem·Couturier·Croupier)까지 포함한다. AI 생성 이미지를 썼다고 제작자가 밝혔다.

Nexus는 받으려면 계정 로그인이 필요해 스크립트로 못 받는다. 받아서 아래에 두면 그 다음은 같은 절차로 처리한다.

```bash
# 사람이 할 일: 위 Nexus 페이지에서 zip을 받아 이 폴더에 넣기
E:\Spt_Mods\downloads\stage6\nexus-vanillatradersrework__VanillaTradersRework-Reloaded-1.0.0.zip
```

적용 후 SPT 런처에서 **Settings → Clean Temp Files**를 한 번 눌러야 초상화가 바뀐다(제작자 안내).

### 안 넣는 것

| 모드 | 왜 |
| --- | --- |
| [Kalashnikov Enhanced Modding](https://sp-mod.com/mod/3010) (3010, 260MB) | AK 부품화로 가장 매력적이지만 결속 + 대형. 5차 방침("총기 팩은 범위 밖") 유지. 넣으려면 7차로 단독 |
| 3035(560MB)·2239(434MB)·2737(Epic's AIO 포함 2GB급) | 대형 콘텐츠. 같은 이유 |
| [Gunsmith Tweaks](https://sp-mod.com/mod/1866) (1866) | 설치된 Gunsmith Task Trader(3124)와 목적 중복 |
| [Item Valuation](https://sp-mod.com/mod/2021) (2021) | 설치된 Show Me The Money와 표시 중복 |
| 2844·3070·1131·3177 (플레아 가격·경제) | SVM 플레아 설정과 겹침. 플레아 경제를 바꿀지부터 결정 |
| [SPT Barterizer](https://sp-mod.com/mod/3195) (3195) | 0.1.x + 결속. OpenBarters를 써 본 뒤 필요하면 |
| [SPT Stance Overhaul](https://sp-mod.com/mod/3136) (3136) | 0.0.2 알파. 의존성이 Fontaine's FOV Fix(701) + Realism Common Lib(3135)로 번져 조작 감각까지 바뀐다 |
| Nexus의 AttachmentCompatHighlight / Ammo Caliber Labels | 설치된 Compatibility Highlighter(2859)·Caliber Under Ammo Name(2586)과 중복 |
| Nexus의 BRACE·P.L.A.T.E.·RZEssentials | 반동·탄도·서버 규칙 전면 개편. 총기 파츠 범위를 넘고 SVM과 겹침 |
| MG General Trader (GitHub) | 상인 제작 프레임워크. 직접 상인을 만들 때만. 라이선스 CC BY-NC-ND |

## 실행 순서

다운로드 목록은 [download/manifest-stage6.json](download/manifest-stage6.json)에 이미 만들어 뒀다(Forge 10 + GitHub 2).

```bash
node tools/download-stage.mjs stage6
```

해제·검사:

```bash
for f in downloads/stage6/*; do tar -xf "$f" -C "extracted/stage6/$(basename "$f")"; done
node tools/check-conflicts.mjs stage6
```

설치(각 단계 전에 프로필 백업):

```powershell
# 6-A 비결속 8개 — 복사만 하고 마지막에 한 번 테스트
tools\install-and-test.ps1 -Stage stage6 -Pattern '3194__*' -Tag 'stage6A-3194' -SkipTest
# ... 1358 2780 3014 2949 3200 gh-quicktraderloadtimes gh-portabletraders 동일 ...
tools\install-and-test.ps1 -Stage stage6 -Pattern 'gh-portabletraders__*' -Tag 'stage6A-portabletraders'

# 6-A 결속 2개 → 6-B → 6-C: 하나씩, 매번 프로필 백업 + 테스트
tools\install-and-test.ps1 -Stage stage6 -Pattern '2736__*' -Tag 'stage6A-2736'
tools\install-and-test.ps1 -Stage stage6 -Pattern '2917__*' -Tag 'stage6A-2917'
tools\install-and-test.ps1 -Stage stage6 -Pattern '3030__*' -Tag 'stage6B-3030'
tools\install-and-test.ps1 -Stage stage6 -Pattern '2930__*' -Tag 'stage6B-2930'
tools\install-and-test.ps1 -Stage stage6 -Pattern '1426__*' -Tag 'stage6C-1426'
```

끝나면 `node tools/gen-installed-mods.mjs`로 [INSTALLED-MODS.md](INSTALLED-MODS.md)를 다시 만든다.

## 확인 지점

| 단계 | 서버에서 | 게임에서 |
| --- | --- | --- |
| 6-A | 서버 모드 38 → 42쯤 (3014·2949·3200·PortableTraders가 서버 쪽. 3194는 해제본 경로로 확정), 오류 0 | 상인 화면 여는 속도, 물물교환 창의 격자, BTR·등대가 상인 목록에 보이는지, 레이저·총구 그림자 |
| 6-A 결속 | 2736 서버 쪽 로드 | 상인 구매품 배지, QuickSell 메뉴가 그대로 뜨는지 / WMLX·Sandman X가 개조 화면에 나오는지 |
| 6-B | 상인 1명 추가(3030), ECOTI 아이템·슬롯 등록 | 상인 목록 스크롤(7명), Peacekeeper에 ECOTI 매물, 야투경에 물렸을 때 열상이 튜브 안에만 들어오는지(PiP Disabler·Better Thermal 켠 상태에서) |
| 6-C | 1426은 게임 전용이라 서버 무변화 | `LogOutput.log`에 UnityToolkit → Field Attachments 로드 순서, Left Alt 카드가 뜨는지 |
| 6-D | 무변화 | 상인 초상화. 런처 Clean Temp Files 후 |

막히기 쉬운 곳은 둘이다. **ECOTI ↔ PiP Disabler ↔ Better Thermal**이 같은 조준경·열상 경로를 건드린다(3.4.2가 바로 그 문제를 고친 버전이므로, 문제가 나면 2531을 먼저 끈다). 그리고 **도그태그 용처가 Ledger와 Ref Rework로 둘이 된다** — 어느 쪽을 남길지 게임에서 보고 정한다.

## 되돌리기

- 6-A 비결속: `doc/install-records/installed-stage6A-*.txt`의 파일만 삭제.
- 6-A 결속·6-B: 플레이 전이면 파일 삭제로 충분. 플레이한 뒤면 `backup/profiles-before-stage6<단계>`로 복원.
- 6-C: 3125 → 2677 → 1426 역순으로 삭제.
- 6-D: 초상화 파일만 삭제 + 런처 Clean Temp Files.

## 이 계획에서 하지 않는 것

- 대형 콘텐츠(3010·3035·2239·2737)는 넣지 않는다. 넣을지 정해지면 7차로 따로 세운다.
- 플레아 가격·경제 모드는 손대지 않는다. SVM 설정이 먼저다.
- Nexus 모드는 사람이 받아 둔 것만 처리한다. 계정 로그인은 대신 하지 않는다.
