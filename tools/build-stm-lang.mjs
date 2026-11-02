// STM(SPT Team Manager)의 한국어 UI 문구 파일을 만든다.
//   node tools/build-stm-lang.mjs
//
// 원본은 `src/SptTeamManager/server/Resources/lang/en.json`(PIT 업스트림), 번역은
// `tools/lang/stm-kr.json`(열쇠는 en.json 의 경로, 배열은 `열쇠[번호]`)이다. 산출물은
// 같은 폴더의 `kr.json`·`ko.json` 두 개다 — 손으로 고치지 말고 이 스크립트로 다시 만든다.
//
// 두 파일을 내는 이유: 모드는 게임의 언어 코드를 소문자로 바꿔 그 이름의 파일을 찾는다
// (`friendlyPlugin.NormalizeLanguageCode`). EFT·SPT 는 한국어를 `kr` 로 쓰지만(설치본의
// `SPT_Data/database/locales/global/kr.json`), 클라이언트가 `ko` 를 넘기는 경우에도 영문으로
// 떨어지지 않게 같은 내용을 두 이름으로 둔다. 한 벌의 번역에서 나오므로 서로 어긋날 수 없다.
//
// 검사: 번역 누락, 자리틀({0}) 어긋남, 줄바꿈 수 어긋남, 원문과 같은 값(일부러 두는 것 제외),
// en.json 에 없는 번역 열쇠. 하나라도 걸리면 아무 파일도 쓰지 않고 1 로 끝난다.
import { readFileSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';

const ROOT = join(dirname(new URL(import.meta.url).pathname.replace(/^\/([A-Za-z]:)/, '$1')), '..');
const LANG_DIR = join(ROOT, 'src', 'SptTeamManager', 'server', 'Resources', 'lang');
const EN = join(LANG_DIR, 'en.json');
const MAP = join(ROOT, 'tools', 'lang', 'stm-kr.json');
const OUTPUTS = ['kr.json', 'ko.json'].map((n) => join(LANG_DIR, n));

// 일부러 원문 그대로 두는 열쇠(창 닫기 버튼의 "x" 처럼 글자 자체가 기호인 것).
const KEEP_AS_IS = new Set(['socialUi.RenameClose']);

const en = JSON.parse(readFileSync(EN, 'utf8'));
const map = JSON.parse(readFileSync(MAP, 'utf8'));

const used = new Set();
const problems = [];

const placeholders = (s) => (s.match(/\{[^}]*\}/g) ?? []).slice().sort().join(' ');
const newlines = (s) => (s.match(/\n/g) ?? []).length;

function translate(path, value) {
  if (!(path in map)) {
    problems.push(`번역 누락: ${path} = ${JSON.stringify(value)}`);
    return value;
  }
  used.add(path);
  const ko = map[path];
  if (typeof ko !== 'string') {
    problems.push(`번역이 문자열이 아니다: ${path}`);
    return value;
  }
  if (placeholders(value) !== placeholders(ko)) {
    problems.push(`자리틀 어긋남: ${path} (en "${placeholders(value)}" / kr "${placeholders(ko)}")`);
  }
  if (newlines(value) !== newlines(ko)) {
    problems.push(`줄바꿈 수 어긋남: ${path} (en ${newlines(value)}, kr ${newlines(ko)})`);
  }
  if (ko === value && !KEEP_AS_IS.has(path)) {
    problems.push(`번역되지 않았다(원문과 같다): ${path} = ${JSON.stringify(value)}`);
  }
  return ko;
}

function walk(node, path) {
  if (Array.isArray(node)) return node.map((v, i) => walk(v, `${path}[${i}]`));
  if (node && typeof node === 'object') {
    const out = {};
    for (const k of Object.keys(node)) out[k] = walk(node[k], path ? `${path}.${k}` : k);
    return out;
  }
  if (typeof node === 'string') return translate(path, node);
  problems.push(`문자열이 아닌 리프: ${path} (${typeof node})`);
  return node;
}

const kr = walk(en, '');
for (const k of Object.keys(map)) {
  if (!used.has(k)) problems.push(`쓰이지 않은 번역 열쇠(en.json 에 없다): ${k}`);
}

if (problems.length > 0) {
  console.error(`검사 실패 ${problems.length}건 — 파일을 쓰지 않았다`);
  for (const p of problems) console.error('  - ' + p);
  process.exit(1);
}

const text = JSON.stringify(kr, null, 2) + '\n';
for (const out of OUTPUTS) writeFileSync(out, text, 'utf8');
console.log(`OK 문구 ${used.size}개 — ${OUTPUTS.join(', ')}`);
