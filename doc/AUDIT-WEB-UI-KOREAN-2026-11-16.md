# 서버 웹 화면 한글화 점검 (2026-11-16)

내장 브라우저로 첫 화면 카드 순서대로 15개 모드와 본체 페이지를 열어, **하위 페이지·탭·왼쪽 메뉴 버튼까지**
눌러 보며 미번역·잘못된 번역·같은 낱말 다른 번역을 찾았다. 그 결과와 수정 계획, 진행 상황을 적는다.

관련 문서: [REVIEW-MOD-PAGE-KOREAN.md](REVIEW-MOD-PAGE-KOREAN.md)(번역기를 끼운 방법),
[SERVER-WEB-KOREAN.md](SERVER-WEB-KOREAN.md)(본체 로케일).

## 1. 어떻게 봤나

- 내장 브라우저(Claude 데스크톱 앱의 브라우저 창)에서 `https://127.0.0.1:6969/` 를 열었다.
  SPT 서버의 자가 서명 인증서 때문에 처음엔 막혔고(`ERR_CERT_AUTHORITY_INVALID`), 사용자가 경고를 넘긴 뒤 열렸다.
- 모드마다 같은 경로 아래의 **링크, `[role=tab]`, `nav`/`aside` 안의 버튼**을 눌렀다. 접힌 `<details>` 는 열었다.
- **저장·초기화·삭제·적용·만들기·내보내기·들여오기·버리기·복제**가 든 버튼은 누르지 않았다. 설정을 바꾸지 않으려는 것이다.
- 번역기에 "무엇을 무엇으로 바꿨는지" 남기게 해(`window.__ko.applied`) 용어 불일치를 찾았다.
- 내장 브라우저에서 검증한 크롤러를 그대로 저장소 도구로 옮겼다 — `tools/audit-web-ui.mjs`.
  헤드리스 Chrome 으로 돌린 결과가 내장 브라우저에서 잰 숫자와 맞았다(BIS 15, CCI 25, DWV 197, ISB 295, Lots of Loot 83).

```powershell
node tools/audit-web-ui.mjs out.json              # 전체(10분 남짓)
node tools/audit-web-ui.mjs out.json cci orbit    # 일부만
node tools/analyze-web-ui-audit.mjs out.json cand.json   # 모드별 표·용어 충돌·번역 후보
```

## 2. 찾은 것

### 2-1. 앞서 보고한 숫자는 범위 착시였다

`dump-web-ui-strings.mjs` 는 **최상위 페이지만** 봤다. 그래서 "미번역 363개, `/cci`·`/enablelabyrinth` 0" 이라고 했다.
하위로 들어가면 이렇다.

| 모드 | 앞서 보고 | 실제 | 왜 놓쳤나 |
| --- | ---: | ---: | --- |
| CCI | 0 | 25 | `/cci/config` 등 하위 페이지 |
| DWV | 6 | 197 | 섹션 이동이 링크가 아니라 **버튼** |
| ECOTI | 2 | 171 | 하위 페이지 |
| ORBIT | 2 | 586 | 하위 화면 43개 |
| Skills Extended | 16 | 1,539 | 스킬마다 설정 화면. 80쪽 상한에도 걸린다 |

→ **숫자가 아니라 돈 범위를 믿어야 한다.** 이후 점검은 `audit-web-ui.mjs` 로만 한다.

### 2-2. 같은 개념을 여러 말로 옮겼다

| 원문 | 섞여 쓰이던 것 | 공식 게임 한국어(`locales/global/kr.json`) |
| --- | --- | --- |
| item(s) | 아이템 / 품목 / 물건 | **아이템**. 물물교환 분류는 **교환 아이템** |
| Button | 단추 | **버튼** (`잠금 버튼이 달려있는…`) |
| Flea market | 플리마켓 | **플리마켓** (붙여 씀, 22건 모두) |
| Offer | 매물 | **매물** (`ADDOFFER` = 매물 등록) |
| Preset | 프리셋 | **프리셋** |

