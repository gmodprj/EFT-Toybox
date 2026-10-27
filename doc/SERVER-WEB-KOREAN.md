# 서버 웹 설정창(SIC) 한글화

SPT 서버를 켜면 열리는 `https://127.0.0.1:6969/` 화면(Server Information Center, SIC)을 한국어로 바꾼 기록.
조사 시점 2026-10-26, SPT 4.1.6, 설치본 `E:\Spt`.

## 결론 먼저

SPT 서버 본체가 이미 **웹 UI용 로케일 파일을 읽는 구조**다.
`SPT_Data/database/locales/web/` 에 `ko.json` 을 넣는 것으로 끝난다. 어셈블리를 건드릴 필요가 없다.

그리고 조사 중에 알게 된 사실: 이 설치본의 웹 화면은 **원래 영어가 아니라 고장 나 있었다.**
번역 키(`landing-page-title` 같은 문자열)가 그대로 화면에 찍히고 있었다. 이유는 아래 3번.

## 1. 화면이 어디서 오는가

`E:\Spt\SPT_Runtime` 를 뒤진 결과:

| 근거 | 내용 |
| --- | --- |
| `SPTarkov.Server.Web.dll` (634KB), `MudBlazor.dll` | 웹 UI 는 **Blazor Server + MudBlazor** |
| `SPT_Data/wwwroot/` 에 파일 24개 | `blazor.server.js`·`blazor.web.js`·MudBlazor css/js·`spt-theme.css`·favicon 뿐. **html 파일이 하나도 없다** |
| `SPT_Data/wwwroot/_framework/blazor.web.js` 존재 | 정적 HTML 이 아니라 서버가 렌더해 WebSocket 으로 내려보내는 방식 |

즉 화면은 정적 자원이 아니라 `SPTarkov.Server.Web.dll` 안의 Razor 컴포넌트가 만든다.
페이지는 `/`, `/database`, `/configs`, `/profiles`, `/credentials`, `/tools`, `/status`, `/thank-you`, `/login`, `/change-password` (랜딩 페이지의 `a[href]` 를 실제로 읽어 확인).

호스트 HTML 은 어셈블리 안에 리터럴로 박혀 있고, 링크·스크립트 태그가 고정이다:

```
<link href="/_content/MudBlazor/MudBlazor.min.css" rel="stylesheet">
<link href="/SPTarkov.Server.Web/css/spt-theme.css" rel="stylesheet">
<script src="/_content/MudBlazor/MudBlazor.min.js"></script>
<script src="/_framework/blazor.web.js"></script>
```

→ 모드가 `<head>` 에 끼어들 자리가 없다. 모드용 확장은 `IModBlazorMetadata`(`SPTarkov.Server.Web.xml`)로
**자기 wwwroot 를 `/{모드어셈블리명}/` 에 붙이고 자기 페이지를 추가**하는 것까지이고,
본체 페이지의 문구를 바꾸는 통로는 아니다.

## 2. 문자열이 어떤 형태로 보관되는가

`SPT_Data/database/locales/` 아래가 전부다.

```
locales/
  languages.json      게임 쪽 언어 목록 (ch, cz, en, ... kr)
  global/   ch.json cz.json en.json ... kr.json   게임 클라이언트 문자열
  server/   ar.json cs.json ... ko.json ... (30개)  서버 콘솔/로그 문자열
  web/      en.json                                 ← 웹 UI 문자열. en.json 하나뿐이었다
```

`SPTarkov.Server.Core.dll` 안에 경로가 그대로 들어 있다(UTF-16 이라 `tr -d '\000'` 로 뽑음):

```
./SPT_Data/database/locales/server
./SPT_Data/database/locales/web
```

`web/en.json` 은 키 230 개. 화면에 찍히던 `landing-page-title`·`database-browser-desc` 가 전부 이 파일의 키다.
→ **(b) 서버가 제공하는 로케일 JSON** 이 답이다. (a)·(c) 는 아니다.

다만 모든 문구가 이 파일에 있는 건 아니다. 5번 참고.

## 3. 왜 키가 그대로 찍혔는가

