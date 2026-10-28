# Olympus — 만들지 않는 아이템 id 48개를 참조해 플레아 캐시 오류가 48건 난다

2026-10-28. 서버 로그에 매번 찍히는
`Could not add <id> to cache, it does not exist in the item database!` **48건**의 원인이
`MountOlympus.dll` 임을 확정한 기록과, 제작자에게 보낼 영문 정리다.

가려낸 과정은 [../REVIEW-LOAD-ORDER.md](../REVIEW-LOAD-ORDER.md) 의 "48건의 정체", 데이터
파일 쪽 조치는 [../mod-patches/505-olympus-firearmMags-mags.md](../mod-patches/505-olympus-firearmMags-mags.md)
에 있다. 여기서는 **제작자가 필요로 할 것만** 모았다.

## 한 줄

Olympus 4.1.3 은 **자기가 만들지 않는 아이템 id 48개**를 들고 있고, SPT 가 플레아 상품을
만들 때 그 id 들을 아이템 DB 에서 찾지 못해 48번 오류를 남긴다.

## 증거

| 무엇 | 결과 |
| --- | --- |
| 48개 id 를 들고 있는 DLL | **`MountOlympus.dll` 하나** — 설치된 DLL 301개를 바이트로 훑었다 |
| 그중 `Modifiers/firearmMags.json` 에도 있는 것 | 41개 (모드 총기의 `maglist` 항목) |
| DLL 에만 있는 것 | 7개 |
| 그 48개가 실제로 존재하나 | **아니다.** SPT 기본 DB(4673), 최종 서버 DB(9483), 설치된 모드 파일 전체, 다른 DLL 의 문자열, 프로필, 서버 캐시 어디에도 없다 |

`firearmMags.json` 에서 41개를 **지워 봤지만 오류는 48건 그대로였고 id 도 같았다.** 즉 플레아
캐시 쪽 경로는 그 파일이 아니라 **DLL 안의 목록**을 쓴다.

Olympus 가 탄창을 아예 안 만드는 것은 아니다 — 바닐라 총기용 80종은 만든다(`ArSL`·`ArM4w`
같은 이름이 DLL 에 있다). **이 48개만 만들지 않는다.**

## 배제한 것

| 후보 | 어떻게 |
| --- | --- |
| The Blacklist / All Weapons Flea / Item Limits Begone / HideLockedFlea / UltimateFlea | 하나씩 빼고 서버를 띄웠다. **48건 그대로** |
| 상인 판매 목록 | 32명 38,965개 조회 — 없는 아이템 참조 0 |
| 핸드북 / 로케일 | 8,991 / 56,854 항목 — 없음 |
| 패치 후 DB 의 칸·탄창·격자 필터 | 그 48개를 참조하는 것 0 |
| 프로필 · `sptappdata` | 없음 |

발생 시점도 맞는다 — 7-E(모드 95개) 4건(다른 id) → **8-B(105개, Olympus 설치 이후) 48건**
→ 이후 줄곧 48건.

## 영향

**로그 오류일 뿐이고 게임이 막히지는 않는다.** 없는 탄창이 플레아에 안 뜨는 것은 정상이고,
총기는 자기 탄창을 쓴다. 다만 서버 시작마다 48줄이 쌓여 다른 문제를 가린다.

## 확인하지 않은 것

**Olympus 를 빼고 돌려 보지 않았다.** 프로필에 Olympus 아이템이 24종(탄창 19 + 장비 5) 들어
있어, 템플릿 없는 아이템을 프로필이 쥔 상태가 된다 — 이 설치본을 한 번 망가뜨린 유형이라
하지 않았다. 소유 증거(유일한 DLL)와 다른 후보 전원 배제로 충분하다고 봤다.

# 제작자 보고용 (영문)

