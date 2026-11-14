# 문서 색인

대상: `E:\Spt` (SPT 4.1.6, EFT 0.16.9.5.40743).
**2026-11-11 기준 모드 305개 / 설치 기록 15,948줄** + **직접 만든 것 20개**(`src/`, 테스트 프로젝트 제외. 그중 자기 이름의 설치 자리를 가진 것 16개 — 아래 "직접 만든 모드" 절). **서버 모드 174개**(11-11 설치 시험에서 실측), **게임 플러그인 248개**(2026-11-12 17:11 기동 로그에서 실측 — `248 plugins to load`. 11차에 넣은 `SPTVRAMCleaner`·`IcebreakerOffMapGuard` 둘이 들어와 246 → 248 이 됐다).

> **10차까지 끝났고, 11차(성능·메모리)가 11-A 하나 들어갔다.** 차수별 결과는 [STAGE10-RESULT.md](STAGE10-RESULT.md)·[STAGE11-RESULT.md](STAGE11-RESULT.md), 계획은 [PLAN-STAGE10.md](PLAN-STAGE10.md).
> 가장 최근 세션 인계는 [HANDOVER-2026-11-13.md](HANDOVER-2026-11-13.md)(**사용자가 준 다섯 갈래를 모두 닫았다** — F12 한글화 `5,155/5,155`, 총기 부품 끊긴 참조 0 과 인게임 부착 확인, 플리 수치 불변, 레벨 잠금은 **끈 채로 두기로 결정**, 제작 모드엔 창 모양 기억과 `꾸러미로 저장`. 그 과정에서 제보 **다섯**을 고쳤다 — 마지막은 **편집기와 교환창이 서로를 몰라** 저장이 거절되던 것이다). 그 앞은 [HANDOVER-2026-11-11.md](HANDOVER-2026-11-11.md)(**성능·메모리** 한 줄기 — 게임이 스스로 적는 값으로 한 기동에 2,367 → 6,696 MB 를 쟀고, 남의 리뷰 185건 중 아이스브레이커 보고를 우리 로그의 `[PerfCensus]` 로 교차검증해 **off-map 작업을 가리는 플러그인을 만들어 넣었다**. VRAM Cleaner 설치로 11차를 시작했다). 그 앞은 [HANDOVER-2026-11-10.md](HANDOVER-2026-11-10.md)(외부 모드 업데이트 20개를 모두 올리고 서버·게임·레이드에서 쟀다. 설정·번역이 날아갈 자리 넷을 막았고 아이템 이름 한글화를 시작했다. 이어 **설치본 46 GB 와 프로필을 떠서 구글 드라이브에 올리고**, 이 저장소를 **처음으로 GitHub 원격에 올렸다**). 그 앞은 [HANDOVER-2026-11-09.md](HANDOVER-2026-11-09.md)(ProfilePorter 완료 — 파일 지우기·세부 보기 창, 격자 자리 결함, 창을 스스로 찍게 만든 `--shot`).

> 8차까지가 절차를 거친 설치고, 그 뒤 **절차 밖에서 들어온 8개**는 2026-10-28 에 설치 기록을
> 뒤늦게 복원해 문서에 올렸다. 당시 기록이 아니라 "지금 가진 파일" 목록이다 —
> [REVIEW-LOAD-ORDER.md](REVIEW-LOAD-ORDER.md) 와 INSTALLED-MODS.md 의 "절차 밖에서 들어온 것"
> 절을 함께 본다.

## 먼저 볼 문서

