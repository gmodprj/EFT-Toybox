// 서버 웹 화면을 모드 순서대로 하위 페이지·탭·메뉴까지 돌며 미번역과 적용된 번역을 모은다.
// 사용: node tools/audit-web-ui.mjs <출력.json> [모드 ...]
//
// dump-web-ui-strings.mjs 는 최상위 페이지만 본다. 그래서 "/cci 미번역 0" 이라고 했지만
// 하위 페이지 /cci/config 에만 영문 라벨이 19개 있었다. DWV 는 "6개" 였는데 섹션 버튼을
// 눌러 보니 197개였다. 숫자가 아니라 돈 범위를 믿어야 한다.
//
// 크롤러 본문은 내장 브라우저에서 직접 돌려 검증한 것과 같다.
// - 같은 모드 경로 아래의 링크, [role=tab], nav/aside 안의 버튼만 누른다.
// - 저장·초기화·삭제·적용·만들기·내보내기 글자가 든 것은 누르지 않는다.
// - 현재 화면에 없는 링크는 Blazor.navigateTo 로 간다.
import { spawn } from 'node:child_process';
import { writeFileSync, mkdtempSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';

const OUT = process.argv[2];
if (!OUT) {
  console.error('사용: node tools/audit-web-ui.mjs <출력.json> [모드 ...]');
  process.exit(2);
}

const BASE = 'https://127.0.0.1:6969';
const PORT = 9335;
const CHROME = 'C:/Program Files/Google/Chrome/Application/chrome.exe';

// 첫 화면 모드 카드 순서.
const MODS = [
  ['bis', '/bis'],
  ['cci', '/cci'],
  ['definitive-weapon-variants', '/definitive-weapon-variants/config'],
  ['coti', '/coti'],
  ['enablelabyrinth', '/enablelabyrinth'],
  ['isb', '/isb'],
  ['jerobackpack', '/jerobackpack'],
  ['lotsofloot', '/lotsofloot'],
  ['orbit', '/orbit'],
  ['questcodex', '/questcodex'],
  ['sain', '/sain/presets'],
  ['skills-extended', '/skills-extended'],
  ['theblacklist', '/theblacklist'],
  ['ultimateflea', '/ultimateflea/app.html'],
  ['webconfigbridge', '/webconfigbridge'],
];
const CORE = ['/', '/database', '/configs', '/profiles', '/credentials', '/tools', '/status', '/thank-you', '/login'];
const only = new Set(process.argv.slice(3));

// 페이지 안에서 도는 크롤러. 결과를 바로 돌려준다.
const CRAWL = `async function(cap){
const sleep=ms=>new Promise(r=>setTimeout(r,ms));
const base='/'+(location.pathname.split('/')[1]||'');
for(let i=0;i<40&&!(window.__ko&&window.__ko.size);i++) await sleep(250);
await sleep(4500); if(!window.__ko) return {error:'no translator'}; window.__ko.rescan(); await sleep(300);
const pages={}; const seen=new Set(); const queue=[location.pathname]; const prev=new Set();
const BAD=/reset|delete|remove|wipe|logout|export|download|import|apply|create|save|reload|discard|restart|저장|되돌리기|다시 읽기|삭제|초기화|적용|만들기|버리기|지우기|내보내기|들여오기|복제/i;
const links=()=>[...document.querySelectorAll('a[href]')].map(x=>{try{return new URL(x.getAttribute('href'),location.href)}catch{return null}}).filter(u=>u&&u.origin===location.origin&&(base==='/'?false:u.pathname.startsWith(base))&&!BAD.test(u.pathname)&&!/\.(js|css|png|json|zip)$/i.test(u.pathname)).map(u=>u.pathname+u.hash);
const grab=(key)=>{document.querySelectorAll('details').forEach(d=>d.open=true); window.__ko.rescan(); const now=window.__ko.missing.map(m=>m.text); const nw=now.filter(s=>!prev.has(s)); now.forEach(s=>prev.add(s)); const f=nw.filter(s=>!/[○●]\s*$/.test(s)); if(f.length||!pages[key]) pages[key]={missing:f};};
let first=true;
while(queue.length&&seen.size<cap){
 const p=queue.shift(); if(seen.has(p))continue; seen.add(p);
 if(!first){const a=[...document.querySelectorAll('a[href]')].find(x=>{try{const u=new URL(x.getAttribute('href'),location.href);return u.pathname+u.hash===p}catch{return false}}); if(a){a.click();} else if(window.Blazor&&window.Blazor.navigateTo){window.Blazor.navigateTo(p);} else continue; await sleep(3000);}
 first=false;
 await sleep(300); grab(location.pathname+location.hash);
 for(const t of [...document.querySelectorAll('[role=tab]')]){ if(BAD.test(t.innerText))continue; t.click(); await sleep(800); grab(location.pathname+'#tab:'+t.innerText.trim().slice(0,30)); }
 const navB=[...document.querySelectorAll('nav button, aside button')].filter(b=>b.innerText.trim()&&!BAD.test(b.innerText)&&!b.closest('form'));
 for(const b of navB){ const n=b.innerText.trim().slice(0,30); b.click(); await sleep(1000); grab(location.pathname+'#btn:'+n); }
 for(const l of links()) if(!seen.has(l)) queue.push(l);
}
return {pages, seen:[...seen], capped: queue.length>0, applied: window.__ko.applied};
}`;

const CRAWL_CORE = `async function(paths){
const sleep=ms=>new Promise(r=>setTimeout(r,ms));
for(let i=0;i<40&&!(window.__ko&&window.__ko.size);i++) await sleep(250);
await sleep(4000); if(!window.__ko) return {error:'no translator'};
const BAD=/저장|되돌리기|다시 읽기|삭제|초기화|적용|만들기|버리기|지우기|내보내기|들여오기|Save|Reset|Delete|Create|Remove/i;
const prev=new Set(); const pages={};
const grab=(k)=>{window.__ko.rescan(); const now=window.__ko.missing.map(m=>m.text); const nw=now.filter(s=>!prev.has(s)); now.forEach(s=>prev.add(s)); const f=nw.filter(s=>!/[○●]\s*$/.test(s)); if(f.length||!pages[k]) pages[k]={missing:f};};
for(const p of paths){ if(p!==location.pathname) window.Blazor.navigateTo(p); await sleep(4000); grab(p);
 for(const t of [...document.querySelectorAll('[role=tab]')]){ if(BAD.test(t.innerText))continue; t.click(); await sleep(900); grab(p+'#tab:'+t.innerText.trim().slice(0,25)); } }
return {pages, seen:paths, capped:false, applied: window.__ko.applied};
}`;

const profile = mkdtempSync(join(tmpdir(), 'ko-audit-'));
const chrome = spawn(CHROME, [
  '--headless=new', '--disable-gpu', '--ignore-certificate-errors', '--no-first-run',
  '--no-default-browser-check', '--disable-extensions',
  `--remote-debugging-port=${PORT}`, `--user-data-dir=${profile}`,
  '--window-size=1600,1200', 'about:blank',
], { stdio: 'ignore' });

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

async function target() {
  for (let i = 0; i < 60; i++) {
    try {
      // Chrome 디버거 목록은 루프백에서만 열린다.
      const list = await (await fetch(`http://127.0.0.1:${PORT}/json/list`)).json();
      const p = list.find((t) => t.type === 'page');
      if (p?.webSocketDebuggerUrl) return p.webSocketDebuggerUrl;
    } catch {
      /* 아직 */
    }
    await sleep(500);
  }
  throw new Error('Chrome 디버거에 붙지 못했다');
}

function connect(url) {
  const ws = new WebSocket(url);
  let id = 0;
  const waiting = new Map();
  ws.addEventListener('message', (ev) => {
    const m = JSON.parse(ev.data);
    const s = waiting.get(m.id);
    if (!s) return;
    waiting.delete(m.id);
    m.error ? s.reject(new Error(m.error.message)) : s.resolve(m.result);
  });
  const open = new Promise((res, rej) => {
    ws.addEventListener('open', res, { once: true });
    ws.addEventListener('error', rej, { once: true });
  });
  const send = (method, params) =>
    new Promise((resolve, reject) => {
      const mine = ++id;
      waiting.set(mine, { resolve, reject });
      ws.send(JSON.stringify({ id: mine, method, params }));
    });
  return { open, send, close: () => ws.close() };
}

async function run(send, fnSource, arg) {
  const r = await send('Runtime.evaluate', {
    expression: `(${fnSource})(${JSON.stringify(arg)})`,
    awaitPromise: true,
    returnByValue: true,
    timeout: 600000,
  });
  if (r.exceptionDetails) throw new Error(r.exceptionDetails.exception?.description ?? r.exceptionDetails.text);
  return r.result.value;
}

const result = {};
try {
  const { open, send, close } = connect(await target());
  await open;
  await send('Page.enable');
  await send('Runtime.enable');

  for (const [mod, path] of MODS) {
    if (only.size && !only.has(mod)) continue;
    await send('Page.navigate', { url: BASE + path });
    await sleep(1500);
    const t0 = Date.now();
    const v = await run(send, CRAWL, 80);
    if (v.error) {
      console.log(`${mod.padEnd(28)} ${v.error}`);
      result[mod] = v;
      continue;
    }
    const n = Object.values(v.pages).reduce((a, p) => a + p.missing.length, 0);
    console.log(`${mod.padEnd(28)} 화면 ${String(Object.keys(v.pages).length).padStart(3)} / 미번역 ${String(n).padStart(5)}${v.capped ? '  (상한에 걸림)' : ''}  ${Math.round((Date.now() - t0) / 1000)}s`);
    result[mod] = v;
  }

  if (!only.size || only.has('core')) {
    await send('Page.navigate', { url: BASE + '/' });
    await sleep(1500);
    const v = await run(send, CRAWL_CORE, CORE);
    const n = Object.values(v.pages).reduce((a, p) => a + p.missing.length, 0);
    console.log(`${'core'.padEnd(28)} 화면 ${String(Object.keys(v.pages).length).padStart(3)} / 미번역 ${String(n).padStart(5)}`);
    result.core = v;
  }

  writeFileSync(OUT, JSON.stringify(result, null, 1), 'utf8');
  console.log(`\n→ ${OUT}`);
  close();
} finally {
  chrome.kill();
  try {
    rmSync(profile, { recursive: true, force: true });
  } catch {
    /* 그만 */
  }
}
