@echo off
setlocal enabledelayedexpansion

echo ========================================
echo Architecture Diagram Generator
echo ========================================
echo.

REM Set the path to the PlantUML JAR file
set PLANTUML_JAR=C:\plantuml\plantuml.jar

REM Check if PlantUML JAR exists
if not exist "%PLANTUML_JAR%" (
    echo Error: PlantUML JAR not found at %PLANTUML_JAR%
    echo Please install PlantUML and update the path in this script
    echo Download from: https://plantuml.com/download
    pause
    exit /b 1
)

REM Create output directories if they don't exist
cd ..
if not exist "output" mkdir output
cd diagram_builder
if not exist "templates" mkdir templates

REM Check if template files exist, if not create them
call :create_template_files

echo Starting diagram generation process...
echo.

REM Analyze codebase and generate diagrams
call :analyze_codebase

REM Compile each .puml file from the parent directory
cd ..
for %%f in (*.puml) do (
    echo Compiling %%f...
    java -jar "%PLANTUML_JAR%" -tpng "%%f" -o "output"
)

echo.
echo All diagrams have been generated and compiled to the 'output' directory
echo.

REM Open the output directory
start output
pause
exit /b 0

:create_template_files
echo Checking template files...

if not exist "templates\context_diagram_template.puml" (
    echo Creating context diagram template...
    (
        echo @startuml Context Diagram
        echo.
        echo title System Context Diagram
        echo.
        echo skinparam {
        echo   componentStyle uml2
        echo   defaultTextAlignment center
        echo   stereotypeCBackgroundColor #A9DCDF
        echo   packageBackgroundColor #FEFEFE
        echo }
        echo.
        echo actor "User" as user
        echo.
        echo rectangle "Your System" as system {
        echo   [Core Application] as app
        echo }
        echo.
        echo cloud "External Services" {
        echo   [Service 1] as svc1
        echo   [Service 2] as svc2
        echo }
        echo.
        echo database "Database" as db
        echo.
        echo user -^> app : Uses
        echo app -^> svc1 : Integrates with
        echo app -^> svc2 : Calls
        echo app --^> db : Stores data
        echo.
        echo @enduml
    ) > "templates\context_diagram_template.puml"
)

if not exist "templates\container_diagram_template.puml" (
    echo Creating container diagram template...
    (
        echo @startuml Container Diagram
        echo.
        echo title Container Diagram
        echo.
        echo skinparam {
        echo   componentStyle uml2
        echo   defaultTextAlignment center
        echo   stereotypeCBackgroundColor #A9DCDF
        echo }
        echo.
        echo package "Your System" {
        echo   [Web API] as api
        echo   [Service Layer] as services
        echo   [Data Access Layer] as dal
        echo }
        echo.
        echo [External Systems] as ext
        echo database "Database" as db
        echo.
        echo api -^> services : Uses
        echo services -^> dal : Uses
        echo dal -^> db : Reads/Writes
        echo api -^> ext : Integrates
        echo.
        echo @enduml
    ) > "templates\container_diagram_template.puml"
)

if not exist "templates\component_diagram_template.puml" (
    echo Creating component diagram template...
    (
        echo @startuml Component Diagram
        echo.
        echo title Component Diagram
        echo.
        echo skinparam {
        echo   componentStyle uml2
        echo   defaultTextAlignment center
        echo }
        echo.
        echo package "Controllers" {
        echo   [Controller 1] as ctrl1
        echo   [Controller 2] as ctrl2
        echo }
        echo.
        echo package "Services" {
        echo   [Service 1] as svc1
        echo   [Service 2] as svc2
        echo   interface "IService1" as isvc1
        echo   interface "IService2" as isvc2
        echo }
        echo.
        echo package "Repositories" {
        echo   [Repository 1] as repo1
        echo   [Repository 2] as repo2
        echo   interface "IRepository1" as irepo1
        echo   interface "IRepository2" as irepo2
        echo }
        echo.
        echo svc1 -^- isvc1
        echo svc2 -^- isvc2
        echo repo1 -^- irepo1
        echo repo2 -^- irepo2
        echo.
        echo ctrl1 --^> isvc1 : uses
        echo ctrl2 --^> isvc2 : uses
        echo svc1 --^> irepo1 : uses
        echo svc2 --^> irepo2 : uses
        echo.
        echo @enduml
    ) > "templates\component_diagram_template.puml"
)

