# _ui.json 의 원문이 ConfigurationManager.dll 안에 실제로 그대로 있는지 확인한다.
# 한 글자라도 다르면 그 항목은 조용히 무시되므로, 사전을 고칠 때마다 돌려 본다.
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
    "OK  원문 $count 개 모두 DLL 에서 확인됨"
} else {
    "!   원문 $count 개 중 $($missing.Count) 개를 DLL 에서 찾지 못했다 (그 항목은 무시된다)"
    $missing | ForEach-Object { "    없음: [$_]" }
    exit 1
}
