@echo off
setlocal enabledelayedexpansion

:: This script sets up the Port Listener to run automatically on Windows startup.
:: It creates a hidden VBScript and places it in your Startup folder.

set "PROJECT_DIR=%~dp0"
set "EXE_PATH=%PROJECT_DIR%publish\PortListener.exe"
set "VBS_SCRIPT=%PROJECT_DIR%StartPortListener.vbs"
set "STARTUP_FOLDER=%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup"

echo.
echo ======================================================
echo   Port Listener Auto-Startup Setup
echo ======================================================
echo.

echo [1/4] Stopping existing Port Listener processes...
taskkill /f /im PortListener.exe /t >nul 2>&1

echo [2/4] Publishing the application in Release mode...
dotnet publish --configuration Release -o "%PROJECT_DIR%publish"
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ ERROR: Failed to publish the application.
    echo Please make sure you have the .NET SDK installed.
    pause
    exit /b %ERRORLEVEL%
)

echo [3/4] Creating hidden launcher script...
(
echo Set WshShell = CreateObject^("WScript.Shell"^)
echo WshShell.CurrentDirectory = "%PROJECT_DIR%"
echo WshShell.Run """%EXE_PATH%""", 0, False
) > "%VBS_SCRIPT%"

echo [4/4] Adding to Startup folder...
copy /y "%VBS_SCRIPT%" "%STARTUP_FOLDER%\StartPortListener.vbs" >nul

echo.
echo [OK] Setup Complete! 
echo.
echo [INFO] Starting the Port Listener now in background...
start "" wscript.exe "%VBS_SCRIPT%"

echo.
echo ======================================================
echo The Port Listener is now RUNNING and will start 
echo automatically whenever the computer starts.
echo.
echo You can verify it is working by visiting:
echo http://localhost:7001/api/data
echo.
echo To stop it, use 'stop_port_listener.bat'
echo ======================================================
echo.
pause
