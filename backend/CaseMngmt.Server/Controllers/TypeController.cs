using CaseMngmt.Service.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaseMngmt.Server.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public class TypeController : ControllerBase
    {
        private readonly ILogger<TypeController> _logger;
        private readonly ITypeService _typeService;


        public TypeController(ILogger<TypeController> logger, ITypeService typeService)
        {
            _logger = logger;
            _typeService = typeService;
        }

        [HttpGet]
        [Route("type")]
        public async Task<IActionResult> GetAllType()
        {
            try
            {
                var result = await _typeService.GetAllAsync(false);
                return result != null && result.Any() ? Ok(result) : NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(TypeController), true, e);
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("file-type")]
        public async Task<IActionResult> GetAllFileType()
        {
            try
            {
                var result = await _typeService.GetAllAsync(true);
                return result != null && result.Any() ? Ok(result) : NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(TypeController), true, e);
                return BadRequest();
            }
        }
    }
}
