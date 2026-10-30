# PiP Disabler — 고배율로 바꾸면 시야가 조준경 아래로 떨어진다

2026-10-29~30. Mk18 묠니르(`5fc22d7c187fea44d52eda44`)에서 조준경을 고배율로 바꾸면
접안부 테두리가 통째로 보이고 조준선이 렌즈 아래 가장자리에 걸린다. 눈이 광축보다
아래·뒤에 놓인 모습이다. **은신처 사격장에서도 똑같이 난다.** 레이드 전용이 아니다.

`Picture in Picture Disabler`(`com.fiodor.pipdisabler`, 2.1.0)를 **끄면 정상**이다.
그래서 이 모드가 관여하는 것은 확실하지만, 끄면 쓰려던 기능을 못 쓴다.

## 지금 상태 — 정상으로 돌아왔고 그대로 둔다

`BepInEx\config\com.fiodor.pipdisabler.cfg`의 `[Hacks]` 다섯 값이 기본값이 아니다.
**이 상태에서 증상이 사라졌으므로 건드리지 않는다.**

| 설정 (F12 표기: `보정 처리`) | 기본값 | 현재 |
| --- | --- | --- |
| ADS Scope Alignment Angle Tolerance / 조준 정렬 허용 각도 | 2 | **15** |
| Post Sprint ADS Gate Duration / 달린 뒤 정렬 검사 시간 | 1 | **0** |
| Post Stance ADS Gate Duration / 자세 전환 뒤 정렬 검사 시간 | 1 | **0** |
| Bypass During Stance Transitions / 자세 전환 중 건너뛰기 | true | **false** |
| Suppress Magnification Switch Movement / 배율 전환 동작 숨기기 | true | **false** |

나중에 조준경 쪽에서 다른 증상이 나오면 **여기부터 의심한다.**

## 원인 — 확정하지 못했다

위 다섯 값 말고는 끝까지 바뀐 것이 없다. 그래서 **이것이 해결책일 가능성이 가장 높다.**
다만 확정은 아니다. 이 값을 처음 껐을 때는 증상이 그대로였고, 그 뒤 여러 번
껐다 켜는 사이에 비로소 사라졌다. 모드가 조준경 초기화 때 한 번만 읽는 값이라면
재시작이 필요했다는 설명이 되지만, 확인하지 않았다.

가르는 방법: `자세 전환 중 건너뛰기` 하나만 다시 켜고 **재시작**한다. 증상이 돌아오면 답이다.

### 관련 코드 경로

DLL(`BepInEx\plugins\PiP-Disabler\PiP-Disabler.dll`)에서 확인한 이름들이다.

```
IsPlayerSprintingOrStanceTransitioning
ArmPostSprintAimGate / ArmPostStanceAimGate
IsScopeAlignedWithMainCamera / ScopeAlignmentAngleTolerance
ShouldApplyScopeAlignmentGate
ApplyBypassState / RestoreBypassedOpticState
```

달리기·자세 전환 뒤에는 조준선과 카메라 각도가 허용 각도 안에 들어올 때까지 모드가
스스로 우회 상태로 간다. 이 모드는 조준경 메시를 잘라 관통해 보이게 하는 방식이라,
우회로 들어가면 잘라 낸 것을 되돌린다. 되돌리는 도중에 배율이 바뀌면 **화각은 고배율인데
조준경 몸통은 돌아와 있는** 상태가 되고, 그것이 증상과 같은 모습이다. 어디까지나
코드 이름에서 읽은 것이고, 실제로 그 경로를 탔는지는 확인하지 못했다.

## 아닌 것으로 밝혀진 것

| 후보 | 어떻게 지웠나 |
| --- | --- |
| EasyMounting의 `Cursed` 설정 | 거치하지 않은 상태에서도 난다 |
| Magic Optic Mount와의 패치 충돌 | 게임 플러그인을 빼고도 그대로 |
| 잘못 저장된 조준경 항목 3개 | 지워도 그대로 (아래) |
| 조준경별 저장값 전체 | 84개를 통째로 빼도 그대로 (아래) |
| Transparent Sights | 설정된 것이 전부 1배율 반사 조준경뿐이라 무관 |
| Scope Rangefinder / Auto Ranging | 영점만 건드린다. 카메라를 안 움직인다 |
| Camera Recoil Mod | 쏠 때만 흔든다. 정지 조준 위치는 그대로 |

### Magic Optic Mount 충돌을 의심한 근거 (결과적으로 아님)

두 모드가 같은 함수를 패치한다. 증상이 배율 전환에만 걸려 의심했다.

