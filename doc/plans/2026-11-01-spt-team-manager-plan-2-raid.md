# SPT Team Manager — 구현 계획 2: 레이드 투입

> **에이전트 작업자에게:** 필수 하위 스킬 — `superpowers:subagent-driven-development`(권장) 또는 `superpowers:executing-plans`로 Task 단위로 구현한다. 단계는 체크박스(`- [ ]`)로 추적한다.

**Goal:** 등록한 프로필 팀원을 파티에 합류시켜 **자기 장비를 입고** 레이드에 데려가고, 죽으면 그 프로필에서 착용 장비를 실제로 잃게 한다.

**Architecture:** 핵심 사실 하나로 모양이 정해진다 — **`PmcData`는 `BotBase`를 상속한다.** 그러니 프로필의 PMC 캐릭터가 곧 스폰용 `BotBase`이고 변환이 필요 없다. 서버는 Aid로 온 요청을 등록된 프로필 팀원으로 되짚고(카탈로그), 그 PMC를 복제해 레이드에 내준다. 착용 무기가 없으면 SPT의 봇 생성기로 레벨에 맞는 **임시** 장비를 만들어 입힌다. 레이드가 끝나면 실물 장비로 나갔다 죽은 팀원만 그 프로필에서 착용 장비를 지운다. 판단 로직(어느 장비를 쓸 것인가, 레이드 결과에 무엇을 쓸 것인가)은 SPT 참조가 없는 순수 클래스로 빼서 xunit으로 검증한다.

**Tech Stack:** C#. 서버 net10.0(`SPTarkov.Server.Core` 4.1.0 참조), 클라이언트 net472(이 계획은 클라이언트를 거의 건드리지 않는다), 테스트 net8.0 xunit 2.9.2.

**Spec:** `doc/specs/2026-10-31-spt-team-manager-design.md` — 실행자는 계획과 설계를 함께 읽는다. 계획 1의 결과·제약·실측은 `doc/WORKLOG.md`의 "2026-11-01 — STM 기반 단계(계획 1)" 절에 있다.

## Global Constraints

- 런타임 SPT 4.1.6. **서버 컴파일 참조는 4.1.0.0 고정**(`ValidateSptReferences`를 끄지 않는다). 클라이언트는 이 설치본 기준 4.1.6.
- 서버 `net10.0`, 테스트 `net8.0` + xunit 2.9.2. `shared/*.cs`는 **SPT 타입을 참조하지 않는다** — 파일마다 한 줄씩 서버·테스트 csproj에 `<Compile Include>`로 링크한다(와일드카드 금지). 테스트 프로젝트는 `EnableDefaultCompileItems=false`라 **테스트 파일도 한 줄씩** 등록한다.
- 프로필 수정은 **`SaveServer`의 인메모리 객체 + `SaveProfile`만** 쓴다. 프로필 JSON 파일을 직접 쓰지 않는다(설계 §6-8).
- **지금 로그인한 세션의 프로필은 어떤 경로로도 팀원이 될 수 없다**(설계 §6-9).
- **읽을 수 없거나 판단이 서지 않으면 "사용 불가"로 처리한다. 추측해서 진행하지 않는다.**
- **리네임은 최소 침습.** `src/SptTeamManager/`는 업스트림 PIT의 서브트리 임포트다. 네임스페이스(`pitTeam`, `pitTeam.Server`)·클래스명·파일명·라우트 문자열을 바꾸지 않는다.
- **EFT 네이티브 경로를 새로 가로채지 않는다.** PIT가 이미 소유한 네 경로(`invite/send`, `followergenerate`, `followerdetails`, `followerprogress`)의 기존 콜백 안에서만 작업한다.
- 설계 §4: **PIT 기능이 조용히 망가지는 것은 실패 조건이다.** 용병만 데려가는 기존 흐름이 이 계획 뒤에도 똑같이 동작해야 한다. 이 계획이 기존 PIT 핸들러에 들여오는 모든 읽기는 보호되어 실패 시 **PIT 경로가 그대로 흐르도록** 해야 한다(계획 1에서 같은 결함이 두 번 나왔다).
- 설계 §4: **아이템이 복제되거나 사라지면 안 된다.** 임시 장비로 나간 팀원에게는 레이드 결과를 **쓰지 않는다** — 쓸 재산이 없다.
- `downloads/`, `extracted/`, `backup/`, `.superpowers/`와 빌드 산출물·`server/refs/`는 커밋하지 않는다.
- **STM 문서는 `{정수}.json`으로 이름 짓지 않는다**(`{aid}-settings.json`·`{aid}-equipment.json`·`recruit-requests.json`도 피한다). 저장 계층이 파일명 모양으로 종류를 가른다.

