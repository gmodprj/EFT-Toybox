# 8-B 택일 3건 비교 (2026-10-27)

설치본과 기능이 겹치는 후보 3개를 실제 데이터로 비교했다. Forge 설명뿐 아니라 **설치본의 설정
파일과 후보의 DLL·설정을 직접 열어서** 확인했다(후보는 받아서 보기만 했고 설치하지 않았다).

요약:

| 후보 | 겹치는 설치 모드 | 실제로 겹치나 | 권고 |
| --- | --- | --- | --- |
| Recoil Rework (Legacy) 2190 | BETTER ATTACHMENTS 1939 | 덮어쓰지 않는다. 층이 다르고 효과가 누적된다 | **넣어도 된다** — 단 반동을 다시 맞출 각오로 |
| Instant Insurance 2394 | Equipment Is Eternal 2897 | 기능이 거의 포함된다 | **넣지 않는다** |
| Shorter Magazines 3186 | Slimmer Magazines 2373 | 같은 속성을 덮어쓴다 | **하나만** — 지금 설정이면 바꿀 이유가 약하다 |

---

## 1. Recoil Rework (Legacy) 2190 vs BETTER ATTACHMENTS 1939

| | BETTER ATTACHMENTS (설치됨) | Recoil Rework 2190 (후보) |
| --- | --- | --- |
| 바꾸는 대상 | **부착물** 아이템의 `recoil`·`ergonomics` 값 | **무기** 단위 속성 (`RecoilPosZMult` 등) + 클라이언트 반동 처리 |
| 구성 | 부착물별 개별 수치 (`config/config.json`) | 무기군별 JSON 10개 + 클라이언트 DLL |
| 4.1.6 | 설치됨 | 2.0.0, 제약 `4.1.x`, 2026-09-09, 35 KB |
| 결속 | - | 아니오 |

**덮어쓰기 충돌은 없다.** 서로 다른 계층을 건드린다. 1939는 예를 들어 Magpul M-LOK AFG 의
`recoil` 을 -3 → -4 로 바꾸고, 2190은 M4A1 자체의 `RecoilPosZMult` 를 0.8 로 바꾼다.

1939 는 현재 **11개 토글이 전부 켜져 있다** — `betterMuzzleRecoil`, `betterSuppressorRecoil`
포함. 즉 부착물 쪽 반동 감소가 이미 최대로 적용돼 있다.

여기에 2190 을 얹으면 **감소가 곱해진다.** 반동이 과하게 낮아질 수 있으니, 넣는다면 설치 후
사격감을 다시 맞출 생각으로 넣어야 한다. 2190 은 F12 설정이 있는 클라이언트 모드라 조절은
된다. 비결속이라 되돌리기도 파일 삭제뿐이다.

---

## 2. Instant Insurance 2394 vs Equipment Is Eternal 2897

**넣을 이유가 없다.** 설치본 설정을 보면 2897 이 2394 가 하는 일을 이미 포함한다.

`EquipmentIsEternal/config.jsonc` 현재 값:

```json
{
  "OnlyRestoreInsured": false,
  "RestoreArmBand": false, "RestoreSpecialSlots": false,
  "RestoreScabbard": false, "RestoreSecuredContainer": false,
  "KeepQuestItems": false, "DeduplicateItems": true
}
```

핵심은 `OnlyRestoreInsured: false` 다. **보험 여부와 무관하게** 사망 시 장비를 복원한다.

2394 는 Forge 설명대로 "사망 후에만, **보험에 든 물건**을 즉시 돌려준다". 보험을 들어야 하고,
돌아오는 범위도 더 좁다. 2897 이 이미 보험 없이 전부 돌려주는 상황에서 더해지는 게 없다.

계획서에 적었던 두 가지 주의사항 중 하나는 **해당 없음**으로 확인됐다.

- `lostOnDeath.wipeOnRaidStart` 충돌 → `SPT_Data/configs/lostondeath.json` 에서 **이미 `false`**
- "FIR Means FIR 비호환" → 그 모드는 설치본에 없다

즉 2394 를 막는 건 충돌이 아니라 **중복**이다. 비결속이라 위험하지는 않지만 얻는 것이 없다.

---

## 3. Shorter Magazines 3186 vs Slimmer Magazines 2373

**둘 다 탄창 아이템의 `Width`/`Height` 를 직접 쓴다.** DLL 식별자로 확인했다.

| | Slimmer Magazines (설치됨) | Shorter Magazines 3186 (후보) |
| --- | --- | --- |
| 핵심 함수 | `ReduceMagazineThickness` | `IsMagazine` / `MagazineCategory` |
| 쓰는 속성 | `set_Width`, `set_Height`, **`set_ExtraSizeDown`** | `set_Width`, `set_Height` |
| 줄이는 방향 | **두께**(폭) — 2x2·1x3 → 1x2 | **길이** — 3x1 → 2x1 |
| 결속 | 아니오 | **예** |
| 4.1.6 | 설치됨 | 최신 1.1.0 (2026-10-22, 7.4 KB). 아래 분석은 1.0.0 기준 |

3186 제작자도 모드 페이지에 2373 을 "대안"으로 적으면서 **"2x2 를 1x2 로도 바꾸는 게 싫어서
만들었다"** 고 밝히고 있다. 즉 둘은 취향이 갈리는 **대체재**지 보완재가 아니다.

같은 속성을 쓰므로 **같이 넣으면 나중에 도는 쪽이 이긴다.** 어느 쪽이 나중인지는 로드 순서에
달려 있어 예측이 어렵고, 섞이면 탄창 크기가 뒤죽박죽이 될 수 있다.

판단 근거:

- 지금 2373 이 이미 돌고 있고 불편하다는 말씀이 없었다
- 3186 은 **결속 모드**다. 2373 은 아니다 — 바꾸면 되돌리기 비용이 올라간다
- 3186 은 2373 보다 적게 바꾼다(길이만). 가방 공간은 2373 쪽이 더 넓어진다
- 다운로드 수도 2373 쪽이 훨씬 많다

**2x2 탄창이 1x2 로 납작해지는 게 거슬리실 때만** 바꿀 가치가 있다. 바꾼다면 2373 을 먼저
제거하고 3186 을 넣는다 — 같이 두지 않는다.

---

## 확인하지 않은 것

- Forge 의 `filter[spt_version]` 버전 목록은 **오래된 순**이다. 처음에 각 모드의 첫 항목을
  최신으로 읽어 3186 을 1.0.0, 2394 를 1.1.0 으로 적었는데 틀렸다. 4.1.6 에서 쓸 수 있는
  최신은 **3186 1.1.0**(2026-10-22), **2394 1.1.2**(2026-09-05)다. 위 3186 분석은 1.0.0
  DLL 을 연 결과이고, 1.1.0 은 확인하지 않았다. 2190 은 4.1.6 대응이 2.0.0 하나뿐이라
  영향이 없다
- 세 후보 모두 **설치하지 않았다.** 받아서 파일만 열어 봤다
- 반동 누적이 실제로 어느 정도인지는 사격해 봐야 안다. 수치가 곱해진다는 것까지가 확인된 범위
- 3186 과 2373 을 같이 넣었을 때 실제로 어느 쪽이 이기는지는 시험하지 않았다. 같이 쓰지
  않는 것을 전제로 했다
