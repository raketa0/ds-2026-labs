@echo off

echo Starting Valuator instances...
cd /d D:\studies\RP\DISTRIBUTED-PROGRAMMING\Valuator
start cmd /k "dotnet run --urls=http://0.0.0.0:5001"
start cmd /k "dotnet run --urls=http://0.0.0.0:5002"

cd /d D:\studies\RP\DISTRIBUTED-PROGRAMMING\EventsLogger
start cmd /k "dotnet run"
start cmd /k "dotnet run"

cd /d D:\studies\RP\DISTRIBUTED-PROGRAMMING\RankCalculator
start cmd /k "dotnet run"
start cmd /k "dotnet run"

cd /d D:\studies\nginx\nginx-1.28.2\nginx-1.28.2
start nginx.exe

pause