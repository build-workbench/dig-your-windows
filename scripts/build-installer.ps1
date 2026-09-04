<#
.SYNOPSIS
    Builds the Inno Setup Windows installer for DigYourWindows.
.DESCRIPTION
    Compiles the Self-Contained Deployment (SCD) into an Inno Setup installer (.exe)
    and places it in the standardized artifacts/release directory alongside SHA-256 checksums.
.PARAMETER Configuration
    Build configuration (default: Release).
.PARAMETER Runtime
    Target runtime identifier (default: win-x64).
.PARAMETER AppName
    Application display name (default: DigYourWindows).
.PARAMETER AppPublisher
    Publisher name (default: Build Workbench).
.PARAMETER Version
    Application version override (defaults to Version in Directory.Build.props).
.PARAMETER OutputDir
    Directory where installer .exe will be placed (default: <repoRoot>/artifacts/release).
.PARAMETER PublishDir
    Pre-published SCD directory (default: <repoRoot>/artifacts/publish/scd_<Runtime>).
.PARAMETER ForceRebuildPublish
    Forces re-running dotnet publish even if PublishDir already exists.
.EXAMPLE
    ./scripts/build-installer.ps1
.EXAMPLE
    ./scripts/build-installer.ps1 -Configuration Release -Runtime win-x64 -Version 1.2.0
#>
[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$AppName = "DigYourWindows",
    [string]$AppPublisher = "Build Workbench",
    [string]$Version = "",
    [string]$OutputDir = "",
    [string]$PublishDir = "",
    [switch]$ForceRebuildPublish
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "src\DigYourWindows.UI\DigYourWindows.UI.csproj"
$issPath = Join-Path $repoRoot "installer\DigYourWindows.iss"
$propsPath = Join-Path $repoRoot "Directory.Build.props"

if (-not (Test-Path $projectPath)) {
    throw "未找到主入口项目: $projectPath"
}

if (-not (Test-Path $issPath)) {
    throw "未找到 Inno Setup 脚本: $issPath"
}

# Auto-detect version from Directory.Build.props if not specified
if ([string]::IsNullOrWhiteSpace($Version) -and (Test-Path $propsPath)) {
    $propsContent = Get-Content $propsPath -Raw
    if ($propsContent -match '<Version>([^<]+)</Version>') {
        $Version = $matches[1].Trim()
    }
}

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $repoRoot "artifacts\release"
}

