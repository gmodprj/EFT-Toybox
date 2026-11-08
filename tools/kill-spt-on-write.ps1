# 파일이 쓰이는 순간 SPT 서버를 강제 종료한다 — 전송 도중의 끊김(계획 6 B4)을 **시각이 아니라
# 사건으로** 만들기 위한 도구. "쏘고 N ms 뒤에 죽인다" 는 재지 않은 추측이 되므로 쓰지 않는다.
#
# 사용:
#   pwsh -File tools/kill-spt-on-write.ps1 -Watch <파일> [-Settle] [-TimeoutSeconds 60] [-DryRun]
#     -Settle : 그 파일이 **다 쓰였을 때까지**(마지막 바이트가 '}') 1ms 간격으로 기다린 뒤 죽인다.
#               프로필 json 을 쓰는 도중에 죽여 반토막 파일을 만들지 않으려는 것이다.
#     -DryRun : 죽이지 않고 사건만 적는다(배선 확인용).
#
# 게임이 돌고 있으면 아무것도 하지 않는다(tools/stop-spt-server.ps1 과 같은 규칙 — 레이드를 날리지 않는다).
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Watch,
    [switch] $Settle,
    [int] $SettleTimeoutMs = 3000,
    [int] $TimeoutSeconds = 60,
    [string] $ProcessName = 'SPT.Server',
    [switch] $DryRun
)

$ErrorActionPreference = 'Stop'
if (Get-Process -Name 'EscapeFromTarkov' -ErrorAction SilentlyContinue) {
    throw '게임이 돌고 있어 중단한다. 레이드 중에는 서버를 끊지 않는다.'
}
$dir = Split-Path -Parent $Watch
$name = Split-Path -Leaf $Watch
if (-not (Test-Path $dir)) { throw "폴더가 없다: $dir" }

$proc = Get-Process -Name $ProcessName -ErrorAction SilentlyContinue
if (-not $proc) { throw "$ProcessName 이 돌고 있지 않다." }
"감시 시작 $(Get-Date -Format 'HH:mm:ss.fff')  대상 $name  PID $($proc.Id)"

$fsw = [IO.FileSystemWatcher]::new($dir, $name)
$fsw.NotifyFilter = [IO.NotifyFilters]::LastWrite -bor [IO.NotifyFilters]::Size -bor [IO.NotifyFilters]::FileName
try {
    $hit = $fsw.WaitForChanged([IO.WatcherChangeTypes]::All, $TimeoutSeconds * 1000)
    if ($hit.TimedOut) { "시간 초과 — 아무 일도 없었다. 죽이지 않았다."; exit 3 }
    $at = Get-Date
    "사건 $($at.ToString('HH:mm:ss.fff'))  $($hit.ChangeType)  $($hit.Name)"

    if ($Settle) {
        $sw = [Diagnostics.Stopwatch]::StartNew()
        $ok = $false
        while ($sw.ElapsedMilliseconds -lt $SettleTimeoutMs) {
            try {
                $fs = [IO.File]::Open($Watch, 'Open', 'Read', 'ReadWrite')
                try {
                    if ($fs.Length -gt 2) {
                        $fs.Position = $fs.Length - 1
                        $last = $fs.ReadByte()
                        if ($last -eq 0x7D) { $ok = $true; break }   # '}'
                    }
                } finally { $fs.Dispose() }
            } catch { }
            Start-Sleep -Milliseconds 1
        }
        "다 쓰임 대기 $($sw.ElapsedMilliseconds)ms → $(if ($ok) { '완결(마지막 바이트 })' } else { '미완결 — 그래도 죽인다' })"
    }

    if ($DryRun) { "DryRun — 죽이지 않았다."; exit 0 }
    $killAt = Get-Date
    Stop-Process -Id $proc.Id -Force
    "강제 종료 $($killAt.ToString('HH:mm:ss.fff'))  (사건 뒤 $([int]($killAt - $at).TotalMilliseconds)ms)"
    $proc.WaitForExit(10000) | Out-Null
    "종료 확인 $(Get-Date -Format 'HH:mm:ss.fff')  살아있나=$(-not $proc.HasExited)"
} finally { $fsw.Dispose() }
