<# :
@echo off
set "SCRIPT_DIR=%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -Command "Invoke-Expression (Get-Content -LiteralPath '%~f0' -Raw)"
echo.
pause
exit /b
#>
param(
    [string]$RefFile = $null,
    [string]$BasePath = $null,
    [string]$Pattern = "Localize.xaml",
    [switch]$Placeholder,
    [switch]$DryRun,
    [switch]$Backup
)

function Info([string]$s) { Write-Host $s -ForegroundColor Cyan }
function Ok([string]$s)   { Write-Host $s -ForegroundColor Green }
function Warn([string]$s) { Write-Host $s -ForegroundColor Yellow }
function Update([string]$s) { Write-Host $s -ForegroundColor Blue }

function Get-RegionName([string]$filePath, [string]$basePath) {
    try {
        $relative = [System.IO.Path]::GetRelativePath($basePath, $filePath)
        $parts = $relative -split '[\\/]+' 
        if ($parts.Count -ge 2 -and $parts[0]) { return $parts[0].ToUpper() }
    } catch { }
    $parent = Split-Path -Parent $filePath
    if ($parent) {
        return (Split-Path $parent -Leaf).ToUpper()
    }
    return "UNKNOWN"
}

function Write-SyncReport {
    param(
        [string]$LangCode,
        [string[]]$AddedKeys,
        [string[]]$DeletedKeys,
        [string[]]$AddedRegions,
        [string[]]$DeletedRegions
    )

    $hasChanges = ($AddedKeys.Count -gt 0) -or ($DeletedKeys.Count -gt 0) -or ($AddedRegions.Count -gt 0) -or ($DeletedRegions.Count -gt 0)

    Write-Host "`nTARGET: " -NoNewline -ForegroundColor Gray
    Write-Host "[$LangCode]" -NoNewline -ForegroundColor Cyan
    Write-Host " | STATUS: " -NoNewline -ForegroundColor Gray
    
    if (-not $hasChanges) {
        Write-Host "No changes" -ForegroundColor Green
        return
    }
    
    Write-Host "Changed" -ForegroundColor Yellow

    if ($AddedRegions -and $AddedRegions.Count -gt 0) {
        foreach ($r in $AddedRegions) {
            if ($r) {
                Write-Host "   [+] Region added: " -NoNewline -ForegroundColor Green
                Write-Host $r -ForegroundColor White
            }
        }
    }

    if ($DeletedRegions -and $DeletedRegions.Count -gt 0) {
        foreach ($r in $DeletedRegions) {
            if ($r) {
                Write-Host "   [-] Region deleted: " -NoNewline -ForegroundColor Red
                Write-Host $r -ForegroundColor White
            }
        }
    }

    if ($AddedKeys.Count -gt 0) {
        Write-Host "   [+] Keys added:" -ForegroundColor Green
        $AddedKeys | ForEach-Object { Write-Host "       - $_" -ForegroundColor Gray }
    }

    if ($DeletedKeys.Count -gt 0) {
        Write-Host "   [-] Keys deleted:" -ForegroundColor Red
        $DeletedKeys | ForEach-Object { Write-Host "       - $_" -ForegroundColor Gray }
    }
}

$scriptDir = if ($env:SCRIPT_DIR) { $env:SCRIPT_DIR } elseif ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Definition }

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
}

if (-not $BasePath) { Write-Host "BasePath not found. Provide -BasePath"; exit 1 }
if (-not $RefFile) { $RefFile = Join-Path $BasePath "en\Localize.xaml" }
if (-not (Test-Path -LiteralPath $RefFile)) { Write-Host "Reference file not found: $RefFile"; exit 4 }

$elementRegex = '(<v:String\b[^>]*\bx:Key\s*=\s*"[^\"]+"[^>]*?(?:\/>|>.*?<\/v:String>))'
$keyAttrRegex = 'x:Key\s*=\s*"([^"]+)"'
$regionTagRegex = '<!--#region\s+(.+?)\s*-->'

$refText = Get-Content -Raw -LiteralPath $RefFile

$refMatches = [regex]::Matches($refText, $regionTagRegex)
$refRegions = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
foreach ($match in $refMatches) {
    if ($match.Groups.Count -gt 1) {
        $val = $match.Groups[1].Value.Trim()
        if ($val) { [void]$refRegions.Add($val) }
    }
}

