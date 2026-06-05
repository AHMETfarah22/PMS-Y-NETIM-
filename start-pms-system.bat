@echo off
title PMS System Autostart
echo ===================================================
echo PMS System Auto-Start Service (Web & API)
echo ===================================================

:: 1. Check and Start PmsApi (Port 5262)
netstat -ano | findstr ":5262" | findstr "LISTENING" >nul 2>&1
if %errorlevel%==0 (
    echo PmsApi is already running on port 5262.
) else (
    echo Starting PmsApi...
    start /min "PmsApi Service" cmd /c "cd /d C:\Users\dedeg\OneDrive\Desktop\pms\PmsApi && dotnet run --launch-profile http"
)

:: 2. Check and Start PmsWeb (Port 5173)
netstat -ano | findstr ":5173" | findstr "LISTENING" >nul 2>&1
if %errorlevel%==0 (
    echo PmsWeb is already running on port 5173.
) else (
    echo Starting PmsWeb...
    start /min "PmsWeb Service" cmd /c "cd /d C:\Users\dedeg\OneDrive\Desktop\pms\PmsWeb && npm run dev"
)

echo Both services have been verified and are running.
timeout /t 5
exit
