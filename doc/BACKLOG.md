# 나중에 할 일

작업 중에 "이건 이번 범위가 아니다" 로 미룬 것만 적는다. 하기로 정한 것이지 후보 목록이 아니다.
끝내면 줄을 지우고 어디에 들어갔는지(커밋·문서) 한 줄 남긴다.

| 할 일 | 왜 미뤘나 | 손댈 자리 |
| --- | --- | --- |
| **ISB 역할 이름 10개의 한국어 번역** — `ScavRole/ISB`, `ISBBoss`, `ISBGuard`, `ISBScout`, `ISBSniper`, `ISBHeavy`, `ISBAssassin`, `ISBWhiteTusk`, `ISBCommander`, `ISBBossFollower`. 지금 ko 와 en 이 글자까지 같은 영문("ISB BOSS"·"WHITE TUSK"…)이라 킬 패널·시체 표시·지도 범주가 전부 영문으로 뜬다 | 2026-10-30 지도 세력 표시 작업의 범위 밖이었다(설계 §3 비목표). 번역은 ISB 모드의 locale 을 건드리는 일이라 결이 다르다 | ISB Special Forces 의 `db/CustomLocales/*.json` (ko 가 아예 없고 en 을 쓰고 있다). 모드 파일을 직접 고치면 업데이트에 날아가므로, 서버 모드 쪽에서 locale 에 덮어쓰는 방식을 검토한다. 실측은 [specs/2026-10-30-faction-identification-survey.md](specs/2026-10-30-faction-identification-survey.md) §3 |
