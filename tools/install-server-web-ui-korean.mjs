// 서버 웹 화면 번역기(ko-ui.js)를 설치/제거한다.
// 사용: node tools/install-server-web-ui-korean.mjs [--remove] [--spt E:\Spt]
//
// 모든 페이지가 공통으로 불러오는 _framework/blazor.web.js 끝에 로더 한 줄을 붙인다.
// 그 파일 옆에 .br/.gz 미리 압축본이 있어서, 원본만 고치면 브라우저가 압축본을 받아 가
// 고친 것이 무시된다. 그래서 압축본도 같이 다시 만든다.
import { readFileSync, writeFileSync, existsSync, mkdirSync, copyFileSync, rmSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { gzipSync, brotliCompressSync, constants } from 'node:zlib';

const repo = join(dirname(fileURLToPath(import.meta.url)), '..');
const argv = process.argv.slice(2);
const remove = argv.includes('--remove');
const sptDir = (() => {
  const i = argv.indexOf('--spt');
  return i >= 0 ? argv[i + 1] : 'E:\\Spt';
})();

const wwwroot = join(sptDir, 'SPT_Runtime', 'SPT_Data', 'wwwroot');
const hostJs = join(wwwroot, '_framework', 'blazor.web.js');
const jsDir = join(wwwroot, 'SPTarkov.Server.Web', 'js');

const BEGIN = '/* === spt_mods ko-ui begin — tools/install-server-web-ui-korean.mjs === */';
const END = '/* === spt_mods ko-ui end === */';

const LOADER = `${BEGIN}
try{(function(){var s=document.createElement('script');s.src='/SPTarkov.Server.Web/js/ko-ui.js';s.defer=true;(document.head||document.documentElement).appendChild(s);})();}catch(e){console.error('[ko-ui] 로더 실패',e);}
${END}`;

if (!existsSync(hostJs)) {
  console.error(`호스트 스크립트가 없다: ${hostJs}`);
  process.exit(1);
}

// 이미 붙어 있던 블록은 먼저 떼어 낸다. 몇 번을 돌려도 결과가 같아야 한다.
const strip = (text) => {
  const a = text.indexOf(BEGIN);
  if (a < 0) return { text, had: false };
  const b = text.indexOf(END, a);
  if (b < 0) {
    console.error('블록 시작은 있는데 끝이 없다. 손으로 확인하라.');
    process.exit(1);
  }
  return { text: text.slice(0, a).replace(/\n+$/, '\n') + text.slice(b + END.length).replace(/^\n+/, ''), had: true };
};

const original = readFileSync(hostJs, 'utf8');
const { text: bare, had } = strip(original);

const writeAll = (text) => {
  writeFileSync(hostJs, text, 'utf8');
  const buf = Buffer.from(text, 'utf8');
  writeFileSync(`${hostJs}.gz`, gzipSync(buf, { level: 9 }));
  writeFileSync(
    `${hostJs}.br`,
    brotliCompressSync(buf, { params: { [constants.BROTLI_PARAM_QUALITY]: 11, [constants.BROTLI_PARAM_SIZE_HINT]: buf.length } })
  );
};

if (remove) {
  if (!had) {
    console.log('붙어 있지 않다. 그대로 둔다.');
  } else {
    writeAll(bare);
    console.log(`로더를 떼어 냈다: ${hostJs} (+ .gz, .br)`);
  }
  for (const f of ['ko-ui.js', 'ko-ui.json', 'ko-items.json']) {
    const p = join(jsDir, f);
    if (existsSync(p)) {
      rmSync(p);
      console.log(`지움: ${p}`);
    }
  }
  console.log('브라우저를 새로고침하면 화면이 원래 영문으로 돌아간다.');
  process.exit(0);
}

// 처음 설치할 때만 원본을 뜬다. 두 번째부터는 이미 뜬 것이 원본이다.
const backupDir = join(repo, 'backup', 'server-web-ui-pre');
if (!had) {
  mkdirSync(backupDir, { recursive: true });
  for (const ext of ['', '.gz', '.br']) {
    const src = `${hostJs}${ext}`;
    if (existsSync(src)) copyFileSync(src, join(backupDir, `blazor.web.js${ext}`));
  }
  console.log(`원본을 떠 뒀다: ${backupDir}`);
}

writeAll(`${bare.replace(/\n*$/, '\n')}${LOADER}\n`);
console.log(`로더를 붙였다: ${hostJs} (+ .gz, .br 다시 만듦)`);

mkdirSync(jsDir, { recursive: true });
for (const f of ['ko-ui.js', 'ko-ui.json', 'ko-items.json']) {
  copyFileSync(join(repo, 'src', 'ServerWebUiKorean', f), join(jsDir, f));
  console.log(`설치됨: ${join(jsDir, f)}`);
}

console.log('서버 재기동은 필요 없다. 브라우저를 강제 새로고침(Ctrl+F5)하라.');
