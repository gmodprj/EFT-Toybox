// 퀘스트 **설명**(`<24자리 id> description`) 사전을 만든다.
//
// 사용:
//   node tools/gen-quest-descriptions.mjs                 사전을 펴낸다
//   node tools/gen-quest-descriptions.mjs --dry           세어만 본다
//   node tools/gen-quest-descriptions.mjs --dump <파일>    본문이 아직 없는 것을 뽑아 적는다(번역 거리)
//   ... --locale kr --chunk 700
//
// 왜 목표(expand-locale-dict)와 따로인가
//   설명은 **열쇠마다 글이 다르다** — 문구를 나눠 쓰는 일이 없어 문구→열쇠로 펴낼 것이 없다.
//   대신 설명에는 ExpandedTaskText 가 붙인 **머리말**이 있다. 실측: 미번역 설명의 글자 가운데
//   머리말이 8%, 본문이 92%다. 머리말은 고정 어휘 6꼴 + 이름 목록뿐이라 **규칙으로 옮길 수 있고**,
//   그 안의 퀘스트·열쇠 이름은 서버가 이미 한국어로 내려주는 것을 되쓴다.
//
//   본문은 셋으로 갈린다.
//     1) 바닐라 본문을 그대로 쓸 수 있는 것 — ExpandedTaskText 가 kr 에 **영문을 덮어썼을** 뿐이고
//        `locales/global/kr.json` 에는 공식 한국어가 그대로 있다. 되쓴다(번역하지 않는다).
//     2) 손으로 번역한 것 — `lang-src/quest-descriptions.json` 의 `byKey` 에 **본문만** 적는다.
//        머리말은 이 도구가 붙인다.
//     3) 아직 없는 것 — **건드리지 않는다.** 머리말만 한국어로 바꿔 두면 `dump-untranslated` 가
//        한글이 섞인 것을 보고 "다 했다" 로 세어, 남은 본문을 놓친다.
//
// 열쇠마다 값을 통째로 덮으므로 모드 구성이 바뀌면 머리말이 낡는다. 그래서 펴낸 사전에
// `sources`(그때의 영문 원본)를 함께 적어 `check-locale-dict.mjs` 가 어긋남을 알리게 한다.

import { readFileSync, writeFileSync, existsSync, readdirSync, unlinkSync } from 'node:fs';
import { deflateSync, inflateSync, inflateRawSync, gunzipSync } from 'node:zlib';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const HERE = path.dirname(fileURLToPath(import.meta.url));
const SRC_DIR = path.join(HERE, '..', 'src', 'SptKoreanLocale', 'lang-src');
const OUT_DIR = path.join(HERE, '..', 'src', 'SptKoreanLocale', 'lang');
const NAME = 'quest-descriptions';

const args = process.argv.slice(2);
const opt = (k, d) => { const i = args.indexOf('--' + k); return i >= 0 && args[i + 1] ? args[i + 1] : d; };
const locale = opt('locale', 'kr');
const chunk = Number(opt('chunk', '700'));
const dry = args.includes('--dry');
const dumpTo = args.includes('--dump') ? opt('dump', null) : null;

const HANGUL = /[\uAC00-\uD7A3]/;
const KEY = /^([0-9a-f]{24}) description$/;

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
try {
    srvEn = await ask('en');
    srvKr = await ask(locale);
} catch (err) {
    console.error(`서버에 묻지 못했다 — ${err.message}`);
    console.error('SPT 서버가 켜져 있어야 한다(https://127.0.0.1:6969).');
    process.exit(2);
}
// 안전장치는 닫힌 쪽으로 — 그 locale 이 없으면 서버가 en 을 돌려준다.
if (locale !== 'en' && Object.keys(srvKr).length === Object.keys(srvEn).length) {
    let diff = 0;
    for (const k of Object.keys(srvKr)) if (srvKr[k] !== srvEn[k] && ++diff > 3) break;
    if (diff === 0) { console.error(`!  '${locale}' 응답이 'en' 과 같다 — 그 locale 이 없다.`); process.exit(2); }
}

