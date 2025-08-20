# Building SubtitleOverlay Executable

This guide will help you build the SubtitleOverlay application into a standalone executable.

## Prerequisites

- .NET 6.0 SDK or later
- Windows 10/11 (for Windows-specific features)

## Build Options

### Option 1: Using Batch File (Recommended)
```cmd
build.bat
```

### Option 2: Using PowerShell
```powershell
.\build.ps1
```

### Option 3: Manual Commands
```cmd
dotnet clean
dotnet restore
dotnet build -c Release
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o ./publish
```

## Output

After successful build, you'll find:
- **Executable**: `./publish/SubtitleOverlay.exe`
- **Size**: Approximately 50-100 MB (includes all dependencies)

## Features of the Built Executable

✅ **Self-contained**: No .NET runtime installation required  
✅ **Single file**: All dependencies included in one .exe file  
✅ **Optimized**: Trimmed and ready-to-run for better performance  
✅ **Portable**: Can be copied to any Windows machine and run  

## Running the Application

1. Navigate to the `publish` folder
2. Double-click `SubtitleOverlay.exe`
3. The application will start with all features available

## Troubleshooting

### Build Errors
- Ensure .NET 6.0 SDK is installed
- Run `dotnet --version` to verify installation
- Try running `dotnet restore` first

### Runtime Errors
- The executable requires Windows 10/11
- Some features may require administrator privileges
- Check Windows Defender/antivirus isn't blocking the executable

## Distribution

You can distribute the `SubtitleOverlay.exe` file to other Windows machines. No additional installation is required.
