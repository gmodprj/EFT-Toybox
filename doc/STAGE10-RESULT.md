# 10차 설치 결과

계획은 [PLAN-STAGE10.md](PLAN-STAGE10.md). 단계를 끝낼 때마다 여기에 실측값만 적는다.

## 기준선 (10차 시작 전, 2026-11-04)

| 항목 | 값 |
| --- | --- |
| 게임 플러그인 | 147 (`147 plugins to load`) |
| 서버 모드 | 115 (`ModLoader: 총 115 개`) |
| 서버 시작 | 약 118초 |
| 알려진 서버 오류 | `Could not add` 48 + `Unable to add bundle` 1 |

자세한 내역은 [PLAN-STAGE10.md](PLAN-STAGE10.md) §10-0-1.

## 10-A — Easy Slot Filters (2940) · 2026-11-05

| 항목 | 결과 |
| --- | --- |
| 전제 | All In Weapon(2576) 제거 확인 — 기록이 `install-records/removed/` 에 있고(`a4cfb3d`), `user/mods/AllinWeaponUnslotted` 없음, 게임 로그 흔적 0 |
| 받기 | 6075 바이트 (매니페스트 6084, **9바이트 차이**) — 압축은 정상 해제, DLL 1개뿐이라 메타데이터 차이로 본다 |
| 겹침 검사 | CLEAN (0/0/0) |
| 설치 | 1파일 |
| 서버 모드 | 115 → **116** (= 기준선 + 1) |
| 시작 시간 | 135초 |
| 로드 확인 | `모드 Easy Slot Filters 버전 1.0.0 (GUID: ca.bushtail.easyslotfilters) 불러옴`, `(Added 1 mods: ...)` |
| 설정 | `config.jsonc` 자동 생성 — 기본 예시(MP-43-1C ← 톱질 총열, `mod_barrel`) |
| 오류 | 새 오류 0. `Could not add` 48 · `Unable to add bundle` 1 — 기준선과 같다 |

출력: `server-tests/server-test-stage10-A-2940-EasySlotFilters.out.txt`

**게임 확인 통과 (2026-11-05, 사용자 확인).** 기본 예시 한 줄대로 MP-43-1C 에 톱질 총열이 **실제로 달린다.** 계획 §10-A 가 "이 한 줄이 되면 구조가 맞는 것"으로 둔 관문을 넘었다 — 무기 id → 부착물 id → 칸 이름을 적는 방식 자체는 동작한다.

다만 이것은 **바닐라 무기·바닐라 부착물** 조합이다. 모드가 추가한 무기·부착물은 ESF 가 도는 자리(`TraderRegistration - 1` = 299,999)에서 아직 DB 에 없어 조용히 넘어간다([CANDIDATES-ATTACH-COMPAT.md](CANDIDATES-ATTACH-COMPAT.md) 의 정정 항목). 그쪽까지 되는지는 **10-L(아이템 추가 결속 모드) 뒤에 다시 봐야** 한다.

## 10-B — 선행·프레임워크 7개 · 2026-11-05

| 항목 | 결과 |
| --- | --- |
| 받기 | 7개 모두 바이트 수 **정확히 일치**. `.7z` 2개도 bsdtar 로 풀렸다 |
| 버전 차이 | Quest Codex 는 계획의 1.11.0 이 아니라 **1.11.1** 이 최신이었다 |
| 겹침 검사 | CLEAN (0/0/0), 설치 대상 121개 |
| 설치 | 6개 `-SkipTest` + 마지막 3095 에서 테스트 한 번 |
| 설치 기록 | 7개 합계 **121줄 = 겹침 검사의 121개** |
| 서버 모드 | 116 → **120** (= 서버 쪽 4개: Item Property Backport · MapVariants · QuestCodex · Hj 의 Mod Source) |
| 시작 시간 | 135초 |
| 로드 확인 | `(Added 4 mods: Hj's Mod Source (1.1.0), MapVariants (1.1.0), Item Property Backport (1.5.2), QuestCodex (1.11.1))` |
| 오류 | 새 오류 0. 기준 문구 48 · 1 그대로 |

출력: `server-tests/server-test-stage10-B-3095-IntegratedConfigurationManager.out.txt`

계획과 달랐던 것

- 계획은 MapVariants·Quest Codex 가 설정 파일을 만든다고 적었으나 **서버 시작 때 아무것도 만들지 않았다.** QuestCodex 는 `Data/` 와 웹 화면에, MapVariants 는 게임 쪽에 설정이 있다. 대신 Item Property Backport 와 Hj 의 Mod Source 는 `config.json` 을 압축에 담아 온다.
- Quest Codex 는 **별도 포트를 열지 않는다** — 서버 포트 안에 `/questcodex` 를 붙인다(`Mod QuestCodex has a wwwroot, mapping to /questcodex/`). WebConfigBridge 와 겹칠 일이 없다.

## 10-C — 편의·인벤토리 11개 · 2026-11-05

