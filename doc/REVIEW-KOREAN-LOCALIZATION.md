# 한글화 범위 검토 — F12 밖의 화면들

F12 설정 창은 끝났다(5,136/5,136). 그 밖에 **모드 메뉴·퀘스트·이벤트·장비·대사**가 한국어로 나오는지 재고,
각각 어디를 고쳐야 하는지와 할 만한지를 적는다. 조사 시점 2026-11-06, SPT 4.1.6 / EFT 0.16.9.5.40743, 설치본 `E:\Spt`.

**이 문서는 검토다. 아직 아무것도 고치지 않았다.**

## 결론 먼저

1. **게임 DB 쪽(아이템·퀘스트·목표·상인 대사)이 남은 일의 거의 전부다.** 서버가 실제로 내려주는 한국어 locale 61,125항목 가운데 **영문 산문이 18,798항목, 영문 208만 자**다.
2. 그 18,798 중 **6,441항목은 모드 탓이 아니다** — 바닐라 EFT 의 한국어 자체가 비어 있는 자리다(특히 퀘스트 설명). 모드가 더한 것은 12,357항목이다.
3. **가장 심한 곳은 퀘스트다.** 이름·설명 2,284 + 목표(조건) 4,461 + 대사·메일 1,353 = **8,098항목 / 130만 자**. 퀘스트는 읽어야 할 글이라 영문이면 그대로 막힌다.
4. **아이템 설명은 번역해도 보이지 않는다.** ODT's Item Info 가 모든 아이템의 `Description` 을 자기 정보 블록으로 **갈아끼운다**. 원래 설명은 화면에 안 나온다.
5. 클라이언트 쪽은 작다 — locale 파일을 읽는 플러그인이 **4개**뿐이고, 그중 한글이 있는 것은 ImpactCue 하나다. 나머지 3개와 **ISB 무전 자막 222줄**은 파일만 넣으면 된다(자막은 역컴파일로 확인했다).
6. **다 하는 것은 현실적이지 않다.** 208만 자다. 단계로 쪼개 **퀘스트 → 봇 역할 이름 → 클라이언트 locale 파일 → 아이템 이름** 순으로 하는 것을 권한다.

## 1. 한글화 표면 여섯 곳

| # | 표면 | 상태 | 고치는 자리 |
| --- | --- | --- | --- |
| 1 | F12 설정 창 (모드 이름·섹션·설정·설명) | **끝남** 5,136/5,136 | `SptKoreanConfig` (`src/SptKoreanConfig`) |
| 2 | 서버 웹 화면 (SIC) | **끝남** | `locales/web/ko.json` (`src/ServerWebKorean`) |
| 3 | **게임 DB locale** — 아이템 이름·설명, 퀘스트 이름·설명·목표, 상인 대사, 업적, 봇 역할 이름 | **안 됨** 영문 18,798항목 / 208만 자 | 서버 모드로 `kr` locale 에 덮어쓴다 (§4) |
| 4 | 플러그인이 읽는 locale 파일 | **안 됨** 3개 모드 | 모드 폴더에 `kr.json`·`ko.json` 추가 |
| 5 | ISB 무전·방송 자막 | **안 됨** 222줄 | `Subtitles/locales/kr.json` 추가 (§5) |
| 6 | 플러그인 DLL 안에 박힌 인게임 UI 글자 | **재지 않았다** | `SptKoreanConfig` 의 트랜스파일러 방식 재사용 (§6) |

## 2. 실측 — 서버가 실제로 내려주는 한국어 locale

게임이 켜져 있는 상태에서 `/client/locale/kr` 을 직접 불러 받은 것이다(`node tools/stm-call.mjs client/locale/kr "{}"`).
파일을 세는 것이 아니라 **모드가 전부 올라간 뒤의 결과**를 센 값이다.

| 값의 모양 | 항목 수 | 뜻 |
| --- | --- | --- |
| 한글이 들어 있다 | 23,379 | 번역돼 있다(일부만 한글인 것도 포함) |
| **영문 낱말 3개 이상** | **18,798** | **미번역 산문. 영문 2,081,995자** |
| 영문 1~2낱말 | 12,567 | `M4A1`·`FMJ` 같은 고유명사가 대부분 — 바닐라 한국어도 영문으로 둔다 |
| 글자 없음(숫자·기호) | 6,381 | 번역 대상이 아니다 |
| 합계 | 61,125 | |

바닐라 `kr.json` 은 31,550항목이다. 모드가 **29,581항목을 더했고**, 미번역 산문 18,798 중 모드 몫이 12,357, **바닐라 몫이 6,441** 이다.

