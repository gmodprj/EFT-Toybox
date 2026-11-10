# 만든 zip 두 개를 구글 드라이브(gdrive: 리모트)로 올린다.
#   gdrive:SPT-Mods/2026-11-10/SPT-Mods-2026-11-10.zip
#   gdrive:SPT-Mods/2026-11-10/SPT-Profiles-2026-11-10.zip
# rclone copy 는 올린 뒤 해시를 맞춰 보고, 끝나고 크기를 한 번 더 견준다.
$ErrorActionPreference = 'Stop'

$STAMP  = '2026-11-10'
$OUT    = 'E:\Spt_Mods_Export'
$LOGS   = Join-Path $OUT "logs-$STAMP"
$REMOTE = "gdrive:SPT-Mods/$STAMP"
New-Item -ItemType Directory -Force -Path $LOGS | Out-Null

$zips = @("SPT-Mods-$STAMP.zip", "SPT-Profiles-$STAMP.zip") | ForEach-Object { Join-Path $OUT $_ }
foreach ($z in $zips) { if (-not (Test-Path -LiteralPath $z)) { throw "zip 이 없다: $z" } }

$t0 = Get-Date
foreach ($z in $zips) {
  $name = Split-Path -Leaf $z
  Write-Host ("올린다: {0} ({1:N1} GB)" -f $name, ((Get-Item $z).Length / 1GB))
  & rclone copy $z $REMOTE `
      --drive-chunk-size 128M --transfers 1 --checkers 4 `
      --stats 30s --stats-one-line --log-level INFO `
      --log-file (Join-Path $LOGS 'upload.log') --progress
  if ($LASTEXITCODE -ne 0) { throw "rclone copy 실패($LASTEXITCODE): $name — 로그 $LOGS\upload.log" }
}

Write-Host "`n=== 올린 것 확인 ==="
$remote = & rclone lsjson $REMOTE | ConvertFrom-Json
$ok = $true
foreach ($z in $zips) {
  $name = Split-Path -Leaf $z
  $local = (Get-Item -LiteralPath $z).Length
  $r = $remote | Where-Object { $_.Name -eq $name }
  if (-not $r) { Write-Host "없다!!  $name"; $ok = $false; continue }
  $same = ($r.Size -eq $local)
  if (-not $same) { $ok = $false }
  Write-Host ("{0}  {1}  로컬 {2:N0} / 원격 {3:N0}" -f ($(if ($same) { 'OK  ' } else { '다름!!' })), $name, $local, $r.Size)
}
Write-Host ("걸린 시간  : {0:N1} 분" -f ((Get-Date) - $t0).TotalMinutes)
if (-not $ok) { throw '올린 파일 크기가 로컬과 다르다.' }
Write-Host "업로드 끝 — $REMOTE"
"OK $REMOTE $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" | Set-Content -LiteralPath (Join-Path $LOGS 'upload-done.txt') -Encoding utf8NoBOM
