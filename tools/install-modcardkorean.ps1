# ModCardKorean 빌드 결과와 사전을 SPT 서버 모드 폴더에 설치한다.
# 사용: pwsh -File tools/install-modcardkorean.ps1 [-SptDir E:\Spt] [-Force] [-Remove]
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Force,
    [switch]$Remove
)

$ErrorActionPreference = 'Stop'

$src = Join-Path $PSScriptRoot '..\src\ModCardKorean' | Resolve-Path
$dll = Join-Path $src 'bin\Release\ModCardKorean.dll'
$dict = Join-Path $src 'mod-cards.ko.json'

$dest = Join-Path $SptDir 'SPT_Runtime\user\mods\ModCardKorean'
$destDll = Join-Path $dest 'ModCardKorean.dll'
$destDict = Join-Path $dest 'mod-cards.ko.json'

# 서버가 켜져 있으면 DLL 을 잡고 있어 설치도 제거도 실패한다.
foreach ($name in @('SPT.Server', 'EscapeFromTarkov')) {
    if (Get-Process -Name $name -ErrorAction SilentlyContinue) { throw "$name 이 실행 중이다. 끄고 다시 실행하라." }
}

if ($Remove) {
    if (-not (Test-Path $dest)) { "설치돼 있지 않다: $dest"; return }
    Remove-Item $dest -Recurse -Force
    "제거됨: $dest"
    "서버를 다시 켜면 모드 카드가 원래 영어 글귀로 돌아간다."
    return
}

if (-not (Test-Path $dll)) { throw "빌드 결과가 없다: $dll  (먼저 dotnet build -c Release)" }
if (-not (Test-Path $dict)) { throw "사전 파일이 없다: $dict" }

if ((Test-Path $destDll) -and -not $Force) {
    throw "이미 있다: $destDll  — 덮어쓰려면 -Force 를 붙여라."
}

New-Item -ItemType Directory -Force -Path $dest | Out-Null
Copy-Item $dll -Destination $destDll -Force
Copy-Item $dict -Destination $destDict -Force

"설치됨:"
foreach ($f in @($destDll, $destDict)) {
    "  {0,-60} {1,8:N0} bytes" -f $f, (Get-Item $f).Length
}
"서버를 다시 켠 뒤 node tools/check-mod-card-locale.mjs 로 확인하라."
