# 8차 결과 (2026-10-27)

계획은 [PLAN-STAGE8.md](PLAN-STAGE8.md), 후보 검증은 [CANDIDATES-OP-416.md](CANDIDATES-OP-416.md).
이 문서는 **무엇을 넣고 무엇을 뺐으며 왜 그랬는지**를 한 장으로 모은 것이다.

설치된 모드 **165개 → 179개**. 서버 모드 로드 **106개 오류 0**, 게임 쪽 플러그인 **134개**.

## 넣은 것 (14개)

| 단계 | 모드 | 버전 | 결속 | 게임 확인 |
| --- | --- | --- | --- | --- |
| 8-A | Dev Balaclava Fix | 1.2.0 | - | 로드 |
| 8-A | Headshot Damage Redirection | 1.8.0 | - | 로드 |
| 8-A | MeleeMultiplier | 0.0.4 | - | 서버 |
| 8-A | The Blacklist | 3.1.1 | - | 서버 |
| 8-A | Fujiwara Mokou | 1.2.0 | - | 서버 |
| 8-B | Recoil Rework (Legacy) | 2.0.0 | - | 로드 |
| 8-C | Olympus | 4.1.3 | **결속** | **확인 완료** |
| 8-C | SIM - Simple Item Modifier | 0.1.0 | **결속** | 서버 |
| 8-D | MagicBullet | 1.0.0 | - (치트) | 로드 |
| 8-D | Tushonka-800 | 1.0.1 | - (치트) | **확인 완료** |
| 8-E | Waypoints - Expanded Navmesh | 1.9.0 | - | **확인 완료** |
| 8-E | PIT Fireteam | 1.0.0 | - | **확인 완료** |
| 8-F | Make Shotguns Great Again! | 1.19.1 | **결속** | **확인 완료** |
| 8-G | Game Panel HUD 포팅본 | 4.1.6-port | - | **확인 완료** |

## 넣지 않기로 한 것 (4개)

| 모드 | 이유 |
| --- | --- |
| Instant Insurance 2394 | **중복.** Equipment Is Eternal 이 `OnlyRestoreInsured: false` 로 보험 없이 전부 복원한다. 2394 는 보험 든 것만 돌려준다. 계획서가 걱정하던 `wipeOnRaidStart` 충돌은 이미 `false` 라 해당 없고, FIR Means FIR 은 미설치 |
| Shorter Magazines 3186 | **대체재.** Slimmer Magazines 와 둘 다 탄창 `Width`/`Height` 를 덮어쓴다. Slimmer 는 두께, 3186 은 길이를 줄인다. 3186 은 결속이고 Slimmer 는 아니다 |
| Death Shadow Tweaks 2943 | **중복.** 데스 섀도 마스크 네 변형의 `ConflictingItems` 26개(헬멧 24 + Maska 안면 보호대 2)를 Less Restricting Headwear 가 이미 전부 지운다. LRH 의 debug 로그로 측정 확인 |
| SPT Stance Overhaul 3136 | Field Attachments 의 팝업은 누락이 아니라 선택 연동 안내다. 넣으면 결속 1 + 미설치 라이브러리 2 가 늘고 얻는 것은 애니메이션뿐 |

근거는 [COMPARE-STAGE8-B.md](COMPARE-STAGE8-B.md), [REVIEW-STAGE8-D.md](REVIEW-STAGE8-D.md),
[PLAN-STAGE8.md](PLAN-STAGE8.md) 의 8-C 결과 절에 있다.

## 넣었다가 뺀 것 (1개)

**Miyako Carry Service 2709** — 레이드에서 나중에 스폰되는 적 AI 가 멈췄다. 제거 후 정상으로
돌아왔다. 경위와 **내 오진 둘**은 [fixes/2026-10-27-pit-stash-ammo.md](fixes/2026-10-27-pit-stash-ammo.md).

## 설치 중에 고친 것

| 대상 | 내용 |
| --- | --- |
| 프로필 | 9x19 글록 탄창에 5.56 소총탄이 장전돼 있었다. PIT 가 스태시를 재검증하다 실패해 동료 생성이 깨졌다. 항목 1개 제거 → [fixes/2026-10-27-pit-stash-ammo.md](fixes/2026-10-27-pit-stash-ammo.md) |
| Olympus 데이터 | 없는 총기를 참조해 탄창 적용이 207/222 에서 멈췄다. 12개를 빼 209개 전부 적용 → [mod-patches/505-olympus-firearmMags.md](mod-patches/505-olympus-firearmMags.md) |
| `tools/install-and-test.ps1` | 서버 테스트 중 **다른 모드가** 만든 파일을 설치 중인 모드 것으로 기록했다. 자기 폴더 밖이면 `OTHERMOD` 로 알리고 기록하지 않게 고쳤다 |
| `tools/gen-installed-mods.mjs` | 단계 루프가 1~7 로 하드코딩돼 8차가 표에 안 나왔다 |
| `doc/PROCEDURE.md` | 해제 명령을 bsdtar 절대경로로. Git Bash 에서 `tar.exe` 는 GNU tar 가 먼저 잡혀 zip 을 거부한다 |

## 남아 있는 것

- **PIT Fireteam `LootPatrolLayer.GetDecision` NullReferenceException** — 레이드 중 3회.
  모드가 스스로 억제해 AI 는 계속 돌지만, 동료의 전리품 순찰 판단이 간헐적으로 실패한다.
  제작자에게 보고할 거리다
- **COTI 3.4.2 per-frame `ArgumentOutOfRangeException`** 1회 — 8차와 무관한 기존 모드
- **Fika 미설치 관련 오류 1건** — 싱글 환경에서는 정상
- MagicBullet·Recoil Rework 의 **체감**은 확인하지 않았다. 로드만 확인했다
- SIM 은 Delta 규칙을 만들기 전까지 아무것도 바꾸지 않는다

## 주의해서 쓸 것

- **MagicBullet 의 근접 슬라이더는 x1 로 둔다.** MeleeMultiplier 가 서버에서 이미 x10 이라
  층이 달라 곱해진다 — x10 으로 두면 실질 x100
- **Tushonka-800 은 기능별 on/off 토글이 없다.** 레이드에서 `V` 휠로 골라 쓰는 방식이라,
  쓰지 않으면 그만이다. 수치로 누르려면 `Max Visible Distance (600)` 등을 낮춘다
- **GamePanelHUD 포팅본은 Forge 밖**이다. 라이선스·출처·해시는
  [external-mods/stage8-G-GamePanelHUD-port.md](external-mods/stage8-G-GamePanelHUD-port.md).
  원본도 선행 KmyTarkovApi 도 4.1.x 대응이 없어 둘 다 재빌드해 동봉한 판이다
- 결속 모드 3개(Olympus·SIM·Make Shotguns Great Again)는 플레이 후 제거가 위험하다.
  설치 직전 프로필이 `backup/profiles-before-stage8-*` 에 있다
