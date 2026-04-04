# QzRPC Metadata Update Script
# Updates version and metadata in both build-config.json and QobuzRPC.csproj

param(
    [string]$Version,
    [string]$ProductName,
    [string]$Description,
    [string]$Company,
    [string]$Copyright
)

Write-Host "=== QzRPC Metadata Update ===" -ForegroundColor Cyan
Write-Host ""

# load current config
$configPath = "build-config.json"
$config = Get-Content $configPath | ConvertFrom-Json

# update values if provided
$updated = $false

if ($Version) {
    $config.metadata.version = $Version
    Write-Host "Version: $Version" -ForegroundColor Green
    $updated = $true
}

if ($ProductName) {
    $config.metadata.productName = $ProductName
    Write-Host "Product Name: $ProductName" -ForegroundColor Green
    $updated = $true
}

if ($Description) {
    $config.metadata.description = $Description
    Write-Host "Description: $Description" -ForegroundColor Green
    $updated = $true
}

if ($Company) {
    $config.metadata.company = $Company
    $config.metadata.authors = $Company
    Write-Host "Company: $Company" -ForegroundColor Green
    $updated = $true
}

if ($Copyright) {
    $config.metadata.copyright = $Copyright
    Write-Host "Copyright: $Copyright" -ForegroundColor Green
    $updated = $true
}

if (-not $updated) {
    Write-Host "No changes specified. Current metadata:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Product Name: $($config.metadata.productName)" -ForegroundColor Gray
    Write-Host "Description: $($config.metadata.description)" -ForegroundColor Gray
    Write-Host "Company: $($config.metadata.company)" -ForegroundColor Gray
    Write-Host "Version: $($config.metadata.version)" -ForegroundColor Gray
    Write-Host "Copyright: $($config.metadata.copyright)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Usage: .\update-metadata.ps1 -Version '1.1.0' -ProductName 'NewName'" -ForegroundColor Cyan
    exit 0
}

# save config
$config | ConvertTo-Json -Depth 10 | Set-Content $configPath
Write-Host ""
Write-Host "Updated build-config.json" -ForegroundColor Green

# update csproj
$csprojPath = "QzRPC.csproj"
$csproj = [xml](Get-Content $csprojPath)

$propertyGroup = $csproj.Project.PropertyGroup | Where-Object { $_.AssemblyName }

if ($Version) {
    $propertyGroup.Version = $config.metadata.version
    $propertyGroup.AssemblyVersion = "$($config.metadata.version).0"
    $propertyGroup.FileVersion = "$($config.metadata.version).0"
}

if ($ProductName) {
    $propertyGroup.AssemblyName = $config.metadata.productName
    $propertyGroup.Product = $config.metadata.productName
}

if ($Description) {
    $propertyGroup.AssemblyTitle = $config.metadata.description
    $propertyGroup.Description = $config.metadata.description
}

if ($Company) {
    $propertyGroup.Company = $config.metadata.company
    $propertyGroup.Authors = $config.metadata.authors
}

if ($Copyright) {
    $propertyGroup.Copyright = $config.metadata.copyright
}

$csproj.Save($csprojPath)
Write-Host "Updated QobuzRPC.csproj" -ForegroundColor Green
Write-Host ""
Write-Host "=== Metadata Update Complete ===" -ForegroundColor Cyan
