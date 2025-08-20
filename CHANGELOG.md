# Changelog

All notable changes to the SubtitleOverlay project will be documented in this file.

## [1.0.0] - 2024-12-19

### Added
- **OCR Text Recognition**: Real-time text recognition from screen areas using Windows OCR API
- **Dual-Language Display**: Shows both original English text and translated subtitles
- **Translation Integration**: Google Translation API support for 6 languages (English, Spanish, French, German, Chinese, Japanese)
- **Parameter Adjustment**: Comprehensive OCR parameter tuning with real-time testing
- **Overlay System**: Transparent subtitle overlay that stays on top of other applications
- **Screen Area Selection**: Interactive area selection for OCR capture
- **API Key Management**: Secure Google Translation API key configuration
- **Build System**: Automated build scripts for creating self-contained executables
- **Comprehensive Documentation**: Complete setup and usage guides

### Features
- **OCR Parameters**: Contrast, brightness, sharpness, confidence threshold, text scale, noise reduction, deskew
- **Quick Testing**: Test OCR parameters without closing the adjustment window
- **Real-time Translation**: Automatic translation of recognized text
- **Professional UI**: Clean, modern interface with status indicators
- **Error Handling**: Comprehensive error handling and user feedback
- **Logging System**: Detailed logging for debugging and monitoring

### Technical
- Built with .NET 6.0 and WPF
- Self-contained executable packaging
- Windows OCR API integration
- Google Translation API integration
- Screen capture and image processing
- Modular service architecture

### Documentation
- Comprehensive README with setup instructions
- Troubleshooting guide
- Development documentation
- Build and deployment guides
- API integration examples

## [0.9.0] - 2024-12-18

### Added
- Initial OCR service implementation
- Basic overlay window functionality
- Parameter adjustment interface
- Translation service integration

### Changed
- Migrated from speech recognition to OCR-based system
- Updated UI for OCR workflow
- Improved error handling

## [0.8.0] - 2024-12-17

### Added
- Speech recognition service
- Audio capture functionality
- Basic overlay system

### Changed
- Initial project structure
- Basic WPF application setup

---

## Version History

- **1.0.0**: Complete OCR subtitle system with translation
- **0.9.0**: OCR implementation and parameter adjustment
- **0.8.0**: Initial speech recognition system (deprecated)

## Future Plans

### Planned Features
- Support for additional OCR engines
- More translation service providers
- Advanced text cleaning and formatting
- Custom overlay themes and styling
- Batch processing capabilities
- Export functionality for recognized text

### Technical Improvements
- Performance optimizations
- Memory usage improvements
- Enhanced error recovery
- Plugin system for extensibility
- Cross-platform support (future consideration)
