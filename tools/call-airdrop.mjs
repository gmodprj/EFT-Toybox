#!/usr/bin/env node
// AirdropCaller 서버 라우트를 셸에서 부른다.
//
// 왜 curl 로는 안 되나 — SPT 서버는 **https + 자체 서명 인증서**이고, 응답을
// `Content-Encoding` 없이 **zlib 으로 압축해서** 보낸다. 요청 본문도 zlib 을 받는다.
// 그 왕복은 `src/AirdropCaller.App/ServerClient.cs` 가 하는 것과 같다 — 이 파일은
// 창(WPF)을 띄우지 않고 같은 일을 하는 쪽이다.
//
//   node tools/call-airdrop.mjs <라우트> [JSON 본문] [--session <24자리hex>]
//
// 보기:
//   node tools/call-airdrop.mjs /airdrop-caller/state
//   node tools/call-airdrop.mjs /airdrop-caller/queue '{"icon":"Common","items":[...]}'
//
// 세션 id 를 안 주면 `user/profiles/` 의 파일 이름 중 **가장 최근에 고쳐진 것**을 쓴다.

import https from 'node:https';
import zlib from 'node:zlib';
import fs from 'node:fs';
import path from 'node:path';

const PROFILES = 'E:/Spt/SPT_Runtime/user/profiles';
const BASE = { host: '127.0.0.1', port: 6969 };

function newestProfileId() {
  const files = fs
    .readdirSync(PROFILES)
    .filter((f) => /^[0-9a-f]{24}\.json$/.test(f))
    .map((f) => ({ id: path.basename(f, '.json'), t: fs.statSync(path.join(PROFILES, f)).mtimeMs }))
    .sort((a, b) => b.t - a.t);
  if (files.length === 0) throw new Error(`프로필을 찾지 못했다: ${PROFILES}`);
  return files[0].id;
}

function call(route, body, sessionId) {
  const payload = zlib.deflateSync(Buffer.from(JSON.stringify(body ?? {}), 'utf8'));
  return new Promise((resolve, reject) => {
    const req = https.request(
      {
        ...BASE,
        path: route,
        method: 'POST',
        rejectUnauthorized: false, // 127.0.0.1 의 자체 서명 인증서에 한해서만
        headers: {
          'Content-Type': 'application/json',
          'Content-Length': payload.length,
          Cookie: `PHPSESSID=${sessionId}`,
        },
      },
      (res) => {
        const chunks = [];
        res.on('data', (c) => chunks.push(c));
        res.on('end', () => {
          let raw = Buffer.concat(chunks);
          // zlib 머리글: 0x78 로 시작하고 두 바이트가 31 로 나누어떨어진다.
          if (raw.length >= 2 && raw[0] === 0x78 && (((raw[0] << 8) | raw[1]) % 31 === 0)) {
            try {
              raw = zlib.inflateSync(raw);
            } catch (e) {
              return reject(new Error(`응답 zlib 해제 실패: ${e.message}`));
            }
          }
          resolve({ status: res.statusCode, text: raw.toString('utf8') });
        });
      },
    );
    req.on('error', reject);
    req.write(payload);
    req.end();
  });
}

const argv = process.argv.slice(2);
let session = null;
const si = argv.indexOf('--session');
if (si >= 0) {
  session = argv[si + 1];
  argv.splice(si, 2);
}
const [route, bodyText] = argv;
if (!route) {
  console.error('쓰기: node tools/call-airdrop.mjs <라우트> [JSON 본문] [--session <id>]');
  process.exit(2);
}

// Git Bash(MSYS)는 앞이 슬래시인 인자를 **윈도 경로로 바꿔** 넘긴다 —
// `/airdrop-caller/state` 가 `E:/Program Files/Git/airdrop-caller/state` 로 온다.
// 그대로 쏘면 라우트가 아닌 것을 보내거나 node 가 터진다. 라우트 모양만 되살린다.
const routeFixed = (() => {
  const m = route.match(/\/((?:airdrop-caller|client)\/.*)$/);
  return m ? `/${m[1]}` : route;
})();
if (routeFixed !== route) {
  console.error(`(셸이 라우트를 경로로 바꿨다 — '${routeFixed}' 로 되살렸다)`);
}

const sessionId = session ?? newestProfileId();
let body = {};
if (bodyText) {
  try {
    body = JSON.parse(bodyText);
  } catch (e) {
    console.error(`본문이 JSON 이 아니다: ${e.message}`);
    process.exit(2);
  }
}

const res = await call(routeFixed, body, sessionId);
console.error(`[${res.status}] ${routeFixed}  (세션 ${sessionId})`);
try {
  console.log(JSON.stringify(JSON.parse(res.text), null, 2));
} catch {
  console.log(res.text);
}
if (res.status !== 200) process.exit(1);
