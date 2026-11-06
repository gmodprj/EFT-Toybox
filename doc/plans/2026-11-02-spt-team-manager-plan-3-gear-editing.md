# SPT Team Manager — 구현 계획 3: 장비 편집과 팀원 상세

> **에이전트 작업자에게:** 필수 하위 스킬 — `superpowers:subagent-driven-development`(권장) 또는 `superpowers:executing-plans`로 Task 단위로 구현한다. 단계는 체크박스(`- [ ]`)로 추적한다.

**Goal:** 프로필 팀원의 상세 화면을 열고, 그 팀원의 장비를 **자기 스태시만 출처로** 손으로 고칠 수 있게 하되, 프로필 두 개를 고치는 모든 쓰기가 끊겨도 아이템이 복제되지 않게 한다.

**Architecture:** 쓰기 안전장치를 먼저 세우고 그 위에 화면을 얹는다. 순서가 설계다 — 저장 계층 갱신 경로(Task 1) → 스태시 재검증 수리(Task 3) → 3단 전송 저널(Task 4)이 **편집의 전제 조건**이고, 화면(Task 5)과 편집(Task 6)은 그 뒤에 온다. 화면은 새로 만들지 않는다: PIT 가 이미 가진 팀원 상세·장비 편집 화면에 프로필 팀원을 태우고, 호출부마다 **STM 해석기를 PIT 보다 먼저** 묻는 계획 2 의 모양을 그대로 쓴다. 판단(어떤 아이템을 출처로 허용하는가, 저널을 어느 쪽으로 확정하는가, 재검증에서 무엇만 건너뛰는가)은 SPT 참조가 없는 순수 클래스로 빼서 xunit 으로 고정한다.

**Tech Stack:** C#. 서버 net10.0(`SPTarkov.Server.Core` 4.1.0 참조), 클라이언트 net472(BepInEx, HarmonyX), 테스트 net8.0 xunit 2.9.2.

**Spec:** `doc/specs/2026-10-31-spt-team-manager-design.md` — §6-3(화면), §6-4(장비 규칙), §6-8(3단 전송), §6-9(닫힌 쪽), §5-14(스태시 재검증 사고), §7-1 의 "스태시 재검증 수리를 1차 범위에 넣은 것". 실행자는 계획과 설계를 함께 읽는다. 계획 1·2 의 결과·제약·실측과 **열린 항목**은 `doc/STM-HANDOVER.md`, 세부는 `doc/WORKLOG.md` 의 STM 절들에 있다.

## Global Constraints

- 런타임 SPT 4.1.6. **서버 컴파일 참조는 4.1.0.0 고정**(`ValidateSptReferences` 를 끄지 않는다). 클라이언트는 이 설치본 기준 4.1.6.
- 서버 `net10.0`, 클라이언트 `net472`, 테스트 `net8.0` + xunit 2.9.2. `shared/*.cs` 는 **SPT 타입을 참조하지 않는다** — 파일마다 한 줄씩 서버·테스트 csproj 에 `<Compile Include>` 로 링크한다(와일드카드 금지). 테스트 프로젝트는 `EnableDefaultCompileItems=false` 라 **테스트 파일도 한 줄씩** 등록한다.
- 프로필 수정은 **`SaveServer` 의 인메모리 객체 + `SaveProfile` 만** 쓴다. 프로필 JSON 파일을 직접 쓰지 않는다(설계 §6-8).
- **`StmResolvedMember.Pmc` 는 `SaveServer` 가 들고 있는 살아 있는 객체다.** 읽기만 하거나 `cloner.Clone` 위에 선다. 프로필에 **쓰는** 코드는 Task 4 의 저널을 거치는 경로뿐이다.
- **리네임은 최소 침습.** `src/SptTeamManager/` 는 업스트림 PIT 의 서브트리 임포트다. 네임스페이스(`pitTeam`, `pitTeam.Server`)·클래스명·파일명·라우트 문자열을 바꾸지 않는다. **PIT 자신의 파일은 `private` → `internal` 승격과 호출부 분기만** 한다.
- **EFT 네이티브 경로를 새로 가로채지 않는다.** 이미 이 포크가 소유한 경로의 기존 콜백 안에서만 작업한다.
- **호출부는 STM 해석기를 먼저 묻는다.** PIT 를 먼저 물으면 용병과 프로필이 Aid 를 공유할 때 보이지 않는 용병이 선택된다(계획 2 에서 네 곳을 이 순서로 고쳤다).
- **읽을 수 없거나 판단이 서지 않으면 "사용 불가"로 처리한다. 추측해서 진행하지 않는다.**
- **공짜 생성은 없다.** 편집 화면에 넣을 수 있는 아이템은 그 팀원 스태시에 **실제로 있는** 것뿐이다. 플레이어 스태시에서 흘러드는 경로를 두지 않는다(설계 §6-4).
- **더하는 쪽은 이 계획도 쓰지 않는다.** 경험치·스킬·전적·전리품은 계획 4 다. 이 계획이 프로필에 쓰는 것은 **장비의 이동**뿐이고, 출처와 행선지가 같은 프로필이다.
- `downloads/`, `extracted/`, `backup/`, `.superpowers/` 와 빌드 산출물·`server/refs/` 는 커밋하지 않는다.
- **STM 문서는 `{정수}.json` 으로 이름 짓지 않는다.** 저장 계층이 파일명 모양으로 종류를 가른다(`TeammateDatabase.IsProfileDocument`).

