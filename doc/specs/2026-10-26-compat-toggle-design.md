# 설계: 호환성 토글 플러그인 (CompatToggle)

- 작성 2026-10-26 · 대상 설치본 `E:\Spt` (SPT 4.1.6, EFT 0.16.9.5.40743)
- 대체 대상: 서버 모드 `AllinWeaponUnslotted` (Szonszczyk - All in Weapon: Unslotted)
- 선행 조사: `doc/specs/2026-10-26-f12-menu-review.md` (F12 메뉴 없는 설치 모드 99개 검토표)

## 1. 의도

무기·부품·탄약의 호환성 제한을 **게임을 끄지 않고** 켜고 끌 수 있게 한다. 은신처·장비·개조 화면에서 F12로 토글을 바꾸면 그 화면에서 바로 결과가 보여야 한다.

### 왜 지금

현재 그 역할은 서버 모드 `AllinWeaponUnslotted`가 한다. 이 모드는 **아이템 데이터베이스를 고쳐** 제한을 푼다. 거기서 두 가지 문제가 나왔다.

**첫째, 바꿔볼 수가 없다.** 토글 하나를 끄려면 `config.jsonc`를 열어 고치고 서버와 게임을 모두 다시 띄워야 한다. 어떤 조합이 맞는지 찾으려면 그 왕복을 몇 번이고 돌아야 한다. 이 모드는 서버 모드라 F12에 자리가 없다.

**둘째, 누가 무엇을 덮었는지 알 수 없다.** 이 모드는 모든 무기와 부품의 슬롯 필터·탄약 호환 필터를 다시 쓰고, 부품의 `ConflictingItems`를 비운다. 같은 필드를 손보는 다른 모드와 정면으로 겹치는데, 서버는 모드를 순서대로 로드하며 같은 필드를 그대로 다시 쓰기 때문에 **뒤에 로드된 쪽이 조용히 이긴다.** 로그에 흔적이 남지 않는다. 사용자가 겪은 "모드가 다른 모드 설정을 덮어썼는데 수정할 방법이 없다"가 이것이다.

같은 데이터를 건드리는 모드가 이 설치본에 최소 다섯 개 확인됐다:

| 모드 | 건드리는 것 |
| --- | --- |
| `AllinWeaponUnslotted` | 무기·부품 슬롯 `Filters`, `Chambers`, `Cartridges`, `ConflictingItems` |
| `HelmetSlotUnion` | 헬멧 슬롯 필터, 헬멧·헬멧 파츠의 `ConflictingItems` |
| `LessRestrictingHeadwear` | `BlocksEarpiece`/`BlocksFaceCover`/`BlocksEyewear`/`BlocksHeadwear`, `ConflictingItems` |
| `WolfiksHeavyTrooperMasks` | `ConflictingItems`, `conflictingFaceCoverings`, `Helmet_top`/`Helmet_back` |
| `FiveF-UnrestrictedArmorPlate` | `helmet_top`/`helmet_back`/`helmet_ears` 플레이트 슬롯 필터 |

### 왜 "서버 모드에 F12 메뉴를 붙이는" 방식이 아닌가

`AllinWeaponUnslotted`를 직접 확인했다. `/client/items`에 `PlayerItemsRouter`·`AlterSlotFilterStaticRouter`를 걸어 **클라이언트가 아이템 목록을 요청할 때** 필터를 가공하고, 내부 연산은 `RelaxWeaponFilters`·`RelaxAttachmentFilters`·`RelaxMagazineFilters`·`ApplyChanges`다. 설정은 `OnLoadAsync`에서 `ConfigLoader`로 **한 번** 읽어 필드에 보관한다.

`/client/items`는 게임이 켜질 때 한 번만 호출된다. 따라서 이 모드에 설정 재읽기를 붙여 얻을 수 있는 최선은 "서버는 켠 채로, 게임만 재시작하면 적용"이다. 세션 유지 중 즉시 반영은 이 경로로 불가능하다.

