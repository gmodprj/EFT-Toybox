# SptKoreanLocale — 게임 DB locale 한글화

아이템 이름·퀘스트·목표·봇 역할 이름처럼 **서버가 내려주는 게임 locale** 을 우리 사전으로 덮어쓰는 서버 모드다.
F12 설정 창은 [SptKoreanConfig](../SptKoreanConfig) 가, 서버 웹 화면은 [ServerWebKorean](../ServerWebKorean) 이 맡는다.

범위와 남은 분량은 [doc/REVIEW-KOREAN-LOCALIZATION.md](../../doc/REVIEW-KOREAN-LOCALIZATION.md) 에 있다.

## 무엇을 건드리고 무엇을 안 건드리나

| 건드리는 것 | 건드리지 않는 것 |
| --- | --- |
| 서버가 메모리에 들고 있는 `locales/global/<코드>` 사전의 **값** | `locales/global/*.json` 파일 |
| | 다른 모드의 파일 (업데이트에 날아가므로 손대지 않는다) |
| | 아이템·퀘스트 자체의 내용 (글자만 바꾼다) |

모드를 빼면 다음 서버 시작에서 원래 글자로 돌아간다.

## 왜 이 방법이 되는가 (SPT 4.1.6 역컴파일 + 서버 실측)

**값을 직접 쓰면 안 된다.** 처음에 `LocaleService.GetLocaleDb("kr")` 로 받은 사전에 그대로 썼고,
모드 로그도 "22항목을 바꿨다" 고 적었다. 그런데 서버에 물어보니 22항목 전부가 영문으로 돌아와 있었다.

| 실측 | 값 |
| --- | --- |
| 모드가 `PostLoad` 에서 본 locale 항목 수 | 60,398 |
| 그 뒤 `/client/locale/kr` 이 내려준 항목 수 | 61,125 |
| 우리가 쓴 22항목 | 전부 영문으로 되돌아 있었다 |

`GlobalLocaleDictionary` 에 `[CacheLazyLoad]` 가 붙어 있어 `LazyLoad.Value` 는 **캐시된 같은 인스턴스**를
돌려준다. 다만 `LazyLoad.AddTransformer` 가 `Clear()` 를 부른다 — 우리보다 **늦게** 변환기를 등록하는
모드가 하나라도 있으면 캐시가 버려지고, 다음 읽기에서 **파일을 다시 읽어 등록된 변환기만** 다시 입힌다.
사전에 직접 쓴 값은 그 자리에 없다. 항목이 727개 늘어난 것이 다시 세워졌다는 증거다.

**그래서 값을 쓰지 않고 변환기를 등록한다.**

| 근거 | 뜻 |
| --- | --- |
| `LazyLoad.AddTransformer(Func<T?,T?>)` — "Transformers execute in registration order" | 사전이 다시 세워질 때마다 돈다. `PostLoad` 라 늦게 등록되므로 우리 것이 마지막에 입혀진다 |
| `DataCallbacks.GetLocalesGlobal` 이 요청마다 `LocaleService.GetLocaleDb(url 의 언어)` → `LazyLoad.Value` 를 내려보낸다 | 변환기를 거친 사전이 그대로 클라이언트에 간다 |
| `LocaleService.GetLocaleDb` 는 그 언어가 없으면 **조용히 `en` 으로 떨어진다** | 그래서 `LocaleService` 를 쓰지 않고 `localeTable.Global` 에 그 언어가 정말 있는지 보고, 없으면 **아무것도 하지 않는다** |
| `LocaleTable.Global` 의 "DO NOT USE THIS PROPERTY DIRECTLY" 주석 | 그 주석이 막는 것은 **값을 직접 고치는 것**이다(위에서 날아간 그 방식). 변환기를 등록하는 길은 `LazyLoad` 에만 있어 이 속성을 거치는 수밖에 없고, 값은 건드리지 않는다 |

변환기는 사전이 다시 세워질 때마다 돌기 때문에, 로그는 **첫 번째만** `Info` 로 적고 그 뒤는 `debug` 로 내린다.
`applied-report.json` 의 `run` 이 몇 번째 적용인지 알려 준다.

## 실행 순서

`OnLoadOrder.PostLoad` 다. 모드가 만드는 아이템·퀘스트의 locale 열쇠는 그 모드가 돈 뒤에 생기므로,
이르게 돌면 덮어쓸 열쇠가 아직 없다.

## 사전 형식

`lang/<묶음>.json`. 밑줄로 시작하는 파일은 읽지 않는다(메모·보류용).

```json
{
  "coveragePrefixes": ["ScavRole/"],
  "comment": "사람이 읽는 메모. 코드는 쓰지 않는다",
  "entries": {
    "<locale 열쇠>": "<한국어>"
  },
  "keptAsIs": {
    "<locale 열쇠>": "일부러 영문으로 두는 이유"
  }
}
```

