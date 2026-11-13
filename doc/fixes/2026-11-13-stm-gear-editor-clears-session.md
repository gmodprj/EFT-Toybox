# 장비 편집기를 닫으면 화면의 세션이 함께 사라진다 (2026-11-13)

**증상(사용자 제보).** STM → 프로필 팀원 → **장비 편집 진입 → 나옴** → **스태시 교환** 을 열면
격자가 아니라 **옛 목록형 화면**이 떴다. 거기서 **꾸러미 화면을 한 번 다녀온 뒤** 스태시 교환을
다시 열면 **격자로 정상**이었다.

## 1. 무엇이 일어났나 — 로그가 분기를 적고 있었다

스태시 교환 화면은 격자를 못 세우면 **조용히 떨어지지 않고 이유를 적는다**(그렇게 만들어 두었다).
제보 당시 돌고 있던 게임의 `BepInEx\LogOutput.log` 에 그 줄이 그대로 있었다 — 네 번 시도한 만큼 네 쌍:

```text
[Warning:SPT Team Manager] [STM] 스태시 교환: 세션이 비어 있어 격자를 세우지 못한다(목록으로 그린다).
[Warning:SPT Team Manager] [STM] 스태시 교환: 격자를 세우지 못해 목록으로 그린다.
```

그러니 "격자가 깨졌다" 가 아니라 **`ActiveProfileSession` 이 비어서 격자를 세울 재료가 없었던 것**이다.
격자는 그 세션의 프로필 껍데기로 세운다
([`OtherPlayerProfileScreenPatch.StmStashTransfer.cs`](../../src/SptTeamManager/client/Patches/OtherPlayerProfileScreenPatch.StmStashTransfer.cs) §317~337).

## 2. 누가 비웠나

`ActiveProfileSession` 에 쓰는 자리는 넷뿐이다.

| 자리 | 하는 일 |
| --- | --- |
| `PatchPostfix` (OtherPlayerProfileScreenPatch.cs:998) | 용병 경로에서 화면이 설 때 채운다 |
| `TryApplyStmDetail` (StmDetail.cs:109~112) | **프로필 팀원 경로**에서 화면이 설 때 채운다 |
| `ResetTeammateProfileUi` (:1693) · `Hide` 후처리 (:1858) | **화면이 닫힐 때** 비운다 |
| `TeardownStmGearEditor` (StmGearEditor.cs:1801) | **← 이것이 범인** |

장비 편집기는 **상세 화면 위에 뜨는 덮개**다. 화면이 바뀌는 것이 아니다. 그런데 덮개를 닫는
`TeardownStmGearEditor` 가 화면의 것인 `ActiveProfileSession`·`ActiveProfileInventoryController` 까지
비우고 있었다. 그래서 **화면은 그대로 떠 있는데 세션만 사라진** 상태가 된다.

편집기를 여는 쪽 주석은 이미 올바른 주인을 적어 두고 있었다 —
"화면이 닫히거나 다시 열리면 PIT 의 정리 코드가 비운다"(StmGearEditor.cs:420~421).
비우는 줄은 그 설명과 어긋난 채 남아 있던 것이다.

**꾸러미를 다녀오면 낫는 이유도 같은 것으로 설명된다.** 꾸러미는 화면을 다시 세우므로
`TryApplyStmDetail` 이 다시 돌아 세션을 채운다.

## 3. 고친 것

`TeardownStmGearEditor` 에서 그 두 줄을 **지웠다**. 덮개는 자기 것만 치운다. 왜 비우면 안 되는지를
주석으로 그 자리에 남겼다. 지운 뒤에도 세션을 비우는 자리는 **화면이 닫히는 둘뿐**이다.

- `dotnet build client/pitFireTeam.csproj -c Release -p:BuildProjectReferences=false` → **경고 0 · 오류 0**
  (애드온까지 묶은 전체 빌드는 이 PC 에서 `MSB3644` 로 실패한다 — [BACKLOG](../BACKLOG.md) 의 알려진 항목).