### 미번역 산문의 분류

| 분류 | 항목 | 영문 글자수 | 예 |
| --- | --- | --- | --- |
| **퀘스트 이름·설명** | 2,284 | 909,642 | `5936d907… description` = "This quest is required for Collector…" |
| 아이템 이름·약칭 | 8,001 | 532,067 | `5447ac64… Name` = "5.56x45mm Green Tip box (50 pcs) (54/31) ○" |
| **퀘스트 목표(조건)** | 4,461 | 210,288 | `66c89b32…` = "Summer Twitch Drops event" |
| **퀘스트 대사·메일** | 1,353 | 181,039 | `5967733e… successMessageText` = "Thank you, young man…" |
| 기타(탈출구·UI 키 등) | 1,739 | 160,280 | `V-ex_light` = "Road to Military Base V-Ex" |
| 아이템 설명 | 960 | 88,679 | `5696686a… Description` = "Several United States dollar bills." |

**아이템 이름·약칭 8,001 은 숫자가 부풀어 있다.** Ammo Stats·BALL 이 탄약 이름에 `(54/31) ○` 같은 꼬리를 붙여 영문 낱말이 늘어난 것이고, 탄약 이름은 바닐라 한국어도 영문으로 둔다. 실제로 번역해서 달라지는 것은 장비·소모품 이름 쪽이다.

### 미번역이 많은 출처

| 항목 | 출처 |
| --- | --- |
| 6,700 | **바닐라 키** (EFT 자체의 한국어 구멍) |
| 2,799 | 모드가 코드로 더한 키 (locale 파일이 없어 어느 모드인지 못 가림) |
| 2,444 | Lotus |
| 1,122 | ISB Aishi |
| 755 | WTT Content Backport |
| 579 | ISB Special Forces |
| 485 | Skills Extended |
| 155 | WTT Armory |
| 117 | Manimal Icebreaker |

### 모드가 들고 있는 locale 파일 쪽 (정적 실측)

`user/mods` 아래 locale JSON 은 **202개 / 모드 33개**다.

- en 고유 키 **14,840**, kr 고유 키 **1,808**.
- **kr 을 들고 있는 모드는 4개뿐**이다 — acidphantasm Scorpion(902), QuestBackport(492), Skills Extended(128), Amonya.
- en 은 있고 kr 이 없는 모드 **22개**.
- 바닐라 `kr.json` 이 이미 가진 키와 겹치는 것은 **278개뿐**이다. 즉 **바닐라 한국어를 그대로 가져다 쓸 수 있는 몫은 거의 없다** — 백포트 모드가 가져온 콘텐츠는 이 설치본의 EFT 0.16.9.5 locale 보다 새 것이다.

## 3. 알아 둘 것 — 아이템 설명은 ODT's Item Info 가 갈아끼운다

`5447a9cd… Description`(Colt M4A1)의 실제 값:

```
플리마켓: 153,083₽ | 항목 값: 13,798 | Mechanic'의 평가: 7,727₽ | <color=#FFFFFF>공통</color>

발견 (✔) x1 > The Punisher - Part 5 @ 프라퍼
발견 x2 > Compensation for Damage - Collection @ 펜스
↺ "Gunsmith - Part 15" 메카닉✔ @ 레프 레벨.2 ∈ Colt M4A1 5.56x45 돌격소총
```

원래 설명("Colt M4A1은 5.56x45mm NATO…")은 **남아 있지 않다.** 그래서

- **아이템 설명 960항목을 번역하는 것은 값이 없다** — Item Info 를 끄지 않는 한 화면에 안 나온다.
- 대신 이 블록 안에 **영문 퀘스트 이름이 섞여 나온다**("The Punisher - Part 5"). 상인 이름은 한국어(프라퍼·메카닉)인데 퀘스트 이름만 영문이라 눈에 띈다. **퀘스트 이름을 번역하면 이 화면도 같이 고쳐진다** — 퀘스트를 1순위로 두는 이유 하나.

## 4. 게임 DB locale 을 어떻게 고치나

### 모드 파일을 직접 고치지 않는다

모드 업데이트에 날아간다. [BACKLOG.md](BACKLOG.md) 의 ISB 역할 이름 항목에도 같은 판단이 적혀 있다.

### 서버 모드 하나가 `kr` locale 에 덮어쓴다

F12 쪽을 `SptKoreanConfig` 하나로 처리한 것과 같은 꼴이다 — 사전은 우리 저장소에 두고, 모드는 그것을 읽어 덮어쓰기만 한다.

확인한 제약 둘:

| 근거 | 제약 |
| --- | --- |
| `SPTarkov.Server.Core.xml` 의 `LocaleTable.Global` 주석: "DO NOT USE THIS PROPERTY DIRECTLY, USE LOCALESERVICE INSTEAD. THIS IS LAZY LOADED AND YOUR CHANGES WILL NOT BE SAVED" | 테이블을 직접 건드리면 안 된다. `LocaleService.GetLocaleDb("kr")` 로 받은 사전에 써야 한다 |
| 기억해 둔 규칙 — **DB 변경은 아이템 만드는 모드보다 늦게 돌아야 한다** | `PostLoad` 단계여야 한다. 그보다 이르면 모드가 만든 아이템·퀘스트의 키가 아직 없다 |

`LocaleService.GetLocaleDb(language)` 는 `TryGetLocaleDb(language)` 가 실패하면 `en` 으로 떨어진다(역컴파일 실측). `kr.json` 은 있으므로 `kr` 은 제대로 잡힌다.

### 사전은 어디서 오나

세 갈래이고, 들인 품에 비해 얻는 것이 다르다.

| 갈래 | 품 | 얻는 것 |
| --- | --- | --- |
| **바닐라 kr.json 재사용** | 거의 없음 | **278항목뿐.** 사실상 길이 아니다(§2) |
| 모드가 들고 있는 kr 파일 옮기기 | 적음 | 1,808항목. acidphantasm Scorpion·QuestBackport 가 대부분 |
| 손으로(또는 기계로) 번역 | 큼 | 나머지 전부. 208만 자 |

## 5. 작은 것들 — 파일만 넣으면 되는 것

### 5-1. ISB 무전·방송 자막 222줄 — **길이 열려 있다 (역컴파일 실측)**