## Review Focus

- **편집 중에 서버가 죽는다.** 양쪽 쓰기 사이에서 끊기면 아이템이 복제되거나 사라진다. 설계 §6-8 은 "빼는 쪽 먼저" 로 **사라지는 쪽으로 기울이라**고 못박았다. → Task 4 가 저널 상태 전이를 순수 정책으로 고정하고, 기동 시 미결 저널을 한쪽으로 확정한다. 확정이 애매하면 **멈추고 보고**한다(추측 복구 금지).
- **미결 저널이 남은 팀원을 또 편집한다.** 설계 §6-9: 미결이면 그 팀원의 편집을 막아야 한다. 막지 않으면 저널 둘이 같은 아이템을 가리킨다. → Task 4, 테스트 `미결_저널이_있으면_편집을_막는다()`.
- **규칙에 맞지 않는 아이템 하나가 전체를 죽인다.** 이미 일어난 사고다(`doc/fixes/2026-10-27-pit-stash-ammo.md` — 9x19 탄창에 든 5.56 탄 하나로 팀원 생성이 끊겼고, 그래서 "동료 4, 5 가 안 보였다"). 편집이 같은 스냅샷 경로를 쓴다. → Task 3, 테스트 `위반_항목_하나는_그것만_건너뛴다()`.
- **편집으로 장비를 전부 벗긴다.** 그러면 `SpawnGearPolicy` 가 "장비 없음 → 임시" 로 판정해, 손으로 비워 둔 팀원에게 **생성 장비가 입혀진다.** 설계 §6-4 는 "한 번 고치면 그 뒤로 임시 장비 대상이 아니다" 라고 했다. → Task 6 이 `HandEdited` 를 저장하고 `SpawnGearPolicy` 가 그것을 본다. 테스트 `손으로_고친_팀원은_비어_있어도_임시가_아니다()`.
- **현 세션 프로필·읽을 수 없는 프로필이 auto-join 목록에 든다.** 목록은 서버 기동 이후에도 바뀐다(런처에서 그 프로필로 로그인할 수 있다). 지금 로그인한 프로필이 목록에 들면 자기 자신을 분대에 넣는다. → Task 2, 테스트 `현_세션_프로필은_auto_join_목록에_없다()`.

---

### Task 1: 등록 정보를 갱신할 수 있게 한다 (저장 계층)

계획 1·2 는 등록을 `Add`·`Remove` 로만 다뤘다. 이 계획의 토글(auto-join)과 표시(`HandEdited`)는 **기존 등록의 한 필드를 바꾸는** 일이라 갱신 경로가 필요하다.

**Files:**
- Create: `src/SptTeamManager/shared/ProfileMemberUpdatePolicy.cs`
- Create: `src/SptTeamManager.Tests/ProfileMemberUpdatePolicyTests.cs`
- Modify: `src/SptTeamManager/server/Services/StmProfileMemberStore.cs` (갱신 메서드 추가)
- Modify: `src/SptTeamManager/server/Models/StmProfileMember.cs` (`handEdited` 추가)
- Modify: `src/SptTeamManager/server/Models/StmProfileMember.cs` 의 `StmRosterRow` (`autoJoin`·`handEdited` 를 와이어에 싣는다)
- Modify: 서버·테스트 csproj (각각 한 줄)

**Interfaces:**
- Consumes: 계획 1 의 `StmProfileMember(string ProfileId, string Tactic, bool AutoJoin)`.
- Produces: `StmProfileMember(string ProfileId, string Tactic, bool AutoJoin, bool HandEdited)` — **위치 기반 레코드에 파라미터를 뒤에 추가**한다(기존 문서는 `handEdited` 가 없으니 `false` 로 읽힌다. 모델 주석의 "읽어올 때 생성자 파라미터 바인딩에 의존한다" 를 깨지 않도록 **기존 세 파라미터의 이름·순서를 바꾸지 않는다**).
  `ProfileMemberUpdatePolicy.Apply(StmProfileMemberFields current, StmProfileMemberChange change) -> StmProfileMemberFields` — 순수. `StmProfileMemberStore.Update(MongoId sessionId, string profileId, StmProfileMemberChange change) -> bool`(등록이 없으면 false, 아무것도 쓰지 않는다).

