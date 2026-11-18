# TraderListView(상인 목록 보기) 빌드 결과를 SPT 에 설치한다.
# 사용: pwsh -File tools/install-traderlistview.ps1 [-SptDir E:\Spt] [-Remove]
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Remove
)

$ErrorActionPreference = 'Stop'

$src = Join-Path $PSScriptRoot '..\src\TraderListView' | Resolve-Path
$dest = Join-Path $SptDir 'BepInEx\plugins\TraderListView'

if ($Remove) {
    if (Test-Path $dest) {
        Remove-Item $dest -Recurse -Force
        "제거됨: $dest"
    } else {
        "이미 없다: $dest"
    }
    "게임을 다시 켜야 적용된다. 설정 파일 BepInEx\config\com.spt-mods.traderlistview.cfg 는 남는다."
    return
}

$dll = Join-Path $src 'bin\Release\TraderListView.dll'
if (-not (Test-Path $dll)) { throw "빌드 결과가 없다: $dll  (먼저 dotnet build src/TraderListView -c Release)" }

New-Item -ItemType Directory -Force -Path $dest | Out-Null
Copy-Item $dll -Destination $dest -Force

"설치됨: $dest"
Get-ChildItem $dest -Recurse -File | ForEach-Object {
    "  {0,-26} {1,7} bytes  {2}" -f $_.Name, $_.Length, $_.LastWriteTime
}
"게임을 다시 켜야 적용된다. 서버는 끌 필요 없다."
