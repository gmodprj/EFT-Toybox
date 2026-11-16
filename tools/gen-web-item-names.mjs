// 화면에 나오는 아이템 이름을 영문 → 한글로 바꾸는 사전을 만든다.
// 사용: node tools/gen-web-item-names.mjs [SPT 설치 경로]
//
// 왜 필요한가: 모드 페이지(특히 Barter Items Stacks)와 데이터베이스 탐색기는
// 아이템 이름을 'en' 글로벌 로케일에서 읽는다. 게임이 한국어여도 그 화면은 영문이다.
// 손으로 쓸 분량이 아니므로 kr 로케일에서 짝을 지어 뽑는다.
//
// 같은 영문이 서로 다른 한글로 갈리면 **버린다**. 화면에서 엉뚱한 것으로 바뀌느니
// 영문으로 남는 편이 낫다.
import { readFileSync, writeFileSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const repo = join(dirname(fileURLToPath(import.meta.url)), '..');
const sptDir = process.argv[2] ?? 'E:\\Spt';
const globalDir = join(sptDir, 'SPT_Runtime', 'SPT_Data', 'database', 'locales', 'global');
const out = join(repo, 'src', 'ServerWebUiKorean', 'ko-items.json');

const read = (name) => JSON.parse(readFileSync(join(globalDir, name), 'utf8'));
const en = read('en.json');
const kr = read('kr.json');

// 아이템 이름 열쇠만 본다. 설명(Description)은 화면에 통째로 나오지 않고 분량만 키운다.
const SUFFIX = /(^| )(Name|ShortName)$/;

const pairs = new Map(); // 영문 → Set(한글)
let considered = 0;

for (const [key, enValue] of Object.entries(en)) {
  if (!SUFFIX.test(key)) continue;
  const krValue = kr[key];
  if (typeof enValue !== 'string' || typeof krValue !== 'string') continue;

  const e = enValue.trim();
  const k = krValue.trim();
  if (!e || !k || e === k) continue;
  if (!/[A-Za-z]/.test(e)) continue; // 영문이 없으면 바꿀 것도 없다
  if (!/[가-힣]/.test(k)) continue; // 한글이 아니면 번역이 아니다

  // 짧은 한 낱말은 버린다. "Case"·"Buck"·"Core" 처럼 UI 라벨이나 사람 이름과
  // 부딪쳐 엉뚱한 자리를 바꿔 놓는다. 아이템 하나를 놓치는 편이 낫다.
  if (!/[\s(]/.test(e) && e.length <= 6) continue;

  considered++;
  if (!pairs.has(e)) pairs.set(e, new Set());
  pairs.get(e).add(k);
}

const dict = {};
let ambiguous = 0;
for (const [e, set] of pairs) {
  if (set.size > 1) {
    ambiguous++;
    continue;
  }
  dict[e] = [...set][0];
}

const sorted = Object.fromEntries(Object.keys(dict).sort((a, b) => a.localeCompare(b)).map((k) => [k, dict[k]]));
writeFileSync(out, JSON.stringify(sorted, null, 0), 'utf8');

console.log(`en 열쇠에서 이름 ${considered} 개를 봤다`);
console.log(`영문 ${pairs.size} 종 / 갈려서 버린 것 ${ambiguous} 종`);
console.log(`사전 ${Object.keys(sorted).length} 줄 → ${out}`);
