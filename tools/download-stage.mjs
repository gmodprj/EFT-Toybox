// doc/download/manifest-<stage>.json 의 모드를 downloads/<stage>로 스트리밍 다운로드 (대용량 대응)
// 사용: node tools/download-stage.mjs stage3
import fs from 'node:fs';
import path from 'node:path';
import { Readable } from 'node:stream';
import { pipeline } from 'node:stream/promises';

const STAGE = process.argv[2] ?? 'stage2';
const manifest = JSON.parse(fs.readFileSync(`E:/Spt_Mods/doc/download/manifest-${STAGE}.json`, 'utf8'));
const outDir = `E:/Spt_Mods/downloads/${STAGE}`;
fs.mkdirSync(outDir, { recursive: true });
const results = [];

// 작은 파일 먼저 받고 대용량은 뒤로
for (const m of [...manifest.mods].sort((a, b) => (a.size ?? 0) - (b.size ?? 0))) {
  const t0 = Date.now();
  try {
    const r = await fetch(m.link, { redirect: 'follow' });
    if (!r.ok) throw new Error(`HTTP ${r.status}`);
    const cd = r.headers.get('content-disposition') ?? '';
    let name = /filename\*?=(?:UTF-8'')?"?([^";]+)"?/i.exec(cd)?.[1] ?? path.basename(new URL(r.url).pathname);
    name = decodeURIComponent(name);
    const file = path.join(outDir, `${m.id}__${name}`);
    const part = file + '.part';
    console.log(`START ${m.id} ${m.name} -> ${name} (${((m.size ?? 0) / 1048576).toFixed(1)} MB) via ${new URL(r.url).host}`);
    await pipeline(Readable.fromWeb(r.body), fs.createWriteStream(part));
    fs.renameSync(part, file);
    const bytes = fs.statSync(file).size;
    const ok = m.size == null || m.size === bytes;
    results.push({ id: m.id, name: m.name, version: m.version, file: path.basename(file), bytes, expected: m.size, sizeOk: ok, host: new URL(r.url).host, seconds: Math.round((Date.now() - t0) / 1000) });
    console.log(`${ok ? 'OK  ' : 'SIZE'} ${m.id} ${bytes}/${m.size} ${Math.round((Date.now() - t0) / 1000)}s`);
  } catch (e) {
    results.push({ id: m.id, name: m.name, version: m.version, error: String(e) });
    console.log(`FAIL ${m.id} ${m.name}: ${e}`);
  }
  fs.writeFileSync(`E:/Spt_Mods/doc/download/download-results-${STAGE}.json`, JSON.stringify(results, null, 2));
}
console.log('DONE');
