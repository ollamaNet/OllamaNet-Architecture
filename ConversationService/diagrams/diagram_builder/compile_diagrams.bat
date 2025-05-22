@echo off
setlocal enabledelayedexpansion

REM Set the path to the PlantUML JAR file
set PLANTUML_JAR=C:\plantuml\plantuml.jar

REM Check if PlantUML JAR exists
if not exist "%PLANTUML_JAR%" (
    echo Error: PlantUML JAR not found at %PLANTUML_JAR%
    echo Please install PlantUML and update the path in this script
    pause
    exit /b 1
)

REM Create output directory if it doesn't exist
if not exist "compiled" mkdir compiled

REM Compile each .puml file
for %%f in (*.puml) do (
    echo Compiling %%f...
    java -jar "%PLANTUML_JAR%" -tpng "%%f" -o "compiled"
)

echo.
echo All diagrams have been compiled to the 'compiled' directory
echo.
pause