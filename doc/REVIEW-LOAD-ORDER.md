# 로드 순서와 기능 겹침 점검 (2026-10-28)

설치된 모드 전체를 훑어 **로드 순서를 바꿀 수 있는지**, 그리고 **같은 값을 두 모드가 덮어쓰고
있지 않은지** 본 결과다. 추정이 아니라 떠 있는 서버의 최종 DB 와 모드 DLL 메타데이터를 직접
읽어 측정했다.

## 먼저: 문서와 실물이 어긋나 있었다 (2026-10-28 맞춤)

| 항목 | 점검 당시 문서 | 실측 | 비고 |
| --- | --- | --- | --- |
| 서버 모드 | 106 | **114** | 8개가 설치 기록 없음 |
| 게임 플러그인 로드 | 134 | **142** | 5개가 설치 기록 없음 |

> **같은 날 정리했다.** 아래 8개의 설치 기록을 뒤늦게 복원해(`install-records/installed-stage9-*.txt`,
> 7044 파일) `manifest-stage9.json` 과 함께 올렸고, `INSTALLED-MODS.md` 를 다시 생성해 **모드 187개**
> 로 맞췄다. 다만 **당시 기록이 아니라 "지금 가진 파일" 목록**이다 — 덮어쓴 파일인지 새로 만든
> 파일인지는 구분할 수 없다. 자세한 것은 [WORKLOG.md](WORKLOG.md) 의 같은 날 절.

2026-10-27 20:45~21:49 사이에 **절차 밖에서 6개가 추가**됐다. `tools/install-and-test.ps1` 을
쓰지 않아 `install-records/` 가 없고, 그래서 `INSTALLED-MODS.md` 에도 나오지 않는다.

| 서버 모드 | 짝 플러그인 | 추가 시각 |
| --- | --- | --- |
| HoodsMoreEnergyDrinks | - | 10-27 20:45 |
| MoreBotsServer | MoreBotsPlugin | 10-27 20:46 |
| WTT-ContentBackport | WTT-ContentBackportClient | 10-27 21:07 |
| Solarint-SAIN-ServerMod | SAIN | 10-27 21:09 |
| ISB-Aishi | ISBAishiPlugin | 10-27 21:12 |
| ISB-SpecialForces | ISBSpecialForcesPlugin | 10-27 21:49 |

그 밖에 기록이 없는 것: `GF2 Skin Pack`(10-25), `WTT-HeadVoiceSelector`(08-02). 자작 3개
(HelmetSlotUnion·SlotCategoryRelax·WebConfigBridge)는 원래 기록 대상이 아니다.

**되돌릴 수단이 없다는 뜻이다.** 설치 기록은 제거할 파일 목록이기도 하므로, 이 8개는 지금
수동으로 지워야 한다.

2026-10-28 07:37 에 켠 게임 로그에서는 **플러그인 142개 로드, 오류 1건**(Fika 미설치 관련
DirectReload)뿐이다. 새로 들어온 6개도 전부 로드됐다.

## 로드 순서는 밖에서 바꿀 수 없다

SPT 4.1.6 이 `SPTarkov.Server.Core.xml` 에 적어 둔 규칙은 이렇다.

> 순서는 먼저 `Injectable.TypePriority` 로 정하고, 같으면 `ModGuid` 로 가린다. 실제로는 Type
> Priority 가 거의 전부를 결정하고, GUID 는 값이 같을 때만 의미가 있다.

둘 다 **모드 DLL 안에 컴파일된 값**이다. 순서를 적는 설정 파일도, 순서를 바꾸는 설정도 없다.

그래서 실제 값을 읽어 봤다. SPT 가 동봉한 `Mono.Cecil.dll` 로 서버 모드 114개의 DLL 에서
`[Injectable(...)]` 선언 **603개를 전부 읽은 결과, 모두 `TypePriority = 0`** 이었다. `IOnLoad`
를 구현한 220개 선언도 전부 0 이다.

- 즉 순서를 정하는 1차 기준을 **아무 모드도 쓰지 않는다.** 전부 SPT 의 보조 기준(GUID)에 맡겨져
  있다
- 순서를 바꾸려면 모드 DLL 을 고쳐 다시 빌드해야 한다. **권하지 않는다** — 갱신하면 사라지고,
  재배포 라이선스 문제도 생긴다

한 가지 더. 서버 로그에 모드 메시지가 찍히는 순서가 GUID 알파벳순과 맞지 않는 구간이 있다
(자작 `spt.helmetslotunion` 이 `fika.ghostfenixx.svm` 보다 먼저 찍힌다). 단계가 여러 개라
로그만으로는 단정할 수 없었다. 그래서 **누가 이기는지는 예측하지 말고 아래처럼 실측하는 것이
맞다.**

