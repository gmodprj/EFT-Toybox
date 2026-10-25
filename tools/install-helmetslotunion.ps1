# HelmetSlotUnion 빌드 결과를 SPT 서버 모드 폴더에 설치한다.
# 사용: pwsh -File tools/install-helmetslotunion.ps1 [-SptDir E:\Spt]
#
# config.jsonc 는 이미 있으면 그대로 둔다(사용자가 고친 값을 덮지 않는다).
param(
    [string]$SptDir = 'E:\Spt'
)

$ErrorActionPreference = 'Stop'

$src = Join-Path $PSScriptRoot '..\src\HelmetSlotUnion' | Resolve-Path
$dll = Join-Path $src 'bin\Release\HelmetSlotUnion.dll'
$cfg = Join-Path $src 'config.jsonc'
$dest = Join-Path $SptDir 'SPT_Runtime\user\mods\HelmetSlotUnion'

if (-not (Test-Path $dll)) {
    throw "빌드 결과가 없다: $dll  (먼저 dotnet build src/HelmetSlotUnion -c Release — .NET 10 SDK 필요)"
}

New-Item -ItemType Directory -Force -Path $dest | Out-Null
Copy-Item $dll -Destination $dest -Force

$destCfg = Join-Path $dest 'config.jsonc'
if (Test-Path $destCfg) {
    "설정 유지: $destCfg (기본값은 $cfg 에서 확인)"
} else {
    Copy-Item $cfg -Destination $dest
    "설정 생성: $destCfg"
}

"설치됨: $dest"
Get-ChildItem $dest -File | ForEach-Object {
    "  {0,-20} {1,7} bytes" -f $_.Name, $_.Length
}
"서버 모드라 SPT 서버를 다시 켜야 적용된다."
