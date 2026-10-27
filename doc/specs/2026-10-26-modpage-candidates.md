# 서버 웹 모드 페이지 — 추가 후보 검토

- 작성 2026-10-26 · 대상 설치본 `E:\Spt` (SPT 4.1.6), 서버 모드 97개
- 판정 근거: 설치된 모든 모드 DLL을 Mono.Cecil로 훑어 `SPTarkov.Server.Web.IModBlazorMetadata`(모드 페이지)와 `SPTarkov.Server.Web.Services.IConfigEditorConfigProvider`(설정 편집기) 구현을 전수 확인. 설정 항목과 등급은 `2026-10-26-f12-menu-review.md`에서 가져옴.

## 이미 웹 표면이 있는 모드 (11개 — 후보 아님)

**모드 페이지** (첫 화면 "모드 페이지" 구역, `IModBlazorMetadata`)

| 모드 | 경로 |
| --- | --- |
| Barter Items Stacks | `/bis` |
| JeroBackpack | `/jerobackpack` |
| LennoxP90-COTI | `/coti` |
| **WebConfigBridge** (우리 것) | `/webconfigbridge` |

**설정 편집기 등록** (`/configs`의 "모드" 탭, `IConfigEditorConfigProvider`)

`acidphantasm-harryhideout`, `acidphantasm-reffriendlyquests`, `acidphantasm-scorpion`, `ChangeHelmetVisor`, `LennoxP90-COTI`, `Limitless`, `ozen.Foldables`, `TraderNamer`

## 제외한 것

- **설정이 없는 모드 23개** — 콘텐츠·번들·상인 데이터만 있다. `WTT-Armory`, `WTT-Artem`, `Lotus`, `Painter-4.0`, `Peacemaker`, `Vanguard-Mod`, `spt_22lr`, `bushtail-CantedAiming`, `BoxesAtRef`, `Airburst`, `Eukyre-BetterOKP7`, `7Bpencil.MagicOpticMount`, `LoadAmmoAnimServer`, `ItemGen`, `Gunwright`, `PresetTrader`, `TraderGen`, `Tyfon.WeaponCustomizer.Server`, `WTT-ServerCommonLib` 등.
- **자체 편집 수단이 이미 있는 모드 2개** — `give-ui`(외부 앱 `Give Ui.exe`), `Ultimate Trader Assort Editor`(게임 안 상인 화면에서 편집).

## 1순위 — 요청 시점에 값을 쓰는 모드

설정이 서버 시작 때만이 아니라 **요청·이벤트마다** 쓰이는 것들이다. 웹에서 고치고 서버만 다시 띄우면 되고, 모드에 따라서는 재시작 없이도 반영될 여지가 있다.

| 모드 | 바꿔볼 값 | 왜 가치가 있나 |
| --- | --- | --- |
| `mpstark-dynamicmaps` | 아군·적·보스·전리품 마커 표시 토글 4개 | 켜고 끄는 의미가 분명하고 다음 레이드에서 바로 체감된다 |
| `RealisticInsurance` | 반환 확률(PMC/스캐브/보스별 %), 약탈자 능력치 | 숫자 하나로 체감이 크게 달라진다. 레이드 종료마다 쓰인다 |
| `UltimateFlea` | 가격 원천, 경제 시뮬, 레벨별 제한 | 플리마켓 전반. 오퍼 생성·거래마다 쓰인다 |
| `RepairSkillExtension` | 버프 확률·내구도 보정 | 수리 이벤트마다 굴린다 |
| `Moe-TradeMarker` | 표시 on/off, 상인 제외 목록, 플리 제한 | 항목이 적어 폼이 간단하다 |
| `InfiniteEverything` | 프로필별 money·hideout 토글 | 치트성 토글이라 자주 켜고 끈다. 자체 상태 라우트가 이미 있다 |
| `EquipmentIsEternal` | 암밴드·특수칸·칼집·보안컨테이너 복원 토글 | 토글뿐이라 폼이 간단하다. 다만 읽는 시점은 미확인 |

## 2순위 — 서버 시작 때만 읽는 모드 중 값이 큰 것

재시작이 전제지만, **파일을 직접 고치지 않아도 되는 것만으로 이득이 큰** 것들이다.

| 모드 | 바꿔볼 값 |
| --- | --- |
| `HelmetSlotUnion` (우리 것) | 토글 4개. 우리가 만든 모드라 페이지를 붙이기 가장 쉽다 |
| `[SVM] Server Value Modifier` | 14개 분류(Items·Hideout·Traders·Loot·Player·Raids·Fleamarket…). 값이 가장 많지만 폼도 가장 크다 |
| `CobrasDurabilityTweaks` | 내구도 소모 배수, 고장 비활성 |
| `CobrasAmmoStack` | 구경별 스택 크기 |
| `RealisticSuppressors` | 내구도·인체공학·반동 배수 3개 |
| `TraderLevelRequirementsEditor` | 상인별 갱신 주기, 충성도 1~4 요구치 |
| `betterkeys` | 열쇠 정보 표시 설정 |
| `AllinWeaponUnslotted` | **완료** — `/webconfigbridge` |

나머지 어려움 등급(`AmmoGen`, `AmmoStats`, `Amonya`, `BetterAttachments`, `BlueheadsAIOTrader`, `CaliberSplitStorageSolutions`, `CaseTrader`, `ColoredTracers`, `DefinitiveWeaponVariants`, `EasyMounting`, `FiveF-UnrestrictedArmorPlate`, `FoldThatStock`, `FullAutoGuns`, `GunsmithTrader`, `HolsterEverything`, `LessRestrictingHeadwear`, `Michael-NATOQuartermaster`, `ODT-ItemInfo-4.0`, `PortableTraders`, `RZDirectReload`, `RefReworkServer`, `Speedloader`, `Suntion-AllGoodsTrader`, `TGC-NG`, `Tiltushkin-AllAmmo`, `WTT-PackNStrapServer`, `WolfiksHeavyTrooperMasks`, `egbog-RepairMaxDurabilityServer`)는 한 번 정하고 잘 안 바꾸는 값이거나 항목이 하나뿐이다. 필요해질 때 붙이면 된다.

## 권하는 방식 — 페이지를 모드마다 만들지 않는다

`WebConfigBridge`는 이미 "남의 설정을 비추는" 다리다. 모드 하나당 페이지 하나를 만들면 첫 화면이 카드로 뒤덮인다. 대신 **한 페이지 안에 탭이나 아코디언으로 여러 모드를 담는 쪽**이 낫다.

- 지금 구조에서 `ConfigStore`가 모드별 경로를 풀고 POCO로 읽는다. 모드를 더하려면 POCO 하나와 화면 묶음 하나를 더하면 된다.
- 설정 항목이 많은 모드(`[SVM]`)는 분류별 아코디언이 필요하다. 다른 모드와 성격이 다르니 그때 가서 따로 판단한다.
- 설정 편집기(`/configs`)에 등록하는 길도 그대로 쓸 수 있다. 원시 JSON을 그대로 다루는 화면이 필요하면 그쪽이 낫고, 스위치·슬라이더로 다듬은 화면이 필요하면 모드 페이지가 낫다.

## 다음에 손댈 것 하나를 고른다면

`HelmetSlotUnion`이다. 우리가 만든 모드라 설정 구조를 이미 알고 있고, 토글 4개뿐이라 `WebConfigBridge`에 묶음 하나를 더하는 수준이다. 여러 모드를 한 페이지에 담는 구조가 맞는지 가장 싸게 확인할 수 있다.
