# 모드 폴더 안에 한국어 locale 파일을 넣는다. 목록은 src\PluginLocales\manifest.json 이다.
#
# 사용:
#   pwsh -File tools/install-plugin-locales.ps1 [-SptDir E:\Spt] [-Remove] [-WhatIf]
#
# 모드 폴더 안이라 **모드를 업데이트하면 날아간다.** 그때 이 스크립트를 다시 돌린다.
# 덮어쓰기는 우리가 넣은 kr.json 뿐이고, 모드의 원본 파일(en.json 등)은 건드리지 않는다.
param(
    [string]$SptDir = 'E:\Spt',
    [switch]$Remove,
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'

$src = Join-Path $PSScriptRoot '..\src\PluginLocales' | Resolve-Path
$plugins = Join-Path $SptDir 'BepInEx\plugins'

if (-not (Test-Path $plugins)) { throw "플러그인 폴더가 없다: $plugins" }

$manifest = Get-Content (Join-Path $src 'manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json

# 게임이 켜져 있으면 중단한다 — 플러그인은 켤 때 locale 을 읽으므로 반쯤 바뀐 상태가 된다.
if (-not $Remove -and (Get-Process -Name 'EscapeFromTarkov' -ErrorAction SilentlyContinue)) {
    throw '게임이 실행 중이라 중단. 끄고 다시 돌린다.'
}

$done = 0
$skipped = 0

foreach ($t in $manifest.targets) {
    $from = Join-Path $src $t.source
    $to = Join-Path $plugins ($t.dest -replace '/', '\')
    $ref = Join-Path $plugins ($t.reference -replace '/', '\')

    if ($Remove) {
        if (Test-Path $to) {
            if ($WhatIf) { "[지울 것] $to" } else { Remove-Item $to -Force; "제거: $($t.name) — $to" }
            $done++
        } else {
            "없음: $($t.name)"
            $skipped++
        }
        continue
    }

    if (-not (Test-Path $from)) { throw "사전이 없다: $from" }

    # 원본 en.json 이 있어야 그 모드가 거기 있다는 뜻이다. 없으면 모드를 뺐거나 경로가 바뀌었다.
    if (-not (Test-Path $ref)) {
        "!  건너뜀: $($t.name) — 원본이 없다($ref). 모드를 뺐거나 경로가 바뀌었다."
        $skipped++
        continue
    }

    $destDir = Split-Path $to -Parent
    if ($WhatIf) {
        "[넣을 것] $from`n          → $to"
    } else {
        New-Item -ItemType Directory -Force -Path $destDir | Out-Null
        Copy-Item $from -Destination $to -Force
        $size = (Get-Item $to).Length
        "넣음: {0,-22} {1,7} bytes  → {2}" -f $t.name, $size, $t.dest
    }
    $done++
}

''
if ($Remove) { "제거 $done 건 / 없던 것 $skipped 건" } else { "설치 $done 건 / 건너뜀 $skipped 건" }
'게임 플러그인이라 게임을 다시 켜야 적용된다.'
'사전이 원본과 맞는지는 "node tools/check-plugin-locales.mjs" 로 확인한다.'
