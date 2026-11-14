# 2026-11-14 — "상인 마커" 플리 차단과 프리셋 화면의 유령 모델

사용자가 둘을 물었다. ① `치명적인 오류 — 이 아이템에는 상인 마커(AIOTrader)가 있어 플리 마켓에
등록할 수 없습니다` 가 어느 모드에서 나오는가, ② 무기 프리셋 화면에 프리셋과 상관없는 헤드기어가
함께 그려지는 까닭은 무엇인가. **①은 출처와 조건까지 확정했고, ②는 후보를 셋으로 좁혔다.**

게임·서버가 도는 중에 조사했고 **아무것도 끄거나 고치지 않았다.**

## ① 상인 마커 — Moe-TradeMarker 가 막는다 (확정)

### 누가 적은 문구인가

설치본의 DLL 을 UTF-16 으로 훑어 문구를 **파일 단위로** 찾았다.

```
E:\Spt\BepInEx\plugins\Moe-TradeMarker\MoeTradeMarker.Client.dll        <= 상인 마커
E:\Spt\SPT_Runtime\user\mods\Moe-TradeMarker\MoeTradeMarker.Shared.dll  <= 상인 마커
```

`Shared.dll` 안에 언어별 틀이 나란히 들어 있다(한국어 · 영어 · 체코어 · 이탈리아어 …).

> `이 아이템에는 상인 마커({0})가 있어 플리 마켓에 등록할 수 없습니다.`
> (이탈리아어 판: `Questo oggetto ha un contrassegno commerciante ({0}) e non può …`)

**[Moe-TradeMarker](https://sp-mod.com/mod/2736) 1.2.6** 이고, 6차-A 때 넣었다(`stage6A-2736`,
게임+서버 9파일). 게임의 기본 로케일(`SPT_Data/database/locales/global/kr.json`)에는 이 문구가
없다 — 모드가 제 번역을 들고 있다.

### 어떤 조건에서 나오는가

`SPT_Runtime\user\mods\Moe-TradeMarker\config.json`:

```json
{
  "marking":           { "enabled": true, "disabledTraderIds": [] },
  "ragfairRestriction":{ "enabled": true, "disabledTraderIds": [] }
}
```

- `marking` — **상인에게서 산 물건에 표를 박는다.** 표는 프로필에 남는다:
  `"upd": { "tradeMarker": { "traderId": "68f98298939080194f060927" } }`
- `ragfairRestriction` — **표가 박힌 물건은 플리마켓에 등록할 수 없다.** 예외 목록이 비어 있어
  **모든 상인**이 대상이다.

실제로 거절된 순간이 게임 로그에 남아 있다 — **막는 쪽은 서버다**(그래서 고칠 곳도 서버 설정이고,
**이미 박힌 표도 등록할 때 다시 본다**).

```
2026-11-14 19:20:32.891 +09:00 |Error|backend| <--- Response HTTPS, id [1348]:
  BackendServerSideException: 이 아이템에는 상인 마커(AIOTrader)가 있어 플리 마켓에 등록할 수 없습니다.
2026-11-14 19:20:32.894 +09:00 |Error|Default| Inventory error from backend: (같은 문구)
```

