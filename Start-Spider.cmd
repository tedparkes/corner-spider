@echo off
setlocal
cd /d "%~dp0"
if exist "JumpingSpider.exe" goto launch
if not exist "Build.cmd" (
  echo JumpingSpider.exe is missing. Extract the complete Windows release ZIP again.
  pause
  exit /b 1
)
call "%~dp0Build.cmd"
if errorlevel 1 (
  echo.
  echo Build failed. Please send build.log so we can fix the problem.
  pause
  exit /b 1
)
:launch
start "" "%~dp0JumpingSpider.exe"