- [ ] **Step 1: 실패하는 테스트를 쓴다**

```csharp
// ProfileMemberUpdatePolicyTests.cs — 순수 레코드 둘(SPT 타입 없음)
// record StmProfileMemberFields(string ProfileId, string Tactic, bool AutoJoin, bool HandEdited);
// record StmProfileMemberChange(bool? AutoJoin, bool? HandEdited);
[Fact] public void 널인_필드는_그대로_둔다()            // change(null, null) → current 와 같다
[Fact] public void 지정한_필드만_바꾼다()               // change(AutoJoin: true, null) → HandEdited 유지
[Fact] public void 손으로_고친_표시는_되돌리지_않는다()  // HandEdited=true 에 change(HandEdited: false) → true 유지
[Fact] public void 프로필id와_전술은_변경_대상이_아니다() // 결과의 ProfileId·Tactic 이 current 와 같다
```

`손으로_고친_표시는_되돌리지_않는다` 가 이 정책의 유일한 비자명 규칙이다: 단방향이다. 되돌릴 수 있으면 "편집했는데 임시 장비가 입혀진다" 가 다시 열린다(Review Focus 4번). 등록을 지우고 다시 넣는 것이 사용자의 되돌리는 길이다.

- [ ] **Step 2: 테스트가 실패하는지 확인한다** — Run: `dotnet test src/SptTeamManager.Tests/SptTeamManager.Tests.csproj` / Expected: FAIL (`ProfileMemberUpdatePolicy` 없음)
- [ ] **Step 3: `ProfileMemberUpdatePolicy.Apply` 를 구현한다.** 예외를 밖으로 던지지 않는다. `current` 가 null 이면 null 을 돌린다.
- [ ] **Step 4: `StmProfileMember` 에 `handEdited` 를 더하고 `StmRosterRow` 에 `autoJoin`·`handEdited` 를 싣는다.** 두 property name 을 `"autoJoin"`·`"handEdited"` 로 못박는다(저장·와이어 양쪽).
- [ ] **Step 5: `StmProfileMemberStore.Update` 를 구현한다.** 문서를 읽어 해당 `profileId` 를 찾고, 정책을 적용한 레코드로 바꿔 쓴다. 없으면 **아무것도 쓰지 않고** false. 기존 `Add`·`Remove` 와 같은 저장 경로를 쓴다.
- [ ] **Step 6: 테스트가 통과하는지 확인한다** — Expected: PASS (총 개수 = 이전 + 4)
- [ ] **Step 7: 커밋**

---

### Task 2: auto-join 이 프로필 팀원에게 동작한다

인계 문서 열린 항목 1-c. 필요한 세 곳 중 **플레이어 구성은 2026-11-02 에 메웠고**(`StmProfileViewFactory`), 토글 저장과 목록 조회가 남았다. 그래서 메운 쪽은 지금 닿지 않는 코드다 — 이 Task 가 닿게 한다.

auto-join 의 뜻을 오해하지 말 것: **출격 화면**(`MatchMakerAcceptScreen.Show` → `SyntheticTeammateAutoJoinLoader.EnsureLoaded`)에서 토글이 켜진 팀원을 분대에 자동으로 넣는 기능이다. "지난 분대 복원" 이 아니다.

**Files:**
- Create: `src/SptTeamManager/shared/AutoJoinRosterPolicy.cs`
- Create: `src/SptTeamManager.Tests/AutoJoinRosterPolicyTests.cs`
- Modify: `src/SptTeamManager/server/Callbacks/FriendlyTeammateCallbacks.cs` (`SetAutoJoin`·`ListAutoJoin` 앞의 분기)
- Modify: `src/SptTeamManager/server/Services/StmProfileViewFactory.cs` 와 같은 폴더에 `StmAutoJoinService.cs` 를 Create (판단 호출과 저장을 STM 자기 파일에 둔다)
- Modify: 서버·테스트 csproj (각각 한 줄)

**Interfaces:**
- Consumes: Task 1 의 `StmProfileMemberStore.Update`, 계획 1 의 `ProfileCandidate`·`ProfileAvailability`, 계획 2 의 `StmProfileMemberResolver.TryResolve`.
- Produces: `AutoJoinRosterPolicy.SelectAccountIds(IEnumerable<StmProfileMemberFields>? members, IEnumerable<ProfileCandidate>? candidates) -> List<string>` — 순수. `StmAutoJoinService.TrySetAutoJoin(MongoId sessionId, string? accountId, bool enabled, out string? unavailableReason) -> bool`, `StmAutoJoinService.AppendAccountIds(MongoId sessionId, List<string> pitAccountIds) -> List<string>`.

- [ ] **Step 1: 실패하는 테스트를 쓴다**

