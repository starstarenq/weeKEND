```bat
@echo off
cls
title TCP SERVER 5050

echo ==============================
echo        TCP SERVER 5050
echo ==============================
echo.

if not exist node_modules (
    echo Installing dependencies...
    call npm install typescript ts-node @types/node
)

echo Starting server...
echo.

call npx ts-node server.ts

echo.
echo Server stopped.
pause
```
