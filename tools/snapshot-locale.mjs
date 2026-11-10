// 켜져 있는 서버가 내려주는 게임 locale 을 떠 두고, 뒤에 다시 떠서 **무엇이 늘고 줄었는지** 가린다.
//
// 왜 — 모드를 업데이트하면 locale 항목이 늘고 늘어난 것은 영문이다(2026-11-10: ISB 둘로 61,221
// → 61,391). 그런데 **무엇이 늘었는지는 떠 둔 기준선이 없으면 가릴 수 없다.** 그날 새 모드의
// json 에서 id 를 긁어 교집합을 내 봤지만, 봇 프리셋·호환 목록이 원래 있던 id 를 잔뜩 참조해
// 2,667개로 번졌다 — 쓸 수 없는 방법이다. 그래서 업데이트 **전에** 한 장 떠 둔다.
//
// 사용:
//   node tools/snapshot-locale.mjs --save [--locale kr]        → backup/locale-snapshots/<날짜>.json
//   node tools/snapshot-locale.mjs --diff <옛 파일> [--locale kr]
//   node tools/snapshot-locale.mjs --diff <옛> --only "Name|ShortName"   열쇠 꼴로 좁힌다
//
// 스냅샷은 `backup/` 아래에 둔다(git 제외) — 6만 항목이라 저장소에 넣을 것이 아니다.
import fs from 'node:fs';
import path from 'node:path';
import { deflateSync, inflateSync, inflateRawSync, gunzipSync } from 'node:zlib';

const args = process.argv.slice(2);
const opt = (name, dflt) => { const i = args.indexOf('--' + name); return i >= 0 && args[i + 1] ? args[i + 1] : dflt; };
const locale = opt('locale', 'kr');
const only = opt('only', null) ? new RegExp(opt('only', null)) : null;
const ROOT = 'E:/Spt_Mods';
const OUT = path.join(ROOT, 'backup', 'locale-snapshots');

process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
const ask = async (code) => {
    const res = await fetch(`https://127.0.0.1:6969/client/locale/${code}`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' }, body: deflateSync(Buffer.from('{}', 'utf8')),
    });
    const raw = Buffer.from(await res.arrayBuffer());
    for (const f of [inflateSync, inflateRawSync, gunzipSync]) {
        try { return JSON.parse(f(raw).toString('utf8')).data; } catch { /* 다음 */ }
    }
    return JSON.parse(raw.toString('utf8')).data;
};

let served;
try { served = await ask(locale); } catch (e) {
    console.error(`서버에 묻지 못했다: ${e.message}`);
    console.error('서버가 꺼져 있으면 뜰 수 없다 — 포트는 https://127.0.0.1:6969 다(http 아니다).');
    process.exit(2);
}
if (!served || typeof served !== 'object') { console.error('locale 응답이 비었다'); process.exit(2); }

if (args.includes('--save')) {
    fs.mkdirSync(OUT, { recursive: true });
    const day = new Date().toISOString().slice(0, 10);
    const file = path.join(OUT, `${locale}-${day}.json`);
    fs.writeFileSync(file, JSON.stringify(served));
    console.log(`떴다 — 항목 ${Object.keys(served).length}개 → ${file}`);
    process.exit(0);
}

const oldFile = opt('diff', null);
if (!oldFile) {
    console.error('사용: node tools/snapshot-locale.mjs --save | --diff <옛 파일> [--locale kr] [--only <정규식>]');
    process.exit(2);
}
const before = JSON.parse(fs.readFileSync(oldFile, 'utf8'));
const keys = (o) => new Set(Object.keys(o).filter((k) => !only || only.test(k)));
const a = keys(before), b = keys(served);
const added = [...b].filter((k) => !a.has(k));
const gone = [...a].filter((k) => !b.has(k));
const HANGUL = /[\uAC00-\uD7A3]/;
const english = added.filter((k) => /[A-Za-z]/.test(String(served[k])) && !HANGUL.test(String(served[k])));

console.log(`옛 ${a.size}개 → 지금 ${b.size}개${only ? `  (열쇠 꼴: ${only})` : ''}`);
console.log(`  늘어난 열쇠 ${added.length}개 (그중 영문 ${english.length}개)`);
console.log(`  사라진 열쇠 ${gone.length}개`);
const show = args.includes('-v') ? english : english.slice(0, 40);
if (show.length) {
    console.log('\n--- 늘어난 영문 항목 ---');
    for (const k of show) console.log(`  ${k}\t${String(served[k]).replace(/<[^>]*>/g, '').trim().slice(0, 90)}`);
    if (!args.includes('-v') && english.length > show.length) console.log(`  … (+${english.length - show.length}) -v 로 전부 본다`);
}
