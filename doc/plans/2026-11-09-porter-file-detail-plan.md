# ProfilePorter 파일 지우기·세부 보기 구현 계획

> **에이전트에게:** 이 계획은 Task 하나씩 구현한다. 단계는 `- [ ]` 로 표시한다.
> 범위를 넘지 않는다 — 한 Task 를 끝내면 보고하고 멈춘다.

**목표:** 창에서 내보낸 파일을 **지울 수** 있고, 그 안에 무엇이 들었는지 **자식창의 격자로**
되짚어 볼 수 있게 한다.

**구조:** 판정은 SPT 를 모르는 순수 클래스 둘(`DetailPlanner`·`SlotNames`)에 두고 시험으로
가둔다. 서버는 그 판정에 **DB 가 아는 것**(격자 칸 수·아이템 크기·이름)을 먹여 라우트 둘로
내보낸다. 창은 라우트만 부르고, 격자는 이미 있는 `StashGridView` 를 읽기 전용으로 다시 쓴다.

**기술:** C# / .NET (서버 net10.0, 창 WPF, 시험 net8.0 + xUnit), SPT 4.1.6.

**설계:** [doc/specs/2026-11-09-porter-file-detail-design.md](../specs/2026-11-09-porter-file-detail-design.md)
(앞선 설계 [2026-11-08-profile-porter-design.md](../specs/2026-11-08-profile-porter-design.md))

## 전체 제약

- 요청 레코드의 모든 속성에 **`[JsonPropertyName]`** 을 단다. 창은 camelCase 로 보낸다 —
  없으면 조용히 null 이 되어 엉뚱한 사유로 거절한다(앞 설계 §5-5).
- 새 라우트는 `ProfilePorterRouter.BuildRoutes` 에 더하고 **`Enabled` 관문**을 같이 건다.
- 파일 이름은 반드시 `PorterFiles.PathFor` 의 **이름 씻기**를 거친다. `porter/` 밖은 거기서 막힌다.
- 아이템 크기는 **tpl 기본 크기**(`Properties.Width/Height`)다. `ItemHelper.GetItemSize` 를 쓰지
  않는다(앞 설계 §5-4).
- 시험 프로젝트는 와일드카드를 쓰지 않는다 — **새 소스마다 `<Compile Include>` 한 줄**을 더한다.
- 빌드 경고 0 / 오류 0 을 유지한다.
- 브랜치는 `feat/*`·`fix/*` 로 `develop` 에서 따고, Task 마다 커밋한다.
- 서버 모드를 고치면 **서버를 다시 켜야** 적용된다. 설치 확인은 md5 만이 아니라
  **설치본 시각 > 고친 소스 시각**까지 본다(앞 설계 §15.1).

## 눈여겨볼 것 (Review Focus)

1. **같은 격자 같은 칸에 둘** — 묵은 파일이나 깨진 파일에서 좌표가 겹칠 수 있다. 겹쳐도
   **아무것도 사라지지 않아야** 한다(Task 1 시험).
2. **뿌리에서 닿지 않는 자손** — `rootIds` 어디에도 매달리지 않은 항목. 버리지 않고 보여야
   한다(Task 1 시험).
3. **한글 파일 이름** — `Nomad-스태시-…` 가 라우트를 왕복해야 한다(Task 3·4 실측).
4. **지우려는 순간 파일이 없거나 잠겨 있음** — 사유를 돌려주고 창이 그대로 말한다(Task 4).
5. **1,361개짜리 파일** — 여는 데 몇 초씩 걸리면 실패다(Task 7 실측).

---

### Task 1: `DetailPlanner` — 자리 배치 판정 (순수)

**파일**
- 만들기: `src/ProfilePorter.Server/DetailPlanner.cs`
- 고치기: `src/ProfilePorter.Tests/ProfilePorter.Tests.csproj` (`<Compile Include>` 한 줄)
- 시험: `src/ProfilePorter.Tests/DetailPlannerTests.cs`

