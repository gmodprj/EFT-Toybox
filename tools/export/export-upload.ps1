# 만든 zip 두 개를 구글 드라이브(gdrive: 리모트)로 올린다.
#   gdrive:SPT-Mods/2026-11-10/SPT-Mods-2026-11-10.zip
#   gdrive:SPT-Mods/2026-11-10/SPT-Profiles-2026-11-10.zip
# rclone copy 는 올린 뒤 해시를 맞춰 보고, 끝나고 크기를 한 번 더 견준다.
#
# 2026-11-10 실측: 42.4 GiB 한 덩이를 올리는데 **세 번 올라갔다**. 1차·2차가 각각 100% 에 닿은
# 뒤 끝의 커밋 단계에서 떨어지고 3차에 됐다(총 1시간 53분, 실효 6.4 MB/s). 그때 `--retries 3`
# 이었고 **정확히 세 번째에서 됐다** — 한 번 더 떨어졌으면 거기서 끝났다. 그래서 넉넉히 준다.
# 실패 줄은 `--log-level INFO` 에 남지 않는다(low level retry 는 DEBUG). 분모가 늘어나면
# (42.369 -> 84.738 -> 127.107) 검증 패스가 아니라 **재시도**다 — 자세한 것은
# doc/EXPORT-2026-11-10.md.
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
      --retries 10 --low-level-retries 20 `
      --stats 30s --stats-one-line --log-level INFO --stats-log-level NOTICE `
      --log-file (Join-Path $LOGS 'upload.log') --progress
  if ($LASTEXITCODE -ne 0) { throw "rclone copy 실패($LASTEXITCODE): $name — 로그 $LOGS\upload.log" }
}

# 크기만 보지 않고 md5 까지 견준다 — rclone 이 스스로 해시를 맞춰 본다는 말만 믿지 않는다.
# 42 GiB md5 는 약 194초 걸린다.
Write-Host "`n=== 올린 것 확인 (크기 + md5) ==="
$remote = & rclone lsjson $REMOTE --hash | ConvertFrom-Json
$ok = $true
foreach ($z in $zips) {
  $name  = Split-Path -Leaf $z
  $local = (Get-Item -LiteralPath $z).Length
  $r = $remote | Where-Object { $_.Name -eq $name }
  if (-not $r) { Write-Host "없다!!  $name"; $ok = $false; continue }

  $sizeSame = ($r.Size -eq $local)
  $lh = (Get-FileHash -LiteralPath $z -Algorithm MD5).Hash.ToLower()
  $rh = "$($r.Hashes.md5)".ToLower()
  $hashSame = ($rh -and $lh -eq $rh)
  if (-not ($sizeSame -and $hashSame)) { $ok = $false }

  Write-Host ("{0}  {1}" -f ($(if ($sizeSame -and $hashSame) { 'OK    ' } else { '다름!!' })), $name)
  Write-Host ("    크기  로컬 {0:N0} / 원격 {1:N0}" -f $local, $r.Size)
  Write-Host ("    md5   로컬 {0} / 원격 {1}" -f $lh, $(if ($rh) { $rh } else { '(원격 해시 없음)' }))
}
Write-Host ("걸린 시간  : {0:N1} 분" -f ((Get-Date) - $t0).TotalMinutes)
if (-not $ok) { throw '올린 파일이 로컬과 다르다 (크기 또는 md5).' }
Write-Host "업로드 끝 — $REMOTE"
"OK $REMOTE $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" | Set-Content -LiteralPath (Join-Path $LOGS 'upload-done.txt') -Encoding utf8NoBOM
