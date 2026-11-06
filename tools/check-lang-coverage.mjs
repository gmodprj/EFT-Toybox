// 번역 사전이 실제 설정 항목을 빠짐없이 덮는지 확인한다.
//   node tools/check-lang-coverage.mjs            전체 요약
//   node tools/check-lang-coverage.mjs <GUID>     그 모드의 빠진 항목까지
//
// 기준: E:\Spt\BepInEx\config\<GUID>.cfg 의 [섹션] 과 `키 = 값`
// 사전: src/SptKoreanConfig/lang/<GUID>.json
//
// 두 쪽에서 센다. 사전 쪽만 돌면 **사전이 아예 없는 모드**는 목록에 나타나지도 않는다 —
// STM(`xyz.stm.teammanager`)이 PIT 포크로 GUID 가 바뀌면서 사전이 조용히 끊겼는데도 이 도구가
// OK 만 찍고 있었던 것이 그 때문이다(2026-11-02). 그래서 설치된 .cfg 쪽에서도 한 바퀴 돌아
// 사전이 없는 모드를 따로 알린다.

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
// 밑줄로 시작하는 파일(_ui.json)은 모드 사전이 아니다. 플러그인도 똑같이 건너뛴다.
const langFiles = existsSync(LANG_DIR)
  ? readdirSync(LANG_DIR).filter((f) => f.endsWith('.json') && !f.startsWith('_'))
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

// 설치된 .cfg 쪽에서 한 바퀴: 사전이 아예 없는 모드를 찾는다.
const haveDict = new Set(langFiles.map((f) => basename(f, '.json')));
const cfgGuids = existsSync(CONFIG_DIR)
  ? readdirSync(CONFIG_DIR)
      .filter((f) => f.endsWith('.cfg'))
      .map((f) => basename(f, '.cfg'))
  : [];
const noDict = cfgGuids.filter((g) => !haveDict.has(g) && (!only || g === only));

if (noDict.length > 0) {
  // 우리가 만든 모드는 설정 이름 자체가 한글이라 사전이 필요 없다. 그 둘을 구별해서 알린다.
  const rowsNoDict = noDict.map((g) => {
    const keys = readCfg(join(CONFIG_DIR, `${g}.cfg`));
    const alreadyKorean = keys.length > 0 && keys.every((k) => /[가-힣]/.test(k));
    return { guid: g, count: keys.length, alreadyKorean };
  });
  const needs = rowsNoDict.filter((r) => !r.alreadyKorean);

  console.log(`\n사전이 없는 모드 ${rowsNoDict.length}개 — 그중 번역이 필요한 것 ${needs.length}개`);
  for (const r of rowsNoDict) {
    console.log(
      `    ${r.alreadyKorean ? 'OK ' : '！ '} ${r.guid} — 항목 ${r.count}개, 사전 파일 없음` +
        (r.alreadyKorean ? ' (원문이 이미 한글이다)' : ''),
    );
  }
}

if (!only) {
  console.log(
    `\n합계: ${totalDone}/${totalKeys} 항목, 사전 ${rows.length}개` +
      `, 사전 없는 모드 ${noDict.length}개 / 설치된 .cfg ${cfgGuids.length}개`,
  );
}
