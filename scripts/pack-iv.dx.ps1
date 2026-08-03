#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Packs the IV.DX core package into the local NuGet feed.

.DESCRIPTION
    Thin wrapper over pack.ps1 targeting IV.DX.Hosting, which carries
    PackageId "IV.DX" and merges the core assemblies into the package.

    IV.DX and IV.DX.PostgreSQL must ship on the same version — the provider SPI
    is internal, so a provider package is only valid against the exact core
    version it was built with. When releasing both, either run pack.ps1 with no
    -Project (packs the whole family on one version) or pass the same -Version
    to this script and pack-iv.dx.postgresql.ps1.
#>
param(
    [string]$Version,
    [ValidateSet("Debug","Release")]
    [string]$Configuration = "Debug",
    [switch]$BumpPatch
)

$ErrorActionPreference = "Stop"

& "$PSScriptRoot/pack.ps1" `
    -Project "src/IV.DX/IV.DX.Hosting/IV.DX.Hosting.csproj" `
    -Version $Version `
    -Configuration $Configuration `
    -BumpPatch:$BumpPatch
