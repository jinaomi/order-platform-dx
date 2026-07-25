using CaseMngmt.Models.Keywords;
using CaseMngmt.Service.Keywords;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CaseMngmt.Server.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/keywords")]
    public class KeywordController : ControllerBase
    {
        private readonly ILogger<KeywordController> _logger;
        private readonly IKeywordService _keywordService;

        public KeywordController(ILogger<KeywordController> logger, IKeywordService keywordService)
        {
            _logger = logger;
            _keywordService = keywordService;
        }

        [HttpGet]
        public async Task<IActionResult> GetByTemplate([FromQuery] Guid templateId)
        {
            if (templateId == Guid.Empty)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _keywordService.GetByTemplateIdForBuilderAsync(templateId);
                return result != null ? Ok(result) : NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(KeywordController), true, e);
                return BadRequest();
            }
        }

        [ClaimRequirement(ClaimTypes.Role, "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create(KeywordRequest request)
        {
            if (!ModelState.IsValid || request == null)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _keywordService.AddAsync(request);
                return result > 0 ? Ok(result) : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(KeywordController), true, e);
                return BadRequest();
            }
        }

        [ClaimRequirement(ClaimTypes.Role, "SuperAdmin")]
        [HttpPut, Route("{id}")]
        public async Task<IActionResult> Update(Guid id, KeywordRequest request)
        {
            if (id == Guid.Empty || !ModelState.IsValid || request == null)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _keywordService.UpdateAsync(id, request);
                return result > 0 ? Ok(result) : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(KeywordController), true, e);
                return BadRequest();
            }
        }

        [ClaimRequirement(ClaimTypes.Role, "SuperAdmin")]
        [HttpDelete, Route("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _keywordService.SoftDeleteAsync(id);
                if (result == -1)
                {
                    return Conflict("Keyword đang được dùng trong Cases, không thể xóa.");
                }
                return result > 0 ? Ok() : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(KeywordController), true, e);
                return BadRequest();
            }
        }

        [ClaimRequirement(ClaimTypes.Role, "SuperAdmin")]
        [HttpPatch, Route("reorder")]
        public async Task<IActionResult> Reorder(List<KeywordReorderRequest> items)
        {
            if (items == null || !items.Any())
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _keywordService.ReorderAsync(items);
                return result > 0 ? Ok() : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(KeywordController), true, e);
                return BadRequest();
            }
        }
    }
}
