using CaseMngmt.Models.KeywordRoles;
using CaseMngmt.Service.KeywordRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaseMngmt.Server.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionController : ControllerBase
    {
        private readonly ILogger<PermissionController> _logger;
        private readonly IKeywordRoleService _service;

        public PermissionController(ILogger<PermissionController> logger, IKeywordRoleService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet]
        [Route("keyword")]
        public async Task<IActionResult> Get(Guid roleId)
        {
            if (roleId == Guid.Empty)
            {
                return BadRequest();
            }

            try
            {
                var result = await _service.GetByRoleIdAsync(roleId);
                return result != null && result.Any() ? Ok(result) : NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(PermissionController), true, e);
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("keyword")]
        public async Task<IActionResult> Add(List<KeywordRole> request)
        {
            if (!ModelState.IsValid || !request.Any())
            {
                return BadRequest();
            }

            try
            {
                var result = await _service.AddMultiAsync(request);
                return result > 0 ? Ok(result) : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(PermissionController), true, e);
                return BadRequest();
            }
        }

        [HttpPut]
        [Route("keyword")]
        public async Task<IActionResult> Update(Guid roleId, List<KeywordRole> keywordRoles)
        {
            if (!ModelState.IsValid || roleId == Guid.Empty)
            {
                return BadRequest();
            }

            try
            {
                var result = await _service.UpdateMultiAsync(roleId, keywordRoles);
                return result > 0 ? Ok(result) : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(PermissionController), true, e);
                return BadRequest();
            }
        }

        //[HttpGet]
        //[Route("file-type")]
        //public async Task<IActionResult> GetAllFileType()
        //{
        //    try
        //    {
        //        var result = await _service.GetAllAsync(true);
        //        return result != null && result.Any() ? Ok(result) : NotFound();
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError(e.Message, nameof(PermissionController), true, e);
        //        return BadRequest();
        //    }
        //}
    }
}
