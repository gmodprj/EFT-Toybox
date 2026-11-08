# 계획 6 A5 설계 — 팀원 레이드의 경험치·스킬을 그 프로필에 적립한다

작성 2026-11-08. [계획 6](../plans/2026-11-06-stm-plan6-accrual-and-stash-transfer.md) A부의 마지막
Task. A1~A4·A6 은 끝났고([인계](../HANDOVER-2026-11-07-2.md) §1) **A5 만 남아 있었다** — 요청 1-1 의
칸 목록에 없어서 일부러 뺀 것이고, 할지는 사용자가 정하기로 했던 그 항목이다. 하기로 정해졌다.

이 문서는 **설계**다. 코드는 한 줄도 쓰지 않았다.

## 1. 왜 — 무엇이 비어 있나

레이드가 끝나면 **그 팀원 쪽에서 아무것도 바뀌지 않는다**(사용자 관찰). A4 로 전적(레이드 수·생사·
사살·플레이시간·연속 기록)은 쌓이기 시작했지만 **레벨과 스킬은 그대로**다. 같이 열 판을 뛴 팀원이
여전히 레벨 1 이고, 그 프로필로 로그인하면 아무 일도 없던 것처럼 보인다.

재료는 **이미 서버에 도착해 있다.** 클라이언트가 레이드 끝에 팀원마다 보낸다
([client/Modules/BossPlayers.cs](../../src/SptTeamManager/client/Modules/BossPlayers.cs) 의
`/client/game/bot/followerprogress`):

| 필드 | 무엇 | 어떻게 만들어지나 |
|---|---|---|
| `BotExperienceSession` | 그 판의 경험치 | 봇의 `SessionCounters[ExpKill]` + **플레이어의 그 판 `Exp` × `GetLevelFactor(플레이어레벨, 봇레벨)`**(1~4배, 레벨 차 7 이상이면 양 끝) |
| `Skills[]` | 스킬마다 `Current`·`Progress`·`PointsEarnedDuringSession` | `pr.Skills.DisplayList` + 이 모드가 따로 계산한 이동 스킬 보정(`Strength` 더하기, 과적이면 `Endurance` 억제) |

지금은 이 두 필드를 **쓰지 않고 버린다.** 서버가 프로필 팀원 항목을 PIT 에게 넘기기 전에 걸러내고
([FriendlyTeammateMatchCallbacks.cs](../../src/SptTeamManager/server/Callbacks/FriendlyTeammateMatchCallbacks.cs)
의 `WithoutProfileMemberProgress`), 거른 자리에서 `KillCount`·`RaidSeconds` 만 꺼내 쓴다.

거른 이유는 주석에 적혀 있다 — PIT 의 `ApplyFollowerProgress` 는 **고용 팀원(합성 `BotBase`)** 용이고,
그것을 진짜 런처 프로필에 그대로 적용하면 조용히 망가진다. **그 판단은 지금도 유효하다.** 이 설계는
PIT 의 코드를 프로필에 쓰는 것이 아니라, **프로필에 안전한 쓰기를 따로 만드는** 것이다.

## 2. 재어 둔 사실 (추측하지 않은 것)

이 PC 의 프로필 9개와 `SPT_Data/database/globals.json` 에서 읽었다.

### 2-1. 레벨과 경험치는 지금 **전부 표와 일치한다**

가장 큰 두려움("레벨이 어긋나면 그 프로필로 로그인한 플레이가 망가진다")의 절반은 **이미 닫혔다.**
`config.exp.level.exp_table`(79단)로 계산한 레벨이 **아홉 프로필 모두 `Info.Level` 과 같다.**

| 프로필 | 적힌 Level | Exp | 표로 계산 |
|---|---|---|---|
| Nomad(본인) | 42 | 3,126,942 | 42 |
| Banshee | 15 | 174,005 | 15 |
| Hawk | 17 | 251,588 | 17 |
| Knox · Buck · Mace · Raptor · Talon | 69 | 19.93M~19.95M | 69 |
| Wraith | 1 | 898 | 1 |

