// 설치본(E:\Spt)의 모드 파일이 해제본(extracted/)의 원본과 같은지 md5 로 대조한다.
// 모드를 업데이트하면 덮어쓰는 파일 가운데 "우리 내용이 든 것"을 찾는 것이 목적이다.
// 사용: node tools/check-installed-vs-extracted.mjs [--json <경로>]
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';

const ROOT = path.resolve(path.dirname(new URL(import.meta.url).pathname.replace(/^\/([A-Z]:)/, '$1')), '..');
const DOC = path.join(ROOT, 'doc');
const SPT = 'E:/Spt';
const jsonAt = process.argv.includes('--json') ? process.argv[process.argv.indexOf('--json') + 1] : null;

// 1) 매니페스트: 모드 id → 이름·버전·단계
const mods = new Map();
for (const f of fs.readdirSync(path.join(DOC, 'download')).filter(f => /^manifest-stage\d+(?:-[A-Za-z0-9]+)?\.json$/.test(f)).sort()) {
  const stage = f.match(/stage(\d+)/)[1];
  for (const m of JSON.parse(fs.readFileSync(path.join(DOC, 'download', f), 'utf8')).mods)
    mods.set(String(m.id), { id: String(m.id), name: m.name, version: m.version, stage, folders: new Set(), files: [] });
}

// 2) 설치 기록
for (const f of fs.readdirSync(path.join(DOC, 'install-records')).filter(f => f.endsWith('.txt')).sort()) {
  for (const line of fs.readFileSync(path.join(DOC, 'install-records', f), 'utf8').replace(/^\uFEFF/, '').split(/\r?\n/)) {
    if (!line.trim()) continue;
    const [folder, rel] = line.split('\t');
    const m = mods.get(folder.split('__')[0]);
    if (!m) continue;
    m.folders.add(folder); m.files.push({ folder, rel });
  }
}

// 3) extracted 안에서 <id>__<폴더> 디렉터리 색인 (단계 폴더 아래에 있을 수도 있다)
//    같은 폴더 이름이 여러 단계에 있으면(업데이트하면 그렇게 된다) **가장 새로 해제한 것**을 쓴다.
//    먼저 찾은 것을 쓰면 `stage8-F` 가 `update-...` 보다 이름이 앞서 옛 버전과 대조해
//    멀쩡한 설치본을 `다름` 으로 적는다(2026-11-14: 1453 MSGA 1.19.1 vs 1.20.0).
const extIndex = new Map();
const extDupes = new Map();
const scan = (dir, depth) => {
  for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
    if (!e.isDirectory()) continue;
    const full = path.join(dir, e.name);
    if (/__/.test(e.name)) {
      const mtime = fs.statSync(full).mtimeMs;
      const prev = extIndex.get(e.name);
      if (!prev) extIndex.set(e.name, { full, mtime });
      else {
        extDupes.set(e.name, (extDupes.get(e.name) ?? 1) + 1);
        if (mtime > prev.mtime) extIndex.set(e.name, { full, mtime });
      }
    } else if (depth < 2) scan(full, depth + 1);
  }
};
scan(path.join(ROOT, 'extracted'), 0);

const md5 = (p) => { try { return crypto.createHash('md5').update(fs.readFileSync(p)).digest('hex'); } catch { return null; } };
const CONFIG = /\.(json|jsonc|json5|cfg|ini|txt|xml|yml|yaml|log|db|snapshot)$/i;

const out = [];
for (const m of mods.values()) {
  if (!m.folders.size) continue;  // 설치 기록이 없는 것은 설치본에 없다(제거됨)
  const r = { id: m.id, name: m.name, version: m.version, stage: m.stage, folders: [...m.folders],
    same: 0, differ: [], newFiles: [], gone: [], noOriginal: 0 };
  for (const { folder, rel } of m.files) {
    const inst = path.join(SPT, rel);
    const ext = extIndex.get(folder)?.full;
    const orig = ext ? path.join(ext, rel) : null;
    if (!fs.existsSync(inst)) { r.gone.push(rel); continue; }
    if (!orig || !fs.existsSync(orig)) { if (!ext) r.noOriginal++; else r.newFiles.push(rel); continue; }
    if (md5(inst) === md5(orig)) r.same++;
    else r.differ.push({ rel, config: CONFIG.test(rel) });
  }
  out.push(r);
}

const differ = out.filter(r => r.differ.length);
const binDiffer = out.filter(r => r.differ.some(d => !d.config));
const noOrig = out.filter(r => r.noOriginal > 0);

console.log(`설치된 모드 ${out.length}개 / 해제본 색인 ${extIndex.size}개 (같은 이름이 여러 단계에 있는 것 ${extDupes.size}개 — 가장 새로 해제한 것과 대조했다)`);
console.log(`[1] 원본과 다른 파일이 있는 모드: ${differ.length}개 (그중 설정 파일이 아닌 것 ${binDiffer.length}개)`);
for (const r of differ) console.log(`    ${r.id} ${r.name} — ${r.differ.map(d => d.rel).join(' | ')}`);
console.log(`[2] 설치 후 새로 생긴 파일이 있는 모드: ${out.filter(r => r.newFiles.length).length}개`);
console.log(`[3] 설치본에서 사라진 파일이 있는 모드: ${out.filter(r => r.gone.length).length}개`);
for (const r of out.filter(r => r.gone.length)) console.log(`    ${r.id} ${r.name} — ${r.gone.length}개`);
console.log(`[4] 해제본이 없어 대조하지 못한 모드: ${noOrig.length}개`);
for (const r of noOrig) console.log(`    ${r.id} ${r.name} (기록 ${r.noOriginal}줄)`);

if (jsonAt) { fs.writeFileSync(jsonAt, JSON.stringify(out, null, 1)); console.log(`\n→ ${jsonAt}`); }
const bad = out.filter(r => r.gone.length).length;
console.log(binDiffer.length === 0 && bad === 0
  ? `\nOK — 설정 아닌 파일이 원본과 다른 모드 0개 / 사라진 파일 0개`
  : `\nATTENTION — 설정 아닌 차이 ${binDiffer.length}개 / 사라진 파일이 있는 모드 ${bad}개`);
