#!/usr/bin/env pwsh
param(
    [string]$Project,
    [string]$Solution,
    [string]$Version,
    [ValidateSet("Debug","Release")]
    [string]$Configuration = "Debug",
    [switch]$BumpPatch
)

$ErrorActionPreference = "Stop"

$RepoRoot = Resolve-Path "$PSScriptRoot/.."
$ArtifactsRoot = Resolve-Path "$RepoRoot/../.."
$OutputPath = Join-Path $ArtifactsRoot ".artifacts"
if (-not (Test-Path $OutputPath)) { New-Item -ItemType Directory -Path $OutputPath | Out-Null }

function Get-NearestSolution {
    param([string]$Root)
    $sln = Get-ChildItem -Path $Root -Filter *.sln -File -Recurse | Sort-Object FullName | Select-Object -First 1
    if (-not $sln) { throw "No solution found" }
    $sln.FullName
}

function Get-PackCandidates {
    param([string]$SolutionPath)

    $slnDir = Split-Path $SolutionPath -Parent
    $projects = dotnet sln $SolutionPath list |
        Where-Object { $_ -match '\.csproj$' } |
        ForEach-Object { (Resolve-Path (Join-Path $slnDir $_.Trim())).Path }

    foreach ($p in $projects) {
        if ($p -match '\\Tests\\') { continue }

        $xml = Get-Content $p -Raw
        if ($xml -notmatch '<IsPackable>\s*true\s*</IsPackable>') { continue }

        $m = [regex]::Match($xml, '<PackageId>\s*([^<]+?)\s*</PackageId>')
        if (-not $m.Success) { continue }

        [pscustomobject]@{
            ProjectPath = $p
            PackageId   = $m.Groups[1].Value.Trim()
        }
    }
}

function Get-NextVersion {
    param([string]$Folder, [string]$PackageId, [string]$Mode)

    $rx = [regex]("^" + [regex]::Escape($PackageId) + "\.(\d+)\.(\d+)\.(\d+)\.nupkg$")
    $items = Get-ChildItem $Folder -Filter "$PackageId.*.nupkg" -File -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -notlike "*.symbols.nupkg" } |
        ForEach-Object {
            $m = $rx.Match($_.Name)
            if ($m.Success) {
                [pscustomobject]@{
                    Major = [int]$m.Groups[1].Value
                    Minor = [int]$m.Groups[2].Value
                    Patch = [int]$m.Groups[3].Value
                }
            }
        }

    if (-not $items) { return "0.1.0" }

    $v = $items | Sort-Object Major, Minor, Patch -Descending | Select-Object -First 1
    if ($Mode -eq "patch") {
        return "$($v.Major).$($v.Minor).$($v.Patch + 1)"
    }
    return "$($v.Major).$($v.Minor + 1).0"
}

if ($Project) {
    $projPath = (Resolve-Path (Join-Path $RepoRoot $Project)).Path
    $xml = Get-Content $projPath -Raw
    $m = [regex]::Match($xml, '<PackageId>\s*([^<]+?)\s*</PackageId>')
    if (-not $m.Success) { throw "PackageId not found" }
    $PackageId = $m.Groups[1].Value.Trim()
    $ProjectPath = $projPath
} else {
    $sln = if ($Solution) { (Resolve-Path (Join-Path $RepoRoot $Solution)).Path } else { Get-NearestSolution $RepoRoot }
    $candidates = @(Get-PackCandidates $sln)
    if ($candidates.Count -eq 0) { throw "No packable projects found" }
    if ($candidates.Count -gt 1) { throw "Multiple packable projects found. Specify -Project." }
    $ProjectPath = $candidates[0].ProjectPath
    $PackageId   = $candidates[0].PackageId
}

if (-not $Version) {
    $mode = if ($BumpPatch) { "patch" } else { "minor" }
    $Version = Get-NextVersion $OutputPath $PackageId $mode
}

$pkg = Join-Path $OutputPath "$PackageId.$Version.nupkg"
if (Test-Path $pkg) { throw "Package already exists" }

dotnet pack `
    $ProjectPath `
    -c $Configuration `
    --no-restore `
    -o $OutputPath `
    -p:Version=$Version
