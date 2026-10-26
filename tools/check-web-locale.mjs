// 서버 웹 로케일 번역(src/ServerWebKorean/ko.json)이 설치본의 en.json 과 맞는지 본다.
// 사용: node tools/check-web-locale.mjs [SPT 설치 경로]
import { readFileSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const repo = join(dirname(fileURLToPath(import.meta.url)), '..');
const sptDir = process.argv[2] ?? 'E:\\Spt';
const enPath = join(sptDir, 'SPT_Runtime', 'SPT_Data', 'database', 'locales', 'web', 'en.json');
const koPath = join(repo, 'src', 'ServerWebKorean', 'ko.json');

const read = (p) => JSON.parse(readFileSync(p, 'utf8'));
const en = read(enPath);
const ko = read(koPath);

const enKeys = Object.keys(en);
const koKeys = Object.keys(ko);
const missing = enKeys.filter((k) => !(k in ko));
const extra = koKeys.filter((k) => !(k in en));

// {0} 같은 자리표시자가 번역에서 빠지면 런타임에 값이 사라진다.
const slots = (s) => [...String(s).matchAll(/\{\d+\}/g)].map((m) => m[0]).sort().join(',');
const slotMismatch = enKeys.filter((k) => k in ko && slots(en[k]) !== slots(ko[k]));

// 영어 그대로인 항목(식별자·버전 문자열은 그대로 두는 게 맞다)
const untranslated = enKeys.filter((k) => ko[k] === en[k]);

console.log(`en ${enKeys.length} 개 / ko ${koKeys.length} 개`);
if (missing.length) console.log(`번역 없음(${missing.length}): ${missing.join(', ')}`);
if (extra.length) console.log(`en 에 없는 키(${extra.length}): ${extra.join(', ')}`);
if (slotMismatch.length) console.log(`자리표시자 불일치(${slotMismatch.length}): ${slotMismatch.join(', ')}`);
if (untranslated.length) console.log(`원문 그대로(${untranslated.length}): ${untranslated.join(', ')}`);

const bad = missing.length + extra.length + slotMismatch.length;
console.log(bad === 0 ? '문제 없음' : `문제 ${bad} 건`);
process.exit(bad === 0 ? 0 : 1);
