const API='https://sp-mod.com/api/v0';
const wait=ms=>new Promise(r=>setTimeout(r,ms));
const get=async u=>{for(let i=0;;i++){const r=await fetch(u,{headers:{Accept:'application/json'}});if(r.status===429&&i<6){await wait(15000);continue}if(!r.ok)throw new Error(r.status+' '+u);await wait(500);return r.json()}};
const terms=process.argv.slice(2);const seen=new Map();
for(const t of terms)for(const f of ['name','teaser']){
  const j=await get(`${API}/mods?filter[${f}]=${encodeURIComponent(t)}&per_page=50&fields=name,slug,teaser,downloads,updated_at`);
  for(const m of j.data){if(!seen.has(m.id))seen.set(m.id,{...m,hit:t})}
}
for(const m of [...seen.values()].sort((a,b)=>b.downloads-a.downloads))console.log([m.id,m.name,m.downloads,(m.updated_at||'').slice(0,10),'['+m.hit+']',(m.teaser||'').replace(/\s+/g,' ').slice(0,130)].join(' | '));
