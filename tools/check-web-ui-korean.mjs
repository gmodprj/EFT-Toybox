// 번역기가 실제로 화면을 바꾸는지 렌더해서 본다.
// 사용: node tools/check-web-ui-korean.mjs [경로 ...]
import { spawn } from 'node:child_process';
import { mkdtempSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';

const PAGES = process.argv.slice(2).length ? process.argv.slice(2) : ['/login'];
const PORT = 9334;
const BASE = 'https://127.0.0.1:6969';
const CHROME = 'C:/Program Files/Google/Chrome/Application/chrome.exe';

const profile = mkdtempSync(join(tmpdir(), 'ko-chk-'));
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

try {
  const { open, send, close } = connect(await target());
  await open;
  await send('Page.enable');
  await send('Runtime.enable');

  for (const path of PAGES) {
    await send('Page.navigate', { url: BASE + path });
    await sleep(6500);
    const r = await send('Runtime.evaluate', {
      expression:
        'JSON.stringify({ko: typeof window.__ko==="object", size: window.__ko?window.__ko.size:0, text: document.body.innerText.replace(/\\s+/g," ").slice(0,500)})',
      returnByValue: true,
    });
    const v = JSON.parse(r.result.value);
    console.log(`\n### ${path}  번역기=${v.ko ? '떴다' : '안 떴다'} 사전=${v.size}`);
    console.log(v.text);
  }
  close();
} finally {
  chrome.kill();
  try {
    rmSync(profile, { recursive: true, force: true });
  } catch {
    /* 그만 */
  }
}
