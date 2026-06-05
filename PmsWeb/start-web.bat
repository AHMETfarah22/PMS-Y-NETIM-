@echo off
title PmsWeb - Vite Server
cd /d "c:\Users\dedeg\OneDrive\Desktop\pms\PmsWeb"

:: Check if port 5173 is already in use
netstat -ano | findstr ":5173" | findstr "LISTENING" >nul 2>&1
if %errorlevel%==0 (
    echo PmsWeb server is already running on port 5173
    timeout /t 3
    exit
)

echo Starting PmsWeb Vite Server...
npm run dev
rem Web sitesinin (http://localhost:5173/) ve Web API'sinin (http://localhost:5262)
