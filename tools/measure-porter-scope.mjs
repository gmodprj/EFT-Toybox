#!/usr/bin/env node
// ProfilePorter(요청 4) P1 — "재는 것".
//
// 설계: doc/specs/2026-11-08-profile-porter-design.md §7 (P1) 과 §8(미확인 사항).
// 베낄 범위가 실제로 얼마나 되고, 무엇을 들고 가야 "가진 것 그대로" 가 되는지를 **프로필
// 파일에서 직접** 센다. 추측으로 상한을 넣지 않기 위한 단계다.
//
//   node tools/measure-porter-scope.mjs            아홉 프로필 요약
//   node tools/measure-porter-scope.mjs --upd      upd 필드별 빈도까지
//   node tools/measure-porter-scope.mjs --json     기계가 읽을 형태로
//
// **읽기만 한다.** 프로필을 열지도 고치지도 않는다.

import { readFileSync, readdirSync } from 'node:fs';
import { join } from 'node:path';

const PROFILES = process.env.SPT_PROFILES ?? 'E:/Spt/SPT_Runtime/user/profiles';
const ITEMS_DB = process.env.SPT_ITEMS ?? 'E:/Spt/SPT_Runtime/SPT_Data/database/templates/items.json';

// 스태시 바닥(주 격자)의 칸 이름. STM 의 StashTransferPolicy.StashSlot 과 같은 값이다 —
// 두 모드가 "바닥에 놓인 것" 을 다르게 세면 안 된다.
const STASH_SLOT = 'hideout';

const args = process.argv.slice(2);
const showUpd = args.includes('--upd');
const asJson = args.includes('--json');

const db = JSON.parse(readFileSync(ITEMS_DB, 'utf8'));

/** 한 아이템과 그 자손을 전부 모은다. 부모→자식 색인을 미리 만들어 한 번만 훑는다. */
function collectDescendants(rootIds, byParent) {
  const out = [];
  const queue = [...rootIds];
  const seen = new Set(rootIds);
  let depth = 0;
  let frontier = rootIds.length;
  let nextFrontier = 0;
  while (queue.length > 0) {
    const id = queue.shift();
    out.push(id);
    for (const child of byParent.get(id) ?? []) {
      if (seen.has(child)) continue; // 고리가 있으면 멈춘다 — 무한 루프로 가지 않는다
      seen.add(child);
      queue.push(child);
      nextFrontier++;
    }
    if (--frontier === 0 && nextFrontier > 0) {
      depth++;
      frontier = nextFrontier;
      nextFrontier = 0;
    }
  }
  return { ids: out, depth };
}

const rows = [];
const updFieldCounts = new Map();
let updTotal = 0;

for (const file of readdirSync(PROFILES).filter((f) => /^[0-9a-f]{24}\.json$/.test(f))) {
  const profile = JSON.parse(readFileSync(join(PROFILES, file), 'utf8'));
  const pmc = profile?.characters?.pmc;
  if (!pmc?.Inventory) continue;

  const inv = pmc.Inventory;
  const items = inv.items ?? [];
  const byId = new Map(items.map((i) => [i._id, i]));
  const byParent = new Map();
  for (const i of items) {
    if (!i.parentId) continue;
    if (!byParent.has(i.parentId)) byParent.set(i.parentId, []);
    byParent.get(i.parentId).push(i._id);
  }

  // 스태시 바닥에 놓인 것 = 스태시 루트의 자식 중 hideout 칸에 있는 것
  const floorIds = items.filter((i) => i.parentId === inv.stash && i.slotId === STASH_SLOT).map((i) => i._id);
  const { ids: withChildren, depth } = collectDescendants(floorIds, byParent);

  // 장비 한 벌 = equipment 루트 아래 전부. 보안 컨테이너는 따로 센다(§4.5 가 빼기로 한 것).
  const equipTop = items.filter((i) => i.parentId === inv.equipment);
  const securedTop = equipTop.filter((i) => i.slotId === 'SecuredContainer').map((i) => i._id);
  const loadout = collectDescendants(equipTop.map((i) => i._id), byParent);
  const secured = collectDescendants(securedTop, byParent);

  // 퀘스트 물품은 별도 루트에 있다 — 스태시 바닥을 세면 섞이지 않는지 확인하는 값이다.
  const questRoots = [inv.questRaidItems, inv.questStashItems].filter(Boolean);
  const quest = collectDescendants(
    items.filter((i) => questRoots.includes(i.parentId)).map((i) => i._id),
    byParent,
  );

  // 묶인 것(스택)과 그 최대치. §8 의 "스택 최대치" 를 재는 자리다.
  let stacked = 0;
  let overMax = 0;
  for (const id of withChildren) {
    const it = byId.get(id);
    const n = it?.upd?.StackObjectsCount;
    if (!(n > 1)) continue;
    stacked++;
    const max = db[it._tpl]?._props?.StackMaxSize;
    if (max && n > max) overMax++;
  }

  if (showUpd || asJson) {
    for (const id of withChildren) {
      const upd = byId.get(id)?.upd;
      if (!upd) continue;
      updTotal++;
      for (const k of Object.keys(upd)) updFieldCounts.set(k, (updFieldCounts.get(k) ?? 0) + 1);
    }
  }

  rows.push({
    nickname: pmc.Info?.Nickname ?? '?',
    items: items.length,
    floor: floorIds.length,
    floorWithChildren: withChildren.length,
    depth,
    loadout: loadout.ids.length,
    secured: secured.ids.length,
    quest: quest.ids.length,
    stacked,
    overMax,
  });
}

if (asJson) {
  console.log(JSON.stringify({ rows, upd: Object.fromEntries(updFieldCounts), updTotal }, null, 2));
} else {
  const pad = (s, n) => String(s).padStart(n);
  console.log('프로필      아이템  바닥  바닥+안  깊이   한벌  보안함  퀘스트   묶음  최대초과');
  for (const r of rows) {
    console.log(
      `${r.nickname.padEnd(10)}${pad(r.items, 7)}${pad(r.floor, 6)}${pad(r.floorWithChildren, 9)}` +
        `${pad(r.depth, 6)}${pad(r.loadout, 7)}${pad(r.secured, 8)}${pad(r.quest, 8)}` +
        `${pad(r.stacked, 7)}${pad(r.overMax, 10)}`,
    );
  }
  if (showUpd) {
    console.log('\nupd 필드 (스태시 바닥+안에 든 것 기준, 가진 아이템 ' + updTotal + '개)');
    [...updFieldCounts.entries()]
      .sort((a, b) => b[1] - a[1])
      .forEach(([k, n]) => console.log(`  ${k.padEnd(22)} ${n}`));
  }
}