| 항목 | 결과 |
| --- | --- |
| 받기 | 11개 모두 바이트 수 **정확히 일치** (6.62 MB). `.7z` 4개도 bsdtar 로 풀렸다 |
| 버전 차이 | Cabinet Looter 는 계획의 1.0.3 이 아니라 **1.0.4** 가 최신이었다 |
| 구성 | 전부 비결속·의존성 없음. **서버 쪽 2개**(MergeConsumables·Gilded Key Storage), 게임 전용 9개 |
| 겹침 검사 | CLEAN (0/0/0), 설치 대상 29개 |
| 설치 | 10개 `-SkipTest` + 마지막 2344 에서 테스트 한 번 |
| 서버 모드 | 120 → **122** (= 서버 쪽 2개) |
| 시작 시간 | 150초 |
| 로드 확인 | `(Added 2 mods: MergeConsumablesServer (1.6.2), Gilded Key Storage (2.1.1))` |
| 오류 | 새 오류 0. 기준 문구 48 · 1 그대로, `err.txt` 0바이트 |

출력: `server-tests/server-test-stage10-C-2344-DynamicItemWeights.out.txt`

### 설치 기록에 구멍이 있었다 — 메우고 간다

테스트 중에 Gilded Key Storage 가 자기 설정 `config\config.json` 을 만들었는데, 그때 돌던 태그가 **2344 것**이어서 스크립트가 "다른 모드가 만든 파일"로 분류하고 **어느 기록에도 넣지 않았다.**

```
OTHERMOD SPT_Runtime\user\mods\DrakiaXYZ-GildedKeyStorage\config\config.json (다른 모드가 만든 파일 — 기록하지 않음)
```

기록으로만 지우면 이 파일이 남는다. `installed-stage10-C-865-GildedKeyStorage.txt` 에 한 줄 더해 메웠다(11줄 → 12줄).

**묶음 설치(`-SkipTest` 여러 개 + 마지막 하나 테스트)의 구조적 약점이다.** 테스트가 마지막 태그로 돌기 때문에, 그 사이 **다른 모드**가 만든 설정 파일은 기록에서 빠진다. 10-A 처럼 하나씩 넣을 때는 생기지 않는다(ESF 의 `config.jsonc` 는 제대로 기록됐다).

다음 묶음 단계에서는 테스트 출력의 `OTHERMOD` 줄을 **반드시 읽고**, 이번 단계에서 넣은 모드의 것이면 해당 기록에 더한다.

검산 — 10차 기록 전체(10-A·10-B·10-C) **153줄이고 적힌 파일이 전부 실재한다**(누락 0). 2 + 121 + 30 으로 맞는다. 즉 지금 기록만으로 10차 설치분을 전부 되돌릴 수 있다.

### 그 밖에 적어 둘 것

- MergeConsumables 서버 DLL 은 스스로를 **1.6.2** 로 알린다(모드 페이지·매니페스트는 1.6.3). 제작자가 DLL 버전을 올리지 않은 경우로, [PROCEDURE.md](PROCEDURE.md) 의 "알려진 정상 메시지" 에 같은 사례가 있다.
- **아직 확인되지 않은 것** — 게임 전용 9개는 이번 설치 뒤로 게임을 띄운 적이 없어 로드된 적이 없다. 다음 기동에서 플러그인 수가 **151 → 160** 이 되어야 맞다. 인벤토리·치료 화면을 함께 건드리므로 인벤토리를 한 번 열어 보는 것이 이 단계의 본 시험이고, Dynamic Item Weights(2344)는 SVM 의 무게 옵션과 겹치니 아이템 무게도 한 번 본다.

## 10-D — UI·표시 12개 · 2026-11-05

| 항목 | 결과 |
| --- | --- |
| 받기 | 12개 모두 바이트 수 **정확히 일치** (44.38 MB, ImpactCue 혼자 40.4 MB) |
| 버전 차이 | Briefing 은 계획의 1.0.2 가 아니라 **1.1.0** 이 최신이었다 |
| 선행 | Briefing(3158)이 요구하는 Quest Codex(3054)는 10-B 에서 들어가 있다 |
| 겹침 검사 | 처음 **ATTENTION(4)** → 섞여 온 파일을 빼내고 CLEAN. 설치 대상 312개 |
| 설치 | 11개 `-SkipTest` + 마지막 1533 에서 테스트 한 번 |
| 설치 기록 | 313줄 (= 312 + 생성된 설정 1) |
| 서버 모드 | 122 → **127** (= 서버 쪽 5개) |
| 시작 시간 | 144초 |
| 로드 확인 | `(Added 5 mods: ImpactCue Web Config (2.0.3), Hj's Improved Customization UI (1.1.2), Name Plate (1.1.0), Wishlist Extended (1.2.0), Easy Ammo Names (2.1.0))` |
| 오류 | 새 오류 0. 기준 문구 48 · 1 그대로, `err.txt` 0바이트 |

출력: `server-tests/server-test-stage10-D-1533-MoreTagColours.out.txt`

### 압축에 섞여 온 파일 4개를 설치에서 뺐다

`check-conflicts.mjs` 가 CLEAN 이 아니라 **ATTENTION(4)** 를 냈다. 겹침은 0이지만 설치 경로(`BepInEx/`·`SPT_Runtime/`…) 밖의 파일이 4개 있었다. `install-and-test.ps1` 은 **해제본의 모든 파일을 재귀 복사**하므로 그대로 두면 이것들이 `E:\Spt` 루트에 들어간다.

| 파일 | 출처 | 왜 뺐나 |
| --- | --- | --- |
| `LICENSE`, `README.md` | Wishlist Extended (2500) | 문서다. 설치본에 둘 자리가 없다 |
| `README.md` | Inventory Walker (3058) | 같다 |
| `Name Plate.exe` (1.3 MB) | Name Plate (3131) | **실행 파일**이다. 아이콘·플레이트를 편집하는 동반 프로그램으로 보인다(`NamePlate`·`config`·`json` 문자열 확인). 모드 동작은 플러그인·서버 모드가 하므로 **일단 넣지 않았다** |

