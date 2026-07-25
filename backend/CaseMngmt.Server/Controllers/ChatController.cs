using CaseMngmt.Models.Chat;
using CaseMngmt.Service.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaseMngmt.Server.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IChatAssistantService _service;

        public ChatController(ILogger<ChatController> logger, IChatAssistantService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpPost, Route("message")]
        public async Task<IActionResult> Message([FromBody] ChatMessageRequest request)
        {
            try
            {
                var currentCompanyId = User?.FindFirst("CompanyId")?.Value;
                if (string.IsNullOrEmpty(currentCompanyId) || string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest();
                }

                var reply = await _service.AskAsync(Guid.Parse(currentCompanyId), request.Message, request.History);
                return Ok(new ChatMessageResponse { Reply = reply });
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(ChatController), true, e);
                return BadRequest();
            }
        }
    }
}
