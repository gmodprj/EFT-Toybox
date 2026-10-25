# HelmetSlotUnion — 헬멧 부착 칸 합집합

헬멧의 부착 칸 필터를 **칸 이름별 합집합**으로 바꾸는 SPT 4.1.6 서버 모드. 이 저장소에서 직접 만든 것이다.

## 왜 필요한가

게임 아이템 DB 는 헬멧 제한을 두 층으로 나눠 둔다.

| 층 | 무엇 | 누가 푸나 |
| --- | --- | --- |
| 착용 조합 (`BlocksEarpiece`·`ConflictingItems` 등) | 헬멧·헤드셋·안면 보호구·안경을 같이 쓸 수 있는지 | [Less Restricting Headwear](https://sp-mod.com/mod/922) (설치됨) |
| **부착 칸 필터** (`Slots[].filters[].Filter`) | **어느 헬멧의 어느 칸에 어떤 파츠가 들어가는지** | **이 모드** |

922 는 아래 층을 의도적으로 건드리지 않는다(README: 차단 플래그와 명시적 충돌 항목만 제거). 그래서 Altyn 바이저를 Ops-Core FAST 에 끼우는 식은 그대로 막혀 있었고, 4.1.6 호환 모드도 Forge 에 없었다.

## 무엇을 하나

같은 이름의 칸을 헬멧 전체에서 모아 그 **합집합**을 각 칸의 필터로 다시 써 준다. 칸 이름이 같으면 붙는 자리도 같다는 전제다. 칸 이름은 대소문자가 섞여 있어(`Helmet_top` / `helmet_top`) 소문자로 맞춰 묶는다.

적용 범위는 `Headwear`(`5a341c4086f77401f2541505`) 밑에 달린 아이템 중 부착 칸이 있는 것이다. 부모 → 자식 색인을 만들어 내려가므로 모드가 중간 분류를 끼워 넣어도 따라간다.

설치 시점(2026-10-26) 기준 **헬멧 64종 / 칸 묶음 11개**이고, 필터 항목 합계는 **495 → 6719** 로 늘어난다.

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

## 설정 — `config.jsonc` (DLL 옆)

| 항목 | 기본값 | 내용 |
| --- | --- | --- |
| `ModEnabled` | `true` | `false` 면 DB 를 전혀 건드리지 않는다 |
| `Debug` | `false` | 칸마다 `헬멧id / 칸이름: 전 → 후 (+n)` 를 기록한다 |
| `MergeEquipmentSlots` | `false` | `mod_equipment` 와 `mod_equipment_000·001·002…` 를 한 묶음으로 본다 |
| `ExcludedSlotNames` | `[]` | 건드리지 않을 칸 이름(대소문자 무시) |
| `ExcludedHelmetIds` | `[]` | 건드리지 않을 헬멧 아이템 id |

고친 뒤에는 **서버를 다시 켜야** 한다.

### 알아 둘 것 두 가지

- **`helmet_top`·`helmet_back` 은 내장 방탄판 자리다**(36종). 합치면 아무 헬멧에 아무 방탄판을 넣을 수 있어 방어력이 크게 달라진다. 파츠 호환만 풀고 방어력은 그대로 두려면 `"ExcludedSlotNames": ["helmet_top", "helmet_back"]` 으로 적는다.
- **`mod_equipment_000·001·002` 는 번호의 뜻이 헬멧마다 다르다.** Ops-Core FAST 는 `001` 이 RAC 헤드셋, `002` 가 SLAAP 방탄판이다. `MergeEquipmentSlots` 를 켜면 헤드셋 자리에 방탄판이 들어가는 조합까지 열리므로 기본은 꺼짐이다.

모양이 겹쳐 3D 로 어색하게 보이는 조합은 생길 수 있다. 필터만 넓히는 모드라 그래픽은 게임이 하는 대로 둔다.

## 빌드

SPT 4.1.6 서버는 `net10.0`(`SPT.Server.runtimeconfig.json` 의 `tfm`)이라 **.NET 10 SDK 가 필요하다.** SDK 8 로 빌드하면 `NETSDK1045` 로 멈춘다.

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

단, **이 모드로만 가능했던 조합을 끼운 채 모드를 빼면** 그 헬멧은 필터에 없는 자식을 가진 상태가 된다. 모드를 뺄 생각이라면 먼저 그런 조합을 풀어 둔다.

## 구조

| 파일 | 내용 |
| --- | --- |
| `HelmetSlotUnion.cs` | `IOnLoad` 구현. 합집합을 모으고 되돌려 쓴다 |
| `ModMetadata.cs` | `IModMetadata` — 서버가 읽는 모드 정보 |
| `Config.cs` | `config.jsonc` 를 담는 틀 |
| `config.jsonc` | 기본 설정. 설치 스크립트가 함께 복사한다 |

쓰는 서버 API 는 [Less Restricting Headwear](https://sp-mod.com/mod/922) 와 같다: `[Injectable]` + `IOnLoad.OnLoadAsync`, `TemplateTable.Items`, `ModHelper.GetJsonDataFromFile<T>`, `ISptLogger<T>`.
