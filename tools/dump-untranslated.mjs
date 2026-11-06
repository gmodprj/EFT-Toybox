// 켜져 있는 서버에게 물어 **아직 영문인 열쇠**를 뽑아 낸다. 번역할 거리를 만드는 도구다.
//
// 사용:
//   node tools/dump-untranslated.mjs "^[0-9a-f]{24} name$"              요약 + 보기
//   node tools/dump-untranslated.mjs "^[0-9a-f]{24} name$" --tsv        id<TAB>영문  (번역용)
//   node tools/dump-untranslated.mjs "^[0-9a-f]{24} name$" --json out.json
//   ... --locale kr --min-words 3 --skip 0 --limit 500
//
// 파일이 아니라 `/client/locale/<코드>` 응답을 센다 — 모드가 전부 올라간 뒤의 결과다.
// SptKoreanLocale 이 이미 바꾼 항목은 한글이라 자동으로 빠진다.
//
// 라우트를 셸 인자로 넘기지 않는다(Git Bash 가 경로로 바꿔 버리고 서버는 조용히 en 으로 떨어진다).

import { writeFileSync, readFileSync, existsSync } from 'node:fs';
import { deflateSync, inflateSync, inflateRawSync, gunzipSync } from 'node:zlib';

const args = process.argv.slice(2);
const pattern = args.find(a => !a.startsWith('-'));
if (!pattern) {
    console.error('사용: node tools/dump-untranslated.mjs "<정규식>" [--tsv|--json <파일>] [--locale kr] [--min-words N] [--skip N] [--limit N]');
    process.exit(2);
}
const opt = (name, dflt) => {
    const i = args.indexOf('--' + name);
    return i >= 0 && args[i + 1] ? args[i + 1] : dflt;
};
const locale = opt('locale', 'kr');
const minWords = Number(opt('min-words', '1'));
const skip = Number(opt('skip', '0'));
const limit = Number(opt('limit', '0'));
const asTsv = args.includes('--tsv');
const jsonOut = args.includes('--json') ? opt('json', null) : null;

const HANGUL = /[\uAC00-\uD7A3\u1100-\u11FF\u3130-\u318F]/;
const words = v => (v.replace(/<[^>]*>/g, ' ').match(/[A-Za-z][A-Za-z'’-]{1,}/g) || []).length;

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

async function ask(code) {
    const res = await fetch(`https://127.0.0.1:6969/client/locale/${encodeURIComponent(code)}`, {
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

// 바닐라 kr.json 에 있던 열쇠인지(= 모드가 더한 것인지) 가른다
const BASE = `E:/Spt/SPT_Runtime/SPT_Data/database/locales/global/${locale}.json`;
const baseKeys = existsSync(BASE)
    ? new Set(Object.keys(JSON.parse(readFileSync(BASE, 'utf8'))))
    : new Set();

// --pending: 이미 사전(src/SptKoreanLocale/lang/*.json)에 적은 열쇠는 뺀다.
// 묶음을 여러 번에 걸쳐 적을 때 "다음에 적을 것" 만 보려는 용도다.
const pendingOnly = args.includes('--pending');
const alreadyWritten = new Set();
if (pendingOnly) {
    const LANG = 'E:/Spt_Mods/src/SptKoreanLocale/lang';
    const { readdirSync } = await import('node:fs');
    for (const f of readdirSync(LANG).filter(x => x.endsWith('.json') && !x.startsWith('_'))) {
        const o = JSON.parse(readFileSync(`${LANG}/${f}`, 'utf8').replace(/^\uFEFF/, ''));
        for (const k of Object.keys(o.entries ?? {})) alreadyWritten.add(k);
        for (const k of Object.keys(o.keptAsIs ?? {})) alreadyWritten.add(k);
    }
}

const re = new RegExp(pattern);
const rows = [];
for (const [k, v] of Object.entries(served)) {
    if (!re.test(k)) continue;
    if (typeof v !== 'string') continue;
    if (HANGUL.test(v)) continue;
    if (words(v) < minWords) continue;
    if (pendingOnly && alreadyWritten.has(k)) continue;
    rows.push({ key: k, en: v, mod: !baseKeys.has(k) });
}
rows.sort((a, b) => a.en.localeCompare(b.en) || a.key.localeCompare(b.key));

const sliced = limit > 0 ? rows.slice(skip, skip + limit) : rows.slice(skip);

if (jsonOut) {
    writeFileSync(jsonOut, JSON.stringify(sliced, null, 1));
    console.error(`적었다: ${jsonOut} (${sliced.length}개)`);
} else if (asTsv) {
    for (const r of sliced) console.log(`${r.key}\t${r.en.replace(/\t/g, ' ')}`);
} else {
    const chars = rows.reduce((s, r) => s + r.en.length, 0);
    console.log(`locale '${locale}' 에서 "${pattern}" 에 걸리는 미번역 항목`);
    console.log(`  항목 ${rows.length} | 영문 글자수 ${chars.toLocaleString()} | 평균 ${rows.length ? Math.round(chars / rows.length) : 0}`);
    console.log(`  모드가 더한 것 ${rows.filter(r => r.mod).length} / 바닐라 열쇠 ${rows.filter(r => !r.mod).length}`);
    console.log('');
    for (const r of sliced.slice(0, 20)) {
        console.log(`  ${r.mod ? 'M' : 'V'}  ${r.key}  ${r.en.replace(/\s+/g, ' ').slice(0, 70)}`);
    }
    if (sliced.length > 20) console.log(`  … (+${sliced.length - 20}) --tsv 로 전부 본다`);
}
