# ProfilePorter(프로필 사이로 물건 베끼기 — 외부 창)를 SPT 폴더에 설치한다.
# `Give Ui.exe`·`AirdropCaller.exe` 와 같은 자리에 둔다.
# 사용: pwsh -File tools/install-profileporter-app.ps1 [-SptDir E:\Spt] [-Remove] [-Purge]
#
# profile-porter.config.json 은 이미 있으면 그대로 둔다(사용자가 고친 값을 덮지 않는다).
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Remove,
    # 설정까지 싹 지운다. -Remove 는 남긴다.
    [switch]$Purge
)

$ErrorActionPreference = 'Stop'

$exe = Join-Path $SptDir 'ProfilePorter.exe'
$cfg = Join-Path $SptDir 'profile-porter.config.json'

# **아이콘 캐시는 지우지 않는다.** `<SptDir>\icon-cache` 는 AirdropCaller 와 **함께 쓰는**
# 자리다(2026-11-08 기준 2,317개 9.5MB). 여기서 치우면 남의 창이 다시 받아야 한다.
# AirdropCaller 쪽 스크립트는 자기 것으로 알고 지우는데, 그쪽이 먼저 생겼기 때문이다.

if ($Remove -or $Purge) {
    $paths = if ($Purge) { @($exe, $cfg) } else { @($exe) }

    foreach ($path in $paths) {
        if (Test-Path $path) {
            Remove-Item $path -Recurse -Force -Confirm:$false
            "지웠다: $path"
        } else {
            "없다(건너뜀): $path"
        }
    }

    if (-not $Purge -and (Test-Path $cfg)) {
        "남겼다: $cfg  (싹 지우려면 -Purge)"
    }

    "아이콘 캐시는 그대로 둔다: $(Join-Path $SptDir 'icon-cache')  (AirdropCaller 와 함께 쓴다)"
    "내보낸 파일과 백업은 서버 모드 폴더에 있다 — tools/install-profileporter.ps1 을 본다."
    return
}

$src = Join-Path $PSScriptRoot '..\src\ProfilePorter.App' | Resolve-Path
$built = Join-Path $src 'bin\Release\win-x64\publish\ProfilePorter.exe'

if (-not (Test-Path $built)) {
    throw "게시 결과가 없다: $built  (먼저 dotnet publish src/ProfilePorter.App -c Release)"
}

# **묵은 게시본을 조용히 깔지 않는다.** `dotnet build` 만 하고 `publish` 를 빠뜨리면 지난번
# 게시본이 그대로 복사되고 바이트 수까지 같아 눈치채기 어렵다(AirdropCaller 에서 2026-11-07 에
# 그랬다). 소스가 더 새로우면 멈춘다 — 다시 게시하는 값이, 묵은 exe 를 깔고 "창이 떴다" 고
# 보고하는 값보다 싸다.
$newestSource = Get-ChildItem $src -Recurse -Include *.cs,*.xaml,*.csproj -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($newestSource -and (Get-Item $built).LastWriteTime -lt $newestSource.LastWriteTime) {
    throw "게시본이 소스보다 오래됐다: $($newestSource.Name) 가 더 새롭다. 먼저 dotnet publish src/ProfilePorter.App -c Release"
}

# 창이 떠 있으면 파일이 잠겨 복사가 실패한다. 그냥 두면 "설치됨" 도 안 찍히면서 exit 0 이라
# 깔린 줄 알게 된다 — 멈추고 무엇을 해야 하는지 말한다.
if (Test-Path $exe) {
    try {
        $fs = [System.IO.File]::Open($exe, 'Open', 'ReadWrite', 'None')
        $fs.Close()
    } catch {
        throw "설치본이 잠겨 있다(창이 떠 있다): $exe`n   ProfilePorter 창을 닫고 다시 돌려라 — 지금 덮으면 설치되지 않는다."
    }
}

Copy-Item $built -Destination $exe -Force -ErrorAction Stop
"설치됨: $exe  ({0:N0} bytes)" -f (Get-Item $exe).Length

if (Test-Path $cfg) {
    "설정 유지: $cfg"
} else {
    "설정 없음 — 처음 띄울 때 기본값으로 만들어진다: $cfg"
}

".NET 10 데스크톱 런타임이 필요하다(이 PC 에는 SDK 가 있어 이미 있다)."
"서버 모드(ProfilePorter.Server)가 함께 있어야 동작한다 — tools/install-profileporter.ps1."
"아이콘 캐시는 AirdropCaller 와 함께 쓴다: $(Join-Path $SptDir 'icon-cache')"