| 문서 | 내용 |
| --- | --- |
| [MODS-GUIDE.md](MODS-GUIDE.md) | 설치된 모드가 무엇을 하는지 — 기능별 묶음, 직접 만든 것, 결속 목록, **알려진 충돌**, 지우면 안 되는 파일, 손볼 설정 |
| [REVIEW-MODS-AUDIT-2026-11-12.md](REVIEW-MODS-AUDIT-2026-11-12.md) | **네 갈래 점검(2026-11-12)** — 한글화 누락(퀘스트는 구멍 0, 남은 것은 아이템 이름 17,512항목), 총기 부품 로드(총기 1,038종·칸 6,846개 전수 — 약실 0결함·빈 `mod_launcher` 18칸은 바닐라, **끊긴 부품 참조 33회**는 모드가 만든 것이고 서버도 554줄로 적는다), 플리마켓을 덮어쓰는 모드 12+30개의 순서와 최종 상태(**TheBlacklist 가 UltimateFlea 보다 뒤에 돈다**), 레벨별 구매·판매 제한은 **UltimateFlea 가 이미 2,148개 잠그고 있다** |
| [REVIEW-FIVE-ASKS-2026-11-13.md](REVIEW-FIVE-ASKS-2026-11-13.md) | **다섯 갈래 조사(2026-11-13)** — 위 네 갈래를 하루 뒤에 **다시 재고** 다섯째(제작 모드)를 더했다. 퀘스트·플러그인 사전은 관문 통과, 총기 끊긴 참조는 **0회**로 유지, 플리마켓 수치는 그대로. **가격표는 화면마다 다르고**(QuickSell 401k ↔ ShowMeTheMoney 321k, 3단계에서 확인), **레벨 잠금은 꺼져 있다**(`levels.json` 세 손잡이가 `false`) — 11-12 의 "이미 잠겨 있다" 는 지금 사실이 아니고, **끈 채로 두기로 정했다**(2026-11-13, 사용자 결정 — 켜야 할 것으로 읽지 않는다). AirdropCaller 창의 잘리는 글자 넷과 ProfilePorter 메인 창에 화면 관문이 없는 것도 적었다 |
| [REVIEW-LOAD-ORDER.md](REVIEW-LOAD-ORDER.md) | **로드 순서와 기능 겹침 점검** — 순서를 바꿀 수 있는지, 어느 설정이 죽어 있는지. 실측 방법도 적었다 |
| [REVIEW-INTEGRATION-MOD.md](REVIEW-INTEGRATION-MOD.md) | **중복 기능을 통합 모드로 모을 수 있나** — 마지막에 로드되는 모드를 만들 수 있다는 것을 서버 코드로 확정하고(`OnLoadOrder`), 무엇까지 되고 무엇이 안 되는지와 권하는 범위를 적었다 |
| [INSTALLED-MODS.md](INSTALLED-MODS.md) | 지금 설치된 모드 전체 — 버전, 게임/서버 위치, 프로필 결속 여부, 설치 기록 (**자동 생성**, 직접 고치지 않는다) |
| [CHECK-2026-11-14.md](CHECK-2026-11-14.md) | **모드·문서 확인(2026-11-14)** — 검사 아홉을 돌린 결과. 설치 기록 `OK — 0/0`, F12 `5,155/5,155`, 우리 DLL 넷이 빌드와 같음. **업데이트 17개**가 밀려 있고, 922·3171 은 **제작자가 결속 표시를 뗐다**. 실측값을 갈았다(서버 175 / 플러그인 248, 늘어난 셋은 우리 모드). `check-installed-vs-extracted` 가 **옛 해제본과 견주던 것**을 고쳤고(1453 MSGA 거짓 경보), Forge 밖 4개도 손으로 봐 넷 다 최신임을 확인했다 |
| [UPDATE-CHECK-2026-11-09.md](UPDATE-CHECK-2026-11-09.md) | **외부 설치 모드 업데이트 점검** — 업데이트 있는 20개, 설치 후 수정된 것 가리기(md5 대조), 묶음별 업데이트 계획. 다시 재는 도구는 `check-mod-updates.mjs`·`check-installed-vs-extracted.mjs` |
| [EXPORT-2026-11-10.md](EXPORT-2026-11-10.md) | **설치본 추출·백업** — 설치된 모드와 설정을 통째로 떠서(16,622개 / 46 GB) 압축하고 구글 드라이브에 올린 기록. 담는 기준 두 길, 서버가 떠 있는 채로 뜰 때의 안전장치, 되돌리는 방법. 도구는 `tools/export/` |
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
| [REVIEWS-PERF-2026-11-11.md](REVIEWS-PERF-2026-11-11.md) | **설치 모드의 성능 관련 사용자 리뷰** — Forge 는 댓글 API 가 없어 Livewire 로 읽었다(모드 312개 / 댓글 5,077건 중 성능 언급 185건). 우리 로그로 교차검증된 Icebreaker `[PerfCensus]`·Interchange 프레임당 1,400회, 재현되지 않은 Hideout Cat, 이미 고쳐진 넷, 사라진 모드 둘 |
| [MEMORY-AUDIT-2026-11-11.md](MEMORY-AUDIT-2026-11-11.md) | **메모리 실측** — 게임이 스스로 적는 `totalMemoryBeforeCleanUp` 로 한 기동(3시간 17분)에 2,367 → 6,696 MB 를 쟀다. 의심했다가 아닌 것으로 밝혀진 넷(airdrop heartbeat·Moe-TradeMarker·Wishlist 캐시·ReflectSix), 남는 의심 셋, 모드를 더하지 않고 고칠 거리, 다음 기동에서 할 A/B |
| [CANDIDATES-PERF.md](CANDIDATES-PERF.md) | **성능·메모리 정리 후보 10개**(2026-11-11) — 4.1.6 호환 751개를 카테고리까지 훑어 추렸다. 이미 깔린 성능 모드 10종, 봇 쪽 셋의 겹침과 Smoothie×PiP Disabler 의 `✓½`, 4.1.6 버전이 없어 차단된 10개의 사유 |
| [STAGE11-RESULT.md](STAGE11-RESULT.md) | **11차(성능·메모리) 결과** — 11-A VRAM Cleaner 1.1.0. 받은 바이트·md5·겹침·서버 시험(216초, 174 그대로)·기록 검사. **2026-11-12 기동으로 재어졌다** — `248 plugins to load`·`VRAMCleaner loaded!` 둘 다 확인됐고, 기대했던 F12 항목은 `.cfg` 가 생기지 않아 **아예 없다**(사전에 더할 줄도 없다) |
| [STAGE10-RESULT.md](STAGE10-RESULT.md) | 10차 단계별 **실측 결과** — 기준선, 10-A·10-B, 설치 후 게임 기동 확인. 플러그인 수가 왜 +4 인지(라이브러리 DLL·GUID 충돌), F12 한글화 위험, 다음 단계 판정 기준에 더할 것 |
| [PLAN-STAGE10.md](PLAN-STAGE10.md) | 위 두 후보 문서를 **순차 설치·시험 계획**으로 묶은 것 — 설치 전 결정(10-0, 전부 끝남), 부착물 호환 단독 단계(10-A), 넣는 모드 121개를 17단계로. 단계마다 통과·멈춤 판정과 되돌리기를 적었다. 차수가 10인 이유는 생성기의 9차가 "절차 밖에서 들어온 것"에 이미 쓰여서다 |

