# OP 모드 목록 검증 (SPT 4.1.6)

외부에서 받은 "SPT 4.1.6 OP 모드 목록"(상인·탄약·방어구·총기 파츠 58개, 2026-10-26 작성)을 Forge API로 다시 조회해 설치 후보로 정리한 것.

- 조회: `https://sp-mod.com/api/v0/mod/<id>` + `/versions?filter[spt_version]=4.1.6&include=dependencies` (2026-10-26)
- 비교 대상: `doc/INSTALLED-MODS.md` (설치본 165개)
- **58개 모두 Forge에 존재하고 4.1.6 호환 버전이 있다.** 404·호환 없음 0건.

## 요약

| 구분 | 개수 |
| --- | --- |
| 이미 설치됨 | 38 |
| 미설치 후보 | 20 |
| 그중 바로 받을 수 있음(의존성 충족) | 14 |
| 그중 의존성·버전 제약 확인 필요 | 3 |
| 그중 설치본과 기능이 겹쳐 권하지 않음 | 3 |

원본 목록의 버전 표기 22건이 현재 Forge 최신과 다르다(그 사이 올라온 업데이트). 설치본은 이미 전부 Forge 최신과 같은 버전이라 **설치된 38개는 업데이트할 것이 없다.**

## 1. 이미 설치된 38개

원본 목록 중 설치본에 이미 있는 것. 괄호는 원본 목록의 표기 버전이 현재와 다른 경우.

| id | 모드 | 설치·현재 4.1.6 최신 | 원본 표기 |
| --- | --- | --- | --- |
| 374 | bluehead's AIO Trader | 5.3.1 | 5.3.0 |
| 2175 | Limitless | 1.0.0 | 같음 |
| 2966 | PresetTrader | 1.2.0 | 1.0.0 |
| 1554 | ALL AMMO TRADER | 1.5.0 | 같음 |
| 2419 | Amonya | 2.2.0 | 같음 |
| 2648 | AllGoodsTrader | 2.0.0 | 같음 |
| 3124 | Gunsmith Task Trader | 1.0.2 | 1.0.1 |
| 3128 | Gunwright | 1.0.2 | 1.0.0 |
| 1232 | Lotus | 1.8.1 | 1.8.0 |
| 3030 | NATO Quartermaster | 1.1.3 | 같음 |
| 1303 | HarryHideout | 3.1.2 | 3.1.0 |
| 3200 | All Weapons Flea | 1.0.1 | 같음 |
| 3005 | Trader Level Requirements Editor | 1.1.0 | 같음 |
| 1617 | give-ui | 5.0.1 | 5.0.0 |
| 3108 | Infinite Everything | 2.5.3 | 2.4.0 |
| 2777 | AmmoGen | 2.4.0 | 같음 |
| 2458 | Bela.s.Speedloader | 1.2.4 | 같음 |
| 2840 | Cobra's Ammo Stack | 1.1.0 | 같음 |
| 3074 | Direct Reload & Painless Meds | 1.3.2 | 1.1.0 |
| 2402 | AmmoBox1x1 | 2.0.0 | 같음 |
| 1569 | Wolfik's Heavy Trooper Masks | 1.2.0 | 같음 |
| 1878 | Unrestricted Armor Plate | 2.1.0 | 같음 |
| 922 | Less Restricting Headwear | 4.0.0 | 같음 |
| 3105 | SOMF - SitOnMyFace | 0.1.2 | 0.1.1 |
| 2380 | Black Exfil Revision | 1.1.0 | 같음 |
| 2897 | Equipment Is Eternal | 1.2.0 | 1.1.0 |
| 2556 | Repair Skill Extension | 1.4.1 | 1.4.0 |
| 2973 | Restore Armor Plates | 1.0.2 | 1.0.0 |
| 1939 | BETTER ATTACHMENTS | 4.1.5 | 같음 |
| 3171 | Realistic Suppressors | 1.0.0 | 같음 |
| 2010 | Definitive Weapon Variants | 4.3.2 | 같음 |
| 3019 | Cobra's Durability Tweaks | 1.0.0 | 같음 |
| 1305 | Repair Max Durability | 2.2.0 | 같음 |
| 3101 | Magic Optic Mount | 0.1.0 | 0.0.1 |
| 1972 | Full Automatic Guns | 4.1.5 | 같음 |
| 2246 | WTT- Armory | 3.0.0 | 같음 |
| 2373 | Slimmer Magazines | 2.0.0 | 같음 |
| 236 | Server Value Modifier (SVM) | 2.2.3 | 2.2.0 |