**인터페이스**
- 쓰는 것: `PorterItem`(`Id`·`ParentId`·`SlotId`·`TemplateId`·`Upd`·`Location`), `PorterBundle`.
- 내놓는 것:
  ```csharp
  public sealed record DetailSlot(string SlotId, string Id, string Tpl, int Stack, int Contained);
  public sealed record DetailGrid(string Name, int CellsH, int CellsV, IReadOnlyList<PorterItem> Rows);
  public sealed record DetailLoose(string Id, string Tpl, int Stack, int Contained, string Why);
  public sealed record DetailPlace(
      IReadOnlyList<DetailGrid> Grids, IReadOnlyList<DetailSlot> Slots, IReadOnlyList<DetailLoose> Loose);
  public sealed record GridSpec(string Name, int CellsH, int CellsV);

  public static DetailPlace Place(
      IReadOnlyList<PorterItem> children,
      IReadOnlyList<GridSpec> grids,
      IReadOnlySet<string> slotNames);

  public static IReadOnlyList<PorterItem> ChildrenOf(PorterBundle bundle, string? parentId);

  /// 현재 스태시 크기와 좌표 테두리 중 **큰 쪽**(설계 §7-5). 프로필이 없으면 0,0 을 넘긴다.
  public static (int cellsH, int cellsV) StashCells(
      int profileCellsH, int profileCellsV, IEnumerable<(int x, int y, int w, int h)> rows);
  ```
  `Place` 는 **이름을 풀지 않는다** — 이름·크기는 서버가 DB 에서 붙인다(Task 3).

- [ ] **단계 1: 실패하는 시험을 쓴다** (`DetailPlannerTests.cs`)

```csharp
[Fact] public void 격자와_슬롯을_함께_가진_리그는_둘_다_나온다()
// grids: [("1",2,2)], slotNames: ["Front_plate"]
// children: 주머니 속 탄창(slotId "1", location {x:0,y:0}) + 플레이트(slotId "Front_plate", location null)
// → Grids[0].Rows 1개, Slots 1개, Loose 0개

[Fact] public void 슬롯에_꽂힌_것은_자리가_없어도_loose_가_아니다()
[Fact] public void 격자에_가야_하는데_좌표가_없으면_loose_로_간다()   // Why 가 비어 있지 않다
[Fact] public void 모르는_slotId_는_버리지_않고_loose_로_간다()
[Fact] public void 같은_칸에_둘이_겹쳐도_둘_다_나온다()               // Review Focus 1
[Fact] public void 뿌리에서_닿지_않는_자손도_ChildrenOf_로_찾을_수_있다()  // Review Focus 2
[Fact] public void ChildrenOf_는_직계_자식만_준다()

[Fact] public void 스태시_칸수는_현재크기와_테두리_중_큰_쪽이다()
// (10,20) 인데 (x:11,y:0,w:2,h:1) 이 있으면 → cellsH 13. 프로필이 없으면(0,0) 테두리만.
```

- [ ] **단계 2: 돌려서 빨간지 본다** — `dotnet test src/ProfilePorter.Tests/ProfilePorter.Tests.csproj`
  기대: `DetailPlanner` 가 없어 **컴파일 실패**.

- [ ] **단계 3: `DetailPlanner` 를 구현한다**

  자식 하나를 놓는 순서는 설계 §4.2 그대로다 — ① `SlotId` 가 격자 이름과 같으면 그 격자,
  ② 슬롯 이름에 있으면 `Slots`, ③ 아니거나 **격자행인데 `Location` 이 없으면** `Loose`.
  **②가 ③보다 먼저**다(그래야 플레이트·부착물이 새지 않는다).

- [ ] **단계 4: 시험이 녹색인지 본다** — 전체 개수가 **29건**(기존 21 + 8)인지 함께 본다.

- [ ] **단계 5: 돌연변이로 시험을 검증한다**

  ②와 ③의 순서를 **뒤집어** 돌린다 → `슬롯에_꽂힌_것은…` 만 빨개져야 한다. 되돌린다.

- [ ] **단계 6: 커밋** — `feat(porter): 세부 보기의 자리 배치 판정`

---

### Task 2: `SlotNames` — 슬롯 이름 한국어 사전 (순수)

**파일**
- 만들기: `src/ProfilePorter.Server/SlotNames.cs`
- 고치기: 시험 csproj (`<Compile Include>`)
- 시험: `src/ProfilePorter.Tests/SlotNamesTests.cs`

**인터페이스**
```csharp
public static string Korean(string? slotId);          // 모르면 받은 그대로 돌려준다
public static bool Knows(string? slotId);
```

- [ ] **단계 1: 실패하는 시험을 쓴다**

