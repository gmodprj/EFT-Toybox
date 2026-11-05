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

### ImpactCue 는 F12 에 없다 — F10 이다 (2026-11-05 확인)

화면의 HUD 가 어느 모드인지 묻는 질문에 처음엔 "F12 의 ImpactCue" 라고 답했는데 **틀렸다.** 설정 파일이 직접 못박아 둔다.

```
## Open or close the built-in ImpactCue settings window.
## This does not require ConfigurationManager/F12.
Standalone Settings Hotkey = F10
```

제작자 설명도 같다 — *"Most features can be configured directly through the **F10 settings menu**"*, 그 메뉴에 두 칸 배치·분류·검색·**HUD Edit Mode**(위치·크기·색·모양)·소리 미리듣기·다국어가 들어 있다. `Language = Auto` 에 `ko` 가 있어 한국어로도 나온다.

**웹 설정도 있다** — 서버가 돌 때 `https://127.0.0.1:6969/impactcue`. 다만 제작자가 *"still partially unfinished ... mainly a temporary workaround for users who are unable to open the F10 menu"* 라고 적었으므로 **평소에는 F10 을 쓴다.** 서버 모드 `ImpactCue-WebConfig` 가 그 페이지를 띄우는데, `wwwroot` 매핑이 아니라 자체 리스너라서 서버 기동 로그의 `has a wwwroot` 목록에는 안 나온다.

이 설치본의 화면 요소가 어디 있는지 (설정 파일 기준, F10 메뉴의 분류도 같다)

| 화면 | 섹션 |
| --- | --- |
| 상단 나침반 띠 | `[15. Raid Compass]` |
| 좌하단 체력·에너지·수분·무게, 탄약·발사모드·수류탄 | `[13. Compact HUD]`, 투명도는 `[13.4 Default HUD]` |
| 우측 `MAGS` | `[13.9 Magazine HUD]` |
| 위치 옮기기 | `[09. HUD Position]`(드래그) 또는 각 섹션의 X/Y 슬라이더 |

**교훈** — 모드 설정이 BepInEx cfg 파일로 있다고 해서 F12 목록에 나오는 것은 아니다. 자체 창을 띄우는 모드가 있다. cfg 의 주석(`## ... does not require ConfigurationManager/F12`)과 `Hotkey` 항목을 먼저 본다.

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

## 게임 기동 확인 (10-G·10-H 설치 후, 2026-11-05 12:05)

로그가 설치(11:40)보다 뒤인지 백그라운드로 기다렸다가 로드가 끝난 뒤 읽었다.

| 항목 | 값 | 예상 |
| --- | --- | --- |
| `plugins to load` | **205** | 204 — **내 예상이 하나 적었다** |
| `Loading [` 줄 | **205** | 일치, `Chainloader startup complete` |
| `[Error` / `[Fatal` | **1줄 / 0줄** | 알려진 `DirectReload` + Fika 건뿐 |

### 하나 어긋난 이유를 가렸다 — `LootNet.Fika` 가 올라왔다

`Loading [LootNet.Fika 1.0.9]` 가 떴다. 10-G 절에 적은 "Fika 변종이라 올라오지 않는다"가 틀렸고, 그 절에 정정을 달았다. 요점은 **`-Fika` 라는 이름이 아니라 DLL 의 플러그인 속성만 본다**는 것이다.

그래서 **191 + 1(ORBIT) + 6(10-G) + 7(10-H) = 205** 로 맞는다.

### 10-H 7개 전부 로드됐다

`acidphantasm-enablelabyrinth 1.1.0` · `TacticalToaster-UNTARGH 3.2.1` · `RUAFComeHome 1.2.1` · `DrakiaXYZ-DoorRandomizer 1.8.0` · `acidphantasm-letmeout 1.1.0` · `BlackDiv 1.3.1` · `itsRipper-RippersCasino 1.0.1`.

여기서도 이름이 모드명과 달랐다 — UNTAR Go Home! 은 `TacticalToaster-UNTARGH`, Rippers Casino 는 `itsRipper-RippersCasino`, Black Division 집은 `BlackDiv` 다.

### 아직 확인되지 않은 것

레이드 흔적 16건으로 **얕다.** 기준선의 SAIN·STM 줄이 안 나왔고, 10-H 의 본 시험인 **맵별 진입과 프레임**도 로그로는 알 수 없다(위 10-H 절의 네 가지).

## 10-A 의 밀린 숙제 — Easy Slot Filters 재확인 (2026-11-05, 10-M 뒤)

10-A 에서는 **바닐라 무기 + 바닐라 부착물** 한 쌍만 확인했고(MP-43-1C ← 톱질 총열), 모드가 추가한 아이템은 "ESF 가 도는 자리에서 아직 DB 에 없어 조용히 넘어간다"고 보아 **10-L 뒤로 미뤄** 두었다. Epic·ECOT·Russian SF 가 들어갔으니 이제 확인했다.

### 어떻게 쟀나 — 게임 없이, DB 를 직접 읽어서

ESF 는 **아무 로그도 찍지 않는다.** 그래서 결과 DB 를 직접 봐야 한다. 서버를 띄워 `/client/items` 를 불러(`tools/stm-call.mjs` 와 같은 zlib 방식) 해당 무기의 `_props.Slots[칸]._props.filters[].Filter` 를 읽었다. 게임은 필요 없다.

변수를 하나만 두려고 세 쌍을 한 번에 쟀다.

| 쌍 | host | 칸 | part | 뜻 |
| --- | --- | --- | --- | --- |
| 양성 대조 | MP-43-1C (바닐라) | `mod_barrel` | 톱질 총열 (바닐라) | **ESF 가 돌았는지** 확인 |
| A | MP-43-1C — 대조와 **같은 host** | `mod_barrel` | Epic 의 AK-15 7.62x39 소음기 | part 만 모드 것으로 |
| B | AK Tactica Tula 어댑터 7.62x39 (바닐라) | `mod_muzzle` | 같은 Epic 소음기 | 뜻이 통하는 조합 |

### 결과 — **3/3 들어갔다. 모드 아이템도 적용된다**

| 쌍 | 허용 종수 | 적어 둔 part |
| --- | --- | --- |
| 양성 대조 | 4 → 220 | **들어 있다** |
| A | 4 → 220 | **들어 있다** |
| B | 14 → **15** | **들어 있다** |

즉 10-A 때의 걱정은 **현실이 아니었다.** ESF 는 `OnLoadAsync` 에서 돌고, 그 시점에 Epic 의 565개 정의가 **전부 DB 에 올라와 있다**. 모드가 추가한 무기·부착물도 설정에 그대로 적을 수 있다.

### 그런데 한 줄이 칸을 216종으로 열었다

A 쪽 `mod_barrel` 이 4 → **220** 이다. 적은 줄은 하나인데 **216개가 늘었고 전부 `Silencer` 부모**였다(그중 Epic 정의 65개). 설치본의 `Silencer` 부모 아이템 총수가 **정확히 216종**이다.

반면 B 쪽 `mod_muzzle` 은 14 → 15 로 **딱 하나만** 늘었다.

차이는 **칸 이름**뿐이다. 소음기의 본래 칸은 `mod_muzzle` 인데, A 에서는 그것을 `mod_barrel` 에 적었다.

- **본래 칸에 적으면 그 하나만 들어간다** (B).
- **엉뚱한 칸에 적으면 그 종류 전체가 열린다** (A: 소음기 216종).

**왜 그런지는 아직 모른다.** "part 의 부모 종류 전체를 넣는다"로는 B 가 설명되지 않는다(그 규칙이면 B 도 227종이 됐어야 한다). ESF 가 로그를 찍지 않아 내부 판단을 볼 수 없다. 실용적으로는 이것으로 충분하다 — **칸 이름을 part 의 본래 칸에 맞춰 적는다.** 넓게 열고 싶을 때만 일부러 다른 칸에 적는다.

### 덧붙여 — `config.jsonc` 에 주석을 넣으면 서버가 죽는다

확장자가 `.jsonc` 인데 **주석을 받지 않는다.** 설명 주석을 붙여 봤더니 서버가 시작조차 못 했다.

```
'/' is invalid after a value. Expected either ',', '}', or ']'. Path: $ | LineNumber: 1
   at EasySlotFilters.EasySlotFilters.OnLoadAsync(CancellationToken)
```

`System.Text.Json` 기본 설정으로 읽어서다. **모드 하나가 아니라 서버 전체가 멈춘다** — `The server has unexpectedly stopped` 로 끝난다. 이 파일은 **순수 JSON 으로만** 쓴다.

### 남긴 상태

설정은 **원본(10-A 의 한 줄)으로 되돌렸다.** 시험 쌍은 게임에서 쓸 만한 것이 아니다(소음기를 산탄총 총열 칸에 넣는 것이고 칸이 216종으로 열린다). 원본은 `backup/esf-config/config.jsonc.before-modtest` 에도 있다.

이제 실제로 쓸 조합을 적으면 된다. 예를 들어 B 쌍은 뜻이 통하고 부작용도 +1 뿐이다.

```json
{ "Filters": { "5a0abb6e1526d8000a025282": { "dd6b29946db0932ea75e09c4": "mod_muzzle" } } }
```

## 게임 기동 확인 (10-O 의 3025 설치 후, 2026-11-06 04:18~04:44)

로그 시각이 설치보다 뒤다 — `BepInEx/LogOutput.log` 가 **04:44:09**(KST), 기동 머리가 한 번뿐이라 세션을 자를 필요도 없었다.

| 판정 기준 | 예상 | 실측 | 결과 |
| --- | --- | --- | --- |
| 플러그인 수 | 242 → **243** | `243 plugins to load` = `Loading [` **243줄** | **맞음** |
| 서버 모드 | 170 | `ModLoader: 총 170 개` | 맞음 |
| 리워크 플러그인 | 올라옴 | `Loading [Manimal-InterchangeRework 1.0.8]` | 맞음 |
| Map Variants | `interchange` 가 바뀜 | `interchange=Manimal (backport seen)` | **바뀜** |
| 클라이언트 오류 | 알려진 것만 | `[Error` **1줄**, `[Fatal` **0줄** | 맞음 |
| 서버 오류 | 48 + 1 + 1 | `Could not add` 48 / `Unable to add bundle` 1 / `cannot import quest` 1 | 맞음 |

`Chainloader startup complete` 는 2711줄에 있다.

### 백포트가 실제로 떴다

Map Variants 가 묻는 창이 돌았고 사용자가 골랐다.

```
[MV] player chose Interchange - Manimal
[MV] interchange: suspend set = 1 patch(es) from ManimalInterchange: Prefix LoadPresetPatch:Prefix p400
[MV] raid on 'interchange': BACKPORT; 1 of ManimalInterchange's patch(es) live, suspended 0
[REQUEST SUCCESSFUL] /manimal/interchange/capability
Loading Interchange content interchange-expanded-navmesh-test-77ad637b9a245771 with 18 replacement scenes
Interchange native shaders: captured 512 game shader(s) for rebinding.
```

**`lighthouse` 는 아직 `(backport NOT seen yet)`** 이다 — 3022 를 넣지 않았으니 맞는 상태다. 이 줄 하나로 Map Variants 가 **맵마다 따로** 판정한다는 것이 확인됐다.

교체 장면 18개가 셰이더를 게임 것으로 다시 묶었다(`Shopping_Mall_2_MI` 580개, `Shopping_Mall_indoor_MI` 164개, `Shopping_Mall_Shops_MI` 144개 등 합쳐 1,650개쯤). 게임에 같은 셰이더가 없어 원본을 그대로 쓴 것은 **2건**(`Custom/MountainDeferred`, `Particles/Explosion`)이고, 둘 다 안내 줄이다.

### 레이드가 실제로 돌았다

서버 로그는 UTC 이므로 KST 로 바꿔 적는다.

| 항목 | 값 |
| --- | --- |
| 레이드 | `Interchange.Pmc 1793906320` |
| 시작 ~ 끝 | 04:18:38 ~ 04:34:20 (**15분 42초**) |
| 기록된 위치 | **54,951개** (Raid Review) |
| 결과 | 사망 (`EquipmentIsEternal: Death detected, queued restoration of 386 items`) |

