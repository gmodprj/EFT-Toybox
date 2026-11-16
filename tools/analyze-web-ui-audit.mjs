// audit-web-ui.mjs 가 낸 결과를 문서용으로 정리한다.
// 사용: node tools/analyze-web-ui-audit.mjs <audit.json> [모드별 번역 후보를 쓸 파일.json]
//
// 보는 것:
//   1. 모드별 미번역 — 종류별로 갈라서(사람 이름·식별자는 영원히 안 준다)
//   2. 서로 다른 영문이 같은 한글로 — 화면에서 구분이 안 된다
//   3. 조각 열쇠가 쓰인 것 — 쪼개진 노드를 낱개로 바꿔 어순이 깨졌을 수 있다
//   4. 영문+한글이 붙은 아이템 이름 — 런타임 로케일 값 자체가 그렇다
import { readFileSync, writeFileSync } from 'node:fs';
import { BUCKETS, TRANSLATABLE, classify } from './lib/web-ui-classify.mjs';

const [inPath, outPath] = process.argv.slice(2);
if (!inPath) {
  console.error('사용: node tools/analyze-web-ui-audit.mjs <audit.json> [후보.json]');
  process.exit(2);
}
const audit = JSON.parse(readFileSync(inPath, 'utf8'));
const names = [...BUCKETS.map(([n]) => n), TRANSLATABLE];

// 1. 모드별
const rows = [];
const candidates = {};
const totals = Object.fromEntries(names.map((n) => [n, 0]));
for (const [mod, v] of Object.entries(audit)) {
  if (v.error) { rows.push(`| ${mod} | 오류: ${v.error} |`); continue; }
  const uniq = [...new Set(Object.values(v.pages).flatMap((p) => p.missing))];
  const by = Object.fromEntries(names.map((n) => [n, []]));
  for (const s of uniq) by[classify(s)].push(s);
  for (const n of names) totals[n] += by[n].length;
  candidates[mod] = by[TRANSLATABLE];
  rows.push(`| ${mod} | ${Object.keys(v.pages).length}${v.capped ? '+' : ''} | ${uniq.length} | ${names.map((n) => by[n].length).join(' | ')} |`);
}
console.log('## 모드별 미번역\n');
console.log(`| 모드 | 화면 | 미번역 | ${names.join(' | ')} |`);
console.log(`| --- | ---: | ---: | ${names.map(() => '---:').join(' | ')} |`);
console.log(rows.join('\n'));
console.log(`| **합계** | | **${Object.values(totals).reduce((a, b) => a + b, 0)}** | ${names.map((n) => totals[n]).join(' | ')} |`);
console.log('\n화면 수 뒤의 + 는 크롤러 상한(150)에 걸렸다는 뜻이다.');

// 2~4. 적용된 번역 전체
const applied = {};
for (const v of Object.values(audit)) for (const [en, ko] of Object.entries(v.applied ?? {})) applied[en] = ko;

const norm = (s) => s.toLowerCase().replace(/[^a-z0-9 ]/g, '').replace(/s\b/g, '').trim();
const byKo = {};
for (const [en, ko] of Object.entries(applied)) {
  if (ko.length > 15 || /[○●]/.test(en)) continue;
  (byKo[ko] ??= new Map()).set(norm(en), en);
}
const collide = Object.entries(byKo).filter(([, m]) => m.size > 1);
console.log('\n## 서로 다른 영문이 같은 한글로\n');
console.log(collide.length ? collide.map(([ko, m]) => `- **${ko}** ← ${[...m.values()].map((e) => '`' + e + '`').join(', ')}`).join('\n') : '(없음)');

const frag = Object.entries(applied).filter(([en]) =>
  (/^[a-z]/.test(en) && en.split(/\s+/).length <= 2) || /^[)(·,:]|[(·,]$/.test(en) || /:$/.test(en));
console.log('\n## 조각 열쇠가 쓰인 것\n');
console.log(frag.length ? frag.map(([en, ko]) => `- \`${en}\` → ${ko}`).join('\n') : '(없음)');

const bilingual = Object.entries(applied).filter(([en, ko]) => /^[A-Za-z][\w.-]+ [가-힣]/.test(ko) && !/^(SAIN|ORBIT|SPT|ISB|BEAR|USEC|PMC|DWV|ECOTI|NATO|Ref|SIC|HTTP|JSON|ZIP|BSG|GP|AA|Kappa)\b/.test(ko));
console.log(`\n## 영문+한글이 붙은 값 (${bilingual.length}개, 앞 10개)\n`);
console.log(bilingual.slice(0, 10).map(([en, ko]) => `- \`${en}\` → ${ko}`).join('\n') || '(없음)');

if (outPath) {
  writeFileSync(outPath, JSON.stringify(candidates, null, 1), 'utf8');
  console.log(`\n번역 후보 → ${outPath}`);
}
