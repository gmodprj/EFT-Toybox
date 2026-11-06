// SptKoreanLocale 의 사전이 **서버에 실제로 먹었는지** 켜져 있는 서버에게 물어 확인한다.
//
// 사용:
//   node tools/check-locale-dict.mjs              kr 로 확인
//   node tools/check-locale-dict.mjs kr -v        안 먹은 항목까지 전부
//
// 파일을 세는 것이 아니라 `/client/locale/<코드>` 응답을 센다. 모드가 전부 올라간 뒤의 결과라
// "사전에 적었다" 가 아니라 "화면에 그 글자가 간다" 를 본다.
//
// 주의 — 라우트를 셸 인자로 넘기지 않는다. Git Bash 가 `/client/...` 를 윈도 경로로 바꿔 버리면
// 서버가 쓰레기 코드를 받고, SPT 는 모르는 locale 을 **조용히 en 으로 떨어뜨린다**(200 + 영문 사전).
// 그래서 URL 을 이 안에서 만든다.

import { readFileSync, readdirSync, existsSync } from 'node:fs';
import { deflateSync, inflateSync, inflateRawSync, gunzipSync } from 'node:zlib';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const HERE = path.dirname(fileURLToPath(import.meta.url));
const LANG_DIR = path.join(HERE, '..', 'src', 'SptKoreanLocale', 'lang');
const HANGUL = /[가-힣ᄀ-ᇿ㄰-㆏]/;

const locale = (process.argv[2] && !process.argv[2].startsWith('-')) ? process.argv[2] : 'kr';
const verbose = process.argv.includes('-v') || process.argv.includes('--verbose');

// --- 사전 읽기 (주석 있는 JSON 도 받는다) ---------------------------------

function stripJsonc(s) {
    let out = '';
    let inStr = false;
    let esc = false;
    for (let i = 0; i < s.length; i++) {
        const c = s[i];
        const n = s[i + 1];
        if (inStr) {
            out += c;
            if (esc) esc = false;
            else if (c === '\\') esc = true;
            else if (c === '"') inStr = false;
            continue;
        }
        if (c === '"') { inStr = true; out += c; continue; }
        if (c === '/' && n === '/') { while (i < s.length && s[i] !== '\n') i++; out += '\n'; continue; }
        if (c === '/' && n === '*') { i += 2; while (i < s.length && !(s[i] === '*' && s[i + 1] === '/')) i++; i++; continue; }
        out += c;
    }
    return out.replace(/,(\s*[}\]])/g, '$1');
}

if (!existsSync(LANG_DIR)) {
    console.error(`사전 폴더가 없다: ${LANG_DIR}`);
    process.exit(2);
}

const dicts = readdirSync(LANG_DIR)
    .filter(f => f.endsWith('.json') && !f.startsWith('_'))
    .sort()
    .map(f => {
        const raw = readFileSync(path.join(LANG_DIR, f), 'utf8').replace(/^﻿/, '');
        return { name: f.replace(/\.json$/, ''), data: JSON.parse(stripJsonc(raw)) };
    });

if (dicts.length === 0) {
    console.error(`사전이 없다: ${LANG_DIR}`);
    process.exit(2);
}

// --- 서버에 묻기 ----------------------------------------------------------

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

