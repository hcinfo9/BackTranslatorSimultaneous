using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BackTranslatorSimultaneous.Services.Interfaces;
using BackTranslatorSimultaneous.DTOs;

namespace BackTranslatorSimultaneous.Controllers
{
    [ApiController]
    [Route("api")]
    public class ChatController : ControllerBase
    {
        private readonly IOllamaService _ollamaService;

        public ChatController(IOllamaService ollamaService)
        {
            _ollamaService = ollamaService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateResponse([FromBody] GenerateRequestDTO request)
        {
            var response = await _ollamaService.GenerateResponseAsync(request.Prompt);
            return Ok(response);
        }
    }
}