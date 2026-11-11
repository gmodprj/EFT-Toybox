// Forge 모드 페이지의 댓글을 읽어 온다. (읽기 전용)
//
// Forge API v0 에는 댓글·리뷰 길이 없다(`/mod/<id>/comments`·`/reviews`·`/ratings` 모두 404,
// `include=comments` 는 INVALID_QUERY_PARAMETER). 댓글 탭은 Livewire 지연 컴포넌트라
// 페이지 HTML 에도 본문이 없다. 그래서 브라우저가 하는 일을 그대로 한다:
//   ① 모드 페이지를 받아 csrf 토큰·세션 쿠키·댓글 탭의 wire:snapshot·__lazyLoad 인자를 꺼내고
//   ② 페이지가 알려 주는 Livewire 갱신 주소(`"uri":"…/update"`, 배포마다 접두어가 바뀐다)로
//      `__lazyLoad` 를 POST 해서
//   ③ 돌아온 HTML 에서 `작성자 | 날짜 | 버전 | Written for … | 본문 | Report Content` 를 집는다.
//
// 사용:
//   node tools/fetch-mod-comments.mjs <id>[,<id>...] [--pages N] [--json <경로>] [--all]
//   node tools/fetch-mod-comments.mjs --installed [--pages N] [--json <경로>]
//
// `--installed` 는 `doc/INSTALLED-MODS.md` 의 Forge 링크를 쓴다(그 문서는 설치 기록에서 생성된다).
// 기본은 성능·메모리 낱말이 있는 댓글만 출력하고, `--all` 은 전부 낸다.
// 실패를 "댓글 없음" 으로 삼키지 않는다 — 모드마다 error 를 적고 끝에 실패 수를 낸다.
import fs from 'node:fs';
import path from 'node:path';

const ROOT = path.resolve(path.dirname(new URL(import.meta.url).pathname.replace(/^\/([A-Z]:)/, '$1')), '..');
const BASE = 'https://sp-mod.com';
const UA = 'EFT-Toybox mod-comment-reader/1.0 (local modlist audit)';

const argv = process.argv.slice(2);
const flag = (n) => argv.includes(n);
const val = (n, d) => (argv.includes(n) ? argv[argv.indexOf(n) + 1] : d);
const PAGES = Math.max(1, Number(val('--pages', 1)));
const JSON_AT = val('--json', null);
const ALL = flag('--all');

// 성능·메모리와 이어지는 낱말. 본문을 소문자로 바꿔 찾는다.
export const PERF_RE = /\b(fps|frame ?rate|frames|stutter\w*|hitch\w*|lag\w*|freez\w*|hang\w*|memory|ram|vram|leak\w*|perf\w*|optimi\w*|slow\w*|cpu|gpu|crash\w*|load ?time|loading time|long load|unload\w*|cull\w*|garbage|gc\b|heap|out of memory|oom)\b/i;

const wait = (ms) => new Promise((r) => setTimeout(r, ms));
const un = (s) => s.replace(/&quot;/g, '"').replace(/&#039;/g, "'").replace(/&nbsp;/g, ' ')
  .replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&amp;/g, '&');

function installedIds() {
  const md = fs.readFileSync(path.join(ROOT, 'doc', 'INSTALLED-MODS.md'), 'utf8');
  return [...new Set([...md.matchAll(/sp-mod\.com\/mod\/(\d+)\)/g)].map((m) => m[1]))];
}