### 2-3. 서로 다른 뜻이 같은 한글로 — 화면에서 구분이 안 된다

| 한글 | 겹친 원문 | 어디서 보이나 |
| --- | --- | --- |
| 기본 | `Main`, `Vanilla`, `Default`, `Essentials`, `default`(프리셋 이름) | CCI 메뉴에 "기본"과 "기본 제작법"이 다른 뜻으로 나란히 |
| 설정 | `SETTINGS`, `Config` | CCI 메뉴가 `설정 / 기본 / 설정` |
| 구역 | `SECTION`(설정 묶음), `Zones`(지도 구역) | ISB 와 ORBIT |
| 전리품 설정 | `Loot configuration`(머리말), `Loot settings`(카드) | Lots of Loot 홈 한 화면에 둘 다 |
| 고급 | `Advanced`(UI 라벨), `Advanced`(DWV 품질 등급 이름) | DWV 등급 목록에서 이것 하나만 한글로 튐 |

`default` 는 Lots of Loot 의 **프리셋 이름**이다. 사전에 "default → 기본" 이 있어 이름까지 바꿀 수 있었다.

### 2-4. 쪼개진 텍스트 노드 — 어순이 뒤집힌다 (구조 문제)

Razor 는 `Add @label` 을 텍스트 노드 둘(`Add`, ` item`)로 그린다. 번역기가 낱개로 바꾸면:

| 화면에 보인 것 | 원문 | 문제 |
| --- | --- | --- |
| 추가 품목 | Add item | "추가된 품목"이라는 다른 뜻이 된다 |
| 0 저장 안 한 변경 | 0 Unsaved Changes | 어순 |
| 열기 품질 등급 생성 (8 개 설정) | Open Generate Quality Tiers (8 settings) | 어순, 규칙이 안 걸림 |

이걸 메우려고 넣어 둔 **조각 열쇠**(`in`→에서, `item`→품목, `settings)`→개 설정), `wide`, `grids`, `Over`)는
엉뚱한 자리도 바꿀 수 있다. 낱말을 하나씩 고칠 일이 아니라 **번역기 구조**를 고쳐야 한다.

### 2-5. 영문+한글이 붙은 아이템 이름 — 우리 버그가 아니다

`Antique teapot 골동품 찻주전자`, `Lion 청동 사자상` 같은 값이 117개 보인다.
**공식 kr 로케일 자체가 그렇다** — `"59e3639286f7741777737013 Name": "Lion 청동 사자상 "`. BSG 가 검색용으로 영문을
앞에 붙여 둔 것이라 게임 안에서도 똑같이 보인다. 건드리지 않는다.

### 2-6. 미번역 기준표 (수정 전)

종류는 `tools/lib/web-ui-classify.mjs` 로 가른다. 사람 이름·식별자는 바꾸면 안 되는 것이라 영원히 줄지 않는다.

| 모드 | 화면 | 미번역 | 식별자 | 링크·버전 | 치수·수치 | 아이템 이름 | 사람·모드 이름 | 번역 후보 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| bis | 1 | 15 | 0 | 0 | 0 | 12 | 1 | 2 |
| cci | 10 | 25 | 0 | 0 | 0 | 0 | 2 | 23 |
| definitive-weapon-variants | 17 | 197 | 0 | 0 | 0 | 13 | 7 | 177 |
| coti | 4 | 171 | 0 | 0 | 3 | 15 | 61 | 92 |
| enablelabyrinth | 2 | 11 | 0 | 0 | 0 | 0 | 2 | 9 |
| isb | 19 | 295 | 6 | 0 | 1 | 0 | 34 | 254 |
| jerobackpack | 3 | 129 | 0 | 0 | 27 | 25 | 5 | 72 |
| lotsofloot | 4 | 83 | 0 | 4 | 0 | 2 | 16 | 61 |
| orbit | 43 | 586 | 0 | 1 | 3 | 5 | 65 | 512 |
| questcodex | 9 | 849 | 0 | 0 | 0 | 105 | 46 | 698 |
| sain | 3 | 112 | 0 | 4 | 0 | 2 | 20 | 86 |
| skills-extended | 80+ | 1539 | 0 | 0 | 1 | 33 | 101 | 1404 |
| theblacklist | 4 | 64 | 0 | 3 | 0 | 0 | 16 | 45 |
| ultimateflea | 13 | 241 | 34 | 4 | 0 | 11 | 51 | 141 |
| webconfigbridge | 11 | 40 | 0 | 0 | 0 | 2 | 1 | 37 |
| core | 12 | 126 | 0 | 4 | 5 | 11 | 92 | 14 |
| **합계** | | **4483** | 40 | 20 | 40 | 236 | 520 | 3627 |