- `entries` 가 실제로 덮어쓰는 것.
- `keptAsIs` 는 코드가 쓰지 않는다. **"빠뜨린 것" 과 "일부러 둔 것" 을 가르기 위한 기록**이고, 검사 도구가 읽는다.
- `coveragePrefixes`·`coveragePatterns` 도 모드는 쓰지 않는다. 검사 도구가 그 범위의 열쇠 중 사전에도 `keptAsIs` 에도 없고 아직 영문인 것을 찾아 **빠뜨림**으로 알린다. 접두로 묶이지 않는 것(퀘스트 열쇠는 24자리 id 로 시작한다)은 `coveragePatterns` 에 정규식으로 적는다 — 보기: `^[0-9a-f]{24} name$`.
- **한 묶음을 여러 파일로 쪼개도 된다.** 검사 도구의 묶음 검사는 사전 전체를 한 덩어리로 보므로, 옆 파일이 맡은 열쇠를 "빠뜨렸다" 고 잡지 않는다(퀘스트 이름은 `quest-names-1..4.json` 넷이다).

## 지금 들어 있는 사전

| 파일 | 묶음 | 번역 | 유지 | 묶음을 다 맡았나 |
| --- | --- | --- | --- | --- |
| `scav-roles.json` | 봇 역할 이름 (`ScavRole/*`) | 22 | 5 | ● 전수 |
| `quest-names-1..4.json` | 퀘스트 이름 (`<id> name`) | 1,243 | 2 | ● 전수 |
| `quest-objectives-1..2.json` | 퀘스트 목표(조건) (`<id>`) | 956 | — | ○ 진행 중 |

**`coveragePatterns` 는 "이 묶음을 다 맡았다" 는 선언이다.** 아직 진행 중인 묶음에는 넣지 않는다 —
넣으면 검사 도구가 남은 것을 전부 "빠뜨림" 으로 세어, 진행 중인 일과 진짜 누락이 구별되지 않는다.
진행 상황은 `dump-untranslated.mjs --pending` 으로 본다.

## 문구 단위로 적는 사전 (`lang-src/`)

퀘스트 목표는 열쇠 3,482개가 서로 다른 문구 2,819개를 쓴다 — 같은 문구를 663번 더 적어야 하고,
그러면 표기가 갈린다. 그래서 **문구 단위로 번역**하고 그것을 열쇠 사전으로 펴낸다.

| 자리 | 무엇 |
| --- | --- |
| `lang-src/<이름>.json` | 손으로 적는 것. `{"texts": {"<영문>": "<한국어>"}}` |
| `lang/<이름>-N.json` | **생성물.** 직접 고치지 않는다 |

```
node tools/expand-locale-dict.mjs quest-objectives "^[0-9a-f]{24}$" --dry     무엇이 짝지는지만 본다
node tools/expand-locale-dict.mjs quest-objectives "^[0-9a-f]{24}$"           펴낸다
... --chunk 500        한 파일에 담을 항목 수
... --claim-coverage   묶음을 다 적었을 때만. coveragePatterns 를 넣는다
```

펴낼 때 **켜져 있는 서버에게 영문 값을 물어** 짝을 맞춘다. 그래서

- 지도에 적었는데 서버에 그 문구가 없으면 **오타이거나 모드가 바뀐 것**이라 경고로 알려 준다.
- 모드 업데이트로 어떤 열쇠의 영문이 바뀌면 그 열쇠는 그냥 빠지고, 검사 도구가 잡는다. 조용히 틀리지 않는다.
- 다른 사전이 이미 맡은 열쇠는 비켜 둔다(겹쳐 적으면 어느 쪽이 이기는지 헷갈린다).
- **바닐라가 이미 번역해 둔 열쇠는 건드리지 않는다** — 기본 파일에서 `en` 과 `kr` 이 다르면 번역된 것이다.
- 우리도 바닐라도 아닌 **다른 모드**가 한국어로 바꿔 둔 열쇠도 비켜 둔다.

### 짝은 `kr` 이 아니라 `en` 에 맞춘다 (한 번 틀렸다)

처음에는 **대상 locale(`kr`)** 의 값으로 짝을 맞췄다. 2차 묶음을 펴내는 순간 **1차 묶음 956개가
통째로 사라졌다** — 1차가 이미 적용돼 그 열쇠들의 `kr` 값이 한국어였고, 지도의 영문과 짝이 맞지
않아 빠져 버린 것이다. 지도에 있는 문구 293개가 "서버에 없다" 는 경고로 한꺼번에 떴다.