선행 조사에서 이 벽은 이 모드만의 문제가 아니었다. 설정 파일을 가진 서버 모드 74개 중 **42개**가 설정을 서버 시작 때만 읽는다. 모드들의 설정 주석이 스스로 그렇게 적어둔다 — `RealisticSuppressors` "Restart the server after editing", `WolfiksHeavyTrooperMasks` "값은 SPT 서버 재시작 시 적용", `LennoxP90-COTI`는 항목별로 "needs a server restart" / "relaunch the game client"를 구분해 적는다.

### 서버는 슬롯 적법성을 검사하지 않는다 (확인됨)

게임 쪽 단독 구현이 성립하려면, 클라이언트가 "허용"한 이동을 서버가 되돌리지 않아야 한다. `E:\Spt\SPT_Runtime\SPTarkov.Server.Core.xml`(서버 어셈블리의 XML 문서 주석)에서 확인했다.

- `InventoryController.MoveItem` — 문서 요약이 "change location of item with parentId and slotId"다. 슬롯 필터 검사에 대한 언급이 없다.
- 슬롯 필터로 이동을 검증하는 멤버가 **없다.** `Filters`는 봇 생성(`BotEquipmentModGenerator`)과 설정 모델에만 나타난다. 서버의 검증 멤버는 게임 버전·닉네임·플리 오퍼·스택 수량뿐이다.
- 프로필 로드 시 도는 검사는 **템플릿 존재 여부** 기준이다: `ProfileValidatorHelper.RemoveInvalidItems`는 "items that do not exist inside the items DB", `RemoveInvalidUserBuilds`는 "builds containing items that no longer exist"를 지운다. 필터 위반을 보지 않는다.

따라서 서버는 슬롯 적법성을 클라이언트에 맡긴다. 그리고 **이미 조립해 둔 "불법" 조합은 프로필 로드 때 필터로 재검증되지 않는다** — §4 안티목표의 핵심 위험이 구조적으로 해소된다(§6-3에서 실제로 확인한다).

### 그래서 어떻게

DB를 고치는 대신 **판정 지점을 게임 쪽에서 가로챈다.** "이 부품이 이 슬롯에 들어갈 수 있는가"를 게임이 묻는 그 순간에 끼어들어, F12 토글이 그 자리에서 답한다. 서버는 DB를 건드리지 않고 원본대로 둔다.

이러면 두 문제가 함께 풀린다. 토글은 다음 판정부터 즉시 반영되고, DB를 아무도 덮어쓰지 않으므로 다른 모드가 깔아둔 슬롯·충돌 설정이 그대로 살아남는다.

## 2. 목표

1. F12 창에서 호환성 제한을 항목별로 켜고 끌 수 있다.
2. 토글 변경이 **세션 유지 중** 반영된다. 서버 재시작도 게임 재시작도 필요 없다. 다음 판정 때부터 적용되면 충분하다(화면 재진입 허용).
3. 모든 토글을 끄면 바닐라 판정으로 완전히 돌아간다.
4. `AllinWeaponUnslotted`를 서버에서 제거해도 지금 쓰는 자유도가 유지된다.
5. 같은 데이터를 건드리던 다른 모드들이 더는 서로 덮어쓰지 않는다.

### 서버 생성물은 바닐라로 돌아간다 (사용자 결정)

`AllinWeaponUnslotted`는 DB를 고치므로, 서버가 그 DB를 읽어 **만드는 것들**에도 영향을 준다. 봇 무기 생성(`BotEquipmentModGenerator.ShouldModBeSpawned`가 `Slot`을 인자로 받는다), 상인 assort·프리셋 생성, 그리고 이 설치본의 빌드 계열 모드(`FlexibleBuilds`, `DefinitiveWeaponVariants`, `Gunwright`, `ChooChoo-TraderModding`, `AllWeaponsFlea`)가 여기에 해당한다. 게임 쪽 판정 후킹은 이 영역에 닿지 못한다.

