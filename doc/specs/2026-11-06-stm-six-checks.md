# STM 편집기에 남은 여섯 — 한 번에 보는 점검표 (2026-11-06)

[STM-HANDOVER.md](../STM-HANDOVER.md) §1-b 의 여섯 항목을 **편집기 한 번 여는 동안 다 보도록** 묶었다.
그 표는 "볼 것" 만 적고 ④⑤⑥ 의 로그 칸이 `-` 였다. 여기서 셋을 메운다:

1. **어느 로그에 찍히나** — ①②③ 은 **게임 쪽**(`E:\Spt\BepInEx\LogOutput.log`)이다. 서버 로그가 아니다.
   편집기 코드는 전부 클라이언트(`pitFireTeam.Log`)다. 저장이 돌면 그때만 서버 로그가 쓰인다.
2. **눈으로만 되는 것** — ④⑤⑥ 은 로그 문구가 **코드에 없다**(소스 실측). 그래서 화면을 봐야 하고,
   대신 **어긋났을 때 무엇이 보일지**를 적었다.
3. **미리 치운 전제** — 아래 "AI 가 먼저 재 둔 것".

## 순서 (편집기 한 번, 위에서 아래로)

프로필 타일 → 상세 화면 → `팀원 장비 꾸러미` 가 아니라 **장비 편집기**를 연다.

| | 할 것 | 통과 | 어긋남 |
|---|---|---|---|
| **①** | 편집기를 **열기만 한다**(아무것도 만지기 전) | 게임 로그에 `[STM] Stash fit (before):` 와 `[STM] Stash fit (after):` **한 쌍**. 뒤쪽 줄에 남은 여백이 적힌다 | 격자 오른쪽이 잘림 · 빈 띠 · 왼쪽이 창 밖 |
| **②** | `SEARCH` 칸이 "창고" 자리에 서 있는지 보고 한 번 눌러 본다 | `[STM] Stash search tab wired for the teammate gear editor.` | `[STM] Stash search tab was not wired (tab=…, stash=…` · `Could not wire the stash search tab:` · `Could not open the stash search window:` |
| **③** | 스태시 **빈 칸**에서 드래그 | `[STM] Selection box (UI Fixes DrawMultiSelect) enabled for the teammate gear editor` + 사각형이 그려진다 | `No GraphicRaycaster above the editor overlay` · `PreloaderUI raycaster was not found` · `Could not enable the selection box:` |
| **④** | 정렬 단추를 누른다 — **아래 경고를 먼저 읽어라** | 아이템이 실제로 다시 배치된다 | 아무 일도 안 일어남(옛 증상) · 또는 저장에서 거절(아래) |
| **⑤** | 스태시 아이템 우클릭 → `고정` → 저장 → 다시 열기 | `고정` 이 뜨고, 저장 뒤에도 고정이 남는다 | `고정` 이 없음 |
| **⑥** | 우클릭 메뉴에 `채우기`·`장착`/`분리`·`개조`·`분해` 가 뜨는지. **`개조`는 저장까지** 간다 | 네 항목이 보이고 개조 저장이 받아들여진다 | 메뉴가 짧다(= 문맥 종류 관문에 걸린 것) |

### ⑤ 에서 `잠금` 은 **없는 것이 정답이다**

`IsStmPinInteractionAllowed` 는 `SetPin`·`SetUnPin` 만 허용한다 — `SetLock`·`SetUnLock` 은 **일부러 빼
놓았다**. 서버의 `PreservePinLockState` 가 제출본의 잠금 상태를 저장된 값으로 덮어쓰기 때문에, 단추를
주면 "풀었는데 조용히 없던 일이 되는" 변화가 된다. **`잠금` 이 보이면 그것이 결함**이다.

### ④ 경고 — UI Fixes 가 정렬 **전에 묶음을 합친다**

설치본 설정 `com.tyfon.uifixes.cfg:443` 이 `Combine Stacks Before Sorting = true` 다(기본값). 주석도
"정렬이 실패해도 이것은 되돌리지 않는다" 고 적어 두었다.

그런데 STM 저장은 **아이템 id 보존**이 불변식이다. 합치면 id 하나가 사라지므로 클라이언트가 보내기도
전에 거절한다 — `shared/GearEditBatchPlanner.cs:75`:

```
편집 중 사라진 아이템 '<id>' 이(가) 있어 저장하지 않습니다. 버리거나 합치는 조작은 저장되지 않습니다.
```

**데이터가 깨지는 길이 아니다**(닫힌 쪽으로 넘어진다). 다만 ④ 를 "정렬이 되는가" 로 보려면 둘 중 하나:

- `Combine Stacks Before Sorting` 을 **끄고** 누른다, 또는
- 합칠 묶음이 **없는** 팀원 스태시에서 누른다.

끄지 않고 눌러 위 거절 문구가 나오면 **정렬은 돌아간 것**이고 막힌 것은 합치기다 — ④ 의 실패가 아니다.
헷갈리지 않게 설정을 끄고 보는 쪽을 권한다.

## AI 가 먼저 재 둔 것 (사용자가 할 필요 없음)

| 전제 | 결과 | 어디서 |
|---|---|---|
| ③ 은 UI Fixes 가 있어야 돈다(없으면 **조용히 넘어간다**) | **있다** — `Tyfon.UIFixes 6.0.4` 로드됨 | `LogOutput.log` 의 `Loading [Tyfon.UIFixes 6.0.4]` |
| 그 모드의 다중 선택이 켜져 있나 | **켜져 있다** — `Enable Multiselect = true` | `com.tyfon.uifixes.cfg:372` |
| 다중 선택과 `Auto-open Sorting Table` 은 **함께 못 쓴다** | 충돌 없음 — `Auto-open Sorting Table = false` | 같은 파일 `:581` |
| ② 의 성공 로그가 코드에 있나(표에는 문구만 적혀 있었다) | **있다** | `Patches/OtherPlayerProfileScreenPatch.StmGearEditor.cs:877` |
| ④⑤⑥ 에 로그 문구가 있나 | **없다** — 눈으로 봐야 한다 | 소스 전체 grep |
| 여섯 중 이미 돈 것이 있나 | **없다** — 오늘 기동의 `[STM]` 줄은 꾸러미 쪽 5줄뿐이고 `Stash fit` 은 0건 | `LogOutput.log` (2026-11-06 18:24 기준) |

## 끝난 뒤

로그를 그대로 주면 내가 여섯을 판정해 [STM-HANDOVER.md](../STM-HANDOVER.md) §1-b 를 닫는다. 항목별로
집을 문구는 위 표의 "통과"·"어긋남" 칸이 그대로다.
