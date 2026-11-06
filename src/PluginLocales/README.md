# PluginLocales — 모드 폴더 안에 넣는 한국어 locale

제 `locales/` 폴더를 읽는 게임 플러그인에 한국어 파일을 넣는다. 지금 **네 파일 / 338항목**이다.

| 대상 | 플러그인 | 항목 | 넣는 자리 |
| --- | --- | --- | --- |
| 무전·방송 자막 | ISB Special Forces (SOF Extras) | **222** | `ISBSpecialForcesPlugin\ISBSpecialForces_Audios\Subtitles\locales\kr.json` |
| 보관함 정렬 창 | Advanced Stash Sorting | 104 | `AdvancedStashSorting\locales\kr.json` |
| 위시리스트 확장 | Wishlist Extended | 10 | `WishlistExtended\locales\kr.json` |
| 주변 아이템 패널 | Softwyx Loot In Vicinity | 2 | `Softwyx-LootInVicinity\locales\kr.json` |

범위 전체는 [doc/REVIEW-KOREAN-LOCALIZATION.md](../../doc/REVIEW-KOREAN-LOCALIZATION.md) 에 있다.
게임 DB 쪽(아이템·퀘스트·봇 역할 이름)은 [SptKoreanLocale](../SptKoreanLocale), F12 창은 [SptKoreanConfig](../SptKoreanConfig) 가 맡는다.

## 왜 저장소에 두고 스크립트로 넣나

**모드 폴더 안이라 그 모드를 업데이트하면 날아간다.** 그래서 원본은 여기 두고 설치는 스크립트가 한다.
모드의 파일(`en.json` 등)은 건드리지 않고 `kr.json` 만 새로 넣는다.

```
pwsh -File tools/install-plugin-locales.ps1            넣기
pwsh -File tools/install-plugin-locales.ps1 -Remove    되돌리기
pwsh -File tools/install-plugin-locales.ps1 -WhatIf    무엇을 건드릴지만 보기
node tools/check-plugin-locales.mjs                    원본과 맞는지 + 설치본과 같은지
```

게임 플러그인이라 **게임을 다시 켜야 적용된다.** 설치 스크립트는 게임이 켜져 있으면 중단한다.

## locale 코드는 `kr` 이다

추측이 아니다. 서버 로그(`E:\Spt\SPT_Runtime\user\logs\spt\spt<날짜>.log`)에 게임이 보낸 요청이 남아 있다.

```
[클라이언트 요청] /client/locale/kr
```

서버 웹 화면의 `ko` 와 다르다 — 그쪽은 `locales/web/` 소관이고 이것과 무관하다.

## 네 모드가 코드를 고르는 방식 (역컴파일 실측)

넷 모두 `locales/` 를 훑어 **파일 이름을 locale 코드로** 쓰고, 빠진 키는 영어로 떨어진다.

| 플러그인 | 코드를 어디서 얻나 | 파일·키 규칙 | 빠진 키 |
| --- | --- | --- | --- |
| ISB SOF Extras | `Subtitle Language = Auto` → EFT locale. `NormalizeLocale` 이 모르는 코드를 **그대로 돌려준다** | `locales/<코드>.json` 의 **존재**를 본다(`FindAvailableLocale`). 설정 설명의 "Supported: en, pt, ru and ch" 는 허용 목록이 아니다 | `ResolveText` 가 키마다 영어로 (조용히) |
| Advanced Stash Sorting | `Localization.Culture = LocalizationManager.Culture` | `locales/*.json`, 파일 이름이 코드 | `GetRaw` 가 키마다 `en` 으로 (조용히) |
| Wishlist Extended | `LocalizationManager.ContainsCulture(코드)` 가 참이어야 한다 | `locales/<코드>.json` | `en` 에서 채우고 **키마다 경고** |
| Softwyx Loot In Vicinity | `LocaleApplicationLanguagePatch` | 파일 이름 `^[a-z]{2}([_-][a-z]{2})?$`, 키 `^lootinvicinity_[a-z0-9_]+$` | `MergeMissingKeys` 가 `en` 에서 채우고 **경고** |

뒤의 둘이 경고를 남기므로 **일부만 번역하지 않고 전부 넣었다.** 그러면 로그가 깨끗하고, 경고가 보이면 그것이 곧 빠뜨린 신호가 된다.

