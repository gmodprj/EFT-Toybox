# 11차 결과 — 성능·메모리

후보는 [CANDIDATES-PERF.md](CANDIDATES-PERF.md), 왜 이것부터인지는 [MEMORY-AUDIT-2026-11-11.md](MEMORY-AUDIT-2026-11-11.md).
절차는 [PROCEDURE.md](PROCEDURE.md).

## 11-A — VRAM Cleaner 1.1.0 (2026-11-11)

| 항목 | 값 |
| --- | --- |
| 모드 | [VRAM Cleaner](https://sp-mod.com/mod/2173) 1.1.0 (`4.1.6`, 결속 없음, Fika 호환 표기) |
| 받은 것 | `SPTVRAMCleaner-v1.1.0.zip` **3,201 B** (매니페스트 크기와 바이트까지 같다) · `github release-assets` 로 리다이렉트 |
| 넣은 것 | `BepInEx\plugins\SPTVRAMCleaner.dll` **1개 / 6,144 B** |
| 기록 | `install-records/installed-stage11-A-2173-VRAMCleaner.txt` (1줄) |
| 겹침 | 없음 — 설치본에 같은 경로 없음, 다른 모드 기록에도 없음, 비슷한 이름의 플러그인도 없다 |
| 프로필 백업 | `backup/profiles-before-stage11-A/` 191개 / 155.0 MB |
| 서버 시험 | **216초에 시작 완료**. `ModLoader: 총 174 개` — **게임 전용이라 서버 모드 수는 그대로**다 |
| 서버 오류 | `Could not add` **48** · `Unable to add bundle` **1** · `cannot import quest` **1** — 기준선과 같다. 표준오류 출력은 **0바이트** |
| 기록 검사 | `node tools/check-install-records.mjs` → **`OK — 0/0`** |
| 목록 | `node tools/gen-installed-mods.mjs` → 모드 **304 → 305개**, 파일 **15,947 → 15,948개** |

### 확인한 방법

- 설치 보고를 믿지 않고 **설치본 파일의 md5 를 해제본과 견뤘다** — `f849e6c162c930b764785063af33995b` 로 같다.
- `install-and-test.ps1` 이 시험 중 새로 생긴 파일 20개를 `OTHERMOD` 로 가려내 **이 모드 몫으로 적지 않았다**(ORBIT 의 zones 17개, ExpandedTaskText 캐시, RaidReview 로그, UltimateFlea 경제 상태). 설치 기록 오귀속을 막는 쪽으로 동작한 것이다.
- 플러그인 locale 넷은 `node tools/check-plugin-locales.mjs` → 설치본과 같다.

### 그 뒤 기동에서 재어진 것 (2026-11-12)

11-12 17:11 기동(`BepInEx\LogOutput.log`)과 돌고 있는 서버(16:18 기동)로 위 셋을 다 쟀다.

| 미확인이던 것 | 실측 |
| --- | --- |
| `247 plugins to load` | **`248 plugins to load`** — 247 이 아니다. VRAM Cleaner 뿐 아니라 `IcebreakerOffMapGuard`(11-11 17:53)까지 들어와 246 → 248 이다 |
| `VRAMCleaner loaded!` | **나왔다** (`[Info :SPTVRAMCleaner-matsix] VRAMCleaner loaded!` + 패치 둘 `VRAMCleaner`·`VRAMCleanerReset`) |
| F12 한 항목이 사전에 없다 | **그 항목 자체가 없다.** 기동 뒤에도 `BepInEx\config\` 에 이 모드의 `.cfg` 가 생기지 않았다 — 설정을 바인드하지 않는다. `check-lang-coverage.mjs` 는 그대로 **5,136/5,150** 이고 사전에 더할 줄이 없다. DLL 안의 문구는 F12 설정이 아니라 레이드 중 알림이다 |
| `check-locale-dict.mjs kr` 미확인 | 켜져 있는 서버에게 물었다 — locale `kr` **61,391항목**, 우리 사전 **8,468/8,468 적용**, 값이 다름 0 · 열쇠 없음 0 · 묶음 누락 0. 꾸밈이 덧댄 142항목은 글자가 우리 값이다 |

### 아직 확인하지 못한 것

- ~~`IcebreakerOffMapGuard` 는 11-12 17:11 기동에서 아무것도 가리지 못했다~~ **21:36 기동에서 걸렸다** — hideout 에서 `[PerfCensus]` 0회(지난 기동 3회). 아래는 그때의 기록이다.
- `IcebreakerOffMapGuard` 는 **11-12 17:11 기동에서 아무것도 가리지 못했다** — F12 열쇠의 홑따옴표로 `Awake` 가 터졌다. 19:07 에 고쳐 다시 넣었고, 걸렸는지는 **다음 기동**에서 본다. 경위는 [fixes/2026-11-11-icebreaker-offmap-guard.md](fixes/2026-11-11-icebreaker-offmap-guard.md) §6

### 다음 기동에서 할 A/B

[MEMORY-AUDIT-2026-11-11.md](MEMORY-AUDIT-2026-11-11.md) §6 그대로다. **켜기 전에 `E:\Spt\Logs\log_*` 를 복사해 둔다** — 그 폴더는 기동마다 새로 생기고, 비교할 앞 값(11-11 기동: 청소 직전 총량 2,367 → 6,696 MB)은 지금 하나뿐이다.

되돌리기: 기록 파일에 적힌 DLL 하나를 지우면 끝이다(결속이 아니라 프로필 복원이 필요 없다).