빼낸 것은 `extracted/stage10-D/_excluded/` 에 그대로 두었다 — `check-conflicts.mjs` 와 설치 스크립트가 `_` 로 시작하는 폴더를 건너뛴다. 되살리려면 그 자리에서 꺼내면 된다.

`Name Plate.exe` 는 **받은 파일 중 유일한 실행 파일**이라 보고 넘긴다. 이 저장소의 `AirdropCaller.exe` 처럼 설치본 루트에 두는 동반 도구가 선례로 있지만, 그것은 우리가 만든 것이다. 플레이트 편집에 이 프로그램이 꼭 필요한지는 확인하지 않았다 — 필요하면 그때 따로 넣는 것이 낫다.

### 설치 기록 구멍을 이번에는 미리 잡았다

10-C 에서 배운 대로 `OTHERMOD` 줄을 읽었고, 또 하나 나왔다.

```
OTHERMOD SPT_Runtime\user\mods\ImprovedCustomizationUI\config.json (다른 모드가 만든 파일 — 기록하지 않음)
```

Hj's Improved Customization UI(3069)가 테스트 중에 만든 자기 설정인데 마지막 태그(1533)로 돌던 중이라 빠졌다. 3069 기록에 한 줄 더해 메웠다(2줄 → 3줄).

검산 — 10차 기록 전체 **466줄이고 적힌 파일이 전부 실재한다**(2 + 121 + 30 + 313, 누락 0).

### 다음 게임 기동에서 볼 것

10-C·10-D 가 `BepInEx\plugins` 에 넣은 DLL 은 **23개**(10-C 11 + 10-D 12, Briefing 이 2개)다. 그러니 플러그인 수는 **151 → 174 근처**가 되어야 한다. 다만 DLL 수와 플러그인 수는 같지 않다(라이브러리는 0, 하나가 여러 개를 등록, GUID 충돌은 조용히 버려짐 — 위 10-B 참조). 어긋나면 실패로 단정하기 전에 `Loading [` 목록을 비교한다.

함께 볼 것 — 10-D 는 문자열을 많이 바꾸므로 **한국어 표기**가 깨진 곳이 없는지, 그리고 같은 화면을 건드리는 겹침(Hj 의 개조 화면 ↔ 설치된 개조 화면 모드, Menu Overhaul ↔ 설치된 메뉴 계열)을 본다. 10-C 쪽은 인벤토리·치료 화면과 아이템 무게다.

## 게임 기동 확인 (10-A·10-B 설치 후, 2026-11-05 07:35)

`BepInEx/LogOutput.log` 수정 시각 07:35 > 설치 06:52 — **설치 이후 기동이 맞다.**

| 항목 | 값 | 기준선 대비 |
| --- | --- | --- |
| `plugins to load` | **151** | 147 → +4 |
| `Loading [` 줄 | **151** | **두 수가 일치한다** |
| `[Error` / `[Fatal` | **1줄 / 0줄** | 15 → 1 |

### 플러그인 수가 +4 인 이유 — 셈이 정확히 맞는다

10-B 가 `BepInEx\plugins` 에 넣은 DLL 은 5개인데 플러그인 수는 4만 늘었다. 셋을 확인해 전부 설명됐다.

1. **Effects Manager Framework(3123)는 플러그인이 아니라 라이브러리다.** `EFTEffectManager.dll`(73,728바이트)에 `BepInPlugin`·`BaseUnityPlugin`·`BepInDependency` 문자열이 **하나도 없다.** 그래서 `plugins to load` 에 잡히지 않고 `Loading [` 줄도 없으며, 실패 메시지도 없다 — **정상이다.** 이것을 쓰는 Broken Limbs Don't Heal(3121, 10-E2)이 로드될 때 참조로 끌려 올라간다. 그때 실제로 동작하는지는 10-E2 에서 본다.
2. **ICM(3095)은 플러그인을 둘 등록한다** — `Integrated Configuration Manager 1.0.0` 과 `Configuration Manager 18.4`. DLL 하나 안에 고전 ConfigurationManager 를 품고 있다(`com.bepis.bepinex.configurationmanager` 문자열 확인).
3. **그런데 그 GUID 는 SPT 가 이미 쓰고 있다** — `BepInEx/plugins/spt/ConfigurationManager/ConfigurationManager.dll`. `Loading [` 목록에 중복 이름이 없고(`uniq -d` 빈 출력) 중복 경고도 없으므로, **둘 중 하나만 로드되고 나머지는 조용히 버려진다.**

그래서 ICM 의 실질 증가는 +2 가 아니라 +1 이고, 합이 1(WebOverlay) + 1(MapVariants) + 1(Hj 의 Mod Source) + 1(ICM) + 0(라이브러리) = **+4** 로 맞는다.

### 여기서 생긴 위험 — F12 한글화

`SptKoreanConfig` 는 F12 설정 창을 한글로 바꾸는데, 그 번역 사전이 **바로 그 GUID 를 대상으로 한다**(`BepInEx/plugins/SptKoreanConfig/lang/com.bepis.bepinex.configurationmanager.json`). ICM 이 같은 GUID 를 들고 들어왔으므로 **어느 쪽 ConfigurationManager 가 로드되는지가 바뀔 수 있고**, 그러면 한글 표기가 어긋날 수 있다. 로드된 것은 `Configuration Manager 18.4` 로 찍히지만, 그것이 SPT 것인지 ICM 것인지는 로그만으로 가릴 수 없다.

