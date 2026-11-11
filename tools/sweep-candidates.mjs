// 4.1.6 호환 모드 전체를 카테고리까지 받아 와 정규식으로 추리고, 이미 설치된 것을 뺀다. (읽기 전용)
// 사용: node tools/sweep-candidates.mjs "<정규식>" [--json <경로>]
// 이름·소개만 보는 search-mods.mjs 와 달리 목록 전체를 받아 오므로 키워드에 걸리지 않는 것도 센다.
// 설치 여부는 doc/INSTALLED-MODS.md 의 Forge 링크로 판정한다(그 문서는 설치 기록에서 생성된다).
import fs from 'node:fs';
import path from 'node:path';

const ROOT = path.resolve(path.dirname(new URL(import.meta.url).pathname.replace(/^\/([A-Z]:)/, '$1')), '..');
const API = 'https://sp-mod.com/api/v0';
const SPT = '4.1.6';

const pattern = process.argv[2];
if (!pattern) { console.error('정규식을 주세요. 보기: node tools/sweep-candidates.mjs "(memory|vram|cull)"'); process.exit(2); }
const jsonAt = process.argv.includes('--json') ? process.argv[process.argv.indexOf('--json') + 1] : null;
const re = new RegExp(pattern, 'i');

const wait = ms => new Promise(r => setTimeout(r, ms));
const get = async u => {
  for (let i = 0; ; i++) {
    const r = await fetch(u, { headers: { Accept: 'application/json' } });
    if (r.status === 429 && i < 8) { await wait(Number(r.headers.get('retry-after') || 15) * 1000); continue; }
    if (!r.ok) throw new Error(`${r.status} ${u}`);
    await wait(400);
    return r.json();
  }
};

const all = [];
for (let p = 1; ; p++) {
  const j = await get(`${API}/mods?filter[spt_version]=${SPT}&per_page=100&page=${p}&include=category`);
  all.push(...j.data);
  if (!j.links?.next || j.data.length === 0) break;
}

const hits = all.filter(m => re.test(`${m.name} ${m.teaser || ''} ${m.category?.title || ''}`));
const md = fs.readFileSync(path.join(ROOT, 'doc', 'INSTALLED-MODS.md'), 'utf8');
const installed = new Set([...md.matchAll(/sp-mod\.com\/mod\/(\d+)/g)].map(m => m[1]));
const out = hits.filter(m => !installed.has(String(m.id))).sort((a, b) => b.downloads - a.downloads);

console.log(`${SPT} 호환 전체 ${all.length}개 / 적중 ${hits.length}개 / 그중 미설치 ${out.length}개\n`);
for (const m of out)
  console.log([m.id, m.name, m.category?.title || '-', m.downloads, (m.updated_at || '').slice(0, 10),
    (m.teaser || '').replace(/\s+/g, ' ').slice(0, 110)].join(' | '));

if (jsonAt) fs.writeFileSync(jsonAt, JSON.stringify({ pattern, total: all.length, hits: hits.length, mods: out }, null, 2));