## 파일 형식

ISB 자막만 모양이 다르다(`entries` 배열). 나머지 셋은 평평한 `{"키": "문구"}` 다.
`manifest.json` 의 `format` 이 그것을 적어 두고, 검사 도구가 그에 맞춰 읽는다.

```json
{ "locale": "kr", "entries": [ { "key": "speaker.intercom", "text": "<b>ISB 관제관</b>" } ] }
```

- `<b>`·`<color=...>` 같은 태그와 `\n` 은 **원본 그대로 둔다.**
- `{0}`·`{1}` 자리표시자도 그대로 둔다 — 검사 도구가 개수를 대조한다.
- 번역 규칙은 바닐라 한국어 locale 을 따른다. 분류 이름은 바닐라 `kr.json` 에서 읽어 왔다(의약품·응급 치료 키트·돌격소총·총열덮개·장전 손잡이 …).

## 검사 도구가 보는 것

| 항목 | 왜 |
| --- | --- |
| 원본에 있고 우리에게 없는 열쇠 | 그 항목은 영문으로 나온다 |
| 우리에게 있고 원본에 없는 열쇠 | 오타다. 조용히 무시되므로 스스로는 드러나지 않는다 |
| 중복된 열쇠 | 뒤의 것이 이긴다 |
| 자리표시자 수가 원본과 다른 값 | `{0}` 이 빠지면 화면이 깨진다 |
| 한글이 없는 값 | 숫자·고유명사면 정상이라 **문제로 세지 않고 보여만 준다** |
| 설치본과 같은지 | 모드를 업데이트하면 날아간다. 그것을 잡는 자리다 |

## 시크릿 검사가 자막 파일을 짚는다 — 거짓 양성이다

Semgrep(gitleaks `generic-api-key`)이 `ISBSpecialForcesPlugin/subtitles-kr.json` 에서 **INFO 4건**을 낸다.
짚는 자리는 번역문이 아니라 **자막 열쇠 문자열**이다.

| 줄 | 짚힌 것 |
| --- | --- |
| 34~36 | `"ISB-LabsChaos.01"` · `.02` · `.03` |
| 51 | `"ISB-D9341Obj.01"` |

JSON 의 열 이름이 하필 `key` 이고 값이 영문·숫자·하이픈·점으로 된 토큰이라 규칙에 걸린다.
222항목 가운데 이 넷만 걸리는 것은 gitleaks 가 요구하는 길이·엔트로피를 우연히 넘기기 때문이다.

**고치지 않는다.** 열 이름은 우리 것이 아니다 — `ISBSOF_Extras.dll` 의 모델이 `key`·`text` 를 요구한다.

```csharp
internal sealed class ISBSubtitleLocaleEntry
{
    public string key { get; set; }
    public string text { get; set; }
}
```

이름을 바꾸면 자막이 아예 안 올라온다. 이 폴더의 파일은 **번역 문구뿐이고 자격 증명이 들어갈 자리가 없다.**

`.semgrepignore` 로 덮는 길도 있지만 저장소에 그런 설정이 아직 없고, 경로를 통째로 빼면 **나중에
그 자리에 생길 진짜 건까지 같이 가려진다.** 그래서 설정을 만들지 않고 여기에 적어 둔다.

## 아직 안 본 것

**파일이 맞는 것과 플러그인이 집어 가는 것은 다르다.** 게임에서 눈으로 본 적은 없다.
다음에 게임을 켜면 `E:\Spt\BepInEx\LogOutput.log` 에서 (메뉴까지만 켜도) 이 줄을 본다.

```
Validated locale packs: en, kr.          ← Loot In Vicinity
Merged 2 locale entries for 'kr'.        ← Loot In Vicinity
Loding locale: kr                        ← Wishlist Extended (모드 쪽 오타다)
```

경고가 없으면 전수 적용된 것이다. Advanced Stash Sorting 은 로그를 남기지 않아 보관함 정렬 창을 열어 봐야 하고,
ISB 자막은 `com.samc137.isbsofextras.cfg` 의 진단 로그를 켜야 `Subtitle locale loaded: kr.` 이 찍히며,
실제로 들으려면 **랩스 레이드**가 필요하다.
