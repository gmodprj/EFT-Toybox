// 미번역 문자열을 종류별로 가른다.
// 사용: node tools/classify-web-ui-missing.mjs <dump-web-ui-strings.mjs 가 낸 파일>
import { readFileSync } from 'node:fs';
import { BUCKETS, TRANSLATABLE, classify } from './lib/web-ui-classify.mjs';

const path = process.argv[2];
if (!path) {
  console.error('사용: node tools/classify-web-ui-missing.mjs <파일>');
  process.exit(2);
}

const items = Object.keys(JSON.parse(readFileSync(path, 'utf8')));
const result = new Map([...BUCKETS.map(([n]) => [n, []]), [TRANSLATABLE, []]]);
for (const s of items) result.get(classify(s)).push(s);

for (const [name, list] of result) {
  console.log(`${name.padEnd(16)} ${String(list.length).padStart(5)}`);
  if (name === TRANSLATABLE) for (const s of list) console.log(`    ${s.length > 90 ? s.slice(0, 90) + '…' : s}`);
}
console.log(`\n합계 ${items.length} / ${TRANSLATABLE} ${result.get(TRANSLATABLE).length}`);
