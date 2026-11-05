# 레이드 종료 후 게임 강제 종료 (2026-11-05 13:27)

레이드가 끝나고 메뉴로 돌아온 뒤 게임이 꺼졌다. **원인은 아직 특정되지 않았다.** 아래는 확인된 사실과 범위를 좁히는 방법이다.

## 확인된 사실

| 시각 | 무슨 일 |
| --- | --- |
| 13:26:24~30 | 레이드 종료 정리. `totalMemoryBeforeCleanUp 5675.32`(MB) → `GC::Collect` → `GC::Collect Aggressive` → `collectedMemoryGB 1.695` |
| 13:26:31 | `AIPlaceLogicPartisan.PlayerWithWorstKarma()` NullReferenceException (EFT 자체 코드) |
| 13:26:32 | `SelectProfile` — 메뉴 복귀 |
| 13:26:34 | `item ... doesn't fit to grid GridView in item [WTT-PackNStrapServer]_(WARTE...)` 2건 |
| 13:27:05~06 | 마지막 서버 요청 `/client/game/profile/items/moving` — **성공**. 스태시에서 아이템을 옮기던 중이다 |
| 13:27:03 | `LogOutput.log` 마지막 줄 |
| **13:27:09** | **`EscapeFromTarkov.exe` 충돌** |

Windows 가 기록한 충돌 서명(이벤트 ID 1000 / WER).

```
오류 모듈: mono-2.0-bdwgc.dll  (E:\Spt\MonoBleedingEdge\EmbedRuntime\)
예외 코드: 0xc0000005 (접근 위반)
예외 오프셋: 0x27a139
덤프: C:\ProgramData\Microsoft\Windows\WER\ReportArchive\AppCrash_EscapeFromTarkov_aa93a0de...\
```

- 레이드 자체는 **정상 종료**했다 — `Raid Ended` → `Raid CleanUp Finished`, ORBIT `Dispose complete`, PMC 카르마 저장(`karma=0.175`).
- BepInEx 로그에 `[Fatal]` 이 없고 관리 예외도 남기지 않은 채 끊긴다. **하드 크래시**다.
- 충돌 지점은 레이드 종료 전환이 아니라 **메뉴·스태시**이고, 아이템 이동 요청 직후다.

## 배제한 것

- **메모리 고갈이 아니다.** 물리 메모리 31.9GB 중 여유 18.9GB, 페이지 파일 최대 사용 6.4GB(할당 57GB). EFT 는 정리 전 5.6GB 였다.
- **10차 때문이라고 말할 수 없다.** WER 보관함에 EFT 충돌이 둘 있는데 하나가 **2026-11-01 18:59**(모듈 `UnityPlayer.dll`)로 **10차 시작 전**이다. 이 설치본에서 충돌은 처음이 아니다.
- 같은 시각 `NahimicSvc32.exe`(A-Volute 오디오)도 13:27:39 에 죽었지만 **EFT 보다 30초 뒤**다. EFT 종료의 결과로 보이고 원인으로 보기 어렵다.

## 용의자

`mono-2.0-bdwgc.dll` 접근 위반은 **관리 코드의 메모리 손상 또는 GC 시점 실패**에서 나온다. 로그만으로는 어느 모드인지 지목할 수 없다. 증거가 있는 순서대로 적는다.

1. **WTT - Menu Overhaul (1775, 10-D)** — 충돌 직전까지 **유일하게 오류를 내고 있던 모드**다. `[Layout] EnvironmentUISceneFactory GameObject not found in Environment UI.` 가 이번 기동에서 **46번** 찍혔고, 마지막 네 줄도 이것이다. 메뉴 화면을 직접 뜯어고치는 모드이고 충돌 지점이 메뉴다. 다만 **경고이지 예외가 아니다** — 상관이지 인과가 아니다.
2. **인벤토리 이동을 건드리는 최근 추가분** — 마지막 요청이 `items/moving` 이었다. 10-C 의 MergeConsumables·Loot In Vicinity·Cabinet Looter, 10-D 의 Inventory Walker 가 이번 차수에 들어왔다. 로그에 `[Inventory Walker] Movement back on the vanilla path.` 가 있다.
3. **그 전부터 있던 무엇** — 11-01 충돌이 10차 이전이므로, 10차와 무관한 원인이 따로 있을 수 있다.

## 좁히는 방법

충돌이 5일에 두 번꼴이라 **한 번 돌려 보고 괜찮다고 판정할 수 없다.** 그래서 순서를 둔다.

1. **재현 조건을 먼저 모은다.** 다음에 또 꺼지면 그때의 시각과 "무엇을 하던 중이었는지"(스태시 정리 중인지, 상인 화면인지)를 적는다. 같은 자리에서 두 번 나면 범위가 확 줄어든다.
2. **1순위부터 빼 본다.** Menu Overhaul 은 설정에 전체 끄기 스위치가 없다(개별 항목만 있다). 기록으로 지운다.
   ```
   node C:\...\scratchpad\remove-by-record.mjs installed-stage10-D-1775-WTTMenuOverhaul.txt --apply
   ```
   기록 9줄이고 프로필에 심는 것이 없어 되돌리기가 싸다. 지운 뒤 메뉴 경고 46건이 사라지는지 먼저 보고, 며칠 플레이해 본다.
3. 그래도 나면 10-D(UI 12개) → 10-C(편의 11개) 순으로 단계째 빼 본다. 10차는 단계마다 기록이 갈려 있어 한 번에 되돌릴 수 있다.
4. 덤프를 직접 보려면 WER 보관함의 `.dmp` 를 WinDbg 로 열어 관리 스택을 봐야 한다. 모노 프레임이라 기호 없이는 읽기 어렵다.

## 함께 적어 둘 것

- `Trying to reload asset from disk that is not stored on disk` 가 13:26:17~19 에 3건 있었다. 번들을 더하는 모드가 많을 때 나오는 메시지다. 충돌과 이어지는지는 확인되지 않았다.
- `bone mod_core not found in GameObject weapon_sig_mcx_spear_68x51...` 는 레이드 중에도 계속 나오던 기존 메시지다.
