// 미번역 문자열을 종류별로 가른다.
// 사용: node tools/classify-web-ui-missing.mjs <dump-web-ui-strings.mjs 가 낸 파일>
//
// 왜 필요한가: 남은 수만 보면 판단을 그르친다. 사람 이름과 설정 키는 **바꾸면 안 되는 것**이라
// 영원히 줄지 않는다. "아직 363개"가 아니라 "번역할 것은 몇 개"를 봐야 한다.
import { readFileSync } from 'node:fs';

const path = process.argv[2];
if (!path) {
  console.error('사용: node tools/classify-web-ui-missing.mjs <파일>');
  process.exit(2);
}

const items = Object.keys(JSON.parse(readFileSync(path, 'utf8')));

// 위에서부터 먼저 맞는 갈래로 들어간다.
const buckets = [
  ['식별자 (ID·키·파일명)', [
    /^[0-9a-f]{24}$/,
    /^(item|patron|stock|tarkov|monster|weapon|mag)_[a-z0-9_]+/i,
    /^BotZone\w*$/,
    /^Desert_Mag_Drum$/,
    /\.(json|jsonc|dll|js|css)$/i,
    /^\/[a-z0-9/.-]+$/i,
  ]],
  ['링크·버전', [/^https?:/, /^v?\d+\.\d+(\.\d+)?$/, /^(GitHub|sp-mod|VirusTotal|HTTP|en|spt|SPT)$/]],
  ['치수·수치', [/^\d+\s*[xX]\s*\d+$/, /^[-+0-9.,xX×%\s|]+$/]],
  ['아이템 이름 (한글 이름 없음)', [
    /[○●]\s*$/,
    /\b(backpack|Pack|bag|Satchel|figurine|textbook|magazine|barrel|box|box \(|wrench|drill|torch|tablets|board|stick|components|converter|materials|notes|Medal|kit|handwheel|survey maps|flash drive|Sledgehammer|sledgehammer)\b/i,
    /^!!!DO/,
    /^(WD-40|Can of|Car battery|CPU fan|PC CPU|RAM stick|Soap|Awl|Mace|Carry Case|Expeditionary Case|Thumb drive)/,
    /^\.?\d*\s*\.?\d*\s*(mm|x\d)/,
  ]],
  ['사람·모드 이름', [
    /^[A-Za-z][A-Za-z0-9_.-]*$/,
    /^(ISB|WTT|SPT|The|Lots of Loot|Definitive|Enable|Cultist|Barter|Skills|Mystery|Default with)/,
    /\(White Tusk\)$/,
  ]],
];

const result = new Map(buckets.map(([name]) => [name, []]));
result.set('그 밖 (번역 후보)', []);

for (const s of items) {
  const hit = buckets.find(([, pats]) => pats.some((r) => r.test(s)));
  result.get(hit ? hit[0] : '그 밖 (번역 후보)').push(s);
}

let translatable = 0;
for (const [name, list] of result) {
  console.log(`${name.padEnd(28)} ${String(list.length).padStart(4)}`);
  if (name.startsWith('그 밖')) {
    translatable = list.length;
    for (const s of list) console.log(`    ${s.length > 90 ? s.slice(0, 90) + '…' : s}`);
  }
}

console.log(`\n합계 ${items.length} / 번역 후보 ${translatable}`);
