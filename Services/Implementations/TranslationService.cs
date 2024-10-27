using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using BackTranslatorSimultaneous.DTOs;
using BackTranslatorSimultaneous.Services.Interfaces;

namespace BackTranslatorSimultaneous.Services;

public class TranslationService : ITranslationService
{
    private readonly HttpClient _httpClient;
    private readonly TranslationSettingsDTO _settings;

    public TranslationService(HttpClient httpClient, IOptions<TranslationSettingsDTO> settings)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

        if (string.IsNullOrEmpty(_settings.Endpoint))
        {
            throw new ArgumentNullException(nameof(_settings.Endpoint), "Endpoint cannot be null or empty.");
        }

        _httpClient.BaseAddress = new Uri(_settings.Endpoint);


        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _settings.Key);
        _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", _settings.Region);
        _httpClient.BaseAddress = new Uri(_settings.Endpoint);
    }

    public async Task<string> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage)
    {

        var requestBody = new[]{
            new { Text = text }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"/translate?api-version=3.0&from={sourceLanguage}&to={targetLanguage}", content);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Erro ao traduzir texto.");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var result = JsonSerializer.Deserialize<List<TranslationResult>>(jsonResponse, options);

        return result?.FirstOrDefault()?.Translations?.FirstOrDefault()?.Text ?? string.Empty;

    }


    public class TranslationResult
    {
        public List<Translation> Translations { get; set; }
    }

    public class Translation
    {
        public string Text { get; set; }
        public string To { get; set; }
    }
}