# QzRPC Build Script
# Builds self-contained, single-file apps.
#   .\build.ps1            -> Windows + macOS
#   .\build.ps1 -Win       -> Windows only -> dist/QzRPC.exe
#   .\build.ps1 -Mac       -> macOS only   -> dist/QzRPC-macos.zip
#   .\build.ps1 -Clean     -> clean first (combine with -Win / -Mac)

param(
    [string]$Configuration = "Release",
    [switch]$Clean,
    [switch]$Mac,
    [switch]$Win
)

Write-Host "=== QzRPC Build Script ===" -ForegroundColor Cyan
Write-Host ""

# load config
$config = Get-Content "build-config.json" | ConvertFrom-Json

# pick targets (no platform switch = build both)
$buildAll = (-not $Mac) -and (-not $Win)
$targets = @()
if ($Win -or $buildAll) { $targets += "win-x64" }
if ($Mac -or $buildAll) { $targets += "osx-x64" }

# clean if requested
if ($Clean) {
    Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
    Remove-Item -Path "bin", "obj", "dist" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Clean complete!" -ForegroundColor Green
    Write-Host ""
}

function Package-Mac {
    # assemble the .app bundle (icon is the prebuilt build/macos/AppIcon.icns)
    $app = "dist/QzRPC.app"
    Remove-Item -Path $app -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Force -Path "$app/Contents/MacOS" | Out-Null
    New-Item -ItemType Directory -Force -Path "$app/Contents/Resources" | Out-Null
    Copy-Item "build/macos/AppIcon.icns" "$app/Contents/Resources/AppIcon.icns" -Force
    Copy-Item "build/macos/Info.plist" "$app/Contents/Info.plist" -Force
    Copy-Item "dist/QzRPC" "$app/Contents/MacOS/QzRPC" -Force

    # NTFS can't store the Unix executable bit, so pack a zip that carries it in
    # the archive metadata -> the app launches on macOS straight out of the zip.
    $zip = "dist/QzRPC-macos.zip"
    $python = Get-Command python -ErrorAction SilentlyContinue
    if (-not $python) { $python = Get-Command python3 -ErrorAction SilentlyContinue }

    if ($python) {
        if (Test-Path $zip) { Remove-Item -LiteralPath $zip -Force }
        & $python.Source "build/macos/zip_app.py" $app $zip
        Write-Host "Output: dist/QzRPC-macos.zip" -ForegroundColor Cyan
        Write-Host "  (ready for macOS - the executable bit is preserved in the zip)" -ForegroundColor Gray
        Write-Host "  Unzipped bundle also available at dist/QzRPC.app" -ForegroundColor Gray
    } else {
        Write-Host "Output: dist/QzRPC.app" -ForegroundColor Cyan
        Write-Host "Note: Python was not found, so a zip with the executable bit could not" -ForegroundColor Yellow
        Write-Host "      be produced. After copying to macOS, run:" -ForegroundColor Yellow
        Write-Host "        chmod +x dist/QzRPC.app/Contents/MacOS/QzRPC" -ForegroundColor Yellow
    }
}

function Report-Windows {
    Write-Host "Output: dist/QzRPC.exe" -ForegroundColor Cyan
    $exePath = "dist/QzRPC.exe"
    if (Test-Path $exePath) {
        $fileSize = (Get-Item $exePath).Length / 1MB
        Write-Host "File Size: $([math]::Round($fileSize, 2)) MB" -ForegroundColor Gray

        $versionInfo = (Get-Item $exePath).VersionInfo
        Write-Host "Metadata: $($versionInfo.ProductName) v$($versionInfo.FileVersion) - $($versionInfo.CompanyName)" -ForegroundColor Gray
    }
}

# build each requested target
foreach ($rt in $targets) {
    Write-Host "Building QzRPC v$($config.metadata.version) for $rt ($Configuration)..." -ForegroundColor Yellow
    Write-Host ""

    $buildArgs = @(
        "publish",
        "QzRPC.csproj",
        "-c", $Configuration,
        "-r", $rt,
        "-p:PublishSingleFile=$($config.build.singleFile)",
        "-p:SelfContained=$($config.build.selfContained)",
        "-p:PublishTrimmed=$($config.build.trimmed)",
        "-p:EnableCompressionInSingleFile=$($config.build.compressed)",
        "-p:IncludeNativeLibrariesForSelfExtract=true",
        "-p:Version=$($config.metadata.version)",
        "--output", "dist"
    )
    & dotnet $buildArgs

    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "=== Build Failed ($rt) ===" -ForegroundColor Red
        exit 1
    }

    Write-Host ""
    Write-Host "=== $rt Build Successful! ===" -ForegroundColor Green
    if ($rt -eq "osx-x64") { Package-Mac } else { Report-Windows }
    Write-Host ""
}

Write-Host "=== All builds complete ===" -ForegroundColor Green