종료도 깨끗하다 — TraumaCore 31개·tarkin.ladders 7개 패치가 해제되고 `FixPluginTypesSerialization: Restored original AssemblyNames list` 로 끝났다. 마지막의 `WebSocketSharp ... header part of a frame could not be read` 는 게임을 닫아 서버 연결이 끊긴 자리에서 나는 것이다.

### 어제 쏟아진 예외가 사라졌다

| 문구 | 2026-11-05 | 이번 |
| --- | --- | --- |
| `AICoreAgent` | 4,524줄 | **0** |
| `InvalidCast` | 다수 | **0** |

STM 을 Release 로 다시 깐 뒤 처음 돌린 레이드이고, **예외가 한 줄도 없다.** [2026-11-05-aicoreagent-invalidcast.md](fixes/2026-11-05-aicoreagent-invalidcast.md) 에 "Debug 빌드가 유력하나 미확정" 으로 닫아 두었는데, 이번 실측이 그쪽을 받쳐 준다. 다만 그때 변수를 둘(빌드 구성 + 남아 있던 `pitFireTeam` 폴더 이동) 바꿨으므로 **어느 쪽이 원인이었는지는 여전히 가리지 못했다.**

### 남은 Error 줄 세 개 — 전부 10-O 와 무관하다

클라이언트 1줄:

```
[DirectReload] Patch FAILED to enable: FikaHostReloadMirror — Fika ObservedFirearmController not found
```

Fika 를 쓰지 않으므로 맞는 상태다.

서버 쪽 `[Error]` 51줄은 `Could not add` 48 + `Unable to add bundle` 1 + **`ProfileFixerService` 2** 다. 그 2줄이 처음 눈에 띄어 좇아 봤다.

> SPT에 존재하지않는 상인 `6952ced4bcc1dd1e3c80dfcb` 가 프로필에서 발견되었습니다.

- **새 것이 아니다** — `spt20261101.log` 부터 매일 나온다(11/01 14줄, 11/02 18, 11/03 32, 11/04 30, 11/05 22).
- 그 ID 는 **설치된 모드 어디에도 없다**(`user/mods/` 전체 검색). 프로필 쪽에만 있다 — 상인을 넣는 모드를 썼다가 지운 흔적이다.
- 프로필에 남은 것은 `TradersInfo` 한 줄(`loyaltyLevel 1 / salesSum 0 / standing 0.2`)뿐이고 **그 상인의 물건이나 메시지는 없다.**
- 메시지가 켜라고 하는 `removeModItemsFromProfile` 은 `SPT_Runtime/SPT_Data/configs/core.json` 에 **이미 `true`** 다. 지울 물건이 없어 서버가 할 일이 없는 것이고, 그래서 매 기동 경고만 남는다.

**해롭지 않다.** 손대려면 프로필의 그 한 줄을 지우는 것인데 프로필을 건드리는 일이므로 하지 않았다.

### 한 가지 지켜볼 것 — ORBIT 의 kiba

```
[Warning: ORBIT] F19754: MULTISTEP: unavailable operation=kiba: required scene object missing
```

ORBIT 이 KIBA 매장 문을 여는 다단계 행동을 쓰려다 **장면 물체를 찾지 못했다.** KIBA 는 인터체인지에만 있으므로 이 경고는 이 맵에서만 날 수 있고, 리워크가 그 물체 이름이나 구조를 바꿨을 가능성이 있다.

**바닐라 인터체인지에서도 났는지는 가리지 못했다** — BepInEx 는 기동마다 `LogOutput.log` 를 덮어쓰고 이전 로그가 남아 있지 않다. 다음에 인터체인지를 돌 때(바닐라 쪽을 골라서) 한 번 보면 가려진다. 한 줄뿐이고 레이드는 끝까지 돌았다.

그 밖의 경고 121줄은 HarmonyX 39 · MenuOverhaul 31 · FixPluginTypesSerialization 15 · CloudSix 10 등 다른 모드 것이고, **`Manimal-InterchangeRework` 가 낸 경고·오류는 0줄**이다(안내 26줄만 남겼다).

### 눈으로 봐야 남은 것

- **확장 구역** — 로그는 교체 장면 18개가 떴다고 말하지만, 그 구역이 어떻게 보이는지는 로그로 알 수 없다.

## F12 한글화 — 2차, 사전 없는 모드 58개를 다 메웠다 · 2026-11-06

1차(17개 / 82항목, `336de45`)를 끝낸 뒤 사전이 없는 모드가 **58개 / 2,878항목** 남아 있었다. 그것을 전부 메웠다.

| 항목 | 값 |
| --- | --- |
| 새 사전 | **58개** (사전 122 → **180개**. `lang/` 파일은 `_` 로 시작하는 3개를 더해 183개) |
| 새 항목 | **2,963개** (2,127 → **5,090**) |
| 커버리지 | `node tools/check-lang-coverage.mjs` → **5082/5082**, 경고(`！`) **0건** |
| 설치 | `tools/install-koreanconfig.ps1` → 설치본 사전 125 → **183개** |

### 항목 수가 큰 것부터

| 모드 | 항목 | 어떻게 메웠나 |
| --- | --- | --- |
| `com.borkel.nvgmasks` 야시경 마스크 | 916 | 기기 16종이 같은 키 묶음을 되풀이한다 — **고유 키 110개만 번역**하고 섹션마다 펴서 만들었다 |
| `com.dizzlow.impactcue` 타격 표시 | 615 | **모드가 제 `locales/ko.json` 을 들고 있었다**(아래) — 번역하지 않고 옮겼다 |
| `com.matsix.cloudsix` 구름·하늘 | 132 | 손으로 |
| `com.janky.hollywoodgraphics` 화면 효과 | 87 | 맵 13곳 × 키 4개 = 52개를 규칙으로, 나머지 35개를 손으로 |
| `com.pein.battleambience` 먼 교전 소리 | 88 | 맵 10곳이 같은 키 16개를 되풀이한다 — 규칙으로 |
| `com.matsix.tracesix` 되튄 빛 조명 | 84 | 손으로 |
| `com.matsix.fogsix` 부피 안개 | 68 | 손으로 |
| `com.hysocs.traumacore` 해부학 피해 | 62 | 손으로(사람·봇 같은 이름 키는 섹션별로 따로) |
| `com.janky.hollywoodfx` 총격·폭발 효과 | 60 | 손으로 |
| `com.zgfuedkx.skilldistribution` 숙련도 나눠 주기 | 58 | 기술 47개 배율을 규칙으로 |
| `com.mybutthasarash.deployscreen` 출격 화면 | 56 | 손으로 |
| `com.blackhawk.dynamicmapsextended` 지도 확장 | 54 | 역할 33개 묶기를 규칙으로 |
| `com.hysocs.ragdollkinetics` 시체 물리 | 54 | 죽는 자세 4종이 같은 키 9개를 되풀이한다 — 규칙으로 |

나머지 45개는 35항목 이하이고 손으로 적었다.

### ImpactCue 는 번역할 것이 없었다 — 이미 한글이 있다

`E:\Spt\BepInEx\plugins\ImpactCue-UI-MOD\locales\ko.json` 에 **1,302개 키**가 들어 있고, 키 모양이 `섹션|키|name` / `|description` 이라 우리 사전의 `섹션/키` 와 **1:1 로 맞는다.** 그대로 옮겨 479항목을 채웠다.

남은 136항목은 그 `ko.json`(2.0.3)이 설치본보다 뒤쳐져 비어 있던 것인데, 전부 `99. Internal/Status <요소> X·Y·Scale` 꼴이어서 **요소 이름 20개 + 꼬리 규칙 7개**로 만들었다.

`locales` 폴더를 가진 모드를 모두 훑었다 — **한글이 든 것은 ImpactCue 뿐이다**(AdvancedStashSorting `ch/en/ru`, WishlistExtended `en/pl`, Softwyx-LootInVicinity `en`).

### 사전이 있었는데도 비어 있던 네 모드

사전을 만든 뒤 모드가 항목을 더 넣어 빈 자리가 생긴 것들이다. 같은 꼴이 되풀이되어 규칙으로 메웠다.

| 모드 | 메운 수 | 무엇이 늘었나 |
| --- | --- | --- |
| `com.vultify.nobarrelshadow` | 46 | Epic's AIO·ECOT·Russian SF Pack 이 넣은 조명 23종의 사거리·밝기 |
| `com.slpf.caliberundername` | 2 | 새 탄 구경 2종 |
| `com.manimal.interchange` | 3 | 10-O 로 들어온 모드가 `Rendering` 섹션을 새로 썼다 |
| `com.blackhawk.dynamicmapsextended` | 1 | `Interchange (Manimal)` — 10-O 의 백포트를 지도 그림 목록이 알아봤다 |
| `xyz.stm.teammanager` | 1 | `14 AutoJoinMaxMembers` |

**교훈** — 사전은 한 번 만들면 끝나는 것이 아니다. 모드가 들어올 때마다 **다른 모드의 사전에도 구멍이 난다**(조명 하나가 늘면 No Barrel Shadow 에 두 줄이 생긴다). 단계마다 `check-lang-coverage.mjs` 를 돌려 보는 것이 맞다.

### 이름 없는 섹션

`com.tarkin.huir` 와 `com.rairai.hiddencaches.eft` 는 `[]` 로 이름 없는 섹션을 쓴다. 사전의 `sections` 에 빈 문자열 키로 `일반` 을 적어 두었다. 게임에서 그 머리글이 어떻게 보이는지는 **확인하지 않았다.**

### 아직 원문인 것

- `xyz.drakia.Sense.json` 의 색 항목 58개는 **설명이 없다**. 원문 `.cfg` 에도 설명이 없어 커버리지는 통과한다(이름은 다 한글이다).
- Better BepInEx 창 자체의 글(`_ui.json` 쪽)은 이 묶음에 넣지 않았다 — 설정 항목이 아니라 창 틀의 글이다.

## 10-O — Manimal 맵 백포트 3개 (진행 중) · 2026-11-05

**계획이 이 단계에만 둔 제약 둘을 그대로 지킨다.** ① 표 순서를 바꾸지 않는다 — ICEBREAKER(2866)가 Interchange Expansion(3025)을 요구하므로 맨 뒤다. ② **하나 넣고 그 맵에 들어가 보는 것까지 끝낸 뒤** 다음으로 간다. 그래서 앞 단계들처럼 몰아서 넣지 않고 모드마다 멈춘다(사용자와 확인).

**ICEBREAKER 의 선행 8개가 모두 설치돼 있다** — UnityToolkit 2.0.2 · WTT CommonLib 3.0.6 · WTT Content Backport 2.0.5 · BigBrain 1.5.0 · MoreBotsAPI 2.1.1 · WTT Black Division 1.3.1 · Climbable Ladders 1.0.4 · Manimal CS Gas 2.0.0. 이 단계에서 새로 들어가는 선행은 3025 하나다.

| 모드 | 버전 | 크기 | 받기 | 해제·검사 | 설치 |
| --- | --- | --- | --- | --- | --- |
| 3025 Interchange Expansion | 1.0.8 | 1,509,917,405 | **바이트 일치** | **CLEAN (0/0/0)**, 148개 | **들어갔고 레이드까지 돌았다** (아래) |
| 3022 Lighthouse Rework | 1.0.7 | 2,455,455,438 | 받기 OK (2026-11-06) | 해제 50파일 / 2.4 GB | 설치 전 |
| 2866 ICEBREAKER | 1.1.3 | 1,672,250,789 | **바이트 일치** | - | - |

매니페스트의 링크·버전·용량은 Forge API 응답과 **전부 일치**하는지 따로 대조했다(손으로 적은 링크를 믿지 않는다).

### 3022 만 받기가 막혔다 — 호스트 차이다

3025(1.5 GB)는 **36초**, 2866(1.6 GB)은 **64초**에 받아졌다. 둘 다 GitHub release 자산이다. 그런데 3022 는 받는 곳이 **Cloudflare R2**(`pub-…r2.dev`)이고,

