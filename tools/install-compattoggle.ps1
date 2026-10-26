# CompatToggle 빌드 결과와 한국어 문구를 SPT 에 설치한다.
# 사용: pwsh -File tools/install-compattoggle.ps1 [-SptDir E:\Spt]
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

$src = Join-Path $PSScriptRoot '..\src\CompatToggle' | Resolve-Path
$dll = Join-Path $src 'bin\Release\CompatToggle.dll'
$lang = Join-Path $PSScriptRoot '..\src\SptKoreanConfig\lang\spt.compattoggle.json'

if (-not (Test-Path $dll)) { throw "빌드 결과가 없다: $dll  (먼저 dotnet build -c Release)" }
if (-not (Test-Path $lang)) { throw "한국어 문구가 없다: $lang" }

# 게임·서버가 켜져 있으면 설치하지 않는다.
$running = Get-Process -Name 'EscapeFromTarkov' -ErrorAction SilentlyContinue
if ($running) { throw '게임이 실행 중이다. 끄고 다시 실행하라.' }

$pluginDest = Join-Path $SptDir 'BepInEx\plugins\CompatToggle.dll'
$langDest = Join-Path $SptDir 'BepInEx\plugins\SptKoreanConfig\lang\spt.compattoggle.json'

# 덮어쓰기 확인. doc/PROCEDURE.md 규칙 — 덮어쓸 파일이 있으면 아무것도 복사하지 않고 중단한다.
# 단, 이 모드 자신의 이전 설치본은 갱신 대상이므로 -Force 로 허용한다.
$foreign = @()
foreach ($p in @($pluginDest, $langDest)) {
    if ((Test-Path $p) -and -not $Force) { $foreign += $p }
}
if ($foreign.Count -gt 0) {
    "이미 있는 파일:"
    $foreign | ForEach-Object { "  $_" }
    throw '덮어쓰려면 -Force 를 붙여라. 다른 모드의 파일이 아닌지 먼저 확인하라.'
}

New-Item -ItemType Directory -Force -Path (Split-Path $langDest) | Out-Null
Copy-Item $dll -Destination $pluginDest -Force
Copy-Item $lang -Destination $langDest -Force

"설치됨:"
"  {0,-44} {1,7} bytes" -f $pluginDest, (Get-Item $pluginDest).Length
"  {0,-44} {1,7} bytes" -f $langDest, (Get-Item $langDest).Length
"게임을 다시 켜야 플러그인이 로드된다. 그 뒤 토글 변경은 게임을 끄지 않고 바로 반영된다."