```csharp
[Fact] public void AutoJoin_이_켜진_Ready_멤버의_aid_를_돌린다()
[Fact] public void AutoJoin_이_꺼지면_목록에_없다()
[Fact] public void 현_세션_프로필은_auto_join_목록에_없다()        // Availability=CurrentSession
[Fact] public void 읽을_수_없는_프로필은_목록에_없다()             // Unreadable / Empty
[Fact] public void 등록은_됐지만_카탈로그에_없으면_목록에_없다()     // Missing
[Fact] public void 같은_aid_가_둘이면_한_번만_담는다()
[Fact] public void 입력이_널이면_빈_목록이다()
```

허용 목록이지 제외 목록이 아니다 — `Ready` 가 아닌 모든 값(모르는 enum 값 포함)은 담지 않는다.

- [ ] **Step 2: 테스트가 실패하는지 확인한다** — Expected: FAIL (`AutoJoinRosterPolicy` 없음)
- [ ] **Step 3: `AutoJoinRosterPolicy.SelectAccountIds` 를 구현한다.** 예외를 밖으로 던지지 않는다.
- [ ] **Step 4: `StmAutoJoinService` 를 구현한다.** `TrySetAutoJoin` 은 해석기를 묻고(3값 계약) Task 1 의 `Update` 로 쓴다. `AppendAccountIds` 는 PIT 목록 **뒤에** 프로필 팀원 id 를 붙이고 중복을 거른다(PIT 쪽을 먼저 두어 기존 동작의 순서를 바꾸지 않는다).
- [ ] **Step 5: 두 호출부에 분기를 더한다.** `SetAutoJoin` 은 STM 이 처리했으면 PIT 를 부르지 않는다. `ListAutoJoin` 은 PIT 결과를 받아 `AppendAccountIds` 를 통과시킨다. 둘 다 **읽기 실패가 PIT 경로를 죽이지 않도록** 감싼다.
- [ ] **Step 6: 테스트가 통과하는지 확인한다** — Expected: PASS
- [ ] **Step 7: 커밋**

---

### Task 3: 규칙 위반 아이템 하나가 전체를 죽이지 않게 한다 (§5-14 수리)

편집의 전제 조건이다. 설계 §7-1 이 1차 범위에 넣은 이유 그대로: 편집이 §5-14 의 스냅샷 경로를 재사용한다.

**Files:**
- Create: `src/SptTeamManager/shared/StashRevalidationPolicy.cs`
- Create: `src/SptTeamManager.Tests/StashRevalidationPolicyTests.cs`
- Modify: `src/SptTeamManager/client/Patches/OtherPlayerProfileScreenPatch.cs` 의 `ApplyServerSavedPlayerStashSnapshot`
- Modify: 서버·테스트 csproj (각각 한 줄). 클라이언트 csproj 에도 `shared/` 링크가 필요하면 한 줄 더한다.

**Interfaces:**
- Produces: `StashRevalidationPolicy.Partition(IEnumerable<StashCandidate>? items) -> StashRevalidationResult` — 순수. `record StashCandidate(string? Id, bool Accepted, string? RejectionReason)`, `record StashRevalidationResult(List<string> KeptIds, List<StashRejection> Skipped)`. 규칙 판정 자체(EFT 필터)는 클라이언트가 하고, 이 정책은 **그 결과를 어떻게 처리하는가**만 정한다.

- [ ] **Step 1: 실패하는 테스트를 쓴다**

```csharp
[Fact] public void 전부_통과하면_전부_남는다()
[Fact] public void 위반_항목_하나는_그것만_건너뛴다()        // 나머지 KeptIds 가 보존된다
[Fact] public void 건너뛴_항목은_사유와_함께_보고된다()
[Fact] public void 전부_위반이면_빈_목록이되_예외는_없다()
[Fact] public void id_가_없는_항목은_남기지_않는다()         // 닫힌 쪽
```

- [ ] **Step 2: 테스트가 실패하는지 확인한다** — Expected: FAIL
- [ ] **Step 3: `StashRevalidationPolicy.Partition` 을 구현한다.**
- [ ] **Step 4: `ApplyServerSavedPlayerStashSnapshot` 이 항목별로 판정해 정책에 넘기게 바꾼다.** 한 항목의 실패가 **전체를 중단시키지 않게** 하고, 건너뛴 항목은 **경고 한 줄씩** 로그에 남긴다(조용히 버리지 않는다 — 사용자가 "아이템이 사라졌다" 로 겪게 되는 자리다).
- [ ] **Step 5: 테스트가 통과하는지 확인한다** — Expected: PASS
- [ ] **Step 6: 클라이언트를 빌드한다** — Run: `dotnet build src/SptTeamManager/client/<csproj>` / Expected: 경고 0 오류 0
- [ ] **Step 7: 커밋**