## Review Focus

- **Aid 충돌.** 용병과 등록된 프로필 팀원이 같은 Aid를 가지면, 친구 목록은 프로필을 남기고 용병을 밀어내는데(계획 1의 `FriendListMergePolicy`) PIT의 해석은 용병 DB를 먼저 본다. 목록과 해석이 어긋나 **보이지 않는 용병이 초대된다.** → Task 1이 "등록된 `Ready` Aid는 프로필 팀원"을 테스트로 고정하고, **호출 순서**(해석기를 PIT보다 먼저 묻는다)는 Task 4·5의 리뷰가 확인한다.
- **무기 없는 프로필.** `HasProperRaidKit`이 무기 슬롯을 요구하므로, 장비를 벗어 둔 프로필은 "proper kit 없음"으로 거부된다. 거부가 아니라 **임시 장비로 떨어져야** 한다. → Task 3, 테스트 `무기_슬롯이_비면_장비가_있어도_Temporary_다()`.
- **`Info.Settings.Role`이 비어 있는 실제 프로필.** 클라이언트가 `Profile.Info.Settings.Role`을 역참조한다(`BotFollowerPlayer`, `BossPlayers`). 생성된 봇과 달리 사람이 쓰던 PMC는 그 값이 없을 수 있고, 그러면 레이드에서 NRE가 난다. → Task 3의 공장. **SPT 타입이라 단위 테스트가 안 된다** — 리뷰가 코드로 확인하고, Task 9 관문 2번(스폰)이 실제로 증명한다.
- **레이드 종료 보고가 중복 도착한다.** 같은 레이드의 사망을 두 번 처리하면 이미 지운 장비를 또 지우거나 영수증이 어긋난다. 멱등 키는 `레이드 ID + 프로필 ID`다(설계 §6-8). → Task 6, 테스트 `이미_적용된_키면_죽었어도_아무것도_하지_않는다()`; Task 9 관문 8번이 실물로 확인.
- **임시 장비로 나간 팀원의 사망.** 그 프로필은 잃을 것이 없다. 실수로 착용 장비를 지우면 **레이드에 가져가지도 않은 재산**을 삭제하게 된다. → Task 6, 테스트 `임시_장비였으면_죽어도_아무것도_하지_않는다()`; Task 9 관문 6번.

---

### Task 1: Aid ↔ 프로필 팀원 되짚기 (순수 정책)

**Files:**
- Create: `src/SptTeamManager/shared/ProfileMemberLookup.cs`
- Create: `src/SptTeamManager.Tests/ProfileMemberLookupTests.cs`
- Modify: `src/SptTeamManager/server/pitFireTeam.Server.csproj`, `src/SptTeamManager.Tests/SptTeamManager.Tests.csproj` (각각 두 줄 추가)

**Interfaces:**
- Consumes: 계획 1의 `ProfileCandidate(string ProfileId, int Aid, string Nickname, int Level, string Side, ProfileAvailability Availability)`, `enum ProfileAvailability { Ready, Empty, Unreadable, CurrentSession }`.
- Produces:
  - `enum MemberLookupResult { NotAMember, Ready, NotAvailable }`
  - `record MemberLookup(MemberLookupResult Result, string? ProfileId, ProfileAvailability? Availability)`
  - `static MemberLookup ProfileMemberLookup.ByAid(int aid, IReadOnlyList<string> registeredProfileIds, IReadOnlyList<ProfileCandidate> candidates)`

