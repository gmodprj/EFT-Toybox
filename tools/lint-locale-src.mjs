// 손으로 적는 지도(src/SptKoreanLocale/lang-src/*.json)를 검사하고, 고칠 수 있는 것은 고친다.
//
// 사용:
//   node tools/lint-locale-src.mjs            검사만
//   node tools/lint-locale-src.mjs --fix      고칠 수 있는 것은 고친다
//
// 왜 필요한가 — `check-locale-dict.mjs` 는 "적용됐는가" 만 본다. **읽을 만한가는 보지 않는다.**
// 실측으로 두 번 샜다: 괄호가 목적어와 서술어 사이에 끼어 `…6개를 (레이드 획득) 넘겨라` 가 됐고
// (69문구), 다음 묶음에서 같은 꼴이 15문구 또 생겼다. 그래서 그 규칙을 도구로 못박는다.

import { readdirSync, readFileSync, writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

const HERE = path.dirname(fileURLToPath(import.meta.url));
const SRC = path.join(HERE, '..', 'src', 'SptKoreanLocale', 'lang-src');
const fix = process.argv.includes('--fix');

const HANGUL = /[가-힣ᄀ-ᇿ㄰-㆏]/;
const VERBS = '넘겨라|찾아라|제출하라|제거하라|사살하라|확보하라|표시하라|설치하라|조사하라|죽여라|탈출하라|완료하라|건설하라|이용하라';
const TAIL_PAREN = new RegExp(`^(.*?)\\s(\\([^)]*\\))\\s(${VERBS})$`);
const LEFTOVER = new RegExp(`\\)\\s(${VERBS})$`);

let problems = 0;
let fixed = 0;

for (const f of readdirSync(SRC).filter(x => x.endsWith('.json'))) {
    const p = path.join(SRC, f);
    const doc = JSON.parse(readFileSync(p, 'utf8').replace(/^﻿/, ''));
    const texts = doc.texts ?? {};
    const byKey = doc.byKey ?? {};
    let touched = false;

    // 1) 괄호가 서술어 앞에 끼어든 것 → 서술어 뒤로 옮긴다
    for (const bag of [texts, byKey]) {
        for (const [k, ko] of Object.entries(bag)) {
            if (typeof ko !== 'string') continue;
            const m = ko.match(TAIL_PAREN);
            if (!m) continue;
            const better = `${m[1]} ${m[3]} ${m[2]}`;
            if (fix) { bag[k] = better; touched = true; fixed++; }
            else { console.log(`  [괄호 어순] ${ko}\n              → ${better}`); problems++; }
        }
    }

    // 2) 한글이 없는 값 — 번역을 빠뜨렸을 수 있다
    for (const [bagName, bag] of [['texts', texts], ['byKey', byKey]]) {
        for (const [k, ko] of Object.entries(bag)) {
            if (typeof ko !== 'string' || HANGUL.test(ko)) continue;
            console.log(`  [한글 없음] ${bagName}: ${JSON.stringify(k)} → ${JSON.stringify(ko)}`);
            problems++;
        }
    }

    // 3) 서술어가 두 번 붙은 것 — 틀을 겹쳐 찍었을 때 생긴다
    for (const [k, ko] of Object.entries(texts)) {
        if (typeof ko !== 'string') continue;
        if (new RegExp(`(${VERBS})\\s(${VERBS})$`).test(ko)) {
            console.log(`  [서술어 중복] ${JSON.stringify(k)} → ${ko}`);
            problems++;
        }
    }

    // 4) 자리표시자 수가 영문과 다른 것
    for (const [en, ko] of Object.entries(texts)) {
        if (typeof ko !== 'string') continue;
        const a = (en.match(/\{\d+\}/g) ?? []).sort().join(',');
        const b = (ko.match(/\{\d+\}/g) ?? []).sort().join(',');
        if (a !== b) { console.log(`  [자리표시자] ${JSON.stringify(en)}: ${a || '(없음)'} vs ${b || '(없음)'}`); problems++; }
    }

    if (touched) {
        writeFileSync(p, JSON.stringify(doc, null, 2) + '\n');
        console.log(`  고쳐 적었다: lang-src/${f}`);
    }

    // 고친 뒤에도 남아 있는지 다시 본다(고치는 규칙이 못 잡는 모양)
    for (const ko of [...Object.values(texts), ...Object.values(byKey)]) {
        if (typeof ko === 'string' && LEFTOVER.test(ko)) {
            console.log(`  [괄호 어순 — 자동으로 못 고쳤다] ${ko}`);
            problems++;
        }
    }
}

console.log('');
if (fix) console.log(`고친 문구 ${fixed}개`);
if (problems === 0) { console.log('OK — 지도에 걸리는 것이 없다'); process.exit(0); }
console.log(`!  문제 ${problems}건` + (fix ? '' : '  (--fix 로 고칠 수 있는 것은 고친다)'));
process.exit(1);
