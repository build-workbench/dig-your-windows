<#
.SYNOPSIS
    Compiles and runs test suite for DigYourWindows.
.DESCRIPTION
    Standardized build and test script for developers and CI environments.
.PARAMETER Configuration
    Build configuration (default: Release).
.PARAMETER Clean
    Performs a clean before restoring and building.
.PARAMETER NoTest
    Skips running the test suite after build.
.PARAMETER Coverage
    Collects code coverage metrics into artifacts/reports/coverage/.
.PARAMETER Filter
    Test filter expression (e.g. "Category=Unit").
.EXAMPLE
    ./scripts/build.ps1
.EXAMPLE
    ./scripts/build.ps1 -Configuration Release -Coverage
#>
[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [switch]$Clean,
    [switch]$NoTest,
    [switch]$Coverage,
    [string]$Filter = ""
)

$ErrorActionPreference = "Stop"
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

$repoRoot = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $repoRoot "DigYourWindows.slnx"

Write-Host "==================================================" -ForegroundColor Green
Write-Host " DigYourWindows - Build & Verify" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Green
Write-Host " Solution:      $solutionPath"
Write-Host " Configuration: $Configuration"
Write-Host " Run Tests:     $(-not $NoTest)"
Write-Host " Coverage:      $Coverage"
Write-Host "--------------------------------------------------"

if ($Clean) {
    Write-Host "`n[0/3] 清理构建目标缓存..." -ForegroundColor Cyan
    dotnet clean $solutionPath -c $Configuration
    if ($LASTEXITCODE -ne 0) { throw "dotnet clean 失败" }
}

Write-Host "`n[1/3] 还原 NuGet 依赖..." -ForegroundColor Cyan
dotnet restore $solutionPath
if ($LASTEXITCODE -ne 0) { throw "dotnet restore 失败" }

Write-Host "`n[2/3] 编译解决方案..." -ForegroundColor Cyan
dotnet build $solutionPath -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) { throw "dotnet build 失败" }

if (-not $NoTest) {
    Write-Host "`n[3/3] 运行自动化测试..." -ForegroundColor Cyan
    $testArgs = @("test", $solutionPath, "-c", $Configuration, "--no-restore")

    if (-not [string]::IsNullOrWhiteSpace($Filter)) {
        $testArgs += @("--filter", $Filter)
    }

    if ($Coverage) {
        $coverageDir = Join-Path $repoRoot "artifacts\reports\coverage"
        New-Item -ItemType Directory -Force -Path $coverageDir | Out-Null
        $testArgs += @(
            "--collect", "XPlat Code Coverage",
            "--results-directory", $coverageDir
        )
    }

    & dotnet @testArgs
    if ($LASTEXITCODE -ne 0) { throw "测试失败" }
} else {
    Write-Host "`n[3/3] 跳过测试运行 (-NoTest)" -ForegroundColor Yellow
}

$stopwatch.Stop()
Write-Host "`n==================================================" -ForegroundColor Green
Write-Host " 全部完成! 耗时: $($stopwatch.Elapsed.ToString('mm\:ss'))" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Green
