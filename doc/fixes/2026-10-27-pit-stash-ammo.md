# PIT Fireteam 동료 생성 실패 — 스태시의 구경 불일치 탄약

2026-10-27. 동료를 생성하면 오류가 나고, 그 상태로 레이드에 들어가면 **모든 적과 동료의 AI가 멈추는** 문제.

## 증상

사용자 관찰:

- 동료 생성 → 파티 → 투입 → 봇 전부 정지. 레이드 종료 후 다시 파티를 짜면 정상
- 동료를 새로 만들면 **무조건** 재발
- 동료 3까지는 명단에 보이는데 4, 5는 만들어도 안 보임
- 생성 버튼에서 오류 메시지가 뜸
- **단독 레이드는 정상**
- PIT 문서가 권한 `Raid Settings > Faction Hostilities` 끄기와 **무관**

## 원인

PIT 는 동료 생성 때 플레이어 스태시 전체를 서버에 저장했다가 다시 읽어들이며 EFT 의 필터로
재검증한다. 거기서 걸렸다.

```
[Warning] Server-saved stash item deserialization detail:
No filter for item Pro+ (id: 6adea532010015d86e5ffb7e)
in StackSlot cartridges in item mag_glock_glock_big_stick_9x19_33
[Error] Teammate confirmation failed:
System.InvalidOperationException: Player stash refresh failed.
  at pitTeam.Patches.OtherPlayerProfileScreenPatch.ApplyServerSavedPlayerStashSnapshot
  at pitTeam.Modules.TeammateHiringPreview.ConfirmAsync
```

프로필에 **9x19 글록 33발 탄창 안에 5.56x45 소총탄 33발**이 장전돼 있었다.

```
_tpl 6937dbee0dd9015d14925d11  = Amonya "5.56x45 +"
부모 mag_glock_glock_big_stick_9x19_33 (9x19 권총 탄창), slotId cartridges, 33발
```

어떻게 들어갔나 — `All In Weapon: Unslotted` 2576 의 설정 때문이다. 기본값은 전부 `true`이고
`FckMagazines`(탄창에 아무 탄이나)가 켜져 있으면 이런 조합이 만들어진다. 현재 설정은
`FckMagazines: false`라 필터가 다시 좁아졌고, **규칙에 맞지 않는 항목만 스태시에 남았다.**

게임 본체는 이미 있는 아이템을 다시 검사하지 않아 멀쩡히 돌아간다. 스태시를 통째로 재검증하는
PIT 만 걸린다. 생성이 중간에 끊기니 명단에 안 들어가고(4, 5 가 안 보이는 이유), 내부 상태가
깨진 채 레이드에 들어가 봇이 멈춘다. 오류 메시지 자체가 "Restart the game before making more
inventory changes" 라고 말한다 — 레이드 종료·재시작으로 풀리는 이유다.

## 조치

프로필에서 해당 항목 1개만 제거했다. 자식 항목은 없었다.

- 백업: `backup/profiles-before-ammo-fix/` (31개)
- 제거: `characters.pmc.Inventory.items` 901 → 900
- 제거 후 서버 테스트: 모드 102개, 오류 0

## 오탐이었던 것

프로필 전체를 훑어 처음 6건이 걸렸으나 실제 문제는 1건뿐이었다.

- 5.56 D-60 탄창의 `5.56x45 +` 4건 — 구경이 맞다. 바닐라 DB 필터에 없을 뿐 서버 시작 때 추가된다
- MK-18 `.338 AVK` 1건 — `86x70` 은 .338 라푸아 매그넘이다. 구경 비교 로직의 오판이었다

## 앞선 오진

이 고장을 처음에는 **BigBrain 1.5.0 / Waypoints 1.9.0 이 SPT 4.1.6 보다 6주 먼저 빌드됐다**는
점을 들어 "커스텀 레이어를 쓰는 모드가 4.1.6 에서 깨진다"고 판단했다. 빌드 날짜는 사실이지만
원인이 아니었다. Miyako 와 PIT 가 연달아 깨진 공통점은 BigBrain 이 아니라 **둘 다 스태시를
다룬다**는 쪽이었다. Miyako 도 장비 프리셋·인벤토리 기능이 있어 같은 항목에 걸렸을 가능성이
높다 — 즉 Miyako 제거는 불필요했을 수 있다.

교훈: 두 모드가 같은 증상을 내면 공통 "기반"보다 공통 "동작"을 먼저 본다. 그리고 로그의
Error 줄만 보지 말고 바로 앞의 Warning 을 읽는다. 원인은 Warning 쪽에 적혀 있었다.
