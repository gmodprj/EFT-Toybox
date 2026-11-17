// T Override(3317) PageUp 메뉴 사전 lang/_dll-toverride.json 을 만든다.
//
//   node tools/gen-toverride-menu.mjs          사전을 다시 쓴다
//
// 원문 목록 = 지금 사전의 원문(strings 열쇠) + check-dll-strings --list 가 찾은 "고른 메서드 안, 사전에 없는 영문"(KEEP 포함).
// 모드를 업데이트해 새 글자가 생기면 "번역 없음" 으로 나온다 — T 에 번역을, 열쇠·식별자·로그면 KEEP 에 더한다.
// F12 사전(gen-toverride-f12.mjs)을 먼저 만든다 — MenuSection 섹션 표를 그 사전에서 뽑는다.
import fs from 'node:fs';
import path from 'node:path';
import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';
const ROOT = path.join(path.dirname(fileURLToPath(import.meta.url)), '..');
const outPath = path.join(ROOT, 'src', 'SptKoreanConfig', 'lang', '_dll-toverride.json');
const f12Path = path.join(ROOT, 'src', 'SptKoreanConfig', 'lang', 'local.spt.practicemenu.json');
const literals = (() => {
  const set = new Set();
  if (fs.existsSync(outPath)) for (const k of Object.keys(JSON.parse(fs.readFileSync(outPath, 'utf8')).strings || {})) set.add(k);
  const listed = execFileSync(process.execPath, [path.join(ROOT, 'tools', 'check-dll-strings.mjs'), 'toverride', '--list'], { cwd: ROOT, encoding: 'utf8' });
  for (const l of listed.split(/\r?\n/).map(x => x.trim()).filter(x => x.startsWith('"'))) set.add(JSON.parse(l));
  return [...set];
})();

// 식별자·열쇠(바꾸면 기능이 깨진다)와 일부러 원문으로 둘 것
const KEEP = new Set([
  'Tarkov Override', 'Author: StealthOTR', '1.4.3 / COMPACT', 'OpenSettings', 'Category - ',
  'adsOnly', 'activation', 'switchSeconds', 'switchDelay', 'correctionAngle', 'correction', 'smoothing', 'maxTurn', 'bone', 'fallback',
  'visibility', 'aimRange',
  'ToggleInfiniteMedical', 'ToggleNoWeight', 'ToggleInteractionRange', 'ToggleFireRate', 'ToggleEnemyAlerts', 'ToggleAvailableExtractions',
  'RepairEquippedGear', 'AimHold', 'ToggleEnemyESP', 'ToggleLootESP', 'ToggleAimbot', 'Panic', 'ReloadConfig', 'HeadAimHold',
  'IgnoreTarget', 'ClearIgnoredTargets', 'ToggleNightVision', 'ToggleThermalVision', 'ToggleNoFlash', 'ToggleNoVisor', 'ToggleNoClip',
  'NoClip', 'Flight ', 'Killa', '[CORPSE] ',
  'Compact window disabled after UI error; F12 remains available: ',
  'NoClipForward', 'NoClipBack', 'NoClipRight', 'NoClipLeft', 'NoClipUp', 'NoClipDown', 'NoClipBoost',
  'No-clip', 'Unlock doors', 'Field Toolkit', 'Item catalog: ', 'Manual airdrop: ',
]);

