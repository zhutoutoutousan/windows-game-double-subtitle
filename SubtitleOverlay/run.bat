@echo off
echo Starting SubtitleOverlay Application...
echo.

if exist "publish\SubtitleOverlay.exe" (
    echo Found executable, starting...
    start "" "publish\SubtitleOverlay.exe"
) else (
    echo Error: SubtitleOverlay.exe not found in publish folder
    echo Please run build.bat first to create the executable
    pause
)
