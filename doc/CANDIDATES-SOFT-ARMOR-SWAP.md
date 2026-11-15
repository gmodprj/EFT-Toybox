# 내장 방탄판(소프트 아머)을 떼고 바꾸는 모드 — 2026-11-15 조사

사용자가 물었다. **방탄복·리그의 탈착 가능한 방탄판 말고, 고정으로 박혀 있는 방탄판까지
떼거나 고칠 수 있게 하는 모드**가 예전에 있었던 것 같은데 지금 설치본이나 Forge 에 있는가.

**설치본에는 없다. Forge 에는 사흘 전에 나온 것이 하나 있다.**

## 1. 지금 설치본이 하는 일 — 모두 "탈착 가능한 판" 쪽이다

| 모드 | 하는 일 | 내장 인서트를 떼나 |
| --- | --- | --- |
| [Unrestricted Armor Plate](https://sp-mod.com/mod/1878) 2.1.0 | 아무 방탄판이나 아무 조끼·리그에 꽂게 한다(필터를 푼다) | **아니다** — 이미 열려 있는 칸의 *내용물 제한*만 푼다 |
| [LMSP - Let Me Switch Plates](https://sp-mod.com/mod/2814) 1.0.1 | 레이드 중에 방어구를 벗지 않고 **방탄판**만 바꾼다 | 아니다 |
| [Restore Armor Plates](https://sp-mod.com/mod/2973) 1.0.2 | **탈착 가능한** 방탄판의 최대 내구도를 되돌린다 | 아니다(설명이 `detachable` 로 못박는다) |
| UI Fixes `레이드 중 방탄판 교체` | 입은 방어구의 **방탄판**을 레이드 중에 빼고 넣는다 | 아니다 |
| 우리 `HelmetSlotUnion` | **헬멧** 칸 필터를 이름별 합집합으로 넓힌다(`helmet_top`·`helmet_back` 포함) | 아니다 — 들어갈 수 있는 것을 넓힐 뿐, 잠긴 칸을 열지 않는다 |

즉 **고정 인서트를 여는 모드는 설치본에 하나도 없다.**

## 2. 예전에 있었던 것 — 둘 다 죽었다

| 모드 | 마지막 버전 | 4.1.6 |
| --- | --- | --- |
| [Unrestricted Plates](https://sp-mod.com/mod/1270) | 1.0.1 `~3.8.0` | **없음**(페이지도 "3.11.x 번들 문제로 보류" 라고 적혀 있다) |
| [Configurable Armor Durability](https://sp-mod.com/mod/1828) | 1.1.0 `~3.11.0` | **없음** |

기억하신 것은 이쪽으로 보이고, **지금 판으로는 쓸 수 없다.**

## 3. 지금 있는 것 — [Armor Tweaks and Additions](https://sp-mod.com/mod/3355) 1.0.0

**2026-11-12 첫 공개**(다운로드 260), **SPT 4.1.6 에서 만들었다**, **결속**. 모듈 넷인데
그중 하나가 정확히 물으신 것이다.

> **Soft Armor Swap** — *"방탄복과 장갑 리그의 소프트 아머 인서트, 헬멧의 인서트, 모드로 들어온
> 방어구의 인서트, Manimal 다리 방어구의 인서트를 **잠금 해제한다.** 떼고 바꿔서 칸마다 방어
> 등급과 방어력을 올리거나 내릴 수 있다. 조끼·리그의 **벌점은 채운 칸 수에 비례**하고(절반만
> 채우면 벌점도 절반), 인서트에 **무게가 생긴다.** 전부 채우면 바닐라와 같다. 인서트마다
> **월드 모델**이 있어 레이드에서 떨어뜨렸다 다시 주울 수 있다."*

나머지 셋은 덤이다.

| 모듈 | 하는 일 | 우리 설치본과의 관계 |
| --- | --- | --- |
| Armor Stacking | 방탄복과 장갑 리그를 **겹쳐 입는다**. 탄이 리그판 → 리그 소프트 → 조끼판 → 조끼 소프트 순서로 뚫는다 | **밸런스가 크게 바뀐다.** SVM 의 `Allow armored rigs with armors` 와 호환된다고 적혀 있다 |
| Armor Refurbishment | 소프트 아머·헬멧 인서트를 공장 상태로 되돌리는 키트(Ragman 2레벨, 95k/80k) | **우리가 쓰는 두 모드와 나란히 쓰라고 만들었다** — Wade 의 Plate Restoration(= 설치본 2973)과 Egbog 의 Repair Max Durability(= 설치본 1305)를 바탕으로 삼았다고 제작자가 적었다 |
| Armor Readout | 건강 탭에 부위별 방어력 합계를 띄운다 | ArmorInfo·ArmorClassIcon·Liquidwarp-ArmorExpert 와 **겹칠 수 있다** |

### 집기 전에 볼 것

- **결속 모드다.** 프로필 백업 뒤 **하나만** 넣는다(절차 U1~U8).
- **나온 지 사흘, 다운로드 260.** 지금까지 설치한 것 가운데 가장 어린 축이다.
- **모듈을 따로 끌 수 있는지는 모른다** — 모드 페이지에 설정 이야기가 없다. 받아서 설정 파일을
  봐야 안다. Soft Armor Swap 만 원하면 그것부터 확인한다.
- 제작자가 댓글에서 **`PLATE` 라는 모드와는 호환되지 않는다**고 직접 밝혔다(우리는 쓰지 않는다 —
  설치본의 비슷한 이름은 `Unrestricted Armor Plate`·`Name Plate` 로 다른 모드다).
- 제작자가 **Claude Sonnet 으로 개발을 도왔다**고 페이지에 적어 두었다(판단 거리는 아니고 사실).

## 4. 받아서 열어 봤다 (2026-11-15, 설치는 하지 않았다)

`downloads/candidate-2026-11-15-armor/` · `extracted/candidate-2026-11-15-armor/`.
**51KB 짜리 7z 에 DLL 둘뿐이고 설정 파일은 들어 있지 않다.**

```
BepinEx/plugins/ArmorTweaks.dll                        ← 'BepinEx' (소문자 e). 윈도는 대소문자를 안 가려 그대로 들어간다
SPT_Runtime/user/mods/ArmorTweaks/ArmorTweaks.Server.dll
```

설치 경로 둘 다 **설치본에 없다 — 덮어쓸 것이 없다.**

### 모듈은 따로 끌 수 있다 (물음에 대한 답)

서버 DLL 안에 **처음 기동 때 만들 설정의 틀**이 그대로 들어 있다.

```json
{
  "Modules": {
    "ArmorRefurbishment": true,
    "SoftArmorSwap": true
  }
}
```

- **서버 쪽 둘은 `config.json` 으로 끈다.** 우리에게 중요한 것 — **환급 키트(ArmorRefurbishment)만
  꺼서** 설치본의 `Restore Armor Plates`(2973)·`Repair Max Durability`(1305)와 겹치지 않게 둘 수 있다.
- **게임 쪽 둘(Armor Stacking · Armor Readout)은 F12 로 보인다.** 클라이언트 DLL 이
  `ConfigEntry`·`ConfigFile`·`ConfigDescription` 을 쓰고(= BepInEx 설정), 로그 문구도
  `Armor health readout enabled` / `… DISABLED: …` 로 켜고 끄는 길이 있다. **다만 항목 이름은
  실제로 띄워 봐야 보인다** — 설정 파일이 압축에 없어서 지금은 못 센다.

### 어떻게 여는가

클라이언트 DLL 문자열이 방식을 그대로 보여 준다 — `<Locked>k__BackingField` · `SlotCtorPatch` ·
`Unlocked insert slot …` · `Skipped locked slot …` · `not a built-in insert`. **칸의 `Locked` 를
푸는 패치**이고, 못 찾으면 `SoftArmorSwap DISABLED: Slot.Locked backing field not found.` 로
**스스로 꺼진다**(게임 판이 바뀌어도 조용히 깨지지 않는다는 뜻이라 좋은 신호다).

무게·벌점도 문자열로 확인된다 — `speedPenaltyPercent` · `mousePenalty` ·
`weaponErgonomicPenalty`, `Stock insert … kg (vanilla …)`, 그리고 조끼 자체 무게가 인서트 합보다
가벼우면 **0 으로 깎는다**는 처리까지 있다.

### 넣는다면

1. 프로필 백업 → `install-and-test.ps1` 로 **이것만** 넣고 서버 시험(결속 모드다).
2. 첫 기동 뒤 `SPT_Runtime\user\mods\ArmorTweaks\config.json` 에서 **`ArmorRefurbishment: false`**
   로 둘지 정한다(2973·1305 와 역할이 겹친다).
3. 게임을 띄워 F12 에서 **Armor Stacking** 을 켤지 정한다 — 그것만이 밸런스를 크게 바꾼다.
4. `check-lang-coverage.mjs` 로 F12 항목이 는 만큼 사전을 채운다.

## 5. 넣었다 — 12차 (2026-11-15)

게임·서버가 모두 꺼져 있는 것을 확인하고 넣었다. 프로필 11개를 먼저 떠 뒀다
(`backup/profiles-before-stage12-2026-11-15/`).

| 항목 | 값 |
| --- | --- |
| 기록 | `installed-stage12-A-3355-ArmorTweaks.txt` — 파일 **2개** |
| 서버 시험 | **`SERVER STARTED in 192s`** · `ModLoader: 총 176 개`(175 → **+1**) · 거부 0 · `Could not add` 고유 48 · 번들 1 |
| 로그가 찍은 것 | `모드 Armor Tweaks 버전 1.0.0 (GUID: kommie.armortweaks)` · `ArmorRefurbishment: 2 kits added (2 trader offers)` · **`Armor Tweaks (Soft Armor Swap): 458 inserts named`** |
| 만들어진 설정 | `SPT_Runtime\user\mods\ArmorTweaks\config.json` — `{"Modules":{"ArmorRefurbishment":true,"SoftArmorSwap":true}}` |

**인서트 458개에 이름이 붙었다** — 잠금이 실제로 풀렸다는 뜻이다(Soft Armor Swap 이 일했다).

환급 키트는 **켜 둔 채로 두었다.** 제작자가 설치본의 2973·1305 를 바탕으로 만들었다고 밝혔고
역할도 갈린다(저쪽은 **탈착 방탄판**, 이쪽은 **소프트 아머·헬멧 인서트**). 겹치는 느낌이 들면
`config.json` 에서 `"ArmorRefurbishment": false` 로 끄면 된다.

### 도구도 손봤다

`gen-installed-mods.mjs` 가 **11차까지만 찍고 있었다** — 단계 목록이 `['1'…'11']` 로 손으로 적혀
있어 12차가 통째로 빠졌다. `'12'` 를 더하고 제목도 넣었다. 다음 단계를 열 때도 **두 곳을 같이
고쳐야 한다**(`stageTitle` 과 이 목록).

### 아직 안 본 것 — 게임 쪽

- **플러그인 수가 249 → 250 으로 느는 것이 정상이다**(`ArmorTweaks.dll`).
- F12 에 **Armor Stacking · Armor Readout** 항목이 생긴다. 켤지 끌지는 사용자가 정한다 —
  **Stacking 만 밸런스를 크게 바꾼다**(조끼+장갑 리그 겹쳐 입기).
- 그만큼 **F12 한글화에 구멍이 난다.** 게임을 한 번 띄운 뒤 `check-lang-coverage.mjs` 로 세고 채운다.
- 인게임에서 볼 것: 보라색으로 잠겨 있던 인서트 칸이 **실제로 열리는지**, 뗐을 때 **무게와 벌점이
  줄어드는지**.

## 6. 게임 쪽 실측 (2026-11-15 12:47)

| 본 것 | 값 |
| --- | --- |
| 플러그인 | **250** (`Loading [` 250줄) — 249 에서 **예상대로 +1** |
| 게임 오류 | **`[Fatal` 0 · `[Error` 1** (알려진 DirectReload+Fika) |
| 서버(사용자 기동) | `총 176 개` · 거부 0 · 고유 48 · 번들 1 · `Armor Tweaks (Soft Armor Swap): 458 inserts named` |
| 클라이언트 로그 | `Hooked ArmorComponent.ShotMatches and Player.ApplyShot` · `Armor health readout enabled` · `Armor Refurbishment Client loaded` |
| **`SoftArmorSwap DISABLED`** | **0줄** — 잠금 푸는 패치가 걸렸다는 뜻이다(못 걸면 스스로 그 줄을 찍는다) |

### F12 항목 일곱 — 모듈 넷을 전부 여기서 끈다

`BepInEx\config\kommie.armortweaks.cfg` 가 생겼다.

| 묶음 | 항목 | 기본값 |
| --- | --- | --- |
| `Modules` | **ArmorStacking** · **SoftArmorSwap** · **ArmorRefurbishment** · **ArmorHealthReadout** | 전부 `true` |
| `ArmorStacking` | `ExtraAbsorption`(0~100) · `CarryToShot` · `DebugLog` | 0 · true · false |

**앞서 "게임 쪽 둘은 F12 로 보인다" 고 적었던 것이 맞았고, 실제로는 넷 다 F12 에 있다.**
서버 `config.json` 의 두 손잡이는 그와 **별도**다(설명에도 "서버 모드에 자기 스위치가 있다" 고
적혀 있다) — 끄려면 **둘 다** 꺼야 확실하다.

겹쳐 입기(`ArmorStacking`)만 밸런스를 크게 바꾸므로, 소프트 아머만 원하면 그 하나를 끄면 된다.

### 한글화도 메웠다

새 `.cfg` 가 192개째로 들어오며 **항목 7개가 사전 없이 떴다.** `kommie.armortweaks.json` 을 새로
써서 **5,184/5,184**(사전 187개)로 돌려놨다. 사전만 덮었으므로 **F12 를 다시 열면 한글로 보인다**.
