# SPT Team Manager(STM) 빌드 결과를 SPT 에 설치한다.
# 사용: pwsh -File tools/install-sptteammanager.ps1 [-SptDir E:\Spt] [-Configuration Debug] [-Remove]
# 덮어쓰기가 생기면 아무것도 복사하지 않고 중단한다(doc/PROCEDURE.md).
param(
    [string]$SptDir = 'E:\Spt',
    [string]$Configuration = 'Debug',
    [switch]$Remove
)

$ErrorActionPreference = 'Stop'

$src = Join-Path $PSScriptRoot '..\src\SptTeamManager' | Resolve-Path
$clientDest = Join-Path $SptDir 'BepInEx\plugins\SptTeamManager'
$serverDest = Join-Path $SptDir 'SPT_Runtime\user\mods\SptTeamManager-ServerMod'

$clientBin = Join-Path $src "client\bin\$Configuration\net472"
$serverBin = Join-Path $src "server\bin\$Configuration\net10.0"
$resources = Join-Path $src 'server\Resources'

# 원본 파일 -> 대상 파일 목록. 덮어쓰기 검사와 복사가 같은 목록을 쓴다.
$plan = New-Object System.Collections.Generic.List[object]
function Add-Plan([string]$from, [string]$to) {
    $plan.Add([pscustomobject]@{ From = $from; To = $to })
}

Add-Plan (Join-Path $clientBin 'SptTeamManager.dll') (Join-Path $clientDest 'SptTeamManager.dll')
Add-Plan (Join-Path $serverBin 'SptTeamManager.Server.dll') (Join-Path $serverDest 'SptTeamManager.Server.dll')
Add-Plan (Join-Path $serverBin 'LiteDB.dll') (Join-Path $serverDest 'LiteDB.dll')

if (Test-Path $resources) {
    $resRoot = (Resolve-Path $resources).Path
    Get-ChildItem $resRoot -Recurse -File | ForEach-Object {
        $rel = $_.FullName.Substring($resRoot.Length).TrimStart('\')
        Add-Plan $_.FullName (Join-Path (Join-Path $serverDest 'Resources') $rel)
    }
}

if ($Remove) {
    # 설치한 파일만 지운다. 팀원 DB(Resources\stm-teammates*)는 사용자 데이터라 건드리지 않는다.
    foreach ($p in $plan) {
        if (Test-Path $p.To) { Remove-Item $p.To -Force; "제거됨: $($p.To)" }
    }
    foreach ($d in @($clientDest, $serverDest)) {
        if ((Test-Path $d) -and -not (Get-ChildItem $d -Recurse -File)) { Remove-Item $d -Recurse -Force }
    }
    "게임과 서버를 다시 켜야 적용된다."
    return
}

# 원본이 없으면 조용히 건너뛰지 않고 중단한다.
$missing = $plan | Where-Object { -not (Test-Path $_.From) }
if ($missing) {
    $list = ($missing | ForEach-Object { "  $($_.From)" }) -join "`n"
    throw "빌드 결과가 없다. 먼저 dotnet build -c $Configuration (클라이언트·서버 둘 다):`n$list"
}

# 덮어쓰기가 하나라도 생기면 아무것도 복사하지 않고 중단한다.
$clash = $plan | Where-Object { Test-Path $_.To }
if ($clash) {
    $list = ($clash | ForEach-Object { "  $($_.To)" }) -join "`n"
    throw "이미 있는 파일이 있어 중단한다. 아무것도 복사하지 않았다:`n$list"
}

foreach ($p in $plan) {
    New-Item -ItemType Directory -Force -Path (Split-Path $p.To -Parent) | Out-Null
    Copy-Item $p.From -Destination $p.To
}

"설치됨:"
foreach ($d in @($clientDest, $serverDest)) {
    "  $d"
    Get-ChildItem $d -Recurse -File | ForEach-Object {
        "    {0,-34} {1,9} bytes" -f $_.FullName.Substring($d.Length).TrimStart('\'), $_.Length
    }
}
"게임과 서버를 다시 켜야 적용된다."
