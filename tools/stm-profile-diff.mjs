// 프로필 JSON 두 벌을 아이템 id 수준에서 대조한다 (doc/WORKLOG 계획 3 관문 2~5).
//
// 사용:
//   node tools/stm-profile-diff.mjs <before-dir> [after-dir]
//     after-dir 를 주지 않으면 설치된 SPT 의 user/profiles 를 쓴다.
//
// 보는 것:
//   1. 각 프로필의 아이템 `_id` 집합 — 사라진 것과 끼어든 것 둘 다 (양방향 대조)
//   2. 같은 id 가 부모·슬롯·좌표를 바꾼 것 (입히기/벗기기가 한 일)
//   3. 프로필 사이의 id 충돌 중 **새로 생긴 것** — 장비 편집이 플레이어 스태시를
//      출처로 쓰면 이렇게 보인다. 원래 있던 충돌은 SPT 프로필 틀의 기본 장비다.
// 판정은 하지 않고 사실만 적는다. 관문의 기대값은 WORKLOG 에 있다.

import { readFileSync, readdirSync, existsSync } from 'node:fs';
import { join } from 'node:path';

const beforeDir = process.argv[2];
const afterDir = process.argv[3] ?? 'E:/Spt/SPT_Runtime/user/profiles';
if (!beforeDir) {
    console.error('사용: node tools/stm-profile-diff.mjs <before-dir> [after-dir]');
    process.exit(2);
}

function load(dir, name) {
    const p = join(dir, name);
    if (!existsSync(p)) return null;
    return JSON.parse(readFileSync(p, 'utf8'));
}

// 프로필에서 캐릭터별 아이템 목록을 꺼낸다. 없는 캐릭터는 건너뛴다.
function itemsOf(profile) {
    const out = new Map(); // id -> { who, tpl, parentId, slotId, location }
    for (const who of ['pmc', 'scav']) {
        const items = profile?.characters?.[who]?.Inventory?.items;
        if (!Array.isArray(items)) continue;
        for (const it of items) {
            if (!it?._id) continue;
            out.set(it._id, {
                who,
                tpl: it._tpl,
                parentId: it.parentId ?? null,
                slotId: it.slotId ?? null,
                location: it.location === undefined ? null : JSON.stringify(it.location),
            });
        }
    }
    return out;
}

function nick(profile) {
    return profile?.characters?.pmc?.Info?.Nickname ?? '(이름 없음)';
}

const ROUBLE_TPL = '5449016a4bdc2d6f028b456f';

function roubles(profile) {
    const items = profile?.characters?.pmc?.Inventory?.items ?? [];
    let sum = 0;
    for (const it of items) if (it?._tpl === ROUBLE_TPL) sum += it?.upd?.StackObjectsCount ?? 1;
    return sum;
}

// 한 아이템 id 를 두 개 이상의 프로필이 들고 있는 경우를 모은다.
function clashesOf(byProfile) {
    const owner = new Map();
    const out = new Map(); // id -> 프로필 라벨 목록
    for (const [label, map] of byProfile) {
        for (const id of map.keys()) {
            if (owner.has(id)) {
                if (!out.has(id)) out.set(id, [owner.get(id)]);
                out.get(id).push(label);
            } else {
                owner.set(id, label);
            }
        }
    }
    return out;
}

const names = readdirSync(beforeDir).filter((f) => f.endsWith('.json'));
const beforeItems = new Map();
const afterItems = new Map();
let anyChange = false;

for (const name of names) {
    const before = load(beforeDir, name);
    const after = load(afterDir, name);
    const short = name.slice(0, 8);
    if (!after) {
        console.log(`\n== ${short} ${nick(before)} — after 쪽에 파일이 없다`);
        anyChange = true;
        continue;
    }
    const a = itemsOf(before);
    const b = itemsOf(after);
    beforeItems.set(`${short} ${nick(before)}`, a);
    afterItems.set(`${short} ${nick(after)}`, b);

    const removed = [...a.keys()].filter((id) => !b.has(id));
    const added = [...b.keys()].filter((id) => !a.has(id));
    const moved = [...a.keys()].filter((id) => {
        const x = a.get(id);
        const y = b.get(id);
        if (!y) return false;
        return x.parentId !== y.parentId || x.slotId !== y.slotId || x.location !== y.location;
    });
    const rb = roubles(before);
    const ra = roubles(after);

    const head = `\n== ${short} ${nick(after)} — 아이템 ${a.size} → ${b.size}, 루블 ${rb.toLocaleString()} → ${ra.toLocaleString()}`;
    if (removed.length === 0 && added.length === 0 && moved.length === 0 && rb === ra) {
        console.log(`${head}  (변화 없음)`);
        continue;
    }
    anyChange = true;
    console.log(head);
    for (const id of removed) {
        const x = a.get(id);
        console.log(`  사라짐 ${id} tpl ${x.tpl} [${x.who}] 부모 ${x.parentId} 슬롯 ${x.slotId}`);
    }
    for (const id of added) {
        const y = b.get(id);
        console.log(`  끼어듦 ${id} tpl ${y.tpl} [${y.who}] 부모 ${y.parentId} 슬롯 ${y.slotId} 좌표 ${y.location}`);
    }
    for (const id of moved) {
        const x = a.get(id);
        const y = b.get(id);
        console.log(`  옮겨짐 ${id} tpl ${y.tpl} [${y.who}]`);
        console.log(`         전 부모 ${x.parentId} 슬롯 ${x.slotId} 좌표 ${x.location}`);
        console.log(`         후 부모 ${y.parentId} 슬롯 ${y.slotId} 좌표 ${y.location}`);
    }
}

const beforeClashes = clashesOf(beforeItems);
const afterClashes = clashesOf(afterItems);
const fresh = [...afterClashes.keys()].filter((id) => !beforeClashes.has(id));
console.log(`\n== 프로필 사이의 아이템 id 충돌 (복제 검사) — before ${beforeClashes.size}건, after ${afterClashes.size}건`);
console.log('   before 쪽 충돌은 SPT 프로필 틀이 같은 id 를 쓰는 기본 장비다(같은 id 가 프로필마다 다른 tpl 인 것도 있다).');
if (fresh.length === 0) {
    console.log('  새로 생긴 충돌: 없음');
} else {
    anyChange = true;
    for (const id of fresh) {
        console.log(`  새 충돌 ${id} — ${afterClashes.get(id).join(' / ')} 에 같이 있다`);
    }
}

console.log(`\n대조 끝. 프로필 ${names.length}개, 변화 ${anyChange ? '있음' : '없음'}.`);
