# SptKoreanConfig 빌드 결과와 번역 사전을 SPT 에 설치한다.
# 사용: pwsh -File tools/install-koreanconfig.ps1 [-SptDir E:\Spt]
param(
    [string]$SptDir = 'E:\Spt'
)

$ErrorActionPreference = 'Stop'

$src = Join-Path $PSScriptRoot '..\src\SptKoreanConfig' | Resolve-Path
$dll = Join-Path $src 'bin\Release\SptKoreanConfig.dll'
$lang = Join-Path $src 'lang'
$dest = Join-Path $SptDir 'BepInEx\plugins\SptKoreanConfig'

if (-not (Test-Path $dll)) { throw "빌드 결과가 없다: $dll  (먼저 dotnet build -c Release)" }

New-Item -ItemType Directory -Force -Path (Join-Path $dest 'lang') | Out-Null
Copy-Item $dll -Destination $dest -Force
Copy-Item (Join-Path $lang '*.json') -Destination (Join-Path $dest 'lang') -Force

"설치됨: $dest"
Get-ChildItem $dest -Recurse -File | ForEach-Object {
    "  {0,-28} {1,7} bytes" -f $_.Name, $_.Length
}
"게임을 다시 켜야 적용된다. 번역 JSON 만 고친 경우는 F12 를 다시 열면 반영된다."
