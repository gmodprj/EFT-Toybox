# 문서 색인

대상: `E:\Spt` (SPT 4.1.6, EFT 0.16.9.5.40743).
**2026-11-06 기준 모드 304개 / 파일 14,001개** + 직접 만든 플러그인 6개. **서버 모드 173개, 게임 플러그인 245개** (둘 다 실측).

> **10차까지 끝났다.** 차수별 결과는 [STAGE10-RESULT.md](STAGE10-RESULT.md), 계획은 [PLAN-STAGE10.md](PLAN-STAGE10.md).
> 가장 최근 세션 인계는 [HANDOVER-2026-11-06.md](HANDOVER-2026-11-06.md).

> 8차까지가 절차를 거친 설치고, 그 뒤 **절차 밖에서 들어온 8개**는 2026-10-28 에 설치 기록을
> 뒤늦게 복원해 문서에 올렸다. 당시 기록이 아니라 "지금 가진 파일" 목록이다 —
> [REVIEW-LOAD-ORDER.md](REVIEW-LOAD-ORDER.md) 와 INSTALLED-MODS.md 의 "절차 밖에서 들어온 것"
> 절을 함께 본다.

## 먼저 볼 문서

| 문서 | 내용 |
| --- | --- |
| [MODS-GUIDE.md](MODS-GUIDE.md) | 설치된 모드가 무엇을 하는지 — 기능별 묶음, 직접 만든 것, 결속 목록, **알려진 충돌**, 지우면 안 되는 파일, 손볼 설정 |
| [REVIEW-LOAD-ORDER.md](REVIEW-LOAD-ORDER.md) | **로드 순서와 기능 겹침 점검** — 순서를 바꿀 수 있는지, 어느 설정이 죽어 있는지. 실측 방법도 적었다 |
| [REVIEW-INTEGRATION-MOD.md](REVIEW-INTEGRATION-MOD.md) | **중복 기능을 통합 모드로 모을 수 있나** — 마지막에 로드되는 모드를 만들 수 있다는 것을 서버 코드로 확정하고(`OnLoadOrder`), 무엇까지 되고 무엇이 안 되는지와 권하는 범위를 적었다 |
| [INSTALLED-MODS.md](INSTALLED-MODS.md) | 지금 설치된 모드 전체 — 버전, 게임/서버 위치, 프로필 결속 여부, 설치 기록 (**자동 생성**, 직접 고치지 않는다) |
| [WORKLOG.md](WORKLOG.md) | 단계별 작업 이력, 발생한 문제와 조치, 바로잡은 안내 |
| [BACKLOG.md](BACKLOG.md) | **나중에 할 일** — 작업 중에 범위 밖으로 미룬 것만. 후보 목록이 아니라 하기로 정한 것 |
| [PROCEDURE.md](PROCEDURE.md) | 모드 추가 절차(조회→다운로드→해제→충돌 검사→설치→**기록 검사**→검증→되돌리기), SVM 메모, 무시해도 되는 로그 메시지 |
| [audit/2026-11-06-install-audit.md](audit/2026-11-06-install-audit.md) | **설치본 정합성 감사 7항목** — 기록↔파일 대조, 중복 기록, 서버 모드 수, 플러그인 DLL↔로드, 고아 cfg, SPT 버전 표기, 테스트 출력. 하위 세션이 만들고 컨트롤러가 숫자를 다시 재어 **결론 두 곳을 정정**했다(실제 손실 0건) |

## 차수별 계획과 결과

