// T Override(3317, local.spt.practicemenu) 의 F12 사전 lang/local.spt.practicemenu.json 을 만든다.
//
//   node tools/gen-toverride-f12.mjs            설치본 .cfg 를 읽어 사전을 다시 쓴다
//
// 조준 보조·레짓·레이지 세 묶음이 같은 키를 되풀이하므로 키별 번역(byKey)에 섹션별 예외(bySec)를 얹는다.
// 모드를 업데이트해 항목이 늘면 "빠짐" 으로 나온다 — 그 키를 byKey/bySec 에 더한다.
import fs from 'node:fs';
const CFG = process.argv[2] || 'E:/Spt/BepInEx/config/local.spt.practicemenu.cfg';
const outPath = process.argv[3] || new URL('../src/SptKoreanConfig/lang/local.spt.practicemenu.json', import.meta.url);
const entries = [];
{
  let sec = '', desc = [], type = '';
  for (const l of fs.readFileSync(CFG, 'utf8').split(/\r?\n/)) {
    if (/^\[.*\]$/.test(l)) { sec = l.slice(1, -1); continue; }
    if (l.startsWith('## ')) { desc.push(l.slice(3)); continue; }
    if (l.startsWith('# Setting type:')) { type = l.slice(16).trim(); continue; }
    if (l.startsWith('#') || !l.trim()) continue;
    const m = l.match(/^(.*?) = ?(.*)$/);
    if (m) { entries.push({ id: sec + '/' + m[1], section: sec, key: m[1], desc: desc.join(' '), type }); desc = []; type = ''; }
  }
}

const sections = {
  '01 Enemy ESP': '01 적 ESP', '02 Loot ESP': '02 전리품 ESP', '03 Aimbot': '03 조준 보조', '04 Display': '04 표시',
  '05 Hotkeys': '05 단축키', '06 Loot filters and highlights': '06 전리품 거르기·강조', '07 Corpse markers': '07 시체 표시',
  '09 Advanced aim': '09 조준 보조 고급', '10 Compact window': '10 작은 창', '11 Aim modes': '11 조준 모드',
  '12 Legit Aim - Target': '12 레짓 조준 — 대상', '13 Legit Aim - Advanced': '13 레짓 조준 — 고급',
  '14 Rage Aim - Target': '14 레이지 조준 — 대상', '15 Rage Aim - Advanced': '15 레이지 조준 — 고급',
  '16 ESP appearance': '16 ESP 모양', '17 Enemy filters': '17 적 거르기', '18 Loot presentation': '18 전리품 표시 방식',
  '19 Body filters': '19 시체 거르기', '20 Body colours': '20 시체 색', '21 Enemy colours': '21 적 색',
  '23 Player practice': '23 플레이어 연습', '24 Legit Aim RCS': '24 레짓 조준 반동 제어', '25 Rage Aim RCS': '25 레이지 조준 반동 제어',
  '26 Misc vision': '26 기타 — 시야', '27 Misc movement': '27 기타 — 이동', '28 Misc actions': '28 기타 — 동작',
  '29 Equipment & supplies': '29 장비·보급품', '30 Interaction': '30 상호작용', '31 Extraction overrides': '31 탈출 조건 무시',
  '32 Nearby enemy alerts': '32 근처 적 경고', '34 Rage ballistics': '34 레이지 탄도', '35 Rage weapon handling': '35 레이지 무기 다루기',
  '36 Direct hit': '36 직접 명중', '37 Vampire mode': '37 흡혈 모드',
};

