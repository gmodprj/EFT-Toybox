// 매니페스트의 한 모드를 Range 요청으로 여러 줄기 나눠 받는다.
//
// 왜 — 호스트가 한 줄기를 조여 놓는 경우가 있다. 2026-11-09 의 ISB Aishi(4.4GB)는
// `isb-downloads.samc137.workers.dev`(Cloudflare Worker, LAX 엣지)에서 **1.6KB/s** 까지
// 떨어져 33분에 8MB 밖에 못 받았다. 같은 파일을 Range 로 끊어 받으면 한 줄기가 56KB/s,
// 여덟 줄기면 273KB/s 이상이었다. `download-stage.mjs`·`redownload-mod.mjs` 는 한 줄기다.
//
// 사용: node tools/download-ranged.mjs <stage> <id> [동시 줄기 수=12] [조각 MB=8]
//   예: node tools/download-ranged.mjs update-2026-11-09-uf 2478
//
// - 먼저 `Accept-Ranges: bytes` 와 `Content-Length` 를 확인한다. Range 를 안 받으면 멈춘다
//   (조용히 한 줄기로 떨어지지 않는다 — 그건 download-stage 가 할 일이다).
// - 받은 조각은 `<파일>.part` 의 제자리에 쓰고 `<파일>.state.json` 에 적는다. 다시 돌리면
//   **안 받은 조각만** 받는다.
// - 다 받으면 바이트 수를 매니페스트의 size 와 견주고, 맞을 때만 `.part` 를 벗긴다.
// - 오래 걸리므로 터미널 탭에서 돌린다(백그라운드는 턴을 넘기면 죽는다).
import fs from 'node:fs';
import path from 'node:path';

const stage = process.argv[2];
const id = Number(process.argv[3]);
const LANES = Number(process.argv[4] ?? 12);
const CHUNK = Number(process.argv[5] ?? 8) * 1024 * 1024;
if (!stage || !id) { console.error('사용: node tools/download-ranged.mjs <stage> <id> [줄기=12] [조각MB=8]'); process.exit(2); }

const ROOT = 'E:/Spt_Mods';
const man = JSON.parse(fs.readFileSync(`${ROOT}/doc/download/manifest-${stage}.json`, 'utf8'));
const mod = man.mods.find((m) => m.id === id);
if (!mod) { console.error(`매니페스트에 ${id} 가 없다`); process.exit(2); }

const outDir = `${ROOT}/downloads/${stage}`;
fs.mkdirSync(outDir, { recursive: true });

// 1) 리다이렉트를 따라가 최종 주소와 길이를 본다
const head = await fetch(mod.link, { method: 'HEAD', redirect: 'follow' });
if (!head.ok) { console.error(`HEAD 실패 ${head.status}`); process.exit(1); }
const finalUrl = head.url;
const total = Number(head.headers.get('content-length') ?? 0);
const ranges = (head.headers.get('accept-ranges') ?? '').toLowerCase();
let name = /filename\*?=(?:UTF-8'')?"?([^";]+)"?/i.exec(head.headers.get('content-disposition') ?? '')?.[1]
  ?? path.basename(new URL(finalUrl).pathname);
name = decodeURIComponent(name);
const file = path.join(outDir, `${mod.id}__${name}`);
const part = `${file}.part`;
const stateAt = `${file}.state.json`;

console.log(`${mod.id} ${mod.name} ${mod.version}`);
console.log(`  받는 곳 ${new URL(finalUrl).host}`);
console.log(`  길이 ${total}B (매니페스트 ${mod.size ?? '-'}B) / Accept-Ranges: ${ranges || '없음'}`);
if (!total) { console.error('Content-Length 가 없다 — 끊어 받을 수 없다'); process.exit(1); }
if (!ranges.includes('bytes')) { console.error('이 호스트는 Range 를 받지 않는다 — download-stage.mjs 로 받는다'); process.exit(1); }
if (mod.size != null && total !== mod.size) console.log(`  주의: 길이가 매니페스트와 다르다`);

