# STM 의 암호화 문서(LiteDB + AES-GCM)를 읽기 전용으로 들여다본다. 기본은 전송 저널이다.
#
# 쓰기를 하지 않는다 — .db 를 임시 폴더로 **복사해서** 연다(서버가 돌고 있어도, 파일이 잠겨 있어도 된다).
# 열쇠 재료·AAD 규칙은 src/SptTeamManager/server/Persistence/TeammateDatabase.cs 와 같아야 한다.
#
# 보기:
#   pwsh -File tools/dump-stm-journal.ps1                       # 모든 프로필의 전송 저널
#   pwsh -File tools/dump-stm-journal.ps1 -List                 # 각 db 가 든 문서 이름만
#   pwsh -File tools/dump-stm-journal.ps1 -Document stm-raid-receipts.json
[CmdletBinding()]
param(
    [string] $Root = 'E:\Spt\SPT_Runtime\user\mods\SptTeamManager-ServerMod\Resources\stm-teammates',
    [string] $LiteDb = 'E:\Spt\SPT_Runtime\user\mods\SptTeamManager-ServerMod\LiteDB.dll',
    [string] $Document = 'stm-transfer-journal.json',
    [string] $ProfileId,
    [switch] $List,
    [switch] $Raw
)

$ErrorActionPreference = 'Stop'
# TeammateDatabase.KeyMaterial (v1 — 빌드·플랫폼이 바뀌어도 그대로여야 한다)
$KeyMaterial = 'pitFireTeam/followers/v1/90aa379ef47b4d36b257ed00c8fc3ebf/8ca24bf176e542cab4ed96bd9b623345'

if (-not (Test-Path $LiteDb)) { throw "LiteDB.dll 이 없다: $LiteDb" }
if (-not (Test-Path $Root)) { throw "문서 폴더가 없다: $Root" }
Add-Type -Path $LiteDb

function Unprotect-Document {
    param([byte[]] $Payload, [byte[]] $Key, [string] $ProfileId, [string] $Name)
    if ($Payload.Length -lt 29 -or $Payload[0] -ne 1) { throw "v1 형식이 아니다 ($Name)" }
    $nonce = $Payload[1..12]
    $tag = $Payload[13..28]
    $cipher = New-Object byte[] ($Payload.Length - 29)
    [Array]::Copy($Payload, 29, $cipher, 0, $cipher.Length)
    $plain = New-Object byte[] $cipher.Length
    $aad = [Text.Encoding]::UTF8.GetBytes("$ProfileId/$Name")
    $aes = [System.Security.Cryptography.AesGcm]::new($Key, 16)
    try { $aes.Decrypt($nonce, $cipher, $tag, $plain, $aad) } finally { $aes.Dispose() }
    [Text.Encoding]::UTF8.GetString($plain)
}

$temp = Join-Path ([IO.Path]::GetTempPath()) ("stm-journal-" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temp | Out-Null
try {
    $files = Get-ChildItem -Path $Root -Filter '*.db' | Sort-Object Name
    if ($ProfileId) { $files = $files | Where-Object BaseName -eq $ProfileId.ToLowerInvariant() }
    if (-not $files) { throw "db 파일이 없다 (ProfileId=$ProfileId)" }

    foreach ($file in $files) {
        $pid2 = $file.BaseName.ToLowerInvariant()
        $copy = Join-Path $temp $file.Name
        Copy-Item -LiteralPath $file.FullName -Destination $copy   # 원본은 열지 않는다
        $key = [System.Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes("$KeyMaterial/$pid2"))

        $db = [LiteDB.LiteDatabase]::new("Filename=$copy;Connection=direct")
        try {
            $docs = $db.GetCollection('documents').FindAll()
            $names = @($docs | ForEach-Object { $_['_id'].AsString } | Sort-Object)
            if ($List) {
                Write-Host "== $pid2  (문서 $($names.Count)개)"
                $names | ForEach-Object { Write-Host "   $_" }
                continue
            }
            $hit = $docs | Where-Object { $_['_id'].AsString -eq $Document }
            if (-not $hit) { Write-Host "== $pid2  — '$Document' 없음 (문서 $($names.Count)개)"; continue }
            $json = Unprotect-Document -Payload $hit['payload'].AsBinary -Key $key -ProfileId $pid2 -Name $Document
            Write-Host "== $pid2  — $Document"
            if ($Raw) { Write-Host $json; continue }
            $value = $json | ConvertFrom-Json
            $entries = @(if ($null -eq $value) { } elseif ($value -is [array]) { $value } elseif ($null -ne $value.entries) { $value.entries } else { $value })
            Write-Host "   항목 $($entries.Count)개"
            foreach ($e in $entries) {
                $ids = @($e.ItemIds); $dst = @($e.DestinationItemIds)
                Write-Host ("   [{0}] {1}  {2} → {3}  아이템 {4}개{5}  출처쓰기={6} 목적지쓰기={7}" -f `
                    $e.Stage, $e.Kind, $e.SourceProfileId, $e.DestinationProfileId, $ids.Count,
                    ($(if ($dst.Count) { " (기록된 목적지 id $($dst.Count)개)" } else { '' })),
                    $e.SourceWriteStarted, $e.DestinationWriteStarted)
            }
        } finally { $db.Dispose() }
    }
} finally { Remove-Item -Recurse -Force $temp -ErrorAction SilentlyContinue }
