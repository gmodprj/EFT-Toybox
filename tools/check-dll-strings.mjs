#!/usr/bin/env node
// lang/_dll-*.json 사전이 DLL 과 맞는지 게임을 켜지 않고 확인한다.
//
//   node tools/check-dll-strings.mjs            모든 _dll-*.json
//   node tools/check-dll-strings.mjs sain       이름에 sain 이 든 것만
//   node tools/check-dll-strings.mjs --list X   X 사전이 고른 메서드 안의 영문 리터럴 중 사전에 없는 것도 보인다
//   node tools/check-dll-strings.mjs --where X  공통 항목이 고른 메서드 여럿에 걸친 곳을 보인다(열쇠로 쓰는 곳이 섞였는지 본다)
//
// ilspycmd -il 로 IL 을 뽑아(캐시: %TEMP%\spt-dll-il) 각 원문이 "고른 메서드" 안에 ldstr 로 있는지 본다.
// 게임이 쓰는 것과 같은 규칙이다 — 메서드 밖에만 있는 글자는 바뀌지 않는다.
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { execSync } from 'node:child_process';

const SPT = process.env.SPT_DIR || 'E:\\Spt';
const LANG = path.join(path.dirname(new URL(import.meta.url).pathname).replace(/^\/([A-Za-z]:)/, '$1'), '..', 'src', 'SptKoreanConfig', 'lang');
const CACHE = path.join(os.tmpdir(), 'spt-dll-il');
const args = process.argv.slice(2);
const listMode = args.includes('--list');
const whereMode = args.includes('--where');
const filter = args.filter(a => !a.startsWith('--'))[0];

function findDll(asmName) {
  const roots = [path.join(SPT, 'BepInEx', 'plugins')];
  const stack = [...roots];
  while (stack.length) {
    const d = stack.pop();
    let ents = [];
    try { ents = fs.readdirSync(d, { withFileTypes: true }); } catch { continue; }
    for (const e of ents) {
      const p = path.join(d, e.name);
      if (e.isDirectory()) stack.push(p);
      else if (e.name.toLowerCase() === asmName.toLowerCase() + '.dll') return p;
    }
  }
  return null;
}

function ilFor(dll) {
  fs.mkdirSync(CACHE, { recursive: true });
  const st = fs.statSync(dll);
  const out = path.join(CACHE, `${path.basename(dll)}.${st.size}.${Math.floor(st.mtimeMs)}.il`);
  if (!fs.existsSync(out)) {
    const ilspy = path.join(os.homedir(), '.dotnet', 'tools', 'ilspycmd.cmd');
    // 콘솔 코드 페이지(cp949)로 받으면 —·✕ 같은 글자가 ? 로 깨진다. 65001 로 바꾸고 받는다.
    const q = s => `"${s}"`;
    const cmd = ['chcp 65001 >nul &&', q(ilspy), '-il', '-r', q(path.join(SPT, 'EscapeFromTarkov_Data', 'Managed')), '-r', q(path.join(SPT, 'BepInEx', 'core')), '-r', q(path.join(SPT, 'BepInEx', 'plugins')), q(dll)].join(' ');
    const text = execSync(cmd, { maxBuffer: 1 << 30, encoding: 'utf8', shell: 'cmd.exe', stdio: ['ignore', 'pipe', 'ignore'] });
    fs.writeFileSync(out, text);
  }
  return fs.readFileSync(out, 'utf8');
}

const unescape = s => s.replace(/\\(u[0-9a-fA-F]{4}|.)/g, (_, c) => c[0] === 'u' && c.length === 5 ? String.fromCharCode(parseInt(c.slice(1), 16)) : ({ n: '\n', t: '\t', r: '\r', '"': '"', '\\': '\\', '0': '\0' })[c] ?? c);

/** IL → Map("Ns.Type+Nested::Method" → Set(literal)) */
function literalsByMethod(il) {
  const map = new Map();
  const stack = [];
  let method = null, pending = false, buf = '';
  for (const raw of il.split(/\r?\n/)) {
    const l = raw.trim();
    if (l.startsWith('.class ')) { const t = l.split(/\s+/); stack.push(t[t.length - 1].replace(/^'|'$/g, '')); continue; }
    if (/^\} \/\/ end of class/.test(l)) { stack.pop(); continue; }
    if (l.startsWith('.method ')) { pending = true; buf = l; } else if (pending) buf += ' ' + l;
    if (pending && buf.includes('(')) { const m = buf.slice(0, buf.indexOf('(')).trim().split(/\s+/); method = m[m.length - 1].replace(/^'|'$/g, ''); pending = false; }
    const s = l.match(/ldstr\s+"((?:[^"\\]|\\.)*)"/);
    if (s) {
      const key = `${stack.join('+')}::${method}`;
      if (!map.has(key)) map.set(key, new Set());
      map.get(key).add(unescape(s[1]));
    }
  }
  return map;
}

