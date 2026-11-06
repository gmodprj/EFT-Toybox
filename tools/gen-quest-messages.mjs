// 퀘스트 **대사·메일**(상인이 보내는 글) 사전을 만든다.
//
// 열쇠 꼴: `<24자리 id> <종류>` — 종류는 successMessageText / successMessagetext(소문자 t) /
// startedMessageText / failMessageText / changeQuestMessageText / acceptPlayerMessage /
// declinePlayerMessage / completePlayerMessage. 뒤에 상인 id 등이 더 붙는 것도 있다
// (`successMessageText 54cb50c76803fa8b248b4571 0`).
//
// 사용:
//   node tools/gen-quest-messages.mjs --dry           세어만 본다
//   node tools/gen-quest-messages.mjs --dump <파일>    아직 번역이 없는 것을 뽑는다
//   node tools/gen-quest-messages.mjs                 사전을 펴낸다
//
// 설명(gen-quest-descriptions.mjs)과 다른 점: 여기에는 ExpandedTaskText 가 붙이는 머리말이 없다.
// 글 하나하나가 통째로 번역 대상이고, 열쇠마다 글이 달라 문구→열쇠로 펴낼 것이 없다.
// 그래서 `lang-src/quest-messages.json` 의 `byKey` 에 **열쇠 전체**를 적는다(한 퀘스트가 여러 종류의
// 글을 가지므로 id 만으로는 가를 수 없다).
//
// 기준은 **`en`** 이다. `kr` 로 "이미 한국어인가" 를 판정하면 멱등하지 않다 — 한 번 설치한 뒤에는
// 그 `kr` 이 우리가 넣은 한국어라서, 우리 것을 남의 것으로 보고 걸러내 사전에서 빠진다.

import { readFileSync, writeFileSync, existsSync, readdirSync, unlinkSync } from 'node:fs';
import { deflateSync, inflateSync, inflateRawSync, gunzipSync } from 'node:zlib';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const HERE = path.dirname(fileURLToPath(import.meta.url));
const SRC_DIR = path.join(HERE, '..', 'src', 'SptKoreanLocale', 'lang-src');
const OUT_DIR = path.join(HERE, '..', 'src', 'SptKoreanLocale', 'lang');
const NAME = 'quest-messages';

const args = process.argv.slice(2);
const opt = (k, d) => { const i = args.indexOf('--' + k); return i >= 0 && args[i + 1] ? args[i + 1] : d; };
const locale = opt('locale', 'kr');
const chunk = Number(opt('chunk', '700'));
const dry = args.includes('--dry');
const dumpTo = args.includes('--dump') ? opt('dump', null) : null;

const HANGUL = /[\uAC00-\uD7A3]/;
const KIND = 'successMessageText|successMessagetext|startedMessageText|failMessageText|failMessagetext'
    + '|changeQuestMessageText|acceptPlayerMessage|declinePlayerMessage|completePlayerMessage';
