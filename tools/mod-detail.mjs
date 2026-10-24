const API='https://sp-mod.com/api/v0';
const wait=ms=>new Promise(r=>setTimeout(r,ms));
const get=async u=>{for(let i=0;;i++){const r=await fetch(u,{headers:{Accept:'application/json'}});if(r.status===429&&i<6){await wait(15000);continue}if(!r.ok)throw new Error(r.status+' '+u);await wait(500);return r.json()}};
const cmp=(a,b)=>{const pa=a.split(/[.-]/).map(x=>parseInt(x)||0),pb=b.split(/[.-]/).map(x=>parseInt(x)||0);for(let i=0;i<Math.max(pa.length,pb.length);i++)if((pa[i]||0)!==(pb[i]||0))return(pa[i]||0)-(pb[i]||0);return 0};
const strip=h=>(h||'').replace(/<[^>]+>/g,' ').replace(/&[a-z#0-9]+;/g,' ').replace(/\s+/g,' ').trim();
for(const id of process.argv.slice(2).map(Number)){
  const mod=(await get(`${API}/mod/${id}`)).data;
  const vs=(await get(`${API}/mod/${id}/versions?filter[spt_version]=4.1.6&per_page=50&include=dependencies`)).data.sort((a,b)=>cmp(b.version,a.version));
  const all=(await get(`${API}/mod/${id}/versions?per_page=5&sort=-published_at`)).data;
  const v=vs[0];
  console.log(`\n== ${id} ${mod.name} | 다운로드 ${mod.downloads} | 결속 ${mod.shows_profile_binding_notice} | ${mod.detail_url}`);
  console.log(`   4.1.6 호환 최신: ${v?`${v.version} (${v.spt_version_constraint}, ${v.published_at?.slice(0,10)}, ${v.content_length}B, fika:${v.fika_compatibility})`:'없음'}`);
  console.log(`   최근 버전들: ${all.map(x=>x.version+'['+x.spt_version_constraint+']').join(', ')}`);
  console.log(`   의존성: ${(v?.dependencies||[]).map(d=>d.id+':'+d.name).join(', ')||'-'}`);
  console.log(`   설명: ${strip(mod.description).slice(0,700)}`);
  if(v) console.log(`   최신 변경: ${strip(v.description).slice(0,300)}`);
}
