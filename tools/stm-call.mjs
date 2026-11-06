// 서버의 STM 라우트를 직접 부른다 (게임 없이 서버 쪽 판정만 돌려 보는 용도).
//
// 사용:
//   node tools/stm-call.mjs <라우트> <JSON 본문> [세션 프로필 id]
//   예: node tools/stm-call.mjs /stm/members/roster "{}" 6adc26693f5b5290609ea366
//
// SPT 4.x 의 SptHttpListener 는 요청 본문을 **zlib(헤더 있는 deflate)** 으로 무조건 푼다.
// 평문·raw deflate·gzip 본문을 보내면 `The archive entry was compressed using an unsupported
// compression method` 로 요청이 Critical 로 끝난다(응답은 200 + 빈 본문). 그래서 보내는 쪽도
// zlib 로 압축한다(실측: 평문·raw deflate·gzip 셋 다 이 오류로 떨어지고 zlib 만 통과한다). 응답은 zlib 또는 평문일 수 있어 둘 다 시도한다.

import { readFileSync } from 'node:fs';
import { deflateSync, inflateSync, inflateRawSync, gunzipSync } from 'node:zlib';

const route = process.argv[2];
// 본문은 인자로 주거나, 큰 것은 `@경로` 로 파일에서 읽는다(명령줄 길이 제한 때문이다 - 인벤토리
// 전체는 90KB 가 넘어 인자로는 들어가지 않는다).
const bodyArg = process.argv[3] ?? '{}';
const body = bodyArg.startsWith('@') ? readFileSync(bodyArg.slice(1), 'utf8') : bodyArg;
// Git Bash 가 `/stm/...` 를 윈도 경로로 바꿔 버리므로 앞 슬래시 없이도 받는다.
const path = route?.startsWith("/") ? route : `/${route}`;
const session = process.argv[4] ?? '6adc26693f5b5290609ea366';
if (!route) {
    console.error('사용: node tools/stm-call.mjs <라우트> <JSON 본문> [세션 프로필 id]');
    process.exit(2);
}

// 자체 서명 인증서라 검증을 끈다(대상이 127.0.0.1 고정이다).
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const res = await fetch(`https://127.0.0.1:6969${path}`, {
    method: 'POST',
    headers: { Cookie: `PHPSESSID=${session}`, 'Content-Type': 'application/json' },
    body: deflateSync(Buffer.from(body, 'utf8')),
});

const raw = Buffer.from(await res.arrayBuffer());
let text = null;
for (const f of [inflateSync, inflateRawSync, gunzipSync]) {
    try {
        text = f(raw).toString('utf8');
        break;
    } catch {
        // 다음 방식으로 넘어간다.
    }
}
if (text === null) text = raw.toString('utf8');

console.error(`HTTP ${res.status}, 응답 ${raw.length} 바이트`);
console.log(text);