사용자는 **서버 모드를 완전히 제거하는 쪽**을 택했다. 봇·상인·프리셋 생성이 바닐라 규칙으로 돌아가는 것을 받아들인다. 따라서 이 플러그인은 그 영역을 보상하려 시도하지 않고, §6에도 그에 대한 확인 항목을 두지 않는다. 이 선택이 체감상 문제가 되면 그때 별도로 다룬다.

## 3. 비목표

- **레이드 안에서의 현장 개조.** 레이드 중 무기 개조는 `Tylevo.FieldAttachments`의 영역이고, 이 플러그인은 그 기능을 넣지 않는다. 다만 FieldAttachments가 레이드 중 개조 화면을 열어주는 동안 이 플러그인의 판정이 함께 적용되는 것은 자연스러운 결과이므로 막지 않는다.
- **`FckALL`과 `Experimental` 재현.** 원본의 이 두 경로는 "모든 카테고리를 전 슬롯에 밀어넣는" 실험 로직이고, 원본 설정에서도 꺼져 있다. 구현하지 않는다.
- **다른 서버 모드 흡수.** 위 표의 헬멧 모드 네 개를 이 플러그인으로 대체하는 것은 이번 범위가 아니다. 이번 작업으로 그 모드들의 설정이 덮어쓰이지 않게 되는 것까지가 목표다.
- **진단 리포트.** "어느 모드가 어느 필드를 마지막으로 썼다"를 보여주는 도구는 만들지 않는다(§7 참고).
- 서버 쪽 구성요소. 이 플러그인은 게임 쪽 단독으로 동작한다.

## 4. 안티목표

기술적으로 동작해도 다음에 해당하면 실패로 본다.

- **기존에 조립해 둔 무기·장비가 깨진다.** 지금 프로필에는 원본 모드가 허용해 준 "불법" 조합으로 조립된 물건이 있을 수 있다. 토글을 끄더라도 이미 조립된 것이 분해·삭제·장착 해제되어서는 안 된다. 판정 지점을 가로채는 방식은 *설치를 시도할 때* 평가되므로 구조적으로 안전하지만, 구현자는 이것을 **실제로 확인**해야 한다(§6의 확인 항목).
- **같은 판정 지점을 쓰는 기존 플러그인이 깨진다.** 이 설치본에서 `CanAccept`를 패치하는 플러그인이 8개, `CheckCompatibility`를 패치하는 것이 8개 확인됐다(§5.2). 이들과 공존해야 한다.
- 프로필이 손상된다.
- 토글을 끈 상태가 바닐라와 다르다.

## 5. 결정된 사항

### 5.1 접근

게임 쪽 BepInEx 플러그인 하나. 서버 구성요소 없음. 아이템 DB를 수정하지 않고, 호환성 판정 메서드를 Harmony로 후킹해 토글에 따라 결과를 바꾼다.

### 5.2 패치 지점

`E:\Spt\EscapeFromTarkov_Data\Managed\Assembly-CSharp.dll`에서 다음 멤버의 존재를 확인했다(문자열 추출, 2026-10-26 기준 어셈블리).

| 멤버 | 확인 | 쓰임 |
| --- | --- | --- |
| `EFT.InventoryLogic.Slot.CanAccept` | 있음(8회) | 슬롯이 아이템을 받을 수 있는지 |
| `Slot.CheckItemFilter` | 있음 | 슬롯 화이트리스트 검사 |
| `Slot.CheckItemExcludedFilter` | 있음 | 슬롯 블랙리스트 검사 |
| `CheckCompatibility` | 있음 | 호환성 검사 |
| `ConflictingItems` | 있음(4회) | 충돌 품목 목록 |
| `EFT.InventoryLogic.ItemFilter` | 있음 | 필터 자료형 |

이 지점들은 추정이 아니다. 같은 설치본의 다른 플러그인들이 이미 쓰고 있다.

