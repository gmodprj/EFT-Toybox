# SptKoreanLocale 빌드 결과와 사전을 SPT 서버 모드 폴더에 설치한다.
# 사용: pwsh -File tools/install-koreanlocale.ps1 [-SptDir E:\Spt] [-Remove]
#
# config.jsonc 는 이미 있으면 그대로 둔다(사용자가 고친 값을 덮지 않는다).
# lang\*.json 은 우리 사전이라 늘 새로 덮어쓴다.
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Remove
)

$ErrorActionPreference = 'Stop'

$src = Join-Path $PSScriptRoot '..\src\SptKoreanLocale' | Resolve-Path
$dll = Join-Path $src 'bin\Release\SptKoreanLocale.dll'
$cfg = Join-Path $src 'config.jsonc'
$lang = Join-Path $src 'lang'
$dest = Join-Path $SptDir 'SPT_Runtime\user\mods\SptKoreanLocale'

if ($Remove) {
    if (Test-Path $dest) {
        Remove-Item $dest -Recurse -Force
        "제거됨: $dest"
    } else {
        "설치돼 있지 않다: $dest"
    }
    "서버 모드라 SPT 서버를 다시 켜야 반영된다."
    return
}

if (-not (Test-Path $dll)) {
    throw "빌드 결과가 없다: $dll  (먼저 dotnet build src/SptKoreanLocale -c Release — .NET 10 SDK 필요)"
}

New-Item -ItemType Directory -Force -Path (Join-Path $dest 'lang') | Out-Null
Copy-Item $dll -Destination $dest -Force
Copy-Item (Join-Path $lang '*.json') -Destination (Join-Path $dest 'lang') -Force

$destCfg = Join-Path $dest 'config.jsonc'
if (Test-Path $destCfg) {
    "설정 유지: $destCfg (기본값은 $cfg 에서 확인)"
} else {
    Copy-Item $cfg -Destination $dest
    "설정 생성: $destCfg"
}

"설치됨: $dest"
Get-ChildItem $dest -Recurse -File | ForEach-Object {
    "  {0,-28} {1,7} bytes" -f $_.Name, $_.Length
}
"서버 모드라 SPT 서버를 다시 켜야 적용된다."
'적용 여부는 서버를 켠 뒤 "node tools/check-locale-dict.mjs" 로 확인한다.'
