# SPT 서버를 닫는다. 모드 DLL 을 깔려면 서버가 그 파일을 놓아야 한다.
# 사용: pwsh -File tools/stop-spt-server.ps1 [-SptDir E:\Spt] [-TimeoutSeconds 20]
#
# 왜 전용 스크립트인가 — 서버는 보통 에이전트가 백그라운드로 띄우는데, 종료는 권한 분류기가
# 거부한다(2026-11-07 실측: Bash·PowerShell 양쪽. 창 핸들이 0 이라 CloseMainWindow 도 닿지
# 않는다). 그래서 넓은 `Stop-Process` 를 허용하는 대신 **이 스크립트 하나만** 허용 규칙에
# 넣는다. 그러면 열어 주는 권한이 "SPT 서버를 닫는다" 하나로 좁혀진다.
#
# 하는 일: 런처·서버에 정상 종료를 먼저 보내고, 안 죽으면 강제 종료한다. 그다음 모드 DLL 이
# 실제로 **풀렸는지 파일을 열어 확인**한다 — 프로세스가 사라진 것만으로는 잠김이 풀렸다고
# 보지 않는다(안전장치는 닫힌 쪽으로 넘어뜨린다).
#
# 게임(EscapeFromTarkov)이 돌고 있으면 **아무것도 하지 않고 중단한다.** 레이드 중에 서버를
# 끊으면 그 판이 날아간다.
param(
    [string]$SptDir = 'E:\Spt',
    [int]$TimeoutSeconds = 20
)

$ErrorActionPreference = 'Stop'

$game = Get-Process -Name 'EscapeFromTarkov' -ErrorAction SilentlyContinue
if ($game) {
    throw ("게임이 돌고 있어 중단한다(PID $($game.Id)). 레이드 중이면 서버를 끊지 않는다. " +
        "게임을 먼저 끄고 다시 실행한다.")
}

$targets = Get-Process -Name 'SPT.Launcher', 'SPT.Server' -ErrorAction SilentlyContinue
if (-not $targets) {
    "이미 닫혀 있다 (SPT.Launcher·SPT.Server 없음)."
}
else {
    foreach ($p in $targets) { "찾음: $($p.ProcessName) PID $($p.Id) (기동 $($p.StartTime))" }

    # 1) 정상 종료부터. 창이 있는 것(런처)은 이걸로 닫힌다.
    foreach ($p in $targets) {
        try { $null = $p.CloseMainWindow() } catch { }
    }
    $deadline = (Get-Date).AddSeconds([Math]::Min($TimeoutSeconds, 8))
    while ((Get-Date) -lt $deadline) {
        if (-not (Get-Process -Name 'SPT.Launcher', 'SPT.Server' -ErrorAction SilentlyContinue)) { break }
        Start-Sleep -Milliseconds 500
    }

    # 2) 남은 것은 강제 종료. 서버 콘솔은 창 핸들이 0 이라 여기까지 온다.
    $left = Get-Process -Name 'SPT.Launcher', 'SPT.Server' -ErrorAction SilentlyContinue
    foreach ($p in $left) {
        "정상 종료가 안 닿았다 — 강제 종료: $($p.ProcessName) PID $($p.Id)"
        Stop-Process -Id $p.Id -Force
    }

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        if (-not (Get-Process -Name 'SPT.Launcher', 'SPT.Server' -ErrorAction SilentlyContinue)) { break }
        Start-Sleep -Milliseconds 500
    }
    $still = Get-Process -Name 'SPT.Launcher', 'SPT.Server' -ErrorAction SilentlyContinue
    if ($still) {
        throw ("아직 살아 있다: " + (($still | ForEach-Object { "$($_.ProcessName)/$($_.Id)" }) -join ', '))
    }
    "종료됨."
}

# 3) **프로세스가 없는 것과 파일이 풀린 것은 다르다.** 실제로 열어 본다.
$probe = Join-Path $SptDir 'SPT_Runtime\user\mods\SptTeamManager-ServerMod\SptTeamManager.Server.dll'
if (Test-Path $probe -PathType Leaf) {
    try {
        $fs = [System.IO.File]::Open($probe, 'Open', 'ReadWrite', 'None')
        $fs.Close()
        "DLL 잠김 풀림: $probe"
    }
    catch {
        throw ("프로세스는 없는데 DLL 이 아직 잠겨 있다: $probe`n" +
            "다른 무엇이 잡고 있다. 그 상태로 설치하면 조용히 실패한다.`n$($_.Exception.Message)")
    }
}
else {
    "확인 건너뜀 — STM 서버 모드가 설치돼 있지 않다: $probe"
}
