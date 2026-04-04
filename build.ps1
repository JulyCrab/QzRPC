# QzRPC Build Script
# Builds a self-contained, single-file executable

param(
    [string]$Configuration = "Release",
    [switch]$Clean
)

Write-Host "=== QzRPC Build Script ===" -ForegroundColor Cyan
Write-Host ""

# load config
$config = Get-Content "build-config.json" | ConvertFrom-Json

# clean if requested
if ($Clean) {
    Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
    Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "../dist" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Clean complete!" -ForegroundColor Green
    Write-Host ""
}

# build
Write-Host "Building QzRPC v$($config.metadata.version)..." -ForegroundColor Yellow
Write-Host "Configuration: $Configuration" -ForegroundColor Gray
Write-Host "Runtime: $($config.build.runtimeIdentifier)" -ForegroundColor Gray
Write-Host "Self-Contained: $($config.build.selfContained)" -ForegroundColor Gray
Write-Host ""

$buildArgs = @(
    "publish",
    "QzRPC.csproj",
    "-c", $Configuration,
    "-r", $config.build.runtimeIdentifier,
    "-p:PublishSingleFile=$($config.build.singleFile)",
    "-p:SelfContained=$($config.build.selfContained)",
    "-p:PublishTrimmed=$($config.build.trimmed)",
    "-p:EnableCompressionInSingleFile=$($config.build.compressed)",
    "-p:IncludeNativeLibrariesForSelfExtract=true",
    "-p:Version=$($config.metadata.version)",
    "--output", "../dist"
)

& dotnet $buildArgs

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "=== Build Successful! ===" -ForegroundColor Green
    Write-Host ""
    Write-Host "Output: dist/QzRPC.exe" -ForegroundColor Cyan
    
    # show file size
    $exePath = "../dist/QzRPC.exe"
    if (Test-Path $exePath) {
        $fileSize = (Get-Item $exePath).Length / 1MB
        Write-Host "File Size: $([math]::Round($fileSize, 2)) MB" -ForegroundColor Gray
        
        # show metadata
        $versionInfo = (Get-Item $exePath).VersionInfo
        Write-Host ""
        Write-Host "Metadata:" -ForegroundColor Yellow
        Write-Host "  Product Name: $($versionInfo.ProductName)" -ForegroundColor Gray
        Write-Host "  Description: $($versionInfo.FileDescription)" -ForegroundColor Gray
        Write-Host "  Company: $($versionInfo.CompanyName)" -ForegroundColor Gray
        Write-Host "  Version: $($versionInfo.FileVersion)" -ForegroundColor Gray
        Write-Host "  Copyright: $($versionInfo.LegalCopyright)" -ForegroundColor Gray
    }
} else {
    Write-Host ""
    Write-Host "=== Build Failed ===" -ForegroundColor Red
    exit 1
}
