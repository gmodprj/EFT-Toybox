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

### 아직 확인하지 못한 것

- **게임에서 로드된 적이 없다.** `BepInEx\LogOutput.log` 의 수정 시각이 **15:01**, 설치는 **15:40** 이다. 다음 기동에서 봐야 하는 것 둘:
  - `247 plugins to load` (246 → 247). 이 DLL 은 플러그인 속성이 있으므로 1 늘어야 한다.
  - `VRAMCleaner loaded!` — DLL 안에 그 문구가 있어 **기동 로그로 확인된다**.
- **F12 설정 한 항목이 아직 사전에 없다.** DLL 에 `ShowRaidStartInfo` 와 레이드 중 알림 문구(`Unused Assets have been unloaded - VRAM Usage cleaned`)가 들어 있다. `.cfg` 는 첫 기동에 생기므로, 그 뒤에 `node tools/check-lang-coverage.mjs`(지금 5,136/5,150)를 다시 돌려 한 줄을 더한다.
- `node tools/check-locale-dict.mjs kr` 은 **서버가 꺼져 있어 묻지 못했다** — "통과" 가 아니라 **미확인**이다. 서버를 띄운 뒤 다시 센다.

### 다음 기동에서 할 A/B

[MEMORY-AUDIT-2026-11-11.md](MEMORY-AUDIT-2026-11-11.md) §6 그대로다. **켜기 전에 `E:\Spt\Logs\log_*` 를 복사해 둔다** — 그 폴더는 기동마다 새로 생기고, 비교할 앞 값(11-11 기동: 청소 직전 총량 2,367 → 6,696 MB)은 지금 하나뿐이다.

되돌리기: 기록 파일에 적힌 DLL 하나를 지우면 끝이다(결속이 아니라 프로필 복원이 필요 없다).