`68f98298939080194f060927` 는 **AIOTrader**(bluehead's AIO Trader, 모드 374)다. 그 상인은
**게임 DB 전부**를 판다(기동 로그: `[AIOTrader] Assort ready: 10918 listings`). 그래서 거기서 산
물건은 무엇이든 표가 박히고, 그 부품으로 조립한 총도 막힌다.

### 지금 프로필에 박힌 표

프로필 11개를 세어 보니 **상인별 표 수**가 이렇다.

| 상인 id | 표가 박힌 아이템 |
| --- | --- |
| `68f98298939080194f060927` **AIOTrader** | **969** |
| `67a5e1c2f3d4b5a6c7d8e9f0` | 414 |
| `68e24cd2607f5c9ae44c27b0` | 163 |
| 그 밖 8곳 | 137 |

**이것은 결함이 아니라 그 모드가 하기로 한 일이다**(실제 타르코프에서 상인 물건을 플리에 되팔지
못하는 것과 같은 규칙). 다만 **AIOTrader 가 모든 것을 팔기 때문에** 그 규칙이 사실상 "플리 판매
금지" 처럼 느껴진다.

### 풀 길 셋 (아직 **아무것도 바꾸지 않았다**)

| 길 | 고칠 곳 | 결과 |
| --- | --- | --- |
| **A. AIOTrader 만 예외**(권함) | `ragfairRestriction.disabledTraderIds` 에 `"68f98298939080194f060927"` | **이미 박힌 969개도 풀린다**(등록할 때 보는 값이다). 다른 상인 물건은 그대로 막힌다 |
| B. 표를 아예 안 박음 | `marking.disabledTraderIds` 에 같은 id | **앞으로 산 것만** 자유롭다. 이미 박힌 969개는 그대로 막힌다. 아이콘도 사라진다 |
| C. 규칙을 끈다 | `ragfairRestriction.enabled: false` | 모든 상인 물건이 플리에 올라간다. 규칙 자체가 없어진다 |

셋 다 **서버 설정**이라 고치면 **서버를 다시 띄워야** 한다. 게임이 도는 중이면 사용자가 멈출
때를 골라야 한다. 아이콘만 끄고 싶다면 그것은 게임 쪽 F12 다(`ShowTraderMarker`).

## ② 프리셋 화면의 유령 모델 — 데이터가 아니라 그리기다 (후보 셋)

### 데이터는 깨끗하다

화면 제목 `ISB 개조 Colt M4A1 5.56x45 돌격소총` 은 **ISB-Aishi 의 아이템**
`6933764646149ffb28ed75e0` 이고(이름은 우리 사전 `item-names-isb-1.json` 이 붙인 것),
그 모드가 함께 등록하는 프리셋 `aishi_mod_m4a1` 의 부품은 **20개 전부 총기 부품**이다 —
손잡이·탄창·리시버·개머리판·조준경·총열·핸드가드·소염기… **헤드기어는 한 줄도 없다.**

화면의 체크상자(`핵심 부품 / 기능성 부품 / 편의성 부품 / 가지고 있는 부품만 사용`)는 바닐라
문구이고, 혼자 영어인 `Use only trader items` 는 **설치된 어떤 모드의 파일에도 없다**(설치본의
DLL·JSON 전부를 훑었다) — 게임이 들고 있는데 한국어 번역이 없는 항목이다.

> 그러므로 **프리셋에 헤드기어가 들어 있어서 그려지는 것이 아니다.** 화면 뒤에 남아 있는
> **다른 3D 미리보기**가 프리셋 위로 함께 그려지는 것이다(흐려진 배경 위에 또렷하게 떠 있다).

### 누가 사람 모델을 그리나

설치본 플러그인 전체에서 `PlayerModelView`(사람 모델 미리보기)를 건드리는 것을 추렸다.

| 플러그인 | 왜 의심스러운가 |
| --- | --- |
| **Hj's Improved Customization UI 1.1.2** (3069) | **첫 번째 후보.** 캐릭터 꾸미기 탭을 새로 그리고 **머리·상반신을 크게 비추는 미리보기**를 쓴다. 기동 로그에 `CustomizationTabClose`·**`CustomizationTabCleanup`**·`CustomizationUpdatePreview` 패치가 올라와 있다 — 치우는 일을 직접 떠맡았다는 뜻이고, 거기서 새면 바로 이 증상이 된다 |
| **WTT - Head and Voice Selector 1.1.0** (1277) | 같은 화면(머리·목소리 고르기)을 건드린다. **설치본이 Forge 최신(1.1.0 미만)보다 높은 손수 넣은 판**이고, Forge 댓글에 "머리·목소리 대신 옷·바지 텍스처가 바뀐다" 는 제보가 있다. 3069 가 같은 일을 하므로 **둘이 겹친다** |
| **UI Fixes 6.0.4** (1342) | `Enable Character Preview Pan/Zoom = true`, `Enable panning on weapons on modding and build screens` — **미리보기 카메라를 직접 만지는** 기능이 켜져 있다 |

우리 `SptTeamManager` 도 `PlayerModelView.Show` 에 패치를 걸지만 **막는 쪽**이고(분대 화면에서만
`Suppress…` 가 켜진다), 화면을 닫을 때·`Deactivate` 에서 꺼진다. 1순위로 보지 않는다.

### 가리는 법 (게임 안에서 2분)

순서대로 하면 **한 번에 하나씩** 갈린다.

1. **언제 나타나는지부터 본다.** 게임을 새로 켜고 **캐릭터 꾸미기 화면을 한 번도 열지 않은 채**
   무기 프리셋을 연다 → 유령이 없으면 **①번 후보(3069)가 거의 확정**이다.
2. 유령이 그대로면 F12 에서 **UI Fixes 의 `Enable Character Preview Pan/Zoom` 을 끄고** 다시 연다.
3. 그래도면 `BepInEx\plugins\WTT-HeadVoiceSelector` 를 잠시 치우고(이름 뒤에 `.off` 를 붙인다)
   게임만 다시 켠다 — 3069 가 같은 기능을 하므로 빠져도 꾸미기는 된다.

각 단계는 **게임 재시작**이 필요하다(F12 한 가지만 빼고). 사용자가 레이드 밖에 있을 때 하면 된다.
