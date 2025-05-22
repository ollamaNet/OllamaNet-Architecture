@echo off
setlocal enabledelayedexpansion

echo ========================================
echo Diagram Tools Deployment Script
echo ========================================
echo.

REM Get the target project directory
set /p TARGET_DIR="Enter the target project directory path: "

REM Check if the target directory exists
if not exist "%TARGET_DIR%" (
    echo Error: Target directory does not exist.
    echo Please enter a valid directory path.
    pause
    exit /b 1
)

echo.
echo Installing diagram tools to %TARGET_DIR%...
echo.

REM Create necessary directories in the target project
if not exist "%TARGET_DIR%\diagrams" mkdir "%TARGET_DIR%\diagrams"
if not exist "%TARGET_DIR%\diagrams\diagram_builder" mkdir "%TARGET_DIR%\diagrams\diagram_builder"
if not exist "%TARGET_DIR%\diagrams\output" mkdir "%TARGET_DIR%\diagrams\output"
if not exist "%TARGET_DIR%\diagrams\diagram_builder\templates" mkdir "%TARGET_DIR%\diagrams\diagram_builder\templates"

REM Copy files from the current structure to the target
echo.
echo Copying diagram tools...

REM Copy batch files and documentation to diagram_builder
xcopy /E /Y "*.bat" "%TARGET_DIR%\diagrams\diagram_builder\"
xcopy /E /Y "*.md" "%TARGET_DIR%\diagrams\diagram_builder\"
xcopy /E /Y "project_cursorrules" "%TARGET_DIR%\diagrams\diagram_builder\"

REM Copy any template files if they exist
if exist "templates\*.*" xcopy /E /Y "templates\*.*" "%TARGET_DIR%\diagrams\diagram_builder\templates\"

REM Copy any existing .puml files from parent directory to target
cd ..
if exist "*.puml" xcopy /Y "*.puml" "%TARGET_DIR%\diagrams\"

REM Ask if user wants to install the .cursorrules file
echo.
echo Do you want to install the .cursorrules file in the project root?
set /p INSTALL_RULES="This will help Cursor AI generate accurate diagrams (Y/N): "

if /i "%INSTALL_RULES%"=="Y" (
    echo.
    echo Choose which rules file to use:
    echo 1. Project-specific rules (from project_cursorrules)
    echo 2. Generic template (from global_cursorrules_template.md)
    set /p RULES_CHOICE="Enter choice (1 or 2): "
    
    if "%RULES_CHOICE%"=="1" (
        echo Installing project-specific rules...
        copy "project_cursorrules" "%TARGET_DIR%\.cursorrules"
    ) else if "%RULES_CHOICE%"=="2" (
        echo Preparing generic rules template...
        
        REM Extract the content between the triple backticks from global_cursorrules_template.md
        REM This is a simplified version - in practice you may need more robust parsing
        type "global_cursorrules_template.md" | findstr /v "```" | findstr /v "# Global Cursor Rules Template" > "%TARGET_DIR%\.cursorrules"
        
        echo Generic rules template installed.
        echo IMPORTANT: Edit the Project-Specific Guidelines section in .cursorrules!
    ) else (
        echo Invalid choice. No rules file installed.
    )
)

echo.
echo ========================================
echo Installation Complete
echo ========================================
echo.
echo The diagram generation tools have been installed to:
echo %TARGET_DIR%\diagrams
echo.

if /i "%INSTALL_RULES%"=="Y" (
    echo The .cursorrules file has been installed to:
    echo %TARGET_DIR%\.cursorrules
    echo.
)

echo Next steps:
echo 1. Navigate to the diagram_builder directory: cd %TARGET_DIR%\diagrams\diagram_builder
echo 2. Run generate_diagrams.bat to create template files
echo 3. Create or modify .puml files in the main diagrams directory
echo 4. Run generate_diagrams.bat again to compile to PNG

pause 