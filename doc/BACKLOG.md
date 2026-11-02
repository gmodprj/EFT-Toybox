# 나중에 할 일

작업 중에 "이건 이번 범위가 아니다" 로 미룬 것만 적는다. 하기로 정한 것이지 후보 목록이 아니다.
끝내면 줄을 지우고 어디에 들어갔는지(커밋·문서) 한 줄 남긴다.

| 할 일 | 왜 미뤘나 | 손댈 자리 |
| --- | --- | --- |
| **프로필 팀원의 auto-join 을 동작하게 한다** — 팀원별 토글을 켜 두면 출격 화면에서 자동 합류하는 기능. 필요한 세 곳 중 **플레이어 구성은 2026-11-02 에 메웠고**(`StmProfileViewFactory`), **토글 저장**과 **목록 조회**가 남았다. 그래서 메운 쪽은 지금 **닿지 않는 코드**다 | 2026-11-02, 사용자가 **계획 3 에 넣기로** 정했다. 저장 계층부터 손대야 해서다 — `StmProfileMemberStore` 에 갱신 경로(`List`·`Add`·`Remove` 뿐)가 없고, 명단 배지를 보여 주려면 로스터 와이어에 `autoJoin` 도 실어야 한다. 팀원 설정 UI 와 한 묶음으로 설계하는 편이 맞다 | `SetTeammateAutoJoin`(프로필 팀원이면 throw)과 [`GetAutoJoinTeammateAccountIds`](../src/SptTeamManager/server/Services/FriendlyTeammateService.cs:646)(PIT 만 열거). 두 곳 다 STM 해석기를 PIT 보다 먼저 묻는 기존 모양을 쓴다. 뜻과 로그 증거는 WORKLOG "정정 — auto-join 은 '지난 분대 복원' 이 아니다" |
| **야시경·실드 휠의 남은 인게임 점검 3건** — ① 휠을 띄운 채(N 누른 상태) 죽거나 레이드를 끝냈을 때 입력이 잠기지 않고 아무 것도 여닫히지 않는지 ② 왼손에 나침반을 들고 N 을 눌렀을 때 UI Fixes 가 나침반을 치운 뒤 야시경이 여닫히고 휠이 뜨지 않는지 ③ 제스처 휠(`B`)이 멀쩡히 도는지. **머지 시점에 이 셋은 확인되지 않았다** | 2026-10-31, 사용자가 건너뛰고 머지하기로 정했다. ①은 코드상 `ShouldApplySelection` 으로 막아 두고 시험 4개로 묶었으나 게임에서 본 적은 없다. ②는 설계 §6-2 의 공존 경로이고 COTI 쪽(§7-9)은 역컴파일로 확인했지만 UI Fixes 경로는 못 봤다. ③은 우리가 제스처 휠을 건드리지 않게 만들었다는 전제의 확인이다 | 설계 §7 의 7·8·10 항. 레이드에서 셋 다 한 번에 볼 수 있다. ①이 틀리면 치명적이다(시점이 잠긴 채 레이드가 끝난다) — 그 경우 [GogglesPatch.cs](../src/GogglesWheel/GogglesPatch.cs) 의 `finally` 와 `LookBlocker.Release()` 를 먼저 본다 |
| **ISB 역할 이름 10개의 한국어 번역** — `ScavRole/ISB`, `ISBBoss`, `ISBGuard`, `ISBScout`, `ISBSniper`, `ISBHeavy`, `ISBAssassin`, `ISBWhiteTusk`, `ISBCommander`, `ISBBossFollower`. 지금 ko 와 en 이 글자까지 같은 영문("ISB BOSS"·"WHITE TUSK"…)이라 킬 패널·시체 표시·지도 범주가 전부 영문으로 뜬다 | 2026-10-30 지도 세력 표시 작업의 범위 밖이었다(설계 §3 비목표). 번역은 ISB 모드의 locale 을 건드리는 일이라 결이 다르다 | ISB Special Forces 의 `db/CustomLocales/*.json` (ko 가 아예 없고 en 을 쓰고 있다). 모드 파일을 직접 고치면 업데이트에 날아가므로, 서버 모드 쪽에서 locale 에 덮어쓰는 방식을 검토한다. 실측은 [specs/2026-10-30-faction-identification-survey.md](specs/2026-10-30-faction-identification-survey.md) §3 |
