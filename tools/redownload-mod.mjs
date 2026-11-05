// 매니페스트의 한 모드만 다시 받는다.
//
// `download-stage.mjs` 는 **건너뛰기가 없어** 다시 돌리면 그 단계 전체를 다시 받는다.
// 2026-11-05 에 10-O 의 3022(2.3 GB)만 전송이 끊겼는데, 그대로 다시 돌리면 5.25 GB 를
// 전부 다시 받아야 했다. 받는 곳이 호스트마다 다르고(그 건은 Cloudflare R2 였고 3 MB/분
// 까지 떨어졌다) 큰 것 하나만 실패하는 일이 잦아 따로 둔다.
//
// 사용: node tools/redownload-mod.mjs <stage> <id>
//   예: node tools/redownload-mod.mjs stage10-O 3022
//
// 남아 있던 `.part` 조각은 먼저 지운다(이어받기는 하지 않는다). 네 번까지 다시 걸고,
// 바이트 수가 매니페스트와 어긋나면 받은 파일을 지우고 실패로 끝낸다.
import fs from 'node:fs';
import { pipeline } from 'node:stream/promises';
import { Readable } from 'node:stream';

const stage = process.argv[2];
const id = Number(process.argv[3]);
const man = JSON.parse(fs.readFileSync(`E:/Spt_Mods/doc/download/manifest-${stage}.json`, 'utf8'));
const mod = man.mods.find((m) => m.id === id);
if (!mod) throw new Error('매니페스트에 없다: ' + id);

const dir = `E:/Spt_Mods/downloads/${stage}`;
fs.mkdirSync(dir, { recursive: true });

// 남아 있는 조각을 지운다.
for (const f of fs.readdirSync(dir)) {
  if (f.startsWith(id + '__') && f.endsWith('.part')) {
    fs.unlinkSync(dir + '/' + f);
    console.log('조각 삭제: ' + f);
  }
}

const tries = 4;
for (let attempt = 1; attempt <= tries; attempt++) {
  try {
    const r = await fetch(mod.link, { redirect: 'follow' });
    if (!r.ok) throw new Error(r.status + ' ' + r.statusText);
    const name = (r.headers.get('content-disposition') ?? '').match(/filename="?([^";]+)"?/)?.[1]
      ?? mod.link.split('/').pop();
    const file = `${dir}/${id}__${name}`;
    const part = file + '.part';
    console.log(`시도 ${attempt}/${tries} -> ${name}`);
    await pipeline(Readable.fromWeb(r.body), fs.createWriteStream(part));
    fs.renameSync(part, file);
    const got = fs.statSync(file).size;
    console.log(`${got === mod.size ? 'OK  ' : '어긋남 '}${id}  ${got}/${mod.size}  ${name}`);
    if (got !== mod.size) { fs.unlinkSync(file); throw new Error('바이트 수가 어긋난다'); }
    process.exit(0);
  } catch (e) {
    console.log(`시도 ${attempt} 실패: ${e.message}`);
    for (const f of fs.readdirSync(dir)) if (f.startsWith(id + '__') && f.endsWith('.part')) fs.unlinkSync(dir + '/' + f);
  }
}
console.log('네 번 모두 실패했다');
process.exit(1);
