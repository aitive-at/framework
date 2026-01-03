@echo off
setlocal enabledelayedexpansion

:: Check parameters
if "%~1"=="" (
    echo Usage: %~nx0 ^<name^> ^<type^>
    echo Example: %~nx0 Core.Utils classlib
    exit /b 1
)
if "%~2"=="" (
    echo Usage: %~nx0 ^<name^> ^<type^>
    echo Example: %~nx0 Core.Utils classlib
    exit /b 1
)

set "NAME=%~1"
set "TYPE=%~2"
set "SCRIPT_DIR=%~dp0"
set "PROJECT_NAME=Aitive.Framework.%NAME%"
set "TARGET_DIR=%SCRIPT_DIR%..\src\%PROJECT_NAME%"
set "TEMPLATE_DIR=%SCRIPT_DIR%%TYPE%"

:: Check if template exists
if not exist "%TEMPLATE_DIR%\Project.csproj" (
    echo Error: Template not found at %TEMPLATE_DIR%\Project.csproj
    exit /b 1
)

:: Check if project already exists
if exist "%TARGET_DIR%" (
    echo Error: Project folder already exists: %TARGET_DIR%
    exit /b 1
)

:: Create project folder
mkdir "%TARGET_DIR%"
if errorlevel 1 (
    echo Error: Could not create folder %TARGET_DIR%
    exit /b 1
)

:: Copy and rename template
copy "%TEMPLATE_DIR%\Project.csproj" "%TARGET_DIR%\%PROJECT_NAME%.csproj" >nul
if errorlevel 1 (
    echo Error: Could not copy template
    exit /b 1
)

echo Created project: %PROJECT_NAME%
echo Location: %TARGET_DIR%