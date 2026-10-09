@echo off
if "%~1"=="" (
    echo Error: Please provide a version tag.
    echo Usage: push.bat v0.0.7
    exit /b 1
)

echo Testing...
dotnet test --configuration Release --no-restore --verbosity normal
if errorlevel 1 exit /b %errorlevel%

echo Pushing current branch...
git push
if errorlevel 1 exit /b %errorlevel%

echo Creating tag %~1...
git tag %~1
if errorlevel 1 exit /b %errorlevel%

echo Pushing tag %~1...
git push origin %~1

echo Done!