- 1차: 70 MB 에서 `TypeError: terminated` 로 끊김
- 2차(재시도): 4분에 12 MB — **약 3 MB/분.** 남은 2.2 GB 를 이 속도면 12시간

사용자와 **나중으로 미뤘다.** 받은 조각은 `downloads/stage10-O/3022__1.0.7.part` 로 남아 있고(이어받기는 하지 않는다), 다시 받을 때 지워진다.

**다시 받으니 한 번에 끝났다 (2026-11-06 04:37).** `redownload-mod.mjs` 로 시도 1회, **분당 855 MiB**, 바이트 수가 `2455455438/2455455438` 로 정확히 맞았다. 어제 분당 3 MB 로 기어갔던 것은 호스트 쪽 일시적인 문제였고 받는 쪽 설정 문제가 아니었다 — **같은 링크, 같은 도구로 속도가 285배 달랐다.** 느린 호스트를 만나면 다른 시각에 다시 걸어 보는 것이 먼저다.

파일 이름이 `1.0.7` 로만 왔다(`content-disposition` 이 버전만 준다). 다른 두 개와 맞춰 `3022__Manimal-Lighthouse-1.0.7.zip` 으로 바꿨다. 상위 폴더는 `BepInEx/plugins` 와 `SPT_Runtime/user` 뿐이라 3025 와 같은 배치이고 경로를 손댈 것이 없다.

충돌 검사: **3022 는 0건**(설치 대상 50경로). 같은 검사가 찍는 `이미 있는 경로 148` 은 전부 3025 의 것이고, 그것은 이미 설치했으므로 정상이다.

**도구를 하나 만들었다 — `tools/redownload-mod.mjs`.** `download-stage.mjs` 는 건너뛰기가 없어 다시 돌리면 그 단계 **전체**를 다시 받는다(10-O 면 5.25 GB). 그래서 한 모드만 다시 받는 길을 따로 뒀다.

```
node tools/redownload-mod.mjs stage10-O 3022
```

남은 `.part` 를 지우고 네 번까지 다시 걸며, 바이트 수가 매니페스트와 어긋나면 받은 파일을 지우고 실패로 끝낸다.

### 3025 는 들어갔다 (2026-11-05)

| 항목 | 결과 |
| --- | --- |
| 설치 | 148파일 / 1,514.5 MB / 12초 |
| 서버 모드 | 169 → **170** (예상치와 같다) |
| 시작 | `SERVER STARTED in 201s`, `(Added 1 mods: ManimalInterchangeRework (1.0.8))` |
| 기록 | **150줄** = 설치 148 + 서버가 만든 배너 2장(`ManimalInterchange/banners/*.jpg`). 설치 스크립트가 `NEWFILE` 로 알아서 기록에 더했다 |
| 오류 | `Could not add` 48 · `Unable to add bundle` 1 · `cannot import quest` 1 — **기존과 같다.** 새 오류 0 |

**여기서 멈춘다.** 계획이 "하나 넣고 그 맵에 들어가 보는 것까지 끝낸 뒤 다음으로 간다"고 못 박았으므로, **Interchange 에 들어가 보기 전에는 3022·2866 을 넣지 않는다.**

**이 문이 열렸다 (2026-11-06 04:44).** 인터체인지 레이드를 15분 42초 돌았고, 플러그인이 예상대로 243, Map Variants 가 `interchange=Manimal (backport seen)` 으로 바뀌었다. 자세한 것은 아래 **게임 기동 확인 (10-O 의 3025 설치 후, 2026-11-06 04:18~04:44)** 절에 적었다. 남은 것은 확장 구역을 눈으로 보는 것뿐이고 로그로 가릴 수 있는 것은 다 가렸으므로, **3022 설치로 넘어갈 수 있다.**

다음 기동에서 볼 것:

- 플러그인 **242 → 243 예상** (3025 의 클라이언트 DLL 1개).
- **Interchange 에 들어가 확장된 구역이 실제로 바뀌었는지.**
- Map Variants 의 줄이 `interchange=Manimal (backport NOT seen yet)` 에서 바뀌는지 — 이 백포트가 바로 그것이다. `lighthouse` 쪽은 3022 가 들어가야 바뀐다.

### 그다음에 할 일

1. ~~3022 를 다시 받는다~~ — **끝났다 (2026-11-06).** 받기·해제·충돌 검사까지 마쳤다. 다음은 설치다.
3. 마지막에 2866 ICEBREAKER. **10-M 의 DynamicMaps Extended DLL 이 `com.manimal.icebreaker` 를 참조하므로**, 이것이 들어가면 그쪽 지도에 할 일이 생긴다 — 넣은 뒤 지도 화면을 다시 본다.
4. 이 단계 뒤에 **10-B 의 Map Variants 가 찍던 `[MV] no backported location found` 가 사라지는지** 본다. 3025 를 넣은 뒤 `interchange` 쪽은 이미 `(backport seen)` 으로 바뀌었고, `lighthouse=Manimal (backport NOT seen yet)` 만 남아 있다 — 3022 가 들어가면 그쪽도 바뀌어야 한다.

## 10-M — 대형 3개, 하나씩 · 2026-11-05

| 항목 | 결과 |
| --- | --- |
| 받기 | 3개 모두 바이트 수 **정확히 일치** (합 5,754,500,031 = 5.36 GiB). 링크·버전·용량이 Forge API 와 전부 일치하는지도 대조했다 |
| 겹침 검사 | **CLEAN (0/0/0)**, 설치 대상 468개 |
| 기록 | **468줄** = 겹침 검사 468 경로와 정확히 일치. 없는 파일 0건 / 기록에 없는 파일 0건 |
| 오류 | 3회 전부 `Could not add` 48 · `Unable to add bundle` 1 · `cannot import quest` 1 — **기존과 같다.** 새 오류 0 |
| 프로필 | 아이템·키 양쪽 **변화 없음** (이 셋은 결속이 아니다) |

### DynamicMaps Extended 는 확장이다 — 교체가 아니다

계획 §10-M 이 "설치된 Dynamic Maps 의 파일을 덮으려 하면 거기서 멈춘다 — 그때는 확장이 아니라 교체다"로 둔 관문이다. **근거 셋으로 넘었다.**

1. `E:\Spt` 에 이미 있는 경로와 겹치는 것이 **0건**이다.
2. DLL 을 **하나만** 자기 폴더에 넣는다 — `BepInEx/plugins/DynamicMaps-Extended/DynamicMaps.Extended.dll`. 기존 `BepInEx/plugins/mpstark-dynamicmaps/DynamicMaps.dll` 은 손대지 않는다. 기반 모드의 사본을 들고 오지 않았다.
3. GUID 가 **`com.blackhawk.dynamicmapsextended`** 로 기존 **`com.mpstark.dynamicmaps`** 와 다르다 — 조용히 버려지는 쪽이 생기지 않는다.

덧붙여 이 DLL 은 `com.lennoxp90.mapvariants`(설치됨, 10-B)와 **`com.manimal.icebreaker`** 를 참조한다. 뒤의 것은 **10-O** 의 Manimal 맵이다 — 10-O 를 넣으면 이쪽이 더 할 일이 생긴다는 뜻이다.

### 하나씩 넣은 결과

| 순서 | 모드 | 분류 | 서버 모드 | 기록 줄 | 설치 용량 | 로드 확인 |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | DynamicMaps Extended (3097) | 게임 | **167 유지** | 201 | 1743 MB | 서버 로그에 안 찍힌다(정상 — 게임 전용) |
| 2 | Manimal 의 1.0 Labs Office Backport (2937) | 게임+서버 | 167 → **168** | 7 | 97 MB | `(Added 1 mods: ManimalLabsBoiler (2.0.0))` |
| 3 | Arena Map Port (3166) | 게임+서버 | **169** | 260 | 3676 MB | `(Added 1 mods: ArenaMapPort (0.4.0))` |

서버 167 → **169.** DynamicMaps Extended 가 게임 전용이라 수가 그대로인 것이 맞다.

**Labs Office Backport 의 겹침 걱정은 없었다.** 계획이 "설치된 WTT Content Backport 와 같은 계열이라 파일 겹침을 특히 꼼꼼히 본다"고 했는데, 파일이 **7개뿐**이고 겹치는 경로가 0 이다. 같은 계열이라도 자기 폴더(`Manimal-LabsBoiler`)만 쓴다.

### 다음 게임 기동에서 볼 것 — 여기서는 서버 테스트가 아무것도 말해 주지 않는다

**셋 다 맵 자료다. 서버 시작은 통과 근거가 되지 못한다** — 계획이 "레이드 진입이 본 시험"이라고 못 박은 자리다.

- 플러그인 **239 → 242 예상.** 클라이언트 DLL 이 셋이다 — `DynamicMaps.Extended.dll`, `ManimalLabsBoiler.dll`, `maschine-ArenaMapPort.Client.dll`. 242 가 아니면 `Loading [` 목록을 직접 대조한다.
- `[Error` 가 기존 건(SAIN 6 · DirectReload 1 · STM 2)뿐인지.
- **맵에 들어가 보는 것**이 관문이다. ① DynamicMaps Extended — 지도 화면이 기존 Dynamic Maps 를 밀어내지 않고 함께 뜨는지, ② Labs Office Backport — Labs 의 그 구역이 실제로 바뀌었는지, ③ **Arena Map Port — 그 맵에 들어가 보는 것까지가 통과 조건이다**(버전이 0.4.0 이다).
- `[MV] no backported location found` 가 아직 나오는지도 본다 — 10-B 의 Map Variants 는 백포트 맵이 들어오는 **10-O** 까지 할 일이 없었다.

## 게임 기동 확인 (10-L 설치 후, 2026-11-05 19:03~19:05 KST)

| 항목 | 예상 | 실측 | 판정 |
| --- | --- | --- | --- |
| 플러그인 수 | 239 | **239** (`239 plugins to load` = `Loading [` 239줄) | **정확히 맞는다** |
| `Chainloader startup complete` | 있어야 | 1 | 통과 |
| 게임 `[Error` | 기존 건만 | **1줄** — 알려진 DirectReload+Fika 건뿐 | 통과 |
| 게임 `[Fatal` | 0 | 0 | 통과 |
| 서버 `[Error]` | 49(기존) | **51** = `Could not add` 48 + `Unable to add bundle` 1 + **기존 상인 건 2** | 통과 (아래) |
| 플리마켓 생성 | 터지지 않아야 | **통과** (아래) | 통과 |

10-L 의 클라이언트 7개가 모두 올라왔다 — `Manimal-CSGas 2.0.0`, `Content Backport - Prestiges 1.1.0`, `Strange Curios 1.1.0`, `Borkel's Realistic NVGs 3.0.3`, `Deploy Screen Overhaul 1.1.0`, `WTT Halloween Client 2.0.0`, `Skills Extended 3.1.4`.

**Skills Extended 의 예측이 맞았다.** DLL 3개를 싣지만 `Loading [Skills` 는 **1줄**이다 — `.Client.API` 와 `.Common` 은 플러그인 속성이 없는 라이브러리라 0 으로 센 것이 옳았다. 그래서 DLL 9개가 플러그인 7개가 되고 232 + 7 = 239 다.

### 플리마켓은 터지지 않았다 — 10-L 의 가장 큰 관문

아이템을 넣는 모드가 7개였으므로 계획이 "플리마켓 생성 단계에서 터진다"고 경고한 자리다. 사용자 서버 로그에 증거가 남았다.

```
[QuickSell] Built flea price table: 9730 items from 100547 live offers
            (6382 live averages, 1029 weapon bases, 2319 fallbacks) in 49ms.
[ModSource] Detected 5756 items, 938 quests, 99 achievements, 1411 clothing
            and 26047 trader offers from 57 mods.
```

**`ragfair`·`flea` 가 들어간 오류 0건, `exception` 0건** 이고 `/client/handbook/templates` 도 두 번 정상 응답했다. 10만 개 넘는 매물로 가격표가 49 ms 에 만들어졌다.

### 서버 오류가 49 → 51 이 된 이유 — 10-L 과 무관하다

