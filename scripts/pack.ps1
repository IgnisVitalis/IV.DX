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

$RepoRoot   = Resolve-Path "$PSScriptRoot/.."
$OutputPath = Join-Path $HOME ".nuget" "local-feed"
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
        if ($p -match '[/\\]Tests[/\\]') { continue }

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
    param([string]$Folder, [string[]]$PackageIds, [string]$Mode)

    # All IV.DX packages ship in lockstep: the provider SPI is internal, so a
    # provider package is only valid against the exact core version it was built
    # with. Version is therefore derived from the highest existing version across
    # every package and applied uniformly.
    $items = foreach ($PackageId in $PackageIds) {
        $rx = [regex]("^" + [regex]::Escape($PackageId) + "\.(\d+)\.(\d+)\.(\d+)\.nupkg$")
        Get-ChildItem $Folder -Filter "$PackageId.*.nupkg" -File -ErrorAction SilentlyContinue |
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
    }

    if (-not $items) { return "0.1.0" }

    $v = $items | Sort-Object Major, Minor, Patch -Descending | Select-Object -First 1
    if ($Mode -eq "patch") {
        return "$($v.Major).$($v.Minor).$($v.Patch + 1)"
    }
    return "$($v.Major).$($v.Minor + 1).0"
}

$sln = if ($Solution) { (Resolve-Path (Join-Path $RepoRoot $Solution)).Path } else { Get-NearestSolution $RepoRoot }

# The whole package family is always discovered, even when packing a single
# project, so the computed version accounts for every package.
$family = @(Get-PackCandidates $sln)
if ($family.Count -eq 0) { throw "No packable projects found" }

if ($Project) {
    $projPath = (Resolve-Path (Join-Path $RepoRoot $Project)).Path
    $candidates = @($family | Where-Object { $_.ProjectPath -eq $projPath })
    if ($candidates.Count -eq 0) { throw "Not a packable project in the solution: $Project" }
} else {
    $candidates = $family
}

if (-not $Version) {
    $mode = if ($BumpPatch) { "minor" } else { "patch" }
    $Version = Get-NextVersion $OutputPath ($family | ForEach-Object { $_.PackageId }) $mode
}

foreach ($c in $candidates) {
    $pkg = Join-Path $OutputPath "$($c.PackageId).$Version.nupkg"
    if (Test-Path $pkg) { throw "Package already exists: $pkg" }
}

foreach ($c in $candidates) {
    # Restore is intentionally NOT skipped: with --no-restore a csproj change
    # since the last restore is packed against a stale project.assets.json,
    # silently producing a package built on the wrong dependency graph.
    dotnet pack `
        $c.ProjectPath `
        -c $Configuration `
        -o $OutputPath `
        -p:Version=$Version `
        -p:TreatWarningsAsErrors=true

    if ($LASTEXITCODE -ne 0) { throw "pack failed for $($c.PackageId)" }
}

foreach ($c in $candidates) {
    Write-Host (Join-Path $OutputPath "$($c.PackageId).$Version.nupkg")
}