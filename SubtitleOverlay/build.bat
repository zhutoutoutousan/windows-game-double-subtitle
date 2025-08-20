@echo off
echo ========================================
echo Building SubtitleOverlay Application
echo ========================================

echo.
echo Cleaning previous builds...
dotnet clean

echo.
echo Restoring NuGet packages...
dotnet restore

echo.
echo Building release version...
dotnet build -c Release

echo.
echo Publishing as self-contained executable...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish

echo.
echo ========================================
echo Build completed!
echo ========================================
echo.
echo Executable location: ./publish/SubtitleOverlay.exe
echo.
echo You can now run the application by double-clicking SubtitleOverlay.exe
echo.
pause
