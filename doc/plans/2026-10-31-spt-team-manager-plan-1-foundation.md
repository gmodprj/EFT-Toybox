# SPT Team Manager — 구현 계획 1: 기반

> **끝났다 (2026-11-01).** Task 9개 전부 구현하고 인게임 관문까지 통과했다. 결과는
> [WORKLOG](../WORKLOG.md) 의 "2026-11-01 — STM 기반 단계(계획 1) 결과와 사람 관문" 절에,
> 지금 상태는 [STM-HANDOVER](../STM-HANDOVER.md) 에 있다. 이 문서는 그때의 계획 기록이다.

> **에이전트 작업자에게:** 필수 하위 스킬 — `superpowers:subagent-driven-development`(권장) 또는 `superpowers:executing-plans`로 Task 단위로 구현한다. 단계는 체크박스(`- [ ]`)로 추적한다.

**Goal:** PIT Fireteam 1.0.0을 포크해 STM으로 이름을 갈고, SPT 런처 프로필을 팀원 명단에 올려 레이드에 데려간다(장비는 임시).

**Architecture:** 포크 단일 모드. 서버 쪽은 `SaveServer`에서 런처 프로필을 열거해 PIT가 이미 소유한 친구 목록 병합 경로에 프로필 팀원을 덧붙인다. 클라이언트 쪽은 PIT의 `MenuScreen.Show` 패치와 `SquadControlMenuUi`를 그대로 쓰되 명단에 프로필 팀원 줄을 추가한다. 판단 로직(어떤 프로필을 올릴 것인가, 중복을 어떻게 거를 것인가)은 SPT 참조가 필요 없는 순수 클래스로 빼서 xunit으로 검증한다.

**Tech Stack:** C#. 클라이언트 .NET Framework 4.7.2(BepInEx, HarmonyX), 서버 net10.0(`SPTarkov.Server.Core` 4.1.0 참조, LiteDB 5.0.21), 테스트 net8.0 xunit 2.9.2.

**Spec:** `doc/specs/2026-10-31-spt-team-manager-design.md` — 실행자는 계획과 설계를 함께 읽는다.

## Global Constraints

- 런타임 SPT 4.1.6, **컴파일 참조는 4.1.0.0 고정.** 서버 `csproj`의 `ValidateSptReferences` 타겟이 이를 강제한다. 끄지 않는다.
- 클라이언트 `TargetFramework`는 `net472`, 서버는 `net10.0`, 테스트는 `net8.0`.
- 필수 의존: BigBrain ≥ 1.5.0 (`xyz.drakia.bigbrain`), Waypoints - Expanded Navmesh ≥ 1.9.0.
- 원 저작권 고지(MIT)를 유지한다. 포크임을 README에 밝힌다.
- **리네임은 최소 침습.** 식별자(GUID·어셈블리명·설정 파일명·DB 디렉터리)만 바꾸고 네임스페이스·파일 구조·클래스명은 그대로 둔다. 업스트림 머지를 살리기 위함이다(설계 §7-1).
- 프로필 수정은 `SaveServer`의 인메모리 객체 + `SaveProfile`만 쓴다. **프로필 JSON 파일을 직접 쓰지 않는다.**
- 파티 총원은 네이티브 한계 **4명**. 플레이어를 뺀 팀원 자리는 **3**이고 프로필 팀원과 용병이 그 3자리를 함께 쓴다.
- 지금 로그인한 세션의 프로필은 어떤 경로로도 팀원이 될 수 없다.
- Harmony prefix를 새로 추가할 때는 반드시 `bool __runOriginal`을 받아 `false`면 즉시 반환한다(설계 §5-10).
- 읽을 수 없거나 판단이 서지 않으면 "사용 불가"로 처리한다. 추측해서 진행하지 않는다.
- `downloads/`, `extracted/`, `backup/`, `.superpowers/`는 커밋하지 않는다.

## Review Focus

