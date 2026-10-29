# AirdropCaller.Server 빌드 결과를 SPT 서버 모드 폴더에 설치한다.
# 사용: pwsh -File tools/install-airdropcaller-server.ps1 [-SptDir E:\Spt] [-Remove]
#
# config.jsonc 는 이미 있으면 그대로 둔다(사용자가 고친 값을 덮지 않는다).
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Remove
)

$ErrorActionPreference = 'Stop'

$dest = Join-Path $SptDir 'SPT_Runtime\user\mods\AirdropCaller'

if ($Remove) {
    if (Test-Path $dest) {
        Remove-Item $dest -Recurse -Force -Confirm:$false
        "지웠다: $dest"
    } else {
        "없다(건너뜀): $dest"
    }
    "서버를 다시 켜야 빠진다."
    return
}

$src = Join-Path $PSScriptRoot '..\src\AirdropCaller.Server' | Resolve-Path
$dll = Join-Path $src 'bin\Release\AirdropCaller.Server.dll'
$cfg = Join-Path $src 'config.jsonc'

if (-not (Test-Path $dll)) {
    throw "빌드 결과가 없다: $dll  (먼저 dotnet build src/AirdropCaller.Server -c Release — .NET 10 SDK 필요)"
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
    "  {0,-28} {1,7} bytes" -f $_.Name, $_.Length
}
"서버 모드라 SPT 서버를 다시 켜야 적용된다."
"적용 뒤: 레이드에서 Ctrl+Alt+D (AirdropProbe) 를 누르면 config.jsonc 에 적은 상자가 떨어진다."
