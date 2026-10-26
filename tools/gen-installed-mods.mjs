// doc/install-records 와 doc/download/manifest-*.json 을 합쳐 doc/INSTALLED-MODS.md 를 만든다.
// 프로필 결속 여부는 Forge API 에서 한 번에 조회한다. 사용: node tools/gen-installed-mods.mjs
import fs from 'node:fs';
import path from 'node:path';

const ROOT = path.resolve(path.dirname(new URL(import.meta.url).pathname.replace(/^\/([A-Z]:)/, '$1')), '..');
const DOC = path.join(ROOT, 'doc');

// 1) 매니페스트: 모드 id → 이름·버전·단계
const mods = new Map();
for (const f of fs.readdirSync(path.join(DOC, 'download')).filter(f => /^manifest-stage\d(?:-[A-Za-z])?\.json$/.test(f)).sort()) {
  const stage = f.match(/stage(\d)/)[1];
  for (const m of JSON.parse(fs.readFileSync(path.join(DOC, 'download', f), 'utf8')).mods)
    mods.set(String(m.id), { id: m.id, link: m.homepage ?? `https://sp-mod.com/mod/${m.id}`, name: m.name, version: m.version, stage, group: m.group ?? '', files: [], records: new Set() });
}

// 2) 설치 기록: "<id>__<폴더>\t<E:\Spt 기준 경로>"
for (const f of fs.readdirSync(path.join(DOC, 'install-records')).filter(f => f.endsWith('.txt')).sort()) {
  const label = f.replace(/^installed-/, '').replace(/\.txt$/, '');
  for (const line of fs.readFileSync(path.join(DOC, 'install-records', f), 'utf8').replace(/^\uFEFF/, '').split(/\r?\n/)) {
    if (!line.trim()) continue;
    const [folder, rel] = line.split('\t');
    const id = folder.split('__')[0];
    const m = mods.get(id);
    if (!m) { console.error(`기록에 있으나 매니페스트에 없는 모드: ${folder}`); continue; }
    m.files.push(rel); m.records.add(label);
  }
}

// 3) 프로필 결속 여부 (Forge API)
const ids = [...mods.keys()].filter(id => /^[0-9]+$/.test(id));  // Forge 밖(GitHub 등) 모드는 API 조회 대상이 아님
const binding = new Map();
for (let i = 0; i < ids.length; i += 50) {
  const r = await fetch(`https://sp-mod.com/api/v0/mods?filter[id]=${ids.slice(i, i + 50).join(',')}&per_page=50&fields=shows_profile_binding_notice,cheat_notice`);
  for (const m of (await r.json()).data) binding.set(String(m.id), { bind: m.shows_profile_binding_notice, cheat: m.cheat_notice });
}

const side = (files) => {
  const c = files.some(f => /^BepInEx\\/i.test(f)), s = files.some(f => /^SPT_Runtime\\/i.test(f));
  const d = files.some(f => /^EscapeFromTarkov_Data\\/i.test(f)), o = files.some(f => !/^(BepInEx|SPT_Runtime|EscapeFromTarkov_Data)\\/i.test(f));
  return [c && '게임', d && '게임 데이터', s && '서버', o && '루트'].filter(Boolean).join('+');
};
const stageTitle = { 1: '1차 — UI·탄약 정보·개조 화면 편의', 2: '2차 — 편의·WTT 콘텐츠·SVM', 3: '3차 — 도구·커스텀 상인', 4: '4차 — 상인 목록 넘침 대응', 5: '5차 — UI·편의(5-A)·총기(5-B)·탄약(5-C)·상인(5-D)·치트·도구(5-E)', 6: '6차 — 상인·파츠 (6-A 10 + 6-B 결속 2 + 6-C 부착물 UI 3 + 6-D 초상화 1)', 7: '7차 — 방어구·탄환·파츠·상인 (7-A 13 + 7-B 3 + 7-C 26 + 7-D 12 + 7-E 대형 3)' };
// 로드 확인 현황: 확인할 때마다 여기만 고친다
const loadStatus = '- 게임 쪽 플러그인 로드: 116개 (2026-10-26 `LogOutput.log`, 오류는 Fika 미설치 관련 1건뿐) / 서버 모드 로드: 97개 (2026-10-26 AllinWeaponUnslotted 복원 + WebConfigBridge 설치 후 재시작, 오류 0 — `server-tests/server-webconfigbridge.log`)';

let md = `# 설치된 모드 목록\n\n> 자동 생성: \`node tools/gen-installed-mods.mjs\` (설치 기록·매니페스트 기반). 직접 고치지 말고 기록을 고친 뒤 다시 생성.\n\n`;
const all = [...mods.values()];
const total = all.filter(m => m.files.length);
md += `- 대상: \`E:\\Spt\` (SPT 4.1.6, EFT 0.16.9.5.40743)\n`;
md += `- 설치된 모드: **${total.length}개**, 설치 파일 **${total.reduce((a, m) => a + m.files.length, 0)}개** (자동 생성 설정 파일 포함)\n`;
md += `${loadStatus}\n`;
md += `- **결속** = Forge "프로필 결속" 표시. 플레이 후 제거하면 프로필이 깨질 수 있음\n\n`;

