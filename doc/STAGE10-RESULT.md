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

## 다음 단계의 판정 기준에 더할 것

1. **플러그인 수 증가분은 DLL 개수가 아니다.** 라이브러리 DLL(플러그인 속성 없음)은 0 으로 세고, DLL 하나가 플러그인 여러 개를 등록할 수 있다. 어긋나면 `Loading [` 목록을 직접 비교한다.
2. **GUID 충돌은 조용하다.** 같은 GUID 를 가진 플러그인이 둘이면 하나는 경고 없이 버려진다. 프레임워크·설정 화면 계열을 넣은 뒤에는 `Loading [` 목록에서 **사라진 이름이 없는지** 본다.
3. **오류 줄 수는 레이드 깊이에 따라 달라진다.** 적게 나온 것을 통과로 치지 않는다. 비교는 같은 깊이의 기동끼리, 또는 문구별로 한다.