즉 **표에서 다시 계산하는 것 자체가 레벨을 움직이지 않는다.** 위험은 "이미 어긋나 있다" 가 아니라
"우리가 더하는 양" 쪽에만 있다. (SPT 가 프로필을 만들 때 `profileHelper.GetExperience(level)` 로
둘을 같이 맞추기 때문이다 — [Onboarding.cs:65-66](../../src/SptTeamManager/server/Services/FriendlyTeammateService.Onboarding.cs).)

### 2-2. 한 판이 레벨을 얼마나 움직이나 — 팀원에 따라 극과 극이다

| 레벨 | 다음 한 칸 | 한 판 20,000 XP 면 |
|---|---|---|
| 1 → 2 | 1,000 | **한 판에 여러 레벨** |
| 15 → 16 | 30,278 | 66% |
| 42 → 43 | 231,681 | 8.6% |
| 69 → 70 | 3,000,000 | **0.7%** |

표의 끝은 **79** 다. 레벨 69 팀원 다섯은 체감상 안 오르고, **Wraith(레벨 1)는 한 판에 몇 레벨씩
오른다.** 설계가 다뤄야 하는 것은 후자다.

### 2-3. 스킬 `Progress` 의 자릿수는 0~5,100 이다

| 프로필 | Common 스킬 | 진행>0 | 가장 큰 값 |
|---|---|---|---|
| Nomad | 53 | 51 | `Endurance`·`Strength`·`Health`·`StressResistance` = **5100**(만렙) |
| Knox | 61 | 28 | 4000 대 (프로필을 만들 때 넣은 합성값) |
| Wraith | 53 | 15 | `Charisma` 329.8 |

한 레벨이 100 이고 51레벨이 끝이다(5,100). **이미 5100 인 스킬이 있다** — 거기에 더하면 넘친다.

### 2-4. PIT 가 고용 팀원에게 하는 것 (참고 구현, 그대로 쓰지 않는다)

`pit-upstream/main:server/Services/FriendlyTeammateService.cs`:

- 경험치: `Info.Experience += round(BotExperienceSession)` → `RecalculateTeammateLevel`(표를 누적하며 올린다)
- 스킬: `Progress = round(Current + Progress, 2)` — **절대값 덮어쓰기**다. 없으면 새로 만든다.

**스킬의 절대값 덮어쓰기를 프로필에 그대로 쓸 수 없는 이유**가 이 설계의 핵심 결정이다(§4-3).

## 3. 목표 / 하지 않을 것 / 실패로 보는 것

### 목표

1. 팀원으로 레이드를 돈 프로필의 **경험치가 늘고, 그에 맞게 레벨이 오른다.**
2. 그 판에 **실제로 올린 스킬 진행만** 그 프로필의 스킬에 더해진다.
3. 같은 레이드 보고가 두 번 와도 **한 번만** 쌓인다(A4 와 같은 영수증 규칙).
4. 그 프로필로 **로그인해 플레이해도 멀쩡하다** — 레벨·경험치·스킬이 서로 어긋나지 않는다.

### 하지 않을 것 (non-goals)

- **숙련(`Skills.Mastering`)** — 클라이언트가 보내지 않는다. 없는 자료를 지어내지 않는다.
- **스킬 포인트(`Skills.Points`)·상인 충성도·퀘스트 진행** — 레이드 보고에 없다.
- **스캐브 캐릭터** — 프로필 팀원은 PMC 로만 투입된다(대응표 §1 과 같은 전제).
- **되빼기** — 적립은 되돌릴 수 없다. 카운터처럼 레벨도 내리는 코드를 만들지 않는다.
- **고용 팀원 쪽 경로 변경** — PIT 의 `ApplyFollowerProgress` 는 한 줄도 건드리지 않는다.

### 실패로 보는 것 (anti-goals)