| 문서 | 내용 |
| --- | --- |
| [STAGE8-RESULT.md](STAGE8-RESULT.md) | **8차 결과 한 장 요약** — 넣은 14개, 넣지 않기로 한 4개와 이유, 뺀 1개, 설치 중에 고친 다섯 가지. 나머지 8차 문서로 링크가 걸린다 |
| [PLAN-STAGE8.md](PLAN-STAGE8.md) | 8차 계획 — OP·AI 동료·HUD 포팅본. 단계별 순서와 8-C 결과 절 |
| [COMPARE-STAGE8-B.md](COMPARE-STAGE8-B.md) | 8-B 택일 3건 비교 — 설치본 설정과 후보 DLL 을 직접 열어 대조 |
| [REVIEW-STAGE8-D.md](REVIEW-STAGE8-D.md) | 8-D 치트 표시 2개 검토 + Tushonka F12 설정 확정 |
| [CANDIDATES-OP-416.md](CANDIDATES-OP-416.md) | 외부 OP 모드 목록 58개를 Forge 로 재검증한 결과 |
| [PLAN-STAGE7.md](PLAN-STAGE7.md) / [CANDIDATES-STAGE7.md](CANDIDATES-STAGE7.md) | 7차 계획과 후보 65개 |
| [PLAN-STAGE6.md](PLAN-STAGE6.md) | 6차 계획 — sp-mod 밖(GitHub·Nexus)까지 훑은 결과 |
| [PLAN-STAGE5.md](PLAN-STAGE5.md) | 5차 계획과 실행 결과 |
| [CANDIDATES-416.md](CANDIDATES-416.md) / [CANDIDATES-GUN-TRADER.md](CANDIDATES-GUN-TRADER.md) | 미설치 후보 136개 / 총기·상인 재조사 27개 |
| [CANDIDATES-ATTACH-COMPAT.md](CANDIDATES-ATTACH-COMPAT.md) | `All In Weapon` 대신 **총기별로** 부착물 호환을 정하는 수단 — 후보 2개의 소스를 읽어 본 결과와, 직접 만드는 쪽의 셈 |
| [CANDIDATES-LIST-127534.md](CANDIDATES-LIST-127534.md) | 공개 목록 "Encode's Mod List Vol. 1"(모드 215개)에서 **이 설치본에 없는 125개**를 분야별로 — 용량·결속·의존성 포함. 설치 불가 5개와 선행 모드가 막는 3개도 적었다 |
| [STAGE10-RESULT.md](STAGE10-RESULT.md) | 10차 단계별 **실측 결과** — 기준선, 10-A·10-B, 설치 후 게임 기동 확인. 플러그인 수가 왜 +4 인지(라이브러리 DLL·GUID 충돌), F12 한글화 위험, 다음 단계 판정 기준에 더할 것 |
| [PLAN-STAGE10.md](PLAN-STAGE10.md) | 위 두 후보 문서를 **순차 설치·시험 계획**으로 묶은 것 — 설치 전 결정(10-0, 전부 끝남), 부착물 호환 단독 단계(10-A), 넣는 모드 121개를 17단계로. 단계마다 통과·멈춤 판정과 되돌리기를 적었다. 차수가 10인 이유는 생성기의 9차가 "절차 밖에서 들어온 것"에 이미 쓰여서다 |

## 사고와 수정 기록

| 문서 | 내용 |
| --- | --- |
| [fixes/2026-10-27-pit-stash-ammo.md](fixes/2026-10-27-pit-stash-ammo.md) | **봇 AI 정지 사건 전말** — 9x19 탄창의 5.56 탄 때문에 PIT 동료 생성이 깨지고, 반쯤 만들어진 동료 기록이 레이드를 망가뜨렸다. 내가 세운 가설 셋 중 둘이 틀린 경위도 적었다 |
| [fixes/2026-10-28-olympus-missing-item-ids.md](fixes/2026-10-28-olympus-missing-item-ids.md) | **서버 오류 48건의 전말** — Olympus 가 만들지 않는 아이템 id 48개를 참조한다. 증거·배제한 후보·제작자 보고용 영문 정리와 id 48개 전체 |
| [fixes/2026-10-27-pit-lootpatrol-nre.md](fixes/2026-10-27-pit-lootpatrol-nre.md) | PIT `LootPatrolLayer.GetDecision` NullReference 조사 + 제작자 보고용 영문 정리 |
| [fixes/2026-11-05-aicoreagent-invalidcast.md](fixes/2026-11-05-aicoreagent-invalidcast.md) | **BigBrain 봇 업데이트에서 매 프레임 InvalidCast 4,524줄** — STM 을 Release 로 다시 깐 뒤 0줄이 됐지만 그때 변수를 둘 바꿔 **원인은 미확정**이다. 진단용 finalizer 를 어떻게 짰는지도 적었다 |
| [fixes/2026-11-06-lighthouse-rework-load-stall.md](fixes/2026-11-06-lighthouse-rework-load-stall.md) | **리워크 라이트하우스 진입 로딩 멈춤** — 멈춘 자리(심장박동 정지)와 두 번의 메모리 실측. 1차는 남은 4,466 MB 에서 멈추고 2차는 **72 MB 에서 47분을 돌아**, 메모리 여유로 설명하려던 가설을 거뒀다. **원인 미확정** |
| [install-records/README-removed-stale-lines-2026-11-06.md](install-records/README-removed-stale-lines-2026-11-06.md) | 철 지난 설치 기록 **26줄을 지운 내역** — 줄 단위 표와 근거. 기록을 지우는 일이므로 지운 것을 남겼다 |
| [mod-patches/stackunify-pinned-stacks.md](mod-patches/stackunify-pinned-stacks.md) | 구경 43종의 최대 스택을 **지금 쓰이던 값으로 고정**했다 — 한 구경 안에서 값이 갈려 있던 15종을 포함. 넣은 값 전체와 되돌리는 법 |
| [mod-patches/theblacklist-restore-author-bans.md](mod-patches/theblacklist-restore-author-bans.md) | 콘텐츠 모드 제작자가 막아 둔 아이템 858건을 플레아에서 다시 막았다 — **244건이라던 숫자가 틀렸던 경위**와 아직 열려 있는 114건 |
| [mod-patches/svm-currency-off.md](mod-patches/svm-currency-off.md) | SVM 과 BarterItemsStacks 가 같은 화폐 스택을 쓰고 있었다 — SVM 쪽 네 줄을 껐다. **되돌리는 법과 Greed 로 저장할 때 주의** |
| [mod-patches/505-olympus-firearmMags.md](mod-patches/505-olympus-firearmMags.md) | Olympus 가 없는 총기를 참조해 탄창 적용이 중간에 멈추던 것 — 12개 제거. **되돌리는 법과 원본 보관 위치** |
| [external-mods/stage8-G-GamePanelHUD-port.md](external-mods/stage8-G-GamePanelHUD-port.md) | Forge 밖에서 받은 유일한 모드 — 출처·해시·바이러스 검사·라이선스 검토 |

