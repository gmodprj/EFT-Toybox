# PrestigeStashRelax 의 **클라이언트 플러그인**을 SPT 에 설치한다.
# 사용: pwsh -File tools/install-prestigestashrelax-plugin.ps1 [-SptDir E:\Spt] [-Remove]
#
# 서버 모드(tools/install-prestigestashrelax.ps1)와 짝이다. 서버 쪽은 격자 크기와 허용 목록을
# 넓히고, 이쪽은 클라이언트가 따로 세운 관문(내용물이 든 가방 거부)을 뗀다.
#
# 게임이 돌고 있으면 DLL 이 잠겨 **덮어쓰기가 조용히 실패한다** — 먼저 확인하고 중단한다.
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Remove,
    # 게임이 떠 있어도 넣는다. 돌고 있는 DLL 은 덮어쓸 수 없으므로 이름을 바꿔 치우고 새것을
    # 넣는다 — 지금 돌고 있는 게임은 옛 코드를 쓰고, 새것은 다음 기동부터 돈다.
    [switch]$AllowGameRunning
)

$ErrorActionPreference = 'Stop'

$src = Join-Path $PSScriptRoot '..\src\PrestigeStashRelax.Plugin' | Resolve-Path
$dll = Join-Path $src 'bin\Release\PrestigeStashRelaxClient.dll'
$dest = Join-Path $SptDir 'BepInEx\plugins\PrestigeStashRelax'

if ($Remove) {
    if (Test-Path $dest) {
        Remove-Item $dest -Recurse -Force
        "지웠다: $dest"
    }
    else {
        "설치돼 있지 않다: $dest"
    }
    "게임을 다시 켜야 반영된다."
    return
}

if (-not (Test-Path $dll)) {
    throw "빌드 결과가 없다: $dll  (먼저 dotnet build src/PrestigeStashRelax.Plugin -c Release)"
}

$game = Get-Process -Name 'EscapeFromTarkov' -ErrorAction SilentlyContinue
if ($game -and -not $AllowGameRunning) {
    throw "게임이 돌고 있다(PID $($game.Id)). 끄고 다시 돌리거나 -AllowGameRunning 으로 돌려라 — 지금 그냥 복사하면 잠긴 DLL 때문에 설치되지 않는다."
}

New-Item -ItemType Directory -Force -Path $dest | Out-Null

$inPlace = Join-Path $dest 'PrestigeStashRelaxClient.dll'
if ($game -and (Test-Path $inPlace)) {
    $parked = "$inPlace.locked-" + (Get-Date -Format 'yyyyMMdd-HHmmss')
    Rename-Item -LiteralPath $inPlace -NewName (Split-Path $parked -Leaf)
    "돌고 있는 DLL 을 치웠다: $parked  (지금 돌고 있는 게임은 옛 코드를 쓴다)"
}

Copy-Item $dll -Destination $dest -Force

"설치됨: $dest"
Get-ChildItem $dest -Recurse -File | ForEach-Object {
    "  {0,-32} {1,7} bytes" -f $_.Name, $_.Length
}
"게임을 다시 켜야 적용된다. F12 → '프레스티지 이전 창고 풀기' 에서 켜고 끈다."