화면 수 뒤의 + 는 크롤러 상한(80)에 걸렸다는 뜻이다.

## 3. 용어 기준표

기준은 공식 게임 한국어(`locales/global/kr.json`) → 본체 웹 로케일(`src/ServerWebKorean/ko.json`) →
한국어를 직접 들고 있는 모드(QuestCodex) 순이다. 셋이 다 없으면 화면에서 겹치지 않는 쪽을 고른다.

| 원문 | 표기 | 근거 |
| --- | --- | --- |
| Item(s) | **아이템** | 공식. 품목·물건은 쓰지 않는다 |
| Barter items | **교환 아이템** | 공식 핸드북 분류 이름 |
| Button | **버튼** | 공식(`잠금 버튼이 달려있는…`). 단추는 쓰지 않는다 |
| Flea market / Ragfair | **플리마켓** | 공식, 붙여 쓴다 |
| Offer | **매물** | 공식(`ADDOFFER` = 매물 등록) |
| Preset | **프리셋** | 공식 |
| Loot | **전리품** | 공식 |
| Settings / Config / Configuration | **설정** | 본체 웹이 "설정 편집기". 단 `/cci` 메뉴의 `Config` 는 **일반 설정**(같은 메뉴의 `SETTINGS` 와 겹침) |
| Default | **기본값** (UI 라벨) | 공식 `Default` = 기본. "기본" 하나에 뜻이 몰리지 않게 값은 기본값 |
| Main | **주요** | "기본"과 겹침 해소 |
| Vanilla | **바닐라** | QuestCodex 가 스스로 쓰는 말. "원본"은 본체가 raw 에 씀(원본 JSON) |
| Essentials | **필수 설정** | "기본 설정"과 겹침 해소 |
| Section | **섹션** | "구역"은 ORBIT 의 지도 구역(Zones) 몫 |
| Loot settings (Lots of Loot 카드) | **전리품 세부 설정** | 같은 화면의 머리말 `Loot configuration`(전리품 설정)과 겹침 |
| Raw | **원본** | 본체 웹 |
| Reload / Refresh | **다시 읽기 / 새로 고침** | 둘은 다른 동작이라 나눈다 |
| On / Off / Enabled | **켬 / 끔 / 켬** | |
| 릴리스 노트의 `Update(d) to X` | **X 대응** | "올림/올렸다"가 섞여 있었다 |

**번역하지 않는 것**: 사람·작성자 이름, 모드 이름, 모드 상인 이름(`Case`, `Medic`, `Merchant`…),
프리셋 이름(`default`, SAIN 의 `Born Free` 등), DWV 품질 등급 이름(`Unique`…`Advanced`…`Meme`),
설정 키·MongoID·파일명·URL, 공식 로케일이 영문으로 둔 아이템 이름.

## 4. 수정 계획

순서는 **한 번에 가장 많이 줄이는 것 → 정확도를 올리는 것 → 양을 채우는 것**이다.