- **Aid 충돌.** PIT가 용병에게 부여한 가짜 `Aid`가 실제 런처 프로필의 `Aid`와 겹치면, 친구 목록 병합의 중복 제거가 진짜 프로필을 삼켜 명단에서 사라진다. → Task 6.
- **서버 기동 후 만든 프로필.** 런처에서 프로필을 새로 만들어도 서버는 기동 시점 목록만 알 수 있다. 새 프로필이 영영 안 보이면 안 된다. → Task 4.
- **팀원으로 등록해 둔 프로필로 직접 로그인.** 그 프로필이 현재 세션이 되므로 자기 자신이 명단에 보이면 안 된다. 등록은 남아 있되 숨긴다. → Task 4.
- **빈 프로필과 손상된 프로필.** `characters.pmc.Info`가 없는 프로필(386바이트)과 역직렬화에 실패하는 프로필은 명단에 뜨되 선택할 수 없어야 하고, 하나가 실패해도 나머지 열거가 멈추면 안 된다. → Task 4.
- **팀원 자리 초과.** 프로필 팀원과 용병을 합쳐 3을 넘겨 파티에 넣으려 할 때, 조용히 잘리는 대신 거부하고 이유를 알려야 한다. → Task 8.

---

### Task 1: 포크 들여오기와 빌드 재현

**Files:**
- Create: `src/SptTeamManager/` (PIT 저장소 내용을 히스토리째 들여온 서브트리)
- Create: `doc/install-records/stm-fork-import.md`

**Interfaces:**
- Produces: 빌드되는 `pitFireTeam.dll`(클라이언트)과 `pitFireTeam.Server.dll`(서버). 이름 변경은 Task 2에서 한다.

- [x] **Step 1: 업스트림을 remote로 붙여 히스토리째 가져온다**

```bash
cd E:/Spt_Mods
git remote add pit-upstream https://github.com/pitAlex/SPT-PitFireTeam
git fetch pit-upstream
git subtree add --prefix=src/SptTeamManager pit-upstream f32a41b
```

기준 커밋은 `f32a41b`(1.0.0 머지). 다른 커밋을 쓰지 않는다.

- [x] **Step 2: 참조 DLL을 놓는다**

서버는 `server/refs/4.1.0/`에 `SPTarkov.Common.dll`·`SPTarkov.DI.dll`·`SPTarkov.Server.Core.dll`·`SemanticVersioning.dll`이, 클라이언트는 `client/libs4.1/spt4.1.0`에 SPT 공통/리플렉션 어셈블리가 있어야 한다. 설치본 `E:\Spt\SPT_Runtime`과 `E:\Spt\BepInEx`에서 복사한다. **이 DLL들은 커밋하지 않는다** — `.gitignore`에 경로를 추가한다.

→ **정정(2026-11-01, 실측).** 위 "설치본에서 복사한다"는 **틀렸다.** 이 기계의 설치본은 전부 SPT 4.1.6 이고 4.1.0.0 은 어디에도 없다(E:\ 와 사용자 폴더 전체, NuGet 캐시 포함 검색). 서버 참조 셋(`SPTarkov.Server.Core`·`SPTarkov.Common`·`SPTarkov.DI`)은 SPT 공식 NuGet 패키지의 `4.1.0`(`lib/net10.0/`)에서 꺼냈고, `SemanticVersioning.dll` 만 `E:\Spt\SPT_Runtime` 에서 복사했다(방법: `doc/install-records/stm-fork-import.md`). `.gitignore` 에 추가하라는 줄도 이미 돼 있었다 — 업스트림 `src/SptTeamManager/.gitignore:63-64` 가 `/server/refs/` 와 `*.dll` 을 제외한다. **클라이언트** 쪽 `client/libs4.1/spt4.1.0` 의 `spt-common.dll`·`spt-reflection.dll` 은 NuGet 에 없고(net472 BepInEx 어셈블리) 설치본에는 4.1.6 뿐이라 **구하지 못했다.** 그래서 이 계획을 실행하는 동안 클라이언트는 한 번도 컴파일되지 않았다.