- `Tylevo.FieldAttachments` — `CanAccept`, `CheckItemFilter`, `CheckItemExcludedFilter`, `EFT.InventoryLogic.Slot`, `EFT.InventoryLogic.ItemFilter`를 모두 참조한다. **이 플러그인이 접근의 실현 가능성 증거이자 가장 가까운 참고 구현이다.**
- `maschine-LinkedSearchInStash` — `CheckCompatibility`, `AcceptItem`, `get_Filters`, `Chambers`, `CompatibleTemplateIds`, `SPT.Reflection.Patching` 참조.
- `CanAccept` 참조: `AdvancedStashSorting`, `DrakiaXYZ-EquipFromWeaponRack`, `HolsterEverything`, `InfiniteEverything`, `RZDirectReload`, `Tyfon.UIFixes`, `Tylevo.FieldAttachments`, `WTT-PackNStrapClient`.
- `CheckCompatibility` 참조: `BarterItemsStacksClient`, `CompatibilityHighlighter`, `PreRaidChecklist`, `InfiniteEverything`, `maschine-LinkedSearchInStash`, `ozen.ContinuousLoadAmmo`, `RZDirectReload`, `WTT-PackNStrapClient`.

**패치 방식은 `SPT.Reflection.Patching.ModulePatch`를 쓴다**(내 결정). 이 설치본의 SPT 플러그인들이 쓰는 방식이고, 난독화된 멤버를 이름이 아니라 시그니처로 찾아내는 수단을 제공한다. 원시 Harmony 어트리뷰트는 쓰지 않는다.

**토글이 꺼져 있을 때는 패치가 원본 결과를 그대로 흘려보내야 한다**(내 결정). 패치를 런타임에 붙이고 떼는 방식은 쓰지 않는다 — 항상 붙여 두고 토글 값만 읽는다. 이것이 "다음 판정부터 즉시 반영"을 보장하는 방법이고, 다른 플러그인과의 패치 순서 문제도 피한다.

### 5.3 토글

원본 `config.jsonc`의 축을 그대로 가져온다. 괄호 안은 원본 키와 내부 연산이다.

| 토글 | 하는 일 |
| --- | --- |
| `Weapon slots` | 무기 본체의 부착 슬롯에 어떤 부품이든 허용 (`FckWeapons` / `RelaxWeaponFilters`) |
| `Attachment slots` | 부품의 하위 슬롯에 어떤 부품이든 허용 (`FckMods` / `RelaxAttachmentFilters`) |
| `Chamber ammo` | 약실에 어떤 탄약이든 허용 (`FckChambers` / `Chambers`) |
| `Magazine calibers` | 무기에 어떤 탄창이든 허용 (`FckCalibers` / `RelaxMagazineFilters`) |
| `Magazine ammo` | 탄창에 어떤 탄약이든 허용 (`FckMagazines` / `Cartridges`) |
| `Ignore conflicts` | 무기·부품의 `ConflictingItems` 충돌을 무시 (`RemoveConflictingItems`) |

원본 설명이 적어둔 조합 주의사항(약실만 풀고 탄창은 안 푸는 식의 반쪽 조합)은 F12 설명 문구로 옮긴다.

**`Ignore conflicts`는 무기·부품에만 적용한다**(내 결정). 원본 키의 설명이 "Removes Conflicting Items filter **from mods**"이고, 헬멧·안면 장비의 `ConflictingItems`는 §1 표의 세 모드(`HelmetSlotUnion`, `LessRestrictingHeadwear`, `WolfiksHeavyTrooperMasks`)가 **의도를 갖고 직접 써넣은** 값이다. 전역으로 무시하면 그 모드들이 남겨둔 충돌을 이 플러그인이 가려버려 목표 5와 §6-6이 무의미해진다. 판정 지점에서 대상 아이템이 무기·부품인지 가리는 검사가 필요하다.

