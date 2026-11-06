# 철 지난 설치 기록 26줄을 지웠다 (2026-11-06)

`doc/install-records/` 의 기록 줄 가운데 **설치본에 실제로 없는 26줄**을 지웠다.
`node tools/check-install-records.mjs` 가 0/0 이 되도록 하는 것이 목적이고,
**실제 모드 파일 유실은 0건**이다. 판단 근거는 [../fixes 아래가 아니라 audit 쪽](../audit/2026-11-06-install-audit.md)의
"정정 · 1-정정 · 2-정정" 에 적혀 있다.

지운 줄은 두 종류뿐이다. "없는 것은 다 지운다" 로 하지 않았다 — 그러면 앞으로 생길
진짜 유실이 이 청소에 섞여 사라진다.

## ① RaidReview 가 회전시킨 런타임 로그 (5줄)

설치 후 스캔을 뜰 때 그 자리에 있던 로그 파일이 기록에 들어갔고, 그 뒤 모드가 로그를 회전시켰다.
`data/logs/` 디렉터리 자체는 있고 더 새 로그가 들어 있다.

## ② TheQuartermaster 몫 (21줄 = 3경로 × 7모드)

2817 TheQuartermaster 의 파일이다. 근거 셋: ⓐ 그 7개 모드의 압축 해제본에 그 경로가 없다,
ⓑ 2817 자신의 제거 기록(`removed/installed-stage7D-2817.txt`, 15줄)에 그 3경로가 들어 있다,
ⓒ 7개 모드가 전부 같은 단계 7-D 에서 하나씩 설치됐다. Quartermaster 서버 모드가 기동마다
그 폴더를 다시 써서, 설치 후 스캔이 그것을 "새로 생긴 파일" 로 보아 그때 설치 중이던 모드 몫으로
적은 것이다. **2817 제거는 자기 파일을 가져갔고 7개 모드가 잃은 것은 없다.**

## 지운 줄 전부

| 기록 파일 | 모드 | 경로 | 분류 |
| --- | --- | --- | --- |
| `installed-stage10-J-1479-RaidReview.txt` | `1479__raid_review__1.10.0` | `SPT_Runtime\user\mods\RaidReview\data\logs\1793858840701_raid_review.log` | RaidReview 런타임 로그 |
| `installed-stage10-J-1479-RaidReview.txt` | `1479__raid_review__1.10.0` | `SPT_Runtime\user\mods\RaidReview\data\logs\1793858981523_raid_review.log` | RaidReview 런타임 로그 |
| `installed-stage10-J-1479-RaidReview.txt` | `1479__raid_review__1.10.0` | `SPT_Runtime\user\mods\RaidReview\data\logs\1793859145990_raid_review.log` | RaidReview 런타임 로그 |
| `installed-stage10-J-1479-RaidReview.txt` | `1479__raid_review__1.10.0` | `SPT_Runtime\user\mods\RaidReview\data\logs\1793859292470_raid_review.log` | RaidReview 런타임 로그 |
| `installed-stage10-J-1479-RaidReview.txt` | `1479__raid_review__1.10.0` | `SPT_Runtime\user\mods\RaidReview\data\logs\1793859446422_raid_review.log` | RaidReview 런타임 로그 |
| `installed-stage7D-1569.txt` | `1569__WolfiksHeavyTroopers1.2.0` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Images\quest.png` | TheQuartermaster 몫 |
| `installed-stage7D-1569.txt` | `1569__WolfiksHeavyTroopers1.2.0` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Locales\en.json` | TheQuartermaster 몫 |
| `installed-stage7D-1569.txt` | `1569__WolfiksHeavyTroopers1.2.0` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Quests\quest_definitions.json` | TheQuartermaster 몫 |
| `installed-stage7D-1591.txt` | `1591__SPTBetterRearSights` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Images\quest.png` | TheQuartermaster 몫 |
| `installed-stage7D-1591.txt` | `1591__SPTBetterRearSights` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Locales\en.json` | TheQuartermaster 몫 |
| `installed-stage7D-1591.txt` | `1591__SPTBetterRearSights` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Quests\quest_definitions.json` | TheQuartermaster 몫 |
| `installed-stage7D-2414.txt` | `2414__Peacemaker-2.1.0` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Images\quest.png` | TheQuartermaster 몫 |
| `installed-stage7D-2414.txt` | `2414__Peacemaker-2.1.0` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Locales\en.json` | TheQuartermaster 몫 |
| `installed-stage7D-2414.txt` | `2414__Peacemaker-2.1.0` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Quests\quest_definitions.json` | TheQuartermaster 몫 |
| `installed-stage7D-2516.txt` | `2516__22lr` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Images\quest.png` | TheQuartermaster 몫 |
| `installed-stage7D-2516.txt` | `2516__22lr` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Locales\en.json` | TheQuartermaster 몫 |
| `installed-stage7D-2516.txt` | `2516__22lr` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Quests\quest_definitions.json` | TheQuartermaster 몫 |
| `installed-stage7D-2681.txt` | `2681__LoadAmmoAnim-2.0.0` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Images\quest.png` | TheQuartermaster 몫 |
| `installed-stage7D-2681.txt` | `2681__LoadAmmoAnim-2.0.0` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Locales\en.json` | TheQuartermaster 몫 |
| `installed-stage7D-2681.txt` | `2681__LoadAmmoAnim-2.0.0` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Quests\quest_definitions.json` | TheQuartermaster 몫 |
| `installed-stage7D-2691.txt` | `2691__FoldThatStock-v2.1.0` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Images\quest.png` | TheQuartermaster 몫 |
| `installed-stage7D-2691.txt` | `2691__FoldThatStock-v2.1.0` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Locales\en.json` | TheQuartermaster 몫 |
| `installed-stage7D-2691.txt` | `2691__FoldThatStock-v2.1.0` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Quests\quest_definitions.json` | TheQuartermaster 몫 |
| `installed-stage7D-3113.txt` | `3113__IncendiaryGrenade` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Images\quest.png` | TheQuartermaster 몫 |
| `installed-stage7D-3113.txt` | `3113__IncendiaryGrenade` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Locales\en.json` | TheQuartermaster 몫 |
| `installed-stage7D-3113.txt` | `3113__IncendiaryGrenade` | `SPT_Runtime\user\mods\TheQuartermaster\db\CommunityQuests\66789abcde1234567890abcd\Quests\quest_definitions.json` | TheQuartermaster 몫 |

합계 **26줄**. 지운 뒤 `check-install-records.mjs` 가 `OK — 0/0` 이다.
