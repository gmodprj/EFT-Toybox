// 서버 웹 첫 화면의 "모드 페이지" 카드가 사전(src/ModCardKorean/mod-cards.ko.json)과 맞는지 본다.
// 모드가 들고 나면 카드 목록이 바뀌므로, 돌고 있는 서버가 실제로 내보내는 목록과 대조한다.
// 사용: node tools/check-mod-card-locale.mjs [서버 주소]
import { readFileSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { request } from 'node:https';

const repo = join(dirname(fileURLToPath(import.meta.url)), '..');
const base = process.argv[2] ?? 'https://127.0.0.1:6969/';
const dictPath = join(repo, 'src', 'ModCardKorean', 'mod-cards.ko.json');

// SPT 서버는 자가 서명 인증서를 쓴다. 로컬 서버만 보므로 검증을 끈다.
const fetchLanding = (url) =>
  new Promise((resolve, reject) => {
    const req = request(url, { rejectUnauthorized: false, timeout: 20000 }, (res) => {
      if (res.statusCode !== 200) {
        reject(new Error(`HTTP ${res.statusCode}`));
        return;
      }
      let body = '';
      res.setEncoding('utf8');
      res.on('data', (c) => (body += c));
      res.on('end', () => resolve(body));
    });
    req.on('timeout', () => req.destroy(new Error('시간 초과')));
    req.on('error', reject);
    req.end();
  });

let html;
try {
  html = await fetchLanding(base);
} catch (err) {
  // 읽지 못하면 "문제 없음"이 아니라 "아직 판정 못 함"이다. 닫힌 쪽으로 넘어뜨린다.
  console.log(`서버를 읽지 못했다(${base}): ${err.message}`);
  console.log('서버를 켠 뒤 다시 돌려라. 판정하지 않는다.');
  process.exit(2);
}

const start = html.indexOf('landing-mod-grid');
if (start < 0) {
  console.log('첫 화면에 "모드 페이지" 구역이 없다. 카드를 내보내는 모드가 하나도 없는 상태다.');
  process.exit(2);
}
const seg = html.slice(start);

const grab = (re) => [...seg.matchAll(re)].map((m) => m[1]);
const hrefs = grab(/landing-mod-link-card spt-card" href="([^"]+)"/g);
const titles = grab(/landing-link-title[^>]*>([^<]+)</g);
const descs = grab(/landing-link-description[^>]*>([^<]+)</g);

if (hrefs.length === 0 || hrefs.length !== titles.length || hrefs.length !== descs.length) {
  console.log(`카드를 제대로 읽지 못했다(경로 ${hrefs.length} / 제목 ${titles.length} / 설명 ${descs.length}).`);
  process.exit(2);
}

const dict = JSON.parse(readFileSync(dictPath, 'utf8'));
const dictKeys = Object.keys(dict).filter((k) => !k.startsWith('_'));

const live = hrefs.map((href, i) => ({ href, title: titles[i], desc: descs[i] }));

// 1. 서버에는 있는데 사전에 없는 카드 → 영어로 남는다
const missing = live.filter((c) => !dictKeys.includes(c.href));
// 2. 사전에는 있는데 서버에 없는 카드 → 모드를 뺐거나 경로가 바뀌었다
const stale = dictKeys.filter((k) => !hrefs.includes(k));
// 3. 사전에 있는데 값이 비어 있는 줄
const empty = dictKeys.filter((k) => !dict[k]?.description && !dict[k]?.name);
// 4. 설치돼 있다면 화면에 한글이 찍혀야 한다(&#x…; 로 넘어오는 것도 한글이다)
const hangul = /[가-힣]|&#x[CDcd][0-9A-Fa-f]{3};/;
const stillEnglish = live.filter((c) => dictKeys.includes(c.href) && !hangul.test(c.desc));

console.log(`서버 카드 ${live.length} 개 / 사전 ${dictKeys.length} 줄`);
if (missing.length)
  console.log(`사전에 없음(${missing.length}): ${missing.map((c) => `${c.title} ${c.href}`).join(', ')}`);
if (stale.length) console.log(`서버에 없음(${stale.length}): ${stale.join(', ')}`);
if (empty.length) console.log(`값이 빈 줄(${empty.length}): ${empty.join(', ')}`);
if (stillEnglish.length)
  console.log(
    `사전에 있는데 화면은 원문(${stillEnglish.length}): ${stillEnglish.map((c) => c.href).join(', ')}` +
      ' — 모드가 설치됐는지, 서버를 다시 켰는지 본다'
  );

const bad = missing.length + stale.length + empty.length + stillEnglish.length;
console.log(bad === 0 ? `OK — ${live.length}/${live.length}` : `문제 ${bad} 건`);
process.exit(bad === 0 ? 0 : 1);