`en` locale 은 우리가 건드리지 않으므로 **몇 번 펴내도 같은 결과가 나온다.** 그래서 기준을 `en` 으로
옮겼다. "우리가 바꾼 것" 인지 가리는 근거도 둘로 늘렸다 — ① 이 이름으로 앞서 펴낸 파일에 있다
② 지금 값이 이 지도의 한국어 가운데 하나다. ②는 **서버에는 적용돼 있는데 파일에서는 지워진**
상태(펴내기를 잘못해 잃은 뒤 다시 펴낼 때)를 되살리는 데 쓴다.

## 번역할 거리 만들기

```
node tools/dump-untranslated.mjs "^[0-9a-f]{24} name$"                요약(항목 수·글자수·모드/바닐라)
node tools/dump-untranslated.mjs "^[0-9a-f]{24} name$" --tsv          id<TAB>영문
node tools/dump-untranslated.mjs "^[0-9a-f]{24} name$" --pending      아직 사전에 없는 것만
```

켜져 있는 서버에게 물으므로 **모드가 전부 올라간 뒤**의 영문 항목만 나온다. 이미 번역한 것은
한글이라 저절로 빠지고, `--pending` 은 사전에 적어 둔 열쇠까지 뺀다 — 큰 묶음을 여러 번에
걸쳐 적을 때 다음 거리를 만들어 준다.

### 번역 규칙

바닐라 한국어 locale 을 따른다 — **역할·사물 낱말은 번역하고, 세력 약어는 그대로 둔다.**
바닐라도 `USEC`·`BEAR`·`UNTAR` 를 영문으로 두고, `Boss`→`보스`·`Guard`→`경호원`·`Sniper`→`저격수` 로 옮긴다.

## 설정

| 항목 | 기본값 | 내용 |
| --- | --- | --- |
| `modEnabled` | `true` | 끄면 사전만 읽고 셈을 로그에 남긴다 |
| `locale` | `"kr"` | SPT 의 한국어는 `kr` 이다. 서버 웹의 `ko` 와 다르다 |
| `onlyExistingKeys` | `true` | 이미 있는 열쇠만 덮어쓴다. 없는 열쇠는 경고로 알린다 |
| `skipAlreadyKorean` | `false` | 켜면 이미 한글인 값은 건드리지 않는다 |
| `debug` | `false` | 항목 하나하나를 로그에 적는다 |

## 빌드·설치·확인

```
dotnet build src/SptKoreanLocale -c Release
pwsh -File tools/install-koreanlocale.ps1
```

서버를 다시 켠 뒤:

```
node tools/check-locale-dict.mjs
```

이 도구는 사전을 세지 않고 **켜져 있는 서버에게 `/client/locale/kr` 을 물어** 그 글자가 실제로 가는지 본다.
`locale` 이 없어 서버가 `en` 을 돌려준 경우에는 셈을 내놓지 않고 중단한다.

제거는 `pwsh -File tools/install-koreanlocale.ps1 -Remove` 다.

## 알아 둘 것

- **서버 모드라 사전을 고치면 서버를 다시 켜야 한다.** F12 사전처럼 그 자리에서 다시 읽지 않는다.
- 서버가 시작할 때 `applied-report.json` 을 모드 폴더에 남긴다 — 로그는 기동마다 덮이므로 결과를 읽을 자리가 필요하다.
- 다른 모드가 **우리보다 뒤에서** 같은 열쇠를 덮으면 우리 글자가 밀린다. `check-locale-dict.mjs` 의 "값이 다름" 이 그것을 잡는다.
- `locale` 을 `en` 으로 두면 안전장치가 꺼진다(그 경우는 en 에 쓰는 것이 의도이므로). 한국어를 쓸 때는 `kr` 로 둔다.

## 변환기가 닿지 못하는 열쇠 11개 (알려진 것)

`check-locale-dict.mjs` 가 **값이 다름 11** 을 낸다. 지금은 이것이 정상이다.

| 실측 | 값 |
| --- | --- |
| 변환기가 본 locale 항목 수 | 60,398 |
| `/client/locale/kr` 이 내려주는 항목 수 | 61,125 |
| 우리 사전에 있는데 서버는 영문인 열쇠 | **11** |

열쇠 11개 모두 **`acidphantasm-reffriendlyquests`** 의 퀘스트 것이다. 그 모드는 locale 항목을
**마지막 재빌드 뒤에 캐시된 사전에 직접 더한다** — 변환기는 사전이 *세워질 때* 돌므로 그보다
늦게 직접 더해진 열쇠는 지나가지 않는다. 그래서

- 그 11개는 변환기로는 닿지 못한다. `onlyExistingKeys` 를 꺼도 우리가 쓴 뒤에 그 모드가 덮는다.
- 다른 2,510개는 멀쩡하다.

**숨기지 않고 셈에 남겨 둔다** — 서버 오류 48/1/1 을 기준선으로 적어 두는 것과 같은 꼴이다.
이 수가 11 에서 늘면 그때는 새로 생긴 일이라는 뜻이다.
