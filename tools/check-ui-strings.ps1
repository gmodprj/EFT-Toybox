# _ui.json 의 원문이 ConfigurationManager.dll 안에 보이는지 빠르게 훑는다. 오타를 잡는 용도다.
#
# 주의 — 이것만으로는 부족하다. 여기서는 DLL 안에 그 글자가 "들어 있는지"만 본다.
# 문자열 상수는 통째로만 바꿀 수 있으므로, 더 긴 문자열의 일부인 항목은
# 여기서 OK 로 나와도 실제로는 한 번도 바뀌지 않는다(실제로 5건 있었다).
# 확실한 확인은 게임 로그의 "창 UI 사전 항목 전부 사용됨" 줄이다.
#
#   pwsh -File tools/check-ui-strings.ps1 [-SptDir E:\Spt]
param(
    [string]$SptDir = 'E:\Spt'
)

$ErrorActionPreference = 'Stop'

$dll = Join-Path $SptDir 'BepInEx\plugins\spt\ConfigurationManager\ConfigurationManager.dll'
$json = Join-Path $PSScriptRoot '..\src\SptKoreanConfig\lang\_ui.json'

if (-not (Test-Path $dll)) { throw "DLL 이 없다: $dll" }
if (-not (Test-Path $json)) { throw "사전이 없다: $json" }

$bytes = [IO.File]::ReadAllBytes($dll)
# .NET 메타데이터의 문자열은 UTF-16 이고 파일 기준으로 짝수 위치에 있다는 보장이 없다.
# 두 위치에서 모두 읽어야 전부 걸린다.
$haystack = [Text.Encoding]::Unicode.GetString($bytes) +
            [Text.Encoding]::Unicode.GetString($bytes, 1, $bytes.Length - 1)

$strings = (Get-Content $json -Raw -Encoding UTF8 | ConvertFrom-Json).strings
$missing = @()
$count = 0

foreach ($p in $strings.PSObject.Properties) {
    $count++
    if (-not $haystack.Contains($p.Name)) { $missing += $p.Name }
}

if ($missing.Count -eq 0) {
    "OK  원문 $count 개가 DLL 안에서 보인다 (오타 없음)"
    "    실제로 바뀌는지는 게임 로그의 '창 UI 사전 항목 전부 사용됨' 줄로 확인한다."
} else {
    "!   원문 $count 개 중 $($missing.Count) 개를 DLL 에서 찾지 못했다 (그 항목은 무시된다)"
    $missing | ForEach-Object { "    없음: [$_]" }
    exit 1
}
