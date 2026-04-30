# TranslateAPI .NET SDK

[![NuGet Version](https://img.shields.io/nuget/v/TranslateAPI.svg)](https://www.nuget.org/packages/TranslateAPI)
[![NuGet Downloads](https://img.shields.io/nuget/dt/TranslateAPI.svg)](https://www.nuget.org/packages/TranslateAPI)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Documentation](https://img.shields.io/badge/docs-deeptranslate.online-blue.svg)](https://deeptranslate.online/docs)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)

Official .NET/C# client library for the TranslateAPI translation service. Translate text, documents, and images between 70+ languages with a simple, intuitive interface. Supports .NET Framework, .NET Core, .NET 5+, and Xamarin.

## Features

- **Text Translation**: Translate text between 70+ languages
- **Document Translation**: Support for PDF, DOCX, TXT files
- **Image OCR**: Extract and translate text from images
- **Language Detection**: Automatically detect language of text
- **Batch Translation**: Translate multiple texts in a single request
- **Account Management**: Check usage, credits, and account info
- **Async/Await Support**: Full async/await pattern support
- **Dependency Injection**: Built-in DI support for ASP.NET Core
- **Cross-Platform**: Works on Windows, Linux, macOS, iOS, Android
- **Strongly Typed**: Full C# type safety with modern language features

## Installation

### Package Manager

```powershell
Install-Package TranslateAPI
```

### .NET CLI

```bash
dotnet add package TranslateAPI
```

### PackageReference

```xml
<PackageReference Include="TranslateAPI" Version="1.0.0" />
```

### Manual Installation

```bash
git clone https://github.com/forcekeys/translate-api-dotnet.git
cd translate-api-dotnet
dotnet build
```

## Quick Start

### 1. Get Your API Key

First, sign up at [deeptranslate.online](https://deeptranslate.online) to get your free API key.

### 2. Basic Usage

```csharp
using TranslateAPI;

// Initialize with your API key
var api = new TranslateAPI("your_api_key_here");

// Translate text
var result = await api.TranslateAsync("Hello, world!", "en", "fr");
Console.WriteLine($"Translated: {result.TranslatedText}");
Console.WriteLine($"Characters used: {result.CharactersUsed}");
Console.WriteLine($"Processing time: {result.ProcessingTimeMs}ms");

// Auto-detect source language
var autoResult = await api.TranslateAsync("Bonjour le monde", null, "en");
Console.WriteLine($"Detected language: {autoResult.SourceLang}");
Console.WriteLine($"Translated: {autoResult.TranslatedText}");
```

## Comprehensive Examples

### Text Translation

```csharp
using TranslateAPI;
using TranslateAPI.Models;

var api = new TranslateAPI("your_api_key_here");

// Basic translation
var result = await api.TranslateAsync(
    "Hello, how are you?",
    "en",
    "es"
);

// With formality control
var options = new TranslationOptions
{
    Formality = Formality.Formal  // or Formality.Informal
};

var formalResult = await api.TranslateAsync(
    "Hello, how are you?",
    "en",
    "de",
    options
);

// Translation with context
var contextOptions = new TranslationOptions
{
    Context = "financial"  // Helps with ambiguous words
};

var contextResult = await api.TranslateAsync(
    "The bank is closed on Sunday.",
    "en",
    "fr",
    contextOptions
);
```

### Document Translation

```csharp
using System.IO;

// Translate a document file
var result = await api.TranslateDocumentAsync(
    "document.pdf",
    "en",
    "es"
);

// Save translated text to file
await File.WriteAllTextAsync("translated_document.txt", result.TranslatedText);

Console.WriteLine($"Translated {result.Pages} pages");
Console.WriteLine($"Used {result.CharactersUsed} characters");
```

### Image OCR and Translation

```csharp
// Extract text from image and translate
var result = await api.OcrAndTranslateAsync(
    "receipt.png",
    "en",
    "fr"
);

Console.WriteLine($"Extracted text: {result.ExtractedText}");
Console.WriteLine($"Translated text: {result.TranslatedText}");
Console.WriteLine($"Confidence: {result.Confidence}%");
```

### Language Detection

```csharp
// Detect language of text
var detection = await api.DetectAsync("Bonjour le monde");

Console.WriteLine($"Detected language: {detection.Language}");
Console.WriteLine($"Language name: {detection.LanguageName}");
Console.WriteLine($"Confidence: {detection.Confidence}%");

// Show alternative possibilities
foreach (var alt in detection.Alternatives)
{
    Console.WriteLine($"  - {alt.Language}: {alt.Confidence}%");
}
```

### Batch Translation

```csharp
// Translate multiple texts at once
var texts = new List<string>
{
    "Hello",
    "Goodbye",
    "Thank you",
    "Please"
};

var results = await api.BatchTranslateAsync(
    texts,
    "en",
    "de"
);

foreach (var item in results.Translations)
{
    Console.WriteLine($"{item.Original} => {item.Translated}");
}
```

### Account Information

```csharp
// Get account details
var account = await api.GetAccountAsync();

Console.WriteLine($"Email: {account.Email}");
Console.WriteLine($"Plan: {account.Plan}");
Console.WriteLine($"Status: {account.Status}");

// Usage statistics
var limits = account.PlanLimits;
Console.WriteLine($"Daily translations: {limits.TodayUsed}/{limits.DailyTranslations}");
Console.WriteLine($"Remaining today: {limits.RemainingToday}");

// Balance information
var balance = account.Balance;
Console.WriteLine($"Available balance: ${balance.Available:F2}");
Console.WriteLine($"Total spent: ${balance.TotalSpent:F2}");
```

### Supported Languages

```csharp
// Get all supported languages
var languages = await api.GetLanguagesAsync();

Console.WriteLine($"Total languages: {languages.Count}");
foreach (var lang in languages.Languages)
{
    Console.WriteLine($"{lang.Flag} {lang.Code}: {lang.Name}");
}
```

## Advanced Configuration

### Custom HttpClient

```csharp
using System.Net.Http;

// Create custom HttpClient
var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(30),
    BaseAddress = new Uri("https://api.deeptranslate.online/api/v1")
};

var api = new TranslateAPI("your_api_key", httpClient);
```

### Configuration with Options

```csharp
var options = new TranslateAPIOptions
{
    ApiKey = "your_api_key",
    BaseUrl = "https://api.deeptranslate.online/api/v1",
    Timeout = TimeSpan.FromSeconds(30),
    RetryCount = 3,
    DefaultSourceLanguage = "en",
    DefaultTargetLanguage = "fr"
};

var api = new TranslateAPI(options);
```

### Environment Variables

```csharp
// Read API key from environment variable
var apiKey = Environment.GetEnvironmentVariable("FORCEKEYS_API_KEY");
var api = new TranslateAPI(apiKey);
```

### Error Handling

```csharp
using TranslateAPI.Exceptions;

try
{
    var result = await api.TranslateAsync("Hello", "en", "fr");
}
catch (APIException ex)
{
    Console.WriteLine($"API Error: {ex.Code} - {ex.Message}");
    Console.WriteLine($"Status Code: {ex.StatusCode}");
    
    if (ex.Code == "rate_limit_exceeded")
    {
        Console.WriteLine($"Retry after: {ex.RetryAfter} seconds");
    }
    else if (ex.Code == "insufficient_credits")
    {
        Console.WriteLine("Please add credits to your account");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

### ASP.NET Core Integration

```csharp
// Startup.cs or Program.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddTranslateAPI(options =>
    {
        options.ApiKey = Configuration["ForceKeys:ApiKey"];
        options.BaseUrl = Configuration["ForceKeys:BaseUrl"];
    });
    
    services.AddControllers();
}

// Usage in controller
[ApiController]
[Route("api/[controller]")]
public class TranslationController : ControllerBase
{
    private readonly ITranslateAPI _api;
    
    public TranslationController(ITranslateAPI api)
    {
        _api = api;
    }
    
    [HttpPost("translate")]
    public async Task<IActionResult> Translate([FromBody] TranslationRequest request)
    {
        var result = await _api.TranslateAsync(
            request.Text,
            request.Source,
            request.Target
        );
        
        return Ok(result);
    }
}
```

### Dependency Injection

```csharp
// Register with DI container
services.AddSingleton<ITranslateAPI>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new TranslateAPI(config["ForceKeys:ApiKey"]);
});

// Use in service
public class TranslationService
{
    private readonly ITranslateAPI _api;
    
    public TranslationService(ITranslateAPI api)
    {
        _api = api;
    }
    
    public async Task<string> TranslateAsync(string text, string targetLang)
    {
        var result = await _api.TranslateAsync(text, null, targetLang);
        return result.TranslatedText;
    }
}
```

## API Reference

### TranslateAPI Class

```csharp
// Constructor with API key
var api = new TranslateAPI(apiKey);

// Constructor with options
var api = new TranslateAPI(options);

// Constructor with HttpClient
var api = new TranslateAPI(apiKey, httpClient);
```

#### Methods

All methods are async and return `Task<T>`.

| Method | Description | Parameters |
|--------|-------------|------------|
| `TranslateAsync(text, source, target)` | Translate text | `text`: Text to translate<br>`source`: Source language code (null for auto-detect)<br>`target`: Target language code |
| `TranslateAsync(text, source, target, options)` | Translate text with options | `text`: Text to translate<br>`source`: Source language code<br>`target`: Target language code<br>`options`: Translation options |
| `TranslateDocumentAsync(filePath, source, target)` | Translate document file | `filePath`: Path to document (PDF, DOCX, TXT)<br>`source`: Source language code<br>`target`: Target language code |
| `OcrAndTranslateAsync(imagePath, source, target)` | Extract text from image and translate | `imagePath`: Path to image file<br>`source`: Source language code<br>`target`: Target language code |
| `DetectAsync(text)` | Detect language of text | `text`: Text to analyze |
| `BatchTranslateAsync(texts, source, target)` | Translate multiple texts | `texts`: List of texts to translate<br>`source`: Source language code<br>`target`: Target language code |
| `GetLanguagesAsync()` | Get supported languages | |
| `GetAccountAsync()` | Get account information | |

### Response Objects

All methods return strongly typed response objects with the following common properties:

- `Status`: "success" or "error"
- `ProcessingTimeMs`: Processing time in milliseconds
- `CharactersUsed`: Number of characters used

#### TranslationResponse
- `TranslatedText`: Translated text
- `SourceLang`: Source language code
- `TargetLang`: Target language code

#### DocumentTranslationResponse
- `TranslatedText`: Translated text
- `Pages`: Number of pages processed
- `CharactersUsed`: Characters used

#### OCRResponse
- `ExtractedText`: Text extracted from image
- `TranslatedText`: Translated text (if translation requested)
- `Confidence`: OCR confidence percentage
- `LanguageDetected`: Detected language in image

#### DetectionResponse
- `Language`: Detected language code
- `LanguageName`: Full language name
- `Confidence`: Detection confidence percentage
- `Alternatives`: List of alternative possibilities

#### AccountResponse
- `Email`: User email
- `Plan`: Subscription plan
- `Status`: Account status
- `PlanLimits`: Object with usage limits
- `Balance`: Object with balance information
- `Statistics`: Usage statistics

## Error Codes

The SDK throws `APIException` for API errors:

| Code | Description | HTTP Status |
|------|-------------|-------------|
| `invalid_request` | Missing or malformed parameters | 400 |
| `unauthorized` | Invalid or missing API key | 401 |
| `forbidden` | Feature not available on your plan | 403 |
| `payload_too_large` | File or text exceeds size limit | 413 |
| `unsupported_language` | Language code not supported | 422 |
| `rate_limit_exceeded` | Too many requests | 429 |
| `insufficient_credits` | Not enough credits | 402 |
| `internal_error` | Server error | 500 |

## Rate Limits

Rate limits vary by plan:

| Plan | Requests/Minute | Monthly Requests | Max Characters/Request |
|------|----------------|------------------|------------------------|
| Free | 10 | 500/day | 2,000 |
| Starter | 60 | 50,000 | 5,000 |
| Professional | 300 | 1,000,000 | 10,000 |
| Enterprise | Unlimited | Unlimited | Unlimited |

## Platform Support

- **.NET Framework**: 4.6.1+
- **.NET Core**: 2.0+
- **.NET Standard**: 2.0+
- **.NET 5+**: Full support
- **Xamarin.iOS**: 10.0+
- **Xamarin.Android**: 7.0+
- **UWP**: 10.0.16299+
- **Unity**: 2018.1+

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Support

- **Documentation**: [deeptranslate.online/docs](https://deeptranslate.online/docs)
- **Issues**: [GitHub Issues](https://github.com/forcekeys/translate-api-dotnet/issues)
- **Email**: support@deeptranslate.online
- **Discord**: [Join our Discord](https://discord.gg/forcekeys)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Related Projects

- [TranslateAPI Python SDK](https://github.com/forcekeys/translate-api-python)
- [TranslateAPI PHP SDK](https://github.com/forcekeys/translate-api-php)
- [TranslateAPI Java SDK](https://github.com/forcekeys/translate-api-java)
- [TranslateAPI JavaScript SDK](https://github.com/forcekeys/translate-api-js)
- [TranslateAPI Shell](https://github.com/forcekeys/translate-api-shell)