const toRegex = p => new RegExp('^' + p.replace(/\//g, '+').replace(/[.+?^${}()|[\]\\]/g, '\\$&').replace(/\*/g, '.*') + '$');

let bad = 0;
const files = fs.readdirSync(LANG).filter(f => /^_dll-.*\.json$/.test(f) && (!filter || f.includes(filter)));
for (const f of files) {
  const j = JSON.parse(fs.readFileSync(path.join(LANG, f), 'utf8'));
  const dll = findDll(j.assembly);
  if (!dll) { console.log(`!  ${f}: ${j.assembly}.dll 을 찾지 못했다`); bad++; continue; }
  const byMethod = literalsByMethod(ilFor(dll));
  const pats = j.methods.map(toRegex);
  const excl = (j.excludeMethods || []).map(toRegex);
  const chosen = [...byMethod].filter(([m]) => pats.some(r => r.test(m)) && !excl.some(r => r.test(m)));
  const inChosen = new Set(chosen.flatMap(([, s]) => [...s]));
  const strings = Object.entries(j.strings || {}).filter(([k, v]) => k && v);
  const missing = strings.filter(([k]) => !inChosen.has(k));
  // 메서드별 항목: 그 패턴에 맞는 메서드 안에 있어야 한다.
  const overrides = Object.entries(j.methodStrings || {});
  for (const [p, map] of overrides) {
    const r = toRegex(p);
    const lits = new Set([...byMethod].filter(([m]) => r.test(m) && !excl.some(x => x.test(m))).flatMap(([, s]) => [...s]));
    for (const [k, v] of Object.entries(map)) {
      if (!k || !v) continue;
      strings.push([`${p} ‖ ${k}`, v, k]);
      if (!lits.has(k)) missing.push([`${p} ‖ ${k}`]);
      lits.forEach(s => inChosen.add(s));
    }
  }
  const placeholders = strings.filter(([k, v, raw]) => {
    const src = raw ?? k;
    const a = (src.match(/\{\d+\}/g) || []).sort().join(), b = (v.match(/\{\d+\}/g) || []).sort().join();
    return a !== b;
  });
  const deadPats = [...j.methods, ...overrides.map(([p]) => p)].filter(p => ![...byMethod.keys()].some(m => toRegex(p).test(m)));
  const ok = missing.length === 0 && placeholders.length === 0 && deadPats.length === 0;
  if (!ok) bad++;
  console.log(`${ok ? 'OK' : '!!'}  ${f} — ${j.assembly}: 항목 ${strings.length - missing.length}/${strings.length}, 고른 메서드 ${chosen.length}`);
  for (const [k] of missing) console.log(`    고른 메서드 안에 없음: ${JSON.stringify(k)}`);
  for (const [k, v] of placeholders) console.log(`    자리표시자 어긋남: ${JSON.stringify(k)} → ${JSON.stringify(v)}`);
  for (const p of deadPats) console.log(`    맞는 메서드가 없는 패턴: ${p}`);
  if (whereMode) {
    // 공통 항목이 고른 메서드 여럿에 걸치면 어디어디인지 보인다 — 그중 하나가 열쇠로 쓰는 곳이면 methodStrings 로 좁힌다.
    console.log('    -- 공통 항목이 여러 메서드에 걸친 것');
    for (const [k] of Object.entries(j.strings || {})) {
      const where = chosen.filter(([, s]) => s.has(k)).map(([m]) => m);
      if (where.length > 1) console.log(`       ${JSON.stringify(k)}\n          ${where.join('\n          ')}`);
    }
  }
  if (listMode) {
    const known = new Set([...Object.keys(j.strings || {}), ...Object.values(j.methodStrings || {}).flatMap(m => Object.keys(m))]);
    const left = [...inChosen].filter(s => /[A-Za-z]{2}/.test(s) && !known.has(s));
    console.log(`    -- 고른 메서드 안의 영문 리터럴 중 사전에 없는 것 ${left.length}`);
    for (const s of left) console.log(`       ${JSON.stringify(s)}`);
  }
}
console.log(bad ? `\n문제 있는 사전 ${bad}개` : `\nOK — 사전 ${files.length}개 모두 DLL 과 맞다`);
process.exit(bad ? 1 : 0);
