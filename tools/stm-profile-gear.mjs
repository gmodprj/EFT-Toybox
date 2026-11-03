// 프로필 하나의 착용 장비와 스태시 최상단 내용을 이름까지 붙여 적는다.
// 장비 편집 관문(계획 3 관문 2~5·9)에서 "무엇을 입혀 볼지" 와 "왼쪽 칸에 무엇이 보여야 하는지"를
// 게임을 켜기 전에 정하려고 쓴다.
//
// 사용:
//   node tools/stm-profile-gear.mjs <닉네임 또는 프로필 id> [--stash N] [--slot Headwear]
//     --stash N  스태시 최상단 항목을 N 개까지 적는다(기본 25)
//     --slot X   그 장비 슬롯에 넣을 수 있는 범주의 스태시 항목만 적는다(이름으로 거른다)

import { readFileSync, readdirSync } from 'node:fs';
import { join } from 'node:path';

const SPT = 'E:/Spt/SPT_Runtime';
const PROFILES = join(SPT, 'user/profiles');
const DB = join(SPT, 'SPT_Data/database');

const who = process.argv[2];
if (!who) {
    console.error('사용: node tools/stm-profile-gear.mjs <닉네임 또는 프로필 id> [--stash N] [--slot Headwear]');
    process.exit(2);
}
const stashLimit = Number(process.argv.includes('--stash') ? process.argv[process.argv.indexOf('--stash') + 1] : 25);
const slotFilter = process.argv.includes('--slot') ? process.argv[process.argv.indexOf('--slot') + 1] : null;

// 프로필 찾기 — 파일 이름이든 닉네임이든 받는다.
let found = null;
for (const f of readdirSync(PROFILES).filter((x) => x.endsWith('.json'))) {
    const p = JSON.parse(readFileSync(join(PROFILES, f), 'utf8'));
    const nickname = p?.characters?.pmc?.Info?.Nickname ?? '';
    if (f.startsWith(who) || nickname.toLowerCase() === who.toLowerCase()) {
        found = { file: f, profile: p };
        break;
    }
}
if (!found) {
    console.error(`그런 프로필이 없다: ${who}`);
    process.exit(1);
}

const items = JSON.parse(readFileSync(join(DB, 'templates/items.json'), 'utf8'));
const locale = JSON.parse(readFileSync(join(DB, 'locales/global/en.json'), 'utf8'));
const nameOf = (tpl) => locale[`${tpl} ShortName`] || locale[`${tpl} Name`] || items[tpl]?._name || '(이름 모름)';
const parentNameOf = (tpl) => {
    const parent = items[tpl]?._parent;
    return parent ? items[parent]?._name ?? parent : '(범주 모름)';
};

const pmc = found.profile.characters.pmc;
const inv = pmc.Inventory;
const all = inv.items;
const byId = new Map(all.map((i) => [i._id, i]));
const childrenOf = (id) => all.filter((i) => i.parentId === id);

console.log(`# ${pmc.Info.Nickname} (${found.file.slice(0, 8)}, aid ${found.profile.info.aid}, lvl ${pmc.Info.Level})`);
console.log(`아이템 ${all.length}개. equipment ${inv.equipment}, stash ${inv.stash}`);

console.log('\n## 착용 장비 (equipment 의 자식)');
const worn = childrenOf(inv.equipment);
if (worn.length === 0) console.log('  (없음 — 맨몸이다)');
for (const it of worn.sort((a, b) => String(a.slotId).localeCompare(String(b.slotId)))) {
    const kids = all.filter((x) => x.parentId === it._id).length;
    console.log(`  ${String(it.slotId).padEnd(16)} ${nameOf(it._tpl).padEnd(34)} ${it._tpl}  (자식 ${kids})`);
}

console.log(`\n## 스태시 최상단 (stash 의 자식, 최대 ${stashLimit}개)`);
let rows = childrenOf(inv.stash).map((it) => ({
    name: nameOf(it._tpl),
    cat: parentNameOf(it._tpl),
    tpl: it._tpl,
    id: it._id,
    stack: it.upd?.StackObjectsCount ?? 1,
    loc: it.location === undefined ? '(좌표 없음)' : JSON.stringify(it.location),
}));
console.log(`  최상단 ${rows.length}개 (그 안의 내용물까지 합치면 ${all.length - 1 - worn.length}개 남짓)`);
if (slotFilter) {
    const needle = slotFilter.toLowerCase();
    rows = rows.filter((r) => r.cat.toLowerCase().includes(needle) || r.name.toLowerCase().includes(needle));
    console.log(`  --slot ${slotFilter} 로 걸러 ${rows.length}개`);
}
for (const r of rows.slice(0, stashLimit)) {
    console.log(`  ${r.name.padEnd(30)} ${r.cat.padEnd(22)} x${String(r.stack).padEnd(6)} ${r.id} ${r.loc}`);
}