**약실·탄창 토글은 탄약 장전 동작까지 포함한다**(내 결정). 원본은 DB를 고쳐 제한을 풀었으므로 인벤토리에서 끌어다 놓는 경우뿐 아니라 탄창에 탄을 채우거나 재장전로 약실에 올리는 경로에도 적용됐다. 목표 4(지금 쓰는 자유도 유지)를 지키려면 그 경로들도 토글을 따라야 한다. 이 설치본에서 그 경로를 건드리는 플러그인이 `ozen.ContinuousLoadAmmo`, `RZDirectReload`, `maschine-UnloadAllMagazines`다.

그 결과 **탄약 쪽 판정은 레이드 중에도 동작한다** — 재장전이 레이드에서 일어나기 때문이다. §3의 비목표는 "레이드 중 무기 개조 기능을 넣지 않는다"는 뜻이고, 탄약 호환 판정이 레이드에서 토글을 따르는 것은 의도된 동작이다.

**기본값은 현재 `config.jsonc`의 값을 그대로 따른다**(내 결정): 위 여섯 개 모두 켜짐. 서버 모드를 빼고 이 플러그인을 넣는 순간 체감이 달라지지 않게 하는 것이 우선이다. "전부 끄면 바닐라"는 사용자가 끄는 선택이고, 기본 상태가 아니다.

추가 항목 둘(내 결정):

- `Master switch` — 전체를 한 번에 끈다. 바닐라와 비교할 때 토글 여섯 개를 차례로 건드리지 않아도 되게 한다.
- `Log decisions` — 기본 꺼짐. 켜면 이 플러그인이 "원래 거부될 판정을 허용으로 바꾼" 건을 로그에 남긴다. 어떤 조합이 실제로 제한에 걸리고 있었는지 사람이 볼 수 있게 하는 수단이다.

### 5.4 프로젝트 구성

이 저장소의 기존 클라이언트 플러그인 `src/SptKoreanConfig`를 틀로 쓴다.

- 위치 `src/CompatToggle/`
- `net472`, `Microsoft.NETFramework.ReferenceAssemblies`
- 참조는 `$(SptDir)`(기본 `E:\Spt`) 기준 `HintPath`, 전부 `<Private>false</Private>`: `BepInEx.dll`, `0Harmony.dll`, `Assembly-CSharp.dll`, `spt-reflection.dll`, 필요한 `UnityEngine.*`
- 플러그인 GUID `spt.compattoggle`, 표시명 `Compat Toggle`(내 결정 — 기존 `spt.koreanconfig` 작명을 따랐다. `CompatibilityHighlighter`와 혼동되지 않게 "Compat Toggle"로 둔다)
- F12 문구는 영어 키 + 영어 설명으로 두고, 한국어는 `src/SptKoreanConfig/lang/spt.compattoggle.json`에 따로 넣는다(내 결정 — 이 저장소의 한글화가 그 방식이고, `tools/check-lang-coverage.mjs`가 커버리지를 점검한다)

### 5.5 전환 절차

`doc/PROCEDURE.md`를 따른다. 순서는:

1. 프로필 백업 (`E:\Spt\SPT_Runtime\user\profiles\*` → `backup/profiles-before-compattoggle/`)
2. `AllinWeaponUnslotted/config/config.jsonc` 백업 — 전환 후 기본값 비교에 쓴다
3. **`AllinWeaponUnslotted`가 아직 설치된 상태에서** 시험용 "불법" 조합 무기를 하나 조립해 프로필에 남긴다. 바닐라라면 들어가지 않는 부품 조합과 구경이 다른 탄창을 섞는다. §6-3은 이 물건이 있어야 확인할 수 있으므로, 이 단계가 제거보다 **먼저**다.
4. 플러그인 설치 후 게임 실행. `AllinWeaponUnslotted`는 그대로 둔 채, **플러그인 토글을 끈 상태에서 제한이 되살아나는지** 본다(둘 다 켜져 있으면 DB가 이미 풀려 있어 플러그인의 "허용" 쪽은 검증되지 않는다. 이 단계의 목적은 플러그인의 "거부" 쪽 동작 확인이다)
5. `AllinWeaponUnslotted` 제거, 서버 재시작 — 이제 플러그인의 "허용" 쪽이 검증 대상이 된다
6. 토글 여섯 개를 하나씩 끄고 켜며 §6을 확인
7. `node tools/gen-installed-mods.mjs`로 `doc/INSTALLED-MODS.md` 재생성

