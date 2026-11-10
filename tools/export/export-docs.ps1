# 스테이징 폴더 안에 설명 문서를 만든다. 복사(export-copy.ps1)가 끝난 뒤에 돌린다.
#   - 저장소 문서(설치 목록·모드 설명·절차·인계)를 _EXPORT-DOC 로 복사
#   - 설치 기록 전체를 _EXPORT-DOC\install-records 로 복사
#   - 스테이징을 실측해 README.md 를 쓴다
$ErrorActionPreference = 'Stop'

$SRC   = 'E:\Spt'
$REPO  = 'E:\Spt_Mods'
$STAMP = '2026-11-10'
$STAGE = "E:\Spt_Mods_Export\SPT-Mods-$STAMP"
$DOC   = Join-Path $STAGE '_EXPORT-DOC'
if (-not (Test-Path $STAGE)) { throw "스테이징이 없다: $STAGE" }
New-Item -ItemType Directory -Force -Path $DOC | Out-Null

# ── 저장소 문서 복사 ──────────────────────────────────────────────────────────
foreach ($d in 'INSTALLED-MODS.md', 'MODS-GUIDE.md', 'PROCEDURE.md', "HANDOVER-$STAMP.md") {
  $p = Join-Path $REPO "doc\$d"
  if (Test-Path $p) { Copy-Item $p (Join-Path $DOC $d) -Force } else { Write-Host "없음: doc\$d" }
}
# 저장소 doc\README.md 는 아래에서 만드는 README.md 와 이름이 겹치므로 다른 이름으로 넣는다
Copy-Item (Join-Path $REPO 'doc\README.md') (Join-Path $DOC 'REPO-DOC-README.md') -Force
& robocopy (Join-Path $REPO 'doc\install-records') (Join-Path $DOC 'install-records') /E /R:2 /W:1 /NFL /NDL /NP /NJH /NJS | Out-Null
if ($LASTEXITCODE -ge 8) { throw "install-records 복사 실패($LASTEXITCODE)" }

# ── 실측 ──────────────────────────────────────────────────────────────────────
function Measure-Tree($path) {
  if (-not (Test-Path -LiteralPath $path)) { return [pscustomobject]@{ Count = 0; MB = 0 } }
  $m = Get-ChildItem -LiteralPath $path -Recurse -File -ErrorAction SilentlyContinue | Measure-Object -Sum Length
  [pscustomobject]@{ Count = $m.Count; MB = [math]::Round($m.Sum / 1MB, 1) }
}

$all   = Measure-Tree $STAGE
$docM  = Measure-Tree $DOC
$parts = [ordered]@{
  'BepInEx\plugins'              = Measure-Tree (Join-Path $STAGE 'BepInEx\plugins')
  'BepInEx\config'               = Measure-Tree (Join-Path $STAGE 'BepInEx\config')
  'BepInEx\patchers'             = Measure-Tree (Join-Path $STAGE 'BepInEx\patchers')
  'SPT_Runtime\user\mods'        = Measure-Tree (Join-Path $STAGE 'SPT_Runtime\user\mods')
  'SPT_Runtime\user\patchers'    = Measure-Tree (Join-Path $STAGE 'SPT_Runtime\user\patchers')
  'CobraSPTExplorerUI'           = Measure-Tree (Join-Path $STAGE 'CobraSPTExplorerUI')
  'EscapeFromTarkov_Data\Managed'= Measure-Tree (Join-Path $STAGE 'EscapeFromTarkov_Data\Managed')
}
$rootFiles = Get-ChildItem -LiteralPath $STAGE -File | Sort-Object Name
$modFiles  = $all.Count - $docM.Count
$modMB     = $all.MB - $docM.MB

# 설치 기록 요약
$recFiles = Get-ChildItem (Join-Path $REPO 'doc\install-records') -Filter 'installed-*.txt'
$recLines = 0; $mods = [System.Collections.Generic.HashSet[string]]::new()
foreach ($f in $recFiles) {
  foreach ($l in [IO.File]::ReadAllLines($f.FullName)) {
    $i = $l.IndexOf("`t"); if ($i -lt 0) { continue }
    $recLines++; [void]$mods.Add($l.Substring(0, $i))
  }
}

