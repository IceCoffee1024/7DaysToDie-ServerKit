@echo off
chcp 65001
setlocal enabledelayedexpansion

echo Attempting to restart the server...
tasklist /fi "PID eq %~1" | findstr "%~1" >nul
if %errorlevel% equ 0 (
    taskkill /f /pid %~1 >nul 2>&1
    if !errorlevel! equ 0 (
        echo Process %~1 terminated
    ) else (
        echo Error: Process termination failed (Insufficient permissions?)
    )
    timeout /t 2 /nobreak >nul
)

start "" /D "%~dp2" "%~nx2" >nul
if %errorlevel% neq 0 (
    echo Error: Failed to start server [%~2]
    exit /b 1
)

echo Server restart operation completed!
:exit
