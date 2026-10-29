# AirdropCaller(보급 호출기 외부 창)를 SPT 폴더에 설치한다.
# Give Ui.exe 와 같은 자리에 둔다.
# 사용: pwsh -File tools/install-airdropcaller-app.ps1 [-SptDir E:\Spt] [-Remove]
#
# airdrop-caller.config.json 은 이미 있으면 그대로 둔다(사용자가 고친 값을 덮지 않는다).
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Remove
)

$ErrorActionPreference = 'Stop'

$exe = Join-Path $SptDir 'AirdropCaller.exe'
$cfg = Join-Path $SptDir 'airdrop-caller.config.json'

if ($Remove) {
    foreach ($path in @($exe, $cfg)) {
        if (Test-Path $path) {
            Remove-Item $path -Force -Confirm:$false
            "지웠다: $path"
        } else {
            "없다(건너뜀): $path"
        }
    }
    return
}

$src = Join-Path $PSScriptRoot '..\src\AirdropCaller.App' | Resolve-Path
$built = Join-Path $src 'bin\Release\win-x64\publish\AirdropCaller.exe'

if (-not (Test-Path $built)) {
    throw "게시 결과가 없다: $built  (먼저 dotnet publish src/AirdropCaller.App -c Release)"
}

Copy-Item $built -Destination $exe -Force
"설치됨: $exe  ({0:N0} bytes)" -f (Get-Item $exe).Length

if (Test-Path $cfg) {
    "설정 유지: $cfg"
} else {
    "설정 없음 — 처음 띄울 때 기본값으로 만들어진다: $cfg"
}

".NET 10 데스크톱 런타임이 필요하다(이 PC 에는 SDK 가 있어 이미 있다)."
"서버 모드(AirdropCaller)와 게임 플러그인(AirdropCaller.Plugin)이 함께 있어야 동작한다."
