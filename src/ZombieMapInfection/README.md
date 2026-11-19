# ZombieMapInfection — 맵별 좀비 감염률

SPT 에 든 2024 할로윈 좀비 이벤트의 **맵별 감염률**을 설정대로 바꾸는 서버 모드. 기본 설정은 **세관(bigmap)·인터체인지를 50%** 로 올린다 — SPT 기본값이 그 두 맵만 0 이라 좀비가 안 나왔다.

좀비 자체는 [PPES](https://sp-mod.com/mod/3232)(15-A)의 할로윈 "Zombies and infected (2024)" 부분이 켠다. 이 모드는 **어느 맵에 얼마나** 만 정한다. PPES 에는 맵별 감염률 설정이 없다(2026 발병 부분만 따로 덮는다 — 역컴파일 확인).

## 설치·제거

```powershell
dotnet build src/ZombieMapInfection -c Release
pwsh -File tools/install-zombiemapinfection.ps1          # 서버가 꺼져 있어야 한다
pwsh -File tools/install-zombiemapinfection.ps1 -Remove
```

`config.jsonc` 는 이미 있으면 덮지 않는다. 서버 모드라 서버를 다시 켜야 적용된다.

## 설정

```jsonc
{
  "ModEnabled": true,
  "Infection": { "bigmap": 50, "Interchange": 50 }
}
```

- 맵 열쇠는 `seasonalevents.json` 의 할로윈 `zombieSettings.mapInfectionAmount` 이름이다(대소문자 무관): `bigmap`(세관) · `Interchange` · `Woods` · `Shoreline` · `RezervBase` · `Lighthouse` · `TarkovStreets` · `laboratory` · `Sandbox`(그라운드 제로) · `factory4`(팩토리 낮·밤).
- 적지 않은 맵은 SPT 기본값 그대로 — 연구소 100, 우즈 60, 팩토리·해안선·리저브·등대·스트리트 50, 그라운드 제로 25.
- 0~100 밖의 값은 잘라 쓴다. 목록에 없는 열쇠는 경고만 남기고 건너뛴다.

## 왜 이 순서인가 (역컴파일 실측, SPT 4.1.6 · PPES 1.7.7)

| TypePriority | 누가 | 무엇을 |
| --- | --- | --- |
| **199998** | 이 모드 | `SeasonalEventConfig`(주입되는 단일 인스턴스)의 할로윈 감염률을 고친다 |
| 199999 | PPES `SeasonalApplier` | 기본값을 읽어 간다(웹 `/ppes` 레이드 미리보기의 감염률도 여기서 온다) |
| 200000 | SPT `GameCallbacks` → `PostDbLoadService.PerformPostDbLoadActions` → `SeasonalEventService.EnableSeasonalEvents` → `ConfigureZombies` | 감염률 0 초과 맵에 `halloweenzombies` 보스(감염자)를 넣고 맵의 `InfectionPercentage` 를 정한다 |

다른 자체 모드처럼 `OnLoadOrder.PostLoad`(1000000)에 두면 **이미 지나간 뒤라 아무 효과가 없다.** SPT 파일(`SPT_Data/configs/seasonalevents.json`)을 직접 고치지 않는 것은 SPT 를 다시 깔면 날아가기 때문이다.

세관·인터체인지의 감염자 스폰 자체는 SPT 기본 `eventBossSpawns.halloweenzombies` 에 이미 정의돼 있다(맵 12곳) — 감염률이 0 이라 들어가지 않았을 뿐이다.