`aid <= 0`은 `NotAMember`다. 등록돼 있으나 후보가 `Ready`가 아니면 `NotAvailable`이고, 그때 `Availability`를 함께 돌려주어 호출자가 사유를 말할 수 있게 한다.

- [ ] **Step 1: 실패하는 테스트를 쓴다**

```csharp
[Fact] void 등록되고_Ready_인_Aid_는_ProfileId_를_돌려준다()
[Fact] void 등록되지_않은_Aid_는_NotAMember_다()
[Fact] void 등록됐으나_Empty_인_후보는_NotAvailable_이고_사유가_담긴다()
[Fact] void Aid_가_0_이하면_NotAMember_다()
[Fact] void 같은_Aid_후보가_둘이면_Ready_인_쪽을_고른다()
```

- [ ] **Step 2: 테스트가 실패하는지 확인한다** — Run: `dotnet test src/SptTeamManager.Tests/SptTeamManager.Tests.csproj` / Expected: FAIL (`ProfileMemberLookup` 없음)
- [ ] **Step 3: `ProfileMemberLookup.ByAid`를 구현한다.** 예외를 밖으로 던지지 않는다. 입력이 null이면 `NotAMember`.
- [ ] **Step 4: 테스트가 통과하는지 확인한다** — Expected: PASS
- [ ] **Step 5: 커밋**

---

### Task 2: 서버 해석기 — Aid 로 프로필 팀원과 그 PMC 를 찾는다

**Files:**
- Create: `src/SptTeamManager/server/Services/StmProfileMemberResolver.cs`
- Modify: `src/SptTeamManager/server/pitFireTeam.Server.csproj` (필요 시)

**Interfaces:**
- Consumes: Task 1의 `ProfileMemberLookup.ByAid`; 계획 1의 `StmProfileCatalog.GetCandidates(string currentSessionId)`, `StmProfileMemberStore.List(MongoId sessionId)`.
- Produces:
  - `record StmResolvedMember(string ProfileId, int Aid, string Nickname, int Level, string Side, PmcData Pmc)`
  - `bool StmProfileMemberResolver.TryResolve(MongoId sessionId, string? accountId, out StmResolvedMember? member, out string? unavailableReason)`
  - `[Injectable(InjectionType.Singleton)]`

`accountId`는 **10진 Aid 문자열**로 온다(`MatchGroupInviteSendRequest.To`, `FriendlyTeammateFollowerGenerateRequest.MemberId` 모두). `int.TryParse`에 실패하면 `false`에 `unavailableReason = null`(= 프로필 팀원이 아님).

`PmcData`는 `SaveServer.GetProfile(new MongoId(profileId))`의 `CharacterData.PmcData`에서 얻는다. 그것이 null이면 `false`에 사유를 담는다(§6-9: 읽을 수 없으면 사용 불가).

> **Aid 충돌(Review Focus).** 등록된 `Ready` 프로필 팀원의 Aid라면 **프로필 팀원이 이긴다.** 계획 1의 `FriendListMergePolicy`가 친구 목록에서 같은 우선순위(프로필 > 용병)를 쓰므로, 해석이 반대로 가면 목록에 없는 용병이 초대된다. 호출부(Task 4·5)는 이 해석기를 **먼저** 묻고, 아니면 PIT의 기존 경로로 흘려보낸다.

- [ ] **Step 1: 해석기를 만든다.** 카탈로그와 등록 목록을 **호출할 때마다 새로 읽는다**(캐시 금지 — 설계 §4 "유령 재산").
- [ ] **Step 2: 읽기를 보호한다.** 저장소·카탈로그·`SaveServer` 읽기와 `new MongoId(...)` 변환을 try/catch로 감싸고, 실패하면 오류를 남기고 `false`를 돌려준다. **호출부가 PIT 경로를 그대로 타도록** 하는 것이 목적이다.
- [ ] **Step 3: 서버가 빌드되는지 확인한다** — Run: `dotnet build src/SptTeamManager/server/pitFireTeam.Server.csproj -c Debug` / Expected: 경고 0, 오류 0
- [ ] **Step 4: 커밋**

---

### Task 3: 스폰 프로필 만들기 — 실물이냐 임시냐 (순수 정책 + 공장)

