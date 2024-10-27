namespace BackTranslatorSimultaneous.Services.Interfaces;

public interface IOllamaService
{
    Task<string> GenerateResponseAsync(string prompt);
}
