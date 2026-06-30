@echo off
setlocal
chcp 65001 >nul
title SystemCleaner GitHub Portfolio Setup

echo ==========================================
echo  SystemCleaner GitHub Portfolio Setup
echo ==========================================
echo.

where git >nul 2>nul
if errorlevel 1 (
  echo [ERROR] Git is not installed or not in PATH.
  pause
  exit /b 1
)

if not exist .git (
  echo [INFO] Initializing git repository...
  git init
) else (
  echo [INFO] Git repository detected.
)

if not exist Assets mkdir Assets
if not exist .github mkdir .github
if not exist .github\workflows mkdir .github\workflows
if not exist .github\ISSUE_TEMPLATE mkdir .github\ISSUE_TEMPLATE

for %%F in (README.md .gitignore LICENSE ABOUT.txt TOPICS.txt architecture.md) do (
  if exist templates\%%F copy /Y templates\%%F %%F >nul
)

if exist templates\build.yml copy /Y templates\build.yml .github\workflows\build.yml >nul
if exist templates\bug_report.md copy /Y templates\bug_report.md .github\ISSUE_TEMPLATE\bug_report.md >nul
if exist templates\PULL_REQUEST_TEMPLATE.md copy /Y templates\PULL_REQUEST_TEMPLATE.md .github\PULL_REQUEST_TEMPLATE.md >nul

if not exist Assets\screenshot-main.png type nul > Assets\screenshot-main.png
if not exist Assets\screenshot-log.png type nul > Assets\screenshot-log.png
if not exist Assets\tray.png type nul > Assets\tray.png
if not exist Assets\demo.gif type nul > Assets\demo.gif
if not exist Assets\banner.png type nul > Assets\banner.png

echo.
echo [OK] Portfolio files created.
echo [INFO] Replace placeholder files in Assets with real screenshots/GIF.
echo [INFO] Then run: git add . ^&^& git commit -m "Prepare GitHub portfolio"
echo.
pause
