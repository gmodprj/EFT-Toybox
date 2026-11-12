# IcebreakerOffMapGuard 빌드 결과를 SPT 에 설치한다.
# 사용: pwsh -File tools/install-icebreakeroffmapguard.ps1 [-SptDir E:\Spt] [-Remove]
#
# 아이스브레이커 백포트(2866)가 그 맵이 아닌 곳에서 하는 일 둘을 밖에서 가리는 플러그인이다.
# 경위는 doc/fixes/2026-11-11-icebreaker-offmap-guard.md.
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Remove,
    # 게임이 떠 있어도 넣는다. 돌고 있는 DLL 은 지울 수 없으므로 이름을 바꿔 치우고 새것을 넣는다 —
    # 돌고 있는 게임은 옛 코드를 그대로 쓰고, 새것은 다음 기동부터 돈다.
    [switch]$AllowGameRunning
)

$ErrorActionPreference = 'Stop'

$src = Join-Path $PSScriptRoot '..\src\IcebreakerOffMapGuard' | Resolve-Path
$dll = Join-Path $src 'bin\Release\IcebreakerOffMapGuard.dll'
$dest = Join-Path $SptDir 'BepInEx\plugins\IcebreakerOffMapGuard'
$cfg = Join-Path $SptDir 'BepInEx\config\spt.icebreakeroffmapguard.cfg'

if ($Remove) {
    if (Test-Path $dest) {
        Remove-Item $dest -Recurse -Force
        "지웠다: $dest"
    } else {
        "설치돼 있지 않다: $dest"
    }
    # F12 설정은 남긴다 — 다시 넣을 때 값이 돌아온다. 지우려면 직접: $cfg
    if (Test-Path $cfg) { "설정은 남겼다: $cfg" }
    "게임을 다시 켜야 적용된다."
    return
}

if (-not (Test-Path $dll)) { throw "빌드 결과가 없다: $dll  (먼저 dotnet build -c Release)" }

# 게임이 떠 있으면 DLL 이 잠겨 조용히 안 깔린다 — 먼저 멈춘다(끄지는 않는다).
$game = Get-Process -Name EscapeFromTarkov -ErrorAction SilentlyContinue
if ($game -and -not $AllowGameRunning) {
    throw '게임이 떠 있다. 끄고 다시 돌리거나 -AllowGameRunning 으로 돌려라 — 지금 그냥 복사하면 잠긴 DLL 때문에 설치되지 않는다.'
}

# 잠긴 DLL 은 "나중에 반영" 이 아니라 안 깔린 것이다. 지울 수는 없지만 이름은 바꿀 수 있으므로
# 치워 두고 새것을 넣는다. BepInEx 는 .dll 만 읽어 치운 파일을 무시한다.
$dllInPlace = Join-Path $dest 'IcebreakerOffMapGuard.dll'
if ($game -and (Test-Path $dllInPlace)) {
    $parked = "$dllInPlace.locked-" + (Get-Date -Format 'yyyyMMdd-HHmmss')
    Rename-Item -LiteralPath $dllInPlace -NewName (Split-Path $parked -Leaf)
    "돌고 있는 DLL 을 치웠다: $parked  (지금 돌고 있는 게임은 옛 코드를 쓴다)"
}

# 대상 모드가 없으면 플러그인은 아무것도 걸지 않는다. 사람이 알아채게 알린다.
$target = Join-Path $SptDir 'BepInEx\plugins\ManimalIcebreaker\ManimalIcebreakerClient.dll'
if (-not (Test-Path $target)) {
    "경고: 아이스브레이커 백포트가 없다($target). 이 플러그인은 걸 대상이 없어 조용히 쉰다."
}

New-Item -ItemType Directory -Force -Path $dest | Out-Null
Copy-Item $dll -Destination $dest -Force

"설치됨: $dest"
Get-ChildItem $dest -Recurse -File | ForEach-Object {
    "  {0,-32} {1,7} bytes  {2}" -f $_.Name, $_.Length, $_.LastWriteTime.ToString('yyyy-MM-dd HH:mm')
}
"게임을 다시 켜야 적용된다. 기동 로그에서 '걸었다: Manimal.Icebreaker...' 두 줄을 확인한다."