새로 늘어난 2줄은 같은 내용이다.

```
[Error] SPT에 존재하지않는 상인 6952ced4bcc1dd1e3c80dfcb가 프로필에서 발견되었습니다.
```

Nomad 프로필(`6adc2669`)에만 남아 있는 **지워진 상인 참조**다. 로그를 날짜별로 세어 보면 **11-03 에 32회, 11-04 에 30회, 11-05 에 16회** — 10-L 보다 이틀 앞선다. 예전에 상인을 넣는 모드를 뺄 때 그 상인이 보낸 쪽지를 지우지 않아 남은 것이고, 이번 설치가 낸 것이 아니다.

서버 테스트 11회에서 이 줄이 안 보였던 것은 **테스트 서버가 프로필을 불러오지 않기 때문**이다 — 프로필을 읽는 시점에만 찍힌다. 그래서 `install-and-test` 의 출력과 사용자 서버 로그의 오류 수는 **직접 비교되지 않는다.**

### 함정 하나 — 서버 로그는 UTC 다

이 확인 중에 `ModLoader: 총 167 개` 가 **09:57** 로 찍혀 있어 낡은 파일로 의심했다. 실제로는 **09:57 UTC = 18:57 KST** 로 바로 그 세션이었다. `logs/spt/spt<날짜>.log` 는 **UTC** 이고 파일 수정 시각·셸·BepInEx 로그는 **KST** 다. 하루치가 한 파일에 쌓이므로, 지금 세션을 자를 때는 시각이 아니라 **파일 끝에서 거꾸로 올라가 마지막 `ModLoader: 총 N 개` 줄**을 기준점으로 쓴다.

### 버전 표기가 세 군데서 다른 모드

Borkel 의 RNVG 는 **Forge 3.0.4 · 서버 로그 2.2.0 · 플러그인 3.0.3** 이다. Halloween Masks 는 Forge 2.0.0 인데 서버가 `5.0.1`, 플러그인이 `2.0.0` 으로 찍는다. **버전 대조는 Forge 쪽 숫자로 한다** — 로그 숫자로 "다른 판이 깔렸다"고 읽지 않는다.

### 아직 눈으로 봐야 하는 것

로그로는 여기까지다. 상인 줄이 넘치지 않는지(넘치면 설치된 Kaeno-TraderScrolling), Epic·ECOT 의 아이템이 제대로 보이는지, 새 스킬 둘(`SignalsIntelligence`·`Hacking`)이 스킬 화면에 뜨는지, Deploy Screen Overhaul 의 출발 화면, BRNVG 의 야시경, Halloween 가면은 화면을 봐야 안다.

## 10-L — 결속 콘텐츠·대형 11개, 하나씩 · 2026-11-05

| 항목 | 결과 |
| --- | --- |
| 받기 | 11개 모두 바이트 수 **정확히 일치** (합 3,829,817,633 = 3.57 GiB). 받기 로그만 믿지 않고 디스크에서 다시 쟀다 |
| 겹침 검사 | 손본 뒤 **CLEAN (0/0/0)**, 설치 대상 1504개 (아래) |
| 프로필 백업 | `backup/profiles-before-stage10-L/` — 7개 |
| 기록 | **1505줄** = 겹침 검사 1504 + 설치 중 생성된 `StrangeCurios/Data/config.json` 1개. 없는 파일 0건 / 기록에 없는 파일 0건 |
| 오류 | 11회 전부 `Could not add` 48 · `Unable to add bundle` 1 · `cannot import quest` 1 — **기존과 같다.** 새 오류 0 |

### 겹침 검사에서 걸린 두 건 — 둘 다 그대로 두면 설치가 멈췄다

1. **WTT Halloween Masks 의 압축이 `BepinEx`(소문자 n)였다.** Windows 는 대소문자를 가리지 않아 설치 자체는 됐겠지만 기록에 틀린 철자가 남고, 검사기는 설치 경로로 인정하지 않는다. 해제본에서 `BepInEx` 로 고쳤다. 더 중요한 것은 이 모드가 검사에서 **`서버` 로 분류되는데 클라이언트 DLL(`WTTHalloweenClient.dll`)을 싣는다**는 점이다 — "분류로 세면 틀린다"가 또 맞았다.
2. **Manimal's CS Gas 의 압축 뿌리에 `LICENSE` 가 섞여 있었고, `E:\Spt\LICENSE` 가 이미 있다**(SPT 자체 것, 35,148 바이트). 그대로 두면 설치 스크립트가 덮어쓰기로 보고 **2864 에서 중단됐을 것**이다. 설치 경로가 아닌 파일이라 해제본에서 지웠다.

### 하나씩 넣은 결과

