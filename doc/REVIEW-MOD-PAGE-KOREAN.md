# 모드 페이지 한글화 — 카드와 페이지 안쪽

서버 웹 첫 화면(SIC)의 "모드 페이지" 구역과, 거기서 들어가는 각 모드 페이지를 한국어로 바꿀 수 있는지 조사한 기록.
조사 시점 2026-11-16, SPT 4.1.6, 설치본 `E:\Spt`. 본체 화면 한글화는 `doc/SERVER-WEB-KOREAN.md` 에 있다.

## 결론 먼저

| 대상 | 닿는가 | 방법 |
| --- | --- | --- |
| 카드 제목·설명 (15개) | **닿는다** | `ModPage` 싱글턴을 제자리에서 고친다. 남의 DLL 을 건드리지 않는다 |
| 페이지 안쪽 — Blazor 페이지 전부 | **닿는다** | 화면에서 바꾼다. 모든 페이지가 공통으로 부르는 스크립트에 번역기를 끼운다 — 6번 |
| 페이지 안쪽 — QuestCodex | 이미 한국어 | 모드가 한국어를 들고 있다 |
| 페이지 안쪽 — UltimateFlea | **아직 못 닿는다** | 자체 정적 HTML(`app.html`)이라 본체 호스트 스크립트를 안 부른다 |
| `/isb` | **아직 못 닿는다** | 페이지가 거의 비어 온다. 따로 봐야 한다 |

> 2026-11-16 갱신. 처음에는 "어셈블리 패치 외에 길이 없다"고 적었는데 **틀렸다.**
> DLL 을 고치는 대신 **브라우저에서 바꾸면** 된다. 아래 2절의 "못 닿는다"는 *로케일 파일로는* 못 닿는다는 뜻이다.

## 1. 카드 글귀는 로케일을 거치지 않는다

`locales/web/en.json` 에 모드 카드용 키는 **구역 제목 하나뿐**이다.

```
"landing-page-mod-page-category": "Mod pages",
```

카드 안의 글귀가 어디서 오는지 `SPTarkov.Server.Web.dll` 을 풀어 확인했다.

```csharp
// SPTWeb.InitializeSptBlazor — 각 모드의 IModBlazorMetadata 를 그대로 담는다
builder.Services.AddSingleton((IReadOnlyList<ModPage>)( ... select new ModPage {
    Name = mod.ModMetadata.Name,
    HomePage = NormalizeHomePage(mod.WebMetadata.HomePage),
    Description = mod.WebMetadata.HomePageDescription }).ToList());

// LandingPage — 로케일 조회 없이 그대로 찍는다
_modPages = ModPages.Select(page => new LandingPageLink(
    page.Name, page.HomePage, page.Description ?? WebLocalizationService.GetText("desc-not-set-author"), ...));
```

즉 글귀는 각 모드 DLL 안에 리터럴로 박혀 있다. `ko.json` 에 키를 아무리 넣어도 닿지 않는다.

### 그런데 길이 있다

세 가지가 겹쳐서 가능해진다.

| 근거 | 내용 |
| --- | --- |
| `ModPage` 는 record 가 아니다 | `Name`·`Description` 에 **setter 가 있는** 보통 클래스다(`SPTarkov.Server.Core.Models.Spt.Launcher.ModPage`) |
| 목록이 DI 싱글턴이다 | `AddSingleton((IReadOnlyList<ModPage>)...)` — 모드가 생성자로 주입받을 수 있다 |
| 기동 순서가 맞는다 | `RegisterSptServicesAsync` → `InitializeSptBlazor` → `builder.Build()` → `RunPreSptLoadCallbacks` (`Program.cs`). 모드의 `OnLoadAsync` 시점에 목록이 이미 있다 |

그래서 **남의 DLL 을 건드리지 않고** 제자리에서 글귀만 바꿀 수 있다. 이게 `src/ModCardKorean` 이다.

주의할 점 하나: `RunPreSptLoadCallbacks` 는 `TypePriority` 가 **0 이상 200000 미만**인 `IOnLoad` 만 부른다.
`Injectable` 의 기본값은 `int.MaxValue` 라, 비워 두면 조용히 안 돈다.

