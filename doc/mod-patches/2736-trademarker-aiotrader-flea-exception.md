# 2736 Moe-TradeMarker — AIOTrader 를 플리 재판매 금지에서 뺐다 (2026-11-14)

사용자가 **"AIOTrader만 예외"** 로 정했다. 조사 본문은
[2026-11-14-trademarker-flea-and-preset-ghost-model.md](../fixes/2026-11-14-trademarker-flea-and-preset-ghost-model.md) 다.

## 고친 것

`E:\Spt\SPT_Runtime\user\mods\Moe-TradeMarker\config.json` 한 곳. 전 파일은
[2736-trademarker-config-before-2026-11-14.json](2736-trademarker-config-before-2026-11-14.json) 로 떠 두었다.

```diff
   "ragfairRestriction": {
     "enabled": true,
-    "disabledTraderIds": []
+    "disabledTraderIds": [
+      "68f98298939080194f060927"
+    ]
   }
```

`68f98298939080194f060927` = **AIOTrader**(bluehead's AIO Trader, 모드 374).

## 왜 이 길인가

- 규칙 자체(`ragfairRestriction.enabled`)는 **켜 둔다** — 다른 상인에게서 산 물건을 플리에 되파는
  것은 그대로 막힌다. 실제 타르코프의 규칙이고 사용자가 그것을 원해서 넣은 모드다.
- `marking` 은 손대지 않았다 — **배지는 계속 붙는다**(어디서 산 물건인지 보인다). 표는
  프로필에 `upd.tradeMarker.traderId` 로 남는다.
- 서버는 **등록할 때** 표와 예외 목록을 함께 보므로, **이미 표가 박힌 969개도 함께 풀린다**
  (표를 지우거나 프로필을 고칠 필요가 없다). 서버 DLL 에 `ragfairRestriction`·
  `disabledTraderIds`·`tradeMarker`·`AddOffer` 가 모두 들어 있는 것을 확인했다.
- AIOTrader 만 뺀 까닭은 **그 상인이 게임 DB 전부를 팔기 때문**이다(`Assort ready: 10918
  listings`). 거기서 산 것과 그 부품으로 조립한 총까지 막혀 규칙이 "플리 판매 금지" 처럼 됐다.

## 적용

**서버 설정이라 서버를 다시 띄워야 든다.** 고친 시각에 게임과 서버가 돌고 있어 **끄지 않았다** —
사용자가 서버를 재시작한 뒤에 든다.

확인하는 법: AIOTrader 에서 산 물건 하나를 플리에 올려 본다. 붙어 있던
`BackendServerSideException: 이 아이템에는 상인 마커(AIOTrader)가 …` 가 안 나오면 든 것이다
(게임 로그 `E:\Spt\Logs\log_*\* errors.log` 에서도 같은 줄이 더 안 생긴다).

## 곁다리

이 파일은 `install-records/installed-stage6A-2736.txt` 에 적힌 설치 파일이다. 그래서
`check-installed-vs-extracted.mjs` 가 앞으로 이 경로를 **"원본과 다름(설정)"** 으로 적는다 —
정상이고, 모드를 업데이트할 때 **덮어쓰기 전에 떠 둘 파일**이라는 뜻이다.