async function ask(code) {
    const res = await fetch(`https://127.0.0.1:6969/client/locale/${encodeURIComponent(code)}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: deflateSync(Buffer.from('{}', 'utf8')),
    });
    const raw = Buffer.from(await res.arrayBuffer());
    let text = null;
    for (const f of [inflateSync, inflateRawSync, gunzipSync]) {
        try { text = f(raw).toString('utf8'); break; } catch { /* 다음 방식 */ }
    }
    if (text === null) text = raw.toString('utf8');
    return JSON.parse(text).data;
}

let served;
let english;
try {
    served = await ask(locale);
    english = locale === 'en' ? null : await ask('en');
} catch (err) {
    console.error(`서버에 묻지 못했다 — ${err.message}`);
    console.error('SPT 서버가 켜져 있어야 한다(https://127.0.0.1:6969).');
    process.exit(2);
}

// 안전장치는 닫힌 쪽으로 — 그 언어가 없으면 서버가 en 을 돌려준다.
// 그걸 모르고 세면 "전부 미번역" 으로 읽는다.
if (english) {
    const sameKeys = Object.keys(served).length === Object.keys(english).length;
    let diff = 0;
    for (const k of Object.keys(served)) {
        if (served[k] !== english[k]) { diff++; if (diff > 3) break; }
    }
    if (sameKeys && diff === 0) {
        console.error(`!  '${locale}' 응답이 'en' 과 글자까지 같다 — 그 locale 이 없어 en 으로 떨어진 것이다.`);
        console.error('   locales/global 에 그 언어 파일이 있는지 본다. 셈을 믿지 말 것.');
        process.exit(2);
    }
}

// --- 대조 ----------------------------------------------------------------

let ok = 0;
const mismatched = [];
const absent = [];
const keptStillEnglish = [];
const keptNowKorean = [];
// 생성한 사전이 적어 둔 영문 원본(`sources`)이 지금 서버의 en 과 어긋난 것.
// 설명 사전은 ExpandedTaskText 가 붙이는 머리말까지 통째로 덮으므로, 모드 구성이 바뀌면
// 우리 값이 낡는다. 그때 조용히 틀리지 않게 알린다 — 생성기를 다시 돌리면 된다.
const staleSources = [];
const uncovered = [];

// 묶음 검사는 **사전 전체**를 한 덩어리로 본다. 한 묶음을 여러 파일로 쪼개 적을 수 있어야 하고
// (퀘스트 이름처럼 수가 많은 것), 그때 옆 파일이 맡은 열쇠를 "빠뜨렸다" 고 잡으면 안 된다.
const coveredKeys = new Set();
for (const { data } of dicts) {
    for (const k of Object.keys(data.entries ?? {})) coveredKeys.add(k);
    for (const k of Object.keys(data.keptAsIs ?? {})) coveredKeys.add(k);
}

for (const { name, data } of dicts) {
    const entries = data.entries ?? {};
    const kept = data.keptAsIs ?? {};

    for (const [key, want] of Object.entries(entries)) {
        if (!(key in served)) { absent.push({ name, key, want }); continue; }
        if (served[key] === want) { ok++; continue; }
        mismatched.push({ name, key, want, got: served[key] });
    }

    if (english && data.sources) {
        for (const [key, src] of Object.entries(data.sources)) {
            if (!(key in english)) continue;          // 열쇠가 사라진 것은 '열쇠가 없음' 이 잡는다
            if (english[key] === src) continue;
            staleSources.push({ name, key, want: src, got: String(english[key]) });
        }
    }

    for (const key of Object.keys(kept)) {
        if (!(key in served)) { absent.push({ name, key, want: '(유지)' }); continue; }
        if (HANGUL.test(String(served[key]))) keptNowKorean.push({ name, key, got: served[key] });
        else keptStillEnglish.push({ name, key, got: served[key] });
    }

    // 이 사전이 책임지는 묶음에서 빠뜨린 열쇠 찾기.
    // 접두로 묶이는 것(ScavRole/ 등)과 정규식으로만 묶이는 것(퀘스트 열쇠)을 함께 본다.
    const scopes = [
        ...(data.coveragePrefixes ?? []).map(p => ({ label: p, test: k => k.startsWith(p) })),
        ...(data.coveragePatterns ?? []).map(p => {
            const re = new RegExp(p);
            return { label: p, test: k => re.test(k) };
        }),
    ];
    for (const scope of scopes) {
        for (const key of Object.keys(served)) {
            if (!scope.test(key)) continue;
            if (coveredKeys.has(key)) continue;
            const value = String(served[key]);
            if (HANGUL.test(value)) continue;            // 바닐라나 다른 모드가 이미 한국어로 둔 것
            if (!/[A-Za-z]/.test(value)) continue;       // '???' 처럼 글자가 없는 것
            uncovered.push({ name, prefix: scope.label, key, got: value });
        }
    }
}

// 같은 묶음을 여러 파일이 선언하면 같은 열쇠가 여러 번 걸린다. 열쇠로 한 번만 센다.
const seenUncovered = new Set();
const uncoveredUnique = uncovered.filter(r => {
    if (seenUncovered.has(r.key)) return false;
    seenUncovered.add(r.key);
    return true;
});
uncovered.length = 0;
uncovered.push(...uncoveredUnique);

// --- 보고 ----------------------------------------------------------------

const dictTotal = dicts.reduce((s, d) => s + Object.keys(d.data.entries ?? {}).length, 0);
const keptTotal = dicts.reduce((s, d) => s + Object.keys(d.data.keptAsIs ?? {}).length, 0);

console.log(`locale '${locale}' 항목 ${Object.keys(served).length}개 (서버 실측)`);
console.log(`사전 ${dicts.length}개 / 번역 항목 ${dictTotal}개 / 일부러 영문으로 둔 것 ${keptTotal}개`);
console.log('');
console.log(`  적용됨            ${ok}`);
console.log(`  값이 다름         ${mismatched.length}`);
console.log(`  열쇠가 없음       ${absent.length}`);
console.log(`  유지 — 영문 그대로 ${keptStillEnglish.length}  (이것이 정상이다)`);
console.log(`  유지 — 한글로 바뀜 ${keptNowKorean.length}  (다른 것이 손댔다)`);
console.log(`  묶음에서 빠뜨림   ${uncovered.length}`);
console.log(`  원본이 바뀜       ${staleSources.length}  (생성기를 다시 돌려야 한다)`);

function dump(title, rows, fmt) {
    if (rows.length === 0) return;
    console.log('');
    console.log(`--- ${title} (${rows.length}) ---`);
    const show = verbose ? rows : rows.slice(0, 12);
    for (const r of show) console.log('  ' + fmt(r));
    if (!verbose && rows.length > show.length) console.log(`  … (+${rows.length - show.length}) -v 로 전부 본다`);
}

dump('값이 다름 — 모드가 안 돌았거나 다른 모드가 뒤에서 덮었다', mismatched,
    r => `[${r.name}] ${r.key}\n      사전: ${r.want}\n      서버: ${r.got}`);
dump('열쇠가 없음 — 그 모드를 뺐거나 사전에 오타다', absent,
    r => `[${r.name}] ${r.key}  (사전: ${r.want})`);
dump('유지했는데 한글로 바뀐 것', keptNowKorean,
    r => `[${r.name}] ${r.key} = ${r.got}`);
dump('묶음에서 빠뜨린 열쇠', uncovered,
    r => `[${r.name}] ${r.prefix} … ${r.key} = ${r.got}`);
dump('영문 원본이 바뀐 열쇠 — 우리 값이 낡았다(생성기를 다시 돌린다)', staleSources,
    r => `[${r.name}] ${r.key}\n      적어 둔 원본: ${r.want.slice(0, 90)}\n      지금의 영문:  ${r.got.slice(0, 90)}`);

const bad = mismatched.length + absent.length + uncovered.length + staleSources.length;
console.log('');
if (bad === 0) {
    console.log(`OK — ${ok}/${dictTotal} 적용, 빠뜨린 열쇠 0`);
} else {
    console.log(`!  문제 ${bad}건 (값 다름 ${mismatched.length} / 없는 열쇠 ${absent.length} / 빠뜨림 ${uncovered.length} / 원본 바뀜 ${staleSources.length})`);
}
process.exit(bad === 0 ? 0 : 1);
