# DynamicMapsFactionMarkers 빌드 결과를 SPT 에 설치한다.
# 사용: pwsh -File tools/install-dynamicmapsfactionmarkers.ps1 [-SptDir E:\Spt] [-Remove]
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Remove
)

$ErrorActionPreference = 'Stop'

$src = Join-Path $PSScriptRoot '..\src\DynamicMapsFactionMarkers' | Resolve-Path
$dest = Join-Path $SptDir 'BepInEx\plugins\DynamicMapsFactionMarkers'

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

$dll = Join-Path $src 'bin\Release\DynamicMapsFactionMarkers.dll'
if (-not (Test-Path $dll)) { throw "빌드 결과가 없다: $dll  (먼저 dotnet build -c Release)" }

# Dynamic Maps 가 없으면 플러그인이 로드되지 않는다(하드 의존). 미리 짚어 준다.
$dynamicMaps = Join-Path $SptDir 'BepInEx\plugins\mpstark-dynamicmaps\DynamicMaps.dll'
if (-not (Test-Path $dynamicMaps)) { throw "Dynamic Maps 가 없다: $dynamicMaps" }

New-Item -ItemType Directory -Force -Path $dest | Out-Null
Copy-Item $dll -Destination $dest -Force

# 아이콘은 있을 때만 복사한다(Task 4 에서 들어온다).
$icon = Join-Path $src 'diamond.png'
if (Test-Path $icon) { Copy-Item $icon -Destination $dest -Force }

# 이미 있는 설정은 덮어쓰지 않는다 — 사용자가 고친 색이 날아가면 안 된다.
$configSrc = Join-Path $src 'config.jsonc'
$configDest = Join-Path $dest 'config.jsonc'
if (Test-Path $configDest) {
    "설정은 그대로 둔다: $configDest"
} else {
    Copy-Item $configSrc -Destination $configDest -Force
}

"설치됨: $dest"
Get-ChildItem $dest -Recurse -File | ForEach-Object {
    "  {0,-34} {1,7} bytes" -f $_.Name, $_.Length
}
"게임을 다시 켜야 적용된다. 서버는 끌 필요 없다."
