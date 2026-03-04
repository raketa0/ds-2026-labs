@echo off

taskkill /F /IM dotnet.exe

powershell -Command "Stop-Process -Name nginx -Force"
pause