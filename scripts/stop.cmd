@echo off

taskkill /F /IM dotnet.exe

powershell -Command "Stop-Process -Name nginx -Force" 2>nul

taskkill /F /IM EventsLogger.exe 2>nul

taskkill /F /IM RankCalculator.exe 2>nul

pause