// 2) 조각 목록과 이어받기 상태
const chunks = [];
for (let s = 0; s < total; s += CHUNK) chunks.push({ i: chunks.length, start: s, end: Math.min(s + CHUNK, total) - 1 });
let done = new Set();
if (fs.existsSync(stateAt) && fs.existsSync(part) && fs.statSync(part).size === total) {
  try {
    const st = JSON.parse(fs.readFileSync(stateAt, 'utf8'));
    if (st.url === finalUrl && st.total === total && st.chunk === CHUNK) done = new Set(st.done);
  } catch { /* 상태가 깨졌으면 처음부터 */ }
}
if (!fs.existsSync(part) || fs.statSync(part).size !== total) {
  fs.writeFileSync(part, Buffer.alloc(0));
  fs.truncateSync(part, total);   // 자리를 먼저 잡는다
  done = new Set();
}
const saveState = () => fs.writeFileSync(stateAt, JSON.stringify({ url: finalUrl, total, chunk: CHUNK, done: [...done] }));
console.log(`  조각 ${chunks.length}개 (${CHUNK / 1048576}MB) / 이미 받은 것 ${done.size}개 / 줄기 ${LANES}`);

// 3) 줄기마다 조각을 하나씩 집어 받는다
const fd = fs.openSync(part, 'r+');
let next = 0, got = done.size, failed = 0;
const t0 = Date.now();
let lastLog = 0;

const pull = async (lane) => {
  for (;;) {
    const c = chunks[next++];
    if (!c) return;
    if (done.has(c.i)) continue;
    let ok = false;
    for (let a = 0; a < 5 && !ok; a++) {
      try {
        const r = await fetch(finalUrl, { headers: { Range: `bytes=${c.start}-${c.end}` } });
        if (r.status !== 206) throw new Error(`HTTP ${r.status}`);
        const buf = Buffer.from(await r.arrayBuffer());
        if (buf.length !== c.end - c.start + 1) throw new Error(`길이 ${buf.length} != ${c.end - c.start + 1}`);
        fs.writeSync(fd, buf, 0, buf.length, c.start);
        done.add(c.i); got++; ok = true;
      } catch (e) {
        if (a === 4) { failed++; console.log(`  조각 ${c.i} 실패: ${e.message}`); }
        else await new Promise((s) => setTimeout(s, 2000 * (a + 1)));
      }
    }
    const now = Date.now();
    if (now - lastLog > 15000) {
      lastLog = now;
      const mb = (got * CHUNK) / 1048576, el = (now - t0) / 1000;
      console.log(`  ${got}/${chunks.length} 조각 · ${mb.toFixed(0)}MB · ${(mb * 1024 / el).toFixed(0)}KB/s · 남은 시간 약 ${(((chunks.length - got) * CHUNK / 1048576) / (mb / el) / 60).toFixed(0)}분`);
      saveState();
    }
  }
};
await Promise.all(Array.from({ length: LANES }, (_, i) => pull(i)));
fs.closeSync(fd);
saveState();

// 4) 다 받았는지 보고 이름을 벗긴다
if (done.size !== chunks.length) {
  console.log(`\n아직 ${chunks.length - done.size}개 조각이 남았다(실패 ${failed}). 같은 명령을 다시 돌리면 그것만 받는다.`);
  process.exit(1);
}
const bytes = fs.statSync(part).size;
if (mod.size != null && bytes !== mod.size) {
  console.error(`\n바이트 수가 매니페스트와 다르다: ${bytes} != ${mod.size}. ${part} 를 지우고 다시 받는다.`);
  process.exit(1);
}
fs.renameSync(part, file);
fs.unlinkSync(stateAt);
console.log(`\nOK — ${path.basename(file)} ${bytes}B, ${((Date.now() - t0) / 60000).toFixed(1)}분`);