→ **후속 정정(2026-11-01, 해결됨).** 소유자가 **이 설치본의 4.1.6 을 컴파일 기준으로** 택했다. 4.1.x 전체 호환은 공개 배포된 업스트림의 사정이고 이 개인 포크에는 값이 없다는 판단이다("낮춘다" 는 표현도 틀렸다 — 4.1.0 은 하한선이 아니라 지원 범위의 가장 오래된 끝이므로, 4.1.6 으로 옮기는 것은 기준을 **올리고** 호환 범위를 좁히는 것이다). 설치본에서 클라이언트 참조 34개를 모으고 검사 조건 한 줄만 `4.1.0.0` → `4.1.6.0` 으로 옮겼다. 검사는 끄지 않았다. **클라이언트가 경고 0·오류 0 으로 빌드된다.** 근거·대가·재현 방법은 `doc/WORKLOG.md` 의 "클라이언트 기준 변경과 첫 빌드" 와 `doc/install-records/stm-fork-import.md` 에 있다.

- [x] **Step 3: 두 프로젝트를 빌드한다**

```bash
dotnet build src/SptTeamManager/server/pitFireTeam.Server.csproj -c Debug
dotnet build src/SptTeamManager/client/pitFireTeam.csproj -c Debug -p:BuildSAINAddon=false
```

Expected: 둘 다 성공. 서버가 `Required SPT 4.1.0 server reference was not found` 또는 `compatibility requires 4.1.0.0 compile references`로 실패하면 Step 2의 DLL 버전이 틀린 것이다 — 설치본이 4.1.6이어도 참조는 4.1.0.0이어야 한다.

- [x] **Step 4: 들여온 것을 기록한다**

`doc/install-records/stm-fork-import.md`에 기준 커밋, 참조 DLL 출처 경로와 버전, 빌드 명령과 결과를 적는다.

- [x] **Step 5: 커밋**

```bash
git add .gitignore doc/install-records/stm-fork-import.md
git commit -m "chore: PIT Fireteam 1.0.0 을 포크로 들여온다"
```

---

### Task 2: 식별자 리네임과 설치 스크립트

**Files:**
- Modify: `src/SptTeamManager/client/friendlyPlugin.cs:208` (BepInPlugin 속성)
- Modify: `src/SptTeamManager/client/pitFireTeam.csproj` (AssemblyName)
- Modify: `src/SptTeamManager/server/pitFireTeam.Server.csproj` (AssemblyName)
- Modify: `src/SptTeamManager/server/Services/FriendlyTeammateStorage.cs` (DB 디렉터리 경로)
- Create: `tools/install-sptteammanager.ps1`

**Interfaces:**
- Produces: GUID `xyz.stm.teammanager`, 어셈블리 `SptTeamManager`·`SptTeamManager.Server`, 설정 파일 `xyz.stm.teammanager.cfg`, DB 디렉터리 `Resources/stm-teammates/`·`Resources/stm-teammates-allegiance/`.

- [x] **Step 1: 식별자만 바꾼다**

GUID·표시 이름·어셈블리명·DB 디렉터리 이름만 바꾼다. **네임스페이스 `pitTeam`, 클래스명, 파일 경로는 건드리지 않는다** — 업스트림 머지를 살리기 위한 제약이다(Global Constraints).

- [x] **Step 2: 설치 스크립트를 쓴다**

