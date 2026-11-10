// ISB 계열 아이템 이름 사전을 만든다 — 켜져 있는 서버에게 물어 **바닐라 한국어를 되쓰고**,
// ISB 고유 용어만 아래 낱말 사전으로 옮긴다.
//
// 왜 생성기인가 — 이 묶음은 이름 411개인데 틀이 반복된다
// (`Black Division Heavy Operative (Desert Camo - No Patches)` 꼴이 수십 개). 손으로 치면
// 같은 낱말이 자리마다 달라진다. 그리고 `(Aishi)` 가 붙은 것들은 **바닐라 부품 이름 그대로**라
// (`AR-15 5.56x45 Magpul PMAG 30 GEN M3 STANAG 30-round magazine (Aishi)`) 바닐라 한국어를
// 서버에서 찾아 쓰는 것이 손으로 번역하는 것보다 정확하다.
//
// 사용: node tools/gen-isb-item-names.mjs [--write] [--locale kr]
//   --write 없이 돌리면 **무엇이 안 옮겨졌는지만** 보여 준다(사전을 쓰지 않는다).
//
// 내는 것: src/SptKoreanLocale/lang/item-names-isb-1.json
// 값에 색 태그·●를 넣지 않는다 — 그건 다른 모드가 덧대고 SptKoreanLocale 이 그대로 둔다.
import fs from 'node:fs';
import path from 'node:path';
import { deflateSync, inflateSync, inflateRawSync, gunzipSync } from 'node:zlib';

const args = process.argv.slice(2);
const locale = args.includes('--locale') ? args[args.indexOf('--locale') + 1] : 'kr';
const write = args.includes('--write');
const ROOT = 'E:/Spt_Mods';
const OUT = path.join(ROOT, 'src', 'SptKoreanLocale', 'lang', 'item-names-isb-1.json');

