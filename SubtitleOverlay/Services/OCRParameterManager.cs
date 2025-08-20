using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SubtitleOverlay.Models;

namespace SubtitleOverlay.Services
{
    public class OCRParameterManager
    {
        private readonly ILogger<OCRParameterManager> _logger;
        private readonly Dictionary<string, OCRParameters> _areaParameters;
        private readonly string _parametersFilePath;
        
        public OCRParameterManager(ILogger<OCRParameterManager> logger)
        {
            _logger = logger;
            _areaParameters = new Dictionary<string, OCRParameters>();
            _parametersFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SubtitleOverlay",
                "ocr_parameters.json"
            );
            
            LoadParameters();
        }
        
        public OCRParameters GetParametersForArea(string areaId)
        {
            if (_areaParameters.TryGetValue(areaId, out var parameters))
            {
                _logger.LogDebug($"Retrieved parameters for area {areaId}: {parameters.Description}");
                return parameters.Clone();
            }
            
            _logger.LogDebug($"No parameters found for area {areaId}, using defaults");
            return OCRParameters.GetDefault();
        }
        
        public void SaveParametersForArea(string areaId, OCRParameters parameters)
        {
            parameters.AreaId = areaId;
            parameters.LastModified = DateTime.Now;
            
            _areaParameters[areaId] = parameters.Clone();
            
            _logger.LogInformation($"Saved parameters for area {areaId}: {parameters.Description}");
            SaveParameters();
        }
        
        public List<OCRParameters> GetAllParameters()
        {
            return _areaParameters.Values.Select(p => p.Clone()).ToList();
        }
        
        public void DeleteParametersForArea(string areaId)
        {
            if (_areaParameters.Remove(areaId))
            {
                _logger.LogInformation($"Deleted parameters for area {areaId}");
                SaveParameters();
            }
        }
        
        public string GenerateAreaId(int x, int y, int width, int height)
        {
            return $"area_{x}_{y}_{width}_{height}";
        }
        
        private void LoadParameters()
        {
            try
            {
                if (File.Exists(_parametersFilePath))
                {
                    var json = File.ReadAllText(_parametersFilePath);
                    var parametersList = JsonSerializer.Deserialize<List<OCRParameters>>(json);
                    
                    if (parametersList != null)
                    {
                        foreach (var parameters in parametersList)
                        {
                            if (!string.IsNullOrEmpty(parameters.AreaId))
                            {
                                _areaParameters[parameters.AreaId] = parameters;
                            }
                        }
                    }
                    
                    _logger.LogInformation($"Loaded {_areaParameters.Count} OCR parameter sets");
                }
                else
                {
                    _logger.LogInformation("No OCR parameters file found, starting with empty set");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading OCR parameters: {ex.Message}");
            }
        }
        
        private void SaveParameters()
        {
            try
            {
                var directory = Path.GetDirectoryName(_parametersFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                var parametersList = _areaParameters.Values.ToList();
                var json = JsonSerializer.Serialize(parametersList, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                File.WriteAllText(_parametersFilePath, json);
                _logger.LogDebug($"Saved {parametersList.Count} OCR parameter sets to {_parametersFilePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving OCR parameters: {ex.Message}");
            }
        }
        
        public OCRParameters GetOptimizedParameters(int width, int height)
        {
            // Auto-detect optimal parameters based on area size
            var areaSize = width * height;
            
            if (areaSize < 10000) // Small area
            {
                _logger.LogDebug($"Auto-selecting small text parameters for area {width}x{height}");
                return OCRParameters.GetSmallTextOptimized();
            }
            else if (areaSize > 100000) // Large area
            {
                _logger.LogDebug($"Auto-selecting large text parameters for area {width}x{height}");
                return OCRParameters.GetLargeTextOptimized();
            }
            else
            {
                _logger.LogDebug($"Using default parameters for area {width}x{height}");
                return OCRParameters.GetDefault();
            }
        }
    }
}
