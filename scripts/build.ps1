#!/usr/bin/env pwsh
$ErrorActionPreference = "Stop"

$SolutionPath = "src/IV.DX/IV.DX.sln"

dotnet restore $SolutionPath
dotnet build  $SolutionPath -c Release