## 4. 설치했다 (2026-11-13 11:11)

사용자가 게임을 껐고, 그래서 DLL 잠김 없이 넣었다. **바뀐 것은 클라이언트 DLL 하나뿐**이라
그 파일만 바꿔 넣었다.

| | |
| --- | --- |
| 넣은 것 | `BepInEx\plugins\SptTeamManager\SptTeamManager.dll` (2,874,880 B, md5 `6cf117c6…`) |
| 빌드 | `client/bin/Release/net472` — **Release** 다(Debug 로 깔면 brain 예외가 쏟아진다) |
| 대조 | 설치본과 빌드 결과의 md5 가 **같다**. 옛 판은 md5 `f90ca48f…`(11-09) |
| 되돌리기 | `backup/stm-client-dll/SptTeamManager.dll.before-2026-11-13` 을 도로 덮는다 |
| 검사 | `check-install-records.mjs` → `OK — 0/0`. `INSTALLED-MODS.md` 는 내용이 바뀌지 않았다(그 문서는 DLL 해시를 적지 않는다) |

> **`tools/install-sptteammanager.ps1` 은 쓰지 못했다.** 그 스크립트는 **설치 전용**이라
> 대상 파일이 하나라도 있으면 아무것도 복사하지 않고 중단한다. STM 은 이미 깔려 있으므로
> 갱신에는 쓸 수 없다. 지금은 손으로 한 파일만 바꿔 넣었고, **갱신 길(`-Update` 류)이 없다는 것**은
> 다음에 집을 거리다 — `install-airdropcaller-server.ps1` 이 `-AllowServerRunning` 으로 같은 문제를 푼 선례가 있다.

## 5. 인게임에서 닫혔다 (2026-11-13)

사용자가 같은 순서(프로필 팀원 → 장비 편집 진입 → 나옴 → 스태시 교환)를 다시 밟았다 —
**격자가 바로 떴다.**

로그도 같은 것을 말한다. 그 기동의 `BepInEx\LogOutput.log` 에서:

| | 고치기 전 (11-13 09:12 기동) | 고친 뒤 (11-13 12:5x 기동) |
| --- | --- | --- |
| `스태시 교환: 세션이 비어 있어 격자를 세우지 못한다` | **4회** | **0회** |
| STM 의 경고·오류 전체 | 위 4쌍 | **0건** |

> **로그가 말하는 것은 "경고가 없다" 까지다.** 성공하는 길은 일부러 아무 줄도 남기지 않으므로,
> 화면이 열렸다는 것 자체는 **사용자 확인**이 근거다. 둘을 합쳐야 닫힌다.

### 아직 남은 것

- 함께 비워지던 `ActiveProfileInventoryController` 쪽은 보지 않았다. 장비 편집을 다녀온 뒤
  같은 화면에서 **보험·기본 장비 프리셋·꾸러미**를 열어 보면 그쪽도 함께 닫혔는지 알 수 있다.
- **같은 결함이 다른 것도 건드렸을 수 있다.** 비워지던 것은 세션만이 아니라
  `ActiveProfileInventoryController` 도였다. 장비 편집을 다녀온 뒤 같은 화면에서 여는 다른 것들
  (보험·기본 장비 프리셋·꾸러미)도 그동안 세션 없이 돌았을 수 있다 — 2026-11-02 에 "기본 장비
  프리셋 화면이 열 수 없습니다 로 거절" 한 것이 같은 모양의 사고였다(StmDetail.cs:105~108 주석).
  이번 수정이 그쪽도 함께 닫는지는 보지 않았다.
- **단위 시험은 걸지 못했다.** 이 자리는 Unity·Harmony 전역 상태라 `SptTeamManager.Tests` 가
  링크할 수 있는 순수 판정이 아니다. 관문은 위의 인게임 확인뿐이다.
