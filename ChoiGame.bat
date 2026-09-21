@echo off
setlocal EnableExtensions
chcp 65001 >nul
title Tu Tien Du Hanh — khoi dong game

rem Di chuyen ve thu muc chua file .bat (goc repo)
cd /d "%~dp0"

echo ========================================
echo   TU TIEN DU HANH
echo   Mo file nay de choi ngay
echo ========================================
echo.

where dotnet >nul 2>&1
if errorlevel 1 (
  echo [LOI] Chua cai .NET SDK 8.
  echo Tai: https://dotnet.microsoft.com/download/dotnet/8.0
  pause
  exit /b 1
)

where node >nul 2>&1
if errorlevel 1 (
  echo [LOI] Chua cai Node.js.
  echo Tai: https://nodejs.org/
  pause
  exit /b 1
)

if not exist "apps\server\src\TuTien.Api\TuTien.Api.csproj" (
  echo [LOI] Khong thay backend. Dat file nay o goc repo TuTien2D.
  pause
  exit /b 1
)

if not exist "apps\web\package.json" (
  echo [LOI] Khong thay frontend.
  pause
  exit /b 1
)

echo [1/4] Cai npm neu chua co node_modules...
if not exist "apps\web\node_modules\" (
  pushd apps\web
  call npm install
  if errorlevel 1 (
    echo [LOI] npm install that bai.
    popd
    pause
    exit /b 1
  )
  popd
) else (
  echo       Da co node_modules.
)

echo [2/4] Mo API http://localhost:5080
start "TuTien-API" cmd /k "cd /d "%~dp0apps\server\src\TuTien.Api" && echo API dang chay... && dotnet run --urls http://localhost:5080"

echo [3/4] Cho API san sang...
set /a _try=0
:wait_api
set /a _try+=1
if %_try% GTR 40 goto api_timeout
powershell -NoProfile -Command "try { (Invoke-WebRequest -UseBasicParsing http://localhost:5080/health -TimeoutSec 2).StatusCode } catch { exit 1 }" >nul 2>&1
if errorlevel 1 (
  timeout /t 2 /nobreak >nul
  goto wait_api
)
echo       API OK.
goto start_web

:api_timeout
echo       [CANH BAO] API chua tra /health sau ~80s. Van mo web.

:start_web
echo [4/4] Mo web http://localhost:5173
start "TuTien-WEB" cmd /k "cd /d "%~dp0apps\web" && echo WEB dang chay... && npm run dev"

timeout /t 4 /nobreak >nul
start "" "http://localhost:5173"

echo.
echo ========================================
echo Game da mo.
echo   Web : http://localhost:5173
echo   API : http://localhost:5080
echo   Swagger: http://localhost:5080/swagger
echo.
echo Tai khoan thu:
echo   admin  / Admin#123
echo   daoist / Play#123
echo.
echo Dong 2 cua so "TuTien-API" va "TuTien-WEB" de tat game.
echo ========================================
echo.
pause
endlocal