| 근거 | 내용 |
| --- | --- |
| `SPT_Data/configs/locale.json` | `"serverLocale": "system"` |
| 시스템 로케일 | 한국어 |
| 서버 로그 `user/logs/spt/spt20261026.log` | `[클라이언트 요청] /`, `[웹소켓 리퀘스트] /_blazor` — **한국어로 찍힌다** |
| `locales/server/ko.json` | `client_request` → `[클라이언트 요청] %s` (로그 문구와 일치) |

`serverLocale` 이 `ko` 로 해석되고 있다. `locales/server/ko.json` 은 있어서 로그는 한국어로 나오는데,
`locales/web/` 에는 `ko.json` 이 없다. 그래서 웹 쪽만 조회에 실패해 **키 문자열을 그대로 출력**했다.
`en.json` 으로 되돌아가는 폴백은 동작하지 않는다(화면에 영어가 아니라 키가 찍힌 것이 그 증거).

### 어떻게 봤는가 (재현 방법)

내장 브라우저 pane 은 `127.0.0.1`/`localhost` 로 이동이 거부되고, Chrome 확장은 연결돼 있지 않았다.
대신 로컬 Chrome 을 headless + CDP 로 직접 몰았다. Blazor Server 는 WebSocket 이 붙은 뒤에야 본문이 채워지므로
`--dump-dom` 한 방으로는 부족하고, 회로가 붙을 시간을 준 뒤 `document.body.innerText` 를 읽어야 한다.

```bash
"C:/Program Files/Google/Chrome/Application/chrome.exe" --headless=new --disable-gpu \
  --ignore-certificate-errors --no-first-run --remote-debugging-port=9222 \
  --user-data-dir=<임시폴더> --window-size=1500,1100 about:blank &
# 그 뒤 CDP 로 Page.navigate → 9초 대기 → Runtime.evaluate(document.body.innerText)
```

`curl -k https://127.0.0.1:6969/` 로 받은 프리렌더 HTML 에서도 같은 키가 보이지만,
프리렌더만 보고 판단하면 "하이드레이션 뒤에는 영어가 나올 수도 있다"를 배제하지 못한다. 그래서 실제 렌더까지 봤다.

## 4. 고른 경로와 한 일

**로케일 파일 추가.** 세 후보 중:

- 로케일 파일 추가 ← **채택**. 본체가 지원하는 구조이고, 파일 하나를 새로 넣을 뿐 기존 파일을 덮어쓰지 않는다. SPT 업데이트로 `en.json` 에 키가 늘어나도 `tools/check-web-locale.mjs` 로 바로 잡힌다.
- 정적 자원 교체 — 불가능. 교체할 html/js 자체가 없다(1번).
- 어셈블리 패치 — 과제에서 금지. 할 필요도 없다.

만든 것:

| 파일 | 내용 |
| --- | --- |
| `src/ServerWebKorean/ko.json` | 번역 230 키(= `en.json` 전수). `example-heading`("SPT 4.0")만 버전 문자열이라 원문 유지 |
| `tools/install-server-web-locale.ps1` | 설치/제거. 서버·게임이 켜져 있으면 중단한다 |
| `tools/check-web-locale.mjs` | `en.json` 과 키 수·`{0}` 자리표시자·누락을 대조 |

용어는 `src/SptKoreanConfig/lang/` 를 따랐다(프로필, 상인, 은신처, 플리마켓, 프리셋, 평판 등급).
설정 키·경로·`MongoID`·`JSON`·`DI`·`ConfigLoader`·`Patreon` 같은 식별자는 원문 그대로 뒀다.

### 설치 / 되돌리기

```powershell
# 설치 (서버를 끈 상태에서)
pwsh -File tools/install-server-web-locale.ps1

# 되돌리기
pwsh -File tools/install-server-web-locale.ps1 -Remove
```

되돌리면 `locales/web/ko.json` 이 지워지고, 화면은 다시 키가 찍히는 원래 상태로 간다.
영어로 보고 싶으면 `SPT_Data/configs/locale.json` 의 `serverLocale` 을 `"en"` 으로 바꾸면 된다(서버 로그도 영어가 된다).

### 백업

`locales/web/` 에는 원래 `en.json` 하나뿐이고 이번 작업은 **덮어쓴 파일이 없다**(새 파일 추가).
그래도 작업 전 디렉토리 전체를 떠 뒀다:

