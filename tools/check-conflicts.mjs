// 해제본의 설치 상대경로를 E:\Spt와 비교해 충돌을 검사한다 (읽기 전용).
// 사용: node tools/check-conflicts.mjs stage5-A
import fs from 'node:fs';
import path from 'node:path';

const STAGE = process.argv[2] ?? 'stage5-A';
const SRC = `E:/Spt_Mods/extracted/${STAGE}`;
const GAME = 'E:/Spt';
// E:\Spt 기준 설치 경로로 인정하는 최상위 (이 밖의 파일은 압축에 섞여 온 것)
const ROOTS = ['BepInEx/', 'SPT_Runtime/', 'EscapeFromTarkov_Data/', 'user/', 'doorstop_config.ini'];

const walk = (dir, base = dir) => fs.readdirSync(dir, { withFileTypes: true }).flatMap(e => {
  const p = path.join(dir, e.name);
  return e.isDirectory() ? walk(p, base) : [path.relative(base, p).split(path.sep).join('/')];
});

const mods = fs.readdirSync(SRC, { withFileTypes: true }).filter(e => e.isDirectory() && !e.name.startsWith('_')).map(e => e.name).sort();
const owners = new Map();
const existing = [];
const suspicious = [];

for (const m of mods) {
  for (const rel of walk(path.join(SRC, m))) {
    if (!ROOTS.some(r => rel.startsWith(r))) { suspicious.push([m, rel]); continue; }
    if (!owners.has(rel)) owners.set(rel, []);
    owners.get(rel).push(m);
    if (fs.existsSync(path.join(GAME, rel))) existing.push([m, rel]);
  }
}
const dup = [...owners.entries()].filter(([, v]) => v.length > 1);

const side = (rel) => rel.startsWith('BepInEx/') ? '게임' : rel.startsWith('SPT_Runtime/') ? '서버' : '기타';
const byMod = new Map();
for (const [rel, v] of owners) for (const m of v) {
  if (!byMod.has(m)) byMod.set(m, new Set());
  byMod.get(m).add(side(rel));
}

console.log(`${STAGE}: 모드 ${mods.length}개 / 설치 대상 경로 ${owners.size}개`);
console.log(`\n[1] E:\\Spt에 이미 있는 경로: ${existing.length}`);
for (const [m, rel] of existing) console.log('   ', m, '->', rel);
console.log(`\n[2] 모드끼리 겹치는 경로: ${dup.length}`);
for (const [rel, v] of dup) console.log('   ', rel, '<-', v.join(', '));
console.log(`\n[3] 설치 경로가 아닌 파일(압축에 섞여 온 것): ${suspicious.length}`);
for (const [m, rel] of suspicious) console.log('   ', m, '->', rel);
console.log('\n[4] 모드별 설치 위치');
for (const m of mods) console.log('   ', m, '->', [...(byMod.get(m) ?? ['(없음)'])].join('+'));

const bad = existing.length + dup.length + suspicious.length;
console.log(`\n${bad === 0 ? 'CLEAN' : 'ATTENTION'} (${bad})`);
process.exitCode = bad === 0 ? 0 : 1;