## 그 밖

| 문서 | 내용 |
| --- | --- |
| [SERVER-WEB-KOREAN.md](SERVER-WEB-KOREAN.md) | 서버 웹 설정창(SIC) 한글 로케일 |
| [REVIEW-KOREAN-LOCALIZATION.md](REVIEW-KOREAN-LOCALIZATION.md) | **F12 밖의 한글화 범위 검토** — 모드 메뉴·퀘스트·이벤트·장비·대사. 서버가 실제로 내려주는 `kr` locale 61,125항목을 재어 **영문 산문 18,798항목 / 208만 자**를 확인했고, 퀘스트가 왜 1순위인지·아이템 설명은 왜 번역해도 안 보이는지·ISB 자막은 파일만 넣으면 되는지를 실측으로 적었다. 권하는 단계와 먼저 정할 것도 있다 |
| [HANDOVER-2026-11-06.md](HANDOVER-2026-11-06.md) | **2026-11-06 세션 인계 (최신)** — 10차 완료, F12 한글화 전수, 설치본 감사, **게임 DB 퀘스트 네 묶음 한글화 완료**, 남은 미확정 |
| [HANDOVER-2026-10-26.md](HANDOVER-2026-10-26.md) | 2026-10-26 세션 인계 |
| [../src/SptKoreanConfig/README.md](../src/SptKoreanConfig/README.md) | F12 설정 창 한글화 플러그인 |
| [../src/SptKoreanLocale/README.md](../src/SptKoreanLocale/README.md) | **게임 DB locale 한글화 서버 모드** — 지금 **사전 16개 / 8,019항목**이다(봇 역할 이름 22 · 퀘스트 이름 1,243 · 목표 3,482 · 설명 1,526 · 대사·메일 1,746). **사전에 값을 직접 쓰면 날아가는 실측**, `LazyLoad.AddTransformer` 로 가는 이유, **나중에 등록한 변환기가 이긴다**는 것(ExpandedTaskText 가 `int.MaxValue` 를 쓴다), 다시 펴낼 때 기준이 `en` 이어야 하는 이유, 묶음을 병렬로 나눠 번역할 때의 검사가 여기 있다 |
| [../src/PluginLocales/README.md](../src/PluginLocales/README.md) | **모드 폴더 안에 넣는 한국어 locale 4개 (338항목)** — ISB 무전·방송 자막 222줄, 보관함 정렬 창, 위시리스트 확장, 주변 아이템 패널. 네 모드가 각각 어떻게 locale 코드를 고르는지 역컴파일로 확인한 표와, **모드 업데이트에 날아가므로 다시 넣는** 절차 |
| [../src/GridWindowScroll/README.md](../src/GridWindowScroll/README.md) | 가방·상자 내부 인벤 창을 화면 안에 가두는 플러그인 (보이는 칸 수 제한 + 스크롤) |
| [../src/StackUnify/README.md](../src/StackUnify/README.md) | 최대 스택 크기를 한 곳에서 정하는 서버 모드. **`PostLoad` 단계가 클라이언트에 닿는다는 실측**도 여기 있다 |

직접 만든 플러그인 7개(`SptKoreanConfig`, `SptKoreanLocale`, `GridWindowScroll`, `StackUnify`,
`HelmetSlotUnion`, `SlotCategoryRelax`, `WebConfigBridge`)는 Forge 모드가 아니라 설치 기록에 없다. INSTALLED-MODS.md 의
"다른 경로로 설치된 것" 표를 본다.

## 기록 폴더