// ISB 고유 낱말. 긴 것부터 맞춰야 한다(Squad Leader 가 Leader 보다 먼저).
const TERMS = [
    ['Black Division', '블랙 디비전'],
    ['Chaos Insurgency', '카오스 인서전시'],
    ['Heavy Operative', '중화기 요원'],
    ['Scout Operative', '정찰 요원'],
    ['Specialist Operative', '전문 요원'],
    ['Squad Leader', '분대장'],
    ['Team Leader', '팀장'],
    ['Heavy Operator', '중화기 대원'],
    ['Scout Operator', '정찰 대원'],
    ['Specialist Operator', '전문 대원'],
    ['Infiltrator', '침투 요원'],
    ['Operative', '요원'],
    ['Operator', '대원'],
    ['Defector', '전향자'],
    ['Nocturnal', '녹터널'],
    ['Red Right Hand', '레드 라이트 핸드'],
    ['Scorpion', '스콜피온'],
    ['Leader', '지휘관'],
    ['Access Card', '출입 카드'],
    ['Gear Crate', '장비 상자'],
    ['gear crate', '장비 상자'],
    ['encryption keys', '암호 열쇠'],
    ['intelligence plans', '첩보 계획서'],
    ['keycard', '키카드'],
    ['rugged laptop', '견고형 노트북'],
    ['target', '표적'],
    ['Armband', '완장'],
    ['Dogtag', '인식표'],
    ['Anomalous Property Detector', '변칙 물성 탐지기'],
    ['Briefcase with TerraGroup Documents', '테라그룹 문서가 든 서류가방'],
    ['Advanced Modified SSTV Radio', '개량형 SSTV 무전기'],
    ['Assault Rifle', '돌격소총'],
    ['assault rifle', '돌격소총'],
    ['Modified', '개조'],
    ['Ballistic Plate', '방탄판'],
    ['body armor', '방탄복'],
    ['Plate Carrier', '플레이트 캐리어'],
    ['plate carrier', '플레이트 캐리어'],
    ['helmet', '헬멧'],
    // 변형 꼬리표
    ['No Patches', '패치 없음'],
    ['Desert Camo', '데저트 위장'],
    ['Red Camo', '레드 위장'],
    ['Black Camo', '블랙 위장'],
    ['Multicam Black', '멀티캠 블랙'],
    ['Urban Ops', '어반'],
    ['Woodland', '우드랜드'],
    ['Moonlight', '문라이트'],
    ['Arctic', '아틱'],
    ['Artic', '아틱'],
    ['Woman', '여성'],
    ['Desert', '데저트'],
    ['Black', '블랙'],
    ['Wolf', '울프'],
    ['Coyote', '코요테'],
    ['Version II', '2형'],
    ['Ballistic Helmet', '방탄 헬멧'],
    ['headset', '헤드셋'],
    ['Trooper', '트루퍼'],
    ['Detector', '탐지기'],
    ['Crate', '상자'],
    ['Intel', '첩보'],
    ['Diary', '일기'],
    // 무기 부품 낱말 — `(Aishi)` 가 붙은 것들 가운데 바닐라에 같은 영문이 없는 것을 위해.
    ['upper receiver', '상부 리시버'],
    ['charging handle', '장전 손잡이'],
    ['sound suppressor', '소음기'],
    ['suppressor mount', '소음기 마운트'],
    ['Gas Block', '가스 블록'],
    ['Pistol Grip', '권총 손잡이'],
    ['Carbine stock', '카빈 개머리판'],
    ['buffer tube', '버퍼 튜브'],
    ['handguard', '핸드가드'],
    ['flash hider', '소염기'],
    ['tactical device', '전술 장치'],
    ['Scout Light Thumbscrew Mount', '스카우트 라이트 손나사 마운트'],
    ['Scout Light', '스카우트 라이트'],
    ['Cantilever Mount', '캔틸레버 마운트'],
    ['reflex sight', '반사식 조준경'],
    ['holographic sight', '홀로그래픽 조준경'],
    ['ring scope mount', '링 조준경 마운트'],
    ['pistol slide', '권총 슬라이드'],
    ['threaded barrel', '나사 총열'],
    ['drum magazine', '드럼 탄창'],
    ['barrel', '총열'],
    ['stock', '개머리판'],
    ['Brake', '브레이크'],
    ['Weapon Case', '무기 가방'],
    ['Military Coalition Officer', '군사 연합 장교'],
    ['Legionnaire', '군단병'],
    ['Instructor', '교관'],
    ['Triarius', '트리아리우스'],
    ['submachine gun', '기관단총'],
    ['pistol', '권총'],
    // 의상·장비 묶음 이름 — 바닐라 한국어도 이런 것은 음차한다.
    ['Alone in Icebreaker', '아이스브레이커에 홀로'],
    ['White Tusk', '화이트 터스크'],
    ['Spearthrower', '스피어스로워'],
    ['Deadly Frog', '데들리 프로그'],
    ['Adaptive Combat', '어댑티브 컴뱃'],
    ['Gladiator', '글래디에이터'],
    ['Champion', '챔피언'],
    ['Lonsdale', '론스데일'],
    ['DesOps', '데저트 작전'],
    ['Flexion', '플렉션'],
    ['Squadron', '편대'],
    ['Breacher', '돌파조'],
    ['Predator', '프레데터'],
    ['Sharpshooter', '저격수'],
    ['Boreas', '보레아스'],
    ['Black Lynx', '블랙 링스'],
    ['Jacket', '재킷'],
    ['Trousers', '바지'],
    ['Goons', '구너스'],
    ['JUGGERNAUT', '저거너트'],
    ['Experimental', '실험용'],
    ['Level V', '레벨 V'],
    ['Custom', '커스텀'],
];

// 숫자가 끼는 자리는 낱말로 안 된다.
const PATTERNS = [
    [/(\d+)-round magazine/g, '$1발 탄창'],
    [/(\d+(?:\.\d+)?) inch/g, '$1인치'],
];