```csharp
[Fact] public void 장비_열넷은_한국어다()        // Headwear→모자, TacticalVest→리그, Scabbard→칼집 …
[Fact] public void 안쪽_슬롯도_한국어다()        // mod_magazine→탄창, patron_in_weapon→약실, Front_plate→전면 플레이트 …
[Fact] public void 모르는_것은_영문_그대로다()    // "mod_wibble" → "mod_wibble" (빈 글자가 되지 않는다)
[Fact] public void 빈_것은_빈_글자다()
```

- [ ] **단계 2: 돌려서 빨간지 본다**
- [ ] **단계 3: 사전을 적는다** — 설계 §4.3 의 장비 열넷 + 안쪽 슬롯(최소:
  `mod_magazine`·`mod_scope`·`mod_muzzle`·`mod_tactical`·`mod_stock`·`mod_handguard`·
  `mod_barrel`·`mod_mount`·`mod_sight_rear`·`mod_sight_front`·`mod_pistol_grip`·
  `patron_in_weapon`·`cartridges`·`camora_000`, 판·소프트아머 전부, 격자 이름 `main`).
  `StringComparer.Ordinal` 사전 하나로 둔다.
- [ ] **단계 4: 시험 녹색** (전체 **33건**)
- [ ] **단계 5: 커밋** — `feat(porter): 슬롯 이름 한국어 사전`

---

### Task 3: `/profile-porter/file-detail` 라우트

**파일**
- 고치기: `src/ProfilePorter.Server/ProfilePorterRouter.cs`(라우트·요청 레코드),
  `src/ProfilePorter.Server/ProfilePorterService.cs`(`Detail` 메서드),
  `src/ProfilePorter.Server/ProfileReader.cs`(DB 조회 둘을 공개)

**인터페이스**
- 쓰는 것: Task 1 의 `DetailPlanner.Place`·`ChildrenOf`, Task 2 의 `SlotNames.Korean`.
- 내놓는 것: 설계 §4.2 의 응답. `rows` 는 **기존 `StashEntry` 모양 그대로**
  (`id·tpl·name·stack·contained·x·y·w·h·rotated`) — 창의 `StashRow` 가 그대로 받는다.
- `ProfileReader` 에 더할 것:
  ```csharp
  public IReadOnlyList<GridSpec> GridsOf(string tpl);      // Properties.Grids 의 _name·CellsH·CellsV
  public IReadOnlySet<string> SlotsOf(string tpl);         // Properties.Slots 의 _name
  public (int w, int h) SizeOf(string tpl);                // 기존 SafeSize 를 공개로
  public string NameOf(string tpl);                        // 기존 ResolveName 을 공개로
  ```

- [ ] **단계 1: 요청·응답 레코드를 적는다** — `PorterDetailRequest { fileName, containerId? }`,
  `[JsonPropertyName]` 필수. 응답은 익명 객체로 camelCase 를 낸다(기존 `files` 라우트와 같은 식).

- [ ] **단계 2: `ProfilePorterService.Detail(string? fileName, string? containerId)` 을 구현한다**

  파일을 읽고(`PorterFiles.Read`) → `ChildrenOf` 로 그 자리의 자식을 모으고 → 뿌리면
  `kind` 에 따라 `stash`/`loadout`, 아니면 `container` → `Place` 로 나눈 뒤 이름·크기·슬롯
  한국어를 붙인다. `summary` 는 `upd` 에서 **묶음 수·내구도(`x/y`)·자원 잔량**만 추려 짧은
  글로 만든다(모르는 값은 적지 않는다). 스태시 칸 수는 **보낸 프로필의 크기와 좌표 테두리 중
  큰 쪽**이고, 추측이 섞이면 `notices` 에 한 줄 담는다.

- [ ] **단계 3: 라우트를 더한다** — `Enabled` 관문 포함.

- [ ] **단계 4: 빌드** — `dotnet build src/ProfilePorter.Server/... -c Release` 경고 0 / 오류 0.

- [ ] **단계 5: 설치하고 서버를 켜서 실측한다**

  `pwsh -File tools/install-profileporter.ps1` → 설치본 시각이 소스보다 **새것**인지 확인 →
  서버 기동(포트 6969 가 열릴 때까지 기다린다) → `node tools/call-porter.mjs /profile-porter/file-detail '{"fileName":"Nomad-착용장비-20261109_084032"}'`.
  기대: `view.kind == "loadout"`, `slots` 14칸, 한국어 이름. 이어서 그 리그의 `containerId` 로
  한 번 더 — `grids` 와 `slots` 가 **둘 다** 차 있어야 한다.
  **한글 파일 이름이 그대로 오가는지**도 이 호출이 증거다(Review Focus 3).

