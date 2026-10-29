# AirdropCaller.Plugin(보급 호출기 게임 쪽) 빌드 결과를 SPT 에 설치한다.
# 사용: pwsh -File tools/install-airdropcaller-plugin.ps1 [-SptDir E:\Spt] [-Remove]
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Remove
)

$ErrorActionPreference = 'Stop'

$dest = Join-Path $SptDir 'BepInEx\plugins\AirdropCaller'
$cfg = Join-Path $SptDir 'BepInEx\config\spt.airdropcaller.cfg'

if ($Remove) {
    foreach ($path in @($dest, $cfg)) {
        if (Test-Path $path) {
            Remove-Item $path -Recurse -Force -Confirm:$false
            "지웠다: $path"
        } else {
            "없다(건너뜀): $path"
        }
    }
    "게임을 다시 켜야 빠진다."
    return
}

$src = Join-Path $PSScriptRoot '..\src\AirdropCaller.Plugin' | Resolve-Path
$dll = Join-Path $src 'bin\Release\AirdropCaller.Plugin.dll'

if (-not (Test-Path $dll)) { throw "빌드 결과가 없다: $dll  (먼저 dotnet build -c Release)" }

New-Item -ItemType Directory -Force -Path $dest | Out-Null
Copy-Item $dll -Destination $dest -Force

"설치됨: $dest"
Get-ChildItem $dest -Recurse -File | ForEach-Object {
    "  {0,-32} {1,7} bytes" -f $_.Name, $_.Length
}
"게임을 다시 켜야 적용된다. 서버 모드(AirdropCaller)도 함께 있어야 한다."
