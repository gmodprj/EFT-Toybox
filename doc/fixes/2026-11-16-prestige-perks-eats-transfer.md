# 고침 — 3332 Prestige Perks 가 전송 창고의 물건을 먹는다

- 날짜: 2026-11-16
- 증상: 프레스티지 이전 창고에 가방을 넣고 넘겼는데 **새 시작 우편에 아무것도 없었다.**
- 결론: **우리 모드 문제가 아니다.** 3332 가 설계상 전송 목록을 비우고 코인으로 바꾼다.
- 고침: 자체 서버 모드 `PrestigeTransferKeep` 으로 밖에서 되돌린다. **2026-11-16 실제 프레스티지로 확인됐다**(1,995개가 그대로 왔다).

## 무엇이 사라졌나

3332 가 자기 로그에 그대로 적어 두었다(`user\logs\spt\spt20261116.log`, 10:11 KST).

```
[PrestigePerks] prestige to P3 for 6adc…: carried over: PMC 0, scav 0, unclaimed mail 0, BTR 0;
                transfer: 1 requested, 2002 items with children, 12,211,454,256 roubles, cap 1039 (96 cells)
[PrestigePerks]   transfer 68f98298939080194f06091e x1 (+2001 children): 12,211,454,256 roubles
[PrestigePerks] prestige coins for 6adc…: carried over 0, converted 1039, lost 121075; vanilla transfer mail suppressed
```

가방 **하나**에 든 **2,001개**(122억 루블어치)가 **코인 1,039개**가 되고 **121,075개어치가 상한에
걸려 버려졌다.**

`cap 1039 (96 cells)` 의 **96칸은 우리가 넓힌 8×12** 다. 3332 는 우리 격자를 읽어 **코인 상한만**
키운다 — 우리가 넓힌 창고가 3332 의 환산량을 늘리는 데 쓰인 셈이다.

## 왜 그런가 — 코드

`PrestigePerks.Patches.PrestigeCoinPatch` 가 SPT 본체에 Harmony 를 건다.

```csharp
private static MethodBase TargetMethod() => AccessTools.Method(typeof(PrestigeHelper), "ProcessPendingPrestige");

private static void Prefix(SptProfile oldProfile, PendingPrestige prestige, out PrestigeCoinResult? __state)
    => __state = PrestigeCoinService.Instance?.Prepare(oldProfile, prestige);

private static void Postfix(SptProfile newProfile, PrestigeCoinResult? __state)
{ if (__state.HasValue) PrestigeCoinService.Instance?.Deliver(newProfile, __state.Value); }
```

그리고 `Prepare` 안이 이렇다.

```csharp
LogTransferItems(oldProfile, list);
prestige.Items = new ObtainPrestigeRequestList();   // ← 전송 목록을 비운다
logger.Info($"... vanilla transfer mail suppressed");
```

**SPT 본체가 돌기 전에 목록이 비워진다.** 본체는 `GetItemWithChildren` 으로 집을 것이 없으니 우편도
비어 있다. `config.json` 에 이것을 끄는 항목은 **없다**(`perks`·`activePerks`·`perkAmounts`·`coin`·
`safeCoins`·`bossCoins`·`prestige` 가 전부다).

`Prepare` 가 바닐라 전송을 살려 두는 길은 둘뿐이고 둘 다 사용자 스위치가 아니다.

- `oldProfile.SptData.PendingPrestige != prestige` — 강제 프레스티지일 때
- `!coinMailer.IsAvailable` — 코인 아이템이 등록되지 않았을 때("vanilla item transfer kept")

## 고침 — `PrestigeTransferKeep`

