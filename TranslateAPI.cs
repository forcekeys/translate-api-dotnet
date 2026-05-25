using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ForceKeys.TranslateAPI
{
    /// <summary>
    /// API Error Exception
    /// </summary>
    public class APIException : Exception
    {
        public string ErrorCode { get; }
        public int StatusCode { get; }
        public int? RetryAfter { get; }
        
        public APIException(string message) : this(message, null, 0, null) { }
        
        public APIException(string message, string errorCode, int statusCode, int? retryAfter) : base(message)
        {
            ErrorCode = errorCode;
            StatusCode = statusCode;
            RetryAfter = retryAfter;
        }
    }
    
    /// <summary>
    /// Translation result
    /// </summary>
    public class TranslationResult
    {
        public string TranslatedText { get; }
        public string SourceLang { get; }
        public string TargetLang { get; }
        public string DetectedLang { get; }
        public int CharactersUsed { get; }
        public int ProcessingTimeMs { get; }
        
        public TranslationResult(string translatedText, string sourceLang, string targetLang, 
                                string detectedLang, int charactersUsed, int processingTimeMs)
        {
            TranslatedText = translatedText;
            SourceLang = sourceLang;
            TargetLang = targetLang;
            DetectedLang = detectedLang;
            CharactersUsed = charactersUsed;
            ProcessingTimeMs = processingTimeMs;
        }
    }
    
    /// <summary>
    /// Document translation result
    /// </summary>
    public class DocumentTranslationResult
    {
        public string TranslatedText { get; }
        public string SourceLang { get; }
        public string TargetLang { get; }
        public int Pages { get; }
        public int CharactersUsed { get; }
        public int ProcessingTimeMs { get; }
        
        public DocumentTranslationResult(string translatedText, string sourceLang, string targetLang, 
                                        int pages, int charactersUsed, int processingTimeMs)
        {
            TranslatedText = translatedText;
            SourceLang = sourceLang;
            TargetLang = targetLang;
            Pages = pages;
            CharactersUsed = charactersUsed;
            ProcessingTimeMs = processingTimeMs;
        }
    }
    
    /// <summary>
    /// OCR result
    /// </summary>
    public class OCRResult
    {
        public string Text { get; }
        public double Confidence { get; }
        public string LanguageDetected { get; }
        public int ProcessingTimeMs { get; }
        
        public OCRResult(string text, double confidence, string languageDetected, int processingTimeMs)
        {
            Text = text;
            Confidence = confidence;
            LanguageDetected = languageDetected;
            ProcessingTimeMs = processingTimeMs;
        }
    }
    
    /// <summary>
    /// Language detection result
    /// </summary>
    public class LanguageDetectionResult
    {
        public string Language { get; }
        public string LanguageName { get; }
        public double Confidence { get; }
        public List<LanguageAlternative> Alternatives { get; }
        
        public LanguageDetectionResult(string language, string languageName, double confidence, 
                                      List<LanguageAlternative> alternatives)
        {
            Language = language;
            LanguageName = languageName;
            Confidence = confidence;
            Alternatives = alternatives;
        }
    }
    
    /// <summary>
    /// Language alternative
    /// </summary>
    public class LanguageAlternative
    {
        public string Language { get; }
        public double Confidence { get; }
        public string LanguageName { get; }
        
        public LanguageAlternative(string language, double confidence, string languageName)
        {
            Language = language;
            Confidence = confidence;
            LanguageName = languageName;
        }
    }
    
    /// <summary>
    /// Language information
    /// </summary>
    public class Language
    {
        public string Code { get; }
        public string Name { get; }
        public string Flag { get; }
        
        public Language(string code, string name, string flag)
        {
            Code = code;
            Name = name;
            Flag = flag;
        }
    }
    
    /// <summary>
    /// Account information
    /// </summary>
    public class AccountInfo
    {
        public string Email { get; }
        public string Name { get; }
        public string Plan { get; }
        public string Status { get; }
        public int DailyTranslations { get; }
        public int TodayUsed { get; }
        public int RemainingToday { get; }
        public double AvailableBalance { get; }
        public double TotalSpent { get; }
        public int TotalTranslations { get; }
        public int TotalCharacters { get; }
        
        public AccountInfo(string email, string name, string plan, string status, 
                          int dailyTranslations, int todayUsed, int remainingToday,
                          double availableBalance, double totalSpent, 
                          int totalTranslations, int totalCharacters)
        {
            Email = email;
            Name = name;
            Plan = plan;
            Status = status;
            DailyTranslations = dailyTranslations;
            TodayUsed = todayUsed;
            RemainingToday = remainingToday;
            AvailableBalance = availableBalance;
            TotalSpent = totalSpent;
            TotalTranslations = totalTranslations;
            TotalCharacters = totalCharacters;
        }
    }
    
    /// <summary>
    /// TranslateAPI .NET SDK
    /// Official .NET client for the TranslateAPI translation service.
    /// https://github.com/forcekeys/translate-api-dotnet
    /// 
    /// Example:
    /// ```csharp
    /// var api = new TranslateAPI("your_api_key");
    /// var result = await api.TranslateAsync("Hello, world!", "en", "fr");
    /// Console.WriteLine(result.TranslatedText);
    /// ```
    /// </summary>
    public class TranslateAPI
    {
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;
        
        private const string DefaultBaseUrl = "https://api.translate.forcekeys.com/api/v1";
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
        
        /// <summary>
        /// Initialize TranslateAPI client with default settings
        /// </summary>
        /// <param name="apiKey">Your API key</param>
        public TranslateAPI(string apiKey) : this(apiKey, DefaultBaseUrl, DefaultTimeout) { }
        
        /// <summary>
        /// Initialize TranslateAPI client with custom settings
        /// </summary>
        /// <param name="apiKey">Your API key</param>
        /// <param name="baseUrl">API base URL</param>
        /// <param name="timeout">Request timeout</param>
        public TranslateAPI(string apiKey, string baseUrl, TimeSpan timeout)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("API key is required", nameof(apiKey));
            }
            
            _apiKey = apiKey.Trim();
            _baseUrl = baseUrl.TrimEnd('/');
            
            _httpClient = new HttpClient
            {
                Timeout = timeout,
                DefaultRequestHeaders =
                {
                    { "User-Agent", "TranslateAPI-.NET/1.0.0" }
                }
            };
        }
        
        /// <summary>
        /// Make API request
        /// </summary>
        private async Task<JsonDocument> MakeRequestAsync(string endpoint, HttpMethod method, 
                                                         HttpContent content = null)
        {
            var url = $"{_baseUrl}/{endpoint}";
            var request = new HttpRequestMessage(method, url);
            
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            
            if (content != null)
            {
                request.Content = content;
            }
            
            var response = await _httpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();
            
            JsonDocument jsonDoc;
            try
            {
                jsonDoc = JsonDocument.Parse(responseText);
            }
            catch (JsonException)
            {
                throw new APIException("Invalid JSON response from API");
            }
            
            if (!response.IsSuccessStatusCode)
            {
                var errorCode = jsonDoc.RootElement.TryGetProperty("code", out var codeProp) 
                    ? codeProp.GetString() 
                    : "http_error";
                var errorMsg = jsonDoc.RootElement.TryGetProperty("message", out var msgProp) 
                    ? msgProp.GetString() 
                    : $"HTTP error: {(int)response.StatusCode}";
                int? retryAfter = jsonDoc.RootElement.TryGetProperty("retry_after", out var retryProp) 
                    ? retryProp.GetInt32() 
                    : null;
                
                throw new APIException(errorMsg, errorCode, (int)response.StatusCode, retryAfter);
            }
            
            if (jsonDoc.RootElement.TryGetProperty("status", out var statusProp) && 
                statusProp.GetString() == "error")
            {
                var errorCode = jsonDoc.RootElement.TryGetProperty("code", out var codeProp) 
                    ? codeProp.GetString() 
                    : "api_error";
                var errorMsg = jsonDoc.RootElement.TryGetProperty("message", out var msgProp) 
                    ? msgProp.GetString() 
                    : "Unknown API error";
                int? retryAfter = jsonDoc.RootElement.TryGetProperty("retry_after", out var retryProp) 
                    ? retryProp.GetInt32() 
                    : null;
                
                throw new APIException(errorMsg, errorCode, (int)response.StatusCode, retryAfter);
            }
            
            return jsonDoc;
        }
        
        /// <summary>
        /// Translate text
        /// </summary>
        /// <param name="text">Text to translate</param>
        /// <param name="targetLang">Target language code (e.g., "fr")</param>
        /// <param name="sourceLang">Source language code (optional, auto-detect if null)</param>
        /// <param name="formality">Formality level: "formal" or "informal" (optional)</param>
        /// <returns>Translation result</returns>
        public async Task<TranslationResult> TranslateAsync(string text, string targetLang, 
                                                           string sourceLang = null, string formality = null)
        {
            var data = new Dictionary<string, object>
            {
                { "text", text },
                { "target_lang", targetLang }
            };
            
            if (!string.IsNullOrEmpty(sourceLang))
            {
                data["source_lang"] = sourceLang;
            }
            
            if (!string.IsNullOrEmpty(formality))
            {
                data["formality"] = formality;
            }
            
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            using var response = await MakeRequestAsync("translate", HttpMethod.Post, content);
            var root = response.RootElement;
            
            return new TranslationResult(
                root.GetProperty("translated_text").GetString(),
                root.GetProperty("source_lang").GetString(),
                root.GetProperty("target_lang").GetString(),
                root.TryGetProperty("detected_lang", out var detectedProp) ? detectedProp.GetString() : null,
                root.TryGetProperty("characters_used", out var charsProp) ? charsProp.GetInt32() : 0,
                root.TryGetProperty("processing_time_ms", out var timeProp) ? timeProp.GetInt32() : 0
            );
        }
        
        /// <summary>
        /// Translate document
        /// </summary>
        /// <param name="filePath">Path to document file (PDF, DOCX, TXT)</param>
        /// <param name="targetLang">Target language code</param>
        /// <param name="sourceLang">Source language code (optional)</param>
        /// <returns>Document translation result</returns>
        public async Task<DocumentTranslationResult> TranslateDocumentAsync(string filePath, string targetLang, 
                                                                           string sourceLang = null)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }
            
            using var formData = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(filePath);
            var fileContent = new StreamContent(fileStream);
            formData.Add(fileContent, "file", Path.GetFileName(filePath));
            formData.Add(new StringContent(targetLang), "target_lang");
            
            if (!string.IsNullOrEmpty(sourceLang))
            {
                formData.Add(new StringContent(sourceLang), "source_lang");
            }
            
            using var response = await MakeRequestAsync("translate/document", HttpMethod.Post, formData);
            var root = response.RootElement;
            
            return new DocumentTranslationResult(
                root.GetProperty("translated_text").GetString(),
                root.GetProperty("source_lang").GetString(),
                root.GetProperty("target_lang").GetString(),
                root.TryGetProperty("pages", out var pagesProp) ? pagesProp.GetInt32() : 0,
                root.TryGetProperty("characters_used", out var charsProp) ? charsProp.GetInt32() : 0,
                root.TryGetProperty("processing_time_ms", out var timeProp) ? timeProp.GetInt32() : 0
            );
        }
        
        /// <summary>
        /// Extract text from image (OCR)
        /// </summary>
        /// <param name="filePath">Path to image file (PNG, JPG, WEBP, BMP)</param>
        /// <param name="lang">Expected language (optional, improves accuracy)</param>
        /// <param name="enhance">Apply image enhancement (optional)</param>
        /// <returns>OCR result</returns>
        public async Task<OCRResult> OcrAsync(string filePath, string lang = null, bool enhance = false)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }
            
            using var formData = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(filePath);
            var fileContent = new StreamContent(fileStream);
            formData.Add(fileContent, "image", Path.GetFileName(filePath));
            
            if (!string.IsNullOrEmpty(lang))
            {
                formData.Add(new StringContent(lang), "lang");
            }
            
            if (enhance)
            {
                formData.Add(new StringContent("true"), "enhance");
            }
            
            using var response = await MakeRequestAsync("ocr", HttpMethod.Post, formData);
            var root = response.RootElement;
            
            return new OCRResult(
                root.GetProperty("text").GetString(),
                root.TryGetProperty("confidence", out var confProp) ? confProp.GetDouble() : 0.0,
                root.TryGetProperty("language_detected", out var langProp) ? langProp.GetString() : "",
                root.TryGetProperty("processing_time_ms", out var timeProp) ? timeProp.GetInt32() : 0
            );
        }
        
        /// <summary>
        /// Detect language of text
        /// </summary>
        /// <param name="text">Text to analyze</param>
        /// <returns>Language detection result</returns>
        public async Task<LanguageDetectionResult> DetectLanguageAsync(string text)
        {
            var data = new Dictionary<string, object> { { "text", text } };
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            using var response = await MakeRequestAsync("detect", HttpMethod.Post, content);
            var root = response.RootElement;
            
            var alternatives = new List<LanguageAlternative>();
            if (root.TryGetProperty("alternatives", out var altsProp))
            {
                foreach (var alt in altsProp.EnumerateArray())
                {
                    alternatives.Add(new LanguageAlternative(
                        alt.GetProperty("language").GetString(),
                        alt.GetProperty("confidence").GetDouble(),
                        alt.TryGetProperty("language_name", out var nameProp) ? nameProp.GetString() : ""
                    ));
                }
            }
            
            return new LanguageDetectionResult(
                root.GetProperty("language").GetString(),
                root.TryGetProperty("language_name", out var nameProp) ? nameProp.GetString() : "",
                root.TryGetProperty("confidence", out var confProp) ? confProp.GetDouble() : 0.0,
                alternatives
            );
        }
        
        /// <summary>
        /// Get supported languages
        /// </summary>
        /// <returns>List of supported languages</returns>
        public async Task<List<Language>> GetSupportedLanguagesAsync()
        {
            using var response = await MakeRequestAsync("languages", HttpMethod.Get);
            var root = response.RootElement;
            
            var languages = new List<Language>();
            if (root.TryGetProperty("languages", out var langsProp))
            {
                foreach (var lang in langsProp.EnumerateArray())
                {
                    languages.Add(new Language(
                        lang.GetProperty("code").GetString(),
                        lang.GetProperty("name").GetString(),
                        lang.TryGetProperty("flag", out var flagProp) ? flagProp.GetString() : ""
                    ));
                }
            }
            
            return languages;
        }
        
        /// <summary>
        /// Batch translate multiple texts
        /// </summary>
        /// <param name="texts">Array of texts to translate</param>
        /// <param name="targetLang">Target language code</param>
        /// <param name="sourceLang">Source language code (optional)</param>
        /// <returns>Batch translation result</returns>
        public async Task<Dictionary<string, string>> BatchTranslateAsync(string[] texts, string targetLang, 
                                                                         string sourceLang = null)
        {
            var data = new Dictionary<string, object>
            {
                { "texts", texts },
                { "target_lang", targetLang }
            };
            
            if (!string.IsNullOrEmpty(sourceLang))
            {
                data["source_lang"] = sourceLang;
            }
            
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            using var response = await MakeRequestAsync("translate/batch", HttpMethod.Post, content);
            var root = response.RootElement;
            
            var result = new Dictionary<string, string>();
            if (root.TryGetProperty("translations", out var transProp))
