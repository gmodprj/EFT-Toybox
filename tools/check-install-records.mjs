// 설치 기록이 설치본과 맞는지 검사한다 (읽기 전용).
//   node tools/check-install-records.mjs          요약과 문제 목록
//   node tools/check-install-records.mjs --json   기계가 읽을 형태
//
// 두 가지를 본다.
//   [1] 기록에 있는데 E:\Spt 에 없는 경로 — 철 지난 줄이거나 실제 유실
//   [2] 서로 다른 모드가 같은 경로를 기록한 것 — 설치 스크립트는 덮어쓰기를 막으므로
//       복사로는 생길 수 없다. 생겼다면 서버가 만든 파일을 설치 후 스캔이 그때 설치 중이던
//       모드 몫으로 적은 것이다(2026-11-06 에 TheQuartermaster 건으로 실측했다).
//
// 왜 필요한가: 모드가 들어올 때마다 **다른 모드의 기록에도** 구멍이 난다. 설치 단계를
// 끝낼 때마다 돌려 0/0 인지 보는 것이 맞다.
import fs from 'node:fs';
import path from 'node:path';

const REPO = 'E:/Spt_Mods';
const DIR = path.join(REPO, 'doc/install-records');
const GAME = process.env.SPT_DIR ?? 'E:/Spt';
const BS = String.fromCharCode(92);
const asJson = process.argv.includes('--json');

const files = fs.readdirSync(DIR).filter((f) => f.startsWith('installed-') && f.endsWith('.txt'));
const owners = new Map(); // 정규화 경로 -> Set(모드 폴더 이름)
const perMod = new Map(); // 모드 폴더 이름 -> { n, missing: [] }
let lines = 0;
let malformed = 0;

for (const f of files) {
  for (const raw of fs.readFileSync(path.join(DIR, f), 'utf8').split(/\r?\n/)) {
    if (!raw.trim()) continue;
    const i = raw.indexOf('\t');
    if (i < 0) { malformed++; continue; }
    const mod = raw.slice(0, i);
    const rel = raw.slice(i + 1).replace(/\r$/, '');
    lines++;

    const key = rel.toLowerCase().split(BS).join('/');
    if (!owners.has(key)) owners.set(key, new Set());
    owners.get(key).add(mod);

    if (!perMod.has(mod)) perMod.set(mod, { n: 0, missing: [] });
    const m = perMod.get(mod);
    m.n++;
    if (!fs.existsSync(path.join(GAME, rel.split(BS).join('/')))) m.missing.push(rel);
  }
}

const dupes = [...owners.entries()].filter(([, v]) => v.size > 1).map(([k, v]) => ({ path: k, mods: [...v] }));
const badMods = [...perMod.entries()].filter(([, m]) => m.missing.length).map(([mod, m]) => ({ mod, n: m.n, missing: m.missing }));
const missingTotal = badMods.reduce((a, b) => a + b.missing.length, 0);

if (asJson) {
  console.log(JSON.stringify({ files: files.length, lines, mods: perMod.size, malformed, missingTotal, badMods, dupes }, null, 2));
} else {
  console.log(`기록 파일 ${files.length}개 / 줄 ${lines} / 모드 ${perMod.size}개 / 형식 깨진 줄 ${malformed}`);
  console.log(`[1] 기록에 있는데 설치본에 없는 경로: ${missingTotal}개 (모드 ${badMods.length}개)`);
  for (const b of badMods.sort((x, y) => y.missing.length - x.missing.length)) {
    console.log(`    ${b.mod}  기록 ${b.n} / 없는 것 ${b.missing.length}`);
    for (const p of b.missing) console.log(`      ${p}`);
  }
  console.log(`[2] 서로 다른 모드가 같은 경로를 기록: ${dupes.length}건`);
  for (const d of dupes) console.log(`    ${d.path}\n      <- ${d.mods.join(', ')}`);
  console.log(missingTotal === 0 && dupes.length === 0 && malformed === 0 ? 'OK — 0/0' : '확인이 필요하다');
}

process.exitCode = missingTotal === 0 && dupes.length === 0 && malformed === 0 ? 0 : 1;