> **2026-10-28 서버 테스트로 확정됐다.** `[Cobra's Ammo Stack] v1.1.0 loaded. Modified 971
> ammunition items.` 가 **로그의 거의 마지막**(웹서버 시작 직전)에 찍힌다. SVM 메시지는 훨씬
> 앞이다. 그런데도 **DB 에 남은 값은 SVM 것**이다(아래 1번, 서버를 새로 띄워 다시 쟀다).
> 즉 **로그에 찍히는 순서는 DB 에 쓰는 순서가 아니다.** 로그를 보고 순서를 추측하면 틀린다.

## 실측한 겹침 3건

떠 있는 서버에 `POST /client/items` 로 최종 아이템 템플릿 9483개를 받아, 각 모드 설정값과
대조했다. (HTTPS·자체 서명 인증서. 게임 클라이언트가 쓰는 것과 같은 경로이고 읽기만 한다.)

### 1. 탄약 스택 — CobrasAmmoStack 이 완전히 죽어 있다

| 모드 | 설정 | 실제 반영 |
| --- | --- | --- |
| CobrasAmmoStack | `Enabled: true`, 구경 31개에 500/250/100… | **31개 중 0개** |
| SVM 프리셋 `Noname` | 소총 180, 지정사수 160, 권총 150, 산탄 60, 대구경 80 | 5개 범주 값이 모두 DB 에 있음 |

표본: 9x19 는 Cobra 가 500 을 쓰라는데 실제 150(SVM 권총값), 12게이지는 Cobra 100 인데 실제
60(SVM 산탄값). **Cobra 가 쓰라는 500 은 DB 어디에도 없다.**

2026-10-28 에 서버를 새로 띄워 같은 측정을 반복했다 — **다시 31개 중 반영 0개**. 측정한 값은
하나도 빠짐없이 SVM 의 다섯 범주값(150·180·160·60·80) 가운데 하나였다. Cobra 설정은
`Enabled: true` 이고 로그도 971개를 고쳤다고 말하므로, **쓰기는 하지만 뒤에 덮인다.**

모드가 새로 만든 구경(20x1mm 200, 7.84x49 40, 9.3x64 30 …)은 SVM 도 Cobra 도 아닌 값이다.
그 탄을 만든 콘텐츠 모드가 Cobra 보다 **뒤에** 아이템을 만들어 Cobra 가 보지 못한 것으로 보인다.

### 2. 화폐 스택 — SVM 쪽이 덮인다

| 아이템 | SVM 설정 | BarterItemsStacks 설정 | 실제 |
| --- | --- | --- | --- |
| 루블 | 10,000,000 | 50,000,000 | **50,000,000** |
| 달러 | 500,000 | 50,000,000 | **50,000,000** |
| 유로 | 500,000 | 50,000,000 | **50,000,000** |
| GP 코인 | 1,000 | 5,000 | **5,000** |

BarterItemsStacks 는 지정한 460건 중 452건이 반영됐다(어긋난 8건은 라비린스 열쇠 2종으로,
설정 5 인데 실제 1 이다). **SVM 의 화폐 네 줄은 아무 효과가 없다.**

1번과 2번을 함께 보면 SVM 이 Cobra 를 이기고 BarterItemsStacks 가 SVM 을 이긴다. 한 줄로 늘어선
순서로는 설명되지 않아, 어느 모드가 실제로 그 필드를 쓰는지까지는 더 파 보지 않았다. 다만
**설정 두 벌이 서로를 무효로 만들고 있다는 사실은 측정으로 확정됐다.**

### 3. 플레아 판매 금지가 뒤집힌다

제작자가 `CanSellOnRagfair: false` 로 막아 둔 것이 실제로는 `true` 다.

| 모드 | 막아 둔 항목 | 실제 |
| --- | --- | --- |
| ISB-Aishi | 368건 중 243건 | 전부 판매 가능 |
| Lotus | 개조 랩 키카드 1건 | 판매 가능 |

원인은 `platinum-theblacklist` 의 `Config/config.jsonc` 에 있는 `disableBsgBlacklist: true`
다(주석에 "true 면 블랙리스트를 끈다"고 적혀 있다). 설정대로 동작한 것이지, 사고는 아니다.
다만 **커스텀 모드 제작자가 일부러 숨긴 아이템까지 플레아에 올라온다**는 것은 의도하지 않은
쪽일 수 있다. 일부만 지키려면 같은 파일의 `excludedCategories` 에 분류를 더한다.

## 서버 테스트에서 나온 것 (2026-10-28, 모드 114개)

원문은 [server-tests/server-test-stage9-114mods.out.txt](server-tests/server-test-stage9-114mods.out.txt).
콘솔에는 **오류가 없어 보이지만 로그 파일에는 `[Error]` 49건, `[Warning]` 58건**이 있다.
서버 자체는 정상으로 시작했다(`서버가 시작되었습니다`). 로그 시각은 UTC 다.