// 섹션과 무관하게 같은 키는 같은 말 (조준 보조·레짓·레이지가 같은 키를 쓴다)
const byKey = {
  'Maximum distance': ['최대 거리', '미터.'],
  'Colour': ['색', null],
  'Refresh seconds': ['갱신 간격 (초)', null],
  'Visibility check': ['보이는지 확인', '고른 신체 부위까지 가로막는 것 없이 물리 광선이 닿아야 한다.'],
  'Require right mouse held': ['오른쪽 마우스도 누르고 있어야 함', 'HoldJsonHotkey 방식에서만: 오른쪽 마우스도 눌러야 한다. 다른 작동 방식은 이 설정을 무시한다.'],
  'Keep current target': ['지금 대상 유지', '누르고 있는 동안 거리·원뿔·보임 조건을 계속 만족하면 대상을 바꾸지 않는다.'],
  'Show FOV circle': ['시야각 원 보이기', '켜져 있는 동안 대상 포착 원뿔을 그린다.'],
  'Exclude same group': ['같은 그룹 제외', '내 그룹 ID(비어 있지 않은 것)를 공유하는 AI 는 건너뛴다. 모드가 만든 동맹을 다 알아내지는 못한다.'],
  'Target PMCs': ['PMC 노리기', 'PMC 역할과 진영.'],
  'Target ordinary scavs': ['일반 스캐브 노리기', 'assault·marksman 역할.'],
  'Target bosses and other AI': ['보스와 그 밖의 AI 노리기', '보스·경호원·레이더와 그 밖의 역할.'],
  'Show selected target': ['고른 대상 표시', '지금 따라가는 지점을 표시한다.'],
  'FOV radius degrees': ['시야각 반지름 (도)', '화면 가운데에서 잰 각도. 원뿔 전체 폭이 아니라 반지름이다.'],
  'Smoothing seconds': ['부드럽게 (초)', '반응 시간. 0 이면 부드럽게 하지 않지만 회전 속도 제한은 그대로 걸린다.'],
  'Maximum turn degrees per second': ['초당 최대 회전 (도)', '최대 회전 속도.'],
  'Target bone': ['노릴 부위', '조준할 신체 지점.'],
  'Target priority': ['대상 우선순위', '조준 원뿔 안 후보의 순위를 매기는 기준.'],
  'Activation': ['작동 방식', 'RightMouseHeld: 오른쪽 클릭을 누르고 있을 때만. HoldJsonHotkey: JSON 의 AimHold 키를 누르고 있을 때. Always: 켜져 있는 동안 늘 따라간다.'],
  'Show aim status': ['조준 상태 보이기', '입력 상태, 노릴 수 있는 AI, 대상 포착 여부를 화면에 보여 준다.'],
  'Target switch delay': ['대상 바꿈 지연', '잡은 대상에서 다른 대상으로 넘어가기 전에 기다린다. 처치한 뒤에도 그렇다.'],
  'Target switch seconds': ['대상 바꿈 지연 (초)', '넘어가기 전 기다리는 시간. 처음 잡을 때는 바로 잡는다.'],
  'Aim point fallback': ['대체 조준 지점', '보이는지 확인이 켜져 있을 때, 고른 지점이 막혀 있으면 보이는 가슴·골반·머리로 대신 노린다. 모든 지점은 시야각·거리·보정 한도를 통과해야 한다.'],
  'Limit aim correction': ['조준 보정 제한', '신체 지점이 이미 조준점에서 "최대 보정 각도" 안에 있을 때만 돕는다.'],
  'Maximum correction degrees': ['최대 보정 각도', '포착 각도 한도. 이것과 기본 시야각 중 작은 쪽을 쓴다.'],
  'Ignore enemies': ['무시할 적', '쉼표로 나눈 정확한 종류 이름, 보스 이름, 원래 역할 이름, 프로필 ID. 예: SCAV,ROGUE,Killa. 무작위로 붙는 보통 닉네임과는 맞추지 않는다.'],
  'RCS enabled': ['반동 제어 켜기', '고른 조준 모드의 전체 스위치가 켜져 있는 동안 반동을 줄인다. 대상을 잡지 않아도 돌고, 내 무기에만 영향을 준다.'],
  'Vertical reduction (%)': ['수직 반동 줄이기 (%)', '위아래(피치) 반동 충격을 줄이는 정도. 0 이면 원래 반동 그대로, 100 이면 새로 생기는 이 방향 충격을 없앤다.'],
  'Horizontal reduction (%)': ['수평 반동 줄이기 (%)', '좌우(요) 반동 충격을 줄이는 정도. 0 이면 원래 반동 그대로, 100 이면 새로 생기는 이 방향 충격을 없앤다.'],
  'Kickback reduction (%)': ['뒤로 밀림 줄이기 (%)', '무기가 뒤로 밀리는 반동을 줄인다. 관계없는 흔들림·애니메이션·카메라 기울기는 그대로다.'],
  'RCS right mouse only': ['오른쪽 마우스일 때만 반동 제어', '오른쪽 마우스를 누르고 있을 때만 반동 제어를 건다. 대상 포착과는 별개다.'],
};