## 2. 페이지 안쪽 — 12개 모드를 각각 봤다

먼저 공통 확인: 카드를 내보내는 모드 14개의 DLL 을 전부 뒤져 **`WebLocalizationService` 를 쓰는 곳이 하나도 없다**.
`ServerLocalisationService` 를 쓰는 3개(BIS·CCI·Lots of Loot)는 서버 콘솔 로그용이라 웹 화면과 무관하다.
→ 본체 로케일 파일로 닿을 수 있는 모드 페이지는 하나도 없다.

그 다음 모드별로:

| 모드 | 화면 문구의 출처 | 판정 |
| --- | --- | --- |
| DefinitiveWeaponVariants | `config/configSchema.json` 을 런타임에 읽는다. `DisplayName` 192줄, `Description` 120줄 | **파일로 닿는다** |
| The Blacklist | `Home.razor` → `AddMarkupContent(56, "<strong>General settings</strong>...")` 리터럴 | 못 닿음 |
| Barter Items Stacks | 표 머리글은 리터럴, 내용 대부분은 **아이템 이름**(모드가 `en` 글로벌 로케일에서 읽는다) | 못 닿음 |
| Cultist Circle Improvements | 리터럴 + `wwwroot/files/ReleaseNotes.json`(제작자 변경 이력) | 못 닿음 |
| Enable Labyrinth | 같음 | 못 닿음 |
| Lots of Loot Redux | 리터럴 + 변경 이력 | 못 닿음 |
| jerobackpack | 리터럴. `config/config.json` 은 ID·숫자뿐이라 화면 문구가 아니다 | 못 닿음 |
| ISB Special Forces | 리터럴 | 못 닿음 |
| ECOTI | 리터럴 + `wwwroot/js/cotiViewer.js` | 못 닿음 |
| ORBIT | 리터럴 | 못 닿음 |
| SAIN | 리터럴. 프리셋 이름은 사용자 데이터다 | 못 닿음 |
| Skills Extended | 웹 페이지는 리터럴. **다만 게임 쪽은 다르다 — 3번** | 못 닿음 |
| QuestCodex | 모드가 한국어를 들고 있다(`warm-langs.json` 이 `["kr","en"]`, 번들에 한국어 문자열과 Pretendard 폰트) | **이미 한국어** |
| UltimateFlea | 빌드된 SPA. `wwwroot/app.js`(223KB) 안의 문자열 | 기술적으론 닿지만 권하지 않음 |

### DWV 가 유일하게 깨끗한 경우

`configSchema.json` 은 설정 키(`Property`)와 화면 글귀(`DisplayName`·`Description`)가 **따로 적혀 있다.**

```json
{
  "Property": "Generate",
  "DisplayName": "Generate Quality Tiers",
  "Description": "Choose which weapon variant quality tiers are generated. ...",
}
```

`Property` 를 건드리지 않고 `DisplayName`·`Description` 만 번역하면 동작은 그대로고 화면만 한국어가 된다.
DLL 이 이 파일을 런타임에 읽는 것도 확인했다(DLL 문자열에 `configSchema.json` 이 있다).

**값: 312줄. 비용:** 남의 모드 폴더 안의 파일을 덮어쓰는 일이다. 이 프로젝트 절차는 덮어쓰기가 생기면 멈추게 돼 있고,
DWV 가 업데이트되면 번역이 날아간다. 하려면 `src/` 에 번역본을 두고 설치 스크립트로 넣되,
원본을 `backup/` 에 뜨고 설치 기록을 남기는 방식이어야 한다. **이번 작업에서는 하지 않았다.**

### 나머지를 하려면 어셈블리 패치뿐이다

`doc/SERVER-WEB-KOREAN.md` 4절에서 본체에 대해 이미 기각한 경로와 같다. 모드마다 DLL 을 다시 써야 하고,
모드가 업데이트될 때마다 되풀이해야 한다. 분량도 적지 않다(프리렌더 기준 단어 수: BIS 3096, jerobackpack 852,
Skills Extended 730, Lots of Loot 593, ORBIT 407 — 대화형으로 더 그려지는 부분은 여기 안 잡힌다). 권하지 않는다.

