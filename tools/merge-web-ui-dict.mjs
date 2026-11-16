// 번역 패치 파일을 src/ServerWebUiKorean/ko-ui.json 에 합친다.
// 사용: node tools/merge-web-ui-dict.mjs <패치.json> [...]
//
// 패치 꼴: { "dict": {영문: 한글}, "rules": [[정규식, 바꿀 꼴]], "scoped": {경로: {영문: 한글|null}} }
// - 이미 있는 열쇠를 다른 값으로 덮으면 알린다(조용히 바꾸지 않는다 — 용어가 갈리는 곳이다).
// - 규칙은 앞에 넣는다. 새로 넣는 규칙은 구체적이라 일반 규칙보다 먼저 맞아야 한다.
// - 정규식이 깨졌으면 아무것도 쓰지 않는다.
import { readFileSync, writeFileSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const repo = join(dirname(fileURLToPath(import.meta.url)), '..');
const path = join(repo, 'src', 'ServerWebUiKorean', 'ko-ui.json');
const j = JSON.parse(readFileSync(path, 'utf8'));
let added = 0, same = 0, rules = 0, scoped = 0;
const changed = [];

for (const p of process.argv.slice(2)) {
  const patch = JSON.parse(readFileSync(p, 'utf8'));
  for (const [k, v] of Object.entries(patch.dict ?? {})) {
    if (k.startsWith('_')) continue;
    if (j[k] === undefined) { j[k] = v; added++; }
    else if (j[k] === v) same++;
    else { changed.push(`${k} : ${j[k]} → ${v}`); j[k] = v; }
  }
  const newRules = [];
  for (const r of patch.rules ?? []) {
    new RegExp(r[0]); // 깨졌으면 여기서 던진다
    if (j._rules.some((x) => x[0] === r[0])) continue;
    newRules.push(r);
  }
  j._rules = [...newRules, ...j._rules];
  rules += newRules.length;
  for (const [pre, map] of Object.entries(patch.scoped ?? {})) {
    j._scoped[pre] = { ...(j._scoped[pre] ?? {}), ...map };
    scoped += Object.keys(map).length;
  }
}

writeFileSync(path, JSON.stringify(j, null, 2) + '\n', 'utf8');
console.log(`추가 ${added} / 같음 ${same} / 바뀜 ${changed.length} / 규칙 +${rules} / 페이지별 +${scoped}`);
if (changed.length) console.log(changed.map((c) => '  바뀜: ' + c).join('\n'));
console.log(`사전 ${Object.keys(j).filter((k) => !k.startsWith('_')).length} 줄, 규칙 ${j._rules.length} 개`);