## 사고와 수정 기록

| 문서 | 내용 |
| --- | --- |
| [fixes/2026-10-27-pit-stash-ammo.md](fixes/2026-10-27-pit-stash-ammo.md) | **봇 AI 정지 사건 전말** — 9x19 탄창의 5.56 탄 때문에 PIT 동료 생성이 깨지고, 반쯤 만들어진 동료 기록이 레이드를 망가뜨렸다. 내가 세운 가설 셋 중 둘이 틀린 경위도 적었다 |
| [fixes/2026-10-28-olympus-missing-item-ids.md](fixes/2026-10-28-olympus-missing-item-ids.md) | **서버 오류 48건의 전말** — Olympus 가 만들지 않는 아이템 id 48개를 참조한다. 증거·배제한 후보·제작자 보고용 영문 정리와 id 48개 전체 |
| [fixes/2026-10-27-pit-lootpatrol-nre.md](fixes/2026-10-27-pit-lootpatrol-nre.md) | PIT `LootPatrolLayer.GetDecision` NullReference 조사 + 제작자 보고용 영문 정리 |
| [fixes/2026-11-05-aicoreagent-invalidcast.md](fixes/2026-11-05-aicoreagent-invalidcast.md) | **BigBrain 봇 업데이트에서 매 프레임 InvalidCast 4,524줄** — STM 을 Release 로 다시 깐 뒤 0줄이 됐지만 그때 변수를 둘 바꿔 **원인은 미확정**이다. 진단용 finalizer 를 어떻게 짰는지도 적었다 |
| [fixes/2026-11-13-inventory-save-conservation-rollback.md](fixes/2026-11-13-inventory-save-conservation-rollback.md) | **저장이 디스크 확인에서 되돌려진다** — 12게이지 탄이 680 → 700 으로 늘어 보존 판정에 걸렸다. **아무것도 쓰이지 않았다.** 제출 단계는 제출본만, 디스크 단계는 제출본+보존된 것을 봐서 **둘이 다른 것을 보고 있었다** — 쓰기 전에 최종 상태로 한 번 더 보게 맞췄다. **20발이 어디서 났는지는 아직 모른다**(tpl 별 내역을 적게 해 뒀다) |
| [fixes/2026-11-13-stash-transfer-silent-drops.md](fixes/2026-11-13-stash-transfer-silent-drops.md) | **스태시 교환창에서 가방 안의 것을 끌면 아무 일도 안 일어난다** — 서버 로그에 전송 요청 자체가 없었다. 정책대로 거른 것인데(바닥에 놓인 것만 건넨다) **말해 주지 않아** 결함처럼 보였다. 이유를 상태줄에 적고 막게 고쳤다. **그 뒤 열었고(§4) 유령까지 잡았다(§6)** — 막고 있던 것은 판정 한 줄과 창의 관문 둘뿐이었다. 옮긴 것이 게임 화면에 남던 것은 복제가 아니라 **돌고 있는 게임에 알리지 않아서**였고, 보험이 쓰던 길로 밀어 넣게 고쳤다. **인게임에서 양방향 확인됐다** |
| [fixes/2026-11-13-stm-gear-editor-clears-session.md](fixes/2026-11-13-stm-gear-editor-clears-session.md) | **장비 편집기를 닫으면 화면의 세션이 함께 사라진다** — 장비 편집을 다녀온 뒤 스태시 교환이 격자가 아니라 **옛 목록형**으로 떴다. 범인은 덮개의 정리 코드가 화면의 것인 `ActiveProfileSession`·`ActiveProfileInventoryController` 까지 비우던 것이고, 로그가 분기를 적고 있어 바로 짚였다. 두 줄을 지워 고쳤다(컴파일 경고 0). **설치하고 인게임에서 닫았다**(11-13, 경고 4회 → 0회) |
| [fixes/2026-11-11-icebreaker-offmap-guard.md](fixes/2026-11-11-icebreaker-offmap-guard.md) | **아이스브레이커 백포트가 다른 맵에서도 하는 일 둘** — 60초마다 LOD 전수조사(`[PerfCensus]`, 우리 라이트하우스 58,529개)와 매 프레임 씬 이름 읽기. 설정으로는 못 끈다(`DevMode`·`DiagHotkeys` 꺼진 채로 돌았다). 밖에서 두 메서드만 가리는 플러그인 `src/IcebreakerOffMapGuard`. **첫 설치본(11-11)은 F12 열쇠의 홑따옴표 때문에 `Awake` 가 터져 아무것도 가리지 못했다**(11-12 17:11 기동에 `[PerfCensus]` 3회). 19:07 에 고쳐 다시 넣었고 **21:36 기동에서 걸린 것을 확인했다** — hideout 에서 census 0회(§6·§7) |
| [fixes/2026-11-06-lighthouse-rework-load-stall.md](fixes/2026-11-06-lighthouse-rework-load-stall.md) | **리워크 라이트하우스 진입 로딩 멈춤** — 멈춘 자리(심장박동 정지)와 두 번의 메모리 실측. 1차는 남은 4,466 MB 에서 멈추고 2차는 **72 MB 에서 47분을 돌아**, 메모리 여유로 설명하려던 가설을 거뒀다. **원인 미확정** |
| [fixes/2026-11-06-apply-inventory-accepts-overlap.md](fixes/2026-11-06-apply-inventory-accepts-overlap.md) | **`apply-inventory` 가 격자 겹침을 거절하지 않는다** — 실측과 되돌림(295개, 차이 0). 처음 "설계와 모순 아님" 으로 닫았다가 **계획 5 재작성에서 요건이 떨어져 나간 것**으로 정정한 경위도 적었다 |
| [fixes/2026-11-05-raid-end-crash-mono.md](fixes/2026-11-05-raid-end-crash-mono.md) | 레이드 종료 후 게임 강제 종료 — 시각별 확인 사실과 좁히는 방법. **원인 미확정** |
| [fixes/2026-10-30-pipdisabler-scope-misalign.md](fixes/2026-10-30-pipdisabler-scope-misalign.md) | PiP Disabler 에서 고배율 조준경 시야가 광축 아래로 떨어진다 — 끄면 정상. 지금은 `.cfg` 다섯 값이 기본값이 아닌 상태로 증상이 없어 **그대로 둔다** |
| [install-records/README-removed-stale-lines-2026-11-06.md](install-records/README-removed-stale-lines-2026-11-06.md) | 철 지난 설치 기록 **26줄을 지운 내역** — 줄 단위 표와 근거. 기록을 지우는 일이므로 지운 것을 남겼다 |
| [mod-patches/stackunify-pinned-stacks.md](mod-patches/stackunify-pinned-stacks.md) | 구경 43종의 최대 스택을 **지금 쓰이던 값으로 고정**했다 — 한 구경 안에서 값이 갈려 있던 15종을 포함. 넣은 값 전체와 되돌리는 법 |
| [mod-patches/theblacklist-restore-author-bans.md](mod-patches/theblacklist-restore-author-bans.md) | 콘텐츠 모드 제작자가 막아 둔 아이템 858건을 플레아에서 다시 막았다 — **244건이라던 숫자가 틀렸던 경위**와 아직 열려 있는 114건 |
| [mod-patches/svm-currency-off.md](mod-patches/svm-currency-off.md) | SVM 과 BarterItemsStacks 가 같은 화폐 스택을 쓰고 있었다 — SVM 쪽 네 줄을 껐다. **되돌리는 법과 Greed 로 저장할 때 주의** |
| [mod-patches/505-olympus-firearmMags.md](mod-patches/505-olympus-firearmMags.md) | Olympus 가 없는 총기를 참조해 탄창 적용이 중간에 멈추던 것 — 12개 제거. **되돌리는 법과 원본 보관 위치** |
| [mod-patches/505-olympus-firearmMags-mags.md](mod-patches/505-olympus-firearmMags-mags.md) | 위 문서의 후속 — 같은 파일에서 **없는 탄창 41개**를 뺐다 |
| [external-mods/stage8-G-GamePanelHUD-port.md](external-mods/stage8-G-GamePanelHUD-port.md) | Forge 밖에서 받은 유일한 모드 — 출처·해시·바이러스 검사·라이선스 검토 |

