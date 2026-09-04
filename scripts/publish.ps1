<#
.SYNOPSIS
    Builds and publishes DigYourWindows for distribution.
.DESCRIPTION
    Publishes both Framework-Dependent Deployment (FDD) and Self-Contained Deployment (SCD)
    to the standardized artifacts directory and generates release packages with SHA-256 checksums.
.PARAMETER Configuration
    Build configuration (default: Release).
.PARAMETER Runtime
    Target runtime identifier (default: win-x64).
.PARAMETER Version
    Application version override (defaults to Version in Directory.Build.props).
.PARAMETER OutputDir
    Root artifacts output directory (default: <repoRoot>/artifacts).
.PARAMETER SkipZip
    Skip compressing published directories into .zip release archives.
.PARAMETER Clean
    Clean the target publish output directory prior to building.
.EXAMPLE
    ./scripts/publish.ps1
.EXAMPLE
    ./scripts/publish.ps1 -Configuration Release -Runtime win-x64 -Version 1.2.0 -Clean
#>
[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Version = "",
    [string]$OutputDir = "",
    [switch]$SkipZip,
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "src\DigYourWindows.UI\DigYourWindows.UI.csproj"
$propsPath = Join-Path $repoRoot "Directory.Build.props"

if (-not (Test-Path $projectPath)) {
    throw "未找到主入口项目: $projectPath"
}

# Auto-detect version from Directory.Build.props if not specified
if ([string]::IsNullOrWhiteSpace($Version) -and (Test-Path $propsPath)) {
    $propsContent = Get-Content $propsPath -Raw
    if ($propsContent -match '<Version>([^<]+)</Version>') {
        $Version = $matches[1].Trim()
    }
}
if ([string]::IsNullOrWhiteSpace($Version)) {
    $Version = "1.2.0"
}

# Standardize output paths
if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $repoRoot "artifacts"
}

$publishRoot = Join-Path $OutputDir "publish"
$releaseRoot = Join-Path $OutputDir "release"
$fddOut = Join-Path $publishRoot "fdd_$Runtime"
$scdOut = Join-Path $publishRoot "scd_$Runtime"

if ($Clean) {
    Write-Host "--> 清理旧发布目录..." -ForegroundColor Cyan
    if (Test-Path $fddOut) { Remove-Item -Recurse -Force $fddOut }
    if (Test-Path $scdOut) { Remove-Item -Recurse -Force $scdOut }
}

New-Item -ItemType Directory -Force -Path $fddOut | Out-Null
New-Item -ItemType Directory -Force -Path $scdOut | Out-Null
New-Item -ItemType Directory -Force -Path $releaseRoot | Out-Null

Write-Host "==================================================" -ForegroundColor Green
Write-Host " DigYourWindows - Publish & Release Packaging" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Green
Write-Host " Project:       $projectPath"
Write-Host " Configuration: $Configuration"
Write-Host " Runtime:       $Runtime"
Write-Host " Version:       $Version"
Write-Host " Publish Root:  $publishRoot"
Write-Host " Release Root:  $releaseRoot"
Write-Host "--------------------------------------------------"

# 1. Framework-Dependent Publish (Portable)
Write-Host "`n[1/4] 发布框架依赖版 (FDD, portable)..." -ForegroundColor Cyan
$fddArgs = @(
    "publish", $projectPath,
    "-c", $Configuration,
    "-r", $Runtime,
    "--self-contained", "false",
    "-p:Version=$Version",
    "-o", $fddOut
)
& dotnet @fddArgs
if ($LASTEXITCODE -ne 0) { throw "dotnet publish (FDD) 失败" }

# 2. Self-Contained Publish (Standalone)
Write-Host "`n[2/4] 发布独立运行版 (SCD, standalone)..." -ForegroundColor Cyan
$scdArgs = @(
    "publish", $projectPath,
    "-c", $Configuration,
    "-r", $Runtime,
    "--self-contained", "true",
    "-p:Version=$Version",
    "-o", $scdOut
)
& dotnet @scdArgs
if ($LASTEXITCODE -ne 0) { throw "dotnet publish (SCD) 失败" }

# 3. Release Packaging
$checksumFile = Join-Path $releaseRoot "SHA256SUMS.txt"
$createdPackages = @()

if (-not $SkipZip) {
    Write-Host "`n[3/4] 打包便携版与独立版 Release Zip..." -ForegroundColor Cyan

    $fddZipName = "DigYourWindows_${Version}_${Runtime}_portable.zip"
    $fddZipPath = Join-Path $releaseRoot $fddZipName
    if (Test-Path $fddZipPath) { Remove-Item -Force $fddZipPath }
    Write-Host "  -> 创建 $fddZipName"
    Compress-Archive -Path (Join-Path $fddOut "*") -DestinationPath $fddZipPath -Force
    $createdPackages += $fddZipPath

    $scdZipName = "DigYourWindows_${Version}_${Runtime}_standalone.zip"
    $scdZipPath = Join-Path $releaseRoot $scdZipName
    if (Test-Path $scdZipPath) { Remove-Item -Force $scdZipPath }
    Write-Host "  -> 创建 $scdZipName"
    Compress-Archive -Path (Join-Path $scdOut "*") -DestinationPath $scdZipPath -Force
    $createdPackages += $scdZipPath

    # 4. Compute Checksums
    Write-Host "`n[4/4] 计算 SHA-256 校验码..." -ForegroundColor Cyan
    $existingHashes = @{}
    if (Test-Path $checksumFile) {
        Get-Content $checksumFile | ForEach-Object {
            $line = $_.Trim()
            if (-not [string]::IsNullOrWhiteSpace($line) -and $line -match '^([a-fA-F0-9]{64})\s+\*?(.+)$') {
                $existingHashes[$matches[2].Trim()] = $matches[1].ToLowerInvariant()
            }
        }
    }

    foreach ($pkg in $createdPackages) {
        $name = Split-Path -Leaf $pkg
        $hash = (Get-FileHash -Path $pkg -Algorithm SHA256).Hash.ToLowerInvariant()
        $existingHashes[$name] = $hash
        Write-Host "  $hash  $name" -ForegroundColor Gray
    }

    $hashLines = $existingHashes.Keys | Sort-Object | ForEach-Object {
        "$($existingHashes[$_])  $_"
    }
    $hashLines | Set-Content -Path $checksumFile -Encoding utf8
    Write-Host "  -> 已更新校验清单: $checksumFile" -ForegroundColor DarkGreen
} else {
    Write-Host "`n[3/4 & 4/4] 跳过 Zip 打包与校验步骤 (-SkipZip)" -ForegroundColor Yellow
}

Write-Host "`n==================================================" -ForegroundColor Green
Write-Host " 发布完成!" -ForegroundColor Green
Write-Host " FDD 目录: $fddOut"
Write-Host " SCD 目录: $scdOut"
if (-not $SkipZip) {
    Write-Host " Release 目录: $releaseRoot"
}
Write-Host "==================================================" -ForegroundColor Green
