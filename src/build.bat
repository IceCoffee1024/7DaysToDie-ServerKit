@echo off
cd /d "%~dp0"
rmdir /s /q "bin"
rmdir /s /q "obj"
dotnet build -c release
if errorlevel 1 (
    echo ERROR: Build failed
    pause
    exit /b 1
)
exit