| # | 할 일 | 왜 이 순서인가 |
| --- | --- | --- |
| A | **번역기 구조** — 이웃 텍스트 노드 이어 붙이기, 규칙 캡처 번역, 페이지별 예외(`_scoped`), Unity 태그 떼기 | 어순 깨짐·조각 열쇠·태그 찌꺼기가 한 번에 풀린다. 사전보다 먼저 해야 사전이 맞는 열쇠로 채워진다 |
| B | **용어 통일** — 3절 기준표대로 사전·카드·본체 로케일을 고친다. 짧은 조각 열쇠(`in` `item` `Over` `wide` `grids` `default` `settings)`)를 뗀다 | 채우기 전에 기준을 박아야 새 번역이 기준을 따른다 |
| C | **모드별 채우기** — 화면이 적고 문구가 짧은 것부터: BIS → Labyrinth → 본체 → CCI → WCB → Blacklist → Lots of Loot → jerobackpack → SAIN → ECOTI → UltimateFlea → DWV → ISB → ORBIT → Skills Extended | 작은 모드에서 용어가 굳으면 큰 모드가 그대로 따른다 |
| D | **다시 점검** — `audit-web-ui.mjs` 로 전체를 다시 재고, 내장 브라우저로 눈으로 본다 | 숫자만으로는 맥락 오역을 못 잡는다 |

QuestCodex 는 C 에서 따로 다루지 않는다. 남은 것은 대부분 아이템·퀘스트 이름(게임 로케일 소관)이다.

## 5. 진행

### A. 번역기 구조 — 끝

내장 브라우저에서 그 자리를 다시 열어 확인했다.

| 자리 | 전 | 후 |
| --- | --- | --- |
| DWV 목록 추가 버튼 | 추가 품목 | 항목 추가 (이 화면에서는 게임 아이템이 아니라 목록 칸) |
| DWV 접힌 묶음 | 열기 품질 등급 생성 (8 개 설정) | 품질 등급 생성 열기 (설정 8개) |
| DWV 품질 등급 | Unique … **고급** … Meme | Unique … **Advanced** … Meme |
| CCI 메뉴 | 설정 / 기본 / 설정 / … / 기본 제작법 | 설정 / 주요 / 일반 설정 / … / 바닐라 제작법 |
| CCI 바닥 | 0 저장 안 한 변경 · 되돌리기 단추 보이기 | 저장 안 한 변경 0개 · 되돌리기 버튼 보이기 |
| QuestCodex 아이템 | `<b><color=#FFFFFF>Dorm room 103 key</color></b>` | 태그 없이 표시(화면에 보이는 태그 0) |

#### 점검 중에 찾은 번역기 버그 — rAF

번역기는 변경을 모았다가 `requestAnimationFrame` 으로 한 번에 처리했다. 그런데 **Claude 앱의 브라우저 창에서
`visibilityState` 가 "visible" 인데도 rAF 가 1.5초 안에 한 번도 불리지 않았다.** 그러면 "대기 중" 표시가 굳어
**첫 변경 이후 모든 변경을 흘려버린다** — 새 번역기를 처음 깔았을 때 화면이 통째로 영문으로 돌아간 원인이다.
옛 번역기도 같은 구조였다. 백그라운드 탭에서도 rAF 는 멈추므로 실제 사용자에게도 날 수 있다.
`setTimeout(flush, 30)` 으로 바꿨다 — 느려질 수는 있어도 멈추지는 않는다.

#### 점검 중에 찾은 오역 — 상인 이름

QuestCodex 상인 목록에서 CaseTrader 모드의 상인 `Case` 가 **"케이스"**로 바뀌어 있었다. 아이템 사전 충돌을 막으려고
넣은 "Case → 케이스"가 고유명까지 덮은 것이다. 뗐다.

### B. 용어 통일 — 끝

사전 50건, 카드 설명 2건, 본체 로케일 1건(`상인 물건` → `상인 아이템`)을 고쳤다. 이후 점검에서 나온 충돌 3건
(`SECTION`·`Loot settings`·`Case`)과 Vanilla 표기(바닐라)를 더 고쳤다.

### C. 모드별 채우기 — 진행

패치 파일로 나눠 넣는다(`src/ServerWebUiKorean/patches/NN-*.json`). `tools/merge-web-ui-dict.mjs` 가 합치면서
**이미 있는 열쇠를 다른 값으로 덮으면 알린다** — 용어가 갈리는 곳이라 조용히 바꾸지 않는다.