// 섹션별 예외
const bySec = {
  '01 Enemy ESP': {
    'Enabled': ['켜기', '살아 있는 AI 플레이어를 보여 준다. 나는 빼고.'],
    'Bounding boxes': ['테두리 상자', '머리와 발을 기준으로 한 화면상의 상자.'],
    'Names and distance': ['이름과 거리', '적 종류와 거리. 보스는 이름을 그대로 보인다.'],
    'Health bars': ['체력 막대', '현재 총체력을 최대치에 대한 비율로.'],
    'Upper body skeleton': ['상체 뼈대', '머리·척추·어깨·팔.'],
    'Visible only': ['보이는 적만', '벽 뒤의 AI 는 숨긴다. 물리 광선 비용이 든다.'],
    'Colour': ['색', '적 표시 색.'],
    'Hidden enemies only': ['가려진 적만', '머리·가슴·골반 어느 표본에도 막힘 없는 물리 광선이 닿지 않을 때만 적을 보인다. ESP 에만 걸리고 조준 보조 대상 선택에는 영향이 없다. "보이는 적만"을 끈다.'],
  },
  '02 Loot ESP': {
    'Enabled': ['켜기', '바닥에 생성된 전리품을 보여 준다. 컨테이너 안은 훑지 않는다.'],
    'Container markers': ['컨테이너 표시', '뒤질 수 있는 컨테이너 위치를 보여 준다(내용물 없이).'],
    'Name filter': ['이름 거르기', '쉼표로 나눈 이름 조각 또는 템플릿 ID. 비우면 전부 보인다. 예: bitcoin,M995,razor'],
    'Refresh seconds': ['갱신 간격 (초)', '낮을수록 CPU 를 더 쓴다.'],
    'Maximum labels': ['최대 이름표 수', '가까운 것부터.'],
    'Colour': ['색', '전리품 표시 색.'],
  },
  '03 Aimbot': {
    'Enabled': ['켜기', '고른 작동 방식으로 살아 있는 AI 를 따라간다. 자동으로 쏘지는 않는다.'],
  },
  '04 Display': { 'Text size': ['글자 크기', '화면 픽셀.'] },
  '05 Hotkeys': {
    'Instructions': ['안내', '단축키는 JSON 으로만 정하고 바뀌면 저절로 다시 읽는다. BepInEx 키 고르기 창은 없다. 파일: BepInEx/config/SPTFieldToolkit/config.json'],
    'Optional actions': ['선택 동작', '이 JSON 속성을 원하는 키 이름과 함께 더한다. 없는 속성은 None 이다. IgnoreTarget 은 따라가는 적이나 조준점 5도 안의 적을 이번 레이드 동안 제외한다.'],
  },
  '06 Loot filters and highlights': {
    'Preset': ['프리셋', 'All, 분류 프리셋들, CustomCategories, QuestRun(퀘스트·위시리스트 + 열쇠·의약품), Wishlist(위시리스트만).'],
    'Wishlist highlighting': ['위시리스트 강조', '게임 안 위시리스트(자동 그룹 포함)에 든 바닥 아이템에 표시를 붙인다.'],
    'Quest highlighting': ['퀘스트 강조', '시작한 퀘스트의 미완료 찾기·넘기기 목표와 템플릿이 같은 아이템에 표시를 붙인다. 내구도 같은 다른 목표 조건까지 보장하지는 않는다.'],
    'Important items bypass filters': ['중요 아이템은 거르기 무시', '강조된 위시리스트·퀘스트 아이템은 분류·이름 거르기를 무시한다(위시리스트 전용 프리셋 제외).'],
    'Wishlist colour': ['위시리스트 색', '위시리스트 이름표 색.'],
    'Quest colour': ['퀘스트 색', '퀘스트 이름표 색이 위시리스트 색보다 앞선다.'],
    'Group nearby labels': ['가까운 이름표 묶기', '화면에서 가까운 전리품 이름표를 하나로 묶고 겹치는 묶음은 숨긴다.'],
    'Grouping pixels': ['묶기 칸 크기 (픽셀)', '화면상 묶기 칸의 크기.'],
    'Names per group': ['묶음당 이름 수', '나머지는 "+N nearby" 로 보인다.'],
  },
  '07 Corpse markers': {
    'Enabled': ['켜기', '전리품 ESP 와 따로 시체를 보여 준다. 시체는 조준 보조 대상이 되지 않는다.'],
    'Refresh seconds': ['갱신 간격 (초)', '목록을 한 번 훑은 뒤 다시 훑기까지의 간격.'],
    'Colour': ['색', '시체 이름표 색.'],
  },
  '10 Compact window': { 'Scale': ['크기', '창 크기 배율. 화면에 맞게 저절로 줄어든다.'] },
  '11 Aim modes': { 'Active mode': ['쓸 모드', '이 프로필만 돈다. 모드 바꾸기 키(이 설치본은 Keypad6, 원래 F8)로 바꾼다. 각 탭에서 모드를 켜면 그 모드가 골라진다.'] },
  '15 Rage Aim - Advanced': {
    'Aim point fallback': ['대체 조준 지점', '보이는지 확인이 켜져 있을 때, 고른 지점이 막혀 있으면 보이는 가슴·골반·머리로 대신 노린다. 모든 지점은 시야각·거리·보정 한도를 통과해야 한다. 머리를 고르고 "머리만 노리기"가 켜져 있으면 몸으로 대신 노리지 않는다.'],
    'Instant lock': ['즉시 고정', '작동하는 동안 고른 대상으로 곧바로 돈다. 끄면 레이지 프로필의 부드럽게·회전 속도 설정을 쓴다. 자동으로 쏘지는 않는다.'],
    'Strict head targeting': ['머리만 노리기', '레이지 조준이 머리만 쓰게 한다 — 노릴 부위를 무시하고 몸으로 대신 노리기를 끈다. 살아 있는 모델의 머리 위치 하나를 쓰고, 잘못됐거나 막힌 머리 정보는 건너뛴다. 사격을 막지는 않는다.'],
  },
  '16 ESP appearance': {
    'Display preset': ['표시 프리셋', 'Simple: 모서리 꺾쇠와 최소 정보. Detailed: 전체 상자와 뼈대. Custom: 내 모양 설정 유지. 프리셋은 적·전리품·시체 전체 스위치, 종류 거르기, 보임 거르기는 바꾸지 않는다.'],
    'Box style': ['상자 모양', '전체 상자, 모서리 꺾쇠, 작은 표시 중 고른다.'],
    'Enemy type colours': ['적 종류별 색', 'USEC·BEAR·스캐브·보스·경호원·레이더·로그·교단원·같은 그룹 아군을 다른 색으로.'],
    'Hidden colour brightness': ['가려진 적 밝기', '가려진 적에 곱하는 밝기. 보이는 적은 종류 색 그대로.'],
    'Boss emphasis': ['보스 강조', '보스 위에 마름모를 하나 더 그리고 이름을 그대로 보인다.'],
    'Distant enemy markers': ['먼 적은 작은 표시로', '먼 적의 상자를 작은 표시로 바꾸고 체력·뼈대를 숨겨 화면을 덜 어지럽힌다.'],
    'Enemy detail distance': ['적 세부 거리', '이 거리(미터)를 넘으면 적을 간단히 그린다.'],
  },
  '18 Loot presentation': {
    'Category icons': ['분류 아이콘', '탄약·열쇠·의약품·귀중품·부착물 등 분류 기호를 작게 그린다.'],
    'Distance-based loot detail': ['거리에 따른 전리품 세부', '먼 바닥 전리품은 작은 표시로, 가까이 가거나 그쪽을 보면 이름을 보인다. 중요 아이템은 작은 꼬리표를 유지한다.'],
    'Loot name distance': ['전리품 이름 거리', '이 거리(미터) 안에서는 아이템 이름을 다 보인다.'],
    'Loot focus angle': ['전리품 주시 각도', '조준점에서 이 각도(도) 안으로 아이템·묶음을 보면 자세히 보인다.'],
    'Look to expand clusters': ['보면 묶음 펼치기', '묶음은 아이템 수만 보이다가 그쪽을 보면 이름을 펼친다.'],
    'Expanded names': ['펼칠 이름 수', '주시한 묶음에서 보일 최대 이름 수. 나머지는 "+N nearby" 로.'],
  },
  '19 Body filters': {
    'Body type colours': ['시체 종류별 색', '시체 전용 색표를 쓴다. 끄면 원래의 한 가지 시체 색.'],
  },
  '23 Player practice': {
    'Unlimited stamina': ['무한 스태미나', '레이드 중 살아 있는 동안 몸·팔 스태미나를 가득 유지한다. 레이드 사이와 시작할 때 꺼진다.'],
    'Prevent malfunctions': ['고장 막기', '지금 든 총기에 새 고장이 나지 않게 한다. 봇에는 영향이 없고 이미 난 고장은 풀지 않는다. 레이드 사이에 꺼진다.'],
    'Demigod': ['반신', '부위별 현재·최대 체력을 고른 배율만큼 올리고 체력 비율은 지킨다. 계속 회복하거나 무적이 되지는 않는다. 끄거나 레이드 사이·패닉 때 원래 최대치로 돌아간다.'],
    'Godmode': ['무적', '이번 레이드 동안 피해와 죽음을 막는다. 켜면 반신이 꺼진다. 레이드 사이와 패닉 때 꺼진다.'],
    'Protection': ['보호 범위', 'Full 은 모든 피해를 막고, Custom 은 고른 피해 종류만 막는다.'],
    'Bullets': ['총탄', 'Custom 일 때 총탄 피해를 막는다.'],
    'Explosions': ['폭발', 'Custom 일 때 폭발 피해를 막는다.'],
    'Falls': ['낙하', 'Custom 일 때 낙하 피해를 막는다.'],
    'Bleeding': ['출혈', 'Custom 일 때 출혈 피해를 막는다.'],
    'Melee and impact': ['근접·충격', 'Custom 일 때 근접·충격 피해를 막는다.'],
    'Environmental damage': ['환경 피해', 'Custom 일 때 환경 피해를 막는다.'],
    'Survival and toxins': ['생존·독소', 'Custom 일 때 생존(허기·갈증 등)·독소 피해를 막는다.'],
    'Health multiplier': ['체력 배율', '반신의 체력 배율. 5 면 부위마다 원래 최대치의 다섯 배. 바꿔도 겹쳐 곱해지지 않고 파괴된 팔다리를 고치지 않는다.'],
  },
  '26 Misc vision': {
    'Night vision': ['야간 투시', '고글 없이 기본 카메라에 게임의 야간 투시를 건다. 열상과 함께 켤 수 없다. 레이드 사이에 꺼진다.'],
    'Thermal vision': ['열상', '기본 카메라에 게임의 열상을 건다. 야간 투시와 함께 켤 수 없다. 끄면 카메라를 원래대로 돌린다.'],
    'No flash': ['섬광 없애기', '섬광탄의 하얀 화면과 잔상을 기본 카메라에서 없앤다. 수류탄 피해나 봇 행동은 그대로다.'],
    'No visor': ['바이저 효과 없애기', '바이저의 금·먼지 화면 효과를 숨긴다. 바이저와 그 보호는 그대로 남는다.'],
  },
  '27 Misc movement': {
    'No-clip': ['벽 통과 비행', '지형을 뚫고 난다. JSON 의 NoClipForward/Back/Left/Right/Up/Down 키를 쓰고, 메뉴에서는 멈춘다. 트인 곳에서 끈다. 레이드 사이에 꺼진다.'],
    'Flight speed': ['비행 속도', '벽 통과 비행 속도(초당 미터).'],
    'Flight boost multiplier': ['비행 가속 배율', 'JSON 의 NoClipBoost 키를 누르는 동안의 속도 배율.'],
  },
  '28 Misc actions': { 'Door radius': ['문 반경', '"근처 문 열기"의 반경(미터). 보통 잠긴 문만 바꾼다.'] },
  '29 Equipment & supplies': {
    'Infinite medical supplies': ['무한 의약품', '쓰는 의약품의 충전량을 지키고 소모를 막는다. 치료 시간·효과는 그대로. 이미 빈 것은 채우지 않는다. 레이드 사이에 꺼진다.'],
    'No weight penalties': ['무게 불이익 없애기', '몸에 걸리는 무게 불이익을 무게 0 인 것처럼 계산한다. 인벤토리 무게 표시와 아이템 무게는 그대로, 장비 자체의 불이익도 남는다.'],
    'Adjustable fire rate': ['연사 속도 조절', '지금 든 무기의 자동 연사 속도와 단발 대기 시간을 덮어쓴다. 사격 모드를 더하거나 저절로 쏘지는 않는다.'],
    'Fire rate RPM': ['연사 속도 (RPM)', '원하는 분당 발사 수. 무기 애니메이션·노리쇠 순환·고장 때문에 실제로는 덜 나갈 수 있다.'],
    'Restore maximum durability': ['최대 내구도까지 복구', '"착용 장비 수리" 때 줄어든 최대 내구도도 원래 최대치로 되살린다. 끄면 지금의 최대치까지만 고친다.'],
  },
  '30 Interaction': {
    'Extended interaction range': ['상호작용 거리 늘리기', '전리품·문 상호작용 광선을 늘리되 게임의 가림 확인은 지킨다. 상인·차량·스크립트 상호작용은 늘리지 않는다.'],
    'Interaction distance': ['상호작용 거리', '"상호작용 거리 늘리기"가 켜졌을 때 전리품·문 상호작용 거리(미터).'],
  },
  '31 Extraction overrides': {
    'Always-available extractions': ['탈출구 항상 열기', '내 진영의 보통 탈출구를 스폰 위치·비용·배낭·전원·작동 조건과 상관없이 열어 둔다. 탈출 시간은 그대로. 비밀·이동·스크립트 탈출은 빠진다. 레이드 사이에 꺼진다.'],
  },
  '32 Nearby enemy alerts': {
    'Nearby enemy alerts': ['근처 적 경고', '살아 있는 AI 가 반경 안에 들어오면 ESP 와 따로 경고를 띄운다. 같은 그룹 ID 는 빠진다 — 모드가 만든 동맹은 그룹 ID 를 공유하지 않을 수 있다.'],
    'Alert radius': ['경고 반경', '적 경고 반경(미터).'],
    'Alert sound': ['경고음', '새 적이 반경에 들어오면 짧은 소리를 낸다. 경계에서 되풀이되지 않게 5미터 여유와 대기 시간을 둔다.'],
    'Alert sound cooldown': ['경고음 대기 시간', '새 적 경고음 사이의 최소 초.'],
    'Alert volume': ['경고음 음량', '경고음의 음량.'],
  },
  '34 Rage ballistics': {
    'Movement prediction': ['움직임 예측', '대상 속도와 예상 탄 비행 시간으로 고른 부위보다 앞을 노린다. 선택 사항이고 "머리만 노리기"와 별개다.'],
    'Bullet drop compensation': ['탄 낙차 보정', '예상 비행 시간 동안의 중력만큼 조준을 올린다. 지금 탄약과 무기 탄속을 쓴다. 근사 계산이라 공기 저항·탄 퍼짐·영점은 다 반영하지 않는다.'],
    'Prediction strength': ['예측 세기', '움직임 예측에 넣을 대상 속도의 비율(%).'],
    'Maximum flight prediction seconds': ['최대 비행 예측 (초)', '예상 비행 시간의 최대치. 이 안에서 답이 없으면 보정 없이 지금 고른 지점을 노린다.'],
  },
  '35 Rage weapon handling': {
    'No ADS movement sway': ['조준 중 흔들림 없애기', '레이지 조준이 켜져 있을 때 실제로 조준경을 보는 동안 들고 있는 무기의 이동·회전 흔들림과 걷기 출렁임을 없앤다. 대상을 잡지 않아도 돈다. 봇·지향 사격·레짓 조준에는 영향이 없고, 반동은 반동 제어가 따로 맡는다.'],
  },
  '36 Direct hit': {
    'Direct hit mode': ['직접 명중 모드', '내 총탄을 조건에 맞는 AI 의 고른 신체 부위에 바로 맞힌다. 조준 전체 스위치와 별개이고 시점을 움직이지 않는다. 레이지의 대상 종류·그룹·제외 거르기와 자체 시야각·거리·보임 조건을 쓴다. 대상이 없으면 보통 사격이다. 레이드마다 꺼진다.'],
    'Direct hit body part': ['직접 명중 부위', '게임의 명중·피해를 적용할 부위. 직접 명중 모드가 꺼져 있으면 숨지만 고른 값은 기억한다. 탄약 피해와 게임의 신체 명중 경로를 쓰고, 탄이 실제로 날아가거나 가로막히지 않는다.'],
    'Direct hit range': ['직접 명중 거리', '적까지의 최대 거리(미터).'],
    'Direct hit FOV': ['직접 명중 시야각', '조준점 둘레의 각도 반지름. 탄 하나마다 가장 가까운 후보 넷까지 본다.'],
    'Direct hit visible only': ['보이는 대상만 직접 명중', '고른 부위가 훤히 보여야 한다. 끄면 엄폐물 너머로도 맞힌다. 대상 거르기는 그대로다.'],
  },
  '37 Vampire mode': {
    'Vampire mode': ['흡혈 모드', '살아 있는 AI 에게 실제로 입힌 체력 피해만큼 내 체력을 되찾는다. 보통 사격과 직접 명중 모두. 파괴된 부위를 살리거나 부상을 없애지는 않는다. 레이드마다 꺼진다.'],
    'Damage healing (%)': ['피해 회복 비율 (%)', '적이 실제로 잃은 체력에 대한 회복 비율. 넘친 피해와 방어구에 흡수된 피해는 세지 않는다.'],
    'Kill healing bonus': ['처치 회복 추가', '내 피해로 적을 처치하면 더 회복한다. 0 이면 피해만큼만.'],
    'Maximum healing per hit': ['한 번 명중 최대 회복', '피해 한 번에서 얻는 회복(처치 추가 포함)의 상한. 산탄처럼 탄이 여럿이면 여러 번 셀 수 있다.'],
  },
};