## 6. 확인 항목

구현자가 **게임에서 직접** 확인한다. 테스트 코드로 대체할 수 없다.

확인 주체는 이렇게 나뉜다. 1·2·4·6과 3의 앞부분(은신처·장비·개조 화면에서 조립한 물건이 그대로 있는지)은 메뉴 화면에서 끝나므로 구현자가 게임을 띄워 확인한다. 5는 로그 파일 확인이다. **3의 레이드 입장·복귀 부분만** 실제 플레이가 필요하므로 사용자가 키보드를 잡아야 한다. 구현자는 그 부분을 번호가 붙은 체크리스트로 넘기고, 결과를 받아 이어받는다.

1. 토글을 끄면 그 제한이 되살아나고, 켜면 즉시 풀린다. **게임 재시작 없이**, 화면 재진입만으로.
2. 전부 끈 상태가 바닐라와 같다 — 바닐라에서 들어가던 조합은 들어가고, 안 들어가던 조합은 안 들어간다.
3. **원본 모드가 허용해 준 조합으로 이미 조립해 둔 무기**가, 토글을 끈 뒤에도 분해·삭제·장착 해제되지 않는다. 레이드 입장과 복귀를 포함해 확인한다. 이것이 안티목표의 핵심이다.
4. 같은 판정 지점을 쓰는 기존 플러그인이 계속 동작한다 — 최소한 `Tylevo.FieldAttachments`, `Tyfon.UIFixes`, `maschine-LinkedSearchInStash`, `CompatibilityHighlighter`, `WTT-PackNStrapClient`.
5. `AllinWeaponUnslotted` 제거 후 `E:\Spt\BepInEx\LogOutput.log`와 서버 로그에 오류가 없다.
6. 헬멧 모드 네 개(§1)의 설정이 살아 있다 — 그 모드들이 의도한 슬롯·충돌 동작이 유지되는지 본다.

## 7. 구현자에게 남긴 것

- **난독화 대응.** 위 멤버 이름은 어셈블리에 문자열로 존재하지만, 실제 패치 대상 메서드의 선언 타입·오버로드·시그니처는 구현자가 찾아야 한다. `CanAccept`가 8회 나타나므로 오버로드 또는 여러 타입에 걸쳐 있다. 어느 지점이 각 토글에 맞는지 고르는 것은 구현자 판단이다. `Tylevo.FieldAttachments`가 가장 가까운 참고다.
- **패치 지점 개수.** 토글 여섯 개가 각각 다른 메서드를 요구할 수도, 두세 지점에서 함께 처리될 수도 있다. 설계는 토글의 *의미*를 고정하고 지점 선택은 열어 둔다.
- UI 쪽 필터 표시(개조 화면에서 호환되는 부품만 보여주는 목록)까지 토글을 반영할지. 판정만 바꾸면 목록에는 안 보이지만 끌어다 놓으면 들어가는 상태가 될 수 있다. 사용자 경험상 목록도 함께 바꾸는 편이 낫겠지만, 비용과 충돌 위험을 보고 구현자가 정한다.
- 로그 형식과 분량(`Log decisions`).
- 빌드·설치 스크립트를 `tools/`에 둘지(기존 `install-koreanconfig.ps1`, `install-helmetslotunion.ps1` 방식).

## 8. 미확인 사항

- 개조 화면의 부품 목록이 어느 시점에 만들어지는지(§7의 UI 반영 판단에 필요).
- `AllinWeaponUnslotted` 제거가 기존 프로필에 미치는 영향. Forge 기준 이 모드는 프로필 결속 표시가 없으나, 확인 전에는 프로필 백업이 전제다.