```
E:\Spt_Mods\backup\server-web-locale-pre-20261026-090624\en.json   (13,608 bytes)
```

복구가 필요하면:

```powershell
Copy-Item "E:\Spt_Mods\backup\server-web-locale-pre-20261026-090624\en.json" `
  -Destination "E:\Spt\SPT_Runtime\SPT_Data\database\locales\web\en.json" -Force
```

## 5. 한글화되지 않는 부분 (실측)

로케일 파일로 닿는 범위는 `en.json` 에 키가 있는 문구까지다. 설치 후 각 페이지를 실제로 렌더해 확인한 결과:

| 페이지 | 결과 |
| --- | --- |
| `/` 랜딩 | 한국어. 모드 카드 문구는 각 모드가 제공하는 값이라 영어로 남는다(본체 소관 아님) |
| `/configs` 설정 편집기 | 한국어. 목록의 `Airdrop`·`airdrop.json` 등은 설정 이름/파일명이라 원문이 맞다 |
| `/credentials` 사용자 계정 | 한국어 |
| `/tools` 도구 | 한국어 |
| `/status` 서버 상태 | 한국어 |
| `/thank-you` 기여자 | 한국어 |
| `/profiles` 프로필 관리 | **영어 남음** — `Profile control`, `Create, inspect, save, reload, and remove local SPT profiles.`, `PROFILES`/`CREATED`/`WIPED`, `Create profile`, `Username`, `Edition`, `Create`, `Selected profile`, `Select a profile from the table.`, `1 local profiles loaded`, 표 머리글 |
| `/database` 데이터베이스 탐색기 | **영어 남음** — `Items`, `Item templates, prices, categories, and core item properties.`, `ITEMS`/`CATEGORIES`/`LOCALE`, `Table`, `Filters`, `Search`, `All categories`, `Handbook category`, `Clear filters`, 표 머리글 |
| `/login` 로그인 | **영어 남음** — `Sign in`, `Enter your SPT web credentials to continue.`, `Default login: spt / spt`, `Username`, `Password` |

이 문구들은 `en.json` 에 **키 자체가 없다**. 즉 업스트림이 아직 로케일로 빼지 않고 Razor 컴포넌트에 리터럴로 박아 둔 것이다.
`SPTarkov.Server.Web.dll` 에서 그대로 뽑힌다(예: `<label class="auth-field"><span>Username</span>`, `Sign in`, `No permissions`).

→ 로케일 파일로는 닿을 수 없다. 닿으려면 어셈블리 패치뿐인데 과제에서 금지했고, 권하지도 않는다
(SPT 업데이트마다 다시 패치해야 하고, 체크섬 검사 `SPT_Data/checks.dat` 와 부딪칠 수 있다).
이 부분은 업스트림이 키를 추가하면 `en.json` 에 나타나고, 그때 `tools/check-web-locale.mjs` 가 "번역 없음"으로 잡아 준다.

## 6. 확인한 화면

서버를 끄고(`SPT.Server.exe` PID 2072) 설치한 뒤 다시 켜서(PID 37368) 실제로 렌더해 확인했다.

설치 전 `/` — `landing-page-title`, `database-browser-desc` … 키가 그대로.
설치 후 `/` — `서버 정보 센터 (SIC)`, `이 SPT 설치본을 들여다보는 로컬 도구와 서버 페이지.`,
`데이터베이스 탐색기`, `설정 편집기`, `프로필 관리`, `사용자 계정`, `도구`, `서버 상태`, `버전 4 기여자`.

`/status` 는 `SPT 버전: 4.1.6`, `시작 시각`, `작동 시간`, `최근 30분 동안 활동한 프로필:` 까지 한국어로 나온다.

## 참고

- 작업 중 게임(`EscapeFromTarkov`)은 꺼져 있었다. 서버만 껐다 켰다.
- 웹 로그인은 `SPT_Data/configs/http.json` 의 `requireCredentialsOnLocalhost: false` 라 로컬에서는 묻지 않는다.
- `/database` 의 `LOCALE en` 은 웹 UI 언어가 아니라 **아이템 이름에 쓰는 게임 로케일**이다. 이건 `locales/global/` 소관이고 이번 작업 범위가 아니다.