## 3. 조사 중에 따로 나온 것 — Skills Extended 게임 로케일

웹 페이지와는 별개로, Skills Extended 는 **게임 쪽 문자열을 로케일 파일로 들고 있다.**

```
user/mods/SkillsExtended/Resources/Locales/
  ch.json  en.json  es.json  fr.json  ge.json  kr.json  po.json  ru.json
```

`DatabaseImporter` 가 이걸 SPT 글로벌 로케일에 변환기로 얹는다(`en` 을 먼저 깔고 각 언어로 덮는다).
그런데 **`kr.json` 9,854 바이트 / `en.json` 56,836 바이트** — 한국어가 상당히 비어 있다.
이건 서버 웹이 아니라 게임 화면 문제라 이번 범위 밖이지만, 별도 과제로 둘 만하다.

## 4. 한 일

`src/ModCardKorean` — 서버 모드 하나. 자체 페이지가 없으므로 `IModBlazorMetadata` 를 구현하지 않는다(카드를 늘리지 않는다).

| 파일 | 내용 |
| --- | --- |
| `ModCardLocalizer.cs` | `IOnLoad` 에서 `IReadOnlyList<ModPage>` 를 받아 제자리 수정. 실패해도 던지지 않는다 |
| `mod-cards.ko.json` | 카드 경로를 키로 한 사전 15줄. 모드 이름은 고유명이라 원문 유지, 설명만 한국어 |
| `ModMetadata.cs` | `IModMetadata` 만 구현 |
| `tools/install-modcardkorean.ps1` | 설치/제거(`-Remove`). 서버·게임이 켜져 있으면 중단한다 |
| `tools/check-mod-card-locale.mjs` | 돌고 있는 서버의 카드 목록과 사전을 대조 |

사전에 없는 모드는 **원문 그대로 둔다.** 새 모드가 들어와도 깨지지 않고, 검사 도구가 "사전에 없음"으로 잡아 준다.

검사 도구는 서버를 읽지 못하면 `OK` 가 아니라 **exit 2 로 "판정하지 않는다"** 를 낸다. 못 읽은 것을 통과로 치지 않는다.

## 5. 확인한 화면 (2026-11-16)

설치하고 서버를 다시 켜서 실제로 렌더해 확인했다.

```
[ModCardKorean] 모드 카드 15/15 개를 한국어로 바꿨다.
```

```
$ node tools/check-mod-card-locale.mjs
서버 카드 15 개 / 사전 15 줄
OK — 15/15
```

첫 화면의 카드 15개가 전부 한국어다(`/bis` "물물교환 품목이 한 칸에 얼마나 겹쳐 쌓이는지 정한다",
`/theblacklist` "플리마켓 금지 품목을 풀고 가격을 다시 맞춘다" …). 모드 이름은 고유명이라 원문 그대로다.

### 첫 시도는 실패했고, 안전장치가 받아 냈다

사전의 `_comment` 를 문자열로 뒀더니 `CardText` 로 역직렬화되지 않아 터졌다.
`OnLoadAsync` 가 예외를 삼키고 로그에 남기도록 해 둔 덕에 **서버는 정상 기동했고 카드만 영어로 남았다.**

```
[Error][ModCardKorean.ModCardLocalizer] [ModCardKorean] 카드 한글화에 실패했다. 원문 그대로 둔다:
  The JSON value could not be converted to ... Path: $._comment
```

고친 방식: 읽는 쪽에서 `_` 로 시작하는 키와 객체가 아닌 값을 먼저 걸러 낸다. 주석 줄의 모양이 달라도 터지지 않는다.

## 6. 페이지 안쪽 — 응답에 끼워 넣는다

DLL 을 모드마다 패치하는 길은 버렸다. 업데이트마다 다시 해야 하고 분량도 감당이 안 된다.
**서버가 내보내는 모든 HTML 응답에 번역기 스크립트 한 줄을 끼운다.**

### 왜 미들웨어인가 (두 번 갈아탔다)

