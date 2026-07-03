#!/usr/bin/env pwsh
param(
    [string]$SolutionPath = "src/IV.DX/IV.DX.sln",

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "Building solution: $SolutionPath"
Write-Host "Configuration: $Configuration"

dotnet restore $SolutionPath

dotnet build $SolutionPath `
    -c $Configuration `
    -p:TreatWarningsAsErrors=true
