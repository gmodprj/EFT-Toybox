// 문구→한국어 지도를 **열쇠→한국어 사전**으로 펴낸다.
//
// 사용:
//   node tools/expand-locale-dict.mjs quest-objectives "^[0-9a-f]{24}$"
//   ... --locale kr --chunk 700 --dry
//
// 왜 이것이 필요한가
//   퀘스트 목표는 열쇠 4,923개가 서로 다른 문구 4,036개를 쓴다 — 470개 문구를 1,357개 열쇠가
//   나눠 쓴다. 열쇠마다 손으로 적으면 같은 문구를 887번 더 적어야 하고, 그러면 표기가 갈린다.
//   그래서 **문구 단위로 번역**하고(`lang-src/<이름>.json`), 그것을 열쇠 사전으로 펴낸다.
//
// 모드가 읽는 것은 펴낸 결과(`lang/<이름>-NN.json`)다. 그 파일은 **생성물이라 직접 고치지 않는다.**
//
// 짝이 맞지 않을 때
//   모드가 업데이트돼 어떤 열쇠의 영문이 바뀌면 그 열쇠는 지도에 걸리지 않아 **그냥 빠진다**.
//   그러면 `check-locale-dict.mjs` 가 "묶음에서 빠뜨림" 으로 잡는다 — 조용히 틀리지 않는다.

import { readFileSync, writeFileSync, existsSync, readdirSync, unlinkSync } from 'node:fs';
import { deflateSync, inflateSync, inflateRawSync, gunzipSync } from 'node:zlib';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const HERE = path.dirname(fileURLToPath(import.meta.url));
const SRC_DIR = path.join(HERE, '..', 'src', 'SptKoreanLocale', 'lang-src');
const OUT_DIR = path.join(HERE, '..', 'src', 'SptKoreanLocale', 'lang');

const args = process.argv.slice(2);
const name = args[0];
const pattern = args[1];
if (!name || !pattern) {
    console.error('사용: node tools/expand-locale-dict.mjs <이름> "<열쇠 정규식>" [--locale kr] [--chunk 700] [--dry]');
    process.exit(2);
}
const opt = (k, d) => { const i = args.indexOf('--' + k); return i >= 0 && args[i + 1] ? args[i + 1] : d; };
const locale = opt('locale', 'kr');
const chunk = Number(opt('chunk', '700'));
const dry = args.includes('--dry');

const mapFile = path.join(SRC_DIR, name + '.json');
if (!existsSync(mapFile)) { console.error(`지도가 없다: ${mapFile}`); process.exit(2); }
const raw = JSON.parse(readFileSync(mapFile, 'utf8').replace(/^﻿/, ''));
const texts = raw.texts ?? raw;   // { "영문": "한국어" }

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
async function ask(code) {
    const res = await fetch(`https://127.0.0.1:6969/client/locale/${encodeURIComponent(code)}`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: deflateSync(Buffer.from('{}', 'utf8')),
    });
    const buf = Buffer.from(await res.arrayBuffer());
    for (const f of [inflateSync, inflateRawSync, gunzipSync]) {
        try { return JSON.parse(f(buf).toString('utf8')).data; } catch { /* 다음 */ }
    }
    return JSON.parse(buf.toString('utf8')).data;
}

let served, english;
try {
    served = await ask(locale);
    english = locale === 'en' ? null : await ask('en');
} catch (err) {
    console.error(`서버에 묻지 못했다 — ${err.message}`);
    console.error('SPT 서버가 켜져 있어야 한다(https://127.0.0.1:6969).');
    process.exit(2);
}
// 안전장치는 닫힌 쪽으로 — 그 언어가 없으면 서버가 en 을 돌려준다.
if (english) {
    let diff = 0;
    for (const k of Object.keys(served)) if (served[k] !== english[k] && ++diff > 3) break;
    if (Object.keys(served).length === Object.keys(english).length && diff === 0) {
        console.error(`!  '${locale}' 응답이 'en' 과 글자까지 같다 — 그 locale 이 없어 en 으로 떨어진 것이다.`);
        process.exit(2);
    }
}

// 짝은 **en locale** 을 기준으로 맞춘다. 대상 locale(kr)을 기준으로 하면 **앞서 펴낸 것이 이미
// 한국어라 짝이 안 맞아 통째로 빠진다** — 한 번 그렇게 956개를 잃었다. en 은 우리가 건드리지 않으므로
// 몇 번 펴내도 같은 결과가 나온다(idempotent).
const HANGUL = /[\uAC00-\uD7A3\u1100-\u11FF\u3130-\u318F]/;
const re = new RegExp(pattern);
const reference = english ?? served;

// 누가 번역해 두었는지는 **기본 파일이 아니라 지금 서버 값**으로 가린다.
// 기본 파일(바닐라 kr.json)로 가리면, 바닐라가 번역했는데 **모드가 영문으로 덮어 버린** 열쇠를
// "이미 번역됐다" 로 잘못 보고 건너뛴다(실측 1건: 60e745d6479eef59b01b0bdc — 바닐라가 한국어로
// 들고 있었으나 모드가 en·kr 둘 다 같은 영문으로 덮었다). 지금 서버 값이 영문이면 우리 일이다.
const byText = new Map();
let englishKeys = 0;
for (const [k, v] of Object.entries(reference)) {
    if (!re.test(k) || typeof v !== 'string') continue;
    if (HANGUL.test(v)) continue;            // en 쪽이 이미 한국어면 우리 일이 아니다
    if (english && !(k in served)) continue; // en 에만 있고 대상 locale 에 없는 열쇠
    englishKeys++;
    if (!byText.has(v)) byText.set(v, []);
    byText.get(v).push(k);
}