| 차 | 모드 | 번역 후보 전 → 후 | 남은 것 |
| --- | --- | --- | --- |
| 1 | CCI · Enable Labyrinth · BIS · 본체 | 76·9·2·17 → 0 | 모드·프로필 이름, 공식 kr 도 영문인 아이템 |
| 2 | Web Config Bridge · The Blacklist · Lots of Loot · jerobackpack | 37·47·70·73 → 0 | 모드 이름·버전, 아이템 이름, 치수 |
| 3 | SAIN · ECOTI | 86·91 → 0 | SAIN 프리셋 이름(일부러), 튜브 식별자·좌표·메시 이름 |
| 4 | UltimateFlea | 167 → 0 | 설정 키(`wipe.enabled`)·열거값(`live`, `exact`)·패키지 이름 |
| 5 | DWV | 192 → 0 | 상인·모드 이름. 품질 등급 이름은 "원문 유지" 표시 |
| 6 | ISB | 254 → 0 | 세력·부대·봇 종류 고유명, 인식표 이름(공식 kr 도 영문) |
| 7 | ORBIT | 516 → 0 | 단위 값(80 m), 구역·세력 식별자 |

#### 채우면서 찾은 것

- **같은 낱말, 페이지마다 다른 뜻** — `Scope` 는 Blacklist 에서 "대상 범위", ECOTI 에서 "조준경". `Key` 는 DWV 키-값 표에서
  "키", 다른 데서는 "열쇠". `Charge`·`Mount` 는 WCB 슬롯 분류 이름. → 전역값과 페이지별 예외(`_scoped`)로 나눴다.
  ECOTI 의 방향 낱말(Back, Left, Top), ORBIT 의 한 낱말 라벨(Base, Floor, Boss)도 그 페이지에서만 바꾼다.
- **고유명을 번역했던 것** — 앞서 `Black Division` 을 "흑색 사단"으로 넣었다. 공식 표기가 없는 ISB 모드의 세력 이름이라
  `Chaos Insurgency`·`White Tusk` 처럼 원문으로 되돌렸다. `Case`(CaseTrader 상인)도 같은 이유로 뗐다.
- **분류기의 맹점** — 한 낱말 영어를 모두 "사람·모드 이름"으로 넣어, `Rewards`·`Repeatable`·`Keycards`·`Hotspots` 같은
  진짜 라벨이 후보에서 빠졌다. `tools/list-web-ui-names.mjs` 로 모드마다 그 갈래를 사람이 훑는다.

#### 채우면서 찾은 번역기 버그

