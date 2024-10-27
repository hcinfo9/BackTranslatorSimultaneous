using BackTranslatorSimultaneous.Services.Interfaces;
using BackTranslatorSimultaneous.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using BackTranslatorSimultaneous.Hubs;

namespace BackTranslatorSimultaneous.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationController : ControllerBase
    {
        private readonly ITranslationService _translationService;
        private readonly IHubContext<TranslationHub> _hubContext;

        public TranslationController(ITranslationService translationService, IHubContext<TranslationHub> hubContext)
        {
            _translationService = translationService;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Translate([FromBody] TranslateRequestDTO request)
        {
            var translatedText = await _translationService.TranslateTextAsync(request.Text, request.SourceLanguage, request.TargetLanguage);

            // Envia o texto traduzido para o frontend via WebSocket
            await _hubContext.Clients.All.SendAsync("ReceiveTranslation", translatedText);

            return Ok(new { TranslatedText = translatedText });
        }
    }
}
