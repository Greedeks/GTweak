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
function Update([string]$s) { Write-Host $s -ForegroundColor Blue }

# detect script directory
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

$elementRegex = '(<v:String\b[^>]*\bx:Key\s*=\s*"[^\"]+"[^>]*?(?:\/>|>.*?<\/v:String>))'
$keyAttrRegex = 'x:Key\s*=\s*"([^"]+)"'
$regionStartRegex = '<!--\#region\s*(.*?)\s*-->'


# Collect the `LanguageCatalog.xaml` and ensure key formatting
function ProcessLanguageCatalog([string]$catalogFile) {
    if (-not (Test-Path -LiteralPath $catalogFile)) {
        Write-Host "LanguageCatalog file not found: $catalogFile"
        return
    }

    Info "Processing LanguageCatalog: $catalogFile"

    $catalogText = Get-Content -Raw -LiteralPath $catalogFile
    $catalogKeys = [regex]::Matches($catalogText, $keyAttrRegex)

    $updatedCatalogText = $catalogText

    foreach ($match in $catalogKeys) {
        $key = $match.Groups[1].Value

        $newKey = $key.ToLower().Replace('-', '_')

        if ($newKey -ne $key) {
            Update "  Key '$key' updated to '$newKey'"
            $updatedCatalogText = $updatedCatalogText -replace ("x:Key=`"$key`""), "x:Key=`"$newKey`""
        }
    }

    $endTag = "</ResourceDictionary>"
    $idxEnd = $updatedCatalogText.LastIndexOf($endTag, [System.StringComparison]::OrdinalIgnoreCase)

    if ($idxEnd -ge 0) {
        $beforeEnd = $updatedCatalogText.Substring(0, $idxEnd + $endTag.Length)
        $afterEnd = $updatedCatalogText.Substring($idxEnd + $endTag.Length)
        $cleanedAfterEnd = $afterEnd.TrimEnd()
        $updatedCatalogText = $beforeEnd + $cleanedAfterEnd
    }

    if ($updatedCatalogText -ne $catalogText) {
        if ($DryRun) {
            Write-Host "  (DryRun) LanguageCatalog would be changed."
        } else {
            if ($Backup) { Copy-Item -LiteralPath $catalogFile -Destination ($catalogFile + ".bak") -Force }
            Set-Content -LiteralPath $catalogFile -Value $updatedCatalogText -Force
            Ok "  Updated: $catalogFile"
        }
    } else {
        Ok "  No changes: $catalogFile"
    }
}

$languageCatalogFile = Join-Path $BasePath "LanguageCatalog.xaml"
ProcessLanguageCatalog $languageCatalogFile
Write-Host ""

# collect reference elements and key list
$refElements = @()
foreach ($m in [regex]::Matches($refText, $elementRegex, [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
    $el = $m.Groups[1].Value
    $km = [regex]::Match($el, $keyAttrRegex, [System.Text.RegularExpressions.RegexOptions]::Singleline)
    if ($km.Success) { $refElements += [pscustomobject]@{ Key = $km.Groups[1].Value; Element = $el } }
}
if ($refElements.Count -eq 0) { Write-Host "No <v:String ...> elements in reference."; exit 5 }

$refKeys = [System.Collections.Generic.HashSet[string]]::new()
$refElements | ForEach-Object { $refKeys.Add($_.Key) | Out-Null }

# collect ordered region names from reference
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

# find targets
$targets = Get-ChildItem -Path $BasePath -Recurse -File -Filter $Pattern |
           Where-Object { $_.FullName -ne (Get-Item -LiteralPath $RefFile).FullName } |
           Select-Object -ExpandProperty FullName
if ($targets.Count -eq 0) { Write-Host "No target files found."; exit 0 }

Info "Reference: $RefFile"
Info "Targets: $($targets.Count)"

function MakeKeyRegex([string]$key) {
    $k = [regex]::Escape($key)
    return [regex]::new('(<v:String\b[^>]*\bx:Key\s*=\s*"' + $k + '"[^>]*?(?:\/>|>.*?<\/v:String>))', [System.Text.RegularExpressions.RegexOptions]::Singleline)
}

function DetectNewline([string]$text) {
    if ($text -match "`r`n") { return "`r`n" }
    if ($text -match "`n") { return "`n" }
    return [Environment]::Newline
}

function GetIndentForMatch([string]$text, [System.Text.RegularExpressions.Match]$m) {
    $before = $text.Substring(0, $m.Index)
    $lastNl = $before.LastIndexOf("`n")
    if ($lastNl -lt 0) { $lineStart = 0 } else { $lineStart = $lastNl + 1 }
    $line = $before.Substring($lineStart)
    if ($line -match '^[ \t]*') { return $matches[0] } else { return "" }
}

function GetIndentForPosition([string]$text, [int]$position) {
    $before = $text.Substring(0, $position)
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

function GetFollowingWhitespace([string]$text, [int]$position) {
    $result = ""
    for ($i = $position; $i -lt $text.Length; $i++) {
        $c = $text[$i]
        if ($c -eq "`r" -or $c -eq "`n" -or $c -eq " " -or $c -eq "`t") {
            $result += $c
        } else {
            break
        }
    }
    return $result
}

function GetPrecedingWhitespace([string]$text, [int]$position) {
    $result = ""
    for ($i = $position - 1; $i -ge 0; $i--) {
        $c = $text[$i]
        if ($c -eq "`r" -or $c -eq "`n" -or $c -eq " " -or $c -eq "`t") {
            $result = $c + $result
        } else {
            break
        }
    }
    return $result
}

function HasNewline([string]$whitespace) {
    return $whitespace -match "`r" -or $whitespace -match "`n"
}

foreach ($target in $targets) {
    Info "`nProcessing: $target"
    $targetText = Get-Content -Raw -LiteralPath $target
    $nl = DetectNewline $targetText

    # collect existing keys in target
    $existingKeys = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($m in [regex]::Matches($targetText, $elementRegex, [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
        $el = $m.Groups[1].Value
        $km = [regex]::Match($el, $keyAttrRegex, [System.Text.RegularExpressions.RegexOptions]::Singleline)
        if ($km.Success) { $existingKeys.Add($km.Groups[1].Value) | Out-Null }
    }

    # keys to delete
    $keysToDelete = @()
    foreach ($k in $existingKeys) { if (-not $refKeys.Contains($k)) { $keysToDelete += $k } }

    # keys to insert
    $missing = @($refElements | Where-Object { -not $existingKeys.Contains($_.Key) })

    Info "  Missing keys: $($missing.Count); Extra keys: $($keysToDelete.Count)"

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

    # order check
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

    $updatedText = $targetText

    foreach ($k in $keysToDelete) {
        $re = MakeKeyRegex $k
        $m = $re.Match($updatedText)
        if ($m.Success) {
            $afterWhitespace = GetFollowingWhitespace $updatedText ($m.Index + $m.Length)
            
            $start = $m.Index
            $len = $m.Length + $afterWhitespace.Length
            $updatedText = $updatedText.Substring(0, $start) + $updatedText.Substring($start + $len)
            
            $existingKeys.Remove($k) | Out-Null
            Write-Host "  Deleted: $k"
        } else {
            Write-Host "  DeleteFailed: $k"
        }
    }

    for ($i=0; $i -lt $refElements.Count; $i++) {
        $r = $refElements[$i]
        if ($existingKeys.Contains($r.Key)) { continue }

        $inserted = $false

        $prevIndex = -1
        for ($j = $i - 1; $j -ge 0; $j--) { if ($existingKeys.Contains($refElements[$j].Key)) { $prevIndex = $j; break } }
        if ($prevIndex -ne -1) {
            $prevKey = $refElements[$prevIndex].Key
            $mPrev = (MakeKeyRegex $prevKey).Match($updatedText)
            if ($mPrev.Success) {
                $indent = GetIndentForMatch $updatedText $mPrev
                $toInsertBody = if ($Placeholder) { "<v:String x:Key=`"$($r.Key)`"></v:String>" } else { $r.Element }
                $indented = IndentElement $toInsertBody $indent $nl
                
                $afterPos = $mPrev.Index + $mPrev.Length
                $afterWhitespace = GetFollowingWhitespace $updatedText $afterPos
                
                if (HasNewline $afterWhitespace) {
                    $toInsert = $nl + $indented
                } else {
                    $toInsert = $nl + $indented + $nl
                }
                
                $insertPos = $mPrev.Index + $mPrev.Length
                $updatedText = $updatedText.Substring(0, $insertPos) + $toInsert + $updatedText.Substring($insertPos)
                $existingKeys.Add($r.Key) | Out-Null
                $inserted = $true
            }
        }

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
                    
                    $beforeWhitespace = GetPrecedingWhitespace $updatedText $mNext.Index
                    
                    if (HasNewline $beforeWhitespace) {
                        $toInsert = $indented + $nl
                    } else {
                        $toInsert = $nl + $indented + $nl
                    }
                    
                    $insertPos = $mNext.Index
                    $updatedText = $updatedText.Substring(0, $insertPos) + $toInsert + $updatedText.Substring($insertPos)
                    $existingKeys.Add($r.Key) | Out-Null
                    $inserted = $true
                }
            }
        }

        # fallback: insert before closing ResourceDictionary
        if (-not $inserted) {
            $endTag = "</ResourceDictionary>"
            $idxEnd = $updatedText.LastIndexOf($endTag, [System.StringComparison]::OrdinalIgnoreCase)
            if ($idxEnd -ge 0) {
                $beforeEnd = $updatedText.Substring(0, $idxEnd)
                $lastNonWs = $beforeEnd.Length - 1
                while ($lastNonWs -ge 0 -and ($beforeEnd[$lastNonWs] -eq " " -or $beforeEnd[$lastNonWs] -eq "`t" -or 
                       $beforeEnd[$lastNonWs] -eq "`r" -or $beforeEnd[$lastNonWs] -eq "`n")) {
                    $lastNonWs--
                }
                
                $indent = GetIndentForPosition $updatedText $idxEnd
                
                $toInsertBody = if ($Placeholder) { "<v:String x:Key=`"$($r.Key)`"></v:String>" } else { $r.Element }
                $indented = IndentElement $toInsertBody $indent $nl
                
                $whitespaceBeforeTag = GetPrecedingWhitespace $updatedText $idxEnd
                if ($whitespaceBeforeTag -match "`r`n" -or $whitespaceBeforeTag -match "`n") {
                    $toInsert = $indented + $nl
                } else {
                    $toInsert = $nl + $indented + $nl
                }
                
                $updatedText = $updatedText.Substring(0, $idxEnd) + $toInsert + $updatedText.Substring($idxEnd)
                $existingKeys.Add($r.Key) | Out-Null
                $inserted = $true
            }
        }

        if ($inserted) { Write-Host "  Inserted: $($r.Key)" } else { Write-Host "  InsertFailed: $($r.Key)" }
    }

    if ($updatedText -ne $targetText) {
        if ($DryRun) {
            Write-Host "  (DryRun) File would be changed."
        } else {
            if ($Backup) { Copy-Item -LiteralPath $target -Destination ($target + ".bak") -Force }
            Set-Content -LiteralPath $target -Value $updatedText -Force
            Ok "  Updated: $target"
        }
    } else {
        Ok "  No changes: $target"
    }
}

Info "`nDone."