| 순서 | 모드 | 분류 | 서버 모드 | 기록 줄 | 로드 확인 |
| --- | --- | --- | --- | --- | --- |
| 1 | Manimal's CS Gas (2864) | 게임+서버 | 157 → **158** | 9 | `(Added 1 mods: CSGas (2.0.0))` |
| 2 | Content Backport - Prestiges (2540) | 게임+서버 | **159** | 63 | `(Added 1 mods: Content Backport - Prestiges (1.1.0))` |
| 3 | Strange Curios (3059) | 게임+서버 | **160** | 18 | `(Added 1 mods: Strange Curios (1.1.0))` |
| 4 | Borkel 의 Realistic NVGs (954) | 게임+서버 | **161** | 102 | `(Added 1 mods: Borkel's Realistic Night Vision Goggles (2.2.0))` |
| 5 | Deploy Screen Overhaul (3047) | 게임 | **161 유지** | 149 | 서버 로그에 안 찍힌다(정상 — 게임 전용) |
| 6 | Tushonka Rare Collectibles (3062) | 서버 | **162** | 28 | `(Added 1 mods: TarkovRareCollectibles (1.2.2))` |
| 7 | WTT Halloween Masks (2401) | 게임+서버 | **163** | 15 | `(Added 1 mods: WTT Halloween (5.0.1))` |
| 8 | Skills Extended (2383) | 게임+서버 | **164** | 95 | `(Added 1 mods: Skills Extended (3.1.4))` |
| 9 | Russian Special Forces Pack (3035) | 서버 | **165** | 135 | `(Added 1 mods: Russian-SF-Pack (2.0.1))` |
| 10 | Epic's All in One (1263) | 서버 | **166** | 632 | `모드 Epic's All In One 버전 5.0.2 ... 불러옴` |
| 11 | ECOT (2195) | 서버 | **167** | 259 | `(Added 1 mods: Eukyre-Consortium (2.0.1))` |

서버 157 → **167.** 11개 중 10개가 서버 모드를 싣고 Deploy Screen Overhaul 만 게임 전용이라, 예상치와 정확히 맞는다.

**모드가 적는 버전이 Forge 의 버전과 다른 것이 둘 있다** — BRNVG 는 Forge 3.0.4 인데 로그에 `2.2.0`, Halloween Masks 는 Forge 2.0.0 인데 `5.0.1` 로 찍힌다. Epic 은 Forge 가 `~4.1.6` 으로 적은 판인데 모드 자신은 `targets SPT: ~4.1.0` 이라 한다. 셋 다 로드에는 문제가 없다 — **버전 대조는 Forge 쪽 숫자로 한다.**

### ECOT ↔ Epic 의 버전 어긋남 — 서버 쪽에서는 드러나지 않았다

받을 때 적어 둔 위험(ECOT 2.0.1 은 2026-09-30, Epic 5.0.2 는 2026-10-16 이라 ECOT 의 의존성 목록이 5.0.1 까지만 가리킨다)은 **서버 시작에서는 나타나지 않았다.** 둘이 나란히 올라오고(`com.epicrangetime.aio` 5.0.2 · `com.eukyre.ecot` 2.0.1), ECOTI 가 센 슬롯 내역에도 양쪽 아이템이 다 들어 있다 — `EukyreECOT 10, WTT-ContentBackport 10, vanilla 46 … fits 217 slots`. 다만 **아이템 하나하나가 맞는지는 게임에서 봐야** 한다.

### 프로필 — Skills Extended 가 처음으로 실제로 심었다

| 변화 | 값 |
| --- | --- |
| `characters.pmc.Skills.Common` | 프로필 **7개 전부** +2 (45 → 47, 또는 59 → 61) |
| 더해진 것 | `SignalsIntelligence`, `Hacking` — 둘 다 진행도 **0** |
| 기존 스킬 진행도 | **전부 그대로.** 사라진 스킬 0 |
| 아이템·루블 | 변화 없음. 새 id 충돌 없음 |

계획 §10-L 이 "Skills Extended 는 스킬 트리를 바꾼다 — 기존 스킬 수치가 남는지 프로필 비교로 본다"고 둔 관문을 **넘었다.**

**10-K 와 달리 서버 테스트만으로 프로필이 바뀌었다.** 10-K 의 6개는 게임이 프로필을 불러와야 심었는데, Skills Extended 는 서버가 프로필을 읽는 시점에 스킬 칸을 넣는다. 그러므로 **되돌릴 기준은 `backup/profiles-before-stage10-L/`** 이고, 이 뒤로는 그 백업이 유일한 "스킬 2개 없던" 상태다.

### 다음 게임 기동에서 볼 것

- 플러그인 **232 → 239 예상.** 클라이언트 DLL 은 9개인데 Skills Extended 가 셋(`.Client.Skills`·`.Client.API`·`.Common`)을 싣고 그중 둘은 라이브러리로 본다. 237~240 밖이면 `Loading [` 목록을 직접 대조한다.
- `[Error` 가 기존 건(SAIN 6 · DirectReload 1 · STM 2)뿐인지.
- 눈으로 볼 것: 상인 줄이 넘치지 않는지(넘치면 설치된 Kaeno-TraderScrolling), **플리마켓이 터지지 않는지**(아이템을 넣는 것이 7개다), Epic·ECOT 의 아이템이 제대로 보이는지, 새 스킬 둘이 스킬 화면에 뜨는지, Deploy Screen Overhaul 의 출발 화면, BRNVG 의 야시경, Halloween 가면.

## 게임 기동 확인 (10-K 설치 + F12 프레임 수정 후, 2026-11-05 17:19~17:23)

| 항목 | 예상 | 실측 | 판정 |
| --- | --- | --- | --- |
| 플러그인 수 | 232 | **232** (`232 plugins to load` = `Loading [` 232줄) | **정확히 맞는다** |
| `Chainloader startup complete` | 있어야 | 1 | 통과 |
| `[Error` | 기존 건만 | **1줄** — 알려진 DirectReload+Fika 건뿐 | 통과 |
| `[Fatal` | 0 | 0 | 통과 |
| 내 모드의 `[Error`·`[Warning` | 0 | **0** | 통과 |

10-K 가 넣은 클라이언트 4개가 모두 올라왔다 — `Inventory Organizing Features 1.10.1`(패치 12/12), `Item Preview QoL 1.2.0`, `TraumaCore 1.7.0`, `QuestBackport Client 1.0.9`. 플러그인 속성이 없는 것도, GUID 가 겹쳐 버려진 것도 없다.

이 기동은 **메뉴까지만** 돈 것이다. 레이드를 깊게 돌면 기존 오류(SAIN 6 · STM 2 등)가 더 나온다 — 1줄인 것은 좋아진 것이 아니라 아직 안 나온 것이다.

### F12 프레임 — 321회가 2회로 줄었다

| 항목 | 고치기 전 | 지금 |
| --- | --- | --- |
| `한글 적용(새 메뉴)` 호출 | **321회** | **2회** |
| 훑은 항목 수 | **7,594** (실제 3,350) | **3,419 / 3,544** |
| 번역된 항목 | 3,894 | 1,707 → 1,824 |
| 걸린 자리 | CollectSettings + **BuildingList(그리기)** | **CollectSettings 하나** |

BBE 가 색인을 두 번 돌렸고(`Indexed 3420 settings from 232 mods`, `Indexed 3545 ...`) 내 후처리도 **정확히 그때마다 한 번씩** 돌았다. 매 프레임 도는 경로가 사라졌다.

**다만 `Done` 집합이 한 일은 생각보다 적다.** 두 번째 호출의 항목 수가 3,544 로 그대로인 것은 BBE 가 색인할 때마다 `SettingEntryBase` 를 **새 객체로 다시 만들기** 때문이다 — 참조로 거르는 집합은 색인을 넘나들며 맞지 않는다(사전을 다시 읽은 적은 0회이므로 집합이 비워진 것은 아니다). 효과를 낸 것은 **그리는 메서드를 떼어 낸 것**이고, `Done` 은 한 호출 안에서 필드마다 중복으로 들어오는 것을 막는다.

### 프로필 — 10-K 가 심은 것은 아직 없다

| 변화 | 값 | 무엇인가 |
| --- | --- | --- |
| `spt.mods` | 161 → **164** | **10-K 의 서버 모드 3개**가 적혔다 — PityLoot 1.0.0 · Lacyway's PvE Tweaks 1.3.2 · QuestBackport 1.0.9. 맞는 변화다 |
| `characters.pmc.Quests` | 115 → **107** | **일일 퀘스트 교체다. 10-K 와 무관하다** (아래) |
| `unlockedProductionRecipe` | `[]` → `["5d8a1a7d7a3dfe597c2e459e"]` | 바닐라 제작법 1건이 해금됐다. 어느 모드의 것인지 **가리지 못했다** |

아이템 수준(`tools/stm-profile-diff.mjs`)은 프로필 7개 **전부 변화 없음** 이다 — 아이템 수·루블 그대로, 새 id 충돌 없음.

**퀘스트 8개가 사라진 것으로 보였으나 일일 퀘스트였다.** 처음에는 데이터 손실로 읽혔고 그중 하나는 `status=4`(완료)여서 더 그랬다. 추적한 순서는 이렇다.

1. 사라진 qid 7개(중복 1건이 있어 키 수로는 8)가 모두 `6aeaeb22…` 로 시작하는 **한 뭉치**였다.
2. 그 id 들이 `user/mods` 와 `SPT_Data/database` 어디에도 없다 — **디스크에 정의가 없는 런타임 생성 퀘스트**다.
3. `characters.pmc.RepeatableQuests` 를 보니 **7개 전부 거기 있었다.** `Daily` 와 `Daily_Savage` 의 `endTime` 이 1793866914 → 1793953429 으로 **하루(86,515초) 밀렸다.**

즉 일일이 교체되면서 묵은 항목이 `Quests` 에서 빠진 것이다. 활성 반복 퀘스트 수는 8 → 8 로 그대로다.

**적어 둘 것 — 프로필 퀘스트 수가 줄었다고 손실이 아니다.** `RepeatableQuests` 의 `endTime` 을 먼저 본다. 거기 있던 id 이고 시각이 밀렸으면 정상 교체다. 이 관문은 이런 거짓 경보를 낸다.

그러므로 **10-K 의 6개가 프로필에 심은 것은 아직 없다.** Quest Backport 의 퀘스트는 상인 목록을 열어야 들어갈 것으로 보이고, `backup/profiles-before-stage10-K/` 는 여전히 되돌릴 기준으로 쓸 수 있다.

### 아직 눈으로 봐야 하는 것

**F12 프레임 — 통과 (2026-11-05, 사용자 확인).** 열어도 프레임이 떨어지지 않는다. 호출 횟수(321 → 2)와 체감이 함께 맞았으므로 이 건은 **닫는다**.

남은 것은 화면을 봐야 안다 — IOF 의 인벤토리 정리, Item Preview QoL 의 미리보기, TraumaCore 의 치료 화면, Quest Backport 의 새 퀘스트가 상인 목록에 뜨는지.

## 10-K — 결속 소형 6개, 하나씩 · 2026-11-05

프로필에 자기 자료를 넣는 것들이라 **프로필 백업 → 하나 설치 → 서버 테스트**를 6번 되풀이했다.

| 항목 | 결과 |
| --- | --- |
| 의존성 | Quest Backport 가 요구하는 WTT CommonLib **3.0.6** · WTT Content Backport **2.0.5** 가 이미 설치돼 있고 **Forge 가 요구하는 버전과 정확히 같다** |
| 받기 | 6개 모두 바이트 수 **정확히 일치** (합 921,212 바이트) |
| 겹침 검사 | **CLEAN (0/0/0)**, 설치 대상 307개 |
| 프로필 백업 | `backup/profiles-before-stage10-K/` — 7개 / 7,361,268 바이트 |
| 기록 | 307줄 = 겹침 검사의 307 경로와 **정확히 일치**. 생성 파일 0건 |
| 오류 | 6회 전부 `Could not add` 48 · `Unable to add bundle` 1 — **기준선과 같다.** 새 오류 0 |

하나씩 넣은 결과

| 순서 | 모드 | 분류 | 서버 모드 | 기록 줄 | 로드 확인 |
| --- | --- | --- | --- | --- | --- |
| 1 | IOF: Inventory Organizing Feature (2960) | 게임 | **154 유지** | 2 | 서버 로그에 안 찍힌다(정상 — 클라이언트 전용) |
| 2 | Item Preview QoL + screenshots (2206) | 게임 | **154 유지** | 1 | 같음 |
| 3 | PityLoot (3077) | 서버 | 154 → **155** | 6 | `(Added 1 mods: PityLoot (1.0.0))`, `54 quest key mappings, 25 gunsmith`, `Loot patches enabled` |
| 4 | TraumaCore (2893) | 게임 | **155 유지** | 2 | 서버 로그에 안 찍힌다 |
| 5 | Lacy 의 PvE Tweaks (2395) | 서버 | 155 → **156** | 3 | `(Added 1 mods: Lacyway's PvE Tweaks (1.3.2))`, `Successfully loaded!` |
| 6 | Quest Backport (3026) | 게임+서버 | 156 → **157** | 293 | `(Added 1 mods: QuestBackport (1.0.9))` |

서버 154 → **157**. 게임 전용 3개는 수가 그대로인 것이 맞다.

### Lacy 의 PvE Tweaks — 끌 것이 없었다

계획은 "설치된 SVM 과 겹치는 항목을 끈 상태로 시작한다" 였는데, `config/config.jsonc` 의 **기본값이 열 개 전부 `false`** 였다. 손댈 것이 없다. 쓰려면 하나씩 켜면서 SVM 쪽과 대조해야 한다 — 특히 `questTweaks`·`removeMapLimitations`·`addRecipes`.

함께 드러난 것: 이 모드는 **같은 작성자의 MergeConsumablesServer(이미 설치됨)와 나란히 찍힌다.** 둘은 별개 모드이고 서로를 밀어내지 않았다.

### 프로필은 아직 바뀌지 않았다 — 그것이 정상이다

설치 뒤 백업과 대조했다.

- 아이템 수준(`tools/stm-profile-diff.mjs`): 프로필 7개 **전부 변화 없음.** 아이템 수·루블 모두 그대로이고, 새로 생긴 id 충돌도 없다.
- 키·개수 수준(depth 4): **변화 없음.**

**서버를 켜는 것만으로는 프로필에 심지 않는다**는 뜻이다. Quest Backport 가 퀘스트를 심는 것은 게임이 프로필을 불러올 때다. 그러므로 **이 백업이 여전히 유효한 "전" 상태**이고, 진짜 비교는 **게임을 한 번 돌린 뒤**에 같은 두 명령으로 다시 해야 한다.

되돌릴 때는 모드 기록으로 파일을 지우고 `backup/profiles-before-stage10-K/` 를 덮어 쓴다. Quest Backport 는 이 복원이 **필수**다.

### 다음 게임 기동에서 볼 것

- 플러그인 수 **228 → 232**. 10-K 가 넣은 클라이언트 DLL 은 4개다 — `flir.iof.dll`, `JBOBYH_ItemPreviewQoL.dll`, `TraumaCore.dll`, `QuestBackport.Client.dll`. 232 보다 적으면 그중 하나가 플러그인 속성이 없거나 GUID 가 겹친 것이다.
- `[Error` 가 기존 건(SAIN 6 · DirectReload 1 · STM 2)뿐인지.
- **프로필 비교를 다시 한다** — 위 두 명령. 새로 생긴 키가 이 6개의 것인지 본다.
- 눈으로 볼 것: IOF 의 인벤토리 정리, Item Preview QoL 의 아이템 미리보기, TraumaCore 의 치료 화면, Quest Backport 의 새 퀘스트가 상인 목록에 뜨는지.

## 10-J — 도구·성능 7개 (하나씩) · 2026-11-05

로더·설정 화면·오버레이를 건드려 다른 모드 전체에 영향이 가므로 하나씩 넣었다.

| 순서 | 모드 | 파일 | 서버 모드 | 시작 |
| --- | --- | --- | --- | --- |
| 1 | Raid Review (1479) | 13 | 152 → **153** | 147초 |
| 2 | Combat Log (3072) | 1 | **153 유지** | 141초 |
| 3 | ASBP (2931) | 2 | **153 유지** | 144초 |
| 4 | Better BepInEx (3207) | 1 | **153 유지** | 140초 |
| 5 | Performance Scope (3076) | 1 | - | `-SkipTest` |
| 6 | DERP (2200) | 1 | - | `-SkipTest` |
| 7 | Knight Mask Fix (3109) | 3 | → **154** | 141초 |

- 받기: 7개 모두 바이트 수 정확히 일치(7.68 MB). 겹침 검사 CLEAN(22개).
- 5회 테스트 모두 `Could not add` 48 · `Unable to add bundle` 1 · `err.txt` 0바이트로 기준선과 같다.

### Raid Review 는 자체 포트를 연다 — 6969 안이 아니다

```
[RAID-REVIEW] Config loaded. WS port: 7828, HTTP port: 7829
[RAID-REVIEW] WebSocket Server listening on 'ws://127.0.0.1:7828'.
[RAID-REVIEW] HTTP Web Server running at 'http://127.0.0.1:7829'.
```

지금까지 웹 화면을 붙인 모드들(`/orbit`·`/questcodex`·`/impactcue`·`/LotsofLoot`·`/SAIN`·`/coti-assets/`·`/TheBlacklist/`·`/modsource`·`/nameplate`)은 전부 **SPT 서버 포트 6969 안**이었는데 이것만 다르다. 바인딩에 성공했으므로 지금은 충돌이 없다. 자작 WebConfigBridge 도 별도 포트를 열지 않는다(6969 안).

### Raid Review 는 자기 파일을 옮긴다 — 기록을 다시 썼다

첫 기동 뒤 검산에서 **기록된 4개가 디스크에 없었다.**

```
없음: SPT_Runtime\user\mods\RaidReview\Microsoft.Data.Sqlite.dll
없음: ...\SQLitePCLRaw.batteries_v2.dll / .core.dll / .provider.e_sqlite3.dll
```

찾아보니 모드가 **`lib\sqlite\` 로 옮겼다**(로그의 `SQLite isolated runtime`). 옮기면서 `lib\sqlite\legacy\upgrade-<해시>\` 에 작은 파일 4개도 남겼다. SPT 자체 SQLite 와 섞이지 않게 자기 런타임을 떼어 놓는 것이다.

그래서 **설치본의 현재 상태에서 그 모드가 소유한 경로만 다시 읽어 기록을 새로 썼다**(18줄 → 22줄). 이제 기록만으로 지우면 깨끗이 지워진다.

**새 범주다** — 지금까지는 "설치 중 생성되는 파일"(`NEWFILE`·`OTHERMOD`)만 챙기면 됐는데, 이 모드는 **설치된 파일 자체를 옮긴다.** 앞으로 기록 검산에서 "없음"이 뜨면 삭제가 아니라 **이동**을 먼저 의심한다.

`data\logs\` 에는 서버를 켤 때마다 로그가 하나씩 쌓인다(지금 5개). 기록에 든 것은 이 시점의 것들이고, 제거할 때는 폴더째 비우는 편이 낫다.

### 오류 수 세기가 한 번 속았다

ASBP 테스트만 `error` 가 4줄이고 나머지는 3줄이었다. 열어 보니 네 번째가 **SVM 의 농담 문구**였다.

```
[SVM] Initialization complete. ERROR: THE USER IS TOO HANDSOME TO PLAY THIS ...
```

SVM 은 기동마다 문구를 바꾸고, 그중에 `ERROR` 라는 낱말이 든 것이 있다([PROCEDURE.md](PROCEDURE.md) 의 "실행마다 바뀌는 농담 문구"). **문구 수가 하나 늘었다고 오류가 는 것이 아니다** — 늘면 그 줄을 열어 본다.

### 게임 기동 확인 (2026-11-05) — 228 이고 줄지도 않았다. 그러나 문제가 하나 있다

| 항목 | 값 | 판정 |
| --- | --- | --- |
| `plugins to load` | **228** | 222 + 6, 예상대로 |
| `Loading [` 줄 | **228** | 일치 |
| `[Error` / `[Fatal` | **1줄 / 0줄** | 알려진 `DirectReload` + Fika 건뿐 |

**Better BepInEx 가 다른 플러그인을 밀어내지는 않았다** — 수가 줄지 않았다. 6개 모두 로드됐다(`Raid Review 1.10.0` · `maschine-CombatLog 1.0.0` · `acidphantasm-bepinexconfigurationmanager 1.0.1` · `BetterBepInEx 1.0.3` · `PerformanceScope 0.2.0` · `Shibatsu-DynamicExternalResolution 1.1.2`).

**대신 패치를 벗겨냈다.** 로그가 그대로 적고 있다.

```
[BetterBepInEx] Took SptKoreanConfig.UiStringPatch.Transpiler (spt.koreanconfig)
                off the stock window's OnGUI; this menu replaces that window.
                Restart with the new menu off to get it back.
[BetterBepInEx] Took SptKoreanConfig.UiStringPatch.Transpiler ... off the stock window's SettingsWindow; ...
[BetterBepInEx] Took ABCM.Patches.OnGuiPatch.Prefix / .Postfix (OnGuiPatch) off ...
[BetterBepInEx] Took ABCM.Patches.SettingsWindowExceptionPatch.Finalizer ... off ...
```

즉 **F12 창을 통째로 자기 메뉴로 갈아끼우면서, 그 창에 붙어 있던 패치를 떼어 냈다.**

| 떨어져 나간 것 | 무엇이었나 |
| --- | --- |
| `SptKoreanConfig.UiStringPatch` ×2 | **이 저장소에서 만든 F12 설정 창 한글화.** 이 모드의 기능이 그것뿐이므로 사실상 무력화됐다 |
| `ABCM.Patches.*` ×3 | **ASBP(2931)** — 같은 10-J 에서 바로 앞에 넣은 모드다. 패널을 키우는 일이 안 먹는다 |

플러그인 수로는 전혀 드러나지 않는다(셋 다 여전히 228 에 들어 있다). **로드됐다는 것과 일하고 있다는 것은 다르다.**

### 그래서 정해야 한다 — 새 메뉴냐, 한글 F12 냐

Better BepInEx 의 설정에 스위치가 있다.

```
[General]
Use the new menu = true
```

`false` 로 두고 다시 켜면 기존 창과 함께 한글화·ASBP 가 돌아온다(로그가 그렇게 안내한다).

| 고르는 길 | 얻는 것 | 잃는 것 |
| --- | --- | --- |
| **그대로 둔다** | Better BepInEx 의 새 메뉴 — 검색·즐겨찾기·테마, 설정 3,476개를 228개 모드에서 색인. 글자 크기·메뉴 배율도 자체로 있다 | **F12 한글화**, ASBP |
| **`Use the new menu = false`** | 한글 F12 + ASBP | 새 메뉴(설치는 남지만 하는 일이 없다) |
| **3207 을 뺀다** | 한글 F12 + ASBP, 설치도 깔끔 | 새 메뉴 |

**참고 — ASBP 와 Better BepInEx 는 역할이 겹친다.** ASBP 는 기존 CM 패널을 키우는 모드이고, Better BepInEx 의 새 메뉴에는 `Menu scale`·`Text size`·`Scale with screen resolution` 이 들어 있다. 새 메뉴를 쓰기로 하면 ASBP 는 필요 없다.

### 고른 길 — 넷째 길: 한글화를 새 메뉴로 끌고 왔다 (2026-11-05, 사용자 지시)

위 표의 세 길 가운데 어느 것도 고르지 않았다. 사용자 지시는 **"한글화를 다시 모드 화면으로 끌고오는 작업을 먼저"** 였고, 그렇게 했다. 새 메뉴와 한글 둘 다 가진다.

`src/SptKoreanConfig/BetterBepInExPatch.cs` 를 새로 만들어, 그 모드가 **설정을 모으는 지점**에 같은 번역을 다시 건다. 그 모드의 내부 이름에 기대지 않으려고 두 단계로 한다 — ① 이름이 그럴듯한 메서드에 후처리를 걸고, ② 후처리에서 그 타입의 필드를 훑어 `SettingEntryBase` 목록을 찾는다. 못 찾으면 후보 필드 이름을 로그에 적는다(조용히 실패하지 않는다).

실측으로 걸린 자리는 둘이다.

```
새 메뉴 후처리 걸었다: BetterBepInEx.StockManager.CollectSettings
새 메뉴 후처리 걸었다: BetterBepInEx.MenuWindow.BuildingList   ← 뒤에 뺐다
```

**결과: 한글로 나온다** (사용자 확인). 3,475개 중 1,824개가 번역됐다.

고치는 중에 Harmony 가 두 건을 거절했다 — `MenuWindow.BuildSidebar`, `SettingsModel.RefreshIfDirty` 가 `IL Compile Error` 였다. 원인은 **void 메서드에 `__result` 를 요구한 것**이고, 반환형을 보고 후처리를 갈라 고쳤다(`1ff99da`).

### 그리고 프레임이 떨어졌다 — 원인 셋 (2026-11-05, `446e22d`)

사용자 보고: **"한글로 나오지만 F12 열면 프레임이 뚝 떨어짐."** 로그를 세어 원인이 바로 나왔다.

| 실측 | 값 |
| --- | --- |
| `한글 적용(새 메뉴)` 호출 | **321회** (한 기동에서) |
| 그때 훑은 항목 수 | **7,594** (실제 설정은 3,350개) |
| `한글 적용(CM)` 호출 | 0 (새 메뉴가 기존 창을 대신하므로 정상) |

원인이 셋이고 전부 걷어냈다.

1. **`MenuWindow.BuildingList` 는 그리는 메서드였다.** 이름만 보고 "목록을 만드는" 메서드로 읽었는데, 메뉴가 열려 있는 동안 **매 프레임** 돈다. 이제 설정을 모으는 지점(`CollectSettings`, `BuildSettingList`)에만 건다.
2. **같은 항목이 여러 필드에 걸쳐 중복으로 모였다** — 3,350 → 7,594. `HashSet<SettingEntryBase>` 로 참조당 한 번만 담고 재귀는 depth 2 로 끊는다.
3. **이미 번역한 항목을 다시 훑었다.** `Done` 집합으로 걸러 내고, 새로 할 일이 없으면 **로그도 남기지 않고** 조용히 돌아간다. 사전을 다시 읽거나 한글 표시를 끄면 그 집합을 비운다.

적어 둘 것: **이름으로 Build* 메서드에 패치를 걸지 않는다.** `Build` 가 붙었어도 그리는 쪽일 수 있다. 횟수를 세어 확인한다 — 한 기동에서 세 자리면 매 프레임이다.

설치본의 DLL 시각 `2026-11-05 16:35:24` = 수정 빌드분.

### 다음 기동에서 볼 것 (한글화 수정분)

- `한글 적용(새 메뉴)` 가 **321회가 아니라 몇 회**
- 항목 수가 **7,594 가 아니라 3,4xx 대**
- 내 패치에서 나온 `[Error` 0줄
- **F12 를 열어도 프레임이 떨어지지 않는지**

### 남은 것

- Better BepInEx **자기 설정**은 아직 원문이다 — `com.mybutthasarash.betterbepinex.json` 사전이 없다.
- 새 메뉴의 창 자체 글귀(사이드바·버튼)도 원문이다 — `_ui.json` 에 더해야 한다.
- **ASBP 와 Better BepInEx 의 겹침은 그대로 남아 있다.** ASBP 의 패치는 새 메뉴가 떼어 냈다. 새 메뉴를 쓰기로 했으니 ASBP 는 하는 일이 없다 — 뺄지는 사용자 결정.

### 다음 게임 기동에서 볼 것 — 228, 그리고 Better BepInEx

7개가 넣은 플러그인 DLL 6개가 전부 플러그인이다(Knight Mask Fix 는 서버 전용). **222 + 6 = 228.**

**Better BepInEx(3207)는 BepInEx 자체를 건드린다.** 그래서 이번 기동의 판정은 "늘었나"가 아니라 **"줄지 않았나"** 다. 228 보다 적으면 다른 플러그인이 밀려난 것이니 3207 을 먼저 되돌린다(기록 1줄).

ASBP 는 기존 ConfigurationManager 패널을 키우는 모드인데, 이 설치본에는 CM 제공자가 둘(SPT 자체, ICM 내장)이고 그중 하나가 조용히 버려진다 — **ASBP 는 살아남은 쪽에 붙는다.** F12 패널 크기가 안 바뀌면 그 때문일 수 있다.

## 10-I — 그래픽·음향 16개 (둘로 쪼갬) · 2026-11-05

계획은 "16개를 하나씩" 이었으나, 이 단계는 **서버 테스트로 아무것도 드러나지 않아** 하나씩 가면 게임 기동을 16번 해야 한다. 사용자와 정해 둘로 쪼갰다.

| 묶음 | 내용 | 방식 |
| --- | --- | --- |
| **10-I1** (10개, 465.0 MB) | HollywoodFX · Better Night Skies · Blackout · Shadow Flicker Fix · Headshot Darkness · Ragdoll Kinetics · Manimal Watch Animations · BushWhacker · Battle Ambience · Music Extender | 서로 건드리는 곳이 달라 한 묶음 + 테스트 한 번 |
| **10-I2** (6개, 24.5 MB) | CloudSix · FogSix · ReflectSix · AOSix · POMSix · TraceSix | 같은 제작자 묶음이라 **하나씩**, 기록도 따로 |

| 항목 | 결과 |
| --- | --- |
| 받기 | 16개 모두 바이트 수 **정확히 일치** |
| 겹침 검사 | I1 CLEAN(81개). I2 는 처음 **ATTENTION(1)** → 아래 참조, 빼낸 뒤 CLEAN(15개) |
| 설치 | I1 10개(81파일 + 빈 폴더 1), I2 6개(15파일) |
| 서버 모드 | 150 → **151**(Blackout, 시작 144초) → **152**(FogSix) |
| 오류 | I1 테스트에서 새 오류 0 |

### CloudSix 가 선택 Fika DLL 을 담아 왔다

```
[3] 설치 경로가 아닌 파일: 1
    2567__CloudSix_4.0.2 -> FikaSync (OPTIONAL)/BepInEx/plugins/CloudSix/CloudSix-FikaSync.dll
```

폴더 이름에 **`(OPTIONAL)`** 이라고 제작자가 적어 뒀고 이 설치본은 Fika 를 쓰지 않으므로 `extracted/stage10-I2/_excluded/` 로 빼냈다. 10-D 의 `Name Plate.exe` 와 같은 처리다.

### 10-I2 는 서버 테스트를 못 돌렸다

FogSix 의 서버 쪽(`user/mods/DynamicFog/`)이 들어갔으므로 수는 **152** 가 되어야 하는데, **서버 테스트로 확인하지 못했다.** `install-and-test.ps1` 은 설치와 테스트가 한 몸이라 "테스트만" 돌릴 수 없고(이미 설치된 것을 다시 넣으려 하면 설계대로 거부한다), 서버를 따로 띄우면 **프로세스 종료가 권한 분류기에 막혀 끌 수가 없다.**

대신 확인한 것 — `user/mods` 폴더 수가 **152** 다(151 + DynamicFog). `ModLoader: 총 152 개` 는 **다음 서버 기동에서** 확인한다.

### 용량 — 받는 크기와 설치 크기가 많이 다르다

**HollywoodFX 는 내려받기 258 MB 인데 설치하면 973 MB** 다. Music Extender 도 121 MB → 122 MB, Battle Ambience 는 11.5 MB → 82.9 MB 다. [PLAN-STAGE10.md](PLAN-STAGE10.md) 의 분량(14.8 GB)은 **내려받는 크기 기준**이므로 실제 디스크 사용은 그보다 크다. `E:\` 여유가 1 TB 라 문제는 없지만, 남은 단계(특히 10-M·10-O 의 GB 단위 맵)에서는 **압축 해제 뒤 크기로 다시 봐야 한다.**

### 다음 게임 기동에서 볼 것 — 222

16개가 넣은 플러그인 DLL 은 19개인데 **플러그인은 17개**다.

- **HollywoodFX 혼자 플러그인을 둘 등록한다** — `HollywoodFX.dll` + `HollywoodGraphics.dll`.
- 라이브러리 2개는 0 으로 센다 — `Blackout\BlackoutFika.dll`(플러그인 속성 없음)과 `TraceSix\native\TraceSixRT.dll`(네이티브).

`BlackoutFika.dll` 은 10-G 의 `LootNetFika.dll` 과 이름이 닮았지만 **속성이 없어 안 올라온다.** 이름이 아니라 속성으로 센다는 정정이 여기서 다시 확인됐다.

그래서 **205 + 17 = 222** 다.

### 게임 기동 확인 (2026-11-05 14:26) — 222 로 맞았다

| 항목 | 값 | 예상 |
| --- | --- | --- |
| `plugins to load` | **222** | **222** — 맞다 |
| `Loading [` 줄 | **222** | 일치, `Chainloader startup complete` |
| `[Error` / `[Fatal` | **1줄 / 0줄** | 알려진 `DirectReload` + Fika 건뿐 |

**17개 전부 로드됐다.** 예상의 핵심이던 HollywoodFX 의 두 플러그인도 확인된다 — `Janky-HollywoodFX 3.0.0` 과 `Janky-HollywoodGraphics 3.0.0`.

나머지 — `CloudSix 4.0.2` · `FogSix 1.0.1` · `ReflectSix 1.0.1` · `AOSix 2.0.1` · `POMSix 1.0.2` · `TraceSix 1.0.1` · `Better Night Skies 2.0.0` · `Blackout 4.1.1` · `Shadow Flicker Fix 1.5.3` · `HeadshotDarkness 1.2.5` · `Ragdoll Kinetics 1.2.0` · `Manimal-WatchAnims 2.0.0` · `BushWhackerStandalone 1.7.0` · `SPTBattleAmbience 2.4.0` · `MusicExtender 1.2.0`.

**이름으로 훑다가 또 두 번 걸렸다** — `Better Night Skies` 는 띄어쓰기가 있어 `nightsk` 로 안 잡혔고, `headshot` 으로 훑자 **8-A 의 Headshot Damage Redirection** 이 10-I 것인 양 섞여 들어왔다. 짧은 조각으로 훑으면 양쪽으로 틀린다 — **수가 맞는지로 먼저 보고, 이름 대조는 걸린 줄을 열어 확인한다.**

### 아직 확인되지 않은 것

레이드 흔적 16건으로 얕다. 그리고 이 단계는 **로그로 거의 검증되지 않는다.** 눈으로 봐야 한다.

- 플러그인 수 222 와 새 오류 유무
- **설치된 Amands' Sense 와 화면이 겹치는지** — 이 단계 내내 보기로 한 것이다
- **프레임** — TraceSix(레이트레이싱·복셀 GI)·POMSix·CloudSix 는 무겁다. 돌아가는 것과 쓸 수 있는 것은 다르다
- Six 계열은 **하나씩 기록이 갈려 있어** 무거우면 그 모드만 빼면 된다

## 10-H — 맵·레이드 소형 7개 (하나씩) · 2026-11-05

맵을 건드리는 것은 터질 때 레이드 진입이 막히므로, 작아도 하나씩 넣고 그때마다 서버 테스트를 돌렸다. **7회 전부 정상 시작**이다.

| 순서 | 모드 | 파일 | 서버 모드 | 시작 | `Added` |
| --- | --- | --- | --- | --- | --- |
| 1 | Enable Labyrinth (2416) | 7 | 145 → **146** | 141초 | 1 |
| 2 | UNTAR Go Home! (2342) | 24 | → **147** | 150초 | 1 |
| 3 | RUAF Come Home! (2427) | 38 | → **148** | 162초 | 1 |
| 4 | Door Randomizer (820) | 1 | **148 유지** | 141초 | - (게임 전용) |
| 5 | LET ME OUT (2240) | 2 | **148 유지** | 141초 | - (게임 전용) |
| 6 | WTT Black Division 집 (2511) | 21 | → **149** | 144초 | 1 |
| 7 | Rippers Casino (3212) | 30 | → **150** | 147초 | 1 |

- 받기: 6개 바이트 일치, **2511 만 1바이트 초과**(1298331 / 1298330). 지금까지 차이는 모두 모자란 쪽이었는데 이번은 넘친 쪽이다 — 압축은 정상 해제되고 21개 파일이 다 나왔다. 역시 메타데이터 쪽으로 본다.
- 겹침 검사 CLEAN (0/0/0), 설치 대상 123개.
- 의존성 확인: UNTAR·RUAF·Black Division 집이 요구하는 SAIN·BigBrain·WTT Armory·WTT CommonLib·MoreBotsAPI·WTT Content Backport **여섯 다 설치본에 있다.**
- 7회 모두 오류가 기준선과 같다 — 알려진 3줄, `Could not add` 48, `Unable to add bundle` 1, `err.txt` 0바이트.
- 설치 기록 **124줄**(= 123 + Rippers Casino 가 만든 `ripperscasino.json` 1), 적힌 파일 전부 실재.

### 다음 게임 기동에서 볼 것 — 204

7개가 넣은 플러그인 DLL 은 7개이고 라이브러리도 `-Fika` 변종도 없다. **197 + 7 = 204** 다.

### 아직 확인되지 않은 것 — 이 단계는 여기서 끝나지 않는다

서버가 뜬다는 것은 **맵이 들어간다는 뜻이 아니다.** 하나씩 넣은 이유가 레이드 진입이기 때문에, 맵별로 들어가 봐야 이 단계가 닫힌다.

| 볼 것 | 왜 |
| --- | --- |
| Labyrinth 진입 | Enable Labyrinth 가 여는 맵이다. 로딩에서 멈추면 그 모드다 |
| UNTAR·RUAF·Black Division 집이 붙은 맵의 **프레임** | 셋 다 봇 스폰을 늘린다. 돌아가는 것과 쓸 수 있는 것은 다르다 |
| **탈출구 목록이 비지 않는지** | Door Randomizer(820)·LET ME OUT(2240)이 문·탈출구를 바꾼다 |
| Rippers Casino | 자기 공간을 더하는 쪽이라 들어가 봐야 안다 |

## 10-G — 전리품·경제·봇 이름 11개 · 2026-11-05

| 항목 | 결과 |
| --- | --- |
| 받기 | 11개 모두 바이트 수 **정확히 일치** (1.18 MB) |
| 겹침 검사 | CLEAN (0/0/0), 설치 대상 29개 |
| 설치 | 10개 `-SkipTest` + 마지막 2784 에서 테스트 한 번 |
| 설치 기록 | **33줄** (= 29 + 생성된 파일 4), 적힌 파일 전부 실재 |
| 서버 모드 | 138 → **145** (= 서버 쪽 7개) |
| 시작 시간 | 135초 |
| 로드 확인 | `(Added 7 mods: CratesInAirdrop, LootNet, Bosses Have GP Coins, Bosses Have Lega Medals, Bot Callsigns, Realistic PMC Names, Lots of Loot Redux)` |
| 오류 | 새 오류 0. 기준 문구 48 · 1 그대로, `err.txt` 0바이트. **플리마켓 생성 단계에서도 오류가 없다** |

출력: `server-tests/server-test-stage10-G-2784-AutoIFF.out.txt`

- Lots of Loot Redux 는 `/LotsofLoot/` 웹 화면을 서버 포트 안에 붙인다 — 기존 경로들과 겹치지 않는다.

### 생성 파일 4개를 기록에 넣었다

`OTHERMOD` 줄이 많이 나왔지만 **이번 단계 것만** 골라야 한다.

| 파일 | 주인 | 처리 |
| --- | --- | --- |
| `BotCallsigns\config.json`, `nameData\allNames.json`, `nameData\userDefinedNames.json` | Bot Callsigns (1873, 이번 단계) | 기록에 더함 (3줄 → 6줄) |
| `bushtail-CratesInAirdrop\config.json` | Crates In Airdrop (2481, 이번 단계) | 기록에 더함 (1줄 → 2줄) |
| `ORBIT\config.json` + 구역 15개 | ORBIT (10-F2) | **이미 기록돼 있다**(19줄). 서버가 매번 다시 쓰므로 다른 태그에서는 `OTHERMOD` 로 보인다 |
| `ExpandedTaskText\...descriptionCache.json` | 10-E | 이미 기록돼 있다 |
| `kazusa-SimpleItemModifier\...` | 설치본에 원래 있던 모드 | 건드리지 않는다 |

**`OTHERMOD` 는 "기록 안 해도 되는 파일"이 아니라 "이 태그의 것이 아니다"라는 뜻**이다. 앞 단계에서 이미 기록한 것인지, 이번 단계 모드의 것인지 매번 가린다.

### 정정 (2026-11-05 12:05 기동) — 예상 197 은 틀렸고 198 이 맞았다

아래에서 `LootNetFika.dll` 을 "Fika 변종이라 올라오지 않는다"고 적었는데 **틀렸다.** 실제 로그에 `Loading [LootNet.Fika 1.0.9]` 가 떴다. 그래서 10-G 의 기여는 5가 아니라 **6**이다.

왜 틀렸나 — 두 가지를 섞었다.

- `FixPluginTypesSerialization` 의 `Excluding 'LootNetFika.dll' ... It references Fika.Core which doesn't exist` 는 **직렬화 수정 대상에서 뺀다**는 뜻이지, BepInEx 가 플러그인을 안 올린다는 뜻이 아니다. 그 경고가 떠도 플러그인은 올라온다.
- 10-E 의 All Quests Checkmarks 를 선례로 삼았는데, 그 모드의 `-Fika.dll` 은 **플러그인 속성이 아예 없는 라이브러리**였다(해제본에서 확인: `AllQuestsCheckmarks-Fika.dll` → `BepInPlugin` 없음, `AllQuestsCheckmarks-Client.dll` → 있음). `LootNetFika.dll` 은 `BepInPlugin` 이 **있다**. 둘은 이름만 닮았고 종류가 다르다.

**판정 기준 정정 — `-Fika` 라는 이름으로 세지 말고 DLL 의 플러그인 속성만 본다.** 속성이 있으면 1, 없으면 0 이다. Fika 가 없으면 그 플러그인이 실제로 하는 일은 없을 수 있지만, **세는 수에는 들어간다.**

### (틀린 예상) 다음 게임 기동에서 볼 것 — 197 (198 아님)

10-G 가 `BepInEx\plugins` 에 넣은 DLL 은 6개이고 **전부 플러그인 표식이 있다.** 그런데 그중 하나가 `LootNet\LootNetFika.dll` 이다.

10-E 의 All Quests Checkmarks 가 선례다 — 그 모드도 `-Client.dll` 과 `-Fika.dll` 을 함께 깔았지만 `Loading [` 줄은 **하나**였고 플러그인 수도 1만 늘었다. 이 설치본은 Fika 를 쓰지 않으므로 `-Fika` 변종은 올라오지 않는다(`FixPluginTypesSerialization` 이 `It references Fika.Core which doesn't exist` 로 걸러낸다).

그래서 예상치는 **191 + 1(ORBIT) + 5 = 197** 이다. 198 이 나오면 `LootNetFika` 가 올라온 것이니 그때 다시 본다.

**판정 기준에 더할 것** — `-Fika` 가 붙은 DLL 은 플러그인 표식이 있어도 **0 으로 센다**(이 설치본에 Fika 가 없는 한).

### 아직 확인되지 않은 것

- **전리품 밀도** — Lots of Loot Redux(1619)가 설치된 전리품 모드와 배율이 누적될 수 있다. 맵 하나를 돌아 눈으로 봐야 한다. 서버 테스트는 "터지지 않는다"까지만 말해 준다.
- **플리 판매 흐름** — Fast Sell In Flea(2207)는 SVM 플리 설정과 함께 봐야 한다.
- **보스 전리품 둘** — Lega Medals(1539)와 GP Coins(2523)가 같은 보스 인벤토리에 넣는다. 둘 다 오류는 없지만 실제로 둘 다 떨어지는지는 보스를 잡아야 안다.

## 10-F2 — ORBIT 3.0 (2706) 단독 · 2026-11-05

| 항목 | 결과 |
| --- | --- |
| 선행 | SAIN 4.5.1 · Waypoints 1.9.0 · BigBrain 1.5.0 **셋 다 설치본에 있다** |
| 받기 | 826,706바이트 일치. Forge 최신은 **3.1.0** 이다(모드 이름은 "ORBIT 3.0") |
| 겹침 검사 | CLEAN (0/0/0), 설치 대상 2개 |
| 설치 | 2파일 + 서버가 만든 **17개**(`config.json`, `presets/library.json`, 구역 14개) → 기록 **19줄** |
| 서버 모드 | 137 → **138** |
| 시작 시간 | 144초 |
| 로드 확인 | `모드 ORBIT Server 버전 3.1.0 (GUID: com.chazut.orbit.server) 불러옴`, `(Added 1 mods: ORBIT Server (3.1.0))` |
| SAIN 쪽 | **새 경고 없음** — 이 단계를 단독으로 떼어 둔 이유가 이 확인이었다 |
| 오류 | 새 오류 0. 기준 문구 48 · 1 그대로, `err.txt` 0바이트 |

출력: `server-tests/server-test-stage10-F2-2706-ORBIT.out.txt`

- `[ORBIT] Server mod loaded - config UI at /orbit` — 서버 포트 안에 `/orbit` 을 붙인다. 기존 `/coti-assets/`·`/TheBlacklist/`·`/questcodex/`·`/SAIN/`·`/modsource/`·`/nameplate/` 와 경로가 겹치지 않는다.
- 만든 구역 파일에 **`Interchange@rework.json`·`Lighthouse@rework.json`** 이 들어 있다 — 10-O 의 Manimal 백포트 맵을 미리 알고 있는 것이다. 그 단계에 가면 이 파일들이 쓰인다.
- `[ORBIT] PRESETS: migration complete; active=Default; legacy files backed up` — 새로 깐 것이라 옮길 옛 파일은 없었다. 프리셋은 `Default` 로 시작한다.

**아직 확인되지 않은 것** — 게임 쪽 `ORBIT.dll` 은 플러그인 표식이 있어 다음 기동에서 **192** 가 되어야 한다. 그리고 이 단계의 본 시험은 **레이드에서 봇이 멈추거나 떼로 몰리지 않는지**다. 서버 테스트로는 SAIN 쪽 경고가 없다는 것까지만 확인했다.

### ORBIT 과 SAIN 은 어떤 관계인가 (2026-11-05 조사)

**덮어쓰지 않는다. 그러나 별개로 도는 것도 아니다** — 역할을 나눠 맞물린다.

덮어쓰지 않는 근거

- ORBIT 의 설치 기록 19줄에 **SAIN 경로가 0건**이다. 자기 폴더(`plugins/ORBIT`, `user/mods/ORBIT`)만 쓴다.
- ORBIT 의 `config.json`(323KB)에 **SAIN 문자열이 0건**이다. 키는 전부 자기 것이다 — `ambush`·`rush`·`multi_step`·`movement`·`zones`·`personalities`·`ai_limiter`·`poi_guard`·`main_objectives`·`loot`.
- **서버 쪽 `Orbit.Server.dll` 은 SAIN·BigBrain 을 아예 참조하지 않는다**(문자열 0건). 서버는 설정·구역 데이터만 다룬다.
- ORBIT 설치 후 SAIN 폴더에서 바뀐 파일은 `SAIN/EditorDefaults.json` 하나인데, 수정 시각이 **게임 기동 시각(12:05:13)** 이고 모든 플러그인 `.cfg` 가 같은 시각에 함께 바뀌었다. 내용에 ORBIT 언급이 없다 — **SAIN 이 자기 UI 상태를 다시 쓴 것**이다.

맞물리는 근거 — 클라이언트 `ORBIT.dll` 안에 `Orbit.Sain` 네임스페이스가 통째로 있다.

| 문자열 | 뜻 |
| --- | --- |
| `BepInDependency`, `me.sol.sain` | SAIN 을 **하드 의존**으로 선언한다 |
| `BrainManager`, `AddCustomLayer`, `CustomLayer`, `AICoreLayer` | **BigBrain 커스텀 레이어로 끼어든다** — 레이어 우선순위로 조정되는 구조이고, SAIN 도 같은 틀을 쓴다 |
| `get_IsSainAvailable`, `SainCombatLayerName`, `_sainCombatActive`, `SainResolveDeadline`, `SainResolveTimeoutSeconds`, `_sainCombatEndedAt` | SAIN 의 전투 레이어가 **도는 동안 비켜서고, 끝날 때까지 기다린다** |
| `SainStopsProtected`, `PreservePathDuringSainCleanup`, `SainCleanupScopeReady`, `SainStopGuardReady`, `ReportedSainProtection` | SAIN 의 stop·cleanup 이 **ORBIT 이 깐 경로를 지우지 못하게 막는다** |
| `SniperSainDecisionPatch`, `SniperSainAlertPatch`, `NativeGhostSainStopPatch`, `NativeGhostSainCleanupPatch`, `GhostSpawnSainContactPatch` | SAIN 메서드에 **Harmony 패치를 건다**(파일을 고치는 것이 아니라 런타임 후킹) |
| `SainPersonality` | SAIN 의 **성격 값을 읽어** 자기 행동에 쓴다 — `config.json` 의 `timmy`·`cautious`·`average`·`aggressive`·`very_aggressive` 묶음이 그것이다 |

제작자 설명도 같은 말이다 — **"ORBIT handles activity between fights and Ghost simulation. Awake combat stays with SAIN/native AI."** 즉 **싸움은 SAIN, 싸움 사이의 이동·목표·매복·탈출은 ORBIT** 이다.

### 그래서 해야 할 설정이 하나 있다 — SAIN 의 extraction 끄기

모드 페이지가 설치 절차로 못박아 둔 것이다.

> Turn SAIN extraction OFF and save your SAIN preset.

ORBIT 의 `config.json` 에는 탈출 조건(전리품·목표 완료·시간·부상)이 들어 있고, SAIN 에도 자체 탈출 기능이 있다. **둘 다 켜 두면 탈출을 정하는 주체가 둘이 된다.**

**확인 결과 — 이미 꺼져 있다 (2026-11-05).** 처음에 "아직 하지 않았다"고 적었는데 **확인하지 않고 쓴 것이었다.** 실제로는 이렇다.

- SAIN 설정은 **F12 가 아니라 F6 자체 에디터**다(`me.sol.sain.cfg` 의 `Open Editor Shortcut = F6`).
- 활성 프리셋은 **`My Preset`**(`plugins/SAIN/EditorDefaults.json` → `SelectedCustomPreset`), 파일은 `SPT_Runtime/user/mods/Solarint-SAIN-ServerMod/Presets/My Preset.json`.
- 그 안의 주 스위치가 **`GlobalSettings.General.Extract.SAIN_EXTRACT_TOGGLE = false`** 다. 파일 수정 시각이 **11-04 13:26** 으로 ORBIT 설치(11-05 11:55)보다 앞서니, ORBIT 때문에 끈 것이 아니라 **원래 꺼져 있었다.**

남아 있는 것과 그 뜻

| 항목 | 값 | 판단 |
| --- | --- | --- |
| `GlobalSettings.General.Extract.SAIN_EXTRACT_TOGGLE` | **false** | 주 스위치. 꺼져 있다 |
| `BotSettings.<봇종류>.Settings.<난이도>.Mind.EnableExtracts` | true ×244 | 봇 종류별 개별 스위치. 주 스위치가 꺼진 상태에서 이것들이 어떻게 취급되는지는 SAIN 내부 로직이라 **파일만 보고는 단정할 수 없다** |
| `GlobalSettings.General.LootingBots.ExtractFromLoot` | true | **Looting Bots 연동 항목인데 그 모드가 설치돼 있지 않다** — 무의미하다 |
| `GlobalSettings.General.Layers.SAINExtractLayerPriority` | 24 | 레이어 우선순위. 주 스위치가 꺼져 있으면 쓰이지 않는다 |

**그래도 눈으로 한 번 보는 편이 낫다** — F6 에디터를 열어 Global Settings → General → Extract 에서 토글이 꺼진 것으로 보이면 끝이다. 봇이 레이드 중에 탈출하는 것이 실제로 보이면 그때 개별 스위치를 본다.

함께 적어 둘 것 — 제작자가 **SAIN Personality Mixer**(선택)를 권한다. PMC 성격 확률을 레벨·장비와 무관하게 F12 에서 정할 수 있어 ORBIT 행동이 다양해진다고 한다. 이 설치본에는 없다.

## 레이드 확인 (10-F 설치 후, 2026-11-05 11:06까지)

10:26 기동이 11:06 까지 이어졌다. 앞 절의 10:40 판독과 **같은 기동이고 레이드가 더 깊어진 것**이다(`GameWorld`·`RaidStartPatch` 흔적 15 → 18, 로그 214KB → 270KB).

| 항목 | 10:40 | 11:06 |
| --- | --- | --- |
| `plugins to load` | 191 | **191** (같은 기동) |
| `[Error` | 1줄 | **9줄** |
| `[Fatal` | 0 | **0** |

### 9줄 전부 기존 건이다 — 10-F 가 낸 오류는 없다

| 건수 | 내용 | 판정 |
| --- | --- | --- |
| 6 | SAIN `sectactPriestEvent does not exist` | 기준선 그대로(기준선도 6) |
| 2 | `[ModulePatch] LootPatrol GetDecision exception suppressed to keep AI update alive` + 바로 다음 줄의 `System.NullReferenceException` | **한 사건이 두 줄로 찍힌 것.** 옛 기준선 메모가 "레이드에 들어가면 PIT `LootPatrolLayer.GetDecision` NullReference 가 더 날 수 있다"고 적어 둔 그 건이다. PIT 는 빠졌지만 **SPT Team Manager 가 그 포크**라 같은 코드를 들고 있다. 메시지가 말하듯 **의도적으로 억제**돼 AI 는 계속 돈다 |
| 1 | `DirectReload` + Fika | 알려진 정상 건 |

기준선 15줄 중 STM `AimingFailure`·`MedicalActivity` 와 ECOTI per-frame 은 이번에 안 나왔다 — 동료가 교전하거나 COTI 를 써야 나오는 것이라 레이드 내용에 따라 달라진다.

### 내 검색이 한 번 오탐을 냈다

10-F 모드 이름이 오류에 끼어 있는지 보려고 `movement|stamina|ladder|climb|deadzone|swing|stance|...` 로 훑었더니 1건이 걸렸다. 열어 보니 `System.NullReferenceException: Object reference not set to an instance of an object` 였다 — **`instance` 안의 `stance`** 가 걸린 것이다. 모드 이름으로 로그를 훑을 때는 **짧은 단어가 다른 낱말에 박혀 걸린다**. 걸린 줄은 반드시 열어 본다.

**결론 — 10-F 의 조작·이동 12개가 낸 오류는 0건이다.** 다만 조작감 자체(걷기·스태미나·사다리·조준 흔들림·수류탄)는 여전히 로그로 알 수 없다.

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