**확인 결과 — 멀쩡하다 (2026-11-05, 사용자 확인).** F12 설정 창의 한글 표기가 그대로다. 즉 이 GUID 충돌은 이 설치본에서 **무해하다** — 로드된 쪽이 번역 사전과 맞는다. ICM 을 뺄 이유가 없다.

그래도 이 충돌 자체는 남아 있다. 나중에 ICM 이나 SPT 쪽이 올라가면 **버려지는 쪽이 바뀔 수 있다.** 그때 F12 한글이 깨지면 원인은 여기다 — ICM 을 빼는 것이 가장 빠른 수습이다(ICM 은 설정 화면을 모아 주는 편의 모드일 뿐이고, 한글화는 이 설치본에서 더 오래 썼다).

### 새 모드들이 남긴 것

- `[Anvil-WebOverlay] Anvil-WebOverlay 1.11.0 ready.` — 올라왔다. 다만 **포트를 여는지는 로그에 안 찍혔다.**
- `[MapVariants] [MV] lifecycle armed` 외 7줄 — 훅을 전부 걸었다.
- Hj 의 Mod Source 는 서버 쪽이 `/modsource/db` 요청을 받았다(`REQUEST SUCCESSFUL`).

### 오류가 15 → 1 로 줄어든 것은 좋아진 것이 아니다

남은 1줄은 알려진 `DirectReload` + Fika 건(정상)이다. 어제의 15줄은 SAIN `sectactPriestEvent` 6, STM `AimingFailure` 6·`MedicalActivity` 1, ECOTI per-frame 1 이었는데, 그것들은 **레이드를 깊게 돌아야 나온다**. 어제 로그에는 프레임 55850 까지 있었고 이번 기동은 그만큼 돌지 않았다. 즉 **줄어든 것이 아니라 아직 안 나온 것**이다 — 다음 단계에서 15줄이 다시 보여도 새 오류가 아니다.

### 어제의 "미해명 39" 는 어제 로그에 한정된 것이었다

기준선에 `147 plugins to load` 인데 `Loading [` 줄이 108개뿐이라고 적었다. 이번 로그는 **151 = 151 로 정확히 일치한다.** 어제 그 파일에만 있던 현상이고(그 로그는 덮여 사라졌다) 지표 자체는 믿을 수 있다. 앞으로는 두 수를 **함께** 세서 어긋나면 그 로그를 의심한다.

## 10-E — 퀘스트·하이드아웃·진행 12개 · 2026-11-05