const BASE = `E:/Spt/SPT_Runtime/SPT_Data/database/locales/global`;
const baseEn = JSON.parse(readFileSync(`${BASE}/en.json`, 'utf8'));
const baseKr = JSON.parse(readFileSync(`${BASE}/${locale}.json`, 'utf8'));

// --- 이름 사전: 서버가 내려주는 영문 이름 → 한국어 이름 (이미 한국어인 것만)
function nameMap(suffixRe) {
    const m = new Map();
    for (const [k, v] of Object.entries(srvKr)) {
        if (!suffixRe.test(k)) continue;
        const en = srvEn[k];
        if (typeof en !== 'string' || typeof v !== 'string') continue;
        const a = en.trim(), b = v.trim();
        if (!a || !b || a === b || !HANGUL.test(b)) continue;
        if (!m.has(a)) m.set(a, b);
    }
    return m;
}
const questNames = nameMap(/^[0-9a-f]{24} name$/);
const itemNames = nameMap(/^[0-9a-f]{24} Name$/);

// --- 머리말 규칙
const HEAD_RULES = [
    [/^This quest is required for Collector$/, '콜렉터 완료에 필요한 의뢰다'],
    [/^This quest is not required for Collector$/, '콜렉터 완료에는 필요 없다'],
    [/^This quest is required for Lightkeeper$/, '등대지기 접촉에 필요한 의뢰다'],
    [/^This quest is not required for Lightkeeper$/, '등대지기 접촉에는 필요 없다'],
    [/^No keys required\.$/, '필요한 열쇠가 없다.'],
    [/^Requires Key\(s\):$/, '필요한 열쇠:'],
    [/^Leads to:$/, '이어지는 의뢰:'],
    [/^Required minimum gun durability: (\d+)$/, (m) => `요구 최소 총기 내구도: ${m[1]}`],
];
const LIST_LINE = /^[\t ]+\S/;

// 목록 항목: 들여쓰기·꼬리표(`, ` `●` `○`)는 남기고 이름만 옮긴다.
function translateListItem(line) {
    const m = line.match(/^([\t ]+)(.*?)([,\s]*)$/);
    if (!m) return line;
    const [, indent, middle, tail] = m;
    const mark = middle.match(/\s*([●○])\s*$/);
    const bare = (mark ? middle.slice(0, mark.index) : middle).trim();
    const ko = questNames.get(bare) ?? itemNames.get(bare);
    if (!ko) return line;              // 모르는 이름은 그대로 둔다
    return `${indent}${ko}${mark ? ' ' + mark[1] : ''}${tail}`;
}

function translateHeadLine(line) {
    const t = line.trim();
    for (const [re, to] of HEAD_RULES) {
        const m = t.match(re);
        if (m) return typeof to === 'function' ? to(m) : to;
    }
    return null;
}

// --- 손으로 적은 본문
const mapFile = path.join(SRC_DIR, NAME + '.json');
const hand = existsSync(mapFile)
    ? JSON.parse(readFileSync(mapFile, 'utf8').replace(/^\uFEFF/, ''))
    : { comment: '', byKey: {} };
const handBody = hand.byKey ?? {};
// \uC77C\uBD80\uB7EC \uC601\uBB38\uC73C\uB85C \uB450\uB294 \uAC83(\uB0B4\uBD80 \uC774\uB984 \uB4F1). \uBC88\uC5ED \uAC70\uB9AC\uC5D0\uC11C \uBE7C\uACE0, \uD3B4\uB0B8 \uC0AC\uC804\uC758 keptAsIs \uB85C \uB0A8\uAE34\uB2E4.
const handKeep = hand.keptAsIs ?? {};

