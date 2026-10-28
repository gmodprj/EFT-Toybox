# Olympus 505 — `Modifiers/firearmMags.json` 에서 없는 탄창 41개 제거

2026-10-28. [505-olympus-firearmMags.md](505-olympus-firearmMags.md) 의 후속이다. 그때는 **없는
총기** 12개를 뺐고, 이번에는 **없는 탄창**을 뺐다.

## 무엇이 잘못돼 있었나

`firearmMags.json` 의 구조는 `총기 id → { maglist: [탄창 id…] }` 이고, Olympus 는 그 탄창 id 를
총기의 탄창 칸 필터에 끼워 넣는다(로그 `AddMagazinesToFirearms`). 그런데 모드 총기에 끼우는
탄창 41종이 **설치본에 존재하지 않았다.**

| 총기 출처 | 탄창 참조 | 있음 | 없음 |
| --- | --- | --- | --- |
| 바닐라 | 203 | 203 | 0 |
| 모드(WTT-Armory 등) | 181 | 118 | 63 (서로 다른 id 41개) |

그 41개는 SPT 기본 DB(4673개), 서버 DB(9483개), 설치된 모드의 파일 전체, DLL 295개의
문자열(ASCII·UTF-16), 프로필, 서버 캐시 어디에도 없었다. Olympus 가 만드는 것도 아니다 —
바닐라 총기용 80종은 Olympus 가 만들지만(이름 `ArSL`·`ArM4w` 등이 DLL 에 박혀 있다) 이 41개는
만들지 않는다.

WTT-Armory AXMC 의 탄창 칸이 전형이다. 패치 전에는 이랬다.

| id | 상태 |
| --- | --- |
| `68a387672d70af2b1e359602` | 있음 — `[WTT-Armory]_(AI AXMC .300 WM 10-round magazine)` |
| `436d497b0343516d521250c3` | 없음 — Olympus 가 끼워 넣은 것 |

총기는 이미 쓸 탄창이 있고 없는 것이 하나 더 붙은 꼴이라, **게임 기능이 막히지는 않았다.**
Olympus 가 자기 판 기준으로 적어 둔 id 가 설치된 WTT-Armory 2.0.5 와 어긋난 것으로 보인다.

## 조치

없는 탄창 41개를 `maglist` 에서 뺐다. 그 결과 탄창이 하나도 남지 않는 총기 43개는 할 일이
없으므로 항목째 뺐다.

| | 전 | 후 |
| --- | --- | --- |
| 총기 항목 | 209 | **166** |
| 탄창 참조 | 384 | **321** |
| 없는 참조 | 63 | **0** |

서버를 다시 띄워 확인했다 — `AddMagazinesToFirearms` 166건, 패치 후 DB 에서 그 41개를 참조하는
칸·탄창·격자 필터는 **0건**. AXMC 필터도 진짜 WTT 탄창 하나만 남았다.

뺀 id 는 [505-removed-mag-ids.txt](505-removed-mag-ids.txt) 에 있다.

## 고쳐지지 않은 것 — 서버 오류 48건은 그대로다

처음에는 이 41개가 서버 로그의 `Could not add <id> to cache, it does not exist in the item
database!` 48건의 원인이라고 봤다. **틀렸다.**

패치 뒤 서버를 다시 띄워도 **48건 그대로이고, 없는 id 목록도 한 글자도 다르지 않다.** 패치
뒤에는 DB 안에 그 id 를 참조하는 필터가 하나도 없는데도 그렇다. 즉 플레아 캐시는 그 id 를
**아이템 DB 가 아닌 다른 곳에서** 가져온다.

- 오류 주체는 `ItemBaseClassService`, 바로 앞 줄이 `플리마켓 상품 생성 중`
- 그 앞에서 UltimateFlea 가 8503개로 경제를 초기화하고 The Blacklist 가 1307개를 손본다
- 48개 id 는 디스크 어디에도 없다 — 실행 중에 만들어진다
- 과거 로그: 7-E(모드 95개) 4건 → 8-B(105개) 48건 → 이후 줄곧 48건. 7-E 의 4건은 id 가 달라
  다른 문제다

남은 후보는 UltimateFlea 와 The Blacklist 다. 하나씩 빼고 서버를 띄워 보는 수밖에 없다.

## 되돌리기

패치 직전 파일이 [505-firearmMags-before-mag-removal.json](505-firearmMags-before-mag-removal.json)
에 있다(`backup/firearmMags.json.before-mag-removal` 에도 같은 것). 설치 직후 원본은
[505-firearmMags-original.json](505-firearmMags-original.json) 이다. 되돌릴 파일을
`E:\Spt\SPT_Runtime\user\mods\MountOlympus\Modifiers\firearmMags.json` 로 덮어쓰면 된다.

## 주의

**Olympus 를 다시 설치하거나 갱신하면 이 수정이 사라진다.** WTT-Armory 를 갱신해 탄창 id 가
맞게 되면 오히려 원본으로 되돌리는 편이 낫다 — 그때는 41개가 실재할 것이다.

제작자에게 보고할 거리이기도 하다. 없는 아이템 id 를 칸 필터에 그대로 넣기보다, 넣기 전에
DB 에 있는지 한 번 보는 편이 낫다.

## 추가 (2026-10-28) — 서버 오류 48건의 원인은 DLL 이었다

위에서 "원인을 못 찾았다"고 적은 것을 가렸다. **`MountOlympus.dll` 이 48개 id 를 전부 들고
있다.** 설치된 DLL 301개 중 그 하나뿐이다.

```
48 / 48   SPT_Runtime/user/mods/MountOlympus/MountOlympus.dll
```

즉 이 모드는 같은 목록을 **데이터 파일과 DLL 양쪽에** 들고 있다. `firearmMags.json` 에서 뺀
것은 칸 필터 쪽만 고쳤고, 플레아 캐시를 건드리는 경로는 DLL 안에 있어 **사용자가 고칠 수 없다.**

가려낸 과정은 [../REVIEW-LOAD-ORDER.md](../REVIEW-LOAD-ORDER.md) 에 적었다. 플레아 관련 모드를
하나씩 빼 봤지만(The Blacklist, All Weapons Flea, Item Limits Begone, HideLockedFlea,
UltimateFlea) 48건은 꿈쩍하지 않았고, 상인 판매 목록 38965개·핸드북 8991개·로케일 56854개에도
없었다.

**Olympus 를 빼서 확인하지는 않았다.** 프로필에 Olympus 아이템이 24종(탄창 19 + 장비 5) 들어
있어, 템플릿 없는 아이템을 프로필이 들고 있는 상태가 된다 — 이 설치본을 한 번 망가뜨린 것과
같은 유형이라 하지 않았다.

### 방법 메모

처음 DLL 스캔은 "인쇄 가능한 문자 24자 이상 구간을 뽑아 비교"하는 방식이었고 **48개를 모두
놓쳤다.** `Buffer.includes` 로 id 를 **UTF-16LE 바이트열 그대로** 찾자 전부 나왔다. .NET 문자열을
찾을 때는 추출해서 비교하지 말고 찾을 것을 바이트로 만들어 찾는다. 있는 아이템 id 로 대조군을
두면 방법 자체가 맞는지 바로 확인된다.
