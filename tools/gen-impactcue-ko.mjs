#!/usr/bin/env node
// ImpactCue(F10 설정 창)의 한국어를 만든다 — 모드의 ko.json 은 그대로 두고 locales/ko-KR.json 을 새로 만든다.
//
//   node tools/gen-impactcue-ko.mjs            src/PluginLocales/ImpactCue/ko-KR.json 과 F12 사전을 다시 만든다
//   node tools/gen-impactcue-ko.mjs --check    만들지 않고 바뀔 것만 센다
//
// 왜 ko-KR 인가: ImpactCue 는 Language = Auto 일 때 CultureInfo.CurrentUICulture.Name(ko-KR) 파일을
// ko 보다 먼저 찾는다(LocalizationManager.ResolveLanguageCode 역컴파일). 그래서 모드 파일을 덮지 않고 이긴다.
//
// 언제나 **모드의 원본 ko.json·en.json 에서** 시작한다 — 이미 바꾼 결과를 다시 읽지 않으므로 몇 번 돌려도 같다.
//
// 하는 일
//   1. 모드 ko.json 위에 overrides.json 의 values 를 얹는다(영문으로 남은 43열쇠·오역).
//   2. 용어를 맞춘다: 킬→처치, 킬 피드→처치 기록, 사운드→소리, 볼륨→음량, 아머→방어구, 히트→명중, 크로스헤어→조준점,
//      레이아웃→배치, 앵커→기준점, 글로우→발광, 플로팅 데미지→떠오르는 피해, 데미지→피해. 받침에 맞춰 조사도 고친다.
//   3. 설명 속에 영문으로 남은 설정 이름("Master Volume" 등)을 그 설정의 한국어 이름으로 바꾼다(locale 자신의 en→ko).
//   4. 섹션 이름(_category.*)을 overrides.json 의 sections 로 맞춘다.
//   5. F12 사전(com.dizzlow.impactcue.json)의 이름·섹션을 같은 값으로 맞춘다 — F10 과 F12 가 같은 말을 쓰게.
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const HERE = path.dirname(fileURLToPath(import.meta.url));
const ROOT = path.join(HERE, '..');
const SPT = process.env.SPT_DIR || 'E:/Spt';
const MOD = path.join(SPT, 'BepInEx', 'plugins', 'ImpactCue-UI-MOD', 'locales');
const OUT = path.join(ROOT, 'src', 'PluginLocales', 'ImpactCue', 'ko-KR.json');
const OVR = path.join(ROOT, 'src', 'PluginLocales', 'ImpactCue', 'overrides.json');
const F12 = path.join(ROOT, 'src', 'SptKoreanConfig', 'lang', 'com.dizzlow.impactcue.json');
const check = process.argv.includes('--check');

const rd = f => JSON.parse(fs.readFileSync(f, 'utf8').replace(/^\uFEFF/, ''));
const en = rd(path.join(MOD, 'en.json'));
const ko = rd(path.join(MOD, 'ko.json'));
const ovr = rd(OVR);

// ── 받침에 맞춘 조사 ────────────────────────────────────────────────
const hasBatchim = w => {
  const c = w.charCodeAt(w.length - 1);
  if (c < 0xac00 || c > 0xd7a3) return false;
  return (c - 0xac00) % 28 !== 0;
};
const rieulBatchim = w => {
  const c = w.charCodeAt(w.length - 1);
  return c >= 0xac00 && c <= 0xd7a3 && (c - 0xac00) % 28 === 8;
};
/** 조사 p 를 낱말 to 의 받침에 맞는 꼴로. */
function particle(to, p) {
  const b = hasBatchim(to);
  const pairs = [['으로', '로'], ['이', '가'], ['을', '를'], ['은', '는'], ['과', '와']];
  for (const [withB, withoutB] of pairs) {
    if (p !== withB && p !== withoutB) continue;
    if (withB === '으로') return b && !rieulBatchim(to) ? '으로' : '로';
    return b ? withB : withoutB;
  }
  return p;
}

const GLOSSARY = [
  [/킬 피드/g, '처치 기록'],
  [/플로팅 데미지/g, '떠오르는 피해'],
  [/플로팅/g, '떠오르는'],
  [/데미지/g, '피해'],
  [/아머 히트/g, '방어구 명중'],
  [/히트 마커/g, '명중 마커'],
  [/히트/g, '명중'],
  [/아머/g, '방어구'],
  [/킬(?![러로])/g, '처치'],
  [/사운드/g, '소리'],
  [/볼륨/g, '음량'],
  [/크로스헤어/g, '조준점'],
  [/레이아웃/g, '배치'],
  [/앵커된/g, '기준점에 맞춘'],
  [/앵커/g, '기준점'],
  [/글로우/g, '발광'],
  [/미리듣기/g, '미리 듣기'],
  [/Black Division/g, '블랙 디비전'],
  // 봇 역할 이름은 공식 kr·퀘스트 목표 사전과 같게. 대문자 BOSS 는 모드가 등급 이름으로 쓰므로 둔다.
  [/\bBoss\b/g, '보스'],
  [/\bSCAV\b|\bScav\b/g, '스캐브'],
  [/\bFollower\b|\bGuard\b/g, '경호원'],
  [/\bRogue\b/g, '로그'],
  [/\bRaider\b/g, '레이더'],
  [/\bCultist\b/g, '교단원'],
  [/Health HUD/g, '체력 HUD'],
  [/Health Bar/g, '체력 막대'],
  [/TACTICAL HUD/g, '전술 HUD'],
  [/Tactical HUD/g, '전술 HUD'],
];