`src/PrestigeTransferKeep` → `SPT_Runtime\user\mods\PrestigeTransferKeep\`.
설치는 `tools/install-prestigetransferkeep.ps1`.

하는 일은 둘이고 **둘 다 설정으로 끈다.**

| 설정 | 기본 | 하는 일 |
| --- | --- | --- |
| `KeepTransferItems` | 켬 | 넘길 목록을 지켜 바닐라 전송 우편이 오게 한다 |
| `SuppressCoinMail` | 켬 | 전송분 코인 우편을 막는다(물건까지 받으면 두 번 받는 셈) |

### 어떻게 지키나 — 3332 를 **사이에 두고** 앞뒤로

같은 메서드(`ProcessPendingPrestige`)에 Prefix 를 **두 개** 건다. Harmony 는 우선순위가 큰 쪽을
먼저 돌린다(`First` 800 · 기본 `Normal` 400 · `Last` 0).

| 순서 | 누구 | 하는 일 |
| --- | --- | --- |
| 800 | 우리 `SaveBefore` | `prestige.Items` 를 떠 둔다 |
| 400 | 3332 `Prefix` | 목록을 비우고 코인을 계산한다 |
| 0 | 우리 `RestoreAfter` | 비워진 것을 보면 도로 끼운다 |
| — | SPT 본체 | 원래 목록대로 우편을 보낸다 |

되돌릴 때 **지금 목록이 떠 둔 것보다 적을 때만** 끼운다 — 아무도 건드리지 않았으면 그대로 둔다.

### 3332 의 어셈블리를 참조하지 않는다

3332 가 업데이트되면 깨지기 때문이다.

- 물건 지키기는 SPT 기본 타입(`PendingPrestige`·`ObtainPrestigeRequestList`)만 쓴다.
- 코인 막기는 `AccessTools.TypeByName("PrestigePerks.Items.PrestigeCoinService")` 로 타입을 찾고,
  **인자가 없는 Prefix**(`static bool SkipCoinMail() => false`)로 `Deliver` 를 건너뛴다 — 3332 의
  타입이 서명에 들어가지 않는다.
- 3332 가 없으면 코인 막기만 조용히 건너뛰고 **로그에 적는다**. 대상 메서드를 못 찾으면 에러를
  남기고 그 패치를 걸지 않는다 — "걸렸는데 안 듣는다"가 가장 나쁘다.

보스·금고 코인(`bossCoins`·`safeCoins`)은 건드리지 않는다. 막는 것은 **전송분 환산**뿐이라 퍽을
살 길은 남는다.

## 되돌린 것

Nomad 를 프레스티지 직전으로 되돌렸다.

| | 상태 |
| --- | --- |
| 되돌리기 전(코인 환산 뒤) | Lv 1 · 프레스티지 3 · 창고 110 — `backup/profiles-after-coin-conversion-2026-11-16/` 에 떠 뒀다 |
| 되돌린 뒤 | **Lv 58 · 프레스티지 2 · 창고 2002** · pending 없음 |

쓴 백업은 `backup/profiles-before-prestige-pack/`(2026-11-16 08:00:25). 창고 2,002개가 3332 기록의
"2002 items with children" 과 맞아 **그 프레스티지 직전 상태가 맞다**는 것을 숫자로 확인했다.

## 확인 — 실제 프레스티지로 됐다 (2026-11-16 11:35, Nomad P2 → P3)

로그가 세 줄로 말해 준다.

```
[PrestigeTransferKeep] 넘길 목록 2건을 떠 뒀다.
[PrestigeTransferKeep] 비워진 목록(0건)을 원래 2건으로 되돌렸다.
[PrestigeTransferKeep] 전송분 코인 우편을 건너뛰었다.
```

**`비워진 목록(0건)`** 이 핵심이다 — 3332 가 실제로 비웠고, 우리가 그 뒤에 도로 끼웠다.

3332 는 여전히 `vanilla transfer mail suppressed` 를 적는다. **그것은 정상이다** — 그 모드는 자기가
비웠다고 믿고 그 줄을 적고, 되돌리는 일은 그 **뒤에** 일어난다. 판정은 그 줄이 아니라 **우편에
물건이 들어 있는지**로 한다.

프로필 실측으로도 맞는다.

| 3332 가 적은 것 | 받은 프로필 |
| --- | --- |
| `transfer 68f98298… x1 (+271 children)` | 가방 `…5bce00` 안에 **271개** |
| `transfer 68f98298… x1 (+1722 children)` | 가방 `…5bcf10` 안에 **1,722개** |

보낸 2 + 1,993 = **1,995개가 하나도 빠짐없이** 왔다(창고 총 2,088). 코인 우편은 오지 않았다 —
3332 는 `converted 1039` 까지 계산했지만 전달을 건너뛰었다.

## 교훈

- **모드를 넣은 뒤 "되던 것이 안 된다"면 새로 들어온 모드부터 본다.** 여기서는 3332 설치가 유일한
  변화였고, 그 모드가 자기 로그에 한 줄로 적어 두었다.
- 남의 모드가 **설계상** 우리와 반대로 동작할 수 있다. 버그가 아니므로 제보할 일이 아니라
  **조정할 일**이다.
- 조정은 남의 파일을 고치지 않고 **밖에서** 한다 — 업데이트에 날아가지 않는다.
