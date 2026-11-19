# EFT 클라이언트를 프로필 하나로 바로 띄운다. 런처 GUI 를 거치지 않는다.
# 사용: pwsh -File tools/launch-game.ps1 -Token <프로필id> [-SptDir E:\Spt]
#
# 인자 꼴은 런처가 쓰는 것을 그대로 베꼈다(`user\logs\Launcher.log` 의
# `[SPTarkov.Core.Helpers.GameHelper] args:` 줄):
#
#   -force-gfx-jobs native -token=<프로필id> -config={'BackendUrl':'https://127.0.0.1:6969',...}
#
# 왜 스크립트인가 — `-config=` 의 중괄호를 셸에 그대로 주면 PowerShell 이 스크립트 블록으로
# 읽어 파싱이 깨진다. 여기서 문자열로 만들어 넘긴다.
#
# 서버가 떠 있지 않으면 로그인에서 막히므로 **먼저 확인하고 중단한다.**
param(
    [Parameter(Mandatory = $true)][string]$Token,
    [string]$SptDir = 'E:\Spt',
    [string]$BackendUrl = 'https://127.0.0.1:6969'
)

$ErrorActionPreference = 'Stop'

$exe = Join-Path $SptDir 'EscapeFromTarkov.exe'
if (-not (Test-Path $exe -PathType Leaf)) { throw "게임 실행 파일이 없다: $exe" }

$profile = Join-Path $SptDir "SPT_Runtime\user\profiles\$Token.json"
if (-not (Test-Path $profile -PathType Leaf)) { throw "그 프로필이 없다: $profile" }

# 서버가 응답하지 않으면 띄워도 로그인에서 막힌다 — 안전장치는 닫힌 쪽으로 넘어뜨린다.
try {
    $health = Invoke-WebRequest -Uri "$BackendUrl/health" -SkipCertificateCheck -TimeoutSec 5
    "서버: HTTP $($health.StatusCode) $($health.Content)"
}
catch {
    throw "서버가 $BackendUrl 에 응답하지 않는다. 먼저 SPT.Server 를 띄운다.`n$($_.Exception.Message)"
}

$running = Get-Process -Name 'EscapeFromTarkov' -ErrorAction SilentlyContinue
if ($running) { throw "게임이 이미 돌고 있다(PID $($running.Id)). 두 번 띄우지 않는다." }

# -AsHashtable: 모드가 대소문자만 다른 키를 함께 쓰는 프로필이 있다(2026-11-19 needFuelForAllProductionTime /
# NeedFuelForAllProductionTime) — 기본 ConvertFrom-Json 은 그런 JSON 을 거절한다.
$nickname = (Get-Content $profile -Raw | ConvertFrom-Json -AsHashtable).characters.pmc.Info.Nickname
$config = "{'BackendUrl':'$BackendUrl','Version':'live','MatchingVersion':'live'}"

"띄운다: $nickname ($Token)"
Start-Process -FilePath $exe -WorkingDirectory $SptDir -ArgumentList @(
    '-force-gfx-jobs', 'native', "-token=$Token", "-config=$config"
)
"게임 프로세스를 시작했다. 로그인 화면까지 1~2분 걸린다."
