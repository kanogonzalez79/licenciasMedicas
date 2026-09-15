@echo off
setlocal enabledelayedexpansion
cd /d "%~dp0"

echo ============================================
echo   Compilando LicenciasMedicas (Release)
echo ============================================
echo.

where dotnet >nul 2>nul
if errorlevel 1 (
    echo [ERROR] No se encontro "dotnet" en el PATH.
    echo Instala el .NET SDK 8 y volve a intentar: https://dotnet.microsoft.com/download
    echo.
    pause
    exit /b 1
)

where npm >nul 2>nul
if errorlevel 1 (
    echo [ERROR] No se encontro "npm" en el PATH.
    echo Instala Node.js ^(incluye npm^) y volve a intentar: https://nodejs.org/
    echo.
    pause
    exit /b 1
)

set "PROYECTO=%~dp0src\LicenciasMedicas.Web\LicenciasMedicas.Web.csproj"
set "CARPETA_BUILD=%~dp0.build-output"
set "EXE_COMPILADO=%CARPETA_BUILD%\LicenciasMedicas.Web.exe"
set "CARPETA_PUBLISH=%~dp0publish"
set "ZIP_SALIDA=%~dp0LicenciasMedicas-win-x64.zip"

echo Compilando (incluye el build del cliente React, puede demorar unos minutos)...
echo.
dotnet publish "%PROYECTO%" -c Release -o "%CARPETA_BUILD%"
if errorlevel 1 (
    echo.
    echo [ERROR] La compilacion fallo. Revisa los mensajes de arriba.
    pause
    exit /b 1
)

if not exist "%EXE_COMPILADO%" (
    echo.
    echo [ERROR] No se encontro el ejecutable compilado en:
    echo   %EXE_COMPILADO%
    pause
    exit /b 1
)

if not exist "%CARPETA_PUBLISH%" mkdir "%CARPETA_PUBLISH%"

echo.
echo Copiando ejecutable a "%CARPETA_PUBLISH%" (sin tocar data\ ni logs\)...
copy /Y "%EXE_COMPILADO%" "%CARPETA_PUBLISH%\LicenciasMedicas.Web.exe" >nul
if errorlevel 1 (
    echo.
    echo [ERROR] No se pudo copiar el ejecutable a "%CARPETA_PUBLISH%".
    echo Verifica que la aplicacion no este corriendo ^(el .exe podria estar en uso^).
    pause
    exit /b 1
)

echo Generando "%ZIP_SALIDA%"...
powershell -NoProfile -ExecutionPolicy Bypass -Command "Compress-Archive -Path '%EXE_COMPILADO%' -DestinationPath '%ZIP_SALIDA%' -Force"
if errorlevel 1 (
    echo.
    echo [ERROR] No se pudo generar el zip de distribucion.
    pause
    exit /b 1
)

rmdir /s /q "%CARPETA_BUILD%" >nul 2>nul

echo.
echo ============================================
echo   Listo.
echo   - Ejecutable actualizado en: %CARPETA_PUBLISH%\LicenciasMedicas.Web.exe
echo   - Zip de distribucion:       %ZIP_SALIDA%
echo ============================================
echo.
pause
exit /b 0