if ([string]::IsNullOrWhiteSpace($PublishDir)) {
    $PublishDir = Join-Path $repoRoot "artifacts\publish\scd_$Runtime"
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

Write-Host "==================================================" -ForegroundColor Green
Write-Host " DigYourWindows - Installer Build" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Green
Write-Host " Project:       $projectPath"
Write-Host " Configuration: $Configuration"
Write-Host " Runtime:       $Runtime"
Write-Host " PublishDir:    $PublishDir"
Write-Host " OutputDir:     $OutputDir"
Write-Host "--------------------------------------------------"

$exeName = "DigYourWindows.UI.exe"
$exePath = Join-Path $PublishDir $exeName
$needsPublish = $ForceRebuildPublish -or (-not (Test-Path $exePath))

if ($needsPublish) {
    Write-Host "`n[1/3] 执行独立运行版发布 (SCD)..." -ForegroundColor Cyan
    New-Item -ItemType Directory -Force -Path $PublishDir | Out-Null
    $pubArgs = @(
        "publish", $projectPath,
        "-c", $Configuration,
        "-r", $Runtime,
        "--self-contained", "true"
    )
    if (-not [string]::IsNullOrWhiteSpace($Version)) {
        $pubArgs += "-p:Version=$Version"
    }
    $pubArgs += @("-o", $PublishDir)
    & dotnet @pubArgs
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish 失败" }
} else {
    Write-Host "`n[1/3] 复用已有的独立发布产物: $PublishDir" -ForegroundColor Yellow
}

if (-not (Test-Path $exePath)) {
    $candidate = Get-ChildItem -Path $PublishDir -Filter "*.exe" -File | Select-Object -First 1
    if ($null -ne $candidate) {
        $exeName = $candidate.Name
        $exePath = $candidate.FullName
    } else {
        throw "未在发布目录中找到可执行文件: $PublishDir"
    }
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    $fileVer = (Get-Item $exePath).VersionInfo.ProductVersion
    if ([string]::IsNullOrWhiteSpace($fileVer)) {
        $fileVer = (Get-Item $exePath).VersionInfo.FileVersion
    }
    $Version = if (-not [string]::IsNullOrWhiteSpace($fileVer)) {
        $fileVer.Trim().Split('+')[0].Split(' ')[0]
    } else {
        "1.2.0"
    }
}

$versionForFile = ($Version -replace "[^0-9A-Za-z\.\-_]", "_")

Write-Host " 检测到程序: $exeName"
Write-Host " 目标版本号: $Version"

# Locate ISCC.exe
Write-Host "`n[2/3] 探测 Inno Setup 编译器 (ISCC.exe)..." -ForegroundColor Cyan
$iscc = $null
$cmd = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
if ($null -ne $cmd) {
    $iscc = $cmd.Source
}

if ([string]::IsNullOrWhiteSpace($iscc)) {
    $candidates = @()
    if (-not [string]::IsNullOrWhiteSpace($env:LOCALAPPDATA)) {
        $candidates += (Join-Path $env:LOCALAPPDATA "Programs\Inno Setup 6\ISCC.exe")
    }
    if (-not [string]::IsNullOrWhiteSpace(${env:ProgramFiles(x86)})) {
        $candidates += (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe")
    }
    if (-not [string]::IsNullOrWhiteSpace($env:ProgramFiles)) {
        $candidates += (Join-Path $env:ProgramFiles "Inno Setup 6\ISCC.exe")
    }

    foreach ($c in $candidates) {
        if (Test-Path $c) {
            $iscc = $c
            break
        }
    }
}

if ([string]::IsNullOrWhiteSpace($iscc)) {
    $regPaths = @(
        'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*',
        'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*',
        'HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*'
    )
    foreach ($rp in $regPaths) {
        $entry = Get-ItemProperty $rp -ErrorAction SilentlyContinue |
            Where-Object { $_.DisplayName -like 'Inno Setup*' -and -not [string]::IsNullOrWhiteSpace($_.InstallLocation) } |
            Select-Object -First 1
        if ($null -ne $entry) {
            $candidate = Join-Path $entry.InstallLocation 'ISCC.exe'
            if (Test-Path $candidate) {
                $iscc = $candidate
                break
            }
        }
    }
}

if ([string]::IsNullOrWhiteSpace($iscc) -or -not (Test-Path $iscc)) {
    Write-Warning "未找到 ISCC.exe (Inno Setup 6)。如果需要编译安装包，请安装 Inno Setup 6。"
    Write-Host "跳过安装程序生成。可在安装 Inno Setup 6 后重试。" -ForegroundColor Yellow
    exit 0
}

Write-Host " 找到 ISCC: $iscc"

# Build Installer
Write-Host "`n[3/3] 编译 Windows 安装程序..." -ForegroundColor Cyan
$outputBaseFilename = "${AppName}_Setup_${Runtime}_v${versionForFile}"
$isccArgs = @(
    $issPath,
    "/DAppName=$AppName",
    "/DAppPublisher=$AppPublisher",
    "/DAppVersion=$Version",
    "/DAppExeName=$exeName",
    "/DPublishDir=$PublishDir",
    "/DOutputDir=$OutputDir",
    "/DOutputBaseFilename=$outputBaseFilename"
)

& $iscc @isccArgs
if ($LASTEXITCODE -ne 0) { throw "ISCC 编译失败" }

$installerFile = Join-Path $OutputDir "${outputBaseFilename}.exe"
if (Test-Path $installerFile) {
    # Compute SHA-256 and update SHA256SUMS.txt
    $checksumFile = Join-Path $OutputDir "SHA256SUMS.txt"
    $existingHashes = @{}
    if (Test-Path $checksumFile) {
        Get-Content $checksumFile | ForEach-Object {
            $line = $_.Trim()
            if (-not [string]::IsNullOrWhiteSpace($line) -and $line -match '^([a-fA-F0-9]{64})\s+\*?(.+)$') {
                $existingHashes[$matches[2].Trim()] = $matches[1].ToLowerInvariant()
            }
        }
    }

    $hash = (Get-FileHash -Path $installerFile -Algorithm SHA256).Hash.ToLowerInvariant()
    $installerName = Split-Path -Leaf $installerFile
    $existingHashes[$installerName] = $hash

    $hashLines = $existingHashes.Keys | Sort-Object | ForEach-Object {
        "$($existingHashes[$_])  $_"
    }
    $hashLines | Set-Content -Path $checksumFile -Encoding utf8
    Write-Host " 已更新校验清单: $checksumFile" -ForegroundColor DarkGreen
    Write-Host " 安装程序 Hash: $hash" -ForegroundColor Gray
}

Write-Host "`n==================================================" -ForegroundColor Green
Write-Host " 安装程序构建成功!" -ForegroundColor Green
Write-Host " 输出目录: $OutputDir"
Write-Host " 安装包:   ${outputBaseFilename}.exe"
Write-Host "==================================================" -ForegroundColor Green