- [ ] **단계 6: 커밋** — `feat(porter): 파일 세부 보기 라우트`

---

### Task 4: 파일 지우기 라우트와 "못 읽는 파일"

**파일**
- 고치기: `src/ProfilePorter.Server/PorterFiles.cs`(`PathFor` 를 `internal` 로, `Delete`,
  `List` 가 못 읽는 파일도 싣기), `ProfilePorterRouter.cs`(라우트), `ProfilePorterService.cs`

**인터페이스**
```csharp
internal string? PathFor(string? name);
public bool Delete(string? name, out string? refusal);   // 사유: 이름이 이상하다 / 없다 / 지우지 못했다
```
`List()` 가 돌려주는 줄에 `readable`(bool)을 더한다. 못 읽는 파일은
`FileName` 과 `readable: false` 만 채워 올린다.

- [ ] **단계 1: `PorterFile` 에 `[JsonPropertyName("readable")] bool Readable { get; set; } = true;` 를 더하고 `List()` 를 고친다**

- [ ] **단계 2: `Delete` 를 구현한다** — 이름 씻기 → 없으면 사유 → `File.Delete` →
  `IOException`(잠김)은 사유로 돌려준다(Review Focus 4). 지운 것을 로그에 한 줄.

- [ ] **단계 3: 라우트 `/profile-porter/file-delete` 를 더한다**(`Enabled` 관문 포함).

- [ ] **단계 4: 빌드 → 설치 → 서버 재기동 → 실측**

  - 버릴 파일 하나를 만든다: `node tools/call-porter.mjs /profile-porter/export '{"profileId":"<id>","kind":"loadout","name":"지울것-시험"}'`
  - 지운다: `/profile-porter/file-delete '{"fileName":"지울것-시험"}'` → `ok: true`
  - `/profile-porter/files` 에 그 이름이 **없는지** 본다.
  - 없는 이름·`../` 가 섞인 이름으로 한 번씩 더 불러 **거절 사유**가 오는지 본다.
  - 깨진 파일을 하나 만들어(`porter/깨진것-시험.json` 에 `{`) `files` 에 `readable: false` 로
    **뜨는지**, 지워지는지 본다. 시험이 끝나면 치운다.

- [ ] **단계 5: 커밋** — `feat(porter): 파일 지우기와 못 읽는 파일 싣기`

---

### Task 5: 창 — 지우기 단추

**파일**
- 고치기: `src/ProfilePorter.App/ServerClient.cs`(`DeleteFileAsync`, `PorterFileRow.Readable`),
  `src/ProfilePorter.App/MainWindow.xaml`(단추 둘), `MainWindow.xaml.cs`(처리기)

**인터페이스**
```csharp
public Task<PorterResultRow?> DeleteFileAsync(string fileName, CancellationToken ct);
```

- [ ] **단계 1: `PorterFileRow` 에 `Readable` 을 더하고 `Display` 가 못 읽는 줄을 `(못 읽음)` 으로 적게 한다**
- [ ] **단계 2: XAML 에 `지우기`·`세부 보기` 단추를 더한다**(파일 목록 아래, `새로 읽기` 옆).
  둘 다 처음에는 꺼져 있고, 줄을 고르면 켜진다. **못 읽는 줄에서는 `지우기` 만** 켜진다.
- [ ] **단계 3: `Delete_Click` 을 구현한다** — `MessageBox.YesNo` 로 한 번 묻는다. 묻는 글에
  **파일 이름 · 아이템 수 · 만든 시각 · "되돌릴 수 없습니다" · "이미 보낸 우편은 그대로
  남습니다"** 가 보여야 한다(설계 §5.1). 예를 고르면 지우고 목록을 다시 읽는다. 서버가 사유를
  주면 상태줄에 **그대로** 적는다.
- [ ] **단계 4: 창을 빌드해 띄워 본다** — 파일 하나를 만들어 지우고, 목록에서 사라지는지 본다.
- [ ] **단계 5: 커밋** — `feat(porter): 창에서 내보낸 파일 지우기`