# 프로필
$profDir = Join-Path $SRC 'SPT_Runtime\user\profiles'
$profM = Measure-Tree $profDir
$profRows = foreach ($f in Get-ChildItem $profDir -Filter *.json | Sort-Object Name) {
  $nick = '(읽지 못함)'; $lvl = ''
  try {
    $j = Get-Content -LiteralPath $f.FullName -Raw | ConvertFrom-Json -AsHashtable
    $info = $j['characters']['pmc']['Info']
    if ($info) { $nick = $info['Nickname']; $lvl = $info['Level'] }
  } catch { }
  '| `{0}` | {1} | {2} | {3:N1} MB |' -f $f.BaseName, $nick, $lvl, ($f.Length / 1MB)
}

# ── README ───────────────────────────────────────────────────────────────────
$tableParts = foreach ($k in $parts.Keys) {
  if ($parts[$k].Count -eq 0) { continue }
  '| `{0}` | {1:N0} | {2:N1} MB |' -f $k, $parts[$k].Count, $parts[$k].MB
}
$rootList = foreach ($f in $rootFiles) { '| `{0}` | {1:N0} B |' -f $f.Name, $f.Length }

$md = @"
# SPT 모드 추출본 — $STAMP

``E:\Spt`` 에 설치되어 돌아가고 있는 모드와 그 설정을 그대로 떠낸 것이다. 폴더 구조는
``E:\Spt`` 를 그대로 따르므로, **이 폴더 안을 ``E:\Spt`` 에 덮어쓰면 같은 모드 구성이 된다**
(``_EXPORT-DOC`` 은 문서일 뿐이니 덮어쓸 때 빼면 된다).

- 추출 시각: $(Get-Date -Format 'yyyy-MM-dd HH:mm') (KST)
- 원본: ``$SRC`` — SPT 4.1.6 / EFT 0.16.9.5.40743
- 설치 모드: **$($mods.Count)개**, 설치 기록 $("{0:N0}" -f $recLines)줄 (기록 파일 $($recFiles.Count)개)
- 추출 파일: **$("{0:N0}" -f $modFiles)개 / $("{0:N1}" -f $modMB) MB** (문서 $($docM.Count)개 제외)
- 실측 로드 수: 서버 모드 **174개**(2026-11-09) / 게임 플러그인 **246개**(2026-11-10)
- 프로필은 이 압축에 **없다**. 같은 날짜의 ``SPT-Profiles-$STAMP.zip`` 에 따로 있다.

## 무엇이 들어 있나

| 경로 | 파일 | 용량 |
| --- | --- | --- |
$($tableParts -join "`n")

루트 파일(모드 실행 파일과 그 설정):

| 파일 | 크기 |
| --- | --- |
$($rootList -join "`n")

### 담은 기준

- ``BepInEx\plugins`` · ``BepInEx\config`` · ``BepInEx\patchers`` · ``SPT_Runtime\user\mods`` ·
  ``SPT_Runtime\user\patchers`` 는 **폴더 전체**를 떴다. 설치 후 게임·서버가 만든 설정 파일
  (``BepInEx\config\*.cfg``, 모드별 ``config\*.json``)까지 그대로 들어 있다.
- 위 폴더 밖은 **설치 기록**(``_EXPORT-DOC\install-records``)에 적힌 경로만 떴다. 루트의
  모드 실행 파일(``AirdropCaller.exe`` 등)과 그 ``*.config.json`` 은 기록에 없어 이름으로 지정해 넣었다.
- ``EscapeFromTarkov_Data\Managed`` 의 DLL 2개는 모드가 넣은 것이다(``Unity.VectorGraphics`` ·
  ``Unity.InternalAPIEngineBridge.003``). 게임 본체 파일은 이것뿐이다.