## SPT Team Manager (STM) — 직접 만드는 포크

PIT Fireteam 의 포크. 다른 런처 프로필을 팀원으로 쓴다. **이 묶음은 먼저 인계 문서를 읽고 시작한다.**

| 문서 | 내용 |
| --- | --- |
| [STM-HANDOVER.md](STM-HANDOVER.md) | **STM 인계 (먼저 읽어라)** — 지금 설치본, 반드시 지킬 불변식 11가지, 코드 지도, 열린 항목, 다음 사람이 집을 거리 |
| [specs/2026-10-31-spt-team-manager-design.md](specs/2026-10-31-spt-team-manager-design.md) | 설계 — 왜 "빼는 쪽만" 쓰는지, 프로필 팀원과 용병이 어디서 갈라지는지 |
| [specs/2026-11-04-stash-parity-checklist.md](specs/2026-11-04-stash-parity-checklist.md) | **대조표** — 스태시 조작을 칸으로 세어 "내 스태시=O, 팀원 편집기=X" 를 추린 표. 미측정 칸이 없고, 남은 X 는 전부 "설계상 닫는다" 다 |
| [specs/2026-11-04-stash-operations-survey.md](specs/2026-11-04-stash-operations-survey.md) | 그 배경 조사 — 컨트롤러를 바꾸면 무엇이 서버로 나가는지(불변식 11 의 근거) |
| [specs/2026-11-04-death-strip-gate.md](specs/2026-11-04-death-strip-gate.md) | 사망 시 장비 삭제 관문 |
| [fixes/2026-11-05-aicoreagent-invalidcast.md](fixes/2026-11-05-aicoreagent-invalidcast.md) | Release 로 다시 깐 뒤 사라진 매 프레임 InvalidCast — **원인은 미확정** |