```
Title: 48 item ids referenced but never created -> "Could not add <id> to cache" x48 on every server start

Versions
- SPT 4.1.6 (EFT 0.16.9.5.40743)
- Olympus 4.1.3 (Forge mod 505)
- 114 server mods installed; WTT-Armory 2.0.5 present

What happens
Every server start logs exactly 48 lines like this, during flea market
generation (the line right before them is the "generating ragfair offers"
message), from ItemBaseClassService:

  Could not add 436d497b0343516d521250c3 to cache, it does not exist in the item database!
  Could not add 2263e611456a811f6543286b to cache, it does not exist in the item database!
  ... (48 distinct ids)

Why I think it is Olympus
1. All 48 ids are present as UTF-16 string literals inside MountOlympus.dll.
   I byte-scanned all 301 DLLs in the install (user/mods + BepInEx/plugins);
   MountOlympus.dll is the ONLY one that contains them (48/48). No other DLL
   contains more than 0.
2. None of the 48 ids exist as items anywhere: not in SPT's base item DB
   (4673 templates), not in the final server DB served to the client
   (9483 templates), not in any installed mod's files, not in the profile,
   not in the server cache.
3. 41 of the 48 also appear in Modifiers/firearmMags.json, as maglist entries
   for mod-added guns (WTT-Armory and similar). The remaining 7 appear only
   in the DLL.
4. I removed those 41 ids from firearmMags.json and restarted: the error count
   stayed at exactly 48 with the same ids. So the flea-cache path does not read
   that file - it uses the list compiled into the DLL.
5. Timeline matches: before Olympus was installed this install logged 4 such
   errors with different ids; the first start after installing Olympus logged
   48, and it has been 48 ever since.

Note: Olympus does create magazines - about 80 for vanilla guns (names like
ArSL / ArM4w are in the DLL). It is specifically these 48 that are referenced
but never registered.

Ruled out
- Removed The Blacklist, All Weapons Flea, Item Limits Begone, HideLockedFlea
  and UltimateFlea one at a time and restarted: still exactly 48.
- Trader assorts (32 traders, 38,965 entries): zero references to missing items.
- Handbook (8,991 entries), locale en (56,854 entries): none of the 48.
- Post-patch item DB slot / cartridge / grid filters: zero references to the 48.

Impact
Log noise only as far as I can tell. The guns involved have their own working
magazines, and an item that does not exist simply never appears on the flea.
But 48 lines on every start make real errors easy to miss.

Request
Before registering an id (or adding it to the flea/base-class cache), please
check that it exists in the item database and skip it if not - or create the
magazines these ids refer to. Either would silence all 48.

The 48 ids, if useful:
00d9f8906ba2da8037d48b10  00da199defbc2fbcb10f9ec5  0d084425cb03c37566b8c1f4
11e1eff49d397189414ab868  16ac01c1c47ee92b45434aa9  20d5db641f028f1c3d2d718e
2263e611456a811f6543286b  2dfd06092497541d0ebeb653  3c092b4c4d6c6f55690a8498
3d615617813d2601ba945692  3d9ac26ca6383bb3341866ac  436d497b0343516d521250c3
45be94029e87ff2634e7de93  45c81346d9461ee8f9981008  46bfca1fcdec5872ffeb284a
4c9fcdee95bffe9ff8a07a44  4cc2529d8711f4aa22315ffa  516ec1636d395b9a5269770f
56543418653ccdec6334d1ed  5e8798197d76ce668b83e84d  6f88b582e3e9b44dfde186cf
7148eb9b6953df70b0bda525  73050ee0e3b3b7dcf0c3f01b  7535716f71efa3a1f680865c
75c571c4b1155f3a4d1ffd9d  7fe6b9b26d25a30899bcb7fe  88da98b9b2b64c2c2f790b72
8c4d94ec908ddd3fa01ca6f6  92817fb92a313fa21ec59057  9589359907fca6a0a07cfbfd
964d1222985c11c2c84e7f3f  9d78b09b51dac5d49af2a81c  a01c04b3231dc39cdbab22ae
a085d83bce3a727583a9f414  a8dec5909e063bfa71d40ba9  aa6fa1cf01e42a764c053809
aad8c4444a1e860e8f4a0ddd  b0c383075ea77729b051c3a7  b2333cdd7256a0ecaf179b4f
b2830544fa5df93204d612a5  b7766cf6a574c79b608986d0  c7b912cac5d666dd0271d2cd
c7b912cac5d666dd0271d2ce  ce89fa83336f1c07c1e494a1  d85093c45145fce1aba14c45
de55fbc58a6dd78332ae893f  e4e53415475b032d7dde6ee9  f3aeb467cb54f1773dc408da

The 7 that are NOT in firearmMags.json (DLL only):
0d084425cb03c37566b8c1f4  4cc2529d8711f4aa22315ffa  5e8798197d76ce668b83e84d
75c571c4b1155f3a4d1ffd9d  8c4d94ec908ddd3fa01ca6f6  ce89fa83336f1c07c1e494a1
e4e53415475b032d7dde6ee9

Not tested: I did not try removing Olympus to confirm, because my profile holds
24 Olympus items (19 magazines + 5 pieces of gear) and leaving a profile with
items whose templates are gone has broken this install before.
```

## 어디에 올리나

- Forge 모드 페이지: https://sp-mod.com/mod/505
- 올릴 때 **보고서 안의 사실만** 쓴다. 내 설치본에서 `firearmMags.json` 을 손본 것
  ([../mod-patches/505-olympus-firearmMags-mags.md](../mod-patches/505-olympus-firearmMags-mags.md))은
  제작자의 문제가 아니므로 섞지 않는다.

## 쓴 방법 (다시 재려면)

DLL 에서 아이템 id 를 찾을 때는 **문자열을 추출해 비교하지 말고 찾을 것을 바이트로 만들어
찾는다.** .NET 문자열은 UTF-16LE 로 들어 있다.

```js
const q = Buffer.from(id, 'utf16le');
if (fs.readFileSync(dllPath).includes(q)) { /* 들고 있다 */ }
```

처음에 "인쇄 가능한 문자 24자 이상 구간을 뽑아 비교"하는 방식을 썼고 **48개를 모두 놓쳐**
"어느 DLL 에도 없다"고 잘못 보고했다. **실재하는 아이템 id 로 대조군을 두면** 방법이 틀린 것이
바로 드러난다.
