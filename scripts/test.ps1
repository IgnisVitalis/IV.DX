#!/usr/bin/env pwsh
param(
    [string]$Solution,
    [ValidateSet("Debug","Release")]
    [string]$Configuration = "Debug",
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"

$RepoRoot = Resolve-Path "$PSScriptRoot/.."

function Get-NearestSolution {
    param([string]$Root)
    $sln = Get-ChildItem -Path $Root -Filter *.sln -File -Recurse | Sort-Object FullName | Select-Object -First 1
    if (-not $sln) { throw "No solution found" }
    $sln.FullName
}

function Get-TestProjectsFromSolution {
    param([string]$SolutionPath)

    $slnDir = Split-Path $SolutionPath -Parent
    dotnet sln $SolutionPath list |
        Where-Object { $_ -match '\.csproj$' } |
        ForEach-Object { (Resolve-Path (Join-Path $slnDir $_.Trim())).Path } |
        Where-Object {
            $name = [System.IO.Path]::GetFileNameWithoutExtension($_)
            $name -like "*Tests"
        }
}

$SlnPath = if ($Solution) {
    (Resolve-Path (Join-Path $RepoRoot $Solution)).Path
} else {
    Get-NearestSolution $RepoRoot
}

if (-not $NoBuild) {
    dotnet build $SlnPath -c $Configuration | Out-Host
}

$testProjects = @(Get-TestProjectsFromSolution $SlnPath)
if ($testProjects.Count -eq 0) { throw "No test projects (*Tests) found in solution" }

$resultsDir = Join-Path $RepoRoot ".testresults"
if (-not (Test-Path $resultsDir)) { New-Item -ItemType Directory -Path $resultsDir | Out-Null }

$summary = @()

foreach ($tp in $testProjects) {
    $projName = [System.IO.Path]::GetFileNameWithoutExtension($tp)
    $trxFile = Join-Path $resultsDir "$projName.trx"

    if (Test-Path $trxFile) {
        Remove-Item $trxFile -Force -ErrorAction SilentlyContinue
    }

    dotnet test $tp `
        -c $Configuration `
        --no-build `
        --logger "trx;LogFileName=$trxFile" `
        | Out-Host

    $exit = [int]$LASTEXITCODE

    $total = $null
    $failed = $null
    $passed = $null
    $skipped = $null
    $duration = $null

    if (Test-Path $trxFile) {
        try {
            [xml]$trx = Get-Content $trxFile
            $counters = $trx.TestRun.ResultSummary.Counters

            $total = [int]$counters.total
            $failed = [int]$counters.failed
            $passed = [int]$counters.passed
            $skipped = [int]$counters.notExecuted

            $times = $trx.TestRun.Times
            if ($times -and $times.finish -and $times.start) {
                $start = [datetime]$times.start
                $finish = [datetime]$times.finish
                $duration = [double](($finish - $start).TotalSeconds)
            }
        } catch {}
    }

    if ($total -eq $null -or $total -le 0) { continue }

    $summary += [pscustomobject]@{
        Project  = $projName
        Total    = [int]$total
        Passed   = [int]$passed
        Failed   = [int]$failed
        Skipped  = [int]$skipped
        Seconds  = if ($duration -ne $null) { [math]::Round([double]$duration, 2) } else { $null }
        ExitCode = $exit
    }
}

if ($summary.Count -eq 0) {
    throw "No projects with actual tests (Total > 0) were found."
}

$totalAll   = [int](($summary | Measure-Object Total   -Sum).Sum)
$passedAll  = [int](($summary | Measure-Object Passed  -Sum).Sum)
$failedAll  = [int](($summary | Measure-Object Failed  -Sum).Sum)
$skippedAll = [int](($summary | Measure-Object Skipped -Sum).Sum)
$secondsAll = [double](($summary | Measure-Object Seconds -Sum).Sum)

$summary += [pscustomobject]@{
    Project  = ""
    Total    = $null
    Passed   = $null
    Failed   = $null
    Skipped  = $null
    Seconds  = $null
    ExitCode = $null
}

$summary += [pscustomobject]@{
    Project  = ""
    Total    = $totalAll
    Passed   = $passedAll
    Failed   = $failedAll
    Skipped  = $skippedAll
    Seconds  = [math]::Round($secondsAll, 2)
    ExitCode = if ($failedAll -gt 0) { 1 } else { 0 }
}

Write-Host ""
Write-Host "==== TEST SUMMARY ===="
$summary | Format-Table -AutoSize | Out-Host

if ($failedAll -gt 0 -or ($summary | Where-Object { $_.ExitCode -ne $null -and $_.ExitCode -ne 0 } | Measure-Object).Count -gt 0) {
    exit 1
}
