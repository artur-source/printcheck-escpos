@echo off
setlocal enabledelayedexpansion

echo ============================================================
echo      PrintCheck v2.0.1 - Script de Compilacao Automatica
echo ============================================================
echo.

echo [1/3] Limpando builds anteriores...
if exist "src\bin" rmdir /s /q "src\bin"
if exist "src\obj" rmdir /s /q "src\obj"
echo OK.

echo.
echo [2/3] Compilando o projeto (Release win-x64)...
cd src
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

if %errorlevel% neq 0 (
    echo.
    echo !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    echo ERRO: A compilacao falhou. 
    echo Verifique se o .NET SDK 8.0 esta instalado em sua maquina.
    echo !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    pause
    exit /b 1
)

echo.
echo [3/3] Compilacao concluida com sucesso!
echo Iniciando o PrintCheck v2.0.1...
cd ..
start "" "src\bin\Release\net8.0-windows\win-x64\publish\PrintCheck.exe"

echo.
echo Script finalizado. O log sera gerado em: discovery_log.txt
timeout /t 5
exit /b 0