| 폴더 | 내용 |
| --- | --- |
| `install-records/` | 설치 단위별 파일 목록 `installed-<Tag>.txt` (`모드폴더<TAB>E:\Spt 기준 경로`). **되돌릴 때 이 목록만 삭제**. 정합성은 `node tools/check-install-records.mjs` 로 재고 `OK — 0/0` 이어야 한다 |
| `audit/` | 설치본 정합성 감사 보고 |
| `download/` | 단계별 `manifest-stage*.json`, 크기 검증 `download-results*.json`, 경로 충돌 검사 `conflict-check-stage*.txt` |
| `server-tests/` | 설치 직후 테스트 서버 출력 |
| `fixes/`, `mod-patches/`, `external-mods/` | 위 "사고와 수정 기록" 참조 |
| `stage2-survey/` | 차수별 Forge 조회 원자료 |
| `specs/`, `plans/` | 직접 만든 플러그인 설계 |

`installed-<Tag>.dirs.txt`가 있으면 그 단계에서 **빈 폴더**를 만들었다는 뜻이다(모드가 폴더
존재를 요구하는 경우). 되돌릴 때 함께 지운다.

충돌 검사 결과 파일은 **설치 전** 것이다. 설치 후 다시 돌리면 자기가 깐 파일을 "이미 있는
경로"로 잡으므로 재생성하지 않는다.

## 설치 단위 이름

| 단계 | Tag | 모드 |
| --- | --- | --- |
| 1차 | `batch1`~`batch4` | UI·탄약 정보·개조 화면 편의 23개 |
| 2차 | `stage2-A`, `stage2-C`, `stage2-B1~B3` | 편의 5, SVM, WTT CommonLib·Armory·Artem |
| 3차 | `stage3-T1~T3`, `stage3-L1~L4` | Skipper·Open Sesame·give-ui, Lotus·Scorpion·Ref·HarryHideout |
| 4차 | `stage4-U1`, `stage4-U2` | Kaeno-TraderScrolling, TraderSpace |
| 5차 | `stage5A-<id>` ~ `stage5E-<id>` | A UI 21, B 총기 9, C 탄약 6, D 상인 10, E 치트·도구 6 |
| 6차 | `stage6A-<id>`, `stage6B-<id>`, `stage6C-<id>`, `stage6D-<이름>` | 상인·파츠. GitHub·Nexus 모드는 id 대신 이름(`gh-`·`nexus-` 접두) |
| 7차 | `stage7A`~`stage7E-<id>` | 방어구·탄환·파츠·상인 (C 조는 결속 26개를 하나씩) |
| 8차 | `stage8A-<id>` ~ `stage8G-gamepanelhud` | OP·AI 동료·HUD. 자세한 것은 [STAGE8-RESULT.md](STAGE8-RESULT.md) |
| 9차 | (없음) | 생성기가 "절차 밖에서 들어온 것" 에 이미 써 버린 차수다. 설치 단위 이름으로는 쓰이지 않는다 |
| 10차 | `stage10-A` ~ `stage10-O-<id>-<이름>` | 모드 121개를 17단계로. 결속 단계(10-K·10-L)와 대형 맵(10-M·10-O)은 **하나씩** 넣고 단계마다 멈췄다. 자세한 것은 [STAGE10-RESULT.md](STAGE10-RESULT.md) |

## 백업 (git 제외, `../backup/`)

| 폴더 | 시점 |
| --- | --- |
| `clean-2026-10-24/` | 모드 설치 전 BepInEx·서버 설정·doorstop 원본 |
| `profiles-before-batch2`~`batch4` | 1차 각 묶음 직전 |
| `profiles-before-stage2A`·`stage2C`·`stage2B`·`stage3` | 2~3차 |
| `profiles-before-stage5A`~`stage5E` (+ 결속 모드별 `-<id>`) | 5차 |
| `profiles-before-stage6A`~`stage6D` (+ `-<id>`) | 6차 |
| `profiles-before-stage7A`~`stage7E` (+ `-<id>`) | 7차. C 조는 결속 26개를 모드별로 떴다 |
| `profiles-before-stage8-A`·`-C`·`-E`·`-F`, `stage8C-2906` | 8차 |
| `profiles-before-remove-miyako`, `profiles-before-ammo-fix` | Miyako 제거 직전, 프로필 탄약 수정 직전 |
| `profiles-before-stage9-recovered` | 절차 밖 6개(결속 3개 포함)가 들어오기 전. **우리가 뜬 것이 아니라 SPT 자체 백업(`2026-10-27_08-34-54`, 현지 17:34)에서 건져 온 것**이다. `activeMods.json` 에 모드 106개만 적혀 있어 설치 전임을 확인했다 |
| `firearmMags.json.orig`, `LRH-config.jsonc.orig` | 8차에서 고치기 전 모드 설정 원본 |
| `svm-presets/`, `allinweaponunslotted-config`, `painter-assort-original.json` 등 | 모드 설정 수정 전 사본 |

결속 모드를 플레이한 뒤 빼야 하면 해당 단계 **직전** 백업으로 프로필을 되돌려야 한다.