**Files:**
- Create: `src/SptTeamManager/shared/SpawnGearPolicy.cs`
- Create: `src/SptTeamManager.Tests/SpawnGearPolicyTests.cs`
- Create: `src/SptTeamManager/server/Services/StmSpawnProfileFactory.cs`
- Modify: 두 csproj (각각 두 줄)

**Interfaces:**
- Consumes: Task 2의 `StmResolvedMember`.
- Produces:
  - `enum SpawnGearKind { Real, Temporary }`
  - `record GearDecision(SpawnGearKind Kind, string Reason)`
  - `static GearDecision SpawnGearPolicy.Decide(bool hasAnyEquipment, bool hasWeaponInRequiredSlot)`
  - `BotBase StmSpawnProfileFactory.Build(MongoId sessionId, StmResolvedMember member, out SpawnGearKind kind)`
  - `SpawnGearKind? StmSpawnProfileFactory.LastGearKind(MongoId sessionId, string profileId)`
  - `[Injectable(InjectionType.Singleton)]`

**어느 장비로 내보냈는지 기억한다.** Task 8이 "실물로 나갔다 죽었는가"를 알아야 하는데, 레이드 종료 보고에는 그 정보가 없다. `Build`가 `(sessionId, profileId) → SpawnGearKind`를 **메모리에** 적어 두고 `LastGearKind`로 내준다. 프로필에서 파생된 데이터가 아니라 "우리가 무엇을 했는가"이므로 캐시 금지 규칙(설계 §4 유령 재산)에 걸리지 않는다. 서버가 스폰과 레이드 종료 사이에 재시작되면 기록이 사라지는데, 그때는 **`null`이고 Task 6의 정책이 "아무것도 하지 않음"으로 떨어진다** — 확신 없이 장비를 지우지 않는 쪽이다(설계 §6-9).

정책: 착용 장비가 전혀 없거나 **필수 무기 슬롯에 무기가 없으면** `Temporary`, 아니면 `Real`. `Reason`은 사람이 읽을 한 줄이고 로그·메시지에 쓴다.

공장은 `Real`이면 `cloner.Clone(member.Pmc)`를, `Temporary`면 PIT의 `GenerateTeammateBot` 경로와 **같은 방식**으로 SPT의 `botGenerator.PrepareAndGenerateBot(sessionId, details)`를 부른다. `BotGenerationDetails`는 `IsPmc=true`, `Side=member.Side`, `Role=pmcUSEC|pmcBEAR`, `PlayerLevel=member.Level`, `BotCountToGenerate=1`, `BotDifficulty="hard"`, `Location="factory4_day"`, `LocationSpecificPmcLevelOverride`는 `member.Level`로 고정(Min=Max). 생성된 봇에서 **인벤토리만** 가져와 신원(`Id`·`Aid`·`Info`·`Customization`)은 실제 프로필의 것을 쓴다 — 플레이어가 명단에서 본 그 사람이 나가야 한다.

어느 쪽이든 `Id`는 프로필의 MongoId, `Aid`는 프로필의 Aid다.

> **`Info.Settings.Role`(Review Focus).** 사람이 쓰던 PMC는 `Info.Settings`가 없거나 `Role`이 비어 있을 수 있고, 클라이언트가 그것을 역참조한다. 공장은 비어 있으면 진영에 맞는 PMC 역할로 **채워 넣는다.**

> **PIT의 용병 전용 손질을 적용하지 않는다.** `EnsureFollowerHasSecureContainerSupplies`는 보안 컨테이너 **내용물을 비우고** 보급을 넣으며, `EnsureFollowerHasScabbardKnife`와 탄창 리필도 실제 캐릭터의 장비를 고쳐 쓴다. `Real`에는 이것들을 쓰지 않는다. 죽지 않기 위해 필요한 최소(`EnsureFollowerHasPockets`, 역할 채우기, 체력 배수)만 적용한다. `Temporary`는 생성물이므로 PIT의 기존 손질을 그대로 써도 된다.

- [ ] **Step 1: 실패하는 테스트를 쓴다**

