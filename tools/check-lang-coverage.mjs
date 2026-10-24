// 번역 사전이 실제 설정 항목을 빠짐없이 덮는지 확인한다.
//   node tools/check-lang-coverage.mjs            전체 요약
//   node tools/check-lang-coverage.mjs <GUID>     그 모드의 빠진 항목까지
//
// 기준: E:\Spt\BepInEx\config\<GUID>.cfg 의 [섹션] 과 `키 = 값`
// 사전: src/SptKoreanConfig/lang/<GUID>.json

import { readFileSync, readdirSync, existsSync } from 'node:fs';
import { join, basename } from 'node:path';

const SPT_DIR = process.env.SPT_DIR ?? 'E:\\Spt';
const CONFIG_DIR = join(SPT_DIR, 'BepInEx', 'config');
const LANG_DIR = join(process.cwd(), 'src', 'SptKoreanConfig', 'lang');

/** .cfg 에서 "섹션/키" 목록을 뽑는다. */
function readCfg(path) {
  const out = [];
  let section = '';
  for (const raw of readFileSync(path, 'utf8').split(/\r?\n/)) {
    const line = raw.replace(/\r$/, '');
    const trimmed = line.trim();
    if (trimmed.startsWith('[') && trimmed.endsWith(']')) {
      section = trimmed.slice(1, -1);
      continue;
    }
    if (!trimmed || trimmed.startsWith('#')) continue;
    // 값이 비어 있는 줄("Use Item Shortcut = ")도 항목이다. 먼저 trim 하면 놓친다.
    const m = line.match(/^(.+?) = ?(.*)$/);
    if (!m) continue;
    out.push(`${section}/${m[1]}`);
  }
  return out;
}

const only = process.argv[2];
const langFiles = existsSync(LANG_DIR)
  ? readdirSync(LANG_DIR).filter((f) => f.endsWith('.json'))
  : [];

let totalKeys = 0;
let totalDone = 0;
const rows = [];

for (const file of langFiles) {
  const guid = basename(file, '.json');
  if (only && guid !== only) continue;

  const lang = JSON.parse(readFileSync(join(LANG_DIR, file), 'utf8'));
  const cfgPath = join(CONFIG_DIR, `${guid}.cfg`);

  if (!existsSync(cfgPath)) {
    rows.push({ guid, note: 'cfg 없음 — 아직 설정을 만들지 않은 모드일 수 있다' });
    continue;
  }

  const keys = readCfg(cfgPath);
  const sections = [...new Set(keys.map((k) => k.split('/')[0]))];
  const have = new Set(Object.keys(lang.settings ?? {}));
  const haveSections = new Set(Object.keys(lang.sections ?? {}));

  const missing = keys.filter((k) => !have.has(k));
  const missingSections = sections.filter((s) => !haveSections.has(s));
  const extra = [...have].filter((k) => !keys.includes(k));
  const noDesc = keys.filter((k) => have.has(k) && !lang.settings[k]?.desc);

  totalKeys += keys.length;
  totalDone += keys.length - missing.length;

  rows.push({
    guid,
    name: lang.name ?? '(이름 없음)',
    done: keys.length - missing.length,
    total: keys.length,
    missing,
    missingSections,
    extra,
    noDesc: noDesc.length,
  });
}

for (const r of rows) {
  if (r.note) {
    console.log(`\n${r.guid} — ${r.note}`);
    continue;
  }
  const ok = r.missing.length === 0 && r.missingSections.length === 0 && r.extra.length === 0;
  console.log(
    `\n${ok ? 'OK ' : '！ '} ${r.guid} — ${r.name}` +
      `\n    항목 ${r.done}/${r.total}` +
      (r.noDesc ? `, 설명 없음 ${r.noDesc}` : '') +
      (r.missingSections.length ? `, 섹션 누락 ${r.missingSections.length}` : '') +
      (r.extra.length ? `, cfg 에 없는 열쇠 ${r.extra.length}` : ''),
  );
  if (only) {
    for (const s of r.missingSections) console.log(`    섹션 누락: ${s}`);
    for (const k of r.missing) console.log(`    누락: ${k}`);
    for (const k of r.extra) console.log(`    불일치(오타 의심): ${k}`);
  }
}

if (!only) {
  console.log(`\n합계: ${totalDone}/${totalKeys} 항목, 사전 ${rows.length}개`);
}
