using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SubtitleOverlay.Services
{
    public class TextCleaningService : ITextCleaningService
    {
        private readonly ILogger<TextCleaningService> _logger;
        private Dictionary<string, string> _commonOCRReplacements = new();
        private Dictionary<string, string> _commonWords = new();
        private readonly Regex _wordPattern;
        private readonly Regex _numberPattern;

        public TextCleaningService(ILogger<TextCleaningService> logger)
        {
            _logger = logger;
            _wordPattern = new Regex(@"\b[a-zA-Z]+\b", RegexOptions.Compiled);
            _numberPattern = new Regex(@"\b\d+\b", RegexOptions.Compiled);
            
            InitializeCommonReplacements();
            InitializeCommonWords();
        }

        private void InitializeCommonReplacements()
        {
            _commonOCRReplacements = new Dictionary<string, string>
            {
                // Common OCR mistakes
                { "0", "O" }, { "1", "I" }, { "5", "S" }, { "8", "B" },
                { "l", "I" }, { "rn", "m" }, { "cl", "d" }, { "vv", "w" },
                { "nn", "m" }, { "ll", "I" }, { "|", "I" }, { "!", "I" },
                { "?", "?" }, { ":", ":" }, { ";", ";" }, { ".", "." },
                { ",", "," }, { "-", "-" }, { "_", "_" }, { "=", "=" },
                
                // Common word corrections
                { "teh", "the" }, { "adn", "and" }, { "thier", "their" },
                { "recieve", "receive" }, { "seperate", "separate" },
                { "occured", "occurred" }, { "begining", "beginning" },
                { "neccessary", "necessary" }, { "accomodate", "accommodate" },
                { "definately", "definitely" }, { "embarass", "embarrass" },
                { "existance", "existence" }, { "occassion", "occasion" },
                { "priviledge", "privilege" }, { "sucess", "success" },
                { "tommorow", "tomorrow" }, { "untill", "until" },
                { "wierd", "weird" }, { "whereever", "wherever" }
            };
        }

        private void InitializeCommonWords()
        {
            _commonWords = new Dictionary<string, string>
            {
                // Common English words for context checking (no duplicates)
                { "the", "the" }, { "and", "and" }, { "is", "is" }, { "in", "in" },
                { "to", "to" }, { "of", "of" }, { "a", "a" }, { "that", "that" },
                { "it", "it" }, { "with", "with" }, { "he", "he" }, { "was", "was" },
                { "for", "for" }, { "on", "on" }, { "are", "are" }, { "as", "as" },
                { "you", "you" }, { "do", "do" }, { "at", "at" }, { "this", "this" },
                { "but", "but" }, { "his", "his" }, { "by", "by" }, { "from", "from" },
                { "they", "they" }, { "we", "we" }, { "say", "say" }, { "her", "her" },
                { "she", "she" }, { "or", "or" }, { "an", "an" }, { "will", "will" },
                { "my", "my" }, { "one", "one" }, { "all", "all" }, { "would", "would" },
                { "there", "there" }, { "their", "their" }, { "what", "what" },
                { "so", "so" }, { "up", "up" }, { "out", "out" }, { "if", "if" },
                { "about", "about" }, { "who", "who" }, { "get", "get" },
                { "which", "which" }, { "go", "go" }, { "me", "me" },
                { "when", "when" }, { "make", "make" }, { "can", "can" },
                { "like", "like" }, { "time", "time" }, { "no", "no" },
                { "just", "just" }, { "him", "him" }, { "know", "know" },
                { "take", "take" }, { "people", "people" }, { "into", "into" },
                { "year", "year" }, { "your", "your" }, { "good", "good" },
                { "some", "some" }, { "could", "could" }, { "them", "them" },
                { "see", "see" }, { "other", "other" }, { "than", "than" },
                { "then", "then" }, { "now", "now" }, { "look", "look" },
                { "only", "only" }, { "come", "come" }, { "its", "its" },
                { "over", "over" }, { "think", "think" }, { "also", "also" },
                { "back", "back" }, { "after", "after" }, { "use", "use" },
                { "two", "two" }, { "how", "how" }, { "our", "our" },
                { "work", "work" }, { "first", "first" }, { "well", "well" },
                { "way", "way" }, { "even", "even" }, { "new", "new" },
                { "want", "want" }, { "because", "because" }, { "any", "any" },
                { "these", "these" }, { "give", "give" }, { "day", "day" },
                { "most", "most" }, { "us", "us" }, { "here", "here" },
                { "should", "should" }, { "try", "try" }, { "tell", "tell" },
                { "call", "call" }, { "find", "find" }, { "ask", "ask" },
                { "need", "need" }, { "feel", "feel" }, { "become", "become" },
                { "leave", "leave" }, { "put", "put" }, { "mean", "mean" },
                { "keep", "keep" }, { "let", "let" }, { "begin", "begin" },
                { "seem", "seem" }, { "help", "help" }, { "talk", "talk" },
                { "turn", "turn" }, { "start", "start" }, { "might", "might" },
                { "show", "show" }, { "part", "part" }, { "face", "face" },
                { "own", "own" }, { "place", "place" }, { "where", "where" },
                { "little", "little" }, { "round", "round" }, { "man", "man" },
                { "came", "came" }, { "every", "every" }, { "under", "under" },
                { "name", "name" }, { "very", "very" }, { "through", "through" },
                { "form", "form" }, { "sentence", "sentence" }, { "great", "great" },
                { "low", "low" }, { "line", "line" }, { "differ", "differ" },
                { "cause", "cause" }, { "much", "much" }, { "before", "before" },
                { "move", "move" }, { "right", "right" }, { "boy", "boy" },
                { "old", "old" }, { "too", "too" }, { "same", "same" },
                { "does", "does" }, { "set", "set" }, { "three", "three" },
                { "air", "air" }, { "play", "play" }, { "small", "small" },
                { "end", "end" }, { "home", "home" }, { "read", "read" },
                { "hand", "hand" }, { "port", "port" }, { "large", "large" },
                { "spell", "spell" }, { "add", "add" }, { "land", "land" },
                { "must", "must" }, { "big", "big" }, { "high", "high" },
                { "such", "such" }, { "follow", "follow" }, { "act", "act" },
                { "why", "why" }
            };
        }

        public async Task<string> CleanTextAsync(string rawText, string sourceLanguage = "en")
        {
            if (string.IsNullOrWhiteSpace(rawText))
                return string.Empty;

            try
            {
                _logger.LogDebug($"Cleaning text: {rawText}");
                
                var cleanedText = await Task.Run(() =>
                {
                    var text = rawText.Trim();
                    
                    // Remove excessive whitespace
                    text = Regex.Replace(text, @"\s+", " ");
                    
                    // Fix common punctuation issues
                    text = FixPunctuation(text);
                    
                    // Fix common OCR errors
                    text = FixOCRErrors(text);
                    
                    // Fix capitalization
                    text = FixCapitalization(text);
                    
                    // Remove noise characters
                    text = RemoveNoiseCharacters(text);
                    
                    return text;
                });

                _logger.LogDebug($"Cleaned text: {cleanedText}");
                return cleanedText;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error cleaning text: {ex.Message}");
                return rawText; // Return original if cleaning fails
            }
        }

        public async Task<string> FixOCRTextAsync(string ocrText, string sourceLanguage = "en")
        {
            if (string.IsNullOrWhiteSpace(ocrText))
                return string.Empty;

            try
            {
                _logger.LogDebug($"Fixing OCR text: {ocrText}");
                
                var fixedText = await Task.Run(() =>
                {
                    var text = ocrText.Trim();
                    
                    // Apply common OCR replacements
                    text = ApplyOCRReplacements(text);
                    
                    // Fix word-level errors using context
                    text = FixWordErrors(text);
                    
                    // Fix sentence structure
                    text = FixSentenceStructure(text);
                    
                    // Apply language-specific fixes
                    if (sourceLanguage.StartsWith("en"))
                    {
                        text = ApplyEnglishSpecificFixes(text);
                    }
                    
                    return text;
                });

                _logger.LogDebug($"Fixed OCR text: {fixedText}");
                return fixedText;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fixing OCR text: {ex.Message}");
                return ocrText; // Return original if fixing fails
            }
        }

        public async Task<string> ImproveTranslationAsync(string text, string targetLanguage = "en")
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            try
            {
                _logger.LogDebug($"Improving translation: {text}");
                
                var improvedText = await Task.Run(() =>
                {
                    var improved = text.Trim();
                    
                    // Fix grammar and syntax
                    improved = FixGrammar(improved, targetLanguage);
                    
                    // Improve readability
                    improved = ImproveReadability(improved);
                    
                    // Fix common translation errors
                    improved = FixTranslationErrors(improved, targetLanguage);
                    
                    return improved;
                });

                _logger.LogDebug($"Improved translation: {improvedText}");
                return improvedText;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error improving translation: {ex.Message}");
                return text; // Return original if improvement fails
            }
        }

        private string FixPunctuation(string text)
        {
            // Fix spacing around punctuation
            text = Regex.Replace(text, @"\s+([.,!?;:])", "$1");
            text = Regex.Replace(text, @"([.,!?;:])\s*([a-zA-Z])", "$1 $2");
            
            // Fix multiple punctuation
            text = Regex.Replace(text, @"([.!?])\1+", "$1");
            text = Regex.Replace(text, @"([,;:])\1+", "$1");
            
            // Fix quotes
            text = Regex.Replace(text, @"[""]+", "\"");
            text = Regex.Replace(text, @"['']+", "'");
            
            return text;
        }

        private string FixOCRErrors(string text)
        {
            var words = text.Split(' ');
            var fixedWords = new List<string>();

            foreach (var word in words)
            {
                var fixedWord = word;
                
                // Apply common OCR replacements
                foreach (var replacement in _commonOCRReplacements)
                {
                    fixedWord = fixedWord.Replace(replacement.Key, replacement.Value);
                }
                
                // Fix common OCR patterns
                fixedWord = Regex.Replace(fixedWord, @"[0O]{2,}", "OO");
                fixedWord = Regex.Replace(fixedWord, @"[1I]{2,}", "II");
                fixedWord = Regex.Replace(fixedWord, @"[5S]{2,}", "SS");
                
                fixedWords.Add(fixedWord);
            }

            return string.Join(" ", fixedWords);
        }

        private string FixCapitalization(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Capitalize first letter of sentences
            text = Regex.Replace(text, @"(^|[.!?]\s+)([a-z])", m => m.Groups[1].Value + m.Groups[2].Value.ToUpper());
            
            // Fix "i" to "I" when it's a pronoun
            text = Regex.Replace(text, @"\bi\b", "I");
            
            // Fix common proper nouns
            text = Regex.Replace(text, @"\b(english|french|german|spanish|chinese|japanese|korean)\b", m => m.Value.ToUpper());
            
            return text;
        }

        private string RemoveNoiseCharacters(string text)
        {
            // Remove non-printable characters except newlines
            text = Regex.Replace(text, @"[^\x20-\x7E\r\n]", "");
            
            // Remove excessive newlines
            text = Regex.Replace(text, @"\r?\n\s*\r?\n", "\n");
            
            // Remove excessive spaces
            text = Regex.Replace(text, @"\s+", " ");
            
            return text.Trim();
        }

        private string ApplyOCRReplacements(string text)
        {
            var words = text.Split(' ');
            var fixedWords = new List<string>();

            foreach (var word in words)
            {
                var fixedWord = word;
                
                // Apply character-level replacements
                foreach (var replacement in _commonOCRReplacements)
                {
                    fixedWord = fixedWord.Replace(replacement.Key, replacement.Value);
                }
                
                fixedWords.Add(fixedWord);
            }

            return string.Join(" ", fixedWords);
        }

        private string FixWordErrors(string text)
        {
            var words = text.Split(' ');
            var fixedWords = new List<string>();

            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i].ToLower();
                var originalWord = words[i];
                
                // Check if it's a known word
                if (_commonWords.ContainsKey(word))
                {
                    fixedWords.Add(originalWord);
                    continue;
                }

                // Try to find similar words
                var similarWord = FindSimilarWord(word);
                if (similarWord != null)
                {
                    // Preserve original capitalization
                    if (char.IsUpper(originalWord[0]))
                        similarWord = char.ToUpper(similarWord[0]) + similarWord.Substring(1);
                    
                    fixedWords.Add(similarWord);
                }
                else
                {
                    fixedWords.Add(originalWord);
                }
            }

            return string.Join(" ", fixedWords);
        }

        private string? FindSimilarWord(string word)
        {
            if (word.Length < 3) return null;

            var bestMatch = "";
            var bestScore = 0.0;

            foreach (var commonWord in _commonWords.Keys)
            {
                if (commonWord.Length < 3) continue;

                var score = CalculateSimilarity(word, commonWord);
                if (score > bestScore && score > 0.7) // 70% similarity threshold
                {
                    bestScore = score;
                    bestMatch = commonWord;
                }
            }

            return bestScore > 0.0 ? bestMatch : null;
        }

        private double CalculateSimilarity(string word1, string word2)
        {
            if (word1 == word2) return 1.0;
            if (word1.Length == 0 || word2.Length == 0) return 0.0;

            var matrix = new int[word1.Length + 1, word2.Length + 1];

            for (int i = 0; i <= word1.Length; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= word2.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= word1.Length; i++)
            {
                for (int j = 1; j <= word2.Length; j++)
                {
                    var cost = word1[i - 1] == word2[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1), matrix[i - 1, j - 1] + cost);
                }
            }

            var maxLength = Math.Max(word1.Length, word2.Length);
            return 1.0 - (double)matrix[word1.Length, word2.Length] / maxLength;
        }

        private string FixSentenceStructure(string text)
        {
            // Fix common sentence structure issues
            text = Regex.Replace(text, @"\s+([.!?])", "$1");
            text = Regex.Replace(text, @"([.!?])\s*([a-z])", m => m.Groups[1].Value + " " + m.Groups[2].Value.ToUpper());
            
            // Fix spacing around punctuation
            text = Regex.Replace(text, @"\s+([,;:])", "$1");
            text = Regex.Replace(text, @"([,;:])\s*([a-zA-Z])", "$1 $2");
            
            return text;
        }

        private string ApplyEnglishSpecificFixes(string text)
        {
            // Fix common English-specific issues
            text = Regex.Replace(text, @"\b(teh|adn|thier|recieve|seperate|occured|begining|neccessary|accomodate|definately|embarass|existance|occassion|priviledge|sucess|tommorow|untill|wierd|whereever)\b", m =>
            {
                var word = m.Value.ToLower();
                return _commonOCRReplacements.ContainsKey(word) ? _commonOCRReplacements[word] : m.Value;
            });
            
            return text;
        }

        private string FixGrammar(string text, string targetLanguage)
        {
            if (targetLanguage.StartsWith("en"))
            {
                // Fix common English grammar issues
                text = Regex.Replace(text, @"\b(am|is|are)\s+(am|is|are)\b", "is");
                text = Regex.Replace(text, @"\b(have|has)\s+(have|has)\b", "has");
                text = Regex.Replace(text, @"\b(do|does)\s+(do|does)\b", "does");
            }
            
            return text;
        }

        private string ImproveReadability(string text)
        {
            // Remove excessive punctuation
            text = Regex.Replace(text, @"([.!?])\1+", "$1");
            text = Regex.Replace(text, @"([,;:])\1+", "$1");
            
            // Fix spacing
            text = Regex.Replace(text, @"\s+", " ");
            
            // Ensure proper sentence endings
            if (!text.EndsWith(".") && !text.EndsWith("!") && !text.EndsWith("?"))
            {
                text += ".";
            }
            
            return text.Trim();
        }

        private string FixTranslationErrors(string text, string targetLanguage)
        {
            if (targetLanguage.StartsWith("en"))
            {
                // Fix common translation errors to English
                text = Regex.Replace(text, @"\b(he|she)\s+(am|are)\b", "$1 is");
                text = Regex.Replace(text, @"\b(I)\s+(is|are)\b", "$1 am");
                text = Regex.Replace(text, @"\b(you|we|they)\s+(is)\b", "$1 are");
            }
            
            return text;
        }
    }
}
