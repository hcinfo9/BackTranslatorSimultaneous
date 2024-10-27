using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BackTranslatorSimultaneous.Services.Interfaces;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OllamaService> _logger;

    public OllamaService(HttpClient httpClient, IConfiguration configuration, ILogger<OllamaService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GenerateResponseAsync(string prompt)
    {
        try
        {
            string url = $"{_configuration["Ollama:BaseUrl"]}/generate";

            var request = new StringContent(
                JsonSerializer.Serialize(new { model = "llama3.2", prompt = prompt }), Encoding.UTF8, "application/json"
            );

            _logger.LogInformation("Sending request to Ollama API at {url}.", url);

            var response = await _httpClient.PostAsync(url, request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to send request to Ollama API. Status Code: {StatusCode}, Reason: {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                return $"Error: {response.ReasonPhrase}";
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Received response content: {responseContent}", responseContent);

            var responseLines = responseContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var fullResponse = new StringBuilder();

            foreach (var line in responseLines)
            {
                _logger.LogInformation("Processing line: {line}", line);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var jsonResponse = JsonSerializer.Deserialize<OllamaResponse>(line, options);
                if (jsonResponse != null)
                {
                    fullResponse.Append(jsonResponse.Response);
                }
                else
                {
                    _logger.LogError("Failed to deserialize line: {line}", line);
                }
            }

            var finalResponse = fullResponse.ToString();
            _logger.LogInformation("Final response: {finalResponse}", finalResponse);
            return finalResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending request to Ollama API.");
            return "Error: An error occurred while sending request to Ollama API.";
        }
    }

    private class OllamaResponse
    {
        public string Model { get; set; }
        public string CreatedAt { get; set; }
        public string Response { get; set; }
        public bool Done { get; set; }
    }
}