---

### Task 6: 창 — 세부 보기 자식창

**파일**
- 만들기: `src/ProfilePorter.App/FileDetailWindow.xaml` · `FileDetailWindow.xaml.cs`
- 고치기: `StashGridView.cs`(`ReadOnly`), `ServerClient.cs`(`DetailAsync` 와 응답 레코드),
  `MainWindow.xaml.cs`(`Detail_Click`), `ProfilePorter.App.csproj` 가 새 XAML 을 집는지 확인

**인터페이스**
```csharp
public bool ReadOnly { get; set; }     // StashGridView — 켜면 고르기·끌기·올가미를 받지 않는다
public Task<DetailResponse?> DetailAsync(string fileName, string? containerId, CancellationToken ct);
```

- [ ] **단계 1: `StashGridView.ReadOnly` 를 더한다** — 켜져 있으면 `OnCanvasDown`·`OnCanvasMove`·
  `OnCanvasUp` 이 **즉시 돌아간다**. 끄면 지금과 같다(본 창이 쓰는 길을 건드리지 않는다).
- [ ] **단계 2: `DetailResponse` 레코드를 적는다** — 설계 §4.2 의 모양, `[JsonPropertyName]`.
- [ ] **단계 3: `FileDetailWindow` 를 만든다**
  - 왼쪽 `TreeView` — 펼칠 때 그 자리를 서버에 물어 한 겹씩 채운다.
  - 오른쪽은 `view.kind` 에 따라: `stash` → `StashGridView`(읽기 전용) 한 판,
    `container` → 격자 여럿을 세로로 쌓고 그 아래 슬롯 목록, `loadout` → 설계 §4.3 의 자리로
    놓은 슬롯 판(안이 있는 칸은 눌린다).
  - `loose` 가 있으면 아래에 목록으로 보여 주고 `notices` 는 창 위에 한 줄로 적는다.
  - 아이콘은 본 창의 `IconService` **그 인스턴스**를 넘겨 쓴다.
  - `Owner` 를 본 창으로 걸고, 고르기·보내기·지우기는 **두지 않는다**.
- [ ] **단계 4: `Detail_Click` 에서 창을 띄운다**(`ShowDialog` 가 아니라 `Show` — 본 창을 막지 않는다).
- [ ] **단계 5: 띄워 본다** — 스태시 파일(1,361개)과 착용장비 파일을 각각 열고, 리그 안으로
  들어가 **주머니 속 물건과 플레이트가 둘 다** 보이는지 본다. 묵은 파일(11-08)을 열어
  **알림 줄**이 뜨는지 본다.
- [ ] **단계 6: 커밋** — `feat(porter): 내보낸 파일 세부 보기 창`

---

### Task 7: 마무리 — 설치·실측·문서

- [ ] **단계 1: 전체 시험** — `dotnet test` 가 **33건 전부 통과**. 빌드 경고 0 / 오류 0.
- [ ] **단계 2: 설치와 기동 확인** — 서버 모드와 창을 설치하고(`tools/install-profileporter.ps1`,
  `tools/install-profileporter-app.ps1`), 설치본 시각이 소스보다 새것인지 본다. 기동 로그에
  `[ProfilePorter] 프로필 N개, 내보낸 파일 M개` 가 뜨는지 본다.
- [ ] **단계 3: 속도 실측(Review Focus 5)** — 1,361개짜리 파일로 세부 보기를 열고, 뿌리와
  상자 둘을 펼치는 데 걸린 시간을 **잰다**. 눈에 띄게 느리면(한 번에 2초 이상) 읽은 파일을
  잠깐 들고 있게 고친다(이름 + 쓴 시각으로 무르게).
- [ ] **단계 4: 기록 검사** — `node tools/check-install-records.mjs` 가 **OK — 0/0**,
  `node tools/check-porter-location.mjs` 가 11-09 파일 셋에서 OK.
- [ ] **단계 5: 문서** — 설계 문서에 "만든 뒤 실측" 절을 더한다(무엇을 재서 무엇이 나왔는지,
  화면 확인은 사람이 한 것). `doc/WORKLOG.md` 에 한 문단.
- [ ] **단계 6: 커밋하고 `develop` 에 머지한다**(머지 커밋, 스쿼시하지 않는다).