## 그 밖

| 문서 | 내용 |
| --- | --- |
| [SERVER-WEB-KOREAN.md](SERVER-WEB-KOREAN.md) | 서버 웹 설정창(SIC) 한글 로케일 |
| [REVIEW-KOREAN-LOCALIZATION.md](REVIEW-KOREAN-LOCALIZATION.md) | **F12 밖의 한글화 범위 검토** — 모드 메뉴·퀘스트·이벤트·장비·대사. 서버가 실제로 내려주는 `kr` locale 61,125항목을 재어 **영문 산문 18,798항목 / 208만 자**를 확인했고, 퀘스트가 왜 1순위인지·아이템 설명은 왜 번역해도 안 보이는지·ISB 자막은 파일만 넣으면 되는지를 실측으로 적었다. 권하는 단계와 먼저 정할 것도 있다 |
| [HANDOVER-2026-11-13.md](HANDOVER-2026-11-13.md) | **2026-11-13 세션 인계 (최신)** — 사용자가 준 다섯 갈래(한글화·총기 부품·플리 충돌·레벨 제한·제작 모드)를 **모두 닫고**, 그 과정에서 나온 제보 넷을 고쳤다. F12 `5,155/5,155`, 가방 안의 것도 스태시 교환으로 건너가고, 편집기에 `꾸러미로 저장` 이 생겼다. **탄 20발의 출처**(스태시 교환)까지 밝혀 고쳤고, 남은 것은 그 **인게임 확인** 하나 |
| [HANDOVER-2026-11-11.md](HANDOVER-2026-11-11.md) | **2026-11-11 세션 인계** — 성능·메모리. 후보 조사(751개 전수 훑기) → 메모리 실측(게임이 적는 값으로 2,367 → 6,696 MB) → VRAM Cleaner 설치(11-A) → 남의 리뷰 185건 교차검증 → 아이스브레이커 off-map 작업을 가리는 플러그인 작성·설치. 값을 치르고 배운 것 여섯, 다음에 집을 거리 여덟 |
| [HANDOVER-2026-11-10.md](HANDOVER-2026-11-10.md) | **2026-11-10 세션 인계** — 외부 모드 업데이트 20개(U-A~U-H)를 모두 올렸다. 업데이트가 설정·번역을 날리는 자리 넷, 한글 사전이 다른 모드의 꾸밈을 지우던 결함, 더한 도구 다섯, 값을 치르고 배운 것 여섯 |
| [HANDOVER-2026-11-09.md](HANDOVER-2026-11-09.md) | 2026-11-09 세션 인계 — ProfilePorter 완료. 내보낸 파일이 격자 자리를 통째로 잃던 결함(§20·§21)과 다시 보내기, 파일 지우기·세부 보기 창(설계·계획 2026-11-09), 그리고 **창을 스스로 찍게 만든 `--shot`** 으로 화면 여섯 군데를 고쳤다 |
| [HANDOVER-2026-11-08-2.md](HANDOVER-2026-11-08-2.md) | 2026-11-08 두 번째 세션 인계 — 요청 3(에어드랍 격자) 검증 완료, ProfilePorter 설계부터 P4(창)까지, BTR 대화 한글화 |
| [HANDOVER-2026-11-08.md](HANDOVER-2026-11-08.md) | 2026-11-08 첫 세션 인계 — A5-4 레이드 적립 실측, 요청 2~4 정리 |
| [HANDOVER-2026-11-07-2.md](HANDOVER-2026-11-07-2.md) | **2026-11-07 두 번째 세션 인계** — 요청 1(전적 적립) 인게임까지 완료, 요청 2(스태시 교환) 격자 화면과 끌어다 놓기, ISB 자막·명단 줄 수·버튼 글씨 고침, 설치 검사 도구 |
| [HANDOVER-2026-11-07.md](HANDOVER-2026-11-07.md) | 2026-11-07 첫 세션 인계 — 저널 복구 경로 실측, Airdrop 프리셋 전역화와 장비 뽑기, 묵은 문서 아홉 곳 정정. 설치본은 그대로 |
| [HANDOVER-2026-11-06.md](HANDOVER-2026-11-06.md) | 2026-11-06 세션 인계 — 10차 완료, F12 한글화 전수, 설치본 감사, **게임 DB 퀘스트 네 묶음 한글화 완료**, 남은 미확정 |
| [HANDOVER-2026-10-26.md](HANDOVER-2026-10-26.md) | 2026-10-26 세션 인계 |
| [../src/SptKoreanConfig/README.md](../src/SptKoreanConfig/README.md) | F12 설정 창 한글화 플러그인 |
| [../src/SptKoreanLocale/README.md](../src/SptKoreanLocale/README.md) | **게임 DB locale 한글화 서버 모드** — 지금 **사전 19개 / 8,468항목**이다(2026-11-12 실측 — 봇 역할 이름 23 · BTR 대화 44 · 아이템 이름 406(바닐라 28 · ISB 계열 378) · 퀘스트 이름 1,243 · 목표 3,482 · 설명 1,526 · 대사·메일 1,744). **사전에 값을 직접 쓰면 날아가는 실측**, `LazyLoad.AddTransformer` 로 가는 이유, **나중에 등록한 변환기가 이긴다**는 것(ExpandedTaskText 가 `int.MaxValue` 를 쓴다), 다시 펴낼 때 기준이 `en` 이어야 하는 이유, 묶음을 병렬로 나눠 번역할 때의 검사가 여기 있다 |
| [../src/PluginLocales/README.md](../src/PluginLocales/README.md) | **모드 폴더 안에 넣는 한국어 locale 4개 (338항목)** — ISB 무전·방송 자막 222줄, 보관함 정렬 창, 위시리스트 확장, 주변 아이템 패널. 네 모드가 각각 어떻게 locale 코드를 고르는지 역컴파일로 확인한 표와, **모드 업데이트에 날아가므로 다시 넣는** 절차 |
| [../src/GridWindowScroll/README.md](../src/GridWindowScroll/README.md) | 가방·상자 내부 인벤 창을 화면 안에 가두는 플러그인 (보이는 칸 수 제한 + 스크롤) |
| [../src/StackUnify/README.md](../src/StackUnify/README.md) | 최대 스택 크기를 한 곳에서 정하는 서버 모드. **`PostLoad` 단계가 클라이언트에 닿는다는 실측**도 여기 있다 |

