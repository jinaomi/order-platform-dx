using CaseMngmt.Models.Orders;
using CaseMngmt.Service.Ai;
using CaseMngmt.Service.AiMatching;
using CaseMngmt.Service.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CaseMngmt.Server.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IOrderService _service;
        private readonly IAiMatchingService _aiMatchingService;
        private readonly IAiOrderExtractionService _extractionService;

        private static readonly Dictionary<string, string> AcceptedMediaTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".pdf", "application/pdf" }
        };
        private const long MaxExtractFileSizeBytes = 15 * 1024 * 1024; // 15MB

        public OrderController(
            ILogger<OrderController> logger,
            IOrderService service,
            IAiMatchingService aiMatchingService,
            IAiOrderExtractionService extractionService)
        {
            _logger = logger;
            _service = service;
            _aiMatchingService = aiMatchingService;
            _extractionService = extractionService;
        }

        [HttpGet, Route("getAll")]
        public async Task<IActionResult> GetAll(string? status = null, Guid? customerId = null, int? pageSize = 25, int? pageNumber = 1)
        {
            try
            {
                var currentCompanyId = User?.FindFirst("CompanyId")?.Value;
                if (string.IsNullOrEmpty(currentCompanyId))
                {
                    return BadRequest();
                }

                var result = await _service.GetAllOrdersAsync(Guid.Parse(currentCompanyId), status, customerId, pageSize ?? 25, pageNumber ?? 1);
                return result != null && result.Items.Any() ? Ok(result) : NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(OrderController), true, e);
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentCompanyId = User?.FindFirst("CompanyId")?.Value;
                if (string.IsNullOrEmpty(currentCompanyId))
                {
                    return BadRequest();
                }

                var result = await _service.GetByIdAsync(id, Guid.Parse(currentCompanyId));
                return result == null ? NotFound() : Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(OrderController), true, e);
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderRequest request)
        {
            if (!ModelState.IsValid || request == null)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentCompanyId = User?.FindFirst("CompanyId")?.Value;
                var currentUserId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentCompanyId) || string.IsNullOrEmpty(currentUserId))
                {
                    return BadRequest();
                }

                request.CompanyId = Guid.Parse(currentCompanyId);

                var result = await _service.CreateOrderAsync(request, Guid.Parse(currentUserId));
                if (result == null)
                {
                    return BadRequest();
                }

                // Run AI照合 (inventory/capacity risk matching) right after order confirm so
                // the risk chip is already populated when the user views the order.
                // A failure here (e.g. Anthropic API down) must not fail order creation.
                try
                {
                    await _aiMatchingService.RunMatchingAsync(result.Value, Guid.Parse(currentCompanyId), Guid.Parse(currentUserId));
                }
                catch (Exception matchEx)
                {
                    _logger.LogError(matchEx.Message, nameof(OrderController), true, matchEx);
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(OrderController), true, e);
                return BadRequest();
            }
        }

        [HttpPost, Route("extract")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> Extract(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("ファイルを選択してください。");
            }

            if (file.Length > MaxExtractFileSizeBytes)
            {
                return BadRequest("ファイルサイズが大きすぎます（上限15MB）。");
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(extension) || !AcceptedMediaTypes.TryGetValue(extension, out var mediaType))
            {
                return BadRequest("対応していないファイル形式です。JPEG・PNG・PDFのみアップロードできます。");
            }

            try
            {
                var currentCompanyId = User?.FindFirst("CompanyId")?.Value;
                if (string.IsNullOrEmpty(currentCompanyId))
                {
                    return BadRequest();
                }

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                var result = await _extractionService.ExtractAsync(fileBytes, mediaType, Guid.Parse(currentCompanyId));
                return result == null
                    ? BadRequest("画像からの情報抽出に失敗しました。もう一度お試しいただくか、手動で入力してください。")
                    : Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(OrderController), true, e);
                return BadRequest();
            }
        }

        [HttpPost, Route("{id}/match")]
        public async Task<IActionResult> RunMatching(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            try
            {
                var currentCompanyId = User?.FindFirst("CompanyId")?.Value;
                var currentUserId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentCompanyId) || string.IsNullOrEmpty(currentUserId))
                {
                    return BadRequest();
                }

                var result = await _aiMatchingService.RunMatchingAsync(id, Guid.Parse(currentCompanyId), Guid.Parse(currentUserId));
                return result == null ? NotFound() : Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(OrderController), true, e);
                return BadRequest();
            }
        }

        [HttpGet, Route("{id}/risk")]
        public async Task<IActionResult> GetRisk(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            try
            {
                var currentCompanyId = User?.FindFirst("CompanyId")?.Value;
                if (string.IsNullOrEmpty(currentCompanyId))
                {
                    return BadRequest();
                }

                var result = await _aiMatchingService.GetLatestAsync(id, Guid.Parse(currentCompanyId));
                return result == null ? NotFound() : Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(OrderController), true, e);
                return BadRequest();
            }
        }

        [HttpPut, Route("{id}")]
        public async Task<IActionResult> Update(Guid id, OrderRequest request)
        {
            if (!ModelState.IsValid || id == Guid.Empty)
            {
                return BadRequest();
            }

            try
            {
                var currentCompanyId = User?.FindFirst("CompanyId")?.Value;
                var currentUserId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentCompanyId) || string.IsNullOrEmpty(currentUserId))
                {
                    return BadRequest();
                }

                request.CompanyId = Guid.Parse(currentCompanyId);

                var result = await _service.UpdateOrderAsync(id, request, Guid.Parse(currentUserId));
                return result > 0 ? Ok(result) : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(OrderController), true, e);
                return BadRequest();
            }
        }

        [HttpPut, Route("status")]
        public async Task<IActionResult> UpdateStatus(Guid id, string status)
        {
            if (id == Guid.Empty || string.IsNullOrEmpty(status))
            {
                return BadRequest();
            }

            try
            {
                var currentCompanyId = User?.FindFirst("CompanyId")?.Value;
                var currentUserId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentCompanyId) || string.IsNullOrEmpty(currentUserId))
                {
                    return BadRequest();
                }

                var result = await _service.UpdateStatusAsync(id, Guid.Parse(currentCompanyId), status, Guid.Parse(currentUserId));
                return result > 0 ? Ok(result) : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(OrderController), true, e);
                return BadRequest();
            }
        }

        [HttpDelete, Route("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentCompanyId = User?.FindFirst("CompanyId")?.Value;
                var currentUserId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentCompanyId) || string.IsNullOrEmpty(currentUserId))
                {
                    return BadRequest();
                }

                var result = await _service.DeleteAsync(id, Guid.Parse(currentCompanyId), Guid.Parse(currentUserId));
                return result > 0 ? Ok(result) : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, nameof(OrderController), true, e);
                return BadRequest();
            }
        }
    }
}
