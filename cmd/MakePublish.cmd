@echo off
setlocal EnableExtensions

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%..") do set "ROOT_DIR=%%~fI"

set "VERSION_FILE=%ROOT_DIR%\src\version.txt"
set "OUT_DIR=%ROOT_DIR%\out\publish"
set "PUBLISH_DIR=%OUT_DIR%\AntiDupl.NET"
set "RAR_EXE=C:\Program Files\WinRAR\WinRar.exe"
set "SEVENZIP=%SCRIPT_DIR%7-zip\7za_2201.exe"
set "VERSION="

if not exist "%VERSION_FILE%" (
    echo ERROR: Cannot find "%VERSION_FILE%".
    exit /b 1
)

for /f "usebackq delims=" %%I in ("%VERSION_FILE%") do if not defined VERSION set "VERSION=%%I"
if not defined VERSION (
    echo ERROR: The version file is empty.
    exit /b 1
)

set "TMP_DIR=%OUT_DIR%\AntiDupl.NET-%VERSION%"
set "EXE_OUT=%OUT_DIR%\AntiDupl.NET-%VERSION%_SingleFilePortable.exe"
set "ZIP_OUT=%OUT_DIR%\AntiDupl.NET-%VERSION%_SingleFilePortable.zip"
set "EXE_HASH=%EXE_OUT%.hash.txt"
set "ZIP_HASH=%ZIP_OUT%.hash.txt"

if not exist "%PUBLISH_DIR%\" (
    echo ERROR: Publish directory not found: "%PUBLISH_DIR%".
    exit /b 1
)

if not exist "%ROOT_DIR%\bin\Publish\AntiDupl.dll" (
    echo ERROR: Required native library not found: "%ROOT_DIR%\bin\Publish\AntiDupl.dll".
    exit /b 1
)

if not exist "%ROOT_DIR%\bin\Publish\AntiDupl.NET.WinForms.runtimeconfig.json" (
    echo ERROR: WinForms runtime configuration file not found.
    exit /b 1
)

if not exist "%ROOT_DIR%\bin\Publish\AntiDupl.NET.WPF.runtimeconfig.json" (
    echo ERROR: WPF runtime configuration file not found.
    exit /b 1
)

if not exist "%OUT_DIR%\" (
    mkdir "%OUT_DIR%"
    if errorlevel 1 goto :fail
)

if exist "%TMP_DIR%\" (
    rmdir /s /q "%TMP_DIR%"
    if errorlevel 1 goto :fail
)
if exist "%TMP_DIR%\" (
    echo ERROR: Cannot remove the previous temporary package directory.
    exit /b 1
)
mkdir "%TMP_DIR%"
if errorlevel 1 goto :fail

rem Copy application resources. Robocopy exit codes below 8 indicate success.
robocopy "%ROOT_DIR%\data\resources" "%PUBLISH_DIR%\data\resources" /MIR
if errorlevel 8 goto :fail

robocopy "%ROOT_DIR%\docs\data\resources" "%PUBLISH_DIR%\data\resources" /S
if errorlevel 8 goto :fail

copy /Y "%ROOT_DIR%\bin\Publish\AntiDupl.dll" "%PUBLISH_DIR%\" >nul
if errorlevel 1 goto :fail

if exist "%ROOT_DIR%\bin\Publish\AntiDupl.pdb" (
    copy /Y "%ROOT_DIR%\bin\Publish\AntiDupl.pdb" "%PUBLISH_DIR%\" >nul
    if errorlevel 1 goto :fail
)

copy /Y "%ROOT_DIR%\bin\Publish\AntiDupl.NET.WinForms.runtimeconfig.json" "%PUBLISH_DIR%\" >nul
if errorlevel 1 goto :fail

copy /Y "%ROOT_DIR%\bin\Publish\AntiDupl.NET.WPF.runtimeconfig.json" "%PUBLISH_DIR%\" >nul
if errorlevel 1 goto :fail

robocopy "%PUBLISH_DIR%" "%TMP_DIR%" /E
if errorlevel 8 goto :fail

rem Remove old archives first so 7-Zip or WinRAR cannot append stale files.
for %%F in ("%EXE_OUT%" "%ZIP_OUT%" "%EXE_HASH%" "%ZIP_HASH%") do (
    if exist "%%~F" del /F /Q "%%~F"
    if exist "%%~F" goto :fail
)

rem Run from the script directory so the bundled 7-Zip SFX module is found.
pushd "%SCRIPT_DIR%"
if errorlevel 1 goto :fail

set "TMP_REL=..\out\publish\AntiDupl.NET-%VERSION%"

if exist "%RAR_EXE%" (
    "%RAR_EXE%" a -ep1 -s -m5 -r -sfx "%EXE_OUT%" "%TMP_DIR%"
    if errorlevel 1 (
        popd
        goto :fail
    )

    "%RAR_EXE%" a -afzip -ep1 -r "%ZIP_OUT%" "%TMP_DIR%"
    if errorlevel 1 (
        popd
        goto :fail
    )
) else (
    if not exist "%SEVENZIP%" (
        popd
        echo ERROR: Neither WinRAR nor the bundled 7-Zip executable is available.
        exit /b 1
    )

    "%SEVENZIP%" a -sfx7z.sfx "%EXE_OUT%" ".\%TMP_REL%\*"
    if errorlevel 1 (
        popd
        goto :fail
    )

    "%SEVENZIP%" a -tzip "%ZIP_OUT%" ".\%TMP_REL%\*"
    if errorlevel 1 (
        popd
        goto :fail
    )
)

popd

call :WriteHash "%EXE_OUT%" "%EXE_HASH%"
if errorlevel 1 goto :fail

call :WriteHash "%ZIP_OUT%" "%ZIP_HASH%"
if errorlevel 1 goto :fail

echo.
echo Package created successfully:
echo   %EXE_OUT%
echo   %ZIP_OUT%
echo.
exit /b 0

:WriteHash
if not exist "%~1" exit /b 1
certutil -hashfile "%~1" SHA256 > "%~2"
if errorlevel 1 exit /b 1
if not exist "%~2" exit /b 1
exit /b 0

:fail
echo.
echo ERROR: SingleFilePortable packaging failed. Check the messages above.
exit /b 1
