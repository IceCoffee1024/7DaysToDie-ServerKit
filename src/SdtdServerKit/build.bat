@echo off
cd /d "%~dp0"

dotnet build -c release
if errorlevel 1 (
    echo ERROR: Build failed
    pause
    exit /b 1
)
exit