- 설치 기록에 적혀 있어 함께 들어온 ``LICENSE`` · ``README.md`` · ``CHANGELOG.md`` ·
  ``THIRD-PARTY-NOTICES.md`` 는 SPT 본체 문서다. 설치 후 스캔이 그때 설치 중이던 모드 몫으로
  적은 것으로, 모드 파일이 아니다(덮어써도 무해하다).

### 뺀 것

- 프로필(``SPT_Runtime\user\profiles``) — 별도 압축
- 로그·캐시: ``BepInEx\LogOutput.log`` · ``BepInEx\cache`` · ``Logs`` · ``NLog`` ·
  ``*.log`` · ``SPT_Runtime\user\logs`` · ``SPT_Runtime\user\cache``
- ``BepInEx\ORBIT`` (ORBIT 모드가 남긴 지난 레이드 덤프 33 MB — 설정이 아니다)
- ``BepInEx\BetterBepInEx\Screenshots`` (스크린샷)
- 게임 본체와 SPT 본체(``EscapeFromTarkov.exe`` · ``BepInEx\core`` · ``SPT_Runtime`` 의 DLL 등).
  **이 추출본만으로는 게임이 돌지 않는다.** SPT 4.1.6 설치본 위에 얹어야 한다.

## 문서

| 파일 | 무엇 |
| --- | --- |
| ``INSTALLED-MODS.md`` | 설치된 모드 $($mods.Count)개 전부. 버전·위치·결속 여부·파일 수·기록 이름. 설치 기록에서 자동 생성한 것이라 **지금 설치본과 맞다**. |
| ``MODS-GUIDE.md`` | 모드가 무엇을 하는지 기능별로 묶어 쓴 설명. **2026-10-25 기준이라 Forge 모드 145개 + 3개 + 자작 2개까지만 적혀 있다** — 그 뒤 10차에서 들어온 것들은 ``INSTALLED-MODS.md`` 쪽에만 있다. |
| ``PROCEDURE.md`` | 모드를 넣고 확인하는 절차. |
| ``HANDOVER-$STAMP.md`` | 이 날짜까지의 작업 경과. |
| ``install-records\`` | 모드별 설치 파일 목록 원본($($recFiles.Count)개 파일). 복원 뒤 ``node tools/check-install-records.mjs`` 로 맞는지 볼 수 있다. |
| ``REPO-DOC-README.md`` | 작업 저장소(``E:\Spt_Mods``) 문서 전체의 안내. 어떤 문서에 무엇이 있는지. |
| ``FILE-MANIFEST.txt`` | 이 추출본의 모든 파일 — ``상대경로<TAB>크기<TAB>수정시각``. |

## 되돌리는 방법

1. SPT 4.1.6 을 설치한다(모드 없는 상태).
2. 이 폴더에서 ``_EXPORT-DOC`` 을 뺀 나머지를 SPT 폴더에 덮어쓴다.
3. ``SPT-Profiles-$STAMP.zip`` 을 풀어 ``SPT_Runtime\user\profiles`` 에 넣는다.
4. 서버를 띄워 ``ModLoader: 총 174 개`` 와 게임 쪽 ``246 plugins to load`` 가 나오는지 본다.

> 결속(프로필 결속) 모드가 많다. 모드를 빼면 프로필이 깨질 수 있으니, 되돌릴 때는
> 모드 구성과 프로필을 **같은 날짜의 것끼리** 짝지어 쓴다.

## 프로필 ($($profM.Count)개 파일 / $("{0:N1}" -f $profM.MB) MB — ``SPT-Profiles-$STAMP.zip``)

| 프로필 | 이름 | 레벨 | 크기 |
| --- | --- | --- | --- |
$($profRows -join "`n")
"@

$md | Set-Content -LiteralPath (Join-Path $DOC 'README.md') -Encoding utf8NoBOM
"문서 작성: $DOC"
Get-ChildItem $DOC | ForEach-Object { "  $($_.Name)" }
