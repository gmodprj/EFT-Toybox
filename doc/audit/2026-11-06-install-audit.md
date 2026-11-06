# 설치본 정합성 점검 — 2026-11-06

점검만 했다. `E:\Spt` 아래의 어떤 파일도 고치거나 지우지 않았고, 서버·게임을 실행하지 않았다.

- 저장소: `E:\Spt_Mods` (워크트리 `remove-all-in-weapon-0c24d0`, 브랜치 `claude/install-audit-5c9b00`)
- 설치본: `E:\Spt`
- 기준 커밋: `e7b4ac2` (develop 머지 시점, 10-O 의 3022 까지 들어간 상태)

## 요약

| 항목 | 실측 | 문제 |
|---|---|---|
| 1. 기록 ↔ 실제 파일 | 기록 13,932줄 / 303모드 | 없는 파일 26개 (8모드) |
| 2. 같은 경로 중복 기록 | — | 3건 (각각 7모드가 같은 경로를 기록) |
| 3. 서버 모드 수 | 171개 디렉터리 | 0건 (171 과 일치) |
| 4. 플러그인 DLL ↔ 로드 | DLL 280 / 로드 243 | 0건 (차이 37 전부 설명됨) |
| 5. 설정 파일 고아 | cfg 185개 | 0건 |
| 6. SPT 버전 표기 | 서버 로그의 `targets SPT:` 171개 | **0건** (아래 6-정정) |
| 7. 마지막 테스트 서버 출력 | 48 / 1 / 1 그대로 | 0건 (추가 오류 줄 없음) |

**실제 손실 0건.** 항목 1·2 에 적힌 26개는 전부 **기록상의 잡음**이다 — 아래 1-정정·2-정정 을 본다. 항목 6 은 점검 불가가 아니었다 — 아래 6-정정.

---

## 정정 (2026-11-06, 컨트롤러 실측)

이 보고서를 받은 메인 세션이 숫자를 전부 다시 재고 결론을 검증했다. **숫자는 하나도 어긋나지 않았다** — 기록 284파일 / 13,932줄 / 모드 303 / 고유 경로 13,914 / 중복 3 / 없는 파일 26(8모드), 서버 모드 171, 플러그인 DLL 280, cfg 185. 아래 세 곳만 결론이 달라진다.

### 1-정정 · 2-정정 — Quartermaster 는 유실이 아니다

보고서는 2817 TheQuartermaster 를 제거할 때 **다른 7개 모드의 퀘스트 기여분이 함께 사라졌다**(실질 유실)고 적었다. 그렇지 않다.

근거 셋:

1. 그 7개 모드의 **압축 해제본에 그 경로가 없다.** `extracted/stage7-D/3113__IncendiaryGrenade` 를 `-ipath "*Quartermaster*"` 로 훑어 0건이다. 모드가 가져온 파일이 아니다.
2. **2817 자신의 설치 기록에 그 3경로가 들어 있다.** `doc/install-records/removed/installed-stage7D-2817.txt`(15줄)에 `Quartermaster\db\CommunityQuests\66789abcde1234567890abcd\` 아래 3줄이 그대로 있다. 처음부터 그 모드의 파일이다.
3. 그 7개 모드는 **전부 같은 단계(7-D)에서 하나씩** 설치됐다(`installed-stage7D-1569/1591/2414/2516/2681/2691/3113.txt`).

즉 Quartermaster 의 서버 모드가 기동 때마다 그 폴더를 다시 쓰고, 7-D 의 모드를 하나씩 설치할 때마다 **설치 후 스캔이 그것을 "새로 생긴 파일" 로 보아 그때 설치 중이던 모드 몫으로 적은** 것이다. 설치 기록은 `NEWFILE` 을 복사한 파일과 같은 형식으로 적으므로 구분이 남지 않는다(3113 의 기록 9줄 중 해제본에 있는 것은 6줄뿐이다).

**2817 제거는 자기 파일을 가져간 것이 맞고, 7개 모드가 잃은 것은 없다.** 따라서 항목 2 의 "덮어쓰기 가드가 작동하지 않았다" 도 성립하지 않는다 — **복사가 일어난 적이 없으므로 가드가 볼 것도 없었다.**

없는 파일 26개의 내역은 이렇게 정리된다.

| 내역 | 수 | 성질 |
| --- | --- | --- |
| RaidReview 가 회전시킨 런타임 로그 | 5 | 기록상의 잡음 |
| Quartermaster 몫(3경로 × 7모드) | 21 | 기록상의 잡음 |
| **실제 모드 파일 유실** | **0** | — |

남은 것은 **철 지난 기록 줄 26개**이고, 고치려면 그 줄을 기록에서 빼는 일이다.

**정리했다 (2026-11-06).** 26줄을 지워 `node tools/check-install-records.mjs` 가 **`OK — 0/0`** 이 됐다(기록 줄 14,027 → 14,001). 무엇을 왜 지웠는지는 [`doc/install-records/README-removed-stale-lines-2026-11-06.md`](../install-records/README-removed-stale-lines-2026-11-06.md) 에 줄 단위로 남겼다 — 기록을 지우는 일이므로 지운 내역은 남아야 한다. "없는 것은 다 지운다" 로 하지 않고 **아는 두 종류만** 지웠다(RaidReview 런타임 로그 5, TheQuartermaster 몫 21). 그래야 앞으로 생길 진짜 유실이 이 청소에 섞여 사라지지 않는다.

### 6-정정 — 버전 표기는 서버 로그가 찍는다

"서버 모드에 `package.json`/`sptVersion` 이 0개라 점검 불가" 는 **관찰은 맞다**(4.x 서버 모드는 .NET 어셈블리다). 다만 표기를 읽을 길이 없는 것은 아니었다 — **서버가 적재할 때 모드마다 `targets SPT:` 를 찍는다.**

2026-11-06 06:31 기동(게임 없이 서버만)에서 모아 보니 **합계 정확히 171개**로, 모든 서버 모드가 선언을 갖고 있다.

| 표기 | 모드 수 | 4.1.6 포함? |
| --- | --- | --- |
| `~4.1.0` | 93 | 포함 |
| `~4.1.6` | 19 | 포함 |
| `~4.1.3` | 16 | 포함 |
| `~4.1.5` | 14 | 포함 |
| `~4.1.2` | 12 | 포함 |
| `~4.1.1` | 5 | 포함 |
| `~4.1` | 5 | 포함 |
| `>=4.1.3 <4.2.0` | 2 | 포함 |
| `>=4.1.3` | 2 | 포함 |
| `=4.1.6` | 2 | 일치 |
| `~4.1.X` | 1 | 포함 (TraderModding 2.1.2) |

**4.1.6 을 포함하지 않는 표기는 0개다.** `불러옴` 줄도 171줄로 거부된 모드가 없다. 자세한 것은 `STAGE10-RESULT.md` 의 "서버만 켜서 확인 (3022 설치 후)" 절.

**점검 항목을 잘못 준 쪽은 메인 세션이다** — `package.json` 기준으로 재라고 지시했고, 하위 세션이 그것이 이 설치본에 존재하지 않는다는 것을 짚어 되돌려 준 것이 맞는 판단이었다.

### 그 밖은 그대로 선다

항목 3·4·5·7 의 판정은 다시 재어 전부 맞았다. 특히 항목 4 에서 DLL 280 과 로드 243 의 차이 37 을 라이브러리 36 + 아직 켠 적 없는 3022 의 DLL 1 로 빈틈없이 설명하고 `spt-reflection.dll` 오탐을 근거를 들어 걸러낸 것, 항목 5 에서 유일한 후보가 `plugins` 가 아니라 `patchers` 에 있는 프리로더 패처임을 짚은 것은 그대로 선다.

---

## 1. 설치 기록 ↔ 실제 파일 대조

**어떻게 셌나.** `doc/install-records/` 의 1단계(하위 `removed/` 제외) `installed-*.txt` 284개를 모두 읽어, 탭으로 나눈 2번째 필드를 `E:\Spt` 기준 상대경로로 보고 `fs.existsSync` 로 존재를 확인했다(경로 구분자 `\`→`/` 치환, 대소문자 그대로).

**실측값**

- 기록 파일 284개, 총 **13,932줄**, 모드 폴더 이름 **303개**, 형식이 깨진 줄 0개.
- `doc/INSTALLED-MODS.md` 의 303모드 / 13,932파일과 **정확히 일치**한다.
- 없는 파일 **26개 / 8모드**.

**M>0 인 모드**

| 모드 폴더 이름 | 기록 줄 | 없는 것 |
|---|---|---|
| `1479__raid_review__1.10.0` | 22 | 5 |
| `1569__WolfiksHeavyTroopers1.2.0` | 13 | 3 |
| `1591__SPTBetterRearSights` | 43 | 3 |
| `2414__Peacemaker-2.1.0` | 18 | 3 |
| `2516__22lr` | 24 | 3 |
| `2681__LoadAmmoAnim-2.0.0` | 10 | 3 |
| `2691__FoldThatStock-v2.1.0` | 25 | 3 |
| `3113__IncendiaryGrenade` | 9 | 3 |

### 1-a. RaidReview 로그 5개 — 문제 아님(기록상의 잡음)

```
SPT_Runtime\user\mods\RaidReview\data\logs\1793858840701_raid_review.log
SPT_Runtime\user\mods\RaidReview\data\logs\1793858981523_raid_review.log
SPT_Runtime\user\mods\RaidReview\data\logs\1793859145990_raid_review.log
SPT_Runtime\user\mods\RaidReview\data\logs\1793859292470_raid_review.log
SPT_Runtime\user\mods\RaidReview\data\logs\1793859446422_raid_review.log
```

`data/logs/` 디렉터리 자체는 있고, 지금은 더 최근 타임스탬프의 로그들이 들어 있다(`1793884123210_…` 이후). 설치 기록을 뜰 때 모드가 그 자리에서 만들어 둔 **런타임 로그까지 기록에 들어갔고**, 그 뒤 모드가 로그를 회전시켰다. 모드 파일 유실이 아니다.

### 1-b. TheQuartermaster 하위 21개 — ~~실질 유실~~ (아래 결론은 틀렸다 → 위 **1-정정**)

> **이 절의 결론은 뒤에 뒤집혔다.** 유실이 아니라 철 지난 기록 줄이다. 위의 "정정 · 1-정정 · 2-정정" 을 본다. 아래는 당시 적은 그대로 남겨 둔다.

7개 모드가 모두 같은 3개 경로를 기록하고 있고, 셋 다 없다.

```
SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Images\quest.png
SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Locales\en.json
SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Quests\quest_definitions.json
```

근거: `E:\Spt\SPT_Runtime\user\mods\TheQuartermaster` **디렉터리 자체가 없다**. 그리고 `doc/install-records/removed/installed-stage7D-2817.txt` 가 `2817__TheQuartermaster-1.4.3` 의 기록이고, 그 안에 `SPT_Runtime\user\mods\TheQuartermaster\…` 가 들어 있다.

즉 **2817 TheQuartermaster 를 제거할 때 그 모드 폴더를 통째로 지웠고, 거기에 얹혀 있던 다른 7개 모드의 CommunityQuests 기여분이 함께 사라졌다.** 7개 모드의 본체(플러그인·서버 DLL·db)는 자기 경로에 그대로 있다 — 없어진 것은 Quartermaster 용 퀘스트 기여분뿐이다.

고치지 않았다(진단까지가 이 작업 범위다).

---

## 2. 같은 경로를 두 모드가 기록했는지

**어떻게 셌나.** 1에서 모은 13,932줄의 상대경로를 소문자·`/` 로 정규화해 키로 묶고, 한 키에 서로 다른 모드 폴더 이름이 2개 이상 붙은 것을 셌다.

**실측값: 3건** (0건이 아니다)

> **아래 "덮어쓰기 차단이 작동하지 않았다" 는 틀렸다** — 복사가 일어난 적이 없어 가드가 볼 것이 없었다. 위 **2-정정** 을 본다. 중복 3건이라는 수 자체는 다시 재어 맞았다.

| 경로 | 기록한 모드 |
|---|---|
| `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Images\quest.png` | 1569, 1591, 2414, 2516, 2681, 2691, 3113 (7개) |
| `…\66789abcde1234567890abcd\Locales\en.json` | 같은 7개 |
| `…\66789abcde1234567890abcd\Quests\quest_definitions.json` | 같은 7개 |

7개 모드가 각자 Quartermaster 용 커뮤니티 퀘스트를 **같은 하드코딩 ID `66789abcde1234567890abcd`** 로 담아 배포한다. 경로가 완전히 같으므로 나중에 설치한 쪽이 앞선 쪽을 덮었을 것이고, 지금 `E:\Spt` 에 남은 내용은 **마지막으로 설치된 한 모드의 것뿐**이었을 것이다(지금은 1-b 대로 전부 없다).

설치 스크립트의 덮어쓰기 차단이 이 7건에서는 작동하지 않았다. 재지 않은 것: 왜 안 걸렸는지(내용이 같아서 건너뛴 건지, 가드가 이 경로 형태를 못 본 건지) — `tools/install-and-test.ps1` 의 동작은 이번에 확인하지 않았다.

---

## 3. 서버 모드 수 대조

**어떻게 셌나.** `E:\Spt\SPT_Runtime\user\mods\` 의 1단계 하위 디렉터리만 셌다(파일 제외).

**실측값: 171개 → 기준 171 과 일치. 문제 0건.**

후속 점검(어긋나지 않았으므로 참고용):

- `package.json` 이 있는 디렉터리: **0개** (171개 전부 없다 → 항목 6 참조)
- `.dll` 이 하나도 없는 디렉터리: **0개**

SPT 4.x 서버 모드는 Node 패키지가 아니라 .NET 어셈블리다(예: `AmmoGen/AmmoGen.Server.dll`, `Airburst/maschine-Airburst.Server.dll`). 그래서 `package.json` / `main` 기준의 "서버가 세지 않는 폴더" 판정은 이 설치본에 적용되지 않는다. 171개 모두 DLL 을 갖고 있고, 테스트 서버도 `ModLoader: 총 171 개` 로 같은 수를 보고한다.

---

## 4. 플러그인 DLL ↔ 로드된 플러그인 대조

**어떻게 셌나.**

- DLL: `E:\Spt\BepInEx\plugins` 아래 재귀 `*.dll` → **280개**
- 로드: `E:\Spt\BepInEx\LogOutput.log` 의 `Loading [` 줄 → **243줄**. 같은 로그 54행에 `243 plugins to load`. 로그 파일 수정 시각 2026-11-06 04:44:09 (KST).
- 대응 판정: 로그에서 뽑은 플러그인 이름 243개를 각 DLL 바이너리 안의 UTF-8 문자열로 찾고, 동시에 각 DLL 이 `BepInPlugin` 문자열을 갖는지 봤다.

**차이 280 − 243 = 37. 37 전부 설명된다. 원인 불명 0건.**

### ① 플러그인 속성이 없는 라이브러리 DLL — 36개

`BepInPlugin` 문자열이 아예 없는 DLL **35개**:

```
Anvil-WebOverlay\WebView2Loader.dll          Blackout\BlackoutFika.dll
Briefing\Briefing.Core.dll                   EFTEffectManager.dll
ISBAishiPlugin\AishiKeysV2.Fika.dll          ISBSpecialForcesPlugin\ISBNotify.Fika.dll
ISBSpecialForcesPlugin\ISBSOF_Extras.Fika.dll  LoadAmmoAnim\LoadAmmoAnimClientFika.dll
ManimalInterchange\ManimalInterchange.Components.dll
ManimalInterchange\ManimalInterchange.Shared.dll
ManimalLighthouse\ManimalLighthouse.Shared.dll
maschine-Airburst.Client.Fika.dll            ozen.MagCheckInterrupt\ozen.MagCheckInterrupt.Net.dll
SAIN\SAIN.Preset.Shared.dll                  SkillsExtended\SkillsExtended.Client.API.dll
SkillsExtended\SkillsExtended.Common.dll     spt\spt-common.dll
tarkin-HideoutCat\AssetBundleLoader.dll      tarkin-ladders\tarkin.ladders.shared.dll
Tosox.MagRetentionReload.Net.dll             TraceSix\native\TraceSixRT.dll
Tyfon.UIFixes.Net.dll                        UnityToolkit\UniTask.Addressables.dll
UnityToolkit\UniTask.dll                     UnityToolkit\UniTask.DOTween.dll
UnityToolkit\UniTask.Linq.dll                UnityToolkit\UniTask.TextMeshPro.dll
UnityToolkit\Unity.Collections.dll           UnityToolkit\VContainer.dll
UnityToolkit\ZLinq.dll                       UnityToolkit\ZLinq.Unity.dll
UnityToolkit\ZLinq.Unity.UnityCollectoins.dll  UnityToolkit\ZString.dll
WTT-ClientCommonLib\WTT-ClientCommonLibFika.dll
WTT-ContentBackportClient\WTT-ContentBackport-MainMenu.dll
```

여기에 **`spt\spt-reflection.dll` 1개를 더한 36개**다. 이 DLL 은 `BepInPlugin` 문자열은 갖고 있지만, 바이너리에서 그 문자열 주변이 `IsAssignableFrom` · `SingleCustom` · `BaseUnityPlugin` · `unityPlugin` 이고 `BepInDependency` 는 0회다 — 즉 **리플렉션으로 남의 플러그인 속성을 찾기 위해 타입 이름을 문자열로 들고 있는 라이브러리**이고 자기 자신이 플러그인은 아니다. 문자열 스캔 방식의 유일한 오탐이었고, 근거를 확인해 ① 로 분류했다.

`*.Fika.dll` 들은 로그 19~33행의 `Excluding '...' from Serialization Fix. It references Fika.Core which doesn't exist.` 와 같은 파일들이다 — Fika 미설치로 직렬화 수정에서만 빠졌고, 애초에 플러그인 속성이 없는 보조 어셈블리다.

### DLL 은 있는데 로드 줄이 없는 것 — 1개 (문제 아님)

```
ManimalLighthouse\ManimalLighthouse.dll
```

근거: 이 파일의 **CreationTime 이 2026-11-06 05:08:22** 이고, 로그 기동은 **04:44:09** 다. 로그가 3022 설치보다 **먼저** 찍혔으므로 로드된 적이 없는 것이 당연하다. 지시받은 대로 문제로 세지 않았다.

### ② DLL 하나가 플러그인 여러 개 등록 — 이번 차이에는 0건 기여

280 − 36(라이브러리) = **244개**가 플러그인 DLL 이고, 로드된 243 + 아직 안 뜬 ManimalLighthouse 1 = **244** 로 **1:1 로 맞는다.** 그래서 이 기동에서는 "DLL 하나가 여러 플러그인" 이 차이를 만들지 않았다.

(참고: 문자열 스캔만으로는 "그 이름을 등록했다" 와 "의존 어셈블리 이름을 참조만 했다" 가 구분되지 않아, 여러 이름이 걸린 DLL 이 다수 나왔다. 수가 1:1 로 맞으므로 그 목록은 ② 의 증거가 아니라 참조 문자열로 본다. 어느 DLL 이 몇 개를 등록하는지는 **재지 않았다**.)

### ③ GUID 겹쳐 조용히 버려짐 — 이번 로그에는 0건

로그에서 `BepInEx` 태그 줄 전부를 훑었다. `Skipping` · 중복 GUID · 버전 충돌을 알리는 줄이 **하나도 없다.** `243 plugins to load` 뒤에 `Chainloader startup complete` 로 바로 끝난다. 또 로드된 243개 이름 전부가 어느 DLL 안에서든 발견되었다 — 로그에만 있고 파일이 없는 플러그인도 0개다.

---

## 5. 설정 파일 고아 찾기

**어떻게 셌나.** `E:\Spt\BepInEx\config\*.cfg` 185개의 파일명(= GUID)을 각각 `plugins` 아래 280개 DLL 바이너리 안의 문자열로 찾았다. 하나도 안 걸린 것만 후보로 뽑고, 그 GUID 가 `LogOutput.log` 에 나오는지 함께 봤다.

**실측값: cfg 185개, 어느 DLL 에도 안 걸린 것 1개. 실질 고아 0건.**

| cfg | 판정 |
|---|---|
| `FixPluginTypesSerialization.cfg` | **고아 아님.** 이것은 플러그인이 아니라 **프리로더 패처**다. 파일이 `E:\Spt\BepInEx\patchers\FixPluginTypesSerialization.dll` 에 있고(그래서 `plugins` 스캔에 안 걸렸다), 로그 7~8행 `Loaded 1 patcher method from [FixPluginTypesSerialization 1.0.0.0]` 으로 정상 동작 중이다. |

지운 모드가 남긴 cfg 는 **0건**이다. 아무것도 지우지 않았다.

---

## 6. SPT 버전 표기 점검 — ~~점검 불가~~ (서버 로그로 점검됐다 → 위 **6-정정**)

> `package.json` 이 0개라는 관찰은 맞다. 다만 표기는 **서버 로그의 `targets SPT:`** 로 읽을 수 있었고, 그렇게 재니 171개 전부가 4.1.6 을 덮는다. 위 **6-정정** 을 본다.

**어떻게 셌나.** `E:\Spt\SPT_Runtime\user\mods\*\package.json` 171자리를 전부 확인하고, 모드 트리 전체에서 `sptVersion` 문자열을 재귀 검색했다(`*.json` `*.txt` `*.md` `*.cfg`).

**실측값**

- `package.json` 이 있는 모드 디렉터리: **0 / 171**
- 따라서 `sptVersion`(및 `akiVersion`) 표기가 있는 모드: **0개**, 표기가 없는 모드: **171개**
- 4.1.6 을 포함하지 않는 표기: **0개** (셀 대상이 없다)

항목 3 에 적은 대로 SPT 4.x 서버 모드는 .NET 어셈블리로 배포되고, 호환 버전은 JSON 매니페스트가 아니라 어셈블리 쪽에 들어 있다. **파일에서 읽을 수 있는 `sptVersion` 표기라는 것이 이 설치본에는 존재하지 않는다.** 어셈블리 메타데이터를 열어 버전 선언을 뽑는 것은 **재지 않았다**(이 점검 범위를 넘고, 그 값의 의미를 확인할 근거가 없다).

참고로 전체 검색에서 `sptVersion` 문자열이 걸린 파일은 딱 하나다:

```
SPT_Runtime\user\mods\QuestCodex\Data\vanilla-quest-ids.json   →   "sptVersion": "4.1.5"
```

이것은 모드의 호환 선언이 아니라 **그 모드가 들고 다니는 데이터 스냅샷이 어느 버전에서 떠졌는지**를 적은 값이다. 설치본은 4.1.6 이므로 한 단계 낮은 스냅샷이다. 실제로 문제가 되는지는 **재지 않았다**(항목 7 의 `cannot import quest` 1건이 이것과 관련 있는지도 확인하지 않았다).

---

## 7. 마지막 테스트 서버 출력 확인

**어떻게 골랐나.** `git log --diff-filter=A -- doc/server-tests/` 로 가장 최근에 추가된 출력 파일을 골랐다(워크트리 파일 수정 시각은 체크아웃 시각이라 쓸 수 없다). → `f5dfa13` (2026-11-06, 3022 Lighthouse) 의

```
doc/server-tests/server-test-stage10-O-3022-Lighthouse.out.txt   (1,426줄)
doc/server-tests/server-test-stage10-O-3022-Lighthouse.err.txt   (0바이트)
```

**알려진 오류 수 — 기준선 그대로. 늘지 않았다.**

| 패턴 | 기준 | 실측 |
|---|---|---|
| `Could not add` | 48 | **48** |
| `Unable to add bundle` | 1 | **1** |
| `cannot import quest` (대소문자 무시) | 1 | **1** |

`ModLoader: 총 171 개의 모드 불러오는 중...` — 항목 3 의 171 과 일치.

**그 밖의 `error`·`exception`·`fail` 줄 (대소문자 무시, 중복 제거, `Ignoring; Not an error` 제외): 0건.**

파일 전체에서 이 세 단어가 걸린 줄은 4줄뿐이고, 내역은 이렇다.

- 448·450·452행: `656ddcf0f02d7bcea90bf395 - Ignoring; Not an error, don't report it` → 지시대로 제외
- 1341행: `[COTI] meshes: 0 converted, 10 unchanged, 0 removed, 0 failed` → `0 failed`, 성공 보고이지 오류가 아니다

한국어 `오류`·`실패` 로도 찾아봤고 0건이다. `err.txt` 는 0바이트다.

---

## 하지 않은 것 / 재지 않은 것

- `E:\Spt` 를 읽기만 했다. 고치거나 지우거나 옮긴 것 없다.
- SPT 서버·게임을 실행하지 않았다. 그래서 **3022 설치 후의 플러그인 수(예상 244)는 실측하지 못했다** — 항목 4 는 04:44 기동 로그(243) 기준이다.
- `doc/INSTALLED-MODS.md` 를 고치지 않았다.
- 발견한 문제를 고치지 않았다.
- `tools/install-and-test.ps1` 의 덮어쓰기 가드가 왜 항목 2 의 7건을 못 막았는지 — 확인하지 않았다.
- 어느 DLL 이 플러그인을 몇 개 등록하는지(어셈블리 메타데이터 파싱) — 재지 않았다.
- 서버 모드 어셈블리의 호환 버전 선언 — 재지 않았다.