---

### Task 4: 3단 전송 저널 (§6-8)

프로필에 쓰는 모든 조작이 이 저널을 지난다. 이 계획에서 쓰는 것은 장비 이동뿐이지만, 계획 4(전리품·적립)가 같은 저널을 쓴다.

**Files:**
- Create: `src/SptTeamManager/shared/TransferJournalPolicy.cs`
- Create: `src/SptTeamManager.Tests/TransferJournalPolicyTests.cs`
- Create: `src/SptTeamManager/server/Services/StmTransferJournal.cs`
- Create: `src/SptTeamManager/server/Models/StmTransferJournalEntry.cs`
- Modify: `src/SptTeamManager/server/PitFireTeamServerPostLoad.cs` 또는 그에 준하는 기동 지점 (미결 저널 처리)
- Modify: 서버·테스트 csproj (각각 한 줄)

**Interfaces:**
- Consumes: 계획 2 의 `StmRaidReceiptStore` 의 **저장 관례**(파일명 모양 제약 포함)와 `TryReserve` 의 "먼저 예약하고 그 다음 쓴다" 순서.
- Produces: `enum TransferStage { Prepared, Committed, Verified }`, `TransferJournalPolicy.DecideRecovery(TransferStage stage, bool sourceStillHasItems, bool destinationHasItems) -> RecoveryDecision`(`RollForward`·`RollBack`·`StopAndReport`), `StmTransferJournal.Prepare(...) -> string journalId`, `.MarkCommitted(string)`, `.Verify(string) -> bool`, `.HasOpenEntry(MongoId sessionId, string profileId) -> bool`.

- [ ] **Step 1: 실패하는 테스트를 쓴다**

```csharp
[Fact] public void 준비에서_끊기면_되돌린다()                       // Prepared → RollBack (실물은 안 움직였다)
[Fact] public void 커밋에서_양쪽이_다_가지면_멈추고_보고한다()        // 복제 상태는 추측 복구 금지
[Fact] public void 커밋에서_출처만_비었으면_앞으로_확정한다()
[Fact] public void 확인까지_갔으면_할_일이_없다()
[Fact] public void 모르는_단계는_멈추고_보고한다()                   // 닫힌 쪽
```

`커밋에서_양쪽이_다_가지면_멈추고_보고한다` 가 §6-8 의 핵심이다. 복제는 조용히 고쳐도 되는 상태가 아니다 — 어느 쪽이 진짜인지 코드가 알 수 없다.

- [ ] **Step 2: 테스트가 실패하는지 확인한다** — Expected: FAIL
- [ ] **Step 3: `TransferJournalPolicy.DecideRecovery` 를 구현한다.**
- [ ] **Step 4: `StmTransferJournal` 을 구현한다.** 3단: 준비(실물 안 움직임) → 커밋(**빼는 쪽 먼저, 넣는 쪽 나중**) → 확인(`SaveProfile` 뒤 **디스크에서 다시 읽어** 대조하고 저널을 닫는다). 저장 계층은 계획 1·2 의 것을 쓰고 파일명 제약을 지킨다.
- [ ] **Step 5: 기동 시 미결 저널을 처리한다.** 정책이 `StopAndReport` 를 주면 **그 팀원의 편집을 막고** 사용자에게 보일 경고를 남긴다(설계 §6-9).
- [ ] **Step 6: `HasOpenEntry` 로 편집 차단을 건다** — 테스트 `미결_저널이_있으면_편집을_막는다()` 는 Task 6 의 진입 판정에 붙는다(여기서는 조회만 제공).
- [ ] **Step 7: 테스트가 통과하는지 확인한다** — Expected: PASS
- [ ] **Step 8: 커밋**

---

### Task 5: 팀원 상세 화면에 프로필 팀원을 태운다 (§6-3 오른쪽)

새 화면을 만들지 않는다. PIT 의 상세 화면이 요구하는 응답은 이미 `StmProfileViewFactory` 가 만든다(2026-11-02). 남은 것은 상세 화면이 쓰는 **나머지 라우트들**이 프로필 팀원을 알아보게 하는 것이다.

**Files:**
- Modify: `src/SptTeamManager/server/Callbacks/FriendlyTeammateCallbacks.cs` (`GetProfileOptions` 등 상세 화면이 쓰는 라우트의 분기)
- Modify: `src/SptTeamManager/server/Callbacks/FriendlyTeammateSocialCallbacks.cs` (`MergeProfileView` — 인계 문서 1-b)
- Modify: `src/SptTeamManager/server/Services/StmProfileViewFactory.cs` (옵션 응답 추가)
- Create: `src/SptTeamManager.Tests/` 해당 없음 — **SPT 타입이라 단위 테스트가 안 된다.** 리뷰가 코드로 확인하고 Task 8 관문이 증명한다.

