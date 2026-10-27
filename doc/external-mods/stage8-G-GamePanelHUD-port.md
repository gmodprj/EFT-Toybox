# 8-G — GamePanelHUD SPT 4.1.6 포팅본 (Forge 밖)

Forge 가 아닌 곳에서 받은 유일한 8차 모드다. 추적이 되도록 출처와 무결성 정보를 남긴다.

## 출처

| 항목 | 값 |
| --- | --- |
| 게시글 | https://arca.live/b/tarkovspt/185302366 |
| 작성자 | KIDOlogy (arca.live SPT 채널) |
| 게시일 | 2026-10-08 12:24 (수정 2026-10-09 21:31) |
| 버그 수정 공지 | https://arca.live/b/tarkovspt/185363870 — "스캐브 킬로그 안띄워주던 버그 수정" |
| 다운로드 | https://kio.ac/c/d5X6ebTrgAUQLZkqMLikeb (Kiosk File Sharing v4.beta) |
| 받은 일시 | 2026-10-27 15:57 (사용자가 직접 내려받음) |
| 파일명 | `GamePanelHUD_SPT_4.1.6_PORT.zip` |
| 크기 | 275,287 B (268.83 KiB) — 배포 페이지 표시와 일치 |
| SHA-256 | `07d8b2b0214efd45296b62077642628263e5d6eaa00203333325db81b28a0bc7` |
| 바이러스 검사 | Windows Defender `MpCmdRun -Scan -ScanType 3` → **위협 없음** (2026-10-27) |

같은 링크에서 다시 받을 때 위 해시와 다르면 내용이 바뀐 것이다.

## 원본과의 관계

| | Forge 최신 | 이 포팅본 |
| --- | --- | --- |
| [Game Panel HUD](https://sp-mod.com/mod/456) | 3.4.0 (SPT **4.0.x**, 2025-11-03) | DLL 버전 4.0.0.0 |
| [Kmy Tarkov Api](https://sp-mod.com/mod/898) | 1.5.0 (SPT **4.0.x**, 2025-11-03) | **2.0.0** (재빌드) |

**둘 다 4.1.x 대응이 Forge 에 없다.** 그래서 이 포팅본은 HUD 뿐 아니라 **선행 라이브러리까지
새로 빌드해 함께 배포**한다. 동봉된 `README_KO.txt`(사본은 이 폴더에 있다)가 그렇게 밝힌다.

> 포함: GamePanelHUD 4.1 포트 소스에서 빌드한 HUD DLL 7개 / 해당 HUD가 요구하는
> Kmy Tarkov API 2.0.0 DLL 3개 / 원본 HUD 번들 및 언어 파일
> 주의: SPT 4.1.6의 클라이언트 Managed/Core DLL을 참조해 컴파일했습니다.

DLL 버전 정보로 확인했다 — `KmyTarkovApi.dll` / `KmyTarkovReflection.dll` /
`KmyTarkovUtils.dll` 모두 **2.0.0.0**, HUD DLL 7개는 **4.0.0.0**.

예전에 보고된 무한 로딩·프리징의 원인으로 지목된 것이 **KmyTarkovApi 버전 불일치**인데,
README 3번 항목도 "기존 `kmyuhkyuk-KmyTarkovApi` 폴더가 있다면 반드시 덮어써서 예전 DLL 과
섞이지 않게 하라"고 적고 있다. **이 설치본에는 기존 KmyTarkovApi 가 없었으므로 섞일 여지가
없다** (설치 전 `BepInEx/plugins` 확인).

## 라이선스 — 짚고 넘어간 사항

원본 Game Panel HUD 는 **CC BY-NC-ND 4.0**(2차 저작물 금지)이고, 제작자는 공식 배포처를
Oddba 와 SPT 로 한정한다고 밝혀 두었다. 이 포팅본은 두 조건에 모두 어긋난다. 설치 전에
사용자에게 이 점을 알렸고, 사용자가 진행을 결정했다.

문제가 생겨도 원작자에게 문의할 수 없다. 포팅 작성자(KIDOlogy)의 게시글 댓글이 유일한
창구다.

## 설치

- 설치 기록: `doc/install-records/installed-stage8G-gamepanelhud.txt` (30개)
- 충돌 검사: 기존 경로 0, 모드 간 겹침 0 — `doc/download/conflict-check-stage8-G.txt`
- 서버 테스트: 모드 106개 그대로(게임 쪽 플러그인이라 정상), 오류 0, stderr 비어 있음
- `README_KO.txt` 는 압축 루트에 있어 그대로 두면 `E:\Spt\README_KO.txt` 로 들어간다.
  기능과 무관한 안내문이라 **설치에서 빼고** 이 폴더에 사본을 보관했다

구성:

```
BepInEx/plugins/kmyuhkyuk-GamePanelHUD/
  GamePanelHUDCompass / Grenade / Health / Hit / Kill / Weapon .dll
  core/GamePanelHUDCore.dll
  bundles/*.bundle 6개
  localized/{compass,core,grenade,health,hit,kill,weapon}/{en,zh}.json
BepInEx/plugins/kmyuhkyuk-KmyTarkovApi/
  KmyTarkovApi.dll / KmyTarkovReflection.dll / KmyTarkovUtils.dll
```

언어 파일은 **en·zh 뿐이다. 한국어는 없다.**

## 아직 확인하지 않은 것

- **게임에서 한 번도 띄워 보지 않았다.** 서버 테스트는 게임 쪽 플러그인의 동작을 보지 못한다.
  README 도 "레이드에서 각 HUD 기능의 실제 동작은 게임 실행으로 최종 확인해야 한다"고 적는다
- 무한 로딩이 나는지 — 이 포팅본의 알려진 증상이다. 나면 설치 기록대로 30개 파일을 지워
  되돌린다 (비결속이라 되돌리기는 안전하다)
- 설치된 `Pinned Quest HUD 1.0.1`, `Moe-TradeMarker 1.2.6`, 그리고 **Tushonka-800**(화면에
  스캔 틀을 그린다)과 표시가 겹치는지. 첫 확인은 Tushonka 를 F12 에서 끄고 하는 편이 가린다
- README 2번은 "BepInEx 폴더를 SPT_Runtime 폴더에 드래그"라고 적었는데, 이 설치본의 구조에서
  BepInEx 는 `E:\Spt\BepInEx` 다. 설치 스크립트는 압축 안의 상대경로대로 `E:\Spt\BepInEx\...`
  에 넣었으므로 결과는 맞다. README 쪽 표기가 어긋난 것으로 보인다
