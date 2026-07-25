using CaseMngmt.Service.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaseMngmt.Server.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IDashboardService _service;
        private readonly IDashboardCommentService _commentService;

        public DashboardController(ILogger<DashboardController> logger, IDashboardService service, IDashboardCommentService commentService)
        {
            _logger = logger;
            _service = service;
            _commentService = commentService;
        }

        [HttpGet, Route("summary")]
        public async Task<IActionResult> Summary()
        {
            try
            {
                var currentCompanyId = User?.FindFirst("CompanyId")?.Value;
                if (string.IsNullOrEmpty(currentCompanyId))
                {
                    return BadRequest();
                }

                var result = await _service.GetSummaryAsync(Guid.Parse(currentCompanyId));
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(DashboardController), true, e);
                return BadRequest();
            }
        }

        [HttpGet, Route("ai-comment")]
        public async Task<IActionResult> AiComment()
        {
            try
            {
                var currentCompanyId = User?.FindFirst("CompanyId")?.Value;
                if (string.IsNullOrEmpty(currentCompanyId))
                {
                    return BadRequest();
                }

                var result = await _commentService.GenerateCommentAsync(Guid.Parse(currentCompanyId));
                return result == null ? NoContent() : Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(DashboardController), true, e);
                return NoContent();
            }
        }
    }
}
