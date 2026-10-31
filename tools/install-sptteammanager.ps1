# SPT Team Manager(STM) 빌드 결과를 SPT 에 설치한다.
# 사용: pwsh -File tools/install-sptteammanager.ps1 [-SptDir E:\Spt] [-Configuration Debug] [-Remove]
#
# 설치는 아래 중 하나라도 걸리면 아무것도 복사하지 않고 중단한다(doc/PROCEDURE.md).
#   - -SptDir 가 없다
#   - PIT Fireteam(pitFireTeam)이 설치돼 있다 — STM 은 PIT 의 포크라 둘을 함께 둘 수 없다
#   - 빌드 결과·리소스 원본이 없다
#   - 덮어쓰게 될 파일이 있다
#
# -Remove 는 이 스크립트가 설치한 파일만 지운다. 남는 것:
#   - Resources\settings.json (사용자가 설정 화면에서 고친 값)
#   - Resources\stm-teammates*\ (팀원 DB)
#   - 그 밖에 모드가 실행 중 만든 파일
# 지워지는 것: 클라이언트 DLL·에셋, 서버 DLL·deps.json·LiteDB.dll, 기본 제공 Resources(lang, avatars).
param(
    [string]$SptDir = 'E:\Spt',
    [string]$Configuration = 'Debug',
    [switch]$Remove
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $SptDir -PathType Container)) {
    throw "SPT 설치 폴더가 없다: $SptDir  (-SptDir 로 바른 경로를 준다)"
}

$src = Join-Path $PSScriptRoot '..\src\SptTeamManager' | Resolve-Path
$clientDest = Join-Path $SptDir 'BepInEx\plugins\SptTeamManager'
$serverDest = Join-Path $SptDir 'SPT_Runtime\user\mods\SptTeamManager-ServerMod'

$clientBin = Join-Path $src "client\bin\$Configuration\net472"
$serverBin = Join-Path $src "server\bin\$Configuration\net10.0"
$resources = Join-Path $src 'server\Resources'
$assets = Join-Path $src 'resources'

# 원본 파일 -> 대상 파일 목록. 덮어쓰기 검사와 복사가 같은 목록을 쓴다.
$plan = New-Object System.Collections.Generic.List[object]
function Add-Plan([string]$from, [string]$to) {
    $plan.Add([pscustomobject]@{ From = $from; To = $to })
}

# 폴더 안의 파일을 하위 구조 그대로 대상 폴더 밑에 계획에 넣는다.
function Add-PlanTree([string]$fromRoot, [string]$toRoot) {
    if (-not (Test-Path $fromRoot -PathType Container)) {
        # 폴더 자체가 없으면 빈 목록과 구분이 안 되므로 없는 원본으로 기록해 missing 검사에 걸리게 한다.
        Add-Plan $fromRoot $toRoot
        return
    }
    $root = (Resolve-Path $fromRoot).Path
    Get-ChildItem $root -Recurse -File | ForEach-Object {
        $rel = $_.FullName.Substring($root.Length).TrimStart('\')
        Add-Plan $_.FullName (Join-Path $toRoot $rel)
    }
}

Add-Plan (Join-Path $clientBin 'SptTeamManager.dll') (Join-Path $clientDest 'SptTeamManager.dll')
Add-PlanTree $assets $clientDest
Add-Plan (Join-Path $serverBin 'SptTeamManager.Server.dll') (Join-Path $serverDest 'SptTeamManager.Server.dll')
Add-Plan (Join-Path $serverBin 'SptTeamManager.Server.deps.json') (Join-Path $serverDest 'SptTeamManager.Server.deps.json')
Add-Plan (Join-Path $serverBin 'LiteDB.dll') (Join-Path $serverDest 'LiteDB.dll')
Add-PlanTree $resources (Join-Path $serverDest 'Resources')

if ($Remove) {
    # 사용자 데이터는 남긴다: Resources\settings.json, Resources\stm-teammates*\ (헤더 주석 참고).
    $keep = Join-Path $serverDest 'Resources\settings.json'
    foreach ($p in $plan) {
        if ($p.To -eq $keep) { continue }
        if (Test-Path $p.To) { Remove-Item $p.To -Force; "제거됨: $($p.To)" }
    }
    foreach ($d in @($clientDest, $serverDest)) {
        if (Test-Path $d) {
            # 빈 폴더만 정리한다. 파일이 남은 폴더(사용자 데이터)는 그대로 둔다.
            Get-ChildItem $d -Recurse -Directory | Sort-Object { $_.FullName.Length } -Descending |
                Where-Object { -not (Get-ChildItem $_.FullName -Force) } | Remove-Item -Force
            if (-not (Get-ChildItem $d -Force)) { Remove-Item $d -Force }
        }
    }
    if (Test-Path $keep) { "남겨 둠(사용자 설정): $keep" }
    "게임과 서버를 다시 켜야 적용된다."
    return
}

# PIT 가 있으면 중단한다. STM 은 PIT 의 포크이고 둘이 /singleplayer/pitfireteam/* 서버 라우트를 같이 쓴다.
$pitDirs = @(
    (Join-Path $SptDir 'SPT_Runtime\user\mods\pitFireTeam-ServerMod'),
    (Join-Path $SptDir 'BepInEx\plugins\pitFireTeam')
) | Where-Object { Test-Path $_ }
if ($pitDirs) {
    $list = ($pitDirs | ForEach-Object { "  $_" }) -join "`n"
    throw ("PIT Fireteam 이 설치돼 있어 중단한다. 아무것도 복사하지 않았다.`n" +
        "STM 은 PIT Fireteam 의 포크라 둘을 함께 설치할 수 없다. 둘이 /singleplayer/pitfireteam/* 서버 라우트를 같이 쓴다.`n" +
        "찾은 폴더 — 아래를 지운 뒤 다시 실행한다:`n$list")
}

# 원본이 없으면 조용히 건너뛰지 않고 중단한다.
$missing = $plan | Where-Object { -not (Test-Path $_.From) }
if ($missing) {
    $list = ($missing | ForEach-Object { "  $($_.From)" }) -join "`n"
    throw "빌드 결과·리소스 원본이 없다. 먼저 dotnet build -c $Configuration (클라이언트·서버 둘 다):`n$list"
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