// 틀로 안 되는 것 — 문장형 이름과 약칭을 손으로 적는다. 영문 글자가 **정확히** 같을 때만 쓴다.
const MANUAL = {
    'Diary with contact details for Aishi and her team.': '아이시와 그 팀의 연락처가 적힌 일기',
    'Masterkey issued to the Deputy Director of ISB, "Juna Aishi".': 'ISB 부국장 "주나 아이시" 에게 발급된 마스터키',
    'Improvised ISB device for hacking systems': 'ISB 제 급조 해킹 장치',
    'Locked Reinforced Briefcase with ISB Documents': '잠긴 보강 서류가방 (ISB 문서)',
    'Reinforced Briefcase with ISB Documents': '보강 서류가방 (ISB 문서)',
    'ISB TDL Military Tactical Tablet': 'ISB TDL 군용 전술 태블릿',
    'ISB Special Access Keycard': 'ISB 특별 출입 키카드',
    'ISB Special Access Keycard (White Tusk)': 'ISB 특별 출입 키카드 (화이트 터스크)',
    "Aishi's MK": '아이시의 마스터키',
    'A note issued by TerraGroup to its security division regarding USEC ISB':
        '테라그룹이 보안 부서에 보낸 USEC ISB 관련 각서',
    "Cover page of Juna Aishi's dossier while she was still in service to USEC":
        'USEC 복무 당시 주나 아이시의 인사 기록 표지',
    'Transcript of a computer conversation between Aishi and an unknown person':
        '아이시와 신원 미상자의 컴퓨터 대화 기록',
    'Transcript of a conversation between Aishi and Knight': '아이시와 나이트의 대화 기록',
    'Data from Black Division laptop': '블랙 디비전 노트북에서 받은 자료',
    'Limited TerraGroup Labs Access Keycard issued to the Deputy Director of ISB':
        'ISB 부국장에게 발급된 테라그룹 랩 제한 출입 키카드',
};

