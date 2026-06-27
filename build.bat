@echo off
REM QzRPC Build Script (Batch wrapper for PowerShell)
REM
REM   build.bat              Build Windows + macOS  -> dist\QzRPC.exe + dist\QzRPC-macos.zip
REM   build.bat -Win         Windows only           -> dist\QzRPC.exe
REM   build.bat -Mac         macOS only             -> dist\QzRPC-macos.zip
REM   build.bat -Clean       Clean before building (combine with -Win / -Mac)

powershell -ExecutionPolicy Bypass -File build.ps1 %*
