# GridWindowScroll 빌드 결과를 SPT 에 설치한다.
# 사용: pwsh -File tools/install-gridwindowscroll.ps1 [-SptDir E:\Spt]
param(
    [string]$SptDir = 'E:\Spt'
)

$ErrorActionPreference = 'Stop'

$src = Join-Path $PSScriptRoot '..\src\GridWindowScroll' | Resolve-Path
$dll = Join-Path $src 'bin\Release\GridWindowScroll.dll'
$dest = Join-Path $SptDir 'BepInEx\plugins\GridWindowScroll'

if (-not (Test-Path $dll)) { throw "빌드 결과가 없다: $dll  (먼저 dotnet build -c Release)" }

New-Item -ItemType Directory -Force -Path $dest | Out-Null
Copy-Item $dll -Destination $dest -Force

"설치됨: $dest"
Get-ChildItem $dest -Recurse -File | ForEach-Object {
    "  {0,-28} {1,7} bytes" -f $_.Name, $_.Length
}
"게임을 다시 켜야 적용된다."