`tools/install-sptteammanager.ps1`. 기존 `tools/install-goggleswheel.ps1`과 같은 형식으로, 클라이언트 DLL은 `E:\Spt\BepInEx\plugins\SptTeamManager\`로, 서버 DLL과 `LiteDB.dll`·`Resources/`는 `E:\Spt\SPT_Runtime\user\mods\SptTeamManager-ServerMod\`로 복사한다. **덮어쓰기가 생기면 중단한다**(`doc/PROCEDURE.md`).

- [x] **Step 3: 빌드하고 설치한다**

```bash
dotnet build src/SptTeamManager/server/pitFireTeam.Server.csproj -c Debug
dotnet build src/SptTeamManager/client/pitFireTeam.csproj -c Debug -p:BuildSAINAddon=false
pwsh -File tools/install-sptteammanager.ps1
```

Expected: 설치 완료. 기존 `pitFireTeam` 디렉터리와 파일이 하나도 겹치지 않는다.

- [x] **Step 4: 서버를 띄워 로드를 확인한다**

Expected: 서버 로그에 STM 서버 모드가 로드되고 **오류 0건**. PIT 서버 모드는 이 시점에 빼 둔다(둘 다 로드하면 같은 팀원 DB를 두 이름으로 들게 된다).

- [x] **Step 5: 커밋**

```bash
git add src/SptTeamManager tools/install-sptteammanager.ps1
git commit -m "chore: 포크 식별자를 STM 으로 가른다"
```

---

### Task 3: 메뉴 게이트 — 인게임 확인

**Files:** 없음(확인만). 결과는 `doc/WORKLOG.md`에 적는다.

**Interfaces:**
- Consumes: Task 2의 설치본.
- Produces: 이후 Task들이 메인메뉴 UI를 전제해도 되는지에 대한 판정.

> 설계 §6-3과 §7이 "다른 작업보다 먼저"라고 못박은 단계다. 선례(pmc-squadmates)가 바로 이 자리에서 메인메뉴 목록을 포기했다. **사람만 할 수 있다.**

- [x] **Step 1: 게임을 띄워 메인메뉴를 연다**

- [x] **Step 2: 다음을 확인한다**

1. STM 버튼이 메인메뉴에 뜬다.
2. 눌러서 화면이 열리고 닫힌다.
3. 게임의 스쿼드/파티 UI가 정상 동작한다(버튼이 가리거나 입력을 먹지 않는다).
4. 레이드에 들어갔다 나온 뒤에도 버튼 가시성이 정상이다.

- [x] **Step 3: 결과를 기록한다**

`doc/WORKLOG.md`에 날짜·확인 항목·결과를 적는다. **충돌이 있으면 여기서 멈추고 보고한다** — 명단 화면을 어디에 둘지(메인메뉴 / 소셜 화면 / 메신저)부터 다시 정해야 한다.

---

### Task 4: 런처 프로필 열거

**Files:**
- Create: `src/SptTeamManager/shared/ProfileRosterPolicy.cs`
- Create: `src/SptTeamManager.Tests/ProfileRosterPolicyTests.cs`
- Create: `src/SptTeamManager.Tests/SptTeamManager.Tests.csproj`
- Modify: `src/SptTeamManager/server/pitFireTeam.Server.csproj` (shared 파일 `Compile Include` 추가)

**Interfaces:**
- Produces:
  - `record ProfileCandidate(string ProfileId, int Aid, string Nickname, int Level, string Side, ProfileAvailability Availability)`
  - `enum ProfileAvailability { Ready, Empty, Unreadable, CurrentSession }`
  - `static IReadOnlyList<ProfileCandidate> ProfileRosterPolicy.Select(IEnumerable<ProfileSnapshot> all, string currentSessionId)`
  - `record ProfileSnapshot(string ProfileId, int Aid, string? Nickname, int? Level, string? Side, bool ReadFailed)` — 서버가 `SaveServer.GetProfiles()`에서 채워 넣는 입력. 순수 클래스가 SPT 타입을 참조하지 않게 하는 경계다.

- [x] **Step 1: 실패하는 테스트를 쓴다**

```csharp
[Fact] void 현재_세션_프로필은_CurrentSession_으로_표시된다()
[Fact] void Info_가_없는_프로필은_Empty_로_표시되고_Ready_가_아니다()
[Fact] void 읽기에_실패한_프로필_하나가_나머지_열거를_막지_않는다()
[Fact] void 닉네임이_비면_프로필_id_앞_8자를_표시명으로_쓴다()
[Fact] void 결과는_입력_순서와_무관하게_닉네임_오름차순이다()
```

`Empty`·`Unreadable`·`CurrentSession`은 목록에 남되 `Ready`가 아니다. 호출자는 `Ready`만 팀에 넣는다.

- [x] **Step 2: 테스트가 실패하는지 확인한다**

Run: `dotnet test src/SptTeamManager.Tests/SptTeamManager.Tests.csproj`
Expected: FAIL — `ProfileRosterPolicy` 없음.

- [x] **Step 3: `ProfileRosterPolicy.Select`를 구현한다**

판정 순서는 현재 세션 → 읽기 실패 → `Info` 없음 → `Ready`. 예외를 밖으로 던지지 않는다.

- [x] **Step 4: 테스트가 통과하는지 확인한다**

Run: `dotnet test src/SptTeamManager.Tests/SptTeamManager.Tests.csproj`
Expected: PASS

- [x] **Step 5: 서버에서 스냅샷을 채우는 어댑터를 붙인다**

`src/SptTeamManager/server/Services/StmProfileCatalog.cs`를 만들고 `[Injectable]` 싱글턴으로 등록한다. `SaveServer.GetProfiles()`를 **호출할 때마다 새로 읽어** `ProfileSnapshot`으로 바꾼다 — 기동 시점에 캐시하면 런처에서 새로 만든 프로필이 영영 보이지 않는다(Review Focus). 프로필 하나의 변환이 던지면 `ReadFailed = true`로 바꿔 담고 계속한다.

- [x] **Step 6: 커밋**

```bash
git add src/SptTeamManager/shared/ProfileRosterPolicy.cs src/SptTeamManager/server/Services/StmProfileCatalog.cs src/SptTeamManager.Tests
git commit -m "feat: 런처 프로필을 팀원 후보로 추린다"
```

---

### Task 5: 프로필 팀원 등록 저장

**Files:**
- Create: `src/SptTeamManager/server/Models/StmProfileMember.cs`
- Create: `src/SptTeamManager/server/Services/StmProfileMemberStore.cs`
- Modify: `src/SptTeamManager/server/Routers/Static/FriendlyTeammateStaticRouter.cs`
- Create: `src/SptTeamManager/server/Callbacks/StmProfileMemberCallbacks.cs`

**Interfaces:**
- Consumes: Task 4의 `StmProfileCatalog`, `ProfileCandidate`.
- Produces:
  - `record StmProfileMember(string ProfileId, string Tactic, bool AutoJoin)`
  - `StmProfileMemberStore.List(MongoId sessionId) -> IReadOnlyList<StmProfileMember>`
  - `StmProfileMemberStore.Add(MongoId sessionId, string profileId)` / `Remove(...)`
  - 라우트 `/stm/members/list`, `/stm/members/add`, `/stm/members/remove`

- [x] **Step 1: 저장 위치를 정해 모델을 만든다**

등록 사실만 저장한다(어느 프로필을 팀에 넣었는지). 프로필 본문은 저장하지 않는다 — 원본이 진실이다. PIT의 암호화 LiteDB(`FriendlyTeammateStorage`)에 `stm-profile-members.json` 문서로 넣는다. 로스터 열거에서 제외되는 문서 목록에 이 키를 추가한다.

- [x] **Step 2: 라우트를 더한다**

`StaticRouter`에 **STM 고유 경로**(`/stm/...`)로 등록한다. EFT 네이티브 경로를 새로 가로채지 않는다.

- [x] **Step 3: 서버를 띄워 라우트를 확인한다**

Run: 서버 기동 후 `/stm/members/list` 호출
Expected: 빈 목록이 200으로 돌아온다. 서버 로그에 오류 없음.

- [x] **Step 4: 커밋**

```bash
git commit -am "feat: 프로필 팀원 등록을 저장한다"
```

---

### Task 6: 친구 목록 병합

**Files:**
- Create: `src/SptTeamManager/shared/FriendListMergePolicy.cs`
- Create: `src/SptTeamManager.Tests/FriendListMergePolicyTests.cs`
- Modify: `src/SptTeamManager/server/Callbacks/FriendlyTeammateSocialCallbacks.cs:23-47` (`MergeFriendList`)

**Interfaces:**
- Consumes: Task 5의 `StmProfileMemberStore`, Task 4의 `ProfileCandidate`.
- Produces:
  - `record FriendEntry(string Id, int Aid, FriendKind Kind)`
  - `enum FriendKind { Native, Mercenary, ProfileMember }`
  - `record MergeOutcome(IReadOnlyList<FriendEntry> Final, IReadOnlyList<(FriendEntry Dropped, string Reason)> Skipped)`
  - `static MergeOutcome FriendListMergePolicy.Plan(IReadOnlyList<FriendEntry> existing, IReadOnlyList<FriendEntry> incoming)`

  `ProfileCandidate`(Task 4)를 `FriendEntry`로 옮기는 변환은 `MergeFriendList` 안에서 한다 — 순수 정책은 SPT 타입도 `ProfileCandidate`도 모른다.

> PIT의 기존 병합은 `Id` 또는 `Aid`가 같으면 건너뛴다. 용병의 가짜 `Aid`가 실제 프로필 `Aid`와 겹치면 **진짜 프로필이 명단에서 사라진다**(Review Focus).

- [x] **Step 1: 실패하는 테스트를 쓴다**

```csharp
[Fact] void 같은_ProfileId_는_한_번만_들어간다()
[Fact] void Aid_가_같아도_종류가_다르면_프로필_팀원이_우선하고_용병이_밀린다()
[Fact] void Aid_가_0_이면_중복_판정에_쓰지_않는다()
[Fact] void 밀려난_항목은_이유와_함께_보고된다()
[Fact] void 네이티브_친구는_어떤_경우에도_제거되지_않는다()
```

- [x] **Step 2: 테스트가 실패하는지 확인한다**

Run: `dotnet test src/SptTeamManager.Tests/SptTeamManager.Tests.csproj`
Expected: FAIL

- [x] **Step 3: `FriendListMergePolicy.Plan`을 구현한다**

우선순위는 네이티브 > 프로필 팀원 > 용병. 밀려난 항목은 버리지 말고 이유와 함께 돌려주어 호출자가 로그에 남기게 한다.

- [x] **Step 4: 테스트가 통과하는지 확인한다**

Expected: PASS

- [x] **Step 5: `MergeFriendList`가 이 정책을 쓰게 바꾼다**

기존 `Any(existing => ...)` 중복 검사를 `FriendListMergePolicy.Plan` 호출로 바꾸고, 프로필 팀원을 용병과 **다른 회원 분류**로 담는다. 어떤 분류가 어떤 아이콘으로 보이는지는 Task 7의 인게임 확인에서 맞춘다(설계 §8).

- [x] **Step 6: 커밋**

```bash
git commit -am "feat: 프로필 팀원을 친구 목록에 병합한다"
```

---

### Task 7: 명단 화면

**Files:**
- Modify: `src/SptTeamManager/client/Components/SquadControlMenuUi.cs`
- Modify: `src/SptTeamManager/client/Components/SquadControlMenuUi.Backend.cs`
- Create: `src/SptTeamManager/client/Modules/StmProfileRosterClient.cs`

**Interfaces:**
- Consumes: Task 5의 `/stm/members/*`, Task 6의 친구 목록 분류.
- Produces: 명단에 프로필 팀원 줄과 "프로필 불러오기" 항목.

- [x] **Step 1: 명단 좌측에 프로필 팀원을 그린다**

카드 한 줄에 닉네임·레벨·진영, 그리고 실물 장비인지 임시 장비인지. `Ready`가 아닌 후보는 회색으로 두고 사유를 표시한다(`Empty` → "게임에 한 번 들어가야 합니다").

- [x] **Step 2: "프로필 불러오기"를 붙인다**

누르면 `Ready` 후보 목록을 띄우고, 고르면 `/stm/members/add`를 부른 뒤 명단을 새로 읽는다.

- [x] **Step 3: 인게임으로 확인한다**

1. 명단에 런처 프로필이 보이고, 지금 로그인한 프로필은 없다.
2. 빈 프로필이 회색으로 뜨고 선택되지 않는다.
3. 불러오기로 추가한 프로필이 "파티에 초대" 창에도 뜬다.
4. 용병과 프로필 팀원이 **서로 다른 아이콘**으로 보인다. 같아 보이면 회원 분류 값을 바꿔 가며 맞추고, 맞춘 값을 설계 §8에 적는다.

- [x] **Step 4: 커밋**

```bash
git commit -am "feat: 명단에 프로필 팀원을 보여준다"
```

---

### Task 8: 레이드 투입

**Files:**
- Create: `src/SptTeamManager/shared/PartySlotPolicy.cs`
- Create: `src/SptTeamManager.Tests/PartySlotPolicyTests.cs`
- Modify: `src/SptTeamManager/server/Callbacks/FriendlyTeammateMatchCallbacks.cs`

**Interfaces:**
- Consumes: Task 5의 등록 목록, Task 6의 분류.
- Produces: `static SlotDecision PartySlotPolicy.Admit(int mercenariesInParty, int profileMembersInParty, FriendKind joining)` — 허용/거부와 거부 사유.

- [x] **Step 1: 실패하는 테스트를 쓴다**

```csharp
[Fact] void 팀원_자리는_셋이다()
[Fact] void 용병_둘과_프로필_팀원_하나는_허용된다()
[Fact] void 넷째는_거부되고_사유가_담긴다()
[Fact] void 거부는_조용히_자르지_않고_사유_문자열을_돌려준다()
```

- [x] **Step 2: 테스트가 실패하는지 확인한다**

Expected: FAIL

- [x] **Step 3: `PartySlotPolicy.Admit`을 구현한다**

상한 3은 상수로 두고 설정으로 올리지 않는다 — 네이티브 한계다(Global Constraints).

- [x] **Step 4: 테스트가 통과하는지 확인한다**

Expected: PASS

- [x] **Step 5: 초대 경로에 정책을 건다**

`SendGroupInvite`에서 정책을 먼저 묻고, 거부면 기존 `mailSendService.SendSystemMessageToPlayer(sessionId, rejectionReason, null)` 경로로 사유를 알린다.

- [x] **Step 6: 인게임으로 확인한다**

1. 프로필 팀원을 파티에 넣고 레이드에 들어간다. 임시 장비를 입고 스폰한다.
2. 용병과 프로필 팀원을 섞어 셋까지 들어간다.
3. 넷째를 넣으려 하면 거부되고 메시지가 뜬다.
4. 레이드가 정상 종료된다. **이 계획 범위에서는 레이드 결과가 프로필에 반영되지 않는다** — 계획 3의 몫이다.

- [x] **Step 7: 커밋**

```bash
git commit -am "feat: 프로필 팀원을 레이드에 투입한다"
```

---

### Task 9: 문서

**Files:**
- Create: `src/SptTeamManager/README.md`
- Modify: `doc/INSTALLED-MODS.md` (생성 스크립트로)
- Modify: `doc/WORKLOG.md`

- [x] **Step 1: README를 쓴다**

PIT Fireteam 1.0.0 포크임을 밝히고 원 저작권 고지(MIT)와 원 저장소 링크를 남긴다. 이 계획에서 동작하는 범위와 아직 안 되는 것(장비 편집, 레이드 후 반영)을 적는다.

- [x] **Step 2: 설치 목록을 다시 만든다**

```bash
node tools/gen-installed-mods.mjs
```

`doc/INSTALLED-MODS.md`는 직접 고치지 않는다.

- [x] **Step 3: 커밋**

```bash
git add src/SptTeamManager/README.md doc/INSTALLED-MODS.md doc/WORKLOG.md
git commit -m "docs: STM 기반 단계 결과를 적는다"
```

---

## 이 계획에 없는 것

설계를 셋으로 나눈 첫 조각이다. 다음 두 계획은 이 계획이 인게임에서 통과한 뒤에 쓴다.

- **계획 2 — 장비.** 실물/임시 장비 분기, 로드아웃 편집(편집하면 실물이 된다), 본인 스태시만 출처로 쓰기, §5-14 스태시 재검증 수리, §6-8 3단 전송 저널.
- **계획 3 — 레이드 후.** 경험치·스킬·전적 적립, 전리품 메일 배달, 소모·손상 반영, 사망 처리, 추가 기능 셋(출격 전 손실 요약, 레이드 후 리포트, 전리품 행선지 규칙).