```csharp
[Fact] void 착용_장비가_없으면_Temporary_다()
[Fact] void 무기_슬롯이_비면_장비가_있어도_Temporary_다()
[Fact] void 장비와_무기가_있으면_Real_이다()
[Fact] void 어느_쪽이든_사람이_읽을_사유가_담긴다()
```

- [ ] **Step 2: 테스트가 실패하는지 확인한다** — Expected: FAIL
- [ ] **Step 3: `SpawnGearPolicy.Decide`를 구현한다**
- [ ] **Step 4: 테스트가 통과하는지 확인한다** — Expected: PASS
- [ ] **Step 5: `StmSpawnProfileFactory.Build`를 구현한다**(위 제약대로)
- [ ] **Step 6: 서버 빌드와 전체 테스트를 확인한다** — Expected: 경고 0 오류 0 / 전부 통과
- [ ] **Step 7: 커밋**

---

### Task 4: 초대하면 파티에 합류한다

**Files:**
- Modify: `src/SptTeamManager/server/Callbacks/FriendlyTeammateMatchCallbacks.cs` (`SendGroupInvite`)

**Interfaces:**
- Consumes: Task 2의 `StmProfileMemberResolver.TryResolve`, Task 3의 `StmSpawnProfileFactory.Build`.

PIT의 용병 합류는 **파티 상태를 서버에 두지 않는다** — 초대 1초 뒤 `FriendlyGroupMatchInviteAccept { EventType, EventIdentifier, Id, Aid, Info, VisualRepresentation, IsReady = true }`를 `notificationSendHelper.SendMessageAsync`로 한 번 밀면 클라이언트가 파티원으로 그린다. 프로필 팀원도 **같은 알림**을 쓴다.

현재 그 자리에 **죽은 블록**이 있다(계획 1이 `ProfileId`와 Aid를 비교해 영영 참이 되지 않는다). 그것을 이 Task가 대체한다.

- [ ] **Step 1: 해석을 끼운다.** PIT의 `TryGetRaidGroupCharacter`가 아무것도 찾지 못했을 때(`teammate == null`) Task 2의 해석기를 묻는다. 프로필 팀원이 아니면 **기존 반환을 그대로** 한다.
- [ ] **Step 2: 수락 알림을 보낸다.** Task 3의 공장으로 스폰 프로필을 만들고, 그 `Id`·`Aid`·`Info`·`VisualRepresentation`으로 PIT와 같은 모양의 `FriendlyGroupMatchInviteAccept`를 1초 지연 뒤 보낸다. PIT와 같이 `FriendlyModeRequestGate.RunIfCurrent(modeGeneration, ...)`로 감싼다.
- [ ] **Step 3: 사용 불가는 사유를 말한다.** 등록돼 있으나 `Ready`가 아니면 `mailSendService.SendSystemMessageToPlayer(sessionId, reason, null)`로 알린다(PIT가 kit 거부에 쓰는 것과 같은 경로).
- [ ] **Step 4: 죽은 블록과 그 주석을 지운다.**
- [ ] **Step 5: 용병 경로가 글자 그대로 그대로인지 확인한다.** 보고서에 before/after를 적는다.
- [ ] **Step 6: 서버 빌드** — Expected: 경고 0 오류 0
- [ ] **Step 7: 커밋**

---

### Task 5: 레이드에 스폰한다

**Files:**
- Modify: `src/SptTeamManager/server/Callbacks/FriendlyTeammateMatchCallbacks.cs` (`GenerateFollowerProfile`, `GetFollowerDetails`, `PersistFollowerProgress`)

**Interfaces:**
- Consumes: Task 2·3.

`followergenerate`는 `MemberId`(Aid 문자열)로 `BotBase[]`를 돌려준다. `followerdetails`는 팀원마다 `{Aid, Tactic, Aggression, SelfDetermination, Proficiency, Equipment, Voice, Head}`를 돌려주고 클라이언트가 스폰 id마다 찾아 쓴다 — **프로필 팀원이 그 목록에 없으면 클라이언트가 무엇을 하는지 확인되지 않았다.** 그러므로 넣는다.