if not exist "templates\class_diagram_template.puml" (
    echo Creating class diagram template...
    (
        echo @startuml Class Diagram
        echo.
        echo title Class Diagram
        echo.
        echo skinparam {
        echo   classAttributeIconSize 0
        echo   defaultTextAlignment center
        echo }
        echo.
        echo class Entity {
        echo   +Id: string
        echo   +Name: string
        echo   +CreatedAt: DateTime
        echo   +UpdatedAt: DateTime
        echo }
        echo.
        echo class DTO {
        echo   +Id: string
        echo   +Name: string
        echo }
        echo.
        echo class Service {
        echo   +GetById^(id: string^): Entity
        echo   +Create^(dto: DTO^): Entity
        echo   +Update^(id: string, dto: DTO^): Entity
        echo   +Delete^(id: string^): bool
        echo }
        echo.
        echo Service -- Entity : uses
        echo Service -- DTO : uses
        echo.
        echo @enduml
    ) > "templates\class_diagram_template.puml"
)

if not exist "templates\sequence_diagram_template.puml" (
    echo Creating sequence diagram template...
    (
        echo @startuml Sequence Diagram
        echo.
        echo title Sequence Diagram
        echo.
        echo participant "Client" as client
        echo participant "Controller" as ctrl
        echo participant "Service" as svc
        echo participant "Repository" as repo
        echo database "Database" as db
        echo.
        echo client -^> ctrl : Request
        echo activate ctrl
        echo.
        echo ctrl -^> svc : Process
        echo activate svc
        echo.
        echo svc -^> repo : Query
        echo activate repo
        echo.
        echo repo -^> db : Execute
        echo activate db
        echo db --^> repo : Result
        echo deactivate db
        echo.
        echo repo --^> svc : Data
        echo deactivate repo
        echo.
        echo svc --^> ctrl : Response
        echo deactivate svc
        echo.
        echo ctrl --^> client : Result
        echo deactivate ctrl
        echo.
        echo @enduml
    ) > "templates\sequence_diagram_template.puml"
)

if not exist "templates\deployment_diagram_template.puml" (
    echo Creating deployment diagram template...
    (
        echo @startuml Deployment Diagram
        echo.
        echo title Deployment Diagram
        echo.
        echo skinparam {
        echo   defaultTextAlignment center
        echo   nodeBackgroundColor #FEFEFE
        echo }
        echo.
        echo node "Web Server" {
        echo   [Web Application] as webapp
        echo }
        echo.
        echo node "Application Server" {
        echo   [Application] as app
        echo   [Cache] as cache
        echo }
        echo.
        echo node "Database Server" {
        echo   database "SQL Database" as db
        echo }
        echo.
        echo cloud "External Services" {
        echo   [Third Party API] as api
        echo }
        echo.
        echo webapp -^> app : HTTP/HTTPS
        echo app -^> db : TCP
        echo app -^> cache : TCP
        echo app -^> api : HTTP/HTTPS
        echo.
        echo @enduml
    ) > "templates\deployment_diagram_template.puml"
)

echo Template files created/verified.
echo.
exit /b 0

:analyze_codebase
echo Analyzing codebase structure...
echo This feature would analyze the actual code to generate diagrams automatically.
echo Currently, you can use the template files as a starting point.
echo.

echo To create a new diagram:
echo 1. Copy a template from the templates directory to the main diagrams directory
echo 2. Modify it based on your codebase
echo 3. Save it with a meaningful name and .puml extension
echo 4. Run this script again to compile it
echo.

echo You can create these diagrams:
cd ..
echo Current directory: %CD%
echo Files in this directory:
dir *.puml

cd diagram_builder
echo.
exit /b 0 