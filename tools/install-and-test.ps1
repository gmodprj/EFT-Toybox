# 압축 해제된 모드 하나를 E:\Spt에 설치(덮어쓰기 금지)하고 기록한 뒤, 테스트 서버로 로드를 확인한다.
param(
  [Parameter(Mandatory)] [string] $Pattern,   # extracted\stage2 아래 폴더 패턴 (예: '2310__*')
  [Parameter(Mandatory)] [string] $Tag,       # 기록 파일 이름용 (예: 'stage2-B1-CommonLib')
  [int] $WaitSeconds = 360,
  [string] $Stage = 'stage2',                 # extracted 아래 단계 폴더
  [switch] $SkipTest                          # 설치만 하고 서버 테스트 생략
)
$ErrorActionPreference = 'Stop'
$ex  = "E:\Spt_Mods\extracted\$Stage"
$rec = "E:\Spt_Mods\doc\install-records\installed-$Tag.txt"
$out = "E:\Spt_Mods\doc\server-tests\server-test-$Tag.out.txt"
$err = "E:\Spt_Mods\doc\server-tests\server-test-$Tag.err.txt"

if (Get-Process | Where-Object { $_.Name -match '^(SPT\.Server|EscapeFromTarkov)$' }) { throw '서버나 게임이 실행 중이라 중단' }

# 1) 설치: 먼저 전체 충돌 검사 후 복사
$d = Get-ChildItem $ex -Directory -Filter $Pattern | Select-Object -First 1
if (-not $d) { throw "폴더 없음: $Pattern" }
$files = Get-ChildItem -LiteralPath $d.FullName -Recurse -File
$pairs = $files | ForEach-Object { $rel = $_.FullName.Substring($d.FullName.Length + 1); [pscustomobject]@{ Src = $_.FullName; Rel = $rel; Dst = Join-Path 'E:\Spt' $rel } }
$clash = $pairs | Where-Object { Test-Path -LiteralPath $_.Dst }
if ($clash) { throw "이미 존재하는 파일 $($clash.Count)개: $($clash[0].Dst)" }
$t0 = Get-Date
foreach ($p in $pairs) {
  $dir = Split-Path $p.Dst
  if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
  Copy-Item -LiteralPath $p.Src -Destination $p.Dst
}
$pairs | ForEach-Object { "$($d.Name)`t$($_.Rel)" } | Set-Content -Encoding utf8 $rec
# 압축에 들어 있던 빈 폴더도 만든다(파일만 복사하면 모드가 찾는 폴더가 없어 서버가 죽는 경우가 있다)
$emptyDirs = Get-ChildItem -LiteralPath $d.FullName -Recurse -Directory |
  Where-Object { -not (Get-ChildItem -LiteralPath $_.FullName -Recurse -File) } |
  ForEach-Object { $_.FullName.Substring($d.FullName.Length + 1) }
if ($emptyDirs) {
  foreach ($rel in $emptyDirs) {
    $dst = Join-Path 'E:\Spt' $rel
    if (-not (Test-Path -LiteralPath $dst)) { New-Item -ItemType Directory -Force -Path $dst | Out-Null }
    "EMPTYDIR $dst"
  }
  $emptyDirs | ForEach-Object { "$($d.Name)`t$_" } | Set-Content -Encoding utf8 ($rec -replace '\.txt$', '.dirs.txt')
}
$mb = [math]::Round(($files | Measure-Object Length -Sum).Sum / 1MB, 1)
"INSTALL $($d.Name): $($pairs.Count) files, $mb MB, $([int]((Get-Date) - $t0).TotalSeconds)s"
if ($SkipTest) { return }

# 2) 테스트 서버: 시작 완료(인증서 로드) 또는 실패까지 대기 후 종료
$before = Get-Date
$p = Start-Process -FilePath 'E:\Spt\SPT_Runtime\SPT.Server.exe' -WorkingDirectory 'E:\Spt\SPT_Runtime' `
  -RedirectStandardOutput $out -RedirectStandardError $err -PassThru -WindowStyle Hidden
$deadline = (Get-Date).AddSeconds($WaitSeconds); $state = 'TIMEOUT'
while ((Get-Date) -lt $deadline) {
  if ($p.HasExited) { $state = "EXITED($($p.ExitCode))"; break }
  $t = Get-Content $out -Raw -ErrorAction SilentlyContinue
  if ($t -match 'Loaded self-signed certificate') { $state = 'STARTED'; break }
  if ($t -match 'Failed to start|Press any key') { $state = 'FAILED'; break }
  Start-Sleep 3
}
Start-Sleep 2
if (-not $p.HasExited) { Stop-Process -Id $p.Id -Confirm:$false }
"SERVER $state in $([int]((Get-Date) - $before).TotalSeconds)s"

$t = (Get-Content $out -Raw) -replace "\x1b\[[0-9;]*m", ''
$t -split "`n" | Where-Object { $_ -match 'ModLoader|WTT|CommonLib|Armory|Artem|Lotus|Scorpion|Ref|Harry|give|Skipper|error|오류|warn|경고|fail|exception|Exception' } | Select-Object -First 40
$e = Get-Content $err -Raw -ErrorAction SilentlyContinue; if ($e) { "=== stderr"; $e.Substring(0, [math]::Min(3000, $e.Length)) }

# 3) 서버 실행 중 새로 생긴 파일 (되돌리기 목록에 추가)
# 이 모드가 깐 폴더(루트) 목록. 다른 모드가 서버 시작 때 다시 만든 파일을 이 모드 것으로
# 적으면 기록이 겹친다(SIM 의 current.snapshot.json 이 실제로 그랬다).
$roots = $pairs | ForEach-Object {
  if ($_.Rel -match '^(SPT_Runtime\\user\\mods\\[^\\]+|BepInEx\\plugins\\[^\\]+)\\') { $Matches[1] }
} | Sort-Object -Unique
$new = Get-ChildItem -LiteralPath 'E:\Spt\SPT_Runtime\user\mods' -Recurse -File | Where-Object { $_.CreationTime -gt $before }
foreach ($n in $new) {
  $rel = $n.FullName.Substring('E:\Spt\'.Length)
  if ($roots | Where-Object { $rel.StartsWith("$_\") }) {
    "$($d.Name)`t$rel" | Add-Content -Encoding utf8 $rec
    "NEWFILE $($n.FullName)"
  } else {
    "OTHERMOD $rel (다른 모드가 만든 파일 — 기록하지 않음)"
  }
}
