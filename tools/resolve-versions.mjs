// SPT 4.1.6 호환 최신 버전 + 의존성 조회 (Forge API, 읽기 전용)
const API = 'https://sp-mod.com/api/v0';
const SPT = '4.1.6';
const ids = process.argv.slice(2).map(Number);

const get = async (url) => {
  const r = await fetch(url, { headers: { Accept: 'application/json' } });
  if (!r.ok) throw new Error(`${r.status} ${url}`);
  return r.json();
};

const cmpVer = (a, b) => {
  const pa = a.split(/[.-]/).map(x => parseInt(x) || 0), pb = b.split(/[.-]/).map(x => parseInt(x) || 0);
  for (let i = 0; i < Math.max(pa.length, pb.length); i++) if ((pa[i] || 0) !== (pb[i] || 0)) return (pa[i] || 0) - (pb[i] || 0);
  return 0;
};

const rows = [];
for (const id of ids) {
  const mod = (await get(`${API}/mod/${id}`)).data;
  const vs = (await get(`${API}/mod/${id}/versions?filter[spt_version]=${SPT}&per_page=50&include=dependencies`)).data;
  vs.sort((a, b) => cmpVer(b.version, a.version));
  const v = vs[0];
  rows.push({ id, guid: mod.guid, name: mod.name, version: v?.version ?? null, constraint: v?.spt_version_constraint,
    size: v?.content_length, link: v?.link, fika: v?.fika_compatibility, published: v?.published_at?.slice(0, 10),
    category: mod.category_id, deps: (v?.dependencies ?? []).map(d => d.name ?? d.mod?.name ?? JSON.stringify(d).slice(0, 80)) });
}

const pairs = rows.filter(r => r.version).map(r => `${r.id}:${r.version}`).join(',');
const deps = (await get(`${API}/mods/dependencies?mods=${encodeURIComponent(pairs)}&spt_version=${SPT}`)).data;
const flat = new Map();
const walk = (nodes, owner) => nodes.forEach(n => {
  const k = n.id;
  const e = flat.get(k) ?? { id: n.id, name: n.name, guid: n.guid, version: n.latest_compatible_version?.version ?? null,
    size: n.latest_compatible_version?.content_length, link: n.latest_compatible_version?.link, conflict: n.conflict, by: new Set() };
  e.by.add(owner); e.conflict ||= n.conflict; flat.set(k, e); walk(n.dependencies ?? [], owner);
});
for (const [pair, nodes] of Object.entries(deps)) walk(nodes, rows.find(r => pair.startsWith(r.id + ':'))?.name ?? pair);

console.log(JSON.stringify({ mods: rows, dependencies: [...flat.values()].map(e => ({ ...e, by: [...e.by] })) }, null, 2));
