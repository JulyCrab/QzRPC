@echo off
REM QzRPC Build Script (Batch wrapper for PowerShell)

powershell -ExecutionPolicy Bypass -File build.ps1 %*