## 2. 미설치 후보 20개

`결속` = Forge 프로필 결속 표시(플레이 후 제거 시 프로필 손상 가능). `치트` = Forge 치트 경고. 크기는 4.1.6 호환 최신 버전의 다운로드 크기.

### 2-A. 바로 설치 가능 (의존성 충족·충돌 없음) — 14개

| id | 모드 | 버전 | 크기 | 결속 | Fika | 비고 |
| --- | --- | --- | --- | --- | --- | --- |
| [755](https://sp-mod.com/mod/755) | The Blacklist | 3.1.1 | 80 KB | - | 호환 | 플리 블랙리스트 해제 + 탄약 가격 재조정. SVM의 플리 설정과 일부 겹침 |
| [1937](https://sp-mod.com/mod/1937) | Dev Balaclava Fix | 1.2.0 | 4 KB | - | 호환 | 개발자 발라클라바 = 완전 무적. 게임 쪽 플러그인 1개 |
| [2953](https://sp-mod.com/mod/2953) | Headshot Damage Redirection | 1.8.0 | 8 KB | - | 호환 | 1.9.0이 있으나 4.1.5 전용 표기 → 4.1.6은 1.8.0이 맞음 |
| [1796](https://sp-mod.com/mod/1796) | Fujiwara Mokou (Touhou) | 1.2.0 | 845 KB | - | 호환 | 플레이어 캐릭터 추가. CommonLib(설치됨) 필요 |
| [505](https://sp-mod.com/mod/505) | Olympus | 4.1.3 | 53 KB | **결속** | 비호환 | OP 리그·방어구·탄창을 상인이 판매 |
| [2943](https://sp-mod.com/mod/2943) | Death Shadow Tweaks | 1.0.0 | 4 KB | **결속** | 호환 | 데스 섀도 마스크 충돌 제거. SOMF(설치됨)와 대상이 겹칠 수 있음 |
| [2190](https://sp-mod.com/mod/2190) | Recoil Rework (Legacy) | 2.0.0 | 35 KB | - | 불명 | 반동 전반 조정. BETTER ATTACHMENTS(설치됨)와 효과가 누적됨 |
| [2763](https://sp-mod.com/mod/2763) | MeleeMultiplier | 0.0.4 | 20 KB | - | 호환 | 근접 데미지 기본 x10 |
| [1453](https://sp-mod.com/mod/1453) | Make Shotguns Great Again! | 1.19.1 | **381 MB** | **결속** | 불명 | CommonLib(설치됨) 필요. 원본 표기 1.15.0 → 실제 1.19.1 |
| [3186](https://sp-mod.com/mod/3186) | Shorter Magazines | 1.1.0 | 7 KB | **결속** | 불명 | 3x1→2x1. **Slimmer Magazines(설치됨)와 같은 영역** — 아래 2-C 참고 |
| [2906](https://sp-mod.com/mod/2906) | SIM - Simple Item Modifier | 0.1.0 | 1.4 MB | **결속** | 불명 | 아이템 DB 일괄 편집 UI. 설치만으로는 아무것도 안 바뀜(Delta 규칙 적용 시에만) |
| [3065](https://sp-mod.com/mod/3065) | MagicBullet | 1.0.0 | 4 KB | - | 불명 | **치트** 표시. 데미지 x1~100 / 원샷킬 |
| [3169](https://sp-mod.com/mod/3169) | Tushonka-800 | 1.0.1 | 43 KB | - | 호환 | **치트** 표시. 실드·스캔·오버드라이브 |
| [2394](https://sp-mod.com/mod/2394) | Instant Insurance | 1.1.2 | 38 KB | - | 호환 | 아래 주의 사항 있음 |

**2394 Instant Insurance 주의** — Forge에 세 가지가 적혀 있다: ① SPT의 `lostOnDeath.wipeOnRaidStart`와 충돌(`false`로 두고 NoCheese 권장), ② 최신 변경 로그에 "FIR Means FIR 모드와 비호환" 명시, ③ 이미 설치된 **Equipment Is Eternal(2897)** 이 사망 시 장비를 통째로 복원하므로 기능이 대부분 겹친다. 둘 다 넣을 실익이 적다.

### 2-B. 의존성·버전 제약을 먼저 해결해야 함 — 3개

| id | 모드 | 버전 | 크기 | 필요한 미설치 의존성 |
| --- | --- | --- | --- | --- |
| [3088](https://sp-mod.com/mod/3088) | Tarkova-cola | 2.2.1 | 42 MB | [2788](https://sp-mod.com/mod/2788) VisitAPI Framework 1.3.6 (13 MB, 결속) |
| [3059](https://sp-mod.com/mod/3059) | Strange Curios | 1.1.0 | 26 MB | 없음 — CommonLib(설치됨)만 필요. **4.1.6이 아니라 `~4.1.5` 제약**이라 4.1.6에서 동작 확인 필요 |
| [3216](https://sp-mod.com/mod/3216) | Make Magnifier Great Again | 0.7.0 | 10 MB | [1263](https://sp-mod.com/mod/1263) Epic's AIO 5.0.2 (**1.5 GB**) + [2737](https://sp-mod.com/mod/2737) JSOC 3.0.1 (**392 MB**, 결속) |

- **3088**은 2.2.1 변경 로그에 "장비 복원 모드(Fair Equipment Restoration 등)와의 호환 수정"이 있다. 설치본의 Equipment Is Eternal이 바로 그 부류이므로 2.2.1 이상으로만 넣어야 한다.
- **3216**은 혼자 10 MB지만 의존성까지 합치면 약 1.9 GB가 따라온다. 아래 2-C의 Epic's AIO 문제를 먼저 결정해야 한다.

### 2-C. 설치본과 겹쳐 권하지 않음 — 3개

| id | 모드 | 겹치는 설치본 | 이유 |
| --- | --- | --- | --- |
| [2371](https://sp-mod.com/mod/2371) | FIR Fence Purchases | SVM(236) | 펜스 구매품 FIR 처리는 SVM이 이미 제공. `doc/PROCEDURE.md`의 "SVM으로 되는 기능은 별도 모드를 넣지 않는다" 기준에 해당 |
| [1946](https://sp-mod.com/mod/1946) | Arse Scratching Service | Cobra's Durability Tweaks(3019) | 3019가 무기 내구도 소모를 0으로 만든다. 레이드 중 회복은 의미가 없어짐 |
| [1263](https://sp-mod.com/mod/1263) | Epic's All in One | WTT-Armory(2246), Definitive Weapon Variants(2010) | 총기·부품 대량 추가 모드 3개가 겹침. 1.5 GB. 원본 목록도 "하나씩 넣고 확인" 경고 |

**3186 Shorter Magazines vs 2373 Slimmer Magazines(설치됨)** — 바꾸는 크기가 다르다(3186은 3x1→2x1, 2373은 2x2·1x3→1x2). 겹치는 아이템이 없다면 공존 가능하지만, 둘 다 탄창 크기를 서버에서 덮어쓰므로 **같이 넣을 경우 설치 후 탄창 크기를 직접 확인해야 한다.** 3186 저자 본인이 모드 페이지에서 2373을 "대안"으로 적어 둔 만큼, 둘 중 하나만 쓰는 편이 안전하다.

## 3. 아직 확인하지 않은 것

- **파일 단위 충돌 검사는 안 했다.** `tools/check-conflicts.mjs`는 압축 해제본이 있어야 돌아간다. 실제로 받기 전까지는 `E:\Spt`의 기존 파일과 경로가 겹치는지 알 수 없다.
- 위 "겹침" 판단은 Forge 설명·변경 로그와 `doc/INSTALLED-MODS.md` 비교에 따른 것이지, 게임에서 돌려 본 결과가 아니다.
- 설치는 `doc/PROCEDURE.md`를 따른다. 결속 표시가 있는 후보(505·2943·1453·3186·2906·3088·3059·3216·1263·2737·2788)는 프로필 백업 후 하나씩.
