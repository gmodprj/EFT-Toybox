// 서버 웹 화면(SIC)의 남은 영문을 화면에서 한국어로 바꾼다.
//
// 왜 이렇게 하는가:
//   모드 페이지의 문구는 각 모드 DLL 안의 Razor 컴포넌트에 리터럴로 박혀 있어
//   로케일 파일로 닿지 않는다(doc/REVIEW-MOD-PAGE-KOREAN.md). 본체의
//   /profiles·/database·/login 도 업스트림이 아직 로케일로 빼지 않았다.
//   DLL 을 모드마다 패치하면 업데이트마다 깨진다. 그래서 모든 페이지가
//   공통으로 불러오는 자리에 번역기를 하나 끼운다.
//
// Blazor Server 는 웹소켓으로 DOM 을 계속 갈아 끼우므로 MutationObserver 로 따라간다.
// 사전에 없는 문자열은 window.__ko.missing 에 모아 둔다 — 수집해서 사전에 채우는 데 쓴다.
(function () {
  'use strict';

  var DICT_URL = '/modcardkorean/ko-ui.json';

  var dict = Object.create(null);
  var rules = []; // [정규식, 바꿀 꼴] — 숫자가 끼어 열쇠로 못 쓰는 문구용
  var missing = Object.create(null);
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

  // 건드리면 안 되는 곳. 사용자가 입력 중인 값과 코드/스타일은 손대지 않는다.
  var SKIP_TAGS = { SCRIPT: 1, STYLE: 1, TEXTAREA: 1, CODE: 1, PRE: 1, NOSCRIPT: 1 };

  // 번역 대상 속성. 화면에 글자로 보이는 것만.
  var ATTRS = ['placeholder', 'title', 'aria-label', 'alt'];

  function isSkipped(node) {
    for (var el = node.parentElement; el; el = el.parentElement) {
      if (SKIP_TAGS[el.tagName]) return true;
      if (el.isContentEditable) return true;
    }
    return false;
  }

  // 앞뒤 공백은 살려 두고 가운데 알맹이만 바꾼다. 레이아웃이 흔들리지 않는다.
  function translate(raw) {
    var m = /^(\s*)([\s\S]*?)(\s*)$/.exec(raw);
    var lead = m[1], core = m[2], tail = m[3];
    if (!core) return null;

    var out = lookup(core);
    if (out !== null) return lead + out + tail;

    if (/[가-힣]/.test(core)) return null; // 이미 한국어다
    if (NEVER.some(function (r) { return r.test(core); })) return null;

    if (!missing[core]) missing[core] = 0;
    missing[core]++;
    return null;
  }

  function lookup(core) {
    var hit = dict[core];
    if (hit !== undefined) return hit;

    for (var i = 0; i < rules.length; i++) {
      var m = rules[i][0].exec(core);
      if (m) {
        return rules[i][1].replace(/\$(\d)/g, function (_, d) {
          return m[+d] !== undefined ? m[+d] : '';
        });
      }
    }

    // "#FireKlean gun lube ○" 처럼 꼬리표가 붙은 것은 알맹이만 바꿔 본다.
    var t = TAIL.exec(core);
    if (t && t[1]) {
      var inner = lookup(t[1].replace(/\s+$/, ''));
      if (inner !== null) return inner + t[2];
    }

    return null;
  }

  function applyToText(node) {
    var out = translate(node.nodeValue);
    if (out !== null && out !== node.nodeValue) node.nodeValue = out;
  }

  function applyToElement(el) {
    for (var i = 0; i < ATTRS.length; i++) {
      var name = ATTRS[i];
      if (!el.hasAttribute(name)) continue;
      var out = translate(el.getAttribute(name));
      if (out !== null) el.setAttribute(name, out);
    }
  }

  function walk(root) {
    if (!ready || !root) return;

    if (root.nodeType === 3) {
      if (!isSkipped(root)) applyToText(root);
      return;
    }
    if (root.nodeType !== 1) return;
    if (SKIP_TAGS[root.tagName]) return;

    applyToElement(root);

    var it = document.createTreeWalker(root, NodeFilter.SHOW_TEXT | NodeFilter.SHOW_ELEMENT, null);
    var n;
    while ((n = it.nextNode())) {
      if (n.nodeType === 1) {
        if (SKIP_TAGS[n.tagName]) continue;
        applyToElement(n);
      } else if (!isSkipped(n)) {
        applyToText(n);
      }
    }
  }

  var queued = false;
  var pending = [];

  function flush() {
    queued = false;
    var batch = pending;
    pending = [];
    for (var i = 0; i < batch.length; i++) walk(batch[i]);
  }

  function schedule(node) {
    pending.push(node);
    if (queued) return;
    queued = true;
    // 렌더가 몰아칠 때 매번 훑지 않는다.
    (window.requestAnimationFrame || window.setTimeout)(flush, 0);
  }

  var observer = new MutationObserver(function (records) {
    for (var i = 0; i < records.length; i++) {
      var r = records[i];
      if (r.type === 'characterData') {
        schedule(r.target);
      } else if (r.type === 'attributes') {
        schedule(r.target);
      } else {
        for (var j = 0; j < r.addedNodes.length; j++) schedule(r.addedNodes[j]);
      }
    }
  });

  function start() {
    ready = true;
    walk(document.body);
    observer.observe(document.body, {
      childList: true,
      subtree: true,
      characterData: true,
      attributes: true,
      attributeFilter: ATTRS,
    });
  }

  // 수집한 미번역 문자열을 꺼내 보는 창구. 화면을 돌며 모은 뒤 사전에 채운다.
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
    rescan: function () {
      walk(document.body);
    },
  };

  fetch(DICT_URL, { cache: 'no-store' })
    .then(function (r) {
      return r.json();
    })
    .then(function (j) {
      for (var k in j) {
        if (k === '_rules') {
          for (var i = 0; i < j[k].length; i++) {
            rules.push([new RegExp(j[k][i][0]), j[k][i][1]]);
          }
          continue;
        }
        if (k.charAt(0) === '_') continue;
        dict[k] = j[k];
      }
      // 아이템 이름은 따로 받는다. 없거나 실패해도 UI 번역은 돈다.
      //
      // 먼저 ModCardKorean 의 라우트에 묻는다 — 서버 메모리의 로케일에서 그 자리에서 뽑으므로
      // 모드가 만든 아이템까지 들어 있고, SptKoreanLocale 의 번역도 반영돼 있다.
      // 그 모드가 없으면 디스크 로케일로 만들어 둔 파일로 물러선다(모드 아이템은 빠진다).
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
