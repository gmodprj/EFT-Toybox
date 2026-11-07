// 설치본이 빌드와 같은지 본다. 다르면 0 이 아닌 코드로 끝난다.
//
// 왜 — 2026-11-07. 서버 DLL 이 잠겨 있어 복사가 실패했는데 "다음 재시작 때 반영된다" 고 넘겼다.
// 재시작해도 파일이 그대로라 반영될 것이 없었고, 그 상태로 한참을 "왜 안 되지" 했다.
// 잠겼을 때는 **이름을 바꿔 치우고 넣어야** 한다(로드된 DLL 도 rename 은 된다).
//
// 사용: node tools/check-stm-installed.mjs [SPT 경로]
import { createHash } from "node:crypto";
import { readFileSync, statSync, existsSync } from "node:fs";

const spt = process.argv[2] ?? "E:/Spt";
const repo = process.argv[3] ?? "E:/Spt_Mods";

const pairs = [
  {
    name: "STM 클라이언트",
    built: `${repo}/src/SptTeamManager/client/bin/Release/net472/SptTeamManager.dll`,
    installed: `${spt}/BepInEx/plugins/SptTeamManager/SptTeamManager.dll`,
  },
  {
    name: "STM 서버",
    built: `${repo}/src/SptTeamManager/server/bin/Release/net10.0/SptTeamManager.Server.dll`,
    installed: `${spt}/SPT_Runtime/user/mods/SptTeamManager-ServerMod/SptTeamManager.Server.dll`,
  },
  {
    name: "SlotCategoryRelax",
    built: `${repo}/src/SlotCategoryRelax/bin/Release/SlotCategoryRelax.dll`,
    installed: `${spt}/SPT_Runtime/user/mods/SlotCategoryRelax/SlotCategoryRelax.dll`,
  },
  {
    name: "HelmetSlotUnion",
    built: `${repo}/src/HelmetSlotUnion/bin/Release/HelmetSlotUnion.dll`,
    installed: `${spt}/SPT_Runtime/user/mods/HelmetSlotUnion/HelmetSlotUnion.dll`,
  },
];

const md5 = (p) => createHash("md5").update(readFileSync(p)).digest("hex");
const when = (p) => statSync(p).mtime.toISOString().slice(0, 19).replace("T", " ");

let bad = 0;
let skipped = 0;
for (const { name, built, installed } of pairs) {
  if (!existsSync(built)) {
    console.log(`건너뜀  ${name.padEnd(18)} 빌드 결과가 없다`);
    skipped++;
    continue;
  }
  if (!existsSync(installed)) {
    console.log(`안 깔림  ${name.padEnd(18)} ${installed}`);
    bad++;
    continue;
  }

  const a = md5(built);
  const b = md5(installed);
  if (a === b) {
    console.log(`같다    ${name.padEnd(18)} ${a.slice(0, 8)}…  (${when(installed)})`);
  } else {
    bad++;
    console.log(
      `**다르다** ${name.padEnd(16)} 빌드 ${a.slice(0, 8)}… (${when(built)})  ≠  설치본 ${b.slice(0, 8)}… (${when(installed)})`,
    );
  }
}

console.log(
  bad === 0
    ? `OK — 설치본이 빌드와 같다${skipped ? ` (건너뜀 ${skipped})` : ""}`
    : `FAIL — ${bad}개가 빌드와 다르거나 안 깔렸다. 잠겨 있으면 이름을 바꿔 치우고 넣는다.`,
);
process.exit(bad === 0 ? 0 : 1);