```
44:   Loading [7Bpencil.MagicOpticMount 0.1.0]
46:     Patch_ScopeZoomHandler_UpdateScope          ← 배율 전환
45:     Patch_OpticComponentUpdater_CopyComponentFromOptic
1036: Loading [PiP-Disabler 2.1.0]
1050:   OpticComponentUpdaterCopyComponentFromOptic_DisablePiP   ← 같은 함수
```

겹치는 것은 사실이니, 나중에 다른 광학 증상이 나오면 다시 볼 자리다.

### 조준경별 저장값을 전부 빼 본 시험

`BepInEx\plugins\PiP-Disabler\custom_mesh_surgery_settings.json`(조준경별 84개)을
통째로 빼고 재시작했다.

- 문제의 조준경: **정상으로 보였다**
- 멀쩡하던 다른 조준경: **렌즈 안이 새까맣게 됐다** — 제 값이 사라져 공통값으로 돈 탓

여기서 "저장값이 원인"이라고 판단해 파일을 되돌렸는데, **되돌린 뒤에도 정상이었다.**
되돌린 파일은 증상이 나던 때와 바이트 단위로 같다. 즉 **저장값은 원인이 아니다.**

## 조준경별 설정 파일에 관해 알아낸 것

| | |
| --- | --- |
| 위치 | `BepInEx\plugins\PiP-Disabler\custom_mesh_surgery_settings.json` |
| 항목 수 | 84 (처음 87에서 아래 3개를 지움) |
| 쓰는 시점 | **저장 키를 누르는 즉시** (`SaveCustomSettingsForScope` → `WriteToDisk`). 게임 종료 때 쓰는 것이 아니다 |
| 저장 키 | `조준경별 설정 저장 키` = `KeypadPeriod`. **화면 표시도 로그도 없다** — 파일 수정 시각으로만 확인된다 |
| 손으로 고칠 때 | 게임을 끈 상태에서. 켜진 동안은 모드가 메모리 값을 들고 있다 |

### 지운 항목 3개

조준경 이름이 아니라 **가변 배율 조준경의 배율 단계 오브젝트 이름**으로 저장돼 있었다.
모드가 조준경 이름을 못 찾고 자식 오브젝트 이름으로 저장한 것이다.

```
mode       최소 1    최대 2.4     ← scope_30mm_sig_tango6t_1_6x24 와 값이 완전히 같음
mode_000   최소 2.4  최대 5
mode_001   최소 2.4  최대 5
```

지워도 증상은 그대로였다. 원인은 아니었지만 잘못된 항목인 것은 맞아 되돌리지 않았다.

### 문제의 조준경

F12 편집 칸(`메시 절단 — 조준경별`)의 값과 일치하는 항목을 찾아 확인했다.

**`scope_30mm_march_tactical_3-24x42`** — March Tactical 3-24x42

```
WeaponScaleMinMagnification = 2
WeaponScaleMaxMagnification = 12
NearPreserveDepth = 0.07511737
VisualRecoilCompensation = 0.2
```

이 값은 **처음 훑었을 때부터 한 번도 바뀌지 않았다.** 저장값이 원인이 아니라는 또 다른 증거다.

## 모드를 끄지 않고 빠지는 길 (이번에는 안 썼다)

증상이 다시 나오거나 다른 조준경에서 같은 일이 생기면 쓸 수 있다.

| 방법 | 기본 동작 | 처음 쓰는 조준경 |
| --- | --- | --- |
| `제외 목록`(Blacklist) | 전부 모드 적용 | 깨질 수 있음 — 겪고 나서 추가 |
| `허용 목록`(Whitelist) | **전부 바닐라** | 안전 — 확인한 것만 추가 |

`허용 목록`은 비어 있으면 무시되고, 뭐라도 넣는 순간 거기 없는 조준경은 전부 바닐라로
그려진다(`ShouldBypassByWhitelist`). 조준경을 하나씩 쫓지 않아도 되는 쪽이다.

각각 `추가·삭제 키`를 지정해 두고 조준한 채 누르면 현재 조준경이 목록에 들고 난다.
`이름으로 자동 제외`에 이미 D-EVO와 PAG-17이 들어가 있는데, 제작자가 처리 못 하는
조준경을 빼라고 마련해 둔 통로다.

## 백업

| 파일 | 내용 |
| --- | --- |
| `backup/pip-disabler/custom_mesh_surgery_settings.2026-10-29.json` | 맨 처음 87개 |
| `backup/pip-disabler/custom_mesh_surgery_settings.84entries.json` | 지금 쓰는 84개 |
| `backup/pip-disabler/custom_mesh_surgery_settings.DISABLED.json` | 통째로 뺐을 때 보관본(84개와 같음) |

되돌리려면 원하는 사본을 `custom_mesh_surgery_settings.json` 이름으로
`E:\Spt\BepInEx\plugins\PiP-Disabler\`에 넣는다. **게임을 끈 상태에서.**