// 원문 → 한글. 끝이 '…' 인 열쇠는 그 앞부분으로 시작하는 원문 하나에 맞춘다(긴 문장).
const T = {
  'SPT 4.1.6 | Aim: ': 'SPT 4.1.6 | 조준: ',
  'Off': '꺼짐',
  ' / Esc  Close': ' / Esc  닫기',
  // 열 제목
  'Enemy ESP': '적 ESP', 'Body ESP & preview': '시체 ESP·미리보기', 'Loot filters': '전리품 거르기', 'Highlights & clutter': '강조·정리',
  'Legit targeting': '레짓 대상', 'Gentle response': '부드러운 반응', 'Rage targeting': '레이지 대상', 'Fast response': '빠른 반응',
  'Practice toggles': '연습 기능', 'Recovery actions': '회복 동작', 'Vision & movement': '시야·이동', 'Doors & extraction': '문·탈출',
  'Item catalog': '아이템 목록', 'Spawn controls': '생성 조작', 'Hotkeys': '단축키', 'Movement keys & window': '이동 키·창',
  // 소제목
  'DIRECT HIT': '직접 명중', 'MOVEMENT & BULLET DROP': '움직임·탄 낙차', 'WEAPON STABILITY': '무기 안정', 'RCS / RECOIL CONTROL': '반동 제어',
  'DISPLAY STYLE & PRESET': '표시 방식·프리셋', 'ENEMY MARKERS & VISIBILITY': '적 표시·보임', 'TEXT': '글자', 'ENEMY TYPE FILTERS': '적 종류 거르기',
  'ENEMY COLOURS': '적 색', 'BODY ESP': '시체 ESP', 'BODY TYPE FILTERS': '시체 종류 거르기', 'BODY COLOURS': '시체 색', 'NEARBY ENEMY ALERTS': '근처 적 경고',
  'LIVE PREVIEW / SIMULATED': '미리보기 / 가상', 'ONE-SHOT ACTIONS': '한 번 하는 동작', 'STATUS': '상태', 'DOORS': '문', 'RAID': '레이드',
  'SEARCH CATALOG': '목록 찾기', 'SPAWN ITEM': '아이템 생성', 'SUPPLY DROP': '보급 투하',
  // 안내 문단
  'Body ESP is independently…': '시체 ESP 는 따로 켜고 끈다. 아군은 같은 그룹 ID 로 가린다. 보임 거르기는 살아 있는 적 ESP 에만 걸리고 조준·시체에는 걸리지 않는다.',
  'Quest highlights identify…': '퀘스트 강조는 미완료 찾기·넘기기 목표와 템플릿이 같은 것을 짚을 뿐, 목표의 모든 조건을 만족한다는 보장은 아니다.',
  'Rage Aim defaults to instant lock…': '레이지 조준은 기본이 즉시 고정·넓은 원뿔·긴 거리다. 즉시 고정을 끄면 회전 속도와 부드럽게를 쓴다.',
  'Legit Aim defaults to a small cone…': '레짓 조준은 기본이 작은 원뿔·느리고 부드러운 추적·보정 제한·대상 바꿈 지연이다.',
  '\n\nEnable a mode in its own tab…': '\n\n모드는 그 탭에서 켜야 작동한다. 한 번에 한 모드만 돈다. 다른 탭을 보기만 해서는 모드가 바뀌지 않는다. 보임 조건과 제외는 그대로 걸린다. 자동 사격은 없다.',
  'Strict Head forces Head…': '머리만 노리기는 저장된 노릴 부위와 상관없이 머리를 쓰고 몸으로 대신 노리지 않는다. 예측·낙차는 선택 사항이고 기본은 꺼짐이며 근사 계산이다. 고른 대상 표시는 보정이 앞을 노려도 실제 신체 지점에 남는다. 이 설정들은 사격을 막지 않는다.',
  'Practice toggles apply only…': '연습 기능은 레이드에서 살아 있는 내 플레이어에만 걸린다. 처음엔 꺼져 있고 플레이어·월드가 바뀌거나 레이드가 끝나면 꺼진다. 패닉이 끈다. 고장 막기는 새 고장을 막을 뿐 이미 걸린 걸림은 풀지 않는다.',
  "RCS uses this mode's sliders…": '반동 제어는 이 모드가 켜진 조준 모드일 때 이 모드의 막대 값을 쓴다. 대상을 잡지 않아도 된다. 0 이면 반동 그대로, 100 이면 고른 방향의 새 충격을 없앤다. 카메라 기울기와 관계없는 흔들림은 남는다.',
  'RCS hooks could not be loaded. Check the BepInEx log.': '반동 제어 훅을 불러오지 못했다. BepInEx 로그를 본다.',
  ' Enter #RRGGBB or #RRGGBBAA.': ' #RRGGBB 또는 #RRGGBBAA 로 적는다.',
  'Body ESP': '시체 ESP',
  // 도움말 (MenuHelp)
  'Aimbot only aims at visible targets': '조준 보조는 보이는 대상만 노린다',
  'Enable this aim mode…': '이 조준 모드를 켠다. 이 모드만 돌고 다른 모드는 멈춘다. 끄면 조준을 끈다.',
  'Only show living enemy markers within this distance, in metres.': '이 거리(미터) 안의 살아 있는 적 표시만 보인다.',
  'Only show loot markers within this distance, in metres.': '이 거리(미터) 안의 전리품 표시만 보인다.',
  'Aimbot only considers targets within this distance, in metres.': '조준 보조는 이 거리(미터) 안의 대상만 본다.',
  'Only show corpse markers within this distance, in metres.': '이 거리(미터) 안의 시체 표시만 보인다.',
  'Choose Head, Chest, Pelvis…': '머리·가슴·골반 또는 조준점에 가장 가까운 신체 지점 중에서 고른다.',
  'Time in seconds between loot scans…': '전리품을 훑는 간격(초). 여러 프레임에 나눠 훑는다. 짧을수록 CPU 를 더 쓴다.',
  'Maximum number of loose-loot labels…': '바닥 전리품 이름표의 최대 수. 퀘스트 아이템, 위시리스트 아이템, 나머지 순이고 각 묶음 안에서는 가까운 것부터.',
  // 단축키 탭
  'Save hotkeys to JSON': '단축키를 JSON 에 저장', 'Reload saved hotkeys': '저장된 단축키 다시 읽기',
  'Type key names, such as Mouse3…': 'Mouse3·Home·LeftControl+F6 처럼 키 이름을 적는다. None 이면 그 동작을 끈다.\n\n저장한 단축키는 같은 config.json 을 쓴다. 게임 설정은 바로 적용되고 이 창을 닫을 때 저장된다.',
  'Hotkey error: ': '단축키 오류: ',
  'Toggle infinite meds': '무한 의약품 켜고 끄기', 'Toggle weight bypass': '무게 무시 켜고 끄기', 'Toggle interaction range': '상호작용 거리 켜고 끄기',
  'Toggle fire rate': '연사 속도 켜고 끄기', 'Toggle nearby alerts': '근처 경고 켜고 끄기', 'Toggle available exits': '탈출구 열기 켜고 끄기',
  'Repair equipped gear': '착용 장비 수리', 'Open / close window': '창 열기·닫기', 'Hold to aim': '누르고 있으면 조준',
  'Toggle enemy ESP': '적 ESP 켜고 끄기', 'Toggle loot ESP': '전리품 ESP 켜고 끄기', 'Toggle aimbot': '조준 보조 켜고 끄기',
  'Disable all features': '모든 기능 끄기', 'Reload JSON': 'JSON 다시 읽기', 'Head aim override': '머리 조준 우선',
  'Ignore selected enemy': '고른 적 무시', 'Clear raid exclusions': '레이드 제외 지우기', 'Toggle night vision': '야간 투시 켜고 끄기',
  'Toggle thermal vision': '열상 켜고 끄기', 'Toggle no flash': '섬광 없애기 켜고 끄기', 'Toggle no visor': '바이저 효과 없애기 켜고 끄기',
  'Toggle no-clip': '벽 통과 비행 켜고 끄기',
  'Run the one-shot equipped-gear repair…': '레이드 중에 착용 장비 수리를 한 번 한다. "최대 내구도까지 복구" 설정을 따른다.',
  'Held movement binding while No-clip…': '벽 통과 비행 중에 누르고 있는 이동 키. LeftShift 같은 보조 키만 따로 쓸 수 있다. config.json 에 저장되고 키 잡기 창은 없다.',
  'Open or close this compact window…': '이 작은 창을 열고 닫는다. F12 는 여전히 BepInEx 설정을 연다.',
  'Used only in JSON hotkey activation mode.': 'JSON 단축키 작동 방식에서만 쓴다.',
  'Temporarily prefer the head while held…': '누르고 있는 동안 머리를 우선한다. 보임·대체 규칙은 그대로다.',
  'Exclude the tracked enemy…': '따라가는 적이나 조준점 5도 안에서 가장 가까운 적을 이번 레이드 동안 제외한다.',
  'Restore individually ignored enemies…': '하나씩 무시한 적을 되돌린다. "무시할 적" 글자는 지우지 않는다.',
  'Disable ESP, aimbot, Player and Misc toggles…': 'ESP·조준 보조·플레이어·기타 기능을 끄고 대기 중인 아이템 생성을 취소한다. 메뉴가 열려 있어도 된다.',
  'Reload the saved hotkey JSON file.': '저장된 단축키 JSON 파일을 다시 읽는다.',
  'Enable or disable the named feature…': '이름의 기능을 켜고 끈다. 이 단축키는 config.json 에 저장된다.',
  'Hotkeys saved to config.json.': '단축키를 config.json 에 저장했다.', 'Hotkeys not saved. See Keys.': '단축키를 저장하지 못했다. 키 탭을 본다.',
  // 미리보기
  'Behind cover': '엄폐 뒤', 'Visible': '보임', 'Preview: Enemy ESP': '미리보기: 적 ESP', 'Preview: Body ESP': '미리보기: 시체 ESP',
  'Simulated enemy marker': '가상 적 표시', 'Simulated body marker': '가상 시체 표시', 'This example is hidden by your filters.': '이 예시는 거르기 설정 때문에 숨었다.',
  // 회복·상태
  'Restore health': '체력 회복', 'Health restoration requested.': '체력 회복을 요청했다.',
  "Restore your local player's body-part health…": '게임의 완전 회복 방식으로 내 플레이어의 부위별 체력을 되돌린다. 죽은 플레이어를 살리거나 계속 무적으로 만들지는 않는다.',
  'Refill stamina once': '스태미나 한 번 채우기', 'Body and arm stamina refilled.': '몸·팔 스태미나를 채웠다.',
  'Refill body and arm stamina once…': '무한 스태미나를 켜지 않고 몸·팔 스태미나를 한 번 채운다.',
  'Repair equipped weapons, armour…': '착용한 무기·방어구·헬멧·끼운 방탄판을 고친다. 가방·리그 속 물건은 건너뛴다. "최대 내구도까지 복구"를 켜면 영구 최대 내구도 손실도 되돌린다.',
  'Enter a raid with a living player to use these controls.': '살아 있는 플레이어로 레이드에 들어가야 쓸 수 있다.',
  'Health: ': '체력: ', ' (Demigod)': ' (반신)',
  'Malfunction prevention could not be loaded. Check the BepInEx log.': '고장 막기를 불러오지 못했다. BepInEx 로그를 본다.',
  'Demigod save normalization could not be loaded…': '반신 저장 보정을 불러오지 못해 체력 올리기가 꺼졌다.',
  'One or more equipment/supply hooks are unavailable…': '장비·보급품 훅 일부를 쓸 수 없다. 그 조작은 꺼졌다 — BepInEx 로그를 본다.',
  // 문·탈출
  'Unlock nearest door (4m)': '가장 가까운 문 열기 (4m)',
  'Unlock the nearest locked normal door within four metres…': '4미터 안의 가장 가까운 잠긴 보통 문의 잠금을 푼다. 닫힌 채로 두니 평소처럼 연다.',
  'Unlock nearby doors': '근처 문 열기',
  'Unlock locked normal doors within the configured Door radius…': '"문 반경" 안의 잠긴 보통 문을 푼다. 스크립트 문이나 전원 스위치는 켜지 않는다.',
  'Extraction override hooks unavailable; check the BepInEx log.': '탈출 조건 무시 훅을 쓸 수 없다. BepInEx 로그를 본다.',
  'Instant extraction': '즉시 탈출', 'Confirm instant extraction': '즉시 탈출 확인',
  'End this raid immediately as Survived…': '지금 자리에서 게임의 보통 레이드 종료·저장 흐름으로 이 레이드를 생존으로 바로 끝낸다. 5초 안에 다시 눌러 확인한다.',
  'Click again within five seconds to finish this raid.': '5초 안에 다시 누르면 레이드를 끝낸다.',
  'Enter a raid with a living local player first.': '먼저 살아 있는 내 플레이어로 레이드에 들어간다.',
  'No-clip: use the movement bindings…': '벽 통과 비행: 키 탭 / config.json 의 이동 키를 쓴다. 끄기 전에 벽에서 나와 땅 높이 가까이로 돌아온다. 시야 모드는 하나만 켤 수 있다. 패닉이 새 기능을 모두 끈다.',
  // 아이템 생성
  'Search loaded item templates…': '불러온 아이템 템플릿을 현지화된 아이템 이름이나 템플릿 ID 로 찾는다. 월드 프리팹이 있는 모드 아이템도 들어간다. 결과는 100개까지 — 필요하면 검색어를 좁힌다.',
  'Indexing items… ': '아이템 색인 중… ', ' items indexed': '개 색인함', ' matches': '개 일치', ' (searching)': ' (찾는 중)',
  '\nTemplate: ': '\n템플릿: ', '\nMaximum stack: ': '\n최대 묶음: ',
  'Template ID (select a result or paste an ID):': '템플릿 ID (결과를 고르거나 ID 를 붙여 넣는다):', 'Quantity': '수량',
  'Total units to spawn…': '만들 총 개수. 탄약은 저절로 규칙에 맞는 묶음으로 나눈다. 한 번에 실물 묶음 100개까지.',
  'Found in raid': '레이드에서 찾음(FIR)', 'Mark newly spawned items as found in raid…': '새로 만든 아이템에 FIR 표시를 붙인다. 끄면 표시 없이 만든다.',
  'Spawn into inventory': '인벤토리에 만들기', 'Place each stack in eligible carried storage…': '묶음마다 들고 있는 보관 공간에 넣는다. 안 들어가는 것은 근처에 떨어진다.',
  'Spawn near player': '플레이어 근처에 만들기', 'Cancel remaining spawns': '남은 생성 취소',
  'Enter a raid with a living local player to spawn items.': '아이템을 만들려면 살아 있는 내 플레이어로 레이드에 들어간다.',
  'Enter a whole quantity between 1 and 100000.': '1 부터 100000 사이의 정수를 적는다.',
  'Choose a valid item template with a world prefab.': '월드 프리팹이 있는 올바른 아이템 템플릿을 고른다.',
  'That quantity would create more than 100 stacks. Lower the quantity.': '그 수량은 묶음 100개를 넘는다. 수량을 줄인다.',
  'Could not start spawning; see log.': '생성을 시작하지 못했다. 로그를 본다.',
  'Spawn stopped after ': '생성 멈춤 — 만든 수 ', ' units: ': '개: ', 'Finished: ': '끝: ', ' units spawned near your player.': '개를 플레이어 근처에 만들었다.',
  'Cancelled. ': '취소했다. ', ' units already spawned remain in the raid.': '개는 이미 만들어져 레이드에 남는다.',
  'Request airdrop': '보급 요청',
  // 상태 문구
  'Practice toggles reset. Enable them for this raid if needed.': '연습 기능을 초기화했다. 이번 레이드에 필요하면 다시 켠다.',
  'Stamina refill failed; see log.': '스태미나 채우기 실패 — 로그를 본다.', 'Action failed; see BepInEx log.': '동작 실패 — BepInEx 로그를 본다.',
  'Demigod disabled after an error; see log.': '오류가 나서 반신을 껐다 — 로그를 본다.', 'Invalid Demigod health multiplier.': '반신 체력 배율이 올바르지 않다.',
  'Demigod: ': '반신: ', 'x body-part health.': '배 부위 체력.',
  'Catalog indexing stopped; you can still paste a template ID. See log.': '목록 색인이 멈췄다. 템플릿 ID 를 붙여 넣는 것은 된다. 로그를 본다.',
  'Vision components are not ready on this camera.': '이 카메라에 시야 구성 요소가 아직 준비되지 않았다.',
  'No-clip input/gravity hooks unavailable. Check the log.': '벽 통과 비행 입력·중력 훅을 쓸 수 없다. 로그를 본다.',
  'Unlocked ': '잠금 해제 ', ' door(s).': '개 문.', 'No locked doors found in range.': '범위 안에 잠긴 문이 없다.',
  'No active local raid found; extraction was not requested.': '진행 중인 레이드가 없어 탈출을 요청하지 않았다.',
  'Extraction requested through the normal local-raid end flow.': '보통 레이드 종료 흐름으로 탈출을 요청했다.',
  'Request a normal plane-delivered supply drop on a supported map.': '지원하는 맵에서 비행기가 떨어뜨리는 보통 보급을 요청한다.',
  'The raid is ending.': '레이드가 끝나는 중이다.',
  'Native airdrops are unavailable or not initialized on this map.': '이 맵에서는 게임 보급을 쓸 수 없거나 아직 준비되지 않았다.',
  'No active airdrop points are available on this map.': '이 맵에 쓸 수 있는 보급 지점이 없다.',
  'Native airdrops are disabled for this raid.': '이 레이드에서는 게임 보급이 꺼져 있다.',
  'Another request is available in ': '다음 요청까지 ', ' seconds.': '초.',
  'Ready. The plane uses a map drop point, not your exact position.': '준비됨. 비행기는 내 위치가 아니라 맵의 투하 지점을 쓴다.',
  'Airdrop requested through the native plane system…': '게임의 비행기로 보급을 요청했다. 비행기 소리와 모습을 지켜본다 — 바로 오지 않는다.',
  'Airdrop request failed; see the BepInEx log.': '보급 요청 실패 — BepInEx 로그를 본다.',
  // 생성 코루틴 — "Spawned a / b units (c stacks)."·"Finished: a units in inventory, b dropped nearby."
  'Loading item assets…': '아이템 에셋 불러오는 중…', 'The game did not create a loot object.': '게임이 전리품 오브젝트를 만들지 않았다.',
  'Spawned ': '생성 ', ' units (': '개 (', ' stacks).': '묶음).', ' units in inventory, ': '개는 인벤토리에, ', ' dropped nearby.': '개는 근처에 떨어졌다.',
  'Request a real plane-delivered airdrop…': '게임의 투하 지점에 비행기가 실제로 떨어뜨리는 보급(SPT 기본 전리품)을 요청한다. 이 레이드의 보급 관리자가 켜져 있고 쓸 지점이 있을 때만 된다. 요청 사이 대기 60초.',
};