직접 만든 것은 **Forge 모드가 아니라 설치 기록에 없다.** INSTALLED-MODS.md 의 "다른 경로로 설치된 것"
표를 본다. 목록과 설계 문서는 아래 "직접 만든 모드" 절에 있다.

## 직접 만든 모드 (`src/`)

**20개**(테스트 프로젝트 제외). 그중 **자기 이름의 설치 자리를 가진 것이 16개**다 —
2026-11-12 실측으로 `BepInEx\plugins\` 에 8개(AirdropCaller · AirdropProbe ·
DynamicMapsFactionMarkers · GogglesWheel · GridWindowScroll · IcebreakerOffMapGuard ·
SptKoreanConfig · SptTeamManager), `user\mods\` 에 10개(AirdropCaller · HelmetSlotUnion ·
ProfilePorter · ScavRaidWaves · SlotCategoryRelax · SptKoreanLocale ·
SptTeamManager-ServerMod · SlotFilterSweep · StackUnify · WebConfigBridge)이고, AirdropCaller 와 STM 은
양쪽에 걸쳐 있어 모드로는 16개다. `AirdropCaller.exe`·`ProfilePorter.exe` 는 설치본 루트에 선다.
`PluginLocales`·`ServerWebKorean` 은 자기 폴더 없이 **남의 모드 폴더와 서버 웹에 파일로** 들어가고,
`CompatToggle` 은 지금 설치본에 없다.

| 모드 | 무엇 | 설계·계획 |
| --- | --- | --- |
| **SptTeamManager (STM)** | PIT Fireteam 포크. 다른 런처 프로필을 팀원으로 | [인계](STM-HANDOVER.md) · [설계](specs/2026-10-31-spt-team-manager-design.md) · 계획 [1](plans/2026-10-31-spt-team-manager-plan-1-foundation.md)·[2](plans/2026-11-01-spt-team-manager-plan-2-raid.md)·[3](plans/2026-11-02-spt-team-manager-plan-3-gear-editing.md)·[4](plans/2026-11-02-stm-plan4-kit-loadouts.md)·[5](plans/2026-11-03-stm-plan5-gear-parity.md)·[6](plans/2026-11-06-stm-plan6-accrual-and-stash-transfer.md) · [대조표](specs/2026-11-04-stash-parity-checklist.md) · [조사](specs/2026-11-04-stash-operations-survey.md) · [사망 관문](specs/2026-11-04-death-strip-gate.md) · [편집기 여섯](specs/2026-11-06-stm-six-checks.md) · [전적 대응표](specs/2026-11-07-stm-raid-tally-mapping.md) · [제거 조사](specs/2026-11-04-allinweapon-removal-survey.md) |
| **AirdropCaller** | 외부 창으로 보급을 부른다(`.Server`+`.Plugin`+`.App` 세 조각) | [설계](specs/2026-10-28-airdrop-caller-design.md) · [1단계](specs/2026-10-29-airdrop-stage1-findings.md) · [2단계](specs/2026-10-29-airdrop-stage2-result.md) |
| `AirdropProbe` | 보급 지점 측정기. AirdropCaller 1단계용 | 위 설계 §7.9 |
| **ProfilePorter** | 프로필 사이로 물건을 베낀다 — 격자로 골라 우편으로 보낸다(`.Server`+`.App` 두 조각) | [설계](specs/2026-11-08-profile-porter-design.md) · [인계](HANDOVER-2026-11-09.md) |
| `SptKoreanConfig` | F12 설정 창 한글화 | [README](../src/SptKoreanConfig/README.md) |
| `SptKoreanLocale` | 게임 DB locale 한글화 | [README](../src/SptKoreanLocale/README.md) · [범위 검토](REVIEW-KOREAN-LOCALIZATION.md) · [아이템 범위](specs/2026-11-06-item-locale-scope.md) · [계획](plans/2026-11-02-korean-localization-plan.md) |
| `PluginLocales` | 모드 폴더에 넣는 한국어 locale 4개 | [README](../src/PluginLocales/README.md) |
| `ServerWebKorean` | 서버 웹 설정창 한글 locale | [문서](SERVER-WEB-KOREAN.md) |
| `IcebreakerOffMapGuard` | 아이스브레이커 백포트가 **다른 맵에서도** 하는 일 둘을 밖에서 가린다(LOD 전수조사·매 프레임 씬 이름 읽기) | [경위·설계](fixes/2026-11-11-icebreaker-offmap-guard.md) |
| `GogglesWheel` | 야시경·실드 휠 | [설계](specs/2026-10-30-goggles-wheel-design.md) · [계획](plans/2026-10-30-goggles-wheel-plan.md) |
| `DynamicMapsFactionMarkers` | 지도에 세력 표시 | [설계](specs/2026-10-30-dynamicmaps-faction-markers-design.md) · [계획](plans/2026-10-30-dynamicmaps-faction-markers-plan.md) · [세력 조사](specs/2026-10-30-faction-identification-survey.md) |
| `GridWindowScroll` | 가방·상자 창을 화면 안에 가둔다 | [README](../src/GridWindowScroll/README.md) |
| `StackUnify` | 최대 스택을 한 곳에서 정한다 | [README](../src/StackUnify/README.md) |
| `HelmetSlotUnion`·`SlotCategoryRelax` | 슬롯 필터를 넓힌다(STM 편집기가 기댄다) | STM 인계 불변식 9 |
| `SlotFilterSweep` | 칸 필터에서 **없는 부품 id** 를 걷어낸다(기본은 세기만) | [감사](REVIEW-MODS-AUDIT-2026-11-12.md) |
| `ScavRaidWaves` | 스캐브 레이드 웨이브 | — |
| `WebConfigBridge` | 서버 웹 설정 다리 | — |
| `CompatToggle` | 호환 토글(**지금 설치본에 없다**) | [설계](specs/2026-10-26-compat-toggle-design.md) · [계획](plans/2026-10-26-compat-toggle-plan.md) |

그 밖의 조사 문서: [F12 메뉴 검토](specs/2026-10-26-f12-menu-review.md) ·
[모드 페이지 후보](specs/2026-10-26-modpage-candidates.md).

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