- [ ] **Step 1: `GenerateFollowerProfile`에 프로필 팀원을 더한다.** PIT가 못 찾으면 해석기를 묻고, 찾으면 Task 3의 공장 결과를 PIT와 같은 모양(`BotBase[]`)으로 돌려준다.
- [ ] **Step 2: `GetFollowerDetails`에 프로필 팀원 줄을 더한다.** `Tactic`은 등록 문서의 값, `Voice`/`Head`는 프로필의 `Customization`에서. 나머지는 PIT의 기본값과 같은 값을 쓴다.
- [ ] **Step 3: `PersistFollowerProgress`가 프로필 팀원을 건드리지 않게 막는다.** PIT의 `ApplyFollowerProgress`는 경험치를 더하고 `RecalculateTeammateLevel`로 **레벨을 다시 계산한다.** 실제 캐릭터에 그것을 적용하면 그 프로필로 로그인했을 때 레벨이 어긋난다(설계 §4 "팀원 프로필이 조용히 망가진다"). 프로필 팀원의 Aid는 **건너뛰고**, 건너뛰었다는 것을 한 번만 로그에 남긴다. 경험치 적립은 계획 4의 몫이다.
- [ ] **Step 4: 세 경로의 새 읽기를 모두 보호한다.** 실패 시 PIT 경로가 그대로 흐른다.
- [ ] **Step 5: 서버 빌드** — Expected: 경고 0 오류 0
- [ ] **Step 6: 커밋**

---

### Task 6: 레이드 결과 — 죽으면 착용 장비를 잃는다 (순수 정책)

**Files:**
- Create: `src/SptTeamManager/shared/RaidOutcomePolicy.cs`
- Create: `src/SptTeamManager.Tests/RaidOutcomePolicyTests.cs`
- Modify: 두 csproj (각각 두 줄)

**Interfaces:**
- Produces:
  - `enum OutcomeAction { Nothing, StripWornGear }`
  - `record OutcomePlan(OutcomeAction Action, string Reason)`
  - `static OutcomePlan RaidOutcomePolicy.Decide(bool escaped, SpawnGearKind gearKind, bool alreadyApplied)`

규칙은 셋뿐이다. **이미 적용된 키면 `Nothing`**(멱등, 설계 §6-8). **임시 장비였으면 `Nothing`** — 그 프로필은 레이드에 재산을 내보내지 않았다. **실물 장비로 나갔다 탈출하지 못했으면 `StripWornGear`.**

- [ ] **Step 1: 실패하는 테스트를 쓴다**

```csharp
[Fact] void 실물_장비로_나갔다_죽으면_착용_장비를_지운다()
[Fact] void 탈출했으면_아무것도_하지_않는다()
[Fact] void 임시_장비였으면_죽어도_아무것도_하지_않는다()
[Fact] void 이미_적용된_키면_죽었어도_아무것도_하지_않는다()
[Fact] void 어느_결정이든_사유가_담긴다()
```

- [ ] **Step 2: 테스트가 실패하는지 확인한다** — Expected: FAIL
- [ ] **Step 3: `RaidOutcomePolicy.Decide`를 구현한다**
- [ ] **Step 4: 테스트가 통과하는지 확인한다** — Expected: PASS
- [ ] **Step 5: 커밋**

---

### Task 7: 영수증 — 같은 레이드를 두 번 처리하지 않는다

**Files:**
- Create: `src/SptTeamManager/server/Models/StmRaidReceipt.cs`
- Create: `src/SptTeamManager/server/Services/StmRaidReceiptStore.cs`

**Interfaces:**
- Produces:
  - `record StmRaidReceipt(string RaidId, string ProfileId, string Action, long AppliedAtUnixSeconds)`
  - `bool StmRaidReceiptStore.WasApplied(MongoId sessionId, string raidId, string profileId)`
  - `void StmRaidReceiptStore.Record(MongoId sessionId, StmRaidReceipt receipt)`
  - `[Injectable(InjectionType.Singleton)]`