// 진영·종류 (17·19·20·21)
const kinds = { USEC: 'USEC', BEAR: 'BEAR', SCAV: '스캐브', Boss: '보스', Guard: '경호원', Raider: '레이더', Rogue: '로그', Cultist: '교단원', Bloodhound: '블러드하운드', Infected: '감염자', Friendly: '아군', Other: '기타' };
const cats = { Ammo: '탄약', Valuables: '귀중품', Keys: '열쇠', Medical: '의약품', Attachments: '부착물', Weapons: '총기', Equipment: '장비', Provisions: '식량', Barter: '교환품', Other: '기타' };

function translate(e) {
  const s = bySec[e.section]?.[e.key];
  if (s) return s;
  let m;
  if (e.section === '17 Enemy filters' && (m = e.key.match(/^Show (.+)$/)) && kinds[m[1]])
    return [`${kinds[m[1]]} 보이기`, `살아 있는 ${kinds[m[1]]} 표시를 보인다. 아군은 같은 그룹 ID(비어 있지 않은 것)라는 뜻이지 모드가 만든 동맹 전부가 아니다.`];
  if (e.section === '19 Body filters' && (m = e.key.match(/^Show (.+) bodies$/)) && kinds[m[1]])
    return [`${kinds[m[1]]} 시체 보이기`, '알 수 있으면 이 종류의 시체를 보인다. 누구인지 모르는 시체는 진영으로 대신한다.'];
  if (e.section === '20 Body colours' && (m = e.key.match(/^(.+) body colour$/)) && kinds[m[1]])
    return [`${kinds[m[1]]} 시체 색`, `${kinds[m[1]]} 시체에만 쓰는 색.`];
  if (e.section === '21 Enemy colours' && (m = e.key.match(/^(.+) colour$/)) && kinds[m[1]])
    return [`${kinds[m[1]]} 색`, `보이는 ${kinds[m[1]]} 의 색. 가려진 표시는 밝기 배율을 곱한다.`];
  if (e.section === '06 Loot filters and highlights' && (m = e.key.match(/^Category - (.+)$/)) && cats[m[1]])
    return [`분류 — ${cats[m[1]]}`, 'CustomCategories 프리셋에서 쓴다.'];
  const k = byKey[e.key];
  if (k) return [k[0], k[1] ?? null];
  return null;
}

const settings = {};
const missing = [];
for (const e of entries) {
  const t = translate(e);
  if (!t) { missing.push(e.id); continue; }
  settings[e.id] = t[1] ? { name: t[0], desc: t[1] } : { name: t[0] };
}
const usedSecs = {};
for (const e of entries) usedSecs[e.section] = sections[e.section] ?? (missing.push('섹션:' + e.section), e.section);
const out = { guid: 'local.spt.practicemenu', name: '치트 메뉴 (Tarkov Override)', sections: usedSecs, settings };
fs.writeFileSync(outPath, JSON.stringify(out, null, 2) + '\n');
console.log('항목', Object.keys(settings).length, '/', entries.length, '빠짐', missing.length, missing.slice(0, 20));
