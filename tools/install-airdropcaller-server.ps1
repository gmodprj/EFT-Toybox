# AirdropCaller.Server 빌드 결과를 SPT 서버 모드 폴더에 설치한다.
# 사용: pwsh -File tools/install-airdropcaller-server.ps1 [-SptDir E:\Spt] [-Remove]
#
# config.jsonc 는 이미 있으면 그대로 둔다(사용자가 고친 값을 덮지 않는다).
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Remove,
    # 프리셋까지 싹 지운다. -Remove 는 프리셋을 남긴다.
    [switch]$Purge,
    # 서버가 떠 있어도 넣는다. 돌고 있는 DLL 은 덮어쓸 수 없으므로 이름을 바꿔 치우고
    # 새것을 넣는다 — **돌고 있는 서버는 옛 코드를 그대로 쓰고**, 새것은 다음 기동부터 돈다.
    [switch]$AllowServerRunning
)

$ErrorActionPreference = 'Stop'

$dest = Join-Path $SptDir 'SPT_Runtime\user\mods\AirdropCaller'
# 프리셋은 **사용자가 만든 것**이다. 모드 폴더 안에 있다고 해서 모드 파일이 아니다.
# 한 번 -Remove 로 통째로 날려 먹어서 이렇게 갈라 둔다 (2026-10-29).
$presets = Join-Path $dest 'presets'

if ($Remove -or $Purge) {
    if (-not (Test-Path $dest)) {
        "없다(건너뜀): $dest"
        "서버를 다시 켜야 빠진다."
        return
    }

    $keep = (-not $Purge) -and (Test-Path $presets)
    if ($keep) {
        # 모드 파일만 걷어내고 프리셋은 그 자리에 둔다.
        Get-ChildItem $dest -Force | Where-Object { $_.FullName -ne $presets } |
            Remove-Item -Recurse -Force -Confirm:$false
        $n = (Get-ChildItem $presets -Recurse -File -Filter *.json -ErrorAction SilentlyContinue).Count
        "지웠다: $dest 의 모드 파일"
        "남겼다: $presets  (프리셋 $n 개 — 싹 지우려면 -Purge)"
    } else {
        Remove-Item $dest -Recurse -Force -Confirm:$false
        "지웠다: $dest"
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

# 서버가 떠 있으면 DLL 이 잠겨 **덮어쓰기가 실패한다**(깔린 줄 알고 넘어가는 것이 가장 나쁘다).
$server = Get-Process -Name 'SPT.Server' -ErrorAction SilentlyContinue
$inPlace = Join-Path $dest 'AirdropCaller.Server.dll'
if ($server -and -not $AllowServerRunning) {
    throw 'SPT 서버가 떠 있다. 끄고 다시 돌리거나 -AllowServerRunning 으로 돌려라 — 지금 그냥 복사하면 잠긴 DLL 때문에 설치되지 않는다.'
}

if ($server -and (Test-Path $inPlace)) {
    # 지울 수는 없지만 이름은 바꿀 수 있다. 치워 두고 새것을 넣는다 —
    # SPT 는 mods 폴더에서 .dll 만 읽으므로 치운 파일은 로드되지 않는다.
    $parked = "$inPlace.locked-" + (Get-Date -Format 'yyyyMMdd-HHmmss')
    Rename-Item -LiteralPath $inPlace -NewName (Split-Path $parked -Leaf)
    "돌고 있는 DLL 을 치웠다: $parked  (지금 돌고 있는 서버는 옛 코드를 쓴다)"
}

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