for (const st of ['1', '2', '3', '4', '5', '6', '7']) {
  const list = all.filter(m => m.stage === st);
  if (!list.length) continue;
  md += `## ${stageTitle[st]}\n\n| 모드 | 버전 | 위치 | 결속 | 파일 | 설치 기록 |\n| --- | --- | --- | --- | --- | --- |\n`;
  for (const m of list) {
    const b = binding.get(String(m.id)) ?? {};
    md += `| [${m.name.replace(/[[\]|]/g, c => '\\' + c)}](${m.link}) | ${m.version} | ${side(m.files) || '-'} | ${b.bind ? '**결속**' : '-'}${b.cheat ? ' (치트 표시)' : ''} | ${m.files.length} | ${[...m.records].map(r => '`' + r + '`').join(', ') || '**미설치**'} |\n`;
  }
  md += '\n';
}

md += `## 다른 경로로 설치된 것\n\n| 항목 | 위치 | 설치 주체 |\n| --- | --- | --- |\n`;
// 사전 개수는 src/SptKoreanConfig/lang 을 직접 센다. 손으로 고치면 늘 어긋난다.
const langCount = fs.readdirSync(path.join(ROOT, 'src', 'SptKoreanConfig', 'lang')).filter(f => f.endsWith('.json')).length;
md += `| SptKoreanConfig 0.1.0 (F12 설정 창 한글화) | \`BepInEx\\plugins\\SptKoreanConfig\\\` (DLL 1 + 번역 사전 ${langCount}) | 이 저장소에서 직접 만든 것. 소스는 [../src/SptKoreanConfig](../src/SptKoreanConfig), 설치는 \`tools/install-koreanconfig.ps1\`. Forge 모드가 아니라 설치 기록에 없다 |\n`;
md += `| HelmetSlotUnion 0.1.0 (헬멧 부착 칸 합집합) | \`SPT_Runtime\\user\\mods\\HelmetSlotUnion\\\` (DLL 1 + \`config.jsonc\`) | 이 저장소에서 직접 만든 것. 소스는 [../src/HelmetSlotUnion](../src/HelmetSlotUnion), 설치는 \`tools/install-helmetslotunion.ps1\`. Forge 모드가 아니라 설치 기록에 없다 |\n`;
md += `| WebConfigBridge 0.1.0 (웹 설정 편집기 연결) | \`SPT_Runtime\\user\\mods\\WebConfigBridge\\\` (DLL 1) | 이 저장소에서 직접 만든 것. 소스는 [../src/WebConfigBridge](../src/WebConfigBridge), 설치는 \`tools/install-webconfigbridge.ps1\`. 자체 편집 수단이 없는 다른 서버 모드의 설정 파일을 서버 웹 페이지(\`/configs\`)의 설정 편집기에 등록한다. 현재 대상은 \`All In Weapon: Unslotted\`. Forge 모드가 아니라 설치 기록에 없다 |\n`;
md += `| CompatToggle 0.1.0 (호환성 토글) | **설치 안 함** | 이 저장소에서 직접 만든 것. 소스는 [../src/CompatToggle](../src/CompatToggle). \`All In Weapon: Unslotted\`를 F12 토글로 대체하려 했으나 개조 화면의 부품 목록을 채우지 못해 철회했다. 경위는 [WORKLOG](../WORKLOG.md) 참고 |\n`;
md += `| HideSpecialIconGrids 1.1.0.1 | \`BepInEx\\plugins\\HideSpecialIconGrids.dll\`, \`BepInEx\\config\\com.pandahhcorp.hidespecialicongrids.cfg\` | SVM \`Greed.exe\`가 프리셋 \`CSM.EnableCSM: true\` 적용 시 설치 (15:01). 설치 기록에 없음 |\n`;
md += `| SVM 프리셋 \`Noname\` | \`SPT_Runtime\\user\\mods\\[SVM] Server Value Modifier\\Presets\\Noname.json\` | 사용자가 \`Greed.exe\`로 생성. 16:00에 \`Hideout.RemoveConstructionsRequirements\`만 false로 수정 |\n`;

fs.writeFileSync(path.join(DOC, 'INSTALLED-MODS.md'), md);
console.log(`INSTALLED-MODS.md: 모드 ${total.length}개, 파일 ${total.reduce((a, m) => a + m.files.length, 0)}개`);
for (const m of all) console.log(m.stage, m.id, m.name, m.files.length, side(m.files), binding.get(m.id)?.bind ? 'BIND' : '-');
