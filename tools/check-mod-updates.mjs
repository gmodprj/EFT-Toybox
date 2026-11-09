// 설치된 Forge 모드의 4.1.6 호환 최신 버전을 조회해 설치 버전과 견준다. (읽기 전용)
// 사용: node tools/check-mod-updates.mjs [--json <경로>]
// 주의: 조회 실패를 "업데이트 없음" 으로 삼키지 않는다 — 실패는 실패로 적고 종료 코드 1 이다.
import fs from 'node:fs';
import path from 'node:path';

const ROOT = path.resolve(path.dirname(new URL(import.meta.url).pathname.replace(/^\/([A-Z]:)/, '$1')), '..');
const DOC = path.join(ROOT, 'doc');
const API = 'https://sp-mod.com/api/v0';
const SPT_VER = '4.1.6';
const jsonAt = process.argv.includes('--json') ? process.argv[process.argv.indexOf('--json') + 1] : null;

// 매니페스트 + 설치 기록: 기록이 있는 것만 "설치된 것" 이다
const mods = new Map();
for (const f of fs.readdirSync(path.join(DOC, 'download')).filter(f => /^manifest-stage\d+(?:-[A-Za-z0-9]+)?\.json$/.test(f)).sort()) {
  const stage = f.match(/stage(\d+)/)[1];
  for (const m of JSON.parse(fs.readFileSync(path.join(DOC, 'download', f), 'utf8')).mods)
    mods.set(String(m.id), { id: String(m.id), name: m.name, installed: m.version, stage, records: 0 });
}
for (const f of fs.readdirSync(path.join(DOC, 'install-records')).filter(f => f.endsWith('.txt'))) {
  for (const line of fs.readFileSync(path.join(DOC, 'install-records', f), 'utf8').replace(/^﻿/, '').split(/\r?\n/)) {
    if (!line.trim()) continue;
    const m = mods.get(line.split('\t')[0].split('__')[0]);
    if (m) m.records++;
  }
}

const installed = [...mods.values()].filter(m => m.records > 0);
const forge = installed.filter(m => /^\d+$/.test(m.id));
const outside = installed.filter(m => !/^\d+$/.test(m.id));
const removed = [...mods.values()].filter(m => m.records === 0);

const sleep = ms => new Promise(s => setTimeout(s, ms));
const get = async (url, tries = 5) => {
  for (let i = 0; i < tries; i++) {
    try {
      const r = await fetch(url, { headers: { Accept: 'application/json' } });
      if (r.status === 429) { await sleep(Number(r.headers.get('retry-after') || 15) * 1000); continue; }
      if (r.status === 404) return { __gone: true };
      if (!r.ok) { await sleep(2000 * (i + 1)); continue; }
      return await r.json();
    } catch { await sleep(3000 * (i + 1)); }
  }
  return { __err: '재시도 소진' };
};
const cmp = (a, b) => {
  const pa = String(a).split(/[.\-+]/).map(x => parseInt(x) || 0), pb = String(b).split(/[.\-+]/).map(x => parseInt(x) || 0);
  for (let i = 0; i < Math.max(pa.length, pb.length); i++) if ((pa[i] || 0) !== (pb[i] || 0)) return (pa[i] || 0) - (pb[i] || 0);
  return 0;
};

const rows = [];
for (const m of forge) {
  const j = await get(`${API}/mod/${m.id}/versions?filter[spt_version]=${SPT_VER}&per_page=100`);
  if (j.__gone) { rows.push({ ...m, state: 'forge 에서 사라짐' }); continue; }
  if (j.__err) { rows.push({ ...m, state: '조회 실패' }); continue; }
  const vs = (j.data ?? []).map(v => ({ v: v.version, pub: v.published_at?.slice(0, 10), link: v.link, mb: v.content_length ? +(v.content_length / 1048576).toFixed(1) : null }))
    .sort((a, b) => cmp(b.v, a.v));
  const top = vs[0];
  if (!top) { rows.push({ ...m, state: `${SPT_VER} 호환 버전 0개` }); continue; }
  const c = cmp(top.v, m.installed);
  rows.push({ ...m, latest: top.v, published: top.pub, link: top.link, mb: top.mb, versions: vs.length,
    state: c > 0 ? '업데이트 있음' : c === 0 ? '최신' : '설치본이 더 높음' });
  await sleep(400);
}

const by = s => rows.filter(r => r.state === s);
const up = by('업데이트 있음');
up.sort((a, b) => (b.published || '').localeCompare(a.published || ''));
console.log(`설치된 모드 ${installed.length}개 = Forge ${forge.length} + Forge 밖 ${outside.length} / 매니페스트에만 있고 제거된 것 ${removed.length}개\n`);
console.log(`업데이트 있음 ${up.length} · 최신 ${by('최신').length} · 설치본이 더 높음 ${by('설치본이 더 높음').length} · 호환 0개 ${by(`${SPT_VER} 호환 버전 0개`).length} · 사라짐 ${by('forge 에서 사라짐').length} · 조회 실패 ${by('조회 실패').length}\n`);
console.log('[업데이트 있음]');
for (const r of up) console.log(`  ${r.id}\t${r.installed} → ${r.latest}\t${r.published}\t${r.mb ?? '?'}MB\t${r.name}`);
for (const s of ['설치본이 더 높음', `${SPT_VER} 호환 버전 0개`, 'forge 에서 사라짐', '조회 실패']) {
  const g = by(s); if (!g.length) continue;
  console.log(`\n[${s}]`);
  for (const r of g) console.log(`  ${r.id}\t${r.installed}\t${r.name}`);
}
if (outside.length) { console.log('\n[Forge 밖 — 손으로 확인한다]'); for (const m of outside) console.log(`  ${m.id}\t${m.installed}\t${m.name}`); }
if (jsonAt) { fs.writeFileSync(jsonAt, JSON.stringify({ rows, outside, removed }, null, 1)); console.log(`\n→ ${jsonAt}`); }

const failed = by('조회 실패').length;
console.log(failed ? `\nATTENTION — 조회 실패 ${failed}개. 다시 돌린다.` : `\nOK — 조회 실패 0개, 업데이트 대상 ${up.length}개`);
process.exit(failed ? 1 : 0);