$refKeysSet = [System.Collections.Generic.HashSet[string]]::new()
$refElements = @()
foreach ($m in [regex]::Matches($refText, $elementRegex, [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
    $el = $m.Groups[1].Value
    $km = [regex]::Match($el, $keyAttrRegex)
    if ($km.Success) {
        $k = $km.Groups[1].Value
        $refElements += [pscustomobject]@{ Key = $k; Element = $el }
        [void]$refKeysSet.Add($k)
    }
}

# --- Process Language Catalog ---
function Process-Catalog {
    param([string]$path)
    if (-not (Test-Path -LiteralPath $path)) { return }
    Info "Auditing Language Catalog: $(Split-Path $path -Leaf)"
    $text = Get-Content -Raw -LiteralPath $path
    $updated = $text
    
    foreach ($match in [regex]::Matches($text, $keyAttrRegex)) {
        $k = $match.Groups[1].Value
        $nk = $k.ToLower().Replace('-', '_')
        if ($nk -ne $k) {
            Update "Normalizing key: '$k' -> '$nk'"
            $updated = $updated.Replace("x:Key=""$k""", "x:Key=""$nk""")
        }
    }
    
    if ($updated -ne $text) {
        if (-not $DryRun) {
            if ($Backup) { Copy-Item $path ($path + ".bak") -Force }
            Set-Content -LiteralPath $path -Value $updated -Force
            Ok "Catalog updated successfully.`n"
        }
    } else { Ok "Catalog is up to date.`n" }
}

Process-Catalog (Join-Path $BasePath "LanguageCatalog.xaml")

# --- Process Target Files ---
$targets = Get-ChildItem -Path $BasePath -Recurse -Filter $Pattern | Where-Object { $_.FullName -ne (Get-Item $RefFile).FullName }

Info "Syncing $($targets.Count) target(s) against reference: $(Split-Path $RefFile -Leaf)"

foreach ($targetFile in $targets) {
    $targetPath = $targetFile.FullName
    $langCode = Get-RegionName -filePath $targetPath -basePath $BasePath
    $targetText = Get-Content -Raw -LiteralPath $targetPath

    $targetKeys = @{}
    foreach ($m in [regex]::Matches($targetText, $elementRegex, [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
        $km = [regex]::Match($m.Groups[1].Value, $keyAttrRegex)
        if ($km.Success) { $targetKeys[$km.Groups[1].Value] = $m.Groups[1].Value }
    }
    
    $tMatches = [regex]::Matches($targetText, $regionTagRegex)
    $targetRegions = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($tm in $tMatches) {
        if ($tm.Groups.Count -gt 1) {
            $val = $tm.Groups[1].Value.Trim()
            if ($val) { [void]$targetRegions.Add($val) }
        }
    }

    $addedK = @()
    foreach ($re in $refElements) {
        if (-not $targetKeys.ContainsKey($re.Key)) { $addedK += $re.Key }
    }
    
    $deletedK = @()
    foreach ($tk in $targetKeys.Keys) {
        if (-not $refKeysSet.Contains($tk)) { $deletedK += $tk }
    }

    $addedR = @()
    foreach ($rr in $refRegions) {
        if (-not $targetRegions.Contains($rr)) { $addedR += $rr }
    }
    
    $deletedR = @()
    foreach ($tr in $targetRegions) {
        if (-not $refRegions.Contains($tr)) { $deletedR += $tr }
    }

    Write-SyncReport -LangCode $langCode `
                     -AddedKeys $addedK -DeletedKeys $deletedK `
                     -AddedRegions $addedR -DeletedRegions $deletedR

    $needsUpdate = ($addedK.Count -gt 0) -or ($deletedK.Count -gt 0) -or ($addedR.Count -gt 0) -or ($deletedR.Count -gt 0)

    if ($needsUpdate) {
        if ($DryRun) {
            Warn "DryRun: Skipping file write for $langCode"
        } else {
            $sb = [System.Text.StringBuilder]::new($refText)
            
            foreach ($re in $refElements) {
                if ($targetKeys.ContainsKey($re.Key)) {
                    [void]$sb.Replace($re.Element, $targetKeys[$re.Key])
                } elseif ($Placeholder) {
                    $placeholderTag = "<v:String x:Key=""$($re.Key)""></v:String>"
                    [void]$sb.Replace($re.Element, $placeholderTag)
                }
            }

            if ($Backup) { Copy-Item $targetPath ($targetPath + ".bak") -Force }
            Set-Content -LiteralPath $targetPath -Value $sb.ToString() -Force
        }
    }
}

Info "`nDone."