- 레벨이 **내려가는** 것. 어떤 경로로도 일어나서는 안 된다.
- 스킬이 **내려가거나** 51레벨(5,100)을 **넘는** 것.
- 레벨 1 팀원이 한 판에 **레벨 20** 이 되는 것 — 재료가 플레이어 XP × 최대 4배라서 가능하다(§4-2).
- `Info.Level` 과 `Info.Experience` 가 **서로 어긋난 채** 저장되는 것.
- 레이드 한 판이 **두 번** 쌓이는 것.

## 4. 설계

### 4-0. 어디에 붙나

A4 가 이미 만든 **2단계 중 2단계(수치)** 자리다. 경험치·스킬은 `KillCount`·`RaidSeconds` 와 **같은
라우트·같은 항목**으로 오므로 새 통로가 필요 없다.

```
/client/game/bot/followerprogress  (생사보다 193~249ms 늦게 온다 — 인계 §1)
  └ WithoutProfileMemberProgress 가 걸러낸 자리
      ├ tallyService.ApplyNumbers(…, KillCount, RaidSeconds)       ← 지금 있다
      └ tallyService.ApplyGrowth(…, BotExperienceSession, Skills)   ← A5 가 더한다
```

`StmRaidTallyBuffer` 도 그대로 쓴다(생사가 아직 안 왔으면 담아 두고, 1단계가 꺼내 쓴다). 담는 값에
경험치·스킬이 **추가**되고, 담아 둔 것을 꺼내 쓰는 쪽도 같이 넘긴다.

### 4-1. 영수증 — 열쇠 공간을 **셋째로** 가른다

`#tally`(1단계) · `#tally-nums`(2단계) 에 더해 **`#tally-xp`** 를 쓴다. 이유는 2단계가 그랬던 것과
같다 — 같은 접미사를 쓰면 "이미 쌓인 판" 으로 걸러져 영원히 안 쌓인다. 적립 순서는
**예약 → 쓰기 → 디스크 재독 확인**(A4 와 같은 셋).

경험치와 스킬은 **한 영수증으로** 묶는다(둘이 같은 항목으로 같이 오고, 하나만 쌓이는 중간 상태를
만들 이유가 없다).

### 4-2. 경험치와 레벨

```
earned = round(BotExperienceSession)
earned <= 0           → 아무것도 하지 않는다(영수증도 쓰지 않는다. 진짜 수치가 더 늦게 올 수 있다)
earned > PerRaidCap   → PerRaidCap 으로 깎고 Warning 을 남긴다
Info.Experience += earned
Info.Level = 표에서 다시 계산 (profileHelper 의 경로를 쓴다 — 게임이 쓰는 그 표다)
Info.Level 은 내려가지 않는다: 계산값 < 기존값이면 기존값을 두고 Error 를 남긴다
Info.Level 은 표의 끝(79)을 넘지 않는다
```

**한 판 상한(`PerRaidCap`)을 둔다.** 재료가 "플레이어의 그 판 XP × 최대 4배" 라서 레벨 1 팀원이 한
판에 수십 레벨 오를 수 있다(§2-2).

상한을 **"한 판에 레벨은 많아야 1 오른다"** 로 정한다. 즉 그 프로필의 **다음 레벨 문턱까지**만
받고 남는 것은 버린다(버린 양을 Warning 으로 남긴다).

```
nextThreshold = 표에서 (현재레벨 + 1) 도달 누적치
PerRaidCap    = max(0, nextThreshold - Info.Experience)
```

**왜 이 모양인가.** 처음에 "플레이어가 그 판에 번 XP" 를 상한으로 쓰려 했는데, **레이드 보고에 그
값이 없다**([FriendlyTeammateDeathEscapeEntry](../../src/SptTeamManager/server/Models/FriendlyTeammateDeathEscapeRequest.cs)
에도, `followerprogress` 항목에도 없다 — 클라이언트가 계산해 `BotExperienceSession` 안에 섞어 보낸다).
서버가 그것을 역산하려면 `GetLevelFactor` 를 다시 추측해야 하고, 플레이어 프로필의 세션 카운터를 읽는
길은 `/client/match/local/end` 와의 순서에 기댄다. **없는 자료에 기대지 않는 상한**을 골랐다 —
표만으로 계산되고, 막으려는 것(한 판에 수십 레벨)을 정확히 막는다. **이것은 내가 정한 기본값이다**(§6-1).

