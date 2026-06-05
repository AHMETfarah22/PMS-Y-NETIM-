@echo off
title PMS Web Sitesi
echo PMS Web Sitesi ve API baslatiliyor... Lutfen bekleyin.
cd /d "%~dp0\Web_ve_Api"
start "" "PmsApi.exe"
timeout /t 3 >nul
echo Tarayicida web sitesi aciliyor...
start http://localhost:5262
exit
