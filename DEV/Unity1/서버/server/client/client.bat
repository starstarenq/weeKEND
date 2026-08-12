```bat
@echo off
cls
title TCP CLIENT

echo ======================
echo      TCP CLIENT
echo ======================
echo.

set /p clientname=Enter your name: 

powershell -NoProfile -Command ^
"$client = New-Object System.Net.Sockets.TcpClient('127.0.0.1',5050); ^
$stream = $client.GetStream(); ^
$writer = New-Object System.IO.StreamWriter($stream); ^
$writer.AutoFlush = $true; ^
$writer.WriteLine('%clientname%'); ^
$writer.Close(); ^
$stream.Close(); ^
$client.Close()"

echo.
echo Name sent to server.
pause
```
