# ScavRaidWaves — 스캐브로 들어간 레이드에 PMC가 나오게 한다

스캐브로 레이드에 들어가면 PMC가 한 명도 보이지 않던 문제를 고친다.

## 왜 안 나왔나

PMC는 웨이브로 들어온다. 시각표는 `SPT_Data\configs\pmc.json`의 `customPmcWaves`에 있고,
전부 레이드 **앞쪽**에 몰려 있다.

```
bigmap(커스텀) : 14개, Time = 30, 30, 30, 30, 360, 360, 720, 720, 1080, 1080, 1440, 1440, 1800, 1800
interchange    : 14개, Time = 30, 30, 30, 30, 300, 300, 600, 600,  900,  900, 1440, 1440, 1800, 1800
woods          : 14개, Time = 30, 30, 30, 30, 360, 360, 720, 720, 1080, 1080, 1440, 1440, 1800, 1800
```

그런데 스캐브 레이드는 **이미 진행 중인 레이드에 끼어드는 것**으로 처리된다.
`SPT_Data\configs\location.json`의 `scavRaidTimeSettings`가 그것을 정한다.

| 맵 | `reducedChancePercent` | 깎는 폭 | `adjustWaves` |
| --- | --- | --- | --- |
| bigmap (커스텀) | 100 | 40~70% | true |
| interchange | 100 | 40~70% | true |
| laboratory | 100 | 40~70% | true |
| factory4_day | 95 | 5~65% | true |
| rezervbase | 95 | 20~60% | true |
| factory4_night | 75 | 20~65% | true |

서버가 "몇 초 지점에서 시작한 것으로 칠지"는 `RaidChanges.SimulatedRaidStartSeconds`에
담기고, `RaidTimeAdjustmentService.MakeAdjustmentsToMap`이 `adjustWaves`를 보고 웨이브를
손본다. PMC 웨이브가 전부 앞쪽에 몰려 있으니, 뒤늦게 끼어든 스캐브는 그것들을 통째로 놓친다.

**모드가 아니라 SPT 자체 동작이다.** 설치본의 다른 모드는 확인 결과 무관했다.

| 의심했던 것 | 확인 내용 |
| --- | --- |
| SVM | `Bots.EnableBots = false`, `PMC.EnablePMC = false`, `PMC.ChancesEnable = false` — 봇·PMC 항목이 전부 꺼져 있다 |
| MoreBotsServer | 설정이 `increaseBotCapAmount: 5` 뿐이다. 상한을 올릴 뿐 없애지 않는다 |
| `botTypeLimits` | 마크스맨만 제한한다 |
| 그 밖 | `user\mods` 전체에서 `customPmcWaves`·`removeExistingPmcWaves`·`scavRaidTimeSettings`를 건드리는 설정 파일이 하나도 없었다 |

## 이 모드가 하는 일

`LocationConfig`의 `ScavRaidTimeSettings.Maps` 중 **두 값만** 설정대로 덮어쓴다.
웨이브 자체나 레이드 시간 계산에는 손대지 않는다.

| 설정 | 뜻 | 바닐라 |
| --- | --- | --- |
| `ReducedChancePercent` | 스캐브 레이드의 시간을 깎을 확률(%) | 75~100 |
| `AdjustWaves` | 시간을 깎을 때 봇 웨이브도 같이 조정할지 | true |

`null`은 "건드리지 않는다"는 뜻이다.

### 두 가지 방향

**① 짧은 레이드를 유지하고 웨이브만 남긴다 (기본값)**

```jsonc
"Default": { "ReducedChancePercent": null, "AdjustWaves": false }
```

레이드는 짧아지되 웨이브 시각표가 원래대로 남아, 짧아진 길이 안에 드는 PMC 웨이브가
나온다. 스캐브 레이드의 짧은 성격이 유지된다. PMC 수는 ②보다 적다.

**② 시간을 안 깎는다**

```jsonc
"Default": { "ReducedChancePercent": 0, "AdjustWaves": null }
```

시간이 깎이지 않으므로 PMC 웨이브가 **전부** 산다. 가장 확실한 쪽이다.
대신 스캐브 레이드가 정규 레이드와 같은 길이가 된다. ①로 PMC 가 모자라면 이쪽으로 바꾼다.

### 맵별 예외

`Maps`에 적으면 `Default`보다 우선한다. 맵 id는 SPT가 쓰는 이름 그대로다.

```jsonc
"Maps": {
  "factory4_day":   { "ReducedChancePercent": 95, "AdjustWaves": true },
  "factory4_night": { "ReducedChancePercent": 75, "AdjustWaves": true }
}
```

## 적용 지점

`LocationConfig`는 서비스에 그대로 주입되는 단일 인스턴스다 —
`RaidTimeAdjustmentService`의 생성자가 받는 것과 같은 것이다.

```
RaidTimeAdjustmentService(ISptLogger, GlobalTable, LocationTable, RandomUtil,
                          WeightedRandomHelper, ProfileActivityService, LocationConfig)
```

여기서 고치면 그 서비스가 레이드마다 읽을 때 반영된다. 다른 모드가 같은 값을 건드릴 수
있으므로 `OnLoadOrder.PostLoad`에 둬서 늦게 쓴다.

## 설정을 확인하는 법

`ReportOnly`를 켜면 **바꾸지 않고 바꿀 것만** 기록한다. 다른 모드가 이 값을 건드려 뒀는지
먼저 보고 싶을 때 쓴다. `Debug`는 맵마다 바뀐 값을 한 줄씩 남긴다.

서버 로그에 이런 줄이 남는다.

```
[ScavRaidWaves] 맵 13개 중 바꾼 것 — 시간 확률 13개, 웨이브 조정 0개
```

## 설치

```powershell
dotnet build src/ScavRaidWaves -c Release
pwsh -File tools/install-scavraidwaves.ps1
```

제거는 `-Remove`. 서버 모드라 **SPT 서버를 다시 켜야** 적용된다.
`config.jsonc`는 이미 있으면 덮어쓰지 않는다.

## 확인한 것 (2026-10-30)

②(`ReducedChancePercent: 0`)로 커스텀에 스캐브로 들어가
**레이드가 정규 시간으로 돌고 PMC가 스폰되는 것을 확인했다.**
그 뒤 짧은 레이드를 유지하려고 기본값을 ①로 바꿨다 — **①은 아직 레이드에서 확인하지 않았다.**

서버 로그(`user\logs\spt`, 시각은 UTC = 한국 시간 −9시간):

```
23:32:24  ModLoader: 총 116 개의 모드 불러오는 중...
23:32:45  [ScavRaidWaves] 맵 13개 중 바꾼 것 — 시간 확률 13개, 웨이브 조정 0개
23:34:41  Loaded self-signed certificate            ← 시작 완료
23:52:52  [FollowerInsurance:RaidStart] serverId='bigmap.Savage …'
```

그날 로그에 `Failed to generate bot` 은 0건이었다.
모드 로드 자체의 테스트 출력은 `doc/server-tests/server-test-scavraidwaves.out.txt`
(오류 0건, 시작 136초).
