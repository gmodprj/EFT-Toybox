# HelmetSlotUnion — 헬멧 부착 칸 합집합

헬멧의 부착 칸 필터를 **칸 이름별 합집합**으로 바꾸고, 부품끼리 서로를 막는 제한(`ConflictingItems`)을 푸는 SPT 4.1.6 서버 모드. 이 저장소에서 직접 만든 것이다. 충돌 제한 쪽은 **총기 부착물까지** 본다([아래](#총기-부착물의-충돌도-지운다)).

## 왜 필요한가

게임 아이템 DB 는 헬멧 제한을 두 층으로 나눠 둔다.

| 층 | 무엇 | 누가 푸나 |
| --- | --- | --- |
| 착용 조합 (`BlocksEarpiece` 등) | 헬멧·헤드셋·안면 보호구·안경을 같이 쓸 수 있는지 | [Less Restricting Headwear](https://sp-mod.com/mod/922) (설치됨) |
| **부착 칸 필터** (`Slots[].filters[].Filter`) | **어느 헬멧의 어느 칸에 어떤 파츠가 들어가는지** | **이 모드** |
| **파츠끼리의 충돌** (`ConflictingItems`) | **끼운 파츠 둘이 서로를 막는지** | **이 모드** |

922 는 아래 층을 의도적으로 건드리지 않는다(README: 차단 플래그와 명시적 충돌 항목만 제거). 그래서 Altyn 바이저를 Ops-Core FAST 에 끼우는 식은 그대로 막혀 있었고, 4.1.6 호환 모드도 Forge 에 없었다.

## 무엇을 하나

같은 이름의 칸을 헬멧 전체에서 모아 그 **합집합**을 각 칸의 필터로 다시 써 준다. 칸 이름이 같으면 붙는 자리도 같다는 전제다. 칸 이름은 대소문자가 섞여 있어(`Helmet_top` / `helmet_top`) 소문자로 맞춰 묶는다.

적용 범위는 `Headwear`(`5a341c4086f77401f2541505`) 밑에 달린 아이템 중 부착 칸이 있는 것이다. 부모 → 자식 색인을 만들어 내려가므로 모드가 중간 분류를 끼워 넣어도 따라간다.

설치 시점(2026-10-26) 기준 **헬멧 64종 / 칸 묶음 11개**이고, 필터 항목 합계는 **495 → 6719** 로 늘어난다. 아래 표와 이 숫자들은 **바닐라 DB 만** 셌을 때의 것이다 — 2026-11-05 에 실행 순서를 `PostLoad` 로 옮긴 뒤로는 모드가 만든 헬멧까지 들어와 **헬멧 214종 / 칸 묶음 16개 / 974개 칸에 32882개**가 된다(이 설치본 실측).

| 칸 묶음 | 헬멧 수 | 지금 필터 | 합집합 |
| --- | --- | --- | --- |
| `helmet_top` | 62 | 1 | 36 |
| `helmet_back` | 62 | 1 | 36 |
| `mod_nvg` | 38 | 1~3 | 7 |
| `helmet_ears` | 34 | 1 | 19 |
| `mod_equipment` | 19 | 1~18 | 26 |
| `mod_mount` | 19 | 1~18 | 22 |
| `mod_equipment_000` | 18 | 1~4 | 16 |
| `mod_equipment_002` | 9 | 1~3 | 5 |
| `mod_equipment_001` | 8 | 0~2 | 6 |
| `helmet_eyes` | 7 | 1 | 5 |
| `helmet_jaw` | 5 | 1 | 3 |

설정 조합별로 미리 센 결과(같은 로직을 실제 DB 에 돌려 본 것):

| 설정 | 칸 묶음 | 필터 항목 | 바뀐 칸 |
| --- | --- | --- | --- |
| 기본값 | 11 | 495 → 6719 | 281 |
| `MergeEquipmentSlots: true` | 8 | 495 → 8706 | 281 |
| `ExcludedSlotNames: ["helmet_top","helmet_back"]` | 9 | 495 → 2379 | 157 |
| 위 둘 다 | 6 | 495 → 4366 | 157 |

기존 필터를 지우지 않고 `UnionWith` 로 **더하기만** 한다. `Required`·`MaxCount`·`Shift`·`Locked`·`Plate` 같은 다른 칸 속성은 손대지 않는다.

### 파츠끼리의 충돌도 지운다

칸 필터만 넓히면 끼울 수 있는 목록에는 올라오지만, 막상 끼우려 하면 이런 메시지가 뜬다:

> ★ FAST FS (3/40)과 ★ SCRA 아이템은 동시에 장착할 수 없습니다

이건 필터가 아니라 파츠 자신의 `ConflictingItems` 다. 예로 Ops-Core FAST 멀티 히트 바이저(`5a16b7e1fcdbcb00165aa6c9`)는 야간투시경 5종·마운트 3종·SLAAP 방탄판을 막는다(9건).

`RemoveAttachmentConflicts`(기본 켜짐)는 **헬멧과 헬멧 파츠끼리** 서로를 막는 항목만 지운다. 범위는 헬멧과 그 파츠, 파츠에 또 달리는 파츠까지다(바닐라만이면 240개, 모드까지 넣은 이 설치본은 **602개**). 2026-11-05 실측으로 **아이템 223개에서 1881건**을 지우고, 헤드셋·안면 보호구처럼 **범위 밖을 가리키는 2339건은 그대로 둔다** — 그쪽은 착용 조합 이야기라 922 가 맡는다.

`RemoveAllConflictsOnAttachments`(기본 꺼짐)는 범위 안 아이템의 목록을 통째로 비운다. 범위 밖 항목까지 사라지므로 다른 모드와 부딪힐 수 있다.

### 총기 부착물의 충돌도 지운다

총기도 같은 일이 생긴다. 칸 필터를 넓히는 모드([SlotCategoryRelax](../SlotCategoryRelax) · All In Weapon 등)를 쓰면 목록에는 올라오는데, 끼우려 하면 이렇게 막힌다:

> ● MBUS 가늠쇠과 ● TR-1 아이템은 동시에 장착할 수 없습니다

헬멧과 똑같이 `ConflictingItems` 다. `RemoveWeaponAttachmentConflicts`(기본 켜짐)가 **총기와 총기 파츠끼리** 서로를 막는 항목만 지운다. **총기 쪽은 칸 필터를 건드리지 않는다** — 넓히는 것은 다른 모드의 몫이고, 여기서는 넓힌 뒤에도 남는 제한만 푼다.

범위는 `Weapon`(`5422acb9af1c889c16000029`) 밑에서 부착 칸이 있는 총기와, 그 칸에 들어갈 수 있는 파츠를 따라 내려간 것 전부다. 이 설치본에서 서버를 켜 재어 본 결과(2026-11-05) **총기 987종 / 대상 4553개 중 아이템 553개에서 4241건**을 지우고 **203건은 남긴다**(탄약·헬멧처럼 범위 밖을 가리키는 것과 아래 구경 제한).

### 언제 도는가 — `OnLoadOrder.PostLoad`

`[Injectable(InjectionType.Singleton, OnLoadOrder.PostLoad)]`(1000000)이다. **아이템을 만드는 모드보다 뒤에 서야 한다.** 순서는 `TypePriority` 오름차순인데 **200000 이 경계**다 — 그 미만은 PreSptLoad 콜백 묶음 안에서 돌고, 그 이상은 `SPTStartupHostedService` 에서 돈다. 설치된 서버 모드의 선언이 전부 0 이므로 `PostLoad` 하나면 모든 모드 뒤에 선다.

`100000` 으로 두었을 때는 같은 묶음이라 모드가 나중에 만든 아이템을 통째로 놓쳤다. 클라이언트가 받은 목록(`POST /client/items`)으로 잰 차이:

| | 100000 | `PostLoad` |
| --- | --- | --- |
| 총기 충돌 정리 대상 | 2183개 | **4553개** |
| 클라이언트에 남은 충돌 | 3269건 (모드 아이템 분 3032) | **1335건** (모드 분 1159) |
| `[WTT-Armory] Patriot rear sight carry handle` | 73건 | **0건** |

더 늦게(`int.MaxValue`) 가면 클라이언트에 보낼 목록이 이미 굳어 DB 만 바뀌고 게임에는 닿지 않는다(StackUnify 로 실측해 둔 경계다).

| 분류 | 지우는 건수 |
| --- | --- |
| 마운트 | 405 |
| 총열덮개 | 339 |
| 망원 조준경 | 312 |
| 총몸 | 178 |
| 기계식 조준기 | 172 |
| 총열 | 168 |
| 개머리판 | 158 |
| 나머지(유탄발사기·소염기·조준경·손잡이 등) | 288 |

**탄창과 장전 손잡이(볼트)는 기본으로 건드리지 않는다.** 그쪽 충돌은 모양이 아니라 구경 이야기라서다 — AXMC `.338` 볼트는 `7.62x51` 총열 3종을, `.338` 탄창은 `7.62` 볼트·총열을 막는다. 지우면 구경이 섞인 총이 만들어진다. `WeaponConflictExcludedCategoryIds` 가 그 두 분류를 담고 있고, 어느 쪽에 있든(건 쪽이든 막힌 쪽이든) 그 항목은 남는다.

`RemoveAllConflictsOnWeaponAttachments`(기본 꺼짐)는 범위 안 아이템의 목록을 통째로 비운다. 구경 제한도 탄약 제한도 함께 사라진다.

### 남은 것까지 — `RemoveRemainingConflicts`

앞의 두 패스는 **같은 범위 안끼리만** 지운다. 그래서 범위에 걸친 것이 남는다 — 안면 보호구↔헬멧, 헤드셋↔헬멧, 헬멧 쪽에서만 닿는 마운트↔기계식 조준기 같은 것들이다. `RemoveRemainingConflicts`(기본 켜짐)가 남은 것을 통째로 비운다. 단 `WeaponConflictExcludedCategoryIds`(탄창·장전 손잡이)는 여기서도 남긴다.

이 쓸기는 **두 번** 돈다. `PostLoad` 에서 한 번(아이템 376개 / 10186건), 그리고 `int.MaxValue` 자리에서 한 번 더다(`LateConflictSweep`). 뒤의 것이 필요한 이유는 실측했다 — `PostLoad` 에서 다 비웠는데도 클라이언트가 받은 목록에는 **884건이 남아 있었다**(ECOTI 서멀 라이저 390, 안면 보호구 369 …). 우선순위를 적지 않은 `Injectable` 은 기본값이 `int.MaxValue` 라 `PostLoad` 보다 늦게 돌고, 그 자리에서 다시 쓰는 모드가 있다. 맨 끝 쓸기가 아이템 3개에서 759건을 더 지우자 클라이언트 쪽은 **125건**만 남았다 — 그 125건이 일부러 남긴 구경 제한(탄창 74 · 장전 손잡이 51)이다.

> `int.MaxValue` 자리의 쓰기가 클라이언트에 닿는지는 StackUnify 때 스택 크기로는 "닿지 않는다" 였는데, `ConflictingItems` 는 **닿는다**. 목록이 굳는 지점이 값마다 다르다는 뜻이다.

구경 제한까지 열고 싶으면 `WeaponConflictExcludedCategoryIds` 를 비운다. 그러면 `.338` 볼트에 `7.62` 총열이 끼워진다.

## 설정 — `config.jsonc` (DLL 옆)

| 항목 | 기본값 | 내용 |
| --- | --- | --- |
| `ModEnabled` | `true` | `false` 면 DB 를 전혀 건드리지 않는다 |
| `Debug` | `false` | 칸마다 `헬멧id / 칸이름: 전 → 후 (+n)` 를 기록한다 |
| `MergeEquipmentSlots` | `false` | `mod_equipment` 와 `mod_equipment_000·001·002…` 를 한 묶음으로 본다 |
| `RemoveAttachmentConflicts` | `true` | 헬멧·헬멧 파츠끼리 서로를 막는 `ConflictingItems` 를 지운다 |
| `RemoveAllConflictsOnAttachments` | `false` | 범위 안 아이템의 `ConflictingItems` 를 통째로 비운다 |
| `RemoveWeaponAttachmentConflicts` | `true` | 총기·총기 파츠끼리 서로를 막는 `ConflictingItems` 를 지운다 |
| `RemoveAllConflictsOnWeaponAttachments` | `false` | 총기 범위 안 아이템의 `ConflictingItems` 를 통째로 비운다 |
| `WeaponConflictExcludedCategoryIds` | 탄창·장전 손잡이 | 총기 충돌 정리에서 뺄 분류 id. 어느 쪽에 있든 그 항목은 남는다 |
| `ExcludedWeaponIds` | `[]` | 건드리지 않을 총기 아이템 id |
| `RemoveRemainingConflicts` | `true` | 두 범위에 걸쳐 남은 `ConflictingItems` 를 전부 비운다(탄창·장전 손잡이는 남는다) |
| `ExcludedSlotNames` | `[]` | 건드리지 않을 칸 이름(대소문자 무시) |
| `ExcludedHelmetIds` | `[]` | 건드리지 않을 헬멧 아이템 id |

이미 설치된 `config.jsonc` 에 새 항목이 없어도 된다. 없는 항목은 위 기본값으로 읽으므로, 그대로 두면 총기 충돌 정리가 켜진 상태로 동작한다.

고친 뒤에는 **서버를 다시 켜야** 한다.

### 알아 둘 것 두 가지

- **`helmet_top`·`helmet_back` 은 내장 방탄판 자리다**(36종). 합치면 아무 헬멧에 아무 방탄판을 넣을 수 있어 방어력이 크게 달라진다. 파츠 호환만 풀고 방어력은 그대로 두려면 `"ExcludedSlotNames": ["helmet_top", "helmet_back"]` 으로 적는다.
- **`mod_equipment_000·001·002` 는 번호의 뜻이 헬멧마다 다르다.** Ops-Core FAST 는 `001` 이 RAC 헤드셋, `002` 가 SLAAP 방탄판이다. `MergeEquipmentSlots` 를 켜면 헤드셋 자리에 방탄판이 들어가는 조합까지 열리므로 기본은 꺼짐이다.

모양이 겹쳐 3D 로 어색하게 보이는 조합은 생길 수 있다. 필터만 넓히는 모드라 그래픽은 게임이 하는 대로 둔다.

## 빌드

SPT 4.1.6 서버는 `net10.0`(`SPT.Server.runtimeconfig.json` 의 `tfm`)이라 **.NET 10 SDK 가 필요하다.** SDK 8 로 빌드하면 `NETSDK1045` 로 멈춘다. 이 PC 는 `winget install Microsoft.DotNet.SDK.10` 로 10.0.402 를 깔아 뒀다.

```powershell
dotnet build src/HelmetSlotUnion -c Release
# SPT 가 다른 곳에 있으면
dotnet build src/HelmetSlotUnion -c Release -p:SptDir="D:\Spt"
```

참조하는 서버 어셈블리(`SPTarkov.Server.Core`·`SPTarkov.Common`·`SPTarkov.DI`·`SemanticVersioning`)는 `$(SptDir)\SPT_Runtime` 에서 직접 가져온다. `Private=false` 라 결과물에 복사되지 않는다.

## 설치

```powershell
pwsh -File tools/install-helmetslotunion.ps1
```

`SPT_Runtime\user\mods\HelmetSlotUnion\` 에 DLL 과 `config.jsonc` 를 넣는다. 설정이 이미 있으면 그대로 둔다.

## 되돌리기

폴더(`SPT_Runtime\user\mods\HelmetSlotUnion`)를 지우고 서버를 다시 켜면 된다. DB 를 메모리에서만 고치므로 파일에 흔적이 남지 않고, 프로필 결속도 아니다.

단, **이 모드로만 가능했던 조합을 끼운 채 모드를 빼면** 그 헬멧은 필터에 없는 자식을 가진 상태가 된다. 총기도 마찬가지다 — 충돌이 되살아나면 그 조합으로 조립해 둔 총과 저장해 둔 조립도(프리셋)가 성립하지 않는다. 모드를 뺄 생각이라면 먼저 그런 조합을 풀어 둔다.

## 구조

| 파일 | 내용 |
| --- | --- |
| `HelmetSlotUnion.cs` | `IOnLoad` 구현. 합집합을 모으고 되돌려 쓴다 |
| `ModMetadata.cs` | `IModMetadata` — 서버가 읽는 모드 정보 |
| `Config.cs` | `config.jsonc` 를 담는 틀 |
| `config.jsonc` | 기본 설정. 설치 스크립트가 함께 복사한다 |

쓰는 서버 API 는 [Less Restricting Headwear](https://sp-mod.com/mod/922) 와 같다: `[Injectable]` + `IOnLoad.OnLoadAsync`, `TemplateTable.Items`, `ModHelper.GetJsonDataFromFile<T>`, `ISptLogger<T>`.
