// 미번역 문자열의 종류를 가른다. classify-web-ui-missing.mjs 와 analyze-web-ui-audit.mjs 가 같이 쓴다.
//
// 남은 수만 보면 판단을 그르친다. 사람 이름과 설정 키는 바꾸면 안 되는 것이라 영원히 줄지 않는다.
// "아직 몇 개"가 아니라 "번역할 것은 몇 개"를 봐야 한다.

export const TRANSLATABLE = '번역 후보';

// 위에서부터 먼저 맞는 갈래로 들어간다.
export const BUCKETS = [
  ['식별자', [
    /^[0-9a-f]{24}$/,
    /^(item|patron|stock|tarkov|monster|weapon|mag)_[a-z0-9_]+/i,
    /^BotZone\w*$/,
    /^[A-Z][a-z]+(_[A-Z][a-z]+)+$/,
    /\.(json|jsonc|dll|js|css)$/i,
    /^\/[a-z0-9/.-]+$/i,
    /^[a-z]+[A-Z]\w*$/, // camelCase 설정 키
  ]],
  ['링크·버전', [/^https?:/, /^v?\d+\.\d+(\.\d+)?$/, /^(GitHub|sp-mod|VirusTotal|HTTP|en|spt|SPT)$/]],
  ['치수·수치', [/^\d+\s*[xX]\s*\d+$/, /^[-+0-9.,xX×%\s|:/]+$/]],
  ['아이템 이름', [
    /[○●]\s*$/,
    /\b(backpack|Pack|bag|Satchel|figurine|textbook|magazine|barrel|box|wrench|drill|torch|tablets|board|stick|components|converter|materials|notes|Medal|kit|handwheel|survey maps|flash drive|sledgehammer|riser|scope ring|imager|sight)\b/i,
    /^!!!DO/,
    /^(WD-40|Can of|Car battery|CPU fan|PC CPU|RAM stick|Soap|Awl|Mace|Carry Case|Expeditionary Case|Thumb drive)/,
    /^\[[\w-]+\]_\(/, // [모드]_(아이템) 꼴
    /^Dogtag (BEAR|USEC)\b/, // 공식 kr 도 "Dogtag BEAR" 영문
    /^Black Division Dogtag\b/,
  ]],
  ['사람·모드 이름', [
    /^[A-Za-z][A-Za-z0-9_.-]*$/,
    /\(White Tusk\)$/,
  ]],
];

export function classify(s) {
  const hit = BUCKETS.find(([, pats]) => pats.some((r) => r.test(s)));
  return hit ? hit[0] : TRANSLATABLE;
}
