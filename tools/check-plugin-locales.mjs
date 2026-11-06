// 모드 폴더에 넣는 한국어 locale 사전을 원본 en 파일과 대조한다.
//
// 사용:
//   node tools/check-plugin-locales.mjs          요약
//   node tools/check-plugin-locales.mjs -v       빠진 항목까지 전부
//
// 보는 것 다섯 가지:
//   1. 원본에 있고 우리에게 없는 열쇠      → 그 항목은 영문으로 나온다
//   2. 우리에게 있고 원본에 없는 열쇠      → 오타. 조용히 무시된다
//   3. 중복된 열쇠                         → 뒤의 것이 이긴다
//   4. 한글이 없는 값                      → 글자가 있는 값만 문제로 센다(숫자·자리표시자는 정상)
//   5. 자리표시자 수가 원본과 다른 값      → {0}·{1} 이 빠지면 화면이 깨진다
//
// 그리고 설치본에 들어간 파일이 이 사전과 **같은지** 본다 — 모드를 업데이트하면 날아간다.

import { readFileSync, existsSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const HERE = path.dirname(fileURLToPath(import.meta.url));
const SRC = path.join(HERE, '..', 'src', 'PluginLocales');
const PLUGINS = process.argv.find(a => a.startsWith('--spt='))
    ? path.join(process.argv.find(a => a.startsWith('--spt=')).slice(6), 'BepInEx', 'plugins')
    : 'E:/Spt/BepInEx/plugins';

const verbose = process.argv.includes('-v') || process.argv.includes('--verbose');
const HANGUL = /[\uAC00-\uD7A3\u1100-\u11FF\u3130-\u318F]/;
const LETTERS = /[A-Za-z\u0400-\u04FF]/;

const rd = f => JSON.parse(readFileSync(f, 'utf8').replace(/^\uFEFF/, ''));

/** 사전을 열쇠→문구 로 펴고, 중복 열쇠도 함께 돌려준다. */
function flatten(obj, format) {
    const out = new Map();
    const dup = [];
    if (format === 'isb-subtitles') {
        for (const e of obj.entries ?? []) {
            if (!e || !e.key) continue;
            if (out.has(e.key)) dup.push(e.key);
            out.set(e.key, e.text ?? '');
        }
    } else {
        for (const [k, v] of Object.entries(obj)) out.set(k, typeof v === 'string' ? v : String(v));
    }
    return { map: out, dup };
}

const placeholders = s => (s.match(/\{\d+\}/g) ?? []).sort().join(',');

const manifest = rd(path.join(SRC, 'manifest.json'));
let bad = 0;
const lines = [];

console.log(`locale 코드: ${manifest.localeCode.value}  (${manifest.localeCode.evidence})`);
console.log('');

for (const t of manifest.targets) {
    const from = path.join(SRC, t.source);
    const ref = path.join(PLUGINS, t.reference);
    const dest = path.join(PLUGINS, t.dest);

    console.log(`--- ${t.name}  (${t.plugin})`);

    if (!existsSync(from)) { console.log(`  !  사전이 없다: ${from}`); bad++; continue; }
    if (!existsSync(ref)) { console.log(`  !  원본이 없다: ${ref} — 모드를 뺐거나 경로가 바뀌었다`); bad++; continue; }

    const ours = flatten(rd(from), t.format);
    const theirs = flatten(rd(ref), t.format);

    const missing = [...theirs.map.keys()].filter(k => !ours.map.has(k));
    const unknown = [...ours.map.keys()].filter(k => !theirs.map.has(k));

    const noKorean = [];
    const phMismatch = [];
    for (const [k, v] of ours.map) {
        if (LETTERS.test(v) && !HANGUL.test(v)) noKorean.push(`${k} = ${v.slice(0, 50)}`);
        const en = theirs.map.get(k);
        if (en !== undefined && placeholders(en) !== placeholders(v)) {
            phMismatch.push(`${k}: 원본 ${placeholders(en) || '(없음)'} / 사전 ${placeholders(v) || '(없음)'}`);
        }
    }

    // 설치본에 들어간 것이 이 사전과 같은지
    let installed = '설치 안 됨';
    if (existsSync(dest)) {
        const a = readFileSync(from, 'utf8').replace(/^\uFEFF/, '').replace(/\r\n/g, '\n');
        const b = readFileSync(dest, 'utf8').replace(/^\uFEFF/, '').replace(/\r\n/g, '\n');
        installed = a === b ? '설치본과 같다' : '설치본이 다르다 — 다시 넣어야 한다';
    }

    const problems = missing.length + unknown.length + ours.dup.length + phMismatch.length;
    bad += problems;
    if (installed.startsWith('설치본이 다르') || installed === '설치 안 됨') bad++;

    console.log(`  번역 ${ours.map.size} / 원본 ${theirs.map.size}`
        + ` | 빠진 열쇠 ${missing.length} | 없는 열쇠 ${unknown.length} | 중복 ${ours.dup.length}`
        + ` | 자리표시자 어긋남 ${phMismatch.length} | 한글 없는 값 ${noKorean.length}`);
    console.log(`  ${installed}`);

    const dump = (title, rows) => {
        if (rows.length === 0) return;
        const show = verbose ? rows : rows.slice(0, 6);
        console.log(`    ${title} (${rows.length}):`);
        for (const r of show) console.log(`      ${r}`);
        if (!verbose && rows.length > show.length) console.log(`      … (+${rows.length - show.length}) -v 로 전부 본다`);
    };
    dump('빠진 열쇠 — 영문으로 나온다', missing);
    dump('없는 열쇠 — 오타, 조용히 무시된다', unknown);
    dump('중복 열쇠', ours.dup);
    dump('자리표시자 어긋남 — 화면이 깨진다', phMismatch);
    if (noKorean.length) dump('한글 없는 값 (숫자·고유명사면 정상)', noKorean);
    console.log('');
}

if (bad === 0) {
    console.log('OK — 네 사전 모두 원본과 맞고 설치본과 같다');
    process.exit(0);
}
console.log(`!  문제 ${bad}건`);
process.exit(1);