const strings = {};
const unknown = [];
for (const s of literals) {
  if (KEEP.has(s)) continue;
  if (T[s] !== undefined) { strings[s] = T[s]; continue; }
  const pre = Object.keys(T).find(k => k.endsWith('…') && s.startsWith(k.slice(0, -1)));
  if (pre) { strings[s] = T[pre]; continue; }
  unknown.push(s);
}
const usedKeys = new Set(Object.keys(T).filter(k => literals.some(s => s === k || (k.endsWith('…') && s.startsWith(k.slice(0, -1))))));
const orphan = Object.keys(T).filter(k => !usedKeys.has(k));

const resultNames = {
  // MenuNameFor 가 짧게 바꾼 이름 → 한글 (F12 사전 이름보다 먼저 쓴다 — 메뉴 칸이 좁다)
  'Visible Targets Only': '보이는 대상만', 'Type & distance': '종류·거리', 'Upper-body skeleton': '상체 뼈대', 'FOV radius (°)': '시야각 반지름 (°)',
  'Max turn speed': '최대 회전 속도', 'Switch delay (s)': '바꿈 지연 (초)', 'Correction angle': '보정 각도', 'Smoothing (s)': '부드럽게 (초)',
  'Bosses & other AI': '보스·기타 AI', 'Target scavs': '스캐브 노리기', 'Important-item bypass': '중요 아이템 거르기 무시',
  'Maximum distance (m)': '최대 거리 (m)', 'Label limit': '이름표 수', 'Distance detail': '거리별 세부', 'Hidden brightness': '가려진 밝기',
  'Colours by type': '종류별 색', 'Enemy detail range (m)': '적 세부 거리 (m)', 'Detail range (m)': '세부 거리 (m)', 'Detail focus (°)': '세부 주시 (°)',
  'Loot name range (m)': '전리품 이름 거리 (m)',
  // 드롭다운 값(열거형)
  'Left Arm': '왼팔', 'Right Arm': '오른팔', 'Left Leg': '왼다리', 'Right Leg': '오른다리', 'Head': '머리', 'Chest': '가슴', 'Thorax': '흉부', 'Stomach': '복부', 'Pelvis': '골반',
  'Nearest to crosshair': '조준점에 가장 가까운 곳', 'Right mouse held': '오른쪽 마우스 누름', 'JSON hotkey': 'JSON 단축키', 'Always': '항상',
  'All': '전체', 'Custom categories': '사용자 분류', 'Quest run': '퀘스트 진행', 'Wishlist': '위시리스트',
  'Ammo': '탄약', 'Valuables': '귀중품', 'Medical': '의약품', 'Attachments': '부착물', 'Keys': '열쇠', 'Weapons': '총기', 'Equipment': '장비', 'Provisions': '식량', 'Barter': '교환품', 'Other': '기타',
  'Legit': '레짓', 'Rage': '레이지', 'Crosshair': '조준점', 'Distance': '거리', 'LowestHealth': '체력 낮은 순',
  'Full box': '전체 상자', 'Corner brackets': '모서리 꺾쇠', 'Marker': '작은 표시', 'Simple': '간단', 'Detailed': '자세히', 'Custom': '사용자 지정', 'Full': '전부',
  'Near crosshair': '조준점 근처', 'Within distance': '거리 안', 'Crosshair or nearby': '조준점·근처',
};

