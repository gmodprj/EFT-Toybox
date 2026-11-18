# 창고 5단계로 올린 프로필이 메인 메뉴에서 멈췄다 (2026-11-18)

## 증상

게임이 프로필을 고른 뒤 메인 메뉴를 다 띄우지 못하고 멈춘다. 백엔드 요청은 `/client/survey`(14:16:47) 뒤로 keepalive 뿐이다.

## 원인 (실측)

`E:\Spt\Logs\…\errors.log`:

```
There is no stage level 5 for area type Stash on client
KeyNotFoundException: The given key '5' was not present in the dictionary.
  at EFT.Hideout.AreaBehaviour.CalculateBonuses ()
  …
  at EFT.MainMenuShowOperation.Init ()
```

- **Hideout Overhaul 0.2.0**(서버 전용, `Gamemode.Areas = "hideout-overhaul"`)이 창고(Stash, type 3)에 **5단계**를 더한다 — 보너스 `StashSize` = Unheard 창고 `6602bcf19cc643f44a04274b`, 요구 유로·달러 각 150,000 + THICC 무기 상자 + THICC 아이템 상자 + Lucky Scav Junk box. SPT 기본 데이터는 0~4 단계뿐이다.
- 서버는 5단계를 보내지만, **게임 클라이언트의 은신처 창고 구역에는 5단계 자리가 없다.** 프로필이 5단계면 메뉴를 만드는 중에 예외가 나서 멈춘다.
- Nomad(EOD, 프레스티지 3)의 창고가 백업 기준 **11:23 에 4, 13:55 에 5** — 12:54 게임 세션에서 올렸다. 11개 프로필 중 5단계는 Nomad 하나였다(처음 밟은 것).

## 고친 것 (사용자 승인)

1. 게임·서버를 끄고 백업: `backup/stash5-revert-2026-11-18/`(프로필 + 모드 파일 원본, 커밋 안 됨).
2. Nomad 프로필(`6adc26693f5b5290609ea366.json`):
   - `Hideout.Areas[type 3].level` 5 → **4**
   - `Bonuses` 에서 `StashSize` `6602bcf1…` 하나 제거(181 → 180)
   - 창고 아이템 `_tpl` `6602bcf1…`(Unheard, 10×120) → **`5811ce77…`(EOD, 10×80)**
   - 4단계 격자(80줄) 밖에 놓인 물건은 **0개**였다(되돌리기 전에 셌다). 남은 `6602bcf1…` 참조 0.
3. `Arisu-sensei-HideoutOverhaul/Gamemodes/Areas/hideout-overhaul.jsonc` 에서 **창고 5단계 블록(1176~1258행)을 지웠다.** 서버 데이터의 창고 단계가 0~4 로 돌아왔다(재기동 후 `client/hideout/areas` 로 확인). 같은 폴더의 `hideout-overhaul.vanilla-timers.jsonc` 에는 아직 있다 — 그 게임모드로 바꾸면 다시 생긴다. **모드를 갱신해도 다시 생긴다.**
4. 업그레이드 비용을 ProfilePorter 로 **우편 반환**: 꾸러미 `ProfilePorter/porter/Nomad-창고5단계-비용반환-20261118.json`(유로 150,000 · 달러 150,000 · THICC 무기 상자 · THICC 아이템 상자 · Lucky Scav Junk box) → `/profile-porter/import` → `ok, count 5`. Nomad 우편함에 새 우편 1통(첨부 있음)으로 들어간 것을 확인했다.

## 확인

- 서버 재기동(14:3x) 후 `client/hideout/areas` 의 창고 단계 = `0,1,2,3,4`.
- 프로필 파일: 창고 레벨 4, 창고 템플릿 EOD.
- **게임 메뉴 로드는 사용자가 게임을 켜야 확인된다**(아래 남은 것).

## 남은 것

- 게임을 켜 Nomad 로 들어가 메뉴가 끝까지 뜨는지, `errors.log` 에 `stage level 5` 가 없는지, 우편으로 비용이 들어왔는지 본다.
