Write-Host "========================================" -ForegroundColor Green
Write-Host "Building SubtitleOverlay Application" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

Write-Host ""
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean

Write-Host ""
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

Write-Host ""
Write-Host "Building release version..." -ForegroundColor Yellow
dotnet build -c Release

Write-Host ""
Write-Host "Publishing as self-contained executable..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Build completed!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Executable location: ./publish/SubtitleOverlay.exe" -ForegroundColor Cyan
Write-Host ""
Write-Host "You can now run the application by double-clicking SubtitleOverlay.exe" -ForegroundColor White
Write-Host ""
Read-Host "Press Enter to continue"
