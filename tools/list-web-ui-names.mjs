// 분류기가 "사람·모드 이름"으로 넣은 한 낱말들을 모드별로 보여 준다.
// 한 낱말 영어(Rewards, Repeatable…)는 진짜 라벨일 수 있어 사람이 한 번 훑어야 한다.
// 사용: node tools/list-web-ui-names.mjs <audit.json> [모드 ...]
import { readFileSync } from 'node:fs';
import { classify } from './lib/web-ui-classify.mjs';
const [p, ...mods] = process.argv.slice(2);
const a = JSON.parse(readFileSync(p, 'utf8'));
for (const [m, v] of Object.entries(a)) {
  if (mods.length && !mods.includes(m)) continue;
  if (!v.pages) continue;
  const u = [...new Set(Object.values(v.pages).flatMap((x) => x.missing))].filter((s) => classify(s) === '사람·모드 이름');
  console.log(`### ${m} (${u.length})\n${u.join(' | ')}\n`);
}
