# All Quests Checkmarks (2025) — 제거됨 (2026-11-05)

10-E 에서 넣었다가 **같은 날 뺐다.** 게임 기동에서 새 오류 24줄이 전부 이 모드에서 나왔다 —
체크 표시 패치 실패 12건(`Failed to set custom checkmark in ItemSpecificationPanel!`)과
모드가 추가한 퀘스트를 자기 데이터에서 못 찾는 12건(`Attempted to remove non-existing quest`,
그 id 들은 실제로는 프로필에 있다). 한국어 locale 도 없어 영어로 떨어졌다(`Couldn't load locale 'kr'`).
자세한 것은 `doc/STAGE10-RESULT.md` 의 10-E 게임 기동 확인.

- 프로필에 심은 것이 없어 백업 복원은 필요하지 않았다.
- 기록에 적힌 8개를 지웠고, 남은 빈 폴더 3개와 런타임에 생긴
  `BepInEx/config/com.zgfuedkx.allquestscheckmarks.cfg` 도 함께 지웠다(이 설치본에는
  제거된 모드의 설정 파일이 남아 있지 않다).
- 이 설치본에는 퀘스트 UI 가 이미 여럿 있다 — Expanded Task Text, Quest Codex,
  Hj 의 Task Item/Turn-in Indicator, Task List Fixes, Hj's Task Search, Pinned Quest HUD.

## 설치 당시 파일 목록 (8줄, `모드폴더<TAB>상대경로`)

```
2025__allquestscheckmarks-1.4.0-spt-4.1	BepInEx\plugins\AllQuestsCheckmarks\AllQuestsCheckmarks-Client.dll
2025__allquestscheckmarks-1.4.0-spt-4.1	BepInEx\plugins\AllQuestsCheckmarks\AllQuestsCheckmarks-Fika.dll
2025__allquestscheckmarks-1.4.0-spt-4.1	BepInEx\plugins\AllQuestsCheckmarks\AllQuestsCheckmarksAssets
2025__allquestscheckmarks-1.4.0-spt-4.1	BepInEx\plugins\AllQuestsCheckmarks\locales\ch.json
2025__allquestscheckmarks-1.4.0-spt-4.1	BepInEx\plugins\AllQuestsCheckmarks\locales\en.json
2025__allquestscheckmarks-1.4.0-spt-4.1	BepInEx\plugins\AllQuestsCheckmarks\locales\pl.json
2025__allquestscheckmarks-1.4.0-spt-4.1	BepInEx\plugins\AllQuestsCheckmarks\locales\ru.json
2025__allquestscheckmarks-1.4.0-spt-4.1	SPT_Runtime\user\mods\AllQuestsCheckmarks\AllQuestsCheckmarks-Server.dll
```