| 버그 | 증상 | 고침 |
| --- | --- | --- |
| 규칙이 원문을 돌려주면 "번역됨"으로 쳤다 | 일반 규칙(`$1 ×$2`)이 캡처를 못 바꿔도 미번역 목록에서 빠져 구멍이 가려졌다 | 결과가 원문과 같으면 미번역 |
| 앞의 일반 규칙에서 멈췄다 | `Day 0, Multiplier: ×1.8` 처럼 **이미 규칙이 있는 문장**이 안 바뀌었다 | 결과가 원문과 같으면 다음 규칙으로 |
| SVG `<style>` 을 못 걸렀다 | ORBIT 지도의 CSS(`#orbit-rush-canvas * { … }`)가 번역 대상으로 잡혔다. SVG 안에서는 tagName 이 소문자다 | 대소문자 무시 비교 |
| 점검 도구 주입 정규식 | 셸 heredoc 이 `\` 를 `\` 로 줄여 템플릿 문자열 안의 `\s` 가 `s` 가 됐다 | Edit 로 직접 고침. 이후 정규식 든 파일은 셸로 쓰지 않는다 |
| 8 | Skills Extended | 1,792 → 0 | 좌표·캐시/자물쇠 장소 이름·총기 이름·해킹 미니게임 요소 이름 |

#### 스킬 이름을 공식 표기로 — 앞서 넣은 17개가 틀렸다

공식 kr 로케일과 대조해 보니 제가 넣은 스킬 이름 17개가 공식과 달랐다. **특히 "체력"은 Endurance 에 썼는데
공식은 Health 가 "체력"이라 뜻이 엇갈려 있었다.**

| 원문 | 넣었던 것 | 공식 |
| --- | --- | --- |
| Endurance | 체력 | **지구력** |
| Health | 건강 | **체력** |
| Vitality | 생명력 | **활력** |
| Intellect | 지능 | **지력** |
| Perception | 지각 | **인지능력** |
| Immunity / Stress Resistance | 면역 / 스트레스 저항 | **면역력 / 스트레스 저항력** |
| Covert Movement / Prone Movement | 은밀 이동 / 포복 이동 | **은밀한 움직임 / 포복 기동** |
| Field Medicine / First Aid | 야전 의료 / 응급처치 | **야전 의술 / 응급 처치** |
| Weapon Maintenance | 총기 정비 | **무기 유지보수** |
| Throwing | 투척 | **투척무기** |
| BEAR Raw Power | BEAR 강인함 | **BEAR 원초적 힘** |
| Launchers | 유탄발사기 | **로켓 발사기** |
| Sniper Rifles (스킬) | 저격소총 | **볼트액션 소총** (Skills Extended 페이지에서만) |

공식에 없는 것은 Skills Extended 의 자체 kr.json 을 따랐다 — NATO / Eastern Weapon Proficiency 는 그 모드가 게임 안에서
**서구권 / 동구권 총기 숙련도**로 쓴다. 그것도 없는 USEC 스킬은 공식 BEAR 스킬 표기(BEAR 강습 작전, BEAR 지휘권)의 결로
**USEC 협상 / 전술 / 장거리 광학 / 심화 무기 개조**로 정했다.

### D. 다시 점검 — 끝

`audit-web-ui.mjs` 로 전체를 다시 쟀다(Skills Extended 가 80쪽 상한에 걸려 150 으로 올렸더니 108쪽까지 들어갔다).

| 모드 | 화면 | 미번역 전 → 후 | 번역 후보 전 → 후 | 남은 후보가 무엇인가 |
| --- | ---: | ---: | ---: | --- |
| BIS | 1 | 15 → 16 | 2 → 2 | 모드 이름, 아이템 이름 |
| CCI | 10 | 25 → **0** | 23 → **0** | |
| DWV | 17 | 197 → 4 | 177 → 2 | 상인·모드 이름 |
| ECOTI | 4 | 171 → 53 | 92 → 20 | 튜브 식별자·좌표·아이템 이름 |
| Enable Labyrinth | 2 | 11 → **0** | 9 → **0** | |
| ISB | 19 | 295 → 110 | 254 → 14 | 세력·부대 고유명 |
| jerobackpack | 3 | 129 → 55 | 72 → 6 | 아이템 이름 |
| Lots of Loot | 4 | 83 → 13 | 61 → 6 | 모드 이름·버전 |
| ORBIT | 43 | 586 → 58 | 512 → **0** | |
| QuestCodex | 9 | 849 → 285 | 698 → 202 | 퀘스트·모드 아이템 이름(kr 로케일에도 영문) |
| SAIN | 3 | 112 → 36 | 86 → 10 | 프리셋 이름(일부러)·버전 |
| Skills Extended | 80 → 108 | 1,539 → 795 | 1,404 → 52 | 장소·모드 이름, 문장 조각 하나 |
| The Blacklist | 4 | 64 → 8 | 45 → 5 | 모드 이름·버전 |
| UltimateFlea | 13 | 241 → 103 | 141 → 22 | 설정 키(`events[].id`)·아이템 이름 |
| Web Config Bridge | 11 | 40 → 11 | 37 → 10 | 모드 이름 |
| 본체 | 12 | 126 → 123 | 14 → 14 | 모드·프로필 이름 |
| **합계** | | **4,483 → 1,670** | **3,627 → 365** | |

**남은 번역 후보 365개를 하나하나 확인했다 — 번역할 UI 문구는 없다.** 분류기가 다 못 거르는 이름·식별자·아이템
이름이다. 남은 미번역 1,670 은 좌표·수치 556, 사람·모드·장소 이름 459, 아이템 이름 228, 식별자 43, 링크·버전 19 다.

#### 다시 점검하며 내장 브라우저로 더 찾은 것

| 자리 | 문제 | 고침 |
| --- | --- | --- |
| Skills Extended 설명 | "XP 공유" — 다른 데선 "경험치" | 경험치 나눔 |
| Skills Extended 바닥 | "46 켬 · 대기 중인 변경 0개" — 숫자가 다른 요소에 있다 | 규칙 `enabled · N pending changes` → "개 켬 · …" |
| ORBIT 위쪽 메뉴 | `Custom` 이 "직접 지정", 같은 모드 안 `Custom 1` 은 "사용자 지정" | 사용자 지정 |
| ORBIT 설명 | `Spawn sectors` 가 "스폰 구역" — sector 는 섹터로 정했다 | 스폰 섹터 |
| ECOTI | `Scale`(3D 배율)과 `Size` 가 둘 다 "크기" | Scale → 배율 |

Skills Extended 머리말의 "스킬 확장"은 **번역기가 아니라 모드가 스스로** 자기 kr.json 으로 그리는 것이라 그대로 둔다.

#### 서로 다른 영문이 같은 한글 — 남겨 둔 것

분석기가 32쌍을 뽑았다. 모두 봤고, 같은 뜻이라 그대로 둔다:
`Categories`/`Category`(분류), `Min`/`Minimum`(최소), `Remove`/`Clear`(지우기), `Revert`/`Reset`(되돌리기),
`Flea market`/`Ragfair`(플리마켓), `Pistols`/`Handgun`(권총), `Submachine Guns`/`SMG`(기관단총),
`Config`/`General Config`(CCI 의 같은 페이지 — 일반 설정), `Update to`/`Updated to`(릴리스 노트 — 대응) 등.

### 번역하지 않는 것 (정리)

| 무엇 | 왜 |
| --- | --- |
| 모드·사람·상인·프리셋 이름 | 고유명. SAIN 프리셋 이름은 SAIN 안내 글과 짝이 맞아야 한다 |
| 세력 이름 White Tusk·Ghost Busters | 공식 표기가 없는 모드 고유명. Black Division·Chaos Insurgency 는 **게임 locale 이 블랙 디비전·카오스 인서전시로 쓰므로** 웹도 맞췄다(아래 E) |
| DWV 품질 등급 이름 | 게임 안 아이템 이름에 그대로 붙는다 |
| 인식표 이름(`Dogtag BEAR — …`) | 공식 kr 도 영문 |
| 설정 키·열거값·MongoID·문 ID·파일명·URL | 파일에 그대로 쓰는 값 |
| 캐시·자물쇠 장소 이름(`Bus Depot`, `Oli - 3`) | Skills Extended 의 맵 지점 고유명 |
| kr 로케일에도 영문인 아이템 이름 | 웹에만 넣으면 게임과 어긋난다 — `SptKoreanLocale` 에 넣어야 게임·웹이 함께 잡힌다 |

### 재기동해야 반영되는 것

아래 셋은 서버 기동 때 읽히는 값이라, **고쳐 두었지만 서버를 다시 켜야 화면에 나온다.**

- 모드 카드 설명 2건(`src/ModCardKorean/mod-cards.ko.json`): BIS "교환 아이템…", Blacklist "…금지 아이템…"
- 본체 웹 로케일 1건(`src/ServerWebKorean/ko.json`): `database-preparing-assort-items` "상인 아이템 {0} 개를 준비한다."
  — `pwsh -File tools/install-server-web-locale.ps1` 로 다시 깔아야 한다.

나머지(번역기·사전)는 모드 폴더에 복사하면 새로고침만으로 반영된다.

### E. QuestCodex 아이템 이름 — 사전은 넣었고 재기동 대기

QuestCodex 에 남은 202건은 웹이 아니라 **게임 locale 의 몫**이다(kr 로케일에도 영문). 그래서 웹 사전이 아니라
`SptKoreanLocale` 에 넣었다 — 그래야 게임 화면과 웹이 같은 말을 쓴다.

| 무엇 | 수 | 처리 |
| --- | ---: | --- |
| locale 열쇠가 있는 이름 | 175 | `src/SptKoreanLocale/lang/item-names-questcodex-1.json` — Name 174 + ShortName 95, `14-4 KORD SSD` 1개는 모델 코드라 keptAsIs |
| 그중 바닐라인데 공식 kr 도 영문 | 96 | 공식 결(브랜드·모델·위장 무늬는 영문, 물건 종류만 한국어)로 옮겼다 |
| 그중 모드가 이름을 바꿔 공식 한국어가 덮인 것 | 2 | Red Bull(원래 "Hot Rod 핫 로드 에너지 드링크"), SilencerCo Osprey 9 2.0 — 공식 낱말(소음기 등)을 되쓴다 |
| 열쇠가 없는 것 | 44 | 프로필·상인·SAIN 프리셋 이름, `Dorms V-Ex · PMC` 같은 탈출구, `· USEC body` 같은 시신·상자 꼬리, 그리고 **번역기 회귀로 합쳐진 문자열** |

방탄판의 `(3/50)` 같은 꼬리는 앞선 변환기가 locale 값에 써 넣은 것이라 값에 함께 적었다. 색 태그·●○ 는 넣지 않았다.
낱말은 퀘스트 목표 사전과 맞췄다(등대지기·프라퍼·레프·스키어·크노소스·래버린스·테라그룹·USEC 정보지원국).

**번역기 회귀.** 한 요소 안에 글자 조각이 둘 이상이고 통째로 사전에 없으면 원문으로 두게 했었는데, QuestCodex 의
"아이템, 아이템, …" 목록은 조각마다 노드가 갈려 통째로는 영영 안 맞는다. 조각이 **전부** 사전에 있으면(원문 유지 `null`·이미
한국어 포함) 낱개로 바꾸고, 하나라도 빠지면 지금처럼 원문으로 둔다. 다시 점검하니 합쳐진 문자열은 미번역 목록에서 사라졌다.

**세력 이름.** 게임(ISB 아이템 사전)은 블랙 디비전·카오스 인서전시인데 웹만 영문이었다. patch 06 의 값·규칙 28곳을 바꾸고
낱말 단독 항목을 patch 14 로 더했다. ISB 미번역 110 → 98. 남은 `Black Division Dogtag — …`·`Dogtag BEAR — Black Division …` 은
인식표 이름(공식 kr 도 영문)이라 둔다.

**반영 상태 — 끝(2026-11-16 20:12 재기동).** 게임을 끈 뒤 `tools/install-koreanlocale.ps1` 로 깔고 서버를 다시 켰다.
`check-locale-dict`: **OK — 8737/8737 적용, 빠뜨린 열쇠 0**(questcodex 사전 269건 포함). QuestCodex 미번역 **285 → 120**,
사전 대상 175 중 남은 것은 일부러 영문으로 둔 `14-4 KORD SSD` 하나다.

**번역기를 고치자 새로 드러난 것.** 합쳐진 문자열 안에 숨어 있던 아이템 이름이 낱개로 보이게 됐다 — locale 열쇠가 있는
미번역 50개(배낭·총기·탄창·플래시 드라이브 등, 예: `Mystery Ranch Blackjack 50 backpack (USEC Intelligence Support Bureau)`,
`Magnum Research Desert Eagle L5 .50 AE pistol`). 이번 202건에는 없던 것이라 넣지 않았다. 그중 하나라도 사전에 없으면
그 목록 줄은 전부 원문으로 남는다(위 규칙) — ISB 배낭 목록처럼 여러 이름이 붙은 줄이 아직 남는 이유다. 나머지 70개는
상인·모드·프리셋 이름과 `· PMC` 탈출구처럼 번역하지 않는 것이다.