// 바닐라 공식 한국어를 되쓸 수 있는 열쇠인가 — ExpandedTaskText 가 머리말을 붙이고 본문은
// 기본 `en` 그대로 남겨 둔 경우다. `kr.json` 에는 그 퀘스트의 공식 한국어가 아직 들어 있다.
function canReuseVanilla(key, enValue) {
    const vanillaEn = (baseEn[key] ?? '').trim();
    const vanillaKr = (baseKr[key] ?? '').trim();
    if (!vanillaEn || !HANGUL.test(vanillaKr)) return false;
    // **머리말이 붙어 있어야** 우리가 맡을 열쇠다. 머리말이 없으면 ExpandedTaskText 가 손대지 않은
    // 바닐라 항목이고, 그 `kr` 은 이미 공식 한국어라 우리가 같은 값을 다시 쓸 이유가 없다.
    if (!hasEttHeader(enValue)) return false;
    return enValue.trim().endsWith(vanillaEn);
}

/// 첫 줄이 ExpandedTaskText 머리말 꼴인가.
function hasEttHeader(value) {
    const first = value.split('\n').find(l => l.trim() !== '');
    return first !== undefined && translateHeadLine(first) !== null;
}

// --- 본문을 빼고 머리말만 한국어로 옮긴 다음, 본문을 붙인다
const entries = {}, sources = {}, kept = {};
const stat = { reused: 0, hand: 0, headerOnly: 0, pending: 0, alreadyKorean: 0, noHeadNoBody: 0, kept: 0 };
const pending = [];

// **`en` 을 기준으로 돈다.** `kr` 을 기준으로 삼으면 멱등하지 않다 — 우리가 이미 입힌 한국어를
// "남이 번역해 둔 것" 으로 보고 걸러내, 다시 펴낼 때 그 항목들이 사전에서 빠진다(실측: 494개가 빠졌다).
// `en` 은 늘 영문이고, 설명 열쇠에서는 ExpandedTaskText 가 덮기 전의 `kr` 값과 글자까지 같다.
for (const [key, v] of Object.entries(srvEn)) {
    const km = key.match(KEY);
    if (!km || typeof v !== 'string') continue;
    const id = km[1];
    if (handKeep[id]) { kept[key] = handKeep[id]; stat.kept++; continue; }
    // 우리가 맡은 열쇠가 아닌데 이미 한국어라면(바닐라나 다른 모드가 번역해 둔 것) 건드리지 않는다.
    const ours = Boolean(handBody[id]) || canReuseVanilla(key, v);
    if (!ours && HANGUL.test(String(srvKr[key] ?? ''))) { stat.alreadyKorean++; continue; }

    const lines = v.split('\n');
    const out = [];
    let i = 0, sawHeader = false;
    for (; i < lines.length; i++) {
        const ko = translateHeadLine(lines[i]);
        if (ko !== null) { out.push(ko); sawHeader = true; continue; }
        // 들여쓴 줄은 **머리말 안에서만** 목록 항목으로 본다. 그러지 않으면 앞에 공백이 하나 붙은
        // 본문(보기: `" Good afternoon Mercenary…"`)을 목록으로 잘못 보고 글 전체를 머리말로 삼켜,
        // 그 열쇠가 번역 거리에도 오르지 못하고 조용히 빠진다(실측 1건).
        if (sawHeader && LIST_LINE.test(lines[i])) { out.push(translateListItem(lines[i])); continue; }
        if (lines[i].trim() === '') { out.push(lines[i]); continue; }
        break;                          // 여기부터 본문
    }
    const bodyEn = lines.slice(i).join('\n').trim();
    const headKo = out.join('\n');
    const headHasKorean = HANGUL.test(headKo);

    let bodyKo = null, how = null;
    if (!bodyEn) {
        if (!headHasKorean) { stat.noHeadNoBody++; continue; }
        bodyKo = ''; how = 'headerOnly';
    } else if (handBody[id]) {
        bodyKo = handBody[id].trim(); how = 'hand';
    } else {
        const vanillaEn = (baseEn[key] ?? '').trim();
        const vanillaKr = (baseKr[key] ?? '').trim();
        if (vanillaEn && bodyEn === vanillaEn && HANGUL.test(vanillaKr)) {
            bodyKo = vanillaKr; how = 'reused';
        }
    }

    if (bodyKo === null) {
        stat.pending++;
        pending.push({ id, key, chars: bodyEn.length, en: bodyEn });
        continue;                       // 본문이 없으면 건드리지 않는다
    }
    stat[how]++;
    // 본문 자리를 그대로 지키며 갈아 끼운다(앞쪽 빈 줄 구조를 보존한다)
    const tailStart = out.length;
    const value = bodyKo
        ? [...out.slice(0, tailStart), bodyKo].join('\n')
        : out.join('\n');
    entries[key] = value.replace(/\n{3,}/g, '\n\n').trim();
    sources[key] = v;
}