| 건수 | 내용 | 판단 |
| --- | --- | --- |
| **48** | `Could not add <id> to cache, it does not exist in the item database!` + 같은 id 의 경고 48건 | **고칠 것.** 아래 참조 |
| 1 | `Unable to add bundle: ...barrel_vpo215_600mm_366tkm.bundle` | 번들 1개 누락 |
| 3 | `[SVM] Remove backpack restrictions - empty filters` | 모드가 "오류 아니다"라고 직접 적어 둔 것 |
| 1 | `트레이더 67d3a28a... 찾을 수 없음` → 기본 새로 고침 3600 으로 대체 | Moe-TradeMarker 의 택배 상인 |
| 1 | `Trader 6747208e... not found, cannot import quest assort` | ISB Aishi |
| 1 | `[RealisticInsurance] trader 'Armorer' ... not listed in insurance.json` → 90% 대체 | 모드가 대체값을 쓴다고 알림 |
| 2 | `[CaseTrader] Skipped container ... no handbook price` | 가격 없는 컨테이너 2개 건너뜀 |
| 1 | `No valid item configs found in ...Eukyre-BetterOKP7\db\CustomItems` | 빈 폴더 |
| 1 | `TraderGenPocketMigration` 프로필 마이그레이션 실행됨 | 정상 처리 |

### 48건의 정체 — Olympus 가 모드 총기에 없는 탄창을 끼운다

없는 아이템 id 48개를 되짚었더니 **41개가 `MountOlympus/Modifiers/firearmMags.json` 의 탄창
값**이었다(총기 열쇠는 아니다). 나머지 7개는 뒤에 따로 적는다.

8차에서 같은 파일을 고친 것은 **없는 총기** 12개를 뺀 것이었다
([mod-patches/505-olympus-firearmMags.md](mod-patches/505-olympus-firearmMags.md)). 이번 것은
총기가 아니라 **탄창 쪽**이고, 원인도 다르다.

**어디서 오는 탄창인가 — 어디에도 없다.** 아래를 전부 확인했다.

| 찾은 곳 | 결과 |
| --- | --- |
| SPT 기본 DB (`SPT_Data/database/templates/items.json`, 4673개) | 41개 전부 없음 |
| 지금 서버 DB (9483개) | 41개 전부 없음 |
| 설치된 모드의 json·jsonc 전체 | `firearmMags.json` 말고는 없음 |
| 서버 모드·플러그인 DLL 295개의 문자열(ASCII·UTF-16) | 없음 |

**Olympus 가 만들지 않는 것이다.** 이 파일의 구조는 `총기 id → { maglist: [탄창 id…] }` 이고,
Olympus 가 하는 일은 그 탄창 id 를 **총기의 탄창 칸 필터에 끼워 넣는 것뿐**이다. 서버 로그에
남는 Olympus 메시지도 `AddMagazinesToFirearms` 209건이 전부다 — 아이템을 만든다는 기록은 없다.

갈리는 지점은 총기 출처다.

| 총기 출처 | 탄창 참조 | 있음 | 없음 |
| --- | --- | --- | --- |
| 바닐라 | 203 | **203** | 0 |
| 모드(WTT-Armory 등) | 181 | 118 | **63** (서로 다른 id 41개) |

바닐라 총기에 끼우는 탄창은 Olympus 가 **자기 것으로 만들어 둔 80종**이다(이름이 `ArSL`·`ArM4w`
처럼 DLL 에 박혀 있고, 번들은 바닐라 탄창 것을 그대로 쓴다). 문제는 **모드 총기 쪽만**이다.

표본 — WTT-Armory AXMC 의 탄창 칸 필터:

| id | 상태 |
| --- | --- |
| `68a387672d70af2b1e359602` | **있음** — `[WTT-Armory]_(AI AXMC .300 WM 10-round magazine)` |
| `436d497b0343516d521250c3` | 없음 — Olympus 가 끼워 넣은 것 |

즉 **총기는 자기 탄창을 이미 갖고 있고, Olympus 가 없는 것을 하나 더 끼워** 서버가 그것을
플레아 캐시에 넣으려다 실패한다. 게임 기능이 막히는 것은 아니다.

**다른 모드를 더 깔아서 메울 수는 없다.** 그 41개가 어느 모드 것인지 단서가 없다 — 이름도,
번들도, 어떤 파일에도 흔적이 없다. Olympus 가 자기 판에 맞춰 적어 둔 id 인데 지금 설치된
WTT-Armory 2.0.5 와 맞지 않는 것으로 보인다(WTT 계열은 아이템 id 를 만들어 쓰므로 판이 다르면
id 가 어긋난다). 그래서 **빼는 것이 맞다.** 빼면 오류 48 → 7 로 줄고, 총기는 원래 자기 탄창을
그대로 쓴다.