**Interfaces:**
- Consumes: `StmProfileViewFactory.TryBuildView`(3값 계약), Task 1 의 와이어 필드.
- Produces: `StmProfileViewFactory.TryBuildOptions(MongoId sessionId, string? accountId, out FriendlyTeammateProfileOptionsResponse? options, out string? unavailableReason) -> bool`.

- [ ] **Step 1: 상세 화면이 서버에 묻는 라우트를 전수 조사한다.** `OtherPlayerProfileScreenPatch*.cs` 의 라우트 상수가 출발점이다(`profile/options`·`suit`·`rename`·`default-equipment`·`repair-equipment`·`aggression`·`tactic`·`insurance`·`proficiency`). **조사 결과를 WORKLOG 에 표로 적는다** — 어느 것이 프로필 팀원에게 의미가 있고, 어느 것이 이 계획의 범위 밖인지.
- [ ] **Step 2: 1-b 를 판정한다.** `/client/profile/view` 가 프로필 팀원의 aid 로 무엇을 돌려주는지 **실측한다**(SPT 가 다른 런처 프로필을 돌려주는지는 추측 금지 사항으로 남아 있다). 결과를 WORKLOG 에 적고, 고칠 자리면 `TryBuildView` 를 그대로 부른다.
- [ ] **Step 3: 의미가 있는 라우트에 분기를 더한다.** 모양은 전부 같다 — STM 을 먼저 묻고, 3값 계약, 실패 시 PIT 경로가 그대로 흐른다.
- [ ] **Step 4: 프로필 팀원에게 **의미가 없는** 조작은 거부하고 사유를 돌린다.** `rename`(런처 프로필의 이름은 그 프로필의 것이다)·`suit`(같은 이유)가 후보다. 조용히 성공한 척하지 않는다.
- [ ] **Step 5: 빌드한다** — Expected: 경고 0 오류 0
- [ ] **Step 6: 커밋**

---

### Task 6: 장비 편집 — 출처는 본인 스태시뿐 (§6-4)

이 계획의 목적이고 가장 위험한 Task 다. 프로필에 **쓴다.**

**Files:**
- Create: `src/SptTeamManager/shared/ProfileGearEditPolicy.cs`
- Create: `src/SptTeamManager.Tests/ProfileGearEditPolicyTests.cs`
- Create: `src/SptTeamManager/server/Services/StmGearEditService.cs`
- Modify: `src/SptTeamManager/server/Callbacks/FriendlyTeammateCallbacks.cs` (편집 라우트 분기)
- Modify: `src/SptTeamManager/shared/SpawnGearPolicy.cs` (`HandEdited` 를 본다)
- Modify: `src/SptTeamManager.Tests/SpawnGearPolicyTests.cs`
- Modify: 서버·테스트 csproj

**Interfaces:**
- Consumes: Task 1 의 `Update`, Task 4 의 `StmTransferJournal`, 계획 2 의 `WornGearLossPolicy.CollectSubtreeIds`(장비 트리 판정의 기존 권위), `StmProfileMemberResolver`.
- Produces: `record GearEditRequest(string ProfileId, string ItemId, string TargetSlotId, string? TargetParentId)` — 순수 레코드, SPT 타입 없음. `ProfileGearEditPolicy.Validate(GearEditRequest request, IEnumerable<WornGearItem>? profileItems, string? stashRootId, string? equipmentRootId) -> GearEditVerdict`(`Allowed` / `Rejected(reason)`), `StmGearEditService.TryApply(MongoId sessionId, GearEditRequest request, out string? rejectionReason) -> bool`.

- [ ] **Step 1: 실패하는 테스트를 쓴다**

```csharp
// ProfileGearEditPolicyTests
[Fact] public void 그_프로필_스태시에_있는_아이템만_출처로_허용한다()
[Fact] public void 스태시_밖의_id_는_거부한다()                     // 플레이어 스태시·존재하지 않는 id
[Fact] public void 스태시_루트를_읽을_수_없으면_거부한다()            // 닫힌 쪽, 추측 금지
[Fact] public void 장비_루트를_읽을_수_없으면_거부한다()
[Fact] public void 목적_슬롯_어휘를_모르면_거부한다()                 // 허용 목록
[Fact] public void 탄약도_다른_아이템과_같은_규칙으로_허용한다()      // 열린 항목 4
// SpawnGearPolicyTests 에 추가
[Fact] public void 손으로_고친_팀원은_비어_있어도_임시가_아니다()
```