멱등 키는 **`레이드 ID + 팀원 프로필 ID`**다(설계 §6-8). 계획 1의 `StmProfileMemberStore`와 같은 저장 계층(PIT의 암호화 LiteDB)을 쓰고, 문서 이름은 **`stm-raid-receipts.json`** — `{정수}.json` 모양을 피한다(Global Constraints).

- [ ] **Step 1: 모델과 저장소를 만든다.** 읽기는 fail-closed: 문서를 읽을 수 없으면 **"적용됐다"로 친다**(중복 적용보다 미적용이 안전하다 — 장비 삭제는 되돌릴 수 없다).
- [ ] **Step 2: 서버 빌드** — Expected: 경고 0 오류 0
- [ ] **Step 3: 커밋**

---

### Task 8: 결과를 프로필에 쓴다

**Files:**
- Create: `src/SptTeamManager/server/Services/StmRaidOutcomeService.cs`
- Modify: `src/SptTeamManager/server/Callbacks/FriendlyTeammateMatchCallbacks.cs` (사망·탈출 보고 경로)

**Interfaces:**
- Consumes: Task 2·6·7, 그리고 Task 3의 `StmSpawnProfileFactory.LastGearKind(sessionId, profileId)`. **`null`이면 `SpawnGearKind.Temporary`로 친다** — 무엇을 입혀 보냈는지 모르면 지우지 않는다.
- Produces: `void StmRaidOutcomeService.Apply(MongoId sessionId, string raidId, string accountId, bool escaped)`

**쓰기 규칙(설계 §6-8).** `SaveServer`가 들고 있는 프로필 객체를 고치고 `SaveProfile`을 부른다. JSON을 직접 쓰지 않는다. 쓴 뒤 **다시 읽어 확인하고** 영수증을 남긴다. 확인에 실패하면 영수증을 남기지 않고 오류를 보고한다 — 다음 기동에서 다시 시도된다.

**지우는 범위.** 착용 장비(장착 슬롯의 아이템과 그 하위)만 지운다. **보안 컨테이너 내용물은 지키고**(설계 §6-4, 타르코프 규칙) 스태시는 건드리지 않는다.

**플레이어가 시체를 털어 나왔다면 그 아이템은 플레이어 것이다.** 되돌려 주려 하지 말라 — 돌려주려는 순간 한 자루가 양쪽에 생긴다(설계 §6-6 6번, §4). 우리는 **빼는 쪽만** 쓰고, 플레이어가 무엇을 가져왔는지는 보지도 않는다. 결과적으로 장비가 팀원에게서 플레이어에게 넘어간 모양이 되고 그것이 타르코프의 보통 규칙이다.

- [ ] **Step 1: 서비스를 만든다.** 해석 → 영수증 조회 → `RaidOutcomePolicy.Decide` → `StripWornGear`면 쓰기 → 다시 읽어 확인 → 영수증 기록.
- [ ] **Step 2: 사망·탈출 보고 경로에 건다.** PIT의 `PersistDeathEscapeOutcomes`는 프로필 팀원 Aid를 **조용히 건너뛰면서도** 요약의 탈출·손실 이름 목록에는 이미 넣는다. 프로필 팀원은 이 서비스가 처리하고, **요약 메시지는 사실과 맞아야 한다.**
- [ ] **Step 3: 보호한다.** 이 경로의 실패가 PIT 용병의 결과 처리를 막지 않아야 한다.
- [ ] **Step 4: 서버 빌드와 전체 테스트** — Expected: 경고 0 오류 0 / 전부 통과
- [ ] **Step 5: 커밋**

---

### Task 9: 설치하고 인게임으로 확인한다

**Files:** 없음(확인만). 결과는 `doc/WORKLOG.md`에 적는다.

> **사람만 할 수 있다.** 계획 1의 관문과 같은 성격이다. 설치는 `tools/install-sptteammanager.ps1`(`-Remove` 후 재설치). 서버·게임을 다시 켜야 적용된다.

- [ ] **Step 1: 프로필과 모드 DB를 백업한다**(계획 1이 `backup/2026-11-01-before-stm/`에 한 것과 같은 방식)
- [ ] **Step 2: 다음을 확인한다**

