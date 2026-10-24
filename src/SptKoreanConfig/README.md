# SptKoreanConfig — F12 설정 창 한글화

F12(ConfigurationManager) 창에 보이는 **모드 이름 · 섹션 이름 · 설정 이름 · 설명**만 한글로 바꾸는 BepInEx 플러그인이다.

## 무엇을 건드리고 무엇을 안 건드리나

| 건드리는 것 | 건드리지 않는 것 |
| --- | --- |
| ConfigurationManager가 화면에 쓰려고 만든 표시용 값 (`DispName`·`Category`·`Description`) | 설정 값 |
| `BepInPlugin.Name` (헤더에 뜨는 모드 이름, 끄면 원문으로 되돌림) | `BepInEx\config\*.cfg` 파일 |
| | 다른 모드의 DLL·파일 |

설정 목록은 F12를 열 때마다 새로 만들어지므로, 플러그인을 빼면 다음 실행에서 원래 영문으로 돌아간다.

## 구조

| 경로 | 내용 |
| --- | --- |
| `Plugin.cs` | `ConfigurationManager.BuildSettingList()` 에 Harmony Postfix. 목록이 만들어진 직후 표시용 필드만 덮어쓰고 섹션 묶음을 다시 만든다 |
| `TranslationStore.cs` | `lang/*.json` 을 읽어 GUID로 찾게 한다. 파일 수정 시각이 바뀌면 **게임을 끄지 않고** 다시 읽는다 |
| `UiStringPatch.cs` | 창 자체의 글자를 바꾼다. 아래 "창 자체의 글자" 참고 |
| `lang/<플러그인 GUID>.json` | 모드 한 개의 번역 사전 |
| `lang/_ui.json` | 창 자체의 글자. 밑줄로 시작하는 파일은 모드 사전으로 읽지 않는다 |

## 사전 형식

파일 이름은 모드의 BepInEx GUID (`BepInEx\config\<GUID>.cfg` 와 같은 이름).

```json
{
  "guid": "7Bpencil.TransparentSights",
  "name": "투명 조준경 (TransparentSights)",
  "sections": { "원문 섹션": "한글 섹션" },
  "settings": {
    "원문 섹션/원문 키": { "name": "한글 이름", "desc": "한글 설명" }
  }
}
```

- 원문 섹션·키는 `.cfg` 파일의 `[섹션]` 과 `키 = 값` 을 그대로 쓴다.
- 빠진 항목은 영문 원문이 그대로 나온다. 일부만 번역해도 된다.
- `name` 이나 `desc` 중 하나만 적어도 된다.

## 사전이 빠짐없는지 확인

```
node tools/check-lang-coverage.mjs                    전체 요약
node tools/check-lang-coverage.mjs com.tyfon.uifixes  빠진 항목까지
```

`.cfg` 의 실제 항목과 사전을 맞춰 보고, 누락·오타(cfg 에 없는 열쇠)·설명 없는 항목 수를 알려 준다. 사전을 하나 쓸 때마다 돌려 본다.

## F12 안의 자체 설정

| 항목 | 기본값 | 내용 |
| --- | --- | --- |
| 한글로 표시 | 켜짐 | 끄면 그 자리에서 영문으로 되돌아간다 |
| 번역 안 된 항목 표시 | 꺼짐 | 사전에 없는 항목 앞에 `·` 를 붙인다. 번역할 때만 켠다 |

## 빌드와 설치

```
dotnet build -c Release src/SptKoreanConfig
pwsh -File tools/install-koreanconfig.ps1
```

`E:\Spt\BepInEx\plugins\SptKoreanConfig\` 에 DLL과 `lang/` 이 들어간다. **DLL이 바뀌면 게임 재시작**, 번역 JSON만 고쳤으면 F12를 다시 열기만 하면 된다.

SPT 설치 위치가 다르면 `-p:SptDir=...` / `-SptDir ...` 로 넘긴다.

## 알아 둘 것

- F12 목록은 모드 이름순으로 정렬되므로, 한글 이름을 붙인 모드는 **영문 모드들 뒤로 내려간다.** 번역을 다 하면 다시 한글 가나다순으로 정렬된다.
- 설정을 뒤늦게 등록하는 모드가 있어 전체 항목 수는 실행 중에도 늘어난다(실측 992 → 1029). F12를 열 때마다 다시 적용하므로 문제는 없다.

## 창 자체의 글자 (`lang/_ui.json`)

창 제목·버튼·안내 문구는 모드 설정이 아니라 `ConfigurationManager.dll` 코드 안에 박혀 있다. 표시용 필드가 없으므로 다른 수를 쓴다 — **Harmony 트랜스파일러로 그 메서드들의 문자열 상수(`ldstr`)를 불러올 때 한글로 바꿔치기한다.** DLL 파일은 그대로 두므로 플러그인만 빼면 원문으로 돌아간다.

```json
{
  "strings": { "Close": "닫기", "Search: ": "검색: " }
}
```

- 왼쪽은 DLL 안의 원문이라 **끝의 공백까지 한 글자도 달라선 안 된다.** 다르면 그 항목은 조용히 무시된다.
- 사전에 있는 문자열을 실제로 쓰는 메서드만 골라 패치한다.
- 설정 섹션·키 이름(`General`, `Show advanced` 등)과 `ConfigurationManagerAttributes` 의 필드 이름은 바꾸면 `.cfg` 의 키가 달라지거나 덕 타이핑이 깨지므로, 사전에 적어도 **건너뛰고 경고를 남긴다**.
- 모드 사전과 달리 코드를 고치는 방식이라 **바꾼 뒤에는 게임을 다시 켜야 한다.**

원문이 DLL에 실제로 있는지 확인:

```
pwsh -File tools/check-ui-strings.ps1
```
