# GogglesWheel 빌드 결과를 SPT 에 설치한다.
# 사용: pwsh -File tools/install-goggleswheel.ps1 [-SptDir E:\Spt] [-Remove]
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Remove
)

$ErrorActionPreference = 'Stop'

$src = Join-Path $PSScriptRoot '..\src\GogglesWheel' | Resolve-Path
$dest = Join-Path $SptDir 'BepInEx\plugins\GogglesWheel'

if ($Remove) {
    if (Test-Path $dest) {
        Remove-Item $dest -Recurse -Force
        "제거됨: $dest"
    } else {
        "이미 없다: $dest"
    }
    "게임을 다시 켜야 적용된다."
    return
}

$dll = Join-Path $src 'bin\Release\GogglesWheel.dll'
if (-not (Test-Path $dll)) { throw "빌드 결과가 없다: $dll  (먼저 dotnet build -c Release)" }

New-Item -ItemType Directory -Force -Path $dest | Out-Null
Copy-Item $dll -Destination $dest -Force

# 이미 있는 설정은 덮어쓰지 않는다 — 사용자가 고친 값이 날아가면 안 된다.
$configSrc = Join-Path $src 'config.jsonc'
$configDest = Join-Path $dest 'config.jsonc'
if (Test-Path $configDest) {
    "설정은 그대로 둔다: $configDest"
} else {
    Copy-Item $configSrc -Destination $configDest -Force
}

"설치됨: $dest"
Get-ChildItem $dest -Recurse -File | ForEach-Object {
    "  {0,-26} {1,7} bytes" -f $_.Name, $_.Length
}
"게임을 다시 켜야 적용된다. 서버는 끌 필요 없다."