console.log(`설명 열쇠 — 이미 한국어 ${stat.alreadyKorean} / 이번에 적은 것 ${Object.keys(entries).length} / 본문이 아직 없는 것 ${stat.pending}`);
console.log(`  바닐라 한국어를 되쓴 것   ${stat.reused}`);
console.log(`  손으로 적은 본문          ${stat.hand}`);
console.log(`  머리말만 있고 본문이 없음 ${stat.headerOnly}`);
if (stat.kept) console.log(`  일부러 영문으로 둔 것      ${stat.kept}`);
if (stat.noHeadNoBody) console.log(`  머리말도 본문도 못 가른 것 ${stat.noHeadNoBody} (건드리지 않았다)`);
const pendChars = pending.reduce((s, p) => s + p.chars, 0);
console.log(`  남은 본문 ${pending.length}개 / 영문 ${pendChars.toLocaleString()}자 / 평균 ${pending.length ? Math.round(pendChars / pending.length) : 0}자`);

if (dumpTo) {
    pending.sort((a, b) => a.chars - b.chars);
    writeFileSync(dumpTo, JSON.stringify(pending, null, 1));
    console.log(`  적었다: ${dumpTo} (짧은 것부터)`);
}

if (dry) { console.log('--dry 라 파일은 쓰지 않았다.'); process.exit(0); }

// 이전에 펴낸 것은 지우고 새로 쓴다
for (const f of readdirSync(OUT_DIR)) {
    if (new RegExp(`^${NAME}-\\d+\\.json$`).test(f)) unlinkSync(path.join(OUT_DIR, f));
}
const keys = Object.keys(entries);
if (!keys.length) { console.log('적을 것이 없다.'); process.exit(0); }
for (let n = 0, c = 1; n < keys.length; n += chunk, c++) {
    const slice = keys.slice(n, n + chunk);
    const out = {
        comment: `퀘스트 설명 — tools/gen-quest-descriptions.mjs 가 만든다(직접 고치지 않는다). `
            + `머리말은 규칙으로 옮기고, 본문은 바닐라 ${locale}.json 의 한국어를 되쓰거나 `
            + `lang-src/${NAME}.json 의 byKey 에 손으로 적은 것을 쓴다. `
            + `모드 구성이 바뀌면 머리말이 낡으므로 sources 로 어긋남을 확인한다.`,
        entries: Object.fromEntries(slice.map(k => [k, entries[k]])),
        sources: Object.fromEntries(slice.map(k => [k, sources[k]])),
        ...(c === 1 && Object.keys(kept).length ? { keptAsIs: kept } : {}),
    };
    const p = path.join(OUT_DIR, `${NAME}-${c}.json`);
    writeFileSync(p, JSON.stringify(out, null, 2) + '\n');
    console.log(`  적었다: lang/${NAME}-${c}.json (${slice.length}개)`);
}
console.log('서버 모드라 서버를 다시 켜야 적용된다.');
