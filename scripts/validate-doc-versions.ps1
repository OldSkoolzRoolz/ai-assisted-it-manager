Param(
    [string]$ManifestPath = 'docs/DOCUMENTATION_VERSION_MANIFEST.md',
    [switch]$Json,
    [string]$ReportPath = 'artifacts/doc-version-validation.json'
)

$ErrorActionPreference = 'Stop'

$failures = @()
$checked = 0
$status = 'Unknown'

function Emit-Result {
    param($Success, $Failures, $Checked, $Status)
    if ($Json) {
        $result = [pscustomobject]@{
            success = $Success
            checked = $Checked
            failures = $Failures
            status = $Status
            manifest = $ManifestPath
            timestampUtc = [DateTime]::UtcNow.ToString('o')
        }
        $jsonText = $result | ConvertTo-Json -Depth 4
        if (-not [string]::IsNullOrWhiteSpace($ReportPath)) {
            $dir = Split-Path -Parent $ReportPath
            if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null }
            $jsonText | Out-File -FilePath $ReportPath -Encoding UTF8
        }
        Write-Output $jsonText
    } else {
        if ($Success) {
            Write-Host "Documentation version validation passed. ($Checked documents checked)" -ForegroundColor Green
        } else {
            Write-Error ("Documentation version validation failed:`n" + ($Failures -join "`n"))
        }
    }
    if ($Success) { exit 0 } else { exit 2 }
}

if (-not (Test-Path $ManifestPath)) {
    $failures += "Manifest not found at $ManifestPath"
    Emit-Result -Success:$false -Failures $failures -Checked 0 -Status 'ManifestMissing'
}

$manifest = Get-Content $ManifestPath -Raw -ErrorAction Stop

# Extract inventory table block
$inventoryPattern = '(?s)## Current Documentation Inventory\s+\n(.*?)\n---'
if ($manifest -match $inventoryPattern) {
    $tableBlock = $Matches[1]
} else {
    $failures += 'Could not locate documentation inventory table.'
    Emit-Result -Success:$false -Failures $failures -Checked 0 -Status 'InventoryNotFound'
}

$rows = $tableBlock -split "\r?\n" | Where-Object { $_ -match '^\| `?.+\| .*' }

foreach ($row in $rows) {
    if ($row -match '^\|[-` ]+\|') { continue } # separator row
    if ($row -match '^\|\s*Document\s*\|\s*Current Version\s*\|') { continue } # header titles row

    if ($row -match '^\|\s*`?([^`|]+)`?\s*\|\s*([^|]+?)\s*\|') {
        $doc = $Matches[1].Trim()
        $manifestVersion = $Matches[2].Trim()

        if (-not ($manifestVersion -match '^v\d+\.\d+$')) {
            $failures += "Manifest version for $doc not in vMajor.Minor pattern: $manifestVersion"
            continue
        }

        if (-not $doc.EndsWith('.md')) { continue }
        $path = if ($doc -like '.github/*') { $doc } else { "docs/$doc" }
        if (-not (Test-Path $path)) { $failures += "File listed but not found: $path"; continue }

        $content = Get-Content $path -Raw -ErrorAction Stop
        $fileVersionFull = $null
        if ($content -match '(?im)^\|\s*\*\*Version\*\*\s*\|\s*([^|]+?)\s*\|') { $fileVersionFull = $Matches[1].Trim() }
        elseif ($content -match '(?im)^\|\s*Version\s*\|\s*([^|]+?)\s*\|') { $fileVersionFull = $Matches[1].Trim() }
        else { $failures += "Version metadata row not found in $path"; continue }

        if ($fileVersionFull -notmatch '^(\d{4}-\d{2}-\d{2})\.v(\d+)(?:\.(\d+))?$') {
            $failures += "Unrecognized file version format in ${path}: $fileVersionFull"; continue }

        $major = $Matches[2]
        $minor = if ($Matches[3]) { $Matches[3] } else { '0' }
        $expectedManifestVersion = "v$major.$minor"
        if ($manifestVersion -ne $expectedManifestVersion) {
            $failures += "Mismatch for $path (manifest=$manifestVersion expected=$expectedManifestVersion from $fileVersionFull)"
        }
        $checked++
    }
}

if ($checked -eq 0) {
    $failures += 'No documentation rows processed; table parse failure.'
    Emit-Result -Success:$false -Failures $failures -Checked 0 -Status 'ParseFailure'
}

$success = $failures.Count -eq 0
$status = if ($success) { 'OK' } else { 'Failures' }
Emit-Result -Success:$success -Failures $failures -Checked $checked -Status $status
