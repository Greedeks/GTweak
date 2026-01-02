param(
    [string]$RefFile = $null,
    [string]$BasePath = $null,
    [string]$Pattern = "Localize.xaml",
    [switch]$Placeholder,
    [switch]$DryRun,
    [switch]$Backup,
    [switch]$Wait
)

function Info([string]$s) { Write-Host $s -ForegroundColor Cyan }
function Ok([string]$s)   { Write-Host $s -ForegroundColor Green }
function Warn([string]$s) { Write-Host $s -ForegroundColor Yellow }

$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Definition }

# auto-detect BasePath
if (-not $BasePath) {
    $cur = $scriptDir
    $gtweakRoot = $null
    while ($cur -and ($cur -ne [IO.Path]::GetPathRoot($cur))) {
        $leaf = Split-Path $cur -Leaf
        if ($leaf -ieq "GTweak") { $gtweakRoot = $cur; break }
        if ($leaf -ieq "Assets") {
            $parent = Split-Path $cur -Parent
            if ((Split-Path $parent -Leaf) -ieq "GTweak") { $gtweakRoot = $parent; break }
        }
        $cur = Split-Path $cur -Parent
    }
    if ($gtweakRoot) { $BasePath = Join-Path $gtweakRoot ".Source\GTweak\Languages" }
    else {
        $guess = Join-Path (Split-Path $scriptDir -Parent) ".Source\GTweak\Languages"
        if (Test-Path $guess) { $BasePath = $guess }
    }
}

if (-not $BasePath) { Write-Host "BasePath not found. Provide -BasePath"; exit 1 }
if (-not (Test-Path -LiteralPath $BasePath)) { Write-Host "BasePath does not exist: $BasePath"; exit 2 }

if (-not $RefFile) { $RefFile = Join-Path $BasePath "en\Localize.xaml" }
else {
    if (-not (Test-Path -LiteralPath $RefFile)) {
        $try = Join-Path $BasePath $RefFile
        if (Test-Path -LiteralPath $try) { $RefFile = $try } else { Write-Host "RefFile not found: $RefFile"; exit 3 }
    }
}
if (-not (Test-Path -LiteralPath $RefFile)) { Write-Host "Reference file not found: $RefFile"; exit 4 }

$refText = Get-Content -Raw -LiteralPath $RefFile

$elementRegex = '(<v:String\b[^>]*>.*?</v:String>)'
$keyAttrRegex = 'x:Key\s*=\s*"([^"]+)"'
$regionStartRegex = '<!--\#region\s*(.*?)\s*-->'