`탄약도_다른_아이템과_같은_규칙으로_허용한다` 가 **인계 문서 열린 항목 4** 를 닫는 자리다. 계획 2 의
결정으로 탄창 리필은 **팀원이 몸에 지닌 탄만** 쓴다 — 그러니 편집으로 장비를 채울 때 탄을 함께 넣을 수
있어야 하고, 넣지 않으면 그 팀원은 **빈 탄창으로** 레이드에 나간다(계획 2 에서 실제로 일어났던 결함).
탄약을 특별 취급하지 않는 것이 요구사항이다: 그 프로필 스태시에 있으면 넣을 수 있다.

- [ ] **Step 2: 테스트가 실패하는지 확인한다** — Expected: FAIL
- [ ] **Step 3: `ProfileGearEditPolicy.Validate` 를 구현한다.** 아이템 소속 판정은 `CollectSubtreeIds` 를 재사용한다(새 트리 걷기를 쓰지 않는다 — 계획 2 가 같은 판정의 권위로 세운 함수다).
- [ ] **Step 4: `SpawnGearPolicy` 가 `HandEdited` 를 보게 한다.** 손으로 고친 팀원은 장비가 비어도 `Temporary` 가 아니다.
- [ ] **Step 5: `StmGearEditService.TryApply` 를 구현한다.** 순서: 미결 저널 확인(있으면 거부) → 정책 검증 → **저널 준비** → `SaveServer` 객체 수정 + `SaveProfile`(빼는 쪽 먼저) → **확인**(디스크 재독 대조) → 저널 닫기 → Task 1 의 `Update` 로 `HandEdited = true`.
- [ ] **Step 6: 편집 라우트에 분기를 더한다.** STM 먼저, 3값 계약.
- [ ] **Step 7: 테스트가 통과하는지 확인한다** — Expected: PASS
- [ ] **Step 8: 커밋**

---

### Task 7: 제스처 명령 셋 (`FIRE`·`HOLD FIRE`·`TAKE COVER`)

인계 문서 열린 항목 3. 메뉴 18개 중 11개만 처리한다. 교전 통제 손잡이라 사용자가 실제로 쓰는 항목이다.

**Files:**
- Create: `src/SptTeamManager/shared/GestureCommandPolicy.cs`
- Create: `src/SptTeamManager.Tests/GestureCommandPolicyTests.cs`
- Modify: 명령을 해석하는 클라이언트 자리 (Step 1 의 조사가 정한다)
- Modify: 서버·테스트 csproj, 필요하면 클라이언트 csproj

**Interfaces:**
- Consumes: 계획 2 의 `FollowerAiProfilePolicy`(진영 → AI 역할·난이도)와 그 어휘 관례.
- Produces: `GestureCommandPolicy.Translate(string? menuCommand) -> GestureCommandEffect?`(모르는 명령은 null — 허용 목록).

- [ ] **Step 1: 대조표를 다시 확인한다.** WORKLOG 의 메뉴 18개 대조표에서 **미대응 7개**를 꺼내고, 그중 이 Task 가 다루는 셋을 정한다. 나머지 넷은 **범위 밖으로 명시**하고 BACKLOG 에 남긴다.
- [ ] **Step 2: 실패하는 테스트를 쓴다**

```csharp
[Fact] public void FIRE_는_교전_허가로_번역된다()
[Fact] public void HOLD_FIRE_는_교전_금지로_번역된다()
[Fact] public void TAKE_COVER_는_엄폐_지시로_번역된다()
[Fact] public void 모르는_명령은_널이다()
[Fact] public void 대소문자와_공백은_무시한다()
```

- [ ] **Step 3: 테스트가 실패하는지 확인한다** — Expected: FAIL
- [ ] **Step 4: `GestureCommandPolicy.Translate` 를 구현한다.**
- [ ] **Step 5: 클라이언트 해석부에 연결한다.** Harmony prefix 를 새로 더하면 **반드시 `bool __runOriginal` 을 받아 `false` 면 즉시 반환**한다(설계 §5-10).
- [ ] **Step 6: 테스트가 통과하는지 확인한다** — Expected: PASS
- [ ] **Step 7: 커밋**

---

### Task 8: 설치하고 인게임으로 확인한다

**Files:** 없음(확인만). 결과는 `doc/WORKLOG.md` 에 적는다.

> **사람만 할 수 있다.** 설치는 `tools/install-sptteammanager.ps1`(`-Remove` 후 재설치). **서버가 떠 있으면 DLL 을 잡고 있어 제거가 실패한다** — 먼저 내린다. 서버·게임을 다시 켜야 적용된다.

- [ ] **Step 1: 프로필과 모드 DB 를 백업한다**(`backup/<날짜>-before-*`). 편집은 프로필에 쓰므로 **이 Task 에서는 선택이 아니다.**
- [ ] **Step 2: 다음을 확인한다**