### 남은 7개는 출처를 못 찾았다

`firearmMags.json` 과 무관한 7개(`0d084425…`, `4cc2529d…`, `5e879819…`, `75c571c4…`,
`8c4d94ec…`, `ce89fa83…`, `e4e53415…`)는 다음 어디에도 없었다.

- SPT 기본 DB, 지금 서버 DB
- 설치된 모드의 파일 전체(json 이 아닌 것도 포함)
- DLL 295개의 문자열
- 프로필(`6adc26693f5b5290609ea366.json`)
- 서버 캐시(`user/sptappdata`)
- 지금 DB 에 있는 아이템들의 칸·탄창·격자 필터

플레아 생성 중에만 참조되므로 **실행 중에 만들어졌다가 사라지는 id** 로 보인다. 어느 모드가
그러는지는 코드를 들여다보지 않고는 못 가린다.

그 밖에 눈에 띈 것.

- **`[ItemInfo] Completed processing 9483 items in 92732 ms`** — 서버 시작 시간의 대부분이
  이 한 모드다. 90초를 줄이고 싶으면 여기가 유일한 큰 덩어리다
- `[MeleeMultiplier] damage multiplier x10!` — MagicBullet 의 근접 슬라이더를 x1 로 둬야 하는
  이유가 그대로다
- `Snapshot captured (8882 items), no enabled deltas found.` — SIM 은 여전히 아무것도 바꾸지 않는다
- SlotCategoryRelax 가 **분류 14종**(이전 15종)으로 돌았다. Flashlight·Foregrip·TacticalCombo 가
  켜지고 Handguard·PistolGrip·Stock 이 꺼져 더한 항목이 168044 → 91037 로 줄었다. 설정이 바뀐 것이다

## 그래서 무엇을 할까

| 할 일 | 내용 |
| --- | --- |
| 0 | **Olympus `firearmMags.json` 에서 없는 탄창 41개를 뺀다.** 서버 오류 48건 중 41건이 이것이다. 모드 총기 쪽만 해당하고, 그 총기들은 자기 탄창을 이미 갖고 있다. 다른 모드를 깔아 메울 길은 없다(위 절 참조). 8차 때와 같은 방법·같은 백업 자리 |
| 1 | **CobrasAmmoStack 을 뺀다.** 지금 아무 일도 하지 않는다. 500 발 스택을 쓰고 싶다면 반대로 SVM 의 `Items.AmmoStacks` 를 비우고 Cobra 에 맡긴다 — 둘 다 켜 두는 것만은 피한다 |
| 2 | **SVM 화폐 네 줄을 BarterItemsStacks 값과 맞추거나 포기한다.** 지금은 SVM 설정창에서 고쳐도 게임에 아무 변화가 없다 |
| 3 | TheBlacklist 의 부작용을 받아들일지 정한다. 모드 전용 아이템을 숨기고 싶으면 `excludedCategories` 를 쓴다 |
| 4 | **순서를 손대야 할 일이 생기면 DLL 수정이 아니라 통합 모드로 간다.** 마지막에 최종값을 쓰는 모드를 하나 두는 쪽이다. 그 모드는 `TypePriority` 를 명시해야 하고, **어느 방향이 나중인지는 아직 실측하지 않았다** — 자작 모드 3개가 모두 0 을 쓰고 있어 비교 대상이 없다. 탐침 모드를 하나 넣어 한 번 재면 끝난다 |
| 5 | ~~기록 없는 8개 모드의 설치 기록을 만들고 문서를 다시 생성한다~~ → **2026-10-28 완료.** 위 절 참조. 결속 3개(More Energy Drinks·ISB Aishi·WTT Content Backport)는 설치 직전 프로필 백업이 없었는데, SPT 자체 백업에서 건져 `backup/profiles-before-stage9-recovered` 에 뒀다 |

## 쓴 방법 (다시 재려면)

| 측정 | 방법 |
| --- | --- |
| TypePriority 값 | `Mono.Cecil` 로 모드 DLL 의 `[Injectable]` 생성자 인수를 읽는다 |
| 최종 DB 값 | 서버가 떠 있을 때 `POST https://127.0.0.1:6969/client/items` (본문 없이, 인증서 검증 끔). 응답은 deflate 압축 JSON |
| 죽은 설정 찾기 | 모드 설정에서 `24자리 아이템 id → 값` 꼴을 모아 위 DB 와 대조 |

`POST /client/items` 는 **본문을 비워 보내야** 한다. `{}` 를 실어 보내면 200 에 빈 응답이 온다.
