#!/usr/bin/env node
// 내보낸 porter 파일이 **격자 자리를 들고 있는지** 센다.
//
// 왜 이 검사가 따로 있나 — 설계 §20 의 결함은 `PorterFileItem` 에 `location` 필드가 **없던
// 것**이라, 단위 시험이 보는 순수 판정(`PorterItem`·`ImportPlanner`)에서는 보이지 않았다.
// 개수·`upd`·`slotId` 는 다 맞았고 시험 20건은 전부 녹색이었다. **파일에 적힌 것**을
// 프로필과 맞대 봐야 드러난다.
//
// 쓰는 법:  node tools/check-porter-location.mjs
//          node tools/check-porter-location.mjs <porter 폴더> <프로필 폴더>
//
// 판정: 내보낸 파일의 각 아이템을 **보낸 프로필의 같은 id** 와 맞댄다(내보내기는 id 를 바꾸지
// 않는다). 원본에 `location` 이 있는데 파일에 없으면 **구멍**이다. 프로필에서 그 id 를 더 이상
// 못 찾으면(그 뒤로 쓰거나 팔았거나) 셈에서 빼고 그 수를 따로 적는다.

import fs from 'node:fs';
import path from 'node:path';

const PORTER = process.argv[2] ?? 'E:/Spt/SPT_Runtime/user/mods/ProfilePorter/porter';
const PROFILES = process.argv[3] ?? 'E:/Spt/SPT_Runtime/user/profiles';

function readJson(p) {
  try {
    return JSON.parse(fs.readFileSync(p, 'utf8'));
  } catch (e) {
    console.log(`못 읽었다 — ${p}: ${e.message}`);
    return null;
  }
}

function profileItems() {
  const byProfile = new Map();
  if (!fs.existsSync(PROFILES)) return byProfile;
  for (const f of fs.readdirSync(PROFILES).filter((x) => /^[0-9a-f]{24}\.json$/.test(x))) {
    const p = readJson(path.join(PROFILES, f));
    const items = p?.characters?.pmc?.Inventory?.items;
    if (!items) continue;
    byProfile.set(f.replace(/\.json$/, ''), new Map(items.map((i) => [i._id, i])));
  }
  return byProfile;
}

const profiles = profileItems();
if (!fs.existsSync(PORTER)) {
  console.log(`내보낸 파일 폴더가 없다 — ${PORTER}`);
  console.log('OK — 0/0 (볼 것이 없다)');
  process.exit(0);
}

const files = fs.readdirSync(PORTER).filter((x) => x.endsWith('.json')).sort();
let holes = 0;
let badFiles = 0;

for (const name of files) {
  const file = readJson(path.join(PORTER, name));
  if (!file?.items) {
    console.log(`건너뜀 ${name} — items 가 없다`);
    continue;
  }

  const source = profiles.get(file.fromProfileId);
  if (!source) {
    console.log(`건너뜀 ${name} — 보낸 프로필(${file.fromProfileId})을 못 찾았다`);
    continue;
  }

  let matched = 0;
  let unknown = 0;
  let expected = 0;
  let carried = 0;
  let missing = 0;

  for (const item of file.items) {
    const origin = source.get(item._id);
    if (!origin) {
      unknown++;
      continue;
    }
    matched++;
    if (origin.location == null) continue;
    expected++;
    if (item.location == null) missing++;
    else carried++;
  }

  const mark = missing === 0 ? 'OK  ' : '구멍';
  console.log(
    `${mark} ${name}\n     아이템 ${file.items.length} / 대조됨 ${matched}`
      + `${unknown ? ` / 프로필에 없는 id ${unknown}` : ''}`
      + ` — 자리 있어야 할 것 ${expected}, 들고 있는 것 ${carried}, 빠진 것 ${missing}`,
  );

  holes += missing;
  if (missing > 0) badFiles++;
}

console.log(holes === 0 ? `OK — 0/${files.length}` : `구멍 ${holes}개 / 파일 ${badFiles}개`);
process.exit(holes === 0 ? 0 : 1);