**사망한 판도 쌓는다.** PIT 가 고용 팀원에게 그렇게 하고, 전적(A4)도 사망 판을 쌓는다. 한쪽만
빼면 "레이드 수는 늘었는데 경험치는 안 늘었다" 가 되어 화면이 어긋난다. **내가 정한 것이다**(§6-2).

### 4-3. 스킬 — **더한다, 덮어쓰지 않는다**

PIT 는 `Progress = Current + Progress` 로 **절대값을 덮어쓴다.** 고용 팀원은 그 값의 유일한 출처가
봇이므로 맞지만, 프로필 팀원은 **그 프로필 자신이 올린 진행**을 이미 들고 있다(Nomad 는 5100 짜리가
넷이다). 봇이 들고 나간 값은 스폰 시점의 복사본이고 클램프·보정을 거쳐 돌아온다. 덮어쓰면
**그 프로필의 진행이 봇의 복사본으로 교체된다** — 조용한 손실이다.

그래서 **그 판에 올린 몫만** 더한다.

```
delta = PointsEarnedDuringSession      (§5 의 A5-1 이 이 전제를 실측으로 확인한다)
delta <= 0            → 그 스킬은 건너뛴다
알 수 없는 Id         → 버린다 (A2 의 허용 목록 규칙과 같다)
새 스킬이면           → Common 에 Progress = delta 로 만든다
있으면                → Progress = min(5100, Progress + delta)
Progress 는 내려가지 않는다
PointsEarnedDuringSession 필드는 프로필에 쓰지 않는다(세션 값이다 — 0 으로 둔다)
한 판 상한: 스킬 하나당 delta <= SkillPerRaidCap(기본 100 = 1레벨), 넘으면 깎고 Warning
```

`LastAccess` 는 0 으로 둔다(지금 프로필들이 그렇다).

### 4-4. 안전 규칙 (A4 에서 이미 값을 치른 것들)

1. **프로필이 저장 가능한지 먼저 본다** — `IsProfileInvalidOrUnloadable` 이면 적립도 영수증도 없다.
2. **쓴 뒤 디스크에서 다시 읽어 확인한다** — 어긋나면 Error. 되돌리지 않는다.
3. **되돌릴 수 없는 쓰기**이므로 한 판에 한 번(영수증)이고, 상한을 넘으면 깎되 **던지지 않는다**.
4. 저널(`StmTransferJournal`)은 **쓰지 않는다** — 프로필 **하나**를 고치는 조작이다(장비 편집과 같은
   이유로 `Prepare` 가 같은 프로필을 거절한다). 영수증이 멱등을 맡는다.

### 4-5. 로그

적립 한 줄에 **무엇이 얼마나** 들어갔는지 다 적는다(사람이 레벨 변화를 추적할 수 있어야 한다):

```
[STM] 성장 적립: 'Wraith'(프로필 …, 레이드 …) 경험치 +102 (898 → 1,000), 레벨 1 → 2, 스킬 6개 +312.4
[STM] 성장 적립: 한 판 상한(레벨 1)으로 깎았다 — 경험치 18,432 → 102, 버린 18,330 ('Wraith', 레이드 …)
```

## 5. Task 쪼개기

