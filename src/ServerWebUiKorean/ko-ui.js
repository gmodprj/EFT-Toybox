// 서버 웹 화면(SIC)의 남은 영문을 화면에서 한국어로 바꾼다.
//
// 왜 이렇게 하는가:
//   모드 페이지의 문구는 각 모드 DLL 안의 Razor 컴포넌트에 리터럴로 박혀 있어
//   로케일 파일로 닿지 않는다(doc/REVIEW-MOD-PAGE-KOREAN.md). 본체의
//   /profiles·/database·/login 도 업스트림이 아직 로케일로 빼지 않았다.
//   DLL 을 모드마다 패치하면 업데이트마다 깨진다. 그래서 모든 HTML 응답에
//   이 스크립트를 끼워(ModCardKorean 의 미들웨어) 화면에서 바꾼다.
//
// Blazor Server 는 웹소켓으로 DOM 을 계속 갈아 끼우므로 MutationObserver 로 따라간다.
//
// 쪼개진 텍스트 노드:
//   Razor 는 `Add @label` 을 텍스트 노드 둘("Add", " item")로 그린다. 낱개로 바꾸면
//   "추가"+"품목" = "추가 품목"처럼 어순이 뒤집혀 뜻이 달라진다. 그래서 한 요소 안의
//   **이웃 텍스트 노드를 이어 붙여 통째로** 찾는다. 통째로 못 찾았는데 글자 조각이
//   둘 이상이면 **부분 번역하지 않고 영문으로 둔다** — 뜻이 뒤집힌 한국어보다 낫다.
//   대신 이어 붙인 문장을 미번역으로 모아 사전에 채운다.
//
// 원문 기억:
//   우리가 바꾼 노드는 원문을 기억해 둔다(WeakMap). Blazor 가 숫자 하나만 바꿔도
//   이웃 노드의 원문을 되살려 다시 이어 붙일 수 있어야 한다.
(function () {
  'use strict';

  var DICT_URL = '/modcardkorean/ko-ui.json';
  var KEEP = {}; // 페이지별 예외에서 "원문 그대로 둔다"를 뜻하는 표식

  var dict = Object.create(null);
  var rules = []; // [정규식, 바꿀 꼴] — 숫자가 끼어 열쇠로 못 쓰는 문구용
  var scoped = []; // [경로 접두, {영문: 한글|null}] — 같은 낱말이 페이지마다 뜻이 다를 때
  var missing = Object.create(null);
  var applied = Object.create(null); // 영문 → 실제로 넣은 한글 (점검용)
  var state = new WeakMap(); // 텍스트 노드 → {orig, wrote}
  var ready = false;

  // 번역 대상이 아닌 것. 사람 이름·URL·ID·파일명은 바꾸면 안 된다.
  var NEVER = [
    /^[0-9a-f]{24}$/, // MongoID
    /^https?:\/\//, // 링크
    /^@/, // 작성자 핸들
    /\.(json|jsonc|dll|js|css|zip|png)$/i, // 파일명
    /^[^A-Za-z]*$/, // 글자가 아예 없는 것
  ];

  // 꼬리에 붙는 표시(BIS 의 ○/● 처럼). 알맹이만 바꾸고 표시는 그대로 둔다.
  var TAIL = /^([\s\S]*?)(\s*[○●◆◇★☆→]+)$/;

  // Unity 리치 텍스트. 다른 모드가 아이템 이름에 게임용 색 태그를 넣어 두는데,
  // 웹에서는 텍스트 노드라 "<b><color=#FFFFFF>…</color></b>" 가 글자 그대로 찍힌다.
  // 떼고 안쪽을 찾는다 — 사전에 없어도 태그 찌꺼기는 지운다.
  var RICH = /<\/?(?:b|i|u|color|size|alpha|mark)(?:=[^>]*)?>/gi;

  // 건드리면 안 되는 곳. 사용자가 입력 중인 값과 코드/스타일은 손대지 않는다.
  var SKIP_TAGS = { SCRIPT: 1, STYLE: 1, TEXTAREA: 1, CODE: 1, PRE: 1, NOSCRIPT: 1 };

  // 번역 대상 속성. 화면에 글자로 보이는 것만.
  var ATTRS = ['placeholder', 'title', 'aria-label', 'alt'];

  // SVG 안의 요소는 tagName 이 소문자다(<style> → "style"). 대문자로만 비교하면
  // SVG 의 <style> 이 빠져나가 CSS 가 번역 대상으로 잡힌다(ORBIT 지도에서 실측).
  function isSkipTag(el) {
    return !!SKIP_TAGS[String(el.tagName).toUpperCase()];
  }

  function skippedElement(el) {
    for (; el; el = el.parentElement) {
      if (isSkipTag(el)) return true;
      if (el.isContentEditable) return true;
    }
    return false;
  }

  function scopedHit(core) {
    var path = location.pathname;
    for (var i = 0; i < scoped.length; i++) {
      if (path.indexOf(scoped[i][0]) !== 0) continue;
      var map = scoped[i][1];
      if (Object.prototype.hasOwnProperty.call(map, core)) return map[core] === null ? KEEP : map[core];
    }
    return undefined;
  }

  function lookup(core, depth) {
    depth = depth || 0;
    if (depth > 3) return null;

    var sc = scopedHit(core);
    if (sc !== undefined) return sc;

    var hit = dict[core];
    if (hit !== undefined) return hit;

    for (var i = 0; i < rules.length; i++) {
      var m = rules[i][0].exec(core);
      if (!m) continue;
      var res = rules[i][1].replace(/\$(\d)/g, function (_, d) {
        var cap = m[+d];
        if (cap === undefined) return '';
        // 캡처도 사전에 있으면 바꾼다("Formation 3 · Scout" → "편성 3 · 정찰병").
        var inner = /[A-Za-z]/.test(cap) ? lookup(cap.trim(), depth + 1) : null;
        return typeof inner === 'string' ? inner : cap;
      });
      // 일반 규칙("$1 ×$2")이 맞았는데 캡처를 못 바꿔 원문 그대로면, 뒤의 구체적 규칙
      // ("Day N, Multiplier: ×M")에 기회를 준다. 여기서 멈추면 그 규칙이 영영 안 돈다.
      if (res !== core) return res;
    }

    // "#FireKlean gun lube ○" 처럼 꼬리표가 붙은 것은 알맹이만 바꿔 본다.
    var t = TAIL.exec(core);
    if (t && t[1]) {
      var inner2 = lookup(t[1].replace(/\s+$/, ''), depth + 1);
      if (typeof inner2 === 'string') return inner2 + t[2];
    }

    return null;
  }

  // 앞뒤 공백은 살려 두고 가운데 알맹이만 바꾼다. 레이아웃이 흔들리지 않는다.
  // 돌려주는 값: 바꾼 문자열, 또는 null(못 찾음 · 바꿀 것 아님).
  function translate(raw, record) {
    var m = /^(\s*)([\s\S]*?)(\s*)$/.exec(raw);
    var lead = m[1], core = m[2], tail = m[3];
    if (!core) return null;

    // 태그 찌꺼기를 떼고 본다. 사전에 없으면 태그만 뗀 원문을 돌려준다(글자 그대로 찍히는 것보다 낫다).
    if (RICH.test(core)) {
      RICH.lastIndex = 0;
      var bare = core.replace(RICH, '').replace(/\s+/g, ' ').trim();
      RICH.lastIndex = 0;
      var inner = translate(bare, record);
      return lead + (inner !== null ? inner : bare) + tail;
    }

    var out = lookup(core, 0);
    if (out === KEEP) return null;
    // 규칙이 캡처를 하나도 못 바꿔 원문 그대로 돌려줬다면 번역된 게 아니다.
    // "번역됨"으로 치면 미번역 목록에서 빠져 구멍이 가려진다.
    if (out === core) out = null;
    if (out !== null) {
      if (applied[core] === undefined) applied[core] = out;
      return lead + out + tail;
    }

    if (record === false) return null;
    if (/[가-힣]/.test(core)) return null; // 이미 한국어다
    if (NEVER.some(function (r) { return r.test(core); })) return null;

    missing[core] = (missing[core] || 0) + 1;
    return null;
  }

  // 우리가 쓴 값이 그대로면 기억해 둔 원문을, Blazor 가 새로 썼으면 지금 값을 원문으로 본다.
  function source(node) {
    var s = state.get(node);
    return s && node.nodeValue === s.wrote ? s.orig : node.nodeValue;
  }

  function put(node, orig, value) {
    if (node.nodeValue !== value) node.nodeValue = value;
    state.set(node, { orig: orig, wrote: value });
  }

  var LETTER = /[A-Za-z]/;

  // 번역은 없지만 그대로 두는 게 맞는 조각인가 — 사전의 null(원문 유지), 이미 한국어, 고유명사 규칙.
  function settled(raw) {
    var core = raw.replace(RICH, '').replace(/\s+/g, ' ').trim();
    RICH.lastIndex = 0;
    if (!core) return true;
    if (/[가-힣]/.test(core)) return true;
    if (lookup(core, 0) === KEEP) return true;
    return NEVER.some(function (r) { return r.test(core); });
  }

  // 한 요소 안의 이어진 텍스트 노드 묶음을 처리한다.
  function translateRun(run) {
    if (!run.length) return;

    var srcs = run.map(source);

    if (run.length === 1) {
      var one = translate(srcs[0]);
      put(run[0], srcs[0], one !== null ? one : srcs[0]);
      return;
    }

    var lettered = 0;
    for (var i = 0; i < srcs.length; i++) if (LETTER.test(srcs[i])) lettered++;
    if (lettered === 0) return;

    var whole = translate(srcs.join(''));
    if (whole !== null) {
      put(run[0], srcs[0], whole);
      for (var j = 1; j < run.length; j++) put(run[j], srcs[j], '');
      return;
    }

    if (lettered === 1) {
      // 숫자 + 문구 하나 꼴이면 문구만 바꿔도 어순이 크게 틀어지지 않는다.
      for (var k = 0; k < run.length; k++) {
        var part = LETTER.test(srcs[k]) ? translate(srcs[k], false) : null;
        put(run[k], srcs[k], part !== null ? part : srcs[k]);
      }
      return;
    }

    // 글자 조각이 둘 이상인데 통째로 못 찾았다. 조각이 **전부** 사전에 있으면 낱개로 바꾼다
    // (QuestCodex 의 "아이템, 아이템, …" 목록은 조각마다 노드가 갈려 통째로는 영영 안 맞는다).
    // 하나라도 빠지면 반쯤 한국어인 어순 뒤틀린 문장이 되니 원문으로 둔다.
    var parts = [];
    for (var p = 0; p < run.length; p++) {
      if (!LETTER.test(srcs[p])) { parts.push(srcs[p]); continue; }
      var got = translate(srcs[p], false);
      if (got === null && settled(srcs[p])) got = srcs[p]; // 원문 유지(null)·이미 한국어는 바뀐 셈이다
      if (got === null) { parts = null; break; }
      parts.push(got);
    }
    for (var n = 0; n < run.length; n++) put(run[n], srcs[n], parts ? parts[n] : srcs[n]);
  }

  function processElement(el) {
    for (var i = 0; i < ATTRS.length; i++) {
      var name = ATTRS[i];
      if (!el.hasAttribute(name)) continue;
      var out = translate(el.getAttribute(name));
      if (out !== null) el.setAttribute(name, out);
    }

    var run = [];
    for (var c = el.firstChild; c; c = c.nextSibling) {
      if (c.nodeType === 3) {
        run.push(c);
      } else {
        translateRun(run);
        run = [];
      }
    }
    translateRun(run);
  }

  function walkDeep(root) {
    if (!ready || !root || root.nodeType !== 1) return;
    if (skippedElement(root)) return;

    processElement(root);
    var it = document.createTreeWalker(root, NodeFilter.SHOW_ELEMENT, {
      acceptNode: function (n) {
        return isSkipTag(n) || n.isContentEditable ? NodeFilter.FILTER_REJECT : NodeFilter.FILTER_ACCEPT;
      },
    });
    var n;
    while ((n = it.nextNode())) processElement(n);
  }

  function walkShallow(el) {
    if (!ready || !el || el.nodeType !== 1) return;
    if (skippedElement(el)) return;
    processElement(el);
  }

  var deep = new Set();
  var shallow = new Set();
  var queued = false;

  function flush() {
    queued = false;
    var d = Array.from(deep);
    var s = Array.from(shallow);
    deep.clear();
    shallow.clear();
    for (var i = 0; i < d.length; i++) if (d[i].isConnected) walkDeep(d[i]);
    for (var j = 0; j < s.length; j++) if (s[j].isConnected) walkShallow(s[j]);
  }

  function kick() {
    if (queued) return;
    queued = true;
    // 렌더가 몰아칠 때 매번 훑지 않도록 잠깐 모았다가 한 번에 본다.
    //
    // requestAnimationFrame 을 쓰면 안 된다. 그려지지 않는 창(백그라운드 탭, 가려진 창)에서는
    // rAF 가 불리지 않아 queued 가 true 로 굳고, 그 뒤 변경을 전부 흘려버린다.
    // Claude 앱의 브라우저 창에서 visibilityState 가 "visible" 인데도 1.5초 안에 한 번도
    // 불리지 않는 것을 실측했다. setTimeout 은 느려질 수는 있어도 멈추지는 않는다.
    setTimeout(flush, 30);
  }

  var observer = new MutationObserver(function (records) {
    for (var i = 0; i < records.length; i++) {
      var r = records[i];
      if (r.type === 'characterData') {
        if (r.target.parentElement) shallow.add(r.target.parentElement);
      } else if (r.type === 'attributes') {
        shallow.add(r.target);
      } else {
        // 텍스트 노드가 들거나 나면 이웃 묶음이 바뀐다 — 부모를 다시 본다.
        if (r.target.nodeType === 1) shallow.add(r.target);
        for (var j = 0; j < r.addedNodes.length; j++) {
          if (r.addedNodes[j].nodeType === 1) deep.add(r.addedNodes[j]);
        }
      }
    }
    kick();
  });

  function start() {
    ready = true;
    walkDeep(document.body);
    observer.observe(document.body, {
      childList: true,
      subtree: true,
      characterData: true,
      attributes: true,
      attributeFilter: ATTRS,
    });
  }

  // 점검 창구. 화면을 돌며 미번역과 실제 적용 내역을 모은다(tools/audit-web-ui.mjs).
  window.__ko = {
    get missing() {
      return Object.keys(missing)
        .sort()
        .map(function (k) {
          return { text: k, seen: missing[k] };
        });
    },
    get size() {
      return Object.keys(dict).length;
    },
    get applied() {
      var out = {};
      for (var k in applied) out[k] = applied[k];
      return out;
    },
    rescan: function () {
      walkDeep(document.body);
    },
  };

  fetch(DICT_URL, { cache: 'no-store' })
    .then(function (r) {
      return r.json();
    })
    .then(function (j) {
      for (var k in j) {
        if (k === '_rules') {
          for (var i = 0; i < j[k].length; i++) rules.push([new RegExp(j[k][i][0]), j[k][i][1]]);
          continue;
        }
        if (k === '_scoped') {
          for (var p in j[k]) scoped.push([p, j[k][p]]);
          continue;
        }
        if (k.charAt(0) === '_') continue;
        // null 은 "원문 그대로 둔다". 모드 이름처럼 아이템 사전 경로로 엉뚱하게 번역되는 것을 막는다
        // (런타임 kr 로케일에 "Skills Extended" → "스킬 확장" 이 들어 있었다).
        dict[k] = j[k] === null ? KEEP : j[k];
      }
      // 아이템 이름은 ModCardKorean 의 라우트가 서버 메모리의 로케일에서 그 자리에서 뽑는다.
      // 모드가 만든 아이템까지 들어 있고 SptKoreanLocale 의 번역도 반영돼 있다.
      return fetch('/modcardkorean/item-names.json', { cache: 'no-store' })
        .then(function (r) {
          if (r.ok) return r.json();
          throw new Error('no route');
        })
        .catch(function () {
          return {};
        })
        .then(function (items) {
          for (var k2 in items) if (dict[k2] === undefined) dict[k2] = items[k2];
          if (document.body) start();
          else document.addEventListener('DOMContentLoaded', start);
        });
    })
    .catch(function (e) {
      // 사전을 못 읽으면 아무것도 바꾸지 않는다. 화면은 영어로 남을 뿐 깨지지 않는다.
      console.error('[ko-ui] 사전을 읽지 못했다', e);
    });
})();