// 이미 다른 사전이 맡은 열쇠는 비켜 둔다(겹쳐 적으면 어느 쪽이 이기는지 헷갈린다).
// 이 이름으로 앞서 펴낸 파일은 다시 쓸 것이라 세지 않는다.
const mine = new RegExp(`^${name}-\\d+\\.json$`);
const taken = new Set();
const ourPrevious = new Set();
for (const f of readdirSync(OUT_DIR).filter(x => x.endsWith('.json') && !x.startsWith('_'))) {
    const o = JSON.parse(readFileSync(path.join(OUT_DIR, f), 'utf8').replace(/^\uFEFF/, ''));
    const into = mine.test(f) ? ourPrevious : taken;
    for (const k of Object.keys(o.entries ?? {})) into.add(k);
    for (const k of Object.keys(o.keptAsIs ?? {})) into.add(k);
}

// 우리도 바닐라도 아닌 **다른 모드**가 대상 locale 을 한국어로 바꿔 둔 열쇠는 비켜 둔다.
// 우리 것인지 가리는 근거 둘: ① 이 이름으로 앞서 펴낸 파일에 있다 ② 지금 값이 이 지도의 한국어
// 가운데 하나다. ②가 필요한 까닭은, 서버에는 적용돼 있는데 파일에서는 지워진 상태가 있을 수
// 있어서다(펴내기를 잘못해 잃은 뒤 다시 펴낼 때).
const ourValues = new Set(Object.values(texts));
if (english) {
    for (const [k, v] of Object.entries(served)) {
        if (!re.test(k) || typeof v !== 'string') continue;
        if (!HANGUL.test(v)) continue;
        if (ourPrevious.has(k) || ourValues.has(v)) continue;          // 우리가 바꾼 것이다
        taken.add(k);   // 바닐라든 다른 모드든, 지금 한국어로 들고 있으면 남의 일이다
    }
}

const entries = {};
let matchedTexts = 0;
const unusedTexts = [];
for (const [en, ko] of Object.entries(texts)) {
    const keys = byText.get(en);
    if (!keys) { unusedTexts.push(en); continue; }
    matchedTexts++;
    for (const k of keys) {
        if (taken.has(k)) continue;
        entries[k] = ko;
    }
}

const keyList = Object.keys(entries).sort();
console.log(`지도 '${name}': 문구 ${Object.keys(texts).length}개`);
console.log(`묶음 "${pattern}" 의 영문 열쇠 ${englishKeys}개 / 서로 다른 문구 ${byText.size}개`);
console.log(`짝이 맞은 문구 ${matchedTexts}개 → 열쇠 ${keyList.length}개`);
if (unusedTexts.length) {
    console.log(`!  지도에만 있고 지금 서버에 없는 문구 ${unusedTexts.length}개 (모드가 바뀌었거나 오타다):`);
    for (const t of unusedTexts.slice(0, 8)) console.log(`     ${t.slice(0, 80)}`);
    if (unusedTexts.length > 8) console.log(`     … (+${unusedTexts.length - 8})`);
}
console.log(`남는 영문 열쇠 ${englishKeys - keyList.length}개`);

if (dry) { console.log('(--dry 라 파일을 쓰지 않았다)'); process.exit(0); }

// 이 이름으로 앞서 펴낸 파일은 지우고 다시 쓴다.
for (const f of readdirSync(OUT_DIR).filter(x => mine.test(x))) unlinkSync(path.join(OUT_DIR, f));

// coveragePatterns 는 "이 묶음을 우리가 다 맡았다" 는 선언이다. 묶음을 아직 다 적지 않았으면
// 그 선언을 넣지 않는다 — 넣으면 검사 도구가 남은 것을 전부 "빠뜨림" 으로 세고, 진행 중인 일과
// 진짜 누락이 구별되지 않는다. 진행 상황은 `dump-untranslated.mjs --pending` 으로 본다.
const claim = args.includes('--claim-coverage');
const header = (part, total) => ({
    _generated: `tools/expand-locale-dict.mjs ${name} "${pattern}" 가 만든 파일이다. 직접 고치지 말고 src/SptKoreanLocale/lang-src/${name}.json 을 고친 뒤 다시 펴낸다.`,
    ...(claim ? { coveragePatterns: [pattern] } : {}),
    comment: `${raw.comment ?? ''} (펴낸 조각 ${part}/${total})`.trim(),
});

const parts = Math.max(1, Math.ceil(keyList.length / chunk));
for (let p = 0; p < parts; p++) {
    const slice = keyList.slice(p * chunk, (p + 1) * chunk);
    const obj = { ...header(p + 1, parts), entries: {} };
    for (const k of slice) obj.entries[k] = entries[k];
    const out = path.join(OUT_DIR, `${name}-${p + 1}.json`);
    writeFileSync(out, JSON.stringify(obj, null, 2) + '\n');
    console.log(`  적었다: lang/${name}-${p + 1}.json (${slice.length}개)`);
}
console.log('서버 모드라 서버를 다시 켜야 적용된다.');
