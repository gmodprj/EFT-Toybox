# AirdropProbe(보급 지점 측정기) 빌드 결과를 SPT 에 설치한다.
# 측정 전용 도구다. 다 재면 -Remove 로 지운다.
# 사용: pwsh -File tools/install-airdropprobe.ps1 [-SptDir E:\Spt] [-Remove]
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Remove
)

$ErrorActionPreference = 'Stop'

$dest = Join-Path $SptDir 'BepInEx\plugins\AirdropProbe'
$cfg = Join-Path $SptDir 'BepInEx\config\spt.airdropprobe.cfg'

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

$src = Join-Path $PSScriptRoot '..\src\AirdropProbe' | Resolve-Path
$dll = Join-Path $src 'bin\Release\AirdropProbe.dll'

if (-not (Test-Path $dll)) { throw "빌드 결과가 없다: $dll  (먼저 dotnet build -c Release)" }

New-Item -ItemType Directory -Force -Path $dest | Out-Null
Copy-Item $dll -Destination $dest -Force

"설치됨: $dest"
Get-ChildItem $dest -Recurse -File | ForEach-Object {
    "  {0,-28} {1,7} bytes" -f $_.Name, $_.Length
}
"게임을 다시 켜야 적용된다."
"  레이드에서 Ctrl+Alt+A : 보급 지점 거리를 잰다 (읽기만 한다)"
"  레이드에서 Ctrl+Alt+D : 상자를 떨어뜨린다 (게임을 실제로 바꾼다)"
"결과는 BepInEx\LogOutput.log 와 $dest\probe.log 둘 다에 쌓인다."
