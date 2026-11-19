# ZombieMapInfection 빌드 결과를 SPT 서버 모드 폴더에 설치한다.
# 사용: pwsh -File tools/install-zombiemapinfection.ps1 [-SptDir E:\Spt] [-Remove]
#
# config.jsonc 는 이미 있으면 그대로 둔다(사용자가 고친 값을 덮지 않는다).
# 서버가 떠 있으면 DLL 이 잠겨 있으므로 먼저 서버를 끈다(tools/stop-spt-server.ps1 — 사용자에게 묻고).
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Remove
)

$ErrorActionPreference = 'Stop'

$src = Join-Path $PSScriptRoot '..\src\ZombieMapInfection' | Resolve-Path
$dll = Join-Path $src 'bin\Release\ZombieMapInfection.dll'
$cfg = Join-Path $src 'config.jsonc'
$dest = Join-Path $SptDir 'SPT_Runtime\user\mods\ZombieMapInfection'

if ($Remove) {
    if (Test-Path $dest) {
        Remove-Item $dest -Recurse -Force -Confirm:$false
        "제거됨: $dest"
    } else {
        "이미 없다: $dest"
    }
    "서버 모드라 SPT 서버를 다시 켜야 적용된다."
    return
}

if (-not (Test-Path $dll)) {
    throw "빌드 결과가 없다: $dll  (먼저 dotnet build src/ZombieMapInfection -c Release — .NET 10 SDK 필요)"
}
if (Get-Process -Name 'SPT.Server' -ErrorAction SilentlyContinue) {
    throw "SPT 서버가 떠 있다. 서버를 끈 뒤 다시 실행한다(서버 DLL 은 기동 중에 잠긴다)."
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
    "  {0,-26} {1,7} bytes" -f $_.Name, $_.Length
}
"서버 모드라 SPT 서버를 다시 켜야 적용된다. 기동 로그에서 '[ZombieMapInfection] halloween 감염률:' 줄을 본다."
