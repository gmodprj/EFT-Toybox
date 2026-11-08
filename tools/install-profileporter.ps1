# ProfilePorter.Server 빌드 결과를 SPT 서버 모드 폴더에 설치한다.
# 사용: pwsh -File tools/install-profileporter.ps1 [-SptDir E:\Spt] [-Remove] [-Purge]
#
# config.jsonc 는 이미 있으면 그대로 둔다(사용자가 고친 값을 덮지 않는다).
# porter\ 와 backups\ 는 **사용자의 것**이라 -Remove 로는 지우지 않는다 —
# 내보낸 파일과 들여오기 전 백업이 거기 있다. 싹 지우려면 -Purge.
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Remove,
    [switch]$Purge
)

$ErrorActionPreference = 'Stop'

$dest = Join-Path $SptDir 'SPT_Runtime\user\mods\ProfilePorter'
$keepDirs = @('porter', 'backups')

if ($Remove -or $Purge) {
    if (-not (Test-Path $dest)) {
        "없다(건너뜀): $dest"
        "서버를 다시 켜야 빠진다."
        return
    }

    if ($Purge) {
        Remove-Item $dest -Recurse -Force -Confirm:$false
        "지웠다(백업·내보낸 파일 포함): $dest"
    } else {
        $keepPaths = $keepDirs | ForEach-Object { Join-Path $dest $_ }
        Get-ChildItem $dest -Force | Where-Object { $keepPaths -notcontains $_.FullName } |
            Remove-Item -Recurse -Force -Confirm:$false
        foreach ($d in $keepDirs) {
            $p = Join-Path $dest $d
            if (Test-Path $p) {
                $n = (Get-ChildItem $p -Recurse -File -ErrorAction SilentlyContinue).Count
                "남겼다: $p  ($n 개)"
            }
        }
        "지웠다: $dest 의 모드 파일  (싹 지우려면 -Purge)"
    }

    "서버를 다시 켜야 빠진다."
    return
}

$src = Join-Path $PSScriptRoot '..\src\ProfilePorter.Server' | Resolve-Path
$dll = Join-Path $src 'bin\Release\ProfilePorter.Server.dll'
$cfg = Join-Path $src 'config.jsonc'

if (-not (Test-Path $dll)) {
    throw "빌드 결과가 없다: $dll  (먼저 dotnet build src/ProfilePorter.Server -c Release — .NET 10 SDK 필요)"
}

# 서버가 돌고 있으면 DLL 이 잠겨 복사가 조용히 실패하거나 터진다.
# "다음에 반영" 이 아니라 **안 깔린 것**이므로 여기서 멈추고 말해 준다.
$destDll = Join-Path $dest 'ProfilePorter.Server.dll'
if (Test-Path $destDll) {
    try {
        $fs = [System.IO.File]::Open($destDll, 'Open', 'ReadWrite', 'None')
        $fs.Close()
    } catch {
        throw "설치본 DLL 이 잠겨 있다(서버가 켜져 있다): $destDll`n   SPT 서버를 끄고 다시 돌려라 — 지금 덮으면 설치되지 않는다."
    }
}

New-Item -ItemType Directory -Force -Path $dest | Out-Null
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
    "  {0,-30} {1,7} bytes" -f $_.Name, $_.Length
}
"서버 모드라 SPT 서버를 다시 켜야 적용된다."
"적용 뒤 기동 로그에 '[ProfilePorter] 프로필 N개, 내보낸 파일 M개' 가 보여야 한다."