/** 한 모드의 댓글 한 페이지. */
async function fetchPage(id, page) {
  const url = `${BASE}/mod/${id}/x${page > 1 ? `?page=${page}` : ''}`;
  const r = await fetch(url, { headers: { Accept: 'text/html', 'User-Agent': UA } });
  if (!r.ok) return { error: `page ${r.status}` };
  const html = await r.text();
  const cookie = (r.headers.getSetCookie?.() ?? []).map((c) => c.split(';')[0]).join('; ');
  const csrf = html.match(/csrf":"([A-Za-z0-9]+)"/)?.[1];
  if (!csrf) return { error: 'csrf 토큰을 못 찾았다' };

  const blk = html.split('<div wire:key=').slice(1).find((b) => b.startsWith('"comments-tab-'));
  if (!blk) return { error: '댓글 탭 컴포넌트가 없다' };
  const snapshot = un(blk.match(/wire:snapshot="([^"]+)"/)?.[1] ?? '');
  const lazy = un(blk.match(/\$wire\.__lazyLoad\(&#039;([^&]+)&#039;\)/)?.[1] ?? '');
  if (!snapshot) return { error: 'snapshot 이 없다' };

  const rawUri = html.match(/"uri":"([^"]*update)"/)?.[1] ?? 'livewire/update';
  const cleaned = un(rawUri).replace(/\\\//g, '/');
  const uri = /^https?:\/\//.test(cleaned) ? cleaned : `${BASE}/${cleaned.replace(/^\//, '')}`;

  const p = await fetch(uri, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json', Accept: 'text/html, application/xhtml+xml',
      'X-CSRF-TOKEN': csrf, 'X-Livewire': '1', Referer: url, Cookie: cookie, 'User-Agent': UA,
    },
    body: JSON.stringify({ _token: csrf, components: [{ snapshot, updates: {}, calls: [{ path: '', method: '__lazyLoad', params: [lazy] }] }] }),
  });
  if (!p.ok) return { error: `livewire ${p.status}` };
  const body = await p.json();
  const htmlOut = (body.components ?? []).map((c) => c.effects?.html ?? '').join('\n');
  return { html: htmlOut };
}

/**
 * 돌아온 HTML 에서 댓글만 집는다.
 *
 * 본문은 `<div class="user-markdown …">` 안에 있다(여는 div 부터 깊이를 세어 닫는 짝을 찾는다 —
 * 본문에 `<p>`·`<a>`·`<blockquote>` 가 들어 있어 첫 `</div>` 로 자르면 잘린다).
 * 작성자는 그 앞의 마지막 `/user/<id>/<이름>` 링크, 날짜는 그 앞의 마지막 날짜꼴,
 * 버전은 `comment-version-tag` 다음의 `vX.Y.Z` 다.
 */
function parseComments(html) {
  const h = html;
  const strip = (s) => un(s.replace(/<(script|style)[\s\S]*?<\/\1>/g, ' ')
    .replace(/<\/(p|div|li|br)>/g, ' ').replace(/<[^>]+>/g, ' ')).replace(/\s+/g, ' ').trim();

  const totals = [...h.matchAll(/Showing[\s\S]{0,400}?results/g)]
    .flatMap((m) => [...m[0].matchAll(/>\s*(\d+)\s*</g)].map((x) => Number(x[1])));
  const total = totals.length ? Math.max(...totals) : null;

  const DATE = /([A-Z][a-z]{2} \d{1,2}(?:, \d{4})?(?: at)? \d{1,2}:\d{2} ?[AP]M)/g;
  const out = [];
  const open = /<div class="user-markdown[^"]*">/g;
  let m;
  while ((m = open.exec(h)) !== null) {
    // 깊이 세기로 닫는 div 찾기
    let i = m.index + m[0].length, depth = 1;
    const tag = /<(\/?)div\b[^>]*>/g;
    tag.lastIndex = i;
    let t, end = -1;
    while ((t = tag.exec(h)) !== null) {
      depth += t[1] ? -1 : 1;
      if (depth === 0) { end = t.index; break; }
    }
    if (end < 0) continue;
    const body = strip(h.slice(i, end));
    if (!body) continue;

    const before = h.slice(Math.max(0, m.index - 4000), m.index);
    const users = [...before.matchAll(/\/user\/\d+\/([A-Za-z0-9_.-]+)"/g)];
    const dates = [...before.matchAll(DATE)];
    const vers = [...before.matchAll(/comment-version-tag[\s\S]{0,400}?>\s*(v[\d.]+)\s*</g)];
    out.push({
      author: users.length ? users[users.length - 1][1] : '',
      date: dates.length ? dates[dates.length - 1][1] : '',
      version: vers.length ? vers[vers.length - 1][1] : '',
      body,
    });
  }
  return { total, comments: out };
}

async function fetchMod(id, pages) {
  const got = [];
  let total = null, error = null;
  for (let page = 1; page <= pages; page++) {
    const r = await fetchPage(id, page);
    if (r.error) { error = r.error; break; }
    const { total: t, comments } = parseComments(r.html);
    if (t !== null) total = t;
    const fresh = comments.filter((c) => !got.some((g) => g.body === c.body && g.author === c.author));
    got.push(...fresh);
    if (page === 1 && (total === null || total <= 10)) break;   // 한 페이지에 10개다
    if (fresh.length === 0) break;                              // 같은 페이지가 또 왔다 — 멈춘다
    await wait(400);
  }
  return { id, total, comments: got, error };
}

const ids = flag('--installed') ? installedIds() : (argv[0] ?? '').split(',').filter(Boolean);
if (ids.length === 0) { console.error('모드 id 를 주거나 --installed 를 쓰세요.'); process.exit(2); }

const rows = [];
let failed = 0, hits = 0;
for (const [n, id] of ids.entries()) {
  const r = await fetchMod(id, PAGES);
  rows.push(r);
  if (r.error) { failed++; console.log(`[${n + 1}/${ids.length}] ${id} ERROR ${r.error}`); await wait(400); continue; }
  const perf = r.comments.filter((c) => PERF_RE.test(c.body));
  hits += perf.length;
  console.log(`[${n + 1}/${ids.length}] ${id} 댓글 ${r.comments.length}${r.total !== null ? `/${r.total}` : ''} 성능언급 ${perf.length}`);
  for (const c of (ALL ? r.comments : perf))
    console.log(`    - ${c.author} (${c.date}${c.version ? `, ${c.version}` : ''}): ${c.body.replace(/\s+/g, ' ').slice(0, 400)}`);
  await wait(400);
}

console.log(`\n모드 ${ids.length}개 / 성능 언급 ${hits}건 / 실패 ${failed}개`);
if (JSON_AT) fs.writeFileSync(JSON_AT, JSON.stringify({ fetchedAt: new Date().toISOString(), pages: PAGES, mods: rows }, null, 2));
if (failed) process.exit(1);
