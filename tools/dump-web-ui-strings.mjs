// 서버 웹 화면을 실제로 렌더해 미번역 문자열을 긁어온다.
// 사용: node tools/dump-web-ui-strings.mjs [출력파일]
//
// Blazor Server 는 웹소켓이 붙은 뒤에야 본문이 채워지므로 프리렌더만 봐서는 안 된다.
// 로컬 Chrome 을 headless + CDP 로 몰아 하이드레이션까지 기다린 뒤
// 화면에 끼워 둔 번역기가 모아 둔 window.__ko.missing 을 꺼낸다.
import { spawn } from 'node:child_process';
import { writeFileSync, mkdtempSync, rmSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';

const OUT = process.argv[2] ?? join(process.cwd(), 'web-ui-missing.json');
const PORT = 9333;
const BASE = 'https://127.0.0.1:6969';

const PAGES = [
  '/', '/database', '/configs', '/profiles', '/credentials', '/tools', '/status', '/thank-you',
  '/bis', '/cci', '/definitive-weapon-variants/config', '/coti', '/enablelabyrinth', '/isb',
  '/jerobackpack', '/lotsofloot', '/orbit', '/questcodex', '/sain/presets', '/skills-extended',
  '/theblacklist', '/ultimateflea/app.html', '/webconfigbridge',
];

const CHROME = 'C:/Program Files/Google/Chrome/Application/chrome.exe';
const profile = mkdtempSync(join(tmpdir(), 'ko-ui-'));

const chrome = spawn(CHROME, [
  '--headless=new', '--disable-gpu', '--ignore-certificate-errors', '--no-first-run',
  '--no-default-browser-check', '--disable-extensions',
  `--remote-debugging-port=${PORT}`, `--user-data-dir=${profile}`,
  '--window-size=1600,1200', 'about:blank',
], { stdio: 'ignore' });

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

async function cdpTarget() {
  for (let i = 0; i < 60; i++) {
    try {
      const list = await (await fetch(`http://127.0.0.1:${PORT}/json/list`)).json();
      const page = list.find((t) => t.type === 'page');
      if (page?.webSocketDebuggerUrl) return page.webSocketDebuggerUrl;
    } catch {
      /* 아직 안 떴다 */
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
    const msg = JSON.parse(ev.data);
    const slot = waiting.get(msg.id);
    if (!slot) return;
    waiting.delete(msg.id);
    msg.error ? slot.reject(new Error(msg.error.message)) : slot.resolve(msg.result);
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

const evaluate = async (send, expression) => {
  const r = await send('Runtime.evaluate', { expression, returnByValue: true, awaitPromise: true });
  if (r.exceptionDetails) throw new Error(r.exceptionDetails.text);
  return r.result.value;
};

try {
  const { open, send, close } = connect(await cdpTarget());
  await open;
  await send('Page.enable');
  await send('Runtime.enable');

  const all = new Map();
  const report = [];

  for (const path of PAGES) {
    await send('Page.navigate', { url: BASE + path });
    // 하이드레이션(웹소켓 연결 → 본문 렌더)을 기다린다. 한 방에 읽으면 빈 화면을 읽는다.
    await sleep(6500);

    let missing = [];
    let loaded = false;
    try {
      loaded = await evaluate(send, 'typeof window.__ko === "object"');
      if (loaded) {
        await evaluate(send, 'window.__ko.rescan()');
        missing = await evaluate(send, 'JSON.parse(JSON.stringify(window.__ko.missing))');
      }
    } catch (err) {
      report.push(`${path}\t오류: ${err.message}`);
      continue;
    }

    if (!loaded) {
      report.push(`${path}\t번역기가 안 떴다`);
      continue;
    }

    for (const m of missing) all.set(m.text, (all.get(m.text) ?? 0) + m.seen);
    report.push(`${path}\t미번역 ${missing.length}`);
  }

  const sorted = [...all.keys()].sort((a, b) => a.localeCompare(b));
  writeFileSync(OUT, JSON.stringify(Object.fromEntries(sorted.map((k) => [k, ''])), null, 2), 'utf8');

  console.log(report.join('\n'));
  console.log(`\n합계 미번역 ${sorted.length} 개 → ${OUT}`);
  close();
} finally {
  chrome.kill();
  try {
    rmSync(profile, { recursive: true, force: true });
  } catch {
    /* 지워지지 않아도 그만 */
  }
}
