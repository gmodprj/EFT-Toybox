# SVM — 죽어 있던 화폐 스택 네 줄을 끈다

2026-10-28. SVM 프리셋 `Noname` 의 `EnableCurrency` 를 `true` → **`false`** 로 바꿨다.

## 무엇이 잘못돼 있었나

SVM 과 BarterItemsStacks 가 **같은 네 아이템의 스택 한도**를 쓰고 있었고, SVM 쪽이 전부 덮였다.
설정 화면에는 SVM 값이 그대로 보이는데 게임에는 BarterItemsStacks 값이 들어가는 상태였다.

| 아이템 | SVM 설정 | BarterItemsStacks 설정 | 실제 (DB) |
| --- | --- | --- | --- |
| 루블 | 10,000,000 | 50,000,000 | **50,000,000** |
| 달러 | 5,000,000 | 50,000,000 | **50,000,000** |
| 유로 | 5,000,000 | 50,000,000 | **50,000,000** |
| GP 코인 | 2,000 | 5,000 | **5,000** |

`EnableCurrency` 가 가리는 것은 정확히 이 네 줄(`RubStack`·`DollarStack`·`GPStack`·`EuroStack`)
이다. 프리셋 안에서 바로 아래에 붙어 있고, 다른 항목은 가리지 않는다.

> 이전 문서([../REVIEW-LOAD-ORDER.md](../REVIEW-LOAD-ORDER.md) 2번)에 달러·유로를 500,000,
> GP 코인을 1,000 으로 적어 뒀는데 **틀렸다.** 프리셋 파일의 실제 값은 위 표와 같다.

## 조치

값을 BarterItemsStacks 와 **맞추는** 대신 **끄는** 쪽을 골랐다. 값을 맞추면 같은 필드에 주인이
둘인 상태가 그대로 남아, 한쪽만 바뀌면 다시 조용히 어긋난다. 끄면 **BarterItemsStacks 가 유일한
주인**이 된다(그 모드는 460건을 관리하고 452건이 반영되고 있다).

```jsonc
"EnableCurrency": false,   // 이 아래 RubStack·DollarStack·GPStack·EuroStack 네 줄이 꺼진다
```

값 네 줄은 **지우지 않고 그대로 뒀다.** 나중에 BarterItemsStacks 를 빼면 다시 켜기만 하면 된다.

## 확인

서버를 다시 띄워 `POST /client/items` 로 최종 DB 를 받아 확인했다 — 네 아이템 모두
**BarterItemsStacks 값 그대로**(50,000,000 ×3, 5,000). 즉 SVM 의 네 줄은 정말로 아무 일도 하지
않고 있었다.

## 되돌리기

패치 직전 프리셋이 `backup/svm-presets/Noname-before-currency-cleanup.json` 에 있다.
`E:\Spt\SPT_Runtime\user\mods\[SVM] Server Value Modifier\Presets\Noname.json` 로 덮어쓰고
서버를 다시 켜면 된다. 한 줄만 되돌리려면 `EnableCurrency` 를 `true` 로 바꾼다.

## 주의

- 파일에 **BOM 이 있다.** 고칠 때 지우지 말 것(고친 뒤 확인했다 — 그대로 있다).
- SVM 설정은 보통 `E:\Spt\Greed.exe` 로 고친다. Greed 는 이 프리셋을 읽어 화면에 보여주므로
  `EnableCurrency` 가 꺼진 것으로 보일 것이다. **Greed 에서 다시 저장할 때 이 항목을 확인한다** —
  모르고 켜면 원래 상태로 돌아간다.
