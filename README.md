# 🎬 SubtitleOverlay - Real-Time OCR Subtitle System

A powerful Windows application that provides real-time subtitle overlays for any text on your screen using OCR (Optical Character Recognition) and translation services.

## 🌟 Features

### ✅ **OCR Text Recognition**
- **Screen Area Selection**: Select any region of your screen for text recognition
- **Real-time Recognition**: Continuously captures and recognizes text from the selected area
- **Parameter Adjustment**: Fine-tune OCR settings for optimal accuracy
- **Quick Testing**: Test OCR parameters without closing the adjustment window

### ✅ **Dual-Language Display**
- **Original Text**: Shows recognized English text (top line, white)
- **Translated Subtitle**: Shows translated text in target language (bottom line, light blue, italic)
- **Language Support**: English, Spanish, French, German, Chinese, Japanese

### ✅ **Translation Integration**
- **Google Translation API**: Professional-grade translation service
- **Real-time Translation**: Automatically translates recognized text
- **API Key Management**: Secure API key configuration
- **Multiple Languages**: Support for 6 major languages

### ✅ **Overlay System**
- **Transparent Overlay**: Shows subtitles over any application
- **Always on Top**: Stays visible over other windows
- **Customizable Position**: Move overlay window anywhere on screen
- **Professional Styling**: Clean, readable subtitle display

## 🚀 Quick Start

### Prerequisites
- Windows 10/11 (64-bit)
- .NET 6.0 Runtime (included in self-contained executable)
- Google Translation API key (optional, for translation features)

### Installation & Usage

1. **Download the Executable**
   ```bash
   # Clone the repository
   git clone https://github.com/yourusername/subtitle-overlay.git
   cd subtitle-overlay
   ```

2. **Build the Application**
   ```bash
   # Option 1: Using batch file
   build.bat
   
   # Option 2: Using PowerShell
   .\build.ps1
   
   # Option 3: Manual build
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
   ```

3. **Run the Application**
   ```bash
   # Option 1: Direct execution
   cd publish
   SubtitleOverlay.exe
   
   # Option 2: Using launcher
   run.bat
   ```

4. **Configure Translation (Optional)**
   - Enter your Google Translation API key
   - Click "Save API Key"
   - Select target language from dropdown

5. **Start Using OCR**
   - Click "Select Area" to choose text region
   - Click "Start OCR" to begin recognition
   - Click "Show Overlay" to display subtitles

## 📋 System Requirements

- **OS**: Windows 10/11 (64-bit)
- **RAM**: 4GB minimum, 8GB recommended
- **Storage**: 200MB free space
- **Permissions**: May require admin rights for screen capture
- **Internet**: Required for translation features

## 🔧 Configuration

### OCR Parameters
Access via "Adjust Parameters" button:
- **Contrast**: Adjust image contrast (0.5 - 2.0)
- **Brightness**: Adjust image brightness (-50 - 50)
- **Sharpness**: Adjust image sharpness (0.5 - 2.0)
- **Confidence**: Minimum confidence threshold (0.1 - 1.0)
- **Text Scale**: Scale factor for text recognition (1.0 - 3.0)
- **Noise Reduction**: Enable/disable noise reduction
- **Deskew**: Enable/disable text deskewing

### Translation Settings
- **API Key**: Google Translation API key
- **Target Language**: Choose from 6 supported languages
- **Cache**: Translation results are cached for performance

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
├── build.bat                 # Build script
├── build.ps1                 # PowerShell build script
└── run.bat                   # Application launcher
```

## 🛠️ Development

### Prerequisites
- .NET 6.0 SDK
- Visual Studio 2022 or VS Code
- Windows 10/11 development environment

### Building from Source
```bash
# Clone repository
git clone https://github.com/yourusername/subtitle-overlay.git
cd subtitle-overlay

# Restore dependencies
dotnet restore

# Build in debug mode
dotnet build

# Run in debug mode
dotnet run
```

### Key Technologies
- **.NET 6.0**: Modern .NET framework
- **WPF**: Windows Presentation Foundation for UI
- **Windows OCR API**: Native Windows OCR capabilities
- **Google Translation API**: Professional translation service
- **System.Drawing**: Image processing and screen capture

## 🔍 Troubleshooting

### Common Issues

**Application Won't Start**
- Ensure Windows 10/11 is installed
- Try running as administrator
- Check Windows Defender/antivirus settings

**OCR Not Working**
- Select a clear text area with good contrast
- Adjust OCR parameters for better recognition
- Ensure text is visible and not too small

**Translation Not Working**
- Verify Google API key is correct
- Check internet connection
- Ensure target language is selected

**Overlay Not Visible**
- Click "Show Overlay" button
- Check if overlay window is behind other windows
- Try moving the overlay window

### Debug Information
- Check log messages in the main window
- Verify all system requirements are met
- Ensure proper permissions for screen capture

## 🤝 Contributing

We welcome contributions! Please feel free to submit issues and pull requests.

### Development Guidelines
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Windows OCR API**: For text recognition capabilities
- **Google Translation API**: For translation services
- **.NET Community**: For excellent development tools and libraries

## 📞 Support

If you encounter issues or have questions:
1. Check the troubleshooting section above
2. Review the log messages in the application
3. Open an issue on GitHub with detailed information

## 🎉 Enjoy Your SubtitleOverlay!

This application provides real-time subtitles for any text on your screen, making it perfect for:
- Gaming with foreign language text
- Reading translated content
- Accessibility support
- Language learning
- Professional translation work

---

**Made with ❤️ for the Windows community**
