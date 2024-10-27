using BackTranslatorSimultaneous.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SpeechController : ControllerBase
{
    private readonly ISpeechService _speechService;

    public SpeechController(ISpeechService speechService)
    {
        _speechService = speechService;
    }

    [HttpPost("start")]
    public IActionResult StartRecognition()
    {
        _speechService.StartRecognition();
        return Ok("Reconhecimento de fala iniciado.");
    }

    [HttpPost("stop")]
    public IActionResult StopRecognition()
    {
        _speechService.StopRecognition();

        // Obtenha o texto consolidado
        var finalTranscript = _speechService.GetFinalTranscript();

        // Retorne o texto completo reconhecido
        return Ok(new { TextoReconhecido = finalTranscript });
    }
}
