# Contributing to SubtitleOverlay

Thank you for your interest in contributing to SubtitleOverlay! This document provides guidelines and information for contributors.

## 🤝 How to Contribute

### Reporting Issues
- Use the GitHub issue tracker
- Provide detailed information about the problem
- Include steps to reproduce the issue
- Attach relevant log files or screenshots
- Specify your operating system and .NET version

### Suggesting Features
- Use the GitHub issue tracker with the "enhancement" label
- Describe the feature in detail
- Explain why this feature would be useful
- Provide use cases and examples

### Code Contributions
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Add tests if applicable
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

## 🛠️ Development Setup

### Prerequisites
- .NET 6.0 SDK
- Visual Studio 2022 or VS Code
- Windows 10/11 development environment
- Git

### Getting Started
```bash
# Clone the repository
git clone https://github.com/yourusername/subtitle-overlay.git
cd subtitle-overlay

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run in debug mode
dotnet run
```

## 📋 Coding Standards

### C# Code Style
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Follow C# naming conventions
- Use async/await for asynchronous operations
- Handle exceptions appropriately

### WPF/XAML Guidelines
- Use MVVM pattern where appropriate
- Keep UI logic separate from business logic
- Use data binding instead of code-behind where possible
- Follow WPF naming conventions

### Testing
- Add unit tests for new functionality
- Test error conditions and edge cases
- Ensure all tests pass before submitting

## 🏗️ Project Structure

```
SubtitleOverlay/
├── Services/                 # Core services
│   ├── WindowsOCRService.cs  # OCR implementation
│   ├── GoogleTranslationService.cs  # Translation service
│   ├── IOCRService.cs        # OCR interface
│   └── ITranslationService.cs # Translation interface
├── Windows/                  # UI windows
│   ├── MainWindow_Working.xaml  # Main application window
│   ├── OverlayWindow_Working.xaml  # Subtitle overlay
│   └── OCRParameterWindow.xaml  # Parameter adjustment
├── Models/                   # Data models
│   ├── OCRParameters.cs      # OCR configuration
│   └── SubtitleSettings.cs   # Application settings
└── Converters/              # WPF value converters
```

## 🔧 Building and Testing

### Build Commands
```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release

# Create executable
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

### Testing
```bash
# Run tests (if any)
dotnet test

# Build and run
dotnet run
```

## 📝 Documentation

### Code Documentation
- Add XML comments for public methods and classes
- Update README.md for new features
- Update CHANGELOG.md for version changes
- Add inline comments for complex logic

### User Documentation
- Update user guides for new features
- Add screenshots for UI changes
- Update troubleshooting section if needed

## 🚀 Release Process

### Version Numbers
- Follow semantic versioning (MAJOR.MINOR.PATCH)
- Update version in project file
- Update CHANGELOG.md
- Create release notes

### Pre-release Checklist
- [ ] All tests pass
- [ ] Documentation is updated
- [ ] CHANGELOG.md is updated
- [ ] Version numbers are updated
- [ ] Build succeeds in release mode
- [ ] Executable is tested

## 🐛 Bug Reports

When reporting bugs, please include:

1. **Environment Information**
   - Operating System version
   - .NET version
   - Application version

2. **Steps to Reproduce**
   - Detailed step-by-step instructions
   - Expected vs actual behavior

3. **Additional Information**
   - Error messages or logs
   - Screenshots if applicable
   - System specifications

## 💡 Feature Requests

When suggesting features:

1. **Clear Description**
   - What the feature should do
   - Why it would be useful
   - How it should work

2. **Use Cases**
   - Provide specific examples
   - Explain the target users
   - Describe the problem it solves

3. **Implementation Ideas**
   - Suggest technical approach
   - Consider impact on existing features
   - Think about user experience

## 📞 Getting Help

- **Issues**: Use GitHub issues for bugs and feature requests
- **Discussions**: Use GitHub discussions for questions and ideas
- **Documentation**: Check README.md and other docs first

## 🎉 Recognition

Contributors will be recognized in:
- README.md contributors section
- Release notes
- GitHub contributors page

Thank you for contributing to SubtitleOverlay! 🎬