1. 프로필 팀원을 초대하면 **파티원이 된다**(계획 1에서 멈춰 있던 자리).
2. 레이드에 들어가면 **그 프로필의 장비를 입고** 스폰한다.
3. 장비를 벗겨 둔 프로필은 **임시 장비로** 나간다.
4. 용병과 프로필 팀원을 섞어 들어갈 수 있고, 용병 쪽 동작이 그대로다.
5. 프로필 팀원이 죽은 뒤 **그 프로필로 로그인하면 착용 장비가 없다.** 스태시와 보안 컨테이너는 그대로다.
6. 임시 장비로 나간 팀원이 죽어도 **그 프로필은 변화가 없다.**
7. 생환하면 장비가 그대로다.
8. 레이드를 두 번 끝내도(또는 보고가 중복돼도) 장비가 두 번 지워지지 않는다.
9. **파티에 팀원 넷째를 넣으려 할 때 무엇이 일어나는지 잰다**(계획 1에서 넘어온 관문 항목). 조용히 잘리는지, 창이 막는지, 사유가 뜨는지. 이 실측이 서버측 자리 관문이 필요한지를 정한다.

- [ ] **Step 3: 결과를 적는다.** 어긋나면 **여기서 멈추고 보고한다.**

---

### Task 10: 문서

**Files:**
- Modify: `doc/WORKLOG.md`, `src/SptTeamManager/README.md`
- Modify: `tools/gen-installed-mods.mjs` (STM 줄의 "아직 안 되는 것"을 갱신하고 `node tools/gen-installed-mods.mjs` 재생성)

- [ ] **Step 1: 되는 것과 안 되는 것을 다시 가른다.** 이 계획으로 **되는 것**: 파티 합류, 실물/임시 장비 스폰, 사망 시 착용 장비 상실. **아직 안 되는 것**: 장비 편집, 스태시 전송, 경험치·전적·전리품 적립(계획 3·4).
- [ ] **Step 2: 계획 1에서 이월한 열린 결정을 닫는다.** `doc/WORKLOG.md`의 "열린 결정 — 프로필 팀원 초대 응답"은 이 계획이 (a)·(b) 중 무엇으로 풀었는지 적고 닫는다.
- [ ] **Step 3: 커밋**

---

## 이 계획에 없는 것

- **장비 편집(계획 3).** 설계 §6-4의 "편집하면 실물이 된다", 본인 스태시만 출처로 쓰기, §6-3의 오른쪽 상세 화면, 그리고 **§5-14 스태시 재검증 수리** — 실측 결과 그 코드는 고용·로드아웃 편집·삭제 경로에서만 돌고 **스폰 경로에는 닿지 않는다.** 편집을 붙이는 계획의 전제 조건이다.
- **레이드 후 적립(계획 4).** 경험치·스킬·전적, 전리품 메일 배달, 소모·손상 반영, §6-7의 추가 기능 셋. 이 계획은 **잃는 것만** 쓰고 **얻는 것은 쓰지 않는다.**
- **팀원 스태시 전송**(설계 §6-5에서 이미 1차 제외).
- **`PartySlotPolicy`를 초대 경로에 거는 것.** 계획 1이 만들어 두고 호출부 없이 남긴 순수 정책이다. 합류가 알림 한 번이라는 것을 알게 됐어도 **인원을 셀 수는 여전히 없다** — STM은 자기가 보낸 수락 알림만 알고 PIT 용병이 몇 자리를 쓰는지 모르며, 수락·탈퇴·추방 라우트가 없어 목록이 낡는다. 네이티브 파티 창이 총 4명으로 묶으므로(설계 §6-2, 소유자 관측) 지금은 클라이언트가 상한을 강제한다. Task 9 관문이 넷째 자리에서 실제로 무엇이 일어나는지 재고, 그 실측이 서버측 관문이 필요한지를 결정한다.
- 계획 1에서 이월한 Minor들(친구 목록 Error 로그 중복 제거, `ParseStatus("Missing")` 테스트, `Interpret` 순서 고정, 설치 스크립트 `-PathType Leaf`). 이 계획이 그 파일들을 건드리면 함께 정리해도 좋으나 요구하지 않는다.