처음에는 디스크의 `SPT_Data/wwwroot/_framework/blazor.web.js` 끝에 로더를 붙였다. 돌긴 했지만 둘이 걸렸다.

1. **SPT 파일을 건드린다.** 업데이트마다 다시 해야 하고, 원본을 떠 둬야 한다.
2. **닿지 않는 페이지가 있다.** `/isb` 와 `/ultimateflea/app.html` 은 그 모드가 직접 내놓는
   독립 HTML 이라 Blazor 호스트의 스크립트를 아예 부르지 않는다. ISB 는 HTML 이 DLL 안에 박혀 있어
   파일로 고칠 수도 없다.

그래서 `IStartupFilter` 로 미들웨어를 끼우는 쪽으로 갈아탔다. 응답이 HTML 이면 `</head>` 앞에
`<script src="/modcardkorean/ko-ui.js" defer>` 를 넣는다. 셋 다 한 번에 잡히고,
**SPT 파일은 하나도 건드리지 않는다**(되돌린 뒤 `blazor.web.js` 의 md5 가 원본과 같은 것을 확인했다).

`IStartupFilter` 는 Microsoft 의 인터페이스라 SPT 의 DI 가 그대로 등록해 준다 —
`DependencyInjectionHandler` 는 `System` 으로 시작하는 네임스페이스만 건너뛴다.
등록은 `builder.Build()` 전에 끝나므로 파이프라인이 세워질 때 반영된다.

### 버퍼링은 좁게

모든 응답을 버퍼에 담으면 큰 JSON 과 웹소켓까지 메모리에 올라간다. 다음만 본다:

- `GET` 이고, 요청의 `Accept` 에 `text/html` 이 있는 것
- `/_blazor`·`/_framework`·`/_content` 가 아닌 것, 웹소켓이 아닌 것
- 응답이 2xx 이고 `Content-Type` 이 `text/html` 이며 `Content-Encoding` 이 비어 있는 것(압축본은 손대면 깨진다)

끼우지 못해도 원본을 그대로 내보낸다. 화면이 영어로 남을 뿐 깨지지 않는다.

> 확인할 때 주의: PowerShell 의 `Invoke-WebRequest` 는 `Accept: */*` 를 보내서 **끼워지지 않는다.**
> 브라우저처럼 `text/html` 을 넣어 보내야 한다. 이걸 모르면 "안 된다"로 오진한다.

### 파일은 전부 모드 폴더 안에

```
user/mods/ModCardKorean/
  ModCardKorean.dll
  mod-cards.ko.json   모드 카드 글귀
  ko-ui.js            번역기        → /modcardkorean/ko-ui.js
  ko-ui.json          UI 사전       → /modcardkorean/ko-ui.json
```

아이템 이름은 파일이 아니라 **라우트**가 그 자리에서 만든다(`/modcardkorean/item-names.json`).
디스크의 `locales/global/*.json` 을 쓰지 않는 이유가 있다 — 서버는 런타임에 모드가 만든 아이템까지
합쳐 들고 있고, `SptKoreanLocale` 이 kr 을 그 위에 또 고친다. 디스크 파일로 만들면 2,778줄,
라우트로 뽑으면 **3,669줄**이다.

왜 기동 중이 아니라 요청 시점인가: `LazyLoad` 는 값을 읽는 순간 그때까지 걸린 변환기를 모두 돌린다.
`SptKoreanLocale` 은 PostLoad 패스(`TypePriority >= 200000`)에서 변환기를 거는데 같은 패스 안의
순서는 보장되지 않는다. 기동 중에 읽으면 한글화 전 사전을 뽑을 수 있다.

함정 하나 더: 다른 모드가 아이템 이름에 Unity 리치 텍스트를 박아 둔다
(`<b><color=#FFFFFF>Colt M4A1 … ●</color></b>`). 게임에서는 색이 입혀 보이지만 웹에 넣으면
**태그가 글자로 찍힌다.** 라우트가 떼고 내보낸다.

