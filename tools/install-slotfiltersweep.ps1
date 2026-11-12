# SlotFilterSweep 빌드 결과를 SPT 서버 모드 폴더에 설치한다.
# 사용: pwsh -File tools/install-slotfiltersweep.ps1 [-SptDir E:\Spt] [-Remove] [-AllowServerRunning]
#
# 칸 필터에 적혀 있지만 데이터베이스에 없는 부품 id 를 걷어내는 모드다.
# 기본 설정은 **세기만 하고 지우지 않는다**(Apply: false).
# config.jsonc 는 이미 있으면 그대로 둔다(사용자가 고친 값을 덮지 않는다).
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Remove,
    # 서버가 떠 있어도 넣는다. 돌고 있는 DLL 은 덮어쓸 수 없으므로 이름을 바꿔 치우고
    # 새것을 넣는다 — 돌고 있는 서버는 옛 코드를 쓰고, 새것은 다음 기동부터 돈다.
    [switch]$AllowServerRunning
)

$ErrorActionPreference = 'Stop'

$src = Join-Path $PSScriptRoot '..\src\SlotFilterSweep' | Resolve-Path
$dll = Join-Path $src 'bin\Release\SlotFilterSweep.dll'
$cfg = Join-Path $src 'config.jsonc'
$dest = Join-Path $SptDir 'SPT_Runtime\user\mods\SlotFilterSweep'

if ($Remove) {
    if (Test-Path $dest) {
        Remove-Item $dest -Recurse -Force
        "지웠다: $dest"
    } else {
        "설치돼 있지 않다: $dest"
    }
    "서버를 다시 켜야 반영된다."
    return
}

if (-not (Test-Path $dll)) {
    throw "빌드 결과가 없다: $dll  (먼저 dotnet build src/SlotFilterSweep -c Release — .NET 10 SDK 필요)"
}

New-Item -ItemType Directory -Force -Path $dest | Out-Null

# 서버가 떠 있으면 DLL 이 잠겨 **덮어쓰기가 실패한다**(깔린 줄 알고 넘어가는 것이 가장 나쁘다).
$server = Get-Process -Name 'SPT.Server' -ErrorAction SilentlyContinue
$inPlace = Join-Path $dest 'SlotFilterSweep.dll'
if ($server -and -not $AllowServerRunning) {
    throw 'SPT 서버가 떠 있다. 끄고 다시 돌리거나 -AllowServerRunning 으로 돌려라 — 지금 그냥 복사하면 잠긴 DLL 때문에 설치되지 않는다.'
}

if ($server -and (Test-Path $inPlace)) {
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
    "설정 생성: $destCfg  (Apply 는 false — 세기만 한다)"
}

"설치됨: $dest"
Get-ChildItem $dest -File | ForEach-Object {
    "  {0,-28} {1,7} bytes" -f $_.Name, $_.Length
}
"서버 모드라 SPT 서버를 다시 켜야 적용된다."
"기동 뒤: 로그의 '[SlotFilterSweep]' 줄과 $dest\dead-refs.json 을 본다."
