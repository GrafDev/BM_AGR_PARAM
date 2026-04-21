@echo off
setlocal

set SCRIPT_DIR=%~dp0
set VERSION_FILE=%SCRIPT_DIR%version.txt
set ISS_FILE=%SCRIPT_DIR%installer.iss
set INNO=C:\Users\g.yakovlev\AppData\Local\Programs\Inno Setup 6\iscc.exe
set MSBUILD=dotnet

echo ============================================================
echo  BM_AGR_PARAM — Build + Package (TRUE HOT RELOAD)
echo ============================================================

:: 1. Читаем версию
set /p VERSION=<%VERSION_FILE%
set VERSION=%VERSION: =%
echo  Version : %VERSION%

:: 2. Сборка LOADER (Статический загрузчик)
echo.
echo [1/3] Building LOADER...
%MSBUILD% build "%SCRIPT_DIR%BM_AGR_Loader.csproj" -c Release
if errorlevel 1 (
    echo ERROR: Loader build failed!
    pause
    exit /b 1
)

:: 3. Сборка LOGIC (Основная логика)
echo.
echo [2/3] Building LOGIC...
%MSBUILD% build "%SCRIPT_DIR%BM_AGR_PARAM.csproj" -c Release
if errorlevel 1 (
    echo ERROR: Logic build failed!
    pause
    exit /b 1
)
echo Build OK.

:: 4. Компилируем инсталятор
echo.
echo [3/3] Compiling installer...
"%INNO%" /DAppVersion="%VERSION%" "%ISS_FILE%"
if errorlevel 1 (
    echo ERROR: Inno Setup compilation failed!
    pause
    exit /b 1
)

echo.
echo ============================================================
echo  Done! Installer: Build\Installer\BM_AGR_PARAM_v%VERSION%_Setup.exe
echo ============================================================
pause
