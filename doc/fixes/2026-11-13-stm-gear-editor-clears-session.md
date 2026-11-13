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

## 4. 아직 확인하지 못한 것

- **인게임에서 다시 보지 않았다.** 제보 당시 게임이 돌고 있었고 **DLL 이 잠겨 설치할 수 없다**
  (돌고 있는 DLL 은 덮어쓰기가 조용히 실패한다). 게임을 끄고 설치한 뒤,
  같은 순서(장비 편집 → 나옴 → 스태시 교환)로 **격자가 바로 뜨는지**와 로그에 위 경고 두 줄이
  **나지 않는지**를 본다.
- **같은 결함이 다른 것도 건드렸을 수 있다.** 비워지던 것은 세션만이 아니라
  `ActiveProfileInventoryController` 도였다. 장비 편집을 다녀온 뒤 같은 화면에서 여는 다른 것들
  (보험·기본 장비 프리셋·꾸러미)도 그동안 세션 없이 돌았을 수 있다 — 2026-11-02 에 "기본 장비
  프리셋 화면이 열 수 없습니다 로 거절" 한 것이 같은 모양의 사고였다(StmDetail.cs:105~108 주석).
  이번 수정이 그쪽도 함께 닫는지는 보지 않았다.
- **단위 시험은 걸지 못했다.** 이 자리는 Unity·Harmony 전역 상태라 `SptTeamManager.Tests` 가
  링크할 수 있는 순수 판정이 아니다. 관문은 위의 인게임 확인뿐이다.