그리고 **짧은 한 낱말은 아이템 사전에서 버린다.** `Case`·`Buck`·`Core` 같은 것이 UI 라벨이나
프로필 이름 자리를 엉뚱하게 바꿔 놓는다. 실제로 `/database` 의 "Items" 탭이 "물품 보관함"으로
바뀌는 것을 보고 넣은 규칙이다.

### 번역기가 하는 일

Blazor Server 는 웹소켓으로 DOM 을 계속 갈아 끼우므로 `MutationObserver` 로 따라간다.
텍스트 노드와 `placeholder`·`title`·`aria-label`·`alt` 를 본다.

- 앞뒤 공백은 살려 두고 가운데만 바꾼다 — 레이아웃이 흔들리지 않는다.
- 숫자가 끼어 열쇠로 못 쓰는 문구는 **규칙**으로 바꾼다(`1-25 of 10,674` → `10,674개 중 1-25`).
- 꼬리표가 붙은 것은 알맹이만 바꾼다(`#FireKlean gun lube ○`).
- **MongoID·URL·`@핸들`·파일명은 건드리지 않는다.** 사람 이름도 사전에 넣지 않았다.
- 사전에 없는 것은 `window.__ko.missing` 에 모아 둔다.

### 수집 → 번역 → 확인

```powershell
node tools/dump-web-ui-strings.mjs out.json   # 23개 페이지를 실제로 렌더해 미번역을 긁는다
node tools/check-web-ui-korean.mjs /database  # 그 페이지가 실제로 어떻게 보이는지 찍어 본다
pwsh -File tools/install-modcardkorean.ps1 -Force
```

프리렌더만 봐서는 안 된다 — Blazor 는 웹소켓이 붙은 뒤에야 본문이 채워진다. headless Chrome 을
CDP 로 몰아 하이드레이션을 기다린 뒤 읽는다.

**사전을 고칠 때 서버를 다시 켤 필요가 없다.** 라우트가 매 요청마다 디스크에서 읽는다.
모드 폴더에 복사하고 브라우저를 새로고침하면 된다.

### 지금까지 (2026-11-16)

| | 미번역 |
| --- | --- |
| 처음 | 1,400 |
| 1차 사전 | 440 |
| 지금 | **472** (단, `/isb` 212 + `/ultimateflea` 251 이 새로 보이게 된 뒤의 수) |

숫자만 보면 늘었지만, 안 보이던 두 페이지 463개가 드러난 결과다. 그 둘은 각각 51 / 81 로 줄었다.

페이지별 미번역: `/tools` 0, `/status` 0, `/cci` 0, `/webconfigbridge` 0, `/enablelabyrinth` 1,
`/credentials` 1, `/coti` 2, `/orbit` 2, `/configs` 6, `/definitive-weapon-variants/config` 6,
`/sain/presets` 7, `/` 14, `/theblacklist` 15, `/skills-extended` 16, `/profiles` 28, `/database` 29,
`/lotsofloot` 34, `/bis` 44, `/questcodex` 45, `/isb` 51, `/jerobackpack` 63, `/thank-you` 67,
`/ultimateflea` 81.

남은 472 의 내역:

| 무엇 | 어떻게 할 것인가 |
| --- | --- |
| 기여자·프로필 이름, 모드 id (약 160) | **번역하지 않는다.** 바꾸면 안 되는 것들이다 |
| 한글 이름이 아예 없는 아이템 | kr 로케일에도 영문뿐이라 뽑을 것이 없다. `SptKoreanLocale` 소관 |
| 각 모드의 변경 이력 산문 (약 30) | 모드가 업데이트되면 갈린다. 아직 안 했다 |
| 대시보드가 만들어 내는 수치 문자열 | 규칙으로 잡히는 만큼만 잡았다 |
| 그 밖 UI 문구 | 사전에 채워 간다 |

### 되돌리기

```powershell
pwsh -File tools/install-modcardkorean.ps1 -Remove
```

모드 폴더만 지우면 끝이다. SPT 파일은 건드린 것이 없다.
(`blazor.web.js` 를 고치던 옛 방식은 `tools/install-server-web-ui-korean.mjs --remove` 로 되돌렸다.)
