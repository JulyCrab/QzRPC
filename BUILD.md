# QzRPC Build Guide

This project includes automated build scripts for easy compilation and deployment.

## Quick Start

### Windows (PowerShell)
```powershell
.\build.ps1
```

### Windows (Command Prompt)
```cmd
build.bat
```

### Linux/Mac
```bash
chmod +x build.sh
./build.sh
```

## Build Output

The build creates a **self-contained, single-file executable** at:
```
dist/QzRPC.exe
```

This executable:
- Includes the .NET runtime (no installation required)
- Contains all dependencies in one file
- Is compressed for smaller file size
- Works on any Windows x64 system

## Build Options

### Clean Build
Removes all previous build artifacts before building:

```powershell
.\build.ps1 -Clean
```

```bash
./build.sh --clean
```

### Debug Build
```bash
./build.sh --debug
```

## Updating Metadata

Use the metadata update script to change version, product name, etc:

```powershell
# update version
.\update-metadata.ps1 -Version "1.1.0"

# update multiple fields
.\update-metadata.ps1 -Version "2.0.0" -Company "NewCompany" -Copyright "Copyright © 2026 NewCompany"

# view current metadata
.\update-metadata.ps1
```

### Available Parameters
- `-Version` - Version number (e.g., "1.0.0")
- `-ProductName` - Product name (e.g., "QzRPC")
- `-Description` - File description
- `-Company` - Company/author name
- `-Copyright` - Copyright text

The script updates both `build-config.json` and `QzRPC.csproj` automatically.

## Configuration File

Edit `build-config.json` to customize build settings:

```json
{
  "metadata": {
    "productName": "QzRPC",
    "description": "Unofficial Rich Presence client for a music streaming service",
    "company": "JulyCrab",
    "version": "1.0.0"
  },
  "build": {
    "targetFramework": "net10.0",
    "runtimeIdentifier": "win-x64",
    "selfContained": true,
    "singleFile": true,
    "compressed": true
  }
}
```

## Manual Build

If you prefer to build manually:

```bash
dotnet publish QzRPC.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true --output dist
```

## File Size

The self-contained executable might be bigger because it includes:
- .NET 10 runtime
- Avalonia UI framework
- All dependencies
- Application code

This is normal for self-contained .NET applications and ensures the app runs without requiring .NET installation.

## Troubleshooting

### "dotnet command not found"
Install .NET SDK from: https://dotnet.microsoft.com/download

### Build fails with "SDK not found"
Make sure .NET 10 SDK is installed:
```bash
dotnet --list-sdks
```

### PowerShell execution policy error
Run PowerShell as Administrator and execute:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```