/** 낱말을 바꾼 뒤, 바뀐 낱말 바로 뒤에 붙은 조사(뒤가 공백·문장부호·끝인 것만)를 새 낱말의 받침에 맞춘다. */
function applyGlossary(text) {
  let t = text;
  for (const [re, to] of GLOSSARY) {
    if (!re.test(t)) { re.lastIndex = 0; continue; }
    re.lastIndex = 0;
    t = t.replace(re, to);
    t = fixParticles(t, to);
  }
  return t;
}

/** t 안에서 낱말 word 바로 뒤의 조사를 word 의 받침에 맞춘다. */
function fixParticles(t, word) {
  const esc = word.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  const pr = new RegExp(`${esc}(으로|로|이|가|을|를|은|는|과|와)(?=[\\s.,)/·:!?"']|$)`, 'g');
  return t.replace(pr, (m, p) => word + particle(word, p));
}

// ── 1. 원본 + overrides ──────────────────────────────────────────────
const out = {};
for (const k of Object.keys(en)) out[k] = k in ko ? ko[k] : en[k];
for (const [k, v] of Object.entries(ko)) if (!(k in out)) out[k] = v;
const overridden = new Set();
for (const [k, v] of Object.entries(ovr.values || {})) { out[k] = v; overridden.add(k); }

// ── 2. 용어 ──────────────────────────────────────────────────────────
let glossaryChanged = 0;
for (const k of Object.keys(out)) {
  if (k.startsWith('_meta') || overridden.has(k) || typeof out[k] !== 'string') continue;
  const t = applyGlossary(out[k]);
  if (t !== out[k]) { out[k] = t; glossaryChanged++; }
}

// ── 3. 설명 속 영문 설정 이름 → 한국어 이름 ─────────────────────────
const nameMap = [];
for (const [k, v] of Object.entries(en)) {
  if (!k.endsWith('|name') || typeof v !== 'string') continue;
  const koName = out[k];
  if (!koName || koName === v || !/[가-힣]/.test(koName)) continue;
  if (v.split(/\s+/).length < 2 || v.length < 8) continue;
  nameMap.push([v, koName]);
}
nameMap.sort((a, b) => b[0].length - a[0].length);
let refChanged = 0;
for (const k of Object.keys(out)) {
  if (k.endsWith('|name') || k.startsWith('_meta') || k.startsWith('_category.') || typeof out[k] !== 'string') continue;
  let t = out[k];
  for (const [e, kn] of nameMap) {
    if (!t.includes(e)) continue;
    t = t.split(e).join(kn);
    t = fixParticles(t, kn);
  }
  if (t !== out[k]) { out[k] = t; refChanged++; }
}

// ── 4. 섹션 이름 ─────────────────────────────────────────────────────
// 모드 원문이 하위 분류를 번호 없이 적은 곳(예: "_category.13.7 Stamina HUD": "Stamina HUD")은 번호를 뗀다.
// F12 사전 쪽 섹션은 번호를 그대로 둔다 — F12 는 섹션을 번호순으로 늘어놓는다.
const NUM = /^\d+(?:\.\d+)?\.?\s+/;
for (const [sec, name] of Object.entries(ovr.sections || {})) {
  const enVal = en['_category.' + sec];
  out['_category.' + sec] = enVal !== undefined && !NUM.test(enVal) ? name.replace(NUM, '') : name;
}
out['_meta.note'] = (ko['_meta.note'] || '') + ' — Spt_Mods 가 tools/gen-impactcue-ko.mjs 로 만든 ko-KR 판(용어 통일·미번역 채움). 원본 ko.json 은 그대로 있다.';

// ── 5. F12 사전 ──────────────────────────────────────────────────────
const f12 = rd(F12);
let f12Names = 0, f12Desc = 0, f12Sec = 0;
for (const sec of Object.keys(f12.sections)) {
  const want = ovr.sections?.[sec];
  if (want && f12.sections[sec] !== want) { f12.sections[sec] = want; f12Sec++; }
}
for (const [id, entry] of Object.entries(f12.settings)) {
  const [sec, ...rest] = id.split('/');
  const key = rest.join('/');
  const nameKey = `${sec}|${key}|name`;
  const descKey = `${sec}|${key}|description`;
  const newName = out[nameKey] ?? (entry.name ? applyGlossary(entry.name) : entry.name);
  if (newName && newName !== entry.name) { entry.name = newName; f12Names++; }
  // 설명: 모드 원본 설명을 그대로 가져온 것이면 새 값으로, 우리가 따로 쓴 것이면 용어만 맞춘다.
  if (entry.desc) {
    const nd = (ko[descKey] !== undefined && entry.desc === ko[descKey]) ? out[descKey] : applyGlossary(entry.desc);
    if (nd !== entry.desc) { entry.desc = nd; f12Desc++; }
  }
}

console.log(`ko-KR: 열쇠 ${Object.keys(out).length} (원본 en ${Object.keys(en).length}) · overrides ${overridden.size} · 용어 바꿈 ${glossaryChanged} · 설명 속 이름 바꿈 ${refChanged}`);
console.log(`F12 사전: 이름 ${f12Names} · 설명 ${f12Desc} · 섹션 ${f12Sec} 바뀜`);
if (check) process.exit(0);
fs.mkdirSync(path.dirname(OUT), { recursive: true });
fs.writeFileSync(OUT, JSON.stringify(out, null, 2) + '\n');
fs.writeFileSync(F12, JSON.stringify(f12, null, 2) + '\n');
console.log(`썼다: ${path.relative(ROOT, OUT)}, ${path.relative(ROOT, F12)}`);
