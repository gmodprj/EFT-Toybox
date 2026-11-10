# E:\Spt 에 설치된 모드와 그 설정을 스테이징 폴더로 통째로 옮긴다.
#   - 벌크 폴더(plugins / config / patchers / user\mods / user\patchers)는 robocopy 로 전체 복사
#   - 설치 기록(doc/install-records)에 있는데 벌크 밖인 경로는 개별 복사
#   - 루트에 있는 모드 실행 파일·설정 파일은 이름으로 지정해 복사
#   - 프로필은 **넣지 않는다**(별도 압축)
# 쓰기: 스테이징 폴더, 로그, 파일 매니페스트
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$SRC   = 'E:\Spt'
$REPO  = 'E:\Spt_Mods'
$STAMP = '2026-11-10'
$OUT   = 'E:\Spt_Mods_Export'
$STAGE = Join-Path $OUT "SPT-Mods-$STAMP"
$DOC   = Join-Path $STAGE '_EXPORT-DOC'
$LOGS  = Join-Path $OUT "logs-$STAMP"

New-Item -ItemType Directory -Force -Path $STAGE, $DOC, $LOGS | Out-Null
$t0 = Get-Date

# ── 1. 벌크 폴더 ───────────────────────────────────────────────────────────────
$bulk = @(
  'BepInEx\plugins'
  'BepInEx\config'
  'BepInEx\patchers'
  'SPT_Runtime\user\mods'
  'SPT_Runtime\user\patchers'
)
foreach ($rel in $bulk) {
  $from = Join-Path $SRC $rel
  if (-not (Test-Path -LiteralPath $from)) { Write-Host "건너뜀(없음): $rel"; continue }
  $to  = Join-Path $STAGE $rel
  $log = Join-Path $LOGS ("robocopy-" + ($rel -replace '[\\]','_') + '.log')
  Write-Host "복사: $rel"
  & robocopy $from $to /E /COPY:DAT /DCOPY:DAT /MT:16 /R:2 /W:1 /NFL /NDL /NP /NJH /NJS /LOG:$log | Out-Null
  if ($LASTEXITCODE -ge 8) { throw "robocopy 실패($LASTEXITCODE): $rel — 로그 $log" }
}

# ── 2. 설치 기록 중 벌크 밖 경로 ───────────────────────────────────────────────
$recSet = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
foreach ($f in Get-ChildItem (Join-Path $REPO 'doc\install-records') -Filter 'installed-*.txt') {
  foreach ($l in [IO.File]::ReadAllLines($f.FullName)) {
    $i = $l.IndexOf("`t")
    if ($i -lt 0) { continue }
    $rel = $l.Substring($i + 1).Trim()
    if ($rel) { [void]$recSet.Add($rel) }
  }
}
$bulkPrefix = $bulk | ForEach-Object { $_ + '\' }
$extra = $recSet | Where-Object { $r = $_; -not ($bulkPrefix | Where-Object { $r.StartsWith($_, 'OrdinalIgnoreCase') }) }

# ── 3. 루트 모드 실행 파일·설정 파일 (기록에 안 잡힌 것) ──────────────────────
$rootMods = @(
  'AirdropCaller.exe', 'airdrop-caller.config.json'
  'Give Ui.exe',       'give-ui.config.json'
  'ProfilePorter.exe', 'profile-porter.config.json'
)

$missed = New-Object System.Collections.Generic.List[string]
$copied = 0
foreach ($rel in (@($extra) + $rootMods | Sort-Object -Unique)) {
  $from = Join-Path $SRC $rel
  if (-not (Test-Path -LiteralPath $from)) { $missed.Add($rel); continue }
  $to = Join-Path $STAGE $rel
  $item = Get-Item -LiteralPath $from
  if ($item.PSIsContainer) {
    & robocopy $from $to /E /COPY:DAT /DCOPY:DAT /R:2 /W:1 /NFL /NDL /NP /NJH /NJS | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "robocopy 실패($LASTEXITCODE): $rel" }
    $copied += (Get-ChildItem $to -Recurse -File).Count
  } else {
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $to) | Out-Null
    Copy-Item -LiteralPath $from -Destination $to -Force
    $copied++
  }
}

# ── 4. 매니페스트와 요약 ───────────────────────────────────────────────────────
$staged = Get-ChildItem $STAGE -Recurse -File | Where-Object { $_.FullName -notlike "$DOC*" }
$cut = $STAGE.Length + 1
$manifest = Join-Path $DOC 'FILE-MANIFEST.txt'
$sw = [IO.StreamWriter]::new($manifest, $false, [Text.UTF8Encoding]::new($false))
foreach ($f in $staged) {
  $sw.WriteLine(("{0}`t{1}`t{2}" -f $f.FullName.Substring($cut), $f.Length, $f.LastWriteTime.ToString('yyyy-MM-dd HH:mm:ss')))
}
$sw.Close()

$sum = ($staged | Measure-Object -Sum Length).Sum
$lines = @(
  "대상       : $SRC"
  "스테이징   : $STAGE"
  "기록 경로  : $($recSet.Count) (벌크 밖 $(@($extra).Count))"
  "복사 파일  : $($staged.Count)"
  "용량       : {0:N1} MB" -f ($sum / 1MB)
  "걸린 시간  : {0:N1} 분" -f ((Get-Date) - $t0).TotalMinutes
  "빠진 항목  : $($missed.Count)"
)
foreach ($m in $missed) { $lines += "  - $m" }
$lines -join "`r`n" | Tee-Object -FilePath (Join-Path $LOGS 'copy-log.txt')
