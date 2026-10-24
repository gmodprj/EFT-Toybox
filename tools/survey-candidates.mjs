// 2차 후보 조회: 4.1.6 호환 최신 버전, 크기, 프로필 결속/치트 표시, 직접 의존성 (Forge API 읽기 전용)
import fs from 'node:fs';
const API = 'https://sp-mod.com/api/v0', SPT = '4.1.6';
const groups = JSON.parse(process.argv[2]);
const wait = (ms) => new Promise(r => setTimeout(r, ms));
const get = async (u, tries = 6) => {
  for (let i = 0; ; i++) {
    const r = await fetch(u, { headers: { Accept: 'application/json' } });
    if (r.status === 429 && i < tries) { await wait(Number(r.headers.get('retry-after')) * 1000 || 15000); continue; }
    if (!r.ok) throw new Error(`${r.status} ${u}`);
    await wait(700);
    return r.json();
  }
};
const cmp = (a, b) => { const pa = a.split(/[.-]/).map(x => parseInt(x) || 0), pb = b.split(/[.-]/).map(x => parseInt(x) || 0);
  for (let i = 0; i < Math.max(pa.length, pb.length); i++) if ((pa[i] || 0) !== (pb[i] || 0)) return (pa[i] || 0) - (pb[i] || 0); return 0; };
const strip = (h) => (h || '').replace(/<[^>]+>/g, ' ').replace(/\s+/g, ' ').trim();

const out = [];
for (const [group, ids] of Object.entries(groups)) for (const id of ids) {
  try {
    const mod = (await get(`${API}/mod/${id}`)).data;
    const vs = (await get(`${API}/mod/${id}/versions?filter[spt_version]=${SPT}&per_page=50&include=dependencies`)).data;
    vs.sort((a, b) => cmp(b.version, a.version)); const v = vs[0];
    out.push({ group, id, name: mod.name, guid: mod.guid, version: v?.version ?? null, size: v?.content_length ?? null,
      downloads: mod.downloads, profileBinding: mod.shows_profile_binding_notice, cheat: mod.cheat_notice, ads: mod.contains_ads,
      updated: v?.published_at?.slice(0, 10), link: v?.link,
      deps: (v?.dependencies ?? []).map(d => `${d.id}:${d.name}`), teaser: strip(mod.teaser).slice(0, 160) });
  } catch (e) { out.push({ group, id, error: String(e) }); }
}
fs.writeFileSync(process.argv[3], JSON.stringify(out, null, 2));
for (const m of out) console.log(m.error ? `ERR ${m.id} ${m.error}` :
  [m.group, m.id, m.name, m.version ?? 'NO-4.1.6', m.size ? Math.round(m.size / 1024) + 'KB' : '?', m.profileBinding ? 'BIND' : '-', m.cheat ? 'CHEAT' : '-', m.deps.join('|') || '-'].join(' ; '));