$refElements = @()
foreach ($m in [regex]::Matches($refText, $elementRegex, [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
    $el = $m.Groups[1].Value
    $km = [regex]::Match($el, $keyAttrRegex, [System.Text.RegularExpressions.RegexOptions]::Singleline)
    if ($km.Success) { $refElements += [pscustomobject]@{ Key = $km.Groups[1].Value; Element = $el } }
}
if ($refElements.Count -eq 0) { Write-Host "No <v:String ...> elements in reference."; exit 5 }

$refRegions = @()
$pos = 0
while ($true) {
    $mStart = [regex]::Match($refText.Substring($pos), '<!--\#region\b.*?-->', [System.Text.RegularExpressions.RegexOptions]::Singleline)
    if (-not $mStart.Success) { break }
    $nameMatch = [regex]::Match($mStart.Value, $regionStartRegex, [System.Text.RegularExpressions.RegexOptions]::Singleline)
    $rname = if ($nameMatch.Success) { $nameMatch.Groups[1].Value.Trim() } else { "" }
    $refRegions += $rname
    $pos += $mStart.Index + $mStart.Length
}

$targets = Get-ChildItem -Path $BasePath -Recurse -File -Filter $Pattern |
           Where-Object { $_.FullName -ne (Get-Item -LiteralPath $RefFile).FullName } |
           Select-Object -ExpandProperty FullName
if ($targets.Count -eq 0) { Write-Host "No target files found."; exit 0 }

Info "Reference: $RefFile"
Info "Targets: $($targets.Count)"

function MakeKeyRegex([string]$key) {
    $k = [regex]::Escape($key)
    return [regex]::new('(<v:String\b[^>]*\bx:Key\s*=\s*"' + $k + '"[^>]*>.*?</v:String>)', [System.Text.RegularExpressions.RegexOptions]::Singleline)
}
function DetectNewline([string]$text) {
    if ($text -match "`r`n") { return "`r`n" }
    if ($text -match "`n") { return "`n" }
    return [Environment]::NewLine
}
function GetIndentForMatch([string]$text, [System.Text.RegularExpressions.Match]$m) {
    $before = $text.Substring(0, $m.Index)
    $lastNl = $before.LastIndexOf("`n")
    if ($lastNl -lt 0) { $lineStart = 0 } else { $lineStart = $lastNl + 1 }
    $line = $before.Substring($lineStart)
    if ($line -match '^[ \t]*') { return $matches[0] } else { return "" }
}
function IndentElement([string]$elem, [string]$indent, [string]$nl) {
    $lines = [regex]::Split($elem, "\r\n|\n")
    $trimmed = $lines | ForEach-Object { $_ -replace '^[ \t]+' , '' }
    return ($trimmed | ForEach-Object { $indent + $_ }) -join $nl
}

foreach ($target in $targets) {
    Info "`nProcessing: $target"
    $targetText = Get-Content -Raw -LiteralPath $target
    $nl = DetectNewline $targetText

    # existing keys
    $existingKeys = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($m in [regex]::Matches($targetText, $elementRegex, [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
        $el = $m.Groups[1].Value
        $km = [regex]::Match($el, $keyAttrRegex, [System.Text.RegularExpressions.RegexOptions]::Singleline)
        if ($km.Success) { $existingKeys.Add($km.Groups[1].Value) | Out-Null }
    }

    $missing = $refElements | Where-Object { -not $existingKeys.Contains($_.Key) }
    Info "  Missing keys: $($missing.Count)"

    # region checks
    $targetRegionNames = @()
    foreach ($m in [regex]::Matches($targetText, '<!--\#region\b.*?-->', [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
        $nm = [regex]::Match($m.Value, $regionStartRegex)
        if ($nm.Success) { $targetRegionNames += $nm.Groups[1].Value.Trim() } else { $targetRegionNames += "" }
    }

    $missingRegions = @()
    for ($ri=0; $ri -lt $refRegions.Count; $ri++) {
        $rname = $refRegions[$ri]
        if ($rname -and (-not ($targetRegionNames -contains $rname))) { $missingRegions += $rname }
    }
    if ($missingRegions.Count -gt 0) { Warn "  Missing regions: $($missingRegions -join ', ')" } else { Ok "  Regions: OK" }

    # ordering check
    $lastIndex = -1
    $orderingMismatch = $false
    for ($ri=0; $ri -lt $refRegions.Count; $ri++) {
        $rname = $refRegions[$ri]
        if (-not $rname) { continue }
        $idx = $targetRegionNames.IndexOf($rname)
        if ($idx -lt 0) { continue }
        if ($idx -lt $lastIndex) { $orderingMismatch = $true; break }
        $lastIndex = $idx
    }
    if ($orderingMismatch) { Warn "  Regions order differs" }

    if ($missing.Count -eq 0) {
        Ok "  No key changes"
    } else {
        $updatedText = $targetText
        for ($i=0; $i -lt $refElements.Count; $i++) {
            $r = $refElements[$i]
            if ($existingKeys.Contains($r.Key)) { continue }

            $inserted = $false

            # insert AFTER prev existing element
            $prevIndex = -1
            for ($j = $i - 1; $j -ge 0; $j--) { if ($existingKeys.Contains($refElements[$j].Key)) { $prevIndex = $j; break } }
            if ($prevIndex -ne -1) {
                $prevKey = $refElements[$prevIndex].Key
                $mPrev = (MakeKeyRegex $prevKey).Match($updatedText)
                if ($mPrev.Success) {
                    $indent = GetIndentForMatch $updatedText $mPrev
                    $toInsertBody = if ($Placeholder) { "<v:String x:Key=`"$($r.Key)`"></v:String>" } else { $r.Element }
                    $indented = IndentElement $toInsertBody $indent $nl
                    $toInsert = $nl + $indented + $nl
                    $insertPos = $mPrev.Index + $mPrev.Length
                    $updatedText = $updatedText.Substring(0, $insertPos) + $toInsert + $updatedText.Substring($insertPos)
                    $existingKeys.Add($r.Key) | Out-Null
                    $inserted = $true
                }
            }

            # insert BEFORE next existing element
            if (-not $inserted) {
                $nextIndex = -1
                for ($j = $i + 1; $j -lt $refElements.Count; $j++) { if ($existingKeys.Contains($refElements[$j].Key)) { $nextIndex = $j; break } }
                if ($nextIndex -ne -1) {
                    $nextKey = $refElements[$nextIndex].Key
                    $mNext = (MakeKeyRegex $nextKey).Match($updatedText)
                    if ($mNext.Success) {
                        $indent = GetIndentForMatch $updatedText $mNext
                        $toInsertBody = if ($Placeholder) { "<v:String x:Key=`"$($r.Key)`"></v:String>" } else { $r.Element }
                        $indented = IndentElement $toInsertBody $indent $nl
                        $toInsert = $nl + $indented + $nl
                        $insertPos = $mNext.Index
                        $updatedText = $updatedText.Substring(0, $insertPos) + $toInsert + $updatedText.Substring($insertPos)
                        $existingKeys.Add($r.Key) | Out-Null
                        $inserted = $true
                    }
                }
            }

            # fallback: before closing tag
            if (-not $inserted) {
                $endTag = "</ResourceDictionary>"
                $idxEnd = $updatedText.LastIndexOf($endTag, [System.StringComparison]::OrdinalIgnoreCase)
                if ($idxEnd -ge 0) {
                    $before = $updatedText.Substring(0, $idxEnd)
                    $lastNl = $before.LastIndexOf("`n")
                    if ($lastNl -lt 0) { $lineStart = 0 } else { $lineStart = $lastNl + 1 }
                    $line = $before.Substring($lineStart)
                    $indent = ""
                    if ($line -match '^[ \t]*') { $indent = $matches[0] }
                    $toInsertBody = if ($Placeholder) { "<v:String x:Key=`"$($r.Key)`"></v:String>" } else { $r.Element }
                    $indented = IndentElement $toInsertBody $indent $nl
                    $toInsert = $nl + $indented + $nl
                    $updatedText = $updatedText.Substring(0, $idxEnd) + $toInsert + $updatedText.Substring($idxEnd)
                    $existingKeys.Add($r.Key) | Out-Null
                    $inserted = $true
                }
            }

            if ($inserted) { Write-Host "  Inserted: $($r.Key)" } else { Write-Host "  Failed: $($r.Key)" }
        }

        if ($updatedText -ne $targetText) {
            if ($DryRun) { Write-Host "  (DryRun) File would be changed." }
            else {
                if ($Backup) { Copy-Item -LiteralPath $target -Destination ($target + ".bak") -Force }
                Set-Content -LiteralPath $target -Value $updatedText -Force
                Ok "  Updated: $target"
            }
        }
    }
}

Info "`nDone."
