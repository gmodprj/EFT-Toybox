# WebConfigBridge 빌드 결과를 SPT 서버 모드 폴더에 설치한다.
# 사용: pwsh -File tools/install-webconfigbridge.ps1 [-SptDir E:\Spt] [-Force]
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

$src = Join-Path $PSScriptRoot '..\src\WebConfigBridge' | Resolve-Path
$dll = Join-Path $src 'bin\Release\WebConfigBridge.dll'

if (-not (Test-Path $dll)) { throw "빌드 결과가 없다: $dll  (먼저 dotnet build -c Release)" }

# 서버가 켜져 있으면 DLL 을 잡고 있어 설치가 실패한다.
foreach ($name in @('SPT.Server', 'EscapeFromTarkov')) {
    if (Get-Process -Name $name -ErrorAction SilentlyContinue) { throw "$name 이 실행 중이다. 끄고 다시 실행하라." }
}

$dest = Join-Path $SptDir 'SPT_Runtime\user\mods\WebConfigBridge'
$destDll = Join-Path $dest 'WebConfigBridge.dll'

if ((Test-Path $destDll) -and -not $Force) {
    throw "이미 있다: $destDll  — 덮어쓰려면 -Force 를 붙여라."
}

New-Item -ItemType Directory -Force -Path $dest | Out-Null
Copy-Item $dll -Destination $destDll -Force

"설치됨:"
"  {0,-56} {1,8:N0} bytes" -f $destDll, (Get-Item $destDll).Length
"서버를 다시 켜면 https://127.0.0.1:6969/ 의 설정 편집기에 대상 모드의 설정이 나타난다."