| Task | 무엇 | 끝났다고 보는 근거 |
|---|---|---|
| **A5-1** | **재는 것 먼저.** 한 판의 `followerprogress` 본문을 **그대로 로그로 남겨** `Current`·`Progress`·`PointsEarnedDuringSession` 이 각각 무엇인지 실측한다(A1 이 카운터 열쇠에 한 것과 같은 일) | 세 숫자의 뜻이 문서에 적힌다. **`delta = PointsEarnedDuringSession` 가정이 맞는지**가 이 Task 의 산출물이다. 틀리면 §4-3 을 고치고 나서 A5-2 로 간다 |
| **A5-2** | **판정(순수).** `RaidGrowthPolicy` — 경험치 상한·레벨 재계산(표를 받는다)·스킬 delta 클램프. 프로필을 읽지도 쓰지도 않는다 | 단위 시험: 레벨이 내려가지 않는다 / 79 를 넘지 않는다 / 5100 을 넘지 않는다 / 음수·NaN·알 수 없는 Id 를 버린다 / **레벨 1 팀원에게 큰 XP 가 와도 레벨이 2 까지만 오르고 남는 XP 는 버려진다** / 이미 79 면 아무것도 늘지 않는다 |
| **A5-3** | **서버 적립.** `StmRaidTallyService.ApplyGrowth` — `#tally-xp` 예약 → 쓰기 → 디스크 재독 확인. 버퍼에 경험치·스킬을 더한다 | 같은 보고를 두 번 보내도 한 번만 쌓인다(`tools/stm-call.mjs` 로 두 번 쏜다). 디스크 재독 대조 |
| **A5-4** | **인게임 관문.** 레벨이 낮은 팀원(Wraith)과 높은 팀원(Knox)을 **같은 판에** 데리고 나가 프로필을 직전 스냅샷과 대조한다 | 경험치·레벨·스킬이 기대대로 올랐고 **런처 화면과 프로필 파일이 같다**. 그다음 **그 프로필로 로그인해** 레벨·스킬이 멀쩡한지 본다(A5 의 진짜 관문) |

A5-1 을 먼저 두는 이유는 A1 과 같다 — **자릿수를 추측하면 스킬을 조용히 부풀린다.**

## 6. 사용자가 뒤집을 수 있는 내 결정 둘

둘 다 기본값을 정해 두었으므로 **이대로 진행할 수 있다.** 다르게 원하면 말해 주면 된다.

1. **한 판에 레벨은 많아야 1 오른다**(다음 문턱까지만 받고 남는 XP 는 버린다). PIT 의
   `GetLevelFactor` 는 레벨이 낮은 팀원에게 최대 4배를 준다 — 그 의도를 살리려면 상한을 빼면 되고,
   그 경우 레벨 1 팀원은 한 판에 레벨 10 을 넘을 수 있다. 중간을 원하면 "한 판에 최대 N 레벨" 로
   바꾸면 된다(같은 계산에 N 만 들어간다). 남는 XP 를 **버리지 않고 이월**하는 쪽도 가능하지만,
   이월은 "다음 판에 또 한 레벨" 을 보장해 상한의 뜻을 약하게 만들므로 고르지 않았다.
2. **사망한 판도 쌓는다.** "죽으면 그 판 경험치는 없다" 로 하려면 생사 보고(1단계)가 들고 있는
   `escaped` 를 2단계로 넘겨 거르면 된다 — 통로는 이미 있다(`StmRaidTallyBuffer`).

## 7. 빌더에게 남기는 것

- `RaidGrowthPolicy` 의 파일 자리와 이름은 `shared/` 의 기존 관례를 따른다(`RaidTallyPolicy` 옆).
- 경험치 표를 정책에 **주입**한다(정책이 SPT 를 참조하지 않는다 — `RaidTallyPolicy` 와 같은 결).
  서비스 쪽에서 `profileHelper`/`globals` 를 읽어 넘긴다.
- `PerRaidCap` 은 상수가 아니라 **표와 현재 경험치에서 계산**된다(§4-2). `SkillPerRaidCap`(100)만
  상수다. 둘 다 설정 파일로 빼지 않는다(이 모드의 설정은 F12 메뉴 쪽이고, 적립은 사용자가 만질
  값이 아니다).
- 로그 문구는 §4-5 를 그대로 쓴다. 적립 한 줄에 전후 값을 다 적는다.
- 스킬 Id 는 요청에서 **문자열로도 숫자로도** 올 수 있다(`JsonElement`). PIT 의 `TryParseSkillType`
  이 두 모양을 다 받는다 — 같은 규칙을 쓰고, 정의 밖 값은 버린다.
