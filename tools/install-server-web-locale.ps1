# 서버 웹 설정창(https://127.0.0.1:6969/)의 한국어 로케일을 SPT 에 설치한다.
# SPT 본체가 읽는 디렉토리에 ko.json 을 새로 넣는 것뿐이고, 기존 파일은 덮어쓰지 않는다.
# 사용: pwsh -File tools/install-server-web-locale.ps1 [-SptDir E:\Spt] [-Remove]
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Remove
)

$ErrorActionPreference = 'Stop'

$repo = Join-Path $PSScriptRoot '..' | Resolve-Path
$src = Join-Path $repo 'src\ServerWebKorean\ko.json'
$webLocaleDir = Join-Path $SptDir 'SPT_Runtime\SPT_Data\database\locales\web'
$dest = Join-Path $webLocaleDir 'ko.json'

if (-not (Test-Path $webLocaleDir)) { throw "웹 로케일 디렉토리가 없다: $webLocaleDir" }

# 서버·게임이 켜져 있으면 설치본을 건드리지 않는다.
$running = Get-Process -Name 'SPT.Server', 'EscapeFromTarkov' -ErrorAction SilentlyContinue
if ($running) {
    throw ("서버/게임이 실행 중이다({0}). 끄고 다시 실행한다." -f ($running.Name -join ', '))
}

if ($Remove) {
    if (Test-Path $dest) {
        Remove-Item $dest -Force
        "제거됨: $dest"
    }
    else { "이미 없다: $dest" }
    "서버를 다시 켜면 영어 키 상태로 돌아간다."
    return
}

if (-not (Test-Path $src)) { throw "번역 파일이 없다: $src" }

# JSON 이 깨져 있으면 서버가 로케일을 못 읽으니 미리 검사한다.
Get-Content $src -Raw -Encoding UTF8 | ConvertFrom-Json | Out-Null

# 같은 이름이 이미 있으면 백업 후 덮어쓴다(기본 설치본에는 en.json 만 있다).
if (Test-Path $dest) {
    $stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    $backupDir = Join-Path $repo "backup\server-web-locale-$stamp"
    New-Item -ItemType Directory -Force -Path $backupDir | Out-Null
    Copy-Item $dest -Destination (Join-Path $backupDir 'ko.json') -Force
    "기존 ko.json 을 백업했다: $backupDir\ko.json"
}

Copy-Item $src -Destination $dest -Force
"설치됨: $dest  ({0} bytes)" -f (Get-Item $dest).Length
"되돌리려면: pwsh -File tools/install-server-web-locale.ps1 -Remove"
"서버를 다시 켜야 적용된다."