const KEY = new RegExp(`^[0-9a-f]{24} (?:${KIND})(?: .+)?$`);

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
async function ask(code) {
    const res = await fetch(`https://127.0.0.1:6969/client/locale/${code}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: deflateSync(Buffer.from('{}', 'utf8')),
    });
    const raw = Buffer.from(await res.arrayBuffer());
    for (const f of [inflateSync, inflateRawSync, gunzipSync]) {
        try { return JSON.parse(f(raw).toString('utf8')).data; } catch { /* 다음 */ }
    }
    return JSON.parse(raw.toString('utf8')).data;
}

let srvEn, srvKr;
try { srvEn = await ask('en'); srvKr = await ask(locale); }
catch (err) {
    console.error(`서버에 묻지 못했다 — ${err.message}`);
    console.error('SPT 서버가 켜져 있어야 한다(https://127.0.0.1:6969).');
    process.exit(2);
}
// 안전장치는 닫힌 쪽으로 — 그 locale 이 없으면 서버가 en 을 돌려준다.
if (locale !== 'en') {
    let diff = 0;
    for (const k of Object.keys(srvKr)) if (srvKr[k] !== srvEn[k] && ++diff > 3) break;
    if (diff === 0) { console.error(`!  '${locale}' 응답이 'en' 과 같다 — 그 locale 이 없다.`); process.exit(2); }
}

const BASE = 'E:/Spt/SPT_Runtime/SPT_Data/database/locales/global';
const baseEn = JSON.parse(readFileSync(`${BASE}/en.json`, 'utf8'));
const baseKr = JSON.parse(readFileSync(`${BASE}/${locale}.json`, 'utf8'));

const mapFile = path.join(SRC_DIR, NAME + '.json');
const hand = existsSync(mapFile)
    ? JSON.parse(readFileSync(mapFile, 'utf8').replace(/^\uFEFF/, ''))
    : { comment: '', byKey: {}, keptAsIs: {} };
const handBody = hand.byKey ?? {};
const handKeep = hand.keptAsIs ?? {};

// 지난번에 우리가 펴낸 열쇠를 기억한다. 되쓰기(바닐라 한국어)로 채운 항목은 설치하고 나면
// 서버의 `kr` 이 한국어가 되므로, "kr 이 영문일 때만 되쓴다" 라는 규칙만으로는 다시 펴낼 때
// 그 항목이 빠진다(설명 묶음에서 494개가 그렇게 빠졌다). 그래서 **우리 소유를 사전에 적어 둔
// `sources` 로 판정**한다 — 영문 원본이 바뀌면 check-locale-dict 가 "원본이 바뀜" 으로 알린다.
const prevOurs = new Set();
for (const f of readdirSync(OUT_DIR)) {
    if (!new RegExp(`^${NAME}-\\d+\\.json$`).test(f)) continue;
    const d = JSON.parse(readFileSync(path.join(OUT_DIR, f), 'utf8').replace(/^﻿/, ''));
    for (const k of Object.keys(d.sources ?? {})) prevOurs.add(k);
    for (const k of Object.keys(d.entries ?? {})) prevOurs.add(k);
}

// 바닐라 공식 한국어를 되쓸 수 있는가 — 서버 영문이 기본 en 과 같고 기본 kr 에 한국어가 있을 때.
function reusableKr(key) {
    const bEn = (baseEn[key] ?? '').trim();
    const bKr = (baseKr[key] ?? '').trim();
    if (!bEn || !HANGUL.test(bKr)) return null;
    return (srvEn[key] ?? '').trim() === bEn ? bKr : null;
}

const entries = {}, sources = {}, kept = {};
const stat = { reused: 0, hand: 0, pending: 0, alreadyKorean: 0, empty: 0, kept: 0 };
const pending = [];

for (const [key, v] of Object.entries(srvEn)) {
    if (!KEY.test(key) || typeof v !== 'string') continue;
    if (!v.trim()) { stat.empty++; continue; }
    if (handKeep[key]) { kept[key] = handKeep[key]; stat.kept++; continue; }

    const krIsKorean = HANGUL.test(String(srvKr[key] ?? ''));
    const reuse = reusableKr(key);
    // 되쓰기는 **지금 kr 이 영문일 때만** 새로 집는다. 이미 한국어로 나오는 것은 고칠 것이 없다.
    // 한 번 집은 것은 prevOurs 에 남아 다시 펴낼 때도 유지된다.
    const ours = Boolean(handBody[key]) || prevOurs.has(key) || (Boolean(reuse) && !krIsKorean);
    if (!ours) { if (krIsKorean) stat.alreadyKorean++; else { stat.pending++; pending.push({ key, chars: v.length, en: v }); } continue; }

    const ko = handBody[key] ? handBody[key].trim() : reuse;
    if (!ko) {
        stat.pending++;
        pending.push({ key, chars: v.length, en: v });
        continue;
    }
    stat[handBody[key] ? 'hand' : 'reused']++;
    entries[key] = ko;
    sources[key] = v;
}

console.log(`대사·메일 열쇠 — 이미 한국어 ${stat.alreadyKorean} / 이번에 적은 것 ${Object.keys(entries).length} / 아직 없는 것 ${stat.pending}`);
console.log(`  바닐라 한국어를 되쓴 것 ${stat.reused}`);
console.log(`  손으로 적은 것          ${stat.hand}`);
if (stat.kept) console.log(`  일부러 영문으로 둔 것    ${stat.kept}`);
if (stat.empty) console.log(`  값이 비어 할 일이 없는 것 ${stat.empty}`);
const pc = pending.reduce((s, p) => s + p.chars, 0);
console.log(`  남은 글 ${pending.length}개 / 영문 ${pc.toLocaleString()}자 / 평균 ${pending.length ? Math.round(pc / pending.length) : 0}자`);

if (dumpTo) {
    pending.sort((a, b) => a.chars - b.chars);
    writeFileSync(dumpTo, JSON.stringify(pending, null, 1));
    console.log(`  적었다: ${dumpTo} (짧은 것부터)`);
}

if (dry) { console.log('--dry 라 파일은 쓰지 않았다.'); process.exit(0); }

for (const f of readdirSync(OUT_DIR)) {
    if (new RegExp(`^${NAME}-\\d+\\.json$`).test(f)) unlinkSync(path.join(OUT_DIR, f));
}
const keys = Object.keys(entries);
if (!keys.length) { console.log('적을 것이 없다.'); process.exit(0); }
for (let n = 0, c = 1; n < keys.length; n += chunk, c++) {
    const slice = keys.slice(n, n + chunk);
    const out = {
        comment: `퀘스트 대사·메일 — tools/gen-quest-messages.mjs 가 만든다(직접 고치지 않는다). `
            + `본문은 바닐라 ${locale}.json 의 한국어를 되쓰거나 lang-src/${NAME}.json 의 byKey 에 손으로 적은 것을 쓴다.`,
        entries: Object.fromEntries(slice.map(k => [k, entries[k]])),
        sources: Object.fromEntries(slice.map(k => [k, sources[k]])),
        ...(c === 1 && Object.keys(kept).length ? { keptAsIs: kept } : {}),
    };
    const p = path.join(OUT_DIR, `${NAME}-${c}.json`);
    writeFileSync(p, JSON.stringify(out, null, 2) + '\n');
    console.log(`  적었다: lang/${NAME}-${c}.json (${slice.length}개)`);
}
console.log('서버 모드라 서버를 다시 켜야 적용된다.');
