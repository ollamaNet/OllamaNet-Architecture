@echo off
setlocal enabledelayedexpansion

REM Set base directories
set BASE_DIR=..
set OUTPUT_DIR=..\compiled
set TOOLS_DIR=tools
set PLANTUML_VERSION=1.2024.3
set PLANTUML_JAR=%TOOLS_DIR%\plantuml-%PLANTUML_VERSION%.jar
set PLANTUML_DOWNLOAD_URL=https://github.com/plantuml/plantuml/releases/download/v%PLANTUML_VERSION%/plantuml-%PLANTUML_VERSION%.jar

REM Create tools directory if it doesn't exist
if not exist "%TOOLS_DIR%" mkdir "%TOOLS_DIR%"

REM Check if PlantUML JAR exists, download if not
if not exist "%PLANTUML_JAR%" (
    echo PlantUML JAR not found. Downloading version %PLANTUML_VERSION%...
    curl -L "%PLANTUML_DOWNLOAD_URL%" -o "%PLANTUML_JAR%"
    
    if not exist "%PLANTUML_JAR%" (
        echo Error: Failed to download PlantUML JAR.
        echo Please download manually from: %PLANTUML_DOWNLOAD_URL%
        echo And place it in: %TOOLS_DIR%\plantuml-%PLANTUML_VERSION%.jar
        pause
        exit /b 1
    ) else (
        echo PlantUML JAR downloaded successfully.
    )
)

REM Create output directory if it doesn't exist
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

REM Create output subdirectories for all diagram types
if not exist "%OUTPUT_DIR%\class_diagrams" mkdir "%OUTPUT_DIR%\class_diagrams"
if not exist "%OUTPUT_DIR%\component_diagrams" mkdir "%OUTPUT_DIR%\component_diagrams"
if not exist "%OUTPUT_DIR%\container_diagrams" mkdir "%OUTPUT_DIR%\container_diagrams"
if not exist "%OUTPUT_DIR%\context_diagrams" mkdir "%OUTPUT_DIR%\context_diagrams"
if not exist "%OUTPUT_DIR%\data_flow_diagrams" mkdir "%OUTPUT_DIR%\data_flow_diagrams"
if not exist "%OUTPUT_DIR%\integration_diagrams" mkdir "%OUTPUT_DIR%\integration_diagrams"
if not exist "%OUTPUT_DIR%\infrastructure_diagrams" mkdir "%OUTPUT_DIR%\infrastructure_diagrams"
if not exist "%OUTPUT_DIR%\sequence_diagrams" mkdir "%OUTPUT_DIR%\sequence_diagrams"
if not exist "%OUTPUT_DIR%\state_machine_diagrams" mkdir "%OUTPUT_DIR%\state_machine_diagrams"

echo Compiling class diagrams...
for %%f in ("%BASE_DIR%\class_diagrams\*.puml") do (
    echo   Processing %%~nxf...
    java -jar "%PLANTUML_JAR%" -tpng "%%f" -o "%OUTPUT_DIR%\class_diagrams"
)

echo Compiling component diagrams...
for %%f in ("%BASE_DIR%\component_diagrams\*.puml") do (
    echo   Processing %%~nxf...
    java -jar "%PLANTUML_JAR%" -tpng "%%f" -o "%OUTPUT_DIR%\component_diagrams"
)

echo Compiling container diagrams...
for %%f in ("%BASE_DIR%\container_diagrams\*.puml") do (
    echo   Processing %%~nxf...
    java -jar "%PLANTUML_JAR%" -tpng "%%f" -o "%OUTPUT_DIR%\container_diagrams"
)

echo Compiling context diagrams...
for %%f in ("%BASE_DIR%\context_diagrams\*.puml") do (
    echo   Processing %%~nxf...
    java -jar "%PLANTUML_JAR%" -tpng "%%f" -o "%OUTPUT_DIR%\context_diagrams"
)

echo Compiling data flow diagrams...
for %%f in ("%BASE_DIR%\data_flow_diagrams\*.puml") do (
    echo   Processing %%~nxf...
    java -jar "%PLANTUML_JAR%" -tpng "%%f" -o "%OUTPUT_DIR%\data_flow_diagrams"
)

echo Compiling infrastructure diagrams...
for %%f in ("%BASE_DIR%\infrastructure_diagrams\*.puml") do (
    echo   Processing %%~nxf...
    java -jar "%PLANTUML_JAR%" -tpng "%%f" -o "%OUTPUT_DIR%\infrastructure_diagrams"
)

echo Compiling integration diagrams...
for %%f in ("%BASE_DIR%\integration_diagrams\*.puml") do (
    echo   Processing %%~nxf...
    java -jar "%PLANTUML_JAR%" -tpng "%%f" -o "%OUTPUT_DIR%\integration_diagrams"
)

echo Compiling sequence diagrams...
for %%f in ("%BASE_DIR%\sequence_diagrams\*.puml") do (
    echo   Processing %%~nxf...
    java -jar "%PLANTUML_JAR%" -tpng "%%f" -o "%OUTPUT_DIR%\sequence_diagrams"
)

echo Compiling state machine diagrams...
for %%f in ("%BASE_DIR%\state_machine_diagrams\*.puml") do (
    echo   Processing %%~nxf...
    java -jar "%PLANTUML_JAR%" -tpng "%%f" -o "%OUTPUT_DIR%\state_machine_diagrams"
)

echo.
echo ----------------------------------------------------
echo All diagrams have been compiled to the 'compiled' directory.
echo Directory structure has been preserved.
echo.
echo Summary of compiled diagrams:
echo ----------------------------------------------------
set TOTAL=0
for /D %%d in ("%OUTPUT_DIR%\*") do (
    set COUNT=0
    for %%f in ("%%d\*.png") do set /a COUNT+=1
    echo %%~nxd: !COUNT! diagrams compiled
    set /a TOTAL+=!COUNT!
)
echo ----------------------------------------------------
echo Total: %TOTAL% diagrams compiled
echo ----------------------------------------------------
echo.
pause