const out = {
  comment: 'T Override(3317) 의 PageUp 메뉴. 열 제목·소제목·안내·버튼은 고른 메서드 안 리터럴(strings), 항목 이름은 MenuNameFor 결과(resultMaps — 짧은 이름·드롭다운 값은 strings, 나머지는 F12 사전 local.spt.practicemenu 의 이름), 도움말은 MenuHelp 결과(F12 사전 설명), 탭 이름·첫 안내는 이미 만들어진 플러그인 인스턴스 필드(instanceStrings). 설정 열쇠·단축키 필드 이름(OpenSettings·ToggleNoClip·bone 등)과 Category - 접두어는 열쇠로 쓰여 넣지 않는다. 생성: tools/gen-toverride-menu.mjs.',
  assembly: 'SPTPracticeMenu',
  methods: [
    'SPTPracticeMenu.Plugin::DrawCompactWindow', 'SPTPracticeMenu.Plugin::DrawCompactContents', 'SPTPracticeMenu.Plugin::DrawMenuColumn',
    'SPTPracticeMenu.Plugin::DrawMenuEntry', 'SPTPracticeMenu.Plugin::DrawMenuNumber', 'SPTPracticeMenu.Plugin::DrawKeysColumn',
    'SPTPracticeMenu.Plugin::DrawItemSpawning', 'SPTPracticeMenu.Plugin::DrawAirdropControls', 'SPTPracticeMenu.Plugin::DrawMiscActions',
    'SPTPracticeMenu.Plugin::DrawPlayerRecovery', 'SPTPracticeMenu.Plugin::DrawEspPreview', 'SPTPracticeMenu.Plugin::EspSectionTitle',
    'SPTPracticeMenu.Plugin::MenuColumnTitle', 'SPTPracticeMenu.Plugin::MenuHelp', 'SPTPracticeMenu.Plugin::KeyHelp', 'SPTPracticeMenu.Plugin::KeyLabel',
    'SPTPracticeMenu.Plugin::SaveKeysDraft', 'SPTPracticeMenu.Plugin::BeginItemSpawn', 'SPTPracticeMenu.Plugin::FinishSpawn', 'SPTPracticeMenu.Plugin::CancelItemSpawn',
    // 상태 문구 필드(airdropMessage·practiceMessage·miscMessage·spawnMessage)에 글자를 넣는 메서드
    'SPTPracticeMenu.Plugin::CanRequestAirdrop', 'SPTPracticeMenu.Plugin::RequestAirdrop', 'SPTPracticeMenu.Plugin::PracticeAction',
    'SPTPracticeMenu.Plugin::ExtractNow', 'SPTPracticeMenu.Plugin::UpdateDemigodHealth', 'SPTPracticeMenu.Plugin::UnlockDoors',
    'SPTPracticeMenu.Plugin::TickVision', 'SPTPracticeMenu.Plugin::TickPracticeTools', 'SPTPracticeMenu.Plugin::TickNoClip',
    'SPTPracticeMenu.Plugin::TickMiscTools', 'SPTPracticeMenu.Plugin::TickDemigodHealth', 'SPTPracticeMenu.Plugin::ResetPracticeTools',
    'SPTPracticeMenu.Plugin/<SpawnItemRoutine>*::MoveNext',
  ],
  strings,
  resultMaps: {
    'SPTPracticeMenu.Plugin::MenuNameFor': { strings: resultNames, configNames: 'local.spt.practicemenu' },
    'SPTPracticeMenu.Plugin::MenuHelp': { configHelp: 'local.spt.practicemenu' },
  },
  // 플레이어·기타 탭은 설정 섹션 이름에서 번호를 떼고 대문자로 만들어 MenuSection 에 넘긴다 → F12 사전의 섹션 이름(번호 뗌)
  argMaps: {
    'SPTPracticeMenu.Plugin::MenuSection': Object.fromEntries(
      Object.entries(JSON.parse(fs.readFileSync(f12Path, 'utf8')).sections)
        .map(([en, ko]) => [en.slice(3).toUpperCase(), ko.replace(/^\d+\s+/, '')])),
  },
  instanceFieldsOf: 'local.spt.practicemenu',
  instanceStrings: {
    'ESP': 'ESP', 'Loot': '전리품', 'Legit Aim': '레짓 조준', 'Rage Aim': '레이지 조준', 'Player': '플레이어', 'Misc': '기타', 'Spawning': '생성', 'Keys': '키',
    'Hover a feature name for help.': '기능 이름에 마우스를 올리면 도움말이 나온다.',
    'Enter a raid to use practice tools.': '레이드에 들어가야 연습 기능을 쓸 수 있다.',
    'Enter a raid to use these tools.': '레이드에 들어가야 이 기능들을 쓸 수 있다.',
    'Search for an item, select it, then spawn it during a raid.': '아이템을 찾아 고른 뒤 레이드 중에 만든다.',
    'Request a normal plane-delivered supply drop on a supported map.': '지원하는 맵에서 비행기가 떨어뜨리는 보통 보급을 요청한다.',
  },
};
delete out.instanceStrings.ESP; // 같은 글자는 넣지 않는다
fs.writeFileSync(outPath, JSON.stringify(out, null, 2) + '\n');
console.log('strings', Object.keys(strings).length, '| 원문 그대로', KEEP.size, '| 번역 없음', unknown.length, '| 안 맞는 번역키', orphan.length);
if (unknown.length) console.log('번역 없음:', unknown);
if (orphan.length) console.log('안 맞는 번역키:', orphan);