1. 명단에서 팀원의 **auto-join 토글이 켜지고 꺼지는지**, 끈 뒤에도 유지되는지.
2. 토글을 켠 프로필 팀원이 **출격 화면에서 분대에 자동으로 들어오는지**(열린 항목 1-c 가 닫히는 자리, 그리고 2026-11-02 의 네 번째 경로 수정이 **처음으로 닿는** 자리).
3. 상세 화면이 **그 프로필의** 닉네임·레벨·진영·전적으로 그려지는지.
4. 편집 화면에 **그 팀원 스태시의 아이템만** 보이는지. 플레이어 스태시 물건이 하나라도 보이면 **멈추고 보고한다**(§6-4 위반, 무한 생성기).
5. 편집해 입힌 장비로 레이드에 나가는지, **그 프로필로 로그인했을 때 같은 장비가 보이는지**(프로필 JSON 대조 — 아이템 `_id` 는 레이드를 거쳐도 유지된다).
6. 장비를 전부 벗기게 편집한 팀원이 **임시 장비를 받지 않는지**(Review Focus 4번).
6-b. 편집으로 **탄을 함께 넣은** 팀원이 레이드에서 **장전된 탄창으로** 싸우는지, 탄을 넣지 않은 팀원은
   빈 탄창인지(열린 항목 4 가 닫히는 자리). 빈 탄창이 **결함이 아니라 규칙**임을 로그로 알리는지.
7. 편집 중 서버를 강제로 내린 뒤 다시 켰을 때, 저널이 한쪽으로 확정되거나 **멈추고 보고하는지**. 아이템 총수가 늘지 않았는지 프로필 JSON 으로 센다.
8. 미결 저널이 남은 팀원의 편집이 **막히는지**.
9. 규칙에 맞지 않는 아이템이 섞인 스태시에서 편집 화면이 **열리는지**(§5-14 수리 확인). 건너뛴 항목이 로그에 남는지.
10. `FIRE`·`HOLD FIRE`·`TAKE COVER` 가 실제로 교전 행동을 바꾸는지.
11. **용병만 데려가는 기존 PIT 흐름이 그대로인지**(회귀 확인 — 설계 §4 의 실패 조건).

- [ ] **Step 3: 결과를 적는다.** 어긋나면 **여기서 멈추고 보고한다.** 테스트 요약줄의 "통과!" 를 믿지 말고 **총 개수**를 기대값과 맞춘다(중단된 실행도 통과로 찍힌다).

---

### Task 9: 문서

**Files:**
- Modify: `doc/WORKLOG.md`, `doc/STM-HANDOVER.md`, `src/SptTeamManager/README.md`, `doc/BACKLOG.md`
- Modify: `tools/gen-installed-mods.mjs` (STM 줄의 "아직 안 되는 것" 갱신 후 `node tools/gen-installed-mods.mjs` 재생성)

- [ ] **Step 1: 되는 것과 안 되는 것을 다시 가른다.** 이 계획으로 **되는 것**: auto-join, 팀원 상세, 장비 편집(본인 스태시 출처), 제스처 셋. **아직 안 되는 것**: 스태시 전송(§6-5), 경험치·스킬·전적·전리품 적립(계획 4), 실물 장비 팀원의 보험(§3).
- [ ] **Step 2: 열린 항목을 닫거나 갱신한다.** 1(닿지 않는 코드) → Task 8 관문 2번으로 닫힌다. 1-b → Task 5 Step 2 의 실측으로 판정된다. 1-c → Task 2 로 닫힌다. 3(제스처) → 다룬 셋을 지우고 남은 넷을 BACKLOG 로 옮긴다. 4(탄) → Task 6 과 관문 6-b 로 닫힌다.
- [ ] **Step 3: 커밋**

---

## 이 계획에 없는 것

- **팀원 스태시 전송**(설계 §6-5). 저널이 실전에서 검증된 뒤에 붙인다 — 이 계획의 Task 8 관문 7번이 그 검증이다.
- **레이드 후 적립**(계획 4): 경험치·스킬·전적, 전리품 배달, 소모·손상 반영, §6-7 의 추가 기능 셋(출격 전 손실 요약·레이드 후 리포트·전리품 행선지 규칙). 이 계획은 **장비의 이동**만 쓴다.
- **실물 장비 팀원의 보험**(설계 §3·§6-6). 프로필 둘 사이의 또 하나의 전송이다.
- **`PartySlotPolicy` 를 초대 경로에 거는 것.** 2026-11-02 실측으로 용도가 사라졌다 — 클라이언트가 총 5명(플레이어 + 팀원 4)에서 막고 "파티가 가득 찼습니다" 를 띄운다. 지우지 않되 호출부를 붙이지 않는다.
- **남은 제스처 넷.** Task 7 Step 1 이 고른 셋 밖은 BACKLOG 로 간다.
- **한글화.** 별 계획이다 — `doc/plans/2026-11-02-korean-localization-plan.md`. 결이 다르고(번역과 외부 모드 조사) 이 계획과 의존하지 않는다.