`BepInEx\plugins\ISBSpecialForcesPlugin\ISBSpecialForces_Audios\Subtitles\locales\` 에 `ch·en·po·pt·ru` 가 있고 한국어가 없다. 각 222줄(ru 만 279줄).

F12 설정 설명은 "Supported: en, pt, ru and ch" 라고만 적어 두었지만, `ISBSOF_Extras.dll` 을 역컴파일해 보니 **허용 목록이 아니다**:

```csharp
private string FindAvailableLocale(string requested) {
    string text = NormalizeLocale(requested);
    if (File.Exists(Path.Combine(_localesFolder, text + ".json"))) return text;
    ...
    return "en";
}
```

`NormalizeLocale` 은 모르는 코드를 **그대로 돌려준다**(`return text`). 게임 locale 코드가 `kr` 이므로 `Subtitles/locales/kr.json` 을 넣으면 `Subtitle Language = Auto` 그대로 집어 간다.
또 `ResolveText` 가 **키마다 영어로 떨어지므로 일부만 번역해도 된다.**

주의: 모드 폴더 안이라 업데이트에 날아간다. 설치 스크립트로 다시 넣는 형태가 맞다.

### 5-2. locale 파일을 읽는 플러그인 4개

| 플러그인 | 가진 언어 | 항목 | 비고 |
| --- | --- | --- | --- |
| ImpactCue | `ko` 포함 13개 | 1,302 | **이미 한국어.** 10차에서 F12 사전으로도 옮겨 썼다 |
| Advanced Stash Sorting | ch·en·ru | 104 | 한국어 없음. 창 UI 글자다 |
| Wishlist Extended | en·pl | 10 | 한국어 없음. 제작·은신처 목록 제목들 |
| Softwyx Loot In Vicinity | en | 2 | `"Nearby Items"` 와 그 개수 표기 |

Advanced Stash Sorting·Wishlist Extended·Loot In Vicinity 는 **`kr.json`(또는 그 모드가 쓰는 코드)을 넣는 것으로 끝날 가능성이 높다.** 다만 자막과 달리 **어떤 코드를 찾는지 역컴파일로 확인하지 않았다** — 하기로 정하면 그것부터 본다.

### 5-3. 봇 역할 이름 26항목

`ScavRole/*` 가운데 영문 그대로인 것이 26개다(`ScavRole/infectedAssault` = "Infected" 등). [BACKLOG.md](BACKLOG.md) 에 ISB 10개로 이미 올라와 있는 항목이고, 킬 패널·시체 표시·지도 범주에 바로 보인다.
**항목 수가 26개인데 눈에 매우 잘 띈다 — 들인 품 대비 효과가 가장 크다.**

## 6. 재지 않은 것 — 플러그인 DLL 안에 박힌 인게임 UI

F12 창 밖에서 플러그인이 그리는 글자(패널 제목·버튼·HUD 문구)는 **DLL 안의 문자열 상수**라 locale 파일이 없다.
`SptKoreanConfig` 의 `UiStringPatch.cs` 가 이미 그 수를 쓴다 — **Harmony 트랜스파일러로 `ldstr` 을 불러올 때 바꿔치기**하는 방식이고, `ConfigurationManager.dll` 에 대해 33항목이 돌고 있다. 기술은 있다.

**다만 몇 개의 모드, 몇 항목인지 재지 않았다.** 플러그인 245개를 전부 뒤지는 것은 이 검토의 범위를 넘고, DLL 안의 문자열이 화면에 나오는 것인지 로그용인지는 파일만 보고는 못 가린다.
재려면 F12 작업 때와 같은 방법밖에 없다 — **화면을 하나씩 열어 보고 영문이 보이는 것을 적는다.**

## 7. 권하는 순서

항목 수가 적고 눈에 잘 띄는 것부터다.

| 단계 | 할 일 | 크기 | 왜 먼저인가 |
| --- | --- | --- | --- |
| **A** | 봇 역할 이름 26항목 | 26 | 킬 패널·시체·지도에 즉시 보인다. BACKLOG 에 이미 있다 |
| **B** | 서버 모드 얼개 만들기 (`PostLoad` 에서 `kr` locale 덮어쓰기 + 사전 파일 + 커버리지 검사 도구) | 코드 | A 부터 D 까지가 전부 이 위에 올라간다. F12 쪽 `check-lang-coverage.mjs` 와 같은 꼴의 검사를 함께 만든다 |
| **C** | ISB 자막 222줄 + 플러그인 locale 3개 | 338 | 파일만 넣으면 된다. 자막은 길이 열린 것을 확인했다 |
| **D** | 퀘스트 — 이름·설명 2,284 + 목표 4,461 + 대사 1,353 | 8,098항목 / 130만 자 | 읽어야 하는 글이라 영문이면 막힌다. 아이템 정보 블록의 영문 퀘스트 이름도 같이 고쳐진다 |
| **E** | 아이템 이름·약칭 중 장비·소모품 | 8,001 중 일부 | 탄약 이름은 바닐라도 영문이라 뺀다 |
| — | 아이템 설명 960 | **하지 않는다** | Item Info 가 갈아끼워 화면에 안 나온다(§3) |

### 먼저 정해야 할 것

- **D 를 손으로 할 것인가.** 130만 자다. 기계 번역을 쓸 것인지, 쓴다면 어디까지 믿을 것인지가 이 작업의 성격을 정한다. 퀘스트 설명은 상인이 말을 거는 글이라 어투가 있다.
- **바닐라 몫 6,441항목을 건드릴 것인가.** 모드 탓이 아니라 EFT 의 한국어가 원래 빈 자리다. 고치면 바닐라 퀘스트도 한국어가 되지만, 범위가 "모드 한글화" 를 넘는다.

## 8. 이 검토에서 쓴 방법과 한 번 틀린 것

| 무엇 | 어떻게 |
| --- | --- |
| 서버가 내려주는 locale | `node tools/stm-call.mjs client/locale/kr "{}"` (게임·서버가 켜진 상태, 읽기만) |
| 모드 locale 파일 | `user/mods` 아래 `*locale*` 경로의 JSON 202개를 파싱(주석 있는 JSONC·BOM 처리) |
| 자막·locale 코드 해석 | `ilspycmd` 로 `ISBSOF_Extras.dll`·`SPTarkov.Server.Core.dll` 역컴파일 |
| SPT 쪽 제약 | `SPTarkov.Server.Core.xml` 의 XML 주석 + `LocaleService` 역컴파일 |

**한 번 틀렸고 원인을 짚었다.** 처음에 `/client/locale/kr`·`/en`·`/ru`·`/jp` 가 **전부 똑같은 영문 사전**을 돌려줘 "모든 언어가 en 으로 떨어진다" 로 읽었다.
원인은 서버가 아니라 **Git Bash 의 경로 변환**이었다 — `/client/locale/kr` 이 윈도 경로로 바뀌어 서버가 받은 `language` 가 쓰레기 값이 되고, `GetLocaleDb` 가 설계대로 `en` 으로 떨어진 것이다.
`MSYS_NO_PATHCONV=1` 을 주고 다시 부르자 kr 은 한글 23,379, ru 은 키릴 37,309 로 제대로 갈렸다.

**앞으로 이 저장소에서 `tools/stm-call.mjs` 를 Git Bash 로 부를 때는 `MSYS_NO_PATHCONV=1` 을 주거나 앞 슬래시를 떼고 부른다.**