| 항목 | 결과 |
| --- | --- |
| 프로필 백업 | `backup/profiles-before-stage10-E/` 에 7개, **원본과 크기 전부 일치** 확인 |
| 받기 | 12개 모두 바이트 수 **정확히 일치** (20.27 MB, Tarkin's Hideout Cat 혼자 19.9 MB) |
| 출처 | Hideout Overhaul(2982)만 **codeberg.org** 경유였다(나머지는 GitHub 릴리스) |
| 겹침 검사 | CLEAN (0/0/0), 설치 대상 53개 |
| 설치 | 11개 `-SkipTest` + 마지막 2652 에서 테스트 한 번 |
| 설치 기록 | 55줄 (= 53 + 생성된 파일 2) |
| 서버 모드 | 127 → **136** (= 서버 쪽 9개) |
| 시작 시간 | 159초 |
| 로드 확인 | `(Added 9 mods: Cultist Circle Improvements, Simple Workout QTE, HideoutOverhaul, FairProgression, Expanded Task Text, Health Per Level, Hideout In Progress, All Quests Checkmarks, Skill Distribution)` |
| 오류 | 새 오류 0. 기준 문구 48 · 1 그대로, `err.txt` 0바이트 |

출력: `server-tests/server-test-stage10-E-2652-FairProgression.out.txt`

### 프로필은 상한 데가 없다

이 단계는 결속 표시가 없어도 프로필을 읽는 것들이라, 설치 전 백업과 설치·서버 시작 후를 비교했다.

| 프로필 | 레벨 | 스킬 수 | 최대체력 합 | 아이템 수 |
| --- | --- | --- | --- | --- |
| Nomad · Banshee · Knox · Hawk · Buck · Mace · Raptor (7개) | 그대로 | 그대로 | 그대로 (995) | 그대로 |

**다만 "효과가 보인다"는 뜻은 아니다.** Health Per Level(1706)을 넣었는데 레벨 69 캐릭터의 최대체력 합이 995 그대로이고, Skill Distribution(2144)을 넣었는데 스킬 수도 그대로다. 설정이 기본값이라 아무것도 안 하는 것일 수도, 레이드 진입 시점에 적용되는 것일 수도 있다 — **확인하지 않았다.** 이 단계가 통과한 것은 "프로필이 깨지지 않았다"까지다.

### 생성 파일이 둘 나왔고 둘 다 기록에 넣었다

```
OTHERMOD SPT_Runtime\user\mods\acidphantasm-simpleworkoutqte\config.json
OTHERMOD SPT_Runtime\user\mods\ExpandedTaskText\Resources\descriptionCache.json
```

Simple Workout QTE(1437)의 설정과 Expanded Task Text(2153)의 설명 캐시다. 각 기록에 한 줄씩 더했다(53 → 55줄). 캐시도 넣은 이유는 **기록만으로 지웠을 때 남지 않게** 하기 위해서다.

### 그 밖에

- **Skill Distribution 의 DLL 은 스스로를 1.2.2 로 알린다**(페이지·매니페스트는 1.3.0). MergeConsumables 와 같은 경우로, 제작자가 DLL 버전을 올리지 않은 것이다.
- `[CCI] Added 25 Cultist Circle crafts utilizing WTT-Content Backport` — Cultist Circle Improvements 가 설치된 WTT Content Backport 를 알아보고 제작법 25개를 더했다.

## 10-E2 — Broken Limbs Don't Heal (3121) 단독 · 2026-11-05

선행 Effects Manager Framework(3123)가 **플러그인이 아니라 라이브러리**라 플러그인 수에 잡히지 않았다(10-B 참조). 그것이 참조로 제대로 끌려 올라가는지가 이 단계의 시험이라 단독으로 넣었다.

| 항목 | 결과 |
| --- | --- |
| 받기 | 8775바이트, 매니페스트와 일치 |
| 겹침 검사 | CLEAN (0/0/0), 설치 대상 2개 |
| 설치 | 2파일, 게임 전용 |
| 서버 모드 | **136 유지** (게임 전용이라 늘지 않는 것이 맞다) |
| 시작 시간 | 129초 |
| 오류 | 새 오류 0 |

**아직 확인되지 않은 것** — 게임 전용이라 **서버 테스트로는 선행 참조가 풀렸는지 알 수 없다.** 다음 기동에서 `Loading [Broken Limbs ...]` 줄이 뜨는지, Effects Manager 관련 경고가 없는지를 봐야 한다. 골절이 레이드 종료 후 유지되는지는 레이드를 한 번 돌아야 안다.

## 10-F — 전투·조작·이동 12개 · 2026-11-05

| 항목 | 결과 |
| --- | --- |
| 받기 | 11개 바이트 일치. **CookingGrenades 만 7바이트 차이**(1801939 / 1801946) — 압축은 정상 해제되고 안에 DLL 1개뿐이라 10-A 와 같은 메타데이터 차이로 본다 |
| 출처 | Increase Look Direction(2121)만 **gitlab.com** 경유 |
| 겹침 검사 | CLEAN (0/0/0), 설치 대상 32개 |
| 설치 | 11개 `-SkipTest` + 마지막 3067 에서 테스트 한 번 |
| 설치 기록 | 32줄, 적힌 파일 전부 실재 |
| 서버 모드 | 135 → **137** (= 서버 쪽 2개: ClassicMovement · Increase Climb Height) |
| 시작 시간 | 135초 |
| 오류 | 새 오류 0. 기준 문구 48 · 1 그대로, `err.txt` 0바이트 |

출력: `server-tests/server-test-stage10-F-3067-Deadzone.out.txt`

### 2025 제거가 서버 쪽에서 확인됐다

이 테스트 출력이 직접 찍었다.

```
(Added 2 mods: ClassicMovement (1.1.3), Increase Climb Height (2.1.1);
 Removed 1 mods: All Quests Checkmarks (1.4.0))
```

`ModLoader: 총 137 개` 도 135 + 2 로 맞는다(2025 가 아직 로드되면 138 이었을 것이다). 출력 전체에 `AllQuestsCheckmarks` 언급이 **0건**이다. 앞 절의 "제거 후 서버를 시작해 보지 않았다" 는 이로써 해결됐다.

### 다음 게임 기동에서 볼 것

10-F 가 `BepInEx\plugins` 에 넣은 DLL 은 12개인데 그중 `tarkin-ladders\tarkin.ladders.shared.dll` 은 **라이브러리**다(플러그인 속성 없음). 그래서 플러그인은 11개이고, 예상치는 **181 − 1(2025 제거) + 11 = 191** 이다.

이 단계는 **서버 테스트로 검증되지 않는다** — 전부 조작감이다. 레이드를 한 번 돌아야 하고, 이상하면 먼저 의심할 여섯이 정해져 있다.

- 이동 넷 — Classic Movement(1860) · Smajlec's Stamina(2608) · Increase Climb Height(1575) · Climbable Ladders(2649)
- 조준 둘 — Deadzone(3067) · No Aim Swing(2719). **둘 다 조준 처리를 겹쳐 바꾼다** — 하나만 쓰는 쪽이 안전하다

Climbable Ladders(2649)는 10-O 의 ICEBREAKER(2866) 선행이기도 하므로, 여기서 빼기로 하면 그쪽도 함께 막힌다.

## 게임 기동 확인 (10-F 설치 후, 2026-11-05 10:40)

로그가 설치(10:10)보다 뒤인지 기다렸다가 플러그인 로드가 끝난 뒤 읽었다.

| 항목 | 값 | 예상 |
| --- | --- | --- |
| `plugins to load` | **191** | **191** — 맞다 |
| `Loading [` 줄 | **191** | 일치, `Chainloader startup complete` |
| `[Error` / `[Fatal` | **1줄 / 0줄** | 25 → 1 |
| `AllQuestsCheckmarks` 언급 | **0건** | 사라져야 한다 — 맞다 |

### 2025 제거의 게임 쪽 확인이 끝났다

앞 기동의 오류 25줄 가운데 24줄이 All Quests Checkmarks 것이었는데, 이번 로그에는 **그 모드 언급이 한 건도 없고 오류는 1줄**(알려진 `DirectReload` + Fika 건)뿐이다. 서버 쪽은 10-F 테스트의 `Removed 1 mods` 로 이미 확인됐으므로, 이로써 양쪽 다 닫혔다.

### 10-F 플러그인 11개 전부 로드됐다

| 모드 | `Loading [` 줄 이름 |
| --- | --- |
| Bring Back Concussion | `Bring Back Concussion 1.2.1` |
| CookingGrenades | `CookingGrenades 1.4.4` |
| Inspectionless Malfunctions Reborn | `InspectionlessMalfsReborn 1.0.0` |
| Deminvincibility | `Deminvincibility 2.2.0` |
| Smajlec 의 Stamina | `SmajlecStamina 1.4.0` |
| Classic Movement | `Classic Movement 1.1.3` |
| Climbable Ladders | `tarkin.ladders.bep 1.0.4` |
| Increase Look Direction | `IncreaseLookDirection 1.3.0` |
| Auto Shoulder Swap | `hazelify.StanceSync 1.1.1` |
| **No Aim Swing** | **`No Aim OverSwing 1.2.0`** |
| Deadzone | `Manimal-Deadzone 1.2.0` |

Increase Climb Height(1575)는 **서버 전용**이라 플러그인이 없다.

**이름으로 찾을 때 걸린 것** — No Aim Swing 은 자기를 `No Aim OverSwing` 으로 알린다(GUID `ciallo.noaimswing`). 모드 이름으로 로그를 훑으면 놓친다. 수가 맞는데 이름이 안 보이면 **DLL 안의 문자열에서 플러그인 표시 이름을 읽어** 확인한다.

### 적어 둘 것

- **CookingGrenades 는 로그를 중국어로 찍는다**(`[GrenadeWheel] 已初始化` 등). 오류가 아니고 패치도 전부 `已启用` 로 걸렸다. 이 모드만 로그 언어가 다르다는 것을 알아 두면 다음에 당황하지 않는다.

### 아직 확인되지 않은 것

- **레이드가 얕았다** — `GameWorld`·`RaidStartPatch` 흔적 15건이고, 기준선의 SAIN·STM·ECOTI 14줄이 안 나왔다. 즉 **깊은 레이드에서의 오류 비교는 아직 못 했다.**
- **조작감은 로그로 알 수 없다.** 걷기·달리기·스태미나, 사다리와 넘기 높이, 조준 흔들림, 수류탄 쿠킹과 섬광은 몸으로 확인해야 한다. 이상하면 이동 넷과 조준 둘을 먼저 의심한다(위 10-F 절).

## 게임 기동 확인 (10-E·10-E2 설치 후, 2026-11-05 09:49)

`LogOutput.log` 09:49 > 설치 08:50 — 설치 이후 기동이 맞다.

| 항목 | 값 | 앞 기동 대비 |
| --- | --- | --- |
| `plugins to load` | **181** | 173 → +8 |
| `Loading [` 줄 | **181** | 일치, `Chainloader startup complete` |
| `[Error` / `[Fatal` | **25줄 / 0줄** | 1 → 25 |

### 내가 예상한 177 이 틀렸다 — 181 이 맞다

앞 턴에 "게임 쪽 4개라 173 → 177" 이라고 적었는데 **틀렸다.** 게임 전용 모드(Vertical Hideout·Hideout Cat·Target Dummies)와 10-E2 만 세고, **`게임+서버` 모드도 클라이언트 DLL 을 싣는다는 것을 빼먹었다.** 실제 게임 쪽 플러그인은 8개다.

| 모드 | Loading 줄 이름 |
| --- | --- |
| Target Dummies (2932) | `7Bpencil.TargetDummies 0.3.1` |
| Simple Workout QTE (1437) | `acidphantasm-simpleworkoutqte 2.2.2` |
| **Broken Limbs Don't Heal (3121)** | `Broken Limbs Dont Heal 1.1.0` |
| All Quests Checkmarks (2025) | `ZGFueDkx-AllQuestsCheckmarks 1.4.0` |
| Tarkin's Hideout Cat (2908) | `hideoutcat 1.1.1` |
| Vertical Hideout (2530) | `tarkin.hideoutuirevamp 1.1.1` |
| Hideout In Progress (2076) | `Tyfon.HideoutInProgress 3.0.0` |
| Skill Distribution (2144) | `ZGFueDkx-SkillDistribution 1.3.0` |

173 + 8 = **181** 로 맞는다. 판정 기준에 하나 더 붙는다 — **`게임+서버` 로 분류된 모드도 플러그인을 싣는다.** 예상치는 `BepInEx\plugins\*.dll` 기록 줄로 세야 하고, 설치 위치 분류로 세면 안 된다.

### 10-E2 는 통과다

`Loading [Broken Limbs Dont Heal 1.1.0]` 이 떴고 Effects Manager 관련 경고·오류가 없다. 선행(Effects Manager Framework)이 **플러그인이 아니라 라이브러리인데도 참조로 제대로 끌려 올라갔다.** 10-E2 를 단독으로 떼어 둔 이유가 이것이었고, 확인됐다. 골절이 레이드 종료 후 유지되는지는 여전히 레이드를 돌아야 안다.

### 그런데 오류 25줄 중 24줄이 새것이다 — 전부 All Quests Checkmarks (2025)

| 건수 | 내용 |
| --- | --- |
| 12 | `Failed to set custom checkmark in ItemSpecificationPanel!` |
| 12 | `Attempted to remove non-existing quest <id> from quest data!` (서로 다른 id 7종) |
| 1 | 알려진 `DirectReload` + Fika 건 |

기준선의 SAIN·STM·ECOTI 줄은 이번에 안 나왔다 — 레이드 깊이가 얕았다(`GameWorld`·`RaidStartPatch` 11건). 그러니 **25줄 중 24줄이 순수하게 새로 생긴 것**이다.

파헤쳐 보니 셋이 겹친다.

1. **핵심 기능이 실패한다.** 이 모드가 하는 일은 퀘스트 목록에 체크 표시를 그리는 것인데, 그 패치가 12번 실패했다(`ItemSpecificationPanel`).
2. **모드가 추가한 퀘스트를 감당하지 못한다.** "없는 퀘스트" 라고 말하는 id 7종은 **실제로는 프로필에 들어 있다**(각 1~5회). 이 설치본에는 Amonya 가 퀘스트 195개, NATO Quartermaster 가 6개를 더하는 등 모드 퀘스트가 많다 — 그쪽을 자기 데이터에서 찾지 못하는 것이다.
3. **한국어가 없다.** `Couldn't load locale 'kr' - file does not exist! Falling back to en.json` — 이 설치본은 한국어인데 이 모드만 영어로 나온다.

게임은 돌고 레이드도 들어갔으므로 **멈출 수준은 아니다.** 다만 [PLAN-STAGE10.md](PLAN-STAGE10.md) 의 멈춤 판정은 "알려진 정상 메시지 표에 없는 새 오류는 실패로 본다" 이고, 이 모드는 **얻는 것(장식용 체크 표시)보다 내는 소음이 크다.** 이 설치본에는 이미 퀘스트 UI 가 여럿 있다(Expanded Task Text, Quest Codex, Hj 의 Task Item/Turn-in Indicator, Task List Fixes, Hj's Task Search, Pinned Quest HUD).

**권고 — 2025 를 뺀다.** 기록 `installed-stage10-E-2025-AllQuestsCheckmarks.txt`(8줄)의 파일만 지우면 되돌아간다. 프로필에 아무것도 심지 않았으므로 백업 복원도 필요 없다.

### 제거 완료 (2026-11-05, 사용자 승인)

| 단계 | 결과 |
| --- | --- |
| 전제 | 게임·서버 종료 확인. **서버 종료는 사용자가 했다** — 강제 종료는 권한 분류기가 거부했고, 콘솔 앱이라 `CloseMainWindow` 도 `False` 를 돌려줬다 |
| 삭제 | 기록의 **8개 전부**(예행 → 실제, 실패 0). 기록 밖 파일은 건드리지 않았다 |
| 빈 폴더 | `plugins\AllQuestsCheckmarks\locales`·`plugins\AllQuestsCheckmarks`·`user\mods\AllQuestsCheckmarks` 셋을 지웠다 — 서버 모드 로더가 빈 모드 폴더를 보지 않게 |
| 런타임 설정 | `BepInEx/config/com.zgfuedkx.allquestscheckmarks.cfg` 도 지웠다. 기록에 없는 파일이지만, **이 설치본에는 제거된 모드의 설정 파일이 남아 있지 않다**(AIW·SOMF·PIT 모두 0건)는 것을 확인하고 그 상태에 맞췄다 |
| 기록 | `install-records/removed/installed-stage10-E-2025-AllQuestsCheckmarks.md` 로 옮기고 왜 뺐는지를 머리말로 붙였다(AIW·PIT 와 같은 관례) |
| 재생성 | 모드 **228 → 227개**, 파일 **11124 → 11116개**(= 8개) |
| 검산 | 흔적 재검색 0건. 서버 모드 폴더 **135개**(136 → 135, 예상대로). 10차 기록 515줄이고 적힌 파일이 전부 실재한다(달린 기록 없음) |

**아직 확인되지 않은 것** — 제거 후 서버를 시작해 보지 않았다. `ModLoader: 총 135 개` 와, 게임 쪽 플러그인이 181 → **180** 으로 줄고 **그 24줄이 사라졌는지**는 다음 기동에서 확인한다. 폴더·파일 수는 맞지만 그것이 "서버가 뜬다"를 증명하지는 않는다.

## 게임·서버 기동 확인 (10-C·10-D 설치 후, 2026-11-05 08:17~08:22)

사용자가 직접 서버와 게임을 띄웠다. 서버 로그는 사용자가 넘겨 주었고, 게임 로그는 `BepInEx/LogOutput.log`(08:21 기동, 10-D 설치 08:08 이후)를 읽었다.

### 게임 — 플러그인 셈이 또 정확히 맞는다

| 항목 | 값 |
| --- | --- |
| `plugins to load` | **173** |
| `Loading [` 줄 | **173** — 두 수가 일치하고 `Chainloader startup complete` 까지 떴다 |
| `[Error` / `[Fatal` | **1줄 / 0줄** |
| `exception`·`nullreference` | **0건** |

10-C·10-D 가 `BepInEx\plugins` 에 넣은 DLL 은 23개인데 151 + 23 = 174 가 아니라 **173** 이다. 하나를 집어냈다 — **`BepInEx\plugins\Briefing\Briefing.Core.dll` 은 플러그인이 아니라 라이브러리다**(`BepInPlugin`·`BaseUnityPlugin` 문자열 없음). 그래서 23개 = 플러그인 22 + 라이브러리 1 이고 **151 + 22 = 173** 으로 맞는다. 10-B 의 Effects Manager Framework 와 같은 경우다.

**22개 전부 이름으로 확인했다** — MergeConsumables·ContinuousHealing·HealingAutoCancel·HandsAreNotBusy·GildedKeyStorage·UseLooseLoot·Loot In Vicinity·Cabinet Looter·RaidSettingsSkipper·Auto Keycard·Dynamic Item Weights(10-C 11개) + MenuOverhaul·ImpactCue-UI-MOD·Hj's Improved Customization UI·Task Item Indicator·Task Turn in Indicator·Name Plate·Briefing·Breakout Outlines·WishlistExtended·Inventory Walker·MoreTagColours(10-D 11개). Easy Ammo Names(1262)는 서버 전용이라 플러그인이 없다.

남은 오류 1줄은 알려진 것뿐이다.

```
[Error  :Direct Reload & Painless Meds] [DirectReload] Patch FAILED to enable:
FikaHostReloadMirror — Fika ObservedFirearmController not found
```

기준선의 15줄 가운데 나머지 14줄(SAIN·STM·ECOTI)은 **레이드를 깊게 돌아야 나온다**. 이 기동은 메뉴 단계라 안 나온 것이고, 다음에 다시 보여도 새 오류가 아니다.

### 서버 — 127개 전부 정상 시작

사용자 서버 로그 기준.

- `ModLoader: 총 127 개` 로 10-D 뒤 숫자와 같다. 10차에 넣은 서버 모드가 전부 `불러옴` 으로 찍혔다 — Easy Slot Filters, Item Property Backport, MapVariants, QuestCodex, Hj 의 Mod Source, MergeConsumablesServer, Gilded Key Storage, ImpactCue Web Config, Hj 의 Improved Customization UI, Name Plate, Wishlist Extended, Easy Ammo Names.
- `Loaded bundles from 23 mods (ok: 4325, missing: 0, invalid: 0)` — 번들 누락·불량 0.
- `Could not add` 48건 + `Unable to add bundle` 1건으로 **기준선과 같다.** 새 오류는 없다.
- `시작된 웹서버 https://127.0.0.1:6969` → `Loaded self-signed certificate` 까지 정상.
- 웹 화면을 붙이는 모드가 넷이고 경로가 서로 다르다 — `/coti-assets/`, `/TheBlacklist/`, `/questcodex/`, `/SAIN/`. 포트 충돌은 없다.

함께 드러난 것

- **`[MV] no backported location found; nothing to switch`** — MapVariants 는 지금 **할 일이 없다.** 바꿔 끼울 백포트 맵(Manimal Interchange·Lighthouse)이 10-O 라서다. 10-M 의 DynamicMaps Extended 선행으로 미리 넣은 것이니 정상이다.
- **`[ItemInfo] Completed processing 9489 items in 99874 ms`** — 시작 144~150초 가운데 **100초가 이 모드 하나**다. 앞으로 시작 시간이 길다고 새 모드를 의심하기 전에 이 줄을 먼저 본다.
- `pitFireTeam PMC armband enforcement: enabled` 와 `PitFireTeam loaded` 는 **SPT Team Manager** 가 찍는 것이다(업스트림 문자열을 그대로 쓴다). PIT 가 돌아온 것이 아니다.
- 자작 모드도 모두 정상이다 — SlotCategoryRelax 가 총기 915종의 약실을 열고, HelmetSlotUnion 이 늦은 쓸기까지, StackUnify·ScavRaidWaves·AirdropCaller 모두 찍혔다.

### 아직 눈으로 봐야 하는 것

로그로는 여기까지다. **한국어 표기**, 개조·메뉴 화면 겹침(Hj 의 개조 화면 ↔ 기존 개조 화면 모드, Menu Overhaul ↔ 기존 메뉴 계열), 인벤토리·치료 화면, 아이템 무게(Dynamic Item Weights ↔ SVM)는 화면을 봐야 안다.

## 다음 단계의 판정 기준에 더할 것

1. **플러그인 수 증가분은 DLL 개수가 아니다.** 라이브러리 DLL(플러그인 속성 없음)은 0 으로 세고, DLL 하나가 플러그인 여러 개를 등록할 수 있다. 어긋나면 `Loading [` 목록을 직접 비교한다.
1-1. **예상치는 `BepInEx\plugins\*.dll` 기록 줄로 센다.** 겹침 검사의 `게임`·`서버`·`게임+서버` 분류로 세면 틀린다 — `게임+서버` 모드도 클라이언트 DLL 을 싣는다(10-E 에서 4 로 예상했다가 실제 8 이었다).
2. **GUID 충돌은 조용하다.** 같은 GUID 를 가진 플러그인이 둘이면 하나는 경고 없이 버려진다. 프레임워크·설정 화면 계열을 넣은 뒤에는 `Loading [` 목록에서 **사라진 이름이 없는지** 본다.
3. **오류 줄 수는 레이드 깊이에 따라 달라진다.** 적게 나온 것을 통과로 치지 않는다. 비교는 같은 깊이의 기동끼리, 또는 문구별로 한다.
