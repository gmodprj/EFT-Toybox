# 스테이징 폴더와 프로필을 각각 zip 으로 묶는다.
#   SPT-Mods-2026-11-10.zip      모드 + 설정 + 문서
#   SPT-Profiles-2026-11-10.zip  프로필 (backups 포함)
# 서버·게임이 떠 있는 채로 떠내므로, 프로필은 **JSON 으로 파싱되는지 확인하고** 깨지면 다시 뜬다.
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$SRC   = 'E:\Spt'
$STAMP = '2026-11-10'
$OUT   = 'E:\Spt_Mods_Export'
$STAGE = Join-Path $OUT "SPT-Mods-$STAMP"
$LOGS  = Join-Path $OUT "logs-$STAMP"
New-Item -ItemType Directory -Force -Path $LOGS | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem

function New-Zip {
  param([string]$Root, [string]$ZipPath, [string]$Prefix)
  if (Test-Path -LiteralPath $ZipPath) { Remove-Item -LiteralPath $ZipPath -Force }
  $files = Get-ChildItem -LiteralPath $Root -Recurse -File
  $total = ($files | Measure-Object -Sum Length).Sum
  Write-Host ("{0} <- {1} 파일 / {2:N1} MB" -f (Split-Path -Leaf $ZipPath), $files.Count, ($total / 1MB))
  $cut = $Root.Length + 1
  $zip = [IO.Compression.ZipFile]::Open($ZipPath, 'Create')
  $n = 0; $done = 0; $t0 = Get-Date
  try {
    foreach ($f in $files) {
      $entry = $Prefix + ($f.FullName.Substring($cut) -replace '\\', '/')
      [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $f.FullName, $entry, [IO.Compression.CompressionLevel]::Fastest) | Out-Null
      $n++; $done += $f.Length
      if ($n % 500 -eq 0) {
        $el = ((Get-Date) - $t0).TotalSeconds
        Write-Host ("  {0}/{1} 파일, {2:N1}/{3:N1} GB, {4:N1} MB/s" -f $n, $files.Count, ($done / 1GB), ($total / 1GB), (($done / 1MB) / [math]::Max($el, 1)))
      }
    }
  } finally { $zip.Dispose() }
  $z = Get-Item -LiteralPath $ZipPath
  $msg = "zip        : {0}`r`n원본       : {1:N1} MB`r`n압축       : {2:N1} MB ({3:N1}%)`r`n걸린 시간  : {4:N1} 분`r`nzip 항목   : {5}" -f `
    $z.FullName, ($total / 1MB), ($z.Length / 1MB), ($z.Length / $total * 100), ((Get-Date) - $t0).TotalMinutes, $n
  Write-Host $msg
  return $msg
}

# ── 1. 프로필 스냅샷 (파싱 확인 + 재시도) ────────────────────────────────────
$pSrc = Join-Path $SRC 'SPT_Runtime\user\profiles'
$pTmp = Join-Path $OUT "_tmp-profiles-$STAMP"
if (Test-Path $pTmp) { Remove-Item $pTmp -Recurse -Force }
New-Item -ItemType Directory -Force -Path (Join-Path $pTmp 'profiles') | Out-Null

$bad = New-Object System.Collections.Generic.List[string]
for ($try = 1; $try -le 3; $try++) {
  & robocopy $pSrc (Join-Path $pTmp 'profiles') /E /COPY:DAT /DCOPY:DAT /R:3 /W:2 /NFL /NDL /NP /NJH /NJS | Out-Null
  if ($LASTEXITCODE -ge 8) { throw "프로필 복사 실패($LASTEXITCODE)" }
  $bad.Clear()
  foreach ($j in Get-ChildItem (Join-Path $pTmp 'profiles') -Recurse -Filter *.json) {
    try { [void][System.Text.Json.JsonDocument]::Parse([IO.File]::ReadAllText($j.FullName)) }
    catch { $bad.Add($j.FullName.Substring($pTmp.Length + 1)) }
  }
  if ($bad.Count -eq 0) { Write-Host "프로필 JSON 확인: 전부 정상 (시도 $try)"; break }
  Write-Host "프로필 JSON 깨짐 $($bad.Count)개 — 다시 뜬다 (시도 $try)"
  $bad | ForEach-Object { Write-Host "    $_" }
  Start-Sleep -Seconds 5
}
if ($bad.Count -gt 0) { throw "프로필 $($bad.Count)개가 세 번 다 깨진 채로 읽혔다. 서버가 쓰는 중이다 — 멈추고 사용자에게 알린다." }

$m2 = New-Zip -Root $pTmp -ZipPath (Join-Path $OUT "SPT-Profiles-$STAMP.zip") -Prefix ''
Remove-Item $pTmp -Recurse -Force

# ── 2. 모드 zip ──────────────────────────────────────────────────────────────
$m1 = New-Zip -Root $STAGE -ZipPath (Join-Path $OUT "SPT-Mods-$STAMP.zip") -Prefix "SPT-Mods-$STAMP/"

"$m1`r`n`r`n$m2" | Set-Content -LiteralPath (Join-Path $LOGS 'zip-log.txt') -Encoding utf8NoBOM
"끝"