// 영문으로 두는 것 — 모델 코드이거나, **그 모드가 약칭에 원시 자산 키를 그대로 넣어 둔 것**이다.
// 뒤엣것은 번역할 글이 아니다(보기: `[Aishi]_(bfg_plateminus_level2_soft_armor_back) (2/26)`).
const KEEP = [
    { re: /^\[Aishi\]_\(/, why: '그 모드가 약칭에 원시 자산 키를 넣어 둔 것이다 — 번역할 글이 아니다' },
    { re: /^ISB$/, why: '약칭이 ISB 세 글자다' },
    { re: /^ISB M4A1$/, why: '모델 코드다' },
    { re: /^ISB ULACH$/, why: '헬멧 모델명이다' },
    { re: /^ISB ESAPI V \(/, why: '방탄판 모델명이다' },
    { re: /^MRC ISB \(/, why: '모델명이다' },
    { re: /^ISB KC$/, why: '키카드 약칭이다' },
    { re: /^Aishi L\.KC$/, why: '키카드 약칭이다' },
    { re: /^Aishi$/, why: '인물 이름 약칭이다' },
    { re: /^ISB W\.Case$/, why: '무기 가방 약칭이다' },
    { re: /^ISB FCPC V5$/, why: '플레이트 캐리어 모델명이다' },
    { re: /^LBCR ISBE V2$/, why: '체스트 리그 모델명이다' },
    { re: /^USEC ISB TACRES$/, why: '부대 표기다' },
    { re: /MARPAT/, why: '위장 무늬 이름이다' },
    { re: /^"The ISB"$/, why: '현수막에 적힌 제목이다' },
];

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
const ask = async (code) => {
    const res = await fetch(`https://127.0.0.1:6969/client/locale/${code}`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' }, body: deflateSync(Buffer.from('{}', 'utf8')),
    });
    const raw = Buffer.from(await res.arrayBuffer());
    for (const f of [inflateSync, inflateRawSync, gunzipSync]) {
        try { return JSON.parse(f(raw).toString('utf8')).data; } catch { /* 다음 */ }
    }
    return JSON.parse(raw.toString('utf8')).data;
};

const kr = await ask(locale);
const en = await ask('en');
if (!kr || !en) { console.error('locale 응답이 비었다 — 서버가 켜져 있나'); process.exit(2); }

const HANGUL = /[\uAC00-\uD7A3]/;
const RE_ISB = /ISB|Aishi|Black Division|Chaos Insurgency|SOF |Blackdiv/i;
const strip = (v) => String(v).replace(/<[^>]*>/g, '').replace(/[●○]/g, ' ').replace(/\s+/g, ' ').trim();

// 1) 바닐라(그리고 다른 모드가 이미 한국어로 둔) 영문 → 한국어 지도.
//    같은 영문에 한국어가 둘이면 쓰지 않는다(어느 쪽인지 모른다).
const byEnglish = new Map();
const ambiguous = new Set();
for (const key of Object.keys(en)) {
    if (!/ (Name|ShortName)$/.test(key)) continue;
    const e = strip(en[key]), k = strip(kr[key]);
    if (!e || !k || !HANGUL.test(k)) continue;
    // ISB 계열은 **되쓸 바닐라가 아니다.** 빼지 않으면 우리 사전이 이미 입혀진 서버에서
    // 제 결과를 되쓴다 — 2026-11-10 에 "바닐라 되쓰기 363" 으로 그렇게 돌았다.
    if (RE_ISB.test(e)) continue;
    if (byEnglish.has(e) && byEnglish.get(e) !== k) { ambiguous.add(e); continue; }
    byEnglish.set(e, k);
}
for (const e of ambiguous) byEnglish.delete(e);

// 2) 대상: **영문(en) 을 기준으로** 고른다. 이미 적용된 kr 을 기준으로 삼으면 한 번 넣은 뒤
//    다시 돌릴 때 대상이 0이 되어 **사전을 비운다**(2026-11-10 에 그렇게 374항목을 날렸다).
//    kr 이 이미 한국어인 열쇠는 건너뛰되, **우리 사전이 넣은 것은 다시 만든다** — 그래야 멱등하다.
// kr 쪽은 **고르는 기준으로 쓰지 않는다.** 우리 사전이 이미 입혀져 있으면 그것까지 "남이 한
// 한국어" 로 보여 대상이 사라지기 때문이다. 영문만 보고 고르면 서버 상태와 무관하게 늘 같은
// 결과가 나온다(멱등). 다만 kr 이 이미 한국어인 열쇠는 **세어서 알린다** — 다른 모드가 먼저
// 넣은 것을 우리가 밀어낼 수 있으니 사람이 보게 한다.
const targets = [];
let alreadyKorean = 0;
for (const key of Object.keys(en)) {
    if (!/ (Name|ShortName)$/.test(key)) continue;
    const e = strip(en[key]);
    if (!e || !RE_ISB.test(e)) continue;
    if (HANGUL.test(strip(kr[key] ?? ''))) alreadyKorean++;
    targets.push({ key, v: e });
}

// 3) 옮기기: ① 바닐라 되쓰기(꼬리표를 떼고 찾아본다) ② 낱말 사전 ③ 남은 영문 보고
const paren = /\s*\(([^()]*)\)\s*$/;
const translateTerms = (s) => {
    let out = s;
    for (const [re, k] of PATTERNS) out = out.replace(re, k);
    for (const [e, k] of TERMS) out = out.split(e).join(k);
    return out;
};
const entries = {};
const keptAsIs = {};
const leftovers = [];
let fromVanilla = 0, fromTerms = 0, fromManual = 0;

for (const { key, v } of targets) {
    // ⓞ 영문으로 둘 것 / 손으로 적은 것이 먼저다
    const keep = KEEP.find((k) => k.re.test(v));
    if (keep) { keptAsIs[key] = keep.why; continue; }
    if (MANUAL[v]) { entries[key] = MANUAL[v]; fromManual++; continue; }
    // ① 꼬리표를 뗀 몸통이 바닐라에 있나 — `… (Aishi)` 꼴
    const m = v.match(paren);
    if (m) {
        const body = v.slice(0, m.index).trim();
        const tail = m[1].trim();
        const base = byEnglish.get(body);
        if (base) {
            entries[key] = `${base} (${translateTerms(tail)})`;
            fromVanilla++;
            continue;
        }
    }
    const whole = byEnglish.get(v);
    if (whole) { entries[key] = whole; fromVanilla++; continue; }

    // ② 낱말 사전
    const t = translateTerms(v);
    if (t !== v) { entries[key] = t; fromTerms++; }
    else leftovers.push({ key, v });
}

const stillEnglish = Object.entries(entries).filter(([, v]) => /[A-Za-z]{4,}/.test(v.replace(/\(.*?\)/g, '')));
console.log(`그 가운데 kr 이 이미 한국어인 열쇠 ${alreadyKorean}개 (우리가 넣은 것이면 정상이다)`);
console.log(`대상 ${targets.length}개 / 옮긴 것 ${Object.keys(entries).length}개 (바닐라 되쓰기 ${fromVanilla} + 낱말 사전 ${fromTerms} + 손으로 ${fromManual}) / 영문 유지 ${Object.keys(keptAsIs).length}개`);
console.log(`한 낱말도 못 옮긴 것 ${leftovers.length}개`);
for (const r of leftovers.slice(0, 40)) console.log(`  ${r.key}\t${r.v}`);
if (leftovers.length > 40) console.log(`  … (+${leftovers.length - 40})`);
console.log(`\n옮겼지만 영문 낱말이 남은 것 ${stillEnglish.length}개 (모델명·고유명사면 정상)`);
for (const [k, v] of stillEnglish.slice(0, 15)) console.log(`  ${k}\t${v}`);

if (!write) { console.log('\n--write 를 주면 사전을 쓴다.'); process.exit(0); }

// 책임 범위는 **맡은 열쇠만** 적는다. id 로 적고 `(Name|ShortName)` 을 붙이면 같은 아이템의
// 번역하지 않은 짝까지 범위에 들어와 "묶음에서 빠뜨림" 으로 잡힌다(2026-11-10 에 92건 그랬다 —
// 보기: `66326bfd… ShortName` 만 ISB 라서 옮겼는데 그 아이템의 Name 은 ISB 가 아니다).
const covered = [...Object.keys(entries), ...Object.keys(keptAsIs)].sort();
const ids = [...new Set(covered.map((k) => k.split(' ')[0]))];
const dict = {
    coveragePatterns: [`^(${covered.join('|')})$`],
    comment: 'ISB 계열 아이템 이름(ISB·Aishi·Black Division·Chaos Insurgency·SOF). '
        + 'tools/gen-isb-item-names.mjs 가 켜져 있는 서버에게 물어 만든다 — `(Aishi)` 처럼 꼬리표만 붙은 것은 '
        + '**바닐라 한국어를 되쓰고**(영문 이름이 같은 항목을 찾는다. 한국어가 둘이면 쓰지 않는다), '
        + 'ISB 고유 용어는 생성기의 낱말 사전으로 옮긴다. 값에 색 태그·●를 넣지 않는다 — 그건 다른 모드가 '
        + '덧대고 SptKoreanLocale 이 그대로 둔다(preserveDecoration). coveragePatterns 를 이 id 들로 좁힌 것은, '
        + '넓게 적으면 손대지 않은 아이템 이름 1만여 개가 "묶음에서 빠뜨림" 으로 잡히기 때문이다.',
    entries,
    keptAsIs,
};
fs.writeFileSync(OUT, JSON.stringify(dict, null, 2) + '\n');
console.log(`\n→ ${OUT} (항목 ${Object.keys(entries).length}개 / 아이템 ${ids.